<template>
  <div class="pai-card" :class="`pai-card--${priorityLevel}`" :id="`task-card-${task.id}`">

    <!-- ── LEFT ACCENT BAR ── -->
    <div class="pai-accent-bar"></div>

    <!-- ── HEADER ROW ── -->
    <div class="pai-header">
      <div class="pai-badges">
        <span class="pai-level-badge" :class="`badge--${priorityLevel}`">
          {{ levelEmoji }} {{ levelLabel }}
        </span>
        <span v-if="daysLeft !== null" class="pai-deadline-chip" :class="deadlineClass">
          <i class="fa-regular fa-calendar"></i>
          {{ deadlineLabel }}
        </span>
        <span v-else class="pai-deadline-chip chip-none">
          <i class="fa-regular fa-calendar-xmark"></i>
          {{ t('dailyFocus.aiCardNoDeadline') }}
        </span>
      </div>
      <span class="pai-score-badge" :title="`${t('dailyFocus.aiHintScore')}: ${score}/140`">
        <i class="fa-solid fa-bolt"></i> {{ score }}
      </span>
    </div>

    <!-- ── TITLE ── -->
    <div class="pai-title">{{ task.title }}</div>

    <!-- ── META ── -->
    <div class="pai-meta">
      <span v-if="task.projectName || task.project?.name" class="pai-meta-item">
        <i class="fa-solid fa-briefcase"></i>
        {{ task.projectName || task.project?.name }}
      </span>
      <span v-if="task.statusName" class="pai-meta-item">
        <i class="fa-solid fa-circle-half-stroke"></i>
        {{ task.statusName }}
      </span>
    </div>

    <!-- ── PROGRESS BAR ── -->
    <div class="pai-progress-wrap">
      <div class="pai-progress-track">
        <div
          class="pai-progress-fill"
          :class="`fill--${priorityLevel}`"
          :style="{ width: progressPct + '%' }"
        ></div>
      </div>
      <span class="pai-progress-pct">{{ progressPct }}%</span>
    </div>

    <!-- ── ACTION BUTTONS ── -->
    <div class="pai-actions">
      <button
        :id="`btn-start-${task.id}`"
        class="pai-btn pai-btn--start"
        type="button"
        @click="$emit('start')"
      >
        <i class="fa-solid fa-play"></i> {{ t('dailyFocus.aiCardStart') }}
      </button>
      <button
        :id="`btn-postpone-${task.id}`"
        class="pai-btn pai-btn--postpone"
        type="button"
        :disabled="isPostponed"
        @click="$emit('postpone')"
      >
        <i class="fa-solid fa-clock-rotate-left"></i>
        {{ isPostponed ? t('dailyFocus.aiCardPostponed') : t('dailyFocus.aiCardPostpone') }}
      </button>
      <button
        :id="`btn-ai-hint-${task.id}`"
        class="pai-btn pai-btn--hint"
        type="button"
        @click="$emit('ai-hint')"
      >
        <i class="fa-solid fa-lightbulb"></i> {{ t('dailyFocus.aiCardHint') }}
      </button>
    </div>

  </div>
</template>

<script setup>
import { computed } from 'vue'
import { useI18n } from '@/composables/useI18n'

const { t } = useI18n()

const props = defineProps({
  task: { type: Object, required: true },
  priorityLevel: { type: String, default: 'medium' }, // 'high' | 'medium' | 'low'
  daysLeft: { type: Number, default: null },           // null = no deadline
  score: { type: Number, default: 0 },
  isPostponed: { type: Boolean, default: false }
})

defineEmits(['start', 'postpone', 'ai-hint'])

const levelEmoji = computed(() => {
  if (props.priorityLevel === 'high') return '🔥'
  if (props.priorityLevel === 'medium') return '⚠️'
  return '📋'
})

const levelLabel = computed(() => {
  if (props.priorityLevel === 'high') return t('dailyFocus.aiCardHigh')
  if (props.priorityLevel === 'medium') return t('dailyFocus.aiCardMedium')
  return t('dailyFocus.aiCardLow')
})

const deadlineLabel = computed(() => {
  if (props.daysLeft === null) return ''
  if (props.daysLeft < 0) return t('dailyFocus.aiCardOverdue', { days: Math.abs(props.daysLeft) })
  if (props.daysLeft === 0) return t('dailyFocus.deadlineToday')
  if (props.daysLeft === 1) return t('dailyFocus.deadlineTomorrow')
  return t('dailyFocus.deadlineDays', { days: props.daysLeft })
})

const deadlineClass = computed(() => {
  if (props.daysLeft === null) return 'chip-none'
  if (props.daysLeft <= 0) return 'chip-overdue'
  if (props.daysLeft <= 1) return 'chip-urgent'
  if (props.daysLeft <= 3) return 'chip-warning'
  return 'chip-ok'
})

/** Lấy progress từ assignees hoặc mặc định */
const progressPct = computed(() => {
  if (Array.isArray(props.task.assignees) && props.task.assignees.length) {
    const sum = props.task.assignees.reduce((acc, a) => acc + (a.progressPercent || 0), 0)
    return Math.round(sum / props.task.assignees.length)
  }
  return 0
})
</script>

<style scoped>
/* ════════════════════════════════════════════════
   CARD BASE
════════════════════════════════════════════════ */
.pai-card {
  position: relative;
  display: flex;
  flex-direction: column;
  gap: 10px;
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 12px;
  padding: 16px 18px 16px 22px;
  overflow: hidden;
  transition: box-shadow .2s, transform .2s;
  animation: pai-card-in 350ms cubic-bezier(0.2,0.8,0.2,1) both;
}
.pai-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 12px 32px rgba(0,0,0,.1);
}

@keyframes pai-card-in {
  from { opacity: 0; transform: translateY(10px); }
  to   { opacity: 1; transform: translateY(0); }
}

/* ── Priority variant borders ── */
.pai-card--high  { border-color: color-mix(in srgb, var(--color-danger)  35%, var(--color-border)); }
.pai-card--medium{ border-color: color-mix(in srgb, var(--color-warning) 35%, var(--color-border)); }
.pai-card--low   { border-color: var(--color-border); }

/* ── Left accent bar ── */
.pai-accent-bar {
  position: absolute;
  top: 0; left: 0; bottom: 0;
  width: 4px;
}
.pai-card--high   .pai-accent-bar { background: var(--color-danger); }
.pai-card--medium .pai-accent-bar { background: var(--color-warning); }
.pai-card--low    .pai-accent-bar { background: var(--color-accent); opacity:.5; }

/* ════════════════════════════════════════════════
   HEADER ROW
════════════════════════════════════════════════ */
.pai-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
}

.pai-badges {
  display: flex;
  align-items: center;
  gap: 6px;
  flex-wrap: wrap;
}

/* Level badge */
.pai-level-badge {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  padding: 3px 10px;
  border-radius: 20px;
  font-size: 11px;
  font-weight: 700;
}
.badge--high   { background: color-mix(in srgb,var(--color-danger)  15%,transparent); color: var(--color-danger);  }
.badge--medium { background: color-mix(in srgb,var(--color-warning) 15%,transparent); color: var(--color-warning); }
.badge--low    { background: color-mix(in srgb,var(--color-border)  40%,transparent); color: var(--color-text-muted); }

/* Deadline chip */
.pai-deadline-chip {
  display: inline-flex;
  align-items: center;
  gap: 5px;
  padding: 3px 9px;
  border-radius: 10px;
  font-size: 11px;
  font-weight: 600;
}
.chip-overdue { background: color-mix(in srgb,var(--color-danger)  20%,transparent); color: var(--color-danger);  }
.chip-urgent  { background: color-mix(in srgb,var(--color-danger)  12%,transparent); color: var(--color-danger);  }
.chip-warning { background: color-mix(in srgb,var(--color-warning) 15%,transparent); color: var(--color-warning); }
.chip-ok      { background: color-mix(in srgb,var(--color-success) 12%,transparent); color: var(--color-success); }
.chip-none    { background: color-mix(in srgb,var(--color-border)  40%,transparent); color: var(--color-text-muted); }

/* Score badge */
.pai-score-badge {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  padding: 4px 10px;
  border-radius: 8px;
  font-size: 12px;
  font-weight: 700;
  background: color-mix(in srgb, var(--color-accent) 10%, var(--color-surface));
  color: var(--color-accent);
  border: 1px solid color-mix(in srgb, var(--color-accent) 20%, transparent);
  white-space: nowrap;
}

/* ════════════════════════════════════════════════
   TITLE
════════════════════════════════════════════════ */
.pai-title {
  font-size: 15px;
  font-weight: 700;
  color: var(--color-text-primary);
  line-height: 1.4;
}

/* ════════════════════════════════════════════════
   META
════════════════════════════════════════════════ */
.pai-meta {
  display: flex;
  align-items: center;
  gap: 14px;
}
.pai-meta-item {
  display: flex;
  align-items: center;
  gap: 5px;
  font-size: 12px;
  color: var(--color-text-muted);
}
.pai-meta-item i { font-size: 11px; }

/* ════════════════════════════════════════════════
   PROGRESS BAR
════════════════════════════════════════════════ */
.pai-progress-wrap {
  display: flex;
  align-items: center;
  gap: 10px;
}

.pai-progress-track {
  flex: 1;
  height: 7px;
  background: color-mix(in srgb, var(--color-border) 60%, transparent);
  border-radius: 4px;
  overflow: hidden;
}

.pai-progress-fill {
  height: 100%;
  border-radius: 4px;
  transition: width .5s ease;
  min-width: 4px;
}
.fill--high   { background: linear-gradient(90deg, #ef4444, #f97316); }
.fill--medium { background: linear-gradient(90deg, #f59e0b, #facc15); }
.fill--low    { background: linear-gradient(90deg, var(--color-accent), var(--color-success)); }

.pai-progress-pct {
  font-size: 12px;
  font-weight: 700;
  color: var(--color-text-secondary);
  min-width: 32px;
  text-align: right;
}

/* ════════════════════════════════════════════════
   ACTION BUTTONS
════════════════════════════════════════════════ */
.pai-actions {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
  margin-top: 2px;
}

.pai-btn {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 8px 16px;
  border-radius: 7px;
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
  transition: all .18s;
  border: 1px solid transparent;
}
.pai-btn:disabled { opacity: .45; cursor: not-allowed; }

/* Start */
.pai-btn--start {
  background: var(--color-accent);
  color: #fff;
  box-shadow: 0 2px 8px color-mix(in srgb, var(--color-accent) 35%, transparent);
}
.pai-btn--start:hover:not(:disabled) {
  filter: brightness(1.1);
  box-shadow: 0 4px 14px color-mix(in srgb, var(--color-accent) 45%, transparent);
}

/* Postpone */
.pai-btn--postpone {
  background: transparent;
  border-color: var(--color-border);
  color: var(--color-text-secondary);
}
.pai-btn--postpone:hover:not(:disabled) {
  border-color: var(--color-warning);
  color: var(--color-warning);
  background: color-mix(in srgb, var(--color-warning) 8%, transparent);
}

/* AI Hint */
.pai-btn--hint {
  background: transparent;
  border-color: color-mix(in srgb, var(--color-accent) 30%, transparent);
  color: var(--color-accent);
}
.pai-btn--hint:hover {
  background: color-mix(in srgb, var(--color-accent) 10%, transparent);
}
</style>
