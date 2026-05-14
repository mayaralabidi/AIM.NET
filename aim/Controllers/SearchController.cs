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
    public class SearchController : BaseController
    {
        public SearchController(AimDbContext context,
                                 UserManager<ApplicationUser> userManager)
            : base(context, userManager) { }

        public async Task<IActionResult> Index(string? q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return View(new SearchResults());

            var term = q.Trim().ToLower();

            var challenges = await _context.Challenges
                .Where(c => c.Title.ToLower().Contains(term) ||
                            c.Description.ToLower().Contains(term))
                .Take(5).ToListAsync();

            var users = await _context.Participants
                .Where(p => p.Name.ToLower().Contains(term))
                .Take(5).ToListAsync();

            var badges = await _context.Badges
                .Where(b => b.Name.ToLower().Contains(term) ||
                            b.Criteria.ToLower().Contains(term))
                .Take(5).ToListAsync();

            var results = new SearchResults
            {
                Query = q,
                Challenges = challenges,
                Users = users,
                Badges = badges
            };

            return View(results);
        }
    }

    public class SearchResults
    {
        public string? Query { get; set; }
        public List<Challenge> Challenges { get; set; } = new();
        public List<Participant> Users { get; set; } = new();
        public List<Badge> Badges { get; set; } = new();
        public int Total => Challenges.Count + Users.Count + Badges.Count;
    }
}