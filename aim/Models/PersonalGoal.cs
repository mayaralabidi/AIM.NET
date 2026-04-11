namespace aim.Models
{
    public class PersonalGoal
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public double TargetValue { get; set; }
        public double CurrentValue { get; set; }
        public DateTime TargetDate { get; set; }
        public bool IsReached { get; set; }

        public int ParticipantId { get; set; }
        public virtual Participant Participant { get; set; } = null!;
    }
}
