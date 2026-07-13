<template>
  <div class="sprinta-user-chip" :class="{ compact: compact }">
    <AppAvatar :src="src" :name="name" :email="email" :size="compact ? 'xs' : 'sm'" />
    <span class="chip-text">{{ name || email || 'Người dùng ẩn danh' }}</span>
  </div>
</template>

<script setup>
import AppAvatar from './AppAvatar.vue'

defineProps({
  src: { type: String, default: '' },
  name: { type: String, default: '' },
  email: { type: String, default: '' },
  compact: { type: Boolean, default: false }
})
</script>

<style scoped>
.sprinta-user-chip {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  background-color: transparent;
  border-radius: 16px;
  padding: 2px 8px 2px 2px;
  transition: background-color 0.2s;
  cursor: default;
}
.sprinta-user-chip:hover {
  background-color: rgba(9, 30, 66, 0.08);
}
.sprinta-user-chip.compact {
  gap: 6px;
  padding: 0;
}
.sprinta-user-chip.compact:hover {
  background-color: transparent;
}
.chip-text {
  font-size: 14px;
  color: var(--sp-text-primary, #172B4D);
  font-weight: 500;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
</style>
