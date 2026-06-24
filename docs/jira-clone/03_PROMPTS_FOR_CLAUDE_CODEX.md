# 03_PROMPTS_FOR_CLAUDE_CODEX

## Prompt nền bắt buộc

```txt
Đọc `docs/jira-clone/00_PROJECT_RULES.md` và `docs/jira-clone/01_SCREENSHOT_INVENTORY.md` trước.

Bạn chỉ được dựa trên ảnh screenshot được nêu trong task, mô tả của người dùng và source code hiện tại.
Không được dùng kiến thức Jira thật để tự bổ sung.
Nếu thiếu ảnh/bằng chứng, dừng và ghi [CẦN XÁC NHẬN].

Khi phân tích luôn chia:
A. Quan sát trực tiếp từ ảnh/code.
B. Suy ra hợp lý từ ảnh/code.
C. Chưa đủ bằng chứng.
D. Cần xác nhận.
```

## Prompt audit ảnh chưa rõ tên

```txt
Hãy đọc các ảnh sau và chỉ phân loại, chưa code:
[đính kèm ảnh hoặc đường dẫn ảnh]

Yêu cầu output:
| Ảnh | Màn hình/khu vực có thể thuộc về | Quan sát trực tiếp | Có đủ để code không? | Cần ảnh bổ sung gì? |

Không được suy diễn workflow. Nếu không chắc, ghi [CẦN XÁC NHẬN].
```

## Prompt frontend task nhỏ

```txt
TASK FRONTEND:
[Dán task cụ thể]

Ảnh bắt buộc được dùng:
- [ID ảnh từ 01_SCREENSHOT_INVENTORY, ví dụ BOARD-02-project-board-dark]

Phạm vi file được phép sửa:
- [liệt kê file/folder]

Không được:
- Không sửa ngoài phạm vi.
- Không thêm UI không có trong ảnh.
- Không thêm behavior không có trong ảnh.
- Không thêm package.
- Không nối backend nếu task không yêu cầu.
- Không clone nét khoanh đỏ/chú thích trong ảnh annotated.

Trước khi sửa code, trả lời:
A. Quan sát trực tiếp từ ảnh/code.
B. Kế hoạch sửa.
C. File dự kiến sửa.
D. Rủi ro.
E. [CẦN XÁC NHẬN].

Sau khi sửa:
- Liệt kê file đã sửa.
- Chạy build/lint nếu có.
- Báo lỗi thật nếu fail.
```

## Prompt backend task nhỏ

```txt
TASK BACKEND:
Tạo/chỉnh endpoint cho màn hình [tên màn hình].

Contract frontend bắt buộc:
[dán JSON contract hoặc đường dẫn file contract]

Phạm vi file được phép sửa:
- [controller/service/dto/entity cụ thể]

Yêu cầu:
1. API trả đúng contract.
2. Không thêm workflow Jira thật.
3. Không thêm permission/notification/search/report logic nếu contract không yêu cầu.
4. Không tạo entity mới nếu entity hiện tại đủ dùng.
5. Nếu cần migration, giải thích UI nào cần field đó.
6. Build backend phải pass.

Nếu thiếu contract hoặc ảnh chưa chứng minh dữ liệu, dừng và ghi [CẦN XÁC NHẬN].
```

## Prompt review diff

```txt
Hãy review diff vừa làm. Không sửa code.

Kiểm tra:
1. Có sửa ngoài scope không?
2. Có thêm UI/hành vi không có trong ảnh không?
3. Có dùng ảnh marketing để suy ra app project không?
4. Có thêm backend logic không có contract không?
5. Build/test có được chạy không?
6. Còn [CẦN XÁC NHẬN] nào không?

Output: PASS / FAIL và lý do.
```
