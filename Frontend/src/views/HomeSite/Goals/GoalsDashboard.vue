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
              <div class="empty-illustration">
                <i class="fa-solid fa-bullseye"></i>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Tab: Tất cả mục tiêu & Đã lưu trữ -->
      <div v-else class="tab-all-archived">
        <div class="list-controls">
          <div class="search-box-wrapper">
            <i class="fa-solid fa-magnifying-glass search-icon"></i>
            <input type="text" v-model="searchQuery" placeholder="Tìm kiếm" class="search-input" />
          </div>
          <div class="filter-actions">
            <button class="filter-btn" v-if="currentTab === 'following'" style="background-color: #E6FCFF; color: #0052CC; border: 1px solid #4C9AFF;">Đang theo dõi <i class="fa-solid fa-xmark"></i></button>
            
            
            
          </div>
          <div class="filter-chips" style="display: flex; flex-wrap: wrap; gap: 8px; margin-top: 12px; width: 100%;">
            <DropdownFilter label="Trạng thái" :options="statusOptions" v-model="filters.status" />
            <DropdownFilter label="Chủ sở hữu" :options="ownerOptions" v-model="filters.owner" />
            <DropdownFilter label="Tiến độ" :options="progressOptions" v-model="filters.progress" />
            <DropdownFilter label="Yêu thích" :options="booleanOptions" v-model="filters.favorite" />
            <DropdownFilter label="Theo dõi" :options="booleanOptions" v-model="filters.following" />
            <button v-if="hasActiveFilters" class="clear-filters-btn" @click="clearFilters">Xóa lọc</button>
          </div>

        </div>

        <div class="table-container" v-if="!isLoading">
          <table class="jira-table" v-if="filteredGoals.length > 0">
            <thead>
              <tr>
                <th class="col-title">Mục tiêu</th>
                <th class="col-status">Trạng thái</th>
                <th class="col-progress">Tiến độ</th>
                <th class="col-created">Ngày tạo</th>
     <th class="col-updated">Ngày cập nhật</th>
     <th class="col-star">Yêu thích</th>
     <th class="col-watch">Theo dõi</th>
                <th class="col-owner">Chủ sở hữu</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="goal in filteredGoals" :key="goal.id" @click="goToGoal(goal.id)">
                <td>
                  <div class="goal-title-cell">
                    <span class="goal-icon"><i class="fa-solid fa-bullseye"></i></span>
                    <span class="goal-title">{{ goal.title }}</span>
                  </div>
                </td>
                <td>
                  <span class="status-badge" :class="getStatusClass(goal.status)">
                    <span class="status-dot"></span>
                    {{ translateStatus(goal.status) }}
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
                <td>{{ goal.createdAt ? new Date(goal.createdAt).toLocaleDateString('vi-VN') : '-' }}</td>
                <td>{{ goal.updatedAt ? new Date(goal.updatedAt).toLocaleDateString('vi-VN') : '-' }}</td>
                <td @click.stop="toggleStar(goal)">
                  <i :class="goal.isStarred ? 'fa-solid fa-star text-yellow-400' : 'fa-regular fa-star text-gray-400'" style="cursor: pointer;"></i>
                </td>
                <td @click.stop="toggleWatch(goal)">
                  <span :class="goal.isFollowing ? 'text-blue-500' : 'text-gray-500'" style="cursor: pointer;">{{ goal.isFollowing ? 'Đang theo dõi' : 'Theo dõi' }}</span>
                </td>
                <td>
                  <div class="owner-cell">
                    <UserAvatar :user="{ id: goal.ownerId, fullName: goal.ownerName, avatarColor: goal.ownerColor, avatarUrl: goal.ownerAvatarUrl }" :size="24" :fontSize="10" />
                    <span class="owner-name">{{ goal.owner || 'Chưa gán' }}</span>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
          
          <div class="empty-state" v-else>
            <div class="empty-icon-wrapper">
              <i class="fa-solid fa-bullseye"></i>
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

    <!-- Create Goal Modal -->
    <div class="modal-overlay" v-if="isCreateModalOpen" @click.self="isCreateModalOpen = false">
      <div class="modal-content" style="width: 500px; padding: 0;">
        <div class="modal-header" style="border-bottom: none; padding-bottom: 0;">
          <h2 style="display: flex; align-items: center; gap: 8px;">
             <i class="fa-solid fa-bullseye" style="color: #6B778C;"></i> Tạo mục tiêu
          </h2>
        </div>
        <div class="modal-body" style="padding-top: 12px;">
          <p style="font-size: 11px; color: #6B778C; margin: 0 0 16px 0;">Các trường bắt buộc được đánh dấu sao <span class="required">*</span></p>
          
          <div class="form-group">
            <label>Tên <span class="required">*</span></label>
            <input type="text" v-model="newGoal.title" @blur="isTitleTouched = true" :class="{'error-input': isTitleTouched && !newGoal.title}" />
            <div v-if="isTitleTouched && !newGoal.title" style="color: #DE350B; font-size: 11px; margin-top: 4px; display: flex; align-items: center; gap: 4px;">
               <i class="fa-solid fa-circle-exclamation"></i> Bạn phải đặt tên mục tiêu
            </div>
          </div>
          
          <div class="form-group">
            <label>Loại <span class="required">*</span></label>
            <div style="position: relative; display: flex; align-items: center; gap: 8px; background: #FAFBFC; padding: 8px 12px; border: 1px solid #DFE1E6; border-radius: 3px; cursor: default;">
               <i class="fa-solid fa-bullseye" style="color: #6B778C; font-size: 14px;"></i>
               <span style="color: #172B4D; font-size: 14px;">Objective</span>
            </div>
          </div>
          
          <div class="form-group" style="position: relative;">
            <label>Ngày mục tiêu</label>
            <el-date-picker
              v-model="newGoal.date"
              type="date"
              placeholder="Chọn ngày"
              format="MMM DD, YYYY"
              value-format="YYYY-MM-DD"
              style="width: 100%"
              class="jira-date-picker"
            />
          </div>
          
          <div class="form-group" style="position: relative;">
            <label>Chủ sở hữu <span class="required">*</span></label>
            <div class="owner-input-wrapper" @click="isOwnerDropdownOpen = !isOwnerDropdownOpen" style="position: relative; border: 2px solid #DFE1E6; border-radius: 3px; padding: 6px 12px; cursor: pointer; display: flex; align-items: center; gap: 8px; background: white;">
               <UserAvatar :user="{ avatarColor: newGoal.ownerAvatarColor, initials: newGoal.ownerAvatar, fullName: newGoal.ownerName, avatarUrl: newGoal.ownerAvatarUrl }" :size="24" :fontSize="11" />
               <span style="font-size: 14px; color: #172B4D;">{{ newGoal.ownerName }}</span>
            </div>
            
            <div v-if="isOwnerDropdownOpen" class="dropdown-menu" style="position: absolute; top: 100%; left: 0; margin-top: 4px; background: white; border: 1px solid #DFE1E6; border-radius: 3px; box-shadow: 0 4px 12px rgba(0,0,0,0.15); width: 100%; z-index: 100; max-height: 200px; overflow-y: auto;">
               <div v-for="user in siteUsers" :key="user.id" @click="selectOwner(user)" style="display: flex; align-items: center; gap: 8px; padding: 8px 12px; cursor: pointer; transition: background 0.1s;" onmouseover="this.style.background='#FAFBFC'" onmouseout="this.style.background='transparent'">
                  <UserAvatar :user="user" :size="24" :fontSize="11" />
                  <span style="font-size: 14px; color: #172B4D;">{{ user.name }}</span>
               </div>
            </div>
          </div>
        </div>
        <div class="modal-footer" style="padding: 16px 24px;">
          <button class="cancel-btn" @click="isCreateModalOpen = false">Hủy</button>
          <button class="primary-btn" @click="submitCreateGoal">Tạo</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import { useGoalStore } from '@/store/useGoalStore'
import { useStarredStore } from '@/store/useStarredStore'
import { useFollowerStore } from '@/store/useFollowerStore'
import { usePeopleStore } from '@/store/usePeopleStore'
import axiosClient from '@/api/axiosClient'
import UserAvatar from '@/components/common/UserAvatar.vue'
import DropdownFilter from '@/components/common/DropdownFilter.vue'
import { getStoredUser } from '@/utils/permissions'
import { getInitials, getAvatarColor } from '@/utils/avatarHelper'

const router = useRouter()
const goalStore = useGoalStore()
const starredStore = useStarredStore()
const followerStore = useFollowerStore()
const peopleStore = usePeopleStore()

const currentTab = ref('all')
const searchQuery = ref('')
const filters = ref({
  status: '',
  progress: '',
  favorite: '',
  following: '',
  owner: ''
})

const uniqueValues = (selector) => Array.from(new Set(
  (goalStore.goals || [])
    .map(selector)
    .filter(value => value && value !== 'N/A')
)).sort()

const statusOptions = computed(() => {
  const statuses = uniqueValues(g => g.status);
  // Map raw statuses to translated statuses and remove duplicates again
  return Array.from(new Set(statuses.map(translateStatus)));
})
const ownerOptions = computed(() => uniqueValues(g => g.owner))
const progressOptions = [
  { label: 'Chưa bắt đầu (0%)', value: '0' },
  { label: 'Đang tiến hành (>0%)', value: 'in_progress' },
  { label: 'Hoàn thành (100%)', value: '100' }
]
const booleanOptions = [
  { label: 'Có', value: 'true' },
  { label: 'Không', value: 'false' }
]

const clearFilters = () => {
  filters.value = {
    status: '',
    progress: '',
    favorite: '',
    following: '',
    owner: ''
  }
}
const hasActiveFilters = computed(() => Object.values(filters.value).some(val => val !== ''))

const isCreateModalOpen = ref(false)
const isTitleTouched = ref(false)
const isOwnerDropdownOpen = ref(false)

const siteUsers = computed(() => {
  return peopleStore.users.map(u => ({
    id: u.id,
    name: u.fullName || u.email,
    initials: u.initials || getInitials(u.fullName || u.email),
    avatarColor: u.avatarColor,
    avatarUrl: u.avatarUrl
  }))
})

const newGoal = ref({
  title: '',
  type: 'Objective',
  date: '',
  ownerId: '',
  ownerName: '',
  ownerAvatar: '',
  ownerAvatarColor: '',
  ownerAvatarUrl: '',
  status: 'Đang chờ cập nhật'
})

onMounted(async () => {
  await goalStore.fetchGoals()
  await starredStore.fetchStarredItems()
  await followerStore.fetchFollowedItems()
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
    // Tất cả mục tiêu (all) thì chỉ hiện những cái chưa archived
    list = list.filter(g => !g.isArchived)
  }

  // Tìm kiếm
  if (searchQuery.value) {
    const q = searchQuery.value.toLowerCase()
    list = list.filter(g => 
      g.title.toLowerCase().includes(q) || 
      (g.status && g.status.toLowerCase().includes(q)) ||
      (g.owner && g.owner.toLowerCase().includes(q))
    )
  }

  if (filters.value.status) {
    list = list.filter(g => translateStatus(g.status) === filters.value.status)
  }
  if (filters.value.owner) {
    list = list.filter(g => g.owner === filters.value.owner)
  }
  if (filters.value.progress) {
    list = list.filter(g => {
      if (filters.value.progress === '0') return g.progress === 0
      if (filters.value.progress === '100') return g.progress === 100
      if (filters.value.progress === 'in_progress') return g.progress > 0 && g.progress < 100
      return true
    })
  }
  if (filters.value.favorite) {
    const isFav = filters.value.favorite === 'true'
    list = list.filter(g => !!g.isFavorite === isFav)
  }
  if (filters.value.following) {
    const isFol = filters.value.following === 'true'
    list = list.filter(g => !!g.isFollowing === isFol)
  }

  return list
})

const getStatusClass = (status) => {
  if (!status) return 'status-pending'
  const map = {
    'đúng tiến độ': 'status-on-track',
    'on track': 'status-on-track',
    'có rủi ro': 'status-at-risk',
    'at risk': 'status-at-risk',
    'trễ tiến độ': 'status-off-track',
    'off track': 'status-off-track',
    'không đúng tiến độ': 'status-off-track',
    'đang chờ cập nhật': 'status-pending',
    'pending': 'status-pending',
    'đã hoàn tất': 'status-done',
    'completed': 'status-done',
    'đã lưu trữ': 'status-archived',
    'archived': 'status-archived'
  }
  return map[status.toLowerCase()] || 'status-pending'
}

const translateStatus = (status) => {
  if (!status) return 'Đang chờ cập nhật'
  const map = {
    'on track': 'Đúng tiến độ',
    'at risk': 'Có rủi ro',
    'off track': 'Trễ tiến độ',
    'pending': 'Đang chờ cập nhật',
    'completed': 'Đã hoàn tất',
    'archived': 'Đã lưu trữ'
  }
  return map[status.toLowerCase()] || status
}

const openCreateModal = () => {
  const stored = getStoredUser() || {}
  const currentUser = peopleStore.users.find(u => u.id === stored.id) || peopleStore.currentUser || stored
  const name = currentUser.fullName || currentUser.name || currentUser.publicName || currentUser.email || 'User'
  const email = currentUser.email || ''

  newGoal.value = { 
    title: '', 
    type: 'Objective', 
    date: '', 
    ownerId: currentUser.id,
    ownerName: name, 
    ownerAvatar: currentUser.initials || getInitials(name, email),
    ownerAvatarColor: currentUser.avatarColor,
    ownerAvatarUrl: currentUser.avatarUrl,
    status: 'Đang chờ cập nhật' 
  }
  isTitleTouched.value = false
  isOwnerDropdownOpen.value = false
  isCreateModalOpen.value = true
}

const selectOwner = (user) => {
  newGoal.value.ownerId = user.id
  newGoal.value.ownerName = user.name
  newGoal.value.ownerAvatar = user.initials
  newGoal.value.ownerAvatarColor = user.avatarColor
  newGoal.value.ownerAvatarUrl = user.avatarUrl
  isOwnerDropdownOpen.value = false
}

const submitCreateGoal = async () => {
  isTitleTouched.value = true
  if (!newGoal.value.title) return
  
  try {
    await goalStore.createGoal({ 
      title: newGoal.value.title, 
      status: newGoal.value.status,
      ownerId: newGoal.value.ownerId,
      owner: newGoal.value.ownerName,
      ownerColor: newGoal.value.ownerAvatarColor,
      type: newGoal.value.type,
      date: newGoal.value.date
    })
    isCreateModalOpen.value = false
  } catch (error) {
    console.error('Lỗi khi tạo mục tiêu:', error)
    // Here we can add a toast notification in the future
  }
}

const goToGoal = (id) => {
  router.push(`/home/goals/${id}`)
}

const toggleStar = async (goal) => {
  if (!goal?.id) return
  goalStore.currentGoal = goal
  await goalStore.toggleStar()
}

const toggleWatch = async (goal) => {
  if (!goal?.id) return
  await goalStore.toggleFollow(goal.id)
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

.empty-illustration {
  width: 280px;
  height: 200px;
  background-color: #E6FCFF;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.empty-illustration i {
  font-size: 64px;
  color: #0052CC;
}

/* List Controls */
.list-controls {
  display: flex;
  align-items: center;
  gap: 16px;
  margin-bottom: 24px;
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

.filter-actions {
  display: flex;
  gap: 8px;
}

.filter-btn {
  background-color: rgba(9, 30, 66, 0.04);
  border: none;
  border-radius: 3px;
  padding: 8px 12px;
  font-size: 14px;
  font-weight: 500;
  color: #42526E;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 6px;
  transition: background-color 0.2s;
}

.filter-btn:hover {
  background-color: rgba(9, 30, 66, 0.08);
}

.filter-btn i {
  font-size: 10px;
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
  cursor: pointer;
  vertical-align: middle;
}

.jira-table tbody tr:hover td {
  background-color: #FAFBFC;
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
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.form-group label {
  display: block;
  font-size: 12px;
  font-weight: 600;
  color: #5E6C84;
  margin-bottom: 8px;
}

.required {
  color: #DE350B;
}

.form-group input, .jira-select {
  width: 100%;
  padding: 8px 12px;
  border: 2px solid #DFE1E6;
  border-radius: 3px;
  font-size: 14px;
  box-sizing: border-box;
  outline: none;
}

.form-group input:focus, .jira-select:focus {
  border-color: #4C9AFF;
}

.error-input {
  border-color: #DE350B !important;
}

.error-input:focus {
  box-shadow: 0 0 0 2px rgba(222, 53, 11, 0.2);
}

.modal-footer {
  padding: 16px 24px;
  display: flex;
  justify-content: flex-end;
  gap: 8px;
  border-top: 1px solid #DFE1E6;
}

.cancel-btn {
  background: transparent;
  color: #5E6C84;
  border: none;
  padding: 8px 12px;
  border-radius: 3px;
  font-weight: 500;
  cursor: pointer;
}

.cancel-btn:hover {
  background: rgba(9, 30, 66, 0.08);
}

:deep(.jira-date-picker .el-input__inner) {
  border: none !important;
  padding: 0 !important;
  height: auto !important;
}
</style>

