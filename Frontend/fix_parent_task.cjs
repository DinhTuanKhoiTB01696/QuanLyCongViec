const fs = require('fs');
const file = '../Backend/src/TaskManagement.Infrastructure/Services/WorkTaskService.cs';
let content = fs.readFileSync(file, 'utf8');

content = content.replace(
  'ParentTaskId = taskId,',
  'ParentTaskId = null,'
);

fs.writeFileSync(file, content, 'utf8');
