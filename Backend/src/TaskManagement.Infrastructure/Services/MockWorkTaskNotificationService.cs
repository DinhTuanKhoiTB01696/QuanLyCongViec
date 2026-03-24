using System;
using System.Threading.Tasks;
using TaskManagement.Application.DTOs.WorkTask;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.Infrastructure.Services
{
    public class MockWorkTaskNotificationService : IWorkTaskNotificationService
    {
        public Task NotifyTaskCreatedAsync(Guid projectId, WorkTaskDto task) => Task.CompletedTask;
        public Task NotifyTaskDeletedAsync(Guid projectId, Guid taskId) => Task.CompletedTask;
        public Task NotifyTaskMovedAsync(Guid projectId, WorkTaskDto task) => Task.CompletedTask;
        public Task NotifyTaskUpdatedAsync(Guid projectId, WorkTaskDto task) => Task.CompletedTask;
    }
}
