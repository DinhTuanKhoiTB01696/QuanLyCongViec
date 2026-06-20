<template>
  <div class="goals-wrapper">
    <header class="module-header">
      <div class="header-content">
        <h1>Mục tiêu</h1>
        <div class="header-actions">
          <button class="primary-btn" @click="openCreateModal">Tạo mục tiêu</button>
        </div>
      </div>

      <div class="tabs-nav">
        <button class="tab-btn" :class="{ active: currentTab === 'all' }" @click="currentTab = 'all'">Thư mục mục tiêu</button>
        <button class="tab-btn" :class="{ active: currentTab === 'following' }" @click="currentTab = 'following'">Đang theo dõi</button>
        <button class="tab-btn" :class="{ active: currentTab === 'archived' }" @click="currentTab = 'archived'">Đã lưu trữ</button>
      </div>
    </header>

    <div class="module-content">
      <!-- Tab: Dành cho bạn -->
      <div v-if="currentTab === 'foryou'" class="tab-foryou">
        <div class="empty-state-banner">
          <div class="empty-banner-content">
            <div class="empty-banner-text">
              <h2>Mục tiêu của bạn</h2>
              <p>Bạn chưa được gán cho hoặc là cộng tác viên trên bất kỳ mục tiêu nào.</p>
              <div class="empty-banner-actions">
                <button class="primary-btn" @click="openCreateModal">Tạo mục tiêu</button>
                <a href="#" class="secondary-btn">Tìm hiểu thêm</a>
              </div>
            </div>
            <div class="empty-banner-illustration">
              <div class="mock-illustration">
                <Target class="w-4 h-4"></Target>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Tab: Tất cả mục tiêu & Đã lưu trữ -->
      <div v-else class="tab-all-archived">
        <div class="list-controls" style="display: flex; flex-direction: column; gap: 16px;">
          <div class="search-box-wrapper" style="width: 100%;">
            <Search class="w-4 h-4 search-icon"></Search>
            <input type="text" v-model="searchQuery" placeholder="Tìm kiếm" class="search-input" />
          </div>
          <div class="filter-chips" style="display: flex; flex-wrap: wrap; gap: 8px;">
            <button class="filter-chip" v-if="currentTab === 'following'" style="background-color: #E6FCFF; color: #0052CC; border: 1px solid #4C9AFF; cursor: default;">
              Đang theo dõi <X class="w-4 h-4" style="cursor: pointer;" @click="currentTab = 'all'"></X>
            </button>

            <el-dropdown trigger="click" @command="val => selectedProject = val">
              <button class="filter-chip" :class="{ 'active-filter': selectedProject }">
                <Rocket class="w-4 h-4"></Rocket> {{ selectedProject || 'Dự án' }}
              </button>
              <template #dropdown>
                <el-dropdown-menu class="filter-dropdown-menu">
                  <el-dropdown-item command="">Tất cả</el-dropdown-item>
                  <el-dropdown-item v-if="uniqueProjects.length === 0" disabled>Không có thông tin</el-dropdown-item>
                  <el-dropdown-item v-for="item in uniqueProjects" :key="item" :command="item">{{ item }}</el-dropdown-item>
                </el-dropdown-menu>
              </template>
            </el-dropdown>

            <el-dropdown trigger="click" @command="val => selectedTeam = val">
              <button class="filter-chip" :class="{ 'active-filter': selectedTeam }">
                <Users class="w-4 h-4"></Users> {{ selectedTeam || 'Nhóm' }}
              </button>
              <template #dropdown>
                <el-dropdown-menu class="filter-dropdown-menu">
                  <el-dropdown-item command="">Tất cả</el-dropdown-item>
                  <el-dropdown-item v-if="uniqueTeams.length === 0" disabled>Không có thông tin</el-dropdown-item>
                  <el-dropdown-item v-for="item in uniqueTeams" :key="item" :command="item">{{ item }}</el-dropdown-item>
                </el-dropdown-menu>
              </template>
            </el-dropdown>

            <el-dropdown trigger="click" @command="val => selectedProgress = val">
              <button class="filter-chip" :class="{ 'active-filter': selectedProgress }">
                <Activity class="w-4 h-4"></Activity> {{ selectedProgress || 'Tiến độ' }}
              </button>
              <template #dropdown>
                <el-dropdown-menu class="filter-dropdown-menu">
                  <el-dropdown-item command="">Tất cả</el-dropdown-item>
                  <el-dropdown-item command="0%">Chưa bắt đầu (0%)</el-dropdown-item>
                  <el-dropdown-item command="1-99%">Đang thực hiện (1-99%)</el-dropdown-item>
                  <el-dropdown-item command="100%">Hoàn thành (100%)</el-dropdown-item>
                </el-dropdown-menu>
              </template>
            </el-dropdown>

            <el-dropdown trigger="click" @command="val => selectedOwner = val">
              <button class="filter-chip" :class="{ 'active-filter': selectedOwner }">
                <User class="w-4 h-4"></User> {{ selectedOwner || 'Chủ sở hữu' }}
              </button>
              <template #dropdown>
                <el-dropdown-menu class="filter-dropdown-menu">
                  <el-dropdown-item command="">Tất cả</el-dropdown-item>
                  <el-dropdown-item v-if="uniqueOwners.length === 0" disabled>Không có thông tin</el-dropdown-item>
                  <el-dropdown-item v-for="item in uniqueOwners" :key="item" :command="item">{{ item }}</el-dropdown-item>
                </el-dropdown-menu>
              </template>
            </el-dropdown>

            <el-dropdown trigger="click" @command="val => selectedStatus = val">
              <button class="filter-chip" :class="{ 'active-filter': selectedStatus }">
                <CheckCircle class="w-4 h-4"></CheckCircle> {{ selectedStatus || 'Trạng thái' }}
              </button>
              <template #dropdown>
                <el-dropdown-menu class="filter-dropdown-menu">
                  <el-dropdown-item command="">Tất cả</el-dropdown-item>
                  <el-dropdown-item v-if="uniqueStatuses.length === 0" disabled>Không có thông tin</el-dropdown-item>
                  <el-dropdown-item v-for="item in uniqueStatuses" :key="item" :command="item">{{ item }}</el-dropdown-item>
                </el-dropdown-menu>
              </template>
            </el-dropdown>

            <el-dropdown trigger="click" @command="val => selectedStartDate = val">
              <button class="filter-chip" :class="{ 'active-filter': selectedStartDate }">
                <Calendar class="w-4 h-4"></Calendar> {{ selectedStartDate || 'Ngày bắt đầu' }}
              </button>
              <template #dropdown>
                <el-dropdown-menu class="filter-dropdown-menu">
                  <el-dropdown-item command="">Tất cả</el-dropdown-item>
                  <el-dropdown-item command="Tuần này">Tuần này</el-dropdown-item>
                  <el-dropdown-item command="Tháng này">Tháng này</el-dropdown-item>
                  <el-dropdown-item command="Quý này">Quý này</el-dropdown-item>
                </el-dropdown-menu>
              </template>
            </el-dropdown>

            <el-dropdown trigger="click" @command="val => selectedEndDate = val">
              <button class="filter-chip" :class="{ 'active-filter': selectedEndDate }">
                <CalendarDays class="w-4 h-4"></CalendarDays> {{ selectedEndDate || 'Ngày kết thúc' }}
              </button>
              <template #dropdown>
                <el-dropdown-menu class="filter-dropdown-menu">
                  <el-dropdown-item command="">Tất cả</el-dropdown-item>
                  <el-dropdown-item v-if="uniqueEndDates.length === 0" disabled>Không có thông tin</el-dropdown-item>
                  <el-dropdown-item v-for="item in uniqueEndDates" :key="item" :command="item">{{ item }}</el-dropdown-item>
                </el-dropdown-menu>
              </template>
            </el-dropdown>
          </div>
        </div>

        <div class="table-container" v-if="!isLoading">
          <table class="jira-table" v-if="filteredGoals.length > 0">
            <thead>
              <tr>
                <th class="col-title">Mục tiêu</th>
                <th class="col-status">Trạng thái</th>
                <th class="col-progress">Tiến độ</th>
                <th class="col-report">Báo cáo nội bộ</th>
                <th class="col-labels">Nhãn</th>
                <th class="col-owner">Chủ sở hữu</th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="goal in filteredGoals"
                :key="goal.id || goal.goalId"
                tabindex="0"
                @click="goToGoal(goal)"
                @keydown.enter.prevent="goToGoal(goal)"
                @keydown.space.prevent="goToGoal(goal)"
              >
                <td>
                  <div class="goal-title-cell">
                    <span class="goal-icon"><Target class="w-4 h-4"></Target></span>
                    <span class="goal-title">{{ goal.title }}</span>
                  </div>
                </td>
                <td>
                  <span class="status-badge" :class="getStatusClass(goal.status)">
                    <span class="status-dot"></span>
                    {{ goal.status }}
                  </span>
                </td>
                <td>
                  <div class="progress-cell">
                    <div class="progress-bar-bg">
                      <div class="progress-bar-fill" :style="{ width: (goal.progress || 0) + '%' }"></div>
                    </div>
                    <span class="progress-text">{{ goal.progress || 0 }}%</span>
                  </div>
                </td>
                <td><span class="report-text">{{ goal.innerReport || '-' }}</span></td>
                <td>
                  <div class="labels-container" v-if="goal.labels && goal.labels.length > 0">
                    <span class="label-badge" v-for="lbl in goal.labels" :key="lbl">{{ lbl }}</span>
                  </div>
                  <span v-else>-</span>
                </td>
                <td>
                  <div class="owner-cell">
                    <UserAvatar :user="goal.owner" size="sm" />
                    <span class="owner-name">{{ goal.owner?.fullName || goal.owner?.email || 'Chưa gán' }}</span>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>

          <div class="empty-state" v-else>
            <div class="empty-icon-wrapper">
              <Target class="w-4 h-4"></Target>
            </div>
            <h3>Không tìm thấy mục tiêu nào</h3>
            <p>Không có mục tiêu nào khớp với bộ lọc của bạn.</p>
          </div>
        </div>

        <div class="loading-state" v-else>
          <div class="loader-spinner"></div>
        </div>
      </div>
    </div>

    <GoalCreateModal
      v-model="isCreateModalOpen"
      :users="siteUsers"
      :current-user="currentUser"
      @created="goalStore.fetchGoals"
    />

  </div>
</template>

<script setup>
import { Target, Search, X, ChevronDown, Calendar, ChevronLeft, ChevronRight, Rocket, Users, Activity, User, CheckCircle, CalendarDays } from 'lucide-vue-next';
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { getStoredUser } from '@/utils/permissions'
import { useGoalStore } from '@/store/useGoalStore'
import { usePeopleStore } from '@/store/usePeopleStore'
import UserAvatar from '@/components/common/UserAvatar.vue'
import GoalCreateModal from './GoalCreateModal.vue'

const router = useRouter()
const goalStore = useGoalStore()
const peopleStore = usePeopleStore()

const currentTab = ref('all')
const searchQuery = ref('')
const isCreateModalOpen = ref(false)

// Filter State
const selectedProject = ref('')
const selectedTeam = ref('')
const selectedProgress = ref('')
const selectedOwner = ref('')
const selectedStatus = ref('')
const selectedStartDate = ref('')
const selectedEndDate = ref('')

// Computed Filter Options
const uniqueProjects = computed(() => {
  const projs = goalStore.goals.flatMap(g => g.linkedProjects || []).map(p => p.name).filter(Boolean)
  return [...new Set(projs)]
})

const uniqueTeams = computed(() => {
  const tms = goalStore.goals.map(g => g.department?.name).filter(Boolean)
  return [...new Set(tms)]
})

const uniqueOwners = computed(() => {
  const owns = goalStore.goals.map(g => g.owner?.fullName || g.owner?.email).filter(Boolean)
  return [...new Set(owns)]
})

const uniqueStatuses = computed(() => {
  const stats = goalStore.goals.map(g => g.status).filter(Boolean)
  return [...new Set(stats)]
})

const uniqueEndDates = computed(() => {
  const dates = goalStore.goals.map(g => {
    if (!g.dueDate) return null
    return new Date(g.dueDate).toLocaleDateString('vi-VN')
  }).filter(Boolean)
  return [...new Set(dates)]
})

const siteUsers = computed(() => {
  return peopleStore.users || []
})

const currentUser = computed(() => {
  try {
    const localUser = getStoredUser() || {}
    const fullUser = siteUsers.value.find(u => u.id === localUser.id)
    return fullUser || localUser
  } catch {
    return {}
  }
})

const currentUserName = computed(() => {
  const u = currentUser.value
  return u.fullName || u.username || u.email || 'Người dùng'
})


onMounted(async () => {
  await goalStore.fetchGoals()
  await peopleStore.fetchPeople()
  window.addEventListener('global-create-click', openCreateModal)
})

onUnmounted(() => {
  window.removeEventListener('global-create-click', openCreateModal)
})

const isLoading = computed(() => goalStore.isLoading)

const filteredGoals = computed(() => {
  let list = goalStore.goals || []

  // Lọc theo tab
  if (currentTab.value === 'archived') {
    list = list.filter(g => g.isArchived)
  } else if (currentTab.value === 'following') {
    list = list.filter(g => !g.isArchived && g.isFollowing)
  } else {
    list = list.filter(g => !g.isArchived)
  }

  // Lọc theo Project
  if (selectedProject.value) {
    list = list.filter(g => g.linkedProjects && g.linkedProjects.some(p => p.name === selectedProject.value))
  }

  // Lọc theo Team
  if (selectedTeam.value) {
    list = list.filter(g => g.department?.name === selectedTeam.value)
  }

  // Lọc theo Tiến độ
  if (selectedProgress.value) {
    if (selectedProgress.value === '0%') {
      list = list.filter(g => !g.progress || g.progress === 0)
    } else if (selectedProgress.value === '100%') {
      list = list.filter(g => g.progress === 100)
    } else {
      list = list.filter(g => g.progress > 0 && g.progress < 100)
    }
  }

  // Lọc theo Chủ sở hữu
  if (selectedOwner.value) {
    list = list.filter(g => (g.owner?.fullName || g.owner?.email) === selectedOwner.value)
  }

  // Lọc theo Trạng thái
  if (selectedStatus.value) {
    list = list.filter(g => g.status === selectedStatus.value)
  }

  // Lọc theo Ngày kết thúc
  if (selectedEndDate.value) {
    list = list.filter(g => {
      if (!g.dueDate) return false
      return new Date(g.dueDate).toLocaleDateString('vi-VN') === selectedEndDate.value
    })
  }

  // Tìm kiếm
  if (searchQuery.value) {
    const q = searchQuery.value.toLowerCase()
    list = list.filter(g =>
      g.title.toLowerCase().includes(q) ||
      (g.status && g.status.toLowerCase().includes(q)) ||
      (g.owner && (g.owner.fullName || g.owner.email) && (g.owner.fullName || g.owner.email).toLowerCase().includes(q))
    )
  }

  return list
})

const getStatusClass = (status) => {
  if (!status) return 'status-pending'
  const map = {
    'đúng tiến độ': 'status-on-track',
    'có rủi ro': 'status-at-risk',
    'trễ tiến độ': 'status-off-track',
    'không đúng tiến độ': 'status-off-track',
    'đang chờ cập nhật': 'status-pending',
    'đang chờ xử lý': 'status-pending',
    'đã hoàn tất': 'status-done',
    'đã lưu trữ': 'status-archived'
  }
  return map[status.toLowerCase()] || 'status-pending'
}

const openCreateModal = () => {
  isCreateModalOpen.value = true
}



const goToGoal = (goal) => {
  const id = goal?.id || goal?.goalId
  if (!id) return
  router.push({ name: 'HomeGoalDetail', params: { id } })
}
</script>

<style scoped>
.goals-wrapper {
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
  background-color: #EBECF0;
  color: #A5ADBA;
  cursor: not-allowed;
}

.secondary-btn {
  background-color: transparent;
  color: #0052CC;
  border: none;
  padding: 8px 16px;
  border-radius: 3px;
  font-weight: 500;
  font-size: 14px;
  cursor: pointer;
  text-decoration: none;
  transition: background-color 0.2s;
}

.secondary-btn:hover {
  background-color: rgba(9, 30, 66, 0.08);
  text-decoration: underline;
}

.tabs-nav {
  display: flex;
  border-bottom: 2px solid #DFE1E6;
  gap: 24px;
}

.tab-btn {
  background: none;
  border: none;
  padding: 8px 0 12px;
  font-size: 14px;
  font-weight: 500;
  color: #5E6C84;
  cursor: pointer;
  position: relative;
  margin-bottom: -2px;
  border-bottom: 2px solid transparent;
  transition: color 0.2s;
}

.tab-btn:hover {
  color: #172B4D;
}

.tab-btn.active {
  color: #0052CC;
  border-bottom-color: #0052CC;
}

.module-content {
  padding: 32px 40px;
  flex: 1;
}

/* Empty State Dành cho bạn */
.empty-state-banner {
  background-color: #FAFBFC;
  border-radius: 8px;
  overflow: hidden;
}

.empty-banner-content {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 40px 64px;
  max-width: 1000px;
  margin: 0 auto;
}

.empty-banner-text {
  flex: 1;
  max-width: 400px;
}

.empty-banner-text h2 {
  font-size: 24px;
  font-weight: 500;
  color: #172B4D;
  margin: 0 0 16px 0;
}

.empty-banner-text p {
  font-size: 16px;
  color: #42526E;
  margin: 0 0 32px 0;
  line-height: 1.5;
}

.empty-banner-actions {
  display: flex;
  gap: 16px;
  align-items: center;
}

.empty-banner-illustration {
  flex: 1;
  display: flex;
  justify-content: flex-end;
}

.mock-illustration {
  width: 280px;
  height: 200px;
  background-color: #E6FCFF;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.mock-illustration i {
  font-size: 64px;
  color: #0052CC;
}

/* List Controls */
.list-controls {
  margin-bottom: 24px;
}

.search-box-wrapper {
  position: relative;
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

/* Table */
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

.col-title { width: 30%; }
.col-status { width: 15%; }
.col-progress { width: 15%; }
.col-report { width: 15%; }
.col-labels { width: 15%; }
.col-owner { width: 10%; }

.jira-table td {
  padding: 12px;
  font-size: 14px;
  color: #172B4D;
  border-bottom: 1px solid #DFE1E6;
  vertical-align: middle;
}

.jira-table tbody tr {
  cursor: pointer;
}

.jira-table tbody tr:hover td {
  background-color: #FAFBFC;
}

.jira-table tbody tr:focus-visible td {
  background-color: #E6FCFF;
  outline: 2px solid #4C9AFF;
  outline-offset: -2px;
}

.goal-title-cell {
  display: flex;
  align-items: center;
  gap: 8px;
}

.goal-icon {
  color: #0052CC;
  font-size: 16px;
}

.goal-title {
  font-weight: 500;
  color: #172B4D;
}

.goal-title:hover {
  color: #0052CC;
}

/* Status Badge matching Jira exactly */
.status-badge {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 2px 6px;
  border-radius: 3px;
  font-size: 11px;
  font-weight: 700;
  text-transform: uppercase;
}

.status-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
}

/* Colors for Statuses */
.status-on-track { background-color: #E3FCEF; color: #006644; }
.status-on-track .status-dot { background-color: #36B37E; }

.status-at-risk { background-color: #FFF0B3; color: #FF8B00; }
.status-at-risk .status-dot { background-color: #FFAB00; }

.status-off-track { background-color: #FFEBE6; color: #BF2600; }
.status-off-track .status-dot { background-color: #FF5630; }

.status-done { background-color: #EAE6FF; color: #403294; }
.status-done .status-dot { background-color: #6554C0; }

.status-pending, .status-archived { background-color: #DFE1E6; color: #42526E; }
.status-pending .status-dot, .status-archived .status-dot { background-color: #7A869A; }

/* Progress Bar */
.progress-cell {
  display: flex;
  align-items: center;
  gap: 8px;
}

.progress-bar-bg {
  flex: 1;
  height: 6px;
  background-color: #DFE1E6;
  border-radius: 3px;
  overflow: hidden;
}

.progress-bar-fill {
  height: 100%;
  background-color: #0052CC;
  border-radius: 3px;
}

.progress-text {
  font-size: 12px;
  color: #5E6C84;
  min-width: 28px;
}

/* Labels */
.labels-container {
  display: flex;
  flex-wrap: wrap;
  gap: 4px;
}

.label-badge {
  background-color: #DFE1E6;
  color: #42526E;
  font-size: 12px;
  padding: 2px 6px;
  border-radius: 3px;
  white-space: nowrap;
}

/* Owner */
.owner-cell {
  display: flex;
  align-items: center;
  gap: 8px;
}

.owner-avatar {
  width: 24px;
  height: 24px;
  background-color: #0052CC;
  color: white;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 10px;
  font-weight: bold;
}

.owner-name {
  color: #172B4D;
}

/* Empty State Table */
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

</style>
