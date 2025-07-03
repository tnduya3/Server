namespace Server_1_.Services
{
    public interface IFirebaseNotificationService
    {
        // Đảm bảo phương thức này tồn tại trong interface
        Task<string> SendNotificationToDeviceAsync(string deviceToken, string title, string body, Dictionary<string, string>? data = null);
        Task<string> SendNotificationToTopicAsync(string topic, string title, string body, Dictionary<string, string>? data = null);
        // Thêm các phương thức khác nếu cần
    }
}
