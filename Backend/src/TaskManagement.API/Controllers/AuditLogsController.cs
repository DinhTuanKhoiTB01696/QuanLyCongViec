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

        [HttpGet]
        public async Task<IActionResult> GetAuditLogs([FromQuery] int page = 1, [FromQuery] int limit = 100)
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
                
                var isAdmin = user?.UserRoles?.Any(ur => ur.Role.Name == "Admin") ?? false;
                if (!isAdmin)
                    return StatusCode(403, new { statusCode = 403, message = "Chỉ Admin mới được phép xem Audit Log." });

                var skip = (page - 1) * limit;

                var query = _context.AuditLogs
                    .Include(al => al.User)
                    .Include(al => al.WorkTask)
                    .OrderByDescending(al => al.CreatedAt)
                    .AsQueryable();

                var total = await query.CountAsync();
                
                var rawLogs = await query
                    .Skip(skip)
                    .Take(limit)
                    .Select(al => new {
                        Id = al.Id,
                        CreatedAt = al.CreatedAt,
                        FullName = al.User.FullName,
                        Email = al.User.Email,
                        OldValue = al.OldValue,
                        NewValue = al.NewValue,
                        FieldChanged = al.FieldChanged,
                        TaskTitle = al.WorkTask.Title,
                        TaskId = al.WorkTaskId
                    })
                    .ToListAsync();
                    
                var logs = rawLogs.Select(al => new {
                        id = "LOG-" + al.Id.ToString().Substring(0, 8).ToUpper(),
                        timestamp = al.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                        user = al.FullName ?? al.Email,
                        action = string.IsNullOrEmpty(al.OldValue) && al.NewValue != null ? "create" : "update",
                        resource = "Task",
                        targetId = al.TaskTitle.Length > 10 ? "TASK-" + al.TaskTitle.Substring(0, 10) : "TASK-" + al.TaskTitle,
                        status = "success",
                        summary = $"Thay đổi trường '{al.FieldChanged}'",
                        details = new {
                            field = al.FieldChanged,
                            oldValue = al.OldValue,
                            newValue = al.NewValue,
                            taskId = al.TaskId
                        }
                    }).ToList();

                return Ok(new { statusCode = 200, message = "Success", data = new { items = logs, total } });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { statusCode = 500, message = ex.Message });
            }
        }
    }
}
