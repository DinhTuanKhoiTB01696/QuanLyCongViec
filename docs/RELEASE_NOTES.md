# SprintA Release Notes

## SprintA delivery snapshot - 2026-07-11

### Hoan thanh

- Project dashboard va demo workspace chuyen nghiep.
- Work Items Kanban/List, dynamic workflow, import/export va task detail.
- Project Custom Fields va permission guards.
- Goals, reports, teams/people va notifications.
- Integration Hub/Unified Inbox khong seed connected account gia.
- Global AI Copilot frontend/backend theo project context, selected text va status mapping.
- Responsive/PWA source, manifest, icons va generated service worker.
- Idempotent demo seed voi tai khoan Development.

### Security

- AI context endpoint yeu cau authentication va project/workspace access.
- Copilot khong tu mutation task; `actions` rong.
- Khong seed OAuth access/refresh token.
- Provider error khong duoc frontend bien thanh success/fake answer.

### Quality status

- Backend build: exit 0, 0 warning/error.
- Backend tests: 40/40 passed.
- Frontend build: exit 0; PWA service worker generated.
- Clean `npm install`: chua dat do peer dependency Vite/PWA plugin.

Xem `docs/KNOWN_LIMITATIONS.md` va `docs/FINAL_QA_CHECKLIST.md` truoc khi phat hanh.

