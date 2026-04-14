<template>
  <NexusLayout>
    <div class="plane-dashboard">
      <div class="dashboard-inner">
        <!-- Dashboard Header Layer -->
        <header class="db-header">
          <div class="header-content">
            <h1 class="greeting">Good morning, {{ currentUser?.fullName || 'Alo' }}</h1>
            <p class="greeting-sub">🌤️ {{ currentDateTime }}</p>
          </div>
        </header>

        <!-- Main Content Grid -->
        <div class="dashboard-widgets">
          <!-- Setup Guide / Quickstart -->
          <section class="widget-section">
            <div class="section-top">
              <h2 class="section-title">Your quickstart guide</h2>
              <div class="header-action-text"><i class="fa-solid fa-xmark"></i> Not right now</div>
            </div>

            <div class="guide-grid">
              <!-- Card 1 -->
              <div class="guide-card completed">
                <div class="guide-icon"><i class="fa-solid fa-briefcase"></i></div>
                <div class="guide-info">
                  <h3>Create a project</h3>
                  <p>Most things start with a project in Plane.</p>
                </div>
                <div class="guide-action"><i class="fa-solid fa-circle-check" style="color: #10b981; font-size: 16px;"></i></div>
              </div>

              <!-- Card 2 -->
              <div class="guide-card">
                <div class="guide-icon" style="color: #0ea5e9;"><i class="fa-solid fa-user-group"></i></div>
                <div class="guide-info">
                  <h3>Invite your team</h3>
                  <p>Build, ship, and manage with coworkers.</p>
                  <a href="#" class="guide-link">Get them in</a>
                </div>
              </div>

              <!-- Card 3 -->
              <div class="guide-card">
                <div class="guide-icon" style="color: #3b82f6;"><i class="fa-solid fa-laptop"></i></div>
                <div class="guide-info">
                  <h3>Set up your workspace.</h3>
                  <p>Turn features on or off or go beyond that.</p>
                  <a href="#" class="guide-link">Configure this workspace</a>
                </div>
              </div>

              <!-- Card 4 -->
              <div class="guide-card">
                <div class="guide-icon" style="color: #10b981;">C</div>
                <div class="guide-info">
                  <h3>Make Plane yours.</h3>
                  <p>Choose your picture, colors, and more.</p>
                  <a href="#" class="guide-link">Personalize now</a>
                </div>
              </div>
            </div>
          </section>

          <!-- Quicklinks -->
          <section class="widget-section">
            <div class="section-top">
              <h2 class="section-title">Quicklinks</h2>
              <div class="header-action-text text-blue"><i class="fa-solid fa-plus"></i> Add quick link</div>
            </div>
            
            <div class="empty-quicklinks">
              <div class="empty-icon-stack">
                <i class="fa-solid fa-layer-group"></i>
                <i class="fa-solid fa-link chain-icon"></i>
              </div>
              <p>Keep important references, resources, or docs handy for your work</p>
            </div>
          </section>

        </div>
      </div>
    </div>
  </NexusLayout>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import NexusLayout from '@/components/layout/NexusLayout.vue'

const currentUser = ref(null)
const currentDateTime = ref('')

const updateTime = () => {
    const now = new Date()
    const options = { weekday: 'long', month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit', hour12: false }
    currentDateTime.value = now.toLocaleDateString('en-US', options)
}

onMounted(() => {
    const userStr = localStorage.getItem('user');
    if (userStr) {
      currentUser.value = JSON.parse(userStr);
    }
    updateTime()
    setInterval(updateTime, 60000) // Update minute by minute
})
</script>

<style scoped>
.plane-dashboard {
  background-color: #0d0f11;
  min-height: 100vh;
  color: #e4e4e7;
  font-family: 'Inter', -apple-system, sans-serif;
  padding: 48px 64px;
}

.dashboard-inner {
  max-width: 900px;
  margin: 0 auto;
}

.db-header {
  display: flex;
  justify-content: center;
  text-align: center;
  align-items: center;
  margin-bottom: 60px;
}

.greeting {
  font-size: 20px;
  font-weight: 600;
  color: #FFFFFF;
  margin: 0 0 8px 0;
  letter-spacing: -0.3px;
}

.greeting-sub {
  font-size: 13px;
  color: #a1a1aa;
  margin: 0;
  font-weight: 500;
}

.dashboard-widgets {
  display: flex;
  flex-direction: column;
  gap: 40px;
}

.section-top {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
}

.section-title {
  font-size: 14px;
  font-weight: 500;
  color: #e4e4e7;
  margin: 0;
}

.header-action-text {
  font-size: 13px;
  color: #a1a1aa;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 8px;
}
.header-action-text:hover {
  color: #e4e4e7;
}

.text-blue {
  color: #0ea5e9;
}
.text-blue:hover {
  color: #38bdf8;
}

.guide-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
}

.guide-card {
  background-color: #16181d;
  border: 1px solid #1e2025;
  border-radius: 12px;
  padding: 24px;
  display: flex;
  flex-direction: column;
  gap: 16px;
  transition: all 0.2s;
  position: relative;
}

.guide-icon {
  width: 32px;
  height: 32px;
  border-radius: 8px;
  background: #1e2025;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 14px;
  color: #a1a1aa;
}

.guide-info h3 {
  margin: 0 0 8px 0;
  font-size: 14px;
  font-weight: 600;
  color: #e4e4e7;
}

.guide-info p {
  margin: 0 0 12px 0;
  font-size: 12px;
  color: #71717a;
  line-height: 1.5;
}

.guide-link {
  font-size: 13px;
  color: #0ea5e9;
  text-decoration: none;
  font-weight: 500;
}
.guide-link:hover {
  text-decoration: underline;
}

.guide-action {
  position: absolute;
  top: 24px;
  right: 24px;
}

.completed-check {
  background: #10b981;
  border-radius: 50%;
  width: 20px;
  height: 20px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-size: 10px;
}

/* Empty Quicklinks */
.empty-quicklinks {
  background-color: #16181d;
  border: 1px dashed #27272a;
  border-radius: 12px;
  padding: 40px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  text-align: center;
}

.empty-icon-stack {
  position: relative;
  font-size: 32px;
  color: #27272a;
  margin-bottom: 16px;
}

.chain-icon {
  position: absolute;
  bottom: -4px;
  right: -8px;
  font-size: 16px;
  color: #71717a;
  background: #16181d;
  padding: 2px;
  border-radius: 4px;
}

.empty-quicklinks p {
  margin: 0;
  font-size: 13px;
  color: #a1a1aa;
}

@media (max-width: 768px) {
  .guide-grid {
    grid-template-columns: 1fr;
  }
}
</style>
