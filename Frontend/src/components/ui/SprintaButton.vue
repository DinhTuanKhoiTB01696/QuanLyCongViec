<template>
  <el-button
    :class="computedClasses"
    :type="elType"
    v-bind="$attrs"
  >
    <template #icon v-if="$slots.icon">
      <slot name="icon"></slot>
    </template>
    <slot></slot>
  </el-button>
</template>

<script setup>
import { computed, useAttrs } from 'vue'

const props = defineProps({
  variant: {
    type: String,
    default: 'default', // primary, default, subtle, danger, warning, link
  },
  size: {
    type: String,
    default: 'default', // small, default, large
  },
  iconOnly: {
    type: Boolean,
    default: false,
  }
})

const attrs = useAttrs()

const elType = computed(() => {
  if (props.variant === 'primary') return 'primary'
  if (props.variant === 'danger') return 'danger'
  if (props.variant === 'warning') return 'warning'
  if (props.variant === 'link') return 'primary'
  return 'default'
})

const computedClasses = computed(() => {
  const base = 'sprinta-btn inline-flex items-center justify-center font-medium transition-colors duration-200 outline-none focus-visible:ring-2 focus-visible:ring-accent focus-visible:ring-offset-1 disabled:opacity-50 disabled:cursor-not-allowed'
  
  const variants = {
    primary: 'bg-accent text-white hover:bg-accent-hover border-none',
    default: 'bg-surface-hover text-primary hover:bg-border-hover/50 border-none',
    subtle: 'bg-transparent text-secondary hover:bg-surface-hover border-none',
    danger: 'bg-danger text-white hover:bg-danger/90 border-none',
    warning: 'bg-warning text-white hover:bg-warning/90 border-none',
    link: 'bg-transparent text-accent hover:underline p-0 h-auto border-none',
  }

  const sizes = {
    small: 'h-6 px-2 text-xs rounded-sm',
    default: 'h-8 px-3 text-sm rounded-sm',
    large: 'h-10 px-4 text-base rounded-md',
  }

  const iconOnlySizes = {
    small: 'h-6 w-6 p-0 rounded-sm',
    default: 'h-8 w-8 p-0 rounded-sm',
    large: 'h-10 w-10 p-0 rounded-md',
  }

  let sizeClass = props.iconOnly ? iconOnlySizes[props.size] : sizes[props.size]
  if (props.variant === 'link') sizeClass = ''

  return [
    base,
    variants[props.variant],
    sizeClass
  ].join(' ')
})
</script>

<style>
/* Global override for sprinta-btn to kill ElButton default padding/colors */
.el-button.sprinta-btn {
  border: none !important;
  margin: 0;
}
.el-button.sprinta-btn:not(.is-disabled):focus,
.el-button.sprinta-btn:not(.is-disabled):hover {
  color: inherit;
  border-color: transparent;
  background-color: inherit;
}
</style>
