# Plane Project & Dashboard Architecture (ASP.NET Core + Vue.js)

Báo cáo này tài liệu hóa quá trình chuyển đổi toàn bộ luồng logic của **Project** và chức năng **Dashboard** từ gốc Plane (Next.js/Django) sang hệ sinh thái công nghệ mới: **Vue.js + ASP.NET Core C#**.

Dựa vào cấu trúc UI sidebar (Work items, Cycles, Modules, Views, Pages) và kiến trúc Database C#, dưới đây là chi tiết Workflow và Code thiết kế cho hệ thống AI Agent hiểu.

---

## 1. Bản đồ Map chức năng (Plane -> .NET/Vue.js)

| Giao diện gốc (Plane) | Frontend (Vue.js Router) | Backend Entity (.NET C#) | Controller C# |
| :--- | :--- | :--- | :--- |
| **Project Dashboard** | `/workspace/:wId/project/:pId` | Tổng hợp từ nhiều bảng | `ProjectsController.GetAnalytics` |
| **Work items** (Issues) | `/workspace/:wId/project/:pId/issues`| `WorkTask` (TaskItem) | `WorkTasksController` |
| **Cycles** (Sprints) | `/workspace/:wId/project/:pId/cycles`| `Sprint` | `SprintsController` |
| **Modules** | `/workspace/:wId/project/:pId/modules`| `Module` | `ModulesController` |
| **Views** | `/workspace/:wId/project/:pId/views`| Custom Filters / Saved Views | `ViewsController` |
| **Pages** | `/workspace/:wId/project/:pId/pages`| `Page` | `PagesController` |

---

## 2. Kiến trúc Data & API Backend (ASP.NET Core)

### 2.1 Project Dashboard Logic
Trang Dashboard của một dự án (Project) trong Plane mang tính chất hiển thị thống kê. 
**Logic cần có:**
- Thông tin dự án (Name, Identifier/CYBWF, Cover image).
- Báo cáo số lượng `WorkTask` theo trạng thái (Backlog, To Do, In Progress, Done).
- Hiển thị Members tham gia dự án (`ProjectMember`).

### 2.2 Logic các nhóm dữ liệu gốc (Project Core Features)
1. **Work items (Task/Issue):** Backend quản lý trạng thái, ưu tiên (Priority), người được gán (Assignees).
2. **Cycles (Chu kỳ/Sprints):** Quản lý khoảng thời gian (StartDate, EndDate). Khi Frontend yêu cầu lấy Work items trong Cycle, Backend sẽ tìm các task liên kết ID của Sprint.
3. **Modules (Tổ hợp tính năng):** Đóng vai trò như các thẻ Epic gom nhóm Task dài hạn không bó buộc thời gian cứng dắt (Khác với Cycle).
4. **Pages:** Trình soạn thảo Rich Text (tiêu chuẩn chuyển sang lưu dạng JSON Markdown hoặc HTML).

---

## 3. Kiến trúc Frontend (Vue.js)

Kiến trúc UI được tái tạo với Vue 3 + Pinia (thay thế cho MobX/Redux) và Tailwind CSS.

### 3.1 Cấu trúc Context / Store (Pinia)
```typescript
import { defineStore } from 'pinia';
import apiClient from '@/utils/apiClient';

export const useProjectStore = defineStore('project', {
  state: () => ({ currentProject: null, workItems: [], loading: false }),
  actions: {
    async fetchWorkItems(projectId: string) {
      this.loading = true;
      const res = await apiClient.get(`/api/projects/${projectId}/WorkTasks`);
      this.workItems = res.data.data;
      this.loading = false;
    }
  }
});
```

### 3.2 Vue Router Setup (Hệ thống điều hướng)
```typescript
const routes = [
  {
    path: '/:workspaceSlug',
    component: WorkspaceLayout,
    children: [
      {
        path: 'projects/:projectId',
        component: ProjectLayout, // Chứa sidebar như hình user cung cấp
        children: [
          { path: '', component: ProjectDashboard },
          { path: 'issues', component: ViewsWorkItems },
          { path: 'cycles', component: ViewsCycles },
          { path: 'modules', component: ViewsModules },
          { path: 'views', component: ViewsCustom },
          { path: 'pages', component: ViewsPages },
        ]
      }
    ]
  }
];
```

### 3.3 Thiết kế Sidebar Component (Layout Vue)
```html
<template>
  <aside class="w-64 bg-[#1e1e1e] text-white flex flex-col h-full border-r border-gray-800">
    <div class="px-4 py-3 flex items-center gap-3 border-b border-gray-800">
      <span class="text-2xl rounded">👆</span>
      <h2 class="font-semibold text-sm">{{ project.identifier }}</h2>
    </div>
    <nav class="flex-1 py-4 px-2 space-y-1">
      <RouterLink :to="`/w/${workspaceId}/projects/${projectId}/issues`" class="menu-item group" active-class="active"><span>Work items</span></RouterLink>
      <RouterLink :to="`/w/${workspaceId}/projects/${projectId}/cycles`" class="menu-item group" active-class="active"><span>Cycles</span></RouterLink>
      <RouterLink :to="`/w/${workspaceId}/projects/${projectId}/modules`" class="menu-item group" active-class="active"><span>Modules</span></RouterLink>
      <RouterLink :to="`/w/${workspaceId}/projects/${projectId}/views`" class="menu-item group" active-class="active"><span>Views</span></RouterLink>
      <RouterLink :to="`/w/${workspaceId}/projects/${projectId}/pages`" class="menu-item group" active-class="active"><span>Pages</span></RouterLink>
    </nav>
  </aside>
</template>
```

---

## 4. Bóc Tách Chi Tiết Toàn Bộ Chức Năng Trong Project

Khi người dùng điều hướng vào một Project cụ thể (Ví dụ: **CYBWF**), họ sẽ tiếp cận hệ sinh thái quản lý vòng đời phát triển dự án. Toàn bộ logic này yêu cầu `.NET Backend` cung cấp cấu trúc trả về tương ứng và `Vue.js` dùng các Component UI/UX nâng cao.

### 4.1. Dashboard (Trang Bảng Điều Khiển Tổng Quan)
* **Thông số Thống kê (Metrics Overview):** Đếm tổng số Work items, phân bổ phần trăm theo trạng thái (Backlog, Todo, In Progress, Done), số đầu việc quá hạn (Overdue/Blocker).
* **Overview Widget:** Biểu đồ hiển thị khối lượng công việc hoặc số task hoàn thành trong 7 ngày gần nhất.
* **Luồng hoạt động (Activity Stream):** Dòng thời gian lịch sử thao tác của các thành viên (Ai tạo thẻ, Ai kéo thả trạng thái, đổi Assignees).
* **Danh sách Thành viên:** Widget nhỏ liệt kê Project Members cùng vai trò cụ thể.

**🚀 Code Tại Dự Án Của Bạn (C# - ProjectsController.cs):**
```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetById(Guid id)
{
    var project = await _projectService.GetByIdAsync(id);
    if (project == null) return NotFound(ApiResponse<object>.Error("Dự án không tồn tại.", 404));

    return Ok(ApiResponse<ProjectResponseDto>.Success(project));
}
```

### 4.2. Work items (Quản Trị Công Việc - Issues / Tasks)
Đây là Core chính yếu của hệ thống.
* **Quản trị CRUD:** Khởi tạo, chỉnh sửa (Tiêu đề, Rich-text Description, Labels, Priority, Cột trạng thái, Assignees).
* **Sub-tasks (Tác vụ con):** Hệ thống cây (Parent / Child hierarchy), phân chia công việc lớn thành module gốc dỡ nhỏ.
* **Hệ thống Layout Board Tùy chọn (View Modes):** Quản lý View State của user trên LocalStorage để duy trì thao tác (Như đúng bức hình giao diện bạn đưa ra):
  - ≡ **List View:** Hiển thị 3 gạch ngang. Mô hình dạng danh sách dọc cơ bản, tối ưu cho bulk action và sắp xếp theo nhóm trạng thái.
  - ◫ **Board / Kanban View:** Biểu tượng các cột dọc. Triển khai kéo-thả (Drag & Drop) siêu việt qua các cột Backlog -> Todo -> In Progress -> Done.
  - 📅 **Calendar View:** Icon tờ lịch. Dàn xếp issue theo trường `StartDate` và `DueDate`.
  - 🪟 **Spreadsheet / Table View:** Icon bảng phân ô. Giống Excel, hỗ trợ gõ phím di chuyển, sửa giá trị thẳng trên các cell mà không cần mở popup.
  - 📊 **Gantt / Timeline View:** Icon các thanh ngang bậc thang. Biểu diễn sự kế thừa và chặn nhau giữa các task thông qua giao diện tuyến tính trượt theo mốc thời gian.
  - 🎯 **Filter Toggle:** Nút cuối cùng nằm tách biệt để bật/tắt menu lọc nâng cao (cùng vạch báo active màu xanh khi đang áp dụng một điều kiện lọc nào đó).
* **Tương tác & Lịch sử (Comments & Audit logs):** Thảo luận theo tiểu ban thời gian thực, lưu trữ mọi thay đổi từng field dữ liệu trong thẻ.

**🚀 Code Tại Dự Án Của Bạn (C# Reorder Task cho giao diện Kéo Thả Kanban - WorkTasksController.cs):**
```csharp
[HttpPut("projects/{projectId}/WorkTasks/{id}/reorder")]
public async Task<IActionResult> Reorder(Guid projectId, Guid id, [FromBody] ReorderTaskDto dto, [FromServices] TaskManagement.Infrastructure.Data.ApplicationDbContext context)
{
    var task = await context.WorkTasks.FirstOrDefaultAsync(wt => wt.Id == id && !wt.IsDeleted);
    task.SortOrder = dto.SortOrder;
    task.UpdatedAt = DateTime.UtcNow;

    // Kéo thả giữa các cột, cập nhật Trạng thái Task
    if (!string.IsNullOrEmpty(dto.NewStatusName))
    {
        var newStatus = await context.TaskStatuses.FirstOrDefaultAsync(ts => ts.ProjectId == projectId && ts.Name == dto.NewStatusName);
        if (newStatus != null) task.TaskStatusId = newStatus.Id;
    }
    await context.SaveChangesAsync();
    return Ok(new { statusCode = 200, message = "Cập nhật thứ tự thành công." });
}
```

### 4.3. Cycles (Sprints / Chu kỳ Vòng Lặp Phát Triển)
* **Khái niệm:** Lấy ý tưởng từ Agile Sprint. Giới hạn một scope thời gian cố định (Ví dụ: Sprint 2 tuần).
* **Tạo và Cấu hình Cycle:** Tên, Thời gian Start/End, Mục tiêu (Goals).
* **Báo cáo Tiến độ Thời gian Thực (Burn-down/Burn-up Chart):** Đo lường trực quan tốc độ tiêu hao các Issue so với thời gian vòng lặp Cycle lý tưởng trên biểu đồ tuyến tính.
* **Rollover Uncompleted work:** Chức năng chuyển tiếp (Push) những task dở dang sang Cycle hoặc Sprint rỗng của tháng tới nếu Cycle hiện tại bị End (đóng).

**🚀 Code Tại Dự Án Của Bạn (C# Đóng Sprint và Đo Lường Burndown - SprintsController.cs):**
```csharp
[HttpPost("{id}/close")]
public async Task<IActionResult> Close(Guid projectId, Guid id, [FromBody] CloseSprintDto dto)
{
    await _sprintService.CloseAsync(id, dto);
    return Ok(ApiResponse<object>.Success(null!, "Sprint đã được đóng. Các task chưa hoàn thành đã được chuyển."));
}

[HttpGet("{id}/burndown")]
public async Task<IActionResult> GetBurndown(Guid projectId, Guid id)
{
    var result = await _sprintService.GetBurndownChartAsync(id);
    return Ok(ApiResponse<List<BurndownDataDto>>.Success(result));
}
```

### 4.4. Modules (Epic / Quản lý Nhóm Phân Hệ Chức Năng Dài Hạn)
* **Khái niệm:** Tương tự cấp độ "Epic" (ví dụ: Chức năng Đăng nhập, Chức năng Thanh Toán). Nhóm các Work item đa cấu trúc, không bị bó hẹp thời gian khắt khe.
* **Khả năng chức năng:** Có người dẫn đầu (Lead/Owner) ấn định. Có theo dõi độc lập các module xem cái nào `Planned`, `In Progress`, `Completed`. Tỉ lệ dứt điểm phần trăm dự án (Progress %) hoàn toàn thể hiện ra.

**🚀 Code Tại Dự Án Của Bạn (C# Thêm Module Mới - ModulesController.cs):**
```csharp
[HttpPost("modules")]
public async Task<IActionResult> Create(Guid projectId, [FromBody] CreateModuleRequest request)
{
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (!Guid.TryParse(userId, out Guid parsedUserId)) return Unauthorized(new { statusCode = 401, message = "Vui lòng đăng nhập." });

    var module = new Module
    {
        Id = Guid.NewGuid(), ProjectId = projectId, Name = request.Name, Description = request.Description,
        StartDate = request.StartDate, TargetDate = request.TargetDate, Status = "Backlog", LeadId = parsedUserId,
        CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
    };

    _context.Modules.Add(module);
    await _context.SaveChangesAsync();
    return CreatedAtAction(nameof(GetByProject), new { projectId }, new { statusCode = 201, data = new { module.Id, module.Name } });
}
```

### 4.5. Views (Bộ Lọc Xem Tuỳ Biến / Custom Filters)
* **Bộ lọc tinh chỉnh:** Filter các issue/task theo rất nhiều điều kiện phức tạp (Assignees = Me, Priority = High, Status = Open).
* **Lưu Views tĩnh:** "Lưu cấu hình View" lại thành lối tắt trên Sidebar. Nó đóng vai trò cho việc truy cập truy vấn lặp đi lặp lại chỉ qua 1 Click thay vì set lại từ đầu trên trang Work items.

**💻 AI Architectural Pattern gợi ý (Lưu View):**
```csharp
[HttpPost("views")]
public async Task<IActionResult> SaveCustomView([FromBody] CustomViewDto request) {
    // Deserialize request filter payload into a JSON string and save
    var view = new Page { Title = request.ViewName, Content = JsonSerializer.Serialize(request.Filters) };
    _context.Pages.Add(view); await _context.SaveChangesAsync();
    return Ok();
}
```

### 4.6. Pages (Tài Liệu / Documentation Wiki)
* **Tạo lập Văn bản (Documents):** Nơi Team Product / Tech viết các Requirement, Notes, hay Meeting minutes (Tương đương Document của Notion/Jira/Confluence).
* **Rich Markdown Editor:** Nơi áp dụng Vue Block-editor, /slash commands. Đặc biệt: Sử dụng `@` hoặc `#ID` để liên kết chéo thẳng vào một Work Item của Project. 

**💻 AI Architectural Pattern gợi ý (Xử lý Autosave qua Vue):**
```vue
<script setup>
import { useDebounceFn } from '@vueuse/core';
const content = ref('');
// Tự động lưu lên DB sau mỗi 1000ms khi người dùng dừng gõ
const autoSave = useDebounceFn(async () => {
    await apiClient.put(`/api/pages/${pageId}`, { content: content.value });
}, 1000);
</script>
```

### 4.7. System Overrides (Thiết Lập Project)
* **Quản trị Label (Nhãn/Tags):** Chuẩn hóa mã hex color và danh sách Label.
* **Cấu hình Trạng thái (Workflow / State Mapping):** Mọi công ty đều chạy Agile khác nhau, hệ thống cho phép tạo nhiều cột Status riêng, bẻ luồng Backlog/Done custom (Mapped backend via `TaskStatus` table).
* **Khu vực Thùng rác (Trash/Archive):** Quản lý khôi phục (Restore functionality) tất cả entity (Issue/Cycle/Pages) đã bị xoá logic (IsDeleted = true).

**💻 Architectural Pattern (C# Thêm cột Trạng thái Tuỳ chỉnh):**
```csharp
[HttpPost("task-status")]
public async Task<IActionResult> CreateProjectStatus(Guid projectId, CreateTaskStatusDto dto) {
    _context.TaskStatuses.Add(new TaskStatus { 
         ProjectId = projectId, Name = dto.Name, Color = dto.Color, SortOrder = dto.SortOrder 
    });
    await _context.SaveChangesAsync(); return Ok();
}
```

### 4.8. Intakes (Hộp Chờ / Triage)
* **Khái niệm:** Phễu lọc Request.
* **Luồng hoạt động:** Các Issue tạo bởi người dùng không có quyền (Guest) hoặc qua form Tích hợp ngoài sẽ không rơi thẳng vào Board làm nhiễu Project. Thay vào đó, nó nhảy vào *Intake Queue*. Project Admin sẽ vào xem và định đoạt (Approve -> Trở thành Issue, Reject -> Bỏ qua, Mark as Duplicate).

**💻 AI Architectural Pattern gợi ý (C# Cho duyệt Intake bằng EF Core):**
```csharp
[HttpPost("intakes/{intakeId}/approve")]
public async Task<IActionResult> ApproveIntake(Guid intakeId) {
    var intake = await _context.Intakes.FindAsync(intakeId);
    intake.Status = "Approved"; // Hoặc chuyển Entity Intake sang bảng WorkTask thực tế
    await _context.SaveChangesAsync();
    return Ok();
}
```

### 4.9. Task Dependencies (Mối Quan Hệ Phụ Thuộc)
* **Xử lý Backend:** Cần thao tác với bảng chuyên dụng `TaskDependency` nối ID Task nguồn và ID Task đích.
* **Các loại quan hệ:**
  - Bị Block bởi (`Blocked by`)
  - Chặn một thẻ khác (`Blocks`)
  - Nhân bản (`Duplicate of`)
  - Liên quan đến (`Relates to`)
* **Chức năng UX:** Khi thao tác chuyển một Task đang "bị block" sang trạng thái "Done", hệ thống phải ngăn chặn và bắn Toast Error cảnh báo rằng phải giải quyết con Task "Blocker" trước. Cực kì quan trọng trong quy trình Enterprise.

**💻 AI Architectural Pattern gợi ý (C# Middleware check Blocker Task):**
```csharp
public async Task<bool> IsBlockedAsync(Guid targetTaskId) {
    return await _context.TaskDependencies
        .AnyAsync(d => d.TargetTaskId == targetTaskId && d.RelationType == "BlockedBy" && d.SourceTask.State != "Done");
}
```

### 4.10. Points / Estimates (Định Lượng Độ Khó)
* Tính năng chấm điểm cho mỗi issue. Sử dụng các chuẩn: Fibonacci (1,2,3,5,8), Linear, hay theo t-shirt size (S, M, L).
* Giúp tính toán sức chứa cho Cycle (Velocity Planning).

**💻 AI Architectural Pattern gợi ý (UI Update Points mượt):**
```vue
<template>
   <!-- UI Select Fibonacci Points cho Issue Card -->
   <select v-model="points" @change="updatePoints" class="point-badge text-xs bg-gray-700">
       <option v-for="p in [0, 1, 2, 3, 5, 8, 13]" :key="p" :value="p">{{p}} pts</option>
   </select>
</template>
```

### 4.11. Project Templates (Mẫu Khởi Tạo Dự Án)
* Thay vì bắt Admin tạo thủ công từng trạng thái (Todo/Done/... ) mỗi khi khởi tạo dự án mới, hệ thống .NET hỗ trợ bảng `ProjectTemplate` lưu lại các framework hoạt động để tái sử dụng ngay lập tức. Trang bị sẵn các mẫu như: Software Engineering, HR Hiring, Marketing Sprint, v.v.

**💻 AI Architectural Pattern gợi ý (C# Hàm Copy Clone Template):**
```csharp
public async Task CloneTemplateToProject(Guid projectId, Guid templateId) {
    // Clone toàn bộ trạng thái (States) từ mẫu sang Project thực
    var states = await _context.TemplateStates.Where(t => t.TemplateId == templateId).ToListAsync();
    foreach (var st in states) {
        _context.TaskStatuses.Add(new TaskStatus { ProjectId = projectId, Name = st.Name, Color = st.Color });
    }
    await _context.SaveChangesAsync();
}
```

---

> [!CAUTION]
> **Điểm Lõi Khi Migrate:** 
> Backend **bắt buộc** phải xử lý ID logic và Auth JWT cho mỗi lớp (Workspace -> Project). Lỗi bảo mật lớn nhất khi chuyển từ Django sang ASP.NET thường gặp là DEV quên kiểm tra `ProjectMember` trong Context request C#, dẫn tới việc XSS hoặc IDOR: User có thể sửa đổi Page, Sprint của dự án khác nếu truyền ID qua API. Dùng Middleware/Policy `.RequireClaim("ProjectId")` hoặc Custom Filter Attribute trong ASP.NET là Best Practice cho phần này.

---

## 5. AI Development Prompt (Template Lệnh Hệ Thống Dành Cho AI)

Khi bạn phân công AI phát triển một Module mới trong `TaskManagement.API` hoặc Vue Frontend, hãy copy đoạn khối lệnh sau đưa cho AI:

```text
[System]
Bạn đang làm việc trong phiên bản migrate dự án Plane từ (Python/NextJS) sang hệ sinh thái (ASP.NET Core C# + Vue.js).
Nhiệm vụ của bạn là code thêm chức năng mới theo đúng chuẩn hệ sinh thái.

[Quy tắc Code Lõi - BẮT BUỘC TUÂN THỦ]:
1. KIỂM TRA QUYỀN TRƯỚC (Auth & RBAC): Mọi API Endpoint C# gọi đến đều phải đi kèm `[Authorize]`. Luôn trích xuất User qua `User.FindFirst(ClaimTypes.NameIdentifier)`. Bắt buộc kiểm tra quyền của người dùng trong hệ thống `ProjectMembers/WorkspaceMembers`.
2. FORMAT RESPONSE API: Backend của tôi yêu cầu mọi Response từ Controller đều phải bọc qua một Class/Pattern là `ApiResponse<T>` hoặc JSON vô danh có đúng cấu trúc `{ statusCode, message, data }`. Đừng trả về dạng lung tung.
3. XỬ LÝ LỖI MỊN: Trong Frontend Vue.js, mọi function gọi `apiClient` phải có biến Loading (`LoadingSpinner`). Cuối cùng, catch lỗi và bật `toast.error()` cho UX mượt mà.
4. KHÔNG SỬA MODELS GỐC LUNG TUNG: Tôn trọng hoàn toàn các Entity/Bảng nằm trong namespace `TaskManagement.Domain.Entities`. Nếu muốn thêm Model, hãy hỏi tôi trước.
```
