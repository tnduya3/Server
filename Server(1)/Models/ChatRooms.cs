using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server_1_.Models
{
    public class ChatRooms
    {
        [Key]
        public int ChatRoomId { get; set; }

        [ForeignKey("Users")]
        public int CreatedBy { get; set; } // ID of the user who created the chat room

        public string? Name { get; set; }
        public bool IsGroup { get; set; } // Indicates if the chat room is a group chat
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public ICollection<Participants>? Participants { get; set; } = new List<Participants>(); // List of participants in the chat room
        public ICollection<Messages>? Messages { get; set; } = new List<Messages>();
    }
}
