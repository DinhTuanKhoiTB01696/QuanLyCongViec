# Smoke Test Checklist

## Điều kiện

- Có `USR-NORMAL`, `USR-ADMIN`, `PROJECT-A`.
- Frontend và backend đang chạy.
- Chạy trên Chromium desktop.

| ID | Mục tiêu | Bước test | Kết quả mong đợi | Ưu tiên |
|---|---|---|---|---|
| SMK-AUTH-001 | Landing/public route mở được | Mở `/` khi chưa đăng nhập | Landing hiển thị, không crash | P0 |
| SMK-AUTH-002 | Guard route private | Khi chưa đăng nhập, mở `/dashboard` | Chuyển tới `/login` và giữ query `redirect` | P0 |
| SMK-AUTH-003 | Đăng nhập cơ bản | Đăng nhập bằng `USR-NORMAL` hợp lệ | Nhận phiên hợp lệ và vào trang private | P0 |
| SMK-AUTH-004 | Refresh vẫn giữ phiên trong tab | Sau login, refresh `/dashboard` | User vẫn đăng nhập trong cùng tab/session | P0 |
| SMK-AUTH-005 | Logout | Chọn logout từ user menu | Token/user bị xóa và route private không còn truy cập được | P0 |
| SMK-SHELL-001 | Global shell render | Login và mở `/dashboard` | Sidebar/topbar hiển thị, không vỡ layout | P0 |
| SMK-SHELL-002 | Điều hướng sidebar | Mở lần lượt For you, Your work, Stickies, Rewards, Spaces | Route và active state cập nhật đúng | P1 |
| SMK-I18N-001 | Đổi sang English | Vào Profile, chọn English | Label i18n đã tích hợp đổi sang English | P1 |
| SMK-I18N-002 | Lưu ngôn ngữ | Refresh sau khi chọn English | `app_language=en`, UI vẫn dùng English | P1 |
| SMK-SPACE-001 | Danh sách project | Mở `/spaces` | Project user được phép xem hiển thị | P0 |
| SMK-SPACE-002 | Mở project | Chọn `PROJECT-A` | Mở `/space/{id}/work-items`, không có forbidden overlay | P0 |
| SMK-SPACE-003 | Chặn project ngoài quyền | Dùng `USR-NORMAL` mở `PROJECT-B` | UI/API từ chối, không lộ dữ liệu project | P0 |
| SMK-WORK-001 | Tải work items | Mở project có dữ liệu | Work item và status tải được | P0 |
| SMK-WORK-002 | Mở chi tiết work item | Chọn một row/card ở view hỗ trợ | Detail hiện đúng key/title của item | P1 |
| SMK-WORK-003 | Cập nhật status | Đổi status một work item ở view hỗ trợ | UI cập nhật và dữ liệu còn đúng sau reload | P0 |
| SMK-BACKLOG-001 | Backlog render | Chọn tab Backlog | Hai group hiện theo dữ liệu, search không crash | P1 |
| SMK-BOARD-001 | Board dark render | Chọn tab Board | Header, toolbar và các column/card mock hiển thị | P1 |
| SMK-BOARD-002 | Board search | Tìm theo title/key rồi xóa | Card lọc đúng; xóa trả lại danh sách | P1 |
| SMK-SUMMARY-001 | Summary render | Chọn tab Summary | Banner, metrics, status, activity và breakdown render | P1 |
| SMK-LIST-001 | List grouped status | Chọn tab List | Group status và count hiển thị; không có table header đầy đủ | P1 |
| SMK-LIST-002 | Thứ tự row List | Quan sát một row | Star, key, title, status, caret, assignee đúng thứ tự | P1 |
| SMK-DEV-001 | Development render | Chọn tab Development | Metrics và connect state hiện, không phát sinh API mới | P2 |
| SMK-FORMS-001 | Forms render | Chọn tab Forms | Empty/create options hiện, không mở builder tự phát | P2 |
| SMK-TIMELINE-001 | Timeline render | Chọn tab Timeline | Toolbar, grid tháng và zoom controls hiện | P1 |
| SMK-DOCS-001 | Docs render | Chọn tab Docs | Confluence card và hai action hiển thị | P2 |
| SMK-REPORTS-001 | Reports render | Chọn tab Reports | Metrics, charts và detail table mock render không lỗi | P1 |
| SMK-CALENDAR-001 | Calendar render | Chọn tab Calendar | Month grid và unscheduled panel hiển thị | P1 |
| SMK-PROFILE-001 | Profile tải | Mở `/profile` | Thông tin user và các profile card render | P1 |
| SMK-PROFILE-002 | Cập nhật profile | Sửa một trường hợp lệ và lưu | Thành công, reload vẫn thấy dữ liệu mới | P1 |
| SMK-ADMIN-001 | Admin guard | Dùng `USR-NORMAL` mở `/admin` | Chuyển về `/dashboard` với trạng thái denied | P0 |
| SMK-ADMIN-002 | Admin dashboard | Dùng `USR-ADMIN` mở `/admin/system/general-configuration` | Trang admin render và gọi API thành công | P0 |
| SMK-API-001 | API thiếu token | Gọi `GET /api/projects` không token | Trả `401`, không trả dữ liệu | P0 |
| SMK-API-002 | Health qua API cốt lõi | Với token hợp lệ gọi users/me, projects/discovery, project tasks | Các endpoint trả response hợp lệ, không có `5xx` | P0 |
| SMK-SEC-001 | User bị khóa | Gọi admin/project API bằng `USR-SUSPENDED` | Bị từ chối theo filter/quy tắc hiện tại | P0 |
| SMK-SEC-002 | Không vượt project role | Dùng Guest thử write vào project | API trả `403`, dữ liệu không đổi | P0 |

## Gate smoke

- Release bị chặn nếu bất kỳ case `P0` fail.
- Case `P1` fail cần bug và quyết định rõ trước khi tiếp tục regression.

