using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server_1_.Models
{
    public class Participants
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

        public DateTime JoinedAt { get; set; }
        public DateTime? LeftAt { get; set; }
        public string Role { get; set; } 
    }
}
