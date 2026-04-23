<script setup>
import { ref, onMounted, computed } from 'vue'
import axiosClient from '@/api/axiosClient'
import NexusLayout from '@/components/layout/NexusLayout.vue'
import { useActivityStore } from '@/store/useActivityStore'

const activeTab = ref('Summary')
const tabs = ['Summary', 'Assigned', 'Created', 'Subscribed', 'Activity']

const myTasks = ref([])
const loading = ref(false)
const actStore = useActivityStore()
const selectedProjectId = ref(null)
const projectList = ref([])

import { ElNotification, ElMessage } from 'element-plus'

const currentUserId = computed(() => {
  const u = localStorage.getItem('user')
  return u ? JSON.parse(u).id : null
})

const fetchProjects = async () => {
    try {
        const [discoveryRes, archivedRes] = await Promise.all([
            axiosClient.get('/projects/discovery'),
            axiosClient.get('/projects/archived')
        ])
        const activeProjects = (discoveryRes.data?.data || []).map(p => ({ ...p, isArchived: false }))
        const archivedProjects = (archivedRes.data?.data || []).map(p => ({ ...p, isArchived: true }))
        projectList.value = [...activeProjects, ...archivedProjects]
    } catch(e) {
        console.error('Error fetching projects', e)
    }
}

const fetchMyTasks = async () => {
  try {
    loading.value = true
    const params = {}
    if (selectedProjectId.value) {
        params.projectId = selectedProjectId.value
    }
    
    // Fetch tasks
    const res = await axiosClient.get('/tasks/search', { params })
    myTasks.value = res.data?.data || []
    
    // Fetch real activities from store (which now calls API)
    await actStore.fetchRecentActivities(params)
    
  } catch (err) {
    console.error('Lỗi load tasks:', err)
    ElMessage.error('Failed to load tasks.')
  } finally {
    loading.value = false
  }
}

onMounted(async () => {
  await fetchProjects()
  fetchMyTasks()
})

const handleProjectChange = () => {
    fetchMyTasks()
}

const seedMockData = async () => {
  try {
    loading.value = true
    const res = await axiosClient.post('/tasks/seed-mock')
    ElNotification({ title: 'Thành công', message: res.data.message || 'Đã tạo dữ liệu mẫu', type: 'success' })
    await fetchMyTasks()
  } catch (err) {
    ElNotification({ title: 'Lỗi', message: 'Lỗi khi tạo dữ liệu mẫu', type: 'error' })
    console.error(err)
  } finally {
    loading.value = false
  }
}

const overview = computed(() => ({
  created: myTasks.value.filter(t => t.reporterId === currentUserId.value).length, 
  assigned: myTasks.value.filter(t => t.assignedUserId === currentUserId.value).length, 
  subscribed: myTasks.value.filter(t => t.isSubscribed).length
}))

const workload = computed(() => {
  let backlog = 0, notStarted = 0, workingOn = 0, completed = 0, canceled = 0
  myTasks.value.forEach(t => {
    const s = (t.statusName || 'BACKLOG').toUpperCase().trim()
    if (s === 'BACKLOG') backlog++
    else if (s === 'TODO' || s === 'TO DO') notStarted++
    else if (s === 'IN PROGRESS' || s === 'INPROGRESS') workingOn++
    else if (s === 'DONE') completed++
    else if (s === 'CANCELED') canceled++
  })
  return { backlog, notStarted, workingOn, completed, canceled }
})

const recentActivity = computed(() => {
  return actStore.activities.map(a => ({
    id: a.id,
    text: a.text + ' ' + a.bold,
    time: a.time
  }))
})

const listData = computed(() => {
  let list = []
  if (activeTab.value === 'Assigned') list = myTasks.value.filter(t => t.assignedUserId === currentUserId.value)
  else if (activeTab.value === 'Created') list = myTasks.value.filter(t => t.reporterId === currentUserId.value)
  else if (activeTab.value === 'Subscribed') list = myTasks.value.filter(t => t.isSubscribed)
  else list = myTasks.value
  
  return list.map(t => ({
    id: t.sequenceId || t.id.substring(0,8).toUpperCase(),
    rawId: t.id,
    title: t.title,
    state: t.statusName || 'To Do',
    priority: t.priority || 3,
    assigneeName: t.assigneeName,
    modules: t.moduleNames?.join(', ') || '0 Modules',
    cycle: t.sprintName || 'No Cycle',
    task: t
  }))
})

const updateTaskProperty = async (task, field, value) => {
   try {
      const idx = myTasks.value.findIndex(t => t.id === task.id)
      if (idx !== -1) {
         // Optimistic update
         const oldValue = myTasks.value[idx][field]
         myTasks.value[idx][field] = value
         
         const updatePayload = {}
         // Map statusName if needed (backend expects statusName or TaskStatusId in different contexts, but PartialUpdate handles statusName)
         updatePayload[field] = value
         
         if (task.projectId) {
            try {
                await axiosClient.patch(`/projects/${task.projectId}/WorkTasks/${task.id}`, updatePayload)
                ElMessage.success('Updated successfully')
                // Refresh activities to show the change
                actStore.fetchRecentActivities({ projectId: selectedProjectId.value })
            } catch (err) {
                // Rollback
                myTasks.value[idx][field] = oldValue
                const msg = err.response?.data?.message || 'Failed to update task'
                ElMessage.error(msg)
            }
         }
      }
   } catch (error) {
      console.error('Failed to update task:', error);
   }
}

const pageActivities = computed(() => actStore.activities)

const downloadWordActivity = () => {
    let htmlContent = `
    <html xmlns:o='urn:schemas-microsoft-com:office:office' xmlns:w='urn:schemas-microsoft-com:office:word' xmlns='http://www.w3.org/TR/REC-html40'>
    <head><meta charset='utf-8'><title>Activity Log</title></head><body>
    <h2>Lịch sử hoạt động (Activity History)</h2>
    <ul>
    `;
    
    actStore.activities.forEach(act => {
        htmlContent += `<li><strong>${act.time}</strong> - ${act.text} <em>${act.bold}</em></li>`;
    });
    
    htmlContent += "</ul></body></html>";
    
    const blob = new Blob(['\ufeff', htmlContent], { type: 'application/msword' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `Activity_Log_${new Date().toISOString().slice(0,10)}.doc`;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
    
    ElNotification({ title: 'Thành công', message: 'Đã xuất file Word lịch sử hoạt động', type: 'success' });
}
</script>

<template>
  <NexusLayout>
    <div class="yw-container">
      
      <!-- Main Content -->
      <div class="yw-main">
        <header class="nexus-feature-header">
           <div class="header-info">
             <p class="eyebrow">Personal Dashboard</p>
             <h1><i class="fa-solid fa-user"></i> Your work</h1>
             <p class="muted">Monitor your assignments, track activities, and manage your contributions across projects.</p>
           </div>
           
           <div class="nexus-controls-row">
             <el-select 
                v-model="selectedProjectId" 
                placeholder="Filter by Project" 
                clearable 
                filterable
                @change="handleProjectChange"
                style="width: 220px;"
                class="nexus-project-select"
             >
                <el-option
                   v-for="p in projectList"
                   :key="p.id"
                   :label="`[${p.key || 'PRJ'}] ${p.name}`"
                   :value="p.id"
                >
                   <div class="flex items-center gap-2">
                      <div class="project-mini-icon" :style="{ background: p.isArchived ? '#71717A' : '#3b82f6' }">
                         {{ p.key?.substring(0,2) || 'PJ' }}
                      </div>
                      <span>{{ p.name }}</span>
                      <el-tag v-if="p.isArchived" size="small" type="info" class="ml-auto">Archived</el-tag>
                   </div>
                </el-option>
             </el-select>
             <button class="nexus-btn nexus-btn-primary" @click="seedMockData" style="background: #10B981">
                <i class="fa-solid fa-seedling"></i> Seed Data
             </button>
           </div>
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
                 <div class="empty-chart" v-if="myTasks.length === 0">
                    <i class="fa-solid fa-chart-simple chart-icon"></i>
                    <span>No work item assigned yet</span>
                 </div>
                 <div v-else class="text-sm p-4 text-gray-400 border border-[#27272A] rounded-lg mt-2 bg-[#16181D]">
                    <div class="flex justify-between mb-2"><span>Urgent</span> <span class="text-white">{{ myTasks.filter(t=>t.priority===1).length }}</span></div>
                    <div class="flex justify-between mb-2"><span>High</span> <span class="text-white">{{ myTasks.filter(t=>t.priority===2).length }}</span></div>
                    <div class="flex justify-between mb-2"><span>Medium</span> <span class="text-white">{{ myTasks.filter(t=>t.priority===3).length }}</span></div>
                    <div class="flex justify-between"><span>Low</span> <span class="text-white">{{ myTasks.filter(t=>t.priority===4).length }}</span></div>
                 </div>
              </div>
              <div class="chart-col">
                 <h3 class="section-title">Work items by state</h3>
                 <div class="empty-chart" v-if="myTasks.length === 0">
                    <i class="fa-solid fa-chart-column chart-icon"></i>
                    <span>No work item assigned yet</span>
                 </div>
                 <div v-else class="text-sm p-4 text-gray-400 border border-[#27272A] rounded-lg mt-2 bg-[#16181D]">
                    <div class="flex justify-between mb-2"><span>Backlog</span> <span class="text-white">{{ workload.backlog }}</span></div>
                    <div class="flex justify-between mb-2"><span>Not Started</span> <span class="text-white">{{ workload.notStarted }}</span></div>
                    <div class="flex justify-between mb-2"><span>In Progress</span> <span class="text-white">{{ workload.workingOn }}</span></div>
                    <div class="flex justify-between"><span>Completed</span> <span class="text-white">{{ workload.completed }}</span></div>
                 </div>
              </div>
           </div>

           <!-- Recent activity -->
           <h3 class="section-title mt-4">Recent activity</h3>
           <div class="list-body">
              <div class="list-row" style="cursor: default;" v-for="act in recentActivity" :key="act.id">
                 <div class="lr-left">
                    <span class="lr-id" style="min-width: 30px;"><i class="fa-solid fa-clock-rotate-left" style="color: #A1A1AA"></i></span>
                    <span class="lr-title">{{ act.text }}</span>
                 </div>
                 <div class="lr-right">
                    <div class="lr-badge cursor-not-allowed">
                       <i class="fa-regular fa-clock"></i> {{ act.time }}
                    </div>
                 </div>
              </div>
           </div>
        </div>

        <!-- LIST VIEWS (Assigned, Created, Subscribed) -->
        <div class="yw-scrollable" v-else-if="['Assigned', 'Created', 'Subscribed'].includes(activeTab)">
           <div class="list-header mt-4">
              <i class="fa-solid fa-circle-dashed f-icon"></i> 
              <span class="lh-title">All work items</span>
              <span class="lh-count">{{ listData.length }}</span>
           </div>
           
           <div class="list-body mt-4">
              <div class="list-row" v-for="item in listData" :key="item.id">
                 <div class="lr-left">
                    <span class="lr-id">{{ item.id }}</span>
                    <span class="lr-title">{{ item.title }}</span>
                 </div>
                  <div class="lr-right">
                    <!-- Status Dropdown -->
                    <el-dropdown trigger="click" @command="(val) => updateTaskProperty(item.task, 'statusName', val)">
                      <div class="lr-badge cursor-pointer hover:bg-[#1E2025]">
                         <i class="fa-solid fa-circle-check" v-if="item.state.toUpperCase() === 'DONE'"></i>
                         <i class="fa-solid fa-circle-half-stroke" v-else-if="item.state.toUpperCase() === 'IN PROGRESS'"></i>
                         <i class="fa-regular fa-circle" v-else></i>
                         {{ item.state }}
                      </div>
                      <template #dropdown>
                        <el-dropdown-menu class="plane-dropdown">
                          <el-dropdown-item command="BACKLOG">Backlog</el-dropdown-item>
                          <el-dropdown-item command="TO DO">To Do</el-dropdown-item>
                          <el-dropdown-item command="IN PROGRESS">In Progress</el-dropdown-item>
                          <el-dropdown-item command="IN REVIEW">In Review</el-dropdown-item>
                          <el-dropdown-item command="DONE">Done</el-dropdown-item>
                        </el-dropdown-menu>
                      </template>
                    </el-dropdown>

                    <!-- Priority Dropdown -->
                    <el-dropdown trigger="click" @command="(val) => updateTaskProperty(item.task, 'priority', val)">
                      <div class="lr-badge cursor-pointer hover:bg-[#1E2025]">
                         <i class="fa-solid fa-angles-up text-red-500" v-if="item.priority === 1"></i>
                         <i class="fa-solid fa-chevron-up text-orange-500" v-else-if="item.priority === 2"></i>
                         <i class="fa-solid fa-minus text-blue-500" v-else-if="item.priority === 3"></i>
                         <i class="fa-solid fa-chevron-down text-gray-400" v-else></i>
                      </div>
                      <template #dropdown>
                        <el-dropdown-menu class="plane-dropdown">
                          <el-dropdown-item :command="1"><i class="fa-solid fa-angles-up text-red-500"></i> Urgent</el-dropdown-item>
                          <el-dropdown-item :command="2"><i class="fa-solid fa-chevron-up text-orange-500"></i> High</el-dropdown-item>
                          <el-dropdown-item :command="3"><i class="fa-solid fa-minus text-blue-500"></i> Normal</el-dropdown-item>
                          <el-dropdown-item :command="4"><i class="fa-solid fa-chevron-down text-gray-400"></i> Low</el-dropdown-item>
                        </el-dropdown-menu>
                      </template>
                    </el-dropdown>

                    <div class="lr-badge cursor-not-allowed">
                       <i class="fa-regular fa-user" v-if="!item.assigneeName"></i>
                       <span v-else>{{ item.assigneeName.substring(0,1).toUpperCase() }}</span>
                    </div>
                    <div class="lr-badge cursor-not-allowed"><i class="fa-regular fa-calendar"></i></div>
                    <div class="lr-badge cursor-not-allowed"><i class="fa-solid fa-table-cells-large"></i> {{ item.modules }}</div>
                    <div class="lr-badge cursor-not-allowed"><i class="fa-solid fa-arrows-spin"></i> {{ item.cycle }}</div>
                  </div>
              </div>
           </div>
        </div>

        <!-- ACTIVITY VIEW -->
        <div class="yw-scrollable" v-else-if="activeTab === 'Activity'">
           <div class="activity-page-header mt-4 flex-between">
              <h3 class="section-title" style="margin: 0;">Recent activity</h3>
              <button class="plane-primary-btn" @click="downloadWordActivity">Download today's activity</button>
           </div>
           
           <div class="list-body mt-4">
              <div class="list-row" style="cursor: default;" v-for="(act, idx) in pageActivities" :key="idx">
                 <div class="lr-left">
                    <span class="lr-id" style="min-width: 30px;"><i :class="act.icon || 'fa-solid fa-bell'"></i></span>
                    <span class="lr-title">{{ act.text }} <span class="p-ac-bold text-white">{{ act.bold }}</span></span>
                 </div>
                 <div class="lr-right">
                    <div class="lr-badge cursor-not-allowed">
                       <i class="fa-regular fa-clock"></i> {{ act.time }}
                    </div>
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
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.project-mini-icon {
  width: 24px;
  height: 24px;
  border-radius: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 10px;
  font-weight: 700;
  color: white;
  text-transform: uppercase;
}

:deep(.nexus-project-select .el-input__wrapper) {
  background-color: #1E2025 !important;
  box-shadow: 0 0 0 1px #3F3F46 inset !important;
}

:deep(.nexus-project-select .el-input__inner) {
  color: #E4E4E7 !important;
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
