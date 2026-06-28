<template>
  <div class="user-avatar-wrapper" :style="wrapperStyle">
    <img v-if="resolvedAvatarUrl" :src="resolvedAvatarUrl" :alt="resolvedName" class="user-avatar-img" @error="handleImageError" />
    <div v-else class="user-avatar-initials" :style="initialsStyle">
      {{ resolvedInitials }}
    </div>
  </div>
</template>

<script setup>
import { computed, ref } from 'vue'
import { getAvatarColor, getInitials } from '@/utils/avatarHelper'
import { usePeopleStore } from '@/store/usePeopleStore'

const peopleStore = usePeopleStore()

const props = defineProps({
  user: {
    type: Object,
    default: () => ({})
  },
  size: {
    type: Number,
    default: 32
  },
  fontSize: {
    type: Number,
    default: 12
  }
})

// Extract properties robustly, handling backend DTO variations
const imageLoadError = ref(false)
const handleImageError = () => {
  imageLoadError.value = true
}

const resolvedAvatarUrl = computed(() => {
  if (imageLoadError.value) return null
  return props.user?.avatarUrl || props.user?.AvatarUrl
})

const resolvedName = computed(() => {
  return props.user?.fullName || props.user?.FullName || 
         props.user?.owner || props.user?.Owner ||
         props.user?.leadName || props.user?.LeadName || 
         props.user?.name || props.user?.Name || ''
})

const resolvedEmail = computed(() => {
  let email = props.user?.email || props.user?.Email || ''
  if (!email && resolvedId.value) {
     const p = peopleStore.users?.find(u => u.id === resolvedId.value)
     if (p?.email) email = p.email
  }
  return email
})

const resolvedId = computed(() => props.user?.id || props.user?.Id || props.user?.userId || props.user?.UserId || '')

const resolvedInitials = computed(() => {
  // If backend provided initials explicitly (e.g., initials, ownerInitials, leadInitials)
  const backendInitials = props.user?.initials || props.user?.Initials || 
                          props.user?.ownerInitials || props.user?.OwnerInitials ||
                          props.user?.leadInitials || props.user?.LeadInitials

  if (backendInitials) return backendInitials

  // Fallback if backend didn't provide
  return getInitials(resolvedName.value, resolvedEmail.value)
})

const resolvedColor = computed(() => {
  const backendColor = props.user?.avatarColor || props.user?.AvatarColor ||
                       props.user?.ownerColor || props.user?.OwnerColor ||
                       props.user?.leadColor || props.user?.LeadColor

  if (backendColor) return backendColor

  // Fallback to email, then id, then name hash to guarantee cross-component consistency
  return getAvatarColor(String(resolvedEmail.value || resolvedId.value || resolvedName.value))
})

const wrapperStyle = computed(() => ({
  width: `${props.size}px`,
  height: `${props.size}px`,
  borderRadius: '50%',
  overflow: 'hidden',
  display: 'inline-flex',
  alignItems: 'center',
  justifyContent: 'center',
  backgroundColor: resolvedAvatarUrl.value ? 'transparent' : resolvedColor.value,
  flexShrink: 0
}))

const initialsStyle = computed(() => ({
  color: '#ffffff',
  fontSize: `${props.fontSize}px`,
  fontWeight: '600',
  lineHeight: 1,
  textTransform: 'uppercase',
  userSelect: 'none'
}))
</script>

<style scoped>
.user-avatar-wrapper {
  /* Default inherited styles */
}

.user-avatar-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  display: block;
}

.user-avatar-initials {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 100%;
  height: 100%;
}
</style>
