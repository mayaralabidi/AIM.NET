using aim.Data;
using aim.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace aim.Extensions
{
    public static class ControllerExtensions
    {
        public static async Task<Participant?> GetCurrentParticipantAsync(
            this Controller controller,
            UserManager<ApplicationUser> userManager,
            AimDbContext context)
        {
            var user = await userManager.GetUserAsync(controller.User);
            if (user?.ParticipantId == null) return null;

            return await context.Participants
                .FirstOrDefaultAsync(p => p.Id == user.ParticipantId);
        }
    }
}