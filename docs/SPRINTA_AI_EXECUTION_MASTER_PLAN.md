# SPRINTA — MASTER EXECUTION PLAN FOR AI-ASSISTED DELIVERY

> **Mục đích:** Đây là tài liệu nguồn duy nhất để Khôi, Tú, Quân, Quân Đạt, Kiệt, Phát và các AI Agent như Codex, Antigravity, Claude Code hoặc Gemini hiểu đúng tình trạng SprintA, phạm vi từng người, thứ tự triển khai, tiêu chuẩn hoàn thành và giới hạn không được vượt qua.
>
> **Ngày lập kế hoạch:** 15/07/2026  
> **Repository:** https://github.com/DinhTuanKhoiTB01696/QuanLyCongViec.git  
> **Nhánh chuẩn khi lập kế hoạch:** `main`  
> **Baseline tham chiếu:** commit `f0dbe64cb523d676e8da26264533ca7fd0ab8766`
>
> **Nguyên tắc quan trọng nhất:** Phần nào đã hoạt động đúng thì không viết lại, không redesign, không đổi kiến trúc. Chỉ sửa khi có bằng chứng rằng chức năng đó lỗi, chưa nối frontend–backend, chỉ lưu localStorage, dùng mock/hard-code, hoặc không đạt tiêu chí nghiệm thu trong tài liệu này.

---

# 1. Bối cảnh dự án

SprintA là hệ thống quản lý công việc và dự án gồm:

- Frontend: Vue 3, Vite, Pinia, Vue Router, Element Plus, TipTap, SignalR, PWA.
- Backend: ASP.NET Core, Entity Framework Core, SQL Server, JWT, Gemini AI.
- Các module chính: Workspace/Space, Project, Work Item/Task, Kanban, Cycle/Sprint, Goal, Team, People, Report, Page/Docs, Notification, Integration, AI Copilot, Admin, Role và Permission.
- Quy tắc dữ liệu: dùng API và database thật; không dùng mock, seed, hard-code hoặc localStorage để giả lập chức năng nghiệp vụ.
- Mục tiêu hiện tại: ổn định chức năng cốt lõi, hoàn thiện các phần còn dang dở có giá trị cao, đồng bộ trải nghiệm và chuẩn bị bản demo thi.

---

# 2. Cơ cấu nhóm và trách nhiệm

| Thành viên | Vai trò | Trách nhiệm chính | Quyền quyết định |
|---|---|---|---|
| **Khôi** | Trưởng nhóm, PO, Full-stack, phụ trách AI | Chốt scope, kiến trúc, tích hợp, AI, review backend, merge, demo | Quyết định cuối cùng về scope, shared files, migration, API contract và merge |
| **Tú** | Dev 1, Full-stack | Auth/User Context, avatar, Site Selection, Task Contingency Plan | Chủ sở hữu workstream Identity + Contingency |
| **Quân** | Dev 2, Full-stack | Collaboration: Activity Feed, Daily Check-in; Chat nếu còn thời gian | Chủ sở hữu workstream Collaboration |
| **Quân Đạt** | BA, Frontend, Báo cáo, Tester | Performance audit, test plan, báo cáo, regression, bằng chứng demo | Chốt acceptance criteria cùng Khôi; không tự đổi backend contract |
| **Kiệt** | Frontend, Tester | i18n, Daily Focus widget, UX và regression frontend | Chủ sở hữu workstream i18n + Daily Focus |
| **Phát** | Frontend, Tester | Functional catalog, permission matrix, role UI QA, module inventory | Chủ sở hữu tài liệu chức năng/quyền; backend enforcement do Khôi review |

---

# 3. Luật bắt buộc cho mọi AI Agent

## 3.1. Quy tắc đọc source để tránh tốn token

Mỗi task chỉ được bắt đầu theo thứ tự:

1. Đọc phần tương ứng trong file này.
2. Đọc file được liệt kê trong `Allowed files`.
3. Tìm chính xác tên component, endpoint, entity hoặc method liên quan.
4. Chỉ mở thêm file khi có dependency trực tiếp.
5. Trước khi mở quá 8 file, AI phải giải thích ngắn gọn vì sao cần thêm.

Không được:

- Đọc toàn bộ repository.
- Quét `node_modules`, `dist`, `bin`, `obj`, `.git`, build artifacts.
- Đọc toàn bộ migrations khi task không sửa schema.
- Đọc toàn bộ locale khi chỉ sửa một module.
- Phân tích lại các module đã được đánh dấu `FROZEN`.
- Viết báo cáo dài sau mỗi thay đổi nhỏ.

## 3.2. Quy tắc sửa code

- Không dùng mock data, fake response, random data, `setTimeout` giả API hoặc hard-code để báo thành công.
- Không thêm fallback làm ứng dụng “trông như chạy” khi backend lỗi.
- Không tự đổi API contract đang hoạt động.
- Không sửa shared component nếu task có thể giải quyết tại module.
- Không thêm dependency mới nếu chưa chứng minh cần thiết.
- Không thay Vue/Element Plus bằng React/Tailwind.
- Không copy JSX hoặc React hook vào dự án Vue.
- Không redesign hàng loạt.
- Không làm đẹp trước khi chức năng hoạt động thật.
- Không tự tạo migration mới khi entity/schema hiện tại đã đáp ứng.
- Không sửa AI Action Preview/Confirm nếu smoke test chưa chứng minh có lỗi.
- Không sửa Goal Detail/Foundation đã ổn nếu không thuộc acceptance criteria.
- Không force push `main`.

## 3.3. Quy tắc kiểm thử

Trong lúc làm task:

- Chỉ chạy build/test trực tiếp liên quan.
- Frontend task: chạy `npm run build` sau khi hoàn tất task.
- Backend task: chạy test project liên quan hoặc `dotnet build` tại solution/backend.
- Task liên quan database: kiểm tra migration status và API thật.
- Task CRUD: bắt buộc test Create → Reload → Read → Update → Reload → Delete.
- Task UI: kiểm tra light/dark, 1366px desktop và 390px mobile nếu component có responsive.
- Không chạy full E2E liên tục; full regression chỉ chạy tại Integration Gate.

## 3.4. Định dạng báo cáo của AI

AI chỉ báo cáo:

```text
A. Task ID và trạng thái
B. File đã sửa
C. Chức năng thật đã hoàn thành
D. Test đã chạy và kết quả
E. Phần còn thiếu hoặc rủi ro
```

Không lặp lại toàn bộ yêu cầu. Không kể quá trình suy nghĩ. Không mở rộng scope.

---

# 4. Trạng thái hiện tại và vùng bị khóa

## 4.1. Ký hiệu

| Trạng thái | Ý nghĩa |
|---|---|
| `FROZEN` | Đã làm; không động tới trừ khi test chứng minh lỗi |
| `REAL` | Dùng API/database thật và hoạt động |
| `PARTIAL` | Có dữ liệu thật nhưng còn luồng thiếu |
| `FRONTEND_MISSING` | Backend có, frontend chưa có hoặc chưa nối |
| `LOCAL_ONLY` | Chỉ lưu trên trình duyệt |
| `MOCK` | Dữ liệu hoặc hành vi giả |
| `PLACEHOLDER` | Chỉ có UI giới thiệu, chưa có chức năng |
| `BACKLOG` | Để sau bản demo |

## 4.2. Ma trận trạng thái

| Khu vực | Trạng thái | Quyết định |
|---|---|---|
| Landing nhận biết đăng nhập, `/auth/context`, logout | `REAL / FROZEN` | Không viết lại |
| Auth login, 2FA email OTP cơ bản | `REAL / FROZEN` | Chỉ sửa khi test lỗi |
| Auth session utility | `REAL` | Được bổ sung tính reactive, không thay contract |
| Avatar toàn hệ thống | `PARTIAL` | Sửa nguồn dữ liệu và đồng bộ realtime |
| Site Selection | `PARTIAL` | Cần avatar thật, dropdown, logout, bố cục đúng |
| Work Items/Kanban/List/Calendar/Timeline cơ bản | `REAL / FROZEN` | Không redesign |
| Import/Export và AI Action Preview/Confirm | `FROZEN` | Chỉ smoke test |
| Goals/Goal Detail và Foundation components | `FROZEN` | Không redesign hàng loạt |
| Daily Focus | `PARTIAL` | Task thật; postpone localStorage; chưa vào Dashboard |
| Task Contingency Plan backend | `REAL / FROZEN` | Không sửa entity/controller/migration nếu API test pass |
| Task Contingency Plan frontend | `FRONTEND_MISSING` | Phải hoàn thiện |
| Notification core | `REAL` | Không viết lại |
| Notification preference theo nhóm | `BACKLOG` | Chưa ưu tiên trước thi |
| Project cover backend | `REAL` | Không sửa nếu API pass |
| Chọn/upload cover khi tạo Project | `FRONTEND_MISSING` | Phải hoàn thiện |
| Role Management UI | `REAL / FROZEN` | Không xây lại |
| Permission function catalog | `PARTIAL` | Bổ sung inventory và mapping |
| Role History | `BACKLOG/PARTIAL` | Làm sau P0, chỉ khi đủ thời gian |
| Activity Feed | `MOCK` | Làm MVP thật nếu đạt P0 |
| Daily Check-in | `MOCK` | Làm MVP thật nếu đạt P0 |
| Chat | `MOCK` | Giữ route; không xóa; triển khai sau Feed/Check-in |
| Stickies | `LOCAL_ONLY` | Giữ; ghi rõ local nếu cần; không ưu tiên |
| Forms | `PLACEHOLDER` | Giữ route; không xóa; backlog |
| Docs | `PLACEHOLDER` | Giữ route; không xóa; backlog |
| Development | `PARTIAL/PLACEHOLDER` | Giữ route; GitHub integration là backlog |
| Reports | `PARTIAL` | Không viết lại; chỉ sửa số liệu sai hoặc action bị disable cần demo |

---

# 5. Quyết định về Chat, Feed, Check-in, Forms, Docs và Development

Không xóa route, không xóa menu và không xóa source.

Áp dụng quy tắc:

- Trang hoặc phần đã hoạt động thật: không gắn nhãn.
- Chức năng có API thật nhưng chưa đầy đủ: dùng `Beta` ở tiêu đề hoặc tooltip.
- Nút chưa có luồng: disable và ghi lý do cụ thể.
- Chức năng hoàn toàn chưa có backend: dùng `Coming soon` ngay tại action liên quan, không phủ cả website.
- Chức năng lưu localStorage: dùng ghi chú `Lưu trên thiết bị này` khi người dùng cần biết.
- Không để nút bấm im lặng.
- Không hiện toast “thành công” khi không có thao tác backend.
- Không gọi một chức năng rule-based là “AI” nếu không gọi AI thật.

Thứ tự phát triển:

```text
Activity Feed thật
→ Daily Check-in thật
→ Chat channel/DM thật
→ Forms
→ Docs integration
→ Development/GitHub integration
```

---

# 6. Nguồn tham khảo UI/UX và cách áp dụng

## 6.1. Taste Skill

Nguồn: https://github.com/Leonxlnx/taste-skill.git

Dùng cho:

- Audit dự án hiện có trước khi redesign.
- Bản đồ design system.
- Phân cấp nội dung, spacing, typography, motion.
- Chống UI generic hoặc redesign tùy tiện.

Thiết lập chuẩn cho SprintA:

```text
DESIGN_VARIANCE = 4/10
MOTION_INTENSITY = 4/10
VISUAL_DENSITY = 7/10
```

Skill ưu tiên:

```text
design-taste-frontend
redesign-existing-projects
high-end-visual-design
```

Không dùng `gpt-taste` ở mức quá mạnh cho toàn hệ thống. Không để AI tự đổi layout cốt lõi.

## 6.2. React Bits / Vue Bits

Nguồn: https://github.com/DavidHDev/react-bits.git

SprintA là Vue 3 nên:

- Không cài React Bits trực tiếp.
- Dùng ý tưởng hoặc official Vue port khi phù hợp.
- Chỉ lấy các pattern nhẹ: text reveal, count-up, shimmer, animated list, spotlight nhẹ, transition giữa trạng thái.
- Không dùng background 3D/particle trong dashboard, table, Kanban hoặc form.
- Component animation phải tree-shakeable và chỉ load khi cần.

## 6.3. GSAP

Nguồn: https://gsap.com/

Dùng có chọn lọc:

- Landing hero và section reveal.
- KPI count-up một lần.
- FLIP cho thay đổi layout/filter.
- Modal/drawer sequence quan trọng.
- Project cover transition.
- Task state transition nếu không xung đột thư viện drag-drop.

Không dùng:

- Mọi button hover.
- Mọi table row.
- Animation vô hạn trên nhiều component.
- Hiệu ứng cản trở đọc dữ liệu.
- Animation thiếu cleanup.

Bắt buộc:

- `prefers-reduced-motion`.
- Cleanup khi unmount.
- Không làm tăng đáng kể bundle hoặc blocking time.
- Animation UI thường 120–320ms; landing 450–700ms.

## 6.4. 21st

Nguồn: https://github.com/serafimcloud/21st.git

Chỉ tham khảo pattern vì đây là hệ React/Tailwind:

- Logic component tách khỏi demo content.
- Component tái sử dụng qua props.
- CSS variables cho theme.
- Light/dark đồng bộ.
- Quality over quantity.
- Card/modal/empty/filter/search dùng Foundation hiện có.
- Không copy Tailwind/Radix component vào Vue một cách máy móc.

## 6.5. Visual direction thống nhất

```text
Tên phong cách: Premium Calm Futuristic SaaS
Nền sáng: off-white / neutral cool
Nền tối: navy / charcoal
Màu chính: cyan-blue
Màu phụ: violet, mint, amber
Border radius app: 8–12px
Border radius landing: 14–18px
Shadow: nhẹ, sạch, không glow quá mức
Density: cao vừa phải cho dashboard
Typography: rõ hierarchy, dễ đọc, hạn chế uppercase
Animation: hỗ trợ nhận biết trạng thái, không phô diễn
```

Foundation hiện có là nguồn chuẩn:

```text
AppPageLayout
AppPageHeader
AppToolbar
AppSearchInput
AppSelect
AppCard
AppEmptyState
AppAvatar
AppUserChip
AppStatusBadge
AppPriorityBadge
AppModal
AppFormField
```

Không tạo component mới nếu Foundation đã đáp ứng.

---

# 7. Chiến lược phát hành nhanh

## 7.1. Demo Release — bắt buộc

Phải hoàn thành trước khi làm stretch:

1. Đồng bộ Auth/User/Avatar.
2. Site Selection đúng tài khoản và logout.
3. Frontend Task Contingency Plan nối API thật.
4. Chọn/upload Project Cover khi tạo project.
5. i18n các luồng demo chính.
6. Daily Focus widget trên Dashboard và deep-link đúng task.
7. Performance baseline + sửa bottleneck có bằng chứng.
8. Functional/Permission catalog đủ để trình bày.
9. Regression test và demo script.
10. Không có nút báo thành công giả trong luồng demo.

## 7.2. Collaboration MVP — thực hiện khi P0 xanh

1. Activity Feed thật.
2. Daily Check-in thật theo Project/Team.
3. Chat thật là stretch.
4. Forms, Docs, Development là backlog sau thi nếu không phải tiêu chí chấm.

## 7.3. Không cố hoàn thành mọi module giả trước kỳ thi

Lý do:

- Chat, Forms, Docs và Development đều là workstream lớn.
- Làm dàn trải dễ làm hỏng core.
- Bản demo mạnh cần luồng end-to-end thật hơn là nhiều màn hình giả.
- Ưu tiên dữ liệu tồn tại sau reload, quyền đúng, UX nhất quán và báo cáo rõ.

---

# 8. Phân chia công việc chi tiết

# 8.1. KHÔI — PO, Integration, AI, Project Cover

## KHOI-00 — Baseline và quản lý tích hợp

**Mức ưu tiên:** P0  
**Thời lượng dự kiến:** 0.5 ngày

### Mục tiêu

- Tạo nhánh tích hợp.
- Chốt baseline.
- Tạo bảng tiến độ.
- Kiểm soát shared files và API contract.

### Việc làm

- Tạo `integration/demo-release`.
- Tạo `docs/progress/README.md`.
- Tạo PR template hoặc yêu cầu báo cáo chuẩn.
- Chốt owner shared files.
- Chạy build baseline frontend/backend.
- Ghi lỗi baseline trước khi thành viên bắt đầu.

### Shared files chỉ Khôi được tự sửa

```text
Frontend/package.json
Frontend/src/main.js
Frontend/src/router/**
Frontend/src/style.css
Frontend/src/assets/styles/**
Backend/src/TaskManagement.API/Program.cs
Backend/src/TaskManagement.Infrastructure/Migrations/**
.github/workflows/**
```

Thành viên khác muốn sửa phải báo Khôi trước.

### Definition of Done

- Baseline build được hoặc có danh sách lỗi hiện hữu.
- Mỗi người có branch riêng.
- Không ai sửa cùng shared file mà chưa thống nhất.

---

## KHOI-01 — Project Cover hoàn chỉnh

**Mức ưu tiên:** P0  
**Thời lượng dự kiến:** 1–1.5 ngày  
**Branch:** `feat/khoi-project-cover`

### Phạm vi

- Chọn ảnh khi tạo project.
- Upload ảnh thật.
- Preview.
- Xóa/thay ảnh.
- Alt text.
- Đồng bộ list/header/sidebar.

### Được đọc/sửa

```text
Frontend/src/components/CreateProjectModal.vue
Frontend/src/views/ProjectSettings.vue
Frontend/src/store/useProjectStore.js
Frontend/src/components/layout/ProjectLayoutWrapper.vue
Frontend/src/views/Dashboard.vue
Backend/src/TaskManagement.API/Controllers/ProjectsController.cs
Backend/src/TaskManagement.Domain/Entities/Project.cs
```

### Không được làm

- Không sửa entity/migration nếu `CoverUrl`, `CoverAltText`, `Icon` đã đủ.
- Không thêm ảnh mẫu fake.
- Không dùng URL ảnh hard-code.
- Không redesign Project Detail.

### Acceptance Criteria

- Người dùng chọn/upload ảnh khi tạo project.
- Project tạo thành công và cover lưu database.
- F5 còn cover.
- Cover hiển thị đồng bộ ở các vị trí đã xác định.
- Ảnh lỗi có fallback icon/tên project.
- Có giới hạn loại file/dung lượng và thông báo rõ.
- Không lộ path local.
- Dark/light không vỡ.

---

## KHOI-02 — AI smoke test và chỉ sửa lỗi thật

**Mức ưu tiên:** P0 kiểm tra, P1 sửa  
**Thời lượng:** 0.5–1 ngày  
**Branch:** `fix/khoi-ai-smoke`

### Test bắt buộc

- Preview action.
- Confirm action.
- Không confirm thì không mutate data.
- Create task/project/cycle/goal theo registry hiện có.
- Update task status.
- Assign task.
- Không tạo dữ liệu trùng khi retry.
- Usage/credit hiển thị đúng nếu cấu hình có.

### Quy tắc

Nếu tất cả pass: không sửa AI.

Nếu fail:

- Chỉ sửa action bị lỗi.
- Không refactor toàn AI service.
- Không đổi prompt chung nếu lỗi thuộc mapping nhỏ.
- Ghi payload, response và DB evidence.

---

## KHOI-03 — Review permission backend và merge

**Mức ưu tiên:** P1  
**Dependency:** PHAT-01

### Việc làm

- Nhận ma trận action từ Phát.
- Đối chiếu endpoint nhạy cảm.
- Chỉ bổ sung guard còn thiếu.
- Không tạo permission trùng.
- Review PR backend của Tú và Quân.
- Merge theo thứ tự tích hợp ở mục 11.

---

# 8.2. TÚ — Identity, Avatar, Site Selection, Contingency

## TU-01 — User Context và Avatar nguồn duy nhất

**Mức ưu tiên:** P0  
**Thời lượng:** 1 ngày  
**Branch:** `fix/tu-auth-avatar-context`

### Vấn đề phải xử lý

- Session chuẩn dùng `sessionStorage['user']`.
- Một số component vẫn ghi key cũ hoặc nguồn khác.
- Avatar đổi chưa cập nhật ngay toàn hệ thống.
- Một số nơi chỉ hiện initials dù có avatar thật.

### Được đọc/sửa

```text
Frontend/src/utils/authSession.js
Frontend/src/utils/permissions.js
Frontend/src/components/UserDropdown.vue
Frontend/src/components/common/UserAvatar.vue
Frontend/src/components/common/Foundation/AppAvatar.vue
Frontend/src/views/Profile.vue
Frontend/src/components/NotificationsDropdown.vue
Frontend/src/store/usePeopleStore.js
Backend/src/TaskManagement.API/Controllers/UserContextController.cs
Backend/src/TaskManagement.API/Controllers/UsersController.cs
```

### Giải pháp yêu cầu

- Tạo hoặc chuẩn hóa một reactive current-user store.
- `saveAuthSession` và store dùng cùng payload.
- Không ghi `localStorage['sprinta_user']`.
- Khi cập nhật avatar:
  - server lưu thành công;
  - store cập nhật;
  - session cập nhật;
  - các component đang mở cập nhật không reload.
- Avatar fallback:
  1. `avatarUrl` hợp lệ;
  2. initials từ fullName/username/email;
  3. icon mặc định.
- Có loading state tránh nhấp nháy avatar sai.

### Acceptance Criteria

- Login một lần, chuyển Landing → Site Selection → Dashboard → Project → Comment → Notification vẫn cùng tài khoản/avatar.
- Đổi avatar cập nhật ngay.
- F5 không mất avatar.
- Logout xóa sạch phiên.
- Không còn key auth/avatar cũ gây xung đột.
- Không hiện email/user fake.

---

## TU-02 — Hoàn thiện Site Selection

**Mức ưu tiên:** P0  
**Thời lượng:** 0.5–1 ngày  
**Branch:** có thể cùng TU-01 nếu tránh PR quá lớn

### Yêu cầu bố cục

```text
┌─────────────────────────────────────────────────────────────┐
│ [Logo SprintA] [Vào SprintA]        [Avatar] [Tên] [Logout] │
└─────────────────────────────────────────────────────────────┘
```

### Được đọc/sửa

```text
Frontend/src/views/SiteSelection.vue
Frontend/src/store/useSiteStore.js
Frontend/src/components/UserDropdown.vue
Frontend/src/components/common/UserAvatar.vue
Frontend/src/i18n/locales/vi.js
Frontend/src/i18n/locales/en.js
```

### Acceptance Criteria

- Nút vào SprintA ở phía trái cùng brand.
- Tài khoản ở bên phải.
- Avatar thật.
- Dropdown có profile/logout.
- Site lấy API thật.
- Create/join site không hỏng.
- Nút chưa có flow phải disable và có tooltip.
- Responsive 390px không tràn.

---

## TU-03 — Frontend Kế hoạch dự phòng

**Mức ưu tiên:** P0  
**Thời lượng:** 1–1.5 ngày  
**Branch:** `feat/tu-task-contingency-ui`

### Backend đã có

```text
GET    /api/projects/{projectId}/worktasks/{taskId}/contingency-plans
POST   /api/projects/{projectId}/worktasks/{taskId}/contingency-plans
PUT    /api/projects/{projectId}/worktasks/{taskId}/contingency-plans/{planId}
DELETE /api/projects/{projectId}/worktasks/{taskId}/contingency-plans/{planId}
```

Không sửa backend nếu API pass.

### Được đọc/sửa

```text
Frontend/src/components/TaskDetail.vue
Frontend/src/components/**/TaskDetail*.vue
Frontend/src/store/useWorkTaskStore.js
Frontend/src/components/common/Foundation/**
Backend/src/TaskManagement.API/Controllers/TaskContingencyPlansController.cs
```

AI phải tìm đúng Task Detail đang được route hiện tại sử dụng trước khi sửa.

### UI yêu cầu

```text
Task Detail
└── Kế hoạch dự phòng
    ├── Danh sách plan
    ├── Tạo plan
    ├── Risk
    ├── Cause
    ├── Response Plan
    ├── Support Person
    ├── Replacement Deadline
    ├── Impact Level
    ├── Trigger Condition
    └── Status
```

### Acceptance Criteria

- CRUD thật.
- Người hỗ trợ lấy project member thật.
- Low/Medium/High/Critical có badge đúng.
- Status Draft/Open/Active/Triggered/Resolved/Archived theo backend.
- Validation risk + response plan.
- Quyền 403 hiển thị đúng, không giả thành công.
- Reload còn dữ liệu.
- Delete có confirm.
- Không tạo duplicate khi double click.
- Audit backend vẫn hoạt động.

---

# 8.3. QUÂN — Collaboration MVP

## QUAN-01 — Activity Feed thật

**Mức ưu tiên:** P1, bắt đầu sau khi P0 core ổn  
**Thời lượng:** 1.5–2 ngày  
**Branch:** `feat/quan-activity-feed`

### Mục tiêu MVP

- Feed tổng hợp theo workspace/project.
- Tạo post/update.
- Comment cơ bản.
- Dữ liệu lưu database.
- Reload còn dữ liệu.
- Có actor avatar thật.
- Có filter project.

### Được đọc/sửa

```text
Frontend/src/views/ActivityFeed.vue
Frontend/src/api/signalrService.js
Frontend/src/store/**
Backend/src/TaskManagement.API/Controllers/**
Backend/src/TaskManagement.Domain/Entities/**
Backend/src/TaskManagement.Infrastructure/Data/ApplicationDbContext.cs
```

### Quy tắc schema

- Trước khi tạo entity mới, tìm xem `AuditLog`, `ProjectUpdate`, `Comment`, `Activity` hiện có có thể tái sử dụng không.
- Không tạo entity trùng nghĩa.
- Migration phải tách riêng và Khôi review.
- Không dùng mảng hard-code.

### MVP acceptance

- GET feed có pagination.
- POST feed lưu DB.
- Comment lưu DB.
- Filter project hoạt động.
- Empty/loading/error rõ.
- Không dùng fake member/avatar.
- Có quyền đọc/ghi theo workspace/project.
- Nếu SignalR chưa kịp: cho refresh thật, không giả realtime.

---

## QUAN-02 — Daily Check-in thật theo Project/Team

**Mức ưu tiên:** P1  
**Thời lượng:** 1.5–2 ngày  
**Branch:** `feat/quan-daily-checkin`

### Mục tiêu MVP

- Check-in gắn Project hoặc Team.
- Mỗi user/ngày có một bản check-in.
- Hôm qua đã làm.
- Hôm nay sẽ làm.
- Blocker.
- Edit trong ngày.
- Danh sách thành viên lấy thật.
- Summary rule-based được gọi đúng tên; AI summary chỉ khi thật sự gọi AI.

### Acceptance Criteria

- Unique user + context + date.
- CRUD thật.
- Timezone xử lý rõ.
- Reload còn dữ liệu.
- Permission đúng.
- Không hard-code ngày hoặc thành viên.
- Không dùng `setTimeout` để giả AI.
- Project page có đường vào Check-in.
- Global route có thể giữ để xem tổng hợp.

---

## QUAN-03 — Chat thật

**Mức ưu tiên:** Stretch  
**Chỉ bắt đầu khi QUAN-01 và QUAN-02 pass**

MVP đề xuất:

```text
Conversation
ConversationMember
Message
MessageAttachment
```

Thứ tự:

1. Project channel.
2. Send/read message.
3. SignalR realtime.
4. Unread count.
5. Attachment.
6. Direct message.
7. Reaction/mention.
8. Call để sau.

Không cố làm video call trước thi.

---

# 8.4. QUÂN ĐẠT — BA, Performance, QA, Báo cáo

## QDAT-01 — Performance baseline có số liệu

**Mức ưu tiên:** P0  
**Thời lượng:** 1 ngày audit + 1 ngày fix được phê duyệt  
**Branch:** `perf/qdat-measured-optimizations`

### Không được tối ưu mò

Đo trước:

- Initial JS bundle.
- Route chunk lớn.
- Dashboard load time.
- API request count.
- Request trùng.
- API average/P50/P95/max từ `/api/performance/summary`.
- Ảnh lớn.
- List nhiều row.
- Search gọi API mỗi keypress.
- Component re-render bất thường.

### Output bắt buộc

Tạo:

```text
docs/performance/PERFORMANCE_BASELINE.md
```

Bảng:

| Route/API | Baseline | Vấn đề | Nguyên nhân | Fix đề xuất | Sau fix |
|---|---:|---|---|---|---:|

### Chỉ được sửa khi có bằng chứng

- Debounce search.
- Cancel request cũ.
- Cache project/member/status.
- Pagination.
- Virtual scroll.
- Lazy load chart/editor.
- Image compression.
- Loại request trùng.
- Skeleton.

### Acceptance Criteria

- Có số trước/sau.
- Không giảm chức năng.
- Không cache dữ liệu gây stale sai.
- Không thêm animation làm chậm.
- Build pass.
- Báo cáo đưa được vào tài liệu thi.

---

## QDAT-02 — Master Test Matrix

**Mức ưu tiên:** P0  
**Thời lượng:** liên tục

Tạo:

```text
docs/testing/DEMO_REGRESSION_MATRIX.md
```

Mỗi testcase:

```text
ID
Module
Precondition
Steps
Expected
Actual
Status
Evidence
Bug ID
Owner
Retest
```

Nhóm test bắt buộc:

- Auth/login/logout/2FA.
- User/avatar.
- Site Selection.
- Workspace/Project.
- Task CRUD/status/assignee/deadline.
- Contingency Plan.
- Daily Focus.
- Goals.
- Notification.
- Role/Permission.
- Import/Export.
- AI action preview/confirm.
- Project cover.
- Collaboration nếu merge.

### Quy tắc bug

- P0: mất dữ liệu, không login, app crash, sai quyền nghiêm trọng.
- P1: core flow không hoàn thành.
- P2: UI/UX gây khó nhưng có workaround.
- P3: cosmetic.

Không giao AI “sửa tất cả bug” trong một prompt. Mỗi bug có ID riêng.

---

## QDAT-03 — Báo cáo và Demo Script

Tạo:

```text
docs/demo/SPRINTA_DEMO_SCRIPT.md
docs/report/FEATURE_COMPLETION_REPORT.md
```

Demo chỉ dùng luồng thật:

1. Login.
2. Chọn workspace.
3. Tạo/chọn project và cover.
4. Tạo task.
5. Gán người và deadline.
6. Thay trạng thái.
7. Tạo contingency plan.
8. Daily Focus.
9. Goal.
10. Notification.
11. Permission.
12. AI Preview → Confirm.
13. Report/Export.

Không demo nút chưa hoạt động.

---

# 8.5. KIỆT — i18n và Daily Focus

## KIET-01 — i18n critical path

**Mức ưu tiên:** P0  
**Thời lượng:** 1–1.5 ngày  
**Branch:** `fix/kiet-i18n-critical`

### Phạm vi P0

- Landing.
- Login/Register.
- Site Selection.
- Dashboard.
- Project/Create Project.
- Task Detail.
- Contingency Plan.
- Daily Focus.
- Notification.
- Role Management action chính.

### Quy tắc

- Chỉ chuyển literal UI string sang key.
- Không thay layout.
- Không đổi backend message tùy tiện.
- Validation frontend dùng key.
- Backend nên trả code/message; frontend dịch code khi có.
- Không tạo key trùng nghĩa.
- Key theo module.

### Acceptance Criteria

- Đổi VI/EN một lần, trang hiện tại cập nhật ngay.
- Button/dialog/placeholder/tooltip/loading/empty/error chính đều đổi.
- Không còn câu Việt giữa giao diện English ở critical path.
- Không còn câu English giữa giao diện Việt ở critical path.
- Build pass.

---

## KIET-02 — Daily Focus Widget trên Dashboard

**Mức ưu tiên:** P0  
**Thời lượng:** 1–1.5 ngày  
**Branch:** `feat/kiet-daily-focus-widget`

### Giữ nguyên

- `/priority` vẫn tồn tại.
- Scoring hiện có không viết lại nếu đúng.
- Task lấy API thật.

### Làm mới

- Tách logic/UI tái sử dụng thành widget.
- Dashboard hiển thị 3–5 việc ưu tiên.
- Nút “Xem tất cả”.
- Filter project.
- Bấm item mở đúng Task Detail bằng projectId + taskId.
- Đổi chữ “AI Hint” thành “Gợi ý ưu tiên” khi chưa gọi AI thật.
- Postpone:
  - Nếu chưa làm backend: ghi rõ “hoãn trên thiết bị hôm nay”.
  - Ưu tiên tạo API khi Khôi chấp thuận.
- Không trùng toàn bộ nội dung của trang Daily Focus.

### Acceptance Criteria

- Dashboard không gọi trùng `/tasks/search` nhiều lần.
- Widget có loading/error/empty.
- Click đúng task.
- Priority mapping thống nhất.
- Responsive.
- Không animation nặng.

---

# 8.6. PHÁT — Functional Catalog, Permission, UI QA

## PHAT-01 — Module Action Inventory

**Mức ưu tiên:** P0  
**Thời lượng:** 1.5–2 ngày  
**Branch:** `docs/phat-function-permission-catalog`

### Mục tiêu

Dùng thử từng trang và ghi các action thật đang tồn tại.

Tạo:

```text
docs/permissions/MODULE_ACTION_MATRIX.md
```

Mẫu:

| Module | Route | Action code đề xuất | UI có | API có | Backend guard có | Test pass | Ghi chú |
|---|---|---|---:|---:|---:|---:|---|

Module tối thiểu:

- Auth.
- Workspace/Site.
- Project.
- Project Member.
- Task.
- Task Comment.
- Task Attachment.
- Checklist.
- Cycle.
- Module.
- Goal.
- Team.
- People.
- Dashboard.
- Daily Focus.
- Report.
- Calendar.
- Notification.
- Integration.
- AI.
- Admin.
- Role.
- Permission.
- Contingency Plan.
- Project Cover.

### Quy tắc action code

```text
module.resource.action
```

Ví dụ:

```text
projects.view
projects.create
projects.update
projects.delete
projects.cover.update
tasks.view
tasks.create
tasks.update
tasks.delete
tasks.status.update
tasks.assignee.update
tasks.contingency.manage
```

Không tạo action:

- Trùng nghĩa.
- Không có UI/API.
- Chỉ dựa trên tên button mà không test.
- Gộp nhiều hành vi rủi ro vào một quyền.

---

## PHAT-02 — Role Management và permission UX QA

**Mức ưu tiên:** P1  
**Dependency:** PHAT-01

### Kiểm tra

- Search/filter.
- Tree/matrix.
- Bulk enable/disable.
- Protected role.
- High-risk badge.
- Save/reload.
- Member assignment.
- Backend 403 khi không đủ quyền.
- History tab.

### Không được

- Xây lại Role Management.
- Thay toàn bộ Element Plus.
- Thêm permissions hàng loạt trước khi map API.

### Output

- Bug list có ID.
- Danh sách permission thiếu backend guard cho Khôi.
- Chỉ sửa UI nhỏ: i18n, empty/error/loading, alignment, disabled reason.

---

# 9. Timeline Fast Track 7 ngày

> Có thể nén còn 4–5 ngày bằng cách bỏ Collaboration MVP và chỉ làm Demo Release.

## Ngày 0 — Baseline và chia nhánh

- Khôi: KHOI-00.
- Quân Đạt: tạo test matrix.
- Phát: tạo khung permission matrix.
- Cả nhóm: build baseline, không sửa vội.

## Ngày 1

- Tú: TU-01.
- Khôi: KHOI-01 bắt đầu.
- Kiệt: KIET-01.
- Quân Đạt: performance baseline.
- Phát: inventory Project/Task/Auth.
- Quân: khảo sát model/API cho Feed/Check-in, chưa tạo schema vội.

## Ngày 2

- Tú: TU-02 + test avatar.
- Khôi: KHOI-01 hoàn tất.
- Kiệt: KIET-02 bắt đầu.
- Quân Đạt: đề xuất 3 bottleneck lớn nhất.
- Phát: inventory module còn lại.
- Quân: QUAN-01 backend/frontend MVP.

## Ngày 3

- Tú: TU-03.
- Khôi: KHOI-02 smoke test AI.
- Kiệt: KIET-02 hoàn tất.
- Quân Đạt: performance fix đã duyệt + retest.
- Phát: PHAT-02.
- Quân: QUAN-01 hoàn tất.

## Ngày 4

- Integration Gate 1.
- Fix conflict và P0/P1.
- Quân bắt đầu QUAN-02 nếu core xanh.
- Không mở thêm workstream mới nếu core còn đỏ.

## Ngày 5

- QUAN-02.
- Full regression.
- Hoàn thiện i18n và UI critical.
- Khôi review backend permission.
- Chốt demo data.

## Ngày 6

- Integration Gate 2.
- Demo rehearsal.
- Performance/report evidence.
- Chỉ fix P0/P1.
- Freeze feature.

## Ngày 7

- Buffer.
- Báo cáo.
- Video/screenshot.
- Final demo rehearsal.
- Không refactor lớn.

---

# 10. Kế hoạch 4 ngày khi quá gấp

Nếu còn dưới 5 ngày:

## Ngày 1

- Tú: Avatar + Site Selection.
- Khôi: Project Cover.
- Kiệt: i18n critical.
- Quân Đạt: baseline + test matrix.
- Phát: permission inventory.
- Quân: hỗ trợ backend/test; chưa làm Chat.

## Ngày 2

- Tú: Contingency UI.
- Kiệt: Daily Focus widget.
- Khôi: AI smoke + integration.
- Quân: Activity Feed tối thiểu hoặc hỗ trợ fix P0.
- Quân Đạt/Phát: test.

## Ngày 3

- Merge.
- Full regression.
- Fix P0/P1.
- Báo cáo.

## Ngày 4

- Freeze.
- Demo rehearsal.
- Không thêm feature.

---

# 11. Git workflow và thứ tự merge

## 11.1. Branch

```text
integration/demo-release
feat/khoi-project-cover
fix/khoi-ai-smoke
fix/tu-auth-avatar-context
feat/tu-task-contingency-ui
feat/quan-activity-feed
feat/quan-daily-checkin
perf/qdat-measured-optimizations
fix/kiet-i18n-critical
feat/kiet-daily-focus-widget
docs/phat-function-permission-catalog
```

## 11.2. Quy tắc

- Pull `main` trước khi tạo branch.
- Mỗi branch chỉ chứa một workstream.
- Commit nhỏ, có ý nghĩa.
- Không commit `.env`, secret, database backup, build output.
- Không force push `main`.
- Không merge trực tiếp khi chưa review.
- Migration phải một PR riêng hoặc rõ ràng trong PR backend.
- Shared files do Khôi xử lý conflict.

## 11.3. Thứ tự merge đề xuất

1. `fix/tu-auth-avatar-context`
2. `fix/kiet-i18n-critical`
3. `feat/khoi-project-cover`
4. `feat/tu-task-contingency-ui`
5. `feat/kiet-daily-focus-widget`
6. `perf/qdat-measured-optimizations`
7. `docs/phat-function-permission-catalog`
8. Backend permission fixes
9. `feat/quan-activity-feed`
10. `feat/quan-daily-checkin`

Lý do: Identity và i18n là nền cho các UI khác; collaboration merge sau để giảm conflict.

---

# 12. Definition of Done toàn dự án

Một task chỉ được đánh dấu Done khi:

- [ ] Đúng scope.
- [ ] Không sửa phần frozen ngoài yêu cầu.
- [ ] Không mock/hard-code.
- [ ] API thật hoạt động.
- [ ] Dữ liệu còn sau reload.
- [ ] Loading/error/empty có.
- [ ] Permission đúng.
- [ ] Không tạo duplicate khi double click/retry.
- [ ] Light/dark không vỡ.
- [ ] Responsive tối thiểu.
- [ ] i18n critical pass.
- [ ] Build pass.
- [ ] Test evidence có.
- [ ] Diff không chứa file ngoài scope.
- [ ] Báo cáo ngắn theo A–E.
- [ ] Quân Đạt retest.
- [ ] Khôi approve.

---

# 13. Integration Gates

## Gate 1 — Functional

- Login/logout.
- Avatar.
- Site Selection.
- Project create/cover.
- Task CRUD.
- Contingency.
- Daily Focus.
- Goal.
- Notification.
- Permission.
- AI preview/confirm.
- Import/export.

Không pass Gate 1 thì không thêm animation mới.

## Gate 2 — UX

- Layout nhất quán.
- Foundation dùng đúng.
- i18n.
- Loading/error/empty.
- Responsive.
- Dark mode.
- Animation đúng mức.
- Không nút im lặng.

## Gate 3 — Demo

- Seed/demo account hoạt động.
- Kịch bản demo hoàn thành không reload bất thường.
- Không click vào placeholder.
- Không lộ secret/error stack.
- Có backup video/screenshots.
- Báo cáo chức năng khớp source thật.

---

# 14. Mẫu quản lý tiến độ

Mỗi người tạo file riêng để tránh conflict:

```text
docs/progress/khoi.md
docs/progress/tu.md
docs/progress/quan.md
docs/progress/quan-dat.md
docs/progress/kiet.md
docs/progress/phat.md
```

Mẫu:

```md
# Progress — [Tên]

## [TASK-ID]
- Status: TODO / DOING / BLOCKED / REVIEW / DONE
- Started:
- Target:
- Branch:
- Commit:
- Files changed:
- API/DB:
- Tests:
- Evidence:
- Blocker:
- Next:
```

Bảng PO:

| Task | Owner | Priority | Dependency | Status | PR | Retest |
|---|---|---:|---|---|---|---|
| TU-01 | Tú | P0 | None | TODO | | |
| TU-02 | Tú | P0 | TU-01 | TODO | | |
| TU-03 | Tú | P0 | Task API | TODO | | |
| KHOI-01 | Khôi | P0 | Project API | TODO | | |
| KHOI-02 | Khôi | P0 | AI config | TODO | | |
| KIET-01 | Kiệt | P0 | None | TODO | | |
| KIET-02 | Kiệt | P0 | Dashboard | TODO | | |
| QDAT-01 | Quân Đạt | P0 | Baseline | TODO | | |
| QDAT-02 | Quân Đạt | P0 | All | TODO | | |
| PHAT-01 | Phát | P0 | Routes/API | TODO | | |
| QUAN-01 | Quân | P1 | Gate 1 | TODO | | |
| QUAN-02 | Quân | P1 | QUAN-01 | TODO | | |

---

# 15. Prompt gốc dùng cho mọi AI Agent

Copy prompt sau, rồi điền Task ID:

```text
Bạn đang làm việc trong repository:
https://github.com/DinhTuanKhoiTB01696/QuanLyCongViec.git

Task của bạn: [TASK-ID — TÊN TASK]
Owner: [TÊN THÀNH VIÊN]
Branch: [TÊN BRANCH]

BẮT BUỘC:
1. Đọc file docs/SPRINTA_AI_EXECUTION_MASTER_PLAN.md, chỉ đọc:
   - Luật AI
   - Trạng thái/frozen
   - Section của [TASK-ID]
   - Definition of Done
2. Chỉ đọc các file trong mục Allowed files của task.
3. Trước khi sửa, xác minh phần nào đã hoạt động và không viết lại phần đó.
4. Không đọc toàn bộ repo.
5. Không mock, seed, hard-code, localStorage hoặc setTimeout để giả backend.
6. Không redesign ngoài scope.
7. Không đổi API contract nếu không cần.
8. Không sửa shared files nếu chưa được PO cho phép.
9. Chỉ chạy test/build liên quan.
10. Khi đạt acceptance criteria thì dừng.

QUY TRÌNH:
Research đúng file → nêu kế hoạch tối đa 8 bước → thực hiện → test → kiểm tra git diff → báo cáo A–E.

BÁO CÁO:
A. Task ID và trạng thái
B. File đã sửa
C. Chức năng thật đã hoàn thành
D. Test và kết quả
E. Phần còn thiếu/rủi ro

Không báo cáo dài dòng và không đề xuất thêm feature ngoài task.
```

---

# 16. Prompt riêng cho từng thành viên

## 16.1. Prompt cho Khôi

```text
Thực hiện KHOI-00, KHOI-01 và KHOI-02 theo thứ tự trong
docs/SPRINTA_AI_EXECUTION_MASTER_PLAN.md.

Vai trò của tôi là PO + integration owner. Không đọc toàn bộ repo.
Ưu tiên:
1. Baseline và shared-file ownership.
2. Project Cover end-to-end.
3. Smoke test AI; chỉ sửa action thực sự fail.
4. Review backend/permission và merge.

Không refactor AI hoặc Project module ngoài acceptance criteria.
Mọi thay đổi phải có build/test evidence và ghi rõ ảnh hưởng database/API.
```

## 16.2. Prompt cho Tú

```text
Thực hiện TU-01 → TU-02 → TU-03 trong
docs/SPRINTA_AI_EXECUTION_MASTER_PLAN.md.

Chỉ tập trung:
- một nguồn User Context/Auth Session;
- avatar đồng bộ realtime;
- Site Selection đúng tài khoản/logout;
- frontend Contingency Plan gọi API đã có.

Không viết lại landing auth.
Không tạo backend/migration Contingency mới nếu API hiện tại pass.
Không sửa module ngoài allowed files.
```

## 16.3. Prompt cho Quân

```text
Thực hiện QUAN-01 trước, QUAN-02 sau; QUAN-03 chỉ là stretch.

Trước khi tạo entity mới, tìm entity/API hiện có để tránh trùng.
Activity Feed và Check-in phải lưu database thật, reload còn dữ liệu,
có permission và không dùng dữ liệu thành viên/avatar hard-code.

Không làm video call.
Không dùng setTimeout giả AI/realtime.
Không bắt đầu Chat nếu Feed và Check-in chưa pass acceptance criteria.
```

## 16.4. Prompt cho Quân Đạt

```text
Thực hiện QDAT-01, QDAT-02, QDAT-03.

Không tối ưu theo cảm giác. Đo trước, ghi baseline, chỉ sửa bottleneck có bằng chứng.
Tạo test matrix và báo cáo có thể đưa vào đồ án.
Mỗi bug phải có ID, mức độ, bước tái hiện, expected/actual và owner.
Không giao AI sửa nhiều bug không liên quan trong một prompt.
```

## 16.5. Prompt cho Kiệt

```text
Thực hiện KIET-01 → KIET-02.

KIET-01 chỉ chuyển UI literal của critical path sang i18n, không đổi layout.
KIET-02 giữ route Daily Focus hiện tại, tách widget cho Dashboard,
mở đúng Task Detail và đổi tên AI Hint nếu chỉ là rule-based.

Không viết lại scoring khi chưa chứng minh sai.
Không thêm animation nặng.
```

## 16.6. Prompt cho Phát

```text
Thực hiện PHAT-01 → PHAT-02.

Dùng thử từng module, chỉ ghi action thật có UI/API.
Lập MODULE_ACTION_MATRIX.md và map backend guard.
Không tạo permission trùng hoặc quyền cho chức năng chưa tồn tại.
Không xây lại Role Management; chỉ QA và sửa UI nhỏ trong scope.
Báo danh sách endpoint thiếu guard cho Khôi xử lý.
```

---

# 17. Prompt sửa bug ngắn

```text
Fix bug [BUG-ID]: [mô tả].

Chỉ đọc:
- [file UI]
- [store/service]
- [controller liên quan]

Reproduce trước khi sửa.
Không refactor module.
Không đổi UI ngoài bug.
Không mock/hard-code.
Sau khi sửa, test đúng bước reproduce và kiểm tra reload/database.
Báo cáo A–E rồi dừng.
```

---

# 18. Prompt UI/UX an toàn

```text
Tinh chỉnh UI/UX cho [TRANG/COMPONENT] theo
docs/SPRINTA_AI_EXECUTION_MASTER_PLAN.md.

Bắt buộc:
- audit chức năng và khóa logic đang hoạt động;
- dùng Foundation hiện có;
- tham khảo Taste Skill theo redesign-existing-projects;
- DESIGN_VARIANCE 4, MOTION_INTENSITY 4, VISUAL_DENSITY 7;
- tham khảo React Bits/Vue Bits, GSAP, 21st chỉ ở mức pattern phù hợp Vue;
- không copy React/Tailwind;
- không redesign trang khác;
- animation phải có mục đích, reduced-motion và cleanup;
- giữ API/store/router;
- test responsive/light/dark/build.

Trước khi code, nêu tối đa 6 điểm UI cần sửa dựa trên source hiện tại.
```

---

# 19. Những việc không làm trước kỳ thi nếu chưa hoàn thành P0

- Video call.
- Full DM/chat ecosystem.
- GitHub development metrics hoàn chỉnh.
- Confluence integration.
- Form builder đầy đủ.
- Notification preference 14 nhóm.
- TOTP authenticator và backup codes.
- Zalo integration.
- 3D/particle dashboard.
- Redesign toàn bộ Admin.
- Microservice migration.
- Thay framework.
- Refactor toàn bộ backend.
- Viết lại Goal/Project/Task core.
- Thêm AI tự động mutation không cần confirm.

---

# 20. Checklist trước khi giao cho giảng viên

## Chức năng

- [ ] Login/logout.
- [ ] Avatar/user đồng bộ.
- [ ] Site Selection.
- [ ] Project + cover.
- [ ] Task CRUD.
- [ ] Assignee/status/deadline.
- [ ] Contingency Plan.
- [ ] Daily Focus.
- [ ] Goal.
- [ ] Notification.
- [ ] Role/Permission.
- [ ] Import/Export.
- [ ] AI preview/confirm.
- [ ] Report cần demo.

## Kỹ thuật

- [ ] Frontend build pass.
- [ ] Backend build/test pass.
- [ ] Migration áp dụng được.
- [ ] Không secret trong git.
- [ ] Không console error nghiêm trọng.
- [ ] Không request 404/500 trong demo flow.
- [ ] Không mock trong demo flow.
- [ ] Không sai priority mapping.
- [ ] Không sai avatar.
- [ ] Reload không mất dữ liệu.

## Trình bày

- [ ] Demo script.
- [ ] Tài khoản demo.
- [ ] Seed ổn định.
- [ ] Báo cáo chức năng.
- [ ] Permission matrix.
- [ ] Performance before/after.
- [ ] Test evidence.
- [ ] Video backup.
- [ ] Screenshot backup.
- [ ] Mỗi thành viên biết phần mình trình bày.

---

# 21. Chốt phạm vi

Bản demo tốt nhất của SprintA không phải là bản có nhiều menu nhất, mà là bản:

- Các luồng được trình bày đều chạy thật.
- Dữ liệu tồn tại sau reload.
- Avatar, quyền và ngôn ngữ nhất quán.
- Không có nút báo thành công giả.
- UI đẹp nhưng không làm mất khả năng đọc dữ liệu.
- AI chỉ thực thi sau preview và confirm.
- Mỗi thành viên có nhiệm vụ không chồng chéo.
- PO kiểm soát shared files, API contract, migration và merge.
- QA có bằng chứng thay vì nhận xét cảm tính.
- Những module chưa hoàn thiện vẫn được giữ để phát triển, nhưng trạng thái được thể hiện trung thực tại đúng action.

**Thứ tự ưu tiên cuối cùng:**

```text
Ổn định dữ liệu và quyền
→ Hoàn thành phần backend đã có nhưng thiếu frontend
→ Đồng bộ tài khoản/i18n/UI
→ Đo và tối ưu hiệu năng
→ Hoàn thiện demo/report/test
→ Collaboration MVP
→ Các module nâng cao sau kỳ thi
```
