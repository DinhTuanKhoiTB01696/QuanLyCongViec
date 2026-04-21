<script setup>
import { computed, onMounted, onUnmounted, ref, watch } from 'vue'
import { ElMessage } from 'element-plus'
import { useWorkTaskStore } from '@/store/useWorkTaskStore'
import axiosClient from '@/api/axiosClient'

const props = defineProps({
  projectId: { type: String, required: true },
  tasks: { type: Array, default: null }
})

const emit = defineEmits(['open-task', 'create-task'])

const taskStore = useWorkTaskStore()
const sourceTasks = computed(() => props.tasks || taskStore.tasks)
const loading = computed(() => taskStore.loading)
const today = ref(new Date())
const viewMode = ref('Week')
const showOptions = ref(false)
const createMode = ref(false)
const scrollContainer = ref(null)
const dragState = ref(null)
const clickedBucket = ref(null)
const timelineAnchorDate = ref(new Date())
const expanded = ref({
  showOnlyScheduled: false,
  hideDone: false,
  onlyCurrentWindow: false
})

const viewModes = [
  { key: 'Week', unit: 'day', cellWidth: 84 },
  { key: 'Month', unit: 'day', cellWidth: 42 },
  { key: 'Quarter', unit: 'week', cellWidth: 92 }
]

const activeView = computed(() => viewModes.find(mode => mode.key === viewMode.value) || viewModes[0])
const preferenceKey = computed(() => `timeline:display:${props.projectId || 'default'}`)

const timelineRange = computed(() => {
  const start = startOfDay(timelineAnchorDate.value)
  const end = startOfDay(timelineAnchorDate.value)

  if (viewMode.value === 'Week') {
    const currentWeekStart = startOfWeek(timelineAnchorDate.value)
    start.setTime(currentWeekStart.getTime())
    end.setTime(currentWeekStart.getTime())
    end.setDate(end.getDate() + 13)
  } else if (viewMode.value === 'Month') {
    start.setDate(1)
    end.setFullYear(start.getFullYear(), start.getMonth() + 1, 0)
  } else {
    const quarterStartMonth = Math.floor(start.getMonth() / 3) * 3
    start.setMonth(quarterStartMonth, 1)
    end.setFullYear(start.getFullYear(), quarterStartMonth + 3, 0)
  }

  return { start, end: endOfDay(end) }
})

const timeBuckets = computed(() => buildBuckets(timelineRange.value.start, timelineRange.value.end, activeView.value.unit))
const cellWidth = computed(() => activeView.value.cellWidth)
const totalWidth = computed(() => timeBuckets.value.length * cellWidth.value)

const headerGroups = computed(() => {
  const groups = []
  let current = null

  timeBuckets.value.forEach((bucket, index) => {
    if (!current || current.label !== bucket.groupLabel) {
      current = { label: bucket.groupLabel, span: 1, startIndex: index }
      groups.push(current)
    } else {
      current.span += 1
    }
  })

  return groups
})

const visibleTasks = computed(() => {
  return sourceTasks.value
    .filter(task => {
      const status = `${task.statusName || ''}`.toUpperCase()
      if (expanded.value.hideDone && status.includes('DONE')) return false

      const windowInfo = getTaskWindow(task)
      if (expanded.value.showOnlyScheduled && !windowInfo) return false
      if (expanded.value.onlyCurrentWindow && windowInfo && !rangesOverlap(windowInfo.start, windowInfo.end, timelineRange.value.start, timelineRange.value.end)) {
        return false
      }

      return true
    })
    .sort((left, right) => (Number(left.sortOrder) || 0) - (Number(right.sortOrder) || 0))
})

const bucketProgress = computed(() => {
  return timeBuckets.value.map(bucket => {
    const overlapping = visibleTasks.value.filter(task => {
      const windowInfo = getTaskWindow(task)
      return windowInfo && rangesOverlap(windowInfo.start, windowInfo.end, bucket.start, bucket.end)
    })

    const done = overlapping.filter(task => `${task.statusName || ''}`.toUpperCase().includes('DONE')).length
    const percent = overlapping.length ? Math.round((done / overlapping.length) * 100) : 0
    return { total: overlapping.length, done, percent }
  })
})

const todayOffset = computed(() => {
  const todayBucketIndex = timeBuckets.value.findIndex(bucket => containsDay(bucket.start, bucket.end, today.value))
  return todayBucketIndex < 0 ? 0 : (todayBucketIndex * cellWidth.value) + (cellWidth.value / 2)
})

const fetchTasks = () => {
  if (!props.tasks && props.projectId) {
    taskStore.fetchTasks(props.projectId)
  }
}

const goToToday = () => {
  today.value = new Date()
  timelineAnchorDate.value = new Date()
  if (!scrollContainer.value) return
  requestAnimationFrame(() => {
    scrollContainer.value.scrollLeft = Math.max(0, todayOffset.value - (scrollContainer.value.clientWidth * 0.45))
  })
}

const taskDurationLabel = (task) => {
  const windowInfo = getTaskWindow(task)
  if (!windowInfo) return '-'

  const days = Math.max(1, diffInDays(startOfDay(windowInfo.start), startOfDay(windowInfo.end)) + 1)
  if (days >= 30) return `${Math.round(days / 30)}mo`
  if (days >= 7) return `${Math.round(days / 7)}w`
  return `${days}d`
}

const getTaskBar = (task) => {
  const windowInfo = getTaskWindow(task)
  if (!windowInfo) return null

  let first = -1
  let last = -1

  timeBuckets.value.forEach((bucket, index) => {
    if (rangesOverlap(windowInfo.start, windowInfo.end, bucket.start, bucket.end)) {
      if (first === -1) first = index
      last = index
    }
  })

  if (first === -1 || last === -1) return null

  return {
    left: `${first * cellWidth.value}px`,
    width: `${Math.max(cellWidth.value, (last - first + 1) * cellWidth.value)}px`
  }
}

const getStatusColor = (statusName) => {
  const normalized = `${statusName || ''}`.toUpperCase()
  if (normalized.includes('DONE') || normalized.includes('COMPLETE')) return '#16a34a'
  if (normalized.includes('PROGRESS')) return '#2563eb'
  if (normalized.includes('REVIEW')) return '#f59e0b'
  if (normalized.includes('TODO')) return '#8b5cf6'
  return '#64748b'
}

const getTaskIcon = (task) => {
  if (task.priority === 1) return '!!'
  if (task.priority === 2) return '!'
  if (task.priority === 3) return '='
  return '.'
}

const requestQuickAdd = (bucket = null) => {
  clickedBucket.value = bucket
  emit('create-task', bucket
    ? {
        plannedStartDate: formatDateOnly(bucket.start),
        dueDate: formatDateOnly(bucket.end)
      }
    : {
        plannedStartDate: null,
        dueDate: null
      })
}

const toggleCreateMode = () => {
  createMode.value = !createMode.value
  clickedBucket.value = null
  if (createMode.value) {
    ElMessage.info('Create mode dang bat. Click vao timeline de them work item nhanh.')
  }
}

const handleTimelineCanvasClick = (bucket) => {
  if (!createMode.value) return
  requestQuickAdd(bucket)
}

const handleBarClick = (task) => {
  if (dragState.value?.moved) return
  emit('open-task', task)
}

const onDragStart = (event, task, type) => {
  event.preventDefault()
  event.stopPropagation()

  dragState.value = {
    task,
    type,
    startX: event.clientX,
    moved: false
  }

  document.addEventListener('mousemove', onMouseMove)
  document.addEventListener('mouseup', onMouseUp)
}

const onMouseMove = (event) => {
  if (!dragState.value) return
  if (Math.abs(event.clientX - dragState.value.startX) > 4) {
    dragState.value.moved = true
  }
}

const onMouseUp = async (event) => {
  if (!dragState.value) return

  const current = dragState.value
  dragState.value = null
  document.removeEventListener('mousemove', onMouseMove)
  document.removeEventListener('mouseup', onMouseUp)

  const stepsDiff = Math.round((event.clientX - current.startX) / cellWidth.value)

  if (stepsDiff === 0) return

  const task = current.task
  const originalStart = task.plannedStartDate
  const originalEnd = task.dueDate || task.plannedEndDate
  const startDate = parseTaskDate(task.plannedStartDate) || parseTaskDate(task.createdAt) || startOfDay(today.value)
  const endDate = parseTaskDate(task.plannedEndDate || task.dueDate) || new Date(startDate)

  if (current.type === 'move') {
    moveDateByView(startDate, stepsDiff)
    moveDateByView(endDate, stepsDiff)
  } else if (current.type === 'resize-left') {
    moveDateByView(startDate, stepsDiff)
  } else if (current.type === 'resize-right') {
    moveDateByView(endDate, stepsDiff)
  }

  if (startDate > endDate) {
    ElMessage.warning('Khoang thoi gian khong hop le.')
    return
  }

  task.plannedStartDate = formatDateOnly(startDate)
  task.dueDate = formatDateOnly(endDate)

  try {
    await axiosClient.put(`/projects/${task.projectId}/WorkTasks/${task.id}`, {
      ...task,
      plannedStartDate: task.plannedStartDate,
      dueDate: task.dueDate
    })
  } catch (error) {
    task.plannedStartDate = originalStart
    task.dueDate = originalEnd
    ElMessage.error(error.response?.data?.message || 'Khong cap nhat duoc timeline.')
  }
}

const shiftTimeline = (direction) => {
  const next = new Date(timelineAnchorDate.value)
  if (viewMode.value === 'Week') {
    next.setDate(next.getDate() + (direction * 7))
  } else if (viewMode.value === 'Month') {
    next.setMonth(next.getMonth() + direction)
  } else {
    next.setMonth(next.getMonth() + (direction * 3))
  }
  timelineAnchorDate.value = next
}

watch(() => props.projectId, fetchTasks, { immediate: true })
watch(() => props.projectId, () => {
  try {
    const saved = localStorage.getItem(preferenceKey.value)
    if (!saved) {
      expanded.value = {
        showOnlyScheduled: false,
        hideDone: false,
        onlyCurrentWindow: false
      }
      return
    }

    const parsed = JSON.parse(saved)
    expanded.value = {
      showOnlyScheduled: Boolean(parsed.showOnlyScheduled),
      hideDone: Boolean(parsed.hideDone),
      onlyCurrentWindow: Boolean(parsed.onlyCurrentWindow)
    }
  } catch {
    expanded.value = {
      showOnlyScheduled: false,
      hideDone: false,
      onlyCurrentWindow: false
    }
  }
}, { immediate: true })
watch(viewMode, () => {
  window.setTimeout(goToToday, 60)
})
watch(expanded, (value) => {
  localStorage.setItem(preferenceKey.value, JSON.stringify(value))
}, { deep: true })

onMounted(() => {
  window.setTimeout(goToToday, 120)
})

onUnmounted(() => {
  document.removeEventListener('mousemove', onMouseMove)
  document.removeEventListener('mouseup', onMouseUp)
})

function buildBuckets(start, end, unit) {
  const buckets = []
  const cursor = new Date(start)

  while (cursor <= end) {
    const bucketStart = startOfDay(cursor)
    let bucketEnd
    let label = ''
    let subLabel = ''
    let groupLabel = ''

    if (unit === 'day') {
      bucketEnd = endOfDay(cursor)
      label = `${bucketStart.getDate()}`
      subLabel = ['Su', 'M', 'T', 'W', 'Th', 'F', 'Sa'][bucketStart.getDay()]
      groupLabel = bucketStart.toLocaleString('en-US', { month: 'short', year: 'numeric' })
      cursor.setDate(cursor.getDate() + 1)
    } else if (unit === 'week') {
      const normalizedWeekStart = startOfWeek(bucketStart)
      bucketEnd = endOfDay(addDays(normalizedWeekStart, 6))
      label = `W${getWeekNumber(normalizedWeekStart)}`
      subLabel = `${normalizedWeekStart.getDate()}-${bucketEnd.getDate()}`
      groupLabel = normalizedWeekStart.toLocaleString('en-US', { month: 'short', year: 'numeric' })
      cursor.setDate(cursor.getDate() + 7)
    } else {
      bucketEnd = endOfDay(new Date(bucketStart.getFullYear(), bucketStart.getMonth() + 1, 0))
      label = bucketStart.toLocaleString('en-US', { month: 'short' })
      subLabel = `${bucketStart.getFullYear()}`
      groupLabel = `${Math.floor(bucketStart.getMonth() / 3) + 1} / ${bucketStart.getFullYear()}`
      cursor.setMonth(cursor.getMonth() + 1, 1)
    }

    buckets.push({
      start: bucketStart,
      end: bucketEnd > end ? endOfDay(end) : bucketEnd,
      label,
      subLabel,
      groupLabel
    })
  }

  return buckets
}

function getTaskWindow(task) {
  let start = parseTaskDate(task.plannedStartDate)
  let end = parseTaskDate(task.plannedEndDate || task.dueDate)

  if (!start && !end) {
    return null
  }

  if (!start && end) start = startOfDay(end)
  if (start && !end) end = endOfDay(start)

  return {
    start: startOfDay(start),
    end: endOfDay(end)
  }
}

function startOfDay(value) {
  const date = new Date(value)
  date.setHours(0, 0, 0, 0)
  return date
}

function endOfDay(value) {
  const date = new Date(value)
  date.setHours(23, 59, 59, 999)
  return date
}

function addDays(value, amount) {
  const date = new Date(value)
  date.setDate(date.getDate() + amount)
  return date
}

function startOfWeek(value) {
  const date = startOfDay(value)
  const day = (date.getDay() + 6) % 7
  date.setDate(date.getDate() - day)
  return date
}

function parseTaskDate(value) {
  if (!value) return null
  if (value instanceof Date) return new Date(value)
  if (typeof value === 'string' && /^\d{4}-\d{2}-\d{2}$/.test(value)) {
    const [year, month, day] = value.split('-').map(Number)
    return new Date(year, month - 1, day)
  }

  if (typeof value === 'string' && /^\d{4}-\d{2}-\d{2}T/.test(value)) {
    const dateOnly = value.slice(0, 10)
    const [year, month, day] = dateOnly.split('-').map(Number)
    return new Date(year, month - 1, day)
  }

  const parsed = new Date(value)
  return Number.isNaN(parsed.getTime()) ? null : parsed
}

function formatDateOnly(value) {
  const parsed = parseTaskDate(value)
  const date = startOfDay(parsed || value)
  const year = date.getFullYear()
  const month = `${date.getMonth() + 1}`.padStart(2, '0')
  const day = `${date.getDate()}`.padStart(2, '0')
  return `${year}-${month}-${day}`
}

function moveDateByView(date, steps) {
  if (viewMode.value === 'Week') {
    date.setDate(date.getDate() + steps)
    return
  }

  if (viewMode.value === 'Month') {
    date.setDate(date.getDate() + steps)
    return
  }

  date.setDate(date.getDate() + (steps * 7))
}

function diffInDays(left, right) {
  return Math.round((right - left) / 86400000)
}

function containsDay(start, end, value) {
  const day = startOfDay(value).getTime()
  return day >= startOfDay(start).getTime() && day <= startOfDay(end).getTime()
}

function rangesOverlap(leftStart, leftEnd, rightStart, rightEnd) {
  return leftStart <= rightEnd && leftEnd >= rightStart
}

function getWeekNumber(value) {
  const date = new Date(Date.UTC(value.getFullYear(), value.getMonth(), value.getDate()))
  const dayNum = date.getUTCDay() || 7
  date.setUTCDate(date.getUTCDate() + 4 - dayNum)
  const yearStart = new Date(Date.UTC(date.getUTCFullYear(), 0, 1))
  return Math.ceil((((date - yearStart) / 86400000) + 1) / 7)
}
</script>

<template>
  <div class="plane-timeline" v-loading="loading">
    <div class="tl-header">
      <div class="tl-header-left">
        <span class="tl-task-count">{{ visibleTasks.length }} Work Items</span>
        <div class="tl-view-modes">
          <button
            v-for="mode in viewModes"
            :key="mode.key"
            class="mode-btn"
            :class="{ active: viewMode === mode.key }"
            @click="viewMode = mode.key"
          >{{ mode.key }}</button>
        </div>
        <div class="tl-nav-actions">
          <button class="tl-btn" type="button" @click="shiftTimeline(-1)"><i class="fa-solid fa-chevron-left"></i></button>
          <button class="tl-btn" type="button" @click="shiftTimeline(1)"><i class="fa-solid fa-chevron-right"></i></button>
        </div>
      </div>

      <div class="tl-header-right">
        <div class="display-options">
          <button class="tl-btn" type="button" @click="showOptions = !showOptions">
            Display options
          </button>
          <div v-if="showOptions" class="display-menu">
            <label class="option-row"><input v-model="expanded.showOnlyScheduled" type="checkbox" /> Only scheduled items</label>
            <label class="option-row"><input v-model="expanded.hideDone" type="checkbox" /> Hide done items</label>
            <label class="option-row"><input v-model="expanded.onlyCurrentWindow" type="checkbox" /> Focus current window</label>
          </div>
        </div>
        <button class="tl-btn" type="button" :class="{ active: createMode }" @click="toggleCreateMode">Create mode</button>
        <button class="tl-btn" type="button" @click="requestQuickAdd()">New Work Item</button>
        <button class="tl-btn" type="button" @click="goToToday">Today</button>
      </div>
    </div>

    <div v-if="createMode" class="create-mode-banner">
      <i class="fa-solid fa-wand-magic-sparkles"></i>
      <span>Click any timeline cell to create a work item with start and due date prefilled.</span>
    </div>

    <div class="tl-body">
      <div class="tl-left-panel">
        <div class="tl-left-header">
          <div class="tl-col-workitems">Work Items</div>
          <div class="tl-col-duration">Duration</div>
        </div>

        <div class="tl-left-rows">
          <div
            v-for="task in visibleTasks"
            :key="task.id"
            class="tl-task-row"
            @click="emit('open-task', task)"
          >
            <div class="tl-col-workitems">
              <span class="task-key">{{ task.sequenceId || task.id?.substring(0, 8)?.toUpperCase() }}</span>
              <span class="task-title-text">{{ task.title }}</span>
              <span class="task-emoji">{{ getTaskIcon(task) }}</span>
            </div>
            <div class="tl-col-duration">{{ taskDurationLabel(task) }}</div>
          </div>

          <button class="tl-task-row tl-add-row" type="button" @click="requestQuickAdd()">
            <span class="add-text"><i class="fa-solid fa-plus"></i> New work item</span>
          </button>
        </div>
      </div>

      <div class="tl-right-panel" ref="scrollContainer">
        <div v-if="createMode" class="tl-create-banner">
          <i class="fa-solid fa-wand-magic-sparkles"></i>
          Click vao o thoi gian trong timeline de tao work item voi ngay bat dau va ket thuc tai vi tri da chon.
        </div>
        <div class="tl-gantt" :style="{ width: `${totalWidth}px` }">
          <div class="tl-group-row">
            <div
              v-for="group in headerGroups"
              :key="`${group.label}-${group.startIndex}`"
              class="tl-group-cell"
              :style="{ width: `${group.span * cellWidth}px` }"
            >
              {{ group.label }}
            </div>
          </div>

          <div class="tl-day-row">
            <button
              v-for="(bucket, index) in timeBuckets"
              :key="`${bucket.label}-${index}`"
              type="button"
              class="tl-day-cell"
              :class="{ 'is-today': containsDay(bucket.start, bucket.end, today.value), 'create-enabled': createMode, 'bucket-selected': clickedBucket && formatDateOnly(clickedBucket.start) === formatDateOnly(bucket.start) }"
              :style="{ width: `${cellWidth}px` }"
              @click="handleTimelineCanvasClick(bucket)"
            >
              <span class="day-num">{{ bucket.label }}</span>
              <span class="day-dow">{{ bucket.subLabel }}</span>
              <span v-if="bucketProgress[index].total" class="bucket-progress">{{ bucketProgress[index].percent }}%</span>
            </button>
          </div>

          <div class="tl-bars-container">
            <div class="tl-grid-lines">
              <button
                v-for="(bucket, index) in timeBuckets"
                :key="`grid-${index}`"
                type="button"
                class="tl-grid-line"
                :class="{ 'is-today': containsDay(bucket.start, bucket.end, today.value), 'create-active': createMode }"
                :style="{ left: `${index * cellWidth}px`, width: `${cellWidth}px` }"
                @click="handleTimelineCanvasClick(bucket)"
              ></button>
            </div>

            <div class="today-line" :style="{ left: `${todayOffset}px` }"></div>

            <div v-for="task in visibleTasks" :key="`row-${task.id}`" class="tl-bar-row">
              <div
                v-if="getTaskBar(task)"
                class="tl-task-bar"
                :style="{
                  left: getTaskBar(task).left,
                  width: getTaskBar(task).width,
                  background: getStatusColor(task.statusName)
                }"
                :title="`${task.title} (${taskDurationLabel(task)})`"
                @click.stop="handleBarClick(task)"
                @mousedown="onDragStart($event, task, 'move')"
              >
                <div class="resize-handle left" @mousedown.stop="onDragStart($event, task, 'resize-left')"></div>
                <span class="bar-label">{{ task.title }}</span>
                <div class="resize-handle right" @mousedown.stop="onDragStart($event, task, 'resize-right')"></div>
              </div>
            </div>

            <button class="tl-bar-row tl-add-canvas-row" type="button" @click="requestQuickAdd(clickedBucket)">
              <span class="canvas-add-label">Click de them work item moi</span>
            </button>
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
  min-height: calc(100vh - 80px);
  background: #0d0f11;
  color: #e4e4e7;
  overflow: hidden;
}

.tl-header,
.tl-header-left,
.tl-header-right,
.tl-view-modes {
  display: flex;
  align-items: center;
}

.tl-header {
  justify-content: space-between;
  gap: 16px;
  padding: 14px 24px;
  border-bottom: 1px solid #1e2025;
}

.create-mode-banner {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 10px 24px;
  border-bottom: 1px solid #1e2025;
  background: linear-gradient(90deg, rgba(14, 165, 233, 0.16), rgba(59, 130, 246, 0.06));
  color: #bae6fd;
  font-size: 13px;
}

.tl-header-left,
.tl-header-right {
  gap: 12px;
}

.tl-nav-actions {
  display: flex;
  gap: 8px;
}

.tl-task-count {
  font-size: 13px;
  color: #a1a1aa;
}

.tl-view-modes {
  gap: 4px;
  background: #15181c;
  border: 1px solid #27272a;
  border-radius: 6px;
  padding: 2px;
}

.mode-btn,
.tl-btn {
  border: 0;
  border-radius: 4px;
  background: transparent;
  color: #a1a1aa;
  cursor: pointer;
}

.mode-btn {
  padding: 5px 12px;
  font-size: 12px;
}

.tl-btn {
  padding: 6px 12px;
  border: 1px solid #27272a;
  background: #111317;
  font-size: 12px;
}

.mode-btn.active,
.tl-btn.active {
  background: #1f2937;
  color: #ffffff;
}

.display-options {
  position: relative;
}

.display-menu {
  position: absolute;
  top: calc(100% + 8px);
  right: 0;
  z-index: 12;
  min-width: 220px;
  border: 1px solid #27272a;
  border-radius: 8px;
  background: #111317;
  padding: 10px;
  box-shadow: 0 12px 30px rgba(0, 0, 0, 0.35);
}

.option-row {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 13px;
  color: #d4d4d8;
  padding: 6px 0;
}

.tl-body {
  display: flex;
  flex: 1;
  min-height: 0;
  overflow: auto;
}

.tl-left-panel {
  width: 440px;
  min-width: 440px;
  border-right: 2px solid #1e2025;
  display: flex;
  flex-direction: column;
  min-height: 0;
  background: #0d0f11;
  position: sticky;
  left: 0;
  z-index: 5;
}

.tl-left-header {
  display: flex;
  height: 88px;
  border-bottom: 1px solid #1e2025;
}

.tl-col-workitems,
.tl-col-duration {
  display: flex;
  align-items: center;
  font-size: 12px;
}

.tl-col-workitems {
  flex: 1;
  gap: 8px;
  padding: 0 12px;
  color: #a1a1aa;
  font-weight: 600;
  text-transform: uppercase;
}

.tl-col-duration {
  width: 96px;
  justify-content: center;
  border-left: 1px solid #27272a;
  color: #71717a;
  text-transform: uppercase;
  font-weight: 600;
}

.tl-left-rows {
  flex: 1;
  overflow-y: auto;
}

.tl-task-row {
  width: 100%;
  height: 52px;
  display: flex;
  border: 0;
  border-bottom: 1px solid #1e2025;
  background: transparent;
  color: inherit;
  cursor: pointer;
  text-align: left;
}

.tl-task-row:hover {
  background: #16181d;
}

.tl-task-row .tl-col-workitems,
.tl-task-row .tl-col-duration {
  text-transform: none;
  font-weight: 400;
}

.task-key {
  color: #71717a;
  font-size: 12px;
  min-width: 86px;
}

.task-title-text {
  overflow: hidden;
  white-space: nowrap;
  text-overflow: ellipsis;
}

.task-emoji {
  margin-left: auto;
  color: #94a3b8;
}

.tl-add-row {
  align-items: center;
}

.add-text {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 0 12px;
  color: #a1a1aa;
  font-size: 13px;
}

.tl-right-panel {
  flex: 1;
  min-height: 0;
  overflow: auto;
}

.tl-gantt {
  min-height: 100%;
  position: relative;
  padding-bottom: 24px;
}

.tl-create-banner {
  position: sticky;
  top: 0;
  z-index: 6;
  margin: 0 0 8px;
  padding: 10px 14px;
  border-bottom: 1px solid #1e2025;
  background: linear-gradient(90deg, rgba(56, 189, 248, 0.12), rgba(37, 99, 235, 0.08));
  color: #c4e7ff;
  font-size: 12px;
  display: flex;
  align-items: center;
  gap: 8px;
}

.tl-group-row,
.tl-day-row {
  display: flex;
  position: sticky;
  z-index: 4;
  background: #101216;
}

.tl-group-row {
  top: 0;
  height: 34px;
  border-bottom: 1px solid #1e2025;
}

.tl-day-row {
  top: 34px;
  height: 48px;
  border-bottom: 1px solid #1e2025;
}

.tl-group-cell,
.tl-day-cell {
  border-right: 1px solid #1e2025;
}

.tl-group-cell {
  display: flex;
  align-items: center;
  padding: 0 10px;
  color: #a1a1aa;
  font-size: 11px;
  font-weight: 600;
}

.tl-day-cell {
  border: 0;
  background: transparent;
  color: #d4d4d8;
  display: flex;
  flex-direction: column;
  justify-content: center;
  gap: 3px;
  padding: 6px 4px;
}

.tl-day-cell.create-enabled:hover {
  background: rgba(56, 189, 248, 0.08);
}

.tl-day-cell.bucket-selected {
  background: rgba(56, 189, 248, 0.18);
}

.tl-day-cell.is-today {
  background: rgba(37, 99, 235, 0.12);
}

.day-num {
  font-size: 14px;
  font-weight: 600;
}

.day-dow,
.bucket-progress {
  font-size: 10px;
  color: #71717a;
}

.bucket-progress {
  color: #38bdf8;
}

.tl-bars-container {
  position: relative;
  min-height: 100%;
}

.tl-grid-lines {
  position: absolute;
  inset: 0;
  z-index: 0;
}

.tl-grid-line {
  position: absolute;
  top: 0;
  bottom: 0;
  border: 0;
  border-right: 1px solid #1e2025;
  background: transparent;
  cursor: default;
}

.tl-grid-line.create-active:hover {
  background: rgba(56, 189, 248, 0.08);
  cursor: crosshair;
}

.tl-grid-line.is-today {
  background: rgba(37, 99, 235, 0.08);
}

.today-line {
  position: absolute;
  top: 0;
  bottom: 0;
  width: 2px;
  background: #3b82f6;
  z-index: 2;
  box-shadow: 0 0 18px rgba(59, 130, 246, 0.4);
}

.tl-bar-row {
  position: relative;
  height: 52px;
  border-bottom: 1px solid #1e2025;
}

.tl-task-bar {
  position: absolute;
  top: 11px;
  height: 30px;
  border-radius: 8px;
  display: flex;
  align-items: center;
  padding: 0 12px;
  color: #ffffff;
  cursor: pointer;
  z-index: 3;
  box-shadow: 0 6px 16px rgba(0, 0, 0, 0.28);
}

.tl-task-bar:hover {
  filter: brightness(1.06);
}

.resize-handle {
  position: absolute;
  top: 0;
  bottom: 0;
  width: 8px;
  cursor: ew-resize;
}

.resize-handle.left { left: 0; }
.resize-handle.right { right: 0; }

.bar-label {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  font-size: 12px;
  font-weight: 600;
}

.tl-add-canvas-row {
  width: 100%;
  border: 0;
  background: transparent;
  color: #71717a;
  cursor: pointer;
}

.tl-add-canvas-row:hover {
  background: rgba(56, 189, 248, 0.05);
}

.canvas-add-label {
  position: absolute;
  left: 12px;
  top: 12px;
  font-size: 12px;
}
</style>
