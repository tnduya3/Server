using Server_1_.Models;
using Server_1_.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Server_1_.Services.FirebaseNotificationService;
using FirebaseAdmin.Messaging;

namespace Server_1_.Services
{
    public class MessageService : IMessageService
    {
        private readonly AppDbContext _context;
        private readonly IFirebaseNotificationService _firebaseNotificationService; // Inject service  

        public MessageService(AppDbContext context, IFirebaseNotificationService firebaseNotificationService)
        {
            _context = context; // Khởi tạo DbContext  
            _firebaseNotificationService = firebaseNotificationService; // Correctly assign the injected service  
        }        public async Task<Messages> SendMessageAsync(int senderId, int chatroomId, string content)
        {
            // Get sender information to store username
            var sender = await _context.Users.FindAsync(senderId);
            
            var message = new Messages
            {
                SenderId = senderId,
                SenderName = sender?.UserName ?? "Unknown", // Store sender name in the message
                ChatRoomId = chatroomId,
                Message = content,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Messages.Add(message); // Thêm tin nhắn vào DbSet  
            await _context.SaveChangesAsync(); // Lưu thay đổi vào cơ sở dữ liệu

            // Gửi thông báo FCM sau khi tin nhắn được lưu  
            var participants = await _context.Participants
                                    .Where(p => p.ChatroomId == chatroomId && p.UserId != senderId && p.LeftAt == null)
                                    .Include(p => p.User)
                                    .ToListAsync();

            foreach (var participant in participants)
            {
                // Lấy DeviceToken từ người dùng. Đảm bảo User có cột DeviceToken
                string deviceToken = participant.User?.DeviceToken; // Use null-conditional operator for safety

                if (!string.IsNullOrEmpty(deviceToken))
                {
                    var senderUser = await _context.Users.FindAsync(senderId); // Lấy thông tin người gửi
                    var chatroom = await _context.Chatrooms.FindAsync(chatroomId); // Lấy thông tin chatroom

                    var notificationData = new Dictionary<string, string>
                    {
                        { "chatroomId", chatroomId.ToString() },
                        { "senderId", senderId.ToString() },
                        { "reatedAt", message.CreatedAt.ToString() },
                        { "type", "new_message" }
                    };
                    await _firebaseNotificationService.SendNotificationToDeviceAsync(
                        deviceToken,
                        $"{senderUser?.UserName ?? "Someone"} in {chatroom?.Name ?? "a chatroom"}",
                        message.Message,
                        notificationData
                    );
                }
            }
                return message;
        }

        public async Task<List<Messages>> GetMessagesInChatroomAsync(int chatroomId, int pageNumber = 1, int pageSize = 20)
        {
            return await _context.Messages
                .Where(m => m.ChatRoomId == chatroomId)
                .OrderByDescending(m => m.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<Messages> GetMessageByIdAsync(int messageId)
        {
            var message = await _context.Messages.FindAsync(messageId);
            if (message == null)
            {
                throw new Exception("Message not found"); // Hoặc tạo một custom exception của bạn  
            }
            return message;
        }

        public async Task<bool> EditMessageAsync(int messageId, string newContent)
        {
            var message = await _context.Messages.FindAsync(messageId);
            if (message == null)
            {
                return false;
            }

            message.Message = newContent;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteMessageAsync(int messageId)
        {
            var message = await _context.Messages.FindAsync(messageId);
            if (message == null)
            {
                return false;
            }

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ParticipantWithToken>> GetParticipantsWithTokensAsync(int chatroomId)
        {
            return await _context.Participants
                .Where(p => p.ChatroomId == chatroomId && p.LeftAt == null)
                .Include(p => p.User)
                .Where(p => !string.IsNullOrEmpty(p.User.DeviceToken))
                .Select(p => new ParticipantWithToken
                {
                    UserId = p.UserId,
                    UserName = p.User.UserName ?? "Unknown",
                    DeviceToken = p.User.DeviceToken!
                })
                .ToListAsync();
        }
        // Implement các phương thức khác của IMessageService  
    }
}
