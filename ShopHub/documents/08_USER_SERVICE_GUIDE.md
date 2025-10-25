# 👤 UserService Implementation Guide

## 📋 Overview

Complete implementation of **UserService** with:
- ✅ **Register** - Create new user accounts
- ✅ **Login** - Authenticate and issue JWT tokens
- ✅ **Logout** - Invalidate refresh tokens
- ✅ **Get Profile** - Retrieve user information

---

## 🎯 Features

### Authentication
- **JWT Bearer Tokens** - Secure API authentication
- **Refresh Tokens** - Extended session management
- **Password Hashing** - BCrypt encryption
- **Token Expiration** - 1-hour access token, 7-day refresh token

### User Management
- **Email Validation** - Unique email constraints
- **Strong Passwords** - Minimum 8 chars, uppercase, lowercase, digits
- **Profile Management** - Store and update user information
- **Account Status** - Active/Inactive tracking

---

## 📁 Project Structure

```
src/Services/UserService/
├── Domain/
│   ├── Entities/
│   │   └── User.cs (User aggregate root)
│   └── Repositories/
│       └── IUserRepository.cs
├── Application/
│   ├── Commands/
│   │   ├── RegisterCommand.cs
│   │   └── LogoutCommand.cs
│   ├── Queries/
│   │   ├── LoginQuery.cs
│   │   └── GetUserProfileQuery.cs
│   ├── Handlers/
│   │   ├── RegisterCommandHandler.cs
│   │   ├── LoginQueryHandler.cs
│   │   ├── LogoutCommandHandler.cs
│   │   └── GetUserProfileQueryHandler.cs
│   └── DTOs/
│       ├── RegisterDto.cs
│       ├── LoginDto.cs
│       ├── LoginResponseDto.cs
│       └── UserProfileDto.cs
├── Infrastructure/
│   ├── Persistence/
│   │   ├── UserDbContext.cs
│   │   └── Repositories/
│   │       └── UserRepository.cs
│   └── DependencyInjection.cs
└── Presentation/
    └── Controllers/
        └── UsersController.cs
```

---

## 🔌 API Endpoints

### 1. Register User
```http
POST /api/users/register
Content-Type: application/json

{
  "email": "user@example.com",
  "fullName": "John Doe",
  "password": "SecurePass123",
  "confirmPassword": "SecurePass123",
  "phoneNumber": "+1234567890"
}
```

**Response (201 Created)**:
```json
{
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "email": "user@example.com",
  "fullName": "John Doe",
  "phoneNumber": "+1234567890",
  "avatarUrl": null,
  "address": null,
  "city": null,
  "country": null,
  "isActive": true,
  "isEmailVerified": false,
  "lastLoginAt": null,
  "createdAt": "2025-10-25T10:30:00Z",
  "updatedAt": null
}
```

**Error (409 Conflict)**:
```json
{
  "message": "Email user@example.com is already registered"
}
```

---

### 2. Login User
```http
POST /api/users/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePass123"
}
```

**Response (200 OK)**:
```json
{
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "email": "user@example.com",
  "fullName": "John Doe",
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "k3j4k5j6k7j8k9j0k1j2k3j4k5j6k7j8",
  "expiresIn": 3600,
  "tokenType": "Bearer"
}
```

**Error (401 Unauthorized)**:
```json
{
  "message": "Invalid email or password"
}
```

---

### 3. Get Current Profile
```http
GET /api/users/me
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Response (200 OK)**:
```json
{
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "email": "user@example.com",
  "fullName": "John Doe",
  "phoneNumber": "+1234567890",
  "avatarUrl": null,
  "address": "123 Main St",
  "city": "New York",
  "country": "USA",
  "isActive": true,
  "isEmailVerified": false,
  "lastLoginAt": "2025-10-25T12:00:00Z",
  "createdAt": "2025-10-25T10:30:00Z",
  "updatedAt": "2025-10-25T12:00:00Z"
}
```

---

### 4. Get User Profile by ID
```http
GET /api/users/{userId}
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Response (200 OK)**:
```json
{
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "email": "user@example.com",
  "fullName": "John Doe",
  ...
}
```

---

### 5. Logout User
```http
POST /api/users/logout
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Response (200 OK)**:
```json
{
  "message": "Logged out successfully"
}
```

---

## 🔐 Password Requirements

Passwords must meet these criteria:
- ✅ Minimum 8 characters
- ✅ At least 1 uppercase letter (A-Z)
- ✅ At least 1 lowercase letter (a-z)
- ✅ At least 1 digit (0-9)

**Valid**: `SecurePass123`, `MyPassword456`  
**Invalid**: `pass123` (no uppercase), `PASSWORD123` (no lowercase)

---

## 🔑 JWT Token Structure

**Access Token** (Expires in 1 hour):
```
Header: {
  "alg": "HS256",
  "typ": "JWT"
}

Payload: {
  "iss": "ShopHub",
  "aud": "ShopHubUsers",
  "sub": "550e8400-e29b-41d4-a716-446655440000",
  "email": "user@example.com",
  "UserId": "550e8400-e29b-41d4-a716-446655440000",
  "jti": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
  "exp": 1729869000,
  "iat": 1729865400
}

Signature: HMAC-SHA256(header + payload + secret)
```

---

## 🔄 Authentication Flow

```
1. User clicks "Register"
   ↓
2. Submit email, name, password
   ↓
3. Server validates & hashes password
   ↓
4. Create User in database
   ↓
5. Return UserProfile (201 Created)

---

1. User clicks "Login"
   ↓
2. Submit email & password
   ↓
3. Server finds user by email
   ↓
4. Verify password (BCrypt)
   ↓
5. Generate JWT tokens
   ↓
6. Store refresh token in database
   ↓
7. Return LoginResponse with tokens

---

1. Client sends request with "Authorization: Bearer <token>"
   ↓
2. Middleware validates JWT
   ↓
3. Extract claims (UserId, Email)
   ↓
4. Continue if valid, return 401 if expired
```

---

## 🛠️ Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ShopHubDb;..."
  },
  "JwtSettings": {
    "SecretKey": "your-super-secret-key-32-chars-minimum!",
    "Issuer": "ShopHub",
    "Audience": "ShopHubUsers",
    "ExpirationMinutes": 60
  }
}
```

### Important
⚠️ **Change JWT SecretKey in production!**
- Minimum 32 characters
- Use strong random string
- Store in Azure Key Vault or AWS Secrets Manager
- Never commit to version control

---

## 📚 Database Schema

### Users Table
```sql
CREATE TABLE Users (
  Id UNIQUEIDENTIFIER PRIMARY KEY,
  Email NVARCHAR(255) NOT NULL UNIQUE,
  FullName NVARCHAR(100) NOT NULL,
  PasswordHash NVARCHAR(255) NOT NULL,
  PhoneNumber NVARCHAR(20),
  AvatarUrl NVARCHAR(500),
  Address NVARCHAR(255),
  City NVARCHAR(100),
  Country NVARCHAR(100),
  IsActive BIT NOT NULL DEFAULT 1,
  IsEmailVerified BIT NOT NULL DEFAULT 0,
  LastLoginAt DATETIME2,
  RefreshToken NVARCHAR(500),
  RefreshTokenExpiresAt DATETIME2,
  CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
  UpdatedAt DATETIME2
)

CREATE INDEX IX_Users_Email ON Users(Email) UNIQUE
```

---

## 🚀 Getting Started

### 1. Build Project
```bash
cd d:\PROJECT\GitHub\ecom-sphere-project\ShopHub
dotnet build
```

### 2. Apply Migrations
```bash
dotnet ef database update --project ShopHub
```

### 3. Run Application
```bash
dotnet run
```

### 4. Test Endpoints

#### Register
```bash
curl -X POST http://localhost:5000/api/users/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "fullName": "John Doe",
    "password": "SecurePass123",
    "confirmPassword": "SecurePass123"
  }'
```

#### Login
```bash
curl -X POST http://localhost:5000/api/users/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "SecurePass123"
  }'
```

#### Get Profile (use token from login response)
```bash
curl -X GET http://localhost:5000/api/users/me \
  -H "Authorization: Bearer <access_token>"
```

---

## 🐛 Common Issues & Solutions

### ❌ "Invalid email or password"
- **Cause**: Wrong email or password
- **Solution**: Verify credentials, ensure user is registered

### ❌ "Email already registered"
- **Cause**: Email exists in database
- **Solution**: Use different email or login instead

### ❌ "Password must be at least 8 characters"
- **Cause**: Password too weak
- **Solution**: Use stronger password with uppercase, lowercase, digit

### ❌ "401 Unauthorized" on protected endpoints
- **Cause**: Missing or invalid token
- **Solution**: Include valid Authorization header with Bearer token

### ❌ "Token is expired"
- **Cause**: Access token expired
- **Solution**: Use refresh token to get new access token

---

## 🔐 Security Best Practices

✅ **DO**
- Hash passwords with BCrypt
- Validate email format
- Expire tokens
- Use HTTPS in production
- Store secrets in Key Vault
- Log authentication attempts
- Rate limit login attempts

❌ **DON'T**
- Store plain text passwords
- Use weak secret key
- Expose tokens in logs
- Send tokens in URLs
- Trust client-side validation
- Store tokens in LocalStorage (use HttpOnly cookies)

---

## 📝 Password Hashing

UserService uses **BCrypt.Net-Core** for secure password hashing:

```csharp
// Hash password on registration
var passwordHash = BCrypt.Net.BCrypt.HashPassword(plainTextPassword);

// Verify password on login
var isValid = BCrypt.Net.BCrypt.Verify(plainTextPassword, storedHash);
```

- ✅ Automatic salt generation
- ✅ Adaptive cost factor (iterations)
- ✅ Resistant to rainbow table attacks
- ✅ Slow by design (protects against brute force)

---

## 🔗 Integration with Other Services

### Using User Context in Other Services
```csharp
// In ProductService controller
[Authorize]
public async Task<IActionResult> CreateProduct([FromBody] CreateProductCommand command)
{
    // Get current user ID
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    
    // Associate product with user
    command.CreatedBy = Guid.Parse(userId);
    
    var result = await _mediator.Send(command);
    return Created(...);
}
```

---

## 📊 Entity Relationship

```
User (Aggregate Root)
├── Id (Guid)
├── Email (string, unique)
├── FullName (string)
├── PasswordHash (string)
├── PhoneNumber (string?)
├── AvatarUrl (string?)
├── Address (string?)
├── City (string?)
├── Country (string?)
├── IsActive (bool)
├── IsEmailVerified (bool)
├── LastLoginAt (DateTime?)
├── RefreshToken (string?)
├── RefreshTokenExpiresAt (DateTime?)
├── CreatedAt (DateTime)
├── UpdatedAt (DateTime?)
└── DomainEvents (List<IDomainEvent>)
```

---

## 📞 Support

For issues or questions:
1. Check documentation in `documents/` folder
2. Review API error messages
3. Check database for user record
4. Verify JWT configuration
5. Check application logs

---

**Last Updated**: Oct 25, 2025  
**Status**: ✅ Production Ready  
**Version**: 1.0.0