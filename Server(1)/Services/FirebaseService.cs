// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text;
// using System.Threading.Tasks;
// using FirebaseAdmin;
// using FirebaseAdmin.Auth;
// using Google.Apis.Auth.OAuth2;
// using System.Windows.Forms; // Để hiển thị MessageBox
// using System.Net.Http;
// using Newtonsoft.Json;

// public class FirebaseAuthService
// {
//     private static FirebaseApp firebaseApp;

//     public static void InitializeFirebaseApp()
//     {
//         if (firebaseApp == null)
//         {
//             try
//             {
//                 var defaultApp = FirebaseApp.DefaultInstance;
//                 if (defaultApp == null)
//                 {
//                     string serviceAccountKeyPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Firebase", "realtimechatapplicationg10-firebase-adminsdk-fbsvc-1c9e190b3c.json");
//                     firebaseApp = FirebaseApp.Create(new AppOptions()
//                     {
//                         Credential = GoogleCredential.FromFile(serviceAccountKeyPath),
//                         ProjectId = Environment.GetEnvironmentVariable("FIREBASE_PROJECT_ID") // Thay thế bằng Project ID của bạn
//                     });
//                 }
//                 else
//                 {
//                     firebaseApp = defaultApp;
//                 }
//             }
//             catch (System.Exception ex)
//             {
//                 MessageBox.Show($"Lỗi khởi tạo Firebase: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
//             }
//         }
//     }

//     private static readonly string apiKey = Environment.GetEnvironmentVariable("FIREBASE_API_KEY"); // Thay thế bằng API Key của bạn
//     private static readonly HttpClient httpClient = new HttpClient();

//     public static async Task<string> SignInWithEmailAndPasswordAsync(string email, string password)
//     {
//         string requestUri = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={apiKey}";
//         var request = new
//         {
//             email = email,
//             password = password,
//             returnSecureToken = true
//         };
//         var jsonRequest = JsonConvert.SerializeObject(request);
//         var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

//         try
//         {
//             var response = await httpClient.PostAsync(requestUri, content);
//             var responseContent = await response.Content.ReadAsStringAsync();
//             if (response.IsSuccessStatusCode)
//             {
//                 // Đăng nhập thành công, trả về ID token (nếu có) hoặc thông tin người dùng
//                 dynamic jsonResponse = JsonConvert.DeserializeObject(responseContent);
//                 if (jsonResponse.idToken != null)
//                 {
//                     return (string)jsonResponse.idToken;
//                 }
//                 else
//                 {
//                     return responseContent; // Trả về toàn bộ response nếu không có idToken
//                 }
//             }
//             else
//             {
//                 // Đăng nhập thất bại, trả về thông báo lỗi
//                 dynamic jsonResponse = JsonConvert.DeserializeObject(responseContent);
//                 if (jsonResponse?.error?.message != null)
//                 {
//                     return (string)jsonResponse.error.message;
//                 }
//                 else
//                 {
//                     return $"Đăng nhập thất bại: {response.StatusCode}";
//                 }
//             }
//         }
//         catch (AggregateException ex)
//         {
//             foreach (var innerException in ex.InnerExceptions)
//             {
//                 MessageBox.Show($"Lỗi bên trong: {innerException.Message}", "Lỗi chi tiết", MessageBoxButtons.OK, MessageBoxIcon.Error);
//                 // Ghi log lỗi chi tiết hơn nếu cần
//             }
//             MessageBox.Show($"Lỗi khởi tạo Firebase: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
//         }
//         catch (System.Exception ex)
//         {
//             MessageBox.Show($"Lỗi khởi tạo Firebase: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
//         }

//         return null;
//     }

//     public static async Task<string> SignUpWithEmailAndPasswordAsync(string email, string password)
//     {
//         string requestUri = $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={apiKey}";
//         var request = new
//         {
//             email = email,
//             password = password,
//             returnSecureToken = true
//         };
//         var jsonRequest = JsonConvert.SerializeObject(request);
//         var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

//         try
//         {
//             var response = await httpClient.PostAsync(requestUri, content);
//             var responseContent = await response.Content.ReadAsStringAsync();
//             if (response.IsSuccessStatusCode)
//             {
//                 // Đăng ký thành công, trả về ID token (nếu có) hoặc thông tin người dùng
//                 dynamic jsonResponse = JsonConvert.DeserializeObject(responseContent);
//                 if (jsonResponse.idToken != null)
//                 {
//                     return (string)jsonResponse.idToken;
//                 }
//                 else
//                 {
//                     return responseContent; // Trả về toàn bộ response nếu không có idToken
//                 }
//             }
//             else
//             {
//                 // Đăng ký thất bại, trả về thông báo lỗi
//                 dynamic jsonResponse = JsonConvert.DeserializeObject(responseContent);
//                 if (jsonResponse?.error?.message != null)
//                 {
//                     return (string)jsonResponse.error.message;
//                 }
//                 else
//                 {
//                     return $"Đăng ký thất bại: {response.StatusCode}";
//                 }
//             }
//         }
//         catch (HttpRequestException ex)
//         {
//             return $"Lỗi kết nối: {ex.Message}";
//         }
//     }

//     public static async Task<string> SendPasswordResetEmailAsync(string email)
//     {
//         string requestUri = $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={apiKey}";
//         var request = new
//         {
//             requestType = "PASSWORD_RESET",
//             email = email
//         };
//         var jsonRequest = JsonConvert.SerializeObject(request);
//         var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

//         try
//         {
//             var response = await httpClient.PostAsync(requestUri, content);
//             var responseContent = await response.Content.ReadAsStringAsync();
//             if (response.IsSuccessStatusCode)
//             {
//                 // Yêu cầu đặt lại mật khẩu thành công
//                 return "Đã gửi email đặt lại mật khẩu. Vui lòng kiểm tra hộp thư đến của bạn.";
//             }
//             else
//             {
//                 // Gửi yêu cầu thất bại, trả về thông báo lỗi
//                 dynamic jsonResponse = JsonConvert.DeserializeObject(responseContent);
//                 if (jsonResponse?.error?.message != null)
//                 {
//                     return (string)jsonResponse.error.message;
//                 }
//                 else
//                 {
//                     return $"Gửi yêu cầu đặt lại mật khẩu thất bại: {response.StatusCode}";
//                 }
//             }
//         }
//         catch (HttpRequestException ex)
//         {
//             return $"Lỗi kết nối: {ex.Message}";
//         }
//     }
// }
