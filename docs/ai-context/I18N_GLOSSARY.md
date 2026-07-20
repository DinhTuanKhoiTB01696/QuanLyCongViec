# SprintA I18N Glossary

Canonical labels for the demo product. Keep API paths, enum keys, IDs, and proper product names unchanged.

| English | Vietnamese | Usage note |
| --- | --- | --- |
| Project | Dự án | Use for a work-management container. |
| Work item | Công việc | Prefer this over the raw word `task` in Vietnamese UI. |
| Cycle | Chu kỳ | A time-boxed planning cycle; do not mix with `Sprint` unless showing the domain value. |
| Sprint | Sprint | Keep when referring to the Scrum domain value or a named sprint. |
| Module | Mô-đun | A feature/epic-sized body of work. |
| Goal | Mục tiêu | Strategic target or OKR objective. |
| Member | Thành viên | A workspace or project participant. |
| Intake | Yêu cầu | An item in the request queue before it becomes a work item. |
| Request queue | Hàng chờ yêu cầu | Explain the purpose of the Intake screen. |
| Saved view | Bộ lọc đã lưu | A reusable filtered view of work items. |
| Page | Tài liệu | Project wiki/document page. |
| Report | Báo cáo | A project or workspace analytical summary. |
| Permission | Phân quyền | Access rights, not approval status. |
| Custom field | Trường tùy chỉnh | A configurable project-level field. |
| Workflow | Quy trình trạng thái | The statuses through which a work item moves. |
| Assignee | Người thực hiện | The member responsible for a work item. |
| Due date | Hạn hoàn thành | Date by which the work should be completed. |
| Priority | Độ ưu tiên | Do not translate priority values inside API payloads. |
| Create work item | Tạo công việc | Primary creation CTA. |
| Confirm | Xác nhận | Required before a write action from AI. |
| Cancel | Hủy | Cancels an unexecuted UI action preview. |
| AI action preview | Xem trước thao tác AI | Never report data as created before execution succeeds. |
| Overdue | Quá hạn | Use for work past its due date. |
| In progress | Đang thực hiện | UI label; keep the backend enum `IN PROGRESS` unchanged. |

## Demo content rules

- Vietnamese locale: use professional Vietnamese for titles, descriptions, alerts, and empty states.
- English locale: use complete English, not mixed labels in one component.
- Do not seed placeholder text such as `abc`, `123456`, `hellu`, `Lorem Ipsum`, or unnamed test records.
- Action fixtures use stable IDs and `IF NOT EXISTS` guards so re-running the seed never creates duplicates.
