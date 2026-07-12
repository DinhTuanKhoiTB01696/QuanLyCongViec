<template>
  <div class="sprinta-toolbar" style="display: flex; flex-wrap: wrap; justify-content: space-between; align-items: center; gap: 16px; margin-bottom: 24px;">
    <div class="toolbar-left" style="display: flex; flex-wrap: wrap; gap: 12px; align-items: center; flex: 1;">
      <slot name="search" />
      <slot name="filters" />
    </div>
    <div class="toolbar-right" style="display: flex; flex-wrap: wrap; gap: 12px; align-items: center;">
      <slot name="views" />
      <slot name="actions" />
    </div>
  </div>
</template>

<script setup>
// Toolbar wrapper component
</script>
