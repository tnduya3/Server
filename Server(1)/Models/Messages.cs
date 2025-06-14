using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server_1_.Models
{
    public class Messages
    {
        [Key]
        public int MessageId { get; set; }

        [ForeignKey("Users")]
        public int UserId { get; set; }
        public int SenderId { get; set; }
        public string? SenderName { get; set; } // Optional: to store sender's name directly in the message
        
        [ForeignKey("ChatRooms")]
        public int ChatRoomId { get; set; }

        public string? Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}
