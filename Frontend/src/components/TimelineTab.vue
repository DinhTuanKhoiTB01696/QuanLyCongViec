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
const viewMode = ref('Day')
const showOptions = ref(false)
const createMode = ref(false)
const scrollContainer = ref(null)
const leftPanelRows = ref(null)
const dragState = ref(null)
const clickedBucket = ref(null)
const timelineAnchorDate = ref(new Date())
const viewportWidth = ref(0)
const expanded = ref({
  showOnlyScheduled: false,
  hideDone: false,
  onlyCurrentWindow: false
})

const viewModes = [
  { key: 'Day', unit: 'day', cellWidth: 64 },
  { key: 'Week', unit: 'week', cellWidth: 120 },
  { key: 'Month', unit: 'month', cellWidth: 140 },
  { key: 'Quarter', unit: 'quarter', cellWidth: 180 }
]

const activeView = computed(() => viewModes.find(mode => mode.key === viewMode.value) || viewModes[0])
const preferenceKey = computed(() => `timeline:display:${props.projectId || 'default'}`)

const timelineRange = computed(() => {
  const start = startOfDay(timelineAnchorDate.value)
  const end = startOfDay(timelineAnchorDate.value)

  if (viewMode.value === 'Day') {
    start.setTime(addDays(timelineAnchorDate.value, -10).getTime())
    end.setTime(addDays(timelineAnchorDate.value, 25).getTime())
  } else if (viewMode.value === 'Week') {
    const currentWeekStart = startOfWeek(timelineAnchorDate.value)
    start.setTime(addDays(currentWeekStart, -35).getTime())
    end.setTime(addDays(currentWeekStart, 48).getTime())
  } else if (viewMode.value === 'Month') {
    start.setFullYear(start.getFullYear(), start.getMonth() - 5, 1)
    end.setFullYear(start.getFullYear(), start.getMonth() + 11, 0)
  } else {
    const anchor = timelineAnchorDate.value
    const anchorQuarterStart = new Date(anchor.getFullYear(), Math.floor(anchor.getMonth() / 3) * 3, 1)
    const quarterStart = new Date(anchorQuarterStart)
    quarterStart.setMonth(quarterStart.getMonth() - 12)
    start.setTime(quarterStart.getTime())

    const quarterEnd = new Date(anchorQuarterStart)
    quarterEnd.setMonth(quarterEnd.getMonth() + 24)
    quarterEnd.setDate(0)
    end.setTime(quarterEnd.getTime())
  }

  return { start, end: endOfDay(end) }
})

const timeBuckets = computed(() => buildBuckets(timelineRange.value.start, timelineRange.value.end, activeView.value.unit))
const cellWidth = computed(() => activeView.value.cellWidth)
const totalWidth = computed(() => timeBuckets.value.length * cellWidth.value)
const canvasWidth = computed(() => Math.max(totalWidth.value, viewportWidth.value || 0))
const rowHeight = 44
const rowsCanvasHeight = computed(() => Math.max((visibleTasks.value.length + 1) * rowHeight, rowHeight * 8))

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
    ElMessage.info('Create mode is on. Click the timeline to add a work item quickly.')
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
    ElMessage.warning('The selected time range is invalid.')
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
    ElMessage.error(error.response?.data?.message || 'Could not update the timeline.')
  }
}

const syncScroll = (e) => {
  const { scrollTop } = e.target
  if (e.target === scrollContainer.value) {
    if (leftPanelRows.value) leftPanelRows.value.scrollTop = scrollTop
  } else {
    if (scrollContainer.value) scrollContainer.value.scrollTop = scrollTop
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
  updateViewportWidth()
  window.addEventListener('resize', updateViewportWidth)
  window.setTimeout(goToToday, 120)
  
  if (scrollContainer.value) scrollContainer.value.addEventListener('scroll', syncScroll)
  if (leftPanelRows.value) leftPanelRows.value.addEventListener('scroll', syncScroll)
})

onUnmounted(() => {
  document.removeEventListener('mousemove', onMouseMove)
  document.removeEventListener('mouseup', onMouseUp)
  window.removeEventListener('resize', updateViewportWidth)
  
  if (scrollContainer.value) scrollContainer.value.removeEventListener('scroll', syncScroll)
  if (leftPanelRows.value) leftPanelRows.value.removeEventListener('scroll', syncScroll)
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
      bucketEnd = endOfDay(bucketStart)
      label = `${bucketStart.getDate()}`
      const dayNames = ['Su', 'M', 'T', 'W', 'Th', 'F', 'Sa']
      subLabel = dayNames[bucketStart.getDay()]
      groupLabel = bucketStart.toLocaleString('en-US', { month: 'short', year: 'numeric' })
      cursor.setDate(cursor.getDate() + 1)
    } else if (unit === 'week') {
      const normalizedWeekStart = startOfWeek(bucketStart)
      bucketEnd = endOfDay(addDays(normalizedWeekStart, 6))
      label = `W${getWeekNumber(normalizedWeekStart)}`
      subLabel = `${normalizedWeekStart.toLocaleString('en-US', { month: 'short' })} ${normalizedWeekStart.getDate()} - ${bucketEnd.toLocaleString('en-US', { month: 'short' })} ${bucketEnd.getDate()}`
      groupLabel = normalizedWeekStart.toLocaleString('en-US', { month: 'short', year: 'numeric' })
      cursor.setDate(cursor.getDate() + 7)
    } else if (unit === 'month') {
      bucketEnd = endOfDay(new Date(bucketStart.getFullYear(), bucketStart.getMonth() + 1, 0))
      label = bucketStart.toLocaleString('en-US', { month: 'short' })
      subLabel = `${bucketStart.getFullYear()}`
      groupLabel = `${Math.floor(bucketStart.getMonth() / 3) + 1} / ${bucketStart.getFullYear()}`
      cursor.setMonth(cursor.getMonth() + 1, 1)
    } else {
      const quarterStart = new Date(bucketStart.getFullYear(), Math.floor(bucketStart.getMonth() / 3) * 3, 1)
      const quarterEnd = new Date(quarterStart.getFullYear(), quarterStart.getMonth() + 3, 0)
      bucketEnd = endOfDay(quarterEnd)
      label = `Q${Math.floor(quarterStart.getMonth() / 3) + 1}`
      subLabel = `${quarterStart.toLocaleString('en-US', { month: 'short' })} - ${new Date(quarterStart.getFullYear(), quarterStart.getMonth() + 2, 1).toLocaleString('en-US', { month: 'short' })}`
      groupLabel = `${quarterStart.getFullYear()}`
      cursor.setMonth(cursor.getMonth() + 3, 1)
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

function updateViewportWidth() {
  viewportWidth.value = scrollContainer.value?.clientWidth || 0
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
  if (activeView.value.unit === 'week') {
    date.setDate(date.getDate() + (steps * 7))
    return
  }

  if (activeView.value.unit === 'month') {
    date.setMonth(date.getMonth() + steps)
    return
  }

  if (activeView.value.unit === 'quarter') {
    date.setMonth(date.getMonth() + (steps * 3))
    return
  }

  date.setDate(date.getDate() + steps)
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

function isWeekend(date) {
  if (!date) return false
  const day = date.getDay()
  return day === 0 || day === 6
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
          <div class="tl-col-workitems">WORK ITEMS</div>
          <div class="tl-col-duration">DURATION</div>
        </div>

        <div class="tl-left-rows" ref="leftPanelRows" :style="{ minHeight: `${rowsCanvasHeight}px` }">
          <div
            v-for="task in visibleTasks"
            :key="task.id"
            class="tl-task-row"
            :style="{ '--task-color': getStatusColor(task.statusName) }"
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
          Click a timeline slot to create a work item with start and due dates prefilled.
        </div>
        <div class="tl-gantt" :style="{ width: `${canvasWidth}px` }">
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
              :class="{ 
                'is-today': containsDay(bucket.start, bucket.end, today.value), 
                'create-enabled': createMode, 
                'bucket-selected': clickedBucket && formatDateOnly(clickedBucket.start) === formatDateOnly(bucket.start),
                'weekend': viewMode === 'Day' && isWeekend(bucket.start)
              }"
              :style="{ width: `${cellWidth}px` }"
              @click="handleTimelineCanvasClick(bucket)"
            >
              <span class="day-num">{{ bucket.label }}</span>
              <span class="day-dow">{{ bucket.subLabel }}</span>
              <span v-if="bucketProgress[index].total" class="bucket-progress">{{ bucketProgress[index].percent }}%</span>
            </button>
          </div>

          <div class="tl-bars-container" :style="{ minHeight: `${rowsCanvasHeight}px` }">
            <div class="tl-grid-lines">
              <button
                v-for="(bucket, index) in timeBuckets"
                :key="`grid-${index}`"
                type="button"
                class="tl-grid-line"
                :class="{ 
                  'is-today': containsDay(bucket.start, bucket.end, today.value), 
                  'create-active': createMode,
                  'weekend': viewMode === 'Day' && isWeekend(bucket.start)
                }"
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
                  '--task-color': getStatusColor(task.statusName)
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
              <span class="canvas-add-label">Click để thêm work item mới</span>
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
  background: var(--color-bg);
  color: var(--color-text-primary);
  font-family: system-ui, -apple-system, sans-serif;
  font-size: 12px;
  overflow: hidden;
}

/* HEADER (TOP NAV) - Stylized minimally to match */
.tl-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 16px;
  padding: 8px 16px;
  background: var(--color-bg);
  border-bottom: 1px solid var(--color-border);
}

.tl-header-left, .tl-header-right, .tl-view-modes {
  display: flex;
  align-items: center;
  gap: 8px;
}

.tl-btn {
  height: 24px;
  padding: 0 8px;
  font-size: 11px;
  border: 1px solid var(--color-border);
  background: var(--color-surface);
  color: var(--color-text-secondary);
  border-radius: 0;
  cursor: pointer;
  transition: background 0.2s;
}

.tl-btn:hover {
  background: var(--color-surface-hover);
}

.tl-btn.active {
  background: var(--color-accent);
  color: #ffffff;
  border-color: var(--color-accent);
}

.mode-btn {
  height: 24px;
  padding: 0 10px;
  font-size: 11px;
  border: none;
  background: transparent;
  color: var(--color-text-secondary);
  cursor: pointer;
}

.mode-btn.active {
  background: var(--color-surface-hover);
  color: var(--color-text-primary);
}

/* BODY LAYOUT */
.tl-body {
  display: flex;
  flex: 1;
  overflow: hidden;
}

/* LEFT PANEL */
.tl-left-panel {
  width: 280px;
  min-width: 280px;
  border-right: 1px solid var(--color-border);
  display: flex;
  flex-direction: column;
  background: var(--color-bg);
  z-index: 5;
}

.tl-left-header {
  display: flex;
  height: 40px;
  align-items: center;
  border-bottom: 1px solid var(--color-border);
  background: var(--color-bg);
}

.tl-col-workitems, .tl-col-duration {
  height: 100%;
  display: flex;
  align-items: center;
  padding: 0 12px;
  font-size: 11px;
  letter-spacing: 0.08em;
  font-weight: 600;
  color: var(--color-text-secondary);
  text-transform: uppercase;
}

.tl-col-workitems {
  flex: 1;
}

.tl-col-duration {
  width: 80px;
  justify-content: flex-end;
  border-left: 1px solid var(--color-border);
}

.tl-left-rows {
  flex: 1;
  overflow-y: auto;
  scrollbar-width: none; /* Hide scrollbar for sync */
}
.tl-left-rows::-webkit-scrollbar { display: none; }

.tl-task-row {
  display: flex;
  height: 40px;
  align-items: center;
  border-bottom: 1px solid var(--color-border);
  cursor: pointer;
  background: transparent;
  width: 100%;
  border-top: 0;
  border-left: 0;
  border-right: 0;
  padding: 0;
}

.tl-task-row:hover {
  background: var(--color-surface-hover);
}

.tl-task-row .tl-col-workitems {
  font-size: 12px;
  text-transform: none;
  letter-spacing: normal;
  color: var(--color-text-primary);
  font-weight: 400;
}

.tl-task-row .tl-col-duration {
  font-size: 11px;
  color: var(--color-text-secondary);
  font-weight: 400;
}

.task-key {
  color: var(--color-text-secondary);
  margin-right: 8px;
  opacity: 0.7;
}

.task-title-text {
  flex: 1;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.tl-add-row {
  color: var(--color-text-secondary);
  font-size: 14px;
}

.add-text {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 0 12px;
}

/* RIGHT PANEL */
.tl-right-panel {
  flex: 1;
  overflow-x: auto;
  overflow-y: auto;
  background: var(--color-bg);
}

.tl-gantt {
  position: relative;
  min-height: 100%;
}

/* HEADER ROWS */
.tl-group-row, .tl-day-row {
  display: flex;
  position: sticky;
  top: 0;
  z-index: 4;
}

.tl-group-row {
  height: 24px;
  background: var(--color-surface);
  border-bottom: 1px solid var(--color-border);
}

.tl-day-row {
  top: 24px;
  height: 40px;
  background: var(--color-bg);
  border-bottom: 1px solid var(--color-border);
}

.tl-group-cell {
  display: flex;
  align-items: center;
  padding: 0 12px;
  font-size: 11px;
  letter-spacing: 0.05em;
  color: var(--color-text-secondary);
  text-transform: uppercase;
  border-right: 1px solid var(--color-border);
}

.tl-day-cell {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  border-right: 1px solid var(--color-border);
  background: transparent;
  border-top: 0;
  border-left: 0;
  border-bottom: 0;
  padding: 0;
}

.day-num {
  font-size: 13px;
  color: var(--color-text-primary);
}

.day-dow {
  font-size: 10px;
  color: var(--color-text-secondary);
  margin-top: 2px;
}

.tl-day-cell.weekend {
  background: var(--color-surface);
}

/* GRID & BARS */
.tl-bars-container {
  position: relative;
  min-height: calc(100% - 64px);
}

.tl-grid-lines {
  position: absolute;
  top: 0;
  bottom: 0;
  left: 0;
  right: 0;
  display: flex;
  pointer-events: none;
}

.tl-grid-line {
  height: 100%;
  border-right: 1px solid var(--color-border);
  opacity: 0.3; /* Subtle vertical lines */
}

/* Weekend columns */
.tl-grid-line.weekend {
  background: var(--color-surface);
  opacity: 0.15;
}

.today-line {
  position: absolute;
  top: 0;
  bottom: 0;
  width: 2px;
  background: var(--color-accent);
  z-index: 2;
}

.tl-bar-row {
  height: 40px;
  border-bottom: 1px solid var(--color-border);
  position: relative;
}

.tl-task-bar {
  position: absolute;
  top: 8px;
  height: 24px;
  background: var(--color-accent);
  color: #ffffff;
  display: flex;
  align-items: center;
  padding: 0 8px;
  font-size: 11px;
  font-weight: 500;
  cursor: pointer;
  z-index: 3;
  border-radius: 0; /* NO rounded corners */
}

.tl-task-bar:hover {
  filter: brightness(1.1);
}

.bar-label {
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.tl-add-canvas-row {
  width: 100%;
  height: 40px;
  border: 0;
  background: transparent;
  position: relative;
  cursor: crosshair;
}

.canvas-add-label {
  position: absolute;
  left: 12px;
  top: 12px;
  font-size: 12px;
  color: var(--color-text-secondary);
  font-style: italic;
  opacity: 0.7;
}

/* SCROLLBAR */
.tl-right-panel::-webkit-scrollbar {
  height: 2px;
  width: 2px;
}

.tl-right-panel::-webkit-scrollbar-track {
  background: var(--color-surface);
}

.tl-right-panel::-webkit-scrollbar-thumb {
  background: var(--color-border);
}

/* UTILS */
.create-mode-banner {
  padding: 8px 16px;
  background: var(--color-accent);
  color: #ffffff;
  font-size: 11px;
  text-align: center;
}

/* Polished Gantt view */
.plane-timeline {
  background:
    radial-gradient(circle at 12% 0%, color-mix(in srgb, var(--color-accent) 10%, transparent), transparent 32%),
    var(--color-bg) !important;
}

.tl-header {
  min-height: 46px;
  padding: 8px 14px !important;
  background: color-mix(in srgb, var(--color-surface) 84%, transparent) !important;
}

.tl-btn,
.mode-btn {
  min-height: 30px !important;
  border-radius: 8px !important;
  font-size: 12px !important;
  font-weight: 800 !important;
}

.mode-btn.active {
  background: color-mix(in srgb, var(--color-accent) 15%, var(--color-surface-hover)) !important;
  color: var(--color-text-primary) !important;
}

.tl-left-panel,
.tl-right-panel {
  background: color-mix(in srgb, var(--color-bg) 92%, var(--color-surface)) !important;
}

.tl-left-header,
.tl-group-row,
.tl-day-row {
  background: color-mix(in srgb, var(--color-surface-hover) 58%, var(--color-surface)) !important;
}

.tl-task-row {
  height: 44px !important;
  border-bottom-color: color-mix(in srgb, var(--color-border) 76%, transparent) !important;
  box-shadow: inset 0 0 0 0 transparent;
}

.tl-task-row:hover {
  background:
    linear-gradient(90deg, color-mix(in srgb, var(--task-color, var(--color-accent)) 12%, transparent), transparent 70%),
    color-mix(in srgb, var(--color-surface-hover) 70%, transparent) !important;
  box-shadow: inset 3px 0 0 var(--task-color, var(--color-accent));
}

.task-key {
  color: color-mix(in srgb, var(--task-color, var(--color-accent)) 62%, var(--color-text-primary)) !important;
  font-weight: 850;
  opacity: 1 !important;
}

.task-title-text {
  font-weight: 650;
}

.tl-day-cell {
  color: var(--color-text-primary);
  cursor: pointer;
}

.tl-day-cell.is-today,
.tl-grid-line.is-today {
  background: color-mix(in srgb, var(--color-accent) 12%, transparent) !important;
}

.tl-day-cell.weekend,
.tl-grid-line.weekend {
  background: color-mix(in srgb, var(--color-text-muted) 8%, transparent) !important;
}

.tl-bar-row {
  height: 44px !important;
  border-bottom-color: color-mix(in srgb, var(--color-border) 76%, transparent) !important;
}

.tl-task-bar {
  top: 9px !important;
  height: 26px !important;
  border-radius: 7px !important;
  background:
    linear-gradient(135deg, color-mix(in srgb, var(--task-color, var(--color-accent)) 92%, #ffffff 8%), color-mix(in srgb, var(--task-color, var(--color-accent)) 76%, #111827 24%)) !important;
  border: 1px solid color-mix(in srgb, var(--task-color, var(--color-accent)) 72%, #ffffff 18%) !important;
  box-shadow: 0 10px 20px color-mix(in srgb, var(--task-color, var(--color-accent)) 24%, transparent);
  font-weight: 850 !important;
}

.tl-task-bar:hover {
  filter: none !important;
  transform: translateY(-1px);
}

.today-line {
  width: 3px !important;
  background: linear-gradient(180deg, transparent, var(--color-accent), transparent) !important;
}

.canvas-add-label {
  color: var(--color-text-muted) !important;
  font-style: normal !important;
}
</style>




