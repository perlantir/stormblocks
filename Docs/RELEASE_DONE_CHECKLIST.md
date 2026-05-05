# Release Done Checklist

Do not call the project done until this passes.

## Gameplay

- [x] Endless Storm is complete.
- [x] Daily Storm is complete.
- [x] Storm Trail has 120+ levels/challenges.
- [x] Tempest Trials is complete.
- [x] Practice/Chill Mode is complete.
- [x] Storm Pushback is automatic, clear, and satisfying.
- [x] Near-death state works.
- [x] Clutch save works.
- [x] Game-over and instant restart work.
- [x] Scoring feels rewarding and understandable.

## Visuals

- [x] 3D board, pieces, storm, camp, and survivors are cohesive.
- [x] UI matches Premium Casual Toy-Puzzle Storm Rescue direction.
- [x] Storm Pushback has signature VFX.
- [x] Near-death has cinematic but readable presentation.
- [x] App icon draft exists.
- [x] App Store screenshot scenes exist.

## Systems

- [x] Local save/profile works.
- [x] Cosmetic unlock/equip works.
- [x] Daily seed is deterministic.
- [x] Local leaderboard/history works.
- [x] Game Center interfaces exist and mocks work.
- [x] Analytics event interfaces exist.
- [x] Remote config interface exists and mock coverage works.
- [x] Cloud save interface exists and mock coverage works.
- [x] Sharing interface exists with native iOS bridge and editor fallback.
- [x] Dedicated Accessibility and Credits screens exist in the UI shell.
- [x] Low Detail mode exists, persists locally, and auto-falls back on constrained iOS hardware.
- [x] Audio service works.
- [x] Haptic service works.

## QA

- [x] Core logic tests pass.
- [x] Daily determinism tests pass.
- [x] Save/load tests pass.
- [x] Play-mode smoke tests pass.
- [x] No console errors in normal flow under automated PlayMode smoke coverage.
- [x] No critical touch/UI issues on portrait safe areas under automated PlayMode safe-area coverage.
- [x] Performance profile documented.
- [x] Dynamic presentation pooling exists for board, tray, drag ghost, survivor, storm, and pushback VFX objects.
- [ ] Physical-device QA pass completed on at least one notched/Dynamic Island iPhone.
- [ ] Physical-device performance pass completed on at least one modern and one older supported iPhone.

## iOS release readiness

- [x] Portrait orientation locked.
- [x] Safe areas respected.
- [x] Xcode/iOS build steps documented.
- [x] App Store metadata draft exists.
- [x] Privacy/permissions reviewed.
- [x] No forced ads/pay-to-win/loot boxes present.
- [x] iOS Simulator export/run path works for local runtime checks.
- [x] Unsigned `Release-iphoneos` Xcode build succeeds.
- [x] Signed Xcode device build succeeds with the App Store Connect team and provisioning profile.
- [x] App Store Connect IPA export succeeds.
- [x] Signed build installs on a paired physical iPhone.
- [ ] App Store Connect app record exists for `com.perlantir.stormblocks`.
- [ ] Game Center leaderboard/achievement identifiers are live in App Store Connect and validated on device.
- [ ] TestFlight upload and install are validated.
