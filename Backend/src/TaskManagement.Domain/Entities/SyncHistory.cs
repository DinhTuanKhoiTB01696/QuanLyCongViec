using System;

namespace TaskManagement.Domain.Entities
{
    public class SyncHistory
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public Guid? IntegrationAccountId { get; set; }
        public IntegrationAccount? IntegrationAccount { get; set; }
        public string Provider { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int ItemsImported { get; set; }
        public string? Message { get; set; }
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
    }
}
