<script setup>
import { ref, onMounted } from 'vue'
import axiosClient from '@/api/axiosClient'

const props = defineProps({
  projectId: { type: String, required: true }
})

const emit = defineEmits(['task-created'])

const intakes = ref([])
const loading = ref(false)
const showCreate = ref(false)
const newIntake = ref({ title: '', description: '', source: 'MANUAL' })

onMounted(() => loadIntakes())

async function loadIntakes() {
  loading.value = true
  try {
    const res = await axiosClient.get(`/projects/${props.projectId}/intakes`)
    intakes.value = res.data?.data || []
  } catch (e) {
    console.error('Failed to load intakes', e)
    intakes.value = []
  } finally {
    loading.value = false
  }
}

async function createIntake() {
  if (!newIntake.value.title.trim()) return
  try {
    await axiosClient.post(`/projects/${props.projectId}/intakes`, newIntake.value)
    newIntake.value = { title: '', description: '', source: 'MANUAL' }
    showCreate.value = false
    loadIntakes()
  } catch (e) {
    alert(e.response?.data?.message || 'Lỗi khi tạo yêu cầu')
  }
}

async function updateStatus(id, status) {
  try {
    await axiosClient.put(`/projects/${props.projectId}/intakes/${id}/review`, { status })
    loadIntakes()
    // When accepted, backend auto-creates a WorkTask. Notify parent to refresh task lists.
    if (status === 'Accepted') {
      emit('task-created')
    }
  } catch (e) {
    alert(e.response?.data?.message || 'Lỗi khi cập nhật trạng thái')
  }
}

function getStatusInfo(status) {
  const map = {
    'Pending': { color: '#f59e0b', label: 'Đang chờ', icon: '⏳' },
    'Accepted': { color: '#22c55e', label: 'Đã duyệt', icon: '✅' },
    'Declined': { color: '#ef4444', label: 'Từ chối', icon: '❌' },
    'Duplicate': { color: '#6b7280', label: 'Trùng lặp', icon: '🔄' },
    'Snoozed': { color: '#8b5cf6', label: 'Tạm hoãn', icon: '💤' }
  }
  return map[status] || map['Pending']
}

function formatDate(d) {
  if (!d) return '—'
  return new Date(d).toLocaleString('vi-VN', {
    day: '2-digit', month: '2-digit', year: 'numeric',
    hour: '2-digit', minute: '2-digit'
  })
}
</script>

<template>
  <div class="intake-inbox">
    <div class="intake-header">
      <h3>📥 Hộp thư đến (Intake)</h3>
      <el-button type="primary" size="small" @click="showCreate = true">+ Thêm yêu cầu</el-button>
    </div>

    <div v-loading="loading" class="intake-list">
      <div v-for="item in intakes" :key="item.id" class="intake-item">
        <div class="intake-left">
          <span class="intake-icon">{{ getStatusInfo(item.status).icon }}</span>
          <div class="intake-info">
            <span class="intake-title">{{ item.title }}</span>
            <span class="intake-meta">
              {{ item.source }} · {{ formatDate(item.createdAt) }}
              <template v-if="item.submittedByName"> · bởi {{ item.submittedByName }}</template>
            </span>
          </div>
        </div>
        <div class="intake-right">
          <span class="intake-status" :style="{ color: getStatusInfo(item.status).color }">
            {{ getStatusInfo(item.status).label }}
          </span>
          <div v-if="item.status === 'Pending'" class="intake-actions">
            <el-button size="small" type="success" plain @click="updateStatus(item.id, 'Accepted')">Duyệt</el-button>
            <el-button size="small" type="danger" plain @click="updateStatus(item.id, 'Declined')">Từ chối</el-button>
          </div>
        </div>
      </div>

      <div v-if="!loading && intakes.length === 0" class="intake-empty">
        <span class="empty-icon">📭</span>
        <p>Hộp thư trống. Chưa có yêu cầu nào.</p>
      </div>
    </div>

    <el-dialog v-model="showCreate" title="Gửi yêu cầu mới" width="480px">
      <el-form label-position="top">
        <el-form-item label="Tiêu đề">
          <el-input v-model="newIntake.title" placeholder="VD: Lỗi khi đăng nhập trên mobile" />
        </el-form-item>
        <el-form-item label="Mô tả chi tiết">
          <el-input v-model="newIntake.description" type="textarea" :rows="4" placeholder="Chi tiết vấn đề..." />
        </el-form-item>
        <el-form-item label="Nguồn">
          <el-select v-model="newIntake.source">
            <el-option value="MANUAL" label="Nhập thủ công" />
            <el-option value="EMAIL" label="Email" />
            <el-option value="API" label="API" />
            <el-option value="FORM" label="Biểu mẫu" />
          </el-select>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showCreate = false">Hủy</el-button>
        <el-button type="primary" @click="createIntake" :disabled="!newIntake.title.trim()">Gửi</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped>
.intake-inbox {
  padding: 4px 0;
}

.intake-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.intake-header h3 {
  font-size: 16px;
  font-weight: 700;
  color: var(--text-primary);
}

.intake-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.intake-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 14px 16px;
  border-radius: 10px;
  background: var(--bg-card);
  border: 1px solid var(--border-color);
  transition: all 0.15s;
}

.intake-item:hover {
  background: var(--hover-bg);
  border-color: var(--el-color-primary);
}

.intake-left {
  display: flex;
  align-items: center;
  gap: 12px;
}

.intake-icon {
  font-size: 20px;
}

.intake-info {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.intake-title {
  font-size: 14px;
  font-weight: 600;
  color: var(--text-primary);
}

.intake-meta {
  font-size: 11px;
  color: var(--text-muted);
}

.intake-right {
  display: flex;
  align-items: center;
  gap: 12px;
}

.intake-status {
  font-size: 12px;
  font-weight: 600;
}

.intake-actions {
  display: flex;
  gap: 4px;
}

.intake-empty {
  text-align: center;
  padding: 40px;
  color: var(--text-muted);
}

.empty-icon {
  font-size: 32px;
  display: block;
  margin-bottom: 8px;
}
</style>
