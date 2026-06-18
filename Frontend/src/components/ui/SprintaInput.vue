<template>
  <div class="sprinta-input-wrapper flex flex-col gap-1 w-full">
    <label v-if="label" class="text-[11px] font-bold uppercase tracking-wider text-secondary mb-0.5">
      {{ label }} <span v-if="required" class="text-danger">*</span>
    </label>
    <el-input
      v-bind="$attrs"
      :class="['sprinta-el-input', { 'has-error': error }]"
    >
      <template v-if="$slots.prefix" #prefix>
        <slot name="prefix"></slot>
      </template>
      <template v-if="$slots.suffix" #suffix>
        <slot name="suffix"></slot>
      </template>
    </el-input>
    <span v-if="error" class="text-xs text-danger mt-0.5">{{ error }}</span>
  </div>
</template>

<script setup>
defineProps({
  label: {
    type: String,
    default: ''
  },
  error: {
    type: String,
    default: ''
  },
  required: {
    type: Boolean,
    default: false
  }
})
</script>

<style>
/* Override Element Plus input styles to match Jira Atlassian */
.sprinta-el-input .el-input__wrapper {
  background-color: var(--color-surface);
  border: 2px solid var(--color-border) !important;
  border-radius: var(--radius-sm) !important;
  box-shadow: none !important;
  padding: 0 8px;
  height: 36px;
  transition: border-color 0.2s, background-color 0.2s;
}

.sprinta-el-input .el-input__wrapper:hover {
  background-color: var(--color-surface-hover);
}

.sprinta-el-input .el-input__wrapper.is-focus,
.sprinta-el-input.is-focus .el-input__wrapper {
  border-color: var(--color-accent) !important;
  background-color: var(--color-surface);
  box-shadow: none !important;
}

.sprinta-el-input.has-error .el-input__wrapper {
  border-color: var(--color-danger) !important;
}

.sprinta-el-input .el-input__inner {
  color: var(--color-text-primary);
  font-size: 14px;
}

.sprinta-el-input .el-input__inner::placeholder {
  color: var(--color-text-muted);
}
</style>
