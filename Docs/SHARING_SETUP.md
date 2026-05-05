# Sharing Setup

Storm Blocks now has a runtime share service with editor fallback and iOS native share-sheet support.

## Runtime Files

- Share service contract: `StormBlocksUnity/Assets/StormBlocks/Scripts/Services/StormBlocksServices.cs`
- Unity share implementation: `StormBlocksUnity/Assets/StormBlocks/Scripts/Presentation/UnityShareService.cs`
- Native iOS share bridge: `StormBlocksUnity/Assets/Plugins/iOS/StormBlocksShareBridge.mm`

## Behavior

- Editor and non-iOS builds copy share text to the system clipboard.
- iOS builds generate a local PNG share card at runtime and present `UIActivityViewController`.
- Share text includes mode, score, survivors rescued, storm tiles pushed back, and the deterministic share token.

## Verification

Current local proof:

- PlayMode smoke tests confirm the Unity share service is present in the playable scene.
- Unity iOS export includes `Libraries/Plugins/iOS/StormBlocksShareBridge.mm`.
- Unsigned Xcode `iphoneos` build succeeds with `CODE_SIGNING_ALLOWED=NO`.

Credentialed follow-up:

- Run a signed physical-device build.
- Complete an actual run, tap Share, and verify the iOS share sheet presents the generated PNG and share text.
