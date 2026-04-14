# Hướng dẫn thiết kế và Logic phần Project (Space Module)

Tài liệu này trích xuất thiết kế UI và logic kinh doanh cốt lõi của phần **Project** (trong scope Space/Workspace) từ dự án gốc [Plane Software](https://github.com/makeplane/plane) để áp dụng vào kiến trúc Vue.js + .NET hiện tại.

## 1. Cấu trúc UI (Giao diện)

Giao diện danh sách dự án của Plane được thiết kế dạng lưới (Grid) với các thẻ bài (Card) rất đẹp và tinh tế.

### 1.1 Layout Tổng Quan (`ProjectCardList`)
- **Hiển thị dạng lưới:** Sử dụng CSS Grid responsive.
  ```css
  grid grid-cols-1 gap-8 md:grid-cols-2 lg:grid-cols-3
  ```
- **Empty State (Trạng thái rỗng):** 
  - Nếu chưa có dự án: Hiển thị icon lớn (size-40), tiêu đề, mô tả và nút "Create Project" (chỉ khả dụng nếu có quyền Admin hoặc Member).
  - Nếu tìm kiếm/lọc không ra kết quả: Hiển thị minh họa "Not found".

### 1.2 Thẻ Dự Án (`ProjectCard`)
Mỗi thẻ dự án có hiệu ứng hover mượt mà (`transition-all duration-300 hover:border-strong hover:shadow-raised-200`) và được chia làm 2 phần tĩnh cơ bản:

#### Phần trên (Ảnh bìa & Tiêu đề - Chiều cao: 118px)
- **Cover Image:** Chứa một ảnh bìa full header (`rounded-t`), có một lớp gradient mờ phủ lên để text nổi bật (`bg-gradient-to-t from-black/60 to-transparent`).
- **Logo & Info:** Nằm ở góc dưới cùng bề mặt của ảnh.
  - **Logo:** Hình vuông nhỏ 36x36px (`bg-white/10`) căn giữa logo icon (size 18).
  - **Title:** Chữ in đậm (`font-semibold text-on-color truncate`).
  - **Identifier & Network:** Phía dưới tiêu đề có mã định danh dự án (vd: `PROJ-1`) và một icon Lock 🔒 nếu dự án nằm trong mạng lưới riêng tư (Private).
- **Quick Actions:** Nằm bên phải của ảnh cover (Copy Link, Thêm vào Favorite). Nút Favorite có hình ngôi sao, sẽ highlights khi người dùng đã đánh dấu.

#### Phần dưới (Mô tả & Thành viên - Chiều cao: 104px)
- **Mô tả (Description):** Hiển thị ở chuẩn 2 dòng, tự động cắt chữ nếu dài quá (`line-clamp-2 text-13 break-words text-tertiary`).
- **Footer của Card:**
  - **Trái:** Hiển thị nhóm Avatar của các thành viên (Tooltip khi hover sẽ hiện "X Members"). Nếu không có thành viên thị "No Member Yet".
  - **Phải (Trạng thái & Actions):** 
    - Nếu đã là thành viên (và có quyền Admin/Member) → Hiển thị nút "Settings" (Icon ⚙️).
    - Nếu đã là thành viên (chỉ là Guest/Viewer) → Hiển thị chữ "Joined" (Icon ✔️).
    - Nếu chưa tham gia → Hiển thị nút "Join".

---

## 2. Logic Kinh Doanh (Business Logic)

Phần Project trong Workspace có các logic kinh doanh cốt lõi sau mà hệ thống Vue + .NET cần đáp ứng:

### 2.1 Role-Based Access Control (RBAC) cho Project
Card sẽ phản ứng theo quyền người dùng (`Member Role`):
1. **Admin/Member:** 
   - Có thể thấy nút `Settings` để vào cài đặt dự án.
   - Có quyền khôi phục/xóa vĩnh viễn (`Restore` / `Delete`) dự án nếu đang ở trong mục "Archived".
   - Được phép thêm vào mục `Favorites`.
2. **Viewer / Non-Member:**
   - Sẽ xuất hiện nút `Join Project`. Bấm vào sẽ mở popup `JoinProjectModal`.
   - Card sẽ bị vô hiệu hóa việc click chui vào màn hình chi tiết Issues nếu người dùng chưa join dự án (`data-prevent-progress` / e.preventDefault()).

### 2.2 Các thao tác Context Menu (Click chuột phải vào Card)
Mỗi dự án đều tích hợp `ContextMenu` chứa (tùy thuộc vào quyền RBAC của người dùng):
- **Settings:** Chuyển đến màn settings.
- **Join:** Tham gia dự án (Nếu chưa join).
- **Open in new tab:** Ngay cả khi chưa join.
- **Copy link:** Lấy link trực tiếp tới issue board.
- **Restore / Delete:** Nếu dự án đang bị lưu trữ (Archived) và có quyền Admin.

### 2.3 Các API & Trạng thái tương tác cần thiết ở Frontend (Vue Store / API)
- `fetchProjects(workspaceSlug)`: Lấy toàn bộ danh sách, hỗ trợ phân trang hoặc tải toàn bộ.
- `addProjectToFavorites(workspaceSlug, projectId)`: Thêm dự án vào Favorites. Cần hiển thị Promise Toast (Đang xử lý -> Thành công / Thất bại).
- `removeProjectFromFavorites(workspaceSlug, projectId)`: Xóa khỏi danh sách yêu thích.
- **Lọc (Filters):** Trong Plane, các dự án hiển thị dựa trên bộ lọc (Filter) và Display Filter (vd: loại trừ dự án `archived_projects`). Do đó Store cần có cơ chế quản lý mảng filter.

## Dành cho AI áp dụng vào dự án Vue.js:
- **Style:** Dựa vào thiết kế trên, hãy xây dựng lại bằng SCSS/CSS thuần (hoặc Tailwind CSS tương đương với codebase hiện tại) đảm bảo tính thẩm mỹ, glassmorphism ở phần header.
- Trích xuất component thành `ProjectCard.vue`, `ProjectCardList.vue`.
- Hãy sử dụng `Pinia` (Vue store) thay vì MobX. Cần có các action tương đương: `fetchProjects`, `addProjectToFavorites`, cấu hình RBAC theo Role.
