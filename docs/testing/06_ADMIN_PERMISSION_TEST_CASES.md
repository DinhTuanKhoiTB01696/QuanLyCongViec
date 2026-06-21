# Admin And Permission Test Cases

## 1. Mô hình quyền hiện tại

### System roles được frontend xem là admin

- `SuperAdmin`
- `Admin`
- `System Admin`
- `Organization Admin`
- `AccessAdmin` / `Access Admin`

### Project roles quản lý thường gặp

- `PROJECT_MANAGER` / `PM`
- `PROJECT_LEAD`
- `PO`
- `SM` / `SCRUM_MASTER`
- `Admin`

### Nguyên tắc cần kiểm

1. Router guard chỉ ngăn điều hướng thuận tiện; API phải tự bảo vệ.
2. System admin có project override theo `ProjectAuthorizeFilter`.
3. User phải là active project member nếu không có system override.
4. Guest/Stakeholder phải là read-only cho project data.
5. Tài khoản suspended/deleted không được dùng quyền cũ.
6. Role string được normalize nhưng không được khiến role ngoài danh sách thành role hợp lệ.

## 2. Ma trận kỳ vọng tổng quát

| Hành động | Normal user | Project member | PM/PO/SM/Lead | Guest/Stakeholder | System admin |
|---|---:|---:|---:|---:|---:|
| Mở route private | Có | Có | Có | Có | Có |
| Đọc project đang active member | Không nếu outsider | Có | Có | Có | Có qua override |
| Sửa work item | Không | Theo rule/task visibility | Có | Không | Có qua override |
| Mở project settings | Không | Không | Có | Không | Có |
| Quản lý member/status/sprint | Không | Không | Có | Không | Có |
| Mở admin user directory | Không | Không | Không | Không | Có role phù hợp |
| Mở system metrics/settings | Không | Không | Không | Không | Tùy `SystemAuthorize` endpoint |

## 3. Frontend route guard

| ID | Mục tiêu | Bước test | Kết quả mong đợi | Ưu tiên |
|---|---|---|---|---|
| PERM-ROUTE-001 | Private route thiếu token | Logout; mở `/dashboard`, `/spaces`, `/profile`, `/admin` | Chuyển `/login` với redirect | P0 |
| PERM-ROUTE-002 | Admin route normal user | Login normal user; mở `/admin/system/general-configuration` | Chuyển `/dashboard?denied=...` | P0 |
| PERM-ROUTE-003 | Admin directory role | Test từng system role với `/admin/users` và `/admin/roles` | Role trong `ADMIN_USER_DIRECTORY_ROLES` được vào; role khác bị chặn | P0 |
| PERM-ROUTE-004 | Admin system route | Test từng system role với `/admin/system/info` | Frontend cho role trong `SYSTEM_ADMIN_ROLES`; API vẫn phải được kiểm riêng | P1 |
| PERM-ROUTE-005 | Project settings manager | PM/PO/SM/Lead mở `/space/{id}/settings` | Router gọi settings API và cho render khi `200` | P0 |
| PERM-ROUTE-006 | Project settings member | Member/Guest mở settings URL trực tiếp | Settings API `403`, router đưa về dashboard | P0 |
| PERM-ROUTE-007 | Project ID không hợp lệ | Mở `/space/not-a-guid/settings` | Không render settings; điều hướng/feedback an toàn | P1 |
| PERM-ROUTE-008 | Không tin dữ liệu local user | Sửa `sessionStorage.user.systemRoles` thành Admin nhưng token thuộc normal user; mở admin | Frontend có thể cho route, nhưng API phải `403`; không lộ dữ liệu | P0 |

## 4. System authorization

| ID | Mục tiêu | Bước test | Kết quả mong đợi | Ưu tiên |
|---|---|---|---|---|
| PERM-SYS-001 | Thiếu JWT | Gọi endpoint `[SystemAuthorize]` không token | `401`, message không lộ chi tiết nhạy cảm | P0 |
| PERM-SYS-002 | User active có role đúng | Gọi `/api/admin/users` bằng từng role được controller cho phép | `200` | P0 |
| PERM-SYS-003 | User không có role | Normal user gọi `/api/admin/users` | `403` | P0 |
| PERM-SYS-004 | User suspended | Suspend admin nhưng giữ token; gọi system endpoint | `403` với account suspended/deleted | P0 |
| PERM-SYS-005 | Role claim giả không đủ | Dùng token/database setup không nhất quán | Filter dựa user/role hợp lệ trong database theo source; không cấp quyền chỉ vì UI state | P0 |
| PERM-SYS-006 | Default SystemAuthorize roles | Organization Admin/AccessAdmin gọi `/api/admin/system/metrics` | Kết quả phải khớp default `SuperAdmin, Admin`; khác biệt với frontend phải ghi bug/contract mismatch | P1 |
| PERM-SYS-007 | Settings endpoint role list | Test admin settings endpoints bằng Organization Admin và AccessAdmin | Endpoint khai báo mở rộng trả `200`; normal user `403` | P1 |
| PERM-SYS-008 | Admin response isolation | Admin list user/role/department | Không trả password hash, OTP secret, refresh token | P0 |

## 5. Project authorization

| ID | Mục tiêu | Bước test | Kết quả mong đợi | Ưu tiên |
|---|---|---|---|---|
| PERM-PRJ-001 | Active member read | Member gọi project/task/member GET | `200` | P0 |
| PERM-PRJ-002 | Outsider read | Outsider gọi cùng GET bằng project ID biết trước | `403`, không trả data/count | P0 |
| PERM-PRJ-003 | Inactive membership | Set ProjectMember.Status=false rồi gọi API | `403` | P0 |
| PERM-PRJ-004 | System override claims | System admin không có ProjectMember gọi project API | Được qua filter và action xử lý bình thường | P0 |
| PERM-PRJ-005 | System override database | Token thiếu role claim nhưng database có system override role | Được qua filter theo database | P1 |
| PERM-PRJ-006 | Role normalization | Test `PROJECT_MANAGER`, `project-manager`, `PM`, `SCRUM_MASTER` ở endpoint tương ứng | Normalize đúng; chỉ role được khai báo được phép | P1 |
| PERM-PRJ-007 | Member settings denial | Member thường gọi `/projects/{id}/settings` | `403` | P0 |
| PERM-PRJ-008 | Manager settings access | PM/PO/SM/Lead/Admin gọi settings/execution-rules | `200` theo danh sách endpoint | P0 |
| PERM-PRJ-009 | Member management | PM thêm/xóa/đổi role; member thường thử | PM thành công; member `403` | P0 |
| PERM-PRJ-010 | Sprint management | PM start/close/update; member thường thử | PM thành công; member `403` | P0 |
| PERM-PRJ-011 | Status management | PM CRUD task statuses; member thường thử | PM thành công; member `403` | P0 |
| PERM-PRJ-012 | Project archive/delete | Manager và member thử | Manager thành công; member `403` | P0 |
| PERM-PRJ-013 | Guest POST/PUT/DELETE | Guest thử create/update/delete project data | `403`; database không đổi | P0 |
| PERM-PRJ-014 | Guest PATCH | Guest thử `PATCH /projects/{id}/WorkTasks/{taskId}` | Không được sửa dữ liệu; nếu PATCH thành công, ghi bug Critical vì filter read-only không bao phủ PATCH | P0 |
| PERM-PRJ-015 | Cross-project route/body | Member Project A dùng route A nhưng body chứa member/sprint/module/task Project B | `400/403`; không tạo liên kết chéo | P0 |
| PERM-PRJ-016 | Project role removed giữa phiên | Xóa membership sau khi user đã login; gọi lại API | Request tiếp theo `403` | P0 |

## 6. Work item visibility và ownership

| ID | Mục tiêu | Bước test | Kết quả mong đợi | Ưu tiên |
|---|---|---|---|---|
| PERM-TASK-001 | Task list visibility rule | Bật role-based task visibility; gọi list bằng manager và member | Manager thấy theo override config; member chỉ thấy task được phép | P0 |
| PERM-TASK-002 | Direct task mutation | Member biết task ID nhưng task bị ẩn; gọi PATCH/PUT | `403`; không thể bypass list bằng ID | P0 |
| PERM-TASK-003 | Assignee update | User không có quyền sửa task thử đổi assignee | `403`; assignment không đổi | P0 |
| PERM-TASK-004 | Comment ownership | User A sửa/xóa comment User B | Bị từ chối trừ khi contract cấp quyền quản trị rõ ràng | P0 |
| PERM-TASK-005 | Attachment ownership | User không có quyền xóa attachment comment | Bị từ chối; file còn tồn tại | P0 |
| PERM-TASK-006 | Time log | Non-assignee log time | `400` theo source; không tạo log | P1 |
| PERM-TASK-007 | Audit log scope | Member query audit của project/task khác | Không trả log ngoài quyền | P0 |
| PERM-TASK-008 | Notification isolation | User A gọi read/delete-like action với notification ID User B | Không thay đổi notification User B | P0 |

## 7. Admin operations

| ID | Mục tiêu | Bước test | Kết quả mong đợi | Ưu tiên |
|---|---|---|---|---|
| PERM-ADM-001 | User directory access | Test normal, PM và system admin tại UI/API | Chỉ system admin role phù hợp truy cập | P0 |
| PERM-ADM-002 | Suspend user effect | Admin suspend user đang login | User bị suspend không tiếp tục dùng protected API | P0 |
| PERM-ADM-003 | Self-suspend/delete guard | Admin thử suspend/delete chính mình nếu UI/API cho phép | Hành vi phải an toàn theo contract; không làm mất toàn bộ admin access ngoài xác nhận | P0 |
| PERM-ADM-004 | Protected roles | Admin thử edit/delete protected role | `400`; role giữ nguyên | P0 |
| PERM-ADM-005 | Assign non-existing role | Gán role ID không tồn tại | `400`; user role cũ giữ nguyên | P1 |
| PERM-ADM-006 | Department duplicate member | Thêm cùng user vào department hai lần | Lần sau `409`; không duplicate relation | P1 |
| PERM-ADM-007 | Project role assignment references | Gửi project/department/role không tồn tại | `400/404`; không tạo assignment | P1 |
| PERM-ADM-008 | Organization settings | Normal user gọi TenantProfile/ContactDiscovery PUT | `403`; settings không đổi | P0 |
| PERM-ADM-009 | Security settings | Normal user gọi IP whitelist PUT | `403`; config không đổi | P0 |
| PERM-ADM-010 | Auditability | Admin thực hiện suspend/role/settings action | Audit log ghi actor, action, target và timestamp nếu source contract yêu cầu | P1 |

## 8. RequirePermission và defense in depth

| ID | Mục tiêu | Bước test | Kết quả mong đợi | Ưu tiên |
|---|---|---|---|---|
| PERM-RBAC-001 | Permission thiếu role | User không có system/project role gọi endpoint `[RequirePermission]` | `403`, message nêu permission thiếu | P0 |
| PERM-RBAC-002 | Role có permission | Gán role có permission code; gọi endpoint | Cho phép action | P0 |
| PERM-RBAC-003 | Permission bị gỡ giữa phiên | Gỡ RolePermission rồi gọi lại bằng token cũ | Bị `403` ngay request sau | P0 |
| PERM-RBAC-004 | System override | SuperAdmin/Admin gọi endpoint permission | Bypass permission check theo source | P1 |
| PERM-RBAC-005 | AccessAdmin override consistency | AccessAdmin gọi endpoint dùng `RequirePermission` | Không tự bypass nếu không thuộc override list; kết quả phải theo permission DB | P1 |
| PERM-RBAC-006 | Case normalization permission | Dùng permission code/role name khác case | So sánh permission code normalized; không duplicate do case | P2 |

## 9. Security regression gate

Release bị chặn nếu:

- Outsider đọc được project/task/member.
- Guest/Stakeholder sửa được project data.
- Normal user gọi được admin API.
- User suspended tiếp tục dùng protected API.
- Cross-project ID tạo được liên kết hoặc thay đổi dữ liệu.
- UI chặn nhưng API trực tiếp vẫn cho phép action.

