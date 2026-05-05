# Performance Profile

This is the current lightweight performance profile for the Unity playable scene. It is not a replacement for physical-device profiling, but it gives a repeatable guardrail for mobile-scene complexity while the project moves toward release candidate.

## Current Baseline

Source: `StormBlocksUnity/playmode-results.xml` from `2026-05-05 17:22:23Z`.

- PlayMode tests: 9 total, 9 passed, 0 failed.
- Playable root renderers: 457.
- Playable root mesh triangles: 166,264.
- Audio listeners: 1.
- Canvases: 1.

## Runtime Optimization Pass

- Dynamic cube/sphere presentation objects are pooled for board refreshes, tray rebuilds, drag ghosts, survivor bodies, storm cells, and Storm Pushback VFX.
- Low Detail mode is available from Accessibility settings and persisted in the local profile.
- Physical iOS devices automatically fall back to Low Detail on constrained hardware heuristics: <= 4096 MB system memory, <= 1536 MB graphics memory, or <= 4 processors.
- Low Detail keeps the signature gold Storm Pushback wave and core board readability while trimming duplicate wave accents, rain, extra puffs, lightning shatter accents, and block highlight dots.
- Raw design GLBs are converted through `Scripts/optimize_design_glbs.sh`; only curated mobile variants are imported into Unity, and only the toy-block charm is currently loaded through `Resources` at runtime. A blurred design-source JPG crop is loaded as the storm backdrop through `Resources/StormSky`. The board itself now uses one mesh for the deep grid seam lattice plus low-count perimeter storm wall geometry, keeping the visual upgrade inside the mobile guardrail.

## Automated Guardrail

Test: `PlayableViewStaysWithinMobileSceneBudgets`

Current guardrails:

- Renderers must be <= 475.
- Mesh triangles must be <= 250,000.
- Audio listeners must equal 1.
- Canvases must be <= 1.

The thresholds are intentionally above the current baseline to allow visual polish while catching accidental scene multiplication, runaway VFX objects, duplicate canvases, or duplicate audio listeners.

## Physical-Device Follow-Up

Before TestFlight/release signoff:

- Run on at least one modern iPhone and one older supported iPhone.
- Capture FPS, frame time, memory, thermal behavior, and input latency during normal play, near-death storm, large combo clears, Storm Pushback, and results/share flow.
- Confirm reduced-motion mode cuts heavy VFX timing without harming readability.
- Confirm Low Detail mode maintains readability and reduces visual object pressure on older supported iPhones.
- Confirm no sustained frame drops during Daily Storm and Tempest Trials.

Profiling helpers:

```bash
Scripts/device_qa_session.sh profile-game
Scripts/device_qa_session.sh profile-power
```

Trace outputs are written under ignored path `StormBlocksUnity/Builds/DeviceProfiles/`.

Latest profiling attempt:

- `Scripts/device_qa_session.sh` now resolves the xctrace hardware UDID from `devicectl` instead of passing the CoreDevice identifier.
- Earlier signed app launched on paired iPhone CoreDevice id `907E2EE7-9C7B-5D0D-9EC0-32E69912287D` at `2026-05-05 15:37:02Z`; the latest current-source install/launch retry is blocked because CoreDevice currently marks that device unavailable.
- `STORMBLOCKS_PROFILE_TIME=15s Scripts/device_qa_session.sh profile-game` captured `StormBlocksUnity/Builds/DeviceProfiles/stormblocks-game-performance-20260505T154140Z.trace` on hardware UDID `00008150-00040D203A88401C`.
- `STORMBLOCKS_PROFILE_TIME=15s Scripts/device_qa_session.sh profile-power` captured `StormBlocksUnity/Builds/DeviceProfiles/stormblocks-power-20260505T154242Z.trace` on the same paired iPhone.
- `xcrun xctrace export --toc` reports target device `iPhone 17 Pro Max`, iOS `26.3 (23D127)`, process `StormBlocks`, and normal time-limit completion for both traces.
- Exported Power Profiler layer metrics include GPU active time around `0.52 ms` on sampled 30-frame windows and frame interval around `33.3 ms`; `ProcessSubsystemPowerImpact` rows show no Wi-Fi or cellular bytes during the sample.

Remaining physical-performance gates:

- Repeat profiling on one older supported iPhone.
- Capture a longer interactive play session covering large clears, near-death storm, Storm Pushback, menus/results, and reduced-motion/Low Detail toggles.
- Review traces manually in Instruments for thermal trend, frame pacing, memory, and sustained input latency before checking the release done box.
