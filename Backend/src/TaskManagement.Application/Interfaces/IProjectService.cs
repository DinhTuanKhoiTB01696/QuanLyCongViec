using TaskManagement.Application.DTOs.Project;

namespace TaskManagement.Application.Interfaces
{
    public interface IProjectService
    {
        Task<List<ProjectResponseDto>> GetAllAsync();
        Task<List<ProjectDiscoveryDto>> GetAllForDiscoveryAsync();
        Task<ProjectResponseDto?> GetByIdAsync(Guid id);
        Task<ProjectResponseDto> CreateAsync(Guid creatorId, CreateProjectDto dto);
        Task<ProjectResponseDto> UpdateAsync(Guid id, UpdateProjectDto dto);
        Task ArchiveAsync(Guid id);
        Task RestoreAsync(Guid id);
        Task SoftDeleteAsync(Guid id);
        Task<List<ProjectMemberResponseDto>> GetMembersAsync(Guid projectId);
        Task<List<ProjectDiscoveryDto>> GetArchivedAsync();
        Task<List<ProjectDiscoveryDto>> GetDeletedAsync();
        Task RestoreDeletedAsync(Guid id);
        Task PermanentDeleteAsync(Guid id);
        Task<IEnumerable<TaskManagement.Application.DTOs.Common.TabItemDto>> GetUpdatesAsync(Guid id);
        Task<TaskManagement.Application.DTOs.Common.TabItemDto> AddUpdateAsync(Guid id, TaskManagement.Application.DTOs.Common.CreateTabItemDto dto, Guid userId);
        Task<IEnumerable<TaskManagement.Application.DTOs.Common.TabItemDto>> GetLessonsAsync(Guid id);
        Task<TaskManagement.Application.DTOs.Common.TabItemDto> AddLessonAsync(Guid id, TaskManagement.Application.DTOs.Common.CreateTabItemDto dto, Guid userId);
        Task<IEnumerable<TaskManagement.Application.DTOs.Common.TabItemDto>> GetRisksAsync(Guid id);
        Task<TaskManagement.Application.DTOs.Common.TabItemDto> AddRiskAsync(Guid id, TaskManagement.Application.DTOs.Common.CreateTabItemDto dto, Guid userId);
        Task<IEnumerable<TaskManagement.Application.DTOs.Common.TabItemDto>> GetDecisionsAsync(Guid id);
        Task<TaskManagement.Application.DTOs.Common.TabItemDto> AddDecisionAsync(Guid id, TaskManagement.Application.DTOs.Common.CreateTabItemDto dto, Guid userId);
    }
}
