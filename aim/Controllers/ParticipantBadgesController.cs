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
    public class ParticipantBadgesController : Controller
    {
        private readonly AimDbContext _context;

        public ParticipantBadgesController(AimDbContext context)
        {
            _context = context;
        }

        // GET: ParticipantBadges
        public async Task<IActionResult> Index()
        {
            var aimDbContext = _context.ParticipantBadges.Include(p => p.Badge).Include(p => p.Participant);
            return View(await aimDbContext.ToListAsync());
        }

        // GET: ParticipantBadges/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var participantBadge = await _context.ParticipantBadges
                .Include(p => p.Badge)
                .Include(p => p.Participant)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (participantBadge == null)
            {
                return NotFound();
            }

            return View(participantBadge);
        }

        // GET: ParticipantBadges/Create
        public IActionResult Create()
        {
            ViewData["BadgeId"] = new SelectList(_context.Badges, "Id", "Id");
            ViewData["ParticipantId"] = new SelectList(_context.Participants, "Id", "Name");
            return View();
        }

        // POST: ParticipantBadges/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ParticipantId,BadgeId,DateUnlocked")] ParticipantBadge participantBadge)
        {
            if (ModelState.IsValid)
            {
                _context.Add(participantBadge);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BadgeId"] = new SelectList(_context.Badges, "Id", "Id", participantBadge.BadgeId);
            ViewData["ParticipantId"] = new SelectList(_context.Participants, "Id", "Name", participantBadge.ParticipantId);
            return View(participantBadge);
        }

        // GET: ParticipantBadges/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var participantBadge = await _context.ParticipantBadges.FindAsync(id);
            if (participantBadge == null)
            {
                return NotFound();
            }
            ViewData["BadgeId"] = new SelectList(_context.Badges, "Id", "Id", participantBadge.BadgeId);
            ViewData["ParticipantId"] = new SelectList(_context.Participants, "Id", "Name", participantBadge.ParticipantId);
            return View(participantBadge);
        }

        // POST: ParticipantBadges/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ParticipantId,BadgeId,DateUnlocked")] ParticipantBadge participantBadge)
        {
            if (id != participantBadge.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(participantBadge);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ParticipantBadgeExists(participantBadge.Id))
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
            ViewData["BadgeId"] = new SelectList(_context.Badges, "Id", "Id", participantBadge.BadgeId);
            ViewData["ParticipantId"] = new SelectList(_context.Participants, "Id", "Name", participantBadge.ParticipantId);
            return View(participantBadge);
        }

        // GET: ParticipantBadges/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var participantBadge = await _context.ParticipantBadges
                .Include(p => p.Badge)
                .Include(p => p.Participant)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (participantBadge == null)
            {
                return NotFound();
            }

            return View(participantBadge);
        }

        // POST: ParticipantBadges/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var participantBadge = await _context.ParticipantBadges.FindAsync(id);
            if (participantBadge != null)
            {
                _context.ParticipantBadges.Remove(participantBadge);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ParticipantBadgeExists(int id)
        {
            return _context.ParticipantBadges.Any(e => e.Id == id);
        }
    }
}
