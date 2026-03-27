using System;
using System.Threading.Tasks;
using TaskManagement.Application.DTOs.WorkTask;

namespace TaskManagement.Application.Interfaces
{
    public interface IWorkTaskService
    {
        Task UpdateTaskStatusAsync(Guid taskId, UpdateTaskStatusRequestDto request);
    }
}
