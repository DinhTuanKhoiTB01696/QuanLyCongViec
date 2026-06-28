const fs = require('fs');

// --- 1. GoalsDashboard.vue ---
let goalsPath = 'Frontend/src/views/HomeSite/Goals/GoalsDashboard.vue';
let goalsContent = fs.readFileSync(goalsPath, 'utf8');

// The translate function in GoalsDashboard is `translateStatus`
// Update statusOptions to translate the statuses
goalsContent = goalsContent.replace(/const statusOptions = computed\(\(\) => uniqueValues\(g => g\.status\)\)/, `const statusOptions = computed(() => {
  const statuses = uniqueValues(g => g.status);
  // Map raw statuses to translated statuses and remove duplicates again
  return Array.from(new Set(statuses.map(translateStatus)));
})`);

// Update filteredGoals to check against translated status
goalsContent = goalsContent.replace(/list\.filter\(g => g\.status === filters\.value\.status\)/, `list.filter(g => translateStatus(g.status) === filters.value.status)`);

fs.writeFileSync(goalsPath, goalsContent);
console.log('Fixed GoalsDashboard');

// --- 2. ProjectList.vue ---
let projPath = 'Frontend/src/views/HomeSite/Projects/ProjectList.vue';
let projContent = fs.readFileSync(projPath, 'utf8');

// Update DropdownFilter labels and boolean options
projContent = projContent.replace(/<DropdownFilter label="Đang theo dõi"/g, '<DropdownFilter label="Theo dõi"');
projContent = projContent.replace(/<DropdownFilter label="Có gắn sao"/g, '<DropdownFilter label="Yêu thích"');

projContent = projContent.replace(/const booleanOptions = \['true', 'false'\]/, `const booleanOptions = [
  { label: 'Có', value: 'true' },
  { label: 'Không', value: 'false' }
]`);

// Update statusOptions to use translation
let translateProjectStatus = `const translateStatus = (status) => {
  if (!status) return 'Đang chờ xử lý'
  if (status === true || status === 'true') return 'Đang chờ xử lý'
  if (status === false || status === 'false') return 'Đã lưu trữ'
  const map = {
    'on track': 'Đúng tiến độ',
    'at risk': 'Có rủi ro',
    'off track': 'Trễ tiến độ',
    'pending': 'Đang chờ xử lý',
    'completed': 'Đã hoàn tất',
    'archived': 'Đã lưu trữ'
  }
  return map[status.toString().toLowerCase()] || status
}
`;

if (!projContent.includes('const translateStatus')) {
  projContent = projContent.replace(/const filters = ref/, translateProjectStatus + '\nconst filters = ref');
}

projContent = projContent.replace(/const statusOptions = computed\(\(\) => \{[\s\S]*?\}\)/, `const statusOptions = computed(() => {
  const statuses = projectStore.projects?.map(p => p.status) || []
  const validStatuses = [...new Set(statuses)].filter(s => s !== null && s !== undefined)
  return Array.from(new Set(validStatuses.map(translateStatus)))
})`);

// Update filteredProjects status check
projContent = projContent.replace(/list\.filter\(p => p\.status === filters\.value\.status\)/, `list.filter(p => translateStatus(p.status) === filters.value.status)`);

fs.writeFileSync(projPath, projContent);
console.log('Fixed ProjectList');

// --- 3. TeamsDashboard.vue ---
let teamsDashPath = 'Frontend/src/views/HomeSite/Teams/TeamsDashboard.vue';
let teamsContent = fs.readFileSync(teamsDashPath, 'utf8');

// The toggle buttons are currently <button class="toggle-btn" ...
// We need to replace them and their container with EXACTLY the structure from TeamList.vue
// TeamList.vue has:
// <div class="view-toggle">
//   <button class="toggle-btn" :class="{ active: viewMode === 'grid' }" @click="viewMode = 'grid'" title="Chế độ lưới">
//     <i class="fa-solid fa-table-cells-large"></i>
//   </button>
//   <button class="toggle-btn" :class="{ active: viewMode === 'table' }" @click="viewMode = 'table'" title="Chế độ danh sách">
//     <i class="fa-solid fa-list"></i>
//   </button>
// </div>

let newToggleHtml = `<div class="view-toggle">
                <button class="toggle-btn" :class="{ active: viewMode === 'grid' }" @click="viewMode = 'grid'" title="Chế độ lưới">
                  <i class="fa-solid fa-table-cells-large"></i>
                </button>
                <button class="toggle-btn" :class="{ active: viewMode === 'table' }" @click="viewMode = 'table'" title="Chế độ danh sách">
                  <i class="fa-solid fa-list"></i>
                </button>
              </div>`;

teamsContent = teamsContent.replace(/<div class="view-toggle">[\s\S]*?<\/div>\s*<\/div>\s*<\/div>/, newToggleHtml + '\n            </div>\n          </div>');

// Replace the injected CSS with EXACTLY TeamList CSS
teamsContent = teamsContent.replace(/\/\* View Toggle \*\/[\s\S]*?\.toggle-btn\.active \{[\s\S]*?\}/, `/* View Toggle */
.view-toggle {
  display: flex;
  border: 1px solid #DFE1E6;
  border-radius: 3px;
  overflow: hidden;
}

.toggle-btn {
  background: #FAFBFC;
  border: none;
  padding: 8px 12px;
  color: #5E6C84;
  cursor: pointer;
  font-size: 14px;
  transition: background-color 0.2s, color 0.2s;
}

.toggle-btn:hover {
  background: #EBECF0;
}

.toggle-btn.active {
  background-color: #DEEBFF;
  color: #0052CC;
}`);

fs.writeFileSync(teamsDashPath, teamsContent);
console.log('Fixed TeamsDashboard');
