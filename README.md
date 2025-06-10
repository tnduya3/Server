# 🚀 Hướng dẫn sử dụng Real-time Chat với SignalR

## Tổng quan
Hệ thống chat real-time này sử dụng ASP.NET Core SignalR để cung cấp khả năng nhắn tin tức thời, bao gồm:

- ✅ Gửi/nhận tin nhắn real-time
- ✅ Typing indicators (hiển thị khi ai đó đang gõ)
- ✅ Quản lý phòng chat (join/leave)
- ✅ Tracking users online/offline
- ✅ Thông báo khi user tham gia/rời phòng
- ✅ Message read receipts
- ✅ Edit/Delete messages với real-time sync
- ✅ Error handling và auto-reconnect

## Cách chạy server

### 1. Cài đặt dependencies
```bash
cd "Server(1)"
```

### 2. Chạy server
```bash
start-server.bat
```

Server sẽ chạy trên:
- HTTP: `http://localhost:5237`
- HTTPS: `https://localhost:7092`

### 3. Truy cập demo client
Mở trình duyệt và truy cập: `https://localhost:7092/chat-client.html`

## API Endpoints

### SignalR Hub: `/chathub`

#### Các phương thức client có thể gọi:
- `RegisterUser(userId)` - Đăng ký user với hub
- `JoinChatroom(chatroomId, userId)` - Tham gia phòng chat
- `LeaveChatroom(chatroomId, userId)` - Rời phòng chat
- `SendMessage(senderId, chatroomId, message)` - Gửi tin nhắn
- `SendTyping(senderId, chatroomId, senderName)` - Báo đang gõ
- `StopTyping(senderId, chatroomId)` - Ngừng báo gõ
- `MarkMessageAsRead(messageId, userId, chatroomId)` - Đánh dấu đã đọc
- `Ping()` - Kiểm tra kết nối

#### Các events client sẽ nhận:
- `Connected` - Xác nhận kết nối thành công
- `ReceiveMessage` - Nhận tin nhắn mới
- `JoinConfirmation` - Xác nhận tham gia phòng
- `UserJoinedChatroom` - User khác tham gia
- `UserLeftChatroom` - User khác rời đi
- `UserTyping` - Ai đó đang gõ
- `UserStoppedTyping` - Ngừng gõ
- `MessageRead` - Tin nhắn đã được đọc
- `MessageEdited` - Tin nhắn đã được sửa
- `MessageDeleted` - Tin nhắn đã được xóa
- `UserOnline/UserOffline` - Trạng thái online
- `ReceiveError` - Lỗi
- `Pong` - Response của ping

### REST API Endpoints

#### Messages
- `POST /api/messages` - Gửi tin nhắn (cũng gửi qua SignalR)
- `GET /api/messages/{id}` - Lấy tin nhắn theo ID
- `GET /api/messages/chatrooms/{chatroomId}` - Lấy tin nhắn trong phòng
- `PUT /api/messages/{id}` - Sửa tin nhắn (sync qua SignalR)
- `DELETE /api/messages/{id}` - Xóa tin nhắn (sync qua SignalR)

#### Chatrooms
- `GET /api/chatrooms` - Lấy danh sách phòng chat
- `POST /api/chatrooms` - Tạo phòng chat mới
- `GET /api/chatrooms/{id}` - Lấy thông tin phòng chat
- `GET /api/chatrooms/{id}/users` - Lấy danh sách users trong phòng
- `POST /api/chatrooms/{chatroomId}/online` - Thông báo user online
- `POST /api/chatrooms/{chatroomId}/broadcast` - Broadcast thông báo
- `GET /api/chatrooms/{chatroomId}/stats` - Thống kê phòng chat

## Cách sử dụng trong code

### JavaScript Client (Web)
```javascript
// Sử dụng class ChatClient đã tạo
const chatClient = new ChatClient('https://localhost:7092/chathub');

// Thiết lập event handlers
chatClient.onMessageReceived = (messageData) => {
    console.log('New message:', messageData);
    // Cập nhật UI
};

chatClient.onUserTyping = (data) => {
    console.log(`${data.SenderName} đang gõ...`);
    // Hiển thị typing indicator
};

// Kết nối và tham gia phòng
await chatClient.connect(1, 'User1');
await chatClient.joinChatroom(1);

// Gửi tin nhắn
await chatClient.sendMessage('Hello everyone!');
```

### C# Client (WPF/MAUI/Console)
```csharp
using Microsoft.AspNetCore.SignalR.Client;

var connection = new HubConnectionBuilder()
    .WithUrl("https://localhost:7092/chathub")
    .Build();

// Lắng nghe tin nhắn
connection.On<object>("ReceiveMessage", (messageData) =>
{
    Console.WriteLine($"Message received: {messageData}");
});

// Kết nối
await connection.StartAsync();

// Đăng ký user
await connection.InvokeAsync("RegisterUser", "1");

// Tham gia phòng
await connection.InvokeAsync("JoinChatroom", "1", "1");

// Gửi tin nhắn
await connection.InvokeAsync("SendMessage", 1, 1, "Hello from C#!");
```

## Test scenarios

### Test 1: Multiple users chat
1. Mở 2 tab trình duyệt với `chat-client.html`
2. Tab 1: User ID = 1, Username = "Alice"
3. Tab 2: User ID = 2, Username = "Bob"
4. Cả 2 tham gia chatroom ID = 1
5. Gửi tin nhắn qua lại và quan sát real-time sync

### Test 2: Typing indicators
1. User 1 bắt đầu gõ tin nhắn
2. User 2 sẽ thấy "Alice đang gõ..."
3. User 1 ngừng gõ hoặc gửi tin nhắn
4. Typing indicator biến mất

### Test 3: Join/Leave notifications
1. User 1 tham gia phòng
2. User 2 sẽ thấy thông báo "Alice đã tham gia phòng chat"
3. User 1 rời phòng
4. User 2 sẽ thấy thông báo "Alice đã rời phòng chat"

### Test 4: API + SignalR integration
1. Sử dụng Postman gửi POST request đến `/api/messages`
2. Users trong chatroom sẽ nhận tin nhắn real-time qua SignalR

## Troubleshooting

### Lỗi kết nối CORS
Đảm bảo origin của client được thêm vào CORS policy trong `Program.cs`

### Lỗi SSL certificate
Chạy lệnh để trust dev certificate:
```bash
dotnet dev-certs https --trust
```

### Database connection issues
Kiểm tra connection string trong `appsettings.json`

### SignalR connection fails
1. Kiểm tra server có chạy không
2. Kiểm tra URL hub (`/chathub`)
3. Kiểm tra browser console để xem lỗi chi tiết

## Monitoring & Debugging

### Logs
Server sẽ in ra console:
- Khi client connect/disconnect
- Khi có lỗi xảy ra
- Khi user join/leave chatroom

### Browser DevTools
Mở F12 → Console để xem:
- SignalR connection status
- Messages được gửi/nhận
- Errors nếu có

## Mở rộng thêm

### 1. Thêm authentication
```csharp
// Trong ChatHub
[Authorize]
public class ChatHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.Identity?.Name;
        // ...
    }
}
```

### 2. Thêm file/image sharing
```csharp
public async Task SendFile(int senderId, int chatroomId, string fileName, string fileData)
{
    // Xử lý file upload
    // Gửi qua SignalR với messageType = "file"
}
```

### 3. Thêm message reactions
```csharp
public async Task AddReaction(int messageId, int userId, string emoji)
{
    // Lưu reaction vào DB
    // Broadcast reaction update
}
```

### 4. Group video/voice call
Tích hợp với WebRTC để thêm tính năng gọi điện/video call nhóm.

## Performance Optimization

### Scaling
- Sử dụng Redis backplane cho multiple server instances
- Implement message pagination
- Cache frequently accessed data

### Database
- Index các trường thường query (ChatRoomId, SenderId, CreatedAt)
- Partition messages table theo thời gian
- Archive old messages

Chúc bạn code vui vẻ! 🎉
