# SPRINTA_MASTER_SPECIFICATION
## Phần 2 — Tích hợp: Teams · Goals · Projects · People · Kudos
**Version:** 1.0 | **Nguồn:** PDF "Tổng hợp chức năng tích hợp" | **Phong cách UI:** Jira Cloud Modern

---

## A. TỔNG QUAN HỆ THỐNG

### Mục tiêu sản phẩm
Trang Home Site là nền tảng quản lý **tổ chức** độc lập, không liên quan đến space/project riêng lẻ. Trang quản lý site-level: Teams (đội ngũ), Goals (mục tiêu), Projects (dự án cấp cao), và People (thành viên). Các thực thể này **liên kết** với các dự án trong space project thông qua cơ chế link.

### Module chính
| Module | Sidebar Label | Icon | Mục đích |
|---|---|---|---|
| Home | Home | 🏠 | Trang tổng quan cá nhân |
| Teams | Teams | 👥 | Quản lý đội ngũ site-level |
| Goals | Goals | 🎯 | Quản lý mục tiêu |
| Projects | Projects | 📁 | Quản lý dự án cấp cao |

### Điều hướng chính (Left Sidebar — Teams module)
```
Dành cho bạn
Nhóm
Mọi người
Khen ngợi
Goals
Projects
```

### Quan hệ thực thể
```
Site
 ├── Teams (Đội ngũ)
 │    ├── Members (Thành viên)
 │    ├── Goals (Mục tiêu liên kết)
 │    ├── Projects (Dự án liên kết)
 │    ├── Linked Resources (Jira project links)
 │    ├── Parent Team (Đội ngũ gốc)
 │    └── Sub Teams (Đội ngũ phụ)
 ├── Goals (Mục tiêu)
 │    ├── Key Results (Kết quả chính)
 │    ├── Sub-goals (Mục tiêu phụ)
 │    ├── Jira Tasks (liên kết qua link)
 │    ├── Projects (Dự án liên kết)
 │    └── Followers (Người theo dõi)
 ├── Projects (Dự án)
 │    ├── Contributors (Người đóng góp)
 │    ├── Goals (Mục tiêu đóng góp)
 │    ├── Related Projects
 │    ├── Jira Board Link (1 link)
 │    └── Tasks (liên kết qua link)
 └── People (Thành viên)
      ├── Teams membership
      ├── Goals (owned / following)
      ├── Projects (owned / following)
      └── Kudos (received / given)
```

---

## B. DANH SÁCH TẤT CẢ MÀN HÌNH

| # | Tên màn hình | Module | Loại | URL pattern |
|---|---|---|---|---|
| S01 | Home Site | Home | Dashboard | /home |
| S02 | Dành cho bạn | Teams | Dashboard/List | /teams/for-you |
| S03 | Nhóm — Tab Tất cả đội ngũ | Teams | List | /teams/all |
| S04 | Nhóm — Tab Nhóm của bạn | Teams | List (filtered) | /teams/my-teams |
| S05 | Nhóm — Tab Lưu trữ nhóm | Teams | List (archived) | /teams/archived |
| S06 | Chi tiết đội ngũ — Tab Giới thiệu | Teams | Detail/Tab | /teams/:id/overview |
| S07 | Chi tiết đội ngũ — Tab Hoạt động | Teams | Detail/Tab | /teams/:id/activity |
| S08 | Chi tiết đội ngũ — Tab Phân cấp | Teams | Detail/Tab | /teams/:id/hierarchy |
| S09 | Chi tiết đội ngũ — Tab Mục tiêu | Teams | Detail/Tab | /teams/:id/goals |
| S10 | Chi tiết đội ngũ — Tab Dự án | Teams | Detail/Tab | /teams/:id/projects |
| S11 | Chi tiết đội ngũ — Tab Khen ngợi | Teams | Detail/Tab | /teams/:id/kudos |
| S12 | Khen ngợi (site-level) | Teams | List | /teams/kudos |
| S13 | Mọi người | People | List | /people |
| S14 | Profile — Tab Tổng quan | People | Detail/Tab | /people/:id/overview |
| S15 | Profile — Tab Mục tiêu | People | Detail/Tab | /people/:id/goals |
| S16 | Profile — Tab Dự án | People | Detail/Tab | /people/:id/projects |
| S17 | Profile — Tab Khen ngợi | People | Detail/Tab | /people/:id/kudos |
| S18 | Goals — Thư mục mục tiêu | Goals | List | /goals/all |
| S19 | Goals — Đang theo dõi | Goals | List (filtered) | /goals/following |
| S20 | Goals — Cập nhật trạng thái | Goals | Dashboard | /goals/status-updates |
| S21 | Goals — Đã lưu trữ | Goals | List (archived) | /goals/archived |
| S22 | Goal Detail — Tab Tổng quan | Goals | Detail/Tab | /goals/:id/overview |
| S23 | Goal Detail — Tab Cập nhật | Goals | Detail/Tab | /goals/:id/updates |
| S24 | Goal Detail — Tab Jira | Goals | Detail/Tab | /goals/:id/jira |
| S25 | Goal Detail — Tab Dự án | Goals | Detail/Tab | /goals/:id/projects |
| S26 | Goal Detail — Tab Bài học rút ra | Goals | Detail/Tab | /goals/:id/learnings |
| S27 | Goal Detail — Tab Rủi ro | Goals | Detail/Tab | /goals/:id/risks |
| S28 | Goal Detail — Tab Quyết định | Goals | Detail/Tab | /goals/:id/decisions |
| S29 | Projects — Thư mục dự án | Projects | List | /projects/all |
| S30 | Projects — Đang theo dõi | Projects | List (filtered) | /projects/following |
| S31 | Projects — Cập nhật trạng thái | Projects | Dashboard | /projects/status-updates |
| S32 | Projects — Đã lưu trữ | Projects | List (archived) | /projects/archived |
| S33 | Project Detail — Tab Giới thiệu | Projects | Detail/Tab | /projects/:id/overview |
| S34 | Project Detail — Tab Cập nhật | Projects | Detail/Tab | /projects/:id/updates |
| S35 | Project Detail — Tab Bài học rút ra | Projects | Detail/Tab | /projects/:id/learnings |
| S36 | Project Detail — Tab Rủi ro | Projects | Detail/Tab | /projects/:id/risks |
| S37 | Project Detail — Tab Quyết định | Projects | Detail/Tab | /projects/:id/decisions |

**Popups / Modals / Drawers:**
| # | Tên | Trigger | Loại |
|---|---|---|---|
| P01 | Tạo đội ngũ | Nút "Tạo đội ngũ" | Modal |
| P02 | Cài đặt nhóm | ... → Cài đặt nhóm | Modal/Drawer |
| P03 | Thêm thành viên | Nút "Thêm người" | Popup |
| P04 | Xóa đội ngũ | ... → Xóa nhóm | Confirm Dialog |
| P05 | Chọn/Sửa đội ngũ cha | Tab Phân cấp → Add parent | Popup |
| P06 | Chọn đội ngũ con | Tab Phân cấp → Add sub-teams | Popup (multi-select) |
| P07 | Liên kết dự án Jira | Liên kết đội ngũ → icon SprintA | Popup |
| P08 | Thêm web link | Liên kết đội ngũ → Thêm liên kết | Popup form |
| P09 | Ngắt liên kết | Xóa liên kết dự án | Confirm Dialog |
| P10 | Give Kudos | Tab Khen ngợi → Give kudos | Full page (sub-view) |
| P11 | Liên kết link trong Kudos | Icon link trong Give Kudos | Popup |
| P12 | Thêm mục tiêu vào team | Tab Mục tiêu → + | Popup |
| P13 | Tạo mục tiêu nhanh | + Tạo mục tiêu (from popup) | Popup form |
| P14 | Thêm chi tiết vai trò Profile | + Thêm chi tiết và vai trò | Modal |
| P15 | Tạo Goal | Nút "Tạo mục tiêu" | Modal |
| P16 | Chọn trạng thái Goal | Nút badge trạng thái | Popup |
| P17 | Chọn ngày Goal | Ngày mục tiêu | Date picker popup |
| P18 | Thêm mục tiêu Jira task | Tab Jira → Thêm hạng mục | Input popup |
| P19 | Hover task preview | Hover task card | Popover |
| P20 | Thêm dự án cho Project | Tab Dự án Goal | Popup |
| P21 | Thêm người đóng góp Project | + Thêm người đóng góp | Popup |
| P22 | Chọn dự án liên quan Project | Các dự án liên quan → + | Popup search |
| P23 | Chỉnh sửa công việc theo dõi | Công việc được theo dõi | Popup |
| P24 | Tạo Project mới | Tạo dự án | Modal |

---

## C. ĐẶC TẢ CHI TIẾT TỪNG MÀN HÌNH

---

### S01 — Home Site (Trang chủ)

**Mục đích:** Dashboard cá nhân của user trên site. Hiển thị ứng dụng, nội dung gần đây, và bước tiếp theo.

**Layout blueprint:**
```
┌─ Sidebar (240px) ─┬─────────── Content Area (flex) ────────────┬─ Topbar ─┐
│ Logo + App name   │  Greeting header                           │ Search   │
│ ─────────────── │  "Thứ Ba, 8 tháng 6 / Xin chào Tua20000"   │ + Tạo    │
│ Dành cho bạn     │  ─────────────────────────────────────────  │ Icons    │
│ Có gần sao       │  [Announcement banner]                      │ Avatar   │
│ Thông báo        │  ─────────────────────────────────────────  │          │
│ Cập nhật TT      │  Ứng dụng của bạn (4 app cards)            │          │
│ # Thẻ            │  ─────────────────────────────────────────  │          │
│ ─────────────── │  Thường xuyên truy cập (1-N items)          │          │
│ 🔴 Jira          │  ─────────────────────────────────────────  │          │
│ 👥 Teams         │  Tiếp theo là gì (list, tab Làm việc/Đã xem)│          │
│ 🎯 Goals         │                                             │          │
│ 📁 Projects      │                                             │          │
└──────────────────┴─────────────────────────────────────────────┘
```

**Components:**
- **Topbar:** Search global (center), + Tạo button (primary), icon buttons (notifications, settings), avatar menu
- **Left sidebar:** Collapsed/expanded toggle (top-left icon), nav items with active state highlight (blue left border)
- **Greeting block:** Date string + user display name + avatar; "Đọc thông tin cập nhật..." banner (dismissible); "Hoạt động nổi bật..." info link
- **App cards grid (2×2):** Icon + App name + site-id. "Xem tất cả ứng dụng →" link
- **Recent items list:** Item icon + name + subtitle (path). Max 3 visible
- **Tiếp theo là gì:** Tab: "Làm việc trên" / "Đã xem". Each item: page icon + title + path + timestamp

**Điều hướng:**
- Click "Duyệt xem tất cả đội ngũ" → S03
- Click "Duyệt xem tất cả mọi người" → S13
- Click Teams sidebar → S02
- Click Goals sidebar → S18
- Click Projects sidebar → S29
- Star team trong Có gần sao → team bị starred xuất hiện ở sidebar Có gần sao

---

### S02 — Dành cho bạn (For You)

**Module:** Teams | **Loại:** Dashboard
**Entry points:** Sidebar → Dành cho bạn

**Layout blueprint:**
```
┌─ Sidebar ─┬──────────── Content (scroll) ────────────────────┐
│ [Teams]   │  Những người bạn làm việc cùng    [Duyệt tất cả →]│
│ > Dành    │  [Avatar] [Avatar] [Avatar]                       │
│   cho bạn │                                                   │
│   Nhóm    │  Nhóm của bạn                    [Duyệt tất cả →]│
│   Mọi ng  │  [Team card] [Team card] [Team card]              │
│   Khen ng │                                                   │
│   Goals   │  Lời khen ngợi gần đây                           │
│   Projects│  [Kudos card]                                     │
└───────────┴───────────────────────────────────────────────────┘
```

**Components:**

**Section 1 — Người làm việc cùng:**
- Row of avatar circles (max ~3 visible)
- Each avatar: circle (40px), initials/photo, hover → name tooltip
- Subtitle (chức danh/username) below name
- Link "Duyệt xem tất cả mọi người →" → S13

**Section 2 — Nhóm của bạn:**
- Row of team cards (max ~3 visible)
- Each card: Team icon (48px square, colored background + initials), team name, badges (notification dot, star icon)
- Subtitle: "Đội ngũ chính thức ✅ • 1 thành viên"
- Link "Duyệt tất cả đội ngũ →" → S03
- Click team card → S06

**Section 3 — Lời khen gần đây:**
- Kudos item: sender avatar + name + team tag + GIF illustration + text + emoji reactions
- Empty state if no kudos

---

### S03 — Nhóm: Tab "Tất cả đội ngũ"

**Module:** Teams | **Loại:** List
**Entry points:** Sidebar → Nhóm

**Layout blueprint:**
```
┌─ Sidebar ─┬────────────────── Content ───────────────────────────────────┐
│           │  h1: Nhóm                            [Tạo đội ngũ] (primary) │
│           │  Tabs: [Tất cả đội ngũ] [Nhóm của bạn] [Đã lưu trữ]         │
│           │  ┌─────────────────────────────────────────────────────────┐ │
│           │  │ 🔍 Tìm kiếm đội ngũ                                    │ │
│           │  └─────────────────────────────────────────────────────────┘ │
│           │  Filter bar: [Lọc theo Dự án] [Mục tiêu] [Loại đội ngũ]      │
│           │              [Có gần sao] [Đã xác minh] [Thành viên đội]    │
│           │              [Đặt lại]                                       │
│           │  "3 đội ngũ"                    [⊞ ≡ ···] (view toggle)      │
│           │  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐         │
│           │  │ Team card    │ │ Team card    │ │ Team card    │         │
│           │  └──────────────┘ └──────────────┘ └──────────────┘         │
└───────────┴───────────────────────────────────────────────────────────────┘
```

**Components:**

**Nút "Tạo đội ngũ" (top-right):**
- Primary button
- Click → mở P01 (Modal Tạo đội ngũ)

**Tabs:**
- 3 tabs: "Tất cả đội ngũ" (active) / "Nhóm của bạn" / "Đã lưu trữ"
- Underline style, active = blue underline

**Search bar:**
- Placeholder: "Tìm kiếm đội ngũ"
- Full-width text input
- Live filter kết quả

**Filter chips (dạng chip/button nhỏ):**
| Chip | Hành vi |
|---|---|
| Lọc theo Dự án | Dropdown chọn dự án |
| Mục tiêu | Dropdown chọn goal |
| Loại đội ngũ | Dropdown: Đội ngũ chính thức / Nhóm |
| Có gần sao | Toggle — lọc starred teams |
| Đã xác minh | Toggle — lọc verified teams |
| Thành viên đội | Toggle — lọc teams có mình |
| Đặt lại | Clear all filters |

**Bộ đếm + View toggle:**
- "N đội ngũ" (count label)
- ⊞ Grid view / ≡ List view / ··· more options

**Team card (Grid view):**
- Icon background: colored square (48px), team initials
- Team name (bold)
- Subtitle: "Đội ngũ chính thức ✅ • N thành viên" hoặc "Nhóm • N thành viên"
- Badges: notification dot (blue circle), star (⭐ nếu starred)
- Click → S06 (Team detail)

**Empty state (khi filter không có kết quả):**
- Illustration (X icon centered)
- Text: "Chúng tôi không tìm được đội ngũ nào phù hợp với nội dung tìm kiếm của bạn."
- Sub-text: "Hãy thử thay đổi tiêu chí tìm kiếm hoặc xóa bộ lọc."
- Link: "Xóa tất cả bộ lọc"
- Footer note: "Do khả năng hiển thị của đội ngũ thay đổi..."

---

### S04 — Nhóm: Tab "Nhóm của bạn"

**Giống S03 nhưng:**
- Active filter mặc định: "Thành viên đội là [username] ×" (chip có thể xóa)
- Nút "Thêm bộ lọc +" và "Đặt lại"
- Chỉ show teams có current user là member

---

### S05 — Nhóm: Tab "Đã lưu trữ"

**Giống S03 nhưng:**
- Không có nút Tạo đội ngũ
- Team cards bị faded/grayscale
- Không thể chỉnh sửa
- Team card có badge "ARCHIVED/[date]" màu gray
- Có nút "Khôi phục lại đội ngũ" trên team detail (khi vào S06 của archived team)
- Empty state nếu không có archived team

---

### S06 — Chi tiết đội ngũ — Tab Giới thiệu

**Module:** Teams | **Loại:** Detail page
**Entry points:** Click team card từ S03/S04/S05

**Layout blueprint:**
```
┌─ Sidebar ─┬──────────────────────────────────────────────────────────────┐
│           │ Breadcrumb: Teams / [Parent] / [Team name]                   │
│           │ ┌──────────────── Cover image (full width, ~200px high) ────┐│
│           │ │ [hover → overlay] [Thêm ảnh bìa btn] / [Tải hình ảnh lên]││
│           │ │                     ··· (3 dots top-right of cover)       ││
│           │ └──────────────────────────────────────────────────────────┘│
│           │ [Team icon 48px] [Team name]  [Type badge ✅]               │
│           │ Tabs: Giới thiệu | Hoạt động | Phân cấp | Mục tiêu |        │
│           │       Dự án | Khen ngợi                                     │
│           │ ─────────────────────────────────── [Thêm người] [···]      │
│           │ ┌─────────────────────────┐ ┌──── Right panel (fixed) ────┐ │
│           │ │ Main content area       │ │ Liên kết đội ngũ [N] [+]   │ │
│           │ │ "Việc chúng tôi đang    │ │  • [Jira icon] Project name │ │
│           │ │  thực hiện"             │ │  • [link icon] Thêm liên kết│ │
│           │ │ [click to edit text]    │ │ ─────────────────────────── │ │
│           │ │                         │ │ Chi tiết                    │ │
│           │ │ Thành viên [N] [+]      │ │  Đội ngũ gốc: [Team]       │ │
│           │ │ [Avatar grid 2 cols]    │ │  Đội ngũ phụ: [Team]       │ │
│           │ └─────────────────────────┘ │  Loại đội ngũ: [dropdown]  │ │
│           │                             └─────────────────────────────┘ │
└───────────┴──────────────────────────────────────────────────────────────┘
```

**Components:**

**Cover image area:**
- Default: gradient/pattern background (full width, ~180-200px height)
- Hover → semi-transparent overlay + button "Thêm ảnh bìa"
- Click "Thêm ảnh bìa" → button "Tải hình ảnh lên" appears
- Click "Tải hình ảnh lên" → native file picker (image files)
- Upload → replaces cover, saves immediately
- ··· button top-right corner of cover (3 dots) → dropdown: Star team / Cài đặt nhóm / Lưu trữ đội ngũ / Xóa nhóm (P02/P04)

**Team header:**
- Team icon: 48×48px colored square + initials
- Team name (h2, editable by admin)
- Type badge: "Đội ngũ chính thức ✅" hoặc "Nhóm"

**Tabs bar:**
6 tabs: Giới thiệu (active) | Hoạt động | Phân cấp | Mục tiêu | Dự án | Khen ngợi

**Action buttons (top-right, above tabs):**
- "Thêm người" → P03
- "···" → dropdown: Star team ⭐ / Cài đặt nhóm ⚙️ / Lưu trữ đội ngũ 📦 / Xóa nhóm 🗑️

**Main content — "Việc chúng tôi đang thực hiện":**
- Label text: "Chia sẻ những gì nhóm bạn đang thực hiện"
- Click text area → inline rich text editor activates
- Plain text only (no format toolbar shown in PDF)
- Auto-save on blur

**Thành viên section:**
- Header: "Thành viên [N]" + "+" button (→ P03)
- Member grid: 2 columns, each cell = [Avatar 40px] [Display name]
- Avatar shapes: colored circle with initials or photo
- Click member → S14 (Profile)

**Right panel — Liên kết đội ngũ:**
- Header: "Liên kết đội ngũ [count badge]" + "+" button
- Click "+" → dropdown: "Thêm dự án Jira" (→ P07) / "Thêm liên kết" (→ P08)
- Each linked item: [app icon] [Project name] + "×" button on hover (→ P09 confirm)
- "Thêm liên kết" text link at bottom

**Right panel — Chi tiết:**
- "Đội ngũ gốc": [Team chip] hoặc "Không có đội ngũ gốc"
- "Đội ngũ phụ": [Team chip(s)] hoặc "Không có đội ngũ phụ"
- "Loại đội ngũ": dropdown Đội ngũ chính thức / Nhóm

**Archived team variant:**
- Cover + header faded (grayscale)
- Banner: "Đội ngũ này đã bị lưu trữ. Khôi phục đội ngũ để thực hiện thay đổi"
- Button: "Khôi phục lại đội ngũ"
- All edit actions disabled

**Jira style notes:**
- Cover area: similar to Confluence page header image
- Tabs: horizontal underline tabs, no background
- Right panel: sticky, ~300px width, separated by subtle border

---

### S07 — Chi tiết đội ngũ — Tab Hoạt động

**Hiển thị 2 khu vực:**

**Section 1 — Công việc của wq (team name):**
- Tiêu đề: "Công việc của [team name]"
- Danh sách các work items từ **tất cả** space projects được liên kết với team
- Mỗi item: [type icon] [title] + [project path] + [time ago] + [avatar]
- Infinite scroll hoặc "Tải thêm" button
- Empty state: illustration + "Hàng mục chưa được chỉ định" text

**Section 2 — Hạng mục công việc Jira được chỉ định cho đội ngũ:**
- Tasks/bugs được project owner assign trực tiếp cho team
- Cùng format với section 1

---

### S08 — Chi tiết đội ngũ — Tab Phân cấp

**Mục đích:** Visualize team hierarchy (cha-con relationship)

**Layout:**
```
                    [Tree node: current team]
                           |
           ┌───────────────┴──────────────┐
    [Parent team node]              [Sub-team node(s)]
```

**Components:**

**Tree visualization (center):**
- Current team node: icon + name + member count + badge
- Parent team box (above, if exists): icon + name + type + member count + edit (✏️) icon
- Sub-team box(es) (below, if exist): same format
- Lines connecting nodes
- "+ Add parent team" text/button (if no parent)
- "+ Add sub-teams" text/button (if no sub-teams)

**Khi không có parent/sub-teams:**
- Empty state text: "Visualize your team's reporting structure"
- Sub-text: "Add a parent team and sub-teams to see where your team sits in the organization"

**Khi có 1 sub-team:** single node below current
**Khi có 2+ sub-teams:** nodes branch out to 2 sides (split layout)

**Edit parent team (✏️ icon on parent node):**
- Opens P05 (popup)
- Popup contents: search field "Tìm kiếm đội ngũ", "Xóa đội ngũ gốc" option, list of teams (single select), "+ Tạo đội ngũ" link at bottom
- Select team → parent node updates

**Add sub-teams (click "Add sub-teams"):**
- Opens P06 (popup with checkboxes)
- Multi-select: each item has checkbox + team icon + name + member count
- "+ Tạo đội ngũ" link at bottom
- Confirm → sub-team nodes added below

**Sub-team count badge:** blue circle badge on current node showing number of sub-teams

---

### S09 — Chi tiết đội ngũ — Tab Mục tiêu

**Layout:**
```
Main: "Đang đóng góp cho"     Right panel: (same as S06)
Trạng thái badge: ĐÃ HOÀN TẤT
[Goal title]
"Thuộc sở hữu của [User]"
─────────────────────────────
"Đã hoàn tất"
(empty row if none)
                    [Theo dõi +] button top-right
```

**Components:**

**Goal list — "Đang đóng góp cho":**
- Each goal item: [Goal icon] [status badge] [Goal title] [owner]
- Status badge variants: ĐÃ HOÀN TẤT (green) / ĐANG TIẾN ĐỘ / ĐANG CHỜ XỬ LÝ / etc.

**"Đã hoàn tất" section:**
- Same format, goals with Done status

**"Theo dõi +" button (top-right):**
- Click → opens small popup (P12): search bar "Tìm kiếm mục tiêu hoặc dán liên kết", list of recent goals, "+ Tạo mục tiêu" link
- Select existing goal → adds to team goal list
- Click "+ Tạo mục tiêu" → replace popup với P13 (Create Goal form)

**Empty state:** Icon + "Đang đóng góp cho / Xem các mục tiêu mà đội ngũ của bạn đang hướng tới..." + "Thêm mục tiêu …" link

---

### S10 — Chi tiết đội ngũ — Tab Dự án

**Khác goals:** không tạo project từ đây.

**Content:**
- Text: "Chỉ định cho đội ngũ của bạn các dự án mà họ đang thực hiện. Mục Dự án có thể giúp đội ngũ của bạn cung cấp bối cảnh các dự án phù hợp để xem tất cả cá dự án được liệt kê ở đây."
- "+ Tạo dự án" link → navigate to Projects home page (không tạo inline)
- Sau khi link: hiển thị list dự án tương tự Goals tab format

---

### S11 — Chi tiết đội ngũ — Tab Khen ngợi

**Content khi trống:**
- Large empty state card:
  - Star + ribbon icon (decorative)
  - Title: "Lời khen của nhóm bạn sẽ xuất hiện tại đây"
  - Sub-text: "Khen ngợi là một cách bày tỏ lời cảm ơn và chúc mừng đồng nghiệp khi họ có thành tích vượt trội."
  - Button: "♡ Give kudos" → P10

**Content khi có kudos:**
- List of kudos items (same format as S12)

---

### S12 — Trang Khen ngợi (Site-level)

**Module:** Teams | **Loại:** List
**Entry points:** Sidebar → Khen ngợi

**Layout:**
```
h1: Khen ngợi              [Khen ngợi] button (top-right)
─────────────────────────────────────────────────────────
[Kudos card] [Kudos card] [Kudos card]  (grid, ~3 per row)
```

**Kudos card structure:**
```
[Sender avatar] [Sender name]   (top-left)
[Team icon] [Team name]         (top-right, recipient team)
─────────────────────────────────
[GIF / illustration - animated]  (~200px tall)
─────────────────────────────────
[Kudos message text]
─────────────────────────────────
Reactions bar: [🤝1] [👍1] [❤️1] [GIF icon] [···]
```

**Emoji reactions:**
- User có thể thả tối đa 3 loại emoji khác nhau
- Mỗi loại chỉ được thả 1 lần
- Click lại emoji đã thả → remove
- Click emoji reaction → expand picker showing more options
- Reaction picker: [🤝] [😊] [👍] [❤️] [🎉] [😮] [···]

**Nút "Khen ngợi" (top-right):**
- Click → navigate to P10 give kudos view

**Jira style notes:** Card grid 3 columns, consistent spacing ~16px gap

---

### P10 — Give Kudos (Sub-view/page)

**Mở từ:** Nút "Give kudos" trên S11 hoặc S12
**Layout:** Chiếm content area (không phải modal riêng)

```
← [back arrow]    🏅 Khen ngợi [Team name]     [Khen ngợi] chip (active tab)
─────────────────────────────────────────────────────────────────
"Hãy cho [team name] biết lý do bạn gửi lời khen ngợi này"
─────────────────────────────────────────────────────────────────
[Icon selector: ☺ 🔗]
[GIF display area - large, ~300px]      [Chọn GIF panel]
                                         ┌───────────────────┐
                                         │ Chọn GIF          │
                                         │ Tìm kiếm Giphy [Q]│
                                         │ [gif] [gif] [gif] │
                                         │ [gif] [gif] [gif] │
                                         └───────────────────┘
[Cá nhân hóa] chip (toggle emoji picker side)
─────────────────────────────────────────────────────────────────
[Text input area: "Lời nhắn..."]        (multiline)
─────────────────────────────────────────────────────────────────
[Hủy]  [Khen ngợi] (primary button)
```

**Icon selector:**
- ☺ → emoji/GIF picker
- 🔗 → P11 (link picker)

**GIF search (Giphy integration):**
- Search field "Tìm kiếm Giphy"
- Grid of GIFs (~3 wide)
- Click GIF → replaces display area

**Link picker (P11):**
- Popup: "Tìm kiếm hoặc dán liên kết *" (required)
- "Văn bản hiển thị (không bắt buộc)"
- Tabs: Home | Jira
- "Đã xem gần đây" list with item icons
- [Hủy] [Chèn] buttons

**Sau khi submit:**
- Kudos appears in team's Khen ngợi tab (S11) ngay lập tức
- Kudos appears on S12
- Không hiện ngay trong danh sách team card list view

---

### P01 — Modal Tạo đội ngũ

**Trigger:** Nút "Tạo đội ngũ"
**Kích thước:** ~480px wide, centered modal with overlay

```
┌──────────────────────────────── Tạo đội ngũ ✕ ─────────────────────────────┐
│ "Các trường bắt buộc được đánh dấu đầu bằng sao*"                          │
│                                                                              │
│ Tên *                                                                        │
│ [___________________________________________________]                        │
│                                                                              │
│ Thêm thành viên đầu *                                                       │
│ [Tua20000 ×] [___________________________]  (tag input)                     │
│                                                                              │
│ Loại *                                                                       │
│ [Đội ngũ chính thức ✅ ▼]                                                   │
│  → Đội ngũ chính thức ✅                                                    │
│     Loại mặc định • Đội ngũ do quản trị viên quản lý và được đánh dấu là..│
│  → Nhóm                                                                     │
│     Loại mặc định • Đội ngũ mà bất cứ ai cũng có thể tạo và quản lý        │
│                                                                              │
│                                              [Hủy]  [Tạo] (primary)        │
└──────────────────────────────────────────────────────────────────────────────┘
```

**Fields:**
| Field | Required | Type | Validation |
|---|---|---|---|
| Tên | ✅ | Text input | Non-empty |
| Thêm thành viên đầu | ✅ | Tag input | Min 1 user |
| Loại | ✅ | Select dropdown | Default: Đội ngũ chính thức |

**Behavior:**
- Esc / click overlay → close, no save
- Submit → creates team, redirects to S06

---

### P02 — Cài đặt nhóm (Modal)

**Trigger:** ··· → Cài đặt nhóm
**Kích thước:** ~480px wide

```
┌──────────────── Cài đặt nhóm ✕ ─────────────────────────────┐
│ Màu nền / ảnh nền: [■■■■■■■■]  (8 color swatches)           │
│ Ảnh nền: [input ảnh hoặc color]                              │
│                                                              │
│ Ai có thể thêm thành viên?    [dropdown]                     │
│  - Quản trị viên chỉ          (Đội ngũ chính thức)           │
│  - Tất cả thành viên          (Nhóm)                         │
│                                                              │
│ Ai có thể xóa thành viên?     [dropdown]                     │
│                                                              │
│ Liên kết dự án (gắn project với nhóm để quản lý)            │
│                                                              │
│                               [Thử]  [Lưu] (primary)        │
└──────────────────────────────────────────────────────────────┘
```

---

### P03 — Thêm thành viên (Popup)

**Trigger:** Nút "Thêm người" trên S06

```
┌──── Thêm thành viên vào nhóm ✕ ────────────────────┐
│ "Bạn đang thêm thành viên tạo vào nhóm phức tạp..."  │
│ "...nếu bạn cần thêm nhiều hơn hãy chia thành nhiều  │
│  phần nhỏ hơn." (link)                               │
│                                                       │
│ [Tua20000 ×] [Tìm Thêm Bạn 4.x ×]  (tag input)     │
│                                                       │
│ [Search results dropdown list]                        │
│                                                       │
│                         [Hủy]  [Lưu thêm] (primary) │
└───────────────────────────────────────────────────────┘
```

**Limits:** Tối đa 50 người/lần thêm
**Sau submit:** Thành viên mới xuất hiện trong member grid (2 cột) trên S06

---

### P04 — Xóa nhóm (Confirm Dialog)

**Trigger:** ··· → Xóa nhóm

```
┌──── ⚠️ Xóa đội ngũ "wq"? ✕ ─────────────────────────────────────────┐
│ Tất cả những thứ sau sẽ bị mất và bạn sẽ không thể khôi phục chúng.  │
│                                                                        │
│ Dưới đây là những thiệt hại và ảnh hưởng nếu xóa nhóm:               │
│ • Hàng mục     • Mục tiêu     • Dự án   • Quyền Đặt cấp và quyền    │
│   công việc    • Nội dung     • Liên kết   của cài đặng               │
│   của nhóm     • Thành viên   • Thành phẩm  Hội chuyển đến Hội/tổng  │
│                                              đã                        │
│ Tìm hiểu thêm về việc xóa đội ngũ là vĩnh viễn của nhóm.             │
│                                                                        │
│ ⚠️ Tất cả công việc của đội ngũ này vĩnh cửu và sẽ không thể hoàn lại│
│                                                         [Hủy]  [Xóa đội ngũ] │
└────────────────────────────────────────────────────────────────────────┘
```

---

### P07 — Popup Liên kết dự án Jira

**Trigger:** Click icon SprintA / icon Jira trong "Liên kết đội ngũ"

```
┌──── Liên kết với dự án Jira ────────────────────────┐
│ [🔍 Tìm kiếm hoặc chọn dự án]                        │
│ Tên dự án                                            │
│ ──────────────────────────────────────────────────── │
│ Đã xem gần đây:                                      │
│ 🔴 Jira                                              │
│   Tứ đa (active)                                     │
│ [🔴] Dự Án Tốt Nghiệp                               │
│      Đội ngũ: không có → Jira                        │
│ [🔴] My Software Team                                │
│      Team managed software • 0 thành viên            │
│                                                      │
│ + Tạo dự án Jira ──────────────────────────────────  │
│  (click → navigate to Projects home để tạo mới)     │
└──────────────────────────────────────────────────────┘
```

**Sau khi liên kết:**
- Project icon + tên hiện trong "Liên kết đội ngũ" panel
- Hover item → "×" button để xóa liên kết (P09)
- "+" button để thêm dự án khác

---

### P08 — Thêm Web Link

**Trigger:** "Thêm liên kết" trong Liên kết đội ngũ

```
┌──── Add a web link ✕ ────────────────────────────────┐
│ "Add links to other tools and resources used by the  │
│  team. How can I add my team to project details?"    │
│                                                      │
│ Web URL *                                            │
│ [https://...]                 (required, URL input)  │
│                                                      │
│ Display text                                         │
│ [Tên hiển thị tuỳ chọn]       (optional)             │
│                                                      │
│                        [Thử]  [Lưu] (primary)       │
└──────────────────────────────────────────────────────┘
```

---

### P09 — Ngắt liên kết Confirm Dialog

```
┌──── ⚠️ Ngắt kết nối địa điểm ✕ ─────────────────────┐
│ Đội ngũ này sẽ không được kết nối với dự án Jira     │
│ [Project name] nữa.                                  │
│                                                      │
│ Việc ngắt kết nối đội ngủ dự án sẽ không ảnh hưởng  │
│ đến bất kỳ công việc nào được kết nối với đội ngũ    │
│ trong dự án.                                         │
│                                                      │
│                             [Hủy]  [Xóa] (danger)   │
└──────────────────────────────────────────────────────┘
```

---

### S13 — Mọi người

**Module:** People | **Loại:** List
**Entry points:** Sidebar → Mọi người

**Layout:**
```
h1: Mọi người                    [Thêm người] button (primary)
──────────────────────────────────────────────────────────────
[🔍 Tìm kiếm người]
Filter chips: [Lọc theo Dự án] [Mục tiêu] [Nhóm] [Chức danh]
              [Người quản lý] [Phòng ban] [Vị trí]
──────────────────────────────────────────────────────────────
"3 người"              [⊞ Grid] [≡ List] [⊞⊞ Cột] [···]
──────────────────────────────────────────────────────────────
[Grid view: name cards 2-col] OR [List view with Tên | Job title | Manager cols]
```

**Grid view:** Card = [Avatar 48px] [Name]
**List view:** Table rows: Tên | Job title | Manager
**Column view:** Custom column selector

**Click avatar/name → S14 (Profile)**

---

### S14 — Profile — Tab Tổng quan

**Module:** People | **Loại:** Detail
**Entry points:** Click avatar anywhere on site

**Layout:**
```
┌─ Sidebar ─┬────────────────────────────────────────────────────────────────┐
│           │ Breadcrumb: Mọi người                                          │
│           │ ┌──────── Cover image (full width ~200px) ──────────────────┐  │
│           │ │ [Avatar: large circle ~80px, overlapping bottom of cover] │  │
│           │ └────────────────────────────────────────────────────────────┘  │
│           │ [User display name]             [Cài đặt tài khoản] (if own)   │
│           │ + Thêm chi tiết về và vai trò   (click → P14 if own profile)   │
│           │                                                                 │
│           │ Tabs: Tổng quan | Mục tiêu [N] | Dự án | Khen ngợi            │
│           │ ────────────────────────────────────────────────────           │
│           │ ┌──── Main content ──────┐ ┌── Right panel ─────────────────┐  │
│           │ │ "Làm việc với tôi"     │ │ Chi tiết:                      │  │
│           │ │ [Editable text, own]   │ │  Email: [email]                │  │
│           │ │                        │ │ Đội ngũ [N]:                   │  │
│           │ │ Công việc gần đây [?] │ │  [Team chip] [Team chip]       │  │
│           │ │ [item 1] [time ago]    │ │  [Team chip] [Team chip]       │  │
│           │ │ [item 2] [time ago]    │ │  "Hiển thị thêm N mục khác"   │  │
│           │ │ ... (max 6)            │ │                                │  │
│           │ │ [Hiển thị thêm →]      │ │ Không gian thường xuyên:       │  │
│           │ └────────────────────────┘ │  [Space icon] [Space name] ✎  │  │
│           │                            └─────────────────────────────────┘  │
└───────────┴────────────────────────────────────────────────────────────────┘
```

**"Làm việc với tôi" section (own profile):**
- Click → inline rich text editor (Bold I ··· :link: ☺ ⊞ +)
- Placeholder: "e.g. I prefer async updates in Confluence..."
- Notification bar: "Thông tin cập nhật có thể bảo vệ cố tài khoản của bạn..." with dismiss

**Công việc gần đây:**
- Max 6 work items shown
- Each: [type icon] [title] [project path] [time ago]
- "Hiển thị thêm" → load more

**Right panel — Đội ngũ:**
- Show max 4 teams as colored chips
- "Hiển thị thêm N mục khác" → expand
- Đây là N = number of remaining teams

**Right panel — Không gian thường xuyên:**
- Most accessed Jira space/project
- [icon] [name] + edit icon (own profile)

---

### P14 — Thêm chi tiết và vai trò (Modal)

**Trigger:** Click "+ Thêm chi tiết về và vai trò" trên own profile

```
┌──── Chi tiết về vai trò của bạn ✕ ───────────────────┐
│ Chức danh                                             │
│ [vd: Thực tập phát triển]             (text input)   │
│                                                       │
│ Phòng ban                                             │
│ [vd: Màn hình xy SME, thuộc đồng kinh doanh]         │
│                                                       │
│ Vị trí                                                │
│ [vd: San Francisco, California]                       │
│                                                       │
│ "Bạn có thể thêm thêm cách sắp xếp đầy đủ vai trò  │
│  của bạn trong Cài đặt tài khoản."  [link]           │
│                                                       │
│                         [Hủy]  [Lưu] (primary)       │
└───────────────────────────────────────────────────────┘
```

**Kết quả sau lưu:** Chức danh + Phòng ban + Vị trí hiển thị dưới tên user trên profile

---

### S15 — Profile: Tab Mục tiêu

**Components:**
- Filter row: [Đóng góp] [Đang theo dõi] toggle | [⊞ Grid] [≡ List] view toggle
- Section "Hiện tại": Goals chưa hoàn thành
- Section "Đã hoàn tất": Goals đã Done
- Each goal card: [icon] [name] [status badge] | owner info

**Filter logic:**
- "Đóng góp" = goals created/owned by this user
- "Đang theo dõi" = goals this user follows

---

### S16 — Profile: Tab Dự án

**Giống S15 nhưng cho Projects:**
- Filter: Đóng góp / Đang theo dõi
- Section "Dự án của bạn" (chưa hoàn thành) / "Đã hoàn tất"
- View: List / Thẻ (card grid)

---

### S17 — Profile: Tab Khen ngợi

**Hai sub-tabs:**
- "Đã nhận" (default active)
- "Đã trao"

**Đã nhận:** Kudos cards nhận từ người khác
**Đã trao:** Kudos cards đã gửi cho teams/people

**Logic:** Nếu gửi kudos cho nhóm → tất cả members của nhóm đều nhận trong tab "Đã nhận"

---

### S18 — Goals: Thư mục mục tiêu

**Module:** Goals | **Loại:** List
**Entry points:** Sidebar → Goals

**Layout:**
```
Sidebar Goals:
├── Thư mục mục tiêu (active)
├── Đang theo dõi
├── Cập nhật trạng thái
├── Đã lưu trữ
└── ─ (separator)
   Teams
   Projects

Content:
h1: Mục tiêu                    [Tạo mục tiêu] button
Tabs: [Tất cả mục tiêu +]  (có thể có thêm tab)
──────────────────────────────────────────────────────
[🔍 Tìm kiếm mục tiêu]
Filters: [Lọc theo Thể] [Trạng thái] [Chủ sở hữu] [Nhóm]
         [Đang theo dõi] [Có gần sao] [Chi số] [Tuyển báo cáo]
──────────────────────────────────────────────────────
"N mục tiêu"          [≡≡ Sort] [Sắp xếp theo đang theo dõi ▼] [⊞ Cột ▼] [···]
──────────────────────────────────────────────────────
Table: Tên | Trạng thái | Tiến độ | Ngày mục tiêu | Chủ sở... | Đang theo dõi | Cập nhật lần...
```

**Table row:**
- Expand arrow (▶) for goals with sub-goals
- [icon] [Goal name]
- Status badge (ĐÃ HOÀN TẤT / ĐANG CHỜ XỬ LÝ / etc.)
- Progress bar (thin) + percentage
- Due date chip
- Follower avatars (max 3) + count
- Following button
- Last updated text
- ··· row menu: Edit / Archive / Delete

**Column customizer (⊞ Cột ▼):**
- Dropdown with toggles: Tìm kiếm | Tên | Trạng thái | Tiến độ | Ngày mục tiêu | Chủ sở hữu | Đang theo dõi | Cập nhật lần cuối | Thẻ | Nhóm | Dự án đóng góp | Số người theo dõi
- "+ Tạo trường mới" link at bottom

**Nút "Tạo mục tiêu":**
- Opens P15 (Create Goal modal)

---

### P15 — Tạo mục tiêu (Modal)

```
┌──── 🎯 Tạo mục tiêu ✕ ─────────────────────────────┐
│ "Các trường bắt buộc được đánh dấu đầu bằng sao*"  │
│                                                     │
│ Tên *                                               │
│ [___________________________________]               │
│ ✘ Bạn phải điền tên mục tiêu       (validation)    │
│                                                     │
│ Loại *                                              │
│ [🎯 Objective ▼]                                    │
│                                                     │
│ Ngày mục tiêu                                       │
│ [Chọn ngày] 📅                                      │
│                                                     │
│ Chủ sở hữu *                                        │
│ [🟡 Tua20000]                  (user picker)        │
│                                                     │
│                     [Hủy]  [Tạo] (primary)          │
└─────────────────────────────────────────────────────┘
```

**Validation:** Tên required. Loại required (default: Objective).
**Sau tạo:** Goal xuất hiện ngay trong danh sách S18.

---

### S19 — Goals: Đang theo dõi

**Giống S18 nhưng:**
- Filter mặc định: "Đang theo dõi ×" (active chip)
- Chỉ hiện goals mà current user đang follow

---

### S20 — Goals: Cập nhật trạng thái

**Mục đích:** Monthly status update dashboard — xem tiến độ Goals theo tháng.

**Layout:**
```
h1: Cập nhật trạng thái    [Cập nhật] [Viết thông tin cập nhật] buttons
Sub-tab: [Đang theo dõi]
──────────────────────────────────────────────────────
← [tháng 05] →                                Right panel:
────────────────────────────────────────────── Mục tiêu của bạn:
Dashboard thống kê (khi có data):             [Goal list with follower count]
┌─────────────────────────────────┐
│ 0 Đúng tiến độ   0 Có rủi ro   │ Mục tiêu mới: (newly created)
│ 0 Không đúng TĐ  0 Không có CN │
│ 0 Đã hủy         1 Đã hoàn tất │ Mục tiêu đã hoàn tất:
└─────────────────────────────────┘

Audit log (list of updates):
┌──────────────────────────────────────────┐
│ [Goal icon] [Goal name]                  │
│ [User avatar] [User name]  [Status badge]│
│ [N ngày trước] [Follower count]          │
│ Đã thay đổi trạng thái: [OLD] → [NEW]   │
│ [Chia sẻ] [Bỏ theo dõi] [···] [🔄 icon] │
│ [Comment input: "Thêm nhận xét..."]      │
└──────────────────────────────────────────┘
```

**Khi không có updates (empty):**
- Illustration (character với X marks)
- "Không ai viết bất kỳ thông tin cập nhật nào!!"
- Sub-text about monthly updates
- "Bạn đang tìm cách viết thông tin cập nhật? Hãy viết vào đây." [link]

**Month navigation:** ← prev month | [tháng N] | → next month
**Filter buttons (top-right of list):** Mọi cập nhật | Mục tiêu đã hoàn tất | Mục tiêu đang tiến hành

---

### S21 — Goals: Đã lưu trữ

**Chứa:** Goals Done + Cancelled + không còn sử dụng
**Tương tự:** Recycle Bin / Archive / History

**Layout:** Giống S18 với filter bar + empty table
**Empty state:** X icon illustration + "Chúng tôi không tìm được mục tiêu nào..." + "Xóa tất cả bộ lọc" link

---

### S22 — Goal Detail: Tab Tổng quan

**Module:** Goals | **Loại:** Detail
**Layout:**
```
Breadcrumb: Mục tiêu / [parent goal name if any]
[Goal icon] [Goal name]                    [Đã hoàn tất ▼] [📅 tháng 06 ▼] [Theo dõi] [Chia sẻ] [🔗]
Tabs: Tổng quan | Cập nhật [N] | Jira | Dự án | Bài học rút ra | Rủi ro | Quyết định
──────────────────────────────────────────────────────────────────────────────────────────────
┌──── Main content ──────────────────────────────────────────┐ ┌── Right panel (sticky) ──────┐
│ Banner: "Truyền đạt tầm nhìn quan trọng..." [X dismiss]   │ │ Tiến độ:                    │
│                                                            │ │  Mục tiêu quan trong [N]    │
│ Mô tả                                                      │ │  ──────── N%                │
│ [Rich text editor: Văn bản bình thường ▼] [B I ─ ··· :]   │ │                             │
│ "Mô tả tầm nhìn dài hạn tao tại..."                       │ │ Chủ sở hữu: [Avatar Name]  │
│ [Lưu] [Hủy lưu]                                           │ │                             │
│                                                            │ │ Người theo dõi [N]:         │
│ Kết quả chính (Key Results)                                │ │  Thêm người theo dõi        │
│ [Key result row: status | name | metric]  [•]              │ │  [Avatar] [Avatar]          │
│ [+ Tạo thêm]  ← button                                    │ │                             │
│                                                            │ │ Mục tiêu chính: [+]         │
│ Hoạt động:                                                 │ │  (max 1)                    │
│ [Nhận xét tab] [Lịch sử tab]                               │ │                             │
│ [Comment textarea]                                         │ │ Mục tiêu phụ: [+]           │
│ [1 Comment item: user | text | reactions]                  │ │  (multiple allowed)          │
│ ● Thêm nhận xét... thêm gia cụ lội thoại.                │ │                             │
└────────────────────────────────────────────────────────────┘ │ Loại: [🎯 Mục tiêu]        │
                                                               │                             │
                                                               │ Nhóm: [+]                   │
                                                               │                             │
                                                               │ Thẻ: [+]                    │
                                                               │                             │
                                                               │ Ngày bắt đầu: ✎             │
                                                               │  [1 Jan 2026]               │
                                                               └─────────────────────────────┘
```

**Top-right buttons:**
- Status badge (Đã hoàn tất ▼) → click → P16 (status picker)
- Date picker (📅 tháng 06 ▼) → P17 (date picker: ngày / tháng / quý options)
- Theo dõi toggle (click → follow/unfollow, counter updates)
- Chia sẻ button
- 🔗 share/link icon

**Key Results section:**
- Each row: [status dot] [metric/name] [progress value] [assignee avatar]
- "+" Tạo thêm → inline create row

**Right panel — Người theo dõi:**
- "Thêm người theo dõi" → search user input
- Shows avatar list, click avatar → share with them (sends notification)

**Right panel — Mục tiêu chính (Parent goal):**
- Only 1 allowed
- "+" → search existing goals to link as parent
- Shows linked parent goal chip

**Right panel — Mục tiêu phụ (Sub-goals):**
- Multiple allowed
- "+" → search/create sub-goals

**Right panel — Nhóm:**
- Teams responsible for this goal

---

### S23 — Goal Detail: Tab Cập nhật

**Mục đích:** Cập nhật tiến độ hàng tháng của goal

**Layout:**
```
Lịch sử dự án     ────────────    Lần cập nhật gần nhất: N ngày trước
     ●
  29 tháng 5
─────────────────────────────────────────────────────────
Trạng thái hiện tại:      Ngày mục tiêu:        Màu ▼
[ĐANG TIẾN ĐỘ ▼]         [📅 tháng 5 ▼]
(status dropdown)

ĐANG TIẾN ĐỘ
CÓ RỦI RO
KHÔNG ĐÚNG TIẾN ĐỘ
ĐÃ HOÀN THÀNH TIẾN ĐỘ
ĐÃ HOÀN TẤT
ĐÃ TAM DỪNG
ĐÃ HỦY

Viết bản cập nhật gần đây 280 ký tự. Nhấp "gửi nhất" để sau chép bản cập nhật gần đây nhất, còn nhập / để điền thêm trình phân khác.
[Rich text editor: Soạn thảo bằng Rows ✵ + B ⊞ © 🔗 ♾ ÷ 0/280  □ i ] [Lưu liên nhập]
                                                         [Đăng bản cập nhập] (primary)
─────────────────────────────────────────────────────────
Thông tin cập nhật trước đó:
● [User avatar] [Name]          [Status badge]
  [N ngày trước • 1 người đã xem]
  [N]
  Đã thay đổi trạng thái: [OLD badge] → [NEW badge]
  [Chia sẻ ▼] [Bỏ theo dõi ▼] [● ■ ♦ ▼ ··· 🔄]
● [Thêm nhận xét...thêm gia cuộc họp thoại]
```

**Status dropdown options:** ĐANG TIẾN ĐỘ / CÓ RỦI RO / KHÔNG ĐÚNG TIẾN ĐỘ / ĐÃ HOÀN THÀNH TIẾN ĐỘ / ĐÃ HOÀN TẤT / ĐÃ TẠM DỪNG / ĐÃ HỦY

**Ngày mục tiêu picker:** Month grid calendar; chia theo Ngày / Tháng / Quý tabs

**"Đăng bản cập nhập" button:** Posts update, adds to timeline; notifies followers

**Timeline dot:** Visual history marker for each update

---

### S24 — Goal Detail: Tab Jira

**Mục đích:** Liên kết Jira tasks với goal

**Layout:**
```
h2: Đóng góp cho hạng mục công việc Jira [N]
─────────────────────────────────────────────────────────────────
[☑] DTN-1: Làm lại tài liệu dự án          VIỆC CẦN LÀM ▼ [1] ×
    + Thêm hạng mục công việc Jira
─────────────────────────────────────────────────────────────────
    [Dán URL Jira]  input  [Hủy] [Thêm]
```

**Add task flow:** Click "Thêm hạng mục công việc Jira" → text input "Dán URL Jira" + Thêm button

**Task item:**
- Checkbox icon (checked/unchecked)
- Task title
- Status badge (VIỆC CẦN LÀM / etc.)
- Follower count badge
- "×" remove button

**Hover on task → Popover (P19):**
```
[Task icon] [Task name]
VIỆC CẦN LÀM ▼  = Trung bình
[🔗 Mở bản xem trước]
[⊂ Sao chép liên kết]
[🔗 Xem liên kết liên quan]
[🔴 Jira] (source app badge)
```

**Click task title → navigate to Jira space (that task)**

---

### S25 — Goal Detail: Tab Dự án

**Mục đích:** Link Projects với Goal

**Layout:**
```
Dự án đóng góp cho [Goal name]
─────────────────────────────────────────────────────────────
Các dự án có đóng góp:
Dự án [icon] [name]  Không [completed/in progress]  ···
Dự án [icon] [name]

Các dự án sẽ đóng góp tới các mục tiêu phụ:
[sub-goal icon] [sub-goal name]  [Không hoàn tất ▼]  [1] ▼
                          + Thêm dự án
```

**"Thêm dự án" → Popup P20:**
```
Tìm kiếm dự án
[search input]
Các dự án an có đóng góp:
● [icon] ueq         [status]
→ 342               [status]
🎯 uew              [status]
+ Tạo dự án mới (→ navigate to Projects home)
```

---

### S26 — Goal Detail: Tab Bài học rút ra

**Empty state:**
- Illustration (rocket/star character)
- "Những bộ đọc vĩ đại noi theo giỏng nhau sẽ chia sẻ kiến thức của họ"
- "Chia sẻ bất kỳ bài học kinh nghiệm nào để giúp các đội ngũ khác khởi đầu thuận lợi khi thực hiện các mục tiêu tương tự."
- [Thêm learning mới] button
- [Xem ví dụ] link

---

### S27 — Goal Detail: Tab Rủi ro

**Empty state:**
- "Nắm bắt các rủi ro đó biết"
- "Theo dõi rủi ro và đảm bảo các bên liên quan của bạn không bị ngạc nhiên bởi những rủi ro tiềm ẩn liên quan đến dự án này."
- [Thêm rủi ro mới] button

---

### S28 — Goal Detail: Tab Quyết định

**Empty state:**
- "Truyền đạt các quyết định lớn"
- "Ghi lại các quyết định lớn cho dự án này tại đây để sau chép bán cập nhật gần đây nhất, còn trong bản cập nhật mới nhất của bạn."
- [Thêm quyết định mới] button

---

### S29 — Projects: Thư mục dự án

**Module:** Projects | **Loại:** List
**Sidebar:** Thư mục dự án / Đang theo dõi / Cập nhật trạng thái / Đã lưu trữ

**Layout:**
```
h1: Dự án                           [Tạo dự án] button
Tabs: [Tất cả dự án] [Dự án của bạn] [+]
──────────────────────────────────────────────────────
[🔍 Tìm kiếm dự án]
Filters: [Lọc theo Thể] [Trạng thái] [Mục tiêu] [Nhóm] [Chủ sở hữu]
         [Người đóng góp] [Đang theo dõi] [Có gần sao] [Tuyển báo cáo]
──────────────────────────────────────────────────────
"Đang hiển thị N dự án"    [≡≡] [≡] sort | [Sắp xếp ▼] [⊞ Cột ▼] [···]
──────────────────────────────────────────────────────
Table: Tên | Trạng thái | Ngày mục tiêu | Chủ sở... | Đang theo dõi | Cập nhật lần...
```

**Row format:** Tương tự Goals table
**Click row → S33 (Project detail)**

---

### S30 — Projects: Đang theo dõi

**Chỉ hiện projects mà user đang follow**
**Không hiện với người khác** (private to viewer)

---

### S31 — Projects: Cập nhật trạng thái

**Tương tự S20 (Goals status updates) nhưng cho Projects**

Layout:
```
h1: Cập nhật trạng thái    [Cập nhật] [Viết thông tin cập nhật]
Sub-tab: [Đang theo dõi]
← [Tuần trước] →

Dashboard: N Đúng tiến độ | N Có rủi ro | N Không đúng TĐ | N Không có CN | N Đã hủy | N Đã hoàn tất

[Project update items list with same structure as Goals]
     Right panel: Dự án mới / Dự án đã hoàn tất
```

---

### S32 — Projects: Đã lưu trữ

**Giống S21 (Goals archived) nhưng cho Projects**

---

### S33 — Project Detail: Tab Giới thiệu

**Module:** Projects | **Loại:** Detail
**Khác với Goal detail:** Tab đầu tiên là "Giới thiệu" (không phải Tổng quan), và nội dung tab là structured sections thay vì free text.

**Layout:**
```
Breadcrumb: Dự án / [Parent if any]
[Project icon] [Project name]              [Đã hoàn tất ▼] [Theo dõi] [Chia sẻ 🔗] [···]
Tabs: Giới thiệu | Cập nhật [N] | Bài học rút ra | Rủi ro | Quyết định
──────────────────────────────────────────────────────────────────────────────────────────
Banner: "Dự án này đã hoàn tất. 🎉 Bày tỏ lời cảm ơn và chúc mừng tất cả những người đã đóng góp cho dự án này." [Khen ngợi] link
Thông báo: "Truyền đạt về bối cảnh của dự án để các nhóm hiểu được nội dung đang phụ lục..." [X]

"Dự án chúng ta đang thực hiện"
[Editable section — click to edit]

"Lý do thực hiện dự án"
[Editable section]

"Cách để biết rằng chúng ta đã thành công"
[Editable section]

"Nhận xét"
● [User] [Thêm nhận xét... thêm gia cuộc họp thoại]

──────────────────────────────────────────────────────────────────────────────────────────
Right panel:
Chủ sở hữu: [Avatar] Name
Người đóng góp [N]:       (click + → P21)
  [User avatar] [User name] (Lập trình viên)
Người theo dõi:            (click + → add follower)
  Thêm người theo dõi
Đóng góp vào mục tiêu:    (click + → P12 variant)
  [Goal icon] [Goal name] N%
Các dự án liên quan:       (click + → P22)
  Liên quan đến: [Project chip]
Công việc được theo dõi ở đâu?  (click ✎ → P23)
  📦 [Jira space name] | Board   ✎ ×
  → "Chinh sửa liên kết"
Liên kết:                  (click + → link input)
Thẻ:                       (click + → tag input)
Ngày bắt đầu: ✎            [1 Jun 2025] calendar
```

**Structured sections:**
- 3 sections với headings cố định
- Click section text → rich text editor với formatting toolbar
- [Lưu] / [Hủy lưu] buttons

**Công việc được theo dõi ở đâu (tracking link):**
- 1 Jira board link maximum
- Shows: [icon] [Space name] | Board
- Hover → edit (✎) and remove (×) buttons
- "Chinh sửa liên kết" label on hover
- Click ✎ → P23 (edit link popup with URL input)

---

### S34 — Project Detail: Tab Cập nhật

**Tương tự S23 (Goal update tab)** nhưng cho Projects:
- Timeline marker: "Lịch sử dự án"
- Status options: Đúng tiến độ / Có rủi ro / Không đúng tiến độ / Đã tạm dừng / Đã hoàn tất / Đã hủy
- Update entries with same social features (share, follow, comment)

---

### S35, S36, S37 — Project: Bài học rút ra / Rủi ro / Quyết định

**Tương tự S26, S27, S28** — cùng empty state + thêm mới pattern

---

### P21 — Thêm người đóng góp (Popup)

**Trigger:** "+" trên "Người đóng góp" trong right panel của S33

```
┌──── Thêm người đóng góp ────────────────────┐
│ [🔍 search input]                            │
│ ──────────────────────────────────────────── │
│ Nhóm của bạn:                               │
│ [88] ###   1 thành viên  ▲                 │
│ [88] RRRR  1 thành viên                    │
│ [88] ###   1 thành viên                    │
│ [88] wq    3 thành viên  ▼                 │
│ ──────────────────────────────────────────── │
│ [Individual user entries with avatar+name]  │
│  ngkiet2805                                 │
│  Thịnh Phát Bùi                             │
│  [88] ### ● Đội ngũ chính thức • 1 TV, k...│
└─────────────────────────────────────────────┘
```

**Shows:** Both teams AND individual users
**After select:** Avatar appears in Người đóng góp list

---

### P22 — Chọn dự án liên quan

```
┌──── Các dự án liên quan ────────────────────┐
│ Dự án liên quan đến ▼                       │
│ [🔍 Tìm kiếm dự án]                         │
│ ──────────────────────────────────────────── │
│ [Project chip options list]                 │
│   ueq [name]                               │
│ [Hủy] [Thêm] (primary)                     │
└─────────────────────────────────────────────┘
```

---

## D. ĐẶC TẢ COMPONENT DÙNG CHUNG

### D1 — Left Sidebar (Teams module)

```
Width: ~240px (estimated, Medium confidence, based on screenshot proportion)
Background: #FFFFFF or light gray
Border-right: 1px solid #DFE1E6

Structure:
┌─────────────────────────────────┐
│ [⊞] [Teams logo/icon]  [□ collapse]│
├─────────────────────────────────┤
│ ○ Dành cho bạn                 │ ← Active: blue left border 3px + light blue bg
│ 👥 Nhóm                        │
│ 👤 Mọi người                   │
│ ♡  Khen ngợi                   │
│ 🎯 Goals                       │
│ 📁 Projects                    │
└─────────────────────────────────┘
```

**Active state:** Left border 3px solid #0052CC + background #DEEBFF (light blue)
**Hover state:** Background #F4F5F7
**Font:** 14px, color #42526E
**Estimated row height:** 36px (Medium confidence)

---

### D2 — Topbar

```
Height: ~56px (High confidence, standard Jira topbar)
Background: white
Border-bottom: 1px solid #DFE1E6

Left section: [⊞ grid icon] [🏠 Home] (app switcher + home)
Center: [🔍 Search input ~500px wide, placeholder "Tìm kiếm"]
Right: [+ Tạo] (primary button) [🔔 notifs] [⚙️ settings] [? help] [🔴 avatar]
```

**+ Tạo button:** Background #0052CC, text white, border-radius 4px, padding 8px 16px

---

### D3 — Search Bar (in-page)

```
Width: 100%
Height: 40px
Background: #F4F5F7
Border: 1px solid #DFE1E6
Border-radius: 4px
Padding: 8px 12px
Placeholder: gray #6B778C
Icon: 🔍 left-aligned, 16px
```

---

### D4 — Filter Chips Bar

```
Display: flex, flex-wrap: wrap, gap: 8px
Chip style:
  Height: 32px
  Border: 1px solid #DFE1E6
  Border-radius: 4px
  Padding: 0 12px
  Font-size: 14px
  Background: #F4F5F7
  Hover: border-color #0052CC
  Active/selected: background #DEEBFF, border #0052CC, text #0052CC
  
"×" remove badge on active chip: inline, smaller font
"Đặt lại" chip: right-aligned, text-only style
```

---

### D5 — Tab Bar

```
Display: flex, border-bottom: 2px solid #DFE1E6
Tab item:
  Padding: 12px 16px
  Font: 14px #42526E
  Active: border-bottom 2px solid #0052CC, color #0052CC, font-weight 600
  Hover: background #F4F5F7
```

---

### D6 — Team Card (Grid view)

```
Width: calc(33.33% - 16px) (3-column grid)
Border: 1px solid #DFE1E6
Border-radius: 8px
Padding: 16px
Cursor: pointer

Contents:
- Team icon: 48x48px, border-radius: 8px, colored background + initials (white text, 18px bold)
- Top-right badges: blue dot (notification) + star (⭐, active=yellow)
- Team name: 16px bold #172B4D, margin-top: 8px
- Subtitle: 12px #6B778C "Đội ngũ chính thức ✅ • N thành viên"

Hover: box-shadow 0 2px 8px rgba(0,0,0,0.1), border-color #0052CC
Archived: filter: grayscale(0.7), opacity 0.8
```

---

### D7 — Goal/Project Table Row

```
Table: border-collapse collapse, width 100%
Row height: 48px
Border-bottom: 1px solid #DFE1E6
Hover: background #F4F5F7

Columns:
- Expand arrow: 24px
- Icon: 20px
- Name: flex 1, font 14px, color #172B4D
- Status badge: pill shape, 12px font, variants:
    ĐÃ HOÀN TẤT: bg #E3FCEF, color #006644
    ĐANG CHỜ XỬ LÝ: bg #DEEBFF, color #0747A6
    ĐANG TIẾN ĐỘ: bg #E3FCEF, color #006644
- Progress bar: 80px wide, 4px height, bg #DFE1E6, fill #36B37E
- Date chip: 14px, calendar icon
- Follower avatars: stacked circles 20px each
- Cập nhật: "Tuần trước" text
- ···: row action menu (visible on hover)
```

---

### D8 — Right Panel

```
Width: ~300px (Medium confidence, fixed)
Position: sticky top when scrolling
Border-left: 1px solid #DFE1E6
Padding: 16px
Background: white

Section headers: 12px uppercase #6B778C, letter-spacing 0.5px
Content rows: 14px #172B4D
"+" icon: blue #0052CC, 16px
Divider between sections: 1px #DFE1E6 margin 12px 0
```

---

### D9 — Modal

```
Backdrop: rgba(0,0,0,0.5)
Modal container:
  Width: 480px (standard) or 560px (wide)
  Border-radius: 8px
  Padding: 24px
  Background: white
  Box-shadow: 0 8px 32px rgba(0,0,0,0.25)

Header: "Title text" 18px bold + "✕" close button right
Body: padding 16px 0
Footer: flex end, gap 8px, [Cancel secondary] [Submit primary]

Esc / click backdrop → close (no save)
```

---

### D10 — Empty State

```
Container: flex column align-center, padding 48px 24px
Illustration: ~200px (robot/character SVG)
Title: 18px bold #172B4D, margin 16px 0 8px
Subtitle: 14px #6B778C, text-align center, max-width 400px
CTA button/link: below subtitle
```

---

### D11 — Status Badge

| Status | Background | Text color | Text |
|---|---|---|---|
| ĐÃ HOÀN TẤT | #E3FCEF | #006644 | ĐÃ HOÀN TẤT |
| ĐANG TIẾN ĐỘ | #E3FCEF | #006644 | ĐANG TIẾN ĐỘ |
| ĐANG CHỜ XỬ LÝ | #FFFAE6 | #974F0C | ĐANG CHỜ XỬ LÝ |
| CÓ RỦI RO | #FFFAE6 | #974F0C | CÓ RỦI RO |
| KHÔNG ĐÚNG TĐ | #FFEBE6 | #BF2600 | KHÔNG ĐÚNG TĐ |
| ĐÃ HỦY | #F4F5F7 | #6B778C | ĐÃ HỦY |

---

### D12 — Confirmation Dialog

```
Same as Modal but smaller (~380px)
Warning icon ⚠️ in title
Body: explain consequences
Footer: [Cancel (default)] [Action button (danger/primary)]

Danger button: bg #DE350B, text white, hover #BF2600
```

---

### D13 — Avatar

```
Sizes: 20px / 32px / 40px / 48px / 80px
Shape: circle
Content: initials (2 chars, uppercase, white) on colored background
Colors: cycle through palette (red/blue/green/purple/orange/teal)
Hover: tooltip with full name
Click: navigate to profile (S14)
Stacked avatars: overlap 8px, z-index increases, max 3-4 shown + "+N" badge
```

---

## E. ĐẶC TẢ THEO MODULE

### E1 — Teams Module

**Entities:** Team
```typescript
interface Team {
  id: string
  name: string
  type: 'OFFICIAL' | 'GROUP' // Đội ngũ chính thức | Nhóm
  icon?: string    // colored background + initials
  coverImage?: string
  description?: string  // "Việc chúng tôi đang thực hiện"
  members: User[]
  parentTeam?: Team
  subTeams: Team[]
  linkedJiraProjects: JiraProject[]
  linkedWebLinks: WebLink[]
  goals: Goal[]
  projects: Project[]
  kudos: Kudos[]
  isArchived: boolean
  isStarred: boolean  // per user
  isVerified: boolean
  createdAt: Date
}
```

**Permissions:**
| Action | Đội ngũ chính thức | Nhóm |
|---|---|---|
| Thêm thành viên | Admin only | All members |
| Xóa thành viên | Admin only | All members |
| Sửa thông tin | Admin only | All members |
| Xóa nhóm | Admin only | All members |
| Archive | Admin only | All members |

---

### E2 — Goals Module

**Entities:** Goal
```typescript
interface Goal {
  id: string
  name: string
  type: 'OBJECTIVE' | 'GOAL' | 'MILESTONE'
  status: 'ON_TRACK' | 'AT_RISK' | 'OFF_TRACK' | 'NO_UPDATE' | 'DONE' | 'CANCELLED' | 'PAUSED'
  progress: number  // 0-100
  owner: User
  followers: User[]
  parentGoal?: Goal
  subGoals: Goal[]
  keyResults: KeyResult[]
  linkedJiraTasks: JiraTask[]
  linkedProjects: Project[]
  team?: Team
  tag?: string
  startDate?: Date
  dueDate?: Date
  isArchived: boolean
  isStarred: boolean
  updates: GoalUpdate[]
  learnings: Learning[]
  risks: Risk[]
  decisions: Decision[]
}
```

---

### E3 — Projects Module (Home-level)

**Lưu ý:** Project ở đây khác với Jira Software Project. Đây là "dự án" cấp tổ chức.

```typescript
interface HomeProject {
  id: string
  name: string
  icon: string
  status: 'ON_TRACK' | 'AT_RISK' | 'OFF_TRACK' | 'DONE' | 'CANCELLED' | 'PAUSED'
  owner: User
  contributors: (User | Team)[]
  followers: User[]
  linkedGoals: Goal[]
  relatedProjects: HomeProject[]
  jiraBoardLink?: string   // 1 link only
  tasks: JiraTask[]
  startDate?: Date
  dueDate?: Date
  isArchived: boolean
  updates: ProjectUpdate[]
  learnings: Learning[]
  risks: Risk[]
  decisions: Decision[]
  // Structured description
  whatWeAreDoing?: string
  whyWeAreDoing?: string
  howWeKnowSuccess?: string
}
```

---

### E4 — Kudos

```typescript
interface Kudos {
  id: string
  sender: User
  recipient: Team   // always a team (members get individual copies)
  message: string
  gifUrl?: string
  linkedItem?: Goal | HomeProject | string  // link embedded
  reactions: KudosReaction[]
  createdAt: Date
}

interface KudosReaction {
  emoji: string
  users: User[]  // max 1 per user per emoji type
}
```

**Rules:**
- Sender giới hạn 3 loại emoji reaction khác nhau
- Mỗi loại emoji chỉ được thả 1 lần (click lại = remove)
- Gửi cho team → tất cả members nhận individually trong profile Kudos tab

---

## F. BẢN ĐỒ DỮ LIỆU VÀ QUAN HỆ

```
User
 ├── owns Goals (1-N)
 ├── follows Goals (N-N)
 ├── owns Projects (1-N)
 ├── follows Projects (N-N)
 ├── is member of Teams (N-N)
 ├── sends Kudos (1-N)
 └── receives Kudos (via Team membership)

Team
 ├── has Members (N-N via Users)
 ├── has Parent (1 Team, optional)
 ├── has SubTeams (N Teams)
 ├── links to JiraProjects (N-N, via icon or URL)
 ├── links to WebLinks (N)
 ├── contributes to Goals (N-N)
 └── receives Kudos (1-N)

Goal
 ├── has Owner (1 User)
 ├── has Followers (N Users)
 ├── has Parent Goal (1 optional)
 ├── has Sub-Goals (N)
 ├── has Key Results (N)
 ├── links to Jira Tasks (N, via URL)
 ├── links to Projects (N)
 └── has Updates/Learnings/Risks/Decisions

HomeProject
 ├── has Owner (1 User)
 ├── has Contributors (N Users + Teams)
 ├── has Followers (N Users)
 ├── contributes to Goals (N)
 ├── links to Related Projects (N)
 ├── has 1 Jira Board Link
 └── has Updates/Learnings/Risks/Decisions

Archive flow:
  Active → Archive (reversible, color fades to gray, read-only)
  Archive → Restore (back to Active)
  Active → Delete (irreversible, confirm dialog required)

Star/Favourite:
  Team starred → shows in filter "Có gần sao" (S03/S04)
  Team starred via Home sidebar star icon → same effect
```

---

## G. CHECKLIST NGHIỆM THU

### G1 — Màn hình

- [ ] S01 Home: greeting, app grid, recent items, next steps visible
- [ ] S02 Dành cho bạn: người làm cùng, nhóm của bạn, lời khen recent
- [ ] S03 Tất cả đội ngũ: search + 6 filter chips + grid/list/count
- [ ] S04 Nhóm của bạn: auto-filter chip "Thành viên đội là [user]"
- [ ] S05 Lưu trữ: archived teams greyed + 0 edit actions
- [ ] S06 Chi tiết — Giới thiệu: cover hover, tabs, member 2-col grid, right panel
- [ ] S07 Hoạt động: 2 work sections visible
- [ ] S08 Phân cấp: tree layout, parent/sub-team boxes, add/edit actions
- [ ] S09 Mục tiêu tab: goal list + "Theo dõi +" button + popup
- [ ] S10 Dự án tab: no inline create, navigate to Projects
- [ ] S11 Khen ngợi tab team: empty + give kudos button
- [ ] S12 Khen ngợi site: card grid + GIF + reactions
- [ ] S13 Mọi người: search + 7 filter chips + grid/list/col toggle
- [ ] S14 Profile Tổng quan: cover, editable bio, 6 recent tasks, right panel
- [ ] S15 Profile Mục tiêu: Đóng góp/Theo dõi toggle, hiện/hoàn tất sections
- [ ] S16 Profile Dự án: same structure as S15
- [ ] S17 Profile Khen ngợi: Đã nhận / Đã trao tabs
- [ ] S18 Goals list: table with all columns, column customizer
- [ ] S19 Goals Đang theo dõi: pre-filtered list
- [ ] S20 Goals Cập nhật: month nav, stats, audit log, right panel goals
- [ ] S21 Goals Archived: empty state visible
- [ ] S22 Goal detail Tổng quan: description editor, key results, activity
- [ ] S23 Goal detail Cập nhật: timeline, status dropdown, date picker, post update
- [ ] S24 Goal detail Jira: add task via URL, hover popover
- [ ] S25 Goal detail Dự án: project list + add
- [ ] S26-28 Bài học/Rủi ro/Quyết định: empty states + add buttons
- [ ] S29-32 Projects pages: same structure as Goals
- [ ] S33 Project detail Giới thiệu: 3 structured sections, right panel differences
- [ ] S34-37 Project tabs: same structure as Goal equivalents

### G2 — Buttons & Actions

- [ ] "Tạo đội ngũ" → P01 opens
- [ ] "Thêm người" (team) → P03 opens, max 50
- [ ] "···" menu shows: Star / Settings / Archive / Delete
- [ ] "Thêm ảnh bìa" hover → Upload button
- [ ] Upload cover image → native file picker
- [ ] "+" liên kết đội ngũ → dropdown: Jira / Web link
- [ ] "Liên kết dự án Jira" → P07 (recent projects + search + create nav)
- [ ] "Thêm liên kết" → P08 (URL + display text)
- [ ] "×" unlink → P09 confirm dialog
- [ ] "Give kudos" → P10 full-page view
- [ ] "Tạo mục tiêu" → P15 modal
- [ ] "Tạo dự án" → P24 modal
- [ ] Star team → appears in Có gần sao + filter + Home sidebar

### G3 — Popups & Modals

- [ ] P01 Tạo đội ngũ: Tên + Members + Loại; loại dropdown has 2 options with descriptions
- [ ] P02 Cài đặt nhóm: color swatches + permissions + project link
- [ ] P03 Thêm thành viên: tag input, max 50, save → member grid updates
- [ ] P04 Xóa nhóm: lists consequences; [Hủy] [Xóa đội ngũ] danger button
- [ ] P05 Sửa đội ngũ cha: search + "Xóa đội ngũ gốc" + list (single select)
- [ ] P06 Chọn đội ngũ con: search + checkboxes (multi-select)
- [ ] P07 Liên kết Jira: recent list + search + "Tạo dự án Jira" navigate
- [ ] P08 Web link: URL required + display text optional
- [ ] P09 Ngắt liên kết: warning text + [Hủy] [Xóa]
- [ ] P10 Give kudos: ← back, team name, icon ☺/🔗, GIF search (Giphy), text input
- [ ] P11 Link picker in Kudos: Home/Jira tabs, recent items list, [Hủy][Chèn]
- [ ] P12 Thêm mục tiêu (team): search + recent goals + "+ Tạo mục tiêu"
- [ ] P13 Tạo mục tiêu nhanh: Tên + Loại + Ngày + Chủ sở hữu
- [ ] P14 Chi tiết vai trò: Chức danh + Phòng ban + Vị trí
- [ ] P15 Tạo Goal (full): same as P13
- [ ] P16 Status picker: 7 status options
- [ ] P17 Date picker: ngày/tháng/quý tabs
- [ ] P18 Add Jira task: paste URL input
- [ ] P19 Task hover popover: status + priority + 4 actions
- [ ] P21 Thêm người đóng góp: shows teams AND individuals

### G4 — Tabs

- [ ] Team detail: 6 tabs all render with correct content
- [ ] Profile: 4 tabs render correctly
- [ ] Goal detail: 7 tabs (Tổng quan/Cập nhật/Jira/Dự án/Bài học/Rủi ro/Quyết định)
- [ ] Project detail: 5 tabs (Giới thiệu/Cập nhật/Bài học/Rủi ro/Quyết định)
- [ ] Goals list: Thư mục/Đang theo dõi/Cập nhật trạng thái/Đã lưu trữ (sidebar nav)
- [ ] Projects list: same 4 pages (sidebar nav)

### G5 — Trạng thái đặc biệt

- [ ] Archived team: grayscale, read-only, "Khôi phục" button only
- [ ] Empty filter result: X illustration + message + "Xóa tất cả bộ lọc"
- [ ] Empty Goals/Projects status update: character illustration + info text
- [ ] Kudos: emoji reaction limit 3 types / 1 each; click again removes
- [ ] Profile teams: max 4 shown + "Hiển thị thêm N mục khác" expand
- [ ] Recent work items: max 6 + "Hiển thị thêm" button
- [ ] Team hierarchy: 1 parent only; multiple sub-teams; 2-side split when 2+ sub-teams

---

## H. IMPLEMENTATION NOTES CHO DEV

### H1 — Routing
```
/teams                     → S02
/teams/all                 → S03
/teams/my-teams            → S04
/teams/archived            → S05
/teams/:id                 → S06 (default tab: overview)
/teams/:id/:tab            → S06-S11
/teams/kudos               → S12
/people                    → S13
/people/:id                → S14
/goals                     → S18
/goals/following           → S19
/goals/status-updates      → S20
/goals/archived            → S21
/goals/:id                 → S22
/goals/:id/:tab            → S22-S28
/projects                  → S29
/projects/following        → S30
/projects/status-updates   → S31
/projects/archived         → S32
/projects/:id              → S33
/projects/:id/:tab         → S33-S37
```

### H2 — Đặc điểm quan trọng
1. **Cover image:** Hover state cần JS mouseover event; overlay opacity 0.5 on hover
2. **Hierarchy tree:** Responsive layout — 1 sub-team: vertical line; 2+ sub-teams: horizontal split
3. **Kudos GIF:** Integrate Giphy API (search endpoint); autoplay on load
4. **Emoji reactions:** Max 3 distinct emoji types per user; toggle on click; batch update
5. **Member grid:** 2-column CSS grid; consistent avatar sizing 40px
6. **Status badge:** Do not hardcode status list — must come from API (extensible)
7. **Right panel sticky:** `position: sticky; top: 80px` (below topbar)
8. **Goal progress:** Aggregate from Key Results progress (auto-calculated)
9. **Project board link:** Only 1 link allowed; UI must enforce
10. **Archive visual:** CSS filter grayscale(0.8) + opacity(0.85) on archived items

### H3 — Jira Style tokens (estimates)
```css
--color-primary: #0052CC
--color-primary-hover: #0065FF
--color-primary-light: #DEEBFF
--color-text-primary: #172B4D
--color-text-secondary: #42526E
--color-text-subtle: #6B778C
--color-border: #DFE1E6
--color-bg-page: #F4F5F7
--color-bg-card: #FFFFFF
--color-danger: #DE350B
--color-success: #006644
--color-warning: #974F0C
--border-radius-sm: 4px
--border-radius-md: 8px
--font-size-body: 14px
--font-size-heading: 18px
--sidebar-width: 240px
--topbar-height: 56px
--right-panel-width: 300px
```

---

*File SPRINTA_MASTER_SPECIFICATION — Generated from PDF "Tổng hợp chức năng tích hợp" — Phần 2: Teams + Goals + Projects + People + Kudos*
*Tất cả thông số pixel là ước lượng (Estimated) dựa trên screenshot tỷ lệ. Confidence ghi rõ per component.*
