<template>
  <div class="goal-progress-chart-container">
    <div class="chart-header">
      <div class="chart-title">
        <h4 style="margin: 0 0 8px 0; font-size: 14px; font-weight: 600; color: #172B4D;">Theo dõi tiến độ hướng tới mục tiêu của bạn</h4>
        <p style="margin: 0 0 0 0; font-size: 14px; color: #5E6C84; line-height: 1.5;">Biểu đồ mô phỏng lịch sử tiến độ (% hoàn thành) theo thời gian.</p>
      </div>
      <div class="chart-filters">
        <button class="filter-btn" :class="{ active: timeRange === 'hour' }" @click="setTimeRange('hour')">Giờ</button>
        <button class="filter-btn" :class="{ active: timeRange === 'day' }" @click="setTimeRange('day')">Ngày</button>
        <button class="filter-btn" :class="{ active: timeRange === 'month' }" @click="setTimeRange('month')">Tháng</button>
        <button class="filter-btn" :class="{ active: timeRange === 'year' }" @click="setTimeRange('year')">Năm</button>
      </div>
    </div>
    <div class="chart-body">
      <apexchart type="line" height="250" :options="chartOptions" :series="series"></apexchart>
    </div>
  </div>
</template>

<script setup>
import { ref, watch, computed } from 'vue'
import Apexchart from 'vue3-apexcharts'

const props = defineProps({
  goal: {
    type: Object,
    required: true
  }
})

const timeRange = ref('day')

// Helper to generate mock historical data based on current progress and created date
const generateMockData = (range) => {
  const currentProgress = props.goal.progress || 0
  const createdAt = new Date(props.goal.createdAt || Date.now() - 30 * 24 * 60 * 60 * 1000)
  const now = new Date()
  
  const data = []
  
  if (range === 'hour') {
    // Generate data for the last 24 hours
    for (let i = 24; i >= 0; i--) {
      const t = new Date(now.getTime() - i * 60 * 60 * 1000)
      // linear interpolation for mock
      const val = Math.max(0, currentProgress - (i * (currentProgress / 24)))
      data.push([t.getTime(), Math.round(val)])
    }
  } else if (range === 'day') {
    // Generate data for the last 30 days
    const daysDiff = Math.max(1, Math.floor((now - createdAt) / (1000 * 60 * 60 * 24)))
    const dataPoints = Math.min(30, daysDiff)
    for (let i = dataPoints; i >= 0; i--) {
      const t = new Date(now.getTime() - i * 24 * 60 * 60 * 1000)
      const val = Math.max(0, currentProgress - (i * (currentProgress / Math.max(1, dataPoints))))
      data.push([t.getTime(), Math.round(val)])
    }
  } else if (range === 'month') {
    // Generate data for the last 12 months
    for (let i = 11; i >= 0; i--) {
      const t = new Date(now.getFullYear(), now.getMonth() - i, 1)
      const val = Math.max(0, currentProgress - (i * (currentProgress / 12)))
      data.push([t.getTime(), Math.round(val)])
    }
  } else if (range === 'year') {
    // Generate data for the last 3 years
    for (let i = 2; i >= 0; i--) {
      const t = new Date(now.getFullYear() - i, 0, 1)
      const val = Math.max(0, currentProgress - (i * (currentProgress / 3)))
      data.push([t.getTime(), Math.round(val)])
    }
  }
  
  // Ensure the last point is exactly the current progress
  if (data.length > 0) {
    data[data.length - 1][1] = currentProgress
  }
  
  return data
}

const series = ref([{
  name: 'Tiến độ (%)',
  data: generateMockData(timeRange.value)
}])

const setTimeRange = (range) => {
  timeRange.value = range
  series.value = [{
    name: 'Tiến độ (%)',
    data: generateMockData(range)
  }]
}

// Re-generate if goal progress changes
watch(() => props.goal.progress, () => {
  setTimeRange(timeRange.value)
})

const chartOptions = computed(() => {
  let format = 'dd MMM'
  if (timeRange.value === 'hour') format = 'HH:mm'
  if (timeRange.value === 'month') format = 'MMM yyyy'
  if (timeRange.value === 'year') format = 'yyyy'

  return {
    chart: {
      type: 'line',
      zoom: { enabled: false },
      toolbar: { show: false },
      fontFamily: 'Inter, sans-serif',
      animations: {
        enabled: true,
        easing: 'easeinout',
        speed: 800
      }
    },
    colors: ['#0052CC'],
    dataLabels: { enabled: false },
    stroke: {
      curve: 'smooth',
      width: 3
    },
    xaxis: {
      type: 'datetime',
      labels: {
        datetimeUTC: false,
        format: format,
        style: { colors: '#6B778C', fontSize: '12px' }
      },
      axisBorder: { show: false },
      axisTicks: { show: false }
    },
    yaxis: {
      min: 0,
      max: 100,
      tickAmount: 5,
      labels: {
        formatter: (val) => val + '%',
        style: { colors: '#6B778C', fontSize: '12px' }
      }
    },
    grid: {
      borderColor: '#DFE1E6',
      strokeDashArray: 4,
      xaxis: { lines: { show: true } },
      yaxis: { lines: { show: true } },
      padding: { top: 0, right: 0, bottom: 0, left: 10 }
    },
    markers: {
      size: 4,
      colors: ['#FFFFFF'],
      strokeColors: '#0052CC',
      strokeWidth: 2,
      hover: { size: 6 }
    },
    tooltip: {
      theme: 'light',
      x: { format: format },
      y: { formatter: (val) => val + '%' }
    }
  }
})
</script>

<style scoped>
.goal-progress-chart-container {
  border: 1px solid #DFE1E6;
  border-radius: 3px;
  padding: 24px;
  background: white;
}

.chart-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 24px;
}

.chart-filters {
  display: flex;
  background: #FAFBFC;
  border: 1px solid #DFE1E6;
  border-radius: 3px;
  overflow: hidden;
}

.filter-btn {
  background: transparent;
  border: none;
  padding: 6px 12px;
  font-size: 13px;
  color: #42526E;
  cursor: pointer;
  border-right: 1px solid #DFE1E6;
  font-weight: 500;
  transition: all 0.2s;
}

.filter-btn:last-child {
  border-right: none;
}

.filter-btn:hover {
  background: #EBECF0;
}

.filter-btn.active {
  background: #DEEBFF;
  color: #0052CC;
  font-weight: 600;
}

.chart-body {
  position: relative;
}
</style>
