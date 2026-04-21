<template>
  <AdminLayout>
    <div class="admin-page-container">
      <div class="page-header">
        <div class="breadcrumb">
          <i class="fa-solid fa-gear"></i> System / Configuration
        </div>
        <h1 class="page-title">{{ t('System Configuration', 'Cấu hình hệ thống') }}</h1>
        <p class="page-subtitle">{{ t('Manage workflow defaults, project lifecycle states, theme presets, and system health.', 'Quản lý trạng thái mặc định, vòng đời dự án, giao diện và sức khỏe hệ thống.') }}</p>
      </div>

      <div class="config-grid" v-loading="isLoading">
        <section class="settings-card">
          <div class="section-head">
            <div>
              <h2 class="card-title">{{ t('Default workflow statuses', 'Trạng thái công việc mặc định') }}</h2>
              <p class="section-desc">{{ t('These statuses are used as the default admin template: Backlog, Todo, In Progress, In Review, Done, Cancelled.', 'Các trạng thái này là mẫu mặc định phía admin: Backlog, Todo, In Progress, In Review, Done, Cancelled.') }}</p>
            </div>
            <div class="section-actions">
              <button type="button" class="neutral-btn" @click="resetDefaultTaskStatuses">{{ t('Reset', 'Khôi phục') }}</button>
              <button type="button" class="primary-btn" @click="saveDefaultTaskStatuses">{{ t('Save', 'Lưu') }}</button>
            </div>
          </div>

          <div class="status-list">
            <div v-for="(status, index) in defaultTaskStatuses" :key="`${status.key}-${index}`" class="status-row">
              <input v-model="status.name" type="text" />
              <input v-model="status.color" type="color" class="color-input" />
              <label class="inline-check">
                <input v-model="status.isDefault" type="checkbox" />
                <span>{{ t('Default', 'Mặc định') }}</span>
              </label>
              <button type="button" class="plain-action danger-action" @click="removeStatusRow(defaultTaskStatuses, index)">{{ t('Remove', 'Xóa') }}</button>
            </div>
          </div>

          <button type="button" class="plain-action add-row-btn" @click="addStatusRow(defaultTaskStatuses)">{{ t('Add status', 'Thêm trạng thái') }}</button>
        </section>

        <section class="settings-card">
          <div class="section-head">
            <div>
              <h2 class="card-title">{{ t('Project lifecycle statuses', 'Trạng thái dự án') }}</h2>
              <p class="section-desc">{{ t('Use these states for project-wide lifecycle management and filtering.', 'Dùng các trạng thái này cho vòng đời dự án và bộ lọc quản trị.') }}</p>
            </div>
            <div class="section-actions">
              <button type="button" class="neutral-btn" @click="resetProjectStatuses">{{ t('Reset', 'Khôi phục') }}</button>
              <button type="button" class="primary-btn" @click="saveProjectStatuses">{{ t('Save', 'Lưu') }}</button>
            </div>
          </div>

          <div class="status-list">
            <div v-for="(status, index) in projectStatuses" :key="`${status.key}-${index}`" class="status-row">
              <input v-model="status.name" type="text" />
              <input v-model="status.color" type="color" class="color-input" />
              <label class="inline-check">
                <input v-model="status.isDefault" type="checkbox" />
                <span>{{ t('Default', 'Mặc định') }}</span>
              </label>
              <button type="button" class="plain-action danger-action" @click="removeStatusRow(projectStatuses, index)">{{ t('Remove', 'Xóa') }}</button>
            </div>
          </div>

          <button type="button" class="plain-action add-row-btn" @click="addStatusRow(projectStatuses)">{{ t('Add project status', 'Thêm trạng thái dự án') }}</button>
        </section>

        <section class="settings-card">
          <div class="section-head">
            <div>
              <h2 class="card-title">{{ t('Global Theme Palette', 'Bảng màu giao diện') }}</h2>
              <p class="section-desc">{{ t('Choose and save the theme variables used across the application.', 'Chọn và lưu bộ biến giao diện dùng trên toàn hệ thống.') }}</p>
            </div>
          </div>

          <div class="preset-row">
            <el-select v-model="selectedPreset" filterable :placeholder="t('Select a theme preset', 'Chọn preset giao diện')" class="glass-input" @change="applyTemplatePreset">
              <el-option v-for="preset in templates" :key="preset.name" :label="preset.enName ? t(preset.enName, preset.name) : preset.name" :value="preset.name"></el-option>
            </el-select>
            <button type="button" class="neutral-btn" @click="resetThemeToDefault">{{ t('Reset theme', 'Khôi phục theme') }}</button>
          </div>

          <div class="color-grid">
            <div v-for="(color, key) in themeColors" :key="key" class="color-setting">
              <span class="color-label">{{ getColorLabel(key) }}</span>
              <div v-if="key === 'bgImage'">
                <el-select v-model="color.value" class="glass-input" @change="onCustomColorChange(key, color.value)">
                  <el-option :label="t('No background', 'Không dùng nền')" value="none"></el-option>
                  <el-option :label="t('Ocean', 'Đại dương')" value="linear-gradient(135deg, #0b1c31 0%, #0d324d 100%)"></el-option>
                  <el-option :label="t('Sunset', 'Hoàng hôn')" value="linear-gradient(135deg, #f59e0b 0%, #ef4444 100%)"></el-option>
                  <el-option :label="t('Midnight', 'Nửa đêm')" value="linear-gradient(135deg, #111827 0%, #1f2937 100%)"></el-option>
                </el-select>
              </div>
              <div v-else class="color-picker-wrapper">
                <el-color-picker v-model="color.value" :predefine="predefineColors" show-alpha @change="onCustomColorChange(key, color.value)" />
              </div>
            </div>
          </div>

          <div class="section-actions right-actions">
            <button type="button" class="neutral-btn" @click="saveThemeToBackend(false)">{{ t('Save theme', 'Lưu theme') }}</button>
            <button type="button" class="primary-btn" @click="saveAndAddFavorite">{{ t('Save to favorites', 'Lưu vào yêu thích') }}</button>
          </div>
        </section>

        <section class="settings-card">
          <div class="section-head">
            <div>
              <h2 class="card-title">{{ t('Performance Metrics', 'Hiệu suất hệ thống') }}</h2>
              <p class="section-desc">{{ t('A lightweight signal based on recent system activity.', 'Chỉ số nhẹ dựa trên hoạt động gần đây của hệ thống.') }}</p>
            </div>
            <strong class="metric-highlight">{{ currentResponseTime }}ms</strong>
          </div>

          <div class="chart-wrapper">
            <apexchart type="area" height="250" :options="chartOptions" :series="chartSeries"></apexchart>
          </div>
        </section>
      </div>
    </div>
  </AdminLayout>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import apexchart from 'vue3-apexcharts'
import AdminLayout from '@/components/layout/AdminLayout.vue'
import axiosClient from '@/api/axiosClient'
import { useLocale } from '@/composables/useLocale'

const { t } = useLocale()

const isLoading = ref(false)
const selectedPreset = ref('')
const predefineColors = ['#0f172a', '#1e293b', 'rgba(255,255,255,0.05)', 'rgba(255,255,255,0.1)', 'rgba(255,255,255,0.85)', 'rgba(29,33,37,0.85)']

const defaultTaskStatuses = ref([])
const projectStatuses = ref([])

const getDefaultTaskStatusSeed = () => ([
  { key: 'BACKLOG', name: 'Backlog', color: '#71717A', position: 0, isDefault: true },
  { key: 'TODO', name: 'Todo', color: '#94A3B8', position: 1, isDefault: true },
  { key: 'IN_PROGRESS', name: 'In Progress', color: '#3B82F6', position: 2, isDefault: true },
  { key: 'IN_REVIEW', name: 'In Review', color: '#F59E0B', position: 3, isDefault: true },
  { key: 'DONE', name: 'Done', color: '#10B981', position: 4, isDefault: true },
  { key: 'CANCELLED', name: 'Cancelled', color: '#EF4444', position: 5, isDefault: true }
])

const getProjectStatusSeed = () => ([
  { key: 'PLANNING', name: 'Planning', color: '#94A3B8', position: 0, isDefault: true },
  { key: 'ACTIVE', name: 'Active', color: '#3B82F6', position: 1, isDefault: true },
  { key: 'ON_HOLD', name: 'On Hold', color: '#F59E0B', position: 2, isDefault: false },
  { key: 'COMPLETED', name: 'Completed', color: '#10B981', position: 3, isDefault: true },
  { key: 'CANCELLED', name: 'Cancelled', color: '#EF4444', position: 4, isDefault: false }
])

const themeColors = ref({
  bgImage: { variable: '--bg-image', value: 'none', default: 'none' },
  bgLayout: { variable: '--bg-layout', value: '#f4f5f7', default: '#f4f5f7' },
  bgCard: { variable: '--bg-card', value: 'rgba(255, 255, 255, 0.85)', default: 'rgba(255, 255, 255, 0.85)' },
  bgHover: { variable: '--bg-hover', value: 'rgba(235, 236, 240, 0.8)', default: 'rgba(235, 236, 240, 0.8)' },
  borderColor: { variable: '--border-color', value: 'rgba(223, 225, 230, 0.5)', default: 'rgba(223, 225, 230, 0.5)' },
  textPrimary: { variable: '--text-primary', value: '#172b4d', default: '#172b4d' }
})

const getColorLabel = (key) => ({
  bgImage: t('Background style', 'Kiểu nền'),
  bgLayout: t('Page background', 'Nền trang'),
  bgCard: t('Card background', 'Nền khối nội dung'),
  bgHover: t('Hover color', 'Màu hover'),
  borderColor: t('Border color', 'Màu viền'),
  textPrimary: t('Primary text', 'Màu chữ chính')
}[key] || key)

const defaultTemplates = [
  { name: 'Light Glass', enName: 'Light Glass', bgImage: 'none', bgLayout: '#f4f5f7', bgCard: 'rgba(255, 255, 255, 0.85)', bgHover: 'rgba(235, 236, 240, 0.8)', borderColor: 'rgba(223, 225, 230, 0.5)', textPrimary: '#172b4d' },
  { name: 'Ocean', enName: 'Ocean', bgImage: 'linear-gradient(135deg, #0b1c31 0%, #0d324d 100%)', bgLayout: '#0b1c31', bgCard: 'rgba(255, 255, 255, 0.05)', bgHover: 'rgba(255, 255, 255, 0.1)', borderColor: 'rgba(255, 255, 255, 0.1)', textPrimary: '#ffffff' },
  { name: 'Sunset', enName: 'Sunset', bgImage: 'linear-gradient(135deg, #f59e0b 0%, #ef4444 100%)', bgLayout: '#7c2d12', bgCard: 'rgba(255, 255, 255, 0.08)', bgHover: 'rgba(255, 255, 255, 0.14)', borderColor: 'rgba(255, 255, 255, 0.12)', textPrimary: '#ffffff' }
]

const templates = ref([...defaultTemplates])
const currentResponseTime = ref(0)
let metricsInterval = null

const chartSeries = ref([{ name: 'Response Time (ms)', data: [] }])
const chartOptions = ref({
  chart: { type: 'area', toolbar: { show: false }, animations: { enabled: true, easing: 'easeinout', speed: 800 } },
  colors: ['#0d9488'],
  fill: { type: 'gradient', gradient: { shadeIntensity: 1, opacityFrom: 0.4, opacityTo: 0.05, stops: [0, 90, 100] } },
  dataLabels: { enabled: false },
  stroke: { curve: 'smooth', width: 2 },
  xaxis: { labels: { show: false }, axisBorder: { show: false }, axisTicks: { show: false } },
  yaxis: { labels: { style: { colors: '#8b949e' } } },
  grid: { borderColor: 'rgba(128,128,128,0.1)', strokeDashArray: 4 },
  tooltip: { theme: 'dark' }
})

const normalizeStatuses = (items) => items
  .filter((item) => item.name && item.name.trim())
  .map((item, index) => ({
    key: (item.key || item.name).trim().toUpperCase().replace(/\s+/g, '_'),
    name: item.name.trim(),
    color: item.color || '#94A3B8',
    position: index,
    isDefault: Boolean(item.isDefault)
  }))

const addStatusRow = (target) => {
  target.value.push({
    key: `CUSTOM_${target.value.length + 1}`,
    name: '',
    color: '#94A3B8',
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

const applyThemeToDocument = () => {
  Object.values(themeColors.value).forEach((color) => {
    document.documentElement.style.setProperty(color.variable, color.value)
  })
}

const fetchTheme = async () => {
  const res = await axiosClient.get('/settings/ThemeSettings')
  const data = res.data?.data || {}

  if (data.SavedPresets) {
    try {
      const parsed = JSON.parse(data.SavedPresets)
      templates.value = [...parsed, ...defaultTemplates]
    } catch (error) {
      templates.value = [...defaultTemplates]
    }
  }

  Object.keys(themeColors.value).forEach((key) => {
    if (data[key]) {
      themeColors.value[key].value = data[key]
    }
  })

  applyThemeToDocument()
}

const saveThemeToBackend = async (showMessage = true) => {
  try {
    const payload = { Settings: {} }
    Object.keys(themeColors.value).forEach((key) => {
      payload.Settings[key] = themeColors.value[key].value
    })
    const customPresets = templates.value.filter((item) => !defaultTemplates.find((preset) => preset.name === item.name))
    payload.Settings.SavedPresets = JSON.stringify(customPresets)
    await axiosClient.put('/settings/ThemeSettings', payload)
    if (showMessage) {
      ElMessage.success(t('Theme saved.', 'Đã lưu theme.'))
    }
  } catch (error) {
    if (showMessage) {
      ElMessage.error(t('Could not save theme.', 'Không thể lưu theme.'))
    }
  }
}

const applyTemplatePreset = async (name) => {
  const preset = templates.value.find((item) => item.name === name)
  if (!preset) return

  Object.keys(themeColors.value).forEach((key) => {
    if (preset[key] !== undefined) {
      themeColors.value[key].value = preset[key]
    }
  })
  applyThemeToDocument()
  await saveThemeToBackend(false)
}

const onCustomColorChange = async (key, value) => {
  themeColors.value[key].value = value
  applyThemeToDocument()
  await saveThemeToBackend(false)
}

const resetThemeToDefault = async () => {
  Object.keys(themeColors.value).forEach((key) => {
    themeColors.value[key].value = themeColors.value[key].default
  })
  selectedPreset.value = ''
  applyThemeToDocument()
  await saveThemeToBackend(true)
}

const saveAndAddFavorite = async () => {
  try {
    const { value } = await ElMessageBox.prompt(
      t('Save and name your theme preset', 'Lưu và đặt tên preset giao diện'),
      t('Save theme', 'Lưu theme'),
      {
        inputPattern: /.+/,
        inputErrorMessage: t('Theme name is required.', 'Tên theme là bắt buộc.')
      }
    )

    const customPreset = { name: value.trim() }
    Object.keys(themeColors.value).forEach((key) => {
      customPreset[key] = themeColors.value[key].value
    })
    templates.value.unshift(customPreset)
    selectedPreset.value = customPreset.name
    await saveThemeToBackend(false)
    ElMessage.success(t('Theme preset saved.', 'Đã lưu preset giao diện.'))
  } catch (error) {
    if (error !== 'cancel') {
      ElMessage.error(t('Could not save theme preset.', 'Không thể lưu preset giao diện.'))
    }
  }
}

onMounted(async () => {
  try {
    isLoading.value = true
    await Promise.all([
      fetchDefaultTaskStatuses(),
      fetchProjectStatuses(),
      fetchTheme(),
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
.page-header {
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
  margin-bottom: 4px;
}

.page-subtitle {
  font-size: 14px;
  color: #8b949e;
}

.config-grid {
  display: grid;
  gap: 20px;
}

.settings-card {
  background-color: var(--bg-card);
  border: 1px solid var(--border-color);
  border-radius: 8px;
  padding: 24px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
}

.section-head {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 16px;
  margin-bottom: 16px;
}

.section-actions {
  display: flex;
  gap: 10px;
  align-items: center;
}

.right-actions {
  justify-content: flex-end;
  margin-top: 16px;
}

.card-title {
  font-size: 18px;
  font-weight: 600;
  color: var(--text-primary);
  margin: 0 0 8px;
}

.section-desc {
  margin: 0;
  color: #8b949e;
  font-size: 13px;
  line-height: 1.5;
}

.status-list {
  display: grid;
  gap: 10px;
}

.status-row {
  display: grid;
  grid-template-columns: minmax(0, 1fr) 72px 120px 80px;
  gap: 10px;
  align-items: center;
}

.status-row input[type='text'] {
  min-height: 36px;
  border: 1px solid var(--border-color);
  border-radius: 8px;
  background: rgba(255, 255, 255, 0.03);
  color: var(--text-primary);
  padding: 0 12px;
}

.color-input {
  width: 100%;
  height: 36px;
  border: 1px solid var(--border-color);
  border-radius: 8px;
  background: transparent;
  padding: 2px;
}

.inline-check {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  color: var(--text-secondary);
  font-size: 13px;
}

.add-row-btn {
  margin-top: 12px;
}

.preset-row {
  display: flex;
  gap: 10px;
  margin-bottom: 16px;
}

.color-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: 16px;
}

.color-setting {
  display: grid;
  gap: 10px;
  border: 1px solid var(--border-color);
  border-radius: 8px;
  padding: 14px;
  background: rgba(255, 255, 255, 0.03);
}

.color-label {
  color: var(--text-primary);
  font-size: 14px;
  font-weight: 600;
}

.color-picker-wrapper {
  display: flex;
  align-items: center;
}

.metric-highlight {
  color: #0d9488;
  font-size: 20px;
}

.chart-wrapper {
  min-height: 250px;
}

.primary-btn,
.neutral-btn,
.plain-action {
  min-height: 34px;
  border-radius: 8px;
  cursor: pointer;
  font-size: 14px;
  font-weight: 600;
  letter-spacing: 0;
}

.primary-btn {
  padding: 0 14px;
  border: 1px solid #579dff;
  background: #579dff;
  color: #081120;
}

.neutral-btn {
  padding: 0 12px;
  border: 1px solid var(--border-color);
  background: rgba(255, 255, 255, 0.02);
  color: var(--text-primary);
}

.plain-action {
  border: 0;
  background: transparent;
  color: #9cc4ff;
}

.danger-action {
  color: #f87171;
}

@media (max-width: 860px) {
  .section-head,
  .preset-row {
    flex-direction: column;
    align-items: stretch;
  }

  .status-row {
    grid-template-columns: 1fr;
  }
}
</style>
