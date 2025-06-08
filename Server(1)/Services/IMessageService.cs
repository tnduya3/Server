using Server_1_.Models; // Đảm bảo namespace của Models được import
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Server_1_.Services
{
    public interface IMessageService
    {
        Task<Messages> SendMessageAsync(int senderId, int chatroomId, string content);
        Task<List<Messages>> GetMessagesInChatroomAsync(int chatroomId, int pageNumber = 1, int pageSize = 20);
        Task<Messages> GetMessageByIdAsync(int messageId);
        Task<bool> EditMessageAsync(int messageId, string newContent);
        Task<bool> DeleteMessageAsync(int messageId);
        Task<IEnumerable<ParticipantWithToken>> GetParticipantsWithTokensAsync(int chatroomId);
        // Các phương thức khác liên quan đến tin nhắn (ví dụ: đánh dấu đã đọc)
    }

    // DTO cho participants với device tokens
    public class ParticipantWithToken
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string DeviceToken { get; set; } = string.Empty;
    }
}