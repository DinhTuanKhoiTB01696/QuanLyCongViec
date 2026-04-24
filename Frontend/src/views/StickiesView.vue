<template>
  <NexusLayout>
    <div class="stickies-wrapper">
      <header class="st-header">
        <div class="st-left">
          <i class="fa-solid fa-note-sticky text-muted"></i>
          <span class="st-title flex items-center gap-2">Stickies <span class="bg-[var(--color-surface)] text-[var(--color-text-muted)] text-[10px] px-1.5 py-0.5 rounded" v-if="stickies.length > 0">{{ stickies.length }}</span></span>
        </div>
        <div class="nexus-controls-row">
          <input type="text" class="nexus-search-input" placeholder="Search stickies..." />
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
          v-for="sticky in stickies" 
          :key="sticky.id" 
          :class="{ 'is-new': sticky.isNew }"
          :data-sticky-color="sticky.color || 'zinc'"
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
                             v-for="c in backgroundColors" 
                             :key="c.id" 
                             class="color-swatch"
                             :data-sticky-color="c.id"
                             @click="sticky.color = c.id; debouncedSave()"
                           >
                             <i v-if="sticky.color === c.id" class="fa-solid fa-check text-[10px]"></i>
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
import { ref, onMounted, nextTick } from 'vue'
import NexusLayout from '@/components/layout/NexusLayout.vue'
import { ElNotification } from 'element-plus'

// Sticky color palette - dark theme friendly
const backgroundColors = [
  { id: 'zinc', label: 'Gray' },
  { id: 'red', label: 'Red' },
  { id: 'purple', label: 'Purple' },
  { id: 'amber', label: 'Amber' },
  { id: 'green', label: 'Green' },
  { id: 'teal', label: 'Teal' },
  { id: 'blue', label: 'Blue' },
  { id: 'indigo', label: 'Indigo' },
  { id: 'stone', label: 'Stone' },
  { id: 'lime', label: 'Lime' },
  { id: 'rose', label: 'Rose' },
  { id: 'cyan', label: 'Cyan' }
]

const stickies = ref([])
// Holds textarea DOM element refs keyed by sticky id
const textareaRefs = {}

const setStickyRef = (sticky, el) => {
  if (!el) {
    delete textareaRefs[sticky.id]
    return
  }

  textareaRefs[sticky.id] = el
  if (document.activeElement !== el && el.innerHTML !== (sticky.content || '')) {
    el.innerHTML = sticky.content || ''
  }
}

// Debounce timer for auto-save
let saveTimer = null
const debouncedSave = () => {
  if (saveTimer) clearTimeout(saveTimer)
  saveTimer = setTimeout(() => saveToStorage(), 600)
}

// Load from local storage when mounted
onMounted(() => {
  const saved = localStorage.getItem('plane_stickies')
  if (saved) {
    try {
      stickies.value = JSON.parse(saved)
    } catch(e) {}
  }
})

// Save to localStorage
const saveToStorage = () => {
  // Strip transient `isNew` flag before persisting
  const toSave = stickies.value.map(({ isNew, ...rest }) => rest)
  localStorage.setItem('plane_stickies', JSON.stringify(toSave))
}

// Pick a random color that is different from the last sticky's color
const getRandomColor = () => {
  const lastColor = stickies.value.length > 0
    ? stickies.value[stickies.value.length - 1].color
    : null
  const available = backgroundColors.filter(c => c.id !== lastColor)
  const pool = available.length > 0 ? available : backgroundColors
  return pool[Math.floor(Math.random() * pool.length)].id
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
    isBold: false, // Legacy, kept for compatibility
    isItalic: false, // Legacy, kept for compatibility
    align: 'left',
    isNew: true // triggers highlight animation
  }
  stickies.value.push(newSticky)
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
  if (sticky.align === 'left') sticky.align = 'center'
  else if (sticky.align === 'center') sticky.align = 'right'
  else sticky.align = 'left'
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
  background: var(--color-bg);
  color: var(--color-text-primary);
}

.st-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 24px;
  border-bottom: 1px solid var(--color-border);
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
  padding: 24px;
}

.stickies-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 24px;
  align-items: start; /* prevents stretching */
}

/* Color Variable Definitions (Universal) */
[data-sticky-color="zinc"] { --s-bg: #f4f4f5; --s-text: #18181b; --s-border: #e4e4e7; }
[data-sticky-color="red"] { --s-bg: #fee2e2; --s-text: #991b1b; --s-border: #fecaca; }
[data-sticky-color="purple"] { --s-bg: #f3e8ff; --s-text: #6b21a8; --s-border: #e9d5ff; }
[data-sticky-color="amber"] { --s-bg: #fef3c7; --s-text: #92400e; --s-border: #fde68a; }
[data-sticky-color="green"] { --s-bg: #dcfce7; --s-text: #166534; --s-border: #bbf7d0; }
[data-sticky-color="teal"] { --s-bg: #ccfbf1; --s-text: #115e59; --s-border: #99f6e4; }
[data-sticky-color="blue"] { --s-bg: #dbeafe; --s-text: #1e40af; --s-border: #bfdbfe; }
[data-sticky-color="indigo"] { --s-bg: #e0e7ff; --s-text: #3730a3; --s-border: #c7d2fe; }
[data-sticky-color="stone"] { --s-bg: #f5f5f4; --s-text: #1c1917; --s-border: #e7e5e4; }
[data-sticky-color="lime"] { --s-bg: #ecfccb; --s-text: #3f6212; --s-border: #d9f99d; }
[data-sticky-color="rose"] { --s-bg: #ffe4e6; --s-text: #9f1239; --s-border: #fecdd3; }
[data-sticky-color="cyan"] { --s-bg: #ecfeff; --s-text: #083344; --s-border: #cffafe; }

/* Dark Theme Overrides */
[data-theme="dark"] [data-sticky-color="zinc"] { --s-bg: #27272a; --s-text: #f4f4f5; --s-border: #3f3f46; }
[data-theme="dark"] [data-sticky-color="red"] { --s-bg: #4c2b2d; --s-text: #fca5a5; --s-border: #7f1d1d; }
[data-theme="dark"] [data-sticky-color="purple"] { --s-bg: #4a314d; --s-text: #e9d5ff; --s-border: #701a75; }
[data-theme="dark"] [data-sticky-color="amber"] { --s-bg: #5c4532; --s-text: #fde68a; --s-border: #92400e; }
[data-theme="dark"] [data-sticky-color="green"] { --s-bg: #1b4d3e; --s-text: #bbf7d0; --s-border: #064e3b; }
[data-theme="dark"] [data-sticky-color="teal"] { --s-bg: #1d4c5c; --s-text: #99f6e4; --s-border: #134e4a; }
[data-theme="dark"] [data-sticky-color="blue"] { --s-bg: #1e3a8a; --s-text: #bfdbfe; --s-border: #1e40af; }
[data-theme="dark"] [data-sticky-color="indigo"] { --s-bg: #3b2e58; --s-text: #c7d2fe; --s-border: #312e81; }
[data-theme="dark"] [data-sticky-color="stone"] { --s-bg: #292524; --s-text: #f5f5f4; --s-border: #44403c; }
[data-theme="dark"] [data-sticky-color="lime"] { --s-bg: #365314; --s-text: #d9f99d; --s-border: #1a2e05; }
[data-theme="dark"] [data-sticky-color="rose"] { --s-bg: #7c2d12; --s-text: #fecdd3; --s-border: #4c0519; }
[data-theme="dark"] [data-sticky-color="cyan"] { --s-bg: #164e63; --s-text: #cffafe; --s-border: #083344; }

.sticky-card {
  background-color: var(--s-bg);
  color: var(--s-text);
  border-radius: 2px;
  height: 280px;
  display: flex;
  flex-direction: column;
  box-shadow: var(--shadow-sm);
  transition: transform 0.2s, box-shadow 0.2s, background-color 0.3s;
  animation: slideInUp 0.3s cubic-bezier(0.16, 1, 0.3, 1);
  border: 1px solid var(--s-border);
}

.sticky-card:hover { 
  box-shadow: var(--shadow-lg);
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
  color: var(--s-text);
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
.sticky-input::placeholder { color: var(--s-text); opacity: 0.4; font-style: normal; font-weight: normal;}

.sticky-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px 16px;
  opacity: 0;
  transition: opacity 0.2s;
}
.sticky-card:hover .sticky-footer, .sticky-card:focus-within .sticky-footer { opacity: 1; }

.sf-left { display: flex; gap: 4px; }
.sf-btn {
  background: transparent;
  border: none;
  color: var(--s-text);
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
  border-radius: 2px;
  cursor: pointer;
  transition: transform 0.15s;
  display: flex;
  align-items: center;
  justify-content: center;
  background-color: var(--s-bg);
  border: 1px solid var(--s-border);
  color: var(--s-text);
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
</style>



