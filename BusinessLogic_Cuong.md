🎯 Module 1: Task Execution Core
This is the backbone of the project, handling all the complex algorithms related to the Task cards. You will directly code these critical guardrails in the Task Lifecycle subsystem:

State Machine Guardrails: Code the PUT /api/tasks/{id}/status API. Use the Position property of the statuses to calculate Math.Abs(). If the distance is > 1, throw a 400 Bad Request to prevent users from skipping statuses (e.g., jumping straight from "To Do" to "Done").

Parent-Subtask Constraint: Write a query to check when a Parent Task is moved to "Done". If db.WorkTasks.AnyAsync(...) == true (meaning there are still unfinished Subtasks), block the action and return an error.

Concurrency Control (Anti-Overwrite): Set up the [ConcurrencyCheck] or [Timestamp] attribute in Entity Framework for the Task entity. Catch the DbUpdateConcurrencyException and return a 409 Conflict. This completely resolves the issue of two developers dragging the same Task card and overwriting each other's database records.

Timezone Handling: Configure the core architecture so the Backend strictly saves all dates as DateTime.UtcNow. (The Frontend team will handle parsing the UTC time to the user's local timezone).

Task Dependencies / Blockers: Currently, the TacVuPhuThuoc (Task Dependencies) table exists, but no API checks it. In the PUT /api/tasks/{id}/status API, before moving a card to "In Progress" or "Done", you must query this table: var blockers = await db.TacVuPhuThuoc.Where(p => p.TacVuSauId == id && p.LoaiPhuThuoc == FinishToStart).Select(p => p.TacVuTruoc).ToListAsync();. If any prerequisite task is not "Done", immediately throw a 400 Bad Request.

Automatic Data Roll-up (Parent-Subtask): Do not rely on the Frontend to calculate total logged hours. Intercept the database layer using EF Core Interceptors or by overriding SaveChangesAsync(). Whenever a new record is inserted into NhatKyThoiGian (Time Log), automatically trigger a SUM of all hours for Subtasks sharing the same TacVuChaId (Parent Task ID), and directly UPDATE the TongThoiGianThucTe (Total Actual Time) column of the Parent Task.

Project Member Invitation & Role Assignment:

The Issue: The system needs a secure way for Project Managers to invite users to a workspace/project and assign them specific access levels (Project Roles), ensuring duplicate invitations are prevented.

.NET Dev Guide: * Target API: POST /api/projects/{projectId}/members

Database Logic: First, verify if the user exists in the NguoiDung (Users) table. If valid, insert a new record into the ThanhVienDuAn (Project Members) table with their specific VaiTroTrongDuAn (Role, e.g., "DEV", "TESTER").

Validation & Security: Implement an idempotency check (await db.ThanhVienDuAn.AnyAsync(...)) to throw a 409 Conflict if the user is already in the project. Strictly protect this API with your custom [ProjectAuthorize(Roles = "PM, Admin")] middleware so only authorized leaders can add members.

Orphan Tasks & Soft Delete Handling:

The Issue: When a member is removed from a project, their assigned tasks become "orphans" if not handled correctly, breaking the workflow.

.NET Dev Guide: * Target API: DELETE /api/projects/{projectId}/members/{userId}

Database Logic: STRICTLY DO NOT use the physical delete command db.ProjectMembers.Remove(). Instead, implement a soft delete: member.Status = 0; member.LeftAt = DateTime.UtcNow;.

Task Reassignment Trigger: Immediately run a follow-up command to clear the task assignments for that user: var orphans = db.TaskAssignments.Where(ta => ta.UserId == removedUserId); db.TaskAssignments.RemoveRange(orphans);. Finally, insert a record into the Notifications table to alert the Project Manager (PM) so they can reassign these orphan tasks to other developers.

🎯 Module 2: AI Integration
The automated "brain" of the system, handling Large Language Model interactions.

Secure AI API Key & Structured Output: Use HttpClient to call the OpenAI/Gemini API. Hide the Secret Key in appsettings.json or User Secrets. Setup the system_prompt to force the AI to return a strictly formatted JSON array. Then, use JsonSerializer.Deserialize<List<SubTaskDto>> to parse the JSON string and automatically insert the generated Subtasks into the WorkTasks table.

AI Quota & Rate Limiting (Kill Switch): Since LLM APIs charge by tokens, you must prevent abuse (e.g., spamming the "Breakdown Task" button).

Level 1 (Middleware): Use the built-in .NET 8 Microsoft.AspNetCore.RateLimiting. Apply [EnableRateLimiting("FixedWindow")] to the AI API (e.g., max 5 requests / user / minute). Exceeding this returns a 429 Too Many Requests.

Level 2 (Database Quota): Before making the HTTP call, query the ChiPhiAI (AI Costs) table to sum the user's tokens for the month. If SoTokenDaDung > Limit (e.g., 100,000 tokens), refuse the service immediately.

🎯 Module 3: Gamification Engine
Handles the "economy" and reward points of the system. This requires absolute transactional accuracy.

Gamification Integrity & Anti-Farming: Wrap the logic for adding points (when a Task is "Done") inside a BeginTransactionAsync. Implement an Idempotency check: db.PointTransactions.AnyAsync(...) to ensure a specific Task cannot be rewarded twice. Enforce a strict database rule that CurrentBalance can never drop below 0.

Point Rollback & Penalty: If a Task is moved to "Done" (points added), but QA later rejects it and moves it back to "To Do" or "Bug", the points must be reclaimed. In the status update API, check if oldStatus == "Done" and newStatus != "Done". Query LichSuGiaoDichDiem (Transaction History) to find the exact rewarded amount, open a Transaction, deduct that amount from ViDiemNguoiDung (User Wallet), and log the reason as "Point refund due to rejected task".

🎯 Module 4: Social Authentication (GitHub / Google Login)
Replaces the previous DevOps/Architecture module. This module handles modern, passwordless access for developers.

OAuth 2.0 Integration: Implement Google and GitHub authentication providers in .NET 8 using Microsoft.AspNetCore.Authentication.Google and AspNet.Security.OAuth.GitHub. Configure the Client IDs and Client Secrets securely in the server environment variables.

User Mapping & JWT Generation: When a user successfully authenticates via Google/GitHub, capture their email and profile picture. Check if the email exists in the NguoiDung (User) table. If it's a new user, auto-register them. Finally, generate and return the standard system AccessToken (JSON body) and RefreshToken (HttpOnly Cookie) so the Vue.js frontend can seamlessly log them in without needing a separate password.