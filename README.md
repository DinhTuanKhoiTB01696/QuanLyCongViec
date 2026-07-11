# SprintA

SprintA la nen tang quan ly cong viec cho doi nhom, gom project dashboard, Kanban/List, workflow dong, custom fields, goals, reports, Integration Hub/Unified Inbox va Global AI Copilot.

## Cong nghe

- Backend: ASP.NET Core, Entity Framework Core, SQL Server, JWT, SignalR.
- Frontend: Vue 3, Vite, Pinia, Element Plus, PWA/Workbox.
- AI: Gemini qua backend; frontend khong goi provider truc tiep.

## Yeu cau

- .NET SDK tuong thich voi cac project trong `Backend`.
- Node.js va npm.
- SQL Server/SQL Express; connection string duoc cau hinh ngoai production source.
- Gemini/OAuth can credentials that trong Development secrets hoac environment variables.

## Chay local

Backend:

```powershell
cd Backend/src/TaskManagement.API
dotnet restore
dotnet run
```

Frontend:

```powershell
cd Frontend
npm install
npm run dev
```

Frontend mac dinh chay tai `http://localhost:5173` va proxy `/api` den `http://localhost:5136`.

## Migration va demo data

```powershell
cd Backend/src/TaskManagement.API
dotnet ef database update --project ../TaskManagement.Infrastructure --context ApplicationDbContext

cd ../../..
powershell -ExecutionPolicy Bypass -File scripts/run-sql.ps1
```

Script `scripts/seed-demo-data.sql` idempotent. Khong seed OAuth token hay IntegrationAccount gia.

Tai khoan Development:

- Email: `khoi.nguyen@novatech.vn`
- Password: `Demo@123`
- Workspace: `A0000001-0001-0001-0001-000000000001`
- Project: `C0000001-0001-0001-0001-000000000001` (`SprintA Enterprise Platform`)

Khong su dung tai khoan nay trong production.

## Cau hinh ngoai

- Dat `Gemini:ApiKey` bang user secrets/environment variable de dung AI Copilot.
- Dat client id/client secret/redirect URI that cho Google Calendar, Gmail, Slack va GitHub neu can OAuth.
- Khong commit JWT secret, OAuth secret, API key, access token hoac refresh token.

## Module chinh

Authentication, Dashboard, Work Items, Dynamic Workflow, Custom Fields, Goals, Teams/People, Reports, Integration Hub/Unified Inbox, Notifications, Global AI Copilot va PWA app shell.

## Kiem thu

```powershell
cd Backend
dotnet build
dotnet test --no-build

cd ../Frontend
npm run build
```

Xem [Final QA Checklist](docs/FINAL_QA_CHECKLIST.md), [Demo Script](docs/DEMO_SCRIPT.md), [Test Accounts](docs/TEST_ACCOUNTS.md), [Release Notes](docs/RELEASE_NOTES.md) va [Known Limitations](docs/KNOWN_LIMITATIONS.md).

## Troubleshooting

- Loi copy DLL khi `dotnet build`: dung API dang chay roi build lai.
- `npm install` bao peer conflict: xem Known Limitations; khong dung `--force` cho release reproducibility.
- Login/API khong hoat dong: kiem tra SQL Server, migration, backend port `5136` va Vite proxy.
- AI tra `503`: kiem tra Gemini key/quota/provider, khong tao fallback answer gia.
- OAuth khong ket noi: kiem tra provider credentials va redirect URI.

