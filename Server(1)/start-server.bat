@echo off
echo Starting Chat Server with SignalR and Firebase integration...
cd /d "d:\Document\Project\Mạng căn bản\Server\Server(1)"
echo.
echo Building project...
dotnet build
echo.
echo Starting server...
echo Server will be available at:
echo - HTTPS: https://localhost:7092
echo - HTTP: http://localhost:5237
echo - Chat Demo: http://localhost:5237/chat-client.html
echo - SignalR Hub: /chathub
echo - Swagger UI: https://localhost:7092/swagger
echo.
dotnet run --urls "https://localhost:7092;http://localhost:5237"
pause
