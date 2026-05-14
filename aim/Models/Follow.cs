namespace aim.Models
{
    public class Follow
    {
        public int Id { get; set; }
        public int FollowerId { get; set; }
        public int FollowingId { get; set; }
        public DateTime CreatedAt { get; set; }
        public Participant? Follower { get; set; }
        public Participant? Following { get; set; }
    }
}