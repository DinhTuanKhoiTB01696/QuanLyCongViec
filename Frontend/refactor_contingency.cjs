const fs = require('fs');
const path = require('path');
const file = 'c:/Users/tua46/OneDrive/Máy tính/DATN_2/QuanLyCongViec/QuanLyCongViec/Frontend/src/components/TaskDetailModal.vue';
let content = fs.readFileSync(file, 'utf8');

const htmlContent = fs.readFileSync('contingency_html.txt', 'utf8');
const modalsContent = fs.readFileSync('contingency_modals.txt', 'utf8');
const jsContent = fs.readFileSync('contingency_js.txt', 'utf8');

// 1. Replace HTML
const htmlStartStr = `<div class="mb-6 contingency-plans-container" v-else-if="activityTab === 'contingency'">`;
const htmlEndStr = `<div v-else-if="activityTab !== 'contingency'" class="activity-empty-state">`;
const htmlStartIdx = content.indexOf(htmlStartStr);
const htmlEndIdx = content.indexOf(htmlEndStr);

if (htmlStartIdx !== -1 && htmlEndIdx !== -1) {
    // Keep everything before startIdx, add new HTML, add the end element and everything after
    content = content.substring(0, htmlStartIdx) + htmlContent + '\n            ' + content.substring(htmlEndIdx);
    console.log('Replaced HTML tab container.');
} else {
    console.log('Failed to find HTML bounds:', htmlStartIdx, htmlEndIdx);
}

// 2. Append Modals before last </template>
const lastTemplateIdx = content.lastIndexOf('</template>');
if (lastTemplateIdx !== -1) {
    content = content.substring(0, lastTemplateIdx) + '\n' + modalsContent + '\n' + content.substring(lastTemplateIdx);
    console.log('Appended Modals to template.');
} else {
    console.log('Failed to find last </template>');
}

// 3. Replace JS logic
const jsStartStr = '// Contingency Plan logic';
const jsEndStr = '// Comments logic';
const jsStartIdx = content.indexOf(jsStartStr);
const jsEndIdx = content.indexOf(jsEndStr);

if (jsStartIdx !== -1 && jsEndIdx !== -1) {
    content = content.substring(0, jsStartIdx) + jsContent + '\n\n' + content.substring(jsEndIdx);
    console.log('Replaced JS Logic.');
} else {
    console.log('Failed to find JS logic bounds:', jsStartIdx, jsEndIdx);
}

// 4. Reset states on watcher
const resetStateTarget = `contingencyPlans.value = [];
      showContingencyForm.value = false;
      contingencyPlanForm.value = { name: '', riskLevel: 'Low', riskStatus: 'Safe', activationCondition: '', notes: '' };`;
const resetStateNew = `contingencyPlans.value = [];
      showContingencyForm.value = false;
      showContingencyDetail.value = false;
      viewingContingencyPlan.value = null;
      contingencyPlanForm.value = { name: '', riskLevel: 'Low', riskStatus: 'Safe', activationCondition: '', notes: '', contingencyTaskId: null };`;
content = content.replace(resetStateTarget, resetStateNew);

fs.writeFileSync(file, content, 'utf8');
console.log('Done rewriting TaskDetailModal.vue');
