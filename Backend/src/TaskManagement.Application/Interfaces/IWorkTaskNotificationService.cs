using System;
using System.Threading.Tasks;
using TaskManagement.Application.DTOs.WorkTask;

namespace TaskManagement.Application.Interfaces
{
    public interface IWorkTaskNotificationService
    {
        Task NotifyTaskCreatedAsync(Guid projectId, WorkTaskDto task);
        Task NotifyTaskUpdatedAsync(Guid projectId, WorkTaskDto task);
        Task NotifyTaskMovedAsync(Guid projectId, WorkTaskDto task);
        Task NotifyTaskDeletedAsync(Guid projectId, Guid taskId);
    }
}
