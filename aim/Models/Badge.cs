namespace aim.Models
{
    public class Badge
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string IconPath { get; set; } = "default_badge.png";
        public string Criteria { get; set; } = string.Empty; // Ex: "7 jours de streak"
    }
}
