using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaskManagement.Application.Interfaces
{
    public interface IGoalService
    {
        Task<object> GetAllAsync(Guid workspaceId);
        Task<object> GetStatusesAsync();
        Task<object?> GetByIdAsync(Guid id);
        Task<object> CreateAsync(Guid creatorId, Guid workspaceId, object dto);
        Task<object> UpdateAsync(Guid id, object dto);
        Task ArchiveAsync(Guid id);
        Task DeleteAsync(Guid id);
        
        Task<object> AddUpdateAsync(Guid goalId, Guid userId, object dto);
        Task<object> AddLessonAsync(Guid goalId, Guid userId, object dto);
        Task<object> AddRiskAsync(Guid goalId, Guid userId, object dto);
        Task<object> AddDecisionAsync(Guid goalId, Guid userId, object dto);
    }
}
