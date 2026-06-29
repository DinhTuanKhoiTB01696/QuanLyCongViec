import re

path = r"c:\Users\tua46\OneDrive\Máy tính\DATN\QuanLyCongViec\Frontend\src\views\HomeSite\Teams\TeamDetail.vue"
with open(path, 'r', encoding='utf-8') as f:
    content = f.read()

# 1. Fix Hierarchy layout (Grid & 'x' button)
old_children = '''          <div class="tree-level children-level" style="display: flex; flex-direction: column; align-items: center; position: relative; width: 100%;">
            <div class="child-nodes-wrapper" style="display: flex; justify-content: center;">
              
              <div class="tree-node child-node" v-for="(child, index) in hierarchy.children" :key="child.id" style="position: relative; padding: 0 12px; display: flex; flex-direction: column; align-items: center;">
                <div style="position: absolute; top: 0; height: 1px; background-color: #DFE1E6;"
                     :style="{
                        left: index === 0 ? '50%' : '0',
                        right: '0'
                     }"></div>
                <div class="tree-line-vertical-up" style="width: 1px; height: 24px; background-color: #DFE1E6;"></div>
                
                <div class="hierarchy-card-box" style="border: 1px solid #DFE1E6; border-radius: 3px; padding: 8px 16px; display: flex; align-items: center; gap: 8px; background: white; min-width: 200px; box-shadow: 0 1px 2px rgba(0,0,0,0.05);">
                  <div class="member-avatar-micro" style="background-color: #36B37E; color: white;">{{ child.name.substring(0,2).toUpperCase() }}</div>
                  <div style="display: flex; flex-direction: column;">
                    <span style="font-size: 13px; font-weight: 500; color: #172B4D;">{{ child.name }}</span>
                    <span style="font-size: 11px; color: #6B778C;">Đội ngũ chính thức <i class="fa-solid fa-circle-check text-primary"></i> • 1 members</span>
                  </div>
                </div>
              </div>

              <div class="tree-node add-node" style="position: relative; padding: 0 12px; display: flex; flex-direction: column; align-items: center;">
                <div style="position: absolute; top: 0; height: 1px; background-color: #DFE1E6;"
                     :style="{
                        left: '0',
                        right: '50%',
                        display: hierarchy.children && hierarchy.children.length > 0 ? 'block' : 'none'
                     }"></div>
                <div class="tree-line-vertical-up" style="width: 1px; height: 24px; background-color: #DFE1E6;"></div>'''

new_children = '''          <div class="tree-level children-level" style="display: flex; flex-direction: column; align-items: center; position: relative; width: 100%;">
            <div class="child-nodes-wrapper" style="display: grid; grid-template-columns: repeat(2, 1fr); gap: 24px; justify-content: center; width: auto; max-width: 600px; margin: 0 auto; position: relative;">
              <div style="position: absolute; top: -24px; left: 25%; right: 25%; height: 1px; background-color: #DFE1E6; z-index: 0;" v-if="hierarchy.children && hierarchy.children.length > 1"></div>
              
              <div class="tree-node child-node" v-for="(child, index) in hierarchy.children" :key="child.id" style="position: relative; padding: 0; display: flex; flex-direction: column; align-items: center; z-index: 1;">
                <div class="tree-line-vertical-up" style="width: 1px; height: 24px; background-color: #DFE1E6; position: absolute; top: -24px;"></div>
                
                <div class="hierarchy-card-box" style="border: 1px solid #DFE1E6; border-radius: 3px; padding: 8px 16px; display: flex; align-items: center; gap: 8px; background: white; min-width: 220px; box-shadow: 0 1px 2px rgba(0,0,0,0.05); position: relative;">
                  <div class="member-avatar-micro" style="background-color: #36B37E; color: white;">{{ child.name.substring(0,2).toUpperCase() }}</div>
                  <div style="display: flex; flex-direction: column; flex: 1;">
                    <span style="font-size: 13px; font-weight: 500; color: #172B4D;">{{ child.name }}</span>
                    <span style="font-size: 11px; color: #6B778C;">Đội ngũ chính thức <i class="fa-solid fa-circle-check text-primary"></i> • 1 members</span>
                  </div>
                  <button class="icon-btn micro" style="position: absolute; right: 8px; color: #6B778C;" @click.stop="removeChildTeam(child.id)"><i class="fa-solid fa-xmark"></i></button>
                </div>
              </div>

              <div class="tree-node add-node" style="position: relative; padding: 0; display: flex; flex-direction: column; align-items: center; z-index: 1;">
                <div class="tree-line-vertical-up" style="width: 1px; height: 24px; background-color: #DFE1E6; position: absolute; top: -24px;" v-if="hierarchy.children?.length === 0"></div>'''
content = content.replace(old_children, new_children)

# Add removeChildTeam script logic
old_addchild = '''const addChildTeam = async (t) => {
  if (!teamStore.hierarchy) teamStore.hierarchy = { parent: null, children: [] }
  if (!teamStore.hierarchy.children) teamStore.hierarchy.children = []
  if (!teamStore.hierarchy.children.find(c => c.id === t.id)) {
    teamStore.hierarchy.children.push(t)
  }
  isChildDropdownOpen.value = false
  teamSearch.value = ''
}'''

new_addchild = '''const addChildTeam = async (t) => {
  if (!teamStore.hierarchy) teamStore.hierarchy = { parent: null, children: [] }
  if (!teamStore.hierarchy.children) teamStore.hierarchy.children = []
  if (!teamStore.hierarchy.children.find(c => c.id === t.id)) {
    teamStore.hierarchy.children.push(t)
  }
  isChildDropdownOpen.value = false
  teamSearch.value = ''
}

const removeChildTeam = async (id) => {
  if (teamStore.hierarchy && teamStore.hierarchy.children) {
    teamStore.hierarchy.children = teamStore.hierarchy.children.filter(c => c.id !== id)
  }
}'''
content = content.replace(old_addchild, new_addchild)


# 2. Fix Goal dropdown position
# It is currently wrapped in:
# <div style="flex: 1;"> ... <button>
# I will change the flex: 1 div to position: relative.
old_goal_text = '''              <div style="flex: 1;">
                 <h4 style="font-size: 14px; color: #172B4D; margin-bottom: 8px;">Xem các mục tiêu mà đội ngũ của bạn đang hướng tới</h4>
                 <p style="font-size: 13px; color: #6B778C; margin-bottom: 16px; line-height: 1.5;">Mục tiêu giúp đội ngũ của bạn kết nối công việc với những kết quả mà họ đóng góp, đồng thời cung cấp một nơi duy nhất để chia sẻ tiến độ thực hiện mục tiêu. Tạo mục tiêu để mọi người hiểu rõ định hướng và ưu tiên của nhóm.</p>
                 <button class="secondary-btn" @click.stop="isGoalDropdownOpen = !isGoalDropdownOpen">Thêm mục tiêu</button>
              </div>

              <!-- Goal Dropdown Menu -->
              <div class="dropdown-menu search-dropdown" v-if="isGoalDropdownOpen" @click.stop style="position: absolute; top: 120px; left: 24px; z-index: 100; width: 300px; padding: 8px; box-shadow: 0 4px 12px rgba(0,0,0,0.15); border-radius: 3px; border: 1px solid #DFE1E6; background: white;">'''

new_goal_text = '''              <div style="flex: 1; position: relative;">
                 <h4 style="font-size: 14px; color: #172B4D; margin-bottom: 8px;">Xem các mục tiêu mà đội ngũ của bạn đang hướng tới</h4>
                 <p style="font-size: 13px; color: #6B778C; margin-bottom: 16px; line-height: 1.5;">Mục tiêu giúp đội ngũ của bạn kết nối công việc với những kết quả mà họ đóng góp, đồng thời cung cấp một nơi duy nhất để chia sẻ tiến độ thực hiện mục tiêu. Tạo mục tiêu để mọi người hiểu rõ định hướng và ưu tiên của nhóm.</p>
                 <button class="secondary-btn" @click.stop="isGoalDropdownOpen = !isGoalDropdownOpen">Thêm mục tiêu</button>

                 <!-- Goal Dropdown Menu -->
                 <div class="dropdown-menu search-dropdown" v-if="isGoalDropdownOpen" @click.stop style="position: absolute; top: calc(100% + 8px); left: 0; z-index: 100; width: 300px; padding: 8px; box-shadow: 0 4px 12px rgba(0,0,0,0.15); border-radius: 3px; border: 1px solid #DFE1E6; background: white;">'''
content = content.replace(old_goal_text, new_goal_text)


# 3. Fix Project tab dropdown
old_proj_text = '''              <div style="position: relative;">
                 <div style="width: 80px; height: 80px; background-color: #EBECF0; border-radius: 8px; display: flex; align-items: center; justify-content: center; transform: rotate(-5deg);">
                    <i class="fa-solid fa-rocket" style="font-size: 32px; color: #172B4D;"></i>
                 </div>
                 <div style="position: absolute; bottom: -8px; right: -8px; width: 32px; height: 32px; background-color: #0052CC; color: white; border-radius: 50%; display: flex; align-items: center; justify-content: center; border: 2px solid white; cursor: pointer; box-shadow: 0 2px 4px rgba(0,0,0,0.1);" @click="goToProjects">
                    <i class="fa-solid fa-plus" style="font-size: 16px;"></i>
                 </div>
              </div>
              <div style="flex: 1;">
                 <h4 style="font-size: 14px; color: #172B4D; margin-bottom: 8px;">Chỉ định cho đội ngũ của bạn các dự án mà họ đang thực hiện</h4>
                 <p style="font-size: 13px; color: #6B778C; margin-bottom: 16px; line-height: 1.5;">Mục Dự án có thể giúp đội ngũ của bạn chia sẻ các bản cập nhật trạng thái hàng tuần với các bên liên quan trong tổ chức của bạn. Thêm đội ngũ của bạn vào các dự án phù hợp để xem tất cả dự án được liệt kê ở đây.</p>
                 <button class="secondary-btn" @click="goToProjects">Tạo dự án</button>
              </div>'''

new_proj_text = '''              <div style="position: relative;">
                 <div style="width: 80px; height: 80px; background-color: #EBECF0; border-radius: 8px; display: flex; align-items: center; justify-content: center; transform: rotate(-5deg);">
                    <i class="fa-solid fa-rocket" style="font-size: 32px; color: #172B4D;"></i>
                 </div>
                 <div style="position: absolute; bottom: -8px; right: -8px; width: 32px; height: 32px; background-color: #0052CC; color: white; border-radius: 50%; display: flex; align-items: center; justify-content: center; border: 2px solid white; cursor: pointer; box-shadow: 0 2px 4px rgba(0,0,0,0.1);" @click.stop="isProjectDropdownOpen = !isProjectDropdownOpen">
                    <i class="fa-solid fa-plus" style="font-size: 16px;"></i>
                 </div>
              </div>
              <div style="flex: 1; position: relative;">
                 <h4 style="font-size: 14px; color: #172B4D; margin-bottom: 8px;">Chỉ định cho đội ngũ của bạn các dự án mà họ đang thực hiện</h4>
                 <p style="font-size: 13px; color: #6B778C; margin-bottom: 16px; line-height: 1.5;">Mục Dự án có thể giúp đội ngũ của bạn chia sẻ các bản cập nhật trạng thái hàng tuần với các bên liên quan trong tổ chức của bạn. Thêm đội ngũ của bạn vào các dự án phù hợp để xem tất cả dự án được liệt kê ở đây.</p>
                 <button class="secondary-btn" @click.stop="isProjectDropdownOpen = !isProjectDropdownOpen">Thêm dự án</button>
                 
                 <!-- Project Dropdown Menu -->
                 <div class="dropdown-menu search-dropdown" v-if="isProjectDropdownOpen" @click.stop style="position: absolute; top: calc(100% + 8px); left: 0; z-index: 100; width: 300px; padding: 8px; box-shadow: 0 4px 12px rgba(0,0,0,0.15); border-radius: 3px; border: 1px solid #DFE1E6; background: white;">
                    <input type="text" v-model="projectSearch" placeholder="Tìm kiếm dự án..." class="search-input" style="width: 100%; margin-bottom: 12px; padding-left: 12px !important;" />
                    <h5 style="font-size: 11px; color: #6B778C; text-transform: uppercase; padding: 0 8px 8px;">Dự án gần đây</h5>
                    <div class="goal-list-options" style="max-height: 200px; overflow-y: auto;">
                      <div class="team-option" v-for="p in siteProjects" :key="p.id" @click="linkProject(p)" style="display: flex; align-items: center; gap: 8px; padding: 8px; cursor: pointer; border-radius: 3px;">
                        <div style="width: 24px; height: 24px; background-color: #0052CC; color: white; border-radius: 4px; display: flex; align-items: center; justify-content: center;"><i class="fa-solid fa-rocket" style="font-size: 12px;"></i></div>
                        <div style="display: flex; flex-direction: column;">
                          <span style="font-size: 13px; color: #172B4D;">{{ p.name }}</span>
                          <span style="font-size: 11px; color: #6B778C;">{{ p.ownerName || p.owner }}</span>
                        </div>
                      </div>
                    </div>
                    <div style="border-top: 1px solid #DFE1E6; margin-top: 8px; padding-top: 8px;">
                       <div class="team-option" style="display: flex; align-items: center; gap: 8px; padding: 8px; cursor: pointer; color: #172B4D;" @click="goToProjects">
                          <i class="fa-solid fa-plus"></i> <span style="font-size: 13px;">Tạo dự án</span>
                       </div>
                    </div>
                 </div>
              </div>'''
content = content.replace(old_proj_text, new_proj_text)

# Also need to add isProjectDropdownOpen, projectSearch to <script setup> if not exists
old_vars = '''const isGoalDropdownOpen = ref(false)
const goalSearch = ref('')'''

new_vars = '''const isGoalDropdownOpen = ref(false)
const goalSearch = ref('')
const isProjectDropdownOpen = ref(false)
const projectSearch = ref('')'''
content = content.replace(old_vars, new_vars)

# Fix click outside for project dropdown
# Find `<div v-if="currentTab === 'projects'" class="tab-pane">`
old_projects_tab = '''<div v-if="currentTab === 'projects'" class="tab-pane">'''
new_projects_tab = '''<div v-if="currentTab === 'projects'" class="tab-pane" @click="isProjectDropdownOpen = false">'''
content = content.replace(old_projects_tab, new_projects_tab)

with open(path, 'w', encoding='utf-8') as f:
    f.write(content)

print("Done python script")
