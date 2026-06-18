<template>
  <div class="audit-log-container">
    <div class="page-header">
      <h1 class="page-title">{{ t('Audit Log') }}</h1>
      <p class="page-subtitle">Track organizational changes and activity history across all sites and projects.</p>
    </div>

    <div class="log-controls">
      <div class="search-box">
        <Search class="w-4 h-4"></Search>
        <input type="text" placeholder="Filter activities..." v-model="searchQuery" />
      </div>
      <select class="filter-select" v-model="filterType">
        <option value="all">All Events</option>
        <option value="create">Created</option>
        <option value="archive">Archived</option>
        <option value="restore">Restored</option>
      </select>
    </div>

    <div class="timeline-container">
      <div class="timeline-item" v-for="log in filteredLogs" :key="log.id">
        <div class="timeline-icon" :class="log.actionClass">
          <i :class="log.icon"></i>
        </div>
        <div class="timeline-content">
          <div class="timeline-header">
            <strong>{{ log.actor }}</strong> {{ log.action }} <strong>{{ log.target }}</strong>
          </div>
          <div class="timeline-meta">
            <span class="meta-type">{{ log.type }}</span>
            <span class="meta-time">{{ log.time }}</span>
          </div>
        </div>
      </div>

      <div v-if="filteredLogs.length === 0" class="empty-state">
        <ClipboardList class="w-4 h-4 empty-icon"></ClipboardList>
        <h3>No activities found</h3>
        <p>Try adjusting your filters to find what you're looking for.</p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { Search, ClipboardList } from 'lucide-vue-next';
import { ref, computed } from 'vue'
import { useI18nStore } from '@/store/useI18nStore'

const i18nStore = useI18nStore()
const t = i18nStore.t

const searchQuery = ref('')
const filterType = ref('all')

const logs = ref([
  { id: 1, actor: 'Tua Nguyen', action: 'created project', target: 'SprintA Redesign', type: 'Project', time: '10 minutes ago', actionClass: 'bg-green-100 text-green-600', icon: 'fa-solid fa-plus', category: 'create' },
  { id: 2, actor: 'Tua Nguyen', action: 'archived team', target: 'Legacy Devs', type: 'Team', time: '2 hours ago', actionClass: 'bg-orange-100 text-orange-600', icon: 'fa-solid fa-box-archive', category: 'archive' },
  { id: 3, actor: 'System Admin', action: 'created goal', target: 'Q3 Revenue Target', type: 'Goal', time: '1 day ago', actionClass: 'bg-green-100 text-green-600', icon: 'fa-solid fa-plus', category: 'create' },
  { id: 4, actor: 'John Doe', action: 'restored project', target: 'Old Marketing', type: 'Project', time: '2 days ago', actionClass: 'bg-blue-100 text-blue-600', icon: 'fa-solid fa-rotate-left', category: 'restore' },
  { id: 5, actor: 'Jane Smith', action: 'created team', target: 'Design Ops', type: 'Team', time: '3 days ago', actionClass: 'bg-green-100 text-green-600', icon: 'fa-solid fa-plus', category: 'create' }
])

const filteredLogs = computed(() => {
  return logs.value.filter(log => {
    const matchesSearch = log.actor.toLowerCase().includes(searchQuery.value.toLowerCase()) || 
                          log.target.toLowerCase().includes(searchQuery.value.toLowerCase())
    const matchesFilter = filterType.value === 'all' || log.category === filterType.value
    return matchesSearch && matchesFilter
  })
})
</script>

<style scoped>
.audit-log-container {
  padding: 32px 40px;
  max-width: 900px;
  margin: 0 auto;
}

.page-header {
  margin-bottom: 24px;
}

.page-title {
  font-size: 24px;
  font-weight: 600;
  color: #172b4d;
  margin: 0 0 8px 0;
}

.page-subtitle {
  color: #5e6c84;
  margin: 0;
  font-size: 14px;
}

.log-controls {
  display: flex;
  gap: 16px;
  margin-bottom: 32px;
}

.search-box {
  flex: 1;
  position: relative;
  display: flex;
  align-items: center;
}

.search-box i {
  position: absolute;
  left: 12px;
  color: #6b778c;
}

.search-box input {
  width: 100%;
  padding: 8px 12px 8px 36px;
  border: 2px solid #dfe1e6;
  border-radius: 3px;
  font-size: 14px;
  outline: none;
  transition: border-color 0.2s;
}

.search-box input:focus {
  border-color: #4c9aff;
}

.filter-select {
  padding: 8px 12px;
  border: 2px solid #dfe1e6;
  border-radius: 3px;
  background-color: #fafbfc;
  color: #172b4d;
  font-size: 14px;
  outline: none;
  cursor: pointer;
}

.timeline-container {
  display: flex;
  flex-direction: column;
  gap: 24px;
  position: relative;
}

.timeline-container::before {
  content: '';
  position: absolute;
  top: 10px;
  bottom: 10px;
  left: 15px;
  width: 2px;
  background-color: #dfe1e6;
  z-index: 0;
}

.timeline-item {
  display: flex;
  gap: 16px;
  position: relative;
  z-index: 1;
}

.timeline-icon {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  border: 4px solid #ffffff;
  box-shadow: 0 1px 2px rgba(0,0,0,0.1);
}

.timeline-content {
  background: #ffffff;
  border: 1px solid #dfe1e6;
  border-radius: 3px;
  padding: 12px 16px;
  flex: 1;
  box-shadow: 0 1px 1px rgba(9, 30, 66, 0.05);
}

.timeline-header {
  font-size: 14px;
  color: #172b4d;
  margin-bottom: 4px;
}

.timeline-meta {
  display: flex;
  align-items: center;
  gap: 12px;
  font-size: 12px;
  color: #6b778c;
}

.meta-type {
  background-color: #ebecf0;
  padding: 2px 6px;
  border-radius: 3px;
  font-weight: 500;
}

.empty-state {
  text-align: center;
  padding: 48px 20px;
  background-color: #fafbfc;
  border: 1px dashed #dfe1e6;
  border-radius: 3px;
}

.empty-icon {
  font-size: 48px;
  color: #b3bac5;
  margin-bottom: 16px;
}

.empty-state h3 {
  margin: 0 0 8px 0;
  color: #172b4d;
}

.empty-state p {
  margin: 0;
  color: #5e6c84;
}

/* Utilities for icons */
.bg-green-100 { background-color: #e3fcef; }
.text-green-600 { color: #006644; }
.bg-orange-100 { background-color: #ffebe6; }
.text-orange-600 { color: #bf2600; }
.bg-blue-100 { background-color: #deebff; }
.text-blue-600 { color: #0747a6; }
</style>
