<template>
  <div class="user-avatar" :class="sizeClass" :style="{ backgroundColor: bgColor }">
    <template v-if="user?.avatarUrl || user?.profileImage">
      <img :src="user.avatarUrl || user.profileImage" alt="User Avatar" class="avatar-image" @error="handleImageError" v-if="!imageError" />
      <span v-else class="avatar-initials">{{ initials }}</span>
    </template>
    <template v-else>
      <span class="avatar-initials">{{ initials }}</span>
    </template>
  </div>
</template>

<script setup>
import { computed, ref } from 'vue'
import { getInitials as getInitialsUtil, getAvatarColor } from '@/utils/avatarUtils'

const props = defineProps({
  user: {
    type: Object,
    default: () => ({})
  },
  size: {
    type: String,
    default: 'md', // sm (24px), md (32px), lg (40px)
  }
})

const imageError = ref(false)

const handleImageError = () => {
  imageError.value = true
}

const bgColor = computed(() => {
  if (props.user?.avatarColor) return props.user.avatarColor;
  const name = props.user?.fullName || props.user?.displayName || props.user?.username || props.user?.name || props.user?.email || 'User'
  return getAvatarColor(name);
})

const sizeClass = computed(() => `avatar-${props.size}`)

const initials = computed(() => {
  const name = props.user?.fullName || props.user?.displayName || props.user?.username || props.user?.name || props.user?.email || 'Người dùng'
  return getInitialsUtil(name)
})
</script>

<style scoped>
.user-avatar {
  border-radius: 50%;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-weight: 500;
  overflow: hidden;
  flex-shrink: 0;
}

.avatar-sm {
  width: 24px;
  height: 24px;
  font-size: 11px;
}

.avatar-md {
  width: 32px;
  height: 32px;
  font-size: 14px;
}

.avatar-lg {
  width: 40px;
  height: 40px;
  font-size: 16px;
}

.avatar-image {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.avatar-initials {
  line-height: 1;
}
</style>
