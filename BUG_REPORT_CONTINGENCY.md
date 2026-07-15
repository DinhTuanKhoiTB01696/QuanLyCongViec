# BUG REPORT - Contingency Plan Tab

## 1. Kiểm tra template
Trong file `TaskDetailModal.vue`, biến điều khiển trạng thái Tab hiện tại là `activityTab` (được khai báo `const activityTab = ref('comments');` tại dòng 1456). 

**Trình tự render các khối HTML:**
- Dòng 1164: `v-if="activityTab === 'history'"` -> Hiển thị bộ lọc History.
- Dòng 1170: `v-if="activityTab === 'comments'"` -> Hiển thị ô nhập Comment.
- Dòng 1222: `v-if="activityEntries.length"` -> Render Khối Timeline / Nhật ký (Activity Feed).
- **Dòng 1319:** `v-else-if="activityTab === 'contingency'"` -> Render Giao diện Contingency.
- Dòng 1392: `v-else` -> Render Dòng text Empty State.

**Vị trí gây lỗi:**
Việc thiết kế Contingency Plan dưới dạng lệnh `v-else-if` gắn liền ngay sau `v-if="activityEntries.length"` là **Nguyên nhân chính** dẫn đến lỗi. Trình biên dịch Vue nhóm 3 lệnh này thành một cây rẽ nhánh độc quyền. Vì Task của bạn đã có Activity Log tạo công việc, `activityEntries.length` sẽ lớn hơn 0, nên Vue ưu tiên render khối List Nhật ký và LẬP TỨC BỎ QUA nhánh `v-else-if` của Contingency Plan, mặc dù bạn đã click đổi Tab.

## 2. Kiểm tra dữ liệu
Khi bạn chọn Tab "Kế hoạch dự phòng":
- Biến `activityTab` thực sự đã chuyển thành `"contingency"`.
- Nhưng do `activityEntries` là mảng toàn cục chứa toàn bộ log lịch sử của hệ thống (như `"Lê Hoàng Phúc đã tạo công việc"`), độ dài mảng lớn hơn 0 nên **Component `<div class="activity-feed">` ở dòng 1222** vẫn tiếp tục được render ra đè lên màn hình.

## 3. Kiểm tra API
**API VẪN ĐƯỢC GỌI CHUẨN XÁC ĐẰNG SAU GIAO DIỆN.**
- **Tại dòng 4104**, hệ thống khai báo:
```javascript
watch(activityTab, (newTab) => {
   if (newTab === 'contingency') {
      fetchContingencyPlan();
   }
});
```
- Nghĩa là khi bạn nhấp chuyển tab, hàm `fetchContingencyPlan` lập tức gọi `GET /worktasks/{id}/contingency-plan`.
- Request chạy thành công và Response được bind vào đối tượng State `contingencyPlan` tại dòng 4046. Tuy nhiên, do UI bị ẩn đi bởi lỗi Template ở trên nên bạn không nhìn thấy dữ liệu vừa tải về.

## 4. Kiểm tra UI
Dựa vào mã nguồn component Vue từ dòng 1319 trở xuống, bộ UI được viết hoàn toàn **ĐẦY ĐỦ**, bám sát 100% thiết kế đã thống nhất, bao gồm:
- Risk Level (Dropdown Low/Medium/High/Critical)
- Risk Status (Dropdown Safe/Monitoring/AtRisk/Occurred)
- Activation Condition (Input Text)
- Risk Description (Textarea)
- Recovery Plan (Thẻ div `contenteditable="true"` ref="recoveryPlanEditor")
- Expected Result (Textarea)
- Backup Owner (Selector với API User)
- Fallback Deadline (DatePicker)
- Nút Save.

## 5. Kiểm tra Save
**HOẠT ĐỘNG TỐT NẾU HIỂN THỊ ĐƯỢC UI.**
- Khi nhấn nút Save, hàm `saveContingencyPlan()` sẽ được chạy (Dòng 4080).
- Hệ thống thực thi validate các field bắt buộc, sau đó gọi `axiosClient.put('/worktasks/{id}/contingency-plan', contingencyPlan.value)`. Trừ khi phát sinh lỗi từ Server, nếu không thì logic lưu Data đã hoàn chỉnh.

## 6. Đề xuất cách sửa
Đây là lỗi cấu trúc Vue DOM (Virtual DOM Tree Conflict). Chúng ta không cần xóa tính năng hay viết lại logic. Chỉ cần tách riêng nhóm điều kiện.

**Cấu trúc DOM hiện tại (Đang lỗi):**
```html
<div v-if="activityEntries.length" class="activity-feed">...</div>
<div class="mb-6" v-else-if="activityTab === 'contingency'">...</div>
<div v-else class="activity-empty-state">...</div>
```

**Cách sửa (Đề xuất):**
Thay `v-else-if` bằng `v-if` độc lập và bọc nhóm Activity/Comment cũ vào một thẻ if riêng:
```html
<!-- CỤM 1: Chỉ render dành cho Comments và History -->
<div v-if="activityTab === 'comments' || activityTab === 'history'">
    <div v-if="activityEntries.length" class="activity-feed">...</div>
    <div v-else class="activity-empty-state">...</div>
</div>

<!-- CỤM 2: Cụm Contingency Plan hoàn toàn độc lập -->
<div class="mb-6" v-if="activityTab === 'contingency'">
    [Toàn bộ Form Kế hoạch dự phòng của dòng 1319]
</div>
```
Việc sửa này chỉ ảnh hưởng đúng 3 dòng mã HTML tại file `TaskDetailModal.vue` và khắc phục triệt để lỗi hiển thị!
