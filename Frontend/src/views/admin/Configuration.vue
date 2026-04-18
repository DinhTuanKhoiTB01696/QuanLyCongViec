<template>
  <AdminLayout>
    <div class="admin-page-container">
    <div class="page-header">
      <div class="breadcrumb">
        <i class="fa-solid fa-gear"></i> System / Configuration
      </div>
      <h1 class="page-title">{{ t('System Configuration', 'Cấu hình Hệ thống (System Configuration)') }}</h1>
      <p class="page-subtitle">{{ t('Manage global interface and monitor system performance.', 'Quản lý giao diện toàn cục và theo dõi hiệu suất hệ thống.') }}</p>
    </div>

    <div class="form-container" v-loading="isLoading">
      
      <!-- Customization Section -->
      <div class="settings-card">
        <h2 class="card-title">{{ t('Global Theme Palette', 'Thiết kế Màu sắc Phân tầng (Global Theme Palette)') }}</h2>
        <p class="section-desc">{{ t('This setting determines the color language and Glass Layering system for the entire website.', 'Thiết lập này sẽ quyết định ngôn ngữ màu sắc và hệ thống kính mờ Layering (Glass) cho toàn bộ trang Web.') }}</p>

        <el-tabs v-model="activeTab" class="theme-tabs mt-24">
          <!-- TAB 1: PRESETS -->
          <el-tab-pane :label="t('Saved presets', 'Mẫu màu có sẵn / Đã lưu')" name="presets">
            <div style="margin-top: 16px; margin-bottom: 24px;">
              <h3 style="font-size: 15px; margin-bottom: 12px; color: var(--text-primary)">{{ t('Explore and select a color theme from the list', 'Khám phá và Chọn Hệ màu trong danh sách (Combobox)') }}</h3>
              <div style="display: flex; gap: 12px; max-width: 400px">
                 <el-select v-model="selectedPreset" filterable :placeholder="t('Search and select a theme...', 'Tìm kiếm và chọn tên hệ màu...')" style="flex: 1" class="glass-input" @change="applyTemplatePreset">
                    <el-option v-for="p in templates" :key="p.name" :label="p.enName ? t(p.enName, p.name) : p.name" :value="p.name"></el-option>
                 </el-select>
              </div>
            </div>

            <div class="action-footer">
              <el-button @click="resetToDefault">{{ t('Reset to default', 'Khôi phục mặc định') }}</el-button>
            </div>
          </el-tab-pane>

          <!-- TAB 2: CUSTOM COLOR -->
          <el-tab-pane :label="t('Custom colors', 'Màu tự thiết kế')" name="custom">
            <div style="margin-top: 16px; margin-bottom: 12px;">
               <h3 style="font-size: 14px; margin-bottom: 8px; color: var(--text-primary)">{{ t('Freely mix colors to your preference:', 'Tự do phối màu theo sở thích:') }}</h3>
               <p style="font-size: 13px; color: var(--text-secondary); margin: 0">{{ t('You can choose each color zone. Then apply or save to favorites.', 'Bạn có thể tự chọn từng vùng màu. Sau đó có thể áp dụng hoặc lưu vào danh sách yêu thích.') }}</p>
            </div>
            
            <div class="color-grid mt-24">
              <div class="color-setting" v-for="(color, key) in themeColors" :key="key">
                <span class="color-label">{{ getColorLabel(key) }}</span>
                <div v-if="key === 'bgImage'" class="input-wrapper">
                   <el-select v-model="color.value" @change="onCustomColorChange(key, color.value)" :placeholder="t('Select background style', 'Chọn kiểu nền')" style="width: 100%" class="glass-input">
                      <el-option :label="t('No background', 'Không dùng nền')" value="none"></el-option>
                      <el-option :label="t('Mysterious Night (Purple)', 'Màn đêm Huyền bí (Tím)')" value="linear-gradient(135deg, #1e0030 0%, #3a005c 100%)"></el-option>
                      <el-option :label="t('Deep Ocean (Blue)', 'Đại dương Sâu thẳm (Xanh)')" value="linear-gradient(135deg, #0b1c31 0%, #0d324d 100%)"></el-option>
                      <el-option :label="t('Sunset Glow (Orange-Red)', 'Hoàng hôn Rực rỡ (Cam-Đỏ)')" value="linear-gradient(135deg, #f59e0b 0%, #ef4444 100%)"></el-option>
                   </el-select>
                </div>
                <div v-else class="color-picker-wrapper">
                  <el-color-picker v-model="color.value" :predefine="predefineColors" show-alpha @change="onCustomColorChange(key, color.value)" />
                </div>
              </div>
            </div>

            <div class="action-footer">
              <el-button @click="resetToDefault">{{ t('Reset to default', 'Khôi phục màu Mặc định') }}</el-button>
              <el-button plain type="success" :loading="isSaving" @click="saveAndAddFavorite">{{ t('Save to Favorites', 'Lưu vào DS Yêu thích') }}</el-button>
            </div>
          </el-tab-pane>
        </el-tabs>

      </div>

      <!-- Performance Metrics Section -->
      <div class="settings-card mt-24">
        <div class="card-header-icon" style="display: flex; align-items: center; gap: 12px; margin-bottom: 24px;">
           <div class="icon-box" style="width: 40px; height: 40px; background: rgba(59, 130, 246, 0.1); border-radius: 8px; display: flex; align-items: center; justify-content: center; color: #3b82f6;">
             <i class="fa-solid fa-chart-line" style="font-size: 18px;"></i>
           </div>
           <h2 class="card-title" style="margin: 0; font-size: 20px;">{{ t('Performance Metrics', 'Hiệu suất Hệ thống') }}</h2>
        </div>

        <div class="metric-container" style="background: var(--bg-hover); border: 1px solid var(--border-color); border-radius: 12px; padding: 24px;">
           <div style="display: flex; justify-content: space-between; margin-bottom: 10px;">
             <span style="font-size: 14px; font-weight: 500; color: var(--text-primary);">{{ t('API Response Time (Last 100 requests)', 'Thời gian phản hồi API (100 yêu cầu gần nhất)') }}</span>
             <span style="font-size: 18px; font-weight: 600; color: #0d9488;">{{ currentResponseTime }}ms</span>
           </div>
           
           <!-- Real ApexChart for System Performance -->
           <div class="chart-wrapper" style="height: 250px;">
              <apexchart type="area" height="100%" :options="chartOptions" :series="chartSeries"></apexchart>
           </div>
        </div>
      </div>

    </div>
  </div>
  </AdminLayout>
</template>

<script setup>
import AdminLayout from '@/components/layout/AdminLayout.vue'
import { ref, onMounted, onUnmounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import axiosClient from '@/api/axiosClient'
import apexchart from 'vue3-apexcharts'
import { useLocale } from '@/composables/useLocale'

const { t } = useLocale()

const activeTab = ref('presets')
const isLoading = ref(false)
const isSaving = ref(false)
const selectedPreset = ref('')

const predefineColors = [
  '#0f172a',
  '#1e293b',
  'rgba(255,255,255,0.05)',
  'rgba(255,255,255,0.1)',
  'rgba(255,255,255,0.85)',
  'rgba(29,33,37,0.85)'
]

// Theme configuration matching root variables
const themeColors = ref({
  bgImage: { variable: '--bg-image', value: 'linear-gradient(135deg, #1e0030 0%, #3a005c 100%)', default: 'none' },
  bgLayout: { variable: '--bg-layout', value: '#1e0030', default: '#f4f5f7' },
  bgCard: { variable: '--bg-card', value: 'rgba(255, 255, 255, 0.05)', default: 'rgba(255, 255, 255, 0.85)' },
  bgHover: { variable: '--bg-hover', value: 'rgba(255, 255, 255, 0.1)', default: 'rgba(235, 236, 240, 0.8)' },
  borderColor: { variable: '--border-color', value: 'rgba(255, 255, 255, 0.1)', default: 'rgba(223, 225, 230, 0.5)' },
  textPrimary: { variable: '--text-primary', value: '#ffffff', default: '#172b4d' }
})

const getColorLabel = (key) => {
  const labels = {
    bgImage: t('Full page background / Gradient', 'Ảnh nền toàn trang / Gradient'),
    bgLayout: t('Page background color', 'Màu nền chân trang (Background)'),
    bgCard: t('Content card color', 'Màu các khung khối chứa nội dung'),
    bgHover: t('Hover / Press color', 'Màu nền khi rê chuột (Hover) / Nhấn'),
    borderColor: t('Border / Separator color', 'Màu của đường viền phân tách'),
    textPrimary: t('Primary text color', 'Màu chữ chính')
  }
  return labels[key] || key
}

const defaultTemplates = [
  { name: 'Zing Purple (Gốc)', enName: 'Zing Purple (Orig)', bgImage: 'linear-gradient(135deg, #1e0030 0%, #3a005c 100%)', bgLayout: '#1e0030', bgCard: 'rgba(255, 255, 255, 0.05)', bgHover: 'rgba(255, 255, 255, 0.1)', borderColor: 'rgba(255, 255, 255, 0.1)', textPrimary: '#ffffff' },
  { name: 'Deep Ocean (Xanh biển)', enName: 'Deep Ocean (Blue)', bgImage: 'linear-gradient(135deg, #0b1c31 0%, #0d324d 100%)', bgLayout: '#0b1c31', bgCard: 'rgba(255, 255, 255, 0.05)', bgHover: 'rgba(255, 255, 255, 0.1)', borderColor: 'rgba(255, 255, 255, 0.1)', textPrimary: '#ffffff' },
  { name: 'Light Glass (Sáng)', enName: 'Light Glass (Bright)', bgImage: 'none', bgLayout: '#f4f5f7', bgCard: 'rgba(255, 255, 255, 0.85)', bgHover: 'rgba(235, 236, 240, 0.8)', borderColor: 'rgba(223, 225, 230, 0.5)', textPrimary: '#172b4d' },
  { name: 'Dark Glass (Tối)', enName: 'Dark Glass (Dark)', bgImage: 'none', bgLayout: '#020617', bgCard: 'rgba(34, 39, 43, 0.75)', bgHover: 'rgba(44, 51, 58, 0.8)', borderColor: 'rgba(51, 65, 85, 0.5)', textPrimary: '#f4f5f7' }
];

const templates = ref([...defaultTemplates])

// ApexCharts Data & Options
const currentResponseTime = ref(0)
let metricsInterval = null

const chartSeries = ref([{
  name: 'Response Time (ms)',
  data: []
}])

const chartOptions = ref({
  chart: {
    type: 'area',
    toolbar: { show: false },
    sparkline: { enabled: false },
    animations: { enabled: true, easing: 'easeinout', speed: 800 }
  },
  colors: ['#0d9488'],
  fill: {
    type: 'gradient',
    gradient: { shadeIntensity: 1, opacityFrom: 0.4, opacityTo: 0.05, stops: [0, 90, 100] }
  },
  dataLabels: { enabled: false },
  stroke: { curve: 'smooth', width: 2 },
  xaxis: { labels: { show: false }, axisBorder: { show: false }, axisTicks: { show: false } },
  yaxis: { labels: { style: { colors: '#8b949e' } } },
  grid: { borderColor: 'rgba(128,128,128,0.1)', strokeDashArray: 4 },
  tooltip: { theme: 'dark' }
})

const fetchMetrics = async () => {
    try {
        const res = await axiosClient.get('/admin/system/metrics');
        if (res.data && res.data.data) {
            const data = res.data.data;
            chartSeries.value = [{ name: 'Response Time (ms)', data: data }];
            if (data.length > 0) currentResponseTime.value = data[data.length - 1];
        }
    } catch(e) {
        if (e.response && (e.response.status === 403 || e.response.status === 401)) {
            if (metricsInterval) {
                clearInterval(metricsInterval);
                metricsInterval = null;
            }
        }
    }
}

const applyTemplatePreset = async (name) => {
  const tpl = templates.value.find(x => x.name === name);
  if (tpl) {
    for (const key in themeColors.value) {
      if (tpl[key] !== undefined) {
        themeColors.value[key].value = tpl[key];
        previewColor(key, tpl[key]);
      }
    }
    await saveThemeToBackend(true);
  }
}

const saveThemeToBackend = async (showMsg = true) => {
  try {
    const payload = { Settings: {} }
    for (const key in themeColors.value) {
      payload.Settings[key] = themeColors.value[key].value
    }
    
    const customPresets = templates.value.filter(t => !defaultTemplates.find(d => d.name === t.name))
    payload.Settings['SavedPresets'] = JSON.stringify(customPresets)
    
    await axiosClient.put('/settings/ThemeSettings', payload)
    if (showMsg) {
       ElMessage.success(t('Theme saved and applied globally!', 'Hệ màu đã được tự động lưu và áp dụng toàn cục!'))
    }
  } catch (err) {
    console.error(err)
    if (showMsg) ElMessage.error(t('An error occurred while saving the theme.', 'Có lỗi xảy ra khi tự động lưu hệ màu.'))
  }
}

// Global click-out trigger for auto-saving color picker
const handleColorClickOut = (e) => {
  const popup = document.querySelector('.el-color-dropdown');
  if (popup && !popup.contains(e.target)) {
    // Ensuring it wasn't a click on a color picker trigger
    if (!e.target.closest('.el-color-picker')) {
      const okBtn = popup.querySelector('.el-color-dropdown__btn');
      if (okBtn) okBtn.click();
    }
  }
}

onMounted(async () => {
  await fetchTheme()
  document.addEventListener('mousedown', handleColorClickOut, true)
  
  // Real metrics polling
  await fetchMetrics();
  metricsInterval = setInterval(fetchMetrics, 3000); // Tự động load mỗi 3s
})

onUnmounted(() => {
  document.removeEventListener('mousedown', handleColorClickOut, true)
  if (metricsInterval) clearInterval(metricsInterval);
})

const fetchTheme = async () => {
  isLoading.value = true
  try {
    const res = await axiosClient.get('/settings/ThemeSettings')
    const data = res.data.data || {}
    
    // Load saved custom templates
    if (data.SavedPresets) {
      const parsed = JSON.parse(data.SavedPresets)
      templates.value = [...parsed, ...defaultTemplates]
    }

    for (const key in themeColors.value) {
      if (data[key]) {
        themeColors.value[key].value = data[key]
      }
    }
    
    applyThemeToDocument()
  } catch (err) {
    console.error(err)
  } finally {
    isLoading.value = false
  }
}

const previewColor = (key, val) => {
  const variable = themeColors.value[key].variable
  document.documentElement.style.setProperty(variable, val)
}

const onCustomColorChange = async (key, val) => {
  previewColor(key, val);
  // Auto-save on custom color change silently to prevent spamming generic success message
  await saveThemeToBackend(false);
}

const applyThemeToDocument = () => {
  for (const key in themeColors.value) {
    document.documentElement.style.setProperty(themeColors.value[key].variable, themeColors.value[key].value)
  }
}

const resetToDefault = async () => {
  for (const key in themeColors.value) {
    themeColors.value[key].value = themeColors.value[key].default
    previewColor(key, themeColors.value[key].default)
  }
  selectedPreset.value = ''
  await saveThemeToBackend(true);
}

const saveAndAddFavorite = async () => {
  try {
     const { value: presetName } = await ElMessageBox.prompt(
       t('Save and name your color theme:', 'Lưu và Đặt tên cho Hệ màu của bạn:'),
       t('Save Theme', 'Lưu Hệ Màu'), {
       confirmButtonText: t('Save to Favorites', 'Lưu vào Yêu thích'),
       cancelButtonText: t('Cancel', 'Hủy'),
       inputPattern: /.+/,
       inputErrorMessage: t('Name cannot be empty', 'Tên không được trống'),
       inputPlaceholder: t('Example: My custom theme', 'Ví dụ: Hệ màu của tui')
     })
     
     if (presetName) {
        isSaving.value = true
        const customObj = { name: presetName }
        for (const key in themeColors.value) {
           customObj[key] = themeColors.value[key].value
        }
        
        // Add to combobox list
        templates.value.unshift(customObj)
        selectedPreset.value = presetName
        
        await saveThemeToBackend(false)
        ElMessage.success(t(`Theme saved and [${presetName}] added to the list!`, `Đã tự động lưu Hệ màu và thêm [${presetName}] vào danh sách!`))
     }
  } catch (err) {
    if (err !== 'cancel') {
       console.error(err)
       ElMessage.error(t('An error occurred while saving.', 'Có lỗi xảy ra khi lưu.'))
    }
  } finally {
    isSaving.value = false
  }
}
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

.settings-card {
  background-color: var(--bg-card);
  backdrop-filter: blur(12px);
  -webkit-backdrop-filter: blur(12px);
  border: 1px solid var(--border-color);
  border-radius: 12px;
  padding: 24px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
}

.card-title {
  font-size: 18px;
  font-weight: 600;
  color: var(--text-primary);
  margin-bottom: 8px;
}

.section-desc {
  font-size: 13px;
  color: #8b949e;
  margin-bottom: 20px;
}

.mt-24 {
  margin-top: 24px;
}

.color-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 20px;
}

.color-setting {
  background-color: rgba(128, 128, 128, 0.1);
  border: 1px solid var(--border-color);
  border-radius: 8px;
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.color-label {
  font-weight: 600;
  color: var(--text-primary);
  font-size: 14px;
}

.color-picker-wrapper {
  display: flex;
  align-items: center;
  gap: 12px;
}

.color-hex {
  font-family: monospace;
  background-color: rgba(128, 128, 128, 0.15);
  padding: 4px 8px;
  border-radius: 4px;
  font-size: 13px;
  color: var(--text-primary);
  border: 1px solid var(--border-color);
}

.action-footer {
  margin-top: 32px;
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}
</style>
