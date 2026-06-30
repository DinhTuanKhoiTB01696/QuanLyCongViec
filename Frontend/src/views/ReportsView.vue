<template>
  <NexusLayout>
    <div class="space-reports-page">
      <header class="reports-header">
        <div>
          <span class="reports-tag">{{ t('reports.analyticsReport', 'Analytics Report') }}</span>
          <h1 class="reports-title">{{ t('projectTabs.reports', 'Reports') }}</h1>
          <p class="reports-subtitle">{{ t('reports.analyticsAndInsights', 'Analytics and insights for this project') }}</p>
        </div>
        <div class="reports-actions">
          <button class="btn-secondary" @click="fetchData">
            <i class="fa-solid fa-rotate-right" :class="{ 'fa-spin': loading }"></i> {{ t('reports.refresh', 'Refresh') }}
          </button>
        </div>
      </header>
      
      <!-- Loading State -->
      <div v-if="loading" class="reports-loading">
        <i class="fa-solid fa-spinner fa-spin text-3xl mb-3 text-[var(--color-accent)]"></i>
        <p class="text-sm font-medium">{{ t('reports.analyzingProjectData', 'Analyzing project data...') }}</p>
      </div>
      
      <!-- Error State -->
      <div v-else-if="error" class="reports-error">
        <i class="fa-solid fa-circle-exclamation text-2xl mb-2"></i>
        <p class="font-semibold">{{ error }}</p>
      </div>

      <div v-else-if="allTasks.length === 0" class="reports-empty-container">
        <div class="reports-empty-state">
          <i class="fa-solid fa-chart-line text-5xl mb-4 text-[var(--color-text-muted)]"></i>
          <h3 class="text-xl font-semibold text-[var(--color-text-primary)] mb-2">{{ t('reports.noReportsToGenerate', 'No reports to generate') }}</h3>
          <p class="text-[var(--color-text-secondary)] mb-6 max-w-md mx-auto">{{ t('reports.noTasksPlaceholderDesc', "This project doesn't have any tasks yet. Create a few tasks to see statistics, charts, and workload distributions here.") }}</p>
        </div>
      </div>
      <!-- Main Dashboard Grid -->
      <div v-else class="reports-content">
        
        <!-- Premium Stats Cards -->
        <div class="reports-stats-grid">
          <!-- Total Tasks -->
          <div class="report-stat-card total-tasks">
            <div class="stat-card-glow"></div>
            <div class="stat-card-content">
              <div class="stat-icon-wrapper">
                <i class="fa-solid fa-list-check"></i>
              </div>
              <div class="stat-info">
                <span class="label">{{ t('reports.totalTasks', 'Total Tasks') }}</span>
                <span class="value">{{ allTasks.length }}</span>
              </div>
            </div>
          </div>
          
          <!-- Done Tasks -->
          <div class="report-stat-card done-tasks">
            <div class="stat-card-glow"></div>
            <div class="stat-card-content">
              <div class="stat-icon-wrapper">
                <i class="fa-solid fa-circle-check"></i>
              </div>
              <div class="stat-info">
                <span class="label">{{ t('reports.completedTasks', 'Completed Tasks') }}</span>
                <span class="value">{{ completedTasksCount }}</span>
              </div>
            </div>
          </div>
          
          <!-- In Progress -->
          <div class="report-stat-card in-progress">
            <div class="stat-card-glow"></div>
            <div class="stat-card-content">
              <div class="stat-icon-wrapper">
                <i class="fa-solid fa-clock-rotate-left"></i>
              </div>
              <div class="stat-info">
                <span class="label">{{ t('reports.inProgress', 'In Progress') }}</span>
                <span class="value">{{ inProgressTasksCount }}</span>
              </div>
            </div>
          </div>
          
          <!-- Overdue Tasks -->
          <div class="report-stat-card overdue-tasks" :class="{ 'has-overdue': overdueTasksCount > 0 }">
            <div class="stat-card-glow"></div>
            <div class="stat-card-content">
              <div class="stat-icon-wrapper">
                <i class="fa-solid fa-triangle-exclamation"></i>
              </div>
              <div class="stat-info">
                <span class="label">{{ t('reports.overdueTasks', 'Overdue Tasks') }}</span>
                <span class="value">{{ overdueTasksCount }}</span>
              </div>
            </div>
          </div>
        </div>

        <!-- Distributions Grid -->
        <div class="distributions-grid">
          
          <!-- Status Distribution Card -->
          <div class="report-card">
            <h3 class="card-title">
              <i class="fa-solid fa-chart-bar text-sky-500"></i> {{ t('reports.statusDistribution', 'Status Distribution') }}
            </h3>
            <div class="status-list">
              <div v-for="status in statusDistribution" :key="status.name" class="status-item">
                <div class="status-item-header">
                  <span class="status-badge" :style="{ color: getStatusColor(status.name), backgroundColor: getStatusBgColor(status.name) }">
                    <span class="status-dot"></span>
                    {{ getStatusLabel(status.name) }}
                  </span>
                  <span class="status-count">
                    <strong>{{ status.count }}</strong> 
                    <span class="percentage-label">({{ status.percentage }}%)</span>
                  </span>
                </div>
                <div class="status-progress-track">
                  <div 
                    class="status-progress-bar" 
                    :style="{ width: `${status.percentage}%`, backgroundColor: getStatusColor(status.name) }"
                  ></div>
                </div>
              </div>
            </div>
          </div>

          <!-- Priority Distribution Card -->
          <div class="report-card">
            <h3 class="card-title">
              <i class="fa-solid fa-chart-pie text-indigo-500"></i> {{ t('reports.priorityDistribution', 'Priority Distribution') }}
            </h3>
            <div class="priority-chart-container">
              <!-- Custom Donut Chart (SVG) -->
              <div class="donut-chart-wrapper">
                <svg viewBox="0 0 36 36" class="donut-chart">
                  <circle cx="18" cy="18" r="14.5" fill="none" stroke="var(--color-border)" stroke-width="3.5"></circle>
                  <circle 
                    v-for="(seg, idx) in prioritySegments"
                    :key="seg.label"
                    cx="18" 
                    cy="18" 
                    r="14.5" 
                    fill="none" 
                    :stroke="seg.color" 
                    stroke-width="3.5"
                    :stroke-dasharray="`${seg.percent} ${100 - seg.percent}`"
                    :stroke-dashoffset="seg.offset"
                    class="donut-segment"
                  ></circle>
                </svg>
                <div class="donut-center">
                  <span class="donut-number">{{ allTasks.length }}</span>
                  <span class="donut-label">{{ t('reports.tasks', 'Tasks') }}</span>
                </div>
              </div>

              <!-- Legend -->
              <div class="priority-legend">
                <div 
                  v-for="seg in prioritySegments" 
                  :key="seg.label"
                  class="legend-item"
                >
                  <div class="legend-info">
                    <span class="legend-color-dot" :style="{ background: seg.color }"></span>
                    <span class="legend-label">{{ t('workItems.priority.' + seg.label.toLowerCase(), seg.label) }}</span>
                  </div>
                  <div class="legend-value">
                    <span class="legend-count">{{ seg.count }}</span>
                    <span class="legend-percent">{{ Math.round(seg.percent) }}%</span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Workload and Overdue Grid -->
        <div class="workload-grid">
          
          <!-- Team Workload Panel -->
          <div class="report-card">
            <h3 class="card-title">
              <i class="fa-solid fa-users text-emerald-500"></i> {{ t('reports.workloadAndCompletionProgress', 'Workload & Completion Progress') }}
            </h3>
            
            <div v-if="teamWorkload.length === 0" class="workload-empty">
              <i class="fa-solid fa-users-slash text-4xl mb-3 text-[var(--color-text-muted)]"></i>
              <span class="text-sm font-semibold">{{ t('reports.noAssigneeDataAvailable', 'No assignee data available') }}</span>
              <p class="text-xs text-[var(--color-text-muted)] mt-1">{{ t('reports.assignTasksToUsersDesc', 'Assign tasks to users to track workload metrics.') }}</p>
            </div>

            <div v-else class="workload-list">
              <div v-for="member in teamWorkload" :key="member.userId" class="workload-item">
                <div class="workload-item-top">
                  <div class="workload-user-info">
                    <UserAvatar :user="{ avatarColor: member.avatarColor || getAvatarBg(member.fullName), initials: member.avatar, fullName: member.fullName }" :size="32" :fontSize="14" />
                    <span class="workload-name">{{ member.fullName }}</span>
                  </div>
                  <span class="workload-completion-text">
                    <strong>{{ member.doneCount }}</strong> / {{ member.count }} {{ t('reports.tasksCompleted', 'tasks completed') }}
                  </span>
                </div>
                <div class="workload-progress-container">
                  <div class="workload-progress-track">
                    <div 
                      class="workload-progress-bar" 
                      :style="{ width: `${member.completionRate}%` }"
                    ></div>
                  </div>
                  <span class="workload-percentage">{{ member.completionRate }}%</span>
                </div>
              </div>
            </div>
          </div>

          <!-- Overdue Tasks Card -->
          <div class="report-card">
            <h3 class="card-title">
              <i class="fa-solid fa-circle-exclamation text-rose-500"></i> {{ t('reports.overdueTaskAlert', 'Overdue Task Alert') }}
            </h3>
            
            <div v-if="overdueTasks.length === 0" class="overdue-empty">
              <div class="overdue-empty-icon">
                <i class="fa-solid fa-circle-check text-4xl text-green-400"></i>
              </div>
              <h4 class="font-bold text-sm text-green-400 mt-2">{{ t('reports.allTasksOnTrack', 'All tasks on track!') }}</h4>
              <p class="text-xs text-[var(--color-text-muted)] mt-1">{{ t('reports.noOverduePendingTasks', 'There are no overdue pending tasks in this project.') }}</p>
            </div>

            <div v-else class="overdue-list-container">
              <div class="overdue-list">
                <div 
                  v-for="task in overdueTasks" 
                  :key="task.id"
                  class="overdue-task-card"
                  @click="navigateToTask(task.id)"
                >
                  <div class="overdue-task-left">
                    <div class="overdue-task-header">
                      <span class="overdue-task-key">{{ task.sequenceId || 'TASK' }}</span>
                      <h4 class="overdue-task-title">{{ task.title }}</h4>
                    </div>
                    <div class="overdue-task-meta">
                      <span class="meta-item">
                        <i class="fa-solid fa-user text-xs"></i>
                        {{ getAssigneeNames(task) }}
                      </span>
                    </div>
                  </div>
                  <div class="overdue-task-right">
                    <span class="overdue-badge">
                      <i class="fa-solid fa-triangle-exclamation"></i> {{ t('reports.overdue', 'Overdue') }}
                    </span>
                    <span class="overdue-date">{{ t('reports.due', 'Due:') }} {{ formatDate(task.dueDate) }}</span>
                  </div>
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
import { useI18n } from '@/composables/useI18n'
import UserAvatar from '@/components/common/UserAvatar.vue'

const { t } = useI18n()
import { useWorkTaskStore } from '@/store/useWorkTaskStore'

const route = useRoute()
const router = useRouter()
const projectId = computed(() => route.params.id)
const workTaskStore = useWorkTaskStore()

const loading = ref(false)
const error = ref(null)

const allTasks = computed(() => workTaskStore.tasks || [])

const doneStatuses = ['done', 'completed', 'finished', 'hoàn thành', 'success', 'hoàn tất']
const cancelStatuses = ['cancel', 'cancelled', 'hủy', 'hủy bỏ']

const completedTasksCount = computed(() => {
  return allTasks.value.filter(task => {
    const s = (task.statusName || '').toLowerCase().trim()
    return doneStatuses.includes(s)
  }).length
})

const inProgressTasksCount = computed(() => {
  return allTasks.value.filter(task => {
    const s = (task.statusName || '').toLowerCase().trim()
    return s === 'in progress' || s === 'inprogress'
  }).length
})

const overdueTasks = computed(() => {
  const todayStr = new Date().toISOString().slice(0, 10)
  return allTasks.value.filter(task => {
    if (!task.dueDate) return false
    const s = (task.statusName || '').toLowerCase().trim()
    const isCompleted = doneStatuses.includes(s) || cancelStatuses.includes(s)
    return !isCompleted && task.dueDate < todayStr
  })
})

const overdueTasksCount = computed(() => overdueTasks.value.length)

// Status Distribution list
const statusDistribution = computed(() => {
  if (allTasks.value.length === 0) return []
  
  const statusCounts = {}
  allTasks.value.forEach(task => {
    const s = (task.statusName || 'No status').trim()
    statusCounts[s] = (statusCounts[s] || 0) + 1
  })

  return Object.entries(statusCounts).map(([name, count]) => ({
    name,
    count,
    percentage: Math.round((count / allTasks.value.length) * 100)
  })).sort((a, b) => b.count - a.count)
})

// Priority Distribution Segments (used for Custom SVG Donut Chart)
const prioritySegments = computed(() => {
  if (allTasks.value.length === 0) return []

  const counts = { Urgent: 0, High: 0, Medium: 0, Low: 0, None: 0 }
  allTasks.value.forEach(task => {
    switch (task.priority) {
      case 1: counts.Urgent++; break
      case 2: counts.High++; break
      case 3: counts.Medium++; break
      case 4: counts.Low++; break
      default: counts.None++; break
    }
  })

  const priorities = [
    { label: 'Urgent', count: counts.Urgent, color: '#ef4444' },
    { label: 'High', count: counts.High, color: '#f97316' },
    { label: 'Medium', count: counts.Medium, color: '#3b82f6' },
    { label: 'Low', count: counts.Low, color: '#a1a1aa' },
    { label: 'None', count: counts.None, color: '#4b5563' }
  ]

  const total = allTasks.value.length
  let currentOffset = 0

  return priorities.map(p => {
    const percent = total > 0 ? (p.count / total) * 100 : 0
    const offset = currentOffset
    currentOffset -= percent
    return {
      ...p,
      percent,
      offset
    }
  })
})

// Team Workload list
const teamWorkload = computed(() => {
  const membersMap = {}
  allTasks.value.forEach(task => {
    const taskAssignees = task.assignees || []
    const s = (task.statusName || '').toLowerCase().trim()
    const isCompleted = doneStatuses.includes(s)

    if (taskAssignees.length === 0) {
      if (!membersMap['unassigned']) {
        membersMap['unassigned'] = {
          userId: 'unassigned',
          fullName: 'Unassigned',
          avatar: null,
          count: 0,
          doneCount: 0
        }
      }
      membersMap['unassigned'].count++
      if (isCompleted) membersMap['unassigned'].doneCount++
    } else {
      taskAssignees.forEach(assignee => {
        const uid = assignee.userId || assignee.id
        if (!uid) return
        if (!membersMap[uid]) {
          const name = assignee.fullName || assignee.name || 'Unknown'
          membersMap[uid] = {
            userId: uid,
            fullName: name,
            avatar: assignee.initials || name.substring(0, 1).toUpperCase(),
            avatarColor: assignee.avatarColor,
            count: 0,
            doneCount: 0
          }
        }
        membersMap[uid].count++
        if (isCompleted) membersMap[uid].doneCount++
      })
    }
  })

  return Object.values(membersMap).map(member => ({
    ...member,
    completionRate: member.count > 0 ? Math.round((member.doneCount / member.count) * 100) : 0
  })).sort((a, b) => b.count - a.count)
})

const getStatusLabel = (statusName) => {
  if (!statusName) return ''
  const norm = statusName.toLowerCase().trim()
  const keyMap = {
    'backlog': 'workItems.statusLabels.backlog',
    'to do': 'workItems.statusLabels.toDo',
    'in progress': 'workItems.statusLabels.inProgress',
    'in review': 'workItems.statusLabels.inReview',
    'done': 'workItems.statusLabels.done',
    'cancelled': 'workItems.statusLabels.cancelled'
  }
  const key = keyMap[norm]
  return key ? t(key) : statusName
}

const getStatusColor = (statusName) => {
  const norm = statusName.toLowerCase().trim()
  if (norm.includes('backlog')) return '#64748b'
  if (norm.includes('plan')) return '#3b82f6'
  if (norm.includes('progress')) return '#fbbf24'
  if (norm.includes('pause')) return '#94a3b8'
  if (norm.includes('done') || norm.includes('complete') || norm.includes('success')) return '#10b981'
  if (norm.includes('cancel')) return '#ef4444'
  return '#0ea5e9'
}

const getStatusBgColor = (statusName) => {
  return getStatusColor(statusName) + '1a'
}

const getAvatarBg = (name) => {
  if (!name || name === 'Unassigned') return '#64748b'
  const colors = ['#3b82f6', '#10b981', '#fbbf24', '#ec4899', '#8b5cf6', '#06b6d4', '#f97316']
  let hash = 0
  for (let i = 0; i < name.length; i++) {
    hash = name.charCodeAt(i) + ((hash << 5) - hash)
  }
  const index = Math.abs(hash) % colors.length
  return colors[index]
}

const getAssigneeNames = (task) => {
  if (!task.assignees || task.assignees.length === 0) return 'Unassigned'
  return task.assignees.map(a => a.fullName || a.name || 'Unknown').join(', ')
}

const formatDate = (value) => {
  if (!value) return ''
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return value
  const day = `${date.getDate()}`.padStart(2, '0')
  const month = `${date.getMonth() + 1}`.padStart(2, '0')
  const year = date.getFullYear()
  return `${day}/${month}/${year}`
}

const navigateToTask = (taskId) => {
  router.push({
    name: 'SpaceSummary',
    params: { id: projectId.value },
    query: { task: taskId }
  })
}

const fetchData = async () => {
  loading.value = true
  error.value = null
  try {
    await workTaskStore.fetchTasks(projectId.value)
  } catch (e) {
    error.value = "Failed to load project reports. Please try again."
    console.error(e)
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  fetchData()
})
</script>

<style scoped>
/* Page Layout Wrapper */
.space-reports-page {
  width: 100%;
  max-width: 1120px;
  margin: 0 auto;
  padding: 34px clamp(20px, 4vw, 48px) 54px;
  min-height: calc(100vh - 64px);
  color: var(--color-text-primary);
  display: flex;
  flex-direction: column;
  gap: 28px;
  font-family: 'Inter', system-ui, sans-serif;
  background:
    radial-gradient(circle at 16% 0%, rgba(14, 165, 233, 0.10), transparent 30%),
    linear-gradient(180deg, #f8fbff, #eef5fb 52%, #f8fafc);
}

/* Header Styles */
.reports-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  border: 1px solid rgba(148, 163, 184, 0.22);
  border-radius: 18px;
  padding: 24px;
  background: rgba(255, 255, 255, 0.9);
  box-shadow: 0 14px 34px rgba(15, 23, 42, 0.065);
}

@media (max-width: 640px) {
  .reports-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 16px;
  }
}

.reports-tag {
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

.reports-title {
  font-size: clamp(28px, 2.8vw, 38px);
  font-weight: 900;
  letter-spacing: 0;
  color: var(--color-text-primary);
  margin: 0;
  line-height: 1.2;
}

.reports-subtitle {
  font-size: 14px;
  color: var(--color-text-muted);
  margin-top: 6px;
}

/* Loading, Error, Empty states */
.reports-loading, .reports-error {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 80px 0;
  color: var(--color-text-muted);
}

.reports-error {
  color: var(--color-danger);
  background: var(--color-danger-bg);
  border: 1px solid rgba(239, 68, 68, 0.2);
  border-radius: 12px;
  padding: 24px;
  text-align: center;
}

.reports-empty-container {
  display: flex;
  justify-content: center;
  align-items: center;
  padding: 60px 20px;
}

.reports-empty-state {
  text-align: center;
  background: var(--color-surface);
  border: 1px dashed var(--color-border);
  border-radius: 12px;
  padding: 40px;
  max-width: 600px;
  width: 100%;
}

/* Content Area */
.reports-content {
  display: flex;
  flex-direction: column;
  gap: 28px;
}

/* Stats Cards Grid */
.reports-stats-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 16px;
  width: 100%;
}

@media (max-width: 1024px) {
  .reports-stats-grid {
    grid-template-columns: repeat(2, 1fr);
  }
}

@media (max-width: 640px) {
  .reports-stats-grid {
    grid-template-columns: 1fr;
  }
}

.report-stat-card {
  background: rgba(255, 255, 255, 0.86);
  border: 1px solid rgba(148, 163, 184, 0.22);
  border-radius: 16px !important;
  padding: 20px;
  position: relative;
  overflow: hidden;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  box-shadow: 0 18px 44px rgba(15, 23, 42, 0.08);
}

.report-stat-card:hover {
  transform: translateY(-1px);
  box-shadow: 0 28px 64px rgba(15, 23, 42, 0.12);
}

.stat-card-glow {
  position: absolute;
  top: -50%;
  left: -50%;
  width: 200%;
  height: 200%;
  background: radial-gradient(circle, rgba(56, 189, 248, 0.04) 0%, transparent 75%);
  opacity: 0;
  transition: opacity 0.3s ease;
  pointer-events: none;
}

.report-stat-card:hover .stat-card-glow {
  opacity: 1;
}

.stat-card-content {
  display: flex;
  align-items: center;
  gap: 18px;
  position: relative;
  z-index: 1;
}

.stat-icon-wrapper {
  width: 48px;
  height: 48px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 20px;
  transition: all 0.3s;
}

/* Color schemes for stats cards */
.total-tasks .stat-icon-wrapper {
  background: rgba(56, 189, 248, 0.08);
  color: var(--color-accent);
}
.total-tasks:hover { border-color: var(--color-accent); }

.done-tasks .stat-icon-wrapper {
  background: rgba(16, 185, 129, 0.08);
  color: #10b981;
}
.done-tasks:hover { border-color: #10b981; }

.in-progress .stat-icon-wrapper {
  background: rgba(245, 158, 11, 0.08);
  color: #f59e0b;
}
.in-progress:hover { border-color: #f59e0b; }

.overdue-tasks .stat-icon-wrapper {
  background: rgba(148, 163, 184, 0.08);
  color: var(--color-text-muted);
}
.overdue-tasks.has-overdue .stat-icon-wrapper {
  background: rgba(239, 68, 68, 0.08);
  color: #ef4444;
}
.overdue-tasks.has-overdue:hover { border-color: #ef4444; }

.report-stat-card .label {
  font-size: 11px;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--color-text-muted);
}

.report-stat-card .value {
  font-size: 32px;
  font-weight: 850;
  line-height: 1.1;
  color: var(--color-text-primary);
  margin-top: 4px;
  display: block;
}

/* Card Design Pattern */
.report-card {
  background: rgba(255, 255, 255, 0.86);
  border: 1px solid rgba(148, 163, 184, 0.22);
  border-radius: 16px !important;
  padding: 24px;
  box-shadow: 0 14px 34px rgba(15, 23, 42, 0.065);
  display: flex;
  flex-direction: column;
  transition: box-shadow 0.3s ease, border-color 0.3s ease;
}

.report-card:hover {
  box-shadow: 0 24px 58px rgba(15, 23, 42, 0.10);
}

[data-theme='dark'] .space-reports-page {
  background:
    radial-gradient(circle at 14% 0%, rgba(14, 165, 233, 0.11), transparent 30%),
    linear-gradient(180deg, #07111f, #0f172a 52%, #101827);
}

[data-theme='dark'] .reports-header,
[data-theme='dark'] .report-stat-card,
[data-theme='dark'] .report-card {
  border-color: rgba(148, 163, 184, 0.18);
  background: rgba(15, 23, 42, 0.78);
  box-shadow: 0 18px 44px rgba(0, 0, 0, 0.24);
}

.card-title {
  font-size: 18px;
  font-weight: 750;
  color: var(--color-text-primary);
  margin-bottom: 20px;
  display: flex;
  align-items: center;
  gap: 10px;
  padding-bottom: 12px;
  border-bottom: 1px solid var(--color-border);
}

/* Grid Layouts for cards */
.distributions-grid, .workload-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 24px;
}

@media (max-width: 1024px) {
  .distributions-grid, .workload-grid {
    grid-template-columns: 1fr;
  }
}

/* Status Distribution styles */
.status-list {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.status-item {
  display: flex;
  flex-direction: column;
  gap: 8px;
  padding: 8px;
  border-radius: 8px;
  transition: background-color 0.2s;
}

.status-item:hover {
  background-color: var(--color-surface-hover);
}

.status-item-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.status-badge {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 4px 10px;
  border-radius: 999px;
  font-size: 11px;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.02em;
}

.status-dot {
  width: 6px;
  height: 6px;
  border-radius: 50%;
  background: currentColor;
}

.status-count {
  font-size: 13px;
  color: var(--color-text-primary);
}

.percentage-label {
  color: var(--color-text-muted);
  font-size: 11px;
  font-weight: 500;
}

.status-progress-track {
  width: 100%;
  background: var(--color-border);
  height: 6px;
  border-radius: 999px;
  overflow: hidden;
}

.status-progress-bar {
  height: 100%;
  border-radius: 999px;
  transition: width 0.6s cubic-bezier(0.4, 0, 0.2, 1);
}

/* Donut Chart and Legend styles */
.priority-chart-container {
  display: flex;
  align-items: center;
  gap: 32px;
  justify-content: center;
  flex-wrap: wrap;
}

.donut-chart-wrapper {
  position: relative;
  width: 150px;
  height: 150px;
  flex-shrink: 0;
}

.donut-chart {
  width: 100%;
  height: 100%;
  transform: rotate(-90deg);
}

.donut-segment {
  transition: stroke-dasharray 0.5s ease, stroke-dashoffset 0.5s ease;
}

.donut-center {
  position: absolute;
  inset: 0;
  margin: auto;
  width: 86px;
  height: 86px;
  background: var(--color-surface);
  border-radius: 50%;
  box-shadow: inset 0 2px 4px rgba(0, 0, 0, 0.05);
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  border: 1px solid var(--color-border);
}

.donut-number {
  font-size: 26px;
  font-weight: 800;
  color: var(--color-text-primary);
  line-height: 1;
}

.donut-label {
  font-size: 9px;
  text-transform: uppercase;
  letter-spacing: 0.1em;
  color: var(--color-text-muted);
  margin-top: 4px;
  font-weight: 700;
}

.priority-legend {
  display: flex;
  flex-direction: column;
  gap: 8px;
  flex-grow: 1;
  min-width: 180px;
}

.legend-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 6px 12px;
  border-radius: 8px;
  background: rgba(0, 0, 0, 0.01);
  transition: all 0.2s;
  border: 1px solid transparent;
}

[data-theme='dark'] .legend-item {
  background: rgba(255, 255, 255, 0.01);
}

.legend-item:hover {
  background: var(--color-surface-hover);
  border-color: var(--color-border);
}

.legend-info {
  display: flex;
  align-items: center;
  gap: 10px;
}

.legend-color-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
}

.legend-label {
  font-size: 13px;
  font-weight: 600;
  color: var(--color-text-secondary);
}

.legend-value {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 13px;
}

.legend-count {
  font-weight: 750;
  color: var(--color-text-primary);
}

.legend-percent {
  font-weight: 500;
  color: var(--color-text-muted);
  font-size: 11px;
}

/* Workload Completion Panel styles */
.workload-empty, .overdue-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 48px 16px;
  text-align: center;
  border: 1px dashed var(--color-border);
  border-radius: 12px;
  background: rgba(0, 0, 0, 0.01);
  flex: 1;
}

[data-theme='dark'] .workload-empty, 
[data-theme='dark'] .overdue-empty {
  background: rgba(255, 255, 255, 0.01);
}

.overdue-empty {
  background: rgba(16, 185, 129, 0.01);
  border-color: rgba(16, 185, 129, 0.15);
}

.workload-list {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.workload-item {
  display: flex;
  flex-direction: column;
  gap: 10px;
  padding: 12px;
  border-radius: 12px;
  background: rgba(0, 0, 0, 0.01);
  border: 1px solid transparent;
  transition: all 0.2s;
}

[data-theme='dark'] .workload-item {
  background: rgba(255, 255, 255, 0.01);
}

.workload-item:hover {
  background: var(--color-surface-hover);
  border-color: var(--color-border);
  transform: translateX(4px);
}

.workload-item-top {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.workload-user-info {
  display: flex;
  align-items: center;
  gap: 12px;
}

.workload-avatar {
  width: 30px;
  height: 30px;
  border-radius: 50%;
  color: #fff;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 12px;
  font-weight: 700;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  border: 1.5px solid var(--color-surface);
}

.workload-name {
  font-size: 13px;
  font-weight: 650;
  color: var(--color-text-primary);
}

.workload-completion-text {
  font-size: 12px;
  color: var(--color-text-secondary);
}

.workload-completion-text strong {
  color: #10b981;
}

.workload-progress-container {
  display: flex;
  align-items: center;
  gap: 12px;
}

.workload-progress-track {
  flex-grow: 1;
  background: var(--color-border);
  height: 6px;
  border-radius: 999px;
  overflow: hidden;
}

.workload-progress-bar {
  height: 100%;
  border-radius: 999px;
  background: linear-gradient(90deg, #10b981 0%, #34d399 100%);
  transition: width 0.6s cubic-bezier(0.4, 0, 0.2, 1);
}

.workload-percentage {
  font-size: 12px;
  font-weight: 750;
  color: var(--color-text-primary);
  min-width: 32px;
  text-align: right;
}

/* Overdue Task Panel styles */
.overdue-list-container {
  flex: 1;
  overflow-y: auto;
  max-height: 310px;
  padding-right: 4px;
}

.overdue-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.overdue-task-card {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 14px 16px;
  border-radius: 12px;
  border: 1px solid rgba(239, 68, 68, 0.1);
  background: rgba(239, 68, 68, 0.02);
  transition: all 0.25s ease;
  cursor: pointer;
}

[data-theme='dark'] .overdue-task-card {
  background: rgba(239, 68, 68, 0.04);
}

.overdue-task-card:hover {
  transform: translateY(-2px);
  border-color: rgba(239, 68, 68, 0.3);
  background: rgba(239, 68, 68, 0.06);
  box-shadow: 0 4px 12px rgba(239, 68, 68, 0.04);
}

.overdue-task-left {
  display: flex;
  flex-direction: column;
  gap: 6px;
  overflow: hidden;
  margin-right: 16px;
}

.overdue-task-header {
  display: flex;
  align-items: center;
  gap: 10px;
}

.overdue-task-key {
  font-size: 10px;
  font-family: monospace;
  font-weight: 700;
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  padding: 2px 6px;
  border-radius: 4px;
  color: var(--color-text-muted);
  flex-shrink: 0;
}

.overdue-task-title {
  font-size: 13px;
  font-weight: 650;
  color: var(--color-text-primary);
  margin: 0;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.overdue-task-card:hover .overdue-task-title {
  color: var(--color-accent);
  text-decoration: underline;
}

.overdue-task-meta {
  display: flex;
  align-items: center;
  gap: 12px;
  font-size: 11px;
  color: var(--color-text-muted);
}

.meta-item {
  display: flex;
  align-items: center;
  gap: 6px;
}

.overdue-task-right {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 6px;
  flex-shrink: 0;
}

.overdue-badge {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  font-size: 9px;
  font-weight: 700;
  color: #ef4444;
  background: rgba(239, 68, 68, 0.1);
  padding: 3px 8px;
  border-radius: 6px;
  text-transform: uppercase;
  letter-spacing: 0.02em;
}

.overdue-date {
  font-size: 11px;
  color: var(--color-text-secondary);
  font-weight: 500;
}

/* Compact density */
.space-reports-page {
  max-width: 1080px !important;
  padding: 18px var(--sa-page-x, 24px) 30px !important;
  min-height: calc(100vh - var(--sa-topbar-height, 52px)) !important;
  gap: 16px !important;
}

.reports-header {
  border-radius: 10px !important;
  padding: 18px !important;
}

.reports-title {
  font-size: clamp(24px, 2.2vw, 32px) !important;
  line-height: 1.12 !important;
}

.reports-subtitle {
  font-size: 12.5px !important;
  margin-top: 4px !important;
}

.reports-content {
  gap: 16px !important;
}

.reports-stats-grid,
.distributions-grid,
.workload-grid {
  gap: 14px !important;
}

.report-stat-card,
.report-card {
  border-radius: 10px !important;
  padding: 18px !important;
  box-shadow: 0 10px 24px rgba(15, 23, 42, 0.06) !important;
}

.stat-card-content {
  gap: 12px !important;
}

.stat-icon-wrapper {
  width: 38px !important;
  height: 38px !important;
  border-radius: 8px !important;
  font-size: 16px !important;
}

.report-stat-card .value {
  font-size: 26px !important;
}

.card-title {
  font-size: 15px !important;
  margin-bottom: 14px !important;
  padding-bottom: 10px !important;
}

.donut-chart-wrapper {
  width: 118px !important;
  height: 118px !important;
}

.donut-center {
  width: 68px !important;
  height: 68px !important;
}

.donut-number {
  font-size: 22px !important;
}

.workload-list,
.status-list {
  gap: 10px !important;
}

.workload-item,
.overdue-task-card {
  border-radius: 8px !important;
  padding: 10px 12px !important;
}

@media (max-width: 720px) {
  .space-reports-page {
    padding: 12px !important;
  }

  .reports-header {
    padding: 14px !important;
    gap: 10px !important;
  }

  .reports-stats-grid,
  .distributions-grid,
  .workload-grid {
    grid-template-columns: 1fr !important;
  }

  .overdue-task-card,
  .workload-item-top {
    align-items: flex-start !important;
    flex-direction: column !important;
  }

  .overdue-task-right {
    align-items: flex-start !important;
  }
}

/* Premium reports presentation */
@keyframes reports-rise-in {
  from {
    opacity: 0;
    transform: translateY(16px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.space-reports-page {
  background:
    radial-gradient(circle at 16% 0%, rgba(56, 189, 248, 0.17), transparent 28%),
    radial-gradient(circle at 82% 8%, rgba(99, 102, 241, 0.10), transparent 26%),
    linear-gradient(180deg, #f8fcff 0%, #edf6fb 52%, #f8fafc 100%) !important;
}

.reports-header,
.report-stat-card,
.report-card {
  animation: reports-rise-in 520ms cubic-bezier(0.2, 0.8, 0.2, 1) both;
  transition:
    transform 220ms cubic-bezier(0.2, 0.8, 0.2, 1),
    box-shadow 220ms ease,
    border-color 220ms ease,
    background 220ms ease !important;
}

.reports-stats-grid .report-stat-card:nth-child(1) { animation-delay: 70ms; }
.reports-stats-grid .report-stat-card:nth-child(2) { animation-delay: 120ms; }
.reports-stats-grid .report-stat-card:nth-child(3) { animation-delay: 170ms; }
.reports-stats-grid .report-stat-card:nth-child(4) { animation-delay: 220ms; }
.report-card:nth-child(1) { animation-delay: 240ms; }
.report-card:nth-child(2) { animation-delay: 290ms; }
.report-card:nth-child(3) { animation-delay: 340ms; }
.report-card:nth-child(4) { animation-delay: 390ms; }

.reports-header {
  position: relative;
  overflow: hidden;
  background:
    linear-gradient(135deg, rgba(255, 255, 255, 0.98), rgba(240, 249, 255, 0.82)),
    var(--color-surface) !important;
  box-shadow:
    0 28px 80px rgba(14, 165, 233, 0.12),
    inset 0 1px 0 rgba(255,255,255,0.86) !important;
}

.reports-header::after {
  content: "";
  position: absolute;
  right: 24px;
  top: 22px;
  width: 72px;
  height: 72px;
  border-radius: 18px;
  background: linear-gradient(135deg, rgba(56,189,248,0.18), rgba(34,197,94,0.14));
  transform: rotate(10deg);
  pointer-events: none;
  z-index: 0;
}

.reports-header > div,
.reports-actions {
  position: relative;
  z-index: 1;
}

.reports-actions {
  align-self: center;
}

.reports-tag {
  color: #0284c7 !important;
  background: linear-gradient(135deg, rgba(56,189,248,0.15), rgba(45,212,191,0.12)) !important;
  border: 1px solid rgba(56,189,248,0.16);
}

.report-stat-card,
.report-card {
  position: relative;
  overflow: hidden;
  background:
    linear-gradient(135deg, rgba(255,255,255,0.96), rgba(248,250,252,0.84)),
    var(--color-surface) !important;
}

.report-stat-card::before,
.report-card::before {
  content: "";
  position: absolute;
  inset: 0 0 auto 0;
  height: 3px;
  background: linear-gradient(90deg, #38bdf8, #2dd4bf, #facc15);
  opacity: 0.88;
}

.report-stat-card:hover,
.report-card:hover {
  transform: translateY(-3px);
  box-shadow: 0 28px 76px rgba(15, 23, 42, 0.14) !important;
}

.report-stat-card .stat-icon-wrapper {
  box-shadow: 0 12px 30px rgba(15, 23, 42, 0.09);
}

.report-stat-card .value {
  letter-spacing: -0.02em;
}

.status-item,
.legend-item,
.workload-item,
.overdue-task-card {
  transition: transform 180ms ease, background 180ms ease, border-color 180ms ease;
}

.status-item:hover,
.legend-item:hover,
.workload-item:hover {
  transform: translateX(4px);
  background: color-mix(in srgb, var(--color-accent) 7%, var(--color-surface));
}

.overdue-task-card {
  background:
    linear-gradient(90deg, rgba(239, 68, 68, 0.08), transparent 72%),
    var(--color-surface) !important;
}

.overdue-task-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 18px 42px rgba(239, 68, 68, 0.12);
}

[data-theme='dark'] .space-reports-page {
  background:
    radial-gradient(circle at 14% 0%, rgba(56, 189, 248, 0.18), transparent 30%),
    radial-gradient(circle at 84% 10%, rgba(99, 102, 241, 0.10), transparent 28%),
    linear-gradient(180deg, #06111f, #0f172a 52%, #101827) !important;
}

[data-theme='dark'] .reports-header,
[data-theme='dark'] .report-stat-card,
[data-theme='dark'] .report-card {
  background:
    linear-gradient(135deg, rgba(30, 41, 59, 0.90), rgba(15, 23, 42, 0.86)),
    #0f172a !important;
}

[data-theme='light'] .reports-header,
[data-theme='light'] .report-stat-card,
[data-theme='light'] .report-card {
  background:
    linear-gradient(135deg, rgba(255,255,255,0.97), rgba(248,250,252,0.88)),
    #ffffff !important;
  color: #0f172a !important;
  border-color: rgba(148, 163, 184, 0.20) !important;
}

[data-theme='light'] .reports-title,
[data-theme='light'] .card-title,
[data-theme='light'] .report-stat-card .value,
[data-theme='light'] .workload-name,
[data-theme='light'] .overdue-task-title,
[data-theme='light'] .status-count strong,
[data-theme='light'] .legend-count,
[data-theme='light'] .donut-number {
  color: #0f172a !important;
}

[data-theme='light'] .reports-subtitle,
[data-theme='light'] .report-stat-card .label,
[data-theme='light'] .percentage-label,
[data-theme='light'] .legend-percent,
[data-theme='light'] .legend-label,
[data-theme='light'] .workload-completion-text,
[data-theme='light'] .overdue-task-meta,
[data-theme='light'] .overdue-date {
  color: #475569 !important;
}

[data-theme='dark'] .reports-title,
[data-theme='dark'] .card-title,
[data-theme='dark'] .report-stat-card .value,
[data-theme='dark'] .workload-name,
[data-theme='dark'] .overdue-task-title,
[data-theme='dark'] .status-count strong,
[data-theme='dark'] .legend-count,
[data-theme='dark'] .donut-number {
  color: #f8fafc !important;
}

[data-theme='dark'] .reports-subtitle,
[data-theme='dark'] .report-stat-card .label,
[data-theme='dark'] .percentage-label,
[data-theme='dark'] .legend-percent,
[data-theme='dark'] .legend-label,
[data-theme='dark'] .workload-completion-text,
[data-theme='dark'] .overdue-task-meta,
[data-theme='dark'] .overdue-date {
  color: #cbd5e1 !important;
}

[data-theme='light'] .status-badge,
[data-theme='light'] .overdue-task-key,
[data-theme='light'] .btn-secondary {
  background-color: #ffffff !important;
  border-color: rgba(148, 163, 184, 0.28) !important;
}

[data-theme='dark'] .status-badge,
[data-theme='dark'] .overdue-task-key,
[data-theme='dark'] .btn-secondary {
  background-color: rgba(15, 23, 42, 0.76) !important;
  border-color: rgba(148, 163, 184, 0.20) !important;
}

.status-item,
.legend-item,
.workload-item,
.overdue-task-card {
  border: 1px solid color-mix(in srgb, var(--color-border) 72%, transparent);
}

.overdue-task-header,
.workload-item-top,
.status-item-header {
  min-width: 0;
}

.overdue-task-title,
.workload-name,
.legend-label,
.status-badge {
  overflow: hidden;
  text-overflow: ellipsis;
}

.overdue-task-title,
.workload-name {
  overflow-wrap: anywhere;
}

.reports-header {
  display: flex !important;
  align-items: center !important;
  justify-content: space-between !important;
  gap: 24px !important;
  min-height: 132px !important;
}

.reports-title {
  max-width: 760px;
}

.reports-subtitle {
  max-width: 620px;
}

.report-card {
  min-width: 0;
}

.status-item,
.priority-item,
.legend-item,
.workload-item,
.overdue-task-card {
  min-width: 0;
}

@media (max-width: 720px) {
  .reports-header {
    min-height: 0 !important;
    align-items: flex-start !important;
    flex-direction: column !important;
  }

  .reports-header::after {
    width: 46px;
    height: 46px;
    right: 14px;
    top: 14px;
    opacity: 0.38;
  }
}

@media (prefers-reduced-motion: reduce) {
  .reports-header,
  .report-stat-card,
  .report-card {
    animation: none !important;
    transition: none !important;
  }
}
</style>
