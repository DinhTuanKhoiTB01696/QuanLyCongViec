<template>
  <div class="module-group">
    <div class="module-header" @click="toggleExpand">
      <div class="module-title">
        <i :class="expanded ? 'fa-solid fa-chevron-down' : 'fa-solid fa-chevron-right'"></i>
        <h3>{{ formatName(moduleName) }}</h3>
      </div>
      <div class="module-summary">
        {{ selectedCount }} / {{ totalCount }} permissions selected
      </div>
    </div>
    <el-collapse-transition>
      <div v-show="expanded" class="module-content">
        <PermissionPageGroup 
          v-for="(pagePermissions, pageName) in pages" 
          :key="pageName"
          :pageName="pageName"
          :permissions="pagePermissions"
          :selectedIds="selectedIds"
          :disabled="disabled"
          @update:selectedIds="updateSelected"
        />
      </div>
    </el-collapse-transition>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import PermissionPageGroup from './PermissionPageGroup.vue'

const props = defineProps({
  moduleName: String,
  pages: Object,
  selectedIds: Array,
  disabled: Boolean
})

const emit = defineEmits(['update:selectedIds'])

const expanded = ref(false)

const toggleExpand = () => {
  expanded.value = !expanded.value
}

const totalCount = computed(() => {
  let count = 0
  for (const page in props.pages) {
    count += props.pages[page].length
  }
  return count
})

const selectedCount = computed(() => {
  let count = 0
  for (const page in props.pages) {
    count += props.pages[page].filter(p => props.selectedIds.includes(p.id)).length
  }
  return count
})

const updateSelected = (newSelected) => {
  emit('update:selectedIds', newSelected)
}

const formatName = (name) => {
  if (!name) return ''
  return name.charAt(0).toUpperCase() + name.slice(1).replace(/_/g, ' ')
}
</script>

<style scoped>
.module-group {
  margin-bottom: 16px;
  background-color: var(--color-surface, #16181d);
  border: 1px solid var(--color-border, #27272a);
  border-radius: 8px;
}
.module-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px;
  cursor: pointer;
  user-select: none;
}
.module-header:hover {
  background-color: rgba(255, 255, 255, 0.02);
}
.module-title {
  display: flex;
  align-items: center;
  gap: 12px;
}
.module-title i {
  color: var(--color-text-muted, #a1a1aa);
  font-size: 14px;
  width: 14px;
  text-align: center;
}
.module-title h3 {
  margin: 0;
  font-size: 15px;
  color: var(--color-text-primary, #e4e4e7);
}
.module-summary {
  font-size: 13px;
  color: var(--color-text-muted, #a1a1aa);
}
.module-content {
  padding: 0 16px 16px 16px;
}
</style>
