const fs = require('fs');
const file = 'Backend/src/TaskManagement.Infrastructure/Data/ApplicationDbContext.cs';
let content = fs.readFileSync(file, 'utf8');

if (!content.includes('DbSet<ContingencyPlan>')) {
  content = content.replace('public DbSet<TaskContingencyPlan> TaskContingencyPlans { get; set; }', 'public DbSet<TaskContingencyPlan> TaskContingencyPlans { get; set; }\r\n        public DbSet<ContingencyPlan> ContingencyPlans { get; set; }\r\n        public DbSet<ContingencyPlanTask> ContingencyPlanTasks { get; set; }');
}

const rel = `
            modelBuilder.Entity<ContingencyPlan>()
                .HasOne(cp => cp.WorkTask)
                .WithMany(wt => wt.ContingencyPlans)
                .HasForeignKey(cp => cp.WorkTaskId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ContingencyPlanTask>()
                .HasOne(cpt => cpt.ContingencyPlan)
                .WithMany(cp => cp.ContingencyPlanTasks)
                .HasForeignKey(cpt => cpt.ContingencyPlanId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ContingencyPlanTask>()
                .HasOne(cpt => cpt.WorkTask)
                .WithMany()
                .HasForeignKey(cpt => cpt.WorkTaskId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ContingencyPlanTask>()
                .HasOne(cpt => cpt.Assignee)
                .WithMany()
                .HasForeignKey(cpt => cpt.AssigneeId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ContingencyPlanTask>()
                .HasOne(cpt => cpt.ActivatedBy)
                .WithMany()
                .HasForeignKey(cpt => cpt.ActivatedById)
                .OnDelete(DeleteBehavior.SetNull);
`;

if (!content.includes('modelBuilder.Entity<ContingencyPlan>()')) {
  const insertionPoint = '            modelBuilder.Entity<TaskDependency>()';
  content = content.replace(insertionPoint, rel + '\r\n' + insertionPoint);
}

fs.writeFileSync(file, content);
console.log('Fixed ApplicationDbContext.cs');
