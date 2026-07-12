<template>
  <div class="permission-matrix">
    <div class="matrix-header">
      <div class="role-info">
        <h2>{{ role.name }}</h2>
        <p>{{ role.description || 'No description' }}</p>
        <span v-if="role.isProtected" class="protected-badge"><i class="fa-solid fa-lock"></i> Protected</span>
      </div>
      <div class="matrix-actions">
        <el-button 
          v-if="!role.isProtected && hasChanges" 
          type="primary" 
          @click="handleSave"
          :loading="saving"
        >
          Save permissions
        </el-button>
        <el-button 
          v-if="!role.isProtected && hasChanges" 
          @click="discardChanges"
          :disabled="saving"
        >
          Discard
        </el-button>
      </div>
    </div>
    
    <div class="matrix-body">
      <div v-if="role.isProtected" class="protected-notice-banner">
        This role is protected. You cannot modify its permissions.
      </div>
      <PermissionModuleGroup
        v-for="(pages, moduleName) in groupedPermissions"
        :key="moduleName"
        :moduleName="moduleName"
        :pages="pages"
        :selectedIds="currentSelectedIds"
        :disabled="role.isProtected"
        @update:selectedIds="currentSelectedIds = $event"
      />
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import PermissionModuleGroup from './PermissionModuleGroup.vue'

const props = defineProps({
  role: {
    type: Object,
    required: true
  },
  allPermissions: {
    type: Array,
    required: true
  },
  saving: Boolean
})

const emit = defineEmits(['save'])

const currentSelectedIds = ref([])

// Initialize selected IDs when role changes
watch(() => props.role, (newRole) => {
  if (newRole) {
    currentSelectedIds.value = [...(newRole.permissionIds || [])]
  }
}, { immediate: true })

const hasChanges = computed(() => {
  if (!props.role) return false
  const originalIds = [...(props.role.permissionIds || [])].sort()
  const currentIds = [...currentSelectedIds.value].sort()
  
  if (originalIds.length !== currentIds.length) return true
  return !originalIds.every((id, index) => id === currentIds[index])
})

const discardChanges = () => {
  currentSelectedIds.value = [...(props.role.permissionIds || [])]
}

const handleSave = () => {
  emit('save', currentSelectedIds.value)
}

// Group permissions by module, then by page
// Format is: module.page.action or module.action
const groupedPermissions = computed(() => {
  const groups = {}
  
  props.allPermissions.forEach(permission => {
    const moduleName = permission.module || 'General'
    
    // Parse the code
    const parts = (permission.code || '').split('.')
    let pageName = 'General'
    let actionName = permission.code
    
    if (parts.length > 1) {
      pageName = parts[0]
      actionName = parts.slice(1).join('.')
    }
    
    if (!groups[moduleName]) {
      groups[moduleName] = {}
    }
    if (!groups[moduleName][pageName]) {
      groups[moduleName][pageName] = []
    }
    
    groups[moduleName][pageName].push({
      ...permission,
      actionName
    })
  })
  
  return groups
})
</script>

<style scoped>
.permission-matrix {
  display: flex;
  flex-direction: column;
  height: 100%;
}
.matrix-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  padding-bottom: 24px;
  border-bottom: 1px solid var(--color-border, #27272a);
  margin-bottom: 24px;
}
.role-info h2 {
  margin: 0 0 8px 0;
  color: var(--color-text-primary, #e4e4e7);
}
.role-info p {
  margin: 0 0 12px 0;
  color: var(--color-text-muted, #a1a1aa);
}
.protected-badge {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  background-color: rgba(234, 179, 8, 0.1);
  color: var(--color-warning, #eab308);
  padding: 4px 10px;
  border-radius: 999px;
  font-size: 12px;
  font-weight: 500;
}
.protected-notice-banner {
  background-color: rgba(234, 179, 8, 0.1);
  border: 1px solid rgba(234, 179, 8, 0.3);
  color: var(--color-warning, #eab308);
  padding: 12px 16px;
  border-radius: 8px;
  margin-bottom: 20px;
  font-size: 14px;
}
</style>
