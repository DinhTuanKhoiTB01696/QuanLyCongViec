
NHIỆM VỤ: NÂNG CẤP STYLE DASHBOARD ĐỒNG BỘ VỚI REWARDS / SAAS HIỆN ĐẠI

Mục tiêu của task này là nâng cấp giao diện Dashboard hiện tại theo cùng một phong cách thiết kế với trang Rewards, để toàn bộ SprintA có một visual language thống nhất, hiện đại, chuyên nghiệp và đồng bộ.

Dashboard phải trông như cùng một hệ sản phẩm với Rewards, không còn cảm giác mỗi trang một style khác nhau.

====================================================

1. NGUYÊN TẮC BẮT BUỘC
   ====================================================

KHÔNG ĐƯỢC:

- thay đổi routing
- thay đổi authentication
- thay đổi authorization / phân quyền
- thay đổi API contract hiện có
- thay đổi database schema nếu không thật sự cần
- làm ảnh hưởng logic của các module khác
- dùng mock data
- hard-code dữ liệu
- làm mất dữ liệu khi F5
- làm hỏng giao diện của trang khác

Chỉ được tập trung vào UI/UX và style của Dashboard.

Mọi logic hiện tại phải giữ nguyên nếu đang hoạt động đúng.

====================================================
2. MỤC TIÊU STYLE
===================

Dashboard hiện tại đang bị cảm giác cũ, phẳng, nhiều khung vuông, chưa có hierarchy rõ.

Tôi muốn nâng cấp theo style giống Rewards:

- dark hero section
- gradient nhẹ
- card bo góc lớn
- shadow mềm
- khoảng trắng hợp lý
- typography rõ ràng
- nhìn hiện đại như Jira Cloud / Linear / ClickUp / Monday / SaaS 2025

Không đi theo kiểu:

- Jira Server cũ
- admin dashboard phẳng
- template bootstrap thô

====================================================
3. DESIGN SYSTEM PHẢI ĐỒNG BỘ
=================================

Hãy dùng Dashboard để đồng bộ style với Rewards theo các tiêu chí sau:

### Màu sắc

- Giữ cùng tone màu chủ đạo với Rewards
- Màu nền, màu card, màu heading, màu border, màu accent phải đồng bộ
- Tránh dùng quá nhiều màu lạ

### Card style

- Bo góc lớn
- Shadow nhẹ
- Border mảnh
- Padding rộng rãi
- Không dùng box vuông cứng

### Typography

- Tiêu đề rõ hierarchy
- Số liệu lớn, nổi bật
- Subtitle nhỏ hơn, nhẹ hơn
- Text phải thoáng và dễ đọc

### Spacing

- Căn lề đều
- Khoảng cách giữa các section rõ ràng
- Không để khoảng trắng chết quá lớn
- Không nhồi nội dung quá sát

### Button / Badge / Empty state

- Button, badge, empty state, progress bar phải cùng style với Rewards
- Các CTA phải nhìn cùng một hệ

====================================================
4. HERO SECTION CỦA DASHBOARD
==============================

Phần đầu Dashboard phải được nâng cấp thành hero section giống Rewards:

Hiển thị:

- tên project
- trạng thái project
- progress tổng
- deadline nếu có
- số task hoàn thành / tổng task

Thiết kế:

- khối hero nổi bật
- nền gradient nhẹ hoặc dark card như Rewards
- progress bar rõ ràng
- thông tin quan trọng nhìn trong 3 giây là hiểu

====================================================
5. KPI CARDS
============

Các card thống kê phải được làm lại theo style Rewards:

- Open Tasks
- Completed
- In Progress
- Blocked

Yêu cầu:

- đồng bộ khoảng cách
- icon rõ ràng hơn
- số lớn hơn
- label nhỏ gọn
- card bo góc đẹp hơn
- hover nhẹ nếu có thể

====================================================
6. CÁC SECTION BÊN DƯỚI
===========================

### Recent Tasks

- làm thành card đẹp hơn
- empty state phải sạch và dễ nhìn
- nếu có dữ liệu thật thì trình bày gọn

### Team Workload

- hiển thị theo style đồng bộ
- thêm thanh workload nếu cần
- nhìn trực quan hơn, không chỉ là text list

### Onboarding / Empty state

Nếu project chưa có dữ liệu:

- hiển thị empty state đẹp
- có CTA rõ ràng
- gợi ý bước tiếp theo
- không để người dùng nhìn vào một màn hình trống vô nghĩa

====================================================
7. ĐỒNG BỘ VỚI REWARDS
==========================

Dashboard phải lấy Rewards làm style reference.

Các yếu tố cần đồng bộ:

- card radius
- shadow
- border
- color palette
- section header
- icon size
- button style
- empty state style
- progress bar style
- overall spacing
- visual hierarchy

Dashboard và Rewards phải có cảm giác cùng một hệ thống.

====================================================
8. KHÔNG ĐƯỢC PHÁ LOGIC HIỆN CÓ
======================================

Không làm thay đổi:

- cách lấy dữ liệu dashboard
- cách tính task / progress / workload
- cách điều hướng sang board / work items
- cách chọn project
- các permission liên quan

Chỉ được làm đẹp và hợp nhất style.

Nếu cần refactor component UI chung thì phải làm theo hướng an toàn, không phá các trang khác.

====================================================
9. ƯU TIÊN TRIỂN KHAI
========================

Làm theo thứ tự:

1. nâng cấp hero section
2. nâng cấp KPI cards
3. nâng cấp Recent Tasks
4. nâng cấp Team Workload
5. nâng cấp empty state / onboarding
6. đồng bộ toàn bộ spacing, typography, card style theo Rewards

====================================================
10. DEFINITION OF DONE
======================

Chỉ được coi là xong khi:

- Dashboard nhìn hiện đại hơn rõ rệt
- Style đồng bộ với Rewards
- Không còn cảm giác mỗi trang một kiểu
- Không đổi logic nghiệp vụ
- Không ảnh hưởng module khác
- Build frontend thành công
- F5 không mất dữ liệu
- Dữ liệu vẫn là thật

Hãy đọc source hiện tại trước, xác định component Dashboard và các component style dùng chung, rồi cải tiến theo hướng tối ưu ít thay đổi logic nhất nhưng nâng cấp UI rõ rệt nhất.



====================================================
11. ƯU TIÊN LOGIC TRƯỚC UI
==============================

Đây KHÔNG PHẢI là task làm đẹp đơn thuần.

Dashboard phải là một màn hình nghiệp vụ hoạt động thực sự.

Tôi không muốn một dashboard đẹp nhưng chỉ hiển thị giao diện.

Trước khi sửa UI, hãy kiểm tra toàn bộ logic Dashboard hiện tại.

====================================================
DASHBOARD PHẢI PHẢN ÁNH DỮ LIỆU THẬT
==========================================

Mọi số liệu trên Dashboard phải lấy từ dữ liệu thật:

- Project
- Goal
- Sprint
- Task
- Team Member
- Reward
- Activity

Không dùng:

- mock data
- fake data
- random data
- hard-coded values

====================================================
PROJECT OVERVIEW
================

Progress phải được tính thật.

Ví dụ:

Project có 100 task

Done: 65

Progress = 65%

Không được hiển thị progress giả.

====================================================
OPEN TASKS
==========

Open Tasks phải lấy từ database.

Hiển thị số task đang:

- Todo
- Open
- Chưa hoàn thành

theo workflow hiện tại của hệ thống.

====================================================
COMPLETED TASKS
===============

Completed phải lấy từ trạng thái hoàn thành thật.

Không được tự cộng tay.

====================================================
IN PROGRESS
===========

In Progress phải phản ánh task đang thực hiện.

====================================================
BLOCKED TASKS
=============

Nếu hệ thống có trạng thái Blocked:

Hiển thị thật.

Nếu chưa có trạng thái Blocked:

Không tạo số liệu giả.

====================================================
RECENT TASKS
============

Recent Tasks phải lấy từ:

- task mới tạo
  hoặc
- task mới cập nhật

Sắp xếp theo thời gian thực tế.

====================================================
TEAM WORKLOAD
=============

Workload phải được tính từ:

- task đang được assign
- task đang active
- estimate
- actual effort

tùy dữ liệu hiện có.

Không hiển thị danh sách thành viên vô nghĩa.

====================================================
UPCOMING DEADLINES
==================

Nếu có:

- Goal deadline
- Sprint deadline
- Task deadline

thì phải lấy dữ liệu thật.

Sắp xếp:

gần hết hạn → xa hơn.

====================================================
EMPTY STATE PHẢI ĐÚNG NGHIỆP VỤ
====================================

Nếu project mới tạo:

Không hiển thị dashboard rỗng.

Hiển thị:

"Tạo Goal đầu tiên"

"Tạo Task đầu tiên"

"Mời thành viên"

Nếu project chưa có dữ liệu:

Dashboard phải hướng dẫn người dùng bước tiếp theo.

====================================================
KIỂM TRA TOÀN BỘ FLOW
========================

Sau khi hoàn thành:

Hãy tự kiểm tra:

1. Tạo project mới
2. Vào dashboard
3. Tạo goal
4. Tạo task
5. Gán task cho member
6. Hoàn thành task
7. Reload trang
8. Kiểm tra dashboard cập nhật đúng

Dashboard phải thay đổi theo dữ liệu thật.

====================================================
DEFINITION OF DONE
==================

Không được coi là hoàn thành nếu:

- Chỉ sửa CSS
- Chỉ sửa giao diện
- Chỉ thêm card đẹp hơn
- Chỉ thêm icon

Chỉ được coi là hoàn thành khi:

✓ Dashboard đẹp hơn

✓ Dashboard đồng bộ với Rewards

✓ Dashboard phản ánh dữ liệu thật

✓ Dashboard cập nhật khi dữ liệu thay đổi

✓ Dashboard hoạt động đúng nghiệp vụ

✓ Không phá logic cũ

✓ Không ảnh hưởng module khác

✓ Build frontend và backend thành công
