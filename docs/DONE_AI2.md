## DONE_AI2

### Scope completed

- `2.1 Drafts: tao 1000 ban nhap -> server dung`
  - Added backend pagination in `DraftsController.cs`
  - Added frontend pagination usage in `DraftsView.vue`
  - Added pagination metadata response
  - Added lightweight virtualized rendering in the drafts list

- `2.2 Drafts: API khong tra data`
  - Fixed draft payload serialization / deserialization so API now returns:
    - `statusName`
    - `priority`
    - `assignee`
    - `label`
    - `startDate`
    - `dueDate`
    - `cycle`
    - `module`
    - `projectId`
  - Synced `DraftsView.vue` fetch flow with project context loading
  - Updated `DraftsController.cs` GET filtering so drafts listing now honors requested `projectId`

- `2.3 Drafts: Start Date / Due Date khong click duoc`
  - Updated hidden picker open logic to use `handleOpen()` when available

- `2.4 Drafts: cai thien hieu suat khi >1000 ban nhap`
  - Added backend pagination
  - Added frontend pagination controls
  - Added virtualized rendering window for visible rows

- `2.5 Modules: load cham`
  - Added backend pagination and search/sort query support in `ModulesController.cs`
  - Added frontend lazy loading with `Load more modules`
  - Reduced repeated refetch patterns

- `2.6 Modules: khong cap nhat duoc cong viec trong module`
  - Added `taskIds` handling in `ModulesController.cs` PUT
  - Synced `IssueModules` links on update
  - Added task assignment UI in `ModulesTab.vue`

- `2.7 Modules: chon lich khong real-time`
  - Replaced module date editing flow with range picker that calls API immediately on change

- `2.8 Modules: tinh toan phan tram chua ro`
  - Backend progress now uses:
    - `% = DONE tasks / total tasks * 100`

- `2.9 Modules: sort + filter chua hoat dong`
  - Added search, sort, and status filter controls in `ModulesTab.vue`

- `2.10 Modules: 3 nut view chua hoat dong`
  - Added working `list / grid / status` view toggles in `ModulesTab.vue`

- `2.11 Labels trong Cycle: cot labels khong click duoc`
  - Added backend label assignment aliases for both:
    - `/tasks/{taskId}/labels`
    - `/WorkTasks/{taskId}/labels`

- `2.12 Labels: DB co data nhung dropdown khong hien`
  - Labels API now returns project labels plus workspace-level labels
  - Added `color` alias in response
  - Added `Color` alias support in create/update request DTOs
  - `LabelManager.vue` now reloads when `projectId` changes and normalizes API data

- `2.13 Filter: bi an sau giao dien`
  - `FilterBar.vue` now teleports the builder to `body`
  - Builder uses fixed positioning with `z-index: 9999`

### Files changed

- `Backend/src/TaskManagement.API/Controllers/DraftsController.cs`
- `Backend/src/TaskManagement.API/Controllers/ModulesController.cs`
- `Backend/src/TaskManagement.API/Controllers/LabelsController.cs`
- `Frontend/src/views/DraftsView.vue`
- `Frontend/src/components/ModulesTab.vue`
- `Frontend/src/components/LabelManager.vue`
- `Frontend/src/components/FilterBar.vue`
- `docs/DONE_AI2.md`
- `docs/CROSS_DEPENDENCY.md`

### Verification

- Backend build:
  - `dotnet build Backend/src/TaskManagement.API/TaskManagement.API.csproj`
  - Result: success
  - Notes:
    - build currently completes with 2 warnings outside AI-2 scope:
      - `Backend/src/TaskManagement.API/Controllers/StickiesController.cs`
      - `Backend/src/TaskManagement.API/Controllers/AuditLogsController.cs`

- Vue SFC parse check for AI-2 edited files:
  - `src/views/DraftsView.vue`
  - `src/components/ModulesTab.vue`
  - `src/components/LabelManager.vue`
  - `src/components/FilterBar.vue`
  - Result: all parsed successfully

- Frontend full build:
  - `npm run build`
  - Result: not re-run by AI-2 after later cross-AI fixes
  - Notes:
    - earlier AI-2 run had failed due to pre-existing issues outside scope
    - newer handoff from other AIs says frontend production build now passes
    - final integration AI should treat the latest repo-level build as source of truth

### Remaining cross dependency

- DB index part of `2.1` still needs another owner because AI-2 is not allowed to edit entity/configuration/migration files outside the assigned file list.
- Module progress logic in `ModulesController.cs` is implemented, but still needs integration-level verification against real status variants and zero-task cases.
