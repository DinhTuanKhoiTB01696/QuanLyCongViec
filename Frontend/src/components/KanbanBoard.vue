<script setup>
import { computed, ref } from 'vue'
import draggable from 'vuedraggable'
import { ElMessage } from 'element-plus'
import axiosClient from '@/api/axiosClient'

const props = defineProps({
  tasks: { type: Array, default: () => [] },
  statuses: { type: Array, default: () => [] },
  projectId: { type: String, required: true },
  loading: { type: Boolean, default: false }
})

const emit = defineEmits(['task-click', 'status-changed', 'task-reordered', 'refresh'])

const dragEnabled = ref(true)
const createMode = ref(false)
const draftTitles = ref({})
const creatingByStatus = ref({})

const statusColumns = computed(() => {
  const defaultColumns = [
    { key: 'TO DO', label: 'To Do', color: '#94a3b8', icon: 'O' },
    { key: 'IN PROGRESS', label: 'In Progress', color: '#3b82f6', icon: '~' },
    { key: 'IN REVIEW', label: 'In Review', color: '#f59e0b', icon: '*' },
    { key: 'DONE', label: 'Done', color: '#22c55e', icon: '+' }
  ]

  return defaultColumns.map(column => ({
    ...column,
    tasks: props.tasks
      .filter(task => (task.statusName || 'TO DO').toUpperCase() === column.key)
      .sort((left, right) => (left.sortOrder || 0) - (right.sortOrder || 0))
  }))
})

function onDragEnd(event, targetStatus) {
  const movedTask = event.item?.__draggable_context?.element
  if (!movedTask) return

  const oldStatus = movedTask.statusName
  const newStatus = targetStatus

  if (oldStatus !== newStatus) {
    emit('status-changed', {
      issueId: movedTask.id,
      newStatusName: newStatus,
      taskStatusId: movedTask.taskStatusId,
      rowVersion: movedTask.rowVersion
    })
  }

  const column = statusColumns.value.find(item => item.key === newStatus)
  if (!column) return

  const newIndex = event.newIndex
  const tasks = column.tasks
  let sortOrder = 65536

  if (tasks.length > 1) {
    if (newIndex === 0) {
      sortOrder = (tasks[1]?.sortOrder || 65536) / 2
    } else if (newIndex >= tasks.length - 1) {
      sortOrder = (tasks[tasks.length - 2]?.sortOrder || 0) + 65536
    } else {
      const before = tasks[newIndex - 1]?.sortOrder || 0
      const after = tasks[newIndex + 1]?.sortOrder || before + 131072
      sortOrder = (before + after) / 2
    }
  }

  emit('task-reordered', {
    issueId: movedTask.id,
    sortOrder,
    newStatusName: newStatus
  })
}

function getPriorityInfo(priority) {
  const map = {
    0: { label: 'None', color: '#94a3b8', icon: '-' },
    1: { label: 'Urgent', color: '#dc2626', icon: '!!' },
    2: { label: 'High', color: '#ef4444', icon: '!' },
    3: { label: 'Normal', color: '#3b82f6', icon: '=' },
    4: { label: 'Low', color: '#22c55e', icon: '.' }
  }
  return map[priority] || map[0]
}

function getInitials(name) {
  if (!name) return '?'
  return name.split(' ').map(word => word[0]).join('').substring(0, 2).toUpperCase()
}

function getAvatarColor(name) {
  if (!name) return '#6b7280'
  let hash = 0
  for (let index = 0; index < name.length; index += 1) {
    hash = name.charCodeAt(index) + ((hash << 5) - hash)
  }
  return `hsl(${Math.abs(hash % 360)}, 55%, 50%)`
}

function formatDate(value) {
  if (!value) return ''
  return new Date(value).toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit' })
}

function getTaskProgress(task) {
  if (Array.isArray(task.assignees) && task.assignees.length) {
    const total = task.assignees.reduce((sum, item) => sum + (Number(item.contributionWeight) || 1), 0)
    const weighted = task.assignees.reduce((sum, item) => {
      const weight = Number(item.contributionWeight) || 1
      return sum + ((Number(item.progressPercent) || 0) * weight)
    }, 0)
    return Math.round(weighted / Math.max(total, 1))
  }

  if (Array.isArray(task.taskAssignments) && task.taskAssignments.length) {
    const total = task.taskAssignments.reduce((sum, item) => sum + (Number(item.contributionWeight) || 1), 0)
    const weighted = task.taskAssignments.reduce((sum, item) => {
      const weight = Number(item.contributionWeight) || 1
      return sum + ((Number(item.progressPercent) || 0) * weight)
    }, 0)
    return Math.round(weighted / Math.max(total, 1))
  }

  if (`${task.statusName || ''}`.toUpperCase().includes('DONE')) return 100
  return 0
}

function progressStyle(task) {
  const percent = getTaskProgress(task)
  return {
    background: `conic-gradient(#22c55e ${percent}%, #27272a ${percent}% 100%)`
  }
}

async function createTask(statusName) {
  const title = `${draftTitles.value[statusName] || ''}`.trim()
  if (!title || creatingByStatus.value[statusName]) return

  creatingByStatus.value = { ...creatingByStatus.value, [statusName]: true }
  try {
    await axiosClient.post(`/projects/${props.projectId}/WorkTasks`, {
      title,
      description: '',
      statusName,
      priority: 3
    })
    draftTitles.value = { ...draftTitles.value, [statusName]: '' }
    emit('refresh')
    ElMessage.success('Da tao work item moi.')
  } catch (error) {
    ElMessage.error(error.response?.data?.message || 'Khong the tao work item.')
  } finally {
    creatingByStatus.value = { ...creatingByStatus.value, [statusName]: false }
  }
}

function handleColumnClick(column) {
  if (!createMode.value) return
  if (!draftTitles.value[column.key]) {
    draftTitles.value = { ...draftTitles.value, [column.key]: '' }
  }
}
</script>

<template>
  <div class="kanban-shell">
    <div class="board-toolbar">
      <button class="toolbar-btn" type="button" :class="{ active: createMode }" @click="createMode = !createMode">
        Create mode
      </button>
      <span class="toolbar-copy">Click vao cot de nhap nhanh va tao work item moi.</span>
    </div>

    <div class="kanban-board">
      <div v-for="column in statusColumns" :key="column.key" class="kanban-column" @click="handleColumnClick(column)">
        <div class="column-header">
          <div class="column-header-left">
            <span class="column-status-dot" :style="{ color: column.color }">{{ column.icon }}</span>
            <span class="column-title">{{ column.label }}</span>
            <span class="column-count">{{ column.tasks.length }}</span>
          </div>
        </div>

        <div v-if="createMode" class="quick-create" @click.stop>
          <input
            v-model="draftTitles[column.key]"
            type="text"
            class="quick-create-input"
            :placeholder="`Add ${column.label}`"
            @keyup.enter="createTask(column.key)"
          />
          <button class="quick-create-btn" type="button" :disabled="creatingByStatus[column.key]" @click="createTask(column.key)">Add</button>
        </div>

        <draggable
          :list="column.tasks"
          group="kanban"
          item-key="id"
          ghost-class="task-ghost"
          drag-class="task-drag"
          :animation="200"
          :disabled="!dragEnabled"
          class="column-body"
          @end="event => onDragEnd(event, column.key)"
        >
          <template #item="{ element }">
            <div class="kanban-card" @click="emit('task-click', element)">
              <div class="card-top">
                <span class="card-seq">{{ element.sequenceId || element.id?.substring(0, 8) }}</span>
                <span class="card-priority" :style="{ color: getPriorityInfo(element.priority).color }">
                  {{ getPriorityInfo(element.priority).icon }}
                </span>
              </div>

              <div class="card-title">{{ element.title }}</div>

              <div class="card-meta">
                <div class="card-meta-left">
                  <span v-if="element.storyPoints" class="card-sp">{{ element.storyPoints }} SP</span>
                  <span v-if="element.dueDate" class="card-due" :class="{ overdue: new Date(element.dueDate) < new Date() }">
                    {{ formatDate(element.dueDate) }}
                  </span>
                </div>

                <div class="card-meta-right">
                  <div class="task-progress-ring" :style="progressStyle(element)" :title="`${getTaskProgress(element)}% progress`">
                    <span class="ring-value">{{ getTaskProgress(element) }}</span>
                  </div>

                  <div
                    v-if="element.assigneeName"
                    class="card-avatar"
                    :style="{ backgroundColor: getAvatarColor(element.assigneeName) }"
                    :title="element.assigneeName"
                  >
                    {{ getInitials(element.assigneeName) }}
                  </div>
                </div>
              </div>
            </div>
          </template>

          <template #footer>
            <div v-if="column.tasks.length === 0" class="column-empty">
              <span class="empty-icon">[]</span>
              <span>Chua co cong viec</span>
            </div>
          </template>
        </draggable>
      </div>
    </div>
  </div>
</template>

<style scoped>
.kanban-shell {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.board-toolbar {
  display: flex;
  align-items: center;
  gap: 12px;
}

.toolbar-btn {
  border: 1px solid #27272a;
  border-radius: 6px;
  background: #111317;
  color: #d4d4d8;
  padding: 7px 12px;
  cursor: pointer;
}

.toolbar-btn.active {
  background: #1f2937;
  color: #ffffff;
}

.toolbar-copy {
  color: #71717a;
  font-size: 12px;
}

.kanban-board {
  display: flex;
  gap: 16px;
  overflow-x: auto;
  padding: 8px 0;
  min-height: 60vh;
}

.kanban-column {
  flex: 0 0 300px;
  min-width: 300px;
  display: flex;
  flex-direction: column;
  background: #111317;
  border: 1px solid #1e2025;
  border-radius: 8px;
}

.column-header,
.column-header-left,
.card-top,
.card-meta,
.card-meta-left,
.card-meta-right {
  display: flex;
  align-items: center;
}

.column-header {
  justify-content: space-between;
  padding: 14px 16px;
  border-bottom: 1px solid #1e2025;
}

.column-header-left,
.card-meta-left,
.card-meta-right {
  gap: 8px;
}

.column-title {
  font-size: 13px;
  font-weight: 600;
  text-transform: uppercase;
}

.column-count {
  padding: 2px 8px;
  border-radius: 999px;
  background: #1f2937;
  color: #a1a1aa;
  font-size: 11px;
}

.quick-create {
  display: flex;
  gap: 8px;
  padding: 12px;
  border-bottom: 1px solid #1e2025;
}

.quick-create-input,
.quick-create-btn {
  border-radius: 6px;
  border: 1px solid #27272a;
}

.quick-create-input {
  flex: 1;
  background: #0d0f11;
  color: #e4e4e7;
  padding: 8px 10px;
}

.quick-create-btn {
  background: #111317;
  color: #e4e4e7;
  padding: 8px 12px;
  cursor: pointer;
}

.column-body {
  flex: 1;
  padding: 8px;
  display: flex;
  flex-direction: column;
  gap: 8px;
  min-height: 100px;
  overflow-y: auto;
}

.kanban-card {
  border: 1px solid #1e2025;
  border-radius: 8px;
  background: #0d0f11;
  padding: 12px;
  cursor: pointer;
}

.kanban-card:hover {
  border-color: #2563eb;
}

.card-top,
.card-meta {
  justify-content: space-between;
}

.card-seq,
.card-due,
.card-sp {
  font-size: 11px;
  color: #a1a1aa;
}

.card-title {
  margin: 8px 0 10px;
  font-size: 13px;
  color: #e4e4e7;
  line-height: 1.45;
}

.card-due.overdue {
  color: #ef4444;
}

.task-progress-ring {
  position: relative;
  width: 26px;
  height: 26px;
  border-radius: 50%;
  display: grid;
  place-items: center;
}

.task-progress-ring::after {
  content: '';
  width: 18px;
  height: 18px;
  border-radius: 50%;
  background: #0d0f11;
}

.ring-value {
  position: absolute;
  opacity: 0;
  font-size: 9px;
  font-weight: 700;
  color: #ffffff;
  transition: opacity 0.15s ease;
}

.kanban-card:hover .ring-value {
  opacity: 1;
}

.card-avatar {
  width: 24px;
  height: 24px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #ffffff;
  font-size: 10px;
  font-weight: 700;
}

.column-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
  color: #71717a;
  padding: 24px;
  font-size: 13px;
}

.task-ghost {
  opacity: 0.4;
  border: 2px dashed #2563eb !important;
}

.task-drag {
  transform: rotate(2deg);
}
</style>
