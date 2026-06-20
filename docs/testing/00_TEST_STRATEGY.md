# Test Strategy - SprintA

## 1. Mục tiêu

Tài liệu này định nghĩa cách kiểm thử thủ công cho ứng dụng quản lý công việc SprintA hiện tại:

- Frontend: Vue 3, Vue Router, Pinia, Element Plus.
- Backend: ASP.NET Core Web API, Entity Framework Core, SQL Server hoặc InMemory fallback ở Development.
- Realtime: SignalR tại `/kanban-hub` và `/notification-hub`.
- UI project clone dựa trên bộ screenshot trong `docs/jira-clone`.

Mục tiêu chính:

1. Bảo vệ các luồng đăng nhập, dự án, công việc và quản trị.
2. Phát hiện sai quyền giữa system role và project role.
3. Kiểm tra UI project bám screenshot mà không bổ sung workflow không có bằng chứng.
4. Bảo đảm chuyển ngôn ngữ không làm thay đổi dữ liệu hoặc identifier được bảo vệ.
5. Tạo bộ regression có thể chạy trước mỗi lần release.

## 2. Nguồn sự thật

Thứ tự ưu tiên khi xác định kết quả mong đợi:

1. Screenshot target được chỉ định và trạng thái trong `01_SCREENSHOT_INVENTORY.md`.
2. Yêu cầu đã được người dùng khóa cho từng màn.
3. Source frontend/backend hiện tại.

Không dùng kiến thức Jira bên ngoài để bổ sung expected result. Ảnh `MKT-*` không được dùng để suy ra project app. Ảnh `usable_annotated` không bao gồm nét khoanh hoặc chú thích tay trong tiêu chí visual.

## 3. Phạm vi

### 3.1 Frontend/manual

- Auth: login, register/OTP, 2FA, social callback, invite.
- Dashboard và global pages.
- Profile, email, password, session, avatar, 2FA, language.
- Global shell: sidebar, topbar, menu, search, notification.
- Space/Project: danh sách, tạo, favorite, archive, restore, trash, settings.
- Work items: tạo, sửa, status, assignee, comment, label, subtask, dependency, subscription, time log.
- Project tabs: Backlog, Board, Summary, List, Development, Forms, Timeline, Docs, Reports, Calendar.
- Cycles, Modules, Pages, Intakes, Views, Drafts, Rewards, Analytics.
- Admin: user, role, department, organization, configuration, audit, security.

### 3.2 Backend/API

- Authentication, token refresh/logout, OTP, 2FA.
- Users/Profile/Security.
- Projects, members, settings, execution rules.
- WorkTasks, comments, labels, dependencies, subtasks.
- Sprints, modules, pages, views, intakes.
- Notifications, drafts, stickies, gamification, audit.
- Admin users/roles/departments/settings/system metrics.
- Authorization filters, IP whitelist, CORS, rate limiting và validation.

### 3.3 Ngoài phạm vi

- Viết unit/integration/E2E automation.
- Benchmark tải lớn hoặc penetration test chuyên sâu.
- Kiểm thử Jira thật.
- Workflow chưa tồn tại trong source hoặc chưa được screenshot chứng minh.

## 4. Loại kiểm thử

| Loại | Mục đích | Tài liệu |
|---|---|---|
| Smoke manual | Xác nhận build/deploy có thể dùng ở mức cơ bản | `01_SMOKE_TEST_CHECKLIST.md` |
| Functional frontend | Kiểm tra route, state, form, data và project tabs | `02_FRONTEND_TEST_CASES.md` |
| Backend/API | Kiểm tra contract, status code, auth và persistence | `03_BACKEND_API_TEST_CASES.md` |
| Visual QA | So sánh layout với screenshot target | `04_JIRA_SCREENSHOT_VISUAL_QA.md` |
| i18n | Kiểm tra `vi`/`en`, persistence và protected terms | `05_I18N_TEST_CASES.md` |
| Permission | Kiểm tra system role/project role và defense in depth | `06_ADMIN_PERMISSION_TEST_CASES.md` |
| Release regression | Gate trước khi phát hành | `08_RELEASE_REGRESSION_CHECKLIST.md` |

## 5. Môi trường

### 5.1 Tối thiểu

- Trình duyệt Chromium desktop mới.
- Viewport visual chuẩn theo kích thước screenshot target; mặc định project dark dùng khoảng `1900x900`.
- Backend chạy đúng environment cần test.
- Database riêng cho QA; không dùng dữ liệu production.
- Có thể kiểm tra Network, Console và local/session storage trong DevTools.

### 5.2 Biến thể cần chạy

- Database SQL Server khả dụng.
- Development fallback khi SQL Server không khả dụng, nếu nhánh release hỗ trợ.
- Tài khoản mới, tài khoản thường, project manager và system admin.
- Ngôn ngữ mặc định `vi` và ngôn ngữ `en`.
- Phiên có token hợp lệ, hết hạn, thiếu token và token của tài khoản bị khóa.

## 6. Dữ liệu kiểm thử

Tạo bộ dữ liệu có thể reset:

| Mã | Dữ liệu |
|---|---|
| `USR-NORMAL` | User active, không có system admin role |
| `USR-ADMIN` | User active có một role thuộc nhóm system admin |
| `USR-SUSPENDED` | User không active hoặc bị suspend |
| `PRJ-MANAGER` | Thành viên project với role PM/PO/SM/Project Lead/Admin phù hợp |
| `PRJ-MEMBER` | Thành viên project thường |
| `PRJ-GUEST` | Guest hoặc Stakeholder |
| `PRJ-OUTSIDER` | User không phải active member |
| `PROJECT-A` | Project active có status, members và work items |
| `PROJECT-B` | Project user không được truy cập |
| `TASK-SET` | Work item có đủ status, assignee, due date, subtask và comment |

Issue key, project name, user name và task title là dữ liệu nguyên bản; không dịch hoặc tự chuẩn hóa khi test i18n.

## 7. Mức ưu tiên

| Mức | Ý nghĩa |
|---|---|
| `P0` | Blocker release: auth, mất dữ liệu, vượt quyền, app không mở |
| `P1` | Luồng cốt lõi hoặc sai visual lớn ở target chính |
| `P2` | Chức năng phụ, edge case hoặc sai visual trung bình |
| `P3` | Cosmetic nhỏ, không chặn nghiệp vụ |

## 8. Trạng thái test

- `PASS`: Kết quả thực tế khớp expected.
- `FAIL`: Có sai khác có thể tái hiện.
- `BLOCKED`: Thiếu môi trường, dữ liệu hoặc dependency.
- `NOT RUN`: Chưa thực hiện.
- `N/A`: Không áp dụng cho build hiện tại và có lý do rõ ràng.

## 9. Tiêu chí vào và ra

### Entry criteria

- Build/deploy hoàn tất hoặc môi trường local khởi động được.
- Database và seed data sẵn sàng.
- Có tài khoản cho các role cần test.
- Screenshot target và viewport được xác định cho visual QA.

### Exit criteria

- 100% case `P0` và `P1` đã chạy.
- Không còn bug mở mức Blocker/Critical.
- Bug High còn mở phải có quyết định chấp nhận rủi ro.
- Smoke, auth, permission và release regression đều pass.
- Visual deviation ngoài scope phải được ghi `[CẦN XÁC NHẬN]`, không tự duyệt.

## 10. Quy tắc thực thi

1. Ghi build/commit, môi trường, browser, database và tài khoản role trước khi chạy.
2. Với API write, chụp request/response và xác minh lại bằng GET hoặc database.
3. Với permission, kiểm cả frontend guard và gọi API trực tiếp.
4. Với visual, chụp cùng viewport, theme và state với target.
5. Với mock/static tab, chỉ kiểm hành vi source có triển khai; nút visual không được kỳ vọng gọi API.
6. Không dùng dữ liệu nhạy cảm thật trong bug report.
7. Sau mỗi case làm thay đổi dữ liệu, dọn hoặc reset dữ liệu trước case phụ thuộc tiếp theo.

## 11. Bằng chứng

Mỗi lần chạy nên lưu:

- Test run ID, ngày giờ, người test, commit/build.
- Kết quả từng case.
- Screenshot/video với lỗi UI.
- Request, response, status code và correlation/timestamp với lỗi API.
- Console log và Network log đã loại token/password/OTP.
- Bug ID liên kết với case fail.

## 12. Traceability tối thiểu

| Khu vực | Manual | API | Visual | Permission |
|---|---|---|---|---|
| Auth/Profile | Có | Có | Khi có target | Có |
| Global shell/i18n | Có | Hỗ trợ | Có | Có |
| Project/Work items | Có | Có | Có | Có |
| Project tabs | Có | Contract hiện có | Có | Có |
| Admin/Security | Có | Có | Không bắt buộc | Có |
