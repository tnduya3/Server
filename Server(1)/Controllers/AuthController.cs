using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.ComponentModel.DataAnnotations;
using Server_1_.Models;
using Server_1_.Services;
using Server_1_.Hubs;

namespace Server_1_.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IFirebaseAuthService _firebaseAuthService;
        private readonly IUserService _userService;
        private readonly ILogger<AuthController> _logger;
        private readonly IHubContext<ChatHub> _hubContext;

        public AuthController(
            IFirebaseAuthService firebaseAuthService,
            IUserService userService,
            ILogger<AuthController> logger,
            IHubContext<ChatHub> hubContext)
        {
            _firebaseAuthService = firebaseAuthService;
            _userService = userService;
            _logger = logger;
            _hubContext = hubContext;
        }

        /// <summary>
        /// User login with email and password
        /// </summary>
        /// <param name="request">Login credentials</param>
        /// <returns>Authentication result with tokens</returns>
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid input data",
                    });
                }

                _logger.LogInformation("Login attempt for email: {Email}", request.Email);

                // Authenticate with Firebase
                var authResult = await _firebaseAuthService.SignInWithEmailAndPasswordAsync(request.Email, request.Password);

                if (!authResult.Success)
                {
                    _logger.LogWarning("Login failed for email {Email}: {Message}", request.Email, authResult.Message);
                    return Unauthorized(new AuthResponse
                    {
                        Success = false,
                        Message = authResult.Message ?? "Login failed"
                    });
                }

                // Get or create user in local database
                var user = await GetOrCreateLocalUser(authResult);

                // Calculate token expiration (Firebase tokens typically expire in 1 hour)
                var expiresAt = DateTime.UtcNow.AddHours(1);

                var response = new AuthResponse
                {
                    Success = true,
                    Message = "Login successful",
                    AccessToken = authResult.IdToken,
                    RefreshToken = authResult.RefreshToken,
                    UserId = user?.UserId.ToString(),
                    Email = authResult.Email,
                    ExpiresAt = expiresAt,
                    User = user != null ? new UserInfo
                    {
                        UserId = user.UserId.ToString(),
                        Email = user.Email ?? string.Empty,
                        DisplayName = user.UserName,
                        EmailVerified = true, // Firebase handles email verification
                        CreatedAt = user.CreatedAt,
                        LastSignInAt = DateTime.UtcNow
                    } : null
                };

                // Notify via SignalR that user is online
                if (user != null)
                {
                    await _hubContext.Clients.All.SendAsync("UserOnline", user.UserId.ToString());
                }

                _logger.LogInformation("User {UserId} logged in successfully", response.UserId);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during login for email: {Email}", request.Email);
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Message = "An error occurred during login"
                });
            }
        }

        /// <summary>
        /// User registration with email and password
        /// </summary>
        /// <param name="request">Registration data</param>
        /// <returns>Authentication result with tokens</returns>
        [HttpPost("signup")]
        public async Task<ActionResult<AuthResponse>> SignUp([FromBody] SignUpRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid input data"
                    });
                }

                _logger.LogInformation("Sign up attempt for email: {Email}", request.Email);

                // Check if user already exists in local database
                var existingUser = await _userService.GetUserByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    return Conflict(new AuthResponse
                    {
                        Success = false,
                        Message = "User with this email already exists"
                    });
                }

                // Create user in Firebase
                var authResult = await _firebaseAuthService.SignUpWithEmailAndPasswordAsync(request.Email, request.Password);

                if (!authResult.Success)
                {
                    _logger.LogWarning("Sign up failed for email {Email}: {Message}", request.Email, authResult.Message);
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = authResult.Message ?? "Sign up failed"
                    });
                }

                // Create user in local database
                var newUser = new Users
                {
                    Email = request.Email,
                    UserName = request.DisplayName ?? request.Email.Split('@')[0],
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                var createdUser = await _userService.CreateUserAsync(newUser);
                if (createdUser == null)
                {
                    _logger.LogError("Failed to create user in local database for email: {Email}", request.Email);
                    return StatusCode(500, new AuthResponse
                    {
                        Success = false,
                        Message = "Failed to create user account"
                    });
                }

                // Calculate token expiration
                var expiresAt = DateTime.UtcNow.AddHours(1);

                var response = new AuthResponse
                {
                    Success = true,
                    Message = "Account created successfully",
                    AccessToken = authResult.IdToken,
                    RefreshToken = authResult.RefreshToken,
                    UserId = createdUser.UserId.ToString(),
                    Email = authResult.Email,
                    ExpiresAt = expiresAt,
                    User = new UserInfo
                    {
                        UserId = createdUser.UserId.ToString(),
                        Email = createdUser.Email ?? string.Empty,
                        DisplayName = createdUser.UserName,
                        EmailVerified = false, // May need email verification
                        CreatedAt = createdUser.CreatedAt,
                        LastSignInAt = DateTime.UtcNow
                    }
                };

                // Send welcome notification via SignalR
                await _hubContext.Clients.All.SendAsync("UserRegistered", new
                {
                    UserId = createdUser.UserId,
                    Username = createdUser.UserName,
                    Email = createdUser.Email,
                    JoinedAt = DateTime.UtcNow
                });

                _logger.LogInformation("User {UserId} registered successfully", response.UserId);

                return CreatedAtAction(nameof(Login), new { id = createdUser.UserId }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during sign up for email: {Email}", request.Email);
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Message = "An error occurred during registration"
                });
            }
        }

        /// <summary>
        /// Send password reset email
        /// </summary>
        /// <param name="request">Email for password reset</param>
        /// <returns>Result of password reset email sending</returns>
        [HttpPost("forgot-password")]
        public async Task<ActionResult<AuthResponse>> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid email address"
                    });
                }

                _logger.LogInformation("Password reset requested for email: {Email}", request.Email);

                // Check if user exists in local database
                var user = await _userService.GetUserByEmailAsync(request.Email);
                if (user == null)
                {
                    // Don't reveal whether user exists or not for security
                    return Ok(new AuthResponse
                    {
                        Success = true,
                        Message = "If an account with this email exists, a password reset email has been sent."
                    });
                }

                // Send password reset email via Firebase
                var authResult = await _firebaseAuthService.SendPasswordResetEmailAsync(request.Email);

                var response = new AuthResponse
                {
                    Success = authResult.Success,
                    Message = authResult.Success 
                        ? "Password reset email sent successfully. Please check your inbox."
                        : authResult.Message ?? "Failed to send password reset email"
                };

                if (authResult.Success)
                {
                    _logger.LogInformation("Password reset email sent for user: {Email}", request.Email);
                }
                else
                {
                    _logger.LogWarning("Failed to send password reset email for: {Email}, Error: {Error}", 
                        request.Email, authResult.Message);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during forgot password for email: {Email}", request.Email);
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Message = "An error occurred while processing password reset request"
                });
            }
        }

        /// <summary>
        /// Refresh access token using refresh token
        /// </summary>
        /// <param name="request">Refresh token</param>
        /// <returns>New access token</returns>
        [HttpPost("refresh-token")]
        public async Task<ActionResult<AuthResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid refresh token"
                    });
                }

                _logger.LogInformation("Token refresh requested");

                var authResult = await _firebaseAuthService.RefreshTokenAsync(request.RefreshToken);

                if (!authResult.Success)
                {
                    _logger.LogWarning("Token refresh failed: {Message}", authResult.Message);
                    return Unauthorized(new AuthResponse
                    {
                        Success = false,
                        Message = authResult.Message ?? "Token refresh failed"
                    });
                }

                var expiresAt = DateTime.UtcNow.AddHours(1);

                var response = new AuthResponse
                {
                    Success = true,
                    Message = "Token refreshed successfully",
                    AccessToken = authResult.IdToken,
                    RefreshToken = authResult.RefreshToken,
                    UserId = authResult.UserId,
                    ExpiresAt = expiresAt
                };

                _logger.LogInformation("Token refreshed successfully for user: {UserId}", authResult.UserId);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during token refresh");
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Message = "An error occurred during token refresh"
                });
            }
        }

        /// <summary>
        /// Verify ID token and get user info
        /// </summary>
        /// <param name="request">ID token to verify</param>
        /// <returns>User information if token is valid</returns>
        [HttpPost("verify-token")]
        public async Task<ActionResult<AuthResponse>> VerifyToken([FromBody] VerifyTokenRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid token"
                    });
                }

                var authResult = await _firebaseAuthService.VerifyIdTokenAsync(request.IdToken);

                if (!authResult.Success)
                {
                    return Unauthorized(new AuthResponse
                    {
                        Success = false,
                        Message = authResult.Message ?? "Invalid token"
                    });
                }

                // Get user from local database
                var user = await _userService.GetUserByEmailAsync(authResult.Email ?? "");

                var response = new AuthResponse
                {
                    Success = true,
                    Message = "Token is valid",
                    UserId = authResult.UserId,
                    Email = authResult.Email,
                    User = user != null ? new UserInfo
                    {
                        UserId = user.UserId.ToString(),
                        Email = user.Email ?? string.Empty,
                        DisplayName = user.UserName,
                        EmailVerified = true,
                        CreatedAt = user.CreatedAt,
                        LastSignInAt = DateTime.UtcNow
                    } : null
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during token verification");
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Message = "An error occurred during token verification"
                });
            }
        }

        /// <summary>
        /// User logout (mainly for logging purposes)
        /// </summary>
        /// <returns>Logout confirmation</returns>
        [HttpPost("logout")]
        public async Task<ActionResult<AuthResponse>> Logout()
        {
            try
            {
                // Note: Firebase doesn't have a server-side logout, tokens expire naturally
                // This endpoint is mainly for logging and client-side cleanup
                
                var userId = HttpContext.Request.Headers["UserId"].ToString();
                
                if (!string.IsNullOrEmpty(userId))
                {
                    // Notify via SignalR that user is offline
                    await _hubContext.Clients.All.SendAsync("UserOffline", userId);
                    _logger.LogInformation("User {UserId} logged out", userId);
                }

                return Ok(new AuthResponse
                {
                    Success = true,
                    Message = "Logged out successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during logout");
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Message = "An error occurred during logout"
                });
            }
        }

        // Helper method to get or create user in local database
        private async Task<Users?> GetOrCreateLocalUser(Services.AuthResult authResult)
        {
            try
            {
                if (string.IsNullOrEmpty(authResult.Email))
                    return null;

                // Try to get existing user
                var existingUser = await _userService.GetUserByEmailAsync(authResult.Email);
                if (existingUser != null)
                {
                    return existingUser;
                }

                // Create new user if doesn't exist
                var newUser = new Users
                {
                    Email = authResult.Email,
                    UserName = authResult.Email.Split('@')[0],
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                return await _userService.CreateUserAsync(newUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting or creating local user for email: {Email}", authResult.Email);
                return null;
            }
        }
    }
}
