import re
path = r"c:\Users\tua46\OneDrive\Máy tính\DATN\QuanLyCongViec\Frontend\src\views\HomeSite\Teams\TeamDetail.vue"
with open(path, "r", encoding="utf-8") as f:
    content = f.read()

# 1. GOALS dropdown
goals_pattern = re.compile(
    r'(<button class="secondary-btn" @click\.stop="isGoalDropdownOpen = !isGoalDropdownOpen">Thêm mục tiêu</button>\s*</div>\s*<!-- Goal Dropdown Menu -->\s*<div class="dropdown-menu search-dropdown" v-if="isGoalDropdownOpen" @click\.stop style="position: absolute; )top: 120px; left: 24px;',
    re.DOTALL
)

def goals_repl(m):
    return m.group(1) + 'top: calc(100% + 4px); left: 0;'

content = goals_pattern.sub(goals_repl, content)

content = content.replace(
    '<button class="secondary-btn" @click.stop="isGoalDropdownOpen = !isGoalDropdownOpen">Thêm mục tiêu</button>',
    '<div style="position: relative; display: inline-block;">\n                 <button class="secondary-btn" @click.stop="isGoalDropdownOpen = !isGoalDropdownOpen">Thêm mục tiêu</button>'
)

# Find the end of Goal Dropdown Menu and add </div>
goal_menu_end_idx = content.find('<!-- Goal Dropdown Menu -->')
if goal_menu_end_idx != -1:
    goal_plus_idx = content.find('Tạo mục tiêu</span>', goal_menu_end_idx)
    if goal_plus_idx != -1:
        end_divs_idx = content.find('</div>', goal_plus_idx)
        end_divs_idx2 = content.find('</div>', end_divs_idx + 6)
        end_divs_idx3 = content.find('</div>', end_divs_idx2 + 6)
        content = content[:end_divs_idx3 + 6] + '\n                 </div>' + content[end_divs_idx3 + 6:]


# 2. PROJECTS dropdown
projects_pattern = re.compile(
    r'<button class="secondary-btn" @click="isCreateProjectOpen = true">Tạo dự án</button>',
    re.DOTALL
)

proj_replacement = '''<div style="position: relative; display: inline-block;">
                    <button class="secondary-btn" @click.stop="isProjectDropdownOpen = !isProjectDropdownOpen">Thêm dự án</button>
                    <!-- Project Dropdown Menu -->
                    <div class="dropdown-menu search-dropdown" v-if="isProjectDropdownOpen" @click.stop style="position: absolute; top: calc(100% + 4px); left: 0; z-index: 100; width: 300px; padding: 8px; box-shadow: 0 4px 12px rgba(0,0,0,0.15); border-radius: 3px; border: 1px solid #DFE1E6; background: white;">
                       <input type="text" v-model="projectSearch" placeholder="Tìm kiếm dự án hoặc dán liên kết" class="search-input" style="width: 100%; margin-bottom: 12px; padding-left: 12px !important;" />
                       <h5 style="font-size: 11px; color: #6B778C; text-transform: uppercase; padding: 0 8px 8px;">Dự án gần đây</h5>
                       <div class="goal-list-options" style="max-height: 200px; overflow-y: auto;">
                         <div class="team-option" v-for="p in siteProjects" :key="p.id" @click="linkProject(p)" style="display: flex; align-items: center; gap: 8px; padding: 8px; cursor: pointer; border-radius: 3px;">
                           <div style="width: 16px; height: 16px; background-color: #0052cc; border-radius: 2px; flex-shrink: 0;"></div>
                           <div style="display: flex; flex-direction: column;">
                             <span style="font-size: 13px; color: #172B4D;">{{ p.name || p.title }}</span>
                           </div>
                         </div>
                         <div v-if="siteProjects.length === 0" style="padding: 8px; font-size: 12px; color: #6B778C;">Không tìm thấy dự án nào.</div>
                       </div>
                       <div style="border-top: 1px solid #DFE1E6; margin-top: 8px; padding-top: 8px;">
                          <div class="team-option" style="display: flex; align-items: center; gap: 8px; padding: 8px; cursor: pointer; color: #172B4D;" @click="isCreateProjectOpen = true">
                             <i class="fa-solid fa-plus"></i> <span style="font-size: 13px;">Tạo dự án mới</span>
                          </div>
                       </div>
                    </div>
                 </div>'''

content = projects_pattern.sub(proj_replacement, content)


with open(path, "w", encoding="utf-8") as f:
    f.write(content)
print("Done!")
