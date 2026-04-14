<template>
  <NexusLayout>
    <div class="archives-container">
      <!-- Header -->
      <header class="archives-header">
        <div class="ah-left">
          <span class="breadcrumb"><i class="fa-solid fa-briefcase"></i> Projects <i class="fa-solid fa-chevron-right separator"></i> Archived</span>
        </div>
        <div class="ah-right">
          <div class="search-box">
             <i class="fa-solid fa-magnifying-glass"></i>
          </div>
          <button class="plane-toolbar-btn"><i class="fa-solid fa-arrow-down-short-wide"></i> Created date</button>
          <button class="plane-toolbar-btn"><i class="fa-solid fa-filter"></i> Filters</button>
        </div>
      </header>

      <!-- Empty State -->
      <div class="empty-state">
        <div class="empty-icon-wrap">
          <i class="fa-solid fa-box-archive empty-icon"></i>
        </div>
        <h3 class="empty-title">No projects archived</h3>
        <p class="empty-desc">Looks like all your projects are still active—great job!</p>
      </div>
    </div>
  </NexusLayout>
</template>

<script setup>
import NexusLayout from '@/components/layout/NexusLayout.vue'
</script>

<style scoped>
.archives-container {
  display: flex;
  flex-direction: column;
  height: 100vh;
  background: #0D0F11;
  color: #E4E4E7;
}

.archives-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 24px;
  border-bottom: 1px solid #1E2025;
}
.breadcrumb {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 14px;
  font-weight: 500;
  color: #A1A1AA;
}
.separator { font-size: 10px; color: #71717A; }

.ah-right {
  display: flex;
  gap: 12px;
}
.search-box {
  width: 32px; height: 32px;
  display: flex; align-items: center; justify-content: center;
  color: #71717A;
  border-radius: 4px;
  cursor: pointer;
}
.search-box:hover { color: #E4E4E7; background: #1E2025; }

.plane-toolbar-btn {
  background: transparent;
  border: 1px solid #27272A;
  color: #D4D4D8;
  font-size: 13px;
  font-weight: 500;
  cursor: pointer;
  padding: 6px 12px;
  border-radius: 6px;
  transition: background 0.2s;
  display: flex;
  align-items: center;
  gap: 6px;
}
.plane-toolbar-btn:hover { background: #1E2025; }

.empty-state {
  flex: 1;
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
}
.empty-icon-wrap {
  width: 80px; height: 80px;
  background: #111315;
  border: 1px solid #1E2025;
  border-radius: 16px;
  display: flex; align-items: center; justify-content: center;
  margin-bottom: 24px;
  position: relative;
  box-shadow: 0 10px 30px rgba(0,0,0,0.5);
}
.empty-icon { font-size: 32px; color: #71717A; }
.empty-title { margin: 0 0 8px 0; font-size: 16px; font-weight: 600; color: #E4E4E7; }
.empty-desc { margin: 0; font-size: 14px; color: #A1A1AA; }
</style>
