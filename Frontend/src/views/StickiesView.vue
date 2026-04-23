<template>
  <NexusLayout>
    <div class="stickies-wrapper">
      <header class="nexus-feature-header">
        <div class="header-info">
          <p class="eyebrow">Personal Space</p>
          <h1><i class="fa-solid fa-note-sticky"></i> Stickies <span class="text-muted ml-3 text-sm font-normal" v-if="stickies.length > 0">{{ stickies.length }}</span></h1>
          <p class="muted">Capture your thoughts and ideas effortlessly. Your personal digital whiteboard.</p>
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
             <h2 class="text-[18px] font-medium text-[#E4E4E7] mb-2">Stickies are quick notes and to-dos you take down on the fly.</h2>
             <p class="text-[13px] text-[#A1A1AA] mb-10">Capture your thoughts and ideas effortlessly by creating stickies that you can access anytime and from anywhere.</p>
             
             <!-- Mocked background area mirroring Plane design -->
             <div class="empty-bg relative w-full h-[400px] bg-[#141518] border border-[#1E2025] rounded-xl flex flex-col items-center justify-end overflow-hidden">
                <div class="absolute inset-0 flex items-center justify-center opacity-30">
                  <i class="fa-solid fa-note-sticky text-[150px] text-[#27272A] rotate-12"></i>
                  <i class="fa-solid fa-note-sticky text-[120px] text-[#1E2025] -ml-20 -mt-20 -rotate-12"></i>
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
          :style="{ backgroundColor: sticky.color }"
        >
             <div 
                contenteditable="true"
                class="sticky-input" 
                @input="e => { sticky.content = e.target.innerHTML; debouncedSave() }"
                @blur="debouncedSave()"
                placeholder="Click to type here..."
                :ref="el => { if (el) textareaRefs[sticky.id] = el }"
                :style="{ 
                  textAlign: sticky.align
                }"
                v-html="sticky.content"
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
                             :key="c" 
                             class="color-swatch"
                             :style="{ backgroundColor: c }"
                             @click="sticky.color = c; debouncedSave()"
                           >
                             <i v-if="sticky.color === c" class="fa-solid fa-check text-[10px] text-white"></i>
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
  '#3F3F46', // Zinc Gray
  '#4C2B2D', // Deep Red
  '#4A314D', // Deep Purple
  '#5C4532', // Deep Bronze
  '#1B4D3E', // Forest Green
  '#1D4C5C', // Ocean Teal
  '#1E3A8A', // Dark Navy
  '#3B2E58', // Violet
  '#44403C', // Warm Stone
  '#365314', // Olive Green
  '#7C2D12', // Rust
  '#1E3A5F'  // Steel Blue
]

const stickies = ref([])
// Holds textarea DOM element refs keyed by sticky id
const textareaRefs = {}

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
  const available = backgroundColors.filter(c => c !== lastColor)
  const pool = available.length > 0 ? available : backgroundColors
  return pool[Math.floor(Math.random() * pool.length)]
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
  background: #0D0F11;
  color: #E4E4E7;
}

.st-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 24px;
  border-bottom: 1px solid #1E2025;
}
.st-left {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 14px;
  font-weight: 500;
}
.text-muted { color: #A1A1AA; }
.st-title { color: #E4E4E7; }

.st-right {
  display: flex;
  gap: 12px;
}
.plane-toolbar-btn {
  background: transparent;
  border: none;
  color: #D4D4D8;
  cursor: pointer;
  padding: 6px;
  border-radius: 4px;
  transition: background 0.2s;
}
.plane-toolbar-btn:hover { background: #1E2025; }
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

.sticky-card {
  border-radius: 6px;
  height: 280px;
  display: flex;
  flex-direction: column;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.4);
  transition: transform 0.2s, box-shadow 0.2s;
  animation: slideInUp 0.3s cubic-bezier(0.16, 1, 0.3, 1);
}
.sticky-card:hover { 
  box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.5);
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
  color: rgba(255, 255, 255, 0.95);
  padding: 16px;
  font-family: inherit;
  font-size: 14px;
  resize: none;
  line-height: 1.5;
}
.sticky-input::placeholder { color: rgba(255, 255, 255, 0.4); font-style: normal; font-weight: normal;}

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
  color: rgba(255,255,255,0.5);
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
.sf-btn:hover { color: white; background: rgba(255,255,255,0.1); }
.sf-btn.active { color: white; background: rgba(255,255,255,0.2); }
.trash:hover { color: #F87171; background: rgba(248, 113, 113, 0.1); }

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
  border-radius: 8px;
  cursor: pointer;
  transition: transform 0.1s;
  display: flex;
  align-items: center;
  justify-content: center;
}
.color-swatch:hover {
  transform: scale(1.1);
  box-shadow: 0 0 0 2px rgba(255,255,255,0.2)
}

:deep(.custom-swatch-popover) {
  background-color: #16181D !important;
  border: 1px solid #2D2F36 !important;
  border-radius: 8px !important;
}
</style>
