using System;
using System.Collections.Generic;

namespace TaskManagement.Domain.Entities
{
    public class IntegrationAccount
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public string Provider { get; set; } = string.Empty;
        public string AccountEmail { get; set; } = string.Empty;
        public string? ExternalAccountId { get; set; }
        public string AccessToken { get; set; } = string.Empty;
        public string? RefreshToken { get; set; }
        public DateTime? AccessTokenExpiresAt { get; set; }
        public string Scopes { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime? LastSyncedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<InboxItem> InboxItems { get; set; } = new List<InboxItem>();
        public ICollection<SyncHistory> SyncHistories { get; set; } = new List<SyncHistory>();
    }
}
