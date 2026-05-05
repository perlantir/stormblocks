# QA Eval Report

Current eval date: 2026-05-05.

## Automated Coverage

- EditMode: 25 total, 25 passed, 0 failed at `2026-05-05 15:00:45Z`.
- PlayMode: 7 total, 7 passed, 0 failed at `2026-05-05 15:00:52Z`.
- Covered core evals: placement, valid move detection, line clears, scoring, storm spread, automatic Storm Pushback, clutch save, game-over states, daily seed determinism, save/load, progression, service seams, normal-flow console cleanliness, portrait safe-area controls, results/retry, UI shell navigation, and mobile scene budgets.
- Current full-detail PlayMode budget: 365 renderers, 136,308 mesh triangles, 1 audio listener, 1 canvas.
- Local Gate 12 performance coverage now includes primitive pooling for dynamic board/tray/drag/VFX presentation and a persisted Low Detail setting with older-device auto fallback heuristics.

## Build And Runtime Proof

- Unity iOS Simulator export succeeded: `/tmp/stormblocks-ios-simulator-lowdetail-pool.log`.
- XcodeBuildMCP built, installed, launched, and captured a simulator screenshot for `com.perlantir.stormblocks` on booted iPhone 16 Pro simulator `BFD7E422-B789-4380-9588-B952559B6A92`.
- Unity iOS device export succeeded with team `7JL22TDB44`: `/tmp/stormblocks-ios-device-team7jl.log`.
- Unsigned `Release-iphoneos` Xcode build succeeded: `/tmp/stormblocks-xcode-lowdetail-pool-unsigned.log`.
- Signed `Release-iphoneos` Xcode build succeeded under team `7JL22TDB44`: `/tmp/stormblocks-xcode-team7jl-default-signed.log`.
- Signed app installed on paired iPhone `907E2EE7-9C7B-5D0D-9EC0-32E69912287D`: `/tmp/stormblocks-device-install.json`.
- Physical-device launch succeeded on paired iPhone `907E2EE7-9C7B-5D0D-9EC0-32E69912287D`: `/tmp/stormblocks-device-launch.json`.
- Xcode archive succeeded: `/tmp/stormblocks-xcode-team7jl-archive.log`.
- App Store Connect IPA export succeeded: `/tmp/stormblocks-xcode-team7jl-export-appstore.log`, output `StormBlocksUnity/Builds/iOS/ExportAppStoreTeam7JL/StormBlocks.ipa`.
- App Store Connect upload probe authenticated but found no app record for bundle id `com.perlantir.stormblocks`: `/tmp/stormblocks-xcode-team7jl-upload-appstore.log`.

## Playability Eval Status

These are not final human scores. They are current local QA estimates from automated coverage, generated captures, and simulator launch:

- Instant understandability: 4/5 local estimate; first-run flow is visual and menu-free once gameplay starts.
- One-more-run pull: pending human five-run test.
- Board readability: 4/5 local estimate; automated safe-area and mobile budget checks pass.
- Visual polish: 4/5 local estimate for cohesive procedural launch art after the reference-led board pass, optimized GLB source import, and design-source storm backdrop crop; final device visual review is still required.
- Storm Pushback satisfaction: 4/5 local estimate; automatic mechanic, VFX, score feedback, audio, and haptic hooks are present.
- Touch feel: pending signed-device test.
- Performance: pending physical-device profiling; structural scene budget is under guardrails and now includes a Low Detail fallback path.

## Required Manual Gates

- Five-run addiction test with a human tester.
- Physical-device QA on a notched/Dynamic Island iPhone.
- Physical-device performance and thermal profile on one modern and one older supported iPhone.
- Signed Game Center authentication, leaderboard submission, achievement reporting, and Game Center UI validation.
- App Store Connect app record creation for `com.perlantir.stormblocks`.
- TestFlight upload, install, and launch validation.

Execution runbook:

- `Docs/PHYSICAL_QA_RUNBOOK.md`
- `Scripts/device_qa_session.sh plan`
