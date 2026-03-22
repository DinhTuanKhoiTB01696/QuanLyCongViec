using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Domain.Entities;
using TaskStatus = TaskManagement.Domain.Entities.TaskStatus;

namespace TaskManagement.Infrastructure.Data
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // Seed Roles
            if (!await context.Roles.AnyAsync())
            {
                var roles = new List<Role>
                {
                    new Role { Id = Guid.Parse("22222222-0000-0000-0000-000000000001"), Name = "PM", Description = "Project Manager" },
                    new Role { Id = Guid.Parse("22222222-0000-0000-0000-000000000002"), Name = "PO", Description = "Product Owner" },
                    new Role { Id = Guid.Parse("22222222-0000-0000-0000-000000000003"), Name = "Developer", Description = "Developer" },
                    new Role { Id = Guid.Parse("22222222-0000-0000-0000-000000000008"), Name = "System Admin", Description = "System Admin" }
                };
                context.Roles.AddRange(roles);
                await context.SaveChangesAsync();
            }

            // 1. Get or Create User 1
            var user1 = await context.Users.Include(u => u.UserRoles).FirstOrDefaultAsync(u => u.Email == "user1@test.com");
            if (user1 == null)
            {
                user1 = new User
                {
                    Id = Guid.Parse("11111111-0000-0000-0000-000000000001"),
                    Email = "user1@test.com",
                    FullName = "User 1 (PM)",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };
                context.Users.Add(user1);
                await context.SaveChangesAsync();
            }

            // 2. Get or Create User 8
            var user8 = await context.Users.Include(u => u.UserRoles).FirstOrDefaultAsync(u => u.Email == "user8@test.com");
            if (user8 == null)
            {
                user8 = new User
                {
                    Id = Guid.Parse("11111111-0000-0000-0000-000000000008"),
                    Email = "user8@test.com",
                    FullName = "User 8 (Admin)",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };
                context.Users.Add(user8);
                await context.SaveChangesAsync();
            }

            // Ensure Roles Assigned
            if (!user1.UserRoles.Any()) {
                context.UserRoles.Add(new UserRole { UserId = user1.Id, RoleId = Guid.Parse("22222222-0000-0000-0000-000000000001") });
            }
            if (!user8.UserRoles.Any()) {
                context.UserRoles.Add(new UserRole { UserId = user8.Id, RoleId = Guid.Parse("22222222-0000-0000-0000-000000000008") });
            }
            await context.SaveChangesAsync();

            // 3. Get or Create Project
            var mainProjectId = Guid.Parse("55555555-0000-0000-0000-000000000001");
            var project = await context.Projects.FirstOrDefaultAsync(p => p.Id == mainProjectId);
            if (project == null)
            {
                project = new Project
                {
                    Id = mainProjectId,
                    Name = "Dự án mẫu Test Kanban",
                    Description = "Dự án để kiểm tra các tính năng của hệ thống",
                    Status = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatorId = user1.Id
                };
                context.Projects.Add(project);
                await context.SaveChangesAsync();

                // Seed Default Types & Statuses
                context.TaskTypes.Add(new TaskType { Id = Guid.NewGuid(), ProjectId = project.Id, Name = "Task", ColorCode = "#FFFFFF", Icon = "fa-solid fa-list-check" });
                context.TaskStatuses.Add(new TaskStatus { Id = Guid.NewGuid(), ProjectId = project.Id, Name = "TO DO", Position = 1, ColorCode = "#3b82f6" });
                context.TaskStatuses.Add(new TaskStatus { Id = Guid.NewGuid(), ProjectId = project.Id, Name = "IN PROGRESS", Position = 2, ColorCode = "#a855f7" });
                context.TaskStatuses.Add(new TaskStatus { Id = Guid.NewGuid(), ProjectId = project.Id, Name = "DONE", Position = 3, ColorCode = "#22c55e" });
                await context.SaveChangesAsync();
            }

            // 4. Ensure Memberships are active using REAL IDs
            var member1 = await context.ProjectMembers.FirstOrDefaultAsync(pm => pm.ProjectId == project.Id && pm.UserId == user1.Id);
            if (member1 == null)
            {
                context.ProjectMembers.Add(new ProjectMember { 
                    ProjectId = project.Id, 
                    UserId = user1.Id, 
                    ProjectRole = "PM", 
                    JoinedAt = DateTime.UtcNow, 
                    Status = true 
                });
            }
            else if (!member1.Status)
            {
                member1.Status = true;
            }

            var member8 = await context.ProjectMembers.FirstOrDefaultAsync(pm => pm.ProjectId == project.Id && pm.UserId == user8.Id);
            if (member8 == null)
            {
                context.ProjectMembers.Add(new ProjectMember { 
                    ProjectId = project.Id, 
                    UserId = user8.Id, 
                    ProjectRole = "Admin", 
                    JoinedAt = DateTime.UtcNow, 
                    Status = true 
                });
            }
            else if (!member8.Status)
            {
                member8.Status = true;
            }

            await context.SaveChangesAsync();
        }
    }
}
