<template>
  <div>
    <div class="rewards-page">
      <header class="nexus-feature-header">
        <div class="header-info">
          <p class="eyebrow">Hệ thống điểm</p>
          <h1>Phần thưởng</h1>
          <p class="muted">Theo dõi điểm thưởng, cấp độ nghề nghiệp, tỷ lệ đóng góp và thưởng hoàn thành sớm.</p>
        </div>
        <button class="refresh-btn" type="button" :disabled="loading" @click="loadRewards">
          <i class="fa-solid fa-rotate"></i> {{ loading ? 'Đang cập nhật...' : 'Làm mới' }}
        </button>
      </header>

      <section class="wallet-band">
        <div class="wallet-card">
          <span class="label">Số dư hiện tại</span>
          <strong>{{ wallet.totalPoints }}</strong>
          <span class="unit">điểm</span>
        </div>
        <div class="wallet-card">
          <span class="label">Cấp độ nghề nghiệp</span>
          <strong>{{ career.level }}</strong>
          <span class="unit">{{ career.title }}</span>
        </div>
        <div class="wallet-card wide">
          <div class="wallet-card-head">
            <span class="label">Tiến độ cấp độ</span>
            <span class="unit">{{ pointsToNext }} điểm đến cấp tiếp theo</span>
          </div>
          <div class="progress-container">
            <div class="progress-track">
              <div class="progress-fill" :style="{ width: `${career.progressPercent}%` }"></div>
            </div>
            <span class="progress-text">{{ career.progressPercent }}%</span>
          </div>
        </div>
      </section>

      <section class="formula-band">
        <div class="panel">
          <div class="panel-head">
            <h2>Công thức tính điểm</h2>
            <span>{{ formula.expression }}</span>
          </div>
          <div class="formula-grid">
            <div class="formula-cell">
              <span>Độ khó</span>
              <strong>{{ formula.sample.difficulty }}</strong>
            </div>
            <div class="formula-cell">
              <span>Thời lượng</span>
              <strong>{{ formula.sample.duration }}</strong>
            </div>
            <div class="formula-cell">
              <span>Tỷ lệ</span>
              <strong>{{ formula.sample.share }}%</strong>
            </div>
            <div class="formula-cell total">
              <span>Điểm cuối cùng</span>
              <strong>{{ formula.sample.total }}</strong>
            </div>
          </div>
          <p class="helper-copy">{{ formula.sample.note }}</p>
          <div class="policy-list">
            <div class="summary-row"><span>Quy tắc thực tế</span><strong>{{ formula.actualHoursRule }}</strong></div>
            <div class="summary-row"><span>Nhiều người thực hiện</span><strong>{{ formula.policy?.multiAssignee }}</strong></div>
            <div class="summary-row"><span>Chuyển tiếp</span><strong>{{ formula.policy?.carryOver }}</strong></div>
          </div>
        </div>

        <div class="panel">
          <div class="panel-head">
            <h2>Tổng kết</h2>
            <span>Sprint này</span>
          </div>
          <div class="summary-list">
            <div class="summary-row"><span>Công việc hoàn thành</span><strong>{{ summary.completedTasks }}</strong></div>
            <div class="summary-row"><span>Thưởng hoàn thành sớm</span><strong>{{ summary.earlyBonuses }}</strong></div>
            <div class="summary-row"><span>Điểm cơ bản</span><strong>{{ summary.basePoints }}</strong></div>
            <div class="summary-row"><span>Điểm thưởng</span><strong>{{ summary.bonusPoints }}</strong></div>
            <div class="summary-row"><span>Điểm phạt</span><strong>{{ summary.penaltyPoints }}</strong></div>
            <div class="summary-row"><span>Tỷ lệ đóng góp</span><strong>{{ summary.contributionPercent }}%</strong></div>
            <div class="summary-row"><span>Giờ dự kiến</span><strong>{{ summary.estimatedHours }}h</strong></div>
            <div class="summary-row"><span>Giờ thực tế</span><strong>{{ summary.actualHours }}h</strong></div>
            <div class="summary-row"><span>Giờ đã ghi nhận</span><strong>{{ summary.loggedHours }}h</strong></div>
            <div class="summary-row"><span>Điểm thu hồi</span><strong>{{ summary.rollbackPoints }}</strong></div>
          </div>
        </div>
      </section>

      <main class="rewards-grid">
        <section class="panel">
          <div class="panel-head"><h2>Công việc tiêu biểu</h2><span>Giá trị nhất</span></div>
          <article v-for="task in displaySpotlightTasks" :key="task.id" class="spotlight-row">
            <div class="spotlight-main">
              <strong>{{ task.sequenceId || 'TASK' }}</strong>
              <div class="spotlight-title">{{ task.title }}</div>
              <small>{{ task.estimatedDays }} ngày · {{ task.estimatedHours }}h dự kiến / {{ task.actualHours }}h thực tế · tỷ lệ {{ task.contributionShare }}%</small>
            </div>
            <div class="spotlight-side">
              <span class="chip">{{ task.fairPoints }} điểm</span>
              <span class="chip muted">hiệu suất x{{ task.efficiency }}</span>
              <span class="chip muted">chất lượng x{{ task.qualityModifier }}</span>
              <span class="chip muted">{{ task.progressPercent }}%</span>
            </div>
          </article>
        </section>

        <section class="panel">
          <div class="panel-head"><h2>Thành tích gần đây</h2><span>{{ displayRecentAchievements.length }}</span></div>
          <article v-for="item in displayRecentAchievements" :key="item.id" class="achievement-row">
            <div>
              <strong>{{ item.title }}</strong>
              <div class="muted">{{ item.reason }}</div>
            </div>
            <div class="achievement-points">+{{ item.amount }}</div>
          </article>
        </section>
      </main>

      <section class="rewards-grid lower-grid">
        <section class="panel">
          <div class="panel-head"><h2>Lịch sử điểm</h2><span>{{ displayTransactions.length }}</span></div>
          <div v-if="loading" class="empty">Đang tải dữ liệu...</div>
          <article v-for="tx in displayTransactions" v-else :key="tx.id" class="tx-row">
            <div class="tx-icon" :class="{ negative: tx.amount < 0 }">
              <i :class="tx.amount >= 0 ? 'fa-solid fa-plus' : 'fa-solid fa-minus'"></i>
            </div>
            <div class="tx-main">
              <div class="tx-title">{{ tx.taskSequenceId || tx.taskTitle || tx.reason }}</div>
              <div class="tx-reason">{{ tx.reason }}</div>
              <time>{{ formatDate(tx.createdAt) }}</time>
            </div>
            <strong class="tx-points" :class="{ negative: tx.amount < 0 }">{{ tx.amount > 0 ? '+' : '' }}{{ tx.amount }}</strong>
          </article>
        </section>

        <section class="panel">
          <div class="panel-head"><h2>Bảng xếp hạng</h2><span>Top 20</span></div>
          <div v-if="!leaderboard.length" class="empty">Chưa có dữ liệu xếp hạng.</div>
          <article v-for="(item, index) in leaderboard" :key="item.userId" class="leader-row">
            <span class="rank">#{{ index + 1 }}</span>
            <UserAvatar :user="{ avatarColor: getAvatarBg(item.userName), initials: getInitials(item.userName), fullName: item.userName }" :size="32" :fontSize="11" class="avatar-wrapper" />
            <div class="leader-main">
              <strong>{{ item.userName || 'Thành viên' }}</strong>
              <small>{{ item.careerTitle || `Cấp độ ${item.level}` }}</small>
            </div>
            <span class="leader-points">{{ item.totalPoints }} điểm</span>
          </article>
        </section>
      </section>
    </div>
  </div>
</template>

<script setup>
import { computed, onMounted, ref } from 'vue'
import { ElMessage } from 'element-plus'

import axiosClient from '@/api/axiosClient'
import UserAvatar from '@/components/common/UserAvatar.vue'

const loading = ref(false)
const wallet = ref({ totalPoints: 0, level: 1, nextLevelAt: 1000 })
const career = ref({ level: 1, title: 'Contributor', progressPercent: 0, nextThreshold: 1000 })
const formula = ref({
  expression: 'Base effort x Efficiency x Quality x Contribution share',
  actualHoursRule: '',
  policy: {},
  sample: { difficulty: 0, duration: 0, share: 0, total: 0, bonus: 0, penalty: 0, note: '' }
})
const summary = ref({ completedTasks: 0, earlyBonuses: 0, basePoints: 0, bonusPoints: 0, penaltyPoints: 0, contributionPercent: 0, rollbackPoints: 0, estimatedHours: 0, actualHours: 0, loggedHours: 0 })
const spotlightTasks = ref([])
const recentAchievements = ref([])
const transactions = ref([])
const leaderboard = ref([])

const now = Date.now()
const demoSpotlightTasks = [
  {
    id: 'demo-spotlight-1',
    sequenceId: 'SPR-128',
    title: 'Hoàn thiện luồng tạo task từ Integration Inbox',
    estimatedDays: 2,
    estimatedHours: 8,
    actualHours: 6,
    contributionShare: 100,
    fairPoints: 96,
    efficiency: 1.25,
    qualityModifier: 1.1,
    progressPercent: 100
  },
  {
    id: 'demo-spotlight-2',
    sequenceId: 'SPR-117',
    title: 'Đồng bộ Gmail và Google Calendar vào Unified Inbox',
    estimatedDays: 3,
    estimatedHours: 12,
    actualHours: 11,
    contributionShare: 80,
    fairPoints: 72,
    efficiency: 1.09,
    qualityModifier: 1,
    progressPercent: 100
  },
  {
    id: 'demo-spotlight-3',
    sequenceId: 'SPR-104',
    title: 'Thiết kế lại Rewards dashboard gọn và dễ đọc',
    estimatedDays: 1,
    estimatedHours: 5,
    actualHours: 4,
    contributionShare: 100,
    fairPoints: 48,
    efficiency: 1.2,
    qualityModifier: 1,
    progressPercent: 90
  }
]
const demoAchievements = [
  { id: 'demo-achievement-1', title: 'Hoàn thành đúng hạn', reason: 'Task tích hợp được đưa về trạng thái hoàn thành trước deadline.', amount: 35 },
  { id: 'demo-achievement-2', title: 'Đóng góp nổi bật', reason: 'Tạo task thật từ dữ liệu lịch/email và giữ nội dung có dấu rõ ràng.', amount: 28 },
  { id: 'demo-achievement-3', title: 'Cộng tác tốt', reason: 'Có cập nhật tiến độ, giảm lỗi trùng dữ liệu khi đồng bộ lại.', amount: 18 }
]
const demoTransactions = [
  { id: 'demo-tx-1', taskSequenceId: 'SPR-128', taskTitle: 'Hoàn thiện luồng tạo task từ Integration Inbox', reason: 'Điểm cơ bản + thưởng hoàn thành sớm', amount: 96, createdAt: new Date(now - 1000 * 60 * 35).toISOString() },
  { id: 'demo-tx-2', taskSequenceId: 'SPR-117', taskTitle: 'Đồng bộ Gmail và Google Calendar', reason: 'Điểm đóng góp theo tỷ lệ thực hiện 80%', amount: 72, createdAt: new Date(now - 1000 * 60 * 60 * 3).toISOString() },
  { id: 'demo-tx-3', taskSequenceId: 'SPR-104', taskTitle: 'Thiết kế Rewards dashboard', reason: 'Điểm chất lượng cho UI không chồng chữ', amount: 48, createdAt: new Date(now - 1000 * 60 * 60 * 8).toISOString() },
  { id: 'demo-tx-4', taskSequenceId: 'SPR-099', taskTitle: 'Kiểm tra OAuth redirect', reason: 'Phạt nhẹ do cập nhật trễ cấu hình callback', amount: -6, createdAt: new Date(now - 1000 * 60 * 60 * 24).toISOString() }
]

const displaySpotlightTasks = computed(() => spotlightTasks.value.length ? spotlightTasks.value : demoSpotlightTasks)
const displayRecentAchievements = computed(() => recentAchievements.value.length ? recentAchievements.value : demoAchievements)
const displayTransactions = computed(() => transactions.value.length ? transactions.value : demoTransactions)

const formatDate = (value) => (value ? new Date(value).toLocaleString('vi-VN', { dateStyle: 'short', timeStyle: 'short' }) : '')
const pointsToNext = computed(() => Math.max(0, Number(career.value?.nextThreshold || 0) - Number(wallet.value?.totalPoints || 0)))

const getInitials = (name = '') => {
  const parts = name.trim().split(/\s+/).filter(Boolean)
  return (parts.length > 1 ? `${parts[0][0]}${parts.at(-1)[0]}` : name.slice(0, 2)).toUpperCase() || 'U'
}

const loadRewards = async () => {
  loading.value = true
  try {
    const [mine, leaders] = await Promise.all([
      axiosClient.get('/gamification/me'),
      axiosClient.get('/gamification/leaderboard')
    ])

    const data = mine.data?.data || {}
    const nextWallet = data.wallet || {}
    const nextCareer = data.career || {}

    wallet.value = {
      ...wallet.value,
      ...nextWallet,
      totalPoints: Number(nextWallet.totalPoints ?? wallet.value.totalPoints ?? 0),
      level: Number(nextWallet.level ?? wallet.value.level ?? 1),
      nextLevelAt: Number(nextWallet.nextLevelAt ?? wallet.value.nextLevelAt ?? 1000)
    }
    career.value = {
      ...career.value,
      ...nextCareer,
      level: Number(nextCareer.level ?? wallet.value.level ?? career.value.level ?? 1),
      title: nextCareer.title || wallet.value.rankTitle || career.value.title || 'Contributor',
      nextThreshold: Number(nextCareer.nextThreshold ?? wallet.value.nextLevelAt ?? career.value.nextThreshold ?? 1000),
      progressPercent: Math.max(0, Math.min(100, Number(nextCareer.progressPercent ?? career.value.progressPercent ?? 0)))
    }
    formula.value = {
      ...formula.value,
      ...(data.formula || {}),
      sample: {
        ...formula.value.sample,
        ...(data.formula?.sample || {})
      },
      policy: {
        ...formula.value.policy,
        ...(data.formula?.policy || {})
      }
    }
    summary.value = {
      ...summary.value,
      ...(data.summary || {})
    }
    spotlightTasks.value = data.spotlightTasks || []
    recentAchievements.value = data.recentAchievements || []
    transactions.value = data.transactions || []
    leaderboard.value = leaders.data?.data || []
  } catch (error) {
    ElMessage.error(error.response?.data?.message || 'Unable to load rewards.')
  } finally {
    loading.value = false
  }
}

const getAvatarBg = (name) => {
  if (!name || name === 'Unassigned') return '#64748b'
  const colors = ['#3b82f6', '#10b981', '#fbbf24', '#ec4899', '#8b5cf6', '#06b6d4', '#f97316']
  let hash = 0
  for (let i = 0; i < name.length; i++) {
    hash = name.charCodeAt(i) + ((hash << 5) - hash)
  }
  const index = Math.abs(hash) % colors.length
  return colors[index]
}

onMounted(loadRewards)
</script>

<style scoped>
.rewards-page {
  min-height: calc(100vh - 56px);
  background: var(--color-bg);
  color: var(--color-text-primary);
  padding: 32px;
  font-family: 'Inter', sans-serif;
}

.rewards-header,
.wallet-band,
.formula-band,
.rewards-grid,
.panel-head,
.summary-row,
.tx-row,
.leader-row,
.achievement-row,
.spotlight-row,
.wallet-card-head {
  display: flex;
}

.rewards-header,
.panel-head,
.summary-row,
.tx-row,
.leader-row,
.achievement-row,
.spotlight-row,
.wallet-card-head {
  align-items: center;
}

.rewards-header,
.panel-head,
.summary-row,
.achievement-row,
.spotlight-row {
  justify-content: space-between;
}

.rewards-header {
  gap: 24px;
  margin-bottom: 28px;
}

.eyebrow {
  color: var(--color-accent);
  font-size: 11px;
  text-transform: uppercase;
  font-weight: 700;
  margin: 0 0 6px;
  letter-spacing: 1px;
}

h1 {
  margin: 0;
  font-size: 28px;
  font-weight: 700;
  color: var(--color-text-primary);
}

.muted,
time,
small,
.helper-copy {
  color: var(--color-text-muted);
}

.refresh-btn {
  border: 1px solid var(--color-border);
  border-radius: 8px;
  background: var(--color-surface);
  color: var(--color-text-primary);
  padding: 8px 16px;
  cursor: pointer;
  font-size: 13px;
  font-weight: 500;
  transition: all 0.2s ease;
  display: inline-flex;
  align-items: center;
  gap: 8px;
}
.refresh-btn:hover {
  background: var(--color-surface-hover);
  border-color: var(--color-accent);
}

.wallet-band,
.formula-band,
.rewards-grid {
  gap: 20px;
}

.wallet-band {
  margin-bottom: 24px;
}

.wallet-card,
.panel {
  border: 1px solid var(--color-border);
  border-radius: 16px;
  background: var(--color-surface);
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.02);
  transition: all 0.3s cubic-bezier(0.25, 0.8, 0.25, 1);
  backdrop-filter: blur(10px);
}
.wallet-card:hover,
.panel:hover {
  transform: translateY(-4px);
  box-shadow: 0 10px 24px rgba(0, 0, 0, 0.06);
  border-color: var(--color-accent);
}

.wallet-card {
  flex: 1;
  padding: 24px;
}

.wallet-card.wide {
  flex: 2;
}

.label {
  display: block;
  color: var(--color-text-secondary);
  font-size: 12px;
  margin-bottom: 8px;
  font-weight: 500;
}

.wallet-card strong,
.formula-cell strong {
  font-size: 32px;
  font-weight: 700;
  color: var(--color-text-primary);
}

.unit {
  color: var(--color-text-muted);
  margin-left: 8px;
  font-size: 13px;
}

.progress-container {
  display: flex;
  align-items: center;
  gap: 16px;
  margin-top: 14px;
}

.progress-track {
  flex: 1;
  height: 10px;
  background: var(--color-border);
  border-radius: 8px;
  overflow: hidden;
  position: relative;
}

.progress-fill {
  height: 100%;
  background: linear-gradient(90deg, var(--color-success), var(--color-accent));
  transition: width 0.6s cubic-bezier(0.4, 0, 0.2, 1);
}

.progress-text {
  font-size: 13px;
  font-weight: 700;
  color: var(--color-accent);
  min-width: 40px;
}

.formula-band,
.rewards-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  margin-bottom: 24px;
}

.lower-grid {
  margin-bottom: 0;
}

.panel {
  overflow: hidden;
}

.panel-head {
  padding: 18px 24px;
  border-bottom: 1px solid var(--color-border);
}

.panel-head h2 {
  margin: 0;
  font-size: 16px;
  font-weight: 600;
}

.formula-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 12px;
  padding: 24px 24px 0;
}

.formula-cell {
  padding: 16px;
  border: 1px solid var(--color-border);
  border-radius: 12px;
  background: var(--color-bg);
  transition: all 0.2s ease;
}
.formula-cell:hover {
  border-color: var(--color-accent);
  background: var(--color-surface-hover);
}

.formula-cell span {
  display: block;
  color: var(--color-text-muted);
  margin-bottom: 8px;
  font-size: 12px;
}

.formula-cell.total strong {
  color: var(--color-success);
}

.helper-copy,
.summary-list,
.empty {
  padding: 24px;
}

.summary-row {
  padding: 12px 0;
  border-bottom: 1px solid var(--color-border);
}

.summary-row:last-child {
  border-bottom: 0;
}

.spotlight-row,
.achievement-row,
.tx-row,
.leader-row {
  gap: 16px;
  padding: 16px 24px;
  border-bottom: 1px solid var(--color-border);
  transition: background 0.2s ease;
}
.spotlight-row:hover,
.achievement-row:hover,
.tx-row:hover,
.leader-row:hover {
  background: var(--color-surface-hover);
}

.spotlight-row:last-child,
.achievement-row:last-child,
.tx-row:last-child,
.leader-row:last-child {
  border-bottom: 0;
}

.spotlight-main,
.tx-main,
.leader-main {
  flex: 1;
  min-width: 0;
}

.spotlight-title,
.tx-title {
  font-weight: 600;
  margin-top: 4px;
  color: var(--color-text-primary);
}

.spotlight-side {
  display: flex;
  gap: 8px;
}

.chip {
  padding: 4px 10px;
  border-radius: 20px;
  background: color-mix(in srgb, var(--color-success) 10%, transparent);
  color: var(--color-success);
  font-size: 11px;
  font-weight: 600;
}

.chip.muted {
  background: var(--color-surface-hover);
  color: var(--color-text-muted);
}

.tx-icon,
.avatar {
  width: 32px;
  height: 32px;
  display: grid;
  place-items: center;
  border-radius: 50%;
  flex-shrink: 0;
}

.tx-icon {
  background: color-mix(in srgb, var(--color-success) 10%, transparent);
  color: var(--color-success);
}

.tx-icon.negative {
  background: color-mix(in srgb, var(--color-danger) 10%, transparent);
  color: var(--color-danger);
}

.tx-reason {
  color: var(--color-text-secondary);
  font-size: 13px;
  margin: 3px 0;
}

.tx-points,
.achievement-points {
  color: var(--color-success);
  font-size: 18px;
  font-weight: 700;
}

.tx-points.negative {
  color: var(--color-danger);
}

.rank {
  width: 32px;
  font-weight: 700;
  font-size: 14px;
  color: var(--color-text-muted);
}
.leader-row:nth-child(1) .rank { color: #fbbf24; }
.leader-row:nth-child(2) .rank { color: #94a3b8; }
.leader-row:nth-child(3) .rank { color: #b45309; }

.avatar {
  color: #ffffff;
  font-size: 11px;
  font-weight: 700;
  border: 1.5px solid var(--color-bg);
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.15);
}

.leader-main strong {
  display: block;
  font-size: 14px;
  font-weight: 500;
}

.leader-points {
  color: var(--color-warning);
  font-weight: 700;
  font-size: 14px;
}

.empty {
  text-align: center;
  color: var(--color-text-muted);
  font-size: 14px;
}

@media (max-width: 960px) {
  .wallet-band,
  .formula-band,
  .rewards-grid,
  .formula-grid {
    grid-template-columns: 1fr;
  }

  .wallet-band {
    flex-direction: column;
  }

  .rewards-header {
    flex-direction: column;
    align-items: flex-start;
  }
}

/* SprintA visual refresh */
.rewards-page {
  background:
    radial-gradient(circle at 6% 0%, rgba(56, 189, 248, 0.18), transparent 32%),
    radial-gradient(circle at 88% 8%, rgba(34, 197, 94, 0.12), transparent 30%),
    linear-gradient(180deg, #f8fbff 0%, #eef5fb 46%, #f8fafc 100%);
  padding: 34px clamp(20px, 3vw, 44px);
  font-family: inherit;
}

.nexus-feature-header {
  position: relative;
  overflow: hidden;
  border: 1px solid rgba(14, 165, 233, 0.18);
  border-radius: 18px;
  padding: 24px;
  margin-bottom: 22px;
  background:
    linear-gradient(135deg, rgba(14, 165, 233, 0.16), transparent 45%),
    linear-gradient(100deg, rgba(255, 255, 255, 0.96), rgba(255, 255, 255, 0.76));
  box-shadow: 0 22px 60px rgba(15, 23, 42, 0.08);
}

.nexus-feature-header::after {
  content: "";
  position: absolute;
  right: 24px;
  top: 22px;
  width: 90px;
  height: 90px;
  border-radius: 24px;
  background:
    linear-gradient(135deg, #facc15, #22c55e);
  opacity: 0.16;
  transform: rotate(8deg);
}

.eyebrow {
  color: #0284c7;
  letter-spacing: 0.08em;
}

h1 {
  font-size: clamp(30px, 3vw, 46px);
  font-weight: 900;
  letter-spacing: 0;
}

.muted {
  max-width: 720px;
  font-size: 14px;
  line-height: 1.65;
}

.refresh-btn {
  border-color: rgba(14, 165, 233, 0.28);
  border-radius: 11px;
  background: #ffffff;
  color: #0369a1;
  font-weight: 800;
  box-shadow: 0 10px 26px rgba(14, 165, 233, 0.13);
}

.refresh-btn:hover {
  transform: translateY(-1px);
  background: #e0f2fe;
}

.wallet-card,
.panel {
  position: relative;
  overflow: hidden;
  border-color: rgba(148, 163, 184, 0.34);
  border-radius: 18px;
  background:
    linear-gradient(180deg, rgba(255, 255, 255, 0.92), rgba(255, 255, 255, 0.78)),
    #ffffff;
  box-shadow:
    0 18px 44px rgba(15, 23, 42, 0.07),
    inset 0 1px 0 rgba(255, 255, 255, 0.9);
  backdrop-filter: none;
}

.wallet-card::before,
.panel::before {
  content: "";
  position: absolute;
  inset: 0 0 auto;
  height: 4px;
  background: linear-gradient(90deg, #38bdf8, #22c55e, #facc15);
  opacity: 0.9;
}

.wallet-card:hover,
.panel:hover {
  transform: translateY(-3px);
  border-color: rgba(14, 165, 233, 0.42);
  box-shadow: 0 24px 62px rgba(15, 23, 42, 0.1);
}

.wallet-card strong,
.formula-cell strong {
  color: #0f172a;
  font-weight: 900;
}

.label,
.panel-head span,
.formula-cell span {
  color: #64748b;
  font-weight: 800;
}

.progress-track {
  height: 12px;
  background: #dbeafe;
}

.progress-fill {
  background: linear-gradient(90deg, #06b6d4, #22c55e, #facc15);
  box-shadow: 0 0 18px rgba(34, 197, 94, 0.35);
}

.progress-text {
  color: #0284c7;
  font-weight: 900;
}

.panel-head {
  background:
    linear-gradient(90deg, rgba(14, 165, 233, 0.08), transparent),
    rgba(255, 255, 255, 0.58);
}

.panel-head h2 {
  font-size: 17px;
  font-weight: 900;
}

.formula-cell {
  border-color: rgba(148, 163, 184, 0.28);
  border-radius: 14px;
  background: linear-gradient(180deg, #f8fafc, #eef6ff);
}

.formula-cell.total {
  background: linear-gradient(135deg, rgba(34, 197, 94, 0.14), #f8fafc);
}

.summary-row,
.spotlight-row,
.achievement-row,
.tx-row,
.leader-row {
  border-color: rgba(148, 163, 184, 0.18);
}

.spotlight-row,
.achievement-row,
.tx-row,
.leader-row {
  margin: 8px 12px;
  border: 1px solid rgba(148, 163, 184, 0.18);
  border-radius: 14px;
  background: rgba(255, 255, 255, 0.58);
}

.spotlight-row:hover,
.achievement-row:hover,
.tx-row:hover,
.leader-row:hover {
  background: #ffffff;
  border-color: rgba(14, 165, 233, 0.28);
}

.chip {
  border: 1px solid rgba(16, 185, 129, 0.24);
  background: rgba(16, 185, 129, 0.12);
  font-weight: 800;
}

.chip.muted {
  border-color: rgba(148, 163, 184, 0.24);
  background: #f1f5f9;
  color: #64748b;
}

.tx-icon {
  border-radius: 12px;
}

.achievement-points,
.tx-points {
  color: #059669;
  font-weight: 900;
}

[data-theme='dark'] .rewards-page {
  background:
    radial-gradient(circle at 6% 0%, rgba(56, 189, 248, 0.16), transparent 32%),
    radial-gradient(circle at 88% 8%, rgba(34, 197, 94, 0.1), transparent 30%),
    linear-gradient(180deg, #07111f 0%, #0f172a 46%, #101827 100%);
  color: #e2e8f0;
}

[data-theme='dark'] .nexus-feature-header,
[data-theme='dark'] .wallet-card,
[data-theme='dark'] .panel {
  border-color: rgba(148, 163, 184, 0.2);
  background:
    linear-gradient(180deg, rgba(30, 41, 59, 0.92), rgba(15, 23, 42, 0.86)),
    #0f172a;
  box-shadow:
    0 22px 58px rgba(0, 0, 0, 0.32),
    inset 0 1px 0 rgba(255, 255, 255, 0.05);
}

[data-theme='dark'] .nexus-feature-header {
  background:
    linear-gradient(135deg, rgba(56, 189, 248, 0.14), transparent 45%),
    rgba(15, 23, 42, 0.84);
}

[data-theme='dark'] h1,
[data-theme='dark'] .panel-head h2,
[data-theme='dark'] .wallet-card strong,
[data-theme='dark'] .formula-cell strong,
[data-theme='dark'] .spotlight-title,
[data-theme='dark'] .tx-title,
[data-theme='dark'] .leader-main strong {
  color: #f8fafc;
}

[data-theme='dark'] .muted,
[data-theme='dark'] time,
[data-theme='dark'] small,
[data-theme='dark'] .helper-copy,
[data-theme='dark'] .label,
[data-theme='dark'] .panel-head span,
[data-theme='dark'] .formula-cell span,
[data-theme='dark'] .tx-reason,
[data-theme='dark'] .chip.muted {
  color: #94a3b8;
}

[data-theme='dark'] .refresh-btn {
  border-color: rgba(56, 189, 248, 0.24);
  background: rgba(15, 23, 42, 0.82);
  color: #7dd3fc;
}

[data-theme='dark'] .formula-cell,
[data-theme='dark'] .spotlight-row,
[data-theme='dark'] .achievement-row,
[data-theme='dark'] .tx-row,
[data-theme='dark'] .leader-row {
  border-color: rgba(148, 163, 184, 0.18);
  background: rgba(15, 23, 42, 0.58);
}

[data-theme='dark'] .formula-cell.total {
  background: linear-gradient(135deg, rgba(34, 197, 94, 0.14), rgba(15, 23, 42, 0.58));
}

[data-theme='dark'] .panel-head {
  border-bottom-color: rgba(148, 163, 184, 0.16);
  background:
    linear-gradient(90deg, rgba(56, 189, 248, 0.08), transparent),
    rgba(15, 23, 42, 0.5);
}

[data-theme='dark'] .progress-track {
  background: rgba(51, 65, 85, 0.92);
}

/* Compact density */
.rewards-page {
  padding: 18px var(--sa-page-x, 24px) 30px !important;
  min-height: calc(100vh - var(--sa-topbar-height, 52px)) !important;
}

.nexus-feature-header {
  border-radius: 10px !important;
  padding: 18px !important;
  margin-bottom: 16px !important;
}

.nexus-feature-header::after {
  width: 58px !important;
  height: 58px !important;
  border-radius: 12px !important;
}

h1 {
  font-size: clamp(24px, 2.2vw, 34px) !important;
  line-height: 1.1 !important;
}

.muted {
  font-size: 12.5px !important;
  line-height: 1.45 !important;
}

.wallet-band,
.formula-band,
.rewards-grid {
  gap: 14px !important;
  margin-bottom: 16px !important;
}

.wallet-card,
.panel {
  border-radius: 10px !important;
  box-shadow: 0 10px 24px rgba(15, 23, 42, 0.06) !important;
}

.wallet-card {
  padding: 16px !important;
}

.wallet-card strong,
.formula-cell strong {
  font-size: 26px !important;
}

.progress-track {
  height: 8px !important;
}

.panel-head {
  padding: 12px 16px !important;
}

.panel-head h2 {
  font-size: 14px !important;
}

.formula-grid {
  gap: 10px !important;
  padding: 16px 16px 0 !important;
}

.formula-cell {
  border-radius: 8px !important;
  padding: 12px !important;
}

.helper-copy,
.summary-list,
.empty {
  padding: 16px !important;
}

.summary-row {
  padding: 9px 0 !important;
}

.spotlight-row,
.achievement-row,
.tx-row,
.leader-row {
  margin: 8px 14px !important;
  padding: 12px 14px !important;
  border-radius: 8px !important;
  gap: 10px !important;
}

.chip {
  border-radius: 999px !important;
  padding: 3px 8px !important;
}

@media (max-width: 780px) {
  .rewards-page {
    padding: 12px !important;
  }

  .nexus-feature-header {
    padding: 14px !important;
  }

  .wallet-band,
  .formula-band,
  .rewards-grid,
  .formula-grid {
    grid-template-columns: 1fr !important;
    flex-direction: column !important;
  }

  .spotlight-row {
    align-items: flex-start !important;
    flex-direction: column !important;
  }

  .spotlight-side {
    flex-wrap: wrap !important;
  }
}

/* Premium rewards presentation */
@keyframes rewards-rise-in {
  from {
    opacity: 0;
    transform: translateY(16px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@keyframes rewards-progress-glow {
  0%, 100% { filter: saturate(1); }
  50% { filter: saturate(1.25) brightness(1.08); }
}

.rewards-page {
  background:
    radial-gradient(circle at 10% 0%, rgba(56, 189, 248, 0.18), transparent 30%),
    radial-gradient(circle at 84% 8%, rgba(250, 204, 21, 0.13), transparent 28%),
    linear-gradient(180deg, #06111f, #0f172a 50%, #101827) !important;
}

.nexus-feature-header,
.wallet-card,
.panel {
  position: relative;
  overflow: hidden;
  animation: rewards-rise-in 540ms cubic-bezier(0.2, 0.8, 0.2, 1) both;
  transition:
    transform 220ms cubic-bezier(0.2, 0.8, 0.2, 1),
    box-shadow 220ms ease,
    border-color 220ms ease,
    background 220ms ease !important;
}

.wallet-card:nth-child(2) { animation-delay: 80ms; }
.wallet-card:nth-child(3) { animation-delay: 130ms; }
.panel:nth-child(1) { animation-delay: 180ms; }
.panel:nth-child(2) { animation-delay: 230ms; }
.panel:nth-child(3) { animation-delay: 280ms; }
.panel:nth-child(4) { animation-delay: 330ms; }

.nexus-feature-header {
  background:
    radial-gradient(circle at 78% 20%, rgba(250, 204, 21, 0.14), transparent 18%),
    linear-gradient(135deg, rgba(56, 189, 248, 0.14), rgba(15, 23, 42, 0.84) 42%),
    rgba(15, 23, 42, 0.92) !important;
  border-color: rgba(56, 189, 248, 0.24) !important;
  box-shadow: 0 30px 90px rgba(2, 8, 23, 0.34) !important;
}

.nexus-feature-header::after {
  background:
    linear-gradient(135deg, rgba(250, 204, 21, 0.32), rgba(34, 197, 94, 0.20)) !important;
  box-shadow: 0 18px 44px rgba(250, 204, 21, 0.16);
}

.wallet-card,
.panel {
  background:
    linear-gradient(135deg, rgba(30, 41, 59, 0.94), rgba(15, 23, 42, 0.88)),
    #0f172a !important;
  border-color: rgba(148, 163, 184, 0.20) !important;
  box-shadow:
    0 22px 58px rgba(0, 0, 0, 0.26),
    inset 0 1px 0 rgba(255, 255, 255, 0.055) !important;
}

.wallet-card::before,
.panel::before {
  content: "";
  position: absolute;
  inset: 0 0 auto 0;
  height: 3px;
  background: linear-gradient(90deg, #38bdf8, #2dd4bf, #facc15);
}

.wallet-card:hover,
.panel:hover {
  transform: translateY(-3px);
  border-color: rgba(56, 189, 248, 0.34) !important;
  box-shadow:
    0 30px 80px rgba(2, 8, 23, 0.34),
    0 0 0 1px rgba(56, 189, 248, 0.10) inset !important;
}

.wallet-card strong,
.formula-cell strong {
  letter-spacing: -0.025em;
}

.wallet-card strong {
  text-shadow: 0 10px 28px rgba(56, 189, 248, 0.24);
}

.progress-track span,
.progress-bar span,
.level-progress span {
  animation: rewards-progress-glow 2.8s ease-in-out infinite;
  background: linear-gradient(90deg, #22c55e, #38bdf8, #facc15) !important;
  box-shadow: 0 0 24px rgba(56, 189, 248, 0.22);
}

.formula-cell {
  background:
    linear-gradient(135deg, rgba(56, 189, 248, 0.08), transparent 64%),
    rgba(15, 23, 42, 0.56) !important;
}

.formula-cell.total {
  background:
    linear-gradient(135deg, rgba(34, 197, 94, 0.16), rgba(56, 189, 248, 0.08)),
    rgba(15, 23, 42, 0.64) !important;
  border-color: rgba(34, 197, 94, 0.28) !important;
}

.summary-row,
.spotlight-row,
.achievement-row,
.tx-row,
.leader-row {
  transition: transform 180ms ease, background 180ms ease, border-color 180ms ease;
}

.summary-row:hover,
.spotlight-row:hover,
.achievement-row:hover,
.tx-row:hover,
.leader-row:hover {
  transform: translateX(4px);
  background:
    linear-gradient(90deg, rgba(56, 189, 248, 0.10), transparent 74%),
    rgba(15, 23, 42, 0.68) !important;
  border-color: rgba(56, 189, 248, 0.24) !important;
}

.chip {
  box-shadow: inset 0 1px 0 rgba(255,255,255,0.06);
}

.nexus-feature-header,
.wallet-card,
.panel,
.summary-row,
.formula-cell,
.spotlight-row,
.achievement-row,
.tx-row,
.leader-row {
  color: var(--color-text-primary);
  transition:
    background 220ms ease,
    color 220ms ease,
    border-color 220ms ease,
    box-shadow 220ms ease,
    transform 220ms cubic-bezier(0.2, 0.8, 0.2, 1) !important;
}

.nexus-feature-header {
  padding: 24px !important;
}

.panel-head {
  padding: 16px 20px !important;
  gap: 16px !important;
}

.panel-head h2 {
  line-height: 1.25 !important;
}

.formula-grid {
  padding: 18px 20px 0 !important;
}

.helper-copy,
.summary-list,
.empty {
  padding: 18px 20px !important;
}

.summary-row {
  padding: 12px 0 !important;
  line-height: 1.45 !important;
}

.summary-row span {
  min-width: 120px;
}

.formula-cell span,
.wallet-card .label,
.panel-head span,
.helper-copy {
  font-weight: 700;
}

[data-theme='light'] .rewards-page {
  background:
    radial-gradient(circle at 10% 0%, rgba(56, 189, 248, 0.16), transparent 30%),
    radial-gradient(circle at 84% 8%, rgba(250, 204, 21, 0.13), transparent 28%),
    linear-gradient(180deg, #f8fcff 0%, #eef6fb 52%, #f8fafc 100%) !important;
}

[data-theme='light'] .nexus-feature-header {
  background:
    radial-gradient(circle at 78% 20%, rgba(250, 204, 21, 0.18), transparent 18%),
    linear-gradient(135deg, rgba(255,255,255,0.98), rgba(240,249,255,0.82)),
    #ffffff !important;
  border-color: rgba(56, 189, 248, 0.22) !important;
  box-shadow: 0 28px 80px rgba(14, 165, 233, 0.12) !important;
}

[data-theme='light'] .wallet-card,
[data-theme='light'] .panel {
  background:
    linear-gradient(135deg, rgba(255,255,255,0.98), rgba(248,250,252,0.88)),
    #ffffff !important;
  border-color: rgba(148, 163, 184, 0.20) !important;
  box-shadow: 0 22px 58px rgba(15, 23, 42, 0.08) !important;
}

[data-theme='light'] .formula-cell {
  background:
    linear-gradient(135deg, rgba(56, 189, 248, 0.08), transparent 64%),
    #f8fafc !important;
}

[data-theme='light'] .formula-cell.total {
  background:
    linear-gradient(135deg, rgba(34, 197, 94, 0.12), rgba(56, 189, 248, 0.08)),
    #f0fdf4 !important;
}

[data-theme='light'] .nexus-feature-header h1,
[data-theme='light'] .wallet-card strong,
[data-theme='light'] .formula-cell strong,
[data-theme='light'] .panel-head h2,
[data-theme='light'] .summary-row strong,
[data-theme='light'] .spotlight-title,
[data-theme='light'] .tx-title,
[data-theme='light'] .leader-main strong {
  color: #0f172a !important;
}

[data-theme='light'] .muted,
[data-theme='light'] .wallet-card .label,
[data-theme='light'] .wallet-card .unit,
[data-theme='light'] .formula-cell span,
[data-theme='light'] .panel-head span,
[data-theme='light'] .summary-row span,
[data-theme='light'] .helper-copy,
[data-theme='light'] .tx-reason,
[data-theme='light'] .leader-main small {
  color: #475569 !important;
}

[data-theme='dark'] .rewards-page {
  background:
    radial-gradient(circle at 10% 0%, rgba(56, 189, 248, 0.18), transparent 30%),
    radial-gradient(circle at 84% 8%, rgba(250, 204, 21, 0.13), transparent 28%),
    linear-gradient(180deg, #06111f, #0f172a 50%, #101827) !important;
}

[data-theme='dark'] .nexus-feature-header h1,
[data-theme='dark'] .wallet-card strong,
[data-theme='dark'] .formula-cell strong,
[data-theme='dark'] .panel-head h2,
[data-theme='dark'] .summary-row strong,
[data-theme='dark'] .spotlight-title,
[data-theme='dark'] .tx-title,
[data-theme='dark'] .leader-main strong {
  color: #f8fafc !important;
}

[data-theme='dark'] .muted,
[data-theme='dark'] .wallet-card .label,
[data-theme='dark'] .wallet-card .unit,
[data-theme='dark'] .formula-cell span,
[data-theme='dark'] .panel-head span,
[data-theme='dark'] .summary-row span,
[data-theme='dark'] .helper-copy,
[data-theme='dark'] .tx-reason,
[data-theme='dark'] .leader-main small {
  color: #cbd5e1 !important;
}

/* Rewards readability pass */
.wallet-card {
  min-width: 0;
}

.wallet-card strong,
.formula-cell strong,
.summary-row strong,
.leader-points,
.achievement-points,
.tx-points {
  font-variant-numeric: tabular-nums;
  letter-spacing: 0;
}

.wallet-card strong {
  display: inline-flex;
  align-items: baseline;
  min-height: 36px;
  padding: 2px 8px 2px 0;
}

.formula-cell.total,
.summary-list .summary-row:nth-child(6),
.summary-list .summary-row:nth-child(10) {
  position: relative;
  background:
    linear-gradient(90deg, color-mix(in srgb, var(--color-success) 10%, transparent), transparent 72%),
    color-mix(in srgb, var(--color-surface-hover) 48%, transparent);
}

.summary-list {
  padding: 12px 20px 16px !important;
}

.summary-row {
  gap: 16px;
  min-height: 38px;
  padding: 10px 0 !important;
}

.summary-row span {
  min-width: 0;
  line-height: 1.35;
}

.summary-row strong {
  flex: 0 0 auto;
  min-width: 54px;
  text-align: right;
  font-weight: 900;
  color: var(--color-text-primary);
}

.spotlight-side {
  flex-wrap: wrap;
  justify-content: flex-end;
  max-width: 320px;
}

.spotlight-main small,
.tx-reason,
.leader-main small,
.muted {
  line-height: 1.45;
}

.spotlight-row,
.achievement-row,
.tx-row,
.leader-row {
  min-width: 0;
  border-color: color-mix(in srgb, var(--color-border) 76%, transparent) !important;
}

.spotlight-row strong:first-child,
.tx-title,
.leader-main strong {
  overflow-wrap: anywhere;
}

.chip:first-child,
.leader-points,
.achievement-points,
.tx-points {
  border-radius: 999px;
  padding: 4px 10px;
  background: color-mix(in srgb, var(--color-success) 12%, transparent);
}

.leader-row:nth-child(1) {
  background:
    linear-gradient(90deg, rgba(250, 204, 21, 0.13), transparent 68%),
    color-mix(in srgb, var(--color-surface) 88%, transparent) !important;
  border-color: rgba(250, 204, 21, 0.30) !important;
}

.leader-row:nth-child(2),
.leader-row:nth-child(3) {
  background:
    linear-gradient(90deg, rgba(56, 189, 248, 0.10), transparent 68%),
    color-mix(in srgb, var(--color-surface) 88%, transparent) !important;
}

[data-theme='light'] .spotlight-row,
[data-theme='light'] .achievement-row,
[data-theme='light'] .tx-row,
[data-theme='light'] .leader-row {
  background: rgba(255, 255, 255, 0.78) !important;
}

[data-theme='dark'] .spotlight-row,
[data-theme='dark'] .achievement-row,
[data-theme='dark'] .tx-row,
[data-theme='dark'] .leader-row {
  background: rgba(15, 23, 42, 0.62) !important;
}

/* Rewards layout repair */
.rewards-page {
  max-width: 1180px;
  margin: 0 auto;
}

.formula-band {
  grid-template-columns: minmax(0, 1.05fr) minmax(300px, 0.95fr) !important;
  align-items: start;
}

.panel-head {
  align-items: flex-start !important;
  min-width: 0;
}

.panel-head h2,
.panel-head span {
  min-width: 0;
}

.panel-head span {
  max-width: 62%;
  text-align: right;
  white-space: normal;
  overflow-wrap: anywhere;
  line-height: 1.35;
}

.formula-grid {
  grid-template-columns: repeat(4, minmax(94px, 1fr)) !important;
}

.formula-cell {
  min-width: 0;
}

.formula-cell span {
  min-height: 32px;
  line-height: 1.25;
}

.policy-list {
  display: grid;
  gap: 8px;
  padding: 0 20px 18px;
}

.policy-list .summary-row {
  display: grid !important;
  grid-template-columns: minmax(96px, 0.34fr) minmax(0, 1fr);
  align-items: start;
  gap: 14px;
  min-height: 0;
  padding: 10px 0 !important;
}

.policy-list .summary-row span,
.policy-list .summary-row strong {
  min-width: 0 !important;
  text-align: left;
  white-space: normal;
  overflow-wrap: anywhere;
  word-break: normal;
  line-height: 1.42;
}

.policy-list .summary-row span {
  color: var(--color-text-muted);
  font-size: 12px;
  font-weight: 800;
}

.policy-list .summary-row strong {
  font-size: 12.5px;
  font-weight: 800;
}

.spotlight-row,
.achievement-row,
.tx-row,
.leader-row {
  align-items: flex-start !important;
}

.spotlight-main small {
  display: block;
  margin-top: 6px;
}

.spotlight-side {
  max-width: 260px;
}

.achievement-row > div:first-child,
.tx-main,
.spotlight-main,
.leader-main {
  min-width: 0;
}

.achievement-row strong,
.spotlight-title,
.tx-title {
  overflow-wrap: anywhere;
  line-height: 1.35;
}

.achievement-points,
.tx-points,
.leader-points {
  flex: 0 0 auto;
  white-space: nowrap;
}

.empty {
  min-height: 84px;
  display: grid;
  place-items: center;
}

@media (max-width: 1180px) {
  .formula-band,
  .rewards-grid {
    grid-template-columns: 1fr !important;
  }

  .formula-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr)) !important;
  }

  .panel-head span {
    max-width: 58%;
  }
}

@media (max-width: 640px) {
  .panel-head {
    flex-direction: column;
    gap: 6px !important;
  }

  .panel-head span {
    max-width: none;
    text-align: left;
  }

  .formula-grid {
    grid-template-columns: 1fr !important;
  }

  .policy-list .summary-row,
  .summary-row {
    grid-template-columns: 1fr;
    gap: 4px;
  }

  .spotlight-row,
  .achievement-row,
  .tx-row,
  .leader-row {
    flex-direction: column;
  }

  .achievement-points,
  .tx-points,
  .leader-points {
    align-self: flex-start;
  }
}

@media (prefers-reduced-motion: reduce) {
  .nexus-feature-header,
  .wallet-card,
  .panel,
  .progress-track span,
  .progress-bar span,
  .level-progress span {
    animation: none !important;
    transition: none !important;
  }
}
</style>




