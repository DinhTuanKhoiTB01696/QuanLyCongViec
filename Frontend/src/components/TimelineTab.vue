<script setup>
import { computed, onMounted, onUnmounted, ref, watch } from 'vue'
import { ElMessage } from 'element-plus'
import { useWorkTaskStore } from '@/store/useWorkTaskStore'
import axiosClient from '@/api/axiosClient'

const props = defineProps({
  projectId: { type: String, required: true },
  tasks: { type: Array, default: null }
})

const emit = defineEmits(['open-task'])

const taskStore = useWorkTaskStore()
const sourceTasks = computed(() => props.tasks || taskStore.tasks)
const loading = computed(() => taskStore.loading)

const today = new Date()
const viewMode = ref('Week')
const showOptions = ref(false)
const createMode = ref(false)
const scrollContainer = ref(null)
const dragState = ref(null)
const clickedBucket = ref(null)
const expanded = ref({
  showOnlyScheduled: true,
  hideDone: false,
  onlyCurrentWindow: true
})

const viewModes = [
  { key: 'Week', unit: 'day', cellWidth: 40 },
  { key: 'Month', unit: 'week', cellWidth: 68 },
  { key: 'Quarter', unit: 'month', cellWidth: 88 }
]

const activeView = computed(() => viewModes.find(mode => mode.key === viewMode.value) || viewModes[0])

const timelineRange = computed(() => {
  const start = startOfDay(today)
  const end = startOfDay(today)

  if (viewMode.value === 'Week') {
    start.setDate(start.getDate() - 7)
    end.setDate(end.getDate() + 21)
  } else if (viewMode.value === 'Month') {
    start.setDate(1)
    start.setMonth(start.getMonth() - 1)
    end.setMonth(end.getMonth() + 2, 0)
  } else {
    start.setMonth(start.getMonth() - 1, 1)
    end.setMonth(end.getMonth() + 4, 0)
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
  const todayBucketIndex = timeBuckets.value.findIndex(bucket => containsDay(bucket.start, bucket.end, today))
  return todayBucketIndex < 0 ? 0 : (todayBucketIndex * cellWidth.value) + (cellWidth.value / 2)
})

const fetchTasks = () => {
  if (!props.tasks && props.projectId) {
    taskStore.fetchTasks(props.projectId)
  }
}

const goToToday = () => {
  if (!scrollContainer.value) return
  scrollContainer.value.scrollLeft = Math.max(0, todayOffset.value - 240)
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
  createMode.value = false
  window.dispatchEvent(
    new CustomEvent('global-create-task', {
      detail: bucket
        ? {
            plannedStartDate: bucket.start.toISOString(),
            dueDate: bucket.end.toISOString()
          }
        : undefined
    })
  )
}

const toggleCreateMode = () => {
  createMode.value = !createMode.value
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

  const daysStep = viewMode.value === 'Week' ? 1 : viewMode.value === 'Month' ? 7 : 30
  const stepsDiff = Math.round((event.clientX - current.startX) / cellWidth.value)

  if (stepsDiff === 0) return

  const task = current.task
  const originalStart = task.plannedStartDate
  const originalEnd = task.dueDate || task.plannedEndDate
  const startDate = task.plannedStartDate ? new Date(task.plannedStartDate) : task.createdAt ? new Date(task.createdAt) : startOfDay(today)
  const endDate = (task.plannedEndDate || task.dueDate) ? new Date(task.plannedEndDate || task.dueDate) : new Date(startDate)

  if (current.type === 'move') {
    startDate.setDate(startDate.getDate() + (stepsDiff * daysStep))
    endDate.setDate(endDate.getDate() + (stepsDiff * daysStep))
  } else if (current.type === 'resize-left') {
    startDate.setDate(startDate.getDate() + (stepsDiff * daysStep))
  } else if (current.type === 'resize-right') {
    endDate.setDate(endDate.getDate() + (stepsDiff * daysStep))
  }

  if (startDate > endDate) {
    ElMessage.warning('Khoang thoi gian khong hop le.')
    return
  }

  task.plannedStartDate = startDate.toISOString()
  task.dueDate = endDate.toISOString()

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

watch(() => props.projectId, fetchTasks, { immediate: true })
watch(viewMode, () => {
  window.setTimeout(goToToday, 60)
})

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
      bucketEnd = endOfDay(addDays(bucketStart, 6))
      label = `W${getWeekNumber(bucketStart)}`
      subLabel = `${bucketStart.getDate()}-${bucketEnd.getDate()}`
      groupLabel = bucketStart.toLocaleString('en-US', { month: 'short', year: 'numeric' })
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
  let start = task.plannedStartDate ? new Date(task.plannedStartDate) : null
  let end = (task.plannedEndDate || task.dueDate) ? new Date(task.plannedEndDate || task.dueDate) : null

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
              :class="{ 'is-today': containsDay(bucket.start, bucket.end, today) }"
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
                :class="{ 'is-today': containsDay(bucket.start, bucket.end, today), 'create-active': createMode }"
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
  min-height: calc(100vh - 180px);
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
  padding: 12px 20px;
  border-bottom: 1px solid #1e2025;
}

.tl-header-left,
.tl-header-right {
  gap: 12px;
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
  overflow: hidden;
}

.tl-left-panel {
  width: 320px;
  min-width: 320px;
  border-right: 2px solid #1e2025;
  display: flex;
  flex-direction: column;
}

.tl-left-header {
  display: flex;
  height: 72px;
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
  width: 78px;
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
  height: 42px;
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
  min-width: 76px;
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
  overflow: auto;
}

.tl-gantt {
  min-height: 100%;
  position: relative;
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
  height: 30px;
  border-bottom: 1px solid #1e2025;
}

.tl-day-row {
  top: 30px;
  height: 42px;
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
  gap: 2px;
  padding: 4px 0;
}

.tl-day-cell.is-today {
  background: rgba(37, 99, 235, 0.12);
}

.day-num {
  font-size: 12px;
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
  cursor: pointer;
}

.tl-grid-line.create-active:hover {
  background: rgba(56, 189, 248, 0.08);
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
}

.tl-bar-row {
  position: relative;
  height: 42px;
  border-bottom: 1px solid #1e2025;
}

.tl-task-bar {
  position: absolute;
  top: 8px;
  height: 26px;
  border-radius: 6px;
  display: flex;
  align-items: center;
  padding: 0 10px;
  color: #ffffff;
  cursor: pointer;
  z-index: 3;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.25);
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
  font-size: 11px;
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
