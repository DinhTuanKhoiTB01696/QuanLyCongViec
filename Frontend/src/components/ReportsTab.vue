<template>
  <div class="reports-tab">
    <div class="reports-head">
      <button class="more-reports-btn" type="button" disabled :title="t('workItems.reports.moreReportsNeedsFlow')">
        <i class="fa-solid fa-chart-line"></i> {{ t('workItems.reports.moreReports') }}
      </button>
    </div>

    <div class="summary-cards">
      <div class="sum-card">
        <span class="sum-icon green"><i class="fa-solid fa-circle-check"></i></span>
        <div class="sum-text">
          <span class="sum-val">{{ t('workItems.reports.workItemsCount', { count: summary.completed }) }}</span>
          <span class="sum-sub">{{ t('workItems.reports.completedLast7Days') }}</span>
        </div>
      </div>
      <div class="sum-card">
        <span class="sum-icon blue"><i class="fa-solid fa-pen"></i></span>
        <div class="sum-text">
          <span class="sum-val">{{ t('workItems.reports.workItemsCount', { count: summary.updated }) }}</span>
          <span class="sum-sub">{{ t('workItems.reports.updatedLast7Days') }}</span>
        </div>
      </div>
      <div class="sum-card">
        <span class="sum-icon orange"><i class="fa-solid fa-plus"></i></span>
        <div class="sum-text">
          <span class="sum-val">{{ t('workItems.reports.workItemsCount', { count: summary.created }) }}</span>
          <span class="sum-sub">{{ t('workItems.reports.createdLast7Days') }}</span>
        </div>
      </div>
      <div class="sum-card">
        <span class="sum-icon red"><i class="fa-regular fa-calendar"></i></span>
        <div class="sum-text">
          <span class="sum-val">{{ t('workItems.reports.workItemsCount', { count: summary.due }) }}</span>
          <span class="sum-sub">{{ t('workItems.reports.dueNext7Days') }}</span>
        </div>
      </div>
    </div>

    <div class="chart-row three">
      <div class="chart-card">
        <h4>{{ t('workItems.reports.workItemsByStatus') }}</h4>
        <v-chart class="donut" :option="donut(statusData, totalTasks)" autoresize />
      </div>
      <div class="chart-card">
        <h4>{{ t('workItems.reports.workItemsByType') }}</h4>
        <v-chart class="donut" :option="donut(typeData, totalTasks)" autoresize />
      </div>
      <div class="chart-card">
        <h4>{{ t('workItems.reports.workItemsByAssignees') }}</h4>
        <v-chart class="donut" :option="donut(assigneeData, totalTasks)" autoresize />
      </div>
    </div>

    <div class="chart-row two">
      <div class="chart-card">
        <h4>{{ t('workItems.reports.workItemCreationTrend') }}</h4>
        <v-chart class="bar" :option="creationTrendOption" autoresize />
      </div>
      <div class="chart-card">
        <h4>{{ t('workItems.reports.workItemCycleTime') }}</h4>
        <v-chart class="line" :option="emptyLineOption(t('workItems.reports.averageCycleTime'))" autoresize />
        <span class="data-note">{{ t('workItems.reports.needCycleTimeData') }}</span>
      </div>
    </div>

    <div class="chart-row two">
      <div class="chart-card">
        <h4>{{ t('workItems.reports.workItemLeadTime') }}</h4>
        <v-chart class="line" :option="emptyLineOption(t('workItems.reports.leadTime'))" autoresize />
        <span class="data-note">{{ t('workItems.reports.needLeadTimeData') }}</span>
      </div>
      <div class="chart-card">
        <h4>{{ t('workItems.reports.workItemCompletionTrend') }}</h4>
        <v-chart class="line" :option="emptyLineOption(t('workItems.completed'))" autoresize />
        <span class="data-note">{{ t('workItems.reports.needCompletionTrendData') }}</span>
      </div>
    </div>

    <div class="details-card">
      <h4>{{ t('workItems.reports.workItemDetails') }}</h4>
      <div class="details-search">
        <i class="fa-solid fa-magnifying-glass"></i>
        <input v-model="tableSearch" type="text" :placeholder="t('workItems.reports.searchTable')" />
      </div>
      <table class="details-table">
        <thead>
          <tr>
            <th>{{ t('workItems.reports.workItemKey') }}</th>
            <th>{{ t('workItems.reports.workItemType') }}</th>
            <th>{{ t('workItems.reports.workItemPriority') }}</th>
            <th>{{ t('workItems.reports.summary') }}</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="row in pagedRows" :key="row.id" @click="$emit('open-task', row.task)">
            <td class="key-cell">{{ row.key }}</td>
            <td><i :class="row.typeIcon" :style="{ color: row.typeColor }"></i> {{ row.type }}</td>
            <td><i :class="row.priorityIcon"></i> {{ row.priority }}</td>
            <td>{{ row.summary }}</td>
          </tr>
          <tr v-if="!pagedRows.length">
            <td colspan="4" class="empty-cell">{{ t('workItems.reports.noWorkItems') }}</td>
          </tr>
        </tbody>
      </table>
      <div class="table-footer">
        <div class="pager">
          <button :disabled="page === 1" @click="page--"><i class="fa-solid fa-chevron-left"></i></button>
          <span class="page-num">{{ page }}</span>
          <button :disabled="page >= totalPages" @click="page++"><i class="fa-solid fa-chevron-right"></i></button>
        </div>
        <span class="showing">{{ t('workItems.reports.showingItems', { start: rangeStart, end: rangeEnd, total: filteredRows.length }) }}</span>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { use } from 'echarts/core'
import { CanvasRenderer } from 'echarts/renderers'
import { PieChart, BarChart, LineChart } from 'echarts/charts'
import { TooltipComponent, LegendComponent, GridComponent } from 'echarts/components'
import VChart from 'vue-echarts'
import { useI18n } from '@/composables/useI18n'

use([CanvasRenderer, PieChart, BarChart, LineChart, TooltipComponent, LegendComponent, GridComponent])

const props = defineProps({
  tasks: { type: Array, default: () => [] },
  projectMembers: { type: Array, default: () => [] },
  statusOptions: { type: Array, default: () => [] }
})
defineEmits(['open-task'])

const { t } = useI18n()
const tableSearch = ref('')
const page = ref(1)
const PAGE_SIZE = 10

const totalTasks = computed(() => props.tasks.length)
const normalizeStatus = (v) => `${v || 'BACKLOG'}`.toUpperCase().replace(/\s+/g, ' ').trim()

const isSubtask = (task) => Boolean(task.parentTaskId || task.parentId || task.parentTaskTitle || task.parentTitle)

const daysAgo = (n) => {
  const date = new Date()
  date.setDate(date.getDate() - n)
  date.setHours(0, 0, 0, 0)
  return date
}
const daysAhead = (n) => {
  const date = new Date()
  date.setDate(date.getDate() + n)
  date.setHours(23, 59, 59, 999)
  return date
}
const toDate = (value) => {
  if (!value) return null
  const date = new Date(value)
  return Number.isNaN(date.getTime()) ? null : date
}

const summary = computed(() => {
  const last7 = daysAgo(7)
  const next7 = daysAhead(7)
  const now = new Date()
  let completed = 0
  let updated = 0
  let created = 0
  let due = 0

  props.tasks.forEach((task) => {
    const createdAt = toDate(task.createdAt)
    const updatedAt = toDate(task.updatedAt || task.createdAt)
    const dueDate = toDate(task.dueDate)
    if (normalizeStatus(task.statusName) === 'DONE' && updatedAt && updatedAt >= last7) completed++
    if (updatedAt && updatedAt >= last7) updated++
    if (createdAt && createdAt >= last7) created++
    if (dueDate && dueDate >= now && dueDate <= next7) due++
  })

  return { completed, updated, created, due }
})

const STATUS_COLORS = {
  'TO DO': '#8993a4',
  BACKLOG: '#8993a4',
  'IN PROGRESS': '#0c66e4',
  'IN REVIEW': '#e56910',
  DONE: '#22a06b',
  CANCELLED: '#e2483d'
}

const statusData = computed(() => {
  const map = new Map()
  props.tasks.forEach((task) => {
    const status = normalizeStatus(task.statusName)
    map.set(status, (map.get(status) || 0) + 1)
  })
  const labelOf = (status) => props.statusOptions.find((option) => option.name === status)?.label || status
  return Array.from(map.entries()).map(([status, count]) => ({
    name: labelOf(status),
    value: count,
    color: STATUS_COLORS[status] || '#8993a4'
  }))
})

const typeData = computed(() => {
  let taskCount = 0
  let subtaskCount = 0
  props.tasks.forEach((task) => {
    isSubtask(task) ? subtaskCount++ : taskCount++
  })
  return [
    { name: t('workItems.reports.task'), value: taskCount, color: '#0c66e4' },
    { name: t('workItems.reports.subtask'), value: subtaskCount, color: '#6e5dc6' }
  ].filter((item) => item.value > 0)
})

const AVATAR_COLORS = ['#22a06b', '#0c66e4', '#e2483d', '#e56910', '#6e5dc6']
const getAssigneeIds = (task) => Array.from(new Set([
  ...(Array.isArray(task.assigneeIds) ? task.assigneeIds : []),
  ...(Array.isArray(task.assignees) ? task.assignees.map((assignee) => assignee.userId || assignee.id).filter(Boolean) : []),
  ...(task.assignedUserId ? [task.assignedUserId] : [])
]))

const assigneeData = computed(() => {
  const map = new Map()
  props.tasks.forEach((task) => {
    const ids = getAssigneeIds(task)
    if (!ids.length) {
      map.set('__none', (map.get('__none') || 0) + 1)
      return
    }
    ids.forEach((id) => map.set(id, (map.get(id) || 0) + 1))
  })

  return Array.from(map.entries()).map(([id, value], index) => {
    if (id === '__none') {
      return { name: t('workItems.reports.unassigned'), value, color: '#8993a4' }
    }
    const member = props.projectMembers.find((item) => (item.userId || item.id) === id)
    return {
      name: member?.fullName || member?.name || member?.email || t('workItems.assignee'),
      value,
      color: AVATAR_COLORS[index % AVATAR_COLORS.length]
    }
  })
})

const donut = (data, total) => ({
  tooltip: { trigger: 'item' },
  legend: { bottom: 0, left: 'center', textStyle: { color: 'var(--color-text-muted)', fontSize: 11 }, icon: 'circle' },
  series: [{
    type: 'pie',
    radius: ['55%', '75%'],
    center: ['50%', '42%'],
    avoidLabelOverlap: false,
    label: {
      show: true,
      position: 'center',
      formatter: `${total}\n${t('workItems.reports.totalValue')}`,
      color: 'var(--color-text-primary)',
      fontSize: 13,
      lineHeight: 18
    },
    labelLine: { show: false },
    data: data.map((item) => ({ name: item.name, value: item.value, itemStyle: { color: item.color } }))
  }],
  backgroundColor: 'transparent'
})

const creationTrendOption = computed(() => {
  const byDay = new Map()
  props.tasks.forEach((task) => {
    const createdAt = toDate(task.createdAt)
    if (!createdAt) return
    const key = createdAt.toISOString().slice(0, 10)
    if (!byDay.has(key)) byDay.set(key, { todo: 0, progress: 0 })
    const bucket = byDay.get(key)
    normalizeStatus(task.statusName) === 'IN PROGRESS' ? bucket.progress++ : bucket.todo++
  })

  const days = Array.from(byDay.keys()).sort()
  const todoLabel = t('workItems.reports.toDo')
  const inProgressLabel = t('workItems.reports.inProgress')

  return {
    tooltip: { trigger: 'axis' },
    legend: { top: 0, left: 0, data: [todoLabel, inProgressLabel], textStyle: { color: 'var(--color-text-muted)', fontSize: 11 }, icon: 'rect' },
    grid: { left: '3%', right: '4%', bottom: '8%', top: 32, containLabel: true },
    xAxis: { type: 'category', data: days, axisLabel: { color: 'var(--color-text-muted)' } },
    yAxis: { type: 'value', minInterval: 1, splitLine: { lineStyle: { color: 'var(--color-border)' } }, axisLabel: { color: 'var(--color-text-muted)' } },
    series: [
      { name: todoLabel, type: 'bar', stack: 'c', data: days.map((day) => byDay.get(day).todo), itemStyle: { color: '#0c66e4' } },
      { name: inProgressLabel, type: 'bar', stack: 'c', data: days.map((day) => byDay.get(day).progress), itemStyle: { color: '#22a06b' } }
    ],
    backgroundColor: 'transparent'
  }
})

const emptyLineOption = (name) => ({
  tooltip: { trigger: 'axis' },
  grid: { left: '3%', right: '4%', bottom: '8%', top: 20, containLabel: true },
  xAxis: { type: 'category', data: [], axisLabel: { color: 'var(--color-text-muted)' } },
  yAxis: { type: 'value', splitLine: { lineStyle: { color: 'var(--color-border)' } }, axisLabel: { color: 'var(--color-text-muted)' } },
  series: [{ name, type: 'line', data: [] }],
  backgroundColor: 'transparent'
})

const priorityMeta = (priority) => {
  if (priority === 1) return { icon: 'fa-solid fa-angles-up', label: t('workItems.priority.urgent') }
  if (priority === 2) return { icon: 'fa-solid fa-chevron-up', label: t('workItems.priority.high') }
  if (priority === 3) return { icon: 'fa-solid fa-equals', label: t('workItems.priority.medium') }
  if (priority === 4) return { icon: 'fa-solid fa-chevron-down', label: t('workItems.priority.low') }
  return { icon: 'fa-solid fa-minus', label: t('workItems.priority.medium') }
}

const allRows = computed(() => props.tasks.map((task) => {
  const subtask = isSubtask(task)
  const priority = priorityMeta(task.priority)
  return {
    id: task.id,
    task,
    key: task.sequenceId || task.id.substring(0, 8).toUpperCase(),
    type: subtask ? t('workItems.reports.subtask') : t('workItems.reports.task'),
    typeIcon: subtask ? 'fa-solid fa-diagram-project' : 'fa-solid fa-square-check',
    typeColor: subtask ? '#6e5dc6' : '#0c66e4',
    priority: priority.label,
    priorityIcon: priority.icon,
    summary: task.title || ''
  }
}))

const filteredRows = computed(() => {
  const query = tableSearch.value.trim().toLowerCase()
  if (!query) return allRows.value
  return allRows.value.filter((row) => row.key.toLowerCase().includes(query) || row.summary.toLowerCase().includes(query))
})
const totalPages = computed(() => Math.max(1, Math.ceil(filteredRows.value.length / PAGE_SIZE)))
const pagedRows = computed(() => filteredRows.value.slice((page.value - 1) * PAGE_SIZE, page.value * PAGE_SIZE))
const rangeStart = computed(() => filteredRows.value.length ? (page.value - 1) * PAGE_SIZE + 1 : 0)
const rangeEnd = computed(() => Math.min(page.value * PAGE_SIZE, filteredRows.value.length))
</script>

<style scoped>
.reports-tab { flex: 1; min-height: 0; overflow: auto; padding: 16px 24px 40px; background: var(--color-bg); }
.reports-head { margin-bottom: 16px; }
.more-reports-btn {
  background: var(--color-surface); border: 1px solid var(--color-border); color: var(--color-text-primary);
  padding: 6px 12px; border-radius: 4px; font-size: 13px; cursor: pointer; display: inline-flex; align-items: center; gap: 8px;
}
.more-reports-btn:hover { background: var(--color-border); }
.more-reports-btn:disabled { opacity: .55; cursor: not-allowed; }

.summary-cards { display: grid; grid-template-columns: repeat(4, 1fr); gap: 16px; margin-bottom: 16px; }
.sum-card { display: flex; align-items: center; gap: 12px; border: 1px solid var(--color-border); border-radius: 6px; padding: 14px 16px; background: var(--color-surface); }
.sum-icon { width: 32px; height: 32px; border-radius: 6px; display: flex; align-items: center; justify-content: center; color: #fff; font-size: 14px; flex-shrink: 0; }
.sum-icon.green { background: #22a06b; } .sum-icon.blue { background: #0c66e4; } .sum-icon.orange { background: #e56910; } .sum-icon.red { background: #e2483d; }
.sum-text { display: flex; flex-direction: column; }
.sum-val { font-weight: 700; color: var(--color-text-primary); font-size: 14px; }
.sum-sub { color: var(--color-text-muted); font-size: 12px; }

.chart-row { display: grid; gap: 16px; margin-bottom: 16px; }
.chart-row.three { grid-template-columns: repeat(3, 1fr); }
.chart-row.two { grid-template-columns: repeat(2, 1fr); }
.chart-card { border: 1px solid var(--color-border); border-radius: 6px; padding: 16px; background: var(--color-surface); position: relative; }
.chart-card h4 { margin: 0 0 8px; font-size: 14px; color: var(--color-text-primary); }
.donut { height: 240px; }
.bar, .line { height: 240px; }
.data-note { position: absolute; bottom: 8px; right: 12px; font-size: 10px; color: var(--color-text-muted); }

.details-card { border: 1px solid var(--color-border); border-radius: 6px; padding: 16px; background: var(--color-surface); }
.details-card h4 { margin: 0 0 12px; font-size: 14px; color: var(--color-text-primary); }
.details-search { display: flex; align-items: center; gap: 8px; border: 1px solid var(--color-border); border-radius: 4px; padding: 6px 10px; max-width: 280px; margin-bottom: 12px; color: var(--color-text-muted); }
.details-search input { background: transparent; border: none; outline: none; color: var(--color-text-primary); font-size: 13px; width: 100%; }
.details-table { width: 100%; border-collapse: collapse; font-size: 13px; }
.details-table th { text-align: left; padding: 8px 12px; color: var(--color-text-muted); font-weight: 600; border-bottom: 1px solid var(--color-border); }
.details-table td { padding: 10px 12px; color: var(--color-text-primary); border-bottom: 1px solid var(--color-border); cursor: pointer; }
.details-table tbody tr:hover { background: var(--color-bg); }
.key-cell { color: #0c66e4; }
.empty-cell { text-align: center; color: var(--color-text-muted); cursor: default; }
.table-footer { display: flex; align-items: center; justify-content: space-between; margin-top: 12px; }
.pager { display: flex; align-items: center; gap: 8px; }
.pager button { background: var(--color-surface); border: 1px solid var(--color-border); color: var(--color-text-secondary); width: 28px; height: 28px; border-radius: 4px; cursor: pointer; }
.pager button:disabled { opacity: .4; cursor: not-allowed; }
.page-num { font-size: 13px; color: var(--color-text-primary); }
.showing { font-size: 12px; color: var(--color-text-muted); }
</style>
