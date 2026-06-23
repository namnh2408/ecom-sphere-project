# Implementation Plan Giai Đoạn 2 - API Gateway YARP Và Authentication Với Keycloak

> Thuộc roadmap: `Docs/plans/plan_overral_time20260623.md`  
> Nguồn tham chiếu: `Docs/ecommerce-microservice-roadmap.md`, `Docs/api/api-guidelines.md`, `Docs/architecture/local-port-mapping.md`  
> Giai đoạn: 2  
> Timestamp: 20260623_1339  
> Mục tiêu thời gian: Tuần 3-4  
> Runtime mục tiêu: .NET 8 LTS  
> Trạng thái: Kế hoạch implement chi tiết, chưa thực thi code

---

## 1. Mục Tiêu Giai Đoạn 2

Giai đoạn 2 thiết lập API Gateway làm entrypoint duy nhất cho client và đưa cơ chế xác thực/ủy quyền nền tảng vào hệ thống bằng Keycloak, OAuth2/OIDC và JWT Bearer.

Kết thúc giai đoạn này cần có:

- `ECommerceMS.ApiGateway` chạy bằng .NET 8.
- YARP reverse proxy route được tới Order Service.
- Cấu hình sẵn route placeholder cho Product, Payment, Notification để dùng ở các giai đoạn sau.
- Keycloak local chạy được và có realm `ecommerce`.
- Client `ecommerce-api`, roles `customer`, `admin` và user test tối thiểu.
- Gateway và Order API validate JWT hợp lệ.
- Endpoint protected trả `401` khi thiếu token, `403` khi thiếu role.
- Rate limiting hoạt động tại Gateway.
- Health endpoints đi qua Gateway hoặc được cấu hình rõ ràng.
- Không hardcode Keycloak secret hoặc password thật trong code.

---

## 2. Phạm Vi Công Việc

### 2.1. Trong phạm vi

- Tạo project `ECommerceMS.ApiGateway`.
- Thêm YARP Reverse Proxy.
- Cấu hình route và cluster cho Order Service.
- Chuẩn bị route cho Product, Payment, Notification ở trạng thái disabled/commented hoặc placeholder rõ ràng.
- Tạo cấu hình Keycloak local.
- Tạo realm export mẫu cho development.
- Cấu hình JWT Bearer authentication ở Gateway và Order API.
- Cấu hình authorization policy theo role.
- Thêm rate limiting tại Gateway.
- Cập nhật configuration convention nếu cần thêm key mới.
- Viết checklist test thủ công lấy token, gọi API qua Gateway và kiểm tra status code.

### 2.2. Ngoài phạm vi

- Không implement Product, Payment, Notification business logic.
- Không implement Kafka, MassTransit, Outbox.
- Không implement CI/CD.
- Không implement Docker Compose full stack production-like.
- Không implement refresh token flow cho frontend thật.
- Không implement user registration business flow.
- Không đưa Keycloak vào Kubernetes.
- Không triển khai observability đầy đủ ngoài logging cơ bản.

Giai đoạn này chỉ cần đủ auth/gateway nền tảng để các request client không gọi trực tiếp vào service trong luồng chuẩn.

---

## 3. Điều Kiện Đầu Vào

Giai đoạn 2 phụ thuộc vào giai đoạn 1.

### 3.1. Điều kiện bắt buộc

- `.NET SDK 8.x` đã sẵn sàng.
- `ECommerceMS.sln` đã tồn tại.
- Order Service đã chạy được.
- Order API có các endpoint cơ bản:
  - `POST /api/orders`
  - `GET /api/orders/{id}`
  - `GET /api/orders`
  - `PATCH /api/orders/{id}/cancel`
  - `/health/live`
  - `/health/ready`
- Order API có port local rõ ràng, mặc định `5001`.
- Gateway dùng port local `5000`.

### 3.2. Tooling cần có

| Công cụ | Mục đích |
|---|---|
| .NET SDK 8.x | Build Gateway và Order API |
| Docker Desktop | Chạy Keycloak và PostgreSQL cho Keycloak nếu dùng DB riêng |
| HTTP client | Test token và API |
| Browser | Truy cập Keycloak Admin Console |

### 3.3. Kiểm tra trước khi bắt đầu

```powershell
dotnet --version
dotnet build ECommerceMS.sln
dotnet run --project src/Services/Order/ECommerceMS.Order.API
docker --version
docker compose version
```

Điều kiện pass:

- `dotnet --version` trả về SDK `8.x`.
- Order API chạy và mở được Scalar/OpenAPI.
- Docker CLI chạy được.

---

## 4. Cấu Trúc Project Mục Tiêu

Tạo Gateway dưới `src/ApiGateway`:

```text
src/ApiGateway/
└── ECommerceMS.ApiGateway/
    ├── Extensions/
    │   ├── AuthenticationExtensions.cs
    │   ├── RateLimitingExtensions.cs
    │   └── ReverseProxyExtensions.cs
    ├── Middlewares/
    │   └── CorrelationIdMiddleware.cs
    ├── appsettings.json
    ├── appsettings.Development.json
    └── Program.cs
```

Nếu cần shared auth helper sớm, có thể tạo:

```text
src/Shared/
└── ECommerceMS.Shared.Infrastructure/
    └── Authentication/
```

Khuyến nghị giai đoạn 2 chỉ tạo Shared Infrastructure nếu cần reuse JWT setup giữa Gateway và Order API. Nếu code trùng nhỏ, có thể giữ local extension ở từng project và refactor sang Shared sau khi pattern ổn định.

---

## 5. Packages Mục Tiêu

### 5.1. API Gateway

```powershell
dotnet add src/ApiGateway/ECommerceMS.ApiGateway package Yarp.ReverseProxy
dotnet add src/ApiGateway/ECommerceMS.ApiGateway package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.*
```

Rate limiting dùng API built-in của ASP.NET Core 8, không cần package riêng.

### 5.2. Order API

Nếu chưa có:

```powershell
dotnet add src/Services/Order/ECommerceMS.Order.API package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.*
```

Nếu tạo shared helper:

```powershell
dotnet add src/Shared/ECommerceMS.Shared.Infrastructure package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.*
```

---

## 6. Configuration Keys

### 6.1. Gateway appsettings

```json
{
  "Keycloak": {
    "Authority": "http://localhost:8080/realms/ecommerce",
    "Audience": "ecommerce-api",
    "RequireHttpsMetadata": false
  },
  "RateLimiting": {
    "PermitLimit": 100,
    "WindowSeconds": 60,
    "SegmentsPerWindow": 6,
    "QueueLimit": 10
  },
  "ReverseProxy": {
    "Routes": {},
    "Clusters": {}
  }
}
```

### 6.2. Order API appsettings

```json
{
  "Keycloak": {
    "Authority": "http://localhost:8080/realms/ecommerce",
    "Audience": "ecommerce-api",
    "RequireHttpsMetadata": false
  }
}
```

### 6.3. Environment variables

Các biến môi trường mẫu nên bổ sung vào `.env.example` khi implement:

```text
KEYCLOAK_URL=http://localhost:8080
KEYCLOAK_REALM=ecommerce
KEYCLOAK_AUTHORITY=http://localhost:8080/realms/ecommerce
KEYCLOAK_AUDIENCE=ecommerce-api
KEYCLOAK_ADMIN=admin
KEYCLOAK_ADMIN_PASSWORD=change-me-local-only
API_GATEWAY_HTTP_PORT=5000
ORDER_SERVICE_URL=http://localhost:5001
```

Không commit password thật. Password local trong sample phải thể hiện rõ là placeholder.

---

## 7. YARP Route Design

### 7.1. Route mục tiêu giai đoạn 2

| Route | Match path | Cluster | Auth | Ghi chú |
|---|---|---|---|---|
| `order-api` | `/api/orders/{**catch-all}` | `order-cluster` | Required | Route chính |
| `order-health-live` | `/orders/health/live` | `order-cluster` | Anonymous | Optional |
| `order-health-ready` | `/orders/health/ready` | `order-cluster` | Anonymous | Optional |
| `gateway-health-live` | `/health/live` | Gateway | Anonymous | Health của Gateway |
| `gateway-health-ready` | `/health/ready` | Gateway | Anonymous | Health của Gateway |

Khuyến nghị:

- Gateway health dùng `/health/live`, `/health/ready`.
- Service health qua Gateway dùng prefix service như `/orders/health/live` để tránh trùng path.
- Client nghiệp vụ chỉ gọi `/api/orders...` qua Gateway.

### 7.2. Placeholder routes cho service tương lai

Có thể chuẩn bị nhưng chưa bật nếu service chưa tồn tại:

```text
/api/products/{**catch-all}
/api/payments/{**catch-all}
/api/notifications/{**catch-all}
```

Nếu bật placeholder mà service chưa chạy, test phải kỳ vọng `503`. Khuyến nghị chưa bật trong config active cho đến khi service có thật.

### 7.3. Cluster config mẫu

```json
{
  "ReverseProxy": {
    "Routes": {
      "order-route": {
        "ClusterId": "order-cluster",
        "AuthorizationPolicy": "Authenticated",
        "Match": {
          "Path": "/api/orders/{**catch-all}"
        }
      }
    },
    "Clusters": {
      "order-cluster": {
        "Destinations": {
          "order-service": {
            "Address": "http://localhost:5001/"
          }
        }
      }
    }
  }
}
```

Trong Docker Compose sau này, destination đổi thành `http://order-service:8080/`.

---

## 8. Keycloak Local Design

### 8.1. Realm

Realm:

```text
ecommerce
```

### 8.2. Client

Client:

```text
ecommerce-api
```

Khuyến nghị cho local development:

| Setting | Giá trị |
|---|---|
| Client type | OpenID Connect |
| Access type | Public cho frontend test hoặc confidential nếu test client credentials |
| Standard flow | Enabled |
| Direct access grants | Enabled cho local test bằng password grant |
| Valid redirect URIs | `http://localhost:5000/*`, `http://localhost:*/*` cho local |
| Web origins | `http://localhost:5000`, `*` chỉ nếu local và có ghi chú rõ |

Nếu dùng password grant để test, chỉ dùng local development. Production không khuyến nghị password grant cho public app.

### 8.3. Roles

Realm roles:

```text
customer
admin
```

### 8.4. Test users

Tạo user local:

| Username | Roles | Mục đích |
|---|---|---|
| `customer01` | `customer` | Test endpoint customer |
| `admin01` | `admin` | Test endpoint admin/policy |

Password local chỉ là placeholder, không commit password thật.

### 8.5. Token claim mapping

Cần đảm bảo JWT có role claim mà ASP.NET Core đọc được.

Keycloak thường đưa realm roles vào:

```json
{
  "realm_access": {
    "roles": ["customer"]
  }
}
```

ASP.NET Core không tự map nested roles này thành `ClaimTypes.Role`. Cần một trong hai cách:

- Cấu hình Keycloak mapper để roles xuất hiện ở claim phẳng như `roles`.
- Hoặc trong JWT events/claims transformation, đọc `realm_access.roles` và add role claims.

Khuyến nghị giai đoạn 2: dùng claims transformation trong .NET để chủ động map role từ `realm_access.roles`.

---

## 9. Auth Và Authorization Design

### 9.1. Gateway authentication

Gateway phải:

- Validate issuer.
- Validate audience.
- Validate lifetime.
- Validate signing key qua authority metadata.
- Tắt `RequireHttpsMetadata` chỉ ở Development.

Pseudo config:

```csharp
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = config["Keycloak:Authority"];
        options.Audience = config["Keycloak:Audience"];
        options.RequireHttpsMetadata = false;
    });
```

### 9.2. Gateway authorization policies

Policies:

| Policy | Rule | Dùng cho |
|---|---|---|
| `Authenticated` | Any authenticated user | Order customer endpoints |
| `AdminOnly` | Role `admin` | Admin endpoints sau này |
| `CustomerOrAdmin` | Role `customer` hoặc `admin` | Order endpoints nếu cần role |

Giai đoạn 2 tối thiểu cần `Authenticated`; nên thêm `AdminOnly` để test `403`.

### 9.3. Order API authentication

Order API cũng phải validate JWT, không chỉ Gateway. Lý do:

- Nếu ai đó gọi thẳng service port trong local/dev, service vẫn tự bảo vệ.
- Sau này khi Gateway hoặc internal routing thay đổi, security không chỉ nằm ở edge.

Quy tắc:

- `POST /api/orders`: `[Authorize]` hoặc policy `CustomerOrAdmin`.
- `GET /api/orders/{id}`: `[Authorize]`.
- `GET /api/orders`: `[Authorize]`.
- `PATCH /api/orders/{id}/cancel`: `[Authorize]`.
- `/health/live`, `/health/ready`: `[AllowAnonymous]`.
- Scalar/OpenAPI Development có thể anonymous.

### 9.4. Role checks ở Order API

Giai đoạn 2 chưa cần phân quyền owner-by-order vì chưa có user/customer mapping hoàn chỉnh. Chỉ cần:

- Endpoint order yêu cầu authenticated user.
- Thêm một endpoint hoặc route test admin nếu cần xác minh `403`.

Không hardcode authorization bằng cách đọc raw header trong controller.

---

## 10. Rate Limiting Design

Rate limit đặt ở Gateway.

### 10.1. Policy mặc định

```text
Policy name: api
PermitLimit: 100 requests
Window: 60 seconds
SegmentsPerWindow: 6
QueueLimit: 10
```

### 10.2. Partition key

Thứ tự ưu tiên partition:

1. User id claim nếu authenticated.
2. Client id claim nếu có.
3. Remote IP address.

Nếu implement đơn giản giai đoạn 2, có thể dùng IP partition trước và ghi rõ sẽ nâng cấp theo user claim khi auth flow ổn định.

### 10.3. Response khi bị limit

Khi vượt rate limit:

- HTTP `429`.
- Có body ngắn gọn.
- Có thể thêm header `Retry-After` nếu dễ implement.

---

## 11. Polly Và Resilience

YARP đã proxy HTTP, giai đoạn 2 không nên thêm resilience phức tạp nếu chưa có service-to-service HTTP client rõ ràng.

Áp dụng theo thứ tự:

1. Ưu tiên route/health/auth/rate limit ổn định.
2. Nếu có HTTP client nội bộ trong Gateway hoặc service, thêm Polly:
   - Retry tối đa 3 lần.
   - Timeout 5 giây.
   - Circuit breaker khi lỗi cao.

Nếu chưa có downstream HTTP client ngoài YARP, ghi rõ Polly để sau; không ép thêm abstraction không dùng.

---

## 12. Các Bước Implement Chi Tiết

### Bước 2.1: Tạo Gateway project

**Việc làm**

- Tạo `ECommerceMS.ApiGateway` target `net8.0`.
- Add vào `ECommerceMS.sln`.
- Xóa `.gitkeep` ở `src/ApiGateway` khi có project thật.
- Cài `Yarp.ReverseProxy`.

**Command gợi ý**

```powershell
dotnet new webapi -n ECommerceMS.ApiGateway -o src/ApiGateway/ECommerceMS.ApiGateway --framework net8.0
dotnet sln ECommerceMS.sln add src/ApiGateway/ECommerceMS.ApiGateway/ECommerceMS.ApiGateway.csproj
dotnet add src/ApiGateway/ECommerceMS.ApiGateway package Yarp.ReverseProxy
dotnet add src/ApiGateway/ECommerceMS.ApiGateway package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.*
```

**Điều kiện pass**

- Gateway build được.
- Project target `net8.0`.

---

### Bước 2.2: Cấu hình YARP route đến Order Service

**Việc làm**

- Tạo route `/api/orders/{**catch-all}`.
- Destination local là `http://localhost:5001/`.
- Route yêu cầu auth policy `Authenticated`.
- Gateway có health endpoints riêng.

**Output mong đợi**

- Gọi `GET http://localhost:5000/api/orders` được proxy đến Order API nếu có token.
- Thiếu token trả `401` trước khi đến Order API.

**Điều kiện pass**

- Không gọi trực tiếp Order Service trong tài liệu test chuẩn trừ khi debug.
- Gateway không chứa business logic của Order.

---

### Bước 2.3: Chạy Keycloak local

**Việc làm**

- Chọn cách chạy Keycloak local:
  - Cách nhanh: `docker run`.
  - Cách bền hơn: compose file tối thiểu trong `infra/docker/keycloak`.
- Tạo realm `ecommerce`.
- Tạo client `ecommerce-api`.
- Tạo roles và users test.
- Export realm ra file development.

**Command chạy nhanh gợi ý**

```powershell
docker run --name ecommerce-keycloak `
  -p 8080:8080 `
  -e KEYCLOAK_ADMIN=admin `
  -e KEYCLOAK_ADMIN_PASSWORD=admin `
  quay.io/keycloak/keycloak:25.0 start-dev
```

Nếu tạo compose tối thiểu, file nên nằm ở:

```text
infra/docker/keycloak/docker-compose.keycloak.yml
```

Realm export nên nằm ở:

```text
infra/docker/keycloak/realm-export.json
```

**Điều kiện pass**

- Admin Console mở được tại `http://localhost:8080`.
- Realm `ecommerce` tồn tại.
- Có thể lấy access token local.

---

### Bước 2.4: Cấu hình JWT validation trong Gateway

**Việc làm**

- Add authentication.
- Add authorization policies.
- Map Keycloak roles nếu cần.
- Bật middleware đúng thứ tự:
  - `UseAuthentication()`
  - `UseAuthorization()`
  - `MapReverseProxy()`

**Điều kiện pass**

- Token hợp lệ đi qua Gateway.
- Token thiếu/sai trả `401`.
- Token không đủ role trả `403` với endpoint/policy test.

---

### Bước 2.5: Cấu hình JWT validation trong Order API

**Việc làm**

- Cài `Microsoft.AspNetCore.Authentication.JwtBearer` nếu chưa có.
- Add auth config giống Gateway.
- Protect order endpoints.
- Cho phép anonymous health endpoints.
- Nếu dùng controller, thêm `[Authorize]` ở controller hoặc actions.

**Điều kiện pass**

- Gọi trực tiếp `http://localhost:5001/api/orders` thiếu token trả `401`.
- Health endpoint vẫn anonymous.

---

### Bước 2.6: Cấu hình rate limiting tại Gateway

**Việc làm**

- Add rate limiter policy `api`.
- Apply rate limiter cho route API hoặc global Gateway.
- Trả `429` khi vượt limit.

**Test gợi ý**

- Tạm giảm `PermitLimit` xuống 3 trong Development để test.
- Gửi 5 request liên tục.
- Xác nhận request vượt limit trả `429`.
- Trả lại config mặc định sau test.

**Điều kiện pass**

- Rate limit không áp dụng quá mức lên health endpoint.
- API endpoint bị limit đúng policy.

---

### Bước 2.7: Cập nhật tài liệu và config mẫu

**Việc làm**

- Cập nhật `.env.example` nếu thêm biến Keycloak/Gateway.
- Cập nhật README cách chạy Gateway/Auth nếu cần.
- Thêm runbook auth local nếu muốn tách riêng:

```text
Docs/runbooks/auth-keycloak-local.md
```

**Nội dung runbook auth nên có**

- Cách chạy Keycloak.
- Cách vào Admin Console.
- Realm/client/role/user cần tạo.
- Cách lấy token.
- Cách gọi Gateway với Bearer token.

---

## 13. Cách Lấy Token Local

Nếu bật Direct Access Grants cho local:

```powershell
$body = @{
  client_id = "ecommerce-api"
  username = "customer01"
  password = "change-me-local-only"
  grant_type = "password"
}

Invoke-RestMethod `
  -Method Post `
  -Uri "http://localhost:8080/realms/ecommerce/protocol/openid-connect/token" `
  -ContentType "application/x-www-form-urlencoded" `
  -Body $body
```

Lưu ý:

- Password grant chỉ dùng local test.
- Không commit password thật.
- Nếu client là confidential, cần thêm `client_secret` và không commit secret.

---

## 14. Manual Verification

### 14.1. Start services

Terminal 1:

```powershell
dotnet run --project src/Services/Order/ECommerceMS.Order.API
```

Terminal 2:

```powershell
dotnet run --project src/ApiGateway/ECommerceMS.ApiGateway
```

Terminal 3:

```powershell
docker start ecommerce-keycloak
```

Hoặc dùng compose nếu đã tạo compose Keycloak tối thiểu.

### 14.2. Test anonymous denied

```http
GET http://localhost:5000/api/orders
```

Expected:

```text
401 Unauthorized
```

### 14.3. Test invalid token denied

```http
GET http://localhost:5000/api/orders
Authorization: Bearer invalid-token
```

Expected:

```text
401 Unauthorized
```

### 14.4. Test valid token accepted

```http
GET http://localhost:5000/api/orders?pageNumber=1&pageSize=20
Authorization: Bearer {access_token}
```

Expected:

```text
200 OK
```

Hoặc response nghiệp vụ hợp lệ từ Order API.

### 14.5. Test role forbidden

Tạo endpoint/policy test hoặc admin-only route nếu có.

```http
GET http://localhost:5000/api/admin/test
Authorization: Bearer {customer_token}
```

Expected:

```text
403 Forbidden
```

Nếu chưa có admin endpoint thật, ghi rõ test này dùng endpoint tạm trong Development và phải xóa trước khi hoàn thành nếu không thuộc roadmap.

### 14.6. Test health anonymous

```http
GET http://localhost:5000/health/live
GET http://localhost:5000/health/ready
```

Expected:

```text
200 OK
```

### 14.7. Test rate limit

- Giảm `PermitLimit` tạm thời xuống `3`.
- Gửi hơn 3 request trong cùng window.
- Expected: request vượt ngưỡng trả `429`.
- Khôi phục `PermitLimit = 100`.

---

## 15. API/Gateway Contract Sau Giai Đoạn 2

### Public entrypoint

Client chỉ dùng:

```text
http://localhost:5000
```

### Order routes qua Gateway

```text
POST   http://localhost:5000/api/orders
GET    http://localhost:5000/api/orders/{id}
GET    http://localhost:5000/api/orders?pageNumber=1&pageSize=20
PATCH  http://localhost:5000/api/orders/{id}/cancel
```

### Health routes

```text
GET http://localhost:5000/health/live
GET http://localhost:5000/health/ready
```

Nếu expose service health qua Gateway:

```text
GET http://localhost:5000/orders/health/live
GET http://localhost:5000/orders/health/ready
```

---

## 16. Deliverables

Kết thúc giai đoạn 2 cần có:

- `ECommerceMS.ApiGateway` trong solution.
- YARP route tới Order Service.
- Gateway auth bằng Keycloak JWT.
- Order API auth bằng Keycloak JWT.
- Authorization policies tối thiểu: `Authenticated`, `AdminOnly`, `CustomerOrAdmin`.
- Rate limiting tại Gateway.
- Keycloak realm `ecommerce` export cho local dev.
- User test local có role `customer` và `admin`.
- Checklist test 401/403/200/429.
- README hoặc runbook auth local được cập nhật.

---

## 17. Definition Of Done

Giai đoạn 2 hoàn thành khi:

- [ ] `dotnet build ECommerceMS.sln` pass.
- [ ] Gateway chạy ở port `5000`.
- [ ] Order API chạy ở port `5001`.
- [ ] Keycloak chạy ở port `8080`.
- [ ] Realm `ecommerce` tồn tại.
- [ ] Client `ecommerce-api` tồn tại.
- [ ] Roles `customer`, `admin` tồn tại.
- [ ] Có ít nhất một user test customer và một user test admin.
- [ ] Gateway route `/api/orders` đến Order API.
- [ ] Client gọi Order qua Gateway, không gọi trực tiếp trong luồng chuẩn.
- [ ] Thiếu token trả `401`.
- [ ] Token sai trả `401`.
- [ ] Token hợp lệ trả response nghiệp vụ hợp lệ.
- [ ] Thiếu role trả `403` ở endpoint/policy test.
- [ ] Rate limit trả `429` khi vượt ngưỡng.
- [ ] Health endpoint anonymous hoạt động.
- [ ] Không hardcode secret thật trong code/config.

---

## 18. Checklist Nghiệm Thu

### Gateway

- [ ] Project target `net8.0`.
- [ ] Có package `Yarp.ReverseProxy`.
- [ ] Route `/api/orders/{**catch-all}` hoạt động.
- [ ] Route không chứa business logic.
- [ ] Destination local dùng config, không hardcode rải rác.
- [ ] Health endpoint của Gateway hoạt động.

### Keycloak

- [ ] Realm export được lưu ở vị trí rõ ràng.
- [ ] Client audience khớp `Keycloak:Audience`.
- [ ] Issuer khớp `Keycloak:Authority`.
- [ ] Roles xuất hiện trong token hoặc được transform đúng.
- [ ] Password/secret thật không commit.

### Auth

- [ ] Gateway validate JWT.
- [ ] Order API validate JWT.
- [ ] Health endpoint anonymous.
- [ ] Protected endpoint yêu cầu auth.
- [ ] Admin policy test được.

### Rate Limiting

- [ ] Policy `api` được đăng ký.
- [ ] API route áp dụng policy.
- [ ] Health route không bị limit sai.
- [ ] Vượt limit trả `429`.

### Documentation

- [ ] `.env.example` có biến auth/gateway cần thiết.
- [ ] README hoặc runbook có cách lấy token local.
- [ ] Tài liệu nói rõ password grant chỉ dùng local nếu có dùng.

---

## 19. Rủi Ro Và Cách Kiểm Soát

| Rủi ro | Tác động | Cách kiểm soát |
|---|---|---|
| Chỉ validate JWT ở Gateway | Gọi thẳng service có thể bypass auth | Order API cũng phải validate JWT |
| Role claim Keycloak không map đúng | Policy role luôn fail | Transform `realm_access.roles` hoặc cấu hình mapper |
| Hardcode Keycloak URL/secret | Khó deploy, rủi ro bảo mật | Dùng configuration/env vars |
| Gateway chứa business logic | Gateway phình to và coupling | Gateway chỉ route, auth, rate limit, cross-cutting |
| Rate limit áp dụng cả health check | Monitoring bị lỗi giả | Exclude hoặc policy riêng cho health |
| Bật placeholder route tới service chưa có | Test fail 503 gây nhiễu | Chưa bật route hoặc ghi rõ expected 503 |
| Direct access grant dùng như production | Sai security model | Chỉ dùng local test, ghi rõ trong runbook |
| CORS mở `*` không kiểm soát | Rủi ro khi deploy | Chỉ mở rộng ở local, production cấu hình riêng |

---

## 20. Lỗi Thường Gặp Và Cách Xử Lý

### Lỗi 1: Token hợp lệ nhưng API trả 401

**Nguyên nhân thường gặp**

- `Authority` sai realm.
- `Audience` không khớp client.
- Gateway/Order API không truy cập được Keycloak metadata.
- Token lấy từ realm khác.

**Cách xử lý**

- Kiểm tra `iss` trong JWT.
- Kiểm tra `aud` hoặc `azp`.
- Mở `http://localhost:8080/realms/ecommerce/.well-known/openid-configuration`.

### Lỗi 2: Token có role nhưng API trả 403

**Nguyên nhân thường gặp**

- Role nằm trong `realm_access.roles` nhưng ASP.NET Core chưa map.
- Policy dùng role name sai.

**Cách xử lý**

- Decode token kiểm tra claim.
- Thêm claims transformation.
- Chuẩn hóa role `customer`, `admin`.

### Lỗi 3: Gateway route trả 502/503

**Nguyên nhân thường gặp**

- Order API chưa chạy.
- Destination URL sai port.
- Path transform sai.

**Cách xử lý**

- Gọi trực tiếp `http://localhost:5001/health/live`.
- Kiểm tra YARP cluster destination.
- Kiểm tra log Gateway.

### Lỗi 4: Rate limit không hoạt động

**Nguyên nhân thường gặp**

- Chưa gọi `UseRateLimiter()`.
- Endpoint không require rate limiting policy.
- Test chưa vượt window/limit.

**Cách xử lý**

- Giảm limit tạm thời để test.
- Kiểm tra middleware order.
- Kiểm tra response status `429`.

---

## 21. Thứ Tự Commit Khuyến Nghị

```text
feat(gateway): add yarp api gateway
feat(auth): add keycloak jwt authentication
feat(auth): protect order endpoints with jwt
feat(gateway): add rate limiting policy
docs(auth): add keycloak local runbook
```

Nếu commit một lần cho toàn giai đoạn:

```text
feat(gateway): add yarp gateway and keycloak authentication
```

---

## 22. Ghi Chú Cho Giai Đoạn 3

Giai đoạn 2 cần để lại nền tốt cho event-driven ở giai đoạn 3:

- Correlation id nên được tạo/forward ở Gateway nếu implement kịp.
- Auth context nên có `userId`/`sub` để Order Service có thể dùng khi tạo event sau này.
- Gateway route ổn định giúp client không đổi URL khi các service phía sau chuyển sang event-driven.
- Không đưa Kafka concern vào Gateway.

