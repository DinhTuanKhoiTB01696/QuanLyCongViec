using System;

namespace TaskManagement.Domain.Entities
{
    public class GoalFollower
    {
        public Guid GoalId { get; set; }
        public Goal Goal { get; set; } = null!;

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public DateTime FollowedAt { get; set; } = DateTime.UtcNow;
    }
}
