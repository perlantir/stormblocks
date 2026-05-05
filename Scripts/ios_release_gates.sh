#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
UNITY="${UNITY:-/Applications/Unity/Hub/Editor/6000.4.5f1/Unity.app/Contents/MacOS/Unity}"
BUNDLE_ID="${STORMBLOCKS_BUNDLE_ID:-com.perlantir.stormblocks}"
TEAM_ID="${STORMBLOCKS_TEAM_ID:-7JL22TDB44}"
DEVICE_ID="${STORMBLOCKS_DEVICE_ID:-907E2EE7-9C7B-5D0D-9EC0-32E69912287D}"

UNITY_PROJECT="$ROOT/StormBlocksUnity"
IOS_PROJECT="$UNITY_PROJECT/Builds/iOS/StormBlocks/Unity-iPhone.xcodeproj"
ARCHIVE_PATH="$UNITY_PROJECT/Builds/iOS/Archives/StormBlocks-Team7JL.xcarchive"
EXPORT_DIR="$UNITY_PROJECT/Builds/iOS/ExportAppStoreTeam7JL"
IPA_PATH="$EXPORT_DIR/StormBlocks.ipa"

die() {
  printf 'ERROR: %s\n' "$*" >&2
  exit 1
}

log() {
  printf '==> %s\n' "$*"
}

require_file() {
  [[ -e "$1" ]] || die "Missing required path: $1"
}

require_log_contains() {
  local file="$1"
  local pattern="$2"
  require_file "$file"
  grep -q "$pattern" "$file" || die "Expected '$pattern' in $file"
}

require_nunit_pass() {
  local file="$1"
  local total="$2"
  require_file "$file"
  grep -q "result=\"Passed\" total=\"$total\" passed=\"$total\" failed=\"0\"" "$file" \
    || die "Expected $total passing tests in $file"
}

make_export_options() {
  local destination="$1"
  local output="$2"
  rm -f "$output"
  plutil -create xml1 "$output"
  /usr/libexec/PlistBuddy \
    -c 'Add :method string app-store-connect' \
    -c "Add :teamID string $TEAM_ID" \
    -c 'Add :signingStyle string automatic' \
    -c "Add :destination string $destination" \
    -c 'Add :stripSwiftSymbols bool true' \
    -c 'Add :uploadSymbols bool true' \
    "$output" >/dev/null
}

status() {
  log "Checking cached release evidence"
  require_nunit_pass "$UNITY_PROJECT/editmode-results.xml" 25
  require_nunit_pass "$UNITY_PROJECT/playmode-results.xml" 7
  require_log_contains "$UNITY_PROJECT/playmode-results.xml" "Storm Blocks mobile budget renderers=432 triangles=156984 audioListeners=1 canvases=1"
  require_log_contains /tmp/stormblocks-ios-device-team7jl.log "Build Finished, Result: Success"
  require_log_contains /tmp/stormblocks-xcode-lowdetail-pool-unsigned.log "BUILD SUCCEEDED"
  require_log_contains /tmp/stormblocks-xcode-team7jl-default-signed.log "BUILD SUCCEEDED"
  require_log_contains /tmp/stormblocks-xcode-team7jl-archive.log "ARCHIVE SUCCEEDED"
  require_log_contains /tmp/stormblocks-xcode-team7jl-export-appstore.log "EXPORT SUCCEEDED"
  require_file "$IPA_PATH"
  unzip -p "$IPA_PATH" Payload/StormBlocks.app/embedded.mobileprovision > /tmp/stormblocks-ipa-embedded.mobileprovision
  security cms -D -i /tmp/stormblocks-ipa-embedded.mobileprovision 2>/dev/null \
    | plutil -extract Entitlements.application-identifier raw -o - - \
    | grep -q "^$TEAM_ID\\.$BUNDLE_ID$" \
    || die "IPA provisioning profile is not for $TEAM_ID.$BUNDLE_ID"
  log "Release evidence is present. IPA: $IPA_PATH"
}

test_unity() {
  log "Running EditMode tests"
  "$UNITY" -batchmode -nographics \
    -projectPath "$UNITY_PROJECT" \
    -executeMethod StormBlocks.Editor.StormBlocksTestRunner.RunEditMode \
    -stormBlocksTestResults "$UNITY_PROJECT/editmode-results.xml" \
    -logFile /tmp/stormblocks-editmode-lowdetail.log

  log "Running PlayMode tests"
  "$UNITY" -batchmode -nographics \
    -projectPath "$UNITY_PROJECT" \
    -executeMethod StormBlocks.Editor.StormBlocksTestRunner.RunPlayMode \
    -stormBlocksTestResults "$UNITY_PROJECT/playmode-results.xml" \
    -logFile /tmp/stormblocks-playmode-lowdetail.log
}

export_ios() {
  log "Exporting Unity iOS project"
  "$UNITY" -batchmode -nographics \
    -projectPath "$UNITY_PROJECT" \
    -executeMethod StormBlocks.Editor.StormBlocksBuildPipeline.BuildIOSDevelopment \
    -logFile /tmp/stormblocks-ios-device-team7jl.log \
    -quit
}

build_signed() {
  require_file "$IOS_PROJECT"
  log "Building signed Release-iphoneos app"
  xcodebuild \
    -project "$IOS_PROJECT" \
    -scheme Unity-iPhone \
    -configuration Release \
    -destination 'generic/platform=iOS' \
    -derivedDataPath "$UNITY_PROJECT/Builds/iOS/DerivedDataSignedTeam7JLDefault" \
    -allowProvisioningUpdates \
    build > /tmp/stormblocks-xcode-team7jl-default-signed.log 2>&1
}

install_device() {
  require_file "$UNITY_PROJECT/Builds/iOS/DerivedDataSignedTeam7JLDefault/Build/Products/Release-iphoneos/StormBlocks.app"
  log "Installing signed app on device $DEVICE_ID"
  xcrun devicectl device install app \
    --device "$DEVICE_ID" \
    --timeout 120 \
    --json-output /tmp/stormblocks-device-install.json \
    --log-output /tmp/stormblocks-device-install.log \
    "$UNITY_PROJECT/Builds/iOS/DerivedDataSignedTeam7JLDefault/Build/Products/Release-iphoneos/StormBlocks.app"
}

launch_device() {
  log "Launching $BUNDLE_ID on device $DEVICE_ID"
  xcrun devicectl device process launch \
    --device "$DEVICE_ID" \
    --terminate-existing \
    --activate \
    --timeout 60 \
    --json-output /tmp/stormblocks-device-launch.json \
    --log-output /tmp/stormblocks-device-launch.log \
    "$BUNDLE_ID"
}

archive_app() {
  require_file "$IOS_PROJECT"
  log "Creating Xcode archive"
  mkdir -p "$(dirname "$ARCHIVE_PATH")"
  xcodebuild archive \
    -project "$IOS_PROJECT" \
    -scheme Unity-iPhone \
    -configuration Release \
    -destination 'generic/platform=iOS' \
    -archivePath "$ARCHIVE_PATH" \
    -derivedDataPath "$UNITY_PROJECT/Builds/iOS/DerivedDataArchiveTeam7JL" \
    -allowProvisioningUpdates \
    > /tmp/stormblocks-xcode-team7jl-archive.log 2>&1
}

export_appstore() {
  require_file "$ARCHIVE_PATH"
  log "Exporting App Store Connect IPA"
  local options=/tmp/stormblocks-exportOptions-appstore.plist
  make_export_options export "$options"
  rm -rf "$EXPORT_DIR"
  xcodebuild -exportArchive \
    -archivePath "$ARCHIVE_PATH" \
    -exportPath "$EXPORT_DIR" \
    -exportOptionsPlist "$options" \
    -allowProvisioningUpdates \
    > /tmp/stormblocks-xcode-team7jl-export-appstore.log 2>&1
}

upload_probe() {
  require_file "$ARCHIVE_PATH"
  log "Uploading archive through Xcode App Store Connect flow"
  local options=/tmp/stormblocks-exportOptions-upload.plist
  make_export_options upload "$options"
  xcodebuild -exportArchive \
    -archivePath "$ARCHIVE_PATH" \
    -exportOptionsPlist "$options" \
    -allowProvisioningUpdates \
    > /tmp/stormblocks-xcode-team7jl-upload-appstore.log 2>&1
}

case "${1:-status}" in
  status)
    status
    ;;
  test)
    test_unity
    ;;
  export-ios)
    export_ios
    ;;
  build-signed)
    build_signed
    ;;
  install-device)
    install_device
    ;;
  launch-device)
    launch_device
    ;;
  archive)
    archive_app
    ;;
  export-appstore)
    export_appstore
    ;;
  upload-probe)
    upload_probe
    ;;
  all-local)
    test_unity
    export_ios
    build_signed
    install_device
    archive_app
    export_appstore
    status
    ;;
  *)
    cat <<USAGE
Usage: $0 [status|test|export-ios|build-signed|install-device|launch-device|archive|export-appstore|upload-probe|all-local]

Environment overrides:
  UNITY=$UNITY
  STORMBLOCKS_TEAM_ID=$TEAM_ID
  STORMBLOCKS_DEVICE_ID=$DEVICE_ID
  STORMBLOCKS_BUNDLE_ID=$BUNDLE_ID
USAGE
    exit 2
    ;;
esac
