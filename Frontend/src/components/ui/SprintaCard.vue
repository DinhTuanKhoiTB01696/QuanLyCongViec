<template>
  <div class="sprinta-card bg-surface border border-border rounded-md text-primary overflow-hidden transition-shadow duration-200" :class="[{'hover:shadow-md cursor-pointer hover:border-border-hover': hoverable, 'shadow-sm': shadow}]">
    <div v-if="$slots.header || title" class="px-5 py-4 border-b border-border flex items-center justify-between bg-surface">
      <slot name="header">
        <h3 v-if="title" class="font-bold text-base m-0 text-primary">{{ title }}</h3>
      </slot>
      <div v-if="$slots.actions" class="flex items-center gap-2">
        <slot name="actions"></slot>
      </div>
    </div>
    
    <div :class="['sprinta-card-body', noPadding ? 'p-0' : 'p-5']">
      <slot></slot>
    </div>
    
    <div v-if="$slots.footer" class="px-5 py-3 border-t border-border bg-surface-hover/30">
      <slot name="footer"></slot>
    </div>
  </div>
</template>

<script setup>
defineProps({
  title: {
    type: String,
    default: ''
  },
  hoverable: {
    type: Boolean,
    default: false
  },
  shadow: {
    type: Boolean,
    default: false
  },
  noPadding: {
    type: Boolean,
    default: false
  }
})
</script>
