# Jira Screenshot Visual QA

## 1. Quy tắc

1. Chỉ so với screenshot target được inventory đánh dấu dùng được.
2. Không dùng `MKT-*` để đánh giá project app.
3. Với `usable_annotated`, bỏ qua nét khoanh đỏ, mũi tên và chú thích tay.
4. Không coi UI Jira thật ngoài bộ ảnh là expected.
5. Nếu ảnh crop hoặc chi tiết không đọc được, ghi `[CẦN XÁC NHẬN]`.
6. Dữ liệu động như user/project/issue key chỉ cần đúng cấu trúc và không bị dịch; không ép trùng dữ liệu nếu target không phải mock đã khóa.

## 2. Thiết lập chụp

- Browser: Chromium, zoom 100%.
- Tắt extension dịch trang và overlay không thuộc app.
- Theme đúng target.
- Viewport theo kích thước inventory; ưu tiên chính xác width/height.
- Cùng route, tab, language, popup/panel state và dữ liệu với target.
- Chờ font, icon, image và chart render ổn định trước khi chụp.
- Không resize target để che sai khác; nếu cần overlay thì scale cả hai theo cùng tỷ lệ.

## 3. Mức sai lệch

| Mức | Ví dụ |
|---|---|
| Blocker | Sai màn, thiếu vùng chính, shell che nội dung, app trắng |
| Major | Sai layout/cột/panel, spacing lớn, màu nền/theme sai |
| Minor | Typography, icon size, border, padding lệch thấy rõ |
| Cosmetic | Sai 1-2 px hoặc anti-aliasing không ảnh hưởng tổng thể |

## 4. Ma trận target

| Khu vực | Target chính | Status | Viewport tham chiếu |
|---|---|---|---|
| Global sidebar | `GS-01-global-sidebar-annotated` | usable_annotated | 3396x1760 |
| Settings menu | `SETTINGS-01-global-settings-menu-dark` | usable | 506x624 |
| User menu | `USER-01-user-menu-dark` | usable | 303x355 |
| Board | `BOARD-02-project-board-dark` | usable | 1911x901 |
| Summary | `SUMMARY-02-project-summary-dark` | usable | 1900x914 |
| List | `LIST-01-project-list-dark` | usable | 1904x902 |
| Development | `DEVELOPMENT-02-project-development-dark` | usable | 1907x896 |
| Forms | `FORMS-04-project-forms-dark-create-options` | usable | 1908x888 |
| Timeline | `TIMELINE-02-project-timeline-dark` | usable | 1917x898 |
| Docs | `DOCS-02-project-docs-dark-confluence-card` | usable | 1896x909 |
| Reports | `REPORTS-01`, `REPORTS-02` | usable | 575x848, cropped |
| Calendar | `CALENDAR-02-project-calendar-dark-unscheduled-panel` | usable | 1908x922 |
| Backlog | `BACKLOG-01`, `BACKLOG-02` | usable | 1763x844 |
| Issue detail | `ISSUE-01` và các ảnh `ISSUE-*` liên quan | usable/annotated | Theo inventory |

## 5. Visual test cases

| ID | Mục tiêu | Bước test | Kết quả mong đợi | Ưu tiên |
|---|---|---|---|---|
| VQA-BASE-001 | Đúng viewport/state | Mở target route; đặt viewport/theme/language/panel state; chụp | Evidence ghi đủ state và cùng tỷ lệ target | P1 |
| VQA-BASE-002 | Không có overlay ngoài app | Kiểm tra translate bar, DevTools overlay, tooltip và cursor | Screenshot sạch, chỉ chứa UI app | P1 |
| VQA-BASE-003 | Annotation không bị clone | So màn với target `usable_annotated` | Không có nét đỏ/chú thích tay trong app | P1 |
| VQA-SHELL-001 | Sidebar structure | So shell với `GS-01` ở vùng được đánh dấu | Width, hierarchy, icon/label alignment và active state gần target; bỏ annotation | P1 |
| VQA-SHELL-002 | Settings menu | Mở gear menu và so `SETTINGS-01` | Kích thước, background, spacing và menu hierarchy đúng target/source | P2 |
| VQA-SHELL-003 | User menu | Mở avatar menu và so `USER-01` | Menu width, item spacing, divider và dark palette khớp | P2 |
| VQA-BOARD-001 | Board composition | Mở Board ở 1911x901 | Project header, tabs, toolbar, canvas và ba cột đúng bố cục `BOARD-02` | P1 |
| VQA-BOARD-002 | Board colors | Overlay/chụp vùng board | Nền `#1d1f21` gần target; cột/card/border/accent không lệch theme lớn | P1 |
| VQA-BOARD-003 | Board cards/count | So column header/card visible | Count và card visible theo mock; không tạo card bị khuất để đủ total | P1 |
| VQA-BOARD-004 | Board overflow | Thu viewport rồi kiểm canvas | Scroll ngang; cột không co thành mobile layout tự tạo | P2 |
| VQA-SUM-001 | Summary top region | So banner, filter và metric cards với `SUMMARY-02` | Vị trí, kích thước, spacing và dark palette gần target | P1 |
| VQA-SUM-002 | Summary cards | So status/activity/priority/type/workload cards | Grid, card border, chart placeholder/data region đúng cấu trúc ảnh | P1 |
| VQA-LIST-001 | List grouped override | Mở List và kiểm theo yêu cầu đã khóa cùng `LIST-01` | Không có full table header; group theo status | P1 |
| VQA-LIST-002 | List group header | So từng group | Chevron/icon, uppercase status name và count thẳng hàng | P1 |
| VQA-LIST-003 | List row order | So một row từ trái sang phải | Star, issue key, title, status pill, caret, assignee icon; không có cột ngày/priority/reporter | P1 |
| VQA-DEV-001 | Development metrics | So với `DEVELOPMENT-02` | Key metrics grid, related tabs và dark spacing đúng cấu trúc | P1 |
| VQA-DEV-002 | Development connect state | So illustration, title, copy, actions, feedback | Empty/connect state đúng; không có integration UI ngoài ảnh | P1 |
| VQA-FORM-001 | Forms landing | So với `FORMS-04` | Heading, flow illustration/captions và create panel đúng bố cục | P1 |
| VQA-FORM-002 | Forms options | So option count/icon/tone | Chỉ option có trong source/target; không thêm builder panel | P1 |
| VQA-TIME-001 | Timeline toolbar/grid | So với `TIMELINE-02` | Search/avatar/status/options, month grid và rows đúng dark composition | P1 |
| VQA-TIME-002 | Timeline zoom | So control góc dưới | Today/Weeks/Months/Quarters và icon đúng vị trí; Months active ban đầu | P2 |
| VQA-DOC-001 | Docs Confluence card | So với `DOCS-02` | Card full-width, illustration trái, content/actions phải và palette đúng | P1 |
| VQA-REP-001 | Reports structure | So từng vùng với `REPORTS-01/02` | Metrics/chart/detail table theo source; không dùng Dashboard thay Reports | P1 |
| VQA-REP-002 | Reports cropped uncertainty | Đối chiếu phần ngoài khung ảnh reports | Chỉ đánh giá cấu trúc nhìn thấy; phần không đọc được ghi `[CẦN XÁC NHẬN]` | P1 |
| VQA-CAL-001 | Calendar toolbar/grid | So với `CALENDAR-02` ở 1908x922 | Toolbar, weekday row, month cells và border alignment gần target | P1 |
| VQA-CAL-002 | Unscheduled panel | Để panel mở và so target | Panel width, header, search, sort/filter và card stack đúng cấu trúc | P1 |
| VQA-BACK-001 | Backlog light | So Backlog với `BACKLOG-01` | Toolbar, groups, rows và detail-free state gần target | P2 |
| VQA-BACK-002 | Backlog detail panel | Mở item nếu UI hiện hỗ trợ panel và so `BACKLOG-02` | Panel chỉ được đánh giá nếu đúng state/source; không tự thêm panel khác | P2 |
| VQA-ISSUE-001 | Issue detail main | Mở TaskDetail và so `ISSUE-01` | Header/content/metadata/activity hierarchy đúng vùng ảnh áp dụng | P1 |
| VQA-ISSUE-002 | Annotated issue sections | So subtasks, dependencies, attachments, activity với ảnh `ISSUE-*` | Kiểm vùng nội dung; bỏ nét annotation | P2 |
| VQA-I18N-001 | Visual vi/en | Chụp cùng màn bằng vi và en | Text có thể dài/ngắn nhưng không overlap, truncate sai hoặc đẩy mất control | P1 |
| VQA-DATA-001 | Protected data visual | Đổi language và so issue/user/project fields | Identifier và dynamic data giữ nguyên, không biến dạng | P1 |

## 6. Checklist cho từng lần so ảnh

- [ ] Đúng screenshot ID.
- [ ] Đúng route/tab.
- [ ] Đúng viewport và zoom.
- [ ] Đúng theme/language.
- [ ] Đúng panel/menu state.
- [ ] Không có overlay ngoài app.
- [ ] Shell và content boundary đúng.
- [ ] Background, surface, border và accent đúng nhóm màu.
- [ ] Typography hierarchy đúng.
- [ ] Icon đúng loại, kích thước và alignment.
- [ ] Spacing/padding/gap không lệch lớn.
- [ ] Scroll/overflow đúng.
- [ ] Không có section ngoài ảnh/yêu cầu.
- [ ] Không clone annotation.
- [ ] Mọi vùng không đọc được đã ghi `[CẦN XÁC NHẬN]`.

## 7. Evidence format

Tên file đề xuất:

`{build}_{case-id}_{screenshot-id}_{language}_{viewport}_{result}.png`

Ví dụ:

`2026.06.15_VQA-BOARD-001_BOARD-02_vi_1911x901_PASS.png`

