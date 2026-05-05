# Storm Blocks — Full Game Codex `/goal` Package

This package is designed to be unzipped into the root of a new Git repo before starting Codex.

The objective is **not** a prototype and not an MVP. The objective is a **launch-quality, feature-complete, studio-polished iPhone game** using the working title **Storm Blocks**.

## Game

**Storm Blocks** is a free, portrait-first iPhone block puzzle game with premium stylized 3D visuals. Players drag colorful block pieces onto an 8x8 grid, clear rows and columns, rescue tiny survivors, and push back a living storm before it reaches the warm central camp.

Core promise:

> Place blocks. Clear lines. Rescue survivors. Push back the storm.

## Visual target

Use the design references in `/DesignReferences`.

Final blend:

- Primary gameplay layout: `04_ref_primary_gameplay_layout.jpg`
- Mass-market charm / palette / cute characters: `02_ref_mass_market_charm.jpg`
- Storm intensity / near-death / combo energy: `03_ref_storm_intensity_combo.jpg`
- Logo and atmosphere: `01_ref_logo_mood_phone_frame.jpg`

Final art direction:

> Premium Casual Toy-Puzzle Storm Rescue, with studio-quality stylized 3D, warm camp lighting, cute survivors, blue-purple storm energy, juicy combo VFX, rounded UI, and a readable 8x8 board.

## How to use this package with Codex

1. Create a new Git repo.
2. Unzip this package into the repo root.
3. Create a new Unity 6 URP iOS project in a folder named `/StormBlocksUnity`, or let Codex create/organize that folder if your environment has Unity available.
4. Open `CodexPrompts/00_SET_GOAL.txt` and paste it into Codex using `/goal`.
5. Then paste `CodexPrompts/01_START_FULL_GAME_BUILD.txt`.
6. When Codex pauses, paste `CodexPrompts/02_CONTINUE_TO_RELEASE_CANDIDATE.txt`.
7. Do not approve “done” unless `Docs/RELEASE_DONE_CHECKLIST.md` passes.

## Important

The build should proceed through milestones, but those milestones are quality gates toward the full game. They are **not** permission to stop at a small prototype.

Codex should always keep the final launch target in mind: full gameplay, full modes, full polish, full 3D presentation, Game Center-ready social layer, production UX, audio/haptics, QA, and iOS release readiness.
