const fs = require('fs');
const path = 'c:\\Users\\tua46\\OneDrive\\Máy tính\\DATN\\QuanLyCongViec\\Frontend\\src\\views\\HomeSite\\Teams\\TeamDetail.vue';
let content = fs.readFileSync(path, 'utf8');

// 1. Add imports
const old_imports = `import { useTeamStore } from '@/store/useTeamStore'
import { usePeopleStore } from '@/store/usePeopleStore'`;

const new_imports = `import { useTeamStore } from '@/store/useTeamStore'
import { usePeopleStore } from '@/store/usePeopleStore'
import { useGoalStore } from '@/store/useGoalStore'
import { useHomeProjectStore } from '@/store/useHomeProjectStore'
import { getStoredUser } from '@/utils/permissions'`;

content = content.replace(old_imports, new_imports);

// 2. Add variables
const old_stores = `const teamStore = useTeamStore()
const peopleStore = usePeopleStore()`;

const new_stores = `const teamStore = useTeamStore()
const peopleStore = usePeopleStore()
const goalStore = useGoalStore()
const homeProjectStore = useHomeProjectStore()`;

content = content.replace(old_stores, new_stores);

// 3. Add computed properties for SiteOwner, Goals, Projects
const old_team_computed = `const team = computed(() => teamStore.currentTeam)
const isArchived = computed(() => team.value?.status === 'Archived')`;

const new_team_computed = `const team = computed(() => teamStore.currentTeam)
const isArchived = computed(() => team.value?.status === 'Archived')

const isSiteOwner = computed(() => {
  const currentUser = getStoredUser();
  if (!currentUser) return false;
  const r = (currentUser.role || '').toLowerCase();
  return r === 'owner' || r === 'developer' || r === 'dev';
})

const siteGoals = computed(() => {
  let list = goalStore.goals || [];
  if (goalSearch.value) {
    const q = goalSearch.value.toLowerCase();
    list = list.filter(g => g.title?.toLowerCase().includes(q) || g.name?.toLowerCase().includes(q));
  }
  return list;
})

const siteProjects = computed(() => {
  let list = homeProjectStore.projects || [];
  if (projectSearch.value) {
    const q = projectSearch.value.toLowerCase();
    list = list.filter(p => p.name?.toLowerCase().includes(q));
  }
  return list;
})`;

content = content.replace(old_team_computed, new_team_computed);

// 4. Update onMounted
const old_onmounted = `onMounted(async () => {
  const id = route.params.id
  await teamStore.fetchTeamDetail(id)`;

const new_onmounted = `onMounted(async () => {
  const id = route.params.id
  await teamStore.fetchTeamDetail(id)
  goalStore.fetchGoals()
  homeProjectStore.fetchProjects()`;

content = content.replace(old_onmounted, new_onmounted);

// 5. Replace mockRecentGoals with siteGoals
// Search for v-for="g in mockRecentGoals"
const old_mock_goals_vfor = `v-for="g in mockRecentGoals"`;
const new_mock_goals_vfor = `v-for="g in siteGoals"`;
content = content.replace(old_mock_goals_vfor, new_mock_goals_vfor);
content = content.replace(old_mock_goals_vfor, new_mock_goals_vfor); // Maybe 2 places

// Update Goal owner binding inside the list
const old_goal_owner = `{{ g.owner }}`;
const new_goal_owner = `{{ g.ownerName || g.owner }}`;
content = content.replace(old_goal_owner, new_goal_owner);
content = content.replace(old_goal_owner, new_goal_owner);

// Update linkGoal function to handle real goal properties properly
const old_linkGoal = `const linkGoal = (goal) => {
  if (!teamStore.goals) teamStore.goals = []
  if (!teamStore.goals.find(g => g.id === goal.id)) {
    teamStore.goals.push({ ...goal, status: 'Đã hoàn tất 🚀' })
  }
  isGoalDropdownOpen.value = false
}`;

const new_linkGoal = `const linkGoal = async (goal) => {
  try {
    // Ideally this connects via API, but we'll mock the UI link for now until backend is ready for Team-Goal links
    if (!teamStore.goals) teamStore.goals = []
    if (!teamStore.goals.find(g => g.id === goal.id)) {
      teamStore.goals.push({ id: goal.id, title: goal.title || goal.name, status: goal.status || 'Active', owner: goal.ownerName || goal.owner })
    }
  } catch (err) {}
  isGoalDropdownOpen.value = false
}`;

content = content.replace(old_linkGoal, new_linkGoal);


// 6. Replace mockRecentProjects with siteProjects
const old_mock_projects_vfor = `v-for="p in mockRecentProjects"`;
const new_mock_projects_vfor = `v-for="p in siteProjects"`;
content = content.replace(old_mock_projects_vfor, new_mock_projects_vfor);
content = content.replace(old_mock_projects_vfor, new_mock_projects_vfor); // Maybe 2 places

// Also update linkProject function
const old_linkProject = `const linkProject = (project) => {
  if (!teamStore.projects) teamStore.projects = []
  if (!teamStore.projects.find(p => p.id === project.id)) {
    teamStore.projects.push({ ...project, status: 'Đang thực hiện' })
  }
  isProjectDropdownOpen.value = false
}`;

const new_linkProject = `const linkProject = async (project) => {
  try {
    if (!teamStore.projects) teamStore.projects = []
    if (!teamStore.projects.find(p => p.id === project.id)) {
      teamStore.projects.push({ id: project.id, name: project.name, status: project.status || 'Active' })
    }
  } catch (err) {}
  isProjectDropdownOpen.value = false
}`;

content = content.replace(old_linkProject, new_linkProject);


// 7. Fix Manager Display
const old_manager_display = `         <div class="meta-item-row" style="display: flex; align-items: center;">
            <span style="width: 120px; color: #6B778C; font-size: 13px;">Người quản lý</span>
            <span style="flex: 1; font-size: 13px; color: #172B4D;">Đang cập nhật</span>
         </div>`;

const new_manager_display = `         <div class="meta-item-row" style="display: flex; align-items: center; position: relative;">
            <span style="width: 120px; color: #6B778C; font-size: 13px;">Người quản lý</span>
            <div style="flex: 1; position: relative;">
              <span v-if="team.manager" style="font-size: 13px; color: #172B4D; cursor: pointer; display: flex; align-items: center; gap: 8px;" @click="isSiteOwner && (isManagerDropdownOpen = !isManagerDropdownOpen)">
                <UserAvatar :user="team.manager" :size="24" :fontSize="10" />
                {{ team.manager.name || team.manager.fullName || team.manager.email }}
              </span>
              <span v-else style="font-size: 13px; color: #172B4D; cursor: pointer;" @click="isSiteOwner && (isManagerDropdownOpen = !isManagerDropdownOpen)">
                 Đang cập nhật
              </span>
              <div class="dropdown-menu search-dropdown" v-if="isManagerDropdownOpen" @click.stop style="position: absolute; top: 100%; left: 0; z-index: 100; width: 300px; padding: 8px; box-shadow: 0 4px 12px rgba(0,0,0,0.15); border-radius: 3px; border: 1px solid #DFE1E6; background: white;">
                  <input type="text" v-model="managerSearch" placeholder="Tìm kiếm thành viên..." class="search-input" style="width: 100%; margin-bottom: 12px; padding-left: 12px !important;" />
                  <div class="goal-list-options" style="max-height: 200px; overflow-y: auto;">
                    <div class="team-option" v-for="user in filteredManagerUsers" :key="user.id" @click="setManager(user)" style="display: flex; align-items: center; gap: 8px; padding: 8px; cursor: pointer; border-radius: 3px;">
                      <UserAvatar :user="user" :size="24" :fontSize="10" />
                      <div style="display: flex; flex-direction: column;">
                        <span style="font-size: 13px; color: #172B4D;">{{ user.fullName || user.email }}</span>
                      </div>
                    </div>
                    <div v-if="filteredManagerUsers.length === 0" style="padding: 8px; font-size: 12px; color: #6B778C;">Không tìm thấy.</div>
                  </div>
              </div>
            </div>
         </div>`;

content = content.replace(old_manager_display, new_manager_display);

// Also add manager variables
const managerVars = `
const isManagerDropdownOpen = ref(false)
const managerSearch = ref('')
const filteredManagerUsers = computed(() => {
  let list = peopleStore.users || []
  if (managerSearch.value) {
    const q = managerSearch.value.toLowerCase()
    list = list.filter(u => (u.fullName || '').toLowerCase().includes(q) || (u.email || '').toLowerCase().includes(q))
  }
  return list
})
const setManager = async (user) => {
  try {
    await teamStore.updateTeam({ managerId: user.id })
    if (!team.value.manager) team.value.manager = {}
    team.value.manager = user
  } catch(e) {}
  isManagerDropdownOpen.value = false
}
`;

content = content.replace(`const isCreateGoalOpen = ref(false)`, `const isCreateGoalOpen = ref(false)` + managerVars);

// Also update goal list empty state "tạo mục tiêu" handler
// Since there's no addGoal in goalStore we just do the normal create flow.
// Actually we leave the "goToCreateGoal" logic as is `isCreateGoalOpen = true`.

fs.writeFileSync(path, content, 'utf8');
console.log("TeamDetail patch 2 done");
