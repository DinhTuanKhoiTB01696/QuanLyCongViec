using System;

namespace TaskManagement.Domain.Entities
{
    public class KudoReaction
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid KudoId { get; set; }
        public Kudo Kudo { get; set; } = null!;

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public string ReactionType { get; set; } = string.Empty; // e.g. "Like", "Heart", "Clap"

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
