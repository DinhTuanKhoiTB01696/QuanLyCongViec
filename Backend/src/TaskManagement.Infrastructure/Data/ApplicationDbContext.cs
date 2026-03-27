using Microsoft.EntityFrameworkCore;
using TaskManagement.Domain.Entities;
using TaskStatus = TaskManagement.Domain.Entities.TaskStatus;

namespace TaskManagement.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        { }

        // Group 1: System & Access
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }

        // Group 2: Organization
        public DbSet<Department> Departments { get; set; }
        public DbSet<DepartmentMember> DepartmentMembers { get; set; }

        // Group 3: Core Work Management
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectMember> ProjectMembers { get; set; }
        public DbSet<Sprint> Sprints { get; set; }
        public DbSet<TaskType> TaskTypes { get; set; }
        public DbSet<TaskManagement.Domain.Entities.TaskStatus> TaskStatuses { get; set; }
        public DbSet<WorkTask> WorkTasks { get; set; }
        public DbSet<TaskAssignment> TaskAssignments { get; set; }
        public DbSet<TaskDependency> TaskDependencies { get; set; }

        // Group 4: Collaboration & Tracking
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        // Group 5: Gamification
        public DbSet<PerformanceReview> PerformanceReviews { get; set; }
        public DbSet<UserWallet> UserWallets { get; set; }
        public DbSet<PointTransaction> PointTransactions { get; set; }

        // Group 6: AI Integration
        public DbSet<AIPromptTemplate> AIPromptTemplates { get; set; }
        public DbSet<AITokenUsage> AITokenUsages { get; set; }
        public DbSet<AIFeedback> AIFeedbacks { get; set; }
        public DbSet<AITrainingDataset> AITrainingDatasets { get; set; }
        public DbSet<TaskVectorEmbedding> TaskVectorEmbeddings { get; set; }

        // Group 6: Time Tracking
        public DbSet<TimeLog> TimeLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =============================================
            // 1. Composite Keys & Special PKs
            // =============================================
            modelBuilder.Entity<UserRole>().HasKey(x => new { x.UserId, x.RoleId });
            modelBuilder.Entity<RolePermission>().HasKey(x => new { x.RoleId, x.PermissionId });
            modelBuilder.Entity<DepartmentMember>().HasKey(x => new { x.DepartmentId, x.UserId });
            modelBuilder.Entity<ProjectMember>().HasKey(x => new { x.ProjectId, x.UserId });
            modelBuilder.Entity<TaskAssignment>().HasKey(x => new { x.WorkTaskId, x.UserId });
            modelBuilder.Entity<TaskDependency>().HasKey(x => new { x.PredecessorTaskId, x.SuccessorTaskId });
            modelBuilder.Entity<UserWallet>().HasKey(x => x.UserId);
            modelBuilder.Entity<TaskVectorEmbedding>().HasKey(x => x.WorkTaskId);

            // =============================================
            // 2. Unique Constraints
            // =============================================
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<Permission>().HasIndex(p => p.Code).IsUnique();

            // =============================================
            // 3. Relationships - Group 1: System & Access
            // =============================================
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            // =============================================
            // 4. Relationships - Group 2: Organization
            // =============================================
            modelBuilder.Entity<Department>()
                .HasOne(d => d.Manager)
                .WithMany(u => u.ManagedDepartments)
                .HasForeignKey(d => d.ManagerId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<DepartmentMember>()
                .HasOne(dm => dm.Department)
                .WithMany(d => d.DepartmentMembers)
                .HasForeignKey(dm => dm.DepartmentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DepartmentMember>()
                .HasOne(dm => dm.User)
                .WithMany(u => u.DepartmentMemberships)
                .HasForeignKey(dm => dm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // =============================================
            // 5. Relationships - Group 3: Core Work Management
            // =============================================
            modelBuilder.Entity<Project>()
                .HasOne(p => p.Creator)
                .WithMany(u => u.CreatedProjects)
                .HasForeignKey(p => p.CreatorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProjectMember>()
                .HasOne(pm => pm.Project)
                .WithMany(p => p.ProjectMembers)
                .HasForeignKey(pm => pm.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProjectMember>()
                .HasOne(pm => pm.User)
                .WithMany(u => u.ProjectMemberships)
                .HasForeignKey(pm => pm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Sprint>()
                .HasOne(s => s.Project)
                .WithMany(p => p.Sprints)
                .HasForeignKey(s => s.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaskType>()
                .HasOne(tt => tt.Project)
                .WithMany(p => p.TaskTypes)
                .HasForeignKey(tt => tt.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaskStatus>()
                .HasOne(ts => ts.Project)
                .WithMany(p => p.TaskStatuses)
                .HasForeignKey(ts => ts.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WorkTask>()
                .HasOne(wt => wt.Project)
                .WithMany(p => p.WorkTasks)
                .HasForeignKey(wt => wt.ProjectId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<WorkTask>()
                .HasOne(wt => wt.Sprint)
                .WithMany(s => s.WorkTasks)
                .HasForeignKey(wt => wt.SprintId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<WorkTask>()
                .HasOne(wt => wt.ParentTask)
                .WithMany(wt => wt.ChildTasks)
                .HasForeignKey(wt => wt.ParentTaskId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WorkTask>()
                .HasOne(wt => wt.TaskType)
                .WithMany(tt => tt.WorkTasks)
                .HasForeignKey(wt => wt.TaskTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WorkTask>()
                .HasOne(wt => wt.TaskStatus)
                .WithMany(ts => ts.WorkTasks)
                .HasForeignKey(wt => wt.TaskStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WorkTask>()
                .HasOne(wt => wt.Reporter)
                .WithMany(u => u.ReportedTasks)
                .HasForeignKey(wt => wt.ReporterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskAssignment>()
                .HasOne(ta => ta.WorkTask)
                .WithMany(wt => wt.TaskAssignments)
                .HasForeignKey(ta => ta.WorkTaskId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaskAssignment>()
                .HasOne(ta => ta.User)
                .WithMany(u => u.TaskAssignments)
                .HasForeignKey(ta => ta.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskDependency>()
                .HasOne(td => td.PredecessorTask)
                .WithMany(wt => wt.SuccessorDependencies)
                .HasForeignKey(td => td.PredecessorTaskId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskDependency>()
                .HasOne(td => td.SuccessorTask)
                .WithMany(wt => wt.PredecessorDependencies)
                .HasForeignKey(td => td.SuccessorTaskId)
                .OnDelete(DeleteBehavior.Cascade);

            // =============================================
            // 6. Relationships - Group 4: Collaboration & Tracking
            // =============================================
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.WorkTask)
                .WithMany(wt => wt.Comments)
                .HasForeignKey(c => c.WorkTaskId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.ParentComment)
                .WithMany(pc => pc.ChildComments)
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Attachment>()
                .HasOne(a => a.WorkTask)
                .WithMany(wt => wt.Attachments)
                .HasForeignKey(a => a.WorkTaskId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Attachment>()
                .HasOne(a => a.User)
                .WithMany(u => u.Attachments)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AuditLog>()
                .HasOne(al => al.WorkTask)
                .WithMany(wt => wt.AuditLogs)
                .HasForeignKey(al => al.WorkTaskId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AuditLog>()
                .HasOne(al => al.User)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(al => al.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // =============================================
            // 7. Relationships - Group 5: Gamification
            // =============================================
            modelBuilder.Entity<PerformanceReview>()
                .HasOne(pr => pr.Reviewer)
                .WithMany(u => u.ReviewsGiven)
                .HasForeignKey(pr => pr.ReviewerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PerformanceReview>()
                .HasOne(pr => pr.Reviewee)
                .WithMany(u => u.ReviewsReceived)
                .HasForeignKey(pr => pr.RevieweeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserWallet>()
                .HasOne(uw => uw.User)
                .WithOne(u => u.Wallet)
                .HasForeignKey<UserWallet>(uw => uw.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PointTransaction>()
                .HasOne(pt => pt.UserWallet)
                .WithMany(uw => uw.PointTransactions)
                .HasForeignKey(pt => pt.UserWalletUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // =============================================
            // 8. Relationships - Group 6: AI Integration
            // =============================================
            modelBuilder.Entity<AITokenUsage>()
                .HasOne(tu => tu.User)
                .WithMany(u => u.AITokenUsages)
                .HasForeignKey(tu => tu.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AIFeedback>()
                .HasOne(f => f.User)
                .WithMany(u => u.AIFeedbacks)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AITrainingDataset>()
                .HasOne(td => td.Creator)
                .WithMany(u => u.AITrainingDatasets)
                .HasForeignKey(td => td.CreatorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskVectorEmbedding>()
                .HasOne(tve => tve.WorkTask)
                .WithOne(wt => wt.TaskVectorEmbedding)
                .HasForeignKey<TaskVectorEmbedding>(tve => tve.WorkTaskId)
                .OnDelete(DeleteBehavior.Cascade);

            // =============================================
            // 9. Relationships - Time Tracking
            // =============================================
            modelBuilder.Entity<TimeLog>()
                .HasOne(tl => tl.WorkTask)
                .WithMany(wt => wt.TimeLogs)
                .HasForeignKey(tl => tl.WorkTaskId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TimeLog>()
                .HasOne(tl => tl.User)
                .WithMany(u => u.TimeLogs)
                .HasForeignKey(tl => tl.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // 1. Timezone Handling: Strictly save dates as UTC
            var dateTimeProperties = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
                .SelectMany(e => e.Properties)
                .Where(p => p.Metadata.ClrType == typeof(DateTime) || p.Metadata.ClrType == typeof(DateTime?));

            foreach (var prop in dateTimeProperties)
            {
                if (prop.CurrentValue is DateTime dt && dt.Kind != DateTimeKind.Utc)
                {
                    prop.CurrentValue = dt.ToUniversalTime();
                }
            }

            // 2. Automatic Data Roll-up logic preparation
            var timeLogEntries = ChangeTracker.Entries<TimeLog>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
                .ToList();

            // Save the logs first
            var result = await base.SaveChangesAsync(cancellationToken);

            // 3. Automatic Data Roll-up execution (Update Tasks and Parent Tasks)
            if (timeLogEntries.Any())
            {
                var taskIds = timeLogEntries.Select(e => e.Entity.WorkTaskId).Distinct().ToList();
                var tasks = await WorkTasks.Where(t => taskIds.Contains(t.Id)).ToListAsync(cancellationToken);

                foreach (var task in tasks)
                {
                    task.TotalActualHours = await TimeLogs.Where(tl => tl.WorkTaskId == task.Id).SumAsync(tl => tl.Hours, cancellationToken);
                }
                await base.SaveChangesAsync(cancellationToken);

                var parentIds = tasks.Where(t => t.ParentTaskId.HasValue).Select(t => t.ParentTaskId.Value).Distinct().ToList();
                if (parentIds.Any())
                {
                    var parents = await WorkTasks.Where(t => parentIds.Contains(t.Id)).ToListAsync(cancellationToken);
                    foreach (var parent in parents)
                    {
                        parent.TotalActualHours = await WorkTasks.Where(t => t.ParentTaskId == parent.Id).SumAsync(t => t.TotalActualHours, cancellationToken);
                    }
                    await base.SaveChangesAsync(cancellationToken);
                }
            }

            return result;
        }
    }
}