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
        <h2 class="dialog-title">Tạo dự án</h2>
        <div class="header-actions">
          <button class="icon-btn-ghost" @click="close" title="Đóng"><i class="fa-solid fa-xmark"></i></button>
        </div>
      </div>
    </template>

    <div class="modal-layout">
      <div class="form-column">
        <div class="form-group">
          <label class="field-label">Tên dự án</label>
          <input v-model="form.name" type="text" placeholder="Kế hoạch Sprint A" class="compact-input-field" />
        </div>

        <div class="form-group">
          <label class="field-label">Mã dự án</label>
          <input v-model="form.key" type="text" maxlength="8" placeholder="SPR" class="compact-input-field" />
        </div>

        <div class="form-group">
          <label class="field-label">Mô tả</label>
          <textarea v-model="form.description" rows="3" placeholder="Dự án này dùng để quản lý việc gì?" class="compact-textarea-field"></textarea>
        </div>

        <div class="split-grid">
          <div class="form-group">
            <label class="field-label">Ngày bắt đầu</label>
            <input v-model="form.startDate" type="date" class="compact-input-field" />
          </div>

          <div class="form-group">
            <label class="field-label">Quyền xem</label>
            <el-select v-model="form.networkType" class="full-width-select">
              <el-option label="Công khai" value="Public" />
              <el-option label="Riêng tư" value="Private" />
            </el-select>
          </div>
        </div>

        <div class="form-group">
          <label class="field-label">Biểu tượng</label>
          <input v-model="form.icon" type="text" maxlength="2" placeholder="🚀" class="compact-input-field" style="width: 60px; text-align: center;" />
        </div>
      </div>

      <div class="cover-column">
        <div class="cover-preview" :class="{ 'has-cover': Boolean(coverPreviewUrl) }" :style="coverPreviewStyle">
          <div class="cover-overlay">
            <span class="preview-badge">{{ form.icon || 'P' }}</span>
            <strong class="preview-name">{{ form.name || 'Xem trước dự án' }}</strong>
          </div>
        </div>

        <div class="gallery-header">
          <h4 class="section-title">Ảnh bìa dự án</h4>
          <p class="helper-text-muted">Chọn ảnh PNG, JPG, JPEG hoặc WEBP, tối đa 5MB. Ảnh sẽ được upload sau khi tạo dự án.</p>
        </div>

        <div class="cover-actions">
          <input
            ref="coverInputRef"
            type="file"
            class="sr-only"
            accept="image/png,image/jpeg,image/webp"
            @change="handleCoverSelected"
          />
          <button type="button" class="btn-secondary-sm" @click="openCoverPicker">
            <i class="fa-regular fa-image"></i>
            Chọn ảnh
          </button>
          <button v-if="coverFile" type="button" class="btn-ghost-sm" @click="clearCover">
            Xóa ảnh
          </button>
        </div>

        <div class="form-group">
          <label class="field-label">Mô tả ảnh bìa</label>
          <input v-model="form.coverAltText" type="text" maxlength="180" placeholder="Ảnh bìa cho dự án" class="compact-input-field" />
        </div>

        <p v-if="coverFile" class="selected-file">{{ coverFile.name }}</p>
        <p v-if="coverError" class="field-error">{{ coverError }}</p>
      </div>
    </div>

    <template #footer>
      <div class="dialog-footer-standard">
        <div class="footer-spacer"></div>
        <div class="footer-actions">
          <button class="btn-secondary-sm" @click="handleClose">Hủy</button>
          <button class="btn-primary-sm" :disabled="submitting" @click="handleSubmit">
            <i v-if="submitting" class="fa-solid fa-spinner fa-spin"></i>
            {{ submitting ? 'Đang tạo...' : 'Tạo dự án' }}
          </button>
        </div>
      </div>
    </template>
  </el-dialog>
</template>

<script setup>
import { computed, onUnmounted, ref } from 'vue'
import { ElMessage } from 'element-plus'
import axiosClient from '@/api/axiosClient'
import { useProjectStore } from '@/store/useProjectStore'

const props = defineProps({
  visible: Boolean
})

const emit = defineEmits(['update:visible', 'created'])

const visibleComp = computed({
  get: () => props.visible,
  set: (value) => emit('update:visible', value)
})

const submitting = ref(false)
const coverInputRef = ref(null)
const coverFile = ref(null)
const coverPreviewUrl = ref('')
const coverError = ref('')
const projectStore = useProjectStore()

const allowedCoverTypes = new Set(['image/png', 'image/jpeg', 'image/webp'])
const maxCoverSize = 5 * 1024 * 1024

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
  coverAltText: '',
  icon: '🚀'
})

const form = ref(createInitialForm())

const resetForm = () => {
  revokeCoverPreview()
  coverFile.value = null
  coverError.value = ''
  if (coverInputRef.value) coverInputRef.value.value = ''
  form.value = createInitialForm()
}

const revokeCoverPreview = () => {
  if (coverPreviewUrl.value) {
    URL.revokeObjectURL(coverPreviewUrl.value)
    coverPreviewUrl.value = ''
  }
}

const coverPreviewStyle = computed(() => {
  if (!coverPreviewUrl.value) return {}
  return { backgroundImage: `url("${coverPreviewUrl.value}")` }
})

const openCoverPicker = () => {
  coverInputRef.value?.click()
}

const clearCover = () => {
  revokeCoverPreview()
  coverFile.value = null
  coverError.value = ''
  if (coverInputRef.value) coverInputRef.value.value = ''
}

const handleCoverSelected = (event) => {
  const [file] = event.target.files || []
  coverError.value = ''

  if (!file) return
  if (!allowedCoverTypes.has(file.type)) {
    coverError.value = 'Ảnh bìa chỉ hỗ trợ PNG, JPG, JPEG hoặc WEBP.'
    event.target.value = ''
    return
  }
  if (file.size > maxCoverSize) {
    coverError.value = 'Ảnh bìa phải nhỏ hơn hoặc bằng 5MB.'
    event.target.value = ''
    return
  }

  revokeCoverPreview()
  coverFile.value = file
  coverPreviewUrl.value = URL.createObjectURL(file)
  if (!form.value.coverAltText.trim()) {
    form.value.coverAltText = form.value.name.trim()
      ? `Ảnh bìa dự án ${form.value.name.trim()}`
      : 'Ảnh bìa dự án'
  }
}

const handleClose = () => {
  visibleComp.value = false
  resetForm()
}

const handleSubmit = async () => {
  if (!form.value.name.trim()) {
    ElMessage.warning('Vui lòng nhập tên dự án')
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
      icon: form.value.icon || null
    })

    const createdProject = response.data?.data || response.data
    let emittedProject = createdProject
    const projectId = createdProject?.id || createdProject?.Id

    if (coverFile.value && projectId) {
      try {
        const payload = new FormData()
        payload.append('file', coverFile.value)
        payload.append('coverAltText', form.value.coverAltText.trim() || `Ảnh bìa dự án ${form.value.name.trim()}`)
        if (form.value.icon?.trim()) payload.append('icon', form.value.icon.trim())

        const coverResponse = await axiosClient.post(`/projects/${projectId}/cover`, payload)
        const coverData = coverResponse.data?.data || coverResponse.data
        emittedProject = {
          ...createdProject,
          cover: coverData?.coverUrl || coverData?.CoverUrl || createdProject?.cover,
          coverAltText: coverData?.coverAltText || coverData?.CoverAltText || form.value.coverAltText,
          icon: coverData?.icon || form.value.icon || createdProject?.icon
        }
      } catch (coverErrorResponse) {
        await projectStore.fetchAllProjects(true)
        emit('created', createdProject)
        ElMessage.error(coverErrorResponse.response?.data?.message || 'Dự án đã tạo nhưng chưa upload được ảnh bìa')
        handleClose()
        return
      }
    }

    await projectStore.fetchAllProjects(true)
    emit('created', emittedProject)
    ElMessage.success('Đã tạo dự án')
    handleClose()
  } catch (error) {
    ElMessage.error(error.response?.data?.message || 'Không thể tạo dự án')
  } finally {
    submitting.value = false
  }
}

onUnmounted(revokeCoverPreview)
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

.cover-actions {
  display: flex;
  align-items: center;
  gap: 10px;
  flex-wrap: wrap;
}

.btn-ghost-sm {
  background: transparent;
  color: var(--color-text-secondary);
  border: 1px solid transparent;
  border-radius: 6px;
  padding: 8px 12px;
  font-weight: 600;
  font-size: 13px;
  cursor: pointer;
}

.btn-ghost-sm:hover {
  background: var(--color-surface-hover);
  color: var(--color-text-primary);
}

.selected-file,
.field-error {
  margin: -6px 0 0;
  font-size: 12px;
}

.selected-file { color: var(--color-text-muted); }
.field-error { color: var(--color-danger); }

.sr-only {
  position: absolute;
  width: 1px;
  height: 1px;
  padding: 0;
  margin: -1px;
  overflow: hidden;
  clip: rect(0, 0, 0, 0);
  white-space: nowrap;
  border: 0;
}

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
