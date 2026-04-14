# Báo Cáo Phân Tích Nghiệp Vụ & Kiến Trúc Hệ Thống Plane
## Bản Thiết Kế Chuyển Đổi Tổng Thể (ASP.NET Core & Vue.js)

Dưới đây là phân tích toàn diện về nghiệp vụ, logic hệ thống, cấu trúc Database của hệ thống Plane (dựa trên Workspace cục bộ, ví dụ `localhost:3000/cun/`). Tài liệu này đóng vai trò như một Software Architecture Document (SAD) để AI kế nhiệm có thể đọc, đối chiếu với source code gốc và tiến hành lập trình phiên bản mới.

---

## 1. Tổng Quan Nghiệp Vụ (Core & Advanced Business Logic)

Hệ thống cung cấp một giải pháp quản lý vòng đời dự án Agile cực kỳ chặt chẽ:

### 1.1. Core Workflows (Luồng cốt lõi)
1. **Authentication & Authorization**: Quản lý Role (Admin, Member, Guest) trên 2 cấp độ bảo mật chéo là **Workspace** (ví dụ: công ty) và **Project** (dự án con).
2. **Issues Management (Tâm điểm)**: 
   - Quản lý thẻ Task (Issue), thay đổi trạng thái (State/Kanban), gán nhãn (Label), giao việc (Assign) và đánh giá độ ưu tiên.
3. **Cycles & Modules**: 
   - **Cycle**: Sprints giới hạn theo chu kỳ thời gian.
   - **Module**: Epics/Features đánh dấu các cột mốc phát triển.

### 1.2. Tính Năng Nâng Cao (Advanced Tracking & Automations)
1. **Inbox / Intake System**: Cấu chế "hàng chờ" (Queue) cho phép gom nhặt Issue từ bên ngoài (Khách hàng, Bot) và yêu cầu Manager "Accept/Decline" trước khi đẩy vào Backlog chính thức.
2. **Pages (Trình soạn thảo Wiki)**: Soạn thảo tài liệu Block-based dạng Notion thu nhỏ tích hợp trực tiếp vào Project. Cho phép đính kèm Issue bên trong bài viết rạch ròi.
3. **Analytics & Views**: Biểu đồ Burndown, Cumulative Flow Diagram (CFD). Cung cấp Rich Filter thông minh lưu trữ theo User Preferences.
4. **Time Tracking & Estimates**: Trọng số điểm ảo (Story Points) theo dãy Fibonacci hoặc T-shirt size.
5. **Webhooks & External Integrations**: Bắn HTTP Post notifications ra Slack/Discord hoặc tiếp nhận thay đổi trạng thái tự động từ Github/Gitlab commits.

---

## 2. Phân Tích Cơ Sở Dữ Liệu (Database Schema)

Dưới đây là lược đồ thực thể tĩnh chuẩn hoá cho việc tạo **Entity Framework Core (C#)** thông qua Code-First:

### 2.1. Nhóm Identity & Multi-Tenancy (Workspace)
- **User**: (Email, Password Hash, IsActive, LastLogin, Avatar).
- **Workspace**: (Slug, Name, Logo, Cấu hình múi giờ).
- **WorkspaceMember**: Lớp mapping giữa User và Workspace + Role tham chiếu. Phân thân xử lý dữ liệu Multi-Tenant (Bắt buộc phải có `WorkspaceId` trong từng Query).

### 2.2. Nhóm Dự án (Project Lifecycle)
- **Project**: Nằm trong `WorkspaceId`. Thiết lập tự động tăng ID Identifier (Quy ước mã như `CUN-1`, `CUN-2`).
- **State**: Định nghĩa các cột Kanban mặc định và Custom của dự án. Ánh xạ State theo Group (Backlog, Todo, InProgress, Done, Cancelled).
- **ProjectMember**: Phân quyền chi tiết (User vào được Workspace nhưng chưa chắc thấy được Project Private).

### 2.3. Nhóm Công việc (Issues & Extensions)
- **Issue**: Khóa ngoại liên kết chéo (`ProjectId`, `StateId`, `CreatedById`, `AssigneeId`). Thuộc tính: Name, Description (JSON block), Priority, **SortOrder** (Double/Decimal phục vụ thuật toán LexoRank kéo thả thứ tự Kanban).
- **IssueComment / IssueLabel**: Hệ thống giao tiếp và phân loại động.
- **Intake**: Track dữ liệu chờ duyệt dành riêng cho mô-đun Inbox.
- **Estimate / Evaluate**: Record các thẻ size và điểm đánh giá.

### 2.4. Nhóm Kế hoạch định tuyến (Planning)
- **Cycle / Module**: Các entity nhóm, ràng buộc Ngày bắt đầu/Ngày kết thúc và Owner.
- M-N Mappings: `IssueCycle`, `IssueModule` lưu lại dấu vết sự thay đổi theo sprints.

### 2.5. Phủ Sóng Quy Mô Multi-tenant (Tổng quan 93 Bảng)
Vì thiết kế cốt lõi là **Multi-Tenant**, hệ thống giới hạn tự trị của Plane quản lý tới **93 bảng thực thể nằm gói gọn trong cấp độ "Space" (Workspace)**. AI kế nhiệm khi tiến hành di trú cần bám sát kiến trúc các file Models nằm tại `d:\A\plane\apps\api\plane\db\models\*.py`:
1. **Workspace & Identity** (~12 bảng): `workspaces`, `workspace_members`, `users`, `profiles`,...
2. **Projects** (~8 bảng): `projects`, `project_members`, `project_identifiers`,...
3. **Issue Core** (~21 bảng): Trái tim của ứng dụng gồm `issues`, `issue_comments`, `issue_relations`, `issue_blockers`,...
4. **Planning & Cycles** (~9 bảng): `modules`, `cycles`, `cycle_issues`,...
5. **Drafts & Intake** (~7 bảng): `intakes`, `draft_issues`,...
6. **Pages (Wiki)** (~5 bảng): `pages`, `page_versions`,...
7. **Ngoại Vi, Analytics & Notifications** (~17 bảng): `integrations`, `webhooks`, `notifications`, `analytic_views`,...
8. **Utilities Khác** (~14 bảng): `estimates`, `stickies`, `devices`, `sessions`,...

#### 📝 Code Mẫu: Entity Framework Core (Base Models)
```csharp
public class Project
{
    public Guid ProjectId { get; set; }
    public string Name { get; set; }
    public string Identifier { get; set; } // Ví dụ: CUN
    
    // Multi-tenant
    public Guid WorkspaceId { get; set; }
    public Workspace Workspace { get; set; }
    
    public ICollection<Issue> Issues { get; set; }
}

public class Issue
{
    public Guid IssueId { get; set; }
    public string Name { get; set; }
    public double SortOrder { get; set; } // Hỗ trợ kéo thả với thuật toán LexoRank
    
    public Guid ProjectId { get; set; }
    public Project Project { get; set; }
    
    public Guid StateId { get; set; }
    public State State { get; set; }
}
```

---

## 3. Kiến Trúc Backend (ASP.NET Core 8 Web API)

- **Design Pattern**: Triển khai theo **Clean Architecture** (Core, Infrastructure, WebApi) kết hợp **CQRS (tích hợp MediatR)**.
- **Authorization & Context Middlewares**: 
  - Khung JWT Bearer + Cookies (HttpOnly) giống hệt hành vi hiện tại. 
  - Cần một `WorkspaceContextMiddleware` chặn từ vòng ngoài Url: Parse `/{workspaceSlug}/...` để lấy Workspace Id map vào Scope của Request hiện tại (Tránh rò rỉ dữ liệu chéo User).
- **Caching & Real-time**: 
  - **SignalR**: Kết nối Socket realtime để update Kanban lập tức khi có user khác kéo/thả thẻ.
  - **Redis Cache**: Lưu User Profiles và cấu hình States để tránh hit DB.
- **Background Workers**: Sử dụng **Hangfire** chạy ngầm để tính toán Analytics hàng đêm (Snapshot data), gửi Email nhắc việc, và dispatch Webhooks ra bên ngoài.

#### 📝 Code Mẫu: Cơ chế CQRS với MediatR Controller & Handler
```csharp
// 1. Command Definition
public record CreateIssueCommand(string Name, string Description, Guid ProjectId) : IRequest<IssueDto>;

// 2. Command Handler (Logic nghiệp vụ)
public class CreateIssueCommandHandler : IRequestHandler<CreateIssueCommand, IssueDto>
{
    private readonly ApplicationDbContext _db;
    
    public CreateIssueCommandHandler(ApplicationDbContext db) => _db = db;

    public async Task<IssueDto> Handle(CreateIssueCommand request, CancellationToken cancellationToken)
    {
        var issue = new Issue 
        { 
            IssueId = Guid.NewGuid(),
            Name = request.Name, 
            ProjectId = request.ProjectId,
            SortOrder = LexoRankGenerator.GetNext() // Logic sinh sort order
        };
        _db.Issues.Add(issue);
        await _db.SaveChangesAsync(cancellationToken);
        
        return new IssueDto(issue.IssueId, issue.Name, issue.SortOrder);
    }
}

// 3. API Controller
[ApiController]
[Route("api/workspaces/{workspaceSlug}/projects/{projectId}/issues")]
public class IssuesController : ControllerBase
{
    private readonly IMediator _mediator;
    public IssuesController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> Create(Guid projectId, [FromBody] CreateIssueDto dto)
    {
        var result = await _mediator.Send(new CreateIssueCommand(dto.Name, dto.Description, projectId));
        return Ok(result);
    }
}
```

---

## 4. Kiến Trúc Frontend (Vue.js 3 + Vite)

- **Framework**: `Vue 3` (Composition API) + `Vue Router` cho Nested Routing (`/:workspaceSlug/projects/:projectId/...`).
- **State & Data Sync**:
  - Global UI / Auth Storage: **Pinia**.
  - Server State & Optimistic UI: Rất gắt gao khuyến khích áp dụng **Vue Query (@tanstack/vue-query)** để quản lý Caching/Fetching API. Khi kéo thả Kanban thả tay, UI Vue Query sẽ tự update cục bộ không chờ API (Optimistic response) khiến Tool siêu mượt.
- **UI System**: Sử dụng **Tailwind CSS** + Headless Components (`radix-vue`). Cần tích hợp `vuedraggable` cho Kanban boards.
- **Rich Editor**: Dùng `Tiptap Vue 3` thay thế Text-Editor, viết Custom Nodes để thực hiện trò Mention (`@userName` / `#IssueId`).

#### 📝 Code Mẫu: Vue 3 Composition API & Optimistic Queries
```vue
<script setup lang="ts">
import { useQuery, useMutation, useQueryClient } from '@tanstack/vue-query'
import { api } from '@/services/api'
import KanbanBoard from '@/components/KanbanBoard.vue'

const props = defineProps<{ workspaceSlug: string, projectId: string }>()
const queryClient = useQueryClient()

// Fetch Issues Data
const { data: issues, isLoading } = useQuery({
  queryKey: ['issues', props.projectId],
  queryFn: () => api.get(`/${props.workspaceSlug}/projects/${props.projectId}/issues`).then(res => res.data)
})

// Optimistic Update khi kéo thả xếp hạng / chuyển State Kanban
const { mutate: updateIssueStatus } = useMutation({
  mutationFn: (data: { issueId: string, newStateId: string }) => 
    api.patch(`/${props.workspaceSlug}/projects/${props.projectId}/issues/${data.issueId}`, data),
  onMutate: async (newData) => {
    // 1. Cần hủy các network requests đang fetch để tránh bị chèn ép state
    await queryClient.cancelQueries({ queryKey: ['issues', props.projectId] })
    // 2. Snapshot mảng dữ liệu cũ trước khi update
    const previousIssues = queryClient.getQueryData(['issues', props.projectId])
    // 3. UI Update ngay lập tức (không chờ độ trễ Server)
    queryClient.setQueryData(['issues', props.projectId], (old: any) => {
      return old.map((i: any) => i.issueId === newData.issueId ? { ...i, stateId: newData.newStateId } : i)
    })
    return { previousIssues }
  },
  onError: (err, variables, context) => {
    // 4. Rollback về mảng ban đầu nếu Request lỗi
    if (context?.previousIssues) {
      queryClient.setQueryData(['issues', props.projectId], context.previousIssues)
    }
  },
  onSettled: () => {
    // 5. Luôn fetch lại từ DB sau khi resolve/reject để đồng bộ
    queryClient.invalidateQueries({ queryKey: ['issues', props.projectId] })
  }
})
</script>

<template>
  <div v-if="isLoading">Loading...</div>
  <KanbanBoard 
    v-else 
    :issues="issues" 
    @drag-end="updateIssueStatus" 
  />
</template>
```

---

## 5. [AI PROMPT] - Yêu Cầu Chỉ Thị Dành Cho Lập Trình Viên AI

*Đoạn nội dung này là **chỉ thị nội trú (System Prompts)**, chỉ dẫn cho bất kỳ AI nào đang đọc báo cáo này thực thi các bước Audit, Verify và Generate code chuẩn.*

### 👉 Yêu Cầu 1: Hệ Thống Tự Động Rà Soát (Audit) Cơ Sở Dữ Liệu
"AI, hãy đối chiếu thiết kế DB (Phần 2) với mô hình dữ liệu thực tế (bạn có thể request đọc các file Model gốc của source code nếu cần). Nhiệm vụ của bạn là:
1. Đảm bảo mọi Navigation Property (One-to-Many, Many-to-Many) đã chuẩn xác và không mâu thuẫn lặp vòng.
2. Phân tích truy xuất: Đánh giá xem có cần thiết phải sử dụng các **Stored Procedures (SP)** tại Database (PostgreSQL/SQL Server) hay không đối với các logic cực nặng. *Ví dụ:* Tính biểu đồ Analytics Burndown cho hàng triệu Issues ở chế độ Group By Date cần viết SP để làm giảm gánh nặng memory của ASP.NET. AI hãy tạo / gợi ý SP tương ứng.
3. Rà soát chuẩn tắc thiết lập Audit Log (Lưu vết thay đổi do User nào thực hiện vào thời gian nào `CreatedAt`/`UpdatedAt`/`CreatedBy`)."

### 👉 Yêu Cầu 2: Hệ Thống Rà Soát (Audit) Code Cũ Và Di Trú
"AI, dựa trên mã code gốc (Django & Vite) đi kèm, hãy rà soát kỹ Logic hiện đã viết và quyết định phương án chuyển đổi sang hệ sinh thái mới (ASP.NET / Vue): 
1. **Phân loại code**: Liệt kê đoạn code / Service logic nào cực kỳ quan trọng đòi hỏi phải GIỮ NGUYÊN cấu trúc nghiệp vụ thuật toán (VD: thuật toán Generate Issue ID, thuật toán Calculate LexoRank để sort Drag-Drop Kanban).
2. **Quyết định refactor/rewrite**: Đề xuất các đoạn code nên lượt bỏ hoặc tự động bổ sung thay thế bằng cơ chế có sẵn tốt hơn trên môi trường CASP.NET Core + Vue.js (VD: Thay thế SWR bằng Vue Query, thay thế Celery bằng Hangfire, thay middleware lồng lụa của Django bằng Attribute Routing và IActionFilter của C#).
3. **Phát triển và Sửa lỗi**: Nếu phát hiện lổ hỗng hoặc các logic lỗi chưa cover ở dự án cũ (Ví dụ: vòng lặp Deadlock khi update parent/child tasks), hãy tự động tạo ra bản vá logic trước khi đổ khuôn sinh ra mã cho hệ C# và C-Vue mới."

### 👉 Yêu Cầu 3: Ranh Giới Nghiệp Vụ - Giới Hạn Context Workspace (Space)
"AI chú ý QUAN TRỌNG: Nhiệm vụ của bạn trong lần phân tích và kiến tạo code này **CHỈ NẰM TRONG GIỚI HẠN KHÔNG GIAN LÀM VIỆC (Space / Workspace / Tenant)**.
1. **KHÔNG VƯỢT QUYỀN**: Tuyệt đối KHÔNG đụng chạm hay thiết kế sửa đổi các logic liên quan đến phân hệ **Admin toàn cầu (Global Admin / Godmode)** của hệ thống nền.
2. **PHÂN QUYỀN MỤC TIÊU**: Các Role cấp phép mà bạn phục vụ xử lý logic chỉ tập trung vào: **PM, PO, và System Admin (thuộc cấp Workspace gốc/dự án)**.
3. **NGHIÊN CỨU DỮ LIỆU GỐC (SEED DATA)**: Để hiểu rõ bộ phân quyền hiện tại và giới hạn hoạt động thực tế của các Role này (từ đó thiết kế các cơ chế `Authorization` phù hợp), bạn **BẮT BUỘC** phải tự tìm đọc các file dữ liệu mầm (Seed Data) đã có sẵn tại hệ sinh thái cũ ở thư mục `D:\A\QuanLyCongViec\Backend\seed_data.sql`:
   - `issues.json`
   - `projects.json`
   - `states.json`
   - `modules.json`
   - ...
   Sau khi quét cấu trúc dữ liệu seed mẫu này, bạn mới có đủ cơ sở đổ logic bảo mật chuẩn xác mà không bị lấn sân vượt khỏi phạm vi dự án."
