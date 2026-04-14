<script setup>
import { ref } from 'vue'
import NexusLayout from '@/components/layout/NexusLayout.vue'

const activeTab = ref('Summary')
const tabs = ['Summary', 'Assigned', 'Created', 'Subscribed', 'Activity']

// Mock data based on the image
const overview = { created: 1, assigned: 0, subscribed: 1 }
const workload = { backlog: 0, notStarted: 0, workingOn: 0, completed: 0, canceled: 0 }

const recentActivity = [
  { id: 1, text: 'You created CUN-8 Test Task', time: 'about 1 hour ago', type: 'create' }
]

const listData = [
  { id: 'CUN-3', title: '2. Invite your team 🤝', state: 'Done', modules: '2 Modules', cycle: 'Cycle 2: Collaboratio...' }
]

const pageActivities = [
   { icon: 'fa-solid fa-arrows-spin', text: 'You set the cycle to', bold: 'Cycle 2: Collaboration & Customization', time: '1 minute ago' },
   { icon: 'fa-solid fa-table-cells-large', text: 'You set the state to Done for CUN-3', bold: '2. Invite your team 🤝', time: '1 minute ago' },
   { icon: 'fa-regular fa-user', text: 'You added a new assignee cuongdqtb01697 to CUN-3', bold: '2. Invite your team 🤝', time: '1 minute ago' },
   { icon: 'fa-regular fa-circle', text: 'You created CUN-8', bold: 'Test Task', time: 'about 1 hour ago' }
]
</script>

<template>
  <NexusLayout>
    <div class="yw-container">
      
      <!-- Main Content -->
      <div class="yw-main">
        <header class="yw-header">
           <span class="yw-title"><i class="fa-regular fa-user"></i> Your work</span>
        </header>

        <div class="yw-tabs">
           <button 
             v-for="t in tabs" 
             :key="t" 
             class="tab-btn" 
             :class="{ active: activeTab === t }"
             @click="activeTab = t"
           >
             {{ t }}
           </button>
        </div>

        <div class="yw-scrollable" v-if="activeTab === 'Summary'">
           <!-- Overview -->
           <h3 class="section-title mt-4">Overview</h3>
           <div class="yw-cards-row">
              <div class="yw-card">
                 <div class="card-icon"><i class="fa-solid fa-plus"></i></div>
                 <div class="card-info">
                   <div class="card-lbl">Work items created</div>
                   <div class="card-val">{{ overview.created }}</div>
                 </div>
              </div>
              <div class="yw-card">
                 <div class="card-icon"><i class="fa-regular fa-circle-user"></i></div>
                 <div class="card-info">
                   <div class="card-lbl">Work items assigned</div>
                   <div class="card-val">{{ overview.assigned }}</div>
                 </div>
              </div>
              <div class="yw-card">
                 <div class="card-icon"><i class="fa-solid fa-inbox"></i></div>
                 <div class="card-info">
                   <div class="card-lbl">Work items subscribed</div>
                   <div class="card-val">{{ overview.subscribed }}</div>
                 </div>
              </div>
           </div>

           <!-- Workload -->
           <h3 class="section-title mt-4">Workload</h3>
           <div class="yw-workload-row">
              <div class="wl-card">
                 <div class="wl-lbl"><span class="dbox bg-gray"></span> Backlog</div>
                 <div class="wl-val">{{ workload.backlog }}</div>
              </div>
              <div class="wl-card">
                 <div class="wl-lbl"><span class="dbox bg-blue"></span> Not started</div>
                 <div class="wl-val">{{ workload.notStarted }}</div>
              </div>
              <div class="wl-card">
                 <div class="wl-lbl"><span class="dbox bg-orange"></span> Working on</div>
                 <div class="wl-val">{{ workload.workingOn }}</div>
              </div>
              <div class="wl-card">
                 <div class="wl-lbl"><span class="dbox bg-green"></span> Completed</div>
                 <div class="wl-val">{{ workload.completed }}</div>
              </div>
              <div class="wl-card">
                 <div class="wl-lbl"><span class="dbox bg-red"></span> Canceled</div>
                 <div class="wl-val">{{ workload.canceled }}</div>
              </div>
           </div>

           <!-- Two cols for charts -->
           <div class="yw-two-cols mt-4">
              <div class="chart-col">
                 <h3 class="section-title">Work items by Priority</h3>
                 <div class="empty-chart">
                    <i class="fa-solid fa-chart-simple chart-icon"></i>
                    <span>No work item assigned yet</span>
                 </div>
              </div>
              <div class="chart-col">
                 <h3 class="section-title">Work items by state</h3>
                 <div class="empty-chart">
                    <i class="fa-solid fa-chart-column chart-icon"></i>
                    <span>No work item assigned yet</span>
                 </div>
              </div>
           </div>

           <!-- Recent activity -->
           <h3 class="section-title mt-4">Recent activity</h3>
           <div class="activity-list">
              <div v-for="act in recentActivity" :key="act.id" class="activity-item">
                 <div class="avatar-sm">C</div>
                 <div class="act-info">
                    <div class="act-text">{{ act.text }}</div>
                    <div class="act-time">{{ act.time }}</div>
                 </div>
              </div>
           </div>
        </div>

        <!-- LIST VIEWS (Assigned, Created, Subscribed) -->
        <div class="yw-scrollable" v-else-if="['Assigned', 'Created', 'Subscribed'].includes(activeTab)">
           <div class="list-header mt-4">
              <i class="fa-solid fa-circle-dashed f-icon"></i> 
              <span class="lh-title">All work items</span>
              <span class="lh-count">1</span>
           </div>
           
           <div class="list-body mt-4">
              <div class="list-row" v-for="item in listData" :key="item.id">
                 <div class="lr-left">
                    <span class="lr-id">{{ item.id }}</span>
                    <span class="lr-title">{{ item.title }}</span>
                 </div>
                 <div class="lr-right">
                    <div class="lr-badge green"><i class="fa-solid fa-circle-check"></i> Done</div>
                    <div class="lr-badge"><i class="fa-solid fa-signal text-orange"></i></div>
                    <div class="lr-badge"><i class="fa-regular fa-user"></i></div>
                    <div class="lr-badge"><i class="fa-regular fa-calendar"></i></div>
                    <div class="lr-badge avatar-badge">C</div>
                    <div class="lr-badge"><i class="fa-solid fa-table-cells-large"></i> {{ item.modules }}</div>
                    <div class="lr-badge"><i class="fa-solid fa-arrows-spin"></i> {{ item.cycle }}</div>
                    <div class="lr-badge"><i class="fa-solid fa-tag"></i></div>
                    <div class="lr-badge"><i class="fa-solid fa-ellipsis"></i></div>
                 </div>
              </div>
           </div>
        </div>

        <!-- ACTIVITY VIEW -->
        <div class="yw-scrollable" v-else-if="activeTab === 'Activity'">
           <div class="activity-page-header mt-4 flex-between">
              <h3 class="section-title" style="margin: 0;">Recent activity</h3>
              <button class="plane-primary-btn">Download today's activity</button>
           </div>
           
           <div class="page-activity-list mt-4">
              <div class="p-act-row" v-for="(act, idx) in pageActivities" :key="idx">
                 <div class="p-act-icon"><i :class="act.icon"></i></div>
                 <div class="p-act-content">
                    <span class="p-ac-text">{{ act.text }} <span class="p-ac-bold">{{ act.bold }}</span></span>
                    <span class="p-ac-time">{{ act.time }}</span>
                 </div>
              </div>
           </div>
        </div>
      </div>

      <!-- Right Sidebar Profile info -->
      <div class="yw-sidebar">
         <div class="cover-image">
            <button class="edit-cover"><i class="fa-solid fa-pencil"></i></button>
         </div>
         <div class="profile-info">
            <div class="avatar-lg bg-blue">A</div>
            <div class="user-details">
               <h2 class="user-name">Alo</h2>
               <p class="user-handle">(cuongdqtb01697)</p>
            </div>
            
            <div class="info-row mt-4">
               <span class="info-lbl">Joined on</span>
               <span class="info-val">Apr 12, 2026</span>
            </div>
            <div class="info-row">
               <span class="info-lbl">Timezone</span>
               <span class="info-val">03:07 UTC</span>
            </div>

            <div class="workspace-row mt-4">
               <i class="fa-solid fa-briefcase ws-icon"></i>
               <span>Cun</span>
               <i class="fa-solid fa-chevron-down ms-auto" style="font-size: 10px; color: #71717A;"></i>
            </div>
         </div>
      </div>

    </div>
  </NexusLayout>
</template>

<style scoped>
.yw-container {
  display: flex;
  height: 100vh;
  background: #0D0F11;
  color: #E4E4E7;
  font-family: 'Inter', sans-serif;
  overflow: hidden;
}

.yw-main {
  flex: 1;
  display: flex;
  flex-direction: column;
  padding: 0 32px;
  overflow-y: auto;
}

.yw-header {
  padding: 24px 0 16px;
}
.yw-title {
  font-size: 16px;
  font-weight: 500;
  display: flex;
  align-items: center;
  gap: 8px;
}

.yw-tabs {
  display: flex;
  gap: 24px;
  border-bottom: 1px solid #1E2025;
}
.tab-btn {
  background: transparent;
  border: none;
  color: #A1A1AA;
  font-size: 13px;
  font-weight: 500;
  padding: 8px 0;
  cursor: pointer;
  border-bottom: 2px solid transparent;
  margin-bottom: -1px;
}
.tab-btn:hover { color: #E4E4E7; }
.tab-btn.active { color: #38BDF8; border-bottom: 2px solid #38BDF8; }

.yw-scrollable {
  padding-bottom: 40px;
}

.mt-4 { margin-top: 24px; }
.section-title { font-size: 14px; font-weight: 600; margin-bottom: 16px; color: #E4E4E7; }

.yw-cards-row {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 16px;
}
.yw-card {
  background: #111315;
  border: 1px solid #1E2025;
  border-radius: 8px;
  padding: 16px;
  display: flex;
  align-items: center;
  gap: 16px;
}
.card-icon { font-size: 18px; color: #71717A; width: 24px; text-align: center; }
.card-lbl { font-size: 11px; color: #71717A; margin-bottom: 4px; }
.card-val { font-size: 18px; font-weight: 600; color: #E4E4E7; }

.yw-workload-row {
  display: grid;
  grid-template-columns: repeat(5, 1fr);
  gap: 16px;
}
.wl-card {
  background: #111315;
  border: 1px solid #1E2025;
  border-radius: 8px;
  padding: 12px 16px;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  height: 60px;
}
.wl-lbl { font-size: 12px; font-weight: 500; color: #A1A1AA; display: flex; align-items: center; gap: 6px; }
.dbox { width: 8px; height: 8px; border-radius: 2px; }
.bg-gray { background: #A1A1AA; }
.bg-blue { background: #3B82F6; }
.bg-orange { background: #F59E0B; }
.bg-green { background: #10B981; }
.bg-red { background: #EF4444; }
.wl-val { font-size: 18px; font-weight: 600; color: #E4E4E7; margin-top: auto;}

.yw-two-cols {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 24px;
}
.empty-chart {
  background: #111315;
  border: 1px solid #1E2025;
  border-radius: 8px;
  height: 150px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 12px;
  color: #71717A;
  font-size: 12px;
}
.chart-icon { font-size: 32px; opacity: 0.3; }

.activity-list {
  background: #111315;
  border: 1px solid #1E2025;
  border-radius: 8px;
}
.activity-item {
  display: flex;
  align-items: center;
  padding: 16px;
  border-bottom: 1px solid #1E2025;
  gap: 12px;
}
.activity-item:last-child { border-bottom: none; }
.avatar-sm { width: 24px; height: 24px; background: #0EA5E9; color: white; display: flex; align-items: center; justify-content: center; font-size: 11px; font-weight: 600; border-radius: 50%; }
.act-text { font-size: 13px; font-weight: 500; color: #E4E4E7; }
.act-time { font-size: 11px; color: #71717A; margin-top: 4px; }

/* List View styles */
.list-header { display: flex; align-items: center; gap: 8px; font-size: 14px; font-weight: 600; color: #E4E4E7; }
.f-icon { color: #A1A1AA; font-size: 12px; }
.lh-count { font-size: 12px; font-weight: 400; color: #71717A; }

.list-body { border-top: 1px solid #1E2025; }
.list-row { display: flex; justify-content: space-between; align-items: center; padding: 12px 0; border-bottom: 1px solid #1E2025; transition: background 0.2s; cursor: pointer; }
.list-row:hover { background: #16181D; }
.lr-left { display: flex; align-items: center; gap: 16px; }
.lr-id { font-size: 12px; color: #71717A; min-width: 45px; }
.lr-title { font-size: 13px; font-weight: 500; color: #E4E4E7; }
.lr-right { display: flex; align-items: center; gap: 6px; }

.lr-badge { border: 1px solid #27272A; border-radius: 4px; padding: 4px 8px; font-size: 12px; color: #A1A1AA; display: flex; align-items: center; gap: 6px; }
.lr-badge.green { border-color: #064E3B; background: rgba(16, 185, 129, 0.1); color: #10B981; }
.lr-badge i { font-size: 11px; }
.text-orange { color: #F59E0B; }
.avatar-badge { width: 24px; height: 24px; border-radius: 50%; background: #0EA5E9; color: white; display: flex; align-items: center; justify-content: center; font-size: 11px; font-weight: 600; padding: 0; border: none; }

/* Activity Page styles */
.flex-between { display: flex; justify-content: space-between; align-items: center; }
.plane-primary-btn { background: #0EA5E9; color: white; border: none; border-radius: 6px; padding: 6px 12px; font-size: 12px; font-weight: 500; cursor: pointer; transition: background 0.2s; }
.plane-primary-btn:hover { background: #0284C7; }

.p-act-row { display: flex; align-items: flex-start; gap: 16px; padding: 16px 0; border-bottom: 1px solid #1E2025; }
.p-act-icon { width: 20px; font-size: 12px; color: #A1A1AA; text-align: center; margin-top: 2px; }
.p-act-content { display: flex; align-items: center; flex-wrap: wrap; gap: 6px; font-size: 13px; }
.p-ac-text { color: #A1A1AA; }
.p-ac-bold { color: #E4E4E7; font-weight: 500; }
.p-ac-time { color: #71717A; font-size: 11px; }

/* Right Sidebar */
.yw-sidebar {
  width: 320px;
  background: #0D0F11;
  border-left: 1px solid #1E2025;
  display: flex;
  flex-direction: column;
}
.cover-image {
  height: 120px;
  background: #27272A;
  background-image: linear-gradient(45deg, #16181D, #27272A);
  position: relative;
}
.edit-cover {
  position: absolute;
  top: 16px; right: 16px;
  background: rgba(0,0,0,0.5);
  border: none;
  color: #E4E4E7;
  width: 24px; height: 24px;
  border-radius: 4px;
  cursor: pointer;
  display: flex; align-items: center; justify-content: center;
}
.profile-info {
  padding: 0 24px 24px;
  position: relative;
}
.avatar-lg {
  position: absolute;
  top: -24px;
  width: 48px;
  height: 48px;
  background: #0EA5E9;
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 20px;
  font-weight: 600;
  border-radius: 8px;
  border: 4px solid #0D0F11;
}
.user-details { margin-top: 40px; }
.user-name { font-size: 16px; font-weight: 600; margin: 0; color: #E4E4E7; }
.user-handle { font-size: 12px; color: #71717A; margin: 4px 0 0 0; }

.info-row { display: flex; justify-content: space-between; font-size: 12px; margin-bottom: 8px; }
.info-lbl { color: #71717A; }
.info-val { color: #E4E4E7; font-weight: 500; }

.workspace-row { display: flex; align-items: center; gap: 8px; font-size: 13px; font-weight: 500; padding-top: 16px; border-top: 1px solid #1E2025; cursor: pointer; }
.ws-icon { color: #F59E0B; }
.ms-auto { margin-left: auto; }
</style>
