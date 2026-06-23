# Backend API Test Cases

## Quy ước

- Base URL API mặc định: `http://localhost:5136/api`.
- Dùng database QA và token riêng cho từng role.
- Với response dùng `ApiResponse<T>`, kiểm `statusCode`, `message`, `data`.
- Với write API, luôn GET lại hoặc kiểm database để xác nhận persistence.
- Không ghi token, refresh cookie, password hoặc OTP thật vào evidence.

## 1. Authentication

| ID | Mục tiêu | Bước test | Kết quả mong đợi | Ưu tiên |
|---|---|---|---|---|
| API-AUTH-001 | Gửi OTP register | `POST /auth/send-otp` với email mới và purpose hợp lệ | `200`; thông báo gửi OTP; không lộ OTP trong response | P0 |
| API-AUTH-002 | OTP email bắt buộc | Gửi request thiếu email | `400`; không tạo OTP | P1 |
| API-AUTH-003 | Chặn email duplicate khi register | Gửi OTP register cho email đã tồn tại | `409` | P1 |
| API-AUTH-004 | Verify OTP hợp lệ | `POST /auth/verify-otp` với OTP hợp lệ | `200`, `verified=true`, trả token OTP theo contract | P0 |
| API-AUTH-005 | Verify OTP sai/hết hạn | Gửi OTP sai hoặc hết hạn | `400`, `verified=false` | P0 |
| API-AUTH-006 | Register | `POST /auth/register` với dữ liệu và OTP token hợp lệ | `200`; user được tạo một lần | P0 |
| API-AUTH-007 | Login hợp lệ | `POST /auth/login` với credential đúng | `200`; trả auth data hoặc `requires2FA=true` | P0 |
| API-AUTH-008 | Login sai | Login password sai | `401`; không phát token/cookie hợp lệ | P0 |
| API-AUTH-009 | Login 2FA | `POST /auth/login-2fa` với challenge/mã hợp lệ | `200`; trả auth response | P0 |
| API-AUTH-010 | Social login lỗi | Gọi Google/GitHub login với token/code sai | `400` hoặc `401` theo controller; không tạo phiên | P1 |
| API-AUTH-011 | Invite info | `GET /auth/invite-info` với token hợp lệ và sai | Hợp lệ `200` có data; sai trả lỗi `4xx`, không lộ invite khác | P1 |
| API-AUTH-012 | Accept invite token | `POST /auth/accept-invite-token` hợp lệ | `200`; membership/user state cập nhật đúng | P1 |
| API-AUTH-013 | Refresh token | Dùng access token hết hạn và refresh cookie hợp lệ gọi `POST /auth/refresh-token` | `200`; trả access token mới | P0 |
| API-AUTH-014 | Refresh thiếu cookie/header | Gọi refresh thiếu refresh cookie hoặc access token | `401` | P0 |
| API-AUTH-015 | Logout | `POST /auth/logout` với phiên hợp lệ | `200`; refresh token bị vô hiệu hóa/xóa | P0 |

## 2. User/Profile/Security

| ID | Mục tiêu | Bước test | Kết quả mong đợi | Ưu tiên |
|---|---|---|---|---|
| API-USER-001 | Current user | `GET /users/me` với token hợp lệ | `200`; data đúng user, không trả password/hash/OTP | P0 |
| API-USER-002 | Current user thiếu token | Gọi `/users/me` không token | `401` | P0 |
| API-USER-003 | Update profile | `PUT /users/profile` với field hợp lệ | `200`; GET lại có dữ liệu mới | P1 |
| API-USER-004 | Change password OTP | Gửi OTP rồi `PUT /users/change-password` với OTP và password đúng | `200`; password mới login được, password cũ không được | P0 |
| API-USER-005 | Change password OTP sai | Gọi change-password với OTP sai/hết hạn | `400`; password không đổi | P0 |
| API-USER-006 | Toggle 2FA | `POST /users/toggle-2fa` enable/disable | `200`; `is2FaEnabled` đúng và persist | P0 |
| API-USER-007 | Upload avatar hợp lệ | `PUT /users/avatar` multipart ảnh <=5MB | `200`; trả `avatarUrl`, file truy cập được theo policy | P1 |
| API-USER-008 | Upload avatar sai | Upload non-image hoặc >5MB | `400`; không thay avatar | P1 |
| API-USER-009 | Email list | `GET /users/emails` | `200`; primary/verified state nhất quán | P1 |
| API-USER-010 | Add duplicate email | `POST /users/emails` cùng email đã có | `400`; không duplicate | P1 |
| API-USER-011 | Set primary chưa verify | `PUT /users/emails/primary` với email chưa verify | `400`; primary không đổi | P1 |
| API-USER-012 | Delete primary email | `DELETE /users/emails/{email}` với primary | `400`; email không bị xóa | P1 |
| API-USER-013 | Sessions | `GET /users/sessions`; revoke một token khác | `200`; session list đúng, token bị revoke không dùng lại được | P1 |
| API-USER-014 | Revoke other sessions | `DELETE /users/sessions/others` | `200`; chỉ session hiện tại còn hợp lệ | P1 |
| API-USER-015 | Login activity isolation | `GET /users/login-activity` | Chỉ trả activity của current user | P1 |
| API-SEC-001 | IP whitelist đọc | Admin gọi `GET /security/ip-whitelist` | `200`; non-admin bị `403` | P0 |
| API-SEC-002 | IP whitelist cập nhật | Admin `PUT /security/ip-whitelist` với danh sách hợp lệ | `200`; config persist; IP hiện tại không bị khóa ngoài rule source | P0 |
| API-SEC-003 | Accessible projects | User gọi `GET /security/accessible-projects` | `200`; chỉ project user được quyền truy cập | P1 |

## 3. Projects và members

| ID | Mục tiêu | Bước test | Kết quả mong đợi | Ưu tiên |
|---|---|---|---|---|
| API-PRJ-001 | Project discovery | `GET /projects/discovery` | `200`; data chỉ gồm project discoverable/accessible | P0 |
| API-PRJ-002 | Project active/archived/deleted isolation | Gọi `/projects`, `/projects/archived`, `/projects/deleted` | Mỗi danh sách đúng trạng thái và quyền current user | P1 |
| API-PRJ-003 | Get project member | Member gọi `GET /projects/{id}` | `200`; outsider `403`; id không tồn tại `404` khi qua được auth | P0 |
| API-PRJ-004 | Create project | `POST /projects` payload hợp lệ | `201`; response có project ID; GET lại được | P0 |
| API-PRJ-005 | Create invalid project | Thiếu field bắt buộc/invalid reference | `400`; không tạo record rác | P1 |
| API-PRJ-006 | Favorite | `PUT /projects/{id}/favorite` | `200`; favorite persist cho đúng user | P1 |
| API-PRJ-007 | Settings access | PM và member thường gọi `GET /projects/{id}/settings` | PM `200`; role không cho phép `403` | P0 |
| API-PRJ-008 | Update project | PM `PUT /projects/{id}` | `200`; field mới persist; member thường `403` | P0 |
| API-PRJ-009 | Archive/restore | PM gọi archive rồi restore | Mỗi request `200`; project chuyển đúng trạng thái | P1 |
| API-PRJ-010 | Delete/restore/permanent delete | Thực hiện soft delete, restore-deleted, permanent delete | State đúng; permanent delete không thể GET lại | P0 |
| API-PRJ-011 | Members list | Active member gọi `GET /projects/{id}/members` | `200`; outsider `403` | P0 |
| API-PRJ-012 | Member CRUD | PM thêm member, đổi role, xóa member | Mỗi write thành công và list phản ánh đúng | P0 |
| API-PRJ-013 | Member CRUD thiếu quyền | Member/Guest gọi member write API | `403`; membership không đổi | P0 |
| API-PRJ-014 | Execution rules | PM GET/PUT execution-rules | `200`; normalized rules persist; role thường `403` | P1 |
| API-PRJ-015 | Integrations validation | PM PUT integrations rỗng/provider sai | `400`; config cũ giữ nguyên | P1 |
| API-PRJ-016 | Management settings | PM GET/PUT reward, capacity, baseline; CRUD milestones/points | `2xx`; dữ liệu đúng project; role thường `403` | P1 |

## 4. Work tasks

| ID | Mục tiêu | Bước test | Kết quả mong đợi | Ưu tiên |
|---|---|---|---|---|
| API-TASK-001 | List project tasks | `GET /projects/{id}/WorkTasks` bằng member | `200`; task đúng project và visibility rule | P0 |
| API-TASK-002 | My tasks/search | Gọi `/tasks/my-tasks` và `/tasks/search` | `200`; không lộ task ngoài quyền | P0 |
| API-TASK-003 | Create task | `POST /projects/{id}/WorkTasks` hợp lệ | `201`; key/project/status đúng; GET lại được | P0 |
| API-TASK-004 | Create invalid task | Thiếu title hoặc status/member reference sai | `400`; không tạo task | P1 |
| API-TASK-005 | Update task với RowVersion | `PUT /projects/{id}/WorkTasks/{taskId}` có RowVersion mới nhất | `200`; data persist | P0 |
| API-TASK-006 | Update thiếu RowVersion | Gọi PUT không RowVersion | `400`; dữ liệu không đổi | P0 |
| API-TASK-007 | Optimistic concurrency | Hai client dùng cùng RowVersion; client 1 save rồi client 2 save | Client 2 nhận `409`; không ghi đè thay đổi mới | P0 |
| API-TASK-008 | Patch field | `PATCH` một field được hỗ trợ | `200`; chỉ field yêu cầu đổi | P1 |
| API-TASK-009 | Status update | `PUT /WorkTasks/{id}/status` status hợp lệ | `200`; status persist | P0 |
| API-TASK-010 | Invalid status | Cập nhật status không thuộc project | `400`; status cũ giữ nguyên | P0 |
| API-TASK-011 | Assignee project isolation | Gán user không thuộc project | `400`; assignee không đổi | P0 |
| API-TASK-012 | Sprint/module isolation | Gán sprint/module từ project khác | `400`; không tạo liên kết chéo | P0 |
| API-TASK-013 | Parent self reference | Set task làm parent của chính nó | `400` | P0 |
| API-TASK-014 | Parent completion guard | Chuyển parent DONE khi subtask chưa đủ điều kiện | `400`; parent chưa hoàn thành | P1 |
| API-TASK-015 | Reorder | Gọi reorder với sort order/status hợp lệ | `200`; thứ tự/status mới persist | P1 |
| API-TASK-016 | Reorder thiếu quyền | Guest/outsider reorder | `403`; sort order không đổi | P0 |
| API-TASK-017 | Subscription | Toggle `POST /WorkTasks/{id}/subscription` | `200`; subscriber state đổi đúng user | P2 |
| API-TASK-018 | Time log hợp lệ | Assignee active log số giờ >0 | `200`; time log persist | P1 |
| API-TASK-019 | Time log không hợp lệ | Non-assignee hoặc giờ <=0 log time | `400`; không tạo log | P1 |
| API-TASK-020 | Subtask | GET/POST `/WorkTasks/{parentId}/subtasks` | Tạo thành công với parent đúng; list trả đúng children | P1 |
| API-TASK-021 | Task statuses CRUD | PM create/update/delete status | `200`; duplicate `409`; status đang dùng không xóa được (`400`) | P1 |
| API-TASK-022 | Dashboard/analytics | Gọi `/dashboard/stats`, `/analytics/planning-summary` | `200`; số liệu chỉ từ phạm vi user được phép | P1 |

## 5. Comments, labels và dependencies

| ID | Mục tiêu | Bước test | Kết quả mong đợi | Ưu tiên |
|---|---|---|---|---|
| API-CMT-001 | List comments | GET comments của task trong project | `200`; đúng task, đúng thứ tự contract | P1 |
| API-CMT-002 | Create comment | POST text/multipart hợp lệ | `2xx`; comment đúng author/task | P1 |
| API-CMT-003 | Edit/delete ownership | User có quyền sửa/xóa; user khác thử cùng request | Chủ thể hợp lệ thành công; user thiếu quyền bị từ chối | P0 |
| API-CMT-004 | Attachment isolation | Xóa attachment bằng comment/task/project ID không khớp | `4xx`; không xóa file ngoài scope | P0 |
| API-CMT-005 | Reaction idempotency | Toggle cùng reaction nhiều lần theo contract | Không tạo duplicate uncontrolled; count đúng | P2 |
| API-LBL-001 | Label CRUD | Member/manager theo rule create/update/delete label | Thành công theo quyền; duplicate/invalid xử lý `4xx` | P1 |
| API-LBL-002 | Assign/unassign label | POST/DELETE task label | Quan hệ đúng task/project; cross-project bị từ chối | P0 |
| API-DEP-001 | Dependency CRUD | GET/POST/DELETE dependency hợp lệ | Quan hệ persist rồi xóa đúng | P1 |
| API-DEP-002 | Dependency invalid | Self-link, duplicate hoặc task project khác | `4xx`; không tạo quan hệ sai | P0 |

## 6. Sprints, modules, pages, views và intakes

| ID | Mục tiêu | Bước test | Kết quả mong đợi | Ưu tiên |
|---|---|---|---|---|
| API-SPR-001 | Sprint CRUD | GET/POST/PUT sprint trong project | GET `200`, create `201`, update `200`; dữ liệu persist | P1 |
| API-SPR-002 | Start/close role | PM start/close; member thường thử | PM `200`; role không cho phép `403` | P0 |
| API-SPR-003 | Carry-over | GET carry-over; move không chọn task; move hợp lệ | List `200`; rỗng `400`; hợp lệ `200` và movedCount đúng | P1 |
| API-SPR-004 | Carry-over target isolation | Chọn target sprint project khác | `400`; task không đổi sprint | P0 |
| API-SPR-005 | Favorite/burndown | PATCH favorite; GET burndown | `200`; favorite và data burndown đúng sprint | P2 |
| API-MOD-001 | Module CRUD | GET/POST/PUT/DELETE module | `2xx`; data đúng project; outsider `403` | P1 |
| API-PAGE-001 | Page CRUD | GET/POST/PUT/archive/delete page | `2xx`; private/star/archive fields đúng contract | P1 |
| API-VIEW-001 | Saved views | CRUD/favorite project view | `2xx`; owner/project isolation đúng | P2 |
| API-INT-001 | Intake lifecycle | GET/POST intake; PUT review | `2xx`; review status persist | P2 |

## 7. Supporting APIs

| ID | Mục tiêu | Bước test | Kết quả mong đợi | Ưu tiên |
|---|---|---|---|---|
| API-NOTIF-001 | Notification list/count | GET notifications và unread-count | `200`; chỉ notification current user | P1 |
| API-NOTIF-002 | Read state | PUT one/read-all | `200`; unread count cập nhật | P1 |
| API-DRAFT-001 | Draft isolation | CRUD drafts bằng hai user | Mỗi user chỉ thấy/sửa draft của mình | P0 |
| API-STICKY-001 | Sticky isolation | CRUD stickies bằng hai user | Mỗi user chỉ thấy/sửa sticky của mình | P1 |
| API-GAME-001 | Gamification | GET me/leaderboard | `200`; không lộ field nhạy cảm | P2 |
| API-AUDIT-001 | Audit query | GET `/auditlogs` với filter/page | `200`; paging/filter đúng quyền | P1 |
| API-AI-001 | AI authentication | Gọi `/ai/chat` hoặc endpoint AI không token | `401` | P0 |
| API-AI-002 | AI rate limit | Gửi hơn 5 request trong 1 phút cùng user/IP | Request vượt limit trả `429` | P1 |
| API-AI-003 | AI project isolation | Gửi project/task không được truy cập vào AI action | Bị từ chối; không tạo work item ngoài quyền | P0 |

## 8. Admin APIs

| ID | Mục tiêu | Bước test | Kết quả mong đợi | Ưu tiên |
|---|---|---|---|---|
| API-ADM-001 | Admin user list guard | Admin và normal user gọi `GET /admin/users` | Admin `200`; normal user `403` | P0 |
| API-ADM-002 | Invite/add user | Admin POST user hợp lệ, duplicate/deleted/suspended cases | Hợp lệ `200`; invalid theo source trả `400` | P1 |
| API-ADM-003 | Suspend/delete user | Admin suspend và remove user test | `200`; user bị ảnh hưởng không dùng API protected | P0 |
| API-ADM-004 | Protected roles | Thử create/update/delete protected role | `400`; protected role không đổi | P0 |
| API-ADM-005 | Role duplicate/in-use | Tạo duplicate; xóa role còn assigned | Duplicate `409`; role in-use `400` | P1 |
| API-ADM-006 | Assign roles | POST user roles với IDs hợp lệ/sai | Hợp lệ `200`; role sai `400` | P0 |
| API-ADM-007 | Department CRUD | CRUD department, duplicate, member add duplicate | Thành công `200`; duplicate `409`; relation đúng | P1 |
| API-ADM-008 | Project role assignment | GET/PUT/DELETE project-department-role | `200`; invalid reference `400/404` | P1 |
| API-ADM-009 | System settings guard | Non-admin gọi admin settings endpoints | `403`; không trả settings nhạy cảm | P0 |
| API-ADM-010 | System metrics | Admin GET `/admin/system/metrics` | `200`; non-admin `403` | P1 |

## 9. Platform security và resilience

| ID | Mục tiêu | Bước test | Kết quả mong đợi | Ưu tiên |
|---|---|---|---|---|
| API-PLAT-001 | Missing JWT | Gọi endpoint `[Authorize]` không Authorization | `401`; không trả data | P0 |
| API-PLAT-002 | Invalid JWT | Gọi với token malformed/expired không refresh | `401` | P0 |
| API-PLAT-003 | Suspended/deleted account | Dùng token còn hạn của user bị suspend/delete gọi system protected API | `403` theo `SystemAuthorizeFilter` | P0 |
| API-PLAT-004 | Missing projectId | Gọi action/filter không có projectId hợp lệ nếu route cho phép tới filter | `400` với thông báo missing projectId | P1 |
| API-PLAT-005 | Project outsider | Outsider gọi project read/write API | `403`; không lộ data | P0 |
| API-PLAT-006 | Guest/Stakeholder write | Guest gọi POST/PUT/DELETE project API có `ProjectAuthorize` | `403`; data không đổi | P0 |
| API-PLAT-007 | System override | System admin gọi project API không có ProjectMember | Được qua project filter theo override; vẫn chịu validation action | P0 |
| API-PLAT-008 | CORS allowed origin | Gọi từ `http://localhost:5173` hoặc `5174` với credential | CORS headers hợp lệ | P1 |
| API-PLAT-009 | CORS disallowed origin | Preflight từ origin không whitelist | Không cấp quyền CORS cho origin đó | P1 |
| API-PLAT-010 | Error data leakage | Gây validation và server error kiểm soát được | Response không lộ password/token/connection string; production không trả stack | P0 |
| API-PLAT-011 | Upload path safety | Upload file tên chứa path traversal/ký tự đặc biệt | File được sanitize/lưu an toàn, không ghi ngoài uploads | P0 |
| API-PLAT-012 | SignalR auth/isolation | Kết nối hubs với token hợp lệ, thiếu token và user project khác | Chỉ user hợp lệ nhận event được phép; không rò event cross-project | P0 |
| API-PLAT-013 | SQL unavailable development | Khởi động Development khi SQL không kết nối | App fallback InMemory theo source, log rõ; không dùng fallback ngoài điều kiện | P2 |
| API-PLAT-014 | Refresh request concurrency | Tạo nhiều API `401` đồng thời trên frontend/client | Chỉ một refresh chính; queued request retry bằng token mới | P1 |
| API-PLAT-015 | Language header | Chọn vi/en và quan sát `Accept-Language` | Header phải phản ánh nguồn `app_language`; sai lệch với `admin_locale` được ghi bug | P1 |
