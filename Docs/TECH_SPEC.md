# Storm Blocks — Technical Spec

## Engine

Unity 6.x, portrait iOS target, Universal Render Pipeline.

## Project structure

Recommended Unity project folder:

```text
/StormBlocksUnity
  /Assets/StormBlocks
    /Art
      /Materials
      /Meshes
      /Particles
      /Prefabs
      /Shaders
      /Sprites
      /UI
    /Audio
    /Data
      /Configs
      /Cosmetics
      /Levels
      /Modifiers
      /Pieces
    /Scenes
    /Scripts
      /Core
      /Gameplay
      /Presentation
      /Services
      /UI
      /Utilities
    /Tests
      /EditMode
      /PlayMode
  /Packages
  /ProjectSettings
```

## Architecture

### Core layer

Plain C# logic. No Unity scene dependency.

Responsibilities:

- Board model.
- Cell states.
- Piece shapes.
- Placement validation.
- Line clear resolution.
- Storm spread resolution.
- Storm Pushback resolution.
- Survivor rescue scoring.
- Combo scoring.
- Game-over detection.
- Daily seed generation.
- Mode configuration.

### Presentation layer

Unity MonoBehaviours and prefabs.

Responsibilities:

- 3D board view.
- Tile/piece view.
- Drag input.
- Camera.
- UI screens.
- Animations.
- VFX.
- Audio/haptics.

### Service layer

Interfaces first, mock implementations until integrations are ready.

Services:

- `ISaveService`
- `ILeaderboardService`
- `IAchievementService`
- `IChallengeService`
- `IAnalyticsService`
- `IRemoteConfigService`
- `ICloudSaveService`
- `IHapticsService`
- `IAudioService`
- `IShareService`

## Data-driven configs

Use JSON or ScriptableObjects for:

- Piece shape definitions.
- Difficulty curves.
- Storm rules.
- Scoring values.
- Daily modifiers.
- Storm Trail levels.
- Cosmetic unlocks.
- Audio event mappings.
- Haptic event mappings.

## Determinism

Daily Storm must be deterministic.

Given:

- date,
- season version,
- daily rules version,
- player-independent seed,

The game must produce:

- same starting board,
- same piece sequence,
- same storm spread pattern,
- same scoring rules.

## iOS performance targets

Targets:

- 60 FPS on modern iPhones.
- Graceful 30 FPS fallback for older devices.
- Fast app launch.
- No major garbage spikes during placement/clear/storm moments.
- Board VFX pooled.
- No expensive physics for puzzle logic.

## Testing

Edit-mode tests:

- Piece placement.
- Out-of-bounds placement.
- Collision placement.
- Row clears.
- Column clears.
- Multi-line clears.
- Storm spread.
- Storm Pushback.
- Survivor rescue.
- Scoring.
- Game over.
- Daily determinism.
- Save/load roundtrip.

Play-mode smoke tests:

- Main menu loads.
- Endless run starts.
- Player can place a piece.
- Clear animation path triggers.
- Daily Storm starts.
- Results screen appears.

## Integrations

### Game Center

Implement behind service interfaces. Required features:

- Authenticate local player.
- Submit Endless high score.
- Submit Daily Storm score.
- Achievements.
- Friend challenge/share flow stub.

### Firebase / analytics / crash

Optional but recommended for production readiness:

- Crash reporting.
- Analytics events.
- Remote config for balance.
- Cloud save if desired.

Do not let Firebase calls enter core gameplay logic directly.
