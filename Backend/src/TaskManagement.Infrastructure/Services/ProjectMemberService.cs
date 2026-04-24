using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using TaskManagement.Application.Common;
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
                throw new ArgumentException("Nguoi dung khong ton tai trong he thong.");
            }

            var resolvedProjectRole = await ResolveProjectRoleAsync(request.Role);

            bool isAlreadyMember = await _context.ProjectMembers
                .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == user.Id && pm.Status);

            if (isAlreadyMember)
            {
                throw new InvalidOperationException("Thanh vien nay da ton tai trong du an.");
            }

            var softDeletedMember = await _context.ProjectMembers
                .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == user.Id && !pm.Status);

            if (softDeletedMember != null)
            {
                softDeletedMember.Status = true;
                softDeletedMember.ProjectRole = resolvedProjectRole;
                softDeletedMember.JoinedAt = DateTime.UtcNow;
                softDeletedMember.LeftAt = null;
            }
            else
            {
                var newMember = new ProjectMember
                {
                    ProjectId = projectId,
                    UserId = user.Id,
                    ProjectRole = resolvedProjectRole,
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
                    throw new ArgumentException("Thanh vien khong ton tai hoac da roi du an.");
                }

                member.Status = false;
                member.LeftAt = DateTime.UtcNow;

                var orphans = await _context.TaskAssignments
                    .Include(ta => ta.WorkTask)
                    .Where(ta => ta.UserId == userId && ta.WorkTask.ProjectId == projectId)
                    .ToListAsync();

                if (orphans.Any())
                {
                    _context.TaskAssignments.RemoveRange(orphans);

                    var pm = await _context.ProjectMembers
                        .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.ProjectRole == "PM" && pm.Status);

                    if (pm != null)
                    {
                        var notification = new Notification
                        {
                            Id = Guid.NewGuid(),
                            UserId = pm.UserId,
                            Title = "Task mo coi - thanh vien roi du an",
                            Content = $"Mot thanh vien vua bi xoa khoi du an. Co {orphans.Count} task dang bi mo coi can duoc phan cong lai.",
                            CreatedAt = DateTime.UtcNow,
                            IsRead = false
                        };
                        _context.Notifications.Add(notification);
                    }
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

        public async Task UpdateMemberRoleAsync(Guid projectId, Guid userId, string newRole)
        {
            var member = await _context.ProjectMembers
                .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId && pm.Status);

            if (member == null)
            {
                throw new ArgumentException("Thanh vien khong ton tai trong du an.");
            }

            member.ProjectRole = await ResolveProjectRoleAsync(newRole);
            await _context.SaveChangesAsync();
        }

        public async Task<System.Collections.Generic.IEnumerable<ProjectMemberResponseDto>> GetProjectMembersAsync(Guid projectId)
        {
            var members = await _context.ProjectMembers
                .AsNoTracking()
                .Include(pm => pm.User)
                .Where(pm => pm.ProjectId == projectId && pm.Status && !pm.User.IsDeleted)
                .Select(pm => new ProjectMemberResponseDto
                {
                    UserId = pm.UserId,
                    Email = pm.User.Email,
                    FullName = pm.User.FullName,
                    ProjectRole = pm.ProjectRole,
                    JoinedAt = pm.JoinedAt
                })
                .ToListAsync();

            return members;
        }

        private async Task<string> ResolveProjectRoleAsync(string? requestedRole)
        {
            var normalizedRequestedRole = ProjectExecutionRuleHelper.NormalizeProjectRole(requestedRole);
            if (string.IsNullOrWhiteSpace(normalizedRequestedRole))
            {
                normalizedRequestedRole = ProjectExecutionRuleHelper.NormalizeProjectRole("Developer");
            }

            var availableRoles = await _context.Roles
                .AsNoTracking()
                .Select(role => new
                {
                    role.Name,
                    Normalized = ProjectExecutionRuleHelper.NormalizeProjectRole(role.Name)
                })
                .ToListAsync();

            var exactMatch = availableRoles.FirstOrDefault(role => role.Normalized == normalizedRequestedRole);
            if (exactMatch != null)
            {
                return exactMatch.Name;
            }

            var aliasMatch = normalizedRequestedRole switch
            {
                "dev" => availableRoles.FirstOrDefault(role => role.Normalized == "developer"),
                "project_manager" => availableRoles.FirstOrDefault(role => role.Normalized == "pm"),
                "scrum_master" => availableRoles.FirstOrDefault(role => role.Normalized == "sm"),
                _ => null
            };

            if (aliasMatch != null)
            {
                return aliasMatch.Name;
            }

            throw new ArgumentException($"Vai tro '{requestedRole}' khong hop le cho du an.");
        }
    }
}
