#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
BREW_RUBY_BIN="${STORMBLOCKS_RUBY_BIN:-/opt/homebrew/opt/ruby/bin}"

cd "$ROOT"

if [[ -d "$BREW_RUBY_BIN" ]]; then
  export PATH="$BREW_RUBY_BIN:$PATH"
fi

if ! command -v bundle >/dev/null 2>&1; then
  printf 'ERROR: Bundler is not available. Install Ruby/Bundler before running Fastlane release lanes.\n' >&2
  exit 1
fi

if ! bundle -v >/dev/null 2>&1; then
  printf 'ERROR: Bundler exists but cannot run with this Gemfile.lock.\n' >&2
  printf '       On this Mac, use Homebrew Ruby by keeping /opt/homebrew/opt/ruby/bin ahead of /usr/bin.\n' >&2
  exit 1
fi

export FASTLANE_SKIP_UPDATE_CHECK="${FASTLANE_SKIP_UPDATE_CHECK:-1}"
exec bundle exec fastlane "$@"
