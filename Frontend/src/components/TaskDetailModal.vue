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
           <el-dropdown trigger="click" @command="(cmd) => selectedTask.statusName = cmd">
             <div class="t-btn"><i :class="getStatusIcon(selectedTask?.statusName)"></i> <span>State</span> {{ selectedTask?.statusName || 'Todo' }}</div>
             <template #dropdown>
               <el-dropdown-menu class="dark-dropdown">
                 <el-dropdown-item v-for="status in projectStatuses" :key="status.id" :command="status.name"><i :class="getStatusIcon(status.name)" class="mr-2"></i> {{ status.displayName || status.name }}</el-dropdown-item>
               </el-dropdown-menu>
             </template>
           </el-dropdown>

           <!-- PRIORITY -->
           <el-dropdown  trigger="click" @command="(cmd) => selectedTask.priority = cmd">
             <div class="t-btn"><i :class="getPrioIcon(selectedTask?.priority)"></i> <span>Priority</span> {{ getPrioLabel(selectedTask?.priority) }}</div>
             <template #dropdown>
               <el-dropdown-menu class="dark-dropdown">
                 <el-dropdown-item :command="1"><i class="fa-solid fa-angles-up mr-2" style="color: #ef4444"></i> Urgent</el-dropdown-item>
                 <el-dropdown-item :command="2"><i class="fa-solid fa-chevron-up mr-2" style="color: #f59e0b"></i> High</el-dropdown-item>
                 <el-dropdown-item :command="3"><i class="fa-solid fa-minus mr-2" style="color: #3b82f6"></i> Medium</el-dropdown-item>
                 <el-dropdown-item :command="4"><i class="fa-solid fa-arrow-down mr-2" style="color: #9ca3af"></i> Low</el-dropdown-item>
                 <el-dropdown-item :command="0"><i class="fa-solid fa-ban mr-2 text-gray-500"></i> None</el-dropdown-item>
               </el-dropdown-menu>
             </template>
           </el-dropdown>

           <!-- ASSIGNEES -->
           <el-popover  placement="bottom-start" trigger="click" popper-class="plane-popover" :width="220" @show="assigneeSearch = ''">
             <template #reference>
               <div class="t-btn"><i class="fa-regular fa-user"></i> <span>Assignees</span> {{ getAssigneeSummary() }}</div>
             </template>
             <div class="popover-content">
               <input type="text" v-model="assigneeSearch" class="popover-search" placeholder="Type to search..." />
                <div class="popover-list">
                  <div class="popover-item" v-for="user in filteredMembers" :key="user.userId" @click="toggleAssignee(user.userId)">
                    <div class="avatar-xxs bg-gray-600 rounded-full w-5 h-5 flex-center text-white text-xs mr-2">{{ (user.fullName || user.email || 'U').charAt(0).toUpperCase() }}</div>
                    <span>{{ user.fullName || user.email }}</span>
                    <i v-if="getAssigneeIds().includes(user.userId)" class="fa-solid fa-check ms-auto"></i>
                  </div>
                  <div v-if="!filteredMembers.length" class="text-xs text-center text-gray-500 py-2">No assignees found.</div>
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
                      :value="assignee.progressPercent || 0"
                      @change="event => updateAssigneeProgress(assignee.userId, event.target.value)"
                    />
                    <span class="assignee-progress-suffix">%</span>
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
                   <div class="popover-c-circle mr-2 w-3 h-3 rounded-full" :style="{ backgroundColor: l.color || '#3b82f6' }"></div>
                   <span>{{ l.name }}</span>
                   <i v-if="selectedTask?.labelIds?.includes(l.id)" class="fa-solid fa-check ms-auto"></i>
                 </div>
                 <div class="popover-item pointer hover-bg-dark-1" v-if="filteredLabels.length === 0 && labelSearch" @click="createLabel(labelSearch)">
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
                 <div v-if="!filteredCycles.length" class="text-xs text-center text-gray-500 py-2">No cycles setup.</div>
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
           <el-popover  placement="bottom-start" trigger="click" popper-class="plane-popover dark" :width="350" @show="parentSearch = ''">
             <template #reference>
               <div class="t-btn"><i class="fa-solid fa-arrow-turn-up fa-rotate-90"></i> {{ getParentId(selectedTask) ? 'Parent selected' : 'Add parent' }}</div>
             </template>
             <div class="popover-content h-[250px] flex flex-col bg-[#1E2025]">
               <div class="p-2 border-b border-gray-700">
                 <div class="relative flex items-center">
                   <i class="fa-solid fa-magnifying-glass absolute left-2 text-gray-400"></i>
                   <input type="text" v-model="parentSearch" class="w-full bg-transparent border-none text-white pl-8 focus:outline-none" placeholder="Type to search..." />
                 </div>
               </div>
               <div class="flex-1 overflow-y-auto no-scrollbar p-2">
                 <div class="popover-item text-xs text-gray-400 hover:text-white cursor-pointer p-2 rounded hover:bg-gray-700 flex items-center" @click="setTaskParent(selectedTask, null)">
                   <i class="fa-solid fa-ban mr-2"></i> Remove parent
                 </div>
                 <div class="popover-item text-xs text-gray-300 hover:text-white cursor-pointer p-2 rounded hover:bg-gray-700 flex items-center" v-for="pt in filteredParents" :key="pt.id" @click="setTaskParent(selectedTask, pt.id)">
                   <span class="text-gray-500 mr-3 w-16 truncate font-mono">{{ pt.sequenceId || pt.id.substring(0,8) }}</span>
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
               <i class="fa-solid fa-arrow-right icon-btn" @click="showTaskModal = false"></i>
            </div>
            <div class="sph-right">
               <button class="unsub-btn" @click="toggleSubscription"><i class="fa-regular fa-bell-slash"></i> {{ isSubscribed ? 'Unsubscribe' : 'Subscribe' }}</button>
               <button class="icon-btn icon-action-btn" @click="copyTaskLink" title="Copy link"><i class="fa-solid fa-link"></i></button>
               <el-dropdown trigger="click" @command="handleTaskMenuCommand">
                 <button class="icon-btn icon-action-btn" title="More actions"><i class="fa-solid fa-ellipsis"></i></button>
                 <template #dropdown>
                   <el-dropdown-menu class="dark-dropdown">
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
                <div class="w-[1px] h-4 bg-gray-700 mx-1"></div>
                <i class="fa-solid fa-bold icon-hover" @mousedown.prevent="execEditorCommand('bold', null, 'description')"></i>
                <i class="fa-solid fa-italic icon-hover" @mousedown.prevent="execEditorCommand('italic', null, 'description')"></i>
                <i class="fa-solid fa-underline icon-hover" @mousedown.prevent="execEditorCommand('underline', null, 'description')"></i>
                <i class="fa-solid fa-strikethrough icon-hover" @mousedown.prevent="execEditorCommand('strikeThrough', null, 'description')"></i>
                <i class="fa-solid fa-list-ul icon-hover" @mousedown.prevent="execEditorCommand('insertUnorderedList', null, 'description')"></i>
                <i class="fa-solid fa-list-ol icon-hover" @mousedown.prevent="execEditorCommand('insertOrderedList', null, 'description')"></i>
                <i class="fa-solid fa-file-code icon-hover" :class="{ 'is-active': codeMode.description }" @mousedown.prevent="toggleCodeBlockMode('description')"></i>
                <div class="w-[1px] h-4 bg-gray-700 mx-1"></div>
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
               <button class="s-btn" :disabled="isAiBreakingDown" @click="createSubtasksWithAI">
                 <i class="fa-solid fa-wand-magic-sparkles"></i>
                 {{ isAiBreakingDown ? 'AI is creating...' : 'AI split into subtasks' }}
               </button>
               
               <el-popover placement="bottom-start" trigger="click" popper-class="plane-popover dark" :width="300">
                 <template #reference>
                   <button class="s-btn"><i class="fa-solid fa-code-fork"></i> Add relation</button>
                 </template>
                 <div class="popover-content h-[200px] flex flex-col bg-[#1E2025]">
                   <div class="p-2 border-b border-gray-700">
                     <div class="relative flex items-center">
                       <i class="fa-solid fa-magnifying-glass absolute left-2 text-gray-400"></i>
                       <input type="text" class="w-full bg-transparent border-none text-white pl-8 focus:outline-none" placeholder="Search to relate..." />
                     </div>
                   </div>
                   <div class="flex-1 flex-center justify-center py-4 text-gray-500 text-xs">
                     No related items
                   </div>
                 </div>
               </el-popover>
               
               <button class="s-btn" @click="triggerDescriptionFileUpload"><i class="fa-solid fa-paperclip"></i> Attach</button>
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
                      <el-dropdown-menu class="dark-dropdown">
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
                      <el-dropdown-menu class="dark-dropdown">
                        <el-dropdown-item :command="1"><i class="fa-solid fa-angles-up mr-2" style="color: #ef4444"></i> Urgent</el-dropdown-item>
                        <el-dropdown-item :command="2"><i class="fa-solid fa-chevron-up mr-2" style="color: #f59e0b"></i> High</el-dropdown-item>
                        <el-dropdown-item :command="3"><i class="fa-solid fa-minus mr-2" style="color: #3b82f6"></i> Medium</el-dropdown-item>
                        <el-dropdown-item :command="4"><i class="fa-solid fa-arrow-down mr-2" style="color: #9ca3af"></i> Low</el-dropdown-item>
                        <el-dropdown-item :command="0"><i class="fa-solid fa-ban mr-2 text-gray-500"></i> None</el-dropdown-item>
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
                        {{ canEditTaskProgress(selectedTask) ? 'Synced to assigned members' : 'Assign at least one member to track progress' }}
                      </span>
                    </div>
                  </div>
                </div>
                <div class="p-row">
                  <div class="p-label"><i class="fa-regular fa-user"></i> Assignees</div>
                  <div class="p-val">
                    <el-popover placement="bottom-start" trigger="click" popper-class="plane-popover" :width="260" @show="assigneeSearch = ''">
                      <template #reference>
                        <button class="property-trigger" :class="{ 'muted-val': !getAssigneeIds().length }">
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
                  </div>
                </div>
               <div class="p-row">
                 <div class="p-label"><i class="fa-solid fa-chart-simple"></i> Priority</div>
                 <div class="p-val">
                   <el-dropdown  trigger="click" @command="(cmd) => selectPriority(cmd)">
                     <div class="property-trigger" :class="{ 'muted-val': !selectedTask?.priority }"><i :class="getPrioIcon(selectedTask?.priority)"></i><span>Priority</span><span class="property-value">{{ getPrioLabel(selectedTask?.priority) }}</span></div>
                     <template #dropdown>
                       <el-dropdown-menu class="dark-dropdown">
                         <el-dropdown-item :command="1"><i class="fa-solid fa-angles-up mr-2" style="color: #ef4444"></i> Urgent</el-dropdown-item>
                         <el-dropdown-item :command="2"><i class="fa-solid fa-chevron-up mr-2" style="color: #f59e0b"></i> High</el-dropdown-item>
                         <el-dropdown-item :command="3"><i class="fa-solid fa-minus mr-2" style="color: #3b82f6"></i> Medium</el-dropdown-item>
                         <el-dropdown-item :command="4"><i class="fa-solid fa-arrow-down mr-2" style="color: #9ca3af"></i> Low</el-dropdown-item>
                         <el-dropdown-item :command="0"><i class="fa-solid fa-ban mr-2 text-gray-500"></i> None</el-dropdown-item>
                       </el-dropdown-menu>
                     </template>
                   </el-dropdown>
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
                       style="position:absolute; bottom:0; left:0; width:0; height:0; opacity:0; padding:0; border:0; visibility:hidden;"
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
                       style="position:absolute; bottom:0; left:0; width:0; height:0; opacity:0; padding:0; border:0; visibility:hidden;"
                       @change="val => handleTaskDateChange('dueDate', val)"
                     />
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
                    <el-popover placement="bottom-start" trigger="click" popper-class="plane-popover dark" :width="340" @show="parentSearch = ''">
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
                          <span class="font-bold text-white text-[13px]">{{ entry.comment.fullName || 'User' }}</span> 
                          <span class="text-gray-500 text-xs ml-2">commented {{ formatDate(entry.comment.createdAt) }} <span v-if="entry.comment.isEdited" class="italic">(edited)</span></span>
                       </div>
                       
                       <!-- Hover Actions -->
                       <div class="hidden group-hover:flex items-center gap-1 bg-[#16181D] border border-[#27272A] rounded p-0.5 shadow-lg absolute right-0 -top-2">
                          <el-popover  placement="bottom-end" trigger="click" popper-class="plane-popover dark !p-0" :width="320">
                             <template #reference>
                                <i class="fa-regular fa-face-smile text-gray-400 hover:text-white cursor-pointer px-1.5 py-1 rounded hover:bg-gray-700"></i>
                             </template>
                             <div class="popover-content bg-[#1E2025]">
                                <div class="p-2 border-b border-gray-700 relative">
                                  <i class="fa-solid fa-magnifying-glass absolute left-4 top-1/2 transform -translate-y-1/2 text-gray-400 text-xs"></i>
                                  <input type="text" v-model="emojiSearch" class="w-full bg-[#111] border border-gray-700 rounded text-white py-1.5 pl-8 pr-2 text-xs focus:outline-none focus:border-blue-500" placeholder="Search..." />
                                </div>
                                <div class="p-2 text-xs font-semibold text-gray-400">Smileys & emotion</div>
                                <div class="grid grid-cols-8 gap-1 p-2 max-h-[160px] overflow-y-auto no-scrollbar">
                                  <div v-for="emoji in filteredEmojis" :key="emoji" @click="addReaction(entry.comment, emoji)" class="cursor-pointer text-lg text-center hover:bg-gray-700 rounded p-1">{{ emoji }}</div>
                                </div>
                             </div>
                          </el-popover>
                          
                          <el-dropdown  trigger="click" placement="bottom-end">
                             <i class="fa-solid fa-ellipsis text-gray-400 hover:text-white cursor-pointer px-1.5 py-1 rounded hover:bg-gray-700"></i>
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
                       <div class="editor-wrap !bg-[#1E2025]">
                          <textarea class="c-input bg-transparent border-none !h-[60px]" v-model="editingContent" autofocus></textarea>
                          <div class="c-toolbar flex justify-end gap-2 p-2">
                             <button class="px-3 py-1.5 text-xs rounded border border-gray-600 text-gray-300 hover:bg-gray-700 transition" @click="cancelEditingComment">Cancel</button>
                             <button class="px-3 py-1.5 text-xs rounded bg-blue-600 text-white hover:bg-blue-700 transition" @click="saveEditedComment(entry.comment.id, entry.comment)">Update</button>
                          </div>
                       </div>
                    </div>
                    <div v-else>
                       <div class="mt-1 text-[14px] text-gray-300 format-comment-content" v-html="formatCommentDisplay(entry.comment.content)"></div>
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
                          <div v-for="(count, emoji) in entry.comment.reactions" :key="emoji" class="flex items-center gap-1.5 bg-[#1E2025] border border-[#27272A] rounded-full px-2.5 py-0.5 cursor-pointer hover:bg-gray-700 transition-colors" @click="addReaction(entry.comment, emoji)">
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
               <p class="text-[13px] font-semibold mb-2 text-gray-400">Add comment</p>
               <div class="editor-wrap !pt-2">
                  <div v-if="pendingAttachments.length > 0" class="px-3 pb-2 flex flex-wrap gap-2">
                     <div v-for="(file, idx) in pendingAttachments" :key="idx" class="flex items-center gap-1.5 bg-[#1E2025] border border-gray-700 rounded px-2 py-1 text-xs text-gray-300">
                        <i class="fa-regular fa-file-lines text-gray-400"></i>
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
                       <i class="fa-solid fa-align-left icon-hover" @mousedown.prevent="execEditorCommand('justifyLeft', null, 'comment')"></i>
                       <i class="fa-solid fa-align-center icon-hover" @mousedown.prevent="execEditorCommand('justifyCenter', null, 'comment')"></i>
                       <i class="fa-solid fa-align-right icon-hover" @mousedown.prevent="execEditorCommand('justifyRight', null, 'comment')"></i>
                       <div class="w-[1px] h-4 bg-gray-700 mx-1"></div>
                       <i class="fa-solid fa-bold icon-hover" @mousedown.prevent="execEditorCommand('bold', null, 'comment')"></i> 
                       <i class="fa-solid fa-italic icon-hover" @mousedown.prevent="execEditorCommand('italic', null, 'comment')"></i> 
                       <i class="fa-solid fa-underline icon-hover" @mousedown.prevent="execEditorCommand('underline', null, 'comment')"></i> 
                       <i class="fa-solid fa-strikethrough icon-hover" @mousedown.prevent="execEditorCommand('strikeThrough', null, 'comment')"></i>
                       <i class="fa-solid fa-code icon-hover ml-1" @mousedown.prevent="wrapSelectionWithInlineCode('comment')"></i>
                       <i class="fa-solid fa-file-code icon-hover" :class="{ 'is-active': codeMode.comment }" @mousedown.prevent="toggleCodeBlockMode('comment')"></i>
                       <div class="w-[1px] h-4 bg-gray-700 mx-1"></div>
                       <i class="fa-solid fa-list-ul icon-hover" @mousedown.prevent="execEditorCommand('insertUnorderedList', null, 'comment')"></i> 
                       <i class="fa-solid fa-list-ol icon-hover" @mousedown.prevent="execEditorCommand('insertOrderedList', null, 'comment')"></i> 
                       <div class="w-[1px] h-4 bg-gray-700 mx-1"></div>
                       <i class="fa-regular fa-image icon-hover" @mousedown.prevent="triggerCommentImageUpload"></i> 
                       <i class="fa-solid fa-paperclip icon-hover" @mousedown.prevent="triggerCommentFileUpload"></i>
                     </div>
                     <button class="c-submit" :style="commentHasContent ? { background: 'oklch(0.6311 0.126281 238.01)', color: '#fff', cursor: 'pointer' } : {}" :disabled="!commentHasContent" @click="submitComment">Comment</button>
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
import { ref, watch, computed, nextTick } from 'vue';
import { ElMessage, ElNotification } from 'element-plus';
import axiosClient from '@/api/axiosClient';
import DOMPurify from 'dompurify';

const props = defineProps({
  selectedTask: { type: Object, default: null },
  projectId: { type: [String, Number], required: true },
  projectMembers: { type: Array, default: () => [] },
  currentUser: { type: Object, default: () => ({}) }
});

const emit = defineEmits(['updateTask', 'close', 'open-task', 'create-subtask', 'refresh-tasks']);

const showTaskModal = ref(true);
const isSubscribed = ref(false);
const activitySortNewestFirst = ref(true);
const showSubtasks = ref(true);

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
  const candidate = formatDateOnly(date);
  const today = getTodayDateString();
  return Boolean(candidate && today && candidate < today);
};

const disableDueDates = (date) => {
  if (disablePastDates(date)) return true;
  const candidate = formatDateOnly(date);
  const start = formatDateOnly(props.selectedTask?.plannedStartDate);
  return Boolean(candidate && start && candidate < start);
};

const filteredMembers = computed(() => {
    const members = projectMemberOptions.value;
    if (!assigneeSearch.value) return members;
    return members.filter(m => (m.fullName || m.name || m.email || '').toLowerCase().includes(assigneeSearch.value.toLowerCase()));
});

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
    if (p===1) return 'fa-solid fa-angles-up text-red-500';
    if (p===2) return 'fa-solid fa-chevron-up text-yellow-500';
    if (p===3) return 'fa-solid fa-minus text-blue-500';
    if (p===4) return 'fa-solid fa-arrow-down text-gray-400';
    return 'fa-solid fa-ban text-gray-500';
};

const getStatusIcon = (s) => {
    const st = (s||'').toUpperCase();
    if(st.includes('CANCEL')) return 'fa-regular fa-circle-xmark text-red-500';
    if(st.includes('DONE')) return 'fa-regular fa-circle-check text-green-500';
    if(st.includes('PROGRESS')) return 'fa-solid fa-circle-half-stroke text-yellow-500';
    if(st.includes('REVIEW')) return 'fa-regular fa-circle-play text-blue-500';
    if(st.includes('TODO')) return 'fa-regular fa-circle text-gray-400';
    return 'fa-solid fa-circle-dashed text-gray-500';
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
   if (Array.isArray(task.assigneeIds) && task.assigneeIds.length) return task.assigneeIds;
   if (Array.isArray(task.assignees) && task.assignees.length) return task.assignees.map(item => item.userId || item.id).filter(Boolean);
   if (task.assignedUserId) return [task.assignedUserId];
   if (task.assigneeId) return [task.assigneeId];
   return [];
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
         contributionWeight: existing.contributionWeight ?? 1
      };
   });
};

const selectedAssigneeRows = computed(() => buildTaskAssigneeRows());

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

const canEditTaskProgress = (task = props.selectedTask) => getAssigneeIds(task).length > 0;

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
   return parent ? `${parent.sequenceId || parent.id?.substring(0, 8)} ${parent.title}` : 'Parent selected';
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
    if (props.selectedTask?.plannedStartDate && props.selectedTask.plannedStartDate < getTodayDateString()) {
        ElMessage.warning('Cannot select a past start date.');
        return;
    }
    if (props.selectedTask?.dueDate && props.selectedTask.dueDate < getTodayDateString()) {
        ElMessage.warning('Cannot select a past due date.');
        return;
    }
    if (props.selectedTask?.plannedStartDate && props.selectedTask?.dueDate && props.selectedTask.dueDate < props.selectedTask.plannedStartDate) {
        ElMessage.warning('Due date cannot be before start date.');
        return;
    }

    try {
        const response = await axiosClient.post(`/projects/${props.projectId}/WorkTasks/${props.selectedTask.id}/subscription`);
        const subscribed = Boolean(response.data?.data?.isSubscribed);
        isSubscribed.value = subscribed;
        props.selectedTask.isSubscribed = subscribed;
        emit('refresh-tasks');
        ElMessage.success(subscribed ? 'Đã theo dõi công việc' : 'Đã hủy theo dõi công việc');
    } catch (error) {
        ElMessage.error(error.response?.data?.message || 'Không thể cập nhật trạng thái theo dõi');
    }

    return;
    ElMessage.success(isSubscribed.value ? "Đã theo dõi công việc" : "Đã hủy theo dõi công việc");
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
        const [cyclesRes, modulesRes, labelsRes, tasksRes, membersRes, statusesRes] = await Promise.all([
             axiosClient.get(`/projects/${props.projectId}/sprints`).catch(()=>({data:{data:[]}})),
             axiosClient.get(`/projects/${props.projectId}/modules`).catch(()=>({data:{data:[]}})),
             axiosClient.get(`/projects/${props.projectId}/labels`).catch(()=>({data:{data:[]}})),
             axiosClient.get(`/projects/${props.projectId}/WorkTasks`).catch(()=>({data:{data:[]}})),
             axiosClient.get(`/projects/${props.projectId}/members`).catch(()=>({data:{data:[]}})),
             axiosClient.get(`/projects/${props.projectId}/task-statuses`).catch(()=>({data:{data:[]}}))
        ]);
        projectCycles.value = cyclesRes.data?.data || [];
        projectModules.value = modulesRes.data?.data || [];
        projectLabels.value = labelsRes.data?.data || [];
        cachedProjectTasks.value = (tasksRes.data?.data || []).map(item => normalizeTaskSnapshot({ ...item }));
        projectMemberOptions.value = (membersRes.data?.data || []).map(member => ({
            ...member,
            userId: member.userId || member.id,
            fullName: member.fullName || member.name || member.email
        }));
        const desiredOrder = ['BACKLOG', 'TO DO', 'IN PROGRESS', 'IN REVIEW', 'DONE', 'CANCELLED'];
        const statusMap = new Map();
        for (const status of (statusesRes.data?.data || [])) {
            const normalized = normalizeStatusName(status.name);
            if (!statusMap.has(normalized)) {
                statusMap.set(normalized, {
                    ...status,
                    name: normalized,
                    displayName: normalized
                });
            }
        }
        desiredOrder.forEach((name, index) => {
            if (!statusMap.has(name)) {
                statusMap.set(name, { id: `fallback-${index}`, name, displayName: name });
            }
        });
        projectStatuses.value = desiredOrder.map(name => statusMap.get(name)).filter(Boolean);
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

watch(showTaskModal, (val) => {
  if (!val) emit('close');
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

const syncTaskAssignees = (task, assigneeIds) => {
  if (!task) return;
  const existingAssignees = Array.isArray(task.assignees) ? task.assignees : [];
  task.assigneeIds = assigneeIds;
  task.assignedUserId = assigneeIds[0] || null;
  task.assigneeId = assigneeIds[0] || null;
  task.assignees = projectMemberOptions.value
    .filter(member => assigneeIds.includes(member.userId))
    .map(member => {
      const existing = existingAssignees.find(item => (item.userId || item.id) === member.userId) || {};
      return {
        userId: member.userId,
        fullName: member.fullName || member.name || member.email,
        email: member.email,
        progressPercent: existing.progressPercent ?? 0,
        contributionWeight: existing.contributionWeight ?? 1
      };
    });
};

const applySelectedAssignees = async (assigneeIds, task = props.selectedTask) => {
  if (!task) return;
  syncTaskAssignees(task, assigneeIds);

  if (!task.isNew) {
    updateTaskField(task, 'assigneeIds', assigneeIds);
  }
};

const toggleAssignee = async (memberId, task = props.selectedTask) => {
  const currentIds = getAssigneeIds(task);
  const nextIds = currentIds.includes(memberId)
    ? currentIds.filter(id => id !== memberId)
    : [...currentIds, memberId];
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

const handleTaskDateChange = (field, rawValue, task = props.selectedTask) => {
  if (!task) return;

  const normalizedValue = rawValue ? formatDateOnly(rawValue) : null;
  if (normalizedValue && normalizedValue < getTodayDateString()) {
    ElMessage.warning('Cannot select a past date.');
    return;
  }

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
  task.statusName = nextStatus;
  if (!task.isNew) {
    updateTaskField(task, 'statusName', nextStatus);
  }
};

const selectPriority = (priority, task = props.selectedTask) => {
  if (!task) return;
  task.priority = priority;
  if (!task.isNew) {
    updateTaskField(task, 'priority', priority);
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

  if (Array.isArray(task.assignees)) {
    task.assignees = task.assignees.map(item => ({
      ...item,
      userId: item.userId || item.id
    }));
  }

  const assigneeIds = getAssigneeIds(task);
  task.assigneeIds = assigneeIds;
  task.assigneeId = task.assigneeId || task.assignedUserId || assigneeIds[0] || null;
  task.assignedUserId = task.assignedUserId || task.assigneeId || assigneeIds[0] || null;
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
  emit('open-task', normalizeTaskSnapshot(cachedTask ? { ...cachedTask, ...normalized } : normalized));
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
            assignedUserId: getAssigneeIds()[0] || null,
            assigneeIds: getAssigneeIds(),
            plannedStartDate: props.selectedTask.plannedStartDate,
            dueDate: props.selectedTask.dueDate,
            sprintId: props.selectedTask.sprintId,
            moduleId: props.selectedTask.moduleId,
            parentTaskId: getParentId(props.selectedTask),
            labelIds: props.selectedTask.labelIds || []
        });
        ElMessage.success('Đã tạo thành công');
        emit('refresh-tasks');
        if (!createMore.value) {
            emit('close');
        } else {
            props.selectedTask.title = '';
            props.selectedTask.description = '';
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
            priority: props.selectedTask.priority
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
            createSubtasks: true
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

        emit('refresh-tasks');
        ElMessage.success(`AI da tao ${created.length || 'cac'} sub-work items.`);
    } catch (e) {
        ElMessage.error(e.response?.data?.message || 'AI khong tao duoc sub-work items. Kiem tra Gemini API key/quota.');
    } finally {
        isAiBreakingDown.value = false;
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
    isSubscribed.value = Boolean(newTask.isSubscribed);
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
/* GENERAL PANEL STYLING */
.task-modal-overlay {
  position: fixed;
  top: 0; left: 0; right: 0; bottom: 0;
  background: rgba(0, 0, 0, 0.6);
  z-index: 1500;
  display: flex;
  justify-content: center;
  align-items: center;
  font-family: 'Inter', sans-serif;
  color: #E5E7EB;
}

/* UTILITIES */
.flex-wrapper { display: flex; align-items: center; }
.flex-center { display: flex; align-items: center; }
.flex-between { display: flex; justify-content: space-between; align-items: center; }
.gap-2 { gap: 8px; } .gap-3 { gap: 12px; } .gap-4 { gap: 16px; } .gap-5 { gap: 20px; } .gap-8 { gap: 32px; }
.text-muted { color: #A1A1AA; }
.text-primary { color: #38BDF8; }
.bg-dark { background: #16181D; }
.bg-dark-2 { background: #111111; }
.border-gray { border-color: #27272A; }
.icon-btn { cursor: pointer; transition: color 0.2s; } .icon-btn:hover { color: #E5E7EB; }
.icon-hover { cursor: pointer; padding: 4px; border-radius: 4px; } .icon-hover:hover { background: #27272A; }
.icon-hover.is-active {
  background: #1D4ED8;
  color: #FFFFFF;
}

/* CENTRERED CREATION MODAL */
.create-centered-modal {
  width: 780px;
  background: #101010;
  border: 1px solid #27272A;
  border-radius: 8px;
  padding: 24px;
  display: flex;
  flex-direction: column;
  box-shadow: 0 24px 48px rgba(0,0,0,0.8);
  font-family: inherit;
}

.cm-title {
  font-size: 18px;
  font-weight: 600;
  color: #E4E4E7;
  margin: 0 0 16px 0;
}

.cm-badge-row {
  margin-bottom: 20px;
}

.cm-badge {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 4px 10px;
  border: 1px solid #27272A;
  border-radius: 4px;
  font-size: 13px;
  font-weight: 500;
  color: #E4E4E7;
  background-color: transparent;
}

.cm-form-group {
  display: flex;
  flex-direction: column;
  gap: 16px;
  margin-bottom: 20px;
}

.cm-inputbox {
  background: #18191B;
  border: 1px solid #27272A;
  border-radius: 6px;
  padding: 12px 16px;
  font-size: 15px;
  color: #E4E4E7;
  outline: none;
  transition: border-color 0.2s;
  width: 100%;
}
.cm-inputbox::placeholder { color: #52525B; font-weight: 500; }
.cm-inputbox:focus { border-color: #38BDF8; }

.cm-textareabox {
  background: #18191B;
  border: 1px solid #27272A;
  border-radius: 6px;
  padding: 16px;
  font-size: 15px;
  color: #E4E4E7;
  outline: none;
  resize: none;
  height: 180px;
  width: 100%;
  transition: border-color 0.2s;
}
.cm-textareabox::placeholder { color: #52525B; font-weight: 500; }
.cm-textareabox:focus { border-color: #38BDF8; }

.cm-toolbar-row {
  border-top: 1px solid #27272A;
  padding-top: 16px;
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
  margin-bottom: 24px;
}

.cm-toolbar-row .t-btn {
  display: flex;
  align-items: center;
  gap: 8px;
  background: transparent;
  border: 1px solid #27272A;
  color: #E4E4E7;
  border-radius: 4px;
  padding: 6px 12px;
  font-size: 12px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
}
.cm-toolbar-row .t-btn:hover { background: #27272A; border-color: #3F3F46; }

.cm-footer-row {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 16px;
}

.cm-t-more {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-right: 8px;
}
.cm-t-more span {
  font-size: 13px;
  color: #A1A1AA;
}

.btn-discard { 
  background: transparent; 
  border: 1px solid #27272A; 
  padding: 8px 16px; 
  border-radius: 6px; 
  font-size: 14px;
  font-weight: 500;
  color: #E4E4E7; 
  cursor: pointer; 
  transition: background 0.2s;
}
.btn-discard:hover { background: #1E1F21; }

.btn-save { 
  background: #0EA5E9; 
  border: none; 
  padding: 8px 24px; 
  border-radius: 6px; 
  color: white; 
  font-size: 14px;
  font-weight: 500; 
  cursor: pointer; 
  transition: background 0.2s; 
}
.btn-save:hover { background: #0284C7; }

/* SLIDE IN SIDE PANEL (TASK DETAIL) */
.task-side-panel {
  position: absolute;
  top: 0; right: 0; bottom: 0;
  width: 900px;
  max-width: 95vw;
  background: #151515;
  border-left: 1px solid #27272A;
  display: flex;
  flex-direction: column;
  box-shadow: -10px 0 30px rgba(0,0,0,0.5);
  animation: slideRight 0.3s ease-out;
  font-family: inherit;
}
@keyframes slideRight { from { transform: translateX(100%); } to { transform: translateX(0); } }

.sp-header {
  height: 56px;
  border-bottom: 1px solid #27272A;
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0 24px;
  flex-shrink: 0;
}
.sph-left, .sph-right {
  display: flex;
  align-items: center;
  gap: 16px;
  color: #A1A1AA;
}
.unsub-btn {
  background: transparent;
  border: 1px solid #3F3F46;
  border-radius: 4px;
  padding: 6px 12px;
  color: #E4E4E7;
  font-size: 13px;
  font-weight: 500;
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
}
.unsub-btn:hover { background: #27272A; }
.icon-action-btn {
  border: none;
  background: transparent;
  color: #A1A1AA;
  cursor: pointer;
  padding: 4px 6px;
  border-radius: 4px;
}
.icon-action-btn:hover {
  background: #27272A;
  color: #E4E4E7;
}

.sp-body {
  flex: 1;
  overflow-y: auto;
  padding: 32px 32px 80px 32px;
}
.sp-body::-webkit-scrollbar { width: 6px; }
.sp-body::-webkit-scrollbar-thumb { background: #3F3F46; border-radius: 4px; }

.sp-breadcrumb {
  font-size: 14px;
  font-weight: 500;
  color: #A1A1AA;
  margin-bottom: 12px;
}

.sp-title {
  font-size: 28px;
  font-weight: 700;
  color: #E4E4E7;
  margin: 0 0 12px 0;
  outline: none;
}
.sp-title:focus { border-bottom: 1px dashed #38BDF8; }

.sp-desc {
  font-size: 16px;
  color: #A1A1AA;
  margin: 0 0 40px 0;
  outline: none;
}

.description-editor-shell {
  margin-bottom: 28px;
  border: 1px solid #27272A;
  border-radius: 10px;
  background: #111111;
}

.description-toolbar {
  display: flex;
  align-items: center;
  gap: 4px;
  padding: 10px 12px;
  border-bottom: 1px solid #27272A;
  flex-wrap: wrap;
}

.floating-toolbar {
  position: fixed;
  z-index: 2600;
  background: #111317;
  border: 1px solid #27272A;
  border-radius: 8px;
  box-shadow: 0 12px 28px rgba(0,0,0,.35);
  padding: 8px;
}
.format-select, .color-trigger {
  background: #16181D;
  border: 1px solid #3F3F46;
  color: #E4E4E7;
  border-radius: 6px;
  padding: 5px 8px;
}
.color-menu { position: relative; }
.color-palette {
  display: none;
  position: absolute;
  top: 34px;
  left: 0;
  width: 184px;
  grid-template-columns: repeat(8, 18px);
  gap: 6px;
  padding: 10px;
  background: #111317;
  border: 1px solid #27272A;
  border-radius: 8px;
}
.color-menu:hover .color-palette { display: grid; }
.color-palette button {
  width: 18px;
  height: 18px;
  border-radius: 4px;
  border: 1px solid #3F3F46;
  cursor: pointer;
}

.rich-editor {
  min-height: 120px;
}

.rich-editor:empty::before {
  content: attr(data-placeholder);
  color: #52525B;
}

.comment-editor {
  min-height: 90px;
}

.embedded-image {
  max-width: 100%;
  border-radius: 8px;
  margin: 8px 0;
}

.sp-sub-actions {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 32px;
  color: #A1A1AA;
}
.sp-edit-info {
  font-size: 13px;
  display: flex;
  align-items: center;
  gap: 6px;
}

.sp-toolbar {
  display: flex;
  gap: 12px;
  flex-wrap: wrap;
  margin-bottom: 40px;
}
.sp-toolbar .s-btn {
  background: transparent;
  border: 1px solid #27272A;
  border-radius: 6px;
  padding: 8px 14px;
  color: #E4E4E7;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 8px;
  transition: background 0.2s;
}
.sp-toolbar .s-btn:hover { background: #1E1F21; border-color: #3F3F46; }

.quick-subtask-box {
  border: 1px solid #27272A;
  border-radius: 8px;
  background: #111111;
  padding: 12px;
  margin: -20px 0 28px;
}

.quick-subtask-input {
  width: 100%;
  background: transparent;
  border: 1px solid #27272A;
  border-radius: 6px;
  color: #E4E4E7;
  padding: 10px 12px;
  outline: none;
}

.quick-subtask-actions {
  display: flex;
  justify-content: flex-end;
  gap: 8px;
  margin-top: 10px;
}

.subtask-toggle-row {
  margin: -10px 0 14px;
}

.subtask-toggle-btn {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  background: transparent;
  border: 1px solid #27272A;
  color: #D4D4D8;
  border-radius: 999px;
  padding: 6px 12px;
  cursor: pointer;
}

.quick-subtask-cancel,
.quick-subtask-save {
  border-radius: 6px;
  padding: 6px 12px;
  font-size: 12px;
  cursor: pointer;
}

.quick-subtask-cancel {
  border: 1px solid #3F3F46;
  background: transparent;
  color: #D4D4D8;
}

.quick-subtask-save {
  border: none;
  background: #0EA5E9;
  color: #fff;
}

.subtask-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
  margin: -10px 0 28px;
}

.subtask-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 12px;
  border: 1px solid #27272A;
  border-radius: 8px;
  background: #111111;
  color: #E4E4E7;
  text-align: left;
}

.subtask-item:hover {
  border-color: #3F3F46;
  background: #17181C;
}

.subtask-open {
  display: flex;
  align-items: center;
  gap: 10px;
  flex: 1;
  min-width: 0;
  padding: 0;
  border: none;
  background: transparent;
  color: inherit;
  cursor: pointer;
  text-align: left;
}

.subtask-seq {
  color: #71717A;
  font-size: 12px;
  min-width: 70px;
}

.subtask-title {
  font-size: 13px;
  color: #D4D4D8;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.subtask-controls {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  flex-wrap: wrap;
  gap: 8px;
}

.subtask-chip,
.subtask-progress {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  border: 1px solid #27272A;
  border-radius: 999px;
  background: #0D0F11;
  color: #D4D4D8;
  padding: 5px 10px;
  font-size: 12px;
}

.subtask-chip {
  cursor: pointer;
}

.subtask-chip:hover {
  border-color: #3F3F46;
  background: #17181C;
}

.subtask-progress {
  color: #A1A1AA;
}

.subtask-progress input {
  width: 48px;
  border: none;
  background: transparent;
  color: #E4E4E7;
  font-size: 12px;
  text-align: right;
}

.subtask-progress input:disabled {
  color: #71717A;
  cursor: not-allowed;
}

.parent-context-banner {
  display: inline-flex;
  align-items: center;
  gap: 10px;
  margin: 8px 0 12px;
  padding: 6px 10px;
  border: 1px solid #1F2937;
  border-radius: 999px;
  background: #0F172A;
}

.parent-context-label {
  color: #94A3B8;
  font-size: 11px;
  font-weight: 700;
  letter-spacing: 0.08em;
  text-transform: uppercase;
}

.parent-context-link {
  border: none;
  background: transparent;
  color: #38BDF8;
  cursor: pointer;
  font-size: 13px;
  font-weight: 600;
}

.task-progress-editor {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}

.task-progress-input {
  width: 72px;
  border: 1px solid #27272A;
  border-radius: 6px;
  background: #111111;
  color: #E4E4E7;
  padding: 6px 8px;
  font-size: 13px;
}

.task-progress-input:disabled {
  color: #71717A;
  cursor: not-allowed;
}

.task-progress-suffix {
  color: #A1A1AA;
  font-size: 13px;
  font-weight: 600;
}

.task-progress-hint {
  color: #71717A;
  font-size: 12px;
}

.sp-section-title {
  font-size: 16px;
  font-weight: 600;
  color: #E4E4E7;
  margin: 0 0 24px 0;
}

.props-grid {
  display: flex;
  flex-direction: column;
  gap: 20px;
}
.p-row {
  display: flex;
  align-items: center;
  font-size: 14px;
}
.p-label {
  width: 140px;
  color: #A1A1AA;
  display: flex;
  align-items: center;
  gap: 10px;
}
.p-val {
  color: #E4E4E7;
  flex: 1;
}

.property-trigger {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  border: 1px solid transparent;
  background: transparent;
  color: #D4D4D8;
  border-radius: 6px;
  padding: 6px 10px;
  cursor: pointer;
}

.property-trigger:hover {
  border-color: #27272A;
  background: #1A1B1F;
}

.property-value {
  color: #F4F4F5;
  font-weight: 500;
}
.muted-val {
  color: #71717A;
}

.btn-add-label {
  background: #27272A;
  border: none;
  border-radius: 4px;
  padding: 4px 10px;
  color: #A1A1AA;
  font-size: 12px;
  cursor: pointer;
}

.icon-filter-btn {
  background: #27272A;
  border: none;
  color: #A1A1AA;
  padding: 6px 10px;
  border-radius: 4px;
  cursor: pointer;
}

.activity-feed {
  display: flex;
  flex-direction: column;
  gap: 24px;
  margin-bottom: 24px;
  position: relative;
  padding-left: 14px;
  border-left: 2px solid #27272A;
  margin-left: 10px;
}
.feed-item {
  position: relative;
  display: flex;
  gap: 16px;
}
.feed-icon {
  background: #27272A;
  width: 24px;
  height: 24px;
  border-radius: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 12px;
  color: #A1A1AA;
  position: absolute;
  left: -27px;
  top: 0;
}
.feed-avatar {
  background: #27272A;
  width: 28px;
  height: 28px;
  border-radius: 50%;
  border: 2px solid #151515;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 12px;
  color: #A1A1AA;
  position: absolute;
  left: -29px;
  top: -2px;
}
.feed-text {
  font-size: 14px;
  color: #E4E4E7;
}

.comment-box {
  background: transparent;
  margin-top: 24px;
}
.editor-wrap {
  border: 1px solid #27272A;
  border-radius: 6px;
  overflow: hidden;
  background: #111111;
}
.c-input {
  width: 100%;
  background: transparent;
  border: none;
  padding: 16px;
  color: #E4E4E7;
  outline: none;
  resize: none;
  font-family: inherit;
  font-size: 14px;
  height: 100px;
}
.c-toolbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 10px 16px;
  background: transparent;
  border-top: 1px solid #27272A;
}
.ct-left {
  display: flex;
  gap: 12px;
  color: #71717A;
  font-size: 13px;
}
.icon-hover:hover { color: #A1A1AA; cursor: pointer; }
.c-submit {
  background: #27272A;
  color: #A1A1AA;
  border: none;
  padding: 6px 16px;
  border-radius: 4px;
  font-size: 13px;
  font-weight: 500;
  cursor: not-allowed;
}

.comment-attachments {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  margin-top: 10px;
}

.comment-attachment-chip {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 6px 8px;
  border-radius: 6px;
  background: #1E2025;
  border: 1px solid #27272A;
  color: #D4D4D8;
  font-size: 12px;
  cursor: pointer;
  max-width: 260px;
}
.comment-attachment-chip span {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.comment-image-thumb {
  width: 28px;
  height: 28px;
  object-fit: cover;
  border-radius: 4px;
  border: 1px solid #3F3F46;
}
.image-lightbox {
  position: fixed;
  inset: 0;
  z-index: 3000;
  background: rgba(0, 0, 0, .76);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 32px;
}
.image-lightbox-panel {
  position: relative;
  max-width: min(960px, 96vw);
  max-height: 92vh;
  background: #0D0F11;
  border: 1px solid #27272A;
  border-radius: 8px;
  overflow: hidden;
}
.image-lightbox-panel img {
  display: block;
  max-width: 100%;
  max-height: calc(92vh - 54px);
  object-fit: contain;
  background: #050607;
}
.lightbox-close {
  position: absolute;
  top: 10px;
  right: 10px;
  width: 32px;
  height: 32px;
  border: 1px solid #3F3F46;
  background: #16181D;
  color: #fff;
  border-radius: 6px;
  cursor: pointer;
}
.lightbox-footer {
  height: 54px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  padding: 0 14px;
  color: #D4D4D8;
}
.lightbox-actions {
  display: flex;
  align-items: center;
  gap: 10px;
}
.zoom-control {
  display: inline-flex;
  align-items: center;
  gap: 8px;
}
.zoom-control input {
  width: 120px;
}
.lightbox-delete {
  border: 1px solid #7F1D1D;
  background: #450A0A;
  color: #FCA5A5;
  border-radius: 6px;
  padding: 7px 10px;
  cursor: pointer;
}
.download-btn {
  color: #fff;
  background: #2563EB;
  border-radius: 6px;
  padding: 7px 10px;
  text-decoration: none;
  display: inline-flex;
  align-items: center;
  gap: 6px;
}

.comment-rendered :deep(p) {
  margin: 0 0 8px;
}

.comment-inline-code {
  background: #27272A;
  color: #F472B6;
  padding: 1px 6px;
  border-radius: 4px;
  font-size: 13px;
}

.comment-code-block {
  background: #0F1115;
  border: 1px solid #27272A;
  border-radius: 8px;
  padding: 12px;
  overflow-x: auto;
  font-family: Consolas, monospace;
  margin: 8px 0;
}

.comment-list {
  margin: 8px 0 8px 20px;
  list-style: disc;
}

.comment-list.ordered {
  list-style: decimal;
}

.activity-empty-state {
  color: #71717A;
  padding: 12px 0 24px;
}

.dark-dropdown { background: #1E2025 !important; border: 1px solid #333 !important; }
.dark-dropdown .el-dropdown-menu__item { color: #E4E4E7 !important; }
.dark-dropdown .el-dropdown-menu__item:hover { background: #27272A !important; }

/* Plane-like custom popover styles */
:global(.plane-popover) {
  background: #18191B !important;
  border: 1px solid #27272A !important;
  padding: 0 !important;
  border-radius: 8px !important;
  box-shadow: 0 10px 25px rgba(0,0,0,0.5) !important;
}

:global(.plane-popover .popover-content) {
  display: flex;
  flex-direction: column;
}

:global(.plane-popover .popover-search) {
  background: transparent;
  border: none;
  border-bottom: 1px solid #27272A;
  color: #E4E4E7;
  padding: 12px 14px;
  font-size: 13px;
  outline: none;
  width: 100%;
}
:global(.plane-popover .popover-search::placeholder) {
  color: #71717A;
}

:global(.plane-popover .popover-list) {
  max-height: 250px;
  overflow-y: auto;
  padding: 6px;
}
:global(.plane-popover .popover-list::-webkit-scrollbar) {
  width: 6px;
}
:global(.plane-popover .popover-list::-webkit-scrollbar-thumb) {
  background: #27272A;
  border-radius: 4px;
}

:global(.plane-popover .popover-item) {
  display: flex;
  align-items: center;
  padding: 8px 10px;
  border-radius: 4px;
  cursor: pointer;
  color: #D4D4D8;
  font-size: 13px;
  transition: background 0.15s;
}
:global(.plane-popover .popover-item:hover) {
  background: #27272A;
  color: #FFFFFF;
}

.assignee-progress-list {
  border-top: 1px solid #27272A;
  padding: 8px 10px 10px;
}

.assignee-progress-title {
  color: #71717A;
  font-size: 11px;
  font-weight: 600;
  margin-bottom: 8px;
  text-transform: uppercase;
}

.assignee-progress-row {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 6px;
}

.assignee-progress-name {
  color: #D4D4D8;
  flex: 1;
  font-size: 12px;
  min-width: 0;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.assignee-progress-input {
  background: #0D0F11;
  border: 1px solid #27272A;
  border-radius: 4px;
  color: #E4E4E7;
  font-size: 12px;
  padding: 4px 6px;
  width: 58px;
}

.assignee-progress-suffix {
  color: #71717A;
  font-size: 12px;
}

.t-btn-date:deep(.el-input__wrapper) {
  background-color: transparent !important;
  box-shadow: none !important;
  padding: 0 !important;
}

.t-btn-date:deep(.el-input__inner) {
  color: #D4D4D8 !important;
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
  border-color: #27272A;
  background: #1A1B1F !important;
}
</style>
