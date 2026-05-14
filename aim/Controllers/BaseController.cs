using aim.Data;
using aim.Extensions;
using aim.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace aim.Controllers
{
    public class BaseController : Controller
    {
        protected readonly AimDbContext _context;
        protected readonly UserManager<ApplicationUser> _userManager;

        public BaseController(AimDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context,
                                                           ActionExecutionDelegate next)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var participant = await this.GetCurrentParticipantAsync(_userManager, _context);
                if (participant != null)
                {
                    ViewBag.UnreadNotifications = await _context.Notification
                        .CountAsync(n => n.ParticipantId == participant.Id && !n.IsRead);

                    ViewBag.FreezeTokens = participant.StreakFreezeTokens; // ← add this
                }
            }
            await next();
        }
    }
}