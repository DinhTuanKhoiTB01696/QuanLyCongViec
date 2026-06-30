<template>
  <div class="starred-page">
    <header class="page-header">
      <h1>Gan sao</h1>
    </header>

    <div class="page-content">
      <div class="filter-controls">
        <div class="search-input-wrapper">
          <i class="fa-solid fa-magnifying-glass search-icon"></i>
          <input v-model="searchQuery" type="text" placeholder="Tim kiem theo tieu de" class="search-input" />
        </div>
        <select v-model="typeFilter" class="filter-select">
          <option value="">Tất cả loại</option>
          <option value="Project">Project</option>
          <option value="Goal">Goal</option>
          <option value="Team">Team</option>
          <option value="User">User</option>
        </select>
      </div>

      <div v-if="starredStore.loading" class="empty-state">Đang tải dữ liệu...</div>

      <div class="starred-grid" v-else-if="filteredStarredItems.length > 0">
        <div class="starred-card" v-for="item in filteredStarredItems" :key="item.id" @click="openItem(item)">
          <div class="card-icon" :class="(item.itemType || 'default').toLowerCase()">
            <i class="fa-solid" :class="getIcon(item.itemType)"></i>
          </div>
          <div class="card-content">
            <h3>{{ item.itemName || 'Untitled' }}</h3>
            <p>{{ item.itemType || 'Item' }}</p>
          </div>
          <button class="unstar-btn" @click.stop="starredStore.toggleStar(item.itemType, item.itemId)" title="Bo gan sao">
            <i class="fa-solid fa-star"></i>
          </button>
        </div>
      </div>

      <div class="empty-state" v-else>
        <div class="empty-illustration"><i class="fa-regular fa-star"></i></div>
        <h2>Chưa có mục nào được gắn sao</h2>
        <p>Nhung muc ban gan sao se xuat hien o day de truy cap nhanh.</p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useStarredStore } from '@/store/useStarredStore'

const router = useRouter()
const starredStore = useStarredStore()
const searchQuery = ref('')
const typeFilter = ref('')

onMounted(async () => {
  await starredStore.fetchStarredItems()
})

const filteredStarredItems = computed(() => {
  const q = searchQuery.value.trim().toLowerCase()
  return (starredStore.starredItems || []).filter(item => {
    const matchesType = !typeFilter.value || item.itemType === typeFilter.value
    const matchesSearch = !q || `${item.itemName || ''} ${item.itemType || ''}`.toLowerCase().includes(q)
    return matchesType && matchesSearch
  })
})

const getIcon = (type) => {
  if (type === 'Project') return 'fa-rocket'
  if (type === 'Goal') return 'fa-bullseye'
  if (type === 'Team') return 'fa-users'
  if (type === 'User') return 'fa-user'
  return 'fa-file-lines'
}

const openItem = (item) => {
  if (item.itemType === 'Project') router.push(`/home/projects/${item.itemId}`)
  if (item.itemType === 'Goal') router.push(`/home/goals/${item.itemId}`)
  if (item.itemType === 'Team') router.push(`/home/teams/${item.itemId}`)
  if (item.itemType === 'User') router.push(`/home/profile/${item.itemId}`)
}
</script>

<style scoped>
.starred-page { color: #172B4D; background: #fff; min-height: 100vh; }
.page-header { padding: 32px 40px 0; }
.page-header h1 { font-size: 24px; font-weight: 500; margin: 0 0 24px; }
.page-content { padding: 0 40px 40px; border-top: 1px solid #DFE1E6; }
.filter-controls { display: flex; gap: 12px; padding: 24px 0; }
.search-input-wrapper { position: relative; width: 260px; }
.search-icon { position: absolute; left: 10px; top: 50%; transform: translateY(-50%); color: #5E6C84; font-size: 14px; }
.search-input, .filter-select { height: 36px; border: 1px solid #DFE1E6; border-radius: 3px; background: #fff; color: #091E42; font-size: 14px; outline: none; }
.search-input { width: 100%; padding: 8px 12px 8px 36px; box-sizing: border-box; }
.filter-select { padding: 0 12px; }
.search-input:focus, .filter-select:focus { border-color: #4C9AFF; }
.starred-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(300px, 1fr)); gap: 24px; }
.starred-card { display: flex; align-items: center; gap: 16px; padding: 16px; border: 1px solid #DFE1E6; border-radius: 3px; background: #fff; cursor: pointer; }
.starred-card:hover { box-shadow: 0 4px 8px rgba(9,30,66,0.1); border-color: #B3D4FF; }
.card-icon { width: 40px; height: 40px; border-radius: 4px; display: flex; align-items: center; justify-content: center; color: white; }
.card-icon.project { background: #0052CC; }
.card-icon.goal { background: #36B37E; }
.card-icon.team { background: #6554C0; }
.card-icon.user { background: #00B8D9; }
.card-icon.default { background: #FFAB00; }
.card-content { flex: 1; overflow: hidden; }
.card-content h3 { margin: 0 0 4px; font-size: 14px; font-weight: 600; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.card-content p { margin: 0; font-size: 12px; color: #5E6C84; }
.unstar-btn { background: transparent; border: 0; color: #FFC400; cursor: pointer; padding: 8px; font-size: 16px; }
.empty-state { padding: 48px 0; text-align: center; color: #5E6C84; }
.empty-illustration { color: #FFC400; font-size: 48px; margin-bottom: 16px; }
.empty-state h2 { color: #172B4D; font-size: 20px; margin: 0 0 8px; }
.empty-state p { margin: 0; }
</style>
