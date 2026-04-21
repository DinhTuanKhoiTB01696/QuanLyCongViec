<template>
  <el-dialog
    v-model="visibleComp"
    title="Create Project"
    width="720px"
    :before-close="handleClose"
    append-to-body
  >
    <div class="modal-layout">
      <div class="form-column">
        <label>
          Project name
          <input v-model="form.name" type="text" placeholder="Sprint planning" />
        </label>

        <label>
          Key
          <input v-model="form.key" type="text" maxlength="8" placeholder="SPR" />
        </label>

        <label>
          Description
          <textarea v-model="form.description" rows="4" placeholder="What is this project for?"></textarea>
        </label>

        <div class="split-grid">
          <label>
            Start date
            <input v-model="form.startDate" type="date" />
          </label>

          <label>
            Visibility
            <select v-model="form.networkType">
              <option value="Public">Public</option>
              <option value="Private">Private</option>
            </select>
          </label>
        </div>

        <label>
          Icon
          <input v-model="form.icon" type="text" maxlength="2" placeholder="🚀" />
        </label>
      </div>

      <div class="cover-column">
        <div class="cover-preview" :style="{ backgroundImage: `url(${form.cover})` }">
          <div class="cover-overlay">
            <span>{{ form.icon || 'P' }}</span>
            <strong>{{ form.name || 'Project preview' }}</strong>
          </div>
        </div>

        <div>
          <h4>Cover gallery</h4>
          <p>Choose a project cover.</p>
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
      <div class="dialog-actions">
        <button class="secondary-btn" type="button" @click="handleClose">Cancel</button>
        <button class="primary-btn" type="button" :disabled="submitting" @click="handleSubmit">
          {{ submitting ? 'Creating...' : 'Create project' }}
        </button>
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

const createInitialForm = () => ({
  name: '',
  key: '',
  description: '',
  startDate: new Date().toISOString().slice(0, 10),
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
  grid-template-columns: 1.15fr 0.85fr;
  gap: 24px;
}

.form-column,
.cover-column,
label {
  display: grid;
  gap: 8px;
}

label {
  color: #52525b;
  font-size: 13px;
  font-weight: 600;
}

input,
textarea,
select {
  width: 100%;
  border: 1px solid #d4d4d8;
  border-radius: 6px;
  padding: 10px 12px;
  font: inherit;
}

.split-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 14px;
}

.cover-preview {
  min-height: 180px;
  border-radius: 8px;
  overflow: hidden;
  background-size: cover;
  background-position: center;
  border: 1px solid #d4d4d8;
}

.cover-overlay {
  height: 100%;
  display: flex;
  flex-direction: column;
  justify-content: flex-end;
  gap: 8px;
  padding: 16px;
  background: linear-gradient(180deg, rgba(15, 23, 42, 0.05) 0%, rgba(15, 23, 42, 0.75) 100%);
  color: #fff;
}

.cover-overlay span {
  width: 40px;
  height: 40px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border-radius: 8px;
  background: rgba(255, 255, 255, 0.18);
}

.cover-column h4,
.cover-column p {
  margin: 0;
}

.cover-column p {
  color: #71717a;
  font-size: 13px;
}

.cover-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 10px;
}

.cover-option {
  border: 2px solid transparent;
  border-radius: 8px;
  min-height: 74px;
  background-size: cover;
  background-position: center;
  cursor: pointer;
}

.cover-option.active {
  border-color: #0ea5e9;
}

.dialog-actions {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}

.primary-btn,
.secondary-btn {
  border-radius: 6px;
  padding: 10px 14px;
  border: 1px solid transparent;
  cursor: pointer;
  font-weight: 600;
}

.primary-btn {
  background: #0ea5e9;
  color: #fff;
}

.secondary-btn {
  background: #fff;
  border-color: #d4d4d8;
  color: #18181b;
}

@media (max-width: 768px) {
  .modal-layout,
  .split-grid {
    grid-template-columns: 1fr;
  }
}
</style>
