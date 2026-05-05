# Privacy Review

Current review date: 2026-05-05.

## Current SDK / Package State

Installed Unity packages include URP, Input System, Addressables, UGUI, and Unity Test Framework. The runtime UI uses built-in Unity `Text` with a font fallback and does not directly depend on TextMeshPro. There are no ad SDKs, IAP packages, attribution SDKs, Firebase, Facebook, AppLovin, AdMob, Adjust, AppsFlyer, or external analytics SDKs in `StormBlocksUnity/Packages/manifest.json`.

Unity Services state in `ProjectSettings/UnityConnectSettings.asset`:

- Unity Connect: disabled.
- Unity Analytics: disabled.
- Unity Ads: disabled.
- Unity Purchasing: disabled.
- Cloud Diagnostics / Performance Reporting: disabled.

## Permissions

The exported `Info.plist` currently does not declare camera, microphone, location, contacts, photo library, Bluetooth, tracking, or user notification permission strings.

Current native iOS integrations:

- Game Center via GameKit.
- Share sheet via `UIActivityViewController`.

These use system UI and do not require app-declared privacy permission strings in the current implementation.

## Data Stored Locally

Storm Blocks stores local profile/save data through `FileSaveService` under Unity's persistent data path:

- Profile totals and best scores.
- Daily Storm history.
- Tempest Trials history.
- Storm Trail stars.
- Cosmetic unlock/equip state.
- Accessibility/settings preferences.
- Run snapshot for resume.

No current production service uploads this profile automatically. Cloud save remains interface-defined only until a credentialed implementation is selected.

## App Store Privacy Draft

Current draft posture:

- No third-party advertising.
- No tracking.
- No paid power, loot boxes, gacha, paid speedups, or forced ads.
- No App Tracking Transparency prompt.
- No personal data collection by bundled SDKs in the current build.
- Game Center data is handled by Apple's Game Center when the user signs in.
- User-initiated sharing sends only the generated share text/card the player chooses to share.

Credentialed follow-up before submission:

- Re-review after adding any production analytics, crash reporting, cloud save, remote config, or App Store Connect services.
- Confirm App Store privacy nutrition labels after the final service set is fixed.
- Run a signed physical-device build and check that no unexpected permission prompts appear.
