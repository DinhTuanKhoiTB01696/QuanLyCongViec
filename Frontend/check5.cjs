const fs = require('fs');
const file = './src/components/TaskDetailModal.vue';
let content = fs.readFileSync(file, 'utf8');
const htmlStartStr = '<div class="mb-6 contingency-plans-container" v-else-if="activityTab === \'contingency\'">';
const htmlEndStr = '<div v-else-if="activityTab !== \'contingency\'" class="activity-empty-state">';
const htmlStartIdx = content.indexOf(htmlStartStr);
const htmlEndIdx = content.indexOf(htmlEndStr);
console.log('htmlStartIdx:', htmlStartIdx, 'htmlEndIdx:', htmlEndIdx);
if (htmlStartIdx > -1 && htmlEndIdx > -1) {
   console.log(content.substring(htmlStartIdx, htmlEndIdx));
}
