using aim.Data;
using aim.Models;
using aim.Models.enums;
using Microsoft.EntityFrameworkCore;

namespace aim.Services
{
    public interface IBadgeService
    {
        Task<List<Badge>> EvaluateAndAwardAsync(int participantId);
        Task<bool> AwardMilestoneTokenAsync(int participantId);
    }

    public class BadgeService : IBadgeService
    {
        private readonly AimDbContext _context;

        public BadgeService(AimDbContext context) => _context = context;

        public async Task<List<Badge>> EvaluateAndAwardAsync(int participantId)
        {
            var participant = await _context.Participants
                .Include(p => p.ParticipantBadges)
                    .ThenInclude(pb => pb.Badge)
                .FirstOrDefaultAsync(p => p.Id == participantId);

            if (participant == null) return new();

            var alreadyEarned = participant.ParticipantBadges
                .Where(pb => pb.Badge != null)
                .Select(pb => pb.Badge.Key)
                .ToHashSet();

            // --- Build stats ---
            var totalTasks = await _context.TaskCompletions
                .CountAsync(tc => tc.ParticipantId == participantId);

            var previousStreak = participant.ParticipantBadges.Any(); // has ever had a badge
            // "comeback" = had streak broken (CurrentStreak reset to 1) but BestStreak > 1
            bool isComeback = participant.BestStreak > 3 && participant.CurrentStreak == 1
                              && totalTasks > 0;

            // Time-of-day checks — latest completion
            var lastCompletion = await _context.TaskCompletions
                .Where(tc => tc.ParticipantId == participantId)
                .OrderByDescending(tc => tc.CompletedAt)
                .FirstOrDefaultAsync();

            bool isEarlyBird = lastCompletion != null && lastCompletion.CompletedAt.Hour < 8;
            bool isNightOwl = lastCompletion != null && lastCompletion.CompletedAt.Hour >= 22;

            // Overachiever: completed every task in any single challenge
            var completedTaskIds = await _context.TaskCompletions
                .Where(tc => tc.ParticipantId == participantId)
                .Select(tc => tc.ChallengeTaskId)
                .Distinct()
                .ToListAsync();

            bool isOverachiever = await _context.Challenges
                .AnyAsync(c =>
                    c.Tasks.Any() &&
                    c.Tasks.All(t => completedTaskIds.Contains(t.Id)));

            // Team rank
            int? teamRank = null;
            if (participant.TeamId.HasValue)
            {
                var memberIds = await _context.Participants
                    .Where(p => p.TeamId == participant.TeamId)
                    .OrderByDescending(p => p.TotalScore)
                    .Select(p => p.Id)
                    .ToListAsync();

                var idx = memberIds.IndexOf(participantId);
                if (idx >= 0) teamRank = idx + 1;
            }

            var stats = new ParticipantStats
            {
                CurrentStreak = participant.CurrentStreak,
                BestStreak = participant.BestStreak,
                TotalScore = participant.TotalScore,
                TotalTasks = totalTasks,
                IsInTeam = participant.TeamId.HasValue,
                TeamRank = teamRank,
                IsComeback = isComeback,
                IsEarlyBird = isEarlyBird,
                IsNightOwl = isNightOwl,
                IsOverachiever = isOverachiever
            };

            // --- Milestones: keys must exactly match seed ---
            var milestones = new List<(string Key, bool Earned)>
            {
                // Streak
                ("streak_3",   stats.CurrentStreak >= 3),
                ("streak_7",   stats.CurrentStreak >= 7),
                ("streak_14",  stats.CurrentStreak >= 14),
                ("streak_30",  stats.CurrentStreak >= 30),
                ("streak_100", stats.CurrentStreak >= 100),

                // Tasks
                ("tasks_1",    stats.TotalTasks >= 1),
                ("tasks_10",   stats.TotalTasks >= 10),
                ("tasks_50",   stats.TotalTasks >= 50),
                ("tasks_100",  stats.TotalTasks >= 100),
                ("tasks_250",  stats.TotalTasks >= 250),
                ("tasks_500",  stats.TotalTasks >= 500),

                // Score
                ("score_500",   stats.TotalScore >= 500),
                ("score_1000",  stats.TotalScore >= 1000),
                ("score_5000",  stats.TotalScore >= 5000),
                ("score_10000", stats.TotalScore >= 10000),

                // Social
                ("team_join", stats.IsInTeam),
                ("team_top3", stats.TeamRank is >= 1 and <= 3),
                ("team_top1", stats.TeamRank == 1),

                // Special
                ("comeback",      stats.IsComeback),
                ("early_bird",    stats.IsEarlyBird),
                ("night_owl",     stats.IsNightOwl),
                ("overachiever",  stats.IsOverachiever),
            };

            var newBadges = new List<Badge>();

            foreach (var (key, conditionMet) in milestones)
            {
                if (alreadyEarned.Contains(key) || !conditionMet) continue;

                var badge = await _context.Badges.FirstOrDefaultAsync(b => b.Key == key);
                if (badge == null) continue;

                _context.ParticipantBadges.Add(new ParticipantBadge
                {
                    ParticipantId = participantId,
                    BadgeId = badge.Id,
                    BadgeKey = key,
                    EarnedAt = DateTime.UtcNow
                });

                newBadges.Add(badge);
            }

            if (newBadges.Count > 0)
                await _context.SaveChangesAsync();

            return newBadges;
        }

        public async Task<bool> AwardMilestoneTokenAsync(int participantId)
        {
            var participant = await _context.Participants
                .FirstOrDefaultAsync(p => p.Id == participantId);
            if (participant == null) return false;

            // Max 1 token
            if (participant.StreakFreezeTokens >= 1) return false;

            var totalTasks = await _context.TaskCompletions
                .CountAsync(tc => tc.ParticipantId == participantId);

            bool streakMilestone = participant.CurrentStreak > 0
                                && participant.CurrentStreak % 7 == 0;

            bool taskMilestone = totalTasks > 0
                                && totalTasks % 10 == 0;

            if (!streakMilestone && !taskMilestone) return false;

            participant.StreakFreezeTokens = 1;

            string reason = streakMilestone
                ? $"{participant.CurrentStreak}-day streak"
                : $"{totalTasks} tasks completed";

            _context.Notification.Add(new Notification
            {
                ParticipantId = participantId,
                Message = $"🧊 Streak freeze token earned! ({reason}). It will auto-activate if you miss a day.",
                SentAt = DateTime.UtcNow,
                IsRead = false
            });

            await _context.SaveChangesAsync();
            return true;
        }
    }

    public class ParticipantStats
    {
        public int CurrentStreak { get; set; }
        public int BestStreak { get; set; }
        public int TotalScore { get; set; }
        public int TotalTasks { get; set; }
        public bool IsInTeam { get; set; }
        public int? TeamRank { get; set; }
        public bool IsComeback { get; set; }
        public bool IsEarlyBird { get; set; }
        public bool IsNightOwl { get; set; }
        public bool IsOverachiever { get; set; }
    }
}