using System;
using System.IO;
using NUnit.Framework;
using StormBlocks.Core;
using StormBlocks.Services;

namespace StormBlocks.Tests.EditMode
{
    public sealed class ModeProgressionTests
    {
        [Test]
        public void DailyStormDefinitionIsDeterministicAndOfficialFlagControlsLeaderboardEligibility()
        {
            DateTime date = new DateTime(2026, 5, 5, 12, 0, 0, DateTimeKind.Utc);

            var official = ModeConfigFactory.CreateDaily(date, true);
            var practice = ModeConfigFactory.CreateDaily(date, false);

            Assert.AreEqual("2026-05-05", official.DateKey);
            Assert.AreEqual(official.Mode.Seed, practice.Mode.Seed);
            Assert.AreEqual(official.Mode.Modifier, practice.Mode.Modifier);
            Assert.IsTrue(official.Mode.IsOfficialAttempt);
            Assert.IsTrue(official.Mode.IsLeaderboardEligible);
            Assert.IsFalse(practice.Mode.IsOfficialAttempt);
            Assert.IsFalse(practice.Mode.IsLeaderboardEligible);
            Assert.AreEqual(GameModeId.DailyStorm, official.Mode.Config.Mode);
        }

        [Test]
        public void StormTrailCatalogContainsTwelveRegionsAndOneHundredTwentyLevels()
        {
            var catalog = ModeConfigFactory.CreateStormTrailCatalog();
            int levels = 0;

            Assert.AreEqual(12, catalog.Count);
            for (int i = 0; i < catalog.Count; i++)
            {
                Assert.AreEqual(10, catalog[i].Levels.Count);
                levels += catalog[i].Levels.Count;
                Assert.IsFalse(string.IsNullOrEmpty(catalog[i].Levels[9].CosmeticRewardId));
                Assert.AreEqual(GameModeId.StormTrail, catalog[i].Levels[0].Config.Mode);
            }

            Assert.AreEqual(120, levels);
        }

        [Test]
        public void TempestWeekBuildsFiveDeterministicChallengeRuns()
        {
            DateTime thursday = new DateTime(2026, 5, 7, 18, 0, 0, DateTimeKind.Utc);
            DateTime saturdaySameWeek = new DateTime(2026, 5, 9, 18, 0, 0, DateTimeKind.Utc);

            var first = ModeConfigFactory.CreateTempestWeek(thursday);
            var second = ModeConfigFactory.CreateTempestWeek(saturdaySameWeek);

            Assert.AreEqual("2026-05-04", first.WeekKey);
            Assert.AreEqual(first.WeekKey, second.WeekKey);
            Assert.AreEqual(first.Seed, second.Seed);
            Assert.AreEqual(5, first.Runs.Count);
            for (int i = 0; i < first.Runs.Count; i++)
            {
                Assert.AreEqual(first.Runs[i].Seed, second.Runs[i].Seed);
                Assert.AreEqual(GameModeId.TempestTrial, first.Runs[i].Config.Mode);
                Assert.Greater(first.Runs[i].Targets.ThreeStarScore, first.Runs[i].Targets.TwoStarScore);
            }
        }

        [Test]
        public void ProfileProgressionKeepsOneOfficialDailyScoreAndUpdatesStreak()
        {
            var profile = new PlayerProfile();
            var services = new MockGameServices();
            ulong seed = ModeConfigFactory.CreateDaily(new DateTime(2026, 5, 5, 1, 0, 0, DateTimeKind.Utc), true).Mode.Seed;

            ProfileProgression.ApplyRunSummary(profile, DailySummary(seed, 1000, true), new DateTime(2026, 5, 5, 8, 0, 0, DateTimeKind.Utc), services, services);
            ProfileProgression.ApplyRunSummary(profile, DailySummary(seed, 4000, true), new DateTime(2026, 5, 5, 18, 0, 0, DateTimeKind.Utc), services, services);
            ProfileProgression.ApplyRunSummary(profile, DailySummary(seed + 1UL, 1500, true), new DateTime(2026, 5, 6, 8, 0, 0, DateTimeKind.Utc), services, services);
            ProfileProgression.ApplyRunSummary(profile, DailySummary(seed + 2UL, 1600, true), new DateTime(2026, 5, 7, 8, 0, 0, DateTimeKind.Utc), services, services);

            Assert.AreEqual(3, profile.DailyStreak);
            Assert.AreEqual(4000, profile.BestDailyScore);
            Assert.AreEqual(2, profile.DailyHistory["2026-05-05"].Attempts);
            Assert.AreEqual(1000, profile.DailyHistory["2026-05-05"].OfficialScore);
            Assert.AreEqual(3, services.GetLocalScores(LeaderboardId.DailyStorm).Count);
            Assert.IsTrue(profile.UnlockedCosmetics.Contains("banner_daily_glow"));
            Assert.IsTrue(services.IsCompleted(AchievementId.DailyStreak3));
        }

        [Test]
        public void CosmeticsAreProgressionOnlyAndEquipRequiresUnlock()
        {
            var profile = new PlayerProfile();
            CosmeticCatalog.EnsureDefaultCosmetics(profile);
            var catalog = CosmeticCatalog.CreateLaunchCatalog();

            for (int i = 0; i < catalog.Count; i++)
            {
                Assert.IsFalse(catalog[i].AffectsGameplay);
            }

            var locked = catalog[10];
            Assert.Throws<InvalidOperationException>(() => ProfileProgression.EquipCosmetic(profile, locked));
            profile.UnlockedCosmetics.Add(locked.Id);
            ProfileProgression.EquipCosmetic(profile, locked);
            Assert.AreEqual(locked.Id, profile.EquippedCosmetics[locked.Type]);
        }

        [Test]
        public void StormTrailStarsUnlockRegionReward()
        {
            var profile = new PlayerProfile();
            var services = new MockGameServices();
            var region = ModeConfigFactory.CreateStormTrailCatalog()[0];
            int finalStars = 0;

            for (int i = 0; i < region.Levels.Count; i++)
            {
                var level = region.Levels[i];
                var summary = new RunSummary
                {
                    Mode = GameModeId.StormTrail,
                    Score = level.Goals.ThreeStarScore,
                    SurvivorsRescued = level.Goals.SurvivorTarget,
                    StormTilesDestroyed = level.Goals.PushbackTarget
                };
                finalStars = ProfileProgression.ApplyStormTrailResult(profile, level, summary, services);
            }

            Assert.AreEqual(3, finalStars);
            Assert.IsTrue(profile.UnlockedCosmetics.Contains(region.Levels[9].CosmeticRewardId));
            Assert.IsTrue(services.IsCompleted(AchievementId.StormTrailRegionComplete));
        }

        [Test]
        public void TempestTrialsTrackFiveRunWeeklyTotalAndUnlockCosmeticBadge()
        {
            var profile = new PlayerProfile();
            var services = new MockGameServices();
            var week = ModeConfigFactory.CreateTempestWeek(new DateTime(2026, 5, 7, 18, 0, 0, DateTimeKind.Utc));
            int weeklyTotal = 0;

            for (int i = 0; i < week.Runs.Count; i++)
            {
                var run = week.Runs[i];
                var summary = new RunSummary
                {
                    Mode = GameModeId.TempestTrial,
                    Seed = run.Seed,
                    Score = 1000 + i * 100,
                    SurvivorsRescued = i + 1,
                    StormTilesDestroyed = i + 2,
                    ShareToken = "tempest-" + i
                };

                weeklyTotal = ProfileProgression.ApplyTempestTrialResult(profile, week, run, summary, services, services);
            }

            Assert.AreEqual(6000, weeklyTotal);
            Assert.AreEqual(6000, profile.BestTempestWeeklyScore);
            Assert.IsTrue(ProfileProgression.IsTempestWeekComplete(profile, week));
            Assert.IsTrue(profile.UnlockedCosmetics.Contains(week.CosmeticBadgeRewardId));
            Assert.IsTrue(profile.UnlockedCosmetics.Contains("badge_tempest_weekly"));
            Assert.IsTrue(services.IsCompleted(AchievementId.TempestTrialComplete));
            Assert.AreEqual(1, services.GetLocalScores(LeaderboardId.TempestTrialsWeekly).Count);
        }

        [Test]
        public void ProfileCodecPersistsAccessibilitySettingsAndTempestHistory()
        {
            var profile = new PlayerProfile();
            profile.Settings.MusicEnabled = false;
            profile.Settings.EffectsEnabled = true;
            profile.Settings.HapticsEnabled = false;
            profile.Settings.MasterVolume = 0.42f;
            profile.Settings.ReducedMotion = true;
            profile.Settings.HighContrast = true;
            profile.Settings.ColorblindFriendly = true;
            profile.Settings.LeftHandedMode = true;
            profile.Settings.LargeText = true;
            profile.Settings.LowDetailMode = true;

            string key = ProfileProgression.TempestRecordKey("2026-05-04", "2026-05-04_run_1");
            profile.TempestTrialHistory[key] = new TempestTrialRecord
            {
                WeekKey = "2026-05-04",
                RunId = "2026-05-04_run_1",
                Score = 1234,
                SurvivorsRescued = 3,
                StormTilesDestroyed = 5,
                Completed = true
            };

            var restored = ProfileCodec.FromPayload(ProfileCodec.ToPayload(profile));

            Assert.IsFalse(restored.Settings.MusicEnabled);
            Assert.IsTrue(restored.Settings.EffectsEnabled);
            Assert.IsFalse(restored.Settings.HapticsEnabled);
            Assert.AreEqual(0.42f, restored.Settings.MasterVolume, 0.001f);
            Assert.IsTrue(restored.Settings.ReducedMotion);
            Assert.IsTrue(restored.Settings.HighContrast);
            Assert.IsTrue(restored.Settings.ColorblindFriendly);
            Assert.IsTrue(restored.Settings.LeftHandedMode);
            Assert.IsTrue(restored.Settings.LargeText);
            Assert.IsTrue(restored.Settings.LowDetailMode);
            Assert.IsTrue(restored.TempestTrialHistory.ContainsKey(key));
            Assert.AreEqual(1234, restored.TempestTrialHistory[key].Score);
        }

        [Test]
        public void FileSaveServiceRoundTripsProfileAndRunSnapshot()
        {
            string folder = Path.Combine(Path.GetTempPath(), "StormBlocksTests_" + Guid.NewGuid().ToString("N"));
            try
            {
                var service = new FileSaveService(folder);
                var profile = new PlayerProfile
                {
                    TotalRuns = 12,
                    BestEndlessScore = 9876,
                    DailyStreak = 4,
                    LastDailyDateKey = "2026-05-05"
                };
                profile.UnlockedCosmetics.Add("block_lantern_glass");
                profile.StormTrailStars["region_01_level_01"] = 3;
                profile.CompletedAchievements.Add(AchievementId.FirstPushback);

                service.SaveProfile(profile);
                var restored = service.LoadProfile();

                Assert.AreEqual(12, restored.TotalRuns);
                Assert.AreEqual(9876, restored.BestEndlessScore);
                Assert.AreEqual(4, restored.DailyStreak);
                Assert.IsTrue(restored.UnlockedCosmetics.Contains("block_lantern_glass"));
                Assert.AreEqual(3, restored.StormTrailStars["region_01_level_01"]);
                Assert.IsTrue(restored.CompletedAchievements.Contains(AchievementId.FirstPushback));

                var engine = new StormRunEngine(DefaultPieceLibrary.Create());
                var state = engine.StartRun(new RunConfig { StormRules = new StormRulesConfig { InitialStormRingThickness = 0 } }, 42UL);
                service.SaveRunSnapshot(StormRunSnapshot.FromState(state));

                Assert.IsTrue(service.TryLoadRunSnapshot(out var snapshot));
                Assert.AreEqual(42UL, snapshot.Seed);
                service.ClearRunSnapshot();
                Assert.IsFalse(service.TryLoadRunSnapshot(out snapshot));
            }
            finally
            {
                if (Directory.Exists(folder))
                {
                    Directory.Delete(folder, true);
                }
            }
        }

        private static RunSummary DailySummary(ulong seed, int score, bool official)
        {
            return new RunSummary
            {
                Mode = GameModeId.DailyStorm,
                Seed = seed,
                Score = score,
                SurvivorsRescued = 1,
                StormTilesDestroyed = 1,
                BestCombo = 2,
                ClutchSaves = 0,
                IsOfficialDailyScore = official,
                ShareToken = "daily-" + score
            };
        }
    }
}
