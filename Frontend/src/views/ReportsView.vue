<template>
  <NexusLayout>
    <div class="space-reports-page p-6">
      <header class="mb-6 flex justify-between items-center">
        <div>
          <h1 class="text-2xl font-bold text-[var(--color-text-primary)]">{{ t('Reports') }}</h1>
          <p class="text-[var(--color-text-muted)] text-sm mt-1">{{ t('Analytics and insights for this project') }}</p>
        </div>
        <div class="flex gap-2">
          <button class="plane-btn-secondary" @click="fetchData"><i class="fa-solid fa-rotate-right mr-2"></i> {{ t('Refresh') }}</button>
          <button class="plane-btn-primary"><i class="fa-solid fa-download mr-2"></i> {{ t('Export') }}</button>
        </div>
      </header>
      
      <div v-if="loading" class="text-center py-12 text-[var(--color-text-muted)]">
        <i class="fa-solid fa-spinner fa-spin text-2xl"></i>
      </div>
      
      <div v-else-if="error" class="text-center py-12 text-red-500">
        {{ error }}
      </div>

      <div v-else class="reports-grid">
        <div class="empty-state-container col-span-full">
          <div class="empty-state">
            <i class="fa-solid fa-chart-line text-5xl mb-4 text-[var(--color-text-muted)]"></i>
            <h3 class="text-xl font-semibold text-[var(--color-text-primary)] mb-2">{{ t('Reports are being generated') }}</h3>
            <p class="text-[var(--color-text-secondary)] mb-6 max-w-md mx-auto">{{ t('We are collecting data for your project. Comprehensive analytics including burndown charts and status distributions will appear here once enough data is available.') }}</p>
            <button class="plane-btn-secondary" disabled>{{ t('Check back later') }}</button>
          </div>
        </div>
      </div>
    </div>
  </NexusLayout>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import NexusLayout from '@/components/layout/NexusLayout.vue'
import { useI18nStore } from '@/store/useI18nStore'

const i18nStore = useI18nStore()
const t = (key) => i18nStore.t(key)

const route = useRoute()
const loading = ref(false)
const error = ref(null)

const fetchData = async () => {
  loading.value = true
  try {
    // Simulate API fetch
    await new Promise(resolve => setTimeout(resolve, 800))
  } catch (e) {
    error.value = "Failed to load reports"
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  fetchData()
})
</script>

<style scoped>
.reports-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(400px, 1fr));
  gap: 24px;
}
.col-span-full {
  grid-column: 1 / -1;
}
.report-card {
  background: var(--color-bg);
  border: 1px solid var(--color-border);
  border-radius: 8px;
  padding: 20px;
}
.empty-state-container {
  display: flex;
  justify-content: center;
  align-items: center;
  padding: 60px 20px;
}
.empty-state {
  text-align: center;
  background: var(--color-surface);
  border: 1px dashed var(--color-border);
  border-radius: 8px;
  padding: 40px;
  max-width: 600px;
  width: 100%;
}
</style>
