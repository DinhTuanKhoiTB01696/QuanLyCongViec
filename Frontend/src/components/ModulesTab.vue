<script setup>
import { ref } from 'vue'

const modules = ref([
  { id: 1, name: 'Authentication System', progress: 100, status: 'completed', lead: 'H', date: 'Apr 10' },
  { id: 2, name: 'Payment Gateway', progress: 45, status: 'active', lead: 'P', date: 'May 05' },
  { id: 3, name: 'Analytics Dashboard', progress: 0, status: 'upcoming', lead: 'A', date: 'Jun 12' }
])

const activeTab = ref('all'); // all, active, completed
</script>

<template>
  <div class="plane-modules-wrapper">
    <!-- Header Controls -->
    <div class="modules-view-header">
       <div class="vh-left">
          <div class="modules-nav">
             <div class="nav-tab" :class="{ active: activeTab === 'all' }" @click="activeTab = 'all'">All modules</div>
             <div class="nav-tab" :class="{ active: activeTab === 'active' }" @click="activeTab = 'active'">Active</div>
             <div class="nav-tab" :class="{ active: activeTab === 'completed' }" @click="activeTab = 'completed'">Completed</div>
          </div>
       </div>
       <div class="vh-right">
          <button class="icon-action"><i class="fa-solid fa-magnifying-glass"></i></button>
          <button class="filter-action"><i class="fa-solid fa-filter"></i> Filters</button>
          <button class="primary-action">Add module</button>
       </div>
    </div>

    <!-- Body -->
    <div class="modules-body">
      <div class="modules-grid">
         <div class="module-card" v-for="mod in modules" :key="mod.id">
            <div class="mc-header">
               <div class="mc-icon" :class="mod.status">
                  <i class="fa-solid fa-cube" v-if="mod.status === 'upcoming'"></i>
                  <i class="fa-solid fa-cube text-blue" v-if="mod.status === 'active'"></i>
                  <i class="fa-solid fa-cube text-green" v-if="mod.status === 'completed'"></i>
               </div>
               <button class="mc-more"><i class="fa-solid fa-ellipsis"></i></button>
            </div>
            
            <div class="mc-body">
               <h3 class="mod-title">{{ mod.name }}</h3>
               <div class="mod-meta">
                  <span class="mod-date"><i class="fa-regular fa-calendar"></i> {{ mod.date }}</span>
                  <div class="mod-lead">Lead <div class="avatar-xxs">{{ mod.lead }}</div></div>
               </div>
               
               <div class="mod-progress-box">
                  <div class="prog-info">
                     <span>Progress</span>
                     <span :class="{'text-blue': mod.status === 'active', 'text-green': mod.status === 'completed'}">{{ mod.progress }}%</span>
                  </div>
                  <div class="prog-bar">
                     <div class="prog-fill" :class="mod.status" :style="{ width: mod.progress + '%' }"></div>
                  </div>
               </div>
            </div>
         </div>
         
         <div class="module-card new-card">
            <i class="fa-solid fa-plus text-muted"></i>
            <span>Create new module</span>
         </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.plane-modules-wrapper {
  display: flex;
  flex-direction: column;
  height: 100%;
  color: #E4E4E7;
  font-family: 'Inter', sans-serif;
  background: #0D0F11;
  min-height: calc(100vh - 120px);
}

/* Header */
.modules-view-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 24px 0;
  border-bottom: 1px solid #1E2025;
}
.modules-nav {
  display: flex;
  gap: 24px;
}
.nav-tab {
  font-size: 14px;
  font-weight: 500;
  color: #A1A1AA;
  padding-bottom: 12px;
  cursor: pointer;
  border-bottom: 2px solid transparent;
  margin-bottom: -1px;
}
.nav-tab:hover { color: #E4E4E7; }
.nav-tab.active {
  color: #38BDF8;
  border-bottom: 2px solid #38BDF8;
}

.vh-right {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 12px;
}
.icon-action {
  background: transparent;
  border: none;
  color: #A1A1AA;
  cursor: pointer;
  font-size: 14px;
}
.icon-action:hover { color: #E4E4E7; }
.filter-action {
  background: transparent;
  border: 1px solid #27272A;
  color: #E4E4E7;
  padding: 6px 12px;
  border-radius: 6px;
  font-size: 13px;
  cursor: pointer;
}
.filter-action:hover { background: #16181D; }
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
.modules-body {
  padding: 24px;
  flex: 1;
}

.modules-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 20px;
}

.module-card {
  background: #111315;
  border: 1px solid #1E2025;
  border-radius: 8px;
  padding: 20px;
  display: flex;
  flex-direction: column;
  transition: border-color 0.2s;
  cursor: pointer;
}
.module-card:hover { border-color: #38BDF8; }

.module-card.new-card {
  display: flex;
  align-items: center;
  justify-content: center;
  flex-direction: column;
  gap: 12px;
  border-style: dashed;
  color: #A1A1AA;
  font-size: 14px;
  font-weight: 500;
}
.module-card.new-card:hover { border-color: #A1A1AA; color: #E4E4E7; }

.mc-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
}
.mc-icon {
  width: 36px;
  height: 36px;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 16px;
  background: #16181D;
  color: #71717A;
}
.mc-icon.active { background: rgba(59, 130, 246, 0.1); color: #3B82F6; }
.mc-icon.completed { background: rgba(16, 185, 129, 0.1); color: #10B981; }

.mc-more { background: transparent; border: none; color: #71717A; cursor: pointer; }
.mc-more:hover { color: #E4E4E7; }

.mod-title { font-size: 16px; font-weight: 600; margin: 0 0 12px; }
.mod-meta { display: flex; justify-content: space-between; font-size: 12px; color: #71717A; margin-bottom: 24px; }
.mod-date { display: flex; align-items: center; gap: 6px; }
.mod-lead { display: flex; align-items: center; gap: 8px; }

.avatar-xxs {
  width: 20px;
  height: 20px;
  border-radius: 50%;
  background: #F59E0B;
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 10px;
  font-weight: 600;
}

.mod-progress-box { display: flex; flex-direction: column; gap: 8px; }
.prog-info { display: flex; justify-content: space-between; font-size: 12px; color: #A1A1AA; font-weight: 500; }
.text-blue { color: #3B82F6; }
.text-green { color: #10B981; }
.text-muted { color: #A1A1AA; }
.prog-bar { height: 6px; background: #1E2025; border-radius: 3px; overflow: hidden; }
.prog-fill { height: 100%; border-radius: 3px; transition: width 0.3s; }
.prog-fill.upcoming { background: #71717A; }
.prog-fill.active { background: #3B82F6; }
.prog-fill.completed { background: #10B981; }
</style>
