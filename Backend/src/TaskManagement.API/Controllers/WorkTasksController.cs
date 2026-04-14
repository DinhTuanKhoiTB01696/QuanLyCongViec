using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
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

        public WorkTasksController(IWorkTaskService workTaskService)
        {
            _workTaskService = workTaskService;
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
        public async Task<IActionResult> UpdateStatus(Guid projectId, Guid id, [FromBody] UpdateTaskStatusRequestDto request)
        {
            try
            {
                await _workTaskService.UpdateTaskStatusAsync(id, request);
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
        public async Task<IActionResult> Update(Guid projectId, Guid id, [FromBody] UpdateWorkTaskDto dto)
        {
            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdString, out Guid userId))
                {
                    return Unauthorized(new { statusCode = 401, message = "Vui lòng đăng nhập." });
                }

                var result = await _workTaskService.UpdateAsync(id, userId, dto);
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
        public async Task<IActionResult> Reorder(Guid projectId, Guid id, [FromBody] ReorderTaskDto dto, [FromServices] TaskManagement.Infrastructure.Data.ApplicationDbContext context)
        {
            try
            {
                var task = await context.WorkTasks.FirstOrDefaultAsync(wt => wt.Id == id && !wt.IsDeleted);
                if (task == null)
                    return NotFound(new { statusCode = 404, message = "Tác vụ không tồn tại." });

                task.SortOrder = dto.SortOrder;
                task.UpdatedAt = DateTime.UtcNow;

                // If status also changed (dragged to a different column)
                if (!string.IsNullOrEmpty(dto.NewStatusName))
                {
                    var newStatus = await context.TaskStatuses
                        .FirstOrDefaultAsync(ts => ts.ProjectId == projectId && ts.Name == dto.NewStatusName);
                    if (newStatus != null)
                    {
                        task.TaskStatusId = newStatus.Id;
                    }
                }

                await context.SaveChangesAsync();
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
}

