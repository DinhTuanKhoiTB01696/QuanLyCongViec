<template>
  <el-dialog
    v-model="visibleComp"
    title="Tạo Không gian mới"
    class="jira-create-dialog responsive-dialog"
    :before-close="handleClose"
    append-to-body
    width="500px"
  >
    <div class="modal-body-jira custom-scrollbar" style="padding: 24px;">
      <el-form label-position="top" @submit.prevent>
        <div class="form-group-jira">
          <label>Tên Không gian <span class="required">*</span></label>
          <el-input v-model="form.name" class="jira-input" placeholder="Ví dụ: Phát triển phần mềm, Marketing..." />
          <p class="error-text" v-if="submitted && !form.name">Vui lòng nhập tên không gian</p>
        </div>
        
        <div class="form-group-jira">
          <label>Mô tả (Tùy chọn)</label>
          <el-input 
            type="textarea" 
            v-model="form.description" 
            :rows="3" 
            placeholder="Mục đích của không gian này là gì?"
            class="jira-textarea"
          />
        </div>

        <div class="form-group-jira">
          <label>Ngày bắt đầu <span class="required">*</span></label>
          <el-date-picker 
            v-model="form.startDate" 
            type="date" 
            class="jira-date" 
            style="width: 100%" 
            placeholder="Chọn ngày bắt đầu" 
          />
          <p class="error-text" v-if="submitted && !form.startDate">Vui lòng chọn ngày bắt đầu</p>
        </div>
      </el-form>
    </div>
    
    <div class="jira-footer" style="padding: 16px 24px; border-top: 2px solid var(--border-color); display: flex; justify-content: flex-end; gap: 12px; background-color: var(--bg-card);">
      <el-button @click="handleClose" style="border: none; background: transparent; color: var(--text-primary); font-weight: 600;">Hủy</el-button>
      <el-button type="primary" style="background-color: #0052cc; border: none; font-weight: 600; color: white;" :loading="loading" @click="handleSubmit">Tạo mới</el-button>
    </div>
  </el-dialog>
</template>

<script setup>
import { ref, computed } from 'vue'
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
  description: '',
  startDate: new Date()
})

const submitted = ref(false)
const loading = ref(false)

const handleClose = () => {
  visibleComp.value = false
  submitted.value = false
  form.value = {
    name: '',
    description: '',
    startDate: new Date()
  }
}

const handleSubmit = async () => {
  submitted.value = true
  if (!form.value.name || !form.value.startDate) return
  
  loading.value = true
  try {
    const payload = {
      name: form.value.name,
      description: form.value.description,
      startDate: form.value.startDate.toISOString(),
      endDate: null,
      departmentId: null
    }
    
    await axiosClient.post('/projects', payload)
    ElMessage.success(`Tạo không gian "${form.value.name}" thành công!`)
    emit('created')
    handleClose()
  } catch (error) {
    console.error('Create space error:', error)
    ElMessage.error(error.response?.data?.message || 'Có lỗi xảy ra khi tạo không gian')
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.required { color: #de350b; }
.form-group-jira { margin-bottom: 24px; display: flex; flex-direction: column; gap: 8px; }
.form-group-jira label { font-size: 13px; font-weight: 600; color: var(--text-secondary); }
.error-text { color: #de350b; font-size: 12px; margin-top: 4px; }
</style>
