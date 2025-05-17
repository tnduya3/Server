using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server_1_.Models
{
    public class Notifications
    {
        [Key]
        public int Id { get; set; } // NotificationId, là khóa chính duy nhất

        // Khóa ngoại đến bảng User (người nhận thông báo)
        [ForeignKey("Users")]
        public int UserId { get; set; }
        public required Users Recipient { get; set; }

        // Khóa ngoại đến bảng User (người gửi thông báo, có thể null nếu là thông báo hệ thống)
        [ForeignKey("Sender")]
        public int? SenderId { get; set; } // Cho phép null
        public required Users Sender { get; set; }

        public required string Type { get; set; } // Loại thông báo (ví dụ: new_message, friend_request, chatroom_invite)
        public required string Content { get; set; } // Nội dung thông báo (có thể là JSON để linh hoạt)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
    }
}
