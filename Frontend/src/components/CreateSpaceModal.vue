<template>
  <el-dialog
    v-model="visibleComp"
    class="plane-create-modal"
    :show-close="false"
    :before-close="handleClose"
    append-to-body
    width="760px"
    top="6vh"
  >
    <button class="plane-close-btn" @click="handleClose">
      <i class="fa-solid fa-xmark"></i>
    </button>

    <div class="modal-content-inner">
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

        <el-form label-position="top" @submit.prevent>
          <div class="plane-form-row">
            <div class="plane-group flex-1">
              <label>Project name</label>
              <input v-model="form.name" type="text" class="plane-input" placeholder="Name" />
              <p v-if="submitted && !form.name" class="error-msg">Name is required</p>
            </div>
            <div class="plane-group w-140">
              <label>Project ID <i class="fa-regular fa-circle-question info-icon"></i></label>
              <input v-model="form.key" type="text" class="plane-input" placeholder="ID" />
            </div>
          </div>

          <div class="plane-group">
            <label>Description</label>
            <textarea
              v-model="form.description"
              class="plane-textarea"
              rows="4"
              placeholder="Description"
            ></textarea>
          </div>

          <div class="project-settings-grid">
            <div class="plane-group">
              <label>Visibility</label>
              <div class="segmented-control">
                <button
                  type="button"
                  :class="{ active: form.networkType === 'Public' }"
                  @click="form.networkType = 'Public'"
                >
                  <i class="fa-solid fa-globe"></i>
                  Public
                </button>
                <button
                  type="button"
                  :class="{ active: form.networkType === 'Private' }"
                  @click="form.networkType = 'Private'"
                >
                  <i class="fa-solid fa-lock"></i>
                  Private
                </button>
              </div>
            </div>

            <div class="plane-group">
              <label>Lead</label>
              <select v-model="form.leadUserId" class="plane-select">
                <option value="">No lead</option>
                <option v-for="member in workspaceMembers" :key="member.userId" :value="member.userId">
                  {{ member.fullName || member.email }}
                </option>
              </select>
              <p v-if="membersLoading" class="hint-msg">Loading workspace members...</p>
              <p v-else-if="!workspaceMembers.length" class="hint-msg">No workspace members found.</p>
            </div>
          </div>
        </el-form>
      </div>

      <div class="plane-modal-footer">
        <button class="plane-ghost-btn" @click="handleClose">Cancel</button>
        <button class="plane-primary-btn" :class="{ 'opacity-50': loading }" :disabled="loading" @click="handleSubmit">
          <i v-if="loading" class="fa-solid fa-circle-notch fa-spin"></i>
          Create project
        </button>
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
  { name: 'Graphite', value: 'linear-gradient(135deg, #27272a 0%, #71717a 100%)' },
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
const membersLoading = ref(false)
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
  if (isVisible) {
    fetchWorkspaceMembers()
  }
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
  if (workspaceMembers.value.length || membersLoading.value) return

  membersLoading.value = true
  try {
    const workspacesRes = await axiosClient.get('/workspaces')
    const workspaces = workspacesRes.data?.data || []
    const workspaceId = workspaces[0]?.id

    if (!workspaceId) {
      workspaceMembers.value = []
      return
    }

    const membersRes = await axiosClient.get(`/workspaces/${workspaceId}/members`)
    workspaceMembers.value = (membersRes.data?.data || []).map((member) => ({
      userId: member.userId || member.UserId,
      fullName: member.fullName || member.FullName,
      email: member.email || member.Email,
      avatarUrl: member.avatarUrl || member.AvatarUrl
    })).filter((member) => member.userId)
  } catch (error) {
    console.error('Fetch workspace members error:', error)
    workspaceMembers.value = []
  } finally {
    membersLoading.value = false
  }
}

const handleClose = () => {
  visibleComp.value = false
  submitted.value = false
  showCoverPicker.value = false
  showIconPicker.value = false
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
      endDate: null,
      departmentId: null,
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
    console.error('Create space error:', error)
    ElMessage.error(error.response?.data?.message || 'Could not create project')
  } finally {
    loading.value = false
  }
}
</script>

<style>
.plane-create-modal {
  background: transparent !important;
  box-shadow: none !important;
  border-radius: 12px !important;
  padding: 0 !important;
}

.plane-create-modal .el-dialog__header {
  display: none !important;
}

.plane-create-modal .el-dialog__body {
  padding: 0 !important;
  background: transparent !important;
}
</style>

<style scoped>
.modal-content-inner {
  background-color: #16181d;
  border-radius: 12px;
  border: 1px solid #27272a;
  overflow: hidden;
  position: relative;
  font-family: 'Inter', -apple-system, sans-serif;
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.7);
}

.plane-close-btn {
  position: absolute;
  top: 16px;
  right: 16px;
  background: rgba(0, 0, 0, 0.4);
  backdrop-filter: blur(4px);
  border: none;
  color: #fff;
  width: 32px;
  height: 32px;
  border-radius: 6px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  z-index: 10;
}

.plane-cover-header {
  height: 190px;
  width: 100%;
  position: relative;
}

.change-cover-btn {
  position: absolute;
  bottom: 18px;
  right: 24px;
  background: #181a20;
  color: #e4e4e7;
  border: 1px solid #3f3f46;
  border-radius: 6px;
  padding: 8px 14px;
  font-size: 13px;
  font-weight: 500;
  cursor: pointer;
}

.cover-popover,
.icon-popover {
  position: absolute;
  z-index: 20;
  background: #18181b;
  border: 1px solid #3f3f46;
  border-radius: 8px;
  box-shadow: 0 18px 40px rgba(0, 0, 0, 0.45);
}

.cover-popover {
  right: 24px;
  bottom: 58px;
  width: 300px;
  padding: 10px;
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 8px;
}

.cover-swatch {
  height: 58px;
  border: 1px solid rgba(255, 255, 255, 0.16);
  border-radius: 6px;
  color: #fff;
  cursor: pointer;
  display: flex;
  align-items: flex-end;
  padding: 8px;
  font-size: 12px;
  font-weight: 600;
}

.cover-swatch.active,
.icon-choice.active {
  outline: 2px solid #38bdf8;
}

.plane-modal-body {
  padding: 0 32px 30px;
  position: relative;
}

.floating-emoji-selector {
  width: 58px;
  height: 58px;
  background: #27272a;
  border: 4px solid #16181d;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 28px;
  margin-top: -29px;
  margin-bottom: 24px;
  cursor: pointer;
}

.icon-popover {
  top: 42px;
  left: 32px;
  width: 272px;
  padding: 10px;
  display: grid;
  grid-template-columns: repeat(6, 1fr);
  gap: 8px;
}

.icon-choice {
  width: 34px;
  height: 34px;
  border-radius: 6px;
  border: 1px solid transparent;
  background: #27272a;
  cursor: pointer;
  font-size: 18px;
}

.plane-form-row,
.project-settings-grid {
  display: grid;
  grid-template-columns: minmax(0, 1fr) 160px;
  gap: 16px;
  margin-bottom: 20px;
}

.project-settings-grid {
  grid-template-columns: 1fr 1fr;
  margin-top: 20px;
  margin-bottom: 0;
}

.plane-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.plane-group label {
  font-size: 12px;
  color: #a1a1aa;
  font-weight: 500;
  display: flex;
  align-items: center;
  gap: 6px;
}

.plane-input,
.plane-textarea,
.plane-select {
  background-color: transparent;
  border: 1px solid #27272a;
  color: #e4e4e7;
  font-family: inherit;
  font-size: 14px;
  outline: none;
  transition: border-color 0.2s, background 0.2s;
  width: 100%;
}

.plane-input {
  height: 38px;
  border-width: 0 0 1px;
  border-radius: 0;
  padding: 8px 0;
}

.plane-textarea,
.plane-select {
  border-radius: 6px;
  padding: 12px;
}

.plane-select {
  height: 42px;
  background: #18181b;
}

.plane-input:focus,
.plane-textarea:focus,
.plane-select:focus {
  border-color: #38bdf8;
}

.segmented-control {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 8px;
}

.segmented-control button {
  height: 42px;
  border-radius: 6px;
  border: 1px solid #3f3f46;
  background: transparent;
  color: #d4d4d8;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  font-weight: 600;
}

.segmented-control button.active {
  background: #0f172a;
  border-color: #38bdf8;
  color: #fff;
}

.error-msg,
.hint-msg {
  font-size: 11px;
  margin: 0;
}

.error-msg {
  color: #ef4444;
}

.hint-msg {
  color: #71717a;
}

.plane-modal-footer {
  display: flex;
  justify-content: flex-end;
  align-items: center;
  gap: 12px;
  padding: 20px 32px;
  border-top: 1px solid #27272a;
}

.plane-ghost-btn,
.plane-primary-btn {
  border: none;
  padding: 8px 16px;
  border-radius: 6px;
  font-size: 13px;
  font-weight: 500;
  cursor: pointer;
}

.plane-ghost-btn {
  background: transparent;
  color: #a1a1aa;
}

.plane-ghost-btn:hover {
  background: #27272a;
  color: #e4e4e7;
}

.plane-primary-btn {
  background: #0ea5e9;
  color: white;
  display: flex;
  align-items: center;
  gap: 6px;
}

.plane-primary-btn:hover:not(:disabled) {
  background: #0284c7;
}

.opacity-50 {
  opacity: 0.5;
  cursor: not-allowed;
}

@media (max-width: 760px) {
  .plane-form-row,
  .project-settings-grid {
    grid-template-columns: 1fr;
  }

  .cover-popover {
    right: 12px;
    width: calc(100% - 24px);
  }
}
</style>
