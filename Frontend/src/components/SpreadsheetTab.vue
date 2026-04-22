<script setup>
import { computed, onMounted, ref, watch } from 'vue'
import axiosClient from '@/api/axiosClient'

const props = defineProps({
  tasks: { type: Array, default: () => [] },
  projectId: { type: String, default: '' },
  projectMembers: { type: Array, default: () => [] }
})

const emit = defineEmits(['task-click', 'update-task', 'create-task'])

const modules = ref([])
const cycles = ref([])
const assigneeSearch = ref('')
const searchQuery = ref('')
const statusFilter = ref('all')
const page = ref(1)
const pageSize = ref(25)
const displayOptionsOpen = ref(false)
const showOnlyAssigned = ref(false)
const hideDone = ref(false)
const showOnlyScheduled = ref(false)

const fetchOptions = async () => {
  if (!props.projectId) return

  const [modulesRes, cyclesRes] = await Promise.allSettled([
    axiosClient.get(`/projects/${props.projectId}/modules`),
    axiosClient.get(`/projects/${props.projectId}/sprints`)
  ])

  if (modulesRes.status === 'fulfilled') modules.value = modulesRes.value.data?.data || []
  if (cyclesRes.status === 'fulfilled') cycles.value = cyclesRes.value.data?.data || []
}

const parseLocalDate = (value) => {
  if (!value) return null
  if (value instanceof Date) return new Date(value)
  if (typeof value === 'string' && /^\d{4}-\d{2}-\d{2}$/.test(value)) {
    const [year, month, day] = value.split('-').map(Number)
    return new Date(year, month - 1, day)
  }
  if (typeof value === 'string' && /^\d{4}-\d{2}-\d{2}T/.test(value)) {
    const [year, month, day] = value.slice(0, 10).split('-').map(Number)
    return new Date(year, month - 1, day)
  }
  const parsed = new Date(value)
  return Number.isNaN(parsed.getTime()) ? null : parsed
}

const formatDate = (dateString) => {
  if (!dateString) return '-'
  const date = parseLocalDate(dateString)
  return date ? date.toLocaleString('en-US', { month: 'short', day: 'numeric', year: 'numeric' }) : '-'
}

const toInputDate = (value) => {
  if (!value) return ''
  const date = parseLocalDate(value)
  if (!date) return ''
  if (Number.isNaN(date.getTime())) return ''
  const year = date.getFullYear()
  const month = `${date.getMonth() + 1}`.padStart(2, '0')
  const day = `${date.getDate()}`.padStart(2, '0')
  return `${year}-${month}-${day}`
}

const toApiDate = (value) => (value ? value : null)

const getPrioIcon = (priority) => {
  if (priority === 1) return { class: 'fa-solid fa-angles-up text-red', label: 'Urgent' }
  if (priority === 2) return { class: 'fa-solid fa-chevron-up text-orange', label: 'High' }
  if (priority === 3) return { class: 'fa-solid fa-minus text-blue', label: 'Normal' }
  if (priority === 4) return { class: 'fa-solid fa-chevron-down text-muted', label: 'Low' }
  return { class: 'fa-solid fa-ban text-muted', label: 'None' }
}

const getStatusDisplay = (statusName) => {
  const status = `${statusName || ''}`.toUpperCase()
  if (status === 'DONE') return { class: 'fa-solid fa-circle-check text-green', label: 'Done' }
  if (status === 'IN PROGRESS' || status === 'INPROGRESS') return { class: 'fa-solid fa-circle-half-stroke text-orange', label: 'In Progress' }
  if (status === 'IN REVIEW' || status === 'REVIEW') return { class: 'fa-solid fa-eye text-orange', label: 'In Review' }
  if (status === 'TO DO' || status === 'TODO') return { class: 'fa-regular fa-circle text-muted', label: 'Todo' }
  return { class: 'fa-regular fa-circle-dashed text-muted', label: 'Backlog' }
}

const memberId = (member) => member.userId || member.id
const memberName = (member) => member.fullName || member.name || member.email || 'Unknown'

const filteredMembers = computed(() => {
  const keyword = assigneeSearch.value.trim().toLowerCase()
  if (!keyword) return props.projectMembers
  return props.projectMembers.filter(member => memberName(member).toLowerCase().includes(keyword))
})

const getTaskAssigneeIds = (task) => {
  if (Array.isArray(task.assigneeIds) && task.assigneeIds.length) return task.assigneeIds
  if (Array.isArray(task.assignees) && task.assignees.length) return task.assignees.map(item => item.userId || item.id).filter(Boolean)
  if (task.assignedUserId) return [task.assignedUserId]
  return []
}

const assigneeLabel = (task) => {
  const ids = getTaskAssigneeIds(task)
  if (!ids.length) return 'Assignees'
  if (ids.length > 1) return `${ids.length} assignees`
  const member = props.projectMembers.find(item => memberId(item) === ids[0])
  return member ? memberName(member) : task.assigneeName || 'Assignee'
}

const createdByLabel = (task) => {
  const creatorId = task.createdById || task.createdBy || task.reporterId
  const member = props.projectMembers.find(item => memberId(item) === creatorId)
  return member ? memberName(member) : task.createdByName || task.reporterName || '-'
}

const moduleLabel = (task) => {
  const moduleId = task.moduleId || task.moduleIds?.[0] || task.modules?.[0]?.id || task.modules?.[0]?.moduleId
  return modules.value.find(item => item.id === moduleId)?.name || task.moduleName || 'Modules'
}

const cycleLabel = (task) => cycles.value.find(item => item.id === task.sprintId)?.name || task.sprintName || 'Cycle'
const parentLabel = (task) => task.parentSequenceId || task.parentTitle || task.parentId || 'Parent'

const labelsLabel = (task) => {
  const labels = task.labels || task.labelNames || []
  return Array.isArray(labels) && labels.length ? labels.map(item => item.name || item).join(', ') : 'Labels'
}

const updateTaskTitle = (task, event) => {
  const newTitle = event.target.innerText.trim()
  if (!newTitle) {
    event.target.innerText = task.title
    return
  }
  if (newTitle !== task.title) emit('update-task', task, 'title', newTitle)
}

const updateField = (task, field, value) => {
  emit('update-task', task, field, value, task[field])
}

const updateDateField = (task, field, event) => {
  updateField(task, field, toApiDate(event.target.value))
}

const toggleTaskAssignee = (task, id) => {
  const currentIds = getTaskAssigneeIds(task)
  const nextIds = currentIds.includes(id) ? currentIds.filter(item => item !== id) : [...currentIds, id]
  emit('update-task', task, 'assigneeIds', nextIds, currentIds)
}

const normalizedTasks = computed(() => {
  return props.tasks.filter(task => {
    const status = `${task.statusName || ''}`.toUpperCase()
    if (hideDone.value && status.includes('DONE')) return false
    if (showOnlyAssigned.value && !getTaskAssigneeIds(task).length) return false
    if (showOnlyScheduled.value && !(task.plannedStartDate || task.dueDate || task.plannedEndDate)) return false
    if (statusFilter.value !== 'all' && status !== statusFilter.value) return false

    const keyword = searchQuery.value.trim().toLowerCase()
    if (!keyword) return true

    return [
      task.title,
      task.sequenceId,
      task.description,
      assigneeLabel(task),
      moduleLabel(task),
      cycleLabel(task)
    ].some(value => `${value || ''}`.toLowerCase().includes(keyword))
  })
})

const totalPages = computed(() => Math.max(1, Math.ceil(normalizedTasks.value.length / pageSize.value)))
const pagedTasks = computed(() => {
  const start = (page.value - 1) * pageSize.value
  return normalizedTasks.value.slice(start, start + pageSize.value)
})

watch([searchQuery, statusFilter, showOnlyAssigned, hideDone, showOnlyScheduled, pageSize], () => {
  page.value = 1
})

watch(totalPages, (next) => {
  if (page.value > next) page.value = next
})

onMounted(fetchOptions)
watch(() => props.projectId, fetchOptions)
</script>

<template>
  <div class="spreadsheet-container">
    <div class="table-toolbar">
      <div class="toolbar-left">
        <input v-model="searchQuery" type="text" class="toolbar-search" placeholder="Search work items" />
        <select v-model="statusFilter" class="toolbar-select">
          <option value="all">All states</option>
          <option value="BACKLOG">Backlog</option>
          <option value="TO DO">Todo</option>
          <option value="IN PROGRESS">In Progress</option>
          <option value="IN REVIEW">In Review</option>
          <option value="DONE">Done</option>
        </select>
      </div>

      <div class="toolbar-right">
        <div class="display-options">
          <button class="toolbar-btn" type="button" @click="displayOptionsOpen = !displayOptionsOpen">Display options</button>
          <div v-if="displayOptionsOpen" class="display-options-menu">
            <label class="display-option"><input v-model="showOnlyAssigned" type="checkbox" /> Only assigned</label>
            <label class="display-option"><input v-model="hideDone" type="checkbox" /> Hide done</label>
            <label class="display-option"><input v-model="showOnlyScheduled" type="checkbox" /> Only dated</label>
          </div>
        </div>

        <select v-model.number="pageSize" class="toolbar-select small">
          <option :value="25">25 rows</option>
          <option :value="50">50 rows</option>
          <option :value="100">100 rows</option>
        </select>
      </div>
    </div>

    <table class="plane-table">
      <thead>
        <tr>
          <th class="sticky-work-item">Work items</th>
          <th><i class="fa-regular fa-circle-dot"></i> State</th>
          <th><i class="fa-solid fa-signal"></i> Priority</th>
          <th><i class="fa-solid fa-user-group"></i> Assignees</th>
          <th><i class="fa-regular fa-user"></i> Created by</th>
          <th><i class="fa-regular fa-calendar"></i> Start date</th>
          <th><i class="fa-solid fa-calendar-day"></i> Due date</th>
          <th><i class="fa-solid fa-table-cells-large"></i> Modules</th>
          <th><i class="fa-solid fa-arrows-spin"></i> Cycle</th>
          <th><i class="fa-solid fa-diagram-project"></i> Parent</th>
          <th><i class="fa-solid fa-tag"></i> Labels</th>
          <th>Created on</th>
          <th>Updated on</th>
        </tr>
      </thead>

      <tbody>
        <tr v-for="(task, index) in pagedTasks" :key="task.id || index">
          <td class="sticky-work-item">
            <div class="wi-cell" @click="emit('task-click', task)">
              <span class="wi-id">{{ task.sequenceId || `CUN-${index + 1}` }}</span>
              <span
                class="wi-title"
                contenteditable="true"
                @click.stop
                @blur="updateTaskTitle(task, $event)"
                @keydown.enter.prevent="$event.target.blur()"
              >{{ task.title }}</span>
            </div>
          </td>

          <td>
            <el-dropdown trigger="click" @command="value => updateField(task, 'statusName', value)">
              <button class="cell-btn">
                <i :class="getStatusDisplay(task.statusName).class"></i>
                <span>{{ getStatusDisplay(task.statusName).label }}</span>
              </button>
              <template #dropdown>
                <el-dropdown-menu class="plane-dropdown">
                  <el-dropdown-item command="BACKLOG">Backlog</el-dropdown-item>
                  <el-dropdown-item command="TO DO">Todo</el-dropdown-item>
                  <el-dropdown-item command="IN PROGRESS">In Progress</el-dropdown-item>
                  <el-dropdown-item command="IN REVIEW">In Review</el-dropdown-item>
                  <el-dropdown-item command="DONE">Done</el-dropdown-item>
                </el-dropdown-menu>
              </template>
            </el-dropdown>
          </td>

          <td>
            <el-dropdown trigger="click" @command="value => updateField(task, 'priority', value)">
              <button class="cell-btn">
                <i :class="getPrioIcon(task.priority).class"></i>
                <span>{{ getPrioIcon(task.priority).label }}</span>
              </button>
              <template #dropdown>
                <el-dropdown-menu class="plane-dropdown">
                  <el-dropdown-item :command="1">Urgent</el-dropdown-item>
                  <el-dropdown-item :command="2">High</el-dropdown-item>
                  <el-dropdown-item :command="3">Normal</el-dropdown-item>
                  <el-dropdown-item :command="4">Low</el-dropdown-item>
                </el-dropdown-menu>
              </template>
            </el-dropdown>
          </td>

          <td>
            <el-popover placement="bottom" trigger="click" width="260" popper-class="plane-popover">
              <template #reference>
                <button class="cell-btn"><i class="fa-regular fa-user"></i>{{ assigneeLabel(task) }}</button>
              </template>
              <div class="popover-content">
                <input v-model="assigneeSearch" type="text" class="plane-search-input" placeholder="Search members" />
                <label
                  v-for="member in filteredMembers"
                  :key="memberId(member)"
                  class="member-option"
                  @click.stop="toggleTaskAssignee(task, memberId(member))"
                >
                  <input type="checkbox" :checked="getTaskAssigneeIds(task).includes(memberId(member))" />
                  {{ memberName(member) }}
                </label>
              </div>
            </el-popover>
          </td>

          <td><span class="muted-text">{{ createdByLabel(task) }}</span></td>
          <td><input class="date-input" type="date" :value="toInputDate(task.plannedStartDate || task.startDate)" @change="updateDateField(task, 'plannedStartDate', $event)" /></td>
          <td><input class="date-input" type="date" :value="toInputDate(task.dueDate)" @change="updateDateField(task, 'dueDate', $event)" /></td>

          <td>
            <el-dropdown trigger="click" @command="value => updateField(task, 'moduleId', value)">
              <button class="cell-btn">{{ moduleLabel(task) }}</button>
              <template #dropdown>
                <el-dropdown-menu class="plane-dropdown">
                  <el-dropdown-item :command="null">No module</el-dropdown-item>
                  <el-dropdown-item v-for="module in modules" :key="module.id" :command="module.id">{{ module.name }}</el-dropdown-item>
                </el-dropdown-menu>
              </template>
            </el-dropdown>
          </td>

          <td>
            <el-dropdown trigger="click" @command="value => updateField(task, 'sprintId', value)">
              <button class="cell-btn">{{ cycleLabel(task) }}</button>
              <template #dropdown>
                <el-dropdown-menu class="plane-dropdown">
                  <el-dropdown-item :command="null">No cycle</el-dropdown-item>
                  <el-dropdown-item v-for="cycle in cycles" :key="cycle.id" :command="cycle.id">{{ cycle.name }}</el-dropdown-item>
                </el-dropdown-menu>
              </template>
            </el-dropdown>
          </td>

          <td><span class="muted-text">{{ parentLabel(task) }}</span></td>
          <td><span class="muted-text">{{ labelsLabel(task) }}</span></td>
          <td><span class="date-text">{{ formatDate(task.createdDate || task.createdAt) }}</span></td>
          <td><span class="date-text">{{ formatDate(task.updatedDate || task.updatedAt) }}</span></td>
        </tr>

        <tr v-if="pagedTasks.length === 0">
          <td colspan="13" class="empty-cell">No work items found for the current display options.</td>
        </tr>
      </tbody>
    </table>

    <div class="table-footer">
      <button class="add-btn" type="button" @click="emit('create-task', { statusName: 'TO DO' })">
        <i class="fa-solid fa-plus"></i> Add work item
      </button>

      <div class="pagination">
        <button class="page-btn" type="button" :disabled="page <= 1" @click="page -= 1">Prev</button>
        <span>{{ page }} / {{ totalPages }}</span>
        <button class="page-btn" type="button" :disabled="page >= totalPages" @click="page += 1">Next</button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.spreadsheet-container {
  flex: 1;
  background: var(--color-bg);
  color: var(--color-text-primary);
  overflow: auto;
  border-top: 1px solid var(--color-border);
}

.table-toolbar,
.toolbar-left,
.toolbar-right,
.table-footer,
.pagination {
  display: flex;
  align-items: center;
}

.table-toolbar,
.table-footer {
  justify-content: space-between;
  gap: 12px;
  padding: 12px 16px;
  border-bottom: 1px solid var(--color-border);
  background: var(--color-bg);
  position: sticky;
  left: 0;
  z-index: 40;
}

.toolbar-left,
.toolbar-right,
.pagination {
  gap: 10px;
}

.toolbar-search,
.toolbar-select,
.date-input,
.plane-search-input {
  border: 1px solid var(--color-border);
  border-radius: 6px;
  background: #111317;
  color: var(--color-text-primary);
}

.toolbar-search {
  width: 220px;
  padding: 7px 10px;
}

.toolbar-select {
  padding: 7px 10px;
}

.toolbar-select.small {
  min-width: 90px;
}

.toolbar-btn,
.page-btn {
  border: 1px solid var(--color-border);
  border-radius: 6px;
  background: #111317;
  color: #d4d4d8;
  padding: 7px 10px;
  cursor: pointer;
}

.page-btn:disabled {
  opacity: 0.45;
  cursor: not-allowed;
}

.display-options {
  position: relative;
}

.display-options-menu {
  position: absolute;
  top: calc(100% + 8px);
  right: 0;
  z-index: 50;
  min-width: 200px;
  padding: 10px;
  border-radius: 8px;
  border: 1px solid var(--color-border);
  background: #111317;
  box-shadow: 0 12px 30px rgba(0, 0, 0, 0.35);
}

.display-option {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 6px 0;
  font-size: 13px;
}

.plane-table {
  min-width: 1900px;
  border-collapse: separate;
  border-spacing: 0;
  text-align: left;
  font-size: 13px;
}

.plane-table th,
.plane-table td {
  border-bottom: 1px solid var(--color-border);
  border-right: 1px solid var(--color-border);
  background: var(--color-bg);
}

.plane-table th {
  position: sticky;
  top: 57px;
  z-index: 15;
  padding: 12px 16px;
  color: var(--color-text-secondary);
  font-weight: 500;
  white-space: nowrap;
}

.plane-table td {
  padding: 8px 12px;
  white-space: nowrap;
}

.plane-table tr:hover td {
  background: var(--color-surface);
}

.sticky-work-item {
  position: sticky;
  left: 0;
  z-index: 20;
  min-width: 420px;
  max-width: 420px;
  box-shadow: 12px 0 18px rgba(0, 0, 0, 0.32);
}

th.sticky-work-item {
  z-index: 25;
}

.wi-cell {
  display: flex;
  align-items: center;
  gap: 16px;
  min-width: 0;
  cursor: pointer;
}

.wi-id {
  color: var(--color-text-secondary);
  font-size: 12px;
  min-width: 70px;
  flex-shrink: 0;
}

.wi-title {
  color: var(--color-text-primary);
  font-weight: 500;
  overflow: hidden;
  text-overflow: ellipsis;
  outline: none;
}

.cell-btn {
  display: flex;
  align-items: center;
  gap: 8px;
  width: 100%;
  min-height: 28px;
  padding: 4px 8px;
  border: 1px solid transparent;
  border-radius: 6px;
  background: transparent;
  color: var(--color-text-primary);
  cursor: pointer;
  text-align: left;
}

.cell-btn:hover {
  background: var(--color-border);
  border-color: var(--color-border);
}

.date-input {
  width: 135px;
  padding: 5px 8px;
}

.plane-search-input {
  width: 100%;
  padding: 6px 8px;
  margin-bottom: 8px;
}

.member-option {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 6px 8px;
  border-radius: 6px;
  cursor: pointer;
}

.member-option:hover {
  background: var(--color-border);
}

.text-green { color: #10b981; }
.text-orange { color: #f59e0b; }
.text-red { color: #ef4444; }
.text-blue { color: #3b82f6; }
.text-muted,
.muted-text,
.date-text { color: var(--color-text-secondary); }

.empty-cell {
  padding: 40px;
  text-align: center;
  color: var(--color-text-muted);
}

.add-btn {
  border: 0;
  background: transparent;
  color: var(--color-text-secondary);
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 8px;
}
</style>




