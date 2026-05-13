using aim.Data;
using aim.Models;
using Microsoft.EntityFrameworkCore;

namespace aim.Services
{
    public interface IChallengeService
    {
        Task<List<Challenge>> GetActiveAsync();
        Task<Challenge?> GetWithTasksAsync(int id);
        Task<List<int>> GetCompletedTodayAsync(int participantId);
    }

    public class ChallengeService : IChallengeService
    {
        private readonly AimDbContext _context;
        public ChallengeService(AimDbContext context) => _context = context;

        public async Task<List<Challenge>> GetActiveAsync() =>
            await _context.Challenges
                .Where(c => c.EndDate == default || c.EndDate >= DateTime.UtcNow)
                .OrderByDescending(c => c.StartDate)
                .ToListAsync();

        public async Task<Challenge?> GetWithTasksAsync(int id) =>
            await _context.Challenges
                .Include(c => c.Tasks)
                .FirstOrDefaultAsync(c => c.Id == id);

        public async Task<List<int>> GetCompletedTodayAsync(int participantId)
        {
            var today = DateTime.UtcNow.Date;
            return await _context.TaskCompletions
                .Where(tc => tc.ParticipantId == participantId &&
                             tc.CompletedAt.Date == today)
                .Select(tc => tc.ChallengeTaskId)
                .ToListAsync();
        }
    }
}