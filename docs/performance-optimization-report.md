# Performance Optimization Report

Last updated: 2026-04-21

## Scope reviewed

- Frontend: `Frontend/src/views/SpaceSummary.vue`, `Frontend/src/components/TimelineTab.vue`, `Frontend/src/components/SpreadsheetTab.vue`, `Frontend/src/store/useProjectStore.js`, router files inside `Frontend/src/router`
- Backend: `Backend/src/TaskManagement.API/Controllers/WorkTasksController.cs`, `Backend/src/TaskManagement.API/Controllers/ProjectsController.cs`, `Backend/src/TaskManagement.Infrastructure/Services/WorkTaskService.cs`
- Database config: `Backend/src/TaskManagement.Infrastructure/Data/ApplicationDbContext.cs`

This report is based on the actual current codebase, not generic theory.

## 1. Current-state analysis

### 1.1 Frontend

#### Task list rendering

- The main task UI currently renders directly with `v-for` inside `SpaceSummary.vue` for:
  - list view
  - board view
  - analytics grouping
- `TimelineTab.vue` also renders the full `visibleTasks`
- `SpreadsheetTab.vue` has client-side pagination, but it still receives the full `tasks` array first and only slices afterward
- Only `DraftsView.vue` currently has a manual virtual scrolling pattern; the main task views do not use virtualization

Conclusion:

- If a project has 1000+ tasks, the frontend will mount too many DOM nodes at once
- Board/List/Timeline are the 3 biggest rendering bottlenecks

#### Lazy load / code splitting

- The router already uses dynamic import for many routes:
  - `dashboardRoutes.js`
  - `spaceRoutes.js`
  - `authRoutes.js`
  - `adminRoutes.js`
- This is a positive point: route-level code splitting already exists
- However, inside `SpaceSummary.vue`, heavy tabs such as board/list/timeline/analytics are still bundled into one large page-level workload

Conclusion:

- Route-level code splitting exists
- Heavy project tabs are not yet strongly lazy-loaded at the component level

#### Data caching

- `useProjectStore.js` provides basic in-memory caching through Pinia state (`allProjects`, `currentProject`)
- No TanStack Query / Vue Query was found
- No structured query-key invalidation strategy was found
- No stale-while-revalidate behavior was found for task lists

Conclusion:

- Current caching is only lightweight state caching, not enough for a 100-concurrent-user scenario

#### Request cancellation during search/filter/switching

- No `AbortController` was found
- No `controller.abort()` was found
- No `signal:` usage was found in frontend API calls

Conclusion:

- Search/filter/project switching can suffer from race conditions
- This strongly matches the real bug where switching projects needs F5 before data becomes correct

#### Pagination / infinite scroll

- `SpreadsheetTab.vue` has UI pagination, but it is client-side only
- `DraftsView.vue` has server-side pagination
- The main project task list API still returns the full list
- No infinite scroll was found for the main task views

Conclusion:

- The 1000+ tasks problem is not yet solved in the main work-item flows

### 1.2 Backend

#### Task list APIs

Main endpoint:

- `GET /api/projects/{projectId}/WorkTasks` inside `WorkTasksController.cs`
- This calls `WorkTaskService.GetByProjectAsync(projectId, userId)`

Current state:

- No `page`, `pageSize`, or `cursor`
- Loads the full task list of a project
- It already uses `Select(...)` into DTOs, which is good
- But the list DTO is still relatively heavy:
  - `Description`
  - `Assignees`
  - `LabelIds`
  - `ModuleId`
  - `RowVersion`
  - `PlannedStartDate`, `PlannedEndDate`, `DueDate`
  - `ReporterName`, `AssigneeName`

Conclusion:

- The list API currently behaves almost like both list and detail at the same time
- This is a critical bottleneck for board/list/timeline

#### Async/await usage

- Backend async/await usage is fairly consistent
- This is not currently the main performance bottleneck

#### DTO / payload design

- DTO projection exists in `WorkTaskService`
- But the list payload is still too heavy for "task card / task row / timeline row" use cases

Conclusion:

- DTO projection is the right direction
- But the code still needs a dedicated `TaskListItemDto` vs `TaskDetailDto` split

#### Backend caching

- `Program.cs` and service registration already use `AddMemoryCache()`
- `OtpService` uses `IMemoryCache`
- There are also middlewares such as `PerformanceMiddleware` and `IpWhitelistMiddleware`
- But no meaningful cache was found for task lists / project analytics / board summaries
- No Redis usage was found

Conclusion:

- Basic memory cache infrastructure exists, but it is not yet applied to the heaviest read paths

### 1.3 Database

#### Existing WorkTask indexes

In `ApplicationDbContext.cs`, the following already exist:

```csharp
modelBuilder.Entity<WorkTask>().HasIndex(wt => new { wt.ProjectId, wt.IsDeleted });
modelBuilder.Entity<WorkTask>().HasIndex(wt => wt.ReporterId);
modelBuilder.Entity<WorkTask>().HasIndex(wt => wt.AssignedUserId);
modelBuilder.Entity<WorkTask>().HasIndex(wt => new { wt.WorkspaceId, wt.ProjectId });
modelBuilder.Entity<WorkTask>().HasIndex(wt => wt.SortOrder);
```

Evaluation:

- Basic indexes exist
- But there are no stronger composite indexes for common access patterns such as:
  - `ProjectId + TaskStatusId + IsDeleted`
  - `ProjectId + AssignedUserId + IsDeleted`
  - `ProjectId + CreatedAt`
  - `ProjectId + UpdatedAt`
  - `ProjectId + ParentTaskId`
  - `ProjectId + SprintId`

#### Query shape

`GetByProjectAsync` currently:

- uses many `Include(...)`
- then projects with `Select(...)`
- EF Core may optimize part of this, but with 1000+ tasks and many relationships, it is still a heavy query

`SearchTasksAsync` currently:

- loads the user's project IDs
- then queries tasks with many `Include(...)`
- still has no pagination

Main risks:

- heavy payload
- complex SQL shape
- increased API memory use when many tasks are loaded at once

#### N+1 queries

- Many services already use projection to avoid obvious N+1 issues
- However, the bigger issue here is not classic N+1
- It is over-fetching + no pagination + no virtualization

Conclusion:

- The primary bottleneck is not a pure N+1 problem
- It is the combination of:
  - over-fetching
  - no pagination
  - full rendering on the frontend

## 2. Problem classification

### 🔴 Critical

1. The main project task list API has no pagination
   - `GET /api/projects/{projectId}/WorkTasks`
   - Directly impacts board/list/timeline

2. Frontend renders the full task list inside `SpaceSummary.vue` and `TimelineTab.vue`
   - 1000+ tasks will cause lag during mount, filtering, and drag-drop

3. No request cancellation when users switch project, search, or filter quickly
   - This causes stale UI and old project data overwriting the new one

4. Task list payload is too heavy
   - The list API currently carries detail-level fields

5. No strong composite indexes for common project/status/assignee/date filters

### 🟠 Medium

1. Analytics and summary data are not split into lighter dedicated APIs
2. Heavy project tabs are not lazy-loaded enough inside `SpaceSummary.vue`
3. Spreadsheet pagination is only client-side
4. Backend cache is not applied to heavy metadata reads
5. Project switching does not yet preload data on hover

### 🟢 Minor

1. Route-level code splitting already exists, so the app is not in the worst possible state
2. Backend async/await usage is generally solid
3. Some baseline indexes already exist

## 3. Optimization recommendations

### 3.1 Frontend

#### A. Add virtual list for List view and Board view

Apply first to:

- `SpaceSummary.vue` list view
- board column cards
- timeline rows

Suggested direction:

- Use `vue-virtual-scroller`
- Or implement a small custom windowing layer if dependency count must stay low

Example:

```vue
<RecycleScroller
  :items="group.items"
  :item-size="44"
  key-field="id"
  class="task-scroller"
>
  <template #default="{ item }">
    <TaskRow :task="item" @click="openTaskDetail(item)" />
  </template>
</RecycleScroller>
```

#### B. Split heavy tabs into async components

Current state:

- `SpaceSummary.vue` is a very large view

Suggested direction:

```vue
<script setup>
import { defineAsyncComponent } from 'vue'

const AsyncTimelineTab = defineAsyncComponent(() => import('@/components/TimelineTab.vue'))
const AsyncSpreadsheetTab = defineAsyncComponent(() => import('@/components/SpreadsheetTab.vue'))
const AsyncAnalyticsPanel = defineAsyncComponent(() => import('@/components/ProjectAnalyticsPanel.vue'))
</script>
```

Benefits:

- reduces initial JS loaded for the project page
- improves first-open performance for projects

#### C. Add query caching and stale-while-revalidate

Suggested use of TanStack Query for:

- task list by project + filters
- project members
- labels
- cycles
- analytics summary

Example:

```ts
const query = useQuery({
  queryKey: ['project-tasks', projectId, filters],
  queryFn: () => fetchProjectTasks(projectId, filters),
  staleTime: 30_000,
  gcTime: 5 * 60_000
})
```

#### D. Abort requests during search/filter/project switching

Example:

```ts
let controller: AbortController | null = null

async function fetchProjectTasks(projectId, params) {
  controller?.abort()
  controller = new AbortController()

  return axiosClient.get(`/projects/${projectId}/WorkTasks`, {
    params,
    signal: controller.signal
  })
}
```

Benefits:

- prevents old responses from overwriting the new project state
- directly addresses the "must press F5 after switching project" issue

#### E. Prefetch project data on hover

Suggested behavior:

- When hovering a project in the sidebar:
  - prefetch project details
  - prefetch task list page 1 summary
  - prefetch members/statuses

Pseudo-example:

```ts
const handleProjectHover = (projectId) => {
  queryClient.prefetchQuery({
    queryKey: ['project-summary', projectId],
    queryFn: () => fetchProjectSummary(projectId)
  })
}
```

### 3.2 Backend

#### A. Add pagination for task list APIs

Recommended approach:

- Add a new list-focused API instead of overloading the current detail-rich one

```csharp
[HttpGet("projects/{projectId}/work-items")]
public async Task<IActionResult> GetProjectTasks(
    Guid projectId,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 50,
    [FromQuery] string? status = null,
    [FromQuery] Guid? assigneeId = null,
    [FromQuery] string? orderBy = "updatedAt")
{
    var query = _context.WorkTasks
        .AsNoTracking()
        .Where(t => t.ProjectId == projectId && !t.IsDeleted && !t.IsArchived);

    if (!string.IsNullOrWhiteSpace(status))
        query = query.Where(t => t.TaskStatus.Name == status);

    if (assigneeId.HasValue)
        query = query.Where(t => t.AssignedUserId == assigneeId.Value);

    var total = await query.CountAsync();

    var items = await query
        .OrderByDescending(t => t.UpdatedAt)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(t => new TaskListItemDto
        {
            Id = t.Id,
            SequenceId = t.SequenceId,
            Title = t.Title,
            ProjectId = t.ProjectId,
            TaskStatusId = t.TaskStatusId,
            StatusName = t.TaskStatus.Name,
            Priority = t.Priority,
            AssignedUserId = t.AssignedUserId,
            PlannedStartDate = t.PlannedStartDate,
            DueDate = t.DueDate,
            SortOrder = t.SortOrder,
            UpdatedAt = t.UpdatedAt,
            ParentTaskId = t.ParentTaskId
        })
        .ToListAsync();

    return Ok(new
    {
        data = items,
        pagination = new
        {
            page,
            pageSize,
            total,
            totalPages = (int)Math.Ceiling(total / (double)pageSize)
        }
    });
}
```

#### B. Split list DTO and detail DTO

Recommended model:

- `TaskListItemDto`: for board/list/timeline
- `TaskDetailDto`: for `TaskDetailModal`
- `TaskAnalyticsDto`: for analytics summary

Benefits:

- smaller payloads
- simpler SQL shape
- lower JSON serialization cost

#### C. Add dedicated analytics summary APIs

Analytics currently appears to depend too much on full task datasets. Recommended split:

- `/projects/{projectId}/analytics/summary`
- `/projects/{projectId}/analytics/by-status`
- `/projects/{projectId}/analytics/by-priority`

These should return only aggregate data, not the full task list.

#### D. Consider cursor pagination for board/timeline if needed

For large boards:

- page-based pagination is fine for list views
- cursor-based pagination may be better for infinite-scroll and reorder-heavy views

### 3.3 Database

#### A. Add targeted indexes

```sql
CREATE INDEX IX_WorkTasks_ProjectId_TaskStatusId_IsDeleted
ON dbo.WorkTasks(ProjectId, TaskStatusId, IsDeleted);

CREATE INDEX IX_WorkTasks_ProjectId_AssignedUserId_IsDeleted
ON dbo.WorkTasks(ProjectId, AssignedUserId, IsDeleted);

CREATE INDEX IX_WorkTasks_ProjectId_CreatedAt
ON dbo.WorkTasks(ProjectId, CreatedAt DESC);

CREATE INDEX IX_WorkTasks_ProjectId_UpdatedAt
ON dbo.WorkTasks(ProjectId, UpdatedAt DESC);

CREATE INDEX IX_WorkTasks_ProjectId_ParentTaskId
ON dbo.WorkTasks(ProjectId, ParentTaskId);

CREATE INDEX IX_WorkTasks_ProjectId_SprintId_IsDeleted
ON dbo.WorkTasks(ProjectId, SprintId, IsDeleted);
```

Why these matter:

- They match the real access patterns already present in the codebase
- They strengthen the existing baseline indexes

#### B. Inspect execution plans for task list queries

Must review:

- project with 1000 tasks
- filtered by assignee
- filtered by status
- timeline/date-range cases

Goal:

- avoid repeated key lookups
- avoid large scans on `WorkTasks`

### 3.4 Caching

#### Use Redis or distributed cache for:

- analytics summary by project
- leaderboard / rewards snapshot
- frequently read metadata:
  - project members
  - labels
  - task statuses
  - task types
  - cycles

#### Cache only data that is:

- read frequently
- changed less often than active task detail

Do not aggressively hard-cache highly volatile task detail unless invalidation is well designed.

## 4. Proposed architecture

### Text diagram

```text
Vue Project Page
  -> Query Layer (TanStack Query / Pinia orchestration)
    -> ASP.NET API
      -> Application Service
        -> EF Core Query Projection
          -> SQL Server
      -> Redis / MemoryCache
```

### Recommended flow

#### Load task list

```text
Open project
  -> fetch project summary
  -> fetch task list page 1 (light DTO)
  -> render virtual list
  -> prefetch next page when near bottom
```

#### Load more on scroll

```text
User scrolls
  -> Intersection Observer triggers
  -> fetch page 2/page 3
  -> append rows
  -> DOM stays small because virtualization is active
```

#### Open task detail

```text
Click task
  -> fetch task detail by id
  -> open TaskDetailModal
  -> lazy-load comments / relations / activity tabs
```

#### Realtime task updates

```text
User updates task
  -> optimistic UI update
  -> PATCH/PUT API
  -> invalidate:
     - task detail
     - current task list page
     - analytics summary
```

## 5. Estimated improvement

If the highest-priority changes are implemented, the system can reasonably expect:

- Task list initial payload reduction: 40% - 75%
- Simultaneous DOM node reduction: 80% - 95%
- Faster project open time on large projects: 35% - 60%
- Reduced lag during filter/sort/search: 50%+
- Backend task-list capacity improvement: 2x - 4x depending on final query/index design
- Major reduction in duplicate or stale project-switch requests once abort + query cache are added

## 6. Before vs after optimization

### Before

```text
Frontend:
- open project -> fetch full tasks
- render full board/list/timeline
- no request aborting

Backend:
- list API returns heavy payload
- no pagination
- no dedicated analytics summary API
```

### After

```text
Frontend:
- open project -> fetch page 1 + required metadata only
- virtual rendering
- prefetch on hover
- abort stale requests when switching project quickly

Backend:
- light list API with pagination
- separate detail API
- analytics summary becomes cacheable
```

## 7. Immediate implementation roadmap

1. Add paginated task list API + light DTO
2. Make `SpaceSummary.vue` consume the new list API
3. Add virtualization to list view first
4. Add `AbortController` to project switch / search / filter requests
5. Prefetch project data on sidebar hover
6. Split analytics into aggregate APIs
7. Add the SQL indexes listed above

## 8. Final conclusion

The codebase already has a decent foundation:

- async/await is used fairly consistently
- DTO projection exists
- route-level lazy loading exists
- baseline indexes already exist

However, for the target of `1000+ tasks` and `~100 concurrent users`, the system still has 3 main bottlenecks:

1. the main task list API is not paginated
2. the frontend renders too many tasks at once
3. request cancellation / project-data prefetching is missing

If only one improvement cluster can be prioritized first, it should be:

- paginated task list
- light list DTO
- virtualized list

This combination will produce the biggest immediate gain and fits the current codebase with the least disruption.
