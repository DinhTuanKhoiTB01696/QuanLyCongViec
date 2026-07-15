const fs = require('fs');
let mainContent = fs.readFileSync('ApplicationDbContext_main.cs', 'utf8');

// Insert my DbSets
const dbsetsStr = '        public DbSet<WorkTask> WorkTasks { get; set; }\r\n        public DbSet<TaskContingencyPlan> TaskContingencyPlans { get; set; }';
mainContent = mainContent.replace(dbsetsStr, dbsetsStr + '\r\n        public DbSet<ContingencyPlan> ContingencyPlans { get; set; }\r\n        public DbSet<ContingencyPlanTask> ContingencyPlanTasks { get; set; }');

// Insert my relations
const relationsStr = '            modelBuilder.Entity<WorkTask>()\r\n                .HasOne(wt => wt.Reporter)\r\n                .WithMany(u => u.ReportedTasks)\r\n                .HasForeignKey(wt => wt.ReporterId)\r\n                .OnDelete(DeleteBehavior.Restrict);';

const myRelations = `
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

mainContent = mainContent.replace(relationsStr, relationsStr + '\r\n' + myRelations);

fs.writeFileSync('Backend/src/TaskManagement.Infrastructure/Data/ApplicationDbContext.cs', mainContent);
console.log('Merged ApplicationDbContext.cs successfully.');
