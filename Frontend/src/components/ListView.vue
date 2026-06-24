<template>
  <div class="plane-list-view">
    <div v-for="(group, key) in groupedTasks" :key="key" class="list-group">
      <div class="group-header" @click="toggleGroup(key)">
        <div class="gh-left">
          <i class="gh-chevron fa-solid" :class="collapsedGroups[key] ? 'fa-chevron-right' : 'fa-chevron-down'"></i>
          <i class="status-icon" :class="group.iconClass" :style="{ color: group.color }"></i>
          <span class="group-name">{{ group.name }}</span>
          <span class="group-count">{{ group.tasks.length }}</span>
        </div>
        <div class="gh-right">
          <i class="fa-solid fa-plus add-icon"></i>
        </div>
      </div>

      <div v-show="!collapsedGroups[key]" class="group-content">
        <div v-for="task in group.tasks" :key="task.id" class="task-row" @click="emit('task-click', task)">
          <div class="tr-left">
            <span class="task-id">{{ task.sequenceId || task.id.substring(0, 8).toUpperCase() }}</span>
            <span class="task-title" :style="group.name === 'Done' ? { textDecoration: 'line-through', color: '#71717a' } : {}">
              {{ task.title }}
            </span>
          </div>

          <div class="tr-right">
            <div class="task-progress-ring" :style="progressStyle(task)" :title="`${taskProgress(task)}% progress`">
              <span class="ring-value">{{ taskProgress(task) }}</span>
            </div>

            <div class="pill-group" @click.stop>
              <el-dropdown trigger="click" @command="value => updateTaskProperty(task, 'statusName', value)">
                <div class="pill">
                  <i class="status-icon-sm" :class="group.iconClass" :style="{ color: group.color }"></i>
                  {{ task.statusName || group.name }}
                </div>
                <template #dropdown>
                  <el-dropdown-menu class="plane-dropdown">
                    <el-dropdown-item command="BACKLOG">Backlog</el-dropdown-item>
                    <el-dropdown-item command="TO DO">To Do</el-dropdown-item>
                    <el-dropdown-item command="IN PROGRESS">In Progress</el-dropdown-item>
                    <el-dropdown-item command="IN REVIEW">In Review</el-dropdown-item>
                    <el-dropdown-item command="DONE">Done</el-dropdown-item>
                  </el-dropdown-menu>
                </template>
              </el-dropdown>

              <el-dropdown trigger="click" @command="value => updateTaskProperty(task, 'priority', value)">
                <div class="pill">
                  <i class="fa-solid fa-angles-up text-red-500" v-if="task.priority === 1"></i>
                  <i class="fa-solid fa-chevron-up text-orange-500" v-else-if="task.priority === 2"></i>
                  <i class="fa-solid fa-minus text-blue-500" v-else-if="task.priority === 3"></i>
                  <i class="fa-solid fa-chevron-down text-gray-400" v-else></i>
                </div>
                <template #dropdown>
                  <el-dropdown-menu class="plane-dropdown">
                    <el-dropdown-item :command="1">Urgent</el-dropdown-item>
                    <el-dropdown-item :command="2">High</el-dropdown-item>
                    <el-dropdown-item :command="3">Normal</el-dropdown-item>
                    <el-dropdown-item :command="4">Low</el-dropdown-item>
                  </el-dropdown-menu>
                </template>
              </el-dropdown>

              <el-popover placement="bottom" trigger="click" width="260" popper-class="plane-popover">
                <template #reference>
                  <div class="pill">
                    <div class="avatar-xxs">
                      <i v-if="!getTaskAssigneeSummary(task).label" class="fa-regular fa-user"></i>
                      <span v-else>{{ getTaskAssigneeSummary(task).avatar }}</span>
                    </div>
                    <span v-if="getTaskAssigneeSummary(task).label" class="pill-user-text">{{ getTaskAssigneeSummary(task).label }}</span>
                  </div>
                </template>
                <div class="popover-content">
                  <input v-model="searchAssignee" type="text" class="plane-search-input" placeholder="Search members" />
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
        </div>

        <div v-if="inlineCreateGroup !== key" class="add-row-placeholder" @click="openInlineCreate(key)">
          <i class="fa-solid fa-plus"></i> New work item
        </div>

        <div v-if="inlineCreateGroup === key" class="inline-create-box">
          <input
            ref="inlineInputs"
            v-model="inlineTaskTitle"
            type="text"
            class="ic-input"
            placeholder="Work item title"
            @keyup.enter="submitInlineTask(group)"
            @keyup.esc="inlineCreateGroup = null"
          />
          <div class="dm-toolbar mt-2">
            <el-dropdown trigger="click" @command="value => (inlineTaskStatus = value)">
              <button class="dm-tool-btn">{{ inlineTaskStatus }}</button>
              <template #dropdown>
                <el-dropdown-menu class="plane-dropdown">
                  <el-dropdown-item command="BACKLOG">Backlog</el-dropdown-item>
                  <el-dropdown-item command="TO DO">To Do</el-dropdown-item>
                  <el-dropdown-item command="IN PROGRESS">In Progress</el-dropdown-item>
                  <el-dropdown-item command="DONE">Done</el-dropdown-item>
                </el-dropdown-menu>
              </template>
            </el-dropdown>

            <el-dropdown trigger="click" @command="value => (inlineTaskPriority = value)">
              <button class="dm-tool-btn">
                {{ inlineTaskPriority === 1 ? 'Urgent' : inlineTaskPriority === 2 ? 'High' : inlineTaskPriority === 3 ? 'Normal' : 'Low' }}
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
          </div>
          <div class="ic-hint mt-2">Press Enter to add another work item.</div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, nextTick, ref } from 'vue'

const props = defineProps({
  tasks: { type: Array, default: () => [] },
  projectMembers: { type: Array, default: () => [] },
  groupBy: { type: String, default: 'States' },
  showSubItems: { type: Boolean, default: false }
})

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
    const label = member?.fullName || member?.name || member?.email || task.assigneeName || 'Assignee'
    return { label, avatar: label.substring(0, 1).toUpperCase() }
  }
  return { label: `${ids.length} assignees`, avatar: `${ids.length}` }
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
  display: flex;
  flex-direction: column;
  color: var(--color-text-primary);
}

.list-group {
  margin-bottom: 24px;
}

.group-header,
.gh-left,
.tr-left,
.tr-right,
.pill-group,
.pill {
  display: flex;
  align-items: center;
}

.group-header {
  justify-content: space-between;
  padding: 8px 0;
  border-bottom: 1px solid var(--color-border);
  cursor: pointer;
}

.gh-left,
.tr-left,
.tr-right,
.pill-group,
.pill {
  gap: 10px;
}

.group-name {
  font-size: 14px;
  font-weight: 600;
}

.group-count,
.task-id,
.add-row-placeholder,
.ic-hint {
  color: var(--color-text-muted);
}

.add-icon {
  opacity: 0;
}

.group-header:hover .add-icon,
.task-row:hover .pill-group {
  opacity: 1;
}

.task-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 10px 0 10px 24px;
  border-bottom: 1px solid var(--color-border);
  cursor: pointer;
}

.task-row:hover {
  background: var(--color-surface);
}

.task-title {
  color: var(--color-text-primary);
}

.tr-right {
  gap: 12px;
}

.task-progress-ring {
  position: relative;
  width: 26px;
  height: 26px;
  border-radius: 50%;
  display: grid;
  place-items: center;
  flex-shrink: 0;
}

.task-progress-ring::after {
  content: '';
  width: 18px;
  height: 18px;
  border-radius: 50%;
  background: var(--color-bg);
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
  opacity: 0;
  transition: opacity 0.2s;
}

.pill {
  padding: 4px 8px;
  border: 1px solid var(--color-border);
  border-radius: 2px;
  font-size: 12px;
  color: var(--color-text-secondary);
}

.pill-user-text {
  max-width: 120px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.avatar-xxs {
  width: 16px;
  height: 16px;
  border-radius: 50%;
  border: 1px dashed #3f3f46;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 9px;
}

.add-row-placeholder {
  padding: 12px 0 12px 24px;
  cursor: pointer;
}

.inline-create-box {
  margin: 8px 16px;
  padding: 12px;
  border: 1px solid #38bdf8;
  border-radius: 8px;
  background: var(--color-surface);
}

.ic-input,
.plane-search-input {
  width: 100%;
  background: var(--color-bg);
  border: 1px solid var(--color-border);
  border-radius: 2px;
  color: var(--color-text-primary);
  padding: 8px 10px;
}

.dm-toolbar {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.dm-tool-btn {
  border: 1px solid var(--color-border);
  border-radius: 4px;
  background: transparent;
  color: var(--color-text-secondary);
  padding: 4px 8px;
  cursor: pointer;
}

.mt-2 {
  margin-top: 8px;
}
</style>




