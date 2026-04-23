<script setup>
import { computed, onMounted, onUnmounted, provide, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useSprintStore } from '@/store/useSprintStore'
import axiosClient from '@/api/axiosClient'

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

const priorityLabel = (priority) => {
  if (priority === 1) return 'Urgent'
  if (priority === 2) return 'High'
  if (priority === 3) return 'Normal'
  if (priority === 4) return 'Low'
  return 'None'
}

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
    { label: 'Completed', value: completed, width: `${(completed / total) * 100}%`, className: 'bg-green' },
    { label: 'Started', value: started, width: `${(started / total) * 100}%`, className: 'bg-orange' },
    { label: 'Backlog', value: backlog, width: `${(backlog / total) * 100}%`, className: 'bg-lightgray' },
    { label: 'Other', value: remaining, width: `${(remaining / total) * 100}%`, className: 'bg-darkgray' }
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
          textStyle: { color: '#A1A1AA', fontSize: 10 }
        },
        grid: { top: 10, left: 30, right: 10, bottom: 40 },
        xAxis: {
          type: 'category',
          data: burndown.map(item => item.date),
          axisLine: { show: false },
          axisTick: { show: false },
          axisLabel: { color: '#71717A', fontSize: 9 }
        },
        yAxis: {
          type: 'value',
          splitLine: { lineStyle: { color: 'rgba(255,255,255,0.05)' } },
          axisLabel: { color: '#71717A', fontSize: 10 }
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
            itemStyle: { color: '#A1A1AA' },
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
    alert(error.response?.data?.message || 'Khong the tao cycle')
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
  if (!newCycle.value.startDate) return 'Chon khoang thoi gian'
  const start = formatDateCompact(newCycle.value.startDate)
  const end = newCycle.value.endDate ? formatDateCompact(newCycle.value.endDate) : '...'
  return `${start} -> ${end}`
})

watch(() => props.projectId, () => loadCycles(true), { immediate: true })

let cycleRefreshTimer = null
onMounted(() => {
  cycleRefreshTimer = window.setInterval(() => {
    if (props.projectId) {
      loadCycles()
    }
  }, 60000)
})

onUnmounted(() => {
  if (cycleRefreshTimer) {
    window.clearInterval(cycleRefreshTimer)
  }
})
</script>

<template>
  <div class="plane-cycles-wrapper">
    <header class="nexus-project-header">
      <div class="nexus-breadcrumb">
        <div class="project-icon" style="background: #F59E0B">
          <i class="fa-solid fa-certificate"></i>
        </div>
        <span class="view-name">Cycles</span>
      </div>

      <div class="nexus-controls-row">
        <div class="nexus-controls-row" v-if="showCycleSearch">
          <input v-model="cycleSearchQuery" class="nexus-search-input" type="text" placeholder="Search cycles..." />
        </div>
        <button class="nexus-btn nexus-btn-icon" type="button" @click="showCycleSearch = !showCycleSearch" :class="{ active: showCycleSearch }"><i class="fa-solid fa-magnifying-glass"></i></button>
        
        <div class="cycle-filter-wrapper">
          <button class="nexus-btn nexus-btn-outlined" type="button" @click="showCycleFilters = !showCycleFilters" :class="{ active: showCycleFilters || hasCycleFilters }">
            <i class="fa-solid fa-filter"></i> Filters
          </button>
          <div class="cycle-filter-menu" v-if="showCycleFilters" @click.stop>
            <div class="filter-title">Progress</div>
            <label class="filter-option"><input type="radio" value="all" v-model="cycleProgressFilter" /> All cycles</label>
            <label class="filter-option"><input type="radio" value="not-started" v-model="cycleProgressFilter" /> Not started</label>
            <label class="filter-option"><input type="radio" value="in-progress" v-model="cycleProgressFilter" /> In progress</label>
            <label class="filter-option"><input type="radio" value="completed" v-model="cycleProgressFilter" /> Completed</label>
            <button class="clear-filter-btn" type="button" @click="clearCycleFilters">Clear filters</button>
          </div>
        </div>
        <button class="nexus-btn nexus-btn-primary" type="button" @click="showCreateModal = true"><i class="fa-solid fa-plus"></i> Add cycle</button>
      </div>
    </header>

    <div class="cycles-body">
      <div class="cycle-section">
        <div class="cs-header" @click="toggleTab('active')">
          <i class="chevron fa-solid" :class="expandedTabs.active ? 'fa-chevron-down' : 'fa-chevron-right'"></i>
          <i class="fa-solid fa-circle-half-stroke text-orange"></i>
          <span class="cs-title">Active cycle</span>
        </div>

        <div class="cs-content" v-show="expandedTabs.active">
          <div class="empty-state text-muted" v-if="activeSprints.length === 0">No active cycles.</div>
          <div class="cycle-card expanded" v-for="cycle in activeSprints" :key="cycle.id">
            <div class="cc-top">
              <div class="cct-left">
                <div class="progress-ring text-orange">{{ percentLabel(cycle) }}</div>
                <span class="cycle-name">{{ cycle.name }}</span>
              </div>
              <div class="cct-right">
                <span class="detail-link cursor-pointer hover:text-white" @click.stop="openCycleBoard(cycle)">
                  <i class="fa-solid fa-info-circle"></i> Open board
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
                  <span>Progress</span>
                  <span class="sub">Work items</span>
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
                  <span>Work item burndown</span>
                  <span class="sub text-right">{{ percentLabel(cycle) }}</span>
                </div>
                <div class="chart-mockup" style="height: 140px;">
                  <v-chart v-if="burndownCharts[cycle.id]" :option="burndownCharts[cycle.id]" autoresize />
                  <div v-else class="text-muted text-xs text-center pt-8">No burndown data yet.</div>
                </div>
              </div>

              <div class="grid-panel panel-tabs">
                <div class="tabs-header">
                  <button class="tab-h" :class="{ active: getCyclePanelTab(cycle.id) === 'state' }" @click="setCyclePanelTab(cycle, 'state')">Cycle state</button>
                  <button class="tab-h" :class="{ active: getCyclePanelTab(cycle.id) === 'items' }" @click="setCyclePanelTab(cycle, 'items')">Work items</button>
                </div>
                <div class="tabs-body" v-if="getCyclePanelTab(cycle.id) === 'state'">
                  <div class="tab-row">
                    <div class="tr-user">
                      <i class="fa-solid fa-arrows-spin avatar-icon"></i> {{ cycle.state }}
                    </div>
                    <div class="tr-stat text-muted">{{ activeItemCount(cycle) }} items</div>
                  </div>
                  <div class="tab-row">
                    <div class="tr-user">
                      <i class="fa-solid fa-circle-check avatar-icon"></i> Completed
                    </div>
                    <div class="tr-stat text-muted">{{ cycle.completedTaskCount || 0 }}</div>
                  </div>
                </div>
                <div class="tabs-body work-items-body" v-else>
                  <div v-if="cycleWorkItemsLoading[cycle.id]" class="tab-empty text-muted">Loading work items...</div>
                  <div v-else-if="cycleItemsFor(cycle.id).length === 0" class="tab-empty text-muted">No work items in this cycle.</div>
                  <div v-else class="cycle-work-item" v-for="item in cycleItemsFor(cycle.id)" :key="item.id">
                    <div class="work-item-main">
                      <span class="work-item-id">{{ item.sequenceId || item.id?.substring(0, 8).toUpperCase() }}</span>
                      <span class="work-item-title">{{ item.title }}</span>
                    </div>
                    <div class="work-item-meta">
                      <span>{{ item.statusName || 'Backlog' }}</span>
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
          <span class="cs-title">Upcoming cycle</span>
          <span class="cs-count">{{ upcomingSprints.length }}</span>
        </div>

        <div class="cs-content" v-show="expandedTabs.upcoming">
          <div class="empty-state text-muted" v-if="upcomingSprints.length === 0">No upcoming cycles.</div>
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
              <span class="detail-link cursor-pointer hover:text-white" @click.stop="openCycleBoard(cycle)">
                <i class="fa-solid fa-info-circle"></i> Open board
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
          <span class="cs-title">Completed cycle</span>
          <span class="cs-count">{{ completedSprints.length }}</span>
        </div>

        <div class="cs-content" v-show="expandedTabs.completed">
          <div class="empty-state text-muted" v-if="completedSprints.length === 0">No completed cycles yet.</div>
          <div v-for="cycle in completedSprints" :key="cycle.id" class="completed-cycle-wrapper">
            <div class="cycle-card collapsed hover-card">
              <div class="cct-left">
                <div class="progress-ring text-green">{{ percentLabel(cycle) }}</div>
                <span class="cycle-name">{{ cycle.name }}</span>
              </div>
              <div class="cct-right">
                <span class="completed-badge">Completed</span>
                <span class="detail-link cursor-pointer hover:text-white" @click.stop="openCycleBoard(cycle)">
                  <i class="fa-solid fa-info-circle"></i> Open board
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
          <h2 class="cm-title">Create cycle</h2>
        </div>

        <div class="cm-body" :style="{ paddingBottom: showCalendar ? '300px' : '24px' }">
          <input v-model="newCycle.name" type="text" class="cm-input" placeholder="Title" autofocus />
          <textarea v-model="newCycle.description" class="cm-textarea" placeholder="Description" rows="4"></textarea>

          <div class="dp-wrapper mt-4">
            <button class="dp-btn" @click="toggleCalendar">
              <i class="fa-regular fa-calendar"></i> {{ btnDateText }}
            </button>

            <div class="dp-popover" v-if="showCalendar" @click.stop>

              <div class="dp-header">
                <div class="dp-month-year">
                  <span>{{ monthNames[currentMonth] }}</span>
                  <span>{{ currentYear }}</span>
                </div>
                <div class="dp-nav">
                  <button @click="moveMonth(-1)"><i class="fa-solid fa-chevron-left"></i></button>
                  <button @click="moveMonth(1)"><i class="fa-solid fa-chevron-right"></i></button>
                </div>
              </div>

              <div class="dp-grid">
                <div class="dp-day-num headday" v-for="dayName in dayNames" :key="dayName">{{ dayName }}</div>

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
          <button class="cm-btn-cancel" @click="showCreateModal = false">Cancel</button>
          <button class="cm-btn-create" @click="createNewCycle">Create Cycle</button>
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
  color: #E4E4E7;
  font-family: inherit;
  background: #0D0F11;
  min-height: calc(100vh - 100px);
}

.text-muted { color: #71717A; }
.text-orange { color: #F59E0B; }
.text-green { color: #10B981; }
.text-blue { color: #3B82F6; }
.bg-green { background-color: #10B981; }
.bg-orange { background-color: #F59E0B; }
.bg-darkgray { background-color: #3F3F46; }
.bg-lightgray { background-color: #71717A; }

.cycles-view-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 24px;
  border-bottom: 1px solid #1E2025;
}

.vh-right { display: flex; align-items: center; gap: 12px; }
.icon-action { background: transparent; border: none; color: #A1A1AA; cursor: pointer; font-size: 14px; border-radius: 6px; padding: 6px 8px; }
.icon-action:hover { color: #E4E4E7; }
.icon-action.active { color: #E4E4E7; background: #1E2025; }
.filter-action { background: transparent; border: 1px solid #27272A; color: #E4E4E7; padding: 6px 12px; border-radius: 6px; font-size: 13px; cursor: pointer; display: flex; align-items: center; gap: 6px; }
.filter-action:hover { background: #1E2025; }
.filter-action.active { background: #1E2025; border-color: #3F3F46; }
.cycle-search-wrapper {
  display: flex;
  align-items: center;
  gap: 8px;
  width: 220px;
  border: 1px solid #27272A;
  border-radius: 6px;
  padding: 5px 10px;
  background: #111315;
}
.cycle-search-wrapper i { color: #71717A; font-size: 12px; }
.cycle-search-wrapper input {
  flex: 1;
  min-width: 0;
  background: transparent;
  border: 0;
  color: #E4E4E7;
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
.filter-title { color: #A1A1AA; font-size: 12px; font-weight: 600; margin-bottom: 8px; }
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
  border: 1px solid #27272A;
  border-radius: 6px;
  background: #111315;
  color: #D4D4D8;
  padding: 7px;
  cursor: pointer;
}
.clear-filter-btn:hover { background: #27272A; }
.primary-action { background: #0EA5E9; color: white; border: none; border-radius: 6px; padding: 6px 16px; font-size: 13px; cursor: pointer; font-weight: 500; }
.primary-action:hover { background: #0284C7; }

.cycles-body { padding: 24px; flex: 1; }
.cycle-section { margin-bottom: 24px; }
.cs-header { display: flex; align-items: center; gap: 12px; padding: 8px 0; cursor: pointer; user-select: none; }
.chevron { font-size: 12px; color: #A1A1AA; width: 16px; text-align: center; }
.cs-title { font-size: 14px; font-weight: 600; color: #E4E4E7; }
.cs-count { font-size: 12px; color: #A1A1AA; background: #27272A; padding: 2px 8px; border-radius: 12px; }
.cs-content { padding-left: 28px; margin-top: 12px; display: flex; flex-direction: column; gap: 12px; }

.cycle-card {
  background: #16181D;
  border: 1px solid #27272A;
  border-radius: 8px;
  overflow: hidden;
  transition: border-color 0.2s, background 0.2s;
}

.cycle-card.hover-card:hover { border-color: #3F3F46; background: #1B1D22; }
.cycle-card.collapsed { display: flex; justify-content: space-between; align-items: center; padding: 12px 16px; }
.cc-top { display: flex; justify-content: space-between; align-items: center; padding: 16px 20px; border-bottom: 1px solid #27272A; }
.cct-left, .cct-right { display: flex; align-items: center; gap: 12px; }
.progress-ring { width: 34px; height: 34px; border-radius: 50%; border: 3px solid currentColor; display: flex; align-items: center; justify-content: center; font-size: 10px; font-weight: 600; }
.cycle-name { font-size: 15px; font-weight: 500; color: #E4E4E7; }
.detail-link { font-size: 13px; color: #3B82F6; display: flex; align-items: center; gap: 6px; }
.date-range { font-size: 12px; color: #A1A1AA; display: flex; align-items: center; gap: 6px; background: #27272A; padding: 4px 10px; border-radius: 6px; }
.icon-btn { background: transparent; border: none; color: #A1A1AA; cursor: pointer; font-size: 14px; transition: color 0.2s; }
.icon-btn:hover { color: #E4E4E7; }
.completed-badge { font-size: 12px; color: #10B981; font-weight: 500; }
.task-count-badge { font-size: 12px; color: #A1A1AA; display: flex; align-items: center; gap: 6px; }

.cc-grid { display: grid; grid-template-columns: 1fr 2fr 1.5fr; }
.grid-panel { padding: 20px; border-right: 1px solid #27272A; }
.grid-panel:last-child { border-right: none; }
.gp-header { display: flex; justify-content: space-between; font-size: 13px; font-weight: 500; margin-bottom: 24px; }
.gp-header .sub { color: #71717A; font-weight: 400; }

.progress-bar-container { display: flex; height: 8px; border-radius: 4px; overflow: hidden; background: #27272A; margin-bottom: 20px; }
.pb-segment { height: 100%; }
.legend-list { display: flex; flex-direction: column; gap: 12px; }
.legend-item { display: flex; align-items: center; font-size: 12px; color: #A1A1AA; }
.legend-item .dot { width: 8px; height: 8px; border-radius: 50%; margin-right: 10px; }
.legend-item .val { margin-left: auto; color: #E4E4E7; }

.tabs-header { display: flex; border-bottom: 1px solid #27272A; margin-bottom: 16px; }
.tab-h { padding: 0 12px 8px 12px; font-size: 12px; color: #71717A; border: none; border-bottom: 2px solid transparent; background: transparent; cursor: pointer; }
.tab-h.active { color: #E4E4E7; border-bottom-color: #38BDF8; font-weight: 500; }
.tab-row { display: flex; justify-content: space-between; font-size: 12px; padding: 8px 12px; border-radius: 4px; }
.tr-user { display: flex; align-items: center; gap: 8px; color: #E4E4E7; }
.avatar-icon { background: #27272A; border-radius: 50%; width: 24px; height: 24px; display: flex; align-items: center; justify-content: center; }
.work-items-body { display: flex; flex-direction: column; gap: 8px; max-height: 150px; overflow-y: auto; padding-right: 4px; }
.tab-empty { font-size: 12px; padding: 8px 12px; }
.cycle-work-item { display: flex; justify-content: space-between; gap: 12px; padding: 8px 10px; border: 1px solid #27272A; border-radius: 6px; background: #111315; }
.work-item-main { min-width: 0; display: flex; align-items: center; gap: 8px; }
.work-item-id { color: #71717A; font-size: 11px; flex-shrink: 0; }
.work-item-title { color: #E4E4E7; font-size: 12px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.work-item-meta { display: flex; align-items: center; gap: 6px; flex-shrink: 0; color: #A1A1AA; font-size: 11px; }
.work-item-meta span { background: #27272A; border-radius: 4px; padding: 2px 6px; }

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
  background: #1B1C20;
  border: 1px solid #2D2F36;
  border-radius: 12px;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.8);
}

.cm-header { padding: 24px 24px 16px; }
.cm-badge { display: inline-flex; align-items: center; gap: 8px; border: 1px solid #3F3F46; padding: 4px 10px; border-radius: 6px; font-size: 12px; color: #E4E4E7; font-weight: 500; margin-bottom: 16px; }
.cm-title { font-size: 20px; font-weight: 600; color: #E4E4E7; margin: 0; }
.cm-body { padding: 0 24px 24px; display: flex; flex-direction: column; }
.cm-input, .cm-textarea { width: 100%; background: #141518; border: 1px solid #2D2F36; border-radius: 6px; padding: 12px 16px; color: #E4E4E7; outline: none; }
.cm-input { margin-bottom: 16px; font-size: 15px; }
.cm-textarea { font-size: 14px; resize: none; }
.cm-footer { padding: 16px 24px; border-top: 1px solid #2D2F36; display: flex; justify-content: flex-end; gap: 12px; background: #141518; border-bottom-left-radius: 12px; border-bottom-right-radius: 12px; }
.cm-btn-cancel { background: transparent; border: 1px solid #3F3F46; border-radius: 6px; padding: 8px 16px; color: #E4E4E7; font-size: 13px; font-weight: 500; cursor: pointer; }
.cm-btn-create { background: #38BDF8; border: none; border-radius: 6px; padding: 8px 16px; color: white; font-size: 13px; font-weight: 500; cursor: pointer; }

.dp-wrapper { position: relative; }
.dp-btn { background: transparent; border: 1px solid #2D2F36; color: #E4E4E7; padding: 6px 12px; border-radius: 6px; font-size: 13px; cursor: pointer; display: flex; align-items: center; gap: 8px; }
.dp-popover { position: absolute; top: 100%; left: 0; margin-top: 8px; background: #141518; border: 1px solid #2D2F36; border-radius: 8px; width: 280px; padding: 16px; box-shadow: 0 4px 20px rgba(0,0,0,0.5); z-index: 1001; }
.dp-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px; }
.dp-month-year { display: flex; gap: 12px; font-size: 14px; font-weight: 600; }
.dp-nav { display: flex; gap: 8px; }
.dp-nav button { background: transparent; border: none; color: #71717A; cursor: pointer; }
.dp-grid { display: grid; grid-template-columns: repeat(7, 1fr); row-gap: 6px; }
.headday { font-size: 10px; font-weight: 600; color: #A1A1AA; text-align: center; }
.dp-day-wrapper { position: relative; display: flex; align-items: center; justify-content: center; height: 32px; }
.dp-bg-range { position: absolute; inset: 0; background: #1D435E; z-index: 1; }
.range-start { border-top-left-radius: 16px; border-bottom-left-radius: 16px; }
.range-end { border-top-right-radius: 16px; border-bottom-right-radius: 16px; }
.dp-day-num { position: relative; z-index: 2; width: 32px; height: 32px; display: flex; align-items: center; justify-content: center; border-radius: 50%; font-size: 12px; color: #52525B; cursor: pointer; }
.dp-day-num.current-month { color: #A1A1AA; }
.dp-day-num:hover:not(.headday):not(.disabled) { background: #27272A; color: white; }
.dp-day-num.selected { background: #0EA5E9; color: white; }
.dp-day-num.disabled { color: #3F3F46; cursor: not-allowed; opacity: 0.55; }
</style>
