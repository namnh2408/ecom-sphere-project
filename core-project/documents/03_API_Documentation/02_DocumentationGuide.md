# 📚 HƯỚNG DẪN TÀI LIỆU SWAGGER API

**Phiên bản:** 1.0  
**Cập nhật lần cuối:** 2025  
**Trạng thái:** ✅ HOÀN THÀNH

---

## 🎯 TỔNG QUAN

Tài liệu này mô tả các cải tiến được triển khai trong tài liệu Swagger (OpenAPI 3.0) của API Core Project.

### Những gì đã được cải tiến:

✅ **Cấu hình Swagger hoàn chỉnh** - JWT Bearer, Versioning, Metadata  
✅ **XML Comments chi tiết** - Trên tất cả các endpoint  
✅ **Security Scheme** - Xác thực JWT Bearer  
✅ **Ví dụ Request/Response** - Ví dụ thực tế cho mỗi thao tác  
✅ **Tags & Grouping** - Các endpoint được nhóm theo chức năng  
✅ **Tài liệu lỗi** - Tài liệu về tất cả các mã lỗi  
✅ **Thực tiễn tốt nhất** - Swagger UI được tối ưu hóa  

---

## 📁 CÁC TẬP TIN ĐÃ SỬA ĐỔI

### Cấu hình cốt lõi
- **Program.cs** - Cấu hình Swagger hoàn chỉnh
- **Gateway.WebApi.csproj** - Bật tài liệu XML

### Controllers (Với chi tiết XML đầy đủ)
- **AuthController.cs** - 8 endpoint xác thực
- **UsersController.cs** - 13 endpoint quản lý người dùng
- **RolesController.cs** - 5 endpoint quản lý vai trò

### Mô hình phản hồi (Mới)
- **ResponseModels.cs** - Mô hình phản hồi tiêu chuẩn
- **SwaggerExamples.cs** - Ví dụ cho Swagger

---

## 🔧 CÁC CẤU HÌNH ĐÃ TRIỂN KHAI

### 1. Thông tin OpenAPI

```csharp
{
    "version": "v1.0.0",
    "title": "Core Project API",
    "description": "Hệ thống quản lý mô-đun người dùng API với xác thực, ủy quyền và ghi nhật ký kiểm toán",
    "contact": {
        "name": "API Support",
        "email": "support@example.com"
    },
    "license": {
        "name": "MIT License"
    }
}
```

### 2. Security Scheme (JWT Bearer)

```csharp
// Tự động được thêm vào tất cả các endpoint được bảo vệ
Authorization: Bearer <jwt_token>
```

**Endpoint công khai (không cần JWT):**
- POST /api/auth/register
- POST /api/auth/login
- POST /api/auth/verify-email
- POST /api/auth/request-password-reset
- POST /api/auth/reset-password
- POST /api/auth/refresh-token

**Endpoint được bảo vệ (cần JWT):**
- Tất cả endpoint trong `/api/users/`
- Tất cả endpoint trong `/api/roles/`
- POST /api/auth/change-password
- POST /api/auth/request-email-verification

### 3. Cải tiến Swagger UI

**Tính năng được bật:**
- ✅ Tìm kiếm bộ lọc
- ✅ Hiển thị Operation ID
- ✅ Trình xác thực
- ✅ Mở rộng mô hình
- ✅ Hiển thị tài liệu

**Cấu hình:**
```csharp
options.DefaultModelsExpandDepth(0);     // Mô hình thu gọn theo mặc định
options.DefaultModelExpandDepth(2);      // Độ sâu mở rộng 2 cấp
options.DocExpansion(DocExpansion.List); // Mở rộng danh sách hoạt động
options.EnableFilter();                  // Bộ lọc tìm kiếm
options.EnableValidator();               // Xác thực yêu cầu
```

---

## 📋 CÁC ENDPOINT ĐÃ TÀI LIỆU HÓA

### Xác thực (8 endpoint)

| Phương thức | Endpoint | Mô tả |
|-----------|----------|--------|
| POST | /api/auth/register | Đăng ký người dùng mới |
| POST | /api/auth/login | Đăng nhập |
| POST | /api/auth/change-password | Thay đổi mật khẩu |
| POST | /api/auth/request-email-verification | Yêu cầu xác minh email |
| POST | /api/auth/verify-email | Xác minh email |
| POST | /api/auth/request-password-reset | Yêu cầu đặt lại mật khẩu |
| POST | /api/auth/reset-password | Đặt lại mật khẩu |
| POST | /api/auth/refresh-token | Làm mới token JWT |

### Quản lý người dùng (13 endpoint)

| Phương thức | Endpoint | Mô tả |
|-----------|----------|--------|
| GET | /api/users | Liệt kê tất cả người dùng (với phân trang/bộ lọc) |
| GET | /api/users/{userId} | Lấy người dùng theo ID |
| GET | /api/users/email/{email} | Lấy người dùng theo email |
| PUT | /api/users/{userId}/profile | Cập nhật hồ sơ người dùng |
| POST | /api/users/{userId}/roles/{roleId} | Gán vai trò cho người dùng |
| DELETE | /api/users/{userId}/roles/{roleId} | Xóa vai trò khỏi người dùng |
| POST | /api/users/{userId}/deactivate | Vô hiệu hóa tài khoản người dùng |
| POST | /api/users/{userId}/activate | Kích hoạt tài khoản người dùng |
| DELETE | /api/users/{userId} | Xóa mềm của người dùng |
| POST | /api/users/{userId}/profile-picture | Tải lên ảnh hồ sơ |
| DELETE | /api/users/{userId}/profile-picture | Xóa ảnh hồ sơ |
| GET | /api/users/{userId}/login-history | Lấy lịch sử đăng nhập |
| GET | /api/users/{userId}/activity-history | Lấy lịch sử hoạt động |
| GET | /api/users/{userId}/audit-logs | Lấy nhật ký kiểm toán |

### Quản lý vai trò (5 endpoint)

| Phương thức | Endpoint | Mô tả |
|-----------|----------|--------|
| GET | /api/roles | Liệt kê tất cả các vai trò |
| GET | /api/roles/{roleId} | Lấy vai trò theo ID |
| POST | /api/roles | Tạo vai trò mới |
| PUT | /api/roles/{roleId} | Cập nhật vai trò |
| POST | /api/roles/{roleId}/permissions/{permissionId} | Gán quyền cho vai trò |

---

## 📝 CẤU TRÚC XML COMMENTS

Mỗi endpoint được tài liệu hóa với:

### Summary (Bắt buộc)
Mô tả ngắn gọn về chức năng của endpoint

```xml
/// <summary>
/// Xác thực người dùng bằng email và mật khẩu
/// </summary>
```

### Remarks (Tùy chọn nhưng được khuyến nghị)
Chi tiết bổ sung, quy tắc, tính năng bảo mật

```xml
/// <remarks>
/// Xác thực thông tin đăng nhập của người dùng và trả về token JWT để truy cập được xác thực.
/// 
/// **Tính năng bảo mật:**
/// - Nỗ lực đăng nhập thất bại được theo dõi và giới hạn tốc độ
/// - Tài khoản có thể bị khóa sau nhiều lần thất bại
/// - Mật khẩu không bao giờ được trả về trong phản hồi
/// </remarks>
```

### Parameters (Cho mỗi tham số)
Mô tả chi tiết

```xml
/// <param name="email">Địa chỉ email của người dùng</param>
/// <param name="password">Mật khẩu của người dùng (8+ ký tự)</param>
```

### Returns
Những gì phương thức trả về

```xml
/// <returns>Token xác thực khi đăng nhập thành công</returns>
```

### Response Codes
Tất cả các mã trạng thái có thể có

```xml
/// <response code="200">Đăng nhập thành công, trả về token JWT</response>
/// <response code="401">Thông tin đăng nhập không hợp lệ hoặc tài khoản bị khóa</response>
```

---

## 🔐 TÁCH LIỆU BẢO MẬT

### Định dạng Bearer Token

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Thành phần Token

**Access Token:**
- Loại: JWT
- Hết hạn: 15-30 phút (có thể cấu hình)
- Sử dụng: Xác thực yêu cầu
- Lưu trữ: Memory hoặc Session Storage

**Refresh Token:**
- Loại: JWT
- Hết hạn: 7-30 ngày (có thể cấu hình)
- Sử dụng: Lấy Access Token mới
- Lưu trữ: HttpOnly Cookie (được khuyến nghị)

### Luồng Xác thực

```
1. POST /auth/register hoặc /auth/login
   ↓
   Phản hồi: { accessToken, refreshToken, expiresIn }
   
2. Sử dụng accessToken trong các yêu cầu tiếp theo
   Headers: Authorization: Bearer <accessToken>
   
3. Khi accessToken hết hạn
   POST /auth/refresh-token
   Body: { refreshToken }
   ↓
   Phản hồi: { newAccessToken, newRefreshToken, expiresIn }
```

---

## 📊 MÔ HÌNH PHẢN HỒI

### Phản hồi thành công (2xx)

```json
{
    "data": {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "email": "john@example.com",
        "name": "John Doe",
        "isActive": true,
        "isEmailVerified": true,
        "createdAt": "2025-01-15T10:30:00Z"
    },
    "message": "Hoạt động thành công"
}
```

### Phản hồi có phân trang

```json
{
    "items": [
        { "id": "1", "name": "User 1" },
        { "id": "2", "name": "User 2" }
    ],
    "totalCount": 100,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 10,
    "hasNextPage": true,
    "hasPreviousPage": false
}
```

### Phản hồi lỗi (4xx, 5xx)

```json
{
    "error": "INVALID_CREDENTIALS",
    "message": "Email hoặc mật khẩu không chính xác",
    "details": {
        "field": "password",
        "reason": "incorrect_value"
    }
}
```

---

## 🧪 KIỂM TRA TRONG SWAGGER UI

### 1. Truy cập Swagger UI

```
URL: http://localhost:5000/swagger
hoặc
URL: http://localhost:5000/swagger/index.html
```

### 2. Xác thực bằng Bearer Token

1. Nhấp vào **Authorize** (biểu tượng khóa)
2. Chọn **Bearer**
3. Dán token JWT của bạn
4. Nhấp **Authorize**
5. Nhấp **Đóng**

### 3. Thực hiện Requests

1. Mở rộng một endpoint
2. Nhấp **Try it out**
3. Điền các tham số
4. Nhấp **Execute**
5. Xem phản hồi

### 4. Xem Ví dụ

- Ví dụ xuất hiện ở bên phải
- JSON hợp lệ để sao chép/dán
- Dựa trên SwaggerExamplesProvider

---

## 🔧 CẤU HÌNH ENDPOINT MỚI

### Mẫu hoàn chỉnh

```csharp
/// <summary>
/// [HÀNH ĐỘNG] [TÀI NGUYÊN]
/// </summary>
/// <remarks>
/// Mô tả chi tiết...
/// 
/// **Yêu cầu:**
/// - Liệt kê các yêu cầu
/// 
/// **Tác động:**
/// - Liệt kê các tác động
/// </remarks>
/// <param name="paramName">Mô tả</param>
/// <returns>Những gì trả về</returns>
/// <response code="200">Tình huống thành công</response>
/// <response code="400">Tình huống lỗi 1</response>
/// <response code="401">Tình huống lỗi 2</response>
[HttpGet("{id:guid}")]
[SwaggerOperation(Summary = "Lấy tài nguyên theo ID", OperationId = "GetResourceById")]
[ProducesResponseType(typeof(ResourceDto), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
public async Task<IActionResult> GetById(Guid id)
{
    // Triển khai
}
```

### Tags & Grouping

Sử dụng `[Tags("Danh mục")]` để nhóm các endpoint:

```csharp
[Tags("Quản lý người dùng")]
[Tags("Xác thực")]
[Tags("Quản lý vai trò")]
```

---

## 📖 VÍ DỤ SỬ DỤNG

### Đăng ký người dùng

**Request:**
```bash
curl -X POST "http://localhost:5000/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john@example.com",
    "password": "SecurePassword123",
    "firstName": "John",
    "lastName": "Doe"
  }'
```

**Phản hồi (201 Created):**
```json
{
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "eyJhbGciOiJIUzI1NiIs...",
    "expiresIn": 1800
}
```

### Đăng nhập người dùng

**Request:**
```bash
curl -X POST "http://localhost:5000/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john@example.com",
    "password": "SecurePassword123"
  }'
```

**Phản hồi (200 OK):**
```json
{
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "eyJhbGciOiJIUzI1NiIs...",
    "expiresIn": 1800
}
```

### Lấy danh sách người dùng

**Request:**
```bash
curl -X GET "http://localhost:5000/api/users?pageNumber=1&pageSize=10" \
  -H "Authorization: Bearer eyJhbGc..."
```

**Phản hồi (200 OK):**
```json
{
    "items": [
        {
            "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
            "email": "john@example.com",
            "firstName": "John",
            "lastName": "Doe",
            "isActive": true,
            "isEmailVerified": true
        }
    ],
    "totalCount": 50,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 5,
    "hasNextPage": true,
    "hasPreviousPage": false
}
```

### Cập nhật hồ sơ

**Request:**
```bash
curl -X PUT "http://localhost:5000/api/users/3fa85f64-5717-4562-b3fc-2c963f66afa6/profile" \
  -H "Authorization: Bearer eyJhbGc..." \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "Johnny",
    "lastName": "Smith",
    "phoneNumber": "+1234567890"
  }'
```

**Phản hồi (200 OK):**
```json
{
    "message": "Hồ sơ đã được cập nhật thành công"
}
```

---

## 🔄 BẢNG CHI TIẾT STATUS CODES

### 2xx - Thành công
| Code | Ý nghĩa | Ví dụ |
|------|---------|-------|
| 200 | OK | GET thành công, dữ liệu trả về |
| 201 | Created | POST tạo tài nguyên mới |

### 4xx - Lỗi client
| Code | Ý nghĩa | Ví dụ |
|------|---------|-------|
| 400 | Bad Request | Dữ liệu không hợp lệ |
| 401 | Unauthorized | Token không hợp lệ/hết hạn |
| 403 | Forbidden | Không có quyền truy cập |
| 404 | Not Found | Tài nguyên không tồn tại |

### 5xx - Lỗi server
| Code | Ý nghĩa | Ví dụ |
|------|---------|-------|
| 500 | Server Error | Lỗi nội bộ server |

---

## 💡 MẸMEO VÀ THỦ THUẬT

### Sử dụng GUIDs
```
Format: 3fa85f64-5717-4562-b3fc-2c963f66afa6
Luôn sử dụng GUID hợp lệ trong yêu cầu
```

### Định dạng ngày tháng
```
Format: ISO 8601
Ví dụ: 2025-01-15T10:30:00Z
```

### Phân biệt Deactivate vs Soft Delete
```
Deactivate: Người dùng có thể được kích hoạt lại
Soft Delete: Tài khoản bị đánh dấu là đã xóa vĩnh viễn
```

---

## 📞 HỖ TRỢ

Nếu bạn gặp sự cố:
1. Xem tài liệu TROUBLESHOOTING.md
2. Kiểm tra nhật ký ứng dụng
3. Xem lại ví dụ yêu cầu
4. Liên hệ hỗ trợ API

---

**Lần cập nhật cuối:** 2025-01-15
**Phiên bản:** 1.0