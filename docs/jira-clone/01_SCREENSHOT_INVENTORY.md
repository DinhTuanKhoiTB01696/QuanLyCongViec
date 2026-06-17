# 01_SCREENSHOT_INVENTORY
Tài liệu này dùng để Claude/Codex hiểu các ảnh Jira đã thu thập. Không dùng tên file gốc khi prompt; dùng `id` hoặc `suggested_filename`.
Quy ước `status`:
- `usable`: có thể dùng làm nguồn UI.
- `usable_annotated`: ảnh có khoanh/chú thích, chỉ dùng để hiểu vùng được đánh dấu, không clone nét vẽ đỏ.
- `needs_more_for_auth`: có một phần auth nhưng chưa đủ toàn bộ luồng.
- `out_of_scope_for_project_app`: ảnh website marketing/public, không dùng cho project app trừ khi task nói rõ.
- `ignore`: không phải Jira UI.
- `needs_classification`: cần người xác nhận.

## Atlassian account settings
| ID | File đề xuất | Status | Tin cậy | Size | Ghi chú |
|---|---|---|---|---|---|
| `ACCOUNT-01-profile-and-visibility-dark` | `ACCOUNT-01-profile-and-visibility-dark.png` | usable | medium | 1906x903 | Account profile & visibility dark mode; likely outside project app unless cloning account settings. |
| `ACCOUNT-02-email-settings-dark` | `ACCOUNT-02-email-settings-dark.png` | usable | medium | 1867x893 | Account email settings dark mode. |
| `ACCOUNT-03-security-settings-dark` | `ACCOUNT-03-security-settings-dark.png` | usable | medium | 1856x901 | Account security settings dark mode. |
| `ACCOUNT-04-privacy-settings-dark` | `ACCOUNT-04-privacy-settings-dark.png` | usable | medium | 1741x547 | Account privacy settings dark mode. |
| `ACCOUNT-05-account-preferences-dark` | `ACCOUNT-05-account-preferences-dark.png` | usable | medium | 1917x894 | Account preferences/language/timezone dark mode. |
| `ACCOUNT-06-connected-apps-dark` | `ACCOUNT-06-connected-apps-dark.png` | usable | medium | 1915x890 | Account connected apps settings dark mode. |
| `ACCOUNT-07-link-preferences-dark` | `ACCOUNT-07-link-preferences-dark.png` | usable | medium | 1376x813 | Account link preferences dark mode. |
| `ACCOUNT-08-product-settings-dark` | `ACCOUNT-08-product-settings-dark.png` | usable | medium | 1457x792 | Account product settings dark mode. |

## Authentication / onboarding
| ID | File đề xuất | Status | Tin cậy | Size | Ghi chú |
|---|---|---|---|---|---|
| `AUTH-02-select-site-to-join-space-modal` | `AUTH-02-select-site-to-join-space-modal.jpeg` | usable | high | 1763x844 | Modal chọn site/space sau đăng nhập/đăng ký. |

## Authentication / signup
| ID | File đề xuất | Status | Tin cậy | Size | Ghi chú |
|---|---|---|---|---|---|
| `AUTH-01-jira-signup-page` | `AUTH-01-jira-signup-page.jpeg` | needs_more_for_auth | high | 1763x2561 | Có màn signup nhưng chưa đủ login/register/forgot password theo luồng auth đầy đủ. |
| `AUTH-03-get-started-signup-modal` | `AUTH-03-get-started-signup-modal.png` | usable | high | 1476x828 | Get started with Jira modal; signup/social buttons. |

## Dashboard
| ID | File đề xuất | Status | Tin cậy | Size | Ghi chú |
|---|---|---|---|---|---|
| `DASH-02-dashboard-create-modal` | `DASH-02-dashboard-create-modal.png` | usable | high | 1919x905 | Modal tạo dashboard. |
| `DASH-05-dashboard-edit-view` | `DASH-05-dashboard-edit-view.png` | usable | high | 1919x907 | Màn dashboard edit với gadget/layout. |
| `DASH-04-dashboard-add-gadget-panel` | `DASH-04-dashboard-add-gadget-panel.png` | usable | high | 381x715 | Panel add gadget. |
| `DASH-03-dashboard-layout-dropdown` | `DASH-03-dashboard-layout-dropdown.png` | usable | high | 405x106 | Dropdown đổi layout dashboard. |
| `DASH-01-dashboard-list` | `DASH-01-dashboard-list.png` | usable | high | 1919x902 | Danh sách dashboard dark mode. |
| `DASH-08-dashboard-save-annotated` | `DASH-08-dashboard-save-annotated.png` | usable_annotated | medium | 1858x867 | Dashboard save/config annotated. |
| `DASH-06-dashboard-create-modal-annotated` | `DASH-06-dashboard-create-modal-annotated.jpg` | usable_annotated | medium | 1913x802 | Create dashboard modal có chú thích đỏ. |
| `DASH-07-dashboard-config-annotated` | `DASH-07-dashboard-config-annotated.png` | usable_annotated | medium | 1771x867 | Dashboard config/layout/gadget có chú thích đỏ. |
| `DASH-09-dashboard-report-charts` | `DASH-09-dashboard-report-charts.png` | usable | medium | 1857x868 | Dashboard charts layout. |

## Global shell / sidebar
| ID | File đề xuất | Status | Tin cậy | Size | Ghi chú |
|---|---|---|---|---|---|
| `GS-01-global-sidebar-annotated` | `GS-01-global-sidebar-annotated.png` | usable_annotated | medium | 3396x1760 | Ảnh sidebar có đánh dấu/annotation; dùng để hiểu shell nếu thiếu ảnh sạch. |

## Global topbar / settings menu
| ID | File đề xuất | Status | Tin cậy | Size | Ghi chú |
|---|---|---|---|---|---|
| `SETTINGS-01-global-settings-menu-dark` | `SETTINGS-01-global-settings-menu-dark.png` | usable | high | 506x624 | Settings dropdown/panel from gear icon. |

## Global topbar / user menu
| ID | File đề xuất | Status | Tin cậy | Size | Ghi chú |
|---|---|---|---|---|---|
| `USER-01-user-menu-dark` | `USER-01-user-menu-dark.png` | usable | high | 303x355 | User avatar menu dark mode. |

## Goals app
| ID | File đề xuất | Status | Tin cậy | Size | Ghi chú |
|---|---|---|---|---|---|
| `GOALS-02-goal-directory-light` | `GOALS-02-goal-directory-light.png` | usable | high | 1913x902 | Standalone Goals directory/light, selected left nav: Thư mục mục tiêu. |
| `GOALS-03-goal-following-light` | `GOALS-03-goal-following-light.png` | usable | high | 1908x909 | Standalone Goals following/list state with callouts. |
| `GOALS-04-goal-status-updates-light` | `GOALS-04-goal-status-updates-light.png` | usable | high | 1914x903 | Standalone Goals status updates page. |
| `GOALS-05-goal-archived-light` | `GOALS-05-goal-archived-light.png` | usable | high | 1905x900 | Standalone Goals archived nav state. |

## Issue detail
| ID | File đề xuất | Status | Tin cậy | Size | Ghi chú |
|---|---|---|---|---|---|
| `ISSUE-13-issue-detail-light-old` | `ISSUE-13-issue-detail-light-old.jpeg` | usable | medium | 1763x844 | Old issue detail light mode. |
| `ISSUE-14-issue-detail-activity-light-old` | `ISSUE-14-issue-detail-activity-light-old.jpeg` | usable | medium | 1763x844 | Old issue detail activity/metadata light mode. |
| `ISSUE-01-full-issue-page` | `ISSUE-01-full-issue-page.png` | usable | high | 1366x768 | Full issue detail page default. |

## Issue detail / activity
| ID | File đề xuất | Status | Tin cậy | Size | Ghi chú |
|---|---|---|---|---|---|
| `ISSUE-12-activity-all-tab` | `ISSUE-12-activity-all-tab.png` | usable_annotated | high | 1366x817 | Activity all tab. |
| `ISSUE-09-activity-comments-tab` | `ISSUE-09-activity-comments-tab.png` | usable_annotated | high | 1366x768 | Activity comments tab. |
| `ISSUE-10-activity-history-tab` | `ISSUE-10-activity-history-tab.png` | usable_annotated | high | 1366x769 | Activity history tab. |
| `ISSUE-11-activity-worklog-tab` | `ISSUE-11-activity-worklog-tab.png` | usable_annotated | high | 1366x768 | Activity worklog tab/empty. |

## Issue detail / attachments
| ID | File đề xuất | Status | Tin cậy | Size | Ghi chú |
|---|---|---|---|---|---|
| `ISSUE-08-attachments-section` | `ISSUE-08-attachments-section.png` | usable_annotated | high | 1366x768 | Attachments section. |

## Issue detail / linked work items
| ID | File đề xuất | Status | Tin cậy | Size | Ghi chú |
|---|---|---|---|---|---|
| `ISSUE-07-linked-work-items-section` | `ISSUE-07-linked-work-items-section.png` | usable_annotated | high | 1357x768 | Linked work items section. |

## Issue detail / metadata
| ID | File đề xuất | Status | Tin cậy | Size | Ghi chú |
|---|---|---|---|---|---|
| `ISSUE-05-metadata-automation-panel` | `ISSUE-05-metadata-automation-panel.png` | usable_annotated | high | 1419x767 | Metadata Automation panel. |
| `ISSUE-03-metadata-details-panel` | `ISSUE-03-metadata-details-panel.png` | usable_annotated | high | 1367x768 | Tên gốc typo Detailis; metadata Details panel. |
| `ISSUE-04-metadata-development-panel` | `ISSUE-04-metadata-development-panel.png` | usable_annotated | high | 1404x768 | Metadata Development panel. |
| `ISSUE-02-metadata-status-in-progress` | `ISSUE-02-metadata-status-in-progress.png` | usable_annotated | high | 1463x792 | Có khoanh đỏ; dùng để hiểu metadata panel/status. |

## Issue detail / subtasks
| ID | File đề xuất | Status | Tin cậy | Size | Ghi chú |
|---|---|---|---|---|---|
| `ISSUE-06-subtasks-section` | `ISSUE-06-subtasks-section.png` | usable_annotated | high | 1366x768 | Subtasks section. |

## Marketing / public Jira website
| ID | File đề xuất | Status | Tin cậy | Size | Ghi chú |
|---|---|---|---|---|---|
| `MKT-01-jira-public-home-long-page` | `MKT-01-jira-public-home-long-page.jpeg` | out_of_scope_for_project_app | medium | 935x3918 | Trang marketing/home rất dài; chỉ dùng nếu cần clone landing page, không dùng để suy ra project app. |
| `MKT-02-jira-public-home-top` | `MKT-02-jira-public-home-top.png` | out_of_scope_for_project_app | high | 1910x897 | Public Jira landing page top; không dùng để suy ra app UI trừ auth/landing scope. |
| `MKT-03-jira-public-features-dropdown` | `MKT-03-jira-public-features-dropdown.png` | out_of_scope_for_project_app | high | 1852x465 | Public site Features dropdown. |
| `MKT-04-jira-public-solutions-dropdown` | `MKT-04-jira-public-solutions-dropdown.png` | out_of_scope_for_project_app | high | 1154x445 | Public site Solutions dropdown. |
| `MKT-05-jira-public-templates-dropdown` | `MKT-05-jira-public-templates-dropdown.png` | out_of_scope_for_project_app | high | 1463x847 | Public site Templates dropdown. |
| `MKT-06-ai-powered-project-management-section` | `MKT-06-ai-powered-project-management-section.png` | out_of_scope_for_project_app | high | 1095x849 | Marketing section about AI-powered project management. |
| `MKT-07-tools-and-teamwork-collection-section` | `MKT-07-tools-and-teamwork-collection-section.png` | out_of_scope_for_project_app | high | 1258x694 | Marketing integrations/teamwork collection section. |
| `MKT-08-jira-public-footer-cta` | `MKT-08-jira-public-footer-cta.png` | out_of_scope_for_project_app | high | 1018x771 | Marketing footer/CTA section. |

## Marketing / public Jira website / search
| ID | File đề xuất | Status | Tin cậy | Size | Ghi chú |
|---|---|---|---|---|---|
| `MKT-09-public-search-overlay-bar` | `MKT-09-public-search-overlay-bar.png` | out_of_scope_for_project_app | medium | 1538x68 | Public site search overlay; không thay thế in-app search popup. |

## Not Jira / source-management screenshot
| ID | File đề xuất | Status | Tin cậy | Size | Ghi chú |
|---|---|---|---|---|---|
| `META-01-google-drive-folder-not-jira` | `META-01-google-drive-folder-not-jira.png` | ignore | high | 1917x1079 | Google Drive folder screenshot; không dùng để clone Jira. |

## Project tab / Archived work items
| ID | File đề xuất | Status | Tin cậy | Size | Ghi chú |
|---|---|---|---|---|---|
| `ARCHIVED-01-project-archived-work-items-dark-empty` | `ARCHIVED-01-project-archived-work-items-dark-empty.png` | usable | high | 1907x891 | Archived work items dark mode empty state. |

## Project tab / Backlog
| ID | File đề xuất | Status | Tin cậy | Size | Ghi chú |
|---|---|---|---|---|---|
| `BACKLOG-01-project-backlog-light` | `BACKLOG-01-project-backlog-light.png` | usable | medium | 1763x844 | Ảnh backlog light mode. |
| `BACKLOG-02-project-backlog-with-detail-panel-light` | `BACKLOG-02-project-backlog-with-detail-panel-light.jpeg` | usable | medium | 1763x844 | Backlog với panel detail bên phải. |

## Project tab / Board
| ID | File đề xuất | Status | Tin cậy | Size | Ghi chú |
|---|---|---|---|---|---|
| `BOARD-01-project-board-light` | `BOARD-01-project-board-light.jpeg` | usable | medium | 1763x844 | Board light mode. |
| `BOARD-02-project-board-dark` | `BOARD-02-project-board-dark.png` | usable | high | 1911x901 | Board dark mode with project shell, columns, sprint complete button. |

## Project tab / Calendar
| ID | File đề xuất | Status | Tin cậy | Size | Ghi chú |
|---|---|---|---|---|---|
| `CALENDAR-01-project-calendar-light` | `CALENDAR-01-project-calendar-light.jpeg` | usable | medium | 1763x844 | Calendar light mode với unscheduled work panel. |
| `CALENDAR-02-project-calendar-dark-unscheduled-panel` | `CALENDAR-02-project-calendar-dark-unscheduled-panel.png` | usable | high | 1908x922 | Calendar dark mode with unscheduled work right panel. |

## Project tab / Code
| ID | File đề xuất | Status | Tin cậy | Size | Ghi chú |
|---|---|---|---|---|---|
| `CODE-01-project-code-dark-empty-connect-tools` | `CODE-01-project-code-dark-empty-connect-tools.png` | usable | high | 1909x899 | Code dark mode empty/connect GitHub/Bitbucket. |

## Project tab / Development
| ID | File đề xuất | Status | Tin cậy | Size | Ghi chú |
|---|---|---|---|---|---|
| `DEVELOPMENT-01-project-development-light` | `DEVELOPMENT-01-project-development-light.jpeg` | usable | high | 575x848 | Development tab light mode, kích thước hẹp/cropped. |
| `DEVELOPMENT-02-project-development-dark` | `DEVELOPMENT-02-project-development-dark.png` | usable | high | 1907x896 | Development dark mode with metrics and related work tabs. |

## Project tab / Docs
| ID | File đề xuất | Status | Tin cậy | Size | Ghi chú |
|---|---|---|---|---|---|
| `DOCS-01-project-docs-list-light` | `DOCS-01-project-docs-list-light.jpeg` | usable | high | 1763x844 | Docs tab/list light mode. |
| `DOCS-02-project-docs-dark-confluence-card` | `DOCS-02-project-docs-dark-confluence-card.png` | usable | high | 1896x909 | Docs dark mode Confluence card. |

## Project tab / Forms
| ID | File đề xuất | Status | Tin cậy | Size | Ghi chú |
|---|---|---|---|---|---|
| `FORMS-01-project-forms-empty-light` | `FORMS-01-project-forms-empty-light.jpeg` | usable | high | 1763x844 | Forms landing/empty light mode. |
| `FORMS-02-project-forms-create-dropdown-light` | `FORMS-02-project-forms-create-dropdown-light.jpeg` | usable | high | 1763x844 | Forms create/template dropdown. |
| `FORMS-03-project-forms-builder-light` | `FORMS-03-project-forms-builder-light.jpeg` | usable | high | 1763x844 | Forms builder/editor page. |
| `FORMS-04-project-forms-dark-create-options` | `FORMS-04-project-forms-dark-create-options.png` | usable | high | 1908x888 | Forms dark mode empty/create options. |

## Project tab / Goals
| ID | File đề xuất | Status | Tin cậy | Size | Ghi chú |
|---|---|---|---|---|---|
| `GOALS-01-project-goals-tab-dark-empty` | `GOALS-01-project-goals-tab-dark-empty.png` | usable | high | 1880x904 | Goals tab in project dark mode empty/linked goals prompt. |

## Project tab / List
| ID | File đề xuất | Status | Tin cậy | Size | Ghi chú |
|---|---|---|---|---|---|
| `LIST-01-project-list-dark` | `LIST-01-project-list-dark.png` | usable | high | 1904x902 | List/table view dark mode. |

## Project tab / Reports
| ID | File đề xuất | Status | Tin cậy | Size | Ghi chú |
|---|---|---|---|---|---|
| `REPORTS-02-project-reports-light-old` | `REPORTS-02-project-reports-light-old.jpeg` | usable | medium | 575x848 | Report light mode old/cropped. |
| `REPORTS-01-project-reports-light` | `REPORTS-01-project-reports-light.jpeg` | usable | high | 575x848 | Reports page light mode, kích thước hẹp/cropped. |

## Project tab / Summary
| ID | File đề xuất | Status | Tin cậy | Size | Ghi chú |
|---|---|---|---|---|---|
| `SUMMARY-01-project-summary-light` | `SUMMARY-01-project-summary-light.jpeg` | usable | high | 575x848 | Summary page light mode, kích thước hẹp/cropped. |
| `SUMMARY-02-project-summary-dark` | `SUMMARY-02-project-summary-dark.png` | usable | high | 1900x914 | Summary dark mode with metrics/cards/activity. |

## Project tab / Timeline
| ID | File đề xuất | Status | Tin cậy | Size | Ghi chú |
|---|---|---|---|---|---|
| `TIMELINE-01-project-timeline-light` | `TIMELINE-01-project-timeline-light.jpeg` | usable | medium | 1763x844 | Ảnh cũ timeline light mode. |
| `TIMELINE-02-project-timeline-dark` | `TIMELINE-02-project-timeline-dark.png` | usable | high | 1917x898 | Timeline dark mode monthly timeline. |

## Sprint / backlog
| ID | File đề xuất | Status | Tin cậy | Size | Ghi chú |
|---|---|---|---|---|---|
| `SPRINT-01-project-sprint-view-light` | `SPRINT-01-project-sprint-view-light.jpeg` | usable | medium | 1763x844 | Ảnh cũ sprint view light mode. |

## Teams / for you
| ID | File đề xuất | Status | Tin cậy | Size | Ghi chú |
|---|---|---|---|---|---|
| `TEAMS-04-for-you-light` | `TEAMS-04-for-you-light.png` | usable | medium | 1919x900 | Teams For You page with people/team cards. |

## Teams / groups
| ID | File đề xuất | Status | Tin cậy | Size | Ghi chú |
|---|---|---|---|---|---|
| `TEAMS-03-team-list-light` | `TEAMS-03-team-list-light.png` | usable | medium | 1917x894 | Team list/card light mode. |

## Teams / praise
| ID | File đề xuất | Status | Tin cậy | Size | Ghi chú |
|---|---|---|---|---|---|
| `TEAMS-02-praise-empty-light` | `TEAMS-02-praise-empty-light.png` | usable | medium | 1909x910 | Praise empty state. |

## Teams / profile
| ID | File đề xuất | Status | Tin cậy | Size | Ghi chú |
|---|---|---|---|---|---|
| `TEAMS-01-user-profile-overview-light` | `TEAMS-01-user-profile-overview-light.png` | usable | medium | 1906x901 | User profile page in Teams area. |
