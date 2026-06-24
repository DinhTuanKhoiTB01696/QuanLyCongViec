<template>
  <NexusLayout>
    <div v-if="isForbidden" class="forbidden-overlay">
      <div class="forbidden-content">
        <div class="lock-icon"><i class="fa-solid fa-lock"></i></div>
        <h2>Access Denied</h2>
        <p>Bạn không đủ quyền để truy cập dự án này.</p>
        <button class="plane-primary-btn mt-4" @click="router.push('/spaces')">Quay lại trang Home</button>
      </div>
    </div>
    <div v-else class="plane-board-container">
      <!-- Plane Style Header -->
      <header class="plane-space-header">
        <div class="sh-left">
          <div class="breadcrumb">
            <span class="proj-icon">{{ projectBadge }}</span>
            <span class="proj-name">{{ project?.name || 'Project' }}</span>
            <i class="fa-solid fa-chevron-right separator"></i>
            <span class="active-page">
              <i class="fa-solid fa-layer-group"></i> Work Items
            </span>
            <span class="item-count">{{ visibleTopLevelTasks.length }}</span>
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
                      <label class="dd-item"><input type="radio" name="order" value="manual" v-model="displayOrder" /> Manual</label>
                      <label class="dd-item"><input type="radio" name="order" value="created" v-model="displayOrder" /> Last created</label>
                      <label class="dd-item"><input type="radio" name="order" value="updated" v-model="displayOrder" /> Last updated</label>
                      <label class="dd-item"><input type="radio" name="order" value="priority" v-model="displayOrder" /> Priority</label>
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
              <template v-for="task in group.items" :key="task.id">
               <div class="task-row" @click="openTaskDetail(task)">
                 <div class="tr-left">
                   <button class="star-task-btn" :class="{ starred: isTaskStarred(task.id) }" @click.stop="toggleTaskStar(task)">
                     <i :class="isTaskStarred(task.id) ? 'fa-solid fa-star text-yellow-400' : 'fa-regular fa-star text-gray-400'"></i>
                   </button>
                   <span class="task-id" style="margin-left: 8px;">{{ task.sequenceId || task.id.substring(0,8).toUpperCase() }}</span>
                   <span class="task-title" :style="group.statusName === 'DONE' ? { textDecoration: 'line-through', color: 'var(--color-text-muted)' } : {}">
                     {{ task.title }}
                   </span>
                 </div>
                 <div class="tr-right" @click.stop>
                   <div class="pill-group">
                     <el-dropdown trigger="click" @command="(val) => updateTask(task, 'statusName', val, task.statusName)">
                       <div class="pill pill-status cursor-pointer hover:bg-[var(--color-border)]">
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
                       <div class="pill pill-priority cursor-pointer hover:bg-[var(--color-border)]">
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
                         <div class="pill pill-user cursor-pointer hover:bg-[var(--color-border)]">
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
               </template>

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
         <TimelineTab :projectId="getProjectId()" :tasks="filteredTasksList" @open-task="openTaskDetail" @create-task="openCreateTaskFromCalendar" />
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
                  <div class="flex-between mb-1">
                    <p class="issue-sequence">{{ element.sequenceId || element.id.substring(0,8).toUpperCase() }}</p>
                    <button class="star-task-btn small" @click.stop="toggleTaskStar(element)">
                      <i :class="isTaskStarred(element.id) ? 'fa-solid fa-star text-yellow-400' : 'fa-regular fa-star text-gray-400'"></i>
                    </button>
                  </div>
                  <p class="issue-title" :style="element.statusName === 'DONE' ? { textDecoration: 'line-through', color: 'var(--color-text-muted)' } : {}">{{ element.title }}</p>
                  <div class="issue-meta mt-2" style="display:flex; align-items:center; gap:8px;" @click.stop>
                     <el-dropdown trigger="click" @command="(val) => updateTask(element, 'statusName', val, element.statusName)">
                       <div class="badge cursor-pointer hover:bg-[var(--color-border)]">
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
                       <div class="badge cursor-pointer hover:bg-[var(--color-border)]">
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
                         <div class="avatar-xs ms-auto cursor-pointer hover:bg-[var(--color-border)]" v-if="getTaskAssigneeSummary(element).label">
                           {{ getTaskAssigneeSummary(element).avatar }}
                         </div>
                         <div class="avatar-xs ms-auto cursor-pointer hover:bg-[var(--color-border)]" style="border: 1px dashed var(--color-text-muted); background: transparent; color: var(--color-text-muted);" v-else><i class="fa-solid fa-user"></i></div>
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
      :currentProjectRole="currentProjectRole"
      :canGoBack="taskDetailHistory.length > 0"
      @close="closeTaskDetail"
      @back="goBackTaskDetail"
      @open-task="openTaskDetailFromModal"
      @updateTask="updateTask"
      @refresh-tasks="fetchTasks"
    />

    <!-- Analytics Sidebar Overlay -->
    <div v-if="showAnalyticsSidebar" class="analytics-overlay" @click.self="closeAnalyticsSidebar">
      <div class="analytics-panel" :class="{ 'slide-in': showAnalyticsSidebar, 'is-expanded': isAnalyticsExpanded }">
         <div class="ap-header">
            <h3>Thống kê {{ project?.name || t('Project') }}</h3>
            <div class="ap-actions">
               <button class="icon-btn" @click="toggleAnalyticsExpand"><i :class="isAnalyticsExpanded ? 'fa-solid fa-compress' : 'fa-solid fa-expand'"></i></button>
               <button class="icon-btn" @click="closeAnalyticsSidebar"><i class="fa-solid fa-xmark"></i></button>
            </div>
         </div>
         
         <div class="ap-body">
            <!-- Stats -->
            <div class="ap-stats-grid">
               <div class="stat-box">
                  <span class="lbl">Tổng công việc</span>
                  <span class="val">{{ visibleTopLevelTasks.length }}</span>
               </div>
               <div class="stat-box">
                  <span class="lbl">Đang thực hiện</span>
                  <span class="val">{{ visibleTopLevelTasks.filter(t => t.statusName === 'IN PROGRESS').length }}</span>
               </div>
               <div class="stat-box">
                  <span class="lbl">Chờ xử lý</span>
                  <span class="val">{{ visibleTopLevelTasks.filter(t => !t.statusName || t.statusName === 'TO DO' || t.statusName === 'TODO').length }}</span>
               </div>
               <div class="stat-box">
                  <span class="lbl">Đang đánh giá</span>
                  <span class="val">{{ visibleTopLevelTasks.filter(t => t.statusName === 'IN REVIEW').length }}</span>
               </div>
               <div class="stat-box">
                  <span class="lbl">Hoàn thành</span>
                  <span class="val">{{ visibleTopLevelTasks.filter(t => t.statusName === 'DONE').length }}</span>
               </div>
            </div>
            
            <!-- Created vs Resolved Chart Overlay -->
            <div class="ap-chart-card mt-4">
               <h4>Đã tạo và đã xử lý</h4>
               <v-chart class="chart-container" :option="createdResolvedOptions" autoresize />
            </div>

            <!-- Customized Insights -->
            <div class="ap-chart-card mt-4">
               <div class="flex-between">
                  <h4>Phân tích tùy chỉnh</h4>
                  <el-dropdown trigger="click" @command="setAnalyticsInsightMode">
                    <button class="filter-btn" type="button">
                      <i class="fa-solid fa-sliders"></i> {{ analyticsInsightLabel }} <i class="fa-solid fa-chevron-down"></i>
                    </button>
                    <template #dropdown>
                      <el-dropdown-menu class="plane-dropdown">
                        <el-dropdown-item command="priority">Phân bổ độ ưu tiên</el-dropdown-item>
                        <el-dropdown-item command="status">Phân bổ trạng thái</el-dropdown-item>
                        <el-dropdown-item command="assignee">Phân bổ người thực hiện</el-dropdown-item>
                      </el-dropdown-menu>
                    </template>
                  </el-dropdown>
               </div>
               
               <v-chart class="chart-container mt-4" :option="insightChartOptions" autoresize />
            </div>
            
            <!-- Tables -->
            <div class="ap-table-wrap mt-4">
               <div class="table-head">
                  <span class="text-muted">{{ analyticsBreakdownRows.length }} {{ analyticsTableHeading }}</span>
                  <div class="flex-center gap-1">
                     <i class="fa-solid fa-magnifying-glass text-muted"></i>
                     <button class="export-btn" @click="exportAnalyticsCsv()"><i class="fa-solid fa-download"></i> Xuất CSV</button>
                  </div>
               </div>
               <table class="ap-table">
                  <thead><tr><th>{{ analyticsTableHeading }}</th><th style="text-align: right;">Số lượng</th></tr></thead>
                  <tbody>
                     <tr v-for="row in analyticsBreakdownRows" :key="row.label">
                       <td>{{ row.label }}</td>
                       <td style="text-align: right;">{{ row.count }}</td>
                     </tr>
                  </tbody>
               </table>
            </div>

            <div class="ap-table-wrap mt-4">
               <div class="table-head">
                  <span class="text-muted">{{ assigneeAnalyticsRows.length }} người thực hiện</span>
                  <div class="flex-center gap-1">
                     <i class="fa-solid fa-magnifying-glass text-muted"></i>
                     <button class="export-btn" @click="exportAnalyticsCsv('assignee')"><i class="fa-solid fa-download"></i> Xuất CSV</button>
                  </div>
               </div>
               <table class="ap-table">
                  <thead>
                     <tr>
                        <th>Người thực hiện</th>
                        <th style="text-align: right;">Chờ xử lý</th>
                        <th style="text-align: right;">Đang làm</th>
                        <th style="text-align: right;">Đang đánh giá</th>
                        <th style="text-align: right;">Hoàn thành</th>
                        <th style="text-align: right;">Đã hủy</th>
                     </tr>
                  </thead>
                  <tbody>
                     <tr v-for="row in assigneeAnalyticsRows" :key="row.id">
                        <td><i class="fa-regular fa-user"></i> {{ row.label }}</td>
                        <td style="text-align: right;">{{ row.backlog }}</td>
                        <td style="text-align: right;">{{ row.started }}</td>
                        <td style="text-align: right;">{{ row.unstarted }}</td>
                        <td style="text-align: right;">{{ row.completed }}</td>
                        <td style="text-align: right;">{{ row.cancelled }}</td>
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
  import { broadcastAdminRealtime, subscribeAdminRealtime } from '@/utils/adminRealtime'
  import { getStoredUserSession } from '@/utils/authSession'
  import { getScopedCurrentProjectId, setScopedCurrentProjectId } from '@/utils/projectContext'
  import { signalRService } from '@/api/signalrService'
import { hasSystemAdminAccess, normalizeProjectRole } from '@/utils/permissions'
import NexusLayout from '@/components/layout/NexusLayout.vue'
import draggable from 'vuedraggable'
import TaskDetailModal from '@/components/TaskDetailModal.vue'
import CalendarTab from '@/components/CalendarTab.vue'
import TimelineTab from '@/components/TimelineTab.vue'
import SpreadsheetTab from '@/components/SpreadsheetTab.vue'
import FilterBar from '@/components/FilterBar.vue'
import { useWorkTaskStore } from '@/store/useWorkTaskStore';
import { useProjectStore } from '@/store/useProjectStore';
import { useI18nStore } from '@/store/useI18nStore';

import { use } from 'echarts/core';
import { CanvasRenderer } from 'echarts/renderers';
import { LineChart, BarChart } from 'echarts/charts';
import { TitleComponent, TooltipComponent, LegendComponent, GridComponent } from 'echarts/components';
import { LegacyGridContainLabel } from 'echarts/features';
import VChart from 'vue-echarts';

use([
  CanvasRenderer,
  LineChart,
  BarChart,
  TitleComponent,
  TooltipComponent,
  LegendComponent,
  GridComponent,
  LegacyGridContainLabel
]);

const showDisplayDropdown = ref(false)
const showAnalyticsSidebar = ref(false)
const isAnalyticsExpanded = ref(false)
const showFilterPanel = ref(false)
const isForbidden = ref(false)
const showSubtasks = ref(false)
const collapsedListGroups = ref({})
const assigneeSearch = ref('')

const route = useRoute()
const router = useRouter()
const currentProjectId = computed(() => route.params.id || getScopedCurrentProjectId() || null)
const store = useWorkTaskStore();
const projectStore = useProjectStore()
const i18nStore = useI18nStore()
const tr = (en, vi) => i18nStore.locale === 'vi' ? vi : en
const t = (key) => {
  const map = {
    'Project': 'Dự án',
    'Work Items': 'Công việc',
    'Display': 'Hiển thị',
    'Display Properties': 'Thuộc tính hiển thị',
    'Order by': 'Sắp xếp theo',
    'Manual': 'Thủ công',
    'Last created': 'Tạo gần nhất',
    'Last updated': 'Cập nhật gần nhất',
    'Priority': 'Độ ưu tiên',
    'Show sub-work items': 'Hiển thị công việc con',
    'Analytics': 'Thống kê',
    'Add work item': 'Thêm công việc',
    'Access Denied': 'Truy cập bị từ chối',
    'You do not have permission to access this project.': 'Bạn không đủ quyền để truy cập dự án này.',
    'Back to Home': 'Quay lại trang Home',
    'List view': 'Xem danh sách',
    'Kanban view': 'Xem Kanban',
    'Calendar view': 'Xem lịch',
    'Spreadsheet view': 'Xem bảng tính',
    'Gantt chart view': 'Xem biểu đồ Gantt',
    'Urgent': 'Khẩn cấp',
    'High': 'Cao',
    'Normal': 'Bình thường',
    'Medium': 'Trung bình',
    'Low': 'Thấp',
    'None': 'Không',
    'Search members': 'Tìm thành viên',
    'New work item': 'Công việc mới',
    'Statistics of': 'Thống kê',
    'Total tasks': 'Tổng công việc',
    'In progress': 'Đang thực hiện',
    'Pending': 'Chờ xử lý',
    'In review': 'Đang đánh giá',
    'Completed': 'Hoàn thành',
    'Created and resolved': 'Đã tạo và đã xử lý',
    'Custom analysis': 'Phân tích tùy chỉnh',
    'Priority distribution': 'Phân bổ độ ưu tiên',
    'Status distribution': 'Phân bổ trạng thái',
    'Assignee distribution': 'Phân bổ người thực hiện',
    'Export CSV': 'Xuất CSV',
    'Count': 'Số lượng',
    'assignees': 'người thực hiện',
    'Assignee': 'Người thực hiện',
    'Working': 'Đang làm',
    'Cancelled': 'Đã hủy'
  }
  if (i18nStore.locale === 'vi') {
    return map[key] || key
  }
  return key
}

const project = ref({})
const rawTasks = ref([])
const allTasks = ref([])
const projectMembers = ref([])
const projectStatuses = ref([])
const projectExecutionRules = ref({
  enableRoleBasedTaskVisibility: false,
  managerAlwaysSeeAllTasks: true
})
const visibilityOverrideRoles = ['pm', 'po', 'project_lead', 'admin']
const selectedTask = ref(null)
const taskDetailHistory = ref([])
const inlineCreateColId = ref(null)
const inlineTaskTitle = ref('')

const currentTab = ref('board')
const searchQuery = ref('')
const activeFilters = ref({ assignee: null })
const activeTaskFilters = ref([])
const displayOrder = ref('manual')
const groupBy = ref('status')
const analyticsInsightMode = ref('priority')
const activeSprintFilterId = computed(() => route.query.sprintId || route.params.cycleId || null)

watch(currentTab, (val) => {
  if (val === 'board') {
    document.body.classList.add('no-shadow-context')
  } else {
    document.body.classList.remove('no-shadow-context')
  }
}, { immediate: true })

onUnmounted(() => {
  document.body.classList.remove('no-shadow-context')
})
const activeModuleFilterId = computed(() => route.query.moduleId || null)
const activeCarryOverSprintId = computed(() => route.query.carryOverFromSprintId || null)
const carryOverTaskIds = ref([])
const projectBadge = computed(() => project.value?.icon || project.value?.identifier?.charAt(0)?.toUpperCase() || project.value?.name?.charAt(0)?.toUpperCase() || 'P')
const getShowSubtasksStorageKey = (projectId = currentProjectId.value || getProjectId()) => `space-summary:${projectId || 'default'}:show-subtasks`
const loadShowSubtasksPreference = (projectId = currentProjectId.value || getProjectId()) => {
  try {
    return localStorage.getItem(getShowSubtasksStorageKey(projectId)) === 'true'
  } catch {
    return false
  }
}
const persistShowSubtasksPreference = (value, projectId = currentProjectId.value || getProjectId()) => {
  try {
    localStorage.setItem(getShowSubtasksStorageKey(projectId), value ? 'true' : 'false')
  } catch {
    // ignore storage failures
  }
}

const isForbiddenError = (error) => Number(error?.response?.status) === 403

const resolveAccessibleProjectId = async (preferredProjectId = null) => {
  const projects = await projectStore.fetchAllProjects(true)
  const accessibleProjects = (projects || []).filter(item => item?.id)
  if (!accessibleProjects.length) return null

  if (preferredProjectId && accessibleProjects.some(item => `${item.id}` === `${preferredProjectId}`)) {
    return preferredProjectId
  }

  return accessibleProjects[0].id
}

const recoverFromForbiddenProject = async (forbiddenProjectId) => {
  const fallbackProjectId = await resolveAccessibleProjectId()
  if (!fallbackProjectId) {
    rawTasks.value = []
    allTasks.value = []
    projectMembers.value = []
    project.value = {}
    ElMessage.error('You no longer have access to any project.')
    return false
  }

  if (`${fallbackProjectId}` === `${forbiddenProjectId}`) {
    rawTasks.value = []
    allTasks.value = []
    projectMembers.value = []
    project.value = {}
    ElMessage.error('You do not have permission to load work items for this project.')
    return false
  }

  dynamicProjectId = fallbackProjectId
  setScopedCurrentProjectId(fallbackProjectId)
  await router.replace(`/space/${fallbackProjectId}`)
  return true
}

const getParentTaskLinkId = (task) => (
  task?.parentTaskId ||
  task?.parentId ||
  task?.ParentTaskId ||
  task?.ParentId ||
  null
)

const isSubtask = (task) => Boolean(
  getParentTaskLinkId(task) ||
  task?.parentTaskTitle ||
  task?.parentTitle ||
  task?.parentName
)

const getTaskAssigneeIds = (task) => {
  return Array.from(new Set([
    ...(Array.isArray(task.assigneeIds) ? task.assigneeIds : []),
    ...(Array.isArray(task.assignees) ? task.assignees.map(item => item.userId || item.id).filter(Boolean) : []),
    ...(task.assignedUserId ? [task.assignedUserId] : [])
  ]))
}

const getTaskAssigneeSummary = (task) => {
  const ids = getTaskAssigneeIds(task)
  if (!ids.length) return { label: '', avatar: '' }
  if (ids.length === 1) {
    const member = projectMembers.value.find(item => (item.userId || item.id) === ids[0])
    const label = member?.fullName || member?.name || member?.email || task.assigneeName || 'Assignee'
    return { label, avatar: label.substring(0, 1).toUpperCase() }
  }

  return { label: `${ids.length} assignees`, avatar: `${ids.length}` }
}

const matchesTaskFilters = (task) => {
  if (!task) return false

  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    const title = task.title?.toLowerCase?.() || ''
    const sequenceId = task.sequenceId?.toLowerCase?.() || ''
    if (!title.includes(query) && !sequenceId.includes(query)) {
      return false
    }
  }

  if (activeFilters.value.assignee) {
    return getTaskAssigneeIds(task).includes(activeFilters.value.assignee.userId)
  }

  return true
}

const topLevelTasks = computed(() => rawTasks.value)
const visibleTasks = computed(() => {
  const sourceTasks = showSubtasks.value ? (allTasks.value || []) : topLevelTasks.value
  return sourceTasks.filter(canCurrentUserSeeTask)
})
const visibleTopLevelTasks = computed(() => filteredTasksList.value.filter(task => !isSubtask(task)))
const defaultTaskStatusOptions = computed(() => [
  { name: 'BACKLOG', label: tr('Backlog', 'Chờ xử lý'), color: 'var(--color-text-muted)', icon: 'fa-regular fa-circle-dashed' },
  { name: 'TO DO', label: tr('To Do', 'Cần làm'), color: '#D4D4D8', icon: 'fa-regular fa-circle' },
  { name: 'IN PROGRESS', label: tr('In Progress', 'Đang thực hiện'), color: '#3B82F6', icon: 'fa-solid fa-circle-half-stroke' },
  { name: 'IN REVIEW', label: tr('In Review', 'Đang đánh giá'), color: '#F59E0B', icon: 'fa-solid fa-eye' },
  { name: 'DONE', label: tr('Done', 'Hoàn thành'), color: '#10B981', icon: 'fa-solid fa-circle-check' },
  { name: 'CANCELLED', label: tr('Cancelled', 'Đã hủy'), color: '#EF4444', icon: 'fa-regular fa-circle-xmark' }
])

const normalizeText = (value) => `${value || ''}`.toLowerCase().trim()
const normalizeStatus = (value) => `${value || 'BACKLOG'}`.toUpperCase().replace(/\s+/g, ' ').trim()
const resolveStatusIcon = (value) => {
  const status = normalizeStatus(value)
  if (status.includes('CANCEL')) return 'fa-regular fa-circle-xmark'
  if (status.includes('DONE') || status.includes('COMPLETE')) return 'fa-solid fa-circle-check'
  if (status.includes('PROGRESS') || status.includes('ACTIVE')) return 'fa-solid fa-circle-half-stroke'
  if (status.includes('REVIEW') || status.includes('TEST')) return 'fa-solid fa-eye'
  if (status.includes('TODO') || status.includes('TO DO')) return 'fa-regular fa-circle'
  return 'fa-regular fa-circle-dashed'
}
const taskStatusOptions = computed(() => {
  if (projectStatuses.value.length) {
    return projectStatuses.value.map((status, index) => ({
      name: normalizeStatus(status.name),
      label: status.displayName || status.name,
      color: status.colorCode || defaultTaskStatusOptions.value[index % defaultTaskStatusOptions.value.length]?.color || 'var(--color-text-muted)',
      icon: resolveStatusIcon(status.name)
    }))
  }

  return defaultTaskStatusOptions.value
})
const normalizeDateOnly = (value) => {
  if (!value) return null
  if (typeof value === 'string' && /^\d{4}-\d{2}-\d{2}$/.test(value)) return value
  if (typeof value === 'string' && /^\d{4}-\d{2}-\d{2}T/.test(value)) return value.slice(0, 10)
  if (value instanceof Date) {
    const year = value.getFullYear()
    const month = `${value.getMonth() + 1}`.padStart(2, '0')
    const day = `${value.getDate()}`.padStart(2, '0')
    return `${year}-${month}-${day}`
  }

  const parsed = new Date(value)
  if (Number.isNaN(parsed.getTime())) return null
  const year = parsed.getFullYear()
  const month = `${parsed.getMonth() + 1}`.padStart(2, '0')
  const day = `${parsed.getDate()}`.padStart(2, '0')
  return `${year}-${month}-${day}`
}
const getTaskDateOnly = (task, fields) => {
  for (const field of fields) {
    const normalized = normalizeDateOnly(task?.[field])
    if (normalized) return normalized
  }
  return null
}
const getTaskCreatedDate = (task) => getTaskDateOnly(task, ['createdAt', 'createdDate', 'createdOn', 'CreatedAt', 'CreatedDate'])
const getTaskResolvedDate = (task) => {
  if (normalizeStatus(task?.statusName) !== 'DONE') return null
  return getTaskDateOnly(task, [
    'completedAt',
    'completedDate',
    'resolvedAt',
    'doneAt',
    'closedAt',
    'updatedAt',
    'updatedDate',
    'UpdatedAt'
  ]) || getTaskCreatedDate(task)
}
const formatAnalyticsDateLabel = (dateOnly) => {
  if (!dateOnly) return ''
  const [year, month, day] = dateOnly.split('-').map(Number)
  return new Date(year, month - 1, day).toLocaleDateString('vi-VN', {
    day: '2-digit',
    month: '2-digit'
  })
}
const buildAnalyticsDateBuckets = (tasks) => {
  const dates = new Set()
  tasks.forEach(task => {
    const createdDate = getTaskCreatedDate(task)
    const resolvedDate = getTaskResolvedDate(task)
    if (createdDate) dates.add(createdDate)
    if (resolvedDate) dates.add(resolvedDate)
  })

  const sortedDates = Array.from(dates).sort()
  const windowDates = sortedDates.length > 14 ? sortedDates.slice(-14) : sortedDates
  const fallbackDate = normalizeDateOnly(new Date())
  const labels = windowDates.length ? windowDates : [fallbackDate]
  const createdCounts = new Map(labels.map(date => [date, 0]))
  const resolvedCounts = new Map(labels.map(date => [date, 0]))

  tasks.forEach(task => {
    const createdDate = getTaskCreatedDate(task)
    if (createdCounts.has(createdDate)) {
      createdCounts.set(createdDate, createdCounts.get(createdDate) + 1)
    }

    const resolvedDate = getTaskResolvedDate(task)
    if (resolvedCounts.has(resolvedDate)) {
      resolvedCounts.set(resolvedDate, resolvedCounts.get(resolvedDate) + 1)
    }
  })

  return {
    labels,
    created: labels.map(date => createdCounts.get(date) || 0),
    resolved: labels.map(date => resolvedCounts.get(date) || 0)
  }
}
const normalizeStatusLabel = (value) => {
  const status = normalizeStatus(value)
  return taskStatusOptions.value.find(item => item.name === status)?.label || status
}
const analyticsStatusBucket = (statusName) => {
  const normalized = normalizeStatus(statusName)
  if (normalized === 'BACKLOG') return 'backlog'
  if (normalized === 'IN PROGRESS') return 'started'
  if (normalized === 'DONE') return 'completed'
  if (normalized === 'CANCELLED') return 'cancelled'
  return 'unstarted'
}
const getBoardStatusIcon = (value) => taskStatusOptions.value.find(item => item.name === normalizeStatus(value))?.icon || 'fa-regular fa-circle-dashed'
const getStatusColor = (value) => taskStatusOptions.value.find(item => item.name === normalizeStatus(value))?.color || 'var(--color-text-muted)'
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
  const user = getStoredUserSession()
  return user?.id || user?.userId || null
}

const toggleTaskStar = (task) => {
  store.toggleTaskStar(task.id)
}

const isTaskStarred = (taskId) => {
  return store.isTaskStarred(taskId)
}

const currentProjectRole = computed(() => {
  const currentUser = getStoredUserSession()
  const currentUserIdValue = currentUser?.id || currentUser?.userId
  const matchedMember = (projectMembers.value || [])
    .find(member => `${member.userId || member.id || ''}` === `${currentUserIdValue || ''}`)
  const membershipRole = matchedMember?.projectRole || matchedMember?.ProjectRole

  const role = membershipRole
    || project.value?.myRole
    || project.value?.MyRole
    || project.value?.projectRole
    || project.value?.ProjectRole

  return normalizeProjectRole(role)
})

const canCurrentUserSeeTask = (task) => {
  const rules = projectExecutionRules.value || {}

  const currentUser = getStoredUserSession()
  if (hasSystemAdminAccess(currentUser)) return true

  if (rules.managerAlwaysSeeAllTasks && currentProjectRole.value && visibilityOverrideRoles.includes(currentProjectRole.value)) {
    return true
  }

  const visibilityMode = normalizeProjectRole(task?.visibilityMode || '').replace(/_scoped$/, '') || 'project'
  if (visibilityMode === 'project') return true

  const me = currentUserId()
  const assigneeIds = getTaskAssigneeIds(task)
  if (visibilityMode === 'assigned') {
    return Boolean(
      me && (
        `${task?.assignedUserId || ''}` === `${me}`
        || assigneeIds.some(id => `${id}` === `${me}`)
      )
    )
  }
  if (visibilityMode === 'role') {
    if (!currentProjectRole.value) return false
    return (task?.visibleToRoles || [])
      .map(role => normalizeProjectRole(role))
      .includes(currentProjectRole.value)
  }

  return true
}
const getTaskDate = (task, field) => {
  const value = task[field] || (field === 'startDate' ? task.plannedStartDate : null) || (field === 'dueDate' ? task.dueDate : null)
  if (!value) return null
  if (typeof value === 'string' && /^\d{4}-\d{2}-\d{2}$/.test(value)) {
    const [year, month, day] = value.split('-').map(Number)
    return new Date(year, month - 1, day)
  }
  const date = new Date(value)
  return Number.isNaN(date.getTime()) ? null : date
}
const taskHasModuleMatch = (task, moduleId) => {
  if (!task || !moduleId) return false
  if (task.moduleId === moduleId) return true
  if (Array.isArray(task.moduleIds) && task.moduleIds.includes(moduleId)) return true
  if (Array.isArray(task.modules) && task.modules.some(item => (item.id || item.moduleId) === moduleId)) return true
  return false
}
const taskMatchesSprintScope = (task, sprintId) => {
  if (!sprintId) return true
  if (task.sprintId === sprintId) return true
  if (isSubtask(task)) return false
  return allTasks.value.some(candidate => isSubtask(candidate) && getParentTaskLinkId(candidate) === task.id && candidate.sprintId === sprintId)
}
const taskMatchesModuleScope = (task, moduleId) => {
  if (!moduleId) return true
  if (taskHasModuleMatch(task, moduleId)) return true
  if (isSubtask(task)) return false
  return allTasks.value.some(candidate => isSubtask(candidate) && getParentTaskLinkId(candidate) === task.id && taskHasModuleMatch(candidate, moduleId))
}

const taskMatchesCarryOverScope = (task, taskIds) => {
  if (!taskIds.length) return true
  if (taskIds.includes(task.id)) return true
  if (isSubtask(task)) return false
  return allTasks.value.some(candidate => isSubtask(candidate) && getParentTaskLinkId(candidate) === task.id && taskIds.includes(candidate.id))
}
const prioritySortWeight = (priority) => {
  if (priority === 1) return 0
  if (priority === 2) return 1
  if (priority === 3) return 2
  if (priority === 4) return 3
  return 4
}
const toTimestamp = (value) => {
  if (!value) return 0
  const date = new Date(value)
  return Number.isNaN(date.getTime()) ? 0 : date.getTime()
}
const sortTasksByDisplayOrder = (tasks) => {
  const items = [...tasks]

  return items.sort((a, b) => {
    const aStarred = store.isTaskStarred(a.id)
    const bStarred = store.isTaskStarred(b.id)
    if (aStarred !== bStarred) {
      return aStarred ? -1 : 1
    }

    if (displayOrder.value === 'created') {
      return toTimestamp(b.createdAt) - toTimestamp(a.createdAt) || (Number(a.sortOrder) || 0) - (Number(b.sortOrder) || 0)
    }

    if (displayOrder.value === 'updated') {
      return toTimestamp(b.updatedAt || b.createdAt) - toTimestamp(a.updatedAt || a.createdAt) || (Number(a.sortOrder) || 0) - (Number(b.sortOrder) || 0)
    }

    if (displayOrder.value === 'priority') {
      return prioritySortWeight(a.priority) - prioritySortWeight(b.priority) || (Number(a.sortOrder) || 0) - (Number(b.sortOrder) || 0)
    }

    return (Number(a.sortOrder) || 0) - (Number(b.sortOrder) || 0)
  })
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
    let p = dynamicProjectId || currentProjectId.value || getScopedCurrentProjectId();
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

  filteredTasks = filteredTasks.filter(matchesTaskFilters)
  if (activeSprintFilterId.value) {
     filteredTasks = filteredTasks.filter(t => taskMatchesSprintScope(t, activeSprintFilterId.value));
  }
  if (activeModuleFilterId.value) {
     filteredTasks = filteredTasks.filter(t => taskMatchesModuleScope(t, activeModuleFilterId.value));
  }
  if (carryOverTaskIds.value.length) {
     filteredTasks = filteredTasks.filter(t => taskMatchesCarryOverScope(t, carryOverTaskIds.value));
  }
  if (activeTaskFilters.value.length) {
     filteredTasks = filteredTasks.filter(task => activeTaskFilters.value.every(filter => taskMatchesFilter(task, filter)));
  }

  const shouldIncludeSubtasks = showSubtasks.value
  return sortTasksByDisplayOrder(shouldIncludeSubtasks ? filteredTasks : filteredTasks.filter(task => !isSubtask(task)));
});

const createdResolvedOptions = computed(() => {
   const buckets = buildAnalyticsDateBuckets(visibleTopLevelTasks.value)
   return {
      tooltip: {
        trigger: 'axis',
        backgroundColor: '#f8fafc',
        borderWidth: 0,
        textStyle: { color: '#0f172a' }
      },
      legend: { data: [tr('Created', 'Đã tạo'), tr('Resolved', 'Đã xử lý')], bottom: 0, textStyle: { color: 'var(--color-text-muted)' } },
      grid: { left: '2%', right: '3%', bottom: '16%', top: '10%', containLabel: true },
      xAxis: {
        type: 'category',
        data: buckets.labels.map(formatAnalyticsDateLabel),
        axisLine: { lineStyle: { color: 'rgba(148, 163, 184, 0.35)' } },
        axisLabel: { color: 'var(--color-text-muted)' }
      },
      yAxis: {
        type: 'value',
        minInterval: 1,
        splitLine: { lineStyle: { color: 'rgba(148, 163, 184, 0.16)' } },
        axisLabel: { color: 'var(--color-text-muted)' }
      },
      series: [
         {
           name: tr('Created', 'Đã tạo'),
           type: 'line',
           data: buckets.created,
           symbolSize: 8,
           lineStyle: { width: 3, color: '#38BDF8' },
           itemStyle: { color: '#38BDF8' },
           areaStyle: { color: 'rgba(56, 189, 248, 0.12)' },
           smooth: true
         },
         {
           name: tr('Resolved', 'Đã xử lý'),
           type: 'line',
           data: buckets.resolved,
           symbolSize: 8,
           lineStyle: { width: 3, color: '#34D399' },
           itemStyle: { color: '#34D399' },
           areaStyle: { color: 'rgba(52, 211, 153, 0.1)' },
           smooth: true
         }
      ],
      backgroundColor: 'transparent'
   }
});

const analyticsBreakdownRows = computed(() => {
  if (analyticsInsightMode.value === 'assignee') {
    const counts = new Map()

    visibleTopLevelTasks.value.forEach(task => {
      const ids = getTaskAssigneeIds(task)
      if (!ids.length) {
        counts.set('unassigned', (counts.get('unassigned') || 0) + 1)
        return
      }

      ids.forEach(id => counts.set(id, (counts.get(id) || 0) + 1))
    })

    return Array.from(counts.entries())
      .map(([id, count]) => {
        const member = projectMembers.value.find(item => (item.userId || item.id) === id)
        return {
          label: id === 'unassigned' ? tr('Unassigned', 'Chưa giao') : (member?.fullName || member?.name || member?.email || tr('Assignee', 'Người thực hiện')),
          count,
          color: id === 'unassigned' ? 'var(--color-text-muted)' : '#38BDF8'
        }
      })
      .sort((a, b) => b.count - a.count || a.label.localeCompare(b.label))
  }

  if (analyticsInsightMode.value === 'status') {
    return taskStatusOptions.value.map(option => ({
      label: option.label,
      count: visibleTopLevelTasks.value.filter(task => normalizeStatus(task.statusName) === option.name).length,
      color: option.color
    }))
  }

  return [
    { label: tr('Urgent', 'Khẩn cấp'), count: visibleTopLevelTasks.value.filter(task => task.priority === 1).length, color: '#EF4444' },
    { label: tr('High', 'Cao'), count: visibleTopLevelTasks.value.filter(task => task.priority === 2).length, color: '#F97316' },
    { label: tr('Medium', 'Trung bình'), count: visibleTopLevelTasks.value.filter(task => task.priority === 3).length, color: '#3B82F6' },
    { label: tr('Low', 'Thấp'), count: visibleTopLevelTasks.value.filter(task => task.priority === 4).length, color: '#10B981' },
    { label: tr('None', 'Không có'), count: visibleTopLevelTasks.value.filter(task => !task.priority).length, color: 'var(--color-text-muted)' }
  ]
})

const assigneeAnalyticsRows = computed(() => {
  const rows = new Map()

  visibleTopLevelTasks.value.forEach(task => {
    const ids = getTaskAssigneeIds(task)
    const bucket = analyticsStatusBucket(task.statusName)
    const targets = ids.length ? ids : ['unassigned']

    targets.forEach(id => {
      if (!rows.has(id)) {
        const member = projectMembers.value.find(item => (item.userId || item.id) === id)
        rows.set(id, {
          id,
          label: id === 'unassigned' ? tr('Unassigned', 'Chưa giao') : (member?.fullName || member?.name || member?.email || tr('Assignee', 'Người thực hiện')),
          backlog: 0,
          started: 0,
          unstarted: 0,
          completed: 0,
          cancelled: 0,
          total: 0
        })
      }

      const row = rows.get(id)
      row[bucket] += 1
      row.total += 1
    })
  })

  return Array.from(rows.values()).sort((a, b) => b.total - a.total || a.label.localeCompare(b.label))
})

const analyticsInsightLabel = computed(() => {
  if (analyticsInsightMode.value === 'status') return tr('Status Distribution', 'Phân bổ trạng thái')
  if (analyticsInsightMode.value === 'assignee') return tr('Assignee Distribution', 'Phân bổ người thực hiện')
  return tr('Priority Distribution', 'Phân bổ độ ưu tiên')
})
const analyticsTableHeading = computed(() => {
  if (analyticsInsightMode.value === 'status') return tr('Status', 'Trạng thái')
  if (analyticsInsightMode.value === 'assignee') return tr('Assignee', 'Người thực hiện')
  return tr('Priority', 'Độ ưu tiên')
})
const setAnalyticsInsightMode = (mode) => {
  analyticsInsightMode.value = mode
}

const insightChartOptions = computed(() => ({
  tooltip: { trigger: 'axis' },
  grid: { left: '3%', right: '4%', bottom: '3%', containLabel: true },
  xAxis: {
    type: 'category',
    data: analyticsBreakdownRows.value.map(item => item.label),
    axisLine: { lineStyle: { color: '#3F3F46' } },
    axisLabel: { color: 'var(--color-text-muted)' }
  },
  yAxis: {
    type: 'value',
    splitLine: { lineStyle: { color: 'var(--color-border)' } },
    axisLabel: { color: 'var(--color-text-muted)' }
  },
  series: [
    {
      type: 'bar',
      barWidth: '30%',
      data: analyticsBreakdownRows.value.map(item => ({
        value: item.count,
        itemStyle: { color: item.color, borderRadius: [4, 4, 0, 0] }
      }))
    }
  ],
  backgroundColor: 'transparent'
}))

const kanbanColumns = computed(() => {
  const groups = taskStatusOptions.value.map((status, index) => ({
    id: `${status.name.toLowerCase().replace(/\s+/g, '-')}-${index}`,
    name: status.name,
    color: status.color,
    icon: status.icon,
    priorityValue: null,
    items: []
  }));

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
       col = groups.find(group => group.name === normalizeStatus(s)) || groups[0];
       
       col.items.push(t);
     });
     return groups;
  }
});

const listViewGroups = computed(() => {
  const groups = taskStatusOptions.value.map((status, index) => ({
    id: `${status.name.toLowerCase().replace(/\s+/g, '-')}-${index}`,
    name: status.label,
    statusName: status.name,
    icon: status.icon,
    color: status.color,
    items: []
  }))

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
    : Array.from(new Set([...currentIds, memberId]))

  task.assigneeIds = nextIds
  task.assignedUserId = nextIds[0] || null
  updateTask(task, 'assigneeIds', nextIds, currentIds)
}

const loadInitialData = async (options = {}) => {
  const { preserveExisting = false } = options
  let pid = getProjectId()
  if(!pid) {
      try {
          const res = await axiosClient.get('/projects');
          if (res.data?.data?.length > 0) {
              pid = res.data.data[0].id;
              dynamicProjectId = pid;
              setScopedCurrentProjectId(pid);
          }
      } catch (err) {
          console.error('Cannot resolve valid projectId', err);
          return;
      }
  }

  try {
    setScopedCurrentProjectId(pid)
    showSubtasks.value = loadShowSubtasksPreference(pid)
    if (!preserveExisting) {
      rawTasks.value = []
      allTasks.value = []
      selectedTask.value = null
      projectMembers.value = []
      project.value = {}
      carryOverTaskIds.value = []
    }
    const [pRes, mRes, statusesRes, executionRulesRes] = await Promise.all([
      axiosClient.get(`/projects/${pid}`),
      axiosClient.get(`/projects/${pid}/members`),
      axiosClient.get(`/projects/${pid}/task-statuses`).catch(() => ({ data: { data: [] } })),
      axiosClient.get(`/projects/${pid}/execution-rules`).catch(() => ({ data: { data: {} } }))
    ])
    project.value = pRes.data.data
    projectMembers.value = (mRes.data.data || []).map(member => ({
      ...member,
      userId: member.userId || member.id,
      fullName: member.fullName || member.name || member.email,
      projectRole: member.projectRole || member.ProjectRole || member.myRole || member.MyRole || ''
    }))
    projectStatuses.value = (statusesRes.data?.data || []).map((status) => ({
      ...status,
      name: normalizeStatus(status.name),
      displayName: status.displayName || status.name,
      colorCode: status.colorCode || ''
    }))
    projectExecutionRules.value = {
      enableRoleBasedTaskVisibility: Boolean(executionRulesRes.data?.data?.enableRoleBasedTaskVisibility),
      managerAlwaysSeeAllTasks: executionRulesRes.data?.data?.managerAlwaysSeeAllTasks !== false
    }

    if (activeCarryOverSprintId.value) {
      const carryOverRes = await axiosClient.get(`/projects/${pid}/sprints/${activeCarryOverSprintId.value}/carry-over-tasks`)
      carryOverTaskIds.value = (carryOverRes.data?.data || []).map(task => task.id)
    }

    await fetchTasks({ reset: false })
  } catch (error) {
    if (isForbiddenError(error)) {
      isForbidden.value = true
    } else {
      console.error('Lỗi load dự án:', error)
    }
  }
}

const fetchTasks = async (options = {}) => {
  const pid = getProjectId()
  if(!pid) return
  try {
      const tasks = await store.fetchTasks(pid, options);
      allTasks.value = Array.isArray(tasks) ? tasks : []
      
      // Auto update selectedTask if open
      if (selectedTask.value) {
        const updatedTask = allTasks.value.find(t => t.id === selectedTask.value.id);
        if (updatedTask && canCurrentUserSeeTask(updatedTask)) selectedTask.value = updatedTask;
        else if (!updatedTask || !canCurrentUserSeeTask(selectedTask.value)) selectedTask.value = null;
      }
  } catch(error) {
    console.error('Lỗi load tasks:', error)
  }
}

const logViewedTask = (task) => {
  if (!task || !task.id) return
  let viewed = JSON.parse(localStorage.getItem('recently_viewed_tasks') || '[]')
  viewed = viewed.filter(item => item.id !== task.id)
  viewed.unshift({
    id: task.id,
    title: task.title,
    sequenceId: task.sequenceId,
    projectId: task.projectId || getProjectId(),
    projectName: project.value?.name || 'Project',
    projectColor: project.value?.cover || '#3b82f6',
    updatedAt: new Date().toISOString(),
    statusName: task.statusName,
    priority: task.priority
  })
  viewed = viewed.slice(0, 15)
  localStorage.setItem('recently_viewed_tasks', JSON.stringify(viewed))
}

const openTaskDetail = (task) => {
  logViewedTask(task)
  taskDetailHistory.value = []
  selectedTask.value = task;
}
const openTaskDetailFromModal = (task, options = {}) => {
  logViewedTask(task)
  const previousTask = options?.fromTask || selectedTask.value
  if (previousTask?.id && previousTask.id !== task?.id) {
    const cachedPrevious = allTasks.value.find(item => item.id === previousTask.id) || previousTask
    taskDetailHistory.value = [...taskDetailHistory.value, cachedPrevious]
  }
  selectedTask.value = task
}
const goBackTaskDetail = () => {
  const history = [...taskDetailHistory.value]
  const previousTask = history.pop()
  if (!previousTask) return
  taskDetailHistory.value = history
  selectedTask.value = allTasks.value.find(item => item.id === previousTask.id) || previousTask
}
const closeTaskDetail = () => {
  taskDetailHistory.value = []
  selectedTask.value = null;
}

const putBackedTaskFields = new Set([
  'title',
  'description',
  'priority',
  'assignedUserId',
  'sprintId',
  'taskTypeId',
  'totalEstimatedHours',
  'visibilityMode',
  'visibleToRoles'
])

const buildPutTaskPayload = (task, overrides = {}) => {
  const mergedTask = { ...task, ...overrides }

  return {
    title: mergedTask.title || '',
    description: mergedTask.description ?? '',
    priority: mergedTask.priority ?? 0,
    storyPoints: mergedTask.storyPoints ?? 0,
    assignedUserId: mergedTask.assignedUserId ?? mergedTask.assigneeId ?? null,
    plannedStartDate: normalizeDateOnly(mergedTask.plannedStartDate) || null,
    plannedEndDate: normalizeDateOnly(mergedTask.plannedEndDate) || null,
    dueDate: normalizeDateOnly(mergedTask.dueDate) || null,
    sprintId: mergedTask.sprintId || null,
    taskTypeId: mergedTask.taskTypeId || '00000000-0000-0000-0000-000000000000',
    totalEstimatedHours: Number(mergedTask.totalEstimatedHours || 0),
    visibilityMode: mergedTask.visibilityMode || 'project',
    visibleToRoles: Array.isArray(mergedTask.visibleToRoles) ? mergedTask.visibleToRoles : [],
    rowVersion: mergedTask.rowVersion || null
  }
}

const syncTopLevelTasks = () => {
  rawTasks.value = (allTasks.value || []).filter(task => !isSubtask(task))
}

watch(allTasks, syncTopLevelTasks, { deep: true, immediate: true })

watch(
  () => ({
    enableRoleBasedTaskVisibility: Boolean(projectExecutionRules.value?.enableRoleBasedTaskVisibility),
    managerAlwaysSeeAllTasks: Boolean(projectExecutionRules.value?.managerAlwaysSeeAllTasks)
  }),
  async (rules) => {
    allTasks.value = (allTasks.value || []).filter(canCurrentUserSeeTask)

    if (selectedTask.value && !canCurrentUserSeeTask(selectedTask.value)) {
      selectedTask.value = null
    }

    await fetchTasks({ reset: false })
  },
  { deep: true }
)

const updateTask = async (task, field, value, previousValue = task ? task[field] : undefined) => {
  const pid = getProjectId()
  if (!pid || !task?.id) return

  const isBatchPayload = field && typeof field === 'object' && !Array.isArray(field)
  const payloadOverrides = isBatchPayload ? field : { [field]: value }
  const previousValues = Object.fromEntries(
    Object.keys(payloadOverrides).map(key => [key, task?.[key]])
  )

  try {
    Object.entries(payloadOverrides).forEach(([key, nextValue]) => {
      task[key] = nextValue
    })

    const usesPutUpdate = !isBatchPayload && putBackedTaskFields.has(field)
    const payload = usesPutUpdate
      ? buildPutTaskPayload(task, payloadOverrides)
      : payloadOverrides

    await store.updateTask(pid, task.id, payload, { method: usesPutUpdate ? 'put' : 'patch' })
    await fetchTasks()
  } catch (error) {
    console.error('Failed to update task:', error)
    if (task) {
      Object.entries(previousValues).forEach(([key, rollbackValue]) => {
        task[key] = rollbackValue
      })
    }
    ElMessage.error(error.response?.data?.message || 'Khong the cap nhat cong viec')
    await fetchTasks()
  }
}

const openCreateTask = (statusName, defaults = {}) => {
   taskDetailHistory.value = []
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

const toggleAnalyticsExpand = () => {
  isAnalyticsExpanded.value = !isAnalyticsExpanded.value
}

const closeAnalyticsSidebar = () => {
  showAnalyticsSidebar.value = false
  isAnalyticsExpanded.value = false
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

const exportAnalyticsCsv = (mode = analyticsInsightMode.value) => {
  const rows = mode === 'assignee'
    ? [
        ['Người thực hiện', 'Chờ xử lý', 'Đang làm', 'Đang đánh giá', 'Hoàn thành', 'Đã hủy', 'Tổng'],
        ...assigneeAnalyticsRows.value.map(item => [item.label, item.backlog, item.started, item.unstarted, item.completed, item.cancelled, item.total])
      ]
    : [
        [analyticsTableHeading.value, 'Số lượng'],
        ...analyticsBreakdownRows.value.map(item => [item.label, item.count])
      ]
  const csv = rows.map(row => row.join(',')).join('\n')
  const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' })
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = url
  link.download = `${mode}-analytics.csv`
  link.click()
  URL.revokeObjectURL(url)
}

onMounted(() => {
  hydrateFiltersFromUrl()
  loadInitialData()
  window.addEventListener('global-create-task', handleGlobalCreate)
})

let unsubscribeAdminRealtime = null
let signalRTaskUpdatedHandler = null
let signalRProjectEventHandler = null
let realtimeRefreshTimer = null

const handleRealtimeTaskUpdated = (task) => {
  if (!task?.id) return
  const normalizedTask = store.normalizeTaskRecord(task, getProjectId())
  if (!canCurrentUserSeeTask(normalizedTask)) {
    allTasks.value = allTasks.value.filter(item => item.id !== normalizedTask.id)
    if (selectedTask.value?.id === normalizedTask.id) {
      selectedTask.value = null
    }
    clearTimeout(realtimeRefreshTimer)
    realtimeRefreshTimer = setTimeout(() => {
      fetchTasks({ reset: false })
    }, 120)
    return
  }

  const index = allTasks.value.findIndex(item => item.id === normalizedTask.id)
  if (index >= 0) {
    allTasks.value[index] = { ...allTasks.value[index], ...normalizedTask }
  } else {
    allTasks.value = [...allTasks.value, normalizedTask]
  }

  if (selectedTask.value?.id === normalizedTask.id) {
    if (canCurrentUserSeeTask(normalizedTask)) {
      selectedTask.value = { ...selectedTask.value, ...normalizedTask }
    } else {
      selectedTask.value = null
    }
  }

  clearTimeout(realtimeRefreshTimer)
  realtimeRefreshTimer = setTimeout(() => {
    fetchTasks({ reset: false })
  }, 120)
}

const startTaskRealtime = async (projectId) => {
  if (!projectId) return
  if (signalRTaskUpdatedHandler) {
    signalRService.off('TaskUpdated', signalRTaskUpdatedHandler)
    signalRService.off('WorkTaskUpdated', signalRTaskUpdatedHandler)
  }
  if (signalRProjectEventHandler) {
    signalRService.off('ProjectRealtimeEvent', signalRProjectEventHandler)
  }

  await signalRService.startConnection(projectId)
  signalRTaskUpdatedHandler = handleRealtimeTaskUpdated
  signalRService.on('TaskUpdated', signalRTaskUpdatedHandler)
  signalRService.on('WorkTaskUpdated', signalRTaskUpdatedHandler)
  signalRProjectEventHandler = (event) => {
    if (!event?.type) return
    if (event?.projectId && `${event.projectId}` !== `${projectId}`) return
    broadcastAdminRealtime(event.type, event.payload || { projectId })
  }
  signalRService.on('ProjectRealtimeEvent', signalRProjectEventHandler)
}

onMounted(() => {
  startTaskRealtime(getProjectId())
  unsubscribeAdminRealtime = subscribeAdminRealtime(async ({ type, payload }) => {
    const pid = getProjectId()
    if (!pid) return
    if (payload?.projectId && `${payload.projectId}` !== `${pid}`) return

    if (
      [
        'project-settings-updated',
        'project-settings-favorite-updated',
        'project-settings-integrations-updated',
        'project-administration-updated'
      ].includes(type)
    ) {
      await loadInitialData({ preserveExisting: false })
    }
  })
})

watch(currentProjectId, (projectId, previousProjectId) => {
  if (!projectId || projectId === previousProjectId) {
    return
  }

  dynamicProjectId = projectId
  showAnalyticsSidebar.value = false
  isAnalyticsExpanded.value = false
  startTaskRealtime(projectId)
  loadInitialData()
}, { immediate: false })

watch(showSubtasks, (value) => {
  persistShowSubtasksPreference(value)
})

watch(
  () => [route.query.tab, route.query.sprintId, route.query.moduleId, route.params.cycleId, route.query.carryOverFromSprintId],
  () => {
    if (route.query.tab === 'spreadsheet' || activeSprintFilterId.value || activeModuleFilterId.value || activeCarryOverSprintId.value) {
      currentTab.value = 'spreadsheet'
    } else if (route.query.tab === 'board') {
      currentTab.value = 'board'
    }
  },
  { immediate: true }
)

watch(
  () => route.query.carryOverFromSprintId,
  () => {
    loadInitialData()
  }
)

onUnmounted(() => {
  window.removeEventListener('global-create-task', handleGlobalCreate)
  clearTimeout(realtimeRefreshTimer)
  if (signalRTaskUpdatedHandler) {
    signalRService.off('TaskUpdated', signalRTaskUpdatedHandler)
    signalRService.off('WorkTaskUpdated', signalRTaskUpdatedHandler)
  }
  if (signalRProjectEventHandler) {
    signalRService.off('ProjectRealtimeEvent', signalRProjectEventHandler)
  }
  unsubscribeAdminRealtime?.()
})
</script>

<style scoped>
/* ==================================
   PLANE.SO PROJECT KANBAN THEME
   ================================== */
.plane-board-container {
  background-color: var(--color-bg); 
  height: 100vh;
  display: flex;
  flex-direction: column;
  color: var(--color-text-primary);
  font-family: 'Inter', sans-serif;
  overflow: hidden;
}

.timeline-wrapper {
  flex: 1;
  min-height: 0;
  overflow: auto;
}

/* ── PLANE HEADER ── */
.plane-space-header {
  height: 52px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0 24px;
  border-bottom: 1px solid var(--color-border);
  flex-shrink: 0;
  background-color: var(--color-bg);
}

.breadcrumb {
  display: flex;
  align-items: center;
  gap: 10px;
  font-size: 14px;
  color: var(--color-text-muted);
}
.proj-icon {
  background: var(--color-accent);
  color: var(--color-text-primary);
  width: 18px;
  height: 18px;
  border-radius: 2px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 10px;
  font-weight: bold;
}
.proj-name {
  color: var(--color-text-primary);
  font-weight: 700;
  cursor: pointer;
}
.proj-name:hover { color: var(--color-accent); }
.separator {
  font-size: 10px;
  color: var(--color-text-muted);
}
.active-page {
  color: var(--color-text-primary);
  display: flex;
  align-items: center;
  gap: 6px;
  font-weight: 500;
}
.active-page i { color: var(--color-text-muted); }
.item-count {
  background: var(--color-border);
  color: #38BDF8;
  padding: 2px 8px;
  border-radius: 2px;
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
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 2px;
  padding: 2px;
  margin-right: 8px;
}
.toggle-btn {
  background: transparent;
  border: none;
  color: var(--color-text-muted);
  width: 28px;
  height: 28px;
  border-radius: 2px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all 0.2s;
}
.toggle-btn:hover { color: var(--color-text-primary); }
.toggle-btn.active {
  background: var(--color-border);
  color: var(--color-text-primary);
}

.plane-toolbar-btn {
  background: transparent;
  border: none;
  color: var(--color-text-secondary);
  font-size: 13px;
  font-weight: 500;
  cursor: pointer;
  padding: 6px 12px;
  border-radius: 2px;
  transition: background 0.2s;
  display: flex;
  align-items: center;
}
.plane-toolbar-btn:hover {
  background: var(--color-border);
}
.plane-toolbar-btn.active {
  background: var(--color-border);
  color: var(--color-text-primary);
}
.filter-count {
  margin-left: 6px;
  min-width: 16px;
  height: 16px;
  border-radius: 999px;
  background: #0EA5E9;
  color: var(--color-text-primary);
  font-size: 10px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
}
.work-filter-row {
  padding: 10px 24px;
  border-bottom: 1px solid var(--color-border);
  background: var(--color-bg);
  flex-shrink: 0;
}

.plane-primary-btn {
  background: #0EA5E9;
  color: var(--color-text-primary);
  border: none;
  border-radius: 2px;
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
  color: var(--color-text-primary);
}

.col-count {
  background: var(--color-surface-hover);
  color: var(--color-text-muted);
  padding: 2px 8px;
  border-radius: 2px;
  font-size: 12px;
}

.add-btn {
  color: var(--color-text-muted);
  cursor: pointer;
  font-size: 14px;
}
.add-btn:hover { color: var(--color-text-primary); }

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
  height: 230px;
}

.col-draggable {
  display: flex;
  flex-direction: column;
  gap: 12px;
  min-height: 10px;
}

.issue-card {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 2px;
  padding: 16px;
  cursor: pointer;
  transition: all 0.2s;
}
.issue-card:hover {
  border-color: var(--color-border);
}
.issue-card.active-card {
  border-color: var(--color-border);
}

.issue-sequence { font-size: 11px; color: var(--color-text-muted); margin: 0; }
.issue-title {
  margin: 0;
  font-size: 13px;
  font-weight: 500;
  color: var(--color-text-primary);
  line-height: 1.5;
}

.issue-meta {
  display: flex;
  align-items: center;
  gap: 12px;
}

.id { font-size: 12px; color: var(--color-text-muted); font-weight: 600; }
.ms-auto { margin-left: auto; }

.avatar-xs {
  width: 20px;
  height: 20px;
  border-radius: 50%;
  background-color: var(--color-surface-hover);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 10px;
  font-weight: 600;
  color: var(--color-text-secondary);
  border: 1px solid var(--color-border);
}

/* Colors for priority icons */
.text-muted { color: var(--color-text-muted); }
.text-blue { color: #3B82F6; }
.text-orange { color: #F59E0B; }
.text-red { color: #EF4444; }
.text-green { color: #10B981; }

.badge { border: 1px solid var(--color-border); border-radius: 2px; padding: 2px 6px; font-size: 11px; color: var(--color-text-muted); display: flex; align-items: center; gap: 6px; }

.add-btn-bottom { 
  color: var(--color-text-primary); 
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
  background-color: var(--color-bg);
  box-shadow: 0 -4px 10px rgba(13, 15, 17, 0.8);
  border-radius: 2px;
}
.add-btn-bottom:hover { color: var(--color-text-primary); background-color: var(--color-border); }

.inline-create-box { 
  background: var(--color-surface); 
  border: 1px solid var(--color-border); 
  border-radius: 2px; 
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
  color: var(--color-text-primary);
  font-size: 16px;
}
.ic-input { 
  width: 100%; 
  background: transparent; 
  border: none; 
  color: var(--color-text-primary); 
  outline: none; 
  font-size: 14px; 
  font-weight: 500;
  padding: 0;
}
.ic-input::placeholder { color: var(--color-text-muted); }

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
  border: 1px solid var(--color-border);
  border-radius: 2px;
  padding: 4px 8px;
  font-size: 11px;
  color: var(--color-text-muted);
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
.kanban-wrapper::-webkit-scrollbar-thumb, .col-body::-webkit-scrollbar-thumb { background: var(--color-border); border-radius: 2px; }
.kanban-wrapper::-webkit-scrollbar-thumb:hover, .col-body::-webkit-scrollbar-thumb:hover { background: #3F3F46; }

/* Display Dropdown Styles */
.display-dropdown-wrapper { position: relative; display: inline-block; }
.plane-dropdown-menu {
  position: absolute;
  top: 100%;
  left: 0;
  margin-top: 8px;
  background: var(--color-border);
  border: 1px solid #333;
  border-radius: 2px;
  width: 260px;
  box-shadow: 0 10px 30px rgba(0,0,0,0.5);
  z-index: 1000;
  color: var(--color-text-primary);
  font-size: 13px;
  padding: 8px 0;
}
.dd-section { padding: 8px 16px; }
.dd-section.border-top { border-top: 1px solid var(--color-border); }
.dd-title { display: flex; justify-content: space-between; color: var(--color-text-muted); font-size: 12px; font-weight: 500; margin-bottom: 8px; }
.dd-btns { display: flex; gap: 8px; flex-wrap: wrap; }
.dd-tag { background: var(--color-surface); border: 1px solid var(--color-border); color: var(--color-text-primary); border-radius: 2px; padding: 4px 8px; font-size: 12px; cursor: pointer; }
.dd-tag.active { background: #0EA5E9; color: var(--color-text-primary); border-color: #0EA5E9; }
.dd-list { display: flex; flex-direction: column; gap: 8px; }
.dd-item { display: flex; align-items: center; gap: 8px; cursor: pointer; }
.dd-item input[type="radio"], .dd-item input[type="checkbox"] { accent-color: #0EA5E9; cursor: pointer; }

.plane-list-view {
  display: flex;
  flex-direction: column;
  color: var(--color-text-primary);
}

.list-wrapper {
  flex: 1;
  min-height: 0;
  overflow-y: auto;
  overflow-x: hidden;
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
  border-bottom: 1px solid var(--color-border);
  margin-bottom: 8px;
}

.group-header:hover .add-icon {
  opacity: 1;
}

.gh-left,
.gh-right,
.pill-group {
  display: flex;
  align-items: center;
}

.group-content {
  display: flex;
  flex-direction: column;
  align-items: stretch;
}

.gh-left {
  gap: 10px;
}

.gh-chevron {
  font-size: 10px;
  color: var(--color-text-muted);
  width: 14px;
  text-align: center;
}

.group-name {
  font-size: 14px;
  font-weight: 600;
  color: var(--color-text-primary);
}

.group-count {
  font-size: 12px;
  font-weight: 500;
  color: var(--color-text-muted);
  margin-left: 4px;
}

.add-icon {
  color: var(--color-text-muted);
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
  border-bottom: 1px solid var(--color-border);
  cursor: pointer;
}

.task-row:hover {
  background-color: var(--color-surface);
}

.subtask-row {
  margin-left: 28px;
  border-left: 1px dashed var(--color-border);
  background: rgba(22, 24, 29, 0.55);
}

.subtask-row:hover {
  background: rgba(30, 32, 37, 0.92);
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

.subtask-indent {
  width: 18px;
  color: var(--color-text-muted);
  display: inline-flex;
  justify-content: center;
  align-items: center;
  flex-shrink: 0;
}

.tr-right {
  justify-content: flex-end;
}

.task-id {
  font-size: 12px;
  color: var(--color-text-muted);
  font-weight: 600;
  min-width: 86px;
}

.task-title {
  color: var(--color-text-primary);
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
  border: 1px solid var(--color-border);
  border-radius: 999px;
  padding: 5px 10px;
  font-size: 12px;
  color: var(--color-text-secondary);
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
  background: var(--color-border);
  color: var(--color-text-primary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-size: 10px;
  border: 1px solid var(--color-border);
}

.add-row-placeholder {
  color: var(--color-text-muted);
  font-size: 13px;
  padding: 10px 0 10px 24px;
  cursor: pointer;
}

.add-row-placeholder:hover {
  color: var(--color-text-primary);
  background: var(--color-surface);
}

.plane-dropdown {
  background: var(--bg-secondary) !important;
  border: 1px solid var(--border-color) !important;
}

:global(.plane-popover) {
  background: var(--bg-secondary) !important;
  border: 1px solid var(--border-color) !important;
  padding: 12px !important;
  box-shadow: var(--shadow-lg) !important;
  border-radius: var(--radius-input) !important;
  color: var(--text-primary) !important;
}

.no-shadow-context :global(.plane-popover) {
  box-shadow: none !important;
}

:global(.plane-popover .el-popper__arrow::before) {
  background: var(--bg-secondary) !important;
  border: 1px solid var(--border-color) !important;
}


.plane-search-input {
  width: 100%;
  background: var(--bg-tertiary);
  border: 1px solid var(--border-color);
  color: var(--text-primary);
  border-radius: var(--radius-small);
  padding: 8px 12px;
  outline: none;
  font-size: 13px;
  transition: all 0.2s;
}

.plane-search-input:focus {
  border-color: var(--color-accent);
}

.plane-search-input::placeholder {
  color: var(--color-text-muted);
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
  gap: 10px;
  color: var(--text-primary);
  cursor: pointer;
  padding: 8px 10px;
  border-radius: var(--radius-small);
  transition: all 0.2s;
  font-size: 13px;
}


.plane-list-item:hover {
  background: var(--hover-bg);
}

.plane-list-item input[type="checkbox"] {
  accent-color: var(--color-accent);
  width: 14px;
  height: 14px;
  cursor: pointer;
}

.star-task-btn {
  background: transparent;
  border: none;
  cursor: pointer;
  padding: 4px;
  color: var(--color-text-muted);
  border-radius: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s;
}
.star-task-btn:hover {
  background: var(--color-surface-hover);
}
.star-task-btn.small {
  padding: 2px;
  font-size: 12px;
}


/* Analytics Sidebar */
.forbidden-overlay { display: flex; align-items: center; justify-content: center; height: 100%; width: 100%; background: var(--color-bg); }
.forbidden-content { text-align: center; max-width: 400px; padding: 40px; background: var(--color-surface); border: 1px solid var(--color-border); border-radius: 16px; }
.forbidden-content .lock-icon { font-size: 48px; color: #ef4444; margin-bottom: 24px; }
.forbidden-content h2 { margin: 0 0 12px 0; font-size: 20px; color: var(--color-text-primary); }
.forbidden-content p { margin: 0 0 24px 0; color: var(--color-text-secondary); line-height: 1.5; }
.forbidden-content .mt-4 { margin-top: 16px; }

.analytics-overlay {
  position: fixed;
  top: 0; left: 0; right: 0; bottom: 0;
  background: rgba(2, 6, 23, 0.52);
  z-index: 9999;
  display: flex;
  justify-content: flex-end;
  backdrop-filter: blur(2px);
}
.analytics-panel {
  width: min(860px, 92vw);
  max-width: 92vw;
  background:
    linear-gradient(180deg, rgba(14, 165, 233, 0.10), transparent 280px),
    color-mix(in srgb, var(--color-bg) 88%, #0f172a 12%);
  height: 100%;
  box-shadow: -24px 0 64px rgba(0, 0, 0, 0.36) !important;
  display: flex;
  flex-direction: column;
  transform: translateX(100%);
  transition: transform 0.3s cubic-bezier(0.16, 1, 0.3, 1);
  border-left: 1px solid var(--color-border);
}
.analytics-panel.slide-in { transform: translateX(0); }
.analytics-panel.is-expanded {
  width: 100vw;
  max-width: 100vw;
}
.ap-header {
  padding: 20px 24px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-bottom: 1px solid var(--color-border);
  background:
    linear-gradient(90deg, rgba(56, 189, 248, 0.18), transparent 56%),
    color-mix(in srgb, var(--color-surface) 82%, transparent);
}
.ap-header h3 { margin: 0; font-size: 18px; font-weight: 800; color: var(--color-text-primary); letter-spacing: 0; }
.ap-actions { display: flex; gap: 12px; }
.icon-btn { background: transparent; border: none; color: var(--color-text-muted); font-size: 14px; cursor: pointer; }
.icon-btn:hover { color: var(--color-text-primary); }

.ap-body {
  padding: 22px 24px 30px;
  overflow-y: auto;
  flex: 1;
}

/* Stats Grid */
.ap-stats-grid {
  display: grid;
  grid-template-columns: repeat(5, minmax(0, 1fr));
  gap: 10px;
}
.stat-box {
  position: relative;
  overflow: hidden;
  display: flex;
  flex-direction: column;
  gap: 8px;
  min-width: 0;
  padding: 14px;
  border: 1px solid rgba(148, 163, 184, 0.18);
  border-radius: 8px;
  background: linear-gradient(180deg, rgba(255,255,255,0.035), rgba(255,255,255,0.01));
}
.stat-box::before {
  content: "";
  position: absolute;
  inset: 0 auto 0 0;
  width: 3px;
  background: var(--stat-accent, #38bdf8);
}
.stat-box:nth-child(1) { --stat-accent: #38bdf8; }
.stat-box:nth-child(2) { --stat-accent: #3b82f6; }
.stat-box:nth-child(3) { --stat-accent: #94a3b8; }
.stat-box:nth-child(4) { --stat-accent: #f59e0b; }
.stat-box:nth-child(5) { --stat-accent: #22c55e; }
.stat-box:hover {
  border-color: color-mix(in srgb, var(--stat-accent) 56%, var(--color-border));
  background: color-mix(in srgb, var(--color-surface) 86%, var(--stat-accent) 14%);
}
.stat-box .lbl { color: var(--color-text-muted); font-size: 11px; font-weight: 650; line-height: 1.35; }
.stat-box .val { color: var(--color-text-primary); font-size: 24px; font-weight: 850; line-height: 1; }

.ap-chart-card {
  margin-top: 16px;
  padding: 16px;
  border: 1px solid rgba(148, 163, 184, 0.18);
  border-radius: 10px;
  background:
    radial-gradient(circle at top right, rgba(56, 189, 248, 0.12), transparent 34%),
    color-mix(in srgb, var(--color-surface) 78%, transparent);
}
.ap-chart-card h4 { margin: 0; font-size: 14px; font-weight: 800; color: var(--color-text-primary); }
.chart-container { height: 260px; }

.line-chart-mock {
  position: relative;
  height: 200px;
  margin-top: 16px;
  border-bottom: 1px solid var(--color-border);
}
.grid-l {
  position: absolute;
  width: 100%;
  border-top: 1px solid var(--color-border);
}
.grid-l span {
  position: absolute;
  left: -20px;
  top: -8px;
  font-size: 10px;
  color: var(--color-text-muted);
}
.dot { position: absolute; width: 6px; height: 6px; border-radius: 50%; transform: translate(-50%, 50%); border: 2px solid; background: var(--color-surface); }
.dot.blue { border-color: #0EA5E9; z-index: 2; }
.dot.green { border-color: #10B981; z-index: 1; }
.x-label { position: absolute; bottom: -20px; font-size: 11px; color: var(--color-text-muted); }

.chart-legend { display: flex; gap: 16px; font-size: 12px; color: var(--color-text-primary); margin-top: 24px; }
.leg-item { display: flex; align-items: center; gap: 8px; font-weight: 500; }
.box { width: 8px; height: 8px; border-radius: 2px; }
.bg-green { background: #10B981; }
.bg-blue { background: #0EA5E9; }

.insight-filters { display: flex; gap: 8px; }

.bar-chart-mock {
  position: relative;
  height: 250px;
  margin-top: 24px;
  border-bottom: 1px solid var(--color-border);
}
.bars-container {
  display: flex;
  justify-content: space-around;
  align-items: flex-end;
  height: 100%;
  padding-bottom: 1px; /* Avoid overlapping border */
}
.bar-wrapper { display: flex; flex-direction: column; align-items: center; gap: 8px; height: 100%; justify-content: flex-end; width: 40px; }
.bar { width: 100%; border-radius: 2px 4px 0 0; }
.bar-lbl { position: absolute; bottom: -24px; font-size: 12px; color: var(--color-text-muted); }
.bg-orange { background: #F97316; }
.bg-gray { background: #D4D4D8; }
.bg-red { background: #EF4444; }

.y-label {
  position: absolute;
  left: -40px;
  top: 50%;
  transform: rotate(-90deg) translateY(-50%);
  font-size: 10px;
  color: var(--color-text-muted);
  letter-spacing: 1px;
}

.ap-table-wrap {
  margin-top: 16px;
  padding: 16px;
  border: 1px solid rgba(148, 163, 184, 0.18);
  border-radius: 10px;
  background: color-mix(in srgb, var(--color-surface) 78%, transparent);
}
.table-head { display: flex; justify-content: space-between; align-items: center; margin-bottom: 12px; font-size: 13px; }
.flex-center { display: flex; align-items: center; }
.export-btn { background: transparent; border: 1px solid var(--color-border); color: var(--color-text-secondary); border-radius: 6px; padding: 5px 8px; font-size: 12px; cursor: pointer; }
.export-btn:hover { background: var(--color-bg-secondary); color: var(--color-text-primary); }

.ap-table { width: 100%; border-collapse: collapse; font-size: 13px; color: var(--color-text-primary); }
.ap-table th { color: var(--color-text-muted); font-weight: 650; border-bottom: 1px solid var(--color-border); padding: 10px 0; text-align: left; }
.ap-table td { padding: 11px 0; border-bottom: 1px solid color-mix(in srgb, var(--color-border) 70%, transparent); }
.ap-table tr:hover { background: color-mix(in srgb, var(--color-surface) 82%, transparent); }

@media (max-width: 920px) {
  .ap-stats-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}
</style>





