using aim.Data;
using aim.Extensions;
using aim.Helpers;
using aim.Models;
using aim.Models.enums;
using aim.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace aim.Controllers
{
    [Authorize]
    public class ChallengesController : Controller
    {
        private readonly AimDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IChallengeService _challengeService;

        public ChallengesController(AimDbContext context,
                                     UserManager<ApplicationUser> userManager, IChallengeService challengeService)
        {
            _context = context;
            _userManager = userManager;
            _challengeService = challengeService;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var query = _context.Challenges
                .OrderByDescending(c => c.StartDate)
                .AsQueryable();

            var paged = await PaginatedList<Challenge>.CreateAsync(query, page, 10);
            return View(paged);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var challenge = await _challengeService.GetWithTasksAsync(id.Value);
            if (challenge == null) return NotFound();

            var participant = await this.GetCurrentParticipantAsync(_userManager, _context);
            if (participant == null) return RedirectToAction("Login", "Auth");

            bool hasJoined = await _context.Participation.AnyAsync(p => p.ParticipantId == participant.Id &&
                   p.ChallengeId == id);
            ViewBag.HasJoined = hasJoined;

            var completedTodayIds = await _challengeService.GetCompletedTodayAsync(participant.Id);

            var comments = await _context.ChallengeComments
                .Include(c => c.Participant)
                .Where(c => c.ChallengeId == id)
                .OrderByDescending(c => c.CreatedAt)
                .Take(30)
                .ToListAsync();

            ViewBag.Comments = comments;
            ViewBag.CurrentUserId = participant.Id;
            ViewBag.CompletedTodayIds = completedTodayIds;
            ViewBag.ParticipantScore = participant.TotalScore;
            ViewBag.ParticipantStreak = participant.CurrentStreak;
            ViewBag.IsExpired = challenge.EndDate != default && challenge.EndDate < DateTime.UtcNow;

            return View(challenge);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,PointsReward,StartDate,EndDate")] Challenge challenge)
        {
            if (ModelState.IsValid)
            {
                _context.Add(challenge);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(challenge);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var challenge = await _context.Challenges.FindAsync(id);
            if (challenge == null) return NotFound();
            return View(challenge);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,PointsReward,StartDate,EndDate")] Challenge challenge)
        {
            if (id != challenge.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(challenge);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChallengeExists(challenge.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(challenge);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var challenge = await _context.Challenges.FirstOrDefaultAsync(m => m.Id == id);
            if (challenge == null) return NotFound();
            return View(challenge);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var challenge = await _context.Challenges.FindAsync(id);
            if (challenge != null) _context.Challenges.Remove(challenge);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Challenges/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateChallenge(string title, string description,
                                                           DateTime endDate, List<string> taskTitles,
                                                           List<string> taskDescriptions,
                                                           List<int> taskDifficulties)
        {
            var participant = await this.GetCurrentParticipantAsync(_userManager, _context);
            if (participant == null) return RedirectToAction("Login", "Auth");

            if (string.IsNullOrWhiteSpace(title))
            {
                ViewBag.Error = "Challenge title is required.";
                return View();
            }

            var challenge = new Challenge
            {
                Title = title,
                Description = description,
                PointsReward = 0,
                StartDate = DateTime.UtcNow,
                EndDate = endDate,
                CreatedByParticipantId = participant.Id
            };

            _context.Challenges.Add(challenge);
            await _context.SaveChangesAsync();

            // Add tasks
            for (int i = 0; i < taskTitles.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(taskTitles[i])) continue;

                var difficulty = (DifficultyLevel)(taskDifficulties.ElementAtOrDefault(i));
                int points = difficulty switch
                {
                    DifficultyLevel.Easy => 10,
                    DifficultyLevel.Medium => 20,
                    DifficultyLevel.Hard => 30,
                    _ => 10
                };

                _context.ChallengeTasks.Add(new ChallengeTask
                {
                    Title = taskTitles[i],
                    Description = taskDescriptions.ElementAtOrDefault(i) ?? "",
                    Difficulty = difficulty,
                    PointsValue = points,
                    ChallengeId = challenge.Id
                });
            }

            // Auto-join the creator
            _context.Participation.Add(new Participation
            {
                ParticipantId = participant.Id,
                ChallengeId = challenge.Id,
                JoinedAt = DateTime.UtcNow,
                CurrentProgress = 0,
                IsCompleted = false
            });

            await _context.SaveChangesAsync();

            TempData["Toast"] = $"Challenge '{title}' created!";
            TempData["ToastType"] = "success";
            return RedirectToAction("Details", new { id = challenge.Id });
        }

        // POST: Challenges/Join
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> JoinChallenge(int challengeId)
        {
            var participant = await this.GetCurrentParticipantAsync(_userManager, _context);
            if (participant == null) return RedirectToAction("Login", "Auth");

            bool alreadyJoined = await _context.Participation
                .AnyAsync(p => p.ParticipantId == participant.Id &&
                               p.ChallengeId == challengeId);

            if (!alreadyJoined)
            {
                _context.Participation.Add(new Participation
                {
                    ParticipantId = participant.Id,
                    ChallengeId = challengeId,
                    JoinedAt = DateTime.UtcNow,
                    CurrentProgress = 0,
                    IsCompleted = false
                });
                await _context.SaveChangesAsync();

                TempData["Toast"] = "Challenge joined!";
                TempData["ToastType"] = "success";
            }
            else
            {
                TempData["Toast"] = "Already joined.";
                TempData["ToastType"] = "warning";
            }

            return RedirectToAction("Details", new { id = challengeId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Comment(int challengeId, string content)
        {
            var participant = await this.GetCurrentParticipantAsync(_userManager, _context);
            if (participant == null) return RedirectToAction("Login", "Auth");

            if (string.IsNullOrWhiteSpace(content) || content.Length > 300)
            {
                TempData["Toast"] = "Comment must be 1-300 characters.";
                TempData["ToastType"] = "warning";
                return RedirectToAction("Details", new { id = challengeId });
            }

            _context.ChallengeComments.Add(new ChallengeComment
            {
                Content = content.Trim(),
                CreatedAt = DateTime.UtcNow,
                ChallengeId = challengeId,
                ParticipantId = participant.Id
            });

            await _context.SaveChangesAsync();

            TempData["Toast"] = "Comment posted!";
            TempData["ToastType"] = "success";
            return RedirectToAction("Details", new { id = challengeId });
        }
        private bool ChallengeExists(int id) => _context.Challenges.Any(e => e.Id == id);
    }
}