<template>
  <div>
    <div class="page-header">
      <h1 class="text-hero">Audit Log</h1>
      <p class="text-desc">Review and track project activities, changes, and access logs.</p>
    </div>

          <!-- Filters Section -->
          <div class="filters-card">
            <el-row :gutter="16">
              <el-col :xs="24" :sm="12" :md="6">
                <div class="filter-item">
                  <label>Thời gian</label>
                  <el-date-picker
                    v-model="filters.dateRange"
                    type="daterange"
                    range-separator="-"
                    start-placeholder="Từ ngày"
                    end-placeholder="Đến ngày"
                    style="width: 100%"
                  />
                </div>
              </el-col>
              <el-col :xs="24" :sm="12" :md="4">
                <div class="filter-item">
                  <label>Người dùng</label>
                  <el-select v-model="filters.user" placeholder="Chọn user" clearable style="width: 100%">
                    <el-option label="Admin" value="admin" />
                    <el-option label="Manager" value="manager" />
                    <el-option label="Developer" value="dev" />
                  </el-select>
                </div>
              </el-col>
              <el-col :xs="24" :sm="12" :md="4">
                <div class="filter-item">
                  <label>Loại Action</label>
                  <el-select v-model="filters.action" placeholder="Chọn action" clearable style="width: 100%">
                    <el-option label="Create" value="create" />
                    <el-option label="Update" value="update" />
                    <el-option label="Delete" value="delete" />
                    <el-option label="Login" value="login" />
                  </el-select>
                </div>
              </el-col>
              <el-col :xs="24" :sm="12" :md="4">
                <div class="filter-item">
                  <label>Dự án</label>
                  <el-select v-model="filters.projectId" placeholder="Tất cả dự án" clearable filterable style="width: 100%">
                    <el-option
                      v-for="p in projectList"
                      :key="p.id"
                      :label="`[${p.key || 'PRJ'}] ${p.name}`"
                      :value="p.id"
                    />
                  </el-select>
                </div>
              </el-col>

              <el-col :xs="24" :sm="12" :md="6">
                <div class="filter-item">
                  <label>Tìm kiếm</label>
                  <el-input
                    v-model="filters.keyword"
                    placeholder="Tìm theo keyword hoặc ID..."
                    :prefix-icon="Search"
                    clearable
                  />
                </div>
              </el-col>
            </el-row>
          </div>

          <!-- Data Table -->
          <div class="table-container jira-card">
            <el-table 
              :data="paginatedLogs" 
              style="width: 100%" 
              v-loading="loading"
              @row-click="handleRowClick"
              highlight-current-row
            >
              <el-table-column prop="timestamp" label="Thời gian" width="180" />
              <el-table-column prop="user" label="User" width="150" />
              <el-table-column prop="action" label="Action" width="120">
                <template #default="scope">
                  <el-tag :type="getActionTagType(scope.row.action)">
                    {{ scope.row.action.toUpperCase() }}
                  </el-tag>
                </template>
              </el-table-column>
              <el-table-column prop="resource" label="Resource" width="150" />
              <el-table-column prop="targetId" label="ID" width="100" />
              <el-table-column prop="status" label="Status" width="120">
                <template #default="scope">
                  <el-tag :type="scope.row.status === 'success' ? 'success' : 'danger'">
                    {{ scope.row.status }}
                  </el-tag>
                </template>
              </el-table-column>
              <el-table-column prop="summary" label="Tóm tắt" min-width="250" show-overflow-tooltip />
              <el-table-column fixed="right" label="Thao tác" width="100">
                <template #default="scope">
                  <el-button link type="primary" size="small" @click.stop="handleRowClick(scope.row)">
                    Chi tiết
                  </el-button>
                </template>
              </el-table-column>
            </el-table>

            <div class="pagination-footer">
              <el-pagination
                v-model:current-page="currentPage"
                v-model:page-size="pageSize"
                :page-sizes="[10, 20, 50, 100]"
                layout="total, sizes, prev, pager, next, jumper"
                :total="filteredLogs.length"
                @size-change="handleSizeChange"
                @current-change="handleCurrentChange"
              />
            </div>
          </div>


    <!-- Details Drawer -->
    <el-drawer
      v-model="drawerVisible"
      title="Chi tiết Log"
      direction="rtl"
      size="40%"
      custom-class="audit-drawer"
    >
      <div v-if="selectedLog" class="log-details-content">
        <div class="detail-item">
          <span class="label">Mã định danh (ID):</span>
          <span class="value">{{ selectedLog.id }}</span>
        </div>
        <div class="detail-item">
          <span class="label">Loại Action:</span>
          <span class="value"><el-tag :type="getActionTagType(selectedLog.action)">{{ selectedLog.action }}</el-tag></span>
        </div>
        <div class="detail-item">
          <span class="label">Thời gian thực hiện:</span>
          <span class="value">{{ selectedLog.timestamp }}</span>
        </div>
        <div class="detail-item">
          <span class="label">Người thực hiện:</span>
          <div class="user-info-flex">
            <div class="user-avatar-small">{{ selectedLog.user.substring(0, 2).toUpperCase() }}</div>
            <span class="value">{{ selectedLog.user }}</span>
          </div>
        </div>
        <div class="detail-item">
          <span class="label">Status:</span>
          <span class="value"><el-tag :type="selectedLog.status === 'success' ? 'success' : 'danger'">{{ selectedLog.status }}</el-tag></span>
        </div>
        <div class="detail-item">
          <span class="label">Resource:</span>
          <span class="value">{{ selectedLog.resource }} (ID: {{ selectedLog.targetId }})</span>
        </div>
        
        <el-divider />
        
        <div class="detail-section">
          <span class="label">Nội dung thay đổi:</span>
          <div class="json-viewer">
            <pre>{{ JSON.stringify(selectedLog.details, null, 2) }}</pre>
          </div>
        </div>
      </div>
    </el-drawer>

    <!-- Customize Sidebar Modal -->
    <CustomizeSidebarModal :visible="showCustomizeModal" @update:visible="showCustomizeModal = $event" @saved="handleSidebarSaved" />
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { Search } from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'
import logoImg from '../assets/logo_QLCV.png'
import UserDropdown from '../components/UserDropdown.vue'
import NotificationsDropdown from '../components/NotificationsDropdown.vue'
import CustomizeSidebarModal from '../components/CustomizeSidebarModal.vue'

import axiosClient from '../api/axiosClient'
import { canAccessAdminUserDirectory, getStoredUser, hasSystemAdminAccess } from '@/utils/permissions'

const router = useRouter()
const sidebarVisible = ref(false)
const loading = ref(false)
const drawerVisible = ref(false)
const selectedLog = ref(null)
const currentPage = ref(1)
const pageSize = ref(10)
const showCustomizeModal = ref(false)

const sidebarPreferences = ref({
  audit: true,
  users: true
})

const filters = ref({
  dateRange: [],
  user: '',
  action: '',
  resource: '',
  keyword: '',
  projectId: null
})

const auditLogs = ref([])

const projectList = ref([])

onMounted(async () => {
  // Admin/PM guard - chỉ Admin và PM mới được truy cập trang này
  const currentUser = getStoredUser()
  const isAuthorized = hasSystemAdminAccess(currentUser) || canAccessAdminUserDirectory(currentUser)
  
  if (!isAuthorized) {
    ElMessage.error('Bạn không có quyền truy cập trang Audit Log.')
    router.push('/dashboard')
    return
  }

  // restore sidebar settings
  const saved = localStorage.getItem('sidebarPreferences')
  if (saved) {
    try {
      const prefs = JSON.parse(saved)
      const newPrefs = { ...sidebarPreferences.value }
      if (prefs && prefs.navItems) {
        prefs.navItems.forEach(item => {
          if (['recent', 'spaces', 'ai', 'audit', 'users'].includes(item.id)) {
            newPrefs[item.id] = item.checked
          }
        })
      } else {
        Object.assign(newPrefs, prefs)
      }
      sidebarPreferences.value = newPrefs
    } catch (e) {
      console.error('Error parsing sidebar:', e)
    }
  }

  await fetchProjects()
  await fetchLogs()
})

const fetchProjects = async () => {
    try {
        const [discoveryRes, archivedRes] = await Promise.all([
            axiosClient.get('/projects/discovery'),
            axiosClient.get('/projects/archived')
        ])
        const activeProjects = (discoveryRes.data?.data || []).map(p => ({ ...p, isArchived: false }))
        const archivedProjects = (archivedRes.data?.data || []).map(p => ({ ...p, isArchived: true }))
        projectList.value = [...activeProjects, ...archivedProjects]
    } catch(e) {
        console.error('Error fetching projects', e)
    }
}

const fetchLogs = async () => {
  loading.value = true
  try {
    const params = {}
    if (filters.value.projectId) {
        params.projectId = filters.value.projectId
    }
    if (filters.value.keyword) {
        params.search = filters.value.keyword
    }
    
    const { data } = await axiosClient.get('/auditlogs', { params })
    auditLogs.value = data.data.items || []
  } catch (error) {
    if (error.response?.status === 403) {
      ElMessage.error(error.response.data?.message || 'Bạn không có quyền truy cập trang Audit Log.')
      router.push('/dashboard')
    } else {
      console.error('Lỗi khi tải Audit Logs', error)
    }
  } finally {
    loading.value = false
  }
}

// Watch filters for auto-fetch
watch(() => filters.value.projectId, () => {
    fetchLogs()
})
watch(() => filters.value.keyword, () => {
    // Debounce would be nice, but simple for now
    fetchLogs()
})

const filteredLogs = computed(() => {
  return auditLogs.value.filter(log => {
    const matchUser = !filters.value.user || log.user.toLowerCase().includes(filters.value.user.toLowerCase())
    const matchAction = !filters.value.action || log.action === filters.value.action
    const matchResource = !filters.value.resource || log.resource.toLowerCase().includes(filters.value.resource.toLowerCase())
    // keyword is now handled by backend
    return matchUser && matchAction && matchResource
  })
})


const paginatedLogs = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value
  const end = start + pageSize.value
  return filteredLogs.value.slice(start, end)
})

const getActionTagType = (action) => {
  switch (action) {
    case 'create': return 'success'
    case 'update': return 'warning'
    case 'delete': return 'danger'
    case 'login': return 'info'
    default: return ''
  }
}

const handleRowClick = (row) => {
  selectedLog.value = row
  drawerVisible.value = true
}

const handleSizeChange = (val) => {
  pageSize.value = val
}

const handleCurrentChange = (val) => {
  currentPage.value = val
}

const exportToCSV = () => {
  ElMessage.success('Đang chuẩn bị tệp CSV để tải xuống...')
  // Placeholder for real export logic
}



const handleSidebarSaved = (prefs) => {
  const newPrefs = { ...sidebarPreferences.value }
  if (prefs && prefs.navItems) {
    prefs.navItems.forEach(item => {
      if (['recent', 'spaces', 'ai', 'audit', 'users'].includes(item.id)) {
        newPrefs[item.id] = item.checked
      }
    })
  }
  sidebarPreferences.value = newPrefs
  localStorage.setItem('sidebarPreferences', JSON.stringify(newPrefs))
}
</script>

<style scoped>


.page-header-flex { display: flex; justify-content: space-between; align-items: center; margin-bottom: 32px; }
.page-title { font-size: 26px; font-weight: 700; color: var(--color-text-primary); margin: 0; }

.filters-card {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 2px;
  padding: 20px;
  margin-bottom: 24px;
}

.filter-item { margin-bottom: 16px; }
.filter-item label { display: block; font-size: 12px; font-weight: 600; color: var(--color-text-muted); margin-bottom: 8px; text-transform: uppercase; }

.table-container {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 2px;
  padding: 0;
  overflow: hidden;
}

.pagination-footer { padding: 16px; display: flex; justify-content: flex-end; border-top: 1px solid var(--border-color); }

/* Custom Element Plus theme styles local to this component */
:deep(.el-table) {
  --el-table-bg-color: var(--table-bg);
  --el-table-tr-bg-color: var(--table-bg);
  --el-table-header-bg-color: var(--bg-secondary);
  --el-table-header-text-color: var(--text-secondary);
  --el-table-text-color: var(--text-primary);
  --el-table-border-color: var(--border-color);
  --el-table-row-hover-bg-color: var(--hover-bg);
}

:deep(.el-input__wrapper), :deep(.el-select__wrapper) {
  background-color: var(--input-bg) !important;
  box-shadow: 0 0 0 1px var(--border-color) inset !important;
}

::v-deep(.el-input__inner) {
  color: var(--text-primary) !important;
}

::v-deep(.el-table td.el-table__cell), ::v-deep(.el-table th.el-table__cell) {
  border-bottom: 1px solid var(--border-color) !important;
}

::v-deep(.el-pagination) {
  --el-pagination-bg-color: transparent;
  --el-pagination-hover-color: var(--color-accent);
  color: var(--color-text-secondary);
}

/* Drawer styles */
.log-details-content { padding: 0 20px; }
.detail-item { margin-bottom: 20px; display: flex; flex-direction: column; gap: 8px; }
.detail-item .label { font-size: 13px; font-weight: 600; color: var(--color-text-muted); }
.detail-item .value { font-size: 15px; color: var(--color-text-primary); }

.user-info-flex { display: flex; align-items: center; gap: 12px; }
.user-avatar-small { 
  width: 28px; height: 28px; background: var(--color-accent); border-radius: 2px; display: flex; align-items: center; justify-content: center; font-size: 11px; font-weight: 700; color: white;
}

.json-viewer {
  background: var(--bg-primary); border-radius: 2px; padding: 16px; overflow: auto; max-height: 400px;
  border: 1px solid var(--border-color);
}
.json-viewer pre { color: #10b981; font-family: 'Fira Code', 'Courier New', monospace; font-size: 13px; margin: 0; }


</style>


