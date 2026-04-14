<script setup>
import { ref, onMounted, computed } from 'vue'
import { useSprintStore } from '@/store/useSprintStore'

const props = defineProps({
  projectId: { type: String, required: true }
})

const sprintStore = useSprintStore()
const showCreateModal = ref(false)

const expandedTabs = ref({
  active: true,
  upcoming: false,
  completed: false
});

const toggleTab = (tab) => {
  expandedTabs.value[tab] = !expandedTabs.value[tab]
}

// Data binding
onMounted(async () => {
  await sprintStore.fetchSprints(props.projectId)
})

const allSprints = computed(() => sprintStore.sprints || [])
const activeSprint = computed(() => allSprints.value.find(s => s.status === true || s.status === 1))
const upcomingSprints = computed(() => allSprints.value.filter(s => !s.status && s.status !== 2))
const completedSprints = computed(() => allSprints.value.filter(s => s.status === 2))

const formatDateCompact = (d) => {
  if (!d) return ''
  const date = new Date(d)
  const month = date.toLocaleString('en-US', { month: 'short' })
  const day = date.getDate()
  const year = date.getFullYear()
  return `${month} ${day}, ${year}`
}
</script>

<template>
  <div class="plane-cycles-wrapper">
    <!-- Header Controls -->
    <!-- The user specifies top-right search/filters in Space header, 
         but we will put it in cycles view to match the image precisely -->
    <div class="cycles-view-header">
       <div class="vh-left">
          <!-- Handled by SpaceSummary header usually, keeping empty to stay true to image inside wrapper -->
       </div>
       <div class="vh-right">
          <button class="icon-action"><i class="fa-solid fa-magnifying-glass"></i></button>
          <button class="filter-action"><i class="fa-solid fa-filter"></i> Filters</button>
          <button class="primary-action">Add cycle</button>
       </div>
    </div>

    <div class="cycles-body">
      
      <!-- Active Cycle -->
      <div class="cycle-section">
        <div class="cs-header" @click="toggleTab('active')">
          <i class="chevron fa-solid" :class="expandedTabs.active ? 'fa-chevron-down' : 'fa-chevron-right'"></i>
          <i class="fa-solid fa-circle-half-stroke text-orange"></i>
          <span class="cs-title">Active cycle</span>
        </div>
        
        <div class="cs-content" v-show="expandedTabs.active">
           <div class="cycle-card expanded">
             <!-- Card Header -->
             <div class="cc-top">
                <div class="cct-left">
                   <div class="progress-ring text-green">25%</div>
                   <span class="cycle-name">{{ activeSprint?.name || 'Cycle 1: Getting Started with Plane' }}</span>
                </div>
                <div class="cct-right">
                   <span class="detail-link"><i class="fa-solid fa-info-circle"></i> More details</span>
                   <span class="date-range">
                     <i class="fa-regular fa-calendar"></i>
                     {{ activeSprint ? formatDateCompact(activeSprint.startDate) : 'Apr 12' }} - 
                     {{ activeSprint ? formatDateCompact(activeSprint.endDate) : 'Apr 26, 2026' }}
                   </span>
                   <div class="avatar-xxs bg-green">P</div>
                   <button class="icon-btn"><i class="fa-regular fa-star"></i></button>
                   <button class="icon-btn"><i class="fa-solid fa-ellipsis"></i></button>
                </div>
             </div>
             
             <!-- Card Body Split in 3 -->
             <div class="cc-grid">
                <!-- Progress -->
                <div class="grid-panel panel-progress">
                   <div class="gp-header">
                      <span>Progress</span>
                      <span class="sub">1/4 Work item closed</span>
                   </div>
                   <div class="progress-bar-container">
                      <div class="pb-segment bg-green" style="width: 25%"></div>
                      <div class="pb-segment bg-orange" style="width: 25%"></div>
                      <div class="pb-segment bg-darkgray" style="width: 25%"></div>
                      <div class="pb-segment bg-lightgray" style="width: 25%"></div>
                   </div>
                   <div class="legend-list">
                      <div class="legend-item"><span class="dot bg-green"></span> Completed <span class="val">1 Work item</span></div>
                      <div class="legend-item"><span class="dot bg-orange"></span> Started <span class="val">1 Work item</span></div>
                      <div class="legend-item"><span class="dot bg-darkgray"></span> Unstarted <span class="val">1 Work item</span></div>
                      <div class="legend-item"><span class="dot bg-lightgray"></span> Backlog <span class="val">1 Work item</span></div>
                   </div>
                </div>

                <!-- Burndown Chart -->
                <div class="grid-panel panel-chart">
                   <div class="gp-header">
                      <span>Work item burndown</span>
                      <span class="sub text-right">Pending work items - 3</span>
                   </div>
                   <!-- CSS Mock of the graph -->
                   <div class="chart-mockup">
                      <!-- Grid lines -->
                      <div class="grid-line" style="top: 10%"><span>9</span></div>
                      <div class="grid-line" style="top: 25%"><span>7</span></div>
                      <div class="grid-line" style="top: 40%"><span>6</span></div>
                      <div class="grid-line" style="top: 55%"><span>5</span></div>
                      <div class="grid-line" style="top: 70%"><span>4</span></div>
                      <div class="grid-line" style="top: 85%"><span>0</span></div>
                      
                      <!-- Graph line mock -->
                      <svg viewBox="0 0 100 100" preserveAspectRatio="none" style="position:absolute; width:100%; height:100%; top:0; left:20px; right:0;">
                         <!-- Ideal burn -->
                         <line x1="0" y1="70" x2="95" y2="85" stroke="#71717A" stroke-width="1" stroke-dasharray="2,2"/>
                         <circle cx="0" cy="70" r="1.5" fill="#A1A1AA"/>
                         <circle cx="95" cy="85" r="1.5" fill="#A1A1AA"/>

                         <!-- Real burn -->
                         <path d="M 0 70 L 25 70 L 30 85 L 95 85" fill="none" stroke="#3B82F6" stroke-width="1"/>
                         <path d="M 0 70 L 25 70 L 30 85 L 95 85 L 95 100 L 0 100 Z" fill="rgba(59, 130, 246, 0.15)"/>
                         <circle cx="0" cy="70" r="1.5" fill="#3B82F6"/>
                         <circle cx="25" cy="70" r="1.5" fill="#3B82F6"/>
                         <circle cx="30" cy="85" r="1.5" fill="#3B82F6"/>
                      </svg>
                      
                      <!-- X-axis -->
                      <div class="x-axis">
                         <span>Apr 12</span>
                         <span>Apr 14</span>
                         <span>Apr 16</span>
                         <span>Apr 18</span>
                         <span>Apr 20</span>
                         <span>Apr 22</span>
                         <span>Apr 24</span>
                         <span>Apr 26</span>
                      </div>
                      
                      <div class="chart-legend">
                        <span class="leg-item"><span class="box bg-blue"></span> Current work items</span>
                        <span class="leg-item"><span class="box bg-gray"></span> Ideal work items</span>
                      </div>
                   </div>
                </div>

                <!-- Tabs panel -->
                <div class="grid-panel panel-tabs">
                   <div class="tabs-header">
                      <div class="tab-h active">Priority work items</div>
                      <div class="tab-h">Assignees</div>
                      <div class="tab-h">Labels</div>
                   </div>
                   <div class="tabs-body">
                      <div class="tab-row">
                         <div class="tr-user">
                            <i class="fa-regular fa-user avatar-icon"></i> No assignee
                         </div>
                         <div class="tr-stat text-muted">0% of 0</div>
                      </div>
                   </div>
                </div>
             </div>
           </div>
        </div>
      </div>

      <!-- Upcoming Cycle -->
      <div class="cycle-section">
        <div class="cs-header" @click="toggleTab('upcoming')">
          <i class="chevron fa-solid" :class="expandedTabs.upcoming ? 'fa-chevron-down' : 'fa-chevron-right'"></i>
          <i class="fa-regular fa-circle-dashed text-blue"></i>
          <span class="cs-title">Upcoming cycle</span>
          <span class="cs-count">{{ upcomingSprints.length || 1 }}</span>
        </div>
        
        <div class="cs-content" v-show="expandedTabs.upcoming">
           <!-- Collapsed cycle card mimic -->
           <div class="cycle-card collapsed">
             <div class="cct-left">
                <div class="progress-ring text-muted">0%</div>
                <span class="cycle-name">Cycle 2: Collaboration & Customization</span>
             </div>
             <div class="cct-right">
                <div class="avatar-xxs bg-green">P</div>
                <button class="icon-btn"><i class="fa-regular fa-star"></i></button>
                <button class="icon-btn"><i class="fa-solid fa-ellipsis"></i></button>
             </div>
           </div>
        </div>
      </div>

      <!-- Completed Cycle -->
      <div class="cycle-section">
        <div class="cs-header" @click="toggleTab('completed')">
          <i class="chevron fa-solid" :class="expandedTabs.completed ? 'fa-chevron-down' : 'fa-chevron-right'"></i>
          <i class="fa-solid fa-circle-check text-green"></i>
          <span class="cs-title">Completed cycle</span>
          <span class="cs-count">{{ completedSprints.length }}</span>
        </div>
        <div class="cs-content" v-show="expandedTabs.completed">
           <div class="empty-state text-muted" v-if="completedSprints.length === 0">No completed cycles yet.</div>
        </div>
      </div>

    </div>
  </div>
</template>

<style scoped>
.plane-cycles-wrapper {
  display: flex;
  flex-direction: column;
  height: 100%;
  color: #E4E4E7;
  font-family: 'Inter', sans-serif;
}

/* Header */
.cycles-view-header {
  display: flex;
  justify-content: space-between;
  padding: 16px 24px;
  background-color: #0D0F11;
}
.vh-right {
  display: flex;
  gap: 12px;
  margin-left: auto;
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
.cycles-body {
  padding: 0 24px 24px;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.cycle-section {
  display: flex;
  flex-direction: column;
}

.cs-header {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 12px 0;
  cursor: pointer;
  border-bottom: 1px solid #16181D;
  margin-bottom: 8px;
}
.chevron { font-size: 10px; color: #71717A; width: 14px; text-align: center; }
.cs-title { font-size: 14px; font-weight: 600; color: #E4E4E7; }
.cs-count { font-size: 12px; color: #71717A; }

.text-orange { color: #F59E0B; }
.text-blue { color: #3B82F6; }
.text-green { color: #10B981; }
.text-muted { color: #A1A1AA; }
.bg-orange { background-color: #F59E0B; }
.bg-green { background-color: #10B981; }
.bg-darkgray { background-color: #3F3F46; }
.bg-lightgray { background-color: #71717A; }
.bg-blue { background-color: #3B82F6; }
.bg-gray { background-color: #A1A1AA; }

/* Cards */
.cycle-card {
  background: #111315;
  border: 1px solid #1E2025;
  border-radius: 8px;
  padding: 0;
  overflow: hidden;
}
.cycle-card.collapsed {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px 14px;
}

.cc-top {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 20px;
  border-bottom: 1px solid #1E2025;
}

.cct-left { display: flex; align-items: center; gap: 12px; }
.progress-ring {
  width: 28px;
  height: 28px;
  border-radius: 50%;
  border: 2px solid currentColor; /* Simplified ring */
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 10px;
  font-weight: 600;
}
.cycle-name { font-size: 14px; font-weight: 500; }

.cct-right { display: flex; align-items: center; gap: 16px; }
.detail-link { font-size: 12px; color: #38BDF8; cursor: pointer; display: flex; align-items: center; gap: 6px; }
.date-range { font-size: 12px; color: #A1A1AA; display: flex; align-items: center; gap: 6px; }

.avatar-xxs {
  width: 18px;
  height: 18px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 9px;
  font-weight: 600;
  color: white;
}

.icon-btn { background: transparent; border: none; color: #71717A; cursor: pointer; font-size: 13px; padding: 4px; }
.icon-btn:hover { color: #E4E4E7; }

/* 3 Pane Grid */
.cc-grid {
  display: grid;
  grid-template-columns: 1fr 1fr 1fr;
  min-height: 250px;
}
.grid-panel {
  padding: 20px;
  border-right: 1px solid #1E2025;
}
.grid-panel:last-child { border-right: none; }

.gp-header {
  display: flex;
  justify-content: space-between;
  font-size: 13px;
  font-weight: 500;
  color: #E4E4E7;
  margin-bottom: 24px;
}
.gp-header .sub { font-size: 12px; color: #71717A; font-weight: 400; }

/* Progress Panel */
.progress-bar-container {
  display: flex;
  height: 8px;
  border-radius: 4px;
  overflow: hidden;
  gap: 2px;
  margin-bottom: 16px;
}
.pb-segment { height: 100%; border-radius: 2px; }
.legend-list { display: flex; flex-direction: column; gap: 12px; }
.legend-item { display: flex; align-items: center; font-size: 12px; color: #A1A1AA; position: relative; }
.legend-item .val { margin-left: auto; color: #E4E4E7; }
.legend-item .dot { width: 8px; height: 8px; border-radius: 50%; margin-right: 8px; }

/* Chart Panel */
.panel-chart { position: relative; display: flex; flex-direction: column; }
.chart-mockup {
  position: relative;
  flex: 1;
  margin-bottom: 16px;
  overflow: hidden;
}
.grid-line {
  position: absolute;
  left: 0;
  right: 0;
  height: 1px;
  background: rgba(255,255,255,0.05);
}
.grid-line span {
  position: absolute;
  left: 0;
  top: -6px;
  font-size: 10px;
  color: #71717A;
}
.x-axis {
  position: absolute;
  bottom: 24px;
  left: 20px;
  right: 0;
  display: flex;
  justify-content: space-between;
  font-size: 9px;
  color: #71717A;
  text-transform: uppercase;
}
.chart-legend {
  position: absolute;
  bottom: 0px;
  left: 20px;
  right: 0;
  display: flex;
  justify-content: center;
  gap: 16px;
  font-size: 10px;
  color: #A1A1AA;
}
.leg-item { display: flex; align-items: center; gap: 4px; }
.leg-item .box { width: 6px; height: 6px; }

/* Tabs Panel */
.panel-tabs { display: flex; flex-direction: column; padding: 0; }
.tabs-header {
  display: flex;
  background: #1E2025;
}
.tab-h {
  flex: 1;
  text-align: center;
  padding: 8px 0;
  font-size: 12px;
  color: #71717A;
  background: #111315;
  border-bottom: 1px solid #1E2025;
  border-right: 1px solid #1E2025;
  cursor: pointer;
}
.tab-h.active {
  background: #1E2025;
  color: #E4E4E7;
  border-bottom: none;
}
.tabs-body { padding: 16px 20px; }
.tab-row { display: flex; justify-content: space-between; align-items: center; font-size: 12px; }
.tr-user { display: flex; align-items: center; gap: 8px; color: #A1A1AA; }
.avatar-icon { background: #27272A; width: 24px; height: 24px; display: flex; align-items: center; justify-content: center; border-radius: 50%; font-size: 10px; color: #71717A;}

.empty-state {
  padding: 16px 30px;
  font-size: 13px;
}
</style>
