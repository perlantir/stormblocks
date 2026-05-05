# Full Game Milestones

These are not MVP phases. They are production gates toward the full launch game.

## Gate 0 — Repo and Unity production setup

Create Unity project, URP, package setup, folder structure, tests, scenes, initial build settings.

Done when:

- Project opens.
- Portrait scene loads.
- Build/test commands documented.
- Core assembly definitions created.
- Edit-mode test assembly created.

## Gate 1 — Complete core puzzle engine

Build all plain C# gameplay logic.

Done when:

- Board, pieces, clears, storm, pushback, scoring, game-over, and deterministic seed systems work.
- Full test coverage exists for core rules.

## Gate 2 — 3D gameplay board production foundation

Create the playable 3D board and drag controls.

Done when:

- 8x8 board displays with 3D tiles.
- Pieces are draggable and placeable.
- Core logic drives presentation.
- Portrait camera is polished.
- Touch feels responsive.

## Gate 3 — Signature Storm Pushback experience

Make the signature mechanic feel spectacular.

Done when:

- Automatic pushback resolves correctly.
- Line glow, storm shatter, survivor rescue, score pop, haptic/audio hooks, and camera pulse work.
- Clear differences between normal clear, combo, pushback, and clutch save.

## Gate 4 — Endless Storm full mode

Complete Endless Storm.

Done when:

- Difficulty ramp works.
- Calm/strategic/panic phases work.
- Game over works.
- Results screen works.
- Instant restart works.
- Local bests work.

## Gate 5 — Daily Storm full mode

Complete deterministic daily challenge.

Done when:

- Daily seed is deterministic.
- Daily run state is tracked.
- Local daily history works.
- Daily result screen works.
- Leaderboard service interface is wired with mock implementation.
- Share-card data object exists.

## Gate 6 — Storm Trail full progression

Build 120+ level journey structure.

Done when:

- Level data format exists.
- Region map screen exists.
- At least 120 levels or deterministic challenge definitions exist.
- Star goals work.
- Tutorials are embedded naturally.
- Cosmetic rewards unlock.

## Gate 7 — Tempest Trials weekly mode

Build weekly challenges.

Done when:

- Weekly seed/rules generated.
- 5-run playlist works.
- Weekly score works.
- Weekly result screen works.
- Cosmetic badge reward works.

## Gate 8 — Cosmetic progression and profile

Build cosmetic-only progression.

Done when:

- Player profile stores unlocks.
- Cosmetic equip screen exists.
- Cosmetic types are implemented.
- No cosmetic changes gameplay power.
- Reward flow feels polished.

## Gate 9 — Full UI shell and navigation

Build all production screens.

Required screens:

- Title.
- Main menu.
- Mode select.
- Endless gameplay.
- Daily Storm.
- Storm Trail map.
- Storm Trail level intro.
- Results.
- Game over.
- Cosmetics.
- Profile/stats.
- Achievements.
- Settings.
- Accessibility.
- Credits.

## Gate 10 — Audio, haptics, juice, and animation pass

Done when:

- Placement, invalid placement, clear, combo, pushback, storm warning, storm spread, rescue, near-death, game over, UI tap, reward, and daily result all have audio/haptic hooks.
- Motion and timing feel premium.

## Gate 11 — Game Center and service integration pass

Done when:

- Game Center service implementation exists or integration checklist is documented if credentials are missing.
- Mocks remain available.
- Leaderboards/achievements/challenges are interface-backed.
- No service failure breaks local play.

## Gate 12 — Performance optimization and device polish

Done when:

- Pools implemented for common VFX/pieces/tiles.
- Allocation spikes reduced.
- URP/mobile settings tuned.
- Quality tiers defined.
- Portrait safe areas handled.
- Older-device fallback exists.

## Gate 13 — QA and release candidate

Done when:

- Release checklist passes.
- Critical bugs fixed.
- Known issues documented.
- App Store metadata draft exists.
- TestFlight build steps documented.
