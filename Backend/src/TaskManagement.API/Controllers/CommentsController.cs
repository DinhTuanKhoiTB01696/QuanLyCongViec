using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;

using TaskManagement.API.Filters;

namespace TaskManagement.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/projects/{projectId}/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CommentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ProjectAuthorize("Member,Admin,PM,DEV,Developer,PROJECT_MANAGER")]
        public async Task<IActionResult> AddComment(Guid projectId, [FromBody] string content, [FromQuery] Guid taskId)
        {
            if (string.IsNullOrWhiteSpace(content)) return BadRequest("Comment content cannot be empty.");

            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            // XSS Prevention: Simple sanitization (in production use a library like HtmlSanitizer)
            var sanitizedContent = content.Replace("<", "&lt;").Replace(">", "&gt;");

            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                WorkTaskId = taskId,
                UserId = userId,
                Content = sanitizedContent,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return Ok(comment);
        }
    }
}
