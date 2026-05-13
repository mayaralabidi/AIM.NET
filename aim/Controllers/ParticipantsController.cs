using aim.Data;
using aim.Extensions;
using aim.Models;
using aim.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace aim.Controllers
{
    [Authorize]
    public class ParticipantsController : Controller
    {
        private readonly AimDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IParticipantService _participantService;

        public ParticipantsController(AimDbContext context, UserManager<ApplicationUser> userManager,
                               IParticipantService participantService)
        {
            _context = context;
            _userManager = userManager;
            _participantService = participantService;
        }

        public async Task<IActionResult> Index()
        {
            var aimDbContext = _context.Participants.Include(p => p.Team);
            return View(await aimDbContext.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var participant = await _context.Participants
                .Include(p => p.Team)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (participant == null) return NotFound();
            return View(participant);
        }

        public IActionResult Create()
        {
            ViewData["TeamId"] = new SelectList(_context.Teams, "Id", "Id");
            return View();
        }

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

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var participant = await _context.Participants.FindAsync(id);
            if (participant == null) return NotFound();
            ViewData["TeamId"] = new SelectList(_context.Teams, "Id", "Id", participant.TeamId);
            return View(participant);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Email,TotalScore,CurrentStreak,BestStreak,LastActivityDate,TeamId")] Participant participant)
        {
            if (id != participant.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(participant);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ParticipantExists(participant.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["TeamId"] = new SelectList(_context.Teams, "Id", "Id", participant.TeamId);
            return View(participant);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var participant = await _context.Participants
                .Include(p => p.Team)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (participant == null) return NotFound();
            return View(participant);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var participant = await _context.Participants.FindAsync(id);
            if (participant != null) _context.Participants.Remove(participant);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Participants/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var participant = await this.GetCurrentParticipantAsync(_userManager, _context);
            if (participant == null) return RedirectToAction("Login", "Auth");

            var data = await _participantService.GetDashboardDataAsync(participant.Id);

            participant = await _context.Participants
                .Include(p => p.Team)
                .FirstOrDefaultAsync(p => p.Id == participant.Id);
            if (participant == null) return RedirectToAction("Login", "Auth");

            var now = DateTime.UtcNow;
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var last28 = now.AddDays(-27).Date;

            var badges = await _context.ParticipantBadges
                .Include(pb => pb.Badge)
                .Where(pb => pb.ParticipantId == participant.Id)
                .ToListAsync();

            var completions = await _context.TaskCompletions
                .Where(tc => tc.ParticipantId == participant.Id)
                .ToListAsync();

            var participations = await _context.Participation
                .Include(p => p.Challenge)
                    .ThenInclude(c => c.Tasks)
                .Where(p => p.ParticipantId == participant.Id)
                .ToListAsync();

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

            participant.ParticipantBadges = badges;

            //ViewBag.Teammates = teammates;
            //ViewBag.TeamRank = teamRank;
            ViewBag.TasksThisMonth = completions.Count(tc => tc.CompletedAt >= monthStart);
            ViewBag.StreakAtRisk = participant.CurrentStreak > 0
                && participant.LastActivityDate.HasValue
                && (now - participant.LastActivityDate.Value).TotalHours >= 20;
            ViewBag.HoursLeft = participant.LastActivityDate.HasValue
                ? Math.Max(0, Math.Ceiling(24 - (now - participant.LastActivityDate.Value).TotalHours))
                : 0;

            ViewBag.StreakCalendar = data.StreakCalendar;
            ViewBag.ChallengeProgresses = data.ChallengeProgresses;
            ViewBag.Teammates = data.Teammates;
            ViewBag.TeamRank = data.TeamRank;
            ViewBag.TasksThisMonth = data.TasksThisMonth;
            ViewBag.StreakAtRisk = data.StreakAtRisk;
            ViewBag.HoursLeft = data.HoursLeft;

            return View(participant);
        }

        // GET: Participants/Badges
        public async Task<IActionResult> Badges()
        {
            var participant = await this.GetCurrentParticipantAsync(_userManager, _context);
            if (participant == null) return RedirectToAction("Login", "Auth");

            participant = await _context.Participants
                .Include(p => p.ParticipantBadges)
                    .ThenInclude(pb => pb.Badge)
                .FirstOrDefaultAsync(p => p.Id == participant.Id);
            if (participant == null) return RedirectToAction("Login", "Auth");

            var allBadges = await _context.Badges.ToListAsync();

            var earnedKeys = participant.ParticipantBadges
                .Where(pb => pb.Badge != null)
                .Select(pb => pb.BadgeKey)
                .ToHashSet();

            var totalTasks = await _context.TaskCompletions
                .CountAsync(tc => tc.ParticipantId == participant.Id);

            var hardTasks = await _context.TaskCompletions
                .Include(tc => tc.ChallengeTask)
                .CountAsync(tc => tc.ParticipantId == participant.Id
                    && tc.ChallengeTask.Difficulty == aim.Models.enums.DifficultyLevel.Hard);

            ViewBag.EarnedKeys = earnedKeys;
            ViewBag.TotalTasks = totalTasks;
            ViewBag.HardTasks = hardTasks;
            ViewBag.CurrentStreak = participant.CurrentStreak;
            ViewBag.TotalScore = participant.TotalScore;
            ViewBag.AllBadges = allBadges;
            ViewBag.EarnedBadges = participant.ParticipantBadges
                .OrderByDescending(pb => pb.EarnedAt)
                .ToList();

            return View(participant);
        }

        private bool ParticipantExists(int id) =>
            _context.Participants.Any(e => e.Id == id);
    }
}