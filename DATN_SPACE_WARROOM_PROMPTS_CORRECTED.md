# DATN SPACE WAR ROOM – BẢN PHÂN CÔNG ĐÃ SỬA ĐÚNG THEO NHÓM

> Bản này sửa lại đúng theo phân công của Tú.  
> Mục tiêu hôm nay: chạy được demo thật, không mock/hard-code/localStorage trong flow chính.  
> Cách làm: Research → Plan → Implement → Verify → Report.

---

## 0. BẢNG PHÂN CÔNG CHỐT LẠI

| Người | Phụ trách chính | Không phụ trách |
|---|---|---|
| Tú | Site/HomeSite + Rewards | Không ôm Work Items/Kanban/Cycles/Modules/Views/Pages |
| Phát | Recent, Starred, Your Work | Không làm Profile sidebar, không làm Work Items |
| Kiệt | Profile/current user/avatar trong Your Work và toàn app + Cycles | Không làm Recent/Starred/Rewards |
| Khôi | Dashboard, Modules, Reports | Không làm Your Work/Profile/Cycles |
| Quân | Work Items + Task Detail + Kanban/List/Calendar/Timeline/Spreadsheet | Không làm Cycles/Modules/Views/Pages |
| Đạt | Views + Pages | Không làm Work Items chính |

---

## 1. LUẬT LÀM VIỆC HÔM NAY

1. Không thêm package.
2. Không sửa module người khác.
3. Không tạo mock data mới.
4. Không hard-code user/avatar/date/status/số liệu.
5. Không dùng localStorage cho dữ liệu nghiệp vụ chính nếu đã có API.
6. UI nào có nhưng backend chưa có: hoặc sửa API cho chạy thật, hoặc ẩn/tạm khóa trước demo.
7. Mỗi người phải dán prompt vào Codex/Copilot Agent và bắt nó:
   - Research trước,
   - Plan trước,
   - chờ duyệt,
   - rồi mới sửa.
8. Sau khi sửa:
   - Frontend: `cd Frontend && npm run build`
   - Backend nếu có sửa: `dotnet build Backend/src/TaskManagement.API/TaskManagement.API.csproj`

---

# 2. TÚ – SITE/HOMESITE + REWARDS

## 2.1 Scope của Tú

Tú làm:
- Site/HomeSite:
  - Site Selection
  - Goals
  - Projects trong Site
  - Teams
  - People Directory
- Rewards page:
  - ví điểm,
  - cấp độ,
  - công thức tính điểm,
  - tổng kết sprint/cycle,
  - công việc tiêu biểu,
  - thành tích gần đây,
  - lịch sử điểm,
  - bảng xếp hạng.

Tú không làm:
- Recent,
- Starred,
- Your Work,
- Work Items,
- Kanban,
- Cycles,
- Modules,
- Views,
- Pages,
- Reports.

## 2.2 Cách Rewards phải vận hành

Rewards là trang gamification. Luồng đúng:

1. User mở Rewards.
2. Frontend gọi:
   - `GET /gamification/me`
   - `GET /gamification/leaderboard`
3. Trang hiển thị:
   - số dư điểm hiện tại,
   - level/career title,
   - progress đến level tiếp theo,
   - công thức tính điểm,
   - summary sprint/cycle,
   - spotlight tasks,
   - recent achievements,
   - transactions,
   - leaderboard.
4. Bấm refresh thì load lại API.
5. Nếu API lỗi thì hiện lỗi rõ, không crash.
6. Không được hiển thị điểm mock.

## 2.3 Done của Tú cho Rewards

Rewards được tính là xong khi:

- Load được API thật.
- Không còn dữ liệu giả trong cards chính.
- Nếu chưa có task/achievement thì empty state rõ.
- Leaderboard lấy DB thật.
- Refresh chạy.
- Build pass.

## 2.4 Prompt Codex cho Tú

```text
Bạn phụ trách Site/HomeSite và Rewards trong dự án DATN.

PHẠM VI ĐƯỢC SỬA:
- Frontend/src/views/HomeSite/**
- Frontend/src/store/useGoalStore.js
- Frontend/src/store/useHomeProjectStore.js
- Frontend/src/store/useTeamStore.js
- Frontend/src/store/usePeopleStore.js
- Frontend/src/views/RewardsView.vue
- Backend/src/TaskManagement.API/Controllers/GamificationController.cs nếu cần sửa nhỏ
- Backend service/entity liên quan Gamification nếu cần

KHÔNG ĐƯỢC SỬA:
- Không sửa Recent/Starred/Your Work của Phát.
- Không sửa Profile/Cycles của Kiệt.
- Không sửa Work Items/Kanban của Quân.
- Không sửa Modules/Dashboard/Reports của Khôi.
- Không sửa Views/Pages của Đạt.
- Không thêm package.

NHIỆM VỤ RESEARCH:
1. Đọc RewardsView.vue.
2. Đọc GamificationController.cs.
3. Kiểm tra API /gamification/me và /gamification/leaderboard trả những field nào.
4. Liệt kê field UI đang cần:
   - wallet.totalPoints
   - career.level
   - career.title
   - career.progressPercent
   - formula
   - summary
   - spotlightTasks
   - recentAchievements
   - transactions
   - leaderboard
5. Liệt kê field nào thiếu backend, field nào đang fallback default.

MỤC TIÊU P0:
1. Rewards gọi API thật.
2. Refresh button chạy.
3. Nếu API không có dữ liệu thì hiển thị empty state chuyên nghiệp, không mock.
4. Nếu API thiếu field thì hiện 0 hoặc “—” hợp lý, không hard-code điểm giả.
5. Nếu backend thiếu endpoint nhỏ thì bổ sung.
6. Không làm gamification quá rộng; chỉ làm đủ demo.

VERIFY:
- Login user thật.
- Mở Rewards.
- Gọi /gamification/me thành công.
- Gọi /gamification/leaderboard thành công.
- Refresh không crash.
- Nếu chưa có điểm thì trang vẫn hiển thị empty state rõ.
- cd Frontend && npm run build
- Nếu sửa backend: dotnet build Backend/src/TaskManagement.API/TaskManagement.API.csproj

BÁO CÁO:
- File đã đọc.
- File đã sửa.
- Field nào đã lấy từ API.
- Field nào backend còn thiếu.
- Build pass/fail.
```

---

# 3. PHÁT – RECENT / STARRED / YOUR WORK

## 3.1 Scope của Phát

Phát làm 3 trang/mục:
- Recent
- Starred
- Your Work

Phát không làm:
- Profile sidebar bên phải trong Your Work,
- Rewards,
- Work Items,
- Kanban,
- Cycles.

## 3.2 Luồng vận hành đúng

### Recent

Mục đích: lưu và hiển thị những task/project user vừa mở.

Luồng:
1. User mở task hoặc project.
2. Hệ thống ghi nhận item đó là recent.
3. User bấm Recent.
4. Dropdown hiện recent items theo Today / Yesterday / Older.
5. Search lọc recent.
6. Click item chuyển về đúng `/space/{projectId}/work-items`.
7. Reload vẫn còn nếu có backend. Nếu chưa có backend thì localStorage chỉ được coi là temporary, phải ghi rõ.

### Starred

Mục đích: user đánh dấu task/project quan trọng.

Luồng:
1. User star task/project.
2. Item xuất hiện trong Starred.
3. F5/reload vẫn còn.
4. User unstar.
5. Item biến mất và DB cập nhật.

### Your Work

Mục đích: dashboard công việc cá nhân.

Luồng:
1. User vào Your Work.
2. Summary hiển thị số task:
   - Created by me,
   - Assigned to me,
   - Subscribed by me,
   - Starred.
3. Workload chia theo trạng thái:
   - Backlog,
   - Not started,
   - Working on,
   - Completed,
   - Canceled.
4. Chart priority/state lấy từ task thật.
5. Tab Assigned/Created/Subscribed/Starred hiển thị đúng.
6. Activity lấy dữ liệu thật hoặc ẩn/tạm khóa nếu chưa có backend.
7. Đổi status/priority nếu UI cho phép thì phải lưu DB.

## 3.3 Done của Phát

Phát xong khi:

- Recent không crash.
- Starred task/project reload vẫn còn.
- Your Work summary đúng theo task DB.
- Assigned/Created/Subscribed lọc đúng current user.
- Activity không tự bịa.
- Không còn dùng localStorage làm nguồn dữ liệu chính nếu có API.
- Build pass.

## 3.4 Prompt Codex cho Phát

```text
Bạn phụ trách Recent, Starred và Your Work trong Space của dự án DATN.

PHẠM VI ĐƯỢC SỬA:
- Frontend/src/views/YourWorkView.vue
- Frontend/src/components/RecentDropdown.vue
- Frontend/src/components/StarredDropdown.vue
- Frontend/src/components/layout/NexusSidebar.vue nếu cần
- Frontend/src/store/useWorkTaskStore.js
- Frontend/src/store/useProjectStore.js
- Frontend/src/store/useActivityStore.js
- Backend/src/TaskManagement.API/Controllers/WorkTasksController.cs nếu cần sửa endpoint nhỏ
- Backend/src/TaskManagement.API/Controllers/StarredItemsController.cs nếu cần dùng API starred

KHÔNG ĐƯỢC SỬA:
- Không sửa profile sidebar bên phải trong Your Work, phần này Kiệt làm.
- Không sửa Rewards, phần này Tú làm.
- Không sửa Work Items/Kanban, phần này Quân làm.
- Không sửa Cycles, phần này Kiệt làm.
- Không thêm package.

NHIỆM VỤ RESEARCH:
1. Đọc YourWorkView.vue.
2. Đọc RecentDropdown.vue.
3. Đọc StarredDropdown.vue.
4. Đọc useWorkTaskStore.js.
5. Đọc useProjectStore.js.
6. Đọc useActivityStore.js.
7. Đọc WorkTasksController.cs.
8. Đọc StarredItemsController.cs.
9. Liệt kê:
   - UI nào đã có,
   - API nào đang gọi,
   - phần nào đang dùng localStorage,
   - phần nào hard-code,
   - endpoint nào thiếu.

MỤC TIÊU P0:
1. Your Work Summary dùng task thật.
2. Assigned tab lọc task của current user.
3. Created tab lọc task user tạo.
4. Subscribed tab dùng field/API thật; nếu backend chưa có thì ẩn/tạm khóa.
5. Starred tab dùng StarredItems API nếu có, không dùng localStorage làm nguồn chính.
6. Recent dropdown hoạt động; nếu chưa có recent backend thì ghi rõ temporary.
7. Activity nếu chưa có backend thì không export dữ liệu giả.
8. Đổi status/priority trong Your Work phải gọi đúng endpoint backend.

VERIFY:
- Login user thật.
- Tạo task / nhận task.
- Mở Your Work.
- Kiểm Summary, Assigned, Created.
- Star task/project.
- F5.
- Mở Starred kiểm item còn.
- Mở Recent kiểm task vừa xem.
- cd Frontend && npm run build
- Nếu sửa backend: dotnet build Backend/src/TaskManagement.API/TaskManagement.API.csproj

BÁO CÁO:
- File đã sửa.
- LocalStorage nào đã bỏ.
- Phần nào còn temporary.
- API còn thiếu.
- Build pass/fail.
```

---

# 4. KIỆT – PROFILE TRONG YOUR WORK + CYCLES

## 4.1 Scope của Kiệt

Kiệt làm:
- Profile/current user/avatar trong Your Work và toàn app.
- Cycles logic sprint-style.

Kiệt không làm:
- Recent/Starred,
- Rewards,
- Work Items core,
- Modules/Reports,
- Views/Pages.

## 4.2 Profile đúng phải vận hành

Panel bên phải trong Your Work hiện phải là thông tin user thật.

Luồng:
1. User login.
2. App hydrate current user từ API.
3. Your Work lấy current user.
4. Sidebar hiển thị:
   - avatarUrl hoặc initials,
   - fullName,
   - email/username/handle,
   - joinedAt nếu backend có,
   - timezone nếu backend có,
   - workspace/project nếu có.
5. Nếu field chưa có API thì hiện `—`, không hard-code.

## 4.3 Cycles đúng phải vận hành như Sprint

Cycle = vòng đời/sprint của project.

Luồng:
1. Vào `/space/{projectId}/cycles`.
2. Bấm Add cycle.
3. Nhập name, description, start date, end date.
4. Cycle mới nằm Upcoming nếu chưa start.
5. Bấm Start cycle.
6. Cycle chuyển Active.
7. Vào cycle board.
8. Tạo task hoặc gán task vào cycle.
9. Task có `sprintId/cycleId`.
10. Work Items có thể filter theo cycle.
11. Close cycle:
    - task done giữ hoàn thành,
    - task chưa xong carry-over sang backlog/cycle khác nếu kịp.
12. Nếu close/carry-over chưa ổn thì ẩn nút, không để lỗi.

## 4.4 Done của Kiệt

Kiệt xong khi:

- Your Work profile không còn Alo/A/Cun hard-code.
- F5 vẫn đúng user.
- Tạo cycle được.
- Start cycle được.
- Vào cycle board được.
- Gán/tạo task trong cycle được.
- Reload vẫn còn.
- Close/carry-over nếu chưa xong thì ẩn.
- Build pass.

## 4.5 Prompt Codex cho Kiệt

```text
Bạn phụ trách Profile trong Your Work và Cycles trong Space.

PHẠM VI ĐƯỢC SỬA:
- Frontend/src/views/YourWorkView.vue
- Frontend/src/views/Profile.vue nếu cần tham khảo/helper
- Frontend/src/utils/authSession.js
- Frontend/src/api/axiosClient.js
- Frontend/src/views/CyclesView.vue
- Frontend/src/components/CyclesTab.vue
- Frontend/src/store/useSprintStore.js
- Frontend/src/store/useWorkTaskStore.js nếu cần gán task vào cycle
- Backend/src/TaskManagement.API/Controllers/UsersController.cs nếu thiếu current user field nhỏ
- Backend/src/TaskManagement.API/Controllers/SprintsController.cs nếu endpoint cycle thiếu nhỏ
- Backend/src/TaskManagement.API/Controllers/WorkTasksController.cs nếu cần gán sprintId/cycleId cho task

KHÔNG ĐƯỢC SỬA:
- Không sửa Recent/Starred của Phát.
- Không sửa Rewards của Tú.
- Không sửa Work Items/Kanban chính của Quân nếu không cần.
- Không sửa Modules/Reports của Khôi.
- Không sửa Views/Pages của Đạt.
- Không thêm package.

NHIỆM VỤ RESEARCH:
1. Đọc YourWorkView.vue và tìm mọi hard-code trong profile sidebar.
2. Đọc authSession.js để biết current user lưu/hydrate thế nào.
3. Đọc UsersController.cs để biết API user trả gì.
4. Đọc CyclesView.vue.
5. Đọc CyclesTab.vue.
6. Đọc useSprintStore.js.
7. Đọc SprintsController.cs.
8. Đọc WorkTasksController.cs phần update task sprint/cycle.
9. Liệt kê:
   - field profile cần,
   - field backend có,
   - cycle action nào đã có API,
   - action nào chưa chắc chạy.

MỤC TIÊU P0 PROFILE:
1. Bỏ hard-code Alo/A/Cun/joined date/timezone.
2. Dùng current user thật.
3. Avatar lấy avatarUrl nếu có, không có thì initials.
4. Field thiếu thì hiện “—”.
5. F5 không mất thông tin.

MỤC TIÊU P0 CYCLES:
1. List cycle theo project thật.
2. Add cycle tạo DB.
3. Start cycle gọi API thật.
4. Vào cycle board đúng route.
5. Tạo/gán task vào cycle.
6. Reload vẫn giữ cycle và task trong cycle.
7. Close/carry-over nếu chưa chắc thì ẩn/tạm khóa.

VERIFY:
- Login user thật.
- Vào Your Work, profile đúng.
- F5, profile vẫn đúng.
- Vào /space/{projectId}/cycles.
- Add cycle.
- Start cycle.
- Vào cycle.
- Gán hoặc tạo task trong cycle.
- F5 kiểm tra.
- cd Frontend && npm run build
- Nếu sửa backend: dotnet build Backend/src/TaskManagement.API/TaskManagement.API.csproj

BÁO CÁO:
- Profile lấy field nào từ API.
- Cycle action nào chạy.
- Close/carry-over đã làm hay đã ẩn.
- Build pass/fail.
```

---

# 5. KHÔI – DASHBOARD / MODULES / REPORTS

## 5.1 Scope của Khôi

Khôi làm:
- Dashboard
- Modules
- Reports

Khôi không làm:
- Your Work,
- Profile,
- Rewards,
- Cycles,
- Work Items core,
- Views/Pages.

## 5.2 Dashboard đúng phải vận hành

Dashboard là tổng quan project.

Luồng:
1. Vào `/space/{projectId}/dashboard`.
2. Gọi task thật của project.
3. Tính:
   - Open Tasks,
   - Completed,
   - In Progress,
   - Blocked nếu có field.
4. Recent Tasks = task updated mới nhất.
5. Team Workload = group theo assignee.
6. Không còn số hard-code 12/45/3/2.
7. Nếu thiếu field blocked/workload thì empty state rõ, không fake.

## 5.3 Modules đúng phải vận hành

Module = phân hệ/chức năng của project.

Luồng:
1. Vào `/space/{projectId}/modules`.
2. Add Module.
3. Nhập:
   - name,
   - description,
   - status,
   - lead,
   - taskIds,
   - start/end date.
4. Module xuất hiện.
5. Search/sort/filter status chạy.
6. Đổi status inline lưu DB.
7. Đổi date range lưu DB.
8. Edit module lưu DB.
9. Disable module.
10. Restore module.
11. Open module chuyển qua Work Items với query moduleId.
12. Reload vẫn còn.

## 5.4 Reports đúng phải vận hành

Reports = báo cáo project.

Luồng:
1. Vào `/space/{projectId}/reports`.
2. Hiển thị report từ task thật:
   - total tasks,
   - done,
   - in progress,
   - overdue nếu có dueDate,
   - by assignee,
   - by status.
3. Nếu export chưa có backend thì ẩn nút export.
4. Không hard-code chart/số liệu.

## 5.5 Done của Khôi

Khôi xong khi:

- Dashboard hết hard-code.
- Modules CRUD/status/date/disable/restore chạy.
- Module favorite nếu chưa có backend thì ẩn.
- Reports không có số giả/export giả.
- Build pass.

## 5.6 Prompt Codex cho Khôi

```text
Bạn phụ trách Dashboard, Modules và Reports trong Space.

PHẠM VI ĐƯỢC SỬA:
- Frontend/src/views/SpaceDashboard.vue
- Frontend/src/views/ModulesView.vue
- Frontend/src/components/ModulesTab.vue
- Frontend/src/views/ReportsView.vue
- Backend/src/TaskManagement.API/Controllers/ModulesController.cs nếu cần sửa nhỏ
- Backend/src/TaskManagement.API/Controllers/WorkTasksController.cs nếu cần thống kê task
- Backend/src/TaskManagement.API/Controllers/ProjectManagementController.cs nếu có endpoint dashboard/report

KHÔNG ĐƯỢC SỬA:
- Không sửa Your Work của Phát.
- Không sửa Profile/Cycles của Kiệt.
- Không sửa Rewards của Tú.
- Không sửa Work Items/Kanban của Quân.
- Không sửa Views/Pages của Đạt.
- Không thêm package.

NHIỆM VỤ RESEARCH:
1. Đọc SpaceDashboard.vue.
2. Đọc ModulesView.vue.
3. Đọc ModulesTab.vue.
4. Đọc ReportsView.vue.
5. Đọc ModulesController.cs.
6. Đọc WorkTasksController.cs endpoint lấy task theo project.
7. Liệt kê:
   - dashboard field nào hard-code,
   - module action nào đã có API,
   - module action nào local-only,
   - reports widget nào fake.

MỤC TIÊU P0 DASHBOARD:
1. Bỏ số hard-code.
2. Lấy task thật theo project.
3. Tính open/completed/inProgress/blocked.
4. Recent Tasks lấy task updated mới nhất.
5. Team Workload group theo assignee nếu có.
6. Nếu thiếu dữ liệu thì empty state thật.

MỤC TIÊU P0 MODULES:
1. List modules theo project.
2. Add module.
3. Edit module.
4. Search/sort/filter.
5. Update status.
6. Update date range.
7. Assign taskIds.
8. Disable/restore.
9. Open module task view.
10. Ẩn favorite module nếu chưa có backend.

MỤC TIÊU P0 REPORTS:
1. Không để export giả.
2. Nếu chưa có report API thì tính report cơ bản từ task thật.
3. Không hard-code chart/số liệu.

VERIFY:
- Vào Dashboard.
- Tạo task rồi xem dashboard đổi số.
- Vào Modules.
- Add module.
- Gán task vào module.
- Đổi status/date.
- Reload.
- Disable/restore.
- Vào Reports, không có dữ liệu giả.
- cd Frontend && npm run build
- Nếu sửa backend: dotnet build Backend/src/TaskManagement.API/TaskManagement.API.csproj

BÁO CÁO:
- Dashboard còn field nào thiếu.
- Modules action nào chạy.
- Reports action nào đã ẩn.
- Build pass/fail.
```

---

# 6. QUÂN – WORK ITEMS / TASK DETAIL / KANBAN

## 6.1 Scope của Quân

Quân làm:
- Work Items
- Task Detail
- Kanban
- List
- Calendar
- Timeline
- Spreadsheet
- FilterBar
- Add work item

Quân không làm:
- Cycles,
- Modules,
- Views,
- Pages,
- Dashboard,
- Your Work.

## 6.2 Work Items đúng phải vận hành

Luồng:
1. Vào `/space/{projectId}/work-items`.
2. Load task thật theo project.
3. Board/list có trạng thái:
   - Backlog,
   - To Do,
   - In Progress,
   - In Review,
   - Done,
   - Cancelled.
4. Bấm Add work item.
5. Tạo task DB.
6. Task hiện đúng cột/list.
7. Đổi status.
8. Đổi priority.
9. Gán assignee.
10. Gán label/module/cycle nếu có UI và API.
11. Mở Task Detail.
12. Sửa title/description/date/status/priority/assignee.
13. Comment/upload nếu kịp.
14. Kéo Kanban.
15. Reload vẫn đúng.

## 6.3 Done của Quân

Quân xong khi:

- Add task lưu DB.
- Kanban drag/drop lưu DB.
- Status/priority/assignee lưu DB.
- Task detail sửa field chính.
- Reload vẫn đúng.
- Calendar/Timeline/Spreadsheet không crash.
- Build pass.

## 6.4 Prompt Codex cho Quân

```text
Bạn phụ trách Work Items, Task Detail và Kanban trong Space.

PHẠM VI ĐƯỢC SỬA:
- Frontend/src/views/SpaceSummary.vue
- Frontend/src/components/TaskDetailModal.vue
- Frontend/src/components/KanbanBoard.vue
- Frontend/src/components/ListView.vue
- Frontend/src/components/CalendarTab.vue
- Frontend/src/components/TimelineTab.vue
- Frontend/src/components/SpreadsheetTab.vue
- Frontend/src/components/FilterBar.vue
- Frontend/src/store/useWorkTaskStore.js
- Frontend/src/api/signalrService.js
- Backend/src/TaskManagement.API/Controllers/WorkTasksController.cs nếu cần sửa endpoint nhỏ
- Backend/src/TaskManagement.API/Controllers/CommentsController.cs nếu cần comment task
- Backend/src/TaskManagement.API/Controllers/TaskDependenciesController.cs nếu cần dependency
- Backend/src/TaskManagement.API/Hubs/KanbanHub.cs nếu cần realtime

KHÔNG ĐƯỢC SỬA:
- Không sửa Cycles của Kiệt.
- Không sửa Modules/Dashboard/Reports của Khôi.
- Không sửa Recent/Starred/Your Work của Phát.
- Không sửa Views/Pages của Đạt.
- Không sửa Rewards của Tú.
- Không thêm package.

NHIỆM VỤ RESEARCH:
1. Đọc SpaceSummary.vue.
2. Đọc TaskDetailModal.vue.
3. Đọc KanbanBoard.vue.
4. Đọc ListView.vue.
5. Đọc useWorkTaskStore.js.
6. Đọc WorkTasksController.cs.
7. Đọc KanbanHub.cs.
8. Liệt kê:
   - endpoint nào frontend gọi,
   - backend endpoint nào có,
   - field nào mismatch,
   - update nào thiếu RowVersion,
   - chức năng nào local-only.

MỤC TIÊU P0:
1. Load task thật theo project.
2. Add work item tạo DB.
3. List view đổi status/priority/assignee lưu DB.
4. Kanban drag/drop đổi status lưu DB.
5. TaskDetailModal sửa:
   - title,
   - description,
   - status,
   - priority,
   - assignee,
   - dates,
   - module nếu API ổn,
   - cycle nếu API ổn,
   - label nếu API ổn.
6. Comment task nếu backend có.
7. Upload task nếu chưa chắc thì ẩn.
8. RowVersion conflict không crash.

VERIFY:
- Vào Work Items.
- Add task.
- F5 task vẫn còn.
- Kéo task qua In Progress.
- F5 vẫn đúng.
- Mở Task Detail sửa field.
- F5 vẫn đúng.
- Chuyển List/Kanban/Calendar/Timeline/Spreadsheet không lỗi.
- cd Frontend && npm run build
- Nếu sửa backend: dotnet build Backend/src/TaskManagement.API/TaskManagement.API.csproj

BÁO CÁO:
- Field nào đã lưu DB.
- View nào chỉ read-only.
- Chức năng nào đã ẩn.
- Build pass/fail.
```

---

# 7. ĐẠT – VIEWS / PAGES

## 7.1 Scope của Đạt

Đạt làm:
- Views
- Pages

Đạt không làm:
- Work Items chính,
- Cycles,
- Modules,
- Dashboard,
- Your Work,
- Rewards.

## 7.2 Views đúng phải vận hành

Views = saved filter/view cho task.

Luồng:
1. Vào `/space/{projectId}/views`.
2. Empty nếu chưa có view.
3. Bấm Add view.
4. Nhập title/description.
5. Chọn filter cơ bản:
   - status,
   - assignee,
   - priority,
   - label,
   - cycle,
   - module,
   - date.
6. Chọn display properties.
7. Lưu view.
8. Reload vẫn còn.
9. Click view.
10. View load task theo metadata.
11. Favorite/unfavorite.
12. Delete.

Nếu filter phức tạp chưa kịp:
- Hỗ trợ tối thiểu status/priority/projectId.
- Các filter chưa áp dụng thật thì ẩn/tạm khóa trong demo.

## 7.3 Pages đúng phải vận hành

Pages = tài liệu/wiki/note trong project.

Luồng:
1. Vào `/space/{projectId}/pages`.
2. Add page.
3. Page lưu DB.
4. Mở page.
5. Sửa title/content.
6. Autosave.
7. Reload vẫn còn.
8. Lock/unlock.
9. Public/private.
10. Star.
11. Archive/restore.
12. Delete.
13. Copy page.
14. Copy link.

## 7.4 Done của Đạt

Đạt xong khi:

- Views CRUD/favorite/delete chạy.
- Click view ra task đúng filter cơ bản.
- Pages CRUD/autosave/lock/private/star/archive/copy/delete chạy.
- Không còn hard-code `CYBWF`, `P`, `D`, `You` trong flow chính.
- Không fallback projectId sai gây tạo default project bừa.
- Build pass.

## 7.5 Prompt Codex cho Đạt

```text
Bạn phụ trách Views và Pages trong Space.

PHẠM VI ĐƯỢC SỬA:
- Frontend/src/views/ViewsView.vue
- Frontend/src/components/ViewsTab.vue
- Frontend/src/views/PagesView.vue
- Frontend/src/components/PagesTab.vue
- Frontend/src/components/ListView.vue nếu cần hiển thị task trong view
- Backend/src/TaskManagement.API/Controllers/ProjectViewsController.cs nếu cần sửa nhỏ
- Backend/src/TaskManagement.API/Controllers/PagesController.cs nếu cần sửa nhỏ
- Backend/src/TaskManagement.API/Controllers/WorkTasksController.cs nếu cần search task cho view

KHÔNG ĐƯỢC SỬA:
- Không sửa Work Items/Kanban của Quân.
- Không sửa Cycles của Kiệt.
- Không sửa Modules/Dashboard/Reports của Khôi.
- Không sửa Recent/Starred/Your Work của Phát.
- Không sửa Rewards của Tú.
- Không thêm package.

NHIỆM VỤ RESEARCH:
1. Đọc ViewsTab.vue.
2. Đọc PagesTab.vue.
3. Đọc ProjectViewsController.cs.
4. Đọc PagesController.cs.
5. Đọc WorkTasksController.cs phần /tasks/search.
6. Liệt kê:
   - Views action nào chạy API thật,
   - Views filter nào chỉ UI,
   - Pages action nào chạy API thật,
   - Pages chỗ nào hard-code,
   - Pages fallback projectId nào nguy hiểm.

MỤC TIÊU P0 VIEWS:
1. List views theo project thật.
2. Add view lưu DB.
3. Favorite view lưu DB.
4. Delete view xóa DB.
5. Click view load task theo metadata.
6. Hỗ trợ filter cơ bản status/priority/projectId.
7. Filter chưa làm thật thì ẩn/tạm khóa.

MỤC TIÊU P0 PAGES:
1. List pages theo project thật.
2. Create page lưu DB.
3. Open page lấy content DB.
4. Autosave title/content.
5. Lock/unlock.
6. Private/public.
7. Star.
8. Archive/restore.
9. Delete.
10. Copy page.
11. Bỏ hard-code CYBWF/P/D/You.

VERIFY:
- Vào Views.
- Add view.
- Reload.
- Favorite view.
- Click view thấy task.
- Delete view.
- Vào Pages.
- Add page.
- Sửa content.
- Reload.
- Lock/private/star/archive/copy/delete.
- cd Frontend && npm run build
- Nếu sửa backend: dotnet build Backend/src/TaskManagement.API/TaskManagement.API.csproj

BÁO CÁO:
- Views filter nào làm thật.
- Pages action nào chạy.
- Hard-code nào đã bỏ.
- Build pass/fail.
```

---

# 8. SCRIPT DEMO CUỐI NGÀY

1. Login.
2. F5 vẫn giữ session.
3. Vào Site/HomeSite: Tú demo nhanh.
4. Vào Rewards: Tú demo load điểm/leaderboard.
5. Vào Your Work:
   - Phát demo Summary/Assigned/Created/Starred.
   - Kiệt demo profile đúng user.
6. Vào Recent/Starred:
   - Phát demo recent/starred reload vẫn còn.
7. Vào Dashboard:
   - Khôi demo số liệu thật.
8. Vào Work Items:
   - Quân tạo task,
   - kéo Kanban,
   - sửa task detail,
   - reload.
9. Vào Cycles:
   - Kiệt tạo cycle,
   - start cycle,
   - gán task vào cycle,
   - reload.
10. Vào Modules:
   - Khôi tạo module,
   - gán task,
   - đổi status/date,
   - reload.
11. Vào Reports:
   - Khôi mở report không lỗi, không export giả.
12. Vào Views:
   - Đạt tạo view,
   - click view,
   - favorite/delete.
13. Vào Pages:
   - Đạt tạo page,
   - sửa content,
   - lock/private/star/archive/copy/delete.
14. Build:
   - `npm run build`
   - `dotnet build Backend/src/TaskManagement.API/TaskManagement.API.csproj` nếu có sửa backend.

---

# 9. TIÊU CHUẨN “HOÀN THÀNH” CHUYÊN NGHIỆP

Một chức năng chỉ được gọi là hoàn thành khi:

- Có UI.
- Có API/backend thật hoặc có quyết định ẩn/tạm khóa rõ ràng.
- Dữ liệu lưu DB.
- Reload/F5 vẫn còn.
- Không hard-code.
- Không mock.
- Không localStorage cho dữ liệu nghiệp vụ chính.
- Có loading/error state.
- Không lỗi console.
- Build pass.
- Người phụ trách demo được chính flow đó.

Nếu thiếu một trong các tiêu chí trên thì không gọi là “xong”, chỉ gọi là “UI đang có, chưa hoàn thiện”.
