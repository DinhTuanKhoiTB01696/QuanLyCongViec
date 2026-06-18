<template>
  <div class="people-wrapper">
    <header class="module-header">
      <div class="header-content">
        <h1>Mọi người</h1>
        <div class="header-actions">
          <button class="primary-btn" @click="openInviteModal">Mời mọi người</button>
        </div>
      </div>
      
      <div class="tabs-nav">
        <router-link to="/home/teams" class="tab-link" exact-active-class="active">Dành cho bạn</router-link>
        <router-link to="/home/teams/list" class="tab-link" active-class="active">Tất cả các đội ngũ</router-link>
        <router-link to="/home/teams/kudos" class="tab-link" active-class="active">Khen ngợi</router-link>
        <router-link to="/home/people" class="tab-link" active-class="active">Mọi người</router-link>
      </div>
    </header>

    <div class="module-content">
      <div class="list-controls" style="display: flex; flex-direction: column; gap: 16px;">
        <div class="search-box-wrapper" style="width: 100%;">
          <Search class="w-4 h-4 search-icon"></Search>
          <input type="text" v-model="searchQuery" placeholder="Tìm kiếm người" class="search-input" />
        </div>
        
        <div class="filter-chips" style="display: flex; flex-wrap: wrap; gap: 8px;">
          <el-dropdown trigger="click" @command="val => selectedProject = val">
            <button class="filter-chip" :class="{ 'active-filter': selectedProject }">
              <Rocket class="w-4 h-4"></Rocket> {{ selectedProject || 'Lọc theo Dự án' }}
            </button>
            <template #dropdown>
              <el-dropdown-menu class="filter-dropdown-menu">
                <el-dropdown-item command="">Tất cả</el-dropdown-item>
                <el-dropdown-item v-if="uniqueProjects.length === 0" disabled>Không có thông tin</el-dropdown-item>
                <el-dropdown-item v-for="item in uniqueProjects" :key="item" :command="item">{{ item }}</el-dropdown-item>
              </el-dropdown-menu>
            </template>
          </el-dropdown>

          <el-dropdown trigger="click" @command="val => selectedGoal = val">
            <button class="filter-chip" :class="{ 'active-filter': selectedGoal }">
              <Target class="w-4 h-4"></Target> {{ selectedGoal || 'Mục tiêu' }}
            </button>
            <template #dropdown>
              <el-dropdown-menu class="filter-dropdown-menu">
                <el-dropdown-item command="">Tất cả</el-dropdown-item>
                <el-dropdown-item v-if="uniqueGoals.length === 0" disabled>Không có thông tin</el-dropdown-item>
                <el-dropdown-item v-for="item in uniqueGoals" :key="item" :command="item">{{ item }}</el-dropdown-item>
              </el-dropdown-menu>
            </template>
          </el-dropdown>

          <el-dropdown trigger="click" @command="val => selectedTeam = val">
            <button class="filter-chip" :class="{ 'active-filter': selectedTeam }">
              <User class="w-4 h-4"></User> {{ selectedTeam || 'Nhóm' }}
            </button>
            <template #dropdown>
              <el-dropdown-menu class="filter-dropdown-menu">
                <el-dropdown-item command="">Tất cả</el-dropdown-item>
                <el-dropdown-item v-if="uniqueTeams.length === 0" disabled>Không có thông tin</el-dropdown-item>
                <el-dropdown-item v-for="item in uniqueTeams" :key="item" :command="item">{{ item }}</el-dropdown-item>
              </el-dropdown-menu>
            </template>
          </el-dropdown>

          <el-dropdown trigger="click" @command="val => selectedPosition = val">
            <button class="filter-chip" :class="{ 'active-filter': selectedPosition }">
              <Briefcase class="w-4 h-4"></Briefcase> {{ selectedPosition || 'Chức danh' }}
            </button>
            <template #dropdown>
              <el-dropdown-menu class="filter-dropdown-menu">
                <el-dropdown-item command="">Tất cả</el-dropdown-item>
                <el-dropdown-item v-if="uniquePositions.length === 0" disabled>Không có thông tin</el-dropdown-item>
                <el-dropdown-item v-for="item in uniquePositions" :key="item" :command="item">{{ item }}</el-dropdown-item>
              </el-dropdown-menu>
            </template>
          </el-dropdown>



          <el-dropdown trigger="click" @command="val => selectedDepartment = val">
            <button class="filter-chip" :class="{ 'active-filter': selectedDepartment }">
              <Building class="w-4 h-4"></Building> {{ selectedDepartment || 'Phòng ban' }}
            </button>
            <template #dropdown>
              <el-dropdown-menu class="filter-dropdown-menu">
                <el-dropdown-item command="">Tất cả</el-dropdown-item>
                <el-dropdown-item v-if="uniqueDepartments.length === 0" disabled>Không có thông tin</el-dropdown-item>
                <el-dropdown-item v-for="item in uniqueDepartments" :key="item" :command="item">{{ item }}</el-dropdown-item>
              </el-dropdown-menu>
            </template>
          </el-dropdown>

          <el-dropdown trigger="click" @command="val => selectedLocation = val">
            <button class="filter-chip" :class="{ 'active-filter': selectedLocation }">
              <i class="fa-solid fa-location-dot"></i> {{ selectedLocation || 'Vị trí' }}
            </button>
            <template #dropdown>
              <el-dropdown-menu class="filter-dropdown-menu">
                <el-dropdown-item command="">Tất cả</el-dropdown-item>
                <el-dropdown-item v-if="uniqueLocations.length === 0" disabled>Không có thông tin</el-dropdown-item>
                <el-dropdown-item v-for="item in uniqueLocations" :key="item" :command="item">{{ item }}</el-dropdown-item>
              </el-dropdown-menu>
            </template>
          </el-dropdown>

          <!-- Clear Filters Button -->
          <button v-if="hasActiveFilters" class="clear-filters-btn" @click="clearFilters">
            Xóa bộ lọc
          </button>
        </div>
      </div>

      <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px; margin-top: 24px;">
        <div style="font-size: 14px; font-weight: 600; color: #172B4D;">{{ filteredUsers.length }} người</div>
        
        <div class="view-toggle" style="display: flex; border: 1px solid #DFE1E6; border-radius: 3px; overflow: hidden; height: 32px;">
          <button class="icon-btn" :style="{ background: viewMode === 'grid' ? '#DEEBFF' : 'white', color: viewMode === 'grid' ? '#0052CC' : '#6B778C' }" @click="viewMode = 'grid'" style="border: none; border-radius: 0; padding: 0 12px; height: 100%; display: flex; align-items: center; justify-content: center;"><Grid class="w-4 h-4"></Grid></button>
          <div style="width: 1px; background-color: #DFE1E6; height: 100%;"></div>
          <button class="icon-btn" :style="{ background: viewMode === 'table' ? '#DEEBFF' : 'white', color: viewMode === 'table' ? '#0052CC' : '#6B778C' }" @click="viewMode = 'table'" style="border: none; border-radius: 0; padding: 0 12px; height: 100%; display: flex; align-items: center; justify-content: center;"><i class="fa-solid fa-list"></i></button>
          <div style="width: 1px; background-color: #DFE1E6; height: 100%;"></div>
          <button class="icon-btn" style="border: none; border-radius: 0; background: white; color: #6B778C; padding: 0 12px; height: 100%; display: flex; align-items: center; justify-content: center;"><MoreHorizontal class="w-4 h-4"></MoreHorizontal></button>
        </div>
      </div>

      <div class="table-container" v-if="!isLoading">
        <!-- Grid View -->
        <div v-if="viewMode === 'grid' && filteredUsers.length > 0" style="display: grid; grid-template-columns: repeat(auto-fill, minmax(250px, 1fr)); gap: 16px;">
          <div class="people-card" v-for="(user, idx) in filteredUsers" :key="user.id" @click="goToProfile(user.id)" style="border: 1px solid #DFE1E6; border-radius: 3px; padding: 24px 16px; text-align: center; cursor: pointer; transition: background 0.2s, box-shadow 0.2s; display: flex; flex-direction: column; align-items: center;" onmouseover="this.style.boxShadow='0 4px 8px rgba(9, 30, 66, 0.15)'" onmouseout="this.style.boxShadow='none'">
            <div class="grid-avatar" :style="{ backgroundColor: getAvatarColor(user.fullName || user.email) }">{{ user.avatar }}</div>
            <div style="font-size: 14px; font-weight: 500; color: #172B4D; margin-bottom: 4px;">{{ user.fullName }}</div>
            <div v-if="user.email && user.email.includes('@')" style="font-size: 12px; color: #6B778C;">{{ user.email }}</div>
            <div v-if="!user.email || !user.email.includes('@')" style="font-size: 12px; color: #6B778C;">
               <div v-if="user.email">{{ user.email }}</div>
               <div v-if="user.location">{{ user.location }}</div>
            </div>
          </div>
        </div>

        <!-- Table View -->
        <table class="jira-table" v-if="viewMode === 'table' && filteredUsers.length > 0">
          <thead>
            <tr>
              <th class="col-name">Tên</th>
              <th>Chức danh nghề nghiệp</th>
              <th>Phòng ban</th>
              <th>Vị trí</th>
              <th>Thông tin liên hệ</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(user, idx) in filteredUsers" :key="user.id" @click="goToProfile(user.id)">
              <td>
                <div class="user-cell">
                  <div class="user-avatar" :style="{ backgroundColor: getAvatarColor(user.fullName || user.email) }">{{ user.avatar }}</div>
                  <div class="user-info-stack">
                    <span class="user-name">{{ user.fullName }}</span>
                  </div>
                </div>
              </td>
              <td>{{ user.position || '-' }}</td>
              <td>{{ user.department || '-' }}</td>
              <td>{{ user.location || '-' }}</td>
              <td>{{ user.email || '-' }}</td>
            </tr>
          </tbody>
        </table>
        
        <div class="empty-state" v-if="filteredUsers.length === 0">
          <div class="empty-icon-wrapper">
            <User class="w-4 h-4"></User>
          </div>
          <h3>Không tìm thấy ai</h3>
          <p>Chúng tôi không tìm thấy ai khớp với tiêu chí tìm kiếm của bạn.</p>
        </div>
      </div>
      
      <div class="loading-state" v-else>
        <div class="loader-spinner"></div>
      </div>
    </div>

    <!-- Invite Modal -->
    <div class="modal-overlay" v-if="isInviteModalOpen" @click.self="closeInviteModal">
      <div class="modal-content">
        <div class="modal-header">
          <h2>Mời mọi người tham gia SprintA</h2>
          <button class="close-btn" @click="closeInviteModal">&times;</button>
        </div>
        <div class="modal-body">
          <p class="invite-description">Mời thành viên mới vào Không gian làm việc của bạn qua email. Họ sẽ nhận được một email chứa liên kết để tham gia.</p>
          <div class="form-group">
            <label>Địa chỉ email</label>
            <div class="email-input-container" @click="focusEmailInput">
              <div class="email-chip" v-for="(email, index) in inviteEmails" :key="index">
                {{ email }}
                <X class="w-4 h-4 remove-chip" @click.stop="removeEmail(index)"></X>
              </div>
              <input 
                type="text" 
                v-model="emailInput" 
                placeholder="Ví dụ: name@example.com" 
                @keydown.enter.prevent="addEmail"
                @keydown.space.prevent="addEmail"
                @keydown.comma.prevent="addEmail"
                @blur="addEmail"
                ref="emailInputRef"
              />
            </div>
            <span class="helper-text">Nhập email và nhấn Enter hoặc Dấu phẩy để thêm nhiều người</span>
          </div>
          
          <div class="success-message" v-if="inviteSuccess">
            <CheckCircle2 class="w-4 h-4"></CheckCircle2> Đã gửi lời mời thành công!
          </div>
        </div>
        <div class="modal-footer">
          <button class="cancel-btn" @click="closeInviteModal">Hủy</button>
          <button class="primary-btn" :disabled="(inviteEmails.length === 0 && !emailInput) || isSending" @click="sendInvites">
            {{ isSending ? 'Đang gửi...' : 'Gửi lời mời' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { Search, Rocket, Target, User, Briefcase, Building, Grid, MoreHorizontal, X, CheckCircle2 } from 'lucide-vue-next';
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { usePeopleStore } from '@/store/usePeopleStore'
import { useTeamStore } from '@/store/useTeamStore'
import { useProjectStore } from '@/store/useProjectStore'
import { useGoalStore } from '@/store/useGoalStore'
import { getInitials, getAvatarColor } from '@/utils/avatarUtils'

const router = useRouter()
const peopleStore = usePeopleStore()
const teamStore = useTeamStore()
const projectStore = useProjectStore()
const goalStore = useGoalStore()

const searchQuery = ref('')
const viewMode = ref('grid')

// Filter States
const selectedProject = ref('')
const selectedGoal = ref('')
const selectedTeam = ref('')
const selectedPosition = ref('')
const selectedDepartment = ref('')
const selectedLocation = ref('')

const hasActiveFilters = computed(() => {
  return selectedProject.value || selectedGoal.value || selectedTeam.value || 
         selectedPosition.value || selectedDepartment.value || selectedLocation.value
})

const clearFilters = () => {
  selectedProject.value = ''
  selectedGoal.value = ''
  selectedTeam.value = ''
  selectedPosition.value = ''
  selectedDepartment.value = ''
  selectedLocation.value = ''
}

// Compute unique options
const uniqueProjects = computed(() => {
  const items = new Set()
  // Add from projects store
  if (projectStore.projects) {
    projectStore.projects.forEach(p => items.add(p.name))
  }
  // Fallback check users
  peopleStore.users?.forEach(u => {
    u.linkedProjects?.forEach(p => items.add(p.title || p.name))
  })
  return Array.from(items).filter(Boolean).sort()
})

const uniqueGoals = computed(() => {
  const items = new Set()
  if (goalStore.goals) {
    goalStore.goals.forEach(g => items.add(g.title))
  }
  peopleStore.users?.forEach(u => {
    u.linkedGoals?.forEach(g => items.add(g.title))
  })
  return Array.from(items).filter(Boolean).sort()
})

const uniqueTeams = computed(() => {
  const items = new Set()
  if (teamStore.allTeams) {
    teamStore.allTeams.forEach(t => { if (t.name) items.add(t.name) })
  }
  peopleStore.users?.forEach(u => {
    u.teamsList?.forEach(t => { if (t.name) items.add(t.name) })
  })
  return Array.from(items).filter(Boolean).sort()
})

const extractUnique = (extractor) => {
  const items = new Set()
  peopleStore.users?.forEach(u => extractor(u, items))
  return Array.from(items).sort()
}

const uniquePositions = computed(() => extractUnique((u, set) => {
  const val = u.position?.trim()
  if (val) set.add(val)
}))

const uniqueDepartments = computed(() => extractUnique((u, set) => {
  const val = u.department?.trim()
  if (val) set.add(val)
}))

const uniqueLocations = computed(() => extractUnique((u, set) => {
  const val = u.location?.trim()
  if (val) set.add(val)
}))

// Invite Modal State
const isInviteModalOpen = ref(false)
const emailInput = ref('')
const inviteEmails = ref([])
const emailInputRef = ref(null)
const isSending = ref(false)
const inviteSuccess = ref(false)

const openInviteModal = () => {
  isInviteModalOpen.value = true
  inviteEmails.value = []
  emailInput.value = ''
  inviteSuccess.value = false
}

const closeInviteModal = () => {
  isInviteModalOpen.value = false
}

const focusEmailInput = () => {
  emailInputRef.value?.focus()
}

const addEmail = () => {
  const emails = emailInput.value.split(/[\s,]+/).filter(e => e.trim() !== '')
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
  
  emails.forEach(email => {
    if (emailRegex.test(email) && !inviteEmails.value.includes(email)) {
      inviteEmails.value.push(email)
    }
  })
  emailInput.value = ''
}

const removeEmail = (index) => {
  inviteEmails.value.splice(index, 1)
}

const sendInvites = async () => {
  addEmail() // add whatever is currently in the input
  if (inviteEmails.value.length === 0) return
  
  isSending.value = true
  inviteSuccess.value = false
  
  try {
    // Simulate API call to send invites
    await new Promise(resolve => setTimeout(resolve, 1000))
    inviteSuccess.value = true
    
    // Auto close after 2 seconds
    setTimeout(() => {
      if (inviteSuccess.value) {
        closeInviteModal()
      }
    }, 2000)
  } catch (error) {
    console.error('Failed to send invites:', error)
  } finally {
    isSending.value = false
  }
}

onMounted(async () => {
  await Promise.all([
    peopleStore.fetchPeople(),
    teamStore.fetchAllTeams(),
    projectStore.fetchAllProjects(),
    goalStore.fetchGoals()
  ])
})

const isLoading = computed(() => peopleStore.isLoading)

const filteredUsers = computed(() => {
  let list = peopleStore.users || []

  // Apply Search
  if (searchQuery.value) {
    const q = searchQuery.value.toLowerCase()
    list = list.filter(u => 
      u.fullName?.toLowerCase().includes(q) || 
      u.position?.toLowerCase().includes(q) || 
      u.team?.toLowerCase().includes(q) ||
      u.department?.toLowerCase().includes(q)
    )
  }

  // Apply Filters
  if (selectedProject.value) {
    const projInfo = projectStore.projects?.find(p => p.name === selectedProject.value || p.title === selectedProject.value)
    list = list.filter(u => {
      if (u.linkedProjects?.some(p => (p.title || p.name) === selectedProject.value)) return true;
      if (projInfo) {
        if (projInfo.leadId === u.id) return true;
        if (projInfo.members?.some(m => m.id === u.id || m.userId === u.id)) return true;
      }
      return false;
    })
  }
  
  if (selectedGoal.value) {
    const goalInfo = goalStore.goals?.find(g => g.title === selectedGoal.value)
    list = list.filter(u => {
      if (u.linkedGoals?.some(g => g.title === selectedGoal.value)) return true;
      if (goalInfo) {
         if (goalInfo.ownerId === u.id || goalInfo.assignedUserId === u.id) return true;
         if (goalInfo.members?.some(m => m.id === u.id || m.userId === u.id)) return true;
      }
      return false;
    })
  }
  
  if (selectedTeam.value) {
    const teamInfo = teamStore.allTeams?.find(t => t.name === selectedTeam.value)
    list = list.filter(u => {
      if (u.teamsList?.some(t => t.name === selectedTeam.value)) return true;
      if (teamInfo) {
        if (teamInfo.leadId === u.id) return true;
        if (teamInfo.members?.some(m => m.id === u.id || m.userId === u.id)) return true;
      }
      return false;
    })
  }
  
  if (selectedPosition.value) {
    list = list.filter(u => u.position?.trim() === selectedPosition.value)
  }
  
  if (selectedDepartment.value) {
    list = list.filter(u => u.department?.trim() === selectedDepartment.value)
  }
  
  if (selectedLocation.value) {
    list = list.filter(u => u.location?.trim() === selectedLocation.value)
  }

  // Map to format with standard initials
  return list.map(u => ({
    ...u,
    avatar: getInitials(u.fullName || u.email)
  }))
})

const goToProfile = (id) => {
  router.push(`/home/profile/${id}`)
}
</script>

<style scoped>
.people-wrapper {
  display: flex;
  flex-direction: column;
  min-height: 100vh;
  background-color: #FFFFFF;
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
}

.module-header {
  padding: 32px 40px 0;
  background-color: #FFFFFF;
}

.header-content {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
}

.header-content h1 {
  font-size: 24px;
  font-weight: 500;
  color: #172B4D;
  margin: 0;
}

.primary-btn {
  background-color: #0052CC;
  color: white;
  border: none;
  padding: 8px 16px;
  border-radius: 3px;
  font-weight: 500;
  font-size: 14px;
  cursor: pointer;
  transition: background-color 0.2s;
}

.primary-btn:hover:not(:disabled) {
  background-color: #0047B3;
}

.primary-btn:disabled {
  background-color: rgba(9, 30, 66, 0.04);
  color: #A5ADBA;
  cursor: not-allowed;
}

.tabs-nav {
  display: flex;
  border-bottom: 2px solid #dfe1e6;
  gap: 24px;
}

.tab-link {
  padding: 8px 0 12px;
  font-size: 14px;
  font-weight: 500;
  color: #5e6c84;
  text-decoration: none;
  position: relative;
  margin-bottom: -2px;
  border-bottom: 2px solid transparent;
  transition: color 0.2s;
}

.tab-link:hover {
  color: #172b4d;
}

.tab-link.active {
  color: #0052cc;
  border-bottom-color: #0052cc;
}

.module-content {
  padding: 32px 40px 40px;
  flex: 1;
}

.list-controls {
  margin-bottom: 0px;
}

.filter-chip {
  background: white;
  border: 1px solid #DFE1E6;
  border-radius: 3px;
  padding: 6px 12px;
  font-size: 13px;
  font-weight: 500;
  color: #42526E;
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  transition: background 0.2s, border-color 0.2s, color 0.2s;
}

.filter-chip:hover {
  background: #FAFBFC;
}

.filter-chip.active-filter {
  background: #E6FCFF;
  color: #0052CC;
  border-color: #0052CC;
}

.clear-filters-btn {
  background: transparent;
  border: none;
  color: #0052CC;
  font-size: 13px;
  font-weight: 500;
  cursor: pointer;
  padding: 6px 12px;
  transition: color 0.2s;
}

.clear-filters-btn:hover {
  color: #0047B3;
  text-decoration: underline;
}

.search-box-wrapper {
  position: relative;
  width: 250px;
}

.search-icon {
  position: absolute;
  left: 10px;
  top: 50%;
  transform: translateY(-50%);
  color: #5E6C84;
  font-size: 14px;
}

.search-input {
  width: 100%;
  padding: 8px 12px 8px 44px;
  border: 2px solid #DFE1E6;
  border-radius: 3px;
  font-size: 14px;
  color: #172B4D;
  outline: none;
  transition: border-color 0.2s, background-color 0.2s;
  box-sizing: border-box;
}

.search-input:hover {
  background-color: #FAFBFC;
}

.search-input:focus {
  background-color: #FFFFFF;
  border-color: #4C9AFF;
}

.jira-table {
  width: 100%;
  border-collapse: collapse;
  text-align: left;
}

.jira-table th {
  padding: 8px 12px;
  font-size: 12px;
  font-weight: 600;
  color: #5E6C84;
  border-bottom: 2px solid #DFE1E6;
}

.jira-table th:hover {
  background-color: #FAFBFC;
  cursor: pointer;
}

.col-name {
  width: 25%;
}

.jira-table td {
  padding: 12px;
  font-size: 14px;
  color: #172B4D;
  border-bottom: 1px solid #DFE1E6;
  cursor: pointer;
}

.jira-table tbody tr:hover td {
  background-color: #FAFBFC;
}

.user-cell {
  display: flex;
  align-items: center;
  gap: 12px;
}

.user-avatar {
  width: 24px;
  height: 24px;
  color: white;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 10px;
  font-weight: bold;
}

.grid-avatar {
  width: 56px;
  height: 56px;
  color: white;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 20px;
  font-weight: bold;
  margin-bottom: 12px;
}

.user-info-stack {
  display: flex;
  flex-direction: column;
}

.user-name {
  font-weight: 500;
  color: #0052CC;
}

.jira-table tbody tr:hover .user-name {
  text-decoration: underline;
}

.empty-state {
  text-align: center;
  padding: 64px 20px;
}

.empty-icon-wrapper {
  width: 80px;
  height: 80px;
  background-color: #E6FCFF;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto 16px;
  color: #0052CC;
  font-size: 32px;
}

.empty-state h3 {
  margin: 0 0 8px 0;
  color: #172B4D;
  font-size: 20px;
}

.empty-state p {
  margin: 0;
  color: #5E6C84;
}

.loading-state {
  display: flex;
  justify-content: center;
  padding: 64px;
}

.loader-spinner {
  width: 32px;
  height: 32px;
  border: 3px solid #DFE1E6;
  border-top-color: #0052CC;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

/* Modal Styles */
.modal-overlay {
  position: fixed;
  top: 0; left: 0; right: 0; bottom: 0;
  background-color: rgba(9, 30, 66, 0.54);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.modal-content {
  background-color: #FFFFFF;
  border-radius: 3px;
  width: 500px;
  box-shadow: 0 8px 16px -4px rgba(9, 30, 66, 0.25);
  display: flex;
  flex-direction: column;
}

.modal-header {
  padding: 20px 24px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-bottom: 1px solid #DFE1E6;
}

.modal-header h2 {
  margin: 0;
  font-size: 20px;
  font-weight: 500;
  color: #172B4D;
}

.close-btn {
  background: none;
  border: none;
  font-size: 24px;
  color: #5E6C84;
  cursor: pointer;
}

.modal-body {
  padding: 24px;
}

.invite-description {
  margin: 0 0 16px 0;
  font-size: 14px;
  color: #42526E;
  line-height: 1.5;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.form-group label {
  font-size: 12px;
  font-weight: 600;
  color: #5E6C84;
}

.email-input-container {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 4px;
  padding: 4px;
  border: 2px solid #DFE1E6;
  border-radius: 3px;
  min-height: 40px;
  box-sizing: border-box;
  cursor: text;
  transition: border-color 0.2s;
}

.email-input-container:focus-within {
  border-color: #4C9AFF;
}

.email-input-container input {
  border: none !important;
  outline: none !important;
  flex: 1;
  min-width: 150px;
  padding: 4px 8px !important;
  font-size: 14px;
  color: #172B4D;
}

.email-chip {
  display: flex;
  align-items: center;
  gap: 6px;
  background-color: #FAFBFC;
  border: 1px solid #DFE1E6;
  border-radius: 3px;
  padding: 4px 8px;
  font-size: 12px;
  color: #172B4D;
}

.remove-chip {
  color: #5E6C84;
  cursor: pointer;
  font-size: 10px;
}

.remove-chip:hover {
  color: #DE350B;
}

.helper-text {
  font-size: 11px;
  color: #5E6C84;
}

.success-message {
  margin-top: 16px;
  padding: 12px 16px;
  background-color: #E3FCEF;
  color: #006644;
  border-radius: 3px;
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 14px;
  font-weight: 500;
}

.modal-footer {
  padding: 16px 24px;
  border-top: 1px solid #DFE1E6;
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}

.cancel-btn {
  background: transparent;
  border: none;
  color: #42526E;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  padding: 8px 12px;
  border-radius: 3px;
}

.cancel-btn:hover {
  background-color: rgba(9, 30, 66, 0.08);
}
</style>
