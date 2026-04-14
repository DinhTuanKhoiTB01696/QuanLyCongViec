using Microsoft.EntityFrameworkCore;
using TaskManagement.Domain.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManagement.Infrastructure.Data
{
    public static class DataSeeder
    {
        public static async Task SeedMockDataAsync(ApplicationDbContext context)
        {
            // 1. Chỉ seed nếu chưa có project Demo. Hoặc kiểm tra nếu có Project mà không có Task.
            var defaultProject = await context.Projects.FirstOrDefaultAsync(p => p.Name == "Demo Plane Project");
            if (defaultProject != null)
            {
                bool hasTasks = await context.WorkTasks.AnyAsync(t => t.ProjectId == defaultProject.Id);
                if (hasTasks) return; // Đã có dữ liệu thì thôi
            }

            // --- BẮT ĐẦU SEED ---
            
            // 2. Tạo Workspace
            var workspaceId = Guid.NewGuid();
            var ownerId = Guid.Parse("11111111-0000-0000-0000-000000000001"); // Giả sử user này có sẵn từ migration

            var userExists = await context.Users.AnyAsync(u => u.Id == ownerId);
            if (!userExists)
            {
                ownerId = Guid.NewGuid();
                context.Users.Add(new User
                {
                    Id = ownerId,
                    FullName = "Admin (Seeded)",
                    Email = "admin@example.com",
                    PasswordHash = "AQAAAAEAACcQAAAAE...", // Dummy
                    IsActive = true
                });
            }

            var workspace = new Workspace
            {
                Id = workspaceId,
                Name = "Cybwf Workspace",
                Slug = "cybwf",
                OwnerId = ownerId,
                Timezone = "Asia/Ho_Chi_Minh",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Workspaces.Add(workspace);

            context.WorkspaceMembers.Add(new WorkspaceMember
            {
                WorkspaceId = workspaceId,
                UserId = ownerId,
                WorkspaceRole = "OWNER",
                JoinedAt = DateTime.UtcNow,
                IsActive = true
            });

            // 3. Tạo Project
            var projectId = Guid.NewGuid();
            var project = new Project
            {
                Id = projectId,
                WorkspaceId = workspaceId,
                Identifier = "CYBWF",
                IssueSequence = 10,
                NetworkType = "Public",
                Name = "Demo Plane Project",
                Description = "A sample project to test the Plane-like UI",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Projects.Add(project);

            context.ProjectMembers.Add(new ProjectMember
            {
                ProjectId = projectId,
                UserId = ownerId,
                ProjectRole = "PM",
                JoinedAt = DateTime.UtcNow,
                Status = true
            });

            // 4. Tạo Statuses & Types
            var statusTodo = new TaskManagement.Domain.Entities.TaskStatus { Id = Guid.NewGuid(), ProjectId = projectId, Name = "TO DO", Position = 1 };
            var statusProgress = new TaskManagement.Domain.Entities.TaskStatus { Id = Guid.NewGuid(), ProjectId = projectId, Name = "IN PROGRESS", Position = 2 };
            var statusReview = new TaskManagement.Domain.Entities.TaskStatus { Id = Guid.NewGuid(), ProjectId = projectId, Name = "IN REVIEW", Position = 3 };
            var statusDone = new TaskManagement.Domain.Entities.TaskStatus { Id = Guid.NewGuid(), ProjectId = projectId, Name = "DONE", Position = 4 };

            context.TaskStatuses.AddRange(statusTodo, statusProgress, statusReview, statusDone);

            var typeTask = new TaskType { Id = Guid.NewGuid(), ProjectId = projectId, Name = "Task", ColorCode = "#3b82f6" };
            var typeBug = new TaskType { Id = Guid.NewGuid(), ProjectId = projectId, Name = "Bug", ColorCode = "#ef4444" };
            context.TaskTypes.AddRange(typeTask, typeBug);

            // 5. Tạo WorkTasks mô phỏng (ÍT NHẤT 1 CHO MỖI TRẠNG THÁI)
            var tasks = new[]
            {
                new WorkTask
                {
                    Id = Guid.NewGuid(), ProjectId = projectId, WorkspaceId = workspaceId,
                    Title = "Nghiên cứu kiến trúc Plane", Description = "Phân tích schema database",
                    Priority = 3, TaskTypeId = typeTask.Id, TaskStatusId = statusDone.Id,
                    ReporterId = ownerId, SortOrder = 10000, SequenceId = "CYBWF-1", CreatedAt = DateTime.UtcNow
                },
                new WorkTask
                {
                    Id = Guid.NewGuid(), ProjectId = projectId, WorkspaceId = workspaceId,
                    Title = "Thiết kế giao diện Dark Mode", Description = "#0D0D0D background",
                    Priority = 2, TaskTypeId = typeTask.Id, TaskStatusId = statusProgress.Id,
                    ReporterId = ownerId, SortOrder = 20000, SequenceId = "CYBWF-2", CreatedAt = DateTime.UtcNow
                },
                new WorkTask
                {
                    Id = Guid.NewGuid(), ProjectId = projectId, WorkspaceId = workspaceId,
                    Title = "Viết API Kanban Reorder", Description = "Testing Lexorank",
                    Priority = 1, TaskTypeId = typeBug.Id, TaskStatusId = statusReview.Id,
                    ReporterId = ownerId, SortOrder = 30000, SequenceId = "CYBWF-3", CreatedAt = DateTime.UtcNow
                },
                new WorkTask
                {
                    Id = Guid.NewGuid(), ProjectId = projectId, WorkspaceId = workspaceId,
                    Title = "Lên kế hoạch Sprint 1", Description = "Chuẩn bị backlog",
                    Priority = 4, TaskTypeId = typeTask.Id, TaskStatusId = statusTodo.Id,
                    ReporterId = ownerId, SortOrder = 40000, SequenceId = "CYBWF-4", CreatedAt = DateTime.UtcNow
                }
            };
            
            context.WorkTasks.AddRange(tasks);
            await context.SaveChangesAsync();
        }
    }
}
