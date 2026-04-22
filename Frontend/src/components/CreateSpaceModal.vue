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

      <div class="plane-cover-header" :style="{ background: selectedCover.value }">
        <button class="change-cover-btn" @click="showCoverPicker = !showCoverPicker">
          Change cover
        </button>

        <div v-if="showCoverPicker" class="cover-popover">
          <button
            v-for="cover in coverOptions"
            :key="cover.name"
            class="cover-swatch"
            :class="{ active: cover.value === form.cover }"
            :style="{ background: cover.value }"
            @click="selectCover(cover.value)"
          >
            <span>{{ cover.name }}</span>
          </button>
        </div>
      </div>

      <div class="plane-modal-body">
        <button class="floating-emoji-selector" type="button" @click="showIconPicker = !showIconPicker">
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
          <button class="btn-primary-sm" :disabled="loading" @click="handleSubmit">
            <i v-if="loading" class="fa-solid fa-spinner fa-spin"></i>
            <span>Create project</span>
          </button>
        </div>
      </div>
    </div>
  </el-dialog>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import axiosClient from '@/api/axiosClient'
import { ElMessage } from 'element-plus'

const props = defineProps({
  visible: Boolean
})

const emit = defineEmits(['update:visible', 'created'])

const coverOptions = [
  { name: 'Coral', value: 'linear-gradient(135deg, #ffb199 0%, #ff5f6d 100%)' },
  { name: 'Ocean', value: 'linear-gradient(135deg, #2563eb 0%, #06b6d4 100%)' },
  { name: 'Forest', value: 'linear-gradient(135deg, #047857 0%, #84cc16 100%)' },
  { name: 'Sunset', value: 'linear-gradient(135deg, #f97316 0%, #db2777 100%)' },
  { name: 'Graphite', value: 'linear-gradient(135deg, #3f3f46 0%, #18181b 100%)' },
  { name: 'Violet', value: 'linear-gradient(135deg, #7c3aed 0%, #ec4899 100%)' }
]

const iconOptions = ['😀', '🚀', '⚡', '💡', '🔥', '🎯', '📦', '🧩', '🛠️', '📌', '🌱', '🏁']

const defaultForm = () => ({
  name: '',
  key: '',
  description: '',
  startDate: new Date(),
  cover: coverOptions[0].value,
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
const showCoverPicker = ref(false)
const showIconPicker = ref(false)

const selectedCover = computed(() => {
  return coverOptions.find((cover) => cover.value === form.value.cover) || coverOptions[0]
})

watch(() => form.value.name, (newVal) => {
  if (!form.value.key && newVal) {
    form.value.key = newVal.substring(0, 4).toUpperCase().replace(/[^A-Z0-9]/g, '')
  }
})

watch(() => props.visible, (isVisible) => {
  if (isVisible) fetchWorkspaceMembers()
})

const selectCover = (cover) => {
  form.value.cover = cover
  showCoverPicker.value = false
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
  visibleComp.value = false
  submitted.value = false
  form.value = defaultForm()
}

const handleSubmit = async () => {
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
      cover: form.value.cover,
      icon: form.value.icon,
      leadUserId: form.value.leadUserId || null
    }
    const response = await axiosClient.post('/projects', payload)
    ElMessage.success(`Created project "${form.value.name}"`)
    emit('created', response.data?.data || response.data)
    handleClose()
  } catch (error) {
    ElMessage.error(error.response?.data?.message || 'Could not create project')
  } finally {
    loading.value = false
  }
}
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
}

.change-cover-btn {
  position: absolute; bottom: 16px; right: 24px;
  background: rgba(0,0,0,0.4); color: #fff;
  border: 1px solid rgba(255,255,255,0.2); border-radius: 6px;
  padding: 6px 12px; font-size: 12px; font-weight: 600; cursor: pointer;
}

.cover-popover {
  position: absolute; bottom: 54px; right: 24px;
  width: 300px; padding: 12px; background: var(--color-surface);
  border: 1px solid var(--color-border); border-radius: 10px;
  box-shadow: var(--shadow-xl); display: grid; grid-template-columns: 1fr 1fr; gap: 8px; z-index: 20;
}

.cover-swatch {
  height: 50px; border-radius: 6px; border: 2px solid transparent;
  cursor: pointer; display: flex; align-items: center; justify-content: center;
  font-size: 11px; font-weight: 700; color: #fff; text-shadow: 0 1px 2px rgba(0,0,0,0.3);
}
.cover-swatch.active { border-color: var(--color-accent); }

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
