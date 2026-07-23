param(
    [string]$Repo = "C:\Users\phucl\OneDrive\Desktop\QLCV_2\QuanLyCongViec",
    [string]$RemoteBranch = "origin/main"
)

$ErrorActionPreference = "Stop"
Set-Location $Repo

$stamp = Get-Date -Format "yyyyMMdd_HHmmss"
$out = Join-Path $Repo "artifacts\reconciliation\$stamp"
New-Item -ItemType Directory -Force $out | Out-Null

git branch --show-current | Out-File "$out\branch.txt"
git rev-parse HEAD | Out-File "$out\head.txt"
git status --short | Out-File "$out\status-before.txt"
git diff --binary | Out-File "$out\working-tree.patch" -Encoding utf8
git diff --cached --binary | Out-File "$out\staged.patch" -Encoding utf8
git ls-files --others --exclude-standard | Out-File "$out\untracked.txt"

git fetch origin --prune --tags

git remote show origin | Out-File "$out\remote.txt"
git branch -a -vv | Out-File "$out\branches.txt"
git rev-list --left-right --count "HEAD...$RemoteBranch" | Out-File "$out\ahead-behind.txt"
git log --left-right --cherry-pick --oneline "HEAD...$RemoteBranch" | Out-File "$out\commits.txt"
git diff --name-status "HEAD..$RemoteBranch" | Out-File "$out\remote-vs-head.txt"
git diff --name-status "$RemoteBranch" | Out-File "$out\remote-vs-worktree.txt"

$forbidden = @(
  "Backend/src/TaskManagement.API/Program.cs",
  "Backend/src/TaskManagement.API/appsettings.json",
  "Frontend/src/views/AIPage.vue",
  "Frontend/src/views/ProjectSettings.vue"
)

"DO NOT AUTO-RESTORE THESE FILES:" | Out-File "$out\REVIEW_REQUIRED.txt"
$forbidden | Out-File "$out\REVIEW_REQUIRED.txt" -Append

Write-Host "Comparison created at $out"
Write-Host "No merge, pull, restore, stage, commit or delete was performed."
