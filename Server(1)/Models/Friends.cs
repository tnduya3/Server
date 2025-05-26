using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server_1_.Models
{
    public class Friends
    {
        // Khóa chính kết hợp
        public int UserId { get; set; }
        public int FriendId { get; set; }

        [ForeignKey("UserId")]
        public Users User { get; set; }

        [ForeignKey("FriendId")] // Để phân biệt với UserId
        public Users FriendUser { get; set; }

        public DateTime CreatedAt { get; set; }
        public FriendshipStatus Status { get; set; }
        public enum FriendshipStatus
        {
            Pending,
            Accepted,
            Blocked,
            Unfriended
        }
    }
}
