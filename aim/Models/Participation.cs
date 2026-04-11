namespace aim.Models
{
    public class Participation
    {
        public int Id { get; set; }

        public int ParticipantId { get; set; }
        public virtual Participant Participant { get; set; } = null!;

        public int ChallengeId { get; set; }
        public virtual Challenge Challenge { get; set; } = null!;

        public DateTime JoinedAt { get; set; } = DateTime.Now;

        public int CurrentProgress { get; set; } // Score ou étape actuelle
        public bool IsCompleted { get; set; } = false;
    }
}
