<template>
  <el-drawer
    v-bind="$attrs"
    class="sprinta-drawer"
    :show-close="false"
  >
    <template #header="{ close, titleId, titleClass }">
      <div class="sprinta-drawer-header">
        <h4 :id="titleId" :class="[titleClass, 'sprinta-drawer-title']">
          <slot name="title">{{ $attrs.title }}</slot>
        </h4>
        <div class="sprinta-drawer-actions">
          <slot name="header-actions"></slot>
          <button class="sprinta-drawer-close" @click="close">
            <X class="w-5 h-5" />
          </button>
        </div>
      </div>
    </template>
    
    <div class="sprinta-drawer-body">
      <slot></slot>
    </div>

    <template #footer v-if="$slots.footer">
      <div class="sprinta-drawer-footer">
        <slot name="footer"></slot>
      </div>
    </template>
  </el-drawer>
</template>

<script setup>
import { X } from 'lucide-vue-next'
</script>

<style>
.sprinta-drawer.el-drawer {
  background-color: var(--color-bg) !important;
  box-shadow: var(--shadow-xl) !important;
  border-left: 1px solid var(--color-border);
}

.sprinta-drawer-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 20px 24px 16px;
  border-bottom: 1px solid var(--color-border);
  background-color: var(--color-surface);
}

.sprinta-drawer-title {
  color: var(--color-text-primary);
  font-size: 20px;
  font-weight: 600;
  margin: 0;
  letter-spacing: -0.01em;
}

.sprinta-drawer-actions {
  display: flex;
  align-items: center;
  gap: 8px;
}

.sprinta-drawer-close {
  background: transparent;
  border: none;
  color: var(--color-text-secondary);
  cursor: pointer;
  padding: 6px;
  border-radius: var(--radius-sm);
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s;
}

.sprinta-drawer-close:hover {
  background-color: var(--color-surface-hover);
  color: var(--color-text-primary);
}

.sprinta-drawer-body {
  padding: 24px;
  height: 100%;
  overflow-y: auto;
  color: var(--color-text-primary);
}

.sprinta-drawer-footer {
  padding: 16px 24px;
  border-top: 1px solid var(--color-border);
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  background-color: var(--color-surface);
}

.sprinta-drawer.el-drawer .el-drawer__header {
  padding: 0 !important;
  margin: 0 !important;
}

.sprinta-drawer.el-drawer .el-drawer__body {
  padding: 0 !important;
  display: flex;
  flex-direction: column;
}
</style>
