using aim.Data;
using aim.Models;
using Microsoft.EntityFrameworkCore;

namespace aim.Services
{
    public class BadgeService
    {
        private readonly AimDbContext _context;

        private static readonly List<(string Key, string Name, Func<ParticipantStats, bool> Condition)> Milestones = new()
        {
            ("streak_7",   "Week Warrior",  s => s.CurrentStreak >= 7),
            ("streak_15",  "On Fire",       s => s.CurrentStreak >= 15),
            ("streak_30",  "Unstoppable",   s => s.CurrentStreak >= 30),
            ("tasks_10",   "Getting Started",s => s.TotalTasks >= 10),
            ("tasks_50",   "Half Century",  s => s.TotalTasks >= 50),
            ("tasks_100",  "Centurion",     s => s.TotalTasks >= 100),
            ("score_500",  "Rising Star",   s => s.TotalScore >= 500),
            ("score_2000", "Elite",         s => s.TotalScore >= 2000),
            ("hard_first", "Hard Mode",     s => s.HardTasksDone >= 1),
        };

        public BadgeService(AimDbContext context) => _context = context;

        public async Task<List<Badge>> EvaluateAndAwardAsync(int participantId)
        {
            var participant = await _context.Participants
                .Include(p => p.ParticipantBadges)
                .FirstOrDefaultAsync(p => p.Id == participantId);

            if (participant == null) return new();

            // Get keys of already earned badges via the Badge navigation
            var alreadyEarned = participant.ParticipantBadges
                .Select(pb => pb.Badge.Key)
                .ToHashSet();

            var stats = new ParticipantStats
            {
                CurrentStreak = participant.CurrentStreak,
                TotalScore = participant.TotalScore,
                TotalTasks = await _context.TaskCompletions
                    .CountAsync(tc => tc.ParticipantId == participantId),
                HardTasksDone = await _context.TaskCompletions
                    .Include(tc => tc.ChallengeTask)
                    .CountAsync(tc => tc.ParticipantId == participantId
                        && tc.ChallengeTask.Difficulty == Models.enums.DifficultyLevel.Hard)
            };

            var newBadges = new List<Badge>();

            foreach (var (key, name, condition) in Milestones)
            {
                if (!alreadyEarned.Contains(key) && condition(stats))
                {
                    var badge = await _context.Badges
                        .FirstOrDefaultAsync(b => b.Key == key);
                    if (badge == null) continue;

                    _context.ParticipantBadges.Add(new ParticipantBadge
                    {
                        ParticipantId = participantId,
                        BadgeId = badge.Id,
                        EarnedAt = DateTime.UtcNow
                    });

                    newBadges.Add(badge);
                }
            }

            if (newBadges.Any())
                await _context.SaveChangesAsync();

            return newBadges;
        }
    }

    public class ParticipantStats
    {
        public int CurrentStreak { get; set; }
        public int TotalScore { get; set; }
        public int TotalTasks { get; set; }
        public int HardTasksDone { get; set; }
    }
}