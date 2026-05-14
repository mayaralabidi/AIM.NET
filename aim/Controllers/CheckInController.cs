using aim.Data;
using aim.Extensions;
using aim.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace aim.Controllers
{
    [Authorize]
    public class CheckInController : BaseController
    {
        public CheckInController(AimDbContext context,
                                  UserManager<ApplicationUser> userManager)
            : base(context, userManager) { }

        public async Task<IActionResult> Index()
        {
            var participant = await this.GetCurrentParticipantAsync(_userManager, _context);
            if (participant == null) return RedirectToAction("Login", "Auth");

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var todayCheckIn = await _context.DailyCheckIns
                .FirstOrDefaultAsync(c => c.ParticipantId == participant.Id &&
                                          DateOnly.FromDateTime(c.Date) == today);

            var last30 = await _context.DailyCheckIns
                .Where(c => c.ParticipantId == participant.Id &&
                            c.Date >= DateTime.UtcNow.AddDays(-30))
                .OrderByDescending(c => c.Date)
                .ToListAsync();

            var tasksByDay = await _context.TaskCompletions
                .Where(tc => tc.ParticipantId == participant.Id &&
                             tc.CompletedAt >= DateTime.UtcNow.AddDays(-30))
                .GroupBy(tc => tc.CompletedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            ViewBag.TodayCheckIn = todayCheckIn;
            ViewBag.Last30 = last30;
            ViewBag.TasksByDay = tasksByDay.ToDictionary(x => x.Date, x => x.Count);

            return View(participant);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int mood, int energy, string? note)
        {
            var participant = await this.GetCurrentParticipantAsync(_userManager, _context);
            if (participant == null) return RedirectToAction("Login", "Auth");

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var existing = await _context.DailyCheckIns
                .FirstOrDefaultAsync(c => c.ParticipantId == participant.Id &&
                                          DateOnly.FromDateTime(c.Date) == today);

            if (existing != null)
            {
                existing.Mood = (MoodLevel)mood;
                existing.Energy = (EnergyLevel)energy;
                existing.Note = note?.Trim();
            }
            else
            {
                _context.DailyCheckIns.Add(new DailyCheckIn
                {
                    Date = DateTime.UtcNow,
                    Mood = (MoodLevel)mood,
                    Energy = (EnergyLevel)energy,
                    Note = note?.Trim(),
                    ParticipantId = participant.Id
                });
            }

            await _context.SaveChangesAsync();

            TempData["Toast"] = "Check-in saved!";
            TempData["ToastType"] = "success";
            return RedirectToAction("Index");
        }
    }
}