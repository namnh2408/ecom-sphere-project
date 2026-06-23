# Implementation Plan Giai Đoạn 3 - Kafka, MassTransit, Event Contracts Và Outbox

> Thuộc roadmap: `Docs/plans/plan_overral_time20260623.md`  
> Nguồn tham chiếu: `Docs/ecommerce-microservice-roadmap.md`, `Docs/architecture/naming-conventions.md`, `Docs/implements/plan_impl_giaidoan_1_20260623_1328.md`, `Docs/implements/plan_impl_giaidoan_2_20260623_1339.md`  
> Giai đoạn: 3  
> Timestamp: 20260623_1345  
> Mục tiêu thời gian: Tuần 5-6  
> Runtime mục tiêu: .NET 8 LTS  
> Trạng thái: Kế hoạch implement chi tiết, chưa thực thi code

---

## 1. Mục Tiêu Giai Đoạn 3

Giai đoạn 3 chuyển hệ thống từ request/response đơn service sang nền tảng event-driven bằng Kafka, MassTransit, shared integration contracts và Outbox pattern.

Kết thúc giai đoạn này cần có:

- Project `ECommerceMS.Shared.Contracts` chứa integration event contracts công khai.
- Kafka local chạy được.
- Order Service publish `order.created`, `order.cancelled`, `order.confirmed` thông qua Outbox.
- Outbox đảm bảo event không bị mất khi transaction database thành công.
- Consumer mẫu nhận event và log payload để chứng minh luồng publish/consume.
- Retry/error handling tối thiểu.
- Correlation id, causation id, event id, occurred time và version trong event metadata.
- Checklist kiểm thử publish/consume và idempotency.

Giai đoạn này chưa cần hoàn thiện Product, Payment, Notification business logic. Mục tiêu là dựng nền messaging đúng để giai đoạn 4 nối business service vào mà không phải thay đổi event contract lớn.

---

## 2. Phạm Vi Công Việc

### 2.1. Trong phạm vi

- Tạo `ECommerceMS.Shared.Contracts`.
- Định nghĩa base integration event contract và event metadata.
- Định nghĩa các event chính của Order, Payment, Stock.
- Cấu hình Kafka local tối thiểu.
- Cấu hình MassTransit Kafka rider trong Order Service.
- Implement Outbox Pattern trong Order Infrastructure.
- Publish integration event từ domain event hoặc application handler sau khi lưu transaction.
- Tạo consumer mẫu cho ít nhất `order.created`.
- Bổ sung idempotency store tối thiểu cho consumer mẫu hoặc thiết kế rõ ràng.
- Bổ sung retry/error topic hoặc error handling policy cơ bản.
- Viết manual verification và integration-test plan.

### 2.2. Ngoài phạm vi

- Không implement Product Service đầy đủ.
- Không implement Payment Service đầy đủ.
- Không implement Notification Service đầy đủ.
- Không xử lý Saga orchestration hoàn chỉnh.
- Không implement Redis lock, Elasticsearch, SignalR, Hangfire.
- Không cần Docker Compose full stack production-like.
- Không cần OpenTelemetry tracing đầy đủ, nhưng phải chuẩn bị correlation id.

---

## 3. Điều Kiện Đầu Vào

Giai đoạn 3 phụ thuộc vào giai đoạn 1 và một phần giai đoạn 2.

### 3.1. Bắt buộc

- `ECommerceMS.sln` tồn tại.
- Order Service đã có Domain/Application/Infrastructure/API.
- Order Service đã có EF Core `OrderDbContext` và PostgreSQL migration.
- CreateOrder/CancelOrder hoạt động.
- Domain events nội bộ đã tồn tại hoặc có thể bổ sung.
- `.NET SDK 8.x` đã sẵn sàng.

### 3.2. Nên có

- Gateway/Auth từ giai đoạn 2 đã chạy để tạo order qua Gateway.
- Docker Desktop chạy được.
- Kafka local chạy được bằng container.
- PostgreSQL local chạy được.

### 3.3. Kiểm tra trước khi bắt đầu

```powershell
dotnet --version
dotnet build ECommerceMS.sln
docker --version
docker compose version
```

Điều kiện pass:

- `dotnet build` pass trước khi thêm messaging.
- Docker CLI hoạt động.
- Không có migration pending ngoài phạm vi giai đoạn 3.

---

## 4. Kiến Trúc Messaging Mục Tiêu

### 4.1. Luồng publish chính

```text
Client/Gateway
  -> Order API
  -> Application Handler
  -> Order Aggregate
  -> Domain Event
  -> Save Order + Outbox Message trong cùng DB transaction
  -> Outbox Processor/MassTransit Bus Outbox
  -> Kafka topic
  -> Consumer mẫu
```

### 4.2. Quy tắc bắt buộc

- Không publish trực tiếp ra Kafka trước khi database transaction commit.
- Integration event contract nằm trong `ECommerceMS.Shared.Contracts`.
- Domain event nội bộ không đồng nghĩa với integration event.
- Mapping domain event -> integration event nằm ở Application hoặc Infrastructure boundary, không nằm trong Domain.
- Consumer phải idempotent hoặc ít nhất có thiết kế idempotency rõ ràng.
- Event phải có metadata để trace và debug.

### 4.3. Ranh giới Domain Event và Integration Event

| Loại | Vị trí | Mục đích | Ví dụ |
|---|---|---|---|
| Domain Event | `ECommerceMS.Order.Domain` | Diễn tả việc xảy ra trong aggregate | `OrderCreatedDomainEvent` |
| Integration Event | `ECommerceMS.Shared.Contracts` | Giao tiếp liên service qua Kafka | `OrderCreatedIntegrationEvent` |

Domain event có thể chứa object/value object nội bộ. Integration event phải ổn định, serialize tốt và không expose cấu trúc domain nội bộ không cần thiết.

---

## 5. Project Và Packages

### 5.1. Shared Contracts

Tạo project:

```powershell
dotnet new classlib -n ECommerceMS.Shared.Contracts -o src/Shared/ECommerceMS.Shared.Contracts --framework net8.0
dotnet sln ECommerceMS.sln add src/Shared/ECommerceMS.Shared.Contracts/ECommerceMS.Shared.Contracts.csproj
```

Package:

- Không cần package ngoài nếu chỉ dùng records.
- Có thể dùng `System.Text.Json` built-in.

### 5.2. Order Infrastructure

Packages:

```powershell
dotnet add src/Services/Order/ECommerceMS.Order.Infrastructure package MassTransit --version 8.*
dotnet add src/Services/Order/ECommerceMS.Order.Infrastructure package MassTransit.Kafka --version 8.*
dotnet add src/Services/Order/ECommerceMS.Order.Infrastructure package MassTransit.EntityFrameworkCore --version 8.*
```

Nếu dùng Newtonsoft cho serialize đặc biệt thì cần quyết định riêng. Khuyến nghị dùng System.Text.Json mặc định.

### 5.3. Order API

Nếu MassTransit registration nằm ở Infrastructure extension nhưng được gọi từ API, API có thể không cần package MassTransit trực tiếp. Chỉ cài package ở API nếu compiler/DI yêu cầu.

### 5.4. Test project nếu có

Nếu giai đoạn này tạo integration test:

```powershell
dotnet new xunit -n ECommerceMS.Order.MessagingTests -o tests/Integration/ECommerceMS.Order.MessagingTests --framework net8.0
dotnet sln ECommerceMS.sln add tests/Integration/ECommerceMS.Order.MessagingTests/ECommerceMS.Order.MessagingTests.csproj
dotnet add tests/Integration/ECommerceMS.Order.MessagingTests package Testcontainers.Kafka
dotnet add tests/Integration/ECommerceMS.Order.MessagingTests package FluentAssertions
```

Integration test có thể để giai đoạn 11 nếu thời lượng giai đoạn 3 không đủ. Tuy nhiên phải có manual verification rõ.

---

## 6. Event Contracts

### 6.1. Cấu trúc thư mục Shared Contracts

```text
ECommerceMS.Shared.Contracts/
├── Abstractions/
│   ├── IIntegrationEvent.cs
│   └── IntegrationEvent.cs
├── Orders/
│   ├── OrderCancelledIntegrationEvent.cs
│   ├── OrderConfirmedIntegrationEvent.cs
│   ├── OrderCreatedIntegrationEvent.cs
│   └── OrderItemIntegrationDto.cs
├── Payments/
│   ├── PaymentCompletedIntegrationEvent.cs
│   └── PaymentFailedIntegrationEvent.cs
└── Stocks/
    ├── StockInsufficientIntegrationEvent.cs
    ├── StockReservedIntegrationEvent.cs
    └── ReservedItemIntegrationDto.cs
```

### 6.2. Base event metadata

Tất cả integration events phải có:

```text
EventId: Guid
EventType: string
Version: int
OccurredAtUtc: DateTime
CorrelationId: string?
CausationId: string?
Source: string
```

Khuyến nghị:

```csharp
public interface IIntegrationEvent
{
    Guid EventId { get; }
    string EventType { get; }
    int Version { get; }
    DateTime OccurredAtUtc { get; }
    string? CorrelationId { get; }
    string? CausationId { get; }
    string Source { get; }
}
```

### 6.3. OrderCreatedIntegrationEvent

Fields:

```text
OrderId: Guid
CustomerId: Guid
Items: IReadOnlyCollection<OrderItemIntegrationDto>
TotalAmount: decimal
Currency: string
ShippingCity: string
CreatedAtUtc: DateTime
```

Không đưa toàn bộ address chi tiết nếu consumer chưa cần. Nếu Notification cần address sau này, cân nhắc thêm field ở version mới hoặc thêm event riêng.

### 6.4. OrderCancelledIntegrationEvent

Fields:

```text
OrderId: Guid
CustomerId: Guid
Reason: string
CancelledAtUtc: DateTime
```

### 6.5. OrderConfirmedIntegrationEvent

Fields:

```text
OrderId: Guid
CustomerId: Guid
ConfirmedAtUtc: DateTime
```

### 6.6. Payment events

Payment events có thể được định nghĩa trong Shared Contracts ở giai đoạn 3 để Order consumer dùng sau này, nhưng chưa cần Payment Service thật.

`PaymentCompletedIntegrationEvent`:

```text
PaymentId: Guid
OrderId: Guid
Amount: decimal
Currency: string
Method: string
ProcessedAtUtc: DateTime
```

`PaymentFailedIntegrationEvent`:

```text
PaymentId: Guid
OrderId: Guid
Reason: string
FailedAtUtc: DateTime
```

### 6.7. Stock events

`StockReservedIntegrationEvent`:

```text
OrderId: Guid
Items: IReadOnlyCollection<ReservedItemIntegrationDto>
ReservedAtUtc: DateTime
```

`StockInsufficientIntegrationEvent`:

```text
OrderId: Guid
ProductId: Guid
RequestedQuantity: int
AvailableQuantity: int
Reason: string
OccurredAtUtc: DateTime
```

---

## 7. Kafka Topic Matrix

| Topic | Publisher | Consumers hiện tại | Consumers tương lai | Event |
|---|---|---|---|---|
| `order.created` | Order Service | Consumer mẫu | Product, Payment, Notification | `OrderCreatedIntegrationEvent` |
| `order.confirmed` | Order Service | Consumer mẫu hoặc none | Notification, SignalR | `OrderConfirmedIntegrationEvent` |
| `order.cancelled` | Order Service | Consumer mẫu hoặc none | Product, Payment, Notification | `OrderCancelledIntegrationEvent` |
| `payment.completed` | Payment Service tương lai | Order Service consumer mẫu nếu cần | Notification | `PaymentCompletedIntegrationEvent` |
| `payment.failed` | Payment Service tương lai | Order Service consumer mẫu nếu cần | Notification | `PaymentFailedIntegrationEvent` |
| `stock.reserved` | Product Service tương lai | Order Service consumer mẫu nếu cần | None | `StockReservedIntegrationEvent` |
| `stock.insufficient` | Product Service tương lai | Order Service consumer mẫu nếu cần | Notification | `StockInsufficientIntegrationEvent` |

Giai đoạn 3 bắt buộc publish được `order.created`. Các topic còn lại có thể tạo producer/contract sẵn và consumer mẫu tùy thời lượng.

---

## 8. Kafka Local Setup

### 8.1. Cách chạy tối thiểu

Khuyến nghị tạo compose tối thiểu:

```text
infra/docker/kafka/docker-compose.kafka.yml
```

Nội dung mục tiêu:

- Kafka `bitnami/kafka:3.7` hoặc image ổn định tương đương.
- Kafka UI tùy chọn.
- Expose host port `9092`.
- Internal listener dùng được cho app local.

Nếu chưa muốn tạo compose file ở giai đoạn 3, có thể dùng docker run, nhưng compose dễ lặp lại hơn.

### 8.2. Environment variables

Bổ sung vào `.env.example` nếu chưa có:

```text
KAFKA_BOOTSTRAP_SERVERS=localhost:9092
KAFKA_CLIENT_ID=ecommerce-local
KAFKA_CONSUMER_GROUP_ORDER=order-service-group
KAFKA_CONSUMER_GROUP_DEBUG=debug-consumer-group
```

### 8.3. Kiểm tra Kafka

```powershell
docker ps
docker logs <kafka-container>
```

Nếu có Kafka UI:

```text
http://localhost:8085
```

---

## 9. Outbox Design

### 9.1. Lựa chọn triển khai

Khuyến nghị dùng MassTransit EntityFramework Outbox để giảm code tự viết:

- Add MassTransit EF Core outbox vào Order Service.
- Outbox tables nằm trong `order_db`.
- Publish integration event thông qua MassTransit bus/outbox trong cùng request flow.

Nếu muốn tự viết outbox, cần thêm:

- `OutboxMessages` table.
- Background worker poll outbox.
- Mark processed/error.
- Retry policy riêng.

Khuyến nghị giai đoạn 3: dùng MassTransit EF Core Outbox.

### 9.2. Tables cần có

MassTransit EF Outbox thường thêm:

```text
OutboxMessage
OutboxState
InboxState
```

Tên table thực tế theo MassTransit conventions. Migration phải review để đảm bảo nằm trong `order_db`.

### 9.3. Transaction flow

```text
CreateOrderCommandHandler
  -> Order.Create()
  -> repository.Add(order)
  -> map domain event to integration event
  -> publish integration event through MassTransit outbox
  -> unitOfWork.SaveChangesAsync()
  -> DB commit order + outbox
  -> Outbox dispatch to Kafka
```

Điểm cần kiểm soát:

- Nếu `SaveChangesAsync` fail, không có event ra Kafka.
- Nếu Kafka down nhưng DB commit thành công, outbox giữ message để retry.
- Nếu consumer xử lý trùng, consumer không tạo side effect trùng.

### 9.4. Mapping domain event -> integration event

Tạo mapper ở Application hoặc Infrastructure:

```text
OrderCreatedDomainEvent -> OrderCreatedIntegrationEvent
OrderCancelledDomainEvent -> OrderCancelledIntegrationEvent
OrderConfirmedDomainEvent -> OrderConfirmedIntegrationEvent
```

Không để Domain project reference Shared.Contracts nếu muốn giữ domain thuần. Khuyến nghị mapping nằm ngoài Domain.

---

## 10. MassTransit Configuration

### 10.1. Cấu trúc thư mục Infrastructure

```text
ECommerceMS.Order.Infrastructure/
├── Messaging/
│   ├── Consumers/
│   │   ├── DebugOrderCreatedConsumer.cs
│   │   ├── PaymentCompletedConsumer.cs
│   │   └── StockReservedConsumer.cs
│   ├── Mapping/
│   │   └── IntegrationEventMapper.cs
│   └── MessagingOptions.cs
├── Persistence/
│   └── OrderDbContext.cs
└── DependencyInjection.cs
```

### 10.2. MessagingOptions

```text
Kafka:BootstrapServers
Kafka:ClientId
Kafka:ConsumerGroup
Kafka:CreateTopicsIfMissing
```

Config mẫu:

```json
{
  "Kafka": {
    "BootstrapServers": "localhost:9092",
    "ClientId": "order-service",
    "ConsumerGroup": "order-service-group",
    "CreateTopicsIfMissing": true
  }
}
```

### 10.3. Producer config

Order Service producers:

```text
order.created
order.cancelled
order.confirmed
```

### 10.4. Consumer config

Giai đoạn 3 consumer mẫu:

```text
DebugOrderCreatedConsumer
```

Optional consumer chuẩn bị cho giai đoạn 4:

```text
PaymentCompletedConsumer
PaymentFailedConsumer
StockReservedConsumer
StockInsufficientConsumer
```

Nếu implement optional consumers, chỉ log và cập nhật state khi logic đã rõ. Không giả lập nghiệp vụ phức tạp của giai đoạn 4.

### 10.5. Retry/error policy

Mục tiêu tối thiểu:

- Retry 3 lần.
- Delay tăng dần hoặc interval ngắn.
- Message lỗi được đưa vào error transport/topic theo MassTransit convention.
- Log event id, topic, consumer name, exception.

---

## 11. Idempotency Design

### 11.1. Vì sao cần idempotency

Kafka/MassTransit có thể deliver message nhiều hơn một lần. Consumer không được tạo side effect trùng nếu nhận lại cùng `EventId`.

### 11.2. Cách làm tối thiểu

Tạo table:

```text
ProcessedMessages
```

Columns:

| Column | Rule |
|---|---|
| `EventId` | PK hoặc unique |
| `ConsumerName` | unique cùng EventId nếu nhiều consumer |
| `ProcessedAtUtc` | required |
| `EventType` | required |

Unique key:

```text
(EventId, ConsumerName)
```

### 11.3. Consumer flow

```text
Receive message
  -> Check ProcessedMessages(EventId, ConsumerName)
  -> If exists: log duplicate and return success
  -> Execute side effect
  -> Insert ProcessedMessages
  -> Commit
```

Giai đoạn 3 nếu consumer chỉ log, vẫn nên tạo hoặc document idempotency mechanism để giai đoạn 4 dùng ngay.

---

## 12. Correlation Và Causation

### 12.1. Correlation id

Nguồn correlation id:

1. Header `X-Correlation-Id` từ Gateway nếu có.
2. Trace identifier của ASP.NET Core nếu không có header.
3. New GUID nếu không có context.

### 12.2. Causation id

`CausationId` dùng để liên kết event với command/request trước đó:

- Có thể dùng command id nếu command có id.
- Có thể dùng request id/correlation id ở giai đoạn đầu.

### 12.3. Kafka headers

Nếu dễ implement, đưa metadata vào Kafka headers:

```text
x-correlation-id
x-causation-id
x-event-id
x-event-type
x-event-version
```

Payload vẫn phải chứa metadata để consumer không phụ thuộc hoàn toàn vào header.

---

## 13. Các Bước Implement Chi Tiết

### Bước 3.1: Tạo Shared Contracts project

**Việc làm**

- Tạo `ECommerceMS.Shared.Contracts`.
- Add vào solution.
- Xóa `.gitkeep` trong `src/Shared` nếu cần.
- Thêm project reference từ Order Application/Infrastructure đến Shared.Contracts nếu cần mapping/publish.

**Command gợi ý**

```powershell
dotnet new classlib -n ECommerceMS.Shared.Contracts -o src/Shared/ECommerceMS.Shared.Contracts --framework net8.0
dotnet sln ECommerceMS.sln add src/Shared/ECommerceMS.Shared.Contracts/ECommerceMS.Shared.Contracts.csproj
dotnet add src/Services/Order/ECommerceMS.Order.Infrastructure reference src/Shared/ECommerceMS.Shared.Contracts
```

Nếu mapping nằm ở Application:

```powershell
dotnet add src/Services/Order/ECommerceMS.Order.Application reference src/Shared/ECommerceMS.Shared.Contracts
```

Khuyến nghị mapping integration event ở Infrastructure hoặc Application service tùy code hiện tại. Không reference Shared.Contracts từ Domain.

**Điều kiện pass**

- Shared.Contracts không phụ thuộc service project.
- Domain không reference Shared.Contracts.

---

### Bước 3.2: Định nghĩa event contracts

**Việc làm**

- Tạo base `IIntegrationEvent`.
- Tạo Order events.
- Tạo Payment/Stock events để chuẩn bị consumers.
- Đảm bảo tất cả event serialize được bằng System.Text.Json.

**Điều kiện pass**

- Event contract không chứa domain entity.
- Event field dùng primitive/DTO ổn định.
- Có `Version`.

---

### Bước 3.3: Chạy Kafka local

**Việc làm**

- Tạo compose hoặc docker run cho Kafka.
- Đảm bảo `localhost:9092` dùng được từ app local.
- Nếu có Kafka UI, xác nhận topic xuất hiện.

**Điều kiện pass**

- Kafka container healthy/running.
- App có thể connect tới `localhost:9092`.

---

### Bước 3.4: Cài và cấu hình MassTransit trong Order

**Việc làm**

- Cài packages MassTransit.
- Tạo `MessagingOptions`.
- Add MassTransit + Kafka rider.
- Add EntityFramework Outbox với `OrderDbContext`.
- Add producer cho Order events.

**Điều kiện pass**

- `dotnet build` pass.
- App start được khi Kafka chạy.
- App không crash ngay nếu Kafka tạm chưa ready, hoặc lỗi được log rõ.

---

### Bước 3.5: Thêm Outbox vào DbContext và migration

**Việc làm**

- Update `OrderDbContext` để include MassTransit outbox entities.
- Tạo migration `AddOrderOutbox`.
- Apply migration vào `order_db`.

**Command gợi ý**

```powershell
dotnet ef migrations add AddOrderOutbox `
  --project src/Services/Order/ECommerceMS.Order.Infrastructure `
  --startup-project src/Services/Order/ECommerceMS.Order.API `
  --output-dir Persistence/Migrations

dotnet ef database update `
  --project src/Services/Order/ECommerceMS.Order.Infrastructure `
  --startup-project src/Services/Order/ECommerceMS.Order.API
```

**Điều kiện pass**

- Outbox tables tồn tại.
- Existing order tables không bị drop/recreate ngoài ý muốn.

---

### Bước 3.6: Publish Order integration events

**Việc làm**

- Từ CreateOrder flow, publish `OrderCreatedIntegrationEvent`.
- Từ CancelOrder flow, publish `OrderCancelledIntegrationEvent`.
- Nếu ConfirmOrder đã có, publish `OrderConfirmedIntegrationEvent`.
- Metadata phải có correlation id.

**Điều kiện pass**

- Tạo order thành công -> có message `order.created`.
- Hủy order thành công -> có message `order.cancelled`.
- Nếu transaction fail -> không có message.

---

### Bước 3.7: Tạo consumer mẫu

**Việc làm**

- Tạo `DebugOrderCreatedConsumer`.
- Subscribe topic `order.created`.
- Log event metadata và payload chính.
- Không tạo side effect business.

**Điều kiện pass**

- Consumer nhận được event khi tạo order.
- Log có `EventId`, `CorrelationId`, `OrderId`.

---

### Bước 3.8: Idempotency tối thiểu

**Việc làm**

- Tạo `ProcessedMessages` table hoặc document cơ chế idempotency nếu chưa implement được.
- Nếu implement table, consumer check trước khi xử lý.
- Tạo migration `AddProcessedMessages`.

**Điều kiện pass**

- Nhận cùng event id lần 2 không xử lý side effect lần 2.
- Duplicate được log ở mức Information.

---

### Bước 3.9: Retry/error handling

**Việc làm**

- Cấu hình retry 3 lần.
- Tạo consumer lỗi giả lập trong Development hoặc test để xác nhận retry.
- Kiểm tra MassTransit error behavior.

**Điều kiện pass**

- Exception trong consumer không làm app crash.
- Message lỗi có thể quan sát trong log/error topic.

---

### Bước 3.10: Tài liệu hóa messaging

**Việc làm**

Tạo hoặc cập nhật:

```text
Docs/architecture/event-contracts.md
Docs/runbooks/kafka-local.md
```

Nội dung cần có:

- Topic matrix.
- Event schema.
- Cách chạy Kafka local.
- Cách xem topic/message.
- Cách debug outbox pending messages.

---

## 14. Manual Verification

### 14.1. Start dependencies

```powershell
docker compose -f infra/docker/kafka/docker-compose.kafka.yml up -d
```

Hoặc command docker run tương ứng nếu chưa có compose.

### 14.2. Start Order API

```powershell
dotnet run --project src/Services/Order/ECommerceMS.Order.API
```

### 14.3. Create order

Gọi qua Gateway nếu giai đoạn 2 đã xong:

```http
POST http://localhost:5000/api/orders
Authorization: Bearer {access_token}
Content-Type: application/json
```

Hoặc gọi trực tiếp Order API khi debug:

```http
POST http://localhost:5001/api/orders
Content-Type: application/json
```

Expected:

- Order tạo thành công.
- Outbox message được tạo.
- Message được publish tới `order.created`.
- Consumer mẫu log nhận message.

### 14.4. Cancel order

```http
PATCH http://localhost:5000/api/orders/{id}/cancel
Authorization: Bearer {access_token}
Content-Type: application/json

{
  "reason": "Customer requested cancellation"
}
```

Expected:

- Order chuyển sang `Cancelled`.
- Event `order.cancelled` được publish nếu producer/consumer đã bật.

### 14.5. Kafka down scenario

1. Stop Kafka.
2. Gọi CreateOrder.
3. Xác nhận database transaction thành công.
4. Xác nhận message nằm pending trong outbox.
5. Start Kafka.
6. Xác nhận outbox dispatch message.

Expected:

- Không mất event.
- App log retry/dispatch rõ ràng.

### 14.6. Duplicate event scenario

Nếu có cách replay/gửi lại message:

- Gửi lại cùng `EventId`.
- Consumer phát hiện duplicate.
- Không xử lý side effect lần 2.

---

## 15. Integration Test Plan

Nếu có thời gian trong giai đoạn 3, tạo test với Testcontainers.

### 15.1. Test publish `order.created`

Setup:

- PostgreSQL container.
- Kafka container.
- Order API test host.

Steps:

1. Start containers.
2. Run migration.
3. POST `/api/orders`.
4. Consume topic `order.created`.
5. Assert event payload.

Assertions:

- `OrderId` khớp order tạo.
- `CustomerId` khớp request.
- `TotalAmount` đúng.
- `EventId` không empty.
- `Version = 1`.
- `OccurredAtUtc` có giá trị UTC.

### 15.2. Test no event if transaction fails

Steps:

1. Gửi request invalid hoặc force DB failure trong test.
2. Assert không có message mới trên topic.

### 15.3. Test consumer idempotency

Steps:

1. Publish cùng event 2 lần.
2. Consumer xử lý lần đầu.
3. Lần hai được skip.

Assertions:

- `ProcessedMessages` chỉ có một record cho `(EventId, ConsumerName)`.

---

## 16. Deliverables

Kết thúc giai đoạn 3 cần có:

- `ECommerceMS.Shared.Contracts`.
- Event contracts cho Order, Payment, Stock.
- Kafka local setup hoặc runbook rõ ràng.
- MassTransit Kafka configuration trong Order Service.
- EntityFramework Outbox trong Order Service.
- Migration thêm outbox tables.
- Producer cho `order.created` tối thiểu.
- Consumer mẫu nhận `order.created`.
- Retry/error handling cơ bản.
- Correlation id/cause metadata trong event.
- Tài liệu topic matrix và cách debug Kafka local.

---

## 17. Definition Of Done

Giai đoạn 3 hoàn thành khi:

- [ ] `dotnet build ECommerceMS.sln` pass.
- [ ] `ECommerceMS.Shared.Contracts` tồn tại và nằm trong solution.
- [ ] Domain không reference Shared.Contracts.
- [ ] Kafka chạy được local.
- [ ] Order Service connect được Kafka.
- [ ] Outbox tables tồn tại trong `order_db`.
- [ ] CreateOrder tạo order và publish `order.created`.
- [ ] Kafka down không làm mất event đã commit DB.
- [ ] Consumer mẫu nhận được `order.created`.
- [ ] Event có `EventId`, `Version`, `OccurredAtUtc`, `CorrelationId`, `Source`.
- [ ] Consumer có idempotency hoặc cơ chế idempotency đã implement tối thiểu.
- [ ] Retry/error handling được cấu hình.
- [ ] Topic naming khớp convention.
- [ ] Không có business logic của Product/Payment/Notification bị nhét vào Order Service.

---

## 18. Checklist Nghiệm Thu

### Contracts

- [ ] Event contract dùng record/DTO ổn định.
- [ ] Không expose domain entity nội bộ.
- [ ] Có version.
- [ ] Có metadata.
- [ ] Namespace rõ theo bounded context.

### Kafka/MassTransit

- [ ] Bootstrap server lấy từ config.
- [ ] Producer topic đúng.
- [ ] Consumer group rõ ràng.
- [ ] Retry policy có giới hạn.
- [ ] Error behavior quan sát được.

### Outbox

- [ ] Event và order lưu trong cùng transaction.
- [ ] Kafka down không làm fail transaction nếu outbox đã ghi được.
- [ ] Pending outbox được dispatch khi Kafka trở lại.
- [ ] Migration không phá schema hiện có.

### Idempotency

- [ ] Consumer kiểm tra `EventId`.
- [ ] Duplicate không tạo side effect trùng.
- [ ] Có log khi skip duplicate.

### Documentation

- [ ] Có topic matrix.
- [ ] Có cách chạy Kafka local.
- [ ] Có cách kiểm tra message.
- [ ] Có cách debug outbox pending/error.

---

## 19. Rủi Ro Và Cách Kiểm Soát

| Rủi ro | Tác động | Cách kiểm soát |
|---|---|---|
| Publish trực tiếp trước DB commit | Event sai/lạc khi transaction fail | Bắt buộc dùng Outbox |
| Domain reference Shared.Contracts | Domain bị coupling với integration layer | Mapping domain -> integration ngoài Domain |
| Event contract chứa entity nội bộ | Consumer bị phụ thuộc implementation | Dùng DTO/event riêng, version rõ |
| Kafka config hardcode | Khó chạy môi trường khác | Dùng `Kafka:*` config/env vars |
| Consumer không idempotent | Xử lý trùng khi retry/replay | `ProcessedMessages` hoặc unique event handling |
| Retry vô hạn | Làm nghẽn consumer | Retry hữu hạn + error handling |
| Topic đặt theo consumer | Khó thêm consumer mới | Topic đặt theo event đã xảy ra |
| Thiếu correlation id | Khó debug flow liên service | Bắt buộc metadata và header |

---

## 20. Lỗi Thường Gặp Và Cách Xử Lý

### Lỗi 1: App không connect được Kafka

**Dấu hiệu**

- Log báo broker unavailable.
- Producer không publish được.

**Cách xử lý**

- Kiểm tra Kafka container đang chạy.
- Kiểm tra listener config.
- Kiểm tra `Kafka:BootstrapServers`.

### Lỗi 2: Có order nhưng không có message

**Dấu hiệu**

- Order lưu DB thành công.
- Topic không có event.

**Cách xử lý**

- Kiểm tra outbox tables có pending message không.
- Kiểm tra MassTransit outbox hosted service.
- Kiểm tra producer topic config.

### Lỗi 3: Consumer nhận message nhiều lần

**Dấu hiệu**

- Log cùng `EventId` nhiều lần.

**Cách xử lý**

- Kiểm tra retry do exception.
- Implement idempotency.
- Kiểm tra consumer commit offset.

### Lỗi 4: Deserialize event fail

**Dấu hiệu**

- Consumer lỗi khi đọc payload.

**Cách xử lý**

- Kiểm tra namespace/type mapping MassTransit.
- Không rename event contract tùy tiện.
- Giữ backward-compatible fields.

### Lỗi 5: Migration outbox phá schema

**Dấu hiệu**

- Migration drop/change bảng Order ngoài ý muốn.

**Cách xử lý**

- Review migration trước khi apply.
- Không apply migration nếu có operation nguy hiểm không giải thích được.

---

## 21. Thứ Tự Commit Khuyến Nghị

```text
feat(contracts): add integration event contracts
infra(kafka): add local kafka setup
feat(order-messaging): configure masstransit kafka
feat(order-outbox): add ef core outbox
feat(order-events): publish order integration events
feat(order-consumers): add debug order event consumer
docs(events): document kafka topics and outbox flow
```

Nếu commit một lần:

```text
feat(messaging): add kafka event contracts and order outbox
```

---

## 22. Ghi Chú Cho Giai Đoạn 4

Giai đoạn 3 phải để lại nền cho giai đoạn 4:

- Product Service sẽ consume `order.created` để reserve stock.
- Payment Service sẽ consume `order.created` hoặc command/event phù hợp để khởi tạo checkout giả lập.
- Notification Service sẽ consume `order.created`, `order.cancelled`, `payment.completed`, `payment.failed`.
- Order Service sẽ consume `stock.reserved`, `stock.insufficient`, `payment.completed`, `payment.failed` để cập nhật state.

Không implement đầy đủ các service này ở giai đoạn 3. Chỉ chuẩn bị event contracts, topic naming, consumer pattern và outbox để giai đoạn 4 nối vào nhất quán.

