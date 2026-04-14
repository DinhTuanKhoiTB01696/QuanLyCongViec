const fs = require('fs');
const path = require('path');

const srcPath = path.join('d:', 'A', 'QuanLyCongViec', 'Frontend', 'src');
const spaceFile = path.join(srcPath, 'views', 'SpaceSummary.vue');

let content = fs.readFileSync(spaceFile, 'utf8');

// 1. EXTRACT MODAL
const startStr = '<transition name=\"fade\">\r\n        <div class=\"task-modal-overlay\"';
let startIndex = content.indexOf(startStr);
if (startIndex === -1) {
    startIndex = content.indexOf('<transition name=\"fade\">\n        <div class=\"task-modal-overlay\"');
}

const endStr = '</transition>';
const afterStr = '      <!-- Nexus Layout handles Topbar and Sidebar -->';
const afterIdx = content.indexOf(afterStr);
let endIndex = content.lastIndexOf(endStr, afterIdx) + endStr.length;

if (startIndex === -1 || endIndex === -1) {
    console.log('Cannot find modal bounds');
    process.exit(1);
}

const modalTemplate = content.substring(startIndex, endIndex);

let n = '\n';
let modalVue = '<template>' + n + modalTemplate.trim() + n + '</template>' + n + '<script setup>'+ n +'</script>';
fs.writeFileSync(path.join(srcPath, 'components', 'TaskDetailModal.vue'), modalVue);

let newSummary = content.substring(0, startIndex) +
    '      <!-- Task Detail Modal Component -->\n' +
    '      <TaskDetailModal \n' +
    '        v-if=\"showTaskModal\" \n' +
    '        :selectedTask=\"selectedTask\" \n' +
    '        :projectId=\"projectId\"\n' +
    '        @close=\"showTaskModal = false\"\n' +
    '        @updateTask=\"updateTaskField\"\n' +
    '      />\n' + 
    content.substring(endIndex);

// 2. EXTRACT KANBAN BOARD
const kbStartStr = '<div class=\"kanban-board\">';
let kbStartIndex = newSummary.indexOf(kbStartStr);
const kbEndStr = '<!-- ADD STATUS COLUMN BUTTON -->';
let kbEndIndex = newSummary.indexOf(kbEndStr, kbStartIndex);
if (kbStartIndex !== -1 && kbEndIndex !== -1) {
    // We want to delete until the </div> of the kanban-board, but it's tricky.
    // The kanban-board wraps the columns and ADD STATUS.
    // We can just replace the whole thing.
    let beforeKb = newSummary.substring(0, kbStartIndex);
    // Find the closing div of <div class="kanban-board">
    // We'll roughly replace till '<!-- TAB CONTENT: BACKLOG (List) -->'
    const nextTabStr = '<!-- TAB CONTENT: BACKLOG (List) -->';
    const nextTabIdx = newSummary.indexOf(nextTabStr, kbStartIndex);
    const afterKb = newSummary.substring(nextTabIdx);
    
    newSummary = beforeKb + 
      '              <KanbanBoard \n' +
      '                :tasks=\"filteredTasks\"\n' +
      '                :statuses=\"projectStatuses\"\n' +
      '                :projectId=\"projectId\"\n' +
      '                :loading=\"isFetchingTasks\"\n' +
      '                @task-click=\"openTaskDetail\"\n' +
      '                @status-changed=\"handleDraggableChange\"\n' +
      '                @task-reordered=\"handleTaskReordered\"\n' +
      '                @refresh=\"fetchTasks\"\n' +
      '              />\n\n            </div>\n          </div>\n\n          ' + afterKb;
}

// Ensure KanbanBoard is imported
if (newSummary.indexOf('import KanbanBoard') === -1) {
    newSummary = newSummary.replace(
        'import NexusLayout from \'@/components/layout/NexusLayout.vue\'',
        'import NexusLayout from \'@/components/layout/NexusLayout.vue\'\nimport KanbanBoard from \'@/components/KanbanBoard.vue\'\nimport TaskDetailModal from \'@/components/TaskDetailModal.vue\''
    );
}

fs.writeFileSync(spaceFile, newSummary);
console.log('SpaceSummary.vue updated!');
