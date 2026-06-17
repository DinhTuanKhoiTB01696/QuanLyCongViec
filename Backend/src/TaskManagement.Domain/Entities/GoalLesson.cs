using System;

namespace TaskManagement.Domain.Entities
{
    public class GoalLesson
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid GoalId { get; set; }
        public Goal Goal { get; set; } = null!;
        
        public string Text { get; set; } = string.Empty;
        
        public Guid CreatorId { get; set; }
        public User Creator { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
