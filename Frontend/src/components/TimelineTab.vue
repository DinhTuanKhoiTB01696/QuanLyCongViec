<script setup>
import { ref, onMounted, computed } from 'vue'
import axiosClient from '@/api/axiosClient'

const props = defineProps({
  projectId: { type: String, required: true }
})

const emit = defineEmits(['open-task'])

const tasks = ref([])
const loading = ref(false)
const viewMode = ref('Week') // Week, Month, Quarter
const scrollContainer = ref(null)

// Timeline config
const today = new Date()
const cellWidth = 40 // px per day

// View ranges
const viewModes = ['Week', 'Month', 'Quarter']

const timelineRange = computed(() => {
  const start = new Date(today)
  const end = new Date(today)
  
  if (viewMode.value === 'Week') {
    start.setDate(start.getDate() - 7)
    end.setDate(end.getDate() + 21)
  } else if (viewMode.value === 'Month') {
    start.setDate(start.getDate() - 14)
    end.setDate(end.getDate() + 60)
  } else {
    start.setMonth(start.getMonth() - 1)
    end.setMonth(end.getMonth() + 4)
  }
  return { start, end }
})

// Generate day columns
const dayColumns = computed(() => {
  const { start, end } = timelineRange.value
  const days = []
  const current = new Date(start)
  while (current <= end) {
    days.push(new Date(current))
    current.setDate(current.getDate() + 1)
  }
  return days
})

// Generate week headers
const weekHeaders = computed(() => {
  const weeks = []
  let currentWeek = null
  dayColumns.value.forEach((day, idx) => {
    const weekNum = getWeekNumber(day)
    const monthNames = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec']
    const label = `Week ${weekNum}`
    const monthLabel = `${monthNames[day.getMonth()]} ${day.getFullYear()}`
    
    if (!currentWeek || currentWeek.weekNum !== weekNum) {
      currentWeek = { weekNum, label, monthLabel, startIdx: idx, span: 1 }
      weeks.push(currentWeek)
    } else {
      currentWeek.span++
    }
  })
  return weeks
})

// Format day columns
const formatDayHeader = (date) => {
  const dayNames = ['Su', 'M', 'T', 'W', 'Th', 'F', 'Sa']
  return {
    dayOfWeek: dayNames[date.getDay()],
    dayNum: date.getDate(),
    isToday: isSameDay(date, today),
    isWeekend: date.getDay() === 0 || date.getDay() === 6
  }
}

const isSameDay = (a, b) => {
  return a.getFullYear() === b.getFullYear() && a.getMonth() === b.getMonth() && a.getDate() === b.getDate()
}

const getWeekNumber = (date) => {
  const d = new Date(Date.UTC(date.getFullYear(), date.getMonth(), date.getDate()))
  const dayNum = d.getUTCDay() || 7
  d.setUTCDate(d.getUTCDate() + 4 - dayNum)
  const yearStart = new Date(Date.UTC(d.getUTCFullYear(), 0, 1))
  return Math.ceil((((d - yearStart) / 86400000) + 1) / 7)
}

// Calculate task bar position - uses fallback dates for tasks with no dates
const getTaskBar = (task) => {
  let start = task.plannedStartDate ? new Date(task.plannedStartDate) : null
  let end = task.plannedEndDate || task.dueDate ? new Date(task.plannedEndDate || task.dueDate) : null
  
  // Fallback: if neither date exists, use createdAt as start and +3 days as end
  if (!start && !end) {
    if (task.createdAt) {
      start = new Date(task.createdAt)
      end = new Date(start)
      end.setDate(end.getDate() + 3)
    } else {
      return null
    }
  }
  
  // If only one date is set, create a reasonable range
  if (!start) start = new Date(end)
  if (!end) {
    end = new Date(start)
    end.setDate(end.getDate() + 3) // Default 3-day duration
  }
  
  const rangeStart = timelineRange.value.start
  const left = Math.max(0, daysBetween(rangeStart, start)) * cellWidth
  const width = Math.max(1, daysBetween(start, end) + 1) * cellWidth
  
  return { left: `${left}px`, width: `${width}px` }
}

const daysBetween = (a, b) => {
  return Math.floor((b - a) / (1000 * 60 * 60 * 24))
}

const getTaskDuration = (task) => {
  let start = task.plannedStartDate ? new Date(task.plannedStartDate) : null
  let end = task.plannedEndDate || task.dueDate ? new Date(task.plannedEndDate || task.dueDate) : null
  
  if (!start && !end) {
    if (task.createdAt) return '3d' // fallback default
    return '-'
  }
  if (!start) start = new Date(end)
  if (!end) return '3d' // fallback default
  
  const days = daysBetween(start, end) + 1
  return `${days}d`
}

const getStatusColor = (statusName) => {
  const s = (statusName || '').toUpperCase()
  if (s.includes('DONE') || s.includes('COMPLETE')) return '#16a34a'
  if (s.includes('PROGRESS')) return '#3b82f6'
  if (s.includes('REVIEW')) return '#f59e0b'
  if (s.includes('TODO')) return '#a855f7'
  return '#6b7280'
}

const getTaskIcon = (task) => {
  const p = task.priority
  if (p === 1) return '🔴'
  if (p === 2) return '🟠'
  if (p === 3) return '🟡'
  return '⚪'
}

const goToToday = () => {
  if (scrollContainer.value) {
    const rangeStart = timelineRange.value.start
    const todayOffset = daysBetween(rangeStart, today) * cellWidth
    scrollContainer.value.scrollLeft = Math.max(0, todayOffset - 200)
  }
}

const totalWidth = computed(() => dayColumns.value.length * cellWidth)

// Today line offset
const todayOffset = computed(() => {
  const rangeStart = timelineRange.value.start
  return daysBetween(rangeStart, today) * cellWidth + cellWidth / 2
})

const fetchTasks = async () => {
  loading.value = true
  try {
    const res = await axiosClient.get(`/projects/${props.projectId}/WorkTasks`)
    tasks.value = res.data?.data || []
  } catch (err) {
    console.error(err)
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  fetchTasks()
  setTimeout(goToToday, 300)
})
</script>

<template>
  <div class="plane-timeline" v-loading="loading">
    <!-- Timeline Header -->
    <div class="tl-header">
      <div class="tl-header-left">
        <span class="tl-task-count">{{ tasks.length }} Work Items</span>
        <div class="tl-view-modes">
          <button 
            v-for="mode in viewModes" 
            :key="mode" 
            class="mode-btn" 
            :class="{ active: viewMode === mode }"
            @click="viewMode = mode"
          >{{ mode }}</button>
        </div>
      </div>
      <div class="tl-header-right">
        <button class="tl-btn" @click="goToToday">Today</button>
        <button class="tl-btn expand-btn"><i class="fa-solid fa-expand"></i></button>
      </div>
    </div>

    <!-- Timeline Body -->
    <div class="tl-body">
      <!-- Left panel: Task list -->
      <div class="tl-left-panel">
        <div class="tl-left-header">
          <div class="tl-col-workitems">Work Items</div>
          <div class="tl-col-duration">Duration</div>
        </div>
        <div class="tl-left-rows">
          <div 
            v-for="task in tasks" 
            :key="task.id" 
            class="tl-task-row"
            @click="$emit('open-task', task)"
          >
            <div class="tl-col-workitems">
              <span class="task-key">{{ task.id?.substring(0, 8).toUpperCase() }}</span>
              <span class="task-title-text">{{ task.title }}</span>
              <span class="task-emoji">{{ getTaskIcon(task) }}</span>
            </div>
            <div class="tl-col-duration">{{ getTaskDuration(task) }}</div>
          </div>
          <!-- New work item row -->
          <div class="tl-task-row tl-add-row">
            <span class="add-text"><i class="fa-solid fa-plus"></i> New work item</span>
          </div>
        </div>
      </div>

      <!-- Right panel: Gantt chart -->
      <div class="tl-right-panel" ref="scrollContainer">
        <div class="tl-gantt" :style="{ width: totalWidth + 'px' }">
          <!-- Week headers -->
          <div class="tl-week-row">
            <div 
              v-for="week in weekHeaders" 
              :key="week.weekNum + '-' + week.startIdx"
              class="tl-week-cell"
              :style="{ width: (week.span * cellWidth) + 'px' }"
            >
              <span class="week-month">{{ week.monthLabel }}</span>
              <span class="week-label">{{ week.label }}</span>
            </div>
          </div>

          <!-- Day headers -->
          <div class="tl-day-row">
            <div 
              v-for="(day, idx) in dayColumns" 
              :key="idx"
              class="tl-day-cell"
              :class="{ 
                'is-today': formatDayHeader(day).isToday,
                'is-weekend': formatDayHeader(day).isWeekend
              }"
              :style="{ width: cellWidth + 'px' }"
            >
              <span class="day-num">{{ formatDayHeader(day).dayNum }}</span>
              <span class="day-dow">{{ formatDayHeader(day).dayOfWeek }}</span>
            </div>
          </div>

          <!-- Task bars layer -->
          <div class="tl-bars-container">
            <!-- Grid lines (day columns) -->
            <div class="tl-grid-lines">
              <div 
                v-for="(day, idx) in dayColumns" 
                :key="idx"
                class="tl-grid-line"
                :class="{ 
                  'is-today': formatDayHeader(day).isToday,
                  'is-weekend': formatDayHeader(day).isWeekend
                }"
                :style="{ left: (idx * cellWidth) + 'px', width: cellWidth + 'px' }"
              ></div>
            </div>

            <!-- Today line -->
            <div class="today-line" :style="{ left: todayOffset + 'px' }"></div>

            <!-- Task bar rows -->
            <div v-for="task in tasks" :key="task.id" class="tl-bar-row">
              <div 
                v-if="getTaskBar(task)"
                class="tl-task-bar"
                :style="{ 
                  left: getTaskBar(task).left, 
                  width: getTaskBar(task).width,
                  background: getStatusColor(task.statusName)
                }"
                :title="`${task.title} (${getTaskDuration(task)})`"
                @click="$emit('open-task', task)"
              >
                <span class="bar-label">{{ task.title }}</span>
              </div>
            </div>
            <!-- Empty add row -->
            <div class="tl-bar-row"></div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.plane-timeline {
  display: flex;
  flex-direction: column;
  height: 100%;
  min-height: calc(100vh - 180px);
  background: #0D0F11;
  color: #E4E4E7;
  overflow: hidden;
}

/* ── Header ── */
.tl-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px 20px;
  border-bottom: 1px solid #1E2025;
  flex-shrink: 0;
}
.tl-header-left {
  display: flex;
  align-items: center;
  gap: 16px;
}
.tl-task-count {
  font-size: 13px;
  font-weight: 500;
  color: var(--text-secondary);
}
.tl-view-modes {
  display: flex;
  gap: 2px;
  background: var(--bg-card);
  border-radius: 6px;
  padding: 2px;
  border: 1px solid var(--border-color);
}
.mode-btn {
  padding: 4px 12px;
  border: none;
  background: transparent;
  color: var(--text-secondary);
  font-size: 12px;
  font-weight: 500;
  border-radius: 4px;
  cursor: pointer;
  transition: all 0.15s;
}
.mode-btn.active {
  background: var(--hover-bg);
  color: var(--text-primary);
  font-weight: 600;
}
.mode-btn:hover:not(.active) {
  color: var(--text-primary);
}
.tl-header-right {
  display: flex;
  gap: 8px;
}
.tl-btn {
  padding: 5px 12px;
  border: 1px solid var(--border-color);
  border-radius: 4px;
  background: transparent;
  color: var(--text-secondary);
  font-size: 12px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.15s;
}
.tl-btn:hover {
  background: var(--hover-bg);
  color: var(--text-primary);
}
.expand-btn {
  padding: 5px 8px;
}

/* ── Body ── */
.tl-body {
  display: flex;
  flex: 1;
  overflow: hidden;
}

/* Left panel */
.tl-left-panel {
  width: 320px;
  min-width: 320px;
  border-right: 2px solid #1E2025;
  display: flex;
  flex-direction: column;
  flex-shrink: 0;
}
.tl-left-header {
  display: flex;
  height: 72px; /* Match week + day header height */
  border-bottom: 1px solid #1E2025;
  background: transparent;
}
.tl-col-workitems {
  flex: 1;
  padding: 0 12px;
  display: flex;
  align-items: center;
  font-size: 12px;
  font-weight: 600;
  color: var(--text-muted);
  text-transform: uppercase;
  letter-spacing: 0.3px;
  gap: 8px;
  overflow: hidden;
}
.tl-col-duration {
  width: 70px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 12px;
  font-weight: 600;
  color: var(--text-muted);
  text-transform: uppercase;
  letter-spacing: 0.3px;
  border-left: 1px solid var(--border-color);
}
.tl-left-rows {
  flex: 1;
  overflow-y: auto;
}
.tl-task-row {
  display: flex;
  height: 42px;
  border-bottom: 1px solid var(--border-color);
  cursor: pointer;
  transition: background 0.12s;
}
.tl-task-row:hover {
  background: var(--hover-bg);
}
.tl-task-row .tl-col-workitems {
  font-weight: 400;
  font-size: 13px;
  color: var(--text-primary);
  text-transform: none;
  letter-spacing: 0;
  gap: 6px;
}
.tl-task-row .tl-col-duration {
  font-weight: 400;
  font-size: 12px;
  color: var(--text-muted);
  text-transform: none;
  letter-spacing: 0;
}
.task-key {
  color: var(--text-muted);
  font-size: 12px;
  font-weight: 500;
  flex-shrink: 0;
}
.task-title-text {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.task-emoji {
  font-size: 14px;
  flex-shrink: 0;
}
.tl-add-row {
  cursor: pointer;
  border-bottom: none;
}
.tl-add-row:hover {
  background: var(--hover-bg);
}
.add-text {
  color: var(--text-muted);
  font-size: 13px;
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 0 12px;
}

/* Right panel */
.tl-right-panel {
  flex: 1;
  overflow-x: auto;
  overflow-y: auto;
}
.tl-gantt {
  min-height: 100%;
  position: relative;
}

/* Week row */
.tl-week-row {
  display: flex;
  height: 32px;
  border-bottom: 1px solid var(--border-color);
  background: var(--bg-card);
  position: sticky;
  top: 0;
  z-index: 3;
}
.tl-week-cell {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 8px;
  font-size: 11px;
  color: var(--text-muted);
  border-right: 1px solid var(--border-color);
  overflow: hidden;
  white-space: nowrap;
}
.week-month {
  font-weight: 600;
  color: var(--text-secondary);
}
.week-label {
  font-weight: 400;
}

/* Day row */
.tl-day-row {
  display: flex;
  height: 40px;
  border-bottom: 1px solid var(--border-color);
  background: var(--bg-card);
  position: sticky;
  top: 32px;
  z-index: 3;
}
.tl-day-cell {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  border-right: 1px solid var(--border-color);
  gap: 1px;
}
.tl-day-cell .day-num {
  font-size: 12px;
  font-weight: 500;
  color: var(--text-secondary);
  line-height: 1;
}
.tl-day-cell .day-dow {
  font-size: 10px;
  color: var(--text-muted);
  line-height: 1;
}
.tl-day-cell.is-today .day-num {
  background: #3b82f6;
  color: white;
  width: 22px;
  height: 22px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
}
.tl-day-cell.is-weekend {
  background: rgba(100, 116, 139, 0.05);
}

/* Bars container */
.tl-bars-container {
  position: relative;
}
.tl-grid-lines {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  pointer-events: none;
  z-index: 0;
}
.tl-grid-line {
  position: absolute;
  top: 0;
  bottom: 0;
  border-right: 1px solid var(--border-color);
}
.tl-grid-line.is-weekend {
  background: rgba(100, 116, 139, 0.04);
}
.tl-grid-line.is-today {
  background: rgba(59, 130, 246, 0.08);
}

/* Today line */
.today-line {
  position: absolute;
  top: 0;
  bottom: 0;
  width: 2px;
  background: #3b82f6;
  z-index: 2;
  opacity: 0.8;
}

/* Task bar rows */
.tl-bar-row {
  height: 42px;
  position: relative;
  border-bottom: 1px solid var(--border-color);
  z-index: 1;
}
.tl-task-bar {
  position: absolute;
  top: 8px;
  height: 26px;
  border-radius: 4px;
  display: flex;
  align-items: center;
  padding: 0 8px;
  cursor: pointer;
  transition: filter 0.15s, box-shadow 0.15s;
  box-shadow: 0 1px 3px rgba(0,0,0,0.15);
  min-width: 20px;
}
.tl-task-bar:hover {
  filter: brightness(1.15);
  box-shadow: 0 2px 8px rgba(0,0,0,0.25);
}
.bar-label {
  font-size: 11px;
  font-weight: 500;
  color: white;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
</style>
