using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using TaskManagement.Application.DTOs.Project;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure.Services
{
    public class ProjectMemberService : IProjectMemberService
    {
        private readonly ApplicationDbContext _context;

        public ProjectMemberService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task InviteMemberAsync(Guid projectId, ProjectMemberRequestDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email && !u.IsDeleted);
            if (user == null)
            {
                throw new ArgumentException("Người dùng không tồn tại trong hệ thống.");
            }

            bool isAlreadyMember = await _context.ProjectMembers
                .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == user.Id && pm.Status);
            
            if (isAlreadyMember)
            {
                throw new InvalidOperationException("Thành viên này đã tồn tại trong dự án."); // Controller can map this to 409 Conflict
            }

            var softDeletedMember = await _context.ProjectMembers
                .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == user.Id && !pm.Status);

            if (softDeletedMember != null)
            {
                softDeletedMember.Status = true;
                softDeletedMember.ProjectRole = request.Role;
                softDeletedMember.JoinedAt = DateTime.UtcNow;
                softDeletedMember.LeftAt = null;
            }
            else
            {
                var newMember = new ProjectMember
                {
                    ProjectId = projectId,
                    UserId = user.Id,
                    ProjectRole = request.Role,
                    JoinedAt = DateTime.UtcNow,
                    Status = true
                };
                _context.ProjectMembers.Add(newMember);
            }

            await _context.SaveChangesAsync();
        }

        public async Task RemoveMemberAsync(Guid projectId, Guid userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var member = await _context.ProjectMembers
                    .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId && pm.Status);

                if (member == null)
                {
                    throw new ArgumentException("Thành viên không tồn tại hoặc đã rời dự án.");
                }

                // Soft Delete
                member.Status = false;
                member.LeftAt = DateTime.UtcNow;

                // Handle Orphan Tasks
                var orphans = await _context.TaskAssignments
                    .Include(ta => ta.WorkTask)
                    .Where(ta => ta.UserId == userId && ta.WorkTask.ProjectId == projectId)
                    .ToListAsync();

                if (orphans.Any())
                {
                    _context.TaskAssignments.RemoveRange(orphans);

                    // Notify PM
                    var pm = await _context.ProjectMembers
                        .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.ProjectRole == "PM" && pm.Status);

                    if (pm != null)
                    {
                        var notification = new Notification
                        {
                            Id = Guid.NewGuid(),
                            UserId = pm.UserId,
                            Title = "Task Mồ Côi - Thành viên rời dự án",
                            Content = $"Một thành viên vừa bị xóa khỏi dự án. Có {orphans.Count} task đang bị mồ côi cần được phân công lại.",
                            CreatedAt = DateTime.UtcNow,
                            IsRead = false
                        };
                        _context.Notifications.Add(notification);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
