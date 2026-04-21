# Master Integration Handoff

Date: 2026-04-21

Purpose:
- This file is the single handoff brief for one AI with full repo authority to finish final integration, cleanup, verification, and any cross-scope fixes in one pass.

## Current Overall Status

- AI-1, AI-2, AI-3, AI-4, AI-5 have all completed meaningful scoped work and produced handoff notes.
- Frontend production build currently passes.
- AI-4 scope is functionally done.
- AI-2 scoped feature work is functionally done inside assigned files.
- Remaining work is now mostly integration, cross-scope consistency, and final verification.

## What Is Already Done

- AI-4 completed:
  - admin route redirect
  - profile GET/PUT/avatar/scroll
  - departments CRUD
  - project-role mapping for departments
  - default task status admin UI/API
  - project status admin UI/API
  - admin-side project filtering via `/api/security/accessible-projects`
  - admin/settings/security role protection in AI-4 controllers

- AI-5 resolved prior frontend blockers:
  - `Frontend/src/router/aiRoutes.js`
  - `Frontend/src/views/AIPage.vue`
  - `Frontend/src/components/CalendarTab.vue`

- AI-2 completed inside scope:
  - drafts pagination and payload normalization
  - drafts virtualized rendering and date-picker usability fixes
  - modules pagination, lazy loading, search/sort/filter controls
  - module task-to-module sync via `IssueModules`
  - module date update flow calling API immediately
  - labels API normalization and assignment route compatibility
  - filter builder teleport/fixed overlay behavior

## Remaining Cross-Scope Gaps

### 1. Global RBAC consistency is not fully finished

AI-4 added:
- `GET /api/security/accessible-projects`

But this is only fully consumed in AI-4-owned admin pages.

Still needs end-to-end alignment in general project discovery/listing flows outside AI-4 scope, especially:
- `Frontend/src/store/useProjectStore.js`
- non-admin pages still using `/projects` or `/projects/discovery`
- any backend project-list endpoints that should enforce the same visibility rule globally

### 2. Backend build verification is still unresolved

Observed status:
- `dotnet build Backend/src/TaskManagement.API/TaskManagement.API.csproj`
- has had inconsistent results across handoffs
- AI-2 reported a successful backend build for its changed scope
- earlier notes from other AIs reported exit code `1` without a concrete compile error

The full-permission integration AI should rerun backend build from the current repo head and treat the latest repo state as source of truth.
If it still fails, investigate as a repo-level restore/project-graph/build-environment issue rather than assuming a single-scope syntax failure.

### 3. AI-2 drafts integration is not fully finished end-to-end

Related files:
- `Backend/src/TaskManagement.API/Controllers/DraftsController.cs`
- `Frontend/src/views/DraftsView.vue`
- data layer / migration files outside AI-2 scope

Current status:
- Done:
  - `DraftsView.vue` sends `projectId` with drafts fetch requests
  - `DraftsController.cs` paginates drafts and returns normalized payload fields
  - `DraftsController.cs` can store `projectId` inside draft payload
  - `DraftsController.cs` now narrows results by requested/current `projectId`
- Not done:
  - supporting DB index for drafts listing query is still missing

Concrete risk:
- Pagination reduces payload size, but query scalability is still incomplete without the database index.

Suspected cause:
- `projectId` is currently stored in draft payload JSON rather than as a first-class database column.
- AI-2 was not allowed to edit entity/configuration/migration files.

Recommended fix path for full-permission integration AI:
1. Better structural fix:
   - add real `ProjectId` column to `TaskDraft`
   - backfill/migrate if needed
   - query directly by `(UserId, ProjectId, UpdatedAt)`
2. Performance fix:
   - add index for drafts listing, preferably:
     - `TaskDrafts(UserId, UpdatedAt)`
   - or if `ProjectId` becomes a real column:
     - `TaskDrafts(UserId, ProjectId, UpdatedAt)`

### 4. AI-2 module progress logic still needs integration-level verification

Related files:
- `Backend/src/TaskManagement.API/Controllers/ModulesController.cs`
- any future test or verification harness outside AI-2 scope

Current status:
- Done:
  - progress formula was changed to requested behavior:
    - `DONE tasks / total tasks * 100`
- Not done:
  - no dedicated automated verification was added by AI-2

Concrete risk:
- Logic may still need validation against real status variants:
  - `DONE`
  - `Done`
  - `Complete`
- Edge cases like zero tasks and mixed status sets should be verified at integration time.

Recommended fix path:
- add or run targeted verification for:
  - no tasks
  - mixed statuses
  - status-name casing variants

### 5. AI-5 gamification engine is not fully aligned with UI/controller contract

UI/controller work exists, but the core engine is still outside AI-5 scope:
- `Backend/src/TaskManagement.Infrastructure/Services/GamificationService.cs`

If the product rule must exactly match:
- `Gia tri x Anh huong x So ngay`
- early completion bonus `10%`
- contribution-based split

then the service layer needs to be aligned.

### 6. Timeline quick-add still depends on global create flow

Current state:
- `TimelineTab.vue` dispatches `global-create-task`

If product wants exact bucket dates to prefill create modal consistently, the integration owner should verify and possibly align:
- `Frontend/src/views/SpaceSummary.vue`
- task creation modal/global create flow

### 7. Sidebar / project tree may still need final integration

AI-3 prepared store/data side, but final visible navigation may still require integration in layout-owned files:
- `Frontend/src/components/layout/NexusSidebar.vue`
- `Frontend/src/components/layout/NexusTopbar.vue`

## Files To Read First

The full-permission AI should read these first:
- `docs/CROSS_DEPENDENCY.md`
- `docs/DONE_AI1.md`
- `docs/DONE_AI2.md`
- `docs/DONE_AI3.md`
- `docs/DONE_AI4.md`
- `docs/DONE_AI5.md`
- `docs/CODEX_5AI_TASK_DELEGATION.md`

## Prompt For One Full-Permission AI

Copy and send this:

```text
Bạn là AI tích hợp cuối cùng, có toàn quyền trong toàn repo `D:\\A\\QuanLyCongViec`.

Hãy đọc trước các file:
- docs/MASTER_INTEGRATION_HANDOFF.md
- docs/CROSS_DEPENDENCY.md
- docs/DONE_AI1.md
- docs/DONE_AI2.md
- docs/DONE_AI3.md
- docs/DONE_AI4.md
- docs/DONE_AI5.md
- docs/CODEX_5AI_TASK_DELEGATION.md

Mục tiêu:
1. Hoàn tất mọi integration còn thiếu giữa các phần của 5 AI
2. Đồng bộ RBAC project visibility cho toàn app, không chỉ admin pages
3. Điều tra và sửa nguyên nhân backend `dotnet build Backend/src/TaskManagement.API/TaskManagement.API.csproj` đang exit code 1
4. Kiểm tra lại gamification engine nếu cần để khớp contract UI/controller
5. Kiểm tra luồng timeline quick-add và sidebar/project tree nếu còn lệch
6. Chạy verify cuối:
   - frontend build
   - backend build
   - ghi rõ cái gì pass/cái gì fail

Yêu cầu:
- Đọc file trước khi sửa
- Không dừng ở phân tích, hãy sửa trực tiếp các vấn đề còn lại
- Nếu gặp xung đột giữa các AI, ưu tiên trạng thái hiện tại trong repo và các file DONE/CROSS_DEPENDENCY
- Khi xong, cập nhật:
  - docs/MASTER_FINAL_REPORT.md
  - docs/CROSS_DEPENDENCY.md nếu vẫn còn tồn đọng

Đầu ra cần có:
- danh sách file đã sửa
- các blocker đã xử lý
- các blocker còn lại nếu có
- kết quả build frontend/backend
```

## Prompt To Send Other AIs

Copy and send this to any AI còn đang giữ phần việc riêng:

```text
Hãy đọc docs/MASTER_INTEGRATION_HANDOFF.md, docs/CROSS_DEPENDENCY.md và file DONE của bạn.

Nếu phần của bạn còn blocker, thiếu sót, integration note, hoặc có chỗ nào cần AI tích hợp cuối xử lý ngoài scope của bạn, hãy ghi thật rõ vào:
- docs/CROSS_DEPENDENCY.md

Ghi theo format:
- file liên quan
- vấn đề cụ thể
- nguyên nhân nghi ngờ
- hướng xử lý đề xuất

Nếu phần của bạn đã ổn, hãy ghi ngắn gọn rằng scope của bạn đã done và không còn blocker mới.

Không sửa ngoài phạm vi của bạn, chỉ cập nhật tài liệu handoff cần thiết cho AI tích hợp cuối.
```

## Suggested Execution Order For The Full-Permission AI

1. Read handoff docs
2. Stabilize backend build issue
3. Finish AI-2 drafts DB index path / optional schema promotion for `ProjectId`
4. Verify AI-2 module progress behavior
5. Align global RBAC project visibility
6. Validate sidebar/project tree integration
7. Validate timeline quick-add and gamification consistency
8. Run frontend build
9. Run backend build
10. Write final report
