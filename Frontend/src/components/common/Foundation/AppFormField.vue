<template>
  <div class="sprinta-form-field" :class="{ 'has-error': error }">
    <label v-if="label" class="sprinta-field-label">
      {{ label }} <span v-if="required" class="required-asterisk">*</span>
    </label>
    <div class="sprinta-field-content">
      <slot></slot>
    </div>
    <div v-if="error" class="sprinta-field-error">
      <i class="fa-solid fa-circle-exclamation"></i> {{ error }}
    </div>
    <div v-else-if="helpText" class="sprinta-field-help">
      {{ helpText }}
    </div>
  </div>
</template>

<script setup>
defineProps({
  label: {
    type: String,
    default: ''
  },
  required: {
    type: Boolean,
    default: false
  },
  error: {
    type: String,
    default: ''
  },
  helpText: {
    type: String,
    default: ''
  }
})
</script>

<style scoped>
.sprinta-form-field {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.sprinta-field-label {
  font-size: 12px;
  font-weight: 600;
  color: var(--sp-text-secondary, #5E6C84);
}

.required-asterisk {
  color: #DE350B;
}

.sprinta-field-error {
  color: #DE350B;
  font-size: 11px;
  display: flex;
  align-items: center;
  gap: 4px;
}

.sprinta-field-help {
  color: var(--sp-text-secondary, #5E6C84);
  font-size: 11px;
}

.sprinta-form-field.has-error :deep(input),
.sprinta-form-field.has-error :deep(.el-input__wrapper),
.sprinta-form-field.has-error :deep(textarea) {
  border-color: #DE350B !important;
  box-shadow: 0 0 0 1px #DE350B inset !important;
}
</style>
