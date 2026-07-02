<template>
  <div class="checkin-container admin-page">
    <div class="page-header flex justify-between items-center mb-6">
      <div>
        <h1 class="page-title" style="display:flex; align-items:center;">
          <i class="fa-solid fa-calendar-check text-success" style="margin-right: 10px; flex-shrink: 0;"></i>
          <span>Virtual Check-in Hàng Ngày</span>
        </h1>
        <p class="page-subtitle">Cập nhật nhanh tiến độ làm việc, mục tiêu trong ngày và chia sẻ khó khăn với các đồng nghiệp.</p>
      </div>

      <!-- Quick Action: Submit Checkin -->
      <div v-if="!userCheckedIn">
        <el-button class="btn-primary" type="primary" @click="openCheckinModal">
          <i class="fa-solid fa-plus" style="margin-right: 6px;"></i>Báo cáo Check-in
        </el-button>
      </div>
      <div v-else>
        <el-tag type="success" size="large" class="flex items-center gap-2 font-semibold">
          <i class="fa-solid fa-circle-check" style="margin-right: 5px;"></i>Đã Check-in hôm nay
        </el-tag>
      </div>
    </div>

    <!-- AI Meeting Summary Widget (Tóm tắt cuộc họp AI) -->
    <div class="ai-summary-widget card mb-6 p-5">
      <div class="flex justify-between items-center mb-3 border-bottom pb-2">
        <div style="display:flex; align-items:center; gap: 10px;">
          <i class="fa-solid fa-brain text-accent" style="font-size: 20px; flex-shrink: 0;"></i>
          <h2 class="font-bold" style="font-size: 16px; margin: 0;">AI Tóm Tắt Cuộc Họp &amp; Check-in</h2>
        </div>
        <el-button size="small" class="btn-secondary" :loading="aiLoading" @click="generateAiSummary">
          <i class="fa-solid fa-wand-magic-sparkles" style="margin-right: 6px;"></i>Trích xuất bằng AI
        </el-button>
      </div>
      <div class="summary-body">
        <p v-if="!aiSummaryText" class="text-sm text-muted italic">Nhấp vào nút bên phải để AI tự động phân tích và tóm tắt nhanh tình trạng check-in hôm nay của cả đội.</p>
        <div v-else class="ai-response-box">
          <p class="text-sm leading-relaxed text-secondary mb-2 whitespace-pre-line">{{ aiSummaryText }}</p>
          <div class="flex gap-2 mt-3">
            <el-tag size="small" type="success">3/5 Thành viên đã làm</el-tag>
            <el-tag size="small" type="danger">Có 1 Blocker (Khôi)</el-tag>
          </div>
        </div>
      </div>
    </div>

    <!-- Checkin Cards List (Both checked-in and not checked-in members) -->
    <div class="team-checkins-grid">
      <div v-for="team in teamCheckins" :key="team.id" class="checkin-card card" :class="{ 'not-checked': !team.checkedIn }">
        <div class="card-header flex items-center justify-between">
          <div class="flex items-center" style="gap: 8px;">
            <el-avatar :size="32" :src="team.userAvatar" style="flex-shrink: 0;">{{ team.userName.charAt(0) }}</el-avatar>
            <div style="line-height: 1.3;">
              <span class="font-bold block text-sm">{{ team.userName }}</span>
              <span class="text-xxs text-muted">{{ team.role }}</span>
            </div>
          </div>
          <el-tag size="small" :type="team.checkedIn ? 'success' : 'info'">
            {{ team.checkedIn ? 'Đã Check-in' : 'Chưa Check-in' }}
          </el-tag>
        </div>

        <div class="card-body">
          <div v-if="team.checkedIn" class="checkin-details">
            <!-- Done yesterday -->
            <div class="detail-section">
              <span class="section-label">✅ Ngày hôm qua:</span>
              <p class="section-desc">{{ team.yesterday }}</p>
            </div>
            
            <!-- Focus today -->
            <div class="detail-section mt-3">
              <span class="section-label">📌 Mục tiêu hôm nay:</span>
              <p class="section-desc">{{ team.today }}</p>
            </div>

            <!-- Blockers -->
            <div class="detail-section mt-3">
              <span class="section-label">⚠️ Khó khăn (Blocker):</span>
              <p class="section-desc" :class="{ 'has-blocker': team.blocker }">
                {{ team.blocker || 'Không có khó khăn gì' }}
              </p>
            </div>
          </div>
          
          <div v-else class="empty-checkin flex flex-col items-center justify-center py-6 text-center">
            <i class="fa-regular fa-bell text-2xl text-muted mb-2"></i>
            <span class="text-xs text-muted">Chưa nộp báo cáo hàng ngày</span>
          </div>
        </div>
      </div>
    </div>

    <!-- Virtual Check-in Modal Dialog -->
    <el-dialog
      v-model="checkinModalOpen"
      title="Báo cáo tiến độ hàng ngày"
      width="540px"
      append-to-body
    >
      <div class="checkin-form-body flex flex-col gap-4">
        <!-- Yesterday input -->
        <div>
          <span class="field-label mb-1">Hôm qua bạn đã làm được gì?</span>
          <textarea 
            v-model="form.yesterday" 
            placeholder="Ví dụ: Hoàn tất cập nhật connection string cho SQL Server, thiết kế các layout..."
            class="w-full h-20 p-2"
          ></textarea>
        </div>

        <!-- Today input -->
        <div>
          <span class="field-label mb-1">Mục tiêu chính hôm nay của bạn?</span>
          <textarea 
            v-model="form.today" 
            placeholder="Ví dụ: Thiết kế giao diện Chat nhóm và Daily Checkin..."
            class="w-full h-20 p-2"
          ></textarea>
        </div>

        <!-- Blocker input -->
        <div>
          <span class="field-label mb-1">Khó khăn đang gặp phải (nếu có)?</span>
          <input 
            v-model="form.blocker" 
            type="text" 
            placeholder="Để trống nếu không có khó khăn nào"
            class="w-full"
          />
        </div>
      </div>
      <template #footer>
        <div class="flex justify-end gap-2">
          <el-button class="btn-secondary" @click="checkinModalOpen = false">Hủy</el-button>
          <el-button class="btn-primary" type="primary" @click="submitCheckin">Gửi báo cáo</el-button>
        </div>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { ElMessage } from 'element-plus'

const userCheckedIn = ref(false)
const checkinModalOpen = ref(false)
const aiLoading = ref(false)
const aiSummaryText = ref('')

const form = ref({
  yesterday: '',
  today: '',
  blocker: ''
})

const teamCheckins = ref([
  {
    id: 'user-kiet',
    userName: 'Nguyễn Tuấn Kiệt',
    userAvatar: 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?auto=format&fit=crop&q=80&w=128',
    role: 'Smart Priority Lead',
    checkedIn: true,
    yesterday: 'Hoàn thiện logic tính toán điểm ưu tiên Task dựa trên thời hạn và độ quan trọng.',
    today: 'Đồng bộ AI suggestion panel để gợi ý phân loại thứ tự ưu tiên cho user.',
    blocker: ''
  },
  {
    id: 'user-phat',
    userName: 'Trần Gia Phát',
    userAvatar: 'https://images.unsplash.com/photo-1500648767791-00dcc994a43e?auto=format&fit=crop&q=80&w=128',
    role: 'Kanban Board dev',
    checkedIn: true,
    yesterday: 'Code xong cột Kanban, xử lý drag drop drag-start and drag-end events.',
    today: 'Xử lý animation kéo thả mượt mà hơn và thêm tooltip đếm số lượng task trong mỗi cột.',
    blocker: ''
  },
  {
    id: 'user-khoi',
    userName: 'Đinh Tuấn Khôi',
    userAvatar: '',
    role: 'Integration Hub Lead',
    checkedIn: true,
    yesterday: 'Tích hợp OAuth với GitHub và Slack.',
    today: 'Cố gắng đồng bộ email Gmail thành task tự động.',
    blocker: 'Đang bị nghẽn (Block) do API rate limit của Google OAuth cần cấu hình lại khóa app.'
  },
  {
    id: 'user-quan',
    userName: 'Đoàn Minh Quân',
    userAvatar: '',
    role: 'Collaboration Lead',
    checkedIn: false,
    yesterday: '',
    today: '',
    blocker: ''
  }
])

const openCheckinModal = () => {
  checkinModalOpen.value = true
}

const submitCheckin = () => {
  if (!form.value.yesterday.trim() || !form.value.today.trim()) {
    ElMessage.warning('Vui lòng điền đầy đủ thông tin ngày hôm qua và hôm nay!')
    return
  }

  // Find index of current user and update
  const quanIdx = teamCheckins.value.findIndex(t => t.id === 'user-quan')
  if (quanIdx !== -1) {
    teamCheckins.value[quanIdx] = {
      ...teamCheckins.value[quanIdx],
      checkedIn: true,
      yesterday: form.value.yesterday,
      today: form.value.today,
      blocker: form.value.blocker
    }
  }

  userCheckedIn.value = true
  checkinModalOpen.value = false
  ElMessage.success('Gửi báo cáo Check-in ngày thành công!')
}

const generateAiSummary = () => {
  aiLoading.value = true
  setTimeout(() => {
    aiLoading.value = false
    aiSummaryText.value = `### Tóm tắt Daily Scrum của cả đội (Ngày 01/07/2026):

1. **Tiến độ tốt**: Kiệt và Phát đang chạy đúng tiến độ với các phần việc AI Priority và Kanban Board, các tính năng kéo thả mượt mà đã hoàn tất cơ bản.
2. **Điểm cần lưu ý**: Khôi đang gặp sự cố kết nối API với Google OAuth do bị giới hạn Request Limit.
3. **Đề xuất**: Quân hỗ trợ Khôi cấu hình lại Redirect URI hoặc tạo sandbox key mới để giải quyết vấn đề OAuth trước khi Sprint kết thúc.`
    ElMessage.success('Đã tạo tóm tắt họp AI thành công!')
  }, 1200)
}
</script>

<style scoped>
.checkin-container {
  width: min(100%, 1080px) !important;
  max-width: 1080px !important;
  margin: 0 auto !important;
  padding: 22px 24px 32px !important;
}

.checkin-container .page-header {
  margin-bottom: 16px !important;
  padding: 16px 18px !important;
  border: 1px solid color-mix(in srgb, var(--color-border) 88%, transparent);
  border-radius: 14px;
  background:
    linear-gradient(135deg, color-mix(in srgb, var(--color-success) 7%, var(--color-surface)), var(--color-surface));
  box-shadow: 0 16px 42px color-mix(in srgb, #020617 10%, transparent);
}

.checkin-container .page-title {
  margin-bottom: 4px !important;
  font-size: clamp(24px, 2.2vw, 34px) !important;
  line-height: 1.1 !important;
}

.checkin-container .page-subtitle {
  max-width: 760px;
  margin: 0 !important;
  font-size: 14px !important;
  line-height: 1.5 !important;
}

.ai-summary-widget {
  margin-bottom: 14px !important;
  padding: 16px 18px !important;
  border-left: 4px solid var(--color-accent);
  border-radius: 12px;
}

.ai-response-box {
  background-color: var(--color-surface-hover);
  border: 1px solid var(--color-border);
  padding: 14px;
  border-radius: var(--radius-card);
}

.team-checkins-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  gap: 14px;
}

.checkin-card {
  display: flex;
  flex-direction: column;
  height: 100%;
}

.checkin-card.not-checked {
  opacity: 0.7;
  border-style: dashed;
}

.card-header {
  border-bottom: 1px solid var(--color-border);
  padding: 11px 14px;
}

.card-body {
  padding: 14px;
  flex: 1;
}

.section-label {
  font-size: 11px;
  font-weight: 700;
  color: var(--color-text-muted);
  text-transform: uppercase;
  display: block;
  margin-bottom: 2px;
}

.section-desc {
  font-size: 13px;
  color: var(--color-text-primary);
  line-height: 1.5;
}

.has-blocker {
  color: var(--color-danger);
  font-weight: 500;
}

:deep(.card-header .el-avatar) {
  margin: 0 !important;
  flex-shrink: 0;
}

@media (max-width: 860px) {
  .checkin-container {
    padding: 16px !important;
  }

  .checkin-container .page-header,
  .ai-summary-widget > .flex {
    align-items: flex-start !important;
    flex-direction: column !important;
  }
}
</style>
