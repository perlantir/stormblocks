using System.Collections.Generic;
using StormBlocks.Core;
using StormBlocks.Services;

namespace StormBlocks.Gameplay
{
    public sealed class StormRunSession
    {
        private readonly StormRunEngine _engine;
        private readonly IAudioService _audio;
        private readonly IHapticsService _haptics;
        private readonly IAnalyticsService _analytics;

        public StormRunSession(
            IReadOnlyList<PieceDefinition> pieceLibrary,
            IAudioService audio,
            IHapticsService haptics,
            IAnalyticsService analytics)
        {
            _engine = new StormRunEngine(pieceLibrary);
            _audio = audio;
            _haptics = haptics;
            _analytics = analytics;
        }

        public StormRunState State { get; private set; }

        public StormRunState Start(RunConfig config, ulong seed)
        {
            State = _engine.StartRun(config, seed);
            _analytics.Track(new AnalyticsEvent("run_started", new Dictionary<string, string>
            {
                { "mode", config.Mode.ToString() },
                { "seed", seed.ToString() }
            }));
            return State;
        }

        public PlacementResult PlaceQueuedPiece(int queueIndex, GridPosition origin)
        {
            var result = _engine.TryPlacePiece(State, queueIndex, origin);
            if (!result.Success)
            {
                _audio.Play(AudioEventId.InvalidPlacement);
                _haptics.Play(HapticEventId.LightTap);
                return result;
            }

            _audio.Play(AudioEventId.ValidPlacement);
            _haptics.Play(HapticEventId.MediumPlacement);

            if (result.Clear.Lines.Count > 0)
            {
                _audio.Play(result.Clear.Lines.Count > 1 ? AudioEventId.Combo : AudioEventId.LineClear);
                _haptics.Play(HapticEventId.HeavyClear);
            }

            if (result.Clear.SurvivorsRescuedAt.Count > 0)
            {
                _audio.Play(AudioEventId.SurvivorRescued);
                _haptics.Play(HapticEventId.SuccessBurst);
            }

            if (result.Clear.AutomaticPushbackTriggered)
            {
                _audio.Play(result.Clear.ClutchSave ? AudioEventId.ClutchSave : AudioEventId.StormPushback);
                _haptics.Play(HapticEventId.SuccessBurst);
            }

            if (result.StormSpread.WarningCells.Count > 0)
            {
                _audio.Play(AudioEventId.StormWarning);
                _haptics.Play(HapticEventId.WarningPulse);
            }

            if (result.StormSpread.SpreadResolved)
            {
                _audio.Play(AudioEventId.StormSpread);
            }

            if (!result.GameOver && StormResolver.IsNearDeath(State.Board, State.Config.StormRules))
            {
                _audio.Play(AudioEventId.NearDeathLoop);
                _haptics.Play(HapticEventId.LongNearDeathWarning);
            }

            if (result.GameOver)
            {
                _audio.Play(AudioEventId.GameOver);
            }

            return result;
        }
    }
}
