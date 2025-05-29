namespace Server_1_.Models
{
    public class DeviceTokenDto
    {
        public int UserId { get; set; } // ID của người dùng mà token này thuộc về
        public string Token { get; set; } // Device Token từ FCM
    }
}
