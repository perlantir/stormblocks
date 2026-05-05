using NUnit.Framework;
using StormBlocks.Core;
using StormBlocks.Gameplay;
using StormBlocks.Services;

namespace StormBlocks.Tests.EditMode
{
    public sealed class ServiceInterfaceTests
    {
        [Test]
        public void MockServicesKeepLocalPlayIndependentFromExternalCredentials()
        {
            var services = new MockGameServices();
            var profile = new PlayerProfile { BestEndlessScore = 1200 };
            services.SaveProfile(profile);

            Assert.IsTrue(services.HasProfile());
            Assert.AreEqual(1200, services.LoadProfile().BestEndlessScore);

            services.SubmitScore(new LeaderboardScore
            {
                LeaderboardId = LeaderboardId.EndlessHighScore,
                Score = 1200,
                Context = "local"
            });

            services.ReportProgress(new AchievementProgress
            {
                AchievementId = AchievementId.FirstPushback,
                PercentComplete = 100.0
            });

            Assert.AreEqual(1, services.GetLocalScores(LeaderboardId.EndlessHighScore).Count);
            Assert.IsTrue(services.IsCompleted(AchievementId.FirstPushback));
        }

        [Test]
        public void MockServicesRoundTripRunSnapshot()
        {
            var services = new MockGameServices();
            var engine = new StormRunEngine(DefaultPieceLibrary.Create());
            var state = engine.StartRun(new RunConfig { StormRules = new StormRulesConfig { InitialStormRingThickness = 0 } }, 55UL);
            var snapshot = StormRunSnapshot.FromState(state);

            services.SaveRunSnapshot(snapshot);

            Assert.IsTrue(services.TryLoadRunSnapshot(out var restored));
            Assert.AreEqual(snapshot.Seed, restored.Seed);

            services.ClearRunSnapshot();
            Assert.IsFalse(services.TryLoadRunSnapshot(out restored));
        }

        [Test]
        public void MockServicesCoverCredentialDependentInterfaces()
        {
            var services = new MockGameServices();
            var profile = new PlayerProfile { TotalRuns = 7, BestDailyScore = 3400 };
            var summary = new RunSummary
            {
                Mode = GameModeId.DailyStorm,
                Seed = 99UL,
                Score = 3400,
                SurvivorsRescued = 5
            };

            services.SetRemoteString("daily_modifier", "gusty");
            services.SetRemoteInt("storm_pushback_bonus", 2);
            services.SetRemoteFloat("storm_speed", 1.25f);
            services.UploadProfile(profile);
            services.Track(new AnalyticsEvent("qa_event", new System.Collections.Generic.Dictionary<string, string>
            {
                { "mode", "daily" }
            }));
            services.Play(AudioEventId.StormPushback);
            services.Play(HapticEventId.SuccessBurst);
            services.ShareRun(summary);

            Assert.IsTrue(services.TryGetString("daily_modifier", out var modifier));
            Assert.AreEqual("gusty", modifier);
            Assert.IsTrue(services.TryGetInt("storm_pushback_bonus", out var bonus));
            Assert.AreEqual(2, bonus);
            Assert.IsTrue(services.TryGetFloat("storm_speed", out var speed));
            Assert.AreEqual(1.25f, speed);
            Assert.IsTrue(services.TryDownloadProfile(out var restored));
            Assert.AreEqual(7, restored.TotalRuns);
            Assert.AreEqual(1, services.AnalyticsEvents.Count);
            Assert.AreEqual(1, services.AudioEvents.Count);
            Assert.AreEqual(1, services.HapticEvents.Count);
            Assert.AreEqual(3400, services.LastSharedRun.Score);
        }

        [Test]
        public void RunSessionEmitsRescueAndNearDeathFeedbackHooks()
        {
            var single = new PieceDefinition("single", new[] { new GridPosition(0, 0) });
            var services = new MockGameServices();
            var session = new StormRunSession(new[] { single }, services, services, services);
            var config = new RunConfig
            {
                QueueSize = 1,
                StormRules = new StormRulesConfig
                {
                    BoardSize = 4,
                    InitialStormRingThickness = 0,
                    WarningBeforeSpread = false
                }
            };

            var state = session.Start(config, 8081UL);
            for (int x = 0; x < 3; x++)
            {
                state.Board.SetOccupant(new GridPosition(x, 0), CellOccupant.Block, "setup");
            }

            state.Board.SetSurvivor(new GridPosition(3, 0), true);
            var rescue = session.PlaceQueuedPiece(0, new GridPosition(3, 0));

            Assert.IsTrue(rescue.Success, rescue.FailureReason);
            CollectionAssert.Contains(services.AudioEvents, AudioEventId.ValidPlacement);
            CollectionAssert.Contains(services.AudioEvents, AudioEventId.LineClear);
            CollectionAssert.Contains(services.AudioEvents, AudioEventId.SurvivorRescued);
            CollectionAssert.Contains(services.HapticEvents, HapticEventId.MediumPlacement);
            CollectionAssert.Contains(services.HapticEvents, HapticEventId.HeavyClear);
            CollectionAssert.Contains(services.HapticEvents, HapticEventId.SuccessBurst);

            services.AudioEvents.Clear();
            services.HapticEvents.Clear();
            var nearDeathSession = new StormRunSession(new[] { single }, services, services, services);
            var nearDeath = nearDeathSession.Start(config, 9091UL);
            nearDeath.Board.SetOccupant(new GridPosition(0, 1), CellOccupant.Storm, string.Empty);

            var warning = nearDeathSession.PlaceQueuedPiece(0, new GridPosition(3, 3));

            Assert.IsTrue(warning.Success, warning.FailureReason);
            CollectionAssert.Contains(services.AudioEvents, AudioEventId.NearDeathLoop);
            CollectionAssert.Contains(services.HapticEvents, HapticEventId.LongNearDeathWarning);
        }
    }
}
