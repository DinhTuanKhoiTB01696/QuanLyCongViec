<template>
  <div class="plane-list-view">
    <div v-for="(group, key) in groupedTasks" :key="key" class="list-group">
      <div class="group-header" @click="toggleGroup(key)">
        <div class="gh-left">
          <i class="gh-chevron fa-solid" :class="collapsedGroups[key] ? 'fa-chevron-right' : 'fa-chevron-down'"></i>
          <i class="status-icon" :class="group.iconClass" :style="{ color: group.color }"></i>
          <span class="group-name">{{ getGroupName(group.name, key) }}</span>
          <span class="group-count">{{ group.tasks.length }}</span>
        </div>
        <div class="gh-right">
          <i class="fa-solid fa-plus add-icon"></i>
        </div>
      </div>

      <div v-show="!collapsedGroups[key]" class="group-content">
        <div v-for="task in group.tasks" :key="task.id" class="task-row" @click="emit('task-click', task)">
          <span class="task-id">{{ task.sequenceId || task.id.substring(0, 8).toUpperCase() }}</span>
          <span class="task-title" :style="group.name === 'Done' ? { textDecoration: 'line-through', color: '#71717a' } : {}">
            {{ task.title }}
          </span>

          <div class="task-progress-ring" :style="progressStyle(task)" :title="`${taskProgress(task)}% ${t('workItems.progress', 'progress')}`">
            <span class="ring-value">{{ taskProgress(task) }}</span>
          </div>

          <div class="pill-group status-cell" @click.stop>
            <el-dropdown trigger="click" @command="value => updateTaskProperty(task, 'statusName', value)">
              <div class="pill status-pill">
                <i class="status-icon-sm" :class="group.iconClass" :style="{ color: group.color }"></i>
                {{ getStatusLabel(task.statusName || group.name) }}
              </div>
              <template #dropdown>
                <el-dropdown-menu class="plane-dropdown">
                  <el-dropdown-item command="BACKLOG">{{ t('workItems.statusLabels.backlog') }}</el-dropdown-item>
                  <el-dropdown-item command="TO DO">{{ t('workItems.statusLabels.toDo') }}</el-dropdown-item>
                  <el-dropdown-item command="IN PROGRESS">{{ t('workItems.statusLabels.inProgress') }}</el-dropdown-item>
                  <el-dropdown-item command="IN REVIEW">{{ t('workItems.statusLabels.inReview') }}</el-dropdown-item>
                  <el-dropdown-item command="DONE">{{ t('workItems.statusLabels.done') }}</el-dropdown-item>
                </el-dropdown-menu>
              </template>
            </el-dropdown>
          </div>

          <div class="pill-group priority-cell" @click.stop>
            <el-dropdown trigger="click" @command="value => updateTaskProperty(task, 'priority', value)">
              <div class="pill icon-pill">
                <i class="fa-solid fa-angles-up text-red-500" v-if="task.priority === 1"></i>
                <i class="fa-solid fa-chevron-up text-orange-500" v-else-if="task.priority === 2"></i>
                <i class="fa-solid fa-minus text-blue-500" v-else-if="task.priority === 3"></i>
                <i class="fa-solid fa-chevron-down text-gray-400" v-else></i>
              </div>
              <template #dropdown>
                <el-dropdown-menu class="plane-dropdown">
                  <el-dropdown-item :command="1">{{ t('workItems.priority.urgent') }}</el-dropdown-item>
                  <el-dropdown-item :command="2">{{ t('workItems.priority.high') }}</el-dropdown-item>
                  <el-dropdown-item :command="3">{{ t('workItems.priority.normal') }}</el-dropdown-item>
                  <el-dropdown-item :command="4">{{ t('workItems.priority.low') }}</el-dropdown-item>
                </el-dropdown-menu>
              </template>
            </el-dropdown>
          </div>

          <div class="pill-group assignee-cell" @click.stop>
            <el-popover placement="bottom" trigger="click" width="260" popper-class="plane-popover">
              <template #reference>
                <div class="pill assignee-pill">
                  <div class="avatar-xxs">
                    <i v-if="!getTaskAssigneeSummary(task).label" class="fa-regular fa-user"></i>
                    <span v-else>{{ getTaskAssigneeSummary(task).avatar }}</span>
                  </div>
                  <span v-if="getTaskAssigneeSummary(task).label" class="pill-user-text">{{ getTaskAssigneeSummary(task).label }}</span>
                </div>
              </template>
              <div class="popover-content">
                <input v-model="searchAssignee" type="text" class="plane-search-input" :placeholder="t('workItems.searchMembers')" />
                <div class="plane-list mt-2">
                  <label
                    v-for="member in filteredMembers"
                    :key="member.userId || member.id"
                    class="plane-list-item"
                    @click.stop="toggleTaskAssignee(task, member.userId || member.id)"
                  >
                    <input type="checkbox" :checked="getTaskAssigneeIds(task).includes(member.userId || member.id)" />
                    {{ member.fullName || member.name || member.email }}
                  </label>
                </div>
              </div>
            </el-popover>
          </div>
        </div>

        <div v-if="inlineCreateGroup !== key" class="add-row-placeholder" @click="openInlineCreate(key)">
          <i class="fa-solid fa-plus"></i> {{ t('workItems.newWorkItem') }}
        </div>

        <div v-if="inlineCreateGroup === key" class="inline-create-box">
          <input
            ref="inlineInputs"
            v-model="inlineTaskTitle"
            type="text"
            class="ic-input"
            :placeholder="t('workItems.workItemTitlePlaceholder', 'Work item title')"
            @keyup.enter="submitInlineTask(group)"
            @keyup.esc="inlineCreateGroup = null"
          />
          <div class="dm-toolbar mt-2">
            <el-dropdown trigger="click" @command="value => (inlineTaskStatus = value)">
              <button class="dm-tool-btn">{{ getStatusLabel(inlineTaskStatus) }}</button>
              <template #dropdown>
                <el-dropdown-menu class="plane-dropdown">
                  <el-dropdown-item command="BACKLOG">{{ t('workItems.statusLabels.backlog') }}</el-dropdown-item>
                  <el-dropdown-item command="TO DO">{{ t('workItems.statusLabels.toDo') }}</el-dropdown-item>
                  <el-dropdown-item command="IN PROGRESS">{{ t('workItems.statusLabels.inProgress') }}</el-dropdown-item>
                  <el-dropdown-item command="DONE">{{ t('workItems.statusLabels.done') }}</el-dropdown-item>
                </el-dropdown-menu>
              </template>
            </el-dropdown>

            <el-dropdown trigger="click" @command="value => (inlineTaskPriority = value)">
              <button class="dm-tool-btn">
                {{ inlineTaskPriority === 1 ? t('workItems.priority.urgent') : inlineTaskPriority === 2 ? t('workItems.priority.high') : inlineTaskPriority === 3 ? t('workItems.priority.normal') : t('workItems.priority.low') }}
              </button>
              <template #dropdown>
                <el-dropdown-menu class="plane-dropdown">
                  <el-dropdown-item :command="1">{{ t('workItems.priority.urgent') }}</el-dropdown-item>
                  <el-dropdown-item :command="2">{{ t('workItems.priority.high') }}</el-dropdown-item>
                  <el-dropdown-item :command="3">{{ t('workItems.priority.normal') }}</el-dropdown-item>
                  <el-dropdown-item :command="4">{{ t('workItems.priority.low') }}</el-dropdown-item>
                </el-dropdown-menu>
              </template>
            </el-dropdown>
          </div>
          <div class="ic-hint mt-2">{{ t('workItems.pressEnterToAddHint', 'Press Enter to add another work item.') }}</div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, nextTick, ref } from 'vue'
import { useI18n } from '@/composables/useI18n'

const { t } = useI18n()

const props = defineProps({
  tasks: { type: Array, default: () => [] },
  projectMembers: { type: Array, default: () => [] },
  groupBy: { type: String, default: 'States' },
  showSubItems: { type: Boolean, default: false }
})

const getStatusLabel = (status) => {
  if (!status) return ''
  const s = status.toLowerCase().replace(/\s+/g, '')
  if (s === 'todo') return t('workItems.statusLabels.toDo')
  if (s === 'inprogress') return t('workItems.statusLabels.inProgress')
  if (s === 'inreview') return t('workItems.statusLabels.inReview')
  if (s === 'done') return t('workItems.statusLabels.done')
  if (s === 'cancelled') return t('workItems.statusLabels.cancelled')
  if (s === 'backlog') return t('workItems.statusLabels.backlog')
  return status
}

const getGroupName = (name, key) => {
  if (props.groupBy === 'None') {
    return t('workItems.allTasks', 'All tasks')
  }
  if (props.groupBy === 'Priority') {
    if (key === 'normal') return t('workItems.priority.normal', 'Normal')
    return t(`workItems.priority.${key}`)
  }
  // States
  if (key === 'todo') return t('workItems.statusLabels.toDo')
  if (key === 'inprogress') return t('workItems.statusLabels.inProgress')
  return t(`workItems.statusLabels.${key}`)
}

const emit = defineEmits(['task-click', 'task-created', 'update-task'])

const collapsedGroups = ref({})
const inlineCreateGroup = ref(null)
const inlineTaskTitle = ref('')
const inlineTaskPriority = ref(3)
const inlineTaskStatus = ref('TO DO')
const inlineInputs = ref(null)
const searchAssignee = ref('')

const filteredMembers = computed(() => {
  const keyword = searchAssignee.value.trim().toLowerCase()
  if (!keyword) return props.projectMembers
  return props.projectMembers.filter(member =>
    `${member.fullName || member.name || member.email || ''}`.toLowerCase().includes(keyword)
  )
})

const getTaskAssigneeIds = (task) => {
  return Array.from(new Set([
    ...(Array.isArray(task.assigneeIds) ? task.assigneeIds : []),
    ...(Array.isArray(task.assignees) ? task.assignees.map(item => item.userId || item.id).filter(Boolean) : []),
    ...(task.assignedUserId ? [task.assignedUserId] : [])
  ]))
}

const getTaskAssigneeSummary = (task) => {
  const ids = getTaskAssigneeIds(task)
  if (!ids.length) return { label: '', avatar: '' }
  if (ids.length === 1) {
    const member = props.projectMembers.find(item => (item.userId || item.id) === ids[0])
    const label = member?.fullName || member?.name || member?.email || task.assigneeName || t('workItems.assignee', 'Assignee')
    return { label, avatar: label.substring(0, 1).toUpperCase() }
  }
  return { label: t('workItems.assigneeCount', { count: ids.length }), avatar: `${ids.length}` }
}

const toggleTaskAssignee = (task, memberId) => {
  const currentIds = getTaskAssigneeIds(task)
  const nextIds = currentIds.includes(memberId)
    ? currentIds.filter(id => id !== memberId)
    : Array.from(new Set([...currentIds, memberId]))
  emit('update-task', task, 'assigneeIds', nextIds, currentIds)
}

const openInlineCreate = (key) => {
  inlineCreateGroup.value = key
  inlineTaskTitle.value = ''
  inlineTaskPriority.value = 3

  const groupMap = {
    backlog: 'BACKLOG',
    todo: 'TO DO',
    inprogress: 'IN PROGRESS',
    done: 'DONE'
  }
  inlineTaskStatus.value = groupMap[key] || 'BACKLOG'

  nextTick(() => {
    if (!inlineInputs.value) return
    if (Array.isArray(inlineInputs.value)) {
      inlineInputs.value[0]?.focus()
    } else {
      inlineInputs.value.focus()
    }
  })
}

const submitInlineTask = () => {
  if (!inlineTaskTitle.value.trim()) {
    inlineCreateGroup.value = null
    return
  }

  emit('task-created', {
    title: inlineTaskTitle.value.trim(),
    statusName: inlineTaskStatus.value,
    priority: inlineTaskPriority.value
  })
  inlineTaskTitle.value = ''
}

const toggleGroup = (key) => {
  collapsedGroups.value[key] = !collapsedGroups.value[key]
}

const groupedTasks = computed(() => {
  const visibleTasks = props.tasks.filter(task => props.showSubItems || !(task.parentTaskId || task.parentId))

  if (props.groupBy === 'None') {
    return {
      all: { name: 'All tasks', iconClass: 'fa-solid fa-layer-group', color: '#0EA5E9', tasks: visibleTasks }
    }
  }

  if (props.groupBy === 'Priority') {
    const groups = {
      urgent: { name: 'Urgent', iconClass: 'fa-solid fa-angles-up', color: '#ef4444', tasks: [] },
      high: { name: 'High', iconClass: 'fa-solid fa-chevron-up', color: '#f97316', tasks: [] },
      normal: { name: 'Normal', iconClass: 'fa-solid fa-minus', color: '#3b82f6', tasks: [] },
      low: { name: 'Low', iconClass: 'fa-solid fa-chevron-down', color: '#9ca3af', tasks: [] }
    }

    visibleTasks.forEach(task => {
      const priority = Number(task.priority) || 3
      if (priority === 1) groups.urgent.tasks.push(task)
      else if (priority === 2) groups.high.tasks.push(task)
      else if (priority === 3) groups.normal.tasks.push(task)
      else groups.low.tasks.push(task)
    })
    return groups
  }

  // Default: States
  const groups = {
    backlog: { name: 'Backlog', iconClass: 'fa-regular fa-circle-dashed', color: '#71717a', tasks: [] },
    todo: { name: 'Todo', iconClass: 'fa-regular fa-circle', color: '#a1a1aa', tasks: [] },
    inprogress: { name: 'In Progress', iconClass: 'fa-solid fa-circle-half-stroke', color: '#f59e0b', tasks: [] },
    done: { name: 'Done', iconClass: 'fa-solid fa-circle-check', color: '#10b981', tasks: [] }
  }

  visibleTasks.forEach(task => {
    const status = `${task.statusName || ''}`.toUpperCase().trim()
    if (status === 'IN PROGRESS' || status === 'INPROGRESS') groups.inprogress.tasks.push(task)
    else if (status === 'DONE') groups.done.tasks.push(task)
    else if (status === 'BACKLOG' || status === '') groups.backlog.tasks.push(task)
    else groups.todo.tasks.push(task)
  })

  return groups
})

const taskProgress = (task) => {
  if (Array.isArray(task.assignees) && task.assignees.length) {
    const total = task.assignees.reduce((sum, item) => sum + (Number(item.contributionWeight) || 1), 0)
    const weighted = task.assignees.reduce((sum, item) => sum + ((Number(item.progressPercent) || 0) * (Number(item.contributionWeight) || 1)), 0)
    return Math.round(weighted / Math.max(total, 1))
  }

  if (`${task.statusName || ''}`.toUpperCase().includes('DONE')) return 100
  return 0
}

const progressStyle = (task) => {
  const percent = taskProgress(task)
  return {
    background: `conic-gradient(#22c55e ${percent}%, var(--color-border) ${percent}% 100%)`
  }
}

const updateTaskProperty = (task, field, value) => {
  emit('update-task', task, field, value, task[field])
}
</script>

<style scoped>
.plane-list-view {
  --list-panel: color-mix(in srgb, var(--color-surface) 82%, #020617);
  --list-panel-strong: color-mix(in srgb, var(--color-surface) 92%, #020617);
  --list-line: color-mix(in srgb, var(--color-border) 78%, #38bdf8);
  --list-accent-soft: color-mix(in srgb, var(--color-accent) 12%, transparent);
  display: flex;
  flex-direction: column;
  gap: 14px;
  color: var(--color-text-primary);
  font-variant-numeric: tabular-nums;
}

.list-group {
  overflow: hidden;
  border: 1px solid var(--list-line);
  border-radius: 8px;
  background: color-mix(in srgb, var(--list-panel) 90%, transparent);
}

.group-header,
.gh-left,
.pill-group,
.pill {
  display: flex;
  align-items: center;
}

.group-header {
  justify-content: space-between;
  min-height: 42px;
  padding: 0 14px;
  border-bottom: 1px solid var(--list-line);
  background: color-mix(in srgb, var(--list-panel-strong) 84%, transparent);
  cursor: pointer;
  user-select: none;
  transition: background 160ms ease;
}

.group-header:hover {
  background: color-mix(in srgb, var(--list-panel-strong) 90%, var(--color-accent));
}

.gh-left,
.pill-group,
.pill {
  gap: 10px;
}

.group-name {
  font-size: 13px;
  font-weight: 720;
}

.group-count {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 22px;
  height: 20px;
  padding: 0 7px;
  border: 1px solid var(--list-line);
  border-radius: 999px;
  background: color-mix(in srgb, var(--color-bg) 58%, transparent);
  color: var(--color-text-secondary);
  font-size: 11px;
  font-weight: 720;
}

.task-id,
.add-row-placeholder,
.ic-hint {
  color: var(--color-text-muted);
}

.add-icon {
  color: var(--color-text-muted);
  opacity: 0.55;
  transition: opacity 160ms ease, color 160ms ease;
}

.group-header:hover .add-icon,
.task-row:hover .pill-group,
.task-row:focus-within .pill-group {
  opacity: 1;
}

.task-row {
  display: grid;
  grid-template-columns: minmax(76px, 92px) minmax(180px, 1fr) 42px 140px 42px 126px;
  align-items: center;
  column-gap: 10px;
  min-height: 43px;
  padding: 0 12px 0 32px;
  border-bottom: 1px solid color-mix(in srgb, var(--list-line) 82%, transparent);
  cursor: pointer;
  transition: background 150ms ease, box-shadow 150ms ease;
}

.task-row:hover {
  background: color-mix(in srgb, var(--list-panel-strong) 80%, #ffffff);
  box-shadow: inset 2px 0 0 var(--color-accent);
}

.task-row:active {
  background: color-mix(in srgb, var(--list-panel-strong) 88%, #ffffff);
}

.task-row:last-child {
  border-bottom: 0;
}

.task-title {
  min-width: 0;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  color: var(--color-text-primary);
  font-size: 13px;
  font-weight: 620;
}

.task-id {
  min-width: 0;
  color: color-mix(in srgb, var(--color-accent) 74%, var(--color-text-muted));
  font-size: 12px;
  font-weight: 650;
}

.gh-chevron {
  width: 14px;
  color: var(--color-text-muted);
  font-size: 11px;
}

.status-icon {
  width: 14px;
  font-size: 13px;
}

.task-progress-ring {
  position: relative;
  width: 24px;
  height: 24px;
  border-radius: 50%;
  display: grid;
  place-items: center;
  flex-shrink: 0;
  justify-self: center;
  box-shadow: 0 0 0 1px color-mix(in srgb, var(--color-border) 52%, transparent);
}

.task-progress-ring::after {
  content: '';
  width: 16px;
  height: 16px;
  border-radius: 50%;
  background: color-mix(in srgb, var(--color-bg) 82%, #020617);
}

.ring-value {
  position: absolute;
  opacity: 0;
  font-size: 9px;
  font-weight: 700;
  transition: opacity 0.15s ease;
}

.task-row:hover .ring-value {
  opacity: 1;
}

.pill-group {
  min-width: 0;
  opacity: 0.62;
  transition: opacity 160ms ease;
}

.status-cell,
.priority-cell,
.assignee-cell {
  justify-content: center;
}

.status-cell {
  justify-content: flex-start;
}

.assignee-cell {
  justify-content: flex-end;
}

.pill {
  min-height: 26px;
  padding: 0 8px;
  border: 1px solid color-mix(in srgb, var(--color-border) 82%, transparent);
  border-radius: 7px;
  background: color-mix(in srgb, var(--color-bg) 54%, transparent);
  font-size: 12px;
  font-weight: 560;
  color: var(--color-text-secondary);
  transition: background 150ms ease, border-color 150ms ease, color 150ms ease;
}

.status-pill {
  width: 128px;
  justify-content: flex-start;
}

.icon-pill {
  width: 32px;
  justify-content: center;
}

.assignee-pill {
  width: 116px;
  justify-content: flex-start;
}

.pill:hover {
  border-color: color-mix(in srgb, var(--color-accent) 42%, var(--color-border));
  background: var(--list-accent-soft);
  color: var(--color-text-primary);
}

.status-icon-sm {
  font-size: 11px;
}

.pill-user-text {
  max-width: 120px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.avatar-xxs {
  width: 17px;
  height: 17px;
  border-radius: 5px;
  border: 1px solid color-mix(in srgb, var(--color-border) 72%, transparent);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 9px;
  font-weight: 720;
}

.add-row-placeholder {
  display: flex;
  align-items: center;
  gap: 7px;
  min-height: 38px;
  padding: 0 12px 0 38px;
  border-top: 1px solid color-mix(in srgb, var(--list-line) 58%, transparent);
  cursor: pointer;
  font-size: 12px;
  transition: color 150ms ease, background 150ms ease;
}

.add-row-placeholder:hover {
  background: color-mix(in srgb, var(--list-accent-soft) 78%, transparent);
  color: var(--color-accent);
}

.inline-create-box {
  margin: 10px 12px 12px 38px;
  padding: 12px;
  border: 1px solid color-mix(in srgb, var(--color-accent) 66%, var(--color-border));
  border-radius: 8px;
  background: color-mix(in srgb, var(--list-panel-strong) 88%, #020617);
  box-shadow: 0 0 0 3px color-mix(in srgb, var(--color-accent) 10%, transparent);
}

.ic-input,
.plane-search-input {
  width: 100%;
  background: color-mix(in srgb, var(--color-bg) 76%, #020617);
  border: 1px solid var(--list-line);
  border-radius: 7px;
  color: var(--color-text-secondary);
  padding: 9px 10px;
  font-size: 13px;
  outline: none;
}

.ic-input:focus,
.plane-search-input:focus {
  border-color: color-mix(in srgb, var(--color-accent) 62%, var(--color-border));
  box-shadow: 0 0 0 3px color-mix(in srgb, var(--color-accent) 12%, transparent);
  color: var(--color-text-primary);
}

.dm-toolbar {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.dm-tool-btn {
  min-height: 28px;
  border: 1px solid var(--list-line);
  border-radius: 7px;
  background: color-mix(in srgb, var(--color-bg) 68%, transparent);
  color: var(--color-text-secondary);
  padding: 0 10px;
  cursor: pointer;
  font-size: 12px;
  transition: background 150ms ease, color 150ms ease, border-color 150ms ease;
}

.dm-tool-btn:hover {
  border-color: color-mix(in srgb, var(--color-accent) 42%, var(--color-border));
  color: var(--color-text-primary);
}

.popover-content {
  color: var(--color-text-primary);
}

.plane-list {
  display: grid;
  gap: 4px;
}

.plane-list-item {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 7px 8px;
  border-radius: 7px;
  color: var(--color-text-secondary);
  cursor: pointer;
  font-size: 12px;
}

.plane-list-item:hover {
  background: color-mix(in srgb, var(--color-surface-hover) 78%, transparent);
  color: var(--color-text-primary);
}

.mt-2 {
  margin-top: 8px;
}

@media (max-width: 820px) {
  .task-row {
    grid-template-columns: minmax(72px, auto) minmax(0, 1fr) 32px;
    row-gap: 8px;
    padding: 10px 12px;
  }

  .task-progress-ring {
    grid-column: 3;
    grid-row: 1;
  }

  .status-cell {
    grid-column: 1 / span 2;
    grid-row: 2;
  }

  .priority-cell {
    grid-column: 3;
    grid-row: 2;
  }

  .assignee-cell {
    grid-column: 1 / -1;
    grid-row: 3;
    justify-content: flex-start;
  }

  .pill-group {
    opacity: 1;
  }

  .status-pill,
  .assignee-pill {
    width: auto;
    max-width: 100%;
  }
}
</style>




