# 🔔 Firebase Push Notifications Integration

## Tổng quan
Hệ thống chat đã được tích hợp Firebase Cloud Messaging (FCM) để gửi push notifications khi có tin nhắn mới, ngay cả khi app không được mở.

## Tính năng Firebase Notifications

### ✅ Automatic Push Notifications
- 🔔 Gửi thông báo đẩy khi có tin nhắn mới
- 👥 Chỉ gửi cho participants trong chatroom (trừ người gửi)
- 📱 Hỗ trợ cả Android và iOS
- 🎯 Targeted notifications dựa trên device tokens

### ✅ Integration Points
1. **MessageService.SendMessageAsync()** - Tự động gửi FCM khi lưu tin nhắn
2. **ChatHub.SendMessage()** - Gửi FCM khi gửi qua SignalR
3. **MessagesController.SendMessage()** - Gửi FCM khi gửi qua REST API

## API Endpoints cho Firebase

### 1. Test Firebase Notification
```http
POST /api/messages/test-firebase
Content-Type: application/json

{
    "deviceToken": "DEVICE_TOKEN_HERE",
    "title": "Test Title",
    "body": "Test message body"
}
```

### 2. Broadcast Notification to Chatroom
```http
POST /api/messages/chatrooms/{chatroomId}/broadcast-notification
Content-Type: application/json

{
    "title": "Important Announcement",
    "body": "Meeting at 3 PM today!"
}
```

## Cấu hình Firebase

### 1. Setup Firebase Project
1. Tạo project tại [Firebase Console](https://console.firebase.google.com/)
2. Thêm app Android/iOS
3. Download `google-services.json` (Android) hoặc `GoogleService-Info.plist` (iOS)
4. Enable Cloud Messaging trong Firebase Console

### 2. Server Configuration
Đảm bảo file Firebase service account key đã được đặt đúng vị trí và `FirebaseNotificationService` được cấu hình:

```csharp
// Trong Program.cs
builder.Services.AddSingleton<IFirebaseNotificationService, FirebaseNotificationService>();
```

### 3. Client Setup

#### Android (Java/Kotlin)
```kotlin
// Thêm dependencies trong build.gradle
implementation 'com.google.firebase:firebase-messaging:23.1.0'

// Lấy FCM token
FirebaseMessaging.getInstance().token.addOnCompleteListener { task ->
    if (!task.isSuccessful) return@addOnCompleteListener
    
    val token = task.result
    Log.d("FCM", "Token: $token")
    
    // Gửi token lên server
    sendTokenToServer(token)
}
```

#### iOS (Swift)
```swift
// Thêm FCM
import FirebaseMessaging

// Lấy FCM token
Messaging.messaging().token { token, error in
    if let error = error {
        print("Error fetching FCM token: \(error)")
    } else if let token = token {
        print("FCM token: \(token)")
        // Gửi token lên server
        sendTokenToServer(token)
    }
}
```

#### Web (JavaScript)
```javascript
// Import Firebase SDK
import { getMessaging, getToken } from "firebase/messaging";

const messaging = getMessaging();
getToken(messaging, { vapidKey: 'YOUR_VAPID_KEY' }).then((currentToken) => {
    if (currentToken) {
        console.log('FCM Token:', currentToken);
        // Gửi token lên server
        sendTokenToServer(currentToken);
    }
});
```

## Message Flow với Firebase

### Khi gửi tin nhắn:
1. **Client gửi tin nhắn** → API/SignalR
2. **Server lưu tin nhắn** → Database
3. **Server tự động gửi FCM** → Tất cả participants (trừ người gửi)
4. **Server gửi SignalR** → Real-time update cho users đang online
5. **Mobile devices nhận push notification** → Users đang offline

### Firebase Notification Payload:
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

## Testing Firebase Notifications

### 1. Sử dụng Firebase Console
1. Mở Firebase Console → Cloud Messaging
2. Click "Send your first message"
3. Nhập title, body
4. Chọn target (device token hoặc topic)
5. Send

### 2. Sử dụng API Test Endpoint
```bash
curl -X POST "https://localhost:7092/api/messages/test-firebase" \
-H "Content-Type: application/json" \
-d '{
    "deviceToken": "YOUR_DEVICE_TOKEN",
    "title": "Test Notification",
    "body": "This is a test from API"
}'
```

### 3. Test với Postman
Import collection để test:
```json
{
    "info": {
        "name": "Firebase Chat Notifications"
    },
    "item": [
        {
            "name": "Test Firebase Notification",
            "request": {
                "method": "POST",
                "header": [
                    {
                        "key": "Content-Type",
                        "value": "application/json"
                    }
                ],
                "body": {
                    "raw": "{\n    \"deviceToken\": \"DEVICE_TOKEN_HERE\",\n    \"title\": \"Test Title\",\n    \"body\": \"Test message body\"\n}"
                },
                "url": {
                    "raw": "https://localhost:7092/api/messages/test-firebase"
                }
            }
        }
    ]
}
```

## Troubleshooting

### 1. FCM Token Issues
- **Token null/empty**: App chưa được cấp permission notifications
- **Token invalid**: Token đã expire, cần refresh
- **Token không nhận được**: Kiểm tra Firebase project setup

### 2. Notifications không đến
- Kiểm tra device online/offline
- Kiểm tra app có đang chạy foreground không
- Xem logs server để đảm bảo FCM được gửi
- Kiểm tra Firebase Console → Cloud Messaging → Reports

### 3. Server Logs
```bash
# Xem logs để debug
dotnet run

# Logs sẽ hiển thị:
# "Sent FCM to Username (deviceToken)"
# "Message sent successfully - ID: 123, Firebase notifications sent"
```

## Advanced Features

### 1. Notification Topics
Thay vì gửi đến từng device token, có thể subscribe devices vào topics:

```csharp
// Subscribe user to chatroom topic
await _firebaseNotificationService.SubscribeToTopicAsync(deviceToken, $"chatroom_{chatroomId}");

// Send to topic instead of individual tokens
await _firebaseNotificationService.SendNotificationToTopicAsync(
    $"chatroom_{chatroomId}",
    title,
    body,
    data
);
```

### 2. Notification Analytics
Track notification delivery và engagement:

```csharp
public class NotificationLog
{
    public int Id { get; set; }
    public string DeviceToken { get; set; }
    public string Title { get; set; }
    public string Body { get; set; }
    public DateTime SentAt { get; set; }
    public bool IsDelivered { get; set; }
    public bool IsClicked { get; set; }
}
```

### 3. Conditional Notifications
Chỉ gửi notification khi user thật sự offline:

```csharp
public async Task SendMessageAsync(int senderId, int chatroomId, string content)
{
    // ... lưu message ...
    
    // Chỉ gửi FCM cho users offline
    var onlineUsers = _connectionTrackingService.GetUsersInChatroom(chatroomId.ToString());
    var participants = await GetParticipantsWithTokensAsync(chatroomId);
    
    foreach (var participant in participants)
    {
        // Chỉ gửi FCM nếu user không online
        if (!onlineUsers.Contains(participant.UserId.ToString()))
        {
            await _firebaseNotificationService.SendNotificationToDeviceAsync(/* ... */);
        }
    }
}
```

### 4. Rich Notifications
Gửi notifications với images, actions:

```csharp
var notification = new
{
    title = "New Message",
    body = "You have a new message",
    image = "https://example.com/avatar.jpg",
    click_action = "OPEN_CHATROOM",
    icon = "chat_icon",
    color = "#0084FF"
};
```

## Performance Tips

1. **Batch FCM requests** khi có nhiều recipients
2. **Cache device tokens** để tránh query DB mỗi lần
3. **Remove invalid tokens** khi FCM trả về lỗi
4. **Use topics** cho group notifications lớn
5. **Implement retry logic** cho failed notifications

## Security Considerations

1. **Validate device tokens** trước khi gửi
2. **Rate limiting** để tránh spam notifications
3. **User preferences** cho notification settings
4. **Content filtering** để tránh sensitive data trong notifications

Happy coding với Firebase notifications! 🔔🚀
