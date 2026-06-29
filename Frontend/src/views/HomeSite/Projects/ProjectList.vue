<template>
  <div class="projects-wrapper">
    <header class="module-header">
      <div class="header-content">
        <h1>{{ pageTitle }}</h1>
        <div class="header-actions">
          <button class="primary-btn" @click="openCreateModal">Tạo dự án</button>
        </div>
      </div>
      
      <div class="tabs-nav">
        <button class="tab-btn" :class="{ active: currentTab === 'all' }" @click="currentTab = 'all'">Tất cả dự án</button>
        <button class="tab-btn" :class="{ active: currentTab === 'following' }" @click="currentTab = 'following'">Đang theo dõi</button>
        <button class="tab-btn" :class="{ active: currentTab === 'archived' }" @click="currentTab = 'archived'">Đã lưu trữ</button>
      </div>
    </header>

    <div class="module-content">
      <div class="list-controls-section">
        <div class="search-box-full">
          <i class="fa-solid fa-magnifying-glass search-icon"></i>
          <input type="text" v-model="searchQuery" placeholder="Tìm kiếm dự án" class="search-input" />
        </div>
        
                <div class="filter-chips" style="display: flex; flex-wrap: wrap; gap: 8px; margin-top: 12px; width: 100%;">
          <DropdownFilter label="Trạng thái" :options="statusOptions" v-model="filters.status" />
          <DropdownFilter label="Chủ sở hữu" :options="ownerOptions" v-model="filters.owner" />
          <DropdownFilter label="Theo dõi" :options="booleanOptions" v-model="filters.following" />
          <DropdownFilter label="Yêu thích" :options="booleanOptions" v-model="filters.starred" />
          <button v-if="hasActiveFilters" class="clear-filters-btn" @click="clearFilters" style="padding: 6px 12px; border-radius: 16px; border: 1px solid #DFE1E6; background: #fff; cursor: pointer; color: #5E6C84; font-size: 14px;">Xóa lọc</button>
        </div>
      </div>

      <div class="table-toolbar mt-24">
        <div class="results-count">Đang hiển thị {{ filteredProjects.length }} dự án</div>
        <div class="toolbar-actions">
          <div class="view-toggles">
            <button class="icon-btn active"><i class="fa-solid fa-list-ul"></i></button>
            <button class="icon-btn"><i class="fa-solid fa-bars-staggered"></i></button>
          </div>
          <button class="secondary-btn small-btn">Sắp xếp theo đang theo dõi <i class="fa-solid fa-chevron-down"></i></button>
          <button class="secondary-btn small-btn"><i class="fa-solid fa-table-columns"></i> Cột</button>
          <button class="icon-btn"><i class="fa-solid fa-ellipsis"></i></button>
        </div>
      </div>

      <div class="table-container mt-16" v-if="!isLoading">
        <table class="jira-table" v-if="filteredProjects.length > 0">
          <thead>
            <tr>
              <th class="col-name">Tên</th>
              <th class="col-status">Trạng thái</th>
              <th class="col-date">Ngày mục tiêu</th>
              <th class="col-owner">Chủ sở hữu</th>
              <th class="col-following">Đang theo dõi</th>
                <th class="col-star" style="width: 100px;">Có gắn sao</th>
              <th class="col-updated">Cập nhật lần cuối</th>
              <th class="actions-col"></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="proj in filteredProjects" :key="proj.id" @click="goToProject(proj.id)">
              <td>
                <div class="project-title-cell">
                  <span class="project-emoji">😎</span>
                  <span class="project-title">{{ proj.title }}</span>
                </div>
              </td>
              <td>
                <span class="status-badge" :class="getStatusClass(proj.status || 'ĐÚNG TIẾN ĐỘ')">
                  {{ proj.status || 'ĐÚNG TIẾN ĐỘ' }} <i class="fa-solid fa-chevron-down ms-1" v-if="proj.status !== 'ĐÃ HOÀN TẤT'"></i>
                </span>
              </td>
              <td>
                <div class="target-date-badge" :class="{ 'overdue': false }">
                  <i class="fa-regular fa-calendar"></i> {{ formatDate(proj.startDate || proj.createdAt) }}
                </div>
              </td>
              <td>
                <UserAvatar :user="{ id: proj.ownerId, fullName: proj.owner || proj.ownerName || proj.creatorName, avatarUrl: proj.ownerAvatarUrl, avatarColor: proj.ownerColor }" :size="24" :fontSize="10" class="owner-avatar-micro" />
              </td>
              <td @click.stop="toggleFollow(proj.id)">
                <span class="following-text" style="cursor: pointer;">{{ proj.isFollowing ? 'Đang theo dõi' : 'Theo dõi' }}</span>
              </td>
              <td @click.stop="toggleStar(proj.id)">
                <i :class="proj.isStarred ? 'fa-solid fa-star text-yellow-400' : 'fa-regular fa-star text-gray-400'" style="cursor: pointer;"></i>
              </td>
              <td>
                <span class="updated-text">{{ formatDate(proj.updatedAt || proj.createdAt) }}</span>
              </td>
              <td class="actions-col" @click.stop>
                <button class="icon-btn"><i class="fa-solid fa-ellipsis"></i></button>
              </td>
            </tr>
          </tbody>
        </table>
        
        <div class="empty-state-large" v-else>
          <div class="empty-icon-wrapper-large">
            <i class="fa-solid fa-magnifying-glass"></i>
          </div>
          <p class="empty-text-main">Chúng tôi không tìm được dự án nào phù hợp với nội dung tìm kiếm của bạn.</p>
          <p class="empty-text-sub">Hãy thử thay đổi tiêu chí tìm kiếm hoặc <a href="#">xóa tất cả bộ lọc</a>.</p>
        </div>
      </div>
      
      <div class="loading-state" v-else>
        <div class="loader-spinner"></div>
      </div>
    </div>

    <!-- Create Project Modal (Jira Style) -->
    <div class="modal-overlay" v-if="isCreateModalOpen" @click.self="isCreateModalOpen = false">
      <div class="jira-dialog">
        <div class="jira-dialog-header">
          <button class="icon-btn-header" @click="isCreateModalOpen = false"><i class="fa-solid fa-arrow-left"></i></button>
          <div class="dialog-title">
            <span class="dialog-icon"><i class="fa-solid fa-rocket"></i></span>
            <h2>Dự án</h2>
          </div>
        </div>
        
        <div class="jira-dialog-body">
          <p class="required-note">Các trường bắt buộc được đánh dấu bằng dấu sao <span class="required">*</span></p>
          
          <div class="form-group mt-16">
            <label>Tên <span class="required">*</span></label>
            <input type="text" v-model="newProject.title" class="jira-input" />
          </div>
          
          <div class="form-group mt-16">
            <label>Chọn một biểu tượng cảm xúc</label>
            <div class="emoji-picker-control">
              <button class="emoji-btn">{{ newProject.icon }}</button>
              <button class="refresh-emoji-btn" @click="cycleEmoji"><i class="fa-solid fa-arrows-rotate"></i></button>
            </div>
          </div>
          
          <div class="form-group mt-16">
            <label>Liên kết tới quy mô lớn SprintA hiện có</label>
            <input type="text" placeholder="Tìm kiếm quy mô lớn" class="jira-input" />
          </div>
          
          <div class="form-group mt-16 privacy-group">
            <div class="privacy-info">
              <label>Kiểm soát quyền riêng tư</label>
              <p>Chỉ những người đóng góp hoặc những người bạn chia sẻ mới có thể xem dự án riêng tư.</p>
            </div>
            <div class="toggle-switch" :class="{ 'active': newProject.isPrivate }" @click="newProject.isPrivate = !newProject.isPrivate">
              <div class="toggle-knob"></div>
            </div>
          </div>
        </div>
        
        <div class="jira-dialog-footer">
          <button class="cancel-btn" @click="isCreateModalOpen = false">Hủy</button>
          <button class="primary-btn" :disabled="!newProject.title" @click="submitCreateProject">Tạo</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useHomeProjectStore } from '@/store/useHomeProjectStore'
import { useStarredStore } from '@/store/useStarredStore'
import { useFollowerStore } from '@/store/useFollowerStore'
import UserAvatar from '@/components/common/UserAvatar.vue'
import DropdownFilter from '@/components/common/DropdownFilter.vue'

const router = useRouter()
const projectStore = useHomeProjectStore()
const starredStore = useStarredStore()
const followerStore = useFollowerStore()

const currentTab = ref('all')
const searchQuery = ref('')
const translateStatus = (status) => {
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

const filters = ref({
  status: '',
  owner: '',
  following: '',
  starred: ''
})

const uniqueValues = (selector) => Array.from(new Set(
  (projectStore.projects || [])
    .map(selector)
    .filter(value => value && value !== 'N/A')
)).sort()

const statusOptions = computed(() => {
  const statuses = uniqueValues(p => p.status);
  return Array.from(new Set(statuses.map(translateStatus).filter(s => s && s.toString().trim() !== '')));
});
const ownerOptions = computed(() => uniqueValues(p => p.owner))

const booleanOptions = [
  { label: 'Có', value: 'true' },
  { label: 'Không', value: 'false' }
]

const clearFilters = () => {
  filters.value = {
    status: '',
    owner: '',
    following: '',
    starred: ''
  }
}
const hasActiveFilters = computed(() => Object.values(filters.value).some(val => val !== ''))

const isCreateModalOpen = ref(false)

const newProject = ref({
  title: '',
  icon: '😎',
  isPrivate: false
})

const projectEmojis = ['😎', '🚀', '🎯', '💡', '🔥', '🌟', '💻', '📈', '✨', '🌈']

const cycleEmoji = () => {
  const currentIndex = projectEmojis.indexOf(newProject.value.icon)
  const nextIndex = (currentIndex + 1) % projectEmojis.length
  newProject.value.icon = projectEmojis[nextIndex]
}

const openCreateModal = () => {
  newProject.value = { title: '', icon: '😎', isPrivate: false }
  isCreateModalOpen.value = true
}

const submitCreateProject = async () => {
  if (!newProject.value.title) return
  
  try {
    const payload = {
      name: newProject.value.title, // Backend uses Name
      icon: newProject.value.icon, // Backend uses Icon
      startDate: new Date().toISOString(), // Required by CreateProjectDto
      networkType: newProject.value.isPrivate ? 'Private' : 'Public'
    }
    
    await projectStore.createProject(payload)
    isCreateModalOpen.value = false
  } catch (err) {
    console.error('Lỗi khi tạo dự án:', err)
  }
}

const route = useRoute()

const isDirectory = computed(() => currentTab.value === 'all')
const isFollowing = computed(() => currentTab.value === 'following')
const isArchived = computed(() => currentTab.value === 'archived')

const pageTitle = computed(() => {
  if (isFollowing.value) return 'Đang theo dõi'
  if (isArchived.value) return 'Đã lưu trữ'
  return 'Dự án'
})

onMounted(async () => {
  await projectStore.fetchProjects()
  await starredStore.fetchStarredItems()
  await followerStore.fetchFollowedItems()
  window.addEventListener('global-create-click', openCreateModal)
})

onUnmounted(() => {
  window.removeEventListener('global-create-click', openCreateModal)
})

const isLoading = computed(() => projectStore.isLoading)

const filteredProjects = computed(() => {
  let list = projectStore.projects || []

  // Filter by tab
  if (isArchived.value) {
    list = list.filter(p => p.isArchived)
  } else if (isFollowing.value) {
    list = list.filter(p => p.isFollowing)
  } else {
    list = list.filter(p => !p.isArchived)
  }

  // Filter by search
  if (searchQuery.value) {
    const q = searchQuery.value.toLowerCase()
    list = list.filter(p => 
      p.title.toLowerCase().includes(q) || 
      (p.owner && p.owner.toLowerCase().includes(q)) ||
      (p.key && p.key.toLowerCase().includes(q))
    )
  }

  
  if (filters.value.status) {
    list = list.filter(p => translateStatus(p.status) === filters.value.status)
  }
  if (filters.value.owner) {
    list = list.filter(p => p.owner === filters.value.owner)
  }
  if (filters.value.following) {
    const isFol = filters.value.following === 'true'
    list = list.filter(p => !!p.isFollowing === isFol)
  }
  if (filters.value.starred) {
    const isStar = filters.value.starred === 'true'
    list = list.filter(p => !!p.isStarred === isStar)
  }

    return list.map(p => ({
    ...p,
    key: p.key || (p.title ? p.title.substring(0, 3).toUpperCase() : 'PRJ'),
    status: p.status === true ? 'ĐANG CHỜ XỬ LÝ' : (p.status === false ? 'ĐÃ LƯU TRỮ' : (p.status || 'ĐANG CHỜ XỬ LÝ')),
    isStarred: starredStore.starredItems?.some(i => i.itemId === p.id) || false,
    isFollowing: followerStore.followedItems?.some(i => i.entityId === p.id) || false
  }))
})

const goToProject = (id) => {
  router.push(`/home/projects/${id}`)
}

const toggleStar = async (id) => {
  await starredStore.toggleStar('Project', id)
}

const toggleFollow = async (id) => {
  await followerStore.toggleFollow('Project', id)
}

const getInitials = (value) => {
  const text = String(value || '').trim()
  if (!text) return '?'
  return text
    .split(/\s+/)
    .slice(0, 2)
    .map(part => part[0]?.toUpperCase())
    .join('')
}

const formatDate = (value) => {
  if (!value) return '-'
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return '-'
  return date.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' })
}

const getStatusClass = (status) => {
  const value = String(status || '').toLowerCase()
  if (value.includes('risk') || value.includes('rui ro') || value.includes('rủi ro')) return 'status-default'
  if (value.includes('done') || value.includes('complete') || value.includes('hoan') || value.includes('hoàn')) return 'status-done'
  if (value.includes('on track') || value.includes('active') || value.includes('dung') || value.includes('đúng')) return 'status-on-track'
  return 'status-default'
}

</script>

<style scoped>
.projects-wrapper {
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
  background-color: rgba(9, 30, 66, 0.04);
  color: #42526E;
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

.sort-icon {
  margin-left: 4px;
  font-size: 12px;
  color: #5E6C84;
}

.col-name { width: 40%; }
.col-key { width: 15%; }
.col-type { width: 25%; }
.col-lead { width: 15%; }
.actions-col { width: 5%; text-align: right; }

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

.project-title-cell {
  display: flex;
  align-items: center;
  gap: 12px;
}

.project-avatar-small {
  width: 24px;
  height: 24px;
  background-color: #0052CC;
  color: white;
  border-radius: 3px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 10px;
  font-weight: bold;
}

.project-title {
  font-weight: 500;
  color: #0052CC;
}

.project-title:hover {
  text-decoration: underline;
}

.owner-cell {
  display: flex;
  align-items: center;
  gap: 8px;
}

.owner-avatar {
  width: 24px;
  height: 24px;
  background-color: #172B4D;
  color: white;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 10px;
  font-weight: bold;
}

.icon-btn {
  background: none;
  border: none;
  font-size: 14px;
  color: #6B778C;
  cursor: pointer;
  padding: 6px;
  border-radius: 3px;
  opacity: 0;
  transition: opacity 0.2s, background-color 0.2s;
}

.icon-btn.starred {
  color: #FFAB00;
  opacity: 1;
}

.jira-table tbody tr:hover .icon-btn {
  opacity: 1;
}

.icon-btn:hover {
  background-color: rgba(9, 30, 66, 0.08);
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

/* Modal */
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

.form-group input {
  width: 100%;
  padding: 8px 12px;
  border: 2px solid #DFE1E6;
  border-radius: 3px;
  font-size: 14px;
  box-sizing: border-box;
  outline: none;
}

.form-group input:focus {
  border-color: #4C9AFF;
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

/* --- Jira Dialog Styles --- */
.jira-dialog {
  background: #FFFFFF;
  border-radius: 3px;
  width: 400px;
  max-width: 90vw;
  box-shadow: 0 8px 16px -4px rgba(9,30,66,0.25), 0 0 1px rgba(9,30,66,0.31);
  display: flex;
  flex-direction: column;
}

.jira-dialog-header {
  display: flex;
  align-items: center;
  padding: 16px;
  gap: 12px;
}

.icon-btn-header {
  background: transparent;
  border: none;
  color: #42526E;
  cursor: pointer;
  padding: 4px 8px;
  border-radius: 3px;
  font-size: 16px;
}

.icon-btn-header:hover {
  background-color: rgba(9, 30, 66, 0.08);
}

.dialog-title {
  display: flex;
  align-items: center;
  gap: 8px;
}

.dialog-icon {
  color: #42526E;
  font-size: 16px;
}

.dialog-title h2 {
  margin: 0;
  font-size: 16px;
  color: #172B4D;
  font-weight: 600;
}

.jira-dialog-body {
  padding: 0 24px 16px;
}

.required-note {
  font-size: 12px;
  color: #5E6C84;
  margin: 0;
}

.required {
  color: #DE350B;
}

.jira-input {
  width: 100%;
  padding: 8px 10px;
  border: 2px solid #DFE1E6;
  border-radius: 3px;
  font-size: 14px;
  color: #172B4D;
  transition: border-color 0.2s, background-color 0.2s;
  box-sizing: border-box;
}

.jira-input:hover {
  background-color: #FAFBFC;
}

.jira-input:focus {
  border-color: #4C9AFF;
  background-color: #FFFFFF;
  outline: none;
}

.emoji-picker-control {
  display: flex;
  align-items: center;
  gap: 8px;
}

.emoji-btn {
  background-color: #FAFBFC;
  border: 1px solid #DFE1E6;
  border-radius: 3px;
  font-size: 20px;
  width: 40px;
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
}

.emoji-btn:hover {
  background-color: #EBECF0;
}

.refresh-emoji-btn {
  background: transparent;
  border: 1px solid transparent;
  color: #5E6C84;
  width: 32px;
  height: 32px;
  border-radius: 3px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
}

.refresh-emoji-btn:hover {
  background-color: rgba(9, 30, 66, 0.08);
}

.privacy-group {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 16px;
}

.privacy-info p {
  margin: 4px 0 0;
  font-size: 12px;
  color: #5E6C84;
  line-height: 1.4;
}

.toggle-switch {
  width: 32px;
  height: 16px;
  background-color: #DFE1E6;
  border-radius: 8px;
  position: relative;
  cursor: pointer;
  transition: background-color 0.2s;
  flex-shrink: 0;
}

.toggle-switch.active {
  background-color: #36B37E;
}

.toggle-knob {
  width: 12px;
  height: 12px;
  background-color: #FFFFFF;
  border-radius: 50%;
  position: absolute;
  top: 2px;
  left: 2px;
  transition: transform 0.2s;
  box-shadow: 0 1px 2px rgba(0,0,0,0.2);
}

.toggle-switch.active .toggle-knob {
  transform: translateX(16px);
}

.jira-dialog-footer {
  padding: 16px 24px;
  display: flex;
  justify-content: flex-end;
  gap: 8px;
  border-top: 1px solid #DFE1E6;
}

/* --- New Styles for Project List Layout --- */
.search-box-full {
  position: relative;
  width: 100%;
}

.search-box-full .search-icon {
  position: absolute;
  left: 12px;
  top: 50%;
  transform: translateY(-50%);
  color: #5E6C84;
}

.search-box-full .search-input {
  width: 100%;
  padding: 10px 12px 10px 40px;
  border: 2px solid #DFE1E6;
  border-radius: 3px;
  font-size: 14px;
  box-sizing: border-box;
}

.search-box-full .search-input:hover {
  background-color: #FAFBFC;
}

.search-box-full .search-input:focus {
  border-color: #4C9AFF;
  background-color: #FFFFFF;
}

.mt-16 { margin-top: 16px; }
.mt-24 { margin-top: 24px; }
.ms-1 { margin-left: 4px; }

.filters-row {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  align-items: center;
}

.filter-btn {
  background: transparent;
  border: none;
  color: #42526E;
  font-size: 14px;
  font-weight: 500;
  padding: 6px 12px;
  border-radius: 3px;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 6px;
  transition: background-color 0.2s;
}

.filter-btn:hover {
  background: rgba(9, 30, 66, 0.08);
}

.active-filter-chip {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  background-color: rgba(9, 30, 66, 0.08);
  padding: 6px 12px;
  border-radius: 16px;
  font-size: 14px;
  color: #172B4D;
}

.chip-close {
  cursor: pointer;
  color: #5E6C84;
  margin-left: 4px;
}

.chip-close:hover {
  color: #172B4D;
}

.table-toolbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.results-count {
  font-size: 14px;
  color: #5E6C84;
  font-weight: 500;
}

.toolbar-actions {
  display: flex;
  align-items: center;
  gap: 12px;
}

.view-toggles {
  display: flex;
  border: 1px solid #DFE1E6;
  border-radius: 3px;
  overflow: hidden;
}

.view-toggles .icon-btn {
  border-radius: 0;
  padding: 6px 10px;
  color: #5E6C84;
  opacity: 1;
}

.view-toggles .icon-btn.active {
  background-color: rgba(9, 30, 66, 0.08);
  color: #172B4D;
}

.small-btn {
  padding: 6px 12px;
  background: transparent;
  color: #42526E;
}

.small-btn:hover {
  background: rgba(9, 30, 66, 0.08);
}

.col-status { width: 15%; }
.col-date { width: 15%; }
.col-owner { width: 10%; }
.col-following { width: 10%; }
.col-updated { width: 15%; }

.project-emoji {
  font-size: 16px;
}

.status-badge {
  display: inline-flex;
  align-items: center;
  padding: 2px 6px;
  border-radius: 3px;
  font-size: 11px;
  font-weight: 700;
  text-transform: uppercase;
}

.status-on-track {
  background-color: #E3FCEF;
  color: #006644;
}

.status-done {
  background-color: #EAE6FF;
  color: #403294;
}

.status-default {
  background-color: #DFE1E6;
  color: #42526E;
}

.target-date-badge {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  padding: 2px 6px;
  border-radius: 3px;
  font-size: 12px;
  color: #42526E;
  background-color: transparent;
}

.target-date-badge.overdue {
  color: #DE350B;
  background-color: #FFEBE6;
}

.owner-avatar-micro {
  width: 24px;
  height: 24px;
  border-radius: 50%;
  background-color: #0052CC;
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 12px;
  font-weight: bold;
}

.following-text, .updated-text {
  color: #5E6C84;
  font-size: 14px;
}

.empty-state-large {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 80px 20px;
  text-align: center;
  border: 1px solid #DFE1E6;
  border-radius: 3px;
}

.empty-icon-wrapper-large {
  width: 120px;
  height: 120px;
  border-radius: 50%;
  background-color: #F4F5F7;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-bottom: 24px;
}

.empty-icon-wrapper-large i {
  font-size: 48px;
  color: #A5ADBA;
}

.empty-text-main {
  font-size: 16px;
  color: #172B4D;
  margin: 0 0 8px 0;
}

.empty-text-sub {
  font-size: 14px;
  color: #5E6C84;
  margin: 0;
}

.empty-text-sub a {
  color: #0052CC;
  text-decoration: none;
}

.empty-text-sub a:hover {
  text-decoration: underline;
}

.clear-filters-btn {
  background: white;
  border: 1px solid #DFE1E6;
  border-radius: 3px;
  padding: 6px 12px;
  font-size: 13px;
  font-weight: 500;
  color: #0052CC;
  cursor: pointer;
  transition: background 0.2s;
}
.clear-filters-btn:hover {
  background: #E6FCFF;
}
</style>

