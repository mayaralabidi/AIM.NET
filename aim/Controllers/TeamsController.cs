using aim.Data;
using aim.Extensions;
using aim.Helpers;
using aim.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace aim.Controllers
{
    [Authorize]
    public class TeamsController : Controller
    {
        private readonly AimDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TeamsController(AimDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Teams.Include(t => t.Members).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var team = await _context.Teams.FirstOrDefaultAsync(m => m.Id == id);
            if (team == null) return NotFound();
            return View(team);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,InviteCode")] Team team)
        {
            if (ModelState.IsValid)
            {
                _context.Add(team);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(team);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var team = await _context.Teams.FindAsync(id);
            if (team == null) return NotFound();
            return View(team);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,InviteCode")] Team team)
        {
            if (id != team.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(team);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TeamExists(team.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(team);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var team = await _context.Teams.FirstOrDefaultAsync(m => m.Id == id);
            if (team == null) return NotFound();
            return View(team);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var team = await _context.Teams.FindAsync(id);
            if (team != null) _context.Teams.Remove(team);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Teams/Join
        public IActionResult Join() => View();

        // POST: Teams/Join
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Join(string inviteCode)
        {
            if (string.IsNullOrWhiteSpace(inviteCode))
            {
                ViewBag.Error = "Please enter an invite code.";
                return View();
            }

            var team = await _context.Teams
                .FirstOrDefaultAsync(t => t.InviteCode == inviteCode.Trim().ToUpper());

            if (team == null)
            {
                ViewBag.Error = "No team found with that code.";
                return View();
            }

            var participant = await this.GetCurrentParticipantAsync(_userManager, _context);
            if (participant == null) return RedirectToAction("Login", "Auth");

            if (participant.TeamId == team.Id)
            {
                TempData["Toast"] = "You're already in this team.";
                TempData["ToastType"] = "warning";
                return RedirectToAction("Leaderboard", new { id = team.Id });
            }

            participant.TeamId = team.Id;
            await _context.SaveChangesAsync();

            TempData["Toast"] = $"Joined {team.Name}!";
            TempData["ToastType"] = "success";
            return RedirectToAction("Leaderboard", new { id = team.Id });
        }

        // GET: Teams/Leaderboard/5
        public async Task<IActionResult> Leaderboard(int? id, int page = 1)
        {
            if (id == null) return NotFound();

            var team = await _context.Teams
                .FirstOrDefaultAsync(t => t.Id == id);
            if (team == null) return NotFound();

            var query = _context.Participants
                .Where(p => p.TeamId == id)
                .OrderByDescending(p => p.TotalScore);

            var paged = await PaginatedList<Participant>.CreateAsync(query, page, 20);

            var participant = await this.GetCurrentParticipantAsync(_userManager, _context);

            ViewBag.Team = team;
            ViewBag.CurrentParticipantId = participant?.Id ?? 0;

            return View(paged); 
        }

        private bool TeamExists(int id) => _context.Teams.Any(e => e.Id == id);
    }
}