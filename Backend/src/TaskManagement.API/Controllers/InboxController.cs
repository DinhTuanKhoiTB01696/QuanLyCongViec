using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagement.API.Infrastructure;
using TaskManagement.Application.DTOs.WorkTask;
using TaskManagement.Application.Interfaces;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.API.Controllers
{
    [ApiController]
    [Route("api/inbox")]
    [Authorize]
    public class InboxController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWorkTaskService _workTaskService;
        private readonly IAiIntegrationService _aiIntegrationService;

        public InboxController(
            ApplicationDbContext context,
            IWorkTaskService workTaskService,
            IAiIntegrationService aiIntegrationService)
        {
            _context = context;
            _workTaskService = workTaskService;
            _aiIntegrationService = aiIntegrationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetInbox([FromQuery] string? source)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();
            await IntegrationSchemaGuard.EnsureCreatedAsync(_context);

            var query = _context.InboxItems
                .AsNoTracking()
                .Where(item => item.UserId == userId.Value);

            if (!string.IsNullOrWhiteSpace(source) && source != "all")
            {
                query = query.Where(item => item.Source == source);
            }

            var items = await query
                .OrderByDescending(item => item.StartsAt ?? item.CreatedAt)
                .Take(100)
                .Select(item => new
                {
                    item.Id,
                    item.Source,
                    item.Provider,
                    item.ExternalId,
                    item.Title,
                    item.Content,
                    item.Location,
                    item.StartsAt,
                    item.EndsAt,
                    item.IsRead,
                    item.CreatedTaskId,
                    item.CreatedAt,
                    item.UpdatedAt
                })
                .ToListAsync();

            return Ok(new { statusCode = 200, data = items });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetInboxItem(Guid id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();
            await IntegrationSchemaGuard.EnsureCreatedAsync(_context);

            var item = await _context.InboxItems
                .AsNoTracking()
                .Where(inboxItem => inboxItem.Id == id && inboxItem.UserId == userId.Value)
                .Select(inboxItem => new
                {
                    inboxItem.Id,
                    inboxItem.Source,
                    inboxItem.Provider,
                    inboxItem.ExternalId,
                    inboxItem.Title,
                    inboxItem.Content,
                    inboxItem.Location,
                    inboxItem.StartsAt,
                    inboxItem.EndsAt,
                    inboxItem.IsRead,
                    inboxItem.CreatedTaskId,
                    inboxItem.CreatedAt,
                    inboxItem.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (item == null) return NotFound(new { message = "Không tìm thấy mục inbox" });
            return Ok(new { statusCode = 200, data = item });
        }

        [HttpPatch("{id:guid}/read")]
        public async Task<IActionResult> MarkRead(Guid id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();
            await IntegrationSchemaGuard.EnsureCreatedAsync(_context);

            var item = await _context.InboxItems.FirstOrDefaultAsync(inboxItem => inboxItem.Id == id && inboxItem.UserId == userId.Value);
            if (item == null) return NotFound(new { message = "Không tìm thấy mục inbox" });

            item.IsRead = true;
            item.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { statusCode = 200, data = new { item.Id, item.IsRead } });
        }

        [HttpPost("{id:guid}/create-task")]
        public async Task<IActionResult> CreateTask(Guid id, [FromBody] CreateTaskFromInboxRequest? request)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();
            await IntegrationSchemaGuard.EnsureCreatedAsync(_context);

            var item = await _context.InboxItems.FirstOrDefaultAsync(inboxItem => inboxItem.Id == id && inboxItem.UserId == userId.Value);
            if (item == null) return NotFound(new { message = "Không tìm thấy mục inbox" });

            if (item.CreatedTaskId.HasValue)
            {
                return Conflict(new { message = "Mục inbox này đã tạo task rồi", taskId = item.CreatedTaskId.Value });
            }

            var projectId = request?.ProjectId ?? await ResolveDefaultProjectIdAsync(userId.Value);
            if (projectId == null)
            {
                return BadRequest(new { message = "Không tìm thấy dự án để tạo task. Hãy chọn một project trước." });
            }

            var description = string.Join(Environment.NewLine, new[]
            {
                $"Nguồn tích hợp: {item.Provider} / {item.Source}",
                item.StartsAt.HasValue ? $"Bắt đầu: {item.StartsAt.Value:O}" : null,
                item.EndsAt.HasValue ? $"Kết thúc: {item.EndsAt.Value:O}" : null,
                !string.IsNullOrWhiteSpace(item.Location) ? $"Địa điểm: {item.Location}" : null,
                "",
                "Nội dung gốc:",
                string.IsNullOrWhiteSpace(item.Content) ? "(Không có mô tả)" : item.Content
            }.Where(line => line != null));

            var created = await _workTaskService.CreateAsync(userId.Value, new CreateWorkTaskDto
            {
                ProjectId = projectId.Value,
                Title = item.Title,
                Description = description,
                TypeName = "Task",
                Priority = 3,
                StoryPoints = 0,
                DueDate = item.StartsAt
            });

            item.CreatedTaskId = created.Id;
            item.IsRead = true;
            item.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { statusCode = 200, data = created });
        }

        [HttpGet("{id:guid}/ai/summary")]
        public async Task<IActionResult> Summarize(Guid id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();
            var result = await _aiIntegrationService.SummarizeInboxItemAsync(id, userId.Value);
            return Ok(new { statusCode = 200, data = result });
        }

        [HttpGet("{id:guid}/ai/suggest-task")]
        public async Task<IActionResult> SuggestTask(Guid id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();
            var result = await _aiIntegrationService.SuggestTaskFromInboxItemAsync(id, userId.Value);
            return Ok(new { statusCode = 200, data = result });
        }

        [HttpGet("{id:guid}/ai/suggest-related-task")]
        public async Task<IActionResult> SuggestRelatedTask(Guid id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();
            var result = await _aiIntegrationService.SuggestRelatedTaskAsync(id, userId.Value);
            return Ok(new { statusCode = 200, data = result });
        }

        private async Task<Guid?> ResolveDefaultProjectIdAsync(Guid userId)
        {
            return await _context.ProjectMembers
                .AsNoTracking()
                .Where(member => member.UserId == userId && member.Status && !member.Project.IsDeleted && !member.Project.IsArchived)
                .OrderBy(member => member.Project.Name)
                .Select(member => (Guid?)member.ProjectId)
                .FirstOrDefaultAsync();
        }

        private Guid? GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(claim, out var id) ? id : null;
        }
    }

    public class CreateTaskFromInboxRequest
    {
        public Guid? ProjectId { get; set; }
    }
}
