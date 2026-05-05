# GOAL.md — Full Game Goal

Build the complete launch-quality game **Storm Blocks** for iPhone.

This is not an MVP request. This is not a prototype request. This is a full-game production request using Codex `/goal`.

## Final product

Storm Blocks is a free, portrait-first, premium casual 3D block puzzle game for iPhone.

The player drags colorful 3D block pieces onto an 8x8 grid, clears rows and columns, rescues tiny survivors, and pushes back a living blue-purple storm before it reaches a warm central camp.

## Core player promise

> Place blocks. Clear lines. Rescue survivors. Push back the storm. Beat today’s Daily Storm.

## Final shipped modes

1. **Endless Storm** — the main one-more-run high-score mode.
2. **Daily Storm** — deterministic daily challenge with same seed, same pieces, same storm pattern, leaderboard-ready score, streak, and share-ready result.
3. **Storm Trail** — curated progression journey with 120+ challenge boards, modifiers, tutorialized mechanics, star goals, and cosmetic rewards.
4. **Tempest Trials** — rotating weekly challenge playlist using modifiers and score targets.
5. **Practice / Chill Mode** — optional no-pressure mode for casual players, unlocked early.

## Full game systems

- 8x8 board with central camp.
- Three-piece queue.
- Drag-and-drop placement.
- Row/column clears.
- Storm spread with warning telegraphs.
- Automatic Storm Pushback when clears touch storm tiles.
- Survivors rescued through clears, chains, and bonus tiles.
- Combo, streak, clutch, near-death, perfect-set, and storm-clear scoring.
- Difficulty ramp: calm → strategic → intense.
- Daily deterministic seed system.
- Local profile, local saves, high scores, best runs, streaks.
- Game Center-ready leaderboard/challenge/achievement service interfaces, with mocks first and real integration later.
- Cosmetic-only progression: block skins, camp skins, survivor outfits, storm skins, flare/pushback effects, profile banners.
- Optional earned tools: Undo, Forecast, Flare, Shield. These must be free-earned and never paid power.
- Achievements.
- Daily/weekly quests.
- Seasonal cosmetic collection structure.
- Results/share-card-ready run summaries.
- Accessibility options.
- Audio/haptics.
- iOS build readiness.

## Visual quality bar

Target **studio-quality stylized 3D**, not flat programmer art.

The game should look like a polished top-charting mobile puzzle game:

- Soft 3D toy-like blocks.
- High-end rounded UI.
- Warm camp glow.
- Cute tiny survivor characters.
- Blue-purple storm clouds and lightning.
- Juicy particles, screen shake, bloom, haptics, and sound hooks.
- Cinematic near-death state.
- Spectacular Storm Pushback combo effects.
- Readable board at all times.

## Non-negotiables

- No forced ads in launch build.
- No paid speedups.
- No paid power.
- No gacha.
- No loot boxes.
- No battle pass that gates power.
- No copyrighted IP, characters, logos, brands, or cloned UI.
- Do not make the storm cosmetic only. Storm Pushback must be mechanical, visual, and emotional.
- Do not ship sloppy UI or default Unity-looking screens.

## Done when

The game is a launch-ready iPhone release candidate satisfying `Docs/RELEASE_DONE_CHECKLIST.md`, including full gameplay, full modes, full polish, optimized 3D presentation, save/profile systems, Daily Storm, progression, cosmetics, audio/haptics, Game Center-ready interfaces, QA checks, and documented iOS build steps.
