# 🔧 HƯỚNG DẪN XỬ LÝ SỰ CỐ (Troubleshooting Guide)

**Cập nhật:** 23/10/2025  
**Phiên bản:** 2.0  

---

## 🚨 DANH SÁCH NHANH CÁC VẤN ĐỀ PHỔ BIẾN

| Lỗi | Nguyên Nhân | Giải Pháp |
|-----|-----------|---------|
| 401 Unauthorized | Token hết hạn | Refresh token hoặc đăng nhập lại |
| 403 Forbidden | Không có quyền | Liên hệ admin cấp quyền |
| 500 Internal Error | Database error | Kiểm tra logs & database |
| Connection timeout | Database không chạy | Khởi động SQL Server |
| Migration failed | Schema conflict | Rollback & tạo migration mới |

---

## 📝 CHI TIẾT CÁC VẤN ĐỀ & GIẢI PHÁP

### 1️⃣ CÁC VẤN ĐỀ XÁC THỰC (Authentication Issues)

#### ❌ Lỗi: "401 Unauthorized"

**Dấu hiệu:**
```
GET /api/users
Response: 401 Unauthorized
Message: "Unauthorized"
```

**Nguyên Nhân:**
- [ ] Token JWT không được cung cấp
- [ ] Token đã hết hạn
- [ ] Token không hợp lệ
- [ ] Header `Authorization` sai format

**Giải Pháp:**

```bash
# 1. Kiểm tra header Authorization
# ❌ SAI:
Authorization: {token}
Authorization: JWT {token}

# ✅ ĐÚNG:
Authorization: Bearer {token}

# 2. Kiểm tra token hạn sử dụng
# Đăng nhập lại để lấy token mới
POST /api/auth/login
{
  "email": "user@example.com",
  "password": "password"
}

# 3. Hoặc refresh token nếu có refresh token
POST /api/auth/refresh-token
{
  "refreshToken": "refresh_token_value"
}

# 4. Kiểm tra token trong jwt.io
# Xem expiresIn (exp claim) có hợp lệ không
```

**Xác Minh:**
```bash
# Test với curl
curl -H "Authorization: Bearer YOUR_TOKEN" \
     http://localhost:5000/api/users
```

---

#### ❌ Lỗi: "Invalid token" hoặc "Token signature invalid"

**Dấu hiệu:**
```
Response: 401
Message: "Invalid token" hoặc "Token signature invalid"
```

**Nguyên Nhân:**
- [ ] JWT secret key không khớp
- [ ] Token bị sửa đổi
- [ ] Token từ environment khác

**Giải Pháp:**

```json
// 1. Kiểm tra appsettings.json
{
  "Jwt": {
    "SecretKey": "your-super-secret-key-min-32-chars-long!",
    "Issuer": "YourApp",
    "Audience": "YourAppUsers"
  }
}

// 2. Đảm bảo secret key:
// - Ít nhất 32 ký tự
// - Giống trong config
// - Không có khoảng trắng thừa

// 3. Tạo token mới sau khi sửa config
```

---

#### ❌ Lỗi: "Token expired"

**Dấu hiệu:**
```
Response: 401
Message: "Token expired"
```

**Nguyên Nhân:**
- [ ] Access token hết hạn (mặc định 15 phút)
- [ ] ExpirationMinutes quá ngắn

**Giải Pháp:**

```bash
# 1. Sử dụng refresh token
POST /api/auth/refresh-token
{
  "refreshToken": "your_refresh_token"
}

# 2. Nếu không có refresh token, đăng nhập lại
POST /api/auth/login
{
  "email": "user@example.com",
  "password": "password"
}

# 3. Điều chỉnh expiration time
{
  "Jwt": {
    "ExpirationMinutes": 30  // Tăng từ 15 thành 30
  }
}
```

---

### 2️⃣ CÁC VẤN ĐỀ CƠ SỞ DỮ LIỆU (Database Issues)

#### ❌ Lỗi: "Cannot open database connection"

**Dấu hiệu:**
```
Exception: SqlException: Cannot open database connection
Failed to connect to Server=...;Database=UsersDb
```

**Nguyên Nhân:**
- [ ] SQL Server không chạy
- [ ] Connection string sai
- [ ] Database chưa tạo
- [ ] Firewall chặn cổng 1433

**Giải Pháp:**

```powershell
# 1. Kiểm tra SQL Server chạy
Get-Service | Where-Object {$_.Name -like "*MSSQL*"} | Format-Table

# 2. Khởi động SQL Server nếu dừng
Start-Service MSSQLSERVER

# 3. Kiểm tra connection string
# appsettings.json
{
  "ConnectionStrings": {
    "UsersDb": "Server=.\\SQLEXPRESS;Database=UsersDb;Integrated Security=true;"
  }
}

# Format khác:
# - "Server=localhost" (default instance)
# - "Server=.\\SQLEXPRESS" (named instance)
# - "Server=127.0.0.1,1433" (TCP/IP)

# 4. Test connection
sqlcmd -S . -d UsersDb -Q "SELECT 1"

# 5. Kiểm tra firewall
netstat -an | findstr ":1433"
```

---

#### ❌ Lỗi: "No migrations pending"

**Dấu hiệu:**
```
Exception: No migrations pending
Database is up to date
```

**Nguyên Nhân:**
- [ ] Migrations đã được apply
- [ ] Database schema không khớp
- [ ] Migration lịch sử bị xóa

**Giải Pháp:**

```powershell
# 1. Kiểm tra migration history
dotnet ef migrations list `
  -p src/Modules/Users/Users.Infrastructure `
  -s src/Gateway/WebApi

# 2. Nếu migrations rỗng, tạo mới
dotnet ef migrations add InitialCreate `
  -p src/Modules/Users/Users.Infrastructure `
  -s src/Gateway/WebApi

# 3. Apply migrations
dotnet ef database update -s src/Gateway/WebApi

# 4. Nếu cần rollback
dotnet ef database update PreviousMigrationName `
  -s src/Gateway/WebApi
```

---

#### ❌ Lỗi: "The IDENTITY_INSERT for table is set to OFF"

**Dấu hiệu:**
```
SqlException: Cannot insert explicit value for identity column
when IDENTITY_INSERT is set to OFF
```

**Nguyên Nhân:**
- [ ] Cố gắng insert ID thủ công
- [ ] Migration conflict

**Giải Pháp:**

```sql
-- 1. Kiểm tra current state
SET IDENTITY_INSERT [Users] OFF;

-- 2. Xóa primary key constraint nếu cần
ALTER TABLE [Users] DROP CONSTRAINT [PK_Users];

-- 3. Hoặc reset seed
DBCC CHECKIDENT (Users, RESEED, 0);

-- 4. Khôi phục database
RESTORE DATABASE [UsersDb] FROM DISK = 'backup_path'
```

---

### 3️⃣ CÁC VẤN ĐỀ MIGRATION

#### ❌ Lỗi: "Migration 'AddXXX' has already been applied to the database"

**Dấu hiệu:**
```
Exception: Migration 'AddXXX' has already been applied to the database
```

**Nguyên Nhân:**
- [ ] Migration đã được apply trước đó
- [ ] __EFMigrationsHistory bị lỗi

**Giải Pháp:**

```powershell
# 1. Kiểm tra migration history
dotnet ef migrations list `
  -p src/Modules/Users/Users.Infrastructure

# 2. Xóa migration cuối cùng nếu chưa deploy
dotnet ef migrations remove `
  -p src/Modules/Users/Users.Infrastructure `
  -s src/Gateway/WebApi

# 3. Hoặc delete + recreate
dotnet ef database update 0 -s src/Gateway/WebApi  # Rollback all
dotnet ef database update -s src/Gateway/WebApi    # Apply all
```

---

#### ❌ Lỗi: "The model backing the context has changed"

**Dấu hiệu:**
```
Exception: The model backing the 'UsersDbContext' context has changed
since the database was last created
```

**Nguyên Nhân:**
- [ ] Entity model thay đổi
- [ ] DbContext thay đổi nhưng không có migration
- [ ] Database schema cũ

**Giải Pháp:**

```powershell
# 1. Tạo migration mới
dotnet ef migrations add UpdateModelChanges `
  -p src/Modules/Users/Users.Infrastructure `
  -s src/Gateway/WebApi

# 2. Xem migration được tạo
# (Xem migrations folder)

# 3. Apply migration
dotnet ef database update -s src/Gateway/WebApi

# 4. Nếu development, có thể reset
dotnet ef database drop --force -s src/Gateway/WebApi
dotnet ef database update -s src/Gateway/WebApi
```

---

### 4️⃣ CÁC VẤN ĐỀ NGƯỜI DÙNG (User Issues)

#### ❌ Lỗi: "User not found"

**Dấu hiệu:**
```
Response: 404
Message: "User not found"
```

**Nguyên Nhân:**
- [ ] UserId không tồn tại
- [ ] User đã bị xóa mềm (IsDeleted = true)
- [ ] UserId sai format (không GUID)

**Giải Pháp:**

```bash
# 1. Kiểm tra user tồn tại
GET /api/users/{userId}

# 2. Kiểm tra trong database
SELECT * FROM Users WHERE UserId = '550e8400-e29b-41d4-a716-446655440000';

# 3. Kiểm tra IsDeleted flag
SELECT UserId, Email, IsDeleted, IsActive 
FROM Users 
WHERE IsDeleted = 0;

# 4. Restore nếu bị xóa mềm
-- SQL: UPDATE Users SET IsDeleted = 0, DeletedAtUtc = NULL WHERE UserId = ...
```

---

#### ❌ Lỗi: "Email already exists"

**Dấu hiệu:**
```
Response: 409
Message: "Email already exists"
```

**Nguyên Nhân:**
- [ ] Email đã được đăng ký
- [ ] User cũ chưa bị xóa
- [ ] Duplicate trong database

**Giải Pháp:**

```bash
# 1. Kiểm tra email
GET /api/users?searchTerm=email@example.com

# 2. Nếu user cũ, xóa mềm trước
DELETE /api/users/{oldUserId}

# 3. Hoặc nếu test, xóa database
dotnet ef database drop --force -s src/Gateway/WebApi
dotnet ef database update -s src/Gateway/WebApi
```

---

#### ❌ Lỗi: "Email not verified"

**Dấu hiệu:**
```
Response: 400
Message: "Email has not been verified"
```

**Nguyên Nhân:**
- [ ] Email chưa được xác minh
- [ ] Xác minh email hết hạn
- [ ] Token không hợp lệ

**Giải Pháp:**

```bash
# 1. Yêu cầu token xác minh email
POST /api/auth/request-email-verification
Authorization: Bearer {accessToken}

# Response sẽ có verification token hoặc gửi email

# 2. Xác minh email
POST /api/auth/verify-email
{
  "token": "verification_token_from_email"
}

# 3. Kiểm tra trong database
SELECT UserId, Email, IsEmailVerified 
FROM Users 
WHERE IsEmailVerified = 1;
```

---

### 5️⃣ CÁC VẤN ĐỀ FILE (File Upload Issues)

#### ❌ Lỗi: "File too large" hoặc "Maximum file size exceeded"

**Dấu hiệu:**
```
Response: 413
Message: "File too large (max 5MB)"
```

**Nguyên Nhân:**
- [ ] File upload lớn hơn 5MB
- [ ] Request size limit quá nhỏ

**Giải Pháp:**

```csharp
// 1. Kiểm tra file size trước khi upload
// Client-side: 
const MAX_FILE_SIZE = 5 * 1024 * 1024; // 5MB
if (file.size > MAX_FILE_SIZE) {
  alert("File quá lớn!");
}

// 2. Tăng Kestrel limit (nếu cần)
// Program.cs
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10MB
});
```

---

#### ❌ Lỗi: "Invalid file type" hoặc "Unsupported media type"

**Dấu hiệu:**
```
Response: 400
Message: "Invalid file type. Allowed: JPEG, PNG, GIF, WebP"
```

**Nguyên Nhân:**
- [ ] File format không hỗ trợ
- [ ] Content-Type sai
- [ ] File extension sai

**Hỗ Trợ:**
- ✅ JPEG (.jpg, .jpeg)
- ✅ PNG (.png)
- ✅ GIF (.gif)
- ✅ WebP (.webp)

**Giải Pháp:**

```bash
# 1. Chuyển đổi image format
# Sử dụng tool như ImageMagick

# Windows:
# powershell
# $image = [System.Drawing.Image]::FromFile("input.bmp")
# $image.Save("output.jpg")

# 2. Upload file hợp lệ
# Kiểm tra Content-Type
# ✅ image/jpeg
# ✅ image/png
# ✅ image/gif
# ✅ image/webp

# 3. Kiểm tra phần mở rộng file
# OK: file.jpg, file.png
# NOT OK: file.jpg.exe
```

---

#### ❌ Lỗi: "Unable to save profile picture"

**Dấu hiệu:**
```
Response: 500
Message: "Unable to save profile picture"
```

**Nguyên Nhân:**
- [ ] Lỗi permission thư mục
- [ ] Đĩa đầy
- [ ] Path không hợp lệ

**Giải Pháp:**

```powershell
# 1. Kiểm tra thư mục tồn tại
Test-Path "d:\projects\core-project\uploads"

# 2. Tạo thư mục nếu không tồn tại
New-Item -ItemType Directory -Path "uploads" -Force

# 3. Kiểm tra permission
icacls "d:\projects\core-project\uploads"

# 4. Grant permission nếu cần
icacls "d:\projects\core-project\uploads" /grant "Users:(OI)(CI)F"

# 5. Kiểm tra dung lượng đĩa
Get-Volume | Where-Object {$_.DriveLetter -eq 'D'}
```

---

### 6️⃣ CÁC VẤN ĐỀ HIỆU NĂNG (Performance Issues)

#### ⚠️ Vấn đề: "API response time slow"

**Dấu hiệu:**
```
Response time: > 1000ms
Server processing: Slow
Database: Slow queries
```

**Nguyên Nhân:**
- [ ] Missing indexes
- [ ] N+1 queries
- [ ] Large result sets
- [ ] Slow database queries

**Giải Pháp:**

```sql
-- 1. Kiểm tra slow queries
SELECT TOP 10
  qt.text,
  qs.total_elapsed_time/1000000 as TotalElapsedTime_Sec,
  qs.execution_count
FROM sys.dm_exec_query_stats AS qs
CROSS APPLY sys.dm_exec_sql_text(qs.sql_handle) AS qt
ORDER BY qs.total_elapsed_time DESC;

-- 2. Thêm indexes
CREATE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_LoginAttempts_UserId ON LoginAttempts(UserId);

-- 3. Sử dụng phân trang
-- Không lấy hết dữ liệu
GET /api/users?pageNumber=1&pageSize=20

-- 4. Projection
-- Chỉ lấy columns cần thiết
SELECT UserId, Email, FirstName FROM Users
-- Thay vì
SELECT * FROM Users
```

---

#### ⚠️ Vấn đề: "High CPU usage"

**Dấu hiệu:**
```
CPU: > 80%
Process: WebApi.exe using 1GB+ RAM
```

**Nguyên Nhân:**
- [ ] Memory leak
- [ ] Infinite loop
- [ ] Large collection processing
- [ ] Compression overhead

**Giải Pháp:**

```csharp
// 1. Kiểm tra memory leak
// Sử dụng Diagnostic tools

// 2. Tối ưu collections
// ❌ Không tốt:
var allUsers = dbContext.Users.ToList();
foreach(var user in allUsers)
{
    ProcessUser(user);
}

// ✅ Tốt:
var users = dbContext.Users.Where(u => u.IsActive).ToList();
foreach(var user in users)
{
    ProcessUser(user);
}

// 3. Sử dụng streaming
// Thay vì load all data
foreach(var user in dbContext.Users.AsEnumerable())
{
    ProcessUser(user);
}

// 4. Giảm compression level (tạm thời)
```

---

### 7️⃣ CÁC VẤN ĐỀ LOGGING & DEBUGGING

#### ❌ Lỗi: "No logs being written"

**Dấu hiệu:**
```
No log files created
Logs folder empty
```

**Nguyên Nhân:**
- [ ] Logging không được configure
- [ ] Log level quá cao
- [ ] Path sai

**Giải Pháp:**

```json
// 1. Kiểm tra appsettings.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Users.Application": "Debug"
    },
    "File": {
      "Path": "logs/app-{Date}.txt",
      "RollingInterval": "Day"
    }
  }
}

// 2. Serilog configuration
// Program.cs
var logger = new LoggerConfiguration()
    .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day)
    .MinimumLevel.Debug()
    .CreateLogger();

Log.Logger = logger;
```

---

### 8️⃣ CÁC VẤN ĐỀ KIỂM TOÁN (Audit Issues)

#### ❌ Vấn đề: "Audit logs not recorded"

**Dấu hiệu:**
```
AuditLogs table empty
ChangeHistories table empty
No activity tracking
```

**Nguyên Nhân:**
- [ ] Middleware không được configure
- [ ] Interceptor không hoạt động
- [ ] Event handler không registered

**Giải Pháp:**

```csharp
// 1. Đảm bảo middleware được thêm
// Program.cs
app.UseAuditMiddleware();
app.UseAccessLoggingMiddleware();

// 2. Đảm bảo handlers được registered
// Dependency Injection
services.AddScoped<IAuditService, AuditService>();

// 3. Kiểm tra SaveChanges override
// DbContext
public override async Task<int> SaveChangesAsync()
{
    await AuditEntities();
    return await base.SaveChangesAsync();
}
```

---

## 🔍 CÔNG CỤ DEBUG

### 1. Postman Collection

```json
{
  "info": {
    "name": "Users API",
    "version": "2.0"
  },
  "item": [
    {
      "name": "Auth",
      "item": [
        {
          "name": "Register",
          "request": {
            "method": "POST",
            "url": "http://localhost:5000/api/auth/register",
            "body": {
              "email": "test@example.com",
              "password": "Test123!",
              "firstName": "John",
              "lastName": "Doe"
            }
          }
        }
      ]
    }
  ]
}
```

### 2. SQL Server Profiler

```sql
-- Theo dõi queries
-- SQL Server Management Studio
-- Tools → SQL Server Profiler
```

### 3. Entity Framework Core Logging

```csharp
// Program.cs
optionsBuilder
    .LogTo(Console.WriteLine)
    .EnableSensitiveDataLogging()
    .EnableDetailedErrors();
```

---

## 📞 LIÊN HỆ HỖ TRỢ

**Nếu vẫn gặp sự cố:**

1. 📋 Kiểm tra log files: `logs/`
2. 🔍 Xem SQL queries: SQL Server Profiler
3. 💾 Kiểm tra database: SQL Server Management Studio
4. 🌐 Test API: Postman hoặc Swagger UI
5. 📧 Liên hệ team development

---

**Tài liệu này sẽ được cập nhật khi phát hiện vấn đề mới.** 🔄