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
    public class TeamChallengesController : Controller
    {
        private readonly AimDbContext _context;

        public TeamChallengesController(AimDbContext context)
        {
            _context = context;
        }

        // GET: TeamChallenges
        public async Task<IActionResult> Index()
        {
            var aimDbContext = _context.TeamChallenges.Include(t => t.Challenge).Include(t => t.Team);
            return View(await aimDbContext.ToListAsync());
        }

        // GET: TeamChallenges/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teamChallenge = await _context.TeamChallenges
                .Include(t => t.Challenge)
                .Include(t => t.Team)
                .FirstOrDefaultAsync(m => m.TeamId == id);
            if (teamChallenge == null)
            {
                return NotFound();
            }

            return View(teamChallenge);
        }

        // GET: TeamChallenges/Create
        public IActionResult Create()
        {
            ViewData["ChallengeId"] = new SelectList(_context.Challenges, "Id", "Title");
            ViewData["TeamId"] = new SelectList(_context.Teams, "Id", "Id");
            return View();
        }

        // POST: TeamChallenges/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TeamId,ChallengeId,RegisteredAt")] TeamChallenge teamChallenge)
        {
            if (ModelState.IsValid)
            {
                _context.Add(teamChallenge);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ChallengeId"] = new SelectList(_context.Challenges, "Id", "Title", teamChallenge.ChallengeId);
            ViewData["TeamId"] = new SelectList(_context.Teams, "Id", "Id", teamChallenge.TeamId);
            return View(teamChallenge);
        }

        // GET: TeamChallenges/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teamChallenge = await _context.TeamChallenges.FindAsync(id);
            if (teamChallenge == null)
            {
                return NotFound();
            }
            ViewData["ChallengeId"] = new SelectList(_context.Challenges, "Id", "Title", teamChallenge.ChallengeId);
            ViewData["TeamId"] = new SelectList(_context.Teams, "Id", "Id", teamChallenge.TeamId);
            return View(teamChallenge);
        }

        // POST: TeamChallenges/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TeamId,ChallengeId,RegisteredAt")] TeamChallenge teamChallenge)
        {
            if (id != teamChallenge.TeamId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(teamChallenge);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TeamChallengeExists(teamChallenge.TeamId))
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
            ViewData["ChallengeId"] = new SelectList(_context.Challenges, "Id", "Title", teamChallenge.ChallengeId);
            ViewData["TeamId"] = new SelectList(_context.Teams, "Id", "Id", teamChallenge.TeamId);
            return View(teamChallenge);
        }

        // GET: TeamChallenges/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teamChallenge = await _context.TeamChallenges
                .Include(t => t.Challenge)
                .Include(t => t.Team)
                .FirstOrDefaultAsync(m => m.TeamId == id);
            if (teamChallenge == null)
            {
                return NotFound();
            }

            return View(teamChallenge);
        }

        // POST: TeamChallenges/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var teamChallenge = await _context.TeamChallenges.FindAsync(id);
            if (teamChallenge != null)
            {
                _context.TeamChallenges.Remove(teamChallenge);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TeamChallengeExists(int id)
        {
            return _context.TeamChallenges.Any(e => e.TeamId == id);
        }
    }
}
