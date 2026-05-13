using Microsoft.AspNetCore.Identity;

namespace aim.Models
{
    public class ApplicationUser : IdentityUser
    {
        public int? ParticipantId { get; set; }
        public Participant? Participant { get; set; }
    }
}