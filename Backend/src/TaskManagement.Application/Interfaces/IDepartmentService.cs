using TaskManagement.Application.DTOs.Department;

namespace TaskManagement.Application.Interfaces
{
    public interface IDepartmentService
    {
        Task<List<DepartmentResponseDto>> GetAllAsync();
        Task<DepartmentResponseDto?> GetByIdAsync(Guid id);
        Task<DepartmentResponseDto> CreateAsync(CreateDepartmentDto dto);
        Task<DepartmentResponseDto> UpdateAsync(Guid id, UpdateDepartmentDto dto);
        Task ArchiveAsync(Guid id);
        Task RestoreAsync(Guid id);
        Task SoftDeleteAsync(Guid id);
        
        // Members
        Task AddMembersAsync(Guid departmentId, List<Guid> userIds);
        Task RemoveMemberAsync(Guid departmentId, Guid userId);

        // Hierarchy
        Task UpdateHierarchyAsync(Guid departmentId, Guid? parentId);
    }
}
