namespace aim.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.Now;
        public bool IsRead { get; set; } = false;

        public int ParticipantId { get; set; }
        public virtual Participant Participant { get; set; } = null!;
    }
}
