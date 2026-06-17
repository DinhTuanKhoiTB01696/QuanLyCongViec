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
            return await _context.StarredItems
                .Where(s => s.UserId == userId && s.WorkspaceId == workspaceId)
                .ToListAsync();
        }

        public async Task<object> ToggleStarAsync(Guid userId, Guid workspaceId, string itemType, Guid itemId)
        {
            var existing = await _context.StarredItems
                .FirstOrDefaultAsync(s => s.UserId == userId && s.WorkspaceId == workspaceId && s.ItemType == itemType && s.ItemId == itemId);

            if (existing != null)
            {
                _context.StarredItems.Remove(existing);
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
                await _context.SaveChangesAsync();
                return new { status = "starred", data = starredItem };
            }
        }
    }
}
