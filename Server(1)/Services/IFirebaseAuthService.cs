using System.Threading.Tasks;

namespace Server_1_.Services
{
    public interface IFirebaseAuthService
    {
        Task<AuthResult> SignInWithEmailAndPasswordAsync(string email, string password);
        Task<AuthResult> SignUpWithEmailAndPasswordAsync(string email, string password);
        Task<AuthResult> SendPasswordResetEmailAsync(string email);
        Task<AuthResult> VerifyIdTokenAsync(string idToken);
        Task<AuthResult> RefreshTokenAsync(string refreshToken);
    }

    public class AuthResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? IdToken { get; set; }
        public string? RefreshToken { get; set; }
        public string? UserId { get; set; }
        public string? Email { get; set; }
        public object? Data { get; set; }
    }
}
