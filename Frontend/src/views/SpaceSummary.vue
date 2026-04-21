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
            <span class="item-count">{{ topLevelTasks.length }}</span>
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

          <button class="plane-toolbar-btn" @click="showFilterPanel = !showFilterPanel" :class="{ active: showFilterPanel || activeTaskFilters.length }">
            <i class="fa-solid fa-filter"></i>
            <span v-if="activeTaskFilters.length" class="filter-count">{{ activeTaskFilters.length }}</span>
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
                     <input type="checkbox" v-model="showSubtasks" /> Show sub-work items
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

      <div class="work-filter-row" v-if="showFilterPanel || activeTaskFilters.length">
        <FilterBar
          v-model:filters="activeTaskFilters"
          @apply="applyTaskFilters"
          @remove="removeTaskFilter"
          @clear="clearTaskFilters"
        />
      </div>

      <!-- Other Tab Views -->
      <div v-if="currentTab === 'list'" class="list-wrapper" style="padding: 16px;">
         <div class="plane-list-view">
           <div v-for="group in listViewGroups" :key="group.id" class="list-group">
             <div class="group-header" @click="toggleListGroup(group.id)">
               <div class="gh-left">
                 <i class="gh-chevron fa-solid" :class="collapsedListGroups[group.id] ? 'fa-chevron-right' : 'fa-chevron-down'"></i>
                 <i class="status-icon" :class="group.icon" :style="{ color: group.color }"></i>
                 <span class="group-name">{{ group.name }}</span>
                 <span class="group-count">{{ group.items.length }}</span>
               </div>
               <div class="gh-right">
                 <i class="fa-solid fa-plus add-icon" @click.stop="openCreateTask(group.statusName)"></i>
               </div>
             </div>

             <div class="group-content" v-show="!collapsedListGroups[group.id]">
               <div class="task-row" v-for="task in group.items" :key="task.id" @click="openTaskDetail(task)">
                 <div class="tr-left">
                   <span class="task-id">{{ task.sequenceId || task.id.substring(0,8).toUpperCase() }}</span>
                   <span class="task-title" :style="group.statusName === 'DONE' ? { textDecoration: 'line-through', color: '#71717A' } : {}">
                     {{ task.title }}
                   </span>
                 </div>
                 <div class="tr-right" @click.stop>
                   <div class="pill-group">
                     <el-dropdown trigger="click" @command="(val) => updateTask(task, 'statusName', val, task.statusName)">
                       <div class="pill pill-status cursor-pointer hover:bg-[#1E2025]">
                         <i :class="getBoardStatusIcon(task.statusName)" :style="{ color: getStatusColor(task.statusName) }"></i>
                         {{ normalizeStatusLabel(task.statusName) }}
                       </div>
                       <template #dropdown>
                         <el-dropdown-menu class="plane-dropdown">
                           <el-dropdown-item v-for="status in taskStatusOptions" :key="status.name" :command="status.name">
                             <i :class="status.icon" :style="{ color: status.color }"></i>
                             {{ status.label }}
                           </el-dropdown-item>
                         </el-dropdown-menu>
                       </template>
                     </el-dropdown>

                     <el-dropdown trigger="click" @command="(val) => updateTask(task, 'priority', val, task.priority)">
                       <div class="pill pill-priority cursor-pointer hover:bg-[#1E2025]">
                         <i :class="getPriorityIcon(task.priority)"></i>
                       </div>
                       <template #dropdown>
                         <el-dropdown-menu class="plane-dropdown">
                           <el-dropdown-item :command="1"><i class="fa-solid fa-angles-up text-red-500"></i> Urgent</el-dropdown-item>
                           <el-dropdown-item :command="2"><i class="fa-solid fa-chevron-up text-orange-500"></i> High</el-dropdown-item>
                           <el-dropdown-item :command="3"><i class="fa-solid fa-minus text-blue-500"></i> Normal</el-dropdown-item>
                           <el-dropdown-item :command="4"><i class="fa-solid fa-chevron-down text-gray-400"></i> Low</el-dropdown-item>
                           <el-dropdown-item :command="0"><i class="fa-solid fa-ban text-gray-500"></i> None</el-dropdown-item>
                         </el-dropdown-menu>
                       </template>
                     </el-dropdown>

                     <el-popover placement="bottom" trigger="click" width="260" popper-class="plane-popover">
                       <template #reference>
                         <div class="pill pill-user cursor-pointer hover:bg-[#1E2025]">
                           <div class="avatar-xxs">
                             <i class="fa-regular fa-user" v-if="!getTaskAssigneeSummary(task).label"></i>
                             <span v-else>{{ getTaskAssigneeSummary(task).avatar }}</span>
                           </div>
                           <span v-if="getTaskAssigneeSummary(task).label" class="pill-user-text">{{ getTaskAssigneeSummary(task).label }}</span>
                         </div>
                       </template>
                       <div class="popover-content">
                         <input type="text" class="plane-search-input" v-model="assigneeSearch" placeholder="Search members" />
                         <div class="plane-list mt-2">
                           <label
                             class="plane-list-item"
                             v-for="member in filteredProjectMembers"
                             :key="member.userId || member.id"
                             @click.stop="toggleTaskAssignee(task, member.userId || member.id)"
                           >
                             <input type="checkbox" :checked="getTaskAssigneeIds(task).includes(member.userId || member.id)" />
                             {{ member.fullName || member.name || member.email }}
                           </label>
                         </div>
                       </div>
                     </el-popover>
                   </div>
                 </div>
               </div>

               <div class="add-row-placeholder" @click="openCreateTask(group.statusName)">
                 <i class="fa-solid fa-plus"></i> New work item
               </div>
             </div>
           </div>
         </div>
      </div>
      <div v-if="currentTab === 'calendar'" class="calendar-wrapper">
         <CalendarTab :tasks="filteredTasksList" @open-task="openTaskDetail" @create-task="openCreateTaskFromCalendar" />
      </div>
      <div v-if="currentTab === 'spreadsheet'" class="spreadsheet-wrapper" style="display: flex; flex: 1; overflow: hidden;">
          <SpreadsheetTab
            :tasks="filteredTasksList"
            :projectId="getProjectId()"
            :projectMembers="projectMembers"
            @task-click="openTaskDetail"
            @update-task="updateTask"
            @create-task="payload => openCreateTask(payload?.statusName || 'TO DO')"
          />
      </div>
      <div v-if="currentTab === 'timeline'" class="timeline-wrapper">
         <TimelineTab :projectId="projectId" :tasks="filteredTasksList" @open-task="openTaskDetail" />
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
          
          <div class="col-body">
            <draggable 
              class="col-draggable" 
              :list="col.items" 
              group="tasks" 
              item-key="id"
              @change="(evt) => handleDraggableChange(evt, col)"
            >
              <template #item="{ element }">
                <div class="issue-card" :class="{ 'active-card': selectedTask?.id === element.id }" @click="openTaskDetail(element)">
                  <p class="issue-sequence mb-1">{{ element.sequenceId || element.id.substring(0,8).toUpperCase() }}</p>
                  <p class="issue-title" :style="element.statusName === 'DONE' ? { textDecoration: 'line-through', color: '#A1A1AA' } : {}">{{ element.title }}</p>
                  <div class="issue-meta mt-2" style="display:flex; align-items:center; gap:8px;" @click.stop>
                     <el-dropdown trigger="click" @command="(val) => updateTask(element, 'statusName', val, element.statusName)">
                       <div class="badge cursor-pointer hover:bg-[#1E2025]">
                         <i :class="getBoardStatusIcon(element.statusName)" :style="{ color: getStatusColor(element.statusName) }"></i>
                         <span>{{ normalizeStatusLabel(element.statusName) }}</span>
                       </div>
                       <template #dropdown>
                         <el-dropdown-menu class="plane-dropdown">
                           <el-dropdown-item v-for="status in taskStatusOptions" :key="status.name" :command="status.name">
                             <i :class="status.icon" :style="{ color: status.color }"></i>
                             {{ status.label }}
                           </el-dropdown-item>
                         </el-dropdown-menu>
                       </template>
                     </el-dropdown>

                     <el-dropdown trigger="click" @command="(val) => updateTask(element, 'priority', val, element.priority)">
                       <div class="badge cursor-pointer hover:bg-[#1E2025]">
                         <i :class="getPriorityIcon(element.priority)"></i>
                       </div>
                       <template #dropdown>
                         <el-dropdown-menu class="plane-dropdown">
                           <el-dropdown-item :command="1"><i class="fa-solid fa-angles-up text-red-500"></i> Urgent</el-dropdown-item>
                           <el-dropdown-item :command="2"><i class="fa-solid fa-chevron-up text-orange-500"></i> High</el-dropdown-item>
                           <el-dropdown-item :command="3"><i class="fa-solid fa-minus text-blue-500"></i> Medium</el-dropdown-item>
                           <el-dropdown-item :command="4"><i class="fa-solid fa-chevron-down text-gray-400"></i> Low</el-dropdown-item>
                           <el-dropdown-item :command="0"><i class="fa-solid fa-ban text-gray-500"></i> None</el-dropdown-item>
                         </el-dropdown-menu>
                       </template>
                     </el-dropdown>

                     <el-popover placement="bottom" trigger="click" width="260" popper-class="plane-popover">
                       <template #reference>
                         <div class="avatar-xs ms-auto cursor-pointer hover:bg-[#1E2025]" v-if="getTaskAssigneeSummary(element).label">
                           {{ getTaskAssigneeSummary(element).avatar }}
                         </div>
                         <div class="avatar-xs ms-auto cursor-pointer hover:bg-[#1E2025]" style="border: 1px dashed #3f3f46; background: transparent; color: #3f3f46;" v-else><i class="fa-solid fa-user"></i></div>
                       </template>
                       <div class="popover-content">
                         <input type="text" class="plane-search-input" v-model="assigneeSearch" placeholder="Search members" />
                         <div class="plane-list mt-2">
                           <label
                             class="plane-list-item"
                             v-for="member in filteredProjectMembers"
                             :key="member.userId || member.id"
                             @click.stop="toggleTaskAssignee(element, member.userId || member.id)"
                           >
                             <input type="checkbox" :checked="getTaskAssigneeIds(element).includes(member.userId || member.id)" />
                             {{ member.fullName || member.name || member.email }}
                           </label>
                         </div>
                       </div>
                     </el-popover>
                  </div>
                </div>
              </template>
            </draggable>
            
            <div class="inline-create-box" v-if="inlineCreateColId === col.id">
               <div class="ic-top">
                 <i class="fa-solid fa-plus ic-plus"></i>
                 <input type="text" class="ic-input" v-model="inlineTaskTitle" placeholder="New work item" @keyup.enter="submitInlineTask(col)" @keyup.esc="inlineCreateColId = null" ref="inlineInput" />
               </div>
               <div class="ic-bottom">
                 <div class="ic-chip"><i class="fa-regular fa-circle"></i> {{ col.name }}</div>
                 <div class="ic-chip"><i class="fa-solid fa-minus text-blue"></i></div>
                 <div class="avatar-xs ms-auto ic-avatar"><i class="fa-solid fa-user"></i></div>
               </div>
            </div>
            <div class="add-btn-bottom" v-else @click="openInlineCreate(col.id)">
               <i class="fa-solid fa-plus"></i> New work item
            </div>
          </div>
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
               <button class="icon-btn" @click="showAnalyticsSidebar = false"><i class="fa-solid fa-expand"></i></button>
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
               <v-chart class="chart-container" :option="createdResolvedOptions" autoresize />
            </div>

            <!-- Customized Insights -->
            <div class="ap-chart-card mt-4">
               <div class="flex-between">
                  <h4>Customized Insights</h4>
                  <div class="insight-filters">
                     <button class="filter-btn" @click="showFilterPanel = true"><i class="fa-solid fa-briefcase"></i> Work item <i class="fa-solid fa-chevron-down"></i></button>
                     <button class="filter-btn" @click="groupBy = groupBy === 'status' ? 'priority' : 'status'"><i class="fa-solid fa-list"></i> {{ groupBy === 'status' ? 'Priority' : 'Status' }} <i class="fa-solid fa-chevron-down"></i></button>
                     <button class="filter-btn" @click="showFilterPanel = true"><i class="fa-solid fa-plus-minus"></i> Add Property <i class="fa-solid fa-chevron-down"></i></button>
                  </div>
               </div>
               
               <v-chart class="chart-container mt-4" :option="priorityChartOptions" autoresize />
            </div>
            
            <!-- Tables -->
            <div class="ap-table-wrap mt-4">
               <div class="table-head">
                  <span class="text-muted">4 Priority</span>
                  <div class="flex-center gap-1">
                     <i class="fa-solid fa-magnifying-glass text-muted"></i>
                     <button class="export-btn" @click="exportAnalyticsCsv('priority')"><i class="fa-solid fa-download"></i> Export as csv</button>
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
                     <button class="export-btn" @click="exportAnalyticsCsv('assignee')"><i class="fa-solid fa-download"></i> Export as csv</button>
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
import { ref, onMounted, computed, defineAsyncComponent, watch, nextTick, onUnmounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import axiosClient from '@/api/axiosClient'
import NexusLayout from '@/components/layout/NexusLayout.vue'
import draggable from 'vuedraggable'
import TaskDetailModal from '@/components/TaskDetailModal.vue'
import CalendarTab from '@/components/CalendarTab.vue'
import TimelineTab from '@/components/TimelineTab.vue'
import SpreadsheetTab from '@/components/SpreadsheetTab.vue'
import FilterBar from '@/components/FilterBar.vue'
import { useWorkTaskStore } from '@/store/useWorkTaskStore';

import { use } from 'echarts/core';
import { CanvasRenderer } from 'echarts/renderers';
import { LineChart, BarChart } from 'echarts/charts';
import { TitleComponent, TooltipComponent, LegendComponent, GridComponent } from 'echarts/components';
import VChart from 'vue-echarts';

use([
  CanvasRenderer,
  LineChart,
  BarChart,
  TitleComponent,
  TooltipComponent,
  LegendComponent,
  GridComponent
]);

const showDisplayDropdown = ref(false)
const showAnalyticsSidebar = ref(false)
const showFilterPanel = ref(false)
const showSubtasks = ref(false)
const collapsedListGroups = ref({})
const assigneeSearch = ref('')

const route = useRoute()
const router = useRouter()
const projectId = route.params.id
const store = useWorkTaskStore();

const project = ref({})
const rawTasks = ref([])
const projectMembers = ref([])
const selectedTask = ref(null)
const inlineCreateColId = ref(null)
const inlineTaskTitle = ref('')

const currentTab = ref('board')
const searchQuery = ref('')
const activeFilters = ref({ assignee: null })
const activeTaskFilters = ref([])
const groupBy = ref('status')
const activeSprintFilterId = computed(() => route.query.sprintId || route.params.cycleId || null)
const activeModuleFilterId = computed(() => route.query.moduleId || null)

const isSubtask = (task) => Boolean(task?.parentTaskId || task?.parentId)

const getTaskAssigneeIds = (task) => {
  if (Array.isArray(task.assigneeIds) && task.assigneeIds.length) return task.assigneeIds
  if (Array.isArray(task.assignees) && task.assignees.length) return task.assignees.map(item => item.userId || item.id).filter(Boolean)
  if (task.assignedUserId) return [task.assignedUserId]
  return []
}

const topLevelTasks = computed(() => rawTasks.value.filter(task => !isSubtask(task)))
const visibleTasks = computed(() => showSubtasks.value ? rawTasks.value : topLevelTasks.value)
const taskStatusOptions = [
  { name: 'BACKLOG', label: 'Backlog', color: '#71717A', icon: 'fa-regular fa-circle-dashed' },
  { name: 'TO DO', label: 'To Do', color: '#D4D4D8', icon: 'fa-regular fa-circle' },
  { name: 'IN PROGRESS', label: 'In Progress', color: '#3B82F6', icon: 'fa-solid fa-circle-half-stroke' },
  { name: 'IN REVIEW', label: 'In Review', color: '#F59E0B', icon: 'fa-solid fa-eye' },
  { name: 'DONE', label: 'Done', color: '#10B981', icon: 'fa-solid fa-circle-check' },
  { name: 'CANCELLED', label: 'Cancelled', color: '#EF4444', icon: 'fa-regular fa-circle-xmark' }
]

const normalizeText = (value) => `${value || ''}`.toLowerCase().trim()
const normalizeStatus = (value) => `${value || 'BACKLOG'}`.toUpperCase().replace(/\s+/g, ' ').trim()
const normalizeStatusLabel = (value) => {
  const status = normalizeStatus(value)
  return taskStatusOptions.find(item => item.name === status)?.label || status
}
const getBoardStatusIcon = (value) => taskStatusOptions.find(item => item.name === normalizeStatus(value))?.icon || 'fa-regular fa-circle-dashed'
const getStatusColor = (value) => taskStatusOptions.find(item => item.name === normalizeStatus(value))?.color || '#71717A'
const getPriorityIcon = (priority) => {
  if (priority === 1) return 'fa-solid fa-angles-up text-red-500'
  if (priority === 2) return 'fa-solid fa-chevron-up text-orange-500'
  if (priority === 3) return 'fa-solid fa-minus text-blue-500'
  if (priority === 4) return 'fa-solid fa-chevron-down text-gray-400'
  return 'fa-solid fa-ban text-gray-500'
}
const normalizePriority = (value) => {
  const map = { urgent: 1, high: 2, normal: 3, low: 4, none: null }
  return Object.prototype.hasOwnProperty.call(map, normalizeText(value)) ? map[normalizeText(value)] : value
}
const filterValues = (value) => Array.isArray(value) ? value : `${value || ''}`.split(',').map(item => item.trim()).filter(Boolean)
const valuesInclude = (values, target) => values.map(normalizeText).includes(normalizeText(target))
const currentUserId = () => {
  try {
    const rawUser = localStorage.getItem('user')
    const user = rawUser ? JSON.parse(rawUser) : null
    return user?.id || user?.userId || localStorage.getItem('userId') || null
  } catch {
    return localStorage.getItem('userId') || null
  }
}
const getTaskDate = (task, field) => {
  const value = task[field] || (field === 'startDate' ? task.plannedStartDate : null) || (field === 'dueDate' ? task.dueDate : null)
  if (!value) return null
  const date = new Date(value)
  return Number.isNaN(date.getTime()) ? null : date
}
const startOfToday = () => {
  const date = new Date()
  date.setHours(0, 0, 0, 0)
  return date
}
const isThisWeek = (date) => {
  if (!date) return false
  const today = startOfToday()
  const end = new Date(today)
  end.setDate(today.getDate() + 7)
  return date >= today && date <= end
}
const taskMatchesFilter = (task, filter) => {
  const operator = filter.operator || filter.condition || 'is'
  const value = filter.value || filter.displayValue
  const field = filter.field

  if (field === 'status') {
    const left = normalizeStatus(task.statusName)
    const rightValues = filterValues(value).map(normalizeStatus)
    if (operator === 'is not' || operator === 'not in') return !rightValues.includes(left)
    return rightValues.includes(left)
  }

  if (field === 'priority') {
    const left = task.priority || null
    const rightValues = filterValues(value).map(normalizePriority)
    if (operator === 'is not' || operator === 'not in') return !rightValues.includes(left)
    return rightValues.includes(left)
  }

  if (field === 'assignee') {
    const assigneeIds = getTaskAssigneeIds(task)
    if (operator === 'empty') return assigneeIds.length === 0
    if (operator === 'not empty') return assigneeIds.length > 0
    if (normalizeText(value) === 'unassigned') return operator === 'is not' ? assigneeIds.length > 0 : assigneeIds.length === 0
    const assigneeNames = (task.assignees || []).map(item => item.fullName || item.name || item.email)
    const hasMatch = filterValues(value).some(item => assigneeIds.includes(item) || valuesInclude(assigneeNames, item))
    return operator === 'is not' ? !hasMatch : hasMatch
  }

  if (field === 'creator') {
    const creatorIds = [task.reporterId, task.createdById, task.createdBy].filter(Boolean)
    const creatorNames = [task.reporterName, task.createdByName, task.creatorName, task.createdBy?.fullName].filter(Boolean)
    const values = filterValues(value)
    const me = currentUserId()
    const hasMatch = values.some(item => {
      if (normalizeText(item) === 'me') return Boolean(me && creatorIds.includes(me))
      return creatorIds.includes(item) || valuesInclude(creatorNames, item)
    })
    return operator === 'is not' ? !hasMatch : hasMatch
  }

  if (field === 'label') {
    const labelIds = task.labelIds || []
    const labelNames = (task.labels || task.labelNames || []).map(item => item.name || item)
    if (operator === 'empty' || normalizeText(value) === 'no label') return labelIds.length === 0 && labelNames.length === 0
    const hasMatch = filterValues(value).some(item => labelIds.includes(item) || valuesInclude(labelNames, item))
    return operator === 'not includes' || operator === 'not_includes' ? !hasMatch : hasMatch
  }

  if (['startDate', 'dueDate', 'createdAt', 'updatedAt'].includes(field)) {
    const dateField = field === 'startDate' ? 'plannedStartDate' : field
    const date = getTaskDate(task, dateField)
    if (operator === 'empty') return !date
    if (operator === 'overdue') return Boolean(date && date < startOfToday() && normalizeStatus(task.statusName) !== 'DONE')
    if (normalizeText(value) === 'empty') return !date
    if (normalizeText(value) === 'today') return Boolean(date && date.toDateString() === startOfToday().toDateString())
    if (normalizeText(value) === 'this week') return isThisWeek(date)
    return true
  }

  if (field === 'cycle') {
    if (operator === 'empty' || normalizeText(value) === 'no cycle') return !task.sprintId
    const hasMatch = filterValues(value).some(item => task.sprintId === item || normalizeText(task.sprintName) === normalizeText(item))
    return operator === 'is not' ? !hasMatch : hasMatch
  }

  if (field === 'module') {
    if (operator === 'empty' || normalizeText(value) === 'no module') return !task.moduleId && !(task.moduleIds || []).length
    const moduleIds = [task.moduleId, ...(task.moduleIds || []), ...(task.modules || []).map(item => item.id || item.moduleId)].filter(Boolean)
    const moduleNames = [task.moduleName, ...(task.modules || []).map(item => item.name)].filter(Boolean)
    const hasMatch = filterValues(value).some(item => moduleIds.includes(item) || valuesInclude(moduleNames, item))
    return operator === 'is not' ? !hasMatch : hasMatch
  }

  return true
}

let dynamicProjectId = null;
const getProjectId = () => {
    let p = dynamicProjectId || projectId || localStorage.getItem('lastProjectId');
    return p === 'default' ? null : p;
}

const filteredProjectMembers = computed(() => {
  const keyword = assigneeSearch.value.trim().toLowerCase()
  if (!keyword) return projectMembers.value
  return projectMembers.value.filter(member =>
    `${member.fullName || member.name || member.email || ''}`.toLowerCase().includes(keyword)
  )
})

const filteredTasksList = computed(() => {
  let filteredTasks = [...visibleTasks.value];

  if (searchQuery.value) {
     filteredTasks = filteredTasks.filter(t => t.title.toLowerCase().includes(searchQuery.value.toLowerCase()) || (t.sequenceId && t.sequenceId.toLowerCase().includes(searchQuery.value.toLowerCase())));
  }
  if (activeFilters.value.assignee) {
     filteredTasks = filteredTasks.filter(t => getTaskAssigneeIds(t).includes(activeFilters.value.assignee.userId));
  }
  if (activeSprintFilterId.value) {
     filteredTasks = filteredTasks.filter(t => t.sprintId === activeSprintFilterId.value);
  }
  if (activeModuleFilterId.value) {
     filteredTasks = filteredTasks.filter(t => {
       if (t.moduleId === activeModuleFilterId.value) return true
       if (Array.isArray(t.moduleIds) && t.moduleIds.includes(activeModuleFilterId.value)) return true
       if (Array.isArray(t.modules) && t.modules.some(m => (m.id || m.moduleId) === activeModuleFilterId.value)) return true
       return false
     });
  }
  if (activeTaskFilters.value.length) {
     filteredTasks = filteredTasks.filter(task => activeTaskFilters.value.every(filter => taskMatchesFilter(task, filter)));
  }

  return filteredTasks.sort((a,b) => (Number(a.sortOrder) || 0) - (Number(b.sortOrder) || 0));
});

const createdResolvedOptions = computed(() => {
   return {
      tooltip: { trigger: 'axis' },
      legend: { data: ['Created', 'Resolved'], bottom: 0, textStyle: { color: '#A1A1AA' } },
      grid: { left: '3%', right: '4%', bottom: '15%', containLabel: true },
      xAxis: { type: 'category', data: ['Apr 01', 'Apr 02', 'Apr 03', 'Apr 04'], axisLine: { lineStyle: { color: '#3F3F46' } } },
      yAxis: { type: 'value', splitLine: { lineStyle: { color: '#27272A' } } },
      series: [
         { name: 'Created', type: 'line', data: [rawTasks.value.length, 0, 0, 0], itemStyle: { color: '#3B82F6' }, smooth: true },
         { name: 'Resolved', type: 'line', data: [rawTasks.value.filter(t => t.statusName === 'DONE').length, 0, 0, 0], itemStyle: { color: '#10B981' }, smooth: true }
      ],
      backgroundColor: 'transparent'
   }
});

const priorityChartOptions = computed(() => {
   const urgent = rawTasks.value.filter(t => t.priority === 1).length;
   const high = rawTasks.value.filter(t => t.priority === 2).length;
   const normal = rawTasks.value.filter(t => t.priority === 3).length;
   const low = rawTasks.value.filter(t => t.priority === 4).length;
   const none = rawTasks.value.filter(t => !t.priority).length;
   
   return {
      tooltip: { trigger: 'axis' },
      grid: { left: '3%', right: '4%', bottom: '3%', containLabel: true },
      xAxis: { type: 'category', data: ['Urgent', 'High', 'Normal', 'Low', 'None'], axisLine: { lineStyle: { color: '#3F3F46' } }, axisLabel: { color: '#A1A1AA' } },
      yAxis: { type: 'value', splitLine: { lineStyle: { color: '#27272A' } }, axisLabel: { color: '#A1A1AA' } },
      series: [
         {
            type: 'bar',
            barWidth: '30%',
            data: [
               { value: urgent, itemStyle: { color: '#EF4444', borderRadius: [4, 4, 0, 0] } },
               { value: high, itemStyle: { color: '#F97316', borderRadius: [4, 4, 0, 0] } },
               { value: normal, itemStyle: { color: '#3B82F6', borderRadius: [4, 4, 0, 0] } },
               { value: low, itemStyle: { color: '#10B981', borderRadius: [4, 4, 0, 0] } },
               { value: none, itemStyle: { color: '#71717A', borderRadius: [4, 4, 0, 0] } }
            ]
         }
      ],
      backgroundColor: 'transparent'
   }
});

const kanbanColumns = computed(() => {
  const groups = [
    { id: 'backlog', name: 'BACKLOG', color: '#71717A', icon: 'fa-regular fa-circle-dashed', priorityValue: null, items: [] },
    { id: 'todo', name: 'TO DO', color: '#D4D4D8', icon: 'fa-regular fa-circle', priorityValue: null, items: [] },
    { id: 'inprogress', name: 'IN PROGRESS', color: '#3B82F6', icon: 'fa-solid fa-circle-half-stroke', priorityValue: null, items: [] },
    { id: 'review', name: 'IN REVIEW', color: '#F59E0B', icon: 'fa-solid fa-eye', priorityValue: null, items: [] },
    { id: 'done', name: 'DONE', color: '#10B981', icon: 'fa-solid fa-circle-check', priorityValue: null, items: [] },
    { id: 'cancelled', name: 'CANCELLED', color: '#EF4444', icon: 'fa-regular fa-circle-xmark', priorityValue: null, items: [] }
  ];

  const pGroups = [
    { id: 'p1', name: 'Urgent', color: '#EF4444', icon: 'fa-solid fa-angles-up', priorityValue: 1, items: [] },
    { id: 'p2', name: 'High', color: '#F97316', icon: 'fa-solid fa-chevron-up', priorityValue: 2, items: [] },
    { id: 'p3', name: 'Normal', color: '#3B82F6', icon: 'fa-solid fa-minus', priorityValue: 3, items: [] },
    { id: 'p4', name: 'Low', color: '#94A3B8', icon: 'fa-solid fa-chevron-down', priorityValue: 4, items: [] }
  ];

  const validTasks = filteredTasksList.value || [];

  if (groupBy.value === 'priority') {
     validTasks.forEach(t => {
       let col = pGroups.find(g => g.priorityValue === (t.priority || 3));
       if (!col) col = pGroups[2];
       col.items.push(t);
     });
     return pGroups;
  } else {
     validTasks.forEach(t => {
       const s = (t.statusName || 'BACKLOG').toUpperCase().trim();
       let col;
       if (s === 'BACKLOG') col = groups[0];
       else if (s === 'TODO' || s === 'TO DO') col = groups[1];
       else if (s === 'IN PROGRESS' || s === 'INPROGRESS') col = groups[2];
       else if (s === 'IN REVIEW' || s === 'REVIEW') col = groups[3];
       else if (s === 'DONE') col = groups[4];
       else if (s === 'CANCELLED' || s === 'CANCELED') col = groups[5];
       else col = groups[0]; // fallback to backlog
       
       col.items.push(t);
     });
     return groups;
  }
});

const listViewGroups = computed(() => {
  const groups = [
    { id: 'backlog', name: 'Backlog', statusName: 'BACKLOG', icon: 'fa-regular fa-circle-dashed', color: '#71717A', items: [] },
    { id: 'todo', name: 'To Do', statusName: 'TO DO', icon: 'fa-regular fa-circle', color: '#D4D4D8', items: [] },
    { id: 'inprogress', name: 'In Progress', statusName: 'IN PROGRESS', icon: 'fa-solid fa-circle-half-stroke', color: '#3B82F6', items: [] },
    { id: 'review', name: 'In Review', statusName: 'IN REVIEW', icon: 'fa-solid fa-eye', color: '#F59E0B', items: [] },
    { id: 'done', name: 'Done', statusName: 'DONE', icon: 'fa-solid fa-circle-check', color: '#10B981', items: [] },
    { id: 'cancelled', name: 'Cancelled', statusName: 'CANCELLED', icon: 'fa-regular fa-circle-xmark', color: '#EF4444', items: [] }
  ]

  filteredTasksList.value.forEach(task => {
    const status = normalizeStatus(task.statusName)
    const target = groups.find(group => group.statusName === status) || groups[0]
    target.items.push(task)
  })

  return groups
})

const toggleListGroup = (groupId) => {
  collapsedListGroups.value[groupId] = !collapsedListGroups.value[groupId]
}

const toggleTaskAssignee = (task, memberId) => {
  const currentIds = getTaskAssigneeIds(task)
  const nextIds = currentIds.includes(memberId)
    ? currentIds.filter(id => id !== memberId)
    : [...currentIds, memberId]

  task.assigneeIds = nextIds
  task.assignedUserId = nextIds[0] || null
  updateTask(task, 'assigneeIds', nextIds, currentIds)
}

const loadInitialData = async () => {
  let pid = getProjectId()
  if(!pid) {
      try {
          const res = await axiosClient.get('/projects');
          if (res.data?.data?.length > 0) {
              pid = res.data.data[0].id;
              dynamicProjectId = pid;
              localStorage.setItem('lastProjectId', pid);
          }
      } catch (err) {
          console.error('Cannot resolve valid projectId', err);
          return;
      }
  }

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
    const store = useWorkTaskStore();
    await store.fetchTasks(pid);
    rawTasks.value = store.tasks; // Keep local reactivity working while syncing Pinia
    
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

const putBackedTaskFields = new Set([
  'title',
  'description',
  'priority',
  'storyPoints',
  'assignedUserId',
  'plannedStartDate',
  'plannedEndDate',
  'dueDate',
  'sprintId',
  'taskTypeId'
])

const buildPutTaskPayload = (task, overrides = {}) => {
  const mergedTask = { ...task, ...overrides }

  return {
    title: mergedTask.title || '',
    description: mergedTask.description ?? '',
    priority: mergedTask.priority ?? 0,
    storyPoints: mergedTask.storyPoints ?? 0,
    assignedUserId: mergedTask.assignedUserId ?? mergedTask.assigneeId ?? null,
    plannedStartDate: mergedTask.plannedStartDate || null,
    plannedEndDate: mergedTask.plannedEndDate || null,
    dueDate: mergedTask.dueDate || null,
    sprintId: mergedTask.sprintId || null,
    taskTypeId: mergedTask.taskTypeId || '00000000-0000-0000-0000-000000000000',
    rowVersion: mergedTask.rowVersion || null
  }
}

const updateTask = async (task, field, value, previousValue = task ? task[field] : undefined) => {
   try {
      const pid = getProjectId();
      if (!pid || !task?.id) return;
      
      task[field] = value;
      const usesPutUpdate = putBackedTaskFields.has(field);
      const payload = usesPutUpdate
        ? buildPutTaskPayload(task, { [field]: value })
        : { [field]: value };

      await store.updateTask(pid, task.id, payload, { method: usesPutUpdate ? 'put' : 'patch' });
      await fetchTasks();
   } catch (error) {
      console.error('Failed to update task:', error);
      if (task) task[field] = previousValue;
      ElMessage.error(error.response?.data?.message || 'Khong the cap nhat cong viec');
      await fetchTasks();
   }
}

const openCreateTask = (statusName, defaults = {}) => {
   selectedTask.value = {
     isNew: true,
     title: '',
     description: '',
     statusName: statusName || 'BACKLOG',
     priority: 3,
     sprintId: activeSprintFilterId.value || null,
     plannedStartDate: defaults?.plannedStartDate || null,
     dueDate: defaults?.dueDate || null
   };
}

const openCreateTaskFromCalendar = (dates) => {
   openCreateTask('TO DO', dates);
}

const inlineInput = ref(null);

const openInlineCreate = (colId) => {
   inlineCreateColId.value = colId;
   inlineTaskTitle.value = '';
   nextTick(() => {
     if(inlineInput.value) {
        // inlineInput.value could be an array if inside v-for, or a proxy. We handle both:
        if (Array.isArray(inlineInput.value)) {
           inlineInput.value[0]?.focus();
        } else {
           inlineInput.value.focus();
        }
     }
   });
}

const submitInlineTask = async (col) => {
   if(!inlineTaskTitle.value.trim()) {
      inlineCreateColId.value = null;
      return;
   }
   try {
      await axiosClient.post(`/projects/${getProjectId()}/WorkTasks`, {
         title: inlineTaskTitle.value.trim(),
         description: '',
         statusName: col.name || 'BACKLOG',
          priority: 3,
          sprintId: activeSprintFilterId.value || null
      });
      inlineTaskTitle.value = '';
      fetchTasks();
   } catch (e) {
      console.error(e);
      ElMessage.error(e.response?.data?.message || 'Khong the tao cong viec');
   }
}

const handleListTaskCreate = async (payload) => {
   const pid = getProjectId();
   if (!pid) return;
   try {
      await axiosClient.post(`/projects/${pid}/WorkTasks`, {
         title: payload.title,
         description: '',
         statusName: payload.statusName || 'BACKLOG',
          priority: payload.priority || 3,
          sprintId: activeSprintFilterId.value || null
      });
      fetchTasks();
   } catch (error) {
      console.error(error);
      ElMessage.error(error.response?.data?.message || 'Khong the tao cong viec');
   }
}

const handleDraggableChange = async (evt, group) => {
  if (evt.added || evt.moved) {
    const element = evt.added ? evt.added.element : evt.moved.element;
    const newIndex = evt.added ? evt.added.newIndex : evt.moved.newIndex;
    const previousTask = { ...element };
    const getSortOrder = (task, fallback) => {
      const sortOrder = Number(task?.sortOrder);
      return Number.isFinite(sortOrder) ? sortOrder : fallback;
    };
    
    // Math cho LexoRank
    let newSortOrder = 65536;
    if (group.items.length === 1) {
       newSortOrder = 65536;
    } else if (newIndex === 0) {
       newSortOrder = getSortOrder(group.items[1], 131072) / 2.0;
    } else if (newIndex === group.items.length - 1) {
       newSortOrder = getSortOrder(group.items[group.items.length - 2], 0) + 65536;
    } else {
       const beforeSort = getSortOrder(group.items[newIndex - 1], 0);
       const afterSort = getSortOrder(group.items[newIndex + 1], beforeSort + 131072);
       newSortOrder = (beforeSort + afterSort) / 2.0;
    }
    
    element.sortOrder = newSortOrder;
    
    if (groupBy.value === 'status') {
       element.statusName = group.name; // Cập nhật Optimistic UI
       try {
         await store.reorderTask(getProjectId(), element.id, newSortOrder, group.name);
         await fetchTasks();
       } catch (error) {
         Object.assign(element, previousTask);
         ElMessage.error(error.response?.data?.message || 'Khong the cap nhat bang Kanban');
         console.error('Lỗi API reorder:', error);
         fetchTasks(); // Load lại data nếu gặp lỗi
       }
    } else if (groupBy.value === 'priority') {
       element.priority = group.priorityValue;
       try {
         await store.updateTask(getProjectId(), element.id, {
           sortOrder: newSortOrder,
           priority: group.priorityValue
          });
          await fetchTasks();
        } catch (error) {
          Object.assign(element, previousTask);
          ElMessage.error(error.response?.data?.message || 'Khong the cap nhat do uu tien');
         console.error('Lỗi API reorder:', error);
         fetchTasks();
       }
    }
  }
}


const handleGlobalCreate = (event) => {
    const detail = event?.detail || {};
    openCreateTask(detail.statusName || 'TO DO', {
      plannedStartDate: detail.plannedStartDate || null,
      dueDate: detail.dueDate || null
    });
}

const syncFiltersToUrl = () => {
  const query = { ...route.query }
  if (activeTaskFilters.value.length) {
    query.filters = encodeURIComponent(JSON.stringify(activeTaskFilters.value))
  } else {
    delete query.filters
  }
  router.replace({ query })
}

const applyTaskFilters = (filters) => {
  activeTaskFilters.value = Array.isArray(filters) ? filters : activeTaskFilters.value
  syncFiltersToUrl()
}

const removeTaskFilter = (id) => {
  activeTaskFilters.value = activeTaskFilters.value.filter(filter => filter.id !== id)
  syncFiltersToUrl()
}

const clearTaskFilters = () => {
  activeTaskFilters.value = []
  syncFiltersToUrl()
}

const hydrateFiltersFromUrl = () => {
  if (!route.query.filters) return
  try {
    const parsed = JSON.parse(decodeURIComponent(route.query.filters))
    activeTaskFilters.value = Array.isArray(parsed) ? parsed : []
    showFilterPanel.value = activeTaskFilters.value.length > 0
  } catch (error) {
    activeTaskFilters.value = []
  }
}

const exportAnalyticsCsv = (type) => {
  const rows = type === 'priority'
    ? [['Priority', 'Count'], ['Urgent', rawTasks.value.filter(t => t.priority === 1).length], ['High', rawTasks.value.filter(t => t.priority === 2).length], ['Normal', rawTasks.value.filter(t => t.priority === 3).length], ['Low', rawTasks.value.filter(t => t.priority === 4).length]]
    : [['Assignee', 'Count'], ['Unassigned', rawTasks.value.filter(t => !getTaskAssigneeIds(t).length).length]]
  const csv = rows.map(row => row.join(',')).join('\n')
  const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' })
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = url
  link.download = `${type}-analytics.csv`
  link.click()
  URL.revokeObjectURL(url)
}

onMounted(() => {
  hydrateFiltersFromUrl()
  loadInitialData()
  window.addEventListener('global-create-task', handleGlobalCreate)
})

watch(
  () => [route.query.tab, route.query.sprintId, route.query.moduleId, route.params.cycleId],
  () => {
    if (route.query.tab === 'spreadsheet' || activeSprintFilterId.value || activeModuleFilterId.value) {
      currentTab.value = 'spreadsheet'
    } else if (route.query.tab === 'board') {
      currentTab.value = 'board'
    }
  },
  { immediate: true }
)

onUnmounted(() => {
  window.removeEventListener('global-create-task', handleGlobalCreate)
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
.plane-toolbar-btn.active {
  background: #1E2025;
  color: #fff;
}
.filter-count {
  margin-left: 6px;
  min-width: 16px;
  height: 16px;
  border-radius: 999px;
  background: #0EA5E9;
  color: #fff;
  font-size: 10px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
}
.work-filter-row {
  padding: 10px 24px;
  border-bottom: 1px solid #1E2025;
  background: #0D0F11;
  flex-shrink: 0;
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
  flex: 1;
  overflow-y: auto;
  padding-right: 4px; /* for scrollbar */
  position: relative;
}

.chart-container {
  width: 100%;
  height: 250px;
}

.col-draggable {
  display: flex;
  flex-direction: column;
  gap: 12px;
  min-height: 10px;
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

.issue-sequence { font-size: 11px; color: #71717A; margin: 0; }
.issue-title {
  margin: 0;
  font-size: 13px;
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
.text-green { color: #10B981; }

.badge { border: 1px solid #27272A; border-radius: 4px; padding: 2px 6px; font-size: 11px; color: #A1A1AA; display: flex; align-items: center; gap: 6px; }

.add-btn-bottom { 
  color: #E5E7EB; 
  font-size: 13px; 
  font-weight: 500; 
  cursor: pointer; 
  display: flex; 
  align-items: center; 
  gap: 8px; 
  padding: 8px; 
  margin-top: 12px; 
  position: sticky;
  bottom: 0;
  background-color: #0D0F11;
  box-shadow: 0 -4px 10px rgba(13, 15, 17, 0.8);
  border-radius: 4px;
}
.add-btn-bottom:hover { color: white; background-color: #1E2025; }

.inline-create-box { 
  background: #111111; 
  border: 1px solid #27272A; 
  border-radius: 8px; 
  padding: 12px 16px; 
  margin-top: 12px; 
  box-shadow: 0 4px 12px rgba(0,0,0,0.5); 
  position: sticky;
  bottom: 0;
  z-index: 10;
  display: flex;
  flex-direction: column;
  gap: 12px;
}
.ic-top {
  display: flex;
  align-items: center;
  gap: 10px;
}
.ic-plus {
  color: #E5E7EB;
  font-size: 16px;
}
.ic-input { 
  width: 100%; 
  background: transparent; 
  border: none; 
  color: #E5E7EB; 
  outline: none; 
  font-size: 15px; 
  font-weight: 500;
  padding: 0;
}
.ic-input::placeholder { color: #E5E7EB; }

.ic-bottom {
  display: flex;
  align-items: center;
  gap: 8px;
}
.ic-chip {
  display: flex;
  align-items: center;
  gap: 6px;
  background: transparent;
  border: 1px solid #27272A;
  border-radius: 4px;
  padding: 4px 8px;
  font-size: 11px;
  color: #A1A1AA;
}
.ic-avatar {
  border: 1px dashed #3F3F46;
  background: transparent;
  color: #3F3F46;
  border-radius: 50%;
}

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

.plane-list-view {
  display: flex;
  flex-direction: column;
  color: #E4E4E7;
}

.list-group {
  margin-bottom: 24px;
}

.group-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 8px 0;
  cursor: pointer;
  border-bottom: 1px solid #1E2025;
  margin-bottom: 8px;
}

.group-header:hover .add-icon {
  opacity: 1;
}

.gh-left,
.gh-right,
.group-content,
.pill-group {
  display: flex;
  align-items: center;
}

.gh-left {
  gap: 10px;
}

.gh-chevron {
  font-size: 10px;
  color: #71717A;
  width: 14px;
  text-align: center;
}

.group-name {
  font-size: 14px;
  font-weight: 600;
  color: #E4E4E7;
}

.group-count {
  font-size: 12px;
  font-weight: 500;
  color: #71717A;
  margin-left: 4px;
}

.add-icon {
  color: #71717A;
  font-size: 14px;
  opacity: 0;
  transition: opacity 0.2s;
  padding: 4px;
}

.task-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 16px;
  padding: 10px 0 10px 24px;
  border-bottom: 1px solid #1E2025;
  cursor: pointer;
}

.task-row:hover {
  background-color: #16181D;
}

.tr-left,
.tr-right {
  display: flex;
  align-items: center;
}

.tr-left {
  gap: 16px;
  min-width: 0;
}

.tr-right {
  justify-content: flex-end;
}

.task-id {
  font-size: 12px;
  color: #71717A;
  font-weight: 600;
  min-width: 86px;
}

.task-title {
  color: #E4E4E7;
  font-size: 14px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.pill-group {
  gap: 8px;
  flex-wrap: wrap;
}

.pill {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  border: 1px solid #27272A;
  border-radius: 999px;
  padding: 5px 10px;
  font-size: 12px;
  color: #D4D4D8;
}

.pill-user-text {
  max-width: 140px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.avatar-xxs {
  width: 18px;
  height: 18px;
  border-radius: 999px;
  background: #1E2025;
  color: #E4E4E7;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-size: 10px;
  border: 1px solid #3F3F46;
}

.add-row-placeholder {
  color: #A1A1AA;
  font-size: 13px;
  padding: 10px 0 10px 24px;
  cursor: pointer;
}

.add-row-placeholder:hover {
  color: #FFFFFF;
  background: #16181D;
}

.plane-dropdown {
  background: #1E2025 !important;
  border: 1px solid #333 !important;
}

.plane-search-input {
  width: 100%;
  background: transparent;
  border: 1px solid #27272A;
  color: #E4E4E7;
  border-radius: 6px;
  padding: 8px 10px;
  outline: none;
}

.plane-list {
  display: flex;
  flex-direction: column;
  gap: 6px;
  max-height: 220px;
  overflow-y: auto;
}

.plane-list-item {
  display: flex;
  align-items: center;
  gap: 8px;
  color: #D4D4D8;
  cursor: pointer;
  padding: 6px 8px;
  border-radius: 6px;
}

.plane-list-item:hover {
  background: #27272A;
}

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
