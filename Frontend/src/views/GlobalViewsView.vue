<script setup>
import { ref, onMounted, computed, watch } from 'vue'
import axiosClient from '@/api/axiosClient'

import FilterBar from '@/components/FilterBar.vue'

const rawTasks = ref([])
const loading = ref(false)
const showFilters = ref(false)
const displayProps = ref(['ID', 'Assignee', 'Start date', 'Due date', 'Labels', 'Priority', 'State'])
const groupBy = ref('States')
const orderBy = ref('Manual')
const showSubItems = ref(false)
const activeFilters = ref([])

const analyticsScope = ref('all') // all, my, archived
const selectedProjectId = ref(null)
const projectList = ref([])

const filters = ref({
  status: '',
  search: ''
})

const getTaskFieldValue = (task, field) => {
  const fieldMap = {
    status: task.statusName || task.state || task.status,
    priority: task.priorityName || task.priority,
    assignee: task.assigneeName || task.assignedToName || task.assignees?.map(a => a.fullName || a.name).join(', '),
    creator: task.reporterName || task.createdByName || task.creatorName,
    label: task.labels?.map(l => l.name || l.labelName).join(', '),
    startDate: task.plannedStartDate || task.startDate,
    dueDate: task.dueDate || task.plannedEndDate,
    cycle: task.sprintName || task.cycleName,
    module: task.moduleName,
    createdAt: task.createdAt || task.createdDate,
    updatedAt: task.updatedAt || task.updatedDate
  }
  return fieldMap[field] ?? task[field]
}

const normalizeValue = (value) => `${value ?? ''}`.toLowerCase()

const applyTaskFilters = (items, filterList) => {
  if (!filterList?.length) return items

  return items.filter(task => filterList.every(filter => {
    const actual = getTaskFieldValue(task, filter.field)
    const expected = filter.value
    const operator = filter.operator || filter.condition || 'is'
    const actualText = normalizeValue(actual)
    const expectedText = normalizeValue(expected)
    const actualDate = actual ? new Date(actual).getTime() : null
    const expectedDate = expected ? new Date(expected).getTime() : null

    if (operator === 'empty') return !actual
    if (operator === 'not empty') return Boolean(actual)
    if (operator === 'overdue') return actualDate && actualDate < Date.now()
    if (operator === 'before') return actualDate && expectedDate && actualDate < expectedDate
    if (operator === 'after') return actualDate && expectedDate && actualDate > expectedDate
    if (operator === 'between') {
      const endDate = filter.valueTo ? new Date(filter.valueTo).getTime() : null
      return actualDate && expectedDate && endDate && actualDate >= expectedDate && actualDate <= endDate
    }
    if (operator === 'is not' || operator === 'is_not') return actualText !== expectedText
    if (operator === 'includes') return actualText.includes(expectedText)
    if (operator === 'not includes' || operator === 'not_includes') return !actualText.includes(expectedText)
    if (operator === 'in') return expectedText.split(',').map(v => v.trim()).includes(actualText)
    if (operator === 'not in' || operator === 'not_in') return !expectedText.split(',').map(v => v.trim()).includes(actualText)
    return actualText === expectedText || actualText.includes(expectedText)
  }))
}

const visibleColumns = computed(() => new Set(displayProps.value))
const showColumn = (name) => visibleColumns.value.has(name)

const toggleDisplayProp = (prop) => {
  const idx = displayProps.value.indexOf(prop)
  if (idx > -1) displayProps.value.splice(idx, 1)
  else displayProps.value.push(prop)
}

const handleRemoveFilter = (id) => {
  activeFilters.value = activeFilters.value.filter(f => f.id !== id)
}

const handleClearFilters = () => {
  activeFilters.value = []
}

const handleAddFilter = (filter) => {
  if (!filter?.id) return
  activeFilters.value.push(filter)
}

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
  let nextTasks = [...rawTasks.value]

  if (!showSubItems.value) {
    nextTasks = nextTasks.filter(task => !(task.parentTaskId || task.parentId))
  }

  nextTasks = applyTaskFilters(nextTasks, activeFilters.value)

  if (orderBy.value === 'Last created') {
    nextTasks.sort((a, b) => new Date(b.createdAt || 0) - new Date(a.createdAt || 0))
  } else if (orderBy.value === 'Last updated') {
    nextTasks.sort((a, b) => new Date(b.updatedAt || 0) - new Date(a.updatedAt || 0))
  } else if (orderBy.value === 'Start date') {
    nextTasks.sort((a, b) => new Date(a.startDate || a.plannedStartDate || 0) - new Date(b.startDate || b.plannedStartDate || 0))
  } else if (orderBy.value === 'Due date') {
    nextTasks.sort((a, b) => new Date(a.dueDate || a.plannedEndDate || 0) - new Date(b.dueDate || b.plannedEndDate || 0))
  } else if (orderBy.value === 'Priority') {
    nextTasks.sort((a, b) => (a.priority ?? 999) - (b.priority ?? 999))
  }

  return nextTasks
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
  <div>
    <div class="views-wrapper">
      <!-- Header -->
      <header class="vh-header">
        <div class="vh-left">
           <span class="breadcrumb"><i class="fa-solid fa-layer-group"></i> Views <i class="fa-solid fa-chevron-right separator"></i> All work items <i class="fa-solid fa-chevron-down ms-2 text-[10px]"></i></span>
        </div>
        
        <!-- Filter Controls -->
        <div class="analytics-filters">
            <div class="scope-selector">
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

            <div class="project-selector-wrap">
                <select 
                    v-model="selectedProjectId"
                    class="vh-select"
                    :disabled="analyticsScope === 'all'"
                >
                    <option :value="null">Filter by project</option>
                    <option v-for="p in filteredProjects" :key="p.id" :value="p.id">
                        {{ p.name }}
                    </option>
                </select>
            </div>
        </div>

        <div class="vh-right">
           <input type="text" v-model="filters.search" placeholder="Search tasks..." class="vh-input" />
           <select v-model="filters.status" class="vh-select">
              <option value="">All Status</option>
              <option value="BACKLOG">Backlog</option>
              <option value="TO DO">To Do</option>
              <option value="IN PROGRESS">In Progress</option>
              <option value="DONE">Done</option>
           </select>
           <button class="plane-toolbar-btn" @click="showFilters = !showFilters"><i class="fa-solid fa-filter"></i></button>
           <el-dropdown trigger="click" popper-class="display-popper-final" placement="bottom-end" :hide-on-click="false" :z-index="5000">
             <button class="plane-toolbar-btn">Display</button>
             <template #dropdown>
               <div class="display-scroll-vfinal">
                 <div class="st-content">
                   <div class="st-sect">
                     <div class="st-sect-header"><span>Display Properties</span><i class="fa-solid fa-chevron-up"></i></div>
                     <div class="st-chips">
                       <span v-for="p in ['ID', 'Assignee', 'Start date', 'Due date', 'Labels', 'Priority', 'State', 'Module', 'Cycle']"
                             :key="p" class="p-chip-st" :class="{ selected: displayProps.includes(p) }" @click.stop="toggleDisplayProp(p)">{{ p }}</span>
                     </div>
                   </div>
                   <div class="st-sect">
                     <div class="st-sect-header"><span>Group by</span><i class="fa-solid fa-chevron-up"></i></div>
                     <div class="st-radios">
                       <label class="st-opt" v-for="g in ['States', 'Priority', 'Cycle', 'Module', 'Labels', 'Assignees', 'Created by', 'None']" :key="g">
                         <input type="radio" name="global-groupby" :value="g" v-model="groupBy" />
                         <span class="st-dot"></span><span class="st-label">{{ g }}</span>
                       </label>
                     </div>
                   </div>
                   <div class="st-sect">
                     <div class="st-sect-header"><span>Order by</span><i class="fa-solid fa-chevron-up"></i></div>
                     <div class="st-radios">
                       <label class="st-opt" v-for="o in ['Manual', 'Last created', 'Last updated', 'Start date', 'Due date', 'Priority']" :key="o">
                         <input type="radio" name="global-orderby" :value="o" v-model="orderBy" />
                         <span class="st-dot"></span><span class="st-label">{{ o }}</span>
                       </label>
                     </div>
                   </div>
                   <div class="divider"></div>
                   <div class="st-foot">
                     <label class="st-check">
                       <input type="checkbox" v-model="showSubItems" />
                       <span class="checkmark"></span>
                       <span class="st-label">Show sub-work items</span>
                     </label>
                   </div>
                 </div>
               </div>
             </template>
           </el-dropdown>
        </div>
      </header>

      <div v-if="showFilters" class="global-filter-bar">
        <FilterBar
          :filters="activeFilters"
          @remove="handleRemoveFilter"
          @clear="handleClearFilters"
          @add="handleAddFilter"
        />
      </div>
      
      <!-- Table content -->
      <div class="spreadsheet-container">
        <table class="plane-table">
          <thead>
            <tr>
              <th style="width: 25%;" v-if="showColumn('ID')">Work items</th>
              <th v-if="showColumn('State')"><i class="fa-regular fa-circle-dot"></i> State <i class="fa-solid fa-chevron-down f-10"></i></th>
              <th v-if="showColumn('Priority')"><i class="fa-solid fa-signal"></i> Priority <i class="fa-solid fa-chevron-down f-10"></i></th>
              <th v-if="showColumn('Assignee')"><i class="fa-solid fa-user-group"></i> Assignees <i class="fa-solid fa-chevron-down f-10"></i></th>
              <th v-if="showColumn('Labels')"><i class="fa-solid fa-tag"></i> Labels <i class="fa-solid fa-chevron-down f-10"></i></th>
              <th v-if="showColumn('Module')"><i class="fa-solid fa-table-cells-large"></i> Modules <i class="fa-solid fa-chevron-down f-10"></i></th>
              <th v-if="showColumn('Cycle')"><i class="fa-solid fa-arrows-spin"></i> Cycle <i class="fa-solid fa-chevron-down f-10"></i></th>
              <th v-if="showColumn('Start date')"><i class="fa-regular fa-calendar"></i> Start date <i class="fa-solid fa-chevron-down f-10"></i></th>
              <th v-if="showColumn('Due date')"><i class="fa-solid fa-calendar-day"></i> Due date <i class="fa-solid fa-chevron-down f-10"></i></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="t in filteredTasks" :key="t.id">
              <td v-if="showColumn('ID')">
                <div class="wi-cell">
                  <span class="wi-id">{{ t.sequenceId || t.id.substring(0,8).toUpperCase() }}</span>
                  <span class="wi-title">{{ t.title }}</span>
                </div>
              </td>
              <td v-if="showColumn('State')">
                <div class="state-cell">
                  <i :class="getStatusIcon(t.statusName || 'BACKLOG').class"></i>
                  <span>{{ t.statusName || 'BACKLOG' }}</span>
                </div>
              </td>
              <td v-if="showColumn('Priority')">
                <div class="prio-cell">
                  <i class="fa-solid fa-signal" v-if="t.priority === 3" style="color: #F59E0B"></i>
                  <i class="fa-solid fa-ban text-muted" v-else></i>
                  <span>{{ t.priority === 3 ? 'High' : 'None' }}</span>
                </div>
              </td>
              <td v-if="showColumn('Assignee')">
                <div class="assignee-cell">
                  <i class="fa-regular fa-user" v-if="!t.assigneeName"></i>
                  <span class="text-muted" v-if="!t.assigneeName">Assignees</span>
                  <span class="d-dot" style="background: #0EA5E9; padding: 2px 6px; color: white; border-radius: 4px; font-size: 10px;" v-else>{{ t.assigneeName.substring(0,2).toUpperCase() }}</span>
                </div>
              </td>
              <td v-if="showColumn('Labels')">
                <div class="label-cell text-muted">
                  <i class="fa-solid fa-tag"></i> Select labels
                </div>
              </td>
              <td v-if="showColumn('Module')">
                <div class="module-cell text-muted">
                  <i class="fa-solid fa-table-cells-large"></i> 0 Modules
                </div>
              </td>
              <td v-if="showColumn('Cycle')">
                <div class="cycle-cell text-muted">
                  <i class="fa-solid fa-arrows-spin"></i> No Cycle
                </div>
              </td>
              <td v-if="showColumn('Start date')" class="text-muted"><i class="fa-regular fa-calendar"></i> {{ t.plannedStartDate || t.startDate || 'Start date' }}</td>
              <td v-if="showColumn('Due date')" class="text-muted"><i class="fa-solid fa-calendar-day"></i> {{ t.dueDate || t.plannedEndDate || 'Due date' }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
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
  border-bottom: 1px solid var(--border-color);
  background: var(--bg-secondary);
}
.breadcrumb {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 14px;
  font-weight: 500;
  color: var(--text-secondary);
}
.separator { font-size: 10px; color: var(--color-text-muted); padding-right: 8px; margin-right: 8px; }
.ms-2 { margin-left: 8px; }

.vh-right {
  display: flex;
  gap: 12px;
  align-items: center;
}
.plane-toolbar-btn {
  background: transparent;
  border: none;
  color: var(--text-secondary);
  font-size: 13px;
  font-weight: 500;
  cursor: pointer;
  padding: 6px 12px;
  border-radius: 6px;
  transition: all 0.2s;
  display: flex;
  align-items: center;
  gap: 6px;
}
.plane-toolbar-btn:hover { background: var(--hover-bg); color: var(--text-primary); }
.global-filter-bar {
  padding: 12px 24px 0;
}

.analytics-filters {
  display: flex;
  gap: 8px;
  align-items: center;
  background: var(--bg-secondary);
  padding: 4px 12px;
  border-radius: 8px;
  border: 1px solid var(--border-color);
  margin: 0 16px;
}
.scope-selector {
  display: flex;
  background: var(--bg-primary);
  border-radius: 6px;
  padding: 2px;
}
.scope-btn {
  background: transparent;
  border: none;
  color: var(--text-secondary);
  padding: 4px 12px;
  font-size: 12px;
  font-weight: 500;
  cursor: pointer;
  border-radius: 4px;
  transition: all 0.2s;
}
.scope-btn:hover { color: var(--text-primary); background: var(--hover-bg); }
.scope-btn.active {
  background: var(--bg-tertiary);
  color: var(--text-primary);
  box-shadow: var(--shadow-sm);
}

.vh-input, .vh-select {
  background: var(--bg-tertiary);
  border: 1px solid var(--border-color);
  color: var(--text-primary);
  padding: 4px 12px;
  border-radius: 4px;
  font-size: 13px;
  outline: none;
  transition: all 0.2s;
}
.vh-select { min-width: 140px; cursor: pointer; }
.vh-select option { background: var(--bg-secondary); color: var(--text-primary); }

.vh-input:focus, .vh-select:focus {
  border-color: var(--accent-color);
  box-shadow: 0 0 0 2px rgba(14, 165, 233, 0.2);
}

.project-selector-wrap { position: relative; }
.project-selector-wrap select:disabled { opacity: 0.5; cursor: not-allowed; }

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
  color: var(--text-secondary);
  border-bottom: 2px solid var(--border-color);
  border-right: 1px solid var(--border-color);
  background: var(--bg-secondary);
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

.display-scroll-vfinal {
  width: 330px;
  background: var(--bg-secondary);
  border-radius: 12px;
  border: 1px solid var(--border-color);
  max-height: 520px;
  overflow-y: auto;
  overflow-x: hidden;
  box-shadow: var(--shadow-xl);
}
.display-scroll-vfinal::-webkit-scrollbar { width: 5px; }
.display-scroll-vfinal::-webkit-scrollbar-thumb { background: var(--border-color); border-radius: 10px; }
.st-content { padding: 20px; padding-bottom: 30px; }
.st-sect { margin-bottom: 24px; }
.st-sect-header { display: flex; justify-content: space-between; align-items: center; font-size: 11px; font-weight: 700; color: var(--color-text-muted); margin-bottom: 12px; }
.st-chips { display: flex; flex-wrap: wrap; gap: 8px; }
.p-chip-st { padding: 6px 10px; background: var(--color-bg); border-radius: 6px; font-size: 12px; color: var(--color-text-secondary); cursor: pointer; border: 1px solid var(--color-border); }
.p-chip-st.selected { background: var(--color-accent); color: var(--color-text-inverse); border-color: var(--color-accent); }
.st-radios { display: flex; flex-direction: column; gap: 11px; }
.st-opt { display: flex; align-items: center; gap: 12px; cursor: pointer; }
.st-opt input { display: none; }
.st-dot { width: 14px; height: 14px; border-radius: 50%; border: 1.5px solid var(--color-border); position: relative; }
.st-opt input:checked + .st-dot { border-color: var(--color-accent); }
.st-opt input:checked + .st-dot::after {
  content: "\f00c"; font-family: "Font Awesome 6 Free"; font-weight: 900; font-size: 8px; color: var(--color-accent);
  position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%);
}
.st-label { font-size: 13px; color: var(--text-secondary); }
.divider { height: 1px; background: var(--border-color); margin: 16px 0; }
.st-check { display: flex; align-items: center; gap: 10px; cursor: pointer; }
.st-check input { display: none; }
.checkmark { width: 15px; height: 15px; border: 1.5px solid #3F3F46; border-radius: 4px; position: relative; }
.st-check input:checked + .checkmark { background: #0EA5E9; border-color: #0EA5E9; }
.st-check input:checked + .checkmark::after {
  content: ""; position: absolute; left: 4px; top: 1px; width: 4px; height: 8px; border: solid white; border-width: 0 1.5px 1.5px 0; transform: rotate(45deg);
}
:deep(.display-popper-final) { background: transparent !important; border: none !important; box-shadow: none !important; z-index: 10000 !important; }
</style>



