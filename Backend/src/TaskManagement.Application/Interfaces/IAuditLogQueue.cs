using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Interfaces
{
    public interface IAuditLogQueue
    {
        ValueTask EnqueueAsync(AuditLog log, CancellationToken cancellationToken = default);
        ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken = default);
        bool TryDequeue(out AuditLog? log);
    }
}
