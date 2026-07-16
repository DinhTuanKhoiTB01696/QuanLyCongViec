const fs = require('fs');
const file = '../Backend/src/TaskManagement.Infrastructure/Services/WorkTaskService.cs';
let content = fs.readFileSync(file, 'utf8');

content = content.replace(
  '.OrderByDescending(p => p.CreatedAt)',
  '.OrderBy(p => p.CreatedAt)'
);

fs.writeFileSync(file, content, 'utf8');
