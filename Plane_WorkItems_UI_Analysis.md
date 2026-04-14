# Phân tích & Chỉ đạo thiết kế UI phần "Work Items" (ListView, Kanban & Filter)

*Bản cập nhật này được tinh chỉnh dựa trên các ảnh chụp thực tế từ chế độ List, Kanban, Table và Menu Filter của bạn.*

**Mục tiêu:** Giữ nguyên độ sắc nét, tối giản của giao diện Plane gốc. **Cắt bỏ hoàn toàn các chức năng thừa (Cycles, Modules, Views, Pages) trên Sidebar.**

---

## 1. Các chế độ xem (View Modes) cần thực hiện
Thay vì làm quá nhiều thứ, hãy tập trung cho 2 chế độ hiển thị quan trọng nhất là **List View** và **Kanban Board**. Tab chuyển đổi nằm ở góc phải (cạnh ô Display).

### 1.1 List View (Giao diện Nhóm theo Trạng thái) - Dành cho `ListView.vue`
- Cấu trúc hiển thị y hệt như *Ảnh 1* của bạn. Dòng `Backlog`, `Todo`, `In Progress` sẽ đóng vai trò là "Group Header".
- **Giao diện Dòng (Row):** Mỗi công việc hiển thị siêu gọn trên 1 dòng duy nhất.
  - Cột chứa ID (`CYBWF-4`) và Tiêu đề `3. Create and assign Work Items` nằm bên trái.
  - Cột Badge `State`, Icon `Priority` (dạng cột sóng), `Assignees` (Avatar) nằm bên thẳng hàng bên phải.
- **Nút "Thêm nhanh" (`+ New work item`):** Nằm gọn gàng, mờ nhạt ở cuối mỗi khối (Vd: Dưới khối *In Progress 2*).

### 1.2 Kanban Board - Dành cho `KanbanBoard.vue`
- Bám sát *Ảnh 2* của bạn: Các cột sẽ xếp theo chiều ngang (`Backlog`, `Todo`, `In Progress`, `Done`).
- Phía trên có thanh hiển thị Filter hiện tại: Ví dụ mũi tên `Priority is Urgent` với nút xóa nhỏ (`x`).
- Nền cột trong suốt, Header cột có icon vòng tròn của State.
- Nút `+ New work item` nằm ẩn mờ ngay bên dưới tên cột. 

---

## 2. Menu Lọc & Tùy chỉnh (Display Dropdown)
*Ảnh 4* của bạn cho thấy menu tùy chỉnh khi bấm nút **`Display`**. Cần chú ý cách thiết kế:
- Menu thả xuống dạng **Vertical List** (Dọc), icon mảnh, chữ nằm bên phải.
- Có thẻ `<input>` tìm kiếm (`Search`) nằm ngay trên cùng.
- **Để giảm tải chức năng code:** Hãy yêu cầu AI MỞ KHÓA (Hiển thị) các mục cơ bản như: `State`, `Assignees`, `Priority`, `Label`. 
- TẠM THỜI ẨN CÁC MỤC KHÓ: `Cycle`, `Module`, `Mentions` để làm sau.

---

## 3. Bảng Thống Kê (Analytics Modal)
*Dựa trên Ảnh 5 của bạn:*
- Nếu cần ráp logic Analytics, nó đơn giản chỉ là một khung màu xám than (Deep charcoal) phủ lên toàn màn hình (Modal full-screen hoặc cỡ lớn).
- Gồm các chỉ số lớn: `Total Work items`, `Started`, `Backlog`, `Completed`.
- Nếu thư viện Chart (như Chart.js/ApexCharts) tốn thời gian quá, có thể thay bằng các thanh Progress bar tạm thời.

---
**Tóm tắt cho AI:** "Hãy xây dựng component danh sách công việc gồm 2 View chính: ListView (Group by State) và KanbanBoard. Chú ý nút `Display` mở ra Dropdown filter loại bỏ viền thô kệch, chỉ dùng CSS backdrop-blur. Tuyệt đối không làm sidebar menu thừa thãi (Bỏ qua Cycles/Modules)."
