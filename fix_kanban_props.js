const fs = require('fs');
const path = require('path');

const spaceFile = path.join('d:', 'A', 'QuanLyCongViec', 'Frontend', 'src', 'views', 'SpaceSummary.vue');
let content = fs.readFileSync(spaceFile, 'utf8');

const sIdx = content.indexOf('<KanbanBoard \r\n                :tasks="filteredTasks"');
if (sIdx > -1) {
    const eIdx = content.indexOf('/>', sIdx) + 2;
    
    // In original code, the array of tasks might be \`tasks\` and statuses might be a computed or mapped list
    const newHtml = `<KanbanBoard 
              :tasks="tasks"
              :statuses="['TO DO', 'IN PROGRESS', 'IN REVIEW', 'DONE']"
              :projectId="projectId"
              :loading="isFetchingTasks"
              @task-click="openTaskDetail"
              @status-changed="(payload) => handleDraggableChange(null, payload)"
              @task-reordered="handleTaskReordered"
              @refresh="fetchTasks"
            />`;

    const newContent = content.substring(0, sIdx) + newHtml + content.substring(eIdx);
    fs.writeFileSync(spaceFile, newContent);
    console.log('KanbanBoard attributes fixed!');
} else {
    // try different search
    const sIdx2 = content.indexOf('<KanbanBoard');
    if (sIdx2 > -1) {
        console.log('Found KanbanBoard, but exact text differs. Replacing from generic tag found...');
        let substr = content.substring(sIdx2, sIdx2+200);
        console.log(substr);
        
        const eIdx2 = content.indexOf('/>', sIdx2) + 2;
        const newHtml = `<KanbanBoard 
              :tasks="tasks"
              :statuses="['TO DO', 'IN PROGRESS', 'IN REVIEW', 'DONE']"
              :projectId="projectId"
              :loading="isFetchingTasks"
              @task-click="openTaskDetail"
              @status-changed="(payload) => handleDraggableChange(null, payload)"
              @task-reordered="handleTaskReordered"
              @refresh="fetchTasks"
            />`;
         const newContent = content.substring(0, sIdx2) + newHtml + content.substring(eIdx2);
         fs.writeFileSync(spaceFile, newContent);
         console.log('KanbanBoard attributes fixed (fallback)!');
    } else {
        console.log('Cannot find KanbanBoard tags.');
    }
}
