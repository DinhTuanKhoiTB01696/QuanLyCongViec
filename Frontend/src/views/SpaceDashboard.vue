<template>
  <NexusLayout>
    <div class="space-dashboard-page p-6">
      <header class="mb-6 flex justify-between items-center">
        <div>
          <h1 class="text-2xl font-bold text-[var(--color-text-primary)]">{{ t('spaceDashboard.title') }}</h1>
          <p class="text-[var(--color-text-muted)] text-sm mt-1">{{ t('spaceDashboard.subtitle') }}</p>
        </div>
      </header>

      <div v-if="loading" class="text-center py-12 text-[var(--color-text-muted)]">
        <i class="fa-solid fa-spinner fa-spin text-2xl"></i>
      </div>

      <div v-else-if="loadError" class="text-center py-12 text-[var(--color-text-muted)]">
        <i class="fa-solid fa-triangle-exclamation text-2xl mb-2"></i>
        <p>{{ t('spaceDashboard.loadFailed') }}</p>
      </div>

      <div v-else class="dashboard-grid">
        <div class="stat-card">
          <div class="stat-icon bg-blue-500/10 text-blue-500"><i class="fa-solid fa-list-check"></i></div>
          <div class="stat-info">
            <span class="stat-value">{{ openTasksCount }}</span>
            <span class="stat-label">{{ t('spaceDashboard.openTasks') }}</span>
          </div>
        </div>
        <div class="stat-card">
          <div class="stat-icon bg-green-500/10 text-green-500"><i class="fa-solid fa-check-double"></i></div>
          <div class="stat-info">
            <span class="stat-value">{{ completedCount }}</span>
            <span class="stat-label">{{ t('spaceDashboard.completed') }}</span>
          </div>
        </div>
        <div class="stat-card">
          <div class="stat-icon bg-yellow-500/10 text-yellow-500"><i class="fa-solid fa-clock-rotate-left"></i></div>
          <div class="stat-info">
            <span class="stat-value">{{ inProgressCount }}</span>
            <span class="stat-label">{{ t('spaceDashboard.inProgress') }}</span>
          </div>
        </div>
        <div class="stat-card">
          <div class="stat-icon bg-red-500/10 text-red-500"><i class="fa-solid fa-triangle-exclamation"></i></div>
          <div class="stat-info">
            <span class="stat-value">{{ blockedCount }}</span>
            <span class="stat-label">{{ t('spaceDashboard.blocked') }}</span>
          </div>
        </div>

        <div class="dashboard-panel col-span-2">
          <h3 class="font-semibold text-lg mb-4">{{ t('spaceDashboard.recentTasks') }}</h3>
          <div v-if="recentTasks.length === 0" class="empty-state">
            <i class="fa-solid fa-inbox text-3xl mb-3 text-[var(--color-text-muted)]"></i>
            <h4 class="text-[var(--color-text-primary)] font-medium">{{ t('spaceDashboard.noRecentTasks') }}</h4>
            <p class="text-[var(--color-text-secondary)] text-sm mt-1">{{ t('spaceDashboard.noRecentTasksDesc') }}</p>
          </div>
          <ul v-else class="dashboard-list">
            <li v-for="task in recentTasks" :key="task.id" class="dashboard-list-item">
              <span class="item-main" :title="task.title">{{ task.title }}</span>
              <span class="item-meta">{{ task.statusName }}</span>
            </li>
          </ul>
        </div>

        <div class="dashboard-panel col-span-2">
          <h3 class="font-semibold text-lg mb-4">{{ t('spaceDashboard.teamWorkload') }}</h3>
          <div v-if="workload.length === 0" class="empty-state">
            <i class="fa-solid fa-users text-3xl mb-3 text-[var(--color-text-muted)]"></i>
            <h4 class="text-[var(--color-text-primary)] font-medium">{{ t('spaceDashboard.workloadDistribution') }}</h4>
            <p class="text-[var(--color-text-secondary)] text-sm mt-1">{{ t('spaceDashboard.noWorkloadDesc') }}</p>
          </div>
          <ul v-else class="dashboard-list">
            <li v-for="member in workload" :key="member.id" class="dashboard-list-item">
              <span class="item-main" :title="member.name">{{ member.name }}</span>
              <span class="item-meta">{{ t('spaceDashboard.tasksCount', { count: member.count }) }}</span>
            </li>
          </ul>
        </div>
      </div>
    </div>
  </NexusLayout>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute } from 'vue-router'
import axiosClient from '@/api/axiosClient'
import NexusLayout from '@/components/layout/NexusLayout.vue'
import { useI18n } from '@/composables/useI18n'

const route = useRoute()
const { t } = useI18n()

const loading = ref(false)
const loadError = ref(false)
const tasks = ref([])
const members = ref([])

// /space/:id is project-scoped (sidebar builds /space/{projectId}/...).
const projectId = computed(() => route.params.id || null)

const statusUpper = (task) => (task.statusName || '').toUpperCase()
const isDone = (task) => statusUpper(task).includes('DONE') || statusUpper(task).includes('COMPLETE')
const isCancelled = (task) => statusUpper(task).includes('CANCEL')
const isInProgress = (task) => statusUpper(task).includes('PROGRESS')
// Backend hiện không có status "BLOCKED"; chỉ tính nếu dữ liệu thật có cờ/giá trị này -> thường là 0 (không bịa).
const isBlocked = (task) => statusUpper(task).includes('BLOCK') || task.isBlocked === true

const openTasksCount = computed(() =>
  tasks.value.filter(task => !isDone(task) && !isCancelled(task)).length
)
const completedCount = computed(() => tasks.value.filter(isDone).length)
const inProgressCount = computed(() => tasks.value.filter(isInProgress).length)
const blockedCount = computed(() => tasks.value.filter(isBlocked).length)

const recentTasks = computed(() =>
  [...tasks.value]
    .sort((a, b) => new Date(b.updatedAt || b.createdAt || 0) - new Date(a.updatedAt || a.createdAt || 0))
    .slice(0, 5)
)

const workload = computed(() =>
  members.value
    .map(member => {
      const uid = member.userId || member.id
      const count = tasks.value.filter(task =>
        task.assignedUserId === uid || (task.assignees || []).some(a => (a.userId || a.id) === uid)
      ).length
      return { id: uid, name: member.fullName || member.name || member.email, count }
    })
    .sort((a, b) => b.count - a.count)
)

const fetchDashboard = async () => {
  if (!projectId.value) {
    tasks.value = []
    members.value = []
    return
  }
  loading.value = true
  loadError.value = false
  try {
    const [tasksRes, membersRes] = await Promise.all([
      axiosClient.get(`/projects/${projectId.value}/WorkTasks`),
      axiosClient.get(`/projects/${projectId.value}/members`).catch(() => ({ data: { data: [] } }))
    ])
    const rawTasks = tasksRes.data?.data || []
    tasks.value = rawTasks.filter(task => !task.projectId || task.projectId === projectId.value)
    members.value = membersRes.data?.data || []
  } catch (error) {
    loadError.value = true
    tasks.value = []
    members.value = []
    console.error('Failed to load space dashboard:', error)
  } finally {
    loading.value = false
  }
}

onMounted(fetchDashboard)
watch(projectId, fetchDashboard)
</script>

<style scoped>
.dashboard-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 24px;
}
.col-span-2 {
  grid-column: span 2;
}
.stat-card {
  background: var(--color-bg);
  border: 1px solid var(--color-border);
  border-radius: 8px;
  padding: 20px;
  display: flex;
  align-items: center;
  gap: 16px;
}
.stat-icon {
  width: 48px;
  height: 48px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 20px;
}
.stat-info {
  display: flex;
  flex-direction: column;
}
.stat-value {
  font-size: 24px;
  font-weight: 700;
  line-height: 1.2;
}
.stat-label {
  font-size: 13px;
  color: var(--color-text-muted);
}
.dashboard-panel {
  background: var(--color-bg);
  border: 1px solid var(--color-border);
  border-radius: 8px;
  padding: 20px;
}
.dashboard-list {
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
}
.dashboard-list-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  padding: 12px 4px;
  border-bottom: 1px solid var(--color-border);
}
.dashboard-list-item:last-child {
  border-bottom: none;
}
.item-main {
  font-size: 14px;
  color: var(--color-text-primary);
  font-weight: 500;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
.item-meta {
  font-size: 12px;
  color: var(--color-text-muted);
  flex-shrink: 0;
}
@media (max-width: 1024px) {
  .dashboard-grid {
    grid-template-columns: repeat(2, 1fr);
  }
}
@media (max-width: 640px) {
  .dashboard-grid {
    grid-template-columns: 1fr;
  }
  .col-span-2 {
    grid-column: span 1;
  }
}
.empty-state {
  text-align: center;
  background: var(--color-surface);
  border: 1px dashed var(--color-border);
  border-radius: 8px;
  padding: 32px;
}
</style>
