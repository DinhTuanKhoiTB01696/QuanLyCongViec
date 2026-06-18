<template>
  <div class="project-detail-wrapper" v-if="project">
    <!-- Module Header (matching the list view) -->
    <header class="module-header">
      <div class="header-content">
        <h1>Dự án</h1>
        <div class="header-actions">
          <button class="primary-btn">Tạo dự án</button>
        </div>
      </div>
      
      <div class="tabs-nav">
        <router-link to="/home/projects" class="tab-link">Tất cả dự án</router-link>
        <router-link to="/home/projects" class="tab-link">Đang theo dõi</router-link>
        <router-link to="/home/projects" class="tab-link">Đã lưu trữ</router-link>
      </div>
    </header>

    <!-- Entity Header -->
    <header class="project-header">
      <div class="project-header-inner">
        <!-- Entity Main info -->
        <div class="header-main">
          <div class="title-block">
            <div class="project-emoji-large">{{ project.icon || '😎' }}</div>
            <h1>{{ project.title }}</h1>
          </div>
          
          <div class="header-actions">
            <span class="status-badge status-on-track">
              ĐÚNG TIẾN ĐỘ <ChevronDown class="w-4 h-4 ms-1"></ChevronDown>
            </span>
            <div class="target-date-badge">
              <Calendar class="w-4 h-4"></Calendar> 15 thg 6 <ChevronDown class="w-4 h-4 ms-1"></ChevronDown>
            </div>
            <button class="secondary-btn">
              Đang theo dõi
            </button>
            <button class="secondary-btn" @click="isShareModalOpen = true">
              <i class="fa-solid fa-share-nodes"></i> Chia sẻ
            </button>
            <button class="icon-btn-header"><i class="fa-solid fa-link"></i></button>
            <button class="icon-btn-header"><MoreHorizontal class="w-4 h-4"></MoreHorizontal></button>
          </div>
        </div>
      </div>
      
      <!-- Tabs -->
      <div class="project-tabs-container">
        <div class="project-tabs">
          <button class="tab-btn" :class="{ active: currentTab === 'overview' }" @click="currentTab = 'overview'">Giới thiệu</button>
          <button class="tab-btn" :class="{ active: currentTab === 'updates' }" @click="currentTab = 'updates'">Cập nhật <span class="tab-badge">1</span></button>
          <button class="tab-btn" :class="{ active: currentTab === 'learnings' }" @click="currentTab = 'learnings'">Bài học rút ra</button>
          <button class="tab-btn" :class="{ active: currentTab === 'risks' }" @click="currentTab = 'risks'">Rủi ro</button>
          <button class="tab-btn" :class="{ active: currentTab === 'decisions' }" @click="currentTab = 'decisions'">Quyết định</button>
        </div>
      </div>
    </header>

    <!-- Main Content Grid -->
    <div class="project-content-grid">
      <!-- Left Column: Main Information -->
      <div class="main-column">
        
        <!-- Tab: Giới thiệu -->
        <template v-if="currentTab === 'overview'">
          <section class="content-section">
            <h4>Dự án chúng ta đang thực hiện</h4>
            <div v-if="!editing.description" @click="editing.description = true" class="editable-placeholder">
              <p v-if="project.description">{{ project.description }}</p>
              <p v-else class="empty-text">Mô tả dự án này và công việc liên quan đến dự án.</p>
            </div>
            <RichTextEditor v-else v-model="project.description" @save="editing.description = false" @cancel="editing.description = false" placeholder="Mô tả dự án này và công việc liên quan đến dự án." />
          </section>

          <section class="content-section">
            <h4>Lý do thực hiện dự án</h4>
            <div v-if="!editing.reason" @click="editing.reason = true" class="editable-placeholder">
              <p v-if="project.reason">{{ project.reason }}</p>
              <p v-else class="empty-text">Giải thích lý do vì sao công việc này được thực hiện và những lý do thúc đẩy công việc.</p>
            </div>
            <RichTextEditor v-else v-model="project.reason" @save="editing.reason = false" @cancel="editing.reason = false" placeholder="Giải thích lý do vì sao công việc này được thực hiện và những lý do thúc đẩy công việc." />
          </section>

          <section class="content-section">
            <h4>Cách để biết rằng chúng ta đã thành công</h4>
            <div v-if="!editing.success" @click="editing.success = true" class="editable-placeholder">
              <p v-if="project.success">{{ project.success }}</p>
              <p v-else class="empty-text">Mô tả thành công và mục tiêu bạn hy vọng đạt được.</p>
            </div>
            <RichTextEditor v-else v-model="project.success" @save="editing.success = false" @cancel="editing.success = false" placeholder="Mô tả thành công và mục tiêu bạn hy vọng đạt được." />
          </section>

          <section class="content-section">
            <h4>Nhận xét</h4>
            <div class="comment-input-mockup">
              <div class="user-avatar-current">T</div>
              <div class="fake-input">Thêm nhận xét... tham gia cuộc hội thoại</div>
            </div>
          </section>
        </template>

        <!-- Tab: Cập nhật -->
        <template v-if="currentTab === 'updates'">
          <div class="updates-header">
            <h4>Lịch sử dự án</h4>
            <span class="last-update-text">Lần cập nhật gần nhất: khoảng 14 giờ trước</span>
          </div>

          <div class="timeline-visual">
            <div class="timeline-line"></div>
            <div class="timeline-node current">
              <User class="w-4 h-4"></User>
              <span>Tuần này</span>
            </div>
          </div>

          <div class="update-editor-box">
            <div class="update-editor-header">
              <div class="editor-field">
                <label>Trạng thái hiện tại</label>
                <div class="status-dropdown-wrapper">
                  <span class="status-badge status-on-track" @click="showStatusMenu = !showStatusMenu" style="cursor: pointer;">
                    ĐÚNG TIẾN ĐỘ <ChevronDown class="w-4 h-4 ms-1"></ChevronDown>
                  </span>
                  <!-- Mock Status Dropdown -->
                  <div class="status-dropdown-menu" v-if="showStatusMenu">
                    <div class="status-option"><span class="status-badge status-pending">ĐANG CHỜ XỬ LÝ</span></div>
                    <div class="status-option"><span class="status-badge status-on-track">ĐÚNG TIẾN ĐỘ</span></div>
                    <div class="status-option"><span class="status-badge status-at-risk">CÓ RỦI RO</span></div>
                    <div class="status-option"><span class="status-badge status-off-track">KHÔNG ĐÚNG TIẾN ĐỘ</span></div>
                    <div class="status-option"><span class="status-badge status-done">ĐÃ HOÀN TẤT <i class="fa-solid fa-flag ms-1"></i></span></div>
                    <div class="divider"></div>
                    <div class="status-option text-option">ĐÃ TẠM DỪNG</div>
                    <div class="status-option text-option">ĐÃ HỦY</div>
                  </div>
                </div>
              </div>
              <div class="editor-field">
                <label>Ngày mục tiêu</label>
                <div class="target-date-badge date-dropdown">
                  <Calendar class="w-4 h-4"></Calendar> 15 thg 6 <ChevronDown class="w-4 h-4 ms-1"></ChevronDown>
                </div>
              </div>
              <div class="editor-field template-link">
                <span>Mẫu <ChevronDown class="w-4 h-4 ms-1"></ChevronDown></span>
              </div>
            </div>
            
            <div class="update-editor-body">
              <textarea placeholder="Viết bản cập nhật gồm tối đa 280 ký tự. Nhập '/gần nhất' để sao chép bản cập nhật gần đây nhất, còn nhập / để thêm thành phần khác" rows="4"></textarea>
            </div>
            
            <div class="update-editor-footer">
              <div class="editor-tools">
                <span class="tool-ai"><i class="fa-solid fa-wand-magic-sparkles"></i> Soạn thảo bằng Rovo</span>
                <button class="tool-btn"><Plus class="w-4 h-4"></Plus></button>
                <button class="tool-btn"><i class="fa-solid fa-image"></i></button>
                <button class="tool-btn"><i class="fa-solid fa-at"></i></button>
                <button class="tool-btn"><i class="fa-solid fa-link"></i></button>
              </div>
              <div class="editor-actions">
                <span class="char-count">0/280 <i class="fa-solid fa-circle-question"></i></span>
                <button class="secondary-btn"><User class="w-4 h-4"></User> 1</button>
                <button class="secondary-btn">Lưu bản nháp</button>
                <button class="primary-btn">Đăng bản cập nhật</button>
              </div>
            </div>
          </div>

          <div class="timeline-posts">
            <h4 class="timeline-period">Tuần này</h4>
            
            <div class="timeline-post">
              <div class="post-header">
                <div class="post-user">
                  <div class="user-avatar-current">T</div>
                  <div class="user-info">
                    <span class="user-name">Tua20000</span>
                    <span class="post-time">khoảng 14 giờ trước</span>
                  </div>
                </div>
                <div class="post-status-meta">
                  <span class="status-badge status-on-track">ĐÚNG TIẾN ĐỘ</span> cho <div class="target-date-badge"><Calendar class="w-4 h-4"></Calendar> 15 thg 6</div>
                </div>
              </div>
              
              <div class="post-content">
                <p>qr</p>
                <div class="status-change-log">
                  Đã thay đổi trạng thái <span class="status-badge status-pending mx-1">ĐANG CHỜ XỬ LÝ</span> <ArrowRight class="w-4 h-4 mx-1"></ArrowRight> <span class="status-badge status-on-track mx-1">ĐÚNG TIẾN ĐỘ</span>
                </div>
              </div>
              
              <div class="post-actions">
                <span>Chia sẻ • Sửa • Xóa •</span>
                <button class="reaction-btn">👍</button>
                <button class="reaction-btn">👏</button>
                <button class="reaction-btn">🎉</button>
                <button class="reaction-btn">❤️</button>
                <button class="reaction-btn"><MoreHorizontal class="w-4 h-4"></MoreHorizontal></button>
                <button class="reaction-btn"><Target class="w-4 h-4"></Target></button>
              </div>
              
              <div class="comment-input-mockup mt-16">
                <div class="user-avatar-current">T</div>
                <div class="fake-input">Thêm nhận xét... hỏi nhóm có cần trợ giúp không</div>
              </div>
            </div>
          </div>
        </template>

        <!-- Tab: Bài học rút ra -->
        <template v-if="currentTab === 'learnings'">
          <div v-if="!editing.learnings && !project.learnings" class="empty-state-large-tab">
            <div class="empty-illustration">
              <i class="fa-solid fa-lightbulb" style="color: #0052CC; font-size: 64px;"></i>
            </div>
            <div class="empty-text-content">
              <h4>Những bộ óc vĩ đại có tư duy giống nhau sẽ chia sẻ kiến thức của họ</h4>
              <p>Chia sẻ những gì bạn đã học được với công ty của bạn để giúp những người khác có một khởi đầu thuận lợi khi làm việc trên các dự án tương tự.</p>
              <div class="empty-actions">
                <button class="secondary-btn" @click="editing.learnings = true">Thêm bài học rút ra mới</button>
                <a href="#" class="link-btn">Xem ví dụ</a>
              </div>
            </div>
          </div>
          <div v-else>
            <RichTextEditor v-model="project.learnings" @save="editing.learnings = false" @cancel="editing.learnings = false" v-if="editing.learnings" placeholder="Nhập bài học rút ra..." />
            <div v-else @click="editing.learnings = true" class="editable-placeholder">
              <p>{{ project.learnings }}</p>
            </div>
          </div>
        </template>

        <!-- Tab: Rủi ro -->
        <template v-if="currentTab === 'risks'">
          <div v-if="!editing.risks && !project.risks" class="empty-state-large-tab">
            <div class="empty-illustration">
              <i class="fa-solid fa-triangle-exclamation" style="color: #0052CC; font-size: 64px;"></i>
            </div>
            <div class="empty-text-content">
              <h4>Nắm bắt các rủi ro đã biết</h4>
              <p>Theo dõi rủi ro và đảm bảo rằng các bên liên quan của bạn không bất ngờ nếu điều tồi tệ nhất xảy ra trong dự án này.</p>
              <div class="empty-actions">
                <button class="secondary-btn" @click="editing.risks = true">Thêm rủi ro mới</button>
              </div>
            </div>
          </div>
          <div v-else>
            <RichTextEditor v-model="project.risks" @save="editing.risks = false" @cancel="editing.risks = false" v-if="editing.risks" placeholder="Nhập rủi ro..." />
            <div v-else @click="editing.risks = true" class="editable-placeholder">
              <p>{{ project.risks }}</p>
            </div>
          </div>
        </template>

        <!-- Tab: Quyết định -->
        <template v-if="currentTab === 'decisions'">
          <div v-if="!editing.decisions && !project.decisions" class="empty-state-large-tab">
            <div class="empty-illustration">
              <GitBranch class="w-4 h-4" style="color: #0052CC; font-size: 64px;"></GitBranch>
            </div>
            <div class="empty-text-content">
              <h4>Truyền đạt các quyết định lớn</h4>
              <p>Ghi lại các quyết định lớn cho dự án này tại đây để chia sẻ trong bản cập nhật mới nhất của bạn.</p>
              <div class="empty-actions">
                <button class="secondary-btn" @click="editing.decisions = true">Thêm quyết định mới</button>
              </div>
            </div>
          </div>
          <div v-else>
            <RichTextEditor v-model="project.decisions" @save="editing.decisions = false" @cancel="editing.decisions = false" v-if="editing.decisions" placeholder="Nhập quyết định..." />
            <div v-else @click="editing.decisions = true" class="editable-placeholder">
              <p>{{ project.decisions }}</p>
            </div>
          </div>
        </template>

      </div>

      <!-- Right Column: Sidebar Details -->
      <div class="side-column">
        <div class="details-body">
          <!-- Chủ sở hữu -->
          <div class="detail-row">
            <div class="detail-label">Chủ sở hữu</div>
            <div class="detail-value">
              <div class="owner-chip">
                <div class="owner-avatar-micro">T</div>
                <span class="owner-name">Tua20000</span>
              </div>
            </div>
          </div>
          
          <!-- Người đóng góp -->
          <div class="detail-row">
            <div class="detail-label">Người đóng góp <span class="badge-count">1</span> <button class="icon-btn-micro"><Plus class="w-4 h-4"></Plus></button></div>
            <div class="detail-value">
              <div class="owner-chip">
                <div class="owner-avatar-micro">T</div>
                <div class="owner-info">
                  <span class="owner-name">Tua20000</span>
                  <span class="owner-role">Lập trình viên</span>
                </div>
              </div>
            </div>
          </div>

          <!-- Người theo dõi -->
          <div class="detail-row">
            <div class="detail-label">Người theo dõi <button class="icon-btn-micro" @click="isShareModalOpen = true"><Plus class="w-4 h-4"></Plus></button></div>
            <div class="detail-value flex-between">
              <span class="empty-value" style="cursor: pointer;" @click="isShareModalOpen = true">Thêm người theo dõi</span>
              <div class="follower-icons">
                <i class="fa-brands fa-slack ms-1"></i>
                <i class="fa-brands fa-microsoft ms-1"></i>
              </div>
            </div>
          </div>

          <!-- Đóng góp vào mục tiêu -->
          <div class="detail-row relative-popover-container">
            <div class="detail-label">Đóng góp vào mục tiêu <button class="icon-btn-micro" @click.stop="togglePopover('goal')"><Plus class="w-4 h-4"></Plus></button></div>
            
            <div class="detail-value" v-if="linkedGoals.length > 0">
              <div class="linked-item" v-for="g in linkedGoals" :key="g.id">
                <Target class="w-4 h-4 item-icon"></Target>
                <span class="item-name">{{ g.name }}</span>
                <button class="remove-btn" @click="removeGoal(g.id)"><X class="w-4 h-4"></X></button>
              </div>
            </div>

            <!-- Goal Popover -->
            <div class="custom-popover" v-if="popovers.goal" @click.stop>
              <input type="text" class="popover-search" placeholder="Tìm kiếm mục tiêu hoặc dán liên kết" v-model="searchQueries.goal" />
              <div class="popover-list-title">Mục tiêu gần đây</div>
              <div class="popover-list">
                <div class="popover-item" v-for="g in mockGoalsList" :key="g.id" @click="addGoal(g)">
                  <Target class="w-4 h-4 item-icon-muted"></Target>
                  <div class="item-details">
                    <div class="item-name">{{ g.name }}</div>
                    <div class="item-meta">{{ g.owner }}</div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Các dự án liên quan -->
          <div class="detail-row relative-popover-container">
            <div class="detail-label">Các dự án liên quan <button class="icon-btn-micro" @click.stop="togglePopover('project')"><Plus class="w-4 h-4"></Plus></button></div>
            
            <div class="detail-value" v-if="linkedProjects.length > 0">
              <div class="linked-item" v-for="p in linkedProjects" :key="p.id">
                <span class="item-icon">{{ p.icon }}</span>
                <span class="item-name">{{ p.name }}</span>
                <button class="remove-btn" @click="removeProject(p.id)"><X class="w-4 h-4"></X></button>
              </div>
            </div>

            <!-- Project Popover -->
            <div class="custom-popover" v-if="popovers.project" @click.stop>
              <div class="popover-select-mock">Dự án liên quan đến <ChevronDown class="w-4 h-4 ms-auto"></ChevronDown></div>
              <input type="text" class="popover-search mt-2" placeholder="Tìm kiếm dự án" v-model="searchQueries.project" />
              <div class="popover-list mt-2">
                <div class="popover-item" v-for="p in mockProjectsList" :key="p.id" @click="addProject(p)">
                  <span class="item-icon-muted">{{ p.icon }}</span>
                  <div class="item-name">{{ p.name }}</div>
                </div>
              </div>
            </div>
          </div>

          <!-- Công việc được theo dõi ở đâu? -->
          <div class="detail-row relative-popover-container">
            <div class="detail-label">Công việc được theo dõi ở đâu? <button class="icon-btn-micro" @click.stop="togglePopover('tracked')"><Plus class="w-4 h-4"></Plus></button></div>
            
            <div class="detail-value" v-if="linkedTrackedUrl">
              <div class="linked-item">
                <i class="fa-solid fa-link item-icon"></i>
                <a :href="linkedTrackedUrl" target="_blank" class="item-name truncate">{{ linkedTrackedUrl }}</a>
                <button class="remove-btn" @click="linkedTrackedUrl = ''"><X class="w-4 h-4"></X></button>
              </div>
            </div>

            <!-- Tracked Work Popover -->
            <div class="custom-popover popover-large" v-if="popovers.tracked" @click.stop>
              <h4 class="popover-title">Công việc được theo dõi ở đâu?</h4>
              <p class="popover-desc">Tìm kiếm các hạng mục công việc Jira hoặc thêm liên kết đến các địa điểm mà bạn đang theo dõi công việc cho dự án này.</p>
              <input type="text" class="popover-search" placeholder="Tìm kiếm trên Jira hoặc thêm liên kết" v-model="searchQueries.tracked" />
              <div class="popover-list-title mt-2">KẾT QUẢ</div>
              <div class="popover-actions mt-3">
                <button class="secondary-btn" @click="popovers.tracked = false">Hủy</button>
                <button class="primary-btn" :disabled="!searchQueries.tracked" @click="addTrackedUrl">Thêm</button>
              </div>
            </div>
          </div>

          <!-- Liên kết -->
          <div class="detail-row relative-popover-container">
            <div class="detail-label">Liên kết <button class="icon-btn-micro" @click.stop="togglePopover('link')"><Plus class="w-4 h-4"></Plus></button></div>
            
            <div class="detail-value" v-if="linkedTasks.length > 0">
              <div class="linked-item" v-for="l in linkedTasks" :key="l.id">
                <FileText class="w-4 h-4 item-icon text-blue"></FileText>
                <span class="item-name">{{ l.name }}</span>
                <button class="remove-btn" @click="removeTask(l.id)"><X class="w-4 h-4"></X></button>
              </div>
            </div>

            <!-- Links Popover -->
            <div class="custom-popover" v-if="popovers.link" @click.stop>
              <input type="text" class="popover-search" placeholder="Dán liên kết hoặc tìm nội dung vừa xem" v-model="searchQueries.link" />
              <div class="popover-list mt-2">
                <div class="popover-item" v-for="l in mockLinksList" :key="l.id" @click="addTask(l)">
                  <FileText class="w-4 h-4 item-icon-muted text-blue"></FileText>
                  <div class="item-name">{{ l.name }}</div>
                </div>
              </div>
            </div>
          </div>

          <!-- Ngày bắt đầu -->
          <div class="detail-row">
            <div class="detail-label">Ngày bắt đầu <button class="icon-btn-micro"><i class="fa-regular fa-pen-to-square"></i></button></div>
            <div class="detail-value"><span class="empty-value">15 Jun 2026</span></div>
          </div>
        </div>
      </div>
    </div>

    <!-- Share Modal -->
    <ShareModal 
      :isOpen="isShareModalOpen" 
      :projectId="route.params.id" 
      :projectName="project.title" 
      @close="isShareModalOpen = false" 
    />
  </div>
</template>

<script setup>
import { ChevronDown, Calendar, MoreHorizontal, User, Plus, ArrowRight, Target, GitBranch, X, FileText } from 'lucide-vue-next';
import { ref, computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { useHomeProjectStore } from '@/store/useHomeProjectStore'
import RichTextEditor from '@/components/common/RichTextEditor.vue'
import ShareModal from '@/components/common/ShareModal.vue'

const route = useRoute()
const projectStore = useHomeProjectStore()

const currentTab = ref('overview')
const showStatusMenu = ref(false)
const isShareModalOpen = ref(false)

const popovers = ref({
  goal: false,
  project: false,
  tracked: false,
  link: false
})

const togglePopover = (type) => {
  for (const key in popovers.value) {
    if (key === type) popovers.value[key] = !popovers.value[key]
    else popovers.value[key] = false
  }
}

const closePopovers = (e) => {
  if (!e.target.closest('.custom-popover') && !e.target.closest('.icon-btn-micro')) {
    popovers.value = { goal: false, project: false, tracked: false, link: false }
  }
}

const linkedGoals = ref([])
const linkedProjects = ref([])
const linkedTrackedUrl = ref('')
const linkedTasks = ref([])

const mockGoalsList = [
  { id: 'g1', name: 'r2', owner: 'Tua20000' },
  { id: 'g2', name: '342', owner: 'Tua20000' },
  { id: 'g3', name: 'uew', owner: 'Tua20000' }
]

const mockProjectsList = [
  { id: 'p1', name: 'e', icon: '😎' },
  { id: 'p2', name: 'ueq', icon: '😃' },
  { id: 'p3', name: '##E', icon: '😎' }
]

const mockLinksList = [
  { id: 'l1', type: 'doc', name: 'Dự Án Tốt Nghiệp Home' },
  { id: 'l2', type: 'doc', name: 'Đặc tả lại dự án' },
  { id: 'l3', type: 'doc', name: 'Làm lại tài liệu dự án' },
  { id: 'l4', type: 'link', name: 'Làm trang quản lý riêng cho space' }
]

const searchQueries = ref({ goal: '', project: '', tracked: '', link: '' })

const addGoal = (g) => {
  if (!linkedGoals.value.find(x => x.id === g.id)) linkedGoals.value.push(g)
  popovers.value.goal = false
}
const removeGoal = (id) => { linkedGoals.value = linkedGoals.value.filter(x => x.id !== id) }

const addProject = (p) => {
  if (!linkedProjects.value.find(x => x.id === p.id)) linkedProjects.value.push(p)
  popovers.value.project = false
}
const removeProject = (id) => { linkedProjects.value = linkedProjects.value.filter(x => x.id !== id) }

const addTrackedUrl = () => {
  if (searchQueries.value.tracked) {
    linkedTrackedUrl.value = searchQueries.value.tracked
    searchQueries.value.tracked = ''
    popovers.value.tracked = false
  }
}

const addTask = (l) => {
  if (!linkedTasks.value.find(x => x.id === l.id)) linkedTasks.value.push(l)
  popovers.value.link = false
}
const removeTask = (id) => { linkedTasks.value = linkedTasks.value.filter(x => x.id !== id) }

const editing = ref({
  description: false,
  reason: false,
  success: false,
  learnings: false,
  risks: false,
  decisions: false
})

const project = computed(() => projectStore.currentProject || {
  title: 'uqe',
  description: '',
  reason: '',
  success: '',
  learnings: '',
  risks: '',
  decisions: ''
})

onMounted(async () => {
  window.addEventListener('click', closePopovers)
  if (route.params.id) {
    await projectStore.fetchProjectDetail(route.params.id)
  }
})
</script>

<style scoped>
.project-detail-wrapper {
  display: flex;
  flex-direction: column;
  min-height: 100vh;
  background-color: #FAFBFC;
}

/* Module Header Styles */
.module-header {
  padding: 32px 40px 0;
  background-color: #FFFFFF;
}

.header-content {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
}

.header-content h1 {
  font-size: 24px;
  font-weight: 500;
  color: #172B4D;
  margin: 0;
}

/* Sidebar Popovers */
.relative-popover-container {
  position: relative;
}

.custom-popover {
  position: absolute;
  top: 100%;
  right: 0;
  width: 300px;
  background: white;
  border-radius: 3px;
  box-shadow: 0 4px 8px -2px rgba(9,30,66,0.25), 0 0 1px rgba(9,30,66,0.31);
  padding: 12px;
  z-index: 100;
  margin-top: 4px;
}

.custom-popover.popover-large {
  width: 350px;
  padding: 16px;
}

.popover-search {
  width: 100%;
  padding: 6px 8px;
  border: 2px solid #DFE1E6;
  border-radius: 3px;
  font-size: 14px;
  outline: none;
  transition: border-color 0.2s;
  box-sizing: border-box;
}

.popover-search:focus {
  border-color: #4C9AFF;
}

.popover-list-title {
  font-size: 11px;
  font-weight: 700;
  color: #6B778C;
  margin-top: 12px;
  margin-bottom: 8px;
}

.popover-list {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.popover-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 8px;
  border-radius: 3px;
  cursor: pointer;
}

.popover-item:hover {
  background-color: #F4F5F7;
}

.item-icon-muted {
  color: #5E6C84;
  font-size: 16px;
  width: 16px;
  text-align: center;
}

.item-details {
  display: flex;
  flex-direction: column;
}

.item-name {
  font-size: 14px;
  color: #172B4D;
}

.item-meta {
  font-size: 12px;
  color: #5E6C84;
}

.popover-select-mock {
  display: flex;
  align-items: center;
  border: 2px solid #DFE1E6;
  border-radius: 3px;
  padding: 6px 8px;
  font-size: 14px;
  color: #172B4D;
  cursor: pointer;
}

.popover-title {
  font-size: 16px;
  font-weight: 500;
  color: #172B4D;
  margin: 0 0 8px 0;
}

.popover-desc {
  font-size: 12px;
  color: #5E6C84;
  margin: 0 0 16px 0;
  line-height: 1.5;
}

.popover-actions {
  display: flex;
  justify-content: flex-end;
  gap: 8px;
}

.mt-2 { margin-top: 8px; }
.mt-3 { margin-top: 12px; }
.text-blue { color: #0052CC; }

.linked-item {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 6px 8px;
  background-color: #F4F5F7;
  border-radius: 3px;
  margin-top: 8px;
  group-hover: block; /* Custom handling for remove btn */
}

.linked-item .remove-btn {
  margin-left: auto;
  background: transparent;
  border: none;
  color: #5E6C84;
  cursor: pointer;
  opacity: 0;
  transition: opacity 0.2s;
  padding: 4px;
}

.linked-item:hover .remove-btn {
  opacity: 1;
}

.linked-item .remove-btn:hover {
  background-color: rgba(9, 30, 66, 0.08);
  border-radius: 3px;
}

.truncate {
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  max-width: 200px;
}

.primary-btn {
  background-color: #0052CC;
  color: white;
  border: none;
  padding: 8px 16px;
  border-radius: 3px;
  font-weight: 500;
  font-size: 14px;
  cursor: pointer;
  transition: background-color 0.2s;
}

.primary-btn:hover {
  background-color: #0047B3;
}

.tabs-nav {
  display: flex;
  border-bottom: 2px solid #DFE1E6;
  gap: 24px;
}

.tab-link {
  padding: 8px 0 12px;
  font-size: 14px;
  font-weight: 500;
  color: #5E6C84;
  text-decoration: none;
  position: relative;
  margin-bottom: -2px;
  border-bottom: 2px solid transparent;
  transition: color 0.2s;
}

.tab-link:hover {
  color: #172B4D;
}

.tab-link.active {
  color: #0052CC;
  border-bottom-color: #0052CC;
}

/* Entity Header */
.project-header {
  padding-top: 32px;
  background-color: #FFFFFF;
}

.project-header-inner {
  max-width: 1000px;
  margin: 0 auto;
  padding: 0 40px;
  width: 100%;
}

.header-main {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
}

.title-block {
  display: flex;
  align-items: center;
  gap: 12px;
}

.project-emoji-large {
  font-size: 24px;
}

.title-block h1 {
  margin: 0;
  font-size: 24px;
  font-weight: 500;
  color: #172B4D;
}

.header-actions {
  display: flex;
  gap: 8px;
  align-items: center;
}

.secondary-btn {
  background-color: rgba(9, 30, 66, 0.04);
  color: #42526E;
  border: none;
  padding: 6px 12px;
  border-radius: 3px;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 6px;
}

.secondary-btn:hover {
  background-color: rgba(9, 30, 66, 0.08);
}

.icon-btn-header {
  background: transparent;
  color: #42526E;
  border: none;
  padding: 6px 10px;
  border-radius: 3px;
  cursor: pointer;
}

.icon-btn-header:hover {
  background-color: rgba(9, 30, 66, 0.08);
}

.status-badge {
  display: inline-flex;
  align-items: center;
  padding: 4px 8px;
  border-radius: 3px;
  font-size: 12px;
  font-weight: 700;
  text-transform: uppercase;
}

.status-on-track { background-color: #E3FCEF; color: #006644; }
.status-done { background-color: #EAE6FF; color: #403294; }
.status-at-risk { background-color: #FFF0B3; color: #FF8B00; }
.status-off-track { background-color: #FFEBE6; color: #BF2600; }
.status-pending { background-color: #DFE1E6; color: #42526E; }

.target-date-badge {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  padding: 4px 8px;
  border-radius: 3px;
  font-size: 12px;
  color: #42526E;
  border: 1px solid #DFE1E6;
}

/* Tabs */
.project-tabs-container {
  border-bottom: 2px solid #DFE1E6;
}

.project-tabs {
  display: flex;
  gap: 24px;
  max-width: 1000px;
  margin: 0 auto;
  padding: 0 40px;
  width: 100%;
}

.tab-btn {
  background: transparent;
  border: none;
  color: #5E6C84;
  font-size: 14px;
  font-weight: 500;
  padding: 8px 0 12px;
  cursor: pointer;
  position: relative;
  margin-bottom: -2px;
  border-bottom: 2px solid transparent;
}

.tab-btn:hover {
  color: #172B4D;
}

.tab-btn.active {
  color: #0052CC;
  border-bottom-color: #0052CC;
}

.tab-badge {
  background-color: #DFE1E6;
  color: #172B4D;
  padding: 2px 6px;
  border-radius: 12px;
  font-size: 11px;
  margin-left: 4px;
}

/* Content Grid */
.project-content-grid {
  display: grid;
  grid-template-columns: minmax(0, 1fr) 300px;
  gap: 40px;
  padding: 32px 40px;
  max-width: 1000px;
  margin: 0 auto;
  width: 100%;
}

.main-column {
  display: flex;
  flex-direction: column;
  gap: 40px;
}

.content-section {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.content-section h4 {
  margin: 0;
  font-size: 16px;
  color: #172B4D;
  font-weight: 600;
}

.editable-placeholder {
  padding: 8px 0;
  cursor: pointer;
  border-radius: 3px;
}

.editable-placeholder:hover {
  background-color: #FAFBFC;
}

.editable-placeholder p {
  margin: 0;
  font-size: 14px;
  line-height: 1.5;
  color: #172B4D;
}

.empty-text {
  color: #5E6C84 !important;
}

.comment-input-mockup {
  display: flex;
  align-items: center;
  gap: 12px;
}

.user-avatar-current {
  width: 32px;
  height: 32px;
  background-color: #0052CC;
  color: white;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 12px;
  font-weight: 600;
}

.fake-input {
  flex: 1;
  padding: 10px 16px;
  background-color: #FAFBFC;
  border: 1px solid #DFE1E6;
  border-radius: 24px;
  color: #5E6C84;
  font-size: 14px;
}

/* Sidebar */
.details-body {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.detail-row {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.detail-label {
  font-size: 12px;
  font-weight: 600;
  color: #5E6C84;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.badge-count {
  background-color: #DFE1E6;
  padding: 2px 6px;
  border-radius: 12px;
  font-size: 11px;
  color: #172B4D;
}

.icon-btn-micro {
  background: transparent;
  border: none;
  color: #5E6C84;
  cursor: pointer;
  width: 24px;
  height: 24px;
  border-radius: 3px;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: background-color 0.2s, color 0.2s;
}

.icon-btn-micro:hover {
  background-color: rgba(9, 30, 66, 0.08);
}

.owner-chip {
  display: flex;
  align-items: center;
  gap: 8px;
}

.owner-avatar-micro {
  width: 24px;
  height: 24px;
  border-radius: 50%;
  background-color: #0052CC;
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 10px;
  font-weight: bold;
}

.owner-info {
  display: flex;
  flex-direction: column;
}

.owner-name {
  font-size: 14px;
  color: #172B4D;
}

.owner-role {
  font-size: 12px;
  color: #5E6C84;
}

.empty-value {
  font-size: 14px;
  color: #5E6C84;
}

.flex-between {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

/* Empty Tab States */
.empty-state-large-tab {
  display: flex;
  align-items: center;
  padding: 40px;
  background-color: #FFFFFF;
  border: 1px solid #DFE1E6;
  border-radius: 3px;
  gap: 40px;
  margin-top: 16px;
}

.empty-illustration {
  flex-shrink: 0;
  width: 120px;
  height: 120px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.empty-text-content {
  flex: 1;
}

.empty-text-content h4 {
  margin: 0 0 12px 0;
  font-size: 16px;
  color: #172B4D;
}

.empty-text-content p {
  margin: 0 0 24px 0;
  color: #5E6C84;
  font-size: 14px;
  line-height: 1.5;
}

.empty-actions {
  display: flex;
  gap: 16px;
  align-items: center;
}

.link-btn {
  color: #0052CC;
  text-decoration: none;
  font-size: 14px;
  font-weight: 500;
}

.link-btn:hover {
  text-decoration: underline;
}

/* Updates Tab styles */
.updates-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
}

.updates-header h4 {
  margin: 0;
  font-size: 16px;
  color: #172B4D;
}

.last-update-text {
  font-size: 12px;
  color: #5E6C84;
}

.timeline-visual {
  position: relative;
  height: 40px;
  margin-bottom: 32px;
}

.timeline-line {
  position: absolute;
  top: 50%;
  left: 0;
  right: 0;
  height: 2px;
  background-color: #36B37E;
  z-index: 1;
}

.timeline-node {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  background-color: #FFFFFF;
  padding: 0 8px;
  z-index: 2;
  display: flex;
  flex-direction: column;
  align-items: center;
  color: #36B37E;
}

.timeline-node i {
  background-color: #E3FCEF;
  padding: 6px;
  border-radius: 50%;
  font-size: 14px;
  border: 2px solid #FFFFFF;
}

.timeline-node span {
  font-size: 11px;
  margin-top: 4px;
}

.update-editor-box {
  border: 1px solid #DFE1E6;
  border-radius: 3px;
  background-color: #FFFFFF;
  box-shadow: 0 4px 8px -2px rgba(9,30,66,0.25);
  margin-bottom: 40px;
}

.update-editor-header {
  display: flex;
  padding: 16px;
  border-bottom: 1px solid #DFE1E6;
  gap: 24px;
}

.editor-field {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.editor-field label {
  font-size: 12px;
  font-weight: 600;
  color: #5E6C84;
}

.status-dropdown-wrapper {
  position: relative;
}

.status-dropdown-menu {
  position: absolute;
  top: 100%;
  left: 0;
  margin-top: 4px;
  background: white;
  border: 1px solid #DFE1E6;
  border-radius: 3px;
  box-shadow: 0 4px 8px -2px rgba(9,30,66,0.25);
  padding: 8px 0;
  z-index: 10;
  min-width: 180px;
}

.status-option {
  padding: 6px 12px;
  cursor: pointer;
}

.status-option:hover {
  background-color: #FAFBFC;
}

.text-option {
  font-size: 12px;
  color: #5E6C84;
}

.divider {
  height: 1px;
  background-color: #DFE1E6;
  margin: 4px 0;
}

.date-dropdown {
  background-color: transparent;
  cursor: pointer;
}

.template-link {
  margin-left: auto;
  justify-content: center;
  color: #5E6C84;
  font-size: 12px;
  cursor: pointer;
}

.update-editor-body {
  padding: 16px;
}

.update-editor-body textarea {
  width: 100%;
  border: none;
  resize: none;
  outline: none;
  font-size: 14px;
  font-family: inherit;
  color: #172B4D;
}

.update-editor-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px 16px;
  border-top: 1px solid #DFE1E6;
  background-color: #FAFBFC;
}

.editor-tools {
  display: flex;
  align-items: center;
  gap: 8px;
}

.tool-ai {
  font-size: 12px;
  color: #5E6C84;
  margin-right: 8px;
}

.tool-btn {
  background: transparent;
  border: none;
  color: #42526E;
  padding: 4px 8px;
  cursor: pointer;
  border-radius: 3px;
}

.tool-btn:hover {
  background-color: rgba(9, 30, 66, 0.08);
}

.editor-actions {
  display: flex;
  align-items: center;
  gap: 12px;
}

.char-count {
  font-size: 12px;
  color: #5E6C84;
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

.primary-btn:hover { background-color: #0047B3; }

/* Timeline Posts */
.timeline-period {
  font-size: 16px;
  color: #172B4D;
  margin: 0 0 16px 0;
}

.timeline-post {
  border: 1px solid #DFE1E6;
  border-radius: 3px;
  padding: 16px;
  background-color: #FAFBFC;
}

.post-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 16px;
}

.post-user {
  display: flex;
  gap: 12px;
}

.user-info {
  display: flex;
  flex-direction: column;
}

.user-name {
  font-size: 14px;
  font-weight: 500;
  color: #172B4D;
}

.post-time {
  font-size: 12px;
  color: #5E6C84;
}

.post-status-meta {
  font-size: 12px;
  color: #5E6C84;
  display: flex;
  align-items: center;
  gap: 6px;
}

.post-content {
  margin-bottom: 16px;
}

.post-content p {
  margin: 0 0 16px 0;
  font-size: 14px;
  color: #172B4D;
}

.status-change-log {
  display: inline-block;
  background-color: #FFFFFF;
  border: 1px solid #DFE1E6;
  padding: 8px 12px;
  border-radius: 3px;
  font-size: 12px;
  color: #5E6C84;
}

.mx-1 { margin: 0 4px; }
.mt-16 { margin-top: 16px; }

.post-actions {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 12px;
  color: #5E6C84;
  margin-bottom: 16px;
}

.reaction-btn {
  background-color: #FFFFFF;
  border: 1px solid #DFE1E6;
  border-radius: 12px;
  padding: 2px 8px;
  cursor: pointer;
  font-size: 12px;
}

.reaction-btn:hover {
  background-color: #FAFBFC;
}
</style>
