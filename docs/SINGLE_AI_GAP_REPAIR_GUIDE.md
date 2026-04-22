# Single AI Gap Repair Guide

Last updated: 2026-04-21

## Purpose of this file

This file consolidates all the gaps you listed into clearly separated functional groups, with:

- priority level
- most likely files involved
- business logic that must be preserved
- suggested fix direction
- execution order so that 1 AI can handle the work end-to-end

This is the "GPS guide" so a single AI can enter the repo and work in the right order without getting lost.

## Execution principles

1. Fix logic and data-boundary issues before UI polish
2. Fix backend contracts first if the UI is wrong because payload/state is wrong
3. Lock business rules before touching timeline/cycle/subtask behavior
4. Only after task/project/date/subtask behavior is stable should analytics, rewards, and smaller UX issues be handled

## Quick summary by severity

### P0 - Must be fixed first

- OTP/email existence validation
- Project switching requires F5 before updating correctly
- Task/project mapping leaks across projects
- Task/cycle date pickers shift dates and allow invalid selections
- Subtask default status is wrong, and show/hide behavior is wrong
- Timeline range / Today / Week-Month-Quarter behavior is wrong
- Discard currently clears forms instead of closing them

### P1 - Should be fixed immediately after P0

- Customized Insights UI is wrong
- Analytics expand button closes the panel instead of expanding it
- Task completion percentage is missing, making Done workflow incomplete
- Rewards page cannot be opened properly
- Module/Cycle filtering and counting logic is wrong
- Home page cannot scroll

### P2 - After the core logic is stable

- Hover preload for project data
- Project logo/title should be more dynamic
- List view layout is horizontally broken
- Smaller UI/UX improvements in task detail and analytics

## Group 1: Auth, OTP, Home, Entry Flow

### 1.1 OTP should only be sent when email validity rules pass

Symptoms:

- The system can currently send OTP without validating whether the email exists or should be allowed in the current flow

Already checked:

- `Backend/src/TaskManagement.API/Controllers/AuthController.cs`
- `SendOtp()` currently generates and sends OTP immediately, without checking whether the email exists in the system

Main suspected files:

- `Backend/src/TaskManagement.API/Controllers/AuthController.cs`
- `Backend/src/TaskManagement.Infrastructure/Services/AuthService.cs`
- auth DTOs in `TaskManagement.Application`

Fix direction:

- Split rules clearly:
  - forgot password / login OTP: email must already exist
  - register OTP: email must not already exist
- Return explicit errors:
  - `404/400`: invalid or non-existent email
  - `409`: email already exists for register flow

Acceptance:

- Invalid email format: no send
- Non-existent email in login/reset flow: no send
- Existing email in register flow: no send

### 1.2 Home page cannot scroll

Symptoms:

- Home page appears to have scroll locked

Suspected files:

- `Frontend/src/views/Home.vue`
- shared layout / global styles in `Frontend/src/App.vue`

Likely causes:

- `html/body` or a parent container may have `overflow: hidden`
- sticky layout + min-height + shell wrapper conflict

Acceptance:

- Home scroll works with mouse wheel, touchpad, and keyboard
- Works on both desktop and mobile

### 1.3 Entry flow must require login/register

Current state:

- Home has already been changed so only `Login` and `Register` remain
- This direction should be preserved; do not reopen shortcut access to dashboard

File to preserve:

- `Frontend/src/views/Home.vue`

## Group 2: Project switching, project data, project identity, drafts

### 2.1 Switching project requires F5 before the UI updates

Symptoms:

- Switching to another project still shows data from the previous project until F5

Already checked:

- `Frontend/src/store/useProjectStore.js`
- The store has `fetchProjectDetails(projectId)` but no request cancellation / preload / race-condition guard

Suspected files:

- `Frontend/src/store/useProjectStore.js`
- `Frontend/src/components/layout/NexusTopbar.vue`
- `Frontend/src/views/SpaceSummary.vue`
- route watchers around the project page

Fix direction:

- Watch `route.params.id` in the project shell/page and refetch on project change
- Abort previous requests when the user switches quickly
- Reset local page state on the new `projectId` before rebinding data

Acceptance:

- Project switching works without F5
- Topbar, breadcrumb, task list, members, and labels all switch with the new project

### 2.2 Hovering over a project should preload data

Symptoms:

- Desired behavior: hover should preload data to reduce delay when clicking

Performance relation:

- Strongly aligned with the performance report

Suspected files:

- sidebar project item component
- `useProjectStore.js`

Fix direction:

- Prefetch `project details`, `members`, `labels`, `task statuses`, and `task list page 1`
- Keep cache short-lived, around 20-30 seconds

### 2.3 Tasks created in project A still appear in project B

Symptoms:

- Tasks leak across projects

Already checked:

- Backend `CreateAsync` in `WorkTaskService.cs` already assigns `ProjectId` and validates sprint/module/parent against project
- This strongly suggests the leak is more likely in frontend state reuse / local filters / draft hydration

Suspected files:

- `Frontend/src/views/SpaceSummary.vue`
- `Frontend/src/store/useWorkTaskStore.*`
- `DraftsView` / `TaskDrafts` mapping
- create task modal in `TaskDetailModal.vue`

Must inspect carefully:

- When fetching task list, does the page clear the previous project's tasks first?
- When restoring drafts, is `projectId` correctly bound?
- When `localStorage/currentProjectId` changes, is state invalidated properly?

Acceptance:

- Tasks only appear inside their correct project
- Drafts created in a project restore inside the same project

### 2.4 Project name/logo in the header

Symptoms:

- When a project is selected, the header area should display that project's name/logo; otherwise it should fall back to the default logo

Suspected files:

- `Frontend/src/components/layout/NexusTopbar.vue`
- `Frontend/src/components/layout/*Sidebar*.vue`
- `useProjectStore.js`

Acceptance:

- With selected project: show correct project name/logo
- Without selected project: use default fallback

## Group 3: Task detail, subtask, progress, workflow

### 3.1 Task progress % is missing, so Done workflow is incomplete

Symptoms:

- There is no clear task completion percentage field
- Users cannot understand why a task should or should not be Done

Already checked:

- `TaskDetailModal.vue` already has per-assignee progress in task assignments
- But overall task/subtask progress is still not surfaced clearly enough in the UI

Suspected files:

- `Frontend/src/components/TaskDetailModal.vue`
- DTO / entity `TaskAssignment.ProgressPercent`
- backend update endpoint for assignment progress

Fix direction:

- Show total task progress
- Allow editing progress by assignee
- Decide the Done rule explicitly:
  - if business requires 100% before Done, enforce it in backend
  - if not, still expose current % clearly in the UI

Acceptance:

- Task detail displays total progress
- Progress can be updated
- If 100% is required before Done, backend blocks correctly

### 3.2 Subtask rows are missing important information and cannot be edited inline

Symptoms:

- Subtasks should display:
  - title
  - id/sequence
  - priority
  - status
  - assignee
- Status/priority/assignee should all be clickable and editable directly inside the parent task

Already checked:

- `TaskDetailModal.vue` currently renders subtasks in a very minimal list, mostly sequence + title

Suspected files:

- `Frontend/src/components/TaskDetailModal.vue`
- `Backend/src/TaskManagement.Infrastructure/Services/WorkTaskService.cs`
- task detail payload for child subtasks may be incomplete

Acceptance:

- In the parent task, each subtask row shows the required metadata
- Inline editing works without opening the subtask detail panel

### 3.3 The "Add work parent" line should show the parent task name

Symptoms:

- When opening a subtask detail, the parent section is not clear; it should display the parent task name and must not disappear

Suspected files:

- `TaskDetailModal.vue`

Acceptance:

- If the current task is a subtask, the parent label + parent task name are always visible

### 3.4 New subtasks default to Done instead of Backlog

Symptoms:

- Default status is wrong

Already checked:

- Backend has normalized status handling and default statuses
- Need to inspect the quick subtask create payload in `TaskDetailModal.vue`

Suspected files:

- `Frontend/src/components/TaskDetailModal.vue`
- `Backend/src/TaskManagement.Infrastructure/Services/WorkTaskService.cs`

Acceptance:

- New subtasks default to `BACKLOG`

### 3.5 Show/Hide subtask behavior is not working correctly

Symptoms:

- Hidden inside task detail but still visible incorrectly in project views
- Display > `Show sub-work items` does not behave reliably

Already checked:

- `SpaceSummary.vue` has a `showSubtasks` checkbox
- `ViewsTab.vue` also has a `Show sub-work items` setting
- High chance the display setting exists only in UI state but is not properly applied to the data pipeline

Suspected files:

- `Frontend/src/views/SpaceSummary.vue`
- `Frontend/src/components/ViewsTab.vue`
- grouping/computed filtering logic for top-level tasks

Acceptance:

- When `Show sub-work items` is off: subtasks are hidden from project views
- When on: subtasks appear correctly
- Parent tasks still account for subtask totals

### 3.6 Done rules, subtasks, relations, dependencies

Rules that must be preserved:

- Parent task cannot be moved to Done while unfinished subtasks still exist
- Closed cycles blocking task edits is correct and must remain

Already checked in backend:

- `WorkTaskService.UpdateTaskStatusAsync()` already has:
  - dependency checks
  - parent/subtask unfinished checks
  - sprint lock

Requirement:

- UI must show clear messages so users understand why actions are blocked

## Group 4: Analytics, Customized Insights, Rewards

### 4.1 Customized Insights should be reduced to one dropdown button

Symptoms:

- Desired UX: just one `Customized Insights` button
- Dropdown options:
  - by priority
  - by status
- Underlying logic should remain

Already checked:

- `SpaceSummary.vue` currently renders multiple separate analytics blocks in the analytics sidebar

Suspected files:

- `Frontend/src/views/SpaceSummary.vue`

Acceptance:

- Cleaner UI
- Existing logic/export still works

### 4.2 Analytics expand button closes the panel instead of expanding it

Already checked:

- In `SpaceSummary.vue`, both the expand icon and the close icon currently do `showAnalyticsSidebar = false`

Conclusion:

- This is a direct bug, not a guess

Fix direction:

- Add `isAnalyticsFullscreen` state
- Expand button should toggle fullscreen class/state
- Close button should close the panel

Acceptance:

- Clicking expand makes the panel larger
- The panel does not disappear

### 4.3 Rewards cannot be opened

Symptoms:

- User cannot view reward points after task completion

Suspected files:

- rewards route/page in frontend
- gamification/rewards API
- route guards

Need to check further during implementation:

- does the route exist?
- is the navigation click wired?
- does the API return data?

## Group 5: Date picker, cycle, timeline, calendar, timezone

### 5.1 Forms that create new schedule data must hide past dates

Business rule:

- New selections should only allow today and future dates
- End date must not be before start date
- Existing tasks/cycles created in the past should still display their original dates normally

Already checked:

- `TaskDetailModal.vue` currently uses basic `el-date-picker` without clear disable-date rules
- `CyclesTab.vue` has custom date-picker-related UI and should be checked for disabled-day logic

Suspected files:

- `Frontend/src/components/TaskDetailModal.vue`
- `Frontend/src/components/CyclesTab.vue`
- `Frontend/src/components/TimelineTab.vue`

Acceptance:

- New create flow: cannot select past dates
- Editing old items: still shows historical values normally
- End date before start date: blocked in both UI and backend

### 5.2 Cycle dates are shifted by 1 day (21 -> 20, 28 -> 27)

Symptoms:

- This strongly resembles timezone / `toISOString()` / local-to-UTC conversion bugs

Already checked:

- `TimelineTab.vue` currently sets `plannedStartDate = startDate.toISOString()`
- This is a strong signal for date-only shift bugs if the UI works with local dates

Fix GPS:

- Find all places where date-only values are stored via `toISOString()`
- Replace with a local date-only helper strategy, for example:
  - `YYYY-MM-DD`
  - or noon-local strategy if backend still expects DateTime

High-probability files:

- `Frontend/src/components/TimelineTab.vue`
- `Frontend/src/components/CyclesTab.vue`
- `Frontend/src/components/TaskDetailModal.vue`
- sprint/task DTO date handling in backend

Acceptance:

- Selecting 21-28 must save and display exactly 21-28

### 5.3 Cycle logic says "not started" cycles are already closed

Symptoms:

- Cycle status classification is wrong

Suspected files:

- `Frontend/src/components/CyclesTab.vue`
- backend sprint service / cycle status mapping

Rule to preserve:

- Closed cycles blocking task edits is correct
- Upcoming cycles must not be displayed as closed

Acceptance:

- Upcoming, Active, Completed are classified correctly by date and status

### 5.4 Timeline view has wrong Week/Month/Quarter/Today/Create mode behavior

Already checked:

- `TimelineTab.vue` currently maps:
  - `Week` => buckets by `day`
  - `Month` => buckets by `week`
  - `Quarter` => buckets by `month`
- Technically that can be valid if the labels and UX expectation match
- But current user expectation is:
  - Week = weekly view
  - Month = monthly view
  - Quarter = correct 3-month quarter view

Direct bugs to fix:

- `Today` does not focus tasks for today the way the user expects
- `Create mode` says it is on, but clicking the timeline does not reliably create a task

Suspected files:

- `Frontend/src/components/TimelineTab.vue`

Acceptance:

- Week, Month, Quarter align with expected business behavior
- Today focuses the correct area
- Create mode allows clicking canvas/grid to create tasks

## Group 6: List/Board/Display/Filtering

### 6.1 List view is laid out horizontally but should be vertical

Symptoms:

- List layout uses the wrong axis

Suspected files:

- `Frontend/src/views/SpaceSummary.vue`
- `Frontend/src/components/ListView.vue`

Acceptance:

- Task groups stack vertically as a true list
- It no longer behaves like a horizontal board

### 6.2 Display ordering options are currently not applied

Symptoms:

- `Manual`, `Last created`, `Last updated`, `Priority` do not actually affect rendering
- `Start date` should be removed from the display ordering menu

Already checked:

- `SpaceSummary.vue` has radio-button UI, but it appears to be dead UI with no binding to actual computed sorting

Suspected files:

- `Frontend/src/views/SpaceSummary.vue`

Acceptance:

- Each display ordering option actually re-sorts tasks
- `Start date` is removed from this menu

### 6.3 Module/Cycle filters should hide subtasks but still count them properly

Business rule:

- Do not show subtasks directly inside module/cycle lists
- But parent task counters must still account for related subtasks

Suspected files:

- `Frontend/src/components/ModulesTab.vue`
- `Frontend/src/components/CyclesTab.vue`
- backend aggregate/count APIs if any

Acceptance:

- Clear counts in UI:
  - task count
  - subtask count
- Subtasks do not clutter the main list

## Group 7: Form behavior

### 7.1 Discard button should close the form, not wipe the form

Already checked:

- `TaskDetailModal.vue` has a `Discard` button in create mode

Suspected files:

- `TaskDetailModal.vue`
- other create modals with `Discard`

Acceptance:

- Clicking `Discard` closes the modal/form
- It does not simply clear the inputs and remain open

## Group 8: Comment rich text formatting

Current state:

- There was a previous issue where bold/italic/underline/strike formatting did not persist correctly into the DB
- Some fixes were already applied in backend sanitization and frontend editor command handling

Must remain in the regression checklist:

- Create rich text comment
- Reload task detail
- DB still stores the correct HTML
- Editing a comment does not strip formatting

Related files:

- `Frontend/src/components/TaskDetailModal.vue`
- `Backend/src/TaskManagement.API/Controllers/CommentsController.cs`

## Group 9: Performance-related gaps that also affect correctness

These are not just "nice-to-have" optimizations. They directly affect real bugs:

- project switching requiring F5
- no hover preload
- full task list heavy payload
- rendering too many tasks at once

Related file:

- `docs/performance-optimization-report.md`

## Recommended 1-AI execution roadmap

### Phase 1 - Lock project identity and data boundary first

1. Fix OTP/email validation
2. Fix project switching reactivity + abort stale requests
3. Fix task/draft/project mapping
4. Fix project header/logo binding

### Phase 2 - Lock task/subtask workflow

1. Default subtask = backlog
2. Correct show/hide subtask behavior
3. Show full subtask metadata in parent task
4. Show parent task name in child task detail
5. Show and edit progress % for task/subtask
6. Expose clear Done rule messages

### Phase 3 - Lock date/timezone/cycle/timeline behavior

1. Disable past dates in create flow
2. Block end < start
3. Fix 1-day date shift in cycle/task flows
4. Fix cycle upcoming/active/completed logic
5. Fix timeline Today / create mode / Week-Month-Quarter

### Phase 4 - Analytics, display, rewards, admin

1. Fix analytics expand
2. Simplify Customized Insights
3. Fix rewards click/view
4. Fix display ordering logic
5. Fix module/cycle counts
6. Fix list view layout

### Phase 5 - UX and performance finishing pass

1. Fix Home scrolling
2. Add hover preload for project switching
3. Apply the performance hardening from the performance report

## Verification checklist after each phase

### General verification

- Switch projects repeatedly without F5
- Tasks do not leak into the wrong project
- Draft restores into the correct project
- Task detail/subtask detail keeps correct parent context

### Date verification

- New tasks cannot choose past dates
- End date cannot be before start date
- Choosing 21-28 persists exactly as 21-28

### Subtask verification

- New subtask => backlog
- Hide/show subtasks works in both task detail and project view
- Inline edit for status/priority/assignee works

### Analytics verification

- Open analytics
- Expand analytics
- Close analytics
- Customized Insights dropdown works

### Rewards verification

- Complete a task
- Reward points are added
- Rewards page can display the transaction/history

## Conclusion

The full list of gaps can be handled by a single AI, but only if the work is done in the correct order.

The most effective order is:

1. project/data boundary
2. task/subtask workflow
3. date/timezone/cycle/timeline
4. analytics/rewards/display
5. performance/UX finishing

If a single AI starts fixing UI first without stabilizing backend/date/project-state behavior, many fixes will likely break again later.
