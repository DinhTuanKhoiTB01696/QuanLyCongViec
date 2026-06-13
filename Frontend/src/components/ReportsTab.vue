<template>
  <!--
    Reports tab — dựng theo ảnh Report.jpeg.
    Donut by status / type / assignee tính từ task thật.
    [CẦN XÁC NHẬN] Công thức cycle time / lead time / completion trend không có trong ảnh
    => dùng dữ liệu mock placeholder, không bịa logic tính toán.
  -->
  <div class="reports-tab">
    <div class="reports-head">
      <button class="more-reports-btn"><i class="fa-solid fa-chart-line"></i> More reports</button>
    </div>

    <!-- Summary cards -->
    <div class="summary-cards">
      <div class="sum-card">
        <span class="sum-icon green"><i class="fa-solid fa-circle-check"></i></span>
        <div class="sum-text"><span class="sum-val">{{ summary.completed }} work items</span><span class="sum-sub">completed in the last 7 days</span></div>
      </div>
      <div class="sum-card">
        <span class="sum-icon blue"><i class="fa-solid fa-pen"></i></span>
        <div class="sum-text"><span class="sum-val">{{ summary.updated }} work items</span><span class="sum-sub">updated in the last 7 days</span></div>
      </div>
      <div class="sum-card">
        <span class="sum-icon orange"><i class="fa-solid fa-plus"></i></span>
        <div class="sum-text"><span class="sum-val">{{ summary.created }} work items</span><span class="sum-sub">created in the last 7 days</span></div>
      </div>
      <div class="sum-card">
        <span class="sum-icon red"><i class="fa-regular fa-calendar"></i></span>
        <div class="sum-text"><span class="sum-val">{{ summary.due }} work item</span><span class="sum-sub">due in the next 7 days</span></div>
      </div>
    </div>

    <!-- Donut row -->
    <div class="chart-row three">
      <div class="chart-card">
        <h4>Work items by status</h4>
        <v-chart class="donut" :option="donut(statusData, totalTasks)" autoresize />
      </div>
      <div class="chart-card">
        <h4>Work items by type</h4>
        <v-chart class="donut" :option="donut(typeData, totalTasks)" autoresize />
      </div>
      <div class="chart-card">
        <h4>Work items by assignees</h4>
        <v-chart class="donut" :option="donut(assigneeData, totalTasks)" autoresize />
      </div>
    </div>

    <!-- trend / cycle -->
    <div class="chart-row two">
      <div class="chart-card">
        <h4>Work item creation trend</h4>
        <v-chart class="bar" :option="creationTrendOption" autoresize />
      </div>
      <div class="chart-card">
        <h4>Work item cycle time</h4>
        <v-chart class="line" :option="emptyLineOption('Average cycle time')" autoresize />
        <span class="mock-note">[CẦN XÁC NHẬN] công thức cycle time chưa có trong ảnh</span>
      </div>
    </div>

    <div class="chart-row two">
      <div class="chart-card">
        <h4>Work item lead time</h4>
        <v-chart class="line" :option="leadTimeOption" autoresize />
        <span class="mock-note">[CẦN XÁC NHẬN] công thức lead time chưa có trong ảnh</span>
      </div>
      <div class="chart-card">
        <h4>Work item completion trend</h4>
        <v-chart class="line" :option="emptyLineOption('Completed')" autoresize />
        <span class="mock-note">[CẦN XÁC NHẬN] công thức completion trend chưa có trong ảnh</span>
      </div>
    </div>

    <!-- Details table -->
    <div class="details-card">
      <h4>Work item details</h4>
      <div class="details-search">
        <i class="fa-solid fa-magnifying-glass"></i>
        <input v-model="tableSearch" type="text" placeholder="Search table" />
      </div>
      <table class="details-table">
        <thead>
          <tr>
            <th>Work item key</th>
            <th>Work item type</th>
            <th>Work item priority</th>
            <th>Summary</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="row in pagedRows" :key="row.id" @click="$emit('open-task', row.task)">
            <td class="key-cell">{{ row.key }}</td>
            <td><i :class="row.typeIcon" :style="{ color: row.typeColor }"></i> {{ row.type }}</td>
            <td><i :class="row.priorityIcon"></i> {{ row.priority }}</td>
            <td>{{ row.summary }}</td>
          </tr>
          <tr v-if="!pagedRows.length"><td colspan="4" class="empty-cell">No work items</td></tr>
        </tbody>
      </table>
      <div class="table-footer">
        <div class="pager">
          <button :disabled="page === 1" @click="page--"><i class="fa-solid fa-chevron-left"></i></button>
          <span class="page-num">{{ page }}</span>
          <button :disabled="page >= totalPages" @click="page++"><i class="fa-solid fa-chevron-right"></i></button>
        </div>
        <span class="showing">Showing items {{ rangeStart }}-{{ rangeEnd }} of {{ filteredRows.length }}</span>
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

use([CanvasRenderer, PieChart, BarChart, LineChart, TooltipComponent, LegendComponent, GridComponent])

const props = defineProps({
  tasks: { type: Array, default: () => [] },
  projectMembers: { type: Array, default: () => [] },
  statusOptions: { type: Array, default: () => [] }
})
defineEmits(['open-task'])

const tableSearch = ref('')
const page = ref(1)
const PAGE_SIZE = 10

const totalTasks = computed(() => props.tasks.length)
const normalizeStatus = (v) => `${v || 'BACKLOG'}`.toUpperCase().replace(/\s+/g, ' ').trim()

const isSubtask = (t) => Boolean(t.parentTaskId || t.parentId || t.parentTaskTitle || t.parentTitle)

const daysAgo = (n) => { const d = new Date(); d.setDate(d.getDate() - n); d.setHours(0,0,0,0); return d }
const daysAhead = (n) => { const d = new Date(); d.setDate(d.getDate() + n); d.setHours(23,59,59,999); return d }
const toDate = (v) => { if (!v) return null; const d = new Date(v); return Number.isNaN(d.getTime()) ? null : d }

const summary = computed(() => {
  const last7 = daysAgo(7); const next7 = daysAhead(7); const now = new Date()
  let completed = 0, updated = 0, created = 0, due = 0
  props.tasks.forEach(t => {
    const c = toDate(t.createdAt); const u = toDate(t.updatedAt || t.createdAt); const dd = toDate(t.dueDate)
    if (normalizeStatus(t.statusName) === 'DONE' && u && u >= last7) completed++
    if (u && u >= last7) updated++
    if (c && c >= last7) created++
    if (dd && dd >= now && dd <= next7) due++
  })
  return { completed, updated, created, due }
})

const STATUS_COLORS = { 'TO DO': '#8993a4', 'BACKLOG': '#8993a4', 'IN PROGRESS': '#0c66e4', 'IN REVIEW': '#e56910', 'DONE': '#22a06b', 'CANCELLED': '#e2483d' }
const statusData = computed(() => {
  const map = new Map()
  props.tasks.forEach(t => { const s = normalizeStatus(t.statusName); map.set(s, (map.get(s) || 0) + 1) })
  const labelOf = (s) => props.statusOptions.find(o => o.name === s)?.label || s
  return Array.from(map.entries()).map(([s, count]) => ({ name: labelOf(s), value: count, color: STATUS_COLORS[s] || '#8993a4' }))
})

const typeData = computed(() => {
  let task = 0, sub = 0
  props.tasks.forEach(t => { isSubtask(t) ? sub++ : task++ })
  return [
    { name: 'Task', value: task, color: '#0c66e4' },
    { name: 'Subtask', value: sub, color: '#6e5dc6' }
  ].filter(d => d.value > 0)
})

const AVATAR_COLORS = ['#22a06b', '#0c66e4', '#e2483d', '#e56910', '#6e5dc6']
const getAssigneeIds = (t) => Array.from(new Set([
  ...(Array.isArray(t.assigneeIds) ? t.assigneeIds : []),
  ...(Array.isArray(t.assignees) ? t.assignees.map(a => a.userId || a.id).filter(Boolean) : []),
  ...(t.assignedUserId ? [t.assignedUserId] : [])
]))
const assigneeData = computed(() => {
  const map = new Map()
  props.tasks.forEach(t => {
    const ids = getAssigneeIds(t)
    if (!ids.length) { map.set('__none', (map.get('__none') || 0) + 1); return }
    ids.forEach(id => map.set(id, (map.get(id) || 0) + 1))
  })
  return Array.from(map.entries()).map(([id, value], i) => {
    if (id === '__none') return { name: 'Unassigned', value, color: '#8993a4' }
    const m = props.projectMembers.find(x => (x.userId || x.id) === id)
    return { name: m?.fullName || m?.name || m?.email || 'Assignee', value, color: AVATAR_COLORS[i % AVATAR_COLORS.length] }
  })
})

const donut = (data, total) => ({
  tooltip: { trigger: 'item' },
  legend: { bottom: 0, left: 'center', textStyle: { color: 'var(--color-text-muted)', fontSize: 11 }, icon: 'circle' },
  series: [{
    type: 'pie', radius: ['55%', '75%'], center: ['50%', '42%'], avoidLabelOverlap: false,
    label: { show: true, position: 'center', formatter: `${total}\nTotal value`, color: 'var(--color-text-primary)', fontSize: 13, lineHeight: 18 },
    labelLine: { show: false },
    data: data.map(d => ({ name: d.name, value: d.value, itemStyle: { color: d.color } }))
  }],
  backgroundColor: 'transparent'
})

// Creation trend: gom theo ngày tạo (dữ liệu thật), tách To Do / In Progress
const creationTrendOption = computed(() => {
  const byDay = new Map()
  props.tasks.forEach(t => {
    const c = toDate(t.createdAt); if (!c) return
    const key = c.toISOString().slice(0, 10)
    if (!byDay.has(key)) byDay.set(key, { todo: 0, progress: 0 })
    const bucket = byDay.get(key)
    normalizeStatus(t.statusName) === 'IN PROGRESS' ? bucket.progress++ : bucket.todo++
  })
  const days = Array.from(byDay.keys()).sort()
  return {
    tooltip: { trigger: 'axis' },
    legend: { top: 0, left: 0, data: ['To Do', 'In Progress'], textStyle: { color: 'var(--color-text-muted)', fontSize: 11 }, icon: 'rect' },
    grid: { left: '3%', right: '4%', bottom: '8%', top: 32, containLabel: true },
    xAxis: { type: 'category', data: days, axisLabel: { color: 'var(--color-text-muted)' } },
    yAxis: { type: 'value', minInterval: 1, splitLine: { lineStyle: { color: 'var(--color-border)' } }, axisLabel: { color: 'var(--color-text-muted)' } },
    series: [
      { name: 'To Do', type: 'bar', stack: 'c', data: days.map(d => byDay.get(d).todo), itemStyle: { color: '#0c66e4' } },
      { name: 'In Progress', type: 'bar', stack: 'c', data: days.map(d => byDay.get(d).progress), itemStyle: { color: '#22a06b' } }
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

// Lead time: placeholder mock flat line
const leadTimeOption = computed(() => ({
  tooltip: { trigger: 'axis' },
  legend: { top: 0, left: 0, data: ['Task', 'Subtask'], textStyle: { color: 'var(--color-text-muted)', fontSize: 11 }, icon: 'circle' },
  grid: { left: '3%', right: '4%', bottom: '8%', top: 32, containLabel: true },
  xAxis: { type: 'category', boundaryGap: false, data: ['2026-06-01', '2026-06-08'], axisLabel: { color: 'var(--color-text-muted)' } },
  yAxis: { type: 'value', splitLine: { lineStyle: { color: 'var(--color-border)' } }, axisLabel: { color: 'var(--color-text-muted)' } },
  series: [{ name: 'Task', type: 'line', data: [0, 0], itemStyle: { color: '#0c66e4' } }],
  backgroundColor: 'transparent'
}))

// Details table
const priorityMeta = (p) => {
  if (p === 1) return { icon: 'fa-solid fa-angles-up', label: 'Highest' }
  if (p === 2) return { icon: 'fa-solid fa-chevron-up', label: 'High' }
  if (p === 3) return { icon: 'fa-solid fa-equals', label: 'Medium' }
  if (p === 4) return { icon: 'fa-solid fa-chevron-down', label: 'Low' }
  return { icon: 'fa-solid fa-minus', label: 'Medium' }
}

const allRows = computed(() => props.tasks.map(t => {
  const sub = isSubtask(t)
  const pm = priorityMeta(t.priority)
  return {
    id: t.id, task: t,
    key: t.sequenceId || t.id.substring(0, 8).toUpperCase(),
    type: sub ? 'Subtask' : 'Task',
    typeIcon: sub ? 'fa-solid fa-diagram-project' : 'fa-solid fa-square-check',
    typeColor: sub ? '#6e5dc6' : '#0c66e4',
    priority: pm.label, priorityIcon: pm.icon,
    summary: t.title || ''
  }
}))

const filteredRows = computed(() => {
  const q = tableSearch.value.trim().toLowerCase()
  if (!q) return allRows.value
  return allRows.value.filter(r => r.key.toLowerCase().includes(q) || r.summary.toLowerCase().includes(q))
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
.mock-note { position: absolute; bottom: 8px; right: 12px; font-size: 10px; color: var(--color-text-muted); }

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
