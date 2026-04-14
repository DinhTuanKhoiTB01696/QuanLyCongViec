<template>
  <NexusLayout>
    <div class="drafts-wrapper">
      <!-- Header -->
      <header class="dr-header">
        <div class="dr-left">
          <i class="fa-solid fa-pen-nib text-muted"></i>
          <span class="dr-title">Drafts</span>
          <span class="dr-badge">2</span>
        </div>
        <div class="dr-right">
          <button class="plane-primary-btn" @click="showModal = true">Draft a work item</button>
        </div>
      </header>

      <!-- Body / List -->
      <div class="dr-body">
         <div class="list-row">
            <div class="lr-left">
               <span class="lr-id">CUN</span>
               <span class="lr-title">u</span>
            </div>
            <div class="lr-right">
               <div class="lr-badge"><i class="fa-regular fa-circle text-muted"></i> Backlog</div>
               <div class="lr-badge"><i class="fa-solid fa-ban text-muted"></i></div>
               <div class="lr-badge"><i class="fa-solid fa-tag"></i></div>
               <div class="lr-badge"><i class="fa-solid fa-layer-group"></i></div>
               <div class="lr-badge"><i class="fa-solid fa-user-group"></i></div>
               <div class="lr-badge"><i class="fa-regular fa-calendar"></i></div>
               <div class="lr-badge"><i class="fa-solid fa-ellipsis"></i></div>
            </div>
         </div>

         <div class="list-row">
            <div class="lr-left">
               <span class="lr-id">CUN</span>
               <span class="lr-title">dev</span>
            </div>
            <div class="lr-right">
               <div class="lr-badge orange"><i class="fa-solid fa-circle-half-stroke"></i> In Progress</div>
               <div class="lr-badge"><i class="fa-solid fa-ban text-muted"></i></div>
               <div class="lr-badge"><i class="fa-solid fa-tag"></i></div>
               <div class="lr-badge"><i class="fa-solid fa-layer-group"></i></div>
               <div class="lr-badge"><i class="fa-solid fa-user-group"></i></div>
               <div class="lr-badge"><i class="fa-regular fa-calendar"></i></div>
               <div class="lr-badge"><i class="fa-solid fa-ellipsis"></i></div>
            </div>
         </div>
      </div>
    </div>

    <!-- Create Draft Modal -->
    <div class="modal-overlay" v-if="showModal" @click.self="showModal = false">
       <div class="draft-modal">
          <h3 class="modal-title">Create a draft</h3>
          
          <div class="proj-badge mt-4">
             <i class="fa-solid fa-bell" style="color: #F59E0B"></i>
             <span>Cun</span>
          </div>
          
          <input type="text" class="dm-title-input mt-4" placeholder="Title" />
          <textarea class="dm-desc-input mt-2" placeholder="Click to add description"></textarea>
          
          <div class="dm-toolbar mt-2">
             <button class="dm-tool-btn"><i class="fa-regular fa-circle text-muted"></i> Backlog</button>
             <button class="dm-tool-btn"><i class="fa-solid fa-ban text-muted"></i> None</button>
             <button class="dm-tool-btn"><i class="fa-solid fa-user-group text-muted"></i> Assignees</button>
             <button class="dm-tool-btn"><i class="fa-solid fa-tag text-muted"></i> Labels</button>
             <button class="dm-tool-btn"><i class="fa-regular fa-calendar text-muted"></i> Start date</button>
             <button class="dm-tool-btn"><i class="fa-solid fa-calendar-day text-muted"></i> Due date</button>
             <button class="dm-tool-btn"><i class="fa-solid fa-arrows-spin text-muted"></i> Cycle</button>
             <button class="dm-tool-btn"><i class="fa-solid fa-table-cells-large text-muted"></i> Modules</button>
             <button class="dm-tool-btn">Add parent</button>
          </div>
          
          <div class="dm-footer mt-4">
             <div class="dm-footer-left">
                <label class="toggle-wrap">
                   <div class="toggle-bg"><div class="toggle-knob"></div></div>
                   Create more
                </label>
             </div>
             <div class="dm-footer-right">
                <button class="discard-btn" @click="showModal = false">Discard</button>
                <button class="save-btn" @click="showModal = false">Save to Drafts</button>
             </div>
          </div>
       </div>
    </div>
  </NexusLayout>
</template>

<script setup>
import { ref } from 'vue'
import NexusLayout from '@/components/layout/NexusLayout.vue'

const showModal = ref(false)
</script>

<style scoped>
.drafts-wrapper {
  display: flex;
  flex-direction: column;
  height: 100vh;
  background: #0D0F11;
  color: #E4E4E7;
}

.dr-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 24px;
  border-bottom: 1px solid #1E2025;
}
.dr-left {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 14px;
  font-weight: 500;
}
.text-muted { color: #A1A1AA; }
.dr-title { color: #E4E4E7; }
.dr-badge {
  background: #1E2025;
  color: #0EA5E9;
  font-size: 11px;
  padding: 2px 6px;
  border-radius: 12px;
  font-weight: 600;
}

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

/* List Styles similar to YourWorks */
.dr-body { padding: 0 24px; }
.list-row { display: flex; justify-content: space-between; align-items: center; padding: 12px 0; border-bottom: 1px solid #1E2025; transition: background 0.2s; cursor: pointer; }
.list-row:hover { background: #16181D; }
.lr-left { display: flex; align-items: center; gap: 16px; }
.lr-id { font-size: 12px; color: #71717A; min-width: 40px; }
.lr-title { font-size: 13px; font-weight: 500; color: #E4E4E7; }
.lr-right { display: flex; align-items: center; gap: 6px; }

.lr-badge { border: 1px solid #27272A; border-radius: 4px; padding: 4px 8px; font-size: 12px; color: #A1A1AA; display: flex; align-items: center; gap: 6px; }
.lr-badge.orange { border-color: #78350F; background: rgba(245, 158, 11, 0.1); color: #F59E0B; }
.lr-badge i { font-size: 11px; }


/* Modal Styles */
.modal-overlay {
  position: fixed;
  top: 0; left: 0; right: 0; bottom: 0;
  background: rgba(0,0,0,0.6);
  z-index: 9999;
  display: flex;
  align-items: center;
  justify-content: center;
}
.draft-modal {
  background: #111315;
  border: 1px solid #1E2025;
  width: 700px;
  border-radius: 8px;
  padding: 24px;
  box-shadow: 0 20px 40px rgba(0,0,0,0.5);
}

.modal-title { margin: 0; font-size: 16px; font-weight: 600; color: #E4E4E7; }
.mt-4 { margin-top: 24px; }
.mt-2 { margin-top: 12px; }

.proj-badge { display: inline-flex; align-items: center; gap: 8px; font-size: 12px; font-weight: 500; background: #16181D; padding: 4px 8px; border-radius: 4px; border: 1px solid #27272A; }

.dm-title-input {
  width: 100%;
  background: #16181D;
  border: 1px solid #27272A;
  color: #E4E4E7;
  padding: 12px 16px;
  border-radius: 6px;
  font-size: 14px;
  outline: none;
}
.dm-title-input::placeholder { color: #71717A; }
.dm-title-input:focus { border-color: #38BDF8; }

.dm-desc-input {
  width: 100%;
  background: #16181D;
  border: 1px solid #27272A;
  color: #E4E4E7;
  padding: 12px 16px;
  border-radius: 6px;
  font-size: 14px;
  outline: none;
  height: 120px;
  resize: none;
  font-family: inherit;
}
.dm-desc-input::placeholder { color: #71717A; }
.dm-desc-input:focus { border-color: #38BDF8; }

.dm-toolbar {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}
.dm-tool-btn {
  background: transparent;
  border: 1px solid #27272A;
  color: #A1A1AA;
  font-size: 12px;
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 4px 8px;
  border-radius: 4px;
  cursor: pointer;
}
.dm-tool-btn:hover { background: #1E2025; color: #E4E4E7; }

.dm-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-top: 1px solid #1E2025;
  padding-top: 16px;
}
.toggle-wrap { display: flex; align-items: center; gap: 8px; font-size: 12px; color: #A1A1AA; cursor: pointer; }
.toggle-bg { width: 32px; height: 18px; background: #27272A; border-radius: 9px; position: relative; }
.toggle-knob { width: 14px; height: 14px; background: #71717A; border-radius: 50%; position: absolute; top: 2px; left: 2px; }

.dm-footer-right { display: flex; gap: 12px; }
.discard-btn { background: transparent; border: none; color: #A1A1AA; font-size: 13px; font-weight: 500; cursor: pointer; }
.discard-btn:hover { color: #E4E4E7; }
.save-btn { background: #0EA5E9; color: white; border: none; border-radius: 6px; padding: 6px 16px; font-size: 13px; font-weight: 500; cursor: pointer; }
.save-btn:hover { background: #0284C7; }
</style>
