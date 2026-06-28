using System;

namespace TaskManagement.Domain.Entities
{
    public class TeamGoal
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid DepartmentId { get; set; }
        public Department Department { get; set; } = null!;
        public Guid GoalId { get; set; }
        public Goal Goal { get; set; } = null!;
        public Guid CreatedByUserId { get; set; }
        public User CreatedByUser { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
