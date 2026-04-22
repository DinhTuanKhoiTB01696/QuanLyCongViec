<template>
  <AdminLayout>
    <div class="admin-page">
      <div class="page-header">
        <div class="breadcrumb">SYSTEM / CONFIGURATION</div>
        <h1 class="text-hero">{{ t('System Configuration', 'Cấu hình hệ thống') }}</h1>
        <p class="text-desc">
          Manage workflow defaults, project lifecycle states, and application aesthetics. 
          Configure the foundational building blocks for all workspace activities.
        </p>
      </div>

      <div class="config-grid" v-loading="isLoading">
        <!-- Default Workflow Statuses -->
        <section class="settings-card">
          <div class="section-head">
            <div>
              <h2 class="text-section">{{ t('Default workflow statuses', 'Trạng thái công việc mặc định') }}</h2>
              <p class="text-desc">{{ t('Standard lifecycle states used for new projects and task templates.', 'Các trạng thái tiêu chuẩn dùng cho dự án mới và mẫu công việc.') }}</p>
            </div>
            <div class="section-actions">
              <button type="button" class="secondary-btn" @click="resetDefaultTaskStatuses">{{ t('Reset', 'Khôi phục') }}</button>
              <button type="button" class="primary-btn" @click="saveDefaultTaskStatuses">{{ t('Save Changes', 'Lưu thay đổi') }}</button>
            </div>
          </div>

          <div class="status-list">
            <div v-for="(status, index) in defaultTaskStatuses" :key="`${status.key}-${index}`" class="status-row">
              <input v-model="status.name" type="text" class="status-input" />
              <input v-model="status.color" type="color" class="color-input" />
              <label class="toggle-row mini-toggle">
                <input v-model="status.isDefault" type="checkbox" />
                <span>{{ t('Default', 'Mặc định') }}</span>
              </label>
              <button type="button" class="plain-action danger-action" @click="removeStatusRow(defaultTaskStatuses, index)">{{ t('Remove', 'Xóa') }}</button>
            </div>
          </div>

          <button type="button" class="btn-ghost add-row-btn" @click="addStatusRow(defaultTaskStatuses)">
            <i class="fa-solid fa-plus"></i> {{ t('Add status', 'Thêm trạng thái') }}
          </button>
        </section>

        <!-- Project Lifecycle Statuses -->
        <section class="settings-card">
          <div class="section-head">
            <div>
              <h2>{{ t('Project lifecycle statuses', 'Trạng thái dự án') }}</h2>
              <p>{{ t('Define the high-level roadmap states for organization-wide visibility.', 'Định nghĩa các trạng thái lộ trình cho khả năng hiển thị toàn tổ chức.') }}</p>
            </div>
            <div class="section-actions">
              <button type="button" class="secondary-btn" @click="resetProjectStatuses">{{ t('Reset', 'Khôi phục') }}</button>
              <button type="button" class="primary-btn" @click="saveProjectStatuses">{{ t('Save Changes', 'Lưu thay đổi') }}</button>
            </div>
          </div>

          <div class="status-list">
            <div v-for="(status, index) in projectStatuses" :key="`${status.key}-${index}`" class="status-row">
              <input v-model="status.name" type="text" class="status-input" />
              <input v-model="status.color" type="color" class="color-input" />
              <label class="toggle-row mini-toggle">
                <input v-model="status.isDefault" type="checkbox" />
                <span>{{ t('Default', 'Mặc định') }}</span>
              </label>
              <button type="button" class="plain-action danger-action" @click="removeStatusRow(projectStatuses, index)">{{ t('Remove', 'Xóa') }}</button>
            </div>
          </div>

          <button type="button" class="btn-ghost add-row-btn" @click="addStatusRow(projectStatuses)">
            <i class="fa-solid fa-plus"></i> {{ t('Add project status', 'Thêm trạng thái dự án') }}
          </button>
        </section>

        <!-- System Performance -->
        <section class="settings-card">
          <div class="section-head">
            <div>
              <h2>{{ t('System Health & Performance', 'Sức khỏe & Hiệu suất hệ thống') }}</h2>
              <p>{{ t('Real-time response metrics for monitoring system stability.', 'Chỉ số phản hồi thời gian thực để giám sát sự ổn định hệ thống.') }}</p>
            </div>
            <div class="performance-metric">
              <span class="metric-label">Avg Response</span>
              <strong class="metric-value">{{ currentResponseTime }}ms</strong>
            </div>
          </div>

          <div class="chart-container">
            <apexchart type="area" height="280" :options="chartOptions" :series="chartSeries"></apexchart>
          </div>
        </section>
      </div>
    </div>
  </AdminLayout>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import { ElMessage } from 'element-plus'
import apexchart from 'vue3-apexcharts'
import AdminLayout from '@/components/layout/AdminLayout.vue'
import axiosClient from '@/api/axiosClient'
import { useLocale } from '@/composables/useLocale'

const { t } = useLocale()
const isLoading = ref(false)
const currentResponseTime = ref(0)
let metricsInterval = null

const defaultTaskStatuses = ref([])
const projectStatuses = ref([])

const getDefaultTaskStatusSeed = () => ([
  { key: 'BACKLOG', name: 'Backlog', color: '#94a3b8', position: 0, isDefault: true },
  { key: 'TODO', name: 'Todo', color: '#64748b', position: 1, isDefault: true },
  { key: 'IN_PROGRESS', name: 'In Progress', color: '#3B82F6', position: 2, isDefault: true },
  { key: 'IN_REVIEW', name: 'In Review', color: '#F59E0B', position: 3, isDefault: true },
  { key: 'DONE', name: 'Done', color: '#10B981', position: 4, isDefault: true },
  { key: 'CANCELLED', name: 'Cancelled', color: '#EF4444', position: 5, isDefault: true }
])

const getProjectStatusSeed = () => ([
  { key: 'PLANNING', name: 'Planning', color: '#94a3b8', position: 0, isDefault: true },
  { key: 'ACTIVE', name: 'Active', color: '#3B82F6', position: 1, isDefault: true },
  { key: 'ON_HOLD', name: 'On Hold', color: '#F59E0B', position: 2, isDefault: false },
  { key: 'COMPLETED', name: 'Completed', color: '#10B981', position: 3, isDefault: true },
  { key: 'CANCELLED', name: 'Cancelled', color: '#EF4444', position: 4, isDefault: false }
])

const chartSeries = ref([{ name: 'Response Time (ms)', data: [] }])
const chartOptions = ref({
  chart: { type: 'area', toolbar: { show: false }, animations: { enabled: true, easing: 'easeinout', speed: 800 } },
  colors: ['#38bdf8'],
  fill: { type: 'gradient', gradient: { shadeIntensity: 1, opacityFrom: 0.4, opacityTo: 0.05, stops: [0, 90, 100] } },
  dataLabels: { enabled: false },
  stroke: { curve: 'smooth', width: 2 },
  xaxis: { labels: { show: false }, axisBorder: { show: false }, axisTicks: { show: false } },
  yaxis: { labels: { style: { colors: 'var(--color-text-muted)' } } },
  grid: { borderColor: 'var(--color-border)', strokeDashArray: 4 },
  tooltip: { theme: 'dark' }
})

const normalizeStatuses = (items) => items
  .filter((item) => item.name && item.name.trim())
  .map((item, index) => ({
    key: (item.key || item.name).trim().toUpperCase().replace(/\s+/g, '_'),
    name: item.name.trim(),
    color: item.color || '#94a3b8',
    position: index,
    isDefault: Boolean(item.isDefault)
  }))

const addStatusRow = (target) => {
  target.value.push({
    key: `CUSTOM_${target.value.length + 1}`,
    name: '',
    color: '#94a3b8',
    position: target.value.length,
    isDefault: false
  })
}

const removeStatusRow = (target, index) => {
  target.value.splice(index, 1)
}

const resetDefaultTaskStatuses = () => {
  defaultTaskStatuses.value = getDefaultTaskStatusSeed()
}

const resetProjectStatuses = () => {
  projectStatuses.value = getProjectStatusSeed()
}

const fetchDefaultTaskStatuses = async () => {
  const res = await axiosClient.get('/settings/admin/default-task-statuses')
  defaultTaskStatuses.value = (res.data?.data?.length ? res.data.data : getDefaultTaskStatusSeed())
}

const fetchProjectStatuses = async () => {
  const res = await axiosClient.get('/settings/admin/project-statuses')
  projectStatuses.value = (res.data?.data?.length ? res.data.data : getProjectStatusSeed())
}

const saveDefaultTaskStatuses = async () => {
  try {
    await axiosClient.put('/settings/admin/default-task-statuses', {
      items: normalizeStatuses(defaultTaskStatuses.value)
    })
    ElMessage.success(t('Default workflow statuses saved.', 'Đã lưu trạng thái công việc mặc định.'))
    await fetchDefaultTaskStatuses()
  } catch (error) {
    ElMessage.error(error.response?.data?.message || t('Could not save default workflow statuses.', 'Không thể lưu trạng thái công việc mặc định.'))
  }
}

const saveProjectStatuses = async () => {
  try {
    await axiosClient.put('/settings/admin/project-statuses', {
      items: normalizeStatuses(projectStatuses.value)
    })
    ElMessage.success(t('Project statuses saved.', 'Đã lưu trạng thái dự án.'))
    await fetchProjectStatuses()
  } catch (error) {
    ElMessage.error(error.response?.data?.message || t('Could not save project statuses.', 'Không thể lưu trạng thái dự án.'))
  }
}

const fetchMetrics = async () => {
  try {
    const res = await axiosClient.get('/admin/system/metrics')
    const data = res.data?.data || []
    chartSeries.value = [{ name: 'Response Time (ms)', data }]
    currentResponseTime.value = data.length ? data[data.length - 1] : 0
  } catch (error) {
    console.error(error)
  }
}

onMounted(async () => {
  try {
    isLoading.value = true
    await Promise.all([
      fetchDefaultTaskStatuses(),
      fetchProjectStatuses(),
      fetchMetrics()
    ])
    metricsInterval = setInterval(fetchMetrics, 10000)
  } finally {
    isLoading.value = false
  }
})

onUnmounted(() => {
  if (metricsInterval) clearInterval(metricsInterval)
})
</script>

<style scoped>
.breadcrumb {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  color: var(--color-text-muted);
  font-size: 13px;
  margin-bottom: 8px;
}

.config-grid {
  display: grid;
  gap: 24px;
}

.section-actions {
  display: flex;
  gap: 12px;
}

.status-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
  margin-bottom: 20px;
}

.status-row {
  display: grid;
  grid-template-columns: minmax(0, 1fr) 56px 120px 80px;
  gap: 16px;
  align-items: center;
  padding: 8px 12px;
  border: 1px solid var(--color-border);
  border-radius: 2px;
  background: var(--color-surface-hover);
}

.status-input {
  background: transparent !important;
  border: none !important;
  padding: 0 !important;
  font-weight: 600;
}

.color-input {
  width: 100%;
  height: 32px;
  padding: 0;
  border: none;
  background: transparent;
  cursor: pointer;
}

.mini-toggle {
  padding: 0;
}

.mini-toggle span {
  font-size: 12px;
}

.add-row-btn {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  color: var(--color-accent);
  font-weight: 600;
  background: transparent;
  border: none;
  cursor: pointer;
}

.performance-metric {
  text-align: right;
}

.metric-label {
  display: block;
  font-size: 11px;
  color: var(--color-text-muted);
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.metric-value {
  font-size: 24px;
  color: var(--color-accent);
}

.btn-ghost {
  padding: 8px 16px;
  border-radius: 2px;
  font-size: 14px;
  font-weight: 600;
  cursor: pointer;
  background: transparent;
  border: 1px dashed var(--color-border);
  color: var(--color-text-secondary);
}

.btn-ghost:hover {
  background: var(--color-surface-hover);
  border-style: solid;
  color: var(--color-text-primary);
}

.danger-action {
  color: #ef4444;
  background: transparent;
  border: none;
  font-weight: 600;
  cursor: pointer;
}
</style>

