# API Documentation - Chat Server Application

## Overview
Đây là tài liệu API cho ứng dụng Chat Server được xây dựng với .NET Core và SignalR. Server hỗ trợ authentication qua Firebase, quản lý tin nhắn realtime, quản lý bạn bè và chatroom.

**Base URL**: `https://your-server-domain.com/api`

---

## 1. Authentication API (`/api/auth`)

### 1.1 Đăng nhập

| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| **Phương thức** | POST | Xác thực người dùng với email và password |
| **URL** | string | https://domain/api/auth/login |

**Headers yêu cầu:**
| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| Content-Type | string | application/json |

**Body yêu cầu:**
| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| email | string | Email người dùng |
| password | string | Mật khẩu người dùng |

**Dữ liệu trả về:**
| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| success | boolean | Trạng thái thành công |
| message | string | Thông báo kết quả |
| accessToken | string | JWT token để xác thực |
| refreshToken | string | Token để làm mới access token |
| userId | string | ID người dùng |
| email | string | Email người dùng |
| expiresAt | datetime | Thời gian hết hạn token |
| user | object | Thông tin chi tiết người dùng |

### 1.2 Đăng ký

| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| **Phương thức** | POST | Tạo tài khoản mới cho người dùng |
| **URL** | string | https://domain/api/auth/signup |

**Headers yêu cầu:**
| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| Content-Type | string | application/json |

**Body yêu cầu:**
| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| email | string | Email người dùng |
| password | string | Mật khẩu người dùng |
| confirmPassword | string | Xác nhận mật khẩu |
| displayName | string | Tên hiển thị (tùy chọn) |

**Dữ liệu trả về:** Tương tự như API đăng nhập

### 1.3 Quên mật khẩu

| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| **Phương thức** | POST | Gửi email reset password |
| **URL** | string | https://domain/api/auth/forgot-password |

**Headers yêu cầu:**
| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| Content-Type | string | application/json |

**Body yêu cầu:**
| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| email | string | Email người dùng |

**Dữ liệu trả về:**
| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| success | boolean | Trạng thái thành công |
| message | string | Thông báo kết quả |

### 1.4 Refresh Token

| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| **Phương thức** | POST | Làm mới access token |
| **URL** | string | https://domain/api/auth/refresh-token |

**Headers yêu cầu:**
| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| Content-Type | string | application/json |

**Body yêu cầu:**
| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| refreshToken | string | Token để làm mới |

### 1.5 Verify Token

| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| **Phương thức** | POST | Xác minh tính hợp lệ của ID token |
| **URL** | string | https://domain/api/auth/verify-token |

**Headers yêu cầu:**
| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| Content-Type | string | application/json |

**Body yêu cầu:**
| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| idToken | string | Firebase ID token |

### 1.6 Đăng xuất

| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| **Phương thức** | POST | Đăng xuất người dùng (chủ yếu để logging) |
| **URL** | string | https://domain/api/auth/logout |

**Headers yêu cầu:**
| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| Content-Type | string | application/json |
| UserId | string | ID người dùng |

**Body yêu cầu:** Không cần

---

## 2. Users API (`/api/users`)

### 2.1 Lấy thông tin người dùng theo ID

| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| **Phương thức** | GET | Lấy thông tin chi tiết của một người dùng |
| **URL** | string | https://domain/api/users/{id} |

**Dữ liệu trả về:**
| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| userId | int | ID người dùng |
| userName | string | Tên đăng nhập |
| email | string | Email người dùng |
| isOnline | boolean | Trạng thái online |
| createdAt | datetime | Thời gian tạo tài khoản |
| deviceToken | string | Token thiết bị cho push notification |

### 2.2 Lấy danh sách tất cả người dùng

| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| **Phương thức** | GET | Lấy danh sách tất cả người dùng trong hệ thống |
| **URL** | string | https://domain/api/users |

**Dữ liệu trả về:** Mảng các đối tượng người dùng

### 2.3 Cập nhật thông tin người dùng

| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| **Phương thức** | PUT | Cập nhật thông tin người dùng |
| **URL** | string | https://domain/api/users/{id} |

**Headers yêu cầu:**
| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| Content-Type | string | application/json |

**Body yêu cầu:**
| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| id | int | ID người dùng |
| username | string | Tên đăng nhập mới |

### 2.4 Cập nhật Device Token

| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| **Phương thức** | PUT | Cập nhật Firebase device token cho push notification |
| **URL** | string | https://domain/api/users/{id}/token |

**Headers yêu cầu:**
| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| Content-Type | string | application/json |

**Body yêu cầu:**
| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| deviceToken | string | Firebase device token |

### 2.5 Xóa người dùng

| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| **Phương thức** | DELETE | Xóa tài khoản người dùng |
| **URL** | string | https://domain/api/users/{id} |

---

## 3. Messages API (`/api/messages`)

### 3.1 Gửi tin nhắn

| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| **Phương thức** | POST | Gửi tin nhắn mới trong chatroom |
| **URL** | string | https://domain/api/messages |

**Headers yêu cầu:**
| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| Content-Type | string | application/json |

**Body yêu cầu:**
| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| senderId | int | ID người gửi |
| chatroomId | int | ID chatroom |
| message | string | Nội dung tin nhắn |

**Dữ liệu trả về:**
| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| messageId | int | ID tin nhắn |
| senderId | int | ID người gửi |
| senderUsername | string | Tên người gửi |
| chatroomId | int | ID chatroom |
| content | string | Nội dung tin nhắn |
| createdAt | datetime | Thời gian tạo |
| messageType | string | Loại tin nhắn (text) |

### 3.2 Lấy tin nhắn theo ID

| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| **Phương thức** | GET | Lấy thông tin chi tiết của một tin nhắn |
| **URL** | string | https://domain/api/messages/{id} |

### 3.3 Lấy tin nhắn trong chatroom

| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| **Phương thức** | GET | Lấy danh sách tin nhắn trong một chatroom (có phân trang) |
| **URL** | string | https://domain/api/messages/chatrooms/{chatroomId} |

**Query Parameters:**
| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| page | int | Số trang (mặc định: 1) |
| pageSize | int | Số tin nhắn mỗi trang (mặc định: 20) |

**Dữ liệu trả về:** Mảng các đối tượng tin nhắn

### 3.4 Chỉnh sửa tin nhắn

| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| **Phương thức** | PUT | Chỉnh sửa nội dung tin nhắn đã gửi |
| **URL** | string | https://domain/api/messages/{id} |

**Headers yêu cầu:**
| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| Content-Type | string | application/json |

**Body yêu cầu:**
| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| id | int | ID tin nhắn |
| message | string | Nội dung tin nhắn mới |

### 3.5 Xóa tin nhắn

| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| **Phương thức** | DELETE | Xóa tin nhắn đã gửi |
| **URL** | string | https://domain/api/messages/{id} |

### 3.6 Test Firebase Notification

| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| **Phương thức** | POST | Test gửi push notification qua Firebase |
| **URL** | string | https://domain/api/messages/test-firebase |

**Headers yêu cầu:**
| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| Content-Type | string | application/json |

**Body yêu cầu:**
| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| deviceToken | string | Firebase device token |
| title | string | Tiêu đề thông báo |
| body | string | Nội dung thông báo |

### 3.7 Broadcast Notification

| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| **Phương thức** | POST | Gửi thông báo cho tất cả thành viên trong chatroom |
| **URL** | string | https://domain/api/messages/chatrooms/{chatroomId}/broadcast-notification |

**Headers yêu cầu:**
| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| Content-Type | string | application/json |

**Body yêu cầu:**
| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| title | string | Tiêu đề thông báo |
| body | string | Nội dung thông báo |

---

## 4. Friends API (`/api/friends`)

### 4.1 Gửi lời mời kết bạn

| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| **Phương thức** | POST | Gửi lời mời kết bạn đến người dùng khác |
| **URL** | string | https://domain/api/friends/send-request |

**Headers yêu cầu:**
| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| Content-Type | string | application/json |

**Body yêu cầu:**
| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| userId | int | ID người gửi lời mời |
| friendId | int | ID người nhận lời mời |

### 4.2 Chấp nhận lời mời kết bạn

| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| **Phương thức** | POST | Chấp nhận lời mời kết bạn |
| **URL** | string | https://domain/api/friends/accept-request |

**Headers yêu cầu:**
| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| Content-Type | string | application/json |

**Body yêu cầu:**
| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| userId | int | ID người chấp nhận |
| friendId | int | ID người gửi lời mời |

### 4.3 Từ chối lời mời kết bạn

| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| **Phương thức** | POST | Từ chối lời mời kết bạn |
| **URL** | string | https://domain/api/friends/reject-request |

### 4.4 Chặn người dùng

| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| **Phương thức** | POST | Chặn một người dùng |
| **URL** | string | https://domain/api/friends/block-user |

### 4.5 Hủy kết bạn

| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| **Phương thức** | POST | Hủy mối quan hệ bạn bè |
| **URL** | string | https://domain/api/friends/unfriend |

### 4.6 Lấy danh sách bạn bè

| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| **Phương thức** | GET | Lấy danh sách bạn bè của người dùng |
| **URL** | string | https://domain/api/friends/{userId}/friends |

### 4.7 Lấy lời mời kết bạn đang chờ

| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| **Phương thức** | GET | Lấy danh sách lời mời kết bạn đang chờ xử lý |
| **URL** | string | https://domain/api/friends/{userId}/pending-requests |

### 4.8 Lấy lời mời đã gửi

| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| **Phương thức** | GET | Lấy danh sách lời mời kết bạn đã gửi |
| **URL** | string | https://domain/api/friends/{userId}/sent-requests |

### 4.9 Kiểm tra trạng thái bạn bè

| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| **Phương thức** | GET | Kiểm tra trạng thái mối quan hệ giữa hai người dùng |
| **URL** | string | https://domain/api/friends/{userId}/status/{friendId} |

### 4.10 Kiểm tra có phải bạn bè không

| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| **Phương thức** | GET | Kiểm tra xem hai người có phải bạn bè không |
| **URL** | string | https://domain/api/friends/{userId}/are-friends/{friendId} |

**Dữ liệu trả về:**
| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| areFriends | boolean | Có phải bạn bè hay không |

### 4.11 Bắt đầu chat với bạn bè

| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| **Phương thức** | POST | Tạo hoặc lấy chatroom trực tiếp với bạn bè |
| **URL** | string | https://domain/api/friends/{userId}/start-chat/{friendId} |

**Headers yêu cầu:**
| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| Content-Type | string | application/json |

**Body yêu cầu (tùy chọn):**
| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| initialMessage | string | Tin nhắn đầu tiên |

### 4.12 Lấy danh sách chat trực tiếp

| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| **Phương thức** | GET | Lấy danh sách tất cả chat trực tiếp của người dùng |
| **URL** | string | https://domain/api/friends/{userId}/direct-chats |

### 4.13 Kiểm tra chat trực tiếp tồn tại

| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| **Phương thức** | GET | Kiểm tra xem đã có chat trực tiếp với bạn bè chưa |
| **URL** | string | https://domain/api/friends/{userId}/check-direct-chat/{friendId} |

---

## 5. Device Token API (`/api/devicetoken`)

### 5.1 Đăng ký Device Token

| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| **Phương thức** | POST | Đăng ký device token cho push notification |
| **URL** | string | https://domain/api/devicetoken/register |

**Headers yêu cầu:**
| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| Content-Type | string | application/json |

**Body yêu cầu:**
| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| userId | int | ID người dùng |
| token | string | Firebase device token |

---

## 6. Chatrooms API (`/api/chatrooms`)

### 6.1 Lấy danh sách tất cả chatroom

| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| **Phương thức** | GET | Lấy danh sách tất cả chatroom trong hệ thống |
| **URL** | string | https://domain/api/chatrooms |

### 6.2 Lấy thông tin chatroom theo ID

| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| **Phương thức** | GET | Lấy thông tin chi tiết của một chatroom |
| **URL** | string | https://domain/api/chatrooms/{id} |

### 6.3 Tạo chatroom mới

| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| **Phương thức** | POST | Tạo chatroom mới |
| **URL** | string | https://domain/api/chatrooms |

**Headers yêu cầu:**
| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| Content-Type | string | application/json |

**Body yêu cầu:**
| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| name | string | Tên chatroom |
| createdBy | int | ID người tạo |
| isGroup | boolean | Có phải group chat không |
| description | string | Mô tả chatroom |

### 6.4 Cập nhật chatroom

| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| **Phương thức** | PUT | Cập nhật thông tin chatroom |
| **URL** | string | https://domain/api/chatrooms/{id} |

**Headers yêu cầu:**
| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| Content-Type | string | application/json |

**Body yêu cầu:**
| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| id | int | ID chatroom |
| name | string | Tên chatroom mới |
| description | string | Mô tả mới |
| updatedBy | int | ID người cập nhật |

### 6.5 Xóa chatroom (soft delete)

| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| **Phương thức** | DELETE | Xóa chatroom (đánh dấu là đã xóa) |
| **URL** | string | https://domain/api/chatrooms/{id}?deletedBy={userId} |

**Query Parameters:**
| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| deletedBy | int | ID của người xóa |

### 6.6 Xóa chatroom vĩnh viễn

| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| **Phương thức** | DELETE | Xóa chatroom vĩnh viễn khỏi database |
| **URL** | string | https://domain/api/chatrooms/{id}/hard?deletedBy={userId} |

### 6.7 Lấy chatroom của người dùng

| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| **Phương thức** | GET | Lấy danh sách chatroom mà người dùng tham gia |
| **URL** | string | https://domain/api/chatrooms/user/{userId} |

### 6.8 Tìm kiếm chatroom

| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| **Phương thức** | GET | Tìm kiếm chatroom theo tên |
| **URL** | string | https://domain/api/chatrooms/search |

**Query Parameters:**
| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| searchTerm | string | Từ khóa tìm kiếm |
| userId | int | ID người dùng thực hiện tìm kiếm |

### 6.9 Lấy chatroom đã lưu trữ

| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| **Phương thức** | GET | Lấy danh sách chatroom đã được lưu trữ |
| **URL** | string | https://domain/api/chatrooms/archived/{userId} |

### 6.10 Thêm người dùng vào chatroom

| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| **Phương thức** | POST | Thêm một người dùng vào chatroom |
| **URL** | string | https://domain/api/chatrooms/{chatroomId}/users/{userId} |

**Headers yêu cầu:**
| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| Content-Type | string | application/json |

**Body yêu cầu (tùy chọn):**
| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| role | string | Vai trò trong chatroom |
| addedBy | int | ID người thêm |

### 6.11 Thêm nhiều người dùng vào chatroom

| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| **Phương thức** | POST | Thêm nhiều người dùng cùng lúc vào chatroom |
| **URL** | string | https://domain/api/chatrooms/{chatroomId}/users/bulk |

**Headers yêu cầu:**
| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| Content-Type | string | application/json |

**Body yêu cầu:**
| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| userIds | array[int] | Danh sách ID người dùng |
| role | string | Vai trò trong chatroom |
| addedBy | int | ID người thêm |

### 6.12 Xóa người dùng khỏi chatroom

| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| **Phương thức** | DELETE | Xóa người dùng khỏi chatroom |
| **URL** | string | https://domain/api/chatrooms/{chatroomId}/users/{userId} |

**Query Parameters:**
| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| removedBy | int | ID người thực hiện xóa |

### 6.13 Cập nhật vai trò người dùng

| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| **Phương thức** | PUT | Cập nhật vai trò của người dùng trong chatroom |
| **URL** | string | https://domain/api/chatrooms/{chatroomId}/users/{userId}/role |

**Headers yêu cầu:**
| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| Content-Type | string | application/json |

**Body yêu cầu:**
| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| role | string | Vai trò mới (admin, member, etc.) |
| updatedBy | int | ID người thực hiện cập nhật |

### 6.14 Lấy danh sách thành viên

| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| **Phương thức** | GET | Lấy danh sách thành viên trong chatroom |
| **URL** | string | https://domain/api/chatrooms/{chatroomId}/users |

### 6.15 Lấy thành viên với vai trò

| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| **Phương thức** | GET | Lấy danh sách thành viên kèm vai trò trong chatroom |
| **URL** | string | https://domain/api/chatrooms/{chatroomId}/participants |

### 6.16 Lấy tin nhắn trong chatroom

| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| **Phương thức** | GET | Lấy danh sách tin nhắn trong chatroom (có phân trang) |
| **URL** | string | https://domain/api/chatrooms/{chatroomId}/messages |

**Query Parameters:**
| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| page | int | Số trang (mặc định: 1) |
| pageSize | int | Số tin nhắn mỗi trang (mặc định: 20) |

### 6.17 Thống kê chatroom

| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| **Phương thức** | GET | Lấy thống kê chi tiết của chatroom |
| **URL** | string | https://domain/api/chatrooms/{chatroomId}/stats |

### 6.18 Lưu trữ chatroom

| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| **Phương thức** | POST | Lưu trữ chatroom (ẩn khỏi danh sách chính) |
| **URL** | string | https://domain/api/chatrooms/{chatroomId}/archive |

**Headers yêu cầu:**
| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| Content-Type | string | application/json |

**Body yêu cầu:**
| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| archivedBy | int | ID người thực hiện lưu trữ |

### 6.19 Broadcast thông báo

| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| **Phương thức** | POST | Gửi thông báo chung cho tất cả thành viên |
| **URL** | string | https://domain/api/chatrooms/{chatroomId}/broadcast |

**Headers yêu cầu:**
| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| Content-Type | string | application/json |

**Body yêu cầu:**
| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| type | string | Loại thông báo |
| message | string | Nội dung thông báo |
| data | object | Dữ liệu bổ sung (tùy chọn) |

---

## SignalR Hub Events

Server hỗ trợ realtime communication qua SignalR Hub tại endpoint `/chathub`.

### Events từ Client:
| Event | Ý nghĩa |
|-------|---------|
| JoinGroup | Tham gia vào group chatroom |
| LeaveGroup | Rời khỏi group chatroom |
| SendMessage | Gửi tin nhắn realtime |
| TypingStart | Bắt đầu typing |
| TypingStop | Dừng typing |
| UserOnline | Thông báo user online |
| UserOffline | Thông báo user offline |

### Events từ Server:
| Event | Ý nghĩa |
|-------|---------|
| ReceiveMessage | Nhận tin nhắn mới |
| MessageEdited | Tin nhắn được chỉnh sửa |
| MessageDeleted | Tin nhắn được xóa |
| UserOnline | Người dùng online |
| UserOffline | Người dùng offline |
| UserTyping | Người dùng đang typing |
| UserStoppedTyping | Người dùng dừng typing |
| BroadcastNotification | Thông báo broadcast |
| ChatroomNotification | Thông báo trong chatroom |

---

## Error Codes

### HTTP Status Codes:
| Mã lỗi | Ý nghĩa |
|--------|---------|
| 200 OK | Thành công |
| 201 Created | Tạo mới thành công |
| 204 No Content | Thành công nhưng không có dữ liệu trả về |
| 400 Bad Request | Dữ liệu đầu vào không hợp lệ |
| 401 Unauthorized | Không có quyền truy cập |
| 403 Forbidden | Bị cấm truy cập |
| 404 Not Found | Không tìm thấy tài nguyên |
| 409 Conflict | Xung đột dữ liệu |
| 500 Internal Server Error | Lỗi server |

### Typical Error Response:
| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| success | boolean | Trạng thái thành công (false) |
| message | string | Mô tả lỗi |
| details | string | Chi tiết lỗi (nếu có) |

---

## Authentication & Authorization

Hầu hết các API endpoint yêu cầu authentication thông qua Firebase JWT token.

**Headers yêu cầu:**
| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| Authorization | string | Bearer {firebase_jwt_token} |
| UserId | string | ID người dùng hiện tại (một số endpoint) |

---

## WebSocket Connection

Để kết nối SignalR Hub:

| Tham số | Kiểu dữ liệu | Ý nghĩa |
|---------|--------------|---------|
| **Endpoint** | string | /chathub |
| **Protocol** | string | WebSocket/SignalR |

**JavaScript Example:**
```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chathub")
    .build();
```

**Lưu ý:** Đảm bảo authenticate trước khi join các group chatroom để nhận tin nhắn realtime.
