<template>
  <NexusLayout>
    <div class="dashboard-header">
      <div class="header-content">
        <h1>Welcome back, {{ currentUser?.name || 'User' }}! 👋</h1>
        <p class="text-muted">Here's what's happening with your projects today.</p>
      </div>

    </div>

    <!-- 4.1 Statistical Cards -->
    <div class="stats-grid">
      <div class="stat-card" v-for="stat in stats" :key="stat.id">
        <div class="stat-header">
          <span class="stat-title">{{ stat.title }}</span>
          <div class="stat-icon"><i :class="stat.icon"></i></div>
        </div>
        <div class="stat-body">
          <h2 class="stat-value">{{ stat.value }}</h2>
          <div class="stat-trend" :class="stat.trend > 0 ? 'positive' : 'negative'">
            <i :class="stat.trend > 0 ? 'fa-solid fa-arrow-up' : 'fa-solid fa-arrow-down'"></i>
            <span>{{ Math.abs(stat.trend) }}%</span>
            <span class="trend-text">from last month</span>
          </div>
        </div>
      </div>
    </div>

    <!-- Main Content Grid -->
    <div class="dashboard-grid">
      <!-- 4.2 Campaign Chart -->
      <div class="chart-section dashboard-panel">
        <div class="panel-header">
          <h3>Activity Overview</h3>
          <div class="panel-actions">
            <select class="action-select">
              <option>Last 7 days</option>
              <option>Last 30 days</option>
            </select>
          </div>
        </div>
        <div class="chart-container" ref="chartRef">
          <!-- Echarts will mount here -->
        </div>
      </div>

      <!-- 4.3 Schedule & Events -->
      <div class="schedule-section dashboard-panel">
        <div class="panel-header">
          <h3>Upcoming Schedule</h3>
          <div class="schedule-nav">
            <button class="nav-btn"><i class="fa-solid fa-chevron-left"></i></button>
            <button class="nav-btn"><i class="fa-solid fa-chevron-right"></i></button>
          </div>
        </div>
        <div class="weekly-calendar">
          <div class="day" v-for="day in weekDays" :key="day.date" :class="{ active: day.active }">
            <span class="day-name">{{ day.name }}</span>
            <span class="day-num">{{ day.date }}</span>
          </div>
        </div>
        <div class="events-list">
          <div v-if="events.length === 0" class="empty-events">
            <p>No upcoming events</p>
          </div>
          <div class="event-item" v-for="event in events" :key="event.id" v-else>
            <div class="event-dot" :style="{ backgroundColor: event.color }"></div>
            <div class="event-info">
              <h4>{{ event.title }}</h4>
              <span class="event-time">{{ event.time }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Recent Spaces -->
    <div class="recent-spaces-section">
      <div class="section-header">
        <h3>Không gian gần đây</h3>
      </div>
      <div v-if="isLoading" class="loading-state">
        <i class="fa-solid fa-spinner fa-spin"></i> Đang tải...
      </div>
      <div v-else-if="spaces.length === 0" class="empty-state">
        <i class="fa-regular fa-folder-open"></i>
        <p>Chưa có không gian nào</p>
      </div>
      <div v-else class="recent-spaces-grid">
        <div class="space-card" v-for="(space, index) in spaces" :key="space.id" @click="goToSpace(space)" :class="{ 'not-member': !space.isMember }">
          <div class="space-icon-box" :style="{ background: spaceGradients[index % spaceGradients.length] }">
            <i :class="spaceIcons[index % spaceIcons.length]"></i>
          </div>
          <div class="space-details">
            <h4>{{ space.name }}</h4>
            <span>{{ space.description || 'Workspace' }}</span>
            <div class="space-meta">
              <span class="meta-item"><i class="fa-solid fa-users"></i> {{ space.activeMemberCount || 0 }}</span>
              <span class="meta-item" :class="space.status ? 'active-status' : 'archived-status'">
                <i :class="space.status ? 'fa-solid fa-circle-check' : 'fa-solid fa-archive'"></i>
                {{ space.status ? 'Hoạt động' : 'Đã lưu trữ' }}
              </span>
            </div>
          </div>
          <div class="space-arrow" v-if="space.isMember">
            <i class="fa-solid fa-arrow-right"></i>
          </div>
          <div class="join-badge" v-else>
            <span>Tham gia</span>
          </div>
        </div>
      </div>
    </div>
  </NexusLayout>
</template>

<script setup>
import { ref, onMounted, onBeforeUnmount } from 'vue'
import { useRouter } from 'vue-router'
import * as echarts from 'echarts'
import axiosClient from '@/api/axiosClient'
import NexusLayout from '@/components/layout/NexusLayout.vue'
import { ElMessage } from 'element-plus'

const router = useRouter()
const currentUser = JSON.parse(localStorage.getItem('user') || '{}')

const isLoading = ref(false)
const spaces = ref([])
const chartRef = ref(null)
let chartInstance = null

const stats = ref([
  { id: 2, title: 'Active Projects', value: '...', trend: 0, icon: 'fa-solid fa-folder' },
  { id: 3, title: 'Tasks Completed', value: '...', trend: 0, icon: 'fa-solid fa-check-double' },
  { id: 4, title: 'Team Members', value: '...', trend: 0, icon: 'fa-solid fa-users' }
])

const chartData = ref({
  days: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'],
  created: [0, 0, 0, 0, 0, 0, 0],
  completed: [0, 0, 0, 0, 0, 0, 0]
})

const getWeekDays = () => {
  const days = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat']
  const today = new Date()
  const todayDay = today.getDay()
  const week = []
  
  for (let i = 0; i < 7; i++) {
    const d = new Date()
    d.setDate(today.getDate() - todayDay + i)
    week.push({
      name: days[d.getDay()],
      date: d.getDate().toString(),
      active: i === todayDay
    })
  }
  return week
}

const weekDays = ref(getWeekDays())

const events = ref([])

const goToSpace = (space) => {
  if (space.isMember) {
    router.push(`/space/${space.id}`)
  } else {
    ElMessage.warning('Bạn chưa là thành viên của dự án này. Hãy liên hệ PM để được mời tham gia.')
  }
}

const spaceGradients = [
  'linear-gradient(135deg, #3b82f6, #8b5cf6)',
  'linear-gradient(135deg, #10b981, #059669)',
  'linear-gradient(135deg, #f59e0b, #ef4444)',
  'linear-gradient(135deg, #8b5cf6, #ec4899)',
  'linear-gradient(135deg, #06b6d4, #3b82f6)',
  'linear-gradient(135deg, #f97316, #eab308)'
]

const spaceIcons = [
  'fa-solid fa-rocket',
  'fa-solid fa-code',
  'fa-solid fa-palette',
  'fa-solid fa-chart-line',
  'fa-solid fa-gear',
  'fa-solid fa-bolt'
]

const fetchSpaces = async () => {
  isLoading.value = true
  try {
    const response = await axiosClient.get('/projects/discovery')
    const projectList = response.data.data || response.data || []
    spaces.value = projectList
    
    // Fetch dashboard stats from backend
    const statsRes = await axiosClient.get('/dashboard/stats')
    const realStats = statsRes.data.data
    
    if (realStats) {
      stats.value[0].value = realStats.totalProjects?.toString() || '0'
      stats.value[1].value = realStats.totalTasks?.toString() || '0'
      stats.value[2].value = realStats.totalMembers?.toString() || '0'
    }

    // Fetch tasks for the user to determine dashboard chart and schedule logic
    const tasksRes = await axiosClient.get('/tasks/my-tasks')
    const tasks = tasksRes.data.data || tasksRes.data || []
    
    processTasksForDashboard(tasks)
  } catch (error) {
    console.error('Fetch dashboard data error:', error)
  } finally {
    isLoading.value = false
  }
}

const processTasksForDashboard = (tasks) => {
  const now = new Date()
  const today = new Date(now.getFullYear(), now.getMonth(), now.getDate())
  
  const days = []
  const dates = []
  for (let i = 6; i >= 0; i--) {
    const d = new Date(today)
    d.setDate(today.getDate() - i)
    days.push(['Sun','Mon','Tue','Wed','Thu','Fri','Sat'][d.getDay()])
    dates.push(d)
  }

  const createdCounts = [0, 0, 0, 0, 0, 0, 0]
  const completedCounts = [0, 0, 0, 0, 0, 0, 0]
  const upcomingEventsList = []
  let totalTasksCompleted = 0

  tasks.forEach(task => {
    // Determine overall completed count
    if (task.statusName === 'DONE' || task.status === 'Completed') {
      totalTasksCompleted++;
    }

    // Created distribution
    if (task.createdAt) {
      let cd = new Date(task.createdAt)
      cd = new Date(cd.getFullYear(), cd.getMonth(), cd.getDate())
      const diffIndex = Math.floor((cd - dates[0]) / (1000 * 60 * 60 * 24))
      if (diffIndex >= 0 && diffIndex <= 6) {
        createdCounts[diffIndex]++
      }
    }

    // Completed distribution
    if ((task.statusName === 'DONE' || task.status === 'Completed') && task.updatedAt) {
      let ud = new Date(task.updatedAt)
      ud = new Date(ud.getFullYear(), ud.getMonth(), ud.getDate())
      const diffIndex = Math.floor((ud - dates[0]) / (1000 * 60 * 60 * 24))
      if (diffIndex >= 0 && diffIndex <= 6) {
        completedCounts[diffIndex]++
      }
    }

    // Upcoming schedule items (focus on tasks due in the future week)
    let targetDate = task.dueDate || task.plannedEndDate
    if (targetDate && task.statusName !== 'DONE') {
      let dDate = new Date(targetDate)
      const diffDays = Math.floor((dDate - today) / (1000 * 60 * 60 * 24))
      
      // Events in the span of next 7 days
      if (diffDays >= 0 && diffDays <= 7) {
        upcomingEventsList.push({
          id: task.id,
          title: task.title,
          time: dDate.toLocaleDateString('vi-VN') + ' ' + dDate.toLocaleTimeString('vi-VN', {hour: '2-digit', minute:'2-digit'}),
          color: task.priority === 'High' ? '#ef4444' : (task.priority === 'Medium' ? '#f59e0b' : '#3b82f6'),
          realDate: dDate
        })
      }
    }
  })

  upcomingEventsList.sort((a, b) => a.realDate - b.realDate)
  events.value = upcomingEventsList

  chartData.value = {
    days: days,
    created: createdCounts,
    completed: completedCounts
  }

  // Re-render chart with new data
  initChart()
}

const initChart = () => {
  if (chartRef.value) {
    chartInstance = echarts.init(chartRef.value)
    
    let textColor = '#64748b'
    let splitLineColor = '#e2e8f0'
    let isDark = document.documentElement.classList.contains('dark')

    if (isDark) {
      textColor = '#94a3b8'
      splitLineColor = '#334155'
    }

    const option = {
      tooltip: { trigger: 'axis' },
      legend: { 
        data: ['Tasks Created', 'Tasks Completed'],
        bottom: 0,
        textStyle: { color: textColor }
      },
      grid: { left: '3%', right: '4%', bottom: '15%', top: '5%', containLabel: true },
      xAxis: {
        type: 'category',
        boundaryGap: false,
        data: chartData.value.days,
        axisLabel: { color: textColor },
        axisLine: { lineStyle: { color: splitLineColor } }
      },
      yAxis: {
        type: 'value',
        axisLabel: { color: textColor },
        splitLine: { lineStyle: { color: splitLineColor, type: 'dashed' } },
        minInterval: 1
      },
      series: [
        {
          name: 'Tasks Created',
          type: 'line',
          smooth: true,
          itemStyle: { color: '#3b82f6' },
          areaStyle: {
            color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
              { offset: 0, color: 'rgba(59, 130, 246, 0.3)' },
              { offset: 1, color: 'rgba(59, 130, 246, 0.05)' }
            ])
          },
          data: chartData.value.created
        },
        {
          name: 'Tasks Completed',
          type: 'line',
          smooth: true,
          itemStyle: { color: '#10b981' }, // green for completed
          areaStyle: {
            color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
              { offset: 0, color: 'rgba(16, 185, 129, 0.3)' },
              { offset: 1, color: 'rgba(16, 185, 129, 0.05)' }
            ])
          },
          data: chartData.value.completed
        }
      ]
    }
    chartInstance.setOption(option)
  }
}

const handleResize = () => {
  if (chartInstance) chartInstance.resize()
}

const observer = new MutationObserver((mutations) => {
  mutations.forEach((mutation) => {
    if (mutation.attributeName === 'class' && chartInstance) {
      initChart()
    }
  })
})

onMounted(() => {
  fetchSpaces()
  setTimeout(() => {
    initChart()
    window.addEventListener('resize', handleResize)
    observer.observe(document.documentElement, { attributes: true })
  }, 300)
})

onBeforeUnmount(() => {
  window.removeEventListener('resize', handleResize)
  observer.disconnect()
  if (chartInstance) chartInstance.dispose()
})
</script>

<style scoped>
/* Dashboard Specific Layout */
.dashboard-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 32px;
}

.dashboard-header h1 {
  font-size: 28px;
  font-weight: 700;
  margin: 0 0 8px 0;
  color: var(--text-primary);
}

.btn-primary {
  background-color: #3b82f6;
  color: white;
  border: none;
  padding: 10px 20px;
  border-radius: 8px;
  font-weight: 600;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 8px;
  transition: all 0.2s;
}
.btn-primary:hover {
  background-color: #2563eb;
}

/* Stats Cards */
.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
  gap: 24px;
  margin-bottom: 32px;
}

.stat-card {
  background: var(--bg-card);
  border-radius: 24px;
  padding: 24px;
  border: 1px solid var(--border-color);
  box-shadow: 0 4px 20px rgba(0,0,0,0.03);
  transition: transform 0.2s;
}

.dark .stat-card { box-shadow: 0 4px 20px rgba(0,0,0,0.2); }
.stat-card:hover { transform: translateY(-3px); }

.stat-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
}

.stat-title {
  color: var(--text-muted);
  font-size: 14px;
  font-weight: 500;
}

.stat-icon {
  width: 40px;
  height: 40px;
  border-radius: 12px;
  background-color: var(--bg-layout);
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--text-primary);
  font-size: 18px;
}

.stat-value {
  font-size: 28px;
  font-weight: 700;
  color: var(--text-primary);
  margin: 0 0 8px 0;
}

.stat-trend {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 13px;
  font-weight: 600;
}
.stat-trend.positive { color: #10b981; }
.stat-trend.negative { color: #ef4444; }
.trend-text { color: var(--text-muted); font-weight: 400; }

/* Dashboard Grid Component */
.dashboard-grid {
  display: grid;
  grid-template-columns: 2fr 1fr;
  gap: 24px;
  margin-bottom: 32px;
}

@media (max-width: 1024px) {
  .dashboard-grid {
    grid-template-columns: 1fr;
  }
}

.dashboard-panel {
  background: var(--bg-card);
  border-radius: 24px;
  padding: 24px;
  border: 1px solid var(--border-color);
  box-shadow: 0 4px 20px rgba(0,0,0,0.03);
}
.dark .dashboard-panel { box-shadow: 0 4px 20px rgba(0,0,0,0.2); }

.panel-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
}

.panel-header h3 {
  margin: 0;
  font-size: 18px;
  font-weight: 600;
}

.action-select {
  background: var(--bg-layout);
  border: 1px solid var(--border-color);
  color: var(--text-primary);
  padding: 6px 12px;
  border-radius: 8px;
  font-size: 13px;
  outline: none;
}

.chart-container {
  width: 100%;
  height: 300px;
}

/* Schedule Section */
.schedule-nav {
  display: flex;
  gap: 8px;
}
.nav-btn {
  background: var(--bg-layout);
  border: 1px solid var(--border-color);
  color: var(--text-primary);
  width: 32px;
  height: 32px;
  border-radius: 50%;
  cursor: pointer;
  transition: all 0.2s;
}
.nav-btn:hover { background: var(--hover-bg); }

.weekly-calendar {
  display: flex;
  justify-content: space-between;
  margin-bottom: 24px;
}

.day {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
  padding: 12px 8px;
  border-radius: 16px;
  cursor: pointer;
  transition: all 0.2s;
}

.day:hover { background: var(--bg-layout); }

.day.active {
  background: #3b82f6;
}

.day.active .day-name, .day.active .day-num {
  color: white;
}

.day-name { font-size: 12px; color: var(--text-muted); font-weight: 500; }
.day-num { font-size: 16px; color: var(--text-primary); font-weight: 700; }

.events-list {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.event-item {
  display: flex;
  align-items: flex-start;
  gap: 16px;
  padding: 12px;
  border-radius: 12px;
  background: var(--bg-layout);
  border: 1px solid transparent;
  transition: border-color 0.2s;
}
.event-item:hover { border-color: var(--border-color); }

.event-dot {
  width: 12px;
  height: 12px;
  border-radius: 50%;
  margin-top: 4px;
}

.event-info h4 { margin: 0 0 4px 0; font-size: 14px; font-weight: 600; color: var(--text-primary); }
.event-time { font-size: 12px; color: var(--text-muted); }

.empty-events {
  text-align: center;
  padding: 20px;
  color: var(--text-muted);
  font-size: 14px;
}

/* Recent Spaces Grid */
.section-header h3 { font-size: 20px; font-weight: 600; margin-bottom: 20px; }

.recent-spaces-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 20px;
}

.space-card {
  display: flex;
  align-items: center;
  padding: 20px;
  background: var(--bg-card);
  border: 1px solid var(--border-color);
  border-radius: 16px;
  cursor: pointer;
  transition: all 0.2s;
}

.space-card:hover {
  transform: translateY(-2px);
  border-color: #3b82f6;
  box-shadow: 0 6px 16px rgba(59, 130, 246, 0.1);
}


.space-details h4 { margin: 0 0 4px 0; font-size: 16px; font-weight: 600; }
.space-details > span { font-size: 13px; color: var(--text-muted); display: block; margin-bottom: 6px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; max-width: 180px; }

.space-meta {
  display: flex;
  gap: 12px;
  margin-top: 4px;
}

.meta-item {
  font-size: 12px;
  color: var(--text-muted);
  display: flex;
  align-items: center;
  gap: 4px;
}

.meta-item.active-status {
  color: #10b981;
}

.meta-item.archived-status {
  color: #94a3b8;
}

.space-icon-box {
  width: 48px;
  height: 48px;
  min-width: 48px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-size: 20px;
  margin-right: 16px;
}

.space-arrow {
  margin-left: auto;
  color: var(--text-muted);
}
.space-card:hover .space-arrow { color: #3b82f6; }

/* Non-member project cards */
.space-card.not-member {
  opacity: 0.7;
  border-style: dashed;
}
.space-card.not-member:hover {
  opacity: 0.85;
  border-color: #f59e0b;
  box-shadow: 0 6px 16px rgba(245, 158, 11, 0.1);
}
.join-badge {
  margin-left: auto;
  background: linear-gradient(135deg, #f59e0b, #ef4444);
  color: white;
  padding: 4px 12px;
  border-radius: 16px;
  font-size: 12px;
  font-weight: 600;
  white-space: nowrap;
}

.loading-state {
  text-align: center;
  color: var(--text-muted);
  padding: 20px;
}

.empty-state {
  text-align: center;
  color: var(--text-muted);
  padding: 40px;
}
.empty-state i {
  font-size: 32px;
  margin-bottom: 12px;
  display: block;
  opacity: 0.5;
}
</style>
