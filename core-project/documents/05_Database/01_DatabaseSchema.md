# 🗄️ SCHEMA CƠ SỞ DỮ LIỆU (Database Schema)

**Ngày cập nhật:** 23/10/2025  
**Phiên bản:** 2.0  

---

## 📋 Danh Sách Toàn Bộ Bảng

| Thứ Tự | Tên Bảng | Mô Tả | Loại | Phase |
|--------|---------|-------|------|-------|
| 1 | Users | Thông tin người dùng | Core | Core |
| 2 | Roles | Vai trò hệ thống | Core | Core |
| 3 | UserRoles | Gán vai trò cho người dùng | Core | Core |
| 4 | Permissions | Quyền hệ thống | Core | Core |
| 5 | EmailVerificationTokens | Tokens xác minh email | Auth | Phase 1 |
| 6 | PasswordResetTokens | Tokens đặt lại mật khẩu | Auth | Phase 1 |
| 7 | RefreshTokens | Tokens làm mới | Auth | Phase 1 |
| 8 | LoginAttempts | Nỗ lực đăng nhập | Tracking | Phase 1 |
| 9 | UserActivityHistories | Lịch sử hoạt động người dùng | Audit | Phase 2 |
| 10 | AuditLogs | Nhật ký kiểm toán | Audit | Phase 2 |
| 11 | ChangeHistories | Lịch sử thay đổi trường | Audit | Phase 2 |
| 12 | AccessLogs | Nhật ký truy cập API | Audit | Phase 2 |

---

## 🔹 BẢNG CỐT LÕI (Core Tables)

### 1. Users (Người Dùng)

**Mô tả:** Lưu trữ thông tin cơ bản của người dùng  
**Primary Key:** UserId (GUID)  
**Relationships:** 1-n UserRoles, 1-n LoginAttempts, 1-n Activities

| Cột | Kiểu Dữ Liệu | Nullable | Mô Tả | Constraints |
|-----|---------|----------|-------|------------|
| UserId | UNIQUEIDENTIFIER | NO | Khóa chính | PRIMARY KEY |
| Email | NVARCHAR(255) | NO | Email người dùng | UNIQUE, INDEX |
| PasswordHash | NVARCHAR(MAX) | NO | Bcrypt hash | - |
| FirstName | NVARCHAR(100) | NO | Tên | - |
| LastName | NVARCHAR(100) | NO | Họ | - |
| PhoneNumber | NVARCHAR(20) | YES | Số điện thoại | - |
| IsEmailVerified | BIT | NO | Đã xác minh email? | DEFAULT(0) |
| IsActive | BIT | NO | Tài khoản hoạt động? | DEFAULT(1), INDEX |
| IsDeleted | BIT | NO | Đã xóa mềm? | DEFAULT(0), INDEX |
| DeletedAtUtc | DATETIME2 | YES | Thời gian xóa | - |
| ProfilePicturePath | NVARCHAR(500) | YES | Đường dẫn ảnh đại diện | - |
| CreatedAtUtc | DATETIME2 | NO | Thời gian tạo | DEFAULT(GETUTCDATE()) |
| UpdatedAtUtc | DATETIME2 | YES | Thời gian cập nhật | - |

**Indexes:**
```sql
CREATE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_Users_IsActive_IsDeleted ON Users(IsActive, IsDeleted);
```

**Sample Query:**
```sql
-- Lấy người dùng hoạt động, chưa xóa, email đã xác minh
SELECT * FROM Users 
WHERE IsActive = 1 AND IsDeleted = 0 AND IsEmailVerified = 1;
```

---

### 2. Roles (Vai Trò)

**Mô tả:** Định nghĩa các vai trò trong hệ thống  
**Primary Key:** RoleId (GUID)  
**Relationships:** 1-n UserRoles

| Cột | Kiểu Dữ Liệu | Nullable | Mô Tả |
|-----|---------|----------|-------|
| RoleId | UNIQUEIDENTIFIER | NO | Khóa chính |
| Name | NVARCHAR(100) | NO | Tên vai trò (Admin, User, Moderator) |
| Description | NVARCHAR(500) | YES | Mô tả vai trò |
| CreatedAtUtc | DATETIME2 | NO | Thời gian tạo |

**Sample Data:**
```sql
INSERT INTO Roles VALUES 
  (NEWID(), 'Admin', 'Quản trị viên', GETUTCDATE()),
  (NEWID(), 'User', 'Người dùng thông thường', GETUTCDATE()),
  (NEWID(), 'Moderator', 'Quản trị viên nội dung', GETUTCDATE());
```

---

### 3. UserRoles (Gán Vai Trò)

**Mô tả:** Liên kết người dùng với vai trò  
**Primary Key:** (UserId, RoleId)  
**Relationships:** FK→Users, FK→Roles

| Cột | Kiểu Dữ Liệu | Nullable | Mô Tả |
|-----|---------|----------|-------|
| UserId | UNIQUEIDENTIFIER | NO | Khóa ngoài→Users |
| RoleId | UNIQUEIDENTIFIER | NO | Khóa ngoài→Roles |
| AssignedAtUtc | DATETIME2 | NO | Thời gian gán |

**Unique Constraint:**
```sql
ALTER TABLE UserRoles ADD CONSTRAINT UQ_UserRole UNIQUE(UserId, RoleId);
```

---

### 4. Permissions (Quyền)

**Mô tả:** Định nghĩa các quyền trong hệ thống  
**Primary Key:** PermissionId (GUID)

| Cột | Kiểu Dữ Liệu | Nullable | Mô Tả |
|-----|---------|----------|-------|
| PermissionId | UNIQUEIDENTIFIER | NO | Khóa chính |
| Name | NVARCHAR(100) | NO | Tên quyền |
| Description | NVARCHAR(500) | YES | Mô tả quyền |
| Category | NVARCHAR(50) | NO | Danh mục (Users, Roles, Reports) |
| CreatedAtUtc | DATETIME2 | NO | Thời gian tạo |

---

## 🔹 BẢNG AUTHENTICATION (Auth Tables)

### 5. EmailVerificationTokens (Xác Minh Email)

**Mô tả:** Lưu trữ tokens xác minh email  
**Primary Key:** TokenId (GUID)  
**Relationships:** FK→Users

| Cột | Kiểu Dữ Liệu | Nullable | Mô Tả |
|-----|---------|----------|-------|
| TokenId | UNIQUEIDENTIFIER | NO | Khóa chính |
| UserId | UNIQUEIDENTIFIER | NO | Khóa ngoài→Users |
| Token | NVARCHAR(256) | NO | Token value (64 chars) |
| ExpiresAtUtc | DATETIME2 | NO | Thời gian hết hạn |
| IsUsed | BIT | NO | Đã sử dụng? |
| CreatedAtUtc | DATETIME2 | NO | Thời gian tạo |

**Indexes:**
```sql
CREATE INDEX IX_EmailVerificationTokens_UserId ON EmailVerificationTokens(UserId);
CREATE INDEX IX_EmailVerificationTokens_Token ON EmailVerificationTokens(Token);
CREATE INDEX IX_EmailVerificationTokens_ExpiresAtUtc ON EmailVerificationTokens(ExpiresAtUtc);
```

**Data Retention:**
```sql
-- Xóa tokens hết hạn hàng ngày
DELETE FROM EmailVerificationTokens 
WHERE ExpiresAtUtc < GETUTCDATE() AND IsUsed = 1;
```

---

### 6. PasswordResetTokens (Đặt Lại Mật Khẩu)

**Mô tả:** Lưu trữ tokens đặt lại mật khẩu  
**Primary Key:** TokenId (GUID)  
**TTL:** 60 phút

| Cột | Kiểu Dữ Liệu | Nullable | Mô Tả |
|-----|---------|----------|-------|
| TokenId | UNIQUEIDENTIFIER | NO | Khóa chính |
| UserId | UNIQUEIDENTIFIER | NO | Khóa ngoài→Users |
| Token | NVARCHAR(256) | NO | Token value |
| ExpiresAtUtc | DATETIME2 | NO | Thời gian hết hạn |
| IsUsed | BIT | NO | Đã sử dụng? |
| CreatedAtUtc | DATETIME2 | NO | Thời gian tạo |

**Constraint:** Một token tại một thời điểm trên mỗi người dùng

---

### 7. RefreshTokens (Làm Mới Token)

**Mô tả:** Lưu trữ refresh tokens  
**Primary Key:** TokenId (GUID)  
**TTL:** 7 ngày

| Cột | Kiểu Dữ Liệu | Nullable | Mô Tả |
|-----|---------|----------|-------|
| TokenId | UNIQUEIDENTIFIER | NO | Khóa chính |
| UserId | UNIQUEIDENTIFIER | NO | Khóa ngoài→Users |
| Token | NVARCHAR(256) | NO | Token value |
| ExpiresAtUtc | DATETIME2 | NO | Thời gian hết hạn |
| RevokedAtUtc | DATETIME2 | YES | Thời gian hủy |
| ReplacedByTokenId | UNIQUEIDENTIFIER | YES | Token thay thế |
| CreatedAtUtc | DATETIME2 | NO | Thời gian tạo |

**Token Rotation Logic:**
```
1. Người dùng cấp refresh token mới
2. Token cũ được set RevokedAtUtc = GETUTCDATE()
3. Token cũ được set ReplacedByTokenId = NewTokenId
4. Chỉ token mới được cấp access token
```

---

### 8. LoginAttempts (Nỗ Lực Đăng Nhập)

**Mô tả:** Theo dõi toàn bộ nỗ lực đăng nhập
**Primary Key:** AttemptId (BIGINT identity)
**Relationships:** FK→Users

| Cột | Kiểu Dữ Liệu | Nullable | Mô Tả |
|-----|---------|----------|-------|
| AttemptId | BIGINT | NO | Khóa chính (auto-increment) |
| UserId | UNIQUEIDENTIFIER | YES | Khóa ngoài→Users |
| Email | NVARCHAR(255) | NO | Email đăng nhập |
| IsSuccessful | BIT | NO | Thành công? |
| IpAddress | NVARCHAR(45) | NO | Địa chỉ IP |
| UserAgent | NVARCHAR(MAX) | YES | User Agent browser |
| AttemptedAtUtc | DATETIME2 | NO | Thời gian cố gắng |

**Indexes:**
```sql
CREATE INDEX IX_LoginAttempts_UserId ON LoginAttempts(UserId);
CREATE INDEX IX_LoginAttempts_Email ON LoginAttempts(Email);
CREATE INDEX IX_LoginAttempts_AttemptedAtUtc ON LoginAttempts(AttemptedAtUtc);
CREATE INDEX IX_LoginAttempts_IsSuccessful ON LoginAttempts(IsSuccessful);
```

**Query Ví Dụ - Phát Hiện Tấn Công Brute Force:**
```sql
-- Kiểm tra >5 lần đăng nhập thất bại trong 15 phút gần nhất
SELECT Email, COUNT(*) as FailedAttempts
FROM LoginAttempts
WHERE IsSuccessful = 0 
  AND AttemptedAtUtc >= DATEADD(MINUTE, -15, GETUTCDATE())
GROUP BY Email
HAVING COUNT(*) > 5;
```

---

## 🔹 BẢNG AUDIT & TRACKING (Audit Tables)

### 9. UserActivityHistories (Lịch Sử Hoạt Động)

**Mô tả:** Ghi nhận toàn bộ hoạt động của người dùng  
**Primary Key:** ActivityId (BIGINT identity)  
**Relationships:** FK→Users

| Cột | Kiểu Dữ Liệu | Nullable | Mô Tả |
|-----|---------|----------|-------|
| ActivityId | BIGINT | NO | Khóa chính |
| UserId | UNIQUEIDENTIFIER | NO | Khóa ngoài→Users |
| ActivityType | NVARCHAR(50) | NO | LOGIN, LOGOUT, CREATE_ROLE, UPDATE_PROFILE, DELETE_ACCOUNT |
| Metadata | NVARCHAR(MAX) | YES | JSON metadata (thêm thông tin bổ sung) |
| IpAddress | NVARCHAR(45) | NO | Địa chỉ IP |
| UserAgent | NVARCHAR(MAX) | YES | User Agent |
| OccurredAtUtc | DATETIME2 | NO | Thời gian xảy ra |

**Indexes:**
```sql
CREATE INDEX IX_UserActivityHistories_UserId ON UserActivityHistories(UserId);
CREATE INDEX IX_UserActivityHistories_ActivityType ON UserActivityHistories(ActivityType);
CREATE INDEX IX_UserActivityHistories_OccurredAtUtc ON UserActivityHistories(OccurredAtUtc);
CREATE INDEX IX_UserActivityHistories_UserId_OccurredAtUtc ON UserActivityHistories(UserId, OccurredAtUtc);
```

**Metadata JSON Example:**
```json
{
  "deviceName": "Chrome on Windows 10",
  "location": "Ho Chi Minh City, Vietnam",
  "sessionId": "abc123xyz"
}
```

---

### 10. AuditLogs (Nhật Ký Kiểm Toán)

**Mô tả:** Ghi nhận mọi thay đổi trên entities  
**Primary Key:** AuditLogId (BIGINT identity)  
**Purpose:** Tuân thủ GDPR, SOC 2, HIPAA

| Cột | Kiểu Dữ Liệu | Nullable | Mô Tả |
|-----|---------|----------|-------|
| AuditLogId | BIGINT | NO | Khóa chính |
| UserId | UNIQUEIDENTIFIER | YES | Người thực hiện (NULL nếu system) |
| EntityName | NVARCHAR(100) | NO | Tên entity (User, Role, Permission) |
| EntityId | UNIQUEIDENTIFIER | NO | ID của entity |
| OperationType | NVARCHAR(20) | NO | CREATE, UPDATE, DELETE, RESTORE |
| BeforeValue | NVARCHAR(MAX) | YES | JSON trước thay đổi |
| AfterValue | NVARCHAR(MAX) | YES | JSON sau thay đổi |
| IpAddress | NVARCHAR(45) | YES | Địa chỉ IP |
| OccurredAtUtc | DATETIME2 | NO | Thời gian xảy ra |

**Indexes:**
```sql
CREATE INDEX IX_AuditLogs_UserId ON AuditLogs(UserId);
CREATE INDEX IX_AuditLogs_EntityName_EntityId ON AuditLogs(EntityName, EntityId);
CREATE INDEX IX_AuditLogs_OperationType ON AuditLogs(OperationType);
CREATE INDEX IX_AuditLogs_OccurredAtUtc ON AuditLogs(OccurredAtUtc);
```

**Query Ví Dụ - Kiểm Toán:**
```sql
-- Lịch sử đầy đủ thay đổi của một user
SELECT 
  AuditLogId, UserId, OperationType, BeforeValue, AfterValue, OccurredAtUtc
FROM AuditLogs
WHERE EntityName = 'User' AND EntityId = @UserId
ORDER BY OccurredAtUtc DESC;

-- Người thay đổi gần đây
SELECT 
  DISTINCT UserId, COUNT(*) as ChangeCount
FROM AuditLogs
WHERE OccurredAtUtc >= DATEADD(DAY, -7, GETUTCDATE())
GROUP BY UserId
ORDER BY ChangeCount DESC;
```

---

### 11. ChangeHistories (Lịch Sử Thay Đổi Trường)

**Mô tả:** Ghi nhận thay đổi từng trường cụ thể  
**Primary Key:** ChangeId (BIGINT identity)  
**Scope:** PASSWORD, EMAIL, FIRST_NAME, LAST_NAME, PHONE, etc.

| Cột | Kiểu Dữ Liệu | Nullable | Mô Tả |
|-----|---------|----------|-------|
| ChangeId | BIGINT | NO | Khóa chính |
| UserId | UNIQUEIDENTIFIER | NO | Khóa ngoài→Users |
| FieldName | NVARCHAR(100) | NO | Tên trường (PASSWORD, EMAIL) |
| OldValue | NVARCHAR(MAX) | YES | Giá trị cũ |
| NewValue | NVARCHAR(MAX) | YES | Giá trị mới |
| ChangedByUserId | UNIQUEIDENTIFIER | YES | Người thực hiện (NULL=chính user) |
| ChangeReason | NVARCHAR(500) | YES | Lý do thay đổi |
| IsReversible | BIT | NO | Có thể đảo ngược? |
| ChangedAtUtc | DATETIME2 | NO | Thời gian thay đổi |

**Indexes:**
```sql
CREATE INDEX IX_ChangeHistories_UserId ON ChangeHistories(UserId);
CREATE INDEX IX_ChangeHistories_FieldName ON ChangeHistories(FieldName);
CREATE INDEX IX_ChangeHistories_UserId_FieldName ON ChangeHistories(UserId, FieldName);
CREATE INDEX IX_ChangeHistories_ChangedAtUtc ON ChangeHistories(ChangedAtUtc);
```

**Query Ví Dụ - Lịch Sử Email:**
```sql
-- Lịch sử thay đổi email của người dùng
SELECT OldValue, NewValue, ChangedAtUtc, ChangeReason
FROM ChangeHistories
WHERE UserId = @UserId AND FieldName = 'EMAIL'
ORDER BY ChangedAtUtc DESC;
```

---

### 12. AccessLogs (Nhật Ký Truy Cập)

**Mô tả:** Ghi nhận truy cập API/tài nguyên  
**Primary Key:** LogId (BIGINT identity)  
**Purpose:** Security analysis, performance monitoring

| Cột | Kiểu Dữ Liệu | Nullable | Mô Tả |
|-----|---------|----------|-------|
| LogId | BIGINT | NO | Khóa chính |
| UserId | UNIQUEIDENTIFIER | YES | Khóa ngoài→Users |
| ResourceName | NVARCHAR(200) | NO | API endpoint hoặc resource |
| HttpMethod | NVARCHAR(10) | NO | GET, POST, PUT, DELETE, PATCH |
| HttpStatusCode | INT | NO | 200, 400, 401, 403, 404, 500 |
| ResponseTimeMs | INT | NO | Thời gian phản hồi (milliseconds) |
| WasSuccessful | BIT | NO | Thành công? (2xx status code) |
| IpAddress | NVARCHAR(45) | NO | Địa chỉ IP |
| UserAgent | NVARCHAR(MAX) | YES | User Agent |
| AccessedAtUtc | DATETIME2 | NO | Thời gian truy cập |

**Indexes:**
```sql
CREATE INDEX IX_AccessLogs_UserId ON AccessLogs(UserId);
CREATE INDEX IX_AccessLogs_ResourceName ON AccessLogs(ResourceName);
CREATE INDEX IX_AccessLogs_WasSuccessful ON AccessLogs(WasSuccessful);
CREATE INDEX IX_AccessLogs_UserId_WasSuccessful ON AccessLogs(UserId, WasSuccessful);
CREATE INDEX IX_AccessLogs_AccessedAtUtc ON AccessLogs(AccessedAtUtc);
CREATE INDEX IX_AccessLogs_HttpStatusCode ON AccessLogs(HttpStatusCode);
```

**Query Ví Dụ - Phân Tích Hiệu Năng:**
```sql
-- Top 10 endpoints chậm nhất
SELECT TOP 10
  ResourceName,
  COUNT(*) as RequestCount,
  AVG(ResponseTimeMs) as AvgResponseTime,
  MAX(ResponseTimeMs) as MaxResponseTime
FROM AccessLogs
WHERE AccessedAtUtc >= DATEADD(DAY, -7, GETUTCDATE())
GROUP BY ResourceName
ORDER BY AvgResponseTime DESC;

-- Lỗi trong 24 giờ gần nhất
SELECT 
  ResourceName, HttpStatusCode, COUNT(*) as ErrorCount
FROM AccessLogs
WHERE WasSuccessful = 0 
  AND AccessedAtUtc >= DATEADD(DAY, -1, GETUTCDATE())
GROUP BY ResourceName, HttpStatusCode
ORDER BY ErrorCount DESC;
```

---

## 📊 RELATIONSHIPS (Quan Hệ)

```
Users (1) ──────┬──── (n) UserRoles ──────┐ (n) Roles
                │                         │
                ├──── (n) LoginAttempts   │
                │                         │
                ├──── (n) EmailVerificationTokens
                │
                ├──── (n) PasswordResetTokens
                │
                ├──── (n) RefreshTokens
                │
                ├──── (n) UserActivityHistories
                │
                ├──── (n) AuditLogs
                │
                ├──── (n) ChangeHistories
                │
                └──── (n) AccessLogs
```

---

## 🧹 DATA RETENTION POLICY

| Bảng | Thời Hạn Lưu Trữ | Ghi Chú |
|-----|---------|--------|
| Users | ∞ (Không giới hạn) | Xoá mềm là chính |
| Roles | ∞ | Cấu hình hệ thống |
| UserRoles | ∞ | - |
| Permissions | ∞ | - |
| EmailVerificationTokens | 24 giờ sau khi hết hạn | Xoá tự động |
| PasswordResetTokens | 60 phút | Một lần sử dụng |
| RefreshTokens | 7 ngày | Token rotation |
| LoginAttempts | 90 ngày | Cần cho phân tích |
| UserActivityHistories | 1 năm | Tuân thủ GDPR |
| AuditLogs | 3 năm | Yêu cầu pháp lý |
| ChangeHistories | 3 năm | Tuân thủ audit |
| AccessLogs | 6 tháng | Bảo mật & hiệu năng |

**Cleanup Stored Procedures:**
```sql
-- Xóa old activity histories
CREATE PROCEDURE sp_CleanupOldActivities
AS
DELETE FROM UserActivityHistories 
WHERE OccurredAtUtc < DATEADD(YEAR, -1, GETUTCDATE());

-- Xóa old access logs
CREATE PROCEDURE sp_CleanupOldAccessLogs
AS
DELETE FROM AccessLogs 
WHERE AccessedAtUtc < DATEADD(MONTH, -6, GETUTCDATE());
```

---

## 🔒 SECURITY CONSIDERATIONS

### 1. Sensitive Data Protection
- ❌ Không lưu password thô
- ✅ Lưu password hash (Bcrypt)
- ✅ Mask sensitive data trong logs
- ✅ Encrypt connection string

### 2. Access Control
- ✅ Row-level security cho audit logs
- ✅ Column-level encryption cho sensitive fields
- ✅ Audit trail không thể thay đổi (append-only)

### 3. Compliance
- ✅ GDPR: Soft delete, data retention
- ✅ SOC 2: Comprehensive audit logs
- ✅ PCI DSS: Password security, access control
- ✅ HIPAA: Change history, access logs

---

## 📈 PERFORMANCE TIPS

1. **Pagination:** Luôn sử dụng phân trang cho lịch sử
2. **Indexes:** Kiểm tra thực hiện query plan
3. **Archiving:** Lưu trữ lịch sử cũ sang bảng archive
4. **Partitioning:** Phân vùng bảng lớn theo ngày
5. **Compression:** Nén dữ liệu cũ

---

**Tài liệu này được tạo tự động. Vui lòng cập nhật khi có thay đổi schema mới.** 📝