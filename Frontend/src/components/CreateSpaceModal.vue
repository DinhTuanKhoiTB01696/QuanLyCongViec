<template>
  <el-dialog
    v-model="visibleComp"
    class="plane-create-modal"
    :show-close="false"
    :before-close="handleClose"
    append-to-body
    width="680px"
    top="10vh"
  >
    <!-- Custom close button -->
    <button class="plane-close-btn" @click="handleClose">
      <i class="fa-solid fa-xmark"></i>
    </button>

    <div class="modal-content-inner">
      
      <!-- Cover Header -->
      <div class="plane-cover-header" :style="{ background: selectedCover }">
        <button class="change-cover-btn">
          Change cover
        </button>
      </div>

      <!-- Main Form Area -->
      <div class="plane-modal-body">
        
        <!-- Floating Emoji -->
        <div class="floating-emoji-selector">
          <span>{{ selectedEmoji }}</span>
        </div>

        <el-form label-position="top" @submit.prevent>
          <!-- Name & ID Row -->
          <div class="plane-form-row">
            <div class="plane-group flex-1">
              <label>Project name</label>
              <input type="text" class="plane-input" v-model="form.name" placeholder="Name" />
              <p class="error-msg" v-if="submitted && !form.name">Name is required</p>
            </div>
            <div class="plane-group w-140">
              <label>Project ID <i class="fa-regular fa-circle-question info-icon"></i></label>
              <input type="text" class="plane-input" v-model="form.key" placeholder="ID" />
            </div>
          </div>

          <!-- Description -->
          <div class="plane-group">
            <label>Description</label>
            <textarea 
              class="plane-textarea" 
              v-model="form.description" 
              rows="4" 
              placeholder="Description"
            ></textarea>
          </div>

          <!-- Extra Fields (Networked to the API optionally, but here just UI per Plane) -->
          <div class="plane-bottom-controls">
             <button class="control-badge">
                <i class="fa-solid fa-globe"></i> Public
             </button>
             <button class="control-badge">
                <i class="fa-regular fa-user"></i> Lead
             </button>
             
             <!-- Keep StartDate hidden but functional to not break logic -->
          </div>
        </el-form>
      </div>

      <!-- Footer -->
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

const visibleComp = computed({
  get: () => props.visible,
  set: (val) => emit('update:visible', val)
})

const form = ref({
  name: '',
  key: '',
  description: '',
  startDate: new Date()
})

const selectedEmoji = ref('😃')
const selectedCover = ref('linear-gradient(135deg, #FFB88C 0%, #DE6262 100%)')

const submitted = ref(false)
const loading = ref(false)

// Auto gen ID from Name
watch(() => form.value.name, (newVal) => {
  if (!form.value.key && newVal) {
    const suggestedKey = newVal.substring(0, 4).toUpperCase().replace(/[^A-Z0-9]/g, '');
    form.value.key = suggestedKey;
  }
})

const handleClose = () => {
  visibleComp.value = false
  submitted.value = false
  form.value = {
    name: '',
    key: '',
    description: '',
    startDate: new Date()
  }
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
      startDate: form.value.startDate.toISOString(),
      endDate: null,
      departmentId: null
    }
    
    const response = await axiosClient.post('/projects', payload)
    ElMessage.success(`Tạo dự án "${form.value.name}" thành công!`)
    emit('created', response.data?.data || response.data)
    handleClose()
  } catch (error) {
    console.error('Create space error:', error)
    ElMessage.error(error.response?.data?.message || 'Có lỗi xảy ra khi tạo dự án')
  } finally {
    loading.value = false
  }
}
</script>

<style>
/* Reset ElementPlus overrides for this specific modal */
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
  background-color: #16181D;
  border-radius: 12px;
  border: 1px solid #27272A;
  overflow: hidden;
  position: relative;
  font-family: 'Inter', -apple-system, sans-serif;
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.7);
}

.plane-close-btn {
  position: absolute;
  top: 16px;
  right: 16px;
  background: rgba(0,0,0,0.4);
  backdrop-filter: blur(4px);
  border: none;
  color: #FFF;
  width: 28px;
  height: 28px;
  border-radius: 6px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  z-index: 10;
  transition: background 0.2s;
}
.plane-close-btn:hover { background: rgba(0,0,0,0.6); }

/* Cover Header */
.plane-cover-header {
  height: 180px;
  width: 100%;
  position: relative;
}

.change-cover-btn {
  position: absolute;
  bottom: 16px;
  right: 16px;
  background: #181A20;
  color: #E4E4E7;
  border: 1px solid #3F3F46;
  border-radius: 6px;
  padding: 6px 12px;
  font-size: 12px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
}
.change-cover-btn:hover { background: #27272A; }

/* Main Body */
.plane-modal-body {
  padding: 0 32px;
  position: relative;
}

.floating-emoji-selector {
  width: 52px;
  height: 52px;
  background: #27272A;
  border: 4px solid #16181D;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 26px;
  margin-top: -26px;
  margin-bottom: 24px;
  cursor: pointer;
  transition: transform 0.2s;
}
.floating-emoji-selector:hover { transform: scale(1.05); }

/* Form Controls */
.plane-form-row {
  display: flex;
  gap: 16px;
  margin-bottom: 20px;
}
.flex-1 { flex: 1; }
.w-140 { width: 140px; }

.plane-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}
.plane-group label {
  font-size: 12px;
  color: #A1A1AA;
  font-weight: 500;
  display: flex;
  align-items: center;
  gap: 6px;
}

.info-icon { font-size: 13px; color: #71717A; cursor: help; }

.plane-input, .plane-textarea {
  background-color: transparent;
  border: 1px solid transparent;
  color: #E4E4E7;
  font-family: inherit;
  font-size: 14px;
  outline: none;
  transition: border-color 0.2s;
  width: 100%;
}
.plane-input {
  height: 38px;
  border-bottom: 1px solid #27272A;
  border-radius: 0;
  padding: 8px 0;
}
.plane-textarea {
  border: 1px solid #27272A;
  border-radius: 6px;
  padding: 12px;
  resize: vertical;
}
.plane-input:focus { border-bottom-color: #3B82F6; }
.plane-textarea:focus { border-color: #3B82F6; }

.plane-input::placeholder, .plane-textarea::placeholder {
  color: #52525B;
}

.error-msg {
  color: #EF4444;
  font-size: 11px;
  margin: 0;
}

.plane-bottom-controls {
  display: flex;
  gap: 12px;
  margin-top: 16px;
  margin-bottom: 32px;
}
.control-badge {
  background: transparent;
  border: 1px solid #3F3F46;
  color: #D4D4D8;
  border-radius: 6px;
  padding: 6px 12px;
  font-size: 12px;
  font-weight: 500;
  display: flex;
  align-items: center;
  gap: 6px;
  cursor: pointer;
  transition: all 0.2s;
}
.control-badge i { color: #A1A1AA; }
.control-badge:hover { background: #27272A; }

/* Footer */
.plane-modal-footer {
  display: flex;
  justify-content: flex-end;
  align-items: center;
  gap: 12px;
  padding: 20px 32px;
  border-top: 1px solid #27272A;
}

.plane-ghost-btn {
  background: transparent;
  border: none;
  color: #A1A1AA;
  font-weight: 500;
  cursor: pointer;
  padding: 8px 16px;
  font-size: 13px;
  border-radius: 6px;
  transition: all 0.2s;
}
.plane-ghost-btn:hover { background: #27272A; color: #E4E4E7; }

.plane-primary-btn {
  background: #0EA5E9;
  color: white;
  border: none;
  padding: 8px 16px;
  border-radius: 6px;
  font-size: 13px;
  font-weight: 500;
  cursor: pointer;
  transition: background 0.2s;
  display: flex;
  align-items: center;
  gap: 6px;
}
.plane-primary-btn:hover:not(:disabled) { background: #0284C7; }
.opacity-50 { opacity: 0.5; cursor: not-allowed; }
</style>
