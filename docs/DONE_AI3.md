## DONE_AI3

### Scope completed

Files handled in AI-3 scope:

- `Backend/src/TaskManagement.API/Controllers/ProjectsController.cs`
- `Backend/src/TaskManagement.API/Controllers/NotificationsController.cs`
- `Frontend/src/views/Dashboard.vue`
- `Frontend/src/views/ManageSpaces.vue`
- `Frontend/src/components/CreateProjectModal.vue`
- `Frontend/src/views/Home.vue`
- `Frontend/src/components/NotificationsDropdown.vue`
- `Frontend/src/store/useProjectStore.js`
- `Frontend/src/router/spaceRoutes.js`
- `Frontend/src/router/dashboardRoutes.js`

### High bugs handled

#### 3.1 Sidebar Project data

- Added full project fetch in `useProjectStore.js`
- Added normalized project tree data
- Added child nodes:
  - Work items
  - Cycles
  - Modules
  - Views
  - Pages
- Added expand/collapse state helpers

#### 3.2 Task hiển thị không đúng project

- Added `GET /api/projects/{id}/work-items`
- Query is filtered strictly by `ProjectId`

#### 3.4 Project settings route

- Added route alias in `spaceRoutes.js`:
  - `/space/:id/settings` -> `/admin/configuration?projectId=:id`
- Updated project settings button in `ManageSpaces.vue`

#### 3.5 Dashboard favorite persistence

- Added `PUT /api/projects/{id}/favorite`
- Favorite state is stored inside project `NavigationConfig`
- `ManageSpaces.vue` and `Dashboard.vue` now persist favorite state via API

#### 3.7 Dashboard New Work Item

- Rebuilt `Dashboard.vue`
- Added real project list
- Added working `New Work Item` dialog
- Added create task flow via:
  - `POST /projects/{projectId}/WorkTasks`

#### 3.8 Search bar logic

- Added logged-in search bar in `Home.vue`
- Added backend alias:
  - `GET /api/worktasks?search=keyword`
- Search results route to:
  - `/space/{projectId}?task={taskId}`

#### 3.9 Notifications dropdown

- Rebuilt `NotificationsDropdown.vue`
- Fetch notifications from backend
- Support unread filter
- Mark single read
- Mark all read
- Normalize old links into `/space/...`
- Keep SignalR live updates

### Medium bugs handled

#### 3.3 Create Project cover gallery

- Rebuilt `CreateProjectModal.vue` into actual project creation modal
- Added cover gallery using `picsum.photos`
- Added live cover preview
- Added create project API call

#### 3.6 Replace hardcoded personal naming in dashboard

- Dashboard header now uses `SprintA`
- Replaced personal-style quickstart page with project-oriented dashboard

#### 3.10 Notification event endpoints

Added notification creation endpoints in `NotificationsController.cs`:

- `POST /api/notifications/events/task-assigned`
- `POST /api/notifications/events/task-status-changed`
- `POST /api/notifications/events/comment-added`

These endpoints are ready for event producers to call.

#### Dashboard route build blocker

- Updated `dashboardRoutes.js` to remove the `RewardsView.vue` dependency from AI-3-owned routing
- Kept `/rewards` as a valid route by redirecting it to `/dashboard`
- This prevents dashboard route build issues without editing AI-5-owned view files

### Remaining note

Visible sidebar render of the full project tree still depends on layout files outside AI-3 scope:

- `Frontend/src/components/layout/NexusSidebar.vue`
- `Frontend/src/components/layout/NexusTopbar.vue`

Data layer for that dependency is already prepared in `useProjectStore.js`.
