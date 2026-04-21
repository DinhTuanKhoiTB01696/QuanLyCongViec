<template>
  <AdminLayout>
    <div class="admin-page-header">
      <div class="header-title-section">
        <div class="breadcrumb">
          <i class="fa-solid fa-file-lines"></i> System / Audit Log
        </div>
        <h1 class="page-title">{{ t('System Audit Log', 'Nhật ký Hệ thống (Audit Log)') }}</h1>
        <p class="page-subtitle">{{ t('Monitor and search important system activities and events.', 'Theo dõi và tra cứu các hoạt động, sự kiện quan trọng trong hệ thống.') }}</p>
      </div>
      <div class="header-actions">
        <el-tooltip class="box-item" effect="dark" :content="t('Auto-refresh every 10 seconds', 'Tự động làm mới dữ liệu mỗi 10 giây (Auto Refresh)')" placement="top">
          <el-switch
            v-model="isRealtime"
            inline-prompt
            active-text="Realtime"
            inactive-text="Paused"
            style="margin-right: 16px; --el-switch-on-color: #10b981; --el-switch-off-color: #27272a"
            @change="handleRealtimeToggle"
          />
        </el-tooltip>
        
        <el-tooltip effect="dark" :content="t('Search by Name, Email, Resource, or Action', 'Tìm kiếm theo Tên, Email, Tài nguyên, hoặc Hành động')" placement="top">
          <el-input v-model="searchQuery" class="glass-input" :placeholder="t('Search logs...', 'Tìm kiếm log...')" style="width: 220px; margin-right: 12px" @input="debounceSearch" clearable />
        </el-tooltip>
        
        <el-select v-model="selectedProjectId" class="glass-input" :placeholder="t('All Projects', 'Tất cả Dự án')" style="width: 180px; margin-right: 12px" @change="fetchLogs" clearable>
           <el-option v-for="p in projects" :key="p.id" :label="p.name" :value="p.id" />
        </el-select>

        <el-date-picker
          v-model="dateRange"
          type="daterange"
          range-separator="->"
          :start-placeholder="t('From Date', 'Từ ngày')"
          :end-placeholder="t('To Date', 'Đến ngày')"
          :disabled-date="disabledDate"
          style="width: 260px"
          class="glass-input custom-date-picker"
          @change="fetchLogs"
        />
      </div>
    </div>

    <div class="admin-card" v-loading="loading">
      <el-table :data="logs" style="width: 100%" class="admin-table" :show-header="true">
        <el-table-column prop="timestamp" :label="t('TIMESTAMP', 'THỜI GIAN')" min-width="150">
           <template #default="scope">
             {{ formatDateLocal(scope.row.timestamp) }}
           </template>
        </el-table-column>
        <el-table-column prop="user" :label="t('USER', 'NGƯỜI DÙNG')" min-width="180" />
        <el-table-column prop="action" :label="t('ACTION', 'HÀNH ĐỘNG')" min-width="120" />
        <el-table-column prop="resource" label="RESOURCE" min-width="260">
           <template #default="scope">
             <div style="font-weight: 500; font-size: 13px;">{{ scope.row.resource }}</div>
             <div style="color: #64748b; font-size: 12px;">{{ scope.row.targetId }}</div>
           </template>
        </el-table-column>
        <el-table-column prop="summary" :label="t('DETAILS', 'CHI TIẾT')" min-width="250">
           <template #default="scope">
             <div style="font-size: 13px; color: #44546f; line-height: 1.5" v-if="scope.row.summary">
               {{ scope.row.summary }}
             </div>
             <span v-else style="color: #94a3b8">--</span>
           </template>
        </el-table-column>
        
        <el-table-column prop="status" label="STATUS" min-width="120">
          <template #default="scope">
            <div class="status-cell">
              <span class="status-dot" :class="scope.row.status.toLowerCase()"></span>
              <span :class="scope.row.status.toLowerCase() + '-text'">{{ scope.row.status }}</span>
            </div>
          </template>
        </el-table-column>
      </el-table>

      <div class="pagination-container" v-if="total > 0">
        <el-pagination
          v-model:current-page="currentPage"
          layout="prev, pager, next"
          :total="total"
          :page-size="20"
          @current-change="handlePageChange"
          class="custom-pagination"
        />
      </div>
    </div>
  </AdminLayout>
</template>

<script setup>
import { ref, onMounted, onUnmounted, watch } from 'vue'
import AdminLayout from '@/components/layout/AdminLayout.vue'
import axiosClient from '@/api/axiosClient'
import { useLocale } from '@/composables/useLocale'

const { t, formatDateLocal } = useLocale()

const selectedProjectId = ref(null)
const searchQuery = ref('')
const dateRange = ref([])
const loading = ref(false)
const currentPage = ref(1)
const total = ref(0)
const logs = ref([])
const projects = ref([])

// Real-time polling
const isRealtime = ref(true)
let pollingInterval = null

// Giới hạn chỉ được tra cứu trong 90 ngày quá khứ
const disabledDate = (time) => {
  const today = new Date()
  const limitDate = new Date()
  limitDate.setDate(today.getDate() - 90)
  return time.getTime() > today.getTime() || time.getTime() < limitDate.getTime()
}

let searchTimeout = null
const debounceSearch = () => {
    clearTimeout(searchTimeout)
    searchTimeout = setTimeout(() => {
        currentPage.value = 1
        fetchLogs()
    }, 500)
}

const handlePageChange = (page) => {
    currentPage.value = page
    fetchLogs()
}

const fetchProjects = async () => {
    try {
        const res = await axiosClient.get('/security/accessible-projects')
        projects.value = res.data?.data?.items || []
    } catch(e) {
        console.error(e)
    }
}

const fetchLogs = async (isBackground = false) => {
    if (!isBackground) {
        loading.value = true
    }
    try {
        const params = {
            page: currentPage.value,
            limit: 20
        }
        if (dateRange.value && dateRange.value.length === 2) {
            params.startDate = dateRange.value[0].toISOString()
            params.endDate = dateRange.value[1].toISOString()
        }
        if (selectedProjectId.value) params.projectId = selectedProjectId.value
        if (searchQuery.value) params.search = searchQuery.value

        const res = await axiosClient.get('/auditlogs', { params })
        logs.value = res.data.data.items
        total.value = res.data.data.total
    } catch(e) {
        console.error(e)
    } finally {
        if (!isBackground) {
            loading.value = false
        }
    }
}

const startPolling = () => {
    if (pollingInterval) clearInterval(pollingInterval)
    pollingInterval = setInterval(() => {
        // Only auto refresh when on page 1 and no specific historical filters
        if (isRealtime.value && currentPage.value === 1) {
            fetchLogs(true) // background fetch
        }
    }, 10000)
}

const stopPolling = () => {
    if (pollingInterval) {
        clearInterval(pollingInterval)
        pollingInterval = null
    }
}

const handleRealtimeToggle = () => {
    if (isRealtime.value) {
        fetchLogs(true)
        startPolling()
    } else {
        stopPolling()
    }
}

onMounted(() => {
    fetchProjects()
    fetchLogs()
    if (isRealtime.value) {
        startPolling()
    }
})

onUnmounted(() => {
    stopPolling()
})
</script>

<style scoped>
.admin-page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
}

.breadcrumb {
  font-size: 13px;
  color: #8b949e;
  margin-bottom: 8px;
  display: flex;
  align-items: center;
  gap: 8px;
}

.page-title {
  font-size: 24px;
  font-weight: 600;
  color: var(--text-primary);
  margin: 0;
}

.page-subtitle {
  font-size: 14px;
  color: #8b949e;
  margin-top: 4px;
  margin-bottom: 0;
}
.header-actions {
  display: flex;
  align-items: center;
}

.admin-card {
  background: var(--bg-card);
  backdrop-filter: blur(12px);
  -webkit-backdrop-filter: blur(12px);
  border: 1px solid var(--border-color);
  border-radius: 12px;
  padding: 24px;
  box-shadow: 0 4px 20px rgba(0,0,0,0.05);
}

:deep(.admin-table th.el-table__cell) {
  background-color: transparent !important;
  color: var(--text-primary) !important;
  opacity: 0.8;
  font-weight: 700;
  font-size: 12px;
  text-transform: uppercase;
  border-bottom: 2px solid var(--border-color);
}

:deep(.admin-table td.el-table__cell) {
  padding: 16px 0;
  color: var(--text-primary);
  background-color: transparent !important;
  font-size: 14px;
  border-bottom: 1px solid var(--border-color);
  white-space: pre-line;
}

:deep(.el-table), :deep(.el-table__inner-wrapper), :deep(.el-table tr) {
  background-color: transparent !important;
}

.status-cell {
  display: flex;
  align-items: center;
  gap: 8px;
}

.status-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
}

.status-dot.success { background-color: #10b981; }
.success-text { color: #10b981; font-weight: 500; }

.status-dot.warning { background-color: #f59e0b; }
.warning-text { color: #f59e0b; font-weight: 500; }

.status-dot.error { background-color: #ef4444; }
.error-text { color: #ef4444; font-weight: 500; }

/* Custom Radio Group matching image */
:deep(.custom-radio-group .el-radio-button__inner) {
  border: none !important;
  background-color: var(--bg-hover);
  box-shadow: none;
  color: var(--text-primary);
  border-radius: 6px !important;
  margin-left: 8px;
  font-weight: 500;
  border: 1px solid var(--border-color) !important;
}

:deep(.custom-radio-group .el-radio-button:first-child .el-radio-button__inner) {
  margin-left: 0;
}

:deep(.custom-radio-group .el-radio-button__original-radio:checked + .el-radio-button__inner) {
  background-color: rgba(13, 148, 136, 0.2);
  color: #0d9488;
  box-shadow: none;
  border: 1px solid #0d9488 !important;
}

.pagination-container {
  display: flex;
  justify-content: center;
  margin-top: 24px;
}
</style>
