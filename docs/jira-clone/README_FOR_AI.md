# Jira AI Pack cho Claude/Codex

Gói này được tạo từ ZIP ảnh bạn gửi.

## Nội dung

- `00_PROJECT_RULES.md`: luật bắt buộc của dự án.
- `01_SCREENSHOT_INVENTORY.md`: bảng phân loại toàn bộ ảnh.
- `02_SCREENSHOT_GAPS.md`: phần nào có đủ ảnh, phần nào thiếu.
- `03_PROMPTS_FOR_CLAUDE_CODEX.md`: prompt dùng trực tiếp cho Claude/Codex.
- `04_TASK_QUEUE.md`: thứ tự task nên giao.
- `05_RENAME_PLAN.csv`: mapping từ file gốc sang tên file đề xuất.
- `renamed_screenshots/`: bản copy ảnh đã đổi tên để AI dễ hiểu.
- `contact_sheets/`: ảnh tổng hợp theo thư mục để review nhanh.

## Cách dùng với Claude/Codex

1. Copy thư mục này vào repo, ví dụ `docs/jira-clone/`.
2. Khi giao task, chỉ gửi 1 nhóm ảnh liên quan, không gửi toàn bộ ZIP.
3. Luôn yêu cầu AI đọc `00_PROJECT_RULES.md` và `01_SCREENSHOT_INVENTORY.md`.
4. Nếu ảnh có status `out_of_scope_for_project_app`, không được dùng cho project app.
5. Nếu ảnh có status `usable_annotated`, không clone đường khoanh đỏ/chú thích tay.
6. Nếu thiếu ảnh, AI phải hỏi thêm ảnh và ghi `[CẦN XÁC NHẬN]`.

## Nguyên tắc quan trọng

Frontend bám ảnh. Backend bám contract frontend. Không bám Jira thật.
