# PHÂN CÔNG SỬA BUG KHÔNG XUNG ĐỘT (Danh - Tú - Khôi)

Ngày lập: 2026-04-22  
Người lập: Cường + Codex  
Mục tiêu: Chia việc cho 3 thành viên sửa song song, tránh đụng vào file/logic đang được sửa trong máy local hiện tại.

## 1) Vùng cấm sửa (LOCKED - KHÔNG ĐƯỢC ĐỤNG VÀO)

Lý do: Các file này đang ở trạng thái `M` trong local, nếu sửa tiếp sẽ dễ xung đột và dễ ghi đè logic đang làm.

- Backend/src/TaskManagement.API/Controllers/AdminUsersController.cs
- Backend/src/TaskManagement.API/Controllers/AuditLogsController.cs
- Backend/src/TaskManagement.API/Controllers/CommentsController.cs
- Backend/src/TaskManagement.API/Controllers/GamificationController.cs
- Backend/src/TaskManagement.API/Controllers/LabelsController.cs
- Backend/src/TaskManagement.API/Controllers/ModulesController.cs
- Backend/src/TaskManagement.API/Controllers/ProjectMembersController.cs
- Backend/src/TaskManagement.API/Controllers/ProjectViewsController.cs
- Backend/src/TaskManagement.API/Controllers/ProjectsController.cs
- Backend/src/TaskManagement.API/Controllers/SecurityController.cs
- Backend/src/TaskManagement.API/Controllers/SprintsController.cs
- Backend/src/TaskManagement.API/Controllers/StickiesController.cs
- Backend/src/TaskManagement.API/Controllers/SystemSettingsController.cs
- Backend/src/TaskManagement.API/Controllers/TaskDependenciesController.cs
- Backend/src/TaskManagement.API/Controllers/WorkTasksController.cs
- Backend/src/TaskManagement.API/Filters/ProjectAuthorizeAttribute.cs
- Backend/src/TaskManagement.Application/DTOs/Sprint/SprintResponseDto.cs
- Backend/src/TaskManagement.Application/Interfaces/ISprintService.cs
- Backend/src/TaskManagement.Infrastructure/Data/ApplicationDbContext.cs
- Backend/src/TaskManagement.Infrastructure/Data/DataSeeder.cs
- Backend/src/TaskManagement.Infrastructure/Services/AuthService.cs
- Backend/src/TaskManagement.Infrastructure/Services/SprintService.cs
- Frontend/src/api/adminUserApi.js
- Frontend/src/components/CyclesTab.vue
- Frontend/src/components/ListView.vue
- Frontend/src/components/SettingsDropdown.vue
- Frontend/src/components/SpreadsheetTab.vue
- Frontend/src/components/TaskDetailModal.vue
- Frontend/src/components/TimelineTab.vue
- Frontend/src/components/UserDropdown.vue
- Frontend/src/components/layout/AdminSidebar.vue
- Frontend/src/components/layout/NexusSidebar.vue
- Frontend/src/router/adminRoutes.js
- Frontend/src/router/index.js
- Frontend/src/router/spaceRoutes.js
- Frontend/src/store/useAdminUserStore.js
- Frontend/src/store/useProjectStore.js
- Frontend/src/store/useWorkTaskStore.js
- Frontend/src/views/AuditLog.vue
- Frontend/src/views/Dashboard.vue
- Frontend/src/views/ManageSpaces.vue
- Frontend/src/views/RewardsView.vue
- Frontend/src/views/SpaceSummary.vue
- Frontend/src/views/UserManagement.vue
- Frontend/src/views/YourWorkView.vue
- Frontend/src/views/admin/AuditLog.vue
- Frontend/src/views/admin/Configuration.vue
- Frontend/src/views/admin/UserManagement.vue

## 2) Nguyên tắc chia việc để không xung đột

- Mỗi người tạo 1 nhánh riêng từ local hiện tại: `bugfix/danh-*`, `bugfix/tu-*`, `bugfix/khoi-*`.
- Mỗi người chỉ sửa đúng file trong "phạm vi được sửa" của mình.
- Không sửa file trong mục LOCKED ở Mục 1.
- Nếu gặp bug bắt buộc phải đụng file LOCKED: ghi vào backlog `OWNER-HANDOFF`, không tự ý sửa.

## 3) Phân công chi tiết theo từng thành viên

## 3.1 Danh - Tài khoản, OTP, tìm kiếm header, giao diện sáng/tối và hồ sơ

Module chính:
- Auth/Invite OTP flow
- Hồ sơ người dùng (upload avatar/cover)
- Tìm kiếm toàn cục ở topbar
- Chuyển giao diện sáng/tối

Bug ưu tiên:
- Invite OTP: gửi lại email/OTP bị treo, luồng accept invite bị lỗi OTP.
- Chuyển tài khoản không hoạt động.
- Thanh tìm kiếm lớn ở giữa ứng dụng không tìm được.
- Chuyển dark/light không hoạt động.
- Hồ sơ không đổi được avatar/cover.

Danh được sửa các file sau:
- Frontend/src/views/AcceptInvite.vue
- Frontend/src/views/Profile.vue
- Frontend/src/components/layout/NexusTopbar.vue
- Frontend/src/components/layout/NexusLayout.vue
- Frontend/src/utils/theme.js
- Frontend/src/api/axiosClient.js
- Backend/src/TaskManagement.API/Controllers/AuthController.cs
- Backend/src/TaskManagement.API/Controllers/UsersController.cs
- Backend/src/TaskManagement.Infrastructure/Services/EmailService.cs
- Backend/src/TaskManagement.Infrastructure/Services/OtpService.cs

Danh KHÔNG được sửa:
- Toàn bộ file trong Mục 1 (LOCKED), đặc biệt: `AdminUsersController.cs`, `useAdminUserStore.js`, `views/admin/UserManagement.vue`, `components/UserDropdown.vue`.

## 3.2 Tú - Pages, Stickies, Calendar, popup UI

Module chính:
- Pages (wiki editor + table + search)
- Sticky Notes

Bug ưu tiên:
- Pages không tạo được table, icon toolbar bị liệt, search không chạy.
- Stickies giới hạn tối đa 100 sticky.
- Sticky không format bold/italic theo đoạn text.
- Popup create cycle bị khuất màn hình.

Tú được sửa các file sau:
- Frontend/src/components/PagesTab.vue
- Frontend/src/views/PagesView.vue
- Frontend/src/views/StickiesView.vue
- Frontend/src/views/CyclesView.vue
- Frontend/src/style.css

Tú KHÔNG được sửa:
- Toàn bộ file trong Mục 1 (LOCKED), đặc biệt: `components/CyclesTab.vue`, `views/SpaceSummary.vue`, `controllers/StickiesController.cs`.

## 3.3 Khôi - Work item flow, Modules, project routing logic không đụng vùng LOCKED

Module chính:
- Work item UX ở các màn hình khác với file LOCKED
- Quản lý module
- Kiểm tra route/project context để tránh trùng work item giữa 2 project

Bug ưu tiên:
- Nếu chưa tạo project mà bấm New Work Item thì điều hướng sang tạo project + thông báo.
- Module chưa xong 100% vẫn complete được.
- Chưa set được người phụ trách module.
- 2 project bị trùng work item (ưu tiên validate project context trong các luồng được phép).

Khôi được sửa các file sau:
- Frontend/src/components/KanbanBoard.vue
- Frontend/src/views/ModulesView.vue
- Frontend/src/views/DraftsView.vue
- Frontend/src/views/Home.vue
- Frontend/src/views/ProjectSettings.vue
- Backend/src/TaskManagement.Infrastructure/Services/WorkTaskService.cs
- Backend/src/TaskManagement.Infrastructure/Services/ProjectService.cs
- Backend/src/TaskManagement.Application/DTOs/WorkTask/CreateWorkTaskDto.cs
- Backend/src/TaskManagement.Application/DTOs/WorkTask/WorkTaskResponseDto.cs

Khôi KHÔNG được sửa:
- Toàn bộ file trong Mục 1 (LOCKED), đặc biệt: `views/SpaceSummary.vue`, `store/useWorkTaskStore.js`, `controllers/WorkTasksController.cs`, `controllers/ProjectsController.cs`.

## 4) Các bug tạm thời chưa giao vì đang đụng file LOCKED

- Dashboard/project switch bị lỗi trực tiếp liên quan `NexusSidebar.vue`, `useProjectStore.js`, `Dashboard.vue` (đang LOCKED).
- Backlog trống/chức năng filter-display trong work items liên quan `useWorkTaskStore.js`, `ListView.vue`, `SpaceSummary.vue` (đang LOCKED).
- Audit log filter date range và sai timestamp liên quan `AuditLogsController.cs`, `views/admin/AuditLog.vue` (đang LOCKED).
- Admin user layout bị che nút liên quan `views/admin/UserManagement.vue` (đang LOCKED).

Trạng thái các mục này:
- Đánh dấu backlog `OWNER-HANDOFF` để Cường/Codex xử lý sau khi xong nhóm file hiện tại.

## 5) Cách làm việc Git để tránh xung đột

- Bước 1: Mỗi người checkout branch riêng.
- Bước 2: Chỉ add/commit file đúng scope đã giao.
- Bước 3: Trước khi push, chạy `git diff --name-only` và tự kiểm tra không có file LOCKED.
- Bước 4: Nếu có file ngoài scope, tách commit hoặc reset file đó trước khi push.

## 6) Mẫu message commit để dễ review

- Danh: `fix(auth-profile): stabilize invite otp + profile uploads + topbar search/theme`
- Tú: `fix(pages-stickies-calendar): restore editor actions + sticky cap + calendar visibility`
- Khôi: `fix(workflow-modules): enforce module completion rules and project task isolation`
