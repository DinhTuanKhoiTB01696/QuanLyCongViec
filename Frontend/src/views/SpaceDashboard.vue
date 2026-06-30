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
  max-width: 1180px;
  margin: 0 auto;
  padding: 34px clamp(20px, 4vw, 48px) 54px;
  min-height: calc(100vh - 64px);
  color: var(--color-text-primary);
  display: flex;
  flex-direction: column;
  gap: 22px;
  background:
    radial-gradient(circle at 16% 0%, rgba(14, 165, 233, 0.10), transparent 30%),
    linear-gradient(180deg, #f8fbff, #f1f6fb 54%, #f8fafc);
}

.dashboard-header {
  margin-bottom: 4px;
  padding: 22px 24px;
  border: 1px solid rgba(148, 163, 184, 0.22);
  border-radius: 16px;
  background: rgba(255, 255, 255, 0.9);
  box-shadow: 0 14px 34px rgba(15, 23, 42, 0.065);
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
  font-size: clamp(26px, 2.4vw, 34px);
  font-weight: 900;
  letter-spacing: 0;
  color: var(--color-text-primary);
  margin: 0;
  line-height: 1.15;
  overflow-wrap: anywhere;
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
  gap: 18px;
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
  min-height: 78px;
  background: rgba(255, 255, 255, 0.86);
  border: 1px solid rgba(148, 163, 184, 0.22);
  border-radius: 14px;
  padding: 16px;
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
  transform: none;
  border-color: rgba(14, 165, 233, 0.32);
  box-shadow: 0 18px 42px rgba(15, 23, 42, 0.10);
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
  min-width: 0;
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
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

/* Panels Grid */
.panels-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 18px;
  align-items: stretch;
}

@media (max-width: 1024px) {
  .panels-grid {
    grid-template-columns: 1fr;
  }
}

.dashboard-panel {
  background: rgba(255, 255, 255, 0.86);
  border: 1px solid rgba(148, 163, 184, 0.22);
  border-radius: 14px;
  padding: 18px;
  min-height: 320px;
  display: flex;
  flex-direction: column;
  transition: box-shadow 0.25s ease;
  position: relative;
  overflow: hidden;
}

.dashboard-panel:hover {
  box-shadow: 0 24px 58px rgba(15, 23, 42, 0.10);
}

[data-theme='dark'] .space-dashboard-page {
  background:
    radial-gradient(circle at 14% 0%, rgba(14, 165, 233, 0.11), transparent 30%),
    linear-gradient(180deg, #07111f, #0f172a 52%, #101827);
}

[data-theme='dark'] .dashboard-header,
[data-theme='dark'] .stat-card,
[data-theme='dark'] .dashboard-panel {
  border-color: rgba(148, 163, 184, 0.18);
  background: rgba(15, 23, 42, 0.78);
  box-shadow: 0 18px 44px rgba(0, 0, 0, 0.24);
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
  min-width: 0;
}

.panel-title span,
.panel-title {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
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
  grid-template-columns: minmax(0, 1fr) minmax(82px, 96px) minmax(88px, 104px);
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
  transform: none;
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
  min-width: 0;
  max-width: 100%;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
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
  min-width: 0;
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
  overflow: hidden;
  white-space: nowrap;
  text-overflow: ellipsis;
}

.workload-count {
  font-size: 12px;
  font-weight: 700;
  color: var(--color-text-secondary);
  flex: 0 0 auto;
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

/* Compact density */
.space-dashboard-page {
  max-width: 1120px !important;
  padding: 18px var(--sa-page-x, 24px) 30px !important;
  min-height: calc(100vh - var(--sa-topbar-height, 52px)) !important;
  gap: 16px !important;
}

.dashboard-header {
  padding: 18px !important;
  border-radius: 10px !important;
}

.dashboard-header h1 {
  font-size: clamp(24px, 2.2vw, 32px) !important;
  line-height: 1.12 !important;
}

.dashboard-header p {
  font-size: 12.5px !important;
}

.dashboard-content {
  gap: 16px !important;
}

.stats-grid,
.panels-grid {
  gap: 14px !important;
}

.stat-card,
.dashboard-panel {
  border-radius: 10px !important;
  padding: 14px !important;
  box-shadow: 0 10px 24px rgba(15, 23, 42, 0.06) !important;
}

.stat-card {
  min-height: 82px !important;
}

.stat-icon {
  width: 36px !important;
  height: 36px !important;
  border-radius: 8px !important;
  font-size: 15px !important;
}

.stat-value {
  font-size: 24px !important;
}

.panel-title {
  font-size: 14px !important;
  padding-bottom: 10px !important;
  margin-bottom: 12px !important;
}

.recent-task-row,
.workload-item {
  padding: 8px 0 !important;
}

@keyframes overview-panel-in {
  from {
    opacity: 0;
    transform: translateY(14px) scale(0.99);
  }
  to {
    opacity: 1;
    transform: translateY(0) scale(1);
  }
}

@keyframes overview-glow-drift {
  0% { transform: translate3d(-1%, -1%, 0) scale(1); opacity: 0.55; }
  50% { transform: translate3d(1.5%, 1%, 0) scale(1.02); opacity: 0.85; }
  100% { transform: translate3d(0.5%, -0.5%, 0) scale(1.01); opacity: 0.65; }
}

@keyframes overview-border-flow {
  from { background-position: 0% 50%; }
  to { background-position: 200% 50%; }
}

@keyframes overview-progress-grow {
  from { transform: scaleX(0); }
  to { transform: scaleX(1); }
}

.dashboard-header {
  position: relative !important;
  overflow: hidden !important;
  animation: overview-panel-in 520ms cubic-bezier(0.2, 0.8, 0.2, 1) both;
  background:
    radial-gradient(circle at 16% 22%, rgba(56, 189, 248, 0.18), transparent 24%),
    linear-gradient(135deg, rgba(255,255,255,0.96), rgba(239,248,255,0.78)),
    var(--color-surface) !important;
  border: 1px solid rgba(56, 189, 248, 0.18) !important;
  box-shadow: 0 24px 70px rgba(14, 165, 233, 0.12) !important;
}

.dashboard-header::before {
  content: "";
  position: absolute;
  inset: 0;
  background:
    radial-gradient(circle at 76% 22%, rgba(45, 212, 191, 0.16), transparent 22%),
    radial-gradient(circle at 8% 88%, rgba(250, 204, 21, 0.10), transparent 20%);
  pointer-events: none;
  animation: overview-glow-drift 8s ease-in-out infinite alternate;
}

.dashboard-header > * {
  position: relative;
  z-index: 1;
}

.stat-card,
.dashboard-panel {
  animation: overview-panel-in 520ms cubic-bezier(0.2, 0.8, 0.2, 1) both;
  transition:
    transform 220ms cubic-bezier(0.2, 0.8, 0.2, 1),
    background 220ms ease,
    border-color 220ms ease,
    box-shadow 220ms ease,
    color 220ms ease !important;
  background:
    linear-gradient(135deg, rgba(255,255,255,0.96), rgba(248,250,252,0.84)),
    var(--color-surface) !important;
  border-color: rgba(148, 163, 184, 0.20) !important;
}

.stat-card::after,
.dashboard-panel::after {
  content: "";
  position: absolute;
  inset: 0;
  border-radius: inherit;
  padding: 1px;
  background: linear-gradient(90deg, rgba(56, 189, 248, 0.70), rgba(45, 212, 191, 0.45), rgba(250, 204, 21, 0.52), rgba(56, 189, 248, 0.70));
  background-size: 200% 100%;
  -webkit-mask: linear-gradient(#000 0 0) content-box, linear-gradient(#000 0 0);
  -webkit-mask-composite: xor;
  mask-composite: exclude;
  opacity: 0;
  pointer-events: none;
  animation: overview-border-flow 4.6s linear infinite;
  transition: opacity 180ms ease;
}

.stat-card:hover,
.dashboard-panel:hover {
  transform: translateY(-3px);
  border-color: rgba(56, 189, 248, 0.30) !important;
  box-shadow: 0 28px 72px rgba(15, 23, 42, 0.14) !important;
}

.stat-card:hover::after,
.dashboard-panel:hover::after {
  opacity: 0.95;
}

.stats-grid .stat-card:nth-child(1) { animation-delay: 60ms; }
.stats-grid .stat-card:nth-child(2) { animation-delay: 110ms; }
.stats-grid .stat-card:nth-child(3) { animation-delay: 160ms; }
.stats-grid .stat-card:nth-child(4) { animation-delay: 210ms; }
.panels-grid .dashboard-panel:nth-child(1) { animation-delay: 250ms; }
.panels-grid .dashboard-panel:nth-child(2) { animation-delay: 300ms; }

.workload-progress-bar {
  background: linear-gradient(90deg, #38bdf8 0%, #2dd4bf 58%, #facc15 100%) !important;
  transform-origin: left center;
  animation: overview-progress-grow 700ms 320ms cubic-bezier(0.2, 0.8, 0.2, 1) both;
}

[data-theme='dark'] .dashboard-header {
  background:
    radial-gradient(circle at 16% 22%, rgba(56, 189, 248, 0.18), transparent 24%),
    linear-gradient(135deg, rgba(30,41,59,0.92), rgba(15,23,42,0.86)),
    #0f172a !important;
  border-color: rgba(125, 211, 252, 0.18) !important;
}

[data-theme='dark'] .stat-card,
[data-theme='dark'] .dashboard-panel {
  background:
    linear-gradient(135deg, rgba(30,41,59,0.90), rgba(15,23,42,0.88)),
    #0f172a !important;
  border-color: rgba(148, 163, 184, 0.18) !important;
}

[data-theme='dark'] .project-title,
[data-theme='dark'] .stat-value,
[data-theme='dark'] .panel-title,
[data-theme='dark'] .task-title-btn,
[data-theme='dark'] .user-name {
  color: #f8fafc !important;
}

[data-theme='dark'] .project-subtitle,
[data-theme='dark'] .stat-label,
[data-theme='dark'] .workload-count {
  color: #cbd5e1 !important;
}

[data-theme='light'] .project-title,
[data-theme='light'] .stat-value,
[data-theme='light'] .panel-title,
[data-theme='light'] .task-title-btn,
[data-theme='light'] .user-name {
  color: #0f172a !important;
}

[data-theme='light'] .project-subtitle,
[data-theme='light'] .stat-label,
[data-theme='light'] .workload-count {
  color: #475569 !important;
}

@media (prefers-reduced-motion: reduce) {
  .dashboard-header,
  .dashboard-header::before,
  .stat-card,
  .dashboard-panel {
    animation: none !important;
    transition: none !important;
  }
}

@media (max-width: 760px) {
  .space-dashboard-page {
    padding: 12px !important;
  }

  .dashboard-header {
    padding: 14px !important;
  }

  .stats-grid,
  .panels-grid {
    grid-template-columns: 1fr !important;
  }
}
</style>
