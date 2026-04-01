using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure.Services
{
    public class AuditLogWorker : BackgroundService
    {
        private readonly IAuditLogQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AuditLogWorker> _logger;
        private const int MaxBatchSize = 50;

        public AuditLogWorker(IAuditLogQueue queue, IServiceScopeFactory scopeFactory, ILogger<AuditLogWorker> logger)
        {
            _queue = queue;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AuditLogWorker is starting.");

            var batch = new List<AuditLog>(MaxBatchSize);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Block until at least one item is available
                    var itemAvailable = await _queue.WaitToReadAsync(stoppingToken);
                    if (!itemAvailable) break; // Channel closed

                    // Read up to MaxBatchSize items currently in queue
                    while (batch.Count < MaxBatchSize && _queue.TryDequeue(out var log))
                    {
                        if (log != null)
                        {
                            batch.Add(log);
                        }
                    }

                    if (batch.Count > 0)
                    {
                        await ProcessBatchAsync(batch, stoppingToken);
                        batch.Clear();
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing audit logs in background worker.");
                }
            }
        }

        private async Task ProcessBatchAsync(List<AuditLog> logs, CancellationToken cancellationToken)
        {
            if (logs.Count == 0) return;

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                await dbContext.AuditLogs.AddRangeAsync(logs, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save AuditLogs batch.");
            }
        }
    }
}
