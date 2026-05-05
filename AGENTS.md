# AGENTS.md — Codex Instructions

## Read order

Before coding, read these files in order:

1. `GOAL.md`
2. `Docs/GAME_DESIGN_SPEC.md`
3. `Docs/ART_DIRECTION.md`
4. `Docs/TECH_SPEC.md`
5. `Docs/FULL_GAME_MILESTONES.md`
6. `Docs/ACCEPTANCE_CRITERIA.md`
7. `Docs/RELEASE_DONE_CHECKLIST.md`
8. `Docs/EVALS.md`
9. `Docs/IMPLEMENTATION_LOG.md`

## Product instruction

Build the full game, not a prototype. Milestones are implementation gates, not the final scope.

## Engineering rules

- Use Unity/C#.
- Target iPhone portrait-first gameplay.
- Use clean architecture with gameplay logic testable outside MonoBehaviours.
- Keep domain logic in plain C# where possible.
- Use MonoBehaviours for presentation, scene wiring, input, camera, UI, and effects.
- Use explicit service interfaces for Game Center, analytics, cloud save, remote config, audio, haptics, and sharing.
- Use deterministic RNG for Daily Storm, Storm Trail seeds, piece queues, storm spread, and score validation.
- Add tests for deterministic logic.
- Use data-driven configs for scoring, storm tuning, pieces, levels, cosmetics, and daily modifiers.
- Keep the project compiling after each meaningful change.
- Update `Docs/IMPLEMENTATION_LOG.md` after meaningful changes.

## Visual quality rules

- Match `/DesignReferences` and `Docs/ART_DIRECTION.md`.
- Implement a polished 3D board, 3D pieces, warm camp, storm VFX, survivor characters, juice effects, UI transitions, haptics, and sound hooks.
- Do not leave the game looking like default Unity UI.
- Placeholder assets are allowed only while building; final release candidate must include cohesive production-style placeholders or generated assets that match the art direction.
- Use efficient mobile-friendly stylized 3D. Avoid expensive photorealism.

## Gameplay identity rules

- Storm Pushback is automatic when a clear intersects storm tiles.
- Storm Pushback is the game’s signature mechanic.
- The board must always stay readable.
- The first 10 seconds must teach by doing, not by text.
- Losing should make the player want to instantly retry.
- Daily Storm is the core retention/social mode.

## Monetization rules

The launch build must not contain forced ads, paid power, loot boxes, gacha, paid speedups, or pay-to-win systems.

Cosmetics and optional support features may be scaffolded but should not distract from free-first gameplay.

## Reporting format

After each major pass, report:

- What changed.
- Files changed.
- Tests/checks run.
- What still fails or is risky.
- Current milestone status.
- Next milestone.

Never declare final completion until `Docs/RELEASE_DONE_CHECKLIST.md` is satisfied.
