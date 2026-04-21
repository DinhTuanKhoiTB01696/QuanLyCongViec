# DONE AI-5

Ngay cap nhat: 2026-04-21

## Pham vi AI-5 da xu ly

### Backend

- `Backend/src/TaskManagement.API/Controllers/AiController.cs`
  - Them retry logic 3 lan cho Gemini trong `breakdown-task`
  - Tra fallback message than thien khi Gemini tam thoi unavailable

- `Backend/src/TaskManagement.API/Controllers/GamificationController.cs`
  - Mo rong payload `/api/gamification/me`
  - Them career ladder / level progress / spotlight tasks / formula sample / achievement summary
  - Nang cap `/api/gamification/leaderboard` de hien career title

### Frontend

- `Frontend/src/views/AIPage.vue`
  - Fix `v-model` cho input va `@click` cho nut send
  - Them loading/thinking progress cho chat va breakdown prompts
  - Them GitHub repo analysis flow bang GitHub API neu co token
  - Them quick prompt de chen lenh breakdown vao chat box

- `Frontend/src/views/RewardsView.vue`
  - Render wallet + career level + formula + summary + spotlight tasks + achievements + leaderboard
  - Dam bao file ton tai va route import hoat dong de go build blocker verify

- `Frontend/src/components/TimelineTab.vue`
  - Fix nut `New work item`
  - Click vao task bar de mo task detail
  - Them `Create mode`
  - Them display options cục bộ
  - Them progress theo bucket cho Month / Quarter

- `Frontend/src/components/CalendarTab.vue`
  - Them hover tooltip hien task trong ngay
  - Them option hien/an done va overdue highlight
  - Sua logic expand `+ more`
  - Fix syntax issue tung chan frontend build gan line 100

- `Frontend/src/components/SpreadsheetTab.vue`
  - Them display options/filter logic
  - Them search, filter theo status, pagination local

- `Frontend/src/components/KanbanBoard.vue`
  - Them create mode tao task nhanh theo cot
  - Them progress ring tren task card, hover hien so %

- `Frontend/src/components/ListView.vue`
  - Them progress ring tren task row, hover hien so %

- `Frontend/src/components/CyclesTab.vue`
  - Dong bo store cache/fetch mode moi

- `Frontend/src/store/useSprintStore.js`
  - Them cache TTL 30s
  - Toggle favorite optimistic

- `Frontend/src/router/aiRoutes.js`
  - Them alias `/ai`

## Ket qua verify

- Frontend: `npm run build` PASS
- Frontend blocker verify cho AI-1 va AI-4 da duoc go:
  - `Frontend/src/views/RewardsView.vue` ton tai va build duoc
  - `Frontend/src/router/aiRoutes.js` resolve duoc `Frontend/src/views/AIPage.vue`
  - `Frontend/src/components/CalendarTab.vue` khong con syntax error
- Backend: chua verify compile xong do `dotnet build` dang fail o restore/project graph cua repo, khong tra ra loi C# cu the tu file AI-5

## Ghi chu

- Phan quick add tu Timeline dang dispatch `global-create-task` de mo flow tao task hien co cua app
- Repo analysis o AIPage dung GitHub API tu frontend; token duoc luu local browser de tai su dung
