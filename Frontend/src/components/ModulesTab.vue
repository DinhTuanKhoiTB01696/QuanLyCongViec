<script setup>
import { computed, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import axiosClient from '@/api/axiosClient'
import { ElMessage, ElMessageBox } from 'element-plus'

const props = defineProps({
  projectId: { type: String, required: true }
})

const router = useRouter()

const modules = ref([])
const projectMembers = ref([])
const projectTasks = ref([])
const loadingModules = ref(false)
const loadingTasks = ref(false)
const loadingMoreModules = ref(false)
const showCreateModal = ref(false)
const isEditing = ref(false)
const editingModuleId = ref(null)
const rowCalendarModId = ref(null)
const moduleSearch = ref('')
const sortBy = ref('updatedAt')
const sortDirection = ref('desc')
const statusFilter = ref('all')
const viewMode = ref('list')
const activeModule = ref(null)

const modulePagination = ref({
  page: 1,
  pageSize: 20,
  totalCount: 0,
  totalPages: 0,
  hasPreviousPage: false,
  hasNextPage: false
})

const statusOptions = [
  { key: 'backlog', label: 'Backlog', icon: 'fa-solid fa-expand', color: 'var(--color-text-muted)', bg: 'rgba(113,113,122,0.15)' },
  { key: 'planned', label: 'Planned', icon: 'fa-regular fa-circle', color: '#60A5FA', bg: 'rgba(96,165,250,0.15)' },
  { key: 'in progress', label: 'In Progress', icon: 'fa-solid fa-circle-notch', color: '#FBBF24', bg: 'rgba(251,191,36,0.15)' },
  { key: 'paused', label: 'Paused', icon: 'fa-solid fa-pause', color: 'var(--color-text-muted)', bg: 'rgba(161,161,170,0.15)' },
  { key: 'completed', label: 'Completed', icon: 'fa-regular fa-circle-check', color: '#4ADE80', bg: 'rgba(74,222,128,0.15)' },
  { key: 'cancelled', label: 'Cancelled', icon: 'fa-regular fa-circle-xmark', color: '#F87171', bg: 'rgba(248,113,113,0.15)' }
]

const statusConfig = Object.fromEntries(statusOptions.map(option => [option.key, option]))

const form = ref({
  name: '',
  description: '',
  status: 'Backlog',
  leadId: null,
  taskIds: [],
  dateRange: []
})

const getStatusKey = (raw) => {
  if (!raw) return 'backlog'
  const normalized = raw.toLowerCase().trim()
  if (normalized.includes('progress') || normalized === 'active') return 'in progress'
  if (normalized.includes('complete') || normalized === 'done') return 'completed'
  if (normalized.includes('cancel')) return 'cancelled'
  if (normalized.includes('plan')) return 'planned'
  if (normalized.includes('pause')) return 'paused'
  return 'backlog'
}

const formatDate = (value) => {
  if (!value) return null
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return null
  return new Intl.DateTimeFormat('en-US', { month: 'short', day: 'numeric', year: 'numeric' }).format(date)
}

const formatDateRange = (startDate, targetDate) => {
  const start = formatDate(startDate)
  const end = formatDate(targetDate)
  if (start && end) return `${start} -> ${end}`
  if (start) return `${start} -> ...`
  if (end) return `... -> ${end}`
  return 'Start date -> End date'
}

const getLeadName = (leadId) => projectMembers.value.find(member => member.id === leadId)?.name || 'No lead'
const getLeadAvatar = (leadId) => projectMembers.value.find(member => member.id === leadId)?.avatar || null

const normalizeModule = (module) => ({
  id: module.id,
  name: module.name,
  description: module.description || '',
  statusKey: getStatusKey(module.status),
  statusRaw: module.status || 'Backlog',
  leadId: module.leadId || null,
  leadName: module.leadName || getLeadName(module.leadId),
  startDate: module.startDate || null,
  targetDate: module.targetDate || null,
  taskIds: Array.isArray(module.taskIds) ? module.taskIds : [],
  issueCount: module.issueCount ?? 0,
  doneIssueCount: module.doneIssueCount ?? 0,
  progress: module.progressPercent ?? 0,
  createdAt: module.createdAt,
  updatedAt: module.updatedAt,
  isFavorite: Boolean(module.isFavorite)
})

const filteredModules = computed(() => {
  if (statusFilter.value === 'all') {
    return modules.value
  }

  return modules.value.filter(module => module.statusKey === statusFilter.value)
})

const groupedModules = computed(() =>
  statusOptions
    .map(option => ({
      ...option,
      items: filteredModules.value.filter(module => module.statusKey === option.key)
    }))
    .filter(group => group.items.length > 0)
)

const canLoadMore = computed(() => modulePagination.value.hasNextPage)
const totalLoaded = computed(() => modules.value.length)

const buildModuleParams = (page) => ({
  page,
  pageSize: modulePagination.value.pageSize,
  search: moduleSearch.value || undefined,
  sortBy: sortBy.value,
  sortDirection: sortDirection.value
})

const fetchMembers = async () => {
  if (!props.projectId) return
  try {
    const res = await axiosClient.get(`/projects/${props.projectId}/members`)
    const raw = res.data?.data || res.data || []
    projectMembers.value = (Array.isArray(raw) ? raw : []).map(member => ({
      id: member.userId || member.id,
      name: member.fullName || member.name || member.userName || member.email || 'Unknown',
      avatar: (member.fullName || member.name || member.userName || member.email || 'U').substring(0, 1).toUpperCase()
    }))
  } catch (error) {
    console.error('[ModulesTab] Failed to fetch members', error)
  }
}

const fetchProjectTasks = async () => {
  if (!props.projectId) return
  loadingTasks.value = true
  try {
    const res = await axiosClient.get(`/projects/${props.projectId}/WorkTasks`)
    const raw = res.data?.data || []
    projectTasks.value = raw.map(task => ({
      id: task.id,
      title: task.title || 'Untitled',
      statusName: task.statusName || task.status || 'Backlog'
    }))
  } catch (error) {
    console.error('[ModulesTab] Failed to fetch project tasks', error)
  } finally {
    loadingTasks.value = false
  }
}

const fetchModules = async ({ page = 1, append = false } = {}) => {
  if (!props.projectId) return

  if (append) {
    loadingMoreModules.value = true
  } else {
    loadingModules.value = true
  }

  try {
    const res = await axiosClient.get(`/projects/${props.projectId}/modules`, {
      params: buildModuleParams(page)
    })

    const incomingModules = (res.data?.data || []).map(normalizeModule)
    modules.value = append ? [...modules.value, ...incomingModules] : incomingModules

    const pagination = res.data?.pagination || {}
    modulePagination.value = {
      page: pagination.page || page,
      pageSize: pagination.pageSize || modulePagination.value.pageSize,
      totalCount: pagination.totalCount || incomingModules.length,
      totalPages: pagination.totalPages || 0,
      hasPreviousPage: Boolean(pagination.hasPreviousPage),
      hasNextPage: Boolean(pagination.hasNextPage)
    }
  } catch (error) {
    console.error('[ModulesTab] Failed to fetch modules', error)
    if (!append) {
      modules.value = []
    }
  } finally {
    loadingModules.value = false
    loadingMoreModules.value = false
  }
}

const refreshModules = async () => {
  await fetchModules({ page: 1, append: false })
}

const loadMoreModules = async () => {
  if (!canLoadMore.value) return
  await fetchModules({ page: modulePagination.value.page + 1, append: true })
}

const openModuleTaskView = (module) => {
  router.push({
    name: 'SpaceSummary',
    params: { id: props.projectId },
    query: {
      tab: 'spreadsheet',
      moduleId: module.id,
      moduleName: module.name
    }
  })
}

const updateModuleStatus = async (module, newStatusKey) => {
  const config = statusConfig[newStatusKey]
  if (!config) return

  try {
    await axiosClient.put(`/projects/${props.projectId}/modules/${module.id}`, {
      name: module.name,
      description: module.description,
      status: config.label,
      leadId: module.leadId,
      taskIds: module.taskIds,
      startDate: module.startDate,
      targetDate: module.targetDate
    })

    module.statusKey = newStatusKey
    module.statusRaw = config.label
    ElMessage.success(`Status updated to ${config.label}`)
  } catch (error) {
    console.error('[ModulesTab] Failed to update status', error)
    ElMessage.error('Failed to update status')
  }
}

const updateModuleDateRange = async (module, value) => {
  const [startDate, targetDate] = Array.isArray(value) ? value : []

  try {
    await axiosClient.put(`/projects/${props.projectId}/modules/${module.id}`, {
      name: module.name,
      description: module.description,
      status: module.statusRaw,
      leadId: module.leadId,
      taskIds: module.taskIds,
      startDate: startDate || null,
      targetDate: targetDate || null
    })

    module.startDate = startDate || null
    module.targetDate = targetDate || null
    rowCalendarModId.value = null
    ElMessage.success('Date range updated')
  } catch (error) {
    console.error('[ModulesTab] Failed to update date range', error)
    ElMessage.error('Failed to update date range')
  }
}

const deleteModule = async (module) => {
  try {
    await ElMessageBox.confirm(
      `Are you sure you want to delete "${module.name}"?`,
      'Delete Module',
      { confirmButtonText: 'Delete', cancelButtonText: 'Cancel', type: 'warning' }
    )
    await axiosClient.delete(`/projects/${props.projectId}/modules/${module.id}`)
    modules.value = modules.value.filter(item => item.id !== module.id)
    modulePagination.value.totalCount = Math.max(0, modulePagination.value.totalCount - 1)
    ElMessage.success('Module deleted')
  } catch (error) {
    if (error !== 'cancel') {
      console.error('[ModulesTab] Failed to delete module', error)
      ElMessage.error('Failed to delete module')
    }
  }
}

const toggleFavorite = (module) => {
  module.isFavorite = !module.isFavorite
}

const openCreateModal = () => {
  isEditing.value = false
  editingModuleId.value = null
  form.value = {
    name: '',
    description: '',
    status: 'Backlog',
    leadId: null,
    taskIds: [],
    dateRange: []
  }
  showCreateModal.value = true
}

const editModule = (module) => {
  isEditing.value = true
  editingModuleId.value = module.id
  form.value = {
    name: module.name,
    description: module.description,
    status: module.statusRaw,
    leadId: module.leadId,
    taskIds: [...module.taskIds],
    dateRange: module.startDate || module.targetDate ? [module.startDate, module.targetDate] : []
  }
  showCreateModal.value = true
}

const submitModule = async () => {
  if (!form.value.name.trim()) {
    ElMessage.warning('Module name is required')
    return
  }

  const [startDate, targetDate] = form.value.dateRange || []
  const payload = {
    name: form.value.name.trim(),
    description: form.value.description?.trim() || '',
    status: form.value.status,
    leadId: form.value.leadId,
    taskIds: form.value.taskIds,
    startDate: startDate || null,
    targetDate: targetDate || null
  }

  try {
    if (isEditing.value && editingModuleId.value) {
      await axiosClient.put(`/projects/${props.projectId}/modules/${editingModuleId.value}`, payload)
      ElMessage.success('Module updated')
    } else {
      await axiosClient.post(`/projects/${props.projectId}/modules`, payload)
      ElMessage.success('Module created')
    }

    showCreateModal.value = false
    await refreshModules()
  } catch (error) {
    console.error('[ModulesTab] Failed to save module', error)
    ElMessage.error('Failed to save module')
  }
}

watch(
  () => props.projectId,
  async () => {
    modules.value = []
    await Promise.all([fetchMembers(), fetchProjectTasks(), refreshModules()])
  },
  { immediate: true }
)

watch([moduleSearch, sortBy, sortDirection], async () => {
  await refreshModules()
})

</script>

<template>
  <div class="plane-modules-wrapper">
    <div class="modules-view-header">
      <div class="vh-left">
        <div class="breadcrumb">
          <i class="fa-solid fa-certificate breadcrumb-icon"></i>
          <span>Modules</span>
        </div>
      </div>

      <div class="vh-right">
        <div class="search-box">
          <i class="fa-solid fa-magnifying-glass"></i>
          <input v-model="moduleSearch" type="text" placeholder="Search modules" />
        </div>

        <el-dropdown trigger="click" @command="(value) => { sortBy = value.field; sortDirection = value.direction }">
          <button class="filter-action" type="button">
            <i class="fa-solid fa-arrow-up-z-a"></i>
            Sort
          </button>
          <template #dropdown>
            <el-dropdown-menu>
              <el-dropdown-item :command="{ field: 'updatedAt', direction: 'desc' }">Recently updated</el-dropdown-item>
              <el-dropdown-item :command="{ field: 'name', direction: 'asc' }">Name A-Z</el-dropdown-item>
              <el-dropdown-item :command="{ field: 'name', direction: 'desc' }">Name Z-A</el-dropdown-item>
              <el-dropdown-item :command="{ field: 'status', direction: 'asc' }">Status</el-dropdown-item>
            </el-dropdown-menu>
          </template>
        </el-dropdown>

        <el-dropdown trigger="click" @command="(value) => statusFilter = value">
          <button class="filter-action" type="button">
            <i class="fa-solid fa-filter"></i>
            {{ statusFilter === 'all' ? 'All statuses' : statusConfig[statusFilter]?.label }}
          </button>
          <template #dropdown>
            <el-dropdown-menu>
              <el-dropdown-item command="all">All statuses</el-dropdown-item>
              <el-dropdown-item v-for="status in statusOptions" :key="status.key" :command="status.key">
                {{ status.label }}
              </el-dropdown-item>
            </el-dropdown-menu>
          </template>
        </el-dropdown>

        <div class="view-toggles">
          <button class="view-btn" :class="{ active: viewMode === 'list' }" @click="viewMode = 'list'">
            <i class="fa-solid fa-bars"></i>
          </button>
          <button class="view-btn" :class="{ active: viewMode === 'grid' }" @click="viewMode = 'grid'">
            <i class="fa-solid fa-border-all"></i>
          </button>
          <button class="view-btn" :class="{ active: viewMode === 'status' }" @click="viewMode = 'status'">
            <i class="fa-solid fa-table-list"></i>
          </button>
        </div>

        <button class="primary-action" @click="openCreateModal">Add Module</button>
      </div>
    </div>

    <div class="modules-toolbar-meta">
      <span>{{ totalLoaded }} / {{ modulePagination.totalCount }} loaded</span>
      <span>{{ sortBy }} · {{ sortDirection }}</span>
    </div>

    <div class="modules-body" v-loading="loadingModules">
      <div v-if="!loadingModules && filteredModules.length === 0" class="empty-state-wrapper">
        <div class="es-icon"><i class="fa-solid fa-cube"></i></div>
        <h3 class="es-title">No modules found</h3>
        <p class="es-desc">Create a module, adjust the status, then assign work items into it.</p>
      </div>

      <div v-else-if="viewMode === 'list'" class="modules-list">
        <div class="module-row" v-for="module in filteredModules" :key="module.id">
          <div class="mr-left" @dblclick="openModuleTaskView(module)">
            <div class="m-progress-ring">{{ Math.round(module.progress) }}%</div>
            <div class="module-copy">
              <div class="m-title">{{ module.name }}</div>
              <div class="m-subtitle">
                {{ module.doneIssueCount }} / {{ module.issueCount }} tasks done
              </div>
            </div>
          </div>

          <div class="mr-right">
            <el-popover placement="bottom-end" :width="280" trigger="click" @show="rowCalendarModId = module.id">
              <template #reference>
                <button class="m-date" type="button">
                  <i class="fa-regular fa-calendar"></i>
                  {{ formatDateRange(module.startDate, module.targetDate) }}
                </button>
              </template>
              <div class="date-editor">
                <el-date-picker
                  :model-value="[module.startDate, module.targetDate]"
                  type="daterange"
                  start-placeholder="Start date"
                  end-placeholder="Target date"
                  value-format="YYYY-MM-DDTHH:mm:ss.SSS[Z]"
                  @change="updateModuleDateRange(module, $event)"
                />
              </div>
            </el-popover>

            <el-dropdown trigger="click" @command="(key) => updateModuleStatus(module, key)">
              <button
                class="m-status-chip"
                type="button"
                :style="{ background: statusConfig[module.statusKey]?.bg, color: statusConfig[module.statusKey]?.color }"
              >
                {{ statusConfig[module.statusKey]?.label }}
              </button>
              <template #dropdown>
                <el-dropdown-menu>
                  <el-dropdown-item v-for="status in statusOptions" :key="status.key" :command="status.key">
                    {{ status.label }}
                  </el-dropdown-item>
                </el-dropdown-menu>
              </template>
            </el-dropdown>

            <div class="m-avatar" :class="{ 'has-lead': getLeadAvatar(module.leadId) }" :title="getLeadName(module.leadId)">
              <span v-if="getLeadAvatar(module.leadId)">{{ getLeadAvatar(module.leadId) }}</span>
              <i v-else class="fa-solid fa-user"></i>
            </div>

            <button class="icon-action m-icon" :class="{ 'is-fav': module.isFavorite }" @click="toggleFavorite(module)">
              <i :class="module.isFavorite ? 'fa-solid fa-star' : 'fa-regular fa-star'"></i>
            </button>

            <el-dropdown trigger="click">
              <button class="icon-action m-icon"><i class="fa-solid fa-ellipsis"></i></button>
              <template #dropdown>
                <el-dropdown-menu>
                  <el-dropdown-item @click="editModule(module)">Edit</el-dropdown-item>
                  <el-dropdown-item @click="openModuleTaskView(module)">Open</el-dropdown-item>
                  <el-dropdown-item @click="deleteModule(module)">Delete</el-dropdown-item>
                </el-dropdown-menu>
              </template>
            </el-dropdown>
          </div>
        </div>
      </div>

      <div v-else-if="viewMode === 'grid'" class="module-grid">
        <article class="grid-card" v-for="module in filteredModules" :key="module.id">
          <div class="grid-card-top">
            <div>
              <h3 class="grid-title">{{ module.name }}</h3>
              <p class="grid-desc">{{ module.description || 'No description yet.' }}</p>
            </div>
            <button class="icon-action" @click="editModule(module)"><i class="fa-solid fa-pen"></i></button>
          </div>

          <div class="grid-progress">
            <div class="grid-progress-bar">
              <span :style="{ width: `${Math.min(Math.max(module.progress, 0), 100)}%` }"></span>
            </div>
            <strong>{{ Math.round(module.progress) }}%</strong>
          </div>

          <div class="grid-meta">
            <span>{{ module.doneIssueCount }}/{{ module.issueCount }} done</span>
            <span>{{ formatDateRange(module.startDate, module.targetDate) }}</span>
          </div>

          <div class="grid-footer">
            <span class="grid-chip" :style="{ background: statusConfig[module.statusKey]?.bg, color: statusConfig[module.statusKey]?.color }">
              {{ statusConfig[module.statusKey]?.label }}
            </span>
            <button class="mini-link" @click="openModuleTaskView(module)">Open</button>
          </div>
        </article>
      </div>

      <div v-else class="status-groups">
        <section class="status-group" v-for="group in groupedModules" :key="group.key">
          <header class="status-group-header">
            <div class="status-group-title">
              <i :class="group.icon" :style="{ color: group.color }"></i>
              <span>{{ group.label }}</span>
            </div>
            <span>{{ group.items.length }}</span>
          </header>

          <div class="status-group-list">
            <div class="status-row" v-for="module in group.items" :key="module.id">
              <div>
                <div class="m-title">{{ module.name }}</div>
                <div class="m-subtitle">{{ module.doneIssueCount }} / {{ module.issueCount }} done</div>
              </div>
              <button class="mini-link" @click="editModule(module)">Edit</button>
            </div>
          </div>
        </section>
      </div>

      <div v-if="canLoadMore" class="load-more-wrap">
        <button class="load-more-btn" :disabled="loadingMoreModules" @click="loadMoreModules">
          {{ loadingMoreModules ? 'Loading...' : 'Load more modules' }}
        </button>
      </div>
    </div>

    <div class="modal-overlay" v-if="showCreateModal" @click.self="showCreateModal = false">
      <div class="create-module-modal">
        <div class="cm-header">
          <h2 class="cm-title">{{ isEditing ? 'Edit module' : 'Create module' }}</h2>
        </div>

        <div class="cm-body">
          <input v-model="form.name" class="cm-input" type="text" placeholder="Module name" />
          <textarea v-model="form.description" class="cm-textarea" rows="4" placeholder="Description"></textarea>

          <div class="cm-grid">
            <label class="field-block">
              <span>Status</span>
              <select v-model="form.status" class="field-select">
                <option v-for="status in statusOptions" :key="status.key" :value="status.label">{{ status.label }}</option>
              </select>
            </label>

            <label class="field-block">
              <span>Lead</span>
              <select v-model="form.leadId" class="field-select">
                <option :value="null">No lead</option>
                <option v-for="member in projectMembers" :key="member.id" :value="member.id">{{ member.name }}</option>
              </select>
            </label>
          </div>

          <label class="field-block">
            <span>Date range</span>
            <el-date-picker
              v-model="form.dateRange"
              type="daterange"
              start-placeholder="Start date"
              end-placeholder="Target date"
              value-format="YYYY-MM-DDTHH:mm:ss.SSS[Z]"
            />
          </label>

          <label class="field-block">
            <span>Work items in module</span>
            <div class="task-picker" v-loading="loadingTasks">
              <label v-for="task in projectTasks" :key="task.id" class="task-option">
                <input v-model="form.taskIds" type="checkbox" :value="task.id" />
                <span>{{ task.title }}</span>
                <small>{{ task.statusName }}</small>
              </label>
            </div>
          </label>
        </div>

        <div class="cm-footer">
          <button class="cm-btn-cancel" @click="showCreateModal = false">Cancel</button>
          <button class="cm-btn-create" @click="submitModule">{{ isEditing ? 'Update Module' : 'Create Module' }}</button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.plane-modules-wrapper {
  display: flex;
  flex-direction: column;
  min-height: calc(100vh - 120px);
  background: var(--color-bg);
  color: var(--color-text-primary);
}

.modules-view-header,
.modules-toolbar-meta {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 16px 24px;
  border-bottom: 1px solid var(--color-border);
}

.modules-toolbar-meta {
  padding-top: 10px;
  padding-bottom: 10px;
  color: var(--color-text-secondary);
  font-size: 12px;
}

.vh-right,
.breadcrumb,
.search-box,
.view-toggles,
.mr-right,
.grid-footer,
.grid-meta,
.grid-progress,
.status-group-header,
.status-row,
.cm-grid {
  display: flex;
  align-items: center;
  gap: 10px;
}

.search-box {
  min-width: 220px;
  padding: 0 10px;
  border: 1px solid var(--color-border);
  border-radius: 6px;
  background: var(--color-surface);
}

.search-box input {
  height: 32px;
  width: 100%;
  background: transparent;
  border: none;
  color: var(--color-text-primary);
  outline: none;
}

.filter-action,
.primary-action,
.view-btn,
.icon-action,
.m-date,
.load-more-btn,
.cm-btn-cancel,
.cm-btn-create,
.mini-link {
  border-radius: 6px;
}

.filter-action,
.view-btn,
.m-date,
.load-more-btn,
.cm-btn-cancel,
.field-select,
.cm-input,
.cm-textarea,
.task-picker {
  border: 1px solid var(--color-border);
  background: var(--color-surface);
  color: var(--color-text-primary);
}

.filter-action,
.primary-action,
.view-btn,
.icon-action,
.m-date,
.load-more-btn,
.cm-btn-cancel,
.cm-btn-create,
.mini-link {
  cursor: pointer;
}

.filter-action,
.primary-action,
.load-more-btn,
.cm-btn-cancel,
.cm-btn-create {
  padding: 8px 12px;
  font-size: 13px;
}

.view-toggles {
  padding: 2px;
  border-radius: 6px;
  background: var(--color-surface);
}

.view-btn {
  width: 34px;
  height: 30px;
}

.view-btn.active {
  background: var(--color-border);
  color: var(--color-text-primary);
}

.primary-action,
.cm-btn-create {
  background: #0ea5e9;
  border: none;
  color: var(--color-text-primary);
}

.modules-body {
  padding: 24px;
  flex: 1;
}

.modules-list,
.status-groups {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.module-row,
.status-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  padding: 14px 16px;
  border: 1px solid var(--color-border);
  border-radius: 8px;
  background: var(--color-surface);
}

.mr-left,
.module-copy {
  display: flex;
  align-items: center;
  gap: 14px;
}

.module-copy {
  flex-direction: column;
  align-items: flex-start;
  gap: 4px;
}

.m-progress-ring {
  width: 38px;
  height: 38px;
  border-radius: 50%;
  border: 3px solid var(--color-border);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 10px;
  color: var(--color-text-secondary);
  flex-shrink: 0;
}

.m-title,
.grid-title {
  font-size: 14px;
  font-weight: 600;
}

.m-subtitle,
.grid-desc,
.grid-meta,
.status-group-header,
.task-option small {
  color: var(--color-text-secondary);
  font-size: 12px;
}

.m-status-chip,
.grid-chip {
  padding: 6px 10px;
  border: none;
  font-size: 12px;
  font-weight: 600;
  border-radius: 6px;
}

.m-avatar {
  width: 28px;
  height: 28px;
  border-radius: 50%;
  border: 1px dashed #52525b;
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--color-text-muted);
}

.m-avatar.has-lead {
  border: none;
  background: #3b82f6;
  color: var(--color-text-primary);
}

.module-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));
  gap: 16px;
}

.grid-card,
.status-group {
  border: 1px solid var(--color-border);
  border-radius: 8px;
  background: var(--color-surface);
  padding: 16px;
}

.grid-card-top {
  display: flex;
  justify-content: space-between;
  gap: 12px;
  margin-bottom: 16px;
}

.grid-progress-bar {
  flex: 1;
  height: 8px;
  border-radius: 999px;
  overflow: hidden;
  background: var(--color-border);
}

.grid-progress-bar span {
  display: block;
  height: 100%;
  background: #0ea5e9;
}

.status-group-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
  margin-top: 14px;
}

.load-more-wrap {
  display: flex;
  justify-content: center;
  margin-top: 20px;
}

.empty-state-wrapper {
  min-height: 50vh;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  text-align: center;
}

.es-icon {
  font-size: 40px;
  color: #3f3f46;
  margin-bottom: 16px;
}

.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.55);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.create-module-modal {
  width: min(720px, calc(100vw - 32px));
  max-height: calc(100vh - 32px);
  overflow: auto;
  background: #141518;
  border: 1px solid var(--color-border);
  border-radius: 8px;
}

.cm-header,
.cm-body,
.cm-footer {
  padding: 20px;
}

.cm-body {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.cm-grid {
  align-items: stretch;
}

.field-block {
  display: flex;
  flex-direction: column;
  gap: 8px;
  flex: 1;
  font-size: 13px;
  color: #d4d4d8;
}

.cm-input,
.cm-textarea,
.field-select {
  width: 100%;
  padding: 10px 12px;
  font: inherit;
}

.task-picker {
  max-height: 220px;
  overflow: auto;
  padding: 8px;
  border-radius: 8px;
}

.task-option {
  display: grid;
  grid-template-columns: auto 1fr auto;
  gap: 10px;
  align-items: center;
  padding: 8px;
  border-radius: 6px;
}

.task-option:hover {
  background: var(--color-surface);
}

.cm-footer {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  border-top: 1px solid var(--color-border);
}

.mini-link {
  border: none;
  background: transparent;
  color: #38bdf8;
  padding: 0;
}

@media (max-width: 900px) {
  .modules-view-header,
  .modules-toolbar-meta {
    flex-direction: column;
    align-items: stretch;
  }

  .vh-right,
  .cm-grid,
  .module-row {
    flex-wrap: wrap;
  }

  .mr-right {
    width: 100%;
    justify-content: flex-start;
    flex-wrap: wrap;
  }
}
</style>




