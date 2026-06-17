using System;
using System.Threading.Tasks;

namespace TaskManagement.Application.Interfaces
{
    public interface IStarredItemService
    {
        Task<object> GetAllAsync(Guid userId, Guid workspaceId);
        Task<object> ToggleStarAsync(Guid userId, Guid workspaceId, string itemType, Guid itemId);
    }
}
