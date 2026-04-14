<script setup>
import { ref, onMounted } from 'vue'
import axiosClient from '@/api/axiosClient'
import { ElNotification, ElMessageBox } from 'element-plus'

const props = defineProps({
  projectId: { type: String, required: true }
})

const pages = ref([])
const activePage = ref(null)
const loading = ref(false)
const saving = ref(false)

let saveTimeout = null

onMounted(() => {
  fetchPages()
})

async function fetchPages() {
  loading.value = true
  try {
    const res = await axiosClient.get(`/projects/${props.projectId}/pages`)
    pages.value = res.data?.data || []
  } catch (error) {
    console.error(error)
  } finally {
    loading.value = false
  }
}

async function createPage() {
  try {
    const res = await axiosClient.post(`/projects/${props.projectId}/pages`, {
      title: 'Trang mới',
      content: ''
    })
    await fetchPages()
    openPage(res.data.data.id)
  } catch (error) {
    ElNotification({ title: 'Lỗi', message: 'Không thể tạo trang', type: 'error' })
  }
}

async function openPage(pageId) {
  loading.value = true
  try {
    const res = await axiosClient.get(`/projects/${props.projectId}/pages/${pageId}`)
    activePage.value = res.data?.data
  } catch (error) {
    ElNotification({ title: 'Lỗi', message: 'Không thể mở trang', type: 'error' })
  } finally {
    loading.value = false
  }
}

function handleContentInput() {
  if (saveTimeout) clearTimeout(saveTimeout)
  saving.value = true
  saveTimeout = setTimeout(() => {
    savePage()
  }, 1000)
}

async function savePage() {
  if (!activePage.value) return
  try {
    await axiosClient.put(`/projects/${props.projectId}/pages/${activePage.value.id}`, {
      title: activePage.value.title,
      content: activePage.value.content
    })
    const idx = pages.value.findIndex(p => p.id === activePage.value.id)
    if (idx !== -1) pages.value[idx].title = activePage.value.title
  } catch (error) {
    ElNotification({ title: 'Lỗi', message: 'Lỗi khi lưu trang', type: 'error' })
  } finally {
    saving.value = false
  }
}

async function archivePage(pageId) {
  try {
    await ElMessageBox.confirm('Bạn có chắc muốn lưu trữ trang này?', 'Xác nhận', { type: 'warning' })
    await axiosClient.put(`/projects/${props.projectId}/pages/${pageId}/archive`)
    if (activePage.value?.id === pageId) activePage.value = null
    fetchPages()
    ElNotification({ title: 'Thành công', message: 'Đã lưu trữ trang', type: 'success' })
  } catch (e) {
      if (e !== 'cancel') console.error(e)
  }
}
</script>

<template>
  <div class="plane-pages-wrapper">
    <div v-if="!activePage" class="pages-list-view">
      <!-- Header -->
      <div class="pages-header">
         <div class="ph-left">
            <!-- Empty left for spacing/consistency -->
         </div>
         <div class="ph-right">
            <button class="primary-action" @click="createPage">Add page</button>
         </div>
      </div>

      <!-- Navigation Tabs -->
      <div class="pages-nav">
         <div class="nav-tab active">Public</div>
         <div class="nav-tab">Private</div>
         <div class="nav-tab">Archived</div>
      </div>

      <!-- Toolbar -->
      <div class="pages-toolbar">
         <div class="pt-left"></div>
         <div class="pt-right">
            <button class="icon-toggle"><i class="fa-solid fa-magnifying-glass"></i></button>
            <button class="filter-btn"><i class="fa-solid fa-arrow-down-up-across-line"></i> Date modified</button>
            <button class="filter-btn"><i class="fa-solid fa-filter"></i> Filters</button>
         </div>
      </div>

      <!-- List -->
      <div class="pages-list" v-loading="loading">
         <div v-if="pages.length === 0" class="empty-state text-muted">No pages yet.</div>
         
         <div v-for="page in pages" :key="page.id" class="page-row" @click="openPage(page.id)">
            <div class="pr-left">
               <i class="fa-regular fa-file-lines doc-icon"></i>
               <span class="page-title">{{ page.title || 'Project Design Spec' }}</span>
            </div>
            <div class="pr-right">
               <div class="avatar-xxs bg-green">P</div>
               <button class="icon-btn"><i class="fa-solid fa-globe"></i></button>
               <button class="icon-btn"><i class="fa-solid fa-circle-info"></i></button>
               <button class="icon-btn"><i class="fa-regular fa-star"></i></button>
               <button class="icon-btn" @click.stop="archivePage(page.id)"><i class="fa-solid fa-ellipsis"></i></button>
            </div>
         </div>
         <!-- Static Mock Item mimicking image 5 -->
         <div class="page-row" v-if="pages.length === 0">
            <div class="pr-left">
               <i class="fa-regular fa-file-lines doc-icon"></i>
               <span class="page-title">Project Design Spec</span>
            </div>
            <div class="pr-right">
               <div class="avatar-xxs bg-green">P</div>
               <button class="icon-btn"><i class="fa-solid fa-globe"></i></button>
               <button class="icon-btn"><i class="fa-solid fa-circle-info"></i></button>
               <button class="icon-btn"><i class="fa-regular fa-star"></i></button>
               <button class="icon-btn"><i class="fa-solid fa-ellipsis"></i></button>
            </div>
         </div>
      </div>
    </div>
    
    <div v-else class="page-editor-view">
       <div class="editor-header">
         <el-button size="small" @click="activePage = null" class="dark-btn"><i class="fa-solid fa-arrow-left"></i> Back</el-button>
         <div class="save-status">
           <span v-if="saving" class="text-orange">Saving...</span>
           <span v-else class="text-green">Saved</span>
         </div>
       </div>

       <!-- Browser-like Editor Window -->
      <div class="editor-window">
        <!-- Editor structure remains intact to keep existing logic working -->
        <div class="editor-window-header">
          <div class="header-dots">
            <span></span><span></span><span></span>
          </div>
        </div>
        <div class="editor-toolbar-expanded">
          <div class="t-left">
            <div class="t-icon"><i class="fa-solid fa-bold"></i></div>
            <div class="t-icon"><i class="fa-solid fa-italic"></i></div>
            <div class="t-icon"><i class="fa-solid fa-list-ul"></i></div>
            <div class="t-icon"><i class="fa-solid fa-list-check"></i></div>
            <div class="t-icon"><i class="fa-solid fa-image"></i></div>
          </div>
        </div>
        <div class="editor-content-area">
          <input 
             class="editor-title-input" 
             v-model="activePage.title" 
             @input="handleContentInput"
             placeholder="Page title..." 
          />
          <textarea 
             class="editor-textarea" 
             v-model="activePage.content"
             @input="handleContentInput" 
             placeholder="Start typing..."
          ></textarea>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.plane-pages-wrapper {
  display: flex;
  flex-direction: column;
  height: 100%;
  color: #E4E4E7;
  font-family: 'Inter', sans-serif;
  background: #0D0F11;
  min-height: calc(100vh - 120px);
}

/* Header */
.pages-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 24px 0;
}
.primary-action {
  background: #0EA5E9;
  color: white;
  border: none;
  border-radius: 6px;
  padding: 6px 12px;
  font-size: 13px;
  cursor: pointer;
  font-weight: 500;
}
.primary-action:hover { background: #0284C7; }

/* Nav Tabs */
.pages-nav {
  display: flex;
  gap: 24px;
  padding: 0 24px;
  border-bottom: 1px solid #1E2025;
  margin-top: 12px;
}
.nav-tab {
  font-size: 14px;
  font-weight: 500;
  color: #A1A1AA;
  padding-bottom: 12px;
  cursor: pointer;
  border-bottom: 2px solid transparent;
  margin-bottom: -1px;
}
.nav-tab:hover { color: #E4E4E7; }
.nav-tab.active {
  color: #38BDF8;
  border-bottom: 2px solid #38BDF8;
}

/* Toolbar */
.pages-toolbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px 24px;
}
.pt-right {
  display: flex;
  align-items: center;
  gap: 12px;
}
.icon-toggle {
  background: transparent;
  border: none;
  color: #A1A1AA;
  cursor: pointer;
  font-size: 13px;
}
.icon-toggle:hover { color: #E4E4E7; }
.filter-btn {
  background: transparent;
  border: 1px solid #27272A;
  color: #E4E4E7;
  padding: 6px 12px;
  border-radius: 6px;
  font-size: 13px;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 6px;
}
.filter-btn:hover { background: #16181D; }

/* List */
.pages-list {
  display: flex;
  flex-direction: column;
  padding: 0 24px 24px;
}

.page-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px 16px;
  border-bottom: 1px solid #16181D;
  transition: background 0.2s;
  cursor: pointer;
}
.page-row:hover { background: #111315; }

.pr-left {
  display: flex;
  align-items: center;
  gap: 12px;
}
.doc-icon { color: #71717A; font-size: 14px; }
.page-title { font-size: 14px; font-weight: 500; color: #E4E4E7; }

.pr-right {
  display: flex;
  align-items: center;
  gap: 16px;
}
.avatar-xxs {
  width: 20px;
  height: 20px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 10px;
  font-weight: 600;
  color: white;
}
.bg-green { background-color: #10B981; }
.icon-btn {
  background: transparent;
  border: none;
  color: #71717A;
  font-size: 14px;
  cursor: pointer;
}
.icon-btn:hover { color: #E4E4E7; }

.text-muted { color: #A1A1AA; }
.empty-state { padding: 40px; text-align: center; }

/* Editor View Overrides */
.page-editor-view { padding: 24px; }
.editor-header { display: flex; justify-content: space-between; margin-bottom: 16px; }
.dark-btn { background: #16181D; border: 1px solid #27272A; color: #E4E4E7; }
.text-orange { color: #F59E0B; font-size: 13px; }
.text-green { color: #10B981; font-size: 13px; }

.editor-window {
  background: #111315;
  border-radius: 8px;
  border: 1px solid #1E2025;
  display: flex;
  flex-direction: column;
}

.editor-window-header {
  background: #0D0F11;
  height: 40px;
  display: flex;
  align-items: center;
  padding: 0 16px;
  border-bottom: 1px solid #1E2025;
}

.header-dots { display: flex; gap: 6px; }
.header-dots span { width: 10px; height: 10px; border-radius: 50%; background: #ff5f56; }
.header-dots span:nth-child(2) { background: #ffbd2e; }
.header-dots span:nth-child(3) { background: #27c93f; }

.editor-toolbar-expanded {
  display: flex;
  padding: 8px 16px;
  background: #16181D;
  border-bottom: 1px solid #1E2025;
}

.t-left { display: flex; gap: 12px; }
.t-icon { color: #A1A1AA; cursor: pointer; }
.t-icon:hover { color: #E4E4E7; }

.editor-content-area {
  padding: 40px;
  display: flex;
  flex-direction: column;
  min-height: 500px;
}

.editor-title-input {
  background: transparent;
  border: none;
  font-size: 32px;
  font-weight: 700;
  color: #E4E4E7;
  outline: none;
  margin-bottom: 20px;
}
.editor-title-input::placeholder { color: #71717A; }

.editor-textarea {
  background: transparent;
  border: none;
  font-size: 16px;
  color: #D4D4D8;
  outline: none;
  width: 100%;
  flex: 1;
  resize: none;
  line-height: 1.6;
}
.editor-textarea::placeholder { color: #71717A; }
</style>
