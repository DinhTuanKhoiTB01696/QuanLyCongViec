<template>
  <el-dialog
    v-model="visible"
    title="Cập nhật trạng thái của bạn"
    width="460px"
    class="status-update-dialog"
    :before-close="handleClose"
    align-center
    append-to-body
  >
    <div class="status-content">
      <!-- Current Status Preview -->
      <div v-if="currentStatusText || currentStatusEmoji" class="current-status-box">
        <span class="field-label mb-1.5 opacity-80">Trạng thái hiện tại:</span>
        <div class="current-status-preview">
          <span class="preview-emoji">{{ currentStatusEmoji || '💬' }}</span>
          <span class="preview-text font-medium">{{ currentStatusText || 'Chưa cập nhật chi tiết' }}</span>
          <span class="delete-btn ml-auto" @click="clearStatus">
            Xóa trạng thái
          </span>
        </div>
      </div>

      <!-- Emoji Presets Grid -->
      <div class="emoji-presets mb-3">
        <span class="field-label mb-2">Chọn trạng thái nhanh</span>
        <div class="presets-grid">
          <button
            v-for="preset in presets"
            :key="preset.emoji"
            type="button"
            class="preset-btn"
            :class="{ active: selectedEmoji === preset.emoji }"
            @click="selectPreset(preset)"
          >
            <span class="preset-emoji">{{ preset.emoji }}</span>
            <span class="preset-text">{{ preset.text }}</span>
          </button>
        </div>
      </div>

      <!-- Custom Status Form -->
      <div class="custom-status-form mb-3" style="margin-top: 12px !important; padding-top: 12px !important; border-top: 1px solid rgba(56, 189, 248, 0.1);">
        <span class="field-label mb-2">Trạng thái tùy chỉnh</span>
        <div class="custom-input-wrapper">
          <div class="emoji-selector-trigger" @click="toggleEmojiPicker">
            {{ selectedEmoji || '💬' }}
          </div>
          <input
            v-model="selectedText"
            type="text"
            placeholder="Bạn đang làm gì thế?"
            maxlength="100"
          />
        </div>
      </div>

      <!-- Expiration Selector -->
      <div class="expiration-section mb-2" style="margin-top: 12px !important; padding-top: 12px !important; border-top: 1px solid rgba(56, 189, 248, 0.1);">
        <span class="field-label mb-2">Xóa trạng thái sau</span>
        <el-select v-model="expiration" class="w-full custom-select">
          <el-option label="Không xóa" value="never" />
          <el-option label="30 phút" value="30m" />
          <el-option label="1 giờ" value="1h" />
          <el-option label="Hôm nay" value="today" />
          <el-option label="Tuần này" value="week" />
        </el-select>
      </div>
    </div>

    <template #footer>
      <div class="dialog-footer flex justify-end gap-2">
        <el-button class="btn-cancel" @click="handleClose">Hủy</el-button>
        <el-button class="btn-save" type="primary" @click="saveStatus">Lưu trạng thái</el-button>
      </div>
    </template>
  </el-dialog>
</template>

<script setup>
import { ref, watch } from 'vue'
import { ElMessage } from 'element-plus'

const props = defineProps({
  modelValue: {
    type: Boolean,
    default: false
  },
  initialEmoji: {
    type: String,
    default: ''
  },
  initialText: {
    type: String,
    default: ''
  }
})

const emit = defineEmits(['update:modelValue', 'save', 'clear'])

const visible = ref(false)
const selectedEmoji = ref('💻')
const selectedText = ref('')
const expiration = ref('today')

const currentStatusEmoji = ref('')
const currentStatusText = ref('')

const presets = [
  { emoji: '💻', text: 'Đang làm việc' },
  { emoji: '☕', text: 'Nghỉ giải lao' },
  { emoji: '🤒', text: 'Nghỉ ốm' },
  { emoji: '🏠', text: 'WFH' },
  { emoji: '🚀', text: 'Đang triển khai' },
  { emoji: '💬', text: 'Đang họp' }
]

watch(() => props.modelValue, (newVal) => {
  visible.value = newVal
  if (newVal) {
    selectedEmoji.value = props.initialEmoji || '💻'
    selectedText.value = props.initialText || ''
    currentStatusEmoji.value = props.initialEmoji
    currentStatusText.value = props.initialText
  }
})

watch(visible, (newVal) => {
  emit('update:modelValue', newVal)
})

const selectPreset = (preset) => {
  selectedEmoji.value = preset.emoji
  selectedText.value = preset.text
}

const toggleEmojiPicker = () => {
  // Simple rotation picker for demo
  const emojis = ['💻', '☕', '🤒', '🏠', '🚀', '💬', '🎉', '✈️', '🔍', '💡']
  const idx = emojis.indexOf(selectedEmoji.value)
  selectedEmoji.value = emojis[(idx + 1) % emojis.length]
}

const saveStatus = () => {
  currentStatusEmoji.value = selectedEmoji.value
  currentStatusText.value = selectedText.value
  emit('save', {
    emoji: selectedEmoji.value,
    text: selectedText.value,
    expiration: expiration.value
  })
  ElMessage.success('Cập nhật trạng thái thành công')
  visible.value = false
}

const clearStatus = () => {
  selectedEmoji.value = '💻'
  selectedText.value = ''
  currentStatusEmoji.value = ''
  currentStatusText.value = ''
  emit('clear')
  ElMessage.success('Đã xóa trạng thái làm việc')
  visible.value = false
}

const handleClose = () => {
  visible.value = false
}
</script>

<style scoped>
/* Dialog styling overrides */
:deep(.status-update-dialog) {
  background-color: var(--color-surface) !important;
  border: 1px solid var(--color-border);
  border-radius: var(--sa-radius-md, 8px) !important;
  box-shadow: var(--shadow-xl) !important;
  overflow: hidden;
}

:deep(.status-update-dialog .el-dialog__header) {
  margin-right: 0;
  padding: 18px 20px 14px;
  border-bottom: 1px solid var(--color-border);
}

:deep(.status-update-dialog .el-dialog__title) {
  font-size: 15px;
  font-weight: 600;
  color: var(--color-text-primary);
}

:deep(.status-update-dialog .el-dialog__headerbtn) {
  top: 18px;
}

:deep(.status-update-dialog .el-dialog__body) {
  padding: 14px 20px;
  max-height: 70vh;
  overflow-y: auto;
}

:deep(.status-update-dialog .el-dialog__footer) {
  padding: 14px 20px;
  border-top: 1px solid var(--color-border);
  background-color: var(--color-surface-hover);
}

.field-label {
  display: block;
  font-size: 11px;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--color-text-muted);
}

/* Current status box */
.current-status-box {
  background-color: var(--sa-primary-soft);
  border: 1px dashed var(--color-accent);
  padding: 10px 14px;
  border-radius: var(--radius-card);
  margin-bottom: 20px;
}

.current-status-preview {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 14px;
  color: var(--color-text-primary);
}

.preview-emoji {
  font-size: 18px;
}

.delete-btn {
  color: var(--color-danger);
  font-weight: 500;
  font-size: 12px;
  cursor: pointer;
  transition: opacity 0.2s;
}

.delete-btn:hover {
  opacity: 0.8;
  text-decoration: underline;
}

/* Presets Grid */
.presets-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 8px;
}

.preset-btn {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 10px 14px;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-button);
  background-color: var(--color-surface);
  color: var(--color-text-primary);
  font-size: 13px;
  text-align: left;
  cursor: pointer;
  transition: all 0.2s;
  font-weight: 500;
}

.preset-btn:hover {
  background-color: var(--color-surface-hover);
  border-color: var(--color-border-hover);
}

.preset-btn.active {
  border-color: var(--color-accent);
  background-color: var(--sa-primary-soft);
  color: var(--color-accent);
  font-weight: 600;
  box-shadow: 0 0 0 1px var(--color-accent);
}

.preset-emoji {
  font-size: 16px;
}

/* Custom Input Wrapper */
.custom-input-wrapper {
  display: flex;
  align-items: center;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-input);
  background-color: var(--color-input-bg);
  overflow: hidden;
  height: 38px;
  transition: border-color 0.2s, box-shadow 0.2s;
}

.custom-input-wrapper:focus-within {
  border-color: var(--color-accent);
  box-shadow: 0 0 0 2px var(--sa-primary-soft);
}

.emoji-selector-trigger {
  padding: 0 12px;
  font-size: 18px;
  cursor: pointer;
  user-select: none;
  display: flex;
  align-items: center;
  justify-content: center;
  border-right: 1px solid var(--color-border);
  background-color: var(--color-surface-hover);
  height: 100%;
  transition: background-color 0.2s;
}

.emoji-selector-trigger:hover {
  background-color: var(--color-border);
}

.custom-input-wrapper input {
  flex: 1;
  border: none !important;
  outline: none !important;
  background: transparent !important;
  padding: 0 12px;
  color: var(--color-text-primary);
  font-size: 13px;
  height: 100% !important;
}

/* Expiration Selector */
.custom-select {
  margin-top: 6px !important;
}

:deep(.custom-select .el-select__wrapper) {
  background-color: rgba(255, 255, 255, 0.03) !important;
  border: 1px solid var(--color-border) !important;
  box-shadow: none !important;
  border-radius: 8px !important;
  padding: 4px 12px !important;
  transition: all 0.2s !important;
  height: 38px !important;
  min-height: 38px !important;
}

:deep(.custom-select .el-select__wrapper:hover) {
  border-color: rgba(56, 189, 248, 0.3) !important;
}

:deep(.custom-select .el-select__wrapper.is-focus) {
  border-color: rgba(56, 189, 248, 0.5) !important;
  box-shadow: 0 0 0 1px rgba(56, 189, 248, 0.5) !important;
}

:deep(.custom-select .el-select__selected-item) {
  color: var(--color-text-primary) !important;
  font-size: 13.5px !important;
  font-weight: 500 !important;
}

/* Footer Buttons */
:deep(.btn-cancel),
:deep(.btn-save) {
  height: 34px !important;
  padding: 0 18px !important;
  border-radius: 7px !important;
  font-size: 13px !important;
  font-weight: 600 !important;
  line-height: 34px !important;
  cursor: pointer;
  transition: all 0.2s ease !important;
  display: inline-flex !important;
  align-items: center !important;
}

:deep(.btn-cancel) {
  background: transparent !important;
  border: 1px solid rgba(148, 163, 184, 0.3) !important;
  color: var(--color-text-secondary) !important;
}

:deep(.btn-cancel:hover) {
  background-color: rgba(148, 163, 184, 0.08) !important;
  border-color: rgba(148, 163, 184, 0.5) !important;
  color: var(--color-text-primary) !important;
}

:deep(.btn-save) {
  background: linear-gradient(135deg, #0ea5e9, #0284c7) !important;
  border: none !important;
  color: #ffffff !important;
  box-shadow: 0 2px 8px rgba(14, 165, 233, 0.3) !important;
}

:deep(.btn-save:hover) {
  background: linear-gradient(135deg, #38bdf8, #0ea5e9) !important;
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(14, 165, 233, 0.4) !important;
}
</style>

