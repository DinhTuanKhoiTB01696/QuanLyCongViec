<template>
  <AdminLayout>
    <div class="admin-page">
      <div class="page-header">
        <div class="breadcrumb">SYSTEM / AUDIT LOG</div>
        <h1 class="text-hero">{{ t('System Audit Log', 'Nhật ký Hệ thống') }}</h1>
        <p class="text-desc">Monitor and search important system activities, security events, and administrative changes.</p>
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
            :disabled-date="disabledDate"
            class="compact-date-picker"
            @change="fetchLogs"
          />

          <div class="realtime-toggle">
            <el-switch v-model="isRealtime" @change="handleRealtimeToggle" />
            <span>Realtime</span>
          </div>
        </div>
      </div>

      <section class="settings-card no-padding">
        <div v-loading="loading">
          <el-table :data="logs" style="width: 100%" class="admin-table">
            <el-table-column prop="timestamp" label="TIMESTAMP" width="180">
              <template #default="scope">
                <span class="timestamp-text">{{ formatDateLocal(scope.row.timestamp) }}</span>
              </template>
            </el-table-column>
            
            <el-table-column prop="user" label="USER" width="200">
              <template #default="scope">
                <div class="user-cell-audit">
                  <div class="mini-avatar">{{ getInitials(scope.row.user) }}</div>
                  <span>{{ scope.row.user }}</span>
                </div>
              </template>
            </el-table-column>

            <el-table-column prop="action" label="ACTION" width="140">
              <template #default="scope">
                <span class="action-tag">{{ scope.row.action }}</span>
              </template>
            </el-table-column>

            <el-table-column prop="resource" label="RESOURCE">
              <template #default="scope">
                <div class="resource-stack">
                  <strong>{{ scope.row.resource }}</strong>
                  <small>{{ scope.row.targetId }}</small>
                </div>
              </template>
            </el-table-column>

            <el-table-column prop="status" label="STATUS" width="120">
              <template #default="scope">
                <div class="status-badge" :class="scope.row.status.toLowerCase()">
                  {{ scope.row.status }}
                </div>
              </template>
            </el-table-column>
          </el-table>

          <div class="table-footer" v-if="total > 0">
            <el-pagination
              v-model:current-page="currentPage"
              layout="prev, pager, next"
              :total="total"
              :page-size="20"
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
        const params = { page: currentPage.value, limit: 20 }
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
  padding: 8px 12px 8px 36px;
  background: var(--input-bg);
  border: 1px solid var(--border-color);
  border-radius: 2px;
  color: var(--text-primary);
  outline: none;
}

.filter-group {
  display: flex;
  align-items: center;
  gap: 12px;
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

@media (max-width: 1000px) {
  .header-actions-row { flex-direction: column; align-items: stretch; }
  .search-box { max-width: 100%; }
}
</style>
