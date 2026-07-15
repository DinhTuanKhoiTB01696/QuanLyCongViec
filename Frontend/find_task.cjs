const fs = require('fs');
const lines = fs.readFileSync('src/components/TaskDetailModal.vue', 'utf8').split('\n');
const idx = lines.findIndex(l => l.includes('TASK DỰ PHÒNG'));
if (idx !== -1) {
    for(let i=idx-2; i<=idx+15; i++) console.log(lines[i]);
}
