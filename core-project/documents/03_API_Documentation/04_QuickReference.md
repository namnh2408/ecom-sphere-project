# ⚡ SWAGGER API - HƯỚNG DẪN NHANH

**Tl;dr** - Mọi thứ về Swagger trong 1 trang

---

## 🚀 TRUY CẬP NHANH

### URL Swagger
```
http://localhost:5000/
```

### Truy cập Swagger UI
```
1. Khởi động ứng dụng: dotnet run
2. Mở trình duyệt: http://localhost:5000
3. Xem tài liệu tương tác
```

---

## 🔐 XÁC THỰC JWT

### Lấy Token

**Đăng nhập:**
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "password123"
  }'
```

**Phản hồi:**
```json
{
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "accessToken": "eyJhbGc...",
    "refreshToken": "eyJhbGc...",
    "expiresIn": 1800
}
```

### Sử dụng Token trong Swagger UI

1. Nhấp **"Authorize"** (biểu tượng khóa ở trên cùng)
2. Chọn **"Bearer"**
3. Dán **accessToken**
4. Nhấp **"Authorize"**

### Sử dụng Token trong Requests

```bash
curl -X GET http://localhost:5000/api/users \
  -H "Authorization: Bearer eyJhbGc..."
```

---

## 📋 CÁC HOẠT ĐỘNG CHÍNH

### Xác thực

| Hoạt động | Phương thức | Endpoint |
|----------|----------|----------|
| Đăng ký | POST | `/api/auth/register` |
| Đăng nhập | POST | `/api/auth/login` |
| Thay đổi mật khẩu | POST | `/api/auth/change-password` |
| Xác minh email | POST | `/api/auth/verify-email` |
| Đặt lại mật khẩu | POST | `/api/auth/reset-password` |
| Làm mới token | POST | `/api/auth/refresh-token` |

### Người dùng

| Hoạt động | Phương thức | Endpoint |
|----------|----------|----------|
| Liệt kê người dùng | GET | `/api/users?pageNumber=1&pageSize=10` |
| Lấy người dùng | GET | `/api/users/{userId}` |
| Cập nhật hồ sơ | PUT | `/api/users/{userId}/profile` |
| Xóa người dùng | DELETE | `/api/users/{userId}` |
| Vô hiệu hóa | POST | `/api/users/{userId}/deactivate` |
| Kích hoạt | POST | `/api/users/{userId}/activate` |
| Tải lên ảnh | POST | `/api/users/{userId}/profile-picture` |
| Lịch sử đăng nhập | GET | `/api/users/{userId}/login-history` |

### Vai trò

| Hoạt động | Phương thức | Endpoint |
|----------|----------|----------|
| Liệt kê vai trò | GET | `/api/roles` |
| Tạo vai trò | POST | `/api/roles` |
| Lấy vai trò | GET | `/api/roles/{roleId}` |
| Gán vai trò | POST | `/api/users/{userId}/roles/{roleId}` |

---

## 📝 VÍ DỤ REQUEST

### Đăng ký
```json
{
    "email": "john@example.com",
    "password": "SecurePassword123",
    "firstName": "John",
    "lastName": "Doe"
}
```

### Đăng nhập
```json
{
    "email": "john@example.com",
    "password": "SecurePassword123"
}
```

### Cập nhật hồ sơ
```json
{
    "firstName": "Johnny",
    "lastName": "Smith",
    "phoneNumber": "+1234567890"
}
```

### Tạo vai trò
```json
{
    "name": "Editor",
    "description": "Vai trò trình chỉnh sửa nội dung"
}
```

---

## 🔑 MÃ TRẠNG THÁI HTTP

| Mã | Ý nghĩa | Ví dụ |
|-----|---------|-------|
| 200 | OK | GET /users - thành công |
| 201 | Created | POST /users - đã tạo |
| 400 | Bad Request | Email không hợp lệ |
| 401 | Unauthorized | Token không hợp lệ |
| 403 | Forbidden | Không có quyền |
| 404 | Not Found | User không tồn tại |
| 500 | Server Error | Lỗi server |

---

## ⚠️ LỖI PHỔ BIẾN

### 401 Unauthorized
**Vấn đề:** Token không hợp lệ hoặc hết hạn  
**Giải pháp:** Đăng nhập lại, lấy token mới

### 400 Bad Request
**Vấn đề:** Dữ liệu không hợp lệ  
**Giải pháp:** Kiểm tra định dạng JSON, tham số bắt buộc

### 404 Not Found
**Vấn đề:** Tài nguyên không tồn tại  
**Giải pháp:** Kiểm tra ID, có thể sai

### Token hết hạn
**Vấn đề:** AccessToken hết hạn (30 phút)  
**Giải pháp:** Sử dụng refreshToken để lấy token mới

```bash
curl -X POST http://localhost:5000/api/auth/refresh-token \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "eyJhbGc..."
  }'
```

---

## 📊 BỘ LỌC VÀ PHÂN TRANG

### Liệt kê với bộ lọc
```
GET /api/users?pageNumber=1&pageSize=10&searchTerm=john&isActive=true
```

**Tham số:**
- `pageNumber` - Trang (mặc định: 1)
- `pageSize` - Mục trên trang (mặc định: 10)
- `searchTerm` - Tìm kiếm theo tên/email
- `isActive` - true/false/null
- `isEmailVerified` - true/false/null

### Phản hồi phân trang
```json
{
    "items": [...],
    "totalCount": 100,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 10,
    "hasNextPage": true,
    "hasPreviousPage": false
}
```

---

## 🧪 KIỂM TRA TRONG SWAGGER

### Tùy chọn 1: Swagger UI
1. Truy cập http://localhost:5000
2. Chọn endpoint
3. Nhấp "Try it out"
4. Điền tham số
5. Nhấp "Execute"

### Tùy chọn 2: cURL
```bash
curl -X GET http://localhost:5000/api/users \
  -H "Authorization: Bearer TOKEN" \
  -H "Content-Type: application/json"
```

### Tùy chọn 3: Postman
1. Nhập OpenAPI: http://localhost:5000/swagger/v1/swagger.json
2. Cấu hình Bearer token
3. Thực hiện requests

### Tùy chọn 4: Thunder Client (VS Code)
1. Cài đặt tiện ích mở rộng
2. Nhập Swagger
3. Cấu hình xác thực
4. Kiểm tra endpoint

---

## 🔍 TAGS (Nhóm)

**Xác thực**
- Đăng ký, Đăng nhập, Thay đổi mật khẩu, v.v.

**Quản lý người dùng**
- Lấy người dùng, Cập nhật hồ sơ, Tải lên ảnh, v.v.

**Quản lý vai trò**
- Tạo vai trò, Gán quyền, v.v.

---

## 💡 MẸO VÀ THỦ THUẬT

### Định dạng JWT Token
```
Authorization: Bearer <accessToken>

Token điển hình:
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.
eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.
SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c
```

### URL với ID
```
GET /api/users/3fa85f64-5717-4562-b3fc-2c963f66afa6
                └─ GUID của người dùng
```

### Timestamps
```
Định dạng: ISO 8601 (UTC)
Ví dụ: 2025-01-15T10:30:00Z
```

### Phân biệt Deactivate vs Soft Delete
```
Deactivate: Người dùng có thể được kích hoạt lại
Soft Delete: Tài khoản bị đánh dấu là đã xóa vĩnh viễn
```

---

## 🔐 SECURITY HEADERS

**Được khuyến nghị gửi:**
```
Authorization: Bearer <token>
Content-Type: application/json
Accept: application/json
```

**Không được gửi:**
- ❌ Mật khẩu trong Headers
- ❌ Token trong URL query
- ❌ Token trong localStorage (dùng cookies)

---

## 📱 VÍ DỤ POSTMAN

### Đăng ký người dùng
```
POST http://localhost:5000/api/auth/register
Content-Type: application/json

{
    "email": "john@example.com",
    "password": "SecurePassword123",
    "firstName": "John",
    "lastName": "Doe"
}
```

### Lấy người dùng (có xác thực)
```
GET http://localhost:5000/api/users/{{userId}}
Authorization: Bearer {{accessToken}}
```

### Cập nhật hồ sơ
```
PUT http://localhost:5000/api/users/{{userId}}/profile
Authorization: Bearer {{accessToken}}
Content-Type: application/json

{
    "firstName": "Johnny",
    "phoneNumber": "+1234567890"
}
```

---

## 🚀 LUỒNG ĐIỂN HÌNH

```
1. POST /auth/register
   ↓ (tạo tài khoản)
   Phản hồi: { accessToken, refreshToken }
   
2. Nhận email xác minh
   ↓
3. POST /auth/verify-email
   ↓ (với token từ email)
   
4. Sử dụng API thông thường
   GET /api/users
   Headers: Authorization: Bearer {accessToken}
   
5. AccessToken hết hạn (30 phút)
   ↓
6. POST /auth/refresh-token
   Body: { refreshToken }
   ↓
   Phản hồi: { newAccessToken }
   
7. Tiếp tục sử dụng API với token mới
```

---

## 🔗 LIÊN KẾT HỮU ÍCH

- 📖 [Hướng dẫn đầy đủ](./SWAGGER_DOCUMENTATION_GUIDE.md)
- 📊 [Tóm tắt cải tiến](./SWAGGER_IMPROVEMENTS_SUMMARY.md)
- 🗄️ [Sơ đồ cơ sở dữ liệu](./DATABASE_SCHEMA.md)
- 🚀 [Bắt đầu nhanh](./QUICK_START.md)

---

## 🆘 KHẮC PHỤC SỰ CỐ

### Swagger không tải?
```
1. Kiểm tra ứng dụng đang chạy: dotnet run
2. Truy cập: http://localhost:5000
3. Kiểm tra console trình duyệt (F12)
```

### Token không hoạt động?
```
1. Đăng nhập lại
2. Sao chép token chính xác (không có khoảng trắng)
3. Sử dụng "Bearer " trước token
4. Kiểm tra token chưa hết hạn
```

### Lỗi CORS?
```
Có thể do:
- Nguồn không chính xác
- Header bị thiếu
- Phương thức không được phép
```

### 404 Not Found?
```
Kiểm tra:
- URL chính xác
- ID hợp lệ (GUID)
- Tài nguyên tồn tại
```

### 500 Server Error?
```
1. Kiểm tra nhật ký ứng dụng
2. Kiểm tra kết nối cơ sở dữ liệu
3. Kiểm tra dữ liệu đầu vào
```

---

## 📞 HỖ TRỢ NHANH

**Gặp vấn đề?**

1. Kiểm tra lại yêu cầu của bạn
2. Xem ví dụ trong Swagger
3. Kiểm tra mã lỗi được trả về
4. Đọc tài liệu hướng dẫn
5. Liên hệ hỗ trợ nếu cần

---

**Cập nhật lần cuối:** 2025-01-15  
**Phiên bản:** 1.0  
**Ngôn ngữ:** Tiếng Việt