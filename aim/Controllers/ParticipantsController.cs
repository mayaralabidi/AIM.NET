using aim.Data;
using aim.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace aim.Controllers
{
    public class ParticipantsController : Controller
    {
        private readonly AimDbContext _context;

        public ParticipantsController(AimDbContext context)
        {
            _context = context;
        }

        // GET: Participants
        public async Task<IActionResult> Index()
        {
            var aimDbContext = _context.Participants.Include(p => p.Team);
            return View(await aimDbContext.ToListAsync());
        }

        // GET: Participants/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var participant = await _context.Participants
                .Include(p => p.Team)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (participant == null)
            {
                return NotFound();
            }

            return View(participant);
        }

        // GET: Participants/Create
        public IActionResult Create()
        {
            ViewData["TeamId"] = new SelectList(_context.Teams, "Id", "Id");
            return View();
        }

        // POST: Participants/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Email,TotalScore,CurrentStreak,LastActivityDate,TeamId")] Participant participant)
        {
            if (ModelState.IsValid)
            {
                _context.Add(participant);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["TeamId"] = new SelectList(_context.Teams, "Id", "Id", participant.TeamId);
            return View(participant);
        }

        // GET: Participants/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var participant = await _context.Participants.FindAsync(id);
            if (participant == null)
            {
                return NotFound();
            }
            ViewData["TeamId"] = new SelectList(_context.Teams, "Id", "Id", participant.TeamId);
            return View(participant);
        }

        // POST: Participants/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Email,TotalScore,CurrentStreak,LastActivityDate,TeamId")] Participant participant)
        {
            if (id != participant.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(participant);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ParticipantExists(participant.Id))
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
            ViewData["TeamId"] = new SelectList(_context.Teams, "Id", "Id", participant.TeamId);
            return View(participant);
        }

        // GET: Participants/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var participant = await _context.Participants
                .Include(p => p.Team)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (participant == null)
            {
                return NotFound();
            }

            return View(participant);
        }

        // POST: Participants/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var participant = await _context.Participants.FindAsync(id);
            if (participant != null)
            {
                _context.Participants.Remove(participant);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Participants/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            // Simple load first — no complex includes
            var participant = await _context.Participants
                .Include(p => p.Team)
                .FirstOrDefaultAsync();

            if (participant == null) return NotFound();

            var now = DateTime.UtcNow;
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var last28 = now.AddDays(-27).Date;

            // Load badges separately
            var badges = await _context.ParticipantBadges
                .Include(pb => pb.Badge)
                .Where(pb => pb.ParticipantId == participant.Id)
                .ToListAsync();

            // Load completions separately
            var completions = await _context.TaskCompletions
                .Where(tc => tc.ParticipantId == participant.Id)
                .ToListAsync();

            // Load participations separately
            var participations = await _context.Participation
                .Include(p => p.Challenge)
                    .ThenInclude(c => c.Tasks)
                .Where(p => p.ParticipantId == participant.Id)
                .ToListAsync();

            // Streak calendar
            var activeDates = completions
                .Where(tc => tc.CompletedAt.Date >= last28)
                .Select(tc => tc.CompletedAt.Date)
                .ToHashSet();

            ViewBag.StreakCalendar = Enumerable.Range(0, 28)
                .Select(i => last28.AddDays(i))
                .Select(d => new {
                    Date = d,
                    HasActivity = activeDates.Contains(d),
                    IsToday = d == now.Date
                })
                .ToList();

            // Challenge progresses
            var completedTaskIds = completions
                .Select(tc => tc.ChallengeTaskId)
                .ToHashSet();

            ViewBag.ChallengeProgresses = participations
                .Select(par => new {
                    ChallengeId = par.Challenge.Id,
                    Title = par.Challenge.Title,
                    TotalTasks = par.Challenge.Tasks.Count,
                    CompletedTasks = par.Challenge.Tasks.Count(t => completedTaskIds.Contains(t.Id)),
                    PointsEarned = completions
                        .Where(tc => par.Challenge.Tasks.Select(t => t.Id).Contains(tc.ChallengeTaskId))
                        .Sum(tc => tc.PointsEarned),
                    DaysLeft = par.Challenge.EndDate != default(DateTime)
                        ? Math.Max(0, (par.Challenge.EndDate - now).Days)
                        : -1
                })
                .ToList();

            // Team leaderboard
            var teammates = new List<Participant>();
            int teamRank = 0;

            if (participant.TeamId != null)
            {
                teammates = await _context.Participants
                    .Where(p => p.TeamId == participant.TeamId)
                    .OrderByDescending(p => p.TotalScore)
                    .Take(5)
                    .ToListAsync();

                teamRank = teammates.FindIndex(p => p.Id == participant.Id) + 1;
            }

            // Attach badges to participant for the view
            participant.ParticipantBadges = badges;

            ViewBag.Teammates = teammates;
            ViewBag.TeamRank = teamRank;
            ViewBag.TasksThisMonth = completions.Count(tc => tc.CompletedAt >= monthStart);
            ViewBag.StreakAtRisk = participant.CurrentStreak > 0
                && participant.LastActivityDate.HasValue
                && (now - participant.LastActivityDate.Value).TotalHours >= 20;
            ViewBag.HoursLeft = participant.LastActivityDate.HasValue
                ? Math.Max(0, Math.Ceiling(24 - (now - participant.LastActivityDate.Value).TotalHours))
                : 0;

            return View(participant);
        }
        private bool ParticipantExists(int id)
        {
            return _context.Participants.Any(e => e.Id == id);
        }
    }
}
