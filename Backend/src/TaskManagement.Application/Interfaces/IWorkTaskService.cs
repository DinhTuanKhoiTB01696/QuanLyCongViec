using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskManagement.Application.DTOs.WorkTask;

namespace TaskManagement.Application.Interfaces
{
    public interface IWorkTaskService
    {
        Task<IEnumerable<WorkTaskDto>> GetTasksByProjectAsync(Guid projectId);
        Task<WorkTaskDto?> GetTaskByIdAsync(Guid taskId);
        Task<WorkTaskDto> CreateTaskAsync(Guid userId, CreateWorkTaskDto dto);
        Task<WorkTaskDto?> UpdateTaskAsync(Guid userId, Guid taskId, UpdateWorkTaskDto dto);
        Task<WorkTaskDto?> MoveTaskAsync(Guid userId, Guid taskId, MoveTaskDto dto);
        Task<bool> DeleteTaskAsync(Guid userId, Guid taskId);
    }
}
