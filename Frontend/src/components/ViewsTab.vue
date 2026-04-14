<script setup>
import { ref } from 'vue'

const views = ref([
  { id: 1, name: 'Bugs assigned to me', description: 'All bugs currently assigned to my user ID', icon: 'fa-bug', color: '#EF4444' },
  { id: 2, name: 'High Priority Features', description: 'Priority = High or Urgent, Status = Unstarted', icon: 'fa-bolt', color: '#F59E0B' },
  { id: 3, name: 'Q2 Roadmap', description: 'All epics mapped to Q2 timeframe', icon: 'fa-map', color: '#3B82F6' },
])
</script>

<template>
  <div class="plane-views-wrapper">
    <!-- Header Controls -->
    <div class="views-header">
       <div class="vh-left">
          <div class="views-title">All Views</div>
       </div>
       <div class="vh-right">
          <button class="icon-action"><i class="fa-solid fa-magnifying-glass"></i></button>
          <button class="primary-action">Create view</button>
       </div>
    </div>

    <!-- Body -->
    <div class="views-body">
      <div v-if="views.length === 0" class="empty-state text-muted">No custom views found.</div>
      
      <div class="views-list">
         <div class="view-item" v-for="v in views" :key="v.id">
            <div class="vi-icon" :style="{ color: v.color, background: v.color + '22' }">
               <i class="fa-solid" :class="v.icon"></i>
            </div>
            <div class="vi-info">
               <h3 class="vi-name">{{ v.name }}</h3>
               <p class="vi-desc">{{ v.description }}</p>
            </div>
            <div class="vi-actions">
               <button class="icon-btn"><i class="fa-regular fa-star"></i></button>
               <button class="icon-btn"><i class="fa-solid fa-ellipsis"></i></button>
            </div>
         </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.plane-views-wrapper {
  display: flex;
  flex-direction: column;
  height: 100%;
  color: #E4E4E7;
  font-family: 'Inter', sans-serif;
  background: #0D0F11;
  min-height: calc(100vh - 120px);
}

/* Header */
.views-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 24px;
  border-bottom: 1px solid #1E2025;
}
.views-title {
  font-size: 16px;
  font-weight: 600;
  color: #E4E4E7;
}

.vh-right {
  display: flex;
  align-items: center;
  gap: 12px;
}
.icon-action {
  background: transparent;
  border: none;
  color: #A1A1AA;
  cursor: pointer;
  font-size: 14px;
}
.icon-action:hover { color: #E4E4E7; }
.primary-action {
  background: #0EA5E9;
  color: white;
  border: none;
  border-radius: 6px;
  padding: 6px 12px;
  font-size: 13px;
  cursor: pointer;
  font-weight: 500;
}
.primary-action:hover { background: #0284C7; }

/* Body Area */
.views-body {
  padding: 24px;
  flex: 1;
}

.views-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.view-item {
  background: #111315;
  border: 1px solid #1E2025;
  border-radius: 8px;
  padding: 16px;
  display: flex;
  align-items: center;
  gap: 16px;
  transition: background 0.2s;
  cursor: pointer;
}
.view-item:hover { background: #16181D; }

.vi-icon {
  width: 40px;
  height: 40px;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 18px;
}

.vi-info {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 4px;
}
.vi-name { font-size: 14px; font-weight: 500; margin: 0; color: #E4E4E7; }
.vi-desc { font-size: 12px; color: #71717A; margin: 0; }

.vi-actions {
  display: flex;
  align-items: center;
  gap: 12px;
  opacity: 0;
  transition: opacity 0.2s;
}
.view-item:hover .vi-actions { opacity: 1; }

.icon-btn {
  background: transparent;
  border: none;
  color: #71717A;
  font-size: 14px;
  cursor: pointer;
  padding: 4px;
}
.icon-btn:hover { color: #E4E4E7; }

.text-muted { color: #A1A1AA; }
.empty-state { text-align: center; margin-top: 40px; font-size: 14px; }
</style>
