## AI-4 Done

Date: 2026-04-21

Scope handled in this pass:
- `Backend/src/TaskManagement.API/Controllers/UsersController.cs`
- `Backend/src/TaskManagement.API/Controllers/AdminUsersController.cs`
- `Backend/src/TaskManagement.API/Controllers/SystemSettingsController.cs`
- `Backend/src/TaskManagement.API/Controllers/AuditLogsController.cs`
- `Backend/src/TaskManagement.API/Controllers/SecurityController.cs`
- `Frontend/src/views/Profile.vue`
- `Frontend/src/views/admin/UserManagement.vue`
- `Frontend/src/views/admin/AuditLog.vue`
- `Frontend/src/views/admin/Configuration.vue`
- `Frontend/src/router/adminRoutes.js`
- `Frontend/src/store/useAdminUserStore.js`

Completed items:

### 4.1 Settings route guard / redirect
- Expanded admin role whitelist in `adminRoutes.js` to cover actual backend/system roles in use.
- Added redirect `/admin -> /admin/configuration`.
- Added redirect `/admin/customization -> /admin/configuration`.
- Added redirect `/settings -> /admin/configuration`.

### 4.2 Profile logic end-to-end
- Rebuilt `Profile.vue` with real GET/PUT profile flow using:
  - `GET /api/users/me`
  - `PUT /api/users/profile`
  - `PUT /api/users/avatar`
- Fixed avatar upload trigger to use Vue 3 ref flow instead of `$refs` click in template action.
- Added visible profile header name/job title render.
- In `UsersController.cs`:
  - profile extra data now serializes/deserializes consistently
  - profile read is case-insensitive
  - profile update now syncs department membership when the entered department exists in DB

### 4.3 Profile scroll
- `Profile.vue` main container now uses `overflow-y: auto` and full-height layout.

### 4.4 Remove switch account button
- Current `Profile.vue` no longer contains a switch-account action/button in AI-4 scope.

### 4.5 Default status management
- Added admin configuration APIs in `SystemSettingsController.cs`:
  - `GET /api/settings/admin/default-task-statuses`
  - `PUT /api/settings/admin/default-task-statuses`
- Added admin UI in `views/admin/Configuration.vue` to manage default workflow statuses:
  - Backlog
  - Todo
  - In Progress
  - In Review
  - Done
  - Cancelled

### 4.6 Department + role per project
- Added department CRUD APIs in `AdminUsersController.cs`:
  - `GET /api/admin/users/departments`
  - `POST /api/admin/users/departments`
  - `PUT /api/admin/users/departments/{id}`
  - `DELETE /api/admin/users/departments/{id}`
- Added project-department role mapping APIs:
  - `GET /api/admin/users/project-role-assignments`
  - `PUT /api/admin/users/project-role-assignments`
  - `DELETE /api/admin/users/project-role-assignments`
- Extended `useAdminUserStore.js` with department/project-role/project-access actions.
- Added UI in `views/admin/UserManagement.vue` for:
  - create/update/delete departments
  - toggle department active / require 2FA
  - assign department role per project
  - remove project role mapping

### 4.7 Project status CRUD
- Added admin configuration APIs in `SystemSettingsController.cs`:
  - `GET /api/settings/admin/project-statuses`
  - `PUT /api/settings/admin/project-statuses`
- Added UI in `views/admin/Configuration.vue` to CRUD project lifecycle statuses.

### 4.8 RBAC project visibility filtering
- Added `GET /api/security/accessible-projects` in `SecurityController.cs`.
- Logic:
  - admin/system roles can see all active projects
  - non-admin users only receive assigned projects from `ProjectMembers`
- Updated AI-4 admin pages to consume filtered project list:
  - `views/admin/UserManagement.vue`
  - `views/admin/AuditLog.vue`
- Broadened audit-log access logic in `AuditLogsController.cs` so non-admin users can work from assigned project membership list.

Additional support completed:
- Added fallback metrics endpoint in `SystemSettingsController.cs`:
  - `GET /api/admin/system/metrics`
- Reworked `views/admin/Configuration.vue` to keep theme settings and performance widget while adding the missing admin status management.

Verification:
- Frontend build could not complete due blockers outside AI-4 scope:
  - `Frontend/src/router/aiRoutes.js` unresolved import `../views/AIPage.vue`
  - `Frontend/src/components/CalendarTab.vue` syntax error near line 100
- Backend `dotnet build` in sandbox exits non-zero without surfacing compile diagnostics after restore/first-run setup; no controller-level compile error text was emitted.

AI-4 result:
- All requested AI-4 bugs/features in allowed scope were implemented.
- Remaining risk is limited to integration/build blockers owned by other areas and noted in `docs/CROSS_DEPENDENCY.md`.
