<template>
  <div class="sprinta-input-wrapper flex flex-col gap-1 w-full">
    <label v-if="label" class="text-[11px] font-bold uppercase tracking-wider text-secondary mb-0.5">
      {{ label }} <span v-if="required" class="text-danger">*</span>
    </label>
    <el-select
      v-bind="$attrs"
      :class="['sprinta-el-select', { 'has-error': error }]"
      popper-class="sprinta-select-dropdown"
    >
      <slot></slot>
    </el-select>
    <span v-if="error" class="text-xs text-danger mt-0.5">{{ error }}</span>
  </div>
</template>

<script setup>
defineProps({
  label: { type: String, default: '' },
  error: { type: String, default: '' },
  required: { type: Boolean, default: false }
})
</script>

<style>
.sprinta-el-select .el-input__wrapper {
  background-color: var(--color-surface);
  border: 2px solid var(--color-border) !important;
  border-radius: var(--radius-sm) !important;
  box-shadow: none !important;
  padding: 0 8px;
  height: 36px;
  transition: border-color 0.2s, background-color 0.2s;
}
.sprinta-el-select .el-input__wrapper:hover {
  background-color: var(--color-surface-hover);
}
.sprinta-el-select .el-input__wrapper.is-focus,
.sprinta-el-select.is-focus .el-input__wrapper {
  border-color: var(--color-accent) !important;
  background-color: var(--color-surface);
  box-shadow: none !important;
}
.sprinta-el-select.has-error .el-input__wrapper {
  border-color: var(--color-danger) !important;
}
.sprinta-el-select .el-input__inner {
  color: var(--color-text-primary);
  font-size: 14px;
}
.sprinta-select-dropdown {
  background-color: var(--color-surface) !important;
  border: 1px solid var(--color-border) !important;
  border-radius: var(--radius-md) !important;
  box-shadow: var(--shadow-md) !important;
}
.sprinta-select-dropdown .el-select-dropdown__item {
  color: var(--color-text-primary);
  border-radius: var(--radius-sm);
  margin: 2px 4px;
}
.sprinta-select-dropdown .el-select-dropdown__item.hover,
.sprinta-select-dropdown .el-select-dropdown__item:hover {
  background-color: var(--color-surface-hover) !important;
}
.sprinta-select-dropdown .el-select-dropdown__item.is-selected {
  color: var(--color-accent) !important;
  font-weight: 600;
  background-color: color-mix(in srgb, var(--color-accent) 10%, transparent) !important;
}
</style>
