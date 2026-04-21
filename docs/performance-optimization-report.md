# Performance Optimization Report

Ngay cap nhat: 2026-04-21

## 1. Frontend

### Van de de gap khi co 1000+ tasks

- Render qua nhieu row/card cung luc
- Loc/sort lai toan bo list tren moi re-render
- Request trung lap khi chuyen tab
- Payload lon lam mount cham va scroll lag

### De xuat

1. Virtual list cho Spreadsheet/List/Timeline
   - Dung `vue-virtual-scroller` cho row-based views
   - Chi render nhung row trong viewport

```vue
<RecycleScroller
  :items="tasks"
  :item-size="44"
  key-field="id"
  class="task-scroller"
>
  <template #default="{ item }">
    <TaskRow :task="item" />
  </template>
</RecycleScroller>
```

2. Lazy load tab/component nang
   - Timeline, Calendar, Spreadsheet, analytics chart nen dynamic import

```js
const TimelineTab = defineAsyncComponent(() => import('@/components/TimelineTab.vue'))
```

3. Cache request
   - Da ap dung mot phan cho sprint store TTL 30s
   - Nen mo rong cho modules, labels, project members

4. Pagination va chunk loading
   - Backend tra task theo page thay vi full list
   - Frontend yeu cau them khi user scroll / paginates

5. Memoized filtering
   - Tach `rawTasks`, `serverFilteredTasks`, `displayFilteredTasks`
   - Tranh filter nhieu tang lap lai tren tung component

## 2. Backend

### De xuat API

1. Pagination cho task listing

```csharp
[HttpGet]
public async Task<IActionResult> GetTasks(Guid projectId, int page = 1, int pageSize = 50)
{
    var query = _context.WorkTasks
        .AsNoTracking()
        .Where(x => x.ProjectId == projectId && !x.IsDeleted);

    var total = await query.CountAsync();

    var items = await query
        .OrderByDescending(x => x.UpdatedAt)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(x => new TaskListItemDto
        {
            Id = x.Id,
            Title = x.Title,
            StatusName = x.TaskStatus.Name,
            AssigneeId = x.AssignedUserId,
            CreatedAt = x.CreatedAt
        })
        .ToListAsync();

    return Ok(new { total, items });
}
```

2. DTO projection
   - Khong `Include()` du lieu khong can cho list page
   - Project thang sang DTO de giam RAM + SQL payload

3. Async all the way
   - Tat ca query / save / external AI call da nen giu `async/await`

4. Cache
   - `IMemoryCache` cho data doc nhieu: sprint list, modules, labels
   - Redis cho leaderboard, analytics snapshots, hot project dashboard

```csharp
if (_cache.TryGetValue(cacheKey, out SprintDto[] cached))
{
    return cached;
}
```

## 3. Database

### Index de xuat

```sql
CREATE INDEX IX_WorkTasks_ProjectId_Status_Assignee_CreatedAt
ON WorkTasks(ProjectId, TaskStatusId, AssignedUserId, CreatedAt DESC);

CREATE INDEX IX_WorkTasks_ProjectId_UpdatedAt
ON WorkTasks(ProjectId, UpdatedAt DESC);

CREATE INDEX IX_WorkTasks_ProjectId_SprintId
ON WorkTasks(ProjectId, SprintId);
```

### N+1 query

- Tranh loop xong moi query assignee/module/status tung task
- Dung projection gom mot lan
- Dung aggregate query cho dashboard/leaderboard thay vi query ben trong `foreach`

## 4. Benchmark de xuat

### Scenario

- 1000, 5000, 10000 tasks / project
- 100 concurrent users
- Workload:
  - mo board
  - search/filter
  - update status
  - timeline drag
  - calendar load

### Chi so can do

- P50 / P95 response time
- SQL query count per request
- Memory usage API
- Time to interactive o frontend
- Scroll FPS cho list/timeline

### Cong cu

- Backend: `bombardier`, `k6`, `dotnet-counters`
- Frontend: Chrome Performance, Lighthouse, Vue Devtools
- Database: Query Store / execution plan

## 5. Uu tien cai thien

1. Them pagination task API
2. Them DTO projection cho task listing
3. Virtual list cho Spreadsheet/List
4. Cache danh muc docs/sprints/modules
5. Index SQL cho `ProjectId + Status + Assignee + CreatedAt`
6. Tach analytics nhe/summary API khoi task full payload

## 6. Ghi chu AI-5

- Trong pham vi AI-5 da bat dau toi uu bang cache cho sprint store
- Cac de xuat con lai can phoi hop voi owner cua `WorkTasksController`, data layer va main task store de dat hieu qua lon nhat
