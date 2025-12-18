# 🚀 HƯỚNG DẪN BẮTẦU NHANH (Quick Start Guide)

## 📌 Yêu Cầu Hệ Thống

- **.NET:** 8.0 hoặc cao hơn
- **SQL Server:** 2019 hoặc cao hơn
- **Visual Studio:** 2022 Community hoặc cao hơn

---

## ⚙️ CẤU HÌNH CƠNG BAN ĐẦU

### 1️⃣ Clone & Open Project

```powershell
cd d:\projects\core-project
dotnet restore
```

### 2️⃣ Cấu Hình Database Connection

**File:** `appsettings.json` (Gateway.WebApi)

```json
{
  "ConnectionStrings": {
    "UsersDb": "Server=YOUR_SERVER;Database=UsersDb;Integrated Security=true;"
  },
  "Jwt": {
    "SecretKey": "your-super-secret-key-min-32-chars-long!",
    "Issuer": "YourApp",
    "Audience": "YourAppUsers",
    "ExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  }
}
```

### 3️⃣ Tạo & Apply Database Migrations

```powershell
# Tạo migration
dotnet ef migrations add InitialCreate `
  -p src/Modules/Users/Users.Infrastructure `
  -s src/Gateway/WebApi

# Apply migration
dotnet ef database update -s src/Gateway/WebApi
```

### 4️⃣ Chạy ứng dụng

```powershell
# Development
dotnet run -c Debug --project src/Gateway/WebApi

# Production
dotnet run -c Release --project src/Gateway/WebApi
```

---

## 🔑 API Authentication

### 1. Đăng Ký Người Dùng

**POST** `/api/auth/register`

```json
{
  "email": "user@example.com",
  "password": "SecurePass123!",
  "firstName": "John",
  "lastName": "Doe"
}
```

**Response (201 Created):**
```json
{
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "email": "user@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "isEmailVerified": false
}
```

### 2. Đăng Nhập

**POST** `/api/auth/login`

```json
{
  "email": "user@example.com",
  "password": "SecurePass123!"
}
```

**Response (200 OK):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "refresh_token_value",
  "expiresIn": 900
}
```

### 3. Sử Dụng Access Token

**GET** `/api/users`

```bash
Authorization: Bearer {accessToken}
```

### 4. Refresh Token

**POST** `/api/auth/refresh-token`

```json
{
  "refreshToken": "refresh_token_value"
}
```

---

## 📧 Xác Minh Email

### 1. Yêu Cầu Xác Minh Email

**POST** `/api/auth/request-email-verification`

```bash
Authorization: Bearer {accessToken}
```

**Response:**
```json
{
  "message": "Verification email sent",
  "verificationToken": "token_value_or_empty_based_on_config"
}
```

### 2. Xác Minh Email

**POST** `/api/auth/verify-email`

```json
{
  "token": "verification_token_from_email"
}
```

---

## 🔐 Đặt Lại Mật Khẩu

### 1. Yêu Cầu Đặt Lại

**POST** `/api/auth/request-password-reset`

```json
{
  "email": "user@example.com"
}
```

### 2. Đặt Lại Mật Khẩu

**POST** `/api/auth/reset-password`

```json
{
  "token": "reset_token_from_email",
  "newPassword": "NewSecurePass123!"
}
```

---

## 👤 Quản Lý Người Dùng

### 1. Lấy Danh Sách Người Dùng

**GET** `/api/users?pageNumber=1&pageSize=10&searchTerm=john&isActive=true`

```bash
Authorization: Bearer {accessToken}
```

### 2. Lấy Chi Tiết Người Dùng

**GET** `/api/users/{userId}`

```bash
Authorization: Bearer {accessToken}
```

### 3. Cập Nhật Thông Tin

**PUT** `/api/users/{userId}`

```json
{
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "+1234567890"
}
```

### 4. Đổi Mật Khẩu

**POST** `/api/users/{userId}/change-password`

```json
{
  "currentPassword": "OldPass123!",
  "newPassword": "NewPass123!"
}
```

---

## 📸 Hình Đại Diện Người Dùng

### Upload Hình Đại Diện

**POST** `/api/users/{userId}/profile-picture`

```bash
Content-Type: multipart/form-data
Authorization: Bearer {accessToken}

File: (JPEG, PNG, GIF, WebP, tối đa 5MB)
```

### Xóa Hình Đại Diện

**DELETE** `/api/users/{userId}/profile-picture`

```bash
Authorization: Bearer {accessToken}
```

---

## 📊 Lịch Sử & Kiểm Toán

### 1. Lịch Sử Đăng Nhập

**GET** `/api/users/{userId}/login-history?pageNumber=1&pageSize=20`

```bash
Authorization: Bearer {accessToken}
```

### 2. Lịch Sử Hoạt Động

**GET** `/api/users/{userId}/activity-history?pageNumber=1&pageSize=20&activityType=LOGIN`

```bash
Authorization: Bearer {accessToken}
```

### 3. Nhật Ký Kiểm Toán

**GET** `/api/users/{userId}/audit-logs?pageNumber=1&pageSize=10&entityName=User`

```bash
Authorization: Bearer {accessToken}
```

---

## 🔄 Quản Lý Trạng Thái Người Dùng

### Vô Hiệu Hoá Người Dùng

**POST** `/api/users/{userId}/deactivate`

```bash
Authorization: Bearer {accessToken}
```

### Kích Hoạt Người Dùng

**POST** `/api/users/{userId}/activate`

```bash
Authorization: Bearer {accessToken}
```

### Xóa Mềm Người Dùng

**DELETE** `/api/users/{userId}`

```bash
Authorization: Bearer {accessToken}
```

---

## 🧪 Kiểm Tra & Debug

### 1. Enable Logging

**appsettings.Development.json:**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Information"
    }
  }
}
```

### 2. Swagger Documentation

- **URL:** `https://localhost:5001/swagger`
- Xem tất cả endpoints
- Test API trực tiếp

### 3. Database Inspection

```powershell
# Mở Package Manager Console
# Chạy lệnh SQL
Select * from Users;
Select * from LoginAttempts;
Select * from AuditLogs;
```

---

## 🚨 Xử Lý Lỗi Phổ Biến

### 1. "ConnectionString not found"
✅ Kiểm tra `appsettings.json` có thuộc tính `ConnectionStrings.UsersDb`

### 2. "No migrations pending"
✅ Chạy `dotnet ef database update`

### 3. "Invalid token"
✅ Đảm bảo token vẫn còn hạn
✅ Sử dụng refresh token để lấy token mới

### 4. "Unauthorized (401)"
✅ Thêm header `Authorization: Bearer {token}`
✅ Kiểm tra token có hiệu lực

### 5. "Forbidden (403)"
✅ Người dùng không có quyền
✅ Liên hệ admin để cấp quyền

---

## 📚 Tài Liệu Bổ Sung

- 📖 [Thống Kê Phát Triển](THONG_KE_PHAT_TRIEN_HE_THONG.md)
- 📖 [API Specification](../PHASE2_FEATURES_EXTENDED.md)
- 📖 [Database Schema](../COMPLETE_USERS_MODULE_ROADMAP.md)

---

## 💡 Mẹo & Thủ Thuật

1. **Local Testing:** Sử dụng Postman hoặc cURL
2. **Database Testing:** Sử dụng SQL Server Management Studio
3. **Performance:** Kiểm tra query execution plans
4. **Security:** Luôn xác nhận email trước

---

**Cần hỗ trợ? Kiểm tra tài liệu hoặc liên hệ team!** 🎉