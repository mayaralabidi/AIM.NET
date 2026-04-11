using aim.Models.enums;
using System.ComponentModel.DataAnnotations;

namespace aim.Models
{
    public class ChallengeTask
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }
        public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Easy;

        // Points de base + Bonus de difficulté
        public int PointsValue { get; set; }

        public int ChallengeId { get; set; }
        public virtual Challenge Challenge { get; set; } = null!;

        public virtual ICollection<TaskCompletion> Completions { get; set; } = new List<TaskCompletion>();

        // Calculer les points selon la difficulté
        public int GetCalculatedPoints()
        {
            return PointsValue * (int)Difficulty;
        }
    }
}
