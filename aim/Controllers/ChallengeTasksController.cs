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
    public class ChallengeTasksController : Controller
    {
        private readonly AimDbContext _context;

        public ChallengeTasksController(AimDbContext context)
        {
            _context = context;
        }

        // GET: ChallengeTasks
        public async Task<IActionResult> Index()
        {
            var aimDbContext = _context.ChallengeTasks.Include(c => c.Challenge);
            return View(await aimDbContext.ToListAsync());
        }

        // GET: ChallengeTasks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var challengeTask = await _context.ChallengeTasks
                .Include(c => c.Challenge)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (challengeTask == null)
            {
                return NotFound();
            }

            return View(challengeTask);
        }

        // GET: ChallengeTasks/Create
        public IActionResult Create()
        {
            ViewData["ChallengeId"] = new SelectList(_context.Challenges, "Id", "Title");
            return View();
        }

        // POST: ChallengeTasks/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Difficulty,PointsValue,ChallengeId")] ChallengeTask challengeTask)
        {
            if (ModelState.IsValid)
            {
                _context.Add(challengeTask);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ChallengeId"] = new SelectList(_context.Challenges, "Id", "Title", challengeTask.ChallengeId);
            return View(challengeTask);
        }

        // GET: ChallengeTasks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var challengeTask = await _context.ChallengeTasks.FindAsync(id);
            if (challengeTask == null)
            {
                return NotFound();
            }
            ViewData["ChallengeId"] = new SelectList(_context.Challenges, "Id", "Title", challengeTask.ChallengeId);
            return View(challengeTask);
        }

        // POST: ChallengeTasks/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Difficulty,PointsValue,ChallengeId")] ChallengeTask challengeTask)
        {
            if (id != challengeTask.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(challengeTask);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChallengeTaskExists(challengeTask.Id))
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
            ViewData["ChallengeId"] = new SelectList(_context.Challenges, "Id", "Title", challengeTask.ChallengeId);
            return View(challengeTask);
        }

        // GET: ChallengeTasks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var challengeTask = await _context.ChallengeTasks
                .Include(c => c.Challenge)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (challengeTask == null)
            {
                return NotFound();
            }

            return View(challengeTask);
        }

        // POST: ChallengeTasks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var challengeTask = await _context.ChallengeTasks.FindAsync(id);
            if (challengeTask != null)
            {
                _context.ChallengeTasks.Remove(challengeTask);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ChallengeTaskExists(int id)
        {
            return _context.ChallengeTasks.Any(e => e.Id == id);
        }
    }
}
