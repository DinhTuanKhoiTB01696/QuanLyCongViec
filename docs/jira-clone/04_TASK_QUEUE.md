# 04_TASK_QUEUE

## Cách làm chuẩn

1. Chọn 1 màn hình.
2. Chọn đúng ảnh trong `01_SCREENSHOT_INVENTORY.md`.
3. Viết UI spec ngắn.
4. Codex/Claude chỉ sửa 1 task nhỏ.
5. Chạy build.
6. Chụp app hiện tại.
7. So với ảnh target.
8. Chỉ sửa phần lệch.

## Thứ tự đề xuất

### Sprint 1 — Dọn scope và shell

- TASK-001: Audit repo, không sửa code.
- TASK-002: Ẩn các module ngoài ảnh nếu đang hiện trong UI clone.
- TASK-003: Chuẩn hóa Global Shell dark mode dựa trên ảnh `BOARD-02`, `LIST-01`, `SUMMARY-02`.
- TASK-004: Chuẩn hóa Project Header + Project Tab Bar dựa trên `BOARD-02`.

### Sprint 2 — Project tabs đã có ảnh rõ

- TASK-005: Board dark mode từ `BOARD-02`; light mode tham khảo `BOARD-01`.
- TASK-006: List dark mode từ `LIST-01`.
- TASK-007: Calendar dark mode từ `CALENDAR-02`; light mode tham khảo `CALENDAR-01`.
- TASK-008: Timeline dark mode từ `TIMELINE-02`; light mode tham khảo `TIMELINE-01`.
- TASK-009: Summary dark mode từ `SUMMARY-02`; light mode tham khảo `SUMMARY-01`.
- TASK-010: Forms dark/light từ `FORMS-01..04`.
- TASK-011: Docs dark/light từ `DOCS-01..02`.
- TASK-012: Development dark/light từ `DEVELOPMENT-01..02`.
- TASK-013: Code dark empty state từ `CODE-01`.
- TASK-014: Archived work items dark empty từ `ARCHIVED-01`.

### Sprint 3 — Issue detail

- TASK-015: Full issue detail shell từ `ISSUE-01`.
- TASK-016: Metadata panels từ `ISSUE-02..05`.
- TASK-017: Subtasks/linked/attachments từ `ISSUE-06..08`.
- TASK-018: Activity tabs từ `ISSUE-09..12`.

### Sprint 4 — Dashboard

- TASK-019: Dashboard list từ `DASH-01`.
- TASK-020: Dashboard create modal từ `DASH-02`/`DASH-06`.
- TASK-021: Dashboard edit/layout/gadgets từ `DASH-03..05`.

### Sprint 5 — Backend theo contract

Chỉ bắt đầu khi frontend mock đã render đúng ảnh.

- TASK-022: Contract `board.contract.json` từ Board UI.
- TASK-023: `GET /api/projects/{projectId}/board` trả đúng contract.
- TASK-024: Contract `list.contract.json` từ List UI.
- TASK-025: Contract `calendar.contract.json` từ Calendar UI.
- TASK-026: Contract `timeline.contract.json` từ Timeline UI.
- TASK-027: Contract `summary.contract.json` từ Summary UI.
- TASK-028: Contract `issue-detail.contract.json` từ Issue Detail UI.

## Task bị khóa cho đến khi có ảnh bổ sung

- Search popup in-app.
- Notification popup.
- Work item create modal.
- Login/Forgot password.
- For You nội dung chính.
- Backlog dark mode đầy đủ.
- Reports dark mode đầy đủ.
