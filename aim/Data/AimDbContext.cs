using aim.Models;
using Microsoft.EntityFrameworkCore;

namespace aim.Data
{
    public class AimDbContext : DbContext
    {
        public AimDbContext(DbContextOptions<AimDbContext> options)
            : base(options)
        {
        }

        // Tables principales
        public DbSet<Participant> Participants { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Challenge> Challenges { get; set; }
        public DbSet<ChallengeTask> ChallengeTasks { get; set; }

        // Tables de liaison et logs
        public DbSet<TeamChallenge> TeamChallenges { get; set; }
        public DbSet<TaskCompletion> TaskCompletions { get; set; }
        public DbSet<Badge> Badges { get; set; }
        public DbSet<ParticipantBadge> ParticipantBadges { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Configuration de la Team (Privée avec code d'invitation)
            modelBuilder.Entity<Team>()
                .HasIndex(t => t.InviteCode)
                .IsUnique();

            // 2. Relation Participant -> Team (Un participant a une seule équipe)
            modelBuilder.Entity<Participant>()
                .HasOne(p => p.Team)
                .WithMany(t => t.Members)
                .HasForeignKey(p => p.TeamId)
                .OnDelete(DeleteBehavior.SetNull); // Si la team est supprimée, le participant reste

            // 3. Relation Team <-> Challenge (Many-to-Many via TeamChallenge)
            modelBuilder.Entity<TeamChallenge>()
                .HasKey(tc => new { tc.TeamId, tc.ChallengeId }); 

            modelBuilder.Entity<TeamChallenge>()
                .HasOne(tc => tc.Team)
                .WithMany(t => t.TeamChallenges)
                .HasForeignKey(tc => tc.TeamId);

            modelBuilder.Entity<TeamChallenge>()
                .HasOne(tc => tc.Challenge)
                .WithMany()
                .HasForeignKey(tc => tc.ChallengeId);

            // 4. Relation Challenge -> Tasks (One-to-Many)
            modelBuilder.Entity<ChallengeTask>()
                .HasOne(t => t.Challenge)
                .WithMany(c => c.Tasks)
                .HasForeignKey(t => t.ChallengeId)
                .OnDelete(DeleteBehavior.Cascade); // Si le challenge meurt, les tâches aussi

            // 5. Indexation pour la performance du Leaderboard interne
            // Accélère la recherche des complétions par participant et par date
            modelBuilder.Entity<TaskCompletion>()
                .HasIndex(tc => new { tc.ParticipantId, tc.CompletedAt });

            // 6. Valeurs par défaut pour la Discipline
            modelBuilder.Entity<Participant>()
                .Property(p => p.CurrentStreak)
                .HasDefaultValue(0);

            modelBuilder.Entity<Participant>()
                .Property(p => p.TotalScore)
                .HasDefaultValue(0);
        }
        public DbSet<aim.Models.Notification> Notification { get; set; } = default!;
        public DbSet<aim.Models.PersonalGoal> PersonalGoal { get; set; } = default!;
        public DbSet<aim.Models.Participation> Participation { get; set; } = default!;
    }
}
