# 📋 5 PROMPT CHO 5 AI CODEX — Copy-Paste Gửi Đi

> **Hướng dẫn:** Mở 5 session Codex riêng biệt, copy từng prompt bên dưới gửi vào từng session.  
> Mỗi AI sẽ tự biết mình là AI số mấy, được sửa file gì, và phải làm gì.

---

## 🤖 PROMPT AI-1: TASK DETAIL & TASK CORE

```
BỐI CẢNH DỰ ÁN:
Dự án QuanLyCongViec (SprintA) — Hệ thống quản lý công việc Agile/Scrum.
- Backend: ASP.NET Core 10, EF Core, SQL Server — chạy Port 5136
- Frontend: Vue 3 + Pinia + Element Plus — chạy Port 5173
- Cây thư mục:
  Backend/src/TaskManagement.API/Controllers/
  Backend/src/TaskManagement.Domain/Entities/
  Frontend/src/views/ | Frontend/src/components/ | Frontend/src/store/

VAI TRÒ CỦA BẠN:
Bạn là AI-1 trong nhóm 5 AI chạy song song. Bạn chuyên xử lý TASK DETAIL & TASK CORE.

PHÂN VÙNG FILE CỦA BẠN (CHỈ ĐƯỢC SỬA CÁC FILE NÀY):
- Backend: WorkTasksController.cs, CommentsController.cs
- Frontend: TaskDetailModal.vue, SpaceSummary.vue
- Store: useWorkTaskStore.js

QUY TẮC:
1. KHÔNG ĐƯỢC SỬA file ngoài danh sách trên
2. LUÔN ĐỌC FILE TRƯỚC KHI SỬA — không đoán nội dung
3. Sửa theo thứ tự: 🔴 CRITICAL → 🟠 HIGH → 🟡 MEDIUM → 🟢 LOW
4. Sau khi xong, tạo file docs/DONE_AI1.md liệt kê tất cả thay đổi
5. Nếu cần AI khác hỗ trợ, ghi vào docs/CROSS_DEPENDENCY.md

DANH SÁCH BUG & TASK CẦN LÀM:

🔴 CRITICAL:
1.1) Cập nhật task luôn báo "Không thể cập nhật công việc" → Kiểm tra RowVersion/concurrency trong WorkTasksController.cs PUT endpoint, đảm bảo TaskDetailModal.vue gửi đúng payload
1.2) Subtask hiển thị lên project board (PHẢI ẩn, chỉ hiện trong task detail cha) → Filter SpaceSummary.vue: chỉ hiện task có parentTaskId == null trên board

🟠 HIGH:
1.3) Các nút Priority, Assignee, Status trên mỗi task card click vào = vào detail thay vì mở dropdown → Thêm @click.stop vào các dropdown trong SpaceSummary.vue
1.4) Kéo task sang cột IN_REVIEW và CANCELLED không hoạt động → Thêm status hợp lệ vào handleDraggableChange
1.5) List View không kéo đổi trạng thái được → Thêm dropdown đổi status inline trong list view
1.6) Description: bôi đen text không thay đổi được formatting → Fix execCommand hoặc contentEditable trong TaskDetailModal.vue
1.7) Nút code format: click tạo dòng rác, phải giữ mode code cho tới khi user tắt → Toggle state code mode
1.8) Attachment trong description báo lỗi → Xóa nút attach trong description toolbar, giữ attachment riêng

🟡 MEDIUM:
1.9) Comment: format lỗi giống description, hỗ trợ gửi file đính kèm PDF/Word/Excel
1.10) Comment dạng code phải hiển thị trong <pre><code> block
1.11) Giữ chuột trái kéo ra ngoài task detail → tự đóng modal → Đổi @click.self thành @mousedown.self
1.12) Góc trên trái: chỉ giữ nút Back, bỏ nút phóng to, bỏ nút M + mũi tên
1.13) Icon emote: click phải hiện emoji picker, lưu reaction count vào DB
1.14) Thời gian "Last edited by" hiển thị sai → dùng computed relative time
1.15) Activity tab báo lỗi → Gọi API audit log cho task, hiển thị timeline
1.16) Display toggle subtask: bật = hiện, tắt = ẩn subtask list
1.17) Click ảnh đính kèm: thêm nút delete + kéo dãn hình
1.18) Parent task selector hiển thị R-1, R-2, R-3 giả → Lọc chỉ lấy task cùng projectId

BẮT ĐẦU: Hãy đọc file WorkTasksController.cs và TaskDetailModal.vue trước, sau đó bắt đầu sửa bug 1.1.
```

---

## 🤖 PROMPT AI-2: DRAFTS, MODULES, LABELS, FILTER

```
BỐI CẢNH DỰ ÁN:
Dự án QuanLyCongViec (SprintA) — Hệ thống quản lý công việc Agile/Scrum.
- Backend: ASP.NET Core 10, EF Core, SQL Server — chạy Port 5136
- Frontend: Vue 3 + Pinia + Element Plus — chạy Port 5173
- Cây thư mục:
  Backend/src/TaskManagement.API/Controllers/
  Backend/src/TaskManagement.Domain/Entities/
  Frontend/src/views/ | Frontend/src/components/

VAI TRÒ CỦA BẠN:
Bạn là AI-2 trong nhóm 5 AI chạy song song. Bạn chuyên xử lý DRAFTS, MODULES, LABELS, FILTER.

PHÂN VÙNG FILE CỦA BẠN (CHỈ ĐƯỢC SỬA CÁC FILE NÀY):
- Backend: DraftsController.cs, ModulesController.cs, LabelsController.cs
- Frontend: DraftsView.vue, ModulesTab.vue, ModulesView.vue, LabelManager.vue, FilterBar.vue

QUY TẮC:
1. KHÔNG ĐƯỢC SỬA file ngoài danh sách trên
2. LUÔN ĐỌC FILE TRƯỚC KHI SỬA
3. Sửa theo thứ tự: 🔴 → 🟠 → 🟡
4. Sau khi xong, tạo file docs/DONE_AI2.md
5. Nếu cần AI khác hỗ trợ, ghi vào docs/CROSS_DEPENDENCY.md

DANH SÁCH BUG & TASK CẦN LÀM:

🔴 CRITICAL:
2.1) Drafts: tạo 1000 bản nháp → server đứng → Thêm pagination ?page=1&pageSize=20 vào DraftsController.cs GET endpoint, thêm DB index
2.2) Drafts: API không trả data → Debug DraftsView.vue fetchDrafts + DraftsController.cs query filter

🟠 HIGH:
2.3) Drafts: 2 nút Start Date và Due Date không click được → Kiểm tra v-model binding el-date-picker
2.4) Drafts: cải thiện hiệu suất khi >1000 bản nháp → Virtual scroll frontend + pagination backend
2.5) Modules: load chậm → Thêm pagination, lazy load, giảm payload API
2.6) Modules: không cập nhật được công việc trong module → Fix logic update IssueModule trong ModulesController.cs PUT
2.7) Modules: chọn lịch không real-time → Đảm bảo @change emit + gọi API PUT ngay
2.11) Labels trong Cycle: cột labels không click được → Kiểm tra API GET labels by projectId, fix dropdown trong LabelManager.vue
2.12) Labels: DB có data nhưng dropdown không hiện → Debug API response, kiểm tra projectId filter
2.13) Filter: hay bị ẩn sau giao diện (z-index) → Set z-index: 9999 + position: fixed hoặc teleport to body trên FilterBar.vue

🟡 MEDIUM:
2.8) Modules: tính toán phần trăm chưa rõ → Tính % = (tasks DONE / total tasks) * 100
2.9) Modules: 2 nút sắp xếp + Filter chưa hoạt động → Thêm sort computed + filter logic
2.10) Modules: 3 nút trong box bên trái nút Add Module → Đối chiếu Plane UI, thêm Grid/List/Status view toggle

BẮT ĐẦU: Hãy đọc file DraftsController.cs và DraftsView.vue trước, sau đó sửa bug 2.1.
```

---

## 🤖 PROMPT AI-3: PROJECT, DASHBOARD, SIDEBAR, THÔNG BÁO, TÌM KIẾM

```
BỐI CẢNH DỰ ÁN:
Dự án QuanLyCongViec (SprintA) — Hệ thống quản lý công việc Agile/Scrum.
- Backend: ASP.NET Core 10, EF Core, SQL Server — chạy Port 5136
- Frontend: Vue 3 + Pinia + Element Plus — chạy Port 5173
- Cây thư mục:
  Backend/src/TaskManagement.API/Controllers/
  Frontend/src/views/ | Frontend/src/components/ | Frontend/src/store/ | Frontend/src/router/

VAI TRÒ CỦA BẠN:
Bạn là AI-3 trong nhóm 5 AI chạy song song. Bạn chuyên xử lý PROJECT, DASHBOARD, SIDEBAR, THÔNG BÁO, TÌM KIẾM.

PHÂN VÙNG FILE CỦA BẠN (CHỈ ĐƯỢC SỬA CÁC FILE NÀY):
- Backend: ProjectsController.cs, ProjectMembersController.cs, NotificationsController.cs, WorkspacesController.cs
- Frontend: Dashboard.vue, ManageSpaces.vue, CreateProjectModal.vue, Home.vue, NotificationsDropdown.vue
- Store: useProjectStore.js
- Router: spaceRoutes.js, dashboardRoutes.js

QUY TẮC:
1. KHÔNG ĐƯỢC SỬA file ngoài danh sách trên
2. LUÔN ĐỌC FILE TRƯỚC KHI SỬA
3. Sửa theo thứ tự: 🔴 → 🟠 → 🟡
4. Sau khi xong, tạo file docs/DONE_AI3.md
5. Nếu cần AI khác hỗ trợ, ghi vào docs/CROSS_DEPENDENCY.md

DANH SÁCH BUG & TASK CẦN LÀM:

🟠 HIGH:
3.1) Sidebar Project: chỉ hiện "Cun", phải hiện tất cả projects + click expand xem workitems/cycles/modules/views/pages → Fetch all projects từ API vào useProjectStore.js, render collapsible tree trong Home.vue
3.2) Task hiển thị không đúng project → Đảm bảo ProjectsController.cs trả task theo projectId chính xác
3.4) Nút bánh răng Settings trong Project: admin bấm không vào được admin page → Fix route trong spaceRoutes.js
3.5) Dashboard: bấm ngôi sao Favorite → F5 biến mất → Tạo API PUT /projects/{id}/favorite trong ProjectsController.cs, lưu vào DB
3.7) Dashboard: nút "New Work Item" click không có gì → Mở modal tạo task mới trong Dashboard.vue
3.8) Thanh tìm kiếm trên đầu trang không hoạt động → Gắn logic gọi API search /worktasks?search=keyword + hiện kết quả dropdown trong Home.vue
3.9) Chuông thông báo không hoạt động → Gắn logic fetch GET /notifications trong NotificationsDropdown.vue, mark read khi xem

🟡 MEDIUM:
3.3) Tạo Project: cover chỉ là màu đơn sắc → Thêm gallery ảnh cover (dùng picsum.photos) vào CreateProjectModal.vue
3.6) Dashboard: bỏ chữ "Cường", đổi thành "SprintA" → Thay hardcoded name bằng project name từ API
3.10) Thông báo: tạo notification khi assign task, comment mới, status thay đổi → Logic trong NotificationsController.cs

BẮT ĐẦU: Hãy đọc file Dashboard.vue và Home.vue trước, sau đó sửa bug 3.1.
```

---

## 🤖 PROMPT AI-4: ADMIN, HỒ SƠ, QUẢN TRỊ HỆ THỐNG

```
BỐI CẢNH DỰ ÁN:
Dự án QuanLyCongViec (SprintA) — Hệ thống quản lý công việc Agile/Scrum.
- Backend: ASP.NET Core 10, EF Core, SQL Server — chạy Port 5136
- Frontend: Vue 3 + Pinia + Element Plus — chạy Port 5173
- Có dự án Plane gốc tại D:\A\plane để đối chiếu admin features
- Cây thư mục:
  Backend/src/TaskManagement.API/Controllers/
  Frontend/src/views/ | Frontend/src/views/admin/ | Frontend/src/store/ | Frontend/src/router/

VAI TRÒ CỦA BẠN:
Bạn là AI-4 trong nhóm 5 AI chạy song song. Bạn chuyên xử lý ADMIN, HỒ SƠ, QUẢN TRỊ HỆ THỐNG.

PHÂN VÙNG FILE CỦA BẠN (CHỈ ĐƯỢC SỬA CÁC FILE NÀY):
- Backend: UsersController.cs, AdminUsersController.cs, SystemSettingsController.cs, AuditLogsController.cs, SecurityController.cs
- Frontend: Profile.vue, UserManagement.vue, AuditLog.vue, views/admin/*
- Router: adminRoutes.js
- Store: useAdminUserStore.js

QUY TẮC:
1. KHÔNG ĐƯỢC SỬA file ngoài danh sách trên
2. LUÔN ĐỌC FILE TRƯỚC KHI SỬA
3. Tham khảo dự án Plane tại D:\A\plane để đối chiếu admin features
4. Sửa theo thứ tự: 🔴 → 🟠 → 🟡
5. Sau khi xong, tạo file docs/DONE_AI4.md
6. Nếu cần AI khác hỗ trợ, ghi vào docs/CROSS_DEPENDENCY.md

DANH SÁCH BUG & TASK CẦN LÀM:

🟠 HIGH:
4.1) Click Settings trong project → về Home thay vì Admin → Fix route guard trong adminRoutes.js, kiểm tra role redirect
4.2) Profile: không có logic, cần code toàn bộ → Gắn API GET/PUT profile trong Profile.vue + UsersController.cs, upload avatar, hiển thị tên

🟡 MEDIUM:
4.3) Profile: không kéo lên xuống được → Fix CSS overflow-y: auto trên container chính
4.4) Profile: bỏ nút "Chuyển tài khoản" → Xóa button switch account
4.5) Admin: đối chiếu Plane, bổ sung quản lý trạng thái mặc định (Backlog, Todo, In Progress, In Review, Done, Cancelled) trong admin/*
4.6) Admin: quản lý phòng ban + role theo seed_data → CRUD departments, assign role per project trong UserManagement.vue + AdminUsersController.cs
4.7) Admin: quản lý trạng thái project (CRUD status) → Tạo UI trong admin/*, API trong SystemSettingsController.cs
4.8) RBAC: đăng nhập role nào → chỉ thấy project được assign → Thêm middleware kiểm tra ProjectMember trong SecurityController.cs, filter project list

BẮT ĐẦU: Hãy đọc file Profile.vue và AdminUsersController.cs trước, sau đó sửa bug 4.1.
```

---

## 🤖 PROMPT AI-5: AI TÍCH HỢP, GAMIFICATION, TIMELINE/CALENDAR/VIEWS, HIỆU SUẤT

```
BỐI CẢNH DỰ ÁN:
Dự án QuanLyCongViec (SprintA) — Hệ thống quản lý công việc Agile/Scrum.
- Backend: ASP.NET Core 10, EF Core, SQL Server — chạy Port 5136
- Frontend: Vue 3 + Pinia + Element Plus — chạy Port 5173
- Cây thư mục:
  Backend/src/TaskManagement.API/Controllers/
  Frontend/src/views/ | Frontend/src/components/ | Frontend/src/store/ | Frontend/src/router/

VAI TRÒ CỦA BẠN:
Bạn là AI-5 trong nhóm 5 AI chạy song song. Bạn chuyên xử lý AI TÍCH HỢP, GAMIFICATION, TIMELINE/CALENDAR/VIEWS, HIỆU SUẤT.

PHÂN VÙNG FILE CỦA BẠN (CHỈ ĐƯỢC SỬA CÁC FILE NÀY):
- Backend: AiController.cs, GamificationController.cs, SprintsController.cs
- Frontend: AIPage.vue, RewardsView.vue, CyclesTab.vue, TimelineTab.vue, CalendarTab.vue, SpreadsheetTab.vue, KanbanBoard.vue, ListView.vue
- Store: useSprintStore.js
- Router: aiRoutes.js

QUY TẮC:
1. KHÔNG ĐƯỢC SỬA file ngoài danh sách trên
2. LUÔN ĐỌC FILE TRƯỚC KHI SỬA
3. Sửa theo thứ tự: 🔴 → 🟠 → 🟡
4. Sau khi xong, tạo file docs/DONE_AI5.md
5. Ngoài ra, tạo file docs/performance-optimization-report.md (phân tích hiệu suất)
6. Nếu cần AI khác hỗ trợ, ghi vào docs/CROSS_DEPENDENCY.md

DANH SÁCH BUG & TASK CẦN LÀM:

🟠 HIGH:
5.1) AI phân rã task báo lỗi 503 Gemini unavailable → Thêm retry logic 3 lần + fallback message trong AiController.cs, hiển thị tiến trình trong AIPage.vue
5.2) Nút gửi tin nhắn cho AI không hoạt động → Fix @click handler trên nút send, gắn v-model vào input trong AIPage.vue
5.5) Timeline: nút "New Work Item" không thêm được task → Gắn handler mở quick-add modal trong TimelineTab.vue

🟡 MEDIUM:
5.3) AI phải gửi lệnh phân rã vào chat box + hiển thị tiến trình thinking → Streaming response hoặc loading animation
5.4) AI đọc GitHub repo: chọn repo → phân tích → đề xuất task → Tích hợp GitHub API nếu có token
5.6) Timeline: click vào bar không có gì → Thêm @click mở task detail modal
5.7) Timeline: tab Tháng/Quý không hiện tiến độ → Tính lại dateScale cho monthly/quarterly
5.8) Calendar: hover vào ngày không hiện tooltip → Thêm hover tooltip hiển thị task trong ngày
5.9) Display options: các mục lọc không hoạt động → Gắn logic filter
5.10) Toggle Create Mode: kiểm tra Plane có không, nếu có thì bật = click vào board tạo task nhanh

GAMIFICATION (sau khi xong bugs):
5.G1) Thiết kế hệ thống điểm: Giá trị × Ảnh hưởng × Số ngày = Tổng điểm → GamificationController.cs + RewardsView.vue
5.G2) Chia điểm theo % đóng góp trong task/cycle/module
5.G3) Hoàn thành sớm → bonus 10%
5.G4) Vòng tròn % trên mỗi task card (hover mới thấy số) → KanbanBoard.vue, ListView.vue
5.G5) Level system: lên level càng chậm, đến mốc = thăng chức

HIỆU SUẤT (tạo file riêng docs/performance-optimization-report.md):
- Frontend: virtual list, lazy load, code splitting, cache, pagination
- Backend: pagination API, DTO, async/await, MemoryCache/Redis
- Database: index (ProjectId, Status, AssigneeId, CreatedAt), N+1 query
- Benchmark: 1000+ tasks, 100 concurrent users
- Đề xuất cải thiện cụ thể với code mẫu Vue + C# + SQL

BẮT ĐẦU: Hãy đọc file AiController.cs và AIPage.vue trước, sau đó sửa bug 5.1.
```
