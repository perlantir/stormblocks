using System.Collections.Generic;
using System.Runtime.InteropServices;
using StormBlocks.Services;
using UnityEngine;

namespace StormBlocks.Presentation
{
    public sealed class UnityGameCenterServices : MonoBehaviour, ILeaderboardService, IAchievementService, IChallengeService
    {
        [SerializeField] private bool authenticateOnStart = true;

        private readonly List<LeaderboardScore> _pendingScores = new List<LeaderboardScore>();
        private readonly List<AchievementProgress> _pendingAchievements = new List<AchievementProgress>();
        private ILeaderboardService _localLeaderboards;
        private IAchievementService _localAchievements;
        private IChallengeService _localChallenges;
        private bool _authenticationRequested;
        private bool _authenticated;
        private float _authPollTimer;

        public bool IsAvailable
        {
            get { return _authenticated; }
        }

        public static string LeaderboardIdentifier(LeaderboardId leaderboardId)
        {
            switch (leaderboardId)
            {
                case LeaderboardId.EndlessHighScore:
                    return "com.perlantir.stormblocks.leaderboard.endless_high_score";
                case LeaderboardId.DailyStorm:
                    return "com.perlantir.stormblocks.leaderboard.daily_storm";
                case LeaderboardId.TempestTrialsWeekly:
                    return "com.perlantir.stormblocks.leaderboard.tempest_trials_weekly";
                default:
                    return "com.perlantir.stormblocks.leaderboard.unknown";
            }
        }

        public static string AchievementIdentifier(AchievementId achievementId)
        {
            switch (achievementId)
            {
                case AchievementId.FirstRescue:
                    return "com.perlantir.stormblocks.achievement.first_rescue";
                case AchievementId.FirstPushback:
                    return "com.perlantir.stormblocks.achievement.first_pushback";
                case AchievementId.ClutchSave:
                    return "com.perlantir.stormblocks.achievement.clutch_save";
                case AchievementId.DailyStreak3:
                    return "com.perlantir.stormblocks.achievement.daily_streak_3";
                case AchievementId.DailyStreak7:
                    return "com.perlantir.stormblocks.achievement.daily_streak_7";
                case AchievementId.StormTrailRegionComplete:
                    return "com.perlantir.stormblocks.achievement.storm_trail_region_complete";
                case AchievementId.TempestTrialComplete:
                    return "com.perlantir.stormblocks.achievement.tempest_trial_complete";
                case AchievementId.CosmeticCollector:
                    return "com.perlantir.stormblocks.achievement.cosmetic_collector";
                default:
                    return "com.perlantir.stormblocks.achievement.unknown";
            }
        }

        public void Configure(
            ILeaderboardService localLeaderboards,
            IAchievementService localAchievements,
            IChallengeService localChallenges)
        {
            _localLeaderboards = localLeaderboards;
            _localAchievements = localAchievements;
            _localChallenges = localChallenges;

            if (authenticateOnStart)
            {
                TryAuthenticate();
            }
        }

        public void TryAuthenticate()
        {
#if UNITY_IOS && !UNITY_EDITOR
            if (_authenticationRequested)
            {
                return;
            }

            _authenticationRequested = true;
            SBGameKitAuthenticate();
            PollAuthenticationState();
#else
            _authenticated = false;
#endif
        }

        private void Update()
        {
#if UNITY_IOS && !UNITY_EDITOR
            if (!_authenticationRequested || _authenticated)
            {
                return;
            }

            _authPollTimer += Time.unscaledDeltaTime;
            if (_authPollTimer >= 0.75f)
            {
                _authPollTimer = 0f;
                PollAuthenticationState();
            }
#endif
        }

        public void SubmitScore(LeaderboardScore score)
        {
            _localLeaderboards?.SubmitScore(score);

            if (!CanReportToGameCenter())
            {
                QueueScore(score);
                return;
            }

            ReportScoreToGameCenter(score);
        }

        public IReadOnlyList<LeaderboardScore> GetLocalScores(LeaderboardId leaderboardId)
        {
            if (_localLeaderboards != null)
            {
                return _localLeaderboards.GetLocalScores(leaderboardId);
            }

            return new LeaderboardScore[0];
        }

        public void ReportProgress(AchievementProgress progress)
        {
            _localAchievements?.ReportProgress(progress);

            if (!CanReportToGameCenter())
            {
                QueueAchievement(progress);
                return;
            }

            ReportAchievementToGameCenter(progress);
        }

        public bool IsCompleted(AchievementId achievementId)
        {
            return _localAchievements != null && _localAchievements.IsCompleted(achievementId);
        }

        public void SendChallenge(RunSummary runSummary, string message)
        {
            _localChallenges?.SendChallenge(runSummary, message);
        }

        public void ShowLeaderboardUI()
        {
#if UNITY_IOS && !UNITY_EDITOR
            if (!CanReportToGameCenter())
            {
                TryAuthenticate();
                return;
            }

            SBGameKitShowLeaderboard();
#else
            Debug.Log("Storm Blocks Game Center leaderboard UI is available on iOS builds.");
#endif
        }

        public void ShowAchievementsUI()
        {
#if UNITY_IOS && !UNITY_EDITOR
            if (!CanReportToGameCenter())
            {
                TryAuthenticate();
                return;
            }

            SBGameKitShowAchievements();
#else
            Debug.Log("Storm Blocks Game Center achievements UI is available on iOS builds.");
#endif
        }

        private bool CanReportToGameCenter()
        {
#if UNITY_IOS && !UNITY_EDITOR
            PollAuthenticationState();
            return _authenticated;
#else
            return false;
#endif
        }

        private void PollAuthenticationState()
        {
#if UNITY_IOS && !UNITY_EDITOR
            bool wasAuthenticated = _authenticated;
            _authenticated = SBGameKitIsAuthenticated();
            if (!wasAuthenticated && _authenticated)
            {
                FlushQueuedReports();
            }
#endif
        }

        private void QueueScore(LeaderboardScore score)
        {
            for (int i = 0; i < _pendingScores.Count; i++)
            {
                if (_pendingScores[i].LeaderboardId == score.LeaderboardId && _pendingScores[i].Score == score.Score)
                {
                    return;
                }
            }

            _pendingScores.Add(score);
        }

        private void QueueAchievement(AchievementProgress progress)
        {
            for (int i = 0; i < _pendingAchievements.Count; i++)
            {
                if (_pendingAchievements[i].AchievementId == progress.AchievementId &&
                    _pendingAchievements[i].PercentComplete >= progress.PercentComplete)
                {
                    return;
                }
            }

            _pendingAchievements.Add(progress);
        }

        private void FlushQueuedReports()
        {
            for (int i = 0; i < _pendingScores.Count; i++)
            {
                ReportScoreToGameCenter(_pendingScores[i]);
            }

            _pendingScores.Clear();

            for (int i = 0; i < _pendingAchievements.Count; i++)
            {
                ReportAchievementToGameCenter(_pendingAchievements[i]);
            }

            _pendingAchievements.Clear();
        }

        private static void ReportScoreToGameCenter(LeaderboardScore score)
        {
#if UNITY_IOS && !UNITY_EDITOR
            SBGameKitReportScore(score.Score, LeaderboardIdentifier(score.LeaderboardId));
#endif
        }

        private static void ReportAchievementToGameCenter(AchievementProgress progress)
        {
#if UNITY_IOS && !UNITY_EDITOR
            double percent = progress.PercentComplete;
            if (percent < 0.0)
            {
                percent = 0.0;
            }
            else if (percent > 100.0)
            {
                percent = 100.0;
            }

            SBGameKitReportAchievement(AchievementIdentifier(progress.AchievementId), percent);
#endif
        }

#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void SBGameKitAuthenticate();

        [DllImport("__Internal")]
        private static extern bool SBGameKitIsAuthenticated();

        [DllImport("__Internal")]
        private static extern void SBGameKitReportScore(long score, string leaderboardId);

        [DllImport("__Internal")]
        private static extern void SBGameKitReportAchievement(string achievementId, double percentComplete);

        [DllImport("__Internal")]
        private static extern void SBGameKitShowLeaderboard();

        [DllImport("__Internal")]
        private static extern void SBGameKitShowAchievements();
#endif
    }
}
