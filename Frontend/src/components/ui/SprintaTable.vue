<template>
  <div class="sprinta-table-container">
    <el-table
      v-bind="$attrs"
      class="sprinta-table"
      :header-cell-class-name="'sprinta-table-header-cell'"
      :cell-class-name="'sprinta-table-cell'"
    >
      <slot></slot>
      
      <template #empty v-if="$slots.empty">
        <slot name="empty"></slot>
      </template>
    </el-table>
  </div>
</template>

<style>
.sprinta-table-container {
  width: 100%;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  overflow: hidden;
  background-color: var(--color-surface);
}

.sprinta-table {
  --el-table-border-color: var(--color-border);
  --el-table-header-bg-color: var(--color-surface);
  --el-table-row-hover-bg-color: var(--color-surface-hover);
  --el-table-bg-color: var(--color-surface);
  --el-table-tr-bg-color: var(--color-surface);
  --el-table-text-color: var(--color-text-primary);
  --el-table-header-text-color: var(--color-text-secondary);
  font-family: 'Inter', system-ui, sans-serif;
}

.sprinta-table-header-cell {
  font-size: 12px !important;
  font-weight: 600 !important;
  text-transform: uppercase;
  letter-spacing: 0.02em;
  padding: 12px 16px !important;
  background-color: var(--color-bg) !important;
}

.sprinta-table-cell {
  padding: 12px 16px !important;
  font-size: 14px;
}

.sprinta-table .el-table__inner-wrapper::before {
  display: none;
}
</style>
