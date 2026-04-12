using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using aim.Data;
using aim.Models;

namespace aim.Controllers
{
    public class TaskCompletionsController : Controller
    {
        private readonly AimDbContext _context;

        public TaskCompletionsController(AimDbContext context)
        {
            _context = context;
        }

        // GET: TaskCompletions
        public async Task<IActionResult> Index()
        {
            var aimDbContext = _context.TaskCompletions.Include(t => t.ChallengeTask).Include(t => t.Participant);
            return View(await aimDbContext.ToListAsync());
        }

        // GET: TaskCompletions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskCompletion = await _context.TaskCompletions
                .Include(t => t.ChallengeTask)
                .Include(t => t.Participant)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (taskCompletion == null)
            {
                return NotFound();
            }

            return View(taskCompletion);
        }

        // GET: TaskCompletions/Create
        public IActionResult Create()
        {
            ViewData["ChallengeTaskId"] = new SelectList(_context.ChallengeTasks, "Id", "Title");
            ViewData["ParticipantId"] = new SelectList(_context.Participants, "Id", "Name");
            return View();
        }

        // POST: TaskCompletions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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

        // GET: TaskCompletions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskCompletion = await _context.TaskCompletions.FindAsync(id);
            if (taskCompletion == null)
            {
                return NotFound();
            }
            ViewData["ChallengeTaskId"] = new SelectList(_context.ChallengeTasks, "Id", "Title", taskCompletion.ChallengeTaskId);
            ViewData["ParticipantId"] = new SelectList(_context.Participants, "Id", "Name", taskCompletion.ParticipantId);
            return View(taskCompletion);
        }

        // POST: TaskCompletions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CompletedAt,ParticipantId,ChallengeTaskId")] TaskCompletion taskCompletion)
        {
            if (id != taskCompletion.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(taskCompletion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaskCompletionExists(taskCompletion.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ChallengeTaskId"] = new SelectList(_context.ChallengeTasks, "Id", "Title", taskCompletion.ChallengeTaskId);
            ViewData["ParticipantId"] = new SelectList(_context.Participants, "Id", "Name", taskCompletion.ParticipantId);
            return View(taskCompletion);
        }

        // GET: TaskCompletions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskCompletion = await _context.TaskCompletions
                .Include(t => t.ChallengeTask)
                .Include(t => t.Participant)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (taskCompletion == null)
            {
                return NotFound();
            }

            return View(taskCompletion);
        }

        // POST: TaskCompletions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var taskCompletion = await _context.TaskCompletions.FindAsync(id);
            if (taskCompletion != null)
            {
                _context.TaskCompletions.Remove(taskCompletion);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: TaskCompletions/Complete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int taskId, int challengeId)
        {
            // ── Get the current participant ──────────────────────────────────────
            // Replace this with your actual auth once you add Identity.
            // For now, grab the first participant from seed data for testing.
            var participant = await _context.Participants.FirstOrDefaultAsync();
            if (participant == null) return NotFound();

            var task = await _context.ChallengeTasks
                .Include(t => t.Challenge)
                .FirstOrDefaultAsync(t => t.Id == taskId);
            if (task == null) return NotFound();

            // ── Guard: already completed today? ─────────────────────────────────
            var today = DateTime.UtcNow.Date;
            bool alreadyDone = await _context.TaskCompletions.AnyAsync(tc =>
                tc.ParticipantId == participant.Id &&
                tc.ChallengeTaskId == taskId &&
                tc.CompletedAt.Date == today);

            if (alreadyDone)
            {
                TempData["Error"] = "Already completed today.";
                return RedirectToAction("Details", "Challenges", new { id = challengeId });
            }

            // ── 1. Calculate points ──────────────────────────────────────────────
            int basePoints = 10;
            int multiplier = task.Difficulty switch
            {
                Models.enums.DifficultyLevel.Easy => 1,
                Models.enums.DifficultyLevel.Medium => 2,
                Models.enums.DifficultyLevel.Hard => 3,
                _ => 1
            };
            int pointsEarned = basePoints * multiplier;

            // ── 2. Update streak ─────────────────────────────────────────────────
            var now = DateTime.UtcNow;

            if (participant.LastActivityDate == null)
            {
                participant.CurrentStreak = 1;
            }
            else
            {
                double hoursSinceLast = (now - participant.LastActivityDate.Value).TotalHours;
                participant.CurrentStreak = hoursSinceLast < 24
                    ? participant.CurrentStreak + 1
                    : 1;
            }

            if (participant.CurrentStreak > participant.BestStreak)
                participant.BestStreak = participant.CurrentStreak;

            // ── 3. Update score + timestamp ──────────────────────────────────────
            participant.TotalScore += pointsEarned;
            participant.LastActivityDate = now;

            // ── 4. Audit log ─────────────────────────────────────────────────────
            _context.TaskCompletions.Add(new TaskCompletion
            {
                ParticipantId = participant.Id,
                ChallengeTaskId = taskId,
                CompletedAt = now,
                PointsEarned = pointsEarned
            });

            await _context.SaveChangesAsync();

            // ── 5. TempData for toast ────────────────────────────────────────────
            TempData["Toast"] = $"+{pointsEarned} pts";
            TempData["ToastType"] = "points";
            TempData["NewStreak"] = participant.CurrentStreak;

            return RedirectToAction("Details", "Challenges", new { id = challengeId });
        }
        private bool TaskCompletionExists(int id)
        {
            return _context.TaskCompletions.Any(e => e.Id == id);
        }
    }
}
