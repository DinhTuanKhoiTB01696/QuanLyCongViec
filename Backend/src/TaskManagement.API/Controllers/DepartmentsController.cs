using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.DTOs.Common;
using TaskManagement.Application.DTOs.Department;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.API.Controllers
{
    [ApiController]
    [Route("api/departments")]
    [Authorize]
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;
        private readonly TaskManagement.Infrastructure.Data.ApplicationDbContext _context;

        public DepartmentsController(IDepartmentService departmentService, TaskManagement.Infrastructure.Data.ApplicationDbContext context)
        {
            _departmentService = departmentService;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var departments = await _departmentService.GetAllAsync();
            return Ok(ApiResponse<List<DepartmentResponseDto>>.Success(departments));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var department = await _departmentService.GetByIdAsync(id);
            if (department == null)
                return NotFound(ApiResponse<object>.Error("Phòng ban không tồn tại.", 404));

            return Ok(ApiResponse<DepartmentResponseDto>.Success(department));
        }

        [HttpGet("{id}/full")]
        public async Task<IActionResult> GetFullById(Guid id)
        {
            var department = await _departmentService.GetByIdAsync(id);
            if (department == null)
                return NotFound(ApiResponse<object>.Error("Phòng ban không tồn tại.", 404));

            var members = await _context.DepartmentMembers
                .Where(dm => dm.DepartmentId == id)
                .Select(dm => new { 
                    id = dm.UserId, 
                    name = dm.User.FullName ?? dm.User.Email, 
                    email = dm.User.Email,
                    avatar = dm.User.AvatarUrl ?? (dm.User.FullName != null ? dm.User.FullName.Substring(0, 2).ToUpper() : "U"),
                    role = "Member"
                })
                .ToListAsync();

            var goals = await _context.Goals
                .Where(g => g.DepartmentId == id && !g.IsArchived)
                .Select(g => new { id = g.Id, title = g.Title, status = g.Status })
                .ToListAsync();

            var projects = await _context.ProjectDepartmentRoles
                .Where(pdr => pdr.DepartmentId == id)
                .Select(pdr => new { id = pdr.ProjectId, name = pdr.Project.Name, status = "Active" })
                .ToListAsync();

            var parent = department.ParentId.HasValue ? await _context.Departments
                .Where(d => d.Id == department.ParentId.Value)
                .Select(d => new { id = d.Id, name = d.Name })
                .FirstOrDefaultAsync() : null;

            var children = await _context.Departments
                .Where(d => d.ParentId == id && !d.IsDeleted)
                .Select(d => new { id = d.Id, name = d.Name })
                .ToListAsync();

            var kudos = await _context.Kudos
                .Include(k => k.Sender)
                .Where(k => k.DepartmentId == id)
                .OrderByDescending(k => k.CreatedAt)
                .Select(k => new {
                    id = k.Id,
                    message = k.Message,
                    sender = k.Sender.FullName ?? k.Sender.Email,
                    icon = k.Icon ?? "🌟",
                    createdAt = k.CreatedAt
                })
                .ToListAsync();

            return Ok(new {
                statusCode = 200,
                data = new {
                    id = department.Id,
                    name = department.Name,
                    isArchived = !department.IsActive,
                    description = department.Description ?? "Phòng ban / Đội nhóm trong hệ thống",
                    coverImage = department.CoverImage ?? "https://images.unsplash.com/photo-1550751827-4bd374c3f58b?w=1200&q=80",
                    members = members,
                    hierarchy = new { parent = parent, children = children },
                    goals = goals,
                    projects = projects,
                    kudos = kudos
                }
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDepartmentDto dto)
        {
            try
            {
                var result = await _departmentService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = result.Id },
                    ApiResponse<DepartmentResponseDto>.Created(result));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.Error(ex.Message));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDepartmentDto dto)
        {
            try
            {
                var result = await _departmentService.UpdateAsync(id, dto);
                return Ok(ApiResponse<DepartmentResponseDto>.Success(result, "Cập nhật thành công."));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.Error(ex.Message));
            }
        }

        /// <summary>
        /// 5.1 Archive: Vô hiệu hóa tạm thời (ẩn khỏi dropdown, vẫn xem lịch sử)
        /// </summary>
        [HttpPut("{id}/archive")]
        public async Task<IActionResult> Archive(Guid id)
        {
            try
            {
                await _departmentService.ArchiveAsync(id);
                return Ok(ApiResponse<object>.Success(null!, "Phòng ban đã được vô hiệu hóa."));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Khôi phục phòng ban đã bị Archive
        /// </summary>
        [HttpPut("{id}/restore")]
        public async Task<IActionResult> Restore(Guid id)
        {
            try
            {
                await _departmentService.RestoreAsync(id);
                return Ok(ApiResponse<object>.Success(null!, "Phòng ban đã được khôi phục."));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.Error(ex.Message));
            }
        }

        /// <summary>
        /// 5.1 Soft Delete: Đánh dấu xóa (Global Query Filter sẽ tự ẩn)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDelete(Guid id)
        {
            try
            {
                await _departmentService.SoftDeleteAsync(id);
                return Ok(ApiResponse<object>.Success(null!, "Phòng ban đã được xóa."));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.Error(ex.Message));
            }
        }

        [HttpPost("{id}/members")]
        public async Task<IActionResult> AddMembers(Guid id, [FromBody] List<Guid> userIds)
        {
            try
            {
                await _departmentService.AddMembersAsync(id, userIds);
                return Ok(ApiResponse<object>.Success(null!, "Thêm thành viên thành công."));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.Error(ex.Message));
            }
        }

        [HttpDelete("{id}/members/{userId}")]
        public async Task<IActionResult> RemoveMember(Guid id, Guid userId)
        {
            try
            {
                await _departmentService.RemoveMemberAsync(id, userId);
                return Ok(ApiResponse<object>.Success(null!, "Xóa thành viên thành công."));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.Error(ex.Message));
            }
        }

        [HttpPut("{id}/hierarchy")]
        public async Task<IActionResult> UpdateHierarchy(Guid id, [FromBody] Guid? parentId)
        {
            try
            {
                await _departmentService.UpdateHierarchyAsync(id, parentId);
                return Ok(ApiResponse<object>.Success(null!, "Cập nhật sơ đồ phân cấp thành công."));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.Error(ex.Message));
            }
        }

        [HttpPost("{id}/goals")]
        public async Task<IActionResult> LinkGoal(Guid id, [FromBody] Guid goalId)
        {
            var goal = await _context.Goals.FindAsync(goalId);
            if (goal == null) return NotFound(ApiResponse<object>.Error("Mục tiêu không tồn tại."));

            var department = await _context.Departments.FindAsync(id);
            if (department == null) return NotFound(ApiResponse<object>.Error("Đội ngũ không tồn tại."));

            goal.DepartmentId = id;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.Success(null!, "Đã liên kết mục tiêu vào đội ngũ."));
        }

        [HttpDelete("{id}/goals/{goalId}")]
        public async Task<IActionResult> UnlinkGoal(Guid id, Guid goalId)
        {
            var goal = await _context.Goals.FindAsync(goalId);
            if (goal == null) return NotFound(ApiResponse<object>.Error("Mục tiêu không tồn tại."));

            if (goal.DepartmentId == id)
            {
                goal.DepartmentId = null;
                await _context.SaveChangesAsync();
            }

            return Ok(ApiResponse<object>.Success(null!, "Đã gỡ liên kết mục tiêu khỏi đội ngũ."));
        }

        [HttpPost("{id}/projects")]
        public async Task<IActionResult> LinkProject(Guid id, [FromBody] Guid projectId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null) return NotFound(ApiResponse<object>.Error("Dự án không tồn tại."));

            var department = await _context.Departments.FindAsync(id);
            if (department == null) return NotFound(ApiResponse<object>.Error("Đội ngũ không tồn tại."));

            var existingLink = await _context.ProjectDepartmentRoles
                .FirstOrDefaultAsync(pdr => pdr.ProjectId == projectId && pdr.DepartmentId == id);

            if (existingLink == null)
            {
                _context.ProjectDepartmentRoles.Add(new TaskManagement.Domain.Entities.ProjectDepartmentRole
                {
                    ProjectId = projectId,
                    DepartmentId = id,
                    RoleName = "Member",
                    AssignedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }

            return Ok(ApiResponse<object>.Success(null!, "Đã liên kết dự án vào đội ngũ."));
        }

        [HttpDelete("{id}/projects/{projectId}")]
        public async Task<IActionResult> UnlinkProject(Guid id, Guid projectId)
        {
            var existingLink = await _context.ProjectDepartmentRoles
                .FirstOrDefaultAsync(pdr => pdr.ProjectId == projectId && pdr.DepartmentId == id);

            if (existingLink != null)
            {
                _context.ProjectDepartmentRoles.Remove(existingLink);
                await _context.SaveChangesAsync();
            }

            return Ok(ApiResponse<object>.Success(null!, "Đã gỡ liên kết dự án khỏi đội ngũ."));
        }
    }
}
