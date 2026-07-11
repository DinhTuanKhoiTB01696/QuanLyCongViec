# SprintA - Master Plan Các Việc Còn Lại Để Hoàn Thiện Dự Án

> File này dùng để đưa trực tiếp cho Codex / Antigravity / Claude / AI Agent đọc và triển khai một mạch các việc còn lại của SprintA.  
> Mục tiêu: không cần hỏi lại từng phase nhỏ, không làm lan man, không mock/fake, không báo pass nếu chưa build/test thật.

---

## 0. Bối cảnh dự án

Repo chính:

```txt
https://github.com/DinhTuanKhoiTB01696/QuanLyCongViec.git
```

Sản phẩm:

```txt
SprintA - nền tảng quản lý công việc / dự án cho SME Việt Nam
```

Định hướng sản phẩm:

```txt
SprintA không clone Jira.
SprintA hướng tới người dùng Việt Nam và doanh nghiệp nhỏ/SME Việt Nam.
Mục tiêu là dễ dùng, trực quan, giao việc rõ ràng, theo dõi tiến độ tốt, báo cáo dễ hiểu, giảm phụ thuộc Excel/Zalo.
```

Công nghệ:

```txt
Frontend:
- Vue 3
- Vite
- Pinia
- Vue Router
- Element Plus
- Tailwind/CSS custom
- TipTap
- SignalR
- Chart libraries

Backend:
- ASP.NET Core API
- EF Core
- SQL Server
- JWT/Auth
- SignalR
- Google/GitHub login
- Notification system
```

---

## 1. Quy tắc bắt buộc cho AI Agent

### 1.1. Quy tắc làm việc

```txt
1. Không đọc toàn repo nếu không cần.
2. Chỉ đọc file liên quan trực tiếp đến task đang làm.
3. Trước khi sửa mỗi nhóm việc, liệt kê tối đa 5-8 file sẽ đọc/sửa.
4. Không mock data.
5. Không fake API.
6. Không hard-code dữ liệu demo vào frontend để làm đẹp.
7. Không toast success nếu API fail.
8. Không báo pass nếu chưa build/test thật.
9. Không tự ý đổi API contract nếu backend đang hoạt động đúng.
10. Không phá các phase đã hoàn thành.
11. Không refactor toàn app nếu task chỉ cần sửa vài file.
12. Nếu gặp lỗi sandbox ACL Windows `opening NUL for ACL write: Access is denied`, phải ghi rõ chưa build được và đưa lệnh để người dùng chạy local.
```

### 1.2. Format báo cáo bắt buộc sau mỗi nhóm việc

Sau mỗi nhóm việc, báo cáo đúng format:

```txt
A. File đã sửa/tạo
B. UI thay đổi rõ ở đâu
C. API/chức năng đã giữ nguyên hoặc thay đổi gì
D. Có sửa backend không
E. Migration/build/test
F. Rủi ro còn lại
G. Có chốt PASS được chưa
```

### 1.3. Không được làm

```txt
- Không thêm tính năng mới ngoài phạm vi file này.
- Không fake OAuth.
- Không fake "đã kết nối".
- Không tạo seed/demo data nếu task không yêu cầu.
- Không cache API/user data/token trong PWA.
- Không log access token/refresh token.
- Không tự động tạo task thật từ AI nếu người dùng chưa xác nhận.
- Không xóa cứng dữ liệu quan trọng nếu có thể soft delete.
```

---

## 2. Trạng thái hiện tại

Các phase đã làm hoặc gần xong:

```txt
Phase 1  - Ổn định lõi & demo thật                  ✅ Đã làm gần hết
Phase 2A - Dashboard cho sếp + reminder thật        ✅ Đã làm
Phase 2B - Intake / Request Portal                  ✅ Đã làm
Phase 2C - Import/Export + AI File Intake           ✅ Đã làm
Phase 2D - Permission preset SME                    ✅ PASS
Phase 3A - AI Assistant tiếng Việt nâng cao         ✅ Source đã làm
Phase 3B - Integration Hub + Unified Inbox          ✅ PASS
Phase 3C - Custom Field & Workflow                  ✅ Source đã làm
Phase 3D - Bước 1 Responsive Mobile UX              ✅ Source đã làm, cần build local nếu chưa xác nhận
```

Việc còn lại cần làm để hoàn thiện dự án:

```txt
1. Phase 3D - Bước 2: PWA nền tảng
2. Phase 3E - Security Hardening
3. UX/UI Polish toàn hệ thống
4. Demo Data chuyên nghiệp
5. Final QA / Smoke Test toàn bộ
6. Tài liệu demo / kịch bản thuyết trình / hướng dẫn cài đặt
```

---

# PHẦN A - HOÀN TẤT NHỮNG VIỆC ĐANG CHỜ BUILD/MIGRATION

## A1. Verify Phase 3C - Custom Fields

Phase 3C - Bước 2 đã thêm Custom Fields cho WorkTask. Cần đảm bảo migration/database/build pass.

### Lệnh cần chạy

```powershell
cd Backend/src/TaskManagement.API
dotnet ef migrations add AddProjectCustomFields --project ../TaskManagement.Infrastructure --context ApplicationDbContext
dotnet ef database update --project ../TaskManagement.Infrastructure --context ApplicationDbContext
```

Sau đó:

```powershell
cd ../../
dotnet build
```

Frontend:

```powershell
cd ../Frontend
npm run build
```

### Nếu migration đã tồn tại

Nếu migration `AddProjectCustomFields` đã được tạo rồi, không tạo trùng. Chạy:

```powershell
cd Backend/src/TaskManagement.API
dotnet ef database update --project ../TaskManagement.Infrastructure --context ApplicationDbContext
```

### Test Custom Fields

```txt
1. Vào Project Settings → Trường tùy chỉnh.
2. Tạo field Text: Mã khách hàng.
3. Tạo field Select: Mức ảnh hưởng với options:
   - Thấp
   - Trung bình
   - Cao
4. Mở một task detail.
5. Kiểm tra section Thông tin bổ sung xuất hiện.
6. Nhập giá trị.
7. Đóng modal, mở lại task.
8. F5 trang, mở lại task.
9. Giá trị vẫn còn.
10. Viewer chỉ xem, không sửa được.
```

### Acceptance Criteria

```txt
- Backend build pass.
- Frontend build pass.
- Migration update database pass.
- Custom field definition lưu được.
- Custom field value lưu được.
- F5 không mất dữ liệu.
- Viewer không sửa được.
```

---

## A2. Verify Phase 3D - Bước 1 Responsive Mobile UX

Nếu chưa build sau khi sửa responsive, chạy:

```powershell
cd Frontend
npm run build
```

### Test responsive

```txt
Desktop >= 1280px:
- Layout không đổi.
- Sidebar/topbar vẫn hoạt động như cũ.

Tablet 768px:
- Sidebar mở dạng drawer.
- Không đẩy content.
- Overlay click đóng được.

Mobile 360px - 390px:
- Không có horizontal scroll toàn trang.
- Topbar không chồng icon.
- Work Items toolbar không tràn nút.
- Kanban vuốt ngang mượt.
- Mỗi cột Kanban cuộn dọc ổn.
- Project Settings tabs cuộn ngang.
```

### Các flow không được phá

```txt
- Nạp dữ liệu công việc.
- Kanban dynamic status.
- Fallback column Khác / Chưa phân loại.
- Tab Trạng thái.
- Tab Trường tùy chỉnh.
- Tab Quản lý phân quyền.
```

---

# PHẦN B - PHASE 3D BƯỚC 2: PWA NỀN TẢNG

## B1. Mục tiêu

Triển khai PWA nền tảng để SprintA có thể install như app, nhưng phải an toàn.

Không làm offline database phức tạp.

Không cache API nhạy cảm.

Không cache token/user data.

Không làm push notification thật.

---

## B2. Scope

Làm:

```txt
- Cài vite-plugin-pwa.
- Cấu hình manifest.
- App name: SprintA.
- Theme color theo branding hiện tại.
- App icons 192x192 và 512x512.
- Service worker cache static assets an toàn.
- Hiển thị thông báo offline cơ bản nếu mất mạng.
```

Không làm:

```txt
- Push notification.
- Offline sync.
- Background sync.
- Cache API /api/*
- Cache SignalR /kanban-hub
- Cache token/user data
- Cache response chứa dữ liệu task/project/user
```

---

## B3. File dự kiến

Đọc/sửa tối đa các file:

```txt
1. Frontend/package.json
2. Frontend/vite.config.js
3. Frontend/public/ hoặc assets logo hiện có
4. Frontend/src/main.js nếu cần đăng ký/offline hook
5. Layout/App component nếu cần banner offline
```

Nếu cần tạo icon:

```txt
- public/pwa-192x192.png
- public/pwa-512x512.png
```

Không chia sẻ font file.

---

## B4. Cấu hình PWA bắt buộc

Trong `vite.config.js`, cấu hình theo hướng:

```txt
- registerType: autoUpdate
- manifest:
  - name: SprintA
  - short_name: SprintA
  - description: Nền tảng quản lý công việc cho đội nhóm Việt Nam
  - theme_color
  - background_color
  - display: standalone
  - start_url: /
  - icons 192/512
```

Workbox runtime caching:

```txt
- Chỉ cache static assets.
- Không cache /api/*
- Không cache /kanban-hub
- Không cache /auth/*
- Không cache token/user data
```

Nếu dùng runtimeCaching, phải exclude API rõ ràng.

---

## B5. Offline UX

Nếu offline:

```txt
- Không fake dữ liệu.
- Không báo đồng bộ thành công.
- Hiển thị banner: "Bạn đang offline. Một số dữ liệu có thể không cập nhật."
- Các thao tác ghi dữ liệu phải báo lỗi thật nếu không có mạng.
```

Không làm offline task creation trong bước này.

---

## B6. Test PWA

```txt
1. npm run build pass.
2. npm run dev hoặc preview.
3. Chrome DevTools → Application → Manifest.
4. Kiểm tra app installable.
5. Kiểm tra icon hiển thị.
6. Tắt mạng:
   - app shell vẫn tải được nếu đã cache static assets.
   - API fail thì hiện lỗi/offline message.
7. Kiểm tra service worker không cache /api/*
8. Lighthouse PWA cơ bản không báo lỗi manifest nghiêm trọng.
```

---

## B7. Báo cáo sau khi làm

```txt
A. File đã sửa/tạo
B. PWA manifest cấu hình gì
C. Service worker cache gì và không cache gì
D. Offline UX hoạt động thế nào
E. Có cache API/token/user data không
F. Build/test kết quả
G. Rủi ro còn lại
```

---

# PHẦN C - PHASE 3E: SECURITY HARDENING

## C1. Mục tiêu

Siết bảo mật trước khi demo/nộp.

Đây là phase quan trọng vì audit đã phát hiện:

```txt
IntegrationAccount đang lưu AccessToken/RefreshToken plaintext trong database.
```

---

## C2. Scope Security

Làm theo thứ tự ưu tiên:

```txt
1. Mã hóa token integration.
2. Kiểm tra không trả token ra frontend.
3. Kiểm tra không log token.
4. Siết authorization cho Project Settings / Permission / Custom Fields.
5. Siết authorization cho AI Project Assistant.
6. Siết Inbox → Create Task.
7. Kiểm tra CORS/Auth config.
8. Kiểm tra upload file policy.
```

---

## C3. Token Encryption cho IntegrationAccount

### Vấn đề

Entity `IntegrationAccount` có:

```txt
AccessToken
RefreshToken
```

Hiện đang lưu plaintext.

### Yêu cầu

```txt
- Không lưu plaintext token trong DB.
- Mã hóa token trước khi save.
- Giải mã token khi cần gọi provider API.
- Không trả token về frontend.
- Không log token.
```

### Hướng triển khai gợi ý

Tận dụng ASP.NET Core Data Protection nếu phù hợp:

```txt
- Tạo service ISecretProtector hoặc ITokenEncryptionService.
- Dùng IDataProtector để Protect/Unprotect.
- Inject vào IntegrationsController hoặc service xử lý integration.
```

File cần audit/sửa tối đa:

```txt
1. IntegrationAccount.cs
2. IntegrationsController.cs
3. ApplicationDbContext.cs nếu cần migration flag/version
4. Program.cs hoặc DI registration
5. appsettings nếu cần config purpose
```

### Migration/Backfill

Cần xử lý token cũ plaintext.

Có 2 hướng:

#### Hướng an toàn đơn giản cho dev/demo

```txt
- Khi đọc token, thử Unprotect.
- Nếu Unprotect fail, coi là plaintext cũ.
- Sau khi dùng thành công, re-protect và lưu lại.
```

#### Hướng migration/backfill

```txt
- Viết migration/job chuyển plaintext sang encrypted.
- Phức tạp hơn, chỉ làm nếu cần.
```

Ưu tiên hướng đầu để giảm rủi ro.

### Acceptance

```txt
- Token không xuất hiện trong response frontend.
- Token trong DB không còn đọc được plaintext sau khi connect/sync mới.
- Sync Google/Gmail/Slack vẫn hoạt động.
- Token cũ vẫn không làm app crash.
```

---

## C4. Authorization Audit

Kiểm tra các endpoint quan trọng:

```txt
Project Settings:
- ProjectPermissions:{projectId}
- task-statuses
- custom-fields

AI:
- /api/ai/project-assistant

Integration/Inbox:
- /api/integrations
- /api/inbox/{id}/create-task
- /api/inbox/create-tasks

Task:
- create/update/delete/status
```

Yêu cầu:

```txt
- User ngoài project không đọc/sửa dữ liệu project.
- Viewer không sửa task/custom field/settings.
- Member không sửa permission/settings.
- AI không nạp context project nếu user không thuộc project.
- Inbox item chỉ được tạo task vào project mà user có quyền.
```

---

## C5. Upload/File Security

Kiểm tra:

```txt
- AI analyze file upload size.
- Extension allowlist.
- Không cho exe/bat/cmd/dll/zip/rar nguy hiểm.
- Attachment upload chỉ cho type an toàn.
- Không log file content nhạy cảm.
```

---

## C6. CORS/Auth

Kiểm tra:

```txt
- CORS không để wildcard quá rộng trong production.
- JWT secret không hard-code trong source.
- Refresh token/auth flow không log ra console.
- SignalR hub yêu cầu auth nếu cần.
```

---

## C7. Báo cáo sau Phase 3E

```txt
A. File đã sửa
B. Token encryption làm thế nào
C. Endpoint nào đã audit
D. Lỗ hổng nào đã fix
E. Lỗ hổng nào còn lại
F. Build/test kết quả
G. Khuyến nghị production
```

---

# PHẦN D - UX/UI POLISH TOÀN HỆ THỐNG

## D1. Mục tiêu

Làm SprintA nhìn chuyên nghiệp, đồng bộ, dễ demo.

Không thêm feature lớn.

Chỉ polish.

---

## D2. Scope Polish

Ưu tiên các màn chính:

```txt
1. Dashboard / Reports
2. Work Items / Kanban / List
3. Project Detail / Project Settings
4. Goal Dashboard / Goal Detail
5. Team / People
6. Integration Hub / Unified Inbox
7. AI Assistant
8. Login/Register nếu cần demo
```

---

## D3. Những thứ cần chuẩn hóa

### Tiếng Việt

```txt
- Không lẫn tiếng Anh không cần thiết.
- Dùng thuật ngữ thống nhất:
  Work Items → Công việc
  Goal → Mục tiêu
  Project → Dự án
  Team → Nhóm
  Member → Thành viên
  Permission → Phân quyền
  Integration → Tích hợp
  Inbox → Hộp thư công việc
```

### Empty State

Mỗi màn cần empty state rõ:

```txt
- Chưa có công việc
- Chưa có mục tiêu
- Chưa có thành viên
- Chưa có tích hợp
- Chưa có dữ liệu báo cáo
```

### Loading State

```txt
- Không để màn hình trắng.
- Có skeleton/spinner hợp lý.
- Nút đang submit phải loading.
```

### Error State

```txt
- API fail phải hiện lỗi thật.
- Không toast success giả.
- Message tiếng Việt dễ hiểu.
```

### Layout

```txt
- Header thống nhất.
- Card spacing thống nhất.
- Button style thống nhất.
- Modal không tràn mobile.
- Form label rõ.
```

---

## D4. Những file nên audit trước

Chỉ audit/sửa theo nhóm, không đọc toàn repo:

```txt
1. ReportsView.vue
2. SpaceSummary.vue
3. ProjectSettings.vue
4. GoalsDashboard.vue
5. GoalDetail.vue
6. PeopleDirectory.vue
7. IntegrationHubView.vue
8. NexusLayout.vue / shared style
```

---

## D5. Acceptance Polish

```txt
- Frontend build pass.
- Các màn chính không có UI vỡ rõ ràng.
- Không còn text tiếng Anh lẫn lộn ở flow demo chính.
- Empty/loading/error state ổn.
- Mobile không tràn ở màn chính.
```

---

# PHẦN E - DEMO DATA CHUYÊN NGHIỆP

## E1. Mục tiêu

Tạo dữ liệu demo giống một doanh nghiệp thật đang dùng SprintA.

Không hard-code frontend.

Dữ liệu phải vào database qua seed/script/API thật.

---

## E2. Nguyên tắc

```txt
1. Không sửa UI.
2. Không fake data trong component.
3. Không hard-code mảng demo trong frontend.
4. Dữ liệu phải chuyên nghiệp, có logic.
5. Có đủ workspace, team, project, task, goal, member, custom field, inbox/report.
6. Dữ liệu phải hỗ trợ dashboard/report nhìn có số liệu.
```

---

## E3. Bộ demo gợi ý

Workspace:

```txt
Công ty TNHH Sao Việt Digital
```

Teams:

```txt
1. Ban Quản lý dự án
2. Nhóm Thiết kế UX/UI
3. Nhóm Backend
4. Nhóm Frontend
5. Nhóm QA
6. Nhóm Marketing
```

Projects:

```txt
1. Triển khai hệ thống CRM nội bộ
2. Website bán hàng Q3
3. Chiến dịch Marketing mùa tựu trường
4. Nâng cấp app chăm sóc khách hàng
```

Members:

```txt
- Nguyễn Minh Anh - Project Manager
- Trần Hoàng Phúc - Backend Lead
- Lê Thảo Vy - UX/UI Designer
- Phạm Quốc Huy - Frontend Developer
- Đặng Gia Bảo - QA Tester
- Võ Ngọc Mai - Marketing Executive
- Bùi Thanh Sơn - Business Analyst
```

Tasks cần đủ trạng thái:

```txt
- Backlog
- To Do
- In Progress
- In Review
- Done
- QA Review nếu workflow custom đã có
```

Custom Fields demo:

```txt
- Mã khách hàng
- Mức ảnh hưởng: Thấp / Trung bình / Cao
- Ngày nghiệm thu
- Cần QA xác nhận
```

Goals:

```txt
- Hoàn thành MVP CRM trước 30/09
- Giảm 30% thời gian xử lý yêu cầu khách hàng
- Tăng tỷ lệ hoàn thành công việc đúng hạn lên 85%
```

Reports phải có dữ liệu:

```txt
- Task quá hạn
- Task sắp tới hạn
- Thành viên quá tải
- Project health xanh/vàng/đỏ
```

---

## E4. Cách làm

Tùy dự án hiện có seed system hay không:

### Nếu có SeedService

```txt
- Thêm DemoDataSeeder.
- Chỉ chạy ở Development.
- Không chạy ở Production.
- Có flag tránh seed trùng.
```

### Nếu không có SeedService

```txt
- Tạo script SQL hoặc endpoint admin-only dev.
- Ưu tiên script SQL rõ ràng.
- Không tạo endpoint public nguy hiểm.
```

---

## E5. Acceptance Demo Data

```txt
- Login vào app thấy dashboard có số liệu.
- Vào project thấy task đủ trạng thái.
- Kanban có dữ liệu đẹp.
- Reports có overdues/upcoming/member workload.
- Goals có progress.
- People/Teams có avatar/name/role.
- Custom fields hiển thị trong task.
- Không có dữ liệu rác kiểu test123/lorem quá nhiều.
```

---

# PHẦN F - FINAL QA / SMOKE TEST TOÀN BỘ

## F1. Mục tiêu

Kiểm tra toàn hệ thống trước khi demo/nộp.

---

## F2. Build bắt buộc

Backend:

```powershell
cd Backend
dotnet build
```

Frontend:

```powershell
cd Frontend
npm run build
```

Migration:

```powershell
cd Backend/src/TaskManagement.API
dotnet ef database update --project ../TaskManagement.Infrastructure --context ApplicationDbContext
```

---

## F3. Smoke Test checklist

### Auth

```txt
[ ] Register
[ ] Login
[ ] Logout
[ ] Refresh/F5 không mất context
[ ] Token hết hạn xử lý ổn
```

### Workspace / Project

```txt
[ ] Tạo workspace/site nếu flow có
[ ] Tạo project
[ ] Mở project detail
[ ] Project settings
[ ] Không có GUID rỗng/null/undefined/1 trong Network
```

### Work Items

```txt
[ ] Tạo task
[ ] Sửa task
[ ] Xóa task
[ ] Đổi status
[ ] Kéo thả Kanban
[ ] Dynamic workflow status
[ ] Fallback column
[ ] Nạp dữ liệu Excel/CSV
[ ] Custom field value lưu được
```

### Goals

```txt
[ ] Tạo goal
[ ] Goal detail
[ ] Update status/progress
[ ] Comment/reaction/attachment nếu có
[ ] Avatar đồng bộ
```

### Teams / People

```txt
[ ] Tạo team
[ ] Thêm member
[ ] Đổi role
[ ] People directory
[ ] Avatar đúng
```

### Permission

```txt
[ ] Owner/Admin vào được Quản lý phân quyền
[ ] Viewer không tạo task
[ ] Viewer không sửa custom field
[ ] Member không sửa permission
[ ] Backend trả 403 khi gọi API trái quyền
```

### AI

```txt
[ ] AI Project Assistant hỏi dự án
[ ] User ngoài project bị 403
[ ] AI suggestedTasks không tự tạo task
[ ] Bấm xác nhận mới tạo task
[ ] Viewer không tạo task từ AI
```

### Integration / Inbox

```txt
[ ] Integration Hub hiển thị provider thật
[ ] GitHub/Zalo sắp ra mắt không gọi API giả
[ ] Unified Inbox filter source/status
[ ] Mark read
[ ] Create task từ inbox item
[ ] Sync fail không toast success
[ ] Không trả token frontend
```

### Mobile / Responsive

```txt
[ ] Desktop không vỡ
[ ] Tablet sidebar drawer
[ ] Mobile topbar không tràn
[ ] Work Items toolbar mobile ổn
[ ] Kanban mobile cuộn ngang
[ ] Settings tab mobile cuộn ngang
```

### PWA nếu đã làm

```txt
[ ] Manifest hợp lệ
[ ] App installable
[ ] Icon hiển thị
[ ] Service worker không cache API/token
[ ] Offline banner đúng
```

---

## F4. Final QA Report format

```txt
A. Build kết quả
B. Migration kết quả
C. Smoke test pass/fail
D. Lỗi còn lại
E. Lỗi nào chấp nhận được khi demo
F. Lỗi nào phải fix trước demo
G. Kết luận sẵn sàng demo chưa
```

---

# PHẦN G - TÀI LIỆU DEMO / THUYẾT TRÌNH

## G1. Cần chuẩn bị

```txt
1. README chạy project
2. Tài khoản demo
3. Kịch bản demo 5-7 phút
4. Danh sách tính năng nổi bật
5. Slide hoặc script thuyết trình
6. Video demo nếu cần
```

---

## G2. Kịch bản demo gợi ý

### Mở đầu

```txt
SprintA là nền tảng quản lý công việc dành cho đội nhóm và SME Việt Nam, giúp thay thế cách quản lý rời rạc qua Excel, Zalo và nhiều file riêng lẻ.
```

### Flow demo

```txt
1. Dashboard cho sếp:
   - Xem tổng quan dự án.
   - Xem task trễ hạn.
   - Xem ai đang quá tải.

2. Work Items:
   - Tạo task.
   - Kéo thả Kanban.
   - Custom workflow status.
   - Custom fields.

3. Team/People:
   - Quản lý thành viên.
   - Phân quyền.

4. AI Assistant:
   - Hỏi “Dự án này đang trễ gì?”
   - AI trả lời theo context thật.
   - AI đề xuất task nhưng không tự tạo.

5. Integration Hub:
   - Unified Inbox.
   - Chuyển item thành task.

6. Mobile:
   - Show responsive mobile nhanh.

7. PWA nếu đã làm:
   - Install app.
```

### Kết luận

```txt
SprintA giúp doanh nghiệp nhỏ có một hệ thống quản lý công việc dễ dùng hơn Jira, trực quan hơn Excel, và có AI hỗ trợ tiếng Việt.
```

---

## G3. README cần có

```txt
- Công nghệ sử dụng.
- Cách chạy backend.
- Cách chạy frontend.
- Cách chạy migration.
- Cách seed demo data.
- Tài khoản demo.
- Các module chính.
- Lưu ý cấu hình OAuth/Gemini.
```

---

# PHẦN H - THỨ TỰ TRIỂN KHAI MỘT LẦN

AI Agent hãy làm theo thứ tự này, không hỏi lại từng bước nếu không có blocker nghiêm trọng:

```txt
1. Verify migration/build các phần đang chờ:
   - Custom Fields
   - Responsive mobile

2. Phase 3D - Bước 2:
   - PWA nền tảng

3. Phase 3E:
   - Security hardening
   - Mã hóa integration tokens
   - Audit authorization

4. UX/UI Polish:
   - Tiếng Việt
   - Empty/loading/error state
   - Mobile polish

5. Demo Data:
   - Seed/script dữ liệu chuyên nghiệp

6. Final QA:
   - Build
   - Migration
   - Smoke test toàn hệ thống

7. Tài liệu:
   - README
   - Kịch bản demo
   - Checklist nghiệm thu
```

Nếu một bước bị blocker do thiếu secret/config/migration conflict:

```txt
- Ghi rõ blocker.
- Không dừng toàn bộ.
- Chuyển sang bước độc lập tiếp theo nếu có thể.
- Cuối cùng tổng hợp blocker cần người dùng xử lý.
```

---

# PHẦN I - OUTPUT CUỐI CÙNG CẦN GIAO LẠI

Sau khi làm xong toàn bộ, Agent phải trả báo cáo tổng hợp:

```txt
A. Tổng số file đã sửa/tạo
B. Các nhóm việc đã hoàn thành
C. Các lệnh build/migration đã chạy
D. Test nào đã pass
E. Test nào chưa chạy được và lý do
F. Lỗi còn lại
G. Security debt còn lại
H. Hướng dẫn người dùng chạy/demo
I. Kết luận dự án đã sẵn sàng demo/nộp chưa
```

Không được báo “hoàn thành 100%” nếu chưa build/test thật.
