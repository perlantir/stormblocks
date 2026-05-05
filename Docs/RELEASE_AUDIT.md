# Release Audit

Current audit date: 2026-05-05.

This maps the launch request and repo release gates to concrete evidence. The project must not be called final until every open item below is closed with real proof.

## Prompt-to-Artifact Checklist

| Requirement | Current evidence | Status |
| --- | --- | --- |
| Unity/C# iPhone game, portrait-first | Unity project under `StormBlocksUnity`; iOS export uses portrait-only `Info.plist`; `Docs/BUILD_AND_TEST.md` has the latest export proof. | Pass |
| Complete 8x8 drag-and-place block puzzle | Core logic and PlayMode interaction tests pass; `Docs/QA_EVAL_REPORT.md` lists placement, valid moves, clears, and touch-control coverage. | Pass |
| Storm Pushback is automatic and signature | Core tests cover pushback and clutch save; presentation includes gold/cyan pushback VFX; PlayMode and captures exercise the playable scene. | Pass |
| Endless Storm | Release checklist marks complete; local high-score/best-run coverage exists in profile/progression tests. | Pass |
| Daily Storm | Deterministic daily seed, local history, leaderboard-ready service, and share-card paths are covered by EditMode/PlayMode evidence. | Pass |
| Storm Trail progression | 120+ deterministic level/challenge definitions and progression/reward tests are covered in the mode/progression implementation. | Pass |
| Tempest Trials weekly mode | Weekly seed/rules, playlist, scoring, and badge progression are implemented and covered by mode/progression tests. | Pass |
| Practice/Chill Mode | Mode exists in runtime shell and profile mode flow; release checklist marks complete. | Pass |
| Local profile/save | Save/load roundtrip and profile codec tests pass. | Pass |
| Cosmetic-only progression | Cosmetic unlock/equip/profile progression exists and is release-checklisted; no paid power systems are present. | Pass |
| Achievements | Local achievements and Game Center-ready achievement interface exist; PlayMode verifies entry points. | Local pass; live Game Center IDs pending |
| Game Center-ready interfaces | `UnityGameCenterServices.cs`, `StormBlocksGameKitBridge.mm`, linked `GameKit.framework`, and exported Game Center entitlements exist. | Local pass; live validation pending |
| Accessibility/settings | Dedicated Accessibility screen exists; Low Detail and reduced-motion style settings persist locally. | Pass |
| Audio/haptics | Service interfaces, Unity feedback service, and EditMode coverage for rescue/near-death hooks exist; release checklist marks hooks present. | Pass |
| No forced ads, paid power, gacha, loot boxes, or paid speedups | Package/settings review in implementation log and privacy/app-store docs; Unity Ads/Purchasing disabled. | Pass |
| Design references and visual target | Procedural 3D board, storm, camp, survivors, app icon, regenerated screenshots, optimized design-source GLB imports, blurred design-source storm backdrop, deep grid seam lattice, warm camp sanctuary ring, and living perimeter storm wall exist; visual quality still needs final physical-device human review. | Local pass; human review pending |
| QA tests | Latest EditMode: 26/26 at `2026-05-05 17:03:53Z`; latest PlayMode: 8/8 at `2026-05-05 17:04:00Z`. | Pass |
| Prompt compliance verifier | `Scripts/verify_prompt_compliance.sh` checks required docs/design refs, major gameplay/system surfaces, passing test result files, and non-monetization/copyright guardrails. | Pass |
| Performance optimization | Primitive pooling, Low Detail fallback, URP mobile settings, optimized design-source imports, scene-budget guard, and modern-iPhone Game Performance/Power traces exist; latest full-detail budget is 442 renderers and 161,544 triangles. | Partial physical pass; older-device profiling pending |
| Physical QA handoff | `Docs/PHYSICAL_QA_RUNBOOK.md` and `Scripts/device_qa_session.sh` define launch, five-run QA, Game Center, TestFlight, and profiling steps. | Local pass; physical execution pending |
| GitHub static verification | `Release Static Checks` passed for runtime feedback commit `873db6d`; run `25390174472`: `https://github.com/perlantir/stormblocks/actions/runs/25390174472`. Branch-head status should be checked after any follow-up docs-only commit. | Pass |
| iOS unsigned build | `/tmp/stormblocks-xcode-lowdetail-pool-unsigned.log` reports `** BUILD SUCCEEDED **`. | Pass |
| iOS signed build | Current-source `/tmp/stormblocks-xcode-team7jl-default-signed.log` reports `** BUILD SUCCEEDED **`; team `7JL22TDB44`, Game Center entitlement. | Pass |
| Physical-device install | Current-source `/tmp/stormblocks-device-install.json` reports `"outcome" : "failed"` at `2026-05-05 17:05:00Z` because CoreDevice cannot locate the paired iPhone identifier `907E2EE7-9C7B-5D0D-9EC0-32E69912287D`; `devicectl list devices` currently marks that iPhone unavailable. | Open; CoreDevice device unavailable |
| Physical-device launch | Current-source `Scripts/ios_release_gates.sh launch-device` was retried at `2026-05-05 17:10:00Z`; `/tmp/stormblocks-device-launch.json` reports `"outcome" : "failed"` for the same CoreDevice device-availability reason. | Open; CoreDevice device unavailable |
| Xcode archive | Current-source `/tmp/stormblocks-xcode-team7jl-archive.log` reports `** ARCHIVE SUCCEEDED **`. | Pass |
| App Store Connect IPA export | Current-source `/tmp/stormblocks-xcode-team7jl-export-appstore.log` reports `** EXPORT SUCCEEDED **`; IPA at `StormBlocksUnity/Builds/iOS/ExportAppStoreTeam7JL/StormBlocks.ipa`. | Pass |
| App Store metadata package | `fastlane/metadata/en-US/`, `fastlane/screenshots/en-US/`, and `Docs/APP_STORE_CONNECT_MANIFEST.json` are verified by `Scripts/verify_release_assets.sh`. | Local pass; public URL review pending |
| TestFlight upload | Current-source upload probe authenticated to App Store Connect at `2026-05-05 17:09:43Z`; Apple returned HTTP 200 with `data: []` and `total: 0` for `filter[bundleId]=com.perlantir.stormblocks`, then Xcode failed with `missingApp(bundleId: "com.perlantir.stormblocks")` because no app record exists. | Blocked by missing app record |

## Open Release Gates

- Create the App Store Connect app record for bundle id `com.perlantir.stormblocks` under team `7JL22TDB44` / `UBER KIWI LLC`: https://github.com/perlantir/stormblocks/issues/1
- Create and enable the Game Center leaderboard and achievement identifiers in `Docs/GAME_CENTER_SETUP.md`, then validate them on device: https://github.com/perlantir/stormblocks/issues/2
- Reconnect/unlock the paired iPhone so CoreDevice marks `907E2EE7-9C7B-5D0D-9EC0-32E69912287D` available, then rerun `Scripts/ios_release_gates.sh install-device` and `Scripts/ios_release_gates.sh launch-device` for the current source build.
- Complete physical-device QA plus the human five-run playability test: https://github.com/perlantir/stormblocks/issues/9
- Complete physical performance and thermal profiling on one modern and one older supported iPhone: https://github.com/perlantir/stormblocks/issues/8
- Modern-iPhone Game Performance and Power traces are captured; older-device and longer interactive trace review remain open.
- Upload the exported IPA to TestFlight after the App Store Connect app record exists, then install and launch the TestFlight build: https://github.com/perlantir/stormblocks/issues/7
- Provide App Store Connect API key credentials or Apple ID app-specific password if using the Fastlane lanes.
- Grant desktop automation permission or drive the App Store Connect browser/Xcode UI manually; the current desktop automation channel reports `Sender process is not authenticated`.

GitHub tracking milestone: https://github.com/perlantir/stormblocks/milestone/1

## Reproduction Commands

Use the release gate runner for cached proof and repeatable local gates:

```bash
Scripts/release_audit.sh local
Scripts/release_audit.sh full
Scripts/ios_release_gates.sh status
Scripts/ios_release_gates.sh launch-device
Scripts/ios_release_gates.sh upload-probe
Scripts/device_qa_session.sh plan
Scripts/ci_static_checks.sh
Scripts/verify_prompt_compliance.sh
```

Credentialed Fastlane lanes are available after App Store Connect credentials are set:

```bash
Scripts/verify_release_assets.sh
Scripts/fastlane_release.sh ios create_app_record
Scripts/fastlane_release.sh ios upload_metadata
Scripts/fastlane_release.sh ios upload_testflight
Scripts/fastlane_release.sh ios release_candidate_upload
```

After the App Store Connect app record exists, rerun:

```bash
Scripts/ios_release_gates.sh upload-probe
```

If local build artifacts have been cleaned, regenerate local release evidence:

```bash
Scripts/ios_release_gates.sh all-local
```

## Current Conclusion

Storm Blocks is locally release-candidate-ready through tests, signed build, archive, and App Store Connect IPA export. It is not final-release complete because the current exact-source physical install/launch retry is blocked by CoreDevice device availability, and the App Store Connect app record, live Game Center configuration, TestFlight validation, and physical QA/performance gates are still open.
