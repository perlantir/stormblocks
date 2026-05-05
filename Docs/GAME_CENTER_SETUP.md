# Game Center Setup

Storm Blocks has runtime Game Center-ready interfaces and a native iOS GameKit bridge. Local mocks still back tests and editor play, so gameplay remains credential-independent.

## Runtime Files

- Service contracts and local mocks: `StormBlocksUnity/Assets/StormBlocks/Scripts/Services/StormBlocksServices.cs`
- Unity/iOS adapter: `StormBlocksUnity/Assets/StormBlocks/Scripts/Presentation/UnityGameCenterServices.cs`
- Native GameKit bridge: `StormBlocksUnity/Assets/Plugins/iOS/StormBlocksGameKitBridge.mm`
- iOS framework post-process: `StormBlocksUnity/Assets/StormBlocks/Editor/StormBlocksBuildPipeline.cs`
- Exported entitlement: `StormBlocksUnity/Builds/iOS/StormBlocks/StormBlocks.entitlements`

## Leaderboards

Create these identifiers in App Store Connect Game Center:

- `com.perlantir.stormblocks.leaderboard.endless_high_score`
- `com.perlantir.stormblocks.leaderboard.daily_storm`
- `com.perlantir.stormblocks.leaderboard.tempest_trials_weekly`

The machine-readable setup manifest lives at `Docs/APP_STORE_CONNECT_MANIFEST.json` and includes reference names, display names, score sort order, and score format for these leaderboards.

## Achievements

Create these identifiers in App Store Connect Game Center:

- `com.perlantir.stormblocks.achievement.first_rescue`
- `com.perlantir.stormblocks.achievement.first_pushback`
- `com.perlantir.stormblocks.achievement.clutch_save`
- `com.perlantir.stormblocks.achievement.daily_streak_3`
- `com.perlantir.stormblocks.achievement.daily_streak_7`
- `com.perlantir.stormblocks.achievement.storm_trail_region_complete`
- `com.perlantir.stormblocks.achievement.tempest_trial_complete`
- `com.perlantir.stormblocks.achievement.cosmetic_collector`

The manifest also includes achievement reference names, display titles, pre-earned/earned descriptions, point values, and hidden/repeatable flags.

## Verification

Current local proof:

- EditMode tests pass with mock services.
- PlayMode smoke tests confirm the profile leaderboard and achievements Game Center entry points exist.
- Unity iOS export succeeds and includes `GameKit.framework`.
- Unity iOS export generates `StormBlocks.entitlements` with `com.apple.developer.game-center = true`.
- Exported Xcode build settings include `CODE_SIGN_ENTITLEMENTS = StormBlocks.entitlements`.
- Unsigned Xcode `Release-iphoneos` build succeeds with `CODE_SIGNING_ALLOWED=NO`.
- Signed Xcode `Release-iphoneos` build succeeds under team `7JL22TDB44`.
- App Store IPA export succeeds with Game Center entitlement and `beta-reports-active = true`.
- `Scripts/verify_release_assets.sh` verifies that every Game Center identifier in `Docs/APP_STORE_CONNECT_MANIFEST.json` appears in both `UnityGameCenterServices.cs` and this setup document.

Credentialed follow-up:

- Enable Game Center for bundle id `com.perlantir.stormblocks`.
- Create the leaderboard and achievement ids above.
- Create the App Store Connect app record for bundle id `com.perlantir.stormblocks`; Xcode upload currently finds zero App Store Connect apps for that bundle id.
- Unlock the paired iPhone, launch the installed signed build, and confirm authentication, score submission, achievement completion, and Game Center UI presentation.
