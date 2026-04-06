<template>
  <NexusLayout>
    <div class="manage-spaces-page">
      <!-- Header -->
      <div class="page-header">
        <h1 class="page-title">Manage Spaces</h1>
        <div class="header-actions">
          <el-button type="primary" class="create-space-btn" @click="handleCreateSpace">
            Create space
          </el-button>
          <el-button @click="handleTemplates">Templates</el-button>
          <el-input
            v-model="adminSearch"
            placeholder="Search SprintA admin"
            class="admin-search"
            :prefix-icon="Search"
          />
        </div>
      </div>

      <!-- Filters -->
      <div class="filters-bar">
        <el-input
          v-model="searchQuery"
          placeholder="Search spaces"
          class="spaces-search"
          :prefix-icon="Search"
        />
        <el-select v-model="filterApp" placeholder="Filter by app" class="filter-app" clearable>
          <el-option label="SprintA Software" value="jira" />
          <el-option label="Confluence" value="confluence" />
        </el-select>
      </div>

      <!-- Spaces Table -->
      <div class="table-container jira-card">
        <el-table 
          v-loading="loading"
          :data="paginatedSpaces"
          style="width: 100%"
          class="spaces-table"
        >
          <!-- Star Column -->
          <el-table-column width="45">
            <template #default="scope">
              <i class="fa-star star-icon" :class="scope.row.starred ? 'fa-solid starred' : 'fa-regular'"></i>
            </template>
          </el-table-column>

          <!-- Name Column -->
          <el-table-column prop="name" label="Name" sortable min-width="250">
            <template #default="scope">
              <div class="name-col" @click="goToSpace(scope.row.id)">
                <div class="space-icon-box" :style="{ background: '#3b82f6' }">
                  <i class="fa-solid fa-folder"></i>
                </div>
                <span class="space-name">{{ scope.row.name }}</span>
              </div>
            </template>
          </el-table-column>

          <!-- Key Column -->
          <el-table-column prop="key" label="Key" min-width="120" />

          <!-- Type Column -->
          <el-table-column prop="type" label="Type" min-width="200" />

          <!-- Lead Column -->
          <el-table-column prop="lead" label="Lead" min-width="200">
            <template #default="scope">
              <div class="lead-col">
                <div class="avatar" v-if="scope.row.leadName">
                  {{ scope.row.leadName.substring(0, 1).toUpperCase() }}
                </div>
                <div class="avatar" v-else>U</div>
                <span class="lead-name">{{ scope.row.leadName || 'Unknown User' }}</span>
              </div>
            </template>
          </el-table-column>

          <!-- Last Update Column -->
          <el-table-column prop="lastUpdate" label="Last work update" min-width="180" />

          <!-- Actions / Space URL -->
          <el-table-column label="Space URL" width="100" align="center">
            <template #default>
              <el-button link class="action-dots">
                <i class="fa-solid fa-ellipsis"></i>
              </el-button>
            </template>
          </el-table-column>
        </el-table>

        <!-- Pagination -->
        <div class="pagination-footer">
          <el-pagination
            v-model:current-page="currentPage"
            v-model:page-size="pageSize"
            layout="prev, pager, next"
            :total="filteredSpaces.length"
          />
        </div>
      </div>
    </div>
  </NexusLayout>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { Search } from '@element-plus/icons-vue'
import axiosClient from '@/api/axiosClient'
import NexusLayout from '@/components/layout/NexusLayout.vue'
import { ElMessage } from 'element-plus'

const router = useRouter()
const loading = ref(false)
const spaces = ref([])
const adminSearch = ref('')
const searchQuery = ref('')
const filterApp = ref('')
const currentPage = ref(1)
const pageSize = ref(10)

const fetchSpaces = async () => {
  loading.value = true
  try {
    const response = await axiosClient.get('/projects')
    const data = response.data.data || response.data || []
    
    // Transform data to match required UI columns
    spaces.value = data.map(p => ({
      id: p.id,
      starred: false,
      name: p.name,
      key: p.key || p.name.substring(0, 4).toUpperCase(),
      type: p.type || 'Team-managed software',
      leadName: p.leadName || p.reporterName || 'Cường Đoàn',
      lastUpdate: '13 hours ago', // Mock
      originalRow: p
    }))
  } catch (error) {
    console.error('Fetch spaces error:', error)
    ElMessage.error('Không thể tải danh sách không gian')
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  fetchSpaces()
})

const filteredSpaces = computed(() => {
  return spaces.value.filter(s => {
    const matchSearch = !searchQuery.value || s.name.toLowerCase().includes(searchQuery.value.toLowerCase())
    return matchSearch
  })
})

const paginatedSpaces = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value
  const end = start + pageSize.value
  return filteredSpaces.value.slice(start, end)
})

const goToSpace = (id) => {
  router.push(`/space/${id}`)
}

const handleCreateSpace = () => {
  ElMessage.info('Create space dialog would open here.')
}

const handleTemplates = () => {
  ElMessage.info('Templates library would open here.')
}
</script>

<style scoped>
.manage-spaces-page {
  padding: 24px;
  max-width: 1200px;
  margin: 0 auto;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
}

.page-title {
  font-size: 24px;
  font-weight: 500;
  color: var(--text-primary);
  margin: 0;
}

.header-actions {
  display: flex;
  align-items: center;
  gap: 12px;
}

.admin-search {
  width: 200px;
}

.filters-bar {
  display: flex;
  align-items: center;
  gap: 16px;
  margin-bottom: 16px;
}

.spaces-search {
  width: 280px;
}

.filter-app {
  width: 180px;
}

.table-container {
  background: var(--bg-card);
  border: 1px solid var(--border-color);
  border-radius: 4px;
  overflow: hidden;
  box-shadow: 0 1px 3px rgba(0,0,0,0.05);
}

.spaces-table {
  border-bottom: 1px solid var(--border-color);
}

.star-icon {
  font-size: 16px;
  color: var(--text-muted);
  cursor: pointer;
  transition: color 0.2s;
}

.star-icon:hover {
  color: #f59e0b;
}

.star-icon.starred {
  color: #f59e0b;
}

.name-col {
  display: flex;
  align-items: center;
  gap: 12px;
  cursor: pointer;
}

.name-col:hover .space-name {
  text-decoration: underline;
  color: #0052cc;
}

.space-icon-box {
  width: 26px;
  height: 26px;
  border-radius: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-size: 12px;
}

.space-name {
  font-weight: 500;
  color: #0052cc; /* SprintA brand color for links */
}

.lead-col {
  display: flex;
  align-items: center;
  gap: 8px;
}

.avatar {
  width: 24px;
  height: 24px;
  background-color: #6b21a8;
  color: white;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 10px;
  font-weight: 600;
}

.lead-name {
  font-size: 14px;
  color: var(--text-primary);
}

.action-dots {
  color: var(--text-muted);
  font-size: 18px;
}

.action-dots:hover {
  color: var(--text-primary);
  background-color: var(--hover-bg);
  border-radius: 4px;
}

.pagination-footer {
  padding: 12px 16px;
  display: flex;
  justify-content: center;
}

/* Custom Element Plus theme overrides for this specific page */
:deep(.el-table) {
  --el-table-header-bg-color: #f4f5f7;
  --el-table-header-text-color: #5e6c84;
  --el-table-row-hover-bg-color: #f4f5f7;
  --el-table-border-color: #dfe1e6;
}

.dark :deep(.el-table) {
  --el-table-header-bg-color: #1e293b;
  --el-table-header-text-color: #94a3b8;
  --el-table-row-hover-bg-color: #334155;
  --el-table-border-color: #334155;
}

:deep(.el-table th.el-table__cell) {
  font-size: 12px;
  font-weight: 600;
  text-transform: none;
  border-bottom: 2px solid var(--border-color) !important;
}

:deep(.el-table td.el-table__cell) {
  padding: 8px 0;
  border-bottom: 1px solid var(--border-color) !important;
}

:deep(.el-button--primary.create-space-btn) {
  background-color: #0052cc;
  border-color: #0052cc;
}

:deep(.el-button--primary.create-space-btn:hover) {
  background-color: #0065ff;
  border-color: #0065ff;
}

.dark :deep(.el-button--primary.create-space-btn) {
  background-color: #3b82f6;
  border-color: #3b82f6;
}
</style>
