# DONE AI1

## Scope completed in this pass

- Fixed bug `1.1`
- Fixed bug `1.2`
- Fixed bug `1.3`
- Fixed bug `1.4`
- Fixed bug `1.5`
- Fixed bug `1.6`
- Fixed bug `1.7`
- Fixed bug `1.8`
- Fixed bug `1.9`
- Fixed bug `1.10`
- Fixed bug `1.11`
- Fixed bug `1.12`
- Fixed bug `1.13`
- Fixed bug `1.14`
- Fixed bug `1.15`
- Fixed bug `1.16`
- Fixed bug `1.17`
- Fixed bug `1.18`

## Changes made

### Backend

- `Backend/src/TaskManagement.API/Controllers/WorkTasksController.cs`
  - Updated the `PUT /api/projects/{projectId}/WorkTasks/{id}` endpoint to:
    - verify the task belongs to the `projectId` in the route
    - reject update requests that do not include `RowVersion`
  - This makes the full-update endpoint enforce optimistic concurrency instead of silently accepting overwrite-prone payloads.
  - Wired real business flows to AI-3 notification event endpoints:
    - `POST /api/notifications/events/task-assigned` when assignee changes via `PUT` or `PATCH`
    - `POST /api/notifications/events/task-status-changed` when status changes via direct status update, `PATCH`, or kanban reorder
  - Forwarded the current bearer token when posting internal notification events so the notification actor matches the real authenticated user action.

- `Backend/src/TaskManagement.API/Controllers/CommentsController.cs`
  - Reworked comment read/write mapping so comment reactions are stored persistently in DB using metadata embedded in `Comment.Content`.
  - Added `POST /api/projects/{projectId}/WorkTasks/{taskId}/comments/{commentId}/reactions`.
  - Added `DELETE /api/projects/{projectId}/WorkTasks/{taskId}/comments/{commentId}/attachments/{attachmentId}`.
  - Preserved reactions when editing comment text.
  - Returned normalized `content`, `reactions`, and `attachments` payloads for the frontend activity feed.
  - Wired `POST /api/notifications/events/comment-added` into the real comment creation flow.

### Frontend

- `Frontend/src/store/useWorkTaskStore.js`
  - Extended `updateTask()` so it can send either `PATCH` or `PUT` based on an options flag.

- `Frontend/src/views/SpaceSummary.vue`
  - Added logic to build a full `PUT` payload for core task-detail fields:
    - `title`
    - `description`
    - `priority`
    - `storyPoints`
    - `assignedUserId`
    - `plannedStartDate`
    - `plannedEndDate`
    - `dueDate`
    - `sprintId`
    - `taskTypeId`
  - Included `rowVersion` in that payload.
  - Kept partial-field updates on `PATCH` for fields handled by the partial update endpoint.
  - Hid subtasks from board/list by default and added a working display toggle for showing sub-work items.
  - Added inline list-view status controls inside `SpaceSummary.vue` so list view no longer depends on drag-and-drop to change status.
  - Added inline kanban card controls for status, priority, and assignee with `@click.stop` behavior so clicking controls does not open task detail accidentally.
  - Added `CANCELLED` kanban/list grouping so drag and drop can target that status as well as `IN REVIEW`.

- `Frontend/src/components/TaskDetailModal.vue`
  - Changed overlay close handling from `@click.self` to `@mousedown.self`.
  - Removed top-left extra icons and kept only the back button.
  - Fixed rich text toolbar interactions so formatting applies reliably to selected text in description/comment editors.
  - Added code-block toggle state for description and comment editors.
  - Removed the description toolbar attach button while keeping the separate task attachment action button.
  - Added relative “Last edited by” rendering.
  - Added audit-log loading and merged activity timeline entries with comments.
  - Added show/hide toggle for the subtask list inside task detail.
  - Filtered parent task selector to same-project top-level tasks only.
  - Added image lightbox zoom and delete support for comment attachments.
  - Extended comment attachment file picker to support PDF/Word/Excel-oriented file types.

## Verification

- Ran:

```powershell
dotnet build Backend\src\TaskManagement.API\TaskManagement.API.csproj
npm run build
```

- Result:
  - Build succeeded
  - Existing warnings remain in unrelated controllers:
    - `StickiesController.cs`
    - `AuditLogsController.cs`
  - Frontend build could not complete because of unrelated files outside AI-1 scope:
    - `Frontend/src/router/dashboardRoutes.js` unresolved import for `../views/RewardsView.vue`
    - `Frontend/src/components/CalendarTab.vue` existing syntax error near line 100
