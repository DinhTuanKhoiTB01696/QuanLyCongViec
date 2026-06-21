# i18n Test Cases

## Phạm vi hiện tại

- Nguồn chính: `Frontend/src/i18n/index.js`.
- Ngôn ngữ hỗ trợ: `vi`, `en`.
- Mặc định: `vi`.
- Storage chính: `app_language`.
- Storage legacy: `admin_locale`, chỉ dùng để migrate.
- Helper mới: `t(key)`, `setLanguage(language)`, `getLanguage()`.
- Helper tương thích: `useLocale.t(en, vi)` và `toggleLocale()`.

Protected terms:

- `SprintA`.
- `DQLCV-*`, `PQLCV-*`, `SCRUM-*`.
- User name, project name và dữ liệu do user/backend cung cấp.
- Route, enum, API field, CSS class, component và file name.

## Test cases

| ID | Mục tiêu | Bước test | Kết quả mong đợi | Ưu tiên |
|---|---|---|---|---|
| I18N-CORE-001 | Ngôn ngữ mặc định | Xóa `app_language` và `admin_locale`; mở app | `getLanguage()` là `vi`; label tích hợp dùng tiếng Việt | P1 |
| I18N-CORE-002 | Set English | Gọi UI Profile hoặc `setLanguage('en')` | State đổi `en`, `app_language=en` | P1 |
| I18N-CORE-003 | Set Vietnamese | Từ English chọn Vietnamese | State đổi `vi`, `app_language=vi` | P1 |
| I18N-CORE-004 | Reject language lạ | Gọi `setLanguage('fr')` | Giữ language hiện tại, không crash, không ghi `fr` | P1 |
| I18N-CORE-005 | Persistence refresh | Chọn English rồi refresh | App tiếp tục dùng English | P1 |
| I18N-CORE-006 | Persistence tab mới | Chọn English, mở app ở tab mới cùng origin | Tab mới đọc `app_language=en` khi khởi tạo | P2 |
| I18N-CORE-007 | Storage unavailable | Chặn/throw localStorage bằng môi trường test | App không crash; dùng state/default an toàn | P1 |
| I18N-CORE-008 | Missing key | Gọi `t('missing.path')` | Trả chính key, không throw | P1 |
| I18N-CORE-009 | Empty/invalid key | Gọi `t('')` và giá trị không phải string qua harness phù hợp | Trả chuỗi rỗng, không throw | P2 |
| I18N-CORE-010 | Fallback sang vi | Tạo điều kiện key thiếu ở en nhưng có ở vi trong test branch/harness | Trả bản vi, không crash | P2 |
| I18N-CORE-011 | Interpolation | Gọi key có `{days}` hoặc `{start}/{end}/{total}` | Tham số được chèn đúng; placeholder thiếu vẫn giữ an toàn | P1 |
| I18N-MIG-001 | Migrate legacy vi | Xóa app_language; đặt `admin_locale=vi`; reload | `app_language=vi`; `admin_locale` bị xóa | P1 |
| I18N-MIG-002 | Migrate legacy en | Xóa app_language; đặt `admin_locale=en`; reload | `app_language=en`; `admin_locale` bị xóa | P1 |
| I18N-MIG-003 | app_language ưu tiên | Đặt `app_language=vi`, `admin_locale=en`; reload | Dùng vi và xóa legacy | P1 |
| I18N-MIG-004 | Legacy invalid | Đặt `admin_locale=fr`, không có app_language | Dùng vi; không ghi language không hỗ trợ | P2 |
| I18N-COMP-001 | useLocale đồng bộ | Mở component dùng `useLocale`; đổi language bằng Profile | Component đổi theo cùng `language` ref | P0 |
| I18N-COMP-002 | toggleLocale đồng bộ | Từ component legacy gọi `toggleLocale()` | `app_language` và component dùng `useI18n` cùng đổi | P0 |
| I18N-COMP-003 | Format date vi/en | Format cùng ISO bằng useLocale ở vi và en | Dùng locale `vi-VN`/`en-US`; invalid date trả rỗng | P2 |
| I18N-UI-001 | Profile selector | Mở Profile, đổi qua lại vi/en | Selector state và label language đúng ngay | P1 |
| I18N-UI-002 | Global sidebar | Đổi language và quan sát sidebar | Các label shell đã tích hợp đổi đúng danh mục locale | P1 |
| I18N-UI-003 | Global topbar search | Đổi language | Placeholder search đổi; route/icon/layout giữ nguyên | P1 |
| I18N-UI-004 | Summary | Đổi language khi Summary mở | Banner, metric/card labels đổi; task data giữ nguyên | P1 |
| I18N-UI-005 | List | Đổi language khi List mở | `New work item` đổi; status/data không bị dịch ngoài catalog | P1 |
| I18N-UI-006 | Development | Đổi language | Metric/connect labels đổi, illustration technical text giữ theo source | P2 |
| I18N-UI-007 | Forms | Đổi language | Heading/captions/options đổi, không thêm option | P2 |
| I18N-UI-008 | Timeline | Đổi language | Search/status/zoom labels đổi; mock issue data giữ nguyên | P2 |
| I18N-UI-009 | Docs | Đổi language | Card labels/actions đổi; product name nếu có giữ nguyên | P2 |
| I18N-UI-010 | Reports | Đổi language | Metric/chart/table labels cập nhật; chart không crash | P1 |
| I18N-UI-011 | Calendar | Đổi language | Toolbar, weekday, month label và panel labels đổi | P1 |
| I18N-PROT-001 | SprintA không dịch | Tìm tất cả vị trí hiển thị brand ở vi/en | Luôn là `SprintA` | P0 |
| I18N-PROT-002 | Issue key không dịch | Chuẩn bị `DQLCV-1`, `PQLCV-2`, `SCRUM-3`; đổi language | Key giữ nguyên ký tự và case | P0 |
| I18N-PROT-003 | User/project name không dịch | Dùng tên có dấu/English; đổi language | Tên giữ nguyên, không map qua catalog | P0 |
| I18N-PROT-004 | Task title không dịch | Quan sát cùng task ở Board/List/Summary/Calendar | Title giữ nguyên | P0 |
| I18N-PROT-005 | Technical identifiers | Quan sát route, request payload, enum/status raw, CSS/component qua DevTools | Không bị dịch hoặc đổi key | P0 |
| I18N-VIS-001 | Không overflow tiếng Việt | Chạy shell/project tabs ở viewport target bằng vi | Không overlap/cắt mất control cốt lõi | P1 |
| I18N-VIS-002 | Không overflow English | Lặp lại bằng en | Không overlap/cắt mất control cốt lõi | P1 |
| I18N-API-001 | Accept-Language đồng bộ | Chọn vi/en và quan sát request API | Header phản ánh `app_language` hiện tại; nếu vẫn đọc `admin_locale` thì ghi bug | P1 |
| I18N-REG-001 | Không dịch toàn app ngoài scope | So trước/sau đổi language | Chỉ label đã tích hợp đổi; text chưa tích hợp không bị biến đổi tự động | P1 |

## Danh sách shell cần đối chiếu

- New work item / Tạo công việc mới
- For you / Dành cho bạn
- Recent / Gần đây
- Starred / Đã đánh dấu sao
- Your work / Công việc của bạn
- Stickies / Ghi chú
- Rewards / Phần thưởng
- Workspace / Không gian làm việc
- Projects / Dự án
- More / Thêm
- Hide / Ẩn
- Views / Chế độ xem
- Analytics / Phân tích
- Archives / Lưu trữ
- Work items / Công việc
- Cycles / Chu kỳ
- Modules / Mô-đun
- Pages / Trang
- Community / Cộng đồng
- Search work items... / Tìm kiếm công việc...

