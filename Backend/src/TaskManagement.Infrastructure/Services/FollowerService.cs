using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure.Services
{
    public class FollowerService : IFollowerService
    {
        private readonly ApplicationDbContext _context;

        public FollowerService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<object>> GetAllFollowedAsync(Guid userId)
        {
            var followers = await _context.EntityFollowers
                .AsNoTracking()
                .Where(f => f.UserId == userId)
                .ToListAsync();

            return followers.Select(f => new
            {
                f.Id,
                f.EntityId,
                f.EntityType,
                f.CreatedAt
            });
        }

        public async Task<IEnumerable<object>> GetFollowersAsync(string entityType, Guid entityId)
        {
            var normalizedType = NormalizeEntityType(entityType);

            var followers = await _context.EntityFollowers
                .AsNoTracking()
                .Include(f => f.User)
                .Where(f => f.EntityType == normalizedType && f.EntityId == entityId)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            return followers.Select(f => new
            {
                f.Id,
                f.UserId,
                Name = f.User.FullName,
                FullName = f.User.FullName,
                f.User.Email,
                AvatarUrl = f.User.AvatarUrl,
                f.EntityType,
                f.EntityId,
                f.CreatedAt
            });
        }

        public async Task<IEnumerable<object>> AddFollowersAsync(Guid actorUserId, string entityType, Guid entityId, IEnumerable<Guid> userIds)
        {
            var normalizedType = NormalizeEntityType(entityType);
            var distinctUserIds = userIds
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList();

            if (distinctUserIds.Count == 0)
            {
                return await GetFollowersAsync(normalizedType, entityId);
            }

            var validUserIds = await _context.Users
                .AsNoTracking()
                .Where(u => distinctUserIds.Contains(u.Id))
                .Select(u => u.Id)
                .ToListAsync();

            var existingUserIds = await _context.EntityFollowers
                .Where(f => f.EntityType == normalizedType && f.EntityId == entityId && validUserIds.Contains(f.UserId))
                .Select(f => f.UserId)
                .ToListAsync();

            var now = DateTime.UtcNow;
            var newFollowers = validUserIds
                .Except(existingUserIds)
                .Select(userId => new EntityFollower
                {
                    UserId = userId,
                    EntityType = normalizedType,
                    EntityId = entityId,
                    CreatedAt = now
                })
                .ToList();

            if (newFollowers.Count > 0)
            {
                await _context.EntityFollowers.AddRangeAsync(newFollowers);
                _context.SiteAuditLogs.Add(new SiteAuditLog
                {
                    EntityId = entityId,
                    EntityType = normalizedType,
                    Action = "AddFollowers",
                    UserId = actorUserId,
                    NewValue = string.Join(",", newFollowers.Select(f => f.UserId)),
                    CreatedAt = now
                });

                await _context.SaveChangesAsync();
            }

            return await GetFollowersAsync(normalizedType, entityId);
        }

        public async Task<object> ToggleFollowAsync(Guid userId, string entityType, Guid entityId)
        {
            entityType = NormalizeEntityType(entityType);
            var existing = await _context.EntityFollowers
                .FirstOrDefaultAsync(f => f.UserId == userId && f.EntityType == entityType && f.EntityId == entityId);

            bool isFollowing = false;

            if (existing != null)
            {
                _context.EntityFollowers.Remove(existing);
                _context.SiteAuditLogs.Add(new SiteAuditLog
                {
                    EntityId = entityId,
                    EntityType = entityType,
                    Action = "Unfollow",
                    UserId = userId,
                    OldValue = entityId.ToString(),
                    CreatedAt = DateTime.UtcNow
                });
            }
            else
            {
                var newFollower = new EntityFollower
                {
                    UserId = userId,
                    EntityType = entityType,
                    EntityId = entityId,
                    CreatedAt = DateTime.UtcNow
                };
                await _context.EntityFollowers.AddAsync(newFollower);
                _context.SiteAuditLogs.Add(new SiteAuditLog
                {
                    EntityId = entityId,
                    EntityType = entityType,
                    Action = "Follow",
                    UserId = userId,
                    NewValue = entityId.ToString(),
                    CreatedAt = DateTime.UtcNow
                });
                isFollowing = true;
            }

            await _context.SaveChangesAsync();

            return new { isFollowing };
        }

        private static string NormalizeEntityType(string entityType)
        {
            return string.IsNullOrWhiteSpace(entityType)
                ? string.Empty
                : entityType.Trim();
        }
    }
}
