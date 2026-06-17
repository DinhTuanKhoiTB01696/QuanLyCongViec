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
        
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
