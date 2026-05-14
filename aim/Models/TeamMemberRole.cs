namespace aim.Models
{
    public enum TeamRole { Member, Captain, Admin }

    public class TeamMembership
    {
        public int Id { get; set; }
        public int ParticipantId { get; set; }
        public int TeamId { get; set; }
        public TeamRole Role { get; set; } = TeamRole.Member;
        public DateTime JoinedAt { get; set; }
        public Participant? Participant { get; set; }
        public Team? Team { get; set; }
    }
}