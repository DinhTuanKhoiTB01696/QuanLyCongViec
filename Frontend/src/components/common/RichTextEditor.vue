<template>
  <div class="rich-text-editor-container">
    <div class="rte-toolbar">
      <div class="toolbar-group">
        <button class="toolbar-btn text-style-btn">
          Normal text <i class="fa-solid fa-chevron-down ms-1"></i>
        </button>
      </div>
      <div class="toolbar-divider"></div>
      <div class="toolbar-group">
        <button class="toolbar-btn"><i class="fa-solid fa-bold"></i></button>
        <button class="toolbar-btn"><i class="fa-solid fa-italic"></i></button>
        <button class="toolbar-btn"><i class="fa-solid fa-ellipsis"></i></button>
      </div>
      <div class="toolbar-divider"></div>
      <div class="toolbar-group">
        <button class="toolbar-btn">
          <i class="fa-solid fa-font"></i><i class="fa-solid fa-chevron-down ms-1" style="font-size: 10px;"></i>
        </button>
      </div>
      <div class="toolbar-divider"></div>
      <div class="toolbar-group">
        <button class="toolbar-btn"><i class="fa-solid fa-list-ul"></i></button>
        <button class="toolbar-btn"><i class="fa-solid fa-list-ol"></i></button>
      </div>
      <div class="toolbar-divider"></div>
      <div class="toolbar-group">
        <button class="toolbar-btn"><i class="fa-solid fa-link"></i></button>
        <button class="toolbar-btn" @click="triggerFileUpload"><i class="fa-regular fa-image"></i></button>
        <button class="toolbar-btn"><i class="fa-solid fa-at"></i></button>
        <button class="toolbar-btn"><i class="fa-regular fa-face-smile"></i></button>
        <button class="toolbar-btn"><i class="fa-solid fa-table"></i></button>
        <button class="toolbar-btn"><i class="fa-solid fa-columns"></i></button>
        <button class="toolbar-btn"><i class="fa-solid fa-plus"></i><i class="fa-solid fa-chevron-down ms-1" style="font-size: 10px;"></i></button>
      </div>
    </div>
    
    <input type="file" ref="fileInputRef" style="display: none" accept="image/*" @change="handleFileUpload" />

    <div class="rte-content-area">
      <textarea 
        class="rte-textarea" 
        v-model="internalValue" 
        :placeholder="placeholder"
        ref="textareaRef"
        @input="autoResize"
      ></textarea>
    </div>
    
    <div class="rte-footer">
      <button class="primary-btn" @click="handleSave">Save</button>
      <button class="cancel-btn" @click="handleCancel">Cancel</button>
    </div>
  </div>
</template>

<script setup>
import { ref, watch, onMounted } from 'vue'
import { uploadApi } from '@/api/uploadApi'

const props = defineProps({
  modelValue: {
    type: String,
    default: ''
  },
  placeholder: {
    type: String,
    default: 'Thêm mô tả...'
  }
})

const emit = defineEmits(['update:modelValue', 'save', 'cancel'])

const internalValue = ref(props.modelValue)
const textareaRef = ref(null)
const fileInputRef = ref(null)

watch(() => props.modelValue, (newVal) => {
  internalValue.value = newVal
  autoResize()
})

const autoResize = () => {
  if (textareaRef.value) {
    textareaRef.value.style.height = 'auto'
    textareaRef.value.style.height = (textareaRef.value.scrollHeight) + 'px'
  }
}

onMounted(() => {
  autoResize()
})

const handleSave = () => {
  emit('update:modelValue', internalValue.value)
  emit('save', internalValue.value)
}

const handleCancel = () => {
  internalValue.value = props.modelValue
  emit('cancel')
}

const triggerFileUpload = () => {
  if (fileInputRef.value) {
    fileInputRef.value.click()
  }
}

const handleFileUpload = async (event) => {
  const file = event.target.files[0]
  if (!file) return

  try {
    const response = await uploadApi.uploadImage(file)
    const imageUrl = response.data.url
    
    // Insert markdown image syntax at current cursor position or at the end
    const cursorPosition = textareaRef.value?.selectionStart || internalValue.value.length
    const textBefore = internalValue.value.substring(0, cursorPosition)
    const textAfter = internalValue.value.substring(cursorPosition)
    
    internalValue.value = `${textBefore}\n![Image](${imageUrl})\n${textAfter}`
    emit('update:modelValue', internalValue.value)
    
    // Clear the input
    event.target.value = ''
    autoResize()
  } catch (error) {
    console.error('Failed to upload image:', error)
    alert('Không thể tải ảnh lên. Vui lòng thử lại.')
  }
}
</script>

<style scoped>
.rich-text-editor-container {
  border: 1px solid #DFE1E6;
  border-radius: 3px;
  background-color: #FFFFFF;
  display: flex;
  flex-direction: column;
  box-shadow: 0 1px 1px rgba(9, 30, 66, 0.08);
  margin-bottom: 16px;
}

.rte-toolbar {
  display: flex;
  align-items: center;
  padding: 4px 8px;
  border-bottom: 1px solid #DFE1E6;
  background-color: #FAFBFC;
  border-radius: 3px 3px 0 0;
  flex-wrap: wrap;
  gap: 4px;
}

.toolbar-group {
  display: flex;
  align-items: center;
}

.toolbar-divider {
  width: 1px;
  height: 20px;
  background-color: #DFE1E6;
  margin: 0 4px;
}

.toolbar-btn {
  background: transparent;
  border: none;
  color: #42526E;
  width: 32px;
  height: 32px;
  border-radius: 3px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: background-color 0.2s;
  font-size: 14px;
}

.toolbar-btn:hover {
  background-color: rgba(9, 30, 66, 0.08);
  color: #172B4D;
}

.text-style-btn {
  width: auto;
  padding: 0 8px;
  font-size: 14px;
  font-weight: 500;
}

.ms-1 {
  margin-left: 4px;
}

.rte-content-area {
  padding: 12px 16px;
  min-height: 120px;
}

.rte-textarea {
  width: 100%;
  min-height: 100px;
  border: none;
  resize: none;
  outline: none;
  font-size: 14px;
  color: #172B4D;
  font-family: inherit;
  line-height: 1.5;
  background-color: transparent;
}

.rte-textarea::placeholder {
  color: #8993A4;
}

.rte-footer {
  padding: 12px 16px;
  display: flex;
  gap: 8px;
  align-items: center;
}

.primary-btn {
  background-color: #0052CC;
  color: white;
  border: none;
  padding: 6px 12px;
  border-radius: 3px;
  font-weight: 500;
  font-size: 14px;
  cursor: pointer;
  transition: background-color 0.2s;
}

.primary-btn:hover {
  background-color: #0047B3;
}

.cancel-btn {
  background: transparent;
  color: #42526E;
  border: none;
  padding: 6px 12px;
  border-radius: 3px;
  font-weight: 500;
  font-size: 14px;
  cursor: pointer;
  transition: background-color 0.2s;
}

.cancel-btn:hover {
  background-color: rgba(9, 30, 66, 0.08);
  color: #172B4D;
}
</style>
