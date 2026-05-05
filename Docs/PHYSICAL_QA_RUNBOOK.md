# Physical QA Runbook

Use this runbook with the paired iPhone available and awake. Game Center and TestFlight checks require the App Store Connect app record first; physical QA and profiling can be recorded before that. The automated release audit must stay green except for gates that this runbook is actively closing.

## Device And Build

Current paired device:

- iPhone 17 Pro Max, identifier `907E2EE7-9C7B-5D0D-9EC0-32E69912287D`.

Current signed app:

- `StormBlocksUnity/Builds/iOS/DerivedDataSignedTeam7JLDefault/Build/Products/Release-iphoneos/StormBlocks.app`
- Bundle id: `com.perlantir.stormblocks`
- Team id: `7JL22TDB44`

## Before Starting

1. Unlock the iPhone and keep it awake.
2. Confirm the installed app can launch:

```bash
Scripts/device_qa_session.sh launch
```

3. Confirm the full audit has no local failures:

```bash
Scripts/release_audit.sh full
```

The audit should still report open gates until device QA, performance, Game Center, and TestFlight are complete.

## Five-Run Playability Test

Run five sessions without changing code or assets between runs:

1. Endless Storm: normal run until game over.
2. Daily Storm: official run, confirm one clear daily objective.
3. Storm Trail: complete or fail one level, then return to map.
4. Tempest Trials: complete one weekly run.
5. Practice Mode: confirm lower-pressure play and no leaderboard pressure.

Record each run and score each category from 1 to 5 in `Docs/FIVE_RUN_QA_SCORECARD.md`, then summarize the final result in `Docs/QA_EVAL_REPORT.md`:

- Instant understandability.
- One-more-run pull.
- Board readability.
- Visual polish.
- Storm Pushback satisfaction.
- Touch feel.
- Performance feel.

Release target: 4 or higher in every category.

After filling the scorecard, run:

```bash
Scripts/verify_five_run_scorecard.sh
```

If the verifier fails, any category scores below 4, any required check fails, or any blocker/major defect is found, keep the physical-device QA release checklist item open and create or update a GitHub issue with the evidence.

## Functional Device Checks

Verify on the physical iPhone:

- Dragging pieces does not fight the notch, Dynamic Island, or home indicator.
- Tray pieces remain readable and tappable in portrait.
- Invalid placement feedback is clear but not noisy.
- Row and column clears are readable.
- Survivor rescue is visible and emotionally clear.
- Storm Pushback happens automatically when a clear intersects storm tiles.
- Near-death presentation is dramatic but does not hide playable cells.
- Game-over and Retry are immediate.
- Settings persist after app restart.
- Reduced Motion and Low Detail visibly reduce effects without breaking clarity.
- Share opens the iOS share sheet with text and an image.
- No unexpected permission prompts appear.

## Game Center Device Checks

After App Store Connect Game Center identifiers are live:

- Launch signed app and confirm Game Center authentication path.
- Submit one Endless Storm score.
- Submit one Daily Storm score.
- Submit one Tempest Trials weekly score.
- Complete First Rescue, First Pushback, and Clutch Save achievements.
- Open leaderboard UI from the profile screen.
- Open achievements UI from the achievements screen.

Use the identifiers in `Docs/APP_STORE_CONNECT_MANIFEST.json`.

## Performance Profiling

Run at least one modern iPhone pass and one older supported iPhone pass.

Start with the modern paired device:

```bash
Scripts/device_qa_session.sh profile-game
Scripts/device_qa_session.sh profile-power
```

The profiling helper uses the CoreDevice id for install/launch and automatically resolves the hardware UDID for `xctrace`. Override it with `STORMBLOCKS_XCTRACE_DEVICE_ID=<hardware-udid>` if Instruments lists a different identifier.

During each profile, play through:

- Normal placement and line clears.
- Survivor rescue.
- Large combo clear.
- Storm Pushback.
- Near-death state.
- Results/share flow.

Record the trace output paths and findings in `Docs/PERFORMANCE_PROFILE.md`.

## TestFlight Validation

After App Store Connect upload succeeds:

1. Install from TestFlight.
2. Launch from TestFlight.
3. Complete one short Endless Storm run.
4. Complete one Daily Storm run.
5. Confirm Game Center still authenticates.
6. Confirm no missing icon, splash, entitlement, or privacy prompt issue appears.

Only then check the TestFlight item in `Docs/RELEASE_DONE_CHECKLIST.md`.
