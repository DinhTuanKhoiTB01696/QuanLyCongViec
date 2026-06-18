<template>
  <el-dialog
    v-bind="$attrs"
    class="sprinta-modal"
    :show-close="false"
  >
    <template #header="{ close, titleId, titleClass }">
      <div class="sprinta-modal-header">
        <h4 :id="titleId" :class="[titleClass, 'sprinta-modal-title']">
          <slot name="title">{{ $attrs.title }}</slot>
        </h4>
        <button class="sprinta-modal-close" @click="close">
          <X class="w-5 h-5" />
        </button>
      </div>
    </template>
    
    <div class="sprinta-modal-body">
      <slot></slot>
    </div>

    <template #footer v-if="$slots.footer">
      <div class="sprinta-modal-footer">
        <slot name="footer"></slot>
      </div>
    </template>
  </el-dialog>
</template>

<script setup>
import { X } from 'lucide-vue-next'
</script>

<style>
.sprinta-modal.el-dialog {
  background-color: var(--color-surface) !important;
  border-radius: var(--radius-md) !important;
  border: 1px solid var(--color-border) !important;
  box-shadow: var(--shadow-xl) !important;
  padding: 0 !important;
  overflow: hidden;
}

.sprinta-modal-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 20px 24px 16px;
  border-bottom: 1px solid var(--color-border);
}

.sprinta-modal-title {
  color: var(--color-text-primary);
  font-size: 20px;
  font-weight: 600;
  margin: 0;
  letter-spacing: -0.01em;
}

.sprinta-modal-close {
  background: transparent;
  border: none;
  color: var(--color-text-secondary);
  cursor: pointer;
  padding: 4px;
  border-radius: var(--radius-sm);
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s;
}

.sprinta-modal-close:hover {
  background-color: var(--color-surface-hover);
  color: var(--color-text-primary);
}

.sprinta-modal-body {
  padding: 24px;
  color: var(--color-text-primary);
}

.sprinta-modal-footer {
  padding: 16px 24px 20px;
  border-top: 1px solid var(--color-border);
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  background-color: var(--color-surface);
}

.sprinta-modal.el-dialog .el-dialog__header,
.sprinta-modal.el-dialog .el-dialog__body,
.sprinta-modal.el-dialog .el-dialog__footer {
  padding: 0 !important;
  margin: 0 !important;
}
</style>
