# App Store Screenshots

Generated screenshots are release-candidate drafts for App Store metadata review. They are built from the in-game Unity presentation scene so they stay close to the actual product surface.

## Generation Command

```bash
'/Applications/Unity/Hub/Editor/6000.4.5f1/Unity.app/Contents/MacOS/Unity' \
  -quit -batchmode \
  -projectPath /Users/perlantir/Projects/StormBlocks/StormBlocksUnity \
  -executeMethod StormBlocks.Editor.StormBlocksVisualCapture.CaptureAppStoreScreenshots \
  -logFile /tmp/stormblocks-appstore-screens.log
```

## Output Set

The current output directory is ignored by git:

`StormBlocksUnity/Builds/AppStoreScreens/`

The release handoff copies the current set into Fastlane's tracked screenshot package:

`fastlane/screenshots/en-US/`

Current files:

- `01_place_blocks_save_camp.png` - core loop, warm camp, readable board.
- `02_beat_daily_storm.png` - Daily Storm retention mode.
- `03_storm_trail_progression.png` - Storm Trail map/progression mode.
- `04_tempest_trials_weekly.png` - weekly Tempest Trials run set.
- `05_cosmetic_profile.png` - cosmetic-only profile/progression surface.

Each generated image targets iPhone portrait composition at 1170 x 2532.

## Review Notes

- The screenshots use the procedural 3D art direction currently in the Unity project.
- The scenes are draft store assets until final art/audio/signing review.
- The output should be regenerated after any major visual, UI, mode, or monetization-surface change.
- After regeneration, copy the five PNGs into `fastlane/screenshots/en-US/` and rerun `Scripts/verify_release_assets.sh`.
