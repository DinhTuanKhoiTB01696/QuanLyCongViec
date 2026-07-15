const fs = require('fs');
const file = 'src/views/SpaceSummary.vue';
let content = fs.readFileSync(file, 'utf8');

const oldCode = `                   <span class="task-title" :style="group.statusName === 'DONE' ? { textDecoration: 'line-through', color: 'var(--color-text-muted)' } : {}">
                     {{ task.title }}
                   </span>`;
                   
const newCode = `                   <span class="task-title" :style="group.statusName === 'DONE' ? { textDecoration: 'line-through', color: 'var(--color-text-muted)' } : {}">
                     <span v-if="task.title && task.title.startsWith('[DỰ PHÒNG]')" class="inline-flex items-center px-1.5 py-0.5 rounded-full bg-blue-100 text-blue-700 text-[10px] font-bold mr-1 border border-blue-200 uppercase tracking-wider relative top-[-1px]">Dự phòng</span>
                     {{ task.title && task.title.startsWith('[DỰ PHÒNG]') ? task.title.substring(11).trim() : task.title }}
                   </span>`;

content = content.replace(oldCode, newCode);
if (!content.includes(newCode)) {
    const oldCode2 = `                   <span class="task-title" :style="group.statusName === 'DONE' ? { textDecoration: 'line-through', color: 'var(--color-text-muted)' } : {}">\r\n                     {{ task.title }}\r\n                   </span>`;
    const newCode2 = `                   <span class="task-title" :style="group.statusName === 'DONE' ? { textDecoration: 'line-through', color: 'var(--color-text-muted)' } : {}">\r\n                     <span v-if="task.title && task.title.startsWith('[DỰ PHÒNG]')" class="inline-flex items-center px-1.5 py-0.5 rounded-full bg-blue-100 text-blue-700 text-[10px] font-bold mr-1 border border-blue-200 uppercase tracking-wider relative top-[-1px]">Dự phòng</span>\r\n                     {{ task.title && task.title.startsWith('[DỰ PHÒNG]') ? task.title.substring(11).trim() : task.title }}\r\n                   </span>`;
    content = content.replace(oldCode2, newCode2);
}

fs.writeFileSync(file, content, 'utf8');
