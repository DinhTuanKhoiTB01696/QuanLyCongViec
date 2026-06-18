<template>
  <el-tabs
    v-bind="$attrs"
    class="sprinta-tabs"
  >
    <slot></slot>
  </el-tabs>
</template>

<style>
.sprinta-tabs.el-tabs {
  --el-tabs-header-height: 40px;
}

.sprinta-tabs .el-tabs__nav-wrap::after {
  height: 2px;
  background-color: var(--color-border);
}

.sprinta-tabs .el-tabs__item {
  color: var(--color-text-secondary);
  font-size: 14px;
  font-weight: 500;
  padding: 0 16px !important;
  transition: color 0.2s;
}

.sprinta-tabs .el-tabs__item:hover {
  color: var(--color-text-primary);
}

.sprinta-tabs .el-tabs__item.is-active {
  color: var(--color-accent);
  font-weight: 600;
}

.sprinta-tabs .el-tabs__active-bar {
  background-color: var(--color-accent);
  height: 2px;
  border-radius: 2px 2px 0 0;
}
</style>
