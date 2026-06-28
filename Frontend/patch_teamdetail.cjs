const fs = require('fs');

const path = 'c:\\Users\\tua46\\OneDrive\\Máy tính\\DATN\\QuanLyCongViec\\Frontend\\src\\views\\HomeSite\\Teams\\TeamDetail.vue';
let content = fs.readFileSync(path, 'utf8');

const old_member_list = `          <div class="member-list">
            <div class="member-item" v-for="member in members" :key="member.id">
              <div class="member-avatar-small">{{ member.avatar }}</div>
              <div class="member-info">
                <span class="member-name">{{ member.name }}</span>
                <span class="member-role">{{ member.role }}</span>
              </div>
            </div>
          </div>`;

const new_member_list = `          <div class="member-list">
            <div class="member-item" v-for="member in members" :key="member.id">
              <UserAvatar :user="member" :size="32" :fontSize="14" />
              <div class="member-info">
                <span class="member-name">{{ member.fullName || member.name || member.email }}</span>
                <span class="member-role">{{ member.role || 'Member' }}</span>
              </div>
            </div>
          </div>`;

content = content.replace(old_member_list, new_member_list);

const old_hierarchy = `            <div class="child-nodes-wrapper" style="display: flex; justify-content: center;">
              
              <div class="tree-node child-node" v-for="(child, index) in hierarchy.children" :key="child.id" style="position: relative; padding: 0 12px; display: flex; flex-direction: column; align-items: center;">`;

const new_hierarchy = `            <div class="child-nodes-wrapper" style="display: grid; grid-template-columns: repeat(2, 1fr); gap: 24px; justify-content: center; width: auto; max-width: 600px; margin: 0 auto; position: relative;">
              <!-- Main horizontal branch line connecting children to center -->
              <div style="position: absolute; top: -24px; left: 25%; right: 25%; height: 1px; background-color: #DFE1E6; z-index: 0;" v-if="hierarchy.children && hierarchy.children.length > 1"></div>
              
              <div class="tree-node child-node" v-for="(child, index) in hierarchy.children" :key="child.id" style="position: relative; padding: 0; display: flex; flex-direction: column; align-items: center; z-index: 1;">`;

content = content.replace(old_hierarchy, new_hierarchy);

const old_child_lines = `                <div style="position: absolute; top: 0; height: 1px; background-color: #DFE1E6;"
                     :style="{
                        left: index === 0 ? '50%' : '0',
                        right: '0'
                     }"></div>
                <div class="tree-line-vertical-up" style="width: 1px; height: 24px; background-color: #DFE1E6;"></div>`;

const new_child_lines = `                <div class="tree-line-vertical-up" style="width: 1px; height: 24px; background-color: #DFE1E6; position: absolute; top: -24px;"></div>`;
content = content.replace(old_child_lines, new_child_lines);

const old_add_lines = `                <div style="position: absolute; top: 0; height: 1px; background-color: #DFE1E6;"
                     :style="{
                        left: '0',
                        right: '50%',
                        display: hierarchy.children && hierarchy.children.length > 0 ? 'block' : 'none'
                     }"></div>
                <div class="tree-line-vertical-up" style="width: 1px; height: 24px; background-color: #DFE1E6;"></div>`;
const new_add_lines = `                <div class="tree-line-vertical-up" style="width: 1px; height: 24px; background-color: #DFE1E6; position: absolute; top: -24px;" v-if="hierarchy.children?.length === 0"></div>`;
content = content.replace(old_add_lines, new_add_lines);

const old_filtered_teams = `const filteredTeams = computed(() => {
  return siteTeams.value.filter(t => t.name.toLowerCase().includes(teamSearch.value.toLowerCase()))
})`;

const new_filtered_teams = `const filteredTeams = computed(() => {
  return siteTeams.value.filter(t => {
    if (t.id === team.value?.id) return false;
    if (hierarchy.value?.parent?.id === t.id) return false;
    if (hierarchy.value?.children?.some(c => c.id === t.id)) return false;
    return (t.name || '').toLowerCase().includes(teamSearch.value.toLowerCase());
  })
})`;

content = content.replace(old_filtered_teams, new_filtered_teams);

fs.writeFileSync(path, content, 'utf8');
console.log("Done");
