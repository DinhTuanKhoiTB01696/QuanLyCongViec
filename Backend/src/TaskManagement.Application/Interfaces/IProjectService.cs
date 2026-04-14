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
    }
}
