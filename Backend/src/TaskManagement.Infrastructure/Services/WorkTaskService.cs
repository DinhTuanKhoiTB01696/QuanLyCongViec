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
        private readonly IGamificationService _gamificationService;

        // Roles that can see ALL tasks in a project
        private static readonly string[] ManagerRoles = { "PM", "PO", "SM", "PROJECT_MANAGER", "SCRUM_MASTER" };

        public WorkTaskService(ApplicationDbContext context, IGamificationService gamificationService)
        {
            _context = context;
            _gamificationService = gamificationService;
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
                .Include(wt => wt.IssueModules)
                .Include(wt => wt.IssueLabels)
                .Include(wt => wt.TaskAssignments)
                    .ThenInclude(ta => ta.User)
                .Where(wt => wt.ProjectId == projectId && !wt.IsDeleted && !wt.IsArchived);

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

            var dtos = await query
                .AsNoTracking()
                .OrderByDescending(wt => wt.CreatedAt)
                .Select(wt => new WorkTaskResponseDto
                {
                    Id = wt.Id,
                    Title = wt.Title,
                    Description = wt.Description, // Use projection instead of full loading
                    ProjectId = wt.ProjectId,
                    SprintId = wt.SprintId,
                    TaskTypeId = wt.TaskTypeId,
                    TypeName = wt.TaskType.Name,
                    TaskStatusId = wt.TaskStatusId,
                    StatusName = wt.TaskStatus.Name, // We can normalize this on Frontend/Client if needed, or keeping simple here
                    ReporterId = wt.ReporterId,
                    ReporterName = wt.Reporter.FullName ?? wt.Reporter.Email,
                    AssignedUserId = wt.AssignedUserId,
                    AssigneeName = wt.AssignedUser != null ? wt.AssignedUser.FullName : null,
                    Assignees = wt.TaskAssignments
                        .Where(ta => ta.Status)
                        .Select(ta => new TaskAssigneeDto
                        {
                            UserId = ta.UserId,
                            FullName = ta.User.FullName,
                            Email = ta.User.Email,
                            ProgressPercent = ta.ProgressPercent,
                            ContributionWeight = ta.ContributionWeight,
                            EstimatedHours = ta.EstimatedHours,
                            TotalActualHours = ta.TotalActualHours,
                            IsBlocked = ta.BlockedByUserId.HasValue,
                            BlockedByUserId = ta.BlockedByUserId,
                            BlockReason = ta.BlockReason
                        })
                        .ToList(),
                    ModuleId = wt.IssueModules
                        .OrderBy(im => im.AssignedAt)
                        .Select(im => (Guid?)im.ModuleId)
                        .FirstOrDefault(),
                    LabelIds = wt.IssueLabels
                        .Select(il => il.LabelId)
                        .ToList(),
                    Priority = wt.Priority,
                    StoryPoints = wt.StoryPoints,
                    DueDate = wt.DueDate,
                    PlannedStartDate = wt.PlannedStartDate,
                    PlannedEndDate = wt.PlannedEndDate,
                    CreatedAt = wt.CreatedAt,
                    UpdatedAt = wt.UpdatedAt,
                    RowVersion = wt.RowVersion,
                    SortOrder = wt.SortOrder,
                    SequenceId = wt.SequenceId,
                    IsSubscribed = wt.Subscribers.Any(s => s.UserId == userId)
                })
                .ToListAsync();

            // Optional normalization of status name (since it's a DTO logic)
            foreach (var d in dtos) d.StatusName = NormalizeStatusName(d.StatusName);

            // Sort by SortOrder for Kanban drag-drop compatibility
            dtos = dtos.OrderBy(d => d.SortOrder).ToList();

            return dtos;
        }

        public async Task<WorkTaskResponseDto> CreateAsync(Guid reporterId, CreateWorkTaskDto request)
        {
            // 1. Kiểm tra user là member của project
            if (reporterId != Guid.Empty)
            {
                var isMember = await _context.ProjectMembers
                    .AnyAsync(pm => pm.ProjectId == request.ProjectId && pm.UserId == reporterId && pm.Status);
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

            // 5. Generate SortOrder (LexoRank-style: max current + 65536)
            double maxSort = 0;
            var existingMax = await _context.WorkTasks
                .Where(wt => wt.ProjectId == request.ProjectId && !wt.IsDeleted)
                .MaxAsync(wt => (double?)wt.SortOrder);
            maxSort = (existingMax ?? 0) + 65536;

            // 6. Generate SequenceId (e.g., CUN-42)
            var project = await _context.Projects.FirstAsync(p => p.Id == request.ProjectId);
            project.IssueSequence += 1;
            string sequenceId = $"{project.Identifier}-{project.IssueSequence}";

            var requestedAssigneeIds = (request.AssigneeIds ?? new List<Guid>()).Distinct().ToList();
            if (!requestedAssigneeIds.Any() && request.AssignedUserId.HasValue)
            {
                requestedAssigneeIds.Add(request.AssignedUserId.Value);
            }

            if (requestedAssigneeIds.Any())
            {
                var validAssigneeCount = await _context.ProjectMembers
                    .CountAsync(pm => pm.ProjectId == request.ProjectId && pm.Status && requestedAssigneeIds.Contains(pm.UserId));
                if (validAssigneeCount != requestedAssigneeIds.Count)
                {
                    throw new InvalidOperationException("Assignee khong thuoc du an nay.");
                }
            }

            if (request.SprintId.HasValue)
            {
                var sprintExists = await _context.Sprints
                    .AnyAsync(s => s.Id == request.SprintId.Value && s.ProjectId == request.ProjectId);
                if (!sprintExists)
                {
                    throw new InvalidOperationException("Cycle khong thuoc du an nay.");
                }
            }

            if (request.ModuleId.HasValue)
            {
                var moduleExists = await _context.Modules
                    .AnyAsync(m => m.Id == request.ModuleId.Value && m.ProjectId == request.ProjectId);
                if (!moduleExists)
                {
                    throw new InvalidOperationException("Module khong thuoc du an nay.");
                }
            }

            if (request.ParentTaskId.HasValue)
            {
                var parentExists = await _context.WorkTasks
                    .AnyAsync(wt => wt.Id == request.ParentTaskId.Value && wt.ProjectId == request.ProjectId && !wt.IsDeleted);
                if (!parentExists)
                {
                    throw new InvalidOperationException("Parent task khong thuoc du an nay.");
                }
            }

            var requestedLabelIds = (request.LabelIds ?? new List<Guid>()).Distinct().ToList();
            if (requestedLabelIds.Any())
            {
                var validLabelCount = await _context.Labels
                    .CountAsync(label => requestedLabelIds.Contains(label.Id)
                        && (label.ProjectId == request.ProjectId || (label.ProjectId == null && label.WorkspaceId == project.WorkspaceId)));
                if (validLabelCount != requestedLabelIds.Count)
                {
                    throw new InvalidOperationException("Label khong thuoc du an nay.");
                }
            }

            // 7. Create entity
            var workTask = new TaskManagement.Domain.Entities.WorkTask
            {
                Id = Guid.NewGuid(),
                ProjectId = request.ProjectId,
                WorkspaceId = project.WorkspaceId,
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
                SortOrder = maxSort,
                SequenceId = sequenceId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.WorkTasks.Add(workTask);
            await _context.SaveChangesAsync();

            if (requestedAssigneeIds.Any())
            {
                workTask.AssignedUserId = requestedAssigneeIds[0];
                foreach (var assigneeId in requestedAssigneeIds)
                {
                    _context.TaskAssignments.Add(new Domain.Entities.TaskAssignment
                    {
                        WorkTaskId = workTask.Id,
                        UserId = assigneeId,
                        Status = true
                    });
                }
            }

            if (request.ModuleId.HasValue)
            {
                _context.IssueModules.Add(new Domain.Entities.IssueModule
                {
                    WorkTaskId = workTask.Id,
                    ModuleId = request.ModuleId.Value,
                    AssignedAt = DateTime.UtcNow
                });
            }

            foreach (var labelId in requestedLabelIds)
            {
                _context.IssueLabels.Add(new Domain.Entities.IssueLabel
                {
                    WorkTaskId = workTask.Id,
                    LabelId = labelId,
                    AssignedAt = DateTime.UtcNow
                });
            }

            if (requestedAssigneeIds.Any() || request.ModuleId.HasValue || requestedLabelIds.Any())
            {
                await _context.SaveChangesAsync();
            }

            // Reload with navigation properties
            var created = await _context.WorkTasks
                .Include(wt => wt.TaskStatus)
                .Include(wt => wt.TaskType)
                .Include(wt => wt.Reporter)
                .Include(wt => wt.AssignedUser)
                .Include(wt => wt.IssueModules)
                .Include(wt => wt.IssueLabels)
                .Include(wt => wt.TaskAssignments)
                    .ThenInclude(ta => ta.User)
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
                .FirstOrDefaultAsync(pm => pm.ProjectId == taskToUpdate.ProjectId && pm.UserId == userId && pm.Status);

            if (membership == null)
                throw new UnauthorizedAccessException("Bạn không phải thành viên của dự án này.");

            bool isManager = ManagerRoles.Contains(membership.ProjectRole, StringComparer.OrdinalIgnoreCase);
            if (!isManager && taskToUpdate.ReporterId != userId && taskToUpdate.AssignedUserId != userId && !taskToUpdate.TaskAssignments.Any(ta => ta.UserId == userId))
            {
                 throw new UnauthorizedAccessException("Bạn không có quyền sửa đổi tác vụ này.");
            }

            if (dto.AssignedUserId.HasValue)
            {
                var assigneeIsMember = await _context.ProjectMembers
                    .AnyAsync(pm => pm.ProjectId == taskToUpdate.ProjectId && pm.UserId == dto.AssignedUserId.Value && pm.Status);
                if (!assigneeIsMember)
                {
                    throw new InvalidOperationException("Assignee khong thuoc du an nay.");
                }
            }

            if (dto.SprintId.HasValue)
            {
                var sprintExists = await _context.Sprints
                    .AnyAsync(s => s.Id == dto.SprintId.Value && s.ProjectId == taskToUpdate.ProjectId);
                if (!sprintExists)
                {
                    throw new InvalidOperationException("Cycle khong thuoc du an nay.");
                }
            }

            if (dto.TaskTypeId != Guid.Empty)
            {
                var taskTypeExists = await _context.TaskTypes
                    .AnyAsync(tt => tt.Id == dto.TaskTypeId && tt.ProjectId == taskToUpdate.ProjectId);
                if (!taskTypeExists)
                {
                    throw new InvalidOperationException("Task type khong thuoc du an nay.");
                }
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
            taskToUpdate.AssignedUserId = dto.AssignedUserId;
            taskToUpdate.PlannedStartDate = dto.PlannedStartDate;
            taskToUpdate.PlannedEndDate = dto.PlannedEndDate;
            taskToUpdate.DueDate = dto.DueDate;
            taskToUpdate.SprintId = dto.SprintId;
            taskToUpdate.TaskTypeId = dto.TaskTypeId != Guid.Empty ? dto.TaskTypeId : taskToUpdate.TaskTypeId;
            
            taskToUpdate.UpdatedAt = DateTime.UtcNow;


            if (dto.RowVersion != null && dto.RowVersion.Length > 0)
            {
                _context.Entry(taskToUpdate).Property(nameof(taskToUpdate.RowVersion)).OriginalValue = dto.RowVersion;
            }

            await _context.SaveChangesAsync();

            return MapToDto(taskToUpdate);
        }

        public async Task UpdateTaskStatusAsync(Guid taskId, Guid userId, UpdateTaskStatusRequestDto request)
        {
            var taskToUpdate = await _context.WorkTasks
                .Include(wt => wt.TaskStatus)
                .Include(wt => wt.Sprint)
                .Include(wt => wt.TaskAssignments)
                .FirstOrDefaultAsync(wt => wt.Id == taskId && !wt.IsDeleted);

            if (taskToUpdate == null) throw new ArgumentException("Tác vụ không tồn tại.");

            var membership = await _context.ProjectMembers
                .FirstOrDefaultAsync(pm => pm.ProjectId == taskToUpdate.ProjectId && pm.UserId == userId && pm.Status);
            if (membership == null)
                throw new UnauthorizedAccessException("Ban khong phai thanh vien cua du an nay.");

            var canUpdateTask = ManagerRoles.Contains(membership.ProjectRole, StringComparer.OrdinalIgnoreCase)
                || taskToUpdate.ReporterId == userId
                || taskToUpdate.AssignedUserId == userId
                || taskToUpdate.TaskAssignments.Any(ta => ta.UserId == userId && ta.Status);
            if (!canUpdateTask)
                throw new UnauthorizedAccessException("Ban khong co quyen sua tac vu nay.");

            // Sprint Lock
            if (taskToUpdate.Sprint != null && !taskToUpdate.Sprint.Status)
                throw new InvalidOperationException("Không thể chỉnh sửa Task của một Sprint đã đóng.");

            var oldStatus = taskToUpdate.TaskStatus;
            var oldStatusName = oldStatus.Name;

            // Resolve new status
            TaskManagement.Domain.Entities.TaskStatus? newStatus = null;
            if (request.TaskStatusId != Guid.Empty)
            {
                newStatus = await _context.TaskStatuses.FirstOrDefaultAsync(ts => ts.Id == request.TaskStatusId && ts.ProjectId == taskToUpdate.ProjectId);
            }
            else if (!string.IsNullOrEmpty(request.StatusName))
            {
                newStatus = await ResolveTaskStatusAsync(request.StatusName, taskToUpdate.ProjectId);
            }

            if (newStatus == null) throw new ArgumentException("Trạng thái chuyển đến không tồn tại.");
            if (oldStatus.Id == newStatus.Id) return;

            // Allow all status transitions — guardrails are dependency/subtask checks only.

            bool isMovingToActiveOrDone = newStatus.Name.Contains("In Progress", StringComparison.OrdinalIgnoreCase) ||
                                          newStatus.Name.Contains("Done", StringComparison.OrdinalIgnoreCase);

            // Task Dependencies check
            if (isMovingToActiveOrDone)
            {
                var blockers = await _context.TaskDependencies
                    .Include(td => td.PredecessorTask)
                    .ThenInclude(pt => pt.TaskStatus)
                    .Where(td => td.SuccessorTaskId == taskId && td.DependencyType == 1)
                    .Select(td => td.PredecessorTask)
                    .ToListAsync();

                foreach (var blocker in blockers)
                {
                    bool isBlockerDone = blocker.TaskStatus.Name.Contains("Done", StringComparison.OrdinalIgnoreCase);
                    if (!isBlockerDone)
                        throw new InvalidOperationException($"Tác vụ phụ thuộc '{blocker.Title}' chưa hoàn thành.");
                }
            }

            // Parent-Subtask Constraint
            bool isMovingToDone = newStatus.Name.Contains("Done", StringComparison.OrdinalIgnoreCase);
            if (isMovingToDone)
            {
                bool hasUnfinishedSubtasks = await _context.WorkTasks
                    .Include(wt => wt.TaskStatus)
                    .AnyAsync(wt => wt.ParentTaskId == taskId && !wt.IsDeleted &&
                                    !wt.TaskStatus.Name.Contains("Done", StringComparison.OrdinalIgnoreCase));
                if (hasUnfinishedSubtasks)
                    throw new InvalidOperationException("Không thể hoàn thành tác vụ cha khi vẫn còn subtask chưa hoàn thành.");
            }

            taskToUpdate.TaskStatusId = newStatus.Id;
            taskToUpdate.UpdatedAt = DateTime.UtcNow;

            if (request.RowVersion != null && request.RowVersion.Length > 0)
            {
                _context.Entry(taskToUpdate).Property(nameof(taskToUpdate.RowVersion)).OriginalValue = request.RowVersion;
            }

            try
            {
                await _context.SaveChangesAsync();
                await _gamificationService.ApplyStatusChangeRewardsAsync(taskId, userId, oldStatusName, newStatus.Name);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
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
                .Where(wt => wt.ProjectId == projectId && !wt.IsDeleted && !wt.IsArchived)
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
                .Include(wt => wt.TaskAssignments)
                    .ThenInclude(ta => ta.User)
                .Include(wt => wt.Project)
                .Where(wt => (wt.AssignedUserId == userId || wt.TaskAssignments.Any(ta => ta.UserId == userId && ta.Status) || wt.Subscribers.Any(s => s.UserId == userId)) && !wt.IsDeleted && !wt.IsArchived)
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
                    Assignees = wt.TaskAssignments
                        .Where(ta => ta.Status)
                        .Select(ta => new TaskAssigneeDto
                        {
                            UserId = ta.UserId,
                            FullName = ta.User.FullName,
                            Email = ta.User.Email,
                            ProgressPercent = ta.ProgressPercent,
                            ContributionWeight = ta.ContributionWeight,
                            EstimatedHours = ta.EstimatedHours,
                            TotalActualHours = ta.TotalActualHours,
                            IsBlocked = ta.BlockedByUserId.HasValue,
                            BlockedByUserId = ta.BlockedByUserId,
                            BlockReason = ta.BlockReason
                        })
                        .ToList(),
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
                    ProjectName = wt.Project.Name,
                    IsSubscribed = wt.Subscribers.Any(s => s.UserId == userId)
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
            if (string.IsNullOrEmpty(dbStatusName)) return "BACKLOG";

            var upper = dbStatusName.ToUpper().Replace(" ", "");

            if (upper.Contains("CANCEL") || upper.Contains("HUY"))
                return "CANCELLED";
            if (upper.Contains("DONE") || upper.Contains("HOANTHANH") || upper.Contains("COMPLETE"))
                return "DONE";
            if (upper.Contains("INREVIEW") || upper.Contains("REVIEW") || upper.Contains("KIEMTRA"))
                return "IN REVIEW";
            if (upper.Contains("INPROGRESS") || upper.Contains("DANGLAM") || upper.Contains("ACTIVE"))
                return "IN PROGRESS";
            if (upper.Contains("TODO") || upper.Contains("CANLAM"))
                return "TO DO";
            if (upper.Contains("BACKLOG"))
                return "BACKLOG";

            // For seed data like "Status 1", "Status 2", etc. → default to BACKLOG
            return "BACKLOG";
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
                ReporterName = wt.Reporter != null ? wt.Reporter.FullName : "",
                AssignedUserId = wt.AssignedUserId,
                AssigneeName = wt.AssignedUser != null ? wt.AssignedUser.FullName : null,
                Assignees = wt.TaskAssignments
                    .Where(ta => ta.Status)
                    .Select(ta => new TaskAssigneeDto
                    {
                        UserId = ta.UserId,
                        FullName = ta.User.FullName,
                        Email = ta.User.Email,
                        ProgressPercent = ta.ProgressPercent,
                        ContributionWeight = ta.ContributionWeight,
                        EstimatedHours = ta.EstimatedHours,
                        TotalActualHours = ta.TotalActualHours,
                        IsBlocked = ta.BlockedByUserId.HasValue,
                        BlockedByUserId = ta.BlockedByUserId,
                        BlockReason = ta.BlockReason
                    })
                    .ToList(),
                ModuleId = wt.IssueModules
                    .OrderBy(im => im.AssignedAt)
                    .Select(im => (Guid?)im.ModuleId)
                    .FirstOrDefault(),
                LabelIds = wt.IssueLabels
                    .Select(il => il.LabelId)
                    .ToList(),
                Priority = wt.Priority,
                StoryPoints = wt.StoryPoints,
                DueDate = wt.DueDate,
                PlannedStartDate = wt.PlannedStartDate,
                PlannedEndDate = wt.PlannedEndDate,
                ParentTaskId = wt.ParentTaskId,
                CreatedAt = wt.CreatedAt,
                UpdatedAt = wt.UpdatedAt,
                RowVersion = wt.RowVersion,
                SortOrder = wt.SortOrder,
                SequenceId = wt.SequenceId
            };
        }

        public async Task<List<WorkTaskResponseDto>> SearchTasksAsync(Guid userId, string? query, string? status, Guid? assigneeId, int? priority, Guid? projectId = null, string? scope = "all")
        {
            // TÌM CÁC PROJECT MÀ USER CÓ QUYỀN
            var userProjectIdsQuery = _context.ProjectMembers
                .Where(pm => pm.UserId == userId && pm.Status);

            if (scope == "my")
            {
                // Only projects created by user or where user is a member (already covered by member check)
                // But typically "My Projects" means projects where I am a creator or lead.
                // Let's stick to Projects I am a member of for now, or refine if needed.
                var userCreatedProjectIds = await _context.Projects
                    .Where(p => p.CreatorId == userId && !p.IsDeleted)
                    .Select(p => p.Id)
                    .ToListAsync();
                
                var memberProjectIds = await userProjectIdsQuery.Select(pm => pm.ProjectId).ToListAsync();
                var combinedIds = userCreatedProjectIds.Union(memberProjectIds).ToList();
                userProjectIdsQuery = _context.ProjectMembers.Where(pm => combinedIds.Contains(pm.ProjectId));
            }

            var userProjectIds = await userProjectIdsQuery.Select(pm => pm.ProjectId).ToListAsync();

            var dbQuery = _context.WorkTasks
                .Include(wt => wt.TaskStatus)
                .Include(wt => wt.TaskType)
                .Include(wt => wt.Reporter)
                .Include(wt => wt.AssignedUser)
                .Include(wt => wt.TaskAssignments)
                    .ThenInclude(ta => ta.User)
                .Include(wt => wt.IssueModules)
                .Include(wt => wt.IssueLabels)
                .Include(wt => wt.Project)
                .Where(wt => !wt.IsDeleted);

            // Scope filter
            if (scope == "archived")
            {
                dbQuery = dbQuery.Where(wt => wt.Project.IsArchived || wt.IsArchived);
            }
            else
            {
                dbQuery = dbQuery.Where(wt => !wt.Project.IsArchived && !wt.IsArchived);
            }

            // Project ID filter
            if (projectId.HasValue && projectId.Value != Guid.Empty)
            {
                dbQuery = dbQuery.Where(wt => wt.ProjectId == projectId.Value);
            }
            else
            {
                // If no specific project, restrict to user's projects
                dbQuery = dbQuery.Where(wt => userProjectIds.Contains(wt.ProjectId) || wt.Subscribers.Any(s => s.UserId == userId));
            }

            // Filtering
            if (!string.IsNullOrEmpty(query))
            {
                string lowerQuery = query.ToLower();
                dbQuery = dbQuery.Where(wt => 
                    wt.Title.ToLower().Contains(lowerQuery) || 
                    (wt.Description != null && wt.Description.ToLower().Contains(lowerQuery)));
            }

            if (!string.IsNullOrEmpty(status))
            {
                string lowerStatus = status.ToLower();
                dbQuery = dbQuery.Where(wt => wt.TaskStatus.Name.ToLower() == lowerStatus);
            }

            if (assigneeId.HasValue)
            {
                dbQuery = dbQuery.Where(wt => wt.AssignedUserId == assigneeId.Value || wt.TaskAssignments.Any(ta => ta.UserId == assigneeId.Value && ta.Status));
            }

            if (priority.HasValue)
            {
                dbQuery = dbQuery.Where(wt => wt.Priority == priority.Value);
            }

            var results = await dbQuery
                .OrderByDescending(wt => wt.CreatedAt)
                .Select(wt => new WorkTaskResponseDto
                {
                    Id = wt.Id,
                    Title = wt.Title,
                    Description = wt.Description,
                    ProjectId = wt.ProjectId,
                    SprintId = wt.SprintId,
                    TaskTypeId = wt.TaskTypeId,
                    TypeName = wt.TaskType.Name,
                    TaskStatusId = wt.TaskStatusId,
                    StatusName = wt.TaskStatus.Name,
                    ReporterId = wt.ReporterId,
                    ReporterName = wt.Reporter.FullName ?? wt.Reporter.Email,
                    AssignedUserId = wt.AssignedUserId,
                    AssigneeName = wt.AssignedUser != null ? wt.AssignedUser.FullName : null,
                    Assignees = wt.TaskAssignments
                        .Where(ta => ta.Status)
                        .Select(ta => new TaskAssigneeDto
                        {
                            UserId = ta.UserId,
                            FullName = ta.User.FullName,
                            Email = ta.User.Email,
                            ProgressPercent = ta.ProgressPercent,
                            ContributionWeight = ta.ContributionWeight,
                            EstimatedHours = ta.EstimatedHours,
                            TotalActualHours = ta.TotalActualHours,
                            IsBlocked = ta.BlockedByUserId.HasValue,
                            BlockedByUserId = ta.BlockedByUserId,
                            BlockReason = ta.BlockReason
                        })
                        .ToList(),
                    ModuleId = wt.IssueModules
                        .OrderBy(im => im.AssignedAt)
                        .Select(im => (Guid?)im.ModuleId)
                        .FirstOrDefault(),
                    LabelIds = wt.IssueLabels
                        .Select(il => il.LabelId)
                        .ToList(),
                    Priority = wt.Priority,
                    StoryPoints = wt.StoryPoints,
                    DueDate = wt.DueDate,
                    CreatedAt = wt.CreatedAt,
                    IsSubscribed = wt.Subscribers.Any(s => s.UserId == userId)
                })
                .ToListAsync();

            foreach (var r in results) r.StatusName = NormalizeStatusName(r.StatusName);

            return results;
        }

        public async Task ArchiveAsync(Guid id)
        {
            var task = await _context.WorkTasks.FirstOrDefaultAsync(wt => wt.Id == id);
            if (task == null) throw new ArgumentException("Tac vu khong ton tai.");

            task.IsArchived = true;
            task.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task RestoreAsync(Guid id)
        {
            var task = await _context.WorkTasks.FirstOrDefaultAsync(wt => wt.Id == id);
            if (task == null) throw new ArgumentException("Tac vu khong ton tai.");

            task.IsArchived = false;
            task.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ToggleSubscriptionAsync(Guid taskId, Guid userId)
        {
            var existing = await _context.TaskSubscribers
                .FirstOrDefaultAsync(ts => ts.WorkTaskId == taskId && ts.UserId == userId);

            if (existing != null)
            {
                _context.TaskSubscribers.Remove(existing);
                await _context.SaveChangesAsync();
                return false;
            }

            _context.TaskSubscribers.Add(new TaskManagement.Domain.Entities.TaskSubscriber
            {
                WorkTaskId = taskId,
                UserId = userId,
                SubscribedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<TaskManagement.Domain.Entities.TaskStatus> ResolveTaskStatusAsync(string? statusName, Guid projectId)
        {
            TaskManagement.Domain.Entities.TaskStatus? taskStatus = null;

            // Normalize the requested status name to one of the 3 standard names
            string normalizedName = NormalizeStatusName(statusName);

            // Try to find an existing status in this project that matches the normalized name
            var projectStatuses = await _context.TaskStatuses
                .Where(ts => ts.ProjectId == projectId)
                .ToListAsync();

            taskStatus = projectStatuses.FirstOrDefault(ts =>
                NormalizeStatusName(ts.Name) == normalizedName);

            if (taskStatus != null)
                return taskStatus;

            // No matching status found in this project - create one
            int position = normalizedName switch
            {
                "BACKLOG" => 0,
                "TO DO" => 1,
                "IN PROGRESS" => 2,
                "IN REVIEW" => 3,
                "DONE" => 4,
                "CANCELLED" => 5,
                _ => 0
            };

            taskStatus = new TaskManagement.Domain.Entities.TaskStatus
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                Name = normalizedName,
                Position = position
            };
            _context.TaskStatuses.Add(taskStatus);
            await _context.SaveChangesAsync();

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
                taskType = new TaskManagement.Domain.Entities.TaskType
                {
                    Id = Guid.NewGuid(),
                    Name = string.IsNullOrEmpty(typeName) ? "Task" : typeName,
                    ProjectId = projectId,
                    ColorCode = "#3b82f6"
                };
                _context.TaskTypes.Add(taskType);
                await _context.SaveChangesAsync();
            }

            return taskType.Id;
        }
    }
}
