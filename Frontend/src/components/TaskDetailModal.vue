<template>
  <transition name="fade">
    <div class="task-modal-overlay" v-if="showTaskModal" @mousedown.self="showTaskModal = false">
      
      <!-- MODE: CREATE NEW WORK ITEM (Image 1) -->
      <div class="create-centered-modal" v-if="selectedTask?.isNew">
        <h3 class="cm-title">Create new work item</h3>
        
        <div class="cm-badge-row">
           <div class="cm-badge">
             <i class="fa-solid fa-bell" style="color: #F59E0B"></i> {{ currentProjectBadge }}
           </div>
        </div>

        <div class="cm-form-group">
          <input type="text" class="cm-inputbox" placeholder="Title" v-model="selectedTask.title" />
          <textarea class="cm-textareabox" placeholder="Click to add description" v-model="selectedTask.description"></textarea>
        </div>
        
        <div class="cm-toolbar-row">
           <!-- STATUS -->
           <el-dropdown trigger="click" @command="(cmd) => selectStatus(cmd)">
             <div class="t-btn"><i :class="getStatusIcon(selectedTask?.statusName)"></i> <span>State</span> {{ selectedTask?.statusName || 'Todo' }}</div>
             <template #dropdown>
               <el-dropdown-menu class="theme-dropdown">
                 <el-dropdown-item v-for="status in projectStatuses" :key="status.id" :command="status.name"><i :class="getStatusIcon(status.name)" class="mr-2"></i> {{ status.displayName || status.name }}</el-dropdown-item>
               </el-dropdown-menu>
             </template>
           </el-dropdown>

           <!-- PRIORITY -->
           <el-dropdown  trigger="click" @command="(cmd) => selectedTask.priority = cmd">
             <div class="t-btn"><i :class="getPrioIcon(selectedTask?.priority)"></i> <span>Priority</span> {{ getPrioLabel(selectedTask?.priority) }}</div>
             <template #dropdown>
               <el-dropdown-menu class="theme-dropdown">
                 <el-dropdown-item :command="1"><i class="fa-solid fa-angles-up mr-2" style="color: #ef4444"></i> Urgent</el-dropdown-item>
                 <el-dropdown-item :command="2"><i class="fa-solid fa-chevron-up mr-2" style="color: #f59e0b"></i> High</el-dropdown-item>
                 <el-dropdown-item :command="3"><i class="fa-solid fa-minus mr-2" style="color: #3b82f6"></i> Medium</el-dropdown-item>
                 <el-dropdown-item :command="4"><i class="fa-solid fa-arrow-down mr-2" style="color: var(--color-text-muted)"></i> Low</el-dropdown-item>
                 <el-dropdown-item :command="0"><i class="fa-solid fa-ban mr-2 text-muted"></i> None</el-dropdown-item>
               </el-dropdown-menu>
             </template>
           </el-dropdown>

           <!-- ASSIGNEES -->
           <el-popover  placement="bottom-start" trigger="click" popper-class="plane-popover" :width="220" :disabled="!canManageTaskAssignees" @show="assigneeSearch = ''">
             <template #reference>
               <div class="t-btn" :class="{ disabled: !canManageTaskAssignees }"><i class="fa-regular fa-user"></i> <span>Assignees</span> {{ getAssigneeSummary() }}</div>
             </template>
             <div class="popover-content">
               <input type="text" v-model="assigneeSearch" class="popover-search" placeholder="Type to search..." />
                <div class="popover-list">
                  <div class="popover-item" v-for="user in filteredMembers" :key="user.userId" @click="toggleAssignee(user.userId)">
                    <div class="avatar-xxs bg-accent-muted rounded-full w-5 h-5 flex-center text-white text-xs mr-2">{{ (user.fullName || user.email || 'U').charAt(0).toUpperCase() }}</div>
                    <span>{{ user.fullName || user.email }}</span>
                    <i v-if="getAssigneeIds().includes(user.userId)" class="fa-solid fa-check ms-auto"></i>
                  </div>
                  <div v-if="!filteredMembers.length" class="text-xs text-center text-muted py-2">No assignees found.</div>
                </div>
                <div class="assignee-progress-list" v-if="selectedAssigneeRows.length">
                  <div class="assignee-progress-title">Progress by assignee</div>
                  <div class="assignee-progress-row" v-for="assignee in selectedAssigneeRows" :key="assignee.userId">
                    <span class="assignee-progress-name">{{ assignee.fullName || assignee.email || 'Member' }}</span>
                    <input
                      class="assignee-progress-input"
                      type="number"
                      min="0"
                      max="100"
                      step="1"
                      :disabled="!canManageTaskAssignees"
                      :value="assignee.progressPercent || 0"
                      @change="event => updateAssigneeProgress(assignee.userId, event.target.value)"
                    />
                    <span class="assignee-progress-suffix">%</span>
                    <input
                      class="assignee-progress-input"
                      type="number"
                      min="0"
                      step="0.5"
                      :disabled="!canManageTaskAssignees"
                      :value="assignee.estimatedHours || 0"
                      @change="event => updateAssigneeEstimatedHours(assignee.userId, event.target.value)"
                    />
                    <span class="assignee-progress-suffix">h</span>
                    <input
                      class="assignee-progress-input"
                      type="number"
                      min="0"
                      step="0.1"
                      :disabled="!canManageTaskAssignees"
                      :value="assignee.contributionWeight || 1"
                      @change="event => updateAssigneeContributionWeight(assignee.userId, event.target.value)"
                    />
                    <span class="assignee-progress-suffix">w</span>
                  </div>
                </div>
              </div>
            </el-popover>

           <!-- LABELS -->
           <el-popover  placement="bottom-start" trigger="click" popper-class="plane-popover" :width="220" @show="labelSearch = ''">
             <template #reference>
               <div class="t-btn"><i class="fa-solid fa-tag"></i> {{ selectedTask?.labelIds?.length ? selectedTask.labelIds.length + ' Labels' : 'Labels' }}</div>
             </template>
             <div class="popover-content">
               <input type="text" v-model="labelSearch" class="popover-search" placeholder="Search" />
               <div class="popover-list">
                 <div class="popover-item" v-for="l in filteredLabels" :key="l.id" @click="toggleSelectedLabel(l.id)">
                   <div class="popover-c-circle mr-2 w-3 h-3 rounded-full" :style="{ backgroundColor: l.color || 'var(--color-accent)' }"></div>
                   <span>{{ l.name }}</span>
                   <i v-if="selectedTask?.labelIds?.includes(l.id)" class="fa-solid fa-check ms-auto"></i>
                 </div>
                 <div class="popover-item pointer hover-bg-accent" v-if="filteredLabels.length === 0 && labelSearch" @click="createLabel(labelSearch)">
                   <span>Add "{{ labelSearch }}"</span>
                 </div>
               </div>
             </div>
           </el-popover>

           <!-- DATES -->
           <el-date-picker
             v-model="selectedTask.plannedStartDate"
             type="date"
             placeholder="Start date"
             class="t-btn-date"
             format="MMM DD"
             value-format="YYYY-MM-DD"
             :disabled-date="disablePastDates"
             style="width:130px; height:28px"
             @change="val => handleTaskDateChange('plannedStartDate', val)"
           />
           <el-date-picker
             v-model="selectedTask.dueDate"
             type="date"
             placeholder="Due date"
             class="t-btn-date"
             format="MMM DD"
             value-format="YYYY-MM-DD"
             :disabled-date="disableDueDates"
             style="width:125px; height:28px"
             @change="val => handleTaskDateChange('dueDate', val)"
           />
           <div class="t-btn t-btn-number">
             <i class="fa-regular fa-hourglass-half"></i>
             <span>Estimate</span>
             <input
               :value="getEstimatedHours(selectedTask)"
               type="number"
               min="0"
               step="0.5"
               class="estimate-inline-input"
               @change="event => updateEstimatedHours(event.target.value, selectedTask)"
             />
             <small>h</small>
           </div>

           <el-dropdown v-if="isRoleVisibilityEnabled" trigger="click" @command="(cmd) => selectVisibilityMode(cmd, selectedTask)">
             <div class="t-btn">
               <i class="fa-solid fa-eye"></i>
               <span>Visibility</span>
               {{ getVisibilityLabel(selectedTask) }}
             </div>
             <template #dropdown>
               <el-dropdown-menu class="theme-dropdown">
                 <el-dropdown-item command="project">Project members</el-dropdown-item>
                 <el-dropdown-item command="assigned">Assigned only</el-dropdown-item>
                 <el-dropdown-item command="role">Role scoped</el-dropdown-item>
               </el-dropdown-menu>
             </template>
           </el-dropdown>

           <el-popover v-if="isRoleVisibilityEnabled && selectedTask?.visibilityMode === 'role'" placement="bottom-start" trigger="click" popper-class="plane-popover" :width="260" :disabled="!canEditTaskVisibility">
             <template #reference>
               <div class="t-btn" :class="{ disabled: !canEditTaskVisibility }">
                 <i class="fa-solid fa-user-shield"></i>
                 <span>Roles</span>
                 {{ selectedTask?.visibleToRoles?.length ? selectedTask.visibleToRoles.join(', ') : 'Select roles' }}
               </div>
             </template>
             <div class="popover-content">
               <div class="popover-list">
                 <div class="popover-item" v-for="role in availableVisibilityRoles" :key="role" @click="toggleVisibleRole(role, selectedTask)">
                   <span>{{ role }}</span>
                   <i v-if="selectedTask?.visibleToRoles?.includes(role)" class="fa-solid fa-check ms-auto"></i>
                 </div>
               </div>
             </div>
           </el-popover>

           <!-- CYCLE -->
           <el-popover  placement="bottom-start" trigger="click" popper-class="plane-popover" :width="280" @show="cycleSearch = ''">
             <template #reference>
               <div class="t-btn"><i class="fa-solid fa-circle-half-stroke"></i> {{ getCycleLabel(selectedTask?.sprintId) }}</div>
             </template>
             <div class="popover-content">
               <input type="text" v-model="cycleSearch" class="popover-search" placeholder="Search" />
               <div class="popover-list">
                 <div class="popover-item" @click="selectedTask.sprintId = null">
                   <i class="fa-solid fa-circle-half-stroke mr-2 w-4 text-center"></i> No cycle
                   <i v-if="!selectedTask?.sprintId" class="fa-solid fa-check ms-auto"></i>
                 </div>
                 <div class="popover-item" v-for="c in filteredCycles" :key="c.id" @click="selectedTask.sprintId = c.id">
                   <i class="fa-solid fa-certificate mr-2 w-4 text-center text-blue-500"></i>
                   <span class="truncate flex-1">{{ c.name }}</span>
                   <i v-if="selectedTask?.sprintId === c.id" class="fa-solid fa-check ms-auto"></i>
                 </div>
                 <div v-if="!filteredCycles.length" class="text-xs text-center text-muted py-2">No cycles setup.</div>
               </div>
             </div>
           </el-popover>

           <!-- MODULES -->
           <el-popover  placement="bottom-start" trigger="click" popper-class="plane-popover" :width="280" @show="moduleSearch = ''">
             <template #reference>
               <div class="t-btn"><i class="fa-solid fa-cube"></i> {{ getModuleLabel(selectedTask?.moduleId) }}</div>
             </template>
             <div class="popover-content">
               <input type="text" v-model="moduleSearch" class="popover-search" placeholder="Search" />
               <div class="popover-list">
                 <div class="popover-item" @click="selectedTask.moduleId = null">
                   <i class="fa-solid fa-cube mr-2"></i> No module
                 </div>
                 <div class="popover-item" v-for="m in filteredModules" :key="m.id" @click="selectedTask.moduleId = m.id">
                   <i class="fa-solid fa-box mr-2 text-orange-500"></i>
                   <span class="truncate flex-1">{{ m.name }}</span>
                 </div>
               </div>
             </div>
           </el-popover>

           <!-- PARENT -->
           <el-popover  placement="bottom-start" trigger="click" popper-class="plane-popover" :width="350" @show="parentSearch = ''">
             <template #reference>
               <div class="t-btn"><i class="fa-solid fa-arrow-turn-up fa-rotate-90"></i> {{ getParentId(selectedTask) ? 'Parent selected' : 'Add parent' }}</div>
             </template>
             <div class="popover-content h-[250px] flex flex-col bg-surface-elevation">
               <div class="p-2 border-b border-theme">
                 <div class="relative flex items-center">
                   <i class="fa-solid fa-magnifying-glass absolute left-2 text-muted"></i>
                   <input type="text" v-model="parentSearch" class="w-full bg-transparent border-none text-primary pl-8 focus:outline-none" placeholder="Type to search..." />
                 </div>
               </div>
               <div class="flex-1 overflow-y-auto no-scrollbar p-2">
                 <div class="popover-item text-xs text-muted hover:text-primary cursor-pointer p-2 rounded hover:bg-surface-hover flex items-center" @click="setTaskParent(selectedTask, null)">
                   <i class="fa-solid fa-ban mr-2"></i> Remove parent
                 </div>
                 <div class="popover-item text-xs text-secondary hover:text-primary cursor-pointer p-2 rounded hover:bg-surface-hover flex items-center" v-for="pt in filteredParents" :key="pt.id" @click="setTaskParent(selectedTask, pt.id)">
                   <span class="text-muted mr-3 w-16 truncate font-mono">{{ pt.sequenceId || pt.id.substring(0,8) }}</span>
                   <span class="truncate flex-1">{{ pt.title }}</span>
                   <i v-if="getParentId(selectedTask) === pt.id" class="fa-solid fa-check ml-2 text-blue-500"></i>
                 </div>
               </div>
             </div>
           </el-popover>
        </div>
        
        <div class="cm-footer-row">
           <div class="cm-t-more">
              <el-switch v-model="createMore" size="small" style="--el-switch-on-color: #38bdf8;" /> <span>Create more</span>
           </div>
           <button class="btn-discard" @click="discardNewTask">Discard</button>
           <button class="btn-save" @click="submitNewTask">Save</button>
        </div>
      </div>
      
      <!-- MODE: TASK DETAIL SLIDEOUT (Image 2 & 3) -->
      <div class="task-side-panel slide-in-right" v-else>
         <div class="sp-header">
            <div class="sph-left">
               <button v-if="canGoBack" class="nav-icon-btn" type="button" @click="emit('back')">
                 <i class="fa-solid fa-arrow-left"></i>
               </button>
               <i class="fa-solid fa-arrow-right icon-btn" @click="showTaskModal = false"></i>
            </div>
             <div class="sph-right">
                <button class="s-btn s-btn-outline" @click="toggleSubscription">
                   <i :class="isSubscribed ? 'fa-regular fa-bell-slash' : 'fa-regular fa-bell'"></i> 
                   {{ isSubscribed ? 'Unsubscribe' : 'Subscribe' }}
                </button>
                <button class="s-btn" @click="copyTaskLink">
                   <i class="fa-solid fa-link"></i> 
                   Copy link
                </button>
                <el-dropdown trigger="click" @command="handleTaskMenuCommand">
                  <button class="s-btn s-btn-icon" title="More actions">
                     <i class="fa-solid fa-ellipsis"></i>
                  </button>
                  <template #dropdown>
                    <el-dropdown-menu class="theme-dropdown">
                      <el-dropdown-item command="copy"><i class="fa-solid fa-link mr-2"></i> Copy link</el-dropdown-item>
                      <el-dropdown-item command="duplicate"><i class="fa-regular fa-clone mr-2"></i> Duplicate</el-dropdown-item>
                      <el-dropdown-item command="archive"><i class="fa-solid fa-box-archive mr-2"></i> Archive soon</el-dropdown-item>
                    </el-dropdown-menu>
                  </template>
                </el-dropdown>
             </div>
         </div>
         
         <div class="sp-body">
            <!-- Header Title -->
            <div class="sp-breadcrumb">
               {{ selectedTask?.sequenceId || selectedTask?.id.substring(0,8).toUpperCase() }}
            </div>
            <div v-if="currentParentId" class="parent-context-banner">
              <span class="parent-context-label">Parent</span>
              <button class="parent-context-link" type="button" @click="openParentTask">
                {{ currentParentLabel }}
              </button>
            </div>
            
            <h1 class="sp-title" contenteditable @blur="(e) => updateTaskField(selectedTask, 'title', e.target.innerText)">{{ selectedTask?.title }}</h1>
            <div class="description-editor-shell">
              <div v-if="showFormatToolbar" class="description-toolbar floating-toolbar" :style="{ left: toolbarPosition.x + 'px', top: toolbarPosition.y + 'px' }">
                <select class="format-select" @change="applyBlockFormat($event.target.value)">
                  <option value="div">Text</option>
                  <option value="h1">Heading 1</option>
                  <option value="h2">Heading 2</option>
                  <option value="h3">Heading 3</option>
                  <option value="blockquote">Quote</option>
                </select>
                <div class="color-menu">
                  <button class="color-trigger">Color</button>
                  <div class="color-palette">
                    <button v-for="color in textColors" :key="'fg-' + color" :style="{ background: color }" @click="applyTextColor(color)"></button>
                    <button v-for="color in backgroundColors" :key="'bg-' + color" :style="{ background: color }" @click="applyBackgroundColor(color)"></button>
                  </div>
                </div>
                <i class="fa-solid fa-align-left icon-hover" @mousedown.prevent="execEditorCommand('justifyLeft', null, 'description')"></i>
                <i class="fa-solid fa-align-center icon-hover" @mousedown.prevent="execEditorCommand('justifyCenter', null, 'description')"></i>
                <i class="fa-solid fa-align-right icon-hover" @mousedown.prevent="execEditorCommand('justifyRight', null, 'description')"></i>
                <div class="toolbar-sep"></div>
                <i class="fa-solid fa-bold icon-hover" @mousedown.prevent="execEditorCommand('bold', null, 'description')"></i>
                <i class="fa-solid fa-italic icon-hover" @mousedown.prevent="execEditorCommand('italic', null, 'description')"></i>
                <i class="fa-solid fa-underline icon-hover" @mousedown.prevent="execEditorCommand('underline', null, 'description')"></i>
                <i class="fa-solid fa-strikethrough icon-hover" @mousedown.prevent="execEditorCommand('strikeThrough', null, 'description')"></i>
                <i class="fa-solid fa-list-ul icon-hover" @mousedown.prevent="execEditorCommand('insertUnorderedList', null, 'description')"></i>
                <i class="fa-solid fa-list-ol icon-hover" @mousedown.prevent="execEditorCommand('insertOrderedList', null, 'description')"></i>
                <i class="fa-solid fa-file-code icon-hover" :class="{ 'is-active': codeMode.description }" @mousedown.prevent="toggleCodeBlockMode('description')"></i>
                <div class="toolbar-sep"></div>
                <i class="fa-regular fa-image icon-hover" @mousedown.prevent="triggerDescriptionImageUpload"></i>
              </div>
              <div
                ref="descriptionEditor"
                class="sp-desc rich-editor"
                contenteditable
                :data-placeholder="selectedTask?.description ? '' : 'Click to add description'"
                @focus="activeEditor = 'description'"
                @keydown="handleEditorKeydown($event, 'description')"
                @mouseup="showSelectionToolbar"
                @keyup="showSelectionToolbar"
                @contextmenu.prevent="showSelectionToolbar"
                @paste="handleDescriptionPaste"
                @input="handleDescriptionInput"
                @blur="handleDescriptionBlur"
              ></div>
              <input ref="descriptionImageInput" type="file" accept=".png,.jpg,.jpeg,.webp,.gif,.svg,image/*" style="display:none" @change="handleDescriptionUpload($event, 'image')" />
              <input ref="descriptionFileInput" type="file" style="display:none" @change="handleDescriptionUpload($event, 'file')" />
            </div>
            
            <div class="sp-sub-actions">
               <i class="fa-regular fa-face-smile icon-btn" style="font-size: 16px;"></i>
               <div class="sp-edit-info">
                  <i class="fa-solid fa-clock-rotate-left"></i> Last edited by <b>{{ lastEditedBy }}</b> {{ lastEditedRelative }}
                </div>
            </div>

            <!-- Action Chips -->
             <div class="sp-toolbar">
                <button class="s-btn" @click="startCreateSubtask"><i class="fa-solid fa-layer-group"></i> Add sub-work item</button>
                <button class="s-btn s-btn-primary" :disabled="isAiBreakingDown" @click="createSubtasksWithAI">
                  <i class="fa-solid fa-wand-magic-sparkles"></i>
                  {{ isAiBreakingDown ? 'AI is preparing...' : 'AI split into subtasks' }}
                </button>
                
                <button class="s-btn" @click="triggerDescriptionFileUpload"><i class="fa-solid fa-paperclip"></i> Attach</button>
             </div>
            <div v-if="aiSubtaskPreview.length" class="ai-preview-panel">
              <div class="ai-preview-head">
                <div>
                  <strong>AI subtask preview</strong>
                  <p>Review these suggested sub-work items before creating them.</p>
                </div>
                <div class="ai-preview-actions">
                  <button class="quick-subtask-cancel" @click="discardAiSubtaskPreview">Discard</button>
                  <button class="quick-subtask-save" :disabled="isCreatingPreviewSubtasks" @click="confirmAiSubtaskPreview">
                    {{ isCreatingPreviewSubtasks ? 'Creating...' : `Create ${aiSubtaskPreview.length} sub-work items` }}
                  </button>
                </div>
              </div>
              <div class="ai-preview-list">
                <div v-for="(subtask, index) in aiSubtaskPreview" :key="`ai-preview-${index}`" class="ai-preview-item">
                  <div class="ai-preview-top">
                    <strong>{{ subtask.title }}</strong>
                    <span>{{ Number(subtask.estHours || 0).toFixed(1) }}h · P{{ subtask.priority || 3 }}</span>
                  </div>
                  <p>{{ subtask.description || 'No description' }}</p>
                </div>
              </div>
            </div>
            <div v-if="isCreatingSubtask" class="quick-subtask-box">
              <input
                ref="subtaskInputRef"
                v-model="newSubtaskTitle"
                type="text"
                class="quick-subtask-input"
                placeholder="Create a linked sub-work item"
                @keyup.enter="submitSubtask"
                @keyup.esc="isCreatingSubtask = false"
              />
              <div class="quick-subtask-actions">
                <button class="quick-subtask-cancel" @click="isCreatingSubtask = false">Cancel</button>
                <button class="quick-subtask-save" @click="submitSubtask">Create</button>
              </div>
            </div>
             <div class="subtask-toggle-row" v-if="subtasksList.length">
               <button class="subtask-toggle-btn" @click="showSubtasks = !showSubtasks">
                 <i :class="showSubtasks ? 'fa-solid fa-eye' : 'fa-solid fa-eye-slash'"></i>
                 {{ showSubtasks ? 'Hide sub-work items' : 'Show sub-work items' }}
               </button>
             </div>
             <div v-if="subtasksList.length && showSubtasks" class="subtask-list">
               <div
                 v-for="subtask in subtasksList"
                 :key="subtask.id"
                 class="subtask-item"
               >
                <button class="subtask-open" type="button" @click="openTaskDetail(subtask)">
                  <span class="subtask-seq">{{ subtask.sequenceId || subtask.id?.substring(0, 8) }}</span>
                  <span class="subtask-title">{{ subtask.title }}</span>
                </button>
                <div class="subtask-controls" @click.stop>
                  <el-dropdown trigger="click" @command="(cmd) => selectStatus({ name: cmd }, subtask)">
                    <button class="subtask-chip" type="button">
                      <i :class="getStatusIcon(subtask.statusName)"></i>
                      <span>{{ getStatusLabel(subtask.statusName) }}</span>
                    </button>
                    <template #dropdown>
                      <el-dropdown-menu class="theme-dropdown">
                        <el-dropdown-item v-for="status in projectStatuses" :key="`${subtask.id}-${status.id}`" :command="status.name">
                          <i :class="getStatusIcon(status.name)" class="mr-2"></i>
                          {{ status.displayName || status.name }}
                        </el-dropdown-item>
                      </el-dropdown-menu>
                    </template>
                  </el-dropdown>

                  <el-dropdown trigger="click" @command="(cmd) => selectPriority(cmd, subtask)">
                    <button class="subtask-chip" type="button">
                      <i :class="getPrioIcon(subtask.priority)"></i>
                      <span>{{ getPrioLabel(subtask.priority) }}</span>
                    </button>
                    <template #dropdown>
                      <el-dropdown-menu class="theme-dropdown">
                        <el-dropdown-item :command="1"><i class="fa-solid fa-angles-up mr-2" style="color: var(--color-danger)"></i> Urgent</el-dropdown-item>
                        <el-dropdown-item :command="2"><i class="fa-solid fa-chevron-up mr-2" style="color: var(--color-warning)"></i> High</el-dropdown-item>
                        <el-dropdown-item :command="3"><i class="fa-solid fa-minus mr-2" style="color: var(--color-accent)"></i> Medium</el-dropdown-item>
                        <el-dropdown-item :command="4"><i class="fa-solid fa-arrow-down mr-2" style="color: var(--color-text-muted)"></i> Low</el-dropdown-item>
                        <el-dropdown-item :command="0"><i class="fa-solid fa-ban mr-2 text-muted"></i> None</el-dropdown-item>
                      </el-dropdown-menu>
                    </template>
                  </el-dropdown>

                  <el-popover placement="bottom-start" trigger="click" popper-class="plane-popover" :width="240" @show="assigneeSearch = ''">
                    <template #reference>
                      <button class="subtask-chip" type="button">
                        <i class="fa-regular fa-user"></i>
                        <span>{{ getAssigneeSummary(subtask) }}</span>
                      </button>
                    </template>
                    <div class="popover-content">
                      <input v-model="assigneeSearch" type="text" class="popover-search" placeholder="Search members..." />
                      <div class="popover-list">
                        <div class="popover-item" v-for="member in filteredMembers" :key="`${subtask.id}-${member.userId}`" @click="toggleInlineTaskAssignee(subtask, member.userId)">
                          <div class="avatar-xxs bg-gray-600 rounded-full w-5 h-5 flex-center text-white text-xs mr-2">{{ (member.fullName || member.email || 'U').charAt(0).toUpperCase() }}</div>
                          <span>{{ member.fullName || member.email }}</span>
                          <i v-if="getAssigneeIds(subtask).includes(member.userId)" class="fa-solid fa-check ms-auto"></i>
                        </div>
                      </div>
                    </div>
                  </el-popover>

                  <label class="subtask-progress">
                    <span>Progress</span>
                    <input
                      type="number"
                      min="0"
                      max="100"
                      step="1"
                      :value="getTaskProgressPercent(subtask)"
                      :disabled="!canEditTaskProgress(subtask)"
                      @click.stop
                      @change="event => updateTaskProgress(subtask, event.target.value)"
                    />
                    <span>%</span>
                  </label>
                </div>
              </div>
            </div>

            <h3 class="sp-section-title">Properties</h3>
            <div class="props-grid">
                <div class="p-row">
                  <div class="p-label"><i class="fa-regular fa-circle-dot"></i> State</div>
                  <div class="p-val">
                    <el-popover placement="bottom-start" trigger="click" popper-class="plane-popover" :width="260" @show="statusSearch = ''">
                      <template #reference>
                        <button class="property-trigger">
                          <i :class="getStatusIcon(selectedTask?.statusName)"></i>
                          <span>State</span>
                          <span class="property-value">{{ getStatusLabel(selectedTask?.statusName) }}</span>
                        </button>
                      </template>
                      <div class="popover-content">
                        <input v-model="statusSearch" type="text" class="popover-search" placeholder="Search states..." />
                        <div class="popover-list">
                          <div class="popover-item" v-for="status in filteredStatuses" :key="status.id" @click="selectStatus(status)">
                            <i :class="getStatusIcon(status.name)" class="mr-2"></i>
                            <span>{{ status.displayName || status.name }}</span>
                            <i v-if="selectedTask?.statusName === status.name" class="fa-solid fa-check ms-auto"></i>
                          </div>
                        </div>
                      </div>
                    </el-popover>
                  </div>
                </div>
                <div class="p-row">
                  <div class="p-label"><i class="fa-solid fa-percent"></i> Progress</div>
                  <div class="p-val">
                    <div class="task-progress-editor">
                      <input
                        class="task-progress-input"
                        type="number"
                        min="0"
                        max="100"
                        step="1"
                        :value="getTaskProgressPercent(selectedTask)"
                        :disabled="!canEditTaskProgress(selectedTask)"
                        @change="event => updateTaskProgress(selectedTask, event.target.value)"
                      />
                      <span class="task-progress-suffix">%</span>
                      <span class="task-progress-hint">
                        Progress is calculated from assignee completion and Done state.
                      </span>
                    </div>
                  </div>
                </div>
                <div class="p-row">
                  <div class="p-label"><i class="fa-regular fa-user"></i> Assignees</div>
                  <div class="p-val">
                    <el-popover placement="bottom-start" trigger="click" popper-class="plane-popover" :width="260" :disabled="!canManageTaskAssignees" @show="assigneeSearch = ''">
                      <template #reference>
                        <button class="property-trigger" :class="{ 'muted-val': !getAssigneeIds().length }" :disabled="!canManageTaskAssignees">
                          <i class="fa-regular fa-user"></i>
                          <span>Assignees</span>
                          <span class="property-value">{{ getAssigneeSummary() }}</span>
                        </button>
                      </template>
                      <div class="popover-content">
                        <input v-model="assigneeSearch" type="text" class="popover-search" placeholder="Search members..." />
                        <div class="popover-list">
                          <div class="popover-item" v-for="member in filteredMembers" :key="member.userId" @click="toggleAssignee(member.userId)">
                            <div class="avatar-xxs bg-gray-600 rounded-full w-5 h-5 flex-center text-white text-xs mr-2">{{ (member.fullName || member.email || 'U').charAt(0).toUpperCase() }}</div>
                            <span>{{ member.fullName || member.email }}</span>
                            <i v-if="getAssigneeIds().includes(member.userId)" class="fa-solid fa-check ms-auto"></i>
                          </div>
                        </div>
                      </div>
                    </el-popover>
                    <button v-if="canUseAiAssigneeSuggestion" class="property-trigger estimate-suggestion-btn ai-estimate-btn mt-2" :disabled="isAiSuggestingAssignees" @click="suggestAssigneesWithAI()">
                      <i class="fa-solid fa-user-group"></i>
                      <span>{{ isAiSuggestingAssignees ? 'AI is matching...' : 'AI suggest assignees' }}</span>
                    </button>
                    <div v-if="canUseAiAssigneeSuggestion && aiAssigneeSuggestion" class="estimate-breakdown ai-suggestion-panel ai-assignee-panel">
                      <div class="estimate-breakdown-row">
                        <span>Recommended team size</span>
                        <strong>{{ aiAssigneeSuggestion.recommendedAssigneeCount }}</strong>
                      </div>
                      <small class="estimate-helper-text">{{ aiAssigneeSuggestion.summary }}</small>
                      <div class="ai-assignee-list">
                        <div v-for="candidate in aiAssigneeSuggestion.suggestions" :key="candidate.userId" class="ai-assignee-item">
                          <div class="ai-assignee-top">
                            <strong>{{ candidate.fullName }}</strong>
                            <span>{{ Math.round((candidate.fitScore || 0) * 100) }}% fit</span>
                          </div>
                          <div class="ai-assignee-metrics">
                            <span>{{ candidate.projectRole || 'Member' }}</span>
                            <span>{{ candidate.completedStoryPoints }} pts done</span>
                            <span>{{ candidate.averageAccuracyPercent }}% accuracy</span>
                            <span>{{ candidate.activeEstimatedHours }}h active</span>
                            <span v-if="candidate.suggestedEstimatedHours">{{ candidate.suggestedEstimatedHours }}h suggested</span>
                          </div>
                          <small class="estimate-helper-text">{{ candidate.reasoning }}</small>
                        </div>
                      </div>
                      <div class="ai-preview-actions">
                        <button class="quick-subtask-cancel" type="button" @click="aiAssigneeSuggestion = null">Discard</button>
                        <button class="quick-subtask-save" type="button" @click="applyAiAssigneeSuggestion('top')">Apply top suggestion</button>
                        <button class="quick-subtask-save" type="button" @click="applyAiAssigneeSuggestion('team')">Apply suggested team</button>
                      </div>
                    </div>
                  </div>
                </div>
                <div v-if="isRoleVisibilityEnabled" class="p-row">
                  <div class="p-label"><i class="fa-solid fa-eye"></i> Visibility</div>
                  <div class="p-val">
                    <el-dropdown trigger="click" @command="(cmd) => selectVisibilityMode(cmd)">
                      <div class="property-trigger" :class="{ 'muted-val': !selectedTask?.visibilityMode }">
                        <i class="fa-solid fa-eye"></i>
                        <span>Visibility</span>
                        <span class="property-value">{{ getVisibilityLabel(selectedTask) }}</span>
                      </div>
                      <template #dropdown>
                        <el-dropdown-menu class="theme-dropdown">
                          <el-dropdown-item command="project">Project members</el-dropdown-item>
                          <el-dropdown-item command="assigned">Assigned only</el-dropdown-item>
                          <el-dropdown-item command="role">Role scoped</el-dropdown-item>
                        </el-dropdown-menu>
                      </template>
                    </el-dropdown>
                    <div v-if="selectedTask?.visibilityMode === 'role'" class="estimate-breakdown mt-2">
                      <div class="estimate-breakdown-row">
                        <span>Visible roles</span>
                        <strong>{{ selectedTask?.visibleToRoles?.length ? selectedTask.visibleToRoles.join(', ') : 'None' }}</strong>
                      </div>
                      <div class="ai-assignee-metrics">
                        <button
                          v-for="role in availableVisibilityRoles"
                          :key="`visibility-role-${role}`"
                          class="secondary-mini-btn"
                          type="button"
                          :disabled="!canEditTaskVisibility"
                          @click="toggleVisibleRole(role)"
                        >
                          {{ selectedTask?.visibleToRoles?.includes(role) ? 'Unselect' : 'Select' }} {{ role }}
                        </button>
                      </div>
                    </div>
                  </div>
                </div>
               <div class="p-row">
                 <div class="p-label"><i class="fa-solid fa-chart-simple"></i> Priority</div>
                 <div class="p-val">
                   <el-dropdown  trigger="click" @command="(cmd) => selectPriority(cmd)">
                     <div class="property-trigger" :class="{ 'muted-val': !selectedTask?.priority }"><i :class="getPrioIcon(selectedTask?.priority)"></i><span>Priority</span><span class="property-value">{{ getPrioLabel(selectedTask?.priority) }}</span></div>
                     <template #dropdown>
                       <el-dropdown-menu class="theme-dropdown">
                         <el-dropdown-item :command="1"><i class="fa-solid fa-angles-up mr-2" style="color: var(--color-danger)"></i> Urgent</el-dropdown-item>
                         <el-dropdown-item :command="2"><i class="fa-solid fa-chevron-up mr-2" style="color: var(--color-warning)"></i> High</el-dropdown-item>
                         <el-dropdown-item :command="3"><i class="fa-solid fa-minus mr-2" style="color: var(--color-accent)"></i> Medium</el-dropdown-item>
                         <el-dropdown-item :command="4"><i class="fa-solid fa-arrow-down mr-2" style="color: var(--color-text-muted)"></i> Low</el-dropdown-item>
                         <el-dropdown-item :command="0"><i class="fa-solid fa-ban mr-2 text-muted"></i> None</el-dropdown-item>
                       </el-dropdown-menu>
                     </template>
                   </el-dropdown>
                 </div>
               </div>
               <div class="p-row">
                 <div class="p-label"><i class="fa-solid fa-signal"></i> Story points</div>
                 <div class="p-val">
                   <div class="estimate-editor">
                     <input
                       :value="Number(selectedTask?.storyPoints ?? 0)"
                       type="number"
                       min="0"
                       max="21"
                       step="1"
                       class="estimate-hours-input"
                       @input="event => updateStoryPoints(event.target.value)"
                     />
                     <span class="estimate-unit">SP</span>
                   </div>
                 </div>
               </div>
               <div class="p-row">
                 <div class="p-label"><i class="fa-regular fa-circle-user"></i> Created by</div>
                 <div class="p-val flex items-center gap-2">
                    <div class="avatar-xxs bg-green-700 rounded-full w-5 h-5 flex-center text-white text-[10px]">{{ getCreatorName(selectedTask)[0]?.toUpperCase() || 'U' }}</div>
                    <span class="text-[13px] font-medium">{{ getCreatorName(selectedTask) }}</span>
                 </div>
               </div>
               <div class="p-row">
                 <div class="p-label"><i class="fa-regular fa-calendar"></i> Start date</div>
                 <div class="p-val">
                   <div style="position: relative; display: inline-flex;">
                     <button class="property-trigger" :class="{ 'muted-val': !selectedTask?.plannedStartDate }" @click="openPicker('detail_start')">
                       <i class="fa-regular fa-calendar"></i>
                       <span>Start date</span>
                       <span class="property-value">{{ selectedTask?.plannedStartDate || 'Add start date' }}</span>
                     </button>
                     <el-date-picker
                       :ref="el => setPickerRef('detail_start', el)"
                       v-model="selectedTask.plannedStartDate"
                       type="date"
                       format="YYYY-MM-DD"
                       value-format="YYYY-MM-DD"
                       :disabled-date="disablePastDates"
                       style="position:absolute; bottom:0; left:0; width:0; height:0; opacity:0; padding:0; border:0; display:none;"
                       @change="val => handleTaskDateChange('plannedStartDate', val)"
                     />
                   </div>
                 </div>
               </div>
               <div class="p-row">
                 <div class="p-label"><i class="fa-regular fa-calendar-check"></i> Due date</div>
                 <div class="p-val">
                   <div style="position: relative; display: inline-flex;">
                     <button class="property-trigger" :class="{ 'muted-val': !selectedTask?.dueDate }" @click="openPicker('detail_due')">
                       <i class="fa-regular fa-calendar-check"></i>
                       <span>Due date</span>
                       <span class="property-value">{{ selectedTask?.dueDate || 'Add due date' }}</span>
                     </button>
                     <el-date-picker
                       :ref="el => setPickerRef('detail_due', el)"
                       v-model="selectedTask.dueDate"
                       type="date"
                       format="YYYY-MM-DD"
                       value-format="YYYY-MM-DD"
                       :disabled-date="disableDueDates"
                       style="position:absolute; bottom:0; left:0; width:0; height:0; opacity:0; padding:0; border:0; display:none;"
                       @change="val => handleTaskDateChange('dueDate', val)"
                     />
                   </div>
                 </div>
               </div>
               <div class="p-row">
                 <div class="p-label"><i class="fa-regular fa-hourglass-half"></i> Estimate</div>
                 <div class="p-val estimate-property">
                   <div class="estimate-editor">
                     <input
                       :value="getEstimatedHours(selectedTask)"
                       type="number"
                       min="0"
                       step="0.5"
                       class="estimate-hours-input"
                       :disabled="isEstimateDerivedFromSubtasks"
                       @change="event => updateEstimatedHours(event.target.value)"
                     />
                   </div>
                   <small v-if="isEstimateDerivedFromSubtasks" class="estimate-helper-text">This parent estimate is derived from sub-work items.</small>
                   <small v-else-if="selectedTask?.estimateSourceLabel" class="estimate-helper-text">{{ selectedTask.estimateSourceLabel }}</small>
                   <div class="estimate-breakdown">
                     <div class="estimate-breakdown-row">
                       <span>Actual tracked</span>
                       <strong>{{ formatEstimateHours(getActualHours(selectedTask)) }}h</strong>
                       <small>time logs</small>
                     </div>
                     <div class="estimate-breakdown-row">
                       <span>Log time</span>
                       <div class="estimate-inline-actions">
                         <input
                           v-model="timeLogHours"
                           type="number"
                           min="0.5"
                           step="0.5"
                           class="estimate-inline-input compact"
                           :placeholder="elapsedTimeLogLabel"
                           @keydown.enter.prevent="submitTimeLog()"
                         />
                         <input
                           v-model="timeLogNote"
                           type="text"
                           class="estimate-inline-input compact wide"
                           placeholder="Note"
                           @keydown.enter.prevent="submitTimeLog()"
                         />
                         <button
                           class="secondary-mini-btn"
                           type="button"
                           :disabled="isLoggingTime || isEstimateDerivedFromSubtasks"
                           @mousedown.stop.prevent="submitTimeLog()"
                         >
                           {{ isLoggingTime ? 'Logging...' : 'Log' }}
                         </button>
                       </div>
                     </div>
                     <div class="estimate-breakdown-row">
                       <span>Work session</span>
                       <div class="estimate-inline-actions">
                         <small class="session-status-copy">{{ workSessionStatusLabel }}</small>
                         <button
                           v-if="!isWorkSessionRunning && !isWorkSessionPaused"
                           class="secondary-mini-btn"
                           type="button"
                           :disabled="isEstimateDerivedFromSubtasks || !isAssignedToCurrentUser"
                           @mousedown.stop.prevent="startWorkSession()"
                         >
                           Start
                         </button>
                         <button
                           v-if="isWorkSessionRunning"
                           class="secondary-mini-btn"
                           type="button"
                           @mousedown.stop.prevent="pauseWorkSession()"
                         >
                           Pause
                         </button>
                         <button
                           v-if="isWorkSessionPaused"
                           class="secondary-mini-btn"
                           type="button"
                           :disabled="!isAssignedToCurrentUser"
                           @mousedown.stop.prevent="resumeWorkSession()"
                         >
                           Resume
                         </button>
                         <button
                           v-if="isWorkSessionRunning || isWorkSessionPaused"
                           class="secondary-mini-btn"
                           type="button"
                           :disabled="isLoggingTime"
                           @mousedown.stop.prevent="stopWorkSession()"
                         >
                           Stop
                         </button>
                       </div>
                     </div>
                     <small v-if="!isAssignedToCurrentUser" class="estimate-helper-text">Only the assigned member can run tracked sessions on this work item.</small>
                   </div>
                   <div v-if="visibleEstimateAssigneeRows.length" class="estimate-breakdown">
                     <div class="estimate-breakdown-head">Estimate split by assignee</div>
                     <div class="estimate-breakdown-row" v-for="assignee in visibleEstimateAssigneeRows" :key="`estimate-${assignee.userId}`">
                       <span>{{ assignee.fullName || assignee.email || 'Member' }}</span>
                       <div class="estimate-inline-actions">
                         <input
                           :value="assignee.estimatedHours || 0"
                           type="number"
                           min="0"
                           step="0.5"
                           class="estimate-inline-input compact"
                           :disabled="isEstimateDerivedFromSubtasks"
                           @change="event => updateAssigneeEstimatedHours(assignee.userId, event.target.value)"
                         />
                         <strong>{{ formatEstimateHours(assignee.estimatedHours || 0) }}h</strong>
                       </div>
                       <small>{{ formatEstimateHours(getAssigneeActualHours(assignee)) }}h actual · {{ getAssigneeSharePercent(assignee) }}%</small>
                     </div>
                   </div>
                   <div v-if="subtasksList.length" class="estimate-breakdown">
                     <div class="estimate-breakdown-head">Subtask roll-up</div>
                     <div class="estimate-breakdown-row">
                       <span>{{ subtasksList.length }} sub-work items</span>
                       <strong>{{ formatEstimateHours(subtaskEstimateTotal) }}h</strong>
                       <button class="secondary-mini-btn" type="button" @click="rollupEstimateFromSubtasks">Use roll-up</button>
                     </div>
                   </div>
                   <button class="property-trigger estimate-suggestion-btn" @click="applySuggestedEstimate()">
                     <i class="fa-solid fa-wand-magic-sparkles"></i>
                     <span>Use suggestion</span>
                     <span class="property-value">{{ suggestedEstimateHours }}h</span>
                   </button>
                   <small class="estimate-helper-text">Suggested from priority, story points, and task title keywords.</small>
                   <button class="property-trigger estimate-suggestion-btn ai-estimate-btn" :disabled="isAiSuggestingEstimate" @click="suggestEstimateWithAI()">
                     <i class="fa-solid fa-robot"></i>
                     <span>{{ isAiSuggestingEstimate ? 'AI is thinking...' : 'AI suggest' }}</span>
                     <span class="property-value">{{ aiEstimateSuggestion?.suggestedHours ? `${aiEstimateSuggestion.suggestedHours}h` : 'Gemini' }}</span>
                   </button>
                   <div v-if="aiEstimateSuggestion" class="estimate-breakdown ai-suggestion-panel">
                     <div class="estimate-breakdown-head">AI estimate suggestion</div>
                     <div class="estimate-breakdown-row">
                       <span>Suggested hours</span>
                       <strong>{{ formatEstimateHours(aiEstimateSuggestion.suggestedHours) }}h</strong>
                     </div>
                     <div class="estimate-breakdown-row">
                       <span>Suggested story points</span>
                       <strong>{{ aiEstimateSuggestion.suggestedStoryPoints }}</strong>
                     </div>
                     <div class="estimate-breakdown-row">
                       <span>Complexity / days</span>
                       <strong>{{ aiEstimateSuggestion.complexity }} · {{ aiEstimateSuggestion.suggestedDays }}d</strong>
                     </div>
                     <small class="estimate-helper-text">{{ aiEstimateSuggestion.reasoning }}</small>
                     <div class="estimate-inline-actions">
                       <button class="secondary-mini-btn" type="button" @click="applyAiEstimateSuggestion()">Apply AI suggestion</button>
                     </div>
                   </div>
                 </div>
               </div>
               <div class="p-row">
                 <div class="p-label"><i class="fa-solid fa-cube"></i> Modules</div>
                 <div class="p-val">
                   <el-popover placement="bottom-start" trigger="click" popper-class="plane-popover" :width="280" @show="moduleSearch = ''">
                     <template #reference>
                       <button class="property-trigger" :class="{ 'muted-val': !selectedTask?.moduleId }">
                         <i class="fa-solid fa-cube"></i>
                         <span>Module</span>
                         <span class="property-value">{{ getModuleLabel(selectedTask?.moduleId) }}</span>
                       </button>
                     </template>
                     <div class="popover-content">
                       <input v-model="moduleSearch" type="text" class="popover-search" placeholder="Search modules..." />
                       <div class="popover-list">
                         <div class="popover-item" @click="updateTaskField(selectedTask, 'moduleId', null); selectedTask.moduleId = null">
                           <i class="fa-solid fa-cube mr-2"></i>
                           <span>No module</span>
                           <i v-if="!selectedTask?.moduleId" class="fa-solid fa-check ms-auto"></i>
                         </div>
                         <div class="popover-item" v-for="module in filteredModules" :key="module.id" @click="updateTaskField(selectedTask, 'moduleId', module.id); selectedTask.moduleId = module.id">
                           <i class="fa-solid fa-box mr-2 text-orange-500"></i>
                           <span>{{ module.name }}</span>
                           <i v-if="selectedTask?.moduleId === module.id" class="fa-solid fa-check ms-auto"></i>
                         </div>
                       </div>
                     </div>
                   </el-popover>
                 </div>
               </div>
               <div class="p-row">
                  <div class="p-label"><i class="fa-solid fa-circle-half-stroke"></i> Cycle</div>
                  <div class="p-val">
                    <el-popover placement="bottom-start" trigger="click" popper-class="plane-popover" :width="280" @show="cycleSearch = ''">
                      <template #reference>
                        <button class="property-trigger" :class="{ 'muted-val': !selectedTask?.sprintId }">
                          <i class="fa-solid fa-circle-half-stroke"></i>
                          <span>Cycle</span>
                          <span class="property-value">{{ getCycleLabel(selectedTask?.sprintId) }}</span>
                        </button>
                      </template>
                      <div class="popover-content">
                        <input v-model="cycleSearch" type="text" class="popover-search" placeholder="Search cycles..." />
                        <div class="popover-list">
                          <div class="popover-item" @click="updateTaskField(selectedTask, 'sprintId', null); selectedTask.sprintId = null">
                            <i class="fa-solid fa-circle-half-stroke mr-2"></i>
                            <span>No cycle</span>
                            <i v-if="!selectedTask?.sprintId" class="fa-solid fa-check ms-auto"></i>
                          </div>
                          <div class="popover-item" v-for="cycle in filteredCycles" :key="cycle.id" @click="updateTaskField(selectedTask, 'sprintId', cycle.id); selectedTask.sprintId = cycle.id">
                            <i class="fa-solid fa-certificate mr-2 text-blue-500"></i>
                            <span>{{ cycle.name }}</span>
                            <i v-if="selectedTask?.sprintId === cycle.id" class="fa-solid fa-check ms-auto"></i>
                          </div>
                        </div>
                      </div>
                    </el-popover>
                  </div>
               </div>
               <div class="p-row">
                  <div class="p-label"><i class="fa-solid fa-arrow-turn-up fa-rotate-90"></i> Parent</div>
                  <div class="p-val">
                    <el-popover placement="bottom-start" trigger="click" popper-class="plane-popover" :width="340" @show="parentSearch = ''">
                      <template #reference>
                        <button class="property-trigger" :class="{ 'muted-val': !currentParentId }">
                          <i class="fa-solid fa-arrow-turn-up fa-rotate-90"></i>
                          <span>Parent</span>
                          <span class="property-value">{{ currentParentLabel }}</span>
                        </button>
                      </template>
                      <div class="popover-content">
                        <input v-model="parentSearch" type="text" class="popover-search" placeholder="Search parent task..." />
                        <div class="popover-list">
                          <div class="popover-item" @click="setTaskParent(selectedTask, null)">
                            <i class="fa-solid fa-ban mr-2"></i>
                            <span>No parent</span>
                            <i v-if="!currentParentId" class="fa-solid fa-check ms-auto"></i>
                          </div>
                          <div class="popover-item" v-for="parent in filteredParents" :key="parent.id" @click="setTaskParent(selectedTask, parent.id)">
                            <span class="text-gray-500 mr-2">{{ parent.sequenceId || parent.id?.substring(0, 8) }}</span>
                            <span class="truncate flex-1">{{ parent.title }}</span>
                            <i v-if="currentParentId === parent.id" class="fa-solid fa-check ms-auto"></i>
                          </div>
                        </div>
                      </div>
                    </el-popover>
                  </div>
               </div>
               <div class="p-row">
                  <div class="p-label"><i class="fa-solid fa-tags"></i> Labels</div>
                  <div class="p-val flex flex-wrap gap-2 items-center">
                     <el-popover placement="bottom-start" trigger="click" popper-class="plane-popover" :width="280" @show="labelSearch = ''">
                       <template #reference>
                         <button class="property-trigger" :class="{ 'muted-val': !(selectedTask?.labelIds || []).length }">
                           <i class="fa-solid fa-tags"></i>
                           <span>Labels</span>
                           <span class="property-value">{{ getLabelsSummary(selectedTask?.labelIds || []) }}</span>
                         </button>
                       </template>
                       <div class="popover-content">
                         <input v-model="labelSearch" type="text" class="popover-search" placeholder="Search labels..." />
                         <div class="popover-list">
                           <div class="popover-item" v-for="label in filteredLabels" :key="label.id" @click="toggleLabelDetail(label.id)">
                             <span class="w-3 h-3 rounded-full mr-2" :style="{ backgroundColor: label.colorCode || label.color || '#3b82f6' }"></span>
                             <span>{{ label.name }}</span>
                             <i v-if="(selectedTask?.labelIds || []).includes(label.id)" class="fa-solid fa-check ms-auto"></i>
                           </div>
                           <div class="popover-item" v-if="filteredLabels.length === 0 && labelSearch" @click="createLabelDetail(labelSearch)">
                             <i class="fa-solid fa-plus mr-2"></i>
                             <span>Add "{{ labelSearch }}"</span>
                           </div>
                         </div>
                       </div>
                     </el-popover>
                  </div>
               </div>
            </div>

            <div class="flex-between mb-6" style="margin-top: 56px;">
               <h3 class="sp-section-title mb-0">Activity</h3>
               <div class="flex-center gap-2">
                  <button class="icon-filter-btn" @click="toggleActivitySort" :title="activitySortNewestFirst ? 'Newest first' : 'Oldest first'"><i class="fa-solid fa-arrow-down-short-wide"></i></button>
                  <button class="icon-filter-btn" @click="showActivityFilterInfo"><i class="fa-solid fa-bars-staggered"></i></button>
               </div>
            </div>

            <div v-if="activityEntries.length" class="activity-feed">
               <div class="feed-item">
                  <div class="feed-icon"><i class="fa-solid fa-clone"></i></div>
                  <div class="feed-text"><b>{{ getCreatorName(selectedTask) }}</b> created the work item. <span class="muted-val">{{ formatRelativeTime(selectedTask?.createdAt) }}</span></div>
               </div>

               <div v-for="entry in activityEntries" :key="entry.id" class="feed-item group">
                 <template v-if="entry.type === 'audit'">
                   <div class="feed-icon"><i class="fa-solid fa-clock-rotate-left"></i></div>
                   <div class="feed-content w-full">
                     <div class="feed-text">
                       <b>{{ entry.user || 'System' }}</b> {{ entry.summary }}
                       <span class="muted-val">{{ formatRelativeTime(entry.timestamp) }}</span>
                     </div>
                   </div>
                 </template>
                 <template v-else>
                  <div class="feed-avatar">{{ entry.comment.fullName?.[0] || 'U' }}</div>
                  <div class="feed-content w-full relative">
                    <div class="flex items-center justify-between">
                       <div>
                          <span class="font-bold text-[var(--color-text-primary)] text-[13px]">{{ entry.comment.fullName || 'User' }}</span> 
                          <span class="text-[var(--color-text-muted)] text-xs ml-2">commented {{ formatDate(entry.comment.createdAt) }} <span v-if="entry.comment.isEdited" class="italic">(edited)</span></span>
                       </div>
                       
                       <!-- Hover Actions -->
                       <div class="hidden group-hover:flex items-center gap-1 bg-[var(--bg-tertiary)] border border-[var(--border-color)] rounded p-0.5 shadow-lg absolute right-0 -top-2">
                          <el-popover  placement="bottom-end" trigger="click" popper-class="plane-popover dark !p-0" :width="320">
                             <template #reference>
                                <i class="fa-regular fa-face-smile text-[var(--color-text-muted)] hover:text-[var(--color-text-primary)] cursor-pointer px-1.5 py-1 rounded hover:bg-[var(--color-surface-hover)]"></i>
                             </template>
                             <div class="popover-content bg-[var(--bg-secondary)]">
                                <div class="p-2 border-b border-[var(--border-color)] relative">
                                  <i class="fa-solid fa-magnifying-glass absolute left-4 top-1/2 transform -translate-y-1/2 text-gray-400 text-xs"></i>
                                  <input type="text" v-model="emojiSearch" class="w-full bg-[var(--color-surface)] border border-[var(--border-color)] rounded text-[var(--color-text-primary)] py-1.5 pl-8 pr-2 text-xs focus:outline-none focus:border-blue-500" placeholder="Search..." />
                                </div>
                                <div class="p-2 text-xs font-semibold text-gray-400">Smileys & emotion</div>
                                <div class="grid grid-cols-8 gap-1 p-2 max-h-[160px] overflow-y-auto no-scrollbar">
                                  <div v-for="emoji in filteredEmojis" :key="emoji" @click="addReaction(entry.comment, emoji)" class="cursor-pointer text-lg text-center hover:bg-[var(--color-surface-hover)] rounded p-1">{{ emoji }}</div>
                                </div>
                             </div>
                          </el-popover>
                          
                          <el-dropdown  trigger="click" placement="bottom-end">
                             <i class="fa-solid fa-ellipsis text-[var(--color-text-muted)] hover:text-[var(--color-text-primary)] cursor-pointer px-1.5 py-1 rounded hover:bg-[var(--color-surface-hover)]"></i>
                             <template #dropdown>
                               <el-dropdown-menu class="dark-dropdown" style="width: 150px;">
                                 <el-dropdown-item @click="startEditingComment(entry.comment)"><i class="fa-solid fa-pen mr-2"></i> Edit</el-dropdown-item>
                                 <el-dropdown-item @click="copyCommentLink(entry.comment.id)"><i class="fa-solid fa-link mr-2"></i> Copy link</el-dropdown-item>
                                 <el-dropdown-item @click="deleteComment(entry.comment.id)" style="color: #f87171 !important;"><i class="fa-regular fa-trash-can mr-2"></i> Delete</el-dropdown-item>
                               </el-dropdown-menu>
                             </template>
                          </el-dropdown>
                       </div>
                    </div>
                    
                    <!-- Editable vs Normal -->
                    <div v-if="editingCommentId === entry.comment.id" class="mt-2">
                       <div class="editor-wrap !bg-[var(--color-surface)]">
                          <textarea class="c-input bg-transparent border-none !h-[60px]" v-model="editingContent" autofocus></textarea>
                          <div class="c-toolbar flex justify-end gap-2 p-2">
                             <button class="px-3 py-1.5 text-xs rounded border border-[var(--color-border)] text-[var(--color-text-secondary)] hover:bg-[var(--color-surface-hover)] transition" @click="cancelEditingComment">Cancel</button>
                             <button class="px-3 py-1.5 text-xs rounded bg-blue-600 text-white hover:bg-blue-700 transition" @click="saveEditedComment(entry.comment.id, entry.comment)">Update</button>
                          </div>
                       </div>
                    </div>
                    <div v-else>
                       <div class="mt-1 text-[14px] text-[var(--color-text-secondary)] format-comment-content" v-html="formatCommentDisplay(entry.comment.content)"></div>
                        <div v-if="entry.comment.attachments?.length" class="comment-attachments">
                           <button
                             v-for="attachment in entry.comment.attachments"
                             :key="attachment.id"
                             type="button"
                             class="comment-attachment-chip"
                             @click="handleAttachmentOpen(attachment, entry.comment)"
                           >
                             <img v-if="isImageAttachment(attachment)" :src="resolveFileUrl(attachment.fileUrl)" :alt="attachment.fileName" class="comment-image-thumb" />
                             <i v-else class="fa-solid fa-paperclip"></i>
                             <span>{{ attachment.fileName }}</span>
                             <i v-if="isImageAttachment(attachment)" class="fa-regular fa-eye"></i>
                             <i v-else class="fa-solid fa-download"></i>
                           </button>
                        </div>
                       
                       <!-- Reactions -->
                       <div class="flex flex-wrap gap-2 mt-2" v-if="entry.comment.reactions && Object.keys(entry.comment.reactions).length > 0">
                          <div v-for="(count, emoji) in entry.comment.reactions" :key="emoji" class="flex items-center gap-1.5 bg-[var(--color-surface-hover)] border border-[var(--color-border)] rounded-full px-2.5 py-0.5 cursor-pointer hover:bg-[var(--color-surface-hover)] transition-colors" @click="addReaction(entry.comment, emoji)">
                             <span class="text-sm mt-px">{{ emoji }}</span> <span class="text-xs text-blue-400 font-medium">{{ count }}</span>
                          </div>
                       </div>
                    </div>
                  </div>
                 </template>
               </div>
            </div>
            <div v-else class="activity-empty-state">No activity yet.</div>

            <div class="comment-box">
               <p class="text-[13px] font-semibold mb-2 text-[var(--color-text-muted)]">Add comment</p>
               <div class="editor-wrap !pt-2">
                  <div v-if="pendingAttachments.length > 0" class="px-3 pb-2 flex flex-wrap gap-2">
                     <div v-for="(file, idx) in pendingAttachments" :key="idx" class="flex items-center gap-1.5 bg-[var(--color-surface-hover)] border border-[var(--color-border)] rounded px-2 py-1 text-xs text-[var(--color-text-secondary)]">
                        <i class="fa-regular fa-file-lines text-[var(--color-text-muted)]"></i>
                        <span class="max-w-[150px] truncate">{{ file.name }}</span>
                        <i class="fa-solid fa-xmark ml-1 cursor-pointer hover:text-red-400" @click="pendingAttachments.splice(idx, 1)"></i>
                     </div>
                  </div>
                  <div
                    ref="commentEditor"
                    class="c-input rich-editor comment-editor !pt-0"
                    contenteditable
                    data-placeholder="Click to add comment..."
                    @focus="activeEditor = 'comment'"
                    @keydown="handleEditorKeydown($event, 'comment')"
                    @input="handleCommentEditorInput"
                  ></div>
                  <input ref="commentImageInput" type="file" accept=".png,.jpg,.jpeg,.webp,.gif,.svg,image/*" style="display:none" multiple @change="handleCommentFileChange($event, true)" />
                  <input ref="commentFileInput" type="file" accept=".pdf,.doc,.docx,.xls,.xlsx,.csv,.txt,.zip,.rar,.ppt,.pptx" style="display:none" multiple @change="handleCommentFileChange($event, false)" />
                  <div class="c-toolbar">
                     <div class="ct-left">
                       <!-- Group 1: Text -->
                       <i class="fa-solid fa-bold icon-hover" @mousedown.prevent="execEditorCommand('bold', null, 'comment')"></i> 
                       <i class="fa-solid fa-italic icon-hover" @mousedown.prevent="execEditorCommand('italic', null, 'comment')"></i> 
                       <i class="fa-solid fa-underline icon-hover" @mousedown.prevent="execEditorCommand('underline', null, 'comment')"></i> 
                       <i class="fa-solid fa-strikethrough icon-hover" @mousedown.prevent="execEditorCommand('strikeThrough', null, 'comment')"></i>
                       
                       <div class="toolbar-sep"></div>
                       
                       <!-- Group 2: Code -->
                       <i class="fa-solid fa-code icon-hover" @mousedown.prevent="wrapSelectionWithInlineCode('comment')"></i>
                       <i class="fa-solid fa-file-code icon-hover" :class="{ 'is-active': codeMode.comment }" @mousedown.prevent="toggleCodeBlockMode('comment')"></i>
                       
                       <div class="toolbar-sep"></div>
                       
                       <!-- Group 3: List -->
                       <i class="fa-solid fa-list-ul icon-hover" @mousedown.prevent="execEditorCommand('insertUnorderedList', null, 'comment')"></i> 
                       <i class="fa-solid fa-list-ol icon-hover" @mousedown.prevent="execEditorCommand('insertOrderedList', null, 'comment')"></i> 
                       
                       <div class="toolbar-sep"></div>
                       
                       <!-- Group 4: Insert -->
                       <i class="fa-regular fa-image icon-hover" @mousedown.prevent="triggerCommentImageUpload"></i> 
                       <i class="fa-solid fa-paperclip icon-hover" @mousedown.prevent="triggerCommentFileUpload"></i>
                     </div>
                     <button class="c-submit" :disabled="!commentHasContent" @click="submitComment">Comment</button>
                  </div>
               </div>
            </div>
         </div>
      </div>
      
    </div>
  </transition>

  <div v-if="previewImage" class="image-lightbox" @click.self="previewImage = null">
    <div class="image-lightbox-panel">
      <button class="lightbox-close" @click="previewImage = null"><i class="fa-solid fa-xmark"></i></button>
      <img :src="previewImage.url" :alt="previewImage.fileName" :style="{ transform: `scale(${previewZoom})` }" />
      <div class="lightbox-footer">
        <span>{{ previewImage.fileName }}</span>
        <div class="lightbox-actions">
          <label class="zoom-control">
            <i class="fa-solid fa-up-right-and-down-left-from-center"></i>
            <input v-model="previewZoom" type="range" min="1" max="3" step="0.1" />
          </label>
          <button class="lightbox-delete" @click="removePreviewAttachment"><i class="fa-regular fa-trash-can"></i> Delete</button>
          <a :href="previewImage.url" :download="previewImage.fileName" class="download-btn">
            <i class="fa-solid fa-download"></i> Download
          </a>
        </div>
      </div>
    </div>
  </div>
</template>


<script setup>
import { ref, watch, computed, nextTick, onMounted, onUnmounted } from 'vue';
import { ElMessage, ElNotification } from 'element-plus';
import axiosClient from '@/api/axiosClient';
import DOMPurify from 'dompurify';
import { subscribeAdminRealtime } from '@/utils/adminRealtime';
import { getStoredUser, hasSystemAdminAccess, normalizeProjectRole } from '@/utils/permissions';
import { useProjectStore } from '@/store/useProjectStore';
import {
  buildFreshWorkSession,
  calculateWorkSessionHours,
  clearWorkSession,
  loadWorkSession,
  saveWorkSession
} from '@/utils/workSession';

const aiManagerProjectRoles = ['pm', 'po', 'sm', 'admin', 'project_manager', 'project_lead', 'scrum_master'];

const props = defineProps({
  selectedTask: { type: Object, default: null },
  projectId: { type: [String, Number], required: true },
  projectMembers: { type: Array, default: () => [] },
  currentUser: { type: Object, default: () => ({}) },
  currentProjectRole: { type: String, default: '' },
  canGoBack: { type: Boolean, default: false }
});

const emit = defineEmits(['updateTask', 'close', 'back', 'open-task', 'create-subtask', 'refresh-tasks']);
const projectStore = useProjectStore();

const showTaskModal = ref(true);
const isSubscribed = ref(false);
const activitySortNewestFirst = ref(true);
const showSubtasks = ref(true);
const WORK_SESSION_IDLE_TIMEOUT_MS = 5 * 60 * 1000;
const workSession = ref(null);
const workSessionNow = ref(Date.now());
const workSessionTick = ref(null);
const idleNotificationShownForTaskId = ref(null);

const toBooleanFlag = (value) => {
    if (typeof value === 'boolean') return value;
    if (typeof value === 'string') {
        const normalized = value.trim().toLowerCase();
        if (normalized === 'true') return true;
        if (normalized === 'false') return false;
    }
    if (typeof value === 'number') return value !== 0;
    return Boolean(value);
};

const discardNewTask = () => {
    showTaskModal.value = false;
};

// ====================== CREATE TASK POPOVER REFS ======================
const createMore = ref(false);
const assigneeSearch = ref('');
const labelSearch = ref('');
const cycleSearch = ref('');
const moduleSearch = ref('');
const parentSearch = ref('');
const statusSearch = ref('');

const projectCycles = ref([]);
const projectModules = ref([]);
const projectMemberOptions = ref([]);
const projectStatuses = ref([]);
const projectExecutionRules = ref({
  enableRoleBasedTaskVisibility: false,
  managerAlwaysSeeAllTasks: true,
  defaultTaskVisibilityMode: 'project',
  defaultBaseHours: 4,
  hoursPerStoryPoint: 2,
  estimateBaselineMode: 'role_then_project',
  roleHourMultipliers: {}
});
const DATE_ONLY_PATTERN = /^\d{4}-\d{2}-\d{2}$/;

const parseDateValue = (value) => {
  if (!value) return null;
  if (value instanceof Date) return new Date(value);
  if (typeof value === 'string' && DATE_ONLY_PATTERN.test(value)) {
    const [year, month, day] = value.split('-').map(Number);
    return new Date(year, month - 1, day);
  }
  if (typeof value === 'string' && /^\d{4}-\d{2}-\d{2}T/.test(value)) {
    const [year, month, day] = value.slice(0, 10).split('-').map(Number);
    return new Date(year, month - 1, day);
  }

  const parsed = new Date(value);
  return Number.isNaN(parsed.getTime()) ? null : parsed;
};

const formatDateOnly = (value) => {
  const date = parseDateValue(value);
  if (!date) return null;
  const year = date.getFullYear();
  const month = `${date.getMonth() + 1}`.padStart(2, '0');
  const day = `${date.getDate()}`.padStart(2, '0');
  return `${year}-${month}-${day}`;
};

const getTodayDateString = () => formatDateOnly(new Date());
const getParentId = (task = props.selectedTask) => task?.parentId || task?.parentTaskId || null;

const currentProjectBadge = computed(() => {
  const sequencePrefix = props.selectedTask?.sequenceId?.split('-')?.[0]
    || cachedProjectTasks.value[0]?.sequenceId?.split('-')?.[0];
  if (sequencePrefix) return sequencePrefix;
  return `${props.projectId || 'WORK'}`.slice(0, 6).toUpperCase();
});

const disablePastDates = (date) => {
  return false;
};

const disableDueDates = (date) => {
  const candidate = formatDateOnly(date);
  const start = formatDateOnly(props.selectedTask?.plannedStartDate);
  return Boolean(candidate && start && candidate < start);
};

const filteredMembers = computed(() => {
    const members = projectMemberOptions.value;
    if (!assigneeSearch.value) return members;
    return members.filter(m => (m.fullName || m.name || m.email || '').toLowerCase().includes(assigneeSearch.value.toLowerCase()));
});

const currentProjectRole = computed(() => {
  const currentUser = props.currentUser && Object.keys(props.currentUser).length
    ? props.currentUser
    : getStoredUser();
  const currentUserIdValue = currentUser?.id || currentUser?.userId;
  const matchedMember = projectMemberOptions.value
    .find(member => `${member.userId || member.id || ''}` === `${currentUserIdValue || ''}`);
  const membershipRole = matchedMember?.projectRole || matchedMember?.ProjectRole;

  if (props.currentProjectRole) {
    return normalizeProjectRole(props.currentProjectRole);
  }

  const currentProjectMatch = `${projectStore.currentProject?.id || ''}` === `${props.projectId || ''}`
    ? projectStore.currentProject
    : null;
  const role = membershipRole
    || currentProjectMatch?.myRole
    || currentProjectMatch?.MyRole
    || currentProjectMatch?.projectRole
    || currentProjectMatch?.ProjectRole;

  return normalizeProjectRole(role);
});

const canUseAiAssigneeSuggestion = computed(() => {
  const currentUser = props.currentUser && Object.keys(props.currentUser).length
    ? props.currentUser
    : getStoredUser();

  if (hasSystemAdminAccess(currentUser)) {
    return true;
  }

  return Boolean(currentProjectRole.value && aiManagerProjectRoles.includes(currentProjectRole.value));
});

const canManageTaskAssignees = computed(() => canUseAiAssigneeSuggestion.value);
const isRoleVisibilityEnabled = computed(() => Boolean(projectExecutionRules.value?.enableRoleBasedTaskVisibility));
const canEditTaskVisibility = computed(() => canUseAiAssigneeSuggestion.value && isRoleVisibilityEnabled.value);

const availableVisibilityRoles = computed(() => {
  return Array.from(new Set(
    projectMemberOptions.value
      .map(member => normalizeProjectRole(member.projectRole || member.ProjectRole))
      .filter(Boolean)
  ));
});

const getMemberProjectRole = (userId) => {
  const member = projectMemberOptions.value.find(item => item.userId === userId);
  return normalizeProjectRole(member?.projectRole || member?.ProjectRole);
};

const calculateBaselineEstimate = (task = props.selectedTask) => {
  if (!task) return 0;
  const rules = projectExecutionRules.value || {};
  const storyPoints = Number(task.storyPoints || 0);
  const priority = Number(task.priority || 0);
  const assigneeIds = getAssigneeIds(task);
  const primaryRole = assigneeIds.map(getMemberProjectRole).find(Boolean) || currentProjectRole.value || 'developer';
  let hours = storyPoints > 0
    ? storyPoints * Number(rules.hoursPerStoryPoint || 2)
    : Number(rules.defaultBaseHours || 4);

  if (priority === 1) hours += 4;
  else if (priority === 2) hours += 2;

  const title = String(task.title || '').toLowerCase();
  if (/(api|integration|migration|security|payment)/.test(title)) hours += 2.5;
  if (/(bug|fix|hotfix)/.test(title)) hours += 1;

  const roleMultiplier = Number(rules.roleHourMultipliers?.[primaryRole] ?? 1);
  hours *= roleMultiplier > 0 ? roleMultiplier : 1;

  return Math.round(Math.max(0.5, Math.min(80, hours)) * 10) / 10;
};

const applyProjectDefaultsToTask = (task = props.selectedTask, options = {}) => {
  if (!task) return;

  if (!task.visibilityMode) {
    task.visibilityMode = projectExecutionRules.value.defaultTaskVisibilityMode || 'project';
  }

  if (!Array.isArray(task.visibleToRoles)) {
    task.visibleToRoles = [];
  }

  if (task.visibilityMode === 'role' && task.visibleToRoles.length === 0) {
    const fallbackRole = currentProjectRole.value || getMemberProjectRole(getAssigneeIds(task)[0]);
    if (fallbackRole) {
      task.visibleToRoles = [fallbackRole];
    }
  }

  const shouldApplyBaseline = options.forceEstimate || (task.isNew && !Number(task.totalEstimatedHours || 0))
  if (shouldApplyBaseline && (!subtasksList.value || !subtasksList.value.length)) {
    task.totalEstimatedHours = calculateBaselineEstimate(task);
    task.estimateSourceLabel = 'Suggested from project baseline';
  }
};

const selectVisibilityMode = (mode, task = props.selectedTask) => {
  if (!task || !canEditTaskVisibility.value) return;
  task.visibilityMode = mode;
  if (mode !== 'role') {
    task.visibleToRoles = [];
  } else if (!task.visibleToRoles?.length && currentProjectRole.value) {
    task.visibleToRoles = [currentProjectRole.value];
  }

  if (!task.isNew) {
    updateTaskFields(task, {
      visibilityMode: task.visibilityMode,
      visibleToRoles: task.visibleToRoles || []
    });
  }
};

const toggleVisibleRole = (role, task = props.selectedTask) => {
  if (!task || !canEditTaskVisibility.value) return;
  const nextRole = normalizeProjectRole(role);
  const currentRoles = Array.isArray(task.visibleToRoles) ? [...task.visibleToRoles] : [];
  task.visibleToRoles = currentRoles.includes(nextRole)
    ? currentRoles.filter(item => item !== nextRole)
    : [...currentRoles, nextRole];

  if (!task.isNew) {
    updateTaskFields(task, {
      visibilityMode: task.visibilityMode || 'role',
      visibleToRoles: task.visibleToRoles
    });
  }
};

const getVisibilityLabel = (task = props.selectedTask) => {
  const mode = task?.visibilityMode || 'project';
  if (mode === 'assigned') return 'Assigned only';
  if (mode === 'role') {
    return task?.visibleToRoles?.length
      ? `Role scoped (${task.visibleToRoles.join(', ')})`
      : 'Role scoped';
  }
  return 'Project members';
};

const filteredLabels = computed(() => {
    if (!labelSearch.value) return projectLabels.value;
    return projectLabels.value.filter(l => l.name?.toLowerCase().includes(labelSearch.value.toLowerCase()));
});

const filteredCycles = computed(() => {
    if (!cycleSearch.value) return projectCycles.value;
    return projectCycles.value.filter(c => c.name?.toLowerCase().includes(cycleSearch.value.toLowerCase()));
});

const filteredModules = computed(() => {
    if (!moduleSearch.value) return projectModules.value;
    return projectModules.value.filter(m => m.name?.toLowerCase().includes(moduleSearch.value.toLowerCase()));
});

const filteredParents = computed(() => {
    let tasks = cachedProjectTasks.value.filter(t =>
      t.projectId === props.projectId &&
      t.id !== props.selectedTask?.id &&
      !getParentId(t)
    );
    if (!parentSearch.value) return tasks;
    return tasks.filter(t => t.title?.toLowerCase().includes(parentSearch.value.toLowerCase()) || t.sequenceId?.toLowerCase().includes(parentSearch.value.toLowerCase()));
});

const filteredStatuses = computed(() => {
    if (!statusSearch.value) return projectStatuses.value;
    return projectStatuses.value.filter(status => status.displayName?.toLowerCase().includes(statusSearch.value.toLowerCase()));
});

const getPrioLabel = (p) => {
    if (p===1) return 'Urgent';
    if (p===2) return 'High';
    if (p===3) return 'Medium';
    if (p===4) return 'Low';
    return 'None';
};

const getPrioIcon = (p) => {
    if (p===1) return 'fa-solid fa-angles-up';
    if (p===2) return 'fa-solid fa-chevron-up';
    if (p===3) return 'fa-solid fa-minus';
    if (p===4) return 'fa-solid fa-arrow-down';
    return 'fa-solid fa-ban';
};

const getStatusIcon = (s) => {
    const st = (s||'').toUpperCase();
    if(st.includes('CANCEL')) return 'fa-regular fa-circle-xmark';
    if(st.includes('DONE')) return 'fa-regular fa-circle-check';
    if(st.includes('PROGRESS')) return 'fa-solid fa-circle-half-stroke';
    if(st.includes('REVIEW')) return 'fa-regular fa-circle-play';
    if(st.includes('TODO')) return 'fa-regular fa-circle';
    return 'fa-solid fa-circle-dashed';
};

const getStatusLabel = (statusName) => statusName || 'State';
const normalizeStatusName = (statusName) => {
    const upper = (statusName || '').toUpperCase().replace(/\s+/g, '');
    if (upper.includes('CANCEL')) return 'CANCELLED';
    if (upper.includes('DONE') || upper.includes('COMPLETE')) return 'DONE';
    if (upper.includes('PROGRESS') || upper.includes('ACTIVE')) return 'IN PROGRESS';
    if (upper.includes('TODO')) return 'TO DO';
    return 'BACKLOG';
};

const getAssigneeLabel = (id) => {
   if (!id) return 'Assignees';
   const user = projectMemberOptions.value.find(m => m.userId === id);
   return user ? (user.fullName || user.name || user.email || 'Assignees') : 'Assignees';
};

const getAssigneeIds = (task = props.selectedTask) => {
   if (!task) return [];
   return Array.from(new Set([
     ...(Array.isArray(task.assigneeIds) ? task.assigneeIds : []),
     ...(Array.isArray(task.assignees) ? task.assignees.map(item => item.userId || item.id).filter(Boolean) : []),
     ...(task.assignedUserId ? [task.assignedUserId] : []),
     ...(task.assigneeId ? [task.assigneeId] : [])
   ]));
};

const buildTaskAssigneeRows = (task = props.selectedTask) => {
   const selectedIds = getAssigneeIds(task);
   return selectedIds.map(id => {
      const existing = task?.assignees?.find(item => (item.userId || item.id) === id) || {};
      const member = projectMemberOptions.value.find(item => item.userId === id) || {};
      return {
         userId: id,
         fullName: existing.fullName || member.fullName || member.name,
         email: existing.email || member.email,
         progressPercent: existing.progressPercent ?? 0,
         contributionWeight: existing.contributionWeight ?? 1,
         estimatedHours: Number(existing.estimatedHours ?? existing.EstimatedHours ?? 0),
         totalActualHours: Number(existing.totalActualHours ?? existing.TotalActualHours ?? 0)
      };
   });
};

const selectedAssigneeRows = computed(() => buildTaskAssigneeRows());

const subtaskAssigneeRows = computed(() => {
   const rows = new Map();
   (subtasksList.value || []).forEach(subtask => {
      (Array.isArray(subtask.assignees) ? subtask.assignees : []).forEach(assignee => {
         const userId = assignee.userId || assignee.UserId || assignee.id;
         if (!userId) return;
         const current = rows.get(userId) || {
            userId,
            fullName: assignee.fullName || assignee.FullName || assignee.name,
            email: assignee.email || assignee.Email,
            progressPercent: 0,
            contributionWeight: 0,
            estimatedHours: 0,
            totalActualHours: 0
         };
         current.progressPercent = Math.max(current.progressPercent, Number(assignee.progressPercent ?? assignee.ProgressPercent ?? 0) || 0);
         current.contributionWeight += Number(assignee.contributionWeight ?? assignee.ContributionWeight ?? 1) || 0;
         current.estimatedHours += Number(assignee.estimatedHours ?? assignee.EstimatedHours ?? 0) || 0;
         current.totalActualHours += Number(assignee.totalActualHours ?? assignee.TotalActualHours ?? 0) || 0;
         rows.set(userId, current);
      });
   });
   return Array.from(rows.values()).map(row => ({
      ...row,
      estimatedHours: Math.round(row.estimatedHours * 10) / 10,
      totalActualHours: Math.round(row.totalActualHours * 10) / 10
   }));
});

const visibleEstimateAssigneeRows = computed(() => {
   if (isEstimateDerivedFromSubtasks.value && subtaskAssigneeRows.value.length) {
      return subtaskAssigneeRows.value;
   }
   return selectedAssigneeRows.value;
});

const getAssigneeSummary = (task = props.selectedTask) => {
   const members = buildTaskAssigneeRows(task);
   if (!members.length) return 'Assignees';
   if (members.length === 1) return members[0].fullName || members[0].email || 'Assignee';
   return `${members.length} assignees`;
};

const getTaskProgressPercent = (task = props.selectedTask) => {
   const rows = buildTaskAssigneeRows(task);
   if (!rows.length) {
      return normalizeStatusName(task?.statusName) === 'DONE' ? 100 : 0;
   }

   const total = rows.reduce((sum, assignee) => sum + (Number(assignee.progressPercent) || 0), 0);
   return Math.round(total / rows.length);
};

const canEditTaskProgress = () => false;

const setTaskParent = (task, parentId) => {
   if (!task) return;
   task.parentId = parentId;
   task.parentTaskId = parentId;
   if (!task.isNew) {
      updateTaskField(task, 'parentId', parentId);
   }
};

const currentParentId = computed(() => getParentId(props.selectedTask));
const currentParentLabel = computed(() => getParentLabel(currentParentId.value));

const openParentTask = () => {
   if (!currentParentId.value) return;
   const parentTask = cachedProjectTasks.value.find(task => task.id === currentParentId.value);
   if (parentTask) {
      openTaskDetail(parentTask);
   }
};

const getCycleLabel = (id) => {
   if (!id) return 'Cycle';
   const c = projectCycles.value.find(c => c.id === id);
   return c ? c.name : 'Cycle';
};

const getModuleLabel = (id) => {
   if (!id) return 'Modules';
   const m = projectModules.value.find(m => m.id === id);
   return m ? m.name : 'Modules';
};

const getParentLabel = (id) => {
   if (!id) return 'Add parent work item';
   const parent = cachedProjectTasks.value.find(task => task.id === id);
   if (parent) {
      return `${parent.sequenceId || parent.id?.substring(0, 8)} ${parent.title}`;
   }

   const fallbackTitle = props.selectedTask?.parentTaskTitle || props.selectedTask?.parentTitle || props.selectedTask?.parentName;
   return fallbackTitle ? fallbackTitle : 'Parent selected';
};

const getLabelsSummary = (labelIds) => {
   if (!labelIds?.length) return 'Labels';
   if (labelIds.length === 1) {
      return projectLabels.value.find(label => label.id === labelIds[0])?.name || '1 label';
   }
   return `${labelIds.length} labels`;
};

const toggleSelectedLabel = (lId) => {
    if (!props.selectedTask) return;
    if (!props.selectedTask.labelIds) props.selectedTask.labelIds = [];
    if (props.selectedTask.labelIds.includes(lId)) {
        props.selectedTask.labelIds = props.selectedTask.labelIds.filter(id => id !== lId);
    } else {
        props.selectedTask.labelIds.push(lId);
    }
};
const toggleSelectedLabelDetail = (lId) => {
    if (!props.selectedTask) return;
    if (!props.selectedTask.labelIds) props.selectedTask.labelIds = [];
    let newArr = [...props.selectedTask.labelIds];
    if (newArr.includes(lId)) {
        newArr = newArr.filter(id => id !== lId);
    } else {
        newArr.push(lId);
    }
    emit('updateTask', props.selectedTask, 'labelIds', newArr);
    props.selectedTask.labelIds = newArr;
};

const createLabel = async (name) => {
    try {
        const res = await axiosClient.post(`/projects/${props.projectId}/labels`, { name, color: '#3b82f6' });
        const newL = res.data?.data || res.data;
        if(newL) {
            projectLabels.value.push(newL);
            toggleSelectedLabel(newL.id);
            labelSearch.value = '';
        }
    } catch(e) {}
};
const createLabelDetail = async (name) => {
    try {
        const res = await axiosClient.post(`/projects/${props.projectId}/labels`, { name, color: '#3b82f6' });
        const newL = res.data?.data || res.data;
        if(newL) {
            projectLabels.value.push(newL);
            if (props.selectedTask?.id && !props.selectedTask?.isNew) {
                await toggleLabelDetail(newL.id);
            } else {
                toggleSelectedLabelDetail(newL.id);
            }
            labelSearch.value = '';
        }
    } catch(e) {}
};

// ====================== RICH EDITOR REFS ======================
const pickerRefs = ref({});
const commentEditor = ref(null);
const descriptionEditor = ref(null);
const commentImageInput = ref(null);
const commentFileInput = ref(null);
const descriptionImageInput = ref(null);
const descriptionFileInput = ref(null);
const activeEditor = ref('comment');
const previewImage = ref(null);
const previewZoom = ref(1);
const savedSelection = ref({ description: null, comment: null });
const codeMode = ref({ description: false, comment: false });
const auditEntries = ref([]);
const apiRoot = (import.meta.env.VITE_API_BASE_URL || 'http://localhost:5136/api').replace(/\/api\/?$/, '');
const showFormatToolbar = ref(false);
const toolbarPosition = ref({ x: 260, y: 120 });
const textColors = ['#F8FAFC', '#EF4444', '#F97316', '#22C55E', '#06B6D4', '#3B82F6', '#8B5CF6', '#F472B6'];
const backgroundColors = ['#27272A', '#7F1D1D', '#78350F', '#064E3B', '#164E63', '#1E3A8A', '#4C1D95', '#831843'];

const setPickerRef = (key, el) => {
  if (el) pickerRefs.value[key] = el;
};

const openPicker = (key) => {
  const picker = pickerRefs.value[key];
  if (picker?.handleOpen) picker.handleOpen();
  else if (picker?.focus) picker.focus();
};

const saveEditorSelection = (editorName = activeEditor.value) => {
  const selection = window.getSelection();
  if (!selection || !selection.rangeCount) return;

  const target = editorName === 'description' ? descriptionEditor.value : commentEditor.value;
  const range = selection.getRangeAt(0);
  if (target?.contains(range.commonAncestorContainer)) {
    savedSelection.value[editorName] = range.cloneRange();
  }
};

const restoreEditorSelection = (editorName = activeEditor.value) => {
  const range = savedSelection.value[editorName];
  const target = editorName === 'description' ? descriptionEditor.value : commentEditor.value;
  if (!target) return;

  target.focus();
  if (!range) return;

  const selection = window.getSelection();
  if (!selection) return;
  selection.removeAllRanges();
  selection.addRange(range);
};

const focusEditor = (editorName) => {
  activeEditor.value = editorName;
  const target = editorName === 'description' ? descriptionEditor.value : commentEditor.value;
  target?.focus();
};

const getActiveEditorElement = () => activeEditor.value === 'description' ? descriptionEditor.value : commentEditor.value;

const unwrapFormattingNode = (node) => {
  const parent = node?.parentNode;
  if (!parent) return;
  while (node.firstChild) {
    parent.insertBefore(node.firstChild, node);
  }
  parent.removeChild(node);
};

const toggleInlineFormat = (editorName, tagName) => {
  restoreEditorSelection(editorName);
  const selection = window.getSelection();
  if (!selection || !selection.rangeCount) return;

  const range = selection.getRangeAt(0);
  if (range.collapsed) return;

  const anchorNode = selection.anchorNode?.nodeType === Node.ELEMENT_NODE
    ? selection.anchorNode
    : selection.anchorNode?.parentElement;
  const existingTag = anchorNode?.closest?.(tagName);

  if (existingTag) {
    unwrapFormattingNode(existingTag);
    syncEditorModel(editorName);
    saveEditorSelection(editorName);
    return;
  }

  const wrapper = document.createElement(tagName);
  const fragment = range.extractContents();
  wrapper.appendChild(fragment);
  range.insertNode(wrapper);
  range.selectNodeContents(wrapper);
  selection.removeAllRanges();
  selection.addRange(range);
  syncEditorModel(editorName);
  saveEditorSelection(editorName);
};

const execEditorCommand = (command, value = null, editorName = activeEditor.value) => {
  const editor = editorName === 'description' ? descriptionEditor.value : commentEditor.value;
  if (!editor) return;
  activeEditor.value = editorName;
  const inlineCommandMap = {
    bold: 'strong',
    italic: 'em',
    underline: 'u',
    strikeThrough: 's'
  };

  if (inlineCommandMap[command]) {
    toggleInlineFormat(editorName, inlineCommandMap[command]);
    return;
  }

  restoreEditorSelection(editorName);
  document.execCommand(command, false, value);
  saveEditorSelection(editorName);
  syncEditorModel(editorName);
};

const showSelectionToolbar = () => {
  activeEditor.value = 'description';
  const selection = window.getSelection();
  if (!selection || !selection.rangeCount || selection.toString().trim().length === 0) {
    showFormatToolbar.value = false;
    return;
  }

  saveEditorSelection('description');
  const range = selection.getRangeAt(0);
  const rect = range.getBoundingClientRect();
  toolbarPosition.value = {
    x: Math.max(12, rect.left + window.scrollX),
    y: Math.max(12, rect.top + window.scrollY - 54)
  };
  showFormatToolbar.value = true;
};

const applyBlockFormat = (tagName) => {
  execEditorCommand('formatBlock', tagName, 'description');
};

const applyTextColor = (color) => {
  execEditorCommand('foreColor', color, 'description');
};

const applyBackgroundColor = (color) => {
  execEditorCommand('hiliteColor', color, 'description');
};

const insertNodeAtSelection = (node) => {
  const editor = getActiveEditorElement();
  const selection = window.getSelection();
  if (!editor || !selection || !selection.rangeCount) return;
  const range = selection.getRangeAt(0);
  range.deleteContents();
  range.insertNode(node);
  range.setStartAfter(node);
  range.collapse(true);
  selection.removeAllRanges();
  selection.addRange(range);
  syncEditorModel(activeEditor.value);
};

const wrapSelectionWithInlineCode = (editorName = activeEditor.value) => {
  restoreEditorSelection(editorName);
  const code = document.createElement('code');
  code.className = 'comment-inline-code';
  code.textContent = window.getSelection()?.toString() || 'code';
  insertNodeAtSelection(code);
  saveEditorSelection(editorName);
};

const wrapSelectionWithBlock = (tagName, editorName = activeEditor.value) => {
  restoreEditorSelection(editorName);
  const block = document.createElement(tagName);
  if (tagName === 'pre') {
    const code = document.createElement('code');
    code.textContent = window.getSelection()?.toString() || 'const example = true;';
    block.className = 'comment-code-block';
    block.appendChild(code);
  } else {
    block.textContent = window.getSelection()?.toString() || '';
  }
  insertNodeAtSelection(block);
  saveEditorSelection(editorName);
};

const toggleCodeBlockMode = (editorName) => {
  activeEditor.value = editorName;
  restoreEditorSelection(editorName);
  const selection = window.getSelection();
  const anchor = selection?.anchorNode;
  const container = anchor?.nodeType === Node.ELEMENT_NODE ? anchor : anchor?.parentElement;
  const existingPre = container?.closest?.('pre');

  if (existingPre) {
    const replacement = document.createElement('p');
    replacement.innerHTML = existingPre.querySelector('code')?.innerHTML || existingPre.innerHTML || '<br />';
    existingPre.replaceWith(replacement);
    codeMode.value = { ...codeMode.value, [editorName]: false };
    syncEditorModel(editorName);
    return;
  }

  wrapSelectionWithBlock('pre', editorName);
  codeMode.value = { ...codeMode.value, [editorName]: true };
};

const handleEditorKeydown = (event, editorName) => {
  activeEditor.value = editorName;
  saveEditorSelection(editorName);

  if (event.key === 'Escape' && editorName === 'description') {
    showFormatToolbar.value = false;
    return;
  }

  if (event.key !== 'Enter') return;

  const selection = window.getSelection();
  const anchor = selection?.anchorNode;
  const container = anchor?.nodeType === Node.ELEMENT_NODE ? anchor : anchor?.parentElement;
  if (codeMode.value[editorName] || container?.closest?.('pre')) {
    event.preventDefault();
    document.execCommand('insertHTML', false, '\n');
    syncEditorModel(editorName);
  }
};

const syncEditorModel = (editorName) => {
  if (editorName === 'description') {
    if (props.selectedTask) {
      props.selectedTask.description = descriptionEditor.value?.innerHTML || '';
    }
    return;
  }

  newComment.value = commentEditor.value?.innerHTML || '';
};

const sanitizeRichText = (html) => DOMPurify.sanitize(html || '', {
  ALLOWED_TAGS: ['p', 'br', 'div', 'span', 'strong', 'b', 'em', 'i', 'u', 's', 'code', 'pre', 'ul', 'ol', 'li', 'blockquote', 'a', 'img'],
  ALLOWED_ATTR: ['href', 'target', 'rel', 'src', 'alt', 'class', 'style']
});

const formatCommentDisplay = (text) => {
  if(!text) return '';
  if (/<[a-z][\s\S]*>/i.test(text)) {
    return `<div class="comment-rendered">${sanitizeRichText(text)}</div>`;
  }
  let res = text.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
  res = res.replace(/```([\s\S]*?)```/g, '<pre class="comment-code-block"><code>$1</code></pre>');
  res = res.replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>');
  res = res.replace(/\*(.*?)\*/g, '<em>$1</em>');
  res = res.replace(/~~(.*?)~~/g, '<s>$1</s>');
  res = res.replace(/&lt;u&gt;(.*?)&lt;\/u&gt;/g, '<u>$1</u>');
  res = res.replace(/`([^`]+)`/g, '<code class="comment-inline-code">$1</code>');
  res = res.replace(/&lt;div style="text-align:(left|center|right)"&gt;([\s\S]*?)&lt;\/div&gt;/g, '<div style="text-align:$1">$2</div>');

  const lines = res.split('\n');
  let html = '';
  let listType = null;

  const closeList = () => {
    if (listType) {
      html += `</${listType}>`;
      listType = null;
    }
  };

  lines.forEach((line) => {
    const unordered = line.match(/^\s*-\s+(.*)$/);
    const ordered = line.match(/^\s*\d+\.\s+(.*)$/);

    if (unordered) {
      if (listType !== 'ul') {
        closeList();
        html += '<ul class="comment-list">';
        listType = 'ul';
      }
      html += `<li>${unordered[1]}</li>`;
      return;
    }

    if (ordered) {
      if (listType !== 'ol') {
        closeList();
        html += '<ol class="comment-list ordered">';
        listType = 'ol';
      }
      html += `<li>${ordered[1]}</li>`;
      return;
    }

    closeList();
    html += line ? `<p>${line}</p>` : '<br />';
  });

  closeList();
  return `<div class="comment-rendered">${sanitizeRichText(html)}</div>`;
};

const editingCommentId = ref(null);
const editingContent = ref('');
const emojiSearch = ref('');
const allEmojis = ["😀","😃","😄","😁","😆","😅","😂","🤣","😊","😇","🙂","🙃","😉","😌","😍","🥰","😘","😗","😙","😚","😋","😛","😝","😜","🤪","🤨","🧐","🤓","😎","🤩","🥳","😏","😒","😞","😔","😟","😕","🙁","☹️","😣","😖","😫","😩","🥺","😢","😭","😤","😠","😡","🤬","🤯","😳","🥵","🥶","😱","😨","😰","😥","😓","🤗","🤔","🤭","🤫","🤥","😶","😐","😑","😬","🙄","😯","😦","😧","😮","😲","🥱","😴","🤤","😪","😵","🤐","🥴","🤢","🤮","🤧","😷","🤒","🤕","🤑","🤠","😈","👿","👹","👺","🤡","💩","👻","💀","☠️","👽","👾","🤖","🎃","😺","😸","😹","😻","😼","😽","🙀","😿","😾","👍","👎","👌","✌️","🤞","🤟","🤘","🤙","👈","👉","👆","👇","☝️","✋","🤚","🖐","🖖","👋","👏","🙌","👐","🤲","🤝","🙏","✍️","💅","🤳","💪","🦾","🦵","🦿","🦶","👣","👂","🦻","👃","🧠","🦷","🦴","👀","👁","👅","👄","💋","🩸"];

const filteredEmojis = computed(() => {
    if (!emojiSearch.value) return allEmojis;
    return allEmojis;
});

const startEditingComment = (c) => {
    editingCommentId.value = c.id;
    editingContent.value = c.content;
};
const cancelEditingComment = () => {
    editingCommentId.value = null;
    editingContent.value = '';
};
const saveEditedComment = async (cId, cRef) => {
    try {
        const sanitizedContent = sanitizeRichText(editingContent.value);
        await axiosClient.put(`/projects/${props.projectId}/WorkTasks/${props.selectedTask.id}/comments/${cId}`, {
            content: sanitizedContent
        });
        cRef.content = sanitizedContent;
        cRef.isEdited = true;
        cancelEditingComment();
        fetchAuditTimeline();
        ElMessage.success("Đã cập nhật bình luận");
    } catch(e) { ElMessage.error("Lỗi khi sửa"); }
};
const deleteComment = async (cId) => {
    try {
        await axiosClient.delete(`/projects/${props.projectId}/WorkTasks/${props.selectedTask.id}/comments/${cId}`);
        comments.value = comments.value.filter(cm => cm.id !== cId);
        fetchAuditTimeline();
        ElMessage.success("Đã xoá bình luận");
    } catch(e) { ElMessage.error("Lỗi xóa bình luận"); }
};
const copyCommentLink = (cId) => {
    const url = `${window.location.origin}/projects/${props.projectId}/work-tasks/${props.selectedTask.id}?comment=${cId}`;
    navigator.clipboard.writeText(url);
    ElMessage.success("Đã copy link bình luận");
};
const copyTaskLink = async () => {
    const url = `${window.location.origin}/space/${props.projectId}?task=${props.selectedTask.id}`;
    await navigator.clipboard.writeText(url);
    ElMessage.success("Đã copy link công việc");
};

const toggleSubscription = async () => {
    if (!props.selectedTask?.id || props.selectedTask?.isNew) {
        ElMessage.warning('Cần tạo work item trước khi subscribe.');
        return;
    }
    try {
        const response = await axiosClient.post(`/projects/${props.projectId}/WorkTasks/${props.selectedTask.id}/subscription`);
        const subscribed = toBooleanFlag(response.data?.data?.isSubscribed);
        isSubscribed.value = subscribed;
        props.selectedTask.isSubscribed = subscribed;
        emit('refresh-tasks');
        ElMessage.success(subscribed ? 'Đã theo dõi công việc' : 'Đã hủy theo dõi công việc');
    } catch (error) {
        ElMessage.error(error.response?.data?.message || 'Không thể cập nhật trạng thái theo dõi');
    }
};

const handleTaskMenuCommand = (command) => {
    if (command === 'copy') {
      copyTaskLink();
      return;
    }
    if (command === 'duplicate') duplicateTask();
    if (command === 'archive') {
      ElMessage.info('Archive đang được chuẩn bị.');
    }
};

const toggleActivitySort = () => {
    activitySortNewestFirst.value = !activitySortNewestFirst.value;
    ElMessage.success(activitySortNewestFirst.value ? 'Activity mới nhất trước' : 'Activity cũ nhất trước');
};

const duplicateTask = async () => {
    try {
      await axiosClient.post(`/projects/${props.projectId}/WorkTasks`, {
        title: `${props.selectedTask.title || 'Work item'} copy`,
        description: props.selectedTask.description,
        statusName: props.selectedTask.statusName || 'TO DO',
        priority: props.selectedTask.priority ?? 0,
        totalEstimatedHours: getEstimatedHours(props.selectedTask),
        assignedUserId: getAssigneeIds()[0] || null,
        assigneeIds: getAssigneeIds(),
        plannedStartDate: props.selectedTask.plannedStartDate,
        dueDate: props.selectedTask.dueDate,
        sprintId: props.selectedTask.sprintId,
        moduleId: props.selectedTask.moduleId,
        parentTaskId: props.selectedTask.parentId || props.selectedTask.parentTaskId || null,
        labelIds: props.selectedTask.labelIds || []
      });
      emit('refresh-tasks');
      ElMessage.success('Đã duplicate công việc');
    } catch (error) {
      ElMessage.error(error.response?.data?.message || 'Không duplicate được công việc');
    }
};

const showActivityFilterInfo = () => {
    ElMessage.info('Activity đang hiển thị bình luận và cập nhật hiện có.');
};
const addReaction = async (c, emoji) => {
    if (!props.selectedTask?.id || !c?.id) return;

    const previousReactions = { ...(c.reactions || {}) };
    c.reactions = { ...previousReactions, [emoji]: (previousReactions[emoji] || 0) + 1 };

    try {
        const res = await axiosClient.post(`/projects/${props.projectId}/WorkTasks/${props.selectedTask.id}/comments/${c.id}/reactions`, { emoji });
        c.reactions = res.data?.data?.reactions || c.reactions;
    } catch (error) {
        c.reactions = previousReactions;
        ElMessage.error(error.response?.data?.message || 'Khong the them reaction');
    }
};

const fetchAdditionalProjectData = async () => {
    if (!props.projectId) return;
    try {
        const [cyclesRes, modulesRes, labelsRes, tasksRes, membersRes, statusesRes, executionRulesRes] = await Promise.all([
             axiosClient.get(`/projects/${props.projectId}/sprints`).catch(()=>({data:{data:[]}})),
             axiosClient.get(`/projects/${props.projectId}/modules`).catch(()=>({data:{data:[]}})),
             axiosClient.get(`/projects/${props.projectId}/labels`).catch(()=>({data:{data:[]}})),
             axiosClient.get(`/projects/${props.projectId}/WorkTasks`).catch(()=>({data:{data:[]}})),
             axiosClient.get(`/projects/${props.projectId}/members`).catch(()=>({data:{data:[]}})),
             axiosClient.get(`/projects/${props.projectId}/task-statuses`).catch(()=>({data:{data:[]}})),
             axiosClient.get(`/projects/${props.projectId}/execution-rules`).catch(()=>({data:{data:{}}}))
        ]);
        projectCycles.value = cyclesRes.data?.data || [];
        projectModules.value = modulesRes.data?.data || [];
        projectLabels.value = labelsRes.data?.data || [];
        cachedProjectTasks.value = (tasksRes.data?.data || []).map(item => normalizeTaskSnapshot({ ...item }));
        projectMemberOptions.value = (membersRes.data?.data || []).map(member => ({
            ...member,
            userId: member.userId || member.id,
            fullName: member.fullName || member.name || member.email,
            projectRole: member.projectRole || member.ProjectRole || member.myRole || member.MyRole || ''
        }));
        const fallbackStatuses = [
          { id: 'fallback-backlog', name: 'BACKLOG', displayName: 'Backlog' },
          { id: 'fallback-todo', name: 'TO DO', displayName: 'To Do' },
          { id: 'fallback-progress', name: 'IN PROGRESS', displayName: 'In Progress' },
          { id: 'fallback-review', name: 'IN REVIEW', displayName: 'In Review' },
          { id: 'fallback-done', name: 'DONE', displayName: 'Done' },
          { id: 'fallback-cancelled', name: 'CANCELLED', displayName: 'Cancelled' }
        ];
        const incomingStatuses = (statusesRes.data?.data || []).map((status) => ({
            ...status,
            name: status.name,
            displayName: status.displayName || status.name
        }));
        projectStatuses.value = incomingStatuses.length ? incomingStatuses : fallbackStatuses;
        projectExecutionRules.value = {
          enableRoleBasedTaskVisibility: Boolean(executionRulesRes.data?.data?.enableRoleBasedTaskVisibility),
          managerAlwaysSeeAllTasks: executionRulesRes.data?.data?.managerAlwaysSeeAllTasks !== false,
          defaultTaskVisibilityMode: executionRulesRes.data?.data?.defaultTaskVisibilityMode || 'project',
          defaultBaseHours: Number(executionRulesRes.data?.data?.defaultBaseHours ?? 4),
          hoursPerStoryPoint: Number(executionRulesRes.data?.data?.hoursPerStoryPoint ?? 2),
          estimateBaselineMode: executionRulesRes.data?.data?.estimateBaselineMode || 'role_then_project',
          roleHourMultipliers: executionRulesRes.data?.data?.roleHourMultipliers || {}
        };
        applyProjectDefaultsToTask(props.selectedTask, { forceEstimate: false });
    } catch(e) {}
};

const toggleLabelDetail = async (labelId) => {
    if (!props.selectedTask?.id || props.selectedTask.isNew) {
        toggleSelectedLabelDetail(labelId);
        return;
    }

    const labelIds = props.selectedTask.labelIds || [];
    const exists = labelIds.includes(labelId);
    try {
        if (exists) {
            await axiosClient.delete(`/projects/${props.projectId}/tasks/${props.selectedTask.id}/labels/${labelId}`);
            props.selectedTask.labelIds = labelIds.filter(id => id !== labelId);
        } else {
            await axiosClient.post(`/projects/${props.projectId}/tasks/${props.selectedTask.id}/labels`, { labelId });
            props.selectedTask.labelIds = [...labelIds, labelId];
        }
    } catch (error) {
        ElMessage.error(error.response?.data?.message || 'Khong the cap nhat label');
    }
};
// ======================================================================

let unsubscribeExecutionRulesRealtime = null;

watch(showTaskModal, (val) => {
  if (!val) emit('close');
});

watch(workSession, () => {
  persistCurrentWorkSession();
}, { deep: true });

onMounted(() => {
  startWorkSessionTicker();
  window.addEventListener('mousemove', touchWorkSessionActivity, { passive: true });
  window.addEventListener('keydown', touchWorkSessionActivity, { passive: true });
  window.addEventListener('scroll', touchWorkSessionActivity, { passive: true });
  window.addEventListener('click', touchWorkSessionActivity, { passive: true });
  document.addEventListener('visibilitychange', syncWorkSessionOnVisibility);
  unsubscribeExecutionRulesRealtime = subscribeAdminRealtime(async ({ type, payload }) => {
    if (!props.projectId) return;
    if (payload?.projectId && `${payload.projectId}` !== `${props.projectId}`) return;

    if (['project-settings-updated', 'project-administration-updated'].includes(type)) {
      await fetchAdditionalProjectData();
    }
  });
});

onUnmounted(() => {
  stopWorkSessionTicker();
  window.removeEventListener('mousemove', touchWorkSessionActivity);
  window.removeEventListener('keydown', touchWorkSessionActivity);
  window.removeEventListener('scroll', touchWorkSessionActivity);
  window.removeEventListener('click', touchWorkSessionActivity);
  document.removeEventListener('visibilitychange', syncWorkSessionOnVisibility);
  unsubscribeExecutionRulesRealtime?.();
});

const formatDate = (dateStr) => {
  if (!dateStr) return '';
  const d = parseDateValue(dateStr);
  if (!d) return '';
  return d.toLocaleDateString('vi-VN');
};

const fetchAuditTimeline = async () => {
    if (!props.selectedTask?.id) {
      auditEntries.value = [];
      return;
    }

    try {
      const res = await axiosClient.get('/auditlogs', {
        params: {
          taskId: props.selectedTask.id,
          limit: 50
        }
      });
      auditEntries.value = res.data?.data?.items || [];
    } catch (error) {
      auditEntries.value = [];
    }
};

const formatRelativeTime = (dateStr) => {
  if (!dateStr) return 'just now';
  const date = new Date(dateStr);
  if (Number.isNaN(date.getTime())) return 'just now';

  const diffMs = Date.now() - date.getTime();
  const diffMinutes = Math.max(0, Math.floor(diffMs / 60000));

  if (diffMinutes < 1) return 'just now';
  if (diffMinutes < 60) return `${diffMinutes} minute${diffMinutes === 1 ? '' : 's'} ago`;

  const diffHours = Math.floor(diffMinutes / 60);
  if (diffHours < 24) return `${diffHours} hour${diffHours === 1 ? '' : 's'} ago`;

  const diffDays = Math.floor(diffHours / 24);
  if (diffDays < 7) return `${diffDays} day${diffDays === 1 ? '' : 's'} ago`;

  const diffWeeks = Math.floor(diffDays / 7);
  if (diffWeeks < 5) return `${diffWeeks} week${diffWeeks === 1 ? '' : 's'} ago`;

  const diffMonths = Math.floor(diffDays / 30);
  if (diffMonths < 12) return `${diffMonths} month${diffMonths === 1 ? '' : 's'} ago`;

  const diffYears = Math.floor(diffDays / 365);
  return `${diffYears} year${diffYears === 1 ? '' : 's'} ago`;
};

const resolveFileUrl = (url) => {
  if (!url) return '';
  if (/^https?:\/\//i.test(url)) return url;
  return `${apiRoot}${url.startsWith('/') ? url : `/${url}`}`;
};

const isImageAttachment = (attachment) => {
  const type = attachment?.contentType || '';
  const name = attachment?.fileName || '';
  return /^image\//i.test(type) || /\.(png|jpe?g|webp|gif|svg)$/i.test(name);
};

const handleAttachmentOpen = (attachment, comment = null) => {
  const url = resolveFileUrl(attachment.fileUrl);
  if (isImageAttachment(attachment)) {
    previewZoom.value = 1;
    previewImage.value = { ...attachment, url, commentId: comment?.id || null };
    return;
  }

  const link = document.createElement('a');
  link.href = url;
  link.download = attachment.fileName || '';
  document.body.appendChild(link);
  link.click();
  link.remove();
};

const getCreatorName = (task) => {
  if (!task) return 'Creator';
  return task.reporterName || task.createdByName || task.creatorName || task.createdBy?.fullName || task.reporter?.fullName || 'Creator';
};

const removePreviewAttachment = async () => {
  if (!previewImage.value?.commentId || !previewImage.value?.id) {
    ElMessage.info('Delete attachment requires a dedicated backend endpoint.');
    return;
  }

  try {
    await axiosClient.delete(`/projects/${props.projectId}/WorkTasks/${props.selectedTask.id}/comments/${previewImage.value.commentId}/attachments/${previewImage.value.id}`);
    const targetComment = comments.value.find(comment => comment.id === previewImage.value.commentId);
    if (targetComment?.attachments) {
      targetComment.attachments = targetComment.attachments.filter(item => item.id !== previewImage.value.id);
    }
    previewImage.value = null;
    ElMessage.success('Attachment deleted');
  } catch (error) {
    ElMessage.error(error.response?.data?.message || 'Khong the xoa attachment');
  }
};

const lastEditedBy = computed(() => {
  if (!props.selectedTask) return 'Unknown';
  return props.selectedTask.updatedByName || props.selectedTask.lastEditedBy || getCreatorName(props.selectedTask);
});

const lastEditedRelative = computed(() => formatRelativeTime(props.selectedTask?.updatedAt || props.selectedTask?.createdAt));

const updateTaskField = (task, field, value) => {
  emit('updateTask', task, field, value);
};

const updateTaskFields = (task, payload) => {
  emit('updateTask', task, payload);
};

const persistTaskPatch = async (task, payload) => {
  if (!task?.id || task?.isNew) return null;
  const response = await axiosClient.patch(`/projects/${props.projectId}/WorkTasks/${task.id}`, payload);
  const normalized = normalizeTaskSnapshot({ ...(response.data?.data || {}) });
  Object.assign(task, normalized);
  mergeCachedTask(normalized);
  emit('refresh-tasks');
  return normalized;
};

const formatEstimateHours = (value) => {
  const normalized = Math.max(0, Number(value) || 0);
  return normalized % 1 === 0 ? normalized.toFixed(0) : normalized.toFixed(1);
};

const getCurrentUserId = () => {
  const propUserId = props.currentUser?.id || props.currentUser?.userId;
  if (propUserId) return propUserId;

  const storedUser = getStoredUser();
  return storedUser?.id || storedUser?.userId || null;
};

const getWorkSessionContext = (task = props.selectedTask) => ({
  userId: getCurrentUserId(),
  projectId: props.projectId,
  taskId: task?.id || null
});

const isAssignedToCurrentUser = computed(() => {
  const currentUserId = getCurrentUserId();
  if (!currentUserId || !props.selectedTask) return false;
  return getAssigneeIds(props.selectedTask).includes(currentUserId);
});

const isWorkSessionRunning = computed(() => workSession.value?.status === 'running');
const isWorkSessionPaused = computed(() => workSession.value?.status === 'paused');
const trackedSessionHours = computed(() => calculateWorkSessionHours(workSession.value, workSessionNow.value));
const workSessionStatusLabel = computed(() => {
  if (isWorkSessionRunning.value) {
    return `Tracking ${formatEstimateHours(trackedSessionHours.value)}h`;
  }

  if (isWorkSessionPaused.value) {
    return workSession.value?.idlePausedAt
      ? `Idle paused at ${formatEstimateHours(trackedSessionHours.value)}h`
      : `Paused at ${formatEstimateHours(trackedSessionHours.value)}h`;
  }

  return 'No active session';
});

const persistCurrentWorkSession = () => {
  const context = getWorkSessionContext();
  if (!context.userId || !context.taskId) return;

  if (!workSession.value) {
    clearWorkSession(context);
    return;
  }

  saveWorkSession(context, workSession.value);
};

const stopWorkSessionTicker = () => {
  if (workSessionTick.value) {
    window.clearInterval(workSessionTick.value);
    workSessionTick.value = null;
  }
};

const startWorkSessionTicker = () => {
  stopWorkSessionTicker();
  workSessionTick.value = window.setInterval(() => {
    workSessionNow.value = Date.now();
    maybeAutoPauseWorkSession();
  }, 1000);
};

const touchWorkSessionActivity = () => {
  if (!isWorkSessionRunning.value || !workSession.value) return;
  workSession.value = {
    ...workSession.value,
    lastActivityAt: Date.now(),
    idlePausedAt: null
  };
  persistCurrentWorkSession();
};

const maybeAutoPauseWorkSession = () => {
  if (!isWorkSessionRunning.value || !workSession.value) return;
  const now = Date.now();
  const lastActivityAt = Number(workSession.value.lastActivityAt || workSession.value.startedAt || now);
  if (now - lastActivityAt < WORK_SESSION_IDLE_TIMEOUT_MS) {
    return;
  }

  const runningMs = Math.max(0, lastActivityAt - Number(workSession.value.startedAt || lastActivityAt));
  workSession.value = {
    ...workSession.value,
    status: 'paused',
    accumulatedMs: Math.max(0, Number(workSession.value.accumulatedMs) || 0) + runningMs,
    startedAt: null,
    pausedAt: now,
    idlePausedAt: lastActivityAt
  };
  persistCurrentWorkSession();

  if (idleNotificationShownForTaskId.value !== props.selectedTask?.id) {
    idleNotificationShownForTaskId.value = props.selectedTask?.id || null;
    ElNotification({
      title: 'Work session paused',
      message: 'No activity was detected for 5 minutes, so tracking was paused automatically.',
      type: 'warning',
      duration: 3500
    });
  }
};

const loadCurrentWorkSession = (task = props.selectedTask) => {
  const context = getWorkSessionContext(task);
  const savedSession = loadWorkSession(context);
  workSession.value = savedSession ? {
    ...savedSession,
    taskTitle: task?.title || savedSession.taskTitle || 'Work item'
  } : null;
  workSessionNow.value = Date.now();
  idleNotificationShownForTaskId.value = null;
};

const startWorkSession = (task = props.selectedTask) => {
  if (!task?.id || task?.isNew) return;
  if (!isAssignedToCurrentUser.value) {
    ElMessage.warning('Only assigned members can start time tracking on this work item.');
    return;
  }
  const now = Date.now();
  workSession.value = buildFreshWorkSession({
    ...getWorkSessionContext(task),
    taskTitle: task.title,
    startedAt: now
  });
  workSessionNow.value = now;
  persistCurrentWorkSession();
  ElMessage.success('Work session started.');
};

const pauseWorkSession = ({ notify = true } = {}) => {
  if (!isWorkSessionRunning.value || !workSession.value) return;
  const now = Date.now();
  const runningMs = Math.max(0, now - Number(workSession.value.startedAt || now));
  workSession.value = {
    ...workSession.value,
    status: 'paused',
    accumulatedMs: Math.max(0, Number(workSession.value.accumulatedMs) || 0) + runningMs,
    startedAt: null,
    pausedAt: now,
    idlePausedAt: null,
    lastActivityAt: now
  };
  workSessionNow.value = now;
  persistCurrentWorkSession();
  if (notify) {
    ElMessage.info('Work session paused.');
  }
};

const resumeWorkSession = () => {
  if (!workSession.value || !isWorkSessionPaused.value) return;
  if (!isAssignedToCurrentUser.value) {
    ElMessage.warning('Only assigned members can resume time tracking on this work item.');
    return;
  }
  const now = Date.now();
  workSession.value = {
    ...workSession.value,
    status: 'running',
    startedAt: now,
    lastActivityAt: now,
    pausedAt: null,
    idlePausedAt: null
  };
  workSessionNow.value = now;
  persistCurrentWorkSession();
  ElMessage.success('Work session resumed.');
};

const isEstimateDerivedFromSubtasks = computed(() => (subtasksList.value || []).length > 0);
const timeLogHours = ref('');
const timeLogNote = ref('');
const taskViewStartedAt = ref(Date.now());
const isLoggingTime = ref(false);

const elapsedTimeLogHours = computed(() => {
  const elapsedMs = Math.max(0, Date.now() - taskViewStartedAt.value);
  return Math.max(0.1, Math.round((elapsedMs / 3600000) * 10) / 10);
});

const elapsedTimeLogLabel = computed(() => `Auto ${formatEstimateHours(elapsedTimeLogHours.value)}h`);

const getActualHours = (task = props.selectedTask) => {
  const value = Number(task?.totalActualHours ?? 0);
  return Number.isFinite(value) ? value : 0;
};

const getAssigneeActualHours = (assignee) => {
  const value = Number(assignee?.totalActualHours ?? 0);
  return Number.isFinite(value) ? value : 0;
};

const getAssigneeSharePercent = (assignee, task = props.selectedTask) => {
  const totalEstimate = Math.max(0, Number(task?.totalEstimatedHours) || 0);
  const assigneeEstimate = Math.max(0, Number(assignee?.estimatedHours) || 0);
  if (totalEstimate > 0 && assigneeEstimate > 0) {
    return Math.max(0, Math.min(100, Math.round((assigneeEstimate / totalEstimate) * 100)));
  }

  const assignees = normalizeAssigneeEstimateState(task);
  const totalWeight = assignees.reduce((sum, item) => sum + Math.max(Number(item?.contributionWeight) || 0, 0.1), 0);
  const weight = Math.max(Number(assignee?.contributionWeight) || 0, 0.1);
  if (totalWeight > 0) {
    return Math.max(0, Math.min(100, Math.round((weight / totalWeight) * 100)));
  }

  return assignees.length ? Math.round(100 / assignees.length) : 100;
};

const normalizeAssigneeEstimateState = (task = props.selectedTask) => {
  if (!task) return [];
  if (!Array.isArray(task.assignees)) {
    task.assignees = [];
  }

  return task.assignees.map(assignee => ({
    ...assignee,
    userId: assignee.userId || assignee.id,
    progressPercent: Math.min(100, Math.max(0, Number(assignee.progressPercent) || 0)),
    contributionWeight: Math.max(0.1, Number(assignee.contributionWeight) || 1),
    estimatedHours: Math.max(0, Number(assignee.estimatedHours) || 0),
    totalActualHours: Math.max(0, Number(assignee.totalActualHours) || 0)
  }));
};

const distributeEstimateAcrossAssignees = (task = props.selectedTask, { persist = true } = {}) => {
  if (!task) return;
  const assignees = normalizeAssigneeEstimateState(task);
  if (!assignees.length) {
    task.assignees = assignees;
    return;
  }

  const totalEstimate = Math.max(0, Number(task.totalEstimatedHours) || 0);
  const totalWeight = assignees.reduce((sum, assignee) => sum + Math.max(assignee.contributionWeight, 0.1), 0);
  let assignedTotal = 0;

  task.assignees = assignees.map((assignee, index) => {
    const normalizedWeight = Math.max(assignee.contributionWeight, 0.1);
    const isLast = index === assignees.length - 1;
    const estimateHours = isLast
      ? Math.max(0, Math.round((totalEstimate - assignedTotal) * 10) / 10)
      : Math.max(0, Math.round((totalEstimate * normalizedWeight / totalWeight) * 10) / 10);
    assignedTotal += estimateHours;

    return {
      ...assignee,
      contributionWeight: normalizedWeight,
      estimatedHours: estimateHours
    };
  });

  if (!task.isNew && persist) {
    updateTaskFields(task, {
      assigneeProgress: task.assignees.map(assignee => ({
        userId: assignee.userId,
        progressPercent: assignee.progressPercent || 0,
        contributionWeight: assignee.contributionWeight || 1,
        estimatedHours: assignee.estimatedHours || 0
      }))
    });
  }
};

const syncTaskAssignees = (task, assigneeIds) => {
  if (!task) return;
  const existingAssignees = Array.isArray(task.assignees) ? task.assignees : [];
  const normalizedIds = Array.from(new Set(assigneeIds.filter(Boolean)));
  task.assigneeIds = normalizedIds;
  task.assignedUserId = normalizedIds[0] || null;
  task.assigneeId = normalizedIds[0] || null;
  task.assignees = projectMemberOptions.value
    .filter(member => normalizedIds.includes(member.userId))
    .map(member => {
      const existing = existingAssignees.find(item => (item.userId || item.id) === member.userId) || {};
      return {
        userId: member.userId,
        fullName: member.fullName || member.name || member.email,
        email: member.email,
        progressPercent: existing.progressPercent ?? 0,
        contributionWeight: existing.contributionWeight ?? 1,
        estimatedHours: existing.estimatedHours ?? 0
      };
    });

  distributeEstimateAcrossAssignees(task, { persist: false });
};

const applySelectedAssignees = async (assigneeIds, task = props.selectedTask) => {
  if (!task) return;
  const normalizedIds = Array.from(new Set(assigneeIds.filter(Boolean)));
  syncTaskAssignees(task, normalizedIds);

  if (!task.isNew) {
    updateTaskFields(task, {
      assigneeIds: normalizedIds,
      assigneeProgress: (task.assignees || []).map(assignee => ({
        userId: assignee.userId || assignee.id,
        progressPercent: assignee.progressPercent || 0,
        contributionWeight: assignee.contributionWeight || 1,
        estimatedHours: assignee.estimatedHours || 0
      }))
    });
  }
};

const toggleAssignee = async (memberId, task = props.selectedTask) => {
  if (!canManageTaskAssignees.value) {
    ElMessage.warning('You do not have permission to manage assignees for this work item.');
    return;
  }

  const currentIds = getAssigneeIds(task);
  const nextIds = currentIds.includes(memberId)
    ? currentIds.filter(id => id !== memberId)
    : Array.from(new Set([...currentIds, memberId]));
  await applySelectedAssignees(nextIds, task);
};

const toggleInlineTaskAssignee = async (task, memberId) => {
  await toggleAssignee(memberId, task);
};

const updateAssigneeProgress = (memberId, rawValue, task = props.selectedTask) => {
  if (!task) return;
  const progressPercent = Math.min(100, Math.max(0, Number(rawValue) || 0));
  task.assignees = (task.assignees || []).map(assignee =>
    (assignee.userId || assignee.id) === memberId ? { ...assignee, progressPercent } : assignee
  );

  if (!task.isNew) {
    updateTaskField(task, 'assigneeProgress', [{
      userId: memberId,
      progressPercent
    }]);
  }
};

const updateAssigneeContributionWeight = (memberId, rawValue, task = props.selectedTask) => {
  if (!task) return;
  task.assignees = normalizeAssigneeEstimateState(task).map(assignee =>
    assignee.userId === memberId
      ? { ...assignee, contributionWeight: Math.max(0.1, Number(rawValue) || 1) }
      : assignee
  );

  distributeEstimateAcrossAssignees(task);
};

const updateAssigneeEstimatedHours = (memberId, rawValue, task = props.selectedTask) => {
  if (!task) return;
  const normalizedEstimate = Math.max(0, Number(rawValue) || 0);
  task.assignees = normalizeAssigneeEstimateState(task).map(assignee =>
    assignee.userId === memberId
      ? { ...assignee, estimatedHours: normalizedEstimate }
      : assignee
  );

  task.totalEstimatedHours = Math.round(task.assignees.reduce((sum, assignee) => sum + (Number(assignee.estimatedHours) || 0), 0) * 10) / 10;

  if (!task.isNew) {
    updateTaskFields(task, {
      totalEstimatedHours: task.totalEstimatedHours,
      assigneeProgress: task.assignees.map(assignee => ({
        userId: assignee.userId,
        progressPercent: assignee.progressPercent || 0,
        contributionWeight: assignee.contributionWeight || 1,
        estimatedHours: assignee.estimatedHours || 0
      }))
    });
  }
};

const updateTaskProgress = (task, rawValue) => {
  if (!task) return;
  const progressPercent = Math.min(100, Math.max(0, Number(rawValue) || 0));
  const assigneeIds = getAssigneeIds(task);
  if (!assigneeIds.length) {
    ElMessage.warning('Assign at least one member to track progress.');
    return;
  }

  syncTaskAssignees(task, assigneeIds);
  task.assignees = (task.assignees || []).map(assignee => ({
    ...assignee,
    progressPercent
  }));

  if (!task.isNew) {
    updateTaskField(task, 'assigneeProgress', assigneeIds.map(userId => ({
      userId,
      progressPercent
    })));
  }
};

const getEstimatedHours = (task = props.selectedTask) => {
  const value = Number(task?.totalEstimatedHours ?? 0);
  return Number.isFinite(value) ? value : 0;
};

const calculateSuggestedEstimate = (task = props.selectedTask) => {
  const priority = Number(task?.priority ?? 0);
  const storyPoints = Number(task?.storyPoints ?? 0);
  const title = String(task?.title || '').toLowerCase();

  let hours = 2;

  if (storyPoints > 0) {
    hours = Math.max(hours, storyPoints * 2);
  }

  if (priority === 1) hours += 4;
  if (priority === 2) hours += 2;

  if (/(api|integration|refactor|migration|security|payment|deploy)/.test(title)) hours += 3;
  if (/(bug|fix|hotfix|patch)/.test(title)) hours += 1.5;

  return Math.round(hours * 2) / 2;
};

const suggestedEstimateHours = computed(() => calculateSuggestedEstimate());

const subtaskEstimateTotal = computed(() => {
  return Math.round(
    (subtasksList.value || []).reduce((sum, subtask) => sum + (Number(subtask?.totalEstimatedHours) || 0), 0) * 10
  ) / 10;
});

const updateEstimatedHours = (rawValue, task = props.selectedTask) => {
  if (!task) return;
  if ((subtasksList.value || []).length > 0 && task?.id === props.selectedTask?.id) {
    ElMessage.warning('Parent estimate is derived from sub-work items.');
    return;
  }
  const parsedValue = Number(rawValue);
  const shouldApplyBaseline = !Number.isFinite(parsedValue) || parsedValue <= 0;
  const nextValue = shouldApplyBaseline ? calculateBaselineEstimate(task) : Math.max(0, parsedValue);
  task.totalEstimatedHours = nextValue;
  task.estimateSourceLabel = shouldApplyBaseline ? 'Suggested from project baseline' : '';
  distributeEstimateAcrossAssignees(task, { persist: false });
  if (!task.isNew) {
    updateTaskFields(task, {
      totalEstimatedHours: nextValue,
      assigneeProgress: (task.assignees || []).map(assignee => ({
        userId: assignee.userId || assignee.id,
        progressPercent: assignee.progressPercent || 0,
        contributionWeight: assignee.contributionWeight || 1,
        estimatedHours: assignee.estimatedHours || 0
      }))
    });
  }
};

const applySuggestedEstimate = (task = props.selectedTask) => {
  updateEstimatedHours(calculateSuggestedEstimate(task), task);
  ElMessage.success('Estimate updated from suggestion');
};

const rollupEstimateFromSubtasks = (task = props.selectedTask) => {
  updateEstimatedHours(subtaskEstimateTotal.value, task);
  ElMessage.success('Parent estimate rolled up from sub-work items');
};

const submitTimeLog = async (task = props.selectedTask, options = {}) => {
  if (!task?.id || task?.isNew) return;
  if (isLoggingTime.value) return;
  if (isEstimateDerivedFromSubtasks.value && task?.id === props.selectedTask?.id) {
    ElMessage.warning('Log time on sub-work items so parent actual hours can roll up.');
    return;
  }
  const manualHours = Number(timeLogHours.value);
  const overrideHours = Number(options.hours);
  const hours = Math.max(
    0,
    Number.isFinite(overrideHours) && overrideHours > 0
      ? overrideHours
      : Number.isFinite(manualHours) && manualHours > 0
        ? manualHours
        : elapsedTimeLogHours.value
  );
  if (hours <= 0) {
    ElMessage.warning('Hours must be greater than 0.');
    return;
  }

  isLoggingTime.value = true;
  try {
    ElMessage.info(`Logging ${formatEstimateHours(hours)}h...`);
    const note = options.note ?? timeLogNote.value ?? null;
    const response = await axiosClient.post(`/projects/${props.projectId}/WorkTasks/${task.id}/time-logs`, {
      hours,
      workType: 'GENERAL',
      note
    });

    const updatedTask = response.data?.data;
    if (updatedTask?.id === task.id) {
      const normalized = normalizeTaskSnapshot({ ...task, ...updatedTask });
      Object.assign(task, normalized);
      mergeCachedTask(normalized);
    }

    timeLogHours.value = '';
    timeLogNote.value = '';
    taskViewStartedAt.value = Date.now();
    emit('refresh-tasks');
    ElMessage.success('Time log created.');
  } catch (error) {
    ElMessage.error(error.response?.data?.message || 'Unable to log time.');
  } finally {
    isLoggingTime.value = false;
  }
};

const stopWorkSession = async () => {
  if (!workSession.value) return;
  if (isWorkSessionRunning.value) {
    pauseWorkSession({ notify: false });
  }

  const hours = trackedSessionHours.value;
  if (hours <= 0) {
    workSession.value = null;
    persistCurrentWorkSession();
    ElMessage.info('Tracked session was empty, so nothing was logged.');
    return;
  }

  const sessionNote = timeLogNote.value?.trim()
    ? `[Tracked session] ${timeLogNote.value.trim()}`
    : '[Tracked session] Auto-generated from Start/Pause/Stop tracking.';

  await submitTimeLog(props.selectedTask, {
    hours,
    note: sessionNote
  });

  workSession.value = null;
  persistCurrentWorkSession();
};

const syncWorkSessionOnVisibility = () => {
  if (document.hidden) {
    maybeAutoPauseWorkSession();
    return;
  }

  workSessionNow.value = Date.now();
};

const handleTaskDateChange = (field, rawValue, task = props.selectedTask) => {
  if (!task) return;

  const normalizedValue = rawValue ? formatDateOnly(rawValue) : null;

  if (field === 'dueDate') {
    const startDate = formatDateOnly(task.plannedStartDate);
    if (normalizedValue && startDate && normalizedValue < startDate) {
      ElMessage.warning('Due date cannot be before start date.');
      task.dueDate = startDate;
      if (!task.isNew) {
        updateTaskField(task, 'dueDate', startDate);
      }
      return;
    }
  }

  task[field] = normalizedValue;

  if (field === 'plannedStartDate') {
    const startDate = normalizedValue;
    const dueDate = formatDateOnly(task.dueDate);
    if (startDate && dueDate && dueDate < startDate) {
      task.dueDate = startDate;
      if (!task.isNew) {
        updateTaskField(task, 'dueDate', startDate);
      }
    }
  }

  if (!task.isNew) {
    updateTaskField(task, field, normalizedValue);
  }
};

const selectStatus = (status, task = props.selectedTask) => {
  if (!task) return;
  const nextStatus = typeof status === 'string' ? status : status.name;
  if (!task.isNew) {
    updateTaskField(task, 'statusName', nextStatus);
  } else {
    task.statusName = nextStatus;
  }
};

const selectPriority = (priority, task = props.selectedTask) => {
  if (!task) return;
  task.priority = priority;
  if (!task.isNew) {
    updateTaskField(task, 'priority', priority);
  }
};

const updateStoryPoints = (rawValue, task = props.selectedTask) => {
  if (!task) return;
  const nextValue = Math.min(21, Math.max(0, Number(rawValue) || 0));
  task.storyPoints = nextValue;
  if (!task.isNew) {
    updateTaskField(task, 'storyPoints', nextValue);
  }
};

const handleDescriptionBlur = () => {
  if (!props.selectedTask?.isNew) {
    props.selectedTask.description = descriptionEditor.value?.innerHTML || props.selectedTask.description || '';
    updateTaskField(props.selectedTask, 'description', props.selectedTask.description);
  }
};

const handleDescriptionInput = () => {
  syncEditorModel('description');
};

const handleCommentEditorInput = () => {
  syncEditorModel('comment');
};

const insertHtmlAtCursor = (html, editorName) => {
  focusEditor(editorName);
  document.execCommand('insertHTML', false, html);
  syncEditorModel(editorName);
};

const uploadAsset = async (file, folder = 'tasks') => {
  const formData = new FormData();
  formData.append('file', file);
  formData.append('folder', folder);
  const response = await axiosClient.post('/upload', formData, {
    headers: { 'Content-Type': 'multipart/form-data' }
  });
  return response.data?.data;
};

const handleDescriptionUpload = async (event, kind) => {
  const file = event.target.files?.[0];
  if (!file) return;
  try {
    const uploaded = await uploadAsset(file, 'tasks');
    if (!uploaded?.fileUrl) return;
    if (kind === 'image') {
      insertHtmlAtCursor(`<img src="${resolveFileUrl(uploaded.fileUrl)}" alt="${uploaded.fileName}" class="embedded-image" />`, 'description');
    } else {
      ElMessage.info('File se khong chen vao description. Hay dung khu vuc Attachments cua task.');
    }
    handleDescriptionBlur();
  } catch (error) {
    ElMessage.error(error.response?.data?.message || 'Khong the tai tep len mo ta');
  } finally {
    event.target.value = '';
  }
};

const uploadImageIntoDescription = async (file) => {
  const uploaded = await uploadAsset(file, 'tasks');
  if (!uploaded?.fileUrl) return;
  insertHtmlAtCursor(`<img src="${resolveFileUrl(uploaded.fileUrl)}" alt="${uploaded.fileName}" class="embedded-image" />`, 'description');
  handleDescriptionBlur();
};

const handleDescriptionPaste = async (event) => {
  const files = Array.from(event.clipboardData?.files || []);
  const image = files.find(file => /^image\//.test(file.type));
  if (!image) return;

  event.preventDefault();
  try {
    await uploadImageIntoDescription(image);
  } catch (error) {
    ElMessage.error(error.response?.data?.message || 'Khong the paste anh vao description');
  }
};

const normalizeTaskSnapshot = (task) => {
  if (!task) return task;

  const parentId = getParentId(task);
  task.parentId = parentId;
  task.parentTaskId = parentId;
  task.plannedStartDate = formatDateOnly(task.plannedStartDate);
  task.plannedEndDate = formatDateOnly(task.plannedEndDate);
  task.dueDate = formatDateOnly(task.dueDate);
  task.totalEstimatedHours = Number(task.totalEstimatedHours ?? 0);
  task.storyPoints = Number(task.storyPoints ?? 0);
  task.visibilityMode = task.visibilityMode || task.VisibilityMode || 'project';
  task.visibleToRoles = Array.isArray(task.visibleToRoles)
    ? task.visibleToRoles.map(role => normalizeProjectRole(role)).filter(Boolean)
    : Array.isArray(task.VisibleToRoles)
      ? task.VisibleToRoles.map(role => normalizeProjectRole(role)).filter(Boolean)
      : [];

  if (Array.isArray(task.assignees)) {
    task.assignees = task.assignees.map(item => ({
      ...item,
      userId: item.userId || item.UserId || item.id,
      fullName: item.fullName || item.FullName || item.name,
      email: item.email || item.Email,
      progressPercent: item.progressPercent ?? item.ProgressPercent ?? 0,
      contributionWeight: item.contributionWeight ?? item.ContributionWeight ?? 1,
      estimatedHours: item.estimatedHours ?? item.EstimatedHours ?? 0,
      totalActualHours: item.totalActualHours ?? item.TotalActualHours ?? 0
    }));
  }

  const assigneeIds = getAssigneeIds(task);
  task.assigneeIds = assigneeIds;
  task.assigneeId = task.assigneeId || task.assignedUserId || assigneeIds[0] || null;
  task.assignedUserId = task.assignedUserId || task.assigneeId || assigneeIds[0] || null;
  if (Array.isArray(task.assignees) && task.assignees.length && !(task.assignees || []).some(item => Number(item.estimatedHours) > 0)) {
    const totalWeight = task.assignees.reduce((sum, assignee) => sum + Math.max(Number(assignee.contributionWeight) || 1, 0.1), 0);
    let assignedTotal = 0;
    task.assignees = task.assignees.map((assignee, index) => {
      const weight = Math.max(Number(assignee.contributionWeight) || 1, 0.1);
      const isLast = index === task.assignees.length - 1;
      const estimatedHours = isLast
        ? Math.max(0, Math.round(((Number(task.totalEstimatedHours) || 0) - assignedTotal) * 10) / 10)
        : Math.max(0, Math.round(((Number(task.totalEstimatedHours) || 0) * weight / totalWeight) * 10) / 10);
      assignedTotal += estimatedHours;
      return {
        ...assignee,
        contributionWeight: weight,
        estimatedHours
      };
    });
  }
  return task;
};

const mergeCachedTask = (task) => {
  if (!task?.id) return;
  const index = cachedProjectTasks.value.findIndex(item => item.id === task.id);
  if (index >= 0) {
    cachedProjectTasks.value[index] = { ...cachedProjectTasks.value[index], ...task };
  } else {
    cachedProjectTasks.value = [task, ...cachedProjectTasks.value];
  }
};

const openTaskDetail = (task) => {
  const normalized = normalizeTaskSnapshot({ ...task });
  const cachedTask = cachedProjectTasks.value.find(item => item.id === normalized?.id);
  emit(
    'open-task',
    normalizeTaskSnapshot(cachedTask ? { ...cachedTask, ...normalized } : normalized),
    { fromTask: normalizeTaskSnapshot({ ...props.selectedTask }) }
  );
};
const createSubtask = (task) => emit('create-subtask', task);

const submitNewTask = async () => {
    if(!props.selectedTask?.title) {
        ElMessage.warning('Vui lòng nhập tiêu đề');
        return;
    }
    try {
        await axiosClient.post(`/projects/${props.projectId}/WorkTasks`, {
            title: props.selectedTask.title,
            description: props.selectedTask.description,
            statusName: props.selectedTask.statusName || 'Todo',
            priority: props.selectedTask.priority !== undefined ? props.selectedTask.priority : 0,
            totalEstimatedHours: getEstimatedHours(props.selectedTask),
            assignedUserId: getAssigneeIds()[0] || null,
            assigneeIds: getAssigneeIds(),
            plannedStartDate: props.selectedTask.plannedStartDate,
            dueDate: props.selectedTask.dueDate,
            sprintId: props.selectedTask.sprintId,
            moduleId: props.selectedTask.moduleId,
            parentTaskId: getParentId(props.selectedTask),
            labelIds: props.selectedTask.labelIds || [],
            visibilityMode: props.selectedTask.visibilityMode || 'project',
            visibleToRoles: props.selectedTask.visibleToRoles || []
        });
        ElMessage.success('Đã tạo thành công');
        emit('refresh-tasks');
        if (!createMore.value) {
            emit('close');
        } else {
            props.selectedTask.title = '';
            props.selectedTask.description = '';
            props.selectedTask.totalEstimatedHours = 0;
        }
    } catch(e) {
        ElMessage.error('Lỗi khi tạo công việc');
    }
};

// === Subtasks Logic ===
const subtasksList = ref([]);
const isCreatingSubtask = ref(false);
const newSubtaskTitle = ref('');
const subtaskInputRef = ref(null);

async function fetchSubtasks() {
    try {
        const res = await axiosClient.get(`/projects/${props.projectId}/WorkTasks/${props.selectedTask.id}/subtasks`);
        subtasksList.value = (res.data?.data || []).map(item => {
            const normalized = normalizeTaskSnapshot({ ...item });
            mergeCachedTask(normalized);
            return normalized;
        });
    } catch(e) {}
}

const startCreateSubtask = () => {
    isCreatingSubtask.value = true;
    newSubtaskTitle.value = '';
    nextTick(() => {
        if(subtaskInputRef.value) subtaskInputRef.value.focus();
    });
};

const submitSubtask = async () => {
    if(!newSubtaskTitle.value.trim()) {
        isCreatingSubtask.value = false;
        return;
    }
    try {
        const response = await axiosClient.post(`/projects/${props.projectId}/WorkTasks/${props.selectedTask.id}/subtasks`, {
            title: newSubtaskTitle.value.trim(),
            statusName: 'BACKLOG',
            taskTypeId: props.selectedTask.taskTypeId,
            priority: props.selectedTask.priority,
            totalEstimatedHours: 0,
            visibilityMode: props.selectedTask.visibilityMode || 'project',
            visibleToRoles: props.selectedTask.visibleToRoles || []
        });
        isCreatingSubtask.value = false;
        newSubtaskTitle.value = '';
        const createdSubtask = response.data?.data;
        if (createdSubtask) {
            const normalized = normalizeTaskSnapshot({ ...createdSubtask });
            mergeCachedTask(normalized);
            subtasksList.value = [normalized, ...subtasksList.value];
        } else {
            await fetchSubtasks();
        }
        emit('refresh-tasks');
    } catch(e) {
        ElMessage.error(e.response?.data?.message || 'Lỗi khi tạo subtask');
    }
};

// === AI Brain Logic ===
const aiPrompt = ref('');
const isBrainTyping = ref(false);
const isAiBreakingDown = ref(false);
const isCreatingPreviewSubtasks = ref(false);
const isAiSuggestingEstimate = ref(false);
const isAiSuggestingAssignees = ref(false);
const aiEstimateSuggestion = ref(null);
const aiSubtaskPreview = ref([]);
const aiAssigneeSuggestion = ref(null);

const askBrain = async () => {
    if (!aiPrompt.value.trim() || !props.selectedTask) return;
    isBrainTyping.value = true;
    try {
        const res = await axiosClient.post('/ai/generate-description', { prompt: aiPrompt.value });
        const newDesc = res.data.data;
        props.selectedTask.description = newDesc; // optimistic update
        updateTaskField(props.selectedTask, 'description', newDesc);
        ElMessage.success('Brain đã tạo mô tả thành công!');
        aiPrompt.value = '';
    } catch (e) {
        ElMessage.error('Có lỗi xảy ra khi tạo mô tả bằng AI.');
    } finally {
        isBrainTyping.value = false;
    }
};

const createSubtasksWithAI = async () => {
    if (!props.selectedTask?.id || !props.projectId || isAiBreakingDown.value) return;
    isAiBreakingDown.value = true;
    try {
        const res = await axiosClient.post('/ai/breakdown-task', {
            projectId: props.projectId,
            parentTaskId: props.selectedTask.id,
            title: props.selectedTask.title,
            description: props.selectedTask.description || '',
            createSubtasks: false
        });

        aiSubtaskPreview.value = res.data?.data || [];
        if (aiSubtaskPreview.value.length) {
            ElMessage.success(`AI da preview ${aiSubtaskPreview.value.length} sub-work items.`);
        } else {
            ElMessage.warning('AI khong de xuat duoc sub-work item nao.');
        }
    } catch (e) {
        const msg = e.response?.data?.message || ''
        if (msg.toLowerCase().includes('quota') || e.response?.status === 429) {
          ElMessage.error('Đã hết hạn mức sử dụng AI (Quota). Vui lòng thử lại sau.')
        } else {
          ElMessage.error('AI không thể tạo danh sách công việc con lúc này. Vui lòng kiểm tra lại API key hoặc kết nối mạng.')
        }
    } finally {
        isAiBreakingDown.value = false;
    }
};

const discardAiSubtaskPreview = () => {
    aiSubtaskPreview.value = [];
};

const confirmAiSubtaskPreview = async () => {
    if (!props.selectedTask?.id || !props.projectId || !aiSubtaskPreview.value.length || isCreatingPreviewSubtasks.value) return;
    isCreatingPreviewSubtasks.value = true;
    try {
        const res = await axiosClient.post('/ai/create-subtasks-from-preview', {
            projectId: props.projectId,
            parentTaskId: props.selectedTask.id,
            subtasks: aiSubtaskPreview.value
        });

        const created = res.data?.data || [];
        if (created.length) {
            const normalized = created.map(item => {
                const snapshot = normalizeTaskSnapshot({ ...item });
                mergeCachedTask(snapshot);
                return snapshot;
            });
            subtasksList.value = [...normalized, ...subtasksList.value];
        } else {
            await fetchSubtasks();
        }

        aiSubtaskPreview.value = [];
        emit('refresh-tasks');
        ElMessage.success(`AI da tao ${created.length || 'cac'} sub-work items tu preview.`);
    } catch (e) {
        ElMessage.error(e.response?.data?.message || 'Khong tao duoc sub-work items tu preview.');
    } finally {
        isCreatingPreviewSubtasks.value = false;
    }
};

const suggestEstimateWithAI = async () => {
    if (!props.selectedTask?.title || isAiSuggestingEstimate.value) return;
    isAiSuggestingEstimate.value = true;
    try {
        const res = await axiosClient.post('/ai/suggest-estimate', {
            projectId: props.projectId,
            workItemId: props.selectedTask.id || null,
            title: props.selectedTask.title,
            description: props.selectedTask.description || '',
            priority: Number(props.selectedTask.priority || 0),
            storyPoints: Number(props.selectedTask.storyPoints || 0),
            assigneeCount: (props.selectedTask.assignees || []).length,
            subtaskCount: (subtasksList.value || []).length
        });

        aiEstimateSuggestion.value = res.data?.data || null;
        if (aiEstimateSuggestion.value) {
            ElMessage.success('AI estimate suggestion ready.');
        }
    } catch (e) {
        ElMessage.error(e.response?.data?.message || 'AI could not suggest an estimate.');
    } finally {
        isAiSuggestingEstimate.value = false;
    }
};

const suggestAssigneesWithAI = async () => {
    if (!canUseAiAssigneeSuggestion.value) {
        ElMessage.warning('You do not have permission to use AI assignee suggestions.');
        return;
    }
    if (!props.selectedTask?.title || !props.projectId || isAiSuggestingAssignees.value) return;
    isAiSuggestingAssignees.value = true;
    try {
        const res = await axiosClient.post('/ai/suggest-assignees', {
            projectId: props.projectId,
            workItemId: props.selectedTask.id || null,
            title: props.selectedTask.title,
            description: props.selectedTask.description || '',
            priority: Number(props.selectedTask.priority || 0),
            storyPoints: Number(props.selectedTask.storyPoints || 0),
            estimatedHours: Number(props.selectedTask.totalEstimatedHours || 0),
            candidateCount: 3
        });

        aiAssigneeSuggestion.value = res.data?.data || null;
        if (aiAssigneeSuggestion.value?.suggestions?.length) {
            ElMessage.success('AI assignee suggestion is ready.');
        } else {
            ElMessage.warning('AI did not find a suitable assignee suggestion.');
        }
    } catch (e) {
        ElMessage.error(e.response?.data?.message || 'AI could not suggest assignees.');
    } finally {
        isAiSuggestingAssignees.value = false;
    }
};

const applyAiAssigneeSuggestion = async (mode = 'top') => {
    if (!canUseAiAssigneeSuggestion.value) {
        ElMessage.warning('Only PM, PO, SM, project admins, or system admins can apply AI assignee suggestions.');
        return;
    }

    if (!props.selectedTask || !aiAssigneeSuggestion.value?.suggestions?.length) return;

    const recommendedCount = Math.max(1, Number(aiAssigneeSuggestion.value.recommendedAssigneeCount) || 1);
    const candidates = mode === 'team'
        ? aiAssigneeSuggestion.value.suggestions.slice(0, recommendedCount)
        : aiAssigneeSuggestion.value.suggestions.slice(0, 1);

    const assigneeIds = candidates.map(item => item.userId).filter(Boolean);
    if (!assigneeIds.length) {
        ElMessage.warning('No suggested assignee could be applied.');
        return;
    }

    syncTaskAssignees(props.selectedTask, assigneeIds);

    props.selectedTask.assignees = (props.selectedTask.assignees || []).map(assignee => {
        const match = candidates.find(item => item.userId === assignee.userId);
        if (!match) return assignee;
        return {
            ...assignee,
            contributionWeight: Number(match.suggestedContributionWeight || assignee.contributionWeight || 1),
            estimatedHours: Number(match.suggestedEstimatedHours || assignee.estimatedHours || 0)
        };
    });

    const totalSuggestedEstimate = props.selectedTask.assignees.reduce((sum, assignee) => sum + (Number(assignee.estimatedHours) || 0), 0);
    if (totalSuggestedEstimate > 0 && !isEstimateDerivedFromSubtasks.value) {
        props.selectedTask.totalEstimatedHours = Math.round(totalSuggestedEstimate * 10) / 10;
    }

    if (!props.selectedTask.isNew) {
        await updateTaskFields(props.selectedTask, {
            assigneeIds,
            totalEstimatedHours: props.selectedTask.totalEstimatedHours,
            assigneeProgress: (props.selectedTask.assignees || []).map(assignee => ({
                userId: assignee.userId || assignee.id,
                progressPercent: assignee.progressPercent || 0,
                contributionWeight: assignee.contributionWeight || 1,
                estimatedHours: assignee.estimatedHours || 0
            }))
        });
    }

    ElMessage.success(mode === 'team' ? 'Applied AI suggested team.' : 'Applied top AI assignee.');
};

const applyAiEstimateSuggestion = async () => {
    if (!aiEstimateSuggestion.value || !props.selectedTask) return;

    const task = props.selectedTask;
    const suggestedHours = Math.max(0, Number(aiEstimateSuggestion.value.suggestedHours) || 0);
    const suggestedStoryPoints = Math.max(0, Number(aiEstimateSuggestion.value.suggestedStoryPoints) || 0);
    task.storyPoints = suggestedStoryPoints;

    const isParentDerived = isEstimateDerivedFromSubtasks.value;
    if (!isParentDerived) {
        task.totalEstimatedHours = suggestedHours;
        distributeEstimateAcrossAssignees(task, { persist: false });
    }

    try {
        const payload = {
            storyPoints: suggestedStoryPoints,
            assigneeProgress: (task.assignees || []).map(assignee => ({
                userId: assignee.userId || assignee.id,
                progressPercent: assignee.progressPercent || 0,
                contributionWeight: assignee.contributionWeight || 1,
                estimatedHours: assignee.estimatedHours || 0
            }))
        };

        if (!isParentDerived) {
            payload.totalEstimatedHours = suggestedHours;
        }

        await persistTaskPatch(task, payload);
        ElMessage.success(
            isParentDerived
                ? 'Applied AI story points. Parent estimate stays derived from sub-work items.'
                : 'Applied AI estimate suggestion.'
        );
    } catch (e) {
        ElMessage.error(e.response?.data?.message || 'Could not save AI estimate suggestion.');
    }
};

// Comments logic
const comments = ref([]);
const replyingToCommentId = ref(null);
const newComment = ref('');
const pendingAttachments = ref([]);
const stripHtml = (value) => (value || '').replace(/<[^>]+>/g, ' ');
const commentHasContent = computed(() => stripHtml(newComment.value).replace(/\s+/g, '').length > 0 || pendingAttachments.value.length > 0);

async function fetchComments() {
  if (!props.selectedTask || !props.selectedTask.id) return;
  try {
    const res = await axiosClient.get(`/projects/${props.projectId}/WorkTasks/${props.selectedTask.id}/comments`);
    comments.value = res.data?.data || [];
  } catch (err) { }
}


// === Labels Logic ===
const projectLabels = ref([]);
const assignedLabels = ref([]);

async function fetchLabels() {
    try {
        const res = await axiosClient.get(`/projects/${props.projectId}/labels`);
        projectLabels.value = res.data?.data || [];
    } catch {}
}

async function fetchAssignedLabels() {
    if(props.selectedTask?.issueLabels) {
        assignedLabels.value = props.selectedTask.issueLabels; 
    } else {
        assignedLabels.value = props.selectedTask?.labels || [];
    }
}

const assignLabel = async (labelId) => {
    try {
        await axiosClient.post(`/projects/${props.projectId}/WorkTasks/${props.selectedTask.id}/labels`, { labelId });
        const lbl = projectLabels.value.find(l => l.id === labelId);
        if (lbl && !assignedLabels.value.find(a => a.labelId === labelId)) {
            assignedLabels.value.push({ labelId: lbl.id, name: lbl.name, colorCode: lbl.colorCode });
        }
        ElMessage.success("Gắn nhãn thành công");
    } catch (e) {
        ElMessage.error(e.response?.data?.message || "Lỗi khi gắn nhãn");
    }
};

const removeLabel = async (labelId) => {
    try {
        await axiosClient.delete(`/projects/${props.projectId}/WorkTasks/${props.selectedTask.id}/labels/${labelId}`);
        assignedLabels.value = assignedLabels.value.filter(a => a.labelId !== labelId);
    } catch (e) {
        ElMessage.error(e.response?.data?.message || "Lỗi khi gỡ nhãn");
    }
};

// === Dependencies Logic ===
const taskDependencies = ref([]);
const cachedProjectTasks = ref([]);

async function fetchDependencies() {
    try {
        const res = await axiosClient.get(`/projects/${props.projectId}/WorkTasks/${props.selectedTask.id}/dependencies`);
        const items = res.data?.data || [];
        taskDependencies.value = items.map(d => {
            const isPredecessor = d.predecessorTaskId === props.selectedTask.id;
            let relType = "";
            if(d.dependencyType === 1) { relType = isPredecessor ? "blocks" : "blocked_by"; }
            else if(d.dependencyType === 2) { relType = "relates_to"; }
            else if(d.dependencyType === 3) { relType = "duplicate"; }
            
            return {
                targetId: isPredecessor ? d.successorTaskId : d.predecessorTaskId,
                targetSequenceId: isPredecessor ? d.successorSequenceId : d.predecessorSequenceId,
                targetTitle: isPredecessor ? d.successorTitle : d.predecessorTitle,
                relationType: relType
            };
        });
    } catch {}
}

const fetchProjectTasks = async () => {
    try {
        const res = await axiosClient.get(`/projects/${props.projectId}/WorkTasks`);
        cachedProjectTasks.value = res.data?.data || [];
    } catch {}
};

const depsDropdownTasks = computed(() => {
    return cachedProjectTasks.value.filter(t => t.id !== props.selectedTask.id);
});

const getRelationText = (type) => {
    if(type === 'blocks') return 'Blocks (chặn)';
    if(type === 'blocked_by') return 'Bị chặn bởi';
    if(type === 'relates_to') return 'Liên quan đến';
    if(type === 'duplicate') return 'Trùng lặp';
    return type;
};

const addDependency = async (command) => {
    try {
        await axiosClient.post(`/projects/${props.projectId}/WorkTasks/${props.selectedTask.id}/dependencies`, {
            relatedTaskId: command.relatedId,
            relationType: command.type
        });
        await fetchDependencies();
        ElMessage.success("Đã thêm quan hệ!");
    } catch (e) {
        ElMessage.error(e.response?.data?.message || "Lỗi khi thêm quan hệ");
    }
};

const removeDependency = async (targetId) => {
  try {
     await axiosClient.delete(`/projects/${props.projectId}/WorkTasks/${props.selectedTask.id}/dependencies/${targetId}`);
     await fetchDependencies();
  } catch (e) {
     ElMessage.error("Lỗi khi xóa quan hệ");
  }
};

const topLevelComments = computed(() => {
  if (!comments.value) return [];
  const map = {};
  comments.value.forEach(c => { c.childComments = []; map[c.id] = c; });
  const roots = [];
  comments.value.forEach(c => {
    if (c.parentCommentId && map[c.parentCommentId]) {
      map[c.parentCommentId].childComments.push(c);
    } else {
      roots.push(c);
    }
  });
  return roots;
});

const activityEntries = computed(() => {
  const commentEntries = topLevelComments.value.map(comment => ({
    id: `comment-${comment.id}`,
    type: 'comment',
    timestamp: comment.updatedAt || comment.createdAt,
    comment
  }));

  const auditTimelineEntries = (auditEntries.value || []).map(entry => ({
    ...entry,
    type: 'audit'
  }));

  const items = [...auditTimelineEntries, ...commentEntries];
  const sorted = items.sort((left, right) => new Date(right.timestamp) - new Date(left.timestamp));
  return activitySortNewestFirst.value ? sorted : [...sorted].reverse();
});

const handleCommentFileChange = (event, imagesOnly = false) => {
    const files = Array.from(event.target.files || []);
    if (!files.length) return;
    const acceptedFiles = imagesOnly
        ? files.filter(file => /^image\//.test(file.type) || /\.(png|jpe?g|webp|gif|svg)$/i.test(file.name))
        : files;
    pendingAttachments.value = [...pendingAttachments.value, ...acceptedFiles];
    event.target.value = '';
};

const triggerCommentImageUpload = () => { if (commentImageInput.value) commentImageInput.value.click(); };
const triggerCommentFileUpload = () => { if (commentFileInput.value) commentFileInput.value.click(); };
const triggerDescriptionImageUpload = () => { if (descriptionImageInput.value) descriptionImageInput.value.click(); };
const triggerDescriptionFileUpload = () => { if (descriptionFileInput.value) descriptionFileInput.value.click(); };
const startReply = (c) => { replyingToCommentId.value = c.id; newComment.value = ''; pendingAttachments.value = []; };
const cancelReply = () => { replyingToCommentId.value = null; newComment.value = ''; pendingAttachments.value = []; };

const submitComment = async () => {
    if (!commentHasContent.value) return;
    try {
        const sanitizedContent = sanitizeRichText(newComment.value || '');
        const formData = new FormData();
        formData.append('content', sanitizedContent);
        if (replyingToCommentId.value) {
            formData.append('parentCommentId', replyingToCommentId.value);
        }
        pendingAttachments.value.forEach(file => {
            formData.append('files', file);
        });

        await axiosClient.post(`/projects/${props.projectId}/WorkTasks/${props.selectedTask.id}/comments`, formData, {
            headers: { 'Content-Type': 'multipart/form-data' }
        });
        
        newComment.value = '';
        if (commentEditor.value) commentEditor.value.innerHTML = '';
        pendingAttachments.value = [];
        replyingToCommentId.value = null;
        if (commentFileInput.value) commentFileInput.value.value = '';
        if (commentImageInput.value) commentImageInput.value.value = '';
        fetchComments();
        fetchAuditTimeline();
    } catch(e) {
        ElMessage.error(e.response?.data?.message || "Lỗi khi gửi bình luận");
    }
};

watch(() => props.selectedTask, (newTask) => {
  if (newTask) {
    normalizeTaskSnapshot(newTask);
    taskViewStartedAt.value = Date.now();
    timeLogHours.value = '';
    timeLogNote.value = '';
    loadCurrentWorkSession(newTask);
    isSubscribed.value = toBooleanFlag(newTask.isSubscribed);
    // Only fetch data for EXISTING tasks (have an id)
    // New tasks (isNew: true) have no id, so API calls would crash
    fetchAdditionalProjectData();

    if (newTask.id && !newTask.isNew) {
      fetchComments();
      fetchAuditTimeline();
      fetchDependencies();
      fetchAssignedLabels();
      fetchSubtasks();
    } else {
      // Reset data for new tasks
      comments.value = [];
      auditEntries.value = [];
      taskDependencies.value = [];
      assignedLabels.value = [];
      subtasksList.value = [];
    }
    replyingToCommentId.value = null;
    newComment.value = '';
    pendingAttachments.value = [];
    aiSubtaskPreview.value = [];
    aiAssigneeSuggestion.value = null;
    showTaskModal.value = true;
    nextTick(() => {
      if (descriptionEditor.value) {
        descriptionEditor.value.innerHTML = newTask.description || '';
      }
      if (commentEditor.value) {
        commentEditor.value.innerHTML = '';
      }
    });
  }
}, { immediate: true });
</script>

<style scoped>
.task-modal-overlay {
  position: fixed;
  inset: 0;
  z-index: 2000;
  background: color-mix(in srgb, black 50%, transparent);
  display: flex;
  align-items: center;
  justify-content: center;
  backdrop-filter: blur(2px);
}

/* UTILITIES */
.flex-wrapper { display: flex; align-items: center; }
.flex-center { display: flex; align-items: center; }
.flex-between { display: flex; justify-content: space-between; align-items: center; }
.gap-2 { gap: 8px; } .gap-3 { gap: 12px; } .gap-4 { gap: 16px; } .gap-5 { gap: 20px; } .gap-8 { gap: 32px; }
.text-muted { color: #A1A1AA; }
.text-primary { color: #38BDF8; }
.bg-dark { background: var(--bg-tertiary); }
.bg-dark-2 { background: var(--bg-primary); }
.border-gray { border-color: var(--border-color); }
.icon-btn { cursor: pointer; transition: color 0.2s; } .icon-btn:hover { color: #E5E7EB; }
.nav-icon-btn {
  width: 28px;
  height: 28px;
  border: 1px solid var(--border-color);
  border-radius: 6px;
  background: transparent;
  color: #A1A1AA;
  cursor: pointer;
}
.nav-icon-btn:hover {
  color: #E5E7EB;
  background: var(--hover-bg);
}
.icon-hover { cursor: pointer; padding: 4px; border-radius: 4px; } .icon-hover:hover { background: var(--hover-bg); }
.icon-hover.is-active {
  background: #1D4ED8;
  color: #FFFFFF;
}

[data-theme='dark'] .task-modal-overlay {
  background: rgba(0, 0, 0, 0.6);
}

[data-theme='light'] .task-modal-overlay {
  background: rgba(0, 0, 0, 0.3);
}

/* CREATE MODAL */
.create-centered-modal {
  width: min(600px, 95vw);
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 2px; /* Sharp UI */
  padding: 24px;
  box-shadow: var(--shadow-xl);
}

.cm-title {
  font-size: 18px;
  font-weight: 700;
  margin-bottom: 20px;
  color: var(--color-text-primary);
}

.cm-form-group {
  display: flex;
  flex-direction: column;
  gap: 12px;
  margin-bottom: 24px;
}

.cm-inputbox, .cm-textareabox {
  width: 100%;
  background: var(--color-input-bg);
  border: 1px solid var(--color-input-border);
  border-radius: 2px;
  padding: 12px 14px;
  color: var(--color-text-primary);
  outline: none;
  font-size: 14px;
}

.cm-inputbox:focus, .cm-textareabox:focus {
  border-color: var(--color-accent);
}

.cm-toolbar-row {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  margin-bottom: 24px;
}

.t-btn {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 6px 12px;
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 2px;
  color: var(--color-text-secondary);
  font-size: 13px;
  cursor: pointer;
}

.t-btn:hover {
  background: var(--color-surface-hover);
  border-color: var(--color-border-hover);
}

.cm-footer-row {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 12px;
  padding-top: 20px;
  border-top: 1px solid var(--color-border);
}

.btn-save {
  background: var(--color-accent);
  color: #fff;
  border: none;
  padding: 8px 20px;
  border-radius: 2px;
  font-weight: 600;
  cursor: pointer;
}

.btn-discard {
  background: transparent;
  color: var(--color-text-secondary);
  border: 1px solid var(--color-border);
  padding: 8px 20px;
  border-radius: 2px;
  cursor: pointer;
}

/* SIDE PANEL */
.task-side-panel {
  position: absolute;
  top: 0;
  right: 0;
  bottom: 0;
  width: min(800px, 90vw);
  background: var(--color-bg);
  border-left: 1px solid var(--color-border);
  display: flex;
  flex-direction: column;
}

.sp-header {
  height: 54px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 20px;
  border-bottom: 1px solid var(--color-border);
}

.sph-right {
  display: flex;
  align-items: center;
  gap: 6px;
}

.sp-body {
  flex: 1;
  overflow-y: auto;
  padding: 24px 32px;
}

.sp-breadcrumb {
  font-size: 13px;
  font-weight: 700;
  color: var(--color-text-muted);
  margin-bottom: 12px;
}

.sp-title {
  font-size: 22px;
  font-weight: 800;
  margin-bottom: 20px;
  outline: none;
  color: var(--color-text-primary);
  line-height: 1.3;
}

.sp-toolbar {
  display: flex;
  align-items: center;
  gap: 6px;
  margin-top: 24px;
  margin-bottom: 24px;
}

/* ACTION BUTTONS */
.s-btn {
  height: 28px;
  padding: 0 10px;
  font-size: 12px;
  font-weight: 500;
  border-radius: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
  cursor: pointer;
  background: var(--color-bg-secondary);
  color: var(--color-text-primary);
  border: 1px solid var(--color-border);
  transition: all 0.2s ease;
}

.s-btn-primary {
  background: var(--color-accent);
  color: #ffffff;
  border: 1px solid var(--color-accent);
}

.s-btn-outline {
  background: transparent;
  border: 1px solid var(--color-border);
}

.s-btn-icon {
  padding: 0;
  width: 28px;
  min-width: 28px;
}

.s-btn i {
  font-size: 12px;
}

.s-btn:hover:not(:disabled) {
  background: var(--color-bg-secondary);
  border-color: var(--color-border);
  filter: brightness(1.1);
}

.s-btn-primary:hover:not(:disabled) {
  filter: brightness(1.1);
}

.s-btn-outline:hover:not(:disabled) {
  background: var(--color-bg-secondary);
}

.s-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.rich-editor {
  min-height: 60px;
  font-size: 14px;
  line-height: 1.6;
  color: var(--color-text-secondary);
  outline: none;
}

.rich-editor[data-placeholder]:empty:before {
  content: attr(data-placeholder);
  color: var(--color-text-secondary);
}

.props-grid {
  display: grid;
  gap: 16px;
  margin-top: 20px;
}

.p-row {
  display: grid;
  grid-template-columns: 140px 1fr;
  align-items: center;
}

.p-label {
  font-size: 13px;
  color: var(--color-text-secondary);
  display: flex;
  align-items: center;
  gap: 8px;
}

.property-trigger {
  display: inline-flex;
  align-items: center;
  gap: 10px;
  padding: 6px 12px;
  background: var(--color-bg-secondary);
  border: 1px solid transparent;
  border-radius: 2px;
  color: var(--color-text-primary);
  font-size: 13px;
  cursor: pointer;
  transition: all 0.2s;
}

.property-trigger:hover {
  background: var(--color-bg);
  border-color: var(--color-border);
}

.property-value {
  color: var(--color-text-primary);
  font-weight: 500;
}
.t-btn-number {
  gap: 6px;
}
.estimate-inline-input,
.estimate-hours-input {
  width: 72px;
  border: 1px solid var(--color-border);
  border-radius: 6px;
  background: var(--color-bg-secondary);
  color: var(--color-text-primary);
  padding: 4px 8px;
  font-size: 12px;
}
.estimate-property {
  display: grid;
  gap: 10px;
}
.estimate-editor {
  display: inline-flex;
  align-items: center;
  gap: 8px;
}
.estimate-inline-actions {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}
.secondary-mini-btn {
  border: 1px solid var(--border-color);
  background: var(--color-bg-secondary);
  color: var(--color-text-secondary);
  border-radius: 6px;
  padding: 5px 10px;
  font-size: 12px;
  cursor: pointer;
  transition: all 0.2s ease;
}
.secondary-mini-btn:hover:not(:disabled) {
  border-color: var(--color-accent);
  background: var(--color-bg);
}
.secondary-mini-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}
.session-status-copy {
  color: var(--color-text-secondary);
  font-size: 12px;
  min-width: 120px;
}
.estimate-inline-input.compact {
  width: 68px;
}
.estimate-inline-input.wide {
  width: 140px;
}
.estimate-hours-input:disabled,
.estimate-inline-input:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}
.estimate-unit,
.estimate-helper-text,
.t-btn-number small {
  color: var(--color-text-secondary);
  font-size: 12px;
}
.estimate-suggestion-btn {
  justify-content: space-between;
  width: fit-content;
}
.muted-val {
  color: var(--color-text-secondary);
}

.btn-add-label {
  background: var(--color-bg);
  border: none;
  border-radius: 4px;
  padding: 4px 10px;
  color: #A1A1AA;
  font-size: 12px;
  cursor: pointer;
}

.icon-filter-btn {
  background: var(--color-bg-secondary);
  border: none;
  color: #A1A1AA;
  padding: 6px 10px;
  border-radius: 4px;
  cursor: pointer;
}

.ai-preview-panel {
  margin-top: 14px;
  padding: 14px;
  border: 1px solid var(--color-border);
  border-radius: 10px;
  background: color-mix(in srgb, var(--color-bg-secondary) 88%, #0ea5e9 12%);
}

.ai-preview-head {
  display: flex;
  justify-content: space-between;
  gap: 12px;
  margin-bottom: 12px;
}

.ai-preview-head p {
  margin: 4px 0 0;
  color: var(--color-text-secondary);
  font-size: 12px;
}

.ai-preview-actions {
  display: flex;
  align-items: center;
  gap: 10px;
}

.ai-preview-list {
  display: grid;
  gap: 10px;
}

.ai-assignee-panel {
  margin-top: 10px;
}

.ai-assignee-list {
  display: grid;
  gap: 10px;
  margin-top: 10px;
}

.ai-assignee-item {
  border: 1px solid var(--color-border);
  border-radius: 8px;
  padding: 10px 12px;
  background: var(--color-bg-secondary);
}

.ai-assignee-top {
  display: flex;
  justify-content: space-between;
  gap: 10px;
  margin-bottom: 4px;
  color: var(--color-text-primary);
}

.ai-assignee-metrics {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  margin-bottom: 6px;
  color: var(--color-text-secondary);
  font-size: 12px;
}

.ai-preview-item {
  border: 1px solid var(--color-border);
  border-radius: 8px;
  padding: 12px;
  background: var(--color-bg-secondary);
}

.ai-preview-top {
  display: flex;
  justify-content: space-between;
  gap: 10px;
  margin-bottom: 6px;
  color: var(--color-text-primary);
}

.ai-preview-item p {
  margin: 0;
  color: var(--color-text-secondary);
  font-size: 13px;
  line-height: 1.5;
}

.activity-feed {
  display: flex;
  flex-direction: column;
  gap: 16px;
  margin-top: 16px;
}

.feed-item {
  display: flex;
  gap: 12px;
  font-size: 13px;
}

.feed-icon {
  width: 24px;
  height: 24px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--color-text-secondary);
}

.muted-val {
  color: var(--color-text-secondary);
}

.editor-wrap {
  border: 1px solid var(--color-border);
  border-radius: 6px;
  background: var(--color-bg-secondary);
  overflow: hidden;
}

.comment-box {
  margin-top: 32px;
  padding-top: 24px;
  border-top: 1px solid var(--color-border);
}

.comment-editor {
  min-height: 80px;
  padding: 12px 16px !important;
}

.c-toolbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 8px 12px;
  background: var(--color-bg-secondary);
  border-top: 1px solid var(--color-border);
}

.ct-left {
  display: flex;
  align-items: center;
  gap: 4px;
}

.toolbar-sep {
  width: 1px;
  height: 16px;
  background: var(--color-border);
  margin: 0 4px;
}

.icon-hover {
  width: 28px;
  height: 28px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  border-radius: 4px;
  transition: all 0.2s;
}

.icon-hover:hover {
  background: var(--color-bg);
}

.icon-hover.is-active {
  background: var(--color-accent);
  color: #ffffff;
}

.c-submit {
  height: 28px;
  padding: 0 12px;
  background: var(--color-accent);
  color: #ffffff;
  border: none;
  border-radius: 4px;
  font-size: 12px;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.2s ease;
}

.c-submit:hover:not(:disabled) {
  filter: brightness(1.1);
}

.c-submit:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.theme-dropdown {
  background: var(--color-surface) !important;
  border: 1px solid var(--color-border) !important;
}

.theme-dropdown :deep(.el-dropdown-menu__item) {
  color: var(--color-text-primary) !important;
}

.theme-dropdown :deep(.el-dropdown-menu__item:hover) {
  background: var(--color-surface-hover) !important;
}

:global(.plane-popover) {
}

.t-btn-date:deep(.el-input__wrapper) {
  background-color: transparent !important;
  box-shadow: none !important;
  padding: 0 !important;
}

.t-btn-date:deep(.el-input__inner) {
  color: var(--color-text-primary) !important;
  font-size: 12px;
  font-weight: 500;
  cursor: pointer;
}

.property-date-picker {
  width: 190px;
}

.property-date-picker:deep(.el-input__wrapper) {
  border: 1px solid transparent;
  border-radius: 6px;
  padding: 6px 10px !important;
}

.property-date-picker:deep(.el-input__wrapper:hover) {
  border-color: var(--color-border-hover);
  background: var(--bg-tertiary) !important;
}
</style>
