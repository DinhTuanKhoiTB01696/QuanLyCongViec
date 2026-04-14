<template>
  <NexusLayout>
    <div class="plane-board-container">
      
      <!-- Plane Style Header -->
      <header class="plane-space-header">
        <div class="sh-left">
          <div class="breadcrumb">
            <span class="proj-icon">C</span>
            <span class="proj-name">{{ project?.name || 'Cun' }}</span>
            <i class="fa-solid fa-chevron-right separator"></i>
            <span class="active-page">
              <i class="fa-solid fa-layer-group"></i> Work Items
            </span>
            <span class="item-count">{{ rawTasks.length || 8 }}</span>
          </div>
        </div>
        
        <div class="sh-right">
          <!-- View Toggles -->
          <div class="view-toggles">
            <button class="toggle-btn" :class="{ active: currentTab === 'list' }" @click="currentTab = 'list'" title="List view"><i class="fa-solid fa-bars"></i></button>
            <button class="toggle-btn" :class="{ active: currentTab === 'board' }" @click="currentTab = 'board'" title="Kanban view"><i class="fa-solid fa-table-columns"></i></button>
            <button class="toggle-btn" :class="{ active: currentTab === 'calendar' }" @click="currentTab = 'calendar'" title="Calendar view"><i class="fa-regular fa-calendar"></i></button>
            <button class="toggle-btn" :class="{ active: currentTab === 'spreadsheet' }" @click="currentTab = 'spreadsheet'" title="Spreadsheet view"><i class="fa-solid fa-table-cells"></i></button>
            <button class="toggle-btn" :class="{ active: currentTab === 'timeline' }" @click="currentTab = 'timeline'" title="Gantt chart view"><i class="fa-solid fa-chart-gantt"></i></button>
          </div>

          <button class="plane-toolbar-btn">
            <i class="fa-solid fa-filter"></i>
          </button>
          
          <!-- Display Dropdown -->
          <div class="display-dropdown-wrapper">
             <button class="plane-toolbar-btn" @click.stop="showDisplayDropdown = !showDisplayDropdown" :class="{ 'active': showDisplayDropdown }">Display</button>
             <div class="plane-dropdown-menu" v-show="showDisplayDropdown" @click.stop>
                <div class="dd-section">
                   <div class="dd-title">
                      <span>Display Properties</span>
                      <i class="fa-solid fa-chevron-up"></i>
                   </div>
                   <div class="dd-btns">
                      <button class="dd-tag active">ID</button>
                   </div>
                </div>
                <div class="dd-section border-top">
                   <div class="dd-title">
                      <span>Order by</span>
                      <i class="fa-solid fa-chevron-up"></i>
                   </div>
                   <div class="dd-list">
                      <label class="dd-item"><input type="radio" name="order" checked /> Manual</label>
                      <label class="dd-item"><input type="radio" name="order" /> Last created</label>
                      <label class="dd-item"><input type="radio" name="order" /> Last updated</label>
                      <label class="dd-item"><input type="radio" name="order" /> Start date</label>
                      <label class="dd-item"><input type="radio" name="order" /> Priority</label>
                   </div>
                </div>
                <div class="dd-section border-top">
                   <label class="dd-item checkbox">
                      <input type="checkbox" checked /> Show sub-work items
                   </label>
                </div>
             </div>
          </div>
          
          <button class="plane-toolbar-btn" @click="showAnalyticsSidebar = true">Analytics</button>
          
          <button class="plane-primary-btn" @click="openCreateTask('TO DO')">
            Add work item
          </button>
        </div>
      </header>

      <!-- Other Tab Views -->
      <div v-if="currentTab === 'list'" class="list-wrapper" style="padding: 16px;">
         <ListView :tasks="filteredTasksList" @task-click="openTaskDetail" />
      </div>
      <div v-if="currentTab === 'calendar'" class="calendar-wrapper">
         <CalendarTab :projectId="projectId" @open-task="openTaskDetail" />
      </div>
      <div v-if="currentTab === 'spreadsheet'" class="spreadsheet-wrapper" style="display: flex; flex: 1; overflow: hidden;">
         <SpreadsheetTab :tasks="rawTasks" />
      </div>
      <div v-if="currentTab === 'timeline'" class="timeline-wrapper">
         <TimelineTab :projectId="projectId" @open-task="openTaskDetail" />
      </div>

      <!-- Kanban Board Layout -->
      <div class="kanban-wrapper" v-if="currentTab === 'board'">
        
        <div class="kanban-col" v-for="col in kanbanColumns" :key="col.id">
          <div class="col-head">
            <div class="col-title">
              <i :class="col.icon" :style="{ color: col.color }"></i>
              <span>{{ col.name }}</span>
              <span class="col-count">{{ col.items.length }}</span>
            </div>
            <i class="fa-solid fa-plus add-btn" @click="openCreateTask(col.name)"></i>
          </div>
          
          <draggable 
            class="col-body" 
            :list="col.items" 
            group="tasks" 
            item-key="id"
            @change="(evt) => handleDraggableChange(evt, col)"
          >
            <template #item="{ element }">
              <div class="issue-card" :class="{ 'active-card': selectedTask?.id === element.id }" @click="openTaskDetail(element)">
                <p class="issue-title" :style="element.statusName === 'DONE' ? { textDecoration: 'line-through', color: '#A1A1AA' } : {}">{{ element.title }}</p>
                <div class="issue-meta">
                  <span class="id">{{ element.sequenceId || element.id.substring(0,8).toUpperCase() }}</span>
                  <span class="prio">
                    <i class="fa-solid fa-angles-up text-red" v-if="element.priority === 1"></i>
                    <i class="fa-solid fa-chevron-up text-orange" v-else-if="element.priority === 2"></i>
                    <i class="fa-solid fa-minus text-blue" v-else-if="element.priority === 3"></i>
                    <i class="fa-solid fa-chevron-down text-muted" v-else></i>
                  </span>
                  <div class="avatar-xs ms-auto" v-if="element.assigneeName">{{ element.assigneeName.substring(0,2).toUpperCase() }}</div>
                  <div class="avatar-xs ms-auto" style="border: 1px dashed #3f3f46; background: transparent; color: #3f3f46;" v-else><i class="fa-solid fa-user"></i></div>
                </div>
              </div>
            </template>
          </draggable>
        </div>

      </div>
    </div>

    <!-- Task Detail Modal -->
    <TaskDetailModal 
      v-if="selectedTask"
      :selectedTask="selectedTask"
      :projectId="getProjectId()"
      :projectMembers="projectMembers"
      @close="closeTaskDetail"
      @updateTask="updateTask"
      @refresh-tasks="fetchTasks"
    />

    <!-- Analytics Sidebar Overlay -->
    <div class="analytics-overlay" v-show="showAnalyticsSidebar" @click.self="showAnalyticsSidebar = false">
      <div class="analytics-panel" :class="{ 'slide-in': showAnalyticsSidebar }">
         <div class="ap-header">
            <h3>Analytics for {{ project?.name || 'Cun' }}</h3>
            <div class="ap-actions">
               <button class="icon-btn"><i class="fa-solid fa-expand"></i></button>
               <button class="icon-btn" @click="showAnalyticsSidebar = false"><i class="fa-solid fa-xmark"></i></button>
            </div>
         </div>
         
         <div class="ap-body">
            <!-- Stats -->
            <div class="ap-stats-grid">
               <div class="stat-box">
                  <span class="lbl">Total Work items</span>
                  <span class="val">{{ rawTasks.length }}</span>
               </div>
               <div class="stat-box">
                  <span class="lbl">Started Work items</span>
                  <span class="val">{{ rawTasks.filter(t => t.statusName === 'IN PROGRESS').length }}</span>
               </div>
               <div class="stat-box">
                  <span class="lbl">Backlog Work items</span>
                  <span class="val">{{ rawTasks.filter(t => !t.statusName || t.statusName === 'TO DO' || t.statusName === 'TODO').length }}</span>
               </div>
               <div class="stat-box">
                  <span class="lbl">Unstarted Work items</span>
                  <span class="val">{{ rawTasks.filter(t => t.statusName === 'IN REVIEW').length }}</span>
               </div>
               <div class="stat-box">
                  <span class="lbl">Completed Work items</span>
                  <span class="val">{{ rawTasks.filter(t => t.statusName === 'DONE').length }}</span>
               </div>
            </div>
            
            <!-- Created vs Resolved Chart Overlay -->
            <div class="ap-chart-card mt-4">
               <h4>Created vs Resolved</h4>
               <div class="line-chart-mock">
                  <!-- Grid -->
                  <div class="grid-l" style="bottom: 90%;"><span>9</span></div>
                  <div class="grid-l" style="bottom: 70%;"><span>7</span></div>
                  <div class="grid-l" style="bottom: 50%;"><span>5</span></div>
                  <div class="grid-l" style="bottom: 30%;"><span>3</span></div>
                  <div class="grid-l" style="bottom: 10%;"><span>0</span></div>
                  
                  <!-- Dots -->
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
                  <!-- Grid -->
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
            
            <!-- Tables -->
            <div class="ap-table-wrap mt-4">
               <div class="table-head">
                  <span class="text-muted">4 Priority</span>
                  <div class="flex-center gap-1">
                     <i class="fa-solid fa-magnifying-glass text-muted"></i>
                     <button class="export-btn"><i class="fa-solid fa-download"></i> Export as csv</button>
                  </div>
               </div>
               <table class="ap-table">
                  <thead><tr><th>Priority</th><th style="text-align: right;">Count</th></tr></thead>
                  <tbody>
                     <tr><td>High</td><td style="text-align: right;">3</td></tr>
                     <tr><td>Low</td><td style="text-align: right;">1</td></tr>
                     <tr><td>None</td><td style="text-align: right;">3</td></tr>
                     <tr><td>Urgent</td><td style="text-align: right;">1</td></tr>
                  </tbody>
               </table>
            </div>

            <div class="ap-table-wrap mt-4">
               <div class="table-head">
                  <span class="text-muted">1 Assignee</span>
                  <div class="flex-center gap-1">
                     <i class="fa-solid fa-magnifying-glass text-muted"></i>
                     <button class="export-btn"><i class="fa-solid fa-download"></i> Export as csv</button>
                  </div>
               </div>
               <table class="ap-table">
                  <thead>
                     <tr>
                        <th>Assignee</th>
                        <th style="text-align: right;">Backlog</th>
                        <th style="text-align: right;">Started</th>
                        <th style="text-align: right;">Unstarted</th>
                        <th style="text-align: right;">Completed</th>
                        <th style="text-align: right;">Cancelled</th>
                     </tr>
                  </thead>
                  <tbody>
                     <tr>
                        <td><i class="fa-regular fa-user"></i> Unassigned</td>
                        <td style="text-align: right;">4</td>
                        <td style="text-align: right;">2</td>
                        <td style="text-align: right;">1</td>
                        <td style="text-align: right;">1</td>
                        <td style="text-align: right;">0</td>
                     </tr>
                  </tbody>
               </table>
            </div>
         </div>
      </div>
    </div>
  </NexusLayout>
</template>

<script setup>
// AI 3: CHUYÊN VIÊN GHÉP NỐI LOGIC FRONT-TO-BACK
import { ref, onMounted, computed, defineAsyncComponent } from 'vue'
import { useRoute } from 'vue-router'
import axiosClient from '@/api/axiosClient'
import NexusLayout from '@/components/layout/NexusLayout.vue'
import draggable from 'vuedraggable'
import TaskDetailModal from '@/components/TaskDetailModal.vue'
import ListView from '@/components/ListView.vue'
import CalendarTab from '@/components/CalendarTab.vue'
import TimelineTab from '@/components/TimelineTab.vue'
import SpreadsheetTab from '@/components/SpreadsheetTab.vue'

const showDisplayDropdown = ref(false)
const showAnalyticsSidebar = ref(false)

const route = useRoute()
const projectId = route.params.id

const project = ref({})
const rawTasks = ref([])
const projectMembers = ref([])
const selectedTask = ref(null)

const currentTab = ref('board')
const searchQuery = ref('')
const activeFilters = ref({ assignee: null })
const groupBy = ref('status')

const getProjectId = () => projectId || localStorage.getItem('lastProjectId')

const filteredTasksList = computed(() => {
  let filteredTasks = [...rawTasks.value];

  if (searchQuery.value) {
     filteredTasks = filteredTasks.filter(t => t.title.toLowerCase().includes(searchQuery.value.toLowerCase()) || (t.sequenceId && t.sequenceId.toLowerCase().includes(searchQuery.value.toLowerCase())));
  }
  if (activeFilters.value.assignee) {
     filteredTasks = filteredTasks.filter(t => t.assignedUserId === activeFilters.value.assignee.userId);
  }

  return filteredTasks.sort((a,b) => a.sortOrder - b.sortOrder);
});

const kanbanColumns = computed(() => {
  const groups = [
    { id: 'todo', name: 'TO DO', color: '#D4D4D8', icon: 'fa-regular fa-circle', priorityValue: null, items: [] },
    { id: 'inprogress', name: 'IN PROGRESS', color: '#3B82F6', icon: 'fa-solid fa-circle-half-stroke', priorityValue: null, items: [] },
    { id: 'review', name: 'IN REVIEW', color: '#F59E0B', icon: 'fa-solid fa-eye', priorityValue: null, items: [] },
    { id: 'done', name: 'DONE', color: '#10B981', icon: 'fa-solid fa-circle-check', priorityValue: null, items: [] }
  ];

  const pGroups = [
    { id: 'p1', name: 'Urgent', color: '#EF4444', icon: 'fa-solid fa-angles-up', priorityValue: 1, items: [] },
    { id: 'p2', name: 'High', color: '#F97316', icon: 'fa-solid fa-chevron-up', priorityValue: 2, items: [] },
    { id: 'p3', name: 'Normal', color: '#3B82F6', icon: 'fa-solid fa-minus', priorityValue: 3, items: [] },
    { id: 'p4', name: 'Low', color: '#94A3B8', icon: 'fa-solid fa-chevron-down', priorityValue: 4, items: [] }
  ];

  if (groupBy.value === 'priority') {
     filteredTasksList.value.forEach(t => {
       let col = pGroups.find(g => g.priorityValue === (t.priority || 3));
       if (!col) col = pGroups[2];
       col.items.push(t);
     });
     return pGroups;
  } else {
     filteredTasksList.value.forEach(t => {
       let col = groups.find(g => g.name === (t.statusName || 'TO DO'));
       if (!col) col = groups[0];
       col.items.push(t);
     });
     return groups;
  }
});

const loadInitialData = async () => {
  const pid = getProjectId()
  if(!pid) return

  try {
    const pRes = await axiosClient.get(`/projects/${pid}`)
    project.value = pRes.data.data

    const mRes = await axiosClient.get(`/projects/${pid}/members`)
    projectMembers.value = mRes.data.data || []

    await fetchTasks()
  } catch (error) {
    console.error('Lỗi load dự án:', error)
  }
}

const fetchTasks = async () => {
  const pid = getProjectId()
  if(!pid) return
  try {
    const res = await axiosClient.get(`/projects/${pid}/WorkTasks`)
    rawTasks.value = res.data.data || []
    
    // Auto update selectedTask if open
    if (selectedTask.value) {
      const updatedTask = rawTasks.value.find(t => t.id === selectedTask.value.id);
      if (updatedTask) selectedTask.value = updatedTask;
    }
  } catch(error) {
    console.error('Lỗi load tasks:', error)
  }
}

const openTaskDetail = (task) => {
  selectedTask.value = task;
}
const closeTaskDetail = () => {
  selectedTask.value = null;
}

const updateTask = async (task, field, value) => {
   try {
      const payload = {};
      payload[field] = value;
      await axiosClient.put(`/projects/${getProjectId()}/WorkTasks/${task.id}`, payload);
      await fetchTasks();
   } catch (error) {
      console.error('Failed to update task:', error);
   }
}

const openCreateTask = async (statusName) => {
   const title = prompt(`Enter new issue name (${statusName}):`)
   if(!title) return;
   try {
       await axiosClient.post(`/projects/${getProjectId()}/WorkTasks`, {
           title: title,
           statusName: statusName,
           priority: 3
       });
       await fetchTasks();
   } catch(e) {
       console.error(e)
   }
}

const handleDraggableChange = async (evt, group) => {
  if (evt.added || evt.moved) {
    const element = evt.added ? evt.added.element : evt.moved.element;
    const newIndex = evt.added ? evt.added.newIndex : evt.moved.newIndex;
    
    // Math cho LexoRank
    let newSortOrder = 65536;
    if (group.items.length === 1) {
       newSortOrder = 65536;
    } else if (newIndex === 0) {
       newSortOrder = group.items[1].sortOrder / 2.0;
    } else if (newIndex === group.items.length - 1) {
       newSortOrder = group.items[group.items.length - 2].sortOrder + 65536;
    } else {
       newSortOrder = (group.items[newIndex - 1].sortOrder + group.items[newIndex + 1].sortOrder) / 2.0;
    }
    
    element.sortOrder = newSortOrder;
    
    if (groupBy.value === 'status') {
       element.statusName = group.name; // Cập nhật Optimistic UI
       try {
         await axiosClient.put(`/projects/${getProjectId()}/WorkTasks/${element.id}/reorder`, {
           sortOrder: newSortOrder,
           newStatusName: group.name
         });
       } catch (error) {
         console.error('Lỗi API reorder:', error);
         fetchTasks(); // Load lại data nếu gặp lỗi
       }
    } else if (groupBy.value === 'priority') {
       element.priority = group.priorityValue;
       try {
         await axiosClient.put(`/projects/${getProjectId()}/WorkTasks/${element.id}/reorder`, {
           sortOrder: newSortOrder
         });
       } catch (error) {
         console.error('Lỗi API reorder:', error);
         fetchTasks();
       }
    }
  }
}

onMounted(() => {
  loadInitialData()
})
</script>

<style scoped>
/* ==================================
   PLANE.SO PROJECT KANBAN THEME
   ================================== */
.plane-board-container {
  background-color: #0D0F11; 
  height: 100vh;
  display: flex;
  flex-direction: column;
  color: #E5E7EB;
  font-family: 'Inter', sans-serif;
  overflow: hidden;
}

/* ── PLANE HEADER ── */
.plane-space-header {
  height: 52px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0 24px;
  border-bottom: 1px solid #1E2025;
  flex-shrink: 0;
  background-color: #0D0F11;
}

.breadcrumb {
  display: flex;
  align-items: center;
  gap: 10px;
  font-size: 14px;
  color: #A1A1AA;
}
.proj-icon {
  background: #F59E0B; /* Orange tone for example */
  color: white;
  width: 18px;
  height: 18px;
  border-radius: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 10px;
  font-weight: bold;
}
.proj-name {
  color: #E4E4E7;
  font-weight: 500;
  cursor: pointer;
}
.proj-name:hover { color: #FFFFFF; }
.separator {
  font-size: 10px;
  color: #71717A;
}
.active-page {
  color: #E4E4E7;
  display: flex;
  align-items: center;
  gap: 6px;
  font-weight: 500;
}
.active-page i { color: #A1A1AA; }
.item-count {
  background: #1E2025;
  color: #38BDF8;
  padding: 2px 8px;
  border-radius: 12px;
  font-size: 11px;
  font-weight: 600;
}

.sh-right {
  display: flex;
  align-items: center;
  gap: 12px;
}

.view-toggles {
  display: flex;
  background: #16181D;
  border: 1px solid #27272A;
  border-radius: 6px;
  padding: 2px;
  margin-right: 8px;
}
.toggle-btn {
  background: transparent;
  border: none;
  color: #71717A;
  width: 28px;
  height: 28px;
  border-radius: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all 0.2s;
}
.toggle-btn:hover { color: #E4E4E7; }
.toggle-btn.active {
  background: #27272A;
  color: #E4E4E7;
}

.plane-toolbar-btn {
  background: transparent;
  border: none;
  color: #D4D4D8;
  font-size: 13px;
  font-weight: 500;
  cursor: pointer;
  padding: 6px 12px;
  border-radius: 6px;
  transition: background 0.2s;
  display: flex;
  align-items: center;
}
.plane-toolbar-btn:hover {
  background: #1E2025;
}

.plane-primary-btn {
  background: #0EA5E9;
  color: white;
  border: none;
  border-radius: 6px;
  padding: 6px 12px;
  font-size: 13px;
  font-weight: 500;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 6px;
  transition: background 0.2s;
}
.plane-primary-btn:hover { background: #0284C7; }

/* Kanban Board */
.kanban-wrapper {
  display: flex;
  gap: 20px;
  flex: 1;
  overflow-x: auto;
  padding: 24px;
}

.kanban-col {
  min-width: 320px;
  width: 320px;
  display: flex;
  flex-direction: column;
}

.col-head {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
  padding: 0 4px;
}

.col-title {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 14px;
  font-weight: 600;
  color: #E5E7EB;
}

.col-count {
  background: #1F1F22;
  color: #A1A1AA;
  padding: 2px 8px;
  border-radius: 12px;
  font-size: 12px;
}

.add-btn {
  color: #71717A;
  cursor: pointer;
  font-size: 14px;
}
.add-btn:hover { color: #E5E7EB; }

.col-body {
  display: flex;
  flex-direction: column;
  gap: 12px;
  flex: 1;
  overflow-y: auto;
  padding-right: 4px; /* for scrollbar */
}

.issue-card {
  background: #111111;
  border: 1px solid #27272A;
  border-radius: 8px;
  padding: 16px;
  cursor: pointer;
  transition: all 0.2s;
}
.issue-card:hover {
  border-color: #3F3F46;
}
.issue-card.active-card {
  border-color: #3F3F46;
  box-shadow: 0 4px 12px rgba(0,0,0,0.5);
}

.issue-title {
  margin: 0 0 16px 0;
  font-size: 14px;
  font-weight: 500;
  color: #E5E7EB;
  line-height: 1.5;
}

.issue-meta {
  display: flex;
  align-items: center;
  gap: 12px;
}

.id { font-size: 12px; color: #71717A; font-weight: 600; }
.ms-auto { margin-left: auto; }

.avatar-xs {
  width: 20px;
  height: 20px;
  border-radius: 50%;
  background-color: #1F1F22;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 10px;
  font-weight: 600;
  color: #D4D4D8;
  border: 1px solid #3F3F46;
}

/* Colors for priority icons */
.text-muted { color: #71717A; }
.text-blue { color: #3B82F6; }
.text-orange { color: #F59E0B; }
.text-red { color: #EF4444; }

/* Scrollbar */
.kanban-wrapper::-webkit-scrollbar, .col-body::-webkit-scrollbar { width: 6px; height: 6px; }
.kanban-wrapper::-webkit-scrollbar-track, .col-body::-webkit-scrollbar-track { background: transparent; }
.kanban-wrapper::-webkit-scrollbar-thumb, .col-body::-webkit-scrollbar-thumb { background: #27272A; border-radius: 4px; }
.kanban-wrapper::-webkit-scrollbar-thumb:hover, .col-body::-webkit-scrollbar-thumb:hover { background: #3F3F46; }

/* Display Dropdown Styles */
.display-dropdown-wrapper { position: relative; display: inline-block; }
.plane-dropdown-menu {
  position: absolute;
  top: 100%;
  left: 0;
  margin-top: 8px;
  background: #1E2025;
  border: 1px solid #333;
  border-radius: 8px;
  width: 260px;
  box-shadow: 0 10px 30px rgba(0,0,0,0.5);
  z-index: 1000;
  color: #E4E4E7;
  font-size: 13px;
  padding: 8px 0;
}
.dd-section { padding: 8px 16px; }
.dd-section.border-top { border-top: 1px solid #27272A; }
.dd-title { display: flex; justify-content: space-between; color: #A1A1AA; font-size: 12px; font-weight: 500; margin-bottom: 8px; }
.dd-btns { display: flex; gap: 8px; flex-wrap: wrap; }
.dd-tag { background: #16181D; border: 1px solid #27272A; color: #E4E4E7; border-radius: 4px; padding: 4px 8px; font-size: 12px; cursor: pointer; }
.dd-tag.active { background: #0EA5E9; color: white; border-color: #0EA5E9; }
.dd-list { display: flex; flex-direction: column; gap: 8px; }
.dd-item { display: flex; align-items: center; gap: 8px; cursor: pointer; }
.dd-item input[type="radio"], .dd-item input[type="checkbox"] { accent-color: #0EA5E9; cursor: pointer; }

/* Analytics Sidebar */
.analytics-overlay {
  position: fixed;
  top: 0; left: 0; right: 0; bottom: 0;
  background: rgba(0,0,0,0.5);
  z-index: 9999;
  display: flex;
  justify-content: flex-end;
}
.analytics-panel {
  width: 900px;
  max-width: 90vw;
  background: #111315;
  height: 100%;
  box-shadow: -5px 0 20px rgba(0,0,0,0.5);
  display: flex;
  flex-direction: column;
  transform: translateX(100%);
  transition: transform 0.3s cubic-bezier(0.16, 1, 0.3, 1);
  border-left: 1px solid #1E2025;
}
.analytics-panel.slide-in { transform: translateX(0); }
.ap-header {
  padding: 20px 24px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-bottom: 1px solid #1E2025;
}
.ap-header h3 { margin: 0; font-size: 16px; font-weight: 500; color: #E4E4E7; }
.ap-actions { display: flex; gap: 12px; }
.icon-btn { background: transparent; border: none; color: #A1A1AA; font-size: 14px; cursor: pointer; }
.icon-btn:hover { color: #E4E4E7; }

.ap-body {
  padding: 24px;
  overflow-y: auto;
  flex: 1;
}

/* Stats Grid */
.ap-stats-grid {
  display: flex;
  gap: 24px;
  flex-wrap: wrap;
}
.stat-box { display: flex; flex-direction: column; gap: 8px; flex: 1; min-width: 150px; }
.stat-box .lbl { color: #71717A; font-size: 12px; font-weight: 500; }
.stat-box .val { color: #E4E4E7; font-size: 20px; font-weight: 600; }

.ap-chart-card { margin-top: 32px; }
.ap-chart-card h4 { margin: 0; font-size: 14px; font-weight: 600; color: #E4E4E7; }

.line-chart-mock {
  position: relative;
  height: 200px;
  margin-top: 16px;
  border-bottom: 1px solid #27272A;
}
.grid-l {
  position: absolute;
  width: 100%;
  border-top: 1px solid #1E2025;
}
.grid-l span {
  position: absolute;
  left: -20px;
  top: -8px;
  font-size: 10px;
  color: #71717A;
}
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

.bar-chart-mock {
  position: relative;
  height: 250px;
  margin-top: 24px;
  border-bottom: 1px solid #27272A;
}
.bars-container {
  display: flex;
  justify-content: space-around;
  align-items: flex-end;
  height: 100%;
  padding-bottom: 1px; /* Avoid overlapping border */
}
.bar-wrapper { display: flex; flex-direction: column; align-items: center; gap: 8px; height: 100%; justify-content: flex-end; width: 40px; }
.bar { width: 100%; border-radius: 4px 4px 0 0; }
.bar-lbl { position: absolute; bottom: -24px; font-size: 12px; color: #A1A1AA; }
.bg-orange { background: #F97316; }
.bg-gray { background: #D4D4D8; }
.bg-red { background: #EF4444; }

.y-label {
  position: absolute;
  left: -40px;
  top: 50%;
  transform: rotate(-90deg) translateY(-50%);
  font-size: 10px;
  color: #71717A;
  letter-spacing: 1px;
}

.ap-table-wrap { margin-top: 40px; }
.table-head { display: flex; justify-content: space-between; align-items: center; margin-bottom: 12px; font-size: 13px; }
.flex-center { display: flex; align-items: center; }
.export-btn { background: transparent; border: 1px solid #27272A; color: #E4E4E7; border-radius: 4px; padding: 4px 8px; font-size: 12px; cursor: pointer; }
.export-btn:hover { background: #1E2025; }

.ap-table { width: 100%; border-collapse: collapse; font-size: 13px; color: #E4E4E7; }
.ap-table th { color: #A1A1AA; font-weight: 500; border-bottom: 1px solid #27272A; padding: 12px 16px; text-align: left; }
.ap-table td { padding: 16px; border-bottom: 1px solid #1E2025; }
.ap-table tr:hover { background: #16181D; }
</style>
