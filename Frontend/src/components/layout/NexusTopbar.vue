<template>
  <header class="plane-topbar">
    <div class="nav-left">
      <div class="workspace-switcher" @click="$router.push('/spaces')">
        <div class="ws-icon">C</div>
        <span class="ws-name">Cun</span>
        <i class="fa-solid fa-chevron-down"></i>
      </div>
      <button class="menu-toggle" @click="$emit('toggle-sidebar')">
        <i class="fa-solid fa-bars-staggered"></i>
      </button>
    </div>

    <div class="nav-center">
      <div class="search-input-wrapper">
        <i class="fa-solid fa-magnifying-glass search-icon"></i>
        <input type="text" placeholder="Search commands..." v-model="searchQuery" />
      </div>
    </div>

    <div class="nav-right">
      <div class="help-btn" @click="$emit('toggle-ai')">
         <i class="fa-solid fa-robot"></i>
      </div>
      <a href="https://github.com/plane" target="_blank" class="github-btn">
         <i class="fa-brands fa-github"></i>
         Star us on GitHub
      </a>
      <UserDropdown class="nav-item" />
    </div>
  </header>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import UserDropdown from '@/components/UserDropdown.vue'

const router = useRouter()
const searchQuery = ref('')
</script>

<style scoped>
.plane-topbar {
  height: 52px;
  background-color: #0d0f11;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 16px;
  flex-shrink: 0;
  z-index: 1001;
  border-bottom: 1px solid #1e2025;
}

.nav-left {
  display: flex;
  align-items: center;
  gap: 16px;
  width: 250px;
}

.workspace-switcher {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 4px 8px;
  border-radius: 6px;
  cursor: pointer;
  transition: background 0.2s;
}

.workspace-switcher:hover {
  background: #1e2025;
}

.ws-icon {
  width: 20px;
  height: 20px;
  border-radius: 4px;
  background: #0ea5e9;
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 11px;
  font-weight: 600;
}

.ws-name {
  color: #e4e4e7;
  font-size: 14px;
  font-weight: 500;
}

.workspace-switcher i {
  color: #71717a;
  font-size: 10px;
  margin-left: 2px;
}

.menu-toggle {
  display: none;
  background: transparent;
  border: none;
  color: #a1a1aa;
  cursor: pointer;
  padding: 4px;
}

.nav-center {
  flex: 1;
  display: flex;
  justify-content: center;
  align-items: center;
}

.search-input-wrapper {
  display: flex;
  align-items: center;
  background-color: #1e2025;
  border: 1px solid #27272a;
  border-radius: 6px;
  padding: 0 12px;
  width: 480px;
  height: 32px;
  transition: all 0.2s ease;
}

.search-input-wrapper:focus-within {
  border-color: #3f3f46;
  background-color: #181a1f;
}

.search-icon { 
  color: #71717a; 
  font-size: 13px; 
  margin-right: 8px; 
}

.search-input-wrapper input { 
  background: transparent; 
  border: none; 
  color: #e4e4e7; 
  font-size: 13px; 
  width: 100%; 
  outline: none; 
}

.nav-right {
  display: flex;
  align-items: center;
  gap: 16px;
}

.help-btn {
  color: #a1a1aa;
  font-size: 15px;
  cursor: pointer;
}
.help-btn:hover {
  color: #e4e4e7;
}

.github-btn {
  display: flex;
  align-items: center;
  gap: 8px;
  color: #e4e4e7;
  text-decoration: none;
  font-size: 12px;
  font-weight: 500;
  padding: 6px 12px;
  border-radius: 6px;
  border: 1px solid #27272a;
  background: #1e2025;
  transition: background 0.2s;
}

.github-btn:hover {
  background: #27272a;
}
.github-btn i { font-size: 14px; }

@media (max-width: 1024px) {
  .menu-toggle { display: block; }
  .nav-center { display: none; }
}
</style>
