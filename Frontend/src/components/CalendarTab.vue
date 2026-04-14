<script setup>
import { ref, onMounted, computed, watch } from 'vue'
import axiosClient from '@/api/axiosClient'

const props = defineProps({
  projectId: { type: String, required: true }
})

const emit = defineEmits(['open-task'])

const tasks = ref([])
const loading = ref(false)
const currentDate = ref(new Date())

// Navigation
const goToday = () => { currentDate.value = new Date() }
const prevMonth = () => {
  const d = new Date(currentDate.value)
  d.setMonth(d.getMonth() - 1)
  currentDate.value = d
}
const nextMonth = () => {
  const d = new Date(currentDate.value)
  d.setMonth(d.getMonth() + 1)
  currentDate.value = d
}

const monthLabel = computed(() => {
  const months = ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December']
  return `${months[currentDate.value.getMonth()]} ${currentDate.value.getFullYear()}`
})

const weekDays = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun']

// Build calendar grid (6 weeks)
const calendarDays = computed(() => {
  const year = currentDate.value.getFullYear()
  const month = currentDate.value.getMonth()
  const firstDay = new Date(year, month, 1)
  const lastDay = new Date(year, month + 1, 0)
  
  // Monday = 0, Sunday = 6 (ISO)
  let startDow = firstDay.getDay() - 1
  if (startDow < 0) startDow = 6
  
  const days = []
  // Previous month fill
  const prevLast = new Date(year, month, 0)
  for (let i = startDow - 1; i >= 0; i--) {
    days.push({
      date: new Date(year, month - 1, prevLast.getDate() - i),
      isCurrentMonth: false
    })
  }
  // Current month
  for (let d = 1; d <= lastDay.getDate(); d++) {
    days.push({
      date: new Date(year, month, d),
      isCurrentMonth: true
    })
  }
  // Next month fill (up to 42 cells = 6 weeks)
  const remaining = 42 - days.length
  for (let d = 1; d <= remaining; d++) {
    days.push({
      date: new Date(year, month + 1, d),
      isCurrentMonth: false
    })
  }
  return days
})

// Get tasks for a specific day - matches tasks whose date range overlaps with this day
const getTasksForDay = (date) => {
  const dayStart = new Date(date.getFullYear(), date.getMonth(), date.getDate())
  const dayEnd = new Date(dayStart)
  dayEnd.setDate(dayEnd.getDate() + 1)
  
  return tasks.value.filter(t => {
    const startDate = t.plannedStartDate ? new Date(t.plannedStartDate) : null
    const endDate = t.dueDate || t.plannedEndDate ? new Date(t.dueDate || t.plannedEndDate) : null
    
    // If task has a date range, show on all days within that range
    if (startDate && endDate) {
      const s = new Date(startDate.getFullYear(), startDate.getMonth(), startDate.getDate())
      const e = new Date(endDate.getFullYear(), endDate.getMonth(), endDate.getDate())
      e.setDate(e.getDate() + 1) // make end inclusive
      return dayStart >= s && dayStart < e
    }
    
    // If only one date, show on that specific day
    const singleDate = endDate || startDate
    if (!singleDate) return false
    const d = new Date(singleDate)
    return d.getFullYear() === date.getFullYear() && d.getMonth() === date.getMonth() && d.getDate() === date.getDate()
  })
}

const isToday = (date) => {
  const today = new Date()
  return date.getFullYear() === today.getFullYear() && date.getMonth() === today.getMonth() && date.getDate() === today.getDate()
}

const formatDayNum = (date) => {
  const day = date.getDate()
  // Show month name for 1st day
  if (day === 1) {
    const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec']
    return `${months[date.getMonth()]} ${day}`
  }
  return day
}

const getStatusColor = (statusName) => {
  const s = (statusName || '').toUpperCase()
  if (s.includes('DONE') || s.includes('COMPLETE')) return '#16a34a'
  if (s.includes('PROGRESS') || s.includes('REVIEW')) return '#3b82f6'
  if (s.includes('TODO')) return '#a855f7'
  return '#6b7280'
}

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

onMounted(fetchTasks)
</script>

<template>
  <div class="plane-calendar" v-loading="loading">
    <!-- Calendar Header -->
    <div class="cal-header">
      <div class="cal-nav">
        <button class="nav-btn" @click="prevMonth"><i class="fa-solid fa-chevron-left"></i></button>
        <button class="nav-btn" @click="nextMonth"><i class="fa-solid fa-chevron-right"></i></button>
        <h2 class="cal-month-label">{{ monthLabel }}</h2>
      </div>
      <div class="cal-actions">
        <button class="cal-btn" @click="goToday">Today</button>
        <button class="cal-btn">Options <i class="fa-solid fa-chevron-down"></i></button>
      </div>
    </div>

    <!-- Calendar Grid -->
    <div class="cal-grid">
      <!-- Day headers -->
      <div class="cal-day-header" v-for="d in weekDays" :key="d">{{ d }}</div>
      
      <!-- Day cells -->
      <div 
        class="cal-day-cell" 
        v-for="(day, idx) in calendarDays" 
        :key="idx"
        :class="{ 
          'other-month': !day.isCurrentMonth,
          'is-today': isToday(day.date)
        }"
      >
        <div class="day-number" :class="{ 'today-badge': isToday(day.date) }">
          {{ formatDayNum(day.date) }}
        </div>
        <div class="day-tasks">
          <div 
            v-for="task in getTasksForDay(day.date).slice(0, 2)" 
            :key="task.id" 
            class="day-task-chip"
            :style="{ borderLeft: `3px solid ${getStatusColor(task.statusName)}` }"
            @click="$emit('open-task', task)"
            :title="task.title"
          >
            <span class="chip-text">{{ task.title }}</span>
          </div>
          <div v-if="getTasksForDay(day.date).length > 2" class="day-more">
            + {{ getTasksForDay(day.date).length - 2 }} more
          </div>
        </div>
        <!-- Quick add -->
        <div class="day-add-btn" v-if="day.isCurrentMonth">
          <i class="fa-solid fa-plus"></i> Add work item
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.plane-calendar {
  display: flex;
  flex-direction: column;
  height: 100%;
  min-height: calc(100vh - 180px);
  background: #0D0F11;
  color: #E4E4E7;
}

/* ── Header ── */
.cal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 24px;
  border-bottom: 1px solid #1E2025;
}
.cal-nav {
  display: flex;
  align-items: center;
  gap: 8px;
}
.nav-btn {
  width: 28px;
  height: 28px;
  border-radius: 4px;
  border: 1px solid var(--border-color);
  background: transparent;
  color: var(--text-secondary);
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 12px;
  transition: all 0.15s;
}
.nav-btn:hover {
  background: var(--hover-bg);
  color: var(--text-primary);
}
.cal-month-label {
  margin: 0;
  font-size: 18px;
  font-weight: 600;
  color: var(--text-primary);
  margin-left: 8px;
}
.cal-actions {
  display: flex;
  gap: 8px;
}
.cal-btn {
  padding: 6px 14px;
  border-radius: 4px;
  border: 1px solid var(--border-color);
  background: transparent;
  color: var(--text-secondary);
  font-size: 13px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.15s;
}
.cal-btn:hover {
  background: var(--hover-bg);
  color: var(--text-primary);
}
.cal-btn i {
  margin-left: 4px;
  font-size: 10px;
}

/* ── Grid ── */
.cal-grid {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
  flex: 1;
  border-left: 1px solid #1E2025;
}

.cal-day-header {
  padding: 10px 12px;
  font-size: 12px;
  font-weight: 600;
  color: #71717A;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  border-bottom: 1px solid #1E2025;
  border-right: 1px solid #1E2025;
  text-align: right;
}

.cal-day-cell {
  min-height: 110px;
  border-bottom: 1px solid #1E2025;
  border-right: 1px solid #1E2025;
  padding: 6px 8px;
  display: flex;
  flex-direction: column;
  position: relative;
  transition: background 0.12s;
}
.cal-day-cell:hover {
  background: var(--hover-bg);
}
.cal-day-cell.other-month {
  opacity: 0.35;
}
.cal-day-cell.other-month:hover {
  opacity: 0.5;
}

.day-number {
  font-size: 13px;
  font-weight: 500;
  color: var(--text-secondary);
  text-align: right;
  margin-bottom: 4px;
  line-height: 1;
}
.day-number.today-badge {
  display: inline-flex;
  align-self: flex-end;
  width: 26px;
  height: 26px;
  border-radius: 50%;
  background: #3b82f6;
  color: white;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  font-size: 12px;
}

/* ── Tasks chips ── */
.day-tasks {
  display: flex;
  flex-direction: column;
  gap: 3px;
  flex: 1;
}
.day-task-chip {
  padding: 3px 6px;
  font-size: 11px;
  color: var(--text-primary);
  background: var(--bg-card);
  border-radius: 3px;
  cursor: pointer;
  transition: background 0.15s;
  overflow: hidden;
}
.day-task-chip:hover {
  background: var(--bg-hover);
}
.chip-text {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  display: block;
}
.day-more {
  font-size: 11px;
  color: var(--text-muted);
  padding: 2px 6px;
  cursor: pointer;
}
.day-more:hover {
  color: var(--text-primary);
}

/* ── Quick Add ── */
.day-add-btn {
  position: absolute;
  bottom: 4px;
  left: 4px;
  right: 4px;
  padding: 4px 6px;
  font-size: 11px;
  color: var(--text-muted);
  border-radius: 3px;
  cursor: pointer;
  opacity: 0;
  transition: opacity 0.2s;
  display: flex;
  align-items: center;
  gap: 4px;
}
.cal-day-cell:hover .day-add-btn {
  opacity: 1;
}
.day-add-btn:hover {
  background: var(--hover-bg);
  color: var(--text-primary);
}
</style>
