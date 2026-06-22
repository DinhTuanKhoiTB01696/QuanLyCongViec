<template>
  <div class="user-avatar" :style="avatarStyle">
    <img v-if="avatarUrl" :src="avatarUrl" alt="" />
    <span v-else>{{ initials }}</span>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  user: {
    type: Object,
    default: () => ({})
  },
  size: {
    type: [Number, String],
    default: 32
  }
})

const avatarUrl = computed(() => props.user?.avatarUrl || props.user?.avatar || props.user?.profilePictureUrl || '')

const initials = computed(() => {
  const name = props.user?.fullName || props.user?.name || props.user?.email || ''
  const source = String(name).trim()
  if (!source) return 'U'
  return source
    .split(/\s+/)
    .slice(0, 2)
    .map(part => part[0]?.toUpperCase())
    .join('') || 'U'
})

const pixelSize = computed(() => {
  if (typeof props.size === 'number') return props.size
  const mapped = { sm: 24, md: 32, lg: 40 }
  return mapped[props.size] || Number.parseInt(props.size, 10) || 32
})

const avatarStyle = computed(() => ({
  width: `${pixelSize.value}px`,
  height: `${pixelSize.value}px`,
  fontSize: `${Math.max(10, Math.round(pixelSize.value * 0.38))}px`
}))
</script>

<style scoped>
.user-avatar {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex: 0 0 auto;
  border-radius: 50%;
  overflow: hidden;
  background: #172b4d;
  color: #ffffff;
  font-weight: 700;
  line-height: 1;
}

.user-avatar img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}
</style>
