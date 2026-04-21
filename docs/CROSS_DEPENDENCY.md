## AI-3 Cross Dependency

### AI-3 scope status summary

- AI: `AI-3`
- AI-3 scoped files already handled:
  - `Backend/src/TaskManagement.API/Controllers/ProjectsController.cs`
  - `Backend/src/TaskManagement.API/Controllers/NotificationsController.cs`
  - `Backend/src/TaskManagement.API/Controllers/ProjectMembersController.cs`
  - `Backend/src/TaskManagement.API/Controllers/WorkspacesController.cs`
  - `Frontend/src/views/Dashboard.vue`
  - `Frontend/src/views/ManageSpaces.vue`
  - `Frontend/src/components/CreateProjectModal.vue`
  - `Frontend/src/views/Home.vue`
  - `Frontend/src/components/NotificationsDropdown.vue`
  - `Frontend/src/store/useProjectStore.js`
  - `Frontend/src/router/spaceRoutes.js`
  - `Frontend/src/router/dashboardRoutes.js`

- Scope assessment:
  - AI-3 core scope is functionally mostly complete
  - There are still integration blockers outside AI-3-owned files for final end-to-end completion
  - There is also one route-alignment risk inside AI-3 work that the full-permission integration AI should verify against the final admin route design
  - No new confirmed compile/blocking bug was found inside AI-3-owned files during this re-check

### AI-3 completed items

- Done in `Frontend/src/store/useProjectStore.js`
  - fetch all projects from `/projects/discovery`
  - normalize project rows
  - expose `sidebarProjects`, `favoriteProjects`, `projectTree`
  - expose expand/collapse helpers for sidebar project tree

- Done in `Backend/src/TaskManagement.API/Controllers/ProjectsController.cs`
  - added `PUT /api/projects/{id}/favorite`
  - added `GET /api/projects/{id}/work-items`
  - added `GET /api/worktasks?search=keyword`
  - included `isFavorite` projection in project responses

- Done in `Frontend/src/views/Dashboard.vue`
  - project-oriented dashboard
  - favorite persistence wiring
  - working `New Work Item` dialog flow

- Done in `Frontend/src/views/Home.vue`
  - top search UI for work items
  - dropdown results routing back to project/task context

- Done in `Backend/src/TaskManagement.API/Controllers/NotificationsController.cs`
  - notifications list/read APIs
  - event endpoints for:
    - task assigned
    - task status changed
    - comment added

- Done in `Frontend/src/components/NotificationsDropdown.vue`
  - fetch notifications
  - unread filtering
  - mark read / mark all read
  - SignalR append handling

- Done in `Frontend/src/router/dashboardRoutes.js`
  - removed direct AI-3 routing dependency on `../views/RewardsView.vue`
  - kept `/rewards` route valid by redirecting to `/dashboard`

### Bug 3.1 Sidebar Project Tree

- Priority: `high`
- Related AI: `AI-3`

- Data layer for sidebar tree has been prepared in `Frontend/src/store/useProjectStore.js`.
- `Dashboard.vue` and `Home.vue` now trigger `fetchAllProjects()` so project data is available early.
- `useProjectStore.js` now exposes:
  - `allProjects`
  - `sidebarProjects`
  - `projectTree`
  - `expandedProjectIds`
  - `fetchAllProjects()`
  - `toggleProject()`
  - `expandProject()`
  - `collapseProject()`

### Remaining work by layout/sidebar owner

The actual hardcoded `"Cun"` render is currently located outside AI-3 edit scope:

- `Frontend/src/components/layout/NexusSidebar.vue`
- `Frontend/src/components/layout/NexusTopbar.vue`

Current problem:

- AI-3 completed the data/store side, but the visible sidebar tree depends on layout-owned rendering.
- If those layout files still render hardcoded project labels or ignore `useProjectStore`, bug `3.1` is not fully complete in the real UI.

Suspected cause:

- project tree state was prepared in store first
- layout files were not in AI-3 scope, so the actual navigation render was never fully wired by AI-3

Suggested integration fix:

- read `Frontend/src/store/useProjectStore.js`
- in both layout files:
  - import `useProjectStore()`
  - call `fetchAllProjects()` if needed on mount
  - render `projectStore.projectTree`
  - wire expand/collapse with `toggleProject()`
- replace hardcoded `"Cun"` with dynamic project labels and routes

What AI-3 already finished:

- `Frontend/src/store/useProjectStore.js`
  - project fetch
  - project tree shaping
  - expand/collapse state
- `Frontend/src/views/Home.vue`
  - eager project preload
- `Frontend/src/views/Dashboard.vue`
  - eager project preload

What is still not done:

- actual visible sidebar/tree rendering in layout files

What is outside AI-3 scope:

- `Frontend/src/components/layout/NexusSidebar.vue`
- `Frontend/src/components/layout/NexusTopbar.vue`

To complete bug 3.1, the layout/sidebar AI should:

1. Import `useProjectStore()`
2. Use `projectStore.projectTree` or `projectStore.sidebarProjects`
3. Replace hardcoded `"Cun"` with dynamic project/workspace rendering
4. Render collapsible child nodes:
   - Work items -> `/space/:id`
   - Cycles -> `/space/:id/cycles`
   - Modules -> `/space/:id/modules`
   - Views -> `/space/:id/views`
   - Pages -> `/space/:id/pages`

### Bug 3.4 Project settings route alignment risk

- Priority: `medium`
- Related AI: `AI-3`

Relevant files:

- `Frontend/src/router/spaceRoutes.js`
- `Frontend/src/views/ManageSpaces.vue`
- admin route files outside AI-3 scope, especially AI-4-owned admin routing

Current AI-3 implementation:

- `ManageSpaces.vue` sends settings click to `/space/${projectId}/settings`
- `spaceRoutes.js` redirects `/space/:id/settings` to:
  - `/admin/configuration?projectId=:id`

Potential mismatch:

- `docs/CODEX_5AI_TASK_DELEGATION.md` described the intended shape as `/admin/project/{id}/settings`
- if the final admin UX now standardizes on `/admin/configuration?projectId=:id`, then AI-3 is already aligned
- if the final admin UX uses a project-specific admin route, AI-3 redirect should be updated to match

Suggested integration verification:

- inspect the current admin route source of truth
- keep exactly one canonical project-settings route
- then align:
  - `Frontend/src/router/spaceRoutes.js`
  - `Frontend/src/views/ManageSpaces.vue`

What AI-3 already finished:

- settings button click now routes through:
  - `Frontend/src/views/ManageSpaces.vue`
- route alias exists in:
  - `Frontend/src/router/spaceRoutes.js`

What is still not confirmed:

- whether the final admin route contract is:
  - `/admin/configuration?projectId=:id`
  - or `/admin/project/:id/settings`

What is outside AI-3 scope:

- canonical admin route ownership in AI-4/admin routing files

### Dashboard rewards route ownership note

- Priority: `medium`
- Related AI: `AI-3`

- AI-3 updated `Frontend/src/router/dashboardRoutes.js` so `/rewards` now redirects to `/dashboard`
- This removes the direct routing dependency on AI-5-owned `RewardsView.vue` from AI-3 scope
- If product still wants a dashboard-level rewards entry, AI-5 or the routing owner can later remap `/rewards` back to the restored rewards page

Relevant file:

- `Frontend/src/router/dashboardRoutes.js`

Status:

- not a build blocker anymore
- currently a product-behavior choice

Suggested integration decision:

- either keep `/rewards -> /dashboard` as the safe fallback
- or restore `/rewards` to the real rewards page if the integration owner wants dashboard navigation to expose gamification directly again

What AI-3 already finished:

- removed the route-level dependency that previously caused cross-scope fragility in `dashboardRoutes.js`

What is still not decided:

- product/navigation choice for whether `/rewards` should resolve to dashboard or rewards UI

What is outside AI-3 scope:

- `Frontend/src/views/RewardsView.vue`
- any final top-level gamification navigation choice owned by integration/product decision

### Bug 3.10 Notification event integration is not fully end-to-end yet

- Priority: `high`
- Related AI: `AI-3`

Relevant files:

- `Backend/src/TaskManagement.API/Controllers/NotificationsController.cs`
- AI-1-owned task/comment files outside AI-3 scope:
  - `Backend/src/TaskManagement.API/Controllers/WorkTasksController.cs`
  - `Backend/src/TaskManagement.API/Controllers/CommentsController.cs`

What is done:

- AI-3 added notification endpoints for:
  - task assigned
  - task status changed
  - comment added

What is not confirmed done from AI-3 scope:

- real task/comment business flows calling those endpoints or equivalent service logic
- mention notification flow from the original AI-3 brief

Suspected cause:

- AI-3 could only prepare the notification controller side
- event producers live in AI-1 scope, so the notification system may still be partially disconnected in real usage

Suggested integration fix:

- inspect task update, assignee update, and comment create flows
- either:
  - call the notification endpoints from those flows, or
  - move notification creation into shared service/domain logic if the integration AI wants a cleaner final architecture
- verify whether `mention` must be supported in this sprint
- if yes, add mention parsing + notification creation path

What AI-3 already finished:

- shared notification endpoints in:
  - `Backend/src/TaskManagement.API/Controllers/NotificationsController.cs`
- dropdown UI wiring in:
  - `Frontend/src/components/NotificationsDropdown.vue`

What is still not done / not confirmed:

- dedicated `mention` notification path in AI-3 scope
- guaranteed end-to-end fan-out semantics for all task/comment events

What is outside AI-3 scope:

- task/comment producer logic in:
  - `Backend/src/TaskManagement.API/Controllers/WorkTasksController.cs`
  - `Backend/src/TaskManagement.API/Controllers/CommentsController.cs`

### AI-3 RBAC alignment note for full integration AI

- Priority: `high`
- Related AI: `AI-3`

Relevant files:

- `Frontend/src/store/useProjectStore.js`
- `Backend/src/TaskManagement.API/Controllers/ProjectsController.cs`
- AI-4-owned security/admin files outside AI-3 scope

Current state:

- `useProjectStore.js` still loads general project discovery from `/projects/discovery`
- `ProjectsController.cs` serves those project discovery responses with AI-3 additions such as `isFavorite`
- `docs/MASTER_INTEGRATION_HANDOFF.md` says global project visibility is not fully aligned yet with AI-4's `/api/security/accessible-projects`

Risk:

- non-admin pages may still show projects outside the final intended RBAC visibility model
- AI-3 favorites/sidebar/search flows may appear correct functionally but still rely on broader-than-intended project discovery

Suggested integration fix:

- decide one final source for visible projects:
  - enforce membership/role filtering directly in `ProjectsController.cs`, or
  - change `useProjectStore.js` to consume the security-filtered source
- re-verify:
  - dashboard project cards
  - manage spaces list
  - sidebar tree
  - search entry points that route into project pages

What AI-3 already finished:

- AI-3 project UX flows now consistently rely on:
  - `Frontend/src/store/useProjectStore.js`
  - `Backend/src/TaskManagement.API/Controllers/ProjectsController.cs`

What is still not done:

- final global RBAC alignment for visible project lists outside admin pages

What is outside AI-3 scope:

- AI-4 security source of truth
- any wider integration decision about replacing `/projects/discovery` usage across the app

### Outdated AI-3 build-blocker note

- The older note in this file that frontend build was blocked by `Frontend/src/router/aiRoutes.js` and `Frontend/src/components/CalendarTab.vue` is stale for AI-3 handoff purposes.
- `docs/MASTER_INTEGRATION_HANDOFF.md` states frontend production build currently passes and AI-5 resolved those blockers.
- The integration owner should treat the AI-3 build-blocker wording as superseded by the newer handoff state.

### AI-3 final re-check conclusion

- AI: `AI-3`
- Status: `functionally done inside assigned files, with remaining integration dependencies`
- New blocker found during this re-check: `none inside AI-3-owned files`
- Remaining blockers for full end-to-end closure:
  1. `high` — sidebar project tree visible render still depends on:
     - `Frontend/src/components/layout/NexusSidebar.vue`
     - `Frontend/src/components/layout/NexusTopbar.vue`
  2. `high` — global RBAC alignment for project visibility still needs final integration across:
     - `Frontend/src/store/useProjectStore.js`
     - `Backend/src/TaskManagement.API/Controllers/ProjectsController.cs`
     - AI-4 security source
  3. `high` — notification flow completeness still depends on event producer integration and optional mention rule
  4. `medium` — project settings route canonical path should be verified against final admin route design
  5. `medium` — `/rewards` route is stable but currently a product fallback choice, not the final product decision

---

## AI-2 Cross Dependency

### AI-2 final scope status

- AI: `AI-2`
- Scope status: `done inside assigned files after re-check`
- New blocker found inside AI-2-owned files during this pass: `none`
- Remaining work for final integration is limited to out-of-scope data-layer/performance verification items below.

### 1. Drafts database index is still missing

- AI: `AI-2`
- Priority: `critical`
- Related files:
  - downstream query owner:
    - `Backend/src/TaskManagement.API/Controllers/DraftsController.cs`
  - out-of-scope implementation area:
    - EF model / entity configuration / migrations

- Current status:
  - Done:
    - `DraftsController.cs` paginates draft listing
    - `DraftsController.cs` now honors requested `projectId`
    - `DraftsView.vue` sends `projectId`, `page`, and `pageSize`
    - `DraftsView.vue` uses virtualized rendering to reduce DOM cost
  - Not done:
    - no database index has been added for the drafts listing query path

- Concrete issue:
  - correctness in AI-2 scope is now covered, but large draft datasets can still degrade because the database still sorts and filters without a supporting index.

- Suspected cause:
  - AI-2 was not allowed to edit entity/configuration/migration files outside its assigned file list.

- What AI-2 has already handled:
  - backend pagination
  - frontend pagination
  - drafts payload normalization
  - project-aware drafts filtering in controller
  - virtual list rendering

- What is outside AI-2 scope:
  - adding EF configuration
  - creating migration
  - applying migration

- Recommended handling for full-permission integration AI:
  - add index for drafts listing, preferably:
    - `TaskDrafts(UserId, UpdatedAt)`
  - if the integrator promotes payload `projectId` into a real column, prefer:
    - `TaskDrafts(UserId, ProjectId, UpdatedAt)`

### 2. Drafts project scoping works, but the storage model is still payload-based

- AI: `AI-2`
- Priority: `high`
- Related files:
  - `Backend/src/TaskManagement.API/Controllers/DraftsController.cs`
  - out-of-scope structural follow-up:
    - `TaskDraft` entity / DbContext / migrations

- Current status:
  - Done:
    - `GET /api/drafts` now narrows results by requested `projectId`
    - draft create/update flows preserve `projectId` in payload
  - Not done:
    - `projectId` is still stored in `PayloadJson`, not as a first-class database column

- Concrete issue:
  - current controller behavior is functionally correct for AI-2 scope, but data access remains string/JSON based, which is weaker for indexing, validation, and long-term maintenance.

- Suspected cause:
  - schema changes were outside AI-2 ownership.

- What AI-2 has already handled:
  - fixed the missing backend `projectId` filtering gap that had been previously documented

- What is outside AI-2 scope:
  - promoting `ProjectId` into schema
  - data backfill/migration

- Recommended handling for full-permission integration AI:
  - optional but strongly recommended:
    - add real `ProjectId` column to `TaskDraft`
    - backfill from payload if needed
    - switch drafts query to direct `(UserId, ProjectId, UpdatedAt)` filtering

### 3. Module progress formula needs integration-level verification

- AI: `AI-2`
- Priority: `medium`
- Related files:
  - `Backend/src/TaskManagement.API/Controllers/ModulesController.cs`

- Current status:
  - Done:
    - progress formula now matches requested behavior:
      - `DONE tasks / total tasks * 100`
  - Not done:
    - no dedicated automated verification or integration smoke test was added by AI-2

- Concrete issue:
  - implementation is in place, but final integration should verify real status values and edge cases so the UI does not show misleading percentages.

- Suspected cause:
  - AI-2 scope prioritized controller/UI behavior; test harness and broader verification assets were outside ownership.

- What AI-2 has already handled:
  - formula change in `ModulesController.cs`

- What is outside AI-2 scope:
  - adding broader repo-level tests if they require non-AI-2 files

- Recommended handling for full-permission integration AI:
  - verify with live data or targeted tests for:
    - no tasks
    - mixed statuses
    - `DONE` / `Done` / `Complete` variants

### AI-2 handoff summary for final integrator

- Done:
  - all AI-2 listed UI/API gaps are addressed at feature level inside allowed files
  - no new blocker remains inside:
    - `DraftsController.cs`
    - `DraftsView.vue`
    - `ModulesController.cs`
    - `ModulesTab.vue`
    - `LabelsController.cs`
    - `LabelManager.vue`
    - `FilterBar.vue`
- Still incomplete:
  - database index for drafts query
  - optional schema promotion for drafts `ProjectId`
  - dedicated verification coverage for module progress behavior

## AI-1 Cross Dependency

### AI-1 final scope status

- Scope AI-1 đã hoàn tất trong đúng phân vùng:
  - `Backend/src/TaskManagement.API/Controllers/WorkTasksController.cs`
  - `Backend/src/TaskManagement.API/Controllers/CommentsController.cs`
  - `Frontend/src/components/TaskDetailModal.vue`
  - `Frontend/src/views/SpaceSummary.vue`
  - `Frontend/src/store/useWorkTaskStore.js`
- Các bug AI-1 được giao trong `docs/CODEX_5AI_TASK_DELEGATION.md` đã được xử lý ở mức feature scope.
- Hiện tại không còn blocker nội bộ nào trong các file AI-1 đang chặn AI-1 đi tiếp.
- AI-1 đã tự fix thêm một integration gap vẫn còn nằm trong scope của mình:
  - `Frontend/src/views/SpaceSummary.vue`
  - listener `global-create-task` giờ đã tiêu thụ `event.detail.plannedStartDate` và `event.detail.dueDate`
  - timeline quick-add từ AI-5 không còn bị mất prefill ngày ở phía consumer AI-1 nữa

### Frontend build blockers outside AI-1 scope

- Resolved by AI-5:
  - `Frontend/src/views/RewardsView.vue` now exists and builds correctly
  - `Frontend/src/components/CalendarTab.vue` no longer has the syntax error near line `100`

### Needed owner action

- No remaining AI-1 frontend build blocker from these two files.

### AI-1 verification status re-check

- Re-verified by AI-1:
  - command: `dotnet build Backend/src/TaskManagement.API/TaskManagement.API.csproj`
  - current result: `Build succeeded`
- Re-verified by AI-1:
  - command: `npm run build` in `Frontend/`
  - current result: `Build succeeded`
- This means the older repo-level note saying backend build still exits with code `1` is stale for AI-1 handoff purposes.
- Full integration AI should still run one final full verification pass, but AI-1 changes are not currently blocking backend compile.

### What is done in AI-1 scope

- `Backend/src/TaskManagement.API/Controllers/WorkTasksController.cs`
  - done:
    - fixed task full-update concurrency path for bug `1.1`
    - verified `projectId` ownership in `PUT /api/projects/{projectId}/WorkTasks/{id}`
    - reject missing `RowVersion`
    - wired real business flows to notification event endpoints for:
      - task assignment
      - task status change
    - status-change notification now fires from:
      - `PUT /status`
      - `PATCH`
      - drag-drop reorder flow
  - done:
    - board/list subtask filtering support for AI-1 frontend behavior

- `Backend/src/TaskManagement.API/Controllers/CommentsController.cs`
  - done:
    - comment formatting/reaction persistence via content metadata
    - comment attachment delete endpoint
    - reaction endpoint
    - comment-added notification wired into real comment creation flow

- `Frontend/src/components/TaskDetailModal.vue`
  - done:
    - description/comment formatting fixes
    - code mode toggle
    - remove description attach button
    - activity tab loading
    - relative last-edited rendering
    - subtask toggle
    - image preview/delete UX
    - parent task selector filtering

- `Frontend/src/views/SpaceSummary.vue`
  - done:
    - hide subtasks from board/list by default
    - inline dropdown controls for status/priority/assignee
    - stop click propagation on card controls
    - list-view inline status change
    - support `IN REVIEW` and `CANCELLED`
    - full `PUT` payload path with `rowVersion` for task-detail core updates
    - consume `global-create-task` event detail so timeline-triggered create flow can prefill `plannedStartDate` and `dueDate`

- `Frontend/src/store/useWorkTaskStore.js`
  - done:
    - `updateTask()` supports both `PATCH` and `PUT`

### Notification follow-up outside AI-1 scope

- AI-1 has connected real task/comment/status actions to:
  - `POST /api/notifications/events/task-assigned`
  - `POST /api/notifications/events/task-status-changed`
  - `POST /api/notifications/events/comment-added`

### Remaining AI-1-related integration risks outside scope

- Priority summary for AI-1:
  - `critical`: none confirmed in AI-1 scope after latest re-check
  - `high`: none confirmed in AI-1 scope after latest re-check
  - `medium`: only cross-scope notification architecture/product-rule decisions remain

#### 1. Comment notification fan-out semantics may be narrower than earlier local behavior

- Related files:
  - done in AI-1 scope:
    - `Backend/src/TaskManagement.API/Controllers/CommentsController.cs`
  - outside AI-1 scope:
    - `Backend/src/TaskManagement.API/Controllers/NotificationsController.cs`

- Current status:
  - done:
    - comment create flow now calls `POST /api/notifications/events/comment-added`
  - not guaranteed:
    - reporter notification
    - `@mention` notification
    - any non-assignee fan-out handled before by local comment-side notification logic

- Suspected cause:
  - AI-1 intentionally routed comment creation through the shared notification endpoint to satisfy end-to-end integration
  - current shared endpoint behavior is owned by AI-3 and appears oriented around assignee-targeted notification creation

- Suggested integration fix:
  - integration owner should read `NotificationsController.cs`
  - decide final product rule for comment notifications:
    - assignees only
    - assignees + reporter
    - assignees + reporter + mentioned users
  - if broader fan-out is required, extend the shared notification event handler there instead of reintroducing divergent logic in AI-1 files

#### 2. Notification event dispatch currently depends on internal HTTP self-call

- Related files:
  - `Backend/src/TaskManagement.API/Controllers/WorkTasksController.cs`
  - `Backend/src/TaskManagement.API/Controllers/CommentsController.cs`
  - shared notification target outside AI-1 scope:
    - `Backend/src/TaskManagement.API/Controllers/NotificationsController.cs`

- Current status:
  - done:
    - AI-1 forwards the current bearer token and posts to same-app notification endpoints
  - not done:
    - no shared service abstraction exists yet for notification event publishing
    - failures are intentionally swallowed so task/comment actions do not fail

- Concrete issue:
  - if host/scheme/proxy/auth forwarding differs between environments, business action can still succeed while notification silently does not get created

- Suspected cause:
  - AI-1 was restricted to controller files only and could not introduce a broader infrastructure/service-layer event publisher cleanly

- Suggested integration fix:
  - final integration AI can keep current behavior if sufficient for sprint close
  - better long-term fix:
    - move notification event creation into shared service/domain logic
    - or inject an internal notification application service instead of same-app HTTP self-calls
  - at minimum, run a manual smoke test for:
    - assign task
    - change status
    - add comment
    - verify notification row + dropdown UI update

#### 3. `docs/DONE_AI1.md` contains an outdated frontend-build note

- Related file:
  - `docs/DONE_AI1.md`

- Current status:
  - done:
    - the implementation summary is still valid
  - outdated:
    - the old verification note still says frontend build was blocked by:
      - `Frontend/src/router/dashboardRoutes.js`
      - `Frontend/src/components/CalendarTab.vue`

- Suspected cause:
  - this note was written before AI-5 and AI-3 completed their later unblock work

- Suggested integration fix:
  - no code fix needed
  - full integration AI may optionally refresh `docs/DONE_AI1.md` or simply treat `docs/MASTER_INTEGRATION_HANDOFF.md` + current `docs/CROSS_DEPENDENCY.md` as the newer source of truth

#### 4. AI-5 timeline quick-add dependency note is now resolved from AI-1 side

- Related files:
  - fixed in AI-1 scope:
    - `Frontend/src/views/SpaceSummary.vue`
  - originating producer outside AI-1 scope:
    - `Frontend/src/components/TimelineTab.vue`

- Priority:
  - medium

- Current status:
  - done:
    - AI-1 now consumes `global-create-task` payload dates and hydrates create-modal state
  - not done:
    - none inside AI-1 scope

- Suspected cause of the old issue:
  - `handleGlobalCreate` previously ignored `event.detail`

- What AI-1 handled:
  - updated `SpaceSummary.vue` so timeline quick-add can pass:
    - `plannedStartDate`
    - `dueDate`
    - optional `statusName`

- Outside AI-1 scope:
  - producer behavior in `TimelineTab.vue` remains owned by AI-5

- Suggested final integration action:
  - treat the old AI-5 note about date-prefill consumption as resolved unless a manual smoke test still reproduces a mismatch
  - smoke test:
    - click timeline quick-add in week/month/quarter
    - verify create modal opens with selected dates already filled

## AI-4 Cross Dependency

### Frontend production build blockers outside AI-4 scope

- Resolved by AI-5:
  - `Frontend/src/router/aiRoutes.js` now resolves `../views/AIPage.vue`
  - `Frontend/src/components/CalendarTab.vue` no longer has the syntax error near line `100`

### Wider RBAC completion outside AI-4 scope

AI-4 added `GET /api/security/accessible-projects` and updated AI-4-owned admin pages to use filtered project lists.

To make project visibility fully consistent across the whole app, the owners of the general project listing flow should align their pages/stores with the same access source, especially files outside AI-4 scope such as:

- `Frontend/src/store/useProjectStore.js`
- non-admin pages that still call `/projects` or `/projects/discovery`
- any backend project listing endpoints that should enforce the same membership rule globally

## AI-5 Cross Dependency

### Scope status

- AI-5 scope is functionally completed inside allowed files:
  - `Backend/src/TaskManagement.API/Controllers/AiController.cs`
  - `Backend/src/TaskManagement.API/Controllers/GamificationController.cs`
  - `Backend/src/TaskManagement.API/Controllers/SprintsController.cs`
  - `Frontend/src/views/AIPage.vue`
  - `Frontend/src/views/RewardsView.vue`
  - `Frontend/src/components/CyclesTab.vue`
  - `Frontend/src/components/TimelineTab.vue`
  - `Frontend/src/components/CalendarTab.vue`
  - `Frontend/src/components/SpreadsheetTab.vue`
  - `Frontend/src/components/KanbanBoard.vue`
  - `Frontend/src/components/ListView.vue`
  - `Frontend/src/store/useSprintStore.js`
  - `Frontend/src/router/aiRoutes.js`
- Frontend production build passes after AI-5 changes.
- There are no remaining AI-5-owned frontend build blockers.

### What is done

- `AiController.cs`
  - done: Gemini retry 3 lan + fallback message cho `breakdown-task`
- `AIPage.vue`
  - done: fix send button, input binding, progress/thinking UI
  - done: restore/build-safe route target for `aiRoutes.js`
  - done: GitHub repo analysis flow from frontend via GitHub API
- `RewardsView.vue`
  - done: file exists, route resolves, frontend build blocker removed
  - done: render wallet, formula, leaderboard, achievements
- `TimelineTab.vue`
  - done: click bar mo task detail
  - done: week/month/quarter timeline positioning + bucket progress
  - done: new work item button now triggers existing global create flow
  - done: create mode UI
- `CalendarTab.vue`
  - done: syntax error fixed
  - done: hover tooltip/task preview
- `SpreadsheetTab.vue`
  - done: display options/filter logic
- `KanbanBoard.vue`, `ListView.vue`
  - done: progress ring hover %
- `useSprintStore.js`
  - done: TTL cache + optimistic favorite toggle
- `docs/performance-optimization-report.md`
  - done: performance analysis/report created

### Remaining integration gaps for final AI

#### 1. Gamification engine is only partially aligned end-to-end

- related files:
  - done in scope:
    - `Backend/src/TaskManagement.API/Controllers/GamificationController.cs`
    - `Frontend/src/views/RewardsView.vue`
  - outside AI-5 scope:
    - `Backend/src/TaskManagement.Infrastructure/Services/GamificationService.cs`

- issue:
  - UI/controller now expose the intended product contract:
    - `Gia tri x Anh huong x So ngay`
    - early completion bonus `10%`
    - contribution-based split
    - slower level progression / career ladder
  - but the core reward transaction engine may still calculate points using the old service logic.

- suspected cause:
  - AI-5 was not allowed to edit the infrastructure service where reward transactions are actually written.

- suggested fix:
  - final integration AI should read and update `GamificationService.cs` to match the controller/UI contract exactly.
  - after updating, verify:
    - point transaction creation
    - early bonus percentage
    - contribution-weight split
    - level thresholds shown in `RewardsView.vue`

- done vs not done:
  - done: controller/view contract and presentation layer
  - not done: full service-layer reward math alignment

#### 2. Timeline quick-add is only partially integrated

- related files:
  - done in scope:
    - `Frontend/src/components/TimelineTab.vue`
  - outside AI-5 scope:
    - `Frontend/src/views/SpaceSummary.vue`
    - any task-create modal/global create handler files used by that view

- issue:
  - `TimelineTab.vue` now dispatches `global-create-task`
  - this opens the existing create flow, but exact selected bucket dates are not guaranteed to prefill the create modal end-to-end.

- suspected cause:
  - the consumer of `global-create-task` is outside AI-5 scope and may ignore `event.detail`.

- suggested fix:
  - final integration AI should inspect the `global-create-task` listener in `SpaceSummary.vue`
  - ensure `event.detail.plannedStartDate` and `event.detail.dueDate` are passed into the actual create modal state
  - verify by clicking timeline buckets in Week / Month / Quarter mode

- done vs not done:
  - done: quick-add trigger from timeline
  - not done: guaranteed date-prefill consumption in global create flow

#### 3. GitHub repo analysis is implemented, but only at frontend integration level

- related files:
  - done in scope:
    - `Frontend/src/views/AIPage.vue`
  - not implemented in scope:
    - `Backend/src/TaskManagement.API/Controllers/AiController.cs` does not own a dedicated GitHub repo analysis endpoint yet

- issue:
  - current repo analysis works by calling GitHub API directly from frontend and then sending a composed prompt to AI chat
  - this is usable, but not a full backend-mediated integration.

- suspected cause:
  - priority was to complete usable AI-5 flow without adding cross-scope service dependencies
  - no dedicated GitHub backend/service layer exists in AI-5-owned files

- suggested fix:
  - final integration AI can decide whether current UX is sufficient
  - if stricter architecture is needed, add a backend endpoint/service for repo metadata fetch, token handling, and prompt assembly

- done vs not done:
  - done: working user-facing repo analysis flow
  - not done: backend-owned GitHub analysis API

#### 4. Backend build verification is still unresolved at repo level

- related files:
  - observed command:
    - `dotnet build Backend/src/TaskManagement.API/TaskManagement.API.csproj`
  - AI-5-owned files that were changed:
    - `Backend/src/TaskManagement.API/Controllers/AiController.cs`
    - `Backend/src/TaskManagement.API/Controllers/GamificationController.cs`

- issue:
  - frontend build passes
  - backend build still exits with code `1`
  - current sandbox run does not surface a concrete C# compile error from AI-5 files

- suspected cause:
  - restore / project graph / environment issue at repo level
  - not clearly attributable to AI-5 controller code

- suggested fix:
  - final integration AI should investigate with full repo authority:
    - restore graph
    - transitive project references
    - build environment / SDK restore behavior
    - then re-run backend build after stabilizing restore

- done vs not done:
  - done: AI-5 frontend verify
  - not done: reliable backend compile verification
