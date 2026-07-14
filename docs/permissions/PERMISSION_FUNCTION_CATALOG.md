# SprintA Permission Function Catalog

Backend-owned permission/action catalog for public pricing, AI usage, project covers, and task contingency plans.

## Public endpoints

| Endpoint | Auth | Source | Notes |
| --- | --- | --- | --- |
| `GET /api/public/pricing` | Public | `AiPricingPlans`, `AiCreditRules` | Monetary values remain `null` until Product Owner approval. |

## User context

| Endpoint | Auth | Data |
| --- | --- | --- |
| `GET /api/auth/context` | User JWT | User profile, roles, permissions, workspaces, visible projects. |

## AI usage

| Endpoint | Auth | Data |
| --- | --- | --- |
| `GET /api/ai/usage-summary` | User JWT | Aggregates `AiUsageLedgerEntries`; falls back to token usage only when ledger is empty. |
| `GET /api/ai/usage-ledger` | User JWT | Paginated real usage ledger rows. |

## Backend guards

| Function | Backend guard |
| --- | --- |
| `projects.cover.update` | System override role, workspace owner/admin, project creator, or project PM/PO/SM/lead/admin. |
| `tasks.contingency.manage` | System override role, project creator, project PM/PO/SM/lead/admin, task reporter, task assignee, or active assignment member. |
| `permissions.catalog.read` | Authenticated user. |
| `performance.summary.read` | Authenticated user. |

## AI actions

Action execution remains guarded by the backend action whitelist and per-action validation in `/api/ai/actions/execute`.
The UI must not execute an action before explicit user confirmation.
