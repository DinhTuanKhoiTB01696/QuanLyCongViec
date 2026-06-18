# BÁO CÁO KIỂM TOÁN FRONTEND (FRONTEND AUDIT REPORT)

Đây là báo cáo tổng hợp kết quả rà soát toàn bộ cấu trúc mã nguồn frontend của dự án Task Management (SprintA). Báo cáo được xây dựng dựa trên việc đọc trực tiếp source code hiện tại, phân loại và đánh giá tính đồng bộ UI/UX.

---

## 1. Tổng quan frontend
- **Kiến trúc thư mục**: [A] Dự án được tổ chức theo cấu trúc chuẩn của Vue, bao gồm các thư mục chính trong `src/`: `api/`, `assets/`, `components/` (chia làm layout, common, profile, admin), `router/`, `store/`, `utils/`, và `views/` (phân nhánh rất chi tiết).
- **Framework / thư viện chính**: [A] Vue 3 (Composition API), Vite, TailwindCSS (PostCSS plugin), Element Plus (UI Component), Pinia (State Management), Vue Router. Các thư viện phụ trợ: Tiptap (Rich text editor), Echarts/Apexcharts, V-Calendar.
- **Cách routing hoạt động**: [A] Routing phân mảnh thành nhiều file: `authRoutes`, `homeRoutes`, `dashboardRoutes`, `spaceRoutes`, `siteRoutes`, `adminRoutes`. `router/index.js` có chứa global guard (`beforeEach`) để kiểm tra xác thực (token), phân quyền (role) và Project Settings access.
- **Layout tổng thể**: [A] Hệ thống có 3 bộ layout chính: `HomeSiteLayout.vue` (trang chủ quản lý site), `NexusLayout.vue` (cho khu vực làm việc Space/Board), và `AdminLayout.vue` (cho cài đặt hệ thống).
- **Quản lý Theme**: [A] Sử dụng file `utils/theme.js` lưu `data-theme` vào thẻ HTML và LocalStorage. Có đồng bộ theme với Backend.

---

## 2. Danh sách tất cả trang / màn hình

### 2.1. Nhóm Auth / Public
- **Login, Register**: [A] `views/Login.vue`, `views/Register.vue` - Form đăng nhập/đăng ký, có tích hợp Google/GitHub.
- **GitHubCallback**: [A] `views/GitHubCallback.vue` - Xử lý redirect Oauth.
- **AcceptInvite**: [A] `views/AcceptInvite.vue` - Tham gia Workspace.

### 2.2. Nhóm Site / Global Navigation
- **SiteSelection**: [A] `views/SiteSelection.vue` - Màn hình chọn Site làm việc sau khi login.
- **SitesForYou**: [A] `views/SitesForYou.vue` - Dashboard tổng hợp.

### 2.3. Nhóm HomeSite (Quản lý chung cấp Site)
- **Teams**: [A] `views/HomeSite/Teams/TeamsDashboard.vue`, `TeamList.vue`, `TeamDetail.vue`, `TeamKudos.vue`.
- **Goals**: [A] `views/HomeSite/Goals/GoalsDashboard.vue`, `GoalDetail.vue`.
- **Projects**: [A] `views/HomeSite/Projects/ProjectList.vue`, `ProjectDetail.vue`.
- **People**: [A] `views/HomeSite/People/PeopleDirectory.vue`, `ProfileDetail.vue`.
- **Tools**: [A] `AuditLog.vue`, `RecentActivities.vue`, `StarredList.vue`, `NotificationsView.vue`, `SystemStatus.vue`.

### 2.4. Nhóm Space / Project Workspace (Layout Nexus)
- **SpaceDashboard / SpaceSummary**: [A] `views/SpaceDashboard.vue`, `views/SpaceSummary.vue` - Chứa Backlog, Board, Work Items.
- **Modules nội bộ**: [A] `CyclesView.vue` (Sprint), `IntakesView.vue`, `ModulesView.vue`, `PagesView.vue` (Docs), `ReportsView.vue`, `ViewsView.vue`, `ProjectSettings.vue`.

### 2.5. Nhóm Global Dashboard
- **YourWork / Drafts / Stickies / Rewards**: [A] Quản lý cá nhân (`YourWorkView.vue`, `DraftsView.vue`, `StickiesView.vue`, `RewardsView.vue`).
- **Global Archives/Trash/Analytics**: [A] `GlobalArchivesView.vue`, `GlobalTrashView.vue`, `GlobalAnalyticsView.vue`.

### 2.6. Nhóm Admin
- **User/Role**: [A] `views/admin/UserManagement.vue`, `RoleManagement.vue`.
- **Configuration**: [A] `Configuration.vue`, `OrganizationProfile.vue`, `SystemInfo.vue`, các trang Security (2FA, Password, IP Whitelist).

---

## 3. Danh sách component dùng chung

- **AppTopBar**: [A] `components/layout/AppTopBar.vue`. Dùng làm thanh điều hướng trên cùng, chứa các Dropdown.
- **NexusSidebar / AdminSidebar**: [A] Sidebar linh hoạt thay đổi theo ngữ cảnh (Space hoặc Admin).
- **TaskDetailModal**: [A] `components/TaskDetailModal.vue`. Component siêu to (186KB) dùng để hiển thị chi tiết thẻ task (Side panel / Modal). Cần chia nhỏ [CẦN XÁC NHẬN].
- **CreateProjectModal / CreateSpaceModal**: [A] Popup tạo dự án/không gian dùng chung.
- **Dropdowns**: [A] `UserDropdown`, `SettingsDropdown`, `NotificationsDropdown`, `HelpDropdown`, `StarredDropdown`, `RecentDropdown`. Đang để rải rác ngoài `components/` thay vì gom vào thư mục con `dropdowns/`.
- **RichTextEditor**: [A] `components/common/RichTextEditor.vue` sử dụng Tiptap.
- **ShareModal**: [A] `components/common/ShareModal.vue`.

---

## 4. UI consistency audit (Đánh giá đồng bộ UI)

- **Button Style**: [A] Đang sử dụng lộn xộn giữa Element Plus (`<el-button>`) và button HTML thuần dùng class Tailwind (`<button class="send-btn">` trong NexusLayout).
- **Input / Select / Textarea**: [A] Tương tự button, có sự kết hợp chưa nhất quán giữa component của Element Plus và form native style Tailwind.
- **Modal / Popup**: [A] Đa phần dùng `<el-dialog>`, nhưng một số sidebar AI (trong NexusLayout) được tự code bằng CSS thuần với `transition: transform`.
- **Typography & Màu sắc**: [A] Đã thiết lập được hệ thống biến CSS (`--color-bg`, `--color-surface`, `--color-text-primary`, `--color-accent`) trong `style.css` và `theme.js`. Tuy nhiên, vẫn có hiện tượng hardcode mã màu như `#3b82f6` (trong NexusLayout.vue).
- **Sidebar**: [A] Responsive khá tốt (có overlay khi mở ở mobile).
- **Empty state / Loading**: [A] Component `App.vue` có xử lý màn hình loading toàn cục (`loader-spinner`), nhưng các component con có thể đang thiếu empty state [CẦN XÁC NHẬN].
- **Icon size**: [A] Dùng lẫn lộn giữa `@element-plus/icons-vue` và FontAwesome (`<i class="fa-solid...">`).

---

## 5. Frontend issues / bugs / inconsistencies

- **[P1] Lộn xộn thư viện Icon**: [A] Dùng đồng thời FontAwesome (`fa-solid`) và Element Icons, Lucide Icons (`lucide-vue-next` có trong `package.json`).
  - *Đề xuất*: Chuẩn hóa dùng 1 bộ icon duy nhất (VD: Lucide cho giống Jira).
- **[P1] Kích thước file TaskDetailModal quá lớn**: [A] File `TaskDetailModal.vue` nặng hơn 186KB, chứa quá nhiều logic (Render bình luận, lịch sử, subtask...).
  - *Đề xuất*: Tách thành `TaskComments.vue`, `TaskHistory.vue`, v.v.
- **[P2] Hardcode CSS trong Component**: [A] Trong `NexusLayout.vue` có hardcode màu `#3b82f6` thay vì dùng biến CSS hệ thống.
  - *Đề xuất*: Đổi thành `var(--color-primary)`.
- **[P2] Cấu trúc thư mục Dropdown**: [A] Quá nhiều file `*Dropdown.vue` nằm trực tiếp ở `src/components/`, gây rối.
  - *Đề xuất*: Gom vào thư mục `src/components/dropdowns/`.

---

## 6. Những phần đã làm tốt

- **[A] Kiến trúc State Management**: Dùng Pinia phân chia rất rõ ràng, module hóa cao (`useGoalStore.js`, `useProjectStore.js`, `usePeopleStore.js`, `useWorkTaskStore.js`...).
- **[A] Hệ thống Routing**: Tách file route theo domain (auth, admin, space, home) giúp file `index.js` rất gọn gàng và dễ bảo trì.
- **[A] Quản lý Theme**: Cơ chế theme dark/light được xây dựng bài bản bằng CSS variables, có lưu vào backend và localstorage.
- **[A] Lazy Loading**: Tất cả các màn hình (views) đều sử dụng hàm `import()` để lazy load, giúp tăng tốc độ tải trang ban đầu.

---

## 7. Những phần còn thiếu

- **[A] Chưa thấy component báo lỗi toàn cục chi tiết**: Có `ErrorBoundary.vue` nhưng cơ chế hiển thị 404 Not Found Page chưa rõ ràng ở cấp độ Route [CẦN XÁC NHẬN].
- **[A] UI Component System nội bộ**: Chưa có thư mục chứa các component UI cơ bản (BaseButton, BaseInput) được bọc lại từ ElementPlus để đảm bảo 100% đồng bộ thiết kế.
- **[C] Trạng thái rỗng (Empty States)**: Ở các bảng dữ liệu hoặc danh sách Goal/Project/Task chưa rõ có hiển thị hình ảnh minh họa khi trống không [CẦN XÁC NHẬN].

---

## 8. Kế hoạch chuẩn hóa UI (Jira-style Design System)

Để UI mang cảm giác "Enterprise" giống Jira Atlassian, cần quy hoạch:
1. **Màu sắc**: Giữ nguyên cơ chế CSS Variables. Bổ sung các dải màu cho Status (To Do: Xám, In Progress: Xanh dương, Done: Xanh lá).
2. **Typography**: Đồng nhất font `Inter` ở mọi nơi. Bỏ các thẻ `<h1/2/3>` tự do, thay bằng các class chuẩn như `.text-heading-lg`.
3. **Icons**: Chuyển toàn bộ sang `lucide-vue-next` để nét icon đồng đều, hiện đại.
4. **Button Variants**: Hạn chế dùng button HTML thuần. Bọc `el-button` thành component `JiraButton.vue` với các props: `primary, default, subtle, link, warning`.
5. **Form Controls**: Chuẩn hóa viền (border) của Input khi focus sang màu xanh (Accent color) kèm shadow nhẹ.
6. **Card & Shadow**: Sử dụng viền xám nhạt (`1px solid var(--color-border)`) thay vì bóng đổ (shadow) quá đậm, chỉ dùng shadow cho Dropdown và Modal.

---

## 9. Danh sách file quan trọng

- **Entry points**: `src/main.js`, `src/App.vue`.
- **Layouts**: `src/components/layout/NexusLayout.vue`, `AppTopBar.vue`, `HomeSiteLayout.vue`.
- **Router**: `src/router/index.js` (nơi xử lý Logic Auth Guard).
- **Store**: `src/store/useWorkTaskStore.js`, `useProjectStore.js` (Logic nghiệp vụ chính).
- **Core Views**: `src/views/SpaceSummary.vue` (Chứa board/backlog), `src/views/SiteSelection.vue`.
- **🔥 File cần ưu tiên Refactor**: `TaskDetailModal.vue` (quá lớn), `NexusLayout.vue` (lẫn lộn CSS cứng).

---

## 10. Kết luận cuối

- **Mức độ hoàn thiện**: [A] Về mặt khung sườn, Frontend đã được dựng rất đồ sộ, đầy đủ các luồng từ quản trị hệ thống (Admin), quản lý tổng thể (HomeSite) đến quản lý chi tiết dự án (Space). Logic API và Router đã được nối rất kỹ.
- **Phần đồng bộ tốt**: [A] Quản lý state (Pinia), quản lý Routing và Theme đổi màu sáng/tối.
- **Phần lộn xộn nhất**: [A] Việc sử dụng song song Tailwind CSS, Element Plus, và custom CSS khiến giao diện ở các component nhỏ (button, input, icon) chưa đồng nhất. Tồn tại nhiều loại icon khác nhau (FontAwesome, Element, Lucide). File Component chi tiết tác vụ (TaskDetailModal) đang phình to mất kiểm soát.
- **Nên sửa trước**: [A] Gom nhóm lại các Dropdown, tách nhỏ `TaskDetailModal`, và thống nhất sử dụng 1 thư viện Icon.
- **Cảnh báo (Antigravity không được đụng vào nếu chưa có lệnh)**: Không được tự ý sửa đổi `router/index.js` và cấu trúc Pinia Store (`useWorkTaskStore.js`), vì đây là xương sống logic của ứng dụng, sửa sai có thể gây sập toàn bộ flow đăng nhập và hiển thị dự án.
