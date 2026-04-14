# Báo cáo Phân tích Trải nghiệm và Kế hoạch Phát triển Tối ưu cho AI

Dựa trên quá trình nội soi trực tiếp vào phiên bản gốc `http://localhost:3000/cybwf/` đang chạy trên máy bạn và đối chiếu với thiết kế của Plane gốc, không gian UI của trang **Dashboard / Space Home** và **Projects** cực kỳ phẳng, tối giản (Dark Mode `#0D0D0D`), và lấy tốc độ tương tác làm trọng tâm.

Tài liệu này đóng vai trò là **"Kế hoạch Tổng" (Master Plan)** để bạn điều phối cùng lúc 3 Engine AI (VD: ChatGPT, Claude, Cursor) giúp tối ưu hóa tiến độ làm việc, cho phép cả nhóm AI làm việc độc lập nhưng kết quả ráp lại lại vô cùng khớp nhau.

---

## A. Phân Tích Giao Diện Tổng Hợp (Dựa trên ảnh & Khảo sát localhost)

1. **Sidebar Navigation (Bên trái):**
   - **Giao diện:** Trong suốt, biểu tượng icon outline mảnh. Viền xám mờ phân tách.
   - **Menu:** Có các tab cơ bản: `Home`, `Drafts`, `Your work`, `Stickies` (tạm che lại nếu không cần thiết). Phần Workspace gồm `Projects`.
2. **Main Content (Dashboard / Home):**
   - Có lời chào `Good morning, [Tên User]` kèm theo ngày giờ.
   - Thẻ Quickstart Guide (4 Card): `Create a project`, `Invite your team`, v.v...
   - Toàn bộ khung viền (border) hoặc nền (background) đều sử dụng các Layer tĩnh mịch (Layer 1, Layer 2) để đổ bóng, thiết kế Glassmorphism trên nền xám than. Dùng `:focus-within` để nổi viền chứ không tạo viền nét đứt cứng nhắc.
3. **Logic Flow:**
   - Dữ liệu hoàn toàn **Reactive** (phản ứng tức thời). Không có thao tác cuộn trang reload lại trình duyệt mà mọi thứ load trên một nền SPA (Single Page Application).

---

## B. Lệnh Phân Công Công Việc Cho 3 AI Hoạt Động Cùng Lúc

Bạn hãy *Copy* chính xác các ô Prompt dưới đây, mang dán vào 3 luồng AI khác nhau. Hãy nhớ đính kèm file Báo cáo này để cho AI làm Context Base.

### 🤖 AI #1: Chuyên Viên Cơ Sở Dữ Liệu & Backend (DB & Logic Data)
**Nhiệm vụ:** Đảm bảo toàn bộ Database đã sẵn sàng phục vụ cho bộ khung giao diện của Plane. Bơm dữ liệu ban đầu. Đừng làm gì nếu DB đã chuẩn.
> **Copy dòng lệnh sau dán cho AI 1:**
> *"Bạn là chuyên gia Backend / Database. Hãy kiểm tra các file Entities/Models và DB Schema hiện tại của dự án Quản Lý Công Việc. Rà soát các bảng: `Projects`, `WorkItems/Issues`, `WorkspaceMembers`, `IssueLabels`, và `IssueAssignees`.*
> 1. Nếu cấu trúc DB ĐÃ CÓ các trường bắt buộc như `State` (Backlog, Todo, InProgress, Done), `Priority` (None, Low, Medium, High, Urgent), `ParentId` (để làm sub-task) thì **không cần tạo thêm bảng**.
> 2. ĐẾU CHƯA CÓ, hãy ngay lập tức tạo Migration script để thêm bảng/trường phù hợp.
> 3. Hãy viết script hoặc DbSeed tự động chèn vào Database một số dữ liệu mẫu (Mock data) của các Project và WorkItem để tôi có thể test giao diện một cách trực quan, đặc biệt là phải có ít nhất mỗi Trạng thái một Task mẫu!*

### 🤖 AI #2: Chuyên Viên Giao Diện Tĩnh (Static UI Designer)
**Nhiệm vụ:** Xây ngay bộ khung giao diện Frontend cực đẹp theo chuẩn Dark theme mà không cần nối ghép API hay Logic lằng nhằng, tránh làm hỏng cấu trúc.
> **Copy dòng lệnh sau dán cho AI 2 (Lưu ý: Gửi Kèm cái ảnh Dashboard bạn vừa chụp ở trên nhé):**
> *"Bạn là chuyên gia thiết kế Frontend UI/UX (Vue.js/Tailwind/CSS). Tôi vừa đính kèm bức ảnh chụp giao diện Dashboard (Space Home) của Plane App. Hãy thiết kế lại file `SpaceSummary.vue` và `Dashboard.vue` sao cho bám sát bức ảnh tĩnh này 100%.*
> *Quy tắc cần nhớ:*
> 1. Đây là màn hình TĨNH. Đừng viết các hàm gọi API, Axios hay Pinia State vào lúc này. Không chèn Logic động để tránh lỗi biên dịch.
> 2. Giữ nguyên cấu trúc CSS tĩnh: Background màu tối (#0D0D0D), Text màu ngà sáng (#E5E7EB), các viền thẻ (Cards) đều bo góc, sử dụng icon rỗng (Luicide/Propel).*
> 3. Layout theo Flexbox (Sidebar bên trái và Vùng hiển thị bên phải). Điền dữ liệu mock thẳng vào trong HTML template (Ví dụ gõ cứng chữ: 'Good morning, Admin'). Chỉ tập trung duy nhất vào SỰ XUẤT SẮC CỦA THẨM MỸ!"*

### 🤖 AI #3: Chuyên Viên Ghép Nối Logic Front-to-Back (Data Integration)
**Nhiệm vụ:** Sau khi CSDL và Giao diện tĩnh đã có, AI này chịu trách nhiệm so sánh logic xem giao diện đã khớp với Data chưa, rồi viết các hàm phản ứng ghép data vào màn hình một cách nhịp nhàng.
> **Copy dòng lệnh sau dán cho AI 3:**
> *"Mục tiêu của bạn là kiểm tra lại giao diện Frontend (như `ListView.vue`, `Project.vue`) và xem nó có đáp ứng đúng với Backend API và Database Models hay không dựa trên tài liệu báo cáo giao diện Plane.*
> 1. Viết các hàm `fetchProjects`, `fetchWorkItems` và liên kết với Pinia Store (hoặc Vuex).
> 2. Thay thế tất cả các giá trị tĩnh (Mock Text) trên giao diện tĩnh bằng dữ liệu Render thật từ phía Server (`v-for`, `v-bind`).
> 3. Với các tính năng Lọc (Filter bằng Dropdown - ví dụ Lọc theo Priority) hay thay đổi Trạng Thái trên Kanban Board, hãy code các logic lắng nghe sự kiện để gọi API PUT/PATCH tương ứng nhưng không thao tác Reload cả trang web (Cập nhật giao diện Optimistic UI).*
> *Nếu có điểm nào giao diện tĩnh đang làm chưa khớp với dữ liệu từ DB (Ví dụ DB trả về list UUID nhưng View lại chờ String), hãy sửa thẳng vào Component View để nó đồng bộ!"*

---

Việc dồn 3 prompt này cho 3 phiên AI làm việc cùng lúc sẽ rút ngắn thời gian phát triển xuống chỉ còn 1/3, đồng thời giải phóng đầu óc cho bạn phân tách rõ ràng luồng Tĩnh (UI) và luồng Động (Data)!
