using System;
using System.Threading.Tasks;
using TaskManagement.Application.DTOs.Project;

namespace TaskManagement.Application.Interfaces
{
    public interface IProjectMemberService
    {
        Task InviteMemberAsync(Guid projectId, ProjectMemberRequestDto request);
        Task RemoveMemberAsync(Guid projectId, Guid userId);
    }
}
