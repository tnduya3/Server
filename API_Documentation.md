# API Documentation - Chat Server Application

## Overview
Đây là tài liệu API cho ứng dụng Chat Server được xây dựng với .NET Core và SignalR. Server hỗ trợ authentication qua Firebase, quản lý tin nhắn realtime, quản lý bạn bè và chatroom.

**Base URL**: `https://your-server-domain.com/api`

---

## 1. Authentication API (`/api/auth`)

### 1.1 Đăng nhập
- **URL**: `POST /api/auth/login`
- **Mục đích**: Xác thực người dùng với email và password
- **Headers**:
  ```
  Content-Type: application/json
  ```
- **Body**:
  ```json
  {
    "email": "user@example.com",
    "password": "password123"
  }
  ```
- **Response**:
  ```json
  {
    "success": true,
    "message": "Login successful",
    "accessToken": "firebase_jwt_token",
    "refreshToken": "firebase_refresh_token",
    "userId": "123",
    "email": "user@example.com",
    "expiresAt": "2025-06-30T10:00:00Z",
    "user": {
      "userId": "123",
      "email": "user@example.com",
      "displayName": "User Name",
      "emailVerified": true,
      "createdAt": "2025-01-01T00:00:00Z",
      "lastSignInAt": "2025-06-30T09:00:00Z"
    }
  }
  ```

### 1.2 Đăng ký
- **URL**: `POST /api/auth/signup`
- **Mục đích**: Tạo tài khoản mới cho người dùng
- **Headers**:
  ```
  Content-Type: application/json
  ```
- **Body**:
  ```json
  {
    "email": "newuser@example.com",
    "password": "password123",
    "confirmPassword": "password123",
    "displayName": "New User"
  }
  ```
- **Response**: Tương tự login response

### 1.3 Quên mật khẩu
- **URL**: `POST /api/auth/forgot-password`
- **Mục đích**: Gửi email reset password
- **Headers**:
  ```
  Content-Type: application/json
  ```
- **Body**:
  ```json
  {
    "email": "user@example.com"
  }
  ```
- **Response**:
  ```json
  {
    "success": true,
    "message": "Password reset email sent successfully"
  }
  ```

### 1.4 Refresh Token
- **URL**: `POST /api/auth/refresh-token`
- **Mục đích**: Làm mới access token
- **Headers**:
  ```
  Content-Type: application/json
  ```
- **Body**:
  ```json
  {
    "refreshToken": "firebase_refresh_token"
  }
  ```

### 1.5 Verify Token
- **URL**: `POST /api/auth/verify-token`
- **Mục đích**: Xác minh tính hợp lệ của ID token
- **Headers**:
  ```
  Content-Type: application/json
  ```
- **Body**:
  ```json
  {
    "idToken": "firebase_id_token"
  }
  ```

### 1.6 Đăng xuất
- **URL**: `POST /api/auth/logout`
- **Mục đích**: Đăng xuất người dùng (chủ yếu để logging)
- **Headers**:
  ```
  Content-Type: application/json
  UserId: {userId}
  ```
- **Body**: Không cần

---

## 2. Users API (`/api/users`)

### 2.1 Lấy thông tin người dùng theo ID
- **URL**: `GET /api/users/{id}`
- **Mục đích**: Lấy thông tin chi tiết của một người dùng
- **Headers**: Không cần đặc biệt
- **Response**:
  ```json
  {
    "userId": 123,
    "userName": "user123",
    "email": "user@example.com",
    "isOnline": true,
    "createdAt": "2025-01-01T00:00:00Z",
    "deviceToken": "fcm_device_token"
  }
  ```

### 2.2 Lấy danh sách tất cả người dùng
- **URL**: `GET /api/users`
- **Mục đích**: Lấy danh sách tất cả người dùng trong hệ thống
- **Headers**: Không cần đặc biệt
- **Response**: Array of user objects

### 2.3 Cập nhật thông tin người dùng
- **URL**: `PUT /api/users/{id}`
- **Mục đích**: Cập nhật thông tin người dùng
- **Headers**:
  ```
  Content-Type: application/json
  ```
- **Body**:
  ```json
  {
    "id": 123,
    "username": "newusername"
  }
  ```

### 2.4 Cập nhật Device Token
- **URL**: `PUT /api/users/{id}/token`
- **Mục đích**: Cập nhật Firebase device token cho push notification
- **Headers**:
  ```
  Content-Type: application/json
  ```
- **Body**: 
  ```json
  "fcm_device_token_string"
  ```

### 2.5 Xóa người dùng
- **URL**: `DELETE /api/users/{id}`
- **Mục đích**: Xóa tài khoản người dùng
- **Headers**: Không cần đặc biệt

---

## 3. Messages API (`/api/messages`)

### 3.1 Gửi tin nhắn
- **URL**: `POST /api/messages`
- **Mục đích**: Gửi tin nhắn mới trong chatroom
- **Headers**:
  ```
  Content-Type: application/json
  ```
- **Body**:
  ```json
  {
    "senderId": 123,
    "chatroomId": 456,
    "message": "Hello everyone!"
  }
  ```
- **Response**:
  ```json
  {
    "messageId": 789,
    "senderId": 123,
    "senderUsername": "user123",
    "chatroomId": 456,
    "content": "Hello everyone!",
    "createdAt": "2025-06-30T09:00:00Z",
    "messageType": "text"
  }
  ```

### 3.2 Lấy tin nhắn theo ID
- **URL**: `GET /api/messages/{id}`
- **Mục đích**: Lấy thông tin chi tiết của một tin nhắn
- **Headers**: Không cần đặc biệt

### 3.3 Lấy tin nhắn trong chatroom
- **URL**: `GET /api/messages/chatrooms/{chatroomId}`
- **Mục đích**: Lấy danh sách tin nhắn trong một chatroom (có phân trang)
- **Headers**: Không cần đặc biệt
- **Query Parameters**:
  - `page`: Số trang (mặc định: 1)
  - `pageSize`: Số tin nhắn mỗi trang (mặc định: 20)
- **Response**: Array of message objects

### 3.4 Chỉnh sửa tin nhắn
- **URL**: `PUT /api/messages/{id}`
- **Mục đích**: Chỉnh sửa nội dung tin nhắn đã gửi
- **Headers**:
  ```
  Content-Type: application/json
  ```
- **Body**:
  ```json
  {
    "id": 789,
    "message": "Updated message content"
  }
  ```

### 3.5 Xóa tin nhắn
- **URL**: `DELETE /api/messages/{id}`
- **Mục đích**: Xóa tin nhắn đã gửi
- **Headers**: Không cần đặc biệt

### 3.6 Test Firebase Notification
- **URL**: `POST /api/messages/test-firebase`
- **Mục đích**: Test gửi push notification qua Firebase
- **Headers**:
  ```
  Content-Type: application/json
  ```
- **Body**:
  ```json
  {
    "deviceToken": "fcm_device_token",
    "title": "Test Title",
    "body": "Test notification body"
  }
  ```

### 3.7 Broadcast Notification
- **URL**: `POST /api/messages/chatrooms/{chatroomId}/broadcast-notification`
- **Mục đích**: Gửi thông báo cho tất cả thành viên trong chatroom
- **Headers**:
  ```
  Content-Type: application/json
  ```
- **Body**:
  ```json
  {
    "title": "Important Announcement",
    "body": "This is a broadcast message for all members"
  }
  ```

---

## 4. Friends API (`/api/friends`)

### 4.1 Gửi lời mời kết bạn
- **URL**: `POST /api/friends/send-request`
- **Mục đích**: Gửi lời mời kết bạn đến người dùng khác
- **Headers**:
  ```
  Content-Type: application/json
  ```
- **Body**:
  ```json
  {
    "userId": 123,
    "friendId": 456
  }
  ```

### 4.2 Chấp nhận lời mời kết bạn
- **URL**: `POST /api/friends/accept-request`
- **Mục đích**: Chấp nhận lời mời kết bạn
- **Headers**:
  ```
  Content-Type: application/json
  ```
- **Body**:
  ```json
  {
    "userId": 123,
    "friendId": 456
  }
  ```

### 4.3 Từ chối lời mời kết bạn
- **URL**: `POST /api/friends/reject-request`
- **Mục đích**: Từ chối lời mời kết bạn
- **Headers**:
  ```
  Content-Type: application/json
  ```
- **Body**:
  ```json
  {
    "userId": 123,
    "friendId": 456
  }
  ```

### 4.4 Chặn người dùng
- **URL**: `POST /api/friends/block-user`
- **Mục đích**: Chặn một người dùng
- **Headers**:
  ```
  Content-Type: application/json
  ```
- **Body**:
  ```json
  {
    "userId": 123,
    "friendId": 456
  }
  ```

### 4.5 Hủy kết bạn
- **URL**: `POST /api/friends/unfriend`
- **Mục đích**: Hủy mối quan hệ bạn bè
- **Headers**:
  ```
  Content-Type: application/json
  ```
- **Body**:
  ```json
  {
    "userId": 123,
    "friendId": 456
  }
  ```

### 4.6 Lấy danh sách bạn bè
- **URL**: `GET /api/friends/{userId}/friends`
- **Mục đích**: Lấy danh sách bạn bè của người dùng
- **Headers**: Không cần đặc biệt

### 4.7 Lấy lời mời kết bạn đang chờ
- **URL**: `GET /api/friends/{userId}/pending-requests`
- **Mục đích**: Lấy danh sách lời mời kết bạn đang chờ xử lý
- **Headers**: Không cần đặc biệt

### 4.8 Lấy lời mời đã gửi
- **URL**: `GET /api/friends/{userId}/sent-requests`
- **Mục đích**: Lấy danh sách lời mời kết bạn đã gửi
- **Headers**: Không cần đặc biệt

### 4.9 Kiểm tra trạng thái bạn bè
- **URL**: `GET /api/friends/{userId}/status/{friendId}`
- **Mục đích**: Kiểm tra trạng thái mối quan hệ giữa hai người dùng
- **Headers**: Không cần đặc biệt

### 4.10 Kiểm tra có phải bạn bè không
- **URL**: `GET /api/friends/{userId}/are-friends/{friendId}`
- **Mục đích**: Kiểm tra xem hai người có phải bạn bè không
- **Headers**: Không cần đặc biệt
- **Response**:
  ```json
  {
    "areFriends": true
  }
  ```

### 4.11 Bắt đầu chat với bạn bè
- **URL**: `POST /api/friends/{userId}/start-chat/{friendId}`
- **Mục đích**: Tạo hoặc lấy chatroom trực tiếp với bạn bè
- **Headers**:
  ```
  Content-Type: application/json
  ```
- **Body** (tùy chọn):
  ```json
  {
    "initialMessage": "Hello friend!"
  }
  ```

### 4.12 Lấy danh sách chat trực tiếp
- **URL**: `GET /api/friends/{userId}/direct-chats`
- **Mục đích**: Lấy danh sách tất cả chat trực tiếp của người dùng
- **Headers**: Không cần đặc biệt

### 4.13 Kiểm tra chat trực tiếp tồn tại
- **URL**: `GET /api/friends/{userId}/check-direct-chat/{friendId}`
- **Mục đích**: Kiểm tra xem đã có chat trực tiếp với bạn bè chưa
- **Headers**: Không cần đặc biệt

---

## 5. Device Token API (`/api/devicetoken`)

### 5.1 Đăng ký Device Token
- **URL**: `POST /api/devicetoken/register`
- **Mục đích**: Đăng ký device token cho push notification
- **Headers**:
  ```
  Content-Type: application/json
  ```
- **Body**:
  ```json
  {
    "userId": 123,
    "token": "fcm_device_token_string"
  }
  ```

---

## 6. Chatrooms API (`/api/chatrooms`)

### 6.1 Lấy danh sách tất cả chatroom
- **URL**: `GET /api/chatrooms`
- **Mục đích**: Lấy danh sách tất cả chatroom trong hệ thống
- **Headers**: Không cần đặc biệt

### 6.2 Lấy thông tin chatroom theo ID
- **URL**: `GET /api/chatrooms/{id}`
- **Mục đích**: Lấy thông tin chi tiết của một chatroom
- **Headers**: Không cần đặc biệt

### 6.3 Tạo chatroom mới
- **URL**: `POST /api/chatrooms`
- **Mục đích**: Tạo chatroom mới
- **Headers**:
  ```
  Content-Type: application/json
  ```
- **Body**:
  ```json
  {
    "name": "My Chat Room",
    "createdBy": 123,
    "isGroup": true,
    "description": "This is a group chat"
  }
  ```

### 6.4 Cập nhật chatroom
- **URL**: `PUT /api/chatrooms/{id}`
- **Mục đích**: Cập nhật thông tin chatroom
- **Headers**:
  ```
  Content-Type: application/json
  ```
- **Body**:
  ```json
  {
    "id": 456,
    "name": "Updated Room Name",
    "description": "Updated description",
    "updatedBy": 123
  }
  ```

### 6.5 Xóa chatroom (soft delete)
- **URL**: `DELETE /api/chatrooms/{id}?deletedBy={userId}`
- **Mục đích**: Xóa chatroom (đánh dấu là đã xóa)
- **Headers**: Không cần đặc biệt
- **Query Parameters**: `deletedBy` - ID của người xóa

### 6.6 Xóa chatroom vĩnh viễn
- **URL**: `DELETE /api/chatrooms/{id}/hard?deletedBy={userId}`
- **Mục đích**: Xóa chatroom vĩnh viễn khỏi database
- **Headers**: Không cần đặc biệt

### 6.7 Lấy chatroom của người dùng
- **URL**: `GET /api/chatrooms/user/{userId}`
- **Mục đích**: Lấy danh sách chatroom mà người dùng tham gia
- **Headers**: Không cần đặc biệt

### 6.8 Tìm kiếm chatroom
- **URL**: `GET /api/chatrooms/search?searchTerm={term}&userId={userId}`
- **Mục đích**: Tìm kiếm chatroom theo tên
- **Headers**: Không cần đặc biệt
- **Query Parameters**:
  - `searchTerm`: Từ khóa tìm kiếm
  - `userId`: ID người dùng thực hiện tìm kiếm

### 6.9 Lấy chatroom đã lưu trữ
- **URL**: `GET /api/chatrooms/archived/{userId}`
- **Mục đích**: Lấy danh sách chatroom đã được lưu trữ
- **Headers**: Không cần đặc biệt

### 6.10 Thêm người dùng vào chatroom
- **URL**: `POST /api/chatrooms/{chatroomId}/users/{userId}`
- **Mục đích**: Thêm một người dùng vào chatroom
- **Headers**:
  ```
  Content-Type: application/json
  ```
- **Body** (tùy chọn):
  ```json
  {
    "role": "member",
    "addedBy": 123
  }
  ```

### 6.11 Thêm nhiều người dùng vào chatroom
- **URL**: `POST /api/chatrooms/{chatroomId}/users/bulk`
- **Mục đích**: Thêm nhiều người dùng cùng lúc vào chatroom
- **Headers**:
  ```
  Content-Type: application/json
  ```
- **Body**:
  ```json
  {
    "userIds": [123, 456, 789],
    "role": "member",
    "addedBy": 100
  }
  ```

### 6.12 Xóa người dùng khỏi chatroom
- **URL**: `DELETE /api/chatrooms/{chatroomId}/users/{userId}?removedBy={removerId}`
- **Mục đích**: Xóa người dùng khỏi chatroom
- **Headers**: Không cần đặc biệt

### 6.13 Cập nhật vai trò người dùng
- **URL**: `PUT /api/chatrooms/{chatroomId}/users/{userId}/role`
- **Mục đích**: Cập nhật vai trò của người dùng trong chatroom
- **Headers**:
  ```
  Content-Type: application/json
  ```
- **Body**:
  ```json
  {
    "role": "admin",
    "updatedBy": 123
  }
  ```

### 6.14 Lấy danh sách thành viên
- **URL**: `GET /api/chatrooms/{chatroomId}/users`
- **Mục đích**: Lấy danh sách thành viên trong chatroom
- **Headers**: Không cần đặc biệt

### 6.15 Lấy thành viên với vai trò
- **URL**: `GET /api/chatrooms/{chatroomId}/participants`
- **Mục đích**: Lấy danh sách thành viên kèm vai trò trong chatroom
- **Headers**: Không cần đặc biệt

### 6.16 Lấy thành viên đang hoạt động
- **URL**: `GET /api/chatrooms/{chatroomId}/participants/active`
- **Mục đích**: Lấy danh sách thành viên đang online
- **Headers**: Không cần đặc biệt

### 6.17 Đếm số thành viên
- **URL**: `GET /api/chatrooms/{chatroomId}/participants/count`
- **Mục đích**: Lấy số lượng thành viên trong chatroom
- **Headers**: Không cần đặc biệt

### 6.18 Lấy vai trò người dùng
- **URL**: `GET /api/chatrooms/{chatroomId}/users/{userId}/role`
- **Mục đích**: Lấy vai trò của người dùng trong chatroom
- **Headers**: Không cần đặc biệt

### 6.19 Kiểm tra quyền truy cập
- **URL**: `GET /api/chatrooms/{chatroomId}/users/{userId}/access`
- **Mục đích**: Kiểm tra người dùng có quyền truy cập chatroom không
- **Headers**: Không cần đặc biệt

### 6.20 Lưu trữ chatroom
- **URL**: `POST /api/chatrooms/{chatroomId}/archive`
- **Mục đích**: Lưu trữ chatroom (ẩn khỏi danh sách chính)
- **Headers**:
  ```
  Content-Type: application/json
  ```
- **Body**:
  ```json
  {
    "archivedBy": 123
  }
  ```

### 6.21 Bỏ lưu trữ chatroom
- **URL**: `POST /api/chatrooms/{chatroomId}/unarchive`
- **Mục đích**: Bỏ lưu trữ chatroom
- **Headers**:
  ```
  Content-Type: application/json
  ```
- **Body**:
  ```json
  {
    "archivedBy": 123
  }
  ```

### 6.22 Lấy tin nhắn trong chatroom
- **URL**: `GET /api/chatrooms/{chatroomId}/messages?page={page}&pageSize={pageSize}`
- **Mục đích**: Lấy danh sách tin nhắn trong chatroom (có phân trang)
- **Headers**: Không cần đặc biệt
- **Query Parameters**:
  - `page`: Số trang (mặc định: 1)
  - `pageSize`: Số tin nhắn mỗi trang (mặc định: 20)

### 6.23 Thông báo người dùng online
- **URL**: `POST /api/chatrooms/{chatroomId}/online`
- **Mục đích**: Thông báo người dùng đang online trong chatroom
- **Headers**:
  ```
  Content-Type: application/json
  ```
- **Body**:
  ```json
  {
    "userId": 123
  }
  ```

### 6.24 Broadcast thông báo
- **URL**: `POST /api/chatrooms/{chatroomId}/broadcast`
- **Mục đích**: Gửi thông báo chung cho tất cả thành viên
- **Headers**:
  ```
  Content-Type: application/json
  ```
- **Body**:
  ```json
  {
    "type": "announcement",
    "message": "Important announcement",
    "data": { "priority": "high" }
  }
  ```

### 6.25 Lấy thống kê chatroom
- **URL**: `GET /api/chatrooms/{chatroomId}/stats`
- **Mục đích**: Lấy thống kê chi tiết của chatroom
- **Headers**: Không cần đặc biệt

### 6.26 Lấy hoạt động cuối cùng
- **URL**: `GET /api/chatrooms/{chatroomId}/activity`
- **Mục đích**: Lấy thời gian hoạt động cuối cùng của chatroom
- **Headers**: Không cần đặc biệt

### 6.27 Đếm số tin nhắn
- **URL**: `GET /api/chatrooms/{chatroomId}/messages/count`
- **Mục đích**: Lấy tổng số tin nhắn trong chatroom
- **Headers**: Không cần đặc biệt

---

## SignalR Hub Events

Server cũng hỗ trợ realtime communication qua SignalR Hub tại endpoint `/chathub`.

### Events từ Client:
- `JoinGroup`: Tham gia vào group chatroom
- `LeaveGroup`: Rời khỏi group chatroom
- `SendMessage`: Gửi tin nhắn realtime
- `TypingStart`: Bắt đầu typing
- `TypingStop`: Dừng typing
- `UserOnline`: Thông báo user online
- `UserOffline`: Thông báo user offline

### Events từ Server:
- `ReceiveMessage`: Nhận tin nhắn mới
- `MessageEdited`: Tin nhắn được chỉnh sửa
- `MessageDeleted`: Tin nhắn được xóa
- `UserOnline`: Người dùng online
- `UserOffline`: Người dùng offline
- `UserTyping`: Người dùng đang typing
- `UserStoppedTyping`: Người dùng dừng typing
- `BroadcastNotification`: Thông báo broadcast
- `ChatroomNotification`: Thông báo trong chatroom

---

## Error Codes

### HTTP Status Codes:
- `200 OK`: Thành công
- `201 Created`: Tạo mới thành công
- `204 No Content`: Thành công nhưng không có dữ liệu trả về
- `400 Bad Request`: Dữ liệu đầu vào không hợp lệ
- `401 Unauthorized`: Không có quyền truy cập
- `403 Forbidden`: Bị cấm truy cập
- `404 Not Found`: Không tìm thấy tài nguyên
- `409 Conflict`: Xung đột dữ liệu
- `500 Internal Server Error`: Lỗi server

### Typical Error Response:
```json
{
  "success": false,
  "message": "Error description",
  "details": "Additional error details if available"
}
```

---

## Authentication & Authorization

Hầu hết các API endpoint yêu cầu authentication thông qua Firebase JWT token. Token cần được include trong header:

```
Authorization: Bearer {firebase_jwt_token}
```

Một số endpoint cũng có thể yêu cầu thêm `UserId` trong header để xác định người dùng hiện tại.

---

## Rate Limiting

Server có thể áp dụng rate limiting để tránh spam. Thông tin chi tiết về giới hạn sẽ được trả về trong response headers khi áp dụng.

---

## WebSocket Connection

Để kết nối SignalR Hub:
```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chathub")
    .build();
```

Đảm bảo authenticate trước khi join các group chatroom để nhận tin nhắn realtime.
