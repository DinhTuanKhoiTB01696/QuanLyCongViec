# Admin Comparison Report

Date: 2026-04-21

## Objective

This report compares the admin area of `D:\A\QuanLyCongViec` with the admin app inside `D:\A\plane`, while also summarizing the current test/build status so we can determine:

- what our admin area already has
- what Plane has that we still lack
- which admin capabilities should be added first
- what prompt can be sent to a single AI to begin the work

## 1. Quick full-project test results

### Backend

Command executed:

```powershell
dotnet build Backend\src\TaskManagement.API\TaskManagement.API.csproj
```

Result:

- PASS
- 0 errors
- 2 warnings:
  - `Backend/src/TaskManagement.API/Controllers/AuditLogsController.cs:244`
  - `Backend/src/TaskManagement.API/Controllers/StickiesController.cs:91`

Assessment:

- Backend currently builds successfully
- No full automated test suite was clearly exposed in the repo for immediate execution beyond the build check

### Frontend

Command executed:

```powershell
npm run build
```

Working directory:

```text
Frontend/
```

Result:

- PASS
- Vite production build completed successfully

Current warnings:

- CSS `:deep` is being warned by lightningcss and should gradually be migrated to `::deep`
- Several JS chunks are larger than 500 kB
- The largest chunks currently include:
  - `SpaceSummary` ~340 kB
  - `PagesView` ~435 kB
  - `vue-router` ~1,037 kB
  - several heavy chart-related packages

Assessment:

- Frontend currently builds successfully
- There are clear signs that code-splitting and chunk optimization still need work

## 2. What our current admin area already has

Based on:

- `Frontend/src/router/adminRoutes.js`
- `Frontend/src/views/admin/*`

Our current admin area contains the following groups:

### 2.1 User administration

- `UserManagement.vue`

Likely functionality based on code and routing:

- view user list
- manage roles / departments / project-role assignments inside the current application domain
- perform user administration actions

### 2.2 Audit log

- `AuditLog.vue`

Functionality:

- review system/activity audit logs

### 2.3 Organization

- `OrganizationProfile.vue`
- `OrganizationContact.vue`

Functionality:

- organization profile
- organization contact information

### 2.4 General application configuration

- `Configuration.vue`

Functionality:

- internal application configuration
- theme/configuration behavior in the scope of the current app

### 2.5 Security

- `security/TwoFactorAuth.vue`
- `security/ChangePassword.vue`
- `security/IpWhitelist.vue`

Functionality:

- 2FA
- password change
- IP whitelist

## 3. What Plane admin already has

Directly based on:

- `D:\A\plane\apps\admin\app\(all)\(dashboard)\*`
- `sidebar-menu.tsx`
- `general/page.tsx`
- `authentication/page.tsx`

Plane has a dedicated admin app at the `instance` level, not only an internal workspace/admin section.

### 3.1 General settings

Directory:

- `apps/admin/app/(all)/(dashboard)/general`

Clearly indicated capabilities:

- rename the instance
- manage instance admin email addresses
- enable/disable telemetry

### 3.2 Authentication settings

Directory:

- `apps/admin/app/(all)/(dashboard)/authentication`
- sub-pages for:
  - `google`
  - `github`
  - `gitlab`
  - `gitea`

Clearly indicated capabilities:

- enable/disable authentication modes
- enable/disable sign-up without invite
- configure OAuth credentials per provider
- validate that at least one auth method always remains enabled

### 3.3 Email settings

Directory:

- `apps/admin/app/(all)/(dashboard)/email`

Clearly indicated capabilities:

- email/SMTP configuration
- email security settings
- test email modal

### 3.4 AI settings

Directory:

- `apps/admin/app/(all)/(dashboard)/ai`

Capability:

- system-level AI configuration

### 3.5 Image / media settings

Directory:

- `apps/admin/app/(all)/(dashboard)/image`

Capability:

- image/storage-related admin settings

### 3.6 Workspace administration

Directory:

- `apps/admin/app/(all)/(dashboard)/workspace`
- `workspace/create`

Capability:

- workspace administration
- workspace creation at the admin/instance level

## 4. Comparison: what we are still missing compared to Plane

Below are the missing areas, grouped by practical priority.

### 4.1 Missing a clear "instance admin" layer

Current state:

- Our admin area is currently centered around:
  - user management
  - organization profile
  - internal configuration
  - security
- There is no clearly separated concept for:
  - instance settings
  - instance admin ownership
  - instance-level feature gates

Meaning:

- If we want to move closer to Plane, we need to separate `system / instance admin` from `workspace / project admin`

### 4.2 Missing authentication provider administration

Plane already has:

- enable/disable sign-up
- OAuth providers:
  - Google
  - GitHub
  - GitLab
  - Gitea
- auth mode validation

What we currently do not see in our admin area:

- an admin page to enable/disable login methods
- an admin page to configure OAuth credentials per provider
- instance-level "invite-only sign-up" control

Priority:

- Very high

### 4.3 Missing email/SMTP administration

Plane already has:

- email configuration form
- test email modal
- email security options

What we currently do not see:

- SMTP admin page
- test email action
- email subsystem health / status

Priority:

- Very high

### 4.4 Missing AI administration at admin level

Plane already has:

- dedicated AI admin page

What we currently do not see:

- admin page to configure AI provider / API key / enable-disable
- system-level AI quota / model / provider management

Priority:

- Medium to high

### 4.5 Missing image/media/storage administration

Plane already has:

- image admin page

What we currently do not see:

- upload provider configuration
- storage limit management
- image service settings

Priority:

- Medium

### 4.6 Missing workspace provisioning at admin level

Plane already has:

- workspace list/create in the admin app

What we currently do not see:

- create/manage workspace/space from a super-admin level
- quota / ownership / workspace lifecycle control

Priority:

- High if the product is expected to evolve into a stronger multi-tenant system

### 4.7 Missing instance-level feature flags / telemetry controls

Plane already has:

- telemetry control in general settings
- authentication mode toggles

What we currently do not see:

- feature flag admin panel
- telemetry / tracking consent admin settings

Priority:

- Medium

### 4.8 No clearly separated scalable admin shell

Plane:

- has a dedicated admin app
- has a dedicated sidebar menu
- has page wrapper/grouping designed specifically for admin workflows

Our current state:

- admin is still a group of routes inside the main frontend app

Consequence:

- harder to scale cleanly
- harder to separate instance/workspace responsibilities
- more likely to produce fragmented routes and scattered config logic

Priority:

- High if the goal is a stronger, more scalable admin architecture

## 5. What we already have and should not ignore

Even though we still lack several Plane-style instance admin modules, our current admin area is not weak in every area.

We already have:

- dedicated user management
- dedicated audit log
- organization profile/contact
- security:
  - 2FA
  - password change
  - IP whitelist

This means:

- we should not rebuild admin from scratch
- we should evolve it by:
  - preserving the current internal admin area
  - adding a new `instance/system administration` layer

## 6. Recommended admin priorities to add next

### Priority 1

- Instance General Settings
  - system name
  - admin email
  - logo/system branding
  - telemetry toggle

- Authentication Management
  - enable/disable sign-up
  - enable/disable login methods
  - provider credential configuration pages

- Email/SMTP Management
  - host
  - port
  - username
  - security mode
  - test email

### Priority 2

- Workspace/Space Administration
  - workspace list
  - workspace creation
  - disable/archive workspace

- AI Administration
  - provider
  - model
  - API key status
  - module-level enable/disable

### Priority 3

- Storage/Image Administration
- Feature flags
- System health / admin diagnostics

## 7. Important boundary when learning from Plane

We should not copy Plane blindly. We should borrow the structure:

- separate admin layers
- clear modular sections
- one page + one form + one service/API per admin module
- a dedicated admin shell

But we must still preserve the product-specific business areas of our own system:

- departments
- gamification / rewards
- task/project workflow specifics

## 8. Final conclusion

Our current admin area is solid at the "internal operational administration" level, but it still lacks a lot if compared to Plane at the "instance administration" level.

The biggest missing clusters are:

1. Authentication admin
2. Email/SMTP admin
3. Instance general settings
4. Workspace provisioning
5. AI/image/system settings

If only one cluster should be added first, the best choice is:

- `Instance General + Authentication + Email`

This cluster delivers the clearest system-level administrative value and also reflects the most obvious Plane capabilities that we do not yet have.
