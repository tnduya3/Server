using Microsoft.AspNetCore.SignalR;
using Server_1_.Services;
using Server_1_.Models;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System;


namespace Server_1_.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IMessageService _messageService;
        private readonly IUserService _userService; 
        private readonly IChatroomService _chatroomService; 

        // Inject các service cần thiết vào constructor
        public ChatHub(IMessageService messageService, IUserService userService, IChatroomService chatroomService)
        {
            _messageService = messageService;
            _userService = userService;
            _chatroomService = chatroomService;
        }

        // Phương thức mà client có thể gọi để gửi tin nhắn
        public async Task SendMessage(int senderId, int chatroomId, string messageContent)
        {
            try
            {
                // 1. Lưu tin nhắn vào cơ sở dữ liệu thông qua MessageService
                var message = await _messageService.SendMessageAsync(senderId, chatroomId, messageContent);

                // 2. Lấy thông tin người gửi (username) để hiển thị trên client
                var sender = await _userService.GetUserByIdAsync(senderId);
                if (sender == null)
                {
                    // Xử lý trường hợp không tìm thấy người gửi
                    // Có thể log lỗi hoặc thông báo cho người gửi biết
                    await Clients.Caller.SendAsync("ReceiveError", "Sender not found.");
                    return;
                }

                // 3. Gửi tin nhắn đến tất cả các client trong nhóm (chatroom) cụ thể
                // 'ReceiveMessage' là tên hàm client sẽ lắng nghe
                await Clients.Group(chatroomId.ToString()).SendAsync("ReceiveMessage", new
                {
                    Id = message.MessageId,
                    SenderId = message.SenderId,
                    SenderUsername = sender.UserName, // Thêm username để hiển thị
                    ChatroomId = message.ChatRoomId,
                    Content = message.Message,
                    CreatedAt = message.CreatedAt
                });

                // Hoặc Clients.All.SendAsync("ReceiveMessage", senderId, messageContent); nếu muốn gửi đến tất cả

            }
            catch (Exception ex)
            {
                // Xử lý lỗi khi gửi tin nhắn (ví dụ: log lỗi, thông báo cho người gửi)
                Console.WriteLine($"Error sending message: {ex.Message}");
                await Clients.Caller.SendAsync("ReceiveError", $"Failed to send message: {ex.Message}");
            }
        }

        // Phương thức mà client có thể gọi để tham gia một phòng chat
        public async Task JoinChatroom(string chatroomId)
        {
            // Thêm kết nối hiện tại vào một nhóm (group) SignalR.
            // Các group trong SignalR được định danh bằng string.
            // Chúng ta sẽ dùng ID của chatroom làm tên group.
            await Groups.AddToGroupAsync(Context.ConnectionId, chatroomId);
            await Clients.Caller.SendAsync("JoinConfirmation", $"You have joined chatroom {chatroomId}.");
            await Clients.Group(chatroomId).SendAsync("UserJoined", $"{Context.ConnectionId} has joined the chatroom {chatroomId}.");
        }

        // Phương thức mà client có thể gọi để rời một phòng chat
        public async Task LeaveChatroom(string chatroomId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatroomId);
            await Clients.Caller.SendAsync("LeaveConfirmation", $"You have left chatroom {chatroomId}.");
            await Clients.Group(chatroomId).SendAsync("UserLeft", $"{Context.ConnectionId} has left the chatroom {chatroomId}.");
        }

        // Các phương thức life-cycle của Hub (tùy chọn để override)
        public override async Task OnConnectedAsync()
        {
            // Xử lý khi một client kết nối đến Hub
            Console.WriteLine($"Client connected: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Xử lý khi một client ngắt kết nối khỏi Hub
            Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
            await base.OnDisconnectedAsync(exception);
        }
    }
}
