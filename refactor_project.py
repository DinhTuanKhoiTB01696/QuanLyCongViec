import re

file_path = r'c:\Users\tua46\OneDrive\Máy tính\DATN\QuanLyCongViec\Frontend\src\views\HomeSite\Projects\ProjectDetail.vue'

with open(file_path, 'r', encoding='utf-8') as f:
    content = f.read()

# 1. Imports
imports = """import EntityDetailLayout from '@/components/common/Entity/EntityDetailLayout.vue'
import EntityDetailHeader from '@/components/common/Entity/EntityDetailHeader.vue'
import EntityTabs from '@/components/common/Entity/EntityTabs.vue'
import EntityRightSidebar from '@/components/common/Entity/EntityRightSidebar.vue'
import EntityPropertyRow from '@/components/common/Entity/EntityPropertyRow.vue'
import RichTextEditor"""
content = content.replace("import RichTextEditor", imports)

# 2. Wrapper
content = content.replace('<div class="project-detail-wrapper" v-if="project">', '<EntityDetailLayout v-if="project">\n    <template #header>')

# 3. Header
header_old = re.search(r'    <!-- Entity Header -->\s*<header class="project-header">.*?</header>\s*<!-- Main Content Grid -->\s*<div class="project-content-grid">\s*<!-- Left Column: Main Information -->\s*<div class="main-column">', content, re.DOTALL)

if header_old:
    header_new = """      <EntityDetailHeader
        :title="project.title"
        :icon="project.icon || '😎'"
      >
        <template #actions>
          <span class="status-badge status-on-track" style="margin-right: 8px; font-size: 11px; padding: 2px 6px; border-radius: 3px; font-weight: bold;">
            ĐÚNG TIẾN ĐỘ <ChevronDown class="w-4 h-4 ms-1"></ChevronDown>
          </span>
          <div style="margin-right: 8px; display: inline-flex; align-items: center; border: 1px solid #DFE1E6; padding: 4px 8px; border-radius: 3px; font-size: 12px; color: #42526E; background: white; font-weight: 500;">
            <Calendar class="w-4 h-4" style="margin-right: 4px;"></Calendar> 15 thg 6 <ChevronDown class="w-4 h-4 ms-1"></ChevronDown>
          </div>
          <button class="sprinta-btn-secondary">Đang theo dõi</button>
          <button class="sprinta-btn-secondary" @click="isShareModalOpen = true">
            <i class="fa-solid fa-share-nodes"></i> Chia sẻ
          </button>
          <button class="sprinta-icon-btn"><i class="fa-solid fa-link"></i></button>
          <button class="sprinta-icon-btn"><MoreHorizontal class="w-4 h-4"></MoreHorizontal></button>
        </template>
        <template #tabs>
          <EntityTabs 
            v-model="currentTab" 
            :tabs="[
              { label: 'Giới thiệu', value: 'overview' },
              { label: 'Cập nhật', value: 'updates' },
              { label: 'Bài học rút ra', value: 'learnings' },
              { label: 'Rủi ro', value: 'risks' },
              { label: 'Quyết định', value: 'decisions' }
            ]" 
          />
        </template>
      </EntityDetailHeader>
    </template>
    
    <template #main>"""
    content = content.replace(header_old.group(0), header_new)

# 4. Sidebar Start
sidebar_start_old = re.search(r'      </div>\s*<!-- Right Column: Sidebar Details -->\s*<div class="side-column">\s*<div class="details-body">', content, re.DOTALL)
if sidebar_start_old:
    sidebar_start_new = """    </template>

    <template #sidebar>
      <EntityRightSidebar>"""
    content = content.replace(sidebar_start_old.group(0), sidebar_start_new)

# 5. Sidebar End & wrapper close
end_old = re.search(r'          <!-- Ngày bắt đầu -->.*?</div>\s*</div>\s*</div>\s*<!-- Share Modal -->', content, re.DOTALL)
if end_old:
    end_new = """          <!-- Ngày bắt đầu -->
          <EntityPropertyRow label="Ngày bắt đầu">
            <template #value>
              <span class="empty-value">15 Jun 2026</span>
            </template>
          </EntityPropertyRow>
      </EntityRightSidebar>
    </template>
  </EntityDetailLayout>

    <!-- Share Modal -->"""
    content = content.replace(end_old.group(0), end_new)

# Write back
with open(file_path, 'w', encoding='utf-8') as f:
    f.write(content)

print("Refactored ProjectDetail.vue")
