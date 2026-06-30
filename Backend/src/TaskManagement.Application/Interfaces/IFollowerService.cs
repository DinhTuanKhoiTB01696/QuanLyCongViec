using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaskManagement.Application.Interfaces
{
    public interface IFollowerService
    {
        Task<IEnumerable<object>> GetAllFollowedAsync(Guid userId);
        Task<IEnumerable<object>> GetFollowersAsync(string entityType, Guid entityId);
        Task<IEnumerable<object>> AddFollowersAsync(Guid actorUserId, string entityType, Guid entityId, IEnumerable<Guid> userIds);
        Task<object> ToggleFollowAsync(Guid userId, string entityType, Guid entityId);
    }
}
