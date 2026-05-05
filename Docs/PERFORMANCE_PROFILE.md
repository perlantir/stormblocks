# Performance Profile

This is the current lightweight performance profile for the Unity playable scene. It is not a replacement for physical-device profiling, but it gives a repeatable guardrail for mobile-scene complexity while the project moves toward release candidate.

## Current Baseline

Source: `StormBlocksUnity/playmode-results.xml` from `2026-05-05 13:59:12Z`.

- PlayMode tests: 7 total, 7 passed, 0 failed.
- Playable root renderers: 421.
- Playable root mesh triangles: 142,792.
- Audio listeners: 1.
- Canvases: 1.

## Runtime Optimization Pass

- Dynamic cube/sphere presentation objects are pooled for board refreshes, tray rebuilds, drag ghosts, survivor bodies, storm cells, and Storm Pushback VFX.
- Low Detail mode is available from Accessibility settings and persisted in the local profile.
- Physical iOS devices automatically fall back to Low Detail on constrained hardware heuristics: <= 4096 MB system memory, <= 1536 MB graphics memory, or <= 4 processors.
- Low Detail keeps the signature gold Storm Pushback wave and core board readability while trimming duplicate wave accents, rain, extra puffs, lightning shatter accents, and block highlight dots.

## Automated Guardrail

Test: `PlayableViewStaysWithinMobileSceneBudgets`

Current guardrails:

- Renderers must be <= 450.
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
