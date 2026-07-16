const fs = require('fs');
const file = 'c:/Users/tua46/OneDrive/Máy tính/DATN_2/QuanLyCongViec/QuanLyCongViec/Frontend/src/components/TaskDetailModal.vue';
let content = fs.readFileSync(file, 'utf8');

content = content.replace(':z-index="4000"', ':z-index="9999999"');
content = content.replace(':z-index="4000"', ':z-index="9999999"');
fs.writeFileSync(file, content, 'utf8');
