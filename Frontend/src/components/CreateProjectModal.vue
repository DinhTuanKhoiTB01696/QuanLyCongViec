<template>
  <el-dialog
    v-model="visibleComp"
    width="740px"
    class="standard-dialog"
    :show-close="false"
    append-to-body
  >
    <template #header="{ close }">
      <div class="dialog-header-standard">
        <h2 class="dialog-title">Create Project</h2>
        <div class="header-actions">
          <button class="icon-btn-ghost" @click="close" title="Close"><i class="fa-solid fa-xmark"></i></button>
        </div>
      </div>
    </template>

    <div class="modal-layout">
      <div class="form-column">
        <div class="form-group">
          <label class="field-label">Project name</label>
          <input v-model="form.name" type="text" placeholder="Sprint planning" class="compact-input-field" />
        </div>

        <div class="form-group">
          <label class="field-label">Key</label>
          <input v-model="form.key" type="text" maxlength="8" placeholder="SPR" class="compact-input-field" />
        </div>

        <div class="form-group">
          <label class="field-label">Description</label>
          <textarea v-model="form.description" rows="3" placeholder="What is this project for?" class="compact-textarea-field"></textarea>
        </div>

        <div class="split-grid">
          <div class="form-group">
            <label class="field-label">Start date</label>
            <input v-model="form.startDate" type="date" class="compact-input-field" />
          </div>

          <div class="form-group">
            <label class="field-label">Visibility</label>
            <el-select v-model="form.networkType" class="full-width-select">
              <el-option label="Public" value="Public" />
              <el-option label="Private" value="Private" />
            </el-select>
          </div>
        </div>

        <div class="form-group">
          <label class="field-label">Icon</label>
          <input v-model="form.icon" type="text" maxlength="2" placeholder="🚀" class="compact-input-field" style="width: 60px; text-align: center;" />
        </div>
      </div>

      <div class="cover-column">
        <div class="cover-preview" :style="{ backgroundImage: `url(${form.cover})` }">
          <div class="cover-overlay">
            <span class="preview-badge">{{ form.icon || 'P' }}</span>
            <strong class="preview-name">{{ form.name || 'Project preview' }}</strong>
          </div>
        </div>

        <div class="gallery-header">
          <h4 class="section-title">Cover gallery</h4>
          <p class="helper-text-muted">Choose a project cover.</p>
        </div>

        <div class="cover-grid">
          <button
            v-for="cover in coverOptions"
            :key="cover"
            type="button"
            class="cover-option"
            :class="{ active: form.cover === cover }"
            :style="{ backgroundImage: `url(${cover})` }"
            @click="form.cover = cover"
          />
        </div>
      </div>
    </div>

    <template #footer>
      <div class="dialog-footer-standard">
        <div class="footer-spacer"></div>
        <div class="footer-actions">
          <button class="btn-secondary-sm" @click="handleClose">Cancel</button>
          <button class="btn-primary-sm" :disabled="submitting" @click="handleSubmit">
            <i v-if="submitting" class="fa-solid fa-spinner fa-spin"></i>
            {{ submitting ? 'Creating...' : 'Create project' }}
          </button>
        </div>
      </div>
    </template>
  </el-dialog>
</template>

<script setup>
import { computed, ref } from 'vue'
import { ElMessage } from 'element-plus'
import axiosClient from '@/api/axiosClient'

const props = defineProps({
  visible: Boolean
})

const emit = defineEmits(['update:visible', 'created'])

const visibleComp = computed({
  get: () => props.visible,
  set: (value) => emit('update:visible', value)
})

const submitting = ref(false)
const coverOptions = Array.from({ length: 8 }, (_, index) => `https://picsum.photos/seed/sprinta-cover-${index + 1}/640/360`)

const formatDateOnly = (value) => {
  const date = value instanceof Date ? value : new Date(value)
  if (Number.isNaN(date.getTime())) return ''
  const year = date.getFullYear()
  const month = `${date.getMonth() + 1}`.padStart(2, '0')
  const day = `${date.getDate()}`.padStart(2, '0')
  return `${year}-${month}-${day}`
}

const createInitialForm = () => ({
  name: '',
  key: '',
  description: '',
  startDate: formatDateOnly(new Date()),
  networkType: 'Public',
  cover: coverOptions[0],
  icon: '🚀'
})

const form = ref(createInitialForm())

const resetForm = () => {
  form.value = createInitialForm()
}

const handleClose = () => {
  visibleComp.value = false
  resetForm()
}

const handleSubmit = async () => {
  if (!form.value.name.trim()) {
    ElMessage.warning('Project name is required')
    return
  }

  submitting.value = true
  try {
    const response = await axiosClient.post('/projects', {
      name: form.value.name.trim(),
      key: form.value.key.trim() || null,
      description: form.value.description.trim() || null,
      startDate: form.value.startDate,
      networkType: form.value.networkType,
      cover: form.value.cover,
      icon: form.value.icon || null
    })

    emit('created', response.data?.data || response.data)
    ElMessage.success('Project created')
    handleClose()
  } catch (error) {
    ElMessage.error(error.response?.data?.message || 'Could not create project')
  } finally {
    submitting.value = false
  }
}
</script>

<style scoped>
.modal-layout {
  display: grid;
  grid-template-columns: 1.1fr 0.9fr;
  gap: 32px;
  padding: 0 24px 24px;
}

.form-column { display: flex; flex-direction: column; gap: 20px; }
.form-group { display: flex; flex-direction: column; gap: 6px; }

.field-label {
  font-size: 13px; font-weight: 700;
  color: var(--color-text-secondary);
}

.compact-input-field, .compact-textarea-field {
  background: var(--color-bg);
  border: 1px solid var(--color-border);
  border-radius: 6px;
  padding: 8px 12px;
  color: var(--color-text-primary);
  font-size: 14px;
  outline: none;
  transition: all 0.2s;
}
.compact-input-field:focus, .compact-textarea-field:focus {
  border-color: var(--color-accent);
  box-shadow: 0 0 0 2px color-mix(in srgb, var(--color-accent) 20%, transparent);
}

.split-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; }

.cover-column { display: flex; flex-direction: column; gap: 16px; }

.cover-preview {
  height: 160px; border-radius: 10px; overflow: hidden;
  background-size: cover; background-position: center;
  border: 1px solid var(--color-border);
}

.cover-overlay {
  height: 100%; display: flex; flex-direction: column; justify-content: flex-end;
  gap: 10px; padding: 16px;
  background: linear-gradient(180deg, transparent 0%, rgba(0,0,0,0.7) 100%);
}

.preview-badge {
  width: 36px; height: 36px; display: flex; align-items: center; justify-content: center;
  background: rgba(255,255,255,0.2); backdrop-filter: blur(4px);
  border-radius: 8px; font-size: 18px;
}

.preview-name { color: #fff; font-size: 16px; font-weight: 700; }

.gallery-header { margin-top: 4px; }
.section-title { font-size: 14px; font-weight: 700; color: var(--color-text-primary); margin-bottom: 4px; }
.helper-text-muted { font-size: 12px; color: var(--color-text-muted); }

.cover-grid {
  display: grid; grid-template-columns: repeat(2, 1fr); gap: 10px;
}

.cover-option {
  height: 60px; border-radius: 6px; border: 2px solid transparent;
  background-size: cover; background-position: center;
  cursor: pointer; transition: all 0.2s;
}
.cover-option.active { border-color: var(--color-accent); transform: scale(1.02); }

.dialog-header-standard {
  display: flex; align-items: center; justify-content: space-between;
  padding: 24px 24px 16px;
}
.dialog-title { font-size: 20px; font-weight: 700; color: var(--color-text-primary); margin: 0; }
.icon-btn-ghost {
  width: 32px; height: 32px; display: flex; align-items: center; justify-content: center;
  border: none; background: transparent; color: var(--color-text-muted); cursor: pointer; border-radius: 6px;
}
.icon-btn-ghost:hover { background: var(--color-surface-hover); color: var(--color-text-primary); }

.dialog-footer-standard {
  display: flex; justify-content: space-between; align-items: center;
  padding: 16px 24px 24px; border-top: 1px solid var(--color-border);
}
.footer-actions { display: flex; gap: 12px; }

.btn-primary-sm {
  background: var(--color-accent); color: #fff;
  border: none; border-radius: 6px; padding: 8px 16px;
  font-weight: 600; font-size: 13px; cursor: pointer; transition: all 0.2s;
  display: flex; align-items: center; gap: 8px;
}
.btn-primary-sm:hover { background: var(--color-accent-hover); }

.btn-secondary-sm {
  background: var(--color-surface); color: var(--color-text-primary);
  border: 1px solid var(--color-border); border-radius: 6px; padding: 8px 16px;
  font-weight: 600; font-size: 13px; cursor: pointer; transition: all 0.2s;
}
.btn-secondary-sm:hover { background: var(--color-surface-hover); border-color: var(--color-border-hover); }

.full-width-select { width: 100%; }

@media (max-width: 768px) {
  .modal-layout { grid-template-columns: 1fr; }
}
</style>

<style>
.standard-dialog.el-dialog {
  background: var(--color-surface) !important;
  border-radius: 12px !important;
  box-shadow: var(--shadow-xl) !important;
  border: 1px solid var(--color-border) !important;
}
.standard-dialog .el-dialog__header, .standard-dialog .el-dialog__body, .standard-dialog .el-dialog__footer { padding: 0 !important; }
</style>
