using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server_1_.Models
{    public class Users
    {
        [Key]
        public int UserId { get; set; }

        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? Token { get; set; }
        public bool IsOnline { get; set; }
        public bool IsActive { get; set; } = true;
        public string? DeviceToken { get; set; } // Thêm dòng này
        public string? Avatar { get; set; } // URL hoặc đường dẫn tới avatar
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Mối quan hệ: User này là người gửi yêu cầu kết bạn (UserId)  
        [InverseProperty("User")] // "User" là tên thuộc tính điều hướng trong model Friend  
        public ICollection<Friends> SentFriendRequests { get; set; } = new List<Friends>();

        // Mối quan hệ: User này là người được gửi yêu cầu kết bạn (FriendId)  
        [InverseProperty("FriendUser")] // "FriendUser" là tên thuộc tính điều hướng trong model Friend  
        public ICollection<Friends> ReceivedFriendRequests { get; set; } = new List<Friends>();
    }
}
