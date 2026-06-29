using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure.Services
{
    public class StarredItemService : IStarredItemService
    {
        private readonly ApplicationDbContext _context;

        public StarredItemService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<object> GetAllAsync(Guid userId, Guid workspaceId)
        {
            var items = await _context.StarredItems
                .AsNoTracking()
                .Where(s => s.UserId == userId && s.WorkspaceId == workspaceId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            var goalIds = items.Where(i => i.ItemType == "Goal").Select(i => i.ItemId).ToList();
            var projectIds = items.Where(i => i.ItemType == "Project").Select(i => i.ItemId).ToList();
            var teamIds = items.Where(i => i.ItemType == "Team").Select(i => i.ItemId).ToList();
            var userIds = items.Where(i => i.ItemType == "User").Select(i => i.ItemId).ToList();

            var goalNames = await _context.Goals
                .Where(g => goalIds.Contains(g.Id))
                .ToDictionaryAsync(g => g.Id, g => g.Title);
            var projectNames = await _context.Projects
                .Where(p => projectIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.Name);
            var teamNames = await _context.Departments
                .Where(d => teamIds.Contains(d.Id))
                .ToDictionaryAsync(d => d.Id, d => d.Name);
            var userNames = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.FullName ?? u.Email);

            return items.Select(item => new
            {
                item.Id,
                item.UserId,
                item.WorkspaceId,
                item.ItemType,
                item.ItemId,
                item.CreatedAt,
                itemName = item.ItemType switch
                {
                    "Goal" => goalNames.GetValueOrDefault(item.ItemId),
                    "Project" => projectNames.GetValueOrDefault(item.ItemId),
                    "Team" => teamNames.GetValueOrDefault(item.ItemId),
                    "User" => userNames.GetValueOrDefault(item.ItemId),
                    _ => null
                }
            }).ToList();
        }

        public async Task<object> ToggleStarAsync(Guid userId, Guid workspaceId, string itemType, Guid itemId)
        {
            var existing = await _context.StarredItems
                .FirstOrDefaultAsync(s => s.UserId == userId && s.WorkspaceId == workspaceId && s.ItemType == itemType && s.ItemId == itemId);

            if (existing != null)
            {
                _context.StarredItems.Remove(existing);
                _context.SiteAuditLogs.Add(new SiteAuditLog
                {
                    EntityId = itemId,
                    EntityType = itemType,
                    Action = "Unstar",
                    UserId = userId,
                    OldValue = itemId.ToString(),
                    CreatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
                return new { status = "unstarred" };
            }
            else
            {
                var starredItem = new StarredItem
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    WorkspaceId = workspaceId,
                    ItemType = itemType,
                    ItemId = itemId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.StarredItems.Add(starredItem);
                _context.SiteAuditLogs.Add(new SiteAuditLog
                {
                    EntityId = itemId,
                    EntityType = itemType,
                    Action = "Star",
                    UserId = userId,
                    NewValue = itemId.ToString(),
                    CreatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
                return new { status = "starred", data = starredItem };
            }
        }
    }
}
