<template>
  <div class="sc-empty-state">
    <div class="sc-es-icon">
      <i :class="icon"></i>
    </div>
    <h3 class="sc-es-title">{{ title }}</h3>
    <p class="sc-es-desc">{{ description }}</p>
    <div v-if="$slots.action" class="sc-es-action">
      <slot name="action"></slot>
    </div>
  </div>
</template>

<script setup>
defineProps({
  title: { type: String, required: true },
  description: { type: String, default: '' },
  icon: { type: String, default: 'fa-solid fa-box-open' }
})
</script>

<style scoped>
.sc-empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 48px 24px;
  text-align: center;
  background: var(--color-surface, #ffffff);
  border: 1px dashed var(--color-border, #dfe1e6);
  border-radius: 8px;
}
.sc-es-icon {
  font-size: 48px;
  color: var(--color-text-muted, #6b778c);
  margin-bottom: 16px;
  opacity: 0.5;
}
.sc-es-title {
  margin: 0 0 8px 0;
  font-size: 18px;
  font-weight: 600;
  color: var(--color-text-primary, #172b4d);
}
.sc-es-desc {
  margin: 0;
  font-size: 14px;
  color: var(--color-text-muted, #6b778c);
  max-width: 400px;
}
.sc-es-action {
  margin-top: 24px;
}
</style>
