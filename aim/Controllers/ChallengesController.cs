using aim.Data;
using aim.Extensions;
using aim.Helpers;
using aim.Models;
using aim.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace aim.Controllers
{
    [Authorize]
    public class ChallengesController : Controller
    {
        private readonly AimDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IChallengeService _challengeService;

        public ChallengesController(AimDbContext context,
                                     UserManager<ApplicationUser> userManager, IChallengeService challengeService)
        {
            _context = context;
            _userManager = userManager;
            _challengeService = challengeService;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var query = _context.Challenges
                .OrderByDescending(c => c.StartDate)
                .AsQueryable();

            var paged = await PaginatedList<Challenge>.CreateAsync(query, page, 10);
            return View(paged);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var challenge = await _challengeService.GetWithTasksAsync(id.Value);
            if (challenge == null) return NotFound();

            var participant = await this.GetCurrentParticipantAsync(_userManager, _context);
            if (participant == null) return RedirectToAction("Login", "Auth");

            var completedTodayIds = await _challengeService.GetCompletedTodayAsync(participant.Id);

            ViewBag.CompletedTodayIds = completedTodayIds;
            ViewBag.ParticipantScore = participant.TotalScore;
            ViewBag.ParticipantStreak = participant.CurrentStreak;
            ViewBag.IsExpired = challenge.EndDate != default && challenge.EndDate < DateTime.UtcNow;

            return View(challenge);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,PointsReward,StartDate,EndDate")] Challenge challenge)
        {
            if (ModelState.IsValid)
            {
                _context.Add(challenge);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(challenge);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var challenge = await _context.Challenges.FindAsync(id);
            if (challenge == null) return NotFound();
            return View(challenge);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,PointsReward,StartDate,EndDate")] Challenge challenge)
        {
            if (id != challenge.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(challenge);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChallengeExists(challenge.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(challenge);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var challenge = await _context.Challenges.FirstOrDefaultAsync(m => m.Id == id);
            if (challenge == null) return NotFound();
            return View(challenge);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var challenge = await _context.Challenges.FindAsync(id);
            if (challenge != null) _context.Challenges.Remove(challenge);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ChallengeExists(int id) => _context.Challenges.Any(e => e.Id == id);
    }
}