# Implementation Log

Codex should update this file after meaningful work.

## 2026-05-05 — Initial package

- Full-game Codex `/goal` package created.
- Design references included.
- Production target: launch-quality iPhone game, not MVP.

## 2026-05-05 — Unity foundation, deterministic core, playable shell

- Reviewed all repo docs, design prompts, design references, root data files, and asset briefs before implementation.
- Installed Unity 6000.4.5f1 and then explicitly installed iOS Build Support with Unity Hub `install-modules` after the first iOS export showed the module was not actually present.
- Created `StormBlocksUnity` Unity project with URP, portrait iPhone settings, assembly definitions, ignored generated Unity state, generated main scene, and iOS development export command.
- Implemented plain C# deterministic core gameplay: 8x8 board, pieces, placement validation, line clears, scoring, survivor rescue, deterministic storm warning/spread, automatic Storm Pushback, clutch save, run state, queue refill, game over, and snapshot encoding.
- Implemented mode/progression data: Endless Storm, deterministic Daily Storm, Practice/Chill config, 12-region/120-level Storm Trail catalog, 5-run weekly Tempest Trials, star goals, cosmetic-only catalog, profile progression, daily streak/one-official-score handling, mock service layer, and local file save service.
- Implemented a playable portrait Unity runtime shell with 3D board, bottom piece tray, drag placement, HUD, warm camp, storm visuals, survivor markers, mode buttons, automatic gold pushback VFX, audio/haptic service hooks, profile save hooks, and local run snapshot save.
- Added an iOS build pipeline that exports an Xcode project at `StormBlocksUnity/Builds/iOS/StormBlocks` with bundle id `com.perlantir.stormblocks`, IL2CPP, portrait orientation, automatic signing placeholders, and local Apple Development team id `84D222Q647`.
- Added a visual capture command that renders the current portrait 3D gameplay composition to `StormBlocksUnity/Builds/VisualChecks/stormblocks-gameplay.png`; used it to correct the initial over-warm palette and clipped-board framing.

Evidence:

- Unity project bootstrap: `Unity -quit -batchmode -projectPath StormBlocksUnity -executeMethod StormBlocks.Editor.StormBlocksProjectBootstrap.ConfigureProject` completed with exit code 0 and no C# warnings/errors in `/tmp/stormblocks-playable-bootstrap.log`.
- Visual capture: `StormBlocks.Editor.StormBlocksVisualCapture.CapturePortraitGameplay` completed with exit code 0 and no C# warnings/errors; capture stored under ignored `StormBlocksUnity/Builds/VisualChecks/`.
- EditMode tests: `StormBlocksUnity/editmode-results.xml` reports 21 total, 21 passed, 0 failed at `2026-05-05 03:08:49Z`.
- PlayMode tests: `StormBlocksUnity/playmode-results.xml` reports 2 total, 2 passed, 0 failed at `2026-05-05 03:09:06Z`.
- iOS Xcode export: `/tmp/stormblocks-ios-build.log` reports `Build Finished, Result: Success`; exported `StormBlocksUnity/Builds/iOS/StormBlocks/Unity-iPhone.xcodeproj`.

Known risks / not done:

- The game is now playable and buildable, but it is not release-candidate complete.
- Storm Trail currently has deterministic generated level definitions and first-level runtime entry, not a full map UI.
- Tempest Trials currently starts the first weekly run from a deterministic 5-run playlist, not a full weekly results flow.
- Game Center, achievements, analytics, cloud save, haptics, audio, and sharing are interface-backed/mock-backed; App Store Connect/Game Center setup and real platform implementations still need credentialed integration.
- Visuals are cohesive generated/procedural placeholders matching the design direction, but not final custom production art/audio.
- App Store upload/TestFlight remains blocked on App Store Connect credentials/API key and app record setup.

## 2026-05-05 — Launch shell, settings, screenshots, icon, iOS export hardening

- Expanded the Unity shell from a playable loop into a fuller game surface: launch menu, Endless Storm, Daily Storm, Storm Trail level picker, Tempest Trials run list, Practice/Chill mode, profile, achievements, cosmetics, settings, results, retry, and share flow.
- Added profile persistence for accessibility/settings, daily history, Tempest weekly history, cosmetic unlock/equip state, best score, rescue totals, and Storm Trail stars/rewards.
- Added reduced motion, high contrast, colorblind-friendly palette, left-handed tray layout, large text, music/effects/haptics toggles, and master volume settings.
- Added procedural local audio and haptic services for gameplay feedback with generated clips and mobile-only vibration guards.
- Generated a cohesive 1024 x 1024 app icon draft at `StormBlocksUnity/Assets/StormBlocks/Art/Generated/AppIconDraft.png` and wired the Unity iOS export to populate the Xcode `AppIcon.appiconset`.
- Added App Store screenshot generation for five portrait scenes under `StormBlocksUnity/Builds/AppStoreScreens/`.
- Hardened iOS export cleanup and validated key Xcode settings: portrait orientation, full screen, arm64/Metal requirement, bundle id `com.perlantir.stormblocks`, app icon catalog `AppIcon`, iOS deployment target `15.0`, automatic signing, and local development team id `84D222Q647`.
- Added documentation for generated screenshots and refreshed build/test evidence.

Evidence:

- EditMode tests: `StormBlocksUnity/editmode-results.xml` reports 23 total, 23 passed, 0 failed at `2026-05-05 03:36:31Z`.
- PlayMode tests: `StormBlocksUnity/playmode-results.xml` reports 3 total, 3 passed, 0 failed at `2026-05-05 03:36:45Z`.
- iOS Xcode export: `/tmp/stormblocks-ios-finalpass.log` reports `Build Finished, Result: Success`; exported project is at `StormBlocksUnity/Builds/iOS/StormBlocks/Unity-iPhone.xcodeproj`.
- Generated store screenshots exist at `StormBlocksUnity/Builds/AppStoreScreens/` with five 1170 x 2532 PNGs.
- Generated app icon source exists at `StormBlocksUnity/Assets/StormBlocks/Art/Generated/AppIconDraft.png`.

Known risks / not done:

- The current build has broad mode coverage and a release-candidate shaped shell, but the release checklist is not fully satisfied.
- Game Center, cloud save, analytics, sharing, App Store Connect upload, and final provisioning still need credentialed production integration.
- Art/audio are cohesive generated assets and procedural hooks, not final hand-authored production assets.
- Performance profiling and physical-device QA are still required before TestFlight/release signoff.
- Unity may transiently generate `Assets/Resources/PerformanceTestRunInfo.json` and `PerformanceTestRunSettings.json` during batch builds; these remain ignored and should not be committed.

## 2026-05-05 — Native GameKit bridge and Xcode build validation

- Added a Game Center-ready runtime adapter that submits leaderboards and achievements through a local mock fallback in editor/tests and a native iOS GameKit bridge on device.
- Added a native iOS `StormBlocksGameKitBridge.mm` for Game Center authentication, leaderboard score submission, achievement progress, and Game Center UI presentation.
- Added Unity iOS post-processing to link `GameKit.framework` into the generated `UnityFramework` target.
- Added profile/achievements screen entry points for Game Center leaderboards and achievements.
- Expanded PlayMode smoke coverage so profile Game Center leaderboard access and achievement Game Center access stay present.
- Added `Docs/GAME_CENTER_SETUP.md` with the exact App Store Connect leaderboard and achievement identifiers.

Evidence:

- EditMode tests: `StormBlocksUnity/editmode-results.xml` reports 23 total, 23 passed, 0 failed at `2026-05-05 03:47:20Z`.
- PlayMode tests: `StormBlocksUnity/playmode-results.xml` reports 3 total, 3 passed, 0 failed at `2026-05-05 03:47:36Z`.
- Unity iOS export: `/tmp/stormblocks-ios-gamekit-framework.log` reports `Build Finished, Result: Success`.
- Generated Xcode project includes `Libraries/Plugins/iOS/StormBlocksGameKitBridge.mm` and links `GameKit.framework` in `UnityFramework`.
- Unsigned Xcode device build: `/tmp/stormblocks-xcode-gamekit-framework.log` reports `** BUILD SUCCEEDED **`; output app exists at `StormBlocksUnity/Builds/iOS/DerivedData/Build/Products/Release-iphoneos/StormBlocks.app`.

Known risks / not done:

- Signed physical-device Game Center authentication and submission still require App Store Connect app/Game Center configuration and provisioning.
- Xcode logs include Unity/IL2CPP toolchain warnings and Unity's generated Run Script warning; the Storm Blocks native GameKit bridge no longer has linker errors or deprecated `viewState` warnings in the successful build.

## 2026-05-05 — Native iOS share sheet and share-card pass

- Replaced the results-screen mock share action with `UnityShareService`, keeping an editor/non-iOS clipboard fallback.
- Added runtime generation of a 1200 x 630 Storm Blocks share card PNG from the run summary.
- Added native iOS `StormBlocksShareBridge.mm` that presents `UIActivityViewController` with share text and the generated PNG.
- Updated the playable scene wiring so results sharing uses the production share service.
- Expanded PlayMode smoke coverage to confirm the share service is present in the playable scene.
- Added `Docs/SHARING_SETUP.md` with behavior and physical-device follow-up.

Evidence:

- EditMode tests: `StormBlocksUnity/editmode-results.xml` reports 23 total, 23 passed, 0 failed at `2026-05-05 03:57:27Z`.
- PlayMode tests: `StormBlocksUnity/playmode-results.xml` reports 3 total, 3 passed, 0 failed at `2026-05-05 03:57:40Z`.
- Unity iOS export: `/tmp/stormblocks-ios-share.log` reports `Build Finished, Result: Success` and includes `Libraries/Plugins/iOS/StormBlocksShareBridge.mm`.
- Unsigned Xcode device build: `/tmp/stormblocks-xcode-share.log` reports `** BUILD SUCCEEDED **`; output app exists at `StormBlocksUnity/Builds/iOS/DerivedData/Build/Products/Release-iphoneos/StormBlocks.app`.

Known risks / not done:

- The native share sheet still needs a signed physical-device check to verify presentation and payload behavior outside the editor.
- Xcode logs still include Unity/IL2CPP toolchain warnings and Unity's generated Run Script warning.

## 2026-05-05 — Mobile scene-budget guardrail

- Added a PlayMode mobile scene-budget test for the active playable root.
- The test now guards renderer count, mesh triangle count, duplicate audio listeners, and duplicate canvases.
- Added `Docs/PERFORMANCE_PROFILE.md` with the current baseline and physical-device profiling follow-up.

Evidence:

- PlayMode tests: `StormBlocksUnity/playmode-results.xml` reports 4 total, 4 passed, 0 failed at `2026-05-05 04:01:42Z`.
- Current logged mobile baseline: 201 renderers, 40,212 mesh triangles, 1 audio listener, 1 canvas.

Known risks / not done:

- This is a structural scene-budget guard, not a real FPS/memory/thermal profile.
- Physical-device profiling remains required before TestFlight/release signoff.

## 2026-05-05 — Privacy and permissions review

- Reviewed the current package manifest, Unity services settings, exported iOS `Info.plist`, and native iOS integrations.
- Added `Docs/PRIVACY_REVIEW.md` with current App Store privacy posture and required re-review triggers.
- Updated the release checklist privacy/permissions item based on the current no-ads/no-IAP/no-tracking build state.

Evidence:

- `StormBlocksUnity/Packages/manifest.json` contains no ad SDKs, IAP package, attribution SDKs, Firebase, Facebook, AppLovin, AdMob, Adjust, AppsFlyer, or external analytics SDKs.
- `StormBlocksUnity/ProjectSettings/UnityConnectSettings.asset` has Unity Analytics, Unity Ads, Unity Purchasing, Cloud Diagnostics, and Performance Reporting disabled.
- Exported `Info.plist` does not declare camera, microphone, location, contacts, photo library, Bluetooth, tracking, or notification usage strings.

Known risks / not done:

- App Store privacy answers must be re-reviewed after any production analytics, crash reporting, cloud save, remote config, or service SDK integration.

## 2026-05-05 — Portrait safe-area pass

- Added a dedicated `Storm Blocks Safe Area` root for runtime UI and overlay content.
- Routed HUD, mode overlay, results screen, profile/settings/cosmetics panels, and progression screens through the safe-area root.
- Kept the 3D board, tray, camp, storm, survivor, and VFX world-space presentation outside the UI safe-area transform so gameplay framing remains stable.
- Refreshed release evidence and performance documentation after the safe-area pass.

Evidence:

- PlayMode tests: `StormBlocksUnity/playmode-results.xml` reports 4 total, 4 passed, 0 failed at `2026-05-05 04:04:21Z`.
- Current logged mobile baseline: 199 renderers, 39,432 mesh triangles, 1 audio listener, 1 canvas.
- Unity iOS export: `/tmp/stormblocks-ios-safearea.log` reports `Build Finished, Result: Success` and includes the native GameKit/share bridges.
- Unsigned Xcode device build: `/tmp/stormblocks-xcode-safearea.log` reports `** BUILD SUCCEEDED **`; output app exists at `StormBlocksUnity/Builds/iOS/DerivedData/Build/Products/Release-iphoneos/StormBlocks.app`.

Known risks / not done:

- Safe-area behavior is structurally covered in the Unity presentation shell, but final touch ergonomics still need a signed physical-device pass on notched and Dynamic Island iPhones.
- Xcode logs still include Unity-generated deprecation/toolchain warnings and a Unity `Run Script` build phase warning.

## 2026-05-05 — Game-over and retry coverage

- Added deterministic EditMode coverage for both losing conditions: storm reaches the central camp and no queued piece can be placed.
- Added PlayMode smoke coverage that forces a real game-over path, verifies the results/retry overlay appears, clicks Retry, and confirms a fresh non-game-over run starts.
- Refreshed release and performance evidence after the new tests.

Evidence:

- EditMode tests: `StormBlocksUnity/editmode-results.xml` reports 25 total, 25 passed, 0 failed at `2026-05-05 04:09:37Z`.
- PlayMode tests: `StormBlocksUnity/playmode-results.xml` reports 5 total, 5 passed, 0 failed at `2026-05-05 04:11:33Z`.
- Current logged mobile baseline: 197 renderers, 38,652 mesh triangles, 1 audio listener, 1 canvas.

Known risks / not done:

- Retry and game-over are covered in editor PlayMode. Final tap ergonomics, animation feel, and retention quality still need physical-device QA.

## 2026-05-05 — Visual polish and screenshot text pass

- Reworked the runtime UI labels from TextMeshPro batch-capture defaults to Unity `Text` with a built-in font fallback so HUD, menus, and generated screenshots render visible labels in automated captures.
- Reduced always-on gameplay HUD clutter by moving mode navigation into the menu and keeping only the primary menu control over gameplay.
- Expanded the procedural 3D presentation with a cyan storm barrier, softer toy-like board colors, storm cloud/lightning/rain accents, camp glow/string lights, survivor boot details, tray glow pads, chunkier block highlights, and stronger gold/cyan Storm Pushback VFX.
- Regenerated the five App Store screenshot scenes after the visual/UI pass.
- Refreshed iOS export and unsigned Xcode build evidence after the presentation changes.

Evidence:

- Visual capture: `/tmp/stormblocks-visual-declutter.log` completed with no C# errors or Unity exceptions; capture output is `StormBlocksUnity/Builds/VisualChecks/stormblocks-gameplay.png`.
- App Store screenshots: `/tmp/stormblocks-appstore-polish.log` completed with no C# errors or Unity exceptions and regenerated five 1170 x 2532 PNGs under `StormBlocksUnity/Builds/AppStoreScreens/`.
- EditMode tests: `StormBlocksUnity/editmode-results.xml` reports 25 total, 25 passed, 0 failed at `2026-05-05 04:21:43Z`.
- PlayMode tests: `StormBlocksUnity/playmode-results.xml` reports 5 total, 5 passed, 0 failed at `2026-05-05 04:22:05Z`.
- Current logged mobile baseline: 280 renderers, 60,816 mesh triangles, 1 audio listener, 1 canvas.
- Unity iOS export: `/tmp/stormblocks-ios-polish.log` reports `Build Finished, Result: Success`.
- Unsigned Xcode device build: `/tmp/stormblocks-xcode-polish.log` reports `** BUILD SUCCEEDED **`; output app exists at `StormBlocksUnity/Builds/iOS/DerivedData/Build/Products/Release-iphoneos/StormBlocks.app`.

Known risks / not done:

- The build now has a more cohesive stylized procedural art pass, but it still needs a real-device visual/touch review before calling the presentation final.
- Physical-device profiling remains required because the polish pass increased the scene-budget baseline while staying under current automated guardrails.

## 2026-05-05 — Score-feedback and near-death presentation pass

- Added move-level score feedback to the gameplay toast system so clears, combos, Storm Pushback, clutch saves, and perfect-set bonuses show the reason and point value.
- Added near-death world presentation with a warm warning frame, camp pulse, and lightning accents that stay outside or above playable cells for readability.
- Re-ran EditMode, PlayMode, Unity iOS export, and unsigned Xcode device build after the scoring/near-death changes.

Evidence:

- EditMode tests: `StormBlocksUnity/editmode-results.xml` reports 25 total, 25 passed, 0 failed at `2026-05-05 04:26:53Z`.
- PlayMode tests: `StormBlocksUnity/playmode-results.xml` reports 5 total, 5 passed, 0 failed at `2026-05-05 04:26:26Z`.
- Current logged mobile baseline: 280 renderers, 60,816 mesh triangles, 1 audio listener, 1 canvas.
- Unity iOS export: `/tmp/stormblocks-ios-neardeath-score-retry.log` reports `Build Finished, Result: Success`. The immediately prior export attempt hit a native Unity Burst compiler crash, then the same export command succeeded on rerun.
- Unsigned Xcode device build: `/tmp/stormblocks-xcode-neardeath-score.log` reports `** BUILD SUCCEEDED **`; output app exists at `StormBlocksUnity/Builds/iOS/DerivedData/Build/Products/Release-iphoneos/StormBlocks.app`.

Known risks / not done:

- The near-death and score-feedback pass is locally validated, but final feel/readability still needs physical-device play on real iPhones.
- Xcode logs still include Unity-generated deprecation/toolchain warnings and a Unity `Run Script` build phase warning.

## 2026-05-05 — Release QA gates, Game Center entitlement, and Xcode signing isolation

- Added a Unity Test Runner API wrapper at `StormBlocks.Editor.StormBlocksTestRunner` so EditMode and PlayMode batch verification waits for compilation, writes NUnit XML, and exits with a reliable process status.
- Expanded PlayMode release smoke coverage for normal-flow console cleanliness and active portrait touch controls staying inside the safe-area root with release-sized targets.
- Removed the direct TextMeshPro dependency from the Storm Blocks runtime path and package manifest; the bootstrap UI now uses Unity `Text` with a font fallback so automated captures and builds do not depend on TMP import state.
- Added an iOS build sanitizer that deletes transient `PerformanceTestRunInfo.json` and `PerformanceTestRunSettings.json` before player packing and after export cleanup.
- Added generated Game Center entitlements to the Xcode export: `StormBlocks.entitlements` includes `com.apple.developer.game-center = true`, and the Xcode project sets `CODE_SIGN_ENTITLEMENTS = StormBlocks.entitlements`.
- Updated ignore coverage and removed local Unity-generated `InitTestScene*.unity` and `mono_crash*.json` artifacts from the working tree.
- Expanded service seam coverage so the local mock now exercises remote config, cloud save, analytics, audio, haptics, and sharing without credentials.
- Added dedicated Accessibility and Credits screens to the production UI shell, with PlayMode navigation coverage.
- Added a separate Unity iOS Simulator export command for non-provisioned runtime checks, and made it restore the Unity iOS SDK setting back to device after export.
- Added `Docs/QA_EVAL_REPORT.md` to separate automated/local proof from the required human five-run, physical-device, Game Center, and TestFlight gates.
- Re-ran EditMode, PlayMode, Unity iOS export, unsigned Xcode device build, and signed Xcode build validation after the QA/build pipeline changes.

Evidence:

- EditMode tests: `StormBlocksUnity/editmode-results.xml` reports 25 total, 25 passed, 0 failed at `2026-05-05 05:16:05Z`.
- PlayMode tests: `StormBlocksUnity/playmode-results.xml` reports 7 total, 7 passed, 0 failed at `2026-05-05 05:07:27Z`.
- Current logged mobile baseline: 277 renderers, 60,024 mesh triangles, 1 audio listener, 1 canvas.
- Unity iOS Simulator export: `/tmp/stormblocks-ios-simulator-export-final.log` reports `Build Finished, Result: Success`; XcodeBuildMCP built, installed, launched, and screenshotted `com.perlantir.stormblocks` on booted iPhone 16 Pro simulator `BFD7E422-B789-4380-9588-B952559B6A92`.
- Unity iOS device export: `/tmp/stormblocks-ios-final-device-after-sim.log` reports `Build Finished, Result: Success`, shows iOS device destinations in Xcode, and no `PerformanceTestRun` resources remain in the source `Assets` tree or exported iOS `Data` output.
- Exported Xcode project includes `StormBlocks.entitlements`, `CODE_SIGN_ENTITLEMENTS = StormBlocks.entitlements`, `GameKit.framework`, and `Libraries/Plugins/iOS/StormBlocksGameKitBridge.mm`.
- Unsigned Xcode device build: `/tmp/stormblocks-xcode-final-device-after-sim-unsigned.log` reports `** BUILD SUCCEEDED **`; output app exists at `StormBlocksUnity/Builds/iOS/DerivedDataUnsignedFinalDeviceAfterSim/Build/Products/Release-iphoneos/StormBlocks.app`.
- Signed Xcode device build attempt: `/tmp/stormblocks-xcode-final-device-after-sim-signed.log` fails before compile with `No Account for Team "84D222Q647"` and no provisioning profile for `com.perlantir.stormblocks`. The local keychain contains `Apple Development: Nick Gallick (84D222Q647)`, so the remaining blocker is Xcode account/provisioning visibility rather than code generation or native compilation.
- Device visibility: `xcrun devicectl list devices` sees a paired `iPhone 17 Pro Max (iPhone18,2)`, so the missing device run is gated by signing/provisioning rather than hardware discovery.

Known risks / not done:

- Signed device/TestFlight validation remains blocked until Xcode can see an Apple account/team and provisioning profile for `com.perlantir.stormblocks`.
- Game Center leaderboards and achievement ids still need to be created/enabled in App Store Connect and validated on a signed device.
- Physical-device touch, visual readability, haptics/audio feel, performance, and thermal profiling are still required before release signoff.

## 2026-05-05 — Team 7JL signing, device install, archive, and IPA export

- Discovered that local Xcode provisioning profiles and App Store Connect access are available for team `7JL22TDB44` / `UBER KIWI LLC`, while the prior Unity export defaulted to stale team id `84D222Q647`.
- Updated the Unity iOS build pipeline to export with `PlayerSettings.iOS.appleDeveloperTeamID = "7JL22TDB44"`.
- Regenerated the iOS Xcode project and confirmed `DEVELOPMENT_TEAM = 7JL22TDB44`, automatic signing, `StormBlocks.entitlements`, and `com.apple.developer.game-center = true`.
- Re-ran the signed Xcode device build without command-line team overrides; Xcode created/used development profile `iOS Team Provisioning Profile: com.perlantir.stormblocks`.
- Installed the signed app on the paired physical iPhone.
- Built an Xcode archive and exported an App Store Connect IPA using cloud-managed Apple Distribution signing and the store provisioning profile.
- Probed direct App Store Connect upload; Xcode authenticated and queried the selected provider, but upload failed because there is no App Store Connect app record for bundle id `com.perlantir.stormblocks`.

Evidence:

- Unity iOS device export: `/tmp/stormblocks-ios-device-team7jl.log` reports `Build Finished, Result: Success`.
- Signed Xcode device build: `/tmp/stormblocks-xcode-team7jl-default-signed.log` reports `** BUILD SUCCEEDED **`; output app is `StormBlocksUnity/Builds/iOS/DerivedDataSignedTeam7JLDefault/Build/Products/Release-iphoneos/StormBlocks.app`.
- Signed app entitlements: `application-identifier = 7JL22TDB44.com.perlantir.stormblocks`, `com.apple.developer.game-center = true`, and `get-task-allow = true`.
- Device install: `/tmp/stormblocks-device-install.json` reports `outcome = success` for bundle id `com.perlantir.stormblocks` on device `907E2EE7-9C7B-5D0D-9EC0-32E69912287D`.
- Device launch probe: `/tmp/stormblocks-device-launch.json` fails only because the iPhone was locked (`FBSOpenApplicationErrorDomain` code `7`, `Locked`).
- Xcode archive: `/tmp/stormblocks-xcode-team7jl-archive.log` reports `** ARCHIVE SUCCEEDED **`; archive exists at `StormBlocksUnity/Builds/iOS/Archives/StormBlocks-Team7JL.xcarchive`.
- App Store Connect IPA export: `/tmp/stormblocks-xcode-team7jl-export-appstore.log` reports `** EXPORT SUCCEEDED **`; IPA exists at `StormBlocksUnity/Builds/iOS/ExportAppStoreTeam7JL/StormBlocks.ipa`.
- Export summary reports `Cloud Managed Apple Distribution`, team `7JL22TDB44`, arm64, Game Center entitlement, `beta-reports-active = true`, `get-task-allow = false`, and store profile `iOS Team Store Provisioning Profile: com.perlantir.stormblocks`, UUID `013406e3-56cb-44d6-a480-81d7af0bac49`.
- App Store Connect upload probe: `/tmp/stormblocks-xcode-team7jl-upload-appstore.log` fails with `exportArchive Error Downloading App Information`; distribution logs show App Store Connect returned `data: []` and `total: 0` for bundle id `com.perlantir.stormblocks`.

Known risks / not done:

- Create the App Store Connect app record for bundle id `com.perlantir.stormblocks` under team `7JL22TDB44` / `UBER KIWI LLC`, then upload the exported IPA.
- Unlock the paired iPhone and rerun launch/runtime checks.
- Game Center leaderboards and achievement ids still need to be created/enabled in App Store Connect and validated on a signed device.
- Physical-device touch, visual readability, haptics/audio feel, performance, and thermal profiling are still required before release signoff.

## 2026-05-05 — Release audit and repeatable gate runner

- Added `Docs/RELEASE_AUDIT.md` as a prompt-to-artifact checklist mapping the launch request and release checklist to concrete evidence and open gates.
- Added `Scripts/ios_release_gates.sh` to rerun or verify local release gates: tests, Unity iOS export, signed Xcode build, device install/launch, archive, App Store IPA export, and App Store Connect upload probe.
- Added App Store Connect record fields to `Docs/APP_STORE_PLAN.md` so the missing app record can be created without rediscovering bundle/team/SKU values.

Evidence:

- `bash -n Scripts/ios_release_gates.sh` passes.
- `Scripts/ios_release_gates.sh status` confirms cached EditMode, PlayMode, Unity export, unsigned build, signed build, archive, IPA export, and IPA provisioning evidence.

Known risks / not done:

- App Store Connect still has no app record for `com.perlantir.stormblocks`.
- Physical-device launch remains blocked until the installed iPhone is unlocked.
- Game Center live identifiers, TestFlight upload/install, human five-run QA, and physical performance QA remain open.

## 2026-05-05 — Fastlane App Store Connect handoff

- Added `fastlane/Appfile` and `fastlane/Fastfile` with lanes for creating the missing App Store Connect app record, uploading the exported IPA to TestFlight, and running both as a release-candidate upload.
- Added Fastlane beta metadata for description, feedback email, and release notes.
- Added a root `Gemfile` declaring Fastlane for reproducible release tooling.
- Documented the Fastlane credential options in `fastlane/STORMBLOCKS_RELEASE.md`, `Docs/APP_STORE_PLAN.md`, and `Docs/RELEASE_AUDIT.md`.

Evidence:

- `fastlane --version` reports `fastlane 2.233.1`.
- `fastlane action produce`, `fastlane action pilot`, and `fastlane action app_store_connect_api_key` were inspected locally before adding lanes.
- `FASTLANE_SKIP_UPDATE_CHECK=1 fastlane lanes` lists the `ios create_app_record`, `ios upload_testflight`, and `ios release_candidate_upload` lanes.
- `FASTLANE_SKIP_UPDATE_CHECK=1 fastlane ios create_app_record` fails cleanly without credentials with `Set App Store Connect API key env vars or STORMBLOCKS_APPLE_ID/APPLE_ID before running credentialed lanes.`
- `Scripts/ios_release_gates.sh launch-device` was retried and still fails only because the paired iPhone is locked.

Known risks / not done:

- Fastlane credentialed lanes still require App Store Connect API key environment variables or Apple ID app-specific password.
- The App Store Connect app record for `com.perlantir.stormblocks` is still missing until `Scripts/fastlane_release.sh ios create_app_record` or the App Store Connect UI creates it.

## 2026-05-05 — Machine-verifiable release audit

- Added `Scripts/release_audit.sh` with `local` and `full` modes.
- `local` verifies concrete cached evidence for tests, scene budget, Unity iOS export, unsigned build, signed build, device install, archive, App Store IPA export, IPA team/bundle/Game Center entitlements, Fastlane lanes, and absence of transient generated artifacts.
- `full` runs the same evidence checks and then fails with explicit open gates for locked-device launch, missing App Store Connect app record/TestFlight upload, live Game Center validation, human device QA, and physical performance profiling.

Evidence:

- `Scripts/release_audit.sh local` passes.
- `Scripts/release_audit.sh full` exits nonzero with open gates only, not local evidence failures.

Known risks / not done:

- The full audit intentionally remains non-green until the external release gates are complete.

## 2026-05-05 — Xcode and App Store Connect retry handoff

- Re-ran the full release audit after Xcode/App Store Connect were open locally; it still reports 21 pass, 0 fail, and 7 open external gates.
- Retried signed physical-device launch through `Scripts/ios_release_gates.sh launch-device`; the app still cannot be launched because the paired iPhone is locked.
- Retried Xcode's App Store Connect upload probe; Xcode authenticated to the provider and failed at the app-record fetch step with `missingApp(bundleId: "com.perlantir.stormblocks")`.
- Checked non-UI credential paths: `xcrun altool --list-providers` requires JWT API-key auth or username/app-password auth, and Fastlane's app-record lane requires App Store Connect API key variables or an Apple ID env var.
- Attempted desktop UI automation for App Store Connect/Xcode, but macOS rejected the automation sender with `Sender process is not authenticated`.

Evidence:

- `Scripts/release_audit.sh full` reports 21 pass, 0 fail, 7 open.
- `Scripts/ios_release_gates.sh launch-device` fails with `FBSOpenApplicationErrorDomain` code `7`, `Locked`.
- `/tmp/stormblocks-xcode-team7jl-upload-appstore.log` fails with `exportArchive Error Downloading App Information`; the distribution log reports `IDEDistribution.DistributionAppRecordProviderError.missingApp(bundleId: "com.perlantir.stormblocks")`.
- `FASTLANE_SKIP_UPDATE_CHECK=1 fastlane ios create_app_record` fails cleanly with `Set App Store Connect API key env vars or STORMBLOCKS_APPLE_ID/APPLE_ID before running credentialed lanes.`

Known risks / not done:

- Create the App Store Connect app record or provide App Store Connect API-key/Apple-ID credentials for the credentialed Fastlane lane.
- Unlock the paired iPhone and rerun launch/runtime checks.
- Live Game Center, TestFlight install, physical QA, physical performance profiling, and human five-run playability gates remain open.

## 2026-05-05 — Fastlane Bundler wrapper

- Found that the default `/usr/bin/bundle` uses Apple Ruby 2.6 and cannot run the checked-in `Gemfile.lock`, which was generated with Bundler 4.0.10 under Homebrew Ruby 4.0.3.
- Added `Scripts/fastlane_release.sh` to run Fastlane lanes through Bundler with `/opt/homebrew/opt/ruby/bin` ahead of `/usr/bin`.
- Updated release docs and the machine-verifiable release audit to use the wrapper for credentialed App Store Connect lanes.

Evidence:

- `PATH="/opt/homebrew/opt/ruby/bin:$PATH" bundle exec fastlane lanes` lists `ios create_app_record`, `ios upload_testflight`, and `ios release_candidate_upload`.
- `Scripts/fastlane_release.sh lanes` lists the same credentialed release lanes.
- `Scripts/fastlane_release.sh ios create_app_record` now reaches the Fastlane lane through Bundler and fails cleanly only because App Store Connect credentials are not set.

Known risks / not done:

- The wrapper fixes local lane execution, but credentialed lanes still need App Store Connect API-key variables or Apple ID credentials.

## 2026-05-05 — App Store metadata and Game Center manifest package

- Added `Docs/APP_STORE_CONNECT_MANIFEST.json` with the App Store record fields, metadata URLs, screenshot set, Game Center leaderboard identifiers, and Game Center achievement definitions.
- Added customer-facing support and privacy page drafts at `Docs/PUBLIC_SUPPORT.md` and `Docs/PUBLIC_PRIVACY.md`.
- Added tracked Fastlane metadata under `fastlane/metadata/en-US/` and copied the current five generated 1170 x 2532 screenshots into `fastlane/screenshots/en-US/`.
- Added `Scripts/verify_release_assets.sh` to validate App Store metadata length/byte limits, screenshot dimensions, and Game Center identifier alignment across the manifest, setup docs, and runtime GameKit adapter.
- Added Fastlane lanes for local release-asset validation and metadata/screenshot upload after the App Store Connect app record exists.
- Updated App Store, Game Center, release audit, and build/test docs to reference the new release-asset package.

Evidence:

- `Scripts/verify_release_assets.sh` passes.
- `Scripts/fastlane_release.sh lanes` lists `ios validate_release_assets`, `ios upload_metadata`, `ios create_app_record`, `ios upload_testflight`, and `ios release_candidate_upload`.
- `Scripts/fastlane_release.sh ios validate_release_assets` runs successfully and the wrapper removes the generated Fastlane `report.xml` artifact afterward.
- `Scripts/release_audit.sh full` reports 26 pass, 0 fail, and the same 7 external open gates.

Known risks / not done:

- The support, marketing, and privacy URLs in the manifest still need public reachability/customer-support review before App Store submission.
- The metadata upload lane remains credentialed and cannot run until the App Store Connect app record and credentials are available.

## 2026-05-05 — Physical QA and profiling handoff

- Added `Docs/PHYSICAL_QA_RUNBOOK.md` with the exact five-run playability test, physical-device functional checks, Game Center device checks, performance profiling steps, and TestFlight validation pass.
- Added `Scripts/device_qa_session.sh` with a non-credentialed `plan` mode plus launch and `xctrace` helpers for Game Performance and Power Profiler traces once the iPhone is unlocked.
- Updated QA, performance, release audit, and implementation docs so the remaining physical gates have a repeatable execution path.

Evidence:

- `Scripts/device_qa_session.sh plan` prints the physical QA sequence and external open gates.
- `bash -n Scripts/device_qa_session.sh Scripts/release_audit.sh Scripts/ios_release_gates.sh Scripts/fastlane_release.sh Scripts/verify_release_assets.sh` passes.

Known risks / not done:

- The launch/profile modes still require an unlocked physical device and human execution.

## 2026-05-05 — GitHub static release checks

- Added `Scripts/ci_static_checks.sh` for credential-free static release verification: shell syntax, App Store metadata/screenshot validation, manifest JSON parsing, and transient artifact checks.
- Added `.github/workflows/release-static.yml` to run the static release checks on macOS for pushes, pull requests, and manual workflow dispatch.
- Updated the release audit and build/test docs to track the GitHub static workflow as local release infrastructure.

Evidence:

- `Scripts/ci_static_checks.sh` passes locally.
- `Scripts/release_audit.sh full` includes the static CI script and workflow presence checks.
- GitHub Actions run `25361499244` passed for commit `88625c8` at `https://github.com/perlantir/stormblocks/actions/runs/25361499244`; the initial run used `actions/checkout@v4` and produced a future Node 20 deprecation warning, so the workflow was updated to `actions/checkout@v5`.
- GitHub Actions run `25361526029` passed for commit `c43babe` at `https://github.com/perlantir/stormblocks/actions/runs/25361526029` with `actions/checkout@v5`.

Known risks / not done:

- Remote GitHub Actions proof requires the workflow to run on GitHub after this commit is pushed.

## 2026-05-05 — Prompt compliance verifier

- Added `Scripts/verify_prompt_compliance.sh` to statically verify the explicit launch prompt against repo artifacts: required docs, required design references, Unity/C# project structure, full-mode symbols, automatic Storm Pushback symbols, service interfaces, accessibility settings, 3D presentation markers, test-result files, release audit presence, and no ad/IAP/clone/copyrighted-IP monetization drift in runtime surfaces.
- Wired the verifier into `Scripts/ci_static_checks.sh` and `Scripts/release_audit.sh` so both local audit and GitHub static checks cover prompt compliance in addition to App Store metadata.
- Updated release audit and build/test docs with the new verifier command.

Evidence:

- `Scripts/verify_prompt_compliance.sh` passes locally.
- `Scripts/ci_static_checks.sh` passes locally with the prompt compliance verifier included.
- GitHub Actions run `25361676599` passed for commit `e3e8339` at `https://github.com/perlantir/stormblocks/actions/runs/25361676599` with the prompt compliance verifier included in static checks.

Known risks / not done:

- The verifier is a static guardrail. It complements but does not replace the still-open physical-device, App Store Connect, Game Center, and TestFlight gates.

## 2026-05-05 — GitHub external release gate tracking

- Created GitHub milestone `Release Candidate External Gates` to track Apple, TestFlight, Game Center, and physical-device tasks that cannot be completed without external credentials/device state.
- Created focused release-gate issues for the remaining external work:
  - App Store Connect app record: https://github.com/perlantir/stormblocks/issues/1
  - Live Game Center identifiers and validation: https://github.com/perlantir/stormblocks/issues/2
  - TestFlight upload/install/launch validation: https://github.com/perlantir/stormblocks/issues/7
  - Physical performance and thermal profiling: https://github.com/perlantir/stormblocks/issues/8
  - Physical-device QA and five-run playability test: https://github.com/perlantir/stormblocks/issues/9
- Updated `Docs/RELEASE_AUDIT.md` so every open external gate points to the corresponding GitHub issue.

Evidence:

- `gh issue list --repo perlantir/stormblocks --state open --milestone "Release Candidate External Gates"` lists the five open external gate issues above.

Known risks / not done:

- The issues track the work but do not close any release checklist gate until the Apple/device tasks are actually completed and evidence is recorded.

## 2026-05-05 — Gate 12 pooling and Low Detail fallback

- Added primitive pooling for dynamic presentation cubes/spheres used by board refreshes, tray rebuilds, drag ghosts, survivors, storm cells, block cells, and Storm Pushback VFX.
- Added a persisted Low Detail accessibility setting and profile codec coverage.
- Added automatic Low Detail fallback heuristics for constrained physical iOS hardware while keeping full detail available on newer devices.
- Tuned Low Detail to preserve board readability, survivors, storm identity, and the signature gold Storm Pushback moment while trimming secondary puffs, rain, duplicate cyan waves, shatter lightning, and block highlight dots.
- Expanded PlayMode accessibility smoke coverage so the Low Detail control remains reachable.
- Re-ran EditMode, PlayMode, simulator export/run, device export, unsigned Xcode device build, and signed Xcode validation after the performance/readiness pass.

Evidence:

- EditMode tests: `StormBlocksUnity/editmode-results.xml` reports 25 total, 25 passed, 0 failed at `2026-05-05 05:30:28Z`.
- PlayMode tests: `StormBlocksUnity/playmode-results.xml` reports 7 total, 7 passed, 0 failed at `2026-05-05 05:30:53Z`.
- Current logged mobile baseline: 274 renderers, 59,232 mesh triangles, 1 audio listener, 1 canvas.
- Unity iOS Simulator export: `/tmp/stormblocks-ios-simulator-lowdetail-pool.log` reports `Build Finished, Result: Success`; XcodeBuildMCP built, installed, launched, and screenshotted `com.perlantir.stormblocks` on booted iPhone 16 Pro simulator `BFD7E422-B789-4380-9588-B952559B6A92`.
- Unity iOS device export: `/tmp/stormblocks-ios-device-lowdetail-pool.log` reports `Build Finished, Result: Success`, and no `PerformanceTestRun`, `InitTestScene`, or `mono_crash` artifacts remain under the source `Assets` tree or exported iOS/simulator project outputs.
- Unsigned Xcode device build: `/tmp/stormblocks-xcode-lowdetail-pool-unsigned.log` reports `** BUILD SUCCEEDED **`; output app exists at `StormBlocksUnity/Builds/iOS/DerivedDataUnsignedLowDetailPool/Build/Products/Release-iphoneos/StormBlocks.app`.
- Signed Xcode device build attempt at that point: `/tmp/stormblocks-xcode-lowdetail-pool-signed.log` failed before compile with `No Account for Team "84D222Q647"` and no provisioning profile for `com.perlantir.stormblocks`. This was superseded by the later team `7JL22TDB44` signing pass.

Known risks / not done:

- Signed build, install, archive, and IPA export were resolved in the later team `7JL22TDB44` pass.
- Game Center leaderboards and achievement ids still need to be created/enabled in App Store Connect and validated on a signed device.
- Physical-device touch, visual readability, haptics/audio feel, performance, and thermal profiling are still required before release signoff.

## 2026-05-05 — Reference-led gameplay visual correction

- Reworked the Unity gameplay presentation to move away from the flat primitive look and closer to the supplied design references:
  - Added runtime rounded/sliced UI sprites for HUD panels, buttons, modal screens, and text shadows/outlines.
  - Added procedural glossy toy textures for board tiles, storm tiles, warning tiles, camp tiles, and block pieces.
  - Rebuilt the board shell with a warmer rim, cyan storm barrier, storm cloud corners, lightning accents, and richer storm edge dressing.
  - Reworked the bottom tray with rounded caps, stitched/warm rails, larger toy-piece pads, and textured piece materials.
  - Reworked the camp and survivors with a clearer tent silhouette, doorway, folds, flag, fire, string lights, and larger character silhouettes.
  - Corrected the color balance after an intermediate shader pass over-warmed the scene, restoring the blue-purple storm palette.
- Regenerated the portrait gameplay capture and App Store screenshot set from the updated Unity presentation.
- Copied the regenerated screenshot set into `fastlane/screenshots/en-US`.
- Updated release evidence docs and audit scripts for the new mobile-scene baseline.

Evidence:

- Portrait visual capture: `/tmp/stormblocks-visual-capture-design6.log` completed successfully and wrote `StormBlocksUnity/Builds/VisualChecks/stormblocks-gameplay.png`.
- App Store screenshot capture: `/tmp/stormblocks-appstore-design.log` completed successfully and wrote five screenshots under `StormBlocksUnity/Builds/AppStoreScreens`.
- EditMode tests: `StormBlocksUnity/editmode-results.xml` reports 25 total, 25 passed, 0 failed at `2026-05-05 12:22:09Z`.
- PlayMode tests: `StormBlocksUnity/playmode-results.xml` reports 7 total, 7 passed, 0 failed at `2026-05-05 12:23:33Z`.
- Current logged mobile baseline: 337 renderers, 75,850 mesh triangles, 1 audio listener, 1 canvas.
- `Scripts/verify_release_assets.sh` passes locally: 32 pass, 0 fail.
- `Scripts/verify_prompt_compliance.sh` passes locally: 73 pass, 0 fail.
- `Scripts/ci_static_checks.sh` passes locally after removing transient Unity test scenes from `StormBlocksUnity/Assets`.
- `Scripts/release_audit.sh local` passes locally: 32 pass, 0 fail, 0 open.
- `Scripts/release_audit.sh full` still reports 32 pass, 0 fail, 7 open external gates.

Known risks / not done:

- This pass materially improves the procedural Unity presentation, but it is still not equivalent to bespoke production illustration/modeling. Importing supplied layered design assets or generated target frames would allow a much closer match.
- Physical-device readability, haptics/audio feel, performance, and thermal profiling remain required before release signoff.

## 2026-05-05 — External gate reprobe after visual pass

- Rechecked the remaining Apple/device gates after pushing commit `def09d6`.
- Confirmed branch-head GitHub Actions static verification passed for `def09d6`: `https://github.com/perlantir/stormblocks/actions/runs/25376369090`.
- Re-ran `Scripts/ios_release_gates.sh upload-probe`; Xcode authenticated to App Store Connect, queried bundle id `com.perlantir.stormblocks`, received HTTP 200 with `data: []`, and failed with `missingApp(bundleId: "com.perlantir.stormblocks")`.
- Re-ran the physical-device launch gate; `devicectl` connected to paired iPhone `907E2EE7-9C7B-5D0D-9EC0-32E69912287D`, but launch was denied because the device was locked.
- Rechecked desktop automation availability; Computer Use still reports `Sender process is not authenticated`, so App Store Connect browser setup cannot be driven by automation from this session.

Evidence:

- GitHub Actions run: `https://github.com/perlantir/stormblocks/actions/runs/25376369090`.
- Upload probe log: `/tmp/stormblocks-xcode-team7jl-upload-appstore.log`.
- App Store Connect distribution log: `/var/folders/b2/cl2rv8q13bg48zl073ctm_fc0000gq/T/Unity-iPhone_2026-05-05_07-30-41.300.xcdistributionlogs/IDEDistributionAppStoreConnect.log`.
- Device list: `/tmp/stormblocks-devicectl-list.json`.
- Device launch failure: `/tmp/stormblocks-device-launch.json` and `/tmp/stormblocks-device-launch.log`.

Known risks / not done:

- App Store Connect app record still must be created for `com.perlantir.stormblocks`.
- Live Game Center identifiers, TestFlight upload/install, unlocked-device launch, physical QA, and physical performance profiling remain open.

## 2026-05-05 — Rounded gameplay visual pass and GPT-image-2 target attempt

- Reworked the runtime gameplay presentation toward the supplied references with rounded generated board meshes, UV-mapped glossy tile insets, a painted storm-sunset backdrop, fuller storm edge dressing, clearer blue-purple storm palette, detailed survivor faces/hope bubbles, camp companions, a title-bearing HUD, stronger App Store screenshot capture anti-aliasing, and refreshed App Store screenshot copies under `fastlane/screenshots/en-US/`.
- Added `Docs/VisualTargets/stormblocks_gptimage2_gameplay_target_prompt.md` as an auditable GPT-image-2 target-frame prompt.
- Attempted the repo-bound GPT-image-2 edit workflow against `StormBlocksUnity/Builds/VisualChecks/stormblocks-gameplay.png`; OpenAI returned 403 because the organization must be verified for `gpt-image-2`, so the generated target frame remains blocked on account verification.
- Updated the mobile scene-budget baseline to 409 renderers, 136,600 triangles, 1 audio listener, and 1 canvas after the rounded visual pass.

Evidence:

- Portrait visual capture: `/tmp/stormblocks-visual-capture-final-rounded-pass.log` completed successfully and wrote `StormBlocksUnity/Builds/VisualChecks/stormblocks-gameplay.png`.
- App Store screenshot capture: `/tmp/stormblocks-appstore-final-rounded-pass.log` completed successfully and regenerated the five 1170 x 2532 PNGs under `StormBlocksUnity/Builds/AppStoreScreens/`; the files were copied into `fastlane/screenshots/en-US/`.
- EditMode: `/tmp/stormblocks-editmode-rounded-visuals.log` completed successfully and `StormBlocksUnity/editmode-results.xml` reports 25 total, 25 passed, 0 failed.
- PlayMode after the renderer guard update reports 7 total, 7 passed, 0 failed at `2026-05-05 12:53:21Z`; final measured rounded-visuals budget is 409 renderers, 136,600 triangles, 1 audio listener, and 1 canvas.
- `Scripts/verify_release_assets.sh` passes locally: 32 pass, 0 fail.
- `Scripts/verify_prompt_compliance.sh` passes locally: 73 pass, 0 fail.
- `Scripts/ci_static_checks.sh` passes locally.
- `Scripts/release_audit.sh local` passes locally: 32 pass, 0 fail, 0 open.
- `Scripts/release_audit.sh full` reports 32 pass, 0 fail, and the same 7 open external gates.

Known risks / not done:

- The GPT-image-2 target frame cannot be generated until the OpenAI organization is verified for `gpt-image-2`.
- The higher visual baseline still requires physical-device profiling before release signoff.

## 2026-05-05 — Physical-device launch gate cleared

- Re-ran `Scripts/ios_release_gates.sh launch-device`; `devicectl` acquired the paired iPhone connection and launched `com.perlantir.stormblocks`.
- Re-ran `Scripts/ios_release_gates.sh upload-probe`; Xcode authenticated to App Store Connect but the app-record lookup still returned HTTP 200 with `data: []`, followed by `missingApp(bundleId: "com.perlantir.stormblocks")`.
- Updated the release audit to remove unlocked-device launch as an open gate. Physical-device QA, physical performance profiling, App Store Connect app record creation, live Game Center validation, and TestFlight validation remain open.

Evidence:

- Device launch success: `/tmp/stormblocks-device-launch.json` reports `"outcome" : "success"` for paired iPhone `907E2EE7-9C7B-5D0D-9EC0-32E69912287D`.
- Device launch log: `/tmp/stormblocks-device-launch.log` includes `Launched application with com.perlantir.stormblocks bundle identifier.`
- Upload probe log: `/tmp/stormblocks-xcode-team7jl-upload-appstore.log`.
- App Store Connect distribution log: `/var/folders/b2/cl2rv8q13bg48zl073ctm_fc0000gq/T/Unity-iPhone_2026-05-05_08-00-41.823.xcdistributionlogs/IDEDistributionAppStoreConnect.log`.
- `Scripts/release_audit.sh full` now reports 33 pass, 0 fail, and 6 open external gates.

## 2026-05-05 — Safe-palette gameplay presentation refresh

- Rebalanced the gameplay capture after reference review: lighter lavender board cells for readability, less muddy storm-sunset backdrop, lower tray framing, added foreground haze/cloud fill, and rounded system-font preference for HUD/screen labels before the Unity fallback font.
- Tested a global lit-shader pass, rejected it because it over-warmed the board and camp away from the blue-purple storm target, and kept the safer unlit/toy-texture path.
- Regenerated the portrait gameplay capture and all five App Store screenshot PNGs, then refreshed the tracked Fastlane screenshot copies.
- Updated release evidence docs, QA docs, physical-QA wording, and audit scripts for the new scene-budget baseline and the already-cleared physical launch gate.

Evidence:

- Portrait visual capture: `/tmp/stormblocks-visual-safe-palette.log` completed successfully and wrote `StormBlocksUnity/Builds/VisualChecks/stormblocks-gameplay.png`.
- App Store screenshot capture: `/tmp/stormblocks-appstore-safe-palette.log` completed successfully and regenerated the five 1170 x 2532 PNGs under `StormBlocksUnity/Builds/AppStoreScreens/`; the files were copied into `fastlane/screenshots/en-US/`.
- EditMode tests: `StormBlocksUnity/editmode-results.xml` reports 25 total, 25 passed, 0 failed at `2026-05-05 13:20:30Z`.
- PlayMode tests: `StormBlocksUnity/playmode-results.xml` reports 7 total, 7 passed, 0 failed at `2026-05-05 13:20:37Z`.
- Current logged mobile baseline: 421 renderers, 142,792 mesh triangles, 1 audio listener, 1 canvas.
- `Scripts/verify_release_assets.sh` passes locally: 32 pass, 0 fail.
- `Scripts/verify_prompt_compliance.sh` passes locally: 73 pass, 0 fail.
- `Scripts/ios_release_gates.sh all-local` refreshed Unity iOS export, signed build, physical install, archive, and App Store IPA export; final status initially required the stale 424/143,584 budget, then the audit scripts were updated to the actual PlayMode rerun baseline.
- `Scripts/ios_release_gates.sh launch-device` succeeded after the refreshed install at `2026-05-05 13:30:29Z`.
- `Scripts/ios_release_gates.sh upload-probe` still fails at `2026-05-05 13:30:39Z` because App Store Connect returns `data: []` and `missingApp(bundleId: "com.perlantir.stormblocks")`; current distribution log is `/var/folders/b2/cl2rv8q13bg48zl073ctm_fc0000gq/T/Unity-iPhone_2026-05-05_08-30-37.610.xcdistributionlogs/IDEDistributionAppStoreConnect.log`.
- Branch-head GitHub Actions `Release Static Checks` passed for commit `ee0b9f1`: `https://github.com/perlantir/stormblocks/actions/runs/25379625311`.

Known risks / not done:

- This improves the current procedural Unity art direction, but it still is not a substitute for bespoke layered production art/modeling. The GPT-image-2 target-frame route remains blocked on OpenAI organization verification.
- Physical-device visual QA, touch feel, haptics/audio feel, and physical performance profiling remain required before release signoff.

## 2026-05-05 — Lightweight launch scene cleanup

- Updated the project bootstrap so the saved `StormBlocksMain` scene stays as a lightweight launcher with a single `StormBlocksGameView`; generated board meshes, UI, VFX, and pooled runtime objects are built at runtime instead of being serialized into the scene asset.
- Changed bootstrap to reuse the existing URP asset instead of deleting and recreating it, preventing unrelated render-pipeline serialization churn during export/bootstrap runs.
- Normalized the existing scene in-place so repeated bootstrap/export runs preserve the launcher object and do not churn scene file IDs.
- Kept the strict PlayMode scene-budget baseline on the full-detail runtime path after isolating test saves from the local editor profile: 421 renderers, 142,792 mesh triangles, 1 audio listener, and 1 canvas.

Evidence:

- Bootstrap stability: `/tmp/stormblocks-bootstrap-normalize-3.log` exited successfully after a repeated `StormBlocks.Editor.StormBlocksProjectBootstrap.ConfigureProject` run.
- `Scripts/ios_release_gates.sh all-local` completed successfully after the full-detail baseline correction; it refreshed Unity tests, Unity iOS export, signed Release build, physical install, Xcode archive, App Store IPA export, and cached status verification.
- `Scripts/ios_release_gates.sh launch-device` succeeded after the refreshed signed install at `2026-05-05 14:04:14Z`.
- `Scripts/ios_release_gates.sh upload-probe` still fails at `2026-05-05 14:04:24Z` because App Store Connect returns `data: []` and `missingApp(bundleId: "com.perlantir.stormblocks")`; current distribution log is `/var/folders/b2/cl2rv8q13bg48zl073ctm_fc0000gq/T/Unity-iPhone_2026-05-05_09-04-22.356.xcdistributionlogs/IDEDistributionAppStoreConnect.log`.
- EditMode tests: `StormBlocksUnity/editmode-results.xml` reports 25 total, 25 passed, 0 failed at `2026-05-05 13:59:05Z`.
- PlayMode tests: `StormBlocksUnity/playmode-results.xml` reports 7 total, 7 passed, 0 failed at `2026-05-05 13:59:12Z`.
- Current logged full-detail mobile baseline: 421 renderers, 142,792 mesh triangles, 1 audio listener, 1 canvas.
- `Scripts/release_audit.sh local` reports 32 pass, 0 fail, 0 open.
- `Scripts/release_audit.sh full` reports 33 pass, 0 fail, and 6 open external gates.
- `Scripts/ci_static_checks.sh` passes locally.
- Branch-head GitHub Actions `Release Static Checks` passed for commit `939c474`: `https://github.com/perlantir/stormblocks/actions/runs/25381352963`.

Known risks / not done:

- Physical-device profiling is still required before release signoff; this change improves scene hygiene and automated budget evidence but does not replace device FPS/thermal measurement.

## 2026-05-05 — Physical profiling helper hardening

- Updated `Scripts/device_qa_session.sh` so profiling commands resolve the hardware UDID that `xctrace` expects from the paired `devicectl` device instead of passing the CoreDevice identifier used for install/launch.
- Documented the `STORMBLOCKS_XCTRACE_DEVICE_ID` override in the physical QA runbook.
- Retried a short Game Performance capture; the helper resolved `00008150-00040D203A88401C`, but Instruments timed out with `Timed out waiting for device to boot: iPhone (26.3)`.

Evidence:

- `Scripts/device_qa_session.sh launch` / `Scripts/ios_release_gates.sh launch-device` can launch the signed app through CoreDevice id `907E2EE7-9C7B-5D0D-9EC0-32E69912287D`.
- `STORMBLOCKS_PROFILE_TIME=15s Scripts/device_qa_session.sh profile-game` now reaches `xctrace` with hardware UDID `00008150-00040D203A88401C` and fails on Instruments device visibility, not on script identifier mismatch.
- `Scripts/fastlane_release.sh ios create_app_record` still fails immediately without App Store Connect API key variables or `STORMBLOCKS_APPLE_ID` / `APPLE_ID`.
- Desktop automation for the logged-in App Store Connect UI is still blocked by macOS with `Sender process is not authenticated`.

Known risks / not done:

- Physical performance profiling still requires Instruments/xctrace to see the paired iPhone online, plus an older supported iPhone pass.
- App Store Connect app-record creation still requires API credentials, Apple ID/app-specific password, or authenticated manual browser/Xcode UI work.

## 2026-05-05 — Design-source GLB optimization and runtime board correction

- Reviewed the new `Design GLB`, `Design JPG`, and `Design Sample Video` folders and documented the source/runtime decisions in `Docs/DESIGN_ASSET_REVIEW.md`.
- Added `Scripts/optimize_design_glbs.sh` to produce mobile GLB variants with glTF Transform instead of shipping the raw high-poly source models directly.
- Added `.gitignore` protection for the raw design source folders so the repo keeps optimized runtime assets and avoids accidental 300 MB+ source drops without Git LFS.
- Added Unity GLTFast package dependency `com.unity.cloud.gltfast` `6.18.0` and imported curated optimized GLBs under `Assets/StormBlocks/Art/Imported`.
- Generated a blurred 1170 x 2532 design-source storm backdrop from the supplied phone-layout JPG and loaded it at runtime through `Resources/StormSky`.
- Rejected the direct campfire-rescue GLB overlay in the central board after visual capture because it harmed camp clarity; kept the procedural warm camp as the runtime presentation.
- Updated the actual playable `StormBlocksGameView` board, not just screenshots: narrower board metrics, camera/tray layout, storm spiral edge accents, bottom halo, and runtime tray charm now move closer to the supplied gameplay references while preserving board readability.
- Regenerated portrait gameplay and App Store screenshot captures, then refreshed `fastlane/screenshots/en-US`.

Evidence:

- Raw GLB review found 16 valid GLBs totaling 311 MB; individual raw models range from roughly 178k to 383k triangles, with the campfire-rescue source at roughly 1.4M triangles.
- Optimized runtime/source GLBs now include `blue_2x2_block_mobile_lod1.glb` at 249 KB / 7,452 triangles, `lightning_cloud_cube_mobile_lod1.glb` at 1.1 MB / 3,902 triangles, and `stormy_campfire_rescue_mobile_lod1.glb` at 2.6 MB / 70,165 triangles.
- EditMode tests: `StormBlocksUnity/editmode-results.xml` reports 25 total, 25 passed, 0 failed at `2026-05-05 15:09:10Z`.
- PlayMode tests: `StormBlocksUnity/playmode-results.xml` reports 7 total, 7 passed, 0 failed at `2026-05-05 15:09:18Z`.
- Current logged full-detail mobile baseline after the board pass: 356 renderers, 133,932 mesh triangles, 1 audio listener, and 1 canvas.
- Portrait visual capture: `/tmp/stormblocks-visual-design-backdrop-crop.log` completed successfully and wrote `StormBlocksUnity/Builds/VisualChecks/stormblocks-gameplay.png`.
- App Store screenshot capture: `/tmp/stormblocks-appstore-design-backdrop.log` completed successfully and regenerated all five 1170 x 2532 PNGs under `StormBlocksUnity/Builds/AppStoreScreens/`; the files were copied into `fastlane/screenshots/en-US/`.
- `Scripts/ci_static_checks.sh` passed after clearing Unity test-runner transient scenes.
- `Scripts/release_audit.sh local` passed with 32 pass, 0 fail, 0 open.
- `Scripts/release_audit.sh full` reported 33 pass, 0 fail, 6 open external gates.
- `Scripts/ios_release_gates.sh all-local` rebuilt the current-source Unity iOS project, signed Release-iphoneos app, installed it on the paired iPhone, created the Xcode archive, and exported the App Store Connect IPA on `2026-05-05`; the trailing status check required the updated 356-renderer baseline above.
- After the baseline update, `Scripts/ios_release_gates.sh status` passed against the fresh current-source IPA and `Scripts/ios_release_gates.sh launch-device` launched the fresh install on the paired iPhone at `2026-05-05 15:16:42Z`.
- `Scripts/ios_release_gates.sh upload-probe` retried against the fresh archive at `2026-05-05 15:16:59Z`; App Store Connect returned HTTP 200 with `data: []`, and Xcode failed at `IDEDistributionFetchAppRecordStep` with `missingApp(bundleId: "com.perlantir.stormblocks")`.
- `Scripts/fastlane_release.sh ios create_app_record` is still blocked until App Store Connect API key variables or `STORMBLOCKS_APPLE_ID`/`APPLE_ID` are set.

Known risks / not done:

- The optimized GLB path makes the assets usable, but final art quality still needs physical-device human review against the references.
- The raw design source folders are large; commit/runtime inclusion should stay limited to optimized mobile variants unless Git LFS source archival is explicitly needed.

## 2026-05-05 — Runtime board design pass 2

- Updated the actual playable `StormBlocksGameView` board again against the supplied references, not just marketing captures.
- Added separate frosted cell lips under the 8x8 tile tops, a warmer central camp floor, lighter readable storm tile colors, softened storm cell puffs, subtler storm-corner vortices, and rounded generated block meshes for placed/tray pieces.
- Fixed the optimized GLB tray charm so `RefreshTray` keeps it as a static tray child instead of deleting it during queue refresh.
- Regenerated the portrait visual capture and all five App Store screenshots after the board pass, then copied the refreshed 1170 x 2532 images into `fastlane/screenshots/en-US`.

Evidence:

- Visual capture: `/tmp/stormblocks-visual-board-pass-2b.log` completed successfully and wrote `StormBlocksUnity/Builds/VisualChecks/stormblocks-gameplay.png`.
- App Store screenshot capture: `/tmp/stormblocks-appstore-board-pass-2.log` completed successfully and regenerated all five PNGs under `StormBlocksUnity/Builds/AppStoreScreens/`; the files were copied into `fastlane/screenshots/en-US/`.
- EditMode tests: `StormBlocksUnity/editmode-results.xml` reports 25 total, 25 passed, 0 failed at `2026-05-05 15:31:36Z`.
- PlayMode tests: `StormBlocksUnity/playmode-results.xml` reports 7 total, 7 passed, 0 failed at `2026-05-05 15:31:44Z`.
- Current logged full-detail mobile baseline after the second runtime board pass: 423 renderers, 154,152 mesh triangles, 1 audio listener, and 1 canvas.
- `Scripts/ios_release_gates.sh all-local` rebuilt the current-source Unity iOS project, signed the Release-iphoneos app, installed it on the paired iPhone, created the Xcode archive, and exported the App Store Connect IPA on `2026-05-05`; the trailing status check required the updated 423-renderer baseline above.
- After the baseline update, `Scripts/ios_release_gates.sh launch-device` launched the fresh install on the paired iPhone at `2026-05-05 15:37:02Z`.
- `Scripts/ios_release_gates.sh upload-probe` retried against the fresh archive at `2026-05-05 15:37:05Z`; App Store Connect returned HTTP 200 with `data: []`, and Xcode failed at `IDEDistributionFetchAppRecordStep` with `missingApp(bundleId: "com.perlantir.stormblocks")`. Current distribution log: `/var/folders/b2/cl2rv8q13bg48zl073ctm_fc0000gq/T/Unity-iPhone_2026-05-05_10-37-02.871.xcdistributionlogs/IDEDistributionAppStoreConnect.log`.

Known risks / not done:

- The second board pass is a clear improvement and stays under automated scene budgets, but final art quality still needs human physical-device review against the supplied references.
- TestFlight upload still needs the App Store Connect app record for `com.perlantir.stormblocks`.

## 2026-05-05 — Physical profiling partial pass

- Launched the current-source signed app on the paired iPhone after the second board pass.
- Captured Game Performance and Power Profiler traces with `Scripts/device_qa_session.sh` using the hardware UDID resolved from `devicectl`.
- Exported trace tables with `xcrun xctrace export --toc` / `--xpath` to confirm the target and read key power/frame metrics.

Evidence:

- Device launch: `/tmp/stormblocks-device-launch.json` reports `"outcome": "success"` for `com.perlantir.stormblocks` on paired iPhone CoreDevice id `907E2EE7-9C7B-5D0D-9EC0-32E69912287D` at `2026-05-05 15:37:02Z`.
- Game Performance trace: `StormBlocksUnity/Builds/DeviceProfiles/stormblocks-game-performance-20260505T154140Z.trace`.
- Power Profiler trace: `StormBlocksUnity/Builds/DeviceProfiles/stormblocks-power-20260505T154242Z.trace`.
- Trace TOC identifies target device as `iPhone 17 Pro Max`, iOS `26.3 (23D127)`, hardware UDID `00008150-00040D203A88401C`, process `StormBlocks`.
- Exported Power Profiler layer metrics show GPU active time around `0.52 ms` on sampled 30-frame windows and frame interval around `33.3 ms`; `ProcessSubsystemPowerImpact` rows show zero Wi-Fi and cellular bytes during the sample.

Known risks / not done:

- This is a partial modern-device profiling pass, not final release performance signoff.
- Still need an older supported iPhone trace and longer manual interactive profiling through normal play, near-death storm, Storm Pushback, menus/results, and reduced-motion/Low Detail paths.

## 2026-05-05 — Runtime HUD polish pass

- Updated the actual runtime HUD with procedural score/rescue/best icon sprites, a warmer connected HUD underglow, and a pause-style menu control while preserving the tested `MENU Mode Button` object name.
- Kept the pass scoped to presentation; no gameplay rules or mode systems changed.
- Regenerated portrait gameplay and App Store screenshot captures after the HUD polish.

Evidence:

- Visual capture: `/tmp/stormblocks-visual-hud-polish-2.log` completed successfully and wrote `StormBlocksUnity/Builds/VisualChecks/stormblocks-gameplay.png`.
- App Store screenshot capture: `/tmp/stormblocks-appstore-hud-polish.log` completed successfully and regenerated all five PNGs under `StormBlocksUnity/Builds/AppStoreScreens/`; the files were copied into `fastlane/screenshots/en-US/`.
- EditMode tests: `StormBlocksUnity/editmode-results.xml` reports 25 total, 25 passed, 0 failed at `2026-05-05 16:03:53Z`.
- PlayMode tests: `StormBlocksUnity/playmode-results.xml` reports 7 total, 7 passed, 0 failed at `2026-05-05 16:04:01Z`.
- Current logged full-detail mobile baseline after the HUD polish pass: 432 renderers, 156,984 mesh triangles, 1 audio listener, and 1 canvas.
- `Scripts/ios_release_gates.sh all-local` refreshed Unity iOS export, signed build, paired-iPhone install, Xcode archive, and App Store IPA export for the current HUD-polish source; latest install evidence is `/tmp/stormblocks-device-install.json` from `2026-05-05 16:05:10Z`, and the exported IPA was refreshed at `2026-05-05 16:08:24Z`.
- `Scripts/ios_release_gates.sh launch-device` launched the current-source signed app on the paired iPhone at `2026-05-05 16:09:13Z`.
- `Scripts/ios_release_gates.sh upload-probe` retried against the current-source archive at `2026-05-05 16:09:18Z`; App Store Connect returned HTTP 200 with `data: []` and `total: 0` for `com.perlantir.stormblocks`. Current distribution log: `/var/folders/b2/cl2rv8q13bg48zl073ctm_fc0000gq/T/Unity-iPhone_2026-05-05_11-09-16.316.xcdistributionlogs/IDEDistributionAppStoreConnect.log`.

Known risks / not done:

- App Store Connect upload still requires an app record for bundle id `com.perlantir.stormblocks`; the upload probe is expected to stay blocked until that external record exists.

## 2026-05-05 — Text-free first-move coach pass

- Added a runtime visual first-move coach to `StormBlocksGameView`: a tray pulse, dotted drag path, moving fingertip, and valid target glow that appears before the first placement and clears as soon as the player drags or places.
- Kept the onboarding pass text-free so it teaches by doing and does not add hint/power-up UI.
- Added PlayMode smoke coverage proving the coach exists, contains no `Text` components, and dismisses after the first placement.

Evidence:

- EditMode tests: `StormBlocksUnity/editmode-results.xml` reports 25 total, 25 passed, 0 failed at `2026-05-05 16:28:39Z`.
- PlayMode tests: `StormBlocksUnity/playmode-results.xml` reports 8 total, 8 passed, 0 failed at `2026-05-05 16:28:46Z`.
- Current logged full-detail mobile baseline after the coach pass: 435 renderers, 159,892 mesh triangles, 1 audio listener, and 1 canvas.
- Visual capture: `/tmp/stormblocks-visual-first-move-coach-trimmed.log` completed successfully and wrote `StormBlocksUnity/Builds/VisualChecks/stormblocks-gameplay.png`.
- App Store screenshot capture: `/tmp/stormblocks-appstore-first-move-coach-trimmed.log` completed successfully and regenerated all five PNGs under `StormBlocksUnity/Builds/AppStoreScreens/`; the files were copied into `fastlane/screenshots/en-US/`.
- `Scripts/ios_release_gates.sh all-local` refreshed Unity iOS export, signed build, paired-iPhone install, Xcode archive, and App Store IPA export for the current coach source; latest install evidence is `/tmp/stormblocks-device-install.json` from `2026-05-05 16:29:59Z`, and the exported IPA was refreshed at `2026-05-05 16:33:30Z`.
- `Scripts/ios_release_gates.sh launch-device` was retried at `2026-05-05 16:34:18Z` but the paired iPhone was locked, so the exact-source launch retry remains open.
- `Scripts/ios_release_gates.sh upload-probe` retried against the current-source archive at `2026-05-05 16:34:37Z`; App Store Connect returned HTTP 200 with `data: []` and `total: 0` for `com.perlantir.stormblocks`. Current distribution log: `/var/folders/b2/cl2rv8q13bg48zl073ctm_fc0000gq/T/Unity-iPhone_2026-05-05_11-34-35.358.xcdistributionlogs/IDEDistributionAppStoreConnect.log`.

Known risks / not done:

- Unlock the paired iPhone and rerun `Scripts/ios_release_gates.sh launch-device` for current-source launch evidence.
- App Store Connect upload still requires an app record for bundle id `com.perlantir.stormblocks`; the upload probe is expected to stay blocked until that external record exists.
