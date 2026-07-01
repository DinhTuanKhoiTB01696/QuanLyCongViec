<script setup>
import {
  ArrowRight,
  BarChart3,
  Bot,
  CalendarCheck,
  CheckCircle2,
  Facebook,
  Github,
  Instagram,
  KanbanSquare,
  Layers3,
  Moon,
  Play,
  ShieldCheck,
  Sparkles,
  Sun,
  Twitter,
  Users,
  Zap
} from 'lucide-vue-next'

import dashboardPreview from '../assets/task_management_dashboard_preview_1773375713763.png'
import focusImage from '../assets/modern_desk_setup_product_design_1773375736492.png'
import motionGif from '../assets/Motion_graphics_animation_the_cyan_and_blue.gif'
import logoImg from '../assets/logo_QLCV.png'
import { currentTheme, toggleTheme } from '@/utils/theme'

const promoVideoSrc = ''

const modules = [
  { icon: KanbanSquare, title: 'Kanban & Tasks', text: 'Quản lý backlog, trạng thái, deadline, người phụ trách và độ ưu tiên trong một bảng rõ ràng.' },
  { icon: CalendarCheck, title: 'Sprint / Chu kỳ', text: 'Lập kế hoạch sprint, theo dõi tiến độ và biết ngay việc nào đang bị chậm.' },
  { icon: Layers3, title: 'Modules', text: 'Gom các công việc liên quan vào từng phân hệ để đội nhóm dễ chia phạm vi.' },
  { icon: BarChart3, title: 'Reports', text: 'Tổng hợp tiến độ, tải công việc và rủi ro để báo cáo nhanh hơn.' },
  { icon: Users, title: 'Team Workspace', text: 'Mỗi dự án có không gian riêng cho thành viên, vai trò, công việc và cập nhật.' },
  { icon: Sparkles, title: 'Rewards', text: 'Ghi nhận đóng góp và tạo động lực cho thành viên hoàn thành công việc đúng hạn.' }
]

const stats = [
  { value: 'Rõ việc hơn', label: 'task, người phụ trách, deadline nằm cùng một chỗ' },
  { value: 'Ít hỏi lại hơn', label: 'trạng thái và cập nhật hiển thị theo thời gian thực' },
  { value: 'Báo cáo nhanh', label: 'tiến độ sprint và nhóm được tổng hợp sẵn' }
]

const timeline = [
  'Tạo dự án và chia việc theo priority',
  'Kéo task qua board, sprint và module',
  'Theo dõi báo cáo, điểm thưởng và cảnh báo quá hạn'
]
</script>

<template>
  <div class="landing-page">
    <header class="navbar">
      <div class="container nav-content">
        <router-link to="/" class="logo">
          <img :src="logoImg" alt="SprintA Logo" class="custom-logo" />
          <span>SprintA</span>
        </router-link>

        <nav class="nav-center" aria-label="Public navigation">
          <a href="#features">Tính năng</a>
          <a href="#video">Video</a>
          <a href="#workflow">Quy trình</a>
        </nav>

        <div class="nav-actions">
          <button class="theme-toggle" type="button" aria-label="Toggle theme" @click="toggleTheme()">
            <Sun v-if="currentTheme === 'dark'" :size="18" />
            <Moon v-else :size="18" />
          </button>
          <router-link class="nav-link" to="/login">Đăng nhập</router-link>
          <router-link class="nav-primary" to="/register">Dùng thử</router-link>
        </div>
      </div>
    </header>

    <main>
      <section class="hero-section">
        <div class="hero-orbit orbit-one"></div>
        <div class="hero-orbit orbit-two"></div>

        <div class="container hero-grid">
          <div class="hero-copy">
            <div class="eyebrow">
              <Sparkles :size="16" />
              SPRINTA AGILE WORKSPACE
            </div>
            <h1>Quản lý công việc, sprint và báo cáo trong một workspace dễ dùng hơn.</h1>
            <p>
              SprintA giúp đội nhóm gom task, chu kỳ, module, báo cáo và tiến độ về một nơi — để ai cũng biết việc nào cần làm, ai đang phụ trách và dự án đang chạy tới đâu.
            </p>

            <div class="hero-actions">
              <router-link class="primary-cta" to="/register">
                Bắt đầu miễn phí
                <ArrowRight :size="18" />
              </router-link>
              <a class="secondary-cta" href="#video">
                Xem demo
              </a>
            </div>
            <small style="display: block; margin-top: 12px; color: var(--page-muted); font-size: 13px;">Không cần thẻ thanh toán. Tạo workspace và thử ngay trong vài phút.</small>

            <div class="stat-row">
              <article v-for="stat in stats" :key="stat.label">
                <strong>{{ stat.value }}</strong>
                <span>{{ stat.label }}</span>
              </article>
            </div>
          </div>

          <div class="hero-visual" aria-label="SprintA dashboard preview">
            <div class="preview-shell">
              <div class="preview-topbar">
                <span></span>
                <span></span>
                <span></span>
                <small>Dashboard live</small>
              </div>
              <img :src="dashboardPreview" alt="SprintA dashboard preview" />
            </div>

            <div class="floating-card card-progress">
              <CheckCircle2 :size="18" />
              <div>
                <strong>Báo cáo nhanh</strong>
                <span>Tổng hợp tiến độ</span>
              </div>
            </div>

            <div class="floating-card card-ai">
              <Bot :size="18" />
              <div>
                <strong>Gợi ý thông minh</strong>
                <span>Sắp ra mắt</span>
              </div>
            </div>
          </div>
        </div>
      </section>

      <section id="features" class="feature-strip">
        <div class="container">
          <div class="section-heading">
            <span class="eyebrow small">Product suite</span>
            <h2>Tất cả thứ đội nhóm cần để chạy dự án gọn hơn.</h2>
            <p>Từ backlog, bảng công việc, sprint, module đến báo cáo — SprintA gom các bước quan trọng vào một luồng làm việc dễ theo dõi.</p>
          </div>

          <div class="module-grid">
            <article v-for="module in modules" :key="module.title" class="module-card">
              <div class="module-icon">
                <component :is="module.icon" :size="22" />
              </div>
              <h3>{{ module.title }}</h3>
              <p>{{ module.text }}</p>
            </article>
          </div>
        </div>
      </section>

      <section id="video" class="video-section">
        <div class="container video-grid">
          <div class="video-copy">
            <span class="eyebrow small">Showcase video</span>
            <h2>Xem SprintA hoạt động trong một dự án thật.</h2>
            <p>
              Video ngắn cho bạn thấy cách tạo task, kéo việc qua board, theo dõi sprint và xem báo cáo mà không cần mở nhiều công cụ khác nhau.
            </p>
            <div class="timeline">
              <div v-for="(item, index) in timeline" :key="item" class="timeline-item">
                <span>{{ index + 1 }}</span>
                <p>{{ item }}</p>
              </div>
            </div>
            <a class="secondary-cta" href="#video" style="margin-top: 24px; display: inline-flex;">Xem demo sản phẩm</a>
          </div>

          <div class="video-frame">
            <video v-if="promoVideoSrc" :src="promoVideoSrc" controls playsinline></video>
            <div v-else class="video-placeholder">
              <img :src="motionGif" alt="SprintA motion preview" />
              <button type="button" class="play-button" aria-label="Play product video">
                <Play :size="26" fill="currentColor" />
              </button>
              <div class="video-caption">
                <strong>SprintA product film</strong>
                <span>Demo workspace, sprint, report và module</span>
              </div>
            </div>
          </div>
        </div>
      </section>

      <section id="workflow" class="workflow-section">
        <div class="container workflow-grid">
          <div class="workflow-visual">
            <img :src="focusImage" alt="Modern workspace" />
            <div class="workflow-badge">
              <Zap :size="18" />
              Fast onboarding
            </div>
          </div>

          <div class="workflow-copy">
            <span class="eyebrow small">From idea to done</span>
            <h2>Từ ý tưởng đến hoàn thành, đội nhóm đều nhìn cùng một bức tranh.</h2>
            <p>
              SprintA giúp bạn biến danh sách việc rời rạc thành một quy trình rõ ràng: tạo task, chia sprint, theo dõi tiến độ, cập nhật báo cáo và biết ai đang cần hỗ trợ.
            </p>
            <div class="workflow-pills">
              <span><Layers3 :size="16" /> Projects</span>
              <span><KanbanSquare :size="16" /> Boards</span>
              <span><CalendarCheck :size="16" /> Sprints</span>
              <span><BarChart3 :size="16" /> Reports</span>
              <span><Sparkles :size="16" /> Rewards</span>
            </div>
            <router-link class="primary-cta compact" to="/register">
              Tạo workspace miễn phí
              <ArrowRight :size="18" />
            </router-link>
          </div>
        </div>
      </section>
    </main>

    <footer class="footer">
      <div class="container footer-row">
        <router-link to="/" class="logo footer-logo">
          <img :src="logoImg" alt="SprintA Logo" class="custom-logo" />
          <span>SprintA</span>
        </router-link>
        <p>SprintA — Quản lý công việc, sprint và hiệu suất đội nhóm trong một workspace rõ ràng.</p>
        <div class="socials">
          <Twitter :size="18" />
          <Github :size="18" />
          <Instagram :size="18" />
          <Facebook :size="18" />
        </div>
      </div>
    </footer>
  </div>
</template>

<style scoped>
.landing-page {
  --page-bg: #f6f9ff;
  --page-surface: rgba(255, 255, 255, 0.86);
  --page-surface-strong: #ffffff;
  --page-text: #08142f;
  --page-muted: #5d6b82;
  --page-border: rgba(34, 60, 111, 0.14);
  --page-glow: rgba(24, 181, 255, 0.2);
  --page-shadow: 0 26px 70px rgba(35, 64, 112, 0.14);
  min-height: 100vh;
  overflow-x: hidden;
  color: var(--page-text);
  background:
    radial-gradient(circle at 12% 4%, rgba(39, 210, 255, 0.2), transparent 28%),
    radial-gradient(circle at 85% 12%, rgba(124, 58, 237, 0.16), transparent 26%),
    linear-gradient(180deg, #ffffff 0%, var(--page-bg) 44%, #eef7ff 100%);
}

:global([data-theme='dark'] .landing-page) {
  --page-bg: #071222;
  --page-surface: rgba(13, 25, 45, 0.78);
  --page-surface-strong: #101c32;
  --page-text: #f6fbff;
  --page-muted: #a9b9cf;
  --page-border: rgba(149, 181, 222, 0.2);
  --page-glow: rgba(35, 211, 255, 0.16);
  --page-shadow: 0 30px 80px rgba(0, 0, 0, 0.38);
  background:
    radial-gradient(circle at 12% 4%, rgba(20, 184, 166, 0.22), transparent 28%),
    radial-gradient(circle at 88% 8%, rgba(59, 130, 246, 0.2), transparent 28%),
    linear-gradient(180deg, #07101e 0%, #0a1425 46%, #09111f 100%);
}

.container {
  width: min(1280px, calc(100% - 40px));
  margin: 0 auto;
}

.navbar {
  position: sticky;
  top: 0;
  z-index: 30;
  border-bottom: 1px solid var(--page-border);
  background: color-mix(in srgb, var(--page-surface-strong) 82%, transparent);
  backdrop-filter: blur(18px);
}

.nav-content,
.nav-center,
.nav-actions,
.logo,
.hero-actions,
.stat-row,
.footer-row,
.socials,
.workflow-pills {
  display: flex;
  align-items: center;
}

.nav-content {
  min-height: 68px;
  justify-content: space-between;
  gap: 20px;
}

.logo {
  gap: 8px;
  color: var(--page-text);
  font-size: 22px;
  font-weight: 900;
  text-decoration: none;
}

.custom-logo {
  width: 44px;
  height: 44px;
  object-fit: contain;
}

.nav-center {
  gap: 8px;
  padding: 6px;
  border: 1px solid var(--page-border);
  border-radius: 999px;
  background: color-mix(in srgb, var(--page-surface) 74%, transparent);
}

.nav-center a,
.nav-link {
  color: var(--page-muted);
  text-decoration: none;
  font-weight: 700;
}

.nav-center a {
  padding: 8px 13px;
  border-radius: 999px;
}

.nav-center a:hover {
  color: var(--page-text);
  background: color-mix(in srgb, #2fb8ff 10%, transparent);
}

.nav-actions {
  gap: 10px;
}

.theme-toggle,
.nav-primary,
.primary-cta,
.secondary-cta {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border: 0;
  text-decoration: none;
  font-weight: 850;
  cursor: pointer;
}

.theme-toggle {
  width: 38px;
  height: 38px;
  border-radius: 12px;
  color: var(--page-text);
  border: 1px solid var(--page-border);
  background: var(--page-surface);
}

.nav-link {
  padding: 10px 12px;
}

.nav-primary,
.primary-cta {
  gap: 8px;
  color: #ffffff;
  border-radius: 16px;
  background: linear-gradient(135deg, #00c2ff 0%, #3662ff 52%, #8b5cf6 100%);
  box-shadow: 0 16px 34px rgba(54, 98, 255, 0.28);
}

.nav-primary {
  padding: 10px 18px;
}

.hero-section {
  position: relative;
  padding: 46px 0 44px;
}

.hero-orbit {
  position: absolute;
  width: 320px;
  height: 320px;
  border-radius: 50%;
  filter: blur(4px);
  pointer-events: none;
}

.orbit-one {
  left: -120px;
  top: 110px;
  background: radial-gradient(circle, rgba(34, 211, 238, 0.22), transparent 68%);
  animation: drift 9s ease-in-out infinite;
}

.orbit-two {
  right: -90px;
  top: 22px;
  background: radial-gradient(circle, rgba(251, 191, 36, 0.18), transparent 70%);
  animation: drift 11s ease-in-out infinite reverse;
}

.hero-grid,
.video-grid,
.workflow-grid {
  display: grid;
  grid-template-columns: minmax(0, 0.88fr) minmax(420px, 1.12fr);
  gap: 34px;
  align-items: center;
}

.hero-copy {
  position: relative;
  z-index: 1;
}

.eyebrow {
  width: fit-content;
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 7px 11px;
  border-radius: 999px;
  color: #0369a1;
  background: rgba(14, 165, 233, 0.12);
  font-size: 12px;
  font-weight: 900;
  letter-spacing: 0.04em;
  text-transform: uppercase;
}

:global([data-theme='dark'] .eyebrow) {
  color: #7dd3fc;
  background: rgba(14, 165, 233, 0.16);
}

.eyebrow.small {
  margin-bottom: 14px;
}

.hero-copy h1,
.section-heading h2,
.video-copy h2,
.workflow-copy h2 {
  margin: 0;
  line-height: 1.02;
  letter-spacing: 0;
  color: var(--page-text);
}

.hero-copy h1 {
  max-width: 600px;
  margin-top: 16px;
  font-size: clamp(34px, 3.2vw, 52px);
  line-height: 1.08;
}

.hero-copy p,
.section-heading p,
.video-copy p,
.workflow-copy p,
.module-card p,
.footer p,
.timeline-item p {
  color: var(--page-muted);
  line-height: 1.65;
}

.hero-copy p {
  max-width: 590px;
  margin: 14px 0 0;
  font-size: 16px;
}

.hero-actions {
  flex-wrap: wrap;
  gap: 14px;
  margin-top: 22px;
}

.primary-cta,
.secondary-cta {
  min-height: 46px;
  padding: 0 18px;
  border-radius: 14px;
}

.secondary-cta {
  color: var(--page-text);
  border: 1px solid var(--page-border);
  background: var(--page-surface);
}

.stat-row {
  gap: 12px;
  flex-wrap: wrap;
  margin-top: 18px;
}

.stat-row article {
  min-width: 128px;
  padding: 12px 14px;
  border: 1px solid var(--page-border);
  border-radius: 16px;
  background: var(--page-surface);
}

.stat-row strong {
  display: block;
  font-size: 24px;
}

.stat-row span {
  color: var(--page-muted);
  font-size: 13px;
  font-weight: 700;
}

.hero-visual {
  position: relative;
  min-height: 430px;
}

.preview-shell {
  overflow: hidden;
  border: 1px solid var(--page-border);
  border-radius: 22px;
  background: var(--page-surface-strong);
  box-shadow: var(--page-shadow);
  transform: perspective(1200px) rotateY(-7deg) rotateX(4deg);
  animation: riseIn 720ms cubic-bezier(0.2, 0.8, 0.2, 1) both, floaty 7s ease-in-out 1s infinite;
}

.preview-topbar {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 11px 14px;
  border-bottom: 1px solid var(--page-border);
}

.preview-topbar span {
  width: 10px;
  height: 10px;
  border-radius: 999px;
  background: #f43f5e;
}

.preview-topbar span:nth-child(2) {
  background: #f59e0b;
}

.preview-topbar span:nth-child(3) {
  background: #22c55e;
}

.preview-topbar small {
  margin-left: auto;
  color: var(--page-muted);
  font-weight: 800;
}

.preview-shell img {
  width: 100%;
  display: block;
}

.floating-card {
  position: absolute;
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 12px 14px;
  border: 1px solid var(--page-border);
  border-radius: 18px;
  color: var(--page-text);
  background: color-mix(in srgb, var(--page-surface-strong) 88%, transparent);
  box-shadow: var(--page-shadow);
  backdrop-filter: blur(14px);
  animation: floaty 6s ease-in-out infinite;
}

.floating-card strong,
.floating-card span {
  display: block;
}

.floating-card strong {
  font-size: 18px;
}

.floating-card span {
  color: var(--page-muted);
  font-size: 12px;
  font-weight: 800;
}

.card-progress {
  left: -12px;
  bottom: 44px;
}

.card-ai {
  right: -4px;
  top: 76px;
  animation-delay: 900ms;
}

.feature-strip,
.video-section,
.workflow-section {
  padding: 52px 0;
}

.section-heading {
  max-width: 760px;
}

.section-heading h2,
.video-copy h2,
.workflow-copy h2 {
  font-size: clamp(28px, 3vw, 44px);
}

.section-heading p {
  margin: 16px 0 0;
  font-size: 17px;
}

.module-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 18px;
  margin-top: 30px;
}

.module-card {
  position: relative;
  min-height: 188px;
  padding: 20px;
  border: 1px solid var(--page-border);
  border-radius: 24px;
  background:
    linear-gradient(135deg, color-mix(in srgb, var(--page-surface-strong) 92%, transparent), var(--page-surface)),
    radial-gradient(circle at top right, var(--page-glow), transparent 42%);
  box-shadow: 0 18px 42px rgba(20, 42, 85, 0.08);
  transition: transform 180ms ease, border-color 180ms ease, box-shadow 180ms ease;
}

.module-card:hover {
  transform: translateY(-6px);
  border-color: color-mix(in srgb, #22d3ee 42%, var(--page-border));
  box-shadow: var(--page-shadow);
}

.module-icon {
  width: 42px;
  height: 42px;
  display: grid;
  place-items: center;
  border-radius: 16px;
  color: #ffffff;
  background: linear-gradient(135deg, #06b6d4, #2563eb);
}

.module-card h3 {
  margin: 14px 0 8px;
  font-size: 19px;
}

.video-grid {
  padding: 34px;
  border: 1px solid var(--page-border);
  border-radius: 32px;
  background:
    linear-gradient(135deg, color-mix(in srgb, var(--page-surface-strong) 88%, transparent), var(--page-surface)),
    radial-gradient(circle at 82% 18%, rgba(45, 212, 191, 0.18), transparent 36%);
  box-shadow: var(--page-shadow);
}

.timeline {
  display: grid;
  gap: 12px;
  margin-top: 26px;
}

.timeline-item {
  display: flex;
  gap: 12px;
  align-items: center;
  padding: 14px;
  border: 1px solid var(--page-border);
  border-radius: 18px;
  background: color-mix(in srgb, var(--page-surface-strong) 74%, transparent);
}

.timeline-item span {
  width: 32px;
  height: 32px;
  display: grid;
  place-items: center;
  flex: 0 0 auto;
  border-radius: 12px;
  color: #ffffff;
  font-weight: 900;
  background: linear-gradient(135deg, #0ea5e9, #7c3aed);
}

.timeline-item p {
  margin: 0;
  font-weight: 800;
}

.video-frame {
  overflow: hidden;
  aspect-ratio: 16 / 10;
  border-radius: 28px;
  border: 1px solid var(--page-border);
  background: #071222;
  box-shadow: 0 24px 60px rgba(2, 8, 23, 0.26);
}

.video-frame video,
.video-placeholder,
.video-placeholder img {
  width: 100%;
  height: 100%;
}

.video-placeholder {
  position: relative;
  display: grid;
  place-items: center;
}

.video-placeholder img {
  object-fit: cover;
  opacity: 0.74;
}

.play-button {
  position: absolute;
  width: 78px;
  height: 78px;
  display: grid;
  place-items: center;
  border: 0;
  border-radius: 999px;
  color: #ffffff;
  background: linear-gradient(135deg, #06b6d4, #3662ff);
  box-shadow: 0 18px 40px rgba(6, 182, 212, 0.34);
}

.video-caption {
  position: absolute;
  left: 22px;
  right: 22px;
  bottom: 22px;
  padding: 16px;
  border: 1px solid rgba(255, 255, 255, 0.16);
  border-radius: 18px;
  color: #ffffff;
  background: rgba(5, 10, 24, 0.66);
  backdrop-filter: blur(14px);
}

.video-caption strong,
.video-caption span {
  display: block;
}

.video-caption span {
  margin-top: 4px;
  color: rgba(255, 255, 255, 0.72);
}

.workflow-grid {
  grid-template-columns: minmax(380px, 0.9fr) minmax(0, 1.1fr);
}

.workflow-visual {
  position: relative;
}

.workflow-visual img {
  width: 100%;
  display: block;
  border-radius: 28px;
  border: 1px solid var(--page-border);
  box-shadow: var(--page-shadow);
}

.workflow-badge {
  position: absolute;
  left: 22px;
  bottom: 22px;
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 12px 14px;
  border-radius: 16px;
  color: #ffffff;
  font-weight: 900;
  background: rgba(15, 23, 42, 0.76);
  backdrop-filter: blur(12px);
}

.workflow-pills {
  flex-wrap: wrap;
  gap: 10px;
  margin: 24px 0;
}

.workflow-pills span {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 10px 12px;
  border-radius: 999px;
  color: var(--page-text);
  border: 1px solid var(--page-border);
  background: var(--page-surface);
  font-weight: 850;
}

.primary-cta.compact {
  width: fit-content;
}

.footer {
  padding: 28px 0 38px;
  border-top: 1px solid var(--page-border);
}

.footer-row {
  justify-content: space-between;
  gap: 18px;
}

.footer p {
  margin: 0;
}

.socials {
  gap: 12px;
  color: var(--page-muted);
}

@keyframes riseIn {
  from {
    opacity: 0;
    transform: translateY(28px) scale(0.98) perspective(1200px) rotateY(-7deg) rotateX(4deg);
  }
  to {
    opacity: 1;
    transform: perspective(1200px) rotateY(-7deg) rotateX(4deg);
  }
}

@keyframes floaty {
  0%, 100% {
    transform: translateY(0);
  }
  50% {
    transform: translateY(-12px);
  }
}

@keyframes drift {
  0%, 100% {
    transform: translate3d(0, 0, 0);
  }
  50% {
    transform: translate3d(34px, 22px, 0);
  }
}

@media (max-width: 1020px) {
  .nav-center {
    display: none;
  }

  .hero-grid,
  .video-grid,
  .workflow-grid {
    grid-template-columns: 1fr;
  }

  .hero-visual {
    min-height: auto;
  }

  .preview-shell {
    transform: none;
  }

  .floating-card {
    position: static;
    margin-top: 12px;
  }

  .module-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

@media (max-width: 680px) {
  .container {
    width: min(100% - 24px, 1180px);
  }

  .nav-content {
    min-height: auto;
    padding: 12px 0;
  }

  .logo span {
    display: none;
  }

  .nav-link {
    display: none;
  }

  .hero-section {
    padding: 50px 0 42px;
  }

  .hero-copy h1 {
    font-size: 40px;
  }

  .module-grid {
    grid-template-columns: 1fr;
  }

  .video-grid {
    padding: 18px;
    border-radius: 24px;
  }

  .footer-row {
    flex-direction: column;
    align-items: flex-start;
  }
}

@media (prefers-reduced-motion: reduce) {
  .preview-shell,
  .floating-card,
  .hero-orbit {
    animation: none;
  }
}
</style>
