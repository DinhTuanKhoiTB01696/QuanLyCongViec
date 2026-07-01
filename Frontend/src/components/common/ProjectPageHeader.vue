<template>
  <header class="project-page-header">
    <div class="pph-left">
      <div v-if="icon" class="pph-icon">
        <i :class="icon"></i>
      </div>
      <div class="pph-content">
        <h1 class="pph-title">{{ title }}</h1>
        <p v-if="description" class="pph-desc">{{ description }}</p>
      </div>
    </div>
    <div class="pph-right" v-if="$slots.actions">
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
.project-page-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 16px;
  min-height: 48px;
}
.pph-left {
  display: flex;
  align-items: center;
  gap: 16px;
}
.pph-icon {
  width: 48px;
  height: 48px;
  border-radius: 8px;
  background: var(--color-surface, #ffffff);
  border: 1px solid var(--color-border, #dfe1e6);
  color: var(--color-text-secondary, #42526e);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 20px;
  flex-shrink: 0;
  box-shadow: 0 1px 2px rgba(0,0,0,0.05);
}
.pph-content {
  display: flex;
  flex-direction: column;
  gap: 4px;
}
.pph-title {
  margin: 0;
  font-size: 24px;
  font-weight: 600;
  color: var(--color-text-primary, #172b4d);
  line-height: 1.2;
}
.pph-desc {
  margin: 0;
  font-size: 14px;
  color: var(--color-text-muted, #6b778c);
  line-height: 1.4;
}
.pph-right {
  display: flex;
  align-items: center;
  gap: 12px;
  flex-shrink: 0;
}

/* Ensure the action buttons passed into slot match the uniform design */
:deep(.pph-right button) {
  display: flex !important;
  align-items: center !important;
  justify-content: center !important;
  gap: 8px !important;
  height: 36px !important;
  padding: 0 16px !important;
  font-size: 14px !important;
  font-weight: 500 !important;
  border-radius: 6px !important;
}
:deep(.pph-right button i) {
  font-size: 14px !important;
}
</style>
