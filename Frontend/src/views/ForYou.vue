<script setup>
import { ref, onMounted, computed, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import axiosClient from '@/api/axiosClient'
import NexusLayout from '@/components/layout/NexusLayout.vue'
import TaskDetailModal from '@/components/TaskDetailModal.vue'
import { useProjectStore } from '@/store/useProjectStore'
import { useWorkTaskStore } from '@/store/useWorkTaskStore'
import { useI18nStore } from '@/store/useI18nStore'

const route = useRoute()
const router = useRouter()
const projectStore = useProjectStore()
const workTaskStore = useWorkTaskStore()
const i18nStore = useI18nStore()
const t = (key) => i18nStore.t(key)

const currentProjectId = computed(() => route.params.id || null)

// Loading, empty & error states
const loadingSpaces = ref(false)
const loadingTasks = ref(false)
const errorSpaces = ref(null)
const errorTasks = ref(null)

// Data refs
const spaces = ref([])
const myTasks = ref([])
const projectMembers = ref([])
const currentProjectRole = ref('member')
const activeTab = ref('assigned') // recommended, assigned, starred, worked, viewed
const taskSearch = ref('')
const statusFilter = ref('all') // all, todo, inprogress, done
const sortOption = ref('updated') // updated, created, priority
const currentPage = ref(1)
const itemsPerPage = ref(10)

// Task details modal interaction
const selectedTask = ref(null)
const taskDetailHistory = ref([])

const currentUserId = computed(() => {
  const user = localStorage.getItem('user')
  return user ? JSON.parse(user).id : null
})

const emojiList = ['📦', '🚀', '⚡', '💡', '🔥', '🎯']

// 1. Fetch Spaces/Projects
const fetchSpaces = async () => {
  loadingSpaces.value = true
  errorSpaces.value = null
  try {
    const list = await projectStore.fetchAllProjects(true)
    spaces.value = list.map((p, index) => ({
      id: p.id,
      name: p.name,
      key: p.key || p.name.substring(0, 4).toUpperCase(),
      description: p.description || 'Software space',
      cover: p.cover || '#3b82f6',
      icon: p.icon || '📦',
      taskCount: p.activeMemberCount || p.ActiveMemberCount || 0,
      networkType: p.networkType || 'Public',
      createdAt: p.createdAt || null,
      originalRow: p
    }))
  } catch (error) {
    errorSpaces.value = 'Failed to load projects.'
    console.error('Fetch spaces error:', error)
  } finally {
    loadingSpaces.value = false
  }
}

// 2. Fetch User Personal Tasks
const fetchMyTasks = async () => {
  loadingTasks.value = true
  errorTasks.value = null
  try {
    const res = await axiosClient.get('/tasks/search')
    myTasks.value = res.data?.data || []
  } catch (error) {
    errorTasks.value = 'Failed to load personal tasks.'
    console.error('Failed to load personal tasks:', error)
  } finally {
    loadingTasks.value = false
  }
}

// 3. Task Starring (Client-side localized)
const toggleTaskStar = (task) => {
  workTaskStore.toggleTaskStar(task)
  ElMessage.success(workTaskStore.isTaskStarred(task.id) ? 'Task starred' : 'Task unstarred')
}

// 4. Space Starring
const toggleSpaceStar = async (space) => {
  try {
    const isCurrentlyFav = projectStore.favoriteProjects.some(p => p.id === space.id)
    await projectStore.updateFavorite(space.id, !isCurrentlyFav)
    ElMessage.success(isCurrentlyFav ? 'Space removed from starred' : 'Space starred!')
  } catch {
    ElMessage.error('Could not update space star')
  }
}

// Sorted Spaces
const sortedSpaces = computed(() => {
  return [...spaces.value].sort((a, b) => {
    const aStarred = projectStore.favoriteProjects.some(p => p.id === a.id)
    const bStarred = projectStore.favoriteProjects.some(p => p.id === b.id)
    if (aStarred !== bStarred) return aStarred ? -1 : 1
    return new Date(b.createdAt || 0) - new Date(a.createdAt || 0)
  })
})

// Filtered and Sorted Tasks
const filteredTasksList = computed(() => {
  let list = []
  
  if (activeTab.value === 'assigned') {
    list = myTasks.value.filter(task => task.assignedUserId === currentUserId.value || (task.assignees || []).some(a => a.userId === currentUserId.value || a.id === currentUserId.value))
  } else if (activeTab.value === 'worked') {
    list = myTasks.value.filter(task => task.reporterId === currentUserId.value || task.assignedUserId === currentUserId.value || (task.assignees || []).some(a => a.userId === currentUserId.value || a.id === currentUserId.value))
  } else if (activeTab.value === 'recommended') {
    list = myTasks.value.filter(task => {
      const isMine = task.assignedUserId === currentUserId.value || task.reporterId === currentUserId.value
      const isDone = (task.statusName || '').toUpperCase().includes('DONE')
      return isMine && !isDone
    })
  } else if (activeTab.value === 'starred') {
    const starredLocal = JSON.parse(localStorage.getItem('starred_tasks') || '[]')
    list = starredLocal.map(v => myTasks.value.find(t => t.id === v.id)).filter(Boolean)
  } else if (activeTab.value === 'viewed') {
    const viewed = JSON.parse(localStorage.getItem('recently_viewed_tasks') || '[]')
    list = viewed.map(v => myTasks.value.find(t => t.id === v.id)).filter(Boolean)
  }

  // Status filter
  if (statusFilter.value !== 'all') {
    const sf = statusFilter.value.toUpperCase().replace(/\s+/g, '')
    list = list.filter(task => {
      const ts = (task.statusName || 'BACKLOG').toUpperCase().replace(/\s+/g, '')
      if (sf === 'TODO') return ts.includes('TODO') || ts.includes('BACKLOG')
      if (sf === 'INPROGRESS') return ts.includes('PROGRESS') || ts.includes('REVIEW')
      if (sf === 'DONE') return ts.includes('DONE') || ts.includes('COMPLETE')
      return true
    })
  }

  // Search filter
  if (taskSearch.value.trim()) {
    const q = taskSearch.value.toLowerCase().trim()
    list = list.filter(task => 
      task.title?.toLowerCase().includes(q) || 
      task.sequenceId?.toLowerCase().includes(q) ||
      task.projectName?.toLowerCase().includes(q)
    )
  }

  // Sorting
  if (activeTab.value !== 'viewed') {
    list.sort((a, b) => {
      if (sortOption.value === 'created') {
        return new Date(b.createdAt || 0) - new Date(a.createdAt || 0)
      } else if (sortOption.value === 'priority') {
        return (a.priority || 3) - (b.priority || 3)
      }
      // Default: updated
      return new Date(b.updatedAt || b.createdAt || 0) - new Date(a.updatedAt || a.createdAt || 0)
    })
  }

  return list
})

// Pagination
const totalPages = computed(() => Math.ceil(filteredTasksList.value.length / itemsPerPage.value))
const paginatedTasks = computed(() => {
  const start = (currentPage.value - 1) * itemsPerPage.value
  return filteredTasksList.value.slice(start, start + itemsPerPage.value)
})

// Group Tasks
const groupedTasks = computed(() => {
  // Group by Project for Assigned and Starred
  if (activeTab.value === 'assigned' || activeTab.value === 'starred') {
    const projectGroups = {}
    paginatedTasks.value.forEach(task => {
      const pName = task.projectName || 'Other Projects'
      if (!projectGroups[pName]) projectGroups[pName] = []
      projectGroups[pName].push(task)
    })
    return Object.entries(projectGroups).map(([label, items]) => ({ label, items }))
  }

  // Group by Date for Worked, Viewed, Recommended
  const groups = {
    'Today': [],
    'Yesterday': [],
    'This Week': [],
    'In the last month': [],
    'Older': []
  }

  const today = new Date()
  today.setHours(0, 0, 0, 0)
  const yesterday = new Date(today)
  yesterday.setDate(yesterday.getDate() - 1)
  const lastWeek = new Date(today)
  lastWeek.setDate(lastWeek.getDate() - 7)
  const lastMonth = new Date(today)
  lastMonth.setMonth(lastMonth.getMonth() - 1)

  paginatedTasks.value.forEach(task => {
    const d = new Date(task.updatedAt || task.createdAt || Date.now())
    if (d >= today) groups['Today'].push(task)
    else if (d >= yesterday) groups['Yesterday'].push(task)
    else if (d >= lastWeek) groups['This Week'].push(task)
    else if (d >= lastMonth) groups['In the last month'].push(task)
    else groups['Older'].push(task)
  })

  // Remove empty groups
  return Object.entries(groups).filter(([_, items]) => items.length > 0).map(([label, items]) => ({ label, items }))
})

// Task Details Dialog
const openTaskDetail = async (task) => {
  logViewedTask(task)
  
  try {
    const mRes = await axiosClient.get(`/projects/${task.projectId}/members`)
    projectMembers.value = (mRes.data?.data || []).map(member => ({
      ...member,
      userId: member.userId || member.id,
      fullName: member.fullName || member.name || member.email,
      projectRole: member.projectRole || member.ProjectRole || member.myRole || member.MyRole || ''
    }))
  } catch (err) {
    projectMembers.value = []
  }

  taskDetailHistory.value = []
  selectedTask.value = workTaskStore.normalizeTaskRecord(task, task.projectId)
}

const openTaskDetailFromModal = (task, options = {}) => {
  const previousTask = options?.fromTask || selectedTask.value
  if (previousTask?.id && previousTask.id !== task?.id) {
    const cachedPrevious = myTasks.value.find(item => item.id === previousTask.id) || previousTask
    taskDetailHistory.value = [...taskDetailHistory.value, cachedPrevious]
  }
  selectedTask.value = workTaskStore.normalizeTaskRecord(task, task.projectId)
}

const goBackTaskDetail = () => {
  const history = [...taskDetailHistory.value]
  const previousTask = history.pop()
  if (!previousTask) return
  taskDetailHistory.value = history
  selectedTask.value = myTasks.value.find(item => item.id === previousTask.id) || previousTask
}

const closeTaskDetail = () => {
  taskDetailHistory.value = []
  selectedTask.value = null
}

const updateTask = async (task, field, value) => {
  if (!task?.id) return
  try {
    const updatePayload = { [field]: value }
    const usesPutUpdate = ['title', 'description', 'priority', 'assignedUserId'].includes(field)
    
    let payload = updatePayload
    if (usesPutUpdate) {
      payload = {
        title: task.title || '',
        description: task.description || '',
        priority: task.priority || 3,
        assignedUserId: task.assignedUserId || null,
        statusName: task.statusName || 'TO DO',
        sprintId: task.sprintId || null,
        plannedStartDate: task.plannedStartDate || null,
        plannedEndDate: task.plannedEndDate || null,
        dueDate: task.dueDate || null,
        ...updatePayload
      }
    }

    if (usesPutUpdate) {
      await axiosClient.put(`/projects/${task.projectId}/WorkTasks/${task.id}`, payload)
    } else {
      await axiosClient.patch(`/projects/${task.projectId}/WorkTasks/${task.id}`, payload)
    }

    const idx = myTasks.value.findIndex(item => item.id === task.id)
    if (idx !== -1) myTasks.value[idx][field] = value
    if (selectedTask.value?.id === task.id) selectedTask.value[field] = value
  } catch (error) {
    ElMessage.error('Could not update work item')
  }
}

// Log viewed task vào store (reactive) + localStorage
const logViewedTask = (task) => {
  workTaskStore.logViewedTask(task, spaces.value)
}

const getTaskIcon = (task) => {
  const ts = (task.statusName || '').toUpperCase()
  if (ts.includes('DONE')) return 'fa-solid fa-square-check text-green-500'
  return 'fa-solid fa-square-check text-blue-500'
}

const isTaskStarred = (taskId) => workTaskStore.isTaskStarred(taskId)

const timeAgo = (dateStr) => {
  if (!dateStr || dateStr.startsWith('0001-01-01')) return 'Vừa xong'
  const date = new Date(dateStr)
  if (isNaN(date.getTime()) || date.getFullYear() <= 1970) return 'Vừa xong'
  const seconds = Math.floor((new Date() - date) / 1000)
  if (seconds < 0) return 'Vừa xong'
  
  let interval = seconds / 31536000
  if (interval >= 1) return Math.floor(interval) + ' năm trước'
  interval = seconds / 2592000
  if (interval >= 1) return Math.floor(interval) + ' tháng trước'
  interval = seconds / 86400
  if (interval >= 1) return Math.floor(interval) + ' ngày trước'
  interval = seconds / 3600
  if (interval >= 1) return Math.floor(interval) + ' giờ trước'
  interval = seconds / 60
  if (interval >= 1) return Math.floor(interval) + ' phút trước'
  return 'Vừa xong'
}

onMounted(() => {
  fetchSpaces()
  fetchMyTasks()
})

watch(() => route.query.tab, (tab) => {
  activeTab.value = tab || 'worked'
}, { immediate: true })

watch(activeTab, () => {
  currentPage.value = 1
})

</script>

<template>
  <NexusLayout>
    <div class="jira-dashboard">
      
      <!-- Main Content Area (Left Column) -->
      <div class="main-content-column">
        
        <!-- Recommended Spaces Section -->
        <section class="mb-10 mt-2">
          <div class="section-header flex-between mb-4">
            <h2 class="section-title">{{ t('Recommended spaces') }}</h2>
            <a href="/spaces" class="view-all-link">{{ t('View all spaces') }}</a>
          </div>

          <div v-if="loadingSpaces" class="loading-state">
            <i class="fa-solid fa-spinner fa-spin"></i> Loading spaces...
          </div>
          
          <!-- Premium Empty State for Spaces -->
          <div v-else-if="sortedSpaces.length === 0" class="empty-spaces-card">
            <div class="esc-icon">
              <svg class="h-10 w-10 text-blue-500/80" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5">
                <path stroke-linecap="round" stroke-linejoin="round" d="M2.25 12.75V12A2.25 2.25 0 014.5 9.75h15A2.25 2.25 0 0121.75 12v.75m-8.69-6.44l-2.12-2.12a1.5 1.5 0 00-1.061-.44H4.5A2.25 2.25 0 002.25 6v12a2.25 2.25 0 002.25 2.25h15A2.25 2.25 0 0021.75 18V9a2.25 2.25 0 00-2.25-2.25h-5.379a1.5 1.5 0 01-1.06-.44z" />
              </svg>
            </div>
            <div class="esc-text">
              <h3 class="text-sm font-semibold text-gray-700 dark:text-neutral-300">No active spaces found</h3>
              <p class="text-xs text-gray-500 dark:text-neutral-500 mt-0.5">Projects and collaboration spaces you join will appear here.</p>
            </div>
          </div>
          
          <div v-else class="spaces-row">
            <div 
              v-for="space in sortedSpaces.slice(0, 4)" 
              :key="space.id" 
              class="space-card"
              @click="router.push(`/space/${space.id}`)"
            >
              <div class="sc-icon-wrapper" :style="{ backgroundColor: space.cover }">
                <span class="sc-emoji">{{ space.icon || '📦' }}</span>
              </div>
              <div class="sc-info">
                <h3 class="sc-name" :title="space.name">{{ space.name }}</h3>
                <p class="sc-desc">{{ space.description }} • {{ space.taskCount }} tasks</p>
              </div>
              <!-- Star button for Space -->
              <button 
                class="sc-star-btn"
                :class="{ starred: projectStore.favoriteProjects.some(p => p.id === space.id) }"
                @click.stop="toggleSpaceStar(space)"
                :title="projectStore.favoriteProjects.some(p => p.id === space.id) ? 'Bỏ đánh dấu' : 'Đánh dấu'"
              >
                <i :class="projectStore.favoriteProjects.some(p => p.id === space.id) ? 'fa-solid fa-star' : 'fa-regular fa-star'"></i>
              </button>
            </div>
          </div>
        </section>

        <!-- For You Section -->
        <section>
          <div class="foryou-header-row mb-6">
            <h2 class="section-title text-2xl font-bold">{{ t('For you') }}</h2>
            
            <div class="jira-tabs">
              <button 
                v-for="tab in [
                  { id: 'recommended', label: 'Recommended' },
                  { id: 'assigned', label: 'Assigned to me' },
                  { id: 'starred', label: 'Starred' },
                  { id: 'worked', label: 'Worked on' },
                  { id: 'viewed', label: 'Viewed' }
                ]"
                :key="tab.id"
                class="j-tab"
                :class="{ 'active': activeTab === tab.id }"
                @click="activeTab = tab.id"
              >
                <span>{{ t(tab.label) }}</span>
                <span 
                  v-if="tab.id === 'assigned' && myTasks.filter(t => t.assignedUserId === currentUserId).length > 0" 
                  class="tab-badge"
                >
                  {{ myTasks.filter(t => t.assignedUserId === currentUserId).length }}
                </span>
              </button>
            </div>
          </div>

          <!-- Toolbar: Search, Filter, Sort -->
          <div class="task-toolbar mb-6">
            <div class="search-input">
              <i class="fa-solid fa-magnifying-glass"></i>
              <input type="text" v-model="taskSearch" :placeholder="t('Search tasks...')" />
            </div>
            <select v-model="statusFilter" class="jira-select">
              <option value="all">{{ t('All Statuses') }}</option>
              <option value="todo">{{ t('TO DO') }}</option>
              <option value="inprogress">{{ t('IN PROGRESS') }}</option>
              <option value="done">{{ t('DONE') }}</option>
            </select>
            <select v-model="sortOption" class="jira-select">
              <option value="updated">{{ t('Recently Updated') }}</option>
              <option value="created">{{ t('Recently Created') }}</option>
              <option value="priority">{{ t('Priority') }}</option>
            </select>
          </div>

          <div v-if="loadingTasks" class="loading-state">
            <i class="fa-solid fa-spinner fa-spin"></i> Loading your work...
          </div>
          
          <!-- Premium Empty State for Tasks -->
          <div v-else-if="filteredTasksList.length === 0" class="empty-state">
            <div class="empty-state-illustration">
              <svg class="mx-auto h-16 w-16 text-blue-500/20 dark:text-blue-400/10 mb-4 animate-pulse" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.2">
                <path stroke-linecap="round" stroke-linejoin="round" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4" />
              </svg>
            </div>
            <h3 class="text-sm font-semibold text-gray-700 dark:text-neutral-300">All caught up!</h3>
            <p class="text-xs text-gray-500 dark:text-neutral-500 mt-1 max-w-xs mx-auto">
              You don't have any matching work items in this category.
            </p>
          </div>

          <!-- Task Groups -->
          <div v-else class="task-groups-container">
            <div v-for="group in groupedTasks" :key="group.label" class="task-group mb-6">
              <div class="group-header mb-3">
                <h4 class="group-label">{{ group.label }}</h4>
              </div>

              <div class="task-list">
                <div 
                  v-for="task in group.items" 
                  :key="task.id" 
                  class="jira-task-row"
                  @click="openTaskDetail(task)"
                >
                  <div class="jtr-left">
                    <i :class="getTaskIcon(task)" class="task-type-icon"></i>
                  </div>
                  <div class="jtr-center">
                    <div class="jtr-title" :class="{ 'line-through text-gray-400 dark:text-neutral-500': task.statusName === 'DONE' }">
                      {{ task.title }}
                    </div>
                    <div class="jtr-subtitle">
                      Task • {{ task.sequenceId || 'DTN-5' }} • {{ task.projectName || 'Project' }}
                    </div>
                  </div>
                  <div class="jtr-actions" @click.stop>
                    <button class="star-btn" :class="{ starred: isTaskStarred(task.id) }" @click.stop="toggleTaskStar(task)">
                      <i :class="isTaskStarred(task.id) ? 'fa-solid fa-star text-yellow-400' : 'fa-regular fa-star'"></i>
                    </button>
                  </div>
                  <div class="jtr-right">
                    <span class="time-text">{{ timeAgo(task.createdAt) }}</span>
                  </div>
                </div>
              </div>
            </div>

            <!-- Pagination -->
            <div class="pagination flex justify-center items-center gap-4 mt-6" v-if="totalPages > 1">
              <button class="jira-btn-subtle" :disabled="currentPage === 1" @click="currentPage--"><i class="fa-solid fa-chevron-left"></i></button>
              <span class="text-sm text-gray-500">Page {{ currentPage }} of {{ totalPages }}</span>
              <button class="jira-btn-subtle" :disabled="currentPage === totalPages" @click="currentPage++"><i class="fa-solid fa-chevron-right"></i></button>
            </div>
          </div>

        </section>
      </div>



      <!-- Task Detail Modal integration -->
      <TaskDetailModal 
        v-if="selectedTask"
        :selectedTask="selectedTask"
        :projectId="selectedTask.projectId"
        :projectMembers="projectMembers"
        :currentProjectRole="currentProjectRole"
        :canGoBack="taskDetailHistory.length > 0"
        @close="closeTaskDetail"
        @back="goBackTaskDetail"
        @open-task="openTaskDetailFromModal"
        @updateTask="updateTask"
        @refresh-tasks="fetchMyTasks"
      />

    </div>
  </NexusLayout>
</template>

<style scoped>
.jira-dashboard {
  max-width: 1400px;
  margin: 0 auto;
  padding: 24px 32px;
  font-family: 'Inter', sans-serif;
  color: var(--color-text-primary);
  display: flex;
  flex-direction: column;
  gap: 24px;
}

@media (min-width: 1200px) {
  .jira-dashboard {
    flex-direction: row;
    align-items: flex-start;
  }
}

.main-content-column {
  flex: 1;
  min-width: 0;
}

.section-title {
  font-size: 20px;
  font-weight: 600;
  margin: 0;
  color: var(--color-text-primary);
}

.view-all-link {
  font-size: 14px;
  color: var(--color-accent);
  text-decoration: none;
  font-weight: 500;
  transition: color 0.15s ease;
}
.view-all-link:hover {
  color: var(--color-accent);
  text-decoration: underline;
}

/* Spaces Row */
.spaces-row {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(240px, 1fr));
  gap: 16px;
}

.space-card {
  display: flex;
  align-items: center;
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 16px;
  padding: 16px 20px;
  cursor: pointer;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.02);
  transition: all 0.3s cubic-bezier(0.25, 0.8, 0.25, 1);
  position: relative;
  overflow: hidden;
  backdrop-filter: blur(10px);
}

.space-card:hover {
  background-color: var(--color-surface-hover);
  border-color: var(--color-accent);
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.06);
  transform: translateY(-4px);
}

.sc-icon-wrapper {
  width: 38px;
  height: 38px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-right: 12px;
  flex-shrink: 0;
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
}

.sc-emoji {
  font-size: 18px;
}

.sc-info {
  flex: 1;
  min-width: 0;
  padding-right: 24px;
}

.sc-name {
  font-size: 14px;
  font-weight: 600;
  margin: 0 0 2px 0;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  color: var(--color-text-primary);
}

.sc-desc {
  font-size: 12px;
  color: var(--color-text-muted);
  margin: 0;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.sc-star-btn {
  background: transparent;
  border: none;
  cursor: pointer;
  color: var(--color-text-muted);
  font-size: 14px;
  padding: 6px;
  border-radius: 6px;
  opacity: 0;
  transition: all 0.2s ease;
  flex-shrink: 0;
  position: absolute;
  right: 10px;
  top: 50%;
  transform: translateY(-50%);
  display: flex;
  align-items: center;
  justify-content: center;
}
.space-card:hover .sc-star-btn {
  opacity: 1;
}
.sc-star-btn.starred {
  opacity: 1;
  color: #f5cd47;
}
.sc-star-btn:hover {
  color: var(--color-text-primary);
  background: var(--color-surface-hover);
}

/* Premium Space Empty Card */
.empty-spaces-card {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 20px;
  background: var(--color-surface);
  border: 1px dashed var(--color-border);
  border-radius: 12px;
}
.esc-icon {
  background: var(--color-surface-hover);
  padding: 10px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
}
.esc-text h3 {
  margin: 0;
  font-size: 14px;
}
.esc-text p {
  margin: 0;
}

/* For You Section Header */
.foryou-header-row {
  display: flex;
  justify-content: space-between;
  align-items: baseline;
  flex-wrap: wrap;
  gap: 16px;
  border-bottom: 2px solid var(--color-border, #dfe1e6);
  padding-bottom: 0px;
  margin-top: 16px;
}

.jira-tabs {
  display: flex;
  gap: 4px;
}

.j-tab {
  background: transparent;
  border: none;
  padding: 10px 16px;
  font-size: 14px;
  font-weight: 500;
  color: var(--color-text-secondary, #42526e);
  cursor: pointer;
  transition: all 0.2s ease;
  display: flex;
  align-items: center;
  gap: 6px;
  position: relative;
  outline: none;
}

.j-tab::after {
  content: '';
  position: absolute;
  bottom: -2px;
  left: 0;
  right: 0;
  height: 2px;
  background: transparent;
  transition: background-color 0.2s ease;
}

.j-tab:hover {
  color: var(--color-text-primary, #172b4d);
}

.j-tab.active {
  color: var(--color-accent, #0c66e4);
  font-weight: 600;
}

.j-tab.active::after {
  background-color: var(--color-accent, #0c66e4);
}

.tab-badge {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 18px;
  height: 18px;
  padding: 0 5px;
  font-size: 11px;
  font-weight: 600;
  background-color: var(--color-accent, #0c66e4);
  color: white;
  border-radius: 9px;
  line-height: 1;
}

/* Toolbar */
.task-toolbar {
  display: flex;
  gap: 16px;
  flex-wrap: wrap;
  align-items: center;
}

.task-toolbar .jira-select:first-of-type {
  margin-left: auto; /* Pushes filters to the right */
}

.search-input {
  position: relative;
  display: flex;
  align-items: center;
  flex: 1;
  min-width: 260px;
  max-width: 320px;
}
.search-input i {
  position: absolute;
  left: 12px;
  color: var(--color-text-muted, #6b778c);
  font-size: 13px;
  pointer-events: none;
}
.search-input input {
  width: 100%;
  box-sizing: border-box !important;
  border: 1px solid var(--color-border) !important;
  border-radius: 20px !important;
  padding: 8px 16px 8px 36px !important;
  font-size: 14px !important;
  height: 40px !important;
  background-color: var(--color-surface) !important;
  color: var(--color-text-primary) !important;
  transition: all 0.3s ease;
  outline: none;
}
.search-input input:focus {
  border-color: var(--color-accent) !important;
  box-shadow: 0 0 0 2px color-mix(in srgb, var(--color-accent) 15%, transparent) !important;
  background-color: var(--color-surface) !important;
}

.jira-select {
  box-sizing: border-box !important;
  border: 1px solid var(--color-border) !important;
  border-radius: 20px !important;
  padding: 0 16px !important;
  font-size: 14px !important;
  height: 40px !important;
  background-color: var(--color-surface) !important;
  color: var(--color-text-primary) !important;
  outline: none;
  cursor: pointer;
  transition: all 0.3s ease;
  min-width: 140px;
}
.jira-select:focus {
  border-color: var(--color-accent) !important;
  box-shadow: 0 0 0 2px color-mix(in srgb, var(--color-accent) 15%, transparent) !important;
}

/* Loading & Empty States */
.loading-state {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 40px 0;
  font-size: 14px;
  color: var(--color-text-muted, #6b778c);
}

.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 48px 16px;
  text-align: center;
  border: 1px dashed var(--color-border, #dfe1e6);
  border-radius: 16px;
  background: rgba(9, 30, 66, 0.02);
}

.empty-state-illustration svg {
  animation: float 4s ease-in-out infinite;
}

/* Task Groups */
.group-label {
  font-size: 11px;
  text-transform: uppercase;
  color: var(--color-text-muted, #6b778c);
  font-weight: 600;
  margin: 0;
  letter-spacing: 0.5px;
}

/* Task List */
.task-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.jira-task-row {
  display: flex;
  align-items: center;
  padding: 16px 20px;
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 12px;
  cursor: pointer;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.02);
  transition: all 0.3s cubic-bezier(0.25, 0.8, 0.25, 1);
  overflow: hidden;
  backdrop-filter: blur(10px);
}

.jira-task-row:hover {
  background-color: var(--color-surface-hover);
  border-color: var(--color-accent);
  transform: translateX(4px);
  box-shadow: 0 6px 16px rgba(0, 0, 0, 0.05);
}

.jtr-left {
  margin-right: 14px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.task-type-icon {
  font-size: 16px;
  border-radius: 3px;
}

.jtr-center {
  flex: 1;
  min-width: 0;
}

.jtr-title {
  font-size: 14px;
  font-weight: 500;
  color: var(--color-text-primary, #172b4d);
  margin-bottom: 2px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.jtr-subtitle {
  font-size: 12px;
  color: var(--color-text-muted, #6b778c);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.jtr-actions {
  opacity: 0;
  transition: opacity 0.2s ease;
  margin-right: 16px;
}
.jira-task-row:hover .jtr-actions {
  opacity: 1;
}

.star-btn {
  background: transparent;
  border: none;
  cursor: pointer;
  color: var(--color-text-muted, #6b778c);
  font-size: 14px;
  padding: 4px;
  border-radius: 4px;
  transition: color 0.15s ease;
}
.star-btn:hover {
  color: var(--color-text-primary, #172b4d);
  background: rgba(9, 30, 66, 0.04);
}
.star-btn.starred {
  opacity: 1 !important;
  color: #f5cd47;
}

.jtr-right {
  min-width: 100px;
  text-align: right;
  flex-shrink: 0;
}

.time-text {
  font-size: 12px;
  color: var(--color-text-muted, #6b778c);
}

.jira-btn-subtle {
  background: transparent;
  border: none;
  cursor: pointer;
  padding: 8px 16px;
  border-radius: 8px;
  color: var(--color-text-secondary, #42526e);
  transition: all 0.2s ease;
  display: inline-flex;
  align-items: center;
  justify-content: center;
}
.jira-btn-subtle:hover:not(:disabled) {
  background: rgba(9, 30, 66, 0.08);
  color: var(--color-text-primary);
}
.jira-btn-subtle:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

.flex-between { display: flex; justify-content: space-between; align-items: center; }
.flex-center { display: flex; align-items: center; }

/* Right Sidebar styling */
.jira-right-sidebar {
  width: 320px;
  flex-shrink: 0;
  display: none;
}

@media (min-width: 1200px) {
  .jira-right-sidebar {
    display: block;
    padding-left: 24px;
    border-left: 1px solid var(--color-border, #dfe1e6);
  }
}

.sidebar-widget {
  background: var(--color-surface, #ffffff);
  border: 1px solid var(--color-border, #dfe1e6);
  border-radius: 16px;
  overflow: hidden;
  box-shadow: var(--shadow-sm);
  display: flex;
  flex-direction: column;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

.sidebar-widget:hover {
  transform: translateY(-4px);
  box-shadow: var(--shadow-md);
}

.widget-banner {
  background: linear-gradient(135deg, #0747a6 0%, #0052cc 100%);
  padding: 24px 20px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.widget-body {
  padding: 20px;
}

.widget-title {
  font-size: 16px;
  font-weight: 700;
  margin: 0 0 2px 0;
  color: var(--color-text-primary, #172b4d);
}

.widget-subtitle {
  font-size: 13px;
  font-weight: 600;
  color: var(--color-accent, #0c66e4);
  margin: 0 0 12px 0;
}

.widget-desc {
  font-size: 12.5px;
  line-height: 1.6;
  color: var(--color-text-secondary, #42526e);
  margin: 0 0 20px 0;
}

.widget-actions {
  display: flex;
  gap: 16px;
  align-items: center;
}

.btn-primary-sm {
  background: var(--color-accent, #0c66e4);
  color: white;
  border: none;
  padding: 8px 16px;
  font-size: 12.5px;
  font-weight: 600;
  border-radius: 8px;
  cursor: pointer;
  transition: background 0.15s;
}
.btn-primary-sm:hover {
  background: var(--color-accent-hover, #0052cc);
}

.btn-link-sm {
  background: transparent;
  border: none;
  color: var(--color-accent, #0c66e4);
  font-size: 12.5px;
  font-weight: 600;
  cursor: pointer;
  transition: color 0.15s ease;
}
.btn-link-sm:hover {
  color: var(--color-accent-hover, #0052cc);
  text-decoration: underline;
}

.widget-illustration {
  background: var(--color-surface-hover, #f4f5f7);
  padding: 24px 16px;
  display: flex;
  justify-content: center;
  border-top: 1px solid var(--color-border, #dfe1e6);
}

.illustration-box {
  position: relative;
  width: 180px;
  height: 90px;
}

.ill-circle {
  position: absolute;
  border-radius: 50%;
  opacity: 0.12;
}

.ill-c1 {
  width: 60px;
  height: 60px;
  background: var(--color-accent, #0c66e4);
  left: 20px;
  top: 10px;
}

.ill-c2 {
  width: 40px;
  height: 40px;
  background: #00b8d9;
  right: 30px;
  bottom: 10px;
}

.ill-card {
  position: absolute;
  background: var(--color-surface, #ffffff);
  border: 1px solid var(--color-border, #dfe1e6);
  border-radius: 8px;
  padding: 8px 10px;
  box-shadow: 0 4px 10px rgba(9, 30, 66, 0.08);
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.ill-card1 {
  width: 90px;
  left: 10px;
  top: 25px;
  transform: rotate(-4deg);
  animation: float 4s ease-in-out infinite;
}

.ill-card2 {
  width: 80px;
  right: 15px;
  top: 15px;
  transform: rotate(6deg);
  animation: float-reverse 4s ease-in-out infinite;
}

.ill-line {
  height: 4px;
  background: var(--color-border, #dfe1e6);
  border-radius: 2px;
}

.ill-line.w-16 { width: 100%; }
.ill-line.w-8 { width: 50%; background: var(--color-accent, #0c66e4); }
.ill-line.w-12 { width: 75%; }
.ill-line.w-10 { width: 60%; background: #00b8d9; }

@keyframes float {
  0% { transform: translateY(0px) rotate(-4deg); }
  50% { transform: translateY(-6px) rotate(-3deg); }
  100% { transform: translateY(0px) rotate(-4deg); }
}

@keyframes float-reverse {
  0% { transform: translateY(0px) rotate(6deg); }
  50% { transform: translateY(6px) rotate(5deg); }
  100% { transform: translateY(0px) rotate(6deg); }
}
</style>

