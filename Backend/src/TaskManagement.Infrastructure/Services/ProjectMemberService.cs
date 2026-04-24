using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public ProjectMemberService(ApplicationDbContext context, IEmailService emailService, IConfiguration configuration)
        {
            _context = context;
            _emailService = emailService;
            _configuration = configuration;
        }

        public async Task InviteMemberAsync(Guid projectId, ProjectMemberRequestDto request, string inviterName)
        {
            var normalizedEmail = request.Email?.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(normalizedEmail))
            {
                throw new ArgumentException("Email thanh vien khong duoc de trong.");
            }

            var project = await _context.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == projectId && !p.IsDeleted);

            if (project == null)
            {
                throw new ArgumentException("Du an khong ton tai.");
            }

            var user = await _context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Email == normalizedEmail);

            if (user?.IsDeleted == true)
            {
                throw new ArgumentException("Email nay thuoc ve mot tai khoan da bi xoa.");
            }

            var resolvedProjectRole = await ResolveProjectRoleAsync(request.Role);
            var now = DateTime.UtcNow;

            if (user == null)
            {
                user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = normalizedEmail,
                    FullName = BuildNameFromEmail(normalizedEmail),
                    PasswordHash = string.Empty,
                    IsActive = false,
                    IsDeleted = false,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                _context.Users.Add(user);
            }
            else
            {
                user.UpdatedAt = now;
            }

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
                softDeletedMember.Status = false;
                softDeletedMember.ProjectRole = resolvedProjectRole;
                softDeletedMember.JoinedAt = now;
                softDeletedMember.LeftAt = null;
            }
            else
            {
                var newMember = new ProjectMember
                {
                    ProjectId = projectId,
                    UserId = user.Id,
                    ProjectRole = resolvedProjectRole,
                    JoinedAt = now,
                    Status = false
                };
                _context.ProjectMembers.Add(newMember);
            }

            var activeInviteTokens = await _context.RefreshTokens
                .Where(token => token.UserId == user.Id &&
                                token.DeviceId == "Invite" &&
                                !token.IsRevoked)
                .ToListAsync();

            foreach (var inviteToken in activeInviteTokens)
            {
                inviteToken.IsRevoked = true;
            }

            var rawInviteToken = GenerateInviteToken();
            var inviteTokenHash = HashToken(rawInviteToken);
            _context.RefreshTokens.Add(new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = inviteTokenHash,
                DeviceId = "Invite",
                ExpiryTime = now.AddDays(7),
                IsRevoked = false
            });

            await _context.SaveChangesAsync();

            var acceptUrl = BuildInviteUrl(rawInviteToken);
            await _emailService.SendInviteEmailAsync(
                normalizedEmail,
                user.FullName,
                inviterName,
                "SprintA",
                project.Name,
                acceptUrl,
                request.InviteMessage);
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

        private string BuildInviteUrl(string rawInviteToken)
        {
            var frontendBaseUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:5173";
            return $"{frontendBaseUrl.TrimEnd('/')}/accept-invite?token={Uri.EscapeDataString(rawInviteToken)}";
        }

        private static string GenerateInviteToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(48);
            return Convert.ToBase64String(bytes)
                .Replace("+", "-", StringComparison.Ordinal)
                .Replace("/", "_", StringComparison.Ordinal)
                .TrimEnd('=');
        }

        private static string HashToken(string token)
        {
            var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToHexString(hashBytes);
        }

        private static string BuildNameFromEmail(string email)
        {
            var localPart = email.Split('@')[0];
            var words = localPart
                .Split(new[] { '.', '_', '-' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(word => !string.IsNullOrWhiteSpace(word))
                .Select(word => word.Length == 1
                    ? char.ToUpperInvariant(word[0]).ToString()
                    : char.ToUpperInvariant(word[0]) + word[1..]);

            return string.Join(' ', words);
        }
    }
}
