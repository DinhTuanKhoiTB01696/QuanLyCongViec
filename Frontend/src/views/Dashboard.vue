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
          <div class="event-item" v-for="event in events" :key="event.id">
            <div class="event-dot" :style="{ backgroundColor: event.color }"></div>
            <div class="event-info">
              <h4>{{ event.title }}</h4>
              <span class="event-time">{{ event.time }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Recent Spaces (Legacy Integration) -->
    <div class="recent-spaces-section">
      <div class="section-header">
        <h3>Recent Spaces</h3>
      </div>
      <div v-if="isLoading" class="loading-state">Loading spaces...</div>
      <div v-else-if="spaces.length === 0" class="empty-state">
        <i class="fa-regular fa-folder-open"></i>
        <p>No recent spaces found</p>
      </div>
      <div v-else class="recent-spaces-grid">
        <div class="space-card" v-for="space in spaces" :key="space.id" @click="goToSpace(space.id)">
          <div class="space-icon-box">
            <i class="fa-solid fa-rocket"></i>
          </div>
          <div class="space-details">
            <h4>{{ space.name }}</h4>
            <span>{{ space.type || 'Workspace' }}</span>
          </div>
          <div class="space-arrow">
            <i class="fa-solid fa-arrow-right"></i>
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

// Mock Data
const stats = ref([
  { id: 2, title: 'Active Projects', value: '124', trend: 15.1, icon: 'fa-solid fa-folder' },
  { id: 3, title: 'Tasks Completed', value: '892', trend: -4.5, icon: 'fa-solid fa-check-double' },
  { id: 4, title: 'Team Members', value: '42', trend: 12.0, icon: 'fa-solid fa-users' }
])

const weekDays = ref([
  { name: 'Mon', date: '12', active: false },
  { name: 'Tue', date: '13', active: false },
  { name: 'Wed', date: '14', active: true },
  { name: 'Thu', date: '15', active: false },
  { name: 'Fri', date: '16', active: false },
  { name: 'Sat', date: '17', active: false },
  { name: 'Sun', date: '18', active: false }
])

const events = ref([
  { id: 1, title: 'Design Review: Nexus UI', time: '10:00 AM - 11:30 AM', color: '#3b82f6' },
  { id: 2, title: 'Sprint Planning Meeting', time: '1:00 PM - 2:00 PM', color: '#8b5cf6' },
  { id: 3, title: 'Client Feedback Session', time: '3:30 PM - 4:00 PM', color: '#10b981' }
])

const goToSpace = (id) => { router.push(`/space/${id}`) }

const fetchSpaces = async () => {
  isLoading.value = true
  try {
    const response = await axiosClient.get('/projects')
    const projectList = response.data.data || response.data || []
    spaces.value = projectList
  } catch (error) {
    console.error('Fetch projects error:', error)
  } finally {
    isLoading.value = false
  }
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
        data: ['Metric A', 'Metric B'],
        bottom: 0,
        textStyle: { color: textColor }
      },
      grid: { left: '3%', right: '4%', bottom: '15%', top: '5%', containLabel: true },
      xAxis: {
        type: 'category',
        boundaryGap: false,
        data: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'],
        axisLabel: { color: textColor },
        axisLine: { lineStyle: { color: splitLineColor } }
      },
      yAxis: {
        type: 'value',
        axisLabel: { color: textColor },
        splitLine: { lineStyle: { color: splitLineColor, type: 'dashed' } }
      },
      series: [
        {
          name: 'Metric A',
          type: 'line',
          smooth: true,
          itemStyle: { color: '#3b82f6' },
          areaStyle: {
            color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
              { offset: 0, color: 'rgba(59, 130, 246, 0.3)' },
              { offset: 1, color: 'rgba(59, 130, 246, 0.05)' }
            ])
          },
          data: [120, 132, 101, 134, 90, 230, 210]
        },
        {
          name: 'Metric B',
          type: 'line',
          smooth: true,
          itemStyle: { color: '#8b5cf6' },
          data: [220, 182, 191, 234, 290, 330, 310]
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

.space-icon-box {
  width: 48px;
  height: 48px;
  background: linear-gradient(135deg, #3b82f6, #8b5cf6);
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-size: 20px;
  margin-right: 16px;
}

.space-details h4 { margin: 0 0 4px 0; font-size: 16px; font-weight: 600; }
.space-details span { font-size: 13px; color: var(--text-muted); }

.space-arrow {
  margin-left: auto;
  color: var(--text-muted);
}
.space-card:hover .space-arrow { color: #3b82f6; }
</style>
