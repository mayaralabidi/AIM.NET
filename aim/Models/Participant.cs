using System.ComponentModel.DataAnnotations;

namespace aim.Models
{
    public class Participant
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le nom est obligatoire")]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [EmailAddress]
        public string? Email { get; set; }

        public int TotalScore { get; set; } = 0;

        public int CurrentStreak { get; set; } = 0;

        public DateTime LastActivityDate { get; set; } = DateTime.Now;

        public int? TeamId { get; set; }
        public virtual Team? Team { get; set; }

        public virtual ICollection<Participation> Participations { get; set; } = new List<Participation>();
        public virtual ICollection<ParticipantBadge> ParticipantBadges { get; set; } = new List<ParticipantBadge>();

        public virtual ICollection<TaskCompletion> TaskCompletions { get; set; } = new List<TaskCompletion>();
    }
}
