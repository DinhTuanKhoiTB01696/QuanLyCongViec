<template>
  <div class="rich-text-editor-container">
    <div class="rte-toolbar">
      <div class="toolbar-group">
        <select class="toolbar-select" :value="currentBlock" @change="applyBlock($event.target.value)" title="Kiểu văn bản">
          <option value="div">Normal text</option>
          <option value="h1">Heading 1</option>
          <option value="h2">Heading 2</option>
          <option value="h3">Heading 3</option>
        </select>
      </div>

      <div class="toolbar-divider"></div>

      <div class="toolbar-group">
        <button type="button" class="toolbar-btn" title="Bold" @click="runCommand('bold')"><i class="fa-solid fa-bold"></i></button>
        <button type="button" class="toolbar-btn" title="Italic" @click="runCommand('italic')"><i class="fa-solid fa-italic"></i></button>
        <button type="button" class="toolbar-btn" title="Underline" @click="runCommand('underline')"><i class="fa-solid fa-underline"></i></button>
        <button type="button" class="toolbar-btn" title="Strike" @click="runCommand('strikeThrough')"><i class="fa-solid fa-strikethrough"></i></button>
      </div>

      <div class="toolbar-divider"></div>

      <div class="toolbar-group color-group">
        <label class="color-btn" title="Màu chữ">
          <i class="fa-solid fa-font"></i>
          <input type="color" :value="textColor" @input="applyColor($event.target.value)" />
        </label>
      </div>

      <div class="toolbar-divider"></div>

      <div class="toolbar-group">
        <button type="button" class="toolbar-btn" title="Bullet list" @click="runCommand('insertUnorderedList')"><i class="fa-solid fa-list-ul"></i></button>
        <button type="button" class="toolbar-btn" title="Numbered list" @click="runCommand('insertOrderedList')"><i class="fa-solid fa-list-ol"></i></button>
      </div>

      <div class="toolbar-divider"></div>

      <div class="toolbar-group">
        <button type="button" class="toolbar-btn" title="Chèn link" @click="insertLink"><i class="fa-solid fa-link"></i></button>
        <button type="button" class="toolbar-btn" disabled title="Chưa có API upload ảnh/file"><i class="fa-regular fa-image"></i></button>
        <div class="mention-wrap">
          <button type="button" class="toolbar-btn" :disabled="!users.length" title="Nhắc thành viên" @click="isMentionOpen = !isMentionOpen">
            <i class="fa-solid fa-at"></i>
          </button>
          <div v-if="isMentionOpen" class="mention-menu">
            <button v-for="user in users" :key="user.id" type="button" class="mention-item" @click="insertMention(user)">
              <UserAvatar :user="user" size="sm" />
              <span>{{ user.fullName || user.email }}</span>
            </button>
          </div>
        </div>
        <button type="button" class="toolbar-btn" title="Chèn emoji" @click="insertEmoji"><i class="fa-regular fa-face-smile"></i></button>
        <button type="button" class="toolbar-btn" title="Chèn bảng" @click="insertTable"><i class="fa-solid fa-table"></i></button>
        <button type="button" class="toolbar-btn" title="Đường kẻ ngang" @click="runCommand('insertHorizontalRule')"><i class="fa-solid fa-minus"></i></button>
      </div>
    </div>

    <div
      ref="editorRef"
      class="rte-content"
      contenteditable="true"
      :data-placeholder="placeholder"
      @input="syncFromEditor"
      @blur="syncFromEditor"
      @keyup="detectBlock"
      @mouseup="detectBlock"
    ></div>

    <div v-if="showFooter" class="rte-footer">
      <button class="primary-btn" type="button" @click="handleSave">Save</button>
      <button class="cancel-btn" type="button" @click="handleCancel">Cancel</button>
    </div>
  </div>
</template>

<script setup>
import { nextTick, onMounted, ref, watch } from 'vue'
import UserAvatar from '@/components/common/UserAvatar.vue'

const props = defineProps({
  modelValue: {
    type: String,
    default: ''
  },
  placeholder: {
    type: String,
    default: 'Thêm mô tả...'
  },
  showFooter: {
    type: Boolean,
    default: true
  },
  users: {
    type: Array,
    default: () => []
  }
})

const emit = defineEmits(['update:modelValue', 'save', 'cancel'])

const editorRef = ref(null)
const currentBlock = ref('div')
const textColor = ref('#172b4d')
const isMentionOpen = ref(false)

const focusEditor = () => {
  editorRef.value?.focus()
}

const setEditorHtml = (value) => {
  if (editorRef.value && editorRef.value.innerHTML !== (value || '')) {
    editorRef.value.innerHTML = value || ''
  }
}

watch(
  () => props.modelValue,
  (value) => setEditorHtml(value),
  { flush: 'post' }
)

onMounted(() => {
  setEditorHtml(props.modelValue)
})

const syncFromEditor = () => {
  emit('update:modelValue', editorRef.value?.innerHTML || '')
}

const runCommand = (command, value = null) => {
  focusEditor()
  document.execCommand(command, false, value)
  syncFromEditor()
  detectBlock()
}

const applyBlock = (block) => {
  currentBlock.value = block
  runCommand('formatBlock', block)
}

const applyColor = (color) => {
  textColor.value = color
  runCommand('foreColor', color)
}

const insertLink = () => {
  const url = window.prompt('Nhập URL')
  if (!url) return
  runCommand('createLink', url)
}

const insertEmoji = () => {
  runCommand('insertText', '🙂')
}

const insertMention = (user) => {
  const label = user.fullName || user.email
  if (!label) return
  focusEditor()
  document.execCommand('insertHTML', false, `<span class="mention-chip" data-user-id="${user.id}">@${label}</span>&nbsp;`)
  syncFromEditor()
  isMentionOpen.value = false
}

const insertTable = () => {
  focusEditor()
  document.execCommand('insertHTML', false, '<table><tbody><tr><td>Cell</td><td>Cell</td></tr><tr><td>Cell</td><td>Cell</td></tr></tbody></table><p></p>')
  syncFromEditor()
}

const detectBlock = () => {
  const block = document.queryCommandValue('formatBlock')?.toLowerCase().replace(/[<>]/g, '')
  currentBlock.value = ['h1', 'h2', 'h3'].includes(block) ? block : 'div'
}

const handleSave = () => {
  syncFromEditor()
  emit('save', editorRef.value?.innerHTML || '')
}

const handleCancel = () => {
  setEditorHtml(props.modelValue)
  emit('cancel')
}

defineExpose({
  focus: () => nextTick(focusEditor)
})
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
  padding: 6px 8px;
  border-bottom: 1px solid #DFE1E6;
  background-color: #FAFBFC;
  border-radius: 3px 3px 0 0;
  flex-wrap: wrap;
  gap: 4px;
}

.toolbar-group {
  display: flex;
  align-items: center;
  gap: 2px;
}

.toolbar-divider {
  width: 1px;
  height: 22px;
  background-color: #DFE1E6;
  margin: 0 4px;
}

.toolbar-select {
  height: 30px;
  border: 0;
  background: transparent;
  color: #172B4D;
  font-size: 14px;
  border-radius: 3px;
  padding: 0 8px;
}

.toolbar-select:hover,
.toolbar-btn:hover:not(:disabled),
.color-btn:hover {
  background-color: rgba(9, 30, 66, 0.08);
}

.toolbar-btn,
.color-btn {
  background: transparent;
  border: none;
  color: #42526E;
  width: 30px;
  height: 30px;
  border-radius: 3px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: background-color 0.2s;
  font-size: 14px;
}

.toolbar-btn:disabled {
  cursor: not-allowed;
  opacity: 0.45;
}

.color-btn input {
  position: absolute;
  opacity: 0;
  pointer-events: none;
}

.mention-wrap {
  position: relative;
}

.mention-menu {
  position: absolute;
  top: calc(100% + 6px);
  left: 0;
  width: 240px;
  max-height: 220px;
  overflow-y: auto;
  background: #FFFFFF;
  border: 1px solid #DFE1E6;
  border-radius: 3px;
  box-shadow: 0 8px 16px -4px rgba(9, 30, 66, 0.25);
  padding: 6px;
  z-index: 20;
}

.mention-item {
  width: 100%;
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px;
  border: 0;
  border-radius: 3px;
  background: transparent;
  text-align: left;
  cursor: pointer;
}

.mention-item:hover {
  background: #F4F5F7;
}

.rte-content {
  min-height: 120px;
  padding: 14px 16px;
  outline: none;
  font-size: 14px;
  color: #172B4D;
  line-height: 1.5;
}

.rte-content:empty::before {
  content: attr(data-placeholder);
  color: #8993A4;
}

.rte-content :deep(ul),
.rte-content :deep(ol) {
  padding-left: 22px;
}

.rte-content :deep(a) {
  color: #0052CC;
  text-decoration: underline;
}

.rte-content :deep(table) {
  border-collapse: collapse;
  margin: 8px 0;
  width: 100%;
}

.rte-content :deep(td),
.rte-content :deep(th) {
  border: 1px solid #DFE1E6;
  padding: 6px 8px;
}

.rte-content :deep(.mention-chip) {
  color: #0052CC;
  background: #E6FCFF;
  border-radius: 3px;
  padding: 1px 4px;
  font-weight: 600;
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
}

.cancel-btn:hover {
  background-color: rgba(9, 30, 66, 0.08);
  color: #172B4D;
}
</style>
