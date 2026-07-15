const fs = require('fs');
const file = 'c:/Users/tua46/OneDrive/Máy tính/DATN_2/QuanLyCongViec/QuanLyCongViec/Frontend/src/components/TaskDetailModal.vue';
let content = fs.readFileSync(file, 'utf8');

const matches = content.match(/<!-- Contingency Plan Create\/Edit Dialog -->/g);
console.log('Number of Modals found:', matches ? matches.length : 0);

const htmlMatches = content.match(/<div class="mb-6 contingency-plans-container" v-else-if="activityTab === 'contingency'">/g);
console.log('Number of Tab Containers found:', htmlMatches ? htmlMatches.length : 0);
