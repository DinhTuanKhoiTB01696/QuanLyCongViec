<template>
  <div class="status-page">
    <div class="page-header">
      <div class="header-main">
        <h1>Cập nhật trạng thái</h1>
        <select class="secondary-btn" v-model="rangeDays">
          <option :value="7">7 ngay</option>
          <option :value="14">14 ngay</option>
          <option :value="30">30 ngay</option>
          <option :value="90">90 ngay</option>
        </select>
      </div>
      <div class="tabs">
        <button class="tab-btn" :class="{ active: currentTab === 'projects' }" @click="currentTab = 'projects'">Projects</button>
        <button class="tab-btn" :class="{ active: currentTab === 'goals' }" @click="currentTab = 'goals'">Goals</button>
      </div>
    </div>

    <div class="page-content">
      <div class="main-column">
        <div class="timeline-header">
          <button class="icon-btn" @click="shiftRange(-1)"><i class="fa-solid fa-arrow-left"></i></button>
          <h2 class="timeline-title">{{ rangeLabel }}</h2>
          <button class="icon-btn" @click="shiftRange(1)"><i class="fa-solid fa-arrow-right"></i></button>
        </div>

        <p class="stats-subtitle">Ban dang theo doi {{ filteredItems.length }} {{ itemType }} trong khoang thoi gian nay.</p>

        <div class="stats-grid">
          <div class="stat-card" v-for="stat in stats" :key="stat.key">
            <div class="stat-number" :class="stat.color">{{ stat.count }}</div>
            <div class="stat-info">
              <div class="stat-label" :class="stat.color">{{ stat.label }}</div>
              <div class="stat-desc">{{ stat.description }}</div>
            </div>
          </div>
        </div>

        <div class="tracked-items-list" v-if="!isLoading">
          <div class="tracked-item" v-for="item in filteredItems" :key="item.id">
            <div class="item-header">
              <span class="item-icon"><i :class="currentTab === 'projects' ? 'fa-solid fa-rocket' : 'fa-solid fa-bullseye'"></i></span>
              <div class="item-name-col">
                <span class="item-type-label">{{ itemTypeCapitalized }}</span>
                <span class="item-name">{{ item.name || item.title }}</span>
              </div>
            </div>
            <div class="item-card-inner">
              <div class="item-body">
                <div class="item-user">
                  <div class="item-avatar">{{ getInitials(item.ownerName || 'U') }}</div>
                  <div class="item-user-info">
                    <span class="item-user-name">{{ item.ownerName || 'Chưa gắn' }}</span>
                    <span class="item-time">{{ formatDate(item.updatedAt || item.createdAt) }}</span>
                  </div>
                </div>
                <div class="item-status-badge" :class="statusColor(item.status)">{{ normalizeStatus(item.status) }}</div>
              </div>
              <div class="item-message">Tiến độ hiện tại: {{ item.progress ?? (item.isArchived ? 100 : 0) }}%</div>
            </div>
          </div>

          <div v-if="filteredItems.length === 0" class="empty-state">Không có dữ liệu trạng thái trong khoảng thời gian này.</div>
        </div>
        <div v-else class="empty-state">Đang tải dữ liệu...</div>
      </div>

      <div class="sidebar-column">
        <div class="sidebar-section">
          <h3>{{ itemTypeCapitalized }} moi</h3>
          <div class="sidebar-item" v-for="item in newestItems" :key="item.id">
            <div class="sidebar-item-left">
              <span class="sidebar-item-icon"><i :class="currentTab === 'projects' ? 'fa-solid fa-rocket' : 'fa-solid fa-bullseye'"></i></span>
              <span class="sidebar-item-name">{{ item.name || item.title }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, onMounted, ref, watch } from 'vue'
import axiosClient from '@/api/axiosClient'

const currentTab = ref('projects')
const rangeDays = ref(7)
const rangeOffset = ref(0)
const dashboardItems = ref([])
const dashboardStats = ref(null)
const isLoading = ref(false)

const itemType = computed(() => currentTab.value === 'projects' ? 'du an' : 'muc tieu')
const itemTypeCapitalized = computed(() => currentTab.value === 'projects' ? 'Dự án' : 'Mục tiêu')

const rangeStart = computed(() => {
  const date = new Date()
  date.setDate(date.getDate() - rangeDays.value * (rangeOffset.value + 1))
  return date
})
const rangeEnd = computed(() => {
  const date = new Date()
  date.setDate(date.getDate() - rangeDays.value * rangeOffset.value)
  return date
})
const rangeLabel = computed(() => `${rangeStart.value.toLocaleDateString('vi-VN')} - ${rangeEnd.value.toLocaleDateString('vi-VN')}`)

const filteredItems = computed(() => dashboardItems.value || [])

const stats = computed(() => {
  const serverStats = dashboardStats.value || {}
  return [
    { key: 'newest', label: 'Moi tao', color: 'green', count: serverStats.newest || 0, description: 'Trong 7 ngay gan day' },
    { key: 'updated', label: 'Moi cap nhat', color: 'blue', count: serverStats.updated || 0, description: 'Co thay doi gan day' },
    { key: 'risk', label: 'Co rui ro', color: 'orange', count: serverStats.risk || 0, description: 'Can chu y' },
    { key: 'pending', label: 'Đang chờ xử lý', color: 'gray', count: serverStats.pending || 0, description: 'Chưa có cập nhật rõ' },
    { key: 'done', label: 'Đã hoàn tất', color: 'blue', count: serverStats.done || 0, description: 'Đã kết thúc' }
  ]
})

const newestItems = computed(() => [...(dashboardItems.value || [])]
  .sort((a, b) => new Date(b.createdAt || 0) - new Date(a.createdAt || 0))
  .slice(0, 5))

const normalizeStatus = (status) => {
  if (status === true) return 'Đang thực hiện'
  if (status === false) return 'Đã lưu trữ'
  return status || 'Đang chờ xử lý'
}

const statusColor = (status) => {
  const normalized = normalizeStatus(status).toLowerCase()
  if (normalized.includes('risk') || normalized.includes('rui ro')) return 'orange-badge'
  if (normalized.includes('off') || normalized.includes('khong')) return 'red-badge'
  if (normalized.includes('complete') || normalized.includes('hoan') || normalized.includes('done')) return 'blue-badge'
  return 'green-badge'
}

const getInitials = (value = '') => value.trim().split(/\s+/).slice(0, 2).map(part => part[0]).join('').toUpperCase() || 'U'
const formatDate = (value) => value ? new Date(value).toLocaleString('vi-VN') : ''
const shiftRange = (direction) => { rangeOffset.value = Math.max(0, rangeOffset.value + direction) }

const fetchStatusDashboard = async () => {
  isLoading.value = true
  try {
    const res = await axiosClient.get('/status-dashboard', {
      params: { type: currentTab.value, days: rangeDays.value }
    })
    dashboardItems.value = res.data?.data?.items || []
    dashboardStats.value = res.data?.data?.stats || null
  } catch (err) {
    console.error('Failed to load status dashboard', err)
    dashboardItems.value = []
    dashboardStats.value = null
  } finally {
    isLoading.value = false
  }
}

watch([rangeDays, currentTab], () => {
  rangeOffset.value = 0
  fetchStatusDashboard()
})

onMounted(fetchStatusDashboard)
</script>

<style scoped>
.status-page { color: #172B4D; background: #fff; min-height: 100vh; }
.page-header { padding: 32px 40px 0; border-bottom: 1px solid #DFE1E6; }
.header-main { display: flex; justify-content: space-between; align-items: center; margin-bottom: 24px; }
.header-main h1 { font-size: 24px; font-weight: 500; margin: 0; }
.secondary-btn { background: #F4F5F7; border: 0; padding: 6px 12px; border-radius: 3px; color: #42526E; }
.tabs { display: flex; gap: 24px; }
.tab-btn { background: transparent; border: 0; padding: 0 0 12px; color: #5E6C84; cursor: pointer; }
.tab-btn.active { color: #0052CC; border-bottom: 2px solid #0052CC; }
.page-content { display: flex; padding: 32px 40px; gap: 40px; }
.main-column { flex: 1; max-width: 850px; }
.timeline-header { display: flex; justify-content: center; align-items: center; gap: 24px; margin-bottom: 24px; }
.timeline-title { font-size: 20px; font-weight: 500; color: #5E6C84; margin: 0; }
.icon-btn { border: 1px solid #DFE1E6; background: transparent; border-radius: 3px; width: 32px; height: 32px; cursor: pointer; }
.stats-subtitle { font-size: 14px; font-weight: 600; margin: 0 0 16px; }
.stats-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(180px, 1fr)); gap: 16px; margin-bottom: 40px; }
.stat-card { border: 1px solid #DFE1E6; border-radius: 3px; padding: 12px 16px; display: flex; gap: 12px; }
.stat-number { font-size: 24px; font-weight: 600; }
.green { color: #00875A; } .orange { color: #FF991F; } .red { color: #DE350B; } .gray { color: #5E6C84; } .blue { color: #0052CC; }
.stat-label { font-size: 12px; font-weight: 700; }
.stat-desc { font-size: 11px; color: #5E6C84; }
.tracked-items-list { display: flex; flex-direction: column; gap: 16px; }
.tracked-item { display: flex; flex-direction: column; gap: 8px; }
.item-header, .item-user, .sidebar-item-left { display: flex; align-items: center; gap: 8px; }
.item-name-col, .item-user-info { display: flex; flex-direction: column; }
.item-type-label, .item-time { color: #5E6C84; font-size: 12px; }
.item-name { font-weight: 600; }
.item-card-inner { border: 1px solid #DFE1E6; border-radius: 3px; padding: 16px; margin-left: 24px; display: flex; flex-direction: column; gap: 12px; }
.item-body { display: flex; justify-content: space-between; align-items: center; }
.item-avatar { width: 32px; height: 32px; border-radius: 50%; background: #00875A; color: white; display: flex; align-items: center; justify-content: center; font-size: 12px; font-weight: 700; }
.item-status-badge { font-size: 11px; font-weight: 700; padding: 2px 6px; border-radius: 3px; }
.green-badge { background: #E3FCEF; color: #006644; } .orange-badge { background: #FFFAE6; color: #974F0C; } .red-badge { background: #FFEBE6; color: #BF2600; } .blue-badge { background: #DEEBFF; color: #0052CC; }
.item-message { background: #FAFBFC; border: 1px solid #DFE1E6; border-radius: 3px; padding: 12px; font-size: 14px; }
.sidebar-column { width: 300px; border-left: 1px solid #DFE1E6; padding-left: 32px; }
.sidebar-section { display: flex; flex-direction: column; gap: 12px; }
.sidebar-section h3 { margin: 0; font-size: 14px; }
.sidebar-item { display: flex; justify-content: space-between; align-items: center; }
.sidebar-item-name { font-size: 14px; }
.empty-state { padding: 32px; text-align: center; color: #5E6C84; border: 1px dashed #DFE1E6; border-radius: 3px; }
</style>
