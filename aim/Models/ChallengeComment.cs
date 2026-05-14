namespace aim.Models
{
    public class ChallengeComment
    {
        public int Id { get; set; }
        public string Content { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public int ChallengeId { get; set; }
        public int ParticipantId { get; set; }
        public Challenge? Challenge { get; set; }
        public Participant? Participant { get; set; }
    }
}