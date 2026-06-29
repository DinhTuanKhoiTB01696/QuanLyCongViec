using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.API.Controllers
{
    [ApiController]
    [Route("api/status-dashboard")]
    [Authorize]
    public class StatusDashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StatusDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        private Guid? CurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(claim, out var id) ? id : null;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string type = "projects", [FromQuery] int days = 30)
        {
            var userId = CurrentUserId();
            if (!userId.HasValue) return Unauthorized();

            var from = DateTime.UtcNow.AddDays(-Math.Clamp(days, 1, 365));
            var normalizedType = type.Equals("goals", StringComparison.OrdinalIgnoreCase) ? "goals" : "projects";

            if (normalizedType == "goals")
            {
                var goals = await _context.Goals
                    .AsNoTracking()
                    .Include(g => g.Owner)
                    .Where(g => g.UpdatedAt >= from || g.CreatedAt >= from)
                    .OrderByDescending(g => g.UpdatedAt)
                    .Select(g => new
                    {
                        g.Id,
                        Name = g.Title,
                        Title = g.Title,
                        g.Status,
                        g.Progress,
                        OwnerId = g.OwnerId,
                        OwnerName = g.Owner.FullName ?? g.Owner.Email,
                        OwnerAvatarUrl = g.Owner.AvatarUrl,
                        g.CreatedAt,
                        g.UpdatedAt,
                        g.IsArchived
                    })
                    .ToListAsync();

                var items = goals.Select(g => new DashboardItem(
                    g.Id,
                    g.Name,
                    g.Title,
                    g.Status,
                    g.Progress,
                    g.OwnerId,
                    g.OwnerName,
                    g.OwnerAvatarUrl,
                    g.CreatedAt,
                    g.UpdatedAt,
                    g.IsArchived
                )).ToList();

                return Ok(new { statusCode = 200, data = BuildResponse(items, "Goal") });
            }

            var projectIds = await _context.ProjectMembers
                .Where(pm => pm.UserId == userId.Value && pm.Status)
                .Select(pm => pm.ProjectId)
                .ToListAsync();

            var projects = await _context.Projects
                .AsNoTracking()
                .Include(p => p.Creator)
                .Where(p => (projectIds.Contains(p.Id) || p.NetworkType == "Public") && (p.UpdatedAt >= from || p.CreatedAt >= from))
                .OrderByDescending(p => p.UpdatedAt)
                .Select(p => new
                {
                    p.Id,
                    Name = p.Name,
                    Title = p.Name,
                    Status = p.Updates
                        .OrderByDescending(update => update.CreatedAt)
                        .Select(update => update.NewStatus ?? update.Status)
                        .FirstOrDefault() ?? (p.Status ? "Active" : "Archived"),
                    Progress = p.IsArchived ? 100 : 0,
                    OwnerId = p.CreatorId,
                    OwnerName = p.Creator.FullName ?? p.Creator.Email,
                    OwnerAvatarUrl = p.Creator.AvatarUrl,
                    p.CreatedAt,
                    p.UpdatedAt,
                    p.IsArchived
                })
                .ToListAsync();

            var projectItems = projects.Select(p => new DashboardItem(
                p.Id,
                p.Name,
                p.Title,
                p.Status,
                p.Progress,
                p.OwnerId,
                p.OwnerName,
                p.OwnerAvatarUrl,
                p.CreatedAt,
                p.UpdatedAt,
                p.IsArchived
            )).ToList();

            return Ok(new { statusCode = 200, data = BuildResponse(projectItems, "Project") });
        }

        private static object BuildResponse(IReadOnlyCollection<DashboardItem> items, string entityType)
        {
            var stats = new
            {
                total = items.Count,
                newest = items.Count(i => i.CreatedAt >= DateTime.UtcNow.AddDays(-7)),
                updated = items.Count(i => i.UpdatedAt >= DateTime.UtcNow.AddDays(-7)),
                done = items.Count(i => i.IsArchived ||
                    i.Status.Contains("Done", StringComparison.OrdinalIgnoreCase) ||
                    i.Status.Contains("Completed", StringComparison.OrdinalIgnoreCase) ||
                    i.Status.Contains("Hoan thanh", StringComparison.OrdinalIgnoreCase)),
                risk = items.Count(i => i.Status.Contains("Risk", StringComparison.OrdinalIgnoreCase) ||
                    i.Status.Contains("Rui ro", StringComparison.OrdinalIgnoreCase)),
                pending = items.Count(i => string.IsNullOrWhiteSpace(i.Status) ||
                    i.Status.Contains("Pending", StringComparison.OrdinalIgnoreCase) ||
                    i.Status.Contains("Dang cho", StringComparison.OrdinalIgnoreCase))
            };

            return new { entityType, stats, items };
        }

        private sealed record DashboardItem(
            Guid Id,
            string Name,
            string Title,
            string Status,
            int Progress,
            Guid OwnerId,
            string OwnerName,
            string? OwnerAvatarUrl,
            DateTime CreatedAt,
            DateTime UpdatedAt,
            bool IsArchived);
    }
}
