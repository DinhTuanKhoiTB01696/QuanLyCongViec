<template>
  <NexusLayout>
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
          <div v-if="!spotlightTasks.length" class="empty">Chưa có công việc tiêu biểu nào.</div>
          <article v-for="task in spotlightTasks" :key="task.id" class="spotlight-row">
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
          <div class="panel-head"><h2>Thành tích gần đây</h2><span>{{ recentAchievements.length }}</span></div>
          <div v-if="!recentAchievements.length" class="empty">Chưa có thành tích nào gần đây.</div>
          <article v-for="item in recentAchievements" :key="item.id" class="achievement-row">
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
          <div class="panel-head"><h2>Lịch sử điểm</h2><span>{{ transactions.length }}</span></div>
          <div v-if="loading" class="empty">Đang tải dữ liệu...</div>
          <div v-else-if="!transactions.length" class="empty">Chưa có giao dịch điểm nào.</div>
          <article v-for="tx in transactions" :key="tx.id" class="tx-row">
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
  </NexusLayout>
</template>

<script setup>
import { computed, onMounted, ref } from 'vue'
import { ElMessage } from 'element-plus'
import NexusLayout from '@/components/layout/NexusLayout.vue'
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

const formatDate = (value) => (value ? new Date(value).toLocaleString('en-GB') : '')
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
</style>




