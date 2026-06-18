<template>
  <el-dropdown
    v-bind="$attrs"
    class="sprinta-dropdown"
    popper-class="sprinta-dropdown-popper"
  >
    <slot></slot>
    <template #dropdown>
      <slot name="dropdown"></slot>
    </template>
  </el-dropdown>
</template>

<style>
.sprinta-dropdown-popper.el-dropdown__popper {
  background-color: var(--color-surface) !important;
  border: 1px solid var(--color-border) !important;
  border-radius: var(--radius-md) !important;
  box-shadow: var(--shadow-md) !important;
  padding: 4px 0 !important;
}

.sprinta-dropdown-popper .el-dropdown-menu__item {
  color: var(--color-text-primary);
  font-size: 14px;
  padding: 8px 16px;
  display: flex;
  align-items: center;
  gap: 8px;
  transition: background-color 0.2s;
}

.sprinta-dropdown-popper .el-dropdown-menu__item i,
.sprinta-dropdown-popper .el-dropdown-menu__item svg {
  color: var(--color-text-secondary);
}

.sprinta-dropdown-popper .el-dropdown-menu__item:hover {
  background-color: var(--color-surface-hover) !important;
  color: var(--color-text-primary);
}

.sprinta-dropdown-popper .el-dropdown-menu__item:hover i,
.sprinta-dropdown-popper .el-dropdown-menu__item:hover svg {
  color: var(--color-text-primary);
}

.sprinta-dropdown-popper .el-dropdown-menu__item.is-disabled {
  color: var(--color-text-muted);
  cursor: not-allowed;
  background-color: transparent !important;
}
</style>
