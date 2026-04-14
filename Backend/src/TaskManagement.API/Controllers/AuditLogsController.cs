using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Domain.Entities;

namespace TaskManagement.API.Controllers
{
    [ApiController]
    [Route("api/auditlogs")]
    [Authorize]
    public class AuditLogsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuditLogsController(ApplicationDbContext context)
        {
            _context = context;
        }

        private class LogItemModel
        {
            public Guid Id { get; set; }
            public DateTime CreatedAt { get; set; }
            public string? FullName { get; set; }
            public string? Email { get; set; }
            public string? OldValue { get; set; }
            public string? NewValue { get; set; }
            public string? FieldChanged { get; set; }
            public string? TaskTitle { get; set; }
            public Guid TaskId { get; set; }
            public string? ProjectName { get; set; }
            public string Type { get; set; } = string.Empty;
            public string? Action { get; set; }
            public string? Status { get; set; }
        }

        [HttpGet]
        public async Task<IActionResult> GetAuditLogs(
            [FromQuery] Guid? projectId, 
            [FromQuery] Guid? taskId, 
            [FromQuery] string? timeFilter, 
            [FromQuery] string? search,
            [FromQuery] int page = 1, [FromQuery] int limit = 100)
        {
            try
            {
                // Admin-only guard
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
                    return Unauthorized(new { statusCode = 401, message = "Unauthorized." });

                var user = await _context.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Id == userId);
                
                var isAdmin = user?.UserRoles?.Any(ur => ur.Role.Name.Equals("Admin", StringComparison.OrdinalIgnoreCase)) ?? false;

                var pmProjectIds = new List<Guid>();
                if (!isAdmin && user != null)
                {
                    pmProjectIds = await _context.ProjectMembers
                        .Where(pm => pm.UserId == userId && (pm.ProjectRole == "PM" || pm.ProjectRole == "PROJECT_MANAGER" || pm.ProjectRole == "PO" || pm.ProjectRole == "Admin"))
                        .Select(pm => pm.ProjectId)
                        .ToListAsync();
                }
                
                if (!isAdmin && !pmProjectIds.Any())
                    return StatusCode(403, new { statusCode = 403, message = "Chỉ Admin hoặc PM mới được phép xem Audit Log." });

                var query = _context.AuditLogs
                    .Include(al => al.User)
                    .Include(al => al.WorkTask)
                    .ThenInclude(t => t.Project)
                    .AsQueryable();

                // 1. Phân quyền dữ liệu (Data Authorization)
                if (!isAdmin)
                {
                    query = query.Where(al => pmProjectIds.Contains(al.WorkTask.ProjectId));
                }

                // 2. Bộ lọc (Filters)
                if (projectId.HasValue)
                {
                    query = query.Where(al => al.WorkTask.ProjectId == projectId.Value);
                }
                if (taskId.HasValue)
                {
                    query = query.Where(al => al.WorkTaskId == taskId.Value);
                }
                if (!string.IsNullOrEmpty(search))
                {
                    var s = search.ToLower();
                    query = query.Where(al => al.WorkTask.Title.ToLower().Contains(s) || 
                                              al.User.FullName.ToLower().Contains(s) || 
                                              al.User.Email.ToLower().Contains(s));
                }
                if (!string.IsNullOrEmpty(timeFilter))
                {
                    var now = DateTime.UtcNow;
                    if (timeFilter == "24h")
                        query = query.Where(al => al.CreatedAt >= now.AddHours(-24));
                    else if (timeFilter == "30d")
                        query = query.Where(al => al.CreatedAt >= now.AddDays(-30));
                }

                var rawTaskLogs = await query
                    .OrderByDescending(al => al.CreatedAt)
                    .Take(limit * page)
                    .Select(al => new LogItemModel {
                        Id = al.Id,
                        CreatedAt = al.CreatedAt,
                        FullName = al.User != null ? al.User.FullName : "Unknown",
                        Email = al.User != null ? al.User.Email : "Unknown",
                        OldValue = al.OldValue,
                        NewValue = al.NewValue,
                        FieldChanged = al.FieldChanged,
                        TaskTitle = al.WorkTask != null ? al.WorkTask.Title : "Deleted Task",
                        TaskId = al.WorkTaskId,
                        ProjectName = (al.WorkTask != null && al.WorkTask.Project != null) ? al.WorkTask.Project.Name : "System",
                        Type = "Task"
                    })
                    .ToListAsync();

                var allRawLogs = new List<LogItemModel>(rawTaskLogs);
                var totalTasks = await query.CountAsync();
                int totalSystemLogs = 0;

                if (isAdmin && !projectId.HasValue && !taskId.HasValue) 
                {
                    var sysQuery = _context.SystemAuditLogs.Include(s => s.User).AsQueryable();
                    if (!string.IsNullOrEmpty(search)) {
                        var s = search.ToLower();
                        sysQuery = sysQuery.Where(al => al.Action.ToLower().Contains(s) || al.Resource.ToLower().Contains(s) || (al.User != null && al.User.Email.ToLower().Contains(s)));
                    }
                    if (!string.IsNullOrEmpty(timeFilter)) {
                        var now = DateTime.UtcNow;
                        if (timeFilter == "24h") sysQuery = sysQuery.Where(al => al.CreatedAt >= now.AddHours(-24));
                        else if (timeFilter == "30d") sysQuery = sysQuery.Where(al => al.CreatedAt >= now.AddDays(-30));
                    }
                    
                    totalSystemLogs = await sysQuery.CountAsync();
                    var rawSysLogs = await sysQuery
                        .OrderByDescending(al => al.CreatedAt)
                        .Take(limit * page)
                        .Select(al => new LogItemModel {
                            Id = al.Id,
                            CreatedAt = al.CreatedAt,
                            FullName = al.User != null ? al.User.FullName : "System",
                            Email = al.User != null ? al.User.Email : "System",
                            OldValue = "",
                            NewValue = "",
                            FieldChanged = al.Details ?? al.Action,
                            TaskTitle = al.Resource,
                            TaskId = Guid.Empty,
                            ProjectName = "System",
                            Type = "System",
                            Action = al.Action,
                            Status = al.Status
                        })
                        .ToListAsync();
                    allRawLogs.AddRange(rawSysLogs);
                }

                // Sort and Paginate
                var skip = (page - 1) * limit;
                var sortedRawLogs = allRawLogs.OrderByDescending(l => l.CreatedAt)
                                              .Skip(skip)
                                              .Take(limit)
                                              .ToList();

                var logs = sortedRawLogs.Select(al => {
                    if (al.Type == "Task") {
                        return new {
                            id = "LOG-" + al.Id.ToString().Substring(0, 8).ToUpper(),
                            timestamp = al.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                            user = (string?)(al.FullName ?? al.Email),
                            action = string.IsNullOrEmpty(al.OldValue) && al.NewValue != null ? "create" : "update",
                            resource = (string?)($"[{al.ProjectName}] {al.TaskTitle}"),
                            targetId = !string.IsNullOrEmpty(al.TaskTitle) && al.TaskTitle.Length > 10 ? "TASK-" + al.TaskTitle.Substring(0, 10) : "TASK-" + al.TaskTitle,
                            projectName = al.ProjectName,
                            status = "success",
                            summary = (string?)($"Thay đổi trường '{al.FieldChanged}'")
                        };
                    } else {
                        return new {
                            id = "SYS-" + al.Id.ToString().Substring(0, 8).ToUpper(),
                            timestamp = al.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                            user = (string?)(al.FullName ?? al.Email ?? "System"),
                            action = al.Action ?? "system",
                            resource = (string?)al.TaskTitle,
                            targetId = "SYSTEM",
                            projectName = al.ProjectName,
                            status = al.Status ?? "success",
                            summary = (string?)al.FieldChanged
                        };
                    }
                }).ToList();

                int totalCount = totalTasks + totalSystemLogs;

                return Ok(new { statusCode = 200, message = "Success", data = new { items = logs, total = totalCount } });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { statusCode = 500, message = ex.Message });
            }
        }
    }
}
