using aim.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace aim.Data
{
    public class AimDbContext : IdentityDbContext<ApplicationUser>
    {
        public AimDbContext(DbContextOptions<AimDbContext> options)
            : base(options)
        {
        }
        public DbSet<Participant> Participants { get; set; }
        public DbSet<Challenge> Challenges { get; set; }
        public DbSet<ChallengeTask> ChallengeTasks { get; set; }
        public DbSet<TaskCompletion> TaskCompletions { get; set; }
        public DbSet<ParticipantBadge> ParticipantBadges { get; set; }
        public DbSet<Badge> Badges { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<TeamChallenge> TeamChallenges { get; set; }
        public DbSet<Follow> Follows { get; set; }
        public DbSet<ChallengeComment> ChallengeComments { get; set; }
        public DbSet<DailyCheckIn> DailyCheckIns { get; set; }
        public DbSet<TeamMembership> TeamMemberships { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Participant)
                .WithOne()
                .HasForeignKey<ApplicationUser>(u => u.ParticipantId)
                .OnDelete(DeleteBehavior.SetNull);

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

            // 7. Valeurs pour les badges 
            modelBuilder.Entity<Badge>().HasData(
                // Streak badges
                new Badge { Id = 1, Key = "streak_3", Name = "First Steps", IconPath = "3days.png", Criteria = "3 day streak" },
                new Badge { Id = 2, Key = "streak_7", Name = "Week Warrior", IconPath = "7days.png", Criteria = "7 day streak" },
                new Badge { Id = 3, Key = "streak_14", Name = "Fortnight Fighter", IconPath = "14days.png", Criteria = "14 day streak" },
                new Badge { Id = 4, Key = "streak_30", Name = "Iron Will", IconPath = "30days.png", Criteria = "30 day streak" },
                new Badge { Id = 5, Key = "streak_100", Name = "Legendary", IconPath = "100days.png", Criteria = "100 day streak" },

                // Task count badges
                new Badge { Id = 6, Key = "tasks_1", Name = "First Blood", IconPath = "1stblood.png", Criteria = "Complete first task" },
                new Badge { Id = 7, Key = "tasks_10", Name = "Warming Up", IconPath = "10tasks.png", Criteria = "Complete 10 tasks" },
                new Badge { Id = 8, Key = "tasks_50", Name = "Half Century", IconPath = "50tasks.png", Criteria = "Complete 50 tasks" },
                new Badge { Id = 9, Key = "tasks_100", Name = "Centurion", IconPath = "100tasks.png", Criteria = "Complete 100 tasks" },
                new Badge { Id = 10, Key = "tasks_250", Name = "Machine", IconPath = "250tasks.png", Criteria = "Complete 250 tasks" },
                new Badge { Id = 11, Key = "tasks_500", Name = "Relentless", IconPath = "500tasks.png", Criteria = "Complete 500 tasks" },

                // Score badges
                new Badge { Id = 12, Key = "score_500", Name = "Rising Star", IconPath = "500score.png", Criteria = "Reach 500 points" },
                new Badge { Id = 13, Key = "score_1000", Name = "High Roller", IconPath = "1000score.png", Criteria = "Reach 1000 points" },
                new Badge { Id = 14, Key = "score_5000", Name = "Elite", IconPath = "5000score.png", Criteria = "Reach 5000 points" },
                new Badge { Id = 15, Key = "score_10000", Name = "Untouchable", IconPath = "10000score.png", Criteria = "Reach 10000 points" },

                // Social badges
                new Badge { Id = 16, Key = "team_join", Name = "Squad Up", IconPath = "joinateam.png", Criteria = "Join a team" },
                new Badge { Id = 17, Key = "team_top3", Name = "Podium", IconPath = "podium.png", Criteria = "Reach top 3 in your team" },
                new Badge { Id = 18, Key = "team_top1", Name = "Alpha", IconPath = "alpha.png", Criteria = "Reach #1 in your team" },

                // Special badges
                new Badge { Id = 19, Key = "comeback", Name = "Comeback Kid", IconPath = "comebackkid.png", Criteria = "Restart a streak after losing one" },
                new Badge { Id = 20, Key = "early_bird", Name = "Early Bird", IconPath = "earlybird.png", Criteria = "Complete a task before 8am" },
                new Badge { Id = 21, Key = "night_owl", Name = "Night Owl", IconPath = "nightowl.png", Criteria = "Complete a task after 10pm" },
                new Badge { Id = 22, Key = "overachiever", Name = "Overachiever", IconPath = "overachiever.png", Criteria = "Complete every task in a challenge" }
            );

            modelBuilder.Entity<Follow>()
                .HasOne(f => f.Follower)
                .WithMany()
                .HasForeignKey(f => f.FollowerId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Follow>()
                .HasOne(f => f.Following)
                .WithMany()
                .HasForeignKey(f => f.FollowingId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Follow>()
                .HasIndex(f => new { f.FollowerId, f.FollowingId })
                .IsUnique();
        }
        public DbSet<aim.Models.Notification> Notification { get; set; } = default!;
        public DbSet<aim.Models.PersonalGoal> PersonalGoal { get; set; } = default!;
        public DbSet<aim.Models.Participation> Participation { get; set; } = default!;
    }
}
