# Prompt For A Single AI To Start Working

Copy the entire prompt below and send it to the AI that will execute the work.

---

You are the main integration AI for the repository `D:\A\QuanLyCongViec`. You have full authority over the whole repo and must work end-to-end. Do not stop at analysis only.

## Files you must read before making any changes

- `docs/SINGLE_AI_GAP_REPAIR_GUIDE.md`
- `docs/performance-optimization-report.md`
- `docs/ADMIN_PLANE_COMPARISON_REPORT.md`
- `docs/MASTER_FINAL_REPORT.md`
- `docs/Y3_205_TestCases_Results.md`

## Overall objective

You must:

1. Resolve the gaps in the exact priority order described in `docs/SINGLE_AI_GAP_REPAIR_GUIDE.md`
2. Prioritize Phase 1 and Phase 2 first:
   - project switching / project identity / draft-project mapping
   - OTP/email validation
   - task/subtask workflow
3. Cross-check the current admin area against `docs/ADMIN_PLANE_COMPARISON_REPORT.md`
4. If possible, add or scaffold the most important missing admin modules in a Plane-style direction, but adapted to this project
5. Build, test, and verify after each major fix group
6. Update the related documentation if a major gap group is closed

## Working rules

- Do not stop at analysis
- Read the file before editing it
- If you identify a clear bug or gap, fix the code directly
- The goal is to make the project run correctly, build correctly, and reduce real gaps
- Do not change the existing Markdown report formats unless absolutely necessary; if you update them, preserve the existing structure

## Mandatory execution order

### Phase 1: Project identity and data boundary

Fix first:

1. Switching projects must work without requiring F5
2. Abort stale requests when the user switches project/search/filter quickly
3. Tasks must only appear inside their own project
4. Drafts must be attached to and restored inside the correct project
5. Header/topbar/project logo/title must update correctly for the selected project
6. OTP/email validation:
   - invalid email format => do not send
   - login/reset flow => email must exist before sending
   - register flow => if email already exists, do not send

### Phase 2: Task/subtask workflow

1. Newly created subtasks must default to `BACKLOG`
2. Show/Hide subtask behavior must work correctly
3. Subtasks inside the parent task must display:
   - title
   - sequence/id
   - priority
   - status
   - assignee
4. Status/priority/assignee must be editable inline for subtasks inside the parent task
5. Subtask detail view must display the parent task name in the parent section
6. Add or complete progress % display and editing for both tasks and subtasks

### Phase 3: Date/timezone/cycle/timeline

1. New create forms must not allow selecting past dates
2. End date must never be before start date
3. Fix the 1-day shift bug (for example 21 becomes 20, 28 becomes 27)
4. Fix cycle status logic:
   - not-started cycles must not be shown as closed
   - closed cycles must continue to block task editing, which is correct
5. Fix timeline behavior:
   - Today
   - Create mode
   - Week / Month / Quarter
   - make them match expected business behavior

### Phase 4: Analytics/display/rewards/admin

1. Analytics expand button must not close the panel
2. `Customized Insights` must be reduced to a single clean dropdown button
3. Fix display ordering:
   - Manual
   - Last created
   - Last updated
   - Priority
   - remove Start date from display ordering
4. Fix rewards route/page so reward points can actually be viewed
5. Cross-check admin against `docs/ADMIN_PLANE_COMPARISON_REPORT.md`
6. If time allows, scaffold the highest-priority missing admin modules:
   - Instance General Settings
   - Authentication Management
   - Email/SMTP Management

## Technical requirements

- Frontend: Vue
- Backend: ASP.NET API
- Prioritize root-cause fixes, not shallow UI patches, if the bug really comes from state/API/date serialization
- If you find `toISOString()` used for date-only values, inspect it carefully because it may be the cause of date-shift bugs
- If task list APIs still return full lists with heavy payloads, account for their impact on project switching bugs and performance

## Required verification

After each phase you complete, run and record:

### Build

- Frontend:
  - `npm run build` inside `Frontend`
- Backend:
  - `dotnet build Backend/src/TaskManagement.API/TaskManagement.API.csproj`

### Minimum retest checklist

- project switching works without F5
- tasks do not leak into the wrong project
- newly created subtask = backlog
- hide/show subtasks works correctly
- dates are not shifted
- analytics expand works correctly
- rewards page is accessible if the route/page exists

## Expected output from the AI

The AI must report:

1. What was fixed
2. Which files were changed
3. Build/test results
4. Which gaps were closed
5. Which gaps are still open
6. If admin work was added, which modules were introduced

## Important notes

- Do not remove existing features unless there is a clear and correct replacement
- If a section is too large, complete Phase 1 before moving into Phase 2
- Do not skip project switching, task/project identity, or date timezone issues
- These are the 3 biggest root-cause clusters

Start immediately from Phase 1.

At the end of your work, report your final results in Vietnamese.

---
