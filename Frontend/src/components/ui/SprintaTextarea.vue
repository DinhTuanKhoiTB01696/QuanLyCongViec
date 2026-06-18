<template>
  <div class="sprinta-input-wrapper flex flex-col gap-1 w-full">
    <label v-if="label" class="text-[11px] font-bold uppercase tracking-wider text-secondary mb-0.5">
      {{ label }} <span v-if="required" class="text-danger">*</span>
    </label>
    <el-input
      v-bind="$attrs"
      type="textarea"
      :class="['sprinta-el-textarea', { 'has-error': error }]"
    >
    </el-input>
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
.sprinta-el-textarea .el-textarea__inner {
  background-color: var(--color-surface);
  border: 2px solid var(--color-border) !important;
  border-radius: var(--radius-sm) !important;
  box-shadow: none !important;
  padding: 8px 12px;
  color: var(--color-text-primary);
  font-size: 14px;
  transition: border-color 0.2s, background-color 0.2s;
  min-height: 80px !important;
}
.sprinta-el-textarea .el-textarea__inner:hover {
  background-color: var(--color-surface-hover);
}
.sprinta-el-textarea .el-textarea__inner:focus {
  border-color: var(--color-accent) !important;
  background-color: var(--color-surface);
  outline: none;
}
.sprinta-el-textarea.has-error .el-textarea__inner {
  border-color: var(--color-danger) !important;
}
.sprinta-el-textarea .el-textarea__inner::placeholder {
  color: var(--color-text-muted);
}
</style>
