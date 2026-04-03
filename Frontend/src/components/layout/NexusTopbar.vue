<template>
  <header class="nexus-topbar">
    <div class="nav-left">
      <button class="menu-toggle" @click="$emit('toggle-sidebar')">
        <i class="fa-solid fa-bars"></i>
      </button>
      <router-link to="/dashboard" class="nav-brand">
        <img :src="logoImg" alt="SprintA Logo" class="nav-logo" />
        <span>SprintA</span>
      </router-link>
    </div>

    <div class="nav-center">
      <div class="search-input-wrapper">
        <i class="fa-solid fa-magnifying-glass search-icon"></i>
        <input type="text" placeholder="Search..." v-model="searchQuery" />
      </div>
    </div>

    <div class="nav-right">
      <button class="btn-create-topbar" @click="$emit('toggle-create')">
        <i class="fa-solid fa-plus"></i>
        <span class="btn-text">Tạo mới</span>
      </button>
      
      <button class="btn-ai-topbar" @click="$emit('toggle-ai')">
        <i class="fa-solid fa-robot"></i>
        <span class="btn-text">Ask AI</span>
      </button>

      <NotificationsDropdown class="nav-item" />
      <UserDropdown class="nav-item" />
    </div>
  </header>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import logoImg from '@/assets/logo_QLCV.png'
import NotificationsDropdown from '@/components/NotificationsDropdown.vue'
import UserDropdown from '@/components/UserDropdown.vue'

const router = useRouter()
const searchQuery = ref('')
</script>

<style scoped>
.nexus-topbar {
  height: 64px;
  background-color: var(--bg-nav);
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 24px;
  flex-shrink: 0;
  z-index: 1001;
  border-bottom: 1px solid var(--border-color);
}

.nav-left {
  display: flex;
  align-items: center;
  gap: 16px;
  width: 240px;
}

.menu-toggle {
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--bg-card);
  border: 1px solid var(--border-color);
  color: var(--text-secondary);
  font-size: 16px;
  cursor: pointer;
  width: 36px;
  height: 36px;
  border-radius: 10px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
  transition: all 0.2s ease;
}

.nav-brand {
  display: flex;
  align-items: center;
  gap: 10px;
  color: var(--text-primary);
  text-decoration: none;
  font-weight: 700;
  font-size: 20px;
}

.nav-logo { height: 28px; width: auto; }

.nav-center {
  flex: 1;
  display: flex;
  justify-content: center;
  align-items: center;
  max-width: 500px;
  padding: 0 24px;
}

.search-input-wrapper {
  display: flex;
  align-items: center;
  background-color: var(--bg-secondary);
  border-radius: 9999px;
  padding: 0 16px;
  flex: 1;
  height: 40px;
  transition: all 0.2s ease;
}
.search-input-wrapper:focus-within {
  background-color: var(--bg-nav);
  border: 1px solid #3b82f6;
}
.search-icon { color: var(--text-muted); font-size: 14px; margin-right: 12px; }
.search-input-wrapper input { background: transparent; border: none; color: var(--text-primary); font-size: 14px; width: 100%; outline: none; }

.nav-right {
  display: flex;
  align-items: center;
  gap: 12px;
}

.btn-create-topbar {
  background-color: #3b82f6;
  color: white;
  border: none;
  border-radius: 8px;
  padding: 0 16px;
  height: 36px;
  font-size: 14px;
  font-weight: 600;
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  transition: all 0.2s;
  white-space: nowrap;
}
.btn-create-topbar:hover { background-color: #2563eb; transform: translateY(-1px); }

.btn-ai-topbar {
  display: flex;
  align-items: center;
  gap: 8px;
  background: linear-gradient(135deg, #6366f1 0%, #a855f7 100%);
  color: white;
  border: none;
  border-radius: 8px;
  padding: 0 12px;
  height: 32px;
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
  box-shadow: 0 4px 12px rgba(168, 85, 247, 0.2);
}

@media (max-width: 1024px) {
  .nav-left { width: auto; }
  .nav-center { display: none; }
  .nexus-topbar { padding: 0 16px; }
}

@media (max-width: 640px) {
  .btn-text { display: none; } /* Hide text, keep icon */
  .btn-create-topbar, .btn-ai-topbar { padding: 0; width: 36px; height: 36px; justify-content: center; }
  .btn-create-topbar i, .btn-ai-topbar i { margin: 0; font-size: 16px; }
  .nav-brand span { display: none; } /* Hide "SprintA" text */
}
</style>
