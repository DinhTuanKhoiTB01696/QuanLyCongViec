# Release Regression Checklist

## Thông tin release

- **Release/version:**
- **Commit/tag:**
- **Environment:**
- **Frontend URL:**
- **API URL:**
- **Database:**
- **Người chạy:**
- **Thời gian bắt đầu/kết thúc:**
- **Bug được chấp nhận:**
- **Quyết định:** GO / NO-GO

## 1. Pre-release

| ID | Mục tiêu | Bước test | Kết quả mong đợi | Ưu tiên |
|---|---|---|---|---|
| REL-PRE-001 | Diff đúng phạm vi | Review `git diff --name-only` và PR files | Không có secret, upload, migration/config ngoài scope release | P0 |
| REL-PRE-002 | Frontend production build | Chạy build theo pipeline dự án | Build pass, không có compile error | P0 |
| REL-PRE-003 | Backend build | Build solution/API theo pipeline | Build pass | P0 |
| REL-PRE-004 | Database migration review | Review migration áp dụng trong release và backup plan | Migration có thứ tự rõ, không xóa dữ liệu ngoài kế hoạch | P0 |
| REL-PRE-005 | Config/secret | Review environment config | Không commit secret; API URL/CORS/connection đúng environment | P0 |
| REL-PRE-006 | Test data | Xác nhận role/user/project QA | Có đủ data, không dùng production identity nhạy cảm | P1 |
| REL-PRE-007 | Screenshot baseline | Xác nhận target ID/viewport cho UI thay đổi | Target đúng inventory, không dùng `MKT-*` | P1 |

## 2. Deploy/startup

| ID | Mục tiêu | Bước test | Kết quả mong đợi | Ưu tiên |
|---|---|---|---|---|
| REL-DEP-001 | Backend startup | Start/deploy API và xem startup log | API khởi động, migration/seed không làm process crash | P0 |
| REL-DEP-002 | Frontend assets | Mở landing và một route lazy-loaded | JS/CSS/image tải `2xx`, không lỗi chunk | P0 |
| REL-DEP-003 | API connectivity | Gọi một public API và một protected API hợp lệ | Response đúng, không CORS/mixed-content error | P0 |
| REL-DEP-004 | Database persistence | Tạo dữ liệu test nhỏ rồi đọc lại | Dữ liệu persist trên expected database | P0 |
| REL-DEP-005 | SignalR connect | Login và quan sát kết nối hubs | Kết nối thành công hoặc fallback/error được xử lý rõ | P1 |

## 3. Auth/Profile

| ID | Mục tiêu | Bước test | Kết quả mong đợi | Ưu tiên |
|---|---|---|---|---|
| REL-AUTH-001 | Login/logout | Login normal user rồi logout | Cả hai thành công, route guard đúng | P0 |
| REL-AUTH-002 | Token refresh | Làm access token hết hạn trong phiên có refresh cookie | Request được retry bằng token mới, không loop | P0 |
| REL-AUTH-003 | Register/OTP | Chạy một registration flow QA | OTP/verify/register hoạt động | P1 |
| REL-AUTH-004 | 2FA | Login user bật 2FA | Challenge và verify hoạt động | P0 |
| REL-PROF-001 | Profile update | Sửa một field rồi reload | Dữ liệu persist | P1 |
| REL-PROF-002 | Password/security | Đổi password hoặc chạy flow OTP QA | Thành công; credential cũ bị vô hiệu theo contract | P0 |
| REL-PROF-003 | Session revoke | Revoke một session test | Session bị revoke không dùng lại được | P1 |

## 4. i18n và global shell

| ID | Mục tiêu | Bước test | Kết quả mong đợi | Ưu tiên |
|---|---|---|---|---|
| REL-I18N-001 | Default vi | Xóa language storage và reload | UI tích hợp dùng vi | P1 |
| REL-I18N-002 | Switch en/persist | Chọn English, refresh | Shell/profile/tab labels tích hợp giữ en | P1 |
| REL-I18N-003 | Legacy sync | Dùng component `useLocale` sau khi đổi Profile | Cùng language state | P0 |
| REL-I18N-004 | Protected terms | So key/title/user/project ở vi/en | Không bị dịch | P0 |
| REL-SHELL-001 | Sidebar/topbar | Điều hướng các item chính, mở user/notification menu | Active state, route và menu hoạt động | P1 |
| REL-SHELL-002 | Search | Tìm một work item từ control search hiện có | Kết quả đúng quyền và đúng key/title | P1 |

## 5. Project và work items

| ID | Mục tiêu | Bước test | Kết quả mong đợi | Ưu tiên |
|---|---|---|---|---|
| REL-PRJ-001 | Project list/open | Mở Spaces và `PROJECT-A` | Project hiển thị và mở được | P0 |
| REL-PRJ-002 | Project create/update | Tạo project QA, sửa tên/field, reload | Data persist, không duplicate | P0 |
| REL-PRJ-003 | Archive/restore | Archive rồi restore project QA | State chuyển đúng | P1 |
| REL-PRJ-004 | Member role | PM thêm member và đổi role | Quyền mới có hiệu lực | P0 |
| REL-WORK-001 | Create/open/update | Tạo task, mở detail, sửa title/status | Mọi thay đổi persist | P0 |
| REL-WORK-002 | Comment/attachment | Thêm comment và attachment QA | Data/file đúng task | P1 |
| REL-WORK-003 | Subtask/dependency | Tạo subtask và dependency hợp lệ | Relation đúng và reload được | P1 |
| REL-WORK-004 | Concurrency | Gửi update bằng RowVersion cũ | `409`, không ghi đè | P0 |
| REL-WORK-005 | Cross-project security | Thử member/sprint/module/task từ project khác | Bị từ chối, data không đổi | P0 |

## 6. Project tabs

| ID | Mục tiêu | Bước test | Kết quả mong đợi | Ưu tiên |
|---|---|---|---|---|
| REL-TAB-001 | Backlog | Mở, search, collapse, đổi status | Hoạt động theo frontend cases | P1 |
| REL-TAB-002 | Board | Mở và search key/title | Dark board render; không drag/modal ngoài scope | P1 |
| REL-TAB-003 | Summary | Mở với dataset có task | Metrics/cards/activity render, không placeholder | P1 |
| REL-TAB-004 | List | Kiểm group và row order | Không table header/cột cấm; row đúng sáu phần tử | P1 |
| REL-TAB-005 | Development | Mở tab | Metrics/connect state render; không API integration mới | P2 |
| REL-TAB-006 | Forms | Mở và click options | Create options render; không builder tự phát | P2 |
| REL-TAB-007 | Timeline | Mở và đổi zoom active | Grid render; active control đổi đúng source | P1 |
| REL-TAB-008 | Docs | Mở tab | Confluence card render; không editor tự phát | P2 |
| REL-TAB-009 | Reports | Mở, search table, paginate | Chart/table mock render và tương tác cục bộ đúng | P1 |
| REL-TAB-010 | Calendar | Navigate month, search panel, close/open panel | Grid/panel hoạt động; không drag/create modal | P1 |

## 7. Supporting modules

| ID | Mục tiêu | Bước test | Kết quả mong đợi | Ưu tiên |
|---|---|---|---|---|
| REL-MOD-001 | Cycles | Tạo/start/close sprint QA | State và carry-over đúng | P0 |
| REL-MOD-002 | Modules | CRUD module QA | Persist đúng project | P1 |
| REL-MOD-003 | Pages | CRUD/archive page QA | Persist và isolation đúng | P1 |
| REL-MOD-004 | Drafts | Save/update/publish draft | Lifecycle đúng | P1 |
| REL-MOD-005 | Notifications | Trigger/read/read-all | Badge/list cập nhật đúng user | P1 |
| REL-MOD-006 | Analytics/Rewards | Mở hai màn với data và empty state | Không crash, data đúng quyền | P2 |

## 8. Admin, permission và security

| ID | Mục tiêu | Bước test | Kết quả mong đợi | Ưu tiên |
|---|---|---|---|---|
| REL-SEC-001 | Admin route/API guard | Normal user mở admin và gọi API trực tiếp | UI redirect; API `403` | P0 |
| REL-SEC-002 | Admin access | System admin mở user/settings/system pages | Trang và API được phép theo role endpoint | P0 |
| REL-SEC-003 | Suspended user | Suspend user test đang có token rồi gọi API | Bị từ chối | P0 |
| REL-SEC-004 | Project outsider | Outsider gọi project/task APIs | `403`, không lộ data | P0 |
| REL-SEC-005 | Guest write | Guest thử POST/PUT/PATCH/DELETE project data | Không action nào thay đổi data | P0 |
| REL-SEC-006 | Protected role | Thử edit/delete protected role | Bị từ chối | P0 |
| REL-SEC-007 | IP whitelist | Admin đọc/cập nhật config QA an toàn | API đúng quyền; không khóa tester ngoài kế hoạch | P0 |
| REL-SEC-008 | AI rate limit | Gửi vượt 5 request/phút trong test kiểm soát | Nhận `429` sau limit | P1 |
| REL-SEC-009 | CORS | Test origin allowed và disallowed | Chỉ origin cấu hình được phép | P1 |
| REL-SEC-010 | Sensitive data | Kiểm response/log/browser storage | Không lộ password/hash/OTP/refresh token/secret | P0 |

## 9. Visual regression

| ID | Mục tiêu | Bước test | Kết quả mong đợi | Ưu tiên |
|---|---|---|---|---|
| REL-VQA-001 | Board/Summary/List | Chụp ba tab ở target viewport | Không có sai lệch Major so target/yêu cầu đã khóa | P1 |
| REL-VQA-002 | Development/Forms/Docs | Chụp các empty/connect state | Không thêm section/workflow ngoài ảnh | P1 |
| REL-VQA-003 | Timeline/Calendar | Chụp grid và panel | Alignment/overflow đúng | P1 |
| REL-VQA-004 | Reports | So phần nhìn thấy với target cropped | Cấu trúc đúng; vùng thiếu bằng chứng ghi xác nhận | P2 |
| REL-VQA-005 | vi/en overflow | Chụp shell và project tab bằng hai ngôn ngữ | Không overlap hoặc mất control | P1 |

## 10. Post-deploy

| ID | Mục tiêu | Bước test | Kết quả mong đợi | Ưu tiên |
|---|---|---|---|---|
| REL-POST-001 | Console/network errors | Chạy smoke và kiểm DevTools | Không có lỗi JS/API `5xx` lặp lại | P0 |
| REL-POST-002 | Audit/log monitoring | Kiểm log trong khoảng deploy/test | Không có exception tăng bất thường; action admin có audit phù hợp | P1 |
| REL-POST-003 | Data integrity | So count/project/task trước và sau deploy | Không mất hoặc duplicate dữ liệu ngoài test | P0 |
| REL-POST-004 | Cache/static refresh | Hard refresh và mở tab mới | Không lỗi asset cũ/chunk mismatch | P1 |
| REL-POST-005 | Rollback readiness | Xác nhận artifact, backup và lệnh rollback đã sẵn sàng | Có thể rollback mà không đoán thao tác | P0 |

## 11. GO/NO-GO

### GO khi

- Tất cả `P0` pass.
- Không còn Blocker/Critical.
- `P1` fail đã fix hoặc có chấp nhận rủi ro bằng văn bản.
- Migration, auth, permission và data integrity được xác nhận.

### NO-GO khi

- Có bypass quyền, mất dữ liệu, login không dùng được hoặc API core `5xx`.
- Build/deploy không tái lập được.
- Visual target chính sai màn hoặc thiếu vùng cốt lõi.
- Không có phương án rollback cho migration/rủi ro dữ liệu.

