using FirebaseAdmin.Messaging;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using System.Text;
using Microsoft.AspNetCore.Hosting;

namespace Server_1_.Services
{
    public class FirebaseNotificationService : IFirebaseNotificationService
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public FirebaseNotificationService(IConfiguration configuration, IWebHostEnvironment env) // Inject IWebHostEnvironment
        {
            _configuration = configuration;
            _env = env;

            if (FirebaseApp.DefaultInstance == null)
            {
                // Lấy đường dẫn đến file JSON Service Account
                var serviceAccountPath = Path.Combine(_env.ContentRootPath, "realtimechatapplicationg10-firebase-adminsdk-fbsvc-1c9e190b3c.json");

                if (!File.Exists(serviceAccountPath))
                {
                    throw new FileNotFoundException("Firebase Service Account JSON file not found.", serviceAccountPath);
                }

                using (var stream = new FileStream(serviceAccountPath, FileMode.Open, FileAccess.Read))
                {
                    FirebaseApp.Create(new AppOptions()
                    {
                        Credential = GoogleCredential.FromStream(stream)
                    });
                }
            }
        }

        public async Task<string> SendNotificationToDeviceAsync(string deviceToken, string title, string body, Dictionary<string, string> data = null)
        {
            var message = new Message()
            {
                Notification = new Notification
                {
                    Title = title,
                    Body = body,
                },
                Data = data, // Dữ liệu tùy chỉnh, có thể được ứng dụng client xử lý
                Token = deviceToken, // Gửi đến một thiết bị cụ thể
            };

            // Send a message to the device corresponding to the provided registration token.
            string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            return response; // Response chứa MessageId
        }

        public async Task<string> SendNotificationToTopicAsync(string topic, string title, string body, Dictionary<string, string> data = null)
        {
            var message = new Message()
            {
                Notification = new Notification
                {
                    Title = title,
                    Body = body,
                },
                Data = data,
                Topic = topic, // Gửi đến tất cả các thiết bị đã đăng ký topic này
            };

            string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            return response;
        }
    }

    // Class để ánh xạ cấu hình Firebase từ appsettings.json
    public class FirebaseConfig
    {
        public string ProjectId { get; set; }
        public string PrivateKeyId { get; set; }
        public string PrivateKey { get; set; }
        public string ClientEmail { get; set; }
        public string ClientId { get; set; }
        public string AuthUri { get; set; }
        public string TokenUri { get; set; }
        public string AuthProviderX509CertUrl { get; set; }
        public string ClientX509CertUrl { get; set; }
        public string UniverseDomain { get; set; }
    }
}
