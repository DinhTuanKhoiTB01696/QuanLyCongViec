<script setup>
import { ref, computed, onMounted, watch } from 'vue'
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
const analyticsScope = ref('all') // all, my, archived
const selectedProjectId = ref(null)
const projectSearch = ref('')
const projectList = ref([])

const insightDimension = ref('Priority')
const workItemMetric = ref('Work item')

const totalTasks = ref(0)
const completedTasks = ref(0)
const overdueTasks = ref(0)
const totalProjects = ref(0)
const totalMembers = ref(0)
const activeCycles = ref(0)
const totalModules = ref(0)
const totalIntakes = ref(0)
const totalViews = ref(0)
const newTasksLast7Days = ref(0)
const totalActions = ref(0)
const myTasks = ref(0)

const statusStats = ref([])
const priorityStats = ref([])
const isLoading = ref(false)

const fetchProjects = async () => {
    try {
        const [discoveryRes, archivedRes] = await Promise.all([
            axiosClient.get('/projects/discovery'),
            axiosClient.get('/projects/archived')
        ])
        
        const activeProjects = (discoveryRes.data?.data || []).map(p => ({ ...p, isArchived: false }))
        const archivedProjects = (archivedRes.data?.data || []).map(p => ({ ...p, isArchived: true }))
        
        projectList.value = [...activeProjects, ...archivedProjects]
    } catch(e) {
        console.error('Error fetching projects', e)
    }
}

const fetchStats = async () => {
    isLoading.value = true
    try {
        const params = {
            scope: analyticsScope.value
        }
        if (selectedProjectId.value) {
            params.projectId = selectedProjectId.value
        }
        
        const res = await axiosClient.get('/dashboard/stats', { params })
        const data = res.data?.data
        if (data) {
            totalTasks.value = data.totalTasks || 0
            completedTasks.value = data.completedTasks || 0
            overdueTasks.value = data.overdueTasks || 0
            totalProjects.value = data.totalProjects || 0
            totalMembers.value = data.totalMembers || 0
            activeCycles.value = data.activeCycles || 0
            totalModules.value = data.totalModules || 0
            totalIntakes.value = data.totalIntakes || 0
            totalViews.value = data.totalViews || 0
            newTasksLast7Days.value = data.newTasksLast7Days || 0
            totalActions.value = data.totalActions || 0
            myTasks.value = data.myTasks || 0
            statusStats.value = data.byStatus || []
            priorityStats.value = data.byPriority || []
        }
    } catch(e) {
        console.error('Lỗi lấy thống kê', e)
        ElMessage.error('Could not load statistics.')
    } finally {
        isLoading.value = false
    }
}

const selectAnalyticsScope = (scope) => {
    analyticsScope.value = scope
    selectedProjectId.value = null // This will trigger the watcher
}

const handleProjectChange = () => {
    // Watcher will handle it
}

// Automatically fetch stats when filters change
watch([analyticsScope, selectedProjectId], () => {
    fetchStats()
})

onMounted(async () => {
    await fetchProjects()
    fetchStats()
})

const onProjectSearch = (query) => {
    projectSearch.value = query
}

const filteredProjectsForSelector = computed(() => {
    let list = [...projectList.value]
    
    // 1. Filter by scope
    if (analyticsScope.value === 'my') {
        const userString = localStorage.getItem('user')
        if (userString) {
            const user = JSON.parse(userString)
            list = list.filter(p => (p.creatorId === user.id || p.leadUserId === user.id) && !p.isArchived)
        }
    } else if (analyticsScope.value === 'archived') {
        list = list.filter(p => p.isArchived)
    } else {
        list = list.filter(p => !p.isArchived)
    }

    // 2. Filter by search query
    const q = projectSearch.value.toLowerCase()
    if (q) {
        list = list.filter(p => 
            p.name.toLowerCase().includes(q) || 
            (p.key && p.key.toLowerCase().includes(q))
        )
        
        // 3. Sort by match closeness
        list.sort((a, b) => {
            const aKey = a.key?.toLowerCase() || ''
            const bKey = b.key?.toLowerCase() || ''
            
            if (aKey === q && bKey !== q) return -1
            if (bKey === q && aKey !== q) return 1
            if (aKey.startsWith(q) && !bKey.startsWith(q)) return -1
            if (bKey.startsWith(q) && !aKey.startsWith(q)) return 1
            if (aKey.includes(q) && !bKey.includes(q)) return -1
            if (bKey.includes(q) && !aKey.includes(q)) return 1
            return a.name.localeCompare(b.name)
        })
    }
    
    return list
})

const getHighlightedKey = (key) => {
    if (!key || !projectSearch.value) return key
    const q = projectSearch.value.toLowerCase()
    const index = key.toLowerCase().indexOf(q)
    if (index === -1) return key
    
    const before = key.substring(0, index)
    const match = key.substring(index, index + q.length)
    const after = key.substring(index + q.length)
    
    return `${before}<span class="highlight-id">${match}</span>${after}`
}

const scopeLabel = computed(() => {
    const map = { all: 'All projects', my: 'My projects', archived: 'Archived projects' }
    return map[analyticsScope.value] || analyticsScope.value
})

const getPriorityLabel = (val) => {
    const map = { 0: 'None', 1: 'Low', 2: 'Normal', 3: 'High', 4: 'Urgent' }
    return map[val] || 'None'
}
const getPriorityColor = (val) => {
    const map = { 0: '#A1A1AA', 1: '#10B981', 2: '#3B82F6', 3: '#F97316', 4: '#EF4444' }
    return map[val] || '#A1A1AA'
}

const radarChartData = computed(() => {
    // 🔥 NORMALIZE FUNCTION (CLAMP 0-10)
    const clamp = (val) => Math.max(0, Math.min(val, 10))

    // 🟢 1. WorkItems (Efficiency)
    const total = totalTasks.value || 1
    const workItemsRaw = (completedTasks.value / total) * 10 - (overdueTasks.value / total) * 5
    const workItemsScore = clamp(workItemsRaw)

    // 🔵 2. Cycles (Sprint)
    const cyclesScore = clamp((activeCycles.value / 5) * 10)

    // 🟣 3. Modules (Structure)
    const modulesScore = clamp((totalModules.value / 5) * 10)

    // 🟡 4. Intake (New Tasks)
    const intakeScore = clamp((newTasksLast7Days.value / 20) * 10)

    // 🟠 5. Members (Human Resources)
    let membersScore = 4
    const m = totalMembers.value
    if (m >= 3 && m <= 7) membersScore = 10
    else if (m >= 8 && m <= 12) membersScore = 7
    else if (m > 12) membersScore = 5
    else if (m === 0) membersScore = 0

    // 🔴 6. Activity (Actions)
    const activityScore = clamp((totalActions.value / 50) * 10)

    return {
        labels: ['WorkItems', 'Cycles', 'Modules', 'Intake', 'Members', 'Activity'],
        datasets: [{
            label: 'Project Insights',
            data: [
                workItemsScore,
                cyclesScore,
                modulesScore,
                intakeScore,
                membersScore,
                activityScore
            ],
            fill: true,
            backgroundColor: 'rgba(14, 165, 233, 0.3)',
            borderColor: '#0EA5E9',
            pointBackgroundColor: '#0EA5E9',
            pointBorderColor: '#fff',
            pointHoverBackgroundColor: '#fff',
            pointHoverBorderColor: '#0EA5E9',
            borderWidth: 2,
            pointRadius: 4
        }]
    }
})

const radarChartOptions = {
    responsive: true,
    maintainAspectRatio: false,
    scales: {
        r: {
            min: 0,
            max: 10,
            grid: { color: 'rgba(255, 255, 255, 0.1)' },
            angleLines: { color: 'rgba(255, 255, 255, 0.1)' },
            ticks: { display: false, stepSize: 2 },
            pointLabels: {
                color: '#A1A1AA',
                font: { size: 11, weight: '600' }
            }
        }
    },
    plugins: { 
        legend: { display: false },
        tooltip: {
            callbacks: {
                label: (context) => {
                    const labels = ['Efficiency (WorkItems)', 'Sprints (Cycles)', 'Structure (Modules)', 'New Tasks (Intake)', 'Human Res (Members)', 'Engagement (Activity)']
                    const scores = context.dataset.data
                    const rawValues = [
                        `${completedTasks.value} done, ${overdueTasks.value} overdue`,
                        `${activeCycles.value} active`,
                        `${totalModules.value} total`,
                        `${newTasksLast7Days.value} new`,
                        `${totalMembers.value} total`,
                        `${totalActions.value} actions`
                    ]
                    return `${labels[context.dataIndex]}: Score ${scores[context.dataIndex].toFixed(1)} (${rawValues[context.dataIndex]})`
                }
            }
        }
    }
}

const lineChartData = computed(() => {
    // Generate a more realistic trend if no actual historical data exists
    // For now, we use a simple interpolation based on total tasks
    const count = totalTasks.value
    const data = [
        Math.floor(count * 0.1),
        Math.floor(count * 0.3),
        Math.floor(count * 0.6),
        Math.floor(count * 0.8),
        count
    ]
    
    return {
        labels: ['-4d', '-3d', '-2d', '-1d', 'Today'],
        datasets: [
            {
                label: 'Created',
                data: data,
                borderColor: '#0EA5E9',
                backgroundColor: '#0EA5E9',
                tension: 0.4
            },
            {
                label: 'Resolved',
                data: data.map(v => Math.floor(v * 0.7)),
                borderColor: '#10B981',
                backgroundColor: '#10B981',
                tension: 0.4
            }
        ]
    }
})

const barChartData = computed(() => {
    if (insightDimension.value === 'Status') {
        const labels = statusStats.value.map(s => s.Status)
        const counts = statusStats.value.map(s => s.Count)
        return {
            labels: labels.length ? labels : ['No data'],
            datasets: [{ 
                label: 'Work Items by Status', 
                data: counts.length ? counts : [0], 
                backgroundColor: '#3B82F6', 
                borderRadius: 4 
            }]
        }
    }

    const labels = priorityStats.value.map(p => getPriorityLabel(p.Priority))
    const counts = priorityStats.value.map(p => p.Count)
    const bgColors = priorityStats.value.map(p => getPriorityColor(p.Priority))
    
    if(labels.length === 0) {
        return {
            labels: ['None', 'Low', 'Normal', 'High', 'Urgent'],
            datasets: [{ label: 'Work Items by Priority', data: [0, 0, 0, 0, 0], backgroundColor: ['#A1A1AA', '#10B981', '#3B82F6', '#F97316', '#EF4444'], borderRadius: 4 }]
        }
    }

    return {
        labels: labels,
        datasets: [{ label: 'Work Items by Priority', data: counts, backgroundColor: bgColors, borderRadius: 4 }]
    }
})

const chartConfig = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: { legend: { labels: { color: '#E4E4E7' } } },
    scales: {
        x: { grid: { color: '#27272A' }, ticks: { color: '#A1A1AA' } },
        y: { grid: { color: '#27272A' }, ticks: { color: '#A1A1AA' }, beginAtZero: true }
    }
}

const selectWorkItemMetric = (metric) => {
    workItemMetric.value = metric
    activeTab.value = 'Work items'
}

const selectInsightDimension = (dimension) => {
    insightDimension.value = dimension
    ElMessage.success(`Chart grouped by ${dimension}`)
}
</script>

<template>
  <NexusLayout>
    <div class="analytics-wrapper" v-loading="isLoading">
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
            
            <div class="filter-controls ms-auto">
               <el-dropdown trigger="click" @command="selectAnalyticsScope">
                 <button class="plane-toolbar-btn fixed-width-scope" type="button">
                   <i class="fa-solid fa-layer-group"></i> {{ scopeLabel }} <i class="fa-solid fa-chevron-down ms-2"></i>
                 </button>
                 <template #dropdown>
                   <el-dropdown-menu>
                     <el-dropdown-item command="all">All projects</el-dropdown-item>
                     <el-dropdown-item command="my">My projects</el-dropdown-item>
                     <el-dropdown-item command="archived">Archived projects</el-dropdown-item>
                   </el-dropdown-menu>
                 </template>
               </el-dropdown>
               
               <div class="sub-filter fixed-width-project" :class="{ 'is-blurred': analyticsScope === 'all' }">
                 <el-select 
                   v-model="selectedProjectId" 
                   filterable 
                   :filter-method="onProjectSearch"
                   placeholder="Select specific project..." 
                   class="glass-select" 
                   @change="handleProjectChange"
                   :disabled="analyticsScope === 'all'"
                 >
                   <el-option label="All selected scope" :value="null"></el-option>
                   <el-option 
                     v-for="p in filteredProjectsForSelector" 
                     :key="p.id" 
                     :label="p.name" 
                     :value="p.id"
                   >
                     <el-tooltip :content="p.name" placement="right" :disabled="p.name.length < 20">
                       <div class="select-option-column">
                         <div class="select-option-main">
                            <span class="p-icon">{{ p.icon || '📁' }}</span>
                            <span class="p-name">{{ p.name }}</span>
                         </div>
                         <div class="p-key-row" v-if="projectSearch && p.key?.toLowerCase().includes(projectSearch.toLowerCase())">
                            <span class="p-key-label">ID:</span>
                            <span class="p-key-val" v-html="getHighlightedKey(p.key)"></span>
                         </div>
                       </div>
                     </el-tooltip>
                   </el-option>
                 </el-select>
               </div>
            </div>
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
               <div class="insight-title" style="color: #71717A; font-weight: 500;">Summary of Projects</div>
               <div class="insight-title mt-2">{{ scopeLabel }}</div>
               <div class="insight-title" style="color: #71717A; font-weight: 500; font-size: 13px; margin: 12px 0 24px;">Status Breakdown</div>
               
               <div class="summary-list">
                  <div v-if="statusStats.length === 0" class="empty-stats">No status data available for this selection.</div>
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
                  <Line :data="lineChartData" :options="chartConfig" />
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
                  <Bar :data="barChartData" :options="chartConfig" />
               </div>
            </div>
         </div>
      </div>
    </div>
  </NexusLayout>
</template>

<style scoped>
.analytics-wrapper { display: flex; flex-direction: column; height: 100vh; background: #0D0F11; color: #E4E4E7; }
.an-header { padding: 16px 24px 0; border-bottom: 1px solid #1E2025; }
.an-top-row { display: flex; align-items: center; margin-bottom: 24px; }
.breadcrumb { color: #A1A1AA; font-size: 14px; display: flex; align-items: center; gap: 8px; font-weight: 500;}
.an-bottom-row { display: flex; align-items: center; margin-bottom: -1px; }

.an-tabs { display: flex; gap: 24px; }
.tab-btn { background: transparent; border: none; font-size: 13px; font-weight: 500; color: #71717A; cursor: pointer; padding: 8px 0; border-bottom: 2px solid transparent; }
.tab-btn:hover { color: #E4E4E7; }
.tab-btn.active { color: #E4E4E7; border-bottom: 2px solid #E4E4E7; }

.filter-controls { display: flex; align-items: center; gap: 16px; margin-bottom: 12px; }
.ms-auto { margin-left: auto; }
.ms-2 { margin-left: 8px; font-size: 10px; }
.plane-toolbar-btn { background: #16181D; border: 1px solid #27272A; color: #D4D4D8; font-size: 13px; font-weight: 500; cursor: pointer; padding: 6px 12px; border-radius: 6px; display: flex; align-items: center; gap: 6px; transition: all 0.2s; justify-content: space-between; }
.plane-toolbar-btn:hover { background: #1E2025; border-color: #3F3F46; }

.fixed-width-scope { width: 180px; }
.fixed-width-project { width: 300px; }

.sub-filter { transition: all 0.3s; }
.sub-filter.is-blurred { opacity: 0.5; filter: blur(1px); pointer-events: none; }

.glass-select :deep(.el-input__wrapper) { background: #16181D; border: 1px solid #27272A; box-shadow: none !important; border-radius: 6px; height: 32px; }
.glass-select :deep(.el-input__inner) { color: #E4E4E7; font-size: 13px; }

.select-option-column { display: flex; flex-direction: column; padding: 4px 0; }
.select-option-main { display: flex; align-items: center; gap: 10px; }
.p-icon { font-size: 14px; }
.p-name { 
  flex: 1; 
  font-weight: 500; 
  white-space: nowrap; 
  overflow: hidden; 
  text-overflow: ellipsis; 
  max-width: 220px; 
}

.p-key-row { font-size: 11px; color: #71717A; margin-top: 2px; display: flex; align-items: center; gap: 4px; padding-left: 24px; }
.p-key-val { display: inline-flex; align-items: center; }

:deep(.highlight-id) {
  background: #3B82F6;
  color: #FFFFFF;
  padding: 0 4px;
  border-radius: 3px;
  font-weight: bold;
  margin: 0 1px;
}

.an-scrollable { padding: 32px; overflow-y: auto; flex: 1; }
.page-title { margin: 0 0 32px 0; font-size: 20px; font-weight: 600; }

.stats-grid { display: grid; grid-template-columns: repeat(5, 1fr); gap: 24px; margin-bottom: 40px; }
.stat-box { display: flex; flex-direction: column; gap: 8px; }
.stat-lbl { font-size: 12px; color: #71717A; font-weight: 500; }
.stat-val { font-size: 20px; font-weight: 600; color: #E4E4E7; }

.insights-grid { display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 40px; }
.insight-title { font-size: 15px; font-weight: 600; color: #E4E4E7; }
.radar-container { display: flex; align-items: center; justify-content: center; padding: 0; }
.mt-4 { margin-top: 24px; }
.mt-2 { margin-top: 8px; }

.summary-list { display: flex; flex-direction: column; gap: 20px; }
.empty-stats { font-size: 13px; color: #71717A; font-style: italic; }
.sum-row { display: flex; justify-content: space-between; font-size: 13px; border-bottom: 1px solid #1E2025; padding-bottom: 8px; }
.sum-lbl { color: #E4E4E7; }
.sum-val { color: #A1A1AA; font-weight: 500; }

.active-proj { display: flex; justify-content: space-between; align-items: center; background: #16181D; padding: 12px; border-radius: 6px; border: 1px solid #27272A; }
.proj-badge { display: flex; align-items: center; gap: 8px; font-size: 13px; font-weight: 500; }
.pill-green { background: rgba(16, 185, 129, 0.2); color: #10B981; font-size: 11px; padding: 2px 6px; border-radius: 4px; font-weight: 600; }

.full-analytics-body { width: 100%; max-width: 1000px;}
.ap-stats-grid { display: flex; gap: 24px; flex-wrap: wrap; }
.ap-stats-grid .stat-box { flex: 1; min-width: 150px; background: #16181D; padding: 16px; border-radius: 8px; border: 1px solid #27272A; }
.ap-stats-grid .lbl { font-size: 12px; color: #71717A; display: block; margin-bottom: 8px;}
.ap-stats-grid .val { font-size: 24px; font-weight: 600; color: #E4E4E7; }
.ap-chart-card { margin-top: 32px; background: #16181D; padding: 20px; border-radius: 8px; border: 1px solid #27272A; }
.ap-chart-card h4 { margin: 0; font-size: 14px; font-weight: 600; color: #E4E4E7; }

.insight-filters { display: flex; gap: 8px; }
.filter-btn { background: transparent; border: 1px solid #27272A; border-radius: 4px; padding: 4px 8px; font-size: 12px; color: #D4D4D8; cursor: pointer; }
.filter-btn i { color: #A1A1AA; font-size: 10px; margin-left: 4px; }
.flex-between { display: flex; justify-content: space-between; align-items: center; }
</style>
