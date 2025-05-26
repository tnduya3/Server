using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server_1_.Models
{
    public class Users
    {
        [Key]
        public int UserId { get; set; }

        public string? UserName { get; set; }
        public string? Token { get; set; }
        public bool IsOnline { get; set; }

        // Mối quan hệ: User này là người gửi yêu cầu kết bạn (UserId)  
        [InverseProperty("User")] // "User" là tên thuộc tính điều hướng trong model Friend  
        public ICollection<Friends> SentFriendRequests { get; set; } = new List<Friends>();

        // Mối quan hệ: User này là người được gửi yêu cầu kết bạn (FriendId)  
        [InverseProperty("FriendUser")] // "FriendUser" là tên thuộc tính điều hướng trong model Friend  
        public ICollection<Friends> ReceivedFriendRequests { get; set; } = new List<Friends>();
    }
}
