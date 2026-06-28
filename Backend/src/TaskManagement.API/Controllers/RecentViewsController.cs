using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.API.Controllers
{
    [ApiController]
    [Route("api/recentviews")]
    [Authorize]
    public class RecentViewsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RecentViewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        private Guid? CurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(claim, out var id) ? id : null;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int limit = 50)
        {
            var userId = CurrentUserId();
            if (!userId.HasValue) return Unauthorized();

            var items = await _context.RecentViews
                .AsNoTracking()
                .Where(rv => rv.UserId == userId.Value)
                .OrderByDescending(rv => rv.ViewedAt)
                .Take(Math.Clamp(limit, 1, 100))
                .Select(rv => new
                {
                    rv.Id,
                    rv.EntityType,
                    rv.EntityId,
                    rv.Title,
                    rv.Subtitle,
                    rv.Url,
                    rv.Icon,
                    rv.ViewedAt
                })
                .ToListAsync();

            return Ok(new { statusCode = 200, data = items });
        }

        [HttpPost]
        public async Task<IActionResult> Upsert([FromBody] RecentViewRequest request)
        {
            var userId = CurrentUserId();
            if (!userId.HasValue) return Unauthorized();
            if (request.EntityId == Guid.Empty || string.IsNullOrWhiteSpace(request.EntityType) || string.IsNullOrWhiteSpace(request.Title))
                return BadRequest(new { statusCode = 400, message = "Invalid recent view payload." });

            var entityType = request.EntityType.Trim();
            var existing = await _context.RecentViews.FirstOrDefaultAsync(rv =>
                rv.UserId == userId.Value &&
                rv.EntityType == entityType &&
                rv.EntityId == request.EntityId);

            if (existing == null)
            {
                existing = new RecentView
                {
                    UserId = userId.Value,
                    EntityType = entityType,
                    EntityId = request.EntityId
                };
                _context.RecentViews.Add(existing);
            }

            existing.Title = request.Title.Trim();
            existing.Subtitle = request.Subtitle;
            existing.Url = request.Url;
            existing.Icon = request.Icon;
            existing.ViewedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { statusCode = 200, data = new { existing.Id } });
        }
    }

    public class RecentViewRequest
    {
        public string EntityType { get; set; } = string.Empty;
        public Guid EntityId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Subtitle { get; set; }
        public string? Url { get; set; }
        public string? Icon { get; set; }
    }
}
