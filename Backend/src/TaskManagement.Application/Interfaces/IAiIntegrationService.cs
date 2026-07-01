using System;
using System.Threading.Tasks;

namespace TaskManagement.Application.Interfaces
{
    public interface IAiIntegrationService
    {
        Task<object> SummarizeInboxItemAsync(Guid inboxItemId, Guid userId);
        Task<object> SuggestTaskFromInboxItemAsync(Guid inboxItemId, Guid userId);
        Task<object> SuggestRelatedTaskAsync(Guid inboxItemId, Guid userId);
    }
}
