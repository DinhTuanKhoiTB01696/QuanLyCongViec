# Frontend Manual Test Cases

## Quy ước

- Chạy bằng UI, có thể dùng DevTools để quan sát Network/Console/storage.
- Không thay thế backend/API cases trong `03_BACKEND_API_TEST_CASES.md`.
- Các giá trị SprintA, issue key, project name, user name và task title phải giữ nguyên.

## 1. Auth

| ID | Mục tiêu | Bước test | Kết quả mong đợi | Ưu tiên |
|---|---|---|---|---|
| FE-AUTH-001 | Login hợp lệ | Mở `/login`; nhập email/password hợp lệ; submit | Login thành công, lưu phiên trong session storage và đi tới route private phù hợp | P0 |
| FE-AUTH-002 | Login sai mật khẩu | Nhập email hợp lệ và password sai | Hiện lỗi, không lưu token, không vào route private | P0 |
| FE-AUTH-003 | Login 2FA | Login user bật 2FA; nhập mã hợp lệ | Hoàn tất login sau bước 2FA | P0 |
| FE-AUTH-004 | Mã 2FA sai | Nhập mã sai/hết hạn | Hiện lỗi, vẫn chưa tạo phiên | P1 |
| FE-AUTH-005 | Redirect sau login | Mở route private khi chưa login; login thành công | Quay lại route trong query `redirect` nếu luồng hỗ trợ | P1 |
| FE-AUTH-006 | Register với OTP | Mở `/register`; gửi OTP; verify; submit dữ liệu hợp lệ | Tài khoản được tạo theo response và không duplicate submit | P0 |
| FE-AUTH-007 | Register validation | Bỏ trống trường bắt buộc hoặc dùng dữ liệu không hợp lệ | Form chặn hoặc hiển thị lỗi backend; không tạo user | P1 |
| FE-AUTH-008 | Google login error handling | Thực hiện Google login với response lỗi/đóng popup | UI không crash, hiển thị lỗi hoặc giữ ở login | P2 |
| FE-AUTH-009 | GitHub callback | Mở callback với code hợp lệ | Gọi GitHub login, tạo session và điều hướng đúng | P1 |
| FE-AUTH-010 | Accept invite token | Mở `/accept-invite` với token hợp lệ; hoàn tất OTP nếu yêu cầu | Hiển thị invite info và accept thành công | P1 |
| FE-AUTH-011 | Invite token sai | Mở invite với token thiếu/sai/hết hạn | Hiển thị trạng thái lỗi, không thêm membership | P1 |
| FE-AUTH-012 | Guard public/private | Mở `/`, `/login`, `/register`; sau đó mở `/profile` khi logout | Public route mở; private route chuyển login | P0 |

## 2. Dashboard và global pages

| ID | Mục tiêu | Bước test | Kết quả mong đợi | Ưu tiên |
|---|---|---|---|---|
| FE-DASH-001 | Dashboard route hiện tại | Login; mở `/dashboard` | Render `ForYou.vue`, không lỗi console nghiêm trọng | P0 |
| FE-DASH-002 | For You tải task | Quan sát danh sách và Network | Task từ `/tasks/search` hiển thị đúng key/title/status | P1 |
| FE-DASH-003 | Cập nhật task từ For You | Sửa field được hỗ trợ trên một task | API thành công, UI và reload giữ giá trị | P1 |
| FE-GLOBAL-001 | Your Work | Mở `/your-work` | Active/archived project và task phù hợp hiển thị | P1 |
| FE-GLOBAL-002 | Stickies CRUD | Tạo, sửa, xóa sticky tại `/stickies` | Mỗi thao tác cập nhật đúng UI và dữ liệu | P2 |
| FE-GLOBAL-003 | Rewards | Mở `/rewards` | Wallet/leaderboard tải; trạng thái lỗi được xử lý | P2 |
| FE-GLOBAL-004 | Draft lifecycle | Tạo draft, sửa, reload, chuyển thành work item | Draft persist; khi publish thành công xử lý draft đúng source | P1 |
| FE-GLOBAL-005 | Global views | Mở `/views`; tìm/filter task nếu có | Project và task tải, UI không lộ project không được phép | P2 |
| FE-GLOBAL-006 | Analytics | Mở `/analytics` | Stats/planning summary render; không crash khi dataset rỗng | P1 |
| FE-GLOBAL-007 | Archives | Mở `/archives` hoặc `/spaces/archive`; restore một project hợp lệ | Redirect đúng; restore cập nhật danh sách | P1 |
| FE-GLOBAL-008 | Trash | Mở `/spaces/trash`; restore hoặc permanent delete theo quyền | Confirmation/response được xử lý; danh sách cập nhật đúng | P1 |

## 3. Global shell

| ID | Mục tiêu | Bước test | Kết quả mong đợi | Ưu tiên |
|---|---|---|---|---|
| FE-SHELL-001 | Sidebar render | Mở một route private desktop | Logo, navigation, workspace/project section render, không chồng nội dung | P0 |
| FE-SHELL-002 | Sidebar active state | Điều hướng qua các item sidebar | Item tương ứng có active state, URL đúng | P1 |
| FE-SHELL-003 | Sidebar labels i18n | Đổi vi/en từ Profile rồi quay lại shell | Các label đã tích hợp đổi theo language state | P1 |
| FE-SHELL-004 | Topbar render | Quan sát search, create, notification, settings/user controls hiện có | Icon và control đúng vị trí, không làm đổi route ngoài thao tác | P1 |
| FE-SHELL-005 | Topbar search | Nhập từ khóa vào search hiện tại | Kết quả từ endpoint hiện có hiển thị; xóa query reset | P1 |
| FE-SHELL-006 | Notification dropdown | Mở notification, đánh dấu một item/read all | Count và trạng thái read cập nhật theo API | P1 |
| FE-SHELL-007 | User menu/profile | Mở user menu; chọn Profile | Menu hiển thị user hiện tại, điều hướng `/profile` | P1 |
| FE-SHELL-008 | Logout từ shell | Chọn logout | Session bị xóa, về public/login, không truy cập route private bằng Back | P0 |
| FE-SHELL-009 | Shell overflow | Test viewport desktop hẹp hơn target | Nội dung không che control cốt lõi; vùng có thiết kế scroll vẫn scroll được | P2 |

## 4. Profile

| ID | Mục tiêu | Bước test | Kết quả mong đợi | Ưu tiên |
|---|---|---|---|---|
| FE-PROFILE-001 | Tải hồ sơ | Mở `/profile` | Dữ liệu `/users/me` map đúng form/card | P0 |
| FE-PROFILE-002 | Sửa hồ sơ | Sửa full name hoặc trường hỗ trợ; save | Thành công; reload vẫn đúng; không đổi email ngoài luồng email | P1 |
| FE-PROFILE-003 | Upload avatar | Chọn file ảnh hợp lệ và upload | Avatar mới hiển thị và persist | P1 |
| FE-PROFILE-004 | File avatar không hợp lệ | Chọn file sai loại/quá giới hạn nếu có | UI/backend từ chối an toàn, avatar cũ giữ nguyên | P2 |
| FE-PROFILE-005 | Danh sách email | Mở email card | Email và primary/verified state hiển thị đúng | P1 |
| FE-PROFILE-006 | Thêm email | Nhập email hợp lệ mới | Email được thêm theo response, duplicate bị xử lý | P1 |
| FE-PROFILE-007 | Đổi primary email | Chọn email đủ điều kiện làm primary | Primary state chuyển đúng và persist | P1 |
| FE-PROFILE-008 | Xóa email | Xóa email không primary theo luồng hỗ trợ | Email biến mất; không cho xóa dữ liệu bị bảo vệ | P1 |
| FE-PROFILE-009 | Đổi password | Gửi OTP; nhập current/new password hợp lệ; submit | Password đổi thành công, lỗi không làm mất phiên ngoài contract | P0 |
| FE-PROFILE-010 | Toggle 2FA | Bật/tắt 2FA | State cập nhật theo API và còn đúng sau reload | P0 |
| FE-PROFILE-011 | Session list | Mở security card | Sessions và login activity tải đúng | P1 |
| FE-PROFILE-012 | Revoke session | Revoke một session khác | Session bị xóa khỏi danh sách; session hiện tại không bị ảnh hưởng ngoài lựa chọn | P1 |
| FE-PROFILE-013 | Revoke other sessions | Chọn revoke all others | Các session khác bị xóa, session hiện tại còn hoạt động | P1 |
| FE-PROFILE-014 | Language selector | Chọn Tiếng Việt rồi English | Label tích hợp đổi ngay; `app_language` cập nhật | P1 |

## 5. Space/Project

| ID | Mục tiêu | Bước test | Kết quả mong đợi | Ưu tiên |
|---|---|---|---|---|
| FE-SPACE-001 | Project discovery | Mở `/spaces` | Chỉ project discoverable/accessible hiển thị theo API | P0 |
| FE-SPACE-002 | Tạo project | Mở create modal; nhập dữ liệu hợp lệ; submit | Project mới xuất hiện và mở được | P0 |
| FE-SPACE-003 | Validation tạo project | Submit thiếu field bắt buộc hoặc dữ liệu duplicate | Không tạo project; lỗi rõ ràng | P1 |
| FE-SPACE-004 | Favorite project | Toggle favorite | Trạng thái favorite cập nhật và còn sau reload | P1 |
| FE-SPACE-005 | Mở project redirect | Mở `/space/{id}` | Redirect `/space/{id}/work-items` | P1 |
| FE-SPACE-006 | Archive project | User đủ quyền archive project | Project chuyển khỏi active list vào archive | P1 |
| FE-SPACE-007 | Restore archived project | Restore từ archive | Project trở lại active list | P1 |
| FE-SPACE-008 | Soft delete và restore | Delete theo UI rồi restore từ trash | Project chuyển đúng giữa active/trash | P1 |
| FE-SPACE-009 | Permanent delete | Xóa vĩnh viễn project trong trash | Project không còn; thao tác bị chặn nếu thiếu quyền | P0 |
| FE-SPACE-010 | Project settings guard | User member thường mở `/space/{id}/settings` | Router/API từ chối và đưa về dashboard | P0 |
| FE-SPACE-011 | Project settings manager | PM mở settings, sửa field hợp lệ | Trang tải và save thành công | P1 |
| FE-SPACE-012 | Members management | PM thêm member, đổi role, xóa member | Danh sách cập nhật; quyền mới có hiệu lực | P0 |
| FE-SPACE-013 | Status/label/module settings | Tạo/sửa/xóa dữ liệu cấu hình được hỗ trợ | API/UI đồng bộ; item đang được dùng xử lý lỗi an toàn | P1 |
| FE-SPACE-014 | Execution rules | PM thay rule và reload | Rule persist và tác động đúng nơi source sử dụng | P1 |

## 6. Work items và issue detail

| ID | Mục tiêu | Bước test | Kết quả mong đợi | Ưu tiên |
|---|---|---|---|---|
| FE-WORK-001 | Tạo work item | Từ control create hiện có, nhập title/status hợp lệ | Item được tạo với key/title đúng | P0 |
| FE-WORK-002 | Chống duplicate submit | Submit create nhanh nhiều lần | Không tạo duplicate ngoài số lần request thực tế | P1 |
| FE-WORK-003 | Mở detail | Chọn item trong view hỗ trợ | Modal/detail hiển thị đúng project và item | P0 |
| FE-WORK-004 | Sửa title/description | Sửa field và lưu/blur theo UI | Dữ liệu cập nhật, reload còn đúng | P0 |
| FE-WORK-005 | Đổi status | Chọn status khác | Status pill/list/board đồng bộ sau reload | P0 |
| FE-WORK-006 | Sửa assignee | Thêm/bỏ assignee theo control hiện có | Avatar/member state cập nhật và persist | P1 |
| FE-WORK-007 | Priority/due date | Sửa priority và due date trong detail | Giá trị mới hiển thị đúng, không sai timezone ngày-only | P1 |
| FE-WORK-008 | Comment | Tạo comment text hợp lệ | Comment xuất hiện đúng tác giả/thời gian | P1 |
| FE-WORK-009 | Edit/delete comment | Sửa rồi xóa comment do user có quyền | UI và backend cập nhật đúng | P1 |
| FE-WORK-010 | Comment attachment | Upload attachment hợp lệ; preview/download nếu có | File liên kết đúng comment; lỗi file được xử lý | P1 |
| FE-WORK-011 | Reaction | Toggle reaction trên comment | Count/state thay đổi đúng, không duplicate reaction | P2 |
| FE-WORK-012 | Label | Tạo/chọn/bỏ label | Label chip và backend đồng bộ | P1 |
| FE-WORK-013 | Subscription | Toggle subscribe | Trạng thái subscription cập nhật và persist | P2 |
| FE-WORK-014 | Subtask | Tạo subtask từ parent; tải lại | Subtask liên kết đúng parent, key/title không đổi | P1 |
| FE-WORK-015 | Dependency | Thêm rồi xóa dependency | Quan hệ hiển thị đúng; không tự liên kết item với chính nó | P1 |
| FE-WORK-016 | Time log | Thêm time log hợp lệ | Log được lưu, giá trị không âm/không sai đơn vị | P2 |
| FE-WORK-017 | Audit/history | Mở activity/history trong detail | Chỉ log liên quan hiển thị theo API/filter | P2 |
| FE-WORK-018 | Project isolation | Dùng URL/task ID project khác trong current project | Không lộ hoặc sửa task ngoài quyền | P0 |

## 7. Backlog

| ID | Mục tiêu | Bước test | Kết quả mong đợi | Ưu tiên |
|---|---|---|---|---|
| FE-BACKLOG-001 | Render groups | Chọn Backlog | Group Board/Backlog hiện theo logic source và count đúng dataset | P1 |
| FE-BACKLOG-002 | Search | Tìm theo title và key | Chỉ item khớp hiển thị; xóa search khôi phục | P1 |
| FE-BACKLOG-003 | Collapse group | Click group header | Group đóng/mở, dữ liệu không đổi | P2 |
| FE-BACKLOG-004 | Selection | Chọn row và chọn group checkbox | Selection state đúng số item visible | P2 |
| FE-BACKLOG-005 | Update status | Mở status dropdown và chọn status | Emit/update thành công, chip đổi đúng | P1 |
| FE-BACKLOG-006 | Open detail/create | Click row; click Create ở group | Row mở detail; Create truyền default status của group | P1 |

## 8. Board

| ID | Mục tiêu | Bước test | Kết quả mong đợi | Ưu tiên |
|---|---|---|---|---|
| FE-BOARD-001 | Board shell | Chọn Board | ProjectHeader, ProjectTabBar, toolbar và canvas render dark | P1 |
| FE-BOARD-002 | Columns/count | Đối chiếu mock hiện tại | TO DO, IN PROGRESS, DONE và count hiển thị theo mock/totalCount | P1 |
| FE-BOARD-003 | Card fidelity | Kiểm card visible | Key/title/assignee/status từ mock giữ nguyên | P1 |
| FE-BOARD-004 | Search title | Nhập một phần title | Chỉ card khớp còn trong column | P1 |
| FE-BOARD-005 | Search key | Nhập issue key chính xác | Card đúng được giữ; key không bị dịch | P1 |
| FE-BOARD-006 | Static controls | Click Filter, Group, complete sprint, create, ellipsis, add column | Không mở modal/dropdown hoặc gọi API ngoài source hiện tại | P1 |
| FE-BOARD-007 | Không drag/drop | Thử kéo card | Không phát sinh reorder hoặc API drag/drop | P1 |
| FE-BOARD-008 | Chuyển tab | Chọn tab khác từ ProjectTabBar | Emit `select-tab`, render view tương ứng, URL tổng không đổi | P1 |
| FE-BOARD-009 | Horizontal overflow | Thu hẹp viewport | Canvas scroll ngang, không tự chuyển mobile layout | P2 |

## 9. Summary và List

| ID | Mục tiêu | Bước test | Kết quả mong đợi | Ưu tiên |
|---|---|---|---|---|
| FE-SUMMARY-001 | Summary không còn placeholder | Chọn Summary | `SummaryTab` render thay placeholder | P1 |
| FE-SUMMARY-002 | Metrics | Chuẩn bị task trong/ngoài 7 ngày; mở Summary | Completed/updated/created/due soon tính theo dữ liệu source | P1 |
| FE-SUMMARY-003 | Status overview | So count chart/legend với task | Tổng và breakdown đúng, empty dataset không lỗi | P1 |
| FE-SUMMARY-004 | Recent activity | Click một activity row | Mở đúng task detail | P2 |
| FE-SUMMARY-005 | Protected data | Đổi ngôn ngữ | User name, issue key, title vẫn nguyên bản | P1 |
| FE-LIST-001 | Group theo status | Chọn List | Group dùng status name, có chevron/icon/name/count | P1 |
| FE-LIST-002 | Row order | Quan sát row | Star, key, title, status pill, caret, assignee icon đúng thứ tự | P1 |
| FE-LIST-003 | Không có table header/cột cấm | Quan sát toàn List | Không render Key/Summary/... header; không có Created/Updated/Due/Resolved/Labels/Comments/Reporter/Sprint/Priority | P1 |
| FE-LIST-004 | Collapse group | Click chevron/header | Group đóng/mở đúng | P2 |
| FE-LIST-005 | Star | Click star một task | Star state thay đổi theo logic hiện có, không đổi key/title | P2 |
| FE-LIST-006 | Status/assignee display | So row với task data | Status pill và assignee icon lấy từ task/member hiện tại | P1 |
| FE-LIST-007 | Row caret static | Click caret | Không mở menu/modal vì chưa có bằng chứng/logic | P1 |
| FE-LIST-008 | New work item label | Đổi vi/en | Label dùng i18n; hành động create giữ status group | P1 |

## 10. Development, Forms, Timeline, Docs, Reports, Calendar

| ID | Mục tiêu | Bước test | Kết quả mong đợi | Ưu tiên |
|---|---|---|---|---|
| FE-DEV-001 | Metrics từ task | Mở Development với dataset có done/overdue/reopened/bug | Các metric source-based tính đúng; metric integration chưa có giữ 0 | P1 |
| FE-DEV-002 | Connect state | Quan sát related work/connect area | Render đúng state hiện tại, không tự kết nối tool hoặc gọi API | P1 |
| FE-FORMS-001 | Forms create options | Chọn Forms | Hiển thị đúng sáu option source hiện tại | P1 |
| FE-FORMS-002 | Không có builder | Click từng create option | Không mở builder/submission/backend nếu source chưa gắn action | P1 |
| FE-TIMELINE-001 | Timeline mock | Chọn Timeline | Toolbar/grid dùng mock, Months active ban đầu | P1 |
| FE-TIMELINE-002 | Zoom visual state | Click Weeks/Months/Quarters | Active state đổi; không kỳ vọng thay grid ngoài logic hiện có | P2 |
| FE-TIMELINE-003 | Static controls | Click Today/status/options/more/info/next nếu không có handler | Không phát sinh workflow/API ngoài source | P2 |
| FE-DOCS-001 | Confluence card | Chọn Docs | Card, illustration, description và hai action render | P1 |
| FE-DOCS-002 | Không có editor | Click action nếu không có handler | Không tự mở editor/page tree/backend | P1 |
| FE-REPORTS-001 | Metrics/charts | Chọn Reports | Metric và bảy chart/card khu vực render từ mock không lỗi | P1 |
| FE-REPORTS-002 | Table search | Tìm theo key/title/type/priority hỗ trợ | Row mock lọc đúng; page reset về 1 | P1 |
| FE-REPORTS-003 | Pagination | Chuyển trang tới biên đầu/cuối | Disable đúng, range text đúng total filtered | P2 |
| FE-REPORTS-004 | Language reactivity | Đổi vi/en khi Reports đang mở | Label/chart legend cập nhật, row summary/key không dịch | P1 |
| FE-CALENDAR-001 | Month grid | Chọn Calendar | Weekday, date cells và outside-month state render đúng default month | P1 |
| FE-CALENDAR-002 | Month navigation | Click previous/next/Today | Month label/grid cập nhật đúng locale | P1 |
| FE-CALENDAR-003 | Unscheduled panel | Đóng/mở panel | Panel toggle đúng, calendar không vỡ layout | P1 |
| FE-CALENDAR-004 | Search unscheduled | Tìm theo title/key/status | Card mock lọc đúng; dữ liệu không dịch | P1 |
| FE-CALENDAR-005 | Không drag/create | Thử kéo card hoặc click vùng ngày | Không drag/drop, create modal hoặc API mới | P1 |

## 11. Cycles, Modules, Pages, Intakes và Views

| ID | Mục tiêu | Bước test | Kết quả mong đợi | Ưu tiên |
|---|---|---|---|---|
| FE-CYCLE-001 | Danh sách sprint | Mở `/space/{id}/cycles` | Sprints tải đúng project | P1 |
| FE-CYCLE-002 | Start/close sprint | PM thực hiện start rồi close trên data phù hợp | State cập nhật; carry-over xử lý theo response | P0 |
| FE-CYCLE-003 | Burndown | Mở sprint có dữ liệu | Chart/data burndown tải hoặc empty state an toàn | P2 |
| FE-MODULE-001 | Module CRUD | Tạo, sửa, disable/xóa module | UI/API đồng bộ, project isolation đúng | P1 |
| FE-PAGE-001 | Page CRUD | Tạo, mở, sửa, archive/delete page | Nội dung và state persist; không mở page project khác | P1 |
| FE-INTAKE-001 | Intake lifecycle | Tạo intake và review | Status review cập nhật đúng | P2 |
| FE-VIEW-001 | Saved project view | Tạo/favorite/delete view | View persist và danh sách cập nhật | P2 |

## 12. Error, loading và accessibility cơ bản

| ID | Mục tiêu | Bước test | Kết quả mong đợi | Ưu tiên |
|---|---|---|---|---|
| FE-ERR-001 | API `401` toàn cục | Làm token invalid rồi gọi API từ UI | Session được xử lý an toàn, không loop refresh vô hạn | P0 |
| FE-ERR-002 | API `403` | Thực hiện action thiếu quyền | Có feedback/redirect phù hợp, dữ liệu UI không optimistic sai | P0 |
| FE-ERR-003 | API `500`/network down | Tắt backend hoặc mock lỗi khi tải page | UI không trắng toàn màn; có lỗi hoặc empty fallback hợp lý | P1 |
| FE-ERR-004 | Loading/duplicate action | Throttle network và submit action write | Không tạo request lặp ngoài thiết kế | P1 |
| FE-A11Y-001 | Keyboard cơ bản | Tab qua button/input ở auth, profile và project toolbar | Focus có thể tới control; không bị kẹt keyboard | P2 |
| FE-A11Y-002 | Accessible names | Kiểm icon-only button chính | Có title/aria-label phù hợp với label i18n hoặc chức năng | P2 |

