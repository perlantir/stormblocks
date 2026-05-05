using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using StormBlocks.Core;

namespace StormBlocks.Services
{
    public sealed class CosmeticDefinition
    {
        public string Id = string.Empty;
        public CosmeticType Type;
        public string DisplayName = string.Empty;
        public string UnlockSource = string.Empty;

        public bool AffectsGameplay
        {
            get { return false; }
        }
    }

    public static class CosmeticCatalog
    {
        public static IReadOnlyList<CosmeticDefinition> CreateLaunchCatalog()
        {
            return new[]
            {
                Cosmetic("block_toy_classic", CosmeticType.BlockSkin, "Toy Classic", "default"),
                Cosmetic("camp_warm_canvas", CosmeticType.CampSkin, "Warm Canvas Camp", "default"),
                Cosmetic("survivor_raincoat_yellow", CosmeticType.SurvivorOutfit, "Yellow Raincoat", "default"),
                Cosmetic("storm_moonlit", CosmeticType.StormSkin, "Moonlit Storm", "default"),
                Cosmetic("pushback_sunburst", CosmeticType.PushbackVfx, "Sunburst Pushback", "default"),
                Cosmetic("banner_first_camp", CosmeticType.ProfileBanner, "First Camp", "default"),
                Cosmetic("frame_rescue_map", CosmeticType.ResultFrame, "Rescue Map", "daily"),
                Cosmetic("banner_daily_glow", CosmeticType.ProfileBanner, "Daily Glow", "daily_streak_3"),
                Cosmetic("banner_weeklong_watch", CosmeticType.ProfileBanner, "Weeklong Watch", "daily_streak_7"),
                Cosmetic("badge_tempest_weekly", CosmeticType.TempestBadge, "Tempest Weekly", "tempest"),
                Cosmetic("block_lantern_glass", CosmeticType.BlockSkin, "Lantern Glass", "storm_trail"),
                Cosmetic("camp_signal_fire", CosmeticType.CampSkin, "Signal Fire", "storm_trail"),
                Cosmetic("storm_neon_tempest", CosmeticType.StormSkin, "Neon Tempest", "storm_trail"),
                Cosmetic("pushback_comet_arc", CosmeticType.PushbackVfx, "Comet Arc", "storm_trail")
            };
        }

        public static void EnsureDefaultCosmetics(PlayerProfile profile)
        {
            foreach (var cosmetic in CreateLaunchCatalog())
            {
                if (cosmetic.UnlockSource != "default")
                {
                    continue;
                }

                profile.UnlockedCosmetics.Add(cosmetic.Id);
                if (!profile.EquippedCosmetics.ContainsKey(cosmetic.Type))
                {
                    profile.EquippedCosmetics[cosmetic.Type] = cosmetic.Id;
                }
            }
        }

        private static CosmeticDefinition Cosmetic(string id, CosmeticType type, string displayName, string source)
        {
            return new CosmeticDefinition
            {
                Id = id,
                Type = type,
                DisplayName = displayName,
                UnlockSource = source
            };
        }
    }

    public static class ProfileProgression
    {
        public static void ApplyRunSummary(
            PlayerProfile profile,
            RunSummary summary,
            DateTime utcNow,
            ILeaderboardService leaderboard,
            IAchievementService achievements)
        {
            if (profile == null)
            {
                throw new ArgumentNullException("profile");
            }

            if (summary == null)
            {
                throw new ArgumentNullException("summary");
            }

            CosmeticCatalog.EnsureDefaultCosmetics(profile);
            profile.TotalRuns++;
            profile.TotalSurvivorsRescued += summary.SurvivorsRescued;
            profile.TotalStormTilesDestroyed += summary.StormTilesDestroyed;

            if (summary.Mode == GameModeId.EndlessStorm)
            {
                if (summary.Score > profile.BestEndlessScore)
                {
                    profile.BestEndlessScore = summary.Score;
                    leaderboard?.SubmitScore(new LeaderboardScore
                    {
                        LeaderboardId = LeaderboardId.EndlessHighScore,
                        Score = summary.Score,
                        Context = summary.ShareToken
                    });
                }
            }
            else if (summary.Mode == GameModeId.DailyStorm)
            {
                ApplyDailyResult(profile, summary, utcNow, leaderboard);
            }
            else if (summary.Mode == GameModeId.TempestTrial)
            {
                if (summary.Score > profile.BestTempestWeeklyScore)
                {
                    profile.BestTempestWeeklyScore = summary.Score;
                    leaderboard?.SubmitScore(new LeaderboardScore
                    {
                        LeaderboardId = LeaderboardId.TempestTrialsWeekly,
                        Score = summary.Score,
                        Context = summary.ShareToken
                    });
                }

                Unlock(profile, "badge_tempest_weekly");
                achievements?.ReportProgress(new AchievementProgress { AchievementId = AchievementId.TempestTrialComplete, PercentComplete = 100.0 });
            }

            ReportMetricAchievements(profile, summary, achievements);
        }

        public static int ApplyStormTrailResult(
            PlayerProfile profile,
            StormTrailLevelDefinition level,
            RunSummary summary,
            IAchievementService achievements)
        {
            if (profile == null)
            {
                throw new ArgumentNullException("profile");
            }

            if (level == null)
            {
                throw new ArgumentNullException("level");
            }

            CosmeticCatalog.EnsureDefaultCosmetics(profile);
            profile.TotalRuns++;
            profile.TotalSurvivorsRescued += summary.SurvivorsRescued;
            profile.TotalStormTilesDestroyed += summary.StormTilesDestroyed;

            int stars = level.Goals.Evaluate(summary.Score, summary.SurvivorsRescued, summary.StormTilesDestroyed);
            if (!profile.StormTrailStars.TryGetValue(level.Id, out int existing) || stars > existing)
            {
                profile.StormTrailStars[level.Id] = stars;
            }

            if (stars > 0 && !string.IsNullOrEmpty(level.CosmeticRewardId))
            {
                Unlock(profile, level.CosmeticRewardId);
            }

            if (stars > 0 && IsRegionComplete(profile, level.RegionId))
            {
                achievements?.ReportProgress(new AchievementProgress { AchievementId = AchievementId.StormTrailRegionComplete, PercentComplete = 100.0 });
            }

            ReportMetricAchievements(profile, summary, achievements);
            return stars;
        }

        public static int ApplyTempestTrialResult(
            PlayerProfile profile,
            TempestWeekDefinition week,
            TempestTrialRunDefinition run,
            RunSummary summary,
            ILeaderboardService leaderboard,
            IAchievementService achievements)
        {
            if (profile == null)
            {
                throw new ArgumentNullException("profile");
            }

            if (week == null)
            {
                throw new ArgumentNullException("week");
            }

            if (run == null)
            {
                throw new ArgumentNullException("run");
            }

            if (summary == null)
            {
                throw new ArgumentNullException("summary");
            }

            CosmeticCatalog.EnsureDefaultCosmetics(profile);
            profile.TotalRuns++;
            profile.TotalSurvivorsRescued += summary.SurvivorsRescued;
            profile.TotalStormTilesDestroyed += summary.StormTilesDestroyed;

            string key = TempestRecordKey(week.WeekKey, run.Id);
            if (!profile.TempestTrialHistory.TryGetValue(key, out var record))
            {
                record = new TempestTrialRecord
                {
                    WeekKey = week.WeekKey,
                    RunId = run.Id
                };
                profile.TempestTrialHistory.Add(key, record);
            }

            record.Completed = true;
            record.Score = Math.Max(record.Score, summary.Score);
            record.SurvivorsRescued = Math.Max(record.SurvivorsRescued, summary.SurvivorsRescued);
            record.StormTilesDestroyed = Math.Max(record.StormTilesDestroyed, summary.StormTilesDestroyed);

            int weeklyScore = CalculateTempestWeekScore(profile, week);
            if (IsTempestWeekComplete(profile, week))
            {
                if (weeklyScore > profile.BestTempestWeeklyScore)
                {
                    profile.BestTempestWeeklyScore = weeklyScore;
                    leaderboard?.SubmitScore(new LeaderboardScore
                    {
                        LeaderboardId = LeaderboardId.TempestTrialsWeekly,
                        Score = weeklyScore,
                        Context = week.WeekKey
                    });
                }

                if (!string.IsNullOrEmpty(week.CosmeticBadgeRewardId))
                {
                    Unlock(profile, week.CosmeticBadgeRewardId);
                }

                Unlock(profile, "badge_tempest_weekly");
                achievements?.ReportProgress(new AchievementProgress { AchievementId = AchievementId.TempestTrialComplete, PercentComplete = 100.0 });
            }

            ReportMetricAchievements(profile, summary, achievements);
            return weeklyScore;
        }

        public static int CalculateTempestWeekScore(PlayerProfile profile, TempestWeekDefinition week)
        {
            if (profile == null || week == null)
            {
                return 0;
            }

            int score = 0;
            for (int i = 0; i < week.Runs.Count; i++)
            {
                string key = TempestRecordKey(week.WeekKey, week.Runs[i].Id);
                if (profile.TempestTrialHistory.TryGetValue(key, out var record) && record.Completed)
                {
                    score += record.Score;
                }
            }

            return score;
        }

        public static bool IsTempestWeekComplete(PlayerProfile profile, TempestWeekDefinition week)
        {
            if (profile == null || week == null)
            {
                return false;
            }

            for (int i = 0; i < week.Runs.Count; i++)
            {
                string key = TempestRecordKey(week.WeekKey, week.Runs[i].Id);
                if (!profile.TempestTrialHistory.TryGetValue(key, out var record) || !record.Completed)
                {
                    return false;
                }
            }

            return week.Runs.Count > 0;
        }

        public static void EquipCosmetic(PlayerProfile profile, CosmeticDefinition cosmetic)
        {
            if (profile == null)
            {
                throw new ArgumentNullException("profile");
            }

            if (cosmetic == null)
            {
                throw new ArgumentNullException("cosmetic");
            }

            if (!profile.UnlockedCosmetics.Contains(cosmetic.Id))
            {
                throw new InvalidOperationException("Cosmetic is not unlocked: " + cosmetic.Id);
            }

            profile.EquippedCosmetics[cosmetic.Type] = cosmetic.Id;
        }

        public static string TempestRecordKey(string weekKey, string runId)
        {
            return weekKey + "|" + runId;
        }

        private static void ApplyDailyResult(PlayerProfile profile, RunSummary summary, DateTime utcNow, ILeaderboardService leaderboard)
        {
            string dateKey = ModeConfigFactory.ToDateKey(utcNow);
            if (!profile.DailyHistory.TryGetValue(dateKey, out var record))
            {
                record = new DailyStormRecord
                {
                    DateKey = dateKey,
                    Seed = summary.Seed
                };
                profile.DailyHistory.Add(dateKey, record);
            }

            record.Attempts++;
            record.BestScore = Math.Max(record.BestScore, summary.Score);
            record.SurvivorsRescued = Math.Max(record.SurvivorsRescued, summary.SurvivorsRescued);
            record.BestCombo = Math.Max(record.BestCombo, summary.BestCombo);

            if (summary.Score > profile.BestDailyScore)
            {
                profile.BestDailyScore = summary.Score;
            }

            if (summary.IsOfficialDailyScore && !record.OfficialRunCompleted)
            {
                record.OfficialRunCompleted = true;
                record.OfficialScore = summary.Score;
                UpdateDailyStreak(profile, utcNow);
                leaderboard?.SubmitScore(new LeaderboardScore
                {
                    LeaderboardId = LeaderboardId.DailyStorm,
                    Score = summary.Score,
                    Context = dateKey
                });
            }

            if (profile.DailyStreak >= 3)
            {
                Unlock(profile, "banner_daily_glow");
            }

            if (profile.DailyStreak >= 7)
            {
                Unlock(profile, "banner_weeklong_watch");
            }
        }

        private static void UpdateDailyStreak(PlayerProfile profile, DateTime utcNow)
        {
            string today = ModeConfigFactory.ToDateKey(utcNow);
            if (profile.LastDailyDateKey == today)
            {
                return;
            }

            string yesterday = ModeConfigFactory.ToDateKey(utcNow.ToUniversalTime().Date.AddDays(-1));
            profile.DailyStreak = profile.LastDailyDateKey == yesterday ? profile.DailyStreak + 1 : 1;
            profile.LastDailyDateKey = today;
        }

        private static bool IsRegionComplete(PlayerProfile profile, string regionId)
        {
            for (int level = 1; level <= 10; level++)
            {
                string levelId = regionId + "_level_" + level.ToString("00", CultureInfo.InvariantCulture);
                if (!profile.StormTrailStars.TryGetValue(levelId, out int stars) || stars <= 0)
                {
                    return false;
                }
            }

            return true;
        }

        private static void ReportMetricAchievements(PlayerProfile profile, RunSummary summary, IAchievementService achievements)
        {
            if (summary.SurvivorsRescued > 0)
            {
                profile.CompletedAchievements.Add(AchievementId.FirstRescue);
                achievements?.ReportProgress(new AchievementProgress { AchievementId = AchievementId.FirstRescue, PercentComplete = 100.0 });
            }

            if (summary.StormTilesDestroyed > 0)
            {
                profile.CompletedAchievements.Add(AchievementId.FirstPushback);
                achievements?.ReportProgress(new AchievementProgress { AchievementId = AchievementId.FirstPushback, PercentComplete = 100.0 });
            }

            if (summary.ClutchSaves > 0)
            {
                profile.CompletedAchievements.Add(AchievementId.ClutchSave);
                achievements?.ReportProgress(new AchievementProgress { AchievementId = AchievementId.ClutchSave, PercentComplete = 100.0 });
            }

            if (profile.DailyStreak >= 3)
            {
                profile.CompletedAchievements.Add(AchievementId.DailyStreak3);
                achievements?.ReportProgress(new AchievementProgress { AchievementId = AchievementId.DailyStreak3, PercentComplete = 100.0 });
            }

            if (profile.DailyStreak >= 7)
            {
                profile.CompletedAchievements.Add(AchievementId.DailyStreak7);
                achievements?.ReportProgress(new AchievementProgress { AchievementId = AchievementId.DailyStreak7, PercentComplete = 100.0 });
            }

            if (profile.UnlockedCosmetics.Count >= 10)
            {
                profile.CompletedAchievements.Add(AchievementId.CosmeticCollector);
                achievements?.ReportProgress(new AchievementProgress { AchievementId = AchievementId.CosmeticCollector, PercentComplete = 100.0 });
            }
        }

        private static void Unlock(PlayerProfile profile, string cosmeticId)
        {
            if (!string.IsNullOrEmpty(cosmeticId))
            {
                profile.UnlockedCosmetics.Add(cosmeticId);
            }
        }
    }

    public static class ProfileCodec
    {
        public static string ToPayload(PlayerProfile profile)
        {
            if (profile == null)
            {
                throw new ArgumentNullException("profile");
            }

            var lines = new List<string>
            {
                "version=" + profile.ProfileVersion.ToString(CultureInfo.InvariantCulture),
                "totalRuns=" + profile.TotalRuns.ToString(CultureInfo.InvariantCulture),
                "bestEndless=" + profile.BestEndlessScore.ToString(CultureInfo.InvariantCulture),
                "bestDaily=" + profile.BestDailyScore.ToString(CultureInfo.InvariantCulture),
                "bestTempest=" + profile.BestTempestWeeklyScore.ToString(CultureInfo.InvariantCulture),
                "survivors=" + profile.TotalSurvivorsRescued.ToString(CultureInfo.InvariantCulture),
                "stormTiles=" + profile.TotalStormTilesDestroyed.ToString(CultureInfo.InvariantCulture),
                "dailyStreak=" + profile.DailyStreak.ToString(CultureInfo.InvariantCulture),
                "lastDaily=" + profile.LastDailyDateKey,
                "unlocked=" + JoinSet(profile.UnlockedCosmetics),
                "equipped=" + JoinEquipped(profile.EquippedCosmetics),
                "trail=" + JoinStringInt(profile.StormTrailStars),
                "achievements=" + string.Join(",", profile.CompletedAchievements),
                "daily=" + JoinDailyHistory(profile.DailyHistory),
                "tempest=" + JoinTempestHistory(profile.TempestTrialHistory),
                "settings=" + JoinSettings(profile.Settings)
            };

            return string.Join("\n", lines);
        }

        public static PlayerProfile FromPayload(string payload)
        {
            var profile = new PlayerProfile();
            if (string.IsNullOrEmpty(payload))
            {
                CosmeticCatalog.EnsureDefaultCosmetics(profile);
                return profile;
            }

            var map = new Dictionary<string, string>();
            string[] lines = payload.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < lines.Length; i++)
            {
                int equalsIndex = lines[i].IndexOf('=');
                if (equalsIndex <= 0)
                {
                    continue;
                }

                map[lines[i].Substring(0, equalsIndex)] = lines[i].Substring(equalsIndex + 1);
            }

            profile.ProfileVersion = ReadInt(map, "version", 1);
            profile.TotalRuns = ReadInt(map, "totalRuns", 0);
            profile.BestEndlessScore = ReadInt(map, "bestEndless", 0);
            profile.BestDailyScore = ReadInt(map, "bestDaily", 0);
            profile.BestTempestWeeklyScore = ReadInt(map, "bestTempest", 0);
            profile.TotalSurvivorsRescued = ReadInt(map, "survivors", 0);
            profile.TotalStormTilesDestroyed = ReadInt(map, "stormTiles", 0);
            profile.DailyStreak = ReadInt(map, "dailyStreak", 0);
            profile.LastDailyDateKey = ReadString(map, "lastDaily");
            ReadSet(map, "unlocked", profile.UnlockedCosmetics);
            ReadEquipped(map, "equipped", profile.EquippedCosmetics);
            ReadStringInt(map, "trail", profile.StormTrailStars);
            ReadAchievements(map, "achievements", profile.CompletedAchievements);
            ReadDailyHistory(map, "daily", profile.DailyHistory);
            ReadTempestHistory(map, "tempest", profile.TempestTrialHistory);
            ReadSettings(map, "settings", profile.Settings);
            CosmeticCatalog.EnsureDefaultCosmetics(profile);
            return profile;
        }

        private static string JoinSet(HashSet<string> set)
        {
            var values = new List<string>(set);
            values.Sort(StringComparer.Ordinal);
            return string.Join(",", values);
        }

        private static string JoinEquipped(Dictionary<CosmeticType, string> equipped)
        {
            var values = new List<string>();
            foreach (var pair in equipped)
            {
                values.Add(pair.Key + ":" + pair.Value);
            }

            values.Sort(StringComparer.Ordinal);
            return string.Join(",", values);
        }

        private static string JoinStringInt(Dictionary<string, int> values)
        {
            var parts = new List<string>();
            foreach (var pair in values)
            {
                parts.Add(pair.Key + ":" + pair.Value.ToString(CultureInfo.InvariantCulture));
            }

            parts.Sort(StringComparer.Ordinal);
            return string.Join(",", parts);
        }

        private static string JoinDailyHistory(Dictionary<string, DailyStormRecord> history)
        {
            var parts = new List<string>();
            foreach (var pair in history)
            {
                var record = pair.Value;
                parts.Add(string.Join("~", new[]
                {
                    record.DateKey,
                    record.Seed.ToString(CultureInfo.InvariantCulture),
                    record.BestScore.ToString(CultureInfo.InvariantCulture),
                    record.OfficialScore.ToString(CultureInfo.InvariantCulture),
                    record.Attempts.ToString(CultureInfo.InvariantCulture),
                    record.SurvivorsRescued.ToString(CultureInfo.InvariantCulture),
                    record.BestCombo.ToString(CultureInfo.InvariantCulture),
                    record.OfficialRunCompleted ? "1" : "0"
                }));
            }

            parts.Sort(StringComparer.Ordinal);
            return string.Join(",", parts);
        }

        private static string JoinTempestHistory(Dictionary<string, TempestTrialRecord> history)
        {
            var parts = new List<string>();
            foreach (var pair in history)
            {
                var record = pair.Value;
                parts.Add(string.Join("~", new[]
                {
                    record.WeekKey,
                    record.RunId,
                    record.Score.ToString(CultureInfo.InvariantCulture),
                    record.SurvivorsRescued.ToString(CultureInfo.InvariantCulture),
                    record.StormTilesDestroyed.ToString(CultureInfo.InvariantCulture),
                    record.Completed ? "1" : "0"
                }));
            }

            parts.Sort(StringComparer.Ordinal);
            return string.Join(",", parts);
        }

        private static string JoinSettings(PlayerSettings settings)
        {
            return string.Join(",", new[]
            {
                "music:" + BoolToString(settings.MusicEnabled),
                "effects:" + BoolToString(settings.EffectsEnabled),
                "haptics:" + BoolToString(settings.HapticsEnabled),
                "volume:" + settings.MasterVolume.ToString("0.###", CultureInfo.InvariantCulture),
                "reduced:" + BoolToString(settings.ReducedMotion),
                "contrast:" + BoolToString(settings.HighContrast),
                "colorblind:" + BoolToString(settings.ColorblindFriendly),
                "left:" + BoolToString(settings.LeftHandedMode),
                "large:" + BoolToString(settings.LargeText),
                "lowdetail:" + BoolToString(settings.LowDetailMode)
            });
        }

        private static string BoolToString(bool value)
        {
            return value ? "1" : "0";
        }

        private static int ReadInt(Dictionary<string, string> map, string key, int fallback)
        {
            if (map.TryGetValue(key, out string value) && int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed))
            {
                return parsed;
            }

            return fallback;
        }

        private static string ReadString(Dictionary<string, string> map, string key)
        {
            return map.TryGetValue(key, out string value) ? value : string.Empty;
        }

        private static void ReadSet(Dictionary<string, string> map, string key, HashSet<string> output)
        {
            if (!map.TryGetValue(key, out string value) || value.Length == 0)
            {
                return;
            }

            string[] parts = value.Split(',');
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].Length > 0)
                {
                    output.Add(parts[i]);
                }
            }
        }

        private static void ReadEquipped(Dictionary<string, string> map, string key, Dictionary<CosmeticType, string> output)
        {
            if (!map.TryGetValue(key, out string value) || value.Length == 0)
            {
                return;
            }

            string[] parts = value.Split(',');
            for (int i = 0; i < parts.Length; i++)
            {
                string[] item = parts[i].Split(':');
                if (item.Length == 2 && Enum.TryParse(item[0], out CosmeticType type))
                {
                    output[type] = item[1];
                }
            }
        }

        private static void ReadStringInt(Dictionary<string, string> map, string key, Dictionary<string, int> output)
        {
            if (!map.TryGetValue(key, out string value) || value.Length == 0)
            {
                return;
            }

            string[] parts = value.Split(',');
            for (int i = 0; i < parts.Length; i++)
            {
                string[] item = parts[i].Split(':');
                if (item.Length == 2 && int.TryParse(item[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed))
                {
                    output[item[0]] = parsed;
                }
            }
        }

        private static void ReadAchievements(Dictionary<string, string> map, string key, HashSet<AchievementId> output)
        {
            if (!map.TryGetValue(key, out string value) || value.Length == 0)
            {
                return;
            }

            string[] parts = value.Split(',');
            for (int i = 0; i < parts.Length; i++)
            {
                if (Enum.TryParse(parts[i], out AchievementId achievementId))
                {
                    output.Add(achievementId);
                }
            }
        }

        private static void ReadDailyHistory(Dictionary<string, string> map, string key, Dictionary<string, DailyStormRecord> output)
        {
            if (!map.TryGetValue(key, out string value) || value.Length == 0)
            {
                return;
            }

            string[] records = value.Split(',');
            for (int i = 0; i < records.Length; i++)
            {
                string[] fields = records[i].Split('~');
                if (fields.Length != 8)
                {
                    continue;
                }

                var record = new DailyStormRecord
                {
                    DateKey = fields[0],
                    Seed = ulong.Parse(fields[1], CultureInfo.InvariantCulture),
                    BestScore = int.Parse(fields[2], CultureInfo.InvariantCulture),
                    OfficialScore = int.Parse(fields[3], CultureInfo.InvariantCulture),
                    Attempts = int.Parse(fields[4], CultureInfo.InvariantCulture),
                    SurvivorsRescued = int.Parse(fields[5], CultureInfo.InvariantCulture),
                    BestCombo = int.Parse(fields[6], CultureInfo.InvariantCulture),
                    OfficialRunCompleted = fields[7] == "1"
                };
                output[record.DateKey] = record;
            }
        }

        private static void ReadTempestHistory(Dictionary<string, string> map, string key, Dictionary<string, TempestTrialRecord> output)
        {
            if (!map.TryGetValue(key, out string value) || value.Length == 0)
            {
                return;
            }

            string[] records = value.Split(',');
            for (int i = 0; i < records.Length; i++)
            {
                string[] fields = records[i].Split('~');
                if (fields.Length != 6)
                {
                    continue;
                }

                var record = new TempestTrialRecord
                {
                    WeekKey = fields[0],
                    RunId = fields[1],
                    Score = int.Parse(fields[2], CultureInfo.InvariantCulture),
                    SurvivorsRescued = int.Parse(fields[3], CultureInfo.InvariantCulture),
                    StormTilesDestroyed = int.Parse(fields[4], CultureInfo.InvariantCulture),
                    Completed = fields[5] == "1"
                };
                output[ProfileProgression.TempestRecordKey(record.WeekKey, record.RunId)] = record;
            }
        }

        private static void ReadSettings(Dictionary<string, string> map, string key, PlayerSettings output)
        {
            if (!map.TryGetValue(key, out string value) || value.Length == 0)
            {
                return;
            }

            string[] parts = value.Split(',');
            for (int i = 0; i < parts.Length; i++)
            {
                string[] item = parts[i].Split(':');
                if (item.Length != 2)
                {
                    continue;
                }

                string setting = item[0];
                string raw = item[1];
                if (setting == "music")
                {
                    output.MusicEnabled = raw == "1";
                }
                else if (setting == "effects")
                {
                    output.EffectsEnabled = raw == "1";
                }
                else if (setting == "haptics")
                {
                    output.HapticsEnabled = raw == "1";
                }
                else if (setting == "volume" && float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out float volume))
                {
                    output.MasterVolume = Math.Max(0f, Math.Min(1f, volume));
                }
                else if (setting == "reduced")
                {
                    output.ReducedMotion = raw == "1";
                }
                else if (setting == "contrast")
                {
                    output.HighContrast = raw == "1";
                }
                else if (setting == "colorblind")
                {
                    output.ColorblindFriendly = raw == "1";
                }
                else if (setting == "left")
                {
                    output.LeftHandedMode = raw == "1";
                }
                else if (setting == "large")
                {
                    output.LargeText = raw == "1";
                }
                else if (setting == "lowdetail")
                {
                    output.LowDetailMode = raw == "1";
                }
            }
        }
    }

    public sealed class FileSaveService : ISaveService
    {
        private readonly string _profilePath;
        private readonly string _snapshotPath;

        public FileSaveService(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                throw new ArgumentException("A save folder path is required.", "folderPath");
            }

            Directory.CreateDirectory(folderPath);
            _profilePath = Path.Combine(folderPath, "profile.sbprofile");
            _snapshotPath = Path.Combine(folderPath, "run.sbsnapshot");
        }

        public bool HasProfile()
        {
            return File.Exists(_profilePath);
        }

        public PlayerProfile LoadProfile()
        {
            if (!File.Exists(_profilePath))
            {
                var fresh = new PlayerProfile();
                CosmeticCatalog.EnsureDefaultCosmetics(fresh);
                return fresh;
            }

            return ProfileCodec.FromPayload(File.ReadAllText(_profilePath));
        }

        public void SaveProfile(PlayerProfile profile)
        {
            File.WriteAllText(_profilePath, ProfileCodec.ToPayload(profile));
        }

        public void SaveRunSnapshot(StormRunSnapshot snapshot)
        {
            File.WriteAllText(_snapshotPath, snapshot.ToPayload());
        }

        public bool TryLoadRunSnapshot(out StormRunSnapshot snapshot)
        {
            if (!File.Exists(_snapshotPath))
            {
                snapshot = null;
                return false;
            }

            snapshot = StormRunSnapshot.FromPayload(File.ReadAllText(_snapshotPath));
            return true;
        }

        public void ClearRunSnapshot()
        {
            if (File.Exists(_snapshotPath))
            {
                File.Delete(_snapshotPath);
            }
        }
    }
}
