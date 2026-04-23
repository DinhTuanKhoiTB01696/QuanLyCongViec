using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Infrastructure.Data
{
    public static class DataSeeder
    {
        public static async Task SeedMockDataAsync(ApplicationDbContext context)
        {
            var now = DateTime.UtcNow;
            var preferredOwnerId = Guid.Parse("11111111-0000-0000-0000-000000000001");
            const string demoWorkspaceSlug = "cybwf";
            const string demoProjectIdentifier = "CYBWF";
            const string demoProjectName = "Demo Plane Project";
            var standardRoles = new[]
            {
                new { Name = "Admin", Description = "System Administrator" },
                new { Name = "PM", Description = "Project Manager" },
                new { Name = "Project Lead", Description = "Project lead access" },
                new { Name = "PO", Description = "Product Owner" },
                new { Name = "Developer", Description = "Developer access" },
                new { Name = "QA", Description = "Quality Assurance" },
                new { Name = "Accountant", Description = "Accounting access" }
            };

            foreach (var roleSeed in standardRoles)
            {
                if (!await context.Roles.AnyAsync(role => role.Name == roleSeed.Name))
                {
                    context.Roles.Add(new Role
                    {
                        Id = Guid.NewGuid(),
                        Name = roleSeed.Name,
                        Description = roleSeed.Description
                    });
                }
            }

            if (context.ChangeTracker.HasChanges())
            {
                await context.SaveChangesAsync();
            }

            var owner = await context.Users.FirstOrDefaultAsync(u => u.Id == preferredOwnerId)
                ?? await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@example.com");

            if (owner == null)
            {
                owner = new User
                {
                    Id = preferredOwnerId,
                    FullName = "Admin (Seeded)",
                    Email = "admin@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                context.Users.Add(owner);
                await context.SaveChangesAsync();
            }

            var testUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "test@example.com");
            if (testUser == null)
            {
                testUser = new User
                {
                    Id = Guid.NewGuid(),
                    FullName = "Test User",
                    Email = "test@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@123"),
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                context.Users.Add(testUser);
                await context.SaveChangesAsync();
            }

            var workspaceWasCreated = false;
            var workspace = await context.Workspaces
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(w => w.Slug == demoWorkspaceSlug);
            if (workspace == null)
            {
                workspaceWasCreated = true;
                workspace = new Workspace
                {
                    Id = Guid.NewGuid(),
                    Name = "Cybwf Workspace",
                    Slug = demoWorkspaceSlug,
                    OwnerId = owner.Id,
                    Timezone = "Asia/Ho_Chi_Minh",
                    CreatedAt = now,
                    UpdatedAt = now
                };
                context.Workspaces.Add(workspace);
                await context.SaveChangesAsync();
            }

            var ownerWorkspaceMember = await context.WorkspaceMembers
                .FirstOrDefaultAsync(m => m.WorkspaceId == workspace.Id && m.UserId == owner.Id);
            if (ownerWorkspaceMember == null)
            {
                context.WorkspaceMembers.Add(new WorkspaceMember
                {
                    WorkspaceId = workspace.Id,
                    UserId = owner.Id,
                    WorkspaceRole = "OWNER",
                    JoinedAt = now,
                    IsActive = true
                });
            }

            var testWorkspaceMember = await context.WorkspaceMembers
                .FirstOrDefaultAsync(m => m.WorkspaceId == workspace.Id && m.UserId == testUser.Id);
            if (testWorkspaceMember == null)
            {
                context.WorkspaceMembers.Add(new WorkspaceMember
                {
                    WorkspaceId = workspace.Id,
                    UserId = testUser.Id,
                    WorkspaceRole = "MEMBER",
                    JoinedAt = now,
                    IsActive = true
                });
            }

            if (context.ChangeTracker.HasChanges())
            {
                await context.SaveChangesAsync();
            }

            var project = await context.Projects
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p =>
                    p.WorkspaceId == workspace.Id
                    && (p.Identifier == demoProjectIdentifier || p.Name == demoProjectName));
            if (project == null)
            {
                if (!workspaceWasCreated)
                {
                    return;
                }

                project = new Project
                {
                    Id = Guid.NewGuid(),
                    WorkspaceId = workspace.Id,
                    CreatorId = owner.Id,
                    Identifier = demoProjectIdentifier,
                    IssueSequence = 10,
                    NetworkType = "Public",
                    Name = demoProjectName,
                    Description = "A sample project to test the Plane-like UI",
                    Status = true,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                context.Projects.Add(project);
                await context.SaveChangesAsync();
            }
            else if (project.IsDeleted)
            {
                return;
            }
            else if (project.CreatorId == Guid.Empty)
            {
                project.CreatorId = owner.Id;
                project.UpdatedAt = now;
                await context.SaveChangesAsync();
            }

            var ownerProjectMember = await context.ProjectMembers
                .FirstOrDefaultAsync(m => m.ProjectId == project.Id && m.UserId == owner.Id);
            if (ownerProjectMember == null)
            {
                context.ProjectMembers.Add(new ProjectMember
                {
                    ProjectId = project.Id,
                    UserId = owner.Id,
                    ProjectRole = "PM",
                    JoinedAt = now,
                    Status = true
                });
            }

            var testProjectMember = await context.ProjectMembers
                .FirstOrDefaultAsync(m => m.ProjectId == project.Id && m.UserId == testUser.Id);
            if (testProjectMember == null)
            {
                context.ProjectMembers.Add(new ProjectMember
                {
                    ProjectId = project.Id,
                    UserId = testUser.Id,
                    ProjectRole = "PM",
                    JoinedAt = now,
                    Status = true
                });
            }

            if (context.ChangeTracker.HasChanges())
            {
                await context.SaveChangesAsync();
            }

            var statusBacklog = await EnsureTaskStatusAsync(context, project.Id, "BACKLOG", 0, "#64748b");
            var statusTodo = await EnsureTaskStatusAsync(context, project.Id, "TO DO", 1, "#3b82f6");
            var statusProgress = await EnsureTaskStatusAsync(context, project.Id, "IN PROGRESS", 2, "#f59e0b");
            var statusReview = await EnsureTaskStatusAsync(context, project.Id, "IN REVIEW", 3, "#8b5cf6");
            var statusDone = await EnsureTaskStatusAsync(context, project.Id, "DONE", 4, "#10b981");

            var typeTask = await EnsureTaskTypeAsync(context, project.Id, "Task", "#3b82f6");
            var typeBug = await EnsureTaskTypeAsync(context, project.Id, "Bug", "#ef4444");

            if (context.ChangeTracker.HasChanges())
            {
                await context.SaveChangesAsync();
            }

            await EnsureTaskAsync(
                context,
                project.Id,
                workspace.Id,
                typeTask.Id,
                statusDone.Id,
                owner.Id,
                "CYBWF-1",
                "Nghien cuu kien truc Plane",
                "Phan tich schema database",
                3,
                10000,
                now);

            await EnsureTaskAsync(
                context,
                project.Id,
                workspace.Id,
                typeTask.Id,
                statusProgress.Id,
                owner.Id,
                "CYBWF-2",
                "Thiet ke giao dien Dark Mode",
                "#0D0D0D background",
                2,
                20000,
                now);

            await EnsureTaskAsync(
                context,
                project.Id,
                workspace.Id,
                typeBug.Id,
                statusReview.Id,
                owner.Id,
                "CYBWF-3",
                "Viet API Kanban Reorder",
                "Testing Lexorank",
                1,
                30000,
                now);

            await EnsureTaskAsync(
                context,
                project.Id,
                workspace.Id,
                typeTask.Id,
                statusTodo.Id,
                owner.Id,
                "CYBWF-4",
                "Len ke hoach Sprint 1",
                "Chuan bi backlog",
                4,
                40000,
                now);

            if (context.ChangeTracker.HasChanges())
            {
                await context.SaveChangesAsync();
            }
        }

        private static async Task<TaskManagement.Domain.Entities.TaskStatus> EnsureTaskStatusAsync(
            ApplicationDbContext context,
            Guid projectId,
            string name,
            int position,
            string colorCode)
        {
            var status = await context.TaskStatuses.FirstOrDefaultAsync(s => s.ProjectId == projectId && s.Name == name);
            if (status != null)
            {
                if (status.Position != position || string.IsNullOrWhiteSpace(status.ColorCode))
                {
                    status.Position = position;
                    status.ColorCode = colorCode;
                }

                return status;
            }

            status = new TaskManagement.Domain.Entities.TaskStatus
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                Name = name,
                Position = position,
                ColorCode = colorCode
            };

            context.TaskStatuses.Add(status);
            return status;
        }

        private static async Task<TaskType> EnsureTaskTypeAsync(
            ApplicationDbContext context,
            Guid projectId,
            string name,
            string colorCode)
        {
            var taskType = await context.TaskTypes.FirstOrDefaultAsync(t => t.ProjectId == projectId && t.Name == name);
            if (taskType != null)
            {
                if (string.IsNullOrWhiteSpace(taskType.ColorCode))
                {
                    taskType.ColorCode = colorCode;
                }

                return taskType;
            }

            taskType = new TaskType
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                Name = name,
                ColorCode = colorCode
            };

            context.TaskTypes.Add(taskType);
            return taskType;
        }

        private static async Task EnsureTaskAsync(
            ApplicationDbContext context,
            Guid projectId,
            Guid workspaceId,
            Guid taskTypeId,
            Guid taskStatusId,
            Guid reporterId,
            string sequenceId,
            string title,
            string description,
            int priority,
            double sortOrder,
            DateTime now)
        {
            var existingTask = await context.WorkTasks.FirstOrDefaultAsync(t => t.ProjectId == projectId && t.SequenceId == sequenceId);
            if (existingTask != null)
            {
                return;
            }

            context.WorkTasks.Add(new WorkTask
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                WorkspaceId = workspaceId,
                Title = title,
                Description = description,
                Priority = priority,
                TaskTypeId = taskTypeId,
                TaskStatusId = taskStatusId,
                ReporterId = reporterId,
                SortOrder = sortOrder,
                SequenceId = sequenceId,
                CreatedAt = now,
                UpdatedAt = now
            });
        }
    }
}
