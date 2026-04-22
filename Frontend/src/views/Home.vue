<script setup>
import {
  CheckCircle,
  Layout,
  Users,
  TrendingUp,
  Github,
  Twitter,
  Instagram,
  Facebook
} from 'lucide-vue-next'

import dashboardPreview from '../assets/task_management_dashboard_preview_1773375713763.png'
import focusImage from '../assets/modern_desk_setup_product_design_1773375736492.png'
import logoImg from '../assets/logo_QLCV.png'

const features = [
  {
    title: 'Quan ly cong viec',
    desc: 'Theo doi tien do, uu tien va trang thai theo mot quy trinh ro rang.',
    icon: CheckCircle,
    color: '#0061ff',
    bg: '#eff6ff'
  },
  {
    title: 'Kanban va workflow',
    desc: 'Van hanh cong viec qua backlog, to do, in progress, in review, done va cancelled.',
    icon: Layout,
    color: '#7c3aed',
    bg: '#f5f3ff'
  },
  {
    title: 'Cong tac nhom',
    desc: 'Binh luan, giao viec, theo doi thanh vien va luu vet thay doi trong cung mot noi.',
    icon: Users,
    color: '#0ea5e9',
    bg: '#f0f9ff'
  },
  {
    title: 'Bao cao va tong hop',
    desc: 'Tong hop thong tin de doi ngu nhin nhanh tinh trang du an va nhung viec can xu ly.',
    icon: TrendingUp,
    color: '#16a34a',
    bg: '#f0fdf4'
  }
]
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
          <router-link class="nav-link" to="/login">Dang nhap</router-link>
          <router-link class="nav-primary" to="/register">Dang ky</router-link>
        </div>
      </div>
    </header>

    <section class="hero-section">
      <div class="container hero-grid">
        <div class="hero-copy">
          <div class="new-badge">SprintA Agile Workspace</div>
          <h1 class="hero-title">Dang ky hoac dang nhap de vao dashboard chinh.</h1>
          <p class="hero-sub">
            Luong vao he thong da duoc khoa lai: nguoi dung phai dang ky hoac dang nhap truoc khi su dung dashboard,
            project va cac chuc nang noi bo.
          </p>
          <div class="hero-btns">
            <router-link class="hero-primary" to="/login">Dang nhap</router-link>
            <router-link class="hero-secondary" to="/register">Dang ky</router-link>
          </div>
        </div>

        <div class="dashboard-preview">
          <img :src="dashboardPreview" alt="Dashboard Preview" />
        </div>
      </div>
    </section>

    <section class="features-section">
      <div class="container">
        <div class="section-copy">
          <h2 class="section-title">Moi chuc nang chinh deu tap trung vao workflow Agile</h2>
          <p class="section-sub">Trang dau chi con dong vai tro cong vao xac thuc, khong con duong di tat vao dashboard hay project.</p>
        </div>

        <div class="feature-grid">
          <article v-for="feature in features" :key="feature.title" class="feature-card">
            <div class="icon-wrap" :style="{ backgroundColor: feature.bg }">
              <component :is="feature.icon" :size="22" :color="feature.color" />
            </div>
            <h3>{{ feature.title }}</h3>
            <p>{{ feature.desc }}</p>
          </article>
        </div>
      </div>
    </section>

    <section class="focus-section">
      <div class="container focus-grid">
        <div class="focus-copy">
          <h2 class="section-title">Tap trung vao truy cap an toan va workflow ro rang</h2>
          <p class="section-sub">
            Sau khi xac thuc, nguoi dung moi duoc vao dashboard chinh de tiep tuc lam viec voi project, task,
            comment va notification.
          </p>
          <div class="hero-btns">
            <router-link class="hero-primary" to="/login">Dang nhap</router-link>
            <router-link class="hero-secondary" to="/register">Dang ky</router-link>
          </div>
        </div>

        <div class="focus-image">
          <img :src="focusImage" alt="Focus workspace" />
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
          <p class="footer-about">He thong quan ly cong viec cho doi ngu Agile voi cong vao xac thuc ro rang va workflow nhat quan.</p>
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
  height: auto;
  width: 100%;
  overflow-x: hidden;
  overflow-y: auto;
  background:
    radial-gradient(circle at top left, rgba(59, 130, 246, 0.08), transparent 32%),
    linear-gradient(180deg, #ffffff 0%, #f8fafc 100%);
  color: #111827;
}

.navbar {
  position: sticky;
  top: 0;
  z-index: 1000;
  background: rgba(255, 255, 255, 0.9);
  backdrop-filter: blur(14px);
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
.footer-row,
.hero-btns {
  display: flex;
  align-items: center;
}

.nav-content,
.footer-row {
  justify-content: space-between;
  gap: 24px;
}

.nav-content {
  min-height: 80px;
}

.logo {
  display: inline-flex;
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

.nav-actions,
.hero-btns {
  gap: 12px;
}

.nav-link,
.nav-primary,
.hero-primary,
.hero-secondary {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border-radius: 999px;
  text-decoration: none;
  font-weight: 700;
  transition: all 0.2s ease;
}

.nav-link {
  color: #1f2937;
  padding: 10px 14px;
}

.nav-primary,
.hero-primary {
  background: linear-gradient(135deg, #0f172a 0%, #2563eb 100%);
  color: #fff;
}

.nav-primary {
  padding: 10px 18px;
}

.hero-section,
.features-section,
.focus-section {
  padding: 56px 0;
}

.hero-grid,
.focus-grid {
  justify-content: space-between;
  gap: 40px;
}

.hero-grid,
.focus-grid,
.feature-grid {
  display: grid;
}

.hero-grid,
.focus-grid {
  grid-template-columns: minmax(0, 1.1fr) minmax(320px, 0.9fr);
}

.hero-copy,
.focus-copy,
.section-copy {
  display: flex;
  flex-direction: column;
  gap: 18px;
}

.new-badge {
  width: fit-content;
  padding: 8px 14px;
  border-radius: 999px;
  background: #e0f2fe;
  color: #075985;
  font-size: 13px;
  font-weight: 800;
  letter-spacing: 0.04em;
  text-transform: uppercase;
}

.hero-title,
.section-title {
  margin: 0;
  line-height: 1.1;
}

.hero-title {
  font-size: clamp(36px, 5vw, 64px);
  max-width: 720px;
}

.hero-sub,
.section-sub,
.footer-about,
.feature-card p {
  color: #64748b;
  line-height: 1.7;
}

.hero-primary,
.hero-secondary {
  min-width: 148px;
  padding: 14px 22px;
}

.hero-secondary {
  border: 1px solid #cbd5e1;
  background: rgba(255, 255, 255, 0.92);
  color: #0f172a;
}

.dashboard-preview,
.focus-image {
  position: relative;
}

.dashboard-preview img,
.focus-image img {
  width: 100%;
  display: block;
  border-radius: 28px;
  border: 1px solid rgba(148, 163, 184, 0.25);
  box-shadow: 0 24px 60px rgba(15, 23, 42, 0.14);
}

.feature-grid {
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 20px;
  margin-top: 32px;
}

.feature-card {
  padding: 24px;
  border-radius: 24px;
  background: rgba(255, 255, 255, 0.95);
  border: 1px solid #e2e8f0;
  box-shadow: 0 12px 30px rgba(15, 23, 42, 0.06);
}

.icon-wrap {
  width: 52px;
  height: 52px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border-radius: 18px;
  margin-bottom: 18px;
}

.feature-card h3 {
  margin: 0 0 10px;
  font-size: 20px;
}

.footer {
  padding: 28px 0 42px;
  border-top: 1px solid #e5e7eb;
}

.footer-about {
  max-width: 520px;
}

.socials {
  display: flex;
  gap: 14px;
  color: #475569;
}

@media (max-width: 960px) {
  .hero-grid,
  .focus-grid,
  .feature-grid {
    grid-template-columns: 1fr;
  }

  .hero-grid,
  .focus-grid {
    gap: 28px;
  }

  .nav-content,
  .footer-row {
    flex-direction: column;
    align-items: flex-start;
  }
}

@media (max-width: 640px) {
  .container {
    width: min(100% - 24px, 1180px);
  }

  .nav-actions,
  .hero-btns {
    width: 100%;
    flex-direction: column;
    align-items: stretch;
  }

  .nav-link,
  .nav-primary,
  .hero-primary,
  .hero-secondary {
    width: 100%;
  }
}
</style>
