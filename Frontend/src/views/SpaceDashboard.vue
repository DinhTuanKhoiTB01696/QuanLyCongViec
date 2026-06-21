<template>
  <NexusLayout>
    <div class="space-dashboard-page">
      <header class="dashboard-header">
        <span class="project-key-badge">
          {{ project?.key || 'PROJECT' }}
        </span>
        <h1 class="project-title">
          {{ project?.name || 'Dashboard' }}
        </h1>
        <p class="project-subtitle">Project overview and quick insights</p>
      </header>
      
      <div v-if="loading" class="dashboard-loading-container">
        <i class="fa-solid fa-spinner fa-spin text-3xl mb-3 text-[var(--color-accent)]"></i>
        <p class="text-sm font-medium">Fetching dashboard insights...</p>
      </div>

      <div v-else class="dashboard-content">
        <!-- Stats Cards Grid -->
        <div class="stats-grid">
          <div class="stat-card open-tasks-card">
            <div class="stat-icon">
              <i class="fa-solid fa-list-check"></i>
            </div>
            <div class="stat-info">
              <span class="stat-value">{{ openTasks.length }}</span>
              <span class="stat-label">Open Tasks</span>
            </div>
          </div>
          
          <div class="stat-card completed-tasks-card">
            <div class="stat-icon">
              <i class="fa-solid fa-check-double"></i>
            </div>
            <div class="stat-info">
              <span class="stat-value">{{ completedTasks.length }}</span>
              <span class="stat-label">Completed</span>
            </div>
          </div>
          
          <div class="stat-card inprogress-tasks-card">
            <div class="stat-icon">
              <i class="fa-solid fa-clock-rotate-left"></i>
            </div>
            <div class="stat-info">
              <span class="stat-value">{{ inProgressTasks.length }}</span>
              <span class="stat-label">In Progress</span>
            </div>
          </div>
          
          <div class="stat-card blocked-tasks-card">
            <div class="stat-icon">
              <i class="fa-solid fa-triangle-exclamation"></i>
            </div>
            <div class="stat-info">
              <span class="stat-value">{{ blockedTasks.length }}</span>
              <span class="stat-label">Blocked</span>
            </div>
          </div>
        </div>
        
        <!-- Dashboard panels -->
        <div class="panels-grid">
          <!-- Recent Tasks Panel -->
          <div class="dashboard-panel">
            <div class="panel-header">
              <h3 class="panel-title">
                <i class="fa-solid fa-bolt text-yellow-500"></i> Recent Tasks
              </h3>
              <router-link 
                :to="{ name: 'SpaceSummary', params: { id: projectId } }" 
                class="panel-link"
              >
                View all tasks <i class="fa-solid fa-arrow-right"></i>
              </router-link>
            </div>
            
            <div v-if="recentTasks.length === 0" class="empty-state">
              <i class="fa-solid fa-inbox"></i>
              <h4>No recent tasks</h4>
              <p>Get started by creating a new task in your board or backlog.</p>
            </div>

            <div v-else class="task-list">
              <div 
                v-for="task in recentTasks" 
                :key="task.id" 
                class="task-row"
              >
                <div class="task-info-left">
                  <span class="task-seq-id">
                    {{ task.sequenceId || 'TASK' }}
                  </span>
                  <button 
                    @click="navigateToTask(task.id)"
                    class="task-title-btn"
                  >
                    {{ task.title }}
                  </button>
                </div>
                
                <div class="task-meta-right">
                  <span 
                    v-if="task.priority"
                    class="text-[10px] px-2 py-0.5 rounded-full border"
                    :class="getPriorityClass(task.priority)"
                  >
                    {{ getPriorityLabel(task.priority) }}
                  </span>
                  <span class="task-status-tag">
                    {{ task.statusName || 'No status' }}
                  </span>
                </div>
              </div>
            </div>
          </div>

          <!-- Team Workload Panel -->
          <div class="dashboard-panel">
            <div class="panel-header">
              <h3 class="panel-title">
                <i class="fa-solid fa-users text-blue-500"></i> Team Workload
              </h3>
            </div>
            
            <div v-if="teamWorkload.length === 0" class="empty-state">
              <i class="fa-solid fa-users"></i>
              <h4>Workload distribution</h4>
              <p>Assign tasks to team members to track their workload here.</p>
            </div>

            <div v-else class="workload-list">
              <div 
                v-for="member in teamWorkload" 
                :key="member.userId" 
                class="workload-item"
              >
                <div class="workload-item-header">
                  <div class="workload-user">
                    <div class="user-avatar">
                      <span v-if="member.avatar">{{ member.avatar }}</span>
                      <i v-else class="fa-solid fa-user"></i>
                    </div>
                    <span class="user-name">{{ member.fullName }}</span>
                  </div>
                  <span class="workload-count">
                    {{ member.count }} {{ member.count === 1 ? 'task' : 'tasks' }}
                  </span>
                </div>
                
                <div class="workload-progress-track">
                  <div 
                    class="workload-progress-bar" 
                    :style="{ width: `${member.percentage}%` }"
                  ></div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </NexusLayout>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import NexusLayout from '@/components/layout/NexusLayout.vue'
import { useWorkTaskStore } from '@/store/useWorkTaskStore'
import { useProjectStore } from '@/store/useProjectStore'

const route = useRoute()
const router = useRouter()
const projectId = computed(() => route.params.id)
const workTaskStore = useWorkTaskStore()
const projectStore = useProjectStore()

const loading = ref(true)

const project = computed(() => projectStore.currentProject)
const allTasks = computed(() => workTaskStore.tasks || [])

const doneStatuses = ['done', 'completed', 'finished', 'hoàn thành', 'success', 'hoàn tất']
const cancelStatuses = ['cancel', 'cancelled', 'hủy', 'hủy bỏ']

const openTasks = computed(() => {
  return allTasks.value.filter(task => {
    const status = (task.statusName || '').toLowerCase().trim()
    return !doneStatuses.includes(status) && !cancelStatuses.includes(status)
  })
})

const completedTasks = computed(() => {
  return allTasks.value.filter(task => {
    const status = (task.statusName || '').toLowerCase().trim()
    return doneStatuses.includes(status)
  })
})

const inProgressTasks = computed(() => {
  return allTasks.value.filter(task => {
    const status = (task.statusName || '').toLowerCase().trim()
    return status === 'in progress' || status === 'inprogress'
  })
})

const blockedTasks = computed(() => {
  return allTasks.value.filter(task => {
    const status = (task.statusName || '').toLowerCase().trim()
    return status.includes('block')
  })
})

const recentTasks = computed(() => {
  const tasksCopy = [...allTasks.value]
  tasksCopy.sort((a, b) => {
    const dateA = new Date(a.updatedAt || a.createdAt || 0)
    const dateB = new Date(b.updatedAt || b.createdAt || 0)
    return dateB - dateA
  })
  return tasksCopy.slice(0, 5)
})

const teamWorkload = computed(() => {
  const membersMap = {}
  allTasks.value.forEach(task => {
    const taskAssignees = task.assignees || []
    if (taskAssignees.length === 0) {
      if (!membersMap['unassigned']) {
        membersMap['unassigned'] = {
          userId: 'unassigned',
          fullName: 'Unassigned',
          avatar: null,
          count: 0
        }
      }
      membersMap['unassigned'].count++
    } else {
      taskAssignees.forEach(assignee => {
        const uid = assignee.userId || assignee.id
        if (!uid) return
        if (!membersMap[uid]) {
          const name = assignee.fullName || assignee.name || 'Unknown'
          membersMap[uid] = {
            userId: uid,
            fullName: name,
            avatar: name.substring(0, 1).toUpperCase(),
            count: 0
          }
        }
        membersMap[uid].count++
      })
    }
  })

  const list = Object.values(membersMap)
  if (list.length === 0) return []

  const maxCount = Math.max(...list.map(item => item.count)) || 1
  return list
    .map(item => ({
      ...item,
      percentage: Math.round((item.count / maxCount) * 100)
    }))
    .sort((a, b) => b.count - a.count)
})

const getPriorityLabel = (priority) => {
  switch (priority) {
    case 1: return 'Urgent'
    case 2: return 'High'
    case 3: return 'Medium'
    case 4: return 'Low'
    default: return 'None'
  }
}

const getPriorityClass = (priority) => {
  switch (priority) {
    case 1: return 'bg-red-500/10 text-red-500 border-red-500/20'
    case 2: return 'bg-orange-500/10 text-orange-500 border-orange-500/20'
    case 3: return 'bg-blue-500/10 text-blue-500 border-blue-500/20'
    case 4: return 'bg-gray-500/10 text-gray-500 border-gray-500/20'
    default: return 'bg-gray-500/10 text-gray-400 border-gray-500/10'
  }
}

const navigateToTask = (taskId) => {
  router.push({
    name: 'SpaceSummary',
    params: { id: projectId.value },
    query: { task: taskId }
  })
}

onMounted(async () => {
  loading.value = true
  try {
    await Promise.all([
      workTaskStore.fetchTasks(projectId.value),
      projectStore.fetchProjectDetails(projectId.value)
    ])
  } catch (error) {
    console.error('Failed to load dashboard data', error)
  } finally {
    loading.value = false
  }
})
</script>

<style scoped>
.space-dashboard-page {
  width: 100%;
  max-width: 1300px;
  margin: 0 auto;
  padding: 28px 24px;
  min-height: calc(100vh - 64px);
  color: var(--color-text-primary);
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.dashboard-header {
  margin-bottom: 8px;
}

.project-key-badge {
  font-size: 11px;
  font-weight: 700;
  letter-spacing: 0.15em;
  color: var(--color-accent);
  text-transform: uppercase;
  background: rgba(56, 189, 248, 0.08);
  padding: 4px 8px;
  border-radius: 4px;
  display: inline-block;
  margin-bottom: 8px;
}

.project-title {
  font-size: 32px;
  font-weight: 850;
  letter-spacing: -0.02em;
  color: var(--color-text-primary);
  margin: 0;
  line-height: 1.2;
}

.project-subtitle {
  font-size: 14px;
  color: var(--color-text-muted);
  margin-top: 6px;
}

.dashboard-loading-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 80px 0;
  color: var(--color-text-muted);
}

.dashboard-content {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

/* Stats Cards Grid */
.stats-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 20px;
  width: 100%;
}

@media (max-width: 1024px) {
  .stats-grid {
    grid-template-columns: repeat(2, 1fr);
  }
}

@media (max-width: 640px) {
  .stats-grid {
    grid-template-columns: 1fr;
  }
}

.stat-card {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 12px;
  padding: 24px;
  display: flex;
  align-items: center;
  gap: 20px;
  transition: all 0.25s cubic-bezier(0.4, 0, 0.2, 1);
  position: relative;
  overflow: hidden;
}

.stat-card::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  width: 4px;
  height: 100%;
  background: transparent;
  transition: background-color 0.2s;
}

.stat-card:hover {
  transform: translateY(-3px);
  border-color: var(--color-border-hover);
  box-shadow: var(--shadow-md);
}

.stat-card.open-tasks-card::before { background: #3b82f6; }
.stat-card.completed-tasks-card::before { background: #10b981; }
.stat-card.inprogress-tasks-card::before { background: #f59e0b; }
.stat-card.blocked-tasks-card::before { background: #ef4444; }

.stat-icon {
  width: 48px;
  height: 48px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 20px;
  flex-shrink: 0;
}

.open-tasks-card .stat-icon { background: rgba(59, 130, 246, 0.1); color: #3b82f6; }
.completed-tasks-card .stat-icon { background: rgba(16, 185, 129, 0.1); color: #10b981; }
.inprogress-tasks-card .stat-icon { background: rgba(245, 158, 11, 0.1); color: #f59e0b; }
.blocked-tasks-card .stat-icon { background: rgba(239, 68, 68, 0.1); color: #ef4444; }

.stat-info {
  display: flex;
  flex-direction: column;
}

.stat-value {
  font-size: 32px;
  font-weight: 800;
  line-height: 1;
  color: var(--color-text-primary);
  letter-spacing: -0.01em;
}

.stat-label {
  font-size: 13px;
  color: var(--color-text-secondary);
  margin-top: 6px;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.02em;
}

/* Panels Grid */
.panels-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 24px;
}

@media (max-width: 1024px) {
  .panels-grid {
    grid-template-columns: 1fr;
  }
}

.dashboard-panel {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 12px;
  padding: 24px;
  min-height: 360px;
  display: flex;
  flex-direction: column;
  transition: box-shadow 0.25s ease;
}

.dashboard-panel:hover {
  box-shadow: var(--shadow-sm);
}

.panel-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
  padding-bottom: 12px;
  border-bottom: 1px solid var(--color-border);
}

.panel-title {
  font-size: 18px;
  font-weight: 750;
  color: var(--color-text-primary);
  margin: 0;
  display: flex;
  align-items: center;
  gap: 8px;
}

.panel-link {
  font-size: 12px;
  font-weight: 600;
  color: var(--color-accent);
  text-decoration: none;
  transition: color 0.15s;
}

.panel-link:hover {
  color: var(--color-accent-hover);
  text-decoration: underline;
}

/* Empty state */
.empty-state {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  text-align: center;
  background: rgba(0, 0, 0, 0.05);
  border: 1px dashed var(--color-border);
  border-radius: 8px;
  padding: 32px 16px;
}

[data-theme='dark'] .empty-state {
  background: rgba(255, 255, 255, 0.02);
}

.empty-state i {
  font-size: 36px;
  color: var(--color-text-muted);
  margin-bottom: 12px;
}

.empty-state h4 {
  color: var(--color-text-primary);
  font-size: 14px;
  font-weight: 600;
  margin: 0;
}

.empty-state p {
  color: var(--color-text-secondary);
  font-size: 12px;
  margin-top: 6px;
  max-width: 250px;
  line-height: 1.4;
}

/* Task list styles */
.task-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
  flex: 1;
}

.task-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 12px 16px;
  border: 1px solid var(--color-border);
  background: rgba(0, 0, 0, 0.02);
  border-radius: 8px;
  transition: all 0.2s;
}

[data-theme='dark'] .task-row {
  background: rgba(255, 255, 255, 0.02);
}

.task-row:hover {
  border-color: var(--color-border-hover);
  background: var(--color-surface-hover);
  transform: translateX(2px);
}

.task-info-left {
  display: flex;
  align-items: center;
  gap: 12px;
  overflow: hidden;
  margin-right: 12px;
}

.task-seq-id {
  font-size: 11px;
  font-family: monospace;
  font-weight: 700;
  background: var(--color-surface);
  color: var(--color-text-muted);
  padding: 4px 8px;
  border-radius: 4px;
  border: 1px solid var(--color-border);
  flex-shrink: 0;
}

.task-title-btn {
  background: none;
  border: none;
  text-align: left;
  font-size: 14px;
  font-weight: 600;
  color: var(--color-text-primary);
  cursor: pointer;
  padding: 0;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  transition: color 0.15s;
}

.task-title-btn:hover {
  color: var(--color-accent);
  text-decoration: underline;
}

.task-meta-right {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-shrink: 0;
}

.task-status-tag {
  font-size: 11px;
  font-weight: 600;
  color: var(--color-text-secondary);
  background: rgba(0, 0, 0, 0.05);
  padding: 3px 8px;
  border-radius: 4px;
  text-transform: capitalize;
}

[data-theme='dark'] .task-status-tag {
  background: rgba(255, 255, 255, 0.08);
}

/* Team Workload list styles */
.workload-list {
  display: flex;
  flex-direction: column;
  gap: 18px;
}

.workload-item {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.workload-item-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.workload-user {
  display: flex;
  align-items: center;
  gap: 10px;
}

.user-avatar {
  width: 28px;
  height: 28px;
  border-radius: 50%;
  background: var(--color-accent);
  color: #ffffff;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 12px;
  font-weight: 700;
  border: 1.5px solid var(--color-border);
}

.user-name {
  font-size: 14px;
  font-weight: 600;
  color: var(--color-text-primary);
}

.workload-count {
  font-size: 12px;
  font-weight: 700;
  color: var(--color-text-secondary);
}

.workload-progress-track {
  width: 100%;
  background: var(--color-border);
  height: 8px;
  border-radius: 999px;
  overflow: hidden;
}

.workload-progress-bar {
  height: 100%;
  border-radius: 999px;
  background: linear-gradient(90deg, var(--color-accent) 0%, var(--color-accent-hover) 100%);
  transition: width 0.6s cubic-bezier(0.4, 0, 0.2, 1);
}
</style>


