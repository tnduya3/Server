@echo off
echo =================================================
echo     AUTHENTICATION SYSTEM TEST SCRIPT
echo =================================================
echo.

echo 1. Building project...
cd /d "d:\Document\Project\Mạng căn bản\Server\Server(1)"
dotnet build --verbosity minimal

if %ERRORLEVEL% neq 0 (
    echo Build failed! Please check errors above.
    pause
    exit /b 1
)

echo.
echo 2. Starting server...
echo.
echo Server endpoints:
echo   - HTTPS API: https://localhost:7092
echo   - HTTP API:  http://localhost:5237  
echo   - Swagger:   https://localhost:7092/swagger
echo   - Chat Demo: http://localhost:5237/chat-client.html
echo.
echo Authentication endpoints:
echo   - POST /api/auth/login
echo   - POST /api/auth/signup
echo   - POST /api/auth/forgot-password
echo   - POST /api/auth/refresh-token
echo   - POST /api/auth/verify-token
echo   - POST /api/auth/logout
echo.
echo Test files available:
echo   - test-auth-api.http (Authentication tests)
echo   - test-api.http (Chat API tests)
echo.
echo Starting server... Press Ctrl+C to stop
echo.

dotnet run --urls "https://localhost:7092;http://localhost:5237"
