const fs = require('fs');
const file = 'c:/Users/tua46/OneDrive/Máy tính/DATN_2/QuanLyCongViec/QuanLyCongViec/Frontend/src/components/TaskDetailModal.vue';
let content = fs.readFileSync(file, 'utf8');

const htmlContent = fs.readFileSync('contingency_html.txt', 'utf8');
const modalsContent = fs.readFileSync('contingency_modals.txt', 'utf8');

// 1. Thay thế Tab Container
const htmlStart = 'class="mb-6 contingency-plans-container" v-else-if="activityTab === \'contingency\'"';
const htmlStartIdx = content.indexOf(htmlStart);

const htmlEnd = '<div v-else-if="activityTab !== \'contingency\'" class="activity-empty-state">';
const htmlEndIdx = content.indexOf(htmlEnd);

if (htmlStartIdx !== -1 && htmlEndIdx !== -1) {
    const realStart = content.lastIndexOf('<div ', htmlStartIdx);
    content = content.substring(0, realStart) + htmlContent + '\n            ' + content.substring(htmlEndIdx);
    console.log('Replaced Tab Container.');
} else {
    console.log('Tab Container NOT FOUND:', htmlStartIdx, htmlEndIdx);
}

// 2. Thay thế Dialog/Drawer Modals
const modalStart = '<!-- Contingency Plan Create/Edit Dialog -->';
let modalStartIdx = content.indexOf(modalStart);
let templateEndIdx = content.lastIndexOf('</template>');

if (modalStartIdx !== -1 && templateEndIdx !== -1) {
    content = content.substring(0, modalStartIdx) + modalsContent + '\n' + content.substring(templateEndIdx);
    console.log('Replaced Modals.');
} else if (modalStartIdx === -1 && templateEndIdx !== -1) {
    content = content.substring(0, templateEndIdx) + '\n' + modalsContent + '\n' + content.substring(templateEndIdx);
    console.log('Appended Modals.');
}

fs.writeFileSync(file, content, 'utf8');
