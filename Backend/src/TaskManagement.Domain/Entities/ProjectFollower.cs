using System;

namespace TaskManagement.Domain.Entities
{
    public class ProjectFollower
    {
        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = null!;

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public DateTime FollowedAt { get; set; } = DateTime.UtcNow;
    }
}
