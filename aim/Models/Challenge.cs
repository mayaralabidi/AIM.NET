using System.ComponentModel.DataAnnotations;

namespace aim.Models
{
    public class Challenge
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int PointsReward { get; set; }

        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        public int? CreatedByParticipantId { get; set; }
        public Participant? CreatedBy { get; set; }

        public virtual ICollection<Participation> Participations { get; set; } = new List<Participation>();
        public virtual ICollection<ChallengeTask> Tasks { get; set; } = new List<ChallengeTask>();
    }
}
