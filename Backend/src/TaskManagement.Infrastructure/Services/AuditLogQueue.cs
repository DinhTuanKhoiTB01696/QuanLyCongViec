using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Infrastructure.Services
{
    public class AuditLogQueue : IAuditLogQueue
    {
        private readonly Channel<AuditLog> _channel;

        public AuditLogQueue()
        {
            // Set capacity if needed, Unbounded for now because we don't want DbContext to block
            _channel = Channel.CreateUnbounded<AuditLog>();
        }

        public async ValueTask EnqueueAsync(AuditLog log, CancellationToken cancellationToken = default)
        {
            await _channel.Writer.WriteAsync(log, cancellationToken);
        }

        public ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken = default)
        {
            return _channel.Reader.WaitToReadAsync(cancellationToken);
        }

        public bool TryDequeue(out AuditLog? log)
        {
            return _channel.Reader.TryRead(out log);
        }
    }
}
