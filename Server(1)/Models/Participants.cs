using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server_1_.Models
{    public class Participants
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        // Adding the 'required' modifier to ensure the property is initialized  
        public Users User { get; set; }

        [ForeignKey("ChatRooms")]
        public int ChatroomId { get; set; }
        public ChatRooms Chatroom { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LeftAt { get; set; }
        public string Role { get; set; } = "member"; // member, admin, owner, moderator
        public int? AddedBy { get; set; } // Who added this user
        public int? RemovedBy { get; set; } // Who removed this user
        public bool IsActive { get; set; } = true; // Is still active in the chatroom
        public DateTime? LastSeen { get; set; } // Last time user was active in this chatroom
        
        // Navigation properties
        public Users? AddedByUser { get; set; }
        public Users? RemovedByUser { get; set; }
    }
}
