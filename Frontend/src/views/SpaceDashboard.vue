<template>
  <NexusLayout>
    <div class="space-dashboard-page p-6">
      <header class="mb-6 flex justify-between items-center">
        <div>
          <h1 class="text-2xl font-bold text-[var(--color-text-primary)]">{{ t('Dashboard') }}</h1>
          <p class="text-[var(--color-text-muted)] text-sm mt-1">{{ t('Project overview and quick insights') }}</p>
        </div>
      </header>
      
      <div v-if="loading" class="text-center py-12 text-[var(--color-text-muted)]">
        <i class="fa-solid fa-spinner fa-spin text-2xl"></i>
      </div>

      <div v-else class="dashboard-grid">
        <div class="stat-card">
          <div class="stat-icon bg-blue-500/10 text-blue-500"><i class="fa-solid fa-list-check"></i></div>
          <div class="stat-info">
            <span class="stat-value">12</span>
            <span class="stat-label">{{ t('Open Tasks') }}</span>
          </div>
        </div>
        <div class="stat-card">
          <div class="stat-icon bg-green-500/10 text-green-500"><i class="fa-solid fa-check-double"></i></div>
          <div class="stat-info">
            <span class="stat-value">45</span>
            <span class="stat-label">{{ t('Completed') }}</span>
          </div>
        </div>
        <div class="stat-card">
          <div class="stat-icon bg-yellow-500/10 text-yellow-500"><i class="fa-solid fa-clock-rotate-left"></i></div>
          <div class="stat-info">
            <span class="stat-value">3</span>
            <span class="stat-label">{{ t('In Progress') }}</span>
          </div>
        </div>
        <div class="stat-card">
          <div class="stat-icon bg-red-500/10 text-red-500"><i class="fa-solid fa-triangle-exclamation"></i></div>
          <div class="stat-info">
            <span class="stat-value">2</span>
            <span class="stat-label">{{ t('Blocked') }}</span>
          </div>
        </div>
        
        <div class="dashboard-panel col-span-2">
          <h3 class="font-semibold text-lg mb-4">{{ t('Recent Tasks') }}</h3>
          <div class="empty-state">
            <i class="fa-solid fa-inbox text-3xl mb-3 text-[var(--color-text-muted)]"></i>
            <h4 class="text-[var(--color-text-primary)] font-medium">{{ t('No recent tasks') }}</h4>
            <p class="text-[var(--color-text-secondary)] text-sm mt-1">{{ t('Get started by creating a new task in your board or backlog.') }}</p>
          </div>
        </div>

        <div class="dashboard-panel col-span-2">
          <h3 class="font-semibold text-lg mb-4">{{ t('Team Workload') }}</h3>
          <div class="empty-state">
            <i class="fa-solid fa-users text-3xl mb-3 text-[var(--color-text-muted)]"></i>
            <h4 class="text-[var(--color-text-primary)] font-medium">{{ t('Workload distribution') }}</h4>
            <p class="text-[var(--color-text-secondary)] text-sm mt-1">{{ t('Assign tasks to your team members to see their workload here.') }}</p>
          </div>
        </div>
      </div>
    </div>
  </NexusLayout>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import NexusLayout from '@/components/layout/NexusLayout.vue'
import { useI18nStore } from '@/store/useI18nStore'

const i18nStore = useI18nStore()
const t = (key) => i18nStore.t(key)

const loading = ref(false)

onMounted(() => {
  loading.value = true
  setTimeout(() => {
    loading.value = false
  }, 500)
})
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
