# QA Eval Report

Current eval date: 2026-05-05.

## Automated Coverage

- EditMode: 26 total, 26 passed, 0 failed at `2026-05-05 17:52:40Z`.
- PlayMode: 10 total, 10 passed, 0 failed at `2026-05-05 17:52:47Z`.
- Covered core evals: placement, valid move detection, line clears, scoring, storm spread, automatic Storm Pushback, saved-survivor presentation, signature pushback VFX, reduced-motion/Low Detail VFX trimming, clutch save, game-over states, daily seed determinism, save/load, progression, service seams, visual first-move coaching, normal-flow console cleanliness, portrait safe-area controls, results/retry, UI shell navigation, and mobile scene budgets.
- Current full-detail PlayMode budget: 448 renderers, 163,432 mesh triangles, 1 audio listener, 1 canvas.
- Local Gate 12 performance coverage now includes primitive pooling for dynamic board/tray/drag/VFX presentation, a persisted Low Detail setting with older-device auto fallback heuristics, and PlayMode proof that Reduced Motion plus Low Detail trims nonessential pushback effects without hiding saved/pushback feedback.

## Build And Runtime Proof

- Unity iOS Simulator export succeeded: `/tmp/stormblocks-ios-simulator-lowdetail-pool.log`.
- XcodeBuildMCP built, installed, launched, and captured a simulator screenshot for `com.perlantir.stormblocks` on booted iPhone 16 Pro simulator `BFD7E422-B789-4380-9588-B952559B6A92`.
- Current-source Unity iOS device export succeeded with team `7JL22TDB44` at `2026-05-05 17:33:27Z`: `/tmp/stormblocks-ios-device-team7jl.log`.
- Unsigned `Release-iphoneos` Xcode build succeeded: `/tmp/stormblocks-xcode-lowdetail-pool-unsigned.log`.
- Current-source signed `Release-iphoneos` Xcode build succeeded under team `7JL22TDB44` at `2026-05-05 17:34:39Z`: `/tmp/stormblocks-xcode-team7jl-default-signed.log`.
- Latest current-source physical-device install retry at `2026-05-05 17:56:08Z` on paired iPhone `907E2EE7-9C7B-5D0D-9EC0-32E69912287D` succeeded: `/tmp/stormblocks-device-install.json`.
- Latest current-source physical-device launch retry at `2026-05-05 17:56:17Z` on paired iPhone `907E2EE7-9C7B-5D0D-9EC0-32E69912287D` succeeded for `com.perlantir.stormblocks`: `/tmp/stormblocks-device-launch.json`.
- Current-source Xcode archive succeeded at `2026-05-05 17:37:18Z`: `/tmp/stormblocks-xcode-team7jl-archive.log`.
- Current-source App Store Connect IPA export succeeded at `2026-05-05 17:38:00Z`: `/tmp/stormblocks-xcode-team7jl-export-appstore.log`, output `StormBlocksUnity/Builds/iOS/ExportAppStoreTeam7JL/StormBlocks.ipa`.
- Current-source App Store Connect upload probe at `2026-05-05 17:55:42Z` authenticated but found no app record for bundle id `com.perlantir.stormblocks`: `/tmp/stormblocks-xcode-team7jl-upload-appstore.log`.
- Physical Game Performance traces captured on paired iPhone 17 Pro Max: `StormBlocksUnity/Builds/DeviceProfiles/stormblocks-game-performance-20260505T154140Z.trace`, `StormBlocksUnity/Builds/DeviceProfiles/stormblocks-game-performance-20260505T180036Z.trace`, and `StormBlocksUnity/Builds/DeviceProfiles/stormblocks-game-performance-20260505T181049Z.trace`. The latest retry attached to the already-running app, exported a 10-second windowed Game Performance TOC with `end-reason` = `Time limit reached`, and left `StormBlocks` running on device; it still does not replace the required longer interactive profile.
- Physical Power Profiler traces captured on paired iPhone 17 Pro Max: `StormBlocksUnity/Builds/DeviceProfiles/stormblocks-power-20260505T154242Z.trace` and `StormBlocksUnity/Builds/DeviceProfiles/stormblocks-power-20260505T180251Z.trace`. The latest 60.86-second trace reached its time limit and exported nominal thermal state, average GPU Active Time `0.549 ms`, and average Frame Interval `33.620 ms` across 1,819 sampled frames.

## Playability Eval Status

These are not final human scores. They are current local QA estimates from automated coverage, generated captures, and simulator launch:

- Instant understandability: 4/5 local estimate; first-run flow is visual and menu-free once gameplay starts, with a text-free drag coach covered in PlayMode.
- One-more-run pull: pending human five-run test.
- Board readability: 4/5 local estimate; automated safe-area and mobile budget checks pass.
- Visual polish: 4/5 local estimate for cohesive procedural launch art after the reference-led board pass, optimized GLB source import, design-source storm backdrop crop, deeper 8x8 grid seams, warmer camp sanctuary ring, and living perimeter storm wall; final device visual review is still required.
- Storm Pushback satisfaction: 4/5 local estimate; automatic mechanic, VFX, score feedback, audio, and haptic hooks are present.
- Touch feel: pending signed-device test.
- Performance: partial physical-device profiling captured on a modern iPhone; structural scene budget is under guardrails and includes a Low Detail fallback path. Older-device and longer interactive profiling remain pending.

## Required Manual Gates

- Five-run addiction test with a human tester.
- Physical-device QA on a notched/Dynamic Island iPhone.
- Physical-device performance and thermal profile on one older supported iPhone, plus longer interactive review of captured modern-device traces.
- Signed Game Center authentication, leaderboard submission, achievement reporting, and Game Center UI validation.
- App Store Connect app record creation for `com.perlantir.stormblocks`.
- TestFlight upload, install, and launch validation.

Execution runbook:

- `Docs/PHYSICAL_QA_RUNBOOK.md`
- `Docs/FIVE_RUN_QA_SCORECARD.md`
- `Scripts/device_qa_session.sh plan`
