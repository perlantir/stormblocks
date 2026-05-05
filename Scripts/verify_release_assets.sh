#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
METADATA_DIR="$ROOT/fastlane/metadata/en-US"
SCREENSHOT_DIR="$ROOT/fastlane/screenshots/en-US"
MANIFEST="$ROOT/Docs/APP_STORE_CONNECT_MANIFEST.json"
GAME_CENTER_DOC="$ROOT/Docs/GAME_CENTER_SETUP.md"
GAME_CENTER_CODE="$ROOT/StormBlocksUnity/Assets/StormBlocks/Scripts/Presentation/UnityGameCenterServices.cs"

pass_count=0

pass() {
  pass_count=$((pass_count + 1))
  printf 'PASS: %s\n' "$*"
}

fail() {
  printf 'FAIL: %s\n' "$*" >&2
  exit 1
}

require_file() {
  [[ -f "$1" ]] || fail "Missing file: $1"
  pass "$2"
}

char_count() {
  ruby -e 'print File.read(ARGV[0]).strip.length' "$1"
}

byte_count() {
  ruby -e 'print File.read(ARGV[0]).strip.bytesize' "$1"
}

require_chars_at_most() {
  local file="$1"
  local limit="$2"
  local label="$3"
  local count
  count="$(char_count "$file")"
  [[ "$count" -le "$limit" ]] || fail "$label is $count characters, over $limit"
  pass "$label is within $limit characters"
}

require_bytes_at_most() {
  local file="$1"
  local limit="$2"
  local label="$3"
  local count
  count="$(byte_count "$file")"
  [[ "$count" -le "$limit" ]] || fail "$label is $count bytes, over $limit"
  pass "$label is within $limit bytes"
}

require_no_pattern() {
  local file="$1"
  local pattern="$2"
  local label="$3"
  if grep -Eiq "$pattern" "$file"; then
    fail "$label contains forbidden pattern: $pattern"
  fi
  pass "$label contains no forbidden pattern"
}

require_image_size() {
  local file="$1"
  local expected_width="$2"
  local expected_height="$3"
  local label="$4"
  local width height
  width="$(sips -g pixelWidth "$file" 2>/dev/null | awk '/pixelWidth/ { print $2 }')"
  height="$(sips -g pixelHeight "$file" 2>/dev/null | awk '/pixelHeight/ { print $2 }')"
  [[ "$width" == "$expected_width" && "$height" == "$expected_height" ]] \
    || fail "$label is ${width}x${height}, expected ${expected_width}x${expected_height}"
  pass "$label is ${expected_width}x${expected_height}"
}

require_file "$MANIFEST" "App Store Connect manifest exists"
require_file "$METADATA_DIR/name.txt" "App Store name metadata exists"
require_file "$METADATA_DIR/subtitle.txt" "App Store subtitle metadata exists"
require_file "$METADATA_DIR/promotional_text.txt" "App Store promotional text metadata exists"
require_file "$METADATA_DIR/description.txt" "App Store description metadata exists"
require_file "$METADATA_DIR/keywords.txt" "App Store keywords metadata exists"
require_file "$METADATA_DIR/support_url.txt" "App Store support URL metadata exists"
require_file "$METADATA_DIR/marketing_url.txt" "App Store marketing URL metadata exists"
require_file "$METADATA_DIR/privacy_url.txt" "App Store privacy URL metadata exists"
require_file "$ROOT/Docs/PUBLIC_SUPPORT.md" "Public support page draft exists"
require_file "$ROOT/Docs/PUBLIC_PRIVACY.md" "Public privacy page draft exists"

require_chars_at_most "$METADATA_DIR/name.txt" 30 "App Store name"
require_chars_at_most "$METADATA_DIR/subtitle.txt" 30 "App Store subtitle"
require_chars_at_most "$METADATA_DIR/promotional_text.txt" 170 "App Store promotional text"
require_chars_at_most "$METADATA_DIR/description.txt" 4000 "App Store description"
require_bytes_at_most "$METADATA_DIR/keywords.txt" 100 "App Store keywords"

require_no_pattern "$METADATA_DIR/keywords.txt" 'block blast|tetris|minecraft|roblox|disney|marvel|pokemon|candy crush' "App Store keywords"
require_no_pattern "$METADATA_DIR/description.txt" 'block blast|tetris|minecraft|roblox|disney|marvel|pokemon|candy crush' "App Store description"
grep -q 'support@perlantir.com' "$ROOT/Docs/PUBLIC_SUPPORT.md" \
  || fail "Public support page is missing support email"
pass "Public support page includes support email"
grep -q 'support@perlantir.com' "$ROOT/Docs/PUBLIC_PRIVACY.md" \
  || fail "Public privacy page is missing contact email"
pass "Public privacy page includes contact email"

while IFS= read -r screenshot; do
  require_file "$SCREENSHOT_DIR/$screenshot" "Screenshot asset $screenshot exists"
  require_image_size "$SCREENSHOT_DIR/$screenshot" 1170 2532 "Screenshot asset $screenshot"
done < <(ruby -rjson -e 'JSON.parse(File.read(ARGV[0])).fetch("screenshots").each { |name| puts name }' "$MANIFEST")

ruby -rjson -e '
manifest = JSON.parse(File.read(ARGV[0]))
failures = []
app = manifest.fetch("app")
failures << "bundleId" unless app.fetch("bundleId") == "com.perlantir.stormblocks"
failures << "teamId" unless app.fetch("teamId") == "7JL22TDB44"
failures << "sku" unless app.fetch("sku") == "stormblocks-ios"
failures << "version" unless app.fetch("version") == "0.1.0"
failures << "leaderboard count" unless manifest.fetch("leaderboards").length == 3
failures << "achievement count" unless manifest.fetch("achievements").length == 8
point_total = manifest.fetch("achievements").sum { |achievement| achievement.fetch("points") }
failures << "achievement point total" unless point_total <= 1000
abort("Manifest validation failed: #{failures.join(", ")}") unless failures.empty?
' "$MANIFEST"
pass "App Store Connect manifest core fields validate"

ruby -rjson -e '
manifest = JSON.parse(File.read(ARGV[0]))
ids = manifest.fetch("leaderboards").map { |item| item.fetch("id") } +
  manifest.fetch("achievements").map { |item| item.fetch("id") }
code = File.read(ARGV[1])
doc = File.read(ARGV[2])
missing = ids.flat_map do |id|
  misses = []
  misses << "#{id} missing from UnityGameCenterServices.cs" unless code.include?(id)
  misses << "#{id} missing from GAME_CENTER_SETUP.md" unless doc.include?(id)
  misses
end
abort(missing.join("\n")) unless missing.empty?
' "$MANIFEST" "$GAME_CENTER_CODE" "$GAME_CENTER_DOC"
pass "Game Center identifiers match code and setup docs"

printf '\nRelease asset summary: %d pass, 0 fail\n' "$pass_count"
