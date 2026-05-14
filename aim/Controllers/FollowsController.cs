using aim.Data;
using aim.Extensions;
using aim.Models;
using aim.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace aim.Controllers
{
    [Authorize]
    public class FollowsController : BaseController
    {
        private readonly INotificationService _notificationService;

        public FollowsController(AimDbContext context,
                                  UserManager<ApplicationUser> userManager,
                                  INotificationService notificationService)
            : base(context, userManager)
        {
            _notificationService = notificationService;
        }

        // GET: Follows/Feed — activity feed of people you follow
        public async Task<IActionResult> Feed()
        {
            var participant = await this.GetCurrentParticipantAsync(_userManager, _context);
            if (participant == null) return RedirectToAction("Login", "Auth");

            var followingIds = await _context.Follows
                .Where(f => f.FollowerId == participant.Id)
                .Select(f => f.FollowingId)
                .ToListAsync();

            var recentCompletions = await _context.TaskCompletions
                .Include(tc => tc.Participant)
                .Include(tc => tc.ChallengeTask)
                    .ThenInclude(ct => ct.Challenge)
                .Where(tc => followingIds.Contains(tc.ParticipantId) &&
                             tc.CompletedAt >= DateTime.UtcNow.AddDays(-7))
                .OrderByDescending(tc => tc.CompletedAt)
                .Take(50)
                .ToListAsync();

            var following = await _context.Participants
                .Where(p => followingIds.Contains(p.Id))
                .OrderByDescending(p => p.TotalScore)
                .ToListAsync();

            ViewBag.Following = following;
            ViewBag.CurrentId = participant.Id;
            ViewBag.FollowingIds = followingIds;

            return View(recentCompletions);
        }

        // GET: Follows/Explore — find users to follow
        public async Task<IActionResult> Explore(string? search)
        {
            var participant = await this.GetCurrentParticipantAsync(_userManager, _context);
            if (participant == null) return RedirectToAction("Login", "Auth");

            var followingIds = await _context.Follows
                .Where(f => f.FollowerId == participant.Id)
                .Select(f => f.FollowingId)
                .ToListAsync();

            var query = _context.Participants
                .Where(p => p.Id != participant.Id);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.Name.Contains(search));

            var users = await query
                .OrderByDescending(p => p.TotalScore)
                .Take(30)
                .ToListAsync();

            ViewBag.FollowingIds = followingIds;
            ViewBag.Search = search;
            ViewBag.CurrentId = participant.Id;

            return View(users);
        }

        // POST: Follows/Toggle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int targetId, string? returnUrl)
        {
            var participant = await this.GetCurrentParticipantAsync(_userManager, _context);
            if (participant == null) return RedirectToAction("Login", "Auth");

            if (participant.Id == targetId)
                return BadRequest();

            var existing = await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == participant.Id &&
                                          f.FollowingId == targetId);

            if (existing != null)
            {
                _context.Follows.Remove(existing);
                TempData["Toast"] = "Unfollowed.";
                TempData["ToastType"] = "warning";
            }
            else
            {
                _context.Follows.Add(new Follow
                {
                    FollowerId = participant.Id,
                    FollowingId = targetId,
                    CreatedAt = DateTime.UtcNow
                });

                // Notify the followed user
                await _notificationService.NotifyAsync(targetId,
                    $"👤 {participant.Name} started following you!");

                TempData["Toast"] = "Following!";
                TempData["ToastType"] = "success";
            }

            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Explore");
        }
    }
}