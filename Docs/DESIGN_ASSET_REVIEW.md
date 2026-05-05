# Design Asset Review

Date: 2026-05-05

## Source Folders Reviewed

- `Design GLB/`: 16 valid GLB 2.0 source models, 311 MB total.
- `Design JPG/`: 17 RGB JPG concept frames, mostly 1408 x 1408 plus two 1056 x 1872 phone-layout frames.
- `Design Sample Video/`: two MP4 references. One is 720 x 1280 at 24 fps for 10.04 seconds with AAC audio; one is 544 x 544 at 24 fps for 6.04 seconds with AAC audio.

## GLB Findings

The raw GLBs are useful as visual source, but too heavy to place directly into the iPhone runtime:

- Single toy/block models range from roughly 178k to 383k triangles.
- `Meshy_AI_Stormy_Campfire_Rescu_*` is roughly 1.4M triangles per file.
- Repeating any raw GLB across board cells would break the mobile scene budget immediately.

## Runtime Decision

Use the files as source art and ship only optimized mobile variants where they improve the actual playable scene.

Current optimized outputs:

| Asset | Source | Optimized size | Optimized triangles | Runtime use |
| --- | --- | ---: | ---: | --- |
| `blue_2x2_block_mobile_lod1.glb` | `Meshy_AI_Blue_2x2_Lego_Brick_0505135752_texture.glb` | 249 KB | 7,452 | Runtime tray charm under `Resources/MeshyMobile` |
| `lightning_cloud_cube_mobile_lod1.glb` | `Meshy_AI_Lightning_Cloud_Cube_0505135824_texture.glb` | 1.1 MB | 3,902 | Import-ready source asset, not shipped through `Resources` |
| `stormy_campfire_rescue_mobile_lod1.glb` | `Meshy_AI_Stormy_Campfire_Rescu_0505135724_texture.glb` | 2.6 MB | 70,165 | Import-ready source asset, not shipped through `Resources` |
| `storm_sky_backdrop.png` | `Design JPG/grok-ef376e2a-6624-42ed-b7cb-1d8dc137394e (1).jpg` upper storm crop | 426 KB | N/A | Runtime background under `Resources/StormSky` |

The campfire GLB was tested directly in the central board, but rejected for runtime presentation because it read as a gray pasted object and harmed the board/camp clarity. The central camp is instead rebuilt procedurally to match the reference direction while preserving gameplay readability.

## Actual Board Changes

The playable `StormBlocksGameView` now uses the design references in the runtime board:

- Board metrics, camera, and tray placement were tuned toward the supplied portrait layout.
- Storm corners and board edge pressure were strengthened with blue-purple cloud and spiral accents.
- The central camp keeps a warm tent/fire/survivor silhouette and no longer has the rejected raw camp GLB overlay.
- The piece tray includes an optimized GLB toy-block charm while repeated gameplay cells remain procedural for performance.
- The background uses a blurred design-source phone-layout crop to capture the supplied storm atmosphere without copying the source board/UI into the playable surface.
- App Store screenshots and the portrait gameplay capture were regenerated after the board pass, then refreshed again so the first store screenshot and gameplay capture stage the real saved-pushback moment through the runtime placement API.

## Reproduction

Run the optimizer after raw GLBs change:

```bash
Scripts/optimize_design_glbs.sh
```

Then reimport/capture in Unity:

```bash
Scripts/ios_release_gates.sh test
'/Applications/Unity/Hub/Editor/6000.4.5f1/Unity.app/Contents/MacOS/Unity' \
  -quit -batchmode \
  -projectPath /Users/perlantir/Projects/StormBlocks/StormBlocksUnity \
  -executeMethod StormBlocks.Editor.StormBlocksVisualCapture.CapturePortraitGameplay \
  -logFile /tmp/stormblocks-visual-design-backdrop-crop.log
```
