<template>
  <el-dialog
    v-model="visibleComp"
    title="Create Task"
    class="jira-create-dialog responsive-dialog"
    :before-close="handleClose"
    append-to-body
    :modal-append-to-body="false"
  >
    <div class="modal-body-jira custom-scrollbar">
      <p class="required-notice">Required fields are marked with an asterisk <span class="required">*</span></p>

      <!-- Space -->
      <div class="form-group-jira">
        <label>Space <span class="required">*</span></label>
        <el-select v-model="form.space" class="jira-select" placeholder="Choose a space">
          <el-option label="My Team (SCRUM)" value="my-team">
            <template #default>
              <div class="option-with-icon">
                <i class="fa-solid fa-square-poll-vertical space-icon"></i>
                <span>My Team (SCRUM)</span>
              </div>
            </template>
          </el-option>
        </el-select>
      </div>

      <!-- Work Type -->
      <div class="form-group-jira">
        <label>Work type <span class="required">*</span></label>
        <el-select v-model="form.type" class="jira-select" placeholder="Select work type">
          <el-option label="Task" value="task">
            <template #default>
              <div class="option-with-icon">
                <i class="fa-solid fa-check-square task-icon"></i>
                <span>Task</span>
              </div>
            </template>
          </el-option>
          <el-option label="Story" value="story">
            <template #default>
              <div class="option-with-icon">
                <i class="fa-solid fa-bookmark story-icon"></i>
                <span>Story</span>
              </div>
            </template>
          </el-option>
        </el-select>
        <a href="#" class="learn-more">Learn about work types <i class="fa-solid fa-external-link"></i></a>
      </div>

      <hr class="jira-divider" />

      <!-- Status -->
      <div class="form-group-jira">
        <label>Status</label>
        <el-select v-model="form.status" class="jira-select-small">
          <el-option label="TO DO" value="todo" />
          <el-option label="IN PROGRESS" value="inprogress" />
          <el-option label="DONE" value="done" />
        </el-select>
        <p class="hint">This is the initial status upon creation.</p>
      </div>

      <!-- Summary -->
      <div class="form-group-jira">
        <label>Summary <span class="required">*</span></label>
        <el-input v-model="form.summary" class="jira-input" placeholder="What needs to be done?" />
        <p class="error-text" v-if="!form.summary && submitted">Summary is required</p>
      </div>

      <!-- Description -->
      <div class="form-group-jira">
        <label>Description</label>
        <div class="rich-editor-placeholder">
          <div class="editor-toolbar">
            <i class="fa-solid fa-font"></i>
            <i class="fa-solid fa-bold"></i>
            <i class="fa-solid fa-italic"></i>
            <i class="fa-solid fa-list-ul"></i>
            <i class="fa-solid fa-link"></i>
            <i class="fa-solid fa-image"></i>
          </div>
          <el-input 
            type="textarea" 
            v-model="form.description" 
            :rows="5" 
            placeholder="Type / to ask AI or @ to mention someone..."
            class="jira-textarea"
          />
        </div>
      </div>

      <div class="form-grid-jira">
        <!-- Assignee -->
        <div class="form-group-jira">
          <label>Assignee</label>
          <el-select v-model="form.assignee" class="jira-select" placeholder="Automatic">
            <el-option label="Unassigned" value="unassigned" />
            <el-option label="Me (Admin)" value="me" />
          </el-select>
          <a href="#" class="assign-to-me">Assign to me</a>
        </div>

        <!-- Priority -->
        <div class="form-group-jira">
          <label>Priority</label>
          <el-select v-model="form.priority" class="jira-select">
            <el-option label="Highest" value="highest" />
            <el-option label="High" value="high" />
            <el-option label="Medium" value="medium" />
            <el-option label="Low" value="low" />
          </el-select>
        </div>
      </div>

      <div class="form-grid-jira">
        <div class="form-group-jira">
          <label>Due date</label>
          <el-date-picker v-model="form.dueDate" type="date" class="jira-date" placeholder="Select date" />
        </div>
        <div class="form-group-jira">
          <label>Labels</label>
          <el-select v-model="form.labels" multiple collapse-tags class="jira-select" placeholder="Select labels" />
        </div>
      </div>

      <!-- Reporter -->
      <div class="form-group-jira">
        <label>Reporter <span class="required">*</span></label>
        <el-select v-model="form.reporter" class="jira-select">
          <el-option label="Nguyen Van A" value="user1" />
        </el-select>
      </div>

      <!-- Attachment -->
      <div class="form-group-jira">
        <label>Attachment</label>
        <div class="jira-upload-area">
          <i class="fa-solid fa-cloud-arrow-up"></i>
          <p>Drop files to attach or <span class="browse">Browse</span></p>
        </div>
      </div>

      <!-- Create another -->
      <div class="footer-check">
        <el-checkbox v-model="form.createAnother">Create another</el-checkbox>
      </div>
    </div>

    <div class="jira-footer">
      <el-button @click="handleClose" class="btn-cancel">Cancel</el-button>
      <el-button type="primary" class="btn-submit" @click="handleSubmit">Create</el-button>
    </div>
  </el-dialog>
</template>

<script setup>
import { ref, computed } from 'vue'

const props = defineProps({
  visible: Boolean
})

const emit = defineEmits(['update:visible', 'created'])

const submitted = ref(false)
const visibleComp = computed({
  get: () => props.visible,
  set: (val) => emit('update:visible', val)
})

const form = ref({
  space: 'my-team',
  type: 'task',
  status: 'todo',
  summary: '',
  description: '',
  assignee: 'unassigned',
  priority: 'medium',
  dueDate: null,
  labels: [],
  reporter: 'user1',
  createAnother: false
})

const handleClose = () => {
  visibleComp.value = false
  submitted.value = false
}

const handleSubmit = () => {
  submitted.value = true
  if (!form.value.summary) return
  emit('created', form.value)
  if (!form.value.createAnother) {
    handleClose()
  } else {
    form.value.summary = ''
    form.value.description = ''
    submitted.value = false
  }
}
</script>

<style>
/* Global Styles for Teleported Jira Dialog */
.el-overlay .jira-create-dialog.el-dialog {
  background-color: var(--bg-card) !important;
  border-radius: 4px !important;
  overflow: hidden !important;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.4) !important;
  border: 1px solid var(--border-color) !important;
  display: flex !important;
  flex-direction: column !important;
  margin-top: 5vh !important;
  width: 680px !important;
}

@media (max-width: 768px) {
  .el-overlay .jira-create-dialog.el-dialog {
    width: 95% !important;
    margin: 10px auto !important;
  }
}

.jira-create-dialog .el-dialog__header {
  background-color: var(--bg-card) !important;
  border-bottom: 2px solid var(--border-color) !important;
  padding: 16px 20px !important;
  margin-right: 0 !important;
}

.jira-create-dialog .el-dialog__title {
  color: var(--text-primary) !important;
  font-size: 20px !important;
  font-weight: 600 !important;
}

.jira-create-dialog .el-dialog__body {
  background-color: var(--bg-card) !important;
  color: var(--text-primary) !important;
  padding: 0 !important;
  flex: 1 !important;
}

.jira-create-dialog .jira-footer {
  background-color: var(--bg-card) !important;
  padding: 16px 20px !important;
  border-top: 2px solid var(--border-color) !important;
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}

.dark .el-overlay .jira-create-dialog.el-dialog {
  background-color: #22272b !important;
  border-color: #334155 !important;
}
.dark .jira-create-dialog .el-dialog__header,
.dark .jira-create-dialog .el-dialog__body {
  background-color: #22272b !important;
}
</style>

<style scoped>
.modal-body-jira {
  max-height: 70vh;
  overflow-y: auto;
  padding: 20px;
}

.custom-scrollbar::-webkit-scrollbar { width: 8px; }
.custom-scrollbar::-webkit-scrollbar-track { background: transparent; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: var(--scrollbar-thumb); border-radius: 4px; }

.required-notice { font-size: 12px; color: var(--text-muted); margin-bottom: 20px; }
.required { color: #de350b; }

.form-group-jira { margin-bottom: 24px; display: flex; flex-direction: column; gap: 8px; }
.form-group-jira label { font-size: 12px; font-weight: 600; color: var(--text-secondary); }

.jira-select, .jira-input, .jira-date { width: 100% !important; }

/* Responsive Grid */
.form-grid-jira { 
  display: grid; 
  grid-template-columns: 1fr 1fr; 
  gap: 20px; 
}

@media (max-width: 576px) {
  .form-grid-jira { 
    grid-template-columns: 1fr; 
    gap: 12px;
  }
}

:deep(.el-input__wrapper), :deep(.el-select__wrapper) {
  background-color: var(--bg-secondary) !important;
  box-shadow: 0 0 0 1px var(--border-color) inset !important;
  border-radius: 3px !important;
}

:deep(.el-input__inner) { color: var(--text-primary) !important; }

.jira-divider { border: none; border-top: 2px solid var(--border-color); margin: 32px 0; }
.hint { font-size: 11px; color: var(--text-muted); margin-top: 4px; }
.learn-more, .assign-to-me { font-size: 12px; color: #579dff; text-decoration: none; margin-top: 4px; font-weight: 500; }

.rich-editor-placeholder {
  border: 1px solid var(--border-color);
  border-radius: 3px;
  background: var(--bg-secondary);
}
.editor-toolbar {
  padding: 8px 12px;
  border-bottom: 1px solid var(--border-color);
  display: flex;
  gap: 14px;
  color: var(--text-secondary);
  font-size: 14px;
  background: var(--bg-secondary);
}
.jira-textarea :deep(.el-textarea__inner) {
  border: none !important;
  box-shadow: none !important;
  background: transparent !important;
  font-size: 14px;
  padding: 12px;
  color: var(--text-primary) !important;
}

.jira-upload-area {
  border: 2px dashed var(--border-color);
  border-radius: 3px;
  padding: 24px;
  text-align: center;
  color: var(--text-muted);
  background: var(--bg-secondary);
}
.jira-upload-area i { font-size: 24px; margin-bottom: 8px; }
.browse { color: #579dff; font-weight: 600; }

.btn-cancel { border: none; background: transparent; color: var(--text-primary); font-weight: 600; }
.btn-submit { background-color: #0052cc; border: none; font-weight: 600; color: white !important; }

.option-with-icon { display: flex; align-items: center; gap: 10px; }
.space-icon { color: #f59e0b; }
.task-icon { color: #4c9aff; }
.story-icon { color: #36b37e; }
</style>
