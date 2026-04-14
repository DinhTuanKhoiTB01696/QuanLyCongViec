using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TaskManagement.API.Hubs;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.API.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize]
    public class CommentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificationHub> _notificationHub;
        private readonly IWebHostEnvironment _env;

        public CommentsController(ApplicationDbContext context, IHubContext<NotificationHub> notificationHub, IWebHostEnvironment env)
        {
            _context = context;
            _notificationHub = notificationHub;
            _env = env;
        }

        private Guid? GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(claim, out var id) ? id : null;
        }

        /// <summary>
        /// GET /api/projects/{projectId}/WorkTasks/{taskId}/comments
        /// </summary>
        [HttpGet("projects/{projectId}/WorkTasks/{taskId}/comments")]
        public async Task<IActionResult> GetComments(Guid projectId, Guid taskId)
        {
            var comments = await _context.Comments
                .Where(c => c.WorkTaskId == taskId && !c.IsDeleted)
                .Include(c => c.User)
                .Include(c => c.CommentAttachments)
                .OrderBy(c => c.CreatedAt)
                .Select(c => new
                {
                    c.Id,
                    c.Content,
                    c.ParentCommentId,
                    c.CreatedAt,
                    c.UpdatedAt,
                    UserId = c.UserId,
                    FullName = c.User.FullName ?? c.User.Email,
                    AvatarUrl = c.User.AvatarUrl,
                    Attachments = c.CommentAttachments.Select(a => new
                    {
                        a.Id,
                        a.FileName,
                        a.FileUrl,
                        a.ContentType,
                        a.FileSize,
                        a.CreatedAt
                    }).ToList()
                })
                .ToListAsync();

            return Ok(new { statusCode = 200, message = "Success", data = comments });
        }

        /// <summary>
        /// POST /api/projects/{projectId}/WorkTasks/{taskId}/comments
        /// Supports multipart form: content (text) + files[] (attachments)
        /// </summary>
        [HttpPost("projects/{projectId}/WorkTasks/{taskId}/comments")]
        public async Task<IActionResult> CreateComment(Guid projectId, Guid taskId, [FromForm] string content, [FromForm] List<IFormFile>? files)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            if (string.IsNullOrWhiteSpace(content) && (files == null || files.Count == 0))
                return BadRequest(new { message = "Comment phải có nội dung hoặc file đính kèm." });

            var task = await _context.WorkTasks.FirstOrDefaultAsync(t => t.Id == taskId && !t.IsDeleted);
            if (task == null) return NotFound(new { message = "Task không tồn tại." });

            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                WorkTaskId = taskId,
                UserId = userId.Value,
                Content = content ?? "",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Comments.Add(comment);

            // Process file attachments
            if (files != null && files.Count > 0)
            {
                var uploadsDir = Path.Combine(_env.ContentRootPath, "uploads", "comments");
                if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);

                foreach (var file in files)
                {
                    if (file.Length > 10 * 1024 * 1024) // 10MB limit
                        continue;

                    var ext = Path.GetExtension(file.FileName);
                    var uniqueName = $"{Guid.NewGuid()}{ext}";
                    var filePath = Path.Combine(uploadsDir, uniqueName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    _context.CommentAttachments.Add(new CommentAttachment
                    {
                        Id = Guid.NewGuid(),
                        CommentId = comment.Id,
                        UploadedByUserId = userId.Value,
                        FileName = file.FileName,
                        FileUrl = $"/uploads/comments/{uniqueName}",
                        ContentType = file.ContentType,
                        FileSize = file.Length,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            await _context.SaveChangesAsync();

            // Create notification for task assignee/reporter
            var currentUser = await _context.Users.FindAsync(userId.Value);
            var notifyUserIds = new HashSet<Guid>();
            if (task.AssignedUserId.HasValue && task.AssignedUserId.Value != userId.Value)
                notifyUserIds.Add(task.AssignedUserId.Value);
            if (task.ReporterId != userId.Value)
                notifyUserIds.Add(task.ReporterId);

            foreach (var notifyId in notifyUserIds)
            {
                var notification = new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = notifyId,
                    Title = "Bình luận mới",
                    Content = $"{currentUser?.FullName ?? "Ai đó"} đã bình luận trên \"{task.Title}\"",
                    NotificationType = "COMMENT_ADDED",
                    RelatedTaskId = taskId,
                    RelatedProjectId = projectId,
                    TriggeredByUserId = userId.Value,
                    LinkUrl = $"/projects/{projectId}/tasks/{taskId}",
                    CreatedAt = DateTime.UtcNow
                };
                _context.Notifications.Add(notification);

                // Push via SignalR
                await _notificationHub.Clients.Group($"user_{notifyId}")
                    .SendAsync("ReceiveNotification", new
                    {
                        notification.Id,
                        notification.Title,
                        notification.Content,
                        notification.NotificationType,
                        notification.RelatedTaskId,
                        notification.CreatedAt,
                        TriggeredByName = currentUser?.FullName
                    });
            }
            await _context.SaveChangesAsync();

            // Return the created comment with attachments
            var result = await _context.Comments
                .Where(c => c.Id == comment.Id)
                .Include(c => c.User)
                .Include(c => c.CommentAttachments)
                .Select(c => new
                {
                    c.Id,
                    c.Content,
                    c.CreatedAt,
                    FullName = c.User.FullName ?? c.User.Email,
                    AvatarUrl = c.User.AvatarUrl,
                    Attachments = c.CommentAttachments.Select(a => new
                    {
                        a.Id, a.FileName, a.FileUrl, a.ContentType, a.FileSize
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            return Ok(new { statusCode = 201, message = "Đã thêm bình luận.", data = result });
        }

        /// <summary>
        /// DELETE /api/comments/{commentId}
        /// </summary>
        [HttpDelete("comments/{commentId}")]
        public async Task<IActionResult> DeleteComment(Guid commentId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null) return NotFound();
            if (comment.UserId != userId.Value) return Forbid();

            comment.IsDeleted = true;
            comment.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { statusCode = 200, message = "Đã xóa bình luận." });
        }

        /// <summary>
        /// POST /api/upload  — Generic file upload for attachments, avatars, etc.
        /// </summary>
        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] IFormFile file, [FromForm] string? folder)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();
            if (file == null || file.Length == 0) return BadRequest(new { message = "No file." });
            if (file.Length > 10 * 1024 * 1024) return BadRequest(new { message = "File quá lớn (max 10MB)." });

            var targetFolder = folder ?? "general";
            var uploadsDir = Path.Combine(_env.ContentRootPath, "uploads", targetFolder);
            if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);

            var ext = Path.GetExtension(file.FileName);
            var uniqueName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsDir, uniqueName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var fileUrl = $"/uploads/{targetFolder}/{uniqueName}";
            return Ok(new { statusCode = 200, data = new { fileUrl, fileName = file.FileName, contentType = file.ContentType, fileSize = file.Length } });
        }
    }
}
