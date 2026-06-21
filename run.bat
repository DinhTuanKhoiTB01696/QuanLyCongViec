@echo off
setlocal

cd /d "%~dp0"

title Start Task Management System

echo =======================================
echo KHOI DONG TASK MANAGEMENT
echo =======================================
echo.
echo Backend API:  http://localhost:5136
echo Frontend Vue: http://localhost:5173
echo.
echo Luu y: SQL Server hien dang khong ket noi duoc tren may nay,
echo backend se tu fallback sang InMemory va seed tai khoan dev/admin.
echo.

echo 1. Khoi dong Backend (.NET Web API)...
start "Backend API" cmd /k "cd /d ""%~dp0Backend\src\TaskManagement.API"" && title Backend API && dotnet run --launch-profile http"

echo 2. Khoi dong Frontend (Vue 3)...
start "Frontend Vue" cmd /k "cd /d ""%~dp0Frontend"" && title Frontend Vue && if not exist node_modules (echo Cai dat dependencies bang npm... && npm install) && npm run dev"

echo.
echo Da gui lenh khoi dong cho Backend va Frontend.
echo Dung cac tai khoan test:
echo   Admin: admin@example.com / Admin@123
echo   User moi: dang ky bang mat khau dang Test@123
echo.
echo Neu cua so backend bao "address already in use", hay tat cua so Backend API cu roi chay lai file nay.
echo =======================================

endlocal
