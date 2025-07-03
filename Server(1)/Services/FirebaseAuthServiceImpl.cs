using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;

namespace Server_1_.Services
{
    public class FirebaseAuthService : IFirebaseAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FirebaseAuthService> _logger;
        private readonly IUserService _userService;
        private readonly string _apiKey;

        public FirebaseAuthService(HttpClient httpClient, IConfiguration configuration, ILogger<FirebaseAuthService> logger, IUserService userService)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _userService = userService;
            _apiKey = Environment.GetEnvironmentVariable("FIREBASE_API_KEY") ?? _configuration["Firebase:ApiKey"] ?? "";

            InitializeFirebaseApp();
        }

        private void InitializeFirebaseApp()
        {
            try
            {
                if (FirebaseApp.DefaultInstance == null)
                {
                    string serviceAccountKeyPath = Path.Combine(Directory.GetCurrentDirectory(), "realtimechatapplicationg10-firebase-adminsdk-fbsvc-1c9e190b3c.json");
                    
                    if (File.Exists(serviceAccountKeyPath))
                    {
                        FirebaseApp.Create(new AppOptions()
                        {
                            Credential = GoogleCredential.FromFile(serviceAccountKeyPath),
                            ProjectId = Environment.GetEnvironmentVariable("FIREBASE_PROJECT_ID") ?? _configuration["Firebase:ProjectId"]
                        });
                        _logger.LogInformation("Firebase Admin SDK initialized successfully");
                    }
                    else
                    {
                        _logger.LogWarning("Firebase service account file not found at: {Path}", serviceAccountKeyPath);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing Firebase Admin SDK");
            }
        }

        public async Task<AuthResult> SignInWithEmailAndPasswordAsync(string email, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(_apiKey))
                {
                    _logger.LogError("Firebase API Key is not configured");
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Firebase configuration error"
                    };
                }

                string requestUri = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={_apiKey}";
                var request = new
                {
                    email = email,
                    password = password,
                    returnSecureToken = true
                };
                
                var jsonRequest = JsonConvert.SerializeObject(request);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(requestUri, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    dynamic jsonResponse = JsonConvert.DeserializeObject(responseContent)!;
                    
                    _logger.LogInformation("User signed in successfully: {Email}", email);
                    
                    // Lấy thông tin avatar từ database
                    string? avatar = null;
                    try
                    {
                        var user = await _userService.GetUserByEmailAsync(email);
                        avatar = user?.Avatar;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to get user avatar for email: {Email}", email);
                    }
                    
                    return new AuthResult
                    {
                        Success = true,
                        Message = "Sign in successful",
                        IdToken = jsonResponse.idToken,
                        RefreshToken = jsonResponse.refreshToken,
                        UserId = jsonResponse.localId,
                        Email = jsonResponse.email,
                        Avatar = avatar,
                        Data = jsonResponse
                    };
                }
                else
                {
                    dynamic errorResponse = JsonConvert.DeserializeObject(responseContent)!;
                    string errorMessage = errorResponse?.error?.message ?? "Sign in failed";
                    
                    _logger.LogWarning("Sign in failed for {Email}: {Error}", email, errorMessage);
                    
                    return new AuthResult
                    {
                        Success = false,
                        Message = GetUserFriendlyErrorMessage(errorMessage)
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during sign in for {Email}", email);
                return new AuthResult
                {
                    Success = false,
                    Message = "An error occurred during sign in"
                };
            }
        }

        public async Task<AuthResult> SignUpWithEmailAndPasswordAsync(string email, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(_apiKey))
                {
                    _logger.LogError("Firebase API Key is not configured");
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Firebase configuration error"
                    };
                }

                string requestUri = $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={_apiKey}";
                var request = new
                {
                    email = email,
                    password = password,
                    returnSecureToken = true
                };
                
                var jsonRequest = JsonConvert.SerializeObject(request);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(requestUri, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    dynamic jsonResponse = JsonConvert.DeserializeObject(responseContent)!;
                    
                    _logger.LogInformation("User signed up successfully: {Email}", email);
                    
                    // Người dùng mới sẽ không có avatar
                    
                    return new AuthResult
                    {
                        Success = true,
                        Message = "Sign up successful",
                        IdToken = jsonResponse.idToken,
                        RefreshToken = jsonResponse.refreshToken,
                        UserId = jsonResponse.localId,
                        Email = jsonResponse.email,
                        Avatar = null, // Người dùng mới chưa có avatar
                        Data = jsonResponse
                    };
                }
                else
                {
                    dynamic errorResponse = JsonConvert.DeserializeObject(responseContent)!;
                    string errorMessage = errorResponse?.error?.message ?? "Sign up failed";
                    
                    _logger.LogWarning("Sign up failed for {Email}: {Error}", email, errorMessage);
                    
                    return new AuthResult
                    {
                        Success = false,
                        Message = GetUserFriendlyErrorMessage(errorMessage)
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during sign up for {Email}", email);
                return new AuthResult
                {
                    Success = false,
                    Message = "An error occurred during sign up"
                };
            }
        }

        public async Task<AuthResult> SendPasswordResetEmailAsync(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(_apiKey))
                {
                    _logger.LogError("Firebase API Key is not configured");
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Firebase configuration error"
                    };
                }

                string requestUri = $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={_apiKey}";
                var request = new
                {
                    requestType = "PASSWORD_RESET",
                    email = email
                };

                var jsonRequest = JsonConvert.SerializeObject(request);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(requestUri, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Password reset email sent to: {Email}", email);

                    return new AuthResult
                    {
                        Success = true,
                        Message = "Password reset email sent successfully. Please check your inbox.",
                        Email = email
                    };
                }
                else
                {
                    dynamic errorResponse = JsonConvert.DeserializeObject(responseContent)!;
                    string errorMessage = errorResponse?.error?.message ?? "Failed to send password reset email";

                    _logger.LogWarning("Failed to send password reset email to {Email}: {Error}", email, errorMessage);

                    return new AuthResult
                    {
                        Success = false,
                        Message = GetUserFriendlyErrorMessage(errorMessage)
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during password reset for {Email}", email);
                return new AuthResult
                {
                    Success = false,
                    Message = "An error occurred while sending password reset email"
                };
            }

            //string requestUri = $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={_apiKey}";
            //var request = new
            //{
            //    requestType = "PASSWORD_RESET",
            //    email = email
            //};
            //var jsonRequest = JsonConvert.SerializeObject(request);
            //var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            //try
            //{
            //    var response = await _httpClient.PostAsync(requestUri, content);
            //    var responseContent = await response.Content.ReadAsStringAsync();
            //    if (response.IsSuccessStatusCode)
            //    {
            //        // Yêu cầu đặt lại mật khẩu thành công
            //        return new AuthResult
            //        {
            //            Success = true,
            //            Message = "Password reset email sent successfully. Please check your inbox.",
            //            Email = email
            //        };
            //    }
            //    else
            //    {
            //        // Gửi yêu cầu thất bại, trả về thông báo lỗi
            //        dynamic jsonResponse = JsonConvert.DeserializeObject(responseContent);
            //        if (jsonResponse?.error?.message != null)
            //        {
            //            return new AuthResult
            //            {
            //                Success = false,
            //                Message = "Failed to send password reset email: " + GetUserFriendlyErrorMessage(jsonResponse.error.message)
            //            };
            //        }
            //        else
            //        {
            //            return new AuthResult
            //            {
            //                Success = false,
            //                Message = "Failed to send password reset email: " + response.StatusCode
            //            };
            //        }
            //    }
            //}
            //catch (HttpRequestException ex)
            //{
            //    _logger.LogError(ex, "Exception during password reset for {Email}", email);
            //    return new AuthResult
            //    {
            //        Success = false,
            //        Message = "An error occurred while sending password reset email"
            //    };
            //}
        }

        public async Task<AuthResult> VerifyIdTokenAsync(string idToken)
        {
            try
            {
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
                
                _logger.LogInformation("Token verified successfully for user: {UserId}", decodedToken.Uid);
                
                // Lấy thông tin avatar từ database
                string? avatar = null;
                string? email = decodedToken.Claims.TryGetValue("email", out var emailClaim) ? emailClaim.ToString() : null;
                try
                {
                    if (!string.IsNullOrEmpty(email))
                    {
                        var user = await _userService.GetUserByEmailAsync(email);
                        avatar = user?.Avatar;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get user avatar for email: {Email}", email);
                }
                
                return new AuthResult
                {
                    Success = true,
                    Message = "Token verified successfully",
                    UserId = decodedToken.Uid,
                    Email = email,
                    Avatar = avatar,
                    Data = decodedToken.Claims
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token verification failed");
                return new AuthResult
                {
                    Success = false,
                    Message = "Invalid token"
                };
            }
        }

        public async Task<AuthResult> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                if (string.IsNullOrEmpty(_apiKey))
                {
                    _logger.LogError("Firebase API Key is not configured");
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Firebase configuration error"
                    };
                }

                string requestUri = $"https://securetoken.googleapis.com/v1/token?key={_apiKey}";
                var request = new
                {
                    grant_type = "refresh_token",
                    refresh_token = refreshToken
                };
                
                var jsonRequest = JsonConvert.SerializeObject(request);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(requestUri, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    dynamic jsonResponse = JsonConvert.DeserializeObject(responseContent)!;
                    
                    _logger.LogInformation("Token refreshed successfully");
                    
                    // Lấy thông tin avatar từ database nếu có userId
                    string? avatar = null;
                    try
                    {
                        if (!string.IsNullOrEmpty(jsonResponse.user_id?.ToString()))
                        {
                            // Thử tìm user theo Firebase UID (có thể cần mapping table trong tương lai)
                            // Hiện tại sẽ để null vì không có email trong refresh response
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to get user avatar during token refresh");
                    }
                    
                    return new AuthResult
                    {
                        Success = true,
                        Message = "Token refreshed successfully",
                        IdToken = jsonResponse.id_token,
                        RefreshToken = jsonResponse.refresh_token,
                        UserId = jsonResponse.user_id,
                        Avatar = avatar,
                        Data = jsonResponse
                    };
                }
                else
                {
                    dynamic errorResponse = JsonConvert.DeserializeObject(responseContent)!;
                    string errorMessage = errorResponse?.error?.message ?? "Token refresh failed";
                    
                    _logger.LogWarning("Token refresh failed: {Error}", errorMessage);
                    
                    return new AuthResult
                    {
                        Success = false,
                        Message = GetUserFriendlyErrorMessage(errorMessage)
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during token refresh");
                return new AuthResult
                {
                    Success = false,
                    Message = "An error occurred during token refresh"
                };
            }
        }

        private static string GetUserFriendlyErrorMessage(string firebaseError)
        {
            return firebaseError?.ToLower() switch
            {
                "email_exists" => "Email address is already in use",
                "email_not_found" => "Email address not found",
                "invalid_password" => "Invalid password",
                "user_disabled" => "User account has been disabled",
                "too_many_attempts_try_later" => "Too many unsuccessful attempts. Please try again later",
                "weak_password" => "Password is too weak. Please choose a stronger password",
                "invalid_email" => "Invalid email address format",
                _ => firebaseError ?? "An unknown error occurred"
            };
        }
    }
}
