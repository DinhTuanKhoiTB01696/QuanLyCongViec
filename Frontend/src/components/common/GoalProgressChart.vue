<template>
  <div class="goal-progress-chart-container">
    <div class="chart-header">
      <div class="chart-title">
        <h4>Theo dõi tiến độ hướng tới mục tiêu của bạn</h4>
        <p>Lịch sử tiến độ được tạo từ các bản cập nhật mục tiêu thật.</p>
      </div>
      <div class="chart-filters" v-if="hasProgressHistory">
        <button class="filter-btn" :class="{ active: timeRange === 'hour' }" @click="setTimeRange('hour')">Giờ</button>
        <button class="filter-btn" :class="{ active: timeRange === 'day' }" @click="setTimeRange('day')">Ngày</button>
        <button class="filter-btn" :class="{ active: timeRange === 'month' }" @click="setTimeRange('month')">Tháng</button>
        <button class="filter-btn" :class="{ active: timeRange === 'year' }" @click="setTimeRange('year')">Năm</button>
      </div>
    </div>

    <div class="chart-empty-state" v-if="!hasProgressHistory">
      <div class="empty-icon"><i class="fa-solid fa-chart-line"></i></div>
      <h4>Chưa có dữ liệu tiến độ</h4>
      <p>Hãy đăng cập nhật mục tiêu để tạo lịch sử tiến độ thật.</p>
    </div>

    <div class="chart-body" v-else>
      <apexchart type="line" height="250" :options="chartOptions" :series="series"></apexchart>
    </div>
  </div>
</template>

<script setup>
import { computed, ref, watch } from 'vue'
import Apexchart from 'vue3-apexcharts'

const props = defineProps({
  goal: {
    type: Object,
    required: true
  }
})

const timeRange = ref('day')

const getUpdateProgress = (update) => {
  const value = update?.newProgress ?? update?.progress ?? update?.NewProgress ?? update?.Progress
  const number = Number(value)
  return Number.isFinite(number) ? Math.max(0, Math.min(100, number)) : null
}

const getUpdateTimestamp = (update) => {
  const value = update?.createdAt || update?.updatedAt || update?.CreatedAt || update?.UpdatedAt
  const date = value ? new Date(value) : null
  return date && !Number.isNaN(date.getTime()) ? date.getTime() : null
}

const progressHistory = computed(() => {
  const updates = Array.isArray(props.goal?.updates)
    ? props.goal.updates
    : Array.isArray(props.goal?.Updates)
      ? props.goal.Updates
      : []

  return updates
    .map(update => {
      const progress = getUpdateProgress(update)
      const timestamp = getUpdateTimestamp(update)
      return progress === null || timestamp === null ? null : [timestamp, progress]
    })
    .filter(Boolean)
    .sort((left, right) => left[0] - right[0])
})

const hasProgressHistory = computed(() => progressHistory.value.length > 0)

const filterProgressHistory = (range) => {
  const points = progressHistory.value
  if (!points.length) return []

  const now = Date.now()
  const windowMs = {
    hour: 24 * 60 * 60 * 1000,
    day: 30 * 24 * 60 * 60 * 1000,
    month: 366 * 24 * 60 * 60 * 1000,
    year: 3 * 366 * 24 * 60 * 60 * 1000
  }[range]

  const filtered = points.filter(point => now - point[0] <= windowMs)
  return filtered.length ? filtered : points
}

const series = computed(() => ([{
  name: 'Tiến độ (%)',
  data: filterProgressHistory(timeRange.value)
}]))

const setTimeRange = (range) => {
  timeRange.value = range
}

watch(progressHistory, () => {
  if (!hasProgressHistory.value) {
    timeRange.value = 'day'
  }
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
        format,
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
        formatter: (val) => `${val}%`,
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
      x: { format },
      y: { formatter: (val) => `${val}%` }
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
  gap: 16px;
}

.chart-title h4 {
  margin: 0 0 8px 0;
  font-size: 14px;
  font-weight: 600;
  color: #172B4D;
}

.chart-title p {
  margin: 0;
  font-size: 14px;
  color: #5E6C84;
  line-height: 1.5;
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

.chart-empty-state {
  display: grid;
  justify-items: center;
  gap: 8px;
  padding: 32px 16px;
  text-align: center;
  border: 1px dashed #DFE1E6;
  border-radius: 6px;
  background: #FAFBFC;
}

.empty-icon {
  width: 40px;
  height: 40px;
  display: grid;
  place-items: center;
  border-radius: 50%;
  color: #0052CC;
  background: #DEEBFF;
}

.chart-empty-state h4 {
  margin: 0;
  font-size: 15px;
  color: #172B4D;
}

.chart-empty-state p {
  margin: 0;
  color: #5E6C84;
  font-size: 14px;
}
</style>
