<template>
  <div class="dashboard-layout">
    <!-- Navbar -->
    <header class="top-nav">
      <div class="nav-left">
        <button class="menu-toggle" @click="sidebarVisible = !sidebarVisible">
          <i class="fa-solid fa-bars"></i>
        </button>
        <router-link to="/dashboard" class="nav-brand">
          <img :src="logoImg" alt="SprintA Logo" class="nav-logo" />
          <span>SprintA</span>
        </router-link>
        <span class="nav-link active desktop-only">Audit Log</span>
      </div>

      <div class="nav-right">
        <NotificationsDropdown class="desktop-only" />
        <UserDropdown />
      </div>
    </header>

    <div class="main-body">
      <!-- Sidebar (Consistent with Dashboard) -->
      <aside class="sidebar" :class="{ 'mobile-show': sidebarVisible }">
        <ul class="side-menu">
          <li @click="router.push('/dashboard')"><i class="fa-solid fa-border-all"></i> Dành cho bạn</li>
          <li v-if="sidebarPreferences.spaces" @click="router.push('/dashboard')"><i class="fa-solid fa-folder-open"></i> Không gian</li>
          <li v-if="sidebarPreferences.recent" @click="router.push('/dashboard')"><i class="fa-solid fa-clock"></i> Gần đây</li>
          <li v-if="sidebarPreferences.ai" @click="router.push('/ai-assistant')"><i class="fa-solid fa-robot"></i> Trợ lý AI</li>
          <li v-if="sidebarPreferences.audit" class="active"><i class="fa-solid fa-list-check"></i> Audit Log</li>
          <li v-if="sidebarPreferences.users" @click="router.push('/user-management')"><i class="fa-solid fa-users-gear"></i> Quản lý người dùng</li>

          <li class="more-dropdown-wrapper" style="padding: 0; background: transparent !important; margin-bottom: 4px;">
            <el-dropdown trigger="click" placement="bottom-start" popper-class="custom-sidebar-dropdown" style="width: 100%;">
              <div class="sidebar-more-trigger">
                <i class="fa-solid fa-ellipsis"></i> Thêm
              </div>
              <template #dropdown>
                <el-dropdown-menu class="jira-more-menu" style="background-color: var(--bg-card); border: 1px solid var(--border-color); border-radius: 4px; padding: 4px 0; width: 220px;">
                  <el-dropdown-item v-if="!sidebarPreferences.spaces">
                    <div @click="router.push('/dashboard')" style="display: flex; align-items: center; gap: 12px; color: #b3bac5; font-size: 14px; padding: 4px 8px; width: 100%;">
                      <i class="fa-solid fa-folder-open"></i>
                      <span>Không gian</span>
                    </div>
                  </el-dropdown-item>
                  <el-dropdown-item v-if="!sidebarPreferences.recent">
                    <div @click="router.push('/dashboard')" style="display: flex; align-items: center; gap: 12px; color: #b3bac5; font-size: 14px; padding: 4px 8px; width: 100%;">
                      <i class="fa-solid fa-clock"></i>
                      <span>Gần đây</span>
                    </div>
                  </el-dropdown-item>
                  <el-dropdown-item v-if="!sidebarPreferences.ai">
                    <div @click="router.push('/ai-assistant')" style="display: flex; align-items: center; gap: 12px; color: #b3bac5; font-size: 14px; padding: 4px 8px; width: 100%;">
                      <i class="fa-solid fa-robot"></i>
                      <span>Trợ lý AI</span>
                    </div>
                  </el-dropdown-item>
                  <el-dropdown-item v-if="!sidebarPreferences.audit">
                    <div @click="router.push('/audit-log')" style="display: flex; align-items: center; gap: 12px; color: #b3bac5; font-size: 14px; padding: 4px 8px; width: 100%;">
                      <i class="fa-solid fa-list-check"></i>
                      <span>Audit Log</span>
                    </div>
                  </el-dropdown-item>
                  <el-dropdown-item v-if="!sidebarPreferences.users">
                    <div @click="router.push('/user-management')" style="display: flex; align-items: center; gap: 12px; color: #b3bac5; font-size: 14px; padding: 4px 8px; width: 100%;">
                      <i class="fa-solid fa-users-gear"></i>
                      <span>Quản lý người dùng</span>
                    </div>
                  </el-dropdown-item>
                  <el-dropdown-item v-if="!sidebarPreferences.spaces || !sidebarPreferences.recent || !sidebarPreferences.ai || !sidebarPreferences.audit" divided></el-dropdown-item>

                  <el-dropdown-item>
                    <div @click="showCustomizeModal = true" style="display: flex; align-items: center; gap: 12px; color: #b3bac5; font-size: 14px; padding: 4px 8px; width: 100%;">
                      <i class="fa-solid fa-sliders"></i>
                      <span>Kiểm soát thanh bên</span>
                    </div>
                  </el-dropdown-item>
                </el-dropdown-menu>
              </template>
            </el-dropdown>
          </li>
        </ul>
      </aside>

      <!-- Main Content -->
      <main class="content-area">
        <div class="content-wrapper">
          <div class="page-header-flex">
            <h1 class="page-title">Giao diện trang Audit Log</h1>
            <div class="page-actions">
              <el-button type="primary" @click="exportToCSV">
                <i class="fa-solid fa-download" style="margin-right: 8px;"></i> Export CSV
              </el-button>
            </div>
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
                  <label>Resource</label>
                  <el-select v-model="filters.resource" placeholder="Chọn resource" clearable style="width: 100%">
                    <el-option label="Tasks" value="tasks" />
                    <el-option label="Projects" value="projects" />
                    <el-option label="Members" value="members" />
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
        </div>
      </main>
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
import logoImg from '../assets/logo_QLCV.png'
import UserDropdown from '../components/UserDropdown.vue'
import NotificationsDropdown from '../components/NotificationsDropdown.vue'
import CustomizeSidebarModal from '../components/CustomizeSidebarModal.vue'
import { ElMessage } from 'element-plus'

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
  keyword: ''
})

// Mock Data
const auditLogs = ref([
  { id: 'LOG-001', timestamp: '2026-03-30 14:20:05', user: 'Admin', action: 'update', resource: 'Project', targetId: 'PRJ-101', status: 'success', summary: 'Cập nhật cấu hình bảo mật dự án "SprintA"', details: { old: { visibility: 'public' }, new: { visibility: 'private' } } },
  { id: 'LOG-002', timestamp: '2026-03-30 15:10:12', user: 'Manager', action: 'create', resource: 'Task', targetId: 'TASK-552', status: 'success', summary: 'Tạo công việc "Thiết kế trang Audit Log"', details: { title: 'Thiết kế trang Audit Log', priority: 'High', assignee: 'Dev-01' } },
  { id: 'LOG-003', timestamp: '2026-03-30 15:45:00', user: 'Developer', action: 'delete', resource: 'Task', targetId: 'TASK-102', status: 'failure', summary: 'Thử xóa công việc không có quyền hạn', details: { error: 'Permission Denied', errorCode: 403 } },
  { id: 'LOG-004', timestamp: '2026-03-30 16:05:22', user: 'Admin', action: 'login', resource: 'Auth', targetId: 'USR-AD1', status: 'success', summary: 'Đăng nhập từ IP 192.168.1.10', details: { ip: '192.168.1.10', browser: 'Chrome', os: 'Windows 11' } },
  { id: 'LOG-005', timestamp: '2026-03-30 16:30:15', user: 'Dev-01', action: 'update', resource: 'Member', targetId: 'MB-05', status: 'success', summary: 'Thay đổi vai trò thành viên sang Developer', details: { prevRole: 'Guest', newRole: 'Developer' } },
])

const filteredLogs = computed(() => {
  return auditLogs.value.filter(log => {
    const matchUser = !filters.value.user || log.user.toLowerCase().includes(filters.value.user.toLowerCase())
    const matchAction = !filters.value.action || log.action === filters.value.action
    const matchResource = !filters.value.resource || log.resource.toLowerCase().includes(filters.value.resource.toLowerCase())
    const matchKeyword = !filters.value.keyword || 
                         log.id.toLowerCase().includes(filters.value.keyword.toLowerCase()) ||
                         log.summary.toLowerCase().includes(filters.value.keyword.toLowerCase())
    return matchUser && matchAction && matchResource && matchKeyword
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

onMounted(() => {
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
      console.error('Error parsing sidebar preferences:', e)
    }
  }
})

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
.dashboard-layout {
  height: 100vh;
  display: flex;
  flex-direction: column;
  background-color: var(--bg-layout);
  color: var(--text-primary);
  overflow: hidden;
}

.top-nav {
  height: 56px;
  background-color: var(--bg-nav);
  border-bottom: 1px solid var(--border-color);
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 20px;
  flex-shrink: 0;
}

.nav-left { display: flex; align-items: center; gap: 12px; }
.nav-brand { display: flex; align-items: center; gap: 8px; color: var(--text-primary); text-decoration: none; font-weight: 800; font-size: 20px; }
.nav-logo { height: 28px; }
.nav-link { color: var(--text-secondary); font-size: 14px; font-weight: 600; padding-left: 16px; border-left: 1px solid var(--border-color); }

.nav-right { display: flex; align-items: center; gap: 16px; }

.main-body { display: flex; flex: 1; overflow: hidden; }

.sidebar {
  width: 240px;
  background-color: var(--bg-sidebar);
  border-right: 1px solid var(--border-color);
  padding: 24px 16px;
  flex-shrink: 0;
}

.side-menu { list-style: none; padding: 0; margin: 0; }
.side-menu li {
  padding: 10px 12px; border-radius: 6px; color: var(--text-secondary); font-size: 14px; font-weight: 500; margin-bottom: 4px; cursor: pointer; display: flex; align-items: center; gap: 12px;
}
.side-menu li:hover { background-color: var(--hover-bg); color: var(--text-primary); }
.side-menu li.active { background-color: var(--active-bg); color: #3b82f6; }

.content-area { flex: 1; background-color: var(--bg-content); padding: 40px; overflow-y: auto; }
.content-wrapper { max-width: 1400px; margin: 0 auto; }

.page-header-flex { display: flex; justify-content: space-between; align-items: center; margin-bottom: 32px; }
.page-title { font-size: 26px; font-weight: 700; color: var(--text-primary); margin: 0; }

.filters-card {
  background: var(--bg-card);
  border: 1px solid var(--border-color);
  border-radius: 8px;
  padding: 20px;
  margin-bottom: 24px;
}

.filter-item { margin-bottom: 16px; }
.filter-item label { display: block; font-size: 12px; font-weight: 600; color: var(--text-muted); margin-bottom: 8px; text-transform: uppercase; }

.table-container {
  background: var(--bg-card);
  border: 1px solid var(--border-color);
  border-radius: 8px;
  padding: 0;
  overflow: hidden;
}

.pagination-footer { padding: 16px; display: flex; justify-content: flex-end; border-top: 1px solid var(--border-color); }

/* Custom Element Plus theme styles local to this component */
:deep(.el-table) {
  --el-table-bg-color: var(--bg-card);
  --el-table-tr-bg-color: var(--bg-card);
  --el-table-header-bg-color: var(--bg-layout);
  --el-table-header-text-color: var(--text-secondary);
  --el-table-text-color: var(--text-primary);
  --el-table-border-color: var(--border-color);
  --el-table-row-hover-bg-color: var(--hover-bg);
}

:deep(.el-input__wrapper), :deep(.el-select__wrapper) {
  background-color: var(--bg-secondary) !important;
  box-shadow: 0 0 0 1px var(--border-color) inset !important;
}

:deep(.el-input__inner) {
  color: var(--text-primary) !important;
}

:deep(.el-table td.el-table__cell), :deep(.el-table th.el-table__cell) {
  border-bottom: 1px solid var(--border-color) !important;
}

:deep(.el-pagination) {
  --el-pagination-bg-color: transparent;
  --el-pagination-hover-color: #3b82f6;
  color: var(--text-secondary);
}

/* Drawer styles */
.log-details-content { padding: 0 20px; }
.detail-item { margin-bottom: 20px; display: flex; flex-direction: column; gap: 8px; }
.detail-item .label { font-size: 13px; font-weight: 600; color: var(--text-muted); }
.detail-item .value { font-size: 15px; color: var(--text-primary); }

.user-info-flex { display: flex; align-items: center; gap: 12px; }
.user-avatar-small { 
  width: 28px; height: 28px; background: #3b82f6; border-radius: 4px; display: flex; align-items: center; justify-content: center; font-size: 11px; font-weight: 700; color: white;
}

.json-viewer {
  background: var(--bg-layout); border-radius: 8px; padding: 16px; overflow: auto; max-height: 400px;
  border: 1px solid var(--border-color);
}
.json-viewer pre { color: #10b981; font-family: 'Fira Code', 'Courier New', monospace; font-size: 13px; margin: 0; }

.menu-toggle { display: none; background: transparent; border: none; color: var(--text-secondary); font-size: 20px; cursor: pointer; }

@media (max-width: 1024px) {
  .sidebar { position: fixed; left: -240px; top: 56px; bottom: 0; z-index: 1001; transition: left 0.3s ease; }
  .sidebar.mobile-show { left: 0; }
  .menu-toggle { display: block; }
}
</style>
