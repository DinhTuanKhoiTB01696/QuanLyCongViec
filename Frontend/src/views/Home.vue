<script setup>
import { 
  CheckCircle, 
  Layout, 
  Users, 
  TrendingUp, 
  Play, 
  ArrowRight,
  Zap,
  Clock,
  Github,
  Twitter,
  Instagram,
  Facebook
} from 'lucide-vue-next'
import HelpDropdown from '../components/HelpDropdown.vue'

import dashboardPreview from '../assets/task_management_dashboard_preview_1773375713763.png'
import focusImage from '../assets/modern_desk_setup_product_design_1773375736492.png'
import logoImg from '../assets/logo_QLCV.png'

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

// Dữ liệu mặc định trống
const kanbanColumns = [
  {
    title: 'TO DO',
    count: 1,
    items: [
      { id: 'SCRUM-1', title: 'Task 1', date: 'Mar 17, 2026', overdue: true, icon: 'square-check' }
    ]
  },
  {
    title: 'IN PROGRESS',
    count: 1,
    items: [
      { id: 'SCRUM-2', title: 'Task 2', date: 'Mar 22, 2026', overdue: false, icon: 'square-check', active: true }
    ]
  },
  {
    title: 'CHỜ DUYỆT',
    count: 0,
    items: []
  },
  {
    title: 'DONE',
    count: 0,
    items: [],
    done: true
  }
]
</script>

<template>
  <div class="landing-page" data-theme="light">
    <!-- Navbar -->
    <header class="navbar">
      <div class="container nav-content">
        <a href="/" class="logo">
          <img :src="logoImg" alt="SprintA Logo" class="custom-logo" />
          <span class="logo-text">SprintA</span>
        </a>
        <div class="nav-actions">
          <el-button link @click="$router.push('/login')">Đăng nhập</el-button>
          <el-button type="primary" round @click="$router.push('/register')">Đăng ký</el-button>
        </div>
      </div>
    </header>

    <!-- Hero Section -->
    <section class="hero-section text-center section-padding">
      <div class="container animate-fade-up">
        <div class="new-badge">
          <span>MỚI</span> Có gì mới trong SprintA v3.0?
        </div>
        <h1 class="hero-title">
          Tổ chức Công việc. Quản lý Dự án.<br />
          <span class="text-primary">Tiến độ Thần tốc.</span>
        </h1>
        <p class="hero-sub">
          Nền tảng quản lý công việc tất-cả-trong-một dành cho các đội ngũ muốn tạo ra<br class="desktop-only" />
          sản phẩm xuất sắc mà không bị rối loạn.
        </p>
        <div class="hero-btns">
          <el-button type="primary" size="large" round class="btn-cta" @click="$router.push('/register')">Bắt đầu Miễn phí</el-button>
        </div>

        <div class="dashboard-preview">
          <img :src="dashboardPreview" alt="Dashboard Preview" />
        </div>
      </div>
    </section>

    <!-- Features Grid -->
    <section class="features-section section-padding">
      <div class="container text-center">
        <h2 class="section-title">Mọi thứ bạn cần đều ở đây</h2>
        <p class="section-sub">Tối ưu hóa quy trình làm việc bằng các công cụ cấp doanh nghiệp.</p>
        
        <el-row :gutter="32" class="feature-grid">
          <el-col :xs="24" :sm="12" :md="6" v-for="f in features" :key="f.title">
            <div class="feature-card">
              <div class="icon-wrap" :style="{ backgroundColor: f.bg }">
                <component :is="f.icon" :size="24" :color="f.color" />
              </div>
              <h3>{{ f.title }}</h3>
              <p>{{ f.desc }}</p>
            </div>
          </el-col>
        </el-row>
      </div>
    </section>

    <section class="pipeline-section section-padding bg-white">
      <div class="container animate-fade-up">
        <div class="pipeline-header">
          <h2 class="text-black">Dòng chảy Công việc Khép kín</h2>
          <div class="header-action">
            <a href="#" class="view-all text-black">Xem tất cả biểu mẫu <ArrowRight :size="16" /></a>
          </div>
        </div>
        
        <div class="kanban-container-home">
          <el-row :gutter="20" class="kanban-wrapper-home">
            <el-col :xs="24" :sm="12" :md="6" v-for="col in kanbanColumns" :key="col.title">
              <div class="kanban-col-home">
                <div class="col-head-home">
                  <span class="col-title">{{ col.title }}</span>
                  <span v-if="!col.done" class="col-count">{{ col.count }}</span>
                  <i v-if="col.title === 'IN PROGRESS'" class="fa-solid fa-ellipsis ml-auto"></i>
                  <i v-if="col.done" class="fa-solid fa-check-double ml-auto done-icon"></i>
                </div>
                
                <div class="tasks-container">
                  <div v-for="task in col.items" :key="task.id" class="task-card-home" :class="{ 'active-task': task.active }">
                    <div class="card-top">
                      <h4 class="task-title">{{ task.title }}</h4>
                      <i v-if="task.active" class="fa-regular fa-pen-to-square edit-icon"></i>
                      <i v-if="task.active" class="fa-solid fa-ellipsis more-icon"></i>
                    </div>
                    
                    <div class="card-mid">
                      <span class="date-badge-home" :class="{ 'overdue': task.overdue }">
                        <i :class="task.overdue ? 'fa-solid fa-triangle-exclamation' : 'fa-regular fa-calendar'"></i>
                        {{ task.date }}
                      </span>
                    </div>
                    
                    <div class="card-bottom">
                      <div class="task-id-home">
                        <i class="fa-regular fa-square-check"></i> {{ task.id }}
                      </div>
                      <div class="bottom-right">
                        <span v-if="task.active" class="link-tag-home">-</span>
                        <i v-if="task.active" class="fa-solid fa-network-wired"></i>
                        <div class="avatar-home">
                          <i class="fa-solid fa-user"></i>
                        </div>
                      </div>
                    </div>
                  </div>
                  
                  <div v-if="col.items.length === 0" class="empty-placeholder-home">
                    Trống
                  </div>
                  
                  <div class="btn-create-task-home">
                    <i class="fa-solid fa-plus"></i> Create
                  </div>
                </div>
              </div>
            </el-col>
          </el-row>
        </div>
      </div>
    </section>

    <!-- Focus Section -->
    <section class="focus-section section-padding">
      <div class="container">
        <el-row align="middle" :gutter="40">
          <el-col :xs="24" :md="12">
            <div class="focus-content">
              <h2 class="section-title text-left">Thiết kế để Tập trung</h2>
              <p class="section-sub text-left">Loại bỏ sự phiền nhiễu với các công cụ được xây dựng cho công việc chuyên sâu và năng suất cao.</p>
              
              <div class="focus-item">
                <div class="item-icon"><Zap :size="20" color="#0061ff" /></div>
                <div class="item-text">
                  <h4>Trang chủ Cá Nhân hóa</h4>
                  <p>Bảng điều khiển trung tâm cho công việc hàng ngày, thời hạn sắp tới và các thông báo.</p>
                </div>
              </div>
              <div class="focus-item">
                <div class="item-icon"><Clock :size="20" color="#0061ff" /></div>
                <div class="item-text">
                  <h4>Lọc theo độ Ưu tiên</h4>
                  <p>Hệ thống lọc thông minh giúp bạn tập trung tuyệt đối vào việc quan trọng nhất ngay lúc này.</p>
                </div>
              </div>
            </div>
          </el-col>
          <el-col :xs="24" :md="12">
            <div class="focus-image">
              <img :src="focusImage" alt="Focus" />
            </div>
          </el-col>
        </el-row>
      </div>
    </section>

    <!-- CTA Section -->
    <section class="cta-section container">
      <div class="cta-card">
        <h2>Bắt đầu Quản lý Công việc<br />Hiệu quả hơn</h2>
        <p>Gia nhập 10,000+ đội ngũ đã tối ưu hóa quy trình làm việc với SprintA. Không cần liên kết thẻ.</p>
        <el-button type="primary" size="large" class="cta-primary-btn" @click="$router.push('/register')">Tạo Tài Khoản Miễn Phí</el-button>
        <p class="small-note">Miễn phí cho nhóm dưới 5 thành viên</p>
      </div>
    </section>

    <!-- Footer -->
    <footer class="footer">
      <div class="container">
        <el-row :gutter="40" class="footer-row">
          <el-col :xs="24" :md="8">
            <a href="/" class="logo">
              <img :src="logoImg" alt="SprintA Logo" class="custom-logo" />
              <span class="logo-text">SprintA</span>
            </a>
            <p class="footer-about">
              Trao quyền cho đội ngũ bàn giao sản phẩm bứt phá rào cản thông qua hệ thống quản lý chuẩn Agile và cộng tác liền mạch.
            </p>
            <div class="socials">
              <Twitter :size="20" />
              <Github :size="20" />
              <Instagram :size="20" />
              <Facebook :size="20" />
            </div>
          </el-col>
          <el-col :xs="12" :md="4">
            <h4>Sản Phẩm</h4>
            <ul>
              <li><a href="#">Tính năng</a></li>
              <li><a href="#">Tích hợp</a></li>
              <li><a href="#">Biểu mẫu</a></li>
              <li><a href="#">Cập nhật</a></li>
            </ul>
          </el-col>
          <el-col :xs="12" :md="4">
            <h4>Công Ty</h4>
            <ul>
              <li><a href="#">Về chúng tôi</a></li>
              <li><a href="#">Tuyển dụng</a></li>
              <li><a href="#">Blog</a></li>
              <li><a href="#">Báo chí</a></li>
            </ul>
          </el-col>
          <el-col :xs="12" :md="4">
            <h4>Hỗ trợ</h4>
            <ul>
              <li><a href="#">Trung tâm trợ giúp</a></li>
              <li><a href="#">Trạng thái hệ thống</a></li>
              <li><a href="#">Tài liệu API</a></li>
              <li><a href="#">Bảo mật</a></li>
            </ul>
          </el-col>
        </el-row>
        <div class="footer-bottom">
          <p>© 2024 SprintA Technology Inc. Đã đăng ký bản quyền.</p>
          <div class="bottom-links">
            <a href="#">Chính sách bảo mật</a>
            <a href="#">Điều khoản dịch vụ</a>
            <a href="#">Cookies</a>
          </div>
        </div>
      </div>
    </footer>
  </div>
</template>

<style scoped>
.landing-page {
  background-color: #ffffff;
  color: #1a1a1a;
  min-height: 100vh;
  overflow-x: hidden;
}

/* Navbar */
.navbar {
  height: 80px;
  display: flex;
  align-items: center;
  position: sticky;
  top: 0;
  background: rgba(255, 255, 255, 0.9);
  backdrop-filter: blur(10px);
  z-index: 1000;
  border-bottom: 1px solid #f0f0f0;
}

.nav-content {
  display: flex;
  align-items: center;
  justify-content: space-between;
  width: 100%;
}

.logo {
  display: flex;
  align-items: center;
  text-decoration: none;
  color: #1a1a1a;
}

.custom-logo {
  height: 60px;
  width: auto;
  object-fit: contain;
}

.logo-text {
  font-weight: 900;
  font-size: 24px;
  letter-spacing: -1px;
  margin-left: -10px;
}

.nav-actions {
  display: flex;
  align-items: center;
  gap: 16px;
}

/* Hero Section */
.hero-section {
  padding-top: 60px;
  padding-bottom: 0;
}

.hero-title {
  font-size: clamp(2rem, 5vw, 4rem);
  font-weight: 800;
  line-height: 1.1;
  margin-bottom: 24px;
  color: #0d121f;
}

.hero-sub {
  font-size: 1.1rem;
  color: #64748b;
  line-height: 1.6;
  margin-bottom: 40px;
}

.btn-cta {
  padding: 24px 48px;
  font-size: 1.1rem;
}

.dashboard-preview {
  margin-top: 60px;
  max-width: 1000px;
  margin-left: auto;
  margin-right: auto;
  border-radius: 20px 20px 0 0;
  overflow: hidden;
  box-shadow: 0 -20px 60px rgba(0, 0, 0, 0.1);
}

.dashboard-preview img {
  width: 100%;
  display: block;
}

/* Features */
.section-title {
  font-size: 2.25rem;
  margin-bottom: 16px;
}

.section-sub {
  font-size: 1.1rem;
  color: #64748b;
  margin-bottom: 60px;
}

.feature-card {
  padding: 32px;
  text-align: left;
  background: white;
  border-radius: 24px;
  transition: all 0.3s ease;
  height: 100%;
}

.feature-card:hover {
  transform: translateY(-8px);
  box-shadow: 0 20px 40px rgba(0,0,0,0.05);
}

/* Pipeline Home */
.bg-white { background-color: #ffffff !important; }
.kanban-container-home { padding: 40px 0; }
.kanban-wrapper-home { display: flex; flex-wrap: wrap; }
.kanban-col-home { background-color: #f4f5f7; border-radius: 12px; padding: 16px 14px; height: 100%; display: flex; flex-direction: column; border: 1px solid #ebecf0; }
.col-head-home { display: flex; align-items: center; padding: 0 4px 16px; color: #44546f; }
.col-title { font-size: 12px; font-weight: 700; letter-spacing: 0.5px; }
.col-count { margin-left: 8px; background-color: #ebecf0; color: #44546f; border-radius: 10px; padding: 2px 8px; font-size: 11px; font-weight: 700; }
.done-icon { color: #1f845a; }
.ml-auto { margin-left: auto; }

.task-card-home { background-color: white; border: 1px solid #ebecf0; border-radius: 8px; padding: 16px; margin-bottom: 12px; cursor: pointer; box-shadow: 0 1px 1px rgba(0,0,0,0.05); transition: all 0.2s; }
.task-card-home:hover { transform: translateY(-2px); box-shadow: 0 8px 16px rgba(0,0,0,0.04); border-color: #3b82f644; }
.active-task { border-left: 3px solid #3b82f6; }
.card-top { display: flex; align-items: center; gap: 8px; margin-bottom: 12px; }
.task-title { font-size: 15px; color: #172b4d; flex: 1; margin: 0; font-weight: 600; }
.edit-icon, .more-icon { font-size: 14px; color: #626f84; opacity: 0.6; }
.card-mid { margin-bottom: 12px; }
.date-badge-home { display: inline-flex; align-items: center; gap: 6px; padding: 4px 10px; background-color: #f4f5f7; border-radius: 4px; font-size: 11px; color: #44546f; font-weight: 600; }
.date-badge-home.overdue { background-color: #ffebe6; color: #bf2600; }
.card-bottom { display: flex; justify-content: space-between; align-items: center; }
.task-id-home { font-size: 12px; color: #626f84; display: flex; align-items: center; gap: 6px; font-weight: 500; }
.bottom-right { display: flex; align-items: center; gap: 10px; color: #626f84; }
.link-tag-home { background-color: #ebecf0; padding: 0 6px; border-radius: 4px; font-size: 11px; font-weight: 700; }
.avatar-home { width: 24px; height: 24px; border-radius: 50%; background-color: #dfe1e6; display: flex; align-items: center; justify-content: center; font-size: 11px; color: #44546f; }

.empty-placeholder-home { text-align: center; padding: 24px; border: 2px dashed #ebecf0; border-radius: 12px; color: #94a3b8; font-size: 13px; margin-bottom: 12px; }
.btn-create-task-home { padding: 8px 0; color: #626f84; font-size: 14px; cursor: pointer; border-radius: 4px; display: flex; align-items: center; gap: 8px; font-weight: 500; }
.btn-create-task-home:hover { color: #172b4d; }

/* Focus */
.focus-content {
  padding-right: 0;
}

.focus-image img {
  width: 100%;
  border-radius: 24px;
  margin-top: 40px;
}

/* CTA */
.cta-section {
  margin-top: 100px;
  margin-bottom: 120px;
}

.cta-card {
  background: #0d121f;
  padding: 80px 40px;
  border-radius: 32px;
  text-align: center;
  color: #ffffff;
  box-shadow: 0 20px 40px rgba(0,0,0,0.1);
}

.cta-card h2 {
  font-size: 2.5rem;
  margin-bottom: 24px;
  color: #ffffff;
}

/* Footer */
.footer {
  background: #0d121f;
  padding: 80px 0 40px;
  color: #ffffff;
}

.footer-row {
  margin-bottom: 40px;
}

.footer-about {
  margin: 20px 0;
  opacity: 0.7;
}

.socials {
  display: flex;
  gap: 16px;
  margin-bottom: 40px;
}

.footer ul {
  list-style: none;
  padding: 0;
}

.footer ul li {
  margin-bottom: 12px;
}

.footer ul li a {
  color: #ffffff;
  opacity: 0.7;
  text-decoration: none;
}

.footer-bottom {
  border-top: 1px solid rgba(255,255,255,0.1);
  padding-top: 32px;
  display: flex;
  flex-direction: column;
  gap: 20px;
  align-items: center;
  text-align: center;
}

@media (min-width: 768px) {
  .footer-bottom {
    flex-direction: row;
    justify-content: space-between;
    text-align: left;
  }
}

.desktop-only {
  display: none;
}

@media (min-width: 992px) {
  .desktop-only {
    display: flex;
  }
  .focus-image img {
    margin-top: 0;
  }
}
</style>
