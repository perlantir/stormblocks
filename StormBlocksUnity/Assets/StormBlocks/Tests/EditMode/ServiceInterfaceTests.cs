using NUnit.Framework;
using StormBlocks.Core;
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
    }
}
