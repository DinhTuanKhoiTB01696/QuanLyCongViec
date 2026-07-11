@echo off
title Start Task Management System

echo =======================================
echo KHỞI ĐỘNG HỆ THỐNG TASK MANAGEMENT
echo =======================================

echo.
set /p resetDB="WARNING: Thao tao nay se xoa toan bo du lieu hien tai va nap lai du lieu demo. Ban co muon tiep tuc? (Y/N): "
if /I "%resetDB%"=="Y" goto RESET_DB
goto NORMAL_START

:RESET_DB
echo.
echo --- DANG RESET DATABASE ---
call :STOP_EXISTING_BACKEND
cd Backend\src\TaskManagement.API

echo Restoring NuGet packages...
dotnet restore
if errorlevel 1 (
    echo NuGet package restore that bai.
    pause
    exit /b 1
)

echo 1. Drop Database cu...
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\run-sql.ps1" -Server "KHOI\SQLEXPRESS" -Database "master" -Query "IF DB_ID('TaskManagementDB') IS NOT NULL BEGIN ALTER DATABASE [TaskManagementDB] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [TaskManagementDB]; END"
if errorlevel 1 (
    echo Drop database that bai.
    pause
    exit /b 1
)

echo 2. Cap nhat Database bang migrations...
dotnet ef database update --project ../TaskManagement.Infrastructure --startup-project .
if errorlevel 1 (
    echo Cap nhat database that bai.
    pause
    exit /b 1
)

echo 4. Dang nap demo data doanh nghiep cho admin dev@sprinta.local...
if exist "%~dp0scripts\seed-demo-data.sql" (
    powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\run-sql.ps1" -Server "KHOI\SQLEXPRESS" -Database "TaskManagementDB" -InputFile "%~dp0scripts\seed-demo-data.sql"
    if errorlevel 1 (
        echo Seed demo data that bai.
        pause
        exit /b 1
    )
) else (
    echo Khong tim thay scripts\seed-demo-data.sql, bo qua demo seed.
)

cd ..\..\..
echo --- RESET DATABASE THANH CONG ---
echo.
goto START_APP


:NORMAL_START
echo.
echo --- KIEM TRA VA TAO DATABASE NEU CHUA CO ---
call :STOP_EXISTING_BACKEND
cd Backend\src\TaskManagement.API

echo Restoring NuGet packages...
dotnet restore
if errorlevel 1 (
    echo NuGet package restore that bai.
    pause
    exit /b 1
)

if not exist "..\TaskManagement.Infrastructure\Migrations" (
    echo Chua co Migrations, dang tao moi de chuan bi tao DB...
    dotnet ef migrations add InitialCreate --project ../TaskManagement.Infrastructure --startup-project .
)

echo Cap nhat / Tao moi Database...
dotnet ef database update --project ../TaskManagement.Infrastructure --startup-project .
if errorlevel 1 (
    echo Cap nhat database that bai. Neu database cu dang lech migration, hay chay lai run.bat va chon Y de reset.
    pause
    exit /b 1
)

echo Dang nap demo data cho admin dev@sprinta.local...
if exist "%~dp0scripts\seed-demo-data.sql" (
    powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\run-sql.ps1" -Server "KHOI\SQLEXPRESS" -Database "TaskManagementDB" -InputFile "%~dp0scripts\seed-demo-data.sql"
    if errorlevel 1 (
        echo Seed demo data that bai.
        pause
        exit /b 1
    )
) else (
    echo Khong tim thay scripts\seed-demo-data.sql, bo qua demo seed.
)

cd ..\..\..
echo --- HOAN TAT KIEM TRA ---
echo.
goto START_APP


:START_APP
echo Tu dong kiem tra va sinh PWA Icons neu chua co...
if not exist "Frontend\public\pwa-192x192.png" (
    powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\generate-icons.ps1"
)

echo 1. Khoi dong Backend (.NET Web API)...
start "Backend API" cmd /k "cd Backend\src\TaskManagement.API && title Backend API && dotnet run --launch-profile https"

echo 2. Khoi dong Frontend (Vue 3)...
start "Frontend Vue" cmd /k "cd Frontend && title Frontend Vue && echo Kiem tra va cap nhat frontend dependencies... && npm install --legacy-peer-deps && npm run dev"

echo Da gui lenh khoi dong cho ca Backend va Frontend o cac cua so rieng biet!
echo =======================================
goto :eof

:STOP_EXISTING_BACKEND
echo Dang kiem tra backend cu tren cong 5136/7033...
powershell -NoProfile -ExecutionPolicy Bypass -Command "$ports = 5136,7033; $ids = Get-NetTCPConnection -State Listen -ErrorAction SilentlyContinue | Where-Object { $ports -contains $_.LocalPort } | Select-Object -ExpandProperty OwningProcess -Unique; foreach ($id in $ids) { if ($id -and $id -ne $PID) { Stop-Process -Id $id -Force -ErrorAction SilentlyContinue; Write-Host ('Da dung backend cu PID ' + $id) } }"
exit /b 0
