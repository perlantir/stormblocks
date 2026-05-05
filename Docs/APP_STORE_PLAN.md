# App Store Plan

## Working title

Storm Blocks

Treat as working title until App Store/trademark/domain checks are completed.

## App subtitle ideas

- Rescue Puzzle Challenge
- Block Puzzle Storm Rescue
- Clear Lines. Save Camp.

## Screenshot themes

1. Place blocks to save camp.
2. Beat the Daily Storm.
3. Advance through Storm Trail.
4. Complete the weekly Tempest Trials set.
5. Unlock cozy cosmetic-only profile rewards.

## Current generated asset paths

- App icon source: `StormBlocksUnity/Assets/StormBlocks/Art/Generated/AppIconDraft.png`
- Screenshot directory: `StormBlocksUnity/Builds/AppStoreScreens/`
- Fastlane screenshot package: `fastlane/screenshots/en-US/`
- Fastlane metadata package: `fastlane/metadata/en-US/`
- App Store Connect/Game Center manifest: `Docs/APP_STORE_CONNECT_MANIFEST.json`
- Customer-facing support page draft: `Docs/PUBLIC_SUPPORT.md`
- Customer-facing privacy policy draft: `Docs/PUBLIC_PRIVACY.md`
- Screenshot notes: `Docs/APP_STORE_SCREENSHOTS.md`
- iOS export: `StormBlocksUnity/Builds/iOS/StormBlocks/Unity-iPhone.xcodeproj`

Generated screenshot filenames:

- `01_place_blocks_save_camp.png`
- `02_beat_daily_storm.png`
- `03_storm_trail_progression.png`
- `04_tempest_trials_weekly.png`
- `05_cosmetic_profile.png`

## App Store copy draft

Place blocks, clear lines, and protect your tiny camp from a living storm.

Storm Blocks is a satisfying puzzle game with a rescue twist. Drag colorful blocks onto the board, clear rows and columns, rescue survivors, and blast the storm back before it reaches camp.

Play quick endless runs, challenge yourself in Daily Storm, unlock cozy cosmetics, and chase your best rescue score.

Free to play. No forced ads. No pay-to-win.

## Current iOS readiness note

- Unity iOS export succeeds.
- Unsigned Xcode `Release-iphoneos` build succeeds with `CODE_SIGNING_ALLOWED=NO`.
- Signed Xcode `Release-iphoneos` build succeeds under team `7JL22TDB44`.
- Signed app installs and launches on paired iPhone `907E2EE7-9C7B-5D0D-9EC0-32E69912287D`.
- Xcode archive succeeds at `StormBlocksUnity/Builds/iOS/Archives/StormBlocks-Team7JL.xcarchive`.
- App Store Connect IPA export succeeds at `StormBlocksUnity/Builds/iOS/ExportAppStoreTeam7JL/StormBlocks.ipa`.
- Upload remains blocked because App Store Connect currently returns zero apps for bundle id `com.perlantir.stormblocks`; create the app record under team `7JL22TDB44` / `UBER KIWI LLC`, then upload the exported IPA or rerun the upload export.

## App Store Connect record fields

Use these values when creating the missing app record:

- Platform: iOS.
- Name: `Storm Blocks`.
- Primary language: English (U.S.).
- Bundle ID: `com.perlantir.stormblocks`.
- SKU: `stormblocks-ios`.
- User access: Full Access.
- Team/provider: `7JL22TDB44` / `UBER KIWI LLC`.

After the record exists, rerun:

```bash
Scripts/ios_release_gates.sh upload-probe
```

Fastlane credentialed lanes are also scaffolded:

```bash
Scripts/fastlane_release.sh ios validate_release_assets
Scripts/fastlane_release.sh ios create_app_record
Scripts/fastlane_release.sh ios upload_metadata
Scripts/fastlane_release.sh ios upload_testflight
Scripts/fastlane_release.sh ios release_candidate_upload
```

Before App Store submission, confirm the support, marketing, and privacy URLs in `Docs/APP_STORE_CONNECT_MANIFEST.json` are publicly reachable and acceptable for customer-facing use. Apple documents the current metadata limits and support URL expectations in App Store Connect Help: app name and subtitle are capped at 30 characters, promotional text at 170 characters, description at 4000 characters, and keywords at 100 bytes; support URL must lead to actual contact information.
