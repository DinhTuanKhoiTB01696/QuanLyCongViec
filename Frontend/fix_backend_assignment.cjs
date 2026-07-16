const fs = require('fs');
const file = '../Backend/src/TaskManagement.Infrastructure/Services/WorkTaskService.cs';
let content = fs.readFileSync(file, 'utf8');

const oldAssignment = `var assignment = new TaskManagement.Domain.Entities.TaskAssignment
                    {
                        Id = Guid.NewGuid(),
                        WorkTaskId = newWorkTask.Id,
                        UserId = link.AssigneeId.Value,
                        Status = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };`;

const newAssignment = `var assignment = new TaskManagement.Domain.Entities.TaskAssignment
                    {
                        WorkTaskId = newWorkTask.Id,
                        UserId = link.AssigneeId.Value,
                        Status = true
                    };`;

content = content.replace(oldAssignment, newAssignment);
fs.writeFileSync(file, content, 'utf8');
