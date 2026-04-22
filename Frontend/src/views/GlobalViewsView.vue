<script setup>
import { ref, onMounted, computed } from 'vue'
import axiosClient from '@/api/axiosClient'
import NexusLayout from '@/components/layout/NexusLayout.vue'

const rawTasks = ref([])
const loading = ref(false)
const showFilters = ref(false)

const filters = ref({
  status: '',
  search: ''
})

onMounted(async () => {
  try {
    loading.value = true
    const res = await axiosClient.get('/tasks/search')
    rawTasks.value = res.data?.data || []
  } catch (err) {
    console.error('Failed to load global tasks', err)
  } finally {
    loading.value = false
  }
})

const filteredTasks = computed(() => {
  let list = rawTasks.value
  if (filters.value.status) {
    list = list.filter(t => (t.statusName || 'BACKLOG').toUpperCase().trim() === filters.value.status)
  }
  if (filters.value.search) {
    list = list.filter(t => t.title.toLowerCase().includes(filters.value.search.toLowerCase()) || 
                      (t.sequenceId && t.sequenceId.toLowerCase().includes(filters.value.search.toLowerCase())))
  }
  return list
})

const getStatusIcon = (st) => {
  if (st === 'Done') return { class: 'fa-solid fa-circle-check text-green', color: '#10B981' }
  if (st === 'In Progress') return { class: 'fa-solid fa-circle-half-stroke text-orange', color: '#F59E0B' }
  if (st === 'Todo') return { class: 'fa-regular fa-circle text-muted', color: 'var(--color-text-muted)' }
  return { class: 'fa-solid fa-circle-dashed text-muted', color: 'var(--color-text-muted)' } // Backlog
}

const getPrioIcon = (pr) => {
  if (pr === 'Urgent') return { class: 'fa-solid fa-angles-up text-red' }
  if (pr === 'High') return { class: 'fa-solid fa-chevron-up text-orange' }
  if (pr === 'Low') return { class: 'fa-solid fa-chevron-down text-blue' }
  return { class: 'fa-solid fa-ban text-muted' }
}
</script>

<template>
  <NexusLayout>
    <div class="views-wrapper">
      <!-- Header -->
      <header class="vh-header">
        <div class="vh-left">
           <span class="breadcrumb"><i class="fa-solid fa-layer-group"></i> Views <i class="fa-solid fa-chevron-right separator"></i> All work items <i class="fa-solid fa-chevron-down ms-2" style="font-size: 10px;"></i></span>
        </div>
        <div class="vh-right" style="display: flex; gap: 8px; align-items: center;">
           <input type="text" v-model="filters.search" placeholder="Search tasks..." style="background: transparent; border: 1px solid var(--color-border); color: var(--color-text-primary); padding: 4px 8px; border-radius: 4px; font-size: 13px;" />
           <select v-model="filters.status" style="background: transparent; border: 1px solid var(--color-border); color: var(--color-text-primary); padding: 4px 8px; border-radius: 4px; font-size: 13px;">
              <option value="" style="background: var(--color-border);">All Status</option>
              <option value="BACKLOG" style="background: var(--color-border);">Backlog</option>
              <option value="TO DO" style="background: var(--color-border);">To Do</option>
              <option value="IN PROGRESS" style="background: var(--color-border);">In Progress</option>
              <option value="DONE" style="background: var(--color-border);">Done</option>
           </select>
           <button class="plane-toolbar-btn" @click="showFilters = !showFilters"><i class="fa-solid fa-filter"></i></button>
           <button class="plane-toolbar-btn">Display</button>
        </div>
      </header>
      
      <!-- Table content -->
      <div class="spreadsheet-container">
        <table class="plane-table">
          <thead>
            <tr>
              <th style="width: 25%;">Work items</th>
              <th><i class="fa-regular fa-circle-dot"></i> State <i class="fa-solid fa-chevron-down f-10"></i></th>
              <th><i class="fa-solid fa-signal"></i> Priority <i class="fa-solid fa-chevron-down f-10"></i></th>
              <th><i class="fa-solid fa-user-group"></i> Assignees <i class="fa-solid fa-chevron-down f-10"></i></th>
              <th><i class="fa-solid fa-tag"></i> Labels <i class="fa-solid fa-chevron-down f-10"></i></th>
              <th><i class="fa-solid fa-table-cells-large"></i> Modules <i class="fa-solid fa-chevron-down f-10"></i></th>
              <th><i class="fa-solid fa-arrows-spin"></i> Cycle <i class="fa-solid fa-chevron-down f-10"></i></th>
              <th><i class="fa-regular fa-calendar"></i> Start date <i class="fa-solid fa-chevron-down f-10"></i></th>
              <th><i class="fa-solid fa-calendar-day"></i> Due date <i class="fa-solid fa-chevron-down f-10"></i></th>
              <th><i class="fa-solid fa-triangle-exclamation"></i> Estimate <i class="fa-solid fa-chevron-down f-10"></i></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="t in filteredTasks" :key="t.id">
              <td>
                <div class="wi-cell">
                  <span class="wi-id">{{ t.sequenceId || t.id.substring(0,8).toUpperCase() }}</span>
                  <span class="wi-title">{{ t.title }}</span>
                </div>
              </td>
              <td>
                <div class="state-cell">
                  <i :class="getStatusIcon(t.statusName || 'BACKLOG').class"></i>
                  <span>{{ t.statusName || 'BACKLOG' }}</span>
                </div>
              </td>
              <td>
                <div class="prio-cell">
                  <i class="fa-solid fa-signal" v-if="t.priority === 3" style="color: #F59E0B"></i>
                  <i class="fa-solid fa-ban text-muted" v-else></i>
                  <span>{{ t.priority === 3 ? 'High' : 'None' }}</span>
                </div>
              </td>
              <td>
                <div class="assignee-cell">
                  <i class="fa-regular fa-user" v-if="!t.assigneeName"></i>
                  <span class="text-muted" v-if="!t.assigneeName">Assignees</span>
                  <span class="d-dot" style="background: #0EA5E9; padding: 2px 6px; color: white; border-radius: 4px; font-size: 10px;" v-else>{{ t.assigneeName.substring(0,2).toUpperCase() }}</span>
                </div>
              </td>
              <td>
                <div class="label-cell text-muted">
                  <i class="fa-solid fa-tag"></i> Select labels
                </div>
              </td>
              <td>
                <div class="module-cell text-muted">
                  <i class="fa-solid fa-table-cells-large"></i> 0 Modules
                </div>
              </td>
              <td>
                <div class="cycle-cell text-muted">
                  <i class="fa-solid fa-arrows-spin"></i> No Cycle
                </div>
              </td>
              <td class="text-muted"><i class="fa-regular fa-calendar"></i> Start date</td>
              <td class="text-muted"><i class="fa-solid fa-calendar-day"></i> Due date</td>
              <td class="text-muted"><i class="fa-solid fa-caret-up"></i> Estimate</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </NexusLayout>
</template>

<style scoped>
.views-wrapper {
  display: flex;
  flex-direction: column;
  height: 100vh;
  background: var(--color-bg);
  color: var(--color-text-primary);
}
.vh-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 24px;
  border-bottom: 1px solid var(--color-border);
}
.breadcrumb {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 14px;
  font-weight: 500;
  color: var(--color-text-muted);
}
.separator { font-size: 10px; color: var(--color-text-muted); border-right: 1px solid var(--color-border); padding-right: 8px; margin-right: 8px; }
.ms-2 { margin-left: 8px; }

.vh-right {
  display: flex;
  gap: 12px;
}
.plane-toolbar-btn {
  background: transparent;
  border: none;
  color: #D4D4D8;
  font-size: 13px;
  font-weight: 500;
  cursor: pointer;
  padding: 6px 12px;
  border-radius: 6px;
  transition: background 0.2s;
  display: flex;
  align-items: center;
  gap: 6px;
}
.plane-toolbar-btn:hover { background: var(--color-border); }

.plane-primary-btn {
  background: #0EA5E9;
  color: white;
  border: none;
  border-radius: 6px;
  padding: 6px 12px;
  font-size: 13px;
  font-weight: 500;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 6px;
  transition: background 0.2s;
}
.plane-primary-btn:hover { background: #0284C7; }

/* Spreadsheet styles */
.spreadsheet-container {
  flex: 1;
  overflow: auto;
}
.plane-table {
  width: 100%;
  border-collapse: collapse;
  text-align: left;
  font-size: 12px;
}

.plane-table th {
  padding: 12px 16px;
  font-weight: 500;
  color: var(--color-text-muted);
  border-bottom: 2px solid var(--color-border);
  border-right: 1px solid var(--color-border);
  background: var(--color-bg);
  position: sticky;
  top: 0;
  z-index: 10;
  white-space: nowrap;
}
.plane-table th i.f-10 { font-size: 9px; float: right; margin-top: 4px; opacity: 0.5; }
.plane-table th i:not(.f-10) { margin-right: 6px; }

.plane-table td {
  padding: 8px 16px;
  border-bottom: 1px solid var(--color-border);
  border-right: 1px solid var(--color-border);
  white-space: nowrap;
}
.plane-table tr:hover { background: var(--color-surface); }

.wi-cell { display: flex; align-items: center; gap: 16px; }
.wi-id { color: var(--color-text-muted); min-width: 45px; }
.wi-title { color: var(--color-text-primary); font-weight: 500; }

.state-cell, .prio-cell { display: flex; align-items: center; gap: 8px; color: var(--color-text-primary); }
.text-green { color: #10B981; }
.text-orange { color: #F59E0B; }
.text-red { color: #EF4444; }
.text-blue { color: #3B82F6; }
.text-muted { color: var(--color-text-muted); }

.assignee-cell, .label-cell, .module-cell, .cycle-cell { display: flex; align-items: center; gap: 6px; }
.d-dot { width: 6px; height: 6px; border-radius: 50%; background: #9333EA; display: inline-block; }
td i { width: 14px; text-align: center; }
</style>



