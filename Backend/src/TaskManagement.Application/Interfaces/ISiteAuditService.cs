using System;
using System.Threading.Tasks;

namespace TaskManagement.Application.Interfaces
{
    public interface ISiteAuditService
    {
        Task LogActionAsync(Guid entityId, string entityType, string action, Guid userId, string? oldValue = null, string? newValue = null);
    }
}
