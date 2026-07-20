<template>
  <ProjectPageContainer class="space-reports-page">
    <ProjectPageHeader 
      icon="fa-solid fa-chart-line" 
      :title="t('projectTabs.reports')"
      :description="t('reports.analyticsAndInsights')"
    >
      <template #actions>
        <button class="nexus-btn-outlined" @click="fetchData" :aria-label="t('reports.refresh')">
          <i class="fa-solid fa-rotate-right" :class="{ 'fa-spin': loading }"></i> {{ t('reports.refresh') }}
        </button>
      </template>
    </ProjectPageHeader>

    <!-- Loading State -->
    <ProjectLoadingState v-if="loading" :text="t('reports.analyzingProjectData')" />
    
    <!-- Error State -->
    <div v-else-if="error" class="reports-error">
      <i class="fa-solid fa-circle-exclamation text-2xl mb-2"></i>
      <p class="font-semibold">{{ error }}</p>
    </div>

    <!-- Empty State -->
    <ProjectEmptyState 
      v-else-if="allTasks.length === 0"
      icon="fa-solid fa-chart-line"
      :title="t('reports.noTasksPlaceholder')"
      :description="t('reports.noTasksPlaceholderDesc')"
    >
      <template #action>
        <button class="nexus-btn-primary" @click="router.push(`/space/${projectId}/work-items`)">
          <i class="fa-solid fa-plus"></i> {{ t('reports.createWorkItem') }}
        </button>
      </template>
    </ProjectEmptyState>

    <!-- Main Dashboard Grid -->
    <div v-else class="reports-content">
      
      <!-- Project Health Alert Card -->
      <div class="health-alert-card" :class="projectHealth.level">
        <div class="health-icon">
          <i class="fa-solid" :class="projectHealth.icon"></i>
        </div>
        <div class="health-details">
          <h2 class="health-status-title">{{ t('reports.healthStatus', { status: projectHealth.text }) }}</h2>
          <p class="health-desc">{{ projectHealth.desc }}</p>
        </div>
      </div>

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
              <span class="label">{{ t('reports.totalTasks') }}</span>
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
              <span class="label">{{ t('reports.completedTasks') }}</span>
              <span class="value">
                {{ completedTasksCount }}
                <span class="percentage-tag">{{ completionRate }}%</span>
              </span>
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
              <span class="label">{{ t('reports.inProgress') }}</span>
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
              <span class="label">{{ t('reports.overdueTasks') }}</span>
              <span class="value text-danger">{{ overdueTasksCount }}</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Distributions Grid -->
      <div class="distributions-grid">
        
        <!-- Cột trái: Việc cần chú ý & Thành viên cần nhắc -->
        <div class="dashboard-left-panel">
          
          <!-- Việc cần chú ý Accordions -->
          <div class="report-card attention-panel">
            <h3 class="card-title">
              <i class="fa-solid fa-bell text-rose-500"></i> {{ t('reports.attentionTitle') }}
            </h3>

            <div class="attention-accordions">
              
              <!-- 1. Quá hạn -->
              <div class="accordion-item" :class="{ active: activeAccordion === 'overdue' }">
                <div class="accordion-header" @click="activeAccordion = activeAccordion === 'overdue' ? '' : 'overdue'">
                  <div class="header-left">
                    <span class="badge danger-bg">{{ overdueTasksCount }}</span>
                    <span class="header-text">{{ t('reports.overdueTasks') }}</span>
                  </div>
                  <i class="fa-solid" :class="activeAccordion === 'overdue' ? 'fa-chevron-up' : 'fa-chevron-down'"></i>
                </div>
                <div class="accordion-content" v-show="activeAccordion === 'overdue'">
                  <div v-if="overdueTasks.length === 0" class="empty-substate text-success">
                    <i class="fa-solid fa-circle-check mr-2"></i> {{ t('reports.noOverdueTasks') }}
                  </div>
                  <div v-else class="attention-list">
                    <div v-for="task in overdueTasks" :key="task.id" class="attention-task-row">
                      <div class="task-info" @click="navigateToTask(task.id)">
                        <span class="task-key">{{ task.sequenceId || 'TASK' }}</span>
                        <span class="task-title">{{ task.title }}</span>
                        <span class="task-assignee">{{ t('reports.assignee') }}: {{ getAssigneeNames(task) }}</span>
                        <span class="task-due-date text-danger">{{ t('reports.dueDate') }} {{ formatDate(task.dueDate) }}</span>
                      </div>
                      <button 
                        class="remind-btn" 
                        @click="triggerReminder(task)"
                        :disabled="sendingReminders[task.id] || !task.assignees || task.assignees.length === 0"
                        :title="(!task.assignees || task.assignees.length === 0) ? t('reports.unassignedTooltip') : ''"
                      >
                        <i class="fa-solid fa-paper-plane"></i> {{ t('reports.remind') }}
                      </button>
                    </div>
                  </div>
                </div>
              </div>

              <!-- 2. Sắp đến hạn -->
              <div class="accordion-item" :class="{ active: activeAccordion === 'upcoming' }">
                <div class="accordion-header" @click="activeAccordion = activeAccordion === 'upcoming' ? '' : 'upcoming'">
                  <div class="header-left">
                    <span class="badge warning-bg">{{ upcomingTasks.length }}</span>
                    <span class="header-text">{{ t('reports.upcomingTasks') }}</span>
                  </div>
                  <i class="fa-solid" :class="activeAccordion === 'upcoming' ? 'fa-chevron-up' : 'fa-chevron-down'"></i>
                </div>
                <div class="accordion-content" v-show="activeAccordion === 'upcoming'">
                  <div v-if="upcomingTasks.length === 0" class="empty-substate">
                    {{ t('reports.noUpcomingTasks') }}
                  </div>
                  <div v-else class="attention-list">
                    <div v-for="task in upcomingTasks" :key="task.id" class="attention-task-row">
                      <div class="task-info" @click="navigateToTask(task.id)">
                        <span class="task-key">{{ task.sequenceId || 'TASK' }}</span>
                        <span class="task-title">{{ task.title }}</span>
                        <span class="task-assignee">{{ t('reports.assignee') }}: {{ getAssigneeNames(task) }}</span>
                        <span class="task-due-date text-warning">{{ t('reports.dueDate') }} {{ formatDate(task.dueDate) }}</span>
                      </div>
                      <button 
                        class="remind-btn" 
                        @click="triggerReminder(task)"
                        :disabled="sendingReminders[task.id] || !task.assignees || task.assignees.length === 0"
                        :title="(!task.assignees || task.assignees.length === 0) ? t('reports.unassignedTooltip') : ''"
                      >
                        <i class="fa-solid fa-paper-plane"></i> {{ t('reports.remind') }}
                      </button>
                    </div>
                  </div>
                </div>
              </div>

              <!-- 3. Chưa có người phụ trách -->
              <div class="accordion-item" :class="{ active: activeAccordion === 'unassigned' }">
                <div class="accordion-header" @click="activeAccordion = activeAccordion === 'unassigned' ? '' : 'unassigned'">
                  <div class="header-left">
                    <span class="badge gray-bg">{{ unassignedTasks.length }}</span>
                    <span class="header-text">{{ t('reports.unassignedTasks') }}</span>
                  </div>
                  <i class="fa-solid" :class="activeAccordion === 'unassigned' ? 'fa-chevron-up' : 'fa-chevron-down'"></i>
                </div>
                <div class="accordion-content" v-show="activeAccordion === 'unassigned'">
                  <div v-if="unassignedTasks.length === 0" class="empty-substate text-success">
                    <i class="fa-solid fa-circle-check mr-2"></i> {{ t('reports.allTasksAssigned') }}
                  </div>
                  <div v-else class="attention-list">
                    <div v-for="task in unassignedTasks" :key="task.id" class="attention-task-row plain-row" @click="navigateToTask(task.id)">
                      <div class="task-info">
                        <span class="task-key">{{ task.sequenceId || 'TASK' }}</span>
                        <span class="task-title">{{ task.title }}</span>
                        <span class="task-due-date">{{ t('reports.dueDate') }} {{ formatDate(task.dueDate) || t('reports.noDueDate') }}</span>
                      </div>
                      <span class="unassigned-badge"><i class="fa-solid fa-user-slash"></i> {{ t('reports.unassigned') }}</span>
                    </div>
                  </div>
                </div>
              </div>

              <!-- 4. Việc bị kẹt lâu -->
              <div class="accordion-item" :class="{ active: activeAccordion === 'stuck' }">
                <div class="accordion-header" @click="activeAccordion = activeAccordion === 'stuck' ? '' : 'stuck'">
                  <div class="header-left">
                    <span class="badge info-bg">{{ stuckTasks.length }}</span>
                    <span class="header-text">{{ t('reports.stuckTasks') }}</span>
                  </div>
                  <i class="fa-solid" :class="activeAccordion === 'stuck' ? 'fa-chevron-up' : 'fa-chevron-down'"></i>
                </div>
                <div class="accordion-content" v-show="activeAccordion === 'stuck'">
                  <div v-if="stuckTasks.length === 0" class="empty-substate">
                    {{ t('reports.noStuckTasks') }}
                  </div>
                  <div v-else class="attention-list">
                    <div v-for="task in stuckTasks" :key="task.id" class="attention-task-row">
                      <div class="task-info" @click="navigateToTask(task.id)">
                        <span class="task-key">{{ task.sequenceId || 'TASK' }}</span>
                        <span class="task-title">{{ task.title }}</span>
                        <span class="task-assignee">{{ t('reports.assignee') }}: {{ getAssigneeNames(task) }}</span>
                        <span class="task-status">{{ t('reports.status') }}: {{ getStatusLabel(task.statusName) }}</span>
                        <span class="task-due-date">{{ t('reports.lastUpdated') }}: {{ formatDate(task.updatedAt || task.createdAt) }}</span>
                      </div>
                      <button 
                        class="remind-btn" 
                        @click="triggerReminder(task)"
                        :disabled="sendingReminders[task.id] || !task.assignees || task.assignees.length === 0"
                        :title="(!task.assignees || task.assignees.length === 0) ? t('reports.unassignedTooltip') : ''"
                      >
                        <i class="fa-solid fa-paper-plane"></i> {{ t('reports.followUp') }}
                      </button>
                    </div>
                  </div>
                </div>
              </div>

            </div>
          </div>

          <!-- Thành viên cần nhắc (Overloaded / Pending workload) -->
          <div class="report-card workload-panel">
            <h3 class="card-title">
              <i class="fa-solid fa-users text-emerald-500"></i> {{ t('reports.membersToRemind') }}
            </h3>
            
            <div v-if="membersToRemind.length === 0" class="workload-empty">
              <i class="fa-solid fa-circle-check text-4xl mb-3 text-green-400"></i>
              <span class="text-sm font-semibold">{{ t('reports.allMembersOnTrack') }}</span>
              <p class="text-xs text-[var(--color-text-muted)] mt-1">{{ t('reports.noMemberWorkload') }}</p>
            </div>

            <div v-else class="workload-list">
              <div v-for="member in membersToRemind" :key="member.userId" class="remind-member-card" :class="{ 'has-overdue': member.overdueCount > 0 }">
                <div class="member-card-left">
                  <UserAvatar :user="{ id: member.userId, fullName: member.fullName, name: member.fullName, avatarColor: member.avatarColor, avatarUrl: member.avatarUrl }" :size="40" :fontSize="16" />
                  <div class="member-details">
                    <span class="member-name">{{ member.fullName }}</span>
                    <div class="member-stats">
                      <span class="badge gray-bg">{{ t('reports.pendingCount', { count: member.pendingCount }) }}</span>
                      <span class="badge danger-bg" v-if="member.overdueCount > 0">{{ t('reports.overdueCount', { count: member.overdueCount }) }}</span>
                    </div>
                    <p class="deadline-tip" v-if="member.nearestDeadlineTask">
                      <i class="fa-solid fa-hourglass-half"></i> {{ t('reports.nearestDeadline') }}:
                      <strong @click="navigateToTask(member.nearestDeadlineTask.id)" class="hover-task-link">{{ member.nearestDeadlineTask.title }}</strong> 
                      ({{ formatDate(member.nearestDeadlineTask.dueDate) }})
                    </p>
                  </div>
                </div>
                <button 
                  class="remind-member-btn" 
                  @click="triggerMemberReminder(member)"
                  :disabled="sendingMemberReminders[member.userId]"
                >
                  <i class="fa-solid fa-bell"></i> {{ t('reports.remind') }}
                </button>
              </div>
            </div>
          </div>

        </div>
        
        <!-- Cột phải: Biểu đồ phân phối -->
        <div class="dashboard-right-panel">
          
          <!-- Status Distribution Card -->
          <div class="report-card">
            <h3 class="card-title">
              <i class="fa-solid fa-chart-bar text-sky-500"></i> {{ t('reports.statusDistribution') }}
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
              <i class="fa-solid fa-chart-pie text-indigo-500"></i> {{ t('reports.priorityDistribution') }}
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
                  <span class="donut-label">{{ t('reports.tasks') }}</span>
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

      </div>
    </div>
  </ProjectPageContainer>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useI18n } from '@/composables/useI18n'
import UserAvatar from '@/components/common/UserAvatar.vue'
import ProjectPageContainer from '@/components/common/ProjectPageContainer.vue'
import ProjectPageHeader from '@/components/common/ProjectPageHeader.vue'
import ProjectEmptyState from '@/components/common/ProjectEmptyState.vue'
import ProjectLoadingState from '@/components/common/ProjectLoadingState.vue'
import { useWorkTaskStore } from '@/store/useWorkTaskStore'
import { getStoredUser } from '@/utils/permissions'
import axiosClient from '@/api/axiosClient'
import { ElMessage } from 'element-plus'

const { t } = useI18n()
const route = useRoute()
const router = useRouter()
const projectId = computed(() => route.params.id)
const workTaskStore = useWorkTaskStore()

const loading = ref(false)
const error = ref(null)
const activeAccordion = ref('overdue')
const sendingReminders = ref({})
const sendingMemberReminders = ref({})

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

const completionRate = computed(() => {
  const total = allTasks.value.length
  if (total === 0) return 0
  return Math.round((completedTasksCount.value / total) * 100)
})

const todayStr = new Date().toISOString().slice(0, 10)

// 1. Project status logic (Green / Yellow / Red)
const projectHealth = computed(() => {
  const total = allTasks.value.length
  if (total === 0) return { level: 'gray', text: t('reports.health.noData'), icon: 'fa-triangle-exclamation', desc: t('reports.health.noDataDesc') }
  
  const completed = completedTasksCount.value
  const overdue = overdueTasksCount.value
  const compRate = completionRate.value
  const overdueRate = Math.round((overdue / total) * 100)
  
  if (overdueRate >= 30 || compRate < 40) {
    return {
      level: 'red',
      text: t('reports.health.red'),
      icon: 'fa-circle-xmark',
      desc: t('reports.health.redDesc', { completion: compRate, overdue, overdueRate })
    }
  } else if (overdue > 0 || compRate < 70) {
    return {
      level: 'yellow',
      text: t('reports.health.yellow'),
      icon: 'fa-triangle-exclamation',
      desc: t('reports.health.yellowDesc', { overdue, overdueRate, completion: compRate })
    }
  } else {
    return {
      level: 'green',
      text: t('reports.health.green'),
      icon: 'fa-circle-check',
      desc: t('reports.health.greenDesc', { completion: compRate })
    }
  }
})

// 2. Attention Tasks filters
const overdueTasks = computed(() => {
  return allTasks.value.filter(task => {
    if (!task.dueDate) return false
    const s = (task.statusName || '').toLowerCase().trim()
    const isCompleted = doneStatuses.includes(s) || cancelStatuses.includes(s)
    return !isCompleted && task.dueDate < todayStr
  }).sort((a, b) => new Date(a.dueDate) - new Date(b.dueDate))
})

const overdueTasksCount = computed(() => overdueTasks.value.length)

const upcomingTasks = computed(() => {
  const today = new Date()
  today.setHours(0,0,0,0)
  
  return allTasks.value.filter(task => {
    if (!task.dueDate) return false
    const s = (task.statusName || '').toLowerCase().trim()
    const isCompleted = doneStatuses.includes(s) || cancelStatuses.includes(s)
    if (isCompleted) return false
    
    const dueDate = new Date(task.dueDate)
    dueDate.setHours(0,0,0,0)
    
    const diffTime = dueDate.getTime() - today.getTime()
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24))
    
    // Within 48 hours (0 to 2 days)
    return diffDays >= 0 && diffDays <= 2
  }).sort((a, b) => new Date(a.dueDate) - new Date(b.dueDate))
})

const unassignedTasks = computed(() => {
  return allTasks.value.filter(task => {
    const s = (task.statusName || '').toLowerCase().trim()
    const isCompleted = doneStatuses.includes(s) || cancelStatuses.includes(s)
    if (isCompleted) return false
    return !task.assignees || task.assignees.length === 0
  })
})

const stuckTasks = computed(() => {
  const twoWeeksAgo = new Date()
  twoWeeksAgo.setDate(twoWeeksAgo.getDate() - 14)
  
  return allTasks.value.filter(task => {
    const s = (task.statusName || '').toLowerCase().trim()
    const isCompleted = doneStatuses.includes(s) || cancelStatuses.includes(s)
    if (isCompleted) return false
    
    const taskDate = new Date(task.updatedAt || task.createdAt)
    return taskDate < twoWeeksAgo
  })
})

// 3. Members remind workload list
const membersToRemind = computed(() => {
  const membersMap = {}
  
  allTasks.value.forEach(task => {
    const s = (task.statusName || '').toLowerCase().trim()
    const isCompleted = doneStatuses.includes(s) || cancelStatuses.includes(s)
    if (isCompleted) return
    
    const taskAssignees = task.assignees || []
    
    taskAssignees.forEach(assignee => {
      const uid = assignee.userId || assignee.id
      if (!uid) return
      
      if (!membersMap[uid]) {
        const name = assignee.fullName || assignee.name || t('reports.member')
        membersMap[uid] = {
          userId: uid,
          fullName: name,
          avatar: assignee.initials || name.substring(0, 1).toUpperCase(),
          avatarColor: assignee.avatarColor || assignee.AvatarColor || '#3b82f6',
          avatarUrl: assignee.avatarUrl || assignee.AvatarUrl || null,
          pendingCount: 0,
          overdueCount: 0,
          nearestDeadlineTask: null,
          tasks: []
        }
      }
      
      const memberData = membersMap[uid]
      memberData.pendingCount++
      memberData.tasks.push(task)
      
      const isOverdue = task.dueDate && task.dueDate < todayStr
      if (isOverdue) {
        memberData.overdueCount++
      }
      
      if (task.dueDate) {
        if (!memberData.nearestDeadlineTask || task.dueDate < memberData.nearestDeadlineTask.dueDate) {
          memberData.nearestDeadlineTask = {
            id: task.id,
            title: task.title,
            dueDate: task.dueDate
          }
        }
      }
    })
  })
  
  return Object.values(membersMap).sort((a, b) => b.overdueCount - a.overdueCount || b.pendingCount - a.pendingCount)
})

// 4. Trigger reminder logic (P1)
const triggerReminder = async (task) => {
  const assignees = task.assignees || []
  if (assignees.length === 0) {
    ElMessage.warning(t('reports.remindUnassignedError'))
    return
  }

  sendingReminders.value[task.id] = true
  const currentUser = getStoredUser()
  const actorName = currentUser?.fullName || currentUser?.username || t('reports.manager')
  let succeedCount = 0
  let skippedCount = 0
  let hasFailed = false
  let errorMsg = t('reports.remindError')

  for (const assignee of assignees) {
    const uid = assignee.userId || assignee.id
    if (!uid) continue
    
    const payload = {
      projectId: projectId.value,
      taskId: task.id,
      assigneeUserId: uid,
      projectName: 'SprintA',
      taskTitle: task.title,
      actorName: actorName
    }

    try {
      const res = await axiosClient.post('/notifications/events/task-reminded', payload)
      if (res.data && res.data.skipped) {
        skippedCount++
      } else if (res.data && res.data.data && res.data.data.notificationId) {
        succeedCount++
      }
    } catch (err) {
      console.error('Failed to send reminder via backend', err)
      hasFailed = true
      if (err.response?.data?.message) {
        errorMsg = err.response.data.message
      } else if (err.response?.data) {
        errorMsg = typeof err.response.data === 'string' ? err.response.data : errorMsg
      }
    }
  }

  if (succeedCount > 0) {
    ElMessage.success(t('reports.remindSuccess'))
  }
  if (skippedCount > 0 && succeedCount === 0) {
    ElMessage.warning(t('reports.remindSelfWarning'))
  }
  if (hasFailed) {
    ElMessage.error(errorMsg)
  }
  sendingReminders.value[task.id] = false
}

const triggerMemberReminder = async (member) => {
  sendingMemberReminders.value[member.userId] = true
  const currentUser = getStoredUser()
  const actorName = currentUser?.fullName || currentUser?.username || t('reports.manager')
  const pendingTasks = member.tasks || []
  
  if (pendingTasks.length === 0) {
    ElMessage.info(t('reports.memberNoPending'))
    sendingMemberReminders.value[member.userId] = false
    return
  }
  
  const taskToRemind = pendingTasks.find(t => t.dueDate && t.dueDate < todayStr) || pendingTasks[0]

  const payload = {
    projectId: projectId.value,
    taskId: taskToRemind.id,
    assigneeUserId: member.userId,
    projectName: 'SprintA',
    taskTitle: taskToRemind.title,
    actorName: actorName
  }

  try {
    const res = await axiosClient.post('/notifications/events/task-reminded', payload)
    if (res.data && res.data.skipped) {
      ElMessage.warning(t('reports.remindSelfWarning'))
    } else if (res.data && res.data.data && res.data.data.notificationId) {
      ElMessage.success(t('reports.memberRemindSuccess', { name: member.fullName }))
    }
  } catch (err) {
    console.error('Failed to remind member via backend', err)
    let errorMsg = t('reports.remindError')
    if (err.response?.data?.message) {
      errorMsg = err.response.data.message
    } else if (err.response?.data) {
      errorMsg = typeof err.response.data === 'string' ? err.response.data : errorMsg
    }
    ElMessage.error(errorMsg)
  } finally {
    sendingMemberReminders.value[member.userId] = false
  }
}

// Charts data computations
const statusDistribution = computed(() => {
  if (allTasks.value.length === 0) return []
  
  const statusCounts = {}
  allTasks.value.forEach(task => {
    const s = (task.statusName || t('reports.noStatus')).trim()
    statusCounts[s] = (statusCounts[s] || 0) + 1
  })

  return Object.entries(statusCounts).map(([name, count]) => ({
    name,
    count,
    percentage: Math.round((count / allTasks.value.length) * 100)
  })).sort((a, b) => b.count - a.count)
})

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

const getStatusLabel = (statusName) => {
  if (!statusName) return t('reports.statusNotCreated')
  const norm = statusName.toLowerCase().trim()
  const keyMap = {
    'backlog': t('workItems.statusLabels.backlog'),
    'to do': t('workItems.statusLabels.toDo'),
    'in progress': t('workItems.statusLabels.inProgress'),
    'in review': t('workItems.statusLabels.inReview'),
    'done': t('workItems.statusLabels.done'),
    'cancelled': t('workItems.statusLabels.cancelled')
  }
  return keyMap[norm] || statusName
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

const getAssigneeNames = (task) => {
  if (!task.assignees || task.assignees.length === 0) return t('reports.unassigned')
  return task.assignees.map(a => a.fullName || a.name || t('reports.anonymous')).join(', ')
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
    error.value = t('reports.loadError')
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
  max-width: none;
  margin: 0 auto;
  padding: 28px clamp(24px, 3.2vw, 56px) 48px;
  min-height: calc(100vh - 64px);
  color: var(--color-text-primary);
  display: flex;
  flex-direction: column;
  gap: 28px;
  font-family: 'Inter', system-ui, sans-serif;
  background: var(--color-bg);
}

.reports-error {
  color: var(--color-danger);
  background: var(--color-danger-bg);
  border: 1px solid rgba(239, 68, 68, 0.2);
  border-radius: 12px;
  padding: 24px;
  text-align: center;
}

/* Content Area */
.reports-content {
  display: grid;
  grid-template-columns: repeat(12, minmax(0, 1fr));
  gap: 24px;
}

.reports-content > .health-alert-card,
.reports-content > .reports-stats-grid { grid-column: 1 / -1; }
.reports-content > .attention-panel,
.reports-content > .workload-panel { grid-column: span 7; }
.reports-content > .report-card { grid-column: span 5; }

/* Project Health Alert Card styling */
.health-alert-card {
  display: flex;
  align-items: flex-start;
  gap: 16px;
  padding: 20px 24px;
  border-radius: 16px;
  box-shadow: 0 10px 30px rgba(15, 23, 42, 0.05);
  border-width: 1px;
  border-style: solid;
}

.health-alert-card.green {
  background: linear-gradient(135deg, rgba(16, 185, 129, 0.08), rgba(52, 211, 153, 0.03));
  border-color: rgba(16, 185, 129, 0.28);
  box-shadow: 0 12px 30px rgba(16, 185, 129, 0.04);
}
.health-alert-card.green .health-icon {
  background: rgba(16, 185, 129, 0.12);
  color: #10b981;
}

.health-alert-card.yellow {
  background: linear-gradient(135deg, rgba(245, 158, 11, 0.08), rgba(251, 191, 36, 0.03));
  border-color: rgba(245, 158, 11, 0.28);
  box-shadow: 0 12px 30px rgba(245, 158, 11, 0.04);
}
.health-alert-card.yellow .health-icon {
  background: rgba(245, 158, 11, 0.12);
  color: #f59e0b;
}

.health-alert-card.red {
  background: linear-gradient(135deg, rgba(239, 68, 68, 0.08), rgba(248, 113, 113, 0.03));
  border-color: rgba(239, 68, 68, 0.28);
  box-shadow: 0 12px 30px rgba(239, 68, 68, 0.04);
}
.health-alert-card.red .health-icon {
  background: rgba(239, 68, 68, 0.12);
  color: #ef4444;
}

.health-icon {
  width: 44px;
  height: 44px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 20px;
  flex-shrink: 0;
}

.health-status-title {
  margin: 0;
  font-size: 16px;
  font-weight: 750;
  color: var(--color-text-primary);
}

.health-desc {
  margin: 6px 0 0 0;
  font-size: 13.5px;
  color: var(--color-text-secondary);
  line-height: 1.5;
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
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 16px !important;
  padding: 20px;
  position: relative;
  overflow: hidden;
  box-shadow: var(--shadow-sm);
}

.report-stat-card:hover {
  transform: translateY(-2px);
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
}

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
  display: flex;
  align-items: baseline;
  gap: 8px;
}

.percentage-tag {
  font-size: 13px;
  font-weight: 700;
  color: #10b981;
  background: rgba(16, 185, 129, 0.08);
  padding: 2px 6px;
  border-radius: 6px;
}

/* Card Design Pattern */
.report-card {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 14px !important;
  padding: 24px 26px;
  box-shadow: var(--shadow-sm);
  display: flex;
  flex-direction: column;
  min-width: 0;
}

.report-card:hover {
  box-shadow: 0 24px 58px rgba(15, 23, 42, 0.10);
}

.card-title {
  font-size: 17px;
  font-weight: 750;
  color: var(--color-text-primary);
  margin-bottom: 20px;
  display: flex;
  align-items: center;
  gap: 10px;
  line-height: 1.3;
  padding-bottom: 12px;
  border-bottom: 1px solid var(--color-border);
}

/* Grid Layouts for cards */
.distributions-grid {
  display: grid;
  grid-template-columns: minmax(0, 1fr) minmax(300px, 360px);
  gap: 24px;
  align-items: start;
  grid-column: 1 / -1;
}

.dashboard-left-panel,
.dashboard-right-panel {
  display: flex;
  flex-direction: column;
  gap: 24px;
  min-width: 0;
}

@media (max-width: 1180px) {
  .distributions-grid {
    grid-template-columns: 1fr;
  }
}

/* Accordion list styles */
.attention-accordions {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.accordion-item {
  border: 1px solid var(--color-border);
  border-radius: 12px;
  overflow: hidden;
  background: var(--color-surface);
  transition: border-color 0.2s ease;
}

.accordion-item:hover {
  border-color: #cbd5e1;
}

.accordion-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 14px 18px;
  cursor: pointer;
  background: rgba(0, 0, 0, 0.015);
  user-select: none;
}

.header-left {
  display: flex;
  align-items: center;
  gap: 12px;
  min-width: 0;
}

.header-text {
  font-size: 13.5px;
  font-weight: 700;
  color: var(--color-text-primary);
  overflow-wrap: anywhere;
}

.badge {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 22px;
  height: 22px;
  border-radius: 50%;
  font-size: 11px;
  font-weight: 800;
  padding: 0 6px;
}

.danger-bg { background: rgba(239, 68, 68, 0.08); color: #ef4444; }
.warning-bg { background: rgba(245, 158, 11, 0.08); color: #f59e0b; }
.info-bg { background: rgba(14, 165, 233, 0.08); color: #0ea5e9; }
.gray-bg { background: rgba(100, 116, 139, 0.08); color: #64748b; }

.accordion-content {
  border-top: 1px solid var(--color-border);
  padding: 12px;
  background: var(--color-surface);
}

.empty-substate {
  padding: 16px;
  text-align: center;
  color: var(--color-text-muted);
  font-size: 12.5px;
}

.attention-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.attention-task-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 10px 14px;
  background: rgba(0, 0, 0, 0.01);
  border-radius: 8px;
  border: 1px solid transparent;
  transition: all 0.2s ease;
}

.attention-task-row:hover {
  background: var(--color-surface-hover);
  border-color: var(--color-border);
}

.task-info {
  display: flex;
  flex-direction: column;
  gap: 4px;
  flex: 1;
  cursor: pointer;
  overflow: hidden;
  margin-right: 12px;
}

.task-key {
  font-size: 10px;
  font-family: monospace;
  font-weight: 700;
  color: var(--color-text-muted);
}

.task-title {
  font-size: 13px;
  font-weight: 650;
  color: var(--color-text-primary);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.task-info:hover .task-title {
  color: var(--color-accent);
  text-decoration: underline;
}

.task-assignee,
.task-due-date,
.task-status {
  font-size: 11px;
  color: var(--color-text-muted);
}

.remind-btn {
  background: var(--color-accent);
  color: white;
  border: none;
  border-radius: 6px;
  padding: 6px 10px;
  font-size: 11.5px;
  font-weight: 700;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 4px;
  box-shadow: 0 4px 10px rgba(14, 165, 233, 0.2);
  transition: all 0.2s ease;
}

.remind-btn:hover {
  background: var(--color-accent-hover, #0284c7);
  transform: translateY(-1px);
}

.remind-btn:disabled {
  background: var(--color-text-muted, #94a3b8) !important;
  color: rgba(255, 255, 255, 0.6) !important;
  cursor: not-allowed !important;
  box-shadow: none !important;
  transform: none !important;
  opacity: 0.6 !important;
}

.plain-row {
  cursor: pointer;
}

.unassigned-badge {
  font-size: 11px;
  color: #64748b;
  background: #f1f5f9;
  padding: 4px 8px;
  border-radius: 6px;
  display: flex;
  align-items: center;
  gap: 4px;
}

/* Remind Members card workload design */
.remind-member-card {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px;
  border-radius: 12px;
  background: rgba(0, 0, 0, 0.01);
  border: 1px solid var(--color-border);
  transition: all 0.2s ease;
}

.remind-member-card:hover {
  background: var(--color-surface-hover);
  transform: translateX(2px);
}

.remind-member-card.has-overdue {
  border-color: rgba(239, 68, 68, 0.15);
  background: rgba(239, 68, 68, 0.015);
}
.remind-member-card.has-overdue:hover {
  border-color: rgba(239, 68, 68, 0.3);
  background: rgba(239, 68, 68, 0.03);
}

.member-card-left {
  display: flex;
  align-items: flex-start;
  gap: 14px;
  flex: 1;
  min-width: 0;
}

.member-details {
  display: flex;
  flex-direction: column;
  gap: 6px;
  min-width: 0;
}

.member-name {
  font-size: 14px;
  font-weight: 700;
  color: var(--color-text-primary);
}

.member-stats {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
}

.member-stats .badge {
  border-radius: 6px;
  padding: 2px 8px;
  height: auto;
  font-size: 11px;
}

.deadline-tip {
  margin: 4px 0 0 0;
  font-size: 11.5px;
  color: var(--color-text-muted);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  max-width: 320px;
}

.hover-task-link {
  color: var(--color-text-secondary);
  cursor: pointer;
}
.hover-task-link:hover {
  color: var(--color-accent);
  text-decoration: underline;
}

.remind-member-btn {
  background: linear-gradient(135deg, #10b981, #059669);
  color: white;
  border: none;
  border-radius: 8px;
  padding: 8px 14px;
  font-size: 12.5px;
  font-weight: 700;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 6px;
  box-shadow: 0 4px 12px rgba(16, 185, 129, 0.2);
  transition: all 0.2s ease;
}

.remind-member-btn:hover {
  background: linear-gradient(135deg, #059669, #047857);
  transform: translateY(-1px);
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

/* Animations and Dark Theme adjustments */
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
  background: var(--color-bg) !important;
}

.health-alert-card,
.report-stat-card,
.report-card {
  animation: reports-rise-in 520ms cubic-bezier(0.2, 0.8, 0.2, 1) both;
  transition:
    transform 220ms cubic-bezier(0.2, 0.8, 0.2, 1),
    box-shadow 220ms ease,
    border-color 220ms ease !important;
}

.reports-stats-grid .report-stat-card:nth-child(1) { animation-delay: 70ms; }
.reports-stats-grid .report-stat-card:nth-child(2) { animation-delay: 120ms; }
.reports-stats-grid .report-stat-card:nth-child(3) { animation-delay: 170ms; }
.reports-stats-grid .report-stat-card:nth-child(4) { animation-delay: 220ms; }

[data-theme='dark'] .health-alert-card,
[data-theme='dark'] .report-stat-card,
[data-theme='dark'] .report-card {
  border-color: var(--color-border);
  background: var(--color-surface) !important;
  box-shadow: var(--shadow-sm);
}

[data-theme='light'] .health-alert-card,
[data-theme='light'] .report-stat-card,
[data-theme='light'] .report-card {
  background: var(--color-surface) !important;
  color: var(--color-text-primary) !important;
  border-color: var(--color-border) !important;
}

[data-theme='light'] .card-title,
[data-theme='light'] .report-stat-card .value,
[data-theme='light'] .status-count strong,
[data-theme='light'] .legend-count,
[data-theme='light'] .donut-number,
[data-theme='light'] .member-name {
  color: #0f172a !important;
}

[data-theme='dark'] .card-title,
[data-theme='dark'] .report-stat-card .value,
[data-theme='dark'] .status-count strong,
[data-theme='dark'] .legend-count,
[data-theme='dark'] .donut-number,
[data-theme='dark'] .member-name {
  color: #f8fafc !important;
}

@media (max-width: 900px) {
  .reports-content > .attention-panel,
  .reports-content > .workload-panel,
  .reports-content > .report-card { grid-column: 1 / -1; }
}
</style>
