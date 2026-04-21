<template>
  <NexusLayout>
    <div class="drafts-wrapper">
      <!-- Header -->
      <header class="dr-header">
        <div class="dr-left">
          <i class="fa-solid fa-pen-nib text-muted"></i>
          <span class="dr-title">Drafts</span>
          <span class="dr-badge">{{ pagination.totalCount }}</span>
        </div>
        <div class="dr-right">
          <button class="plane-primary-btn" @click="openModal">Draft a work item</button>
        </div>
      </header>

      <!-- Body / List -->
      <div class="dr-body" ref="draftListContainer" @scroll="handleDraftListScroll">
         <div
           v-if="drafts.length > 0"
           class="drafts-virtual-spacer"
           :style="{ height: `${virtualContentHeight}px` }"
         >
           <div class="drafts-virtual-inner" :style="{ transform: `translateY(${virtualOffsetTop}px)` }">
         <div class="list-row" v-for="draft in visibleDrafts" :key="draft.id" @dblclick="openEditModal(draft)">
            <div class="lr-left">
               <span class="text-muted fw-500 text-xs" style="min-width: 60px;">{{ draft.sequenceId || 'DRAFT' }}</span>
               <span class="lr-title">{{ draft.title || 'Untitled Draft' }}</span>
            </div>
            <div class="lr-right" @dblclick.stop>
               <div class="dm-toolbar">
                 <!-- Status -->
                 <el-dropdown trigger="click" @command="(val) => updateDraftProperty(draft, 'statusName', val)">
                    <button class="dm-tool-btn"><i :class="getStatusIcon(draft.statusName)" :style="{ color: getStatusColor(draft.statusName) }"></i></button>
                    <template #dropdown>
                      <el-dropdown-menu class="plane-dropdown">
                        <el-dropdown-item command="BACKLOG"><i class="fa-regular fa-circle-dot" style="color:#71717A"></i> Backlog</el-dropdown-item>
                        <el-dropdown-item command="TO DO"><i class="fa-regular fa-circle" style="color:#D4D4D8"></i> To Do</el-dropdown-item>
                        <el-dropdown-item command="IN PROGRESS"><i class="fa-solid fa-circle-half-stroke" style="color:#3B82F6"></i> In Progress</el-dropdown-item>
                        <el-dropdown-item command="DONE"><i class="fa-solid fa-circle-check" style="color:#10B981"></i> Done</el-dropdown-item>
                      </el-dropdown-menu>
                    </template>
                 </el-dropdown>
                 
                 <!-- Priority -->
                 <el-dropdown trigger="click" @command="(val) => updateDraftProperty(draft, 'priority', val)">
                    <button class="dm-tool-btn">
                       <i :class="getPriorityIcon(draft.priority)" :style="{ color: getPriorityColor(draft.priority) }"></i>
                    </button>
                    <template #dropdown>
                      <el-dropdown-menu class="plane-dropdown">
                        <el-dropdown-item :command="1"><i class="fa-solid fa-angles-up" style="color:#EF4444"></i> Urgent</el-dropdown-item>
                        <el-dropdown-item :command="2"><i class="fa-solid fa-chevron-up" style="color:#F97316"></i> High</el-dropdown-item>
                        <el-dropdown-item :command="3"><i class="fa-solid fa-minus" style="color:#3B82F6"></i> Normal</el-dropdown-item>
                        <el-dropdown-item :command="4"><i class="fa-solid fa-chevron-down" style="color:#94A3B8"></i> Low</el-dropdown-item>
                      </el-dropdown-menu>
                    </template>
                 </el-dropdown>

                 <!-- Assignees List View -->
                 <el-popover placement="bottom" trigger="click" width="240" popper-class="plane-popover">
                    <template #reference>
                      <button class="dm-tool-btn" :class="{'text-primary': draft.assignee}" title="Assignees">
                         <i class="fa-solid fa-user-group" :class="draft.assignee ? '' : 'text-muted'"></i>
                         <span v-if="draft.assignee" style="margin-left: 4px;">{{ draft.assignee }}</span>
                      </button>
                    </template>
                    <div class="popover-content">
                       <input type="text" class="plane-search-input" v-model="searchAssignee" placeholder="Search members">
                       <div class="plane-list mt-2">
                          <label class="plane-list-item" v-for="m in filteredMembers" :key="m.id" @click="updateDraftProperty(draft, 'assignee', m.name); closeAllPopovers()">
                             {{ m.name }}
                          </label>
                       </div>
                    </div>
                 </el-popover>

                 <!-- Labels List View -->
                 <el-popover placement="bottom" trigger="click" width="240" popper-class="plane-popover">
                    <template #reference>
                      <button class="dm-tool-btn" :class="{'text-primary': draft.label}" title="Labels">
                         <i class="fa-solid fa-tag" :class="draft.label ? '' : 'text-muted'"></i>
                         <span v-if="draft.label" style="margin-left: 4px;">{{ draft.label }}</span>
                      </button>
                    </template>
                    <div class="popover-content">
                       <input type="text" class="plane-search-input" v-model="searchLabel" placeholder="Search labels">
                       <div class="plane-list mt-2">
                          <label class="plane-list-item" v-for="l in filteredLabels" :key="l.id" @click="updateDraftProperty(draft, 'label', l.name); closeAllPopovers()">
                             {{ l.name }}
                          </label>
                       </div>
                    </div>
                 </el-popover>

                 <!-- Dates List View -->
                 <!-- Start Date -->
                 <div style="position: relative; display: inline-flex;">
                    <button class="dm-tool-btn" :class="{'text-primary': draft.startDate}" title="Start date" @click="openPicker('start_' + draft.id)">
                       <i class="fa-regular fa-clock" :class="draft.startDate ? '' : 'text-muted'"></i>
                       <span v-if="draft.startDate" style="margin-left: 4px;">{{ draft.startDate }}</span>
                    </button>
                    <el-date-picker
                      :ref="el => setPickerRef('start_' + draft.id, el)"
                      v-model="draft.startDate"
                      type="date"
                      format="YYYY-MM-DD"
                      value-format="YYYY-MM-DD"
                      style="position: absolute; bottom: 0; left: 0; width: 0; height: 0; opacity: 0; padding: 0; border: 0; visibility: hidden;"
                      @change="updateDraftProperty(draft, 'startDate', $event)"
                    />
                 </div>
                 
                 <!-- Due Date -->
                 <div style="position: relative; display: inline-flex;">
                    <button class="dm-tool-btn" :class="{'text-primary': draft.dueDate}" title="Due date" @click="openPicker('due_' + draft.id)">
                       <i class="fa-regular fa-calendar-check" :class="draft.dueDate ? '' : 'text-muted'"></i>
                       <span v-if="draft.dueDate" style="margin-left: 4px;">{{ draft.dueDate }}</span>
                    </button>
                    <el-date-picker
                      :ref="el => setPickerRef('due_' + draft.id, el)"
                      v-model="draft.dueDate"
                      type="date"
                      format="YYYY-MM-DD"
                      value-format="YYYY-MM-DD"
                      style="position: absolute; bottom: 0; left: 0; width: 0; height: 0; opacity: 0; padding: 0; border: 0; visibility: hidden;"
                      @change="updateDraftProperty(draft, 'dueDate', $event)"
                    />
                 </div>
                 
                 <!-- Cycles List View -->
                 <el-popover placement="bottom" trigger="click" width="240" popper-class="plane-popover">
                    <template #reference>
                      <button class="dm-tool-btn" :class="{'text-primary': draft.cycle}" title="Cycles">
                         <i class="fa-solid fa-arrows-spin" :class="draft.cycle ? '' : 'text-muted'"></i>
                         <span v-if="draft.cycle" style="margin-left: 4px;">{{ draft.cycle }}</span>
                      </button>
                    </template>
                    <div class="popover-content">
                       <input type="text" class="plane-search-input" v-model="searchCycle" placeholder="Search cycles">
                       <div class="plane-list mt-2">
                          <label class="plane-list-item" v-for="c in filteredCycles" :key="c.id" @click="updateDraftProperty(draft, 'cycle', c.name); closeAllPopovers()">
                             {{ c.name }}
                          </label>
                       </div>
                    </div>
                 </el-popover>

                 <!-- Modules List View -->
                 <el-popover placement="bottom" trigger="click" width="240" popper-class="plane-popover">
                    <template #reference>
                      <button class="dm-tool-btn" :class="{'text-primary': draft.module}" title="Modules">
                         <i class="fa-solid fa-table-cells-large" :class="draft.module ? '' : 'text-muted'"></i>
                         <span v-if="draft.module" style="margin-left: 4px;">{{ draft.module }}</span>
                      </button>
                    </template>
                    <div class="popover-content">
                       <input type="text" class="plane-search-input" v-model="searchModule" placeholder="Search modules">
                       <div class="plane-list mt-2">
                          <label class="plane-list-item" v-for="m in filteredModules" :key="m.id" @click="updateDraftProperty(draft, 'module', m.name); closeAllPopovers()">
                             {{ m.name }}
                          </label>
                       </div>
                    </div>
                 </el-popover>

                 <!-- Ellipsis Actions -->
                 <el-dropdown trigger="click" @command="(cmd) => handleEllipsisAction(cmd, draft)">
                    <button class="dm-tool-btn"><i class="fa-solid fa-ellipsis text-muted"></i></button>
                    <template #dropdown>
                      <el-dropdown-menu class="plane-dropdown">
                        <el-dropdown-item command="edit"><i class="fa-solid fa-pen" style="color:#A1A1AA"></i> Edit</el-dropdown-item>
                        <el-dropdown-item command="copy"><i class="fa-regular fa-copy" style="color:#A1A1AA"></i> Make a copy</el-dropdown-item>
                        <el-dropdown-item command="move"><i class="fa-solid fa-arrow-right-from-bracket" style="color:#A1A1AA"></i> Move to project</el-dropdown-item>
                        <el-dropdown-item command="delete" divided><i class="fa-solid fa-trash" style="color:#EF4444"></i> Delete</el-dropdown-item>
                      </el-dropdown-menu>
                    </template>
                 </el-dropdown>
               </div>
            </div>
         </div>
           </div>
         </div>
         <div v-if="drafts.length === 0" style="padding: 24px; color: #71717A;">
            No drafts found.
         </div>
         <div v-else class="drafts-pagination">
            <div class="pagination-summary">
              Page {{ pagination.page }} / {{ pagination.totalPages || 1 }}
            </div>
            <div class="pagination-actions">
              <button
                class="pagination-btn"
                :disabled="loadingDrafts || !pagination.hasPreviousPage"
                @click="changePage(pagination.page - 1)"
              >
                Previous
              </button>
              <button
                class="pagination-btn"
                :disabled="loadingDrafts || !pagination.hasNextPage"
                @click="changePage(pagination.page + 1)"
              >
                Next
              </button>
            </div>
         </div>
      </div>
    </div>

    <!-- Create / Edit Draft Modal -->
    <div class="modal-overlay" v-if="showModal" @click.self="showModal = false">
       <div class="draft-modal">
          <h3 class="modal-title">{{ editMode ? 'Edit draft' : 'Create a draft' }}</h3>
          
          <div class="proj-badge mt-4" v-if="projects.length > 0">
             <i class="fa-solid fa-bell" style="color: #F59E0B"></i>
             <span>{{ projects[0]?.name || 'Project' }}</span>
          </div>
          
          <input type="text" class="dm-title-input mt-4" placeholder="Title" v-model="form.title" />
          <textarea class="dm-desc-input mt-2" placeholder="Click to add description" v-model="form.description"></textarea>
          
          <div class="dm-toolbar mt-2">
             <!-- Status Dropdown -->
             <el-dropdown trigger="click" @command="(val) => form.statusName = val">
                <button class="dm-tool-btn"><i :class="getStatusIcon(form.statusName)" :style="{ color: getStatusColor(form.statusName) }"></i> {{ getStatusLabel(form.statusName) }}</button>
                <template #dropdown>
                  <el-dropdown-menu class="plane-dropdown">
                    <el-dropdown-item command="BACKLOG"><i class="fa-regular fa-circle-dot" style="color:#71717A"></i> Backlog</el-dropdown-item>
                    <el-dropdown-item command="TO DO"><i class="fa-regular fa-circle" style="color:#D4D4D8"></i> To Do</el-dropdown-item>
                    <el-dropdown-item command="IN PROGRESS"><i class="fa-solid fa-circle-half-stroke" style="color:#3B82F6"></i> In Progress</el-dropdown-item>
                    <el-dropdown-item command="DONE"><i class="fa-solid fa-circle-check" style="color:#10B981"></i> Done</el-dropdown-item>
                  </el-dropdown-menu>
                </template>
             </el-dropdown>
             
             <!-- Priority Dropdown -->
             <el-dropdown trigger="click" @command="(val) => form.priority = val">
                <button class="dm-tool-btn">
                   <i :class="getPriorityIcon(form.priority)" :style="{ color: getPriorityColor(form.priority) }"></i>
                   {{ getPriorityLabel(form.priority) }}
                </button>
                <template #dropdown>
                  <el-dropdown-menu class="plane-dropdown">
                    <el-dropdown-item :command="1"><i class="fa-solid fa-angles-up" style="color:#EF4444"></i> Urgent</el-dropdown-item>
                    <el-dropdown-item :command="2"><i class="fa-solid fa-chevron-up" style="color:#F97316"></i> High</el-dropdown-item>
                    <el-dropdown-item :command="3"><i class="fa-solid fa-minus" style="color:#3B82F6"></i> Normal</el-dropdown-item>
                    <el-dropdown-item :command="4"><i class="fa-solid fa-chevron-down" style="color:#94A3B8"></i> Low</el-dropdown-item>
                  </el-dropdown-menu>
                </template>
             </el-dropdown>

             <!-- Assignees Modal -->
             <el-popover placement="bottom" trigger="click" width="240" popper-class="plane-popover" :visible="popovers.assignees">
                <template #reference>
                  <button class="dm-tool-btn" :class="{'text-primary': form.assignee}" @click="togglePopover('assignees')">
                     <i class="fa-solid fa-user-group" :class="form.assignee ? '' : 'text-muted'"></i> 
                     {{ form.assignee || 'Assignees' }}
                  </button>
                </template>
                <div class="popover-content">
                   <input type="text" class="plane-search-input" v-model="searchAssignee" placeholder="Search members...">
                   <div class="plane-list mt-2">
                      <label class="plane-list-item" v-for="m in filteredMembers" :key="m.id" @click="form.assignee = m.name; closeAllPopovers()">
                         {{ m.name }}
                      </label>
                   </div>
                </div>
             </el-popover>

             <!-- Labels Modal -->
             <el-popover placement="bottom" trigger="click" width="240" popper-class="plane-popover" :visible="popovers.labels">
                <template #reference>
                  <button class="dm-tool-btn" :class="{'text-primary': form.label}" @click="togglePopover('labels')">
                     <i class="fa-solid fa-tag" :class="form.label ? '' : 'text-muted'"></i> 
                     {{ form.label || 'Labels' }}
                  </button>
                </template>
                <div class="popover-content">
                   <input type="text" class="plane-search-input" v-model="searchLabel" placeholder="Search labels...">
                   <div class="plane-list mt-2">
                      <label class="plane-list-item" v-for="l in filteredLabels" :key="l.id" @click="form.label = l.name; closeAllPopovers()">
                         {{ l.name }}
                      </label>
                   </div>
                </div>
             </el-popover>

             <!-- Start date -->
             <div style="position: relative; display: inline-flex;">
                <button class="dm-tool-btn" :class="{'text-primary': form.startDate}" @click="openPicker('form_start')">
                   <i class="fa-regular fa-clock" :class="form.startDate ? '' : 'text-muted'"></i>
                   <span v-if="form.startDate" style="margin-left: 4px;">{{ form.startDate }}</span>
                </button>
                <el-date-picker
                  :ref="el => setPickerRef('form_start', el)"
                  v-model="form.startDate"
                  type="date"
                  format="YYYY-MM-DD"
                  value-format="YYYY-MM-DD"
                  style="position: absolute; bottom: 0; left: 0; width: 0; height: 0; opacity: 0; padding: 0; border: 0; visibility: hidden;"
                />
             </div>

             <!-- Due date -->
             <div style="position: relative; display: inline-flex;">
                <button class="dm-tool-btn" :class="{'text-primary': form.dueDate}" @click="openPicker('form_due')">
                   <i class="fa-regular fa-calendar-check" :class="form.dueDate ? '' : 'text-muted'"></i>
                   <span v-if="form.dueDate" style="margin-left: 4px;">{{ form.dueDate }}</span>
                </button>
                <el-date-picker
                  :ref="el => setPickerRef('form_due', el)"
                  v-model="form.dueDate"
                  type="date"
                  format="YYYY-MM-DD"
                  value-format="YYYY-MM-DD"
                  style="position: absolute; bottom: 0; left: 0; width: 0; height: 0; opacity: 0; padding: 0; border: 0; visibility: hidden;"
                />
             </div>

             <!-- Cycle Modal -->
             <el-popover placement="bottom" trigger="click" width="240" popper-class="plane-popover" :visible="popovers.cycles">
                <template #reference>
                  <button class="dm-tool-btn" :class="{'text-primary': form.cycle}" @click="togglePopover('cycles')">
                     <i class="fa-solid fa-arrows-spin" :class="form.cycle ? '' : 'text-muted'"></i> 
                     {{ form.cycle || 'Cycle' }}
                  </button>
                </template>
                <div class="popover-content">
                   <input type="text" class="plane-search-input" v-model="searchCycle" placeholder="Search cycles...">
                   <div class="plane-list mt-2">
                      <label class="plane-list-item" v-for="c in filteredCycles" :key="c.id" @click="form.cycle = c.name; closeAllPopovers()">
                         {{ c.name }}
                      </label>
                   </div>
                </div>
             </el-popover>

             <!-- Modules Modal -->
             <el-popover placement="bottom" trigger="click" width="240" popper-class="plane-popover" :visible="popovers.modules">
                <template #reference>
                  <button class="dm-tool-btn" :class="{'text-primary': form.module}" @click="togglePopover('modules')">
                     <i class="fa-solid fa-table-cells-large" :class="form.module ? '' : 'text-muted'"></i> 
                     {{ form.module || 'Modules' }}
                  </button>
                </template>
                <div class="popover-content">
                   <input type="text" class="plane-search-input" v-model="searchModule" placeholder="Search modules...">
                   <div class="plane-list mt-2">
                      <label class="plane-list-item" v-for="m in filteredModules" :key="m.id" @click="form.module = m.name; closeAllPopovers()">
                         {{ m.name }}
                      </label>
                   </div>
                </div>
             </el-popover>
          </div>
          
          <div class="dm-footer mt-4">
             <div class="dm-footer-left">
                <label class="toggle-wrap" v-if="!editMode">
                   <div class="toggle-bg"><div class="toggle-knob"></div></div>
                   Create more
                </label>
             </div>
             <div class="dm-footer-right">
                <button class="discard-btn" @click="showModal = false">Discard</button>
                <button class="save-btn" @click="saveDraft">{{ editMode ? 'Update Draft' : 'Save as Draft' }}</button>
             </div>
          </div>
       </div>
    </div>

    <!-- Move to Project Modal -->
    <div class="modal-overlay" v-if="showMoveModal" @click.self="showMoveModal = false">
       <div class="draft-modal" style="width: 420px;">
          <h3 class="modal-title">Move to project</h3>
          <p class="text-muted text-xs mt-2" style="margin-bottom: 16px;">Select a project to move this draft into as a work item.</p>
          <div class="project-list">
            <div 
              class="project-item" 
              v-for="p in projects" 
              :key="p.id" 
              @click="moveToProject(p.id)"
            >
              <i class="fa-solid fa-bell" style="color: #F59E0B; font-size: 14px;"></i>
              <span>{{ p.name }}</span>
            </div>
          </div>
          <div class="dm-footer mt-4">
            <div></div>
            <button class="discard-btn" @click="showMoveModal = false">Cancel</button>
          </div>
       </div>
    </div>
  </NexusLayout>
</template>

<script setup>
import { ref, onMounted, computed, onBeforeUnmount, nextTick } from 'vue'
import NexusLayout from '@/components/layout/NexusLayout.vue'
import axiosClient from '@/api/axiosClient'
import { ElMessage } from 'element-plus'

const showModal = ref(false)
const showMoveModal = ref(false)
const editMode = ref(false)
const drafts = ref([])
const projects = ref([])
const movingDraft = ref(null)
const loadingDrafts = ref(false)
const draftListContainer = ref(null)
const scrollTop = ref(0)
const pagination = ref({
  page: 1,
  pageSize: 20,
  totalCount: 0,
  totalPages: 0,
  hasPreviousPage: false,
  hasNextPage: false
})

// Popovers Control
const popovers = ref({ assignees: false, labels: false, cycles: false, modules: false })

const ROW_HEIGHT = 56
const OVERSCAN = 6

const togglePopover = (key) => {
  const current = popovers.value[key]
  closeAllPopovers()
  popovers.value[key] = !current
}

const closeAllPopovers = () => {
  popovers.value = { assignees: false, labels: false, cycles: false, modules: false }
}

const clickOutsidePopover = (e) => {
  if (!e.target.closest('.el-popover') && !e.target.closest('.dm-tool-btn')) {
    closeAllPopovers()
  }
}

// Picker Refs (for hidden el-date-picker)
const pickerRefs = ref({})
const setPickerRef = (key, el) => {
  if (el) {
    pickerRefs.value[key] = el
  }
}
const openPicker = (key) => {
  const picker = pickerRefs.value[key]
  if (picker) {
    if (typeof picker.handleOpen === 'function') {
      picker.handleOpen()
      return
    }

    if (typeof picker.focus === 'function') {
      picker.focus()
      return
    }

    picker.$el?.querySelector('input')?.focus()
  }
}

// Data Storage
const dbMembers = ref([])
const dbLabels = ref([])
const dbCycles = ref([])
const dbModules = ref([])

// Searches
const searchAssignee = ref('')
const searchLabel = ref('')
const searchCycle = ref('')
const searchModule = ref('')

// Computed Filtered Lists
const filteredMembers = computed(() => dbMembers.value.filter(x => x.name.toLowerCase().includes(searchAssignee.value.toLowerCase())))
const filteredLabels = computed(() => dbLabels.value.filter(x => x.name.toLowerCase().includes(searchLabel.value.toLowerCase())))
const filteredCycles = computed(() => dbCycles.value.filter(x => x.name.toLowerCase().includes(searchCycle.value.toLowerCase())))
const filteredModules = computed(() => dbModules.value.filter(x => x.name.toLowerCase().includes(searchModule.value.toLowerCase())))

const viewportHeight = computed(() => draftListContainer.value?.clientHeight || 0)
const visibleCount = computed(() => Math.max(Math.ceil(viewportHeight.value / ROW_HEIGHT) + OVERSCAN, 1))
const startIndex = computed(() => Math.max(Math.floor(scrollTop.value / ROW_HEIGHT) - Math.floor(OVERSCAN / 2), 0))
const endIndex = computed(() => Math.min(startIndex.value + visibleCount.value, drafts.value.length))
const visibleDrafts = computed(() => drafts.value.slice(startIndex.value, endIndex.value))
const virtualOffsetTop = computed(() => startIndex.value * ROW_HEIGHT)
const virtualContentHeight = computed(() => drafts.value.length * ROW_HEIGHT)

const form = ref({
  id: null,
  title: '',
  description: '',
  statusName: 'BACKLOG',
  priority: 3,
  assignee: '',
  label: '',
  startDate: '',
  dueDate: '',
  cycle: '',
  module: ''
})

// ============ HELPERS ============

const getStatusIcon = (status) => {
  const s = (status || 'BACKLOG').toUpperCase()
  if (s === 'DONE') return 'fa-solid fa-circle-check'
  if (s === 'IN PROGRESS') return 'fa-solid fa-circle-half-stroke'
  if (s === 'TO DO') return 'fa-regular fa-circle'
  return 'fa-regular fa-circle-dot' // BACKLOG
}

const getStatusColor = (status) => {
  const s = (status || 'BACKLOG').toUpperCase()
  if (s === 'DONE') return '#10B981'
  if (s === 'IN PROGRESS') return '#3B82F6'
  if (s === 'TO DO') return '#D4D4D8'
  return '#71717A' // BACKLOG
}

const getStatusLabel = (status) => {
  const s = (status || 'BACKLOG').toUpperCase()
  if (s === 'DONE') return 'Done'
  if (s === 'IN PROGRESS') return 'In Progress'
  if (s === 'TO DO') return 'To Do'
  return 'Backlog'
}

const getPriorityIcon = (p) => {
  if (p === 1) return 'fa-solid fa-angles-up'
  if (p === 2) return 'fa-solid fa-chevron-up'
  if (p === 4) return 'fa-solid fa-chevron-down'
  return 'fa-solid fa-minus' // Normal (3 or default)
}

const getPriorityColor = (p) => {
  if (p === 1) return '#EF4444'
  if (p === 2) return '#F97316'
  if (p === 4) return '#94A3B8'
  return '#3B82F6' // Normal
}

const getPriorityLabel = (p) => {
  if (p === 1) return 'Urgent'
  if (p === 2) return 'High'
  if (p === 4) return 'Low'
  return 'Normal'
}

// ============ MODALS ============

const openModal = () => {
  form.value = { 
    id: null, 
    title: '', 
    description: '', 
    statusName: 'BACKLOG', 
    priority: 3,
    assignee: '',
    label: '',
    startDate: '',
    dueDate: '',
    cycle: '',
    module: ''
  }
  editMode.value = false
  showModal.value = true
}

const openEditModal = (draft) => {
  form.value = { 
     id: draft.id, 
     title: draft.title, 
     description: draft.description,
     statusName: draft.statusName || 'BACKLOG',
     priority: draft.priority || 3,
     assignee: draft.assignee || '',
     label: draft.label || '',
     startDate: draft.startDate || '',
     dueDate: draft.dueDate || '',
     cycle: draft.cycle || '',
     module: draft.module || ''
  }
  editMode.value = true
  showModal.value = true
}

// ============ DATA ============

const fetchDrafts = async (page = pagination.value.page) => {
  loadingDrafts.value = true
  try {
    const res = await axiosClient.get('/drafts', {
      params: {
        page,
        pageSize: pagination.value.pageSize,
        projectId: projects.value[0]?.id || undefined
      }
    })
    drafts.value = res.data?.data || []
    const serverPagination = res.data?.pagination || {}
    pagination.value = {
      page: serverPagination.page || page,
      pageSize: serverPagination.pageSize || pagination.value.pageSize,
      totalCount: serverPagination.totalCount || 0,
      totalPages: serverPagination.totalPages || 0,
      hasPreviousPage: Boolean(serverPagination.hasPreviousPage),
      hasNextPage: Boolean(serverPagination.hasNextPage)
    }
  } catch(e) {
    console.error(e)
  } finally {
    loadingDrafts.value = false
    await nextTick()
    if (draftListContainer.value) {
      draftListContainer.value.scrollTop = 0
      scrollTop.value = 0
    }
  }
}

const changePage = async (page) => {
  if (page < 1 || page === pagination.value.page) {
    return
  }

  await fetchDrafts(page)
}

const handleDraftListScroll = () => {
  scrollTop.value = draftListContainer.value?.scrollTop || 0
}

const loadProjectContextData = async (projectId) => {
  try {
    const memRes = await axiosClient.get(`/projects/${projectId}/members`)
    dbMembers.value = (memRes.data?.data || []).map(m => ({ id: m.userId || m.id, name: m.fullName || m.userName || m.email }))
  } catch(e) {}
  
  try {
    const lblRes = await axiosClient.get(`/projects/${projectId}/labels`)
    dbLabels.value = lblRes.data?.data || []
  } catch(e) {}

  try {
    const cycRes = await axiosClient.get(`/projects/${projectId}/sprints`)
    dbCycles.value = cycRes.data?.data || []
  } catch(e) {}

  try {
    const modRes = await axiosClient.get(`/projects/${projectId}/modules`)
    dbModules.value = (modRes.data?.data || []).map(m => ({ id: m.id, name: m.name }))
  } catch(e) {}
}

const fetchProjects = async () => {
  try {
    const res = await axiosClient.get('/projects')
    projects.value = res.data?.data || []
    if (projects.value.length > 0) {
      await loadProjectContextData(projects.value[0].id)
      await fetchDrafts(1)
    }
  } catch(e) {
    console.error(e)
  }
}

const saveDraft = async () => {
  if (!form.value.title) {
    ElMessage.warning('Title is required')
    return
  }
  try {
    const payload = {
      title: form.value.title,
      description: form.value.description,
      statusName: form.value.statusName,
      priority: form.value.priority,
      assignee: form.value.assignee,
      label: form.value.label,
      startDate: form.value.startDate,
      dueDate: form.value.dueDate,
      cycle: form.value.cycle,
      module: form.value.module,
      projectId: projects.value[0]?.id || null
    }
    
    if (editMode.value && form.value.id) {
       await axiosClient.put(`/drafts/${form.value.id}`, payload)
       ElMessage.success('Draft updated')
    } else {
       await axiosClient.post('/drafts', payload)
       ElMessage.success('Draft saved')
    }
    
    showModal.value = false
    fetchDrafts(pagination.value.page)
  } catch(e) {
    console.error(e)
    ElMessage.error('Failed to save draft')
  }
}

const updateDraftProperty = async (draft, field, value) => {
  try {
    const payload = {
      title: draft.title,
      description: draft.description,
      statusName: draft.statusName,
      priority: draft.priority,
      assignee: draft.assignee,
      label: draft.label,
      startDate: draft.startDate,
      dueDate: draft.dueDate,
      cycle: draft.cycle,
      module: draft.module,
      projectId: draft.projectId || projects.value[0]?.id || null
    }
    payload[field] = value
    await axiosClient.put(`/drafts/${draft.id}`, payload)
    draft[field] = value
    ElMessage.success('Draft updated')
  } catch(e) {
    console.error(e)
    ElMessage.error('Failed to update draft')
  }
}

const deleteDraft = async (id) => {
  try {
    await axiosClient.delete(`/drafts/${id}`)
    ElMessage.success('Draft deleted')
    const targetPage =
      drafts.value.length === 1 && pagination.value.page > 1
        ? pagination.value.page - 1
        : pagination.value.page
    fetchDrafts(targetPage)
  } catch(e) {
    console.error(e)
    ElMessage.error('Failed to delete draft')
  }
}

// ============ ELLIPSIS ACTIONS ============

const handleEllipsisAction = (command, draft) => {
  switch (command) {
    case 'edit':
      openEditModal(draft)
      break
    case 'copy':
      makeCopy(draft)
      break
    case 'move':
      handleMoveToProject(draft)
      break
    case 'delete':
      deleteDraft(draft.id)
      break
  }
}

const makeCopy = async (draft) => {
  try {
    await axiosClient.post('/drafts', {
      title: `${draft.title} (copy)`,
      description: draft.description,
      statusName: draft.statusName || 'BACKLOG',
      priority: draft.priority || 3,
      assignee: draft.assignee,
      label: draft.label,
      startDate: draft.startDate,
      dueDate: draft.dueDate,
      cycle: draft.cycle,
      module: draft.module,
      projectId: draft.projectId || projects.value[0]?.id || null
    })
    ElMessage.success('Draft duplicated')
    fetchDrafts(1)
  } catch(e) {
    console.error(e)
    ElMessage.error('Failed to duplicate draft')
  }
}

const handleMoveToProject = (draft) => {
  movingDraft.value = draft
  if (projects.value.length === 1) {
    // Only one project → move directly
    moveToProject(projects.value[0].id)
  } else if (projects.value.length > 1) {
    // Multiple projects → show picker
    showMoveModal.value = true
  } else {
    ElMessage.warning('No projects available')
  }
}

const moveToProject = async (projectId) => {
  if (!movingDraft.value) return
  const draft = movingDraft.value
  
  try {
    // Create as a real work item in the selected project
    await axiosClient.post(`/projects/${projectId}/WorkTasks`, {
      title: draft.title || 'Untitled',
      description: draft.description || '',
      statusName: draft.statusName || 'BACKLOG',
      priority: draft.priority || 3
    })
    
    // Delete the draft after successful move
    await axiosClient.delete(`/drafts/${draft.id}`)
    
    ElMessage.success('Draft moved to project successfully')
    showMoveModal.value = false
    movingDraft.value = null
    const targetPage =
      drafts.value.length === 1 && pagination.value.page > 1
        ? pagination.value.page - 1
        : pagination.value.page
    fetchDrafts(targetPage)
  } catch(e) {
    console.error(e)
    ElMessage.error('Failed to move draft to project')
  }
}

onMounted(() => {
  fetchDrafts()
  fetchProjects()
  document.addEventListener('click', clickOutsidePopover)
})

onBeforeUnmount(() => {
  document.removeEventListener('click', clickOutsidePopover)
})
</script>

<style scoped>
.drafts-wrapper {
  display: flex;
  flex-direction: column;
  height: 100vh;
  background: #0D0F11;
  color: #E4E4E7;
}

.dr-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 24px;
  border-bottom: 1px solid #1E2025;
}
.dr-left {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 14px;
  font-weight: 500;
}
.text-muted { color: #A1A1AA; }
.dr-title { color: #E4E4E7; }
.dr-badge {
  background: #1E2025;
  color: #0EA5E9;
  font-size: 11px;
  padding: 2px 6px;
  border-radius: 12px;
  font-weight: 600;
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
  transition: background 0.2s;
}
.plane-primary-btn:hover { background: #0284C7; }

/* List Styles */
.dr-body { padding: 0 24px; overflow-y: auto; flex: 1; }
.drafts-virtual-spacer { position: relative; width: 100%; }
.drafts-virtual-inner { position: absolute; top: 0; left: 0; right: 0; }
.list-row { display: flex; justify-content: space-between; align-items: center; padding: 10px 16px; border: 1px solid transparent; border-bottom: 1px solid #1E2025; transition: background 0.2s, border 0.2s; cursor: pointer; }
.list-row:hover { background: #16181D; border-radius: 4px; border-color: #27272A; }
.lr-left { display: flex; align-items: center; gap: 12px; flex-shrink: 0; }
.lr-title { font-size: 14px; font-weight: 500; color: #E4E4E7; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; max-width: 300px; }
.lr-right { display: flex; align-items: center; gap: 6px; }
.text-xs { font-size: 12px; }
.fw-500 { font-weight: 500; }

/* Popover Content (Search + List) */
.plane-search-input {
  width: 100%;
  background: #111315;
  border: 1px solid #27272A;
  color: #E4E4E7;
  padding: 6px 12px;
  border-radius: 4px;
  font-size: 13px;
  outline: none;
  box-sizing: border-box;
}
.plane-search-input:focus { border-color: #38BDF8; }
.plane-list { max-height: 200px; overflow-y: auto; display: flex; flex-direction: column; gap: 4px; }
.plane-list-item { 
  display: flex; gap: 8px; align-items: center;
  padding: 6px 8px; border-radius: 4px; cursor: pointer; color: #E4E4E7; font-size: 13px; 
}
.plane-list-item:hover { background: #1E2025; }

/* Legacy date picker override */
.date-picker-wrap {
  display: inline-block;
  width: 110px;
}

/* Modal Styles */
.modal-overlay {
  position: fixed;
  top: 0; left: 0; right: 0; bottom: 0;
  background: rgba(0,0,0,0.6);
  z-index: 1000;
  display: flex;
  align-items: center;
  justify-content: center;
}
.draft-modal {
  background: #111315;
  border: 1px solid #1E2025;
  width: 700px;
  border-radius: 8px;
  padding: 24px;
  box-shadow: 0 20px 40px rgba(0,0,0,0.5);
}

.modal-title { margin: 0; font-size: 16px; font-weight: 600; color: #E4E4E7; }
.mt-4 { margin-top: 24px; }
.mt-2 { margin-top: 12px; }

.proj-badge { display: inline-flex; align-items: center; gap: 8px; font-size: 12px; font-weight: 500; background: #16181D; padding: 4px 8px; border-radius: 4px; border: 1px solid #27272A; }

.dm-title-input {
  width: 100%;
  background: #16181D;
  border: 1px solid #27272A;
  color: #E4E4E7;
  padding: 12px 16px;
  border-radius: 6px;
  font-size: 14px;
  outline: none;
  box-sizing: border-box;
}
.dm-title-input::placeholder { color: #71717A; }
.dm-title-input:focus { border-color: #38BDF8; }

.dm-desc-input {
  width: 100%;
  background: #16181D;
  border: 1px solid #27272A;
  color: #E4E4E7;
  padding: 12px 16px;
  border-radius: 6px;
  font-size: 14px;
  outline: none;
  height: 120px;
  resize: none;
  font-family: inherit;
  box-sizing: border-box;
}
.dm-desc-input::placeholder { color: #71717A; }
.dm-desc-input:focus { border-color: #38BDF8; }

.dm-toolbar {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
  align-items: center;
}
.dm-tool-btn {
  background: transparent;
  border: 1px solid #27272A;
  color: #A1A1AA;
  font-size: 12px;
  display: inline-flex;
  align-items: center;
  gap: 6px;
  height: 26px;
  padding: 0 8px;
  border-radius: 4px;
  cursor: pointer;
  white-space: nowrap;
  transition: background 0.15s, color 0.15s;
}
.dm-tool-btn:hover { background: #1E2025; color: #E4E4E7; }
.text-primary { color: #E4E4E7 !important; border-color: #3f3f46 !important; background-color: #1E2025; }
.text-primary i { color: #E4E4E7 !important; }

.dm-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-top: 1px solid #1E2025;
  padding-top: 16px;
}
.toggle-wrap { display: flex; align-items: center; gap: 8px; font-size: 12px; color: #A1A1AA; cursor: pointer; }
.toggle-bg { width: 32px; height: 18px; background: #27272A; border-radius: 9px; position: relative; }
.toggle-knob { width: 14px; height: 14px; background: #71717A; border-radius: 50%; position: absolute; top: 2px; left: 2px; }

.dm-footer-right { display: flex; gap: 12px; }
.discard-btn { background: transparent; border: none; color: #A1A1AA; font-size: 13px; font-weight: 500; cursor: pointer; }
.discard-btn:hover { color: #E4E4E7; }
.save-btn { background: #0EA5E9; color: white; border: none; border-radius: 6px; padding: 6px 16px; font-size: 13px; font-weight: 500; cursor: pointer; }
.save-btn:hover { background: #0284C7; }

/* Move to project modal */
.project-list {
  display: flex;
  flex-direction: column;
  gap: 4px;
  max-height: 300px;
  overflow-y: auto;
}
.project-item {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 10px 12px;
  border-radius: 6px;
  cursor: pointer;
  font-size: 14px;
  color: #E4E4E7;
  transition: background 0.15s;
  border: 1px solid transparent;
}
.project-item:hover {
  background: #16181D;
  border-color: #27272A;
}

.drafts-pagination {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 16px 0 24px;
  border-top: 1px solid #1E2025;
  margin-top: 8px;
}

.pagination-summary {
  color: #A1A1AA;
  font-size: 12px;
}

.pagination-actions {
  display: flex;
  gap: 8px;
}

.pagination-btn {
  background: transparent;
  border: 1px solid #27272A;
  color: #E4E4E7;
  border-radius: 6px;
  padding: 6px 12px;
  font-size: 12px;
  cursor: pointer;
}

.pagination-btn:disabled {
  opacity: 0.45;
  cursor: not-allowed;
}

.pagination-btn:not(:disabled):hover {
  background: #16181D;
}
</style>
