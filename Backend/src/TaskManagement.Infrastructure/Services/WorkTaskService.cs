using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManagement.Application.DTOs.WorkTask;
using TaskManagement.Application.Interfaces;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure.Services
{
    public class WorkTaskService : IWorkTaskService
    {
        private readonly ApplicationDbContext _context;

        // Roles that can see ALL tasks in a project
        private static readonly string[] ManagerRoles = { "PM", "PO", "SM", "PROJECT_MANAGER", "SCRUM_MASTER" };

        public WorkTaskService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<WorkTaskResponseDto>> GetByProjectAsync(Guid projectId, Guid userId)
        {
            // 1. Check user is a member of this project
            var membership = await _context.ProjectMembers
                .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);

            if (membership == null)
            {
                throw new UnauthorizedAccessException("Bạn không phải thành viên của dự án này.");
            }

            // 2. Load all tasks for this project
            var query = _context.WorkTasks
                .Include(wt => wt.TaskStatus)
                .Include(wt => wt.TaskType)
                .Include(wt => wt.Reporter)
                .Include(wt => wt.AssignedUser)
                .Where(wt => wt.ProjectId == projectId && !wt.IsDeleted);

            // 3. Role-based filtering
            bool isManager = ManagerRoles.Contains(membership.ProjectRole, StringComparer.OrdinalIgnoreCase);

            if (!isManager)
            {
                // DEV, QA, etc. → chỉ thấy tasks được assign cho mình hoặc mình tạo
                query = query.Where(wt =>
                    wt.ReporterId == userId ||
                    wt.AssignedUserId == userId ||
                    wt.TaskAssignments.Any(ta => ta.UserId == userId)
                );
            }

            var tasks = await query
                .OrderByDescending(wt => wt.CreatedAt)
                .ToListAsync();

            // 4. Map → DTO with normalized statusName
            return tasks.Select(wt => MapToDto(wt)).ToList();
        }

        public async Task<WorkTaskResponseDto> CreateAsync(Guid reporterId, CreateWorkTaskDto request)
        {
            // 1. Kiểm tra user là member của project
            if (reporterId != Guid.Empty)
            {
                var isMember = await _context.ProjectMembers
                    .AnyAsync(pm => pm.ProjectId == request.ProjectId && pm.UserId == reporterId);
                if (!isMember)
                {
                    throw new UnauthorizedAccessException("Bạn không phải thành viên của dự án này. Không thể tạo công việc.");
                }
            }

            // 2. Resolve TaskStatus
            var taskStatus = await ResolveTaskStatusAsync(request.StatusName, request.ProjectId);

            // 3. Resolve TaskType
            var resolvedTaskTypeId = await ResolveTaskTypeIdAsync(request.TaskTypeId, request.TypeName, request.ProjectId);

            // 4. Fallback reporter
            if (reporterId == Guid.Empty)
            {
                var firstUser = await _context.Users.FirstOrDefaultAsync();
                if (firstUser != null) reporterId = firstUser.Id;
            }

            // 5. Create entity
            var workTask = new TaskManagement.Domain.Entities.WorkTask
            {
                Id = Guid.NewGuid(),
                ProjectId = request.ProjectId,
                SprintId = request.SprintId,
                ParentTaskId = request.ParentTaskId,
                TaskTypeId = resolvedTaskTypeId,
                TaskStatusId = taskStatus.Id,
                Title = request.Title,
                Description = request.Description,
                Priority = request.Priority,
                StoryPoints = request.StoryPoints,
                PlannedStartDate = request.PlannedStartDate,
                PlannedEndDate = request.PlannedEndDate,
                ReporterId = reporterId,
                AssignedUserId = request.AssignedUserId,
                DueDate = request.DueDate,
                TotalEstimatedHours = request.TotalEstimatedHours,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.WorkTasks.Add(workTask);
            await _context.SaveChangesAsync();

            // Reload with navigation properties
            var created = await _context.WorkTasks
                .Include(wt => wt.TaskStatus)
                .Include(wt => wt.TaskType)
                .Include(wt => wt.Reporter)
                .Include(wt => wt.AssignedUser)
                .FirstAsync(wt => wt.Id == workTask.Id);

            return MapToDto(created);
        }

        public async Task<WorkTaskResponseDto> UpdateAsync(Guid taskId, Guid userId, UpdateWorkTaskDto dto)
        {
            var taskToUpdate = await _context.WorkTasks
                .Include(wt => wt.TaskStatus)
                .Include(wt => wt.Sprint)
                .Include(wt => wt.TaskType)
                .Include(wt => wt.Reporter)
                .Include(wt => wt.AssignedUser)
                .Include(wt => wt.TaskAssignments)
                .FirstOrDefaultAsync(wt => wt.Id == taskId && !wt.IsDeleted);

            if (taskToUpdate == null) throw new ArgumentException("Tác vụ không tồn tại.");

            // 1. RBAC Check
            var membership = await _context.ProjectMembers
                .FirstOrDefaultAsync(pm => pm.ProjectId == taskToUpdate.ProjectId && pm.UserId == userId);

            if (membership == null)
                throw new UnauthorizedAccessException("Bạn không phải thành viên của dự án này.");

            bool isManager = ManagerRoles.Contains(membership.ProjectRole, StringComparer.OrdinalIgnoreCase);
            if (!isManager && taskToUpdate.ReporterId != userId && taskToUpdate.AssignedUserId != userId && !taskToUpdate.TaskAssignments.Any(ta => ta.UserId == userId))
            {
                 throw new UnauthorizedAccessException("Bạn không có quyền sửa đổi tác vụ này.");
            }

            // 2. Sprint Lock
            if (taskToUpdate.Sprint != null && !taskToUpdate.Sprint.Status)
            {
                throw new InvalidOperationException("Không thể chỉnh sửa Task của một Sprint đã đóng.");
            }

            // Update fields
            taskToUpdate.Title = dto.Title;
            taskToUpdate.Description = dto.Description;
            taskToUpdate.Priority = dto.Priority ?? taskToUpdate.Priority;
            taskToUpdate.StoryPoints = dto.StoryPoints ?? taskToUpdate.StoryPoints;
            taskToUpdate.AssignedUserId = dto.AssignedUserId ?? taskToUpdate.AssignedUserId;
            taskToUpdate.PlannedStartDate = dto.PlannedStartDate ?? taskToUpdate.PlannedStartDate;
            taskToUpdate.PlannedEndDate = dto.PlannedEndDate ?? taskToUpdate.PlannedEndDate;
            taskToUpdate.DueDate = dto.DueDate ?? taskToUpdate.DueDate;
            taskToUpdate.SprintId = dto.SprintId ?? taskToUpdate.SprintId;
            taskToUpdate.TaskTypeId = dto.TaskTypeId != Guid.Empty ? dto.TaskTypeId : taskToUpdate.TaskTypeId;
            
            taskToUpdate.UpdatedAt = DateTime.UtcNow;

            if (dto.RowVersion != null && dto.RowVersion.Length > 0)
            {
                _context.Entry(taskToUpdate).Property(nameof(taskToUpdate.RowVersion)).OriginalValue = dto.RowVersion;
            }

            await _context.SaveChangesAsync();

            return MapToDto(taskToUpdate);
        }

        public async Task UpdateTaskStatusAsync(Guid taskId, UpdateTaskStatusRequestDto request)
        {
            var taskToUpdate = await _context.WorkTasks
                .Include(wt => wt.TaskStatus)
                .Include(wt => wt.Sprint)
                .FirstOrDefaultAsync(wt => wt.Id == taskId && !wt.IsDeleted);

            if (taskToUpdate == null) throw new ArgumentException("Tác vụ không tồn tại.");

            // Sprint Lock
            if (taskToUpdate.Sprint != null && !taskToUpdate.Sprint.Status)
            {
                throw new InvalidOperationException("Không thể chỉnh sửa Task của một Sprint đã đóng.");
            }

            var oldStatus = taskToUpdate.TaskStatus;
            var newStatus = await _context.TaskStatuses.FirstOrDefaultAsync(ts => ts.Id == request.TaskStatusId);

            if (newStatus == null) throw new ArgumentException("Trạng thái chuyển đến không tồn tại.");
            if (oldStatus.Id == newStatus.Id) return;

            // State Machine Guardrails
            if (Math.Abs(oldStatus.Position - newStatus.Position) > 1)
            {
                throw new InvalidOperationException("Không được phép nhảy cóc trạng thái.");
            }

            bool isMovingToActiveOrDone = newStatus.Name.Contains("In Progress", StringComparison.OrdinalIgnoreCase) ||
                                          newStatus.Name.Contains("Done", StringComparison.OrdinalIgnoreCase) ||
                                          newStatus.Name.Contains("Đang làm", StringComparison.OrdinalIgnoreCase) ||
                                          newStatus.Name.Contains("Hoàn thành", StringComparison.OrdinalIgnoreCase);

            // Task Dependencies
            if (isMovingToActiveOrDone)
            {
                var blockers = await _context.TaskDependencies
                    .Include(td => td.PredecessorTask)
                    .ThenInclude(pt => pt.TaskStatus)
                    .Where(td => td.SuccessorTaskId == taskId)
                    .Select(td => td.PredecessorTask)
                    .ToListAsync();

                foreach (var blocker in blockers)
                {
                    bool isBlockerDone = blocker.TaskStatus.Name.Contains("Done", StringComparison.OrdinalIgnoreCase) ||
                                         blocker.TaskStatus.Name.Contains("Hoàn thành", StringComparison.OrdinalIgnoreCase);
                    if (!isBlockerDone)
                    {
                        throw new InvalidOperationException($"Tác vụ phụ thuộc '{blocker.Title}' chưa hoàn thành.");
                    }
                }
            }

            // Parent-Subtask Constraint
            bool isMovingToDone = newStatus.Name.Contains("Done", StringComparison.OrdinalIgnoreCase) ||
                                  newStatus.Name.Contains("Hoàn thành", StringComparison.OrdinalIgnoreCase);

            if (isMovingToDone)
            {
                bool hasUnfinishedSubtasks = await _context.WorkTasks
                    .Include(wt => wt.TaskStatus)
                    .AnyAsync(wt => wt.ParentTaskId == taskId && !wt.IsDeleted &&
                                    !wt.TaskStatus.Name.Contains("Done", StringComparison.OrdinalIgnoreCase) &&
                                    !wt.TaskStatus.Name.Contains("Hoàn thành", StringComparison.OrdinalIgnoreCase));

                if (hasUnfinishedSubtasks)
                {
                    throw new InvalidOperationException("Không thể hoàn thành tác vụ cha khi vẫn còn subtask chưa hoàn thành.");
                }
            }

            taskToUpdate.TaskStatusId = newStatus.Id;
            taskToUpdate.UpdatedAt = DateTime.UtcNow;

            if (request.RowVersion != null && request.RowVersion.Length > 0)
            {
                _context.Entry(taskToUpdate).Property(nameof(taskToUpdate.RowVersion)).OriginalValue = request.RowVersion;
            }

            // (A) Concurrency Handler: Bọc SaveChangesAsync để bắt DbUpdateConcurrencyException
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new DbUpdateConcurrencyException(
                    "Dữ liệu đã bị người khác thay đổi trước bạn. Vui lòng tải lại trang để tránh ghi đè (Anti-Overwrite).");
            }
        }

        public async Task<IEnumerable<WorkTaskResponseDto>> GetTasksByProjectIdAsync(Guid projectId)
        {
            var tasks = await _context.WorkTasks
                .AsNoTracking()
                .Include(wt => wt.TaskStatus)
                .Include(wt => wt.TaskType)
                .Include(wt => wt.Reporter)
                .Include(wt => wt.AssignedUser)
                .Where(wt => wt.ProjectId == projectId && !wt.IsDeleted)
                .OrderByDescending(wt => wt.CreatedAt)
                .Select(wt => new WorkTaskResponseDto
                {
                    Id = wt.Id,
                    ProjectId = wt.ProjectId,
                    Title = wt.Title,
                    Description = wt.Description,
                    Priority = wt.Priority,
                    StoryPoints = wt.StoryPoints,
                    StatusName = wt.TaskStatus.Name,
                    TaskStatusId = wt.TaskStatusId,
                    TaskTypeName = wt.TaskType.Name,
                    AssigneeName = wt.AssignedUser != null ? wt.AssignedUser.FullName : null,
                    AssignedUserId = wt.AssignedUserId,
                    ReporterName = wt.Reporter.FullName,
                    ReporterId = wt.ReporterId,
                    PlannedStartDate = wt.PlannedStartDate,
                    PlannedEndDate = wt.PlannedEndDate,
                    DueDate = wt.DueDate,
                    TotalEstimatedHours = wt.TotalEstimatedHours,
                    TotalActualHours = wt.TotalActualHours,
                    ParentTaskId = wt.ParentTaskId,
                    RowVersion = wt.RowVersion,
                    CreatedAt = wt.CreatedAt,
                    UpdatedAt = wt.UpdatedAt
                })
                .ToListAsync();

            return tasks;
        }

        public async Task<IEnumerable<WorkTaskResponseDto>> GetMyTasksAsync(Guid userId)
        {
            var tasks = await _context.WorkTasks
                .AsNoTracking()
                .Include(wt => wt.TaskStatus)
                .Include(wt => wt.TaskType)
                .Include(wt => wt.Reporter)
                .Include(wt => wt.AssignedUser)
                .Include(wt => wt.Project)
                .Where(wt => wt.AssignedUserId == userId && !wt.IsDeleted)
                .OrderByDescending(wt => wt.UpdatedAt)
                .Select(wt => new WorkTaskResponseDto
                {
                    Id = wt.Id,
                    ProjectId = wt.ProjectId,
                    Title = wt.Title,
                    Description = wt.Description,
                    Priority = wt.Priority,
                    StoryPoints = wt.StoryPoints,
                    StatusName = wt.TaskStatus.Name,
                    TaskStatusId = wt.TaskStatusId,
                    TaskTypeName = wt.TaskType.Name,
                    AssigneeName = wt.AssignedUser != null ? wt.AssignedUser.FullName : null,
                    AssignedUserId = wt.AssignedUserId,
                    ReporterName = wt.Reporter.FullName,
                    ReporterId = wt.ReporterId,
                    PlannedStartDate = wt.PlannedStartDate,
                    PlannedEndDate = wt.PlannedEndDate,
                    DueDate = wt.DueDate,
                    TotalEstimatedHours = wt.TotalEstimatedHours,
                    TotalActualHours = wt.TotalActualHours,
                    ParentTaskId = wt.ParentTaskId,
                    RowVersion = wt.RowVersion,
                    CreatedAt = wt.CreatedAt,
                    UpdatedAt = wt.UpdatedAt,
                    ProjectName = wt.Project.Name
                })
                .ToListAsync();

            return tasks;
        }

        // ==================== PRIVATE HELPERS ====================

        /// <summary>
        /// Normalize DB status name → standard frontend group name.
        /// Frontend expects: "TO DO", "IN PROGRESS", "DONE"
        /// </summary>
        private static string NormalizeStatusName(string? dbStatusName)
        {
            if (string.IsNullOrEmpty(dbStatusName)) return "TO DO";

            var upper = dbStatusName.ToUpper().Replace(" ", "");

            if (upper.Contains("DONE") || upper.Contains("HOANTHANH") || upper.Contains("COMPLETE"))
                return "DONE";
            if (upper.Contains("INPROGRESS") || upper.Contains("DANGLAM") || upper.Contains("ACTIVE"))
                return "IN PROGRESS";
            if (upper.Contains("TODO") || upper.Contains("CANLAM") || upper.Contains("BACKLOG"))
                return "TO DO";

            // For seed data like "Status 1", "Status 2", etc. → default to "TO DO"
            return "TO DO";
        }

        private WorkTaskResponseDto MapToDto(TaskManagement.Domain.Entities.WorkTask wt)
        {
            return new WorkTaskResponseDto
            {
                Id = wt.Id,
                Title = wt.Title,
                Description = wt.Description,
                ProjectId = wt.ProjectId,
                SprintId = wt.SprintId,
                TaskTypeId = wt.TaskTypeId,
                TypeName = wt.TaskType?.Name ?? "",
                TaskStatusId = wt.TaskStatusId,
                StatusName = NormalizeStatusName(wt.TaskStatus?.Name),
                ReporterId = wt.ReporterId,
                ReporterName = wt.Reporter?.FullName ?? "",
                AssignedUserId = wt.AssignedUserId,
                AssigneeName = wt.AssignedUser?.FullName,
                Priority = wt.Priority,
                StoryPoints = wt.StoryPoints,
                DueDate = wt.DueDate,
                PlannedStartDate = wt.PlannedStartDate,
                PlannedEndDate = wt.PlannedEndDate,
                CreatedAt = wt.CreatedAt,
                UpdatedAt = wt.UpdatedAt,
                RowVersion = wt.RowVersion
            };
        }

        private async Task<TaskManagement.Domain.Entities.TaskStatus> ResolveTaskStatusAsync(string? statusName, Guid projectId)
        {
            TaskManagement.Domain.Entities.TaskStatus? taskStatus = null;

            // Try exact match on the specific project first
            if (!string.IsNullOrEmpty(statusName))
            {
                taskStatus = await _context.TaskStatuses
                    .FirstOrDefaultAsync(ts => ts.ProjectId == projectId && ts.Name.ToUpper().Replace(" ", "") == statusName.ToUpper().Replace(" ", ""));
            }

            // If still not found, check globally just in case (e.g. template statuses)
            if (taskStatus == null && !string.IsNullOrEmpty(statusName))
            {
                taskStatus = await _context.TaskStatuses
                    .FirstOrDefaultAsync(ts => ts.Name.ToUpper().Replace(" ", "") == statusName.ToUpper().Replace(" ", ""));
                
                // If it's a global match but not in this project, create a new one for this project
                if (taskStatus != null || !string.IsNullOrEmpty(statusName))
                {
                    taskStatus = new TaskManagement.Domain.Entities.TaskStatus
                    {
                        Id = Guid.NewGuid(),
                        ProjectId = projectId,
                        Name = statusName ?? "TO DO",
                        Position = 1
                    };
                    _context.TaskStatuses.Add(taskStatus);
                    await _context.SaveChangesAsync();
                }
            }

            // Ultimate fallback if statusName was empty
            if (taskStatus == null)
            {
                taskStatus = await _context.TaskStatuses.FirstOrDefaultAsync(ts => ts.ProjectId == projectId);
                if (taskStatus == null) 
                {
                    taskStatus = new TaskManagement.Domain.Entities.TaskStatus { Id = Guid.NewGuid(), ProjectId = projectId, Name = "TO DO", Position = 1 };
                    _context.TaskStatuses.Add(taskStatus);
                    await _context.SaveChangesAsync();
                }
            }

            return taskStatus;
        }

        private async Task<Guid> ResolveTaskTypeIdAsync(Guid? taskTypeId, string? typeName, Guid projectId)
        {
            if (taskTypeId.HasValue && taskTypeId.Value != Guid.Empty)
            {
                return taskTypeId.Value;
            }

            TaskManagement.Domain.Entities.TaskType? taskType = null;

            if (!string.IsNullOrEmpty(typeName))
            {
                taskType = await _context.TaskTypes
                    .FirstOrDefaultAsync(tt => tt.Name.ToUpper() == typeName.ToUpper());
            }
            if (taskType == null)
            {
                taskType = await _context.TaskTypes
                    .FirstOrDefaultAsync(tt => tt.ProjectId == projectId);
            }
            if (taskType == null)
            {
                taskType = await _context.TaskTypes.FirstOrDefaultAsync();
            }

            return taskType?.Id ?? throw new InvalidOperationException("Không tìm thấy loại tác vụ nào.");
        }
    }
}
