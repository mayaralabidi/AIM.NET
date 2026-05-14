using aim.Data;
using aim.Models;
using Microsoft.EntityFrameworkCore;

namespace aim.Services
{
    public class NotificationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<NotificationBackgroundService> _logger;

        public NotificationBackgroundService(IServiceProvider services,
                                              ILogger<NotificationBackgroundService> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    await RunChecksAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in NotificationBackgroundService");
                }

                // Run every hour
                await Task.Delay(TimeSpan.FromHours(1), ct);
            }
        }

        private async Task RunChecksAsync()
        {
            using var scope = _services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AimDbContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            var now = DateTime.UtcNow;

            // Load all participants with their Identity email
            var participants = await context.Participants.ToListAsync();
            var userEmails = await context.Users
                .Where(u => u.ParticipantId != null)
                .ToDictionaryAsync(u => u.ParticipantId!.Value, u => u.Email ?? "");

            foreach (var p in participants)
            {
                var email = userEmails.GetValueOrDefault(p.Id, "");

                // ── 1. Streak at risk (20h+ since last activity) ──────────
                if (p.CurrentStreak > 0 && p.LastActivityDate.HasValue)
                {
                    var hours = (now - p.LastActivityDate.Value).TotalHours;
                    if (hours >= 20 && hours < 21) // fire once in the 20-21h window
                    {
                        await SendNotificationAsync(context, emailService, p, email,
                            $"🔥 Streak at risk!",
                            $"🔥 Your {p.CurrentStreak}-day streak is at risk! Complete a task before midnight.",
                            $"<h2>Your streak is at risk!</h2><p>You have a <strong>{p.CurrentStreak}-day streak</strong> going. Complete at least one task today to keep it alive.</p>");
                    }
                }

                // ── 2. Motivation reminder (48h+ no activity) ─────────────
                if (p.LastActivityDate.HasValue)
                {
                    var hours = (now - p.LastActivityDate.Value).TotalHours;
                    if (hours >= 48 && hours < 49)
                    {
                        await SendNotificationAsync(context, emailService, p, email,
                            "💪 Don't give up!",
                            "💪 You haven't completed a task in 2 days. Get back on track — every day counts.",
                            "<h2>We miss you!</h2><p>You haven't completed a task in over 48 hours. Log in and complete something today — no matter how small.</p>");
                    }
                }

                // ── 3. Challenge deadline in ≤24h ─────────────────────────
                var endingSoon = await context.Participation
                    .Include(par => par.Challenge)
                    .Where(par => par.ParticipantId == p.Id
                               && !par.IsCompleted
                               && par.Challenge.EndDate != default
                               && par.Challenge.EndDate > now
                               && par.Challenge.EndDate <= now.AddHours(24))
                    .ToListAsync();

                foreach (var par in endingSoon)
                {
                    // Only notify once — check if already notified today
                    bool alreadyNotified = await context.Notification.AnyAsync(n =>
                        n.ParticipantId == p.Id &&
                        n.Message.Contains(par.Challenge.Title) &&
                        n.Message.Contains("ending") &&
                        n.SentAt.Date == now.Date);

                    if (!alreadyNotified)
                    {
                        var hoursLeft = Math.Round((par.Challenge.EndDate - now).TotalHours, 0);
                        await SendNotificationAsync(context, emailService, p, email,
                            $"⏰ Challenge ending soon!",
                            $"⏰ '{par.Challenge.Title}' is ending in {hoursLeft}h. Complete your remaining tasks now!",
                            $"<h2>Challenge ending in {hoursLeft} hours!</h2><p><strong>{par.Challenge.Title}</strong> ends soon. Log in and complete your remaining tasks before it's too late.</p>");
                    }
                }
            }

            await context.SaveChangesAsync();
        }

        private async Task SendNotificationAsync(AimDbContext context,
                                                   IEmailService emailService,
                                                   Participant participant,
                                                   string email,
                                                   string subject,
                                                   string inAppMessage,
                                                   string emailHtml)
        {
            // In-app notification
            context.Notification.Add(new Notification
            {
                ParticipantId = participant.Id,
                Message = inAppMessage,
                SentAt = DateTime.UtcNow,
                IsRead = false
            });

            // Email notification
            if (!string.IsNullOrWhiteSpace(email))
            {
                try
                {
                    await emailService.SendAsync(email, participant.Name, subject,
                        $@"
                        <div style='font-family:sans-serif;max-width:480px;margin:0 auto;padding:32px 24px;background:#111110;color:#f0ede8;border-radius:12px;'>
                            <div style='font-size:28px;font-weight:700;color:#EF9F27;margin-bottom:24px;'>AIM</div>
                            {emailHtml}
                            <div style='margin-top:32px;'>
                                <a href='#' style='background:#EF9F27;color:#111110;padding:10px 24px;border-radius:8px;text-decoration:none;font-weight:500;'>Open AIM</a>
                            </div>
                            <p style='font-size:11px;color:#888780;margin-top:24px;'>You're receiving this because you have an AIM account.</p>
                        </div>");
                }
                catch (Exception)
                {
                    // Email failed — in-app still saved, don't crash the service
                }
            }
        }
    }
}