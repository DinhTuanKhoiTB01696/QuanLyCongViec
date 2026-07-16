const fs = require('fs');
const file = 'src/views/SpaceSummary.vue';
let content = fs.readFileSync(file, 'utf8');

const oldCode = `<p class="issue-title" :style="element.statusName === 'DONE' ? { textDecoration: 'line-through', color: 'var(--color-text-muted)' } : {}">{{ element.title }}</p>`;
const newCode = `<p class="issue-title" :style="element.statusName === 'DONE' ? { textDecoration: 'line-through', color: 'var(--color-text-muted)' } : {}">
                        <span v-if="element.title && element.title.startsWith('[DỰ PHÒNG]')" class="inline-flex items-center px-1.5 py-0.5 rounded-full bg-blue-100 text-blue-700 text-[10px] font-bold mr-1 border border-blue-200 uppercase tracking-wider relative top-[-1px]">Dự phòng</span>
                        {{ element.title && element.title.startsWith('[DỰ PHÒNG]') ? element.title.substring(11).trim() : element.title }}
                      </p>`;

content = content.replace(oldCode, newCode);
fs.writeFileSync(file, content, 'utf8');
