# Storm Blocks — Art Direction

## Final style name

**Premium Casual Toy-Puzzle Storm Rescue**

## Visual target

Studio-quality stylized 3D for mobile.

The game should look like a top-charting casual iPhone puzzle game, with the emotional identity of a cute rescue camp fighting a magical storm.

## Reference usage

Use `/DesignReferences`:

- `04_ref_primary_gameplay_layout.jpg` — primary production gameplay layout.
- `02_ref_mass_market_charm.jpg` — character charm, palette softness, mass-market appeal.
- `03_ref_storm_intensity_combo.jpg` — near-death tension and combo/pushback energy.
- `01_ref_logo_mood_phone_frame.jpg` — title/logo atmosphere.

These are visual references, not literal UI blueprints. Do not copy confusing buttons such as PUSH, HINT, CLEAR ROW, or fake power-up UI unless the design spec explicitly implements them.

## Style principles

- Bright and readable first.
- Storm/rescue identity second.
- 3D polish always.
- No gritty apocalypse.
- No muddy darkness.
- No flat default UI.
- No generic clone look.

## Board

- 8x8 board, centered.
- 3D rounded square tiles.
- Slight bevels.
- Soft shadows.
- Clear cell boundaries.
- Warm center camp glow.
- Storm visible around edges.
- Board never obscured by VFX.

## Blocks

- Soft 3D toy blocks.
- Chunky rounded shapes.
- High saturation colors.
- Subtle material variation: plastic/clay/candy-like, but not candy IP.
- Colorblind-safe shape/texture accents.

## Storm

- Blue-purple clouds.
- Soft volumetric-looking puffs using optimized sprites/particles.
- Lightning accents.
- Warning telegraphs before spread.
- Storm tiles have cracked/dark tile material plus cloud overlay.
- Near-death state adds vignette, wind streaks, and pulse.

## Camp

- Warm golden/orange light.
- Tiny tent/shelter/fire/signal flag.
- Should feel worth protecting.
- Must not make board cells ambiguous.

## Survivors

- Tiny cute 3D/2.5D characters.
- Simple silhouettes.
- Big readable color outfits.
- No detailed facial rig required.
- Celebrate rescued clears with tiny run/cheer animations.

## UI

- Rounded purple/cream panels.
- High contrast text.
- Big iPhone touch targets.
- TextMeshPro-style bold rounded typography.
- Minimal HUD: score, rescued, best/daily, pause.
- Avoid clutter.

## Signature visual moments

### Storm Pushback

When a line clear touches storm tiles:

- Cleared line glows gold/cyan.
- Energy wave travels along the row/column.
- Storm tiles shatter/whoosh backward.
- Survivors cheer/run.
- Haptic burst.
- Sound swell.
- Combo text appears briefly.

### Near-death

When storm is 1–2 rings from camp:

- Camp pulses warm/red.
- Wind audio intensifies.
- Storm vignettes screen edges.
- Warning tiles pulse.
- Survivors look frantic.
- Board remains readable.

### Clutch save

When near-death pushback occurs:

- Big but brief “CLUTCH SAVE!” or icon-based celebration.
- Golden flare.
- Storm clears back.
- Score multiplier pops.

## Performance target

The game should look high-end but be mobile optimized:

- Limited dynamic lights.
- Mostly baked/gradient lighting and shader tricks.
- Pooled particle systems.
- GPU instancing for tiles/pieces.
- Sprite/mesh atlases.
- Avoid heavy realtime shadows.
- Avoid uncontrolled transparent overdraw.
