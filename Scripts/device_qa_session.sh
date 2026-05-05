#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
BUNDLE_ID="${STORMBLOCKS_BUNDLE_ID:-com.perlantir.stormblocks}"
DEVICE_ID="${STORMBLOCKS_DEVICE_ID:-907E2EE7-9C7B-5D0D-9EC0-32E69912287D}"
XCTRACE_DEVICE_ID="${STORMBLOCKS_XCTRACE_DEVICE_ID:-}"
TRACE_DIR="${STORMBLOCKS_TRACE_DIR:-$ROOT/StormBlocksUnity/Builds/DeviceProfiles}"
TIME_LIMIT="${STORMBLOCKS_PROFILE_TIME:-3m}"

log() {
  printf '==> %s\n' "$*"
}

plan() {
  cat <<PLAN
Storm Blocks physical QA plan

1. Unlock paired device: $DEVICE_ID
2. Launch signed app: Scripts/device_qa_session.sh launch
3. Run five manual playability sessions from Docs/PHYSICAL_QA_RUNBOOK.md
4. Record Game Performance trace: Scripts/device_qa_session.sh profile-game
5. Record Power Profiler trace: Scripts/device_qa_session.sh profile-power
6. Update Docs/QA_EVAL_REPORT.md and Docs/PERFORMANCE_PROFILE.md with results
7. Rerun Scripts/release_audit.sh full

Open external gates remain until App Store Connect has the app record, Game Center identifiers
are live, TestFlight is installed, and human/device QA plus profiling are recorded.
PLAN
}

launch() {
  "$ROOT/Scripts/ios_release_gates.sh" launch-device
}

xctrace_device_id() {
  if [[ -n "$XCTRACE_DEVICE_ID" ]]; then
    printf '%s\n' "$XCTRACE_DEVICE_ID"
    return
  fi

  local device_list
  device_list="$(mktemp -t stormblocks-devices.XXXXXX.json)"
  if xcrun devicectl list devices --json-output "$device_list" >/dev/null 2>/dev/null \
    && plutil -extract result.devices.0.hardwareProperties.udid raw -o - "$device_list" >/dev/null 2>/dev/null; then
    plutil -extract result.devices.0.hardwareProperties.udid raw -o - "$device_list"
  else
    printf '%s\n' "$DEVICE_ID"
  fi

  rm -f "$device_list"
}

record_trace() {
  local template="$1"
  local suffix="$2"
  mkdir -p "$TRACE_DIR"
  local timestamp
  timestamp="$(date -u +%Y%m%dT%H%M%SZ)"
  local output="$TRACE_DIR/stormblocks-$suffix-$timestamp.trace"
  local trace_device
  trace_device="$(xctrace_device_id)"
  log "Recording $template trace on $trace_device for $TIME_LIMIT"
  log "Launch the app first and keep Storm Blocks active while the trace runs."
  xcrun xctrace record \
    --template "$template" \
    --device "$trace_device" \
    --time-limit "$TIME_LIMIT" \
    --output "$output" \
    --attach StormBlocks \
    --no-prompt
  log "Trace output: $output"
}

case "${1:-plan}" in
  plan)
    plan
    ;;
  launch)
    launch
    ;;
  profile-game)
    record_trace "Game Performance" "game-performance"
    ;;
  profile-power)
    record_trace "Power Profiler" "power"
    ;;
  *)
    cat <<USAGE
Usage: $0 [plan|launch|profile-game|profile-power]

Environment overrides:
  STORMBLOCKS_DEVICE_ID=$DEVICE_ID
  STORMBLOCKS_BUNDLE_ID=$BUNDLE_ID
  STORMBLOCKS_XCTRACE_DEVICE_ID=${XCTRACE_DEVICE_ID:-<auto from devicectl hardware UDID>}
  STORMBLOCKS_PROFILE_TIME=$TIME_LIMIT
  STORMBLOCKS_TRACE_DIR=$TRACE_DIR
USAGE
    exit 2
    ;;
esac
