# TC TEST TONG HOP CHO NHANH CONFLICT-RESOLVED PR #44 - 2026-04-24

## Cach bao ket qua

Tra ket qua theo format:

- `TC-M1: PASS`
- `TC-M2: FAIL - mo ta ngan`

Mau copy nhanh de ghi note:

```text
TC-M1:

TC-M2:

TC-M3:

TC-M4:

TC-M5:

TC-M6:

TC-M7:

TC-M8:

TC-M9:

TC-M10:

TC-M11:

TC-M12:

TC-M13:

TC-M14:

TC-M15:

TC-M16:

TC-M17:

TC-M18:

TC-M19:

TC-M20:

TC-M21:

TC-M22:

TC-M23:

TC-M24:
```

## Dieu kien test

- Dang o nhanh: `conflict-resolved/pr44-from-main`
- Nen test it nhat 2 loai account:
  - `System Admin`
  - `Member/Developer`
- Neu test realtime 2 tai khoan:
  - dung 2 browser profile khac nhau
  - hoac 1 cua so thuong + 1 cua so an danh

## Nhom A - Khoi dong va dieu huong co ban

### TC-M1 - Khoi dong FE/BE

1. Chay backend.
2. Chay frontend.
3. Mo ung dung.

Ket qua mong doi:

- FE vao duoc
- BE len duoc
- khong trang trang
- khong loi startup nghiem trong

### TC-M2 - Dang nhap admin

1. Dang nhap bang `System Admin`.
2. Vao trang chinh.

Ket qua mong doi:

- dang nhap thanh cong
- khong bi day ve login
- topbar/sidebar hien du lieu binh thuong

### TC-M3 - Dang nhap member/dev

1. Dang nhap bang account `Member/Developer`.
2. Vao trang chinh.

Ket qua mong doi:

- dang nhap thanh cong
- khong loi route
- hien du lieu dung theo quyen

## Nhom B - Project va Work Items

### TC-M4 - Admin doi project

1. Dang nhap admin.
2. Mo project A.
3. Chuyen sang project B.
4. Chuyen tiep sang project C.

Ket qua mong doi:

- khong bi tu dong bat nguoc ve project cu
- project dang mo dung theo project vua chon
- khong can F5

### TC-M5 - Admin load work items moi project

1. Dang nhap admin.
2. Vao tung project co san.
3. Mo `Work items`.

Ket qua mong doi:

- khong `403` sai o `GET /api/projects/{projectId}/WorkTasks`
- project nao cung load duoc danh sach work items

### TC-M6 - Member load work items dung pham vi

1. Dang nhap member/dev.
2. Vao project ma account do la member/duoc assign.
3. Mo `Work items`.

Ket qua mong doi:

- load duoc work items cua project hop le
- khong bi lay nham work items cua project khac

### TC-M7 - Chuyen project khong bi loạn data

1. Dang nhap admin hoac member.
2. Vao project A, xem work items.
3. Chuyen sang project B.

Ket qua mong doi:

- khong hien work items cua A khi dang o B
- khong mat het data bat thuong

## Nhom C - CRUD task va realtime

### TC-M8 - Tao work item

1. Vao 1 project hop le.
2. Tao work item moi.

Ket qua mong doi:

- tao thanh cong
- task hien trong danh sach
- task detail mo duoc

### TC-M9 - Sua task detail

1. Mo task detail.
2. Sua title/description/priority/story points.

Ket qua mong doi:

- luu thanh cong
- mo lai van con du lieu moi

### TC-M10 - Doi status va keo tha

1. Vao board/list.
2. Doi status task.
3. Neu co board thi keo tha task sang cot khac.

Ket qua mong doi:

- luu thanh cong
- UI cap nhat dung

### TC-M11 - Tao subtask

1. Mo task cha.
2. Tao subtask hoac dung AI breakdown neu co.

Ket qua mong doi:

- subtask tao thanh cong
- hien dung trong task cha

### TC-M12 - Realtime 2 tab cung tai khoan

1. Mo 2 tab cung 1 tai khoan, cung 1 project.
2. O tab 1 doi status hoac sua task.
3. Xem tab 2.

Ket qua mong doi:

- tab 2 tu cap nhat
- khong can F5

### TC-M13 - Realtime 2 profile/tai khoan

1. Mo 2 profile/browser rieng.
2. Vao cung 1 project.
3. O 1 ben sua task/doi status.

Ket qua mong doi:

- ben con lai thay thay doi realtime

## Nhom D - Cac man hinh tung conflict tren PR

### TC-M14 - Your Work

1. Vao `Your work`.
2. Kiem tra cac tab tong quan/assigned/created/subscribed/activity.

Ket qua mong doi:

- giao dien khong vo
- khong mat logic da duyet

### TC-M15 - Rewards

1. Vao `Rewards`.
2. Kiem tra summary, diem, thong ke lien quan.

Ket qua mong doi:

- trang mo duoc
- khong loi UI/du lieu

### TC-M16 - Audit Log

1. Vao `Audit Log`.
2. Thu filter/tim kiem neu co.

Ket qua mong doi:

- trang hoat dong
- khong loi render

### TC-M17 - Global Analytics

1. Vao `Global Analytics`.
2. Kiem tra chart va block tong quan.

Ket qua mong doi:

- chart hien duoc
- khong loi ECharts nghiem trong

### TC-M18 - Manage Spaces

1. Vao `Manage Spaces`.
2. Kiem tra giao dien va thao tac co ban.

Ket qua mong doi:

- trang hoat dong
- khong mat layout/route

### TC-M19 - Pages / Modules / Views

1. Vao project.
2. Lan luot mo `Pages`, `Modules`, `Views`.

Ket qua mong doi:

- mo duoc tung man hinh
- khong bi vo giao dien
- khong bi revert logic goc da duyet

### TC-M20 - Stickies

1. Vao `Stickies`.
2. Kiem tra load va thao tac co ban.

Ket qua mong doi:

- load duoc
- khong loi giao dien

## Nhom E - AI va phan quyen

### TC-M21 - AI Assistant voi admin/role quan ly

1. Dang nhap admin hoac role co quyen o project.
2. Vao `AI Assistant`.
3. Thu repo analysis, create backlog, operational review.

Ket qua mong doi:

- thao tac duoc
- khong loi phan quyen sai

### TC-M22 - AI Assistant voi member/dev

1. Dang nhap member/dev.
2. Vao `AI Assistant`.
3. Thu bam cac nut tao backlog AI.

Ket qua mong doi:

- bi chan neu khong du quyen
- khong tao task moi sai nghiep vu

### TC-M23 - AI suggest assignees

1. Mo task detail bang admin/PM/PO/SM.
2. Thu `AI suggest assignees`.
3. Dang nhap member/dev va thu lai.

Ket qua mong doi:

- role co quyen thi dung duoc
- member/dev bi chan

## Nhom F - Route/file moi

### TC-M24 - ProjectSettingsView moi

1. Vao route/man hinh lien quan `ProjectSettingsView`.
2. Kiem tra route mo duoc va khong vo layout.

Ket qua mong doi:

- route hop le
- khong de route moi pha project settings cu

## Chot merge

Chi nen push nhanh conflict-resolved len remote khi:

- tat ca TC quan trong pass
- khong con loi 403 sai o work items
- khong bi revert logic goc da duyet
- khong con bug realtime nghiem trong
