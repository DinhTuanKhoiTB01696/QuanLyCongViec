<template>
  <div class="teams-wrapper">
    <header class="module-header">
      <div class="header-content">
        <h1>{{ t('homeSite.teams.title') }}</h1>
        <div class="header-actions">
          <button class="primary-btn" @click="openCreateTeam">
            {{ t('homeSite.teams.startTeam') }}
          </button>
        </div>
      </div>

      <div class="tabs-nav">
        <router-link to="/home/teams" class="tab-link" exact-active-class="active">
          {{ t('homeSite.teams.forYou') }}
        </router-link>
        <router-link to="/home/teams/list" class="tab-link" active-class="active">
          {{ t('homeSite.teams.allTeams') }}
        </router-link>
        <router-link to="/home/teams/kudos" class="tab-link" active-class="active">
          {{ t('homeSite.teams.kudos') }}
        </router-link>
        <router-link to="/home/people" class="tab-link">
          {{ t('homeSite.teams.everyone') }}
        </router-link>
      </div>
    </header>

    <div class="module-content">
      <router-view></router-view>
    </div>
  </div>
</template>

<script setup>
import { useI18nStore } from '@/store/useI18nStore'

const i18nStore = useI18nStore()
const t = i18nStore.t

const openCreateTeam = () => {
  window.dispatchEvent(new CustomEvent('global-create-click'))
}
</script>

<style scoped>
.teams-wrapper {
  display: flex;
  flex-direction: column;
  min-height: 100%;
}

.module-header {
  padding: 32px 40px 0;
  background-color: #ffffff;
  position: sticky;
  top: 0;
  z-index: 5;
}

.header-content {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
}

.header-content h1 {
  font-size: 24px;
  font-weight: 500;
  color: #172b4d;
  margin: 0;
}

.primary-btn {
  background-color: #0052cc;
  color: white;
  border: none;
  padding: 8px 16px;
  border-radius: 3px;
  font-weight: 500;
  font-size: 14px;
  cursor: pointer;
  transition: background-color 0.2s;
}

.primary-btn:hover {
  background-color: #0047b3;
}

.tabs-nav {
  display: flex;
  border-bottom: 2px solid #dfe1e6;
  gap: 24px;
}

.tab-link {
  padding: 8px 0 12px;
  font-size: 14px;
  font-weight: 500;
  color: #5e6c84;
  text-decoration: none;
  position: relative;
  margin-bottom: -2px;
  border-bottom: 2px solid transparent;
  transition: color 0.2s;
}

.tab-link:hover {
  color: #172b4d;
}

.tab-link.active {
  color: #0052cc;
  border-bottom-color: #0052cc;
}

.module-content {
  padding: 32px 40px;
  flex: 1;
}
</style>
