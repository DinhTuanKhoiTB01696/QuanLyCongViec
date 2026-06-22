using System;

namespace TaskManagement.Domain.Entities
{
    public class GoalUpdateReaction
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public Guid GoalUpdateId { get; set; }
        public GoalUpdate GoalUpdate { get; set; } = null!;
        
        public string ReactionType { get; set; } = string.Empty; // "like", "clap", "tada", "heart"
        
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
