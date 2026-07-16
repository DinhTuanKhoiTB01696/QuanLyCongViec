@echo off
title Start Task Management System

echo =======================================
echo KHỞI ĐỘNG HỆ THỐNG TASK MANAGEMENT
echo =======================================

echo.
echo --- DANG DONG BACKEND CU NEU DANG CHAY DE TRANH LOCK DLL ---
powershell -NoProfile -ExecutionPolicy Bypass -Command "$repo = (Resolve-Path '%~dp0').Path.TrimEnd('\'); Get-CimInstance Win32_Process | Where-Object { $_.Name -eq 'dotnet.exe' -and $_.CommandLine -like '*TaskManagement.API.dll*' -and $_.CommandLine -like ('*' + $repo + '*') } | ForEach-Object { Stop-Process -Id $_.ProcessId -Force }"
if errorlevel 1 (
    echo Khong the dong backend cu. Hay dong cua so Backend API roi chay lai.
    pause
    exit /b 1
)

echo.
set /p resetDB="Ban co muon reset Database va chay Db Migrations + Seed Data khong? (Y/N): "
if /I "%resetDB%"=="Y" (
    echo.
    echo --- DANG RESET DATABASE ---
    cd Backend\src\TaskManagement.API
    
    echo Restoring NuGet packages...
    dotnet restore
    if errorlevel 1 (
        echo NuGet package restore that bai.
        pause
        exit /b 1
    )
    
    echo 1. Drop Database cu...
    powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\run-sql.ps1" -Server "KIETNGO" -Database "master" -Query "IF DB_ID('TaskManagementDB') IS NOT NULL BEGIN ALTER DATABASE [TaskManagementDB] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [TaskManagementDB]; END"
    if errorlevel 1 (
        echo Drop database that bai.
        pause
        exit /b 1
    )
    
    echo 2. Cap nhat Database bang migrations hien co...
    dotnet ef database update --project ../TaskManagement.Infrastructure --startup-project .
    if errorlevel 1 (
        echo Cap nhat database that bai.
        pause
        exit /b 1
    )
    
    echo 3. Dang nap demo data doanh nghiep cho admin dev@sprinta.local...
    if exist "%~dp0scripts\seed-demo-data.sql" (
        powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\run-sql.ps1" -Server "KIETNGO" -Database "TaskManagementDB" -InputFile "%~dp0scripts\seed-demo-data.sql"
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
) else (
    echo.
    echo --- KIEM TRA VA TAO DATABASE NẾU CHƯA CÓ ---
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
        powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\run-sql.ps1" -Server "KIETNGO" -Database "TaskManagementDB" -InputFile "%~dp0scripts\seed-demo-data.sql"
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
)

echo 1. Khởi động Backend (.NET Web API)...
start "Backend API" cmd /k "cd Backend\src\TaskManagement.API && title Backend API && dotnet run --launch-profile https"

echo 2. Khởi động Frontend (Vue 3)...
start "Frontend Vue" cmd /k "cd Frontend && title Frontend Vue && if not exist node_modules (echo Cai dat dependencies bang npm... && npm install) && npm run dev -- --host localhost --port 5173"

echo Da gui lenh khoi dong cho ca Backend va Frontend o cac cua so rieng biet!
echo =======================================
