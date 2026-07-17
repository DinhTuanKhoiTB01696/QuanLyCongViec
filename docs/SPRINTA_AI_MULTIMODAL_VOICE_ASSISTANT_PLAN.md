# SPRINTA — AI MULTIMODAL FILE, IMAGE & VOICE ASSISTANT PLAN

> **Mục tiêu:** Nâng cấp SprintA AI để người dùng có thể dán ảnh/chụp màn hình, tải file, trò chuyện dựa trên nội dung đó và nhập lệnh bằng giọng nói Việt/Anh có transcript chỉnh sửa trước khi gửi.
>
> **Không thuộc phạm vi file này:** Phục hồi `/integrations` và scalable sync. Phần đó nằm trong `SPRINTA_INTEGRATION_HUB_RECOVERY_AND_SCALABLE_SYNC_PLAN.md`.
>
> **Nguyên tắc:** Không tuyên bố hỗ trợ “mọi định dạng”. Chỉ hỗ trợ định dạng đã có parser, validation và safety rõ ràng.

---

# 1. Nguyên tắc bắt buộc

- Audit AI chat, conversation history và attachment entity/API hiện tại.
- Không tạo conversation/history thứ hai nếu source đã có.
- Không đọc toàn bộ repo.
- Không làm lại Action Preview/Confirm/Retry nếu đã hoạt động.
- Không lưu file trong database dạng base64.
- Không gửi file lớn nguyên vẹn vào prompt.
- Không tự gửi transcript hoặc tạo task khi user chưa xác nhận.
- Không cho file/email thay đổi system prompt hoặc permission.
- Mỗi phase build/test xong rồi dừng.

---

# 2. PHASE 0 — Audit

Kiểm tra:

- AI panel đang mount ở đâu.
- Conversation history hiện có.
- Message schema có attachment chưa.
- File upload/storage hiện có.
- RAG/vector/index hiện có.
- Speech-to-text provider hiện có.
- Permission và audit hiện có.
- Utility Rail/Notes có xung đột không.

Không code trước khi chốt file và dependency.

---

# 3. PHASE 1 — Multimodal Composer UI

## 3.1. Thanh chat

```text
[ + ] [ Nhập nội dung...                         ] [🎤] [Gửi]
```

Nút `+`:

- Tải file từ máy.
- Chọn ảnh.
- Dán ảnh bằng Ctrl+V.
- Kéo thả file/ảnh.
- Chọn tài liệu SprintA.
- Chọn Task/Project/Goal làm context.
- Chụp màn hình nếu browser hỗ trợ và user cấp quyền.

## 3.2. Hiển thị ảnh

Ảnh chụp/copy/paste phải hiển thị **thumbnail thực tế**, không chỉ tên ảnh.

```text
┌──────────────────────────────┐
│ [thumbnail ảnh thực tế]      │
│ Ảnh đã dán · 1240×720        │
│ [Mở xem] [Gỡ]                │
└──────────────────────────────┘
```

Tên ảnh không bắt buộc vì người dùng thường dán ảnh mà không đặt tên.

Hiển thị:

- Thumbnail.
- Kích thước ảnh.
- Trạng thái upload/process.
- Nút preview.
- Nút gỡ.
- Lỗi định dạng/dung lượng.

## 3.3. Hiển thị file tài liệu

File tài liệu dùng file card có tên:

```text
📄 BaoCaoThucTap.docx · 1.8 MB
[Đang xử lý / Sẵn sàng / Lỗi] [Mở] [Gỡ]
```

## 3.4. Context chips

```text
Đang dùng:
[Workspace SprintA ×]
[Project Enterprise Platform ×]
[Task SPRINT-123 ×]
[Ảnh đã dán ×]
[BaoCao.docx ×]
```

Người dùng được xóa context trước khi gửi.

---

# 4. PHASE 2 — Định dạng MVP

Hỗ trợ trước:

## Ảnh

- PNG
- JPG/JPEG
- WEBP

## Tài liệu

- PDF
- DOCX
- TXT
- Markdown
- CSV
- XLSX
- PPTX
- JSON
- Source code dạng text có giới hạn

Không hỗ trợ trực tiếp:

- EXE, APK, DLL.
- File mã hóa/password-protected.
- Archive không kiểm soát.
- File nguy hiểm.
- Định dạng không có parser an toàn.

---

# 5. PHASE 3 — Upload & Processing Pipeline

```text
Upload
→ MIME validation thật
→ Size validation
→ Malware scan
→ Object storage
→ Metadata record
→ Extract text/image
→ Chunk
→ Index
→ AI response with citations
```

Bắt buộc:

- MIME detection, không chỉ extension.
- Giới hạn dung lượng theo loại file.
- Hash để tránh upload trùng.
- Object storage hoặc storage service.
- Signed/private URL.
- Chỉ user/workspace có quyền được tải.
- Có trạng thái Processing/Ready/Failed.
- Có cleanup file orphan.
- Không lưu secret/token/password.

---

# 6. PHASE 4 — Image Understanding

Người dùng có thể:

- Dán ảnh/chụp màn hình.
- Hỏi lỗi giao diện.
- Yêu cầu tóm tắt nội dung ảnh.
- Trích text/bảng từ ảnh.
- So sánh hai ảnh.
- Tạo task/checklist từ ảnh sau Preview.

AI response phải cho biết đang dùng ảnh nào.

Không yêu cầu tên ảnh.

Nếu ảnh không đủ rõ:

- Nói rõ không đọc được phần nào.
- Không bịa text.
- Cho phép người dùng crop/chọn vùng ở phase sau.

---

# 7. PHASE 5 — File Chat & RAG

AI có thể:

- Tóm tắt file.
- Trích action items.
- Tìm deadline.
- Tìm người phụ trách.
- So sánh hai tài liệu.
- Tạo checklist.
- Đề xuất task.
- Trích nguồn theo trang/slide/sheet nếu parser hỗ trợ.

Không nhét toàn bộ file vào prompt mỗi lần.

Citation ví dụ:

```text
Nguồn:
- BaoCaoThucTap.docx, mục 3.2
- KeHoach.xlsx, sheet Sprint 4, hàng 18–24
- Screenshot đã dán, vùng bảng tiến độ
```

---

# 8. PHASE 6 — Voice Input Việt/Anh

## 8.1. Luồng

```text
Bấm mic
→ Cấp quyền
→ Ghi âm
→ Dừng
→ Speech-to-text
→ Hiển thị transcript
→ User chỉnh sửa
→ User bấm Gửi
```

Không tự gửi sau khi nhận dạng.

## 8.2. UI

Bình thường:

```text
[+] Nhập nội dung... [🎤] [Gửi]
```

Đang ghi:

```text
Đang ghi 00:18
Ngôn ngữ: [Tự động VI/EN ▼]
[Hủy] [Dừng]
```

Sau nhận dạng:

```text
Transcript:
“Hãy tạo một công việc review báo cáo tuần 28...”

[Thu lại] [Dùng nội dung này]
```

## 8.3. Ngôn ngữ

MVP:

- Tự động Việt/Anh.
- Tiếng Việt.
- English.

Tên hiển thị phải rõ:

`Ngôn ngữ giọng nói: Tự động (VI/EN)`

Không dùng nhãn mơ hồ `Tự động (VI)`.

## 8.4. Safety/privacy

- Không lưu audio vĩnh viễn mặc định.
- Có consent.
- Có giới hạn thời lượng.
- Có error khi mic bị từ chối.
- Có retry.
- Có thể xóa audio sau transcription.
- Không tự tạo task từ transcript chưa confirm.

---

# 9. PHASE 7 — Chat UX Core

MVP:

- New chat.
- History.
- Search conversation.
- Rename/delete.
- Copy response.
- Edit and resend.
- Regenerate.
- Stop generating.
- Loading/error/rate-limit.
- Attachment preview.
- Context chips.
- Action Preview/Confirm/Cancel/Retry.
- State còn đúng sau F5.

Không làm lại phần đã có; chỉ bổ sung thiếu.

---

# 10. PHASE 8 — Tool Actions từ file/ảnh/voice

Tool đề xuất:

```text
summarize_attachment
extract_action_items
create_tasks_from_attachment
create_checklist_from_attachment
compare_documents
create_sticky_note_from_attachment
prepare_meeting_summary
```

Mọi mutation:

- Permission check.
- Preview.
- Project đích rõ ràng.
- Confirm.
- Idempotency.
- Audit.
- Không double-submit.

Nếu tạo task:

- Hiển thị Project đích.
- Nếu không có context rõ, bắt buộc chọn.
- Không tự chọn project đầu tiên.
- Hiển thị mã task và nút mở sau khi tạo.

---

# 11. Safety & Prompt Injection

File, ảnh, email và transcript là **dữ liệu**, không phải system instruction.

Bắt buộc:

- Không làm theo nội dung file yêu cầu bỏ permission.
- Không tiết lộ system prompt.
- Không truy cập file user khác.
- Không thực thi code trong file.
- Không mở URL/file nguy hiểm tự động.
- Moderation input/output.
- Audit mutation.
- Quét malware.
- Giới hạn file và request rate.
- Không tự gửi email trong MVP.
- Không tự bulk-create task chưa confirm.

---

# 12. UI/UX Reference Rules

Nguồn:

- Mobbin.
- Taste Skill.
- React Bits/Vue Bits.
- GSAP.
- 21st.

Giới hạn:

- Tối đa 3 Mobbin references cho AI drawer/composer.
- Chỉ đọc README và tối đa 2 ví dụ từ mỗi nguồn khác.
- Không clone toàn bộ.
- Không copy React JSX/Tailwind.
- Không thêm dependency nếu Vue/CSS đủ.
- Ảnh dán phải hiển thị thumbnail thật.
- Document hiển thị file card.

Thiết lập:

```text
DESIGN_VARIANCE = 4/10
MOTION_INTENSITY = 4/10
VISUAL_DENSITY = 7/10
```

---

# 13. Thứ tự triển khai

1. Audit.
2. Composer UI + thumbnail/file card.
3. Upload/storage/validation.
4. Image understanding.
5. Document extraction/RAG.
6. Voice VI/EN.
7. Chat UX bổ sung.
8. Tool actions.
9. Safety/evals.

Không làm tất cả trong một lượt.

---

# 14. Definition of Done

- Ảnh dán hiển thị thumbnail thật.
- File tài liệu hiển thị file card.
- Upload/private access đúng.
- F5 còn attachment/conversation.
- User khác không truy cập được.
- AI trả lời có source.
- Transcript chỉnh sửa được trước khi gửi.
- VI/EN hoạt động.
- Không tự gửi transcript.
- Mutation có Preview/Confirm.
- Project đích rõ ràng.
- Build frontend/backend pass.
- Browser/API/DB test pass.
- Không sửa file ngoài scope.

---

# 15. Báo cáo A–E

```text
A. Phase và trạng thái
B. File đã sửa
C. Chức năng thật hoàn thành
D. Test/API/DB evidence
E. Phần còn thiếu/rủi ro
```
