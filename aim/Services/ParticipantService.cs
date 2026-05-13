using aim.Data;
using aim.Models;
using Microsoft.EntityFrameworkCore;

namespace aim.Services
{
    public interface IParticipantService
    {
        Task<DashboardData> GetDashboardDataAsync(int participantId);
    }

    public class DashboardData
    {
        public Participant Participant { get; set; } = null!;
        public List<object> StreakCalendar { get; set; } = new();
        public List<ChallengeProgress> ChallengeProgresses { get; set; } = new();
        public List<Participant> Teammates { get; set; } = new();
        public int TeamRank { get; set; }
        public int TasksThisMonth { get; set; }
        public bool StreakAtRisk { get; set; }
        public double HoursLeft { get; set; }
    }

    public class ChallengeProgress
    {
        public int ChallengeId { get; set; }
        public string Title { get; set; } = "";
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int PointsEarned { get; set; }
        public int DaysLeft { get; set; }
    }

    public class ParticipantService : IParticipantService
    {
        private readonly AimDbContext _context;
        public ParticipantService(AimDbContext context) => _context = context;

        public async Task<DashboardData> GetDashboardDataAsync(int participantId)
        {
            var now = DateTime.UtcNow;
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var last28 = now.AddDays(-27).Date;

            var participant = await _context.Participants
                .Include(p => p.Team)
                .FirstOrDefaultAsync(p => p.Id == participantId)
                ?? throw new InvalidOperationException("Participant not found");

            var badges = await _context.ParticipantBadges
                .Include(pb => pb.Badge)
                .Where(pb => pb.ParticipantId == participantId)
                .ToListAsync();

            var completions = await _context.TaskCompletions
                .Where(tc => tc.ParticipantId == participantId)
                .ToListAsync();

            var participations = await _context.Participation
                .Include(p => p.Challenge).ThenInclude(c => c.Tasks)
                .Where(p => p.ParticipantId == participantId)
                .ToListAsync();

            var activeDates = completions
                .Where(tc => tc.CompletedAt.Date >= last28)
                .Select(tc => tc.CompletedAt.Date)
                .ToHashSet();

            var streakCalendar = Enumerable.Range(0, 28)
                .Select(i => last28.AddDays(i))
                .Select(d => (object)new
                {
                    Date = d,
                    HasActivity = activeDates.Contains(d),
                    IsToday = d == now.Date
                })
                .ToList();

            var completedTaskIds = completions.Select(tc => tc.ChallengeTaskId).ToHashSet();

            var progresses = participations.Select(par => new ChallengeProgress
            {
                ChallengeId = par.Challenge.Id,
                Title = par.Challenge.Title,
                TotalTasks = par.Challenge.Tasks.Count,
                CompletedTasks = par.Challenge.Tasks.Count(t => completedTaskIds.Contains(t.Id)),
                PointsEarned = completions
                    .Where(tc => par.Challenge.Tasks.Select(t => t.Id).Contains(tc.ChallengeTaskId))
                    .Sum(tc => tc.PointsEarned),
                DaysLeft = par.Challenge.EndDate != default
                    ? Math.Max(0, (par.Challenge.EndDate - now).Days)
                    : -1
            }).ToList();

            var teammates = new List<Participant>();
            int teamRank = 0;

            if (participant.TeamId.HasValue)
            {
                teammates = await _context.Participants
                    .Where(p => p.TeamId == participant.TeamId)
                    .OrderByDescending(p => p.TotalScore)
                    .Take(5)
                    .ToListAsync();
                teamRank = teammates.FindIndex(p => p.Id == participantId) + 1;
            }

            participant.ParticipantBadges = badges;

            return new DashboardData
            {
                Participant = participant,
                StreakCalendar = streakCalendar,
                ChallengeProgresses = progresses,
                Teammates = teammates,
                TeamRank = teamRank,
                TasksThisMonth = completions.Count(tc => tc.CompletedAt >= monthStart),
                StreakAtRisk = participant.CurrentStreak > 0
                                      && participant.LastActivityDate.HasValue
                                      && (now - participant.LastActivityDate.Value).TotalHours >= 20,
                HoursLeft = participant.LastActivityDate.HasValue
                                      ? Math.Max(0, Math.Ceiling(24 - (now - participant.LastActivityDate.Value).TotalHours))
                                      : 0
            };
        }
    }
}