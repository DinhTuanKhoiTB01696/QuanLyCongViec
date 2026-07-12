<template>
  <div class="sprinta-page-header-block" style="display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 24px;">
    <div class="header-left">
      <div v-if="$slots.breadcrumb || back" class="header-breadcrumb" style="font-size: 14px; color: var(--sp-text-secondary); margin-bottom: 8px;">
        <slot name="breadcrumb">
          <span v-if="back" @click="$emit('back')" style="cursor: pointer;">
            <i class="fa-solid fa-arrow-left" style="margin-right: 4px;"></i> Quay lại
          </span>
        </slot>
      </div>
      <h1 style="margin: 0; font-size: 24px; font-weight: 500; color: var(--sp-text-primary);">{{ title }}</h1>
      <p v-if="subtitle || $slots.subtitle" style="margin: 4px 0 0; font-size: 14px; color: var(--sp-text-secondary);">
        <slot name="subtitle">{{ subtitle }}</slot>
      </p>
    </div>
    <div v-if="$slots.actions" class="header-actions" style="display: flex; gap: 8px; align-items: center;">
      <slot name="actions" />
    </div>
  </div>
</template>

<script setup>
defineProps({
  title: {
    type: String,
    required: true
  },
  subtitle: {
    type: String,
    default: ''
  },
  back: {
    type: Boolean,
    default: false
  }
})
defineEmits(['back'])
</script>
