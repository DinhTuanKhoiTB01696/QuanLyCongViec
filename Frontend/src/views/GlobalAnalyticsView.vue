<script setup>
import { ref } from 'vue'
import NexusLayout from '@/components/layout/NexusLayout.vue'

const activeTab = ref('Overview')

const stats = [
  { label: 'Total Users', value: 1 },
  { label: 'Total Admins', value: 1 },
  { label: 'Total Members', value: 0 },
  { label: 'Total Guests', value: 0 },
  { label: 'Total Projects', value: 1 },
  { label: 'Total Work items', value: 8 },
  { label: 'Total Cycles', value: 2 },
  { label: 'Total Intake', value: 0 }
]

const summaryData = [
  { label: 'Work items', value: 8 },
  { label: 'Cycles', value: 2 },
  { label: 'Modules', value: 3 },
  { label: 'Intake', value: 0 },
  { label: 'Members', value: 2 },
  { label: 'Pages', value: 2 },
  { label: 'Views', value: 1 }
]

</script>

<template>
  <NexusLayout>
    <div class="analytics-wrapper">
      <header class="an-header">
         <div class="an-top-row">
            <span class="breadcrumb"><i class="fa-solid fa-chart-simple"></i> Analytics</span>
            <div class="ml-auto"></div>
         </div>
         <div class="an-bottom-row">
            <div class="an-tabs">
               <button class="tab-btn" :class="{ active: activeTab === 'Overview' }" @click="activeTab = 'Overview'">Overview</button>
               <button class="tab-btn" :class="{ active: activeTab === 'Work items' }" @click="activeTab = 'Work items'">Work items</button>
            </div>
            <button class="plane-toolbar-btn ms-auto"><i class="fa-solid fa-briefcase"></i> All projects <i class="fa-solid fa-chevron-down ms-2"></i></button>
         </div>
      </header>
      
      <!-- OVERVIEW TAB -->
      <div class="an-scrollable" v-if="activeTab === 'Overview'">
         <h2 class="page-title">Overview</h2>
         
         <div class="stats-grid">
            <div class="stat-box" v-for="s in stats" :key="s.label">
               <div class="stat-lbl">{{ s.label }}</div>
               <div class="stat-val">{{ s.value }}</div>
            </div>
         </div>
         
         <div class="insights-grid">
            <!-- Radar Chart -->
            <div class="insight-box">
               <div class="insight-title">Project Insights</div>
               <div class="radar-container mt-4">
                 <!-- Mocking Spider chart visually with an SVG -->
                 <svg width="240" height="240" viewBox="0 0 200 200">
                    <!-- Spider web lines -->
                    <polygon points="100,10 180,50 180,150 100,190 20,150 20,50" fill="none" stroke="#27272A" />
                    <polygon points="100,30 165,65 165,135 100,170 35,135 35,65" fill="none" stroke="#27272A" />
                    <polygon points="100,50 145,75 145,125 100,150 55,125 55,75" fill="none" stroke="#27272A" />
                    <polygon points="100,70 125,85 125,115 100,130 75,115 75,85" fill="none" stroke="#27272A" />
                    <!-- Cross lines -->
                    <line x1="100" y1="10" x2="100" y2="190" stroke="#27272A" />
                    <line x1="20" y1="50" x2="180" y2="150" stroke="#27272A" />
                    <line x1="180" y1="50" x2="20" y2="150" stroke="#27272A" />
                    
                    <!-- Labels -->
                    <text x="85" y="8" fill="#A1A1AA" font-size="10">Work items</text>
                    <text x="185" y="45" fill="#A1A1AA" font-size="10">Cycles</text>
                    <text x="185" y="160" fill="#A1A1AA" font-size="10">Modules</text>
                    <text x="85" y="205" fill="#A1A1AA" font-size="10">Intake</text>
                    <text x="5" y="160" fill="#A1A1AA" font-size="10">Members</text>
                    <text x="5" y="45" fill="#A1A1AA" font-size="10">Views</text>
                    
                    <!-- Data shape -->
                    <polygon points="100,20 120,95 140,120 100,110 80,125 90,90" fill="rgba(14, 165, 233, 0.4)" stroke="#0EA5E9" stroke-width="2" />
                    <circle cx="100" cy="20" r="4" fill="#0EA5E9" />
                    <circle cx="120" cy="95" r="4" fill="#0EA5E9" />
                    <circle cx="140" cy="120" r="4" fill="#0EA5E9" />
                    <circle cx="100" cy="110" r="4" fill="#0EA5E9" />
                    <circle cx="80" cy="125" r="4" fill="#0EA5E9" />
                    <circle cx="90" cy="90" r="4" fill="#0EA5E9" />
                 </svg>
               </div>
            </div>
            
            <!-- Summary list -->
            <div class="insight-box">
               <div class="insight-title" style="color: #71717A; font-weight: 500;">Summary of Projects</div>
               <div class="insight-title mt-2">All Projects</div>
               <div class="insight-title" style="color: #71717A; font-weight: 500; font-size: 13px; margin: 12px 0 24px;">Trend on charts</div>
               
               <div class="summary-list">
                  <div class="sum-row" v-for="d in summaryData" :key="d.label">
                     <span class="sum-lbl">{{ d.label }}</span>
                     <span class="sum-val">{{ d.value }}</span>
                  </div>
               </div>
            </div>
            
            <!-- Active Projects -->
            <div class="insight-box">
               <div class="insight-title">Active Projects</div>
               <div class="active-proj mt-4">
                  <div class="proj-badge">
                     <i class="fa-solid fa-bell" style="color: #F59E0B"></i>
                     <span>Cun</span>
                  </div>
                  <div class="pill-red">13%</div>
               </div>
            </div>
         </div>
      </div>

      <!-- WORK ITEMS TAB (Expanded Analytics Sidebar style) -->
      <div class="an-scrollable" v-else>
         <div class="full-analytics-body">
            <!-- Stats -->
            <div class="ap-stats-grid">
               <div class="stat-box">
                  <span class="lbl">Total Work items</span>
                  <span class="val">8</span>
               </div>
               <div class="stat-box">
                  <span class="lbl">Started Work items</span>
                  <span class="val">2</span>
               </div>
               <div class="stat-box">
                  <span class="lbl">Backlog Work items</span>
                  <span class="val">4</span>
               </div>
               <div class="stat-box">
                  <span class="lbl">Unstarted Work items</span>
                  <span class="val">1</span>
               </div>
               <div class="stat-box">
                  <span class="lbl">Completed Work items</span>
                  <span class="val">1</span>
               </div>
            </div>
            
            <!-- Created vs Resolved Chart Overlay -->
            <div class="ap-chart-card mt-4">
               <h4>Created vs Resolved</h4>
               <div class="line-chart-mock">
                  <div class="grid-l" style="bottom: 90%;"><span>9</span></div>
                  <div class="grid-l" style="bottom: 70%;"><span>7</span></div>
                  <div class="grid-l" style="bottom: 50%;"><span>5</span></div>
                  <div class="grid-l" style="bottom: 30%;"><span>3</span></div>
                  <div class="grid-l" style="bottom: 10%;"><span>0</span></div>
                  
                  <div class="dot blue" style="bottom: 90%; left: 50%;"></div>
                  <div class="dot green" style="bottom: 10%; left: 50%;"></div>
                  
                  <div class="x-label" style="left: 50%; transform: translateX(-50%);">Apr 01, 2026</div>
               </div>
               <div class="chart-legend mt-2">
                  <span class="leg-item"><span class="box bg-green"></span> Resolved</span>
                  <span class="leg-item"><span class="box bg-blue"></span> Created</span>
               </div>
            </div>

            <!-- Customized Insights -->
            <div class="ap-chart-card mt-4">
               <div class="flex-between">
                  <h4>Customized Insights</h4>
                  <div class="insight-filters">
                     <button class="filter-btn"><i class="fa-solid fa-briefcase"></i> Work item <i class="fa-solid fa-chevron-down"></i></button>
                     <button class="filter-btn"><i class="fa-solid fa-list"></i> Priority <i class="fa-solid fa-chevron-down"></i></button>
                     <button class="filter-btn"><i class="fa-solid fa-plus-minus"></i> Add Property <i class="fa-solid fa-chevron-down"></i></button>
                  </div>
               </div>
               
               <div class="bar-chart-mock mt-4">
                  <div class="grid-l" style="bottom: 90%;"><span>9</span></div>
                  <div class="grid-l" style="bottom: 70%;"><span>7</span></div>
                  <div class="grid-l" style="bottom: 50%;"><span>5</span></div>
                  <div class="grid-l" style="bottom: 30%;"><span>3</span></div>
                  <div class="grid-l" style="bottom: 10%;"><span>0</span></div>

                  <div class="bars-container">
                     <div class="bar-wrapper">
                        <div class="bar bg-orange" style="height: 30%;"></div>
                        <span class="bar-lbl">High</span>
                     </div>
                     <div class="bar-wrapper">
                        <div class="bar bg-green" style="height: 10%;"></div>
                        <span class="bar-lbl">Low</span>
                     </div>
                     <div class="bar-wrapper">
                        <div class="bar bg-gray" style="height: 30%;"></div>
                        <span class="bar-lbl">None</span>
                     </div>
                     <div class="bar-wrapper">
                        <div class="bar bg-red" style="height: 10%;"></div>
                        <span class="bar-lbl">Urgent</span>
                     </div>
                  </div>
                  <div class="y-label">NO. OF WORK ITEM</div>
               </div>
            </div>
            
         </div>
      </div>
    </div>
  </NexusLayout>
</template>

<style scoped>
.analytics-wrapper { display: flex; flex-direction: column; height: 100vh; background: #0D0F11; color: #E4E4E7; }
.an-header { padding: 16px 24px 0; border-bottom: 1px solid #1E2025; }
.an-top-row { display: flex; align-items: center; margin-bottom: 24px; }
.breadcrumb { color: #A1A1AA; font-size: 14px; display: flex; align-items: center; gap: 8px; font-weight: 500;}
.an-bottom-row { display: flex; align-items: center; margin-bottom: -1px; }

.an-tabs { display: flex; gap: 24px; }
.tab-btn { background: transparent; border: none; font-size: 13px; font-weight: 500; color: #71717A; cursor: pointer; padding: 8px 0; border-bottom: 2px solid transparent; }
.tab-btn:hover { color: #E4E4E7; }
.tab-btn.active { color: #E4E4E7; border-bottom: 2px solid #E4E4E7; }

.ms-auto { margin-left: auto; }
.ms-2 { margin-left: 8px; font-size: 10px; }
.plane-toolbar-btn { background: #16181D; border: 1px solid #27272A; color: #D4D4D8; font-size: 13px; font-weight: 500; cursor: pointer; padding: 6px 12px; border-radius: 6px; margin-bottom: 12px; display: flex; align-items: center; gap: 6px; }
.plane-toolbar-btn:hover { background: #1E2025; }

.an-scrollable { padding: 32px; overflow-y: auto; flex: 1; }
.page-title { margin: 0 0 32px 0; font-size: 20px; font-weight: 600; }

.stats-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 24px; margin-bottom: 40px; }
.stat-box { display: flex; flex-direction: column; gap: 8px; }
.stat-lbl { font-size: 12px; color: #71717A; font-weight: 500; }
.stat-val { font-size: 20px; font-weight: 600; color: #E4E4E7; }

.insights-grid { display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 40px; }
.insight-title { font-size: 15px; font-weight: 600; }
.radar-container { display: flex; align-items: center; justify-content: center; padding: 20px 0; }
.mt-4 { margin-top: 24px; }
.mt-2 { margin-top: 8px; }

.summary-list { display: flex; flex-direction: column; gap: 20px; }
.sum-row { display: flex; justify-content: space-between; font-size: 13px; }
.sum-lbl { color: #E4E4E7; }
.sum-val { color: #A1A1AA; font-weight: 500; }

.active-proj { display: flex; justify-content: space-between; align-items: center; }
.proj-badge { display: flex; align-items: center; gap: 8px; font-size: 13px; font-weight: 500; }
.pill-red { background: rgba(153, 27, 27, 0.2); color: #F87171; font-size: 11px; padding: 2px 6px; border-radius: 4px; font-weight: 600; }

/* Expanded Analytics (copied and adapted from SpaceSummary.vue) */
.full-analytics-body { width: 100%; max-width: 1000px;}
.ap-stats-grid { display: flex; gap: 24px; flex-wrap: wrap; }
.ap-stats-grid .stat-box { flex: 1; min-width: 150px; }
.ap-chart-card { margin-top: 32px; }
.ap-chart-card h4 { margin: 0; font-size: 14px; font-weight: 600; color: #E4E4E7; }
.line-chart-mock { position: relative; height: 200px; margin-top: 16px; border-bottom: 1px solid #27272A; }
.grid-l { position: absolute; width: 100%; border-top: 1px solid #1E2025; }
.grid-l span { position: absolute; left: -20px; top: -8px; font-size: 10px; color: #71717A; }
.dot { position: absolute; width: 6px; height: 6px; border-radius: 50%; transform: translate(-50%, 50%); border: 2px solid; background: #111315; }
.dot.blue { border-color: #0EA5E9; z-index: 2; }
.dot.green { border-color: #10B981; z-index: 1; }
.x-label { position: absolute; bottom: -20px; font-size: 11px; color: #71717A; }
.chart-legend { display: flex; gap: 16px; font-size: 12px; color: #E4E4E7; margin-top: 24px; }
.leg-item { display: flex; align-items: center; gap: 8px; font-weight: 500; }
.box { width: 8px; height: 8px; border-radius: 2px; }
.bg-green { background: #10B981; }
.bg-blue { background: #0EA5E9; }

.insight-filters { display: flex; gap: 8px; }
.filter-btn { background: transparent; border: 1px solid #27272A; border-radius: 4px; padding: 4px 8px; font-size: 12px; color: #D4D4D8; cursor: pointer; }
.filter-btn i { color: #A1A1AA; font-size: 10px; }
.flex-between { display: flex; justify-content: space-between; align-items: center; }

.bar-chart-mock { position: relative; height: 250px; margin-top: 24px; border-bottom: 1px solid #27272A; }
.bars-container { display: flex; justify-content: space-around; align-items: flex-end; height: 100%; padding-bottom: 1px; }
.bar-wrapper { display: flex; flex-direction: column; align-items: center; gap: 8px; height: 100%; justify-content: flex-end; width: 40px; }
.bar { width: 100%; border-radius: 4px 4px 0 0; }
.bar-lbl { position: absolute; bottom: -24px; font-size: 12px; color: #A1A1AA; }
.bg-orange { background: #F97316; }
.bg-gray { background: #D4D4D8; }
.bg-red { background: #EF4444; }
.y-label { position: absolute; left: -40px; top: 50%; transform: rotate(-90deg) translateY(-50%); font-size: 10px; color: #71717A; letter-spacing: 1px; }
</style>
