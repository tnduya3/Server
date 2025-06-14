using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server_1_.Models
{    public class ChatRooms
    {
        [Key]
        public int ChatRoomId { get; set; }

        [ForeignKey("Users")]
        public int CreatedBy { get; set; } // ID of the user who created the chat room

        public string? Name { get; set; }
        public string? Description { get; set; }
        public bool IsGroup { get; set; } = true; // Indicates if the chat room is a group chat
        public bool IsPrivate { get; set; } = false; // Private chatroom (invite only)
        public bool IsArchived { get; set; } = false; // Archived status
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastActivity { get; set; }
        public bool IsDeleted { get; set; } = false;
        public int? UpdatedBy { get; set; } // Who last updated the chatroom
        public int? DeletedBy { get; set; } // Who deleted the chatroom
        public DateTime? DeletedAt { get; set; }
        
        // Navigation properties
        public Users? Creator { get; set; } // Navigation to user who created the room
        public ICollection<Participants>? Participants { get; set; } = new List<Participants>(); // List of participants in the chat room
        public ICollection<Messages>? Messages { get; set; } = new List<Messages>();
    }
}
