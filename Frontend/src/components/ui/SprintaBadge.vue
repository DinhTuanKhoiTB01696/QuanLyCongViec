<template>
  <span 
    class="inline-flex items-center justify-center font-bold uppercase tracking-wider rounded-sm transition-colors"
    :class="computedClasses"
  >
    <slot></slot>
  </span>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  variant: {
    type: String,
    default: 'default', // default, primary, success, warning, danger, inprogress, todo, done
  },
  size: {
    type: String,
    default: 'default', // small, default
  }
})

const computedClasses = computed(() => {
  // Mapping variants to Jira-style colors
  // Jira typically uses very soft backgrounds with strong text for tags
  const variants = {
    default: 'bg-surface-hover text-secondary',
    primary: 'bg-accent/10 text-accent',
    success: 'bg-success/15 text-success',
    warning: 'bg-warning/15 text-warning-800 dark:text-warning',
    danger: 'bg-danger/15 text-danger',
    
    // Status specific mappings
    todo: 'bg-surface-hover text-secondary',
    inprogress: 'bg-[#DEEBFF] text-[#0052CC] dark:bg-accent/20 dark:text-accent',
    done: 'bg-[#E3FCEF] text-[#006644] dark:bg-success/20 dark:text-success',
  }

  const sizes = {
    small: 'text-[10px] px-1.5 py-0.5',
    default: 'text-[11px] px-2 py-0.5',
  }

  return `${variants[props.variant.toLowerCase()] || variants.default} ${sizes[props.size]}`
})
</script>
