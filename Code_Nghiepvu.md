# Plane Space Architecture & Workflow Report

Báo cáo này tập trung mô tả **thiết kế**, **cấu trúc dữ liệu** (Entities/Schema) và **Workflow API** dành riêng cho Module **Space (Workspace)**. Báo cáo được định dạng chuẩn hóa để các tác vụ xử lý AI (AI Agents) có thể dễ dàng hiểu, tích hợp và triển khai.

---

## 1. Mục đích và Phạm vi (Scope)
- **Space** (tương đương Workspace trong mã nguồn) là cấp độ cao nhất trong cấu trúc phân cấp dữ liệu của Plane (Space > Project > Issue/WorkTask).
- Tất cả người dùng, dự án, nhãn, chu kỳ (sprints), và công việc đều bắt buộc phải nằm trong một Space cụ thể.
- **Roles trong Space:** `OWNER`, `ADMIN`, `MEMBER`, `GUEST`.
- AI Agent cần phân lập logic dữ liệu bằng điều kiện `WorkspaceId` trong tất cả các truy vấn thuộc phạm vi Space này.

---

## 2. Thiết kế Dữ liệu (Backend Entities)

Dự án .NET (ASP.NET Core) quản lý Space qua 2 Entities chính nằm trong `TaskManagement.Domain.Entities`:

### 2.1 Workspace (Không gian làm việc)
- **Bảng:** `Workspaces`
- **Thuộc tính chính:**
  - `Id` (GUID)
  - `Slug` (string): Định danh URL-friendly duy nhất. (VD: `my-company`)
  - `Name` (string): Tên không gian làm việc.
  - `Logo` (string, nullable)
  - `Timezone` (string): Mặc định `Asia/Ho_Chi_Minh`
  - `OwnerId` (Guid): Liên kết tới người trực tiếp sở hữu workspace.
  - `IsDeleted` (bool): Soft delete pattern.

### 2.2 WorkspaceMember (Thành viên không gian làm việc)
- **Bảng:** `WorkspaceMembers`
- **Mô tả:** Bản đồ Join N-N giữa `Workspace` và `User`.
- **Thuộc tính chính:**
  - `WorkspaceId` (GUID)
  - `UserId` (GUID)
  - `WorkspaceRole` (string): Quyền hạn. Giới hạn trong `["OWNER", "ADMIN", "MEMBER", "GUEST"]`.
  - `IsActive` (bool): Đánh dấu trạng thái đang hoạt động (Soft delete khi user bị xóa khỏi không gian).

---

## 3. Workflow & API Endpoints Thiết Kế

Toàn bộ API được bảo mật với JWT Token (`[Authorize]`) và được đặt trong Controller `WorkspacesController.cs` với Base route `/api/workspaces`. Dưới đây là Workflow dành cho AI Agent:

### Workflow 1: Lấy thông tin Space
1. Khách hàng đăng nhập thành công.
2. Hệ thống gọi **`GET /api/workspaces`**: Trả về danh sách tất cả Workspaces mà user đang hoạt động.
3. Người dùng chọn 1 Workspace cụ thể để truy cập.
4. Hệ thống gọi **`GET /api/workspaces/{slug}`** để lấy siêu dữ liệu, bao gồm số lượng Project, số thành viên và role của user. Phân quyền truy cập tài nguyên bị từ chối (`403 Forbidden`) nếu user không phải là thành viên.

### Workflow 2: Cấp quản lý Space
1. **Tạo mới:** **`POST /api/workspaces`** (Body: `Name`, `Slug`, `Timezone`). Hệ thống tự động thiết lập user gửi API thành `OWNER`.
2. **Cập nhật:** **`PUT /api/workspaces/{workspaceId}`** (Chỉ dành cho ADMIN/OWNER nhằm sửa Tên, Logo, hoặc Slug).
3. **Xóa:** **`DELETE /api/workspaces/{workspaceId}`** (Chỉ OWNER được thao tác. Kích hoạt thuộc tính Soft delete bằng cờ `IsDeleted`).

### Workflow 3: Quản lý Thành viên (Mời, Sửa Quyền, Đuổi)
1. **Danh sách member:** **`GET /api/workspaces/{workspaceId}/members`** (Bất kỳ user nào trong không gian cũng có thể xem).
2. **Thêm member:** **`POST /api/workspaces/{workspaceId}/members`**
   - Ràng buộc: User yêu cầu phải là Owner/Admin.
   - Flow: Kiểm tra email user có tồn tại hệ thống chưa -> Gắn vào bảng Member, nếu đã tồn tại nhưng IsActive=false thì Reactivate cờ lên true.
3. **Cập nhật Role:** **`PUT /api/workspaces/{workspaceId}/members/{memberId}`**
   - Ràng buộc: Owner/Admin thực hiện. Admin không được phép hạ quyền của Owner.
4. **Xóa thành viên:** **`DELETE /api/workspaces/{workspaceId}/members/{memberId}`**
   - Ràng buộc: User có thể tự rời (Remove mình) hoặc Admin/Owner xóa người khác.
   - Fail-safe: Nghiêm cấm gỡ bỏ nếu Member đó là `OWNER` duy nhất còn lại của Workspace.

---

> [!TIP]
> **Hướng dẫn cho AI Agent tích hợp Logic mới:**
> 1. Truy vấn Dữ Liệu mới luôn phải mang mệnh đề `.Where(x => x.WorkspaceId == ...)` để củng cố ranh giới multitenant.
> 2. Các Action Update/Delete liên quan Space cần gọi hàm kiểm tra tính toàn vẹn `WorkspaceRole` từ bảng `WorkspaceMember` trước khi xử lý Logic gốc.
