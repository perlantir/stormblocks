# Evals

## Automated evals

Codex should maintain tests for:

- Piece placement.
- Valid move detection.
- Line clearing.
- Storm spread.
- Storm Pushback.
- Scoring.
- Combo and clutch bonus.
- Daily seed determinism.
- Game-over states.
- Save/load roundtrip.

## Playability evals

After each playable gate, score from 1-5:

- Instant understandability.
- One-more-run pull.
- Board readability.
- Visual polish.
- Storm Pushback satisfaction.
- Touch feel.
- Performance.

Target release score: 4+ in every category.

## Five-run addiction test

A human tester should voluntarily play at least five runs in a row without being asked.

This test is required before release candidate.

## Visual quality eval

Compare current build to `/DesignReferences`:

- Does gameplay resemble reference 04?
- Does charm/palette resemble reference 02?
- Does near-death/combo energy resemble reference 03?
- Does logo/title atmosphere resemble reference 01?
- Does the game avoid confusing fake UI buttons from the reference images?

## Current report

See `Docs/QA_EVAL_REPORT.md` for the current automated eval evidence, simulator runtime proof, local playability estimates, and manual gates that remain before release signoff.
