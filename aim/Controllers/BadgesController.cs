using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using aim.Data;
using aim.Models;

namespace aim.Controllers
{
    [Authorize]
    public class BadgesController : Controller
    {
        private readonly AimDbContext _context;

        public BadgesController(AimDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Badges.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var badge = await _context.Badges.FirstOrDefaultAsync(m => m.Id == id);
            if (badge == null) return NotFound();

            return View(badge);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Key,Name,IconPath,Criteria")] Badge badge)
        {
            if (ModelState.IsValid)
            {
                _context.Add(badge);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(badge);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var badge = await _context.Badges.FindAsync(id);
            if (badge == null) return NotFound();
            return View(badge);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Key,Name,IconPath,Criteria")] Badge badge)
        {
            if (id != badge.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(badge);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BadgeExists(badge.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(badge);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var badge = await _context.Badges.FirstOrDefaultAsync(m => m.Id == id);
            if (badge == null) return NotFound();
            return View(badge);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var badge = await _context.Badges.FindAsync(id);
            if (badge != null) _context.Badges.Remove(badge);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BadgeExists(int id) => _context.Badges.Any(e => e.Id == id);
    }
}