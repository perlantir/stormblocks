# Unity Setup and Recommended Packages

## Required setup

- Unity 6.x with iOS Build Support.
- Universal Render Pipeline project.
- Portrait iPhone target.
- Git repo initialized before major Codex work.

## Required Unity packages

Use Unity Package Manager where possible:

- Universal Render Pipeline.
- Input System.
- Addressables.
- Unity Test Framework.
- Localization package if final text/localization is added.
- 2D Sprite package only if sprite-based UI/effects need it.

## Recommended implementation approach

Use URP for the stylized 3D board, mobile lighting, bloom, shader graph effects, and performance-tunable render settings.

Use Input System for touch drag/drop.

Use bold rounded typography that matches the art direction. The current runtime uses built-in Unity `Text` with a font fallback so automated captures and batch builds do not depend on TextMeshPro import state; TextMeshPro can be reintroduced later only if it is verified in the same build/capture pipeline.

Use Addressables for cosmetic/level/content management if the game grows.

Use Unity Test Framework for deterministic logic tests.

## Game Center

Use service interfaces first.

When integrating for iOS, use Apple GameKit / Apple Unity Plug-ins or an approved Game Center integration path. Keep mocks available so development and tests do not depend on Apple credentials.

## Firebase / analytics / crash reporting

Optional for production readiness:

- Firebase Analytics.
- Firebase Crashlytics.
- Firebase Remote Config.
- Firebase Cloud Firestore or Cloud Save only if needed.

Use interfaces and mocks before real SDK integration.

## Optional external plugins

Only add if needed and license-safe:

- DOTween Free for UI/gameplay tweening, or implement a custom tween runner if avoiding dependencies.
- No paid art packs unless the user explicitly adds them.
- No ad SDKs in the initial full game unless explicitly requested later.

## Avoid

- Heavy photorealistic assets.
- Expensive realtime shadows everywhere.
- Large unoptimized particle systems.
- Full-screen transparent overdraw.
- Paid asset dependencies that Codex cannot access.
- Monetization SDKs that distract from launch gameplay.
