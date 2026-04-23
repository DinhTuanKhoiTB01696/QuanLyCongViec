using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskManagement.Application.DTOs.WorkTask;

namespace TaskManagement.Application.Interfaces
{
    public interface IWorkTaskService
    {
        /// <summary>
        /// Get tasks by project with role-based authorization.
        /// PM/PO/SM see all tasks. DEV/QA see only assigned/reported tasks.
        /// </summary>
        Task<List<WorkTaskResponseDto>> GetByProjectAsync(Guid projectId, Guid userId);
        
        Task<WorkTaskResponseDto> CreateAsync(Guid reporterId, CreateWorkTaskDto request);
        Task<WorkTaskResponseDto> UpdateAsync(Guid taskId, Guid userId, UpdateWorkTaskDto dto);
        Task UpdateTaskStatusAsync(Guid taskId, Guid userId, UpdateTaskStatusRequestDto request);
        Task<IEnumerable<WorkTaskResponseDto>> GetTasksByProjectIdAsync(Guid projectId);
        Task<IEnumerable<WorkTaskResponseDto>> GetMyTasksAsync(Guid userId);
        Task<List<WorkTaskResponseDto>> SearchTasksAsync(Guid userId, string? query, string? status, Guid? assigneeId, int? priority, Guid? projectId = null, string? scope = "all");
        Task ArchiveAsync(Guid id);
        Task RestoreAsync(Guid id);
        Task<bool> ToggleSubscriptionAsync(Guid taskId, Guid userId);
    }
}
