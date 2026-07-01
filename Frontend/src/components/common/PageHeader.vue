<template>
  <header class="sc-page-header">
    <div class="sc-ph-left">
      <div v-if="icon" class="sc-ph-icon">
        <i :class="icon"></i>
      </div>
      <div class="sc-ph-content">
        <h1 class="sc-ph-title">{{ title }}</h1>
        <p v-if="description" class="sc-ph-desc">{{ description }}</p>
      </div>
    </div>
    <div class="sc-ph-right" v-if="$slots.actions">
      <slot name="actions"></slot>
    </div>
  </header>
</template>

<script setup>
defineProps({
  title: { type: String, required: true },
  description: { type: String, default: '' },
  icon: { type: String, default: '' }
})
</script>

<style scoped>
.sc-page-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 16px;
}
.sc-ph-left {
  display: flex;
  align-items: center;
  gap: 16px;
}
.sc-ph-icon {
  width: 48px;
  height: 48px;
  border-radius: 8px;
  background: rgba(12, 102, 228, 0.1);
  color: var(--color-accent, #0c66e4);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 24px;
  flex-shrink: 0;
}
.sc-ph-content {
  display: flex;
  flex-direction: column;
  gap: 4px;
}
.sc-ph-title {
  margin: 0;
  font-size: 24px;
  font-weight: 600;
  color: var(--color-text-primary, #172b4d);
  line-height: 1.2;
}
.sc-ph-desc {
  margin: 0;
  font-size: 14px;
  color: var(--color-text-muted, #6b778c);
  line-height: 1.4;
}
.sc-ph-right {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-shrink: 0;
}
</style>
