<template>
  <NexusLayout>
    <div class="stickies-wrapper">
      <header class="st-header">
        <div class="st-left">
          <i class="fa-solid fa-note-sticky text-muted"></i>
          <span class="st-title flex items-center gap-2">
            Stickies 
            <span class="bg-[var(--color-surface)] text-[var(--color-text-muted)] text-[10px] px-1.5 py-0.5 rounded" v-if="stickies.length > 0">{{ stickies.length }}</span>
            <span class="text-[11px] text-[var(--color-text-muted)] ml-2 font-normal">{{ lastSavedLabel }}</span>
          </span>
        </div>
        <div class="nexus-controls-row">
          <input type="text" class="nexus-search-input" placeholder="Search stickies..." v-model="searchQuery" />
          <button class="nexus-btn nexus-btn-primary" @click="addSticky"><i class="fa-solid fa-plus mr-1.5"></i> Add sticky</button>
        </div>
      </header>

      <div class="st-body">
        <!-- Empty State -->
        <div v-if="stickies.length === 0" class="empty-state flex flex-col items-center justify-center pt-24 h-full">
           <div class="empty-text-container text-left w-full max-w-3xl">
             <h2 class="text-[18px] font-medium text-[var(--color-text-primary)] mb-2">Stickies are quick notes and to-dos you take down on the fly.</h2>
             <p class="text-[13px] text-[var(--color-text-muted)] mb-10">Capture your thoughts and ideas effortlessly by creating stickies that you can access anytime and from anywhere.</p>
             
             <!-- Mocked background area mirroring Plane design -->
             <div class="empty-bg relative w-full h-[400px] bg-[var(--color-surface)] border border-[var(--color-border)] rounded-xl flex flex-col items-center justify-end overflow-hidden">
                <div class="absolute inset-0 flex items-center justify-center opacity-30">
                  <i class="fa-solid fa-note-sticky text-[150px] text-[var(--color-border)] rotate-12"></i>
                  <i class="fa-solid fa-note-sticky text-[120px] text-[var(--color-border)] -ml-20 -mt-20 -rotate-12"></i>
                </div>
                <button class="plane-primary-btn flex items-center gap-1.5 relative z-10 mb-8" @click="addSticky">
                  <i class="fa-solid fa-plus text-xs"></i> Add sticky
                </button>
             </div>
           </div>
        </div>

        <!-- Populated Grid -->
        <div v-else class="stickies-grid">
          <div 
          class="sticky-card" 
          v-for="sticky in filteredStickies" 
          :key="sticky.id" 
          :class="{ 'is-new': sticky.isNew }"
          :style="getDynamicColorStyle(sticky.color)"
        >
             <div 
                contenteditable="true"
                class="sticky-input" 
                @input="e => updateStickyContent(sticky, e)"
                @blur="commitStickyContent(sticky)"
                placeholder="Click to type here..."
                :ref="el => setStickyRef(sticky, el)"
                :style="{ 
                  textAlign: sticky.align
                }"
              ></div>
             
             <div class="sticky-footer">
                <div class="sf-left">
                   <!-- Custom Color Popover -->
                   <el-popover placement="bottom-start" trigger="click" popper-class="plane-popover dark custom-swatch-popover" :width="200">
                     <template #reference>
                        <button class="sf-btn"><i class="fa-solid fa-palette"></i></button>
                     </template>
                     <div class="swatch-container">
                        <div class="text-[12px] font-medium text-gray-300 mb-3 px-1">Background colors</div>
                        <div class="color-grid">
                           <div 
                             v-for="c in COLOR_PALETTE" 
                             :key="c" 
                             class="color-swatch"
                             :style="{ background: c }"
                             @click="sticky.color = c; debouncedSave()"
                           >
                             <i v-if="sticky.color === c || (!COLOR_PALETTE.includes(sticky.color) && c === COLOR_PALETTE[0])" class="fa-solid fa-check text-[10px]" :style="{ color: getContrastTextColor(c) }"></i>
                           </div>
                        </div>
                     </div>
                   </el-popover>
                   
                   <button class="sf-btn" @click="formatText('bold')"><i class="fa-solid fa-bold"></i></button>
                   <button class="sf-btn" @click="formatText('italic')"><i class="fa-solid fa-italic"></i></button>
                   <button class="sf-btn" @click="cycleAlignment(sticky)">
                     <i v-if="sticky.align === 'left'" class="fa-solid fa-align-left"></i>
                     <i v-if="sticky.align === 'center'" class="fa-solid fa-align-center"></i>
                     <i v-if="sticky.align === 'right'" class="fa-solid fa-align-right"></i>
                   </button>
                </div>
                <button class="sf-btn trash" @click="deleteSticky(sticky.id)"><i class="fa-solid fa-trash-can"></i></button>
             </div>
          </div>
        </div>
      </div>
    </div>
  </NexusLayout>
</template>

<script setup>
import { ref, onMounted, nextTick, computed } from 'vue'
import NexusLayout from '@/components/layout/NexusLayout.vue'
import { ElNotification } from 'element-plus'

import { COLOR_PALETTE, getContrastTextColor, getRandomPaletteColor, getDynamicColorStyle } from '@/utils/colors'
const stickies = ref([])
const searchQuery = ref('')
const lastSavedAt = ref(null)

// Holds textarea DOM element refs keyed by sticky id
const textareaRefs = {}

const filteredStickies = computed(() => {
  if (!searchQuery.value.trim()) return stickies.value
  const q = searchQuery.value.toLowerCase()
  return stickies.value.filter(s => 
    s.content?.toLowerCase().includes(q)
  )
})

const setStickyRef = (sticky, el) => {
  if (!el) {
    delete textareaRefs[sticky.id]
    return
  }

  textareaRefs[sticky.id] = el
  // Use a data attribute to avoid unnecessary innerHTML writes which can reset cursor
  if (document.activeElement !== el && el.innerHTML !== (sticky.content || '')) {
    el.innerHTML = sticky.content || ''
  }
}

// Debounce timer for auto-save
let saveTimer = null
const debouncedSave = () => {
  if (saveTimer) clearTimeout(saveTimer)
  saveTimer = setTimeout(() => saveToStorage(), 1000)
}

// Load from local storage when mounted
onMounted(() => {
  const saved = localStorage.getItem('plane_stickies')
  if (saved) {
    try {
      stickies.value = JSON.parse(saved)
    } catch(e) {
      console.error('Failed to parse stickies', e)
    }
  }
})

// Save to localStorage
const saveToStorage = () => {
  // Strip transient `isNew` flag before persisting
  const toSave = stickies.value.map(({ isNew, ...rest }) => rest)
  localStorage.setItem('plane_stickies', JSON.stringify(toSave))
  lastSavedAt.value = new Date()
}

const lastSavedLabel = computed(() => {
  if (!lastSavedAt.value) return ''
  return `Saved at ${lastSavedAt.value.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}`
})

// Pick a random color that is different from the last sticky's color
const getRandomColor = () => {
  const lastColor = stickies.value.length > 0
    ? stickies.value[stickies.value.length - 1].color
    : null
  return getRandomPaletteColor(lastColor)
}

const addSticky = async () => {
  if (stickies.value.length >= 100) {
    ElNotification({ 
      title: 'Limit reached', 
      message: 'Stickies are limited to 100 to ensure performance.', 
      type: 'warning' 
    })
    return
  }

  const newId = Date.now()
  const newSticky = {
    id: newId,
    content: '',
    color: getRandomColor(),
    isBold: false,
    isItalic: false,
    align: 'left',
    isNew: true 
  }
  stickies.value.unshift(newSticky) // Add to front for better UX
  saveToStorage()

  // Wait for DOM to render, then focus the new sticky
  await nextTick()
  const el = textareaRefs[newId]
  if (el) el.focus()

  // Remove the `isNew` highlight flag after animation completes
  setTimeout(() => {
    const sticky = stickies.value.find(s => s.id === newId)
    if (sticky) sticky.isNew = false
  }, 1000)
}

const deleteSticky = (id) => {
  stickies.value = stickies.value.filter(s => s.id !== id)
  delete textareaRefs[id]
  saveToStorage()
}

const updateStickyContent = (sticky, event) => {
  sticky.content = event.target.innerHTML
  debouncedSave()
}

const commitStickyContent = (sticky) => {
  const el = textareaRefs[sticky.id]
  if (el) {
    sticky.content = el.innerHTML
  }
  debouncedSave()
}

const cycleAlignment = (sticky) => {
  const alignments = ['left', 'center', 'right']
  const currentIndex = alignments.indexOf(sticky.align || 'left')
  sticky.align = alignments[(currentIndex + 1) % alignments.length]
  debouncedSave()
}

const formatText = (command) => {
  document.execCommand(command, false, null)
  debouncedSave()
}
</script>

<style scoped>
.stickies-wrapper {
  display: flex;
  flex-direction: column;
  height: 100vh;
  background:
    radial-gradient(circle at 16% 0%, rgba(56, 189, 248, 0.12), transparent 34%),
    radial-gradient(circle at 86% 0%, rgba(34, 197, 94, 0.10), transparent 28%),
    linear-gradient(180deg, #f8fbff, #eef5fb 54%, #f8fafc);
  color: var(--color-text-primary);
}

.st-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 18px 24px;
  border-bottom: 1px solid rgba(148, 163, 184, 0.22);
  background: rgba(255, 255, 255, 0.74);
  backdrop-filter: blur(14px);
  box-shadow: 0 12px 32px rgba(15, 23, 42, 0.05);
}
.st-left {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 14px;
  font-weight: 500;
}
.text-muted { color: var(--color-text-muted); }
.st-title { color: var(--color-text-primary); }

.st-right {
  display: flex;
  gap: 12px;
}
.plane-toolbar-btn {
  background: transparent;
  border: none;
  color: var(--color-text-secondary);
  cursor: pointer;
  padding: 6px;
  border-radius: 4px;
  transition: background 0.2s;
}
.plane-toolbar-btn:hover { background: var(--color-surface-hover); }
.plane-primary-btn {
  background: #0EA5E9;
  color: white;
  border: none;
  border-radius: 6px;
  padding: 6px 12px;
  font-size: 13px;
  font-weight: 500;
  cursor: pointer;
  transition: background 0.2s;
  box-shadow: 0 1px 3px rgba(0,0,0,0.3);
}
.plane-primary-btn:hover { background: #0284C7; }

.st-body {
  flex: 1;
  overflow-y: auto;
  padding: 28px;
}

.stickies-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 24px;
  align-items: start; /* prevents stretching */
}

.sticky-card {
  border-radius: 18px;
  height: 280px;
  display: flex;
  flex-direction: column;
  box-shadow: 0 22px 48px rgba(15, 23, 42, 0.10);
  transition: transform 0.2s, box-shadow 0.2s, background-color 0.3s;
  animation: slideInUp 0.3s cubic-bezier(0.16, 1, 0.3, 1);
  border: 1px solid rgba(255, 255, 255, 0.38);
  overflow: hidden;
}

[data-theme="dark"] .sticky-card {
  border: 1px solid rgba(255,255,255,0.1);
}

.sticky-card:hover { 
  box-shadow: 0 28px 64px rgba(15, 23, 42, 0.15);
  transform: translateY(-2px);
}

/* Highlight pulse for newly created sticky */
.sticky-card.is-new {
  animation: slideInUp 0.3s cubic-bezier(0.16, 1, 0.3, 1), highlightNew 1s ease-out;
}

@keyframes slideInUp {
  from { opacity: 0; transform: translateY(16px) scale(0.97); }
  to   { opacity: 1; transform: translateY(0) scale(1); }
}

@keyframes highlightNew {
  0%   { box-shadow: 0 0 0 3px rgba(14, 165, 233, 0.7); }
  60%  { box-shadow: 0 0 0 6px rgba(14, 165, 233, 0.2); }
  100% { box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.4); }
}

.sticky-input {
  flex: 1;
  background: transparent;
  border: none;
  outline: none;
  color: inherit;
  padding: 16px;
  font-family: inherit;
  font-size: 15px;
  resize: none;
  line-height: 1.6;
  direction: ltr;
  unicode-bidi: plaintext;
  white-space: pre-wrap;
  word-break: break-word;
}
.sticky-input::placeholder { color: inherit; opacity: 0.5; font-style: normal; font-weight: normal;}

.sticky-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px 16px;
  opacity: 0;
  background: linear-gradient(180deg, transparent, rgba(15, 23, 42, 0.10));
  transition: opacity 0.2s;
}
.sticky-card:hover .sticky-footer, .sticky-card:focus-within .sticky-footer { opacity: 1; }

.sf-left { display: flex; gap: 4px; }
.sf-btn {
  background: transparent;
  border: none;
  color: inherit;
  opacity: 0.6;
  cursor: pointer;
  padding: 6px;
  font-size: 14px;
  border-radius: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  width: 28px;
  height: 28px;
  transition: 0.2s;
}
.sf-btn:hover { opacity: 1; background: rgba(0,0,0,0.05); }
[data-theme="dark"] .sf-btn:hover { background: rgba(255,255,255,0.1); }

.sf-btn.active { opacity: 1; background: rgba(0,0,0,0.1); }
[data-theme="dark"] .sf-btn.active { background: rgba(255,255,255,0.15); }

.trash:hover { color: #F87171 !important; background: rgba(248, 113, 113, 0.1) !important; opacity: 1; }

/* Color Swatches Popover Override */
.swatch-container {
  padding: 8px;
}
.color-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 12px;
}
.color-swatch {
  width: 32px;
  height: 32px;
  border-radius: 4px;
  cursor: pointer;
  transition: transform 0.15s;
  display: flex;
  align-items: center;
  justify-content: center;
  border: 1px solid rgba(0,0,0,0.1);
}
[data-theme="dark"] .color-swatch {
  border: 1px solid rgba(255,255,255,0.1);
}
.color-swatch:hover {
  transform: scale(1.15);
  box-shadow: var(--shadow-md);
}

:deep(.custom-swatch-popover) {
  background-color: var(--color-surface) !important;
  border: 1px solid #2D2F36 !important;
  border-radius: 8px !important;
}

.nexus-search-input {
  min-width: 220px;
  height: 38px;
  border-radius: 12px;
  border: 1px solid rgba(148, 163, 184, 0.28);
  background: rgba(255, 255, 255, 0.9);
  color: #0f172a;
  font-weight: 700;
}

[data-theme="dark"] .stickies-wrapper {
  background:
    radial-gradient(circle at 16% 0%, rgba(56, 189, 248, 0.14), transparent 34%),
    radial-gradient(circle at 86% 0%, rgba(34, 197, 94, 0.10), transparent 28%),
    linear-gradient(180deg, #07111f, #0f172a 54%, #101827);
}

[data-theme="dark"] .st-header {
  border-bottom-color: rgba(148, 163, 184, 0.16);
  background: rgba(15, 23, 42, 0.78);
  box-shadow: 0 12px 32px rgba(0, 0, 0, 0.18);
}

[data-theme="dark"] .nexus-search-input {
  border-color: rgba(148, 163, 184, 0.22);
  background: rgba(15, 23, 42, 0.82);
  color: #e2e8f0;
}

/* Compact density */
.st-header {
  padding: 12px 16px !important;
  min-height: 54px !important;
}

.st-body {
  padding: 18px var(--sa-page-x, 24px) !important;
}

.stickies-grid {
  grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)) !important;
  gap: 14px !important;
}

.sticky-card {
  height: 210px !important;
  border-radius: 10px !important;
}

.sticky-toolbar {
  min-height: 34px !important;
  padding: 6px 8px !important;
}

.sticky-input {
  padding: 12px !important;
  font-size: 13px !important;
  line-height: 1.45 !important;
}

.nexus-search-input {
  min-width: 180px !important;
  height: 32px !important;
  border-radius: 8px !important;
  font-size: 12.5px !important;
}

@media (max-width: 700px) {
  .st-header {
    align-items: stretch !important;
    flex-direction: column !important;
    gap: 8px !important;
  }

  .st-body {
    padding: 12px !important;
  }

  .stickies-grid {
    grid-template-columns: 1fr !important;
  }

  .sticky-card {
    height: 180px !important;
  }
}
</style>



