using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.DTOs.Department;
using TaskManagement.Application.Interfaces;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Infrastructure.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly ApplicationDbContext _context;

        public DepartmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<DepartmentResponseDto>> GetAllAsync()
        {
            // Global Query Filter tự động loại IsDeleted == true
            return await _context.Departments
                .Include(d => d.Manager)
                .Select(d => new DepartmentResponseDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    ManagerId = d.ManagerId,
                    ManagerName = d.Manager != null ? d.Manager.FullName : null,
                    IsActive = d.IsActive,
                    MemberCount = d.DepartmentMembers.Count(),
                    ParentId = d.ParentId,
                    Description = d.Description,
                    CoverImage = d.CoverImage,
                    CreatedAt = d.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<DepartmentResponseDto?> GetByIdAsync(Guid id)
        {
            return await _context.Departments
                .Include(d => d.Manager)
                .Where(d => d.Id == id)
                .Select(d => new DepartmentResponseDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    ManagerId = d.ManagerId,
                    ManagerName = d.Manager != null ? d.Manager.FullName : null,
                    IsActive = d.IsActive,
                    MemberCount = d.DepartmentMembers.Count(),
                    ParentId = d.ParentId,
                    Description = d.Description,
                    CoverImage = d.CoverImage,
                    CreatedAt = d.CreatedAt
                })
                .FirstOrDefaultAsync();
        }

        public async Task<DepartmentResponseDto> CreateAsync(CreateDepartmentDto dto)
        {
            // Validate ManagerId nếu có
            if (dto.ManagerId.HasValue)
            {
                var managerExists = await _context.Users.AnyAsync(u => u.Id == dto.ManagerId.Value && !u.IsDeleted);
                if (!managerExists)
                    throw new ArgumentException("Manager không tồn tại.");
            }

            var department = new Department
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                ManagerId = dto.ManagerId,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            return (await GetByIdAsync(department.Id))!;
        }

        public async Task<DepartmentResponseDto> UpdateAsync(Guid id, UpdateDepartmentDto dto)
        {
            var department = await _context.Departments.FirstOrDefaultAsync(d => d.Id == id);
            if (department == null)
                throw new ArgumentException("Phòng ban không tồn tại.");

            // Validate ManagerId nếu có
            if (dto.ManagerId.HasValue)
            {
                var managerExists = await _context.Users.AnyAsync(u => u.Id == dto.ManagerId.Value && !u.IsDeleted);
                if (!managerExists)
                    throw new ArgumentException("Manager không tồn tại.");
            }

            department.Name = dto.Name;
            department.ManagerId = dto.ManagerId;
            department.Description = dto.Description;
            department.CoverImage = dto.CoverImage;

            await _context.SaveChangesAsync();

            return (await GetByIdAsync(department.Id))!;
        }

        /// <summary>
        /// 5.1 Archive: Vô hiệu hóa tạm thời - ẩn khỏi dropdown tạo dự án mới
        /// </summary>
        public async Task ArchiveAsync(Guid id)
        {
            var department = await _context.Departments.FirstOrDefaultAsync(d => d.Id == id);
            if (department == null)
                throw new ArgumentException("Phòng ban không tồn tại.");

            department.IsActive = false;
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Khôi phục phòng ban đã bị Archive
        /// </summary>
        public async Task RestoreAsync(Guid id)
        {
            var department = await _context.Departments.FirstOrDefaultAsync(d => d.Id == id);
            if (department == null)
                throw new ArgumentException("Phòng ban không tồn tại.");

            department.IsActive = true;
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 5.1 Soft Delete: Đánh dấu xóa, Global Query Filter sẽ tự ẩn
        /// </summary>
        public async Task SoftDeleteAsync(Guid id)
        {
            var department = await _context.Departments.FirstOrDefaultAsync(d => d.Id == id);
            if (department == null)
                throw new ArgumentException("Phòng ban không tồn tại.");

            department.IsDeleted = true;
            await _context.SaveChangesAsync();
        }

        public async Task AddMembersAsync(Guid departmentId, List<Guid> userIds)
        {
            var department = await _context.Departments.FirstOrDefaultAsync(d => d.Id == departmentId);
            if (department == null) throw new ArgumentException("Phòng ban không tồn tại.");

            var existingMembers = await _context.DepartmentMembers
                .Where(dm => dm.DepartmentId == departmentId)
                .Select(dm => dm.UserId)
                .ToListAsync();

            var newMembers = userIds.Except(existingMembers).ToList();
            if (!newMembers.Any()) return;

            var validUsers = await _context.Users.Where(u => newMembers.Contains(u.Id) && !u.IsDeleted).Select(u => u.Id).ToListAsync();

            foreach (var userId in validUsers)
            {
                _context.DepartmentMembers.Add(new DepartmentMember
                {
                    DepartmentId = departmentId,
                    UserId = userId,
                    JoinedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
        }

        public async Task RemoveMemberAsync(Guid departmentId, Guid userId)
        {
            var member = await _context.DepartmentMembers
                .FirstOrDefaultAsync(dm => dm.DepartmentId == departmentId && dm.UserId == userId);
            
            if (member == null) return;

            _context.DepartmentMembers.Remove(member);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateHierarchyAsync(Guid departmentId, Guid? parentId)
        {
            var department = await _context.Departments.FirstOrDefaultAsync(d => d.Id == departmentId);
            if (department == null) throw new ArgumentException("Phòng ban không tồn tại.");

            if (parentId.HasValue)
            {
                if (parentId.Value == departmentId) throw new ArgumentException("Phòng ban không thể là cha của chính nó.");
                
                var parentExists = await _context.Departments.AnyAsync(d => d.Id == parentId.Value && !d.IsDeleted);
                if (!parentExists) throw new ArgumentException("Phòng ban cha không tồn tại.");
            }

            department.ParentId = parentId;
            await _context.SaveChangesAsync();
        }
    }
}
