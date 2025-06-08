# 🚀 Real-time Chat Application - Complete Setup Guide

## ✅ HOÀN THÀNH - Tích hợp SignalR & Firebase

Ứng dụng chat thời gian thực đã được tích hợp đầy đủ với:
- **SignalR** cho real-time messaging
- **Firebase Cloud Messaging** cho push notifications
- **REST APIs** cho client integration
- **Demo client** hoàn chỉnh

## 🏗️ Kiến trúc hệ thống

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Web Client    │    │  Mobile Client  │    │   Admin Panel   │
│  (HTML/JS)      │    │  (iOS/Android)  │    │    (React)      │
└─────┬───────────┘    └─────┬───────────┘    └─────┬───────────┘
      │                      │                      │
      └──────────────────────┼──────────────────────┘
                             │
         ┌─────────────────────┴─────────────────────┐
         │              ASP.NET Core Server          │
         │  ┌─────────────┐  ┌─────────────────────┐ │
         │  │   SignalR   │  │      REST APIs      │ │
         │  │   ChatHub   │  │    Controllers     │ │
         │  └─────────────┘  └─────────────────────┘ │
         │  ┌─────────────┐  ┌─────────────────────┐ │
         │  │   Services  │  │  Firebase Service   │ │
         │  │ (Business)  │  │ (Push Notifications)│ │
         │  └─────────────┘  └─────────────────────┘ │
         └─────────────┬─────────────────────────────┘
                       │
         ┌─────────────┴─────────────┐
         │      PostgreSQL DB        │
         │  (Users, Messages, etc.)  │
         └───────────────────────────┘
```

## 🚀 Khởi động nhanh

### 1. Chạy Server
```bash
# Cách 1: Sử dụng script
start-server.bat

# Cách 2: Manual
cd "d:\Document\Project\Mạng căn bản\Server\Server(1)"
dotnet run --urls "https://localhost:7092;http://localhost:5237"
```

### 2. Truy cập Demo
- **Chat Client**: http://localhost:5237/chat-client.html
- **Swagger API**: https://localhost:7092/swagger
- **SignalR Hub**: ws://localhost:5237/chathub

## 📋 Tính năng đã tích hợp

### ✅ SignalR Real-time Features
- ✅ Send/Receive messages in real-time
- ✅ Typing indicators (StartTyping/StopTyping)
- ✅ User online/offline status
- ✅ Join/Leave chatroom notifications
- ✅ Message read receipts
- ✅ Connection tracking and management
- ✅ Auto-reconnection handling

### ✅ Firebase Push Notifications
- ✅ Automatic FCM when sending messages
- ✅ Device token management
- ✅ Broadcast notifications to chatrooms
- ✅ Test endpoints for Firebase integration
- ✅ Metadata support for rich notifications

### ✅ REST API Integration
- ✅ Complete CRUD operations for messages
- ✅ Chatroom management APIs
- ✅ User and friend management
- ✅ SignalR integration in all endpoints
- ✅ Comprehensive error handling

### ✅ Demo Client
- ✅ Beautiful, responsive UI
- ✅ Full SignalR integration
- ✅ Real-time features demonstration
- ✅ Connection status indicators
- ✅ Message history and typing indicators

## 🔧 Cấu hình

### Database Connection
```json
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=ChatAppDB;Username=postgres;Password=yourpassword"
  }
}
```

### CORS Settings (Program.cs)
```csharp
builder.WithOrigins(
    "http://localhost:5237",   // Server HTTP
    "https://localhost:7092",  // Server HTTPS  
    "http://localhost:3000",   // React/Angular
    "http://127.0.0.1:5500"    // VS Code Live Server
)
```

## 📡 API Endpoints

### Messages
- `POST /api/messages` - Send message
- `GET /api/messages/{id}` - Get message by ID
- `GET /api/messages/chatrooms/{chatroomId}` - Get chatroom messages
- `PUT /api/messages/{id}` - Edit message
- `DELETE /api/messages/{id}` - Delete message

### Firebase Testing
- `POST /api/messages/test-firebase` - Test single notification
- `POST /api/messages/chatrooms/{chatroomId}/broadcast-notification` - Broadcast to chatroom

### Chatrooms
- `GET /api/chatrooms` - List all chatrooms
- `POST /api/chatrooms` - Create chatroom
- `GET /api/chatrooms/{id}` - Get chatroom details
- `PUT /api/chatrooms/{id}` - Update chatroom
- `DELETE /api/chatrooms/{id}` - Delete chatroom

### Users & Device Tokens
- `PUT /api/devicetoken` - Update device token
- `GET /api/users` - List users
- `POST /api/users` - Create user

## 🔄 SignalR Events

### Client → Server
```javascript
// Join chatroom
connection.invoke("JoinChatroom", chatroomId);

// Send message
connection.invoke("SendMessage", chatroomId, userId, message);

// Typing indicators
connection.invoke("StartTyping", chatroomId, userId);
connection.invoke("StopTyping", chatroomId, userId);

// Leave chatroom
connection.invoke("LeaveChatroom", chatroomId);
```

### Server → Client
```javascript
// Receive messages
connection.on("ReceiveMessage", (messageData) => { });

// Typing indicators
connection.on("UserStartedTyping", (chatroomId, userId, userName) => { });
connection.on("UserStoppedTyping", (chatroomId, userId) => { });

// User status
connection.on("UserJoined", (chatroomId, userId, userName) => { });
connection.on("UserLeft", (chatroomId, userId, userName) => { });

// Message operations
connection.on("MessageEdited", (messageData) => { });
connection.on("MessageDeleted", (messageData) => { });

// Notifications
connection.on("BroadcastNotification", (notificationData) => { });
```

## 🔔 Firebase Integration

### Device Token Flow
```javascript
// 1. Get FCM token on client
const token = await getToken(messaging, { vapidKey: 'YOUR_VAPID_KEY' });

// 2. Send to server
await fetch('/api/devicetoken', {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ userId: userId, deviceToken: token })
});

// 3. Server automatically sends notifications for new messages
```

### Notification Payload
```json
{
  "notification": {
    "title": "John in General Chat",
    "body": "Hello everyone!"
  },
  "data": {
    "chatroomId": "1",
    "senderId": "123",
    "createdAt": "2025-06-08T10:30:00Z",
    "type": "new_message"
  }
}
```

## 🧪 Testing

### 1. Test SignalR Connection
```javascript
// Mở chat-client.html và:
// 1. Connect to SignalR hub
// 2. Join a chatroom
// 3. Send messages
// 4. Test typing indicators
// 5. Verify real-time updates
```

### 2. Test Firebase Notifications
```bash
# Test endpoint với device token thật
curl -X POST "https://localhost:7092/api/messages/test-firebase" \
  -H "Content-Type: application/json" \
  -d '{
    "deviceToken": "REAL_DEVICE_TOKEN",
    "title": "Test Firebase",
    "body": "Notification test successful"
  }'
```

### 3. Test Full Flow
```javascript
// 1. Register device token
// 2. Join chatroom via SignalR
// 3. Send message via API or SignalR
// 4. Verify:
//    - Real-time delivery via SignalR
//    - Push notification via Firebase
//    - Database persistence
```

## 📁 File Structure
```
Server(1)/
├── Controllers/
│   ├── MessagesController.cs      ✅ SignalR + Firebase
│   ├── ChatroomsController.cs     ✅ SignalR integration
│   └── DeviceTokenController.cs   ✅ Firebase token mgmt
├── Hubs/
│   └── ChatHub.cs                 ✅ Complete SignalR hub
├── Services/
│   ├── MessageService.cs          ✅ FCM integration
│   ├── FirebaseNotificationService.cs ✅ FCM service
│   └── ConnectionTrackingService.cs   ✅ Connection mgmt
├── wwwroot/
│   ├── chat-client.html           ✅ Demo client
│   └── js/chat-client.js          ✅ SignalR integration
├── README-SignalR.md              ✅ SignalR documentation
├── README-Firebase.md             ✅ Firebase documentation
└── start-server.bat               ✅ Quick start script
```

## 🎯 Các bước tiếp theo

### 1. Production Deployment
- [ ] Setup Firebase project thật và service account
- [ ] Configure environment variables
- [ ] Setup SSL certificates
- [ ] Database migration cho production

### 2. Mobile Client Development
- [ ] Implement FCM trong Android/iOS apps
- [ ] Integrate SignalR client SDKs
- [ ] Test push notifications end-to-end

### 3. Performance Optimization
- [ ] Implement message pagination
- [ ] Add Redis cache cho session management  
- [ ] Setup load balancing cho SignalR

### 4. Security Enhancements
- [ ] Add authentication/authorization
- [ ] Implement rate limiting
- [ ] Add message encryption

## 🆘 Troubleshooting

### Common Issues:
1. **SignalR connection fails**: Check CORS configuration
2. **Firebase notifications fail**: Verify service account file
3. **Database connection issues**: Check PostgreSQL setup
4. **Port conflicts**: Ensure ports 5237/7092 are available

### Debug Commands:
```bash
# Check server logs
dotnet run --verbosity detailed

# Test database connection
dotnet ef database update

# Verify Firebase setup
# Check console logs trong FirebaseNotificationService
```

## 📞 Support

Hệ thống hiện đã hoàn chỉnh với đầy đủ tính năng real-time messaging và push notifications. 

**Status**: ✅ READY FOR TESTING & DEPLOYMENT

---
*Last updated: June 8, 2025*
