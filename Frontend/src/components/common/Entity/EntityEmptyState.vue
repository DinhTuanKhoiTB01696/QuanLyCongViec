<template>
  <div class="sprinta-empty-state">
    <div class="sprinta-empty-icon" v-if="icon">
      <component :is="icon" class="w-8 h-8"></component>
    </div>
    <h3 style="font-size: 20px; font-weight: 500; color: #172B4D; margin: 0 0 8px 0;">{{ title }}</h3>
    <p style="color: #5E6C84; margin: 0 0 24px 0; max-width: 400px; line-height: 1.5;">{{ description }}</p>
    <slot name="action"></slot>
  </div>
</template>

<script setup>
defineProps({
  icon: {
    type: Object, // lucide component
    default: null
  },
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
