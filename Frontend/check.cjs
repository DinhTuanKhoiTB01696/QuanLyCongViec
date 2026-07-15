const fs = require('fs');
const file = 'c:/Users/tua46/OneDrive/Máy tính/DATN_2/QuanLyCongViec/QuanLyCongViec/Frontend/src/components/TaskDetailModal.vue';
let content = fs.readFileSync(file, 'utf8');

const lines = content.split('\n');
for (let i = 0; i < lines.length; i++) {
    if (lines[i].includes('<!-- Contingency Plan Create/Edit Dialog -->')) {
        console.log('--- FOUND MODAL AT LINE ' + i + ' ---');
        console.log(lines.slice(i, i+15).join('\n'));
    }
}
