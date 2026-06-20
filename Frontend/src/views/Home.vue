<script setup>
import {
  CheckCircle,
  Layout,
  Users,
  TrendingUp,
  Github,
  Twitter,
  Instagram,
  Facebook,
  ShieldCheck,
  RefreshCw,
  Clock,
  Code,
  Laptop,
  Briefcase,
  Building,
  ArrowRight
} from 'lucide-vue-next'

import dashboardPreview from '../assets/task_management_dashboard_preview_1773375713763.png'
import focusImage from '../assets/modern_desk_setup_product_design_1773375736492.png'
import logoImg from '../assets/logo_QLCV.png'
import { useI18n } from '@/composables/useI18n'

const { t } = useI18n()

const featureConfig = [
  {

    title: 'Quản lý công việc',
    desc: 'Theo dõi tiến độ, ưu tiên và trạng thái theo một quy trình rõ ràng.',

    titleKey: 'landing.features.work.title',
    descKey: 'landing.features.work.desc',

    icon: CheckCircle,
    color: '#0052CC',
    bg: '#DEEBFF'
  },
  {

    title: 'Kanban và quy trình làm việc',
    desc: 'Vận hành công việc qua backlog, to do, in progress, in review, done và cancelled.',

    titleKey: 'landing.features.kanban.title',
    descKey: 'landing.features.kanban.desc',

    icon: Layout,
    color: '#6554C0',
    bg: '#EAE6FF'
  },
  {

    title: 'Cộng tác nhóm',
    desc: 'Bình luận, giao việc, theo dõi thành viên và lưu vết thay đổi trong cùng một nơi.',

    titleKey: 'landing.features.team.title',
    descKey: 'landing.features.team.desc',

    icon: Users,
    color: '#00B8D9',
    bg: '#E6FCFF'
  },
  {

    title: 'Báo cáo và tổng hợp',
    desc: 'Tổng hợp thông tin để đội ngũ nhìn nhanh tình trạng dự án và vấn đề cần xử lý.',

    titleKey: 'landing.features.reports.title',
    descKey: 'landing.features.reports.desc',

    icon: TrendingUp,
    color: '#36B37E',
    bg: '#E3FCEF'
  }
]

const roles = [
  {
    title: 'Dành cho đội ngũ Phần mềm',
    desc: 'Lên kế hoạch, theo dõi và phát hành phần mềm chất lượng cao.',
    icon: Code,
    link: 'Tìm hiểu thêm'
  },
  {
    title: 'Dành cho đội ngũ IT',
    desc: 'Quản lý yêu cầu dịch vụ và sự cố một cách nhanh chóng.',
    icon: Laptop,
    link: 'Tìm hiểu thêm'
  },
  {
    title: 'Dành cho đội ngũ Kinh doanh',
    desc: 'Quản lý dự án, chiến dịch và quy trình làm việc dễ dàng.',
    icon: Briefcase,
    link: 'Tìm hiểu thêm'
  },
  {
    title: 'Dành cho doanh nghiệp',
    desc: 'Mở rộng quy mô Agile trên toàn doanh nghiệp với khả năng hiển thị cao.',
    icon: Building,
    link: 'Tìm hiểu thêm'
  }
]
</script>

<template>
  <div class="landing-page">
    <header class="navbar">
      <div class="container nav-content">
        <a href="/" class="logo">
          <img :src="logoImg" alt="SprintA Logo" class="custom-logo" />
          <span class="logo-text">SprintA</span>
        </a>

        <div class="nav-links hidden md:flex">
          <a href="#" class="nav-item">Tính năng</a>
          <a href="#" class="nav-item">Giải pháp</a>
          <a href="#" class="nav-item">Bảng giá</a>
          <a href="#" class="nav-item">Tài nguyên</a>
        </div>

        <div class="nav-actions">

          <router-link class="nav-link" to="/login">Đăng nhập</router-link>
          <router-link class="nav-primary" to="/register">Đăng ký mới</router-link>

          <router-link class="nav-link" to="/login">{{ t('landing.login') }}</router-link>
          <router-link class="nav-primary" to="/register">{{ t('landing.register') }}</router-link>

        </div>
      </div>
    </header>

    <section class="hero-section">
      <div class="container hero-grid">
        <div class="hero-copy">
          <div class="new-badge">GIẢI PHÁP KHÔNG GIAN LÀM VIỆC NHANH NHẸN</div>
          <h1 class="hero-title">Giải pháp quản lý dự án tối giản và chuyên nghiệp</h1>
          <p class="hero-sub">
            SprintA giúp các đội ngũ Agile lên kế hoạch, theo dõi và hoàn thành công việc một cách thông minh với quy trình chuẩn Atlassian/Jira. Đăng nhập hoặc đăng ký tài khoản để bắt đầu trải nghiệm ngay lập tức.
          </p>
          <div class="hero-btns">
            <router-link class="hero-primary" to="/login">Đăng nhập</router-link>
            <router-link class="hero-secondary" to="/register">Đăng ký mới</router-link>
          </div>

          <div class="new-badge">{{ t('landing.badge') }}</div>
          <h1 class="hero-title">{{ t('landing.heroTitle') }}</h1>
          <p class="hero-sub">
            {{ t('landing.heroSubtitle') }}
          </p>
          <div class="hero-btns">
            <router-link class="hero-primary" to="/login">{{ t('landing.login') }}</router-link>
            <router-link class="hero-secondary" to="/register">{{ t('landing.register') }}</router-link>
          </div>
        </div>

        <div class="dashboard-preview">
          <div class="img-wrap">
            <img :src="dashboardPreview" alt="Dashboard Preview" />
          </div>
        </div>
      </div>
    </section>

    <section class="trusted-section">
      <div class="container">
        <p class="trusted-title">HƠN 200,000+ ĐỘI NGŨ TRÊN TOÀN CẦU TIN DÙNG</p>
        <div class="trusted-logos">
          <span>Tập đoàn Acme</span>
          <span>Công nghệ toàn cầu</span>
          <span>Mối quan hệ</span>
          <span>Đồ họa đám mây</span>
          <span>Đổi mới</span>
          <span>Tiên phong</span>
        </div>
      </div>
    </section>

    <section class="features-section">
      <div class="container">

        <div class="section-copy-center">
          <h2 class="section-title">Mọi chức năng cốt lõi đều hỗ trợ quy trình Agile</h2>
          <p class="section-sub">Chúng tôi mang lại các công cụ cần thiết nhất nhằm tăng tốc độ bàn giao của nhóm mà không gây phiền toái hay phức tạp hóa công việc.</p>
        </div>

        <div class="section-copy">
          <h2 class="section-title">{{ t('landing.featuresTitle') }}</h2>
          <p class="section-sub">{{ t('landing.featuresSubtitle') }}</p>
        </div>

        <div class="feature-grid">
          <article v-for="feature in featureConfig" :key="feature.titleKey" class="feature-card">
            <div class="icon-wrap" :style="{ backgroundColor: feature.bg }">
              <component :is="feature.icon" :size="20" :color="feature.color" />
            </div>
            <h3>{{ t(feature.titleKey) }}</h3>
            <p>{{ t(feature.descKey) }}</p>
          </article>
        </div>
      </div>
    </section>

    <section class="focus-section">
      <div class="container focus-grid">
        <div class="focus-copy">

          <h2 class="section-title">Tập trung vào tính năng thực tế và bảo mật</h2>
          <p class="section-sub">
            Hệ thống xác thực nghiêm ngặt bảo vệ dữ liệu dự án của bạn. Sau khi đăng nhập, bạn sẽ được truy cập vào không gian làm việc nơi chứa các dashboard, bảng Kanban và danh sách công việc của dự án.
          </p>
          
          <ul class="focus-list">
            <li><ShieldCheck :size="18" color="#36B37E" /> Bảo mật cấp doanh nghiệp</li>
            <li><RefreshCw :size="18" color="#36B37E" /> Tích hợp quy trình chuẩn</li>
            <li><Clock :size="18" color="#36B37E" /> Báo cáo trực quan thời gian thực</li>
          </ul>

          <div class="hero-btns mt-4">
            <router-link class="hero-primary" to="/login">Đăng nhập ngay</router-link>
            <router-link class="hero-secondary" to="/register">Tìm hiểu thêm</router-link>
          </div>

          <h2 class="section-title">{{ t('landing.focusTitle') }}</h2>
          <p class="section-sub">
            {{ t('landing.focusSubtitle') }}
          </p>
          <div class="hero-btns">
            <router-link class="hero-primary" to="/login">{{ t('landing.login') }}</router-link>
            <router-link class="hero-secondary" to="/register">{{ t('landing.register') }}</router-link>

          </div>
        </div>

        <div class="focus-image">
          <div class="img-wrap">
            <img :src="focusImage" alt="Focus workspace" />
          </div>
        </div>
      </div>
    </section>

    <section class="roles-section">
      <div class="container">
        <div class="section-copy">
          <h2 class="section-title">Được thiết kế cho mọi đội ngũ</h2>
          <p class="section-sub">Khám phá cách SprintA giúp các bộ phận khác nhau làm việc cùng nhau một cách trơn tru.</p>
        </div>

        <div class="roles-grid">
          <article v-for="role in roles" :key="role.title" class="role-card">
            <div class="role-header">
              <component :is="role.icon" :size="20" color="#0052CC" />
              <h3>{{ role.title }}</h3>
            </div>
            <p>{{ role.desc }}</p>
            <a href="#" class="role-link">{{ role.link }} <ArrowRight :size="16" /></a>
          </article>
        </div>
      </div>
    </section>

    <section class="cta-banner">
      <div class="container cta-content">
        <h2>Sẵn sàng để bắt đầu?</h2>
        <p>Tham gia cùng hàng ngàn đội ngũ đã chuyển sang SprintA để quản lý dự án hiệu quả hơn.</p>
        <router-link class="cta-btn" to="/register">Đăng ký miễn phí</router-link>
      </div>
    </section>

    <footer class="footer">
      <div class="container footer-grid">
        <div class="footer-brand">
          <a href="/" class="logo">
            <img :src="logoImg" alt="SprintA Logo" class="custom-logo" />
            <span class="logo-text">SprintA</span>
          </a>

          <p class="footer-about">Hệ thống quản lý công việc tối ưu dành cho đội ngũ phát triển tinh gọn, kết hợp phong cách thiết kế Jira chuyên nghiệp.</p>
          <div class="socials">
            <Twitter :size="18" />
            <Github :size="18" />
            <Instagram :size="18" />
            <Facebook :size="18" />
          </div>
        </div>
        
        <div class="footer-links-col">
          <h4>SẢN PHẨM</h4>
          <a href="#">Tính năng</a>
          <a href="#">Bảng giá</a>
          <a href="#">Bảo mật</a>
          <a href="#">Tích hợp</a>

          <p class="footer-about">{{ t('landing.footerAbout') }}</p>

        </div>

        <div class="footer-links-col">
          <h4>TÀI NGUYÊN</h4>
          <a href="#">Tài liệu hướng dẫn</a>
          <a href="#">Bài viết</a>
          <a href="#">Cộng đồng</a>
          <a href="#">Trung tâm hỗ trợ</a>
        </div>

        <div class="footer-links-col">
          <h4>CÔNG TY</h4>
          <a href="#">Về chúng tôi</a>
          <a href="#">Tuyển dụng</a>
          <a href="#">Liên hệ</a>
          <a href="#">Đối tác</a>
        </div>
      </div>
      <div class="container footer-bottom">
        <div class="footer-bottom-links">
          <a href="#">Chính sách bảo mật</a>
          <a href="#">Điều khoản sử dụng</a>
          <a href="#">Cookie</a>
        </div>
        <p>© 2026 SprintA. Tất cả quyền được bảo lưu.</p>
      </div>
    </footer>
  </div>
</template>

<style scoped>
.landing-page {
  min-height: 100vh;
  width: 100%;
  overflow-x: hidden;
  background: #ffffff;
  color: #172B4D;
  font-family: 'Inter', system-ui, -apple-system, sans-serif;
}

.navbar {
  position: sticky;
  top: 0;
  z-index: 1000;
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(8px);
  border-bottom: 1px solid #DFE1E6;
}

.container {
  width: min(1200px, calc(100% - 32px));
  margin: 0 auto;
}

.nav-content {
  display: flex;
  align-items: center;
  justify-content: space-between;
  min-height: 72px;
}

.logo {
  display: inline-flex;
  align-items: center;
  color: inherit;
  text-decoration: none;
}

.custom-logo {
  height: 28px;
  object-fit: contain;
}

.logo-text {
  margin-left: 8px;
  font-size: 20px;
  font-weight: 800;
  color: #172B4D;
}

.nav-links {
  display: flex;
  gap: 32px;
}

.nav-item {
  color: #5E6C84;
  font-weight: 500;
  font-size: 15px;
  text-decoration: none;
  transition: color 0.2s;
}
.nav-item:hover {
  color: #0052CC;
}

.nav-actions {
  display: flex;
  align-items: center;
  gap: 12px;
}

.nav-link {
  color: #5E6C84;
  font-weight: 600;
  font-size: 14px;
  text-decoration: none;
  padding: 8px 12px;
  border-radius: 4px;
}
.nav-link:hover {
  background: rgba(9, 30, 66, 0.04);
  color: #172B4D;
}

.nav-primary, .hero-primary {
  background-color: #0052CC;
  color: #ffffff;
  padding: 10px 20px;
  border-radius: 4px;
  font-weight: 600;
  font-size: 14px;
  border: none;
  cursor: pointer;
  transition: background 0.2s;
  text-decoration: none;
  display: inline-flex;
  align-items: center;
  justify-content: center;
}
.nav-primary:hover, .hero-primary:hover {
  background-color: #0047B3;
}

.hero-primary {
  padding: 14px 24px;
  font-size: 16px;
}

.hero-secondary {
  background-color: transparent;
  color: #5E6C84;
  padding: 12px 24px;
  border-radius: 4px;
  font-weight: 600;
  font-size: 16px;
  border: 2px solid #DFE1E6;
  text-decoration: none;
  transition: all 0.2s;
}
.hero-secondary:hover {
  background-color: rgba(9, 30, 66, 0.04);
  color: #172B4D;
}

.hero-section {
  padding: 80px 0 60px;
  background: radial-gradient(circle at top right, rgba(0, 82, 204, 0.03), transparent 40%);
}

.hero-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 60px;
  align-items: center;
}

.hero-copy {
  display: flex;
  flex-direction: column;
  gap: 20px;
  max-width: 540px;
}

.new-badge {
  display: inline-flex;
  padding: 4px 12px;
  background: #DEEBFF;
  color: #0052CC;
  font-size: 11px;
  font-weight: 800;
  letter-spacing: 0.08em;
  text-transform: uppercase;
  border-radius: 4px;
  width: fit-content;
}

.hero-title {
  font-size: 48px;
  font-weight: 800;
  line-height: 1.15;
  color: #172B4D;
  margin: 0;
  letter-spacing: -0.01em;
}

.hero-sub {
  font-size: 18px;
  line-height: 1.6;
  color: #5E6C84;
  margin: 0;
}

.hero-btns {
  display: flex;
  gap: 16px;
  margin-top: 8px;
}

.img-wrap {
  border-radius: 12px;
  background: #F4F5F7;
  padding: 24px;
  box-shadow: 0 20px 40px rgba(9, 30, 66, 0.08);
}

.img-wrap img {
  width: 100%;
  display: block;
  border-radius: 8px;
  box-shadow: 0 4px 12px rgba(9, 30, 66, 0.1);
  border: 1px solid #DFE1E6;
}

.trusted-section {
  padding: 40px 0;
  border-top: 1px solid #DFE1E6;
  border-bottom: 1px solid #DFE1E6;
  background: #FAFBFC;
  text-align: center;
}

.trusted-title {
  font-size: 11px;
  font-weight: 700;
  color: #8993A4;
  letter-spacing: 0.08em;
  margin-bottom: 24px;
}

.trusted-logos {
  display: flex;
  justify-content: center;
  align-items: center;
  flex-wrap: wrap;
  gap: 48px;
  color: #A5ADBA;
  font-weight: 700;
  font-size: 20px;
}

.features-section, .roles-section {
  padding: 80px 0;
  background: #ffffff;
}

.section-copy-center {
  text-align: center;
  max-width: 680px;
  margin: 0 auto 56px;
}

.section-copy {
  max-width: 600px;
  margin-bottom: 48px;
}

.section-title {
  font-size: 32px;
  font-weight: 800;
  color: #172B4D;
  margin: 0 0 16px 0;
  letter-spacing: -0.01em;
}

.section-sub {
  font-size: 18px;
  line-height: 1.6;
  color: #5E6C84;
  margin: 0;
}

.feature-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 24px;
}

.feature-card {
  padding: 32px 24px;
  border-radius: 8px;
  background: #ffffff;
  border: 1px solid #DFE1E6;
  box-shadow: 0 2px 8px rgba(9, 30, 66, 0.04);
  transition: transform 0.2s, box-shadow 0.2s;
}
.feature-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 24px rgba(9, 30, 66, 0.08);
}

.icon-wrap {
  width: 40px;
  height: 40px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border-radius: 8px;
  margin-bottom: 20px;
}

.feature-card h3 {
  font-size: 18px;
  font-weight: 700;
  color: #172B4D;
  margin: 0 0 12px 0;
}

.feature-card p, .role-card p {
  font-size: 14px;
  color: #5E6C84;
  line-height: 1.6;
  margin: 0;
}

.focus-section {
  padding: 80px 0;
  background: #FAFBFC;
}

.focus-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 80px;
  align-items: center;
}

.focus-copy {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.focus-list {
  list-style: none;
  padding: 0;
  margin: 8px 0 0 0;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.focus-list li {
  display: flex;
  align-items: center;
  gap: 12px;
  font-size: 16px;
  font-weight: 500;
  color: #172B4D;
}

.roles-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 24px;
}

.role-card {
  padding: 32px;
  border-radius: 8px;
  background: #FAFBFC;
  border: none;
  transition: background 0.2s;
}
.role-card:hover {
  background: #F4F5F7;
}

.role-header {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 12px;
}

.role-header h3 {
  font-size: 18px;
  font-weight: 700;
  color: #172B4D;
  margin: 0;
}

.role-link {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  margin-top: 16px;
  color: #0052CC;
  font-weight: 600;
  font-size: 14px;
  text-decoration: none;
}
.role-link:hover {
  text-decoration: underline;
}

.cta-banner {
  background: #0052CC;
  padding: 80px 0;
  text-align: center;
  color: #ffffff;
}

.cta-content {
  max-width: 600px;
  margin: 0 auto;
}

.cta-content h2 {
  font-size: 36px;
  font-weight: 800;
  margin: 0 0 16px 0;
  color: #ffffff;
}

.cta-content p {
  font-size: 18px;
  color: #DEEBFF;
  margin: 0 0 32px 0;
  line-height: 1.6;
}

.cta-btn {
  background: #ffffff;
  color: #0052CC;
  padding: 14px 28px;
  border-radius: 4px;
  font-weight: 700;
  font-size: 16px;
  text-decoration: none;
  display: inline-block;
  transition: transform 0.2s;
}
.cta-btn:hover {
  transform: translateY(-2px);
}

.footer {
  background: #ffffff;
  padding: 64px 0 24px;
  border-top: 1px solid #DFE1E6;
}

.footer-grid {
  display: grid;
  grid-template-columns: 2fr 1fr 1fr 1fr;
  gap: 48px;
  margin-bottom: 48px;
}

.footer-brand .logo {
  margin-bottom: 20px;
}

.footer-about {
  color: #5E6C84;
  font-size: 14px;
  line-height: 1.6;
  margin: 0 0 24px 0;
  max-width: 320px;
}

.socials {
  display: flex;
  gap: 16px;
  color: #5E6C84;
}

.socials svg {
  cursor: pointer;
  transition: color 0.2s;
}
.socials svg:hover {
  color: #0052CC;
}

.footer-links-col h4 {
  font-size: 11px;
  font-weight: 800;
  color: #172B4D;
  margin: 0 0 16px 0;
  letter-spacing: 0.08em;
}

.footer-links-col a {
  display: block;
  color: #5E6C84;
  text-decoration: none;
  font-size: 14px;
  margin-bottom: 12px;
  transition: color 0.2s;
}
.footer-links-col a:hover {
  color: #0052CC;
}

.footer-bottom {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-top: 24px;
  border-top: 1px solid #DFE1E6;
  font-size: 13px;
  color: #8993A4;
}

.footer-bottom-links {
  display: flex;
  gap: 24px;
}

.footer-bottom-links a {
  color: inherit;
  text-decoration: none;
}
.footer-bottom-links a:hover {
  text-decoration: underline;
}

@media (max-width: 1024px) {
  .hero-grid, .focus-grid {
    grid-template-columns: 1fr;
    gap: 40px;
  }
  .feature-grid {
    grid-template-columns: repeat(2, 1fr);
  }
  .footer-grid {
    grid-template-columns: 1fr 1fr;
  }
  .trusted-logos {
    gap: 24px;
    font-size: 16px;
  }
}

@media (max-width: 768px) {
  .hero-title {
    font-size: 36px;
  }
  .roles-grid {
    grid-template-columns: 1fr;
  }
  .footer-bottom {
    flex-direction: column;
    gap: 16px;
    text-align: center;
  }
}
</style>
