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

        private bool TaskCompletionExists(int id)
        {
            return _context.TaskCompletions.Any(e => e.Id == id);
        }
    }
}
