<script setup>
import { computed, onMounted, onUnmounted, provide, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useSprintStore } from '@/store/useSprintStore'
import { useI18nStore } from '@/store/useI18nStore'
import axiosClient from '@/api/axiosClient'
import { subscribeAdminRealtime } from '@/utils/adminRealtime'

const i18nStore = useI18nStore()
const t = (key) => i18nStore.t(key)

import { use } from 'echarts/core'
import { CanvasRenderer } from 'echarts/renderers'
import { LineChart } from 'echarts/charts'
import { TitleComponent, TooltipComponent, LegendComponent, GridComponent } from 'echarts/components'
import VChart, { THEME_KEY } from 'vue-echarts'

use([CanvasRenderer, LineChart, TitleComponent, TooltipComponent, LegendComponent, GridComponent])

const props = defineProps({
  projectId: { type: String, required: true }
})

const router = useRouter()
const route = useRoute()
const sprintStore = useSprintStore()

provide(THEME_KEY, 'dark')

const showCreateModal = ref(false)
const burndownCharts = ref({})
const showCalendar = ref(false)
const currentMonth = ref(new Date().getMonth())
const currentYear = ref(new Date().getFullYear())
const dateSelectionStep = ref(0)
const tempStart = ref(null)
const tempEnd = ref(null)
const newCycle = ref({ name: '', description: '', startDate: null, endDate: null })
const showCycleSearch = ref(false)
const showCycleFilters = ref(false)
const cycleSearchQuery = ref('')
const cycleProgressFilter = ref('all')

const expandedTabs = ref({
  active: true,
  upcoming: true,
  completed: true
})
const cyclePanelTabs = ref({})
const cycleWorkItems = ref({})
const cycleWorkItemsLoading = ref({})
const expandedCarryOverCycleId = ref(null)
const carryOverItems = ref({})
const carryOverLoading = ref({})
const carryOverFilters = ref({})
const carryOverSelection = ref({})
const carryOverTargetSprintId = ref({})
const carryOverMoving = ref({})

const monthNames = ['Thang 1', 'Thang 2', 'Thang 3', 'Thang 4', 'Thang 5', 'Thang 6', 'Thang 7', 'Thang 8', 'Thang 9', 'Thang 10', 'Thang 11', 'Thang 12']
const dayNames = ['CN', 'T2', 'T3', 'T4', 'T5', 'T6', 'T7']

const allSprints = computed(() => sprintStore.sprints || [])
const filteredSprints = computed(() => {
  const keyword = cycleSearchQuery.value.trim().toLowerCase()
  return allSprints.value.filter(cycle => {
    const matchesSearch = !keyword ||
      `${cycle.name || ''}`.toLowerCase().includes(keyword) ||
      `${cycle.description || ''}`.toLowerCase().includes(keyword)

    const progress = cycle.progressPercent || 0
    const matchesProgress =
      cycleProgressFilter.value === 'all' ||
      (cycleProgressFilter.value === 'not-started' && progress === 0) ||
      (cycleProgressFilter.value === 'in-progress' && progress > 0 && progress < 100) ||
      (cycleProgressFilter.value === 'completed' && progress >= 100)

    return matchesSearch && matchesProgress
  })
})
const activeSprints = computed(() => filteredSprints.value.filter(s => (s.state || '').toLowerCase() === 'active'))
const upcomingSprints = computed(() => filteredSprints.value.filter(s => (s.state || '').toLowerCase() === 'upcoming'))
const completedSprints = computed(() => filteredSprints.value.filter(s => (s.state || '').toLowerCase() === 'completed'))
const hasCycleFilters = computed(() => Boolean(cycleSearchQuery.value.trim()) || cycleProgressFilter.value !== 'all')

const toggleTab = (tab) => {
  expandedTabs.value[tab] = !expandedTabs.value[tab]
}

const clearCycleFilters = () => {
  cycleSearchQuery.value = ''
  cycleProgressFilter.value = 'all'
}

const getCyclePanelTab = (cycleId) => cyclePanelTabs.value[cycleId] || 'state'

const setCyclePanelTab = async (cycle, tab) => {
  cyclePanelTabs.value = { ...cyclePanelTabs.value, [cycle.id]: tab }
  if (tab === 'items') {
    await fetchCycleWorkItems(cycle.id)
  }
}

const fetchCycleWorkItems = async (cycleId) => {
  if (cycleWorkItems.value[cycleId] || cycleWorkItemsLoading.value[cycleId]) return

  cycleWorkItemsLoading.value = { ...cycleWorkItemsLoading.value, [cycleId]: true }
  try {
    const res = await axiosClient.get(`/projects/${props.projectId}/WorkTasks`)
    const tasks = res.data?.data || []
    cycleWorkItems.value = {
      ...cycleWorkItems.value,
      [cycleId]: tasks.filter(task => task.sprintId === cycleId && !(task.parentTaskId || task.parentId))
    }
  } catch (error) {
    console.error('Failed to load cycle work items', error)
    cycleWorkItems.value = { ...cycleWorkItems.value, [cycleId]: [] }
  } finally {
    cycleWorkItemsLoading.value = { ...cycleWorkItemsLoading.value, [cycleId]: false }
  }
}

const cycleItemsFor = (cycleId) => cycleWorkItems.value[cycleId] || []
const getCarryOverFilter = (cycleId) => carryOverFilters.value[cycleId] || { search: '', scope: 'all' }
const getCarryOverSelection = (cycleId) => carryOverSelection.value[cycleId] || []
const getCarryOverTargetSprintId = (cycleId) => carryOverTargetSprintId.value[cycleId] || null
const carryOverTasksFor = (cycleId) => carryOverItems.value[cycleId] || []

const visibleCarryOverTasks = (cycleId) => {
  const filters = getCarryOverFilter(cycleId)
  const keyword = `${filters.search || ''}`.trim().toLowerCase()
  return carryOverTasksFor(cycleId).filter(task => {
    const matchesSearch = !keyword ||
      `${task.title || ''}`.toLowerCase().includes(keyword) ||
      `${task.sequenceId || ''}`.toLowerCase().includes(keyword)
    const matchesScope =
      filters.scope === 'all' ||
      (filters.scope === 'backlog' && task.currentLocation === 'Backlog') ||
      (filters.scope === 'cycle' && task.currentLocation === 'Cycle')
    return matchesSearch && matchesScope
  })
}

const availableTargetSprints = (cycleId) => allSprints.value.filter(sprint => sprint.id !== cycleId && (sprint.state || '').toLowerCase() !== 'completed')

const priorityLabel = (priority) => {
  if (priority === 1) return t('Urgent')
  if (priority === 2) return t('High')
  if (priority === 3) return t('Normal')
  if (priority === 4) return t('Low')
  return t('None')
}

const assigneeLabel = (task) => task.assignedUserName || t('Unassigned')

const formatDateCompact = (d) => {
  if (!d) return ''
  const date = toLocalDate(d)
  return date.toLocaleDateString('en-US', {
    month: 'short',
    day: 'numeric',
    year: 'numeric'
  })
}

const toLocalDate = (value) => {
  if (value instanceof Date) {
    return new Date(value.getFullYear(), value.getMonth(), value.getDate())
  }

  if (typeof value === 'string') {
    const raw = value.split('T')[0]
    const [year, month, day] = raw.split('-').map(Number)
    if (year && month && day) return new Date(year, month - 1, day)
  }

  const date = new Date(value)
  return new Date(date.getFullYear(), date.getMonth(), date.getDate())
}

const toLocalIsoDate = (value) => {
  const date = toLocalDate(value)
  const year = date.getFullYear()
  const month = `${date.getMonth() + 1}`.padStart(2, '0')
  const day = `${date.getDate()}`.padStart(2, '0')
  return `${year}-${month}-${day}T00:00:00`
}

const percentLabel = (cycle) => `${cycle.progressPercent || 0}%`

const progressSegments = (cycle) => {
  const total = Math.max(cycle.taskCount || 0, 1)
  const completed = cycle.completedTaskCount || 0
  const started = cycle.inProgressTaskCount || 0
  const backlog = cycle.backlogTaskCount || 0
  const remaining = Math.max((cycle.taskCount || 0) - completed - started - backlog, 0)

  return [
    { label: t('Completed'), value: completed, width: `${(completed / total) * 100}%`, className: 'bg-green' },
    { label: t('Started'), value: started, width: `${(started / total) * 100}%`, className: 'bg-orange' },
    { label: t('Backlog'), value: backlog, width: `${(backlog / total) * 100}%`, className: 'bg-lightgray' },
    { label: t('Other'), value: remaining, width: `${(remaining / total) * 100}%`, className: 'bg-darkgray' }
  ]
}

const activeItemCount = (cycle) => Math.max((cycle.taskCount || 0) - (cycle.completedTaskCount || 0), 0)

const openCycleBoard = (cycle) => {
  router.push({
    name: 'CycleDetailView',
    params: { id: props.projectId, cycleId: cycle.id },
    query: {
      tab: 'spreadsheet',
      sprintId: cycle.id,
      sprintName: cycle.name
    }
  })
}

const openCarryOverBoard = (cycle) => {
  router.push({
    name: 'CycleDetailView',
    params: { id: props.projectId, cycleId: cycle.id },
    query: {
      tab: 'spreadsheet',
      carryOverFromSprintId: cycle.id
    }
  })
}

const ensureCarryOverState = (cycleId) => {
  if (!carryOverFilters.value[cycleId]) {
    carryOverFilters.value = {
      ...carryOverFilters.value,
      [cycleId]: { search: '', scope: 'all' }
    }
  }
  if (!carryOverSelection.value[cycleId]) {
    carryOverSelection.value = { ...carryOverSelection.value, [cycleId]: [] }
  }
}

const fetchCarryOverTasks = async (cycleId, force = false) => {
  ensureCarryOverState(cycleId)
  if (!force && (carryOverItems.value[cycleId] || carryOverLoading.value[cycleId])) return

  carryOverLoading.value = { ...carryOverLoading.value, [cycleId]: true }
  try {
    const res = await axiosClient.get(`/projects/${props.projectId}/sprints/${cycleId}/carry-over-tasks`)
    carryOverItems.value = { ...carryOverItems.value, [cycleId]: res.data?.data || [] }
    carryOverSelection.value = { ...carryOverSelection.value, [cycleId]: [] }
  } catch (error) {
    console.error('Failed to load carry-over tasks', error)
    carryOverItems.value = { ...carryOverItems.value, [cycleId]: [] }
  } finally {
    carryOverLoading.value = { ...carryOverLoading.value, [cycleId]: false }
  }
}

const toggleCarryOverPlanner = async (cycle) => {
  expandedCarryOverCycleId.value = expandedCarryOverCycleId.value === cycle.id ? null : cycle.id
  if (expandedCarryOverCycleId.value === cycle.id) {
    await fetchCarryOverTasks(cycle.id)
  }
}

const updateCarryOverFilter = (cycleId, patch) => {
  ensureCarryOverState(cycleId)
  carryOverFilters.value = {
    ...carryOverFilters.value,
    [cycleId]: { ...carryOverFilters.value[cycleId], ...patch }
  }
}

const toggleCarryOverSelection = (cycleId, taskId) => {
  const next = new Set(getCarryOverSelection(cycleId))
  if (next.has(taskId)) next.delete(taskId)
  else next.add(taskId)
  carryOverSelection.value = { ...carryOverSelection.value, [cycleId]: Array.from(next) }
}

const toggleSelectAllCarryOver = (cycleId) => {
  const visibleIds = visibleCarryOverTasks(cycleId).map(task => task.id)
  const selected = getCarryOverSelection(cycleId)
  const shouldSelectAll = visibleIds.some(id => !selected.includes(id))
  carryOverSelection.value = {
    ...carryOverSelection.value,
    [cycleId]: shouldSelectAll ? visibleIds : []
  }
}

const carryOverSelectedCount = (cycleId) => getCarryOverSelection(cycleId).length

const moveCarryOverTasks = async (cycleId, targetSprintId = null) => {
  const taskIds = getCarryOverSelection(cycleId)
  if (!taskIds.length) return

  carryOverMoving.value = { ...carryOverMoving.value, [cycleId]: true }
  try {
    await axiosClient.post(`/projects/${props.projectId}/sprints/${cycleId}/carry-over-tasks/move`, {
      taskIds,
      targetSprintId
    })
    await Promise.all([
      loadCycles(true),
      fetchCarryOverTasks(cycleId, true)
    ])
  } catch (error) {
    console.error('Failed to move carry-over tasks', error)
    alert(error.response?.data?.message || 'Khong the cap nhat task ton dong')
  } finally {
    carryOverMoving.value = { ...carryOverMoving.value, [cycleId]: false }
  }
}

const fetchBurndowns = async () => {
  const chartEntries = {}
  await Promise.all(activeSprints.value.map(async (sprint) => {
    try {
      const res = await axiosClient.get(`/projects/${props.projectId}/sprints/${sprint.id}/burndown`)
      const burndown = res.data?.data || []
      chartEntries[sprint.id] = {
        backgroundColor: 'transparent',
        tooltip: { trigger: 'axis' },
        legend: {
          data: ['Current work items', 'Ideal work items'],
          bottom: 0,
          textStyle: { color: 'var(--color-text-muted)', fontSize: 10 }
        },
        grid: { top: 10, left: 30, right: 10, bottom: 40 },
        xAxis: {
          type: 'category',
          data: burndown.map(item => item.date),
          axisLine: { show: false },
          axisTick: { show: false },
          axisLabel: { color: 'var(--color-text-muted)', fontSize: 9 }
        },
        yAxis: {
          type: 'value',
          splitLine: { lineStyle: { color: 'rgba(255,255,255,0.05)' } },
          axisLabel: { color: 'var(--color-text-muted)', fontSize: 10 }
        },
        series: [
          {
            name: 'Current work items',
            type: 'line',
            data: burndown.map(item => item.actualRemaining ?? item.remainingPoints ?? 0),
            itemStyle: { color: '#3B82F6' },
            lineStyle: { width: 2 },
            areaStyle: { color: 'rgba(59,130,246,0.15)' },
            symbol: 'circle',
            symbolSize: 7,
            step: 'end',
            smooth: false
          },
          {
            name: 'Ideal work items',
            type: 'line',
            data: burndown.map(item => item.idealRemaining ?? item.idealPoints ?? 0),
            itemStyle: { color: 'var(--color-text-muted)' },
            lineStyle: { type: 'dashed', width: 2 },
            symbol: 'circle',
            symbolSize: 7,
            smooth: false
          }
        ]
      }
    } catch (error) {
      console.error('Failed to load burndown', error)
    }
  }))
  burndownCharts.value = chartEntries
}

const loadCycles = async (force = false) => {
  await sprintStore.fetchSprints(props.projectId, { force })
  await fetchBurndowns()
}

const fixDateOffset = (dt) => {
  if (!dt) return null
  return toLocalIsoDate(dt)
}

const createNewCycle = async () => {
  if (!newCycle.value.name?.trim() || !newCycle.value.startDate) return

  let finalEndDate = newCycle.value.endDate ? fixDateOffset(newCycle.value.endDate) : null
  if (!finalEndDate) {
    const fallback = new Date(newCycle.value.startDate)
    fallback.setDate(fallback.getDate() + 14)
    finalEndDate = fixDateOffset(fallback)
  }

  try {
    await axiosClient.post(`/projects/${props.projectId}/sprints`, {
      name: newCycle.value.name.trim(),
      description: newCycle.value.description,
      startDate: fixDateOffset(newCycle.value.startDate),
      endDate: finalEndDate
    })

    showCreateModal.value = false
    showCalendar.value = false
    newCycle.value = { name: '', description: '', startDate: null, endDate: null }
    await loadCycles()
  } catch (error) {
    alert(error.response?.data?.message || 'Không thể tạo cycle')
  }
}

const startCycle = async (cycle) => {
  try {
    await sprintStore.startSprint(props.projectId, cycle.id)
  } catch (error) {
    alert(error.response?.data?.message || 'Không thể bắt đầu cycle')
  }
}

const toggleCalendar = () => {
  showCalendar.value = !showCalendar.value
  if (showCalendar.value) {
    tempStart.value = newCycle.value.startDate
    tempEnd.value = newCycle.value.endDate
    dateSelectionStep.value = 0
  }
}

const daysInMonth = computed(() => {
  const days = []
  const date = new Date(currentYear.value, currentMonth.value, 1)
  const firstDay = date.getDay()
  const lastDate = new Date(currentYear.value, currentMonth.value + 1, 0).getDate()
  const prevLastDate = new Date(currentYear.value, currentMonth.value, 0).getDate()

  for (let i = firstDay - 1; i >= 0; i -= 1) {
    days.push({ day: prevLastDate - i, isCurrent: false, date: null })
  }

  for (let i = 1; i <= lastDate; i += 1) {
    days.push({ day: i, isCurrent: true, date: new Date(currentYear.value, currentMonth.value, i) })
  }

  const remainder = days.length % 7
  if (remainder !== 0) {
    for (let i = 1; i <= 7 - remainder; i += 1) {
      days.push({ day: i, isCurrent: false, date: null })
    }
  }

  return days
})

const moveMonth = (direction) => {
  currentMonth.value += direction
  if (currentMonth.value > 11) {
    currentMonth.value = 0
    currentYear.value += 1
  }
  if (currentMonth.value < 0) {
    currentMonth.value = 11
    currentYear.value -= 1
  }
}

const isSameDate = (left, right) => left && right && left.getFullYear() === right.getFullYear() && left.getMonth() === right.getMonth() && left.getDate() === right.getDate()

const todayStart = () => {
  const today = new Date()
  today.setHours(0, 0, 0, 0)
  return today
}

const isPastDate = (date) => {
  if (!date) return false
  return toLocalDate(date).getTime() < todayStart().getTime()
}

const selectDate = (day) => {
  if (!day.isCurrent || isPastDate(day.date)) return
  const picked = day.date
  if (dateSelectionStep.value === 0) {
    tempStart.value = picked
    tempEnd.value = null
    newCycle.value.startDate = picked
    newCycle.value.endDate = null
    dateSelectionStep.value = 1
    return
  }

  if (picked < tempStart.value) {
    tempStart.value = picked
    tempEnd.value = null
    newCycle.value.startDate = picked
    newCycle.value.endDate = null
    return
  }

  tempEnd.value = picked
  newCycle.value.endDate = picked
  dateSelectionStep.value = 0
  showCalendar.value = false
}

const isSelectedStart = (date) => isSameDate(date, tempStart.value)
const isSelectedEnd = (date) => isSameDate(date, tempEnd.value)
const isInRange = (day) => {
  if (!day.date || !tempStart.value || !tempEnd.value || !day.isCurrent) return false
  const time = day.date.getTime()
  return time > tempStart.value.getTime() && time < tempEnd.value.getTime()
}

const btnDateText = computed(() => {
  if (!newCycle.value.startDate) return t('Select date range')
  const start = formatDateCompact(newCycle.value.startDate)
  const end = newCycle.value.endDate ? formatDateCompact(newCycle.value.endDate) : '...'
  return `${start} -> ${end}`
})

watch(() => props.projectId, () => loadCycles(true), { immediate: true })
watch(
  () => route.query.carryOverFromSprintId,
  async (cycleId) => {
    if (!cycleId || typeof cycleId !== 'string') return
    expandedTabs.value.completed = true
    expandedCarryOverCycleId.value = cycleId
    await fetchCarryOverTasks(cycleId, true)
  },
  { immediate: true }
)

let cycleRefreshTimer = null
let unsubscribeAdminRealtime = null
onMounted(() => {
  cycleRefreshTimer = window.setInterval(() => {
    if (props.projectId) {
      loadCycles()
    }
  }, 60000)

  unsubscribeAdminRealtime = subscribeAdminRealtime(async ({ type, payload }) => {
    if (!props.projectId) return
    if (payload?.projectId && `${payload.projectId}` !== `${props.projectId}`) return

    if (
      [
        'project-settings-updated',
        'project-settings-favorite-updated',
        'project-settings-integrations-updated',
        'project-administration-updated'
      ].includes(type)
    ) {
      await loadCycles(true)
    }
  })
})

onUnmounted(() => {
  if (cycleRefreshTimer) {
    window.clearInterval(cycleRefreshTimer)
  }
  unsubscribeAdminRealtime?.()
})
</script>

<template>
  <div class="plane-cycles-wrapper">
    <header class="nexus-project-header">
      <div class="nexus-breadcrumb">
        <div class="project-icon" style="background: #F59E0B">
          <i class="fa-solid fa-certificate"></i>
        </div>
        <span class="view-name">{{ t('Cycles') }}</span>
      </div>

      <div class="nexus-controls-row">
        <!-- Unified clustering: Search -> Filter -> Add Button -->
        <div class="flex items-center gap-2" v-if="showCycleSearch">
           <input v-model="cycleSearchQuery" class="nexus-search-input" type="text" :placeholder="t('Search cycles...')" style="width: 200px" />
        </div>
        <button class="nexus-btn-icon" type="button" @click="showCycleSearch = !showCycleSearch" :class="{ active: showCycleSearch }"><i class="fa-solid fa-magnifying-glass"></i></button>
        
        <div class="cycle-filter-wrapper">
          <button class="nexus-btn-outlined" type="button" @click="showCycleFilters = !showCycleFilters" :class="{ active: showCycleFilters || hasCycleFilters }">
            <i class="fa-solid fa-filter"></i> {{ t('Filters') }}
          </button>
          <div class="cycle-filter-menu" v-if="showCycleFilters" @click.stop>
            <div class="filter-title">{{ t('Progress') }}</div>
            <label class="filter-option"><input type="radio" value="all" v-model="cycleProgressFilter" /> {{ t('All cycles') }}</label>
            <label class="filter-option"><input type="radio" value="not-started" v-model="cycleProgressFilter" /> {{ t('Not started') }}</label>
            <label class="filter-option"><input type="radio" value="in-progress" v-model="cycleProgressFilter" /> {{ t('IN PROGRESS') }}</label>
            <label class="filter-option"><input type="radio" value="completed" v-model="cycleProgressFilter" /> {{ t('Completed') }}</label>
            <button class="clear-filter-btn" type="button" @click="clearCycleFilters">{{ t('Clear filters') }}</button>
          </div>
        </div>
        
        <button class="nexus-btn-primary" type="button" @click="showCreateModal = true">
          <i class="fa-solid fa-plus"></i> {{ t('Add cycle') }}
        </button>
      </div>
    </header>

    <div class="cycles-body">
      <div class="cycle-section">
        <div class="cs-header" @click="toggleTab('active')">
          <i class="chevron fa-solid" :class="expandedTabs.active ? 'fa-chevron-down' : 'fa-chevron-right'"></i>
          <i class="fa-solid fa-circle-half-stroke text-orange"></i>
          <span class="cs-title">{{ t('Active cycle') }}</span>
        </div>

        <div class="cs-content" v-show="expandedTabs.active">
          <div class="empty-state text-muted" v-if="activeSprints.length === 0">{{ t('No active cycles.') }}</div>
          <div class="cycle-card expanded" v-for="cycle in activeSprints" :key="cycle.id">
            <div class="cc-top">
              <div class="cct-left">
                <div class="progress-ring text-orange">{{ percentLabel(cycle) }}</div>
                <span class="cycle-name">{{ cycle.name }}</span>
              </div>
              <div class="cct-right">
                <span class="detail-link cursor-pointer hover:text-white" @click.stop="openCycleBoard(cycle)">
                  <i class="fa-solid fa-info-circle"></i> {{ t('Open board') }}
                </span>
                <span class="date-range">
                  <i class="fa-regular fa-calendar"></i>
                  {{ formatDateCompact(cycle.startDate) }} - {{ formatDateCompact(cycle.endDate) }}
                </span>
                <button class="icon-btn" @click.stop="sprintStore.toggleFavorite(props.projectId, cycle.id)">
                  <i class="fa-solid fa-star text-orange-400" v-if="cycle.isFavorite"></i>
                  <i class="fa-regular fa-star" v-else></i>
                </button>
              </div>
            </div>

            <div class="cc-grid">
              <div class="grid-panel panel-progress">
                <div class="gp-header">
                  <span>{{ t('Progress') }}</span>
                  <span class="sub">{{ t('Work items') }}</span>
                </div>
                <div class="progress-bar-container">
                  <div
                    v-for="segment in progressSegments(cycle)"
                    :key="segment.label"
                    class="pb-segment"
                    :class="segment.className"
                    :style="{ width: segment.width }"
                  ></div>
                </div>
                <div class="legend-list">
                  <div v-for="segment in progressSegments(cycle)" :key="segment.label" class="legend-item">
                    <span class="dot" :class="segment.className"></span>
                    {{ segment.label }}
                    <span class="val">{{ segment.value }}</span>
                  </div>
                </div>
              </div>

              <div class="grid-panel panel-chart">
                <div class="gp-header">
                  <span>{{ t('Work item burndown') }}</span>
                  <span class="sub text-right">{{ percentLabel(cycle) }}</span>
                </div>
                <div class="chart-mockup" style="height: 140px;">
                  <v-chart v-if="burndownCharts[cycle.id]" :option="burndownCharts[cycle.id]" autoresize />
                  <div v-else class="text-muted text-xs text-center pt-8">{{ t('No burndown data yet.') }}</div>
                </div>
              </div>

              <div class="grid-panel panel-tabs">
                <div class="tabs-header">
                  <button class="tab-h" :class="{ active: getCyclePanelTab(cycle.id) === 'state' }" @click="setCyclePanelTab(cycle, 'state')">{{ t('Cycle state') }}</button>
                  <button class="tab-h" :class="{ active: getCyclePanelTab(cycle.id) === 'items' }" @click="setCyclePanelTab(cycle, 'items')">{{ t('Work items') }}</button>
                </div>
                <div class="tabs-body" v-if="getCyclePanelTab(cycle.id) === 'state'">
                  <div class="tab-row">
                    <div class="tr-user">
                      <i class="fa-solid fa-arrows-spin avatar-icon"></i> {{ cycle.state }}
                    </div>
                    <div class="tr-stat text-muted">{{ activeItemCount(cycle) }} {{ t('items') }}</div>
                  </div>
                  <div class="tab-row">
                    <div class="tr-user">
                      <i class="fa-solid fa-circle-check avatar-icon"></i> {{ t('Completed') }}
                    </div>
                    <div class="tr-stat text-muted">{{ cycle.completedTaskCount || 0 }}</div>
                  </div>
                </div>
                <div class="tabs-body work-items-body" v-else>
                  <div v-if="cycleWorkItemsLoading[cycle.id]" class="tab-empty text-muted">{{ t('Loading work items...') }}</div>
                  <div v-else-if="cycleItemsFor(cycle.id).length === 0" class="tab-empty text-muted">{{ t('No work items in this cycle.') }}</div>
                  <div v-else class="cycle-work-item" v-for="item in cycleItemsFor(cycle.id)" :key="item.id">
                    <div class="work-item-main">
                      <span class="work-item-id">{{ item.sequenceId || item.id?.substring(0, 8).toUpperCase() }}</span>
                      <span class="work-item-title">{{ item.title }}</span>
                    </div>
                    <div class="work-item-meta">
                     <span>{{ t(item.statusName || 'BACKLOG') }}</span>
                      <span>{{ priorityLabel(item.priority) }}</span>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="cycle-section">
        <div class="cs-header" @click="toggleTab('upcoming')">
          <i class="chevron fa-solid" :class="expandedTabs.upcoming ? 'fa-chevron-down' : 'fa-chevron-right'"></i>
          <i class="fa-regular fa-circle-dashed text-blue"></i>
          <span class="cs-title">{{ t('Upcoming cycle') }}</span>
          <span class="cs-count">{{ upcomingSprints.length }}</span>
        </div>

        <div class="cs-content" v-show="expandedTabs.upcoming">
          <div class="empty-state text-muted" v-if="upcomingSprints.length === 0">{{ t('No upcoming cycles.') }}</div>
          <div class="cycle-card collapsed hover-card" v-for="cycle in upcomingSprints" :key="cycle.id">
            <div class="cct-left">
              <div class="progress-ring text-muted">{{ percentLabel(cycle) }}</div>
              <span class="cycle-name">{{ cycle.name }}</span>
            </div>
            <div class="cct-right">
              <span class="date-range mr-4">
                <i class="fa-regular fa-calendar"></i>
                {{ formatDateCompact(cycle.startDate) }} - {{ formatDateCompact(cycle.endDate) }}
              </span>
              <button class="btn-primary" style="margin-right: 8px;" @click.stop="startCycle(cycle)">
                <i class="fa-solid fa-play"></i> Start cycle
              </button>
              <span class="detail-link cursor-pointer hover:text-white" @click.stop="openCycleBoard(cycle)">
                <i class="fa-solid fa-info-circle"></i> {{ t('Open board') }}
              </span>
              <button class="icon-btn" @click.stop="sprintStore.toggleFavorite(props.projectId, cycle.id)">
                <i class="fa-solid fa-star text-orange-400" v-if="cycle.isFavorite"></i>
                <i class="fa-regular fa-star" v-else></i>
              </button>
            </div>
          </div>
        </div>
      </div>

      <div class="cycle-section">
        <div class="cs-header" @click="toggleTab('completed')">
          <i class="chevron fa-solid" :class="expandedTabs.completed ? 'fa-chevron-down' : 'fa-chevron-right'"></i>
          <i class="fa-solid fa-circle-check text-green"></i>
          <span class="cs-title">{{ t('Completed cycle') }}</span>
          <span class="cs-count">{{ completedSprints.length }}</span>
        </div>

        <div class="cs-content" v-show="expandedTabs.completed">
          <div class="empty-state text-muted" v-if="completedSprints.length === 0">{{ t('No completed cycles yet.') }}</div>
          <div v-for="cycle in completedSprints" :key="cycle.id" class="completed-cycle-wrapper">
            <div class="cycle-card collapsed hover-card">
              <div class="cct-left">
                <div class="progress-ring text-green">{{ percentLabel(cycle) }}</div>
                <span class="cycle-name">{{ cycle.name }}</span>
              </div>
              <div class="cct-right">
                <span class="completed-badge">{{ t('Completed') }}</span>
                <span class="detail-link cursor-pointer hover:text-white" @click.stop="openCycleBoard(cycle)">
                  <i class="fa-solid fa-info-circle"></i> {{ t('Open board') }}
                </span>
                <span class="date-range">
                  <i class="fa-regular fa-calendar"></i>
                  {{ formatDateCompact(cycle.startDate) }} - {{ formatDateCompact(cycle.endDate) }}
                </span>
                <span class="task-count-badge" v-if="cycle.taskCount">
                  <i class="fa-solid fa-layer-group"></i> {{ cycle.taskCount }}
                </span>
                <button class="icon-btn" @click.stop="sprintStore.toggleFavorite(props.projectId, cycle.id)">
                  <i class="fa-solid fa-star text-orange-400" v-if="cycle.isFavorite"></i>
                  <i class="fa-regular fa-star" v-else></i>
                </button>
                <button class="filter-action" type="button" @click.stop="toggleCarryOverPlanner(cycle)" :class="{ active: expandedCarryOverCycleId === cycle.id }">
                  <i class="fa-solid fa-list-check"></i> {{ t('Carry-over') }}
                </button>
              </div>
            </div>

            <div v-if="expandedCarryOverCycleId === cycle.id" class="carry-over-panel">
              <div class="carry-over-toolbar">
                <div class="carry-over-filters">
                  <input
                    class="carry-over-search"
                    :value="getCarryOverFilter(cycle.id).search"
                    type="text"
                    :placeholder="t('Search carry-over tasks')"
                    @input="updateCarryOverFilter(cycle.id, { search: $event.target.value })"
                  />
                  <select
                    class="carry-over-select"
                    :value="getCarryOverFilter(cycle.id).scope"
                    @change="updateCarryOverFilter(cycle.id, { scope: $event.target.value })"
                  >
                    <option value="all">{{ t('All locations') }}</option>
                    <option value="backlog">{{ t('Backlog only') }}</option>
                    <option value="cycle">{{ t('Cycle only') }}</option>
                  </select>
                  <button class="filter-action" type="button" @click="fetchCarryOverTasks(cycle.id, true)">{{ t('Refresh') }}</button>
                  <button class="filter-action" type="button" @click="openCarryOverBoard(cycle)">{{ t('Open board') }}</button>
                </div>
                <div class="carry-over-actions">
                  <span class="text-muted">{{ carryOverSelectedCount(cycle.id) }} {{ t('selected') }}</span>
                  <select
                    class="carry-over-select"
                    :value="getCarryOverTargetSprintId(cycle.id)"
                    @change="carryOverTargetSprintId = { ...carryOverTargetSprintId, [cycle.id]: $event.target.value || null }"
                  >
                    <option :value="null">{{ t('Move to cycle...') }}</option>
                    <option v-for="target in availableTargetSprints(cycle.id)" :key="target.id" :value="target.id">
                      {{ target.name }}
                    </option>
                  </select>
                  <button class="filter-action" type="button" :disabled="carryOverMoving[cycle.id]" @click="moveCarryOverTasks(cycle.id, null)">{{ t('Move to backlog') }}</button>
                  <button
                    class="primary-action"
                    type="button"
                    :disabled="carryOverMoving[cycle.id] || !getCarryOverTargetSprintId(cycle.id)"
                    @click="moveCarryOverTasks(cycle.id, getCarryOverTargetSprintId(cycle.id))"
                  >
                    {{ t('Move selected') }}
                  </button>
                </div>
              </div>

              <div v-if="carryOverLoading[cycle.id]" class="tab-empty text-muted">{{ t('Loading carry-over tasks...') }}</div>
              <div v-else-if="visibleCarryOverTasks(cycle.id).length === 0" class="tab-empty text-muted">{{ t('No carry-over tasks from this completed cycle.') }}</div>
              <div v-else class="carry-over-table">
                <div class="carry-over-head">
                  <label class="carry-over-check">
                    <input type="checkbox" :checked="visibleCarryOverTasks(cycle.id).length > 0 && carryOverSelectedCount(cycle.id) === visibleCarryOverTasks(cycle.id).length" @change="toggleSelectAllCarryOver(cycle.id)" />
                    <span>{{ t('Select all visible') }}</span>
                  </label>
                  <span class="text-muted">{{ visibleCarryOverTasks(cycle.id).length }} {{ t('tasks') }}</span>
                </div>

                <div v-for="task in visibleCarryOverTasks(cycle.id)" :key="task.id" class="carry-over-row">
                  <label class="carry-over-check">
                    <input type="checkbox" :checked="getCarryOverSelection(cycle.id).includes(task.id)" @change="toggleCarryOverSelection(cycle.id, task.id)" />
                  </label>
                  <div class="carry-over-main">
                    <span class="work-item-id">{{ task.sequenceId || task.id?.substring(0, 8).toUpperCase() }}</span>
                    <span class="work-item-title">{{ task.title }}</span>
                  </div>
                  <div class="carry-over-meta">
                    <span>{{ t(task.statusName) }}</span>
                    <span>{{ priorityLabel(task.priority) }}</span>
                    <span>{{ assigneeLabel(task) }}</span>
                    <span>{{ task.currentSprintName || t('BACKLOG') }}</span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div class="modal-overlay" v-if="showCreateModal" @click.self="showCreateModal = false; showCalendar = false">
      <div class="create-cycle-modal">
        <div class="cm-header">
          <div class="cm-badge"><i class="fa-solid fa-certificate text-orange"></i> CYBWF</div>
          <h2 class="cm-title">{{ t('Create cycle') }}</h2>
        </div>

        <div class="cm-body" :style="{ paddingBottom: showCalendar ? '300px' : '24px' }">
          <input v-model="newCycle.name" type="text" class="cm-input" :placeholder="t('Title')" autofocus />
          <textarea v-model="newCycle.description" class="cm-textarea" :placeholder="t('Description')" rows="4"></textarea>

          <div class="dp-wrapper mt-4">
            <button class="dp-btn" @click="toggleCalendar">
              <i class="fa-regular fa-calendar"></i> {{ btnDateText }}
            </button>

            <div class="dp-popover" v-if="showCalendar" @click.stop>

              <div class="dp-header">
                <div class="dp-month-year">
                  <span>{{ t(monthNames[currentMonth]) }}</span>
                  <span>{{ currentYear }}</span>
                </div>
                <div class="dp-nav">
                  <button @click="moveMonth(-1)"><i class="fa-solid fa-chevron-left"></i></button>
                  <button @click="moveMonth(1)"><i class="fa-solid fa-chevron-right"></i></button>
                </div>
              </div>

              <div class="dp-grid">
                <div class="dp-day-num headday" v-for="dayName in dayNames" :key="dayName">{{ t(dayName) }}</div>

                <div class="dp-day-wrapper" v-for="(day, index) in daysInMonth" :key="index">
                  <div
                    class="dp-bg-range"
                    v-if="isInRange(day) || (isSelectedStart(day.date) && tempEnd) || isSelectedEnd(day.date)"
                    :class="{ 'range-start': isSelectedStart(day.date) && tempEnd, 'range-end': isSelectedEnd(day.date), 'range-mid': isInRange(day) }"
                  ></div>
                  <div class="dp-day-num" :class="{ 'current-month': day.isCurrent, selected: isSelectedStart(day.date) || isSelectedEnd(day.date), disabled: !day.isCurrent || isPastDate(day.date) }" @click="selectDate(day)">
                    {{ day.day }}
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div class="cm-footer">
          <button class="cm-btn-cancel" @click="showCreateModal = false">{{ t('Cancel') }}</button>
          <button class="cm-btn-create" @click="createNewCycle">{{ t('Create cycle') }}</button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.plane-cycles-wrapper {
  display: flex;
  flex-direction: column;
  height: 100%;
  color: var(--color-text-primary);
  font-family: inherit;
  background: var(--color-bg);
  min-height: calc(100vh - 100px);
}

.text-muted { color: var(--color-text-muted); }
.text-orange { color: #F59E0B; }
.text-green { color: #10B981; }
.text-blue { color: #3B82F6; }
.bg-green { background-color: #10B981; }
.bg-orange { background-color: #F59E0B; }
.bg-darkgray { background-color: #3F3F46; }
.bg-lightgray { background-color: var(--color-text-muted); }

.cycles-view-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 24px;
  border-bottom: 1px solid var(--color-border);
}

.vh-right { display: flex; align-items: center; gap: 12px; }
.icon-action { background: transparent; border: none; color: var(--color-text-muted); cursor: pointer; font-size: 14px; border-radius: 6px; padding: 6px 8px; }
.icon-action:hover { color: var(--color-text-primary); }
.icon-action.active { color: var(--color-text-primary); background: var(--color-border); }
.filter-action { background: transparent; border: 1px solid var(--color-border); color: var(--color-text-primary); padding: 6px 12px; border-radius: 6px; font-size: 13px; cursor: pointer; display: flex; align-items: center; gap: 6px; }
.filter-action:hover { background: var(--color-border); }
.filter-action.active { background: var(--color-border); border-color: #3F3F46; }
.cycle-search-wrapper {
  display: flex;
  align-items: center;
  gap: 8px;
  width: 220px;
  border: 1px solid var(--color-border);
  border-radius: 6px;
  padding: 5px 10px;
  background: var(--color-surface);
}
.cycle-search-wrapper i { color: var(--color-text-muted); font-size: 12px; }
.cycle-search-wrapper input {
  flex: 1;
  min-width: 0;
  background: transparent;
  border: 0;
  color: var(--color-text-primary);
  outline: none;
  font-size: 13px;
}
.cycle-filter-wrapper { position: relative; }
.cycle-filter-menu {
  position: absolute;
  top: calc(100% + 8px);
  right: 0;
  width: 220px;
  background: #1B1C20;
  border: 1px solid #2D2F36;
  border-radius: 8px;
  padding: 12px;
  z-index: 20;
  box-shadow: 0 12px 30px rgba(0, 0, 0, 0.35);
}
.filter-title { color: var(--color-text-muted); font-size: 12px; font-weight: 600; margin-bottom: 8px; }
.filter-option {
  display: flex;
  align-items: center;
  gap: 8px;
  color: #D4D4D8;
  font-size: 13px;
  padding: 6px 0;
  cursor: pointer;
}
.clear-filter-btn {
  width: 100%;
  margin-top: 8px;
  border: 1px solid var(--color-border);
  border-radius: 6px;
  background: var(--color-surface);
  color: #D4D4D8;
  padding: 7px;
  cursor: pointer;
}
.clear-filter-btn:hover { background: var(--color-border); }
.primary-action { background: #0EA5E9; color: white; border: none; border-radius: 6px; padding: 6px 16px; font-size: 13px; cursor: pointer; font-weight: 500; }
.primary-action:hover { background: #0284C7; }

.cycles-body { padding: 24px; flex: 1; }
.cycle-section { margin-bottom: 24px; }
.cs-header { display: flex; align-items: center; gap: 12px; padding: 8px 0; cursor: pointer; user-select: none; }
.chevron { font-size: 12px; color: var(--color-text-muted); width: 16px; text-align: center; }
.cs-title { font-size: 14px; font-weight: 600; color: var(--color-text-primary); }
.cs-count { font-size: 12px; color: var(--color-text-muted); background: var(--color-border); padding: 2px 8px; border-radius: 12px; }
.cs-content { padding-left: 28px; margin-top: 12px; display: flex; flex-direction: column; gap: 12px; }

.cycle-card {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 14px !important;
  overflow: hidden;
  transition: all 0.25s cubic-bezier(0.4, 0, 0.2, 1);
  box-shadow: var(--shadow-sm);
}

.cycle-card.hover-card:hover { 
  border-color: var(--color-accent); 
  background: var(--color-surface-hover); 
  transform: translateX(4px);
  box-shadow: var(--shadow-md);
}
.cycle-card.collapsed { 
  display: flex; 
  justify-content: space-between; 
  align-items: center; 
  padding: 16px 20px; 
}
.cc-top { display: flex; justify-content: space-between; align-items: center; padding: 16px 20px; border-bottom: 1px solid var(--color-border); }
.cct-left, .cct-right { display: flex; align-items: center; gap: 16px; }
.progress-ring { 
  width: 36px; 
  height: 36px; 
  border-radius: 50%; 
  border: 2.5px solid currentColor; 
  display: flex; 
  align-items: center; 
  justify-content: center; 
  font-size: 11px; 
  font-weight: 700; 
  flex-shrink: 0;
}
.cycle-name { font-size: 15px; font-weight: 700; color: var(--color-text-primary); }
.detail-link { 
  font-size: 13px; 
  color: var(--color-accent); 
  display: inline-flex; 
  align-items: center; 
  gap: 6px; 
  font-weight: 600;
  transition: color 0.2s;
}
.detail-link:hover {
  color: var(--color-accent-hover);
  text-decoration: underline;
}
.date-range { 
  font-size: 12px; 
  color: var(--color-text-secondary); 
  display: flex; 
  align-items: center; 
  gap: 6px; 
  background: var(--color-border); 
  padding: 5px 12px; 
  border-radius: 8px !important; 
  font-weight: 500;
}
.icon-btn { background: transparent; border: none; color: var(--color-text-muted); cursor: pointer; font-size: 14px; transition: color 0.2s; }
.icon-btn:hover { color: var(--color-text-primary); }
.completed-badge { font-size: 12px; color: #10B981; font-weight: 700; }
.task-count-badge { font-size: 12px; color: #A1A1AA; display: flex; align-items: center; gap: 6px; }
.carry-over-panel {
  margin-top: 12px;
  border: 1px solid var(--border-color);
  border-radius: 10px;
  background: var(--bg-tertiary);
  padding: 16px;
}
.carry-over-toolbar {
  display: flex;
  justify-content: space-between;
  gap: 12px;
  flex-wrap: wrap;
  margin-bottom: 12px;
}
.carry-over-filters,
.carry-over-actions {
  display: flex;
  align-items: center;
  gap: 10px;
  flex-wrap: wrap;
}
.carry-over-search,
.carry-over-select {
  background: var(--bg-primary);
  border: 1px solid var(--border-color);
  color: var(--text-primary);
  border-radius: 8px;
  padding: 8px 10px;
  font-size: 13px;
}
.carry-over-search {
  min-width: 220px;
}
.carry-over-table {
  display: grid;
  gap: 8px;
}
.carry-over-head,
.carry-over-row {
  display: grid;
  grid-template-columns: 34px minmax(0, 1fr) minmax(280px, 420px);
  gap: 12px;
  align-items: center;
}
.carry-over-head {
  color: #A1A1AA;
  font-size: 12px;
  padding: 4px 0;
}
.carry-over-row {
  border: 1px solid #27272A;
  border-radius: 8px;
  padding: 10px 12px;
  background: #16181D;
}
.carry-over-check {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  font-size: 12px;
}
.carry-over-main {
  min-width: 0;
  display: flex;
  align-items: center;
  gap: 10px;
}
.carry-over-meta {
  display: flex;
  justify-content: flex-end;
  gap: 8px;
  flex-wrap: wrap;
  color: #A1A1AA;
  font-size: 11px;
}
.carry-over-meta span {
  background: #27272A;
  border-radius: 999px;
  padding: 4px 8px;
}

.cc-grid { 
  display: grid; 
  grid-template-columns: 1fr 2fr 1.5fr; 
  background: rgba(0, 0, 0, 0.02);
  border-top: 1px solid var(--color-border);
}
[data-theme='dark'] .cc-grid {
  background: rgba(255, 255, 255, 0.01);
}
@media (max-width: 1024px) {
  .cc-grid {
    grid-template-columns: 1fr;
  }
  .grid-panel {
    border-right: none !important;
    border-bottom: 1px solid var(--color-border);
  }
  .grid-panel:last-child {
    border-bottom: none;
  }
}
.grid-panel { padding: 24px; border-right: 1px solid var(--color-border); }
.grid-panel:last-child { border-right: none; }
.gp-header { 
  display: flex; 
  justify-content: space-between; 
  font-size: 14px; 
  font-weight: 750; 
  color: var(--color-text-primary);
  margin-bottom: 20px; 
}
.gp-header .sub { color: var(--color-text-muted); font-weight: 500; }

.progress-bar-container { display: flex; height: 8px; border-radius: 999px; overflow: hidden; background: var(--color-border); margin-bottom: 20px; }
.pb-segment { height: 100%; }
.legend-list { display: flex; flex-direction: column; gap: 12px; }
.legend-item { display: flex; align-items: center; font-size: 12px; color: var(--color-text-muted); }
.legend-item .dot { width: 8px; height: 8px; border-radius: 50%; margin-right: 10px; }
.legend-item .val { margin-left: auto; color: var(--color-text-primary); font-weight: 650; }

.tabs-header { display: flex; border-bottom: 1px solid var(--color-border); margin-bottom: 16px; }
.tab-h { padding: 0 12px 8px 12px; font-size: 12.5px; color: var(--color-text-muted); border: none; border-bottom: 2px solid transparent; background: transparent; cursor: pointer; font-weight: 600; }
.tab-h.active { color: var(--color-text-primary); border-bottom-color: var(--color-accent); font-weight: 700; }
.tab-row { display: flex; justify-content: space-between; font-size: 12.5px; padding: 10px 12px; border-radius: 8px; background: var(--color-surface-hover); border: 1px solid var(--color-border); }
.tr-user { display: flex; align-items: center; gap: 10px; color: var(--color-text-primary); font-weight: 600; }
.avatar-icon { background: var(--color-border); border-radius: 50%; width: 24px; height: 24px; display: flex; align-items: center; justify-content: center; }
.work-items-body { display: flex; flex-direction: column; gap: 8px; max-height: 180px; overflow-y: auto; padding-right: 4px; }
.tab-empty { font-size: 12px; padding: 8px 12px; color: var(--color-text-muted); }
.cycle-work-item { 
  display: flex; 
  justify-content: space-between; 
  align-items: center; 
  gap: 12px; 
  padding: 10px 14px; 
  border: 1px solid var(--color-border); 
  border-radius: 10px !important; 
  background: var(--color-surface); 
  transition: all 0.2s;
  cursor: pointer;
}
.cycle-work-item:hover {
  border-color: var(--color-accent);
  background: var(--color-surface-hover);
}
.work-item-main { min-width: 0; display: flex; align-items: center; gap: 10px; }
.work-item-id { 
  color: var(--color-text-muted); 
  font-size: 10.5px; 
  font-family: monospace; 
  font-weight: 700; 
  background: var(--color-bg); 
  border: 1px solid var(--color-border); 
  padding: 2px 6px; 
  border-radius: 4px; 
  flex-shrink: 0; 
}
.work-item-title { color: var(--color-text-primary); font-size: 12.5px; font-weight: 600; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.work-item-meta { display: flex; align-items: center; gap: 8px; flex-shrink: 0; color: var(--color-text-secondary); font-size: 11px; }
.work-item-meta span { background: var(--color-border); border-radius: 6px; padding: 3px 8px; font-weight: 600; }

.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.create-cycle-modal {
  width: 600px;
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 12px;
  box-shadow: var(--shadow-xl);
}

.cm-header { padding: 24px 24px 16px; }
.cm-badge { display: inline-flex; align-items: center; gap: 8px; border: 1px solid var(--color-border); background: var(--color-bg-secondary); padding: 4px 10px; border-radius: 6px; font-size: 12px; color: var(--color-text-primary); font-weight: 500; margin-bottom: 16px; }
.cm-title { font-size: 20px; font-weight: 600; color: var(--color-text-primary); margin: 0; }
.cm-body { padding: 0 24px 24px; display: flex; flex-direction: column; }
.cm-input, .cm-textarea { width: 100%; background: var(--color-bg-secondary); border: 1px solid var(--color-border); border-radius: 8px; padding: 12px 16px; color: var(--color-text-primary); outline: none; transition: border-color 0.2s; }
.cm-input:focus, .cm-textarea:focus { border-color: var(--color-accent); }
.cm-input { margin-bottom: 16px; font-size: 15px; }
.cm-textarea { font-size: 14px; resize: none; }
.cm-footer { padding: 16px 24px; border-top: 1px solid var(--color-border); display: flex; justify-content: flex-end; gap: 12px; background: var(--color-surface-hover); border-bottom-left-radius: 12px; border-bottom-right-radius: 12px; }
.cm-btn-cancel { background: transparent; border: 1px solid var(--color-border); border-radius: 8px; padding: 8px 16px; color: var(--color-text-primary); font-size: 13px; font-weight: 500; cursor: pointer; transition: background 0.2s; }
.cm-btn-cancel:hover { background: var(--color-surface-hover); }
.cm-btn-create { background: var(--color-accent); border: none; border-radius: 8px; padding: 8px 16px; color: white; font-size: 13px; font-weight: 500; cursor: pointer; transition: opacity 0.2s; }
.cm-btn-create:hover { opacity: 0.9; }

.dp-wrapper { position: relative; }
.dp-btn { background: var(--color-bg-secondary); border: 1px solid var(--color-border); color: var(--color-text-primary); padding: 8px 14px; border-radius: 8px; font-size: 13px; cursor: pointer; display: flex; align-items: center; gap: 8px; transition: border-color 0.2s; }
.dp-btn:hover { border-color: var(--color-accent); }
.dp-popover { position: absolute; top: 100%; left: 0; margin-top: 8px; background: var(--color-surface); border: 1px solid var(--color-border); border-radius: 12px; width: 280px; padding: 16px; box-shadow: var(--shadow-lg); z-index: 1001; }
.dp-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px; }
.dp-month-year { display: flex; gap: 12px; font-size: 14px; font-weight: 600; color: var(--color-text-primary); }
.dp-nav { display: flex; gap: 8px; }
.dp-nav button { background: transparent; border: none; color: var(--color-text-muted); cursor: pointer; transition: color 0.2s; }
.dp-nav button:hover { color: var(--color-text-primary); }
.dp-grid { display: grid; grid-template-columns: repeat(7, 1fr); row-gap: 6px; }
.headday { font-size: 10px; font-weight: 600; color: var(--color-text-muted); text-align: center; }
.dp-day-wrapper { position: relative; display: flex; align-items: center; justify-content: center; height: 32px; }
.dp-bg-range { position: absolute; inset: 0; background: color-mix(in srgb, var(--color-accent) 20%, transparent); z-index: 1; }
.range-start { border-top-left-radius: 16px; border-bottom-left-radius: 16px; }
.range-end { border-top-right-radius: 16px; border-bottom-right-radius: 16px; }
.dp-day-num { position: relative; z-index: 2; width: 32px; height: 32px; display: flex; align-items: center; justify-content: center; border-radius: 50%; font-size: 12px; color: var(--color-text-muted); cursor: pointer; transition: all 0.2s; }
.dp-day-num.current-month { color: var(--color-text-primary); }
.dp-day-num:hover:not(.headday):not(.disabled) { background: var(--color-surface-hover); color: var(--color-text-primary); }
.dp-day-num.selected { background: var(--color-accent) !important; color: white !important; }
.dp-day-num.disabled { color: var(--color-text-disabled); cursor: not-allowed; opacity: 0.5; }
</style>



