<template>
  <div class="history-tab">
    <div class="tab-header">
      <h3 class="tab-title">Audit Log</h3>
      <div class="header-controls">
        <div class="search-box">
          <Search class="search-icon" />
          <input type="text" placeholder="Filter history..." />
        </div>
      </div>
    </div>

    <div class="timeline-container">
      <div class="timeline-item" v-for="(event, index) in historyEvents" :key="index">
        <div class="timeline-time">
          <span class="date">{{ event.date }}</span>
          <span class="time">{{ event.time }}</span>
        </div>
        
        <div class="timeline-marker">
          <div class="marker-dot"></div>
          <div class="marker-line" v-if="index !== historyEvents.length - 1"></div>
        </div>
        
        <div class="timeline-content">
          <div class="event-card">
            <div class="event-header">
              <div class="actor">
                <div class="avatar">{{ event.actor.charAt(0) }}</div>
                <span class="actor-name">{{ event.actor }}</span>
              </div>
              <span class="action-type" :class="event.type">{{ event.action }}</span>
            </div>
            
            <div class="event-details" v-if="event.details">
              <div class="detail-row" v-if="event.details.before || event.details.after">
                <div class="detail-col before" v-if="event.details.before">
                  <span class="detail-label">Before</span>
                  <div class="detail-val">{{ event.details.before }}</div>
                </div>
                <div class="detail-arrow" v-if="event.details.before && event.details.after">
                  <ArrowRight class="icon-sm" />
                </div>
                <div class="detail-col after" v-if="event.details.after">
                  <span class="detail-label">After</span>
                  <div class="detail-val">{{ event.details.after }}</div>
                </div>
              </div>
              <div class="detail-text" v-else>
                {{ event.details.text }}
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue';
import { Search, ArrowRight } from 'lucide-vue-next';

defineProps({
  role: { type: Object, required: true }
});

const historyEvents = ref([
  {
    date: 'Today',
    time: '14:30',
    actor: 'John Kenwin',
    action: 'Changed permissions',
    type: 'update',
    details: {
      before: 'Granted: Xem dự án',
      after: 'Granted: Xem dự án, Tạo dự án'
    }
  },
  {
    date: 'Yesterday',
    time: '09:15',
    actor: 'Admin System',
    action: 'Added member',
    type: 'add',
    details: {
      text: 'Added Sarah Smith to this role.'
    }
  },
  {
    date: '24 May 2023',
    time: '11:00',
    actor: 'John Kenwin',
    action: 'Created role',
    type: 'create',
    details: {
      text: 'Role Administrator was created from template.'
    }
  }
]);
</script>

<style scoped>
.history-tab {
  display: flex;
  flex-direction: column;
  gap: 24px;
  max-width: 800px;
}

.tab-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.tab-title {
  font-size: 18px;
  font-weight: 600;
  color: #f8fafc;
  margin: 0;
}

.search-box {
  position: relative;
  width: 240px;
}

.search-icon {
  position: absolute;
  left: 10px;
  top: 50%;
  transform: translateY(-50%);
  color: #64748b;
  width: 14px;
  height: 14px;
}

.search-box input {
  width: 100%;
  background: #161922;
  border: 1px solid #2d313f;
  border-radius: 6px;
  padding: 8px 10px 8px 32px;
  color: #f8fafc;
  font-size: 13px;
  outline: none;
}

.search-box input:focus {
  border-color: #3b82f6;
}

.timeline-container {
  padding-left: 10px;
}

.timeline-item {
  display: flex;
  gap: 20px;
  position: relative;
  min-height: 80px;
}

.timeline-time {
  width: 80px;
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  padding-top: 4px;
}

.timeline-time .date {
  font-size: 12px;
  font-weight: 600;
  color: #94a3b8;
}

.timeline-time .time {
  font-size: 11px;
  color: #64748b;
}

.timeline-marker {
  position: relative;
  display: flex;
  flex-direction: column;
  align-items: center;
}

.marker-dot {
  width: 12px;
  height: 12px;
  border-radius: 50%;
  background: #3b82f6;
  border: 3px solid #11131a;
  box-shadow: 0 0 0 1px #222631;
  z-index: 2;
  margin-top: 6px;
}

.marker-line {
  position: absolute;
  top: 18px;
  bottom: -6px;
  width: 2px;
  background: #222631;
  z-index: 1;
}

.timeline-content {
  flex: 1;
  padding-bottom: 30px;
}

.event-card {
  background: #161922;
  border: 1px solid #222631;
  border-radius: 8px;
  padding: 16px;
}

.event-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
}

.actor {
  display: flex;
  align-items: center;
  gap: 8px;
}

.avatar {
  width: 24px;
  height: 24px;
  border-radius: 50%;
  background: #475569;
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 11px;
  font-weight: 600;
}

.actor-name {
  font-size: 14px;
  font-weight: 500;
  color: #f8fafc;
}

.action-type {
  font-size: 12px;
  color: #cbd5e1;
  background: rgba(255, 255, 255, 0.05);
  padding: 2px 8px;
  border-radius: 4px;
}

.action-type.update { color: #3b82f6; background: rgba(59, 130, 246, 0.1); }
.action-type.add { color: #10b981; background: rgba(16, 185, 129, 0.1); }
.action-type.create { color: #8b5cf6; background: rgba(139, 92, 246, 0.1); }

.event-details {
  background: #0B0D14;
  border: 1px solid #1a1e29;
  border-radius: 6px;
  padding: 12px;
}

.detail-row {
  display: flex;
  align-items: center;
  gap: 16px;
}

.detail-col {
  flex: 1;
}

.detail-label {
  font-size: 11px;
  color: #64748b;
  text-transform: uppercase;
  margin-bottom: 4px;
  display: block;
}

.detail-val {
  font-size: 13px;
  color: #cbd5e1;
  word-break: break-word;
}

.detail-col.before .detail-val {
  color: #ef4444;
  text-decoration: line-through;
}

.detail-col.after .detail-val {
  color: #10b981;
}

.detail-arrow {
  color: #64748b;
}

.detail-text {
  font-size: 13px;
  color: #cbd5e1;
}

.icon-sm { width: 16px; height: 16px; }
</style>
