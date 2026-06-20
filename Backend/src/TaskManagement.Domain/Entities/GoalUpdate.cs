using System;

namespace TaskManagement.Domain.Entities
{
    public class GoalUpdate
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid GoalId { get; set; }
        public Goal Goal { get; set; } = null!;
        
        public string Content { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // Status at the time of update
        public string? PreviousStatus { get; set; }
        public string? NewStatus { get; set; }
        public int? PreviousProgress { get; set; }
        public int? NewProgress { get; set; }
        public DateTime? TargetDate { get; set; }
        
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public int ViewCount { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<GoalUpdateReaction> Reactions { get; set; } = new List<GoalUpdateReaction>();
    }
}
