# Báo cáo Chuyên sâu: Nghiệp vụ và Lõi Logic Code của Plane

Dựa trên quá trình chạy kiểm thử trực tiếp trên môi trường thực tế (`http://localhost:3000` của Plane) và dò xét cấu trúc code gốc, tài liệu này bóc tách hoàn toàn cách Plane vận hành (Business Logic) và cách họ tổ chức Mã nguồn (Code Logic). Tài liệu này được biên soạn đặc biệt để đội ngũ phát triển `QuanLyCongViec` (chạy Vue.js + .NET) có thể "copy" mượt nhất kiến trúc của họ.

---

## 1. Phân Tích Nghiệp Vụ (Business Logic)
Plane không phải là một "To-Do list" đơn giản. Nó là một hệ sinh thái quản lý dự án (Project Collaboration) phân theo lớp lang cực kỳ chặt chẽ.

### 1.1 Cấu trúc Dữ liệu Phân cấp (Data Hierarchy)
Quy trình truy cập bắt buộc phải đi qua 3 tầng:
1. **Workspace (Không gian làm việc - Ví dụ: `cun`):** Chứa các giới hạn về User, Tài khoản thanh toán (Billing), Role tổng (God Mode, Admin).
2. **Project (Dự án - Ví dụ: `Cun`):** Nằm bên trong Workspace. Có mã định danh riêng (Identifier: `CUN`). Sẽ sở hữu danh sách Trạng thái (States), Nhãn dán (Labels) riêng biệt. Người khác project sẽ không thấy data của nhau.
3. **Work Item / Issue (Công việc - Ví dụ: `CUN-8`):** Đơn vị nhỏ nhất. Bắt buộc gắn vào 1 Project.

### 1.2 Vòng đời của một Task (Work Item Lifecycle)
Khi thử tạo mới 1 Task (Test Task CUN-8), nghiệp vụ hệ thống yêu cầu:
- **Khởi tạo mộc:** Chỉ cần `Title` (Tên task). Các thông tin khác có thể rỗng.
- **Tiến trình (State):** Mặc định sẽ rơi vào `Backlog` hoặc `Todo`. User có thể kéo thả để di chuyển.
- **Quan hệ lồng ghép (Parent/Sub-issue):** Có thể gán 1 task làm con của task khác -> Hình thành cấu trúc cây.
- **Thuộc tính Bổ trợ:** 
  - `Estimate`: Khối lượng công việc (Quy theo Story Points).
  - `Cycles` (Vòng lặp Sprint): Nhóm các task phải hoàn thành trong 1 tuần (Agile).
  - `Modules`: Nhóm các task thuộc cùng một *Tính năng / Epic* (Ví dụ: Chức năng Đăng nhập).

---

## 2. Giải mã Logic Code của Frontend Plane (React + MobX)

Tôi đã phân tích quá trình Network Request kết hợp với Source Code của Plane. Dưới đây là "bí quyết" tạo nên sự mượt mà của Plane mà bạn cần mang chép sang Vue.js.

### 2.1 Tuyệt chiêu "Optimistic UI" phối hợp State Management
Plane sử dụng **MobX** (giống với **Pinia** của Vue). Khi bạn sửa Trạng thái (State) hoặc Tên của một Task ở màn hình Modal (Cột Sidebar Property):
1. Code **KHÔNG** hề chờ Backend phản hồi thành công mới cập nhật giao diện.
2. Nó gọi hàm `store.updateIssue(issueId, data)`.
3. Giao diện (Màn hình chi tiết và Màn hình Danh sách ở phía dưới Modal) **lập tức thay đổi ngay lập tức** do Store (Pinia) đã đổi dữ liệu Local (Reactive).
4. Phía ngầm (Background), `Axios` mới bắn HTTP PATCH request lên Server.
5. *(Rollback)*: Nếu Server báo lỗi 500, Store mới báo lỗi ra cái Toast (Góc màn hình) và *khôi phục* lại trạng thái cũ.

👉 **Áp dụng vào Vue (.NET API):** Bạn phải thiết kế `useWorkItemStore.js` (Pinia) hệt như thế. Mọi chức năng `Edit`, `Create` đều phải đục thẳng vào Array State ở Frontend trước. Đừng viết code kiểu truyền thống: *Chờ Axios -> Loading quay quay -> Cập nhật UI*. Như thế là hỏng trải nghiệm!

### 2.2 Kiến trúc Component Chia Sẻ (Component Sharing Logic)
Khi click vào một hàng (Row) trong `ListView`, Plane không nhảy qua một đường dẫn/trang mới tốn thời gian.
- Trang danh sách tổng (`ListView.tsx`) bọc bên ngoài một thẻ `<TaskDetailModal />` ẩn.
- Khi Click, nó set trạng thái `selectedIssueId = id` và đẩy Modal trượt vào đè lên trang List.
- Vì sử dụng chung một cái Hook lấy dữ liệu từ MobX gốc, nên Modal và List View được đồng bộ dữ liệu `1:1`. Đổi ở Modal thì List View đang nằm mờ mờ đằng sau lưng lập tức chạy theo.

### 2.3 Auto-save (Lưu tự động qua Debounce)
Ở phần gõ Mô tả (Rich Text Editor) và Nhập Title của Task:
- Code hoàn toàn không có hàm onSubmit toàn form.
- Khi người dùng Focus gõ nội dung, Plane kích hoạt một chuỗi Hook. 
- Khi bạn dừng gõ tay khoảng 1 giây (Debouncing 1000ms) hoặc bạn Blur chuột (Click chuột ra ngoài ô nhập liệu), Trigger ngầm sẽ gọi API HTTP PATCH cập nhật riêng đúng trường `description_html` lên Server.
- Tại Server .NET của bạn, bạn cần thiết kế API sao cho chấp nhận **PATCH** Update từng mẩu thông tin rời rạc, thay vì bắt đẩy nguyên cái Object TO ĐÙNG `UpdateWorkItemDto`.

---

## 3. Bản Chỉ Đạo cho Team Dev (Vue + .NET)

Để copy được trọn vẹn sức mạnh này, Backend và Frontend Developer của `QuanLyCongViec` cần chốt lại hợp đồng như sau:

**🔥 Backend (.NET Core Api):**
1. Phải chuẩn hoá bộ API chuẩn REST là Update 1 phần (sử dụng PATCH thay vì PUT): `[HttpPatch("{id}")]`. Tức là Mobile/Web sửa tên thì truyền Json `{ "title": "mới" }` lên thôi, các trường còn lại null thì bỏ qua, không update vào DB.
2. Thiết kế DB bắt buộc chia scope: `WorkItems` phải luôn join vào bảng `Projects`. Không cho truy vấn WorkItem "chay" để tránh lộ dữ liệu giữa các Project khác nhau.

**🔥 Frontend (Vue.js + Pinia):**
1. Dẹp ngay thói quen Component nào gọi API component nấy. Hãy tạo `useProjectStore` và `useWorkItemStore`. Cả `ListView.vue` và `TaskDetailModal.vue` CÙNG TRỎ vào một cái Array `workItems`.
2. Tạo Component `ClickOutside` hoặc lắng nghe Event `@blur` ở các thẻ Input để gọi Action `UpdateStore` (Lưu tự động).
3. Sử dụng Tailwind `backdrop-blur-md` kết hợp `absolute inset-0 bg-black/50` để bọc lớp kính cho các thẻ Dropdown (Ví dụ Menu Filter), và làm cái Modal vuốt lên không cần tải lại trang.
