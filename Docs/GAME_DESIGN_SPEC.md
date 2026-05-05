# Storm Blocks — Full Game Design Spec

## One-sentence pitch

A premium 3D mobile block puzzle where players clear rows and columns to rescue survivors and push back a living storm before it reaches the central camp.

## Player fantasy

The player is not just clearing blocks. The player is protecting a tiny rescue camp from a magical storm. Every good move feels like saving people.

## Target audience

- Casual iPhone puzzle players.
- People who like short high-score loops.
- Players who enjoy satisfying clears, combos, and daily challenges.
- Players who want something more emotional than an abstract block puzzle.

## Core mechanics

### Board

- 8x8 grid.
- Central camp zone is visually in the middle.
- Camp may occupy a 2x2 visual area but should not confuse playable cell rules.
- Board uses 3D tiles in a fixed portrait camera.

### Pieces

- Player receives 3 block pieces at a time.
- Player places all valid pieces they can.
- Once all 3 are placed, a new set appears.
- Pieces are colorful, chunky, toy-like 3D blocks.

### Clears

- Full rows clear.
- Full columns clear.
- Simultaneous row/column clears produce combos.
- Cleared cells produce rescue, score, particles, haptics, and sound.

### Storm

- Storm begins around outer board edges.
- Storm blocks placement and threatens the central camp.
- Storm spreads inward at move-based intervals, not real-time intervals.
- Storm spread is telegraphed before resolving.
- Storm intensity escalates through the run.

### Storm Pushback

Storm Pushback is automatic.

When a cleared row or column intersects storm tiles, those storm tiles are destroyed or pushed back away from the camp.

Storm Pushback must be:

- mechanically meaningful,
- visually obvious,
- scored generously,
- celebrated with haptics/audio/VFX,
- central to tutorial and app-store screenshots.

### Survivors

Survivors appear as bonus markers, tiny characters, or camp-side characters. Clearing lines through survivor-related cells rescues them.

Survivors are emotional scoring, not complicated pathfinding.

### Game over

Game ends when either:

1. No available piece can be placed.
2. Storm reaches the camp danger threshold.

## Difficulty arc

### Calm phase

- Early run.
- Storm spreads slowly.
- Player learns pieces, clears, and pushback.
- Visuals are bright and cozy.

### Strategic phase

- Storm spreads every 3 placements.
- Player plans for pushback.
- Combos become important.

### Panic phase

- Storm spreads every 2 placements.
- Near-death visuals activate when storm is 1–2 rings from camp.
- Clutch saves are heavily rewarded.

## Modes

### Endless Storm

The main mode. High-score survival with escalating storm pressure.

Features:

- Infinite run.
- Difficulty ramp.
- Local best score.
- Best rescued count.
- Best combo.
- Best survival turn.
- Run summary.
- Instant restart.

### Daily Storm

A deterministic daily challenge.

Features:

- Same seed for all players.
- Same piece sequence.
- Same storm spread pattern.
- Same starting board.
- One official daily score, with optional practice runs clearly separated.
- Local daily history.
- Game Center-ready leaderboard interface.
- Streak tracking.
- Share-card-ready result.

### Storm Trail

A curated progression mode with 120+ boards.

Structure:

- 12 regions.
- 10 levels per region.
- Each region introduces a mechanic or modifier.
- Star goals per level.
- Cosmetic rewards.
- No paid gating.

Example regions:

1. First Camp — basics.
2. Wind Edge — storm warnings.
3. Rescue Hollow — survivor tiles.
4. Lightning Ridge — charged storm tiles.
5. Frozen Flats — frozen cells.
6. Firebreak Forest — flare bonuses.
7. Floodway — water pressure.
8. Night Camp — near-death training.
9. Signal Hill — combo goals.
10. Tempest Gate — heavy storm.
11. Safehouse Road — advanced boards.
12. Final Front — mastery.

### Tempest Trials

Weekly rotating challenge playlist.

Features:

- 5 challenge runs per week.
- Rotating modifiers.
- Score targets.
- Cosmetic badge rewards.
- Game Center-ready weekly leaderboard.

### Practice / Chill Mode

A lower-pressure mode for casual players.

Features:

- Slower storm.
- No leaderboard.
- Good for onboarding.
- No score penalty.

## Tools / boosters

Tools are optional and earned. They must never be sold as power in launch build.

Possible tools:

- **Undo** — undo last placement if no storm spread has resolved.
- **Forecast** — show next storm spread cells.
- **Flare** — remove one storm tile.
- **Shield** — protect camp edge for one spread.

Tools should be introduced in Storm Trail, not in the first endless tutorial.

## Scoring

Score components:

- Cells cleared.
- Lines cleared.
- Survivors rescued.
- Combo multiplier.
- Storm tiles destroyed.
- Pushback distance.
- Clutch save bonus.
- Perfect set bonus.
- Streak bonus.

End-screen metrics:

- Score.
- Survivors rescued.
- Storm tiles destroyed.
- Best combo.
- Longest rescue streak.
- Clutch saves.
- Daily rank/local position.

## Retention hooks

- Instant restart.
- Daily Storm streak.
- Local daily history.
- Weekly Tempest Trials.
- Cosmetic unlocks.
- Game Center leaderboards and achievements.
- Shareable result card.
- Lightweight quests.

## Cosmetic progression

Cosmetics only:

- Block skins.
- Camp skins.
- Survivor outfits.
- Storm skins.
- Pushback VFX.
- Profile banners.
- Result-card frames.

No cosmetic can change gameplay power.
