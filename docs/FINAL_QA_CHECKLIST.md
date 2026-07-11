# SprintA Final QA Checklist

Ngay kiem tra: 2026-07-11. Moi muc chi PASS khi da chay that.

## Build va database

| Hang muc | Ket qua | Bang chung |
|---|---|---|
| Backend build | PASS | Exit 0, 0 warning, 0 error |
| Backend tests | PASS | Exit 0, 40 passed, 0 failed, 0 skipped |
| EF migration | PASS | Exit 0, database already up to date |
| `npm install` sach | FAIL (Major) | Exit 1, peer conflict Vite 8 / vite-plugin-pwa 0.21.2 |
| Frontend production build | PASS | Exit 0, PWA `dist/sw.js` duoc tao |
| Demo login | PASS | Login va chuyen den `/site-selection` |

## Du lieu demo

- [x] SQL Server ket noi duoc.
- [x] Main project co 35 task, 5 goals, 4 custom fields, 4 values.
- [x] `IntegrationAccounts = 0`.
- [x] Khong reset/seed lai database trong dot QA nay.
- [!] Database hien co 18 users, 7 projects va 6 task statuses; khac baseline handoff 15/6/5. Can xac nhan data drift truoc ban giao moi truong chung.

## Smoke test

- [x] Login demo; profile `khoi.nguyen` hien thi.
- [x] Dashboard va project link tai duoc, khong co console error nghiem trong.
- [x] Work Items Kanban tai duoc, task demo va dynamic columns hien thi; loi `fetchCustomFieldValues` khong tai hien.
- [x] Reports, Project Settings, Goals va Integration Hub tai duoc.
- [x] AI widget xuat hien trong layout chung; prompt tom tat tra ve dung 35 task va phan bo status, khong co console error.
- [ ] Logout/login lai va F5 session: chua chay tron flow.
- [ ] Tao/sua/xoa/drag task, import/export va Task Detail: khong mutate demo data trong final QA.
- [ ] Goal update/comment/reaction/attachment: chua mutate.
- [ ] Member/Viewer UI permission matrix: chua co browser account phu hop trong dot nay.
- [ ] Inbox mark-read/create-task: chua mutate.
- [x] Mobile 390: Copilot panel nam trong viewport (374.4 px), body khong tran ngang.
- [ ] Tablet 768, desktop 1440 va install PWA: chua xac minh day du trong dot nay.

## Loi ghi nhan

### Major: clean frontend install khong tai lap

- Man hinh: toolchain Frontend.
- Tai hien: `cd Frontend; npm install`.
- Thuc te: exit 1, `vite-plugin-pwa@0.21.2` khai bao peer Vite 3-6 trong khi project dung Vite 8.
- Mong doi: install sach exit 0 tu lockfile.
- File nghi ngo: `Frontend/package.json`, `Frontend/package-lock.json`.
- Chan demo: Khong chan may hien tai vi `node_modules` san co build duoc; chan setup may moi/CI sach.

### Minor: demo data count drift

- Man hinh: database demo.
- Tai hien: dem records workspace/project bang truy van read-only.
- Thuc te: 18 users, 7 projects, 6 statuses; baseline handoff la 15, 6, 5.
- Mong doi: baseline va database chung khop hoac duoc ghi phien ban.
- File nghi ngo: `scripts/seed-demo-data.sql` va du lieu da co tu cac dot test khac.
- Chan demo: Khong; project chinh co du task/context.

### Major: Copilot gan sai `pageType` tren Work Items

- Man hinh: Work Items / Global AI Copilot.
- Tai hien: mo `/space/{projectId}/work-items`, mo Copilot va xem chip ngu canh.
- Thuc te: chip va payload logic dung `SpaceSummary`, du route va project ID van dung; cau tra loi tom tat van dung 35 task.
- Mong doi: `pageContext.pageType = work-items` de backend co tin hieu trang chinh xac.
- File nghi ngo: `Frontend/src/components/layout/NexusLayout.vue` (computed `pageType`).
- Chan demo: Khong chan prompt tom tat, nhung co the lam suggestions/context theo trang kem chinh xac.

### Minor: mo ta Copilot khong khop contract read-only

- Man hinh: Copilot panel.
- Tai hien: mo widget.
- Thuc te: copy noi co the `tao task that, chuyen trang thai`; context-chat hien doc `actions` thanh rong va prompt tom tat khong mutation.
- Mong doi: copy phan anh dung pham vi Global AI Copilot read-only, hoac tinh nang mutation phai co contract/confirmation duoc phe duyet rieng.
- File nghi ngo: `Frontend/src/components/layout/NexusLayout.vue`.
- Chan demo: Khong, neu demo chi dung cac prompt read-only da quy dinh.

## Ket luan checklist

Khong co Blocker/Critical duoc phat hien. Build va flow demo doc chinh dat; clean install va cac manual mutation/responsive con la rui ro da ghi nhan.
