# Storm Blocks Fastlane Release Handoff

These lanes are for credentialed App Store Connect work after the local Unity/Xcode gates pass.

## Required Credentials

Preferred App Store Connect API key environment:

```bash
export APP_STORE_CONNECT_API_KEY_KEY_ID="..."
export APP_STORE_CONNECT_API_KEY_ISSUER_ID="..."
export APP_STORE_CONNECT_API_KEY_KEY_FILEPATH="/path/to/AuthKey_XXXX.p8"
```

Apple ID fallback:

```bash
export STORMBLOCKS_APPLE_ID="name@example.com"
export FASTLANE_PASSWORD="..."
```

For upload-only lanes, `FASTLANE_APPLE_APPLICATION_SPECIFIC_PASSWORD` can be used with the Apple ID path. Creating the missing App Store Connect app record may require an App Store Connect API key or an interactive Fastlane Apple ID session.

## Lanes

Create the missing App Store Connect app record:

```bash
fastlane ios create_app_record
```

Upload the exported IPA:

```bash
fastlane ios upload_testflight
```

Create the app record and upload:

```bash
fastlane ios release_candidate_upload
```

The IPA path is `StormBlocksUnity/Builds/iOS/ExportAppStoreTeam7JL/StormBlocks.ipa`.
