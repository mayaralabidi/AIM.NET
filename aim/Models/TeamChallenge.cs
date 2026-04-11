namespace aim.Models
{
    public class TeamChallenge
    {
        public int Id { get; set; }

        public int TeamId { get; set; }
        public virtual Team Team { get; set; } = null!;

        public int ChallengeId { get; set; }
        public virtual Challenge Challenge { get; set; } = null!;

        public DateTime RegisteredAt { get; set; } = DateTime.Now;
    }
}
