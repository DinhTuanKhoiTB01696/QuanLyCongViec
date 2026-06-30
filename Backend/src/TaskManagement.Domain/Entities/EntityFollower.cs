using System;

namespace TaskManagement.Domain.Entities
{
    public class EntityFollower
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid EntityId { get; set; }
        public string EntityType { get; set; } = string.Empty; // e.g., "Project", "Goal"
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
