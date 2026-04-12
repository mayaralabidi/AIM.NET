using aim.Data;
using aim.Models;
using Microsoft.EntityFrameworkCore;

namespace aim.Services
{
    public class TaskCompletionResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public int PointsEarned { get; set; }
        public bool StreakIncremented { get; set; }
        public int NewStreak { get; set; }
        public List<Badge> NewlyUnlockedBadges { get; set; } = new();
    }

    public class TaskCompletionService
    {
        private readonly AimDbContext _context;
        private readonly BadgeService _badgeService;

        public TaskCompletionService(AimDbContext context, BadgeService badgeService)
        {
            _context = context;
            _badgeService = badgeService;
        }

        public async Task<TaskCompletionResult> CompleteTaskAsync(int taskId, int participantId)
        {
            // Load task and participant in one round-trip
            var task = await _context.ChallengeTasks
                .Include(t => t.Challenge)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            var participant = await _context.Participants
                .FirstOrDefaultAsync(p => p.Id == participantId);

            if (task == null || participant == null)
                return new TaskCompletionResult { Success = false, ErrorMessage = "Task or participant not found." };

            // Guard: already completed today?
            var today = DateTime.UtcNow.Date;
            bool alreadyDone = await _context.TaskCompletions.AnyAsync(tc =>
                tc.ParticipantId == participantId &&
                tc.ChallengeTaskId == taskId &&
                tc.CompletedAt.Date == today);

            if (alreadyDone)
                return new TaskCompletionResult { Success = false, ErrorMessage = "Already completed today." };

            // 1. Calculate points
            int basePoints = 10;
            int multiplier = task.Difficulty switch
            {
                Models.enums.DifficultyLevel.Easy => 1,
                Models.enums.DifficultyLevel.Medium => 2,
                Models.enums.DifficultyLevel.Hard => 3,
                _ => 1
            };
            int pointsEarned = basePoints * multiplier;

            // 2. Update streak
            var now = DateTime.UtcNow;
            bool streakIncremented;

            if (participant.LastActivityDate == null)
            {
                // First ever action
                participant.CurrentStreak = 1;
                streakIncremented = true;
            }
            else
            {
                var hoursSinceLast = (now - participant.LastActivityDate.Value).TotalHours;
                if (hoursSinceLast < 24)
                {
                    participant.CurrentStreak++;
                    streakIncremented = true;
                }
                else
                {
                    participant.CurrentStreak = 1; // reset, but today counts
                    streakIncremented = false;
                }
            }

            // Track personal best
            if (participant.CurrentStreak > participant.BestStreak)
                participant.BestStreak = participant.CurrentStreak;

            // 3. Update score and timestamp
            participant.TotalScore += pointsEarned;
            participant.LastActivityDate = now;

            // 4. Audit log
            var completion = new TaskCompletion
            {
                ParticipantId = participantId,
                ChallengeTaskId = taskId,
                CompletedAt = now,
                PointsEarned = pointsEarned
            };
            _context.TaskCompletions.Add(completion);

            await _context.SaveChangesAsync();

            // 5. Check badge milestones (after save so counts are accurate)
            var newBadges = await _badgeService.EvaluateAndAwardAsync(participantId);

            return new TaskCompletionResult
            {
                Success = true,
                PointsEarned = pointsEarned,
                StreakIncremented = streakIncremented,
                NewStreak = participant.CurrentStreak,
                NewlyUnlockedBadges = newBadges
            };
        }
    }
}