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

        private static string NormalizeStatusName(string? statusName)
        {
            if (string.IsNullOrWhiteSpace(statusName))
            {
                return "BACKLOG";
            }

            var upper = statusName.ToUpperInvariant().Replace(" ", string.Empty);
            if (upper.Contains("CANCEL"))
            {
                return "CANCELLED";
            }

            if (upper.Contains("DONE") || upper.Contains("COMPLETE"))
            {
                return "DONE";
            }

            if (upper.Contains("INREVIEW") || upper.Contains("REVIEW"))
            {
                return "IN REVIEW";
            }

            if (upper.Contains("INPROGRESS") || upper.Contains("ACTIVE"))
            {
                return "IN PROGRESS";
            }

            if (upper.Contains("TODO"))
            {
                return "TO DO";
            }

            return "BACKLOG";
        }

        private static readonly (string Name, int Position)[] DefaultTaskStatuses =
        {
            ("BACKLOG", 0),
            ("TO DO", 1),
            ("IN PROGRESS", 2),
            ("IN REVIEW", 3),
            ("DONE", 4),
            ("CANCELLED", 5)
        };

        private static async Task EnsureDefaultTaskStatusesAsync(ApplicationDbContext context, Guid projectId)
        {
            var existingStatuses = await context.TaskStatuses
                .Where(ts => ts.ProjectId == projectId)
                .ToListAsync();

            var existingNormalized = existingStatuses
                .Select(ts => NormalizeStatusName(ts.Name))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var missingStatuses = DefaultTaskStatuses
                .Where(status => !existingNormalized.Contains(status.Name))
                .ToList();

            if (!missingStatuses.Any())
            {
                return;
            }

            foreach (var status in missingStatuses)
            {
                context.TaskStatuses.Add(new TaskManagement.Domain.Entities.TaskStatus
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projectId,
                    Name = status.Name,
                    Position = status.Position
                });
            }

            await context.SaveChangesAsync();
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
        public async Task<IActionResult> SearchTasks([FromQuery] string? query, [FromQuery] string? status, [FromQuery] Guid? assigneeId, [FromQuery] int? priority, [FromQuery] Guid? projectId, [FromQuery] string? scope = "all")
        {
            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdString, out Guid userId))
                {
                    return Unauthorized(new { statusCode = 401, message = "Token không hợp lệ." });
                }

                var tasks = await _workTaskService.SearchTasksAsync(userId, query, status, assigneeId, priority, projectId, scope);
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
                await EnsureDefaultTaskStatusesAsync(context, projectId);

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

        [HttpPost("projects/{projectId}/WorkTasks/{id}/subscription")]
        public async Task<IActionResult> ToggleSubscription(Guid projectId, Guid id, [FromServices] ApplicationDbContext context)
        {
            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdString, out Guid userId))
                {
                    return Unauthorized(new { statusCode = 401, message = "Vui long dang nhap." });
                }

                var taskExistsInProject = await context.WorkTasks
                    .AnyAsync(wt => wt.Id == id && wt.ProjectId == projectId && !wt.IsDeleted);
                if (!taskExistsInProject)
                {
                    return NotFound(new { statusCode = 404, message = "Task khong ton tai trong du an nay." });
                }

                var isProjectMember = await context.ProjectMembers
                    .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == userId && pm.Status);
                if (!isProjectMember)
                {
                    return StatusCode(403, new { statusCode = 403, message = "Ban khong phai thanh vien cua du an nay." });
                }

                var isSubscribed = await _workTaskService.ToggleSubscriptionAsync(id, userId);
                return Ok(new
                {
                    statusCode = 200,
                    message = isSubscribed ? "Da theo doi cong viec." : "Da huy theo doi cong viec.",
                    data = new { isSubscribed }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { statusCode = 500, message = "Loi may chu noi bo: " + ex.Message });
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
                    await EnsureDefaultTaskStatusesAsync(context, projectId);
                    var normalizedStatusName = NormalizeStatusName(statusName);
                    var projectStatuses = await context.TaskStatuses
                        .Where(ts => ts.ProjectId == projectId)
                        .ToListAsync();
                    var newStatus = projectStatuses.FirstOrDefault(ts =>
                        string.Equals(NormalizeStatusName(ts.Name), normalizedStatusName, StringComparison.OrdinalIgnoreCase));
                    if (newStatus != null)
                    {
                        var isDoneStatus = newStatus.Name.ToUpperInvariant().Contains("DONE") ||
                                           newStatus.Name.ToUpperInvariant().Contains("COMPLETE") ||
                                           newStatus.Name.ToUpperInvariant().Contains("HOÀN THÀNH") ||
                                           newStatus.Name.ToUpperInvariant().Contains("THÀNH CÔNG");

                        if (isDoneStatus && task.TaskAssignments.Any(ta => ta.Status && ta.ProgressPercent < 100))
                        {
                            var pendingUserNames = task.TaskAssignments
                                .Where(ta => ta.Status && ta.ProgressPercent < 100)
                                .Select(ta => ta.User != null ? ta.User.FullName ?? ta.User.Email : "Unknown")
                                .ToList();
                                
                            return BadRequest(new { 
                                statusCode = 400, 
                                message = $"Không thể hoàn thành tác vụ vì còn người thực hiện chưa đạt 100% tiến độ: {string.Join(", ", pendingUserNames)}." 
                            });
                        }

                        task.TaskStatusId = newStatus.Id;
                        newStatusName = newStatus.Name;
                    }
                    else
                    {
                        return BadRequest(new { statusCode = 400, message = $"Trạng thái '{statusName}' không hợp lệ cho dự án này." });
                    }

                }

                task.UpdatedAt = DateTime.UtcNow;
                await context.SaveChangesAsync();
                if (userId != Guid.Empty && !string.IsNullOrWhiteSpace(newStatusName))
                {
                    await gamificationService.ApplyStatusChangeRewardsAsync(id, userId, oldStatusName, newStatusName);
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
                    await EnsureDefaultTaskStatusesAsync(context, projectId);
                    var normalizedStatusName = NormalizeStatusName(dto.NewStatusName);
                    var projectStatuses = await context.TaskStatuses
                        .Where(ts => ts.ProjectId == projectId)
                        .ToListAsync();
                    var newStatus = projectStatuses.FirstOrDefault(ts =>
                        string.Equals(NormalizeStatusName(ts.Name), normalizedStatusName, StringComparison.OrdinalIgnoreCase));
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
            await EnsureDefaultTaskStatusesAsync(context, projectId);

            var subtasks = await context.WorkTasks
                .Where(wt => wt.ProjectId == projectId && wt.ParentTaskId == parentId && !wt.IsDeleted)
                .Include(wt => wt.TaskStatus)
                .Include(wt => wt.AssignedUser)
                .Include(wt => wt.TaskAssignments)
                    .ThenInclude(ta => ta.User)
                .OrderBy(wt => wt.SortOrder)
                .Select(wt => new
                {
                    wt.Id,
                    wt.ProjectId,
                    wt.ParentTaskId,
                    wt.Title,
                    wt.Description,
                    wt.Priority,
                    wt.SortOrder,
                    wt.SequenceId,
                    wt.TaskTypeId,
                    StatusName = wt.TaskStatus != null ? NormalizeStatusName(wt.TaskStatus.Name) : "BACKLOG",
                    wt.AssignedUserId,
                    AssigneeIds = wt.TaskAssignments
                        .Where(ta => ta.Status)
                        .Select(ta => ta.UserId)
                        .ToList(),
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
                    AssigneeName = wt.AssignedUser != null ? (wt.AssignedUser.FullName ?? wt.AssignedUser.Email) : null,
                    AssigneeAvatar = wt.AssignedUser != null ? wt.AssignedUser.AvatarUrl : null,
                    wt.PlannedStartDate,
                    wt.PlannedEndDate,
                    wt.DueDate,
                    wt.CreatedAt,
                    wt.UpdatedAt
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

            var parent = await context.WorkTasks.FirstOrDefaultAsync(t => t.Id == parentId && t.ProjectId == projectId && !t.IsDeleted);
            if (parent == null) return NotFound(new { message = "Task cha không tồn tại." });

            await EnsureDefaultTaskStatusesAsync(context, projectId);

            // Get default BACKLOG status and type from project
            var projectStatuses = await context.TaskStatuses
                .Where(ts => ts.ProjectId == projectId)
                .OrderBy(ts => ts.Position)
                .ToListAsync();
            var defaultStatus = projectStatuses.FirstOrDefault(ts => NormalizeStatusName(ts.Name) == "BACKLOG")
                ?? projectStatuses.FirstOrDefault();
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
                    subtask.ProjectId,
                    subtask.Title,
                    subtask.Description,
                    subtask.Priority,
                    subtask.ParentTaskId,
                    subtask.SequenceId,
                    subtask.TaskTypeId,
                    subtask.AssignedUserId,
                    AssigneeIds = new List<Guid>(),
                    Assignees = new List<TaskAssigneeDto>(),
                    StatusName = NormalizeStatusName(defaultStatus.Name),
                    subtask.PlannedStartDate,
                    subtask.PlannedEndDate,
                    subtask.DueDate,
                    subtask.CreatedAt,
                    subtask.UpdatedAt
                }
            });
        }

        /// <summary>
        /// GET /api/dashboard/stats — Real-time dashboard statistics from DB
        /// </summary>
        [HttpGet("/api/dashboard/stats")]
        public async Task<IActionResult> GetDashboardStats(
            [FromServices] ApplicationDbContext context,
            [FromQuery] string scope = "all",
            [FromQuery] Guid? projectId = null)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdStr, out Guid userId))
            {
                return Unauthorized();
            }

            // Lấy danh sách Project mà user tham gia hoặc có quyền xem dựa trên scope
            IQueryable<TaskManagement.Domain.Entities.Project> projectQuery = context.Projects.Where(p => !p.IsDeleted);

            if (scope == "my")
            {
                // My projects: User là creator hoặc Member hoạt động
                projectQuery = projectQuery.Where(p => p.CreatorId == userId || p.ProjectMembers.Any(pm => pm.UserId == userId && pm.Status));
            }
            else if (scope == "archived")
            {
                // Archived projects
                projectQuery = projectQuery.Where(p => p.IsArchived);
            }
            else
            {
                // All active projects: User is creator OR Member hoạt động
                projectQuery = projectQuery.Where(p => !p.IsArchived && (p.CreatorId == userId || p.ProjectMembers.Any(pm => pm.UserId == userId && pm.Status)));
            }

                        if (projectId.HasValue)
            {
                projectQuery = projectQuery.Where(p => p.Id == projectId.Value);
            }

            var targetProjectIds = await projectQuery.Select(p => p.Id).ToListAsync();

            var baseTaskQuery = context.WorkTasks
                .Include(t => t.TaskStatus)
                .Where(t => !t.IsDeleted && targetProjectIds.Contains(t.ProjectId));

            var totalTasks = await baseTaskQuery.CountAsync();
            var doneStatuses = new[] { "DONE", "COMPLETED", "FINISHED", "HOÀN THÀNH", "SUCCESS", "FINISHED", "HOÀN TẤT" };

            var completedTasks = await baseTaskQuery.CountAsync(t =>
                t.TaskStatus != null && doneStatuses.Contains(t.TaskStatus.Name.ToUpper().Trim()));

            var now = DateTime.UtcNow;
            var overdueTasks = await baseTaskQuery.CountAsync(t =>
                t.DueDate.HasValue && t.DueDate < now &&
                (t.TaskStatus == null || !doneStatuses.Contains(t.TaskStatus.Name.ToUpper().Trim())));

            var statusGroups = await baseTaskQuery
                .GroupBy(t => t.TaskStatus != null ? t.TaskStatus.Name : "No Status")
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var priorityGroups = await baseTaskQuery
                .GroupBy(t => t.Priority)
                .Select(g => new { Priority = g.Key, Count = g.Count() })
                .ToListAsync();

            var myTasks = await baseTaskQuery.CountAsync(t => 
                t.AssignedUserId == userId || 
                t.TaskAssignments.Any(ta => ta.UserId == userId));

            var totalProjects = targetProjectIds.Count;
            var totalMembers = await context.ProjectMembers
                .Where(pm => targetProjectIds.Contains(pm.ProjectId) && pm.Status)
                .Select(pm => pm.UserId)
                .Distinct()
                .CountAsync();

            var activeCycles = await context.Sprints.CountAsync(s => targetProjectIds.Contains(s.ProjectId) && s.Status);
            var totalModules = await context.Modules.CountAsync(m => targetProjectIds.Contains(m.ProjectId));

            var totalIntakes = await context.Intakes.CountAsync(i => targetProjectIds.Contains(i.ProjectId));
            var totalViews = await context.ProjectViews.CountAsync(v => targetProjectIds.Contains(v.ProjectId));

            var sevenDaysAgo = now.AddDays(-7);
            var newTasksLast7Days = await baseTaskQuery.CountAsync(t => t.CreatedAt >= sevenDaysAgo);

            var thirtyDaysAgo = now.AddDays(-30);
            // Include both Task logs and System logs that belong to these projects if possible
            // For now, task-related logs are the primary activity
            var totalActions = await context.AuditLogs
                .Where(al => al.CreatedAt >= thirtyDaysAgo && al.WorkTask != null && targetProjectIds.Contains(al.WorkTask.ProjectId))
                .CountAsync();


            return Ok(new
            {
                statusCode = 200,
                data = new
                {
                    totalTasks,
                    completedTasks,
                    overdueTasks,
                    totalProjects,
                    totalMembers,
                    activeCycles,
                    totalModules,
                    totalIntakes,
                    totalViews,
                    newTasksLast7Days,
                    totalActions,
                    myTasks,
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

