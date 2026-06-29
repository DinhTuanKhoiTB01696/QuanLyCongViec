# SprintA UI Style Guide

## Product direction

SprintA là web quản lý công việc kiểu SaaS/productivity. Giao diện cần hiện đại, gọn, dễ dùng, đáng tin, có cảm giác sản phẩm thật để demo/đầu tư.

## References

* Plane: tham khảo Work items, Cycles, Modules, Views, issue detail, table, filter.
* Twenty: tham khảo độ premium của card, table, empty state, spacing, detail panel.
* Vben Admin: tham khảo admin shell, sidebar, topbar, layout.
* shadcn-vue: tham khảo button, input, badge, dialog, card, table.
* Reka UI: tham khảo popover, dropdown, dialog, focus, z-index.
* Taste Skill / UI UX skill / frontend-design skill: tham khảo layout, hierarchy, spacing, typography, contrast.

Không copy source code/assets/logo từ các repo trên.

## Visual rules

* Không dùng màu quá nhạt khiến UI chìm.
* Không dùng màu quá tối khiến chữ khó đọc.
* Không dùng shadow đen nặng trong table.
* Không để mỗi màn một kiểu modal/card/table khác nhau.
* Button chính phải nổi bật nhưng không chói.
* Sidebar active item phải rõ.
* Search/input phải cùng chiều cao và cùng radius.
* Modal phải có header/body/footer rõ.
* Popover/date picker phải đúng z-index, không bị đè.

## Component direction

Ưu tiên tạo/dùng các class hoặc token:

* sa-card
* sa-panel
* sa-table
* sa-button-primary
* sa-button-secondary
* sa-input
* sa-modal
* sa-popover
* sa-badge
* sa-empty-state

## Acceptance

Một UI task chỉ đạt khi:

* Nhìn bằng mắt thấy thay đổi rõ.
* Không hỏng chức năng cũ.
* Build pass.
* Không sửa backend.
