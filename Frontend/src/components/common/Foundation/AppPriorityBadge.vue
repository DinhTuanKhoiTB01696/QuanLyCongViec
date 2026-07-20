<template>
  <span class="sprinta-priority-badge" :class="priorityClass" :title="priority">
    <i :class="priorityIcon"></i>
    <span v-if="!iconOnly">{{ priority }}</span>
  </span>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  priority: { type: String, required: true },
  iconOnly: { type: Boolean, default: false }
})

const priorityData = computed(() => {
  const p = (props.priority || '').toLowerCase()
  if (p.includes('highest') || p.includes('cao nhất') || p.includes('khẩn')) {
    return { class: 'priority-highest', icon: 'fa-solid fa-angles-up' }
  }
  if (p.includes('high') || p.includes('cao')) {
    return { class: 'priority-high', icon: 'fa-solid fa-angle-up' }
  }
  if (p.includes('low') || p.includes('thấp')) {
    return { class: 'priority-low', icon: 'fa-solid fa-angle-down' }
  }
  if (p.includes('lowest') || p.includes('rất thấp')) {
    return { class: 'priority-lowest', icon: 'fa-solid fa-angles-down' }
  }
  // Medium / Default
  return { class: 'priority-medium', icon: 'fa-solid fa-equals' }
})

const priorityClass = computed(() => priorityData.value.class)
const priorityIcon = computed(() => priorityData.value.icon)
</script>

<style scoped>
.sprinta-priority-badge {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  font-size: 13px;
  font-weight: 500;
}

.priority-highest { color: #BF2600; }
.priority-high { color: #FF5630; }
.priority-medium { color: #FFAB00; }
.priority-low { color: #36B37E; }
.priority-lowest { color: #006644; }
</style>
