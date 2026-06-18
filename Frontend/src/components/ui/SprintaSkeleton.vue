<template>
  <div 
    class="sprinta-skeleton animate-pulse bg-gray-200 dark:bg-gray-700"
    :class="[
      rounded ? `rounded-${rounded}` : 'rounded-md',
      { 'w-full': fullWidth }
    ]"
    :style="{ width: width, height: height }"
  ></div>
</template>

<script setup>
defineProps({
  width: {
    type: String,
    default: '100%'
  },
  height: {
    type: String,
    default: '20px'
  },
  rounded: {
    type: String,
    default: 'md' // sm, md, lg, full
  },
  fullWidth: {
    type: Boolean,
    default: false
  }
})
</script>

<style scoped>
.sprinta-skeleton {
  background-color: color-mix(in srgb, var(--color-border) 50%, transparent);
}
</style>
