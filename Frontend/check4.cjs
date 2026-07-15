const fs = require('fs');
const file = './src/components/TaskDetailModal.vue';
let content = fs.readFileSync(file, 'utf8');
const htmlStartStr = '<div class="mb-6 contingency-plans-container" v-else-if="activityTab === \'contingency\'">';
const htmlEndStr = '<div v-else-if="activityTab !== \'contingency\'" class="activity-empty-state">';
const htmlStartIdx = content.indexOf(htmlStartStr);
const htmlEndIdx = content.indexOf(htmlEndStr);
const chunk = content.substring(htmlStartIdx, htmlEndIdx);
console.log('Original chunk divs:', (chunk.match(/<div/g) || []).length, 'open,', (chunk.match(/<\/div>/g) || []).length, 'close');

const htmlContent = fs.readFileSync('contingency_html.txt', 'utf8');
console.log('New chunk divs:', (htmlContent.match(/<div/g) || []).length, 'open,', (htmlContent.match(/<\/div>/g) || []).length, 'close');
