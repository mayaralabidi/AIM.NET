namespace aim.Models
{
    public class Team
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // Seul celui qui a le code rejoint
        public string InviteCode { get; set; } = Guid.NewGuid().ToString().Substring(0, 8);

        public virtual ICollection<Participant> Members { get; set; } = new List<Participant>();

        // Les challenges auxquels l'équipe participe
        public virtual ICollection<TeamChallenge> TeamChallenges { get; set; } = new List<TeamChallenge>();
    }
}
