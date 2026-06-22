Bạn phải triển khai theo Functional Spec dưới đây. Đây không phải prompt lập kế hoạch. Đây là đặc tả sản phẩm để code.

Nhiệm vụ của bạn: đọc source hiện tại, tìm đúng route/component/store/API/backend/entity hiện có, rồi triển khai chức năng end-to-end. Một chức năng chỉ được tính là xong khi đi đủ luồng:

UI thao tác được → Store/composable gọi API thật → Backend xử lý → Database lưu thật → reload/F5 dữ liệu vẫn còn → build không lỗi.

Không được:

* Không dùng mock/demo/fake/hard-code data.
* Không tạo nút click được nhưng không gọi API.
* Không gỡ disabled nếu backend/store/API chưa nối thật.
* Không chỉ sửa UI.
* Không chỉ sửa backend.
* Không tạo bảng/API/component trùng nếu cái cũ mở rộng được.
* Không dùng script regex/Powershell để thay hàng loạt Vue template.
* Không xóa migration cũ/snapshot nếu không có lý do thật.
* Không báo hoàn thành nếu F5 mất dữ liệu.
* Không báo cáo giữa chừng thay cho code.
* Không sửa lan man ngoài chức năng đang làm.

Mọi dữ liệu phải scope theo site/workspace hiện tại. Không được lấy Team/Goal/Project/User/Comment/Audit/Starred của site khác.

============================================================
I. HOMESITE / FOR YOU
=====================

Bối cảnh sản phẩm:
Từ trang Site Select, khi người dùng bấm “Bạn đang tìm site khác?” hoặc “Bạn muốn đến trang khác?”, app phải đi vào trang quản lý của site hiện tại, ví dụ site slug dạng `tua4699-2099`. Mỗi site có một trang quản lý riêng.

Hiện trạng cần sửa:
`/home` không được redirect thẳng sang Team nếu yêu cầu là Home/For You. Phải có trang For You đúng nghĩa.

Trang For You cần có 3 khu vực chính:

1. Khu “Ứng dụng của bạn”

* Nằm phía trên.
* Hiển thị danh sách site/workspace gần đây user đã tạo hoặc truy cập.
* Mỗi item/card hiển thị:

  * avatar/icon site nếu có
  * tên site
  * slug/key site
  * nút vào site
* Lấy dữ liệu thật từ database/API theo user hiện tại.
* Nếu chưa có dữ liệu recent site thật, dùng API workspace thật hiện có và hiển thị empty state rõ ràng, không được bịa mock.

2. Khu “Thường xuyên truy cập”

* Nằm bên dưới “Ứng dụng của bạn”.
* Hiển thị các project thường xuyên truy cập trong site hiện tại.
* Chỉ lấy project thuộc site/workspace hiện tại.
* Nếu hệ thống đã có bảng tracking access thì dùng tracking thật.
* Nếu chưa có tracking, không được tạo dữ liệu giả. Hiển thị empty state hoặc tạo tracking backend thật nếu scope task cho phép.

3. Khu “Audit log preview”

* Nằm bên dưới “Thường xuyên truy cập”.
* Hiển thị một số hoạt động gần nhất của site.
* Dữ liệu phải lấy từ audit log thật.
* Mỗi dòng log cần có:

  * icon/entity type: Team/Goal/Project
  * người thực hiện
  * hành động
  * tên entity bị tác động
  * thời gian
* Các hành động cần ghi log khi có mutation:

  * tạo/sửa/xóa Team
  * đổi manager Team
  * link/unlink Team với Goal/Project
  * tạo/sửa/xóa Goal
  * đổi status/progress Goal
  * comment/update/reaction trong Goal
  * tạo/sửa/xóa Project
  * đổi status/progress Project
  * star/unstar Team/Goal/Project
* Nút “Xem tất cả” phải điều hướng sang Recent.

Definition of Done:

* `/home` hiển thị For You đúng 3 khu.
* Dữ liệu lấy từ API/database thật.
* Không còn redirect mất For You.
* Build xanh.
* F5 không mất dữ liệu.

============================================================
II. RECENT
==========

Trang Recent là nơi xem toàn bộ audit log của site hiện tại.

UI cần có:

* Danh sách audit log mới nhất nằm trên.
* Có empty state: “Chưa có hoạt động gần đây trong site này”.
* Có filter chính:

  * Project
  * Team
  * Goal
* Filter có thể là tab/chip/dropdown, nhưng phải rõ ràng và dùng dữ liệu thật.
* Khi chọn Project, chỉ hiện audit log entity/action liên quan Project.
* Khi chọn Team, chỉ hiện audit log entity/action liên quan Team.
* Khi chọn Goal, chỉ hiện audit log entity/action liên quan Goal.
* Mỗi dòng log click được nếu có entity target, điều hướng đến đúng Team/Goal/Project detail.

Backend/API:

* Hoàn thiện endpoint audit log theo workspace/site.
* Hỗ trợ query:

  * entityType
  * limit
  * page nếu có
* Không dùng AuditLog cứng với task nếu không phù hợp. Ưu tiên mở rộng log chung hiện có như SystemAuditLog nếu có.
* Không tạo SiteAuditLog mới nếu log cũ mở rộng được.

Definition of Done:

* Recent load từ audit DB thật.
* Filter Project/Team/Goal gọi API hoặc filter từ dữ liệu audit thật.
* F5 vẫn đúng.
* Không mock.

============================================================
III. STARRED
============

Trang Starred chỉ hiển thị Team/Goal/Project đã được user đánh sao trong site hiện tại.

UI cần có:

* Danh sách starred item.
* Filter:

  * Team
  * Goal
  * Project
* Empty state giống ảnh: nếu chưa có item thì hiển thị thông báo “Chưa có mục yêu thích nào”.
* Star/unstar ở bất kỳ nơi nào phải đồng bộ với trang này.
* Khi unstar trong Starred, item biến mất ngay.

Backend/API:

* Dùng entity StarredItem hiện có nếu đã có.
* Mỗi record cần scope:

  * workspace/site
  * user
  * itemType
  * itemId
* API cần có:

  * fetch starred items
  * star/toggle star
  * unstar
* Không tạo bảng favorite mới nếu StarredItem đã đáp ứng.

Definition of Done:

* Star Team/Goal/Project → sang Starred thấy.
* Unstar → biến mất.
* F5 vẫn giữ.
* Không mock.

============================================================
IV. PEOPLE / MỌI NGƯỜI
======================

Trang People cần sửa bộ lọc, không phải chỉ sửa chữ.

UI bộ lọc:

* Bỏ chữ thừa “Lọc theo” nếu đang lặp nhiều.
* Các filter hiển thị ngắn gọn:

  * Dự án
  * Mục tiêu
  * Đội ngũ
  * Chức danh
  * Phòng ban
  * Vị trí
* Có nút clear/reset filter.
* Search user vẫn hoạt động kết hợp với filter.

Logic filter:

1. Filter Dự án

* Dropdown lấy danh sách Project thật trong site.
* Khi chọn Project, chỉ hiện user/thành viên đang liên quan hoặc được gắn với Project đó.
* Không dùng project mock.

2. Filter Mục tiêu

* Dropdown lấy danh sách Goal thật trong site.
* Khi chọn Goal, chỉ hiện user liên quan Goal đó.

3. Filter Đội ngũ

* Dropdown lấy danh sách Team thật trong site.
* Khi chọn Team, chỉ hiện thành viên thuộc team đó.

4. Filter Chức danh / Phòng ban / Vị trí

* Option lấy từ profile thật của thành viên trong site.
* Dùng distinct values từ user profile.
* Người nào để trống field thì không tạo option trống.
* Chọn option nào thì chỉ hiện user có profile khớp.
* Không hard-code “Front-end”, “Back-end”, “Manager”, v.v.

Definition of Done:

* Filter dropdown có dữ liệu thật.
* Kết quả filter đúng.
* Không lấy user ngoài site.
* F5 vẫn load được filter options.

============================================================
V. TEAM DETAIL
==============

A. Tab Giới thiệu
Mục tiêu:

* Ô tiểu sử/mô tả Team hiện chỉ là UI thì phải biến thành chức năng thật.
* Người dùng nhập mô tả → bấm lưu → gọi API → lưu DB.
* Reload/F5 vẫn còn mô tả.
* Nút thêm thành viên đang hoạt động thì giữ lại, không phá.

UI:

* Có placeholder rõ ràng nếu chưa có mô tả.
* Có trạng thái đang lưu/loading.
* Có thông báo lỗi nếu API fail.

B. Tab Phân cấp
Mục tiêu:

* Sửa logic chọn đội ngũ gốc và đội ngũ phụ.

UI/logic:

1. Nhánh chính / Đội ngũ gốc

* Chỉ chọn được 1 team.
* Không được chọn chính team hiện tại.
* Không được chọn team đã nằm trong nhánh phụ.
* Không được tạo vòng lặp parent/child.
* Lưu DB thật.

2. Nhánh phụ / Đội ngũ phụ

* Có thể chọn nhiều team.
* Team nào đã chọn rồi thì không hiển thị lại trong danh sách chọn.
* Không được chọn chính team hiện tại.
* Không được chọn team đã là nhánh chính.
* Lưu DB thật.

Sidebar sau khi lưu:

* “Đội ngũ gốc” phải hiện đúng team cha nếu có.
* “Đội ngũ phụ” phải hiện đúng danh sách team phụ nếu có.
* Không được còn hiện “Không có đội ngũ gốc/phụ” khi đã chọn.

C. Tab Mục tiêu

* Nút “Thêm mục tiêu” mở dropdown/popup danh sách Goal thật trong site.
* Chỉ hiện Goal chưa gắn với team này.
* Chọn Goal → lưu quan hệ Team–Goal vào DB.
* Danh sách goal trong tab cập nhật ngay.
* F5 vẫn giữ.
* Nếu mô hình hiện tại là Goal.DepartmentId thì update DepartmentId.
* Nếu sản phẩm cần Goal thuộc nhiều Team thì dùng bảng link, nhưng không tự tạo nếu source hiện chỉ support 1 team.

D. Tab Dự án

* Nút “Thêm dự án” mở dropdown/popup Project thật trong site.
* Chỉ hiện Project chưa link với team này.
* Chọn Project → lưu DB.
* F5 vẫn giữ.
* Sidebar quick add project dùng cùng logic này.

E. Tab Khen ngợi
Mục tiêu:

* Đây là trang khen ngợi riêng cho Team.
* Avatar/người nhận ở trên khi click phải xổ danh sách:

  * thành viên thật trong site
  * team thật trong site
* Cho phép chọn khen một người hoặc một team.

Form cần có:

* recipient picker
* nội dung lời khen
* icon/badge nếu UI có
* nút Gửi

Khi gửi:

* Gọi API thật.
* Lưu vào DB.
* Thêm lời khen vào danh sách bên dưới.
* Mới nhất nằm trên.
* F5 vẫn còn.
* Không dùng mockPeopleList/mockTeamList.
* Nếu có Kudo entity sẵn thì mở rộng Kudo, không tạo Praise trùng nếu không cần.

F. Sidebar phải của Team

* Có dấu cộng thêm nhanh Project:

  * click dấu cộng → dropdown Project thật
  * chọn project → link team với project → lưu DB
* Xóa/ẩn mục không cần:

  * “Thêm không gian”
  * “Liên kết”
    nếu không có logic thật.
* Người quản lý:

  * click manager → xổ user thật trong site
  * chọn 1 người làm manager
  * lưu DB
  * header/sidebar/team list đồng bộ
  * khi tạo team mới, người tạo là manager mặc định nếu chưa chọn ai
* Avatar manager/member lấy avatar thật, fallback chữ cái nếu không có avatar.

============================================================
VI. GOAL LIST
=============

Trang danh sách Goal cần filter thật.

Filter gồm:

* Team
* Project
* Owner/chủ sở hữu
* Status/trạng thái
* Progress/tiến độ
* Starred nếu có

Yêu cầu:

* Tất cả option lấy từ dữ liệu thật.
* Không hard-code status/team/project/owner.
* Filter có thể server-side hoặc client-side trên dữ liệu thật, nhưng không được dùng mảng giả.
* Click vào goal đi đúng GoalDetail.
* Star state nếu hiển thị phải đồng bộ StarredItem.

============================================================
VII. GOAL DETAIL — TAB TỔNG QUAN
================================

A. Mô tả

* Có placeholder: “Nhập mô tả mục tiêu...” hoặc tương đương.
* Cho phép nhập/sửa/lưu.
* Lưu DB thật.
* F5 vẫn còn.

B. Comment
UI:

* Ô nhập comment có nút Gửi nằm phía trên danh sách comment.
* Comment mới nhất nằm trên, comment cũ xuống dưới.
* Mỗi comment hiển thị:

  * avatar thật
  * tên/email người comment
  * thời gian
  * nội dung
* Có nút Sửa màu xanh.
* Có nút Xóa màu đỏ.
* Có reply/tag email nếu người dùng nhập email/Gmail.
* Email được tô đậm màu xanh dương.

Logic:

* Load comment thật khi mở GoalDetail.
* Gửi comment gọi API thật.
* Sửa comment gọi API thật.
* Xóa comment gọi API thật.
* Sau mutation UI refetch hoặc update từ response thật.
* F5 vẫn giữ.
* Không được có nút gửi click được nhưng không gọi API.

Endpoint tối thiểu nếu chưa có:

* GET comments by goal
* POST comment
* PUT comment
* DELETE comment

============================================================
VIII. GOAL DETAIL — TAB CẬP NHẬT
================================

Vấn đề trong ảnh:

* Phần ngày mục tiêu đang che tiến độ.
* Editor/update box còn sơ sài.
* Upload ảnh chưa có.
* Icon/reaction đang bị gắn cứng.
* Dưới update cần timeline/report đẹp.

Cần cải tiến:

A. Layout

* Tách rõ khu ngày mục tiêu và khu progress.
* Progress input/slider phải nhìn thấy và nhập được.
* Dropdown/status/datepicker không bị che sau popup.
* Editor không tràn.

B. Form đăng update
Form phải có:

* Nội dung update bằng RichTextEditor.
* Chọn status mới nếu goal có status.
* Nhập/chọn progress mới.
* Upload ảnh:

  * chọn file từ máy
  * upload qua backend/storage/API thật
  * lưu URL/file reference vào DB
  * hiển thị ảnh sau khi đăng
* Emoji/icon picker:

  * không chỉ 1 icon cứng
  * click mở danh sách emoji giống Facebook
  * có thể chọn nhiều nếu data model hỗ trợ
* Nút Đăng.

C. Sau khi bấm Đăng
Tạo một update card trong timeline bên dưới.

Update card phải hiển thị:

* avatar thật người update
* tên/email người update
* thời gian update
* số người đã xem nếu hệ thống có tracking, nếu chưa có thì ẩn hoặc empty, không bịa
* nội dung update
* ảnh đính kèm nếu có
* dòng thay đổi trạng thái:
  “Đã thay đổi trạng thái: [trạng thái cũ] → [trạng thái mới]”
* dòng thay đổi progress nếu có:
  “[progress cũ]% → [progress mới]%”
* nút Sửa
* nút Xóa
* 4 reaction nhanh:

  * Like
  * Vỗ tay
  * Pháo bông
  * Tim
* nút mở emoji đầy đủ.

D. Reaction

* Click reaction phải gọi API và lưu DB.
* Click lại thì toggle hoặc update theo convention.
* Hiển thị count reaction thật.
* Không chỉ đổi local state.

E. Comment dưới update

* Mỗi update có ô comment.
* Có nút Gửi.
* Comment lưu DB thật theo updateId.
* Hiển thị avatar/tên/email/thời gian.
* F5 vẫn giữ.

F. Đồng bộ

* Latest update lấy update mới nhất thật.
* Header/sidebar goal đồng bộ status/progress mới.
* Audit log ghi nhận nếu status/progress đổi.
* Notification nếu người liên quan cần nhận.

============================================================
IX. GOAL DETAIL — TAB SPRINTA
=============================

Đây là tab kết nối goal với dự án trong Space.

UI:

* Có nút “Thêm dự án”.
* Click nút → xổ dropdown danh sách Project thật.
* Có ô dán link phía trên để liên kết nhanh.

Logic:

* Project list lấy từ DB/API theo site/space hiện tại.
* Chỉ hiện project chưa link.
* Chọn project → lưu Goal–Project link DB.
* Dán link:

  * backend parse link
  * validate project có tồn tại
  * validate project thuộc site/space hiện tại
  * validate quyền user
  * link sai thì báo lỗi rõ
* F5 vẫn giữ link.

============================================================
X. GOAL DETAIL — TAB PROJECT
============================

Đây là project của Site, không phải dữ liệu ảo.

UI/logic:

* Nút thêm mở popup/dropdown Project thật của site.
* Chọn project → lưu Goal–Project link.
* Không cho chọn trùng.
* Hiển thị danh sách project đã link.
* Có remove/unlink nếu UI có action.
* F5 vẫn giữ.
* Không dùng availableSpaceProjects/availableSiteProjects giả.

============================================================
XI. GOAL DETAIL — BÀI HỌC / RỦI RO / QUYẾT ĐỊNH
===============================================

Ba tab này dùng cùng logic.

UI:

* Nút “Thêm bài học” / “Thêm rủi ro” / “Thêm quyết định”.
* Click mở RichTextEditor.
* Editor dùng chung bộ công cụ với tab cập nhật:

  * heading
  * bold
  * italic
  * color nếu editor hỗ trợ
  * tag/mention nếu editor hỗ trợ
  * link nếu editor hỗ trợ
* Có nút Save.

Logic:

* Save gọi API thật.
* Lưu DB thật.
* Hiển thị danh sách item trong tab.
* Mới nhất nằm trên.
* Có sửa/xóa nếu có quyền.
* F5 vẫn còn.
* Không để rich text chỉ là HTML local.

Endpoint có thể dùng chung dạng:

* GET goal notes by type
* POST goal note type lesson/risk/decision
* PUT
* DELETE

Hoặc dùng entity/API hiện có nếu source đã có GoalLesson/GoalRisk/GoalDecision.

============================================================
XII. PROJECT LIST
=================

Trang Project của Site có 3 tab:

1. Danh sách chính / Active

* Hiển thị Project thật trong site.
* Có search/filter thật.
* Không mock.

2. Đang theo dõi / Following

* Chỉ hiển thị project user đang follow.
* Follow/unfollow lưu DB.
* F5 vẫn giữ.

3. Archive

* Chỉ hiển thị project đã archive.
* Archive/unarchive lưu DB.
* Project archive không lẫn vào active nếu UI tách riêng.
* Có search/filter tương tự.

============================================================
XIII. PROJECT DETAIL — TAB GIỚI THIỆU
=====================================

Ảnh cho thấy tab giới thiệu có 3 placeholder/khu nhập nội dung. Chức năng chính có thể đã hoạt động, nhưng comment chưa lưu được.

Yêu cầu:

* Giữ layout 3 khu nếu đúng thiết kế hiện tại.
* Không phá chức năng đang hoạt động.
* Hoàn thiện comment giống Goal:

  * ô nhập
  * nút Gửi
  * lưu DB thật
  * avatar/tên/email/thời gian thật
  * mới nhất ở trên
  * sửa màu xanh
  * xóa màu đỏ
  * tag email nếu dùng chung được
  * F5 vẫn giữ
* Không tạo comment table mới nếu Comment entity mở rộng được với ProjectId/entityType.

============================================================
XIV. PROJECT DETAIL — TAB CẬP NHẬT
==================================

Ảnh cho thấy Project update UI đẹp hơn Goal nhưng logic còn sơ sài. Cần làm logic giống Goal để cập nhật tiến độ/status.

Form update project:

* RichTextEditor nhập nội dung.
* Chọn status mới.
* Chọn/nhập progress mới.
* Upload ảnh nếu Goal update đã có.
* Nút Đăng.

Update card/report:

* avatar thật người update
* tên/email
* thời gian
* status mới
* ngày/mốc thời gian nếu có
* nội dung update
* dòng thay đổi:

  * status cũ → status mới
  * progress cũ → progress mới
* reaction nhanh:

  * Like
  * Vỗ tay
  * Pháo bông
  * Tim
* emoji picker đầy đủ
* comment dưới update có nút Gửi
* sửa/xóa update nếu có quyền

Logic:

* Lưu DB thật.
* Header/sidebar project đồng bộ status/progress.
* Latest update đúng.
* F5 vẫn giữ.

Nếu source chưa có ProjectUpdate/ProjectLesson/ProjectRisk/ProjectDecision:

* Kiểm tra có model dùng chung không.
* Nếu không có, tạo entity riêng theo convention hiện có.
* Thêm DbSet.
* Tạo migration additive.
* Không tạo bảng trùng comment.

============================================================
XV. PROJECT DETAIL — BÀI HỌC / RỦI RO / QUYẾT ĐỊNH
==================================================

Giống Goal.

Mỗi tab:

* Nút thêm mở RichTextEditor.
* Save lưu DB thật.
* Hiển thị danh sách.
* Mới nhất trên.
* Có sửa/xóa nếu phù hợp.
* F5 vẫn giữ.
* Không mock.

============================================================
XVI. SIDEBAR / AVATAR / UI ĐỒNG BỘ
==================================

Ảnh cuối cho thấy sidebar Project/Goal/Team đang mỗi nơi một kiểu. Cần đồng bộ.

Yêu cầu sidebar Team/Goal/Project:

* Cùng spacing.
* Cùng cách căn lề.
* Cùng style label/value.
* Cùng style avatar.
* Cùng style nút dấu cộng.
* Cùng style dropdown/popup chọn user/team/project/goal.
* Không phá logic backend.

Các section thường gặp:

* Chủ sở hữu
* Người đóng góp
* Người theo dõi
* Dự án liên quan
* Goal liên quan
* Team liên quan
* Ngày bắt đầu
* Ngày kết thúc
* Manager

Avatar:

* Luôn lấy avatar thật từ profile user nếu có.
* Nếu không có avatar thì fallback chữ cái.
* Không được hiển thị sai “ND” nếu user/owner có avatar thật.
* Avatar trong owner, manager, comment, update, sidebar phải đồng bộ cùng nguồn dữ liệu.

Global UI:

* Đồng bộ card.
* Đồng bộ list/table.
* Đồng bộ empty state.
* Đồng bộ button primary/secondary/danger.
* Đồng bộ dropdown/popup.
* Đồng bộ tab.
* Không copy CSS lung tung nếu đã có design variables/component chung.
* Không phá responsive.

============================================================
XVII. NOTIFICATIONS / STATUS
============================

Notifications:

* Trang notification phải lấy DB thật.
* Có read/unread.
* Có mark read.
* Có mark all read nếu UI cần.
* Click notification điều hướng đúng entity.
* Tạo notification khi phù hợp:

  * user được tag trong comment
  * user được thêm vào team/project/goal
  * user được chọn làm manager/owner
  * có update mới trong goal/project user follow
  * có reply/reaction vào comment/update của user

Status:

* Status lấy từ enum/database/API thật.
* Không hard-code frontend.
* Khi đổi status/progress Goal/Project:

  * header đồng bộ
  * sidebar đồng bộ
  * latest update đồng bộ
  * audit log ghi nhận
  * notification nếu phù hợp

============================================================
XVIII. THỨ TỰ CODE ĐỀ XUẤT
==========================

Không làm toàn bộ bằng sửa hàng loạt. Làm theo vertical slice end-to-end:

1. Goal overview comment end-to-end.
2. Goal update status/progress end-to-end.
3. Goal update reaction/comment end-to-end.
4. Goal SprintA/Project link end-to-end.
5. Goal lesson/risk/decision end-to-end.
6. Project overview comment end-to-end.
7. Project update end-to-end.
8. Project lesson/risk/decision end-to-end.
9. Project list following/archive.
10. Sidebar/avatar đồng bộ.
11. Notifications/status.

Mỗi slice xong phải tự build và tự F5 test trước khi chuyển slice tiếp theo. Không báo cáo giữa chừng trừ khi build fail không tự sửa được.

============================================================
XIX. DEFINITION OF DONE CHUNG
=============================

Một chức năng chỉ được coi là hoàn thành khi:

* UI có thao tác thật.
* Store gọi API thật.
* Backend lưu database thật.
* Scope đúng workspace/site.
* Không mock/hard-code.
* Không nút giả.
* Không disabled để che thiếu backend.
* Reload/F5 vẫn giữ dữ liệu.
* Build frontend thành công.
* Build backend thành công nếu có sửa backend.
* Không có migration bị xóa nhầm.
* Không có component template lỗi.
* Không có báo cáo “xong” nếu chưa test.

Bắt đầu bằng slice nhỏ nhất còn thiếu: Goal overview comment end-to-end.
Không làm Project/Notification trước khi Goal comment thật sự lưu DB và F5 còn.
Không báo cáo giữa chừng.
Code xong slice, build xanh, F5 test xong rồi mới chuyển slice tiếp theo.
