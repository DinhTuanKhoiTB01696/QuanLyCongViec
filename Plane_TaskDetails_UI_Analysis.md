# Thiết kế và Logic chi tiết của Trang chi tiết Công việc (Task Detail)
*Tài liệu này được cập nhật chuẩn xác dựa trên 2 bức hình chụp giao diện Task Detail (CYBWF-8) thực tế của bạn.*
Mục tiêu là chép lại chính xác `TaskDetailModal.vue` theo đúng cái hồn của giao diện Plane, rất thoáng và gọn.
---
## 1. Bố cục Chia Đôi (Split Layout)
Đây là đặc trưng quan trọng nhất. Vùng hiển thị sẽ chia làm 2 mảnh:
- **Cột Trái (Main Content):** Chứa các nội dung bành trướng như Tiêu đề (fd), nút tính năng (Add sub-work item) và khung comment, lịch sử. Không có viền gò bó.
- **Cột Phải (Properties Sidebar):** Hiển thị danh sách các trường thuộc tính dưới dạng dòng siêu tối giản. Được ngăn với bên trái bằng viền mờ `border-l border-subtle` dọc.
---
## 2. Thiết kế Cột Trái (Nội dụng Chính & Hoạt động)
### 2.1 Tiêu đề & Thông tin cơ bản
- **Thanh đỉnh (Top Bar):** Bên trái có icon bung rộng (Expand) và ẩn hiện sidebar. Bên phải có góc nút `Unsubscribe`, icon chép Link, và `...` menu.
- **Project Key:** Ký hiệu dự án in nhỏ (Vd: `CYBWF-8`) làm màu xám.
- **Tiêu đề (Chữ to):** "fd" -> Text input dạng thẻ Header (h1/h2) lớn, fontweight bold. Lúc focus không được mọc ra khung viền đen sì như input mặc định. Gõ trực tiếp.
- **Mô tả (Description - "fds"):** Đặt ngay dưới tiêu đề. Cảm giác giống như một trang Word khổng lồ.
- **Quick Action Bar (Nút thao tác nhanh):** 
  - Nằm ngay dưới phần lịch sử sửa đổi (Last edited by dsa).
  - Có các nút bo góc viền nhạt: `🎯 Add sub-work item`, `🔗 Add relation`, `🔗 Add link`, `📎 Attach`.
### 2.2 Phần Lịch sử & Bình luận (Activity & Comment)
*(Nửa dưới màn hình)*
- **Header:** Chữ `Activity` to đậm nằm ở góc. Kế bên có icon đổi chiều sắp xếp (Sort) và Lọc (Filter).
- **Lịch sử hoạt động (Audit log):** Dạng timeline nhỏ. Ví dụ: `[Avatar] dsa created the work item. 8 minutes ago` - cỡ chữ siêu nhỏ màu xám.
- **Khung Gõ Bình Luận (Rich Text Comment Box):**
  - Chữ mờ `Add comment` khi trống.
  - Ngay cuối khung Editor có một dải thanh công cụ viền mờ màu xám than. Trông giống trình soạn thảo: Chữ In đậm (`B`), Căn giữa, Gắn Code (`</>`), Ảnh (`Hình vuông núi nón`). Góc phải tít cùng là nút `Comment` (màu nhạt khi chưa gõ gì).
---
## 3. Thiết kế Cột Phải (Properties - Thanh Thuộc tính)
Đây là nơi quan trọng nhất cần chép lại từng dòng. Phải làm nó dạng *List*, chứ không phải bảng cục mịch. Mỗi dòng là 1 cặp *[Tên Tính Chất] --- [Giá Trị]*.
**Khung Properties Sidebar chứa:**
1. Chữ `Properties` làm tiêu đề in đậm nhỏ. Gồm một loạt các dòng như sau:
2. 🎯 **State** --- (Biểu tượng tròn xám) `Backlog` (chữ in đậm màu trắng).
3. 👥 **Assignees** --- `Add assignees` (Nhạt màu, nút click).
4. 📶 **Priority** --- `None` (Có icon cấm tròn).
5. 👤 **Created by** --- Đi kèm hình Avatar tròn (Chữ D xanh lá `dsa`).
6. 📅 **Start date** --- `Add start date`.
7. 📅 **Due date** --- `Add due date`.
8. 💼 **Modules** --- `No module` *(Lưu ý: Bạn chọn cắt module nên phần này có thể Ẩn).*
9. 🌓 **Cycle** --- `No cycle` *(Lưu ý: Có thể ẩn).*
10. 🔗 **Parent** --- `Add parent work item`.
11. 🏷️ **Labels** --- Nút `+ Add labels` có viền bo tròn bọc riêng.
> 🛠 **Triết lý Bắt buộc cho Sidebar này:**
> - Tuyệt đối không dùng thẻ `<input>` có viền.
> - Các thuộc tính màu chữ rất nhạt gầy (`font-medium text-13px`).
> - Dùng hiệu ứng Hover sáng viền nhẹ thay vì hiển thị khung viền, giống tính năng của Notion!
---
Bạn có thể kết hợp tài liệu này và tài liệu Plane_WorkItems_UI... cùng 2 bức ảnh mới nhất để làm bộ khung giao cho AI design cho chuẩn xác tuyệt đối nhất. Lệnh này: 
*"Hãy tham khảo 2 file md này cùng những ảnh đính kèm (Ảnh Board và ảnh Task Modal Split View). Sửa code layout trang SpaceDetail và TaskDetailModal theo chuẩn mực màu sắc tĩnh mịch này."*
