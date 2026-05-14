namespace aim.Models
{
    public enum MoodLevel { Terrible = 1, Bad, Okay, Good, Amazing }
    public enum EnergyLevel { Drained = 1, Low, Medium, High, Peak }

    public class DailyCheckIn
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public MoodLevel Mood { get; set; }
        public EnergyLevel Energy { get; set; }
        public string? Note { get; set; }
        public int ParticipantId { get; set; }
        public Participant? Participant { get; set; }
    }
}