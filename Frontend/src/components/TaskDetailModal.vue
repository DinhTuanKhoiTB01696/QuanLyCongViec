<template>
  <transition name="fade">
    <div class="task-modal-overlay" v-if="showTaskModal" @mousedown.self="showTaskModal = false">
      
      <!-- MODE: CREATE NEW WORK ITEM (Image 1) -->
      <div class="create-centered-modal" v-if="selectedTask?.isNew">
        <h3 class="cm-title">{{ tr('Create new work item', 'Tạo công việc mới') }}</h3>
        
        <div class="cm-badge-row">
           <div class="cm-badge">
             <i class="fa-solid fa-bell" style="color: #F59E0B"></i> {{ currentProjectBadge }}
           </div>
        </div>

        <div class="cm-form-group">
          <input type="text" class="cm-inputbox" :placeholder="tr('Title', 'Tiêu đề')" v-model="selectedTask.title" />
          <textarea class="cm-textareabox" :placeholder="tr('Add description...', 'Thêm mô tả...')" v-model="selectedTask.description"></textarea>
        </div>
        
        <div class="cm-toolbar-row">
           <!-- STATUS -->
           <el-dropdown trigger="click" @command="(cmd) => selectStatus(cmd)">
             <div class="t-btn"><i :class="getStatusIcon(selectedTask?.statusName)"></i> <span>{{ tr('Status', 'Trạng thái') }}</span> {{ getStatusLabel(selectedTask?.statusName) }}</div>
             <template #dropdown>
               <el-dropdown-menu class="theme-dropdown">
                 <el-dropdown-item v-for="status in projectStatuses" :key="status.id" :command="status.name"><i :class="getStatusIcon(status.name)" class="mr-2"></i> {{ getStatusLabel(status.name || status.displayName) }}</el-dropdown-item>
               </el-dropdown-menu>
             </template>
           </el-dropdown>

           <!-- PRIORITY -->
           <el-dropdown  trigger="click" @command="(cmd) => selectedTask.priority = cmd">
             <div class="t-btn"><i :class="getPrioIcon(selectedTask?.priority)"></i> <span>{{ tr('Priority', 'Độ ưu tiên') }}</span> {{ getPrioLabel(selectedTask?.priority) }}</div>
             <template #dropdown>
               <el-dropdown-menu class="theme-dropdown">
                 <el-dropdown-item :command="1"><i class="fa-solid fa-angles-up mr-2" style="color: #ef4444"></i> {{ tr('Urgent', 'Khẩn cấp') }}</el-dropdown-item>
                 <el-dropdown-item :command="2"><i class="fa-solid fa-chevron-up mr-2" style="color: #f59e0b"></i> {{ tr('High', 'Cao') }}</el-dropdown-item>
                 <el-dropdown-item :command="3"><i class="fa-solid fa-minus mr-2" style="color: #3b82f6"></i> {{ tr('Medium', 'Trung bình') }}</el-dropdown-item>
                 <el-dropdown-item :command="4"><i class="fa-solid fa-arrow-down mr-2" style="color: var(--color-text-muted)"></i> {{ tr('Low', 'Thấp') }}</el-dropdown-item>
                 <el-dropdown-item :command="0"><i class="fa-solid fa-ban mr-2 text-muted"></i> {{ tr('None', 'Không có') }}</el-dropdown-item>
               </el-dropdown-menu>
             </template>
           </el-dropdown>

           <!-- ASSIGNEES -->
           <el-popover  placement="bottom-start" trigger="click" popper-class="plane-popover" :width="220" :disabled="!canManageTaskAssignees" @show="assigneeSearch = ''">
             <template #reference>
           <div class="t-btn" :class="{ disabled: !canManageTaskAssignees }"><i class="fa-regular fa-user"></i> <span>{{ tr('Assignee', 'Người thực hiện') }}</span> {{ getAssigneeSummary() }}</div>
             </template>
             <div class="popover-content">
               <input type="text" v-model="assigneeSearch" class="popover-search" :placeholder="tr('Find assignee...', 'Tìm người thực hiện...')" />
                <div class="popover-list">
                  <div class="popover-item flex items-center justify-between transition-colors cursor-pointer" 
                       v-for="user in filteredMembers" 
                       :key="user.userId" 
                       @click="toggleAssignee(user.userId)"
                       :class="getAssigneeIds().includes(user.userId) ? 'bg-green-100 hover:bg-green-200 text-green-900 border-l-4 border-green-500 rounded-sm' : 'hover:bg-gray-100'">
                    <div class="flex items-center truncate max-w-[75%] pl-2 py-1">
                      <UserAvatar :user="{ avatarColor: user.avatarColor, initials: user.initials, fullName: user.fullName, email: user.email, id: user.userId }" :size="20" :fontSize="9" class="mr-2" />
                      <span class="truncate" :class="getAssigneeIds().includes(user.userId) ? 'font-semibold' : ''">{{ user.fullName || user.email }}</span>
                    </div>
                    <div class="flex items-center flex-shrink-0 pr-2">
                      <span v-if="user.taskPercentage !== undefined" class="text-[10px] px-1.5 py-0.5 rounded mr-2 font-semibold" :class="getAssigneeIds().includes(user.userId) ? 'bg-green-200 text-green-800' : 'bg-sky-100 text-sky-600'" :title="tr('Current workload', 'Khối lượng hiện tại')">{{ user.taskPercentage }}%</span>
                    </div>
                  </div>
                  <div v-if="!filteredMembers.length" class="text-xs text-center text-muted py-2">{{ tr('No assignees found.', 'Không tìm thấy người thực hiện.') }}</div>
                </div>
                <div class="assignee-progress-list" v-if="selectedAssigneeRows.length">
                  <div class="assignee-progress-title">{{ tr('Progress by assignee', 'Tiến độ theo người thực hiện') }}</div>
                  <div class="assignee-progress-row" v-for="assignee in selectedAssigneeRows" :key="assignee.userId">
                    <span class="assignee-progress-name">{{ assignee.fullName || assignee.email || tr('Member', 'Thành viên') }}</span>
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
                      v-if="showEstimateFeatures"
                      class="assignee-progress-input"
                      type="number"
                      min="0"
                      step="0.5"
                      :disabled="!canManageTaskAssignees"
                      :value="assignee.estimatedHours || 0"
                      @change="event => updateAssigneeEstimatedHours(assignee.userId, event.target.value)"
                    />
                    <span v-if="showEstimateFeatures" class="assignee-progress-suffix">h</span>
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
               <div class="t-btn"><i class="fa-solid fa-tag"></i> {{ selectedTask?.labelIds?.length ? selectedTask.labelIds.length + ' ' + tr('Labels', 'Nhãn') : tr('Labels', 'Nhãn') }}</div>
             </template>
             <div class="popover-content">
               <input type="text" v-model="labelSearch" class="popover-search" :placeholder="tr('Search labels...', 'Tìm nhãn...')" />
               <div class="popover-list">
                 <div class="popover-item" v-for="l in filteredLabels" :key="l.id" @click="toggleSelectedLabel(l.id)">
                   <div class="popover-c-circle mr-2 w-3 h-3 rounded-full" :style="{ backgroundColor: l.color || 'var(--color-accent)' }"></div>
                   <span>{{ l.name }}</span>
                   <i v-if="selectedTask?.labelIds?.includes(l.id)" class="fa-solid fa-check ms-auto"></i>
                 </div>
                 <div class="popover-item pointer hover-bg-accent" v-if="filteredLabels.length === 0 && labelSearch" @click="createLabel(labelSearch)">
                   <span>{{ tr('Add', 'Thêm') }} "{{ labelSearch }}"</span>
                 </div>
               </div>
             </div>
           </el-popover>

           <!-- DATES -->
           <el-date-picker
             v-model="selectedTask.plannedStartDate"
             type="date"
             placeholder="Ngày bắt đầu"
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
              :placeholder="tr('Due date', 'Hạn hoàn thành')"
              class="t-btn-date"
              format="MMM DD"
              value-format="YYYY-MM-DD"
              :disabled-date="disableDueDates"
              style="width:125px; height:28px"
              @change="val => handleTaskDateChange('dueDate', val)"
            />
            <div v-if="showEstimateFeatures" class="t-btn t-btn-number">
              <i class="fa-regular fa-hourglass-half"></i>
              <span>{{ tr('Estimate', 'Thời gian ước tính') }}</span>
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
                <span>{{ tr('Visibility', 'Quyền truy cập') }}</span>
                {{ getVisibilityLabel(selectedTask) }}
              </div>
              <template #dropdown>
                <el-dropdown-menu class="theme-dropdown">
                  <el-dropdown-item command="project">{{ tr('Project members', 'Thành viên dự án') }}</el-dropdown-item>
                  <el-dropdown-item command="assigned">{{ tr('Assigned only', 'Chỉ người được giao') }}</el-dropdown-item>
                  <el-dropdown-item command="role">{{ tr('Role scoped', 'Theo vai trò') }}</el-dropdown-item>
                </el-dropdown-menu>
              </template>
            </el-dropdown>

            <el-popover v-if="isRoleVisibilityEnabled && selectedTask?.visibilityMode === 'role'" placement="bottom-start" trigger="click" popper-class="plane-popover" :width="260" :disabled="!canEditTaskVisibility">
              <template #reference>
                <div class="t-btn" :class="{ disabled: !canEditTaskVisibility }">
                  <i class="fa-solid fa-user-shield"></i>
                  <span>{{ tr('Roles', 'Vai trò') }}</span>
                  {{ selectedTask?.visibleToRoles?.length ? selectedTask.visibleToRoles.join(', ') : tr('Select roles', 'Chọn vai trò') }}
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
                <input type="text" v-model="cycleSearch" class="popover-search" :placeholder="tr('Search cycles...', 'Tìm chu kỳ...')" />
                <div class="popover-list">
                  <div class="popover-item" @click="selectedTask.sprintId = null">
                    <i class="fa-solid fa-circle-half-stroke mr-2 w-4 text-center"></i> {{ tr('No cycle', 'Không có chu kỳ') }}
                    <i v-if="!selectedTask?.sprintId" class="fa-solid fa-check ms-auto"></i>
                  </div>
                  <div class="popover-item" v-for="c in filteredCycles" :key="c.id" @click="selectedTask.sprintId = c.id">
                    <i class="fa-solid fa-certificate mr-2 w-4 text-center text-blue-500"></i>
                    <span class="truncate flex-1">{{ c.name }}</span>
                    <i v-if="selectedTask?.sprintId === c.id" class="fa-solid fa-check ms-auto"></i>
                  </div>
                  <div v-if="!filteredCycles.length" class="text-xs text-center text-muted py-2">{{ tr('No cycles setup.', 'Chưa thiết lập chu kỳ.') }}</div>
                </div>
              </div>
            </el-popover>

            <!-- MODULES -->
            <el-popover  placement="bottom-start" trigger="click" popper-class="plane-popover" :width="280" @show="moduleSearch = ''">
              <template #reference>
                <div class="t-btn"><i class="fa-solid fa-cube"></i> {{ getModuleLabel(selectedTask?.moduleId) }}</div>
              </template>
              <div class="popover-content">
                <input type="text" v-model="moduleSearch" class="popover-search" :placeholder="tr('Search module...', 'Tìm module...')" />
                <div class="popover-list">
                  <div class="popover-item" @click="selectedTask.moduleId = null">
                    <i class="fa-solid fa-cube mr-2"></i> {{ tr('No module', 'Không có phân hệ') }}
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
                <div class="t-btn"><i class="fa-solid fa-arrow-turn-up fa-rotate-90"></i> {{ getParentId(selectedTask) ? tr('Parent selected', 'Đã chọn công việc cha') : tr('Add parent', 'Thêm công việc cha') }}</div>
              </template>
              <div class="popover-content h-[250px] flex flex-col bg-surface-elevation">
                <div class="p-2 border-b border-theme">
                  <div class="relative flex items-center">
                    <i class="fa-solid fa-magnifying-glass absolute left-2 text-muted"></i>
                    <input type="text" v-model="parentSearch" class="w-full bg-transparent border-none text-primary pl-8 focus:outline-none" :placeholder="tr('Search parent task...', 'Tìm công việc cha...')" />
                  </div>
                </div>
                <div class="flex-1 overflow-y-auto no-scrollbar p-2">
                  <div class="popover-item text-xs text-muted hover:text-primary cursor-pointer p-2 rounded hover:bg-surface-hover flex items-center" @click="setTaskParent(selectedTask, null)">
                    <i class="fa-solid fa-ban mr-2"></i> {{ tr('Remove parent', 'Xóa công việc cha') }}
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
              <el-switch v-model="createMore" size="small" style="--el-switch-on-color: #38bdf8;" /> <span>{{ tr('Create more', 'Tạo liên tục') }}</span>
           </div>
           <button class="btn-discard" @click="discardNewTask">{{ tr('Discard', 'Hủy') }}</button>
           <button class="btn-save" @click="submitNewTask">{{ tr('Save', 'Lưu') }}</button>
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
                   {{ isSubscribed ? tr('Unsubscribe', 'Bỏ theo dõi') : tr('Subscribe', 'Theo dõi') }}
                </button>
                <button class="s-btn" @click="copyTaskLink">
                   <i class="fa-solid fa-link"></i> 
                   {{ tr('Copy link', 'Sao chép link') }}
                </button>
                <el-dropdown trigger="click" @command="handleTaskMenuCommand">
                  <button class="s-btn s-btn-icon" :title="tr('More actions', 'Thao tác khác')">
                     <i class="fa-solid fa-ellipsis"></i>
                  </button>
                  <template #dropdown>
                    <el-dropdown-menu class="theme-dropdown">
                      <el-dropdown-item command="copy"><i class="fa-solid fa-link mr-2"></i> {{ tr('Copy link', 'Sao chép link') }}</el-dropdown-item>
                      <el-dropdown-item command="duplicate"><i class="fa-regular fa-clone mr-2"></i> {{ tr('Duplicate', 'Nhân bản') }}</el-dropdown-item>
                      <el-dropdown-item command="archive"><i class="fa-solid fa-box-archive mr-2"></i> {{ tr('Archive task', 'Lưu trữ sau') }}</el-dropdown-item>
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
              <span class="parent-context-label">{{ tr('Parent task', 'Công việc cha') }}</span>
              <button class="parent-context-link" type="button" @click="openParentTask">
                {{ currentParentLabel }}
              </button>
            </div>
            
            <h1 class="sp-title" contenteditable @blur="(e) => updateTaskField(selectedTask, 'title', e.target.innerText)">{{ selectedTask?.title }}</h1>
            <div class="description-editor-shell">
              <div v-if="showFormatToolbar" class="description-toolbar floating-toolbar" :style="{ left: toolbarPosition.x + 'px', top: toolbarPosition.y + 'px' }">
                <select class="format-select" @change="applyBlockFormat($event.target.value)">
                  <option value="div">{{ tr('Text', 'Văn bản') }}</option>
                  <option value="h1">{{ tr('Heading 1', 'Tiêu đề 1') }}</option>
                  <option value="h2">{{ tr('Heading 2', 'Tiêu đề 2') }}</option>
                  <option value="h3">{{ tr('Heading 3', 'Tiêu đề 3') }}</option>
                  <option value="blockquote">{{ tr('Quote', 'Trích dẫn') }}</option>
                </select>
                <div class="color-menu">
                  <button class="color-trigger">{{ tr('Color', 'Màu sắc') }}</button>
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
                :data-placeholder="selectedTask?.description ? '' : tr('Add description...', 'Thêm mô tả... ')"
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
                  <i class="fa-solid fa-clock-rotate-left"></i> {{ tr('Edited by', 'Chỉnh sửa bởi') }} <b>{{ lastEditedBy }}</b> {{ lastEditedRelative }}
                </div>
            </div>

            <!-- Action Chips -->
             <div class="sp-toolbar">
                <button class="s-btn" @click="startCreateSubtask"><i class="fa-solid fa-layer-group"></i> {{ tr('Add sub-work item', 'Thêm công việc con') }}</button>
                <button class="s-btn s-btn-primary" :disabled="isAiBreakingDown" @click="createSubtasksWithAI">
                  <i class="fa-solid fa-wand-magic-sparkles"></i>
                  {{ isAiBreakingDown ? tr('AI is preparing...', 'AI đang chuẩn bị...') : tr('AI breakdown subtasks', 'AI tách thành công việc con') }}
                </button>
                
                <button class="s-btn" @click="triggerDescriptionFileUpload"><i class="fa-solid fa-paperclip"></i> {{ tr('Attachment', 'Đính kèm') }}</button>
             </div>
            <div v-if="aiSubtaskPreview.length" class="ai-preview-panel">
              <div class="ai-preview-head">
                <div>
                  <strong>{{ tr('AI subtask preview', 'Bản xem trước công việc con từ AI') }}</strong>
                  <p>{{ tr('Review these suggested sub-work items before creating them.', 'Xem xét các công việc con được gợi ý này trước khi tạo.') }}</p>
                </div>
                <div class="ai-preview-actions">
                  <button class="quick-subtask-cancel" @click="discardAiSubtaskPreview">{{ tr('Discard', 'Hủy') }}</button>
                  <button class="quick-subtask-save" :disabled="isCreatingPreviewSubtasks" @click="confirmAiSubtaskPreview">
                    {{ isCreatingPreviewSubtasks ? tr('Creating...', 'Đang tạo...') : tr(`Create ${aiSubtaskPreview.length} sub-work items`, `Tạo ${aiSubtaskPreview.length} công việc con`) }}
                  </button>
                </div>
              </div>
              <div class="ai-preview-list">
                <div v-for="(subtask, index) in aiSubtaskPreview" :key="`ai-preview-${index}`" class="ai-preview-item">
                  <div class="ai-preview-top">
                    <strong>{{ subtask.title }}</strong>
                    <span>{{ Number(subtask.estHours || 0).toFixed(1) }}h · P{{ subtask.priority || 3 }}</span>
                  </div>
                  <p>{{ subtask.description || tr('No description', 'Không có mô tả') }}</p>
                </div>
              </div>
            </div>
            <div v-if="isCreatingSubtask" class="quick-subtask-box">
              <input
                ref="subtaskInputRef"
                v-model="newSubtaskTitle"
                type="text"
                class="quick-subtask-input"
                :placeholder="tr('Create a linked sub-work item', 'Tạo công việc con liên kết')"
                @keyup.enter="submitSubtask"
                @keyup.esc="isCreatingSubtask = false"
              />
              <div class="quick-subtask-actions">
                <button class="quick-subtask-cancel" @click="isCreatingSubtask = false">{{ tr('Cancel', 'Hủy') }}</button>
                <button class="quick-subtask-save" @click="submitSubtask">{{ tr('Create', 'Tạo') }}</button>
              </div>
            </div>
             <section class="subtask-section" v-if="subtasksList.length">
               <div class="subtask-section-head">
                 <div>
                   <span class="subtask-kicker">{{ tr('Sub-work items', 'Công việc con') }}</span>
                   <strong>{{ subtasksList.length }} {{ tr('linked items', 'việc liên kết') }}</strong>
                 </div>
                 <button class="subtask-toggle-btn" @click="showSubtasks = !showSubtasks">
                   <i :class="showSubtasks ? 'fa-solid fa-eye' : 'fa-solid fa-eye-slash'"></i>
                   {{ showSubtasks ? tr('Hide', 'Ẩn') : tr('Show', 'Hiện') }}
                 </button>
               </div>
             </section>
             <div v-if="subtasksList.length && showSubtasks" class="subtask-list">
               <div
                 v-for="subtask in subtasksList"
                 :key="subtask.id"
                 class="subtask-item"
               >
                <div class="subtask-main">
                  <button class="subtask-open" type="button" @click="openTaskDetail(subtask)">
                    <span class="subtask-seq">{{ subtask.sequenceId || subtask.id?.substring(0, 8) }}</span>
                    <span class="subtask-title">{{ subtask.title }}</span>
                  </button>
                  <div class="subtask-controls" @click.stop>
                    <el-dropdown trigger="click" @command="(cmd) => selectStatus({ name: cmd }, subtask)">
                      <button class="subtask-chip" type="button" :style="{ color: getStatusColor(subtask.statusName), borderColor: getStatusColor(subtask.statusName), background: `color-mix(in srgb, ${getStatusColor(subtask.statusName)} 15%, transparent)` }">
                        <i :class="getStatusIcon(subtask.statusName)"></i>
                        <span style="margin-left: 4px;">{{ getStatusLabel(subtask.statusName) }}</span>
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
                      <button class="subtask-chip" type="button" :style="{ color: getPriorityColor(subtask.priority), borderColor: getPriorityColor(subtask.priority), background: `color-mix(in srgb, ${getPriorityColor(subtask.priority)} 15%, transparent)` }">
                        <i :class="getPrioIcon(subtask.priority)"></i>
                        <span style="margin-left: 4px;">{{ getPrioLabel(subtask.priority) }}</span>
                      </button>
                      <template #dropdown>
                        <el-dropdown-menu class="theme-dropdown">
                          <el-dropdown-item :command="1"><i class="fa-solid fa-angles-up mr-2" style="color: var(--color-danger)"></i> {{ tr('Urgent', 'Khẩn cấp') }}</el-dropdown-item>
                          <el-dropdown-item :command="2"><i class="fa-solid fa-chevron-up mr-2" style="color: var(--color-warning)"></i> {{ tr('High', 'Cao') }}</el-dropdown-item>
                          <el-dropdown-item :command="3"><i class="fa-solid fa-minus mr-2" style="color: var(--color-accent)"></i> {{ tr('Medium', 'Trung bình') }}</el-dropdown-item>
                          <el-dropdown-item :command="4"><i class="fa-solid fa-arrow-down mr-2" style="color: var(--color-text-muted)"></i> {{ tr('Low', 'Thấp') }}</el-dropdown-item>
                          <el-dropdown-item :command="0"><i class="fa-solid fa-ban mr-2 text-muted"></i> {{ tr('None', 'Không có') }}</el-dropdown-item>
                        </el-dropdown-menu>
                      </template>
                    </el-dropdown>

                    <el-popover placement="bottom-start" trigger="click" popper-class="plane-popover" :width="240" @show="assigneeSearch = ''">
                      <template #reference>
                        <button class="subtask-chip" type="button" style="padding: 2px 8px;">
                          <UserAvatar v-if="buildTaskAssigneeRows(subtask).length === 1" :user="buildTaskAssigneeRows(subtask)[0]" :size="16" :fontSize="8" />
                          <i v-else class="fa-regular fa-user"></i>
                          <span style="margin-left: 4px;">{{ getAssigneeSummary(subtask) }}</span>
                        </button>
                      </template>
                      <div class="popover-content">
                        <input v-model="assigneeSearch" type="text" class="popover-search" :placeholder="tr('Search members...', 'Tìm thành viên... ')" />
                        <div class="popover-list">
                          <div class="popover-item flex items-center justify-between transition-colors cursor-pointer" 
                               v-for="member in filteredMembers" 
                               :key="`${subtask.id}-${member.userId}`" 
                               @click="toggleInlineTaskAssignee(subtask, member.userId)"
                               :class="getAssigneeIds(subtask).includes(member.userId) ? 'bg-green-100 hover:bg-green-200 text-green-900 border-l-4 border-green-500 rounded-sm' : 'hover:bg-gray-100'">
                            <div class="flex items-center truncate max-w-[75%] pl-2 py-1">
                              <UserAvatar :user="{ avatarColor: member.avatarColor, initials: member.initials, fullName: member.fullName, email: member.email, id: member.userId }" :size="20" :fontSize="9" class="mr-2" />
                              <span class="truncate" :class="getAssigneeIds(subtask).includes(member.userId) ? 'font-semibold' : ''">{{ member.fullName || member.email }}</span>
                            </div>
                            <div class="flex items-center flex-shrink-0 pr-2">
                              <span v-if="member.taskPercentage !== undefined" class="text-[10px] px-1.5 py-0.5 rounded mr-2 font-semibold" :class="getAssigneeIds(subtask).includes(member.userId) ? 'bg-green-200 text-green-800' : 'bg-sky-100 text-sky-600'" :title="tr('Current workload', 'Khối lượng hiện tại')">{{ member.taskPercentage }}%</span>
                            </div>
                          </div>
                        </div>
                      </div>
                    </el-popover>
                  </div>
                </div>

                <div class="subtask-progress-wrap" @click.stop>
                  <div class="subtask-progress-top">
                    <span>{{ tr('Progress', 'Tiến độ') }}</span>
                    <strong>{{ getTaskProgressPercent(subtask) }}%</strong>
                  </div>
                  <div class="subtask-progress-track" aria-hidden="true">
                    <span :style="{ width: `${getTaskProgressPercent(subtask)}%` }"></span>
                  </div>
                  <label class="subtask-progress">
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

            <h3 class="sp-section-title">{{ tr('Properties', 'Thuộc tính') }}</h3>
             <div class="props-grid">
                 <div class="p-row">
                   <div class="p-label"><i class="fa-regular fa-circle-dot"></i> {{ tr('Status', 'Trạng thái') }}</div>
                   <div class="p-val">
                     <el-popover placement="bottom-start" trigger="click" popper-class="plane-popover" :width="260" @show="statusSearch = ''">
                       <template #reference>
                         <button class="property-trigger status-property-trigger" :style="{ '--status-color': getStatusColor(selectedTask?.statusName) }">
                           <i :class="getStatusIcon(selectedTask?.statusName)"></i>
                           <span>{{ tr('Status', 'Trạng thái') }}</span>
                           <span class="property-value" :style="{ color: getStatusColor(selectedTask?.statusName), padding: '2px 8px', borderRadius: '4px', background: `color-mix(in srgb, ${getStatusColor(selectedTask?.statusName)} 15%, transparent)` }">{{ getStatusLabel(selectedTask?.statusName) }}</span>
                         </button>
                       </template>
                       <div class="popover-content">
                         <input v-model="statusSearch" type="text" class="popover-search" :placeholder="tr('Search status...', 'Tìm trạng thái... ')" />
                         <div class="popover-list">
                           <div class="popover-item status-popover-item" v-for="status in filteredStatuses" :key="status.id" :style="{ '--status-color': getStatusColor(status.name) }" @click="selectStatus(status)">
                             <i :class="getStatusIcon(status.name)" class="mr-2"></i>
                             <span>{{ getStatusLabel(status.name || status.displayName) }}</span>
                             <i v-if="selectedTask?.statusName === status.name" class="fa-solid fa-check ms-auto"></i>
                           </div>
                         </div>
                       </div>
                     </el-popover>
                   </div>
                 </div>
                 <div class="p-row">
                   <div class="p-label"><i class="fa-solid fa-percent"></i> {{ tr('Progress', 'Tiến độ') }}</div>
                   <div class="p-val">
                     <div class="task-progress-editor" :style="{ '--progress-color': getStatusColor(selectedTask?.statusName), '--progress-width': `${getTaskProgressPercent(selectedTask)}%` }">
                       <div class="task-progress-readout">
                         <span>{{ getStatusLabel(selectedTask?.statusName) }}</span>
                         <strong>{{ getTaskProgressPercent(selectedTask) }}%</strong>
                       </div>
                       <div class="task-progress-bar" aria-hidden="true"><span></span></div>
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
                         {{ tr('Progress follows the current status and assignee completion levels.', 'Tiến độ tự cập nhật theo trạng thái hiện tại và mức hoàn thành của người được giao.') }}
                       </span>
                     </div>
                   </div>
                 </div>
                 <div class="p-row">
                   <div class="p-label"><i class="fa-regular fa-user"></i> {{ tr('Assignee', 'Người thực hiện') }}</div>
                   <div class="p-val">
                     <el-popover placement="bottom-start" trigger="click" popper-class="plane-popover" :width="260" :disabled="!canManageTaskAssignees" @show="assigneeSearch = ''">
                       <template #reference>
                         <button class="property-trigger" :class="{ 'muted-val': !getAssigneeIds().length }" :disabled="!canManageTaskAssignees">
                           <i class="fa-regular fa-user"></i>
                           <span>{{ tr('Assignee', 'Người thực hiện') }}</span>
                           <span class="property-value" style="display: flex; align-items: center; gap: 4px;">
                             <UserAvatar v-if="selectedAssigneeRows.length === 1" :user="selectedAssigneeRows[0]" :size="16" :fontSize="8" />
                             {{ getAssigneeSummary() }}
                           </span>
                         </button>
                       </template>
                       <div class="popover-content">
                         <input v-model="assigneeSearch" type="text" class="popover-search" :placeholder="tr('Search members...', 'Tìm thành viên... ')" />
                         <div class="popover-list">
                           <div class="popover-item flex items-center justify-between transition-colors cursor-pointer" 
                                v-for="member in filteredMembers" 
                                :key="member.userId" 
                                @click="toggleAssignee(member.userId)"
                                :class="getAssigneeIds().includes(member.userId) ? 'bg-green-100 hover:bg-green-200 text-green-900 border-l-4 border-green-500 rounded-sm' : 'hover:bg-gray-100'">
                             <div class="flex items-center truncate max-w-[75%] pl-2 py-1">
                               <UserAvatar :user="{ avatarColor: member.avatarColor, initials: member.initials, fullName: member.fullName, email: member.email, id: member.userId }" :size="20" :fontSize="9" class="mr-2" />
                               <span class="truncate" :class="getAssigneeIds().includes(member.userId) ? 'font-semibold' : ''">{{ member.fullName || member.email }}</span>
                             </div>
                             <div class="flex items-center flex-shrink-0 pr-2">
                               <span v-if="member.taskPercentage !== undefined" class="text-[10px] px-1.5 py-0.5 rounded mr-2 font-semibold" :class="getAssigneeIds().includes(member.userId) ? 'bg-green-200 text-green-800' : 'bg-sky-100 text-sky-600'" :title="tr('Current workload', 'Khối lượng hiện tại')">{{ member.taskPercentage }}%</span>
                             </div>
                           </div>
                         </div>
                       </div>
                     </el-popover>
                     <button v-if="canUseAiAssigneeSuggestion" class="property-trigger estimate-suggestion-btn ai-estimate-btn mt-2" :disabled="isAiSuggestingAssignees" @click="suggestAssigneesWithAI()">
                       <i class="fa-solid fa-user-group"></i>
                       <span>{{ isAiSuggestingAssignees ? tr('AI is suggesting...', 'AI đang gợi ý...') : tr('AI suggest assignees', 'AI gợi ý người thực hiện') }}</span>
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
                   <div class="p-label"><i class="fa-solid fa-eye"></i> {{ tr('Visibility', 'Quyền truy cập') }}</div>
                   <div class="p-val">
                     <el-dropdown trigger="click" @command="(cmd) => selectVisibilityMode(cmd)">
                       <div class="property-trigger" :class="{ 'muted-val': !selectedTask?.visibilityMode }">
                         <i class="fa-solid fa-eye"></i>
                         <span>{{ tr('Visibility', 'Quyền truy cập') }}</span>
                         <span class="property-value">{{ getVisibilityLabel(selectedTask) }}</span>
                       </div>
                       <template #dropdown>
                         <el-dropdown-menu class="theme-dropdown">
                           <el-dropdown-item command="project">{{ tr('Project members', 'Thành viên dự án') }}</el-dropdown-item>
                           <el-dropdown-item command="assigned">{{ tr('Assigned only', 'Chỉ người được giao') }}</el-dropdown-item>
                           <el-dropdown-item command="role">{{ tr('Role scoped', 'Theo vai trò') }}</el-dropdown-item>
                         </el-dropdown-menu>
                       </template>
                     </el-dropdown>
                     <div v-if="selectedTask?.visibilityMode === 'role'" class="estimate-breakdown mt-2">
                       <div class="estimate-breakdown-row">
                         <span>{{ tr('Visible roles', 'Vai trò được xem') }}</span>
                         <strong>{{ selectedTask?.visibleToRoles?.length ? selectedTask.visibleToRoles.join(', ') : tr('None', 'Không có') }}</strong>
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
                           {{ selectedTask?.visibleToRoles?.includes(role) ? tr('Unselect', 'Bỏ chọn') : tr('Select', 'Chọn') }} {{ role }}
                         </button>
                       </div>
                     </div>
                   </div>
                 </div>
                <div class="p-row">
                  <div class="p-label"><i class="fa-solid fa-chart-simple"></i> {{ tr('Priority', 'Độ ưu tiên') }}</div>
                  <div class="p-val">
                    <el-dropdown  trigger="click" @command="(cmd) => selectPriority(cmd)">
                      <div class="property-trigger" :class="{ 'muted-val': !selectedTask?.priority }"><i :class="getPrioIcon(selectedTask?.priority)"></i><span>{{ tr('Priority', 'Độ ưu tiên') }}</span><span class="property-value" :style="{ color: getPriorityColor(selectedTask?.priority), padding: '2px 8px', borderRadius: '4px', background: `color-mix(in srgb, ${getPriorityColor(selectedTask?.priority)} 15%, transparent)` }">{{ getPrioLabel(selectedTask?.priority) }}</span></div>
                      <template #dropdown>
                        <el-dropdown-menu class="theme-dropdown">
                          <el-dropdown-item :command="1"><i class="fa-solid fa-angles-up mr-2" style="color: var(--color-danger)"></i> {{ tr('Urgent', 'Khẩn cấp') }}</el-dropdown-item>
                          <el-dropdown-item :command="2"><i class="fa-solid fa-chevron-up mr-2" style="color: var(--color-warning)"></i> {{ tr('High', 'Cao') }}</el-dropdown-item>
                          <el-dropdown-item :command="3"><i class="fa-solid fa-minus mr-2" style="color: var(--color-accent)"></i> {{ tr('Medium', 'Trung bình') }}</el-dropdown-item>
                          <el-dropdown-item :command="4"><i class="fa-solid fa-arrow-down mr-2" style="color: var(--color-text-muted)"></i> {{ tr('Low', 'Thấp') }}</el-dropdown-item>
                          <el-dropdown-item :command="0"><i class="fa-solid fa-ban mr-2 text-muted"></i> {{ tr('None', 'Không có') }}</el-dropdown-item>
                        </el-dropdown-menu>
                      </template>
                    </el-dropdown>
                 </div>
               </div>
               <div class="p-row">
                 <div class="p-label"><i class="fa-solid fa-signal"></i> {{ tr('Story Points', 'Điểm công việc') }}</div>
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
                 <div class="p-label"><i class="fa-regular fa-circle-user"></i> {{ tr('Creator', 'Người tạo') }}</div>
                 <div class="p-val flex items-center gap-2">
                    <UserAvatar :user="getCreatorUserObject(selectedTask)" :size="20" :fontSize="10" />
                    <span class="text-[13px] font-medium">{{ getCreatorUserObject(selectedTask).fullName }}</span>
                 </div>
               </div>
               <div class="p-row">
                 <div class="p-label"><i class="fa-regular fa-calendar"></i> {{ tr('Start Date', 'Ngày bắt đầu') }}</div>
                 <div class="p-val">
                   <div class="date-trigger-wrap" @click.stop>
                     <button class="property-trigger" :class="{ 'muted-val': !selectedTask?.plannedStartDate, active: activeDatePicker === 'detail_start' }" @click.stop="openPicker('detail_start')">
                       <i class="fa-regular fa-calendar"></i>
                       <span>{{ tr('Start Date', 'Ngày bắt đầu') }}</span>
                       <span class="property-value">{{ selectedTask?.plannedStartDate || tr('Add start date', 'Thêm ngày bắt đầu') }}</span>
                     </button>
                     <div v-if="activeDatePicker === 'detail_start'" class="task-calendar-popover" @click.stop>
                       <div class="calendar-head">
                         <button type="button" class="calendar-nav" @click="shiftCalendarMonth(-1)"><i class="fa-solid fa-chevron-left"></i></button>
                         <strong>{{ getCalendarMonthLabel() }}</strong>
                         <button type="button" class="calendar-nav" @click="shiftCalendarMonth(1)"><i class="fa-solid fa-chevron-right"></i></button>
                       </div>
                       <div class="calendar-weekdays">
                         <span v-for="day in calendarWeekdays" :key="day">{{ day }}</span>
                       </div>
                       <div class="calendar-grid">
                         <button
                           v-for="day in getCalendarCells('detail_start')"
                           :key="day.key"
                           type="button"
                           class="calendar-day"
                           :class="{ muted: !day.currentMonth, selected: day.value === formatDateOnly(selectedTask?.plannedStartDate), today: day.value === getTodayDateString() }"
                           :disabled="day.disabled"
                           @click="selectCalendarDate('detail_start', day.value)"
                         >
                           {{ day.date.getDate() }}
                         </button>
                       </div>
                       <div class="calendar-actions">
                         <button type="button" @click="selectCalendarDate('detail_start', null)">{{ tr('Clear', 'Xóa') }}</button>
                         <button type="button" @click="selectCalendarDate('detail_start', getTodayDateString())">{{ tr('Today', 'Hôm nay') }}</button>
                       </div>
                     </div>
                   </div>
                 </div>
               </div>
               <div class="p-row">
                 <div class="p-label"><i class="fa-regular fa-calendar-check"></i> {{ tr('Due Date', 'Ngày đến hạn') }}</div>
                 <div class="p-val">
                   <div class="date-trigger-wrap" @click.stop>
                     <button class="property-trigger" :class="{ 'muted-val': !selectedTask?.dueDate, active: activeDatePicker === 'detail_due' }" @click.stop="openPicker('detail_due')">
                       <i class="fa-regular fa-calendar-check"></i>
                       <span>{{ tr('Due Date', 'Ngày đến hạn') }}</span>
                       <span class="property-value">{{ selectedTask?.dueDate || tr('Add due date', 'Thêm ngày đến hạn') }}</span>
                     </button>
                     <div v-if="activeDatePicker === 'detail_due'" class="task-calendar-popover" @click.stop>
                       <div class="calendar-head">
                         <button type="button" class="calendar-nav" @click="shiftCalendarMonth(-1)"><i class="fa-solid fa-chevron-left"></i></button>
                         <strong>{{ getCalendarMonthLabel() }}</strong>
                         <button type="button" class="calendar-nav" @click="shiftCalendarMonth(1)"><i class="fa-solid fa-chevron-right"></i></button>
                       </div>
                       <div class="calendar-weekdays">
                         <span v-for="day in calendarWeekdays" :key="day">{{ day }}</span>
                       </div>
                       <div class="calendar-grid">
                         <button
                           v-for="day in getCalendarCells('detail_due')"
                           :key="day.key"
                           type="button"
                           class="calendar-day"
                           :class="{ muted: !day.currentMonth, selected: day.value === formatDateOnly(selectedTask?.dueDate), today: day.value === getTodayDateString() }"
                           :disabled="day.disabled"
                           @click="selectCalendarDate('detail_due', day.value)"
                         >
                           {{ day.date.getDate() }}
                         </button>
                       </div>
                       <div class="calendar-actions">
                         <button type="button" @click="selectCalendarDate('detail_due', null)">{{ tr('Clear', 'Xóa') }}</button>
                         <button type="button" @click="selectCalendarDate('detail_due', getTodayDateString())">{{ tr('Today', 'Hôm nay') }}</button>
                       </div>
                     </div>
                   </div>
                 </div>
               </div>
               <div v-if="showEstimateFeatures" class="p-row">
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
                     <span>{{ isAiSuggestingEstimate ? 'AI đang suy nghĩ...' : 'AI gợi ý' }}</span>
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
                       <button class="secondary-mini-btn" type="button" @click="applyAiEstimateSuggestion()">Áp dụng gợi ý AI</button>
                     </div>
                   </div>
                 </div>
               </div>
               <div class="p-row">
                  <div class="p-label"><i class="fa-solid fa-cube"></i> Module</div>
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
                       <input v-model="moduleSearch" type="text" class="popover-search" placeholder="Tìm module..." />
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
                  <div class="p-label"><i class="fa-solid fa-circle-half-stroke"></i> Chu kỳ</div>
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
                        <input v-model="cycleSearch" type="text" class="popover-search" placeholder="Tìm chu kỳ..." />
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
                  <div class="p-label"><i class="fa-solid fa-arrow-turn-up fa-rotate-90"></i> Công việc cha</div>
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
                        <input v-model="parentSearch" type="text" class="popover-search" placeholder="Tìm công việc cha..." />
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
                  <div class="p-label"><i class="fa-solid fa-tags"></i> Nhãn</div>
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
                         <input v-model="labelSearch" type="text" class="popover-search" placeholder="Tìm nhãn..." />
                         <div class="popover-list">
                           <div class="popover-item" v-for="label in filteredLabels" :key="label.id" @click="toggleLabelDetail(label.id)">
                             <span class="w-3 h-3 rounded-full mr-2" :style="{ backgroundColor: label.colorCode || label.color || '#3b82f6' }"></span>
                             <span>{{ label.name }}</span>
                             <i v-if="(selectedTask?.labelIds || []).includes(label.id)" class="fa-solid fa-check ms-auto"></i>
                           </div>
                           <div class="popover-item" v-if="filteredLabels.length === 0 && labelSearch" @click="createLabelDetail(labelSearch)">
                             <i class="fa-solid fa-plus mr-2"></i>
                             <span>Thêm "{{ labelSearch }}"</span>
                           </div>
                         </div>
                       </div>
                     </el-popover>
                  </div>
               </div>
            </div>

            <div class="flex-between mb-6" style="margin-top: 56px;">
               <h3 class="sp-section-title mb-0">Hoạt động</h3>
               <div class="flex-center gap-2">
                  <button class="icon-filter-btn" @click="toggleActivitySort" :title="activitySortNewestFirst ? 'Mới nhất trước' : 'Cũ nhất trước'"><i class="fa-solid fa-arrow-down-short-wide"></i></button>
                  <button class="icon-filter-btn" @click="showActivityFilterInfo"><i class="fa-solid fa-bars-staggered"></i></button>
               </div>
            </div>

            <div v-if="activityEntries.length" class="activity-feed">
               <div v-for="entry in activityEntries" :key="entry.id" class="feed-item group">
                 <template v-if="entry.type === 'created'">
                   <div class="feed-icon"><i class="fa-solid fa-clone"></i></div>
                   <div class="feed-text"><b>{{ entry.user }}</b> đã tạo công việc. <span class="muted-val">{{ formatRelativeTime(entry.timestamp) }}</span></div>
                 </template>
                 <template v-else-if="entry.type === 'audit'">
                   <div class="feed-icon"><i class="fa-solid fa-clock-rotate-left"></i></div>
                   <div class="feed-content w-full">
                     <div class="feed-text">
                       <b>{{ entry.user || 'Hệ thống' }}</b> {{ entry.summary }}
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
                          <span class="text-[var(--color-text-muted)] text-xs ml-2">đã bình luận {{ formatRelativeTime(entry.comment.createdAt) }} <span v-if="entry.comment.isEdited" class="italic">(đã sửa)</span></span>
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
                                  <input type="text" v-model="emojiSearch" class="w-full bg-[var(--color-surface)] border border-[var(--border-color)] rounded text-[var(--color-text-primary)] py-1.5 pl-8 pr-2 text-xs focus:outline-none focus:border-blue-500" placeholder="Tìm..." />
                                </div>
                                <div class="p-2 text-xs font-semibold text-gray-400">Biểu cảm</div>
                                <div class="grid grid-cols-8 gap-1 p-2 max-h-[160px] overflow-y-auto no-scrollbar">
                                  <div v-for="emoji in filteredEmojis" :key="emoji" @click="addReaction(entry.comment, emoji)" class="cursor-pointer text-lg text-center hover:bg-[var(--color-surface-hover)] rounded p-1">{{ emoji }}</div>
                                </div>
                             </div>
                          </el-popover>
                          
                          <el-dropdown  trigger="click" placement="bottom-end">
                             <i class="fa-solid fa-ellipsis text-[var(--color-text-muted)] hover:text-[var(--color-text-primary)] cursor-pointer px-1.5 py-1 rounded hover:bg-[var(--color-surface-hover)]"></i>
                             <template #dropdown>
                               <el-dropdown-menu class="dark-dropdown" style="width: 150px;">
                                 <el-dropdown-item @click="startEditingComment(entry.comment)"><i class="fa-solid fa-pen mr-2"></i> Sửa</el-dropdown-item>
                                 <el-dropdown-item @click="copyCommentLink(entry.comment.id)"><i class="fa-solid fa-link mr-2"></i> Sao chép link</el-dropdown-item>
                                 <el-dropdown-item @click="deleteComment(entry.comment.id)" style="color: #f87171 !important;"><i class="fa-regular fa-trash-can mr-2"></i> Xóa</el-dropdown-item>
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
                             <button class="px-3 py-1.5 text-xs rounded border border-[var(--color-border)] text-[var(--color-text-secondary)] hover:bg-[var(--color-surface-hover)] transition" @click="cancelEditingComment">Hủy</button>
                             <button class="px-3 py-1.5 text-xs rounded bg-blue-600 text-white hover:bg-blue-700 transition" @click="saveEditedComment(entry.comment.id, entry.comment)">Cập nhật</button>
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
            <div v-else class="activity-empty-state">Chưa có hoạt động.</div>

            <div class="comment-box">
              <p class="text-[13px] font-semibold mb-2 text-[var(--color-text-muted)]">Thêm bình luận</p>
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
                    data-placeholder="Nhập bình luận..."
                    @focus="activeEditor = 'comment'"
                    @keydown="handleEditorKeydown($event, 'comment')"
                    @mouseup="saveEditorSelection('comment')"
                    @keyup="saveEditorSelection('comment')"
                    @input="handleCommentEditorInput"
                    @blur="saveEditorSelection('comment')"
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
                     <button class="c-submit" :disabled="!commentHasContent || isSubmittingComment" @click="submitComment">
                       {{ isSubmittingComment ? 'Đang gửi...' : 'Gửi' }}
                     </button>
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
          <button class="lightbox-delete" @click="removePreviewAttachment"><i class="fa-regular fa-trash-can"></i> Xóa</button>
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
import { usePeopleStore } from '@/store/usePeopleStore';
import { useI18nStore } from '@/store/useI18nStore';
import UserAvatar from '@/components/common/UserAvatar.vue';
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
const i18nStore = useI18nStore();
const tr = (en, vi) => i18nStore.locale === 'vi' ? vi : en;
const showEstimateFeatures = false;

const showTaskModal = ref(true);
const isSubscribed = ref(false);
const activitySortNewestFirst = ref(true);
const localActivityEntries = ref([]);
const localActivityByTaskId = ref({});
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
    const allTasks = cachedProjectTasks.value || [];
    const totalTasks = allTasks.length || 1;
    
    const memberWorkload = {};
    allTasks.forEach(task => {
        const status = (task.statusName || '').toLowerCase().trim();
        const isCompleted = ['done', 'completed', 'finished', 'hoàn thành', 'success', 'hoàn tất'].includes(status);
        if (isCompleted) return; // Only count incomplete/active tasks for workload weighting, or count all tasks? The user just said "phần trăm việc so với dự án", usually it means active or all tasks. Let's count all tasks.
        
        const assignees = Array.isArray(task.assignees) ? task.assignees.map(a => a.userId || a.id) : 
                          (Array.isArray(task.assigneeIds) ? task.assigneeIds : 
                          (task.assignedUserId ? [task.assignedUserId] : 
                          (task.assigneeId ? [task.assigneeId] : [])));
                          
        assignees.forEach(uid => {
            if (uid) {
                memberWorkload[uid] = (memberWorkload[uid] || 0) + 1;
            }
        });
    });

    const members = projectMemberOptions.value.map(m => {
        const count = memberWorkload[m.userId] || 0;
        const percentage = Math.round((count / totalTasks) * 100);
        return {
            ...m,
            taskPercentage: percentage
        };
    });
    
    let filtered = members;
    if (assigneeSearch.value) {
        filtered = members.filter(m => (m.fullName || m.name || m.email || '').toLowerCase().includes(assigneeSearch.value.toLowerCase()));
    }
    
    return filtered.sort((a, b) => a.taskPercentage - b.taskPercentage);
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
    if (p===1) return tr('Urgent', 'Khẩn cấp');
    if (p===2) return tr('High', 'Cao');
    if (p===3) return tr('Medium', 'Trung bình');
    if (p===4) return tr('Low', 'Thấp');
    return tr('None', 'Không có');
};

const getPriorityColor = (p) => {
    if (p===1) return '#ef4444';
    if (p===2) return '#f59e0b';
    if (p===3) return '#3b82f6';
    if (p===4) return '#94a3b8';
    return '#cbd5e1';
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

const getStatusColor = (statusName) => {
    const status = normalizeStatusName(statusName);
    if (status === 'DONE') return '#22C55E';
    if (status === 'IN REVIEW') return '#F59E0B';
    if (status === 'IN PROGRESS') return '#38BDF8';
    if (status === 'TO DO') return '#A78BFA';
    if (status === 'CANCELLED') return '#F43F5E';
    return '#94A3B8';
};

const getStatusLabel = (statusName) => {
    const status = `${statusName || ''}`.toUpperCase().trim();
    if (!status) return tr('Status', 'Trạng thái');
    if (status.includes('CANCEL')) return tr('Cancelled', 'Đã hủy');
    if (status.includes('DONE') || status.includes('COMPLETE')) return tr('Done', 'Hoàn thành');
    if (status.includes('REVIEW')) return tr('In Review', 'Đang đánh giá');
    if (status.includes('PROGRESS') || status.includes('ACTIVE')) return tr('In Progress', 'Đang thực hiện');
    if (status.includes('TODO') || status.includes('TO DO')) return tr('To Do', 'Cần làm');
    if (status.includes('BACKLOG')) return tr('Backlog', 'Chờ xử lý');
    return statusName || tr('Status', 'Trạng thái');
};
const normalizeStatusName = (statusName) => {
    const upper = (statusName || '')
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '')
      .toUpperCase()
      .replace(/\s+/g, '');
    if (upper.includes('CANCEL')) return 'CANCELLED';
    if (upper.includes('HUY')) return 'CANCELLED';
    if (upper.includes('DONE') || upper.includes('COMPLETE') || upper.includes('HOANTHANH')) return 'DONE';
    if (upper.includes('REVIEW') || upper.includes('TEST') || upper.includes('DANHGIA')) return 'IN REVIEW';
    if (upper.includes('PROGRESS') || upper.includes('ACTIVE') || upper.includes('DANGTHUCHIEN')) return 'IN PROGRESS';
    if (upper.includes('TODO') || upper.includes('CANLAM')) return 'TO DO';
    return 'BACKLOG';
};

const getStatusProgressPercent = (statusName) => {
   const status = normalizeStatusName(statusName);
   if (status === 'DONE') return 100;
   if (status === 'IN REVIEW') return 75;
   if (status === 'IN PROGRESS') return 45;
   if (status === 'TO DO') return 15;
   return 0;
};

const getAssigneeLabel = (id) => {
   if (!id) return tr('Assignee', 'Người thực hiện');
   const user = projectMemberOptions.value.find(m => m.userId === id);
   return user ? (user.fullName || user.name || user.email || tr('Assignee', 'Người thực hiện')) : tr('Assignee', 'Người thực hiện');
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
   if (!members.length) return tr('Assignee', 'Người thực hiện');
   if (members.length === 1) return members[0].fullName || members[0].email || tr('Assignee', 'Người thực hiện');
   return tr(`${members.length} assignees`, `${members.length} người thực hiện`);
};

const getTaskProgressPercent = (task = props.selectedTask) => {
   const statusProgress = getStatusProgressPercent(task?.statusName);
   const rows = buildTaskAssigneeRows(task);
   if (!rows.length) {
      return statusProgress;
   }

   const total = rows.reduce((sum, assignee) => sum + (Number(assignee.progressPercent) || 0), 0);
   return Math.max(statusProgress, Math.round(total / rows.length));
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
   if (!id) return tr('Cycle', 'Chu kỳ');
   const c = projectCycles.value.find(c => c.id === id);
   return c ? c.name : tr('Cycle', 'Chu kỳ');
};

const getModuleLabel = (id) => {
   if (!id) return tr('Modules', 'Phân hệ');
   const m = projectModules.value.find(m => m.id === id);
   return m ? m.name : tr('Modules', 'Phân hệ');
};

const getParentLabel = (id) => {
   if (!id) return tr('Add parent work item', 'Thêm công việc cha');
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
const activeDatePicker = ref(null);
const calendarMonth = ref(new Date());
const calendarWeekdays = ['Su', 'Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa'];

const parseDateOnly = (value) => {
  const formatted = formatDateOnly(value);
  if (!formatted) return null;
  const [year, month, day] = formatted.split('-').map(Number);
  if (!year || !month || !day) return null;
  return new Date(year, month - 1, day);
};

const openPicker = (key) => {
  activeDatePicker.value = activeDatePicker.value === key ? null : key;
  const sourceValue = key === 'detail_due' ? props.selectedTask?.dueDate : props.selectedTask?.plannedStartDate;
  const minValue = key === 'detail_due' ? props.selectedTask?.plannedStartDate : getTodayDateString();
  calendarMonth.value = parseDateOnly(sourceValue) || parseDateOnly(minValue) || new Date();
};

const getCalendarMonthLabel = () => calendarMonth.value.toLocaleDateString('en-US', {
  month: 'long',
  year: 'numeric'
});

const shiftCalendarMonth = (amount) => {
  calendarMonth.value = new Date(calendarMonth.value.getFullYear(), calendarMonth.value.getMonth() + amount, 1);
};

const getCalendarMinDate = (key) => parseDateOnly(key === 'detail_due'
  ? props.selectedTask?.plannedStartDate
  : getTodayDateString());

const getCalendarCells = (key) => {
  const monthStart = new Date(calendarMonth.value.getFullYear(), calendarMonth.value.getMonth(), 1);
  const gridStart = new Date(monthStart);
  gridStart.setDate(monthStart.getDate() - monthStart.getDay());
  const minDate = getCalendarMinDate(key);

  return Array.from({ length: 42 }, (_, index) => {
    const date = new Date(gridStart);
    date.setDate(gridStart.getDate() + index);
    const value = formatDateOnly(date);
    return {
      key: `${key}-${value}`,
      date,
      value,
      currentMonth: date.getMonth() === calendarMonth.value.getMonth(),
      disabled: Boolean(minDate && date < minDate)
    };
  });
};

const selectCalendarDate = (key, value) => {
  const field = key === 'detail_due' ? 'dueDate' : 'plannedStartDate';
  handleTaskDateChange(field, value);
  activeDatePicker.value = null;
};

const closeFloatingPickers = () => {
  activeDatePicker.value = null;
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

const placeCaretAtEnd = (target) => {
  if (!target) return;
  const selection = window.getSelection();
  const range = document.createRange();
  range.selectNodeContents(target);
  range.collapse(false);
  selection?.removeAllRanges();
  selection?.addRange(range);
};

const ensureEditorSelection = (editorName = activeEditor.value) => {
  const target = editorName === 'description' ? descriptionEditor.value : commentEditor.value;
  if (!target) return;

  activeEditor.value = editorName;
  target.focus();

  const selection = window.getSelection();
  const currentRange = selection?.rangeCount ? selection.getRangeAt(0) : null;
  if (currentRange && target.contains(currentRange.commonAncestorContainer)) {
    savedSelection.value[editorName] = currentRange.cloneRange();
    return;
  }

  const savedRange = savedSelection.value[editorName];
  if (savedRange && target.contains(savedRange.commonAncestorContainer)) {
    selection?.removeAllRanges();
    selection?.addRange(savedRange);
    return;
  }

  placeCaretAtEnd(target);
  saveEditorSelection(editorName);
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
  ensureEditorSelection(editorName);
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
  ensureEditorSelection(editorName);
  const inlineCommandMap = {
    bold: 'strong',
    italic: 'em',
    underline: 'u',
    strikeThrough: 's'
  };

  if (inlineCommandMap[command]) {
    const selection = window.getSelection();
    const range = selection?.rangeCount ? selection.getRangeAt(0) : null;
    if (range?.collapsed) {
      document.execCommand(command, false, value);
      syncEditorModel(editorName);
      saveEditorSelection(editorName);
      return;
    }
    toggleInlineFormat(editorName, inlineCommandMap[command]);
    return;
  }

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
  activeEditor.value = editorName;
  ensureEditorSelection(editorName);
  const code = document.createElement('code');
  code.className = 'comment-inline-code';
  code.textContent = window.getSelection()?.toString() || 'code';
  insertNodeAtSelection(code);
  saveEditorSelection(editorName);
};

const wrapSelectionWithBlock = (tagName, editorName = activeEditor.value) => {
  activeEditor.value = editorName;
  ensureEditorSelection(editorName);
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
  ensureEditorSelection(editorName);
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
  document.addEventListener('click', closeFloatingPickers);
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
  document.removeEventListener('click', closeFloatingPickers);
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

const parseTimelineDate = (dateStr) => {
  if (!dateStr) return null;
  const raw = String(dateStr).trim();
  if (!raw || raw.startsWith('0001-01-01')) return null;

  const normalized = raw.replace(' ', 'T');
  const hasTimezone = /(?:Z|[+-]\d{2}:?\d{2})$/i.test(normalized);
  const localIso = normalized.match(/^(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2})(?::(\d{2})(?:\.(\d{1,7}))?)?/);

  if (!hasTimezone && localIso) {
    const [, year, month, day, hour, minute, second = '0', fraction = '0'] = localIso;
    const ms = Number(fraction.padEnd(3, '0').slice(0, 3));
    const localDate = new Date(Number(year), Number(month) - 1, Number(day), Number(hour), Number(minute), Number(second), ms);
    return Number.isNaN(localDate.getTime()) ? null : localDate;
  }

  const parsed = new Date(normalized);
  return Number.isNaN(parsed.getTime()) ? null : parsed;
};

const formatRelativeTime = (dateStr) => {
  if (!dateStr) return 'vừa xong';
  const date = parseTimelineDate(dateStr);
  if (!date) return 'vừa xong';

  const diffMs = Date.now() - date.getTime();
  const diffMinutes = Math.max(0, Math.floor(diffMs / 60000));

  if (diffMinutes < 1) return 'vừa xong';
  if (diffMinutes < 60) return `${diffMinutes} phút trước`;

  const diffHours = Math.floor(diffMinutes / 60);
  if (diffHours < 24) return `${diffHours} giờ trước`;

  const diffDays = Math.floor(diffHours / 24);
  if (diffDays < 7) return `${diffDays} ngày trước`;

  const diffWeeks = Math.floor(diffDays / 7);
  if (diffWeeks < 5) return `${diffWeeks} tuần trước`;

  const diffMonths = Math.floor(diffDays / 30);
  if (diffMonths < 12) return `${diffMonths} tháng trước`;

  const diffYears = Math.floor(diffDays / 365);
  return `${diffYears} năm trước`;
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

const getCreatorUserObject = (task) => {
  if (!task) return { fullName: 'Creator' };
  
  const creatorId = task.createdBy || task.creatorId || task.createdById || task.reporterId;
  if (!creatorId) return { fullName: getCreatorName(task) };
  
  let user = projectMemberOptions.value.find(m => m.userId === creatorId || m.id === creatorId);
  if (!user) {
    const peopleStore = usePeopleStore();
    user = peopleStore.users?.find(m => m.id === creatorId);
  }
  
  if (user) {
    return {
      id: user.userId || user.id,
      fullName: user.fullName || user.name,
      email: user.email,
      avatarColor: user.avatarColor,
      initials: user.initials
    };
  }
  
  return { fullName: getCreatorName(task), email: getCreatorName(task) };
};

const getActivityActorName = () => {
  const storedUser = getStoredUser();
  return props.currentUser?.fullName
    || props.currentUser?.name
    || storedUser?.fullName
    || storedUser?.name
    || props.selectedTask?.updatedByName
    || 'Bạn';
};

const activityFieldLabels = {
  title: 'tiêu đề',
  description: 'mô tả',
  statusName: 'trạng thái',
  priority: 'độ ưu tiên',
  storyPoints: 'story points',
  plannedStartDate: 'ngày bắt đầu',
  dueDate: 'ngày đến hạn',
  assigneeIds: 'người được giao',
  assigneeProgress: 'tiến độ',
  labelIds: 'nhãn',
  moduleId: 'module',
  sprintId: 'cycle',
  parentId: 'công việc cha',
  parentTaskId: 'công việc cha'
};

const getActivityFieldLabel = (field) => activityFieldLabels[field] || field;

const recordLocalActivity = (summary) => {
  if (!summary) return;
  const timestamp = new Date().toISOString();
  const entry = {
    id: `local-${timestamp}-${Math.random().toString(36).slice(2)}`,
    type: 'audit',
    user: getActivityActorName(),
    summary,
    timestamp,
    isLocal: true
  };
  localActivityEntries.value.unshift(entry);

  const taskId = props.selectedTask?.id;
  if (taskId) {
    const current = localActivityByTaskId.value[taskId] || [];
    localActivityByTaskId.value = {
      ...localActivityByTaskId.value,
      [taskId]: [entry, ...current].slice(0, 12)
    };
  }

  if (props.selectedTask) {
    props.selectedTask.updatedAt = timestamp;
    props.selectedTask.updatedByName = getActivityActorName();
  }
};

const recordTaskFieldActivity = (field, value) => {
  if (!field) return;
  if (field === 'assigneeProgress') {
    recordLocalActivity('đã cập nhật tiến độ');
    return;
  }
  const label = getActivityFieldLabel(field);
  const valueText = Array.isArray(value) || value === null || value === undefined || value === ''
    ? ''
    : ` thành "${value}"`;
  recordLocalActivity(`đã cập nhật ${label}${valueText}`);
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
  recordTaskFieldActivity(field, value);
  emit('updateTask', task, field, value);
  window.setTimeout(fetchAuditTimeline, 700);
};

const updateTaskFields = (task, payload) => {
  const fields = Object.keys(payload || {});
  if (fields.length === 1) {
    recordTaskFieldActivity(fields[0], payload[fields[0]]);
  } else if (fields.length > 1) {
    recordLocalActivity(`đã cập nhật ${fields.map(getActivityFieldLabel).join(', ')}`);
  }
  emit('updateTask', task, payload);
  window.setTimeout(fetchAuditTimeline, 700);
};

const persistTaskPatch = async (task, payload) => {
  if (!task?.id || task?.isNew) return null;
  const response = await axiosClient.patch(`/projects/${props.projectId}/WorkTasks/${task.id}`, payload);
  const normalized = normalizeTaskSnapshot({ ...(response.data?.data || {}) });
  Object.assign(task, normalized);
  mergeCachedTask(normalized);
  const fields = Object.keys(payload || {});
  if (fields.length === 1) {
    recordTaskFieldActivity(fields[0], payload[fields[0]]);
  } else if (fields.length > 1) {
    recordLocalActivity(`đã cập nhật ${fields.map(getActivityFieldLabel).join(', ')}`);
  }
  emit('refresh-tasks');
  await fetchAuditTimeline();
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
      ElMessage.warning('Hạn hoàn thành không được trước ngày bắt đầu.');
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
const isSubmittingComment = ref(false);
const stripHtml = (value) => (value || '').replace(/<[^>]+>/g, ' ');
const commentHasContent = computed(() => stripHtml(newComment.value).replace(/\s+/g, '').length > 0 || pendingAttachments.value.length > 0);

async function fetchComments() {
  if (!props.selectedTask || !props.selectedTask.id) return;
  try {
    const res = await axiosClient.get(`/projects/${props.projectId}/WorkTasks/${props.selectedTask.id}/comments`);
    comments.value = res.data?.data || [];
  } catch (err) {
    comments.value = [];
    ElMessage.error(err.response?.data?.message || 'Không tải được bình luận từ server');
  }
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
  const createdEntry = props.selectedTask?.createdAt ? [{
    id: `created-${props.selectedTask?.id || 'task'}`,
    type: 'created',
    timestamp: props.selectedTask.createdAt,
    user: getCreatorName(props.selectedTask)
  }] : [];

  const commentEntries = topLevelComments.value.map(comment => ({
    id: `comment-${comment.id}`,
    type: 'comment',
    timestamp: comment.createdAt || comment.updatedAt,
    comment
  }));

  const auditTimelineEntries = (auditEntries.value || []).map(entry => ({
    ...entry,
    type: 'audit',
    timestamp: entry.timestamp || entry.createdAt || entry.occurredAt || entry.createdOn || entry.date
  }));

  const items = [...localActivityEntries.value, ...auditTimelineEntries, ...commentEntries, ...createdEntry]
    .filter(entry => entry.timestamp);
  const sorted = items.sort((left, right) => {
    const rightTime = parseTimelineDate(right.timestamp)?.getTime() || 0;
    const leftTime = parseTimelineDate(left.timestamp)?.getTime() || 0;
    return rightTime - leftTime;
  });
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
    if (!commentHasContent.value || !props.selectedTask?.id || isSubmittingComment.value) return;
    isSubmittingComment.value = true;
    try {
        syncEditorModel('comment');
        const sanitizedContent = sanitizeRichText(newComment.value || '');
        const formData = new FormData();
        formData.append('content', sanitizedContent);
        if (replyingToCommentId.value) {
            formData.append('parentCommentId', replyingToCommentId.value);
        }
        pendingAttachments.value.forEach(file => {
            formData.append('files', file);
        });

        const response = await axiosClient.post(`/projects/${props.projectId}/WorkTasks/${props.selectedTask.id}/comments`, formData, {
            headers: { 'Content-Type': 'multipart/form-data' }
        });
        const createdComment = response.data?.data;
        if (createdComment?.id && !comments.value.some(item => item.id === createdComment.id)) {
            comments.value = [...comments.value, createdComment];
        }
        
        newComment.value = '';
        if (commentEditor.value) commentEditor.value.innerHTML = '';
        pendingAttachments.value = [];
        replyingToCommentId.value = null;
        if (commentFileInput.value) commentFileInput.value.value = '';
        if (commentImageInput.value) commentImageInput.value.value = '';
        await fetchComments();
        fetchAuditTimeline();
        ElMessage.success('Đã thêm bình luận');
        isSubmittingComment.value = false;
    } catch(e) {
        isSubmittingComment.value = false;
        ElMessage.error(e.response?.data?.message || "Lỗi khi gửi bình luận");
    }
};

watch(() => props.selectedTask, (newTask) => {
  if (newTask) {
    normalizeTaskSnapshot(newTask);
    localActivityEntries.value = localActivityByTaskId.value[newTask.id] || [];
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
  z-index: var(--z-modal);
  background: var(--color-modal-overlay);
  display: flex;
  align-items: center;
  justify-content: center;
  backdrop-filter: blur(3px);
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
  background: var(--color-modal-overlay);
}

[data-theme='light'] .task-modal-overlay {
  background: var(--color-modal-overlay);
}

/* CREATE MODAL */
.create-centered-modal {
  width: min(680px, calc(100vw - 32px));
  max-height: calc(100vh - 48px);
  overflow: visible;
  background: var(--color-surface-elevated);
  border: 1px solid var(--color-border);
  border-radius: 14px;
  padding: 0;
  box-shadow: var(--shadow-popover);
}

.cm-title {
  font-size: 20px;
  font-weight: 800;
  margin: 0;
  padding: 22px 28px 8px;
  color: var(--color-text-primary);
}

.cm-badge-row {
  padding: 0 28px 14px;
}

.cm-badge {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  min-height: 30px;
  padding: 5px 10px;
  border: 1px solid var(--color-border);
  border-radius: 999px;
  color: var(--color-text-secondary);
  background: color-mix(in srgb, var(--color-surface-hover) 70%, transparent);
  font-size: 13px;
  font-weight: 600;
}

.cm-form-group {
  display: flex;
  flex-direction: column;
  gap: 12px;
  margin: 0;
  padding: 18px 28px;
  border-top: 1px solid var(--color-border);
}

.cm-inputbox, .cm-textareabox {
  width: 100%;
  background: var(--color-input-bg);
  border: 1px solid var(--color-input-border);
  border-radius: 10px;
  padding: 12px 14px;
  color: var(--color-text-primary);
  outline: none;
  font-size: 14px;
  line-height: 1.5;
}

.cm-textareabox {
  min-height: 96px;
  resize: vertical;
}

.cm-inputbox:focus, .cm-textareabox:focus {
  border-color: var(--color-accent);
}

.cm-toolbar-row {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  margin: 0;
  padding: 0 28px 22px;
}

.t-btn {
  display: flex;
  align-items: center;
  gap: 8px;
  min-height: 34px;
  padding: 7px 12px;
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 999px;
  color: var(--color-text-secondary);
  font-size: 13px;
  cursor: pointer;
  max-width: 100%;
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
  padding: 16px 28px 20px;
  border-top: 1px solid var(--color-border);
  background: color-mix(in srgb, var(--color-surface) 88%, var(--color-bg));
  border-radius: 0 0 14px 14px;
}

.cm-t-more {
  margin-right: auto;
  display: inline-flex;
  align-items: center;
  gap: 8px;
  color: var(--color-text-secondary);
  font-size: 13px;
}

.btn-save {
  background: var(--color-accent);
  color: #fff;
  border: none;
  min-width: 82px;
  padding: 9px 18px;
  border-radius: 10px;
  font-weight: 600;
  cursor: pointer;
}

.btn-discard {
  background: var(--color-surface);
  color: var(--color-text-secondary);
  border: 1px solid var(--color-border);
  min-width: 74px;
  padding: 9px 18px;
  border-radius: 10px;
  cursor: pointer;
}

.btn-discard:hover {
  color: var(--color-text-primary);
  background: var(--color-surface-hover);
}

/* SIDE PANEL */
.task-side-panel {
  position: absolute;
  top: 0;
  right: 0;
  bottom: 0;
  width: min(620px, 92vw);
  background:
    linear-gradient(180deg, color-mix(in srgb, var(--color-accent) 10%, transparent), transparent 260px),
    var(--color-surface);
  border-left: 1px solid var(--color-border);
  display: flex;
  flex-direction: column;
  box-shadow: var(--shadow-drawer);
}

.sp-header {
  height: 50px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 18px;
  border-bottom: 1px solid var(--color-border);
  background:
    linear-gradient(90deg, color-mix(in srgb, var(--color-accent) 10%, transparent), transparent 58%),
    color-mix(in srgb, var(--color-surface-elevated) 92%, transparent);
}

.sph-right {
  display: flex;
  align-items: center;
  gap: 6px;
}

.sp-body {
  flex: 1;
  overflow-y: auto;
  padding: 22px 26px 32px;
}

.parent-context-banner {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 12px;
}
.parent-context-label {
  font-size: 13px;
  color: var(--color-text-muted);
}
.parent-context-link {
  font-size: 13px;
  font-weight: 600;
  color: var(--color-text-primary);
  background: transparent;
  border: 1px solid var(--color-border);
  border-radius: 4px;
  padding: 4px 8px;
  cursor: pointer;
  transition: all 0.2s;
}
.parent-context-link:hover {
  background: var(--color-surface-hover);
  border-color: var(--color-border-hover);
}

.sp-breadcrumb {
  font-size: 13px;
  font-weight: 700;
  color: var(--color-text-muted);
  margin-bottom: 12px;
}

.sp-title {
  font-size: 24px;
  font-weight: 850;
  margin: 0 0 14px;
  outline: none;
  color: var(--color-text-primary);
  line-height: 1.3;
}

.sp-toolbar {
  display: flex;
  align-items: center;
  gap: 6px;
  margin-top: 18px;
  margin-bottom: 20px;
  flex-wrap: wrap;
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
  min-height: 74px;
  font-size: 14px;
  line-height: 1.6;
  color: var(--color-text-secondary);
  outline: none;
  padding: 10px 12px;
  border: 1px solid transparent;
  border-radius: 8px;
  background: color-mix(in srgb, var(--color-surface-hover) 56%, transparent);
}

.rich-editor:focus {
  border-color: color-mix(in srgb, var(--color-accent) 58%, transparent);
  background: var(--color-surface);
}

.rich-editor[data-placeholder]:empty:before {
  content: attr(data-placeholder);
  color: var(--color-text-secondary);
}

.props-grid {
  display: grid;
  gap: 12px;
  margin-top: 18px;
  padding: 18px;
  border: 1px solid color-mix(in srgb, var(--color-border) 70%, transparent);
  border-radius: 14px;
  background:
    radial-gradient(circle at 12% 0%, color-mix(in srgb, #38bdf8 14%, transparent), transparent 34%),
    radial-gradient(circle at 90% 12%, color-mix(in srgb, #a78bfa 12%, transparent), transparent 30%),
    linear-gradient(180deg, rgba(255,255,255,0.045), transparent),
    color-mix(in srgb, var(--color-surface-elevated) 82%, transparent);
  box-shadow:
    inset 0 1px 0 rgba(255, 255, 255, 0.05),
    0 18px 48px color-mix(in srgb, var(--color-bg) 58%, transparent);
}

.p-row {
  display: grid;
  grid-template-columns: minmax(132px, 0.46fr) minmax(0, 1fr);
  align-items: center;
  min-height: 42px;
  gap: 16px;
  padding: 7px 8px;
  border-radius: 10px;
  transition: background 160ms ease;
}

.p-row:hover {
  background: color-mix(in srgb, var(--color-bg) 28%, transparent);
}

.p-label {
  font-size: 12px;
  color: var(--color-text-secondary);
  display: flex;
  align-items: center;
  gap: 8px;
  min-width: 0;
  font-weight: 700;
}

.property-trigger {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  min-height: 34px;
  padding: 7px 11px;
  background: color-mix(in srgb, var(--color-bg) 55%, transparent);
  border: 1px solid color-mix(in srgb, var(--color-border) 76%, transparent);
  border-radius: 9px;
  color: var(--color-text-primary);
  font-size: 12px;
  font-weight: 700;
  cursor: pointer;
  transition: transform 160ms cubic-bezier(0.2, 0.8, 0.2, 1), background 160ms ease, border-color 160ms ease;
  max-width: 100%;
}

.property-trigger:hover {
  transform: translateY(-1px);
  background: color-mix(in srgb, var(--color-bg) 72%, #38bdf8 12%);
  border-color: rgba(56, 189, 248, 0.36);
}

.property-trigger:active {
  transform: none;
}

.property-trigger.status-property-trigger {
  border-color: color-mix(in srgb, var(--status-color) 50%, var(--color-border));
  background:
    linear-gradient(135deg, color-mix(in srgb, var(--status-color) 22%, transparent), transparent),
    color-mix(in srgb, var(--color-bg) 64%, transparent);
  color: color-mix(in srgb, var(--status-color) 24%, var(--color-text-primary));
}

.property-trigger.status-property-trigger i {
  color: var(--status-color);
}

.property-value {
  color: var(--color-text-primary);
  font-weight: 800;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.status-property-trigger .property-value {
  color: var(--color-text-primary);
}

[data-theme='dark'] .property-trigger.status-property-trigger {
  color: color-mix(in srgb, var(--status-color) 26%, #f8fafc);
  background:
    linear-gradient(135deg, color-mix(in srgb, var(--status-color) 18%, transparent), transparent),
    color-mix(in srgb, var(--color-surface) 72%, transparent);
}

[data-theme='light'] .props-grid {
  background:
    radial-gradient(circle at 12% 0%, rgba(56, 189, 248, 0.12), transparent 34%),
    radial-gradient(circle at 90% 12%, rgba(167, 139, 250, 0.10), transparent 30%),
    rgba(255, 255, 255, 0.82);
  box-shadow: 0 18px 48px rgba(15, 23, 42, 0.08);
}

[data-theme='light'] .p-label {
  color: #475569;
}

[data-theme='light'] .property-trigger {
  background: rgba(255, 255, 255, 0.88);
  color: #0f172a;
}

.p-val {
  min-width: 0;
}

.status-popover-item {
  border: 1px solid transparent;
}

.status-popover-item i:first-child {
  color: var(--status-color);
}

.status-popover-item:hover {
  border-color: color-mix(in srgb, var(--status-color) 40%, transparent);
  background: color-mix(in srgb, var(--status-color) 13%, var(--color-surface-hover));
}

.popover-content {
  display: flex;
  flex-direction: column;
  gap: 8px;
  min-width: 0;
  color: var(--color-text-primary);
}

.popover-search {
  width: 100%;
  min-height: 36px;
  border: 1px solid var(--color-border);
  border-radius: 8px;
  background: var(--color-input-bg);
  color: var(--color-text-primary);
  padding: 7px 10px;
  font-size: 13px;
  outline: none;
}

.popover-search:focus {
  border-color: var(--color-accent);
  box-shadow: 0 0 0 3px color-mix(in srgb, var(--color-accent) 16%, transparent);
}

.popover-list {
  display: flex;
  flex-direction: column;
  gap: 4px;
  max-height: 260px;
  overflow-y: auto;
}

.popover-item {
  display: flex;
  align-items: center;
  gap: 8px;
  min-height: 34px;
  padding: 7px 9px;
  border-radius: 8px;
  color: var(--color-text-secondary);
  font-size: 13px;
  cursor: pointer;
}

.popover-item:hover {
  background: var(--color-surface-hover);
  color: var(--color-text-primary);
}

.popover-item .fa-check {
  color: var(--color-accent);
}

.quick-subtask-box {
  display: grid;
  gap: 10px;
  margin: 12px 0 18px;
  padding: 12px;
  border: 1px solid var(--color-border);
  border-radius: 10px;
  background: var(--color-surface-elevated);
}

.quick-subtask-input {
  width: 100%;
  min-height: 38px;
  border: 1px solid var(--color-border);
  border-radius: 8px;
  background: var(--color-input-bg);
  color: var(--color-text-primary);
  padding: 8px 10px;
  font-size: 13px;
  outline: none;
}

.quick-subtask-input:focus {
  border-color: var(--color-accent);
  box-shadow: 0 0 0 3px color-mix(in srgb, var(--color-accent) 16%, transparent);
}

.quick-subtask-actions {
  display: flex;
  justify-content: flex-end;
  gap: 8px;
}

.quick-subtask-cancel,
.quick-subtask-save {
  min-height: 34px;
  border-radius: 8px;
  padding: 0 12px;
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
}

.quick-subtask-cancel {
  border: 1px solid var(--color-border);
  background: var(--color-surface);
  color: var(--color-text-secondary);
}

.quick-subtask-cancel:hover {
  border-color: var(--color-border-hover);
  color: var(--color-text-primary);
}

.quick-subtask-save {
  border: 1px solid var(--color-accent);
  background: var(--color-accent);
  color: #ffffff;
}

.quick-subtask-save:disabled {
  opacity: 0.55;
  cursor: not-allowed;
}

.subtask-section {
  margin-top: 16px;
  border: 1px solid color-mix(in srgb, var(--color-border) 82%, transparent);
  border-radius: 12px;
  background:
    linear-gradient(135deg, color-mix(in srgb, var(--color-accent) 9%, transparent), transparent 46%),
    color-mix(in srgb, var(--color-surface) 88%, transparent);
  box-shadow: inset 0 1px 0 rgba(255, 255, 255, 0.04);
}

.subtask-section-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 12px 14px;
}

.subtask-section-head > div {
  display: grid;
  gap: 3px;
}

.subtask-section-head strong {
  color: var(--color-text-primary);
  font-size: 14px;
  font-weight: 700;
}

.subtask-kicker {
  color: var(--color-text-muted);
  font-size: 11px;
  font-weight: 700;
  letter-spacing: 0.04em;
  text-transform: uppercase;
}

.subtask-toggle-btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 7px;
  min-height: 32px;
  border: 1px solid color-mix(in srgb, var(--color-border) 78%, transparent);
  border-radius: 8px;
  background: color-mix(in srgb, var(--color-bg-secondary) 84%, transparent);
  color: var(--color-text-secondary);
  padding: 0 10px;
  font-size: 12px;
  font-weight: 700;
  cursor: pointer;
  transition: transform 180ms cubic-bezier(0.2, 0.8, 0.2, 1), border-color 180ms ease, color 180ms ease, background 180ms ease;
}

.subtask-toggle-btn:hover {
  border-color: color-mix(in srgb, var(--color-accent) 45%, var(--color-border));
  background: color-mix(in srgb, var(--color-accent) 13%, var(--color-bg-secondary));
  color: var(--color-text-primary);
}

.subtask-toggle-btn:active {
  transform: none;
}

.subtask-list {
  position: relative;
  display: grid;
  gap: 10px;
  margin: 10px 0 18px;
  padding-left: 14px;
}

.subtask-list::before {
  content: "";
  position: absolute;
  top: 10px;
  bottom: 10px;
  left: 4px;
  width: 2px;
  border-radius: 999px;
  background: linear-gradient(
    180deg,
    color-mix(in srgb, var(--color-accent) 55%, transparent),
    color-mix(in srgb, var(--color-border) 75%, transparent)
  );
}

.subtask-item {
  position: relative;
  display: grid;
  gap: 12px;
  padding: 12px;
  border: 1px solid color-mix(in srgb, var(--color-border) 76%, transparent);
  border-radius: 12px;
  background:
    linear-gradient(180deg, rgba(255, 255, 255, 0.035), transparent),
    color-mix(in srgb, var(--color-bg-secondary) 88%, var(--color-surface));
  box-shadow:
    inset 0 1px 0 rgba(255, 255, 255, 0.04),
    0 10px 26px color-mix(in srgb, var(--color-bg) 62%, transparent);
  transition: transform 180ms cubic-bezier(0.2, 0.8, 0.2, 1), border-color 180ms ease, background 180ms ease;
}

.subtask-item::before {
  content: "";
  position: absolute;
  top: 20px;
  left: -16px;
  width: 8px;
  height: 8px;
  border: 2px solid color-mix(in srgb, var(--color-accent) 68%, var(--color-border));
  border-radius: 999px;
  background: var(--color-bg);
}

.subtask-item:hover {
  transform: translateY(-1px);
  border-color: color-mix(in srgb, var(--color-accent) 42%, var(--color-border));
  background:
    linear-gradient(180deg, color-mix(in srgb, var(--color-accent) 6%, transparent), transparent),
    color-mix(in srgb, var(--color-bg-secondary) 92%, var(--color-surface));
}

.subtask-main {
  display: grid;
  gap: 10px;
}

.subtask-open {
  display: grid;
  grid-template-columns: auto 1fr;
  align-items: center;
  gap: 9px;
  width: 100%;
  border: 0;
  background: transparent;
  color: inherit;
  padding: 0;
  text-align: left;
  cursor: pointer;
}

.subtask-open:focus-visible,
.subtask-chip:focus-visible,
.subtask-toggle-btn:focus-visible {
  outline: 2px solid color-mix(in srgb, var(--color-accent) 70%, white);
  outline-offset: 2px;
}

.subtask-seq {
  min-width: 56px;
  border-radius: 6px;
  background: color-mix(in srgb, var(--color-accent) 12%, var(--color-bg));
  color: color-mix(in srgb, var(--color-accent) 82%, #ffffff);
  padding: 4px 7px;
  font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, "Liberation Mono", monospace;
  font-size: 11px;
  font-variant-numeric: tabular-nums;
  font-weight: 800;
  line-height: 1;
}

.subtask-title {
  min-width: 0;
  color: var(--color-text-primary);
  font-size: 14px;
  font-weight: 700;
  line-height: 1.35;
  overflow-wrap: anywhere;
}

.subtask-controls {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}

.subtask-chip {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
  min-height: 28px;
  max-width: 180px;
  border: 1px solid color-mix(in srgb, var(--color-border) 74%, transparent);
  border-radius: 7px;
  background: color-mix(in srgb, var(--color-bg) 72%, transparent);
  color: var(--color-text-secondary);
  padding: 0 9px;
  font-size: 12px;
  font-weight: 700;
  cursor: pointer;
  transition: border-color 160ms ease, color 160ms ease, background 160ms ease, transform 160ms ease;
}

.subtask-chip span {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.subtask-chip:hover {
  border-color: color-mix(in srgb, var(--color-accent) 42%, var(--color-border));
  background: color-mix(in srgb, var(--color-accent) 10%, var(--color-bg));
  color: var(--color-text-primary);
}

.subtask-chip:active {
  transform: none;
}

.subtask-progress-wrap {
  display: grid;
  grid-template-columns: minmax(120px, 1fr) auto;
  align-items: center;
  gap: 8px 12px;
  border-top: 1px solid color-mix(in srgb, var(--color-border) 62%, transparent);
  padding-top: 10px;
}

.subtask-progress-top {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
  grid-column: 1 / -1;
  color: var(--color-text-muted);
  font-size: 11px;
  font-weight: 800;
  letter-spacing: 0.04em;
  text-transform: uppercase;
}

.subtask-progress-top strong {
  color: var(--color-text-primary);
  font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, "Liberation Mono", monospace;
  font-size: 12px;
  font-variant-numeric: tabular-nums;
}

.subtask-progress-track {
  height: 7px;
  overflow: hidden;
  border-radius: 999px;
  background: color-mix(in srgb, var(--color-bg) 78%, var(--color-border));
}

.subtask-progress-track span {
  display: block;
  height: 100%;
  border-radius: inherit;
  background: linear-gradient(90deg, var(--color-accent), color-mix(in srgb, var(--color-accent) 58%, #ffffff));
  transition: width 220ms cubic-bezier(0.2, 0.8, 0.2, 1);
}

.subtask-progress {
  display: inline-flex;
  align-items: center;
  justify-self: end;
  gap: 4px;
  border: 1px solid color-mix(in srgb, var(--color-border) 72%, transparent);
  border-radius: 7px;
  background: color-mix(in srgb, var(--color-bg) 76%, transparent);
  color: var(--color-text-secondary);
  padding: 3px 7px;
  font-size: 12px;
  font-weight: 700;
}

.subtask-progress input {
  width: 54px;
  border: 0;
  background: transparent;
  color: var(--color-text-primary);
  font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, "Liberation Mono", monospace;
  font-size: 12px;
  font-variant-numeric: tabular-nums;
  font-weight: 800;
  outline: none;
  text-align: right;
}

.task-progress-editor {
  display: grid;
  grid-template-columns: minmax(140px, 1fr) auto auto;
  align-items: center;
  gap: 8px;
  width: 100%;
  max-width: 520px;
  padding: 10px;
  border: 1px solid color-mix(in srgb, var(--progress-color) 32%, var(--color-border));
  border-radius: 12px;
  background:
    linear-gradient(135deg, color-mix(in srgb, var(--progress-color) 16%, transparent), transparent 52%),
    color-mix(in srgb, var(--color-bg) 60%, transparent);
}

.task-progress-readout {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
  grid-column: 1 / -1;
  color: var(--color-text-secondary);
  font-size: 12px;
  font-weight: 800;
}

.task-progress-readout strong {
  color: var(--progress-color);
  font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, "Liberation Mono", monospace;
  font-size: 15px;
  font-variant-numeric: tabular-nums;
}

.task-progress-bar {
  height: 8px;
  overflow: hidden;
  border-radius: 999px;
  background: color-mix(in srgb, var(--color-bg) 78%, var(--color-border));
}

.task-progress-bar span {
  display: block;
  width: var(--progress-width);
  height: 100%;
  border-radius: inherit;
  background: linear-gradient(90deg, var(--progress-color), color-mix(in srgb, var(--progress-color) 58%, #ffffff));
  transition: width 240ms cubic-bezier(0.2, 0.8, 0.2, 1);
}

.task-progress-input {
  width: 64px;
  min-height: 32px;
  border: 1px solid color-mix(in srgb, var(--progress-color) 36%, var(--color-border));
  border-radius: 8px;
  background: color-mix(in srgb, var(--color-bg) 84%, transparent);
  color: var(--color-text-primary);
  font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, "Liberation Mono", monospace;
  font-size: 13px;
  font-weight: 800;
  text-align: right;
  padding: 0 8px;
}

.task-progress-input:disabled {
  opacity: 1;
  color: var(--progress-color);
  cursor: default;
}

.task-progress-suffix {
  color: var(--color-text-secondary);
  font-size: 12px;
  font-weight: 800;
}

.task-progress-hint {
  grid-column: 1 / -1;
  color: var(--color-text-secondary);
  font-size: 12px;
  line-height: 1.45;
}

.date-trigger-wrap {
  position: relative;
  display: inline-flex;
}

.property-trigger.active {
  border-color: color-mix(in srgb, var(--color-accent) 42%, var(--color-border));
  background: color-mix(in srgb, var(--color-accent) 13%, var(--color-surface-hover));
  color: var(--color-text-primary);
}

.task-calendar-popover {
  position: absolute;
  top: calc(100% + 10px);
  right: 0;
  z-index: var(--z-popover);
  width: 304px;
  padding: 14px;
  border: 1px solid color-mix(in srgb, var(--color-accent) 24%, var(--color-border));
  border-radius: 16px;
  background:
    linear-gradient(180deg, color-mix(in srgb, var(--color-surface) 96%, var(--color-accent) 4%), var(--color-surface));
  color: var(--color-text-primary);
  box-shadow: 0 24px 64px rgba(2, 8, 23, 0.34), inset 0 1px 0 rgba(255, 255, 255, 0.05);
}

.calendar-head,
.calendar-actions {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
}

.calendar-head strong {
  font-size: 14px;
  font-weight: 900;
}

.calendar-nav,
.calendar-actions button {
  border: 1px solid var(--color-border);
  border-radius: 10px;
  background: var(--color-surface-hover);
  color: var(--color-text-primary);
  cursor: pointer;
  font-weight: 800;
}

.calendar-nav {
  width: 32px;
  height: 32px;
}

.calendar-weekdays,
.calendar-grid {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
  gap: 6px;
}

.calendar-weekdays {
  margin-top: 14px;
  color: var(--color-text-muted);
  font-size: 11px;
  font-weight: 900;
  text-align: center;
  text-transform: uppercase;
}

.calendar-grid {
  margin-top: 8px;
}

.calendar-day {
  width: 34px;
  height: 34px;
  border: 1px solid transparent;
  border-radius: 10px;
  background: transparent;
  color: var(--color-text-primary);
  cursor: pointer;
  font-size: 13px;
  font-weight: 800;
}

.calendar-day:hover:not(:disabled) {
  background: color-mix(in srgb, var(--color-accent) 13%, var(--color-surface-hover));
  border-color: color-mix(in srgb, var(--color-accent) 26%, transparent);
}

.calendar-day.muted {
  color: var(--color-text-muted);
  opacity: 0.55;
}

.calendar-day.today {
  border-color: color-mix(in srgb, var(--color-accent) 42%, transparent);
}

.calendar-day.selected {
  background: linear-gradient(135deg, #38bdf8, #2563eb);
  color: #ffffff;
  box-shadow: 0 10px 20px rgba(37, 99, 235, 0.26);
}

.calendar-day:disabled {
  cursor: not-allowed;
  opacity: 0.28;
}

.calendar-actions {
  margin-top: 12px;
}

.calendar-actions button {
  padding: 7px 10px;
  font-size: 12px;
}

:global(.el-picker__popper),
:global(.el-select__popper) {
  border-color: color-mix(in srgb, var(--color-accent) 22%, var(--color-border)) !important;
  border-radius: 14px !important;
  overflow: hidden;
  background: var(--color-surface) !important;
  box-shadow: 0 22px 56px rgba(2, 8, 23, 0.28) !important;
}

:global(.el-date-picker),
:global(.el-picker-panel),
:global(.el-select-dropdown) {
  background: var(--color-surface) !important;
  color: var(--color-text-primary) !important;
}

:global(.el-date-table td.current:not(.disabled) .el-date-table-cell__text) {
  background: linear-gradient(135deg, #38bdf8, #2563eb) !important;
}

.subtask-progress input:disabled {
  opacity: 0.68;
  cursor: not-allowed;
}

@media (min-width: 720px) {
  .subtask-main {
    grid-template-columns: minmax(0, 1fr) auto;
    align-items: start;
  }

  .subtask-controls {
    justify-content: flex-end;
  }
}

@media (max-width: 560px) {
  .subtask-section-head,
  .subtask-progress-wrap {
    grid-template-columns: 1fr;
  }

  .subtask-section-head {
    align-items: stretch;
  }

  .subtask-toggle-btn,
  .subtask-progress {
    justify-self: stretch;
  }
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
  border: 1px solid var(--color-border);
  color: #A1A1AA;
  padding: 6px 9px;
  border-radius: 6px;
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
  gap: 10px;
  margin-top: 14px;
  padding: 14px;
  border: 1px solid rgba(148, 163, 184, 0.18);
  border-radius: 10px;
  background:
    linear-gradient(180deg, rgba(56, 189, 248, 0.055), transparent),
    color-mix(in srgb, var(--color-surface) 74%, transparent);
}

.feed-item {
  position: relative;
  display: flex;
  gap: 10px;
  font-size: 13px;
  padding: 9px;
  border-radius: 8px;
}

.feed-item:hover {
  background: color-mix(in srgb, var(--color-bg-secondary) 80%, #38bdf8 6%);
}

.feed-icon {
  width: 26px;
  height: 26px;
  min-width: 26px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 999px;
  color: var(--color-accent);
  background: color-mix(in srgb, var(--color-accent) 12%, transparent);
  border: 1px solid color-mix(in srgb, var(--color-accent) 28%, transparent);
}

.feed-avatar {
  width: 26px;
  height: 26px;
  min-width: 26px;
  border-radius: 999px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  background: color-mix(in srgb, var(--color-accent) 18%, transparent);
  color: var(--color-text-primary);
  border: 1px solid color-mix(in srgb, var(--color-accent) 35%, transparent);
  font-size: 11px;
  font-weight: 700;
}

.feed-text {
  color: var(--color-text-secondary);
  line-height: 1.55;
}

.feed-text b {
  color: var(--color-text-primary);
}

.muted-val {
  color: var(--color-text-secondary);
}

.editor-wrap {
  border: 1px solid var(--color-border);
  border-radius: 10px;
  background: var(--color-surface);
  overflow: hidden;
}

.comment-box {
  margin-top: 22px;
  padding: 16px;
  border: 1px solid rgba(148, 163, 184, 0.18);
  border-radius: 10px;
  background:
    radial-gradient(circle at top right, rgba(14, 165, 233, 0.10), transparent 36%),
    color-mix(in srgb, var(--color-surface) 78%, transparent);
}

.comment-editor {
  min-height: 76px;
  padding: 12px 14px !important;
  background: var(--color-input-bg);
  color: var(--color-text-primary);
  outline: none;
}

.comment-editor:focus {
  box-shadow: inset 0 0 0 1px var(--color-accent);
}

.c-toolbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 8px 12px;
  background: color-mix(in srgb, var(--color-surface) 86%, var(--color-bg) 14%);
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
  border-radius: 7px;
  color: var(--color-text-secondary);
  transition: all 0.2s;
}

.icon-hover:hover {
  background: var(--color-surface-hover);
  color: var(--color-text-primary);
}

.icon-hover.is-active {
  background: var(--color-accent);
  color: #ffffff;
}

.c-submit {
  height: 32px;
  padding: 0 16px;
  background: linear-gradient(135deg, #0ea5e9, #2563eb);
  color: #ffffff;
  border: none;
  border-radius: 7px;
  font-size: 12px;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.2s ease;
  box-shadow: 0 8px 22px rgba(37, 99, 235, 0.26);
}

.c-submit:hover:not(:disabled) {
  transform: translateY(-1px);
  box-shadow: 0 12px 28px rgba(37, 99, 235, 0.34);
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
  z-index: var(--z-popover) !important;
  background: var(--color-surface-elevated) !important;
  border: 1px solid var(--color-border) !important;
  border-radius: 10px !important;
  box-shadow: var(--shadow-popover) !important;
  color: var(--color-text-primary) !important;
  padding: 8px !important;
}

.t-btn-date:deep(.el-input__wrapper) {
  min-height: 34px !important;
  background-color: var(--color-surface) !important;
  border: 1px solid var(--color-border) !important;
  border-radius: 999px !important;
  box-shadow: none !important;
  padding: 0 10px !important;
}

.t-btn-date:deep(.el-input__wrapper:hover) {
  background-color: var(--color-surface-hover) !important;
  border-color: var(--color-border-hover) !important;
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
