<template>
  <div style="position: relative; height: 100%; display: flex; flex-direction: column;">
    
    <!-- PERMISSION SUMMARY -->
    <div style="padding: 24px 24px 16px 24px; background: var(--el-fill-color-light); border-bottom: 1px solid var(--el-border-color-light);">
      <el-row :gutter="16">
        <el-col :span="6">
          <el-card shadow="never" class="summary-card">
            <div class="summary-title">Modules</div>
            <div class="summary-value">{{ availableModules.length }}</div>
          </el-card>
        </el-col>
        <el-col :span="6">
          <el-card shadow="never" class="summary-card">
            <div class="summary-title">Total Permissions</div>
            <div class="summary-value">{{ allPermissions.length }}</div>
          </el-card>
        </el-col>
        <el-col :span="6">
          <el-card shadow="never" class="summary-card">
            <div class="summary-title">Enabled</div>
            <div class="summary-value" style="color: var(--el-color-success);">{{ currentSelectedIds.length }}</div>
          </el-card>
        </el-col>
        <el-col :span="6">
          <el-card shadow="never" class="summary-card">
            <div class="summary-title">High Risk Enabled</div>
            <div class="summary-value" :style="{ color: highRiskCount > 0 ? 'var(--el-color-danger)' : 'inherit' }">
              {{ highRiskCount }}
            </div>
          </el-card>
        </el-col>
      </el-row>
    </div>

    <!-- FILTER TOOLBAR -->
    <div style="padding: 16px 24px; border-bottom: 1px solid var(--el-border-color-light); display: flex; flex-direction: column; gap: 16px;">
      <div style="display: flex; justify-content: space-between; flex-wrap: wrap; gap: 16px; align-items: center;">
        <el-space :size="16" wrap style="flex: 1;">
          <el-input
            v-model="filters.search"
            placeholder="Search action, module, desc..."
            prefix-icon="Search"
            clearable
            style="width: 260px;"
          />
          <el-select v-model="filters.status" placeholder="Status" style="width: 140px;">
            <el-option label="All Status" value="all" />
            <el-option label="Enabled Only" value="enabled" />
            <el-option label="Disabled Only" value="disabled" />
          </el-select>
          <el-select v-model="filters.modules" placeholder="Modules" clearable multiple collapse-tags collapse-tags-tooltip style="width: 200px;">
            <el-option v-for="m in availableModules" :key="m" :label="formatModuleName(m)" :value="m" />
          </el-select>
          <el-select v-model="filters.risk" placeholder="Risk Level" clearable style="width: 140px;">
            <el-option label="Critical Risk" :value="4" />
            <el-option label="High Risk" :value="3" />
            <el-option label="Medium Risk" :value="2" />
            <el-option label="Low Risk" :value="1" />
          </el-select>
          <el-select v-model="filters.action" placeholder="Action Type" clearable style="width: 140px;">
            <el-option v-for="a in allDistinctActions" :key="a" :label="formatActionNameStr(a)" :value="a" />
          </el-select>
        </el-space>

        <el-space :size="16">
          <el-button type="primary" @click="editorVisible = true">
            <el-icon style="margin-right: 6px;"><FullScreen /></el-icon> Open Permission Editor
          </el-button>
          
          <el-radio-group v-model="viewMode" size="small">
            <el-radio-button label="tree">
              <el-icon style="vertical-align: middle; margin-right: 4px;"><DataBoard /></el-icon> Tree
            </el-radio-button>
            <el-radio-button label="matrix">
              <el-icon style="vertical-align: middle; margin-right: 4px;"><Grid /></el-icon> Matrix
            </el-radio-button>
          </el-radio-group>
        </el-space>
      </div>
      
      <!-- BULK ACTIONS -->
      <div style="display: flex; justify-content: space-between; align-items: center;">
        <el-space :size="12">
          <el-button size="small" @click="bulkEnableFiltered" type="primary" plain :disabled="role.isProtected || filteredTotalCount === 0">Enable Filtered</el-button>
          <el-button size="small" @click="bulkDisableFiltered" type="danger" plain :disabled="role.isProtected || filteredTotalCount === 0">Disable Filtered</el-button>
        </el-space>
        
        <el-space :size="12" v-if="viewMode === 'tree'">
          <el-button size="small" @click="expandAll" text bg>Expand All</el-button>
          <el-button size="small" @click="collapseAll" text bg>Collapse All</el-button>
        </el-space>
      </div>
    </div>

    <!-- MAIN CONTENT AREA -->
    <el-scrollbar style="flex: 1;" ref="scrollbarRef">
      <div style="padding: 24px; max-width: 100%; padding-bottom: 100px;">
        <el-alert
          v-if="role.isProtected"
          title="System Role"
          type="warning"
          description="This role is managed by the system. Its permissions are locked and cannot be modified."
          show-icon
          :closable="false"
          style="margin-bottom: 24px;"
        />
        
        <template v-if="hasResults">
          <!-- TREE VIEW (Accordion) -->
          <div v-if="viewMode === 'tree'" style="max-width: 900px;">
            <el-collapse v-model="activeModules" style="border-top: none; border-bottom: none;">
              <el-collapse-item 
                v-for="(perms, moduleName) in filteredGroupedPermissions" 
                :key="moduleName" 
                :name="moduleName"
                style="margin-bottom: 16px; border: 1px solid var(--el-border-color-light); border-radius: 8px; overflow: hidden;"
              >
                <template #title>
                  <div style="display: flex; align-items: center; justify-content: space-between; width: 100%;">
                    <div style="display: flex; align-items: center; gap: 12px;">
                      <el-checkbox 
                        v-if="perms.length > 0"
                        :model-value="getModuleCheckState(moduleName)"
                        :indeterminate="isModuleIndeterminate(moduleName)"
                        :disabled="role.isProtected"
                        @change="val => toggleModule(moduleName, val)"
                        @click.stop
                      />
                      <el-text style="font-weight: 600; text-transform: uppercase; letter-spacing: 0.05em; color: var(--el-text-color-primary);">
                        {{ formatModuleName(moduleName) }}
                      </el-text>
                    </div>
                    <el-text v-if="perms.length > 0" type="info" size="small">{{ getSelectedCount(moduleName) }} / {{ perms.length }} Selected</el-text>
                    <el-tag v-else type="warning" size="small" effect="plain">No permission definitions available yet</el-tag>
                  </div>
                </template>
                
                <div style="padding: 16px 24px 24px 48px;">
                  <el-row :gutter="24">
                    <el-col :span="8" :xs="24" :sm="12" :md="8" v-for="perm in perms" :key="perm.id" style="margin-bottom: 16px;">
                      <el-tooltip placement="top" effect="dark" :hide-after="50" :open-delay="400">
                        <template #content>
                          <div style="max-width: 260px; font-size: 13px; line-height: 1.5;">
                            <div style="font-weight: bold; margin-bottom: 4px; font-size: 14px;">{{ formatActionName(perm.code) }}</div>
                            <div style="margin-bottom: 8px; color: #a3a6ad;">{{ perm.code }}</div>
                            <div v-if="perm.description" style="margin-bottom: 8px;">{{ perm.description }}</div>
                            <div style="margin-bottom: 4px; display: flex; align-items: center; gap: 6px;">
                              <span style="font-weight: 600;">Risk:</span> 
                              <el-tag size="small" :type="getRiskTagType(perm.riskLevel)">{{ getRiskLevelStr(perm.riskLevel) }}</el-tag>
                            </div>
                            <div v-if="perm.dependencyJson" style="margin-top: 8px; border-top: 1px solid #4c4d4f; padding-top: 8px;">
                              <span style="font-weight: 600;">Requires:</span> 
                              <span style="color: #e6a23c;">{{ formatDependencies(perm.dependencyJson) }}</span>
                            </div>
                          </div>
                        </template>
                        <el-checkbox
                          :model-value="currentSelectedIds.includes(perm.id)"
                          :disabled="role.isProtected"
                          @change="val => togglePermission(perm.id, val)"
                          style="width: 100%; display: flex; align-items: flex-start; height: auto;"
                        >
                          <el-text style="white-space: normal; line-height: 1.4; display: inline-block;">
                            {{ formatActionName(perm.code) }}
                            <div style="font-size: 11px; color: var(--el-text-color-secondary);">{{ perm.code }}</div>
                          </el-text>
                        </el-checkbox>
                      </el-tooltip>
                    </el-col>
                  </el-row>
                </div>
              </el-collapse-item>
            </el-collapse>
          </div>

          <!-- MATRIX VIEW (Data Table) -->
          <div v-else-if="viewMode === 'matrix'" class="matrix-container">
            <el-table 
              :data="matrixTableData" 
              :span-method="matrixSpanMethod"
              style="width: 100%; border: 1px solid var(--el-border-color-light); border-radius: 8px;"
              :header-cell-style="{ background: 'var(--el-fill-color-light)', color: 'var(--el-text-color-primary)', fontWeight: '600' }"
              border
            >
              <el-table-column label="Module" min-width="200">
                <template #default="{ row }">
                  <div style="display: flex; align-items: center; justify-content: space-between;">
                    <el-text style="font-weight: 600; text-transform: uppercase; letter-spacing: 0.05em; color: var(--el-text-color-primary);">
                      {{ formatModuleName(row.moduleName) }}
                    </el-text>
                    <el-checkbox
                      :model-value="getModuleCheckState(row.moduleName)"
                      :indeterminate="isModuleIndeterminate(row.moduleName)"
                      :disabled="role.isProtected"
                      @change="val => toggleModule(row.moduleName, val)"
                    />
                  </div>
                </template>
              </el-table-column>
              
              <el-table-column label="Action" min-width="250">
                <template #default="{ row }">
                  <div style="display: flex; align-items: center; justify-content: space-between; width: 100%;">
                    <el-tooltip placement="top" effect="dark" :hide-after="50" :open-delay="400">
                      <template #content>
                        <div style="max-width: 260px; font-size: 13px; line-height: 1.5;">
                          <div style="font-weight: bold; margin-bottom: 4px; font-size: 14px;">{{ formatActionName(row.perm.code) }}</div>
                          <div style="margin-bottom: 8px; color: #a3a6ad;">{{ row.perm.code }}</div>
                          <div v-if="row.perm.description" style="margin-bottom: 8px;">{{ row.perm.description }}</div>
                          <div style="margin-bottom: 4px; display: flex; align-items: center; gap: 6px;">
                            <span style="font-weight: 600;">Risk:</span> 
                            <el-tag size="small" :type="getRiskTagType(row.perm.riskLevel)">{{ getRiskLevelStr(row.perm.riskLevel) }}</el-tag>
                          </div>
                          <div v-if="row.perm.dependencyJson" style="margin-top: 8px; border-top: 1px solid #4c4d4f; padding-top: 8px;">
                            <span style="font-weight: 600;">Requires:</span> 
                            <span style="color: #e6a23c;">{{ formatDependencies(row.perm.dependencyJson) }}</span>
                          </div>
                        </div>
                      </template>
                      <el-text style="display: flex; flex-direction: column; align-items: flex-start; line-height: 1.4;">
                        <span>{{ formatActionName(row.perm.code) }}</span>
                        <span style="font-size: 11px; color: var(--el-text-color-secondary);">{{ row.perm.code }}</span>
                      </el-text>
                    </el-tooltip>
                    <el-tag v-if="row.perm.riskLevel >= 3" size="small" :type="getRiskTagType(row.perm.riskLevel)" effect="plain">
                      {{ getRiskLevelStr(row.perm.riskLevel) }} Risk
                    </el-tag>
                  </div>
                </template>
              </el-table-column>

              <el-table-column label="Enabled" width="120" align="center">
                <template #default="{ row }">
                  <el-checkbox
                    :model-value="currentSelectedIds.includes(row.perm.id)"
                    :disabled="role.isProtected"
                    @change="val => togglePermission(row.perm.id, val)"
                  />
                </template>
              </el-table-column>
            </el-table>
          </div>
        </template>
        
        <el-empty v-else description="No permissions match your filters." />
      </div>
    </el-scrollbar>

    <!-- Sticky Footer -->
    <div 
      v-if="hasChanges && !role.isProtected" 
      style="position: absolute; bottom: 24px; left: 50%; transform: translateX(-50%); background: var(--el-bg-color-overlay); padding: 16px 32px; border-radius: 12px; box-shadow: var(--el-box-shadow-light); z-index: 10; display: flex; align-items: center; gap: 32px; border: 1px solid var(--el-border-color-light);"
    >
      <el-text style="font-weight: 500;">You have unsaved changes to permissions.</el-text>
      <el-space>
        <el-button @click="discardChanges" :disabled="saving">Discard</el-button>
        <el-button type="primary" @click="saveChanges" :loading="saving">Save Changes</el-button>
      </el-space>
    </div>

    <!-- FULL SCREEN EDITOR DIALOG -->
    <PermissionEditorDialog
      v-model:visible="editorVisible"
      :role="role"
      :allPermissions="allPermissions"
      :saving="saving"
      @save="onEditorSave"
    />
  </div>
</template>

<script setup>
import { ref, computed, watch, reactive } from 'vue'
import { Search, Grid, DataBoard, FullScreen } from '@element-plus/icons-vue'
import PermissionEditorDialog from './PermissionEditorDialog.vue'

const props = defineProps({
  role: { type: Object, required: true },
  allPermissions: { type: Array, required: true },
  saving: { type: Boolean, default: false }
})

const emit = defineEmits(['save'])

const viewMode = ref('tree') // 'tree' or 'matrix'
const currentSelectedIds = ref([])
const activeModules = ref([])
const editorVisible = ref(false)

const filters = reactive({
  search: '',
  status: 'all',
  modules: [],
  risk: '',
  action: ''
})

watch(() => props.role, (newRole) => {
  if (newRole) {
    currentSelectedIds.value = [...(newRole.permissionIds || [])]
  }
}, { immediate: true })

watch(() => props.saving, (newVal, oldVal) => {
  if (oldVal === true && newVal === false && editorVisible.value) {
    editorVisible.value = false
  }
})

const groupedPermissions = computed(() => {
  const groups = {}
  
  props.allPermissions.forEach(p => {
    if (p.riskLevel === undefined) p.riskLevel = inferRiskLevel(p.code)
    
    const mod = p.module || 'general'
    if (!groups[mod]) groups[mod] = []
    groups[mod].push(p)
  })
  return groups
})

const availableModules = computed(() => Object.keys(groupedPermissions.value).sort())

const allDistinctActions = computed(() => {
  const actions = new Set()
  props.allPermissions.forEach(p => actions.add(getRawActionStr(p.code)))
  return Array.from(actions).sort()
})

const filteredGroupedPermissions = computed(() => {
  const q = filters.search.toLowerCase()
  const filtered = {}
  
  for (const [mod, perms] of Object.entries(groupedPermissions.value)) {
    if (filters.modules.length > 0 && !filters.modules.includes(mod)) continue

    let matched = perms

    if (filters.status === 'enabled') {
      matched = matched.filter(p => currentSelectedIds.value.includes(p.id))
    } else if (filters.status === 'disabled') {
      matched = matched.filter(p => !currentSelectedIds.value.includes(p.id))
    }
    
    if (filters.risk) {
      matched = matched.filter(p => p.riskLevel === filters.risk)
    }

    if (filters.action) {
      matched = matched.filter(p => getRawActionStr(p.code) === filters.action)
    }
    
    if (q) {
      matched = matched.filter(p => 
        p.code.toLowerCase().includes(q) || 
        mod.toLowerCase().includes(q) ||
        (p.description && p.description.toLowerCase().includes(q))
      )
    }

    if (matched.length > 0) {
      filtered[mod] = matched
    }
  }
  return filtered
})

const hasResults = computed(() => Object.keys(filteredGroupedPermissions.value).length > 0)

const filteredTotalCount = computed(() => {
  let count = 0
  Object.values(filteredGroupedPermissions.value).forEach(arr => count += arr.length)
  return count
})

const highRiskCount = computed(() => {
  return props.allPermissions.filter(p => p.riskLevel >= 3 && currentSelectedIds.value.includes(p.id)).length
})

watch(filteredGroupedPermissions, (newVal) => {
  activeModules.value = Object.keys(newVal)
}, { immediate: true })

const hasChanges = computed(() => {
  if (!props.role) return false
  const original = [...(props.role.permissionIds || [])].sort()
  const current = [...currentSelectedIds.value].sort()
  if (original.length !== current.length) return true
  return !original.every((val, idx) => val === current[idx])
})

function discardChanges() {
  currentSelectedIds.value = [...(props.role.permissionIds || [])]
}

function saveChanges() {
  emit('save', currentSelectedIds.value)
}

function onEditorSave(ids) {
  emit('save', ids)
}

function getSelectedCount(moduleName) {
  const perms = groupedPermissions.value[moduleName] || []
  return perms.filter(p => currentSelectedIds.value.includes(p.id)).length
}

function getModuleCheckState(moduleName) {
  const perms = groupedPermissions.value[moduleName] || []
  if (perms.length === 0) return false
  return perms.every(p => currentSelectedIds.value.includes(p.id))
}

function isModuleIndeterminate(moduleName) {
  const perms = groupedPermissions.value[moduleName] || []
  if (perms.length === 0) return false
  const selectedCount = getSelectedCount(moduleName)
  return selectedCount > 0 && selectedCount < perms.length
}

function toggleModule(moduleName, val) {
  const perms = groupedPermissions.value[moduleName] || []
  if (val) {
    perms.forEach(p => {
      if (!currentSelectedIds.value.includes(p.id)) currentSelectedIds.value.push(p.id)
    })
  } else {
    currentSelectedIds.value = currentSelectedIds.value.filter(
      id => !perms.find(p => p.id === id)
    )
  }
}

function togglePermission(permId, val) {
  if (val) {
    if (!currentSelectedIds.value.includes(permId)) currentSelectedIds.value.push(permId)
  } else {
    currentSelectedIds.value = currentSelectedIds.value.filter(id => id !== permId)
  }
}

// Bulk Actions
function expandAll() {
  activeModules.value = Object.keys(filteredGroupedPermissions.value)
}

function collapseAll() {
  activeModules.value = []
}

function bulkEnableFiltered() {
  Object.values(filteredGroupedPermissions.value).forEach(perms => {
    perms.forEach(p => {
      if (!currentSelectedIds.value.includes(p.id)) {
        currentSelectedIds.value.push(p.id)
      }
    })
  })
}

function bulkDisableFiltered() {
  const filteredIds = new Set()
  Object.values(filteredGroupedPermissions.value).forEach(perms => {
    perms.forEach(p => filteredIds.add(p.id))
  })
  
  currentSelectedIds.value = currentSelectedIds.value.filter(id => !filteredIds.has(id))
}

// Formatting
function formatModuleName(name) {
  if (!name) return 'General'
  return name.split('.').map(part => {
    return part.split('_').map(w => w.charAt(0).toUpperCase() + w.slice(1)).join(' ')
  }).join(' - ')
}

function getRawActionStr(code) {
  const parts = code.split('.')
  return parts[parts.length - 1]
}

function formatActionNameStr(actionStr) {
  return actionStr.split('_').map(w => w.charAt(0).toUpperCase() + w.slice(1)).join(' ')
}

function formatActionName(code) {
  return formatActionNameStr(getRawActionStr(code))
}

function formatDependencies(depJson) {
  if (!depJson) return 'None'
  try {
    const parsed = JSON.parse(depJson)
    if (Array.isArray(parsed) && parsed.length > 0) return parsed.join(', ')
    return depJson
  } catch(e) {
    return depJson
  }
}

function inferRiskLevel(code) {
  const lower = code.toLowerCase()
  if (lower.includes('delete') || lower.includes('destroy') || lower.includes('admin') || lower.includes('manage')) return 3
  if (lower.includes('create') || lower.includes('update') || lower.includes('edit') || lower.includes('import')) return 2
  return 1
}

function getRiskLevelStr(level) {
  if (level >= 4) return 'Critical'
  if (level === 3) return 'High'
  if (level === 2) return 'Medium'
  return 'Low'
}

function getRiskTagType(level) {
  if (level >= 4) return 'danger'
  if (level === 3) return 'danger'
  if (level === 2) return 'warning'
  return 'info'
}

// MATRIX VIEW LOGIC
const matrixTableData = computed(() => {
  const data = []
  const sortedModules = Object.keys(filteredGroupedPermissions.value).sort()
  
  sortedModules.forEach(moduleName => {
    const perms = filteredGroupedPermissions.value[moduleName] || []
    if (perms.length === 0) return
    
    const sortedPerms = [...perms].sort((a, b) => {
      return getRawActionStr(a.code).localeCompare(getRawActionStr(b.code))
    })
    
    sortedPerms.forEach((p, index) => {
      data.push({
        moduleName,
        perm: p,
        rowspan: index === 0 ? sortedPerms.length : 0,
        isFirst: index === 0
      })
    })
  })
  return data
})

const matrixSpanMethod = ({ row, column, rowIndex, columnIndex }) => {
  if (columnIndex === 0) {
    if (row.isFirst) {
      return { rowspan: row.rowspan, colspan: 1 }
    } else {
      return { rowspan: 0, colspan: 0 }
    }
  }
}
</script>

<style scoped>
.summary-card {
  border-radius: 8px;
  background: var(--el-bg-color);
  border: 1px solid var(--el-border-color-light);
}
.summary-title {
  font-size: 13px;
  color: var(--el-text-color-secondary);
  margin-bottom: 8px;
  font-weight: 500;
  text-transform: uppercase;
  letter-spacing: 0.05em;
}
.summary-value {
  font-size: 24px;
  font-weight: 600;
  color: var(--el-text-color-primary);
}

::v-deep(.el-collapse-item__header) {
  background: var(--el-fill-color-blank);
  border-bottom: 1px solid var(--el-border-color-light);
  height: 56px;
}
::v-deep(.el-collapse-item__wrap) {
  border-bottom: none;
}
::v-deep(.el-collapse-item__content) {
  padding-bottom: 0;
}
::v-deep(.el-checkbox__label) {
  color: var(--el-text-color-primary);
}
.matrix-container {
  overflow-x: auto;
}
</style>
