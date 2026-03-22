using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using TaskManagement.API.Hubs;
using TaskManagement.Application.DTOs.WorkTask;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.API.Services
{
    public class WorkTaskNotificationService : IWorkTaskNotificationService
    {
        private readonly IHubContext<KanbanHub> _hubContext;

        public WorkTaskNotificationService(IHubContext<KanbanHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyTaskCreatedAsync(Guid projectId, WorkTaskDto task)
        {
            await _hubContext.Clients.Group(projectId.ToString()).SendAsync("TaskCreated", task);
        }

        public async Task NotifyTaskUpdatedAsync(Guid projectId, WorkTaskDto task)
        {
            await _hubContext.Clients.Group(projectId.ToString()).SendAsync("TaskUpdated", task);
        }

        public async Task NotifyTaskMovedAsync(Guid projectId, WorkTaskDto task)
        {
            await _hubContext.Clients.Group(projectId.ToString()).SendAsync("TaskMoved", task);
        }

        public async Task NotifyTaskDeletedAsync(Guid projectId, Guid taskId)
        {
            await _hubContext.Clients.Group(projectId.ToString()).SendAsync("TaskDeleted", taskId);
        }
    }
}
