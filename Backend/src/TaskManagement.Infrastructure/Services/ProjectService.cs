using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TaskManagement.Application.DTOs.Project;
using TaskManagement.Application.Interfaces;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Infrastructure.Services
{
    public class ProjectService : IProjectService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProjectService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// 5.2 Chống N+1: Dùng .Include().Select() gom data trong 1 câu SQL
        /// </summary>
        public async Task<List<ProjectResponseDto>> GetAllAsync()
        {
            var userIdString = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                return new List<ProjectResponseDto>();
            }

            return await _context.Projects
                .AsNoTracking()
                .Include(p => p.Creator)
                .Include(p => p.Department)
                .Where(p => !p.IsDeleted && p.ProjectMembers.Any(pm => pm.UserId == userId && pm.Status))
                .Select(p => new ProjectResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    Status = p.Status,
                    CreatorName = p.Creator.FullName,
                    DepartmentId = p.DepartmentId,
                    DepartmentName = p.Department != null ? p.Department.Name : null,
                    ActiveMemberCount = p.ProjectMembers.Count(m => m.Status == true),
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                })
                .ToListAsync();
        }

        public async Task<ProjectResponseDto?> GetByIdAsync(Guid id)
        {
            return await _context.Projects
                .Include(p => p.Creator)
                .Include(p => p.Department)
                .Where(p => p.Id == id)
                .Select(p => new ProjectResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    Status = p.Status,
                    CreatorName = p.Creator.FullName,
                    DepartmentId = p.DepartmentId,
                    DepartmentName = p.Department != null ? p.Department.Name : null,
                    ActiveMemberCount = p.ProjectMembers.Count(m => m.Status == true),
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                })
                .FirstOrDefaultAsync();
        }

        public async Task<ProjectResponseDto> CreateAsync(Guid creatorId, CreateProjectDto dto)
        {
            // Validate DepartmentId nếu có
            if (dto.DepartmentId.HasValue)
            {
                var deptExists = await _context.Departments.AnyAsync(d => d.Id == dto.DepartmentId.Value);
                if (!deptExists)
                    throw new ArgumentException("Phòng ban không tồn tại.");
            }

            var project = new Project
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Status = true,
                CreatorId = creatorId,
                DepartmentId = dto.DepartmentId,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Projects.Add(project);

            // Seed default Task Statuses for the new project
            _context.TaskStatuses.AddRange(
                new TaskManagement.Domain.Entities.TaskStatus { Id = Guid.NewGuid(), ProjectId = project.Id, Name = "TO DO", Position = 1 },
                new TaskManagement.Domain.Entities.TaskStatus { Id = Guid.NewGuid(), ProjectId = project.Id, Name = "IN PROGRESS", Position = 2 },
                new TaskManagement.Domain.Entities.TaskStatus { Id = Guid.NewGuid(), ProjectId = project.Id, Name = "DONE", Position = 3 }
            );

            await _context.SaveChangesAsync();

            return (await GetByIdAsync(project.Id))!;
        }

        public async Task<ProjectResponseDto> UpdateAsync(Guid id, UpdateProjectDto dto)
        {
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);
            if (project == null)
                throw new ArgumentException("Dự án không tồn tại.");

            // Validate DepartmentId nếu có
            if (dto.DepartmentId.HasValue)
            {
                var deptExists = await _context.Departments.AnyAsync(d => d.Id == dto.DepartmentId.Value);
                if (!deptExists)
                    throw new ArgumentException("Phòng ban không tồn tại.");
            }

            project.Name = dto.Name;
            project.Description = dto.Description;
            project.StartDate = dto.StartDate;
            project.EndDate = dto.EndDate;
            project.DepartmentId = dto.DepartmentId;
            project.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return (await GetByIdAsync(project.Id))!;
        }

        /// <summary>
        /// 5.1 Archive: Set Status = false (ẩn khỏi danh sách active, vẫn xem được lịch sử)
        /// </summary>
        public async Task ArchiveAsync(Guid id)
        {
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);
            if (project == null)
                throw new ArgumentException("Dự án không tồn tại.");

            project.Status = false;
            project.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task RestoreAsync(Guid id)
        {
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);
            if (project == null)
                throw new ArgumentException("Dự án không tồn tại.");

            project.Status = true;
            project.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 5.1 Soft Delete: Set IsDeleted = true, Global Query Filter sẽ tự ẩn
        /// </summary>
        public async Task SoftDeleteAsync(Guid id)
        {
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);
            if (project == null)
                throw new ArgumentException("Dự án không tồn tại.");

            project.IsDeleted = true;
            project.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
