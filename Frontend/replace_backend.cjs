const fs = require('fs');
const file = '../Backend/src/TaskManagement.Infrastructure/Services/WorkTaskService.cs';
let content = fs.readFileSync(file, 'utf8');

const startStr = 'public async Task ActivateContingencyTaskAsync(Guid taskId, Guid planId, Guid contingencyTaskId, Guid userId)';
const endStr = 'await _context.SaveChangesAsync();\r\n        }\r\n    }\r\n}';

let startIdx = content.indexOf(startStr);
let endIdx = content.indexOf(endStr, startIdx);
if (endIdx === -1) {
    const endStrMac = 'await _context.SaveChangesAsync();\n        }\n    }\n}';
    endIdx = content.indexOf(endStrMac, startIdx);
    if (endIdx !== -1) {
        endStr = endStrMac;
    }
}

if (startIdx === -1 || endIdx === -1) {
    console.error('Bounds not found');
    process.exit(1);
}

const newFunc = `public async Task ActivateContingencyTaskAsync(Guid taskId, Guid planId, Guid contingencyTaskId, Guid userId)
        {
            var plan = await _context.ContingencyPlans.FirstOrDefaultAsync(p => p.Id == planId && p.WorkTaskId == taskId && !p.IsDeleted);
            if (plan == null) throw new Exception("Contingency plan not found");
            
            var link = await _context.ContingencyPlanTasks.FirstOrDefaultAsync(l => l.Id == contingencyTaskId && l.ContingencyPlanId == planId);
            if (link == null) throw new Exception("Contingency task not found");
            
            if (link.IsActivated) throw new Exception("Contingency task is already activated");

            link.IsActivated = true;
            link.StatusName = "IN PROGRESS";
            link.ActivatedById = userId;
            link.ActivatedAt = DateTime.UtcNow;

            var auditLog = new TaskManagement.Domain.Entities.AuditLog
            {
                Id = Guid.NewGuid(),
                WorkTaskId = taskId,
                UserId = userId,
                FieldChanged = "ContingencyTask_Activated",
                NewValue = contingencyTaskId.ToString(),
                CreatedAt = DateTime.UtcNow
            };
            _context.AuditLogs.Add(auditLog);

            // Add issue/blocked logic to main task
            var mainTask = await _context.WorkTasks.FirstOrDefaultAsync(t => t.Id == taskId && !t.IsDeleted);
            if (mainTask != null)
            {
                _context.AuditLogs.Add(new TaskManagement.Domain.Entities.AuditLog
                {
                    Id = Guid.NewGuid(),
                    WorkTaskId = mainTask.Id,
                    UserId = userId,
                    FieldChanged = "System_Issue_Reported",
                    NewValue = "Contingency Triggered",
                    CreatedAt = DateTime.UtcNow
                });
                
                // Create a real WorkTask for this contingency task
                var taskStatus = await _context.TaskStatuses.FirstOrDefaultAsync(ts => ts.Name == "IN PROGRESS" && ts.ProjectId == mainTask.ProjectId);
                if (taskStatus == null) taskStatus = await _context.TaskStatuses.FirstOrDefaultAsync(ts => ts.ProjectId == mainTask.ProjectId);
                
                var taskType = await _context.TaskTypes.FirstOrDefaultAsync(tt => tt.ProjectId == mainTask.ProjectId);
                
                double maxSort = 0;
                var existingMax = await _context.WorkTasks
                    .Where(wt => wt.ProjectId == mainTask.ProjectId && !wt.IsDeleted)
                    .MaxAsync(wt => (double?)wt.SortOrder);
                if (existingMax.HasValue) maxSort = existingMax.Value + 65536;
                else maxSort = 65536;

                var newWorkTask = new TaskManagement.Domain.Entities.WorkTask
                {
                    Id = Guid.NewGuid(),
                    ProjectId = mainTask.ProjectId,
                    Title = "[DỰ PHÒNG] " + link.Title,
                    Description = link.Description,
                    StatusId = taskStatus?.Id ?? Guid.Empty,
                    TaskTypeId = taskType?.Id ?? Guid.Empty,
                    Priority = link.Priority,
                    ReporterId = userId,
                    ParentTaskId = taskId,
                    SortOrder = maxSort,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                if (link.AssigneeId.HasValue)
                {
                    var assignment = new TaskManagement.Domain.Entities.TaskAssignment
                    {
                        Id = Guid.NewGuid(),
                        WorkTaskId = newWorkTask.Id,
                        UserId = link.AssigneeId.Value,
                        Status = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.TaskAssignments.Add(assignment);
                }

                _context.WorkTasks.Add(newWorkTask);
            }

            await _context.SaveChangesAsync();
        }
    }
}`;

content = content.substring(0, startIdx) + newFunc;
fs.writeFileSync(file, content, 'utf8');
