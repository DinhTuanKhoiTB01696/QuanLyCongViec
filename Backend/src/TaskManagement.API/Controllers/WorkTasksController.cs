using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TaskManagement.API.Filters;
using TaskManagement.Application.DTOs.WorkTask;
using TaskManagement.Application.Interfaces;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.API.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize]
    public class WorkTasksController : ControllerBase
    {
        private readonly IWorkTaskService _workTaskService;
        private static readonly string[] ManagerRoles = { "PM", "PO", "SM", "PROJECT_MANAGER", "SCRUM_MASTER" };

        public WorkTasksController(IWorkTaskService workTaskService)
        {
            _workTaskService = workTaskService;
        }

        private async Task TriggerNotificationEventAsync(string relativePath, object payload)
        {
            var authorizationHeader = Request.Headers.Authorization.ToString();
            if (string.IsNullOrWhiteSpace(authorizationHeader) || string.IsNullOrWhiteSpace(Request.Host.Value))
            {
                return;
            }

            try
            {
                using var client = new HttpClient
                {
                    BaseAddress = new Uri($"{Request.Scheme}://{Request.Host}")
                };
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", authorizationHeader);

                using var content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json");

                await client.PostAsync(relativePath, content);
            }
            catch
            {
                // Notification event failures must not block task updates.
            }
        }

        private async Task<NotificationActorContext?> BuildNotificationActorContextAsync(
            ApplicationDbContext context,
            Guid projectId,
            Guid actorUserId,
            string taskTitle)
        {
            var projectName = await context.Projects
                .Where(project => project.Id == projectId)
                .Select(project => project.Name)
                .FirstOrDefaultAsync();

            var actorName = await context.Users
                .Where(user => user.Id == actorUserId)
                .Select(user => user.FullName ?? user.Email)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(projectName))
            {
                return null;
            }

            return new NotificationActorContext
            {
                ActorName = string.IsNullOrWhiteSpace(actorName) ? "System" : actorName,
                ProjectName = projectName,
                TaskTitle = taskTitle
            };
        }

        private async Task TriggerTaskAssignedNotificationsAsync(
            ApplicationDbContext context,
            Guid projectId,
            Guid taskId,
            Guid actorUserId,
            string taskTitle,
            IEnumerable<Guid> assigneeIds)
        {
            var assigneeList = assigneeIds.Distinct().ToList();
            if (!assigneeList.Any())
            {
                return;
            }

            var actorContext = await BuildNotificationActorContextAsync(context, projectId, actorUserId, taskTitle);
            if (actorContext == null)
            {
                return;
            }

            foreach (var assigneeId in assigneeList)
            {
                await TriggerNotificationEventAsync("/api/notifications/events/task-assigned", new
                {
                    AssigneeUserId = assigneeId,
                    ProjectId = projectId,
                    TaskId = taskId,
                    ProjectName = actorContext.ProjectName,
                    ActorName = actorContext.ActorName,
                    TaskTitle = actorContext.TaskTitle
                });
            }
        }

        private async Task TriggerTaskStatusChangedNotificationAsync(
            ApplicationDbContext context,
            Guid projectId,
            Guid taskId,
            Guid actorUserId,
            string taskTitle,
            string? statusName)
        {
            if (string.IsNullOrWhiteSpace(statusName))
            {
                return;
            }

            var actorContext = await BuildNotificationActorContextAsync(context, projectId, actorUserId, taskTitle);
            if (actorContext == null)
            {
                return;
            }

            await TriggerNotificationEventAsync("/api/notifications/events/task-status-changed", new
            {
                ProjectId = projectId,
                TaskId = taskId,
                ProjectName = actorContext.ProjectName,
                ActorName = actorContext.ActorName,
                TaskTitle = actorContext.TaskTitle,
                StatusName = statusName
            });
        }

        [HttpGet("projects/{projectId}/WorkTasks")]
        public async Task<IActionResult> GetByProject(Guid projectId)
        {
            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdString, out Guid userId))
                {
                    return Unauthorized(new { statusCode = 401, message = "Vui lòng đăng nhập." });
                }

                var tasks = await _workTaskService.GetByProjectAsync(projectId, userId);
                return Ok(new { statusCode = 200, message = "Success", data = tasks });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { statusCode = 403, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { statusCode = 500, message = "Lỗi máy chủ nội bộ: " + ex.Message, stack = ex.ToString() });
            }
        }

        [HttpGet("tasks/my-tasks")]
        public async Task<IActionResult> GetMyTasks()
        {
            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdString, out Guid userId))
                {
                    return Unauthorized(new { statusCode = 401, message = "Token không hợp lệ." });
                }

                var tasks = await _workTaskService.GetMyTasksAsync(userId);
                return Ok(new { statusCode = 200, message = "Success", data = tasks });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { statusCode = 500, message = "Lỗi máy chủ: " + ex.Message });
            }
        }

        [HttpGet("tasks/search")]
        public async Task<IActionResult> SearchTasks([FromQuery] string? query, [FromQuery] string? status, [FromQuery] Guid? assigneeId, [FromQuery] int? priority)
        {
            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdString, out Guid userId))
                {
                    return Unauthorized(new { statusCode = 401, message = "Token không hợp lệ." });
                }

                var tasks = await _workTaskService.SearchTasksAsync(userId, query, status, assigneeId, priority);
                return Ok(new { statusCode = 200, message = "Success", data = tasks });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { statusCode = 500, message = "Lỗi máy chủ: " + ex.Message });
            }
        }

        [HttpGet("projects/{projectId}/task-statuses")]
        public async Task<IActionResult> GetProjectTaskStatuses(Guid projectId, [FromServices] ApplicationDbContext context)
        {
            try
            {
                var statuses = await context.TaskStatuses
                    .Where(ts => ts.ProjectId == projectId)
                    .OrderBy(ts => ts.Position)
                    .Select(ts => new
                    {
                        ts.Id,
                        ts.Name,
                        DisplayName = ts.Name
                    })
                    .ToListAsync();

                if (!statuses.Any())
                {
                    var defaults = new[]
                    {
                        new { Name = "BACKLOG", Position = 0 },
                        new { Name = "TO DO", Position = 1 },
                        new { Name = "IN PROGRESS", Position = 2 },
                        new { Name = "DONE", Position = 3 },
                        new { Name = "CANCELLED", Position = 4 }
                    };

                    foreach (var item in defaults)
                    {
                        context.TaskStatuses.Add(new TaskManagement.Domain.Entities.TaskStatus
                        {
                            Id = Guid.NewGuid(),
                            ProjectId = projectId,
                            Name = item.Name,
                            Position = item.Position
                        });
                    }

                    await context.SaveChangesAsync();

                    statuses = await context.TaskStatuses
                        .Where(ts => ts.ProjectId == projectId)
                        .OrderBy(ts => ts.Position)
                        .Select(ts => new
                        {
                            ts.Id,
                            ts.Name,
                            DisplayName = ts.Name
                        })
                        .ToListAsync();
                }

                return Ok(new { statusCode = 200, message = "Success", data = statuses });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { statusCode = 500, message = ex.Message });
            }
        }

        [HttpPost("projects/{projectId}/WorkTasks")]
        public async Task<IActionResult> Create(Guid projectId, [FromBody] CreateWorkTaskDto request)
        {
            request.ProjectId = projectId;
            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid reporterId))
                {
                    reporterId = Guid.Empty;
                }
                
                var result = await _workTaskService.CreateAsync(reporterId, request);
                return CreatedAtAction(nameof(GetByProject), new { projectId }, new { statusCode = 201, message = "Tạo tác vụ thành công.", data = result });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { statusCode = 400, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { statusCode = 400, message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { statusCode = 403, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { statusCode = 500, message = "Lỗi máy chủ nội bộ: " + ex.Message, stack = ex.ToString() });
            }
        }

        [HttpPut("projects/{projectId}/WorkTasks/{id}/status")]
        public async Task<IActionResult> UpdateStatus(Guid projectId, Guid id, [FromBody] UpdateTaskStatusRequestDto request, [FromServices] ApplicationDbContext context)
        {
            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdString, out Guid userId))
                {
                    return Unauthorized(new { statusCode = 401, message = "Vui long dang nhap." });
                }

                var previousTask = await context.WorkTasks
                    .AsNoTracking()
                    .Include(wt => wt.TaskStatus)
                    .FirstOrDefaultAsync(wt => wt.Id == id && wt.ProjectId == projectId && !wt.IsDeleted);
                if (previousTask == null)
                {
                    return NotFound(new { statusCode = 404, message = "Task khong ton tai trong du an nay." });
                }

                await _workTaskService.UpdateTaskStatusAsync(id, userId, request);

                var updatedTask = await context.WorkTasks
                    .AsNoTracking()
                    .Include(wt => wt.TaskStatus)
                    .FirstOrDefaultAsync(wt => wt.Id == id && wt.ProjectId == projectId && !wt.IsDeleted);

                if (updatedTask != null && !string.Equals(previousTask.TaskStatus?.Name, updatedTask.TaskStatus?.Name, StringComparison.OrdinalIgnoreCase))
                {
                    await TriggerTaskStatusChangedNotificationAsync(
                        context,
                        projectId,
                        id,
                        userId,
                        updatedTask.Title,
                        updatedTask.TaskStatus?.Name);
                }
                return Ok(new { statusCode = 200, message = "Success", data = "Cập nhật trạng thái tác vụ thành công." });
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(new { statusCode = 409, message = "Dữ liệu đã bị người khác thay đổi. Vui lòng tải lại trang để tránh ghi đè (Anti-Overwrite)." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { statusCode = 400, message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { statusCode = 400, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { statusCode = 500, message = "Lỗi máy chủ nội bộ: " + ex.Message });
            }
        }

        [HttpPut("projects/{projectId}/WorkTasks/{id}")]
        public async Task<IActionResult> Update(Guid projectId, Guid id, [FromBody] UpdateWorkTaskDto dto, [FromServices] ApplicationDbContext context)
        {
            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdString, out Guid userId))
                {
                    return Unauthorized(new { statusCode = 401, message = "Vui lòng đăng nhập." });
                }

                var taskExistsInProject = await context.WorkTasks
                    .AnyAsync(wt => wt.Id == id && wt.ProjectId == projectId && !wt.IsDeleted);
                if (!taskExistsInProject)
                {
                    return NotFound(new { statusCode = 404, message = "Task khong ton tai trong du an nay." });
                }

                if (dto.RowVersion == null || dto.RowVersion.Length == 0)
                {
                    return BadRequest(new { statusCode = 400, message = "Thieu RowVersion. Vui long tai lai cong viec truoc khi cap nhat." });
                }

                var previousTask = await context.WorkTasks
                    .AsNoTracking()
                    .Include(wt => wt.TaskAssignments)
                    .FirstOrDefaultAsync(wt => wt.Id == id && wt.ProjectId == projectId && !wt.IsDeleted);
                var previousAssigneeIds = new HashSet<Guid>();
                if (previousTask?.AssignedUserId is Guid previousAssignedUserId)
                {
                    previousAssigneeIds.Add(previousAssignedUserId);
                }

                if (previousTask != null)
                {
                    foreach (var assignment in previousTask.TaskAssignments.Where(ta => ta.Status))
                    {
                        previousAssigneeIds.Add(assignment.UserId);
                    }
                }

                var result = await _workTaskService.UpdateAsync(id, userId, dto);

                var updatedTask = await context.WorkTasks
                    .AsNoTracking()
                    .Include(wt => wt.TaskAssignments)
                    .FirstOrDefaultAsync(wt => wt.Id == id && wt.ProjectId == projectId && !wt.IsDeleted);

                if (updatedTask != null)
                {
                    var currentAssigneeIds = updatedTask.TaskAssignments
                        .Where(ta => ta.Status)
                        .Select(ta => ta.UserId)
                        .ToHashSet();

                    if (updatedTask.AssignedUserId is Guid currentAssignedUserId)
                    {
                        currentAssigneeIds.Add(currentAssignedUserId);
                    }

                    var newAssigneeIds = currentAssigneeIds
                        .Except(previousAssigneeIds)
                        .ToList();

                    if (newAssigneeIds.Any())
                    {
                        await TriggerTaskAssignedNotificationsAsync(
                            context,
                            projectId,
                            id,
                            userId,
                            updatedTask.Title,
                            newAssigneeIds);
                    }
                }
                return Ok(new { statusCode = 200, message = "Cập nhật công việc thành công.", data = result });
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(new { statusCode = 409, message = "Dữ liệu đã bị người khác thay đổi. Vui lòng tải lại trang để tránh ghi đè (Anti-Overwrite)." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { statusCode = 403, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { statusCode = 400, message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { statusCode = 400, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { statusCode = 500, message = "Lỗi máy chủ nội bộ: " + ex.Message });
            }
        }

        [HttpPatch("projects/{projectId}/WorkTasks/{id}")]
        public async Task<IActionResult> PartialUpdate(Guid projectId, Guid id, [FromBody] System.Text.Json.JsonElement updates, [FromServices] TaskManagement.Infrastructure.Data.ApplicationDbContext context, [FromServices] IGamificationService gamificationService)
        {
            try
            {
                var task = await context.WorkTasks
                    .Include(wt => wt.TaskAssignments)
                    .Include(wt => wt.IssueModules)
                    .Include(wt => wt.TaskStatus)
                    .FirstOrDefaultAsync(wt => wt.Id == id && wt.ProjectId == projectId && !wt.IsDeleted);
                if (task == null) return NotFound(new { statusCode = 404, message = "Task không tồn tại." });

                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdString, out Guid userId))
                {
                    return Unauthorized(new { statusCode = 401, message = "Vui long dang nhap." });
                }

                var membership = await context.ProjectMembers
                    .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId && pm.Status);
                if (membership == null)
                {
                    return StatusCode(403, new { statusCode = 403, message = "Ban khong phai thanh vien cua du an nay." });
                }

                var canUpdateTask = ManagerRoles.Contains(membership.ProjectRole, StringComparer.OrdinalIgnoreCase)
                    || task.ReporterId == userId
                    || task.AssignedUserId == userId
                    || task.TaskAssignments.Any(ta => ta.UserId == userId && ta.Status);
                if (!canUpdateTask)
                {
                    return StatusCode(403, new { statusCode = 403, message = "Ban khong co quyen sua tac vu nay." });
                }
                var oldStatusName = task.TaskStatus?.Name;
                string? newStatusName = null;
                var previousAssigneeIds = task.TaskAssignments
                    .Where(ta => ta.Status)
                    .Select(ta => ta.UserId)
                    .ToHashSet();
                if (task.AssignedUserId is Guid previousAssignedUserId)
                {
                    previousAssigneeIds.Add(previousAssignedUserId);
                }

                if (updates.TryGetProperty("title", out var titleProp) && titleProp.ValueKind != System.Text.Json.JsonValueKind.Null)
                    task.Title = titleProp.GetString() ?? task.Title;
                
                if (updates.TryGetProperty("description", out var descProp))
                    task.Description = descProp.ValueKind == System.Text.Json.JsonValueKind.Null ? null : descProp.GetString();

                if (updates.TryGetProperty("priority", out var prioProp) && prioProp.ValueKind != System.Text.Json.JsonValueKind.Null)
                    task.Priority = prioProp.GetInt32();

                if (updates.TryGetProperty("sortOrder", out var sortProp) && sortProp.ValueKind != System.Text.Json.JsonValueKind.Null)
                    task.SortOrder = sortProp.GetDouble();

                if (updates.TryGetProperty("assigneeId", out var assigneeProp))
                {
                    if (assigneeProp.ValueKind == System.Text.Json.JsonValueKind.Null) task.AssignedUserId = null;
                    else if (Guid.TryParse(assigneeProp.GetString(), out Guid aId))
                    {
                        var assigneeIsMember = await context.ProjectMembers
                            .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == aId && pm.Status);
                        if (!assigneeIsMember)
                        {
                            return BadRequest(new { statusCode = 400, message = "Assignee khong thuoc du an nay." });
                        }

                        task.AssignedUserId = aId;
                    }
                }

                if (updates.TryGetProperty("assigneeIds", out var assigneeIdsProp) && assigneeIdsProp.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    var parsedIds = assigneeIdsProp.EnumerateArray()
                        .Select(item => item.GetString())
                        .Where(value => !string.IsNullOrWhiteSpace(value) && Guid.TryParse(value, out _))
                        .Select(value => Guid.Parse(value!))
                        .Distinct()
                        .ToList();

                    if (parsedIds.Any())
                    {
                        var validAssigneeCount = await context.ProjectMembers
                            .CountAsync(pm => pm.ProjectId == projectId && pm.Status && parsedIds.Contains(pm.UserId));
                        if (validAssigneeCount != parsedIds.Count)
                        {
                            return BadRequest(new { statusCode = 400, message = "Mot hoac nhieu assignee khong thuoc du an nay." });
                        }
                    }

                    var existingAssignments = task.TaskAssignments.ToList();

                    foreach (var assignment in existingAssignments.Where(ta => !parsedIds.Contains(ta.UserId)))
                    {
                        context.TaskAssignments.Remove(assignment);
                    }

                    foreach (var assigneeId in parsedIds.Where(idValue => existingAssignments.All(ta => ta.UserId != idValue)))
                    {
                        context.TaskAssignments.Add(new TaskManagement.Domain.Entities.TaskAssignment
                        {
                            WorkTaskId = task.Id,
                            UserId = assigneeId,
                            Status = true
                        });
                    }

                    task.AssignedUserId = parsedIds.FirstOrDefault();
                    if (!parsedIds.Any())
                    {
                        task.AssignedUserId = null;
                    }
                }

                var progressRewardRequests = new List<(Guid AssigneeId, double OldProgress, double NewProgress)>();
                if (updates.TryGetProperty("assigneeProgress", out var assigneeProgressProp))
                {
                    var progressItems = new List<System.Text.Json.JsonElement>();
                    if (assigneeProgressProp.ValueKind == System.Text.Json.JsonValueKind.Array)
                    {
                        progressItems.AddRange(assigneeProgressProp.EnumerateArray());
                    }
                    else if (assigneeProgressProp.ValueKind == System.Text.Json.JsonValueKind.Object)
                    {
                        progressItems.Add(assigneeProgressProp);
                    }

                    foreach (var item in progressItems)
                    {
                        Guid assigneeId = Guid.Empty;
                        if (item.TryGetProperty("userId", out var userIdProp))
                        {
                            Guid.TryParse(userIdProp.GetString(), out assigneeId);
                        }

                        if (assigneeId == Guid.Empty)
                        {
                            continue;
                        }

                        var assignment = task.TaskAssignments.FirstOrDefault(ta => ta.UserId == assigneeId);
                        if (assignment == null)
                        {
                            assignment = new TaskManagement.Domain.Entities.TaskAssignment
                            {
                                WorkTaskId = task.Id,
                                UserId = assigneeId,
                                Status = true
                            };
                            context.TaskAssignments.Add(assignment);
                            task.TaskAssignments.Add(assignment);
                        }

                        var oldProgress = assignment.ProgressPercent;
                        if (item.TryGetProperty("progressPercent", out var progressProp) && progressProp.ValueKind != System.Text.Json.JsonValueKind.Null)
                        {
                            var progress = progressProp.GetDouble();
                            assignment.ProgressPercent = Math.Clamp(progress, 0, 100);
                            assignment.ProgressUpdatedAt = DateTime.UtcNow;
                        }

                        if (item.TryGetProperty("contributionWeight", out var weightProp) && weightProp.ValueKind != System.Text.Json.JsonValueKind.Null)
                        {
                            assignment.ContributionWeight = Math.Max(0, weightProp.GetDouble());
                        }

                        if (item.TryGetProperty("estimatedHours", out var estimatedProp) && estimatedProp.ValueKind != System.Text.Json.JsonValueKind.Null)
                        {
                            assignment.EstimatedHours = Math.Max(0, estimatedProp.GetDouble());
                        }

                        if (item.TryGetProperty("blockedByUserId", out var blockedByProp))
                        {
                            assignment.BlockedByUserId = blockedByProp.ValueKind == System.Text.Json.JsonValueKind.Null
                                ? null
                                : Guid.TryParse(blockedByProp.GetString(), out var blockedByUserId) ? blockedByUserId : null;
                        }

                        if (item.TryGetProperty("blockReason", out var blockReasonProp))
                        {
                            assignment.BlockReason = blockReasonProp.ValueKind == System.Text.Json.JsonValueKind.Null
                                ? null
                                : blockReasonProp.GetString();
                        }

                        if (oldProgress != assignment.ProgressPercent)
                        {
                            progressRewardRequests.Add((assigneeId, oldProgress, assignment.ProgressPercent));
                        }
                    }

                    var activeAssignments = task.TaskAssignments.Where(ta => ta.Status).ToList();
                    var firstAssignee = activeAssignments.OrderByDescending(ta => ta.ProgressPercent).FirstOrDefault();
                    task.AssignedUserId = firstAssignee?.UserId;
                }

                if (updates.TryGetProperty("plannedStartDate", out var startProp))
                {
                    if (startProp.ValueKind == System.Text.Json.JsonValueKind.Null) task.PlannedStartDate = null;
                    else if (DateTime.TryParse(startProp.GetString(), out DateTime d)) task.PlannedStartDate = d;
                }

                if (updates.TryGetProperty("dueDate", out var dueProp))
                {
                    if (dueProp.ValueKind == System.Text.Json.JsonValueKind.Null) task.DueDate = null;
                    else if (DateTime.TryParse(dueProp.GetString(), out DateTime d)) task.DueDate = d;
                }

                if (updates.TryGetProperty("sprintId", out var sprintProp))
                {
                    if (sprintProp.ValueKind == System.Text.Json.JsonValueKind.Null) task.SprintId = null;
                    else if (Guid.TryParse(sprintProp.GetString(), out Guid sId))
                    {
                        var sprintExists = await context.Sprints.AnyAsync(s => s.Id == sId && s.ProjectId == projectId);
                        if (!sprintExists)
                        {
                            return BadRequest(new { statusCode = 400, message = "Cycle khong thuoc du an nay." });
                        }

                        task.SprintId = sId;
                    }
                }

                if (updates.TryGetProperty("moduleId", out var modProp))
                {
                    context.IssueModules.RemoveRange(task.IssueModules);

                    if (modProp.ValueKind != System.Text.Json.JsonValueKind.Null
                        && Guid.TryParse(modProp.GetString(), out Guid moduleId))
                    {
                        var moduleExists = await context.Modules.AnyAsync(m => m.Id == moduleId && m.ProjectId == projectId);
                        if (!moduleExists)
                        {
                            return BadRequest(new { statusCode = 400, message = "Module khong thuoc du an nay." });
                        }

                        context.IssueModules.Add(new TaskManagement.Domain.Entities.IssueModule
                        {
                            WorkTaskId = task.Id,
                            ModuleId = moduleId,
                            AssignedAt = DateTime.UtcNow
                        });
                    }
                }

                if (updates.TryGetProperty("parentId", out var parProp))
                {
                    if (parProp.ValueKind == System.Text.Json.JsonValueKind.Null) task.ParentTaskId = null;
                    else if (Guid.TryParse(parProp.GetString(), out Guid pId))
                    {
                        if (pId == task.Id)
                        {
                            return BadRequest(new { statusCode = 400, message = "Task khong the tu lam parent." });
                        }

                        var parentExists = await context.WorkTasks
                            .AnyAsync(wt => wt.Id == pId && wt.ProjectId == projectId && !wt.IsDeleted);
                        if (!parentExists)
                        {
                            return BadRequest(new { statusCode = 400, message = "Parent task khong thuoc du an nay." });
                        }

                        task.ParentTaskId = pId;
                    }
                }

                if (updates.TryGetProperty("statusName", out var statusProp) && statusProp.ValueKind != System.Text.Json.JsonValueKind.Null)
                {
                    var statusName = statusProp.GetString();
                    var newStatus = await context.TaskStatuses.FirstOrDefaultAsync(ts => ts.ProjectId == projectId && ts.Name == statusName);
                    if (newStatus != null)
                    {
                        var isDoneStatus = newStatus.Name.Contains("DONE", StringComparison.OrdinalIgnoreCase) ||
                                           newStatus.Name.Contains("Complete", StringComparison.OrdinalIgnoreCase);
                        if (isDoneStatus && task.TaskAssignments.Any(ta => ta.Status && ta.ProgressPercent < 100))
                        {
                            return BadRequest(new { statusCode = 400, message = "Chua the hoan thanh task khi van con assignee chua dat 100%." });
                        }

                        task.TaskStatusId = newStatus.Id;
                        newStatusName = newStatus.Name;
                    }
                }

                task.UpdatedAt = DateTime.UtcNow;
                await context.SaveChangesAsync();
                if (userId != Guid.Empty && !string.IsNullOrWhiteSpace(newStatusName))
                {
                    await gamificationService.ApplyStatusChangeRewardsAsync(id, userId, oldStatusName, newStatusName);
                    if (!string.Equals(oldStatusName, newStatusName, StringComparison.OrdinalIgnoreCase))
                    {
                        await TriggerTaskStatusChangedNotificationAsync(
                            context,
                            projectId,
                            id,
                            userId,
                            task.Title,
                            newStatusName);
                    }
                    if (!string.Equals(oldStatusName, newStatusName, StringComparison.OrdinalIgnoreCase))
                    {
                        await TriggerTaskStatusChangedNotificationAsync(
                            context,
                            projectId,
                            id,
                            userId,
                            task.Title,
                            newStatusName);
                    }
                }
                foreach (var rewardRequest in progressRewardRequests)
                {
                    await gamificationService.ApplyAssignmentProgressRewardsAsync(id, rewardRequest.AssigneeId, userId, rewardRequest.OldProgress, rewardRequest.NewProgress);
                }

                var currentAssigneeIds = task.TaskAssignments
                    .Where(ta => ta.Status)
                    .Select(ta => ta.UserId)
                    .ToHashSet();
                if (task.AssignedUserId is Guid currentAssignedUserId)
                {
                    currentAssigneeIds.Add(currentAssignedUserId);
                }

                var newAssigneeIds = currentAssigneeIds
                    .Except(previousAssigneeIds)
                    .ToList();

                if (newAssigneeIds.Any())
                {
                    await TriggerTaskAssignedNotificationsAsync(
                        context,
                        projectId,
                        id,
                        userId,
                        task.Title,
                        newAssigneeIds);
                }

                if (!string.IsNullOrWhiteSpace(newStatusName) && !string.Equals(oldStatusName, newStatusName, StringComparison.OrdinalIgnoreCase))
                {
                    await TriggerTaskStatusChangedNotificationAsync(
                        context,
                        projectId,
                        id,
                        userId,
                        task.Title,
                        newStatusName);
                }

                return Ok(new { statusCode = 200, message = "Saved", data = task });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { statusCode = 500, message = ex.Message });
            }
        }

        [HttpGet("{id}/comments")]
        public async Task<IActionResult> GetComments(Guid projectId, Guid id, [FromServices] TaskManagement.Infrastructure.Data.ApplicationDbContext context)
        {
            try
            {
                var comments = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(
                    System.Linq.Queryable.OrderBy(
                        System.Linq.Queryable.Select(
                            System.Linq.Queryable.Where(context.Comments, c => c.WorkTaskId == id && !c.IsDeleted),
                            c => new {
                                c.Id,
                                c.Content,
                                c.ParentCommentId,
                                c.CreatedAt,
                                UserId = c.UserId,
                                FullName = c.User.FullName ?? c.User.Email,
                                Avatar = c.User.FullName != null ? c.User.FullName.Substring(0, 1) : "U"
                            }
                        ),
                        c => c.CreatedAt
                    )
                );
                return Ok(new { statusCode = 200, message = "Success", data = comments });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { statusCode = 500, message = ex.Message });
            }
        }

        /// <summary>
        /// Kanban Drag-Drop: Reorder task (update SortOrder + optionally change status)
        /// </summary>
        [HttpPut("projects/{projectId}/WorkTasks/{id}/reorder")]
        public async Task<IActionResult> Reorder(Guid projectId, Guid id, [FromBody] ReorderTaskDto dto, [FromServices] TaskManagement.Infrastructure.Data.ApplicationDbContext context, [FromServices] IGamificationService gamificationService)
        {
            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdString, out Guid userId))
                    return Unauthorized(new { statusCode = 401, message = "Vui long dang nhap." });

                var task = await context.WorkTasks
                    .Include(wt => wt.TaskStatus)
                    .Include(wt => wt.TaskAssignments)
                    .FirstOrDefaultAsync(wt => wt.Id == id && wt.ProjectId == projectId && !wt.IsDeleted);
                if (task == null)
                    return NotFound(new { statusCode = 404, message = "Tác vụ không tồn tại." });

                var oldStatusName = task.TaskStatus?.Name;
                string? newStatusName = null;
                var membership = await context.ProjectMembers
                    .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId && pm.Status);
                if (membership == null)
                {
                    return StatusCode(403, new { statusCode = 403, message = "Ban khong phai thanh vien cua du an nay." });
                }

                var canUpdateTask = ManagerRoles.Contains(membership.ProjectRole, StringComparer.OrdinalIgnoreCase)
                    || task.ReporterId == userId
                    || task.AssignedUserId == userId
                    || task.TaskAssignments.Any(ta => ta.UserId == userId && ta.Status);
                if (!canUpdateTask)
                {
                    return StatusCode(403, new { statusCode = 403, message = "Ban khong co quyen sap xep tac vu nay." });
                }

                task.SortOrder = dto.SortOrder;
                task.UpdatedAt = DateTime.UtcNow;

                // If status also changed (dragged to a different column)
                if (!string.IsNullOrEmpty(dto.NewStatusName))
                {
                    var newStatus = await context.TaskStatuses
                        .FirstOrDefaultAsync(ts => ts.ProjectId == projectId && ts.Name == dto.NewStatusName);
                    if (newStatus != null)
                    {
                        var isDoneStatus = newStatus.Name.Contains("DONE", StringComparison.OrdinalIgnoreCase) ||
                                           newStatus.Name.Contains("Complete", StringComparison.OrdinalIgnoreCase);
                        if (isDoneStatus && task.TaskAssignments.Any(ta => ta.Status && ta.ProgressPercent < 100))
                        {
                            return BadRequest(new { statusCode = 400, message = "Chua the hoan thanh task khi van con assignee chua dat 100%." });
                        }

                        task.TaskStatusId = newStatus.Id;
                        newStatusName = newStatus.Name;
                    }
                }

                await context.SaveChangesAsync();
                if (!string.IsNullOrWhiteSpace(newStatusName))
                {
                    await gamificationService.ApplyStatusChangeRewardsAsync(id, userId, oldStatusName, newStatusName);
                }
                return Ok(new { statusCode = 200, message = "Cập nhật thứ tự thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { statusCode = 500, message = "Lỗi: " + ex.Message });
            }
        }

        /// <summary>
        /// GET /api/projects/{projectId}/WorkTasks/{parentId}/subtasks
        /// </summary>
        [HttpGet("projects/{projectId}/WorkTasks/{parentId}/subtasks")]
        public async Task<IActionResult> GetSubtasks(Guid projectId, Guid parentId, [FromServices] ApplicationDbContext context)
        {
            var subtasks = await context.WorkTasks
                .Where(wt => wt.ParentTaskId == parentId && !wt.IsDeleted)
                .Include(wt => wt.TaskStatus)
                .Include(wt => wt.AssignedUser)
                .OrderBy(wt => wt.SortOrder)
                .Select(wt => new
                {
                    wt.Id,
                    wt.Title,
                    wt.Priority,
                    wt.SortOrder,
                    wt.SequenceId,
                    StatusName = wt.TaskStatus != null ? wt.TaskStatus.Name : "TO DO",
                    AssigneeName = wt.AssignedUser != null ? wt.AssignedUser.FullName : null,
                    AssigneeAvatar = wt.AssignedUser != null ? wt.AssignedUser.AvatarUrl : null,
                    wt.DueDate,
                    wt.CreatedAt
                }).ToListAsync();

            return Ok(new { statusCode = 200, data = subtasks });
        }

        /// <summary>
        /// POST /api/projects/{projectId}/WorkTasks/{parentId}/subtasks — Create a child task
        /// </summary>
        [HttpPost("projects/{projectId}/WorkTasks/{parentId}/subtasks")]
        public async Task<IActionResult> CreateSubtask(Guid projectId, Guid parentId, [FromBody] CreateWorkTaskDto request, [FromServices] ApplicationDbContext context)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdStr, out Guid userId))
                return Unauthorized();

            var parent = await context.WorkTasks.FirstOrDefaultAsync(t => t.Id == parentId && !t.IsDeleted);
            if (parent == null) return NotFound(new { message = "Task cha không tồn tại." });

            // Get default status and type from project
            var defaultStatus = await context.TaskStatuses.FirstOrDefaultAsync(ts => ts.ProjectId == projectId);
            var defaultType = await context.TaskTypes.FirstOrDefaultAsync(tt => tt.ProjectId == projectId);

            if (defaultStatus == null || defaultType == null)
                return BadRequest(new { message = "Project chưa có status hoặc type mặc định." });

            // Generate sequence
            var project = await context.Projects.FindAsync(projectId);
            if (project != null) project.IssueSequence++;

            var subtask = new TaskManagement.Domain.Entities.WorkTask
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                ParentTaskId = parentId,
                Title = request.Title ?? "Subtask",
                Description = request.Description,
                Priority = request.Priority,
                TaskTypeId = defaultType.Id,
                TaskStatusId = defaultStatus.Id,
                ReporterId = userId,
                WorkspaceId = parent.WorkspaceId,
                SequenceId = project != null ? $"{project.Identifier}-{project.IssueSequence}" : null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                SortOrder = 65536
            };

            context.WorkTasks.Add(subtask);
            await context.SaveChangesAsync();

            return Ok(new
            {
                statusCode = 201,
                message = "Tạo subtask thành công.",
                data = new
                {
                    subtask.Id,
                    subtask.Title,
                    subtask.Priority,
                    subtask.ParentTaskId,
                    subtask.SequenceId,
                    StatusName = defaultStatus.Name,
                    subtask.CreatedAt
                }
            });
        }

        /// <summary>
        /// GET /api/dashboard/stats — Real-time dashboard statistics from DB
        /// </summary>
        [HttpGet("/api/dashboard/stats")]
        public async Task<IActionResult> GetDashboardStats([FromServices] ApplicationDbContext context)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdStr, out Guid userId))
            {
                return Unauthorized();
            }

            // Lấy danh sách Project mà user tham gia để ngăn chặn Data Leak (cross-tenant)
            var userProjectIds = await context.ProjectMembers
                .Where(pm => pm.UserId == userId && pm.Status)
                .Select(pm => pm.ProjectId)
                .ToListAsync();

            var baseTaskQuery = context.WorkTasks.Where(t => !t.IsDeleted && userProjectIds.Contains(t.ProjectId));

            var totalTasks = await baseTaskQuery.CountAsync();
            var statusGroups = await baseTaskQuery
                .GroupBy(t => t.TaskStatus!.Name)
                .Select(g => new { Status = g.Key ?? "Unknown", Count = g.Count() })
                .ToListAsync();

            var priorityGroups = await baseTaskQuery
                .GroupBy(t => t.Priority)
                .Select(g => new { Priority = g.Key, Count = g.Count() })
                .ToListAsync();

            var overdueTasks = await baseTaskQuery
                .CountAsync(t => t.DueDate.HasValue && t.DueDate < DateTime.UtcNow);

            var myTasks = await baseTaskQuery.CountAsync(t => t.AssignedUserId == userId);

            var totalProjects = userProjectIds.Count;
            var totalMembers = await context.ProjectMembers
                .Where(pm => userProjectIds.Contains(pm.ProjectId) && pm.Status)
                .Select(pm => pm.UserId)
                .Distinct()
                .CountAsync();

            return Ok(new
            {
                statusCode = 200,
                data = new
                {
                    totalTasks,
                    totalProjects,
                    totalMembers,
                    myTasks,
                    overdueTasks,
                    byStatus = statusGroups,
                    byPriority = priorityGroups
                }
            });
        }
    }

    public class ReorderTaskDto
    {
        public double SortOrder { get; set; }
        public string? NewStatusName { get; set; }
    }

    public class NotificationActorContext
    {
        public string ActorName { get; set; } = "System";
        public string ProjectName { get; set; } = string.Empty;
        public string TaskTitle { get; set; } = string.Empty;
    }
}

