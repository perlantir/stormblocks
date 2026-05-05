#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

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

require_dir() {
  [[ -d "$1" ]] || fail "Missing directory: $1"
  pass "$2"
}

require_grep() {
  local file="$1"
  local pattern="$2"
  local label="$3"
  grep -Eq "$pattern" "$file" || fail "$label missing from $file"
  pass "$label"
}

require_absent_grep() {
  local file="$1"
  local pattern="$2"
  local label="$3"
  if grep -Eiq "$pattern" "$file"; then
    fail "$label found forbidden pattern in $file: $pattern"
  fi
  pass "$label"
}

require_repo_absent() {
  local label="$1"
  shift
  local pattern="$1"
  shift
  local found
  found="$(grep -REin "$pattern" "$@" 2>/dev/null || true)"
  if [[ -n "$found" ]]; then
    printf '%s\n' "$found" >&2
    fail "$label"
  fi
  pass "$label"
}

require_file "AGENTS.md" "AGENTS.md exists"
require_file "GOAL.md" "GOAL.md exists"
require_file "Docs/GAME_DESIGN_SPEC.md" "Game design spec exists"
require_file "Docs/ART_DIRECTION.md" "Art direction spec exists"
require_file "Docs/TECH_SPEC.md" "Tech spec exists"
require_file "Docs/FULL_GAME_MILESTONES.md" "Full milestones doc exists"
require_file "Docs/ACCEPTANCE_CRITERIA.md" "Acceptance criteria doc exists"
require_file "Docs/RELEASE_DONE_CHECKLIST.md" "Release done checklist exists"
require_file "Docs/EVALS.md" "Evals doc exists"
require_file "Docs/IMPLEMENTATION_LOG.md" "Implementation log exists"
require_file "Docs/RELEASE_AUDIT.md" "Release audit exists"

require_file "DesignReferences/01_ref_logo_mood_phone_frame.jpg" "Title atmosphere reference exists"
require_file "DesignReferences/02_ref_mass_market_charm.jpg" "Charm and palette reference exists"
require_file "DesignReferences/03_ref_storm_intensity_combo.jpg" "Storm/combo reference exists"
require_file "DesignReferences/04_ref_primary_gameplay_layout.jpg" "Primary gameplay reference exists"

require_dir "StormBlocksUnity" "Unity project directory exists"
require_file "StormBlocksUnity/Packages/manifest.json" "Unity package manifest exists"
require_file "StormBlocksUnity/Assets/StormBlocks/Scripts/Core/StormBlocksCore.cs" "Plain C# core gameplay exists"
require_file "StormBlocksUnity/Assets/StormBlocks/Scripts/Core/StormBlocksModes.cs" "Mode config factory exists"
require_file "StormBlocksUnity/Assets/StormBlocks/Scripts/Services/StormBlocksServices.cs" "Service interfaces exist"
require_file "StormBlocksUnity/Assets/StormBlocks/Scripts/Services/StormBlocksProfileProgression.cs" "Profile/progression services exist"
require_file "StormBlocksUnity/Assets/StormBlocks/Scripts/Presentation/StormBlocksGameView.cs" "3D presentation view exists"
require_file "StormBlocksUnity/Assets/StormBlocks/Scripts/Presentation/UnityGameCenterServices.cs" "Game Center adapter exists"
require_file "StormBlocksUnity/Assets/StormBlocks/Scripts/Presentation/UnityLocalFeedbackService.cs" "Audio/haptic feedback service exists"
require_file "StormBlocksUnity/Assets/StormBlocks/Scripts/Presentation/UnityShareService.cs" "Share service exists"
require_file "StormBlocksUnity/Assets/Plugins/iOS/StormBlocksGameKitBridge.mm" "Native GameKit bridge exists"
require_file "StormBlocksUnity/Assets/Plugins/iOS/StormBlocksShareBridge.mm" "Native share bridge exists"

require_grep "StormBlocksUnity/Assets/StormBlocks/Scripts/Core/StormBlocksCore.cs" "public enum GameModeId" "GameModeId enum is present"
require_grep "StormBlocksUnity/Assets/StormBlocks/Scripts/Core/StormBlocksCore.cs" "EndlessStorm" "Endless Storm mode id is present"
require_grep "StormBlocksUnity/Assets/StormBlocks/Scripts/Core/StormBlocksCore.cs" "DailyStorm" "Daily Storm mode id is present"
require_grep "StormBlocksUnity/Assets/StormBlocks/Scripts/Core/StormBlocksCore.cs" "StormTrail" "Storm Trail mode id is present"
require_grep "StormBlocksUnity/Assets/StormBlocks/Scripts/Core/StormBlocksCore.cs" "TempestTrial" "Tempest Trials mode id is present"
require_grep "StormBlocksUnity/Assets/StormBlocks/Scripts/Core/StormBlocksCore.cs" "Practice" "Practice mode id is present"
require_grep "StormBlocksUnity/Assets/StormBlocks/Scripts/Core/StormBlocksCore.cs" "PushbackAutomatic = true" "Automatic Storm Pushback default is present"
require_grep "StormBlocksUnity/Assets/StormBlocks/Scripts/Core/StormBlocksCore.cs" "AutomaticPushbackTriggered" "Automatic pushback resolution exists"
require_grep "StormBlocksUnity/Assets/StormBlocks/Scripts/Core/StormBlocksCore.cs" "SurvivorsRescued" "Survivor rescue state exists"
require_grep "StormBlocksUnity/Assets/StormBlocks/Scripts/Core/StormBlocksCore.cs" "DailySeed" "Deterministic daily seed code exists"

require_grep "StormBlocksUnity/Assets/StormBlocks/Scripts/Core/StormBlocksModes.cs" "CreateStormTrailCatalog" "Storm Trail catalog factory exists"
require_grep "StormBlocksUnity/Assets/StormBlocks/Scripts/Core/StormBlocksModes.cs" "CreateTempestWeek" "Tempest Trials weekly factory exists"
require_grep "StormBlocksUnity/Assets/StormBlocks/Scripts/Core/StormBlocksModes.cs" "level <= 10" "Storm Trail has 10 levels per region"
require_grep "StormBlocksUnity/Assets/StormBlocks/Scripts/Core/StormBlocksModes.cs" "Final Front" "Storm Trail includes the 12th region"
require_grep "StormBlocksUnity/Assets/StormBlocks/Scripts/Core/StormBlocksModes.cs" "index < 5" "Tempest Trials has 5 weekly runs"

require_grep "StormBlocksUnity/Assets/StormBlocks/Scripts/Services/StormBlocksServices.cs" "interface ILeaderboardService" "Leaderboard service interface exists"
require_grep "StormBlocksUnity/Assets/StormBlocks/Scripts/Services/StormBlocksServices.cs" "interface IAchievementService" "Achievement service interface exists"
require_grep "StormBlocksUnity/Assets/StormBlocks/Scripts/Services/StormBlocksServices.cs" "interface IAnalyticsService" "Analytics service interface exists"
require_grep "StormBlocksUnity/Assets/StormBlocks/Scripts/Services/StormBlocksServices.cs" "interface IRemoteConfigService" "Remote config service interface exists"
require_grep "StormBlocksUnity/Assets/StormBlocks/Scripts/Services/StormBlocksServices.cs" "interface ICloudSaveService" "Cloud save service interface exists"
require_grep "StormBlocksUnity/Assets/StormBlocks/Scripts/Services/StormBlocksServices.cs" "interface IAudioService" "Audio service interface exists"
require_grep "StormBlocksUnity/Assets/StormBlocks/Scripts/Services/StormBlocksServices.cs" "interface IHapticsService" "Haptics service interface exists"
require_grep "StormBlocksUnity/Assets/StormBlocks/Scripts/Services/StormBlocksServices.cs" "interface IShareService" "Sharing service interface exists"
require_grep "StormBlocksUnity/Assets/StormBlocks/Scripts/Services/StormBlocksServices.cs" "LowDetailMode" "Low Detail setting exists"
require_grep "StormBlocksUnity/Assets/StormBlocks/Scripts/Services/StormBlocksServices.cs" "ColorblindFriendly" "Colorblind-friendly setting exists"
require_grep "StormBlocksUnity/Assets/StormBlocks/Scripts/Services/StormBlocksServices.cs" "ReducedMotion" "Reduced motion setting exists"
require_grep "StormBlocksUnity/Assets/StormBlocks/Scripts/Services/StormBlocksServices.cs" "CosmeticType" "Cosmetic-only types exist"

require_grep "StormBlocksUnity/Assets/StormBlocks/Scripts/Presentation/StormBlocksGameView.cs" "ShowStormTrail" "Storm Trail UI path exists"
require_grep "StormBlocksUnity/Assets/StormBlocks/Scripts/Presentation/StormBlocksGameView.cs" "ShowTempestScreen" "Tempest Trials UI path exists"
require_grep "StormBlocksUnity/Assets/StormBlocks/Scripts/Presentation/StormBlocksGameView.cs" "ShowAccessibility" "Accessibility UI path exists"
require_grep "StormBlocksUnity/Assets/StormBlocks/Scripts/Presentation/StormBlocksGameView.cs" "ShowCredits" "Credits UI path exists"
require_grep "StormBlocksUnity/Assets/StormBlocks/Scripts/Presentation/StormBlocksGameView.cs" "SpawnPushbackFx" "Signature pushback VFX path exists"
require_grep "StormBlocksUnity/Assets/StormBlocks/Scripts/Presentation/StormBlocksGameView.cs" "NearDeath" "Near-death presentation path exists"
require_grep "StormBlocksUnity/Assets/StormBlocks/Scripts/Presentation/StormBlocksGameView.cs" "PrimitiveType.Cube" "3D cube presentation exists"
require_grep "StormBlocksUnity/Assets/StormBlocks/Scripts/Presentation/StormBlocksGameView.cs" "PrimitiveType.Sphere" "3D sphere presentation exists"

require_file "StormBlocksUnity/Assets/StormBlocks/Tests/EditMode/CoreRulesTests.cs" "Core rules tests exist"
require_file "StormBlocksUnity/Assets/StormBlocks/Tests/EditMode/ModeProgressionTests.cs" "Mode/progression tests exist"
require_file "StormBlocksUnity/Assets/StormBlocks/Tests/EditMode/ServiceInterfaceTests.cs" "Service seam tests exist"
require_file "StormBlocksUnity/Assets/StormBlocks/Tests/PlayMode/BootstrapSceneSmokeTests.cs" "PlayMode smoke tests exist"

require_grep "StormBlocksUnity/editmode-results.xml" 'result="Passed" total="25" passed="25" failed="0"' "EditMode test results are passing"
require_grep "StormBlocksUnity/playmode-results.xml" 'result="Passed" total="8" passed="8" failed="0"' "PlayMode test results are passing"
require_grep "Docs/RELEASE_AUDIT.md" "Prompt-to-Artifact Checklist" "Prompt-to-artifact audit exists"
require_grep "Docs/RELEASE_AUDIT.md" "not final-release complete" "Release audit preserves incomplete external-gate conclusion"

require_absent_grep "StormBlocksUnity/Packages/manifest.json" 'com\.unity\.ads|com\.unity\.purchasing|admob|applovin|ironsource|adjust|appsflyer|firebase' "No ad/IAP/attribution SDK package is present"
require_repo_absent "Runtime source contains no clone/copyrighted-IP keywords" 'Block Blast|Tetris|Minecraft|Roblox|Disney|Marvel|Pokemon|Candy Crush' \
  "StormBlocksUnity/Assets/StormBlocks/Scripts" "fastlane/metadata"
require_repo_absent "Runtime source contains no ad/IAP implementation APIs" 'UnityEngine\.Purchasing|IStoreController|ProductType|UnityAds|Advertisement|RewardedAd|InterstitialAd|AdRequest' \
  "StormBlocksUnity/Assets/StormBlocks/Scripts" "StormBlocksUnity/Assets/Plugins"

printf '\nPrompt compliance summary: %d pass, 0 fail\n' "$pass_count"
