<script setup>
import { ref, onMounted, computed, watch } from 'vue'
import { useRoute } from 'vue-router'
import axiosClient from '@/api/axiosClient'
import { ElNotification, ElMessageBox } from 'element-plus'
import ListView from '@/components/ListView.vue'

import FilterBar from '@/components/FilterBar.vue'

const route = useRoute()
const projectId = computed(() => route.params.id || localStorage.getItem('currentProjectId') || 'default')

const views = ref([])
const activeView = ref(null)
const tasks = ref([])
const loading = ref(false)
const showCreateModal = ref(false)
const viewType = ref('list') 

// Selected Filters State
const activeFilters = ref([])
const showViewSearch = ref(false)

const addFilterOption = (label, icon) => {
    // Check if we want to allow multiples (like Start date) or just one of a kind
    // For now, let's just add it with a unique ID
    activeFilters.value.push({
        id: Date.now(),
        label: label,
        condition: 'is',
        value: '--',
        icon: icon
    })
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

const applyTaskFilters = (items, filters) => {
    if (!filters?.length) return items

    return items.filter(task => filters.every(filter => {
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

// Sorting and Filtering
const sortBy = ref('Updated at')
const sortDir = ref('Descending')
const filterSearch = ref('')

// Creation form
const newView = ref({
  name: '',
  description: '',
  queryMetadata: '{}'
})

// Display Properties State
const displayProps = ref(['ID', 'Assignee', 'Start date', 'Due date', 'Labels', 'Priority', 'State'])
const groupBy = ref('States')
const orderBy = ref('Manual')
const showSubItems = ref(false)

const filteredViews = computed(() => {
  const q = filterSearch.value.trim().toLowerCase()
  if (!q) return views.value
  return views.value.filter(view =>
    `${view.name || ''} ${view.description || ''}`.toLowerCase().includes(q)
  )
})

const fetchViews = async () => {
  try {
    const res = await axiosClient.get(`/projects/${projectId.value}/views`)
    views.value = res.data.data
  } catch (err) {
    console.error('Failed to fetch views', err)
  }
}

const selectView = async (view) => {
  activeView.value = view
  await fetchViewTasks(view)
}

const fetchViewTasks = async (view) => {
  loading.value = true
  try {
    let priority = null
    let metadataFilters = []
    try {
        const metadata = JSON.parse(view.queryMetadata)
        if (metadata.priority === 'Urgent') priority = 1
        metadataFilters = metadata.filters || []
    } catch(e) {}

    const res = await axiosClient.get(`/tasks/search`, {
      params: { 
        priority: priority,
        projectId: projectId.value
      }
    })
    tasks.value = applyTaskFilters(res.data.data || [], metadataFilters)
  } catch (err) {
    console.error('Failed to fetch tasks', err)
  } finally {
    loading.value = false
  }
}

const createView = async () => {
  if (!newView.value.name) return
  try {
    const payload = {
      ...newView.value,
      queryMetadata: JSON.stringify({
        filters: activeFilters.value,
        displayProps: displayProps.value,
        groupBy: groupBy.value,
        orderBy: orderBy.value,
        showSubItems: showSubItems.value
      })
    }
    const res = await axiosClient.post(`/projects/${projectId.value}/views`, payload)
    views.value.push(res.data.data)
    ElNotification.success('View created successfully')
    showCreateModal.value = false
    resetForm()
  } catch (err) {
    ElNotification.error('Failed to create view')
  }
}

const resetForm = () => {
    newView.value = { name: '', description: '', queryMetadata: '{}' }
    activeFilters.value = []
}

const deleteView = async (id) => {
  try {
    await ElMessageBox.confirm('Are you sure you want to delete this view?', 'Warning', {
      type: 'warning',
      confirmButtonText: 'Delete',
      confirmButtonClass: 'el-button--danger'
    })
    await axiosClient.delete(`/projects/${projectId.value}/views/${id}`)
    views.value = views.value.filter(v => v.id !== id)
    if (activeView.value?.id === id) activeView.value = null
    ElNotification.success('View deleted')
  } catch (err) {
    if (err !== 'cancel') ElNotification.error('Failed to delete view')
  }
}

const toggleFavorite = async (view) => {
  try {
    const res = await axiosClient.patch(`/projects/${projectId.value}/views/${view.id}/favorite`)
    view.isFavorite = res.data.data.isFavorite
  } catch (err) {
    ElNotification.error('Failed to toggle favorite')
  }
}

const resetModal = () => {
  showCreateModal.value = false
  resetForm()
}

const goBackToList = () => {
    activeView.value = null
}

const toggleDisplayProp = (prop) => {
    const idx = displayProps.value.indexOf(prop)
    if (idx > -1) displayProps.value.splice(idx, 1)
    else displayProps.value.push(prop)
}

const viewTypeIcon = computed(() => {
    switch(viewType.value) {
        case 'list': return 'fa-solid fa-bars'
        case 'board': return 'fa-solid fa-columns'
        case 'calendar': return 'fa-regular fa-calendar'
        case 'table': return 'fa-solid fa-table'
        case 'timeline': return 'fa-solid fa-timeline'
        default: return 'fa-solid fa-bars'
    }
})

onMounted(() => {
  fetchViews()
})
</script>

<template>
  <div class="views-page">
    <!-- Top Header Navigation (Breadcrumbs) - Restored to Turn 9 layout -->
    <header class="app-header">
      <div class="header-left">
        <div class="breadcrumb-refined">
            <i class="fa-solid fa-certificate text-orange-400"></i>
            <span class="p-name">CYBWF</span>
            <i class="fa-solid fa-chevron-right sep"></i>
            <i class="fa-solid fa-layer-group text-slate-400"></i>
            <span class="v-name" @click="goBackToList">Views</span>
            <template v-if="activeView">
                <i class="fa-solid fa-chevron-right sep"></i>
                <span class="cur-view">{{ activeView.name }}</span>
            </template>
        </div>
      </div>

      <div class="header-right">
        <template v-if="!activeView">
            <button class="h-tool-btn" type="button" @click="showViewSearch = !showViewSearch"><i class="fa-solid fa-magnifying-glass"></i></button>
            <input v-if="showViewSearch" v-model="filterSearch" class="view-search-input" type="text" placeholder="Search views" />
            <el-dropdown trigger="click">
                <button class="h-tool-btn outlined" type="button">
                    <i class="fa-solid fa-arrow-down-short-wide mr-2"></i>
                    {{ sortBy }}
                </button>
                <template #dropdown>
                    <el-dropdown-menu class="dark-popover">
                        <el-dropdown-item @click="sortBy = 'Name'">Name</el-dropdown-item>
                        <el-dropdown-item @click="sortBy = 'Created at'">Created at</el-dropdown-item>
                        <el-dropdown-item @click="sortBy = 'Updated at'">Updated at</el-dropdown-item>
                    </el-dropdown-menu>
                </template>
            </el-dropdown>
            
            <button class="h-tool-btn outlined" type="button" @click="showCreateModal = true"><i class="fa-solid fa-filter mr-2"></i> Filters</button>
            <button class="add-view-primary" type="button" @click="showCreateModal = true">Add view</button>
        </template>
        <template v-else>
            <button class="h-tool-btn outlined" type="button" disabled title="Display settings are configured when creating the view"><i class="fa-solid fa-sliders mr-2"></i> Display</button>
        </template>
      </div>
    </header>

    <!-- Main Content Body - Restored to Turn 9 layout -->
    <main class="views-content">
      <div v-if="!activeView" class="views-list">
        <div v-if="views.length === 0" class="empty-placeholder">
          <p>No custom views here.</p>
        </div>
        
        <div class="view-item-row" v-for="view in filteredViews" :key="view.id" @click="selectView(view)">
            <div class="vi-left">
                <i class="fa-solid fa-layer-group vi-icon"></i>
                <span class="vi-name">{{ view.name }}</span>
            </div>
            <div class="vi-right">
                <i class="fa-solid fa-earth-americas vi-globe" v-if="view.isGlobal"></i>
                <div class="vi-avatar">P</div>
                <button class="vi-star" type="button" :class="{ active: view.isFavorite }" @click.stop="toggleFavorite(view)">
                    <i class="fa-regular fa-star"></i>
                </button>
                <button class="vi-more" type="button" @click.stop="deleteView(view.id)"><i class="fa-solid fa-ellipsis"></i></button>
            </div>
        </div>
      </div>

      <div v-else class="detail-container">
        <ListView :tasks="tasks" />
      </div>
    </main>

    <!-- Modal (ONLY REFORMING THE DISPLAY DROPDOWN) -->
    <div class="modal-overlay" v-if="showCreateModal" @click.self="resetModal">
      <div class="view-modal premium">
        <div class="modal-header"><h3>Create View</h3></div>
        <div class="modal-body">
            <div class="input-row">
                <div class="icon-box"><i class="fa-solid fa-layer-group"></i></div>
                <input type="text" v-model="newView.name" placeholder="Title" class="title-input" />
            </div>
            <textarea v-model="newView.description" placeholder="Description" rows="4" class="desc-input"></textarea>
            
            <div class="m-filter-section">
                <FilterBar 
                    :filters="activeFilters" 
                    @remove="handleRemoveFilter" 
                    @clear="handleClearFilters"
                    @add="handleAddFilter"
                />
            </div>
            
            <div class="modal-controls-bar">
                <div class="toggle-group">
                    <button class="m-toggle active" type="button" disabled><i :class="viewTypeIcon" class="mr-2"></i> List</button>
                    <!-- UPDATED PLACEMENT TO 'right-start' FOR SCROLLABILITY -->
                    <el-dropdown trigger="click" popper-class="display-popper-final" placement="right-start" :hide-on-click="false">
                        <button class="m-toggle" type="button">Display</button>
                        <template #dropdown>
                            <div class="display-scroll-vfinal">
                                <div class="st-content">
                                    <div class="st-sect">
                                        <div class="st-sect-header"><span>Display Properties</span><i class="fa-solid fa-chevron-up"></i></div>
                                        <div class="st-chips">
                                            <span v-for="p in ['ID', 'Assignee', 'Start date', 'Due date', 'Labels', 'Priority', 'State', 'Sub-work item count', 'Attachment count', 'Link', 'Estimate', 'Module', 'Cycle']" 
                                                  :key="p" class="p-chip-st" :class="{ selected: displayProps.includes(p) }" @click.stop="toggleDisplayProp(p)">{{ p }}</span>
                                        </div>
                                    </div>
                                    <div class="st-sect">
                                        <div class="st-sect-header"><span>Group by</span><i class="fa-solid fa-chevron-up"></i></div>
                                        <div class="st-radios">
                                            <label class="st-opt" v-for="g in ['States', 'Priority', 'Cycle', 'Module', 'Labels', 'Assignees', 'Created by', 'None']" :key="g">
                                                <input type="radio" name="pop-groupby" :value="g" v-model="groupBy" />
                                                <span class="st-dot"></span><span class="st-label">{{ g }}</span>
                                            </label>
                                        </div>
                                    </div>
                                    <div class="st-sect">
                                        <div class="st-sect-header"><span>Order by</span><i class="fa-solid fa-chevron-up"></i></div>
                                        <div class="st-radios">
                                            <label class="st-opt" v-for="o in ['Manual', 'Last created', 'Last updated', 'Start date', 'Due date', 'Priority']" :key="o">
                                                <input type="radio" name="pop-orderby" :value="o" v-model="orderBy" />
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
                <el-dropdown trigger="click" popper-class="filter-modal-popper" placement="bottom-start">
                    <button class="filter-btn" type="button"><i class="fa-solid fa-filter-circle-plus mr-2"></i> Filters</button>
                    <template #dropdown>
                        <div class="filter-modal-dropdown">
                            <div class="f-search">
                                <i class="fa-solid fa-magnifying-glass"></i>
                                <input type="text" placeholder="Search" />
                            </div>
                            <div class="f-options">
                                <div class="f-opt" @click="addFilterOption('State', 'fa-regular fa-circle-dot')"><i class="fa-regular fa-circle-dot"></i> State</div>
                                <div class="f-opt" @click="addFilterOption('State Group', 'fa-regular fa-circle-dot')"><i class="fa-regular fa-circle-dot"></i> State Group</div>
                                <div class="f-opt" @click="addFilterOption('Assignees', 'fa-regular fa-user')"><i class="fa-regular fa-user"></i> Assignees</div>
                                <div class="f-opt" @click="addFilterOption('Priority', 'fa-solid fa-signal')"><i class="fa-solid fa-signal"></i> Priority</div>
                                <div class="f-opt" @click="addFilterOption('Mentions', 'fa-solid fa-at')"><i class="fa-solid fa-at"></i> Mentions</div>
                                <div class="f-opt" @click="addFilterOption('Label', 'fa-solid fa-tag')"><i class="fa-solid fa-tag"></i> Label</div>
                                <div class="f-opt" @click="addFilterOption('Cycle', 'fa-regular fa-circle-pause')"><i class="fa-regular fa-circle-pause"></i> Cycle</div>
                                <div class="f-opt" @click="addFilterOption('Module', 'fa-solid fa-table-cells-large')"><i class="fa-solid fa-table-cells-large"></i> Module</div>
                                <div class="f-opt" @click="addFilterOption('Start date', 'fa-regular fa-calendar-plus')"><i class="fa-regular fa-calendar-plus"></i> Start date</div>
                                <div class="f-opt" @click="addFilterOption('Target date', 'fa-regular fa-calendar')"><i class="fa-regular fa-calendar"></i> Target date</div>
                                <div class="f-opt" @click="addFilterOption('Created at', 'fa-regular fa-calendar')"><i class="fa-regular fa-calendar"></i> Created at</div>
                                <div class="f-opt" @click="addFilterOption('Updated at', 'fa-regular fa-calendar')"><i class="fa-regular fa-calendar"></i> Updated at</div>
                            </div>
                        </div>
                    </template>
                </el-dropdown>
            </div>
        </div>
        <div class="modal-footer">
            <button class="cancel-btn" type="button" @click="resetModal">Cancel</button>
            <button class="create-btn" type="button" @click="createView" :disabled="!newView.name">Create View</button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
/* Main Page Layout (Restored to Turn 9) */
.views-page { display: flex; flex-direction: column; height: 100vh; background-color: var(--color-bg); font-family: 'Inter', sans-serif; color: var(--color-text-primary); }
.app-header { display: flex; justify-content: space-between; align-items: center; padding: 14px 24px; border-bottom: 1px solid rgba(255,255,255,0.03); }
.breadcrumb-refined { display: flex; align-items: center; gap: 10px; font-size: 13px; font-weight: 500; }
.sep { font-size: 8px; color: #3F3F46; }
.v-name { color: var(--color-text-muted); cursor: pointer; transition: color 0.2s; }
.v-name:hover { color: var(--color-text-primary); }

.header-right { display: flex; align-items: center; gap: 8px; }
.h-tool-btn { background: transparent; border: none; color: var(--color-text-muted); cursor: pointer; padding: 6px 12px; border-radius: 4px; font-size: 13px; }
.h-tool-btn.outlined { border: 1px solid #1E1E22; }
.view-search-input { background: var(--color-bg); border: 1px solid var(--color-border); border-radius: 6px; color: var(--color-text-primary); font-size: 13px; padding: 6px 10px; outline: none; width: 180px; }
.add-view-primary { background: #0EA5E9; border: none; color: var(--color-text-primary); padding: 6px 16px; border-radius: 6px; font-weight: 600; font-size: 13px; cursor: pointer; }

/* Content List (Restored to Turn 9) */
.views-content { flex: 1; padding: 12px 24px; }
.view-item-row { display: flex; justify-content: space-between; align-items: center; padding: 12px 14px; border-radius: 8px; cursor: pointer; transition: background 0.2s; }
.view-item-row:hover { background: var(--color-surface-hover); }
.vi-left { display: flex; align-items: center; gap: 14px; }
.vi-icon { color: var(--color-text-muted); font-size: 14px; }
.vi-name { font-size: 14px; color: var(--color-text-primary); }
.vi-right { display: flex; align-items: center; gap: 16px; opacity: 0; }
.view-item-row:hover .vi-right { opacity: 1; }
.vi-avatar { width: 22px; height: 22px; border-radius: 50%; background: #0F766E; color: var(--color-text-primary); font-size: 10px; font-weight: 700; display: flex; align-items: center; justify-content: center; }

/* Modal (Restored to Premium layout) */
.modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,0.6); display: flex; align-items: center; justify-content: center; z-index: 2000; }
.view-modal.premium { width: 700px; background: var(--color-surface); border: 1px solid var(--color-border); border-radius: 16px; overflow: hidden; }
.modal-header { padding: 20px 24px; border-bottom: 1px solid var(--color-border); font-size: 16px; font-weight: 600; }
.modal-body { padding: 24px; display: flex; flex-direction: column; gap: 16px; }
.m-filter-section { margin: 4px 0; }
.input-row { display: flex; align-items: center; background: var(--color-bg); border: 1px solid var(--color-border); border-radius: 8px; }
.icon-box { padding: 0 16px; border-right: 1px solid var(--color-border); color: var(--color-text-muted); }
.title-input { flex: 1; background: transparent; border: none; padding: 14px; color: var(--color-text-primary); outline: none; }
.desc-input { background: var(--color-bg); border: 1px solid var(--color-border); border-radius: 8px; padding: 14px; color: var(--color-text-secondary); outline: none; resize: none; }

.modal-controls-bar { display: flex; justify-content: space-between; align-items: center; }
.toggle-group { display: flex; background: var(--color-bg); border: 1px solid var(--color-border); border-radius: 8px; padding: 3px; gap: 4px; }
.m-toggle { background: transparent; border: none; color: var(--color-text-muted); padding: 6px 16px; border-radius: 6px; cursor: pointer; font-size: 13px; font-weight: 500; }
.m-toggle.active { background: var(--color-surface-hover); color: var(--color-text-primary); border: 1px solid var(--color-border); }
.filter-btn { background: var(--color-bg); border: 1px solid var(--color-border); color: var(--color-text-muted); padding: 8px 16px; border-radius: 8px; cursor: pointer; font-size: 13px; }

.modal-footer { padding: 16px 24px; background: var(--color-bg); border-top: 1px solid var(--color-border); display: flex; justify-content: flex-end; gap: 12px; }
.cancel-btn { background: transparent; border: none; color: var(--color-text-muted); padding: 8px 16px; cursor: pointer; }
.create-btn { background: var(--color-accent); border: none; color: var(--color-text-inverse); font-weight: 700; padding: 8px 24px; border-radius: 6px; cursor: pointer; }

/* REFINED DISPLAY DROPDOWN (ONLY PART EDITED) */
.display-scroll-vfinal {
    width: 330px; background: var(--color-surface); border-radius: 12px; border: 1px solid var(--color-border);
    max-height: 520px; overflow-y: auto; overflow-x: hidden;
}
.display-scroll-vfinal::-webkit-scrollbar { width: 5px; }
.display-scroll-vfinal::-webkit-scrollbar-thumb { background: var(--color-border); border-radius: 10px; }

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
.st-label { font-size: 13px; color: var(--color-text-secondary); }

.divider { height: 1px; background: #2D2F36; margin: 16px 0; }
.st-check { display: flex; align-items: center; gap: 10px; cursor: pointer; }
.st-check input { display: none; }
.checkmark { width: 15px; height: 15px; border: 1.5px solid #3F3F46; border-radius: 4px; position: relative; }
.st-check input:checked + .checkmark { background: #0EA5E9; border-color: #0EA5E9; }
.st-check input:checked + .checkmark::after {
    content: ""; position: absolute; left: 4px; top: 1px; width: 4px; height: 8px; border: solid white; border-width: 0 1.5px 1.5px 0; transform: rotate(45deg);
}

:deep(.display-popper-final) { background: transparent !important; border: none !important; box-shadow: none !important; }

/* MODAL FILTER DROPDOWN REFINEMENT */
.filter-modal-dropdown {
    width: 240px; background: var(--color-surface); border: 1px solid var(--color-border); border-radius: 8px; overflow: hidden;
}
.f-search {
    display: flex; align-items: center; gap: 10px; padding: 10px 14px; border-bottom: 1px solid var(--color-border);
}
.f-search i { font-size: 11px; color: var(--color-text-muted); }
.f-search input {
    background: transparent; border: none; color: var(--color-text-primary); font-size: 13px; outline: none; width: 100%;
}
.f-options { padding: 4px; max-height: 400px; overflow-y: auto; }
.f-opt {
    display: flex; align-items: center; gap: 12px; padding: 8px 12px; border-radius: 6px; cursor: pointer;
    font-size: 13px; color: var(--color-text-secondary); transition: background 0.2s;
}
.f-opt:hover { background: var(--color-surface-hover); color: var(--color-text-primary); }
.f-opt i { font-size: 12px; width: 16px; text-align: center; color: var(--color-text-muted); }

:deep(.filter-modal-popper) { background: transparent !important; border: none !important; box-shadow: none !important; }
</style>





