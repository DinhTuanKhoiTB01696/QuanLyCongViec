using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskManagement.Application.DTOs.Project;

namespace TaskManagement.Application.Interfaces
{
    public interface IProjectMemberService
    {
        Task RemoveMemberAsync(Guid projectId, Guid userId, Guid adminId);
        Task<IEnumerable<ProjectMemberDto>> GetProjectMembersAsync(Guid projectId);
    }
}
