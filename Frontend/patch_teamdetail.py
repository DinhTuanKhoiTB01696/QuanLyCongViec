import re

path = r"c:\Users\tua46\OneDrive\Máy tính\DATN\QuanLyCongViec\Frontend\src\views\HomeSite\Teams\TeamDetail.vue"

with open(path, "r", encoding="utf-8") as f:
    content = f.read()

# 1. Avatar in Member List
old_member_list = '''          <div class="member-list">
            <div class="member-item" v-for="member in members" :key="member.id">
              <div class="member-avatar-small">{{ member.avatar }}</div>
              <div class="member-info">
                <span class="member-name">{{ member.name }}</span>
                <span class="member-role">{{ member.role }}</span>
              </div>
            </div>
          </div>'''

new_member_list = '''          <div class="member-list">
            <div class="member-item" v-for="member in members" :key="member.id">
              <UserAvatar :user="member" :size="32" :fontSize="14" />
              <div class="member-info">
                <span class="member-name">{{ member.fullName || member.name || member.email }}</span>
                <span class="member-role">{{ member.role || 'Member' }}</span>
              </div>
            </div>
          </div>'''

content = content.replace(old_member_list, new_member_list)

# 2. Hierarchy tab - 2 columns for sub-teams
# Target: <div class="child-nodes-wrapper" style="display: flex; justify-content: center;">
old_hierarchy = '''            <div class="child-nodes-wrapper" style="display: flex; justify-content: center;">
              
              <div class="tree-node child-node" v-for="(child, index) in hierarchy.children" :key="child.id" style="position: relative; padding: 0 12px; display: flex; flex-direction: column; align-items: center;">'''

new_hierarchy = '''            <div class="child-nodes-wrapper" style="display: grid; grid-template-columns: repeat(2, 1fr); gap: 24px; justify-content: center; width: auto;">
              
              <div class="tree-node child-node" v-for="(child, index) in hierarchy.children" :key="child.id" style="position: relative; padding: 0; display: flex; flex-direction: column; align-items: center;">'''

content = content.replace(old_hierarchy, new_hierarchy)

# Remove the horizontal line that spans across children, because grid makes it complicated, or keep it inside.
old_child_lines = '''                <div style="position: absolute; top: 0; height: 1px; background-color: #DFE1E6;"
                     :style="{
                        left: index === 0 ? '50%' : '0',
                        right: '0'
                     }"></div>
                <div class="tree-line-vertical-up" style="width: 1px; height: 24px; background-color: #DFE1E6;"></div>'''

new_child_lines = '''                <div class="tree-line-vertical-up" style="width: 1px; height: 24px; background-color: #DFE1E6; position: absolute; top: -24px;"></div>'''
content = content.replace(old_child_lines, new_child_lines)

# Also fix the add-node lines
old_add_lines = '''                <div style="position: absolute; top: 0; height: 1px; background-color: #DFE1E6;"
                     :style="{
                        left: '0',
                        right: '50%',
                        display: hierarchy.children && hierarchy.children.length > 0 ? 'block' : 'none'
                     }"></div>
                <div class="tree-line-vertical-up" style="width: 1px; height: 24px; background-color: #DFE1E6;"></div>'''
new_add_lines = '''                <div class="tree-line-vertical-up" style="width: 1px; height: 24px; background-color: #DFE1E6; position: absolute; top: -24px;"></div>'''
content = content.replace(old_add_lines, new_add_lines)


# 3. Filter parent/child dropdowns to exclude already selected ones.
# Find `const filteredTeams`
old_filtered_teams = '''const filteredTeams = computed(() => {
  return siteTeams.value.filter(t => t.name.toLowerCase().includes(teamSearch.value.toLowerCase()))
})'''

new_filtered_teams = '''const filteredTeams = computed(() => {
  return siteTeams.value.filter(t => {
    // Exclude current team
    if (t.id === team.value?.id) return false;
    // Exclude parent team if already selected
    if (hierarchy.value?.parent?.id === t.id) return false;
    // Exclude any child team already selected
    if (hierarchy.value?.children?.some(c => c.id === t.id)) return false;
    // Name search
    return t.name.toLowerCase().includes(teamSearch.value.toLowerCase());
  })
})'''

content = content.replace(old_filtered_teams, new_filtered_teams)


# Write back
with open(path, "w", encoding="utf-8") as f:
    f.write(content)

print("Done")
