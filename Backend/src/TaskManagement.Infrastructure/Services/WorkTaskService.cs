using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.DTOs.WorkTask;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;
using TaskStatus = TaskManagement.Domain.Entities.TaskStatus;

namespace TaskManagement.Infrastructure.Services
{
    public class WorkTaskService : IWorkTaskService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWorkTaskNotificationService _notificationService;

        public WorkTaskService(ApplicationDbContext context, IWorkTaskNotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<IEnumerable<WorkTaskDto>> GetTasksByProjectAsync(Guid projectId)
        {
            return await _context.WorkTasks
                .Include(t => t.TaskStatus)
                .Include(t => t.TaskType)
                .Include(t => t.Reporter)
                .Where(t => t.ProjectId == projectId && !t.IsDeleted)
                .Select(t => MapToDto(t))
                .ToListAsync();
        }

        public async Task<WorkTaskDto?> GetTaskByIdAsync(Guid taskId)
        {
            var task = await _context.WorkTasks
                .Include(t => t.TaskStatus)
                .Include(t => t.TaskType)
                .Include(t => t.Reporter)
                .FirstOrDefaultAsync(t => t.Id == taskId && !t.IsDeleted);

            return task != null ? MapToDto(task) : null;
        }

        public async Task<WorkTaskDto> CreateTaskAsync(Guid userId, CreateWorkTaskDto dto)
        {
            // Resolve TaskStatusId if Guid.Empty
            if (dto.TaskStatusId == Guid.Empty)
            {
                var statusName = !string.IsNullOrEmpty(dto.StatusName) ? dto.StatusName : "TO DO";
                var status = await _context.TaskStatuses
                    .Where(s => s.ProjectId == dto.ProjectId || s.ProjectId == null)
                    .FirstOrDefaultAsync(s => s.Name.ToUpper() == statusName.ToUpper());

                if (status == null)
                {
                    status = await _context.TaskStatuses
                        .Where(s => s.ProjectId == dto.ProjectId || s.ProjectId == null)
                        .OrderBy(s => s.Position)
                        .FirstOrDefaultAsync();
                }

                if (status == null)
                {
                    status = new TaskStatus { 
                        Id = Guid.NewGuid(), 
                        ProjectId = dto.ProjectId, 
                        Name = statusName, 
                        Position = 1,
                        ColorCode = "#3b82f6"
                    };
                    _context.TaskStatuses.Add(status);
                    await _context.SaveChangesAsync();
                }
                
                dto.TaskStatusId = status.Id;
            }

            // Resolve TaskTypeId if Guid.Empty
            if (dto.TaskTypeId == Guid.Empty)
            {
                var typeName = !string.IsNullOrEmpty(dto.TypeName) ? dto.TypeName : "Task";
                var type = await _context.TaskTypes
                    .Where(t => t.ProjectId == dto.ProjectId || t.ProjectId == null)
                    .FirstOrDefaultAsync(t => t.Name.ToUpper() == typeName.ToUpper());

                if (type == null)
                {
                    type = await _context.TaskTypes
                        .Where(t => t.ProjectId == dto.ProjectId || t.ProjectId == null)
                        .FirstOrDefaultAsync();
                }

                if (type == null)
                {
                    type = new TaskType { 
                        Id = Guid.NewGuid(), 
                        ProjectId = dto.ProjectId, 
                        Name = typeName, 
                        ColorCode = "#FFFFFF",
                        Icon = "fa-solid fa-list-check"
                    };
                    _context.TaskTypes.Add(type);
                    await _context.SaveChangesAsync();
                }

                dto.TaskTypeId = type.Id;
            }

            var task = new WorkTask
            {
                Id = Guid.NewGuid(),
                ProjectId = dto.ProjectId,
                SprintId = dto.SprintId,
                ParentTaskId = dto.ParentTaskId,
                TaskTypeId = dto.TaskTypeId,
                TaskStatusId = dto.TaskStatusId,
                Title = dto.Title,
                Description = dto.Description,
                Priority = dto.Priority,
                StoryPoints = dto.StoryPoints,
                DueDate = dto.DueDate,
                PlannedStartDate = dto.PlannedStartDate,
                PlannedEndDate = dto.PlannedEndDate,
                ReporterId = userId,
                AssignedUserId = dto.AssignedUserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.WorkTasks.Add(task);
            await _context.SaveChangesAsync();

            // Fetch with related data for DTO
            var createdTask = await _context.WorkTasks
                .Include(t => t.TaskStatus)
                .Include(t => t.TaskType)
                .Include(t => t.Reporter)
                .Include(t => t.AssignedUser)
                .FirstAsync(t => t.Id == task.Id);

            var result = MapToDto(createdTask);

            // Broadcast real-time update
            await _notificationService.NotifyTaskCreatedAsync(task.ProjectId, result);

            return result;
        }

        public async Task<WorkTaskDto?> UpdateTaskAsync(Guid userId, Guid taskId, UpdateWorkTaskDto dto)
        {
            var task = await _context.WorkTasks.FindAsync(taskId);
            if (task == null || task.IsDeleted) return null;

            // Optimistic Concurrency Check
            _context.Entry(task).Property("RowVersion").OriginalValue = dto.RowVersion;

            // Resolve TaskStatusId if Guid.Empty and StatusName is provided
            if (dto.TaskStatusId == Guid.Empty && !string.IsNullOrEmpty(dto.StatusName))
            {
                var status = await _context.TaskStatuses
                    .Where(s => s.ProjectId == task.ProjectId || s.ProjectId == null)
                    .FirstOrDefaultAsync(s => s.Name.ToUpper() == dto.StatusName.ToUpper());
                
                if (status == null)
                {
                    status = new TaskStatus { 
                        Id = Guid.NewGuid(), 
                        ProjectId = task.ProjectId, 
                        Name = dto.StatusName, 
                        Position = 1,
                        ColorCode = "#3b82f6"
                    };
                    _context.TaskStatuses.Add(status);
                    await _context.SaveChangesAsync();
                }
                
                task.TaskStatusId = status.Id;
            }
            else if (dto.TaskStatusId != Guid.Empty)
            {
                task.TaskStatusId = dto.TaskStatusId;
            }

            // Resolve TaskTypeId if Guid.Empty and TypeName is provided
            if (!string.IsNullOrEmpty(dto.TypeName))
            {
                var type = await _context.TaskTypes
                    .Where(t => t.ProjectId == task.ProjectId || t.ProjectId == null)
                    .FirstOrDefaultAsync(t => t.Name.ToUpper() == dto.TypeName.ToUpper());
                
                if (type == null)
                {
                    type = new TaskType { 
                        Id = Guid.NewGuid(), 
                        ProjectId = task.ProjectId, 
                        Name = dto.TypeName, 
                        ColorCode = "#FFFFFF",
                        Icon = "fa-solid fa-list-check"
                    };
                    _context.TaskTypes.Add(type);
                    await _context.SaveChangesAsync();
                }
                
                task.TaskTypeId = type.Id;
            }

            task.SprintId = dto.SprintId;
            task.Title = dto.Title;
            task.Description = dto.Description;
            task.Priority = dto.Priority;
            task.StoryPoints = dto.StoryPoints;
            task.DueDate = dto.DueDate;
            task.PlannedStartDate = dto.PlannedStartDate;
            task.PlannedEndDate = dto.PlannedEndDate;
            task.AssignedUserId = dto.AssignedUserId;
            task.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new Exception("Conflict detected. The task was modified by another user.");
            }

            var updatedTask = await _context.WorkTasks
                .Include(t => t.TaskStatus)
                .Include(t => t.TaskType)
                .Include(t => t.Reporter)
                .Include(t => t.AssignedUser)
                .FirstAsync(t => t.Id == task.Id);

            var result = MapToDto(updatedTask);

            // Broadcast real-time update
            await _notificationService.NotifyTaskUpdatedAsync(task.ProjectId, result);

            return result;
        }

        public async Task<WorkTaskDto?> MoveTaskAsync(Guid userId, Guid taskId, MoveTaskDto dto)
        {
            var task = await _context.WorkTasks.FindAsync(taskId);
            if (task == null || task.IsDeleted) return null;

            // Optimistic Concurrency Check
            _context.Entry(task).Property("RowVersion").OriginalValue = dto.RowVersion;

            // Resolve TaskStatusId if Guid.Empty and StatusName is provided
            if (dto.TaskStatusId == Guid.Empty && !string.IsNullOrEmpty(dto.StatusName))
            {
                var status = await _context.TaskStatuses
                    .Where(s => s.ProjectId == task.ProjectId || s.ProjectId == null)
                    .FirstOrDefaultAsync(s => s.Name.ToUpper() == dto.StatusName.ToUpper());
                
                if (status == null)
                {
                    status = new TaskStatus { 
                        Id = Guid.NewGuid(), 
                        ProjectId = task.ProjectId, 
                        Name = dto.StatusName, 
                        Position = 1,
                        ColorCode = "#3b82f6"
                    };
                    _context.TaskStatuses.Add(status);
                    await _context.SaveChangesAsync();
                }

                task.TaskStatusId = status.Id;
            }
            else if (dto.TaskStatusId != Guid.Empty)
            {
                task.TaskStatusId = dto.TaskStatusId;
            }

            task.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new Exception("Conflict detected. The task was moved by another user.");
            }

            var updatedTask = await _context.WorkTasks
                .Include(t => t.TaskStatus)
                .Include(t => t.TaskType)
                .Include(t => t.Reporter)
                .Include(t => t.AssignedUser)
                .FirstAsync(t => t.Id == task.Id);

            var result = MapToDto(updatedTask);

            // Broadcast real-time update
            await _notificationService.NotifyTaskMovedAsync(task.ProjectId, result);

            return result;
        }

        public async Task<bool> DeleteTaskAsync(Guid userId, Guid taskId)
        {
            var task = await _context.WorkTasks.FindAsync(taskId);
            if (task == null || task.IsDeleted) return false;

            task.IsDeleted = true;
            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Broadcast real-time update
            await _notificationService.NotifyTaskDeletedAsync(task.ProjectId, taskId);

            return true;
        }

        private static WorkTaskDto MapToDto(WorkTask task)
        {
            return new WorkTaskDto
            {
                Id = task.Id,
                ProjectId = task.ProjectId,
                SprintId = task.SprintId,
                ParentTaskId = task.ParentTaskId,
                TaskTypeId = task.TaskTypeId,
                TaskStatusId = task.TaskStatusId,
                Title = task.Title,
                Description = task.Description,
                Priority = task.Priority,
                StoryPoints = task.StoryPoints,
                DueDate = task.DueDate,
                PlannedStartDate = task.PlannedStartDate,
                PlannedEndDate = task.PlannedEndDate,
                ReporterId = task.ReporterId,
                AssignedUserId = task.AssignedUserId,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt,
                RowVersion = task.RowVersion,
                StatusName = task.TaskStatus?.Name ?? string.Empty,
                TypeName = task.TaskType?.Name ?? string.Empty,
                ReporterName = task.Reporter?.FullName ?? "Unknown",
                AssigneeName = task.AssignedUser?.FullName
            };
        }
    }
}
