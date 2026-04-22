<script setup>
import { ref, onMounted } from 'vue'
import NexusLayout from '@/components/layout/NexusLayout.vue'
import axiosClient from '@/api/axiosClient'
import { ElMessage } from 'element-plus'

import { Bar, Line, Radar } from 'vue-chartjs'
import {
  Chart as ChartJS, Title, Tooltip, Legend, 
  BarElement, CategoryScale, LinearScale, 
  PointElement, LineElement, RadialLinearScale, 
  Filler, ArcElement 
} from 'chart.js'

ChartJS.register(
  Title, Tooltip, Legend, BarElement, CategoryScale, LinearScale, 
  PointElement, LineElement, RadialLinearScale, Filler, ArcElement
)

const activeTab = ref('Overview')
const analyticsScope = ref('All projects')
const insightDimension = ref('Priority')
const workItemMetric = ref('Work item')

const totalTasks = ref(0)
const totalProjects = ref(0)
const totalMembers = ref(0)
const myTasks = ref(0)
const overdueTasks = ref(0)

const statusStats = ref([])
const priorityStats = ref([])

const fetchStats = async () => {
    try {
        const res = await axiosClient.get('/dashboard/stats')
        const data = res.data?.data
        if (data) {
            totalTasks.value = data.totalTasks
            totalProjects.value = data.totalProjects
            totalMembers.value = data.totalMembers
            myTasks.value = data.myTasks
            overdueTasks.value = data.overdueTasks
            statusStats.value = data.byStatus || []
            priorityStats.value = data.byPriority || []
        }
    } catch(e) {
        console.error('Lỗi lấy thống kê', e)
    }
}

const notifyAnalyticsFilter = (label) => {
    ElMessage.info(`${label} filter is being prepared.`)
}

const selectAnalyticsScope = (scope) => {
    analyticsScope.value = scope
    ElMessage.success(`Analytics scope: ${scope}`)
}

const selectWorkItemMetric = (metric) => {
    workItemMetric.value = metric
    activeTab.value = 'Work items'
}

const selectInsightDimension = (dimension) => {
    insightDimension.value = dimension
    ElMessage.success(`Chart grouped by ${dimension}`)
}

onMounted(() => {
    fetchStats()
})

const getPriorityLabel = (val) => {
    const map = { 0: 'None', 1: 'Low', 2: 'Normal', 3: 'High', 4: 'Urgent' }
    return map[val] || 'None'
}
const getPriorityColor = (val) => {
    const map = { 0: 'var(--color-text-muted)', 1: '#10B981', 2: '#3B82F6', 3: '#F97316', 4: '#EF4444' }
    return map[val] || 'var(--color-text-muted)'
}

const radarChartData = {
    labels: ['Work items', 'Cycles', 'Modules', 'Intake', 'Members', 'Views'],
    datasets: [{
        label: 'Project Health',
        data: [8, 2, 3, 0, 2, 1], // Fake for layout since API doesn't have Cycles/Modules yet
        fill: true,
        backgroundColor: 'rgba(14, 165, 233, 0.4)',
        borderColor: '#0EA5E9',
        pointBackgroundColor: '#0EA5E9',
        pointBorderColor: '#fff',
        pointHoverBackgroundColor: '#fff',
        pointHoverBorderColor: '#0EA5E9'
    }]
}

const radarChartOptions = {
    responsive: true,
    maintainAspectRatio: false,
    scales: {
        r: {
            grid: { color: 'var(--color-border)' },
            angleLines: { color: 'var(--color-border)' },
            ticks: { display: false }
        }
    },
    plugins: { legend: { display: false } }
}

const getLineChartData = () => {
    return {
        labels: ['Day 1', 'Day 2', 'Day 3', 'Day 4', 'Day 5'], // Example
        datasets: [
            {
                label: 'Created',
                data: [0, 2, 3, 5, 8],
                borderColor: '#0EA5E9',
                backgroundColor: '#0EA5E9',
            },
            {
                label: 'Resolved',
                data: [0, 1, 1, 3, 5],
                borderColor: '#10B981',
                backgroundColor: '#10B981',
            }
        ]
    }
}

const getBarChartData = () => {
    if (insightDimension.value === 'Status') {
        const labels = statusStats.value.map(s => s.Status)
        const counts = statusStats.value.map(s => s.Count)
        return {
            labels: labels.length ? labels : ['No status data'],
            datasets: [{ label: 'Work Items by Status', data: counts.length ? counts : [0], backgroundColor: '#3B82F6', borderRadius: 4 }]
        }
    }

    const labels = priorityStats.value.map(p => getPriorityLabel(p.Priority))
    const counts = priorityStats.value.map(p => p.Count)
    const bgColors = priorityStats.value.map(p => getPriorityColor(p.Priority))
    
    // Default fallback
    if(labels.length === 0) {
        return {
            labels: ['None', 'Low', 'Normal', 'High', 'Urgent'],
            datasets: [{ label: 'Work Items by Priority', data: [0, 0, 0, 0, 0], backgroundColor: ['var(--color-text-muted)', '#10B981', '#3B82F6', '#F97316', '#EF4444'] }]
        }
    }

    return {
        labels,
        datasets: [
            {
                label: 'Work Items by Priority',
                data: counts,
                backgroundColor: bgColors,
                borderRadius: 4
            }
        ]
    }
}

const chartConfig = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: { legend: { labels: { color: 'var(--color-text-primary)' } } },
    scales: {
        x: { grid: { color: 'var(--color-border)' }, ticks: { color: 'var(--color-text-muted)' } },
        y: { grid: { color: 'var(--color-border)' }, ticks: { color: 'var(--color-text-muted)' }, beginAtZero: true }
    }
}

</script>

<template>
  <NexusLayout>
    <div class="analytics-wrapper">
      <header class="an-header">
         <div class="an-top-row">
            <span class="breadcrumb"><i class="fa-solid fa-chart-simple"></i> Analytics</span>
            <div class="ml-auto"></div>
         </div>
         <div class="an-bottom-row">
            <div class="an-tabs">
               <button class="tab-btn" :class="{ active: activeTab === 'Overview' }" @click="activeTab = 'Overview'">Overview</button>
               <button class="tab-btn" :class="{ active: activeTab === 'Work items' }" @click="activeTab = 'Work items'">Work items</button>
            </div>
            <el-dropdown trigger="click" class="ms-auto" @command="selectAnalyticsScope">
              <button class="plane-toolbar-btn" type="button"><i class="fa-solid fa-briefcase"></i> {{ analyticsScope }} <i class="fa-solid fa-chevron-down ms-2"></i></button>
              <template #dropdown>
                <el-dropdown-menu>
                  <el-dropdown-item command="All projects">All projects</el-dropdown-item>
                  <el-dropdown-item command="My projects">My projects</el-dropdown-item>
                  <el-dropdown-item command="Active projects">Active projects</el-dropdown-item>
                </el-dropdown-menu>
              </template>
            </el-dropdown>
         </div>
      </header>
      
      <!-- OVERVIEW TAB -->
      <div class="an-scrollable" v-if="activeTab === 'Overview'">
         <h2 class="page-title">Overview</h2>
         
         <div class="stats-grid">
            <div class="stat-box">
               <div class="stat-lbl">Total Work items</div>
               <div class="stat-val">{{ totalTasks }}</div>
            </div>
            <div class="stat-box">
               <div class="stat-lbl">My Tasks</div>
               <div class="stat-val">{{ myTasks }}</div>
            </div>
            <div class="stat-box">
               <div class="stat-lbl">Overdue Tasks</div>
               <div class="stat-val" style="color: #EF4444">{{ overdueTasks }}</div>
            </div>
            <div class="stat-box">
               <div class="stat-lbl">Total Projects</div>
               <div class="stat-val">{{ totalProjects }}</div>
            </div>
            <div class="stat-box">
               <div class="stat-lbl">Total Members</div>
               <div class="stat-val">{{ totalMembers }}</div>
            </div>
         </div>
         
         <div class="insights-grid">
            <!-- Radar Chart -->
            <div class="insight-box">
               <div class="insight-title">Project Insights</div>
               <div class="radar-container mt-4" style="height: 240px; width: 100%;">
                 <Radar :data="radarChartData" :options="radarChartOptions" />
               </div>
            </div>
            
            <!-- Summary list -->
            <div class="insight-box">
               <div class="insight-title" style="color: var(--color-text-muted); font-weight: 500;">Summary of Projects</div>
               <div class="insight-title mt-2">All Projects</div>
               <div class="insight-title" style="color: var(--color-text-muted); font-weight: 500; font-size: 13px; margin: 12px 0 24px;">Status Breakdown</div>
               
               <div class="summary-list">
                  <div class="sum-row" v-for="d in statusStats" :key="d.Status">
                     <span class="sum-lbl">{{ d.Status }}</span>
                     <span class="sum-val">{{ d.Count }}</span>
                  </div>
               </div>
            </div>
            
            <!-- Active Projects -->
            <div class="insight-box">
               <div class="insight-title">Active Database Info</div>
               <div class="active-proj mt-4">
                  <div class="proj-badge">
                     <i class="fa-solid fa-database" style="color: #F59E0B"></i>
                     <span>Live Data Status</span>
                  </div>
                  <div class="pill-green">Online</div>
               </div>
            </div>
         </div>
      </div>

      <!-- WORK ITEMS TAB -->
      <div class="an-scrollable" v-else>
         <div class="full-analytics-body">
            <!-- Stats -->
            <div class="ap-stats-grid">
               <div class="stat-box">
                  <span class="lbl">Total Work items</span>
                  <span class="val">{{ totalTasks }}</span>
               </div>
               <div class="stat-box" v-for="st in statusStats" :key="'ts_'+st.Status">
                  <span class="lbl">{{ st.Status }}</span>
                  <span class="val">{{ st.Count }}</span>
               </div>
            </div>
            
            <!-- Created vs Resolved Chart Overlay -->
            <div class="ap-chart-card mt-4">
               <h4>Created vs Resolved Trend</h4>
               <div class="line-chart-container" style="height: 250px; margin-top: 16px;">
                  <Line :data="getLineChartData()" :options="chartConfig" />
               </div>
            </div>

            <!-- Customized Insights -->
            <div class="ap-chart-card mt-4">
               <div class="flex-between">
                  <h4>Work items by {{ insightDimension }}</h4>
                  <div class="insight-filters">
                     <el-dropdown trigger="click" @command="selectWorkItemMetric">
                       <button class="filter-btn" type="button"><i class="fa-solid fa-briefcase"></i> {{ workItemMetric }} <i class="fa-solid fa-chevron-down"></i></button>
                       <template #dropdown>
                         <el-dropdown-menu>
                           <el-dropdown-item command="Work item">Work item</el-dropdown-item>
                           <el-dropdown-item command="Created">Created</el-dropdown-item>
                           <el-dropdown-item command="Resolved">Resolved</el-dropdown-item>
                           <el-dropdown-item command="Overdue">Overdue</el-dropdown-item>
                         </el-dropdown-menu>
                       </template>
                     </el-dropdown>
                     <el-dropdown trigger="click" @command="selectInsightDimension">
                       <button class="filter-btn" type="button"><i class="fa-solid fa-list"></i> {{ insightDimension }} <i class="fa-solid fa-chevron-down"></i></button>
                       <template #dropdown>
                         <el-dropdown-menu>
                           <el-dropdown-item command="Priority">Priority</el-dropdown-item>
                           <el-dropdown-item command="Status">Status</el-dropdown-item>
                         </el-dropdown-menu>
                       </template>
                     </el-dropdown>
                  </div>
               </div>
               
               <div class="bar-chart-container mt-4" style="height: 250px;">
                  <Bar :data="getBarChartData()" :options="chartConfig" />
               </div>
            </div>
         </div>
      </div>
    </div>
  </NexusLayout>
</template>

<style scoped>
.analytics-wrapper { display: flex; flex-direction: column; height: 100vh; background: var(--color-bg); color: var(--color-text-primary); }
.an-header { padding: 16px 24px 0; border-bottom: 1px solid var(--color-border); }
.an-top-row { display: flex; align-items: center; margin-bottom: 24px; }
.breadcrumb { color: var(--color-text-muted); font-size: 14px; display: flex; align-items: center; gap: 8px; font-weight: 500;}
.an-bottom-row { display: flex; align-items: center; margin-bottom: -1px; }

.an-tabs { display: flex; gap: 24px; }
.tab-btn { background: transparent; border: none; font-size: 13px; font-weight: 500; color: var(--color-text-muted); cursor: pointer; padding: 8px 0; border-bottom: 2px solid transparent; }
.tab-btn:hover { color: var(--color-text-primary); }
.tab-btn.active { color: var(--color-text-primary); border-bottom: 2px solid var(--color-text-primary); }

.ms-auto { margin-left: auto; }
.ms-2 { margin-left: 8px; font-size: 10px; }
.plane-toolbar-btn { background: var(--color-surface); border: 1px solid var(--color-border); color: #D4D4D8; font-size: 13px; font-weight: 500; cursor: pointer; padding: 6px 12px; border-radius: 6px; margin-bottom: 12px; display: flex; align-items: center; gap: 6px; }
.plane-toolbar-btn:hover { background: var(--color-border); }

.an-scrollable { padding: 32px; overflow-y: auto; flex: 1; }
.page-title { margin: 0 0 32px 0; font-size: 20px; font-weight: 600; }

.stats-grid { display: grid; grid-template-columns: repeat(5, 1fr); gap: 24px; margin-bottom: 40px; }
.stat-box { display: flex; flex-direction: column; gap: 8px; }
.stat-lbl { font-size: 12px; color: var(--color-text-muted); font-weight: 500; }
.stat-val { font-size: 20px; font-weight: 600; color: var(--color-text-primary); }

.insights-grid { display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 40px; }
.insight-title { font-size: 15px; font-weight: 600; color: var(--color-text-primary); }
.radar-container { display: flex; align-items: center; justify-content: center; padding: 0; }
.mt-4 { margin-top: 24px; }
.mt-2 { margin-top: 8px; }

.summary-list { display: flex; flex-direction: column; gap: 20px; }
.sum-row { display: flex; justify-content: space-between; font-size: 13px; border-bottom: 1px solid var(--color-border); padding-bottom: 8px; }
.sum-lbl { color: var(--color-text-primary); }
.sum-val { color: var(--color-text-muted); font-weight: 500; }

.active-proj { display: flex; justify-content: space-between; align-items: center; background: var(--color-surface); padding: 12px; border-radius: 6px; border: 1px solid var(--color-border); }
.proj-badge { display: flex; align-items: center; gap: 8px; font-size: 13px; font-weight: 500; }
.pill-red { background: rgba(153, 27, 27, 0.2); color: #F87171; font-size: 11px; padding: 2px 6px; border-radius: 4px; font-weight: 600; }
.pill-green { background: rgba(16, 185, 129, 0.2); color: #10B981; font-size: 11px; padding: 2px 6px; border-radius: 4px; font-weight: 600; }

/* Expanded Analytics (copied and adapted from SpaceSummary.vue) */
.full-analytics-body { width: 100%; max-width: 1000px;}
.ap-stats-grid { display: flex; gap: 24px; flex-wrap: wrap; }
.ap-stats-grid .stat-box { flex: 1; min-width: 150px; background: var(--color-surface); padding: 16px; border-radius: 8px; border: 1px solid var(--color-border); }
.ap-stats-grid .lbl { font-size: 12px; color: var(--color-text-muted); display: block; margin-bottom: 8px;}
.ap-stats-grid .val { font-size: 24px; font-weight: 600; color: var(--color-text-primary); }
.ap-chart-card { margin-top: 32px; background: var(--color-surface); padding: 20px; border-radius: 8px; border: 1px solid var(--color-border); }
.ap-chart-card h4 { margin: 0; font-size: 14px; font-weight: 600; color: var(--color-text-primary); }

.insight-filters { display: flex; gap: 8px; }
.filter-btn { background: transparent; border: 1px solid var(--color-border); border-radius: 4px; padding: 4px 8px; font-size: 12px; color: #D4D4D8; cursor: pointer; }
.filter-btn i { color: var(--color-text-muted); font-size: 10px; margin-left: 4px; }
.flex-between { display: flex; justify-content: space-between; align-items: center; }
</style>



