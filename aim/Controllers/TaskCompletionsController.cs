using aim.Data;
using aim.Extensions;
using aim.Models;
using aim.Models.enums;
using aim.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace aim.Controllers
{
    [Authorize]
    public class TaskCompletionsController : Controller
    {
        private readonly AimDbContext _context;
        private readonly IBadgeService _badgeService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notificationService;

        public TaskCompletionsController(AimDbContext context,
                                          IBadgeService badgeService,
                                          INotificationService notificationService,
                                          UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _badgeService = badgeService;
            _notificationService = notificationService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var aimDbContext = _context.TaskCompletions
                .Include(t => t.ChallengeTask)
                .Include(t => t.Participant);
            return View(await aimDbContext.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var taskCompletion = await _context.TaskCompletions
                .Include(t => t.ChallengeTask)
                .Include(t => t.Participant)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (taskCompletion == null) return NotFound();
            return View(taskCompletion);
        }

        public IActionResult Create()
        {
            ViewData["ChallengeTaskId"] = new SelectList(_context.ChallengeTasks, "Id", "Title");
            ViewData["ParticipantId"] = new SelectList(_context.Participants, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CompletedAt,ParticipantId,ChallengeTaskId")] TaskCompletion taskCompletion)
        {
            if (ModelState.IsValid)
            {
                _context.Add(taskCompletion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ChallengeTaskId"] = new SelectList(_context.ChallengeTasks, "Id", "Title", taskCompletion.ChallengeTaskId);
            ViewData["ParticipantId"] = new SelectList(_context.Participants, "Id", "Name", taskCompletion.ParticipantId);
            return View(taskCompletion);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var taskCompletion = await _context.TaskCompletions.FindAsync(id);
            if (taskCompletion == null) return NotFound();
            ViewData["ChallengeTaskId"] = new SelectList(_context.ChallengeTasks, "Id", "Title", taskCompletion.ChallengeTaskId);
            ViewData["ParticipantId"] = new SelectList(_context.Participants, "Id", "Name", taskCompletion.ParticipantId);
            return View(taskCompletion);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CompletedAt,ParticipantId,ChallengeTaskId")] TaskCompletion taskCompletion)
        {
            if (id != taskCompletion.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(taskCompletion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaskCompletionExists(taskCompletion.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ChallengeTaskId"] = new SelectList(_context.ChallengeTasks, "Id", "Title", taskCompletion.ChallengeTaskId);
            ViewData["ParticipantId"] = new SelectList(_context.Participants, "Id", "Name", taskCompletion.ParticipantId);
            return View(taskCompletion);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var taskCompletion = await _context.TaskCompletions
                .Include(t => t.ChallengeTask)
                .Include(t => t.Participant)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (taskCompletion == null) return NotFound();
            return View(taskCompletion);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var taskCompletion = await _context.TaskCompletions.FindAsync(id);
            if (taskCompletion != null) _context.TaskCompletions.Remove(taskCompletion);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: TaskCompletions/Complete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int taskId)
        {
            var participant = await this.GetCurrentParticipantAsync(_userManager, _context);
            if (participant == null) return RedirectToAction("Login", "Auth");

            var task = await _context.ChallengeTasks
                .Include(t => t.Challenge)
                .FirstOrDefaultAsync(t => t.Id == taskId);
            if (task == null) return NotFound();

            if (task.Challenge == null)
            {
                TempData["Toast"] = "Invalid task.";
                TempData["ToastType"] = "error";
                return RedirectToAction("Index", "Challenges");
            }

            var now = DateTime.UtcNow;
            var today = DateOnly.FromDateTime(now);

            bool alreadyDone = await _context.TaskCompletions.AnyAsync(tc =>
                tc.ParticipantId == participant.Id &&
                tc.ChallengeTaskId == taskId &&
                DateOnly.FromDateTime(tc.CompletedAt) == today);

            if (alreadyDone)
            {
                TempData["Toast"] = "Already completed today!";
                TempData["ToastType"] = "warning";
                return RedirectToAction("Details", "Challenges", new { id = task.ChallengeId });
            }

            // Points
            int multiplier = task.Difficulty switch
            {
                DifficultyLevel.Easy => 1,
                DifficultyLevel.Medium => 2,
                DifficultyLevel.Hard => 3,
                _ => 1
            };
            int pointsEarned = 10 * multiplier;
            participant.TotalScore += pointsEarned;

            // Streak
            if (participant.LastActivityDate == null)
            {
                participant.CurrentStreak = 1;
            }
            else
            {
                var lastDate = DateOnly.FromDateTime(participant.LastActivityDate.Value);
                if (lastDate == today)
                {
                    // same day — no change
                }
                else if (lastDate == today.AddDays(-1))
                {
                    participant.CurrentStreak += 1;
                }
                else
                {
                    participant.CurrentStreak = 1;
                }
            }

            if (participant.CurrentStreak > participant.BestStreak)
                participant.BestStreak = participant.CurrentStreak;

            participant.LastActivityDate = now;

            _context.TaskCompletions.Add(new TaskCompletion
            {
                ParticipantId = participant.Id,
                ChallengeTaskId = taskId,
                PointsEarned = pointsEarned,
                CompletedAt = now
            });

            await _context.SaveChangesAsync();

            // Badge evaluation
            var newBadges = await _badgeService.EvaluateAndAwardAsync(participant.Id);

            if (newBadges.Count > 0)
            {
                var names = string.Join(", ", newBadges.Select(b => b.Name));
                TempData["Toast"] = $"+{pointsEarned} pts · Streak {participant.CurrentStreak} 🔥 · 🏅 {names}!";
                TempData["ToastType"] = "badge";
            }
            else
            {
                TempData["Toast"] = $"+{pointsEarned} pts · Streak {participant.CurrentStreak} 🔥";
                TempData["ToastType"] = "success";
            }

            // Notify badge unlocks
            foreach (var badge in newBadges)
                await _notificationService.NotifyBadgeUnlockedAsync(participant.Id, badge.Name);

            // Notify streak at risk (20h+ since last activity on next login — check here too)
            if (participant.CurrentStreak > 0)
            {
                var hoursActive = (DateTime.UtcNow - participant.LastActivityDate!.Value).TotalHours;
                if (hoursActive >= 20)
                    await _notificationService.NotifyStreakAtRiskAsync(participant.Id, participant.CurrentStreak);
            }

            // Notify overtaken teammates
            if (participant.TeamId.HasValue)
            {
                var overtaken = await _context.Participants
                    .Where(p => p.TeamId == participant.TeamId
                             && p.Id != participant.Id
                             && p.TotalScore < participant.TotalScore)
                    .ToListAsync();

                foreach (var p in overtaken)
                    await _notificationService.NotifyOvertakenAsync(p.Id, participant.Name);
            }

            return RedirectToAction("Details", "Challenges", new { id = task.ChallengeId });
        }

        private bool TaskCompletionExists(int id) =>
            _context.TaskCompletions.Any(e => e.Id == id);
    }
}