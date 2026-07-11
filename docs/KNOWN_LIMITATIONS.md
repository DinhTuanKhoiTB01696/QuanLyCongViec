# Known Limitations

- OAuth: Google Calendar, Gmail, Slack va GitHub can client secret/redirect URI that. External sync can internet va provider configuration; khong co connected account gia trong seed.
- AI: Global AI Copilot can Gemini API key/quota/provider. Khi context-chat loi, UI hien loi that, khong fake answer.
- PWA: offline chi bao phu app shell/static assets; khong offline sync project, task, token hay API response.
- Frontend dependency: `npm install` sach hien fail vi `vite-plugin-pwa@0.21.2` peer Vite 3-6 trong khi project dung Vite 8. Existing install van `npm run build` thanh cong.
- Bundle: chunk `charts` khoang 1.873 MB (gzip 566 KB), `ui` 1.012 MB; build canh bao chunk lon nhung Workbox precache thanh cong.
- Manual QA chua mutate create/edit/delete/drag task, goal comment/reaction/attachment hay inbox mark-read/create-task trong dot final QA.
- Mobile 390 Copilot da xac minh khong tran ngang. Desktop 1440, tablet 768, install prompt va Cache Storage runtime chua duoc xac minh day du; static Workbox config denylist `/api`, `/auth`, hubs da duoc review.
- Tren Work Items, Copilot hien gan `pageType` la `SpaceSummary` thay vi `work-items`; route/project context van dung va prompt tom tat van tra dung 35 task.
- Demo database shared co count drift: 18 users, 7 projects va 6 statuses so voi baseline handoff 15/6/5. Main project van co 35 task, 5 goals va 4 custom fields.
