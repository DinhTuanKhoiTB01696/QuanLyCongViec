# SPRINTA — FLOATING STICKIES PLAN

> **Mục tiêu:** Cho phép kéo tối đa 5 ghi chú từ Quick Notes Drawer ra màn hình, di chuyển tự do, thu nhỏ, đổi màu, gỡ khỏi màn hình và lưu vị trí sau reload.
>
> **Nguyên tắc:** Quick Notes Drawer là nơi quản lý. Floating Stickies là lớp hiển thị nhanh trên màn hình. Không được làm mất dữ liệu khi chỉ đóng note nổi.

---

# 1. Trải nghiệm mục tiêu

```text
Authenticated App
├── Main Content
├── AI Trigger
├── Notes Trigger
├── Quick Notes Drawer
└── Floating Sticky Layer
    ├── Sticky 1
    ├── Sticky 2
    ├── Sticky 3
    ├── Sticky 4
    └── Sticky 5
```

Người dùng:

1. Mở Quick Notes.
2. Kéo một note ra khỏi drawer.
3. Thả vào vùng nội dung.
4. Note trở thành Floating Sticky.
5. Kéo note tới vị trí khác.
6. F5 vẫn ở đúng vị trí.
7. Gỡ note khỏi màn hình mà không xóa dữ liệu.
8. Mở lại từ drawer.

---

# 2. Giới hạn

- Tối đa 5 Floating Stickies.
- Không tự thay thế note cũ.
- Khi đủ 5:

```text
Bạn chỉ có thể dán tối đa 5 ghi chú.
Hãy gỡ một ghi chú khỏi màn hình trước.
```

- Mobile không dùng drag tự do như desktop.
- Không cho note che global header/sidebar hoàn toàn.
- Không cho note nằm ngoài viewport.

---

# 3. UI của Floating Sticky

```text
┌──────────────────────────────┐
│ ⠿ Review báo cáo        ─ × │
│──────────────────────────────│
│ Kiểm tra phần phân quyền     │
│ trước khi demo.              │
│                              │
│ SprintA Platform             │
│ Cập nhật 17:21               │
└──────────────────────────────┘
```

Action:

| Action | Tác dụng |
|---|---|
| `⠿` | Kéo di chuyển |
| `─` | Thu nhỏ |
| `×` | Gỡ khỏi màn hình, không xóa |
| `📌` | Ghim |
| `↩` | Đưa lại drawer |
| `🗑` | Xóa thật |
| Màu | Đổi màu note |

Không kéo note khi user đang nhập hoặc bôi đen text.

---

# 4. Drag từ Drawer

Mỗi note trong drawer có drag handle:

```text
┌──────────────────────────────┐
│ ⠿ Ghi chú mới        📌  🗑 │
│──────────────────────────────│
│ Nội dung...                  │
└──────────────────────────────┘
```

Luồng:

```text
pointerdown drag handle
→ drag preview
→ drop vào content area
→ clamp position
→ save IsFloating + Position
```

Không dùng toàn card làm drag handle vì dễ xung đột edit/select text.

---

# 5. Scope hiển thị

Mỗi Floating Sticky có:

```text
Hiển thị tại:
○ Chỉ trang này
○ Project hiện tại
● Mọi trang
```

Giá trị:

```text
route
project
global
```

Ví dụ:

- Note email chỉ ở `/integrations`.
- Note Project chỉ trong Project đó.
- Note demo hiện toàn hệ thống.

---

# 6. Entity/API

Trước khi tạo schema mới, tìm entity Sticky/Note hiện có.

Bổ sung nếu thiếu:

```text
IsFloating
FloatingScope
PositionX
PositionY
Width
Height
ZIndex
IsMinimized
SourceRoute
ProjectId
UpdatedAt
```

Không lưu vị trí bằng localStorage làm nguồn chính.

API có thể dùng update hiện có hoặc:

```text
PATCH /api/stickies/{id}/floating-state
```

Payload:

```json
{
  "isFloating": true,
  "floatingScope": "global",
  "positionX": 640,
  "positionY": 180,
  "width": 300,
  "height": 220,
  "zIndex": 3,
  "isMinimized": false
}
```

Bắt buộc user isolation.

---

# 7. Autosave vị trí

- Không gửi request mỗi pixel.
- Trong lúc drag: cập nhật local state.
- Sau `pointerup`: gửi một request.
- Resize: debounce 300–500ms.
- Edit content: debounce riêng.
- Retry khi lỗi.
- Nếu save lỗi, hiển thị trạng thái.

---

# 8. Drag/Resize Safety

- Clamp trong vùng content.
- Không nằm dưới header.
- Không nằm trên sidebar.
- Snap nhẹ vào mép.
- Không che hoàn toàn AI/Notes trigger.
- Modal/dropdown/toast luôn cao hơn.
- Có `Sắp xếp lại` để reset 5 note.

Z-index:

```text
Content:             1–10
Floating Stickies:   30
Utility Rail:        40
AI/Notes Drawer:     50
Popover/Dropdown:   100
Modal/Dialog:       200
Toast:              300
```

---

# 9. Resize

Desktop:

- Min width: 240px.
- Max width: 420px.
- Min height: 140px.
- Max height: 520px.
- Có resize handle ở góc.
- Nội dung cuộn nếu dài.

Không bắt buộc resize trong MVP đầu tiên. Có thể làm sau drag/move/persist.

---

# 10. Mobile

Không drag tự do trên mobile.

Hiển thị dạng chips:

```text
[📝 Review báo cáo] [📝 Demo] [+2]
```

Bấm chip:

- Mở bottom sheet.
- Edit/note actions.
- Gỡ khỏi floating.
- Mở drawer đầy đủ.

Tối đa 3 chip hiện trực tiếp.

---

# 11. Hallmark Component-scope

Áp dụng cho:

- Floating Sticky.
- Drag handle.
- Note card.
- Minimized chip.
- Error/saving state.

Mỗi component có:

```text
default
hover
focus
active
disabled
loading
error
success
```

Không áp dụng page-level hero/footer.

---

# 12. Phases

## PHASE 0 — Audit

- Tìm Sticky entity/API/store.
- Tìm Quick Notes Drawer.
- Tìm global layout.
- Xác định utility drawer manager.
- Chốt file.

## PHASE 1 — Floating MVP

- Drag từ drawer.
- Tối đa 5.
- Move.
- Close khỏi màn hình.
- Persist vị trí.
- Reload.
- Global scope.

## PHASE 2 — Scope

- route.
- project.
- global.

## PHASE 3 — Minimize/Color/Pin

- Minimize.
- Color.
- Pin.
- Bring to front.
- Reset layout.

## PHASE 4 — Resize + Mobile

- Resize.
- Bottom sheet/chips.
- Responsive.

Mỗi phase hoàn thành phải dừng để review.

---

# 13. Test

- Kéo 1–5 notes.
- Kéo note thứ 6.
- Move.
- F5.
- Đổi route.
- Đổi Project.
- Close khỏi màn hình.
- Xóa thật.
- User khác.
- 1366×768.
- 1920×1080.
- 390px.
- AI drawer mở.
- Notes drawer mở.
- Modal/date picker.
- Build frontend/backend.
- API 400/403/404/500.
- `git diff --check`.

---

# 14. Definition of Done

- Kéo note ra được.
- Tối đa 5.
- Move được.
- F5 giữ vị trí.
- Không mất dữ liệu khi đóng floating.
- Scope hoạt động.
- Không che header/sidebar hoàn toàn.
- AI/Notes không xung đột.
- User isolation pass.
- Build/test pass.
- Không sửa ngoài scope.

---

# 15. Báo cáo A–E

```text
A. Phase và trạng thái
B. File đã sửa
C. Chức năng thật hoàn thành
D. Browser/API/DB evidence
E. Phần còn thiếu/rủi ro
```
