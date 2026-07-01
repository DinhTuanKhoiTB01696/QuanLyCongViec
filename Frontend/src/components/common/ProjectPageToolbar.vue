<template>
  <div class="project-page-toolbar">
    <div class="ppt-left">
      <!-- Standardized Search -->
      <div v-if="showSearch" class="ppt-search">
        <i class="fa-solid fa-magnifying-glass search-icon"></i>
        <input 
          :value="searchQuery" 
          @input="$emit('update:searchQuery', $event.target.value)" 
          type="text" 
          class="nexus-search-input" 
          :placeholder="searchPlaceholder" 
        />
      </div>
      <slot name="search"></slot>
      <slot name="left"></slot>
    </div>
    <div class="ppt-right">
      <!-- Standardized Slots for exact alignment -->
      <div class="ppt-group ppt-filters" v-if="$slots.filters">
        <slot name="filters"></slot>
      </div>
      
      <div class="ppt-group ppt-sort" v-if="$slots.sort">
        <slot name="sort"></slot>
      </div>

      <div class="ppt-group ppt-actions" v-if="$slots.actions">
        <slot name="actions"></slot>
      </div>
      
      <div class="ppt-group ppt-toggles" v-if="$slots.toggles">
        <slot name="toggles"></slot>
      </div>
      <slot name="right"></slot>
    </div>
  </div>
</template>

<script setup>
defineProps({
  showSearch: { type: Boolean, default: false },
  searchQuery: { type: String, default: '' },
  searchPlaceholder: { type: String, default: 'Search...' }
})
defineEmits(['update:searchQuery'])
</script>

<style scoped>
.project-page-toolbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 10px;
  min-height: 42px;
  width: 100%;
  padding: 8px;
  border: 1px solid color-mix(in srgb, var(--color-border) 72%, transparent);
  border-radius: 12px;
  background:
    linear-gradient(180deg, color-mix(in srgb, var(--color-surface) 86%, transparent), color-mix(in srgb, var(--color-surface-hover) 46%, transparent));
  box-shadow: 0 10px 24px color-mix(in srgb, #020617 6%, transparent);
}

.ppt-left, .ppt-right {
  display: flex;
  align-items: center;
  gap: 8px;
  min-width: 0;
}

.ppt-group {
  display: flex;
  align-items: center;
  gap: 8px;
}

/* Standardized Search Input */
.ppt-search {
  position: relative;
  display: flex;
  align-items: center;
  width: min(260px, 30vw);
}

.ppt-search .search-icon {
  position: absolute;
  left: 12px;
  color: var(--color-text-muted);
  font-size: 14px;
}

.ppt-search .nexus-search-input {
  width: 100%;
  height: 34px !important;
  padding-left: 36px !important;
  padding-right: 12px !important;
  border-radius: 9px !important;
  border: 1px solid var(--color-border) !important;
  background-color: var(--color-surface) !important;
  color: var(--color-text-primary) !important;
  font-size: 13.5px !important;
  transition: border-color 0.2s, box-shadow 0.2s;
}

.ppt-search .nexus-search-input:focus {
  border-color: var(--color-accent) !important;
  box-shadow: 0 0 0 2px rgba(14, 165, 233, 0.15) !important;
  outline: none;
}

/* Force standard height and typography for all toolbar elements */
:deep(.ppt-group button),
:deep(.ppt-group select),
:deep(.ppt-group input),
:deep(.ppt-group .el-dropdown) {
  height: 34px !important;
  min-height: 34px !important;
  box-sizing: border-box !important;
  display: inline-flex !important;
  align-items: center !important;
  justify-content: center !important;
  font-size: 13px !important;
  border-radius: 9px !important;
}

/* Standardize outlined buttons in toolbar */
:deep(.ppt-group .nexus-btn-outlined),
:deep(.ppt-group .btn-secondary) {
  background-color: transparent !important;
  border: 1px solid var(--color-border) !important;
  color: var(--color-text-secondary) !important;
  padding: 0 12px !important;
  gap: 6px !important;
  font-weight: 500 !important;
}

:deep(.ppt-group .nexus-btn-outlined:hover),
:deep(.ppt-group .btn-secondary:hover) {
  background-color: var(--color-surface-hover) !important;
  color: var(--color-text-primary) !important;
  border-color: var(--color-border-hover) !important;
}

/* Standardize icon-only buttons */
:deep(.ppt-group .nexus-btn-icon) {
  width: 36px !important;
  padding: 0 !important;
  border: 1px solid var(--color-border) !important;
  background-color: transparent !important;
  color: var(--color-text-secondary) !important;
}

:deep(.ppt-group .nexus-btn-icon:hover) {
  background-color: var(--color-surface-hover) !important;
  color: var(--color-text-primary) !important;
}

/* Specific standard for view toggles */
:deep(.view-toggles) {
  display: flex !important;
  align-items: center !important;
  gap: 2px !important;
  background-color: var(--color-surface-hover) !important;
  padding: 2px !important;
  border-radius: 8px !important;
  height: 32px !important;
  border: 1px solid var(--color-border) !important;
}

:deep(.view-toggles button) {
  height: 28px !important;
  min-height: 28px !important;
  width: 28px !important;
  border: none !important;
  background: transparent !important;
  color: var(--color-text-muted) !important;
  border-radius: 6px !important;
}

:deep(.view-toggles button.active) {
  background-color: var(--color-surface) !important;
  color: var(--color-text-primary) !important;
  box-shadow: 0 1px 3px rgba(0,0,0,0.1) !important;
}

</style>
