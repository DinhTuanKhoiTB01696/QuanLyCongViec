# SPRINTA — INTEGRATION HUB SCROLL, LAYOUT & DETAIL DRAWER PLAN

> **Mục tiêu:** Sửa triệt để lỗi không cuộn được hết nội dung trong `/integrations`, cân lại bố cục, giữ đầy đủ chức năng đã phục hồi và chuyển panel chi tiết sang dạng drawer rõ ràng, không còn khoảng trống hoặc che nội dung.
>
> **Repository:** https://github.com/DinhTuanKhoiTB01696/QuanLyCongViec.git
>
> **Tài liệu nền:** `docs/SPRINTA_AI_EXECUTION_MASTER_PLAN.md`
>
> **Nguyên tắc:** Không làm lại chức năng “Tạo task từ Inbox” nếu luồng hiện tại đã được sửa đúng. Chỉ kiểm tra tương thích.

---

# 1. Vấn đề hiện tại

- Không cuộn xuống được hết nội dung bên dưới.
- Có nhiều lớp `height`, `max-height`, `overflow` tranh quyền cuộn.
- Hero quá cao, khoảng trống lớn.
- KPI chưa cân bằng với CTA đồng bộ.
- Cột Connected Apps, Unified Inbox và Detail tranh nhau chiều rộng.
- Panel chi tiết hẹp nhưng chứa nhiều thông tin.
- Quick Notes và AI có thể che nội dung.
- Khi chưa chọn item vẫn có nguy cơ dành sẵn vùng trống.
- Header có thể che phần đầu trang.
- Nội dung dài, attachment, action hoặc Project selector có thể bị cắt.

---

# 2. Quy tắc bắt buộc

- Audit route `/integrations` và component đang mount thật.
- Chỉ đọc tối đa 12 file trực tiếp liên quan.
- Không đọc toàn bộ repository.
- Không xóa chức năng cũ.
- Không mock, hard-code hoặc fake API.
- Không đổi backend contract nếu không cần.
- Không sửa luồng tạo task nếu không phát hiện lỗi thật.
- Không thêm thư viện mới nếu CSS/Vue transition đủ.
- Không redesign toàn hệ thống.
- Mỗi phase phải build/test xong rồi dừng.

---

# 3. PHASE 0 — Xác định chủ sở hữu scroll

Truy vết:

```text
Global Layout
→ content-area
→ IntegrationHubView
→ Connected Apps
→ Unified Inbox
→ Detail Panel
```

Tạo bảng:

| Layer | height/min-height | overflow | position | Quyết định |
|---|---|---|---|---|

Mục tiêu:

- Global header giữ nguyên.
- Sidebar giữ nguyên, có scroll riêng nếu quá dài.
- `/integrations` có một vùng cuộn chính.
- Inbox list có thể cuộn riêng khi dữ liệu dài.
- Detail drawer body có thể cuộn riêng.
- Không có nhiều lớp `overflow:hidden` chặn nhau.
- Không dùng `100vh/100dvh` nếu chưa trừ đúng chiều cao header.

---

# 4. PHASE 1 — Sửa scroll

## 4.1. Cấu trúc đề xuất

```text
Authenticated Layout
├── Global Header (sticky/fixed)
├── Sidebar (fixed/sticky, scroll riêng)
└── Main Content
    └── Integration Page (scroll chính)
        ├── Hero
        ├── KPI
        └── Workspace
            ├── Connected Apps
            └── Unified Inbox
```

## 4.2. CSS direction

```css
.integration-page {
  min-height: 100%;
  overflow: visible;
}

.integration-workspace {
  display: grid;
  grid-template-columns: minmax(280px, 0.34fr) minmax(0, 0.66fr);
  align-items: start;
  min-width: 0;
}

.integration-main,
.integration-inbox {
  min-width: 0;
}

.integration-detail-body {
  min-height: 0;
  overflow-y: auto;
}
```

Không giữ:

```css
height: 100vh;
height: 100dvh;
overflow: hidden;
```

trên wrapper nếu điều đó làm mất nội dung.

## 4.3. Acceptance

- Cuộn tới cuối trang.
- Cuộn được Connected Apps dài.
- Cuộn được Inbox dài.
- Cuộn được Detail dài.
- Không có hai thanh scroll ngang.
- Không che modal, dropdown, date picker, AI hoặc Notes.
- 1366×768, 1920×1080, 390px đều pass.

---

# 5. PHASE 2 — Bố cục mới

## 5.1. Trạng thái mặc định

```text
┌──────────────────────────────────────────────────────────────┐
│ Integration Hub                           [Đồng bộ tất cả]   │
│ Kết nối và tập trung thông báo vào một nơi                   │
├──────────────┬──────────────┬──────────────┬─────────────────┤
│ 3/3 kết nối │ 25 chưa đọc  │ 1 sự kiện   │ Sync gần nhất   │
├───────────────────────┬──────────────────────────────────────┤
│ Connected Apps 32%    │ Unified Inbox 68%                   │
│                       │ Search / Filter / Pagination         │
│ Google Calendar       │ Inbox List                           │
│ Gmail                 │                                      │
│ Slack                 │                                      │
└───────────────────────┴──────────────────────────────────────┘
```

## 5.2. Hero

- Rút ngắn chiều cao.
- Tiêu đề, mô tả, CTA rõ.
- Robot chỉ là accent nhỏ hoặc đặt trong AI Insight.
- Không để robot chiếm khoảng trống lớn.
- CTA “Đồng bộ tất cả” nằm cùng hierarchy với hero.

## 5.3. KPI

- 4 card cùng chiều cao.
- Căn đều.
- Số liệu chính nổi bật.
- Subtitle ngắn.
- Không để một card lệch xuống.

---

# 6. PHASE 3 — Detail Drawer

Khi chưa chọn item:

- Không giữ cột detail.
- Inbox chiếm toàn bộ phần phải.

Khi chọn item:

```text
┌──────────── Chi tiết thông báo ─────────────┐
│ [← Quay lại]                         [×]     │
│─────────────────────────────────────────────│
│ Provider / Sender / Time                    │
│ Subject                                     │
│ Body / Snippet                              │
│ Attachment                                  │
│ AI Insight                                  │
│ Project nhận task                           │
│ Status / Assignee                           │
│ [Mở gốc] [Bỏ qua] [Tạo task]                │
└─────────────────────────────────────────────┘
```

Bắt buộc:

- Có `Quay lại`.
- Có `Đóng`.
- Có `Escape`.
- Có loading/error/empty.
- Body cuộn được.
- Không mở blank panel.
- Không che Quick Notes hoặc AI.
- Mobile dùng full-screen drawer.
- Khi drawer mở, chỉ một utility drawer được phép mở.

---

# 7. PHASE 4 — Hoàn thiện Inbox UX

Ưu tiên:

- Search backend.
- Filter provider.
- Chưa đọc/đã đọc/đã tạo task.
- Task candidate.
- Server-side pagination 25–50 item.
- Sync history.
- Empty/loading/error.
- Link tới task đã tạo.
- Open original.
- Attachment preview.
- Bulk selection chỉ khi backend hỗ trợ thật.

Sau khi tạo task:

```text
Đã tạo task

SPRINT-142 · Review báo cáo
Project: SprintA Enterprise Platform
Status: Backlog

[Mở task] [Xem trong Work Items]
```

---

# 8. PHASE 5 — Task Candidate

Không coi mọi item là task.

```text
Email/Event/Message
→ Rule prefilter
→ AI classification
→ Confidence
→ User review
→ Create task
```

Hiển thị:

```text
Có khả năng là Task · 82%

Lý do:
✓ Có yêu cầu hành động
✓ Có deadline
✓ Gửi trực tiếp cho bạn
```

Không tự tạo.

---

# 9. UI/UX References

Dùng có giới hạn:

- Hallmark: audit/redesign component/page.
- Premium Web Design Skill: hierarchy, spacing, visual polish.
- Mobbin: real-world Integration/Inbox/Drawer patterns.
- Taste Skill: density, spacing, existing-project audit.
- React Bits/Vue Bits: micro-interaction.
- GSAP: chỉ khi CSS/Vue transition không đủ.
- 21st: component composition.

Thiết lập:

```text
DESIGN_VARIANCE = 4/10
MOTION_INTENSITY = 4/10
VISUAL_DENSITY = 7/10
```

Không áp dụng cinematic hero, video background hoặc typography nghệ thuật vào trang nghiệp vụ.

---

# 10. Test bắt buộc

- 1366×768.
- 1920×1080.
- 390px mobile.
- Inbox ít/nhiều item.
- Detail ngắn/dài.
- Có/không attachment.
- AI mở.
- Notes mở.
- Dropdown/date picker/modal.
- Scroll tới cuối.
- F5.
- API 400/403/404/500.
- Frontend build.
- Backend build/test.
- `git diff --check`.

---

# 11. Definition of Done

- Cuộn được toàn bộ trang.
- Không mất nội dung dưới.
- Không blank panel.
- Không khoảng trống vô nghĩa.
- Hero/KPI cân bằng.
- Connected Apps + Inbox dùng không gian hợp lý.
- Detail drawer rõ ràng.
- AI/Notes không che nội dung.
- Chức năng cũ không mất.
- Build/test pass.
- Không sửa ngoài scope.

---

# 12. Báo cáo A–E

```text
A. Phase và trạng thái
B. File đã sửa
C. Root cause và chức năng hoàn thành
D. Test/browser/API evidence
E. Phần còn thiếu/rủi ro
```
