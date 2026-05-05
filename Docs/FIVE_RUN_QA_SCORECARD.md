# Five-Run QA Scorecard

Use this file for the required human five-run playability pass on a physical iPhone. Do not mark the physical-device QA checklist item complete until every required section is filled, each release score is 4 or higher, and no blocker remains open.

After filling it, run:

```bash
Scripts/verify_five_run_scorecard.sh
```

The physical-device QA checklist item must stay open until this verifier passes.

Date:
Tester:
Device model:
iOS version:
Build source / commit:
Install source:

## Run Log

| Run | Mode | Start Time | End State | Score / Level | Storm Pushback Seen | Survivor Rescued | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | Endless Storm |  |  |  | Yes / No | Yes / No |  |
| 2 | Daily Storm |  |  |  | Yes / No | Yes / No |  |
| 3 | Storm Trail |  |  |  | Yes / No | Yes / No |  |
| 4 | Tempest Trials |  |  |  | Yes / No | Yes / No |  |
| 5 | Practice Mode |  |  |  | Yes / No | Yes / No |  |

## Release Scores

Score each from 1 to 5. Release target is 4 or higher in every category.

| Category | Score | Evidence / Notes |
| --- | --- | --- |
| Instant understandability |  |  |
| One-more-run pull |  |  |
| Board readability |  |  |
| Visual polish versus references |  |  |
| Storm Pushback satisfaction |  |  |
| Touch feel |  |  |
| Performance feel |  |  |
| Audio and haptics feel |  |  |
| Accessibility settings clarity |  |  |

## Required Checks

| Check | Pass / Fail | Notes |
| --- | --- | --- |
| Dragging pieces does not fight the notch, Dynamic Island, or home indicator. |  |  |
| Tray pieces remain readable and tappable in portrait. |  |  |
| Invalid placement feedback is clear but not noisy. |  |  |
| Row and column clears are readable. |  |  |
| Survivor rescue is visible and emotionally clear. |  |  |
| Storm Pushback happens automatically when a clear intersects storm tiles. |  |  |
| Near-death presentation is dramatic but does not hide playable cells. |  |  |
| Game-over and Retry are immediate. |  |  |
| Settings persist after app restart. |  |  |
| Reduced Motion and Low Detail reduce effects without breaking clarity. |  |  |
| Share opens the iOS share sheet with text and image. |  |  |
| No unexpected permission prompts appear. |  |  |
| No crash, freeze, corrupted save, unreadable board state, or blocked tap target occurred. |  |  |

## Defects

| Severity | Area | Steps | Expected | Actual | Screenshot / Video |
| --- | --- | --- | --- | --- | --- |
| None |  |  |  |  |  |

## Signoff

Physical QA result: Pass / Fail

Release-blocking issues: None or issue URL

Follow-up issues created: None or issue URL

Tester notes:
