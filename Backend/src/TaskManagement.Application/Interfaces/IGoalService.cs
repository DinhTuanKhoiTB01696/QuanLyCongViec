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
        Task<object> UpdateUpdateAsync(Guid goalId, Guid updateId, Guid userId, object dto);
        Task DeleteUpdateAsync(Guid goalId, Guid updateId, Guid userId);

        Task<object> AddLessonAsync(Guid goalId, Guid userId, object dto);
        Task<object> UpdateLessonAsync(Guid goalId, Guid lessonId, Guid userId, object dto);
        Task DeleteLessonAsync(Guid goalId, Guid lessonId, Guid userId);

        Task<object> AddRiskAsync(Guid goalId, Guid userId, object dto);
        Task<object> UpdateRiskAsync(Guid goalId, Guid riskId, Guid userId, object dto);
        Task DeleteRiskAsync(Guid goalId, Guid riskId, Guid userId);

        Task<object> AddDecisionAsync(Guid goalId, Guid userId, object dto);
        Task<object> UpdateDecisionAsync(Guid goalId, Guid decisionId, Guid userId, object dto);
        Task DeleteDecisionAsync(Guid goalId, Guid decisionId, Guid userId);

        Task<object> GetCommentsAsync(Guid goalId);
        Task<object> AddCommentAsync(Guid goalId, Guid userId, object dto);
        Task<object> UpdateCommentAsync(Guid commentId, Guid userId, object dto);
        Task DeleteCommentAsync(Guid commentId, Guid userId);

        Task<object> GetUpdateCommentsAsync(Guid updateId);
        Task<object> AddUpdateCommentAsync(Guid goalId, Guid updateId, Guid userId, object dto);

        Task<object> GetReactionsAsync(Guid updateId);
        Task<object> ToggleReactionAsync(Guid updateId, Guid userId, object dto);

        Task<object> GetUpdateAttachmentsAsync(Guid updateId);
        Task<object> AddUpdateAttachmentAsync(Guid updateId, Guid userId, string fileName, string fileUrl, long fileSize);
        Task DeleteUpdateAttachmentAsync(Guid attachmentId, Guid userId);
        
        Task<object> GetProjectLinksAsync(Guid goalId, string linkCategory = null);
        Task<object> AddProjectLinkAsync(Guid goalId, Guid userId, object dto);
        Task DeleteProjectLinkAsync(Guid goalId, Guid linkId, Guid userId);
    }
}
