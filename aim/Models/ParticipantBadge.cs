namespace aim.Models
{
    public class ParticipantBadge
    {
        public int Id { get; set; }
        public string BadgeKey { get; set; } = "";  
        public DateTime EarnedAt { get; set; }
        public int ParticipantId { get; set; }
        public virtual Participant Participant { get; set; } = null!;

        public int BadgeId { get; set; }
        public virtual Badge Badge { get; set; } = null!;

    }
}
