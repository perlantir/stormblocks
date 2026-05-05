#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
CLI_VERSION="${STORMBLOCKS_GLTF_TRANSFORM_VERSION:-4.3.0}"
CLI=(npx --yes "@gltf-transform/cli@$CLI_VERSION")
SOURCE_DIR="$ROOT/Design GLB"
OPTIMIZED_DIR="$ROOT/DesignOptimized/GLB"
UNITY_RESOURCES="$ROOT/StormBlocksUnity/Assets/StormBlocks/Art/Imported/Resources/MeshyMobile"
UNITY_SOURCE="$ROOT/StormBlocksUnity/Assets/StormBlocks/Art/Imported/MeshyMobileSource"

optimize() {
  local input="$1"
  local output="$2"
  local texture_size="$3"
  local simplify_ratio="$4"
  local simplify_error="$5"

  "${CLI[@]}" optimize "$SOURCE_DIR/$input" "$OPTIMIZED_DIR/$output" \
    --compress quantize \
    --texture-size "$texture_size" \
    --texture-compress auto \
    --simplify-ratio "$simplify_ratio" \
    --simplify-error "$simplify_error"
}

mkdir -p "$OPTIMIZED_DIR" "$UNITY_RESOURCES" "$UNITY_SOURCE"

optimize \
  "Meshy_AI_Stormy_Campfire_Rescu_0505135724_texture.glb" \
  "stormy_campfire_rescue_mobile_lod1.glb" \
  768 \
  0.015 \
  0.03

optimize \
  "Meshy_AI_Lightning_Cloud_Cube_0505135824_texture.glb" \
  "lightning_cloud_cube_mobile_lod1.glb" \
  512 \
  0.02 \
  0.03

optimize \
  "Meshy_AI_Blue_2x2_Lego_Brick_0505135752_texture.glb" \
  "blue_2x2_block_mobile_lod1.glb" \
  512 \
  0.02 \
  0.02

cp "$OPTIMIZED_DIR/stormy_campfire_rescue_mobile_lod1.glb" "$UNITY_SOURCE/"
cp "$OPTIMIZED_DIR/lightning_cloud_cube_mobile_lod1.glb" "$UNITY_SOURCE/"
cp "$OPTIMIZED_DIR/blue_2x2_block_mobile_lod1.glb" "$UNITY_RESOURCES/"

du -h "$OPTIMIZED_DIR"/*.glb "$UNITY_RESOURCES"/*.glb "$UNITY_SOURCE"/*.glb
