#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
SCORECARD="${1:-$ROOT/Docs/FIVE_RUN_QA_SCORECARD.md}"

pass_count=0
fail_count=0

pass() {
  pass_count=$((pass_count + 1))
  printf 'PASS: %s\n' "$*"
}

fail() {
  fail_count=$((fail_count + 1))
  printf 'FAIL: %s\n' "$*" >&2
}

require_readable() {
  if [[ -r "$SCORECARD" ]]; then
    pass "Five-run QA scorecard is readable"
  else
    fail "Five-run QA scorecard is not readable at $SCORECARD"
  fi
}

require_inline_value() {
  local label="$1"
  if awk -v label="$label" '
      function trim(value) {
        gsub(/^[[:space:]]+|[[:space:]]+$/, "", value)
        return value
      }
      index($0, label ":") == 1 {
        value = substr($0, length(label) + 2)
        if (trim(value) != "") found = 1
      }
      END { exit found ? 0 : 1 }
    ' "$SCORECARD"; then
    pass "$label is filled"
  else
    fail "$label is blank"
  fi
}

require_exact_inline_value() {
  local label="$1"
  local expected="$2"
  if awk -v label="$label" -v expected="$expected" '
      function trim(value) {
        gsub(/^[[:space:]]+|[[:space:]]+$/, "", value)
        return value
      }
      index($0, label ":") == 1 {
        value = substr($0, length(label) + 2)
        if (trim(value) == expected) found = 1
      }
      END { exit found ? 0 : 1 }
    ' "$SCORECARD"; then
    pass "$label is $expected"
  else
    fail "$label must be $expected"
  fi
}

require_none_or_issue_value() {
  local label="$1"
  if awk -v label="$label" '
      function trim(value) {
        gsub(/^[[:space:]]+|[[:space:]]+$/, "", value)
        return value
      }
      index($0, label ":") == 1 {
        value = trim(substr($0, length(label) + 2))
        if (value == "None" || value ~ /^https:\/\/github.com\/perlantir\/stormblocks\/issues\/[0-9]+/) found = 1
      }
      END { exit found ? 0 : 1 }
    ' "$SCORECARD"; then
    pass "$label is recorded"
  else
    fail "$label must be None or a stormblocks issue URL"
  fi
}

require_run_log() {
  if awk -F'|' '
      function trim(value) {
        gsub(/^[[:space:]]+|[[:space:]]+$/, "", value)
        return value
      }
      BEGIN {
        expected[1] = "Endless Storm"
        expected[2] = "Daily Storm"
        expected[3] = "Storm Trail"
        expected[4] = "Tempest Trials"
        expected[5] = "Practice Mode"
      }
      /^\|[[:space:]]*[1-5][[:space:]]*\|/ {
        run = trim($2)
        mode = trim($3)
        start_time = trim($4)
        end_state = trim($5)
        score = trim($6)
        pushback = trim($7)
        survivor = trim($8)
        notes = trim($9)
        found[run] = 1
        if (mode != expected[run]) {
          printf("Run %s mode must be %s, found %s\n", run, expected[run], mode) > "/dev/stderr"
          errors++
        }
        if (start_time == "" || end_state == "" || score == "" || notes == "") {
          printf("Run %s must include start time, end state, score/level, and notes\n", run) > "/dev/stderr"
          errors++
        }
        if (pushback != "Yes") {
          printf("Run %s must record Storm Pushback Seen as Yes\n", run) > "/dev/stderr"
          errors++
        }
        if (survivor != "Yes") {
          printf("Run %s must record Survivor Rescued as Yes\n", run) > "/dev/stderr"
          errors++
        }
      }
      END {
        for (i = 1; i <= 5; i++) {
          if (!found[i]) {
            printf("Run %d is missing from the run log\n", i) > "/dev/stderr"
            errors++
          }
        }
        exit errors ? 1 : 0
      }
    ' "$SCORECARD"; then
    pass "Run log has five complete mode runs with rescue and pushback"
  else
    fail "Run log is incomplete"
  fi
}

require_release_scores() {
  if awk -F'|' '
      function trim(value) {
        gsub(/^[[:space:]]+|[[:space:]]+$/, "", value)
        return value
      }
      BEGIN {
        categories[1] = "Instant understandability"
        categories[2] = "One-more-run pull"
        categories[3] = "Board readability"
        categories[4] = "Visual polish versus references"
        categories[5] = "Storm Pushback satisfaction"
        categories[6] = "Touch feel"
        categories[7] = "Performance feel"
        categories[8] = "Audio and haptics feel"
        categories[9] = "Accessibility settings clarity"
        for (i = 1; i <= 9; i++) expected[categories[i]] = 1
      }
      /^\|/ {
        category = trim($2)
        score = trim($3)
        notes = trim($4)
        if (category in expected) {
          found[category] = 1
          if (score !~ /^[4-5]$/) {
            printf("%s score must be 4 or 5, found %s\n", category, score) > "/dev/stderr"
            errors++
          }
          if (notes == "") {
            printf("%s must include evidence notes\n", category) > "/dev/stderr"
            errors++
          }
        }
      }
      END {
        for (i = 1; i <= 9; i++) {
          if (!found[categories[i]]) {
            printf("%s score row is missing\n", categories[i]) > "/dev/stderr"
            errors++
          }
        }
        exit errors ? 1 : 0
      }
    ' "$SCORECARD"; then
    pass "Release scores are all 4+ with evidence notes"
  else
    fail "Release scores are incomplete or below target"
  fi
}

require_required_checks() {
  if awk -F'|' '
      function trim(value) {
        gsub(/^[[:space:]]+|[[:space:]]+$/, "", value)
        return value
      }
      BEGIN {
        checks[1] = "Dragging pieces does not fight the notch, Dynamic Island, or home indicator."
        checks[2] = "Tray pieces remain readable and tappable in portrait."
        checks[3] = "Invalid placement feedback is clear but not noisy."
        checks[4] = "Row and column clears are readable."
        checks[5] = "Survivor rescue is visible and emotionally clear."
        checks[6] = "Storm Pushback happens automatically when a clear intersects storm tiles."
        checks[7] = "Near-death presentation is dramatic but does not hide playable cells."
        checks[8] = "Game-over and Retry are immediate."
        checks[9] = "Settings persist after app restart."
        checks[10] = "Reduced Motion and Low Detail reduce effects without breaking clarity."
        checks[11] = "Share opens the iOS share sheet with text and image."
        checks[12] = "No unexpected permission prompts appear."
        checks[13] = "No crash, freeze, corrupted save, unreadable board state, or blocked tap target occurred."
        for (i = 1; i <= 13; i++) expected[checks[i]] = 1
      }
      /^\|/ {
        label = trim($2)
        result = trim($3)
        notes = trim($4)
        if (label in expected) {
          found[label] = 1
          if (result != "Pass") {
            printf("%s must be Pass, found %s\n", label, result) > "/dev/stderr"
            errors++
          }
          if (notes == "") {
            printf("%s must include notes\n", label) > "/dev/stderr"
            errors++
          }
        }
      }
      END {
        for (i = 1; i <= 13; i++) {
          if (!found[checks[i]]) {
            printf("%s required check row is missing\n", checks[i]) > "/dev/stderr"
            errors++
          }
        }
        exit errors ? 1 : 0
      }
    ' "$SCORECARD"; then
    pass "Required physical QA checks all pass with notes"
  else
    fail "Required physical QA checks are incomplete"
  fi
}

require_defects_section() {
  if awk -F'|' '
      function trim(value) {
        gsub(/^[[:space:]]+|[[:space:]]+$/, "", value)
        return value
      }
      /^## Defects/ { in_defects = 1; next }
      /^## Signoff/ { in_defects = 0 }
      in_defects && /^\|/ {
        severity = trim($2)
        if (severity == "" || severity == "---" || severity == "Severity") next
        rows++
        area = trim($3)
        steps = trim($4)
        expected = trim($5)
        actual = trim($6)
        if (severity == "None") {
          clean = 1
        } else if (severity == "Minor") {
          minor = 1
          if (area == "" || steps == "" || expected == "" || actual == "") {
            printf("Minor defect rows must include area, steps, expected, and actual\n") > "/dev/stderr"
            errors++
          }
        } else {
          printf("Blocking defect severity remains in scorecard: %s\n", severity) > "/dev/stderr"
          errors++
        }
      }
      END {
        if (rows == 0) {
          printf("Defects section must contain None or fully described Minor rows\n") > "/dev/stderr"
          errors++
        }
        if (clean && minor) {
          printf("Defects section cannot mix None with Minor rows\n") > "/dev/stderr"
          errors++
        }
        exit errors ? 1 : 0
      }
    ' "$SCORECARD"; then
    pass "Defects section has no blocker or major defects"
  else
    fail "Defects section is incomplete or contains blocking defects"
  fi
}

require_readable
require_inline_value "Date"
require_inline_value "Tester"
require_inline_value "Device model"
require_inline_value "iOS version"
require_inline_value "Build source / commit"
require_inline_value "Install source"
require_run_log
require_release_scores
require_required_checks
require_defects_section
require_exact_inline_value "Physical QA result" "Pass"
require_none_or_issue_value "Release-blocking issues"
require_none_or_issue_value "Follow-up issues created"
require_inline_value "Tester notes"

printf '\nFive-run QA scorecard summary: %d pass, %d fail\n' "$pass_count" "$fail_count"

if [[ "$fail_count" -gt 0 ]]; then
  exit 1
fi
