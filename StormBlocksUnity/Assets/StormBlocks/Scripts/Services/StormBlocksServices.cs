using System;
using System.Collections.Generic;
using StormBlocks.Core;

namespace StormBlocks.Services
{
    public enum AudioEventId
    {
        UiTap,
        PiecePickup,
        PieceHover,
        ValidPlacement,
        InvalidPlacement,
        LineClear,
        Combo,
        SurvivorRescued,
        StormWarning,
        StormSpread,
        StormPushback,
        ClutchSave,
        NearDeathLoop,
        GameOver,
        ResultsCelebration,
        CosmeticUnlock,
        DailyStormStart,
        DailyStormEnd
    }

    public enum HapticEventId
    {
        LightTap,
        MediumPlacement,
        WarningPulse,
        HeavyClear,
        SuccessBurst,
        LongNearDeathWarning
    }

    public enum LeaderboardId
    {
        EndlessHighScore,
        DailyStorm,
        TempestTrialsWeekly
    }

    public enum AchievementId
    {
        FirstRescue,
        FirstPushback,
        ClutchSave,
        DailyStreak3,
        DailyStreak7,
        StormTrailRegionComplete,
        TempestTrialComplete,
        CosmeticCollector
    }

    public enum CosmeticType
    {
        BlockSkin,
        CampSkin,
        SurvivorOutfit,
        StormSkin,
        PushbackVfx,
        ProfileBanner,
        ResultFrame,
        TempestBadge
    }

    public struct LeaderboardScore
    {
        public LeaderboardId LeaderboardId;
        public long Score;
        public string Context;
    }

    public struct AchievementProgress
    {
        public AchievementId AchievementId;
        public double PercentComplete;
    }

    public struct AnalyticsEvent
    {
        public string Name;
        public IReadOnlyDictionary<string, string> Properties;

        public AnalyticsEvent(string name, IReadOnlyDictionary<string, string> properties)
        {
            Name = name;
            Properties = properties;
        }
    }

    public sealed class RunSummary
    {
        public GameModeId Mode;
        public ulong Seed;
        public int Score;
        public int SurvivorsRescued;
        public int StormTilesDestroyed;
        public int BestCombo;
        public int ClutchSaves;
        public int Placements;
        public bool IsOfficialDailyScore;
        public string ShareToken = string.Empty;

        public static RunSummary FromRunState(StormRunState state, bool officialDaily)
        {
            return new RunSummary
            {
                Mode = state.Config.Mode,
                Seed = state.Seed,
                Score = state.Score,
                SurvivorsRescued = state.SurvivorsRescued,
                StormTilesDestroyed = state.StormTilesDestroyed,
                BestCombo = state.BestCombo,
                ClutchSaves = state.ClutchSaves,
                Placements = state.Placements,
                IsOfficialDailyScore = officialDaily,
                ShareToken = "SB-" + state.Seed.ToString("X") + "-" + state.Score
            };
        }
    }

    public sealed class DailyStormRecord
    {
        public string DateKey = string.Empty;
        public ulong Seed;
        public int BestScore;
        public int OfficialScore;
        public int Attempts;
        public int SurvivorsRescued;
        public int BestCombo;
        public bool OfficialRunCompleted;
    }

    public sealed class TempestTrialRecord
    {
        public string WeekKey = string.Empty;
        public string RunId = string.Empty;
        public int Score;
        public int SurvivorsRescued;
        public int StormTilesDestroyed;
        public bool Completed;
    }

    public sealed class PlayerSettings
    {
        public bool MusicEnabled = true;
        public bool EffectsEnabled = true;
        public bool HapticsEnabled = true;
        public float MasterVolume = 0.85f;
        public bool ReducedMotion;
        public bool HighContrast;
        public bool ColorblindFriendly;
        public bool LeftHandedMode;
        public bool LargeText;
        public bool LowDetailMode;
    }

    public sealed class PlayerProfile
    {
        public int ProfileVersion = 1;
        public int TotalRuns;
        public int BestEndlessScore;
        public int BestDailyScore;
        public int BestTempestWeeklyScore;
        public int TotalSurvivorsRescued;
        public int TotalStormTilesDestroyed;
        public int DailyStreak;
        public string LastDailyDateKey = string.Empty;
        public readonly HashSet<string> UnlockedCosmetics = new HashSet<string>();
        public readonly Dictionary<CosmeticType, string> EquippedCosmetics = new Dictionary<CosmeticType, string>();
        public readonly Dictionary<string, int> StormTrailStars = new Dictionary<string, int>();
        public readonly Dictionary<string, DailyStormRecord> DailyHistory = new Dictionary<string, DailyStormRecord>();
        public readonly Dictionary<string, TempestTrialRecord> TempestTrialHistory = new Dictionary<string, TempestTrialRecord>();
        public readonly HashSet<AchievementId> CompletedAchievements = new HashSet<AchievementId>();
        public readonly PlayerSettings Settings = new PlayerSettings();
    }

    public interface ISaveService
    {
        bool HasProfile();
        PlayerProfile LoadProfile();
        void SaveProfile(PlayerProfile profile);
        void SaveRunSnapshot(StormRunSnapshot snapshot);
        bool TryLoadRunSnapshot(out StormRunSnapshot snapshot);
        void ClearRunSnapshot();
    }

    public interface ILeaderboardService
    {
        bool IsAvailable { get; }
        void SubmitScore(LeaderboardScore score);
        IReadOnlyList<LeaderboardScore> GetLocalScores(LeaderboardId leaderboardId);
    }

    public interface IAchievementService
    {
        bool IsAvailable { get; }
        void ReportProgress(AchievementProgress progress);
        bool IsCompleted(AchievementId achievementId);
    }

    public interface IChallengeService
    {
        bool IsAvailable { get; }
        void SendChallenge(RunSummary runSummary, string message);
    }

    public interface IAnalyticsService
    {
        void Track(AnalyticsEvent analyticsEvent);
    }

    public interface IRemoteConfigService
    {
        bool TryGetString(string key, out string value);
        bool TryGetInt(string key, out int value);
        bool TryGetFloat(string key, out float value);
    }

    public interface ICloudSaveService
    {
        bool IsAvailable { get; }
        void UploadProfile(PlayerProfile profile);
        bool TryDownloadProfile(out PlayerProfile profile);
    }

    public interface IHapticsService
    {
        bool Enabled { get; set; }
        void Play(HapticEventId eventId);
    }

    public interface IAudioService
    {
        bool MusicEnabled { get; set; }
        bool EffectsEnabled { get; set; }
        float MasterVolume { get; set; }
        void Play(AudioEventId eventId);
        void SetNearDeathIntensity(float intensity);
    }

    public interface IShareService
    {
        bool CanShare { get; }
        void ShareRun(RunSummary runSummary);
    }

    public sealed class MockGameServices :
        ISaveService,
        ILeaderboardService,
        IAchievementService,
        IChallengeService,
        IAnalyticsService,
        IRemoteConfigService,
        ICloudSaveService,
        IHapticsService,
        IAudioService,
        IShareService
    {
        private readonly Dictionary<LeaderboardId, List<LeaderboardScore>> _scores = new Dictionary<LeaderboardId, List<LeaderboardScore>>();
        private readonly Dictionary<string, string> _remoteStrings = new Dictionary<string, string>();
        private readonly Dictionary<string, int> _remoteInts = new Dictionary<string, int>();
        private readonly Dictionary<string, float> _remoteFloats = new Dictionary<string, float>();
        private readonly HashSet<AchievementId> _completedAchievements = new HashSet<AchievementId>();
        private PlayerProfile _profile = new PlayerProfile();
        private StormRunSnapshot _snapshot;
        private bool _hasSnapshot;

        public bool IsAvailable
        {
            get { return true; }
        }

        public bool Enabled { get; set; } = true;
        public bool MusicEnabled { get; set; } = true;
        public bool EffectsEnabled { get; set; } = true;
        public float MasterVolume { get; set; } = 1f;
        public bool CanShare { get; private set; } = true;

        public readonly List<AnalyticsEvent> AnalyticsEvents = new List<AnalyticsEvent>();
        public readonly List<AudioEventId> AudioEvents = new List<AudioEventId>();
        public readonly List<HapticEventId> HapticEvents = new List<HapticEventId>();
        public RunSummary LastSharedRun;

        public void SetRemoteString(string key, string value)
        {
            _remoteStrings[key] = value;
        }

        public void SetRemoteInt(string key, int value)
        {
            _remoteInts[key] = value;
        }

        public void SetRemoteFloat(string key, float value)
        {
            _remoteFloats[key] = value;
        }

        public bool HasProfile()
        {
            return _profile != null;
        }

        public PlayerProfile LoadProfile()
        {
            return _profile ?? new PlayerProfile();
        }

        public void SaveProfile(PlayerProfile profile)
        {
            _profile = profile ?? new PlayerProfile();
        }

        public void SaveRunSnapshot(StormRunSnapshot snapshot)
        {
            _snapshot = snapshot;
            _hasSnapshot = true;
        }

        public bool TryLoadRunSnapshot(out StormRunSnapshot snapshot)
        {
            snapshot = _snapshot;
            return _hasSnapshot;
        }

        public void ClearRunSnapshot()
        {
            _snapshot = null;
            _hasSnapshot = false;
        }

        public void SubmitScore(LeaderboardScore score)
        {
            if (!_scores.TryGetValue(score.LeaderboardId, out var list))
            {
                list = new List<LeaderboardScore>();
                _scores.Add(score.LeaderboardId, list);
            }

            list.Add(score);
            list.Sort(delegate(LeaderboardScore a, LeaderboardScore b)
            {
                return b.Score.CompareTo(a.Score);
            });
        }

        public IReadOnlyList<LeaderboardScore> GetLocalScores(LeaderboardId leaderboardId)
        {
            if (_scores.TryGetValue(leaderboardId, out var list))
            {
                return list.ToArray();
            }

            return new LeaderboardScore[0];
        }

        public void ReportProgress(AchievementProgress progress)
        {
            if (progress.PercentComplete >= 100.0)
            {
                _completedAchievements.Add(progress.AchievementId);
            }
        }

        public bool IsCompleted(AchievementId achievementId)
        {
            return _completedAchievements.Contains(achievementId);
        }

        public void SendChallenge(RunSummary runSummary, string message)
        {
            LastSharedRun = runSummary;
        }

        public void Track(AnalyticsEvent analyticsEvent)
        {
            AnalyticsEvents.Add(analyticsEvent);
        }

        public bool TryGetString(string key, out string value)
        {
            return _remoteStrings.TryGetValue(key, out value);
        }

        public bool TryGetInt(string key, out int value)
        {
            return _remoteInts.TryGetValue(key, out value);
        }

        public bool TryGetFloat(string key, out float value)
        {
            return _remoteFloats.TryGetValue(key, out value);
        }

        public void UploadProfile(PlayerProfile profile)
        {
            SaveProfile(profile);
        }

        public bool TryDownloadProfile(out PlayerProfile profile)
        {
            profile = _profile;
            return profile != null;
        }

        public void Play(HapticEventId eventId)
        {
            if (Enabled)
            {
                HapticEvents.Add(eventId);
            }
        }

        public void Play(AudioEventId eventId)
        {
            if (EffectsEnabled)
            {
                AudioEvents.Add(eventId);
            }
        }

        public void SetNearDeathIntensity(float intensity)
        {
        }

        public void ShareRun(RunSummary runSummary)
        {
            LastSharedRun = runSummary;
        }
    }
}
