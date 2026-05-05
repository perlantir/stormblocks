#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
MODE="${1:-full}"

UNITY_PROJECT="$ROOT/StormBlocksUnity"
IPA_PATH="$UNITY_PROJECT/Builds/iOS/ExportAppStoreTeam7JL/StormBlocks.ipa"
ARCHIVE_PATH="$UNITY_PROJECT/Builds/iOS/Archives/StormBlocks-Team7JL.xcarchive"
BUNDLE_ID="com.perlantir.stormblocks"
TEAM_ID="7JL22TDB44"

pass_count=0
fail_count=0
open_count=0

pass() {
  pass_count=$((pass_count + 1))
  printf 'PASS: %s\n' "$*"
}

fail() {
  fail_count=$((fail_count + 1))
  printf 'FAIL: %s\n' "$*" >&2
}

open_gate() {
  open_count=$((open_count + 1))
  printf 'OPEN: %s\n' "$*"
}

require_file() {
  local path="$1"
  local label="$2"
  if [[ -e "$path" ]]; then
    pass "$label"
  else
    fail "$label missing at $path"
  fi
}

require_grep() {
  local file="$1"
  local pattern="$2"
  local label="$3"
  if [[ -f "$file" ]] && grep -q "$pattern" "$file"; then
    pass "$label"
  else
    fail "$label"
  fi
}

require_absent_glob_find() {
  local label="$1"
  shift
  local found
  found="$(find "$@" 2>/dev/null || true)"
  if [[ -z "$found" ]]; then
    pass "$label"
  else
    fail "$label found unexpected artifacts: $found"
  fi
}

require_fastlane_wrapper() {
  local output
  if output="$(FASTLANE_SKIP_UPDATE_CHECK=1 "$ROOT/Scripts/fastlane_release.sh" lanes 2>&1)" \
      && grep -q "ios validate_release_assets" <<<"$output" \
      && grep -q "ios create_app_record" <<<"$output" \
      && grep -q "ios upload_metadata" <<<"$output" \
      && grep -q "ios upload_testflight" <<<"$output" \
      && grep -q "ios release_candidate_upload" <<<"$output"; then
    pass "Fastlane wrapper runs lanes through Bundler"
  else
    fail "Fastlane wrapper cannot list release lanes"
  fi
}

require_nunit_pass() {
  local file="$1"
  local total="$2"
  local label="$3"
  if [[ -f "$file" ]] && grep -q "result=\"Passed\" total=\"$total\" passed=\"$total\" failed=\"0\"" "$file"; then
    pass "$label"
  else
    fail "$label"
  fi
}

require_ipa_entitlement() {
  local profile=/tmp/stormblocks-audit-embedded.mobileprovision
  if [[ ! -f "$IPA_PATH" ]]; then
    fail "IPA exists before entitlement audit"
    return
  fi

  unzip -p "$IPA_PATH" Payload/StormBlocks.app/embedded.mobileprovision > "$profile"
  if security cms -D -i "$profile" 2>/dev/null \
      | plutil -extract Entitlements.application-identifier raw -o - - \
      | grep -q "^$TEAM_ID\\.$BUNDLE_ID$"; then
    pass "IPA application identifier is $TEAM_ID.$BUNDLE_ID"
  else
    fail "IPA application identifier is not $TEAM_ID.$BUNDLE_ID"
  fi

  if security cms -D -i "$profile" 2>/dev/null \
      | plutil -p - \
      | grep -q '"com.apple.developer.game-center" => true'; then
    pass "IPA Game Center entitlement is present"
  else
    fail "IPA Game Center entitlement missing"
  fi
}

audit_local_evidence() {
  require_file "$ROOT/Docs/RELEASE_AUDIT.md" "Release audit document exists"
  require_file "$ROOT/Docs/RELEASE_DONE_CHECKLIST.md" "Release checklist exists"
  require_file "$ROOT/Docs/QA_EVAL_REPORT.md" "QA eval report exists"
  require_file "$ROOT/Docs/BUILD_AND_TEST.md" "Build and test evidence document exists"

  require_nunit_pass "$UNITY_PROJECT/editmode-results.xml" 25 "EditMode tests are 25/25 passing"
  require_nunit_pass "$UNITY_PROJECT/playmode-results.xml" 7 "PlayMode tests are 7/7 passing"
  require_grep "$UNITY_PROJECT/playmode-results.xml" "Storm Blocks mobile budget renderers=423 triangles=154152 audioListeners=1 canvases=1" "PlayMode mobile budget baseline is current"

  require_grep /tmp/stormblocks-ios-device-team7jl.log "Build Finished, Result: Success" "Unity iOS device export succeeded"
  require_grep /tmp/stormblocks-xcode-lowdetail-pool-unsigned.log "BUILD SUCCEEDED" "Unsigned Xcode device build succeeded"
  require_grep /tmp/stormblocks-xcode-team7jl-default-signed.log "BUILD SUCCEEDED" "Signed Xcode device build succeeded"
  require_grep /tmp/stormblocks-device-install.json "\"outcome\" : \"success\"" "Signed app installed on paired iPhone"
  require_grep /tmp/stormblocks-xcode-team7jl-archive.log "ARCHIVE SUCCEEDED" "Xcode archive succeeded"
  require_grep /tmp/stormblocks-xcode-team7jl-export-appstore.log "EXPORT SUCCEEDED" "App Store IPA export succeeded"
  require_file "$ARCHIVE_PATH/Info.plist" "Xcode archive exists"
  require_file "$IPA_PATH" "App Store IPA exists"
  require_ipa_entitlement

  require_grep "$ROOT/fastlane/Fastfile" "lane :create_app_record" "Fastlane app-record lane exists"
  require_grep "$ROOT/fastlane/Fastfile" "lane :upload_metadata" "Fastlane metadata lane exists"
  require_grep "$ROOT/fastlane/Fastfile" "lane :upload_testflight" "Fastlane TestFlight lane exists"
  require_file "$ROOT/Scripts/fastlane_release.sh" "Fastlane release wrapper exists"
  require_fastlane_wrapper
  require_file "$ROOT/Scripts/verify_release_assets.sh" "Release asset verifier exists"
  if "$ROOT/Scripts/verify_release_assets.sh" >/tmp/stormblocks-release-assets-audit.log 2>&1; then
    pass "Release asset verifier passes"
  else
    fail "Release asset verifier failed; see /tmp/stormblocks-release-assets-audit.log"
  fi
  require_file "$ROOT/Scripts/verify_prompt_compliance.sh" "Prompt compliance verifier exists"
  if "$ROOT/Scripts/verify_prompt_compliance.sh" >/tmp/stormblocks-prompt-compliance-audit.log 2>&1; then
    pass "Prompt compliance verifier passes"
  else
    fail "Prompt compliance verifier failed; see /tmp/stormblocks-prompt-compliance-audit.log"
  fi
  require_file "$ROOT/Docs/PHYSICAL_QA_RUNBOOK.md" "Physical QA runbook exists"
  require_file "$ROOT/Scripts/device_qa_session.sh" "Physical QA session helper exists"
  require_file "$ROOT/Scripts/ci_static_checks.sh" "Static CI script exists"
  require_file "$ROOT/.github/workflows/release-static.yml" "GitHub static release workflow exists"
  require_grep "$ROOT/Scripts/ios_release_gates.sh" "upload-probe" "iOS release gate runner includes upload probe"

  require_absent_glob_find "No transient Unity/Fastlane artifacts remain" \
    "$UNITY_PROJECT/Assets" "$UNITY_PROJECT/Builds/iOS/StormBlocks" "$UNITY_PROJECT/Builds/iOSSimulator/StormBlocks" "$ROOT/fastlane" \
    \( -name 'PerformanceTestRun*.json' -o -name 'PerformanceTestRun*.xml' -o -name 'InitTestScene*.unity' -o -name 'mono_crash*.json' -o -name 'report.xml' \) -print
}

audit_open_gates() {
  if grep -q "FBSOpenApplicationErrorDomain.*Locked\\|\"string\" : \"Locked\"" /tmp/stormblocks-device-launch.json 2>/dev/null; then
    open_gate "Physical-device launch is still blocked because the paired iPhone is locked."
  else
    require_grep /tmp/stormblocks-device-launch.json "\"outcome\" : \"success\"" "Physical-device launch succeeded"
  fi

  if grep -q "Error Downloading App Information" /tmp/stormblocks-xcode-team7jl-upload-appstore.log 2>/dev/null; then
    open_gate "TestFlight upload is blocked because App Store Connect has no app record for $BUNDLE_ID."
  else
    require_grep /tmp/stormblocks-xcode-team7jl-upload-appstore.log "EXPORT SUCCEEDED\\|No errors uploading" "App Store Connect upload completed"
  fi

  if grep -q "\\[ \\].*App Store Connect app record exists" "$ROOT/Docs/RELEASE_DONE_CHECKLIST.md"; then
    open_gate "App Store Connect app record checkbox remains open."
  fi
  if grep -q "\\[ \\].*Game Center leaderboard/achievement identifiers" "$ROOT/Docs/RELEASE_DONE_CHECKLIST.md"; then
    open_gate "Live Game Center leaderboard/achievement validation remains open."
  fi
  if grep -q "\\[ \\].*TestFlight upload and install" "$ROOT/Docs/RELEASE_DONE_CHECKLIST.md"; then
    open_gate "TestFlight upload/install checkbox remains open."
  fi
  if grep -q "\\[ \\].*Physical-device QA pass" "$ROOT/Docs/RELEASE_DONE_CHECKLIST.md"; then
    open_gate "Physical-device QA checkbox remains open."
  fi
  if grep -q "\\[ \\].*Physical-device performance pass" "$ROOT/Docs/RELEASE_DONE_CHECKLIST.md"; then
    open_gate "Physical-device performance checkbox remains open."
  fi
}

case "$MODE" in
  local)
    audit_local_evidence
    ;;
  full)
    audit_local_evidence
    audit_open_gates
    ;;
  *)
    printf 'Usage: %s [local|full]\n' "$0" >&2
    exit 2
    ;;
esac

printf '\nRelease audit summary: %d pass, %d fail, %d open\n' "$pass_count" "$fail_count" "$open_count"

if [[ "$fail_count" -gt 0 ]]; then
  exit 1
fi

if [[ "$MODE" == "full" && "$open_count" -gt 0 ]]; then
  exit 3
fi
