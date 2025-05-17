using Server_1_.Models;
using Server_1_.Data; // Giả sử bạn đặt AppDbContext trong folder Data
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server_1_.Services
{
    public class MessageService : IMessageService
    {
        private readonly AppDbContext _context;

        public MessageService(AppDbContext context)
        {
            _context = context; // Khởi tạo DbContext
        }

        public async Task<Messages> SendMessageAsync(int senderId, int chatroomId, string content)
        {
            var message = new Messages
            {
                SenderId = senderId,
                ChatRoomId = chatroomId,
                Message = content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Messages.Add(message); // Thêm tin nhắn vào DbSet
            await _context.SaveChangesAsync(); // Lưu thay đổi vào cơ sở dữ liệu
            return message;
        }

        public async Task<List<Messages>> GetMessagesInChatroomAsync(int chatroomId, int pageNumber = 1, int pageSize = 20)
        {
            return await _context.Messages
                .Where(m => m.ChatRoomId == chatroomId)
                .OrderByDescending(m => m.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
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
        // Implement các phương thức khác của IMessageService
    }
}
