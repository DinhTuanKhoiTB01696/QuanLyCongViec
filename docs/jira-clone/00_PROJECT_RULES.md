# 00_PROJECT_RULES

Dự án: Jira screenshot clone bằng Vue + .NET.

Nguồn sự thật theo thứ tự ưu tiên:
1. Ảnh screenshot được chỉ định trong task.
2. Mô tả trực tiếp của người dùng.
3. Source code hiện tại.

Không được:
- Tự bịa tính năng.
- Tự thêm module ngoài ảnh.
- Tự suy diễn workflow Jira nếu ảnh không chứng minh.
- Dùng kiến thức Jira thật để lấp chỗ trống.
- Clone các nét khoanh đỏ/chú thích tay trong ảnh `usable_annotated`.

Luôn ghi `[CẦN XÁC NHẬN]` nếu thiếu bằng chứng.

Khi phân tích phải chia:
A. Quan sát trực tiếp từ ảnh/code.
B. Suy ra hợp lý từ ảnh/code.
C. Chưa đủ bằng chứng.
D. Cần xác nhận.

Frontend:
- Bám ảnh, dùng ảnh đúng màn hình.
- Mỗi task chỉ dùng 1-5 ảnh liên quan.
- Không dùng ảnh marketing để suy ra app project.

Backend:
- Không code theo “Jira thật”.
- Chỉ tạo API/DTO/service phục vụ data contract đã rút ra từ frontend/screenshot.
- Nếu chưa có frontend contract thì chưa làm backend.
