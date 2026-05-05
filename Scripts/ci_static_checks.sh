#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

cd "$ROOT"

echo "==> Checking shell syntax"
bash -n Scripts/*.sh

echo "==> Checking release assets"
Scripts/verify_release_assets.sh

echo "==> Checking prompt compliance"
Scripts/verify_prompt_compliance.sh

echo "==> Checking App Store manifest JSON"
ruby -rjson -e 'JSON.parse(File.read("Docs/APP_STORE_CONNECT_MANIFEST.json")); puts "Manifest JSON parses"'

echo "==> Checking for transient generated artifacts"
found="$(find \
  StormBlocksUnity/Assets \
  fastlane \
  \( -name 'PerformanceTestRun*.json' -o \
     -name 'PerformanceTestRun*.xml' -o \
     -name 'InitTestScene*.unity' -o \
     -name 'mono_crash*.json' -o \
     -name 'report.xml' \) \
  -print 2>/dev/null || true)"

if [[ -n "$found" ]]; then
  printf 'Unexpected generated artifacts:\n%s\n' "$found" >&2
  exit 1
fi

echo "==> Static CI checks passed"
