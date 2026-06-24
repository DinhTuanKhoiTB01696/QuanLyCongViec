using System;
using System.Threading.Tasks;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure.Services
{
    public class SiteAuditService : ISiteAuditService
    {
        private readonly ApplicationDbContext _context;

        public SiteAuditService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task LogActionAsync(Guid entityId, string entityType, string action, Guid userId, string? oldValue = null, string? newValue = null)
        {
            var log = new SiteAuditLog
            {
                EntityId = entityId,
                EntityType = entityType,
                Action = action,
                UserId = userId,
                OldValue = oldValue,
                NewValue = newValue,
                CreatedAt = DateTime.UtcNow
            };

            _context.SiteAuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
