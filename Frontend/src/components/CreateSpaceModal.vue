<template>
  <el-dialog
    v-model="visibleComp"
    class="standard-dialog no-header-dialog"
    :show-close="false"
    :before-close="handleClose"
    append-to-body
    width="760px"
    top="6vh"
  >
    <div class="modal-content-inner">
      <button class="floating-close-btn" @click="handleClose">
        <i class="fa-solid fa-xmark"></i>
      </button>

      <div
        class="plane-cover-header"
        :class="{ 'has-cover-preview': coverPreviewUrl }"
        :style="coverPreviewStyle"
        :role="coverPreviewUrl ? 'img' : undefined"
        :aria-label="coverPreviewUrl ? (form.coverAltText || `${form.name || 'Project'} cover preview`) : undefined"
      >
        <div v-if="!coverPreviewUrl" class="cover-empty-state">
          <i class="fa-regular fa-image"></i>
          <span>No cover selected</span>
        </div>

        <div class="cover-actions">
          <label class="change-cover-btn">
            <input
              ref="coverInput"
              class="cover-file-input"
              type="file"
              accept=".png,.jpg,.jpeg,.webp,image/png,image/jpeg,image/webp"
              @change="handleCoverFileChange"
            />
            <span>{{ coverFile ? 'Replace cover' : 'Choose cover' }}</span>
          </label>
          <button v-if="coverFile" class="remove-cover-btn" type="button" @click="removeCoverFile">
            Remove
          </button>
        </div>
      </div>

      <div class="plane-modal-body">
        <button class="floating-emoji-selector" type="button" aria-label="Choose project icon" @click="showIconPicker = !showIconPicker">
          <span>{{ form.icon }}</span>
        </button>

        <div v-if="showIconPicker" class="icon-popover">
          <button
            v-for="icon in iconOptions"
            :key="icon"
            class="icon-choice"
            :class="{ active: icon === form.icon }"
            @click="selectIcon(icon)"
          >
            {{ icon }}
          </button>
        </div>

        <div class="form-container">
          <div class="form-row">
            <div class="form-group flex-1">
              <label class="field-label">Project name</label>
              <input v-model="form.name" type="text" class="underlined-input" placeholder="Project Name" />
              <p v-if="submitted && !form.name" class="error-text">Name is required</p>
            </div>
            <div class="form-group w-140">
              <label class="field-label">Project ID</label>
              <input v-model="form.key" type="text" class="underlined-input" placeholder="ID" maxlength="8" />
            </div>
          </div>

          <div class="form-group">
            <label class="field-label">Description</label>
            <textarea
              v-model="form.description"
              class="compact-textarea-field"
              rows="3"
              placeholder="What is this project for?"
            ></textarea>
          </div>

          <div class="form-group">
            <label class="field-label">Cover alt text</label>
            <input
              v-model="form.coverAltText"
              type="text"
              class="compact-input-field"
              :disabled="!coverFile"
              :placeholder="coverFile ? 'Describe the project cover' : 'Choose a cover image first'"
              maxlength="300"
            />
            <p v-if="coverFile" class="cover-file-meta">{{ coverFile.name }} · {{ formatFileSize(coverFile.size) }}</p>
          </div>

          <div class="settings-grid">
            <div class="form-group">
              <label class="field-label">Visibility</label>
              <div class="segmented-control">
                <button
                  type="button"
                  :class="{ active: form.networkType === 'Public' }"
                  @click="form.networkType = 'Public'"
                >
                  <i class="fa-solid fa-globe"></i>
                  <span>Public</span>
                </button>
                <button
                  type="button"
                  :class="{ active: form.networkType === 'Private' }"
                  @click="form.networkType = 'Private'"
                >
                  <i class="fa-solid fa-lock"></i>
                  <span>Private</span>
                </button>
              </div>
            </div>

            <div class="form-group">
              <label class="field-label">Lead</label>
              <el-select v-model="form.leadUserId" class="full-width-select" placeholder="Select project lead">
                <el-option value="" label="No lead" />
                <el-option v-for="member in workspaceMembers" :key="member.userId" :label="member.fullName || member.email" :value="member.userId" />
              </el-select>
            </div>
          </div>
        </div>
      </div>

      <div class="dialog-footer-standard">
        <div class="footer-spacer"></div>
        <div class="footer-actions">
          <button class="btn-secondary-sm" @click="handleClose">Cancel</button>
          <button class="btn-primary-sm" type="button" :disabled="loading" @click="handleSubmit">
            <i v-if="loading" class="fa-solid fa-spinner fa-spin"></i>
            <span>Create project</span>
          </button>
        </div>
      </div>
    </div>
  </el-dialog>
</template>

<script setup>
import { ref, computed, onBeforeUnmount, watch } from 'vue'
import axiosClient from '@/api/axiosClient'
import { ElMessage } from 'element-plus'
import { useProjectStore } from '@/store/useProjectStore'

const props = defineProps({
  visible: Boolean
})

const emit = defineEmits(['update:visible', 'created'])

const iconOptions = ['😀', '🚀', '⚡', '💡', '🔥', '🎯', '📦', '🧩', '🛠️', '📌', '🌱', '🏁']
const allowedCoverTypes = new Set(['image/png', 'image/jpeg', 'image/webp'])
const allowedCoverExtensions = new Set(['png', 'jpg', 'jpeg', 'webp'])
const maxCoverSizeBytes = 5 * 1024 * 1024
const projectStore = useProjectStore()

const defaultForm = () => ({
  name: '',
  key: '',
  description: '',
  startDate: new Date(),
  coverAltText: '',
  icon: '😀',
  networkType: 'Public',
  leadUserId: ''
})

const visibleComp = computed({
  get: () => props.visible,
  set: (val) => emit('update:visible', val)
})

const formatLocalIsoDate = (value) => {
  const date = value instanceof Date ? value : new Date(value)
  if (Number.isNaN(date.getTime())) return null
  const year = date.getFullYear()
  const month = `${date.getMonth() + 1}`.padStart(2, '0')
  const day = `${date.getDate()}`.padStart(2, '0')
  return `${year}-${month}-${day}T00:00:00`
}

const form = ref(defaultForm())
const submitted = ref(false)
const loading = ref(false)
const workspaceMembers = ref([])
const showIconPicker = ref(false)
const coverInput = ref(null)
const coverFile = ref(null)
const coverPreviewUrl = ref('')

const coverPreviewStyle = computed(() => coverPreviewUrl.value
  ? { backgroundImage: `linear-gradient(rgba(15, 23, 42, 0.08), rgba(15, 23, 42, 0.32)), url("${coverPreviewUrl.value}")` }
  : {})

watch(() => form.value.name, (newVal) => {
  if (!form.value.key && newVal) {
    form.value.key = newVal.substring(0, 4).toUpperCase().replace(/[^A-Z0-9]/g, '')
  }
})

watch(() => props.visible, (isVisible) => {
  if (isVisible) fetchWorkspaceMembers()
})

const clearCoverPreview = () => {
  if (coverPreviewUrl.value) URL.revokeObjectURL(coverPreviewUrl.value)
  coverPreviewUrl.value = ''
}

const removeCoverFile = () => {
  clearCoverPreview()
  coverFile.value = null
  form.value.coverAltText = ''
  if (coverInput.value) coverInput.value.value = ''
}

const formatFileSize = (size) => `${(size / 1024 / 1024).toFixed(2)} MB`

const handleCoverFileChange = (event) => {
  const file = event.target.files?.[0]
  if (!file) return

  const extension = file.name.split('.').pop()?.toLowerCase() || ''
  if (!allowedCoverTypes.has(file.type.toLowerCase()) || !allowedCoverExtensions.has(extension)) {
    ElMessage.error('Project cover must be PNG, JPG, JPEG, or WEBP.')
    event.target.value = ''
    return
  }
  if (file.size > maxCoverSizeBytes) {
    ElMessage.error('Project cover must be 5MB or smaller.')
    event.target.value = ''
    return
  }

  clearCoverPreview()
  coverFile.value = file
  coverPreviewUrl.value = URL.createObjectURL(file)
}

const selectIcon = (icon) => {
  form.value.icon = icon
  showIconPicker.value = false
}

const fetchWorkspaceMembers = async () => {
  try {
    const workspacesRes = await axiosClient.get('/workspaces')
    const workspaces = workspacesRes.data?.data || []
    const workspaceId = workspaces[0]?.id
    if (!workspaceId) return

    const membersRes = await axiosClient.get(`/workspaces/${workspaceId}/members`)
    workspaceMembers.value = (membersRes.data?.data || []).map((member) => ({
      userId: member.userId || member.UserId,
      fullName: member.fullName || member.FullName,
      email: member.email || member.Email
    })).filter((m) => m.userId)
  } catch (error) {
    console.error('Fetch members error:', error)
  }
}

const handleClose = () => {
  if (loading.value) return
  resetModal()
}

const resetModal = () => {
  visibleComp.value = false
  submitted.value = false
  form.value = defaultForm()
  removeCoverFile()
  showIconPicker.value = false
}

const handleSubmit = async () => {
  if (loading.value) return
  submitted.value = true
  if (!form.value.name) return
  loading.value = true
  try {
    const payload = {
      name: form.value.name,
      key: form.value.key,
      description: form.value.description,
      startDate: formatLocalIsoDate(form.value.startDate),
      networkType: form.value.networkType,
      icon: form.value.icon,
      leadUserId: form.value.leadUserId || null
    }
    const response = await axiosClient.post('/projects', payload)
    const createdProject = response.data?.data || response.data
    const projectId = createdProject?.id || createdProject?.Id
    if (!projectId) throw new Error('Project was created without an ID in the response.')

    if (coverFile.value) {
      const coverFormData = new FormData()
      coverFormData.append('file', coverFile.value)
      coverFormData.append('coverAltText', form.value.coverAltText.trim())
      coverFormData.append('icon', form.value.icon)

      try {
        await axiosClient.post(`/projects/${projectId}/cover`, coverFormData)
      } catch (coverError) {
        await projectStore.fetchAllProjects(true).catch(() => {})
        emit('created', createdProject)
        const uploadMessage = coverError.response?.data?.message || 'The cover upload failed.'
        ElMessage.warning(`Project "${form.value.name}" was created, but its cover was not saved. ${uploadMessage} Do not create the project again; add the cover from Project settings.`)
        resetModal()
        return
      }
    }

    await projectStore.fetchAllProjects(true)
    const refreshedProject = projectStore.allProjects.find((project) => project.id === projectId) || createdProject
    ElMessage.success(`Created project "${form.value.name}"`)
    emit('created', refreshedProject)
    resetModal()
  } catch (error) {
    ElMessage.error(error.response?.data?.message || 'Could not create project')
  } finally {
    loading.value = false
  }
}

onBeforeUnmount(clearCoverPreview)
</script>

<style scoped>
.modal-content-inner {
  position: relative;
  background: var(--color-surface);
  border-radius: 12px;
}

.floating-close-btn {
  position: absolute; top: 16px; right: 16px;
  width: 32px; height: 32px; border-radius: 6px;
  background: rgba(0,0,0,0.3); backdrop-filter: blur(4px);
  border: none; color: #fff; cursor: pointer; z-index: 10;
  display: flex; align-items: center; justify-content: center;
  transition: all 0.2s;
}
.floating-close-btn:hover { background: rgba(0,0,0,0.5); transform: scale(1.05); }

.plane-cover-header {
  height: 180px; width: 100%; position: relative;
  display: flex; align-items: center; justify-content: center;
  background: color-mix(in srgb, var(--color-surface) 84%, var(--color-bg));
  background-position: center; background-size: cover;
  border-bottom: 1px solid var(--color-border);
}

.cover-empty-state {
  display: flex; flex-direction: column; align-items: center; gap: 8px;
  color: var(--color-text-muted); font-size: 12px; font-weight: 600;
}
.cover-empty-state i { font-size: 28px; }

.cover-actions { position: absolute; bottom: 16px; right: 24px; z-index: 4; display: flex; gap: 8px; }

.change-cover-btn {
  position: relative; overflow: hidden; display: inline-flex; align-items: center;
  background: rgba(0,0,0,0.4); color: #fff;
  border: 1px solid rgba(255,255,255,0.2); border-radius: 6px;
  padding: 6px 12px; font-size: 12px; font-weight: 600; cursor: pointer;
}
.cover-file-input { position: absolute; inset: 0; width: 100%; height: 100%; opacity: 0; cursor: pointer; }

.remove-cover-btn {
  background: rgba(15,23,42,0.72); color: #fff;
  border: 1px solid rgba(255,255,255,0.28); border-radius: 6px;
  padding: 6px 12px; font-size: 12px; font-weight: 600; cursor: pointer;
}

.plane-modal-body { padding: 0 32px 32px; position: relative; }

.floating-emoji-selector {
  width: 64px; height: 64px; border-radius: 12px;
  background: var(--color-surface); border: 4px solid var(--color-surface);
  box-shadow: var(--shadow-md); display: flex; align-items: center; justify-content: center;
  font-size: 32px; margin-top: -32px; margin-bottom: 24px; cursor: pointer; transition: all 0.2s;
}
.floating-emoji-selector:hover { transform: translateY(-2px); box-shadow: var(--shadow-lg); }

.icon-popover {
  position: absolute; top: 40px; left: 32px;
  width: 280px; padding: 12px; background: var(--color-surface);
  border: 1px solid var(--color-border); border-radius: 10px;
  box-shadow: var(--shadow-xl); display: grid; grid-template-columns: repeat(6, 1fr); gap: 8px; z-index: 20;
}

.icon-choice {
  width: 36px; height: 36px; border-radius: 6px; border: 1px solid var(--color-border);
  background: var(--color-bg); cursor: pointer; font-size: 18px; display: flex; align-items: center; justify-content: center;
}
.icon-choice:hover { background: var(--color-surface-hover); }
.icon-choice.active { border-color: var(--color-accent); background: color-mix(in srgb, var(--color-accent) 10%, transparent); }

.form-container { display: flex; flex-direction: column; gap: 24px; }
.form-row { display: flex; gap: 24px; align-items: flex-end; }
.form-group { display: flex; flex-direction: column; gap: 8px; }

.field-label { font-size: 12px; font-weight: 700; color: var(--color-text-muted); text-transform: uppercase; letter-spacing: 0.05em; }

.underlined-input {
  background: transparent; border: none; border-bottom: 2px solid var(--color-border);
  padding: 8px 0; font-size: 18px; font-weight: 600; color: var(--color-text-primary);
  width: 100%; outline: none; transition: all 0.2s;
}
.underlined-input:focus { border-color: var(--color-accent); }

.compact-textarea-field {
  background: var(--color-bg); border: 1px solid var(--color-border);
  border-radius: 8px; padding: 12px; font-size: 14px; color: var(--color-text-primary);
  outline: none; transition: all 0.2s; resize: none;
}
.compact-textarea-field:focus { border-color: var(--color-accent); box-shadow: 0 0 0 2px color-mix(in srgb, var(--color-accent) 15%, transparent); }

.compact-input-field {
  height: 38px; background: var(--color-bg); border: 1px solid var(--color-border);
  border-radius: 8px; padding: 0 12px; font-size: 14px; color: var(--color-text-primary);
  outline: none; transition: border-color 0.2s, box-shadow 0.2s;
}
.compact-input-field:focus { border-color: var(--color-accent); box-shadow: 0 0 0 2px color-mix(in srgb, var(--color-accent) 15%, transparent); }
.compact-input-field:disabled { cursor: not-allowed; opacity: 0.65; }
.cover-file-meta { margin: 0; font-size: 11px; color: var(--color-text-muted); overflow-wrap: anywhere; }

.settings-grid { display: grid; grid-template-columns: 1fr 1.2fr; gap: 24px; }

.segmented-control { display: flex; gap: 8px; background: var(--color-bg); padding: 4px; border-radius: 8px; border: 1px solid var(--color-border); }
.segmented-control button {
  flex: 1; height: 36px; border: none; background: transparent;
  border-radius: 6px; display: flex; align-items: center; justify-content: center;
  gap: 8px; cursor: pointer; color: var(--color-text-secondary); font-size: 13px; font-weight: 600; transition: all 0.2s;
}
.segmented-control button.active { background: var(--color-surface); color: var(--color-accent); box-shadow: var(--shadow-sm); }

.full-width-select { width: 100%; }

.error-text { font-size: 11px; color: #ef4444; margin-top: 4px; }

.dialog-footer-standard {
  display: flex; justify-content: space-between; align-items: center;
  padding: 16px 32px 32px; border-top: 1px solid var(--color-border);
}

.footer-actions { display: flex; gap: 12px; }

.btn-primary-sm {
  background: var(--color-accent); color: #fff;
  border: none; border-radius: 6px; padding: 10px 20px;
  font-weight: 700; font-size: 14px; cursor: pointer; transition: all 0.2s;
  display: flex; align-items: center; gap: 8px;
}
.btn-primary-sm:hover:not(:disabled) { background: var(--color-accent-hover); transform: translateY(-1px); }

.btn-secondary-sm {
  background: transparent; color: var(--color-text-secondary);
  border: 1px solid var(--color-border); border-radius: 6px; padding: 10px 20px;
  font-weight: 600; font-size: 14px; cursor: pointer; transition: all 0.2s;
}
.btn-secondary-sm:hover { background: var(--color-surface-hover); color: var(--color-text-primary); }

.w-140 { width: 140px; }
.flex-1 { flex: 1; }
</style>

<style>
.no-header-dialog.el-dialog {
  background: transparent !important;
  box-shadow: none !important;
  border: none !important;
}
.no-header-dialog .el-dialog__header { display: none !important; }
.no-header-dialog .el-dialog__body { padding: 0 !important; }
</style>
