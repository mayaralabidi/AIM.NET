using aim.Data;
using aim.Models;
using Microsoft.EntityFrameworkCore;

namespace aim.Services
{
    public interface INotificationService
    {
        Task NotifyAsync(int participantId, string message);
        Task NotifyBadgeUnlockedAsync(int participantId, string badgeName);
        Task NotifyOvertakenAsync(int participantId, string overtakenByName);
        Task NotifyStreakAtRiskAsync(int participantId, int currentStreak);
        Task<List<Notification>> GetUnreadAsync(int participantId);
        Task MarkAllReadAsync(int participantId);
    }

    public class NotificationService : INotificationService
    {
        private readonly AimDbContext _context;

        public NotificationService(AimDbContext context) => _context = context;

        public async Task NotifyAsync(int participantId, string message)
        {
            _context.Notification.Add(new Notification
            {
                ParticipantId = participantId,
                Message = message,
                SentAt = DateTime.UtcNow,
                IsRead = false
            });
            await _context.SaveChangesAsync();
        }

        public Task NotifyBadgeUnlockedAsync(int participantId, string badgeName) =>
            NotifyAsync(participantId, $"🏅 Badge unlocked: {badgeName}!");

        public Task NotifyOvertakenAsync(int participantId, string overtakenByName) =>
            NotifyAsync(participantId, $"⚔️ {overtakenByName} just overtook you on the leaderboard!");

        public Task NotifyStreakAtRiskAsync(int participantId, int currentStreak) =>
            NotifyAsync(participantId, $"🔥 Your {currentStreak}-day streak is at risk! Complete a task before midnight.");

        public async Task<List<Notification>> GetUnreadAsync(int participantId)
        {
            return await _context.Notification
                .Where(n => n.ParticipantId == participantId && !n.IsRead)
                .OrderByDescending(n => n.SentAt)
                .ToListAsync();
        }

        public async Task MarkAllReadAsync(int participantId)
        {
            var unread = await _context.Notification
                .Where(n => n.ParticipantId == participantId && !n.IsRead)
                .ToListAsync();

            foreach (var n in unread) n.IsRead = true;
            await _context.SaveChangesAsync();
        }
    }
}