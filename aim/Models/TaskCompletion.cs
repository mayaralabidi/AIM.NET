namespace aim.Models
{
    public class TaskCompletion
    {
        public int Id { get; set; }

        public DateTime CompletedAt { get; set; } = DateTime.Now;

        public int ParticipantId { get; set; }
        public virtual Participant Participant { get; set; } = null!;

        public int ChallengeTaskId { get; set; }
        public virtual ChallengeTask ChallengeTask { get; set; } = null!;
    }
}
