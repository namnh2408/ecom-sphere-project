# Implementation Plan Giai Đoạn 1 - Nền Móng Clean Architecture Và Order Service

> Thuộc roadmap: `Docs/plans/plan_overral_time20260623.md`  
> Nguồn tham chiếu: `Docs/ecommerce-microservice-roadmap.md`, `Docs/architecture/*`, `Docs/api/api-guidelines.md`  
> Giai đoạn: 1  
> Timestamp: 20260623_1328  
> Mục tiêu thời gian: Tuần 1-2  
> Runtime mục tiêu: .NET 8 LTS  
> Trạng thái: Kế hoạch implement chi tiết, chưa thực thi code

---

## 1. Mục Tiêu Giai Đoạn 1

Giai đoạn 1 tạo nền móng code đầu tiên cho toàn hệ thống bằng cách dựng `ECommerceMS.sln` và Order Service theo Clean Architecture. Order Service sẽ là service mẫu để các giai đoạn sau clone pattern cho Product, Payment và Notification.

Kết thúc giai đoạn này cần có:

- Solution `.NET 8` có cấu trúc rõ ràng.
- Order Service tách 4 layer: Domain, Application, Infrastructure, API.
- Domain model không phụ thuộc framework.
- CQRS với MediatR cho các use case cốt lõi.
- Validation bằng FluentValidation.
- Persistence bằng EF Core 8 và PostgreSQL provider.
- Repository pattern và Unit of Work.
- API endpoints cho tạo, hủy, lấy chi tiết và lấy danh sách đơn hàng.
- OpenAPI/Scalar UI dùng để test thủ công.
- Migration ban đầu cho `order_db`.

---

## 2. Phạm Vi Công Việc

### 2.1. Trong phạm vi

- Xử lý điều kiện bắt buộc về .NET 8 SDK trước khi tạo project.
- Tạo solution và project cho Order Service.
- Thiết lập dependency direction theo Clean Architecture.
- Implement Domain layer cho Order aggregate.
- Implement Application layer cho commands, queries, DTOs, validators và pipeline behaviors.
- Implement Infrastructure layer cho EF Core, repository và unit of work.
- Implement API layer cho controllers hoặc minimal endpoints, exception middleware và OpenAPI/Scalar.
- Tạo migration ban đầu.
- Chạy build và test thủ công các endpoint cơ bản.

### 2.2. Ngoài phạm vi

- Không implement Kafka, MassTransit, Outbox thật ở giai đoạn này.
- Không implement Product, Payment, Notification hoặc Gateway.
- Không implement Keycloak/JWT auth.
- Không implement Redis, Elasticsearch, SignalR, Hangfire.
- Không tạo Dockerfile hoặc Docker Compose hoàn chỉnh.
- Không tạo CI/CD pipeline.
- Không thiết kế production-grade observability đầy đủ.

Giai đoạn này có thể khai báo interface hoặc domain event để chuẩn bị cho giai đoạn 3, nhưng chưa publish event ra Kafka.

---

## 3. Điều Kiện Đầu Vào Và Blocker

### 3.1. Tooling bắt buộc

| Công cụ | Trạng thái mục tiêu | Ghi chú |
|---|---|---|
| .NET SDK | `8.x` | Bắt buộc trước khi tạo solution |
| Git | Đã có | Đã kiểm tra ở giai đoạn 0 |
| Docker | Nên có | Cần cho PostgreSQL local nếu chạy DB bằng container |
| PostgreSQL client | Nên có | `psql`, pgAdmin hoặc DBeaver |
| IDE | Visual Studio/Rider/VS Code | Phát triển và debug |

### 3.2. Blocker hiện tại cần xử lý

Theo `Docs/runbooks/local-setup.md`, máy hiện đang có `.NET SDK 10.0.301`, chưa thấy SDK 8.x trong PATH. Vì dự án đã chốt `.NET 8 LTS`, không tạo solution bằng SDK 10 trước khi xử lý một trong hai cách sau:

- Cài .NET SDK 8.x và xác nhận `dotnet --list-sdks` có dòng `8.x`.
- Nếu máy có SDK 8 nhưng chưa nằm trong PATH, sửa PATH rồi chạy lại `dotnet --list-sdks`.

Sau khi có SDK 8, tạo `global.json` ở root để khóa SDK major:

```json
{
  "sdk": {
    "version": "8.0.400",
    "rollForward": "latestFeature"
  }
}
```

Nếu phiên bản SDK 8 đã cài khác `8.0.400`, thay `version` bằng đúng version 8.x hiện có từ `dotnet --list-sdks`, vẫn giữ `rollForward: latestFeature`.

### 3.3. Kiểm tra trước khi bắt đầu

```powershell
dotnet --list-sdks
dotnet --version
git status --short
```

Điều kiện pass:

- `dotnet --list-sdks` có SDK `8.x`.
- `dotnet --version` trong repo trả về SDK `8.x` sau khi có `global.json`.
- Working tree không có thay đổi ngoài phạm vi đang biết.

---

## 4. Cấu Trúc Project Mục Tiêu

Tạo các project dưới `src/Services/Order`:

```text
src/Services/Order/
├── ECommerceMS.Order.Domain/
├── ECommerceMS.Order.Application/
├── ECommerceMS.Order.Infrastructure/
└── ECommerceMS.Order.API/
```

Tạo solution ở root:

```text
ECommerceMS.sln
global.json
```

Dependency direction bắt buộc:

```text
ECommerceMS.Order.API
  -> ECommerceMS.Order.Application
  -> ECommerceMS.Order.Infrastructure

ECommerceMS.Order.Infrastructure
  -> ECommerceMS.Order.Application
  -> ECommerceMS.Order.Domain

ECommerceMS.Order.Application
  -> ECommerceMS.Order.Domain

ECommerceMS.Order.Domain
  -> không phụ thuộc project nào
```

Quy tắc:

- Domain không reference EF Core, ASP.NET Core, MediatR, FluentValidation, Mapster, Dapper hoặc PostgreSQL provider.
- Application không reference Infrastructure hoặc API.
- Infrastructure không reference API.
- API là composition root, được phép gọi extension methods của Application và Infrastructure để đăng ký DI.

---

## 5. Danh Sách Packages

### 5.1. Domain

Không cài package ngoài ở Domain.

### 5.2. Application

Packages mục tiêu:

```powershell
dotnet add src/Services/Order/ECommerceMS.Order.Application package MediatR --version 12.*
dotnet add src/Services/Order/ECommerceMS.Order.Application package FluentValidation --version 11.*
dotnet add src/Services/Order/ECommerceMS.Order.Application package FluentValidation.DependencyInjectionExtensions --version 11.*
dotnet add src/Services/Order/ECommerceMS.Order.Application package Mapster --version 7.*
```

### 5.3. Infrastructure

Packages mục tiêu:

```powershell
dotnet add src/Services/Order/ECommerceMS.Order.Infrastructure package Microsoft.EntityFrameworkCore --version 8.*
dotnet add src/Services/Order/ECommerceMS.Order.Infrastructure package Microsoft.EntityFrameworkCore.Design --version 8.*
dotnet add src/Services/Order/ECommerceMS.Order.Infrastructure package Npgsql.EntityFrameworkCore.PostgreSQL --version 8.*
dotnet add src/Services/Order/ECommerceMS.Order.Infrastructure package Dapper --version 2.*
```

### 5.4. API

Packages mục tiêu:

```powershell
dotnet add src/Services/Order/ECommerceMS.Order.API package Microsoft.AspNetCore.OpenApi --version 8.*
dotnet add src/Services/Order/ECommerceMS.Order.API package Scalar.AspNetCore --version 1.*
```

Nếu template Web API đã có OpenAPI package phù hợp thì không cài trùng.

---

## 6. Các Bước Implement Chi Tiết

### Bước 1.1: Chuẩn bị SDK và solution

**Việc làm**

1. Cài hoặc xác nhận .NET SDK 8.x.
2. Tạo `global.json`.
3. Tạo solution `ECommerceMS.sln`.
4. Tạo 4 project Order.
5. Add project vào solution.
6. Xóa `.gitkeep` trong `src/Services/Order` sau khi có project thật.

**Command gợi ý**

```powershell
dotnet new globaljson --sdk-version 8.0.400 --roll-forward latestFeature
dotnet new sln -n ECommerceMS
dotnet new classlib -n ECommerceMS.Order.Domain -o src/Services/Order/ECommerceMS.Order.Domain --framework net8.0
dotnet new classlib -n ECommerceMS.Order.Application -o src/Services/Order/ECommerceMS.Order.Application --framework net8.0
dotnet new classlib -n ECommerceMS.Order.Infrastructure -o src/Services/Order/ECommerceMS.Order.Infrastructure --framework net8.0
dotnet new webapi -n ECommerceMS.Order.API -o src/Services/Order/ECommerceMS.Order.API --framework net8.0
dotnet sln ECommerceMS.sln add src/Services/Order/ECommerceMS.Order.Domain/ECommerceMS.Order.Domain.csproj
dotnet sln ECommerceMS.sln add src/Services/Order/ECommerceMS.Order.Application/ECommerceMS.Order.Application.csproj
dotnet sln ECommerceMS.sln add src/Services/Order/ECommerceMS.Order.Infrastructure/ECommerceMS.Order.Infrastructure.csproj
dotnet sln ECommerceMS.sln add src/Services/Order/ECommerceMS.Order.API/ECommerceMS.Order.API.csproj
```

Nếu SDK 8 đã cài không phải `8.0.400`, dùng đúng version hiện có trong `global.json`.

**Output mong đợi**

- `ECommerceMS.sln` tồn tại ở root.
- 4 project Order target `net8.0`.
- `dotnet build ECommerceMS.sln` chạy tới bước thiếu code/dependency hoặc pass sau khi setup reference.

**Điều kiện pass**

- Không có project nào target `net10.0`.
- Không tạo project ngoài `src/Services/Order`.

---

### Bước 1.2: Thiết lập project references

**Việc làm**

Thiết lập reference đúng chiều:

```powershell
dotnet add src/Services/Order/ECommerceMS.Order.Application reference src/Services/Order/ECommerceMS.Order.Domain
dotnet add src/Services/Order/ECommerceMS.Order.Infrastructure reference src/Services/Order/ECommerceMS.Order.Application
dotnet add src/Services/Order/ECommerceMS.Order.Infrastructure reference src/Services/Order/ECommerceMS.Order.Domain
dotnet add src/Services/Order/ECommerceMS.Order.API reference src/Services/Order/ECommerceMS.Order.Application
dotnet add src/Services/Order/ECommerceMS.Order.API reference src/Services/Order/ECommerceMS.Order.Infrastructure
```

**Output mong đợi**

- Project reference đúng như dependency direction.

**Điều kiện pass**

- Domain không reference project nào.
- Application chỉ reference Domain.
- Infrastructure reference Application/Domain.
- API reference Application/Infrastructure.

---

### Bước 1.3: Implement Domain primitives

**Cấu trúc thư mục**

```text
ECommerceMS.Order.Domain/
├── Abstractions/
│   ├── Entity.cs
│   ├── AggregateRoot.cs
│   └── IDomainEvent.cs
├── Entities/
│   ├── Order.cs
│   └── OrderItem.cs
├── Enums/
│   └── OrderStatus.cs
├── Events/
│   ├── OrderCreatedDomainEvent.cs
│   └── OrderCancelledDomainEvent.cs
├── Exceptions/
│   ├── DomainException.cs
│   ├── OrderInvalidStatusException.cs
│   └── OrderMustHaveItemsException.cs
└── ValueObjects/
    ├── Address.cs
    ├── CustomerId.cs
    ├── Money.cs
    ├── OrderId.cs
    └── ProductId.cs
```

**Thiết kế bắt buộc**

- `IDomainEvent` dùng cho event nội bộ domain, chưa phải Kafka integration event.
- `Entity<TId>` có `Id`.
- `AggregateRoot<TId>` quản lý `DomainEvents`.
- Value objects immutable, validate ngay khi tạo.
- Domain exception dùng cho lỗi nghiệp vụ, không chứa HTTP status.

**OrderStatus**

```text
Pending
Confirmed
Shipping
Delivered
Cancelled
```

**Order aggregate behavior**

| Method | Behavior | Domain event |
|---|---|---|
| `Create(customerId, items, shippingAddress)` | Tạo order trạng thái `Pending`, tính total | `OrderCreatedDomainEvent` |
| `Cancel(reason)` | Chỉ cho hủy khi `Pending` hoặc `Confirmed` | `OrderCancelledDomainEvent` |
| `Confirm()` | Chỉ cho confirm khi `Pending` | `OrderConfirmedDomainEvent` nếu implement ở giai đoạn này |
| `MarkAsShipping()` | Có thể để sau, nếu chưa expose API | Không bắt buộc |
| `MarkAsDelivered()` | Có thể để sau, nếu chưa expose API | Không bắt buộc |

**OrderItem rules**

- `ProductId` bắt buộc.
- `ProductName` không rỗng.
- `Quantity > 0`.
- `UnitPrice >= 0`.
- `TotalPrice = UnitPrice * Quantity`.

**Money rules**

- `Amount >= 0`.
- `Currency` mặc định `VND`.
- Không dùng `double` cho tiền, dùng `decimal`.

**Điều kiện pass**

- Domain compile độc lập.
- Không import namespace từ ASP.NET Core, EF Core, MediatR hoặc Infrastructure.
- Không public setter tùy tiện cho state quan trọng.

---

### Bước 1.4: Implement Application contracts và CQRS foundation

**Cấu trúc thư mục**

```text
ECommerceMS.Order.Application/
├── Abstractions/
│   ├── Messaging/
│   │   ├── ICommand.cs
│   │   ├── ICommandHandler.cs
│   │   ├── IQuery.cs
│   │   └── IQueryHandler.cs
│   └── Persistence/
│       ├── IOrderRepository.cs
│       └── IUnitOfWork.cs
├── Behaviors/
│   ├── LoggingBehavior.cs
│   └── ValidationBehavior.cs
├── Commands/
│   ├── CancelOrder/
│   └── CreateOrder/
├── DTOs/
│   ├── CreateOrderItemDto.cs
│   ├── OrderDto.cs
│   └── OrderItemDto.cs
├── Queries/
│   ├── GetOrderById/
│   └── GetOrders/
└── DependencyInjection.cs
```

**Use cases bắt buộc**

| Use case | Input | Output |
|---|---|---|
| CreateOrder | CustomerId, shipping address, items | `OrderDto` |
| CancelOrder | OrderId, reason | `OrderDto` hoặc success result |
| GetOrderById | OrderId | `OrderDto` |
| GetOrders | pageNumber, pageSize | paged result/list |

**Command/query naming**

```text
CreateOrderCommand
CreateOrderCommandHandler
CreateOrderCommandValidator
CancelOrderCommand
CancelOrderCommandHandler
CancelOrderCommandValidator
GetOrderByIdQuery
GetOrderByIdQueryHandler
GetOrdersQuery
GetOrdersQueryHandler
```

**Validation rules**

CreateOrder:

- `CustomerId` không empty.
- `Items` không null và có ít nhất 1 item.
- Mỗi item có `ProductId` không empty.
- `Quantity > 0`.
- `UnitPrice >= 0`.
- `ProductName` không rỗng nếu request có field này.
- Shipping address có tối thiểu receiver name, phone, line1, city.

CancelOrder:

- `OrderId` không empty.
- `Reason` không rỗng và giới hạn độ dài hợp lý.

GetOrders:

- `pageNumber >= 1`.
- `pageSize` nằm trong khoảng `1..100`.

**Pipeline behaviors**

- `ValidationBehavior`: chạy FluentValidation trước handler.
- `LoggingBehavior`: log request name, correlation id nếu có; không log dữ liệu nhạy cảm.

**Dependency injection**

`DependencyInjection.cs` của Application đăng ký:

- MediatR.
- FluentValidation validators.
- Pipeline behaviors.

**Điều kiện pass**

- Application không reference Infrastructure/API.
- Handler không biết EF Core cụ thể.
- Handler gọi repository/unit of work qua abstraction.

---

### Bước 1.5: Implement Infrastructure persistence

**Cấu trúc thư mục**

```text
ECommerceMS.Order.Infrastructure/
├── DependencyInjection.cs
├── Persistence/
│   ├── OrderDbContext.cs
│   ├── Configurations/
│   │   ├── OrderConfiguration.cs
│   │   └── OrderItemConfiguration.cs
│   ├── Repositories/
│   │   └── OrderRepository.cs
│   └── UnitOfWork.cs
└── Migrations/
```

**OrderDbContext**

- Có `DbSet<Order>` và cấu hình `OrderItem`.
- Apply configurations bằng `ApplyConfigurationsFromAssembly`.
- Không expose transaction logic phức tạp ở giai đoạn này nếu `SaveChangesAsync` đủ dùng.

**Entity mapping mục tiêu**

Order table:

| Column | Type gợi ý | Rule |
|---|---|---|
| `Id` | uuid | PK |
| `CustomerId` | uuid | required |
| `Status` | text hoặc int | required |
| `TotalAmount` | numeric(18,2) | required |
| `Currency` | varchar(3) | required |
| `ReceiverName` | varchar(200) | required |
| `Phone` | varchar(50) | required |
| `Line1` | varchar(300) | required |
| `Line2` | varchar(300) | optional |
| `City` | varchar(100) | required |
| `District` | varchar(100) | optional |
| `Ward` | varchar(100) | optional |
| `CreatedAtUtc` | timestamptz | required |
| `UpdatedAtUtc` | timestamptz | optional |
| `CancelledAtUtc` | timestamptz | optional |
| `CancelReason` | varchar(500) | optional |

OrderItems table:

| Column | Type gợi ý | Rule |
|---|---|---|
| `Id` | uuid | PK hoặc owned id |
| `OrderId` | uuid | FK |
| `ProductId` | uuid | required |
| `ProductName` | varchar(300) | required |
| `Quantity` | int | required |
| `UnitPriceAmount` | numeric(18,2) | required |
| `UnitPriceCurrency` | varchar(3) | required |

**Repository methods**

```text
AddAsync(Order order, CancellationToken ct)
GetByIdAsync(OrderId id, CancellationToken ct)
GetPagedAsync(int pageNumber, int pageSize, CancellationToken ct)
ExistsAsync(OrderId id, CancellationToken ct)
```

**UnitOfWork**

```text
SaveChangesAsync(CancellationToken ct)
```

**Connection string key**

Dùng:

```text
ConnectionStrings__Default
```

Local development value mẫu:

```text
Host=localhost;Port=5432;Database=order_db;Username=postgres;Password=change-me-local-only
```

**Điều kiện pass**

- Migration tạo được.
- `OrderDbContext` không nằm trong Domain/Application.
- Infrastructure implement đúng interface của Application.

---

### Bước 1.6: Implement API layer

**Cấu trúc thư mục**

```text
ECommerceMS.Order.API/
├── Controllers/
│   └── OrdersController.cs
├── Contracts/
│   ├── CancelOrderRequest.cs
│   ├── CreateOrderItemRequest.cs
│   ├── CreateOrderRequest.cs
│   └── ShippingAddressRequest.cs
├── Middlewares/
│   └── ExceptionHandlingMiddleware.cs
├── Extensions/
│   └── ServiceCollectionExtensions.cs
├── appsettings.json
├── appsettings.Development.json
└── Program.cs
```

Nếu chọn Minimal API thay controller, phải giữ endpoint behavior tương đương. Khuyến nghị giai đoạn 1 dùng controller để dễ đọc và dễ mở rộng.

**Endpoints bắt buộc**

| Method | Path | Behavior |
|---|---|---|
| `POST` | `/api/orders` | Tạo order, trả `201 Created` |
| `GET` | `/api/orders/{id}` | Lấy chi tiết order, trả `200` hoặc `404` |
| `GET` | `/api/orders?pageNumber=1&pageSize=20` | Lấy danh sách phân trang |
| `PATCH` | `/api/orders/{id}/cancel` | Hủy order, trả `200` hoặc `409` nếu state không hợp lệ |
| `GET` | `/health/live` | Kiểm tra process sống |
| `GET` | `/health/ready` | Kiểm tra readiness cơ bản |

**Response rules**

- Validation lỗi trả `400`.
- Không tìm thấy order trả `404`.
- State không hợp lệ trả `409`.
- Lỗi chưa xử lý trả `500` và có log.
- Không trả stack trace cho client.

**OpenAPI/Scalar**

- Bật OpenAPI ở Development.
- Map Scalar UI để test thủ công.
- Đặt tên document rõ: `Order Service API`.

**Program.cs phải đăng ký**

- Controllers.
- Application DI.
- Infrastructure DI.
- Exception middleware.
- OpenAPI/Scalar.
- Health checks.

**Điều kiện pass**

- API start được bằng `dotnet run`.
- Scalar UI mở được.
- Endpoint order gọi được thủ công.

---

### Bước 1.7: Migration và database local

**Việc làm**

1. Chuẩn bị PostgreSQL local hoặc container.
2. Cấu hình connection string trong `appsettings.Development.json` hoặc user secrets.
3. Tạo migration ban đầu.
4. Apply migration.

**Command gợi ý**

```powershell
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate `
  --project src/Services/Order/ECommerceMS.Order.Infrastructure `
  --startup-project src/Services/Order/ECommerceMS.Order.API `
  --output-dir Persistence/Migrations

dotnet ef database update `
  --project src/Services/Order/ECommerceMS.Order.Infrastructure `
  --startup-project src/Services/Order/ECommerceMS.Order.API
```

Nếu `dotnet-ef` đã cài, dùng:

```powershell
dotnet tool update --global dotnet-ef
```

**Điều kiện pass**

- Database `order_db` có bảng orders/order_items.
- Migration nằm trong Infrastructure.
- Không commit connection string chứa password thật.

---

### Bước 1.8: Manual verification

**Build**

```powershell
dotnet restore ECommerceMS.sln
dotnet build ECommerceMS.sln
```

**Run API**

```powershell
dotnet run --project src/Services/Order/ECommerceMS.Order.API
```

**Test thủ công qua HTTP**

Create order:

```http
POST http://localhost:5001/api/orders
Content-Type: application/json

{
  "customerId": "11111111-1111-1111-1111-111111111111",
  "shippingAddress": {
    "receiverName": "Nguyen Van A",
    "phone": "0900000000",
    "line1": "123 Nguyen Trai",
    "line2": "",
    "ward": "Phuong 1",
    "district": "Quan 1",
    "city": "Ho Chi Minh"
  },
  "items": [
    {
      "productId": "22222222-2222-2222-2222-222222222222",
      "productName": "Laptop",
      "quantity": 2,
      "unitPrice": 15000000,
      "currency": "VND"
    }
  ]
}
```

Expected:

- HTTP `201`.
- Response có `id`, `status = Pending`, `totalAmount = 30000000`, `currency = VND`.

Get order:

```http
GET http://localhost:5001/api/orders/{id}
```

Expected:

- HTTP `200`.
- Response trả đúng order vừa tạo.

Cancel order:

```http
PATCH http://localhost:5001/api/orders/{id}/cancel
Content-Type: application/json

{
  "reason": "Customer requested cancellation"
}
```

Expected:

- HTTP `200`.
- `status = Cancelled`.

Validation test:

```http
POST http://localhost:5001/api/orders
Content-Type: application/json

{
  "customerId": "00000000-0000-0000-0000-000000000000",
  "items": []
}
```

Expected:

- HTTP `400`.
- Response nêu lỗi validation rõ ràng.

---

## 7. Thiết Kế API Contract Giai Đoạn 1

### CreateOrderRequest

```json
{
  "customerId": "guid",
  "shippingAddress": {
    "receiverName": "string",
    "phone": "string",
    "line1": "string",
    "line2": "string",
    "ward": "string",
    "district": "string",
    "city": "string"
  },
  "items": [
    {
      "productId": "guid",
      "productName": "string",
      "quantity": 1,
      "unitPrice": 100000,
      "currency": "VND"
    }
  ]
}
```

### CancelOrderRequest

```json
{
  "reason": "string"
}
```

### OrderDto

```json
{
  "id": "guid",
  "customerId": "guid",
  "status": "Pending",
  "totalAmount": 100000,
  "currency": "VND",
  "shippingAddress": {},
  "items": [],
  "createdAtUtc": "2026-06-23T00:00:00Z",
  "updatedAtUtc": null,
  "cancelledAtUtc": null,
  "cancelReason": null
}
```

### Paged response

Giai đoạn 1 dùng response đơn giản:

```json
{
  "items": [],
  "pageNumber": 1,
  "pageSize": 20,
  "totalCount": 0
}
```

Nếu chưa muốn query `totalCount` để giữ đơn giản, vẫn phải ghi rõ trong implementation. Khuyến nghị có `totalCount` ngay từ đầu.

---

## 8. Logging, Error Handling Và Health Check

### Logging tối thiểu

Giai đoạn 1 chưa cần Serilog/Seq đầy đủ, nhưng cần dùng `ILogger` chuẩn của ASP.NET Core:

- Log khi API nhận command quan trọng.
- Log lỗi trong exception middleware.
- Không log request body đầy đủ nếu có dữ liệu nhạy cảm.

### Exception mapping

| Exception | HTTP status |
|---|---:|
| `ValidationException` | 400 |
| `OrderNotFoundException` hoặc null result | 404 |
| `OrderInvalidStatusException` | 409 |
| `DomainException` khác | 400 hoặc 409 tùy loại |
| Unexpected exception | 500 |

### Health checks

Giai đoạn 1:

- `/health/live`: luôn pass nếu app chạy.
- `/health/ready`: kiểm tra DbContext/PostgreSQL nếu connection đã cấu hình.

---

## 9. Thứ Tự Commit Khuyến Nghị

Không bắt buộc phải commit từng bước, nhưng nên chia nhỏ:

```text
chore(solution): initialize .NET 8 ecommerce solution
feat(order-domain): add order aggregate and value objects
feat(order-application): add order CQRS handlers and validation
feat(order-infrastructure): add EF Core persistence for orders
feat(order-api): expose order endpoints
docs(order): document phase 1 verification steps
```

Nếu chỉ commit một lần cho toàn giai đoạn:

```text
feat(order): implement clean architecture order service foundation
```

---

## 10. Deliverables

Kết thúc giai đoạn 1 cần có:

- `global.json` khóa SDK .NET 8.
- `ECommerceMS.sln`.
- 4 project Order target `net8.0`.
- Domain aggregate và value objects.
- Application CQRS handlers và validators.
- Infrastructure EF Core DbContext, mappings, repository, unit of work.
- API endpoints cho Order.
- Migration `InitialCreate`.
- Scalar/OpenAPI chạy được.
- README hoặc runbook được cập nhật nếu có thay đổi cách chạy local.

---

## 11. Definition Of Done

Giai đoạn 1 hoàn thành khi:

- [ ] `dotnet --version` trong repo trả về SDK 8.x.
- [ ] `dotnet build ECommerceMS.sln` pass.
- [ ] Domain project không có dependency hạ tầng.
- [ ] Application project không reference Infrastructure/API.
- [ ] Infrastructure project chứa EF Core mapping và migration.
- [ ] API project start được.
- [ ] Scalar/OpenAPI UI hiển thị endpoint Order.
- [ ] `POST /api/orders` tạo order thành công.
- [ ] `GET /api/orders/{id}` trả order đúng.
- [ ] `GET /api/orders` trả danh sách phân trang.
- [ ] `PATCH /api/orders/{id}/cancel` hủy order đúng state.
- [ ] Validation lỗi trả HTTP 400.
- [ ] Không tìm thấy order trả HTTP 404.
- [ ] State transition không hợp lệ trả HTTP 409.
- [ ] Migration ban đầu apply được vào `order_db`.
- [ ] Không có secret thật trong config.

---

## 12. Checklist Nghiệm Thu

### Architecture

- [ ] Dependency direction đúng Clean Architecture.
- [ ] Domain model chứa business rule chính.
- [ ] Handler không chứa rule domain phức tạp đáng lẽ thuộc aggregate.
- [ ] Repository interface nằm ở Application.
- [ ] Repository implementation nằm ở Infrastructure.
- [ ] API chỉ orchestration request/response, không chứa business rule.

### Domain

- [ ] Order phải có ít nhất một item.
- [ ] Quantity phải lớn hơn 0.
- [ ] UnitPrice không âm.
- [ ] TotalAmount tính từ item.
- [ ] Cancel chỉ hợp lệ ở state cho phép.
- [ ] Domain events được add khi create/cancel.

### Application

- [ ] CreateOrder có validator.
- [ ] CancelOrder có validator.
- [ ] GetOrders có paging guard.
- [ ] ValidationBehavior được đăng ký.
- [ ] LoggingBehavior được đăng ký.
- [ ] Mapping sang `OrderDto` thống nhất.

### Infrastructure

- [ ] EF mapping không làm lộ setter public không cần thiết.
- [ ] Money/Address value object được map rõ.
- [ ] Migration nằm đúng project Infrastructure.
- [ ] Connection string lấy từ configuration.
- [ ] UnitOfWork gọi `SaveChangesAsync`.

### API

- [ ] Endpoint path khớp `Docs/api/api-guidelines.md`.
- [ ] Response status code đúng.
- [ ] Exception middleware hoạt động.
- [ ] OpenAPI/Scalar hoạt động ở Development.
- [ ] Health endpoints tồn tại.

---

## 13. Rủi Ro Và Cách Kiểm Soát

| Rủi ro | Tác động | Cách kiểm soát |
|---|---|---|
| Tạo project bằng SDK 10 | Target/framework lệch roadmap | Cài SDK 8 và tạo `global.json` trước |
| Domain phụ thuộc EF Core | Mất tính độc lập của domain | Review using/package references ở Domain |
| Handler chứa quá nhiều business rule | Domain model yếu, khó reuse | Đẩy rule vào aggregate/value object |
| Migration chứa mapping sai value object | DB schema khó sửa về sau | Review mapping trước khi apply rộng |
| Validation trùng lặp giữa API và Application | Logic rải rác | Validation chính đặt ở Application |
| Không có exception middleware | Response lỗi không nhất quán | Implement middleware trước khi test endpoint |
| Hardcode connection string | Rủi ro secret và khó deploy | Dùng configuration/user secrets/env vars |
| Không kiểm tra dependency direction | Service mẫu sai kéo theo các service sau sai | Checklist architecture bắt buộc trước khi Done |

---

## 14. Lỗi Thường Gặp Và Cách Xử Lý

### Lỗi 1: `dotnet new` dùng SDK 10

**Dấu hiệu**

- `dotnet --version` trả về `10.0.301`.
- Project file sinh ra không đúng target hoặc template behavior khác mong muốn.

**Cách xử lý**

- Cài SDK 8.
- Tạo/cập nhật `global.json`.
- Chạy lại `dotnet --version` ở root repo.

### Lỗi 2: EF Core không tạo được migration

**Dấu hiệu**

- `dotnet ef migrations add` báo không tìm thấy DbContext.
- Startup project không resolve được DI.

**Cách xử lý**

- Kiểm tra `OrderDbContext` public.
- Kiểm tra API đã đăng ký Infrastructure DI.
- Chỉ định đúng `--project` và `--startup-project`.

### Lỗi 3: Value object mapping lỗi

**Dấu hiệu**

- EF Core không map được `Money`, `Address`, `OrderId`.

**Cách xử lý**

- Dùng `OwnsOne` cho value object phức tạp.
- Dùng value converter cho strongly typed id nếu cần.
- Giữ constructor private/protected cho EF nhưng không phá invariant domain.

### Lỗi 4: API trả 500 cho validation

**Dấu hiệu**

- Request invalid không trả 400.

**Cách xử lý**

- Kiểm tra `ValidationBehavior` đã đăng ký.
- Kiểm tra validators được scan đúng assembly.
- Middleware map `FluentValidation.ValidationException` sang 400.

---

## 15. Ghi Chú Cho Giai Đoạn 2 Và 3

Để không cản các giai đoạn sau, giai đoạn 1 nên chuẩn bị:

- API endpoint path ổn định để Gateway route ở giai đoạn 2.
- Domain events nội bộ để chuyển sang integration event/outbox ở giai đoạn 3.
- Response/error format ổn định để Gateway và client dùng.
- Health endpoints để Docker/Kubernetes dùng ở giai đoạn sau.

Không implement Gateway, Kafka hoặc Outbox trong giai đoạn 1. Chỉ tạo điểm nối hợp lý để không phải refactor lớn.

