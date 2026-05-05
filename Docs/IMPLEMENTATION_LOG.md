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
- The App Store Connect app record for `com.perlantir.stormblocks` is still missing until `fastlane ios create_app_record` or the App Store Connect UI creates it.

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
