<template>
  <article class="sticky-note" :class="{ compact, pinned: note.isPinned }" :style="noteStyle">
    <header>
      <button
        v-if="allowFloat"
        class="float-handle"
        type="button"
        :draggable="!note.isFloating"
        :disabled="note.isFloating"
        :title="note.isFloating ? 'Ghi chú đang ở trên màn hình' : 'Kéo ghi chú ra vùng nội dung'"
        aria-label="Kéo ghi chú ra vùng nội dung"
        @dragstart="$emit('float-drag-start', note, $event)"
        @dragend="$emit('float-drag-end')"
      >
        <i class="fa-solid fa-grip-vertical"></i>
      </button>
      <input
        v-model="note.title"
        type="text"
        maxlength="180"
        aria-label="Tiêu đề ghi chú"
        placeholder="Tiêu đề ghi chú"
        @input="scheduleSave"
      />
      <div class="note-actions">
        <button type="button" :class="{ active: note.isPinned }" :title="note.isPinned ? 'Bỏ ghim' : 'Ghim'" @click="$emit('pin', note, !note.isPinned)">
          <i class="fa-solid fa-thumbtack"></i>
        </button>
        <button type="button" title="Xóa ghi chú" @click="$emit('delete', note)">
          <i class="fa-solid fa-trash-can"></i>
        </button>
      </div>
    </header>

    <textarea
      v-model="note.content"
      maxlength="10000"
      aria-label="Nội dung ghi chú"
      placeholder="Nhập nội dung..."
      @input="scheduleSave"
    ></textarea>

    <footer>
      <div class="color-options" aria-label="Màu ghi chú">
        <button
          v-for="color in colors"
          :key="color"
          type="button"
          :class="{ selected: note.color === color }"
          :style="{ backgroundColor: color }"
          :title="`Chọn màu ${color}`"
          @click="selectColor(color)"
        ></button>
      </div>
      <span class="save-state">{{ saving ? 'Đang lưu...' : formatUpdated(note.updatedAt) }}</span>
    </footer>

    <router-link v-if="note.sourceRoute && note.sourceRoute !== '/stickies'" class="context-link" :to="note.sourceRoute">
      <i class="fa-solid fa-link"></i>
      {{ note.sourceRoute }}
    </router-link>
  </article>
</template>

<script setup>
import { computed, onBeforeUnmount } from 'vue'
import { getContrastTextColor } from '@/utils/colors'

const props = defineProps({
  note: { type: Object, required: true },
  saving: { type: Boolean, default: false },
  compact: { type: Boolean, default: false },
  allowFloat: { type: Boolean, default: false }
})

const emit = defineEmits(['save', 'delete', 'pin', 'float-drag-start', 'float-drag-end'])
const colors = ['#EAB308', '#22C55E', '#06B6D4', '#3B82F6', '#EC4899']
const noteStyle = computed(() => ({
  '--note-color': props.note.color || colors[0],
  '--note-text': getContrastTextColor(props.note.color || colors[0])
}))

let saveTimer = null
let dirty = false

const scheduleSave = () => {
  dirty = true
  clearTimeout(saveTimer)
  saveTimer = setTimeout(flushSave, 850)
}

const flushSave = () => {
  clearTimeout(saveTimer)
  saveTimer = null
  if (!dirty) return
  dirty = false
  emit('save', props.note)
}

const selectColor = color => {
  props.note.color = color
  scheduleSave()
}

const formatUpdated = value => {
  if (!value) return 'Chưa lưu'
  return new Intl.DateTimeFormat('vi-VN', { hour: '2-digit', minute: '2-digit', day: '2-digit', month: '2-digit' }).format(new Date(value))
}

onBeforeUnmount(flushSave)
</script>

<style scoped>
.sticky-note {
  min-width: 0;
  min-height: 220px;
  display: flex;
  flex-direction: column;
  gap: 10px;
  padding: 14px;
  border: 1px solid color-mix(in srgb, var(--note-color) 68%, var(--color-border));
  border-radius: 8px;
  background: color-mix(in srgb, var(--note-color) 17%, var(--color-surface));
  color: var(--color-text-primary);
  box-shadow: var(--shadow-sm);
  transition: transform 160ms ease, box-shadow 160ms ease;
}

.sticky-note:hover { transform: translateY(-1px); box-shadow: var(--shadow-md); }
.sticky-note.pinned { border-top: 3px solid var(--note-color); }
.sticky-note.compact { min-height: 184px; padding: 12px; }
header, footer, .note-actions, .color-options { display: flex; align-items: center; }
header { gap: 8px; }
.float-handle {
  width: 28px;
  height: 30px;
  display: grid;
  place-items: center;
  flex: 0 0 28px;
  border: 0;
  border-radius: 6px;
  background: transparent;
  color: var(--color-text-muted);
  cursor: grab;
}
.float-handle:hover:not(:disabled), .float-handle:focus-visible { background: var(--color-surface-hover); color: var(--color-text-primary); }
.float-handle:active:not(:disabled) { cursor: grabbing; }
.float-handle:disabled { cursor: default; opacity: .45; }
header input {
  min-width: 0;
  flex: 1;
  border: 0;
  border-bottom: 1px solid transparent;
  background: transparent;
  color: inherit;
  font-size: 14px;
  font-weight: 700;
  outline: none;
}
header input:focus { border-bottom-color: var(--note-color); }
.note-actions { gap: 2px; }
.note-actions button {
  width: 30px;
  height: 30px;
  border: 0;
  border-radius: 6px;
  background: transparent;
  color: var(--color-text-muted);
  cursor: pointer;
}
.note-actions button:hover, .note-actions button.active { background: var(--color-surface-hover); color: var(--color-text-primary); }
textarea {
  width: 100%;
  min-height: 118px;
  flex: 1;
  resize: vertical;
  border: 0;
  background: transparent;
  color: inherit;
  font: inherit;
  font-size: 13px;
  line-height: 1.5;
  outline: none;
}
.compact textarea { min-height: 84px; resize: none; }
footer { justify-content: space-between; gap: 8px; }
.color-options { gap: 5px; }
.color-options button {
  width: 16px;
  height: 16px;
  border: 1px solid rgba(15, 23, 42, 0.18);
  border-radius: 50%;
  cursor: pointer;
}
.color-options button.selected { outline: 2px solid var(--color-text-primary); outline-offset: 2px; }
.save-state { color: var(--color-text-muted); font-size: 10px; white-space: nowrap; }
.context-link {
  min-width: 0;
  overflow: hidden;
  color: var(--color-text-muted);
  font-size: 10px;
  text-decoration: none;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.context-link:hover { color: var(--color-accent); }
@media (prefers-reduced-motion: reduce) { .sticky-note { transition: none; } }
</style>
