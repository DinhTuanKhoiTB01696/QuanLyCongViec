<script setup>
import { computed, ref } from 'vue'

const props = defineProps({
  tasks: { type: Array, default: () => [] }
})

const emit = defineEmits(['open-task', 'create-task'])

const currentDate = ref(new Date())
const showOptions = ref(false)
const showOnlyDated = ref(true)
const showDoneTasks = ref(true)
const highlightOverdue = ref(true)
const expandedDayKey = ref('')
const tooltip = ref({
  visible: false,
  x: 0,
  y: 0,
  tasks: [],
  label: ''
})

const goToday = () => {
  currentDate.value = new Date()
}

const prevMonth = () => {
  const next = new Date(currentDate.value)
  next.setMonth(next.getMonth() - 1)
  currentDate.value = next
}

const nextMonth = () => {
  const next = new Date(currentDate.value)
  next.setMonth(next.getMonth() + 1)
  currentDate.value = next
}

const monthLabel = computed(() => currentDate.value.toLocaleDateString('en-US', { month: 'long', year: 'numeric' }))
const weekDays = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun']

const hasScheduleDate = (task) => Boolean(task?.plannedStartDate || task?.plannedEndDate || task?.dueDate)

const unscheduledTasks = computed(() =>
  props.tasks.filter(task => {
    const status = `${task.statusName || ''}`.toUpperCase()
    if (!showDoneTasks.value && status.includes('DONE')) return false
    return !hasScheduleDate(task)
  })
)

const calendarDays = computed(() => {
  const year = currentDate.value.getFullYear()
  const month = currentDate.value.getMonth()
  const firstDay = new Date(year, month, 1)
  const lastDay = new Date(year, month + 1, 0)

  let startDow = firstDay.getDay() - 1
  if (startDow < 0) startDow = 6

  const days = []
  const prevLast = new Date(year, month, 0)

  for (let index = startDow - 1; index >= 0; index -= 1) {
    days.push({
      date: new Date(year, month - 1, prevLast.getDate() - index),
      isCurrentMonth: false
    })
  }

  for (let day = 1; day <= lastDay.getDate(); day += 1) {
    days.push({
      date: new Date(year, month, day),
      isCurrentMonth: true
    })
  }

  while (days.length < 42) {
    days.push({
      date: new Date(year, month + 1, days.length - lastDay.getDate() - startDow + 1),
      isCurrentMonth: false
    })
  }

  return days
})

const getTasksForDay = (date) => {
  const dayStart = startOfDay(date)
  const dayEnd = endOfDay(date)

  return props.tasks.filter(task => {
    const status = `${task.statusName || ''}`.toUpperCase()
    if (!showDoneTasks.value && status.includes('DONE')) return false

    const startDate = task.plannedStartDate ? startOfDay(task.plannedStartDate) : null
    const endDate = (task.dueDate || task.plannedEndDate) ? endOfDay(task.dueDate || task.plannedEndDate) : null
    const singleDate = startDate || endDate

    if (!singleDate && showOnlyDated.value) return false
    if (!singleDate) return false

    if (startDate && endDate) {
      return startDate <= dayEnd && endDate >= dayStart
    }

    return startOfDay(singleDate).getTime() === dayStart.getTime()
  })
}

const dayKey = (date) => formatDateOnly(date)

const visibleTasksForDay = (date) => {
  const tasks = getTasksForDay(date)
  return expandedDayKey.value === dayKey(date) ? tasks : tasks.slice(0, 2)
}

const hiddenCountForDay = (date) => Math.max(0, getTasksForDay(date).length - visibleTasksForDay(date).length)

const toggleTaskLimit = (date) => {
  const key = dayKey(date)
  expandedDayKey.value = expandedDayKey.value === key ? '' : key
}

const isToday = (date) => {
  const now = new Date()
  return startOfDay(date).getTime() === startOfDay(now).getTime()
}

const formatDayNum = (date) => {
  if (date.getDate() === 1) {
    return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' })
  }
  return `${date.getDate()}`
}

const getStatusColor = (statusName) => {
  const normalized = `${statusName || ''}`.toUpperCase()
  if (normalized.includes('DONE') || normalized.includes('COMPLETE')) return '#16a34a'
  if (normalized.includes('PROGRESS') || normalized.includes('REVIEW')) return '#2563eb'
  if (normalized.includes('TODO')) return '#8b5cf6'
  return '#64748b'
}

const isOverdueTask = (task) => {
  if (!highlightOverdue.value || !task?.dueDate) return false
  const normalized = `${task.statusName || ''}`.toUpperCase()
  if (normalized.includes('DONE')) return false
  return new Date(task.dueDate) < new Date()
}

const requestCreateTask = (date) => {
  emit('create-task', {
    plannedStartDate: formatDateOnly(date),
    dueDate: formatDateOnly(date)
  })
}

const showTooltip = (event, date) => {
  const tasks = getTasksForDay(date)
  if (!tasks.length) return

  tooltip.value = {
    visible: true,
    x: event.clientX + 12,
    y: event.clientY + 12,
    tasks: tasks.slice(0, 6),
    label: startOfDay(date).toLocaleDateString('vi-VN')
  }
}

const moveTooltip = (event) => {
  if (!tooltip.value.visible) return
  tooltip.value.x = event.clientX + 12
  tooltip.value.y = event.clientY + 12
}

const hideTooltip = () => {
  tooltip.value.visible = false
}

function startOfDay(value) {
  const date = parseCalendarDate(value)
  date.setHours(0, 0, 0, 0)
  return date
}

function endOfDay(value) {
  const date = parseCalendarDate(value)
  date.setHours(23, 59, 59, 999)
  return date
}

function parseCalendarDate(value) {
  if (value instanceof Date) return new Date(value)
  if (typeof value === 'string' && /^\d{4}-\d{2}-\d{2}$/.test(value)) {
    const [year, month, day] = value.split('-').map(Number)
    return new Date(year, month - 1, day)
  }
  return new Date(value)
}

function formatDateOnly(value) {
  const date = startOfDay(value)
  const year = date.getFullYear()
  const month = `${date.getMonth() + 1}`.padStart(2, '0')
  const day = `${date.getDate()}`.padStart(2, '0')
  return `${year}-${month}-${day}`
}
</script>

<template>
  <div class="plane-calendar">
    <div class="cal-header">
      <div class="cal-nav">
        <button class="nav-btn" type="button" @click="prevMonth"><i class="fa-solid fa-chevron-left"></i></button>
        <button class="nav-btn" type="button" @click="nextMonth"><i class="fa-solid fa-chevron-right"></i></button>
        <h2 class="cal-month-label">{{ monthLabel }}</h2>
      </div>

      <div class="cal-actions">
        <button class="cal-btn" type="button" @click="goToday">Today</button>
        <div class="cal-options">
          <button class="cal-btn" type="button" @click="showOptions = !showOptions">
            Options <i class="fa-solid fa-chevron-down"></i>
          </button>
          <div v-if="showOptions" class="cal-options-menu">
            <label class="cal-option-row"><input v-model="showOnlyDated" type="checkbox" /> Show dated work items</label>
            <label class="cal-option-row"><input v-model="showDoneTasks" type="checkbox" /> Show done work items</label>
            <label class="cal-option-row"><input v-model="highlightOverdue" type="checkbox" /> Highlight overdue</label>
          </div>
        </div>
      </div>
    </div>

    <div class="cal-content">
      <div class="cal-grid">
        <div v-for="dayName in weekDays" :key="dayName" class="cal-day-header">{{ dayName }}</div>

        <div
          v-for="(day, index) in calendarDays"
          :key="index"
          class="cal-day-cell"
          :class="{ 'other-month': !day.isCurrentMonth, 'is-today': isToday(day.date) }"
          @mouseenter="showTooltip($event, day.date)"
          @mousemove="moveTooltip"
          @mouseleave="hideTooltip"
        >
          <div class="day-number" :class="{ 'today-badge': isToday(day.date) }">{{ formatDayNum(day.date) }}</div>

          <div class="day-tasks">
            <div
              v-for="task in visibleTasksForDay(day.date)"
              :key="task.id"
              class="day-task-chip"
              :class="{ overdue: isOverdueTask(task) }"
              :style="{ borderLeft: `3px solid ${getStatusColor(task.statusName)}` }"
              @click="emit('open-task', task)"
              @mouseenter.stop="showTooltip($event, day.date)"
            >
              <span class="chip-text">{{ task.title }}</span>
            </div>

            <button
              v-if="hiddenCountForDay(day.date) > 0 || (getTasksForDay(day.date).length > 2 && expandedDayKey === dayKey(day.date))"
              type="button"
              class="day-more"
              @click="toggleTaskLimit(day.date)"
            >
              {{ expandedDayKey === dayKey(day.date) ? 'Show less' : `+ ${hiddenCountForDay(day.date)} more` }}
            </button>
          </div>

          <button v-if="day.isCurrentMonth" class="day-add-btn" type="button" @click="requestCreateTask(day.date)">
            <i class="fa-solid fa-plus"></i> Add work item
          </button>
        </div>
      </div>

      <aside class="unscheduled-panel">
        <div class="unscheduled-head">
          <span>Unscheduled</span>
          <strong>{{ unscheduledTasks.length }}</strong>
        </div>
        <p class="unscheduled-note">Tasks without start or due date from API.</p>
        <div class="unscheduled-list">
          <button
            v-for="task in unscheduledTasks"
            :key="task.id"
            type="button"
            class="unscheduled-item"
            @click="emit('open-task', task)"
          >
            <span class="unscheduled-key">{{ task.sequenceId || task.id?.substring(0, 8)?.toUpperCase() }}</span>
            <span class="unscheduled-title">{{ task.title }}</span>
          </button>
          <div v-if="!unscheduledTasks.length" class="unscheduled-empty">No unscheduled work items.</div>
        </div>
      </aside>
    </div>

    <div
      v-if="tooltip.visible"
      class="calendar-tooltip"
      :style="{ left: `${tooltip.x}px`, top: `${tooltip.y}px` }"
    >
      <div class="tooltip-title">{{ tooltip.label }}</div>
      <div v-for="task in tooltip.tasks" :key="task.id" class="tooltip-row">
        <span class="tooltip-dot" :style="{ background: getStatusColor(task.statusName) }"></span>
        <span class="tooltip-text">{{ task.title }}</span>
      </div>
    </div>
  </div>
</template>

<style scoped>
.plane-calendar {
  display: flex;
  flex-direction: column;
  min-height: calc(100vh - 180px);
  background: var(--color-bg);
  color: var(--color-text-primary);
}

.cal-header,
.cal-nav,
.cal-actions {
  display: flex;
  align-items: center;
}

.cal-header {
  justify-content: space-between;
  padding: 16px 24px;
  border-bottom: 1px solid var(--color-border);
}

.cal-nav,
.cal-actions {
  gap: 8px;
}

.nav-btn,
.cal-btn {
  border: 1px solid var(--color-border);
  border-radius: 4px;
  background: var(--bg-secondary);
  color: var(--color-text-secondary);
  cursor: pointer;
}

.nav-btn {
  width: 30px;
  height: 30px;
}

.cal-btn {
  padding: 7px 14px;
  font-size: 13px;
}

.cal-month-label {
  margin: 0 0 0 8px;
  font-size: 18px;
}

.cal-options {
  position: relative;
}

.cal-options-menu {
  position: absolute;
  top: calc(100% + 8px);
  right: 0;
  z-index: 20;
  min-width: 220px;
  background: var(--bg-tertiary);
  border: 1px solid var(--color-border);
  border-radius: 8px;
  padding: 10px;
  box-shadow: 0 12px 30px rgba(0, 0, 0, 0.35);
}

.cal-option-row {
  display: flex;
  gap: 8px;
  align-items: center;
  color: var(--text-secondary);
  font-size: 13px;
  padding: 6px 0;
}

.cal-content {
  display: grid;
  grid-template-columns: minmax(0, 1fr) 280px;
  flex: 1;
  min-height: 0;
}

.cal-grid {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
  flex: 1;
  min-width: 0;
  border-left: 1px solid var(--color-border);
}

.cal-day-header {
  padding: 10px 12px;
  border-bottom: 1px solid var(--color-border);
  border-right: 1px solid var(--color-border);
  color: var(--color-text-muted);
  font-size: 12px;
  text-transform: uppercase;
  text-align: right;
}

.cal-day-cell {
  position: relative;
  min-height: 120px;
  padding: 6px 8px 32px;
  border-bottom: 1px solid var(--color-border);
  border-right: 1px solid var(--color-border);
}

.cal-day-cell:hover {
  background: var(--hover-bg);
}

.cal-day-cell.other-month {
  opacity: 0.4;
}

.day-number {
  margin-bottom: 6px;
  text-align: right;
  color: var(--color-text-secondary);
  font-size: 13px;
}

.day-number.today-badge {
  display: inline-flex;
  width: 26px;
  height: 26px;
  align-items: center;
  justify-content: center;
  border-radius: 50%;
  background: #2563eb;
  color: #ffffff;
  margin-left: auto;
}

.day-tasks {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.day-task-chip {
  border-radius: 4px;
  padding: 4px 6px;
  background: var(--bg-tertiary);
  font-size: 11px;
  cursor: pointer;
}

.day-task-chip.overdue {
  background: rgba(239, 68, 68, 0.12);
}

.chip-text {
  display: block;
  overflow: hidden;
  white-space: nowrap;
  text-overflow: ellipsis;
}

.day-more {
  border: 0;
  background: transparent;
  color: #38bdf8;
  text-align: left;
  padding: 2px 4px;
  cursor: pointer;
  font-size: 11px;
}

.day-add-btn {
  position: absolute;
  left: 6px;
  right: 6px;
  bottom: 4px;
  border: 0;
  border-radius: 4px;
  background: transparent;
  color: var(--color-text-muted);
  padding: 4px 6px;
  text-align: left;
  cursor: pointer;
  opacity: 0;
}

.cal-day-cell:hover .day-add-btn {
  opacity: 1;
}

.unscheduled-panel {
  display: flex;
  flex-direction: column;
  gap: 12px;
  border-left: 1px solid var(--color-border);
  background: var(--bg-secondary);
  padding: 16px;
  min-width: 0;
}

.unscheduled-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  color: var(--color-text-primary);
  font-size: 14px;
  font-weight: 600;
}

.unscheduled-head strong {
  color: var(--color-text-muted);
  font-size: 12px;
}

.unscheduled-note {
  margin: 0;
  color: var(--color-text-muted);
  font-size: 12px;
  line-height: 1.4;
}

.unscheduled-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
  overflow: auto;
}

.unscheduled-item {
  display: flex;
  flex-direction: column;
  gap: 4px;
  width: 100%;
  border: 1px solid var(--color-border);
  border-radius: 6px;
  background: var(--bg-tertiary);
  color: var(--color-text-primary);
  padding: 10px;
  text-align: left;
  cursor: pointer;
}

.unscheduled-item:hover {
  border-color: var(--color-primary);
}

.unscheduled-key,
.unscheduled-empty {
  color: var(--color-text-muted);
  font-size: 11px;
}

.unscheduled-title {
  overflow: hidden;
  color: var(--color-text-secondary);
  font-size: 12px;
  white-space: nowrap;
  text-overflow: ellipsis;
}

.calendar-tooltip {
  position: fixed;
  z-index: 40;
  width: 240px;
  max-width: calc(100vw - 24px);
  border: 1px solid #2d2f36;
  border-radius: 8px;
  background: var(--bg-secondary);
  padding: 10px;
  box-shadow: 0 16px 36px rgba(0, 0, 0, 0.35);
  pointer-events: none;
}

.tooltip-title {
  font-size: 12px;
  font-weight: 600;
  color: #ffffff;
  margin-bottom: 8px;
}

.tooltip-row {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 4px 0;
}

.tooltip-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  flex-shrink: 0;
}

.tooltip-text {
  font-size: 12px;
  color: var(--text-secondary);
  overflow: hidden;
  white-space: nowrap;
  text-overflow: ellipsis;
}

@media (max-width: 1100px) {
  .cal-content {
    grid-template-columns: 1fr;
  }

  .unscheduled-panel {
    border-top: 1px solid var(--color-border);
    border-left: 0;
  }
}
</style>




