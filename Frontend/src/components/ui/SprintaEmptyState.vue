<template>
  <div class="sprinta-empty-state flex flex-col items-center justify-center p-12 text-center">
    <div class="empty-icon-wrapper w-16 h-16 rounded-full flex items-center justify-center bg-gray-100 dark:bg-gray-800 mb-4 text-muted">
      <slot name="icon">
        <Inbox class="w-8 h-8 opacity-50" />
      </slot>
    </div>
    
    <h3 class="text-lg font-semibold text-primary mb-2">
      {{ title }}
    </h3>
    
    <p class="text-sm text-secondary max-w-md mb-6">
      {{ description }}
    </p>
    
    <div v-if="$slots.action" class="empty-action">
      <slot name="action"></slot>
    </div>
  </div>
</template>

<script setup>
import { Inbox } from 'lucide-vue-next'

defineProps({
  title: {
    type: String,
    required: true
  },
  description: {
    type: String,
    default: ''
  }
})
</script>

<style scoped>
.sprinta-empty-state {
  border-radius: var(--radius-md);
  border: 2px dashed var(--color-border);
  background-color: var(--color-surface);
}

.empty-icon-wrapper {
  background-color: color-mix(in srgb, var(--color-border) 40%, transparent);
  color: var(--color-text-muted);
}
</style>
