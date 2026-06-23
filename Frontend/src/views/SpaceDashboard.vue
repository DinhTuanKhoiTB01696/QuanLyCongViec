<template>
  <NexusLayout>
    <div class="space-dashboard-page">
      <header class="dashboard-header">
        <span class="project-key-badge">
          {{ project?.key || t('Project') }}
        </span>
        <h1 class="project-title">
          {{ project?.name || t('Dashboard') }}
        </h1>
        <p class="project-subtitle">{{ t('Project overview and quick insights') }}</p>
      </header>
      
      <div v-if="loading" class="dashboard-loading-container">
        <i class="fa-solid fa-spinner fa-spin text-3xl mb-3 text-[var(--color-accent)]"></i>
        <p class="text-sm font-medium">{{ t('Fetching dashboard insights...') }}</p>
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
              <span class="stat-label">{{ t('Open Tasks') }}</span>
            </div>
          </div>
          
          <div class="stat-card completed-tasks-card">
            <div class="stat-icon">
              <i class="fa-solid fa-check-double"></i>
            </div>
            <div class="stat-info">
              <span class="stat-value">{{ completedTasks.length }}</span>
              <span class="stat-label">{{ t('Completed') }}</span>
            </div>
          </div>
          
          <div class="stat-card inprogress-tasks-card">
            <div class="stat-icon">
              <i class="fa-solid fa-clock-rotate-left"></i>
            </div>
            <div class="stat-info">
              <span class="stat-value">{{ inProgressTasks.length }}</span>
              <span class="stat-label">{{ t('In Progress') }}</span>
            </div>
          </div>
          
          <div class="stat-card blocked-tasks-card">
            <div class="stat-icon">
              <i class="fa-solid fa-triangle-exclamation"></i>
            </div>
            <div class="stat-info">
              <span class="stat-value">{{ blockedTasks.length }}</span>
              <span class="stat-label">{{ t('Blocked') }}</span>
            </div>
          </div>
        </div>
        
        <!-- Dashboard panels -->
        <div class="panels-grid">
          <!-- Recent Tasks Panel -->
          <div class="dashboard-panel">
            <div class="panel-header">
              <h3 class="panel-title">
                <i class="fa-solid fa-bolt text-yellow-500"></i> {{ t('Recent Tasks') }}
              </h3>
              <router-link 
                :to="{ name: 'SpaceSummary', params: { id: projectId } }" 
                class="panel-link"
              >
                {{ t('View all tasks') }} <i class="fa-solid fa-arrow-right"></i>
              </router-link>
            </div>
            
            <div v-if="recentTasks.length === 0" class="empty-state">
              <i class="fa-solid fa-inbox"></i>
              <h4>{{ t('No recent tasks') }}</h4>
              <p>{{ t('Get started by creating a new task in your board or backlog.') }}</p>
            </div>

            <div v-else class="task-list">
              <div 
                v-for="task in recentTasks" 
                :key="task.id" 
                class="task-row"
              >
                <div class="task-info-left">
                  <span class="task-seq-id">
                    {{ task.sequenceId || t('Task') }}
                  </span>
                  <button 
                    @click="navigateToTask(task.id)"
                    class="task-title-btn"
                  >
                    {{ task.title }}
                  </button>
                </div>
                
                <div class="task-meta-right">
                  <span class="priority-badge" :class="getPriorityClass(task.priority)">
                    {{ getPriorityLabel(task.priority) }}
                  </span>
                  <span class="task-status-tag" :class="getStatusClass(task.statusName)">
                    {{ normalizeStatusLabel(task.statusName) }}
                  </span>
                </div>
              </div>
            </div>
          </div>

          <!-- Team Workload Panel -->
          <div class="dashboard-panel">
            <div class="panel-header">
              <h3 class="panel-title">
                <i class="fa-solid fa-users text-blue-500"></i> {{ t('Team Workload') }}
              </h3>
            </div>
            
            <div v-if="teamWorkload.length === 0" class="empty-state">
              <i class="fa-solid fa-users"></i>
              <h4>{{ t('Workload distribution') }}</h4>
              <p>{{ t('Assign tasks to your team members to see their workload here.') }}</p>
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
                    {{ member.count }} {{ t(member.count === 1 ? 'task' : 'tasks') }}
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
import { ref, onMounted, computed, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import NexusLayout from '@/components/layout/NexusLayout.vue'

import { useI18nStore } from '@/store/useI18nStore'

const i18nStore = useI18nStore()
const t = (key) => i18nStore.t(key)

import { useWorkTaskStore } from '@/store/useWorkTaskStore'
import { useProjectStore } from '@/store/useProjectStore'

const route = useRoute()
const router = useRouter()
const projectId = computed(() => route.params.id)
const workTaskStore = useWorkTaskStore()
const projectStore = useProjectStore()

const loading = ref(true)
let loadRequestId = 0

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
          fullName: t('Unassigned'),
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
          const name = assignee.fullName || assignee.name || t('Unknown')
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
  switch (Number(priority)) {
    case 1: return t('Urgent')
    case 2: return t('High')
    case 3: return t('Medium')
    case 4: return t('Low')
    default: return t('None')
  }
}

const normalizeStatusLabel = (statusName = '') => {
  const normalized = `${statusName || ''}`.trim().toUpperCase()
  if (!normalized) return t('No status')
  return t(normalized)
}

const getPriorityClass = (priority) => {
  switch (Number(priority)) {
    case 1: return 'priority-urgent'
    case 2: return 'priority-high'
    case 3: return 'priority-medium'
    case 4: return 'priority-low'
    default: return 'priority-none'
  }
}

const getStatusClass = (statusName = '') => {
  const status = `${statusName}`.toLowerCase().trim()
  if (status.includes('done') || status.includes('complete')) return 'status-done'
  if (status.includes('progress')) return 'status-progress'
  if (status.includes('review')) return 'status-review'
  if (status.includes('block')) return 'status-blocked'
  if (status.includes('backlog')) return 'status-backlog'
  if (status.includes('todo') || status.includes('to do')) return 'status-todo'
  return 'status-default'
}

const navigateToTask = (taskId) => {
  router.push({
    name: 'SpaceSummary',
    params: { id: projectId.value },
    query: { task: taskId }
  })
}

const loadDashboard = async () => {
  const id = projectId.value
  const requestId = ++loadRequestId
  loading.value = true

  projectStore.clearProjectContext(id)
  projectStore.fetchProjectDetails(id, { background: true, reset: false }).catch(error => {
    console.error('Failed to load project summary', error)
  })

  try {
    await workTaskStore.fetchTasks(id)
  } catch (error) {
    console.error('Failed to load dashboard data', error)
  } finally {
    if (requestId === loadRequestId) {
      loading.value = false
    }
  }
}

onMounted(loadDashboard)

watch(projectId, () => {
  loadDashboard()
})
</script>

<style scoped>
.space-dashboard-page {
  width: 100%;
  max-width: 1120px;
  margin: 0 auto;
  padding: 30px 24px 48px;
  min-height: calc(100vh - 64px);
  color: var(--color-text-primary);
  display: flex;
  flex-direction: column;
  gap: 26px;
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
  min-height: 420px;
  display: grid;
  place-items: center;
  border: 1px solid color-mix(in srgb, var(--color-border) 80%, transparent);
  border-radius: 8px;
  background: color-mix(in srgb, var(--color-surface) 82%, #020617);
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
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 14px;
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
  min-height: 84px;
  background: color-mix(in srgb, var(--color-surface) 88%, #020617);
  border: 1px solid color-mix(in srgb, var(--color-border) 86%, transparent);
  border-radius: 8px;
  padding: 18px 18px 18px 20px;
  display: flex;
  align-items: center;
  gap: 14px;
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
  transform: translateY(-1px);
  border-color: var(--color-border-hover);
  box-shadow: 0 16px 36px color-mix(in srgb, #020617 22%, transparent);
}

.stat-card.open-tasks-card::before { background: #3b82f6; }
.stat-card.completed-tasks-card::before { background: #10b981; }
.stat-card.inprogress-tasks-card::before { background: #f59e0b; }
.stat-card.blocked-tasks-card::before { background: #ef4444; }

.stat-icon {
  width: 38px;
  height: 38px;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 16px;
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
  font-size: 26px;
  font-weight: 800;
  line-height: 1;
  color: var(--color-text-primary);
  letter-spacing: -0.01em;
}

.stat-label {
  font-size: 11px;
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
  gap: 14px;
  align-items: stretch;
}

@media (max-width: 1024px) {
  .panels-grid {
    grid-template-columns: 1fr;
  }
}

.dashboard-panel {
  background: color-mix(in srgb, var(--color-surface) 88%, #020617);
  border: 1px solid color-mix(in srgb, var(--color-border) 86%, transparent);
  border-radius: 8px;
  padding: 18px;
  min-height: 340px;
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
  margin-bottom: 16px;
  padding-bottom: 12px;
  border-bottom: 1px solid color-mix(in srgb, var(--color-border) 80%, transparent);
}

.panel-title {
  font-size: 15px;
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
  display: grid;
  grid-template-columns: minmax(0, 1fr) 94px 92px;
  align-items: center;
  gap: 10px;
  min-height: 44px;
  padding: 0 12px;
  border: 1px solid color-mix(in srgb, var(--color-border) 70%, transparent);
  background: color-mix(in srgb, var(--color-bg) 42%, transparent);
  border-radius: 7px;
  transition: all 0.2s;
}

[data-theme='dark'] .task-row {
  background: rgba(255, 255, 255, 0.02);
}

.task-row:hover {
  border-color: var(--color-border-hover);
  background: var(--color-surface-hover);
  transform: translateY(-1px);
}

.task-info-left {
  display: flex;
  align-items: center;
  gap: 10px;
  overflow: hidden;
  min-width: 0;
}

.task-seq-id {
  font-size: 11px;
  font-family: monospace;
  font-weight: 700;
  background: color-mix(in srgb, var(--color-surface) 88%, #020617);
  color: var(--color-text-muted);
  min-width: 62px;
  text-align: center;
  padding: 4px 6px;
  border-radius: 4px;
  border: 1px solid var(--color-border);
  flex-shrink: 0;
}

.task-title-btn {
  background: none;
  border: none;
  text-align: left;
  min-width: 0;
  font-size: 13px;
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
  display: contents;
}

.priority-badge,
.task-status-tag {
  justify-self: stretch;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-height: 24px;
  border: 1px solid transparent;
  border-radius: 999px;
  padding: 0 8px;
  font-size: 10px;
  font-weight: 800;
  letter-spacing: 0.02em;
  line-height: 1;
  text-transform: uppercase;
}

.priority-urgent { background: rgba(239, 68, 68, 0.16); border-color: rgba(239, 68, 68, 0.46); color: #f87171; }
.priority-high { background: rgba(249, 115, 22, 0.16); border-color: rgba(249, 115, 22, 0.46); color: #fb923c; }
.priority-medium { background: rgba(59, 130, 246, 0.16); border-color: rgba(59, 130, 246, 0.46); color: #60a5fa; }
.priority-low { background: rgba(148, 163, 184, 0.14); border-color: rgba(148, 163, 184, 0.34); color: #cbd5e1; }
.priority-none { background: rgba(148, 163, 184, 0.1); border-color: rgba(148, 163, 184, 0.24); color: #94a3b8; }

.status-blocked { background: rgba(239, 68, 68, 0.14); border-color: rgba(239, 68, 68, 0.36); color: #f87171; }
.status-progress { background: rgba(245, 158, 11, 0.14); border-color: rgba(245, 158, 11, 0.36); color: #fbbf24; }
.status-review { background: rgba(168, 85, 247, 0.14); border-color: rgba(168, 85, 247, 0.36); color: #c084fc; }
.status-done { background: rgba(16, 185, 129, 0.14); border-color: rgba(16, 185, 129, 0.36); color: #34d399; }
.status-backlog { background: rgba(148, 163, 184, 0.12); border-color: rgba(148, 163, 184, 0.28); color: #cbd5e1; }
.status-todo { background: rgba(59, 130, 246, 0.12); border-color: rgba(59, 130, 246, 0.30); color: #93c5fd; }
.status-default { background: rgba(148, 163, 184, 0.10); border-color: rgba(148, 163, 184, 0.24); color: #cbd5e1; }

/* Team Workload list styles */
.workload-list {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.workload-item {
  display: flex;
  flex-direction: column;
  gap: 9px;
  padding: 10px 0;
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
  width: 26px;
  height: 26px;
  border-radius: 7px;
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
  height: 7px;
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


