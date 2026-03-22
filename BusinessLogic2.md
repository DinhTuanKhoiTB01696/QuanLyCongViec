# TÀI LIỆU ĐẶC TẢ KỸ THUẬT - MODULE 8: REAL-TIME & COLLABORATION

**Mục tiêu:** Đồng bộ hóa dữ liệu bảng Kanban ngay lập tức (Real-time) trên nhiều thiết bị mà không cần tải lại trang, đồng thời đảm bảo an toàn tuyệt đối cho luồng quản lý file và bình luận.

**Tech Stack:** .NET (Backend) & Vue 3 (Frontend).

---

## 1. KIẾN TRÚC ĐỒNG BỘ THỜI GIAN THỰC (REAL-TIME SYNCHRONIZATION)

Để các thao tác (kéo thả Task, Comment) cập nhật ngay lập tức trên màn hình của người khác, hệ thống sử dụng kết hợp 3 nền tảng kỹ thuật sau:

* **Cơ chế Server Push:** Trái ngược với Request-Response truyền thống (Client hỏi - Server trả lời), đường truyền mạng sẽ luôn được mở. Khi có thay đổi trong Database, Backend sẽ chủ động "đẩy" dữ liệu thẳng vào trình duyệt của những người dùng đang xem mà không cần họ thao tác.
* **Giao thức WebSocket:** Sử dụng thư viện **SignalR** của .NET để tạo ra "đường ống ngầm" kết nối liên tục 24/7 (2 chiều) giữa Frontend Vue 3 và Backend .NET.
* **Mô hình Pub/Sub (Xuất bản - Đăng ký):** Thuật toán phân luồng đảm bảo tính bảo mật và chính xác của dữ liệu:
  * **Subscribe (Đăng ký):** Người dùng mở Dự án A sẽ tự động được đưa vào kênh lắng nghe `Project_A` trên Server.
  * **Publish (Xuất bản):** Bất kỳ API thay đổi dữ liệu nào thành công (VD: di chuyển Task), Server lập tức phát sóng tín hiệu `TaskMoved` chỉ riêng vào kênh `Project_A`.
  * **Reaction (Phản ứng UI):** Trình duyệt của các thành viên trong kênh nhận tín hiệu, tự động chạy hàm cập nhật trạng thái UI trên RAM mượt mà.

---

## 2. CÁC "TỬ HUYỆT" KỸ THUẬT & TIÊU CHUẨN XỬ LÝ

Đây là các rủi ro hệ thống nghiêm trọng và bộ quy tắc bắt buộc mọi lập trình viên trong dự án phải tuân thủ để tránh sập Server hoặc rò rỉ dữ liệu.

### ☠️ Tử huyệt 1: Race Condition (Xung đột trạng thái Kanban)

* **Ngữ cảnh:** Hai người dùng cùng kéo một Task ở cùng một thời điểm vào hai cột khác nhau.
* ❌ **Bắt buộc tránh:** Backend lưu đè dữ liệu một cách nhắm mắt. Dẫn đến giao diện hiển thị sai lệch so với Database (hiện tượng bóng ma).
* ✅ **Giải pháp (Optimistic Concurrency - Đồng thời lạc quan):**
  * **Backend (.NET):** Thêm cột `RowVersion` (hoặc `LastUpdatedAt`) vào bảng `Tasks`. API Cập nhật trạng thái phải nhận `RowVersion` từ Frontend và kiểm tra: `if (dbTask.RowVersion != request.RowVersion) return Conflict();` (Mã lỗi 409).
  * **Frontend (Vue 3):** Khi nhận lỗi 409, tuyệt đối không di chuyển Task. Kích hoạt logic Rollback: hất Task về cột cũ, hiển thị Toast báo lỗi: *"Task này vừa được thay đổi bởi người khác"*, và gọi lại API GET để đồng bộ.

### ☠️ Tử huyệt 2: Bão Reconnection & Dò rỉ bộ nhớ (Memory Leak)

* **Ngữ cảnh:** Server khởi động lại, hàng ngàn kết nối SignalR đồng loạt gọi hàm Reconnect trong cùng một giây gây DDoS cục bộ.
* ❌ **Bắt buộc tránh:** Frontend dùng vòng lặp `setInterval` kết nối vô tội vạ. Backend không xóa kết nối rác khi User đóng tab.
* ✅ **Giải pháp:**
  * **Frontend (Exponential Backoff):** Cấu hình tự giãn cách thời gian kết nối lại để giảm tải cho Server.

    ```javascript
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/kanban-hub")
        .withAutomaticReconnect([0, 2000, 10000, 30000]) // Giãn cách: 0s, 2s, 10s, 30s
        .build();
    ```

  * **Backend (Dọn rác tự động):** Ghi đè hàm ngắt kết nối trong Hub để dọn dẹp RAM.

    ```csharp
    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var projectId = GetProjectIdFromUser(); 
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Project_{projectId}");
        await base.OnDisconnectedAsync(exception);
    }
    ```

### ☠️ Tử huyệt 3: Lỗ hổng IDOR & Public File (Lộ tài liệu mật)

* **Ngữ cảnh:** File nhạy cảm của dự án (hợp đồng, bảng lương) bị truy cập trái phép qua một đường link public lộ ra ngoài.
* ❌ **Bắt buộc tránh:** Lưu file thẳng vào thư mục public (`wwwroot`) với tên gốc. Không kiểm tra quyền tải file.
* ✅ **Giải pháp:**
  * **Lưu trữ kín:** Đổi tên file dạng chuỗi ngẫu nhiên (`Guid.NewGuid()`) và lưu vào thư mục/server độc lập (S3 Bucket riêng tư) chặn truy cập trực tiếp từ Internet.
  * **Tải qua Proxy API:** Sinh ra API tải file và tái sử dụng Filter phân quyền.

    ```csharp
    [HttpGet("/api/projects/{projectId}/files/{fileId}")]
    [ProjectAuthorize(Roles = "PO, SM, PM, TechLead, DEV, QA, Designer, Guest, Stakeholder")]
    public IActionResult DownloadFile(int projectId, int fileId)
    {
        // Xác thực JWT -> Query lấy đường dẫn thực -> Trả stream:
        return PhysicalFile(realFilePath, "application/pdf", originalFileName);
    }
    ```

### ☠️ Tử huyệt 4: XSS (Cross-Site Scripting) qua Hệ thống Bình luận

* **Ngữ cảnh:** Hacker chèn mã độc JavaScript dạng `<script>` vào nội dung bình luận để đánh cắp Token của người dùng khác khi họ vô tình đọc comment đó.
* ❌ **Bắt buộc tránh:** Frontend dùng thẻ `v-html` kết xuất text bừa bãi. Backend tin tưởng tuyệt đối vào chuỗi string gửi lên.
* ✅ **Giải pháp (Bảo mật 2 lớp):**
  * **Backend (.NET):** Dùng thư viện `HtmlSanitizer` tước bỏ mọi thẻ nhạy cảm (`<script>`, `<iframe>`, `onmouseover`...) trước khi lưu `SaveChanges` vào DB.
  * **Frontend (Vue 3):** Mặc định hiển thị bằng nội suy `{{ }}` để Vue tự động escape ký tự. Nếu bắt buộc hỗ trợ bình luận Rich-Text, phải tích hợp thư viện **DOMPurify** để lọc nội dung thêm một lần nữa trước khi dùng `v-html`.