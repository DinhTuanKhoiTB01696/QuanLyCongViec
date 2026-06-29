<template>
  <div class="recent-page">
    <header class="page-header">
      <h1>{{ t('Recent') }}</h1>

      <div class="tabs">
        <button class="tab-btn" :class="{ active: activeTab === 'worked' }" @click="activeTab = 'worked'">{{ t('Worked on') }}</button>
        <button class="tab-btn" :class="{ active: activeTab === 'viewed' }" @click="activeTab = 'viewed'">{{ t('Viewed') }}</button>
      </div>
    </header>

    <div class="page-content">
      <div class="filter-container">
        <div class="search-input-wrapper">
          <i class="fa-solid fa-magnifying-glass search-icon"></i>
          <input v-model="searchQuery" type="text" :placeholder="t('Filter by title')" class="search-input" />
        </div>
      </div>

      <div class="audit-list" v-if="!isLoading">
        <div class="time-group" v-for="group in groupedActivities" :key="group.label">
          <h3 class="time-label">{{ t(group.label) }}</h3>
          <div class="audit-item" v-for="act in group.items" :key="act.id" :class="{ clickable: act.url }" @click="goToItem(act)">
            <div class="item-icon light-blue square"><i :class="act.icon"></i></div>
            <div class="item-details">
              <div class="item-title">{{ act.bold || act.text }}</div>
              <div class="item-path">{{ act.text }}</div>
            </div>
            <div class="item-meta">
              <span class="status-badge done">DONE</span>
              <span class="time-ago">{{ act.time }}</span>
            </div>
          </div>
        </div>

        <div v-if="filteredItems.length === 0" class="empty-state">
          Không có hoạt động nào gần đây.
        </div>
      </div>
      <div v-else class="empty-state">
        Đang tải dữ liệu...
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useI18nStore } from '@/store/useI18nStore'
import { useActivityStore } from '@/store/useActivityStore'
import axiosClient from '@/api/axiosClient'

const i18nStore = useI18nStore()
const t = (key) => i18nStore.t(key)

const activityStore = useActivityStore()
const router = useRouter()
const activeTab = ref('worked')
const searchQuery = ref('')
const recentViews = ref([])
const recentLoading = ref(false)

const fetchRecentViews = async () => {
  recentLoading.value = true
  try {
    const res = await axiosClient.get('/recentviews', { params: { limit: 50 } })
    recentViews.value = (res.data?.data || []).map(item => ({
      id: item.id,
      icon: item.icon || 'fa-regular fa-eye',
      text: item.subtitle || item.entityType || '',
      bold: item.title,
      time: new Date(item.viewedAt).toLocaleString(),
      _ts: Date.parse(item.viewedAt) || Date.now(),
      url: item.url
    }))
  } catch (err) {
    console.error('Failed to load recent views', err)
  } finally {
    recentLoading.value = false
  }
}

onMounted(async () => {
  await Promise.all([
    activityStore.fetchRecentActivities({ limit: 50 }),
    fetchRecentViews()
  ])
})

const isLoading = computed(() => activeTab.value === 'worked' ? activityStore.loading : recentLoading.value)

const filteredItems = computed(() => {
  const source = activeTab.value === 'worked' ? activityStore.activities : recentViews.value
  const q = searchQuery.value.trim().toLowerCase()
  if (!q) return source
  return source.filter(item => `${item.bold || ''} ${item.text || ''}`.toLowerCase().includes(q))
})

const groupedActivities = computed(() => {
  const groups = {
    today: [],
    thisWeek: [],
    older: []
  }

  const now = Date.now()
  const oneDay = 24 * 60 * 60 * 1000
  const oneWeek = 7 * oneDay

  filteredItems.value.forEach(act => {
    const age = now - act._ts
    if (age < oneDay) {
      groups.today.push(act)
    } else if (age < oneWeek) {
      groups.thisWeek.push(act)
    } else {
      groups.older.push(act)
    }
  })

  const result = []
  if (groups.today.length > 0) result.push({ label: 'Today', items: groups.today })
  if (groups.thisWeek.length > 0) result.push({ label: 'This week', items: groups.thisWeek })
  if (groups.older.length > 0) result.push({ label: 'Older', items: groups.older })
  return result
})

const goToItem = (item) => {
  if (item.url) router.push(item.url)
}
</script>

<style scoped>
.recent-page {
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
  color: #172B4D;
  background-color: #FFFFFF;
  min-height: 100vh;
  display: flex;
  flex-direction: column;
}

.page-header { padding: 32px 40px 0; border-bottom: 1px solid #DFE1E6; }
.page-header h1 { font-size: 24px; font-weight: 500; color: #172B4D; margin: 0 0 24px 0; }
.tabs { display: flex; gap: 24px; }
.tab-btn { background: transparent; border: none; padding: 0 0 12px 0; font-size: 14px; font-weight: 500; color: #5E6C84; cursor: pointer; position: relative; }
.tab-btn:hover { color: #172B4D; }
.tab-btn.active { color: #0052CC; }
.tab-btn.active::after { content: ''; position: absolute; bottom: -1px; left: 0; right: 0; height: 2px; background-color: #0052CC; }
.page-content { padding: 24px 40px; max-width: 1000px; }
.filter-container { margin-bottom: 24px; }
.search-input-wrapper { position: relative; width: 240px; }
.search-icon { position: absolute; left: 10px; top: 50%; transform: translateY(-50%); color: #5E6C84; font-size: 14px; }
.search-input { width: 100%; padding: 6px 12px 6px 44px; border: 2px solid #DFE1E6; border-radius: 3px; background-color: #FFFFFF; color: #091E42; font-size: 14px; outline: none; transition: background-color 0.2s, border-color 0.2s; box-sizing: border-box; }
.search-input:hover { background-color: #FAFBFC; }
.search-input:focus { background-color: #FFFFFF; border-color: #4C9AFF; }
.audit-list { display: flex; flex-direction: column; gap: 32px; }
.time-group { display: flex; flex-direction: column; }
.time-label { font-size: 12px; font-weight: 600; color: #5E6C84; margin: 0 0 12px 0; }
.audit-item { display: flex; align-items: center; padding: 12px 0; border-bottom: 1px solid #DFE1E6; gap: 16px; }
.audit-item:last-child { border-bottom: none; }
.audit-item.clickable { cursor: pointer; }
.audit-item.clickable:hover { background: #FAFBFC; }
.item-icon { width: 24px; height: 24px; display: flex; align-items: center; justify-content: center; border-radius: 50%; font-size: 12px; }
.item-icon.square { border-radius: 3px; }
.light-blue { background: #E6FCFF; color: #00B8D9; }
.item-details { flex: 1; }
.item-title { font-size: 14px; font-weight: 500; color: #172B4D; margin-bottom: 2px; }
.item-path { font-size: 12px; color: #5E6C84; }
.item-meta { display: flex; align-items: center; gap: 16px; }
.status-badge { font-size: 11px; font-weight: 700; padding: 2px 6px; border-radius: 3px; }
.status-badge.done { background: #E3FCEF; color: #006644; }
.time-ago { font-size: 12px; color: #5E6C84; min-width: 80px; text-align: right; }
.empty-state { text-align: center; color: #5E6C84; margin-top: 40px; padding: 16px; }
</style>
