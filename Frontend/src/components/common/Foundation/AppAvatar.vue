<template>
  <div class="sprinta-avatar" :class="[`sprinta-avatar-${size}`]" :style="wrapperStyle">
    <img v-if="src && !imageError" :src="src" :alt="name || email" @error="imageError = true" />
    <span v-else class="sprinta-avatar-initials" :style="initialsStyle">{{ initials }}</span>
  </div>
</template>

<script setup>
import { computed, ref } from 'vue'

const props = defineProps({
  src: { type: String, default: '' },
  name: { type: String, default: '' },
  email: { type: String, default: '' },
  size: { type: String, default: 'md', validator: (v) => ['xs', 'sm', 'md', 'lg'].includes(v) }
})

const imageError = ref(false)

const initials = computed(() => {
  const text = (props.name || props.email || 'U').trim()
  return text.substring(0, 2).toUpperCase()
})

// Simple hash for consistent colors without external dependencies
const getAvatarColor = (str) => {
  if (!str) return '#0052cc'
  let hash = 0
  for (let i = 0; i < str.length; i++) {
    hash = str.charCodeAt(i) + ((hash << 5) - hash)
  }
  const hue = Math.abs(hash % 360)
  return `hsl(${hue}, 65%, 45%)`
}

const wrapperStyle = computed(() => {
  if (props.src && !imageError.value) return {}
  return {
    backgroundColor: getAvatarColor(props.name || props.email || 'User')
  }
})

const sizeMap = {
  xs: '10px',
  sm: '12px',
  md: '14px',
  lg: '18px'
}

const initialsStyle = computed(() => ({
  fontSize: sizeMap[props.size] || '14px'
}))
</script>

<style scoped>
.sprinta-avatar {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border-radius: 50%;
  overflow: hidden;
  color: #fff;
  font-weight: 600;
  flex-shrink: 0;
}
.sprinta-avatar-xs { width: 24px; height: 24px; }
.sprinta-avatar-sm { width: 32px; height: 32px; }
.sprinta-avatar-md { width: 40px; height: 40px; }
.sprinta-avatar-lg { width: 64px; height: 64px; }
.sprinta-avatar img { width: 100%; height: 100%; object-fit: cover; }
.sprinta-avatar-initials { line-height: 1; user-select: none; }
</style>
