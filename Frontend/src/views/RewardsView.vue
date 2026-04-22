<template>
  <NexusLayout>
    <div class="rewards-page">
      <header class="rewards-header">
        <div>
          <p class="eyebrow">Gamification</p>
          <h1 class="text-hero">Rewards</h1>
          <p class="text-desc">Review and track your career level, contribution share, and early completion bonuses.</p>
        </div>
        <button class="btn-secondary" type="button" :disabled="loading" @click="loadRewards">
          <i class="fa-solid fa-rotate"></i> Refresh
        </button>
      </header>

      <section class="wallet-band">
        <div class="wallet-card">
          <span class="label">Current balance</span>
          <strong>{{ wallet.totalPoints }}</strong>
          <span class="unit">points</span>
        </div>
        <div class="wallet-card">
          <span class="label">Career level</span>
          <strong>{{ career.level }}</strong>
          <span class="unit">{{ career.title }}</span>
        </div>
        <div class="wallet-card wide">
          <div class="wallet-card-head">
            <span class="label">Level progress</span>
            <span class="unit">{{ career.nextThreshold - wallet.totalPoints }} pts to next</span>
          </div>
          <div class="progress-track"><div class="progress-fill" :style="{ width: `${career.progressPercent}%` }"></div></div>
        </div>
      </section>

      <section class="formula-band">
        <div class="panel">
          <div class="panel-head">
            <h2>Point formula</h2>
            <span>{{ formula.expression }}</span>
          </div>
          <div class="formula-grid">
            <div class="formula-cell">
              <span>Gia tri</span>
              <strong>{{ formula.sample.value }}</strong>
            </div>
            <div class="formula-cell">
              <span>Anh huong</span>
              <strong>{{ formula.sample.impact }}</strong>
            </div>
            <div class="formula-cell">
              <span>So ngay</span>
              <strong>{{ formula.sample.days }}</strong>
            </div>
            <div class="formula-cell total">
              <span>Tong diem</span>
              <strong>{{ formula.sample.total }}</strong>
            </div>
          </div>
          <p class="helper-copy">{{ formula.sample.note }}</p>
        </div>

        <div class="panel">
          <div class="panel-head">
            <h2>Summary</h2>
            <span>This sprint</span>
          </div>
          <div class="summary-list">
            <div class="summary-row"><span>Completed tasks</span><strong>{{ summary.completedTasks }}</strong></div>
            <div class="summary-row"><span>Early bonuses</span><strong>{{ summary.earlyBonuses }}</strong></div>
            <div class="summary-row"><span>Contribution share</span><strong>{{ summary.contributionPercent }}%</strong></div>
            <div class="summary-row"><span>Rollback points</span><strong>{{ summary.rollbackPoints }}</strong></div>
          </div>
        </div>
      </section>

      <main class="rewards-grid">
        <section class="panel">
          <div class="panel-head"><h2>Spotlight tasks</h2><span>Top value</span></div>
          <div v-if="!spotlightTasks.length" class="empty">Chua co task nao du de tinh spotlight.</div>
          <article v-for="task in spotlightTasks" :key="task.id" class="spotlight-row">
            <div class="spotlight-main">
              <strong>{{ task.sequenceId || 'TASK' }}</strong>
              <div class="spotlight-title">{{ task.title }}</div>
              <small>{{ task.estimatedDays }} day x share {{ task.contributionShare }}%</small>
            </div>
            <div class="spotlight-side">
              <span class="chip">{{ task.formulaPoints }} pts</span>
              <span class="chip muted">{{ task.progressPercent }}%</span>
            </div>
          </article>
        </section>

        <section class="panel">
          <div class="panel-head"><h2>Recent achievements</h2><span>{{ recentAchievements.length }}</span></div>
          <div v-if="!recentAchievements.length" class="empty">Chua co achievement moi.</div>
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
          <div class="panel-head"><h2>Point history</h2><span>{{ transactions.length }}</span></div>
          <div v-if="loading" class="empty">Loading rewards...</div>
          <div v-else-if="!transactions.length" class="empty">Chua co giao dich diem nao.</div>
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
          <div class="panel-head"><h2>Leaderboard</h2><span>Top 20</span></div>
          <div v-if="!leaderboard.length" class="empty">Chua co bang xep hang.</div>
          <article v-for="(item, index) in leaderboard" :key="item.userId" class="leader-row">
            <span class="rank">#{{ index + 1 }}</span>
            <span class="avatar">{{ getInitials(item.userName) }}</span>
            <div class="leader-main">
              <strong>{{ item.userName || 'User' }}</strong>
              <small>{{ item.careerTitle || `Level ${item.level}` }}</small>
            </div>
            <span class="leader-points">{{ item.totalPoints }} pts</span>
          </article>
        </section>
      </section>
    </div>
  </NexusLayout>
</template>

<script setup>
import { onMounted, ref } from 'vue'
import { ElMessage } from 'element-plus'
import NexusLayout from '@/components/layout/NexusLayout.vue'
import axiosClient from '@/api/axiosClient'

const loading = ref(false)
const wallet = ref({ totalPoints: 0, level: 1, nextLevelAt: 1000 })
const career = ref({ level: 1, title: 'Contributor', progressPercent: 0, nextThreshold: 1000 })
const formula = ref({ expression: 'Gia tri x Anh huong x So ngay', sample: { value: 0, impact: 0, days: 0, total: 0, note: '' } })
const summary = ref({ completedTasks: 0, earlyBonuses: 0, contributionPercent: 0, rollbackPoints: 0 })
const spotlightTasks = ref([])
const recentAchievements = ref([])
const transactions = ref([])
const leaderboard = ref([])

const formatDate = (value) => (value ? new Date(value).toLocaleString('vi-VN') : '')

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
    wallet.value = data.wallet || wallet.value
    career.value = data.career || career.value
    formula.value = data.formula || formula.value
    summary.value = data.summary || summary.value
    spotlightTasks.value = data.spotlightTasks || []
    recentAchievements.value = data.recentAchievements || []
    transactions.value = data.transactions || []
    leaderboard.value = leaders.data?.data || []
  } catch (error) {
    ElMessage.error(error.response?.data?.message || 'Khong tai duoc diem thuong.')
  } finally {
    loading.value = false
  }
}

onMounted(loadRewards)
</script>

<style scoped>
.rewards-page {
  min-height: calc(100vh - 56px);
  background: var(--color-bg);
  color: var(--color-text-primary);
  padding: 32px;
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
  margin-bottom: 24px;
}

.eyebrow {
  color: var(--color-accent);
  font-size: 12px;
  text-transform: uppercase;
  font-weight: 700;
  margin: 0 0 8px;
}

h1 {
  margin: 0;
  font-size: 28px;
}

.muted,
time,
small,
.helper-copy {
  color: var(--color-text-secondary);
}

.refresh-btn {
  border: 1px solid var(--border-color);
  border-radius: 2px;
  background: var(--bg-secondary);
  color: var(--text-primary);
  padding: 8px 12px;
  cursor: pointer;
}

.wallet-band,
.formula-band,
.rewards-grid {
  gap: 16px;
}

.wallet-band {
  margin-bottom: 20px;
}

.wallet-card,
.panel {
  border: 1px solid var(--border-color);
  border-radius: 2px;
  background: var(--bg-secondary);
  box-shadow: var(--shadow-sm);
}

.wallet-card {
  flex: 1;
  padding: 18px 20px;
}

.wallet-card.wide {
  flex: 2;
}

.label {
  display: block;
  color: var(--color-text-secondary);
  font-size: 12px;
  margin-bottom: 8px;
}

.wallet-card strong,
.formula-cell strong {
  font-size: 28px;
}

.unit {
  color: var(--color-text-muted);
  margin-left: 8px;
}

.progress-track {
  height: 10px;
  background: var(--border-color);
  border-radius: 1px;
  overflow: hidden;
}

.progress-fill {
  height: 100%;
  background: linear-gradient(90deg, var(--color-success), var(--color-accent));
}

.formula-band,
.rewards-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  margin-bottom: 20px;
}

.lower-grid {
  margin-bottom: 0;
}

.panel {
  overflow: hidden;
}

.panel-head {
  padding: 14px 16px;
  border-bottom: 1px solid var(--color-border);
}

.panel-head h2 {
  margin: 0;
  font-size: 15px;
}

.formula-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 12px;
  padding: 18px 16px 0;
}

.formula-cell {
  padding: 12px;
  border: 1px solid var(--border-color);
  border-radius: 2px;
  background: var(--bg-primary);
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
  padding: 16px;
}

.summary-row {
  padding: 10px 0;
  border-bottom: 1px solid var(--color-border);
}

.summary-row:last-child {
  border-bottom: 0;
}

.spotlight-row,
.achievement-row,
.tx-row,
.leader-row {
  gap: 12px;
  padding: 14px 16px;
  border-bottom: 1px solid var(--color-border);
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
}

.spotlight-side {
  display: flex;
  gap: 8px;
}

.chip {
  padding: 4px 8px;
  border-radius: 2px;
  background: var(--color-success-bg);
  color: var(--color-success);
  font-size: 12px;
  font-weight: 600;
}

.chip.muted {
  background: var(--hover-bg);
  color: var(--text-secondary);
}

.tx-icon,
.avatar {
  width: 28px;
  height: 28px;
  display: grid;
  place-items: center;
  border-radius: 6px;
}

.tx-icon {
  background: var(--color-success-bg);
  color: var(--color-success);
}

.tx-icon.negative {
  background: var(--color-danger-bg);
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
}

.tx-points.negative {
  color: var(--color-danger);
}

.rank {
  width: 36px;
  color: var(--color-text-muted);
}

.avatar {
  background: var(--color-accent);
  color: #ffffff;
  font-size: 11px;
  font-weight: 700;
}

.leader-main strong {
  display: block;
  font-size: 14px;
}

.leader-points {
  color: var(--color-warning);
  font-weight: 700;
}

.empty {
  text-align: center;
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




