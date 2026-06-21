<template>
  <div class="sprinta-property-row" @click="$emit('click')">
    <div class="sprinta-property-label">{{ label }}</div>
    <div class="sprinta-property-value">
      <slot name="value">
        <template v-if="avatar">
          <div class="sprinta-property-avatar" :style="{ backgroundColor: avatarColor || '#172B4D' }">
            {{ avatar }}
          </div>
        </template>
        <template v-if="icon">
          <component :is="icon" class="w-4 h-4"></component>
        </template>
        <span>{{ value || 'Không có' }}</span>
      </slot>
    </div>
  </div>
</template>

<script setup>
defineProps({
  label: {
    type: String,
    required: true
  },
  value: {
    type: String,
    default: ''
  },
  avatar: {
    type: String,
    default: ''
  },
  avatarColor: {
    type: String,
    default: ''
  },
  icon: {
    type: Object, // lucide component
    default: null
  }
})

defineEmits(['click'])
</script>
