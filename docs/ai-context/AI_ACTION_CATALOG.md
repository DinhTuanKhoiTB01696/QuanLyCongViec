# SprintA AI Action Catalog

Backend owner: CODEX.

## Execution rules

- Gemini may only suggest actions. It never executes data changes directly.
- `POST /api/ai/context-chat` returns structured previews in `data.actions`.
- `POST /api/ai/actions/execute` is the only action execution endpoint.
- Write actions require user confirmation in UI.
- Read-only tools do not require confirmation and return database-backed snapshots.
- All execute calls use server-side whitelist, permission checks, validation, audit logs, and idempotency.
- Destructive actions are not supported: delete project/task/goal, remove members, permission changes, OAuth disconnect, raw SQL, database reset, bulk destructive updates.

## Write actions

| Type | Entity | Required context | Required payload | Permission | Confirmation | Executor |
| --- | --- | --- | --- | --- | --- | --- |
| `create_project` | Project | workspace access | `name` | active workspace member, default runtime workspace | yes | `AiController.ExecuteCreateProjectAsync` |
| `create_task` | WorkTask | project access | `projectId`, `title` | project context access | yes | `AiController.ExecuteCreateTaskAsync` |
| `create_cycle` | Sprint | project access | `projectId`, `name`, optional `startDate`, `endDate` | project context access | yes | `AiController.ExecuteCreateCycleAsync` |
| `create_module` | Module | project access | `projectId`, `name` | project context access; lead must be member | yes | `AiController.ExecuteCreateModuleAsync` |
| `create_page` | Page | project access | `projectId`, `title` | project context access | yes | `AiController.ExecuteCreatePageAsync` |
| `create_view` | ProjectView | project access | `projectId`, `name`, optional JSON `queryMetadata` | project context access | yes | `AiController.ExecuteCreateViewAsync` |
| `create_intake_request` | Intake | project access | `projectId`, `title` | project context access | yes | `AiController.ExecuteCreateIntakeRequestAsync` |
| `update_task_status` | WorkTask | task access | `taskId`, `statusName` or `taskStatusId` | project context access | yes | `AiController.ExecuteUpdateTaskStatusAsync` |
| `update_task_priority` | WorkTask | task access | `taskId`, `priority` | project context access | yes | `AiController.ExecuteUpdateTaskPriorityAsync` |
| `update_task_due_date` | WorkTask | task access | `taskId`, `dueDate` | project context access | yes | `AiController.ExecuteUpdateTaskDueDateAsync` |
| `assign_task` | WorkTask | task access | `taskId`, `assigneeId` | reporter or PM/PO/SM/Admin; assignee must be active member | yes | `AiController.ExecuteAssignTaskAsync` |
| `add_comment` | Comment | entity access | `entityType`, `entityId`, `content` | project/workspace access for target entity | yes | `AiController.ExecuteAddCommentAsync` |
| `create_goal` | Goal | workspace access | `title`, optional `workspaceId` | active workspace member | yes | `AiController.ExecuteCreateGoalAsync` |

## Read-only tools

| Type | Result | Required context | Permission | Confirmation | Executor |
| --- | --- | --- | --- | --- | --- |
| `summarize_dashboard` | workspace task summary | `workspaceId` optional | workspace access | no | `AiController.ExecuteSummarizeDashboardAsync` |
| `summarize_project` | project summary | `projectId` | project context access | no | `AiController.ExecuteSummarizeProjectAsync` |
| `list_overdue_tasks` | overdue tasks | `projectId` | project context access | no | `AiController.ExecuteListOverdueTasksAsync` |
| `get_workload` | member workload | `projectId` | project context access | no | `AiController.ExecuteGetWorkloadAsync` |
| `explain_report` | report snapshot | `projectId` | project context access | no | `AiController.ExecuteExplainReportAsync` |
| `summarize_page` | page preview | `pageId` | project context access for page project | no | `AiController.ExecuteSummarizePageAsync` |
| `summarize_intakes` | intake status counts | `projectId` | project context access | no | `AiController.ExecuteSummarizeIntakesAsync` |
| `suggest_view_filter` | suggested filter payload | `projectId` | project context access | no | `AiController.ExecuteSuggestViewFilterAsync` |

## Idempotency

- Client may send `idempotencyKey`.
- Without a client key, backend hashes user id, action type, and ordered payload.
- Successful execution writes `SystemAuditLog` with action `AI_ACTION_EXECUTE`.
- Repeated successful keys replay the stored result and set `idempotentReplay=true`.

## Test cases

- Create cycle/module/page/view/intake and confirm entity remains after reload.
- Update task status/priority/due date and verify task row changes.
- Assign task to active member; reject non-member.
- Add comment to WorkTask/Project/Goal with access guard.
- Read-only tools return real DB snapshots and create no domain entity.
- User without project/workspace access receives 403.
- Double confirm returns one entity through idempotency replay.
