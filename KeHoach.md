BẠN LÀ SENIOR FULL-STACK ENGINEER + UI/UX REBUILD LEAD.

NHIỆM VỤ:
Audit toàn bộ source hiện tại, đối chiếu với tài liệu tích hợp và các ảnh minh họa trong file đính kèm, rồi cải thiện UI/UX + logic để Home Site trở thành một hệ thống enterprise sạch, đồng bộ, đúng luồng, đúng dữ liệu thật.

NGUYÊN TẮC BẮT BUỘC:

1) Không làm lại từ đầu.
2) Không phá kiến trúc hiện có.
3) Không trộn Home Site với Space Project.
4) Không dùng mock/demo để che core logic.
5) Chỉ dùng mock cho empty/loading/error/placeholder nếu thật sự chưa có dữ liệu.
6) Dữ liệu phải lấy thật từ API/database.
7) Giao diện phải đồng bộ: team, profile, goals, projects, sidebar, topbar, modal, tab, button, table, card.
8) Không để mỗi trang một style riêng.
9) Không hardcode text Anh/Việt lẫn lộn.
10) Nếu có route/tab thừa, phải gom đúng chỗ; nếu chỉ là tab trong detail thì không biến thành trang riêng.

KẾT QUẢ MONG MUỐN:
Trước khi code, hãy xuất một báo cáo gap analysis gồm:

- Đã có
- Còn thiếu
- Cần sửa / mở rộng
- Mock / placeholder còn sót
- Route / popup / API / DB còn thiếu
- Kế hoạch hoàn thiện tiếp
  Sau đó mới bắt đầu code phần cần thiết.

==================================================

1) GLOBAL UI / APP SHELL
   ==================================================

- Chỉ giữ 1 topbar global duy nhất.
- Search / notifications / settings / avatar / language phải đồng bộ.
- Language có tối thiểu Tiếng Việt + English.
- Sidebar trái phải đúng thứ tự và nhất quán.
- Trạng thái active / hover / focus thống nhất.
- Bố cục phải enterprise, sạch, nhiều khoảng trắng vừa đủ.
- Không dùng nested borders, không làm UI kiểu rối như dashboard AI.
- Toàn bộ card / table / modal / drawer / popup phải cùng ngôn ngữ thiết kế.

==================================================
2) SITE MANAGEMENT / HOME SITE
==============================

Trang site management là trang quản lý site thật, không phải Space Project.

Yêu cầu UI:

- Card grid site
- Owner badge
- Member count
- Project count
- Last accessed
- Create site
- Edit site
- Archive site
- Delete site
- Site settings
- Site members
- Search / filter / sort
- Click site đi đúng luồng

Luồng:

- Từ site select, nhấp “Bạn đang tìm site khác?” thì vào Home Site / Site Management.
- “For you” phải có:
  - Ứng dụng của bạn: các site gần đây đã tạo
  - Thường xuyên truy cập: các project thường xuyên truy cập trong site gần đây
  - Audit log: mọi hành động thay đổi data của goal / team / project phải ghi thật từ database
- “Xem tất cả” của audit log phải dẫn sang Recent.

Trang Recent:

- Hiển thị toàn bộ audit log gần đây của site.
- Có 3 bộ lọc thật: Project / Team / Goal.
- Lọc đúng dữ liệu thật, không giả.

Trang Starred:

- Hiển thị các team / project / goal đã được đánh sao.
- Có 3 bộ lọc như Recent để tìm nhanh.
- Danh sách và filter đều phải thật.

==================================================
3) TEAM
=======

Tab “Mọi người”:

- Sửa bộ lọc để bỏ chữ “Lọc theo” thừa.
- Lọc theo dự án / mục tiêu / team phải là logic thật.
- Bộ lọc theo role/profile gồm:
  - chức danh
  - phòng ban
  - vị trí
- Chỉ lọc được những người có dữ liệu đã lưu trong profile.
- Người nào để trống field đó thì không hiện vào filter đó.

Tab “Giới thiệu”:

- Bổ sung chức năng lưu tiểu sử / bio thật.
- Đây không chỉ là giao diện.
- Thêm thành viên thì giữ logic hiện tại nếu đã chạy đúng.

Tab “Phân cấp”:

- Sửa logic chọn nhóm:
  - nhóm đã được chọn rồi thì không được chọn lại
  - nhánh chính chỉ chọn 1 nhóm
  - nhánh phụ chọn được nhiều nhóm
- UI phải thể hiện rõ primary branch và secondary branches.

Tab “Mục tiêu”:

- Nút “Thêm mục tiêu” phải xổ đúng danh sách mục tiêu hiện có.
- Tuyệt đối không dùng demo data.
- Chọn mục tiêu phải lưu thật.

Tab “Dự án”:

- Tương tự mục tiêu.
- Nút thêm phải mở danh sách dự án thật.

Tab “Khen ngợi”:

- Khi bấm vào phải mở trang khen ngợi riêng.
- Avatar ở trên phải mở danh sách team và thành viên trong site để chọn người/nhóm được khen.
- Thêm nút “Gửi”.
- Khi gửi lời khen thì lưu vào danh sách khen ngợi thật.
- Nếu khen team thì lời khen hiển thị cho các profile trong team liên quan.
- Feed khen ngợi phải có:
  - avatar người gửi
  - team hoặc member được khen
  - ảnh minh họa / nội dung lời khen
  - reaction icon
- UI trang này không được sơ sài.

Chi tiết team bên phải:

- Dùng nút dấu cộng để thêm nhanh dự án trong site cho team.
- Xóa phần “thêm không gian” và “liên kết” nếu không còn dùng.
- Phải xem được:
  - đội ngũ gốc
  - đội ngũ phụ
- Phần người quản lý:
  - bấm vào hiện danh sách user để chọn manager
  - hoặc tự động lấy người tạo nhóm làm manager khi tạo mới
- Layout phần detail phải đồng bộ với goal/project/profile.

==================================================
4) GOALS
========

Danh sách Goals:

- Có filter phía trên.
- Có các phân nhóm đúng: hiện tại, đã hoàn thành, đang theo dõi, đã lưu trữ nếu route đã có.
- UI danh sách phải rõ, enterprise.

Chi tiết Goal:
Tab Overview:

- Bổ sung mô tả có placeholder.
- Comment phải có:
  - nút gửi
  - nút sửa màu xanh
  - nút xóa màu đỏ
  - reply bằng cách tag gmail
- Gmail được tag phải được tô xanh đậm để dễ nhận biết.
- Comment mới nhất phải ở trên.
- Comment cũ trôi xuống dưới.
- Có thể lưu comment thật.

Tab Updates:

- Sửa bố cục để ngày mục tiêu không che tiến độ.
- Phải sửa input layout để nhập progress được.
- Thêm chức năng gửi ảnh bằng file picker / upload file thật.
- Sửa phần icon:
  - không được chỉ dính 1 icon cố định
  - khi bấm phải mở bộ chọn icon như Facebook
  - có thể chọn nhiều icon
- Khi bấm đăng:
  - lưu update thật
  - hiển thị latest update
  - hiện avatar + gmail người update
  - hiện thời gian update
  - hiện số lượng người xem
  - hiện mô tả thay đổi
  - hiện trạng thái cũ → trạng thái mới
- Bên dưới update phải có:
  - sửa / xóa update
  - 4 reaction nhanh: like, vỗ tay, pháo bông, tim
  - icon mở danh sách icon để chọn thêm
  - comment cho riêng update đó
  - nút gửi comment

Tab SprintA:

- Đây là tab nối Goal với task trong Space Project.
- Nút thêm dự án/task phải:
  - mở dropdown danh sách task thật
  - hoặc cho nhập link phía trên để liên kết nhanh
- Hover task có thể xem trước, sao chép link, xem liên quan.
- Click task phải đi đúng sang Space Project.

Tab Projects:

- Đây là project của Goal, không phải project của Space.
- Nút thêm phải mở popup danh sách project thật.
- Goal liên kết với project phải hiển thị thật.

Tab Lessons / Risks / Decisions:

- Bắt buộc đồng bộ bộ công cụ text editor với tab Updates.
- Các nút như heading, bold, italic, màu chữ, tag… phải có backend xử lý thật.
- Nhấn Save thì lưu vào danh sách thật.

Tab History:

- Hiển thị audit log / lịch sử thay đổi thật.

Khối bên phải của Goal detail:

- Phải có đầy đủ:
  - progress
  - owner
  - follower
  - team
  - due date
  - main goal chỉ 1
  - sub goals nhiều
  - read-only / archive state
- Khi đổi trạng thái phải tự chuyển về tab Updates.

==================================================
5) PROJECTS
===========

Project trong Home Site phải có chiều sâu ngang Goal, nhưng logic khác Space Project.

Danh sách:

- Directory
- Following
- Status Updates
- Archived
- Search / filter / sort

Chi tiết Project:
Tab Overview:

- Có phần giới thiệu chi tiết hơn overview thường lệ.
- Có:
  - lý do làm dự án
  - tiêu chí thành công
  - nhận xét
  - contributors / team phụ trách
  - linked goals
  - related projects
  - tracked link 1 cái
  - linked tasks nhiều cái
  - ngày chốt dự án

Tab Comments:

- Comment phải lưu được thật, tương tự Goals.

Tab Updates:

- Logic giống Goals nhưng thiết kế đẹp hơn.
- Cập nhật tiến độ phải có backend thật.
- Báo cáo update phải lưu và render đẹp.

Tab Lessons / Risks / Decisions:

- Tương tự Goals.
- Dùng cùng editor toolbar.
- Save xong phải có danh sách thật.

Chi tiết bên phải:

- Không cần đổi core logic nếu đã đúng.
- Nhưng bố cục phải đồng bộ với Team và Goal.
- Không để mỗi khu một layout khác nhau.

Phân biệt:

- Project của Home Site và Project của Space Project phải tách logic rõ ràng.
- Click project phải mở detail đúng.
- Click linked goal/task phải đi đúng màn hình tương ứng.

==================================================
6) PEOPLE / PROFILE
===================

Directory:

- search / filter / sort thật
- add / invite người thật

Profile Detail:

- bio
- recent tasks tối đa 6
- email
- teams tối đa 4
- frequently visited spaces
- current / following goals
- current / completed projects
- received / given kudos
- popup thêm role / department / position
- layout rõ, enterprise

Chức năng:

- bio chỉnh được thật
- click avatar mở profile thật
- goals / projects / kudos / history load từ API thật
- role / department / position phải lưu được thật

==================================================
7) TOOLS
========

- Audit Log: timeline thật
- Starred: list/filter thật
- Notifications: unread / all
- Status: timeline trạng thái của Goals và Projects
- Không còn mock giả vờ chạy

==================================================
8) EMPTY / LOADING / ERROR STATES
=================================

- Mọi màn chưa có dữ liệu hoặc đang load phải có:
  - loading skeleton
  - empty state
  - error state
  - retry nếu cần
- Không được dùng mock data thay cho trạng thái thật.

==================================================
9) STYLE GOAL
=============

Thiết kế mục tiêu:

- sạch
- enterprise
- modern Jira-like
- rõ hierarchy
- spacing hợp lý
- tab và popup gọn
- thống nhất toàn hệ thống
- không game hóa
- không lòe loẹt

==================================================
10) OUTPUT BẮT BUỘC TRƯỚC KHI CODE
======================================

Hãy trả về đúng 6 phần:

1. Tôi hiểu hệ thống như sau
2. Danh sách phần đã có
3. Danh sách phần còn thiếu
4. Danh sách phần cần sửa / mở rộng
5. Kế hoạch phát triển tiếp trên nền hiện tại
6. Những điểm còn mơ hồ cần xác nhận

Chỉ khi phần đó xong và không còn mơ hồ thì mới bắt đầu code toàn bộ phần thiếu theo đúng mô tả.
