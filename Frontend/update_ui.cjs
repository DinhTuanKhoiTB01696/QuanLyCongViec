const fs = require('fs');
const file = 'c:/Users/tua46/OneDrive/Máy tính/DATN_2/QuanLyCongViec/QuanLyCongViec/Frontend/src/components/TaskDetailModal.vue';
let content = fs.readFileSync(file, 'utf8');

const htmlContent = fs.readFileSync('contingency_html.txt', 'utf8');
const modalsContent = fs.readFileSync('contingency_modals.txt', 'utf8');

// 1. Thay HTML
const htmlStartStr = '<div class="mb-6 contingency-plans-container" v-else-if="activityTab === \'contingency\'">';
const htmlEndStr = '<div v-else-if="activityTab !== \'contingency\'" class="activity-empty-state">';
const htmlStartIdx = content.indexOf(htmlStartStr);
const htmlEndIdx = content.indexOf(htmlEndStr);

if (htmlStartIdx !== -1 && htmlEndIdx !== -1) {
    content = content.substring(0, htmlStartIdx) + htmlContent + '\n            ' + content.substring(htmlEndIdx);
    console.log('Replaced HTML tab container.');
}

// 2. Thay Modal
const modalStartStr = '<!-- Contingency Plan Create/Edit Dialog -->';
const lastTemplateStr = '</template>';
const modalStartIdx = content.indexOf(modalStartStr);
const lastTemplateIdx = content.lastIndexOf(lastTemplateStr);

if (modalStartIdx !== -1 && lastTemplateIdx !== -1 && modalStartIdx < lastTemplateIdx) {
    content = content.substring(0, modalStartIdx) + modalsContent + '\n' + content.substring(lastTemplateIdx);
    console.log('Replaced Modals.');
}

fs.writeFileSync(file, content, 'utf8');
