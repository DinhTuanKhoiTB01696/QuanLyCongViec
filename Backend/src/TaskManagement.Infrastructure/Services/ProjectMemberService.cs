using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Constants;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Application.DTOs.Project;

namespace TaskManagement.Infrastructure.Services
{
    public class ProjectMemberService : IProjectMemberService
    {
        private readonly ApplicationDbContext _context;

        public ProjectMemberService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task RemoveMemberAsync(Guid projectId, Guid userId, Guid adminId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Tìm ProjectMember
                var member = await _context.ProjectMembers
                    .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);

                if (member == null || !member.Status)
                {
                    throw new ArgumentException("Member not found in project or already removed.");
                }

                // 2. Cập nhật trạng thái (Soft Delete)
                member.Status = false;
                member.LeftAt = DateTime.UtcNow;

                // 3. Xử lý "Task mồ côi"
                // Lấy tất cả các TaskAssignment của User trong Project này
                var orphans = await _context.TaskAssignments
                    .Include(ta => ta.WorkTask)
                    .Where(ta => ta.UserId == userId && ta.WorkTask.ProjectId == projectId)
                    .ToListAsync();

                if (orphans.Any())
                {
                    _context.TaskAssignments.RemoveRange(orphans);
                }

                // 4. Báo Notification cho các PM (Project Manager)
                var projectManagers = await _context.ProjectMembers
                    .Where(pm => pm.ProjectId == projectId 
                                    && pm.Status == true 
                                    && pm.ProjectRole == ProjectRoles.PM)
                    .Select(pm => pm.UserId)
                    .ToListAsync();

                // Lấy thông tin user bị kick để gán tên thật vào thông báo nếu cần (tối ưu: bỏ qua nếu k quá quan trọng, chỉ dùng ID)
                var kickedUser = await _context.Users.FindAsync(userId);
                string userName = kickedUser?.FullName ?? "Một thành viên";

                var currentTime = DateTime.UtcNow;
                foreach (var pmId in projectManagers)
                {
                    _context.Notifications.Add(new Notification
                    {
                        Id = Guid.NewGuid(),
                        UserId = pmId,
                        Title = "⚠ Cảnh báo: Task cần gán lại",
                        Content = $"User '{userName}' (Role: {member.ProjectRole}) đã bị xóa khỏi dự án. {orphans.Count} task đang bị bỏ trống (orphan), vui lòng phân công lại.",
                        CreatedAt = currentTime,
                        IsRead = false
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<ProjectMemberDto>> GetProjectMembersAsync(Guid projectId)
        {
            return await _context.ProjectMembers
                .Include(pm => pm.User)
                .Where(pm => pm.ProjectId == projectId && pm.Status)
                .Select(pm => new ProjectMemberDto
                {
                    UserId = pm.UserId,
                    FullName = pm.User.FullName,
                    Email = pm.User.Email,
                    ProjectRole = pm.ProjectRole,
                    Status = pm.Status
                })
                .ToListAsync();
        }
    }
}
