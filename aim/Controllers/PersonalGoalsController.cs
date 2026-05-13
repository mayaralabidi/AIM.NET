using aim.Data;
using aim.Extensions;
using aim.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace aim.Controllers
{
    [Authorize]
    public class PersonalGoalsController : BaseController
    {
        public PersonalGoalsController(AimDbContext context,
                                        UserManager<ApplicationUser> userManager)
            : base(context, userManager) { }

        public async Task<IActionResult> Index()
        {
            var participant = await this.GetCurrentParticipantAsync(_userManager, _context);
            if (participant == null) return RedirectToAction("Login", "Auth");

            var goals = await _context.PersonalGoal
                .Where(g => g.ParticipantId == participant.Id)
                .OrderBy(g => g.IsReached)
                .ThenBy(g => g.TargetDate)
                .ToListAsync();

            return View(goals);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string description, int targetValue, DateTime targetDate)
        {
            var participant = await this.GetCurrentParticipantAsync(_userManager, _context);
            if (participant == null) return RedirectToAction("Login", "Auth");

            if (string.IsNullOrWhiteSpace(description) || targetValue <= 0)
            {
                ViewBag.Error = "Please fill in all fields.";
                return View();
            }

            _context.PersonalGoal.Add(new PersonalGoal
            {
                Description = description,
                TargetValue = targetValue,
                CurrentValue = 0,
                TargetDate = targetDate,
                IsReached = false,
                ParticipantId = participant.Id
            });

            await _context.SaveChangesAsync();
            TempData["Toast"] = "Goal created!";
            TempData["ToastType"] = "success";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProgress(int id, int currentValue)
        {
            var participant = await this.GetCurrentParticipantAsync(_userManager, _context);
            if (participant == null) return RedirectToAction("Login", "Auth");

            var goal = await _context.PersonalGoal
                .FirstOrDefaultAsync(g => g.Id == id && g.ParticipantId == participant.Id);

            if (goal == null) return NotFound();

            goal.CurrentValue = Math.Min(currentValue, goal.TargetValue);
            goal.IsReached = goal.CurrentValue >= goal.TargetValue;

            await _context.SaveChangesAsync();

            TempData["Toast"] = goal.IsReached ? "🎯 Goal reached!" : "Progress updated!";
            TempData["ToastType"] = goal.IsReached ? "badge" : "success";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var participant = await this.GetCurrentParticipantAsync(_userManager, _context);
            if (participant == null) return RedirectToAction("Login", "Auth");

            var goal = await _context.PersonalGoal
                .FirstOrDefaultAsync(g => g.Id == id && g.ParticipantId == participant.Id);

            if (goal != null) _context.PersonalGoal.Remove(goal);
            await _context.SaveChangesAsync();

            TempData["Toast"] = "Goal deleted.";
            TempData["ToastType"] = "warning";
            return RedirectToAction(nameof(Index));
        }
    }
}