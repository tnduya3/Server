# 🔐 Authentication System Documentation

## Tổng quan
Hệ thống authentication sử dụng Firebase Authentication kết hợp với database local để quản lý người dùng và phiên đăng nhập.

## 🏗️ Kiến trúc

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Client App    │    │   Web Client    │    │  Mobile Client  │
│                 │    │                 │    │                 │
└─────┬───────────┘    └─────┬───────────┘    └─────┬───────────┘
      │                      │                      │
      └──────────────────────┼──────────────────────┘
                             │
    ┌────────────────────────┴────────────────────────┐
    │              AuthController                     │
    │   ┌─────────────────┐  ┌─────────────────────┐  │
    │   │ Firebase Auth   │  │   Local Database    │  │
    │   │   Service       │  │     UserService     │  │
    │   └─────────────────┘  └─────────────────────┘  │
    └─────────────────┬───────────────────────────────┘
                      │
    ┌─────────────────┴─────────────────┐
    │         Firebase Auth             │
    │    (Email/Password Provider)      │
    └─────────────────┬─────────────────┘
                      │
    ┌─────────────────┴─────────────────┐
    │        PostgreSQL Database        │
    │         (Local Users)             │
    └───────────────────────────────────┘
```

## 🔧 Components

### 1. AuthController
**Path**: `Controllers/AuthController.cs`

Provides REST API endpoints for authentication:
- `POST /api/auth/login` - User login
- `POST /api/auth/signup` - User registration  
- `POST /api/auth/forgot-password` - Password reset
- `POST /api/auth/refresh-token` - Token refresh
- `POST /api/auth/verify-token` - Token verification
- `POST /api/auth/logout` - User logout

### 2. FirebaseAuthService
**Path**: `Services/FirebaseAuthServiceImpl.cs`

Handles Firebase Authentication operations:
- Email/password authentication
- Token management
- Password reset emails
- Token verification and refresh

### 3. Data Models
**Path**: `Models/AuthDtos.cs`

DTOs for authentication requests and responses:
- `LoginRequest`
- `SignUpRequest`
- `ForgotPasswordRequest`
- `RefreshTokenRequest`
- `AuthResponse`
- `UserInfo`

## 🚀 API Endpoints

### Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "password123"
}
```

**Response**:
```json
{
  "success": true,
  "message": "Login successful",
  "accessToken": "eyJhbGciOiJSUzI1NiIs...",
  "refreshToken": "AMf-vBwg1QZ0a...",
  "userId": "123",
  "email": "user@example.com",
  "expiresAt": "2025-06-12T15:30:00Z",
  "user": {
    "userId": "123",
    "email": "user@example.com",
    "displayName": "User Name",
    "emailVerified": true,
    "createdAt": "2025-06-12T14:30:00Z",
    "lastSignInAt": "2025-06-12T14:30:00Z"
  }
}
```

### Sign Up
```http
POST /api/auth/signup
Content-Type: application/json

{
  "email": "newuser@example.com",
  "password": "password123",
  "confirmPassword": "password123",
  "displayName": "New User"
}
```

### Forgot Password
```http
POST /api/auth/forgot-password
Content-Type: application/json

{
  "email": "user@example.com"
}
```

### Refresh Token
```http
POST /api/auth/refresh-token
Content-Type: application/json

{
  "refreshToken": "AMf-vBwg1QZ0a..."
}
```

### Verify Token
```http
POST /api/auth/verify-token
Content-Type: application/json

{
  "idToken": "eyJhbGciOiJSUzI1NiIs..."
}
```

## 🔒 Security Features

### Password Requirements
- Minimum 6 characters
- Firebase handles password strength validation
- Secure password reset via email

### Token Management
- JWT tokens from Firebase (1 hour expiration)
- Refresh tokens for seamless re-authentication
- Server-side token verification

### Error Handling
- User-friendly error messages
- No information disclosure about user existence
- Comprehensive logging for security monitoring

## 🔧 Configuration

### Firebase Setup
1. **Firebase Console Configuration**:
   - Create Firebase project
   - Enable Authentication > Email/Password provider
   - Generate Web API key
   - Create Service Account and download JSON

2. **Environment Variables**:
   ```bash
   FIREBASE_API_KEY=your_web_api_key_here
   FIREBASE_PROJECT_ID=your_project_id_here
   ```

3. **appsettings.json**:
   ```json
   {
     "Firebase": {
       "ProjectId": "your_project_id",
       "ApiKey": "your_web_api_key"
     }
   }
   ```

4. **Service Account File**:
   - Place `realtimechatapplicationg10-firebase-adminsdk-fbsvc-1c9e190b3c.json` in project root
   - Update filename in `FirebaseAuthServiceImpl.cs` if different

### Database Setup
Users table must exist with:
- `UserId` (Primary Key)
- `Email` (Unique)
- `UserName`
- `CreatedAt`
- `IsActive`

## 🔄 Authentication Flow

### Registration Flow
```
1. Client: POST /api/auth/signup
2. Server: Validate input data
3. Server: Check if user exists locally
4. Server: Create user in Firebase
5. Server: Create user in local database
6. Server: Return tokens and user info
7. SignalR: Broadcast UserRegistered event
```

### Login Flow
```
1. Client: POST /api/auth/login
2. Server: Validate credentials with Firebase
3. Server: Get/Create user in local database
4. Server: Return tokens and user info
5. SignalR: Broadcast UserOnline event
```

### Token Refresh Flow
```
1. Client: POST /api/auth/refresh-token
2. Server: Validate refresh token with Firebase
3. Server: Return new access token
4. Client: Update stored tokens
```

## 🧪 Testing

### Unit Testing Scenarios
- Valid login/signup requests
- Invalid email formats
- Password mismatch validation
- Existing user registration attempts
- Token expiration handling
- Firebase service failures

### Integration Testing
Use `test-auth-api.http` file for comprehensive API testing:

```bash
# Test successful registration
POST /api/auth/signup → 201 Created

# Test login with created user  
POST /api/auth/login → 200 OK

# Test password reset
POST /api/auth/forgot-password → 200 OK

# Test token operations
POST /api/auth/refresh-token → 200 OK
POST /api/auth/verify-token → 200 OK
```

## 📊 Monitoring & Logging

### Log Levels
- **Information**: Successful operations
- **Warning**: Failed authentication attempts
- **Error**: System errors and exceptions

### Key Metrics to Monitor
- Authentication success/failure rates
- Token refresh frequency
- Password reset requests
- User registration patterns

### Log Examples
```
[INF] User 123 logged in successfully
[WRN] Sign up failed for email user@example.com: EMAIL_EXISTS
[ERR] Exception during login for email: user@example.com
```

## 🚨 Error Codes

| HTTP Status | Scenario | Message |
|-------------|----------|---------|
| 200 | Success | "Login successful" |
| 201 | User created | "Account created successfully" |
| 400 | Invalid input | "Invalid input data" |
| 401 | Auth failed | "Invalid credentials" |
| 409 | User exists | "User already exists" |
| 500 | Server error | "Internal server error" |

## 🔮 Future Enhancements

### Planned Features
- [ ] OAuth providers (Google, Facebook)
- [ ] Two-factor authentication (2FA)
- [ ] Session management
- [ ] Role-based access control (RBAC)
- [ ] Account lockout policies
- [ ] Password strength requirements

### Security Improvements
- [ ] Rate limiting for auth endpoints
- [ ] IP-based blocking
- [ ] Device tracking
- [ ] Suspicious activity detection

## 🤝 Integration with Chat System

### SignalR Integration
- Login triggers `UserOnline` event
- Logout triggers `UserOffline` event
- Registration triggers `UserRegistered` event

### Chat Features Integration
- User authentication required for chat access
- Token verification for secure messaging
- User presence management

## 📚 Dependencies

```xml
<!-- Required NuGet packages -->
<PackageReference Include="FirebaseAdmin" Version="2.4.0" />
<PackageReference Include="Google.Apis.Auth" Version="1.60.0" />
<PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
<PackageReference Include="Microsoft.AspNetCore.SignalR" Version="1.1.0" />
```

---

**Status**: ✅ **READY FOR TESTING**

Authentication system hoàn chỉnh với Firebase integration và SignalR support!
