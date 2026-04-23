<script setup>
import { ref, onMounted, computed } from 'vue'
import axiosClient from '@/api/axiosClient'
import NexusLayout from '@/components/layout/NexusLayout.vue'

const rawTasks = ref([])
const loading = ref(false)
const showFilters = ref(false)

const analyticsScope = ref('all') // all, my, archived
const selectedProjectId = ref(null)
const projectList = ref([])

const filters = ref({
  status: '',
  search: ''
})

const fetchProjects = async () => {
    try {
        const [discoveryRes, archivedRes] = await Promise.all([
            axiosClient.get('/projects/discovery'),
            axiosClient.get('/projects/archived')
        ])
        
        const activeProjects = (discoveryRes.data?.data || []).map(p => ({ ...p, isArchived: false }))
        const archivedProjects = (archivedRes.data?.data || []).map(p => ({ ...p, isArchived: true }))
        
        projectList.value = [...activeProjects, ...archivedProjects]
    } catch(e) {
        console.error('Error fetching projects', e)
    }
}

const fetchTasks = async () => {
  try {
    loading.value = true
    const params = {
      scope: analyticsScope.value,
      query: filters.value.search,
      status: filters.value.status
    }
    if (selectedProjectId.value) {
      params.projectId = selectedProjectId.value
    }
    const res = await axiosClient.get('/tasks/search', { params })
    rawTasks.value = res.data?.data || []
  } catch (err) {
    console.error('Failed to load global tasks', err)
  } finally {
    loading.value = false
  }
}

onMounted(async () => {
  await fetchProjects()
  fetchTasks()
})

watch([analyticsScope, selectedProjectId, () => filters.value.status, () => filters.value.search], () => {
  fetchTasks()
})

const filteredProjects = computed(() => {
    if (analyticsScope.value === 'all') return projectList.value
    if (analyticsScope.value === 'my') return projectList.value.filter(p => p.isMember)
    if (analyticsScope.value === 'archived') return projectList.value.filter(p => p.isArchived)
    return projectList.value
})

const filteredTasks = computed(() => {
  return rawTasks.value
})

const getStatusIcon = (st) => {
  if (st === 'Done') return { class: 'fa-solid fa-circle-check text-green', color: '#10B981' }
  if (st === 'In Progress') return { class: 'fa-solid fa-circle-half-stroke text-orange', color: '#F59E0B' }
  if (st === 'Todo') return { class: 'fa-regular fa-circle text-muted', color: '#71717A' }
  return { class: 'fa-solid fa-circle-dashed text-muted', color: '#71717A' } // Backlog
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
        
        <!-- Filter Controls -->
        <div class="analytics-filters" style="display: flex; gap: 8px; align-items: center; background: #111315; padding: 4px 12px; border-radius: 8px; border: 1px solid #1E2025; margin: 0 16px;">
            <div class="scope-selector" style="display: flex; background: #0D0F11; border-radius: 6px; padding: 2px;">
                <button 
                    @click="analyticsScope = 'all'; selectedProjectId = null"
                    :class="['scope-btn', { active: analyticsScope === 'all' }]"
                >All projects</button>
                <button 
                    @click="analyticsScope = 'my'; selectedProjectId = null"
                    :class="['scope-btn', { active: analyticsScope === 'my' }]"
                >My projects</button>
                <button 
                    @click="analyticsScope = 'archived'; selectedProjectId = null"
                    :class="['scope-btn', { active: analyticsScope === 'archived' }]"
                >Archived projects</button>
            </div>

            <div class="project-selector-wrap" style="position: relative;">
                <select 
                    v-model="selectedProjectId"
                    style="background: transparent; border: 1px solid #27272A; color: #E4E4E7; padding: 4px 12px; border-radius: 4px; font-size: 13px; min-width: 160px; outline: none; cursor: pointer;"
                    :disabled="analyticsScope === 'all'"
                    :style="{ opacity: analyticsScope === 'all' ? 0.5 : 1 }"
                >
                    <option :value="null" style="background: #1E2025;">Filter by project</option>
                    <option v-for="p in filteredProjects" :key="p.id" :value="p.id" style="background: #1E2025;">
                        {{ p.name }}
                    </option>
                </select>
            </div>
        </div>

        <div class="vh-right" style="display: flex; gap: 8px; align-items: center;">
           <input type="text" v-model="filters.search" placeholder="Search tasks..." style="background: transparent; border: 1px solid #27272A; color: #E4E4E7; padding: 4px 8px; border-radius: 4px; font-size: 13px;" />
           <select v-model="filters.status" style="background: transparent; border: 1px solid #27272A; color: #E4E4E7; padding: 4px 8px; border-radius: 4px; font-size: 13px;">
              <option value="" style="background: #1E2025;">All Status</option>
              <option value="BACKLOG" style="background: #1E2025;">Backlog</option>
              <option value="TO DO" style="background: #1E2025;">To Do</option>
              <option value="IN PROGRESS" style="background: #1E2025;">In Progress</option>
              <option value="DONE" style="background: #1E2025;">Done</option>
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
  background: #0D0F11;
  color: #E4E4E7;
}
.vh-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 24px;
  border-bottom: 1px solid #1E2025;
}
.breadcrumb {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 14px;
  font-weight: 500;
  color: #A1A1AA;
}
.separator { font-size: 10px; color: #71717A; border-right: 1px solid #27272A; padding-right: 8px; margin-right: 8px; }
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
.plane-toolbar-btn:hover { background: #1E2025; }

.scope-btn {
  background: transparent;
  border: none;
  color: #A1A1AA;
  padding: 4px 12px;
  font-size: 12px;
  font-weight: 500;
  cursor: pointer;
  border-radius: 4px;
  transition: all 0.2s;
}
.scope-btn:hover { color: #E4E4E7; }
.scope-btn.active {
  background: #1E2025;
  color: #E4E4E7;
  box-shadow: 0 1px 3px rgba(0,0,0,0.3);
}

.project-selector-wrap select:focus {
  border-color: #3B82F6 !important;
  box-shadow: 0 0 0 1px #3B82F6 !important;
}
.project-selector-wrap select:hover:not(:disabled) {
  border-color: #3F3F46;
}

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
  color: #A1A1AA;
  border-bottom: 2px solid #1E2025;
  border-right: 1px solid #1E2025;
  background: #0D0F11;
  position: sticky;
  top: 0;
  z-index: 10;
  white-space: nowrap;
}
.plane-table th i.f-10 { font-size: 9px; float: right; margin-top: 4px; opacity: 0.5; }
.plane-table th i:not(.f-10) { margin-right: 6px; }

.plane-table td {
  padding: 8px 16px;
  border-bottom: 1px solid #1E2025;
  border-right: 1px solid #1E2025;
  white-space: nowrap;
}
.plane-table tr:hover { background: #16181D; }

.wi-cell { display: flex; align-items: center; gap: 16px; }
.wi-id { color: #A1A1AA; min-width: 45px; }
.wi-title { color: #E4E4E7; font-weight: 500; }

.state-cell, .prio-cell { display: flex; align-items: center; gap: 8px; color: #E4E4E7; }
.text-green { color: #10B981; }
.text-orange { color: #F59E0B; }
.text-red { color: #EF4444; }
.text-blue { color: #3B82F6; }
.text-muted { color: #71717A; }

.assignee-cell, .label-cell, .module-cell, .cycle-cell { display: flex; align-items: center; gap: 6px; }
.d-dot { width: 6px; height: 6px; border-radius: 50%; background: #9333EA; display: inline-block; }
td i { width: 14px; text-align: center; }
</style>
