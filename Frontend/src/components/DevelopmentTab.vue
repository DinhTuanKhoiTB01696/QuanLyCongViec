<template>
  <section class="development-tab">
    <h2>{{ t('workItems.development.keyMetrics') }} <span>{{ t('workItems.development.beta') }}</span></h2>

    <div class="metric-row primary">
      <article v-for="metric in keyMetrics" :key="metric.key" class="metric-card">
        <h3>{{ metric.label }} <i v-if="metric.needsData" class="fa-solid fa-circle-info" :title="metric.note"></i></h3>
        <strong>{{ metric.value }}</strong>
        <p>{{ metric.caption }}</p>
      </article>
    </div>

    <div class="metric-row secondary">
      <article v-for="metric in secondaryMetrics" :key="metric.key" class="metric-card compact">
        <h3>{{ metric.label }} <i v-if="metric.needsData" class="fa-solid fa-circle-info" :title="metric.note"></i></h3>
        <strong>{{ metric.value }}</strong>
        <p>{{ metric.caption }}</p>
      </article>
    </div>

    <div class="related-head">
      <h2>{{ t('workItems.development.relatedWork') }}</h2>
      <div class="related-tabs" role="tablist" :aria-label="t('workItems.development.relatedWork')">
        <button
          v-for="tab in relatedTabs"
          :key="tab.key"
          type="button"
          :class="{ active: tab.key === activeIntegrationTab }"
          disabled
          :title="t('workItems.development.tabNeedsFlow')"
        >
          {{ tab.label }}
        </button>
      </div>
    </div>

    <article class="connect-state">
      <div class="connect-art" aria-hidden="true">
        <div class="work-card">
          <span></span>
          <span></span>
          <small>kan-251</small>
          <i class="fa-regular fa-user"></i>
        </div>
        <div class="terminal-card">$ git commit -m "Kan-1 Update"</div>
      </div>

      <h3>{{ t('workItems.development.connectTitle') }}</h3>
      <p>{{ t('workItems.development.connectHelp') }}</p>
      <button
        class="connect-btn"
        type="button"
        disabled
        :title="t('workItems.development.connectNeedsFlow')"
      >
        {{ t('workItems.development.connectNewTool') }}
        <i class="fa-solid fa-chevron-down"></i>
      </button>
      <span class="needs-flow">{{ t('workItems.development.connectNeedsFlow') }}</span>
    </article>

    <div class="feedback-banner">
      <i class="fa-solid fa-bullhorn"></i>
      <span>{{ t('workItems.development.feedbackPrompt') }}</span>
      <button type="button" disabled :title="t('workItems.development.feedbackNeedsFlow')">
        {{ t('workItems.development.giveFeedback') }}
      </button>
    </div>
  </section>
</template>

<script setup>
import { computed } from 'vue'
import { useI18n } from '@/composables/useI18n'

const props = defineProps({
  tasks: { type: Array, default: () => [] }
})

const { t } = useI18n()
const activeIntegrationTab = 'pullRequests'

const keyMetrics = computed(() => [
  {
    key: 'completed',
    label: t('workItems.development.workItemsCompletedThisWeek'),
    value: completedThisWeek.value,
    caption: t('workItems.development.completedThisWeek')
  },
  {
    key: 'pullRequestCycle',
    label: t('workItems.development.pullRequestCycleTime'),
    value: 0,
    caption: t('workItems.development.rollingMedian'),
    needsData: true,
    note: t('workItems.development.integrationNeedsData')
  },
  {
    key: 'leadTime',
    label: t('workItems.development.leadTimeForChanges'),
    value: 0,
    caption: t('workItems.development.rollingAverage'),
    needsData: true,
    note: t('workItems.development.integrationNeedsData')
  },
  {
    key: 'deploymentFrequency',
    label: t('workItems.development.deploymentFrequency'),
    value: 0,
    caption: t('workItems.development.weeklyAverage'),
    needsData: true,
    note: t('workItems.development.integrationNeedsData')
  }
])

const secondaryMetrics = computed(() => [
  {
    key: 'overdue',
    label: t('workItems.development.workItems'),
    value: overdueCount.value,
    caption: t('workItems.development.overdue')
  },
  {
    key: 'reopened',
    label: t('workItems.development.workItems'),
    value: 0,
    caption: t('workItems.development.reopened'),
    needsData: true,
    note: t('workItems.development.reopenedNeedsData')
  },
  {
    key: 'bugsOpen',
    label: t('workItems.development.bugs'),
    value: bugsOpen.value,
    caption: t('workItems.development.open')
  },
  {
    key: 'pullRequests',
    label: t('workItems.development.pullRequests'),
    value: 0,
    caption: t('workItems.development.open'),
    needsData: true,
    note: t('workItems.development.integrationNeedsData')
  },
  {
    key: 'vulnerabilities',
    label: t('workItems.development.vulnerabilities'),
    value: 0,
    caption: t('workItems.development.critical'),
    needsData: true,
    note: t('workItems.development.integrationNeedsData')
  }
])

const relatedTabs = computed(() => [
  { key: 'pullRequests', label: t('workItems.development.tabs.pullRequests') },
  { key: 'repositories', label: t('workItems.development.tabs.repositories') },
  { key: 'vulnerabilities', label: t('workItems.development.tabs.vulnerabilities') },
  { key: 'deployments', label: t('workItems.development.tabs.deployments') },
  { key: 'workSuggestions', label: t('workItems.development.tabs.workSuggestions') }
])

const completedThisWeek = computed(() => {
  const start = startOfWeek(new Date())
  const end = endOfDay(new Date())
  return props.tasks.filter((task) => {
    const completedAt = toDate(task.completedAt || task.resolvedAt || task.updatedAt)
    return isDone(task) && completedAt && completedAt >= start && completedAt <= end
  }).length
})

const overdueCount = computed(() => {
  const today = startOfDay(new Date())
  return props.tasks.filter((task) => {
    const dueDate = toDate(task.dueDate)
    return dueDate && startOfDay(dueDate) < today && !isDone(task)
  }).length
})

const bugsOpen = computed(() => props.tasks.filter((task) => isBug(task) && !isDone(task)).length)

function isBug(task) {
  const type = `${task.issueType || task.workItemType || task.taskType || task.type || ''}`.trim().toLowerCase()
  return type === 'bug' || type.includes('bug') || type.includes('lỗi') || type.includes('loi')
}

function isDone(task) {
  const status = `${task.statusName || ''}`.toUpperCase().replace(/\s+/g, ' ').trim()
  return status === 'DONE' || status.includes('COMPLETE') || status.includes('HOÀN THÀNH') || status.includes('HOAN THANH')
}

function toDate(value) {
  if (!value) return null
  const date = new Date(value)
  return Number.isNaN(date.getTime()) ? null : date
}

function startOfDay(value) {
  const date = new Date(value)
  date.setHours(0, 0, 0, 0)
  return date
}

function endOfDay(value) {
  const date = new Date(value)
  date.setHours(23, 59, 59, 999)
  return date
}

function startOfWeek(value) {
  const date = startOfDay(value)
  const day = (date.getDay() + 6) % 7
  date.setDate(date.getDate() - day)
  return date
}
</script>

<style scoped>
.development-tab {
  flex: 1;
  min-height: 0;
  overflow: auto;
  padding: 28px 24px 40px;
  background: var(--color-bg);
  color: var(--color-text-primary);
}

.development-tab h2 {
  margin: 0 0 8px;
  font-size: 14px;
  font-weight: 700;
  color: var(--color-text-primary);
}

.development-tab h2 span {
  margin-left: 4px;
  padding: 1px 4px;
  border-radius: 3px;
  background: #e9d7fe;
  color: #6e5dc6;
  font-size: 9px;
  font-weight: 700;
}

.metric-row {
  display: grid;
  gap: 8px;
  margin-bottom: 10px;
}

.metric-row.primary {
  grid-template-columns: repeat(4, minmax(160px, 1fr));
  max-width: 920px;
}

.metric-row.secondary {
  grid-template-columns: repeat(5, minmax(120px, 1fr));
  max-width: 740px;
  margin-bottom: 28px;
}

.metric-card {
  min-height: 78px;
  padding: 12px 14px;
  border: 1px solid var(--color-border);
  border-radius: 6px;
  background: var(--color-surface);
}

.metric-card.compact {
  min-height: 58px;
}

.metric-card h3 {
  display: flex;
  align-items: center;
  gap: 4px;
  margin: 0 0 6px;
  color: var(--color-text-primary);
  font-size: 12px;
  font-weight: 700;
}

.metric-card h3 i {
  color: var(--color-text-muted);
  font-size: 10px;
}

.metric-card strong {
  display: block;
  color: var(--color-text-primary);
  font-size: 18px;
  line-height: 1.1;
}

.metric-card p {
  margin: 4px 0 0;
  color: var(--color-text-secondary);
  font-size: 11px;
}

.related-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  margin-bottom: 24px;
}

.related-head h2 {
  margin: 0;
}

.related-tabs {
  display: inline-flex;
  align-items: center;
  border: 1px solid var(--color-border);
  border-radius: 4px;
  overflow: hidden;
  background: var(--color-surface);
}

.related-tabs button {
  height: 26px;
  padding: 0 12px;
  border: 0;
  border-right: 1px solid var(--color-border);
  background: transparent;
  color: var(--color-text-secondary);
  font-size: 11px;
}

.related-tabs button:last-child {
  border-right: 0;
}

.related-tabs button.active {
  background: var(--color-bg);
  color: #0c66e4;
}

.related-tabs button:disabled {
  cursor: not-allowed;
}

.connect-state {
  min-height: 300px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  text-align: center;
}

.connect-art {
  position: relative;
  width: 230px;
  height: 130px;
  margin-bottom: 18px;
}

.work-card {
  position: absolute;
  left: 30px;
  top: 2px;
  width: 175px;
  height: 70px;
  padding: 12px;
  border: 8px solid #0c66e4;
  border-left-color: #bf63f3;
  background: var(--color-surface);
  box-shadow: -18px 5px 0 #bf63f3;
  text-align: left;
  transform: rotate(-1deg);
}

.work-card span {
  display: block;
  width: 75px;
  height: 6px;
  margin-bottom: 9px;
  border-radius: 99px;
  background: #8993a4;
}

.work-card span:nth-child(2) {
  width: 118px;
  height: 4px;
  background: #dfe1e6;
}

.work-card small {
  position: absolute;
  left: 18px;
  bottom: 14px;
  color: var(--color-text-secondary);
  font-size: 11px;
}

.work-card small::before {
  content: "";
  display: inline-block;
  width: 10px;
  height: 10px;
  margin-right: 4px;
  background: #57d9a3;
}

.work-card i {
  position: absolute;
  right: 10px;
  bottom: 10px;
  width: 30px;
  height: 30px;
  border-radius: 50%;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  background: #dfe1e6;
  color: #6b778c;
}

.terminal-card {
  position: absolute;
  left: 44px;
  bottom: 8px;
  width: 172px;
  padding: 12px 14px;
  border-radius: 3px;
  background: #172b4d;
  color: #57d9a3;
  font-family: Consolas, monospace;
  font-size: 11px;
  text-align: left;
}

.connect-state h3 {
  margin: 0 0 12px;
  font-size: 18px;
  color: var(--color-text-primary);
}

.connect-state p {
  max-width: 380px;
  margin: 0 0 16px;
  color: var(--color-text-secondary);
  font-size: 13px;
  line-height: 1.5;
}

.connect-btn {
  height: 30px;
  display: inline-flex;
  align-items: center;
  gap: 8px;
  border: 1px solid var(--color-border);
  border-radius: 4px;
  background: var(--color-surface);
  color: var(--color-text-primary);
  padding: 0 12px;
  font-size: 12px;
}

.connect-btn:disabled {
  cursor: not-allowed;
  opacity: 0.75;
}

.needs-flow {
  margin-top: 10px;
  color: #0c66e4;
  font-size: 12px;
}

.feedback-banner {
  min-height: 52px;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  margin-top: 24px;
  border: 1px solid var(--color-border);
  background: var(--color-surface);
  color: var(--color-text-primary);
  font-size: 12px;
}

.feedback-banner i {
  color: #ffab00;
}

.feedback-banner button {
  border: 0;
  background: transparent;
  color: #0c66e4;
  padding: 0;
  font-size: 12px;
}

.feedback-banner button:disabled {
  cursor: not-allowed;
  opacity: 0.7;
}

@media (max-width: 1100px) {
  .metric-row.primary,
  .metric-row.secondary {
    grid-template-columns: repeat(2, minmax(160px, 1fr));
    max-width: none;
  }

  .related-head {
    align-items: flex-start;
    flex-direction: column;
  }

  .related-tabs {
    max-width: 100%;
    overflow-x: auto;
  }
}

@media (max-width: 680px) {
  .metric-row.primary,
  .metric-row.secondary {
    grid-template-columns: 1fr;
  }
}
</style>
