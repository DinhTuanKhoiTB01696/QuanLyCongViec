using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.API.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public NotificationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        private Guid? GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(claim, out var id) ? id : null;
        }

        /// <summary>
        /// GET /api/notifications — List notifications for current user (last 30 days)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetNotifications([FromQuery] bool? unreadOnly)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var query = _context.Notifications
                .Where(n => n.UserId == userId.Value)
                .Where(n => n.CreatedAt >= DateTime.UtcNow.AddDays(-30));

            if (unreadOnly == true)
                query = query.Where(n => !n.IsRead);

            var notifications = await query
                .OrderByDescending(n => n.CreatedAt)
                .Take(50)
                .Select(n => new
                {
                    n.Id,
                    n.Title,
                    n.Content,
                    n.NotificationType,
                    n.LinkUrl,
                    n.IsRead,
                    n.RelatedTaskId,
                    n.RelatedProjectId,
                    n.TriggeredByUserId,
                    TriggeredByName = n.TriggeredByUser != null ? n.TriggeredByUser.FullName : null,
                    TriggeredByAvatar = n.TriggeredByUser != null ? n.TriggeredByUser.AvatarUrl : null,
                    n.CreatedAt
                })
                .ToListAsync();

            return Ok(new { statusCode = 200, data = notifications });
        }

        /// <summary>
        /// GET /api/notifications/unread-count
        /// </summary>
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var count = await _context.Notifications
                .CountAsync(n => n.UserId == userId.Value && !n.IsRead);

            return Ok(new { statusCode = 200, data = count });
        }

        /// <summary>
        /// PUT /api/notifications/{id}/read
        /// </summary>
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId.Value);

            if (notification == null) return NotFound();

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return Ok(new { statusCode = 200, message = "Đã đánh dấu đã đọc." });
        }

        /// <summary>
        /// PUT /api/notifications/read-all
        /// </summary>
        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var unread = await _context.Notifications
                .Where(n => n.UserId == userId.Value && !n.IsRead)
                .ToListAsync();

            foreach (var n in unread) n.IsRead = true;
            await _context.SaveChangesAsync();

            return Ok(new { statusCode = 200, message = "Đã đánh dấu tất cả đã đọc." });
        }
    }
}
