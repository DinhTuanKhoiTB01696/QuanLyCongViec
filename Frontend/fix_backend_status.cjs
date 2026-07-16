const fs = require('fs');
const file = '../Backend/src/TaskManagement.Infrastructure/Services/WorkTaskService.cs';
let content = fs.readFileSync(file, 'utf8');

content = content.replace('StatusId = taskStatus?.Id ?? Guid.Empty,', 'TaskStatusId = taskStatus?.Id ?? Guid.Empty,');
fs.writeFileSync(file, content, 'utf8');
