# Build and Test

Unity editor used for this pass:

`/Applications/Unity/Hub/Editor/6000.4.5f1/Unity.app/Contents/MacOS/Unity`

## Bootstrap Project

```bash
'/Applications/Unity/Hub/Editor/6000.4.5f1/Unity.app/Contents/MacOS/Unity' \
  -quit -batchmode \
  -projectPath /Users/perlantir/Projects/StormBlocks/StormBlocksUnity \
  -executeMethod StormBlocks.Editor.StormBlocksProjectBootstrap.ConfigureProject \
  -logFile /tmp/stormblocks-bootstrap.log
```

## EditMode Tests

Use the project test runner for batch verification. Unity 6000 can skip `-runTests` if package compilation is still active during editor initialization; `StormBlocks.Editor.StormBlocksTestRunner` waits for compilation and writes NUnit XML before exiting.

```bash
'/Applications/Unity/Hub/Editor/6000.4.5f1/Unity.app/Contents/MacOS/Unity' \
  -batchmode -nographics \
  -projectPath /Users/perlantir/Projects/StormBlocks/StormBlocksUnity \
  -executeMethod StormBlocks.Editor.StormBlocksTestRunner.RunEditMode \
  -stormBlocksTestResults /Users/perlantir/Projects/StormBlocks/StormBlocksUnity/editmode-results.xml \
  -logFile /tmp/stormblocks-editmode-lowdetail.log
```

Current evidence: `StormBlocksUnity/editmode-results.xml` reports 26 total, 26 passed, 0 failed at `2026-05-05 17:52:40Z`.

## PlayMode Tests

```bash
'/Applications/Unity/Hub/Editor/6000.4.5f1/Unity.app/Contents/MacOS/Unity' \
  -batchmode -nographics \
  -projectPath /Users/perlantir/Projects/StormBlocks/StormBlocksUnity \
  -executeMethod StormBlocks.Editor.StormBlocksTestRunner.RunPlayMode \
  -stormBlocksTestResults /Users/perlantir/Projects/StormBlocks/StormBlocksUnity/playmode-results.xml \
  -logFile /tmp/stormblocks-playmode-lowdetail.log
```

Current evidence: `StormBlocksUnity/playmode-results.xml` reports 10 total, 10 passed, 0 failed at `2026-05-05 17:52:47Z`.

The PlayMode suite includes release smoke guards for the normal flow and active touch controls:

- `NormalFlowDoesNotEmitGameErrors` verifies the generated playable scene completes a normal interaction path without project-level console errors.
- `ActiveTouchControlsStayInsideSafeAreaWithReleaseSizedTargets` verifies active portrait controls stay inside the safe-area root and meet release-sized touch target thresholds.
- `FirstMoveCoachTeachesWithNoTextAndDismissesAfterPlacement` verifies the first-run coach is visual-only and disappears after the first placement.
- `AutomaticPushbackSpawnsSignaturePerimeterRecoil` verifies a real row clear through a storm tile triggers automatic Storm Pushback plus the saved-survivor toast, rescue burst, gold wave, storm shatter flare, and perimeter recoil VFX objects.
- `AccessibilityReducedMotionAndLowDetailTrimSecondaryPushbackFx` verifies Reduced Motion and Low Detail can be enabled from the runtime Accessibility screen, then confirms a real saved-pushback clear keeps essential saved/pushback feedback while trimming secondary waves, lightning, cyan recoil, and block highlight dots.

The PlayMode suite includes a lightweight mobile scene-budget guard. Current logged baseline:

- 448 renderers.
- 163,432 mesh triangles.
- 1 audio listener.
- 1 canvas.

Current optimization notes:

- Dynamic board, tray, ghost, survivor, block, storm, and pushback primitives are pooled instead of recreated every refresh.
- The Accessibility screen includes a persistent Low Detail setting. On physical iOS devices the runtime also auto-selects Low Detail on constrained hardware using memory, graphics memory, and CPU-count heuristics.
- Low Detail preserves the readable camp, board, placed blocks, survivor, storm, and signature pushback identity while trimming secondary storm puffs, rain, duplicate waves, shatter lightning, and block highlight dots. Reduced Motion plus Low Detail is covered in PlayMode to keep the saved toast, rescue burst, camp glow, storm shatter flare, and perimeter recoil while removing extra animated wave/accent objects.
- The current board visual pass imports optimized GLB source art through GLTFast, uses a curated toy-block charm in the runtime tray, loads a blurred design-source storm backdrop through `Resources/StormSky`, and builds the real playfield with a deep grid seam lattice, warm camp sanctuary ring, living perimeter storm wall, and pushback perimeter recoil. Heavier camp/storm GLB outputs remain import-ready source assets, not direct repeated board-cell meshes.

## Visual Capture

```bash
'/Applications/Unity/Hub/Editor/6000.4.5f1/Unity.app/Contents/MacOS/Unity' \
  -quit -batchmode \
  -projectPath /Users/perlantir/Projects/StormBlocks/StormBlocksUnity \
  -executeMethod StormBlocks.Editor.StormBlocksVisualCapture.CapturePortraitGameplay \
  -logFile /tmp/stormblocks-visual-capture.log
```

Output is written to ignored path `StormBlocksUnity/Builds/VisualChecks/stormblocks-gameplay.png`.

Current evidence: `/tmp/stormblocks-visual-saved-pushback.log` completed successfully. The capture stages the actual runtime saved-pushback moment through `TryPlaceForTest`, including the runtime HUD, deeper grid seams, warm camp sanctuary ring, perimeter storm wall, saved toast, rescue burst, and automatic Storm Pushback VFX.

## App Icon Draft

```bash
'/Applications/Unity/Hub/Editor/6000.4.5f1/Unity.app/Contents/MacOS/Unity' \
  -quit -batchmode \
  -projectPath /Users/perlantir/Projects/StormBlocks/StormBlocksUnity \
  -executeMethod StormBlocks.Editor.StormBlocksMarketingAssets.GenerateAppIcon \
  -logFile /tmp/stormblocks-app-icon.log
```

Tracked source asset:

- `StormBlocksUnity/Assets/StormBlocks/Art/Generated/AppIconDraft.png`

The bootstrap/build pipeline assigns this image into the generated iOS `AppIcon.appiconset` slots during Xcode export.

## App Store Screenshot Captures

```bash
'/Applications/Unity/Hub/Editor/6000.4.5f1/Unity.app/Contents/MacOS/Unity' \
  -quit -batchmode \
  -projectPath /Users/perlantir/Projects/StormBlocks/StormBlocksUnity \
  -executeMethod StormBlocks.Editor.StormBlocksVisualCapture.CaptureAppStoreScreenshots \
  -logFile /tmp/stormblocks-appstore-screens.log
```

Outputs are written to ignored path `StormBlocksUnity/Builds/AppStoreScreens/`:

- `01_place_blocks_save_camp.png`
- `02_beat_daily_storm.png`
- `03_storm_trail_progression.png`
- `04_tempest_trials_weekly.png`
- `05_cosmetic_profile.png`

Current evidence: `/tmp/stormblocks-appstore-saved-pushback.log` completed successfully and regenerated all five 1170 x 2532 PNGs. Screenshot `01_place_blocks_save_camp.png` is now staged from the actual saved-pushback gameplay moment; the refreshed Fastlane screenshot package passes `Scripts/verify_release_assets.sh` with 32 pass, 0 fail.

The current Fastlane handoff package includes tracked copies under `fastlane/screenshots/en-US/`. `Scripts/verify_release_assets.sh` validates the five expected PNGs at 1170 x 2532.

## iOS Xcode Export

iOS Build Support must be installed for Unity 6000.4.5f1:

```bash
'/Applications/Unity Hub.app/Contents/MacOS/Unity Hub' -- \
  --headless install-modules --version 6000.4.5f1 --module ios
```

Export:

```bash
'/Applications/Unity/Hub/Editor/6000.4.5f1/Unity.app/Contents/MacOS/Unity' \
  -quit -batchmode -buildTarget iOS \
  -projectPath /Users/perlantir/Projects/StormBlocks/StormBlocksUnity \
  -executeMethod StormBlocks.Editor.StormBlocksBuildPipeline.BuildIOSDevelopment \
  -logFile /tmp/stormblocks-ios-device-team7jl.log
```

Current evidence:

- `/tmp/stormblocks-ios-device-team7jl.log` reports `Build Finished, Result: Success`.
- Xcode project export exists at `StormBlocksUnity/Builds/iOS/StormBlocks/Unity-iPhone.xcodeproj`.
- Exported `Info.plist` is portrait only, requires full screen, requires arm64/Metal, and uses bundle display name `Storm Blocks`.
- Xcode project uses bundle id `com.perlantir.stormblocks`, automatic signing, team id `7JL22TDB44`, app icon catalog `AppIcon`, and iOS deployment target `15.0`.
- Xcode project includes the native GameKit bridge at `Libraries/Plugins/iOS/StormBlocksGameKitBridge.mm`.
- Xcode project includes the native share bridge at `Libraries/Plugins/iOS/StormBlocksShareBridge.mm`.
- Xcode project links `GameKit.framework` into the `UnityFramework` target.
- Xcode project includes `StormBlocks.entitlements` with `com.apple.developer.game-center = true` and `CODE_SIGN_ENTITLEMENTS = StormBlocks.entitlements`.
- The Unity build pre/post-processing sanitizer removed transient `PerformanceTestRunInfo.json` and `PerformanceTestRunSettings.json`; no `PerformanceTestRun` resources are present under the source `Assets` tree or exported iOS `Data` output after the final export.
- `xcodebuild -showdestinations` for this export shows iOS device destinations, including the paired physical iPhone and `Any iOS Device`.

Known note: Unity's performance test package can generate transient `Assets/Resources/PerformanceTestRunInfo.json` and `PerformanceTestRunSettings.json` during batch builds. The build pipeline now deletes those generated resources before player packing and after build completion.

## iOS Simulator Export and Run

The device export is the release path. A separate simulator export exists for non-provisioned local runtime checks:

```bash
'/Applications/Unity/Hub/Editor/6000.4.5f1/Unity.app/Contents/MacOS/Unity' \
  -quit -batchmode -buildTarget iOS \
  -projectPath /Users/perlantir/Projects/StormBlocks/StormBlocksUnity \
  -executeMethod StormBlocks.Editor.StormBlocksBuildPipeline.BuildIOSSimulatorDevelopment \
  -logFile /tmp/stormblocks-ios-simulator-lowdetail-pool.log
```

Current evidence:

- `/tmp/stormblocks-ios-simulator-lowdetail-pool.log` reports `Build Finished, Result: Success`.
- Simulator Xcode project exists at `StormBlocksUnity/Builds/iOSSimulator/StormBlocks/Unity-iPhone.xcodeproj`.
- XcodeBuildMCP built, installed, and launched `com.perlantir.stormblocks` on booted iPhone 16 Pro simulator `BFD7E422-B789-4380-9588-B952559B6A92` with `CODE_SIGNING_ALLOWED=NO`.
- Simulator app output exists at `StormBlocksUnity/Builds/iOSSimulator/DerivedDataSimLowDetailPool/Build/Products/Debug-iphonesimulator/StormBlocks.app`.
- Screenshot capture succeeded at `/var/folders/b2/cl2rv8q13bg48zl073ctm_fc0000gq/T/screenshot_optimized_a396cd73-dcef-4d70-a02e-2777232b925d.jpg`.
- The simulator export command restores Unity's iOS SDK setting back to device after export; the final device export above confirms device destinations are active again.

## Unsigned Xcode Device Build

This validates the generated Xcode project, native GameKit/share bridges, and iOS asset catalog without requiring provisioning credentials.

```bash
xcodebuild \
  -project StormBlocksUnity/Builds/iOS/StormBlocks/Unity-iPhone.xcodeproj \
  -scheme Unity-iPhone \
  -configuration Release \
  -destination 'generic/platform=iOS' \
  -derivedDataPath StormBlocksUnity/Builds/iOS/DerivedDataUnsignedLowDetailPool \
  CODE_SIGNING_ALLOWED=NO \
  build > /tmp/stormblocks-xcode-lowdetail-pool-unsigned.log 2>&1
```

Current evidence:

- `/tmp/stormblocks-xcode-lowdetail-pool-unsigned.log` reports `** BUILD SUCCEEDED **`.
- Output app exists at `StormBlocksUnity/Builds/iOS/DerivedDataUnsignedLowDetailPool/Build/Products/Release-iphoneos/StormBlocks.app`.
- Output framework exists at `StormBlocksUnity/Builds/iOS/DerivedDataUnsignedLowDetailPool/Build/Products/Release-iphoneos/UnityFramework.framework`.

Known note: the Xcode log contains Unity/IL2CPP toolchain warnings and a Unity `Run Script` build phase warning. There are no remaining Storm Blocks native GameKit/share bridge errors in the successful build.

## Signed Xcode Device Build

```bash
xcodebuild \
  -project StormBlocksUnity/Builds/iOS/StormBlocks/Unity-iPhone.xcodeproj \
  -scheme Unity-iPhone \
  -configuration Release \
  -destination 'generic/platform=iOS' \
  -derivedDataPath StormBlocksUnity/Builds/iOS/DerivedDataSignedTeam7JLDefault \
  -allowProvisioningUpdates \
  build > /tmp/stormblocks-xcode-team7jl-default-signed.log 2>&1
```

Current evidence:

- `/tmp/stormblocks-xcode-team7jl-default-signed.log` reports `** BUILD SUCCEEDED **`.
- Output app exists at `StormBlocksUnity/Builds/iOS/DerivedDataSignedTeam7JLDefault/Build/Products/Release-iphoneos/StormBlocks.app`.
- Codesign reports `TeamIdentifier=7JL22TDB44`.
- Signed entitlements include `application-identifier = 7JL22TDB44.com.perlantir.stormblocks`, `com.apple.developer.game-center = true`, and `get-task-allow = true`.
- Xcode created/used provisioning profile `iOS Team Provisioning Profile: com.perlantir.stormblocks`, UUID `5c66a56a-d8a2-414d-a247-5c02ab9a9a7d`, team `UBER KIWI LLC`, expiring `2027-05-05 05:41:43Z`.

Historical note: the earlier signed build failed under stale team id `84D222Q647`; the Unity export now uses the working team id `7JL22TDB44`.

## Physical Device Install

```bash
xcrun devicectl device install app \
  --device 907E2EE7-9C7B-5D0D-9EC0-32E69912287D \
  --timeout 120 \
  --json-output /tmp/stormblocks-device-install.json \
  --log-output /tmp/stormblocks-device-install.log \
  StormBlocksUnity/Builds/iOS/DerivedDataSignedTeam7JLDefault/Build/Products/Release-iphoneos/StormBlocks.app
```

Current evidence:

- Latest current-source install retry ran through `Scripts/ios_release_gates.sh install-device` at `2026-05-05 17:56:08Z`.
- `/tmp/stormblocks-device-install.json` reports `outcome = success` for paired iPhone `907E2EE7-9C7B-5D0D-9EC0-32E69912287D`.
- The installed app bundle id is `com.perlantir.stormblocks`; device install URL is `file:///private/var/containers/Bundle/Application/EEE68FEF-5040-4CEA-85E9-38A403C18E10/StormBlocks.app/`.

Launch probe:

```bash
xcrun devicectl device process launch \
  --device 907E2EE7-9C7B-5D0D-9EC0-32E69912287D \
  --terminate-existing \
  --activate \
  --timeout 60 \
  --json-output /tmp/stormblocks-device-launch.json \
  --log-output /tmp/stormblocks-device-launch.log \
  com.perlantir.stormblocks
```

Current evidence:

- Latest `Scripts/ios_release_gates.sh launch-device` retry for the current saved-pushback build ran at `2026-05-05 17:56:17Z`.
- `/tmp/stormblocks-device-launch.json` reports `outcome = success` for `com.perlantir.stormblocks` on paired iPhone `907E2EE7-9C7B-5D0D-9EC0-32E69912287D`.
- The launched executable path is `file:///private/var/containers/Bundle/Application/EEE68FEF-5040-4CEA-85E9-38A403C18E10/StormBlocks.app/StormBlocks`, process id `1485`.

## Xcode Archive and App Store IPA Export

Archive:

```bash
xcodebuild archive \
  -project StormBlocksUnity/Builds/iOS/StormBlocks/Unity-iPhone.xcodeproj \
  -scheme Unity-iPhone \
  -configuration Release \
  -destination 'generic/platform=iOS' \
  -archivePath StormBlocksUnity/Builds/iOS/Archives/StormBlocks-Team7JL.xcarchive \
  -derivedDataPath StormBlocksUnity/Builds/iOS/DerivedDataArchiveTeam7JL \
  -allowProvisioningUpdates \
  > /tmp/stormblocks-xcode-team7jl-archive.log 2>&1
```

IPA export:

```bash
xcodebuild -exportArchive \
  -archivePath StormBlocksUnity/Builds/iOS/Archives/StormBlocks-Team7JL.xcarchive \
  -exportPath StormBlocksUnity/Builds/iOS/ExportAppStoreTeam7JL \
  -exportOptionsPlist /tmp/stormblocks-exportOptions-appstore.plist \
  -allowProvisioningUpdates \
  > /tmp/stormblocks-xcode-team7jl-export-appstore.log 2>&1
```

Current evidence:

- `/tmp/stormblocks-xcode-team7jl-archive.log` reports `** ARCHIVE SUCCEEDED **`.
- Archive exists at `StormBlocksUnity/Builds/iOS/Archives/StormBlocks-Team7JL.xcarchive`.
- `/tmp/stormblocks-xcode-team7jl-export-appstore.log` reports `** EXPORT SUCCEEDED **`.
- Exported IPA exists at `StormBlocksUnity/Builds/iOS/ExportAppStoreTeam7JL/StormBlocks.ipa` and is 248 MB.
- Export summary reports `Cloud Managed Apple Distribution`, team `7JL22TDB44`, arm64, Game Center entitlement, `beta-reports-active = true`, `get-task-allow = false`, and store profile `iOS Team Store Provisioning Profile: com.perlantir.stormblocks`, UUID `013406e3-56cb-44d6-a480-81d7af0bac49`.

## App Store Connect Upload Probe

```bash
xcodebuild -exportArchive \
  -archivePath StormBlocksUnity/Builds/iOS/Archives/StormBlocks-Team7JL.xcarchive \
  -exportOptionsPlist /tmp/stormblocks-exportOptions-upload.plist \
  -allowProvisioningUpdates \
  > /tmp/stormblocks-xcode-team7jl-upload-appstore.log 2>&1
```

Current evidence:

- `/tmp/stormblocks-xcode-team7jl-upload-appstore.log` fails with `exportArchive Error Downloading App Information`.
- Distribution logs show Xcode authenticated to App Store Connect provider `ae62de71-9179-4836-a662-2c92a63e965e` and queried bundle id `com.perlantir.stormblocks`.
- App Store Connect returned `data: []` and `total: 0`, meaning no app record currently exists for `com.perlantir.stormblocks` under the selected provider/team.
- Latest Xcode retry at `2026-05-05 17:55:42Z` authenticated to App Store Connect, received HTTP 200 with `data: []` / `total: 0`, and failed while downloading app information because no app record exists for `com.perlantir.stormblocks`.
- Current distribution log: `/var/folders/b2/cl2rv8q13bg48zl073ctm_fc0000gq/T/Unity-iPhone_2026-05-05_12-55-40.043.xcdistributionlogs/IDEDistributionAppStoreConnect.log`.

The non-UI credential probes currently require human/App Store Connect credentials:

- `xcrun altool --list-providers` reports that either JWT API-key auth or username/app-password auth is required.
- `Scripts/fastlane_release.sh ios create_app_record` refuses to run until App Store Connect API key variables or `STORMBLOCKS_APPLE_ID`/`APPLE_ID` are set.
- `Scripts/fastlane_release.sh lanes` runs Fastlane through Bundler using Homebrew Ruby, avoiding the system Ruby 2.6 `/usr/bin/bundle` mismatch with `Gemfile.lock`.
- `Scripts/verify_release_assets.sh` validates App Store metadata limits, tracked screenshot dimensions, and Game Center identifier alignment between `Docs/APP_STORE_CONNECT_MANIFEST.json`, `Docs/GAME_CENTER_SETUP.md`, and `UnityGameCenterServices.cs`.

Public metadata URL evidence:

- `curl -L -I https://github.com/perlantir/stormblocks/blob/main/Docs/PUBLIC_SUPPORT.md` returned HTTP 200 on `2026-05-05`.
- `curl -L -I https://github.com/perlantir/stormblocks/blob/main/Docs/PUBLIC_PRIVACY.md` returned HTTP 200 on `2026-05-05`.
- `curl -L -I https://github.com/perlantir/stormblocks` returned HTTP 200 on `2026-05-05`.

## Credentialed Follow-Up

- Create the App Store Connect app record for bundle id `com.perlantir.stormblocks` under team `7JL22TDB44` / `UBER KIWI LLC`.
- App Store Connect API key or Apple ID app-specific password for upload automation.
- Game Center leaderboard and achievement ids matching `Docs/GAME_CENTER_SETUP.md`.
- Upload `StormBlocksUnity/Builds/iOS/ExportAppStoreTeam7JL/StormBlocks.ipa` after the App Store Connect app record exists.

The repeatable command runner is `Scripts/ios_release_gates.sh`.
Credentialed Fastlane lanes should be run through `Scripts/fastlane_release.sh`.

## GitHub Static Checks

The repo includes a lightweight GitHub Actions workflow for credential-free release checks:

- Workflow: `.github/workflows/release-static.yml`
- Script: `Scripts/ci_static_checks.sh`

The workflow runs on macOS so screenshot dimension checks can use `sips`. It validates shell syntax, App Store metadata limits, tracked screenshot dimensions, App Store Connect manifest JSON, Game Center identifier alignment, support/privacy drafts, and absence of transient Unity/Fastlane artifacts. It does not run Unity builds or Apple credentialed gates.

`Scripts/ci_static_checks.sh` also runs `Scripts/verify_prompt_compliance.sh`, which checks the explicit launch prompt surfaces: required source docs, design reference files, core mode/service/presentation files, mode identifiers, automatic Storm Pushback symbols, Storm Trail/Tempest definitions, service interfaces, accessibility settings, tests, and monetization/IP exclusions.
