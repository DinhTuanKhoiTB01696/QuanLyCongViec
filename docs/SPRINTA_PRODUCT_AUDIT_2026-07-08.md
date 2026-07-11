# SprintA Product Audit & Roadmap

Ngày phân tích: 2026-07-08  
Repo chính: `DinhTuanKhoiTB01696/QuanLyCongViec`, local `origin/main` tại `c88bcaba7e19bbcd3d972a946894723a11ae1683`  
Phạm vi: chỉ phân tích, không sửa code ứng dụng.

## 1. Tóm Tắt Điều Hành

SprintA hiện không còn là một prototype nhỏ. Codebase đã có nền tảng quản lý công việc tương đối rộng: workspace/site, project, work item, sprint/cycle, team/department, goals, people, notification, audit log, starred/recent, reports, integrations, AI, admin/security. Backend có nhiều entity/controller thật và frontend đã gọi API thật ở phần lớn module quan trọng.

Tuy nhiên sản phẩm đang ở trạng thái "nhiều mảnh tốt nhưng chưa thành trải nghiệm lõi thật ổn định". Rủi ro lớn nhất khi demo là:

- Một số màn hình vẫn dùng demo/fallback/hard-code để nhìn có dữ liệu, ví dụ `RewardsView.vue`, chart mockup ở cycles/summary, cover `picsum.photos`.
- Workspace/project context còn fallback bằng localStorage hoặc GUID rỗng `00000000-0000-0000-0000-000000000000`, dễ gây lỗi lẫn dữ liệu hoặc API call sai workspace.
- Một số tính năng có backend nhưng UI chưa thành flow rõ, ví dụ intake/form request, custom workflow, reports nâng cao.
- UI có nhiều hướng thiết kế khác nhau: Plane/Jira-like, HomeSite, Nexus, Element Plus admin, card dashboard. Cần chuẩn hóa lại một ngôn ngữ SaaS sạch, hiện đại, phù hợp doanh nghiệp Việt.
- Nhiều chuỗi tiếng Việt trong console hiển thị mojibake do encoding khi đọc bằng PowerShell. Cần kiểm tra thực tế trong browser để xác định lỗi nằm ở file/source encoding hay chỉ là terminal decode.

Kết luận sản phẩm: SprintA có thể demo tốt nếu Phase 1 tập trung vào ổn định lõi, bỏ fallback nguy hiểm, đóng các flow chính từ workspace -> project -> task -> báo cáo -> notification. Không nên cố "clone Jira". Nên định vị SprintA là công cụ quản lý tiến độ cho SME Việt Nam, nhẹ hơn Jira, ít thuật ngữ agile hơn, mạnh ở nhắc việc, báo cáo dễ hiểu, intake từ email/Zalo/form và AI tóm tắt.

## 2. Nguồn Đã Đọc

### Quy tắc repo

- `AGENTS.md`: không thêm mock data, không hard-code để làm đẹp, không thay API contract, không sửa backend khi task UI, phải giữ chức năng/API thật.
- Worktree ban đầu đã có file modified: `Backend/src/TaskManagement.API/TaskManagement.API.csproj`, `Frontend/auto-imports.d.ts`, `Frontend/components.d.ts`. Báo cáo này không đụng vào các file đó.

### Frontend

- Router: `Frontend/src/router/index.js`, `homeRoutes.js`, `siteRoutes.js`, `spaceRoutes.js`, `dashboardRoutes.js`, `adminRoutes.js`.
- API client: `Frontend/src/api/axiosClient.js`, `uploadApi.js`, `signalrService.js`, `adminUserApi.js`.
- Store: `Frontend/src/store/useSiteStore.js`, `useProjectStore.js`, `useHomeProjectStore.js`, `useWorkTaskStore.js`, `useSprintStore.js`, `useGoalStore.js`, `useTeamStore.js`, `usePeopleStore.js`, `useStarredStore.js`, `useAdminUserStore.js`, `useActivityStore.js`, `useFollowerStore.js`.
- Views/components chính: `SpaceSummary.vue`, `TaskDetailModal.vue`, `BacklogTab.vue`, `TimelineTab.vue`, `CalendarTab.vue`, `CyclesTab.vue`, `ReportsView.vue`, `GlobalAnalyticsView.vue`, `IntegrationHubView.vue`, `FormsTab.vue`, `IntakeInbox.vue`, `ModulesTab.vue`, `ViewsTab.vue`, `PagesTab.vue`, `SpaceMembers.vue`, HomeSite Projects/Goals/Teams/People/Tools.

### Backend

- Startup: `Backend/src/TaskManagement.API/Program.cs`.
- DbContext: `Backend/src/TaskManagement.Infrastructure/Data/ApplicationDbContext.cs`.
- Seeder: `Backend/src/TaskManagement.Infrastructure/Data/DataSeeder.cs`.
- Controllers: `WorkTasksController`, `ProjectsController`, `SprintsController`, `DepartmentsController`, `UsersController`, `NotificationsController`, `AuditLogsController`, `SiteAuditLogsController`, `StarredItemsController`, `RecentViewsController`, `ModulesController`, `PagesController`, `IntakesController`, `IntegrationsController`, `InboxController`, `AiController`, `AdminUsersController`, `SecurityController`, `SystemSettingsController`, `ProjectViewsController`.
- Entities: `Workspace`, `WorkspaceMember`, `Project`, `ProjectMember`, `WorkTask`, `Sprint`, `TaskStatus`, `TaskType`, `TaskAssignment`, `TaskDependency`, `Goal`, `Department`, `Notification`, `AuditLog`, `SiteAuditLog`, `StarredItem`, `RecentView`, `Module`, `ProjectView`, `Intake`, `InboxItem`, `IntegrationAccount`, `Page`, `StickyNote`, `TaskDraft`, `Permission`, `Role`, `SystemSetting`, AI entities.
- Migration chính: `20260702034844_PlaneRenovation`.

### Repo tham khảo

- `addyosmani/agent-skills`: dùng cách chia phase, task nhỏ theo vertical slice, acceptance criteria, verification gate, definition of done.
- `Leonxlnx/taste-skill`: dùng tinh thần audit-first, typography/spacing/hierarchy, chống UI generic, không copy code/assets.

## 3. Kiến Trúc Hiện Tại

### Frontend stack

- Vue 3, Vite, Element Plus, Tailwind, Pinia, Chart.js/vue-chartjs, ApexCharts/ECharts, SignalR, TipTap, vuedraggable, lucide-vue-next.
- App có nhiều layout: `NexusLayout`, `HomeSiteLayout`, `AdminLayout`, `ProjectLayoutWrapper`.
- Router chia ba không gian: public/auth, dashboard/global, space/project, home site, admin.
- i18n có `vi` và `en`, mặc định tiếng Việt.

### Backend stack

- ASP.NET Core API, EF Core SQL Server, SignalR, JWT/auth, Google/GitHub login, rate limiter, static uploads.
- Nếu thiếu connection string thì fallback InMemory. Đây là tiện cho dev nhưng nguy hiểm nếu demo/business chạy nhầm môi trường.
- Startup tự migrate và có schema guard SQL thủ công. Cần ổn định migration trước khi demo chính thức.
- Seeder chỉ seed role mặc định nếu không bật `SPRINTA_SEED_DEMO_DATA=true`; demo data là opt-in. Đây là hướng đúng, nhưng vẫn cần audit script seed/demo rải rác.

## 4. Bảng Module: Có Rồi / Thiếu / Cần Cải Thiện

| Module | UI hiện có | API thật | Mock/fallback nguy hiểm | CRUD/flow thiếu | Demo được chưa | Nhận xét UX |
|---|---:|---:|---|---|---|---|
| Workspace/Site | Có: `/sites`, `/site-selection`, `/home`, create site | Có: `WorkspacesController` | localStorage recent site, fallback site ở store khác | Thiếu quản trị workspace rõ: đổi owner, invite, archive/restore UI đầy đủ | Có điều kiện | Thuật ngữ `site`, `space`, `workspace`, `project` đang lẫn, người Việt dễ rối |
| Team/Department | Có: HomeSite Teams, `SpaceMembers` link team | Có: `DepartmentsController`, `KudosController` | `coverImage` Unsplash fallback, text fallback | Có CRUD department/member/hierarchy nhưng flow quản trị còn phân tán | Có | Nên gọi "Phòng ban/Nhóm" thống nhất, giảm thuật ngữ team/department |
| Project | Có: Home projects, project detail, settings, create modal | Có: `ProjectsController`, `ProjectManagementController` | `picsum.photos` cover options, local project context | CRUD khá đủ, nhưng archive/delete/restore/permanent cần UX an toàn hơn | Có | Detail có nhiều tab nhưng cần ưu tiên "Tiến độ, việc cần làm, rủi ro" cho SME |
| Work items/Tasks | Có: list/board/detail/backlog/timeline/calendar | Có: `WorkTasksController`, comments, labels, dependencies, subtasks, time logs | recent localStorage, optimistic temp star id, status normalization cứng | Thiếu import/export Excel, bulk edit hoàn chỉnh, custom field thật | Có, module mạnh nhất | Cần Việt hóa thuật ngữ issue/work item/task thành "Công việc" |
| Goals | Có: Home goals, goal detail | Có: `GoalsController`, updates/risks/lessons/decisions | workspace fallback GUID rỗng trong `useGoalStore` | Thiếu liên kết goals -> project/task trực quan, update cadence | Có điều kiện | OKR có thể khó với SME, nên đổi sang "Mục tiêu/Kết quả" đơn giản |
| People | Có: directory, profile detail, profile cards | Có: `UsersController`, admin users | department/team fallback `N/A` | Thiếu onboarding nhân sự dễ hiểu, invite flow rõ cho admin thường | Có | Nên hiển thị vai trò/phòng ban/dự án đang làm rõ hơn |
| Notifications | Có dropdown + page | Có: `NotificationsController`, `NotificationHub` | Event trigger nội bộ bằng HTTP trong `WorkTasksController` dễ lỗi im lặng | Thiếu rule reminder theo deadline, digest, escalation | Có | Nên tập trung nhắc việc bằng tiếng Việt, ít noise |
| Audit log | Có admin audit, task audit, site audit | Có: `AuditLogsController`, `SiteAuditLogsController`, SaveChanges audit | Audit failures bị swallow, chỉ task/comment audit rõ | Thiếu audit cho mọi entity trọng yếu và filter dễ hiểu | Có điều kiện | Cần ngôn ngữ "Ai đã đổi gì, lúc nào, từ đâu sang đâu" |
| Starred/Recent | Có dropdown/page/list | Có: `StarredItemsController`, `RecentViewsController` | Task recent localStorage song song DB recent | Thiếu dọn/reorder/group favorite | Có | Nên là "Ghim" và "Gần đây" rõ, không chỉ starred |
| Reports/Analytics | Có project reports, global analytics | Có: dashboard stats, planning summary, baseline | Một số chart UI tính từ frontend, có thể thiếu data khi trống | Thiếu dashboard tùy biến, export report, report theo phòng ban | Có điều kiện | Nên ưu tiên báo cáo "dễ hiểu cho sếp", không thuật ngữ Jira |
| Sprints/Cycles | Có Cycles tab/view, burndown/carry-over | Có: `SprintsController` | Chart mockup trong `CyclesTab`; default 14 ngày fallback | Thiếu backlog planning drag/drop hoàn thiện, sprint goal, retrospective | Có điều kiện | SME Việt có thể không dùng sprint, nên gọi "Chu kỳ" hoặc "Đợt làm việc" |
| Modules | Có ModulesTab/ModuleList | Có: `ModulesController`, `IssueModule` | Ít rủi ro lớn | Thiếu module progress, module owner workload sâu | Có | Hợp với dự án phần mềm, nhưng SME cần giải thích "Hạng mục" |
| Views | Có ViewsTab/global views | Có: `ProjectViewsController`, `tasks/search` | Filter/display control còn disabled ở vài nơi | Thiếu saved filters mạnh, sharing, custom columns | Có điều kiện | Nên là "Bộ lọc đã lưu" hơn là "Views" |
| Permissions/Admin | Có admin users/roles/security/settings | Có: roles, departments, project roles, security | System role string nhiều dạng, override role cứng | Thiếu permission matrix UI dễ hiểu, workspace role model thống nhất | Có điều kiện | Cần 3 cấp đơn giản: Chủ workspace, Quản lý dự án, Thành viên |
| Integrations | Có IntegrationHub + Inbox | Có: Google Calendar/Gmail/Slack OAuth/sync endpoints, Inbox | UI nói real data, nhưng provider có coming soon / backend not enabled | Thiếu Zalo, Google Sheets/Excel import, webhook, email forward | Có với cấu hình OAuth | Đây là hướng khác biệt mạnh cho SME Việt |
| Intake/Form request | Có `IntakeInbox`, `FormsTab` minh họa | Có: `IntakesController` | `FormsTab` là visual placeholder, contact admin disabled | Thiếu form builder/public request portal/routing | Chưa | Đây nên là tính năng khác Jira nhưng nhẹ hơn Jira Service Management |
| AI assistant | Có AI page, command, task detail AI, repo analysis | Có: `AiController`, Gemini service/entities | Cần kiểm tra quota/secrets/fallback | Thiếu guardrail, audit AI action, prompt template productized | Demo được nếu cấu hình AI | Nên AI tóm tắt tiếng Việt, tạo task từ mô tả/Zalo/email |
| Mobile/PWA | Chưa thấy PWA rõ | Không liên quan | N/A | Thiếu responsive/mobile-first audit, offline/PWA | Chưa | SME dùng điện thoại nhiều, đây là gap lớn |

## 5. Gap So Với Jira Ở Mức Sản Phẩm

| Năng lực Jira | SprintA hiện tại | Gap |
|---|---|---|
| Board/List/Timeline/Calendar | Có đủ hướng chính | Board/List/Timeline/Calendar chưa đồng nhất về filter, bulk action, saved views |
| Backlog/Sprint/Cycle | Có backlog và cycles | Backlog planning chưa mạnh, sprint goal/review/retrospective còn thiếu |
| Custom fields | Có một số field cố định | Thiếu custom field thật theo project/form/report |
| Custom workflow | Có task statuses CRUD | Thiếu workflow transitions, rule ai được kéo trạng thái nào, status category rõ |
| Automation/reminders | Có notifications events | Thiếu automation builder/reminder theo deadline, overdue escalation, recurring task |
| Reports/dashboard | Có reports/global analytics/planning summary | Thiếu dashboard tùy biến, gadget, export, executive summary tiếng Việt |
| Form intake/request | Có intake API/UI nhỏ | Chưa có request portal/form builder/public link/approval routing |
| Permissions | Có roles/admin/project role | Chưa có model quyền đơn giản, nhất quán, dễ audit |
| Audit log | Có task/system/site audit | Chưa phủ rộng mọi entity; UI chưa giải thích thay đổi thân thiện |
| Integrations | Có Google Calendar/Gmail/Slack mầm | Thiếu Zalo, Google Sheets/Excel, webhook, Drive, import/export enterprise |
| Import/export | Chưa thấy flow chính | Thiếu Excel import/export, CSV, report PDF |
| Mobile/PWA | Chưa rõ | Thiếu trải nghiệm mobile, PWA install, push notification |
| AI assistant | Có nhiều API AI | Chưa thành trợ lý theo workflow người Việt, thiếu kiểm soát hành động |
| Marketplace/apps | Chưa có | Có thể không cần trong SME phase đầu |
| Service management | Intake sơ khai | Thiếu SLA, ticket channel, requester view |

## 6. SprintA Nên Khác Jira Ở Đâu

1. Ít thuật ngữ agile hơn: dùng "Dự án", "Công việc", "Người phụ trách", "Hạn", "Tiến độ", "Rủi ro", "Nhắc việc" thay vì issue/sprint/epic/story point làm mặc định.
2. Màn hình quản lý cho sếp SME: một trang "Hôm nay cần chú ý gì?" gồm việc trễ hạn, người quá tải, quyết định cần duyệt, dự án đỏ/vàng/xanh.
3. Nhắc việc kiểu Việt Nam: deadline reminders, daily/weekly digest, nhắc qua email/Zalo/Slack, escalation nhẹ cho quản lý.
4. Import từ Excel và giảm phụ thuộc Zalo: tạo task từ file Excel, copy-paste bảng, email/calendar/inbox, sau này Zalo OA/webhook.
5. Báo cáo dễ hiểu: không cần velocity phức tạp trước. Cần "Đúng hạn bao nhiêu %, ai đang kẹt, dự án nào trễ, tuần này xong gì".
6. Form yêu cầu đơn giản: public/internal request form cho nhân viên gửi việc, quản lý duyệt thành task.
7. Permission đơn giản: owner, admin, manager, member, viewer. Tránh ma trận quyền quá kiểu enterprise ở phase đầu.
8. AI tiếng Việt thực dụng: tóm tắt cuộc họp/email thành task, viết update tiến độ, gợi ý chia nhỏ công việc, nhắc rủi ro.

## 7. Roadmap Ưu Tiên

### Phase 1: Sửa Lõi Để Demo Ổn Định

#### 1. Chuẩn hóa workspace/project context

- Mục tiêu: không còn fallback GUID rỗng/localStorage mơ hồ khi gọi API workspace/project.
- Frontend cần kiểm tra: `useSiteStore.js`, `useGoalStore.js`, `useHomeProjectStore.js`, `useWorkTaskStore.js`, `SpaceSummary.vue`, `ProjectLayoutWrapper.vue`, `NexusLayout.vue`.
- Backend/API cần kiểm tra: `WorkspacesController`, `ProjectsController`, `StarredItemsController`, `FollowersController`, auth current user.
- Database/entity cần kiểm tra: `Workspace`, `WorkspaceMember`, `Project.WorkspaceId`.
- UI/UX: luôn có selector workspace rõ, empty state "Bạn chưa chọn workspace", không tự nhảy sang project khác nếu 403.
- Luồng người dùng: đăng nhập -> chọn workspace -> chọn/tạo project -> mọi tab dùng đúng context.
- Acceptance criteria:
  - Không API call nào dùng `00000000-0000-0000-0000-000000000000`.
  - Refresh browser vẫn vào đúng workspace/project hợp lệ.
  - User không có quyền thấy 403 friendly, không bị redirect vòng.
- Rủi ro kỹ thuật: nhiều component đang đọc `localStorage` trực tiếp, cần thay dần bằng store/router source of truth.

#### 2. Gỡ hoặc khóa toàn bộ mock/demo fallback trên UI demo thật

- Mục tiêu: mọi số liệu/row/card trong demo business đến từ API hoặc hiện empty state thật.
- Frontend cần kiểm tra: `RewardsView.vue`, `CyclesTab.vue`, `SpaceSummary.vue`, `CreateProjectModal.vue`, `CreateSpaceModal.vue`, `FormsTab.vue`.
- Backend/API cần kiểm tra: `GamificationController`, `SprintsController`, `WorkTasksController`, `IntakesController`.
- Database/entity cần kiểm tra: `UserWallet`, `PointTransaction`, `Sprint`, `WorkTask`, `Intake`.
- UI/UX: skeleton/loading/empty/error đẹp, không "bịa dữ liệu".
- Luồng người dùng: nếu chưa có dữ liệu, app hướng dẫn tạo dữ liệu thật, ví dụ tạo công việc hoặc bật tính điểm.
- Acceptance criteria:
  - Search `demo`, `mock`, `picsum`, `chart-mockup` không còn nằm trên đường demo chính, hoặc chỉ ở story/dev flag rõ.
  - Không hiển thị transaction/achievement/task giả khi API trả rỗng.
  - Empty state có CTA tạo dữ liệu thật.
- Rủi ro kỹ thuật: nếu bỏ fallback ngay, vài màn sẽ trống. Cần làm empty state tốt trước.

#### 3. Đóng flow công việc end-to-end

- Mục tiêu: tạo project -> thêm thành viên -> tạo task -> giao người -> đổi trạng thái -> comment/file -> báo cáo cập nhật.
- Frontend cần kiểm tra: `CreateProjectModal.vue`, `SpaceMembers.vue`, `SpaceSummary.vue`, `TaskDetailModal.vue`, `ReportsView.vue`, `NotificationsDropdown.vue`.
- Backend/API cần kiểm tra: `ProjectsController`, `ProjectMembersController`, `WorkTasksController`, `CommentsController`, `UploadsController`, `NotificationsController`.
- Database/entity cần kiểm tra: `Project`, `ProjectMember`, `WorkTask`, `TaskAssignment`, `Comment`, `Attachment`, `Notification`.
- UI/UX: làm một happy path tiếng Việt rõ, nút primary nhất quán, lỗi inline dễ hiểu.
- Luồng người dùng: PM tạo dự án, mời thành viên, tạo việc, giao việc, thành viên cập nhật tiến độ, PM xem report.
- Acceptance criteria:
  - Demo được 10 phút không cần seed fake.
  - Task mới xuất hiện ở list/board/calendar/report.
  - Notification tạo khi assign/comment/status change.
- Rủi ro kỹ thuật: TaskDetailModal quá lớn, dễ regression khi sửa.

#### 4. Chuẩn hóa tiếng Việt và thuật ngữ

- Mục tiêu: ngôn ngữ sản phẩm nhất quán cho người Việt.
- Frontend cần kiểm tra: `i18n/locales/vi.js`, các view hard-code text, `useI18n`.
- Backend/API cần kiểm tra: response message trong controllers.
- Database/entity cần kiểm tra: seed/default status/type.
- UI/UX: glossary: Workspace = Không gian làm việc, Project = Dự án, Work item/Task = Công việc, Cycle/Sprint = Chu kỳ, Module = Hạng mục, View = Bộ lọc/Lượt xem.
- Luồng người dùng: không gặp lẫn tiếng Anh/Việt trong cùng flow chính.
- Acceptance criteria:
  - Không còn "issue/work item/task" lẫn lộn trên màn chính.
  - Error message auth/permission/status đọc được bằng tiếng Việt.
  - Browser render tiếng Việt không mojibake.
- Rủi ro kỹ thuật: nhiều string hard-code ngoài i18n.

#### 5. Báo cáo demo cho quản lý

- Mục tiêu: một dashboard đủ thuyết phục giảng viên/doanh nghiệp.
- Frontend cần kiểm tra: `GlobalAnalyticsView.vue`, `ReportsView.vue`, `SpaceDashboard.vue`, `HomeSite/Tools/SystemStatus.vue`.
- Backend/API cần kiểm tra: `/dashboard/stats`, `/analytics/planning-summary`, `StatusDashboardController`.
- Database/entity cần kiểm tra: `WorkTask`, `TaskAssignment`, `TimeLog`, `Sprint`, `Project`.
- UI/UX: RAG status đỏ/vàng/xanh, việc trễ hạn, workload, tiến độ, biểu đồ đơn giản.
- Luồng người dùng: quản lý mở dashboard -> thấy dự án nào cần can thiệp -> click vào task/người phụ trách.
- Acceptance criteria:
  - Có số liệu thật từ project đang chọn.
  - Click từ report sang task/project hoạt động.
  - Empty state hướng dẫn tạo công việc/hạn/assignee.
- Rủi ro kỹ thuật: chart dễ gây hiểu sai nếu data thiếu due date/estimate.

### Phase 2: Tính Năng Khác Biệt Cho SME Việt Nam

#### 6. Form intake/request portal nhẹ

- Mục tiêu: thay Excel/Zalo bằng form gửi yêu cầu, quản lý duyệt thành task.
- Frontend cần kiểm tra: `FormsTab.vue`, `IntakeInbox.vue`, `SpaceSummary.vue`, `IntegrationHubView.vue`.
- Backend/API cần kiểm tra: `IntakesController`, `WorkTasksController`, auth/public access nếu có.
- Database/entity cần kiểm tra: `Intake`, `WorkTask`, `Project`, thêm cấu hình form nếu cần.
- UI/UX: form builder tối giản: tiêu đề, mô tả, ưu tiên, hạn mong muốn, người yêu cầu, file đính kèm.
- Luồng người dùng: nhân viên gửi yêu cầu -> quản lý thấy inbox -> duyệt/từ chối/trùng -> task được tạo.
- Acceptance criteria:
  - Có link form nội bộ hoặc public token an toàn.
  - Review intake tạo task và giữ trace về request gốc.
  - Requester nhận thông báo trạng thái.
- Rủi ro kỹ thuật: public form cần chống spam, permission và file upload an toàn.

#### 7. Import/export Excel

- Mục tiêu: giảm friction chuyển từ Excel sang SprintA.
- Frontend cần kiểm tra: `SpaceSummary.vue`, `ReportsView.vue`, project settings/import UI mới.
- Backend/API cần kiểm tra: endpoint import/export tasks/projects/reports.
- Database/entity cần kiểm tra: `WorkTask`, `TaskAssignment`, `Project`, `TaskStatus`, `Label`.
- UI/UX: wizard 3 bước: upload file -> map cột -> preview lỗi -> import.
- Luồng người dùng: PM upload Excel danh sách việc -> map "Tên việc/Hạn/Người phụ trách/Trạng thái" -> import thành task.
- Acceptance criteria:
  - Template Excel tải xuống được.
  - Import báo lỗi theo dòng, không fail toàn bộ nếu vài dòng sai.
  - Export report/task list mở được bằng Excel tiếng Việt.
- Rủi ro kỹ thuật: mapping người dùng/email, encoding tiếng Việt, duplicate detection.

#### 8. Reminder và daily digest

- Mục tiêu: nhắc việc tự động thay cho nhắn Zalo thủ công.
- Frontend cần kiểm tra: `NotificationsDropdown.vue`, `NotificationsView.vue`, `ProjectSettings.vue`, profile notification settings.
- Backend/API cần kiểm tra: `NotificationsController`, background worker mới, email service, SignalR.
- Database/entity cần kiểm tra: `Notification`, `SystemSetting`, có thể thêm `ReminderRule`.
- UI/UX: rule đơn giản: trước hạn 1 ngày, trễ hạn, không cập nhật 3 ngày, digest 8h sáng.
- Luồng người dùng: quản lý bật rule -> thành viên nhận thông báo -> click vào task.
- Acceptance criteria:
  - Không gửi trùng thông báo.
  - Có read/unread và link đúng task.
  - Có cấu hình tắt/bật theo user/project.
- Rủi ro kỹ thuật: scheduler, timezone Asia/Ho_Chi_Minh, spam notification.

#### 9. Executive dashboard tiếng Việt

- Mục tiêu: sếp không cần hiểu Jira vẫn đọc được tình hình.
- Frontend cần kiểm tra: `SpaceDashboard.vue`, `GlobalAnalyticsView.vue`, `HomeSite/Projects/ProjectList.vue`.
- Backend/API cần kiểm tra: analytics/status endpoints.
- Database/entity cần kiểm tra: `ProjectUpdate`, `ProjectRisk`, `Goal`, `WorkTask`.
- UI/UX: 4 câu hỏi chính: dự án nào trễ, ai đang quá tải, việc nào cần quyết định, tuần này xong gì.
- Luồng người dùng: owner mở dashboard mỗi sáng -> lọc theo phòng ban/dự án -> giao follow-up.
- Acceptance criteria:
  - Có RAG status tự động dựa trên deadline/progress/risk.
  - Có action "Nhắc người phụ trách" hoặc "Tạo cập nhật".
  - Có export PDF/Excel ở bản sau hoặc ít nhất copy summary.
- Rủi ro kỹ thuật: thuật toán health score phải minh bạch, tránh gây tranh cãi.

#### 10. Permission đơn giản cho SME

- Mục tiêu: giảm rối so với Jira, đủ an toàn.
- Frontend cần kiểm tra: `admin/UserManagement.vue`, `RoleManagement.vue`, `SpaceMembers.vue`, `ProjectSettings.vue`.
- Backend/API cần kiểm tra: `AdminUsersController`, `SecurityController`, `ProjectAuthorizeAttribute`, `RequirePermissionAttribute`.
- Database/entity cần kiểm tra: `Role`, `Permission`, `UserRole`, `ProjectMember`, `WorkspaceMember`.
- UI/UX: preset role + giải thích quyền bằng checklist tiếng Việt.
- Luồng người dùng: owner thêm nhân sự -> chọn vai trò -> hệ thống tự áp dụng quyền.
- Acceptance criteria:
  - Role preset không cần nhập code role thủ công.
  - Project member không thấy setting nếu không có quyền.
  - Audit log ghi đổi quyền.
- Rủi ro kỹ thuật: hiện role string có nhiều biến thể, cần normalize không phá data cũ.

### Phase 3: AI, Tích Hợp, Nâng Cấp Chuyên Nghiệp

#### 11. AI assistant tiếng Việt theo ngữ cảnh

- Mục tiêu: AI không chỉ chat, mà giúp tạo/cập nhật/nhắc việc an toàn.
- Frontend cần kiểm tra: `AIPage.vue`, `PriorityAIPanel.vue`, `TaskDetailModal.vue`, `NexusLayout.vue`.
- Backend/API cần kiểm tra: `AiController`, `GeminiAiService`, `AiIntegrationService`.
- Database/entity cần kiểm tra: `AIPromptTemplate`, `AITokenUsage`, `AIFeedback`, `AITrainingDataset`.
- UI/UX: AI action preview trước khi ghi DB, người dùng bấm xác nhận.
- Luồng người dùng: paste đoạn chat/email -> AI đề xuất task/subtask/deadline/assignee -> user duyệt.
- Acceptance criteria:
  - AI không tự sửa task nếu chưa confirm.
  - Có log token/feedback/action.
  - Prompt/output ưu tiên tiếng Việt có dấu.
- Rủi ro kỹ thuật: hallucination, quota, bảo mật dữ liệu công ty.

#### 12. Tích hợp Zalo/Email/Calendar nâng cao

- Mục tiêu: đưa tín hiệu công việc từ kênh thực tế SME vào SprintA.
- Frontend cần kiểm tra: `IntegrationHubView.vue`, `InboxController` UI, notification settings.
- Backend/API cần kiểm tra: `IntegrationsController`, `InboxController`, OAuth/webhook/service worker.
- Database/entity cần kiểm tra: `IntegrationAccount`, `InboxItem`, `SyncHistory`.
- UI/UX: unified inbox có phân loại "cuộc họp", "deadline", "yêu cầu", "tin nhắn cần xử lý".
- Luồng người dùng: sync/tích hợp -> inbox -> AI gợi ý task -> duyệt.
- Acceptance criteria:
  - Mỗi inbox item có source, external id, dedupe.
  - Sync lỗi có lịch sử và retry.
  - Tạo task từ inbox giữ link về nguồn.
- Rủi ro kỹ thuật: OAuth production, webhook reliability, chính sách Zalo API.

#### 13. Custom field và workflow nâng cao

- Mục tiêu: đủ linh hoạt khi SME lớn dần nhưng không biến thành Jira clone.
- Frontend cần kiểm tra: `ProjectSettings.vue`, `TaskDetailModal.vue`, `ViewsTab.vue`, `ReportsView.vue`.
- Backend/API cần kiểm tra: `WorkTasksController`, `ProjectViewsController`, settings endpoints.
- Database/entity cần kiểm tra: thêm entity `CustomField`, `CustomFieldValue`, `WorkflowTransition` nếu cần.
- UI/UX: chỉ cho admin tạo field đơn giản: text, number, date, select, user; workflow dạng "trạng thái và ai được chuyển".
- Luồng người dùng: admin tạo field "Chi phí dự kiến" -> task form hiển thị -> view/report filter được.
- Acceptance criteria:
  - Field xuất hiện trong create/edit/detail/list.
  - Có validate type.
  - Saved views filter theo custom field.
- Rủi ro kỹ thuật: schema JSON vs relational, report query phức tạp.

#### 14. Mobile/PWA

- Mục tiêu: thành viên xem/cập nhật việc trên điện thoại.
- Frontend cần kiểm tra: toàn bộ layout chính, đặc biệt `SpaceSummary.vue`, `TaskDetailModal.vue`, `NotificationsView.vue`, `YourWorkView.vue`.
- Backend/API cần kiểm tra: auth refresh, notification endpoints.
- Database/entity cần kiểm tra: không cần entity mới ban đầu.
- UI/UX: mobile focus vào "Việc của tôi", "Thông báo", "Cập nhật trạng thái", "Comment".
- Luồng người dùng: mở PWA -> xem việc hôm nay -> cập nhật tiến độ/comment/ảnh.
- Acceptance criteria:
  - PWA installable, manifest/icon/service worker.
  - Các flow chính dùng được dưới 390px width.
  - Task detail không tràn/chồng chữ.
- Rủi ro kỹ thuật: app hiện nhiều bảng/timeline desktop-heavy, cần mobile alternative chứ không chỉ responsive CSS.

## 8. Prompt Nhỏ Cho Từng Phase

### Prompt Phase 1

```text
Bạn là Senior Product Engineer của SprintA. Hãy đọc AGENTS.md và không sửa backend/API contract nếu không được yêu cầu. Nhiệm vụ Phase 1: ổn định demo lõi.

Mục tiêu:
1. Chuẩn hóa workspace/project context, loại bỏ fallback GUID rỗng/localStorage mơ hồ.
2. Gỡ hoặc khóa demo/mock fallback trên đường demo chính, thay bằng loading/empty/error state thật.
3. Đảm bảo flow tạo project -> thêm member -> tạo/giao task -> cập nhật trạng thái/comment -> report/notification chạy bằng API thật.
4. Chuẩn hóa thuật ngữ tiếng Việt cho màn chính.

Trước khi sửa hãy kiểm tra các file: useSiteStore, useGoalStore, useHomeProjectStore, useWorkTaskStore, SpaceSummary, TaskDetailModal, ReportsView, NotificationsDropdown.

Acceptance criteria:
- Không API call nào dùng workspaceId 00000000-0000-0000-0000-000000000000.
- Không hiển thị dữ liệu demo nếu API trả rỗng.
- Demo happy path chạy được sau refresh browser.
- npm run build pass.
```

### Prompt Phase 2

```text
Bạn là Product Engineer + UX Architect cho SprintA thị trường SME Việt Nam. Hãy xây vertical slice khác biệt, không clone Jira.

Mục tiêu Phase 2:
1. Form intake/request portal nhẹ: gửi yêu cầu -> duyệt -> tạo task.
2. Import/export Excel cho task/report.
3. Reminder/daily digest theo deadline và overdue.
4. Executive dashboard tiếng Việt cho quản lý.
5. Permission preset đơn giản: Owner/Admin/Manager/Member/Viewer.

Luôn thiết kế theo tiếng Việt dễ hiểu, ít thuật ngữ agile. Mỗi hạng mục phải có UI, API thật, empty/error state, acceptance criteria và test/build.
```

### Prompt Phase 3

```text
Bạn là Tech Lead cho SprintA Phase 3. Hãy nâng cấp AI/tích hợp/professional features mà vẫn giữ sản phẩm nhẹ hơn Jira.

Mục tiêu:
1. AI assistant tiếng Việt có action preview và confirm trước khi ghi DB.
2. Unified Inbox từ Calendar/Gmail/Slack/Zalo, dedupe bằng external id.
3. Custom fields/workflow vừa đủ cho SME.
4. Mobile/PWA cho Việc của tôi, Thông báo, Task detail, Comment.

Yêu cầu:
- Không để AI tự động sửa dữ liệu nếu user chưa xác nhận.
- Mọi tích hợp phải có sync history, retry/error state.
- Mobile phải kiểm tra viewport 390px và desktop.
- Có audit/security review cho dữ liệu nhạy cảm.
```

## 9. Ưu Tiên Ngay Sau Báo Cáo

1. Làm một smoke test thực tế với DB sạch và user thật, ghi lại lỗi theo flow demo 10 phút.
2. Chọn một thuật ngữ sản phẩm duy nhất cho `workspace/site/space` trước khi sửa UI rộng.
3. Gỡ fallback demo ở `RewardsView.vue` và chart mockup khỏi đường demo chính.
4. Sửa workspace context fallback trước mọi tính năng mới.
5. Đóng executive dashboard tối giản bằng data thật, vì đây là màn thuyết phục giảng viên/doanh nghiệp nhanh nhất.

