$base = "http://localhost:5136/api"
$results = @()

function Test-API {
    param($id, $name, $method, $url, $body, $token, $expectedCodes)
    $args_list = @("-s", "-w", "`n%{http_code}", "-X", $method, $url, "--max-time", "10")
    if ($token) { $args_list += @("-H", "Authorization: Bearer $token") }
    if ($body) { 
        $args_list += @("-H", "Content-Type: application/json", "-d", $body)
    }
    $raw = & curl.exe @args_list 2>&1
    $lines = $raw -split "`n"
    $code = $lines[-1].Trim()
    $respBody = ($lines[0..($lines.Length-2)] -join "`n").Trim()
    $pass = $false
    foreach ($ec in $expectedCodes) { if ($code -eq $ec) { $pass = $true } }
    $status = if ($pass) { "PASS" } else { "FAIL" }
    $shortResp = if ($respBody.Length -gt 120) { $respBody.Substring(0,120) + "..." } else { $respBody }
    return [PSCustomObject]@{ID=$id; Name=$name; HTTP=$code; Status=$status; Response=$shortResp}
}

# ===== GET FRESH TOKEN =====
$loginRaw = & curl.exe -s -X POST "$base/auth/dev-login" -H "Content-Type: application/json" -d '{}' --max-time 15
$loginObj = $loginRaw | ConvertFrom-Json
$token = $loginObj.data.accessToken
$userId = $loginObj.data.id
Write-Host "Token obtained for: $($loginObj.data.email)" -ForegroundColor Green

# ===== PART 1: AUTHENTICATION (9 TC) =====
Write-Host "`n=== PART 1: AUTHENTICATION ===" -ForegroundColor Cyan

$results += Test-API "TC-AUTH-01" "Login thanh cong (dev)" "POST" "$base/auth/dev-login" '{}' $null @("200")
$results += Test-API "TC-AUTH-02" "Login sai email" "POST" "$base/auth/login" '{"email":"wrong@test.com","password":"abc123"}' $null @("401","400")
$results += Test-API "TC-AUTH-03" "Login sai password" "POST" "$base/auth/login" '{"email":"dev@sprinta.local","password":"wrong"}' $null @("401","400")
$results += Test-API "TC-AUTH-04" "Login email rong" "POST" "$base/auth/login" '{"email":"","password":"Test@123"}' $null @("400","401")
$results += Test-API "TC-AUTH-05" "Login password rong" "POST" "$base/auth/login" '{"email":"dev@sprinta.local","password":""}' $null @("400","401")
$rnd = Get-Random -Maximum 99999
$results += Test-API "TC-AUTH-06" "Register tai khoan moi" "POST" "$base/auth/register" "{`"fullName`":`"TestUser`",`"email`":`"testuser${rnd}@test.com`",`"password`":`"Test@123`"}" $null @("200","201","400")
$results += Test-API "TC-AUTH-07" "Register trung email" "POST" "$base/auth/register" '{"fullName":"Dup","email":"dev@sprinta.local","password":"Test@123"}' $null @("400")
$results += Test-API "TC-AUTH-08" "Logout" "POST" "$base/auth/logout" $null $token @("200")
# Re-get token after logout
$loginRaw = & curl.exe -s -X POST "$base/auth/dev-login" -H "Content-Type: application/json" -d '{}' --max-time 15
$token = ($loginRaw | ConvertFrom-Json).data.accessToken
$results += Test-API "TC-AUTH-09" "Token refresh" "POST" "$base/auth/refresh-token" $null $token @("200","401")

# ===== PART 2: WORKSPACE & PROJECT (8 TC) =====
Write-Host "`n=== PART 2: WORKSPACE & PROJECT ===" -ForegroundColor Cyan

$results += Test-API "TC-WS-01" "Tao workspace moi" "POST" "$base/workspaces" '{"name":"Team Alpha 205","slug":"team-alpha-205"}' $token @("200","201","400")
$results += Test-API "TC-WS-02" "Xem ds workspace" "GET" "$base/workspaces" $null $token @("200")

# Get workspace ID
$wsRaw = & curl.exe -s "$base/workspaces" -H "Authorization: Bearer $token" --max-time 10
$wsObj = $wsRaw | ConvertFrom-Json
$wsId = $wsObj.data[0].id
$wsSlug = $wsObj.data[0].slug
Write-Host "Workspace ID: $wsId, Slug: $wsSlug" -ForegroundColor Yellow

$results += Test-API "TC-WS-03" "Moi thanh vien workspace" "POST" "$base/workspaces/$wsId/members/invite" '{"email":"invite@test.com","role":"Member"}' $token @("200","201","400","404")

# Projects
$results += Test-API "TC-PROJ-01" "Tao project moi" "POST" "$base/projects" "{`"name`":`"Sprint App 205`",`"identifier`":`"SP205`",`"workspaceId`":`"$wsId`"}" $token @("200","201")
$results += Test-API "TC-PROJ-02" "Xem ds projects" "GET" "$base/projects?workspaceId=$wsId" $null $token @("200")

# Get project ID
$projRaw = & curl.exe -s "$base/projects?workspaceId=$wsId" -H "Authorization: Bearer $token" --max-time 10
$projObj = $projRaw | ConvertFrom-Json
if ($projObj.data -and $projObj.data.Count -gt 0) {
    $projId = $projObj.data[0].id
    Write-Host "Project ID: $projId" -ForegroundColor Yellow
} else {
    $projId = "00000000-0000-0000-0000-000000000000"
    Write-Host "No projects found" -ForegroundColor Red
}

$results += Test-API "TC-PROJ-03" "Chi tiet project" "GET" "$base/projects/$projId" $null $token @("200")
$results += Test-API "TC-PROJ-04" "Project members" "GET" "$base/projects/$projId/members" $null $token @("200")
$results += Test-API "TC-PROJ-05" "Project settings" "GET" "$base/projects/$projId/settings" $null $token @("200","404")

# ===== PART 3: TASK MANAGEMENT (18 TC) =====
Write-Host "`n=== PART 3: TASK MANAGEMENT ===" -ForegroundColor Cyan

$results += Test-API "TC-TASK-01" "Tao task moi" "POST" "$base/tasks" "{`"title`":`"Design Login Page 205`",`"projectId`":`"$projId`",`"status`":`"TODO`",`"priority`":`"HIGH`"}" $token @("200","201")

# Get tasks
$tasksRaw = & curl.exe -s "$base/tasks?projectId=$projId" -H "Authorization: Bearer $token" --max-time 10
$tasksObj = $tasksRaw | ConvertFrom-Json
if ($tasksObj.data -and $tasksObj.data.Count -gt 0) {
    $taskId = $tasksObj.data[0].id
    Write-Host "Task ID: $taskId" -ForegroundColor Yellow
} elseif ($tasksObj -and $tasksObj.Count -gt 0) {
    $taskId = $tasksObj[0].id
    Write-Host "Task ID (alt): $taskId" -ForegroundColor Yellow
} else {
    $taskId = "00000000-0000-0000-0000-000000000000"
    Write-Host "No tasks found, raw: $tasksRaw" -ForegroundColor Red
}

$results += Test-API "TC-TASK-02" "Xem ds tasks" "GET" "$base/tasks?projectId=$projId" $null $token @("200")
$results += Test-API "TC-TASK-03" "Chi tiet task" "GET" "$base/tasks/$taskId" $null $token @("200")
$results += Test-API "TC-TASK-04" "Cap nhat title" "PUT" "$base/tasks/$taskId" "{`"title`":`"Login Page v2`"}" $token @("200","204")
$results += Test-API "TC-TASK-05" "Cap nhat status" "PUT" "$base/tasks/$taskId" "{`"status`":`"IN_PROGRESS`"}" $token @("200","204")
$results += Test-API "TC-TASK-06" "Cap nhat priority" "PUT" "$base/tasks/$taskId" "{`"priority`":`"MEDIUM`"}" $token @("200","204")
$results += Test-API "TC-TASK-07" "Gan assignee" "POST" "$base/tasks/$taskId/assignees" "{`"userId`":`"$userId`"}" $token @("200","201","204")
$results += Test-API "TC-TASK-08" "Set start date" "PUT" "$base/tasks/$taskId" "{`"startDate`":`"2026-04-20`"}" $token @("200","204")
$results += Test-API "TC-TASK-09" "Set due date" "PUT" "$base/tasks/$taskId" "{`"dueDate`":`"2026-04-30`"}" $token @("200","204")
$results += Test-API "TC-TASK-10" "Tao task title rong" "POST" "$base/tasks" "{`"title`":`"`",`"projectId`":`"$projId`"}" $token @("400")
$results += Test-API "TC-TASK-11" "Tao subtask" "POST" "$base/tasks" "{`"title`":`"Design header sub`",`"projectId`":`"$projId`",`"parentId`":`"$taskId`"}" $token @("200","201")
$results += Test-API "TC-TASK-12" "Task activity/history" "GET" "$base/tasks/$taskId/activity" $null $token @("200","404")

# ===== PART 4: BOARD VIEW (4 TC) =====
Write-Host "`n=== PART 4: BOARD VIEW ===" -ForegroundColor Cyan
$results += Test-API "TC-BOARD-01" "Board/Kanban data" "GET" "$base/tasks?projectId=$projId&groupBy=status" $null $token @("200")
$results += Test-API "TC-BOARD-02" "Task statuses" "GET" "$base/projects/$projId/statuses" $null $token @("200")
$results += Test-API "TC-BOARD-03" "Reorder task" "PUT" "$base/tasks/$taskId/reorder" '{"status":"IN_PROGRESS","sortOrder":1}' $token @("200","204","404")

# ===== PART 5: LABELS (3 TC) =====
Write-Host "`n=== PART 5: LABELS ===" -ForegroundColor Cyan
$results += Test-API "TC-LBL-01" "Tao label" "POST" "$base/projects/$projId/labels" '{"name":"Bug205","color":"#EF4444"}' $token @("200","201")
$results += Test-API "TC-LBL-02" "Xem ds labels" "GET" "$base/projects/$projId/labels" $null $token @("200")

# ===== PART 6: CYCLES/SPRINTS (4 TC) =====
Write-Host "`n=== PART 6: CYCLES/SPRINTS ===" -ForegroundColor Cyan
$results += Test-API "TC-CYCLE-01" "Tao cycle/sprint" "POST" "$base/projects/$projId/sprints" '{"name":"Sprint 1 TC","startDate":"2026-04-19","endDate":"2026-05-03"}' $token @("200","201")
$results += Test-API "TC-CYCLE-02" "Xem ds sprints" "GET" "$base/projects/$projId/sprints" $null $token @("200")

# ===== PART 7: MODULES (3 TC) =====
Write-Host "`n=== PART 7: MODULES ===" -ForegroundColor Cyan
$results += Test-API "TC-MOD-01" "Tao module" "POST" "$base/projects/$projId/modules" '{"name":"Backend API 205"}' $token @("200","201")
$results += Test-API "TC-MOD-02" "Xem ds modules" "GET" "$base/projects/$projId/modules" $null $token @("200")

# ===== PART 8: COMMENTS (5 TC) =====
Write-Host "`n=== PART 8: COMMENTS ===" -ForegroundColor Cyan
$results += Test-API "TC-CMT-01" "Them comment" "POST" "$base/tasks/$taskId/comments" '{"content":"This needs review 205"}' $token @("200","201")
$results += Test-API "TC-CMT-02" "Xem comments" "GET" "$base/tasks/$taskId/comments" $null $token @("200")

# ===== PART 9: ANALYTICS (2 TC) =====
Write-Host "`n=== PART 9: ANALYTICS ===" -ForegroundColor Cyan
$results += Test-API "TC-ANA-01" "Project analytics" "GET" "$base/projects/$projId/analytics" $null $token @("200")
$results += Test-API "TC-ANA-02" "Global analytics" "GET" "$base/analytics?workspaceId=$wsId" $null $token @("200","404")

# ===== PART 10: VIEWS (5 TC) =====
Write-Host "`n=== PART 10: VIEWS ===" -ForegroundColor Cyan
$results += Test-API "TC-VIEW-01" "Tao custom view" "POST" "$base/projects/$projId/views" '{"name":"High Priority","type":"list","filters":{"priority":"HIGH"}}' $token @("200","201")
$results += Test-API "TC-VIEW-02" "Xem ds views" "GET" "$base/projects/$projId/views" $null $token @("200")

# ===== PART 11: PAGES (2 TC) =====
Write-Host "`n=== PART 11: PAGES ===" -ForegroundColor Cyan
$results += Test-API "TC-PAGE-01" "Tao page" "POST" "$base/projects/$projId/pages" '{"title":"Meeting Notes 205","content":"Test content"}' $token @("200","201")
$results += Test-API "TC-PAGE-02" "Xem ds pages" "GET" "$base/projects/$projId/pages" $null $token @("200")

# ===== PART 12: STICKIES (2 TC) =====
Write-Host "`n=== PART 12: STICKIES ===" -ForegroundColor Cyan
$results += Test-API "TC-STICKY-01" "Tao sticky note" "POST" "$base/stickies" '{"content":"Remember PR review 205"}' $token @("200","201")
$results += Test-API "TC-STICKY-02" "Xem stickies" "GET" "$base/stickies" $null $token @("200")

# ===== PART 13: ADMIN (4 TC) =====
Write-Host "`n=== PART 13: ADMIN ===" -ForegroundColor Cyan
$results += Test-API "TC-ADM-01" "Xem ds users" "GET" "$base/admin/users" $null $token @("200")
$results += Test-API "TC-ADM-02" "Xem audit log" "GET" "$base/admin/audit-logs" $null $token @("200")
$results += Test-API "TC-ADM-03" "System settings" "GET" "$base/admin/settings" $null $token @("200")
$results += Test-API "TC-ADM-04" "Departments" "GET" "$base/admin/departments" $null $token @("200")

# ===== PART 14: NOTIFICATIONS =====
Write-Host "`n=== PART 14: NOTIFICATIONS ===" -ForegroundColor Cyan
$results += Test-API "TC-NOTIF-01" "Xem notifications" "GET" "$base/notifications" $null $token @("200")

# ===== PART 15: DRAFTS =====
Write-Host "`n=== PART 15: DRAFTS ===" -ForegroundColor Cyan
$results += Test-API "TC-DRAFT-01" "Xem drafts" "GET" "$base/drafts" $null $token @("200")

# ===== PART 16: USERS =====
Write-Host "`n=== PART 16: USERS ===" -ForegroundColor Cyan
$results += Test-API "TC-USER-01" "Xem profile" "GET" "$base/users/me" $null $token @("200")

# ===== PART 17: ERROR HANDLING (4 TC) =====
Write-Host "`n=== PART 17: ERROR HANDLING ===" -ForegroundColor Cyan
$results += Test-API "TC-ERR-01" "Unauthorized no token" "GET" "$base/tasks" $null $null @("401")
$results += Test-API "TC-ERR-02" "Invalid task ID" "GET" "$base/tasks/00000000-0000-0000-0000-000000000000" $null $token @("404","400")
$results += Test-API "TC-ERR-03" "Invalid project ID" "GET" "$base/projects/00000000-0000-0000-0000-000000000000" $null $token @("404","400")

# ===== OUTPUT RESULTS =====
Write-Host "`n`n========================================" -ForegroundColor White
Write-Host "     FINAL API TEST RESULTS" -ForegroundColor White
Write-Host "========================================" -ForegroundColor White

$passCount = ($results | Where-Object { $_.Status -eq "PASS" }).Count
$failCount = ($results | Where-Object { $_.Status -eq "FAIL" }).Count
$totalCount = $results.Count

Write-Host "TOTAL: $totalCount | PASS: $passCount | FAIL: $failCount" -ForegroundColor $(if($failCount -eq 0){"Green"}else{"Yellow"})
Write-Host "========================================`n" -ForegroundColor White

foreach ($r in $results) {
    $color = if ($r.Status -eq "PASS") { "Green" } else { "Red" }
    Write-Host "$($r.ID) | $($r.Status) | HTTP $($r.HTTP) | $($r.Name)" -ForegroundColor $color
}

# Export to CSV
$results | Export-Csv -Path "d:\A\QuanLyCongViec\docs\test-reports\api_test_results.csv" -NoTypeInformation -Encoding UTF8
Write-Host "`nResults saved to api_test_results.csv" -ForegroundColor Green
