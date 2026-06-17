using System;
using System.Threading.Tasks;

namespace TaskManagement.Application.Interfaces
{
    public interface IProjectLinkService
    {
        Task<object> GetAllLinksAsync(Guid projectId);
        Task<object> CreateLinkAsync(Guid creatorId, Guid projectId, object dto);
        Task DeleteLinkAsync(Guid id);
    }
}
