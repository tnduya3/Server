using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server_1_.Models
{
    public class Friends
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public required Users User { get; set; }

        [ForeignKey("Users")] // Để phân biệt với UserId
        public int FriendId { get; set; }

        //[InverseProperty("Users")] // Thuộc tính điều hướng ngược lại trong User
        //public required Users FriendUser { get; set; }

        public DateTime CreatedAt { get; set; }
        public enum FriendshipStatus
        {
            Pending,
            Accepted,
            Blocked,
            Unfriended
        }
    }
}
