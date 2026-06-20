# AI Workflow Rules cho dự án QuanLyCongViec / SprintA

## 1. Nguyên tắc sản phẩm

Dự án này là ứng dụng quản lý công việc cho người Việt. Mặc định toàn app phải dùng Tiếng Việt. English chỉ là tùy chọn khi người dùng tự chuyển trong Profile.

Không được để UI nửa Tiếng Việt nửa English.

Không dịch:

* SprintA
* Issue key: DQLCV-*, PQLCV-*, SCRUM-*, CYBWF-*, SKIB-*
* Email
* Tên user
* Tên project do user tạo
* Tên task do user nhập
* Route
* API field
* Enum
* CSS class
* Component name
* File name

## 2. Luật chức năng quan trọng

Không được thay component cũ đang có chức năng thật bằng component visual/mock.

Không được import mock làm runtime nếu API thật đã có.

Không được xóa chức năng cũ để đổi lấy giao diện giống ảnh.

Không được làm nút giả. Nút nào hiện ra phải thuộc một trong ba trạng thái:

1. Có handler thật và hoạt động thật.
2. Disabled rõ ràng nếu chưa có chức năng.
3. Có ghi chú `[CẦN FLOW]` hoặc `[CẦN ẢNH]` trong báo cáo nếu chưa đủ thông tin.

Không được thêm workflow mới nếu source hoặc screenshot không chứng minh.

Không được tự thêm backend schema, migration hoặc API mới nếu task không yêu cầu rõ.

## 3. Luật dữ liệu

Nếu màn đã có API thật thì phải dùng API thật.

Mock chỉ được dùng khi:

* Chưa có API thật.
* Đang làm visual prototype tạm thời.
* Có ghi rõ đây là fallback/dev-only.

Không được để người dùng nhìn thấy mock như dữ liệu thật nếu project hiện tại có dữ liệu API thật.

Task tạo/sửa/xóa phải persist sau reload.

Task tạo ở Board phải xuất hiện đúng Board context.
Task tạo ở Backlog phải nằm đúng Backlog context nếu source có rule phân biệt.
Nếu source chưa có rule phân biệt Board/Backlog thì phải báo `[CẦN BACKEND RULE]`, không tự bịa field.

## 4. Luật rollback và bảo vệ code cũ

Trước khi refactor lớn, phải audit component cũ đang có chức năng thật.

Các component/flow cần bảo vệ:

* TaskDetailModal
* CreateTask modal/form
* BacklogTab
* TimelineTab nếu đang dùng API thật
* CalendarTab nếu đang dùng API thật
* ReportsTab nếu đang dùng dữ liệu thật
* SpreadsheetTab
* useWorkTaskStore
* useProjectStore
* useSprintStore
* API services thật

Không được xóa legacy code khi chưa chứng minh UI mới thay thế đủ chức năng.

## 5. Luật làm việc với Git

Không được dùng `git add .`.

Không được `git reset --hard` nếu chưa tạo backup branch hoặc được yêu cầu rõ.

Sau mỗi task phải báo:

* File đã sửa
* `git diff --name-only`
* Build/test result
* Chức năng đã test
* Chức năng chưa test
* Mock runtime còn ở đâu
* Nút nào chưa có handler thật

Chỉ commit theo scope nhỏ, một task một commit.

## 6. Luật chia task

Không làm nhiều phase trong một lần.

Thứ tự bắt buộc:

1. Mặc định toàn app Tiếng Việt, English là tùy chọn.
2. Khôi phục chức năng thật: create/open/edit task.
3. Timeline/Calendar/Reports dùng API thật, không dùng mock runtime.
4. Đồng bộ Work Items shell.
5. Sau cùng mới chỉnh giao diện giống ảnh.

Không được nhảy sang visual QA nếu chức năng thật chưa chạy.

## 7. Luật báo cáo trước khi sửa

Trước khi sửa code, phải trả lời:
A. Quan sát từ source hiện tại.
B. Component/flow cũ nào đang có chức năng thật.
C. Component/mock nào đang thay thế sai.
D. File dự kiến sửa.
E. Rủi ro.
F. Cách test sau khi sửa.

Sau khi sửa code, phải trả lời:
A. File đã sửa.
B. Chức năng đã khôi phục.
C. Mock runtime đã gỡ khỏi đâu.
D. Nút nào còn disabled/chưa có handler.
E. Build/test result.
F. `git diff --name-only`.
G. Các bước user cần test bằng UI.
