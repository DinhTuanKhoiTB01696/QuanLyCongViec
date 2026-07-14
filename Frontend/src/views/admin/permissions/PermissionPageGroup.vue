<template>
  <div class="page-group">
    <div class="page-header">
      <el-checkbox 
        :model-value="canView" 
        @update:model-value="handleCanViewChange"
        :disabled="disabled"
      >
        Can view {{ formatName(pageName) }}
      </el-checkbox>
    </div>
    <div class="page-actions" v-if="actionPermissions.length > 0">
      <div 
        v-for="action in actionPermissions" 
        :key="action.id"
        class="action-item"
        :class="{ 'disabled-action': !canView }"
      >
        <el-checkbox 
          :model-value="hasAction(action.id)"
          @update:model-value="val => handleActionChange(action.id, val)"
          :disabled="disabled || !canView"
        >
          {{ formatActionName(action.actionName) }}
          <el-tooltip v-if="action.description" :content="action.description" placement="top">
            <i class="fa-regular fa-circle-question info-icon"></i>
          </el-tooltip>
        </el-checkbox>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  pageName: String,
  permissions: Array,
  selectedIds: Array,
  disabled: Boolean
})

const emit = defineEmits(['update:selectedIds'])

// A page needs a 'can_view' action to determine if it's visible.
// If there is no 'can_view' permission in DB for this page, we assume true or just handle it if it exists.
const viewPermission = computed(() => {
  return props.permissions.find(p => p.actionName === 'can_view')
})

const actionPermissions = computed(() => {
  return props.permissions.filter(p => p.actionName !== 'can_view')
})

const canView = computed(() => {
  if (!viewPermission.value) return true // if no explicit can_view exists, consider it viewable
  return props.selectedIds.includes(viewPermission.value.id)
})

const hasAction = (id) => {
  return props.selectedIds.includes(id)
}

const handleCanViewChange = (val) => {
  if (!viewPermission.value) return
  
  let newSelected = [...props.selectedIds]
  if (val) {
    if (!newSelected.includes(viewPermission.value.id)) {
      newSelected.push(viewPermission.value.id)
    }
  } else {
    // If cannot view, also remove all sub-actions
    newSelected = newSelected.filter(id => id !== viewPermission.value.id)
    const actionIds = actionPermissions.value.map(p => p.id)
    newSelected = newSelected.filter(id => !actionIds.includes(id))
  }
  emit('update:selectedIds', newSelected)
}

const handleActionChange = (id, val) => {
  let newSelected = [...props.selectedIds]
  if (val) {
    if (!newSelected.includes(id)) {
      newSelected.push(id)
    }
    // Also auto-check can_view if an action is checked
    if (viewPermission.value && !newSelected.includes(viewPermission.value.id)) {
      newSelected.push(viewPermission.value.id)
    }
  } else {
    newSelected = newSelected.filter(existingId => existingId !== id)
  }
  emit('update:selectedIds', newSelected)
}

const formatName = (name) => {
  if (!name) return ''
  return name.charAt(0).toUpperCase() + name.slice(1).replace(/_/g, ' ')
}

const formatActionName = (name) => {
  if (!name) return ''
  return formatName(name)
}
</script>

<style scoped>
.page-group {
  margin-bottom: 16px;
  border: 1px solid var(--color-border, #27272a);
  border-radius: 8px;
  background-color: var(--color-surface-hover, #1f232a);
  overflow: hidden;
}
.page-header {
  padding: 12px 16px;
  background-color: rgba(0, 0, 0, 0.2);
  border-bottom: 1px solid var(--color-border, #27272a);
}
.page-actions {
  padding: 12px 16px;
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
  gap: 12px;
}
.action-item {
  display: flex;
  align-items: center;
}
.disabled-action {
  opacity: 0.5;
}
.info-icon {
  margin-left: 6px;
  color: var(--color-text-muted, #a1a1aa);
  font-size: 12px;
}
</style>
