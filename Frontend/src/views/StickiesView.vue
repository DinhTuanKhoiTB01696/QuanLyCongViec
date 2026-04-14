<template>
  <NexusLayout>
    <div class="stickies-wrapper">
      <header class="st-header">
        <div class="st-left">
          <i class="fa-solid fa-note-sticky text-muted"></i>
          <span class="st-title">Stickies</span>
        </div>
        <div class="st-right">
          <button class="plane-toolbar-btn"><i class="fa-solid fa-magnifying-glass"></i></button>
          <button class="plane-primary-btn">Add sticky</button>
        </div>
      </header>

      <div class="st-body">
        <div class="stickies-grid">
          
          <!-- Sticky 1: Red -->
          <div class="sticky-card" style="background-color: #4C2B2D;">
             <textarea class="sticky-input" placeholder="Click to type here"></textarea>
             <div class="sticky-footer">
                <div class="sf-left">
                   <button class="sf-btn"><i class="fa-solid fa-palette"></i></button>
                   <button class="sf-btn"><i class="fa-solid fa-bold"></i></button>
                   <button class="sf-btn"><i class="fa-solid fa-italic"></i></button>
                   <button class="sf-btn"><i class="fa-solid fa-align-left"></i></button>
                </div>
                <button class="sf-btn trash"><i class="fa-solid fa-trash-can"></i></button>
             </div>
          </div>

          <!-- Sticky 2: Purple -->
          <div class="sticky-card" style="background-color: #3B2E58;">
             <textarea class="sticky-input">sadawf</textarea>
             <div class="sticky-footer">
                <div class="sf-left">
                   <button class="sf-btn"><i class="fa-solid fa-palette"></i></button>
                   <button class="sf-btn"><i class="fa-solid fa-bold"></i></button>
                   <button class="sf-btn"><i class="fa-solid fa-italic"></i></button>
                   <button class="sf-btn"><i class="fa-solid fa-align-left"></i></button>
                </div>
                <button class="sf-btn trash"><i class="fa-solid fa-trash-can"></i></button>
             </div>
          </div>

          <!-- Sticky 3: Blue -->
          <div class="sticky-card" style="background-color: #1E4B5B;">
             <textarea class="sticky-input">dsa</textarea>
             <div class="sticky-footer">
                <div class="sf-left">
                   <button class="sf-btn"><i class="fa-solid fa-palette"></i></button>
                   <button class="sf-btn"><i class="fa-solid fa-bold"></i></button>
                   <button class="sf-btn"><i class="fa-solid fa-italic"></i></button>
                   <button class="sf-btn"><i class="fa-solid fa-align-left"></i></button>
                </div>
                <button class="sf-btn trash"><i class="fa-solid fa-trash-can"></i></button>
             </div>
          </div>

          <!-- Sticky 4: Green -->
          <div class="sticky-card" style="background-color: #1B4D3E;">
             <textarea class="sticky-input">fdsadasd</textarea>
             <div class="sticky-footer">
                <div class="sf-left">
                   <button class="sf-btn"><i class="fa-solid fa-palette"></i></button>
                   <button class="sf-btn"><i class="fa-solid fa-bold"></i></button>
                   <button class="sf-btn"><i class="fa-solid fa-italic"></i></button>
                   <button class="sf-btn"><i class="fa-solid fa-align-left"></i></button>
                </div>
                <button class="sf-btn trash"><i class="fa-solid fa-trash-can"></i></button>
             </div>
          </div>

        </div>
      </div>
    </div>
  </NexusLayout>
</template>

<script setup>
import NexusLayout from '@/components/layout/NexusLayout.vue'
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
}

.sticky-card {
  border-radius: 4px;
  height: 280px;
  display: flex;
  flex-direction: column;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.5);
  transition: transform 0.2s;
}
.sticky-card:hover {  }

.sticky-input {
  flex: 1;
  background: transparent;
  border: none;
  outline: none;
  color: rgba(255, 255, 255, 0.9);
  padding: 16px;
  font-family: inherit;
  font-size: 14px;
  resize: none;
}
.sticky-input::placeholder { color: rgba(255, 255, 255, 0.5); }

.sticky-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px 16px;
  opacity: 0.6;
  transition: opacity 0.2s;
}
.sticky-card:hover .sticky-footer { opacity: 1; }

.sf-left { display: flex; gap: 8px; }
.sf-btn {
  background: transparent;
  border: none;
  color: rgba(255,255,255,0.7);
  cursor: pointer;
  padding: 4px;
  font-size: 13px;
}
.sf-btn:hover { color: white; }
.trash:hover { color: #F87171; }
</style>
