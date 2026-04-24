<template>
  <AdminLayout>
    <div class="admin-page">
      <div class="page-header">
        <div class="breadcrumb">{{ t('SYSTEM / AUDIT LOG', 'HỆ THỐNG / NHẬT KÝ') }}</div>
        <h1 class="text-hero">{{ t('System Audit Log', 'Nhật ký Hệ thống') }}</h1>
        <p class="text-desc">{{ t('Monitor and search important system activities, security events, and administrative changes.', 'Theo dõi và tìm kiếm các hoạt động quan trọng của hệ thống, sự kiện bảo mật và thay đổi quản trị.') }}</p>
      </div>

      <div class="header-actions-row">
        <div class="search-box">
          <i class="fa-solid fa-magnifying-glass"></i>
          <input v-model="searchQuery" type="text" :placeholder="t('Search logs...', 'Tìm kiếm log...')" @input="debounceSearch" />
        </div>
        
        <div class="filter-group">
          <el-select v-model="selectedProjectId" clearable :placeholder="t('All Projects', 'Tất cả Dự án')" class="compact-select">
            <el-option v-for="p in projects" :key="p.id" :label="p.name" :value="p.id" />
          </el-select>

          <el-date-picker
            v-model="dateRange"
            type="daterange"
            range-separator="→"
            :start-placeholder="t('From', 'Từ')"
            :end-placeholder="t('To', 'Đến')"
            format="DD/MM/YYYY"
            value-format="YYYY-MM-DD"
            :disabled-date="disabledDate"
            class="compact-date-picker"
            @change="fetchLogs"
          />

          <div class="realtime-toggle">
            <el-switch v-model="isRealtime" @change="handleRealtimeToggle" />
            <span>{{ t('Realtime', 'Thời gian thực') }}</span>
          </div>
        </div>
      </div>

      <section class="settings-card no-padding">
        <div v-loading="loading">
          <el-table :data="logs" style="width: 100%" class="admin-table">
            <template #empty>
              <div class="empty-state">
                <span>{{ t('No Data', 'Không có dữ liệu') }}</span>
              </div>
            </template>
            <el-table-column prop="timestamp" :label="t('TIMESTAMP', 'THỜI GIAN')" width="180">
              <template #default="scope">
                <span class="timestamp-text">{{ formatDateLocal(scope.row.timestamp) }}</span>
              </template>
            </el-table-column>
            
            <el-table-column prop="user" :label="t('USER', 'NGƯỜI DÙNG')" width="200">
              <template #default="scope">
                <div class="user-cell-audit">
                  <div class="mini-avatar">{{ getInitials(scope.row.user) }}</div>
                  <span>{{ scope.row.user }}</span>
                </div>
              </template>
            </el-table-column>

            <el-table-column prop="action" :label="t('ACTION', 'HÀNH ĐỘNG')" width="140">
              <template #default="scope">
                <span class="action-tag">{{ scope.row.action }}</span>
              </template>
            </el-table-column>

            <el-table-column prop="resource" :label="t('RESOURCE', 'TÀI NGUYÊN')">
              <template #default="scope">
                <div class="resource-stack">
                  <strong>{{ scope.row.resource }}</strong>
                  <small>{{ scope.row.targetId }}</small>
                </div>
              </template>
            </el-table-column>

            <el-table-column prop="status" :label="t('STATUS', 'TRẠNG THÁI')" width="120">
              <template #default="scope">
                <div class="status-badge" :class="scope.row.status.toLowerCase()">
                  {{ t(scope.row.status, scope.row.status === 'SUCCESS' ? 'THÀNH CÔNG' : (scope.row.status === 'ERROR' ? 'LỖI' : scope.row.status)) }}
                </div>
              </template>
            </el-table-column>
          </el-table>

          <div class="table-footer" v-if="total > 10">
            <el-pagination
              v-model:current-page="currentPage"
              layout="prev, pager, next"
              :total="total"
              :page-size="10"
              :hide-on-single-page="true"
              @current-change="handlePageChange"
              class="compact-pagination"
            />
          </div>
        </div>
      </section>
    </div>
  </AdminLayout>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
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
const isRealtime = ref(true)
let pollingInterval = null

const disabledDate = (time) => {
  const today = new Date()
  const limitDate = new Date()
  limitDate.setDate(today.getDate() - 90)
  return time.getTime() > today.getTime() || time.getTime() < limitDate.getTime()
}

const getInitials = (name) => (name || 'U').charAt(0).toUpperCase()

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
    if (!isBackground) loading.value = true
    try {
        const params = { page: currentPage.value, limit: 10 }
        if (dateRange.value?.length === 2) {
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
        if (!isBackground) loading.value = false
    }
}

const handleRealtimeToggle = () => {
    if (isRealtime.value) fetchLogs(true)
}

onMounted(() => {
    fetchProjects()
    fetchLogs()
    pollingInterval = setInterval(() => {
        if (isRealtime.value && currentPage.value === 1) fetchLogs(true)
    }, 10000)
})

onUnmounted(() => {
    if (pollingInterval) clearInterval(pollingInterval)
})
</script>

<style scoped>
.breadcrumb {
  font-size: 11px;
  font-weight: 800;
  letter-spacing: 0.05em;
  color: var(--color-accent);
  margin-bottom: 12px;
  text-transform: uppercase;
}

.header-actions-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 20px;
  margin-bottom: 24px;
  flex-wrap: wrap;
}

.search-box {
  flex: 1;
  max-width: 400px;
  position: relative;
  display: flex;
  align-items: center;
}

.search-box i {
  position: absolute;
  left: 12px;
  color: var(--color-text-muted);
  font-size: 14px;
}

.search-box input {
  width: 100%;
  padding: 8px 12px 8px 42px !important;
  background: var(--input-bg);
  border: 1px solid var(--border-color);
  border-radius: 8px !important;
  color: var(--text-primary);
  outline: none;
  box-sizing: border-box;
}

::v-deep(.compact-date-picker),
::v-deep(.compact-date-picker.el-date-editor),
::v-deep(.compact-date-picker .el-input__wrapper) {
  width: 360px !important;
  min-width: 360px !important;
  flex: 0 0 auto !important;
}

::v-deep(.el-date-editor) {
  border-radius: 8px !important;
  background-color: var(--input-bg) !important;
  box-shadow: 0 0 0 1px var(--border-color) inset !important;
}
::v-deep(.el-range-input) {
  color: var(--text-primary) !important;
}
::v-deep(.el-range-separator) {
  color: var(--text-muted) !important;
}

::v-deep(.compact-select .el-input__wrapper) {
  border-radius: 8px !important;
  background-color: var(--input-bg) !important;
  box-shadow: 0 0 0 1px var(--border-color) inset !important;
}
::v-deep(.compact-select .el-input__inner) {
  color: var(--text-primary) !important;
}

.filter-group {
  display: flex;
  align-items: center;
  gap: 12px;
  flex-wrap: wrap;
}

::v-deep(.admin-table th.el-table__cell) {
  background-color: transparent !important;
  color: var(--text-primary) !important;
  opacity: 0.8;
  font-weight: 700;
  font-size: 12px;
  text-transform: uppercase;
  border-bottom: 2px solid var(--border-color);
}

::v-deep(.admin-table td.el-table__cell) {
  padding: 16px 0;
  color: var(--text-primary);
}

::v-deep(.el-table), ::v-deep(.el-table__inner-wrapper), ::v-deep(.el-table tr) {
  background-color: transparent !important;
}

.realtime-toggle {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 13px;
  font-weight: 600;
  color: var(--text-secondary);
}

.no-padding { padding: 0 !important; }

.user-cell-audit {
  display: flex;
  align-items: center;
  gap: 10px;
}

.mini-avatar {
  width: 24px; height: 24px;
  border-radius: 50%;
  background: var(--color-accent);
  color: #fff;
  display: flex; align-items: center; justify-content: center;
  font-size: 11px; font-weight: 700;
}

.timestamp-text { font-size: 13px; color: var(--text-secondary); }

.status-dot.warning { background-color: #f59e0b; }
.warning-text { color: #f59e0b; font-weight: 500; }

.status-dot.error { background-color: #ef4444; }
.error-text { color: #ef4444; font-weight: 500; }

/* Custom Radio Group matching image */
::v-deep(.custom-radio-group .el-radio-button__inner) {
  border: none !important;
  background-color: var(--bg-hover);
  box-shadow: none;
  color: var(--text-primary);
  border-radius: 6px !important;
  margin-left: 8px;
  font-weight: 500;
  border: 1px solid var(--border-color) !important;
}

::v-deep(.custom-radio-group .el-radio-button:first-child .el-radio-button__inner) {
  margin-left: 0;
}

::v-deep(.custom-radio-group .el-radio-button__original-radio:checked + .el-radio-button__inner) {
  background-color: rgba(13, 148, 136, 0.2);
  color: #0d9488;
  box-shadow: none;
  border: 1px solid #0d9488 !important;
}
.action-tag {
  font-size: 11px;
  font-weight: 700;
  text-transform: uppercase;
  color: var(--color-accent);
  background: color-mix(in srgb, var(--color-accent) 15%, transparent);
  padding: 2px 8px;
  border-radius: 2px;
}

.resource-stack { display: flex; flex-direction: column; gap: 2px; }
.resource-stack strong { font-size: 14px; color: var(--text-primary); }
.resource-stack small { font-size: 11px; color: var(--text-secondary); opacity: 0.7; }

.status-badge {
  display: inline-block;
  padding: 2px 8px;
  border-radius: 2px;
  font-size: 10px;
  font-weight: 800;
  text-transform: uppercase;
}

.status-badge.success { background: #10b98120; color: #10b981; }
.status-badge.warning { background: #f59e0b20; color: #f59e0b; }
.status-badge.error { background: #ef444420; color: #ef4444; }

.table-footer {
  padding: 16px;
  display: flex;
  justify-content: center;
  border-top: 1px solid var(--border-color);
}

::v-deep(.el-pagination button) {
  background-color: var(--bg-hover) !important;
  color: var(--text-primary) !important;
  border-radius: 6px !important;
  margin: 0 4px;
}
::v-deep(.el-pagination button:disabled) {
  opacity: 0.5;
  cursor: not-allowed;
}
::v-deep(.el-pagination .el-pager li) {
  background-color: var(--bg-hover) !important;
  color: var(--text-primary) !important;
  border-radius: 6px !important;
  margin: 0 4px;
}
::v-deep(.el-pagination .el-pager li.is-active) {
  background-color: rgba(13, 148, 136, 0.2) !important;
  color: #0d9488 !important;
  border: 1px solid #0d9488 !important;
}

@media (max-width: 1000px) {
  .header-actions-row { flex-direction: column; align-items: stretch; }
  .search-box { max-width: 100%; }
}
</style>
