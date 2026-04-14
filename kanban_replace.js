const fs = require('fs');
const path = require('path');

const spaceFile = path.join('d:', 'A', 'QuanLyCongViec', 'Frontend', 'src', 'views', 'SpaceSummary.vue');
let content = fs.readFileSync(spaceFile, 'utf8');

const sIdx = content.indexOf('<div class="kanban-board">');
if (sIdx > -1) {
    const eIdx = content.indexOf('</div>\r\n          </div>', sIdx);
    if (eIdx > -1) {
        const pre = content.substring(0, sIdx);
        const post = content.substring(eIdx); // keep </div>\n          </div> which belong to board-content
        
        const newHtml = <!-- S? d?ng KanbanBoard d?nh nghia chung -->\n            <KanbanBoard \n              :tasks="tasks" \n              :statuses="tasks.map(t => t.statusName || 'TO DO')" \n              :projectId="projectId" \n              @task-click="openTaskDetail" \n            />;
        
        const newContent = pre + newHtml + post;
        fs.writeFileSync(spaceFile, newContent);
        console.log('KanbanBoard replaced!');
    } else {
        console.log('End bound not found');
    }
} else {
    console.log('Start bound not found');
}
