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
| Audio/haptics | Service interfaces and Unity feedback service exist; release checklist marks hooks present. | Pass |
| No forced ads, paid power, gacha, loot boxes, or paid speedups | Package/settings review in implementation log and privacy/app-store docs; Unity Ads/Purchasing disabled. | Pass |
| Design references and visual target | Procedural 3D board, storm, camp, survivors, app icon, and screenshots exist; visual quality still needs final physical-device human review. | Local pass; human review pending |
| QA tests | Latest EditMode: 25/25 at `2026-05-05 05:30:28Z`; latest PlayMode: 7/7 at `2026-05-05 05:30:53Z`. | Pass |
| Performance optimization | Primitive pooling, Low Detail fallback, URP mobile settings, and scene-budget guard exist; latest budget is 274 renderers and 59,232 triangles. | Local pass; physical profiling pending |
| Physical QA handoff | `Docs/PHYSICAL_QA_RUNBOOK.md` and `Scripts/device_qa_session.sh` define launch, five-run QA, Game Center, TestFlight, and profiling steps. | Local pass; physical execution pending |
| iOS unsigned build | `/tmp/stormblocks-xcode-lowdetail-pool-unsigned.log` reports `** BUILD SUCCEEDED **`. | Pass |
| iOS signed build | `/tmp/stormblocks-xcode-team7jl-default-signed.log` reports `** BUILD SUCCEEDED **`; team `7JL22TDB44`, Game Center entitlement. | Pass |
| Physical-device install | `/tmp/stormblocks-device-install.json` reports success for `com.perlantir.stormblocks` on paired iPhone `907E2EE7-9C7B-5D0D-9EC0-32E69912287D`. | Pass |
| Physical-device launch | `/tmp/stormblocks-device-launch.json` failed only because the iPhone was locked. | Blocked by locked device |
| Xcode archive | `/tmp/stormblocks-xcode-team7jl-archive.log` reports `** ARCHIVE SUCCEEDED **`. | Pass |
| App Store Connect IPA export | `/tmp/stormblocks-xcode-team7jl-export-appstore.log` reports `** EXPORT SUCCEEDED **`; IPA at `StormBlocksUnity/Builds/iOS/ExportAppStoreTeam7JL/StormBlocks.ipa`. | Pass |
| App Store metadata package | `fastlane/metadata/en-US/`, `fastlane/screenshots/en-US/`, and `Docs/APP_STORE_CONNECT_MANIFEST.json` are verified by `Scripts/verify_release_assets.sh`. | Local pass; public URL review pending |
| TestFlight upload | Upload probe authenticated to App Store Connect, then failed at `IDEDistributionFetchAppRecordStep` with `missingApp(bundleId: "com.perlantir.stormblocks")`. | Blocked by missing app record |

## Open Release Gates

- Create the App Store Connect app record for bundle id `com.perlantir.stormblocks` under team `7JL22TDB44` / `UBER KIWI LLC`.
- Create and enable the Game Center leaderboard and achievement identifiers in `Docs/GAME_CENTER_SETUP.md`.
- Unlock the paired iPhone and rerun `Scripts/ios_release_gates.sh launch-device`.
- Validate Game Center authentication, leaderboard submission, achievement reporting, and Game Center UI on a signed physical device.
- Complete physical-device QA on the notched/Dynamic Island iPhone that already installed the app.
- Complete physical performance and thermal profiling on one modern and one older supported iPhone.
- Upload the exported IPA to TestFlight after the App Store Connect app record exists.
- Install and launch the TestFlight build.
- Run the human five-run playability test.
- Provide App Store Connect API key credentials or Apple ID app-specific password if using the Fastlane lanes.
- Grant desktop automation permission or drive the App Store Connect browser/Xcode UI manually; the current desktop automation channel reports `Sender process is not authenticated`.

## Reproduction Commands

Use the release gate runner for cached proof and repeatable local gates:

```bash
Scripts/release_audit.sh local
Scripts/release_audit.sh full
Scripts/ios_release_gates.sh status
Scripts/ios_release_gates.sh launch-device
Scripts/ios_release_gates.sh upload-probe
Scripts/device_qa_session.sh plan
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

Storm Blocks is locally release-candidate-ready through signed build, physical-device install, archive, and App Store Connect IPA export. It is not final-release complete because the App Store Connect app record, live Game Center configuration, TestFlight validation, unlocked-device launch, and physical QA/performance gates are still open.
