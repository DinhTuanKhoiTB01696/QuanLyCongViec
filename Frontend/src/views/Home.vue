<script setup>
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import {
  CheckCircle,
  Layout,
  Users,
  TrendingUp,
  ArrowRight,
  Github,
  Twitter,
  Instagram,
  Facebook
} from 'lucide-vue-next'
import axiosClient from '@/api/axiosClient'
import { useProjectStore } from '@/store/useProjectStore'

import dashboardPreview from '../assets/task_management_dashboard_preview_1773375713763.png'
import focusImage from '../assets/modern_desk_setup_product_design_1773375736492.png'
import logoImg from '../assets/logo_QLCV.png'

const router = useRouter()
const projectStore = useProjectStore()
const isLoggedIn = ref(false)
const searchKeyword = ref('')
const searchResults = ref([])
const searching = ref(false)
let searchTimer = null

const features = [
  {
    title: 'Quản lý Công việc',
    desc: 'Theo dõi tiến độ dự án với các công cụ quản lý dễ sử dụng.',
    icon: CheckCircle,
    color: '#0061ff',
    bg: '#eff6ff'
  },
  {
    title: 'Bảng Kanban',
    desc: 'Trực quan hóa luồng công việc và tăng hiệu suất trong chớp mắt.',
    icon: Layout,
    color: '#a855f7',
    bg: '#f5f3ff'
  },
  {
    title: 'Cộng tác Nhóm',
    desc: 'Trò chuyện, bình luận và cộng tác theo thời gian thực với đội ngũ.',
    icon: Users,
    color: '#0ea5e9',
    bg: '#f0f9ff'
  },
  {
    title: 'Theo dõi Quá trình',
    desc: 'Nắm bắt chi tiết thông qua các báo cáo và phân tích chuyên sâu.',
    icon: TrendingUp,
    color: '#3b82f6',
    bg: '#eff6ff'
  }
]

const showSearchDropdown = computed(() => {
  return isLoggedIn.value && (searching.value || searchResults.value.length > 0 || searchKeyword.value.trim().length > 0)
})

const runSearch = async () => {
  const keyword = searchKeyword.value.trim()
  if (!keyword) {
    searchResults.value = []
    searching.value = false
    return
  }

  searching.value = true
  try {
    const response = await axiosClient.get('/worktasks', { params: { search: keyword } })
    searchResults.value = response.data?.data || []
  } catch {
    searchResults.value = []
  } finally {
    searching.value = false
  }
}

const handleSearchInput = () => {
  if (searchTimer) clearTimeout(searchTimer)
  searchTimer = setTimeout(runSearch, 250)
}

const openSearchResult = (result) => {
  searchResults.value = []
  searchKeyword.value = ''
  router.push(`/space/${result.projectId}?task=${result.id}`)
}

onMounted(() => {
  isLoggedIn.value = Boolean(localStorage.getItem('token') || localStorage.getItem('accessToken'))
  if (isLoggedIn.value) {
    projectStore.fetchAllProjects().catch(() => {})
  }
})
</script>

<template>
  <div class="landing-page" data-theme="light">
    <header class="navbar">
      <div class="container nav-content">
        <a href="/" class="logo">
          <img :src="logoImg" alt="SprintA Logo" class="custom-logo" />
          <span class="logo-text">SprintA</span>
        </a>

        <div class="nav-actions">
          <div v-if="isLoggedIn" class="nav-search">
            <i class="fa-solid fa-magnifying-glass"></i>
            <input v-model="searchKeyword" type="text" placeholder="Search work items..." @input="handleSearchInput" />
            <div v-if="showSearchDropdown" class="search-dropdown">
              <div v-if="searching" class="search-state">Searching...</div>
              <template v-else-if="searchResults.length">
                <button v-for="result in searchResults" :key="result.id" type="button" class="search-result" @click="openSearchResult(result)">
                  <strong>{{ result.sequenceId || result.title }}</strong>
                  <span>{{ result.title }}</span>
                  <small>{{ result.projectName }}</small>
                </button>
              </template>
              <div v-else class="search-state">No work items found.</div>
            </div>
          </div>
          <template v-else>
            <el-button link @click="$router.push('/login')">Đăng nhập</el-button>
            <el-button type="primary" round @click="$router.push('/register')">Đăng ký</el-button>
          </template>
        </div>
      </div>
    </header>

    <section class="hero-section section-padding">
      <div class="container hero-grid">
        <div class="hero-copy">
          <div class="new-badge">SprintA Agile Workspace</div>
          <h1 class="hero-title">Tổ chức công việc. Quản lý dự án. Giữ mọi thứ đúng nhịp.</h1>
          <p class="hero-sub">
            Từ dashboard, project, notification đến work item search, SprintA giúp đội ngũ Agile làm việc gọn gàng và rõ ràng hơn.
          </p>
          <div class="hero-btns">
            <el-button type="primary" size="large" round class="btn-cta" @click="$router.push(isLoggedIn ? '/dashboard' : '/register')">
              {{ isLoggedIn ? 'Mở Dashboard' : 'Bắt đầu miễn phí' }}
            </el-button>
            <el-button size="large" round @click="$router.push('/spaces')">Xem Projects</el-button>
          </div>
        </div>
        <div class="dashboard-preview">
          <img :src="dashboardPreview" alt="Dashboard Preview" />
        </div>
      </div>
    </section>

    <section class="features-section section-padding">
      <div class="container text-center">
        <h2 class="section-title">Mọi thứ bạn cần đều ở đây</h2>
        <p class="section-sub">Tối ưu hóa quy trình làm việc bằng các công cụ cấp doanh nghiệp.</p>

        <el-row :gutter="24" class="feature-grid">
          <el-col :xs="24" :sm="12" :md="6" v-for="feature in features" :key="feature.title">
            <div class="feature-card">
              <div class="icon-wrap" :style="{ backgroundColor: feature.bg }">
                <component :is="feature.icon" :size="22" :color="feature.color" />
              </div>
              <h3>{{ feature.title }}</h3>
              <p>{{ feature.desc }}</p>
            </div>
          </el-col>
        </el-row>
      </div>
    </section>

    <section class="focus-section section-padding">
      <div class="container focus-grid">
        <div class="focus-copy">
          <h2 class="section-title">Tập trung vào phần việc quan trọng nhất</h2>
          <p class="section-sub">Dashboard, project tree và notification đều đồng bộ để bạn đi thẳng đến phần việc đang chờ xử lý.</p>
          <a class="view-all" href="/dashboard">Đi tới dashboard <ArrowRight :size="16" /></a>
        </div>
        <div class="focus-image">
          <img :src="focusImage" alt="Focus" />
        </div>
      </div>
    </section>

    <footer class="footer">
      <div class="container footer-row">
        <div>
          <a href="/" class="logo footer-logo">
            <img :src="logoImg" alt="SprintA Logo" class="custom-logo" />
            <span class="logo-text">SprintA</span>
          </a>
          <p class="footer-about">SprintA giúp đội ngũ bàn giao sản phẩm tốt hơn bằng workflow gọn, rõ và có thể theo dõi được.</p>
        </div>
        <div class="socials">
          <Twitter :size="20" />
          <Github :size="20" />
          <Instagram :size="20" />
          <Facebook :size="20" />
        </div>
      </div>
    </footer>
  </div>
</template>

<style scoped>
.landing-page {
  min-height: 100vh;
  background: #fff;
  color: #111827;
}

.navbar {
  position: sticky;
  top: 0;
  z-index: 1000;
  background: rgba(255, 255, 255, 0.92);
  backdrop-filter: blur(12px);
  border-bottom: 1px solid #e5e7eb;
}

.container {
  width: min(1180px, calc(100% - 32px));
  margin: 0 auto;
}

.nav-content,
.nav-actions,
.hero-grid,
.focus-grid,
.footer-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 24px;
}

.nav-content {
  min-height: 80px;
}

.logo {
  display: flex;
  align-items: center;
  color: inherit;
  text-decoration: none;
}

.custom-logo {
  height: 56px;
}

.logo-text {
  margin-left: -8px;
  font-size: 24px;
  font-weight: 900;
}

.nav-search {
  position: relative;
  display: flex;
  align-items: center;
  gap: 10px;
  min-width: 360px;
  background: #f8fafc;
  border: 1px solid #dbe4ee;
  border-radius: 8px;
  padding: 10px 12px;
}

.nav-search input {
  width: 100%;
  border: none;
  background: transparent;
  outline: none;
}

.search-dropdown {
  position: absolute;
  top: calc(100% + 8px);
  left: 0;
  right: 0;
  background: #fff;
  border: 1px solid #dbe4ee;
  border-radius: 8px;
  box-shadow: 0 12px 32px rgba(15, 23, 42, 0.12);
  overflow: hidden;
}

.search-result,
.search-state {
  width: 100%;
  display: grid;
  text-align: left;
  gap: 4px;
  padding: 12px 14px;
}

.search-result {
  border: none;
  background: #fff;
  cursor: pointer;
}

.search-result:hover {
  background: #f8fafc;
}

.search-result span,
.search-result small,
.search-state,
.hero-sub,
.section-sub,
.feature-card p,
.footer-about {
  color: #64748b;
}

.section-padding {
  padding: 72px 0;
}

.hero-grid,
.focus-grid {
  align-items: center;
}

.hero-copy,
.focus-copy {
  flex: 1;
}

.hero-title {
  margin: 16px 0;
  font-size: clamp(2.3rem, 4vw, 4rem);
  line-height: 1.08;
}

.new-badge {
  display: inline-flex;
  align-items: center;
  min-height: 34px;
  padding: 0 12px;
  border-radius: 8px;
  background: #eff6ff;
  color: #1d4ed8;
  font-weight: 700;
}

.hero-btns {
  display: flex;
  gap: 12px;
  margin-top: 28px;
}

.btn-cta {
  padding: 24px 44px;
}

.dashboard-preview {
  flex: 1;
  border-radius: 8px;
  overflow: hidden;
  box-shadow: 0 28px 60px rgba(15, 23, 42, 0.16);
}

.dashboard-preview img,
.focus-image img {
  width: 100%;
  display: block;
}

.section-title {
  margin: 0 0 12px;
  font-size: 2rem;
}

.feature-card {
  height: 100%;
  padding: 24px;
  border: 1px solid #e5e7eb;
  border-radius: 8px;
  text-align: left;
}

.icon-wrap {
  width: 44px;
  height: 44px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border-radius: 8px;
  margin-bottom: 14px;
}

.view-all {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  color: #0f172a;
  font-weight: 700;
  text-decoration: none;
}

.footer {
  background: #0f172a;
  color: #fff;
  padding: 32px 0;
}

.footer-logo {
  color: #fff;
}

.socials {
  display: flex;
  gap: 14px;
}

@media (max-width: 992px) {
  .hero-grid,
  .focus-grid,
  .footer-row,
  .nav-content {
    flex-direction: column;
    align-items: flex-start;
  }

  .nav-search {
    min-width: 0;
    width: 100%;
  }

  .nav-actions {
    width: 100%;
  }
}
</style>
