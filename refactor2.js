const fs = require('fs');
const path = require('path');

const srcPath = path.join('d:', 'A', 'QuanLyCongViec', 'Frontend', 'src');
const spaceFile = path.join(srcPath, 'views', 'SpaceSummary.vue');
const modalFile = path.join(srcPath, 'components', 'TaskDetailModal.vue');

let content = fs.readFileSync(spaceFile, 'utf8');

const startStr = '<transition name=\"fade\">\r\n        <div class=\"task-modal-overlay\"';
let startIndex = content.indexOf(startStr);
if (startIndex === -1) {
    startIndex = content.indexOf('<transition name=\"fade\">\n        <div class=\"task-modal-overlay\"');
}

const endStr = '</transition>';
const afterStr = '<!-- Nexus Layout handles Topbar and Sidebar -->';
const afterIdx = content.indexOf(afterStr);
let endIndex = content.lastIndexOf(endStr, afterIdx) + endStr.length;

if (startIndex === -1 || nextSection === -1) {
    console.log('Cannot find modal bounds');
    process.exit(1);
}

const modalTemplate = content.substring(startIndex, endIndex);

// Build new string
let n = '\n';
let modalVue = '<template>' + n + modalTemplate.trim() + n + '</template>' + n;
fs.writeFileSync('modalTemplate.html', modalVue);
console.log('Saved modalTemplate.html');

let newSummary = content.substring(0, startIndex) +
    '      <!-- Task Detail Modal Component -->\n' +
    '      <TaskDetailModal \n' +
    '        v-if=\"showTaskModal\" \n' +
    '        :selectedTask=\"selectedTask\" \n' +
    '        :projectId=\"projectId\"\n' +
    '        :projectMembers=\"projectMembers\"\n' +
    '        :currentUser=\"currentUser\"\n' +
    '        :mentionCandidates=\"mentionCandidates\"\n' +
    '        :pendingAttachments=\"pendingAttachments\"\n' +
    '        :comments=\"comments\"\n' +
    '        @close=\"showTaskModal = false\"\n' +
    '        @updateTask=\"updateTaskField\"\n' +
    '        @open-task=\"openTaskDetail\"\n' +
    '        @create-subtask=\"createSubtask\"\n' +
    '        @fetch-comments=\"fetchComments\"\n' +
    '      />\n' + 
    content.substring(endIndex);

fs.writeFileSync(spaceFile, newSummary);
console.log('SpaceSummary.vue updated!');
