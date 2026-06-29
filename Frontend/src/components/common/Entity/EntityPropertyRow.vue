<template>
  <div class="sprinta-property-row" @click="$emit('click')">
    <div class="sprinta-property-label">{{ label }}</div>
    <div class="sprinta-property-value">
      <slot name="value">
        <template v-if="user || avatar">
          <UserAvatar :user="user || { avatarColor, initials: avatar }" :size="24" :fontSize="11" class="me-2" />
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
import UserAvatar from '@/components/common/UserAvatar.vue'

defineProps({
  label: {
    type: String,
    required: true
  },
  value: {
    type: String,
    default: ''
  },
  user: {
    type: Object,
    default: null
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
