using System;
using System.Collections.Generic;
using System.Globalization;

namespace StormBlocks.Core
{
    public enum StormModifierId
    {
        None = 0,
        RescueFocus = 1,
        FastFront = 2,
        HeavyStorm = 3,
        ComboWeather = 4,
        ChillBreeze = 5
    }

    public sealed class GameModeDefinition
    {
        public string Id = string.Empty;
        public string DisplayName = string.Empty;
        public string Description = string.Empty;
        public GameModeId Mode;
        public ulong Seed;
        public RunConfig Config = new RunConfig();
        public StormModifierId Modifier = StormModifierId.None;
        public bool IsLeaderboardEligible;
        public bool IsOfficialAttempt;
    }

    public sealed class DailyStormDefinition
    {
        public string DateKey = string.Empty;
        public GameModeDefinition Mode = new GameModeDefinition();
        public int SeasonVersion;
        public int RulesVersion;
    }

    public sealed class StarGoals
    {
        public int OneStarScore;
        public int TwoStarScore;
        public int ThreeStarScore;
        public int SurvivorTarget;
        public int PushbackTarget;

        public int Evaluate(int score, int survivorsRescued, int stormTilesDestroyed)
        {
            if (score >= ThreeStarScore && survivorsRescued >= SurvivorTarget && stormTilesDestroyed >= PushbackTarget)
            {
                return 3;
            }

            if (score >= TwoStarScore && survivorsRescued >= Math.Max(0, SurvivorTarget - 1))
            {
                return 2;
            }

            return score >= OneStarScore ? 1 : 0;
        }
    }

    public sealed class StormTrailLevelDefinition
    {
        public string Id = string.Empty;
        public string DisplayName = string.Empty;
        public string RegionId = string.Empty;
        public string RegionName = string.Empty;
        public int RegionIndex;
        public int LevelIndex;
        public ulong Seed;
        public StormModifierId Modifier;
        public string TutorialBeat = string.Empty;
        public string CosmeticRewardId = string.Empty;
        public RunConfig Config = new RunConfig();
        public StarGoals Goals = new StarGoals();
    }

    public sealed class StormTrailRegionDefinition
    {
        private readonly List<StormTrailLevelDefinition> _levels = new List<StormTrailLevelDefinition>();

        public string Id = string.Empty;
        public string DisplayName = string.Empty;
        public int RegionIndex;

        public IReadOnlyList<StormTrailLevelDefinition> Levels
        {
            get { return _levels; }
        }

        internal void AddLevel(StormTrailLevelDefinition level)
        {
            _levels.Add(level);
        }
    }

    public sealed class TempestTrialRunDefinition
    {
        public string Id = string.Empty;
        public int RunIndex;
        public ulong Seed;
        public StormModifierId Modifier;
        public RunConfig Config = new RunConfig();
        public StarGoals Targets = new StarGoals();
    }

    public sealed class TempestWeekDefinition
    {
        private readonly List<TempestTrialRunDefinition> _runs = new List<TempestTrialRunDefinition>();

        public string WeekKey = string.Empty;
        public ulong Seed;
        public string CosmeticBadgeRewardId = string.Empty;

        public IReadOnlyList<TempestTrialRunDefinition> Runs
        {
            get { return _runs; }
        }

        internal void AddRun(TempestTrialRunDefinition run)
        {
            _runs.Add(run);
        }
    }

    public static class ModeConfigFactory
    {
        private const int SeasonVersion = 1;
        private const int RulesVersion = 1;
        private const string SeedSalt = "storm-blocks";

        public static GameModeDefinition CreateEndless(ulong seed)
        {
            return new GameModeDefinition
            {
                Id = "endless_storm",
                DisplayName = "Endless Storm",
                Description = "High-score survival with an escalating living storm.",
                Mode = GameModeId.EndlessStorm,
                Seed = seed,
                IsLeaderboardEligible = true,
                Config = CreateRunConfig(GameModeId.EndlessStorm, StormModifierId.None, 1, 4, 18, 42)
            };
        }

        public static GameModeDefinition CreatePractice(ulong seed)
        {
            return new GameModeDefinition
            {
                Id = "practice_chill",
                DisplayName = "Practice",
                Description = "Lower-pressure play with slower storm pressure and no leaderboard.",
                Mode = GameModeId.Practice,
                Seed = seed,
                Modifier = StormModifierId.ChillBreeze,
                IsLeaderboardEligible = false,
                Config = CreateRunConfig(GameModeId.Practice, StormModifierId.ChillBreeze, 0, 2, 28, 64)
            };
        }

        public static DailyStormDefinition CreateDaily(DateTime utcDate, bool officialAttempt)
        {
            string dateKey = ToDateKey(utcDate);
            ulong seed = DailySeed.FromDate(utcDate.Date, SeasonVersion, RulesVersion, SeedSalt);
            StormModifierId modifier = PickModifier(seed, true);
            return new DailyStormDefinition
            {
                DateKey = dateKey,
                SeasonVersion = SeasonVersion,
                RulesVersion = RulesVersion,
                Mode = new GameModeDefinition
                {
                    Id = "daily_storm_" + dateKey,
                    DisplayName = "Daily Storm",
                    Description = "Today's shared deterministic storm.",
                    Mode = GameModeId.DailyStorm,
                    Seed = seed,
                    Modifier = modifier,
                    IsLeaderboardEligible = officialAttempt,
                    IsOfficialAttempt = officialAttempt,
                    Config = CreateRunConfig(GameModeId.DailyStorm, modifier, 1, 4, 16, 38)
                }
            };
        }

        public static IReadOnlyList<StormTrailRegionDefinition> CreateStormTrailCatalog()
        {
            string[] regionNames =
            {
                "First Camp",
                "Wind Edge",
                "Rescue Hollow",
                "Lightning Ridge",
                "Frozen Flats",
                "Firebreak Forest",
                "Floodway",
                "Night Camp",
                "Signal Hill",
                "Tempest Gate",
                "Safehouse Road",
                "Final Front"
            };

            string[] tutorialBeats =
            {
                "place_blocks",
                "storm_warning",
                "rescue_survivors",
                "charged_storm",
                "frozen_cells",
                "flare_bonus",
                "water_pressure",
                "near_death",
                "combo_goals",
                "heavy_storm",
                "advanced_boards",
                "mastery"
            };

            var regions = new List<StormTrailRegionDefinition>(regionNames.Length);
            for (int region = 0; region < regionNames.Length; region++)
            {
                string regionId = "region_" + (region + 1).ToString("00", CultureInfo.InvariantCulture);
                var definition = new StormTrailRegionDefinition
                {
                    Id = regionId,
                    DisplayName = regionNames[region],
                    RegionIndex = region + 1
                };

                for (int level = 1; level <= 10; level++)
                {
                    string levelId = regionId + "_level_" + level.ToString("00", CultureInfo.InvariantCulture);
                    StormModifierId modifier = PickTrailModifier(region + 1, level);
                    ulong seed = DailySeed.StableHash64("trail|" + levelId + "|" + RulesVersion.ToString(CultureInfo.InvariantCulture));
                    int difficulty = region * 10 + level;
                    definition.AddLevel(new StormTrailLevelDefinition
                    {
                        Id = levelId,
                        DisplayName = regionNames[region] + " " + level.ToString(CultureInfo.InvariantCulture),
                        RegionId = regionId,
                        RegionName = regionNames[region],
                        RegionIndex = region + 1,
                        LevelIndex = level,
                        Seed = seed,
                        Modifier = modifier,
                        TutorialBeat = tutorialBeats[region],
                        CosmeticRewardId = level == 10 ? "trail_" + regionId + "_badge" : string.Empty,
                        Config = CreateRunConfig(GameModeId.StormTrail, modifier, region <= 1 ? 0 : 1, Math.Min(6, 2 + region / 2), Math.Max(8, 30 - region), Math.Max(20, 70 - region * 3)),
                        Goals = new StarGoals
                        {
                            OneStarScore = 450 + difficulty * 35,
                            TwoStarScore = 850 + difficulty * 55,
                            ThreeStarScore = 1300 + difficulty * 80,
                            SurvivorTarget = 1 + region / 3,
                            PushbackTarget = Math.Max(1, region / 2)
                        }
                    });
                }

                regions.Add(definition);
            }

            return regions;
        }

        public static TempestWeekDefinition CreateTempestWeek(DateTime utcDate)
        {
            DateTime weekStart = GetWeekStartUtc(utcDate);
            string weekKey = ToDateKey(weekStart);
            ulong seed = DailySeed.StableHash64("tempest|" + weekKey + "|" + SeasonVersion.ToString(CultureInfo.InvariantCulture));
            var week = new TempestWeekDefinition
            {
                WeekKey = weekKey,
                Seed = seed,
                CosmeticBadgeRewardId = "tempest_badge_" + weekKey.Replace("-", string.Empty)
            };

            for (int index = 0; index < 5; index++)
            {
                ulong runSeed = DailySeed.StableHash64(seed.ToString(CultureInfo.InvariantCulture) + "|run|" + index.ToString(CultureInfo.InvariantCulture));
                StormModifierId modifier = PickModifier(runSeed, false);
                week.AddRun(new TempestTrialRunDefinition
                {
                    Id = weekKey + "_run_" + (index + 1).ToString(CultureInfo.InvariantCulture),
                    RunIndex = index + 1,
                    Seed = runSeed,
                    Modifier = modifier,
                    Config = CreateRunConfig(GameModeId.TempestTrial, modifier, 1, 5, 12, 30),
                    Targets = new StarGoals
                    {
                        OneStarScore = 1200 + index * 300,
                        TwoStarScore = 2200 + index * 420,
                        ThreeStarScore = 3600 + index * 560,
                        SurvivorTarget = 2 + index,
                        PushbackTarget = 3 + index
                    }
                });
            }

            return week;
        }

        public static string ToDateKey(DateTime utcDate)
        {
            return utcDate.ToUniversalTime().Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        public static DateTime GetWeekStartUtc(DateTime utcDate)
        {
            DateTime date = utcDate.ToUniversalTime().Date;
            int offset = ((int)date.DayOfWeek + 6) % 7;
            return date.AddDays(-offset);
        }

        private static RunConfig CreateRunConfig(GameModeId mode, StormModifierId modifier, int initialRing, int spreadCells, int strategicTurn, int panicTurn)
        {
            var config = new RunConfig
            {
                Mode = mode,
                StrategicPhasePlacement = strategicTurn,
                PanicPhasePlacement = panicTurn,
                QueueSize = 3,
                StormRules = new StormRulesConfig
                {
                    InitialStormRingThickness = initialRing,
                    BaseStormSpreadCells = spreadCells,
                    CalmSpreadEveryPlacements = 4,
                    StrategicSpreadEveryPlacements = 3,
                    PanicSpreadEveryPlacements = 2,
                    WarningBeforeSpread = true,
                    PushbackAutomatic = true,
                    NearDeathDistanceToCamp = 2
                },
                Scoring = new ScoringConfig()
            };

            ApplyModifier(config, modifier);
            return config;
        }

        private static void ApplyModifier(RunConfig config, StormModifierId modifier)
        {
            if (modifier == StormModifierId.RescueFocus)
            {
                config.Scoring.SurvivorRescued = 40;
                config.StormRules.BaseStormSpreadCells = Math.Max(1, config.StormRules.BaseStormSpreadCells - 1);
            }
            else if (modifier == StormModifierId.FastFront)
            {
                config.StormRules.CalmSpreadEveryPlacements = 3;
                config.StormRules.StrategicSpreadEveryPlacements = 2;
            }
            else if (modifier == StormModifierId.HeavyStorm)
            {
                config.StormRules.BaseStormSpreadCells += 2;
                config.StormRules.NearDeathDistanceToCamp = 3;
            }
            else if (modifier == StormModifierId.ComboWeather)
            {
                config.Scoring.ComboMultipliers = new[] { 1, 3, 5, 8, 13 };
                config.StormRules.ComboBonusPushbackRadius = 2;
            }
            else if (modifier == StormModifierId.ChillBreeze)
            {
                config.StormRules.CalmSpreadEveryPlacements = 6;
                config.StormRules.StrategicSpreadEveryPlacements = 5;
                config.StormRules.PanicSpreadEveryPlacements = 4;
                config.StormRules.BaseStormSpreadCells = Math.Max(1, config.StormRules.BaseStormSpreadCells);
                config.Scoring.ClutchSave = 250;
            }
        }

        private static StormModifierId PickModifier(ulong seed, bool includeNone)
        {
            StormModifierId[] modifiers = includeNone
                ? new[] { StormModifierId.None, StormModifierId.RescueFocus, StormModifierId.FastFront, StormModifierId.ComboWeather, StormModifierId.HeavyStorm }
                : new[] { StormModifierId.RescueFocus, StormModifierId.FastFront, StormModifierId.ComboWeather, StormModifierId.HeavyStorm };
            return modifiers[(int)(seed % (ulong)modifiers.Length)];
        }

        private static StormModifierId PickTrailModifier(int region, int level)
        {
            if (region <= 1)
            {
                return StormModifierId.None;
            }

            if (region == 2)
            {
                return StormModifierId.FastFront;
            }

            if (region == 3)
            {
                return StormModifierId.RescueFocus;
            }

            if (region == 9)
            {
                return StormModifierId.ComboWeather;
            }

            if (region >= 10)
            {
                return StormModifierId.HeavyStorm;
            }

            return PickModifier((ulong)(region * 100 + level), false);
        }
    }
}
