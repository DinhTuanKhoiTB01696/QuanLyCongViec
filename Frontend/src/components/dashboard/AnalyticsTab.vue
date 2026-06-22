<script setup>
import { computed, onMounted, ref, watch } from 'vue'
import { ElMessage } from 'element-plus'
import { Bar, Line, Radar } from 'vue-chartjs'
import {
  ArcElement,
  BarElement,
  CategoryScale,
  Chart as ChartJS,
  Filler,
  Legend,
  LinearScale,
  LineElement,
  PointElement,
  RadialLinearScale,
  Title,
  Tooltip
} from 'chart.js'
import axiosClient from '@/api/axiosClient'

ChartJS.register(
  Title,
  Tooltip,
  Legend,
  BarElement,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  RadialLinearScale,
  Filler,
  ArcElement
)

const props = defineProps({
  projectId: { type: String, required: true }
})

const activeTab = ref('Overview')
const insightDimension = ref('Accuracy')
const workItemMetric = ref('Work item')
const loadingPlanning = ref(false)

const totalTasks = ref(0)
const myTasks = ref(0)
const overdueTasks = ref(0)
const statusStats = ref([])
const priorityStats = ref([])

const planningOverview = ref({
  totalProjects: 0,
  totalTasks: 0,
  totalCommittedStoryPoints: 0,
  completedStoryPoints: 0,
  carryOverStoryPoints: 0,
  totalEstimatedHours: 0,
  totalActualHours: 0,
  totalLoggedHours: 0
})

const velocitySummary = ref({
  committedStoryPoints: 0,
  completedStoryPoints: 0,
  carryOverStoryPoints: 0,
  completionRate: 0,
  byProject: [],
  bySprint: []
})

const estimateAccuracy = ref({
  averageAccuracyPercent: 100,
  accurateCount: 0,
  underEstimatedCount: 0,
  overEstimatedCount: 0,
  unplannedCount: 0,
  rows: [],
  byUser: []
})

const workloadSummary = ref({
  rows: [],
  overCapacityCount: 0,
  nearLimitCount: 0
})

const managerReview = ref({
  canConfirmBaseline: false,
  selectedProjectId: null,
  projects: [],
  riskSummary: {
    overCapacityMembers: 0,
    nearLimitMembers: 0,
    carryOverProjects: 0,
    unplannedTasks: 0
  }
})

const fetchStats = async () => {
  try {
    const res = await axiosClient.get('/dashboard/stats', { params: { projectId: props.projectId } })
    const data = res.data?.data
    if (!data) return

    totalTasks.value = data.totalTasks || 0
    myTasks.value = data.myTasks || 0
    overdueTasks.value = data.overdueTasks || 0
    statusStats.value = data.byStatus || []
    priorityStats.value = data.byPriority || []
  } catch (error) {
    console.warn('Unable to load dashboard stats.', error)
  }
}

const fetchPlanningSummary = async () => {
  loadingPlanning.value = true
  try {
    const res = await axiosClient.get('/analytics/planning-summary', {
      params: { projectId: props.projectId }
    })

    const data = res.data?.data || {}
    planningOverview.value = data.overview || planningOverview.value
    velocitySummary.value = data.velocity || velocitySummary.value
    estimateAccuracy.value = data.estimateAccuracy || estimateAccuracy.value
    workloadSummary.value = data.workload || workloadSummary.value
    managerReview.value = data.managerReview || managerReview.value
  } catch (error) {
    ElMessage.error(error.response?.data?.message || 'Unable to load planning analytics.')
  } finally {
    loadingPlanning.value = false
  }
}

const confirmPlanningBaseline = async () => {
  if (!props.projectId) return

  try {
    await axiosClient.post(`/analytics/projects/${props.projectId}/confirm-baseline`)
    ElMessage.success('Planning baseline confirmed.')
    fetchPlanningSummary()
  } catch (error) {
    ElMessage.error(error.response?.data?.message || 'Unable to confirm planning baseline.')
  }
}

const selectWorkItemMetric = (metric) => {
  workItemMetric.value = metric
  activeTab.value = 'Work items'
}

const selectInsightDimension = (dimension) => {
  insightDimension.value = dimension
}

onMounted(() => {
  fetchStats()
  fetchPlanningSummary()
})

watch(() => props.projectId, () => {
  fetchStats()
  fetchPlanningSummary()
})

const getPriorityLabel = (val) => {
  const map = { 0: 'None', 1: 'Low', 2: 'Normal', 3: 'High', 4: 'Urgent' }
  return map[val] || 'None'
}

const getPriorityColor = (val) => {
  const map = { 0: 'var(--color-text-muted)', 1: '#10B981', 2: '#3B82F6', 3: '#F97316', 4: '#EF4444' }
  return map[val] || 'var(--color-text-muted)'
}

const radarChartData = computed(() => ({
  labels: ['Committed SP', 'Completed SP', 'Carry-over', 'Accuracy', 'Estimated h', 'Logged h'],
  datasets: [{
    label: 'Planning health',
    data: [
      Number(velocitySummary.value.committedStoryPoints || 0),
      Number(velocitySummary.value.completedStoryPoints || 0),
      Number(velocitySummary.value.carryOverStoryPoints || 0),
      Number(estimateAccuracy.value.averageAccuracyPercent || 0),
      Number(planningOverview.value.totalEstimatedHours || 0),
      Number(planningOverview.value.totalLoggedHours || 0)
    ],
    fill: true,
    backgroundColor: 'rgba(14, 165, 233, 0.28)',
    borderColor: '#0EA5E9',
    pointBackgroundColor: '#0EA5E9',
    pointBorderColor: '#fff',
    pointHoverBackgroundColor: '#fff',
    pointHoverBorderColor: '#0EA5E9'
  }]
}))

const radarChartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  scales: {
    r: {
      grid: { color: '#e2e8f0' },
      angleLines: { color: '#e2e8f0' },
      ticks: { display: false }
    }
  },
  plugins: { legend: { display: false } }
}

const lineChartData = computed(() => ({
  labels: (velocitySummary.value.byProject || []).map(project => project.name),
  datasets: [
    {
      label: 'Committed story points',
      data: (velocitySummary.value.byProject || []).map(project => Number(project.velocityCommitted || 0)),
      borderColor: '#0EA5E9',
      backgroundColor: '#0EA5E9'
    },
    {
      label: 'Completed story points',
      data: (velocitySummary.value.byProject || []).map(project => Number(project.velocityCompleted || 0)),
      borderColor: '#10B981',
      backgroundColor: '#10B981'
    }
  ]
}))

const sprintVelocityChartData = computed(() => ({
  labels: (velocitySummary.value.bySprint || []).map(sprint => sprint.sprintName),
  datasets: [
    {
      label: 'Committed',
      data: (velocitySummary.value.bySprint || []).map(sprint => Number(sprint.committedStoryPoints || 0)),
      backgroundColor: '#3B82F6',
      borderRadius: 4
    },
    {
      label: 'Completed',
      data: (velocitySummary.value.bySprint || []).map(sprint => Number(sprint.completedStoryPoints || 0)),
      backgroundColor: '#10B981',
      borderRadius: 4
    },
    {
      label: 'Carry-over',
      data: (velocitySummary.value.bySprint || []).map(sprint => Number(sprint.carryOverStoryPoints || 0)),
      backgroundColor: '#F59E0B',
      borderRadius: 4
    }
  ]
}))

const barChartData = computed(() => {
  if (insightDimension.value === 'Accuracy') {
    return {
      labels: ['Accurate', 'Under-estimated', 'Over-estimated', 'Unplanned'],
      datasets: [{
        label: 'Estimate accuracy',
        data: [
          estimateAccuracy.value.accurateCount || 0,
          estimateAccuracy.value.underEstimatedCount || 0,
          estimateAccuracy.value.overEstimatedCount || 0,
          estimateAccuracy.value.unplannedCount || 0
        ],
        backgroundColor: ['#10B981', '#F59E0B', '#EF4444', '#64748B'],
        borderRadius: 4
      }]
    }
  }

  if (insightDimension.value === 'Workload') {
    return {
      labels: (workloadSummary.value.rows || []).map(row => row.userName),
      datasets: [
        {
          label: 'Estimated hours',
          data: (workloadSummary.value.rows || []).map(row => Number(row.estimatedHours || 0)),
          backgroundColor: '#3B82F6',
          borderRadius: 4
        },
        {
          label: 'Actual hours',
          data: (workloadSummary.value.rows || []).map(row => Number(row.actualHours || 0)),
          backgroundColor: '#F59E0B',
          borderRadius: 4
        }
      ]
    }
  }

  if (insightDimension.value === 'Status') {
    const labels = statusStats.value.map(item => item.Status)
    const counts = statusStats.value.map(item => item.Count)
    return {
      labels: labels.length ? labels : ['No status data'],
      datasets: [{
        label: 'Work items by status',
        data: counts.length ? counts : [0],
        backgroundColor: '#3B82F6',
        borderRadius: 4
      }]
    }
  }

  const labels = priorityStats.value.map(item => getPriorityLabel(item.Priority))
  const counts = priorityStats.value.map(item => item.Count)
  const bgColors = priorityStats.value.map(item => getPriorityColor(item.Priority))

  return {
    labels: labels.length ? labels : ['None', 'Low', 'Normal', 'High', 'Urgent'],
    datasets: [{
      label: 'Work items by priority',
      data: labels.length ? counts : [0, 0, 0, 0, 0],
      backgroundColor: labels.length ? bgColors : ['#cbd5e1', '#10B981', '#3B82F6', '#F97316', '#EF4444'],
      borderRadius: 4
    }]
  }
})

const chartConfig = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: { legend: { labels: { color: '#334155' } } },
  scales: {
    x: { grid: { color: '#e2e8f0' }, ticks: { color: '#64748b' } },
    y: { grid: { color: '#e2e8f0' }, ticks: { color: '#64748b' }, beginAtZero: true }
  }
}
</script>

<template>
  <div class="analytics-tab-wrapper">
    <div class="an-tabs">
      <button class="tab-btn" :class="{ active: activeTab === 'Overview' }" @click="activeTab = 'Overview'">Overview</button>
      <button class="tab-btn" :class="{ active: activeTab === 'Work items' }" @click="activeTab = 'Work items'">Work items</button>
    </div>

    <div class="an-content" v-if="activeTab === 'Overview'">
      <div class="stats-grid">
        <div class="stat-box">
          <div class="stat-lbl">Total work items</div>
          <div class="stat-val">{{ totalTasks }}</div>
        </div>
        <div class="stat-box">
          <div class="stat-lbl">My tasks</div>
          <div class="stat-val">{{ myTasks }}</div>
        </div>
        <div class="stat-box">
          <div class="stat-lbl">Overdue tasks</div>
          <div class="stat-val danger">{{ overdueTasks }}</div>
        </div>
        <div class="stat-box">
          <div class="stat-lbl">Committed story points</div>
          <div class="stat-val">{{ planningOverview.totalCommittedStoryPoints }}</div>
        </div>
        <div class="stat-box">
          <div class="stat-lbl">Completion rate</div>
          <div class="stat-val">{{ velocitySummary.completionRate }}%</div>
        </div>
      </div>

      <div class="insights-grid">
        <div class="insight-box">
          <div class="insight-title">Planning health</div>
          <div class="radar-container mt-4">
            <Radar :data="radarChartData" :options="radarChartOptions" />
          </div>
        </div>

        <div class="insight-box">
          <div class="insight-title">Velocity summary</div>
          <div class="summary-list mt-4">
            <div class="sum-row"><span class="sum-lbl">Completed story points</span><span class="sum-val">{{ velocitySummary.completedStoryPoints }}</span></div>
            <div class="sum-row"><span class="sum-lbl">Carry-over story points</span><span class="sum-val">{{ velocitySummary.carryOverStoryPoints }}</span></div>
            <div class="sum-row"><span class="sum-lbl">Estimated hours</span><span class="sum-val">{{ planningOverview.totalEstimatedHours }}h</span></div>
            <div class="sum-row"><span class="sum-lbl">Actual hours</span><span class="sum-val">{{ planningOverview.totalActualHours }}h</span></div>
            <div class="sum-row"><span class="sum-lbl">Logged hours</span><span class="sum-val">{{ planningOverview.totalLoggedHours }}h</span></div>
          </div>
        </div>

        <div class="insight-box">
          <div class="insight-title">Manager review</div>
          <div class="manager-panel mt-4">
            <div class="pill-row">
              <span class="pill" :class="{ positive: managerReview.canConfirmBaseline }">
                {{ managerReview.canConfirmBaseline ? 'Can confirm baseline' : 'View only' }}
              </span>
              <span class="pill warning">Over capacity: {{ workloadSummary.overCapacityCount }}</span>
            </div>
            <div class="summary-list">
              <div class="sum-row"><span class="sum-lbl">Near-limit members</span><span class="sum-val">{{ managerReview.riskSummary?.nearLimitMembers || 0 }}</span></div>
              <div class="sum-row"><span class="sum-lbl">Carry-over projects</span><span class="sum-val">{{ managerReview.riskSummary?.carryOverProjects || 0 }}</span></div>
              <div class="sum-row"><span class="sum-lbl">Unplanned tasks</span><span class="sum-val">{{ managerReview.riskSummary?.unplannedTasks || 0 }}</span></div>
            </div>
            <button
              v-if="managerReview.canConfirmBaseline"
              class="manager-action-btn"
              type="button"
              :disabled="loadingPlanning"
              @click="confirmPlanningBaseline"
            >
              {{ loadingPlanning ? 'Confirming...' : 'Confirm baseline' }}
            </button>
            <div class="baseline-list">
              <div v-for="project in managerReview.projects" :key="project.id" class="baseline-row">
                <strong>{{ project.name }}</strong>
                <small>{{ project.velocityCompleted }} / {{ project.velocityCommitted }} SP</small>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div class="an-content" v-else>
      <div class="full-analytics-body">
        <div class="ap-stats-grid">
          <div class="stat-box">
            <span class="lbl">Average accuracy</span>
            <span class="val">{{ estimateAccuracy.averageAccuracyPercent }}%</span>
          </div>
          <div class="stat-box">
            <span class="lbl">Accurate</span>
            <span class="val">{{ estimateAccuracy.accurateCount }}</span>
          </div>
          <div class="stat-box">
            <span class="lbl">Under-estimated</span>
            <span class="val">{{ estimateAccuracy.underEstimatedCount }}</span>
          </div>
          <div class="stat-box">
            <span class="lbl">Over-estimated</span>
            <span class="val">{{ estimateAccuracy.overEstimatedCount }}</span>
          </div>
          <div class="stat-box">
            <span class="lbl">Unplanned</span>
            <span class="val">{{ estimateAccuracy.unplannedCount }}</span>
          </div>
        </div>

        <div class="ap-chart-card mt-4">
          <h4>Velocity by project</h4>
          <div class="line-chart-container">
            <Line :data="lineChartData" :options="chartConfig" />
          </div>
        </div>

        <div class="ap-chart-card mt-4">
          <h4>Velocity by sprint</h4>
          <div class="bar-chart-container mt-4">
            <Bar :data="sprintVelocityChartData" :options="chartConfig" />
          </div>
        </div>

        <div class="ap-chart-card mt-4">
          <div class="flex-between">
            <h4>{{ workItemMetric }} by {{ insightDimension }}</h4>
            <div class="insight-filters">
              <el-dropdown trigger="click" @command="selectWorkItemMetric">
                <button class="filter-btn" type="button"><i class="fa-solid fa-briefcase"></i> {{ workItemMetric }} <i class="fa-solid fa-chevron-down"></i></button>
                <template #dropdown>
                  <el-dropdown-menu>
                    <el-dropdown-item command="Work item">Work item</el-dropdown-item>
                    <el-dropdown-item command="Planning">Planning</el-dropdown-item>
                  </el-dropdown-menu>
                </template>
              </el-dropdown>
              <el-dropdown trigger="click" @command="selectInsightDimension">
                <button class="filter-btn" type="button"><i class="fa-solid fa-list"></i> {{ insightDimension }} <i class="fa-solid fa-chevron-down"></i></button>
                <template #dropdown>
                  <el-dropdown-menu>
                    <el-dropdown-item command="Accuracy">Accuracy</el-dropdown-item>
                    <el-dropdown-item command="Workload">Workload</el-dropdown-item>
                    <el-dropdown-item command="Status">Status</el-dropdown-item>
                    <el-dropdown-item command="Priority">Priority</el-dropdown-item>
                  </el-dropdown-menu>
                </template>
              </el-dropdown>
            </div>
          </div>
          <div class="bar-chart-container mt-4">
            <Bar :data="barChartData" :options="chartConfig" />
          </div>
        </div>

        <div class="ap-chart-card mt-4">
          <div class="flex-between">
            <h4>Top estimate accuracy gaps</h4>
            <span class="muted-copy">{{ estimateAccuracy.rows.length }} rows</span>
          </div>
          <div class="table-wrap mt-4">
            <table class="analytics-table">
              <thead>
                <tr>
                  <th>Task</th>
                  <th>Estimated</th>
                  <th>Actual</th>
                  <th>Logged</th>
                  <th>Accuracy</th>
                  <th>Bucket</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="row in estimateAccuracy.rows" :key="row.id">
                  <td>{{ row.sequenceId || row.title }}</td>
                  <td>{{ row.estimatedHours }}h</td>
                  <td>{{ row.actualHours }}h</td>
                  <td>{{ row.loggedHours }}h</td>
                  <td>{{ row.accuracyPercent }}%</td>
                  <td>{{ row.bucket }}</td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

        <div class="ap-chart-card mt-4">
          <div class="flex-between">
            <h4>Estimator performance by assignee</h4>
            <span class="muted-copy">{{ estimateAccuracy.byUser.length }} rows</span>
          </div>
          <div class="table-wrap mt-4">
            <table class="analytics-table">
              <thead>
                <tr>
                  <th>User</th>
                  <th>Tasks</th>
                  <th>Estimated</th>
                  <th>Actual</th>
                  <th>Logged</th>
                  <th>Accuracy</th>
                  <th>Progress</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="row in estimateAccuracy.byUser" :key="row.userId">
                  <td>{{ row.userName }}</td>
                  <td>{{ row.taskCount }}</td>
                  <td>{{ row.estimatedHours }}h</td>
                  <td>{{ row.actualHours }}h</td>
                  <td>{{ row.loggedHours }}h</td>
                  <td>{{ row.averageAccuracyPercent }}%</td>
                  <td>{{ row.averageProgressPercent }}%</td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.analytics-tab-wrapper { margin-top: 24px; }
.an-tabs { display: flex; gap: 24px; margin-bottom: 24px; border-bottom: 1px solid #e2e8f0; }
.tab-btn { background: transparent; border: none; font-size: 14px; font-weight: 600; color: #64748b; cursor: pointer; padding: 12px 16px; border-bottom: 2px solid transparent; transition: all 0.2s; }
.tab-btn:hover { color: #0f172a; }
.tab-btn.active { color: #3b82f6; border-bottom: 2px solid #3b82f6; }

.an-content { animation: fadeIn 0.3s ease-in-out; }
@keyframes fadeIn { from { opacity: 0; transform: translateY(10px); } to { opacity: 1; transform: translateY(0); } }

.stats-grid { display: grid; grid-template-columns: repeat(5, 1fr); gap: 20px; margin-bottom: 24px; }
.stat-box { display: flex; flex-direction: column; gap: 8px; background: white; border: 1px solid #e2e8f0; border-radius: 12px; padding: 20px; box-shadow: 0 1px 2px rgba(0,0,0,0.05); }
.stat-lbl, .lbl, .muted-copy { font-size: 13px; color: #64748b; font-weight: 600; text-transform: uppercase; letter-spacing: 0.05em; }
.stat-val, .val { font-size: 24px; font-weight: 800; color: #0f172a; }
.danger { color: #ef4444; }

.insights-grid { display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 24px; }
@media (max-width: 1024px) { .insights-grid { grid-template-columns: 1fr; } .stats-grid { grid-template-columns: repeat(2, 1fr); } }

.insight-box, .ap-chart-card { border: 1px solid #e2e8f0; background: white; border-radius: 16px; padding: 24px; box-shadow: 0 4px 6px -1px rgba(0,0,0,0.05); }
.insight-title, .ap-chart-card h4 { font-size: 16px; font-weight: 700; color: #0f172a; margin: 0; }
.radar-container { height: 260px; }
.summary-list, .baseline-list { display: flex; flex-direction: column; gap: 12px; }
.sum-row, .baseline-row { display: flex; justify-content: space-between; align-items: center; gap: 12px; border-bottom: 1px dashed #f1f5f9; padding-bottom: 8px; }
.sum-row:last-child { border-bottom: none; padding-bottom: 0; }
.sum-lbl { color: #475569; font-size: 14px; }
.sum-val { color: #0f172a; font-weight: 600; }

.manager-panel { display: grid; gap: 16px; }
.pill-row { display: flex; gap: 10px; flex-wrap: wrap; }
.pill { padding: 6px 12px; border-radius: 999px; background: #f1f5f9; color: #475569; font-size: 12px; font-weight: 600; }
.pill.positive { background: #dcfce7; color: #166534; border: 1px solid #bbf7d0; }
.pill.warning { background: #fef3c7; color: #92400e; border: 1px solid #fde68a; }

.manager-action-btn, .filter-btn { background: white; border: 1px solid #cbd5e1; color: #334155; font-size: 13px; font-weight: 600; border-radius: 8px; padding: 8px 16px; cursor: pointer; transition: all 0.2s; display: inline-flex; align-items: center; gap: 8px; justify-content: center; }
.manager-action-btn:hover, .filter-btn:hover { background: #f8fafc; border-color: #94a3b8; }
.manager-action-btn:disabled { opacity: 0.5; cursor: not-allowed; }

.mt-4 { margin-top: 24px; }
.full-analytics-body { width: 100%; }
.ap-stats-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(180px, 1fr)); gap: 16px; }
.flex-between { display: flex; justify-content: space-between; align-items: center; gap: 16px; flex-wrap: wrap; }
.insight-filters { display: flex; gap: 12px; }
.line-chart-container, .bar-chart-container { height: 320px; margin-top: 24px; }

.table-wrap { overflow-x: auto; border: 1px solid #e2e8f0; border-radius: 12px; }
.analytics-table { width: 100%; border-collapse: collapse; font-size: 14px; }
.analytics-table th, .analytics-table td { padding: 12px 16px; border-bottom: 1px solid #e2e8f0; text-align: left; }
.analytics-table th { background: #f8fafc; color: #475569; font-weight: 600; }
.analytics-table tr:last-child td { border-bottom: none; }
.analytics-table tbody tr:hover { background: #f8fafc; }
</style>
