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
    public class PersonalGoalsController : Controller
    {
        private readonly AimDbContext _context;

        public PersonalGoalsController(AimDbContext context)
        {
            _context = context;
        }

        // GET: PersonalGoals
        public async Task<IActionResult> Index()
        {
            var aimDbContext = _context.PersonalGoal.Include(p => p.Participant);
            return View(await aimDbContext.ToListAsync());
        }

        // GET: PersonalGoals/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var personalGoal = await _context.PersonalGoal
                .Include(p => p.Participant)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (personalGoal == null)
            {
                return NotFound();
            }

            return View(personalGoal);
        }

        // GET: PersonalGoals/Create
        public IActionResult Create()
        {
            ViewData["ParticipantId"] = new SelectList(_context.Participants, "Id", "Name");
            return View();
        }

        // POST: PersonalGoals/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Description,TargetValue,CurrentValue,TargetDate,IsReached,ParticipantId")] PersonalGoal personalGoal)
        {
            if (ModelState.IsValid)
            {
                _context.Add(personalGoal);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ParticipantId"] = new SelectList(_context.Participants, "Id", "Name", personalGoal.ParticipantId);
            return View(personalGoal);
        }

        // GET: PersonalGoals/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var personalGoal = await _context.PersonalGoal.FindAsync(id);
            if (personalGoal == null)
            {
                return NotFound();
            }
            ViewData["ParticipantId"] = new SelectList(_context.Participants, "Id", "Name", personalGoal.ParticipantId);
            return View(personalGoal);
        }

        // POST: PersonalGoals/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Description,TargetValue,CurrentValue,TargetDate,IsReached,ParticipantId")] PersonalGoal personalGoal)
        {
            if (id != personalGoal.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(personalGoal);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PersonalGoalExists(personalGoal.Id))
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
            ViewData["ParticipantId"] = new SelectList(_context.Participants, "Id", "Name", personalGoal.ParticipantId);
            return View(personalGoal);
        }

        // GET: PersonalGoals/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var personalGoal = await _context.PersonalGoal
                .Include(p => p.Participant)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (personalGoal == null)
            {
                return NotFound();
            }

            return View(personalGoal);
        }

        // POST: PersonalGoals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var personalGoal = await _context.PersonalGoal.FindAsync(id);
            if (personalGoal != null)
            {
                _context.PersonalGoal.Remove(personalGoal);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PersonalGoalExists(int id)
        {
            return _context.PersonalGoal.Any(e => e.Id == id);
        }
    }
}
