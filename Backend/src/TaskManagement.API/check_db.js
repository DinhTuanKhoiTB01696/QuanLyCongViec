const { execSync } = require('child_process');

try {
  const query = `
    SELECT Id, Name FROM Workspaces;
  `;
  const result = execSync(`sqlcmd -S .\\SQLEXPRESS01 -d TaskManagementDB -Q "${query}"`, { encoding: 'utf-8' });
  console.log(result);
} catch (e) {
  console.error(e.message);
}
