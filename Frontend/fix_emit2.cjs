const fs = require('fs');
const file = 'src/components/TaskDetailModal.vue';
let content = fs.readFileSync(file, 'utf8');

content = content.replace("emit('updated');", "emit('refresh-tasks');");

fs.writeFileSync(file, content, 'utf8');
