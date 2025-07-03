using Microsoft.AspNetCore.SignalR;
using Server_1_.Services;
using Server_1_.Models;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Server_1_.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IMessageService _messageService;
        private readonly IUserService _userService; 
        private readonly IChatroomService _chatroomService;
        
        // Dictionary để theo dõi người dùng nào đang online
        private static readonly ConcurrentDictionary<string, string> ConnectedUsers = new();
        private static readonly ConcurrentDictionary<string, List<string>> UserGroups = new();

        // Inject các service cần thiết vào constructor
        public ChatHub(IMessageService messageService, IUserService userService, IChatroomService chatroomService)
        {
            _messageService = messageService;
            _userService = userService;
            _chatroomService = chatroomService;
        }        // Phương thức để user đăng ký với Hub bằng UserID
        public async Task RegisterUser(string userId)
        {
            ConnectedUsers[Context.ConnectionId] = userId;
            
            // Thông báo cho tất cả friends rằng user này đã online
            await Clients.All.SendAsync("UserOnline", userId);
        }        
        
        // Phương thức mà client có thể gọi để gửi tin nhắn
        public async Task SendMessage(int senderId, int chatroomId, string messageContent)
        {
            try
            {
                // 1. Lưu tin nhắn vào cơ sở dữ liệu thông qua MessageService
                // (Firebase notification sẽ được gửi tự động trong MessageService)
                var message = await _messageService.SendMessageAsync(senderId, chatroomId, messageContent);

                // 2. Tạo đối tượng tin nhắn để gửi đến client sử dụng SenderName từ model
                var messageResponse = new
                {
                    MessageId = message.MessageId,
                    SenderId = message.SenderId,
                    SenderName = message.SenderName, // Sử dụng SenderName từ model Messages
                    ChatroomId = message.ChatRoomId,
                    Content = message.Message,
                    CreatedAt = message.CreatedAt,
                    MessageType = "text" // Có thể mở rộng cho các loại tin nhắn khác (hình ảnh, file...)
                };

                // 3. Gửi tin nhắn đến tất cả các client trong nhóm (chatroom) cụ thể
                await Clients.Group(chatroomId.ToString()).SendAsync("ReceiveMessage", messageResponse);

                // 4. Gửi notification đến các members trong chatroom (trừ sender)
                await SendChatroomNotification(chatroomId, senderId, message.SenderName ?? "Unknown", messageContent, message.MessageId);

                // 5. Gửi thông báo đã gửi thành công cho người gửi
                await Clients.Caller.SendAsync("MessageSent", new { 
                    MessageId = message.MessageId, 
                    Status = "success",
                    FirebaseNotificationSent = true // Xác nhận Firebase notification đã được gửi
                });

                // 6. Log thành công
                Console.WriteLine($"Message sent via SignalR - ID: {message.MessageId}, SenderName: {message.SenderName}, Firebase notifications sent");
            }
            catch (Exception ex)
            {
                // Xử lý lỗi khi gửi tin nhắn
                Console.WriteLine($"Error sending message via SignalR: {ex.Message}");
                await Clients.Caller.SendAsync("ReceiveError", $"Failed to send message: {ex.Message}");
                await Clients.Caller.SendAsync("MessageSent", new { 
                    Status = "failed", 
                    Error = ex.Message 
                });
            }
        }

        // Phương thức gửi tin nhắn typing indicator
        public async Task SendTyping(int senderId, int chatroomId, string senderName)
        {
            try
            {
                await Clients.OthersInGroup(chatroomId.ToString()).SendAsync("UserTyping", new
                {
                    SenderId = senderId,
                    ChatroomId = chatroomId,
                    SenderName = senderName,
                    Timestamp = DateTime.UtcNow
                });
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending typing indicator: {ex.Message}");
                await Clients.Caller.SendAsync("ReceiveError", $"Failed to send typing indicator: {ex.Message}");
            }
        }

        // Phương thức ngừng typing
        public async Task StopTyping(int senderId, int chatroomId)
        {
            try
            {
                await Clients.OthersInGroup(chatroomId.ToString()).SendAsync("UserStoppedTyping", new
                {
                    SenderId = senderId,
                    ChatroomId = chatroomId,
                    Timestamp = DateTime.UtcNow
                });
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending stop typing indicator: {ex.Message}");
                await Clients.Caller.SendAsync("ReceiveError", $"Failed to send stop typing indicator: {ex.Message}");
            }
        }

        // Phương thức đánh dấu tin nhắn đã đọc
        public async Task MarkMessageAsRead(int messageId, int userId, int chatroomId)
        {
            try
            {
                // Có thể thêm logic để lưu trạng thái đã đọc vào database
                // await _messageService.MarkAsReadAsync(messageId, userId);

                await Clients.OthersInGroup(chatroomId.ToString()).SendAsync("MessageRead", new
                {
                    MessageId = messageId,
                    ReadBy = userId,
                    ReadAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error marking message as read: {ex.Message}");
            }
        }        // Phương thức mà client có thể gọi để tham gia một phòng chat
        public async Task JoinChatroom(string chatroomId, string userId)
        {
            try
            {
                // Thêm kết nối hiện tại vào một nhóm (group) SignalR
                await Groups.AddToGroupAsync(Context.ConnectionId, chatroomId);
                
                // Lưu thông tin group mà user đã join
                if (!UserGroups.ContainsKey(Context.ConnectionId))
                {
                    UserGroups[Context.ConnectionId] = new List<string>();
                }
                UserGroups[Context.ConnectionId].Add(chatroomId);

                // Gửi xác nhận cho user hiện tại
                await Clients.Caller.SendAsync("JoinConfirmation", new
                {
                    ChatroomId = chatroomId,
                    Message = $"You have joined chatroom {chatroomId}",
                    JoinedAt = DateTime.UtcNow
                });

                // Thông báo cho các user khác trong room (nếu có userId)
                if (!string.IsNullOrEmpty(userId))
                {
                    var user = await _userService.GetUserByIdAsync(int.Parse(userId));
                    if (user != null)
                    {
                        await Clients.OthersInGroup(chatroomId).SendAsync("UserJoinedChatroom", new
                        {
                            UserId = userId,
                            Username = user.UserName,
                            ChatroomId = chatroomId,
                            JoinedAt = DateTime.UtcNow
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error joining chatroom: {ex.Message}");
                await Clients.Caller.SendAsync("ReceiveError", "Failed to join chatroom");
            }
        }

        // Phương thức mà client có thể gọi để rời một phòng chat
        public async Task LeaveChatroom(string chatroomId, string? userId = null)
        {
            try
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatroomId);
                
                // Xóa thông tin group
                if (UserGroups.ContainsKey(Context.ConnectionId))
                {
                    UserGroups[Context.ConnectionId].Remove(chatroomId);
                }

                // Gửi xác nhận cho user hiện tại
                await Clients.Caller.SendAsync("LeaveConfirmation", new
                {
                    ChatroomId = chatroomId,
                    Message = $"You have left chatroom {chatroomId}",
                    LeftAt = DateTime.UtcNow
                });

                // Thông báo cho các user khác trong room
                if (!string.IsNullOrEmpty(userId))
                {
                    var user = await _userService.GetUserByIdAsync(int.Parse(userId));
                    if (user != null)
                    {
                        await Clients.OthersInGroup(chatroomId).SendAsync("UserLeftChatroom", new
                        {
                            UserId = userId,
                            Username = user.UserName,
                            ChatroomId = chatroomId,
                            LeftAt = DateTime.UtcNow
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error leaving chatroom: {ex.Message}");
                await Clients.Caller.SendAsync("ReceiveError", "Failed to leave chatroom");
            }
        }

        // Phương thức lấy danh sách users online trong chatroom
        public async Task GetOnlineUsersInChatroom(string chatroomId)
        {
            // Logic để lấy danh sách users online
            // Có thể implement bằng cách lưu trữ mapping giữa connectionId và userId
            var onlineUsers = new List<object>(); // Placeholder
            
            await Clients.Caller.SendAsync("OnlineUsersInChatroom", new
            {
                ChatroomId = chatroomId,
                OnlineUsers = onlineUsers,
                Count = onlineUsers.Count
            });
        }        
        
        // Các phương thức life-cycle của Hub
        public override async Task OnConnectedAsync()
        {
            // Xử lý khi một client kết nối đến Hub
            Console.WriteLine($"Client connected: {Context.ConnectionId} at {DateTime.UtcNow}");
            
            // Gửi thông báo kết nối thành công
            await Clients.Caller.SendAsync("Connected", new
            {
                ConnectionId = Context.ConnectionId,
                ConnectedAt = DateTime.UtcNow,
                Message = "Connected to chat hub successfully"
            });
            
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Xử lý khi một client ngắt kết nối khỏi Hub
            Console.WriteLine($"Client disconnected: {Context.ConnectionId} at {DateTime.UtcNow}");
            
            // Lấy userId nếu có
            if (ConnectedUsers.TryRemove(Context.ConnectionId, out string? userId))
            {
                // Thông báo cho tất cả friends rằng user này đã offline
                await Clients.All.SendAsync("UserOffline", userId);
            }

            // Xóa user khỏi tất cả groups
            if (UserGroups.TryRemove(Context.ConnectionId, out List<string>? groups))
            {
                foreach (var group in groups)
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);
                    // Thông báo user đã rời khỏi group
                    if (!string.IsNullOrEmpty(userId))
                    {
                        await Clients.Group(group).SendAsync("UserLeftChatroom", new
                        {
                            UserId = userId,
                            ChatroomId = group,
                            LeftAt = DateTime.UtcNow,
                            Reason = "Disconnected"
                        });
                    }
                }
            }

            if (exception != null)
            {
                Console.WriteLine($"Disconnection exception: {exception.Message}");
            }
            
            await base.OnDisconnectedAsync(exception);
        }

        // Phương thức kiểm tra trạng thái kết nối
        public async Task Ping()
        {
            await Clients.Caller.SendAsync("Pong", DateTime.UtcNow);
        }

        // Phương thức gửi thông báo lỗi chung
        private async Task SendErrorToUser(string message, object? details = null)
        {
            await Clients.Caller.SendAsync("ReceiveError", new
            {
                Message = message,
                Details = details,
                Timestamp = DateTime.UtcNow
            });
        }

        // Phương thức gửi thông báo khi user nhận được tin nhắn
        public async Task SendNotificationToUser(int recipientUserId, int senderId, string senderName, int chatroomId, string messageContent, int messageId)
        {
            try
            {
                // Tạo notification data
                var notificationData = new
                {
                    Type = "new_message",
                    MessageId = messageId,
                    SenderId = senderId,
                    SenderName = senderName,
                    ChatroomId = chatroomId,
                    Content = messageContent.Length > 50 ? messageContent.Substring(0, 50) + "..." : messageContent,
                    FullContent = messageContent,
                    Timestamp = DateTime.UtcNow,
                    Title = $"New message from {senderName}",
                    Body = messageContent
                };

                // Gửi notification đến user cụ thể (nếu họ đang online)
                var recipientConnectionId = ConnectedUsers.FirstOrDefault(x => x.Value == recipientUserId.ToString()).Key;
                
                if (!string.IsNullOrEmpty(recipientConnectionId))
                {
                    // User đang online, gửi notification qua SignalR
                    await Clients.Client(recipientConnectionId).SendAsync("ReceiveNotification", notificationData);
                    
                    Console.WriteLine($"Notification sent to online user {recipientUserId} for message {messageId}");
                }
                else
                {
                    // User offline, có thể lưu notification để gửi sau hoặc dùng push notification
                    Console.WriteLine($"User {recipientUserId} is offline, notification not sent via SignalR");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending notification to user {recipientUserId}: {ex.Message}");
            }
        }

        // Phương thức để gửi notification broadcast cho tất cả members trong chatroom (trừ sender)
        public async Task SendChatroomNotification(int chatroomId, int senderId, string senderName, string messageContent, int messageId)
        {
            try
            {
                var notificationData = new
                {
                    Type = "chatroom_message",
                    MessageId = messageId,
                    SenderId = senderId,
                    SenderName = senderName,
                    ChatroomId = chatroomId,
                    Content = messageContent.Length > 50 ? messageContent.Substring(0, 50) + "..." : messageContent,
                    Timestamp = DateTime.UtcNow,
                    Title = $"New message in chatroom",
                    Body = $"{senderName}: {messageContent}"
                };

                // Gửi notification đến tất cả members trong chatroom trừ sender
                await Clients.GroupExcept(chatroomId.ToString(), 
                    ConnectedUsers.Where(x => x.Value == senderId.ToString()).Select(x => x.Key).ToList())
                    .SendAsync("ReceiveNotification", notificationData);
                
                Console.WriteLine($"Chatroom notification sent for message {messageId} in room {chatroomId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending chatroom notification: {ex.Message}");
            }
        }

        // Phương thức để mark notification đã đọc
        public async Task MarkNotificationAsRead(int notificationId, int userId)
        {
            try
            {
                // Logic để mark notification đã đọc (có thể lưu vào database)
                
                // Thông báo cho client rằng notification đã được đánh dấu đọc
                await Clients.Caller.SendAsync("NotificationMarkedAsRead", new
                {
                    NotificationId = notificationId,
                    UserId = userId,
                    MarkedAt = DateTime.UtcNow
                });
                
                Console.WriteLine($"Notification {notificationId} marked as read by user {userId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error marking notification as read: {ex.Message}");
            }
        }

        // Phương thức để test typing trực tiếp
        public async Task TestTyping()
        {
            try
            {
                Console.WriteLine("TestTyping method called!");
                await Clients.Caller.SendAsync("ReceiveError", "TestTyping method successfully called!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in TestTyping: {ex.Message}");
            }
        }
    }
}
