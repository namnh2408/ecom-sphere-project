# Implementation Plan Giai Đoạn 4 - Product, Payment, Notification Service Theo Luồng Nghiệp Vụ

> Thuộc roadmap: `Docs/plans/plan_overral_time20260623.md`  
> Nguồn tham chiếu: `Docs/ecommerce-microservice-roadmap.md`, `Docs/microservice_full_architecture.svg`, `Docs/implements/plan_impl_giaidoan_1_20260623_1328.md`, `Docs/implements/plan_impl_giaidoan_2_20260623_1339.md`, `Docs/implements/plan_impl_giaidoan_3_20260623_1345.md`  
> Giai đoạn: 4  
> Timestamp: 20260623_1350  
> Mục tiêu thời gian: Tuần 7-8  
> Runtime mục tiêu: .NET 8 LTS  
> Trạng thái: Kế hoạch implement chi tiết, chưa thực thi code

---

## 1. Mục Tiêu Giai Đoạn 4

Giai đoạn 4 hoàn thiện các business service chính để hệ thống có một luồng đặt hàng cơ bản chạy được từ đầu đến cuối:

```text
Customer tạo Order
  -> Order Service publish order.created
  -> Product Service reserve stock
  -> Product Service publish stock.reserved hoặc stock.insufficient
  -> Payment Service xử lý thanh toán giả lập
  -> Payment Service publish payment.completed hoặc payment.failed
  -> Order Service cập nhật trạng thái Order
  -> Notification Service ghi nhận và gửi thông báo giả lập
```

Kết thúc giai đoạn này cần có:

- Product Service theo Clean Architecture, có catalog tối thiểu và stock reservation.
- Payment Service theo Clean Architecture, có payment record và provider giả lập.
- Notification Service theo Clean Architecture, có notification history và sender giả lập.
- Các service consume/publish event thông qua Kafka và MassTransit.
- Order Service phản ứng với event từ Product và Payment để cập nhật trạng thái.
- API Gateway route được đến Product, Payment, Notification.
- Luồng nghiệp vụ chính có thể kiểm thử thủ công qua Gateway và log/event.
- Tài liệu hóa business flow, event flow và trạng thái đơn hàng.

Mục tiêu không phải xây đủ mọi tính năng thương mại điện tử, mà là tạo xương sống nghiệp vụ có thể mở rộng ở các giai đoạn Redis, Elasticsearch, SignalR, Hangfire, Observability và Deployment.

---

## 2. Phạm Vi Công Việc

### 2.1. Trong phạm vi

- Dựng project Product Service:
  - `ECommerceMS.Product.Domain`
  - `ECommerceMS.Product.Application`
  - `ECommerceMS.Product.Infrastructure`
  - `ECommerceMS.Product.API`
- Dựng project Payment Service:
  - `ECommerceMS.Payment.Domain`
  - `ECommerceMS.Payment.Application`
  - `ECommerceMS.Payment.Infrastructure`
  - `ECommerceMS.Payment.API`
- Dựng project Notification Service:
  - `ECommerceMS.Notification.Domain`
  - `ECommerceMS.Notification.Application`
  - `ECommerceMS.Notification.Infrastructure`
  - `ECommerceMS.Notification.API`
- Thiết kế database per service.
- Implement API tối thiểu cho từng service.
- Consume event từ giai đoạn 3.
- Publish event nghiệp vụ tiếp theo.
- Cập nhật Order Service để consume stock/payment events.
- Bổ sung idempotency cho consumer quan trọng.
- Bổ sung route Gateway cho các service mới.
- Viết unit test và integration/manual test plan ở mức cần thiết.

### 2.2. Ngoài phạm vi

- Chưa cần Redis cache, cart/session hoặc distributed lock.
- Chưa cần Elasticsearch indexing/search nâng cao.
- Chưa cần SignalR real-time notification.
- Chưa cần Hangfire background job.
- Chưa cần provider thanh toán thật.
- Chưa cần gửi email/SMS thật.
- Chưa cần Saga orchestration hoàn chỉnh bằng state machine.
- Chưa cần Kubernetes manifest cho các service mới.
- Chưa cần dashboard observability đầy đủ, nhưng log phải đủ correlation id để debug.

---

## 3. Điều Kiện Đầu Vào

### 3.1. Bắt buộc

- `.NET SDK 8.x` sẵn sàng.
- `ECommerceMS.sln` đã tồn tại.
- Order Service từ giai đoạn 1 đã chạy được.
- API Gateway và Keycloak từ giai đoạn 2 đã có cấu hình cơ bản.
- Kafka, MassTransit, Shared.Contracts và Outbox từ giai đoạn 3 đã có nền tảng.
- Các integration events tối thiểu đã được định nghĩa:
  - `OrderCreatedIntegrationEvent`
  - `OrderCancelledIntegrationEvent`
  - `StockReservedIntegrationEvent`
  - `StockInsufficientIntegrationEvent`
  - `PaymentCompletedIntegrationEvent`
  - `PaymentFailedIntegrationEvent`

### 3.2. Nên có

- Docker Desktop chạy được Kafka và PostgreSQL local.
- Mỗi service có connection string riêng trong `appsettings.Development.json` hoặc user secrets.
- Gateway gọi được Order Service qua route chuẩn.
- Logging đã có correlation id cơ bản.

### 3.3. Lệnh kiểm tra trước khi bắt đầu

```powershell
dotnet --version
dotnet build ECommerceMS.sln
docker --version
docker compose version
```

Điều kiện pass:

- `dotnet build ECommerceMS.sln` pass trước khi thêm service mới.
- Kafka local có thể start được.
- PostgreSQL local có thể tạo database mới.
- Không có migration pending ngoài phạm vi đang xử lý.

---

## 4. Kiến Trúc Nghiệp Vụ Mục Tiêu

### 4.1. Service ownership

| Service | Sở hữu dữ liệu | Trách nhiệm chính | Không được làm |
|---|---|---|---|
| Order | Order, OrderItem, Order status | Nhận yêu cầu đặt hàng, quản lý trạng thái đơn hàng | Không trực tiếp trừ stock, không trực tiếp xác nhận payment thật |
| Product | Product, Category, Inventory, StockReservation | Quản lý catalog, tồn kho và giữ hàng | Không cập nhật order DB |
| Payment | Payment, PaymentAttempt, PaymentStatus | Ghi nhận và xử lý thanh toán giả lập | Không cập nhật order DB trực tiếp |
| Notification | Notification history, template tối thiểu | Nhận event và gửi thông báo giả lập | Không quyết định nghiệp vụ order/payment |
| Gateway | Route, auth, rate limit | Entry point duy nhất cho client | Không chứa business logic |

### 4.2. Database per service

| Service | Database đề xuất | Ghi chú |
|---|---|---|
| Order | `order_db` | Đã có từ giai đoạn 1 |
| Product | `product_db` | PostgreSQL, có products và inventories |
| Payment | `payment_db` | PostgreSQL, có payments và payment_attempts |
| Notification | `notification_db` hoặc `notification_mongo` | Roadmap có MongoDB cho history; có thể dùng PostgreSQL tạm nếu muốn đơn giản hóa local |

Quy tắc bắt buộc:

- Service chỉ đọc/ghi database của chính nó.
- Không join database liên service.
- Nếu cần dữ liệu service khác, dùng event hoặc read model riêng ở giai đoạn sau.

### 4.3. Event-driven flow

```text
Order Service
  publishes order.created

Product Service
  consumes order.created
  reserves stock
  publishes stock.reserved hoặc stock.insufficient

Payment Service
  consumes stock.reserved
  creates payment
  simulates payment provider
  publishes payment.completed hoặc payment.failed

Order Service
  consumes stock.reserved, stock.insufficient, payment.completed, payment.failed
  updates order status

Notification Service
  consumes order.created, stock.insufficient, payment.completed, payment.failed
  records notification history
  sends fake email/log notification
```

---

## 5. Trạng Thái Order Mục Tiêu

### 5.1. State machine tối thiểu

```text
Pending
  -> StockReserved
  -> PaymentCompleted
  -> Confirmed

Pending
  -> StockInsufficient
  -> Cancelled

StockReserved
  -> PaymentFailed
  -> Cancelled
```

Tên trạng thái có thể điều chỉnh theo Domain hiện tại, nhưng cần đảm bảo ý nghĩa tương đương:

- `Pending`: Order vừa tạo, chờ giữ hàng.
- `StockReserved`: Product Service đã giữ hàng thành công.
- `StockInsufficient`: Không đủ tồn kho.
- `PaymentPending`: Đã tạo payment nhưng chưa có kết quả.
- `PaymentCompleted`: Thanh toán thành công.
- `PaymentFailed`: Thanh toán thất bại.
- `Confirmed`: Order được xác nhận hoàn tất.
- `Cancelled`: Order bị hủy.

### 5.2. Quy tắc chuyển trạng thái

- `stock.reserved` chỉ được áp dụng khi order đang ở trạng thái chờ stock.
- `stock.insufficient` đưa order về trạng thái hủy hoặc failed theo thiết kế Domain.
- `payment.completed` chỉ xác nhận order nếu stock đã được reserve.
- `payment.failed` không xóa order, chỉ đánh dấu failed/cancelled và lưu lý do.
- Consumer phải idempotent: nhận lại cùng event không làm trạng thái sai hoặc tạo bản ghi trùng.

---

## 6. Product Service

### 6.1. Mục tiêu

Product Service chịu trách nhiệm quản lý catalog và tồn kho tối thiểu để phục vụ luồng đặt hàng.

Kết thúc phần Product cần có:

- API quản lý product tối thiểu.
- Entity `Product`, `Category`, `Inventory`, `StockReservation`.
- Consumer nhận `order.created`.
- Logic reserve stock theo danh sách item trong order.
- Publish `stock.reserved` hoặc `stock.insufficient`.
- Database `product_db` có migration ban đầu.

### 6.2. Project structure

```text
src/Services/Product/
├── ECommerceMS.Product.Domain/
├── ECommerceMS.Product.Application/
├── ECommerceMS.Product.Infrastructure/
└── ECommerceMS.Product.API/
```

### 6.3. Domain model đề xuất

```text
Product
  Id
  Sku
  Name
  Description
  Price
  Currency
  IsActive
  CategoryId
  CreatedAtUtc
  UpdatedAtUtc

Category
  Id
  Name
  Slug
  IsActive

Inventory
  Id
  ProductId
  QuantityOnHand
  QuantityReserved
  UpdatedAtUtc

StockReservation
  Id
  OrderId
  ProductId
  Quantity
  Status
  ReservedAtUtc
  ReleasedAtUtc
```

### 6.4. Application use cases

Commands:

- `CreateProductCommand`
- `UpdateProductCommand`
- `ActivateProductCommand`
- `DeactivateProductCommand`
- `AdjustInventoryCommand`
- `ReserveStockForOrderCommand`
- `ReleaseStockForOrderCommand`

Queries:

- `GetProductByIdQuery`
- `GetProductsQuery`
- `GetInventoryByProductIdQuery`

Consumer handlers:

- `OrderCreatedConsumer`
- `OrderCancelledConsumer`

### 6.5. API endpoints tối thiểu

| Method | Path | Auth | Mục đích |
|---|---|---|---|
| `GET` | `/api/products` | Optional hoặc customer | Danh sách product |
| `GET` | `/api/products/{id}` | Optional hoặc customer | Chi tiết product |
| `POST` | `/api/products` | Admin | Tạo product |
| `PUT` | `/api/products/{id}` | Admin | Cập nhật product |
| `POST` | `/api/products/{id}/inventory/adjust` | Admin | Điều chỉnh tồn kho |
| `GET` | `/api/products/{id}/inventory` | Admin | Xem tồn kho |

### 6.6. Quy tắc reserve stock

Khi nhận `order.created`:

1. Kiểm tra event id đã xử lý chưa.
2. Load inventory của tất cả product trong order.
3. Nếu thiếu product hoặc product inactive, publish `stock.insufficient`.
4. Nếu bất kỳ item nào không đủ tồn kho khả dụng, publish `stock.insufficient`.
5. Nếu đủ, tăng `QuantityReserved` và tạo `StockReservation`.
6. Commit transaction.
7. Publish `stock.reserved` thông qua Outbox.

Tồn kho khả dụng:

```text
AvailableQuantity = QuantityOnHand - QuantityReserved
```

### 6.7. Product Service deliverables

- Product API chạy được độc lập.
- Migration `product_db` ban đầu.
- Seed data tối thiểu cho 3-5 sản phẩm demo.
- Consumer `order.created`.
- Consumer `order.cancelled` để release stock nếu cần.
- Publish `stock.reserved` và `stock.insufficient`.
- Unit test cho reserve stock happy path và insufficient path.

---

## 7. Payment Service

### 7.1. Mục tiêu

Payment Service xử lý thanh toán giả lập để chứng minh luồng nghiệp vụ sau khi giữ hàng thành công.

Kết thúc phần Payment cần có:

- API xem payment theo order.
- Entity `Payment` và `PaymentAttempt`.
- Consumer nhận `stock.reserved`.
- Fake payment provider có thể trả success/failure có kiểm soát.
- Publish `payment.completed` hoặc `payment.failed`.
- Database `payment_db` có migration ban đầu.

### 7.2. Project structure

```text
src/Services/Payment/
├── ECommerceMS.Payment.Domain/
├── ECommerceMS.Payment.Application/
├── ECommerceMS.Payment.Infrastructure/
└── ECommerceMS.Payment.API/
```

### 7.3. Domain model đề xuất

```text
Payment
  Id
  OrderId
  CustomerId
  Amount
  Currency
  Status
  Provider
  ProviderTransactionId
  FailureReason
  CreatedAtUtc
  UpdatedAtUtc

PaymentAttempt
  Id
  PaymentId
  AttemptNo
  Status
  RequestPayload
  ResponsePayload
  CreatedAtUtc
```

### 7.4. Payment status

```text
Pending
Processing
Completed
Failed
Cancelled
```

Quy tắc:

- Một order chỉ có một payment active tại một thời điểm.
- Nhận lại cùng `stock.reserved` không tạo payment trùng.
- Fake provider phải deterministic trong test để dễ nghiệm thu.
- Không lưu thông tin thẻ thật hoặc dữ liệu nhạy cảm.

### 7.5. Application use cases

Commands:

- `CreatePaymentForOrderCommand`
- `ProcessPaymentCommand`
- `MarkPaymentCompletedCommand`
- `MarkPaymentFailedCommand`
- `CancelPaymentCommand`

Queries:

- `GetPaymentByIdQuery`
- `GetPaymentByOrderIdQuery`
- `GetPaymentsQuery`

Consumer handlers:

- `StockReservedConsumer`
- `OrderCancelledConsumer`

### 7.6. API endpoints tối thiểu

| Method | Path | Auth | Mục đích |
|---|---|---|---|
| `GET` | `/api/payments/{id}` | Admin hoặc owner | Chi tiết payment |
| `GET` | `/api/payments/by-order/{orderId}` | Admin hoặc owner | Payment theo order |
| `POST` | `/api/payments/{id}/simulate-success` | Admin hoặc dev only | Ép payment success khi demo |
| `POST` | `/api/payments/{id}/simulate-failure` | Admin hoặc dev only | Ép payment failure khi demo |

Các endpoint simulate chỉ dùng local/dev, không xem là API production.

### 7.7. Fake payment provider

Thiết kế provider giả lập:

- Interface: `IPaymentProvider`.
- Implementation: `FakePaymentProvider`.
- Input: `OrderId`, `Amount`, `Currency`, `CustomerId`.
- Output:
  - `Succeeded`
  - `ProviderTransactionId`
  - `FailureReason`

Cơ chế điều khiển kết quả đề xuất:

- Theo configuration: `Payment:FakeProvider:DefaultResult = Success|Failure`.
- Theo amount: ví dụ amount kết thúc bằng `.13` thì fail để test.
- Theo endpoint dev-only để đổi trạng thái nếu cần demo.

### 7.8. Payment Service deliverables

- Payment API chạy được độc lập.
- Migration `payment_db` ban đầu.
- Consumer `stock.reserved`.
- Consumer `order.cancelled` nếu order bị hủy trước khi payment xong.
- Publish `payment.completed` và `payment.failed`.
- Unit test cho payment success/failure.
- Integration/manual test chứng minh nhận `stock.reserved` tạo payment.

---

## 8. Notification Service

### 8.1. Mục tiêu

Notification Service nhận các event nghiệp vụ quan trọng, lưu lịch sử thông báo và gửi thông báo giả lập bằng log hoặc fake sender.

Kết thúc phần Notification cần có:

- API xem notification history.
- Entity `NotificationMessage` hoặc document tương đương.
- Consumer nhận các event quan trọng.
- Fake email sender hoặc log sender.
- Chuẩn bị naming và contract để giai đoạn SignalR có thể mở rộng.

### 8.2. Project structure

```text
src/Services/Notification/
├── ECommerceMS.Notification.Domain/
├── ECommerceMS.Notification.Application/
├── ECommerceMS.Notification.Infrastructure/
└── ECommerceMS.Notification.API/
```

### 8.3. Persistence lựa chọn

Roadmap đề cập MongoDB cho notification history. Có hai lựa chọn:

| Lựa chọn | Khi dùng | Ghi chú |
|---|---|---|
| MongoDB | Muốn bám roadmap ngay | Phù hợp history/document, thêm dependency local |
| PostgreSQL | Muốn nhất quán giai đoạn đầu | Đơn giản hơn, có thể migrate sang MongoDB sau |

Khuyến nghị cho giai đoạn 4:

- Nếu MongoDB đã có trong local tooling, dùng MongoDB.
- Nếu chưa, dùng PostgreSQL tạm nhưng ghi rõ ADR ngắn trong `Docs/architecture`.

### 8.4. Domain model đề xuất

```text
NotificationMessage
  Id
  RecipientUserId
  RecipientEmail
  Type
  Channel
  Subject
  Body
  Status
  SourceEventId
  SourceEventType
  CorrelationId
  CreatedAtUtc
  SentAtUtc
  FailureReason
```

Notification status:

```text
Pending
Sent
Failed
Skipped
```

Notification channel:

```text
Email
InApp
Sms
```

Giai đoạn 4 chỉ cần `Email` fake và có thể chuẩn bị `InApp` cho SignalR ở giai đoạn 7.

### 8.5. Consumer handlers

Tối thiểu:

- `OrderCreatedConsumer`
- `StockInsufficientConsumer`
- `PaymentCompletedConsumer`
- `PaymentFailedConsumer`
- `OrderCancelledConsumer`

Mỗi consumer:

1. Kiểm tra idempotency theo `EventId`.
2. Tạo notification message.
3. Gọi fake sender.
4. Lưu trạng thái `Sent` hoặc `Failed`.
5. Log với correlation id.

### 8.6. API endpoints tối thiểu

| Method | Path | Auth | Mục đích |
|---|---|---|---|
| `GET` | `/api/notifications` | Admin | Danh sách notification |
| `GET` | `/api/notifications/{id}` | Admin hoặc owner | Chi tiết notification |
| `GET` | `/api/notifications/by-user/{userId}` | Admin hoặc owner | Lịch sử theo user |

### 8.7. Notification templates tối thiểu

| Event | Type | Subject đề xuất |
|---|---|---|
| `order.created` | `OrderCreated` | `Đơn hàng của bạn đã được tạo` |
| `stock.insufficient` | `StockInsufficient` | `Một số sản phẩm không đủ hàng` |
| `payment.completed` | `PaymentCompleted` | `Thanh toán thành công` |
| `payment.failed` | `PaymentFailed` | `Thanh toán thất bại` |
| `order.cancelled` | `OrderCancelled` | `Đơn hàng đã bị hủy` |

### 8.8. Notification Service deliverables

- Notification API chạy được độc lập.
- Persistence cho notification history.
- Consumer các event chính.
- Fake sender ghi log và cập nhật trạng thái.
- Idempotency tránh gửi trùng khi consumer retry.
- Manual test chứng minh notification được tạo sau order/payment event.

---

## 9. Cập Nhật Order Service

### 9.1. Mục tiêu

Order Service phải phản ứng với kết quả từ Product và Payment để cập nhật trạng thái order theo event, không gọi trực tiếp database của service khác.

### 9.2. Consumer cần bổ sung

- `StockReservedConsumer`
- `StockInsufficientConsumer`
- `PaymentCompletedConsumer`
- `PaymentFailedConsumer`

### 9.3. Command nội bộ đề xuất

- `MarkOrderStockReservedCommand`
- `MarkOrderStockInsufficientCommand`
- `MarkOrderPaymentCompletedCommand`
- `MarkOrderPaymentFailedCommand`
- `ConfirmOrderCommand`

### 9.4. Quy tắc xử lý

- Consumer chỉ map event sang command/application service.
- Logic chuyển trạng thái nằm trong aggregate hoặc domain service.
- Nếu order không tồn tại, log warning và đưa event vào retry/error theo chính sách.
- Nếu event đã xử lý, bỏ qua an toàn.
- Nếu event đến sai thứ tự, xử lý theo state machine:
  - Có thể reject và retry sau.
  - Hoặc lưu inbox/pending event nếu thiết kế đã có.
  - Giai đoạn 4 ưu tiên đơn giản: reject rõ ràng và log đủ context.

### 9.5. Deliverables Order update

- Order Service consume được stock/payment events.
- Order status thay đổi đúng theo event.
- Không gọi trực tiếp Product/Payment database.
- Log có `OrderId`, `EventId`, `CorrelationId`.
- Unit test cho state transition quan trọng.

---

## 10. Shared Contracts Cần Rà Soát

### 10.1. Event contract bắt buộc

Tối thiểu cần có:

```text
Orders/
  OrderCreatedIntegrationEvent
  OrderCancelledIntegrationEvent
  OrderConfirmedIntegrationEvent

Stocks/
  StockReservedIntegrationEvent
  StockInsufficientIntegrationEvent
  StockReservationItemIntegrationDto

Payments/
  PaymentCompletedIntegrationEvent
  PaymentFailedIntegrationEvent
```

### 10.2. Event metadata

Mỗi event cần có:

- `EventId`
- `EventType`
- `Version`
- `OccurredAtUtc`
- `CorrelationId`
- `CausationId`
- `Source`

### 10.3. Versioning

Quy tắc:

- Không rename field đã publish nếu không tăng version.
- Field mới nên nullable hoặc có default để backward compatible.
- Consumer không phụ thuộc vào field không cần thiết.
- Không share command/request DTO nội bộ qua contracts.

---

## 11. Gateway Và Auth

### 11.1. Route cần bổ sung

| Service | Prefix Gateway | Downstream local |
|---|---|---|
| Product API | `/api/products`, `/api/categories` | Product API port |
| Payment API | `/api/payments` | Payment API port |
| Notification API | `/api/notifications` | Notification API port |

### 11.2. Auth policy đề xuất

| Endpoint group | Role |
|---|---|
| Product read | `customer` hoặc anonymous tùy demo |
| Product write/inventory | `admin` |
| Payment read | `customer` owner hoặc `admin` |
| Payment simulate | `admin` hoặc dev only |
| Notification history all | `admin` |
| Notification by current user | `customer` owner |

### 11.3. Quy tắc

- Gateway vẫn là entrypoint chính.
- Service API vẫn validate JWT nếu endpoint protected.
- Không hardcode issuer, audience hoặc secret trong code.
- Không đưa business decision vào Gateway.

---

## 12. Thứ Tự Triển Khai Chi Tiết

### Bước 1 - Rà soát giai đoạn 1-3

Việc làm:

- Build solution.
- Kiểm tra Order Service chạy được.
- Kiểm tra Kafka local chạy được.
- Kiểm tra Shared.Contracts đã có event cần thiết.
- Kiểm tra Gateway route Order còn hoạt động.

Output mong đợi:

- Baseline sạch trước khi thêm service.
- Danh sách thiếu sót nhỏ cần xử lý trước khi dựng Product.

### Bước 2 - Dựng Product Service skeleton

Việc làm:

- Tạo 4 project Product theo Clean Architecture.
- Add project vào solution.
- Thiết lập project references đúng chiều.
- Cài packages tương tự Order Service.
- Tạo module registration extension.

Output mong đợi:

- Product projects build được.
- Không có dependency ngược từ Domain ra Infrastructure/API.

### Bước 3 - Implement Product Domain và persistence

Việc làm:

- Tạo Product, Category, Inventory, StockReservation.
- Tạo repository interfaces.
- Tạo DbContext và EF configurations.
- Tạo migration ban đầu.
- Seed product demo.

Output mong đợi:

- `product_db` có schema ban đầu.
- Có thể tạo product và inventory.

### Bước 4 - Implement Product API

Việc làm:

- Tạo commands/queries.
- Tạo validators.
- Tạo endpoints/controllers.
- Cấu hình OpenAPI/Scalar nếu pattern hiện tại có.
- Kiểm tra route trực tiếp service và qua Gateway.

Output mong đợi:

- Có thể list/get/create/update product.
- Có thể adjust inventory.

### Bước 5 - Implement Product consumer/publisher

Việc làm:

- Consume `order.created`.
- Reserve stock trong transaction.
- Publish `stock.reserved` hoặc `stock.insufficient`.
- Consume `order.cancelled` để release stock.
- Bổ sung idempotency.

Output mong đợi:

- Tạo order sinh event làm Product reserve stock.
- Log thể hiện event chain có correlation id.

### Bước 6 - Dựng Payment Service skeleton

Việc làm:

- Tạo 4 project Payment theo Clean Architecture.
- Add project vào solution.
- Thiết lập project references.
- Cài packages EF Core, MassTransit, validation, mapping.

Output mong đợi:

- Payment projects build được.

### Bước 7 - Implement Payment Domain và fake provider

Việc làm:

- Tạo Payment, PaymentAttempt.
- Tạo payment status.
- Tạo `IPaymentProvider`.
- Implement `FakePaymentProvider`.
- Tạo DbContext, configurations, migration.

Output mong đợi:

- `payment_db` có schema ban đầu.
- Fake provider chạy deterministic.

### Bước 8 - Implement Payment consumer/publisher/API

Việc làm:

- Consume `stock.reserved`.
- Tạo payment record.
- Process payment giả lập.
- Publish `payment.completed` hoặc `payment.failed`.
- Tạo endpoint xem payment.
- Tạo endpoint simulate nếu cần demo.

Output mong đợi:

- Payment tự xử lý sau khi Product giữ hàng.
- Có thể tra payment theo order.

### Bước 9 - Dựng Notification Service skeleton

Việc làm:

- Tạo 4 project Notification theo Clean Architecture.
- Chọn persistence MongoDB hoặc PostgreSQL.
- Add project vào solution.
- Thiết lập references và packages.

Output mong đợi:

- Notification projects build được.
- Persistence decision được ghi nhận.

### Bước 10 - Implement Notification consumers và fake sender

Việc làm:

- Tạo NotificationMessage.
- Tạo fake email/log sender.
- Consume event chính.
- Lưu notification history.
- Bổ sung idempotency theo event id.
- Tạo API xem lịch sử.

Output mong đợi:

- Event nghiệp vụ tạo notification.
- Không gửi trùng khi retry cùng event.

### Bước 11 - Cập nhật Order Service consumers

Việc làm:

- Consume `stock.reserved`.
- Consume `stock.insufficient`.
- Consume `payment.completed`.
- Consume `payment.failed`.
- Cập nhật state machine trong Domain nếu thiếu.
- Bổ sung unit test transition.

Output mong đợi:

- Order status đi đúng luồng.
- Không có service nào ghi trực tiếp vào order DB ngoài Order Service.

### Bước 12 - Cập nhật Gateway route

Việc làm:

- Thêm route Product, Payment, Notification.
- Thêm auth policy tương ứng.
- Kiểm tra 401/403 cho endpoint protected.

Output mong đợi:

- Client có thể gọi service mới qua Gateway.
- Direct service port chỉ dùng dev/debug.

### Bước 13 - Kiểm thử end-to-end thủ công

Việc làm:

- Seed product có stock.
- Tạo order qua Gateway.
- Theo dõi Kafka/log.
- Kiểm tra stock reservation.
- Kiểm tra payment record.
- Kiểm tra order status.
- Kiểm tra notification history.

Output mong đợi:

- Happy path chạy từ order đến notification.
- Failure path stock insufficient chạy đúng.
- Failure path payment failed chạy đúng.

### Bước 14 - Viết test tự động trọng tâm

Việc làm:

- Unit test Product reserve stock.
- Unit test Payment success/failure.
- Unit test Order state transition.
- Unit test Notification idempotency.
- Integration test publish/consume nếu thời gian cho phép.

Output mong đợi:

- Test bảo vệ logic nghiệp vụ chính.
- `dotnet test` pass.

### Bước 15 - Cập nhật tài liệu

Việc làm:

- Cập nhật README hoặc runbook local nếu có service mới.
- Tạo business flow note nếu chưa có.
- Ghi rõ cách seed data và chạy demo.
- Ghi rõ event topics và trạng thái order.

Output mong đợi:

- Developer mới biết chạy luồng order end-to-end.
- Demo có checklist rõ ràng.

---

## 13. Test Plan

### 13.1. Unit tests

Product:

- Reserve stock thành công khi đủ hàng.
- Reserve stock fail khi thiếu hàng.
- Reserve stock idempotent theo `OrderId` hoặc `EventId`.
- Release stock khi order cancelled.

Payment:

- Tạo payment từ `stock.reserved`.
- Fake provider success tạo `payment.completed`.
- Fake provider failure tạo `payment.failed`.
- Consumer không tạo payment trùng.

Order:

- Pending -> StockReserved.
- Pending -> Cancelled khi stock insufficient.
- StockReserved -> Confirmed khi payment completed.
- StockReserved -> Cancelled hoặc PaymentFailed khi payment failed.
- Event sai thứ tự không phá state.

Notification:

- Event tạo notification message đúng type.
- Fake sender cập nhật Sent.
- Retry cùng event không tạo notification trùng.

### 13.2. Integration tests nên có

- Tạo order -> Product consume `order.created` -> publish `stock.reserved`.
- Product publish `stock.reserved` -> Payment consume -> publish `payment.completed`.
- Payment publish `payment.completed` -> Order consume -> status Confirmed.
- Payment publish `payment.completed` -> Notification consume -> history có record.

Nếu chưa viết integration test tự động trong giai đoạn này, phải có manual verification chi tiết và log minh chứng.

### 13.3. Manual test happy path

Điều kiện:

- Product A có `QuantityOnHand = 10`, `QuantityReserved = 0`.
- Fake payment provider mặc định success.
- Kafka, Product, Payment, Order, Notification chạy.

Các bước:

1. Gọi Gateway tạo order với Product A quantity 2.
2. Kiểm tra Order ban đầu ở trạng thái pending.
3. Kiểm tra Product tạo stock reservation quantity 2.
4. Kiểm tra Payment tạo payment completed.
5. Kiểm tra Order chuyển confirmed.
6. Kiểm tra Notification có message payment completed.

Kết quả pass:

- Không có exception trong log.
- Event chain có cùng correlation id.
- Tồn kho reserved đúng.
- Order status cuối cùng đúng.

### 13.4. Manual test stock insufficient

Điều kiện:

- Product B có available quantity 1.

Các bước:

1. Tạo order với Product B quantity 5.
2. Product consume `order.created`.
3. Product publish `stock.insufficient`.
4. Order consume `stock.insufficient`.
5. Notification consume `stock.insufficient`.

Kết quả pass:

- Không tạo payment.
- Order chuyển cancelled hoặc stock insufficient theo state machine.
- Notification history có thông báo thiếu hàng.

### 13.5. Manual test payment failed

Điều kiện:

- Product đủ stock.
- Fake payment provider được cấu hình failure.

Các bước:

1. Tạo order.
2. Product reserve stock thành công.
3. Payment xử lý failure.
4. Payment publish `payment.failed`.
5. Order cập nhật trạng thái failure/cancelled.
6. Notification tạo thông báo payment failed.

Kết quả pass:

- Payment record có failure reason.
- Order không confirmed.
- Stock release có thể để giai đoạn sau nếu chưa có, nhưng phải ghi rõ TODO nếu chưa xử lý.

---

## 14. Logging Và Correlation

### 14.1. Log fields bắt buộc

Các consumer/publisher cần log:

- `CorrelationId`
- `CausationId`
- `EventId`
- `EventType`
- `OrderId`
- `CustomerId` nếu có
- `ServiceName`
- `ConsumerName`

### 14.2. Log events quan trọng

- Consumer bắt đầu xử lý event.
- Event bị bỏ qua do đã xử lý.
- Business validation fail.
- Publish event mới.
- Transaction commit thành công.
- Exception trước retry/error.

### 14.3. Quy tắc

- Không log thông tin nhạy cảm.
- Không log full payload nếu payload lớn hoặc chứa dữ liệu cá nhân.
- Log phải đủ để truy theo một order từ đầu đến cuối.

---

## 15. Configuration

### 15.1. Product Service

Config cần có:

```text
ConnectionStrings:ProductDb
Kafka:BootstrapServers
Kafka:ConsumerGroup
Messaging:Topics:OrderCreated
Messaging:Topics:OrderCancelled
Messaging:Topics:StockReserved
Messaging:Topics:StockInsufficient
Auth:Authority
Auth:Audience
```

### 15.2. Payment Service

Config cần có:

```text
ConnectionStrings:PaymentDb
Kafka:BootstrapServers
Kafka:ConsumerGroup
Messaging:Topics:StockReserved
Messaging:Topics:PaymentCompleted
Messaging:Topics:PaymentFailed
Payment:FakeProvider:DefaultResult
Auth:Authority
Auth:Audience
```

### 15.3. Notification Service

Config cần có:

```text
ConnectionStrings:NotificationDb
Mongo:ConnectionString
Mongo:DatabaseName
Kafka:BootstrapServers
Kafka:ConsumerGroup
Notification:Sender:Mode
Auth:Authority
Auth:Audience
```

Chỉ dùng config thật theo service đã chọn. Không cần vừa PostgreSQL vừa MongoDB nếu chỉ dùng một loại persistence.

---

## 16. Port Mapping Đề Xuất

Nếu các port ở `Docs/architecture/local-port-mapping.md` đã có giá trị khác, ưu tiên tài liệu port mapping hiện tại. Nếu chưa có, dùng đề xuất:

| Component | Port |
|---|---:|
| API Gateway | 7000 |
| Order API | 7101 |
| Product API | 7102 |
| Payment API | 7103 |
| Notification API | 7104 |
| Keycloak | 8080 |
| Kafka broker | 9092 |
| PostgreSQL | 5432 |
| MongoDB | 27017 |

Quy tắc:

- Không dùng trùng port.
- Gateway là port client ưu tiên dùng.
- Service port dùng cho debug/local development.

---

## 17. Deliverables Tổng Hợp

### 17.1. Source deliverables

- Product Service đầy đủ 4 layer.
- Payment Service đầy đủ 4 layer.
- Notification Service đầy đủ 4 layer.
- Order Service có consumer cập nhật trạng thái từ stock/payment.
- Shared.Contracts được rà soát và bổ sung nếu thiếu.
- Gateway routes cho Product/Payment/Notification.

### 17.2. Database deliverables

- Migration ban đầu cho `product_db`.
- Migration ban đầu cho `payment_db`.
- Migration hoặc collection setup cho notification history.
- Seed data Product tối thiểu.

### 17.3. Messaging deliverables

- Product consume `order.created`, publish stock events.
- Payment consume `stock.reserved`, publish payment events.
- Notification consume business events và lưu history.
- Order consume stock/payment events.
- Consumer idempotency cho các event chính.

### 17.4. Documentation deliverables

- Business flow đặt hàng end-to-end.
- Event/topic matrix cập nhật.
- Cách chạy service mới local.
- Cách seed product và test demo.
- Ghi chú persistence Notification nếu chọn khác roadmap.

---

## 18. Definition Of Done

Giai đoạn 4 chỉ được xem là hoàn thành khi đạt tất cả điều kiện sau:

- `dotnet build ECommerceMS.sln` pass.
- Product, Payment, Notification API chạy được độc lập.
- Gateway route được đến Product, Payment, Notification.
- Product Service có database riêng và không đọc order/payment database.
- Payment Service có database riêng và không đọc order/product database.
- Notification Service có persistence riêng và không đọc database service khác.
- Tạo order thành công có thể kích hoạt reserve stock.
- Stock đủ hàng tạo `stock.reserved`.
- Stock thiếu hàng tạo `stock.insufficient`.
- Payment success tạo `payment.completed`.
- Payment failure tạo `payment.failed`.
- Order Service cập nhật trạng thái theo event.
- Notification Service tạo lịch sử thông báo theo event.
- Consumer xử lý idempotent ở các luồng chính.
- Log có correlation id xuyên suốt luồng demo.
- Unit test trọng tâm pass.
- Manual test happy path, stock insufficient, payment failed được ghi nhận.
- Không có business logic trong Gateway.
- Không có shared DTO nội bộ bị dùng làm integration contract.

---

## 19. Checklist Nghiệm Thu

### 19.1. Architecture checklist

- [ ] Mỗi service có Clean Architecture riêng.
- [ ] Domain không phụ thuộc EF Core, MassTransit, ASP.NET Core.
- [ ] Infrastructure phụ thuộc Application/Domain, không ngược lại.
- [ ] API chỉ đóng vai trò transport boundary.
- [ ] Shared.Contracts chỉ chứa public contracts.
- [ ] Không có business logic service riêng trong Shared.
- [ ] Database per service được giữ đúng.
- [ ] Gateway là entrypoint chính.

### 19.2. Product checklist

- [ ] Product API list/get/create/update chạy được.
- [ ] Inventory adjust chạy được.
- [ ] `order.created` được consume.
- [ ] Reserve stock thành công khi đủ hàng.
- [ ] Publish `stock.reserved`.
- [ ] Publish `stock.insufficient` khi thiếu hàng.
- [ ] Consumer idempotent.

### 19.3. Payment checklist

- [ ] `stock.reserved` được consume.
- [ ] Payment record được tạo.
- [ ] Fake provider success/failure kiểm soát được.
- [ ] Publish `payment.completed`.
- [ ] Publish `payment.failed`.
- [ ] Không tạo payment trùng khi retry.

### 19.4. Notification checklist

- [ ] Event chính được consume.
- [ ] Notification history được lưu.
- [ ] Fake sender hoạt động.
- [ ] Không gửi/lưu trùng khi retry cùng event.
- [ ] API xem lịch sử hoạt động.

### 19.5. Order checklist

- [ ] Consume `stock.reserved`.
- [ ] Consume `stock.insufficient`.
- [ ] Consume `payment.completed`.
- [ ] Consume `payment.failed`.
- [ ] State transition đúng.
- [ ] Event sai thứ tự không làm hỏng trạng thái.

### 19.6. Test checklist

- [ ] `dotnet build` pass.
- [ ] `dotnet test` pass với test đã có.
- [ ] Happy path được kiểm thử.
- [ ] Stock insufficient path được kiểm thử.
- [ ] Payment failed path được kiểm thử.
- [ ] Log/event đủ để truy vết.

---

## 20. Rủi Ro Và Cách Kiểm Soát

### 20.1. Service coupling quá chặt

Rủi ro:

- Product hoặc Payment gọi trực tiếp Order database.
- Shared project chứa business logic riêng.

Kiểm soát:

- Review dependency direction.
- Chỉ dùng Kafka event cho giao tiếp nghiệp vụ liên service.
- Chỉ share integration contracts ổn định.

### 20.2. Event bị xử lý trùng

Rủi ro:

- Retry consumer làm reserve stock hai lần.
- Payment được tạo nhiều lần.
- Notification gửi trùng.

Kiểm soát:

- Dùng inbox/processed messages theo `EventId`.
- Unique constraint theo `OrderId` cho stock reservation/payment active nếu phù hợp.
- Consumer phải check trước khi tạo side effect.

### 20.3. Event đến sai thứ tự

Rủi ro:

- `payment.completed` đến trước `stock.reserved`.
- `order.cancelled` đến khi payment đang processing.

Kiểm soát:

- State machine kiểm tra transition hợp lệ.
- Log rõ event bị reject.
- Nếu cần, lưu pending event ở giai đoạn sau.

### 20.4. Transaction và publish không đồng bộ

Rủi ro:

- Database commit thành công nhưng event không publish.
- Event publish nhưng database rollback.

Kiểm soát:

- Dùng Outbox cho service publish event sau transaction.
- Không publish trực tiếp trong Domain.
- Manual test restart service khi có outbox pending nếu đã implement processor.

### 20.5. Business flow bị lệch scope

Rủi ro:

- Sa đà vào checkout thật, coupon, cart, shipment, refund.

Kiểm soát:

- Giai đoạn 4 chỉ tập trung Product, Payment giả lập, Notification giả lập.
- Ghi TODO cho tính năng thương mại nâng cao.
- Không mở rộng schema quá sớm nếu chưa có use case trong roadmap.

### 20.6. Notification persistence không khớp roadmap

Rủi ro:

- Roadmap dùng MongoDB nhưng team chọn PostgreSQL tạm, sau này gây nhầm lẫn.

Kiểm soát:

- Ghi ADR ngắn nếu chọn PostgreSQL.
- Giữ interface repository để có thể đổi persistence.
- Không để API phụ thuộc trực tiếp implementation persistence.

---

## 21. Thứ Tự Ưu Tiên Khi Thiếu Thời Gian

Nếu không đủ thời gian hoàn thành toàn bộ, ưu tiên theo thứ tự:

1. Product Service reserve stock và publish stock events.
2. Order Service consume stock events và cập nhật trạng thái.
3. Payment Service consume `stock.reserved` và publish payment events.
4. Order Service consume payment events và xác nhận/hủy order.
5. Notification Service consume payment/order events và lưu history.
6. API quản trị đầy đủ cho Product/Payment/Notification.
7. Test tự động mở rộng.

Không được bỏ qua:

- Database per service.
- Event contracts rõ ràng.
- Idempotency cho consumer có side effect.
- Log correlation id.

---

## 22. Cổng Kiểm Soát Sau Giai Đoạn 4 - Integration Gate

Trước khi sang giai đoạn 5 Redis, phải trả lời được:

- Tạo order qua Gateway có kích hoạt được Product reserve stock không?
- Product có publish đúng `stock.reserved` hoặc `stock.insufficient` không?
- Payment có được tạo sau `stock.reserved` không?
- Order status có được cập nhật chỉ bởi Order Service không?
- Notification có ghi nhận event nghiệp vụ không?
- Mỗi service có database riêng không?
- Consumer retry có tạo side effect trùng không?
- Log có correlation id để trace toàn bộ luồng không?
- Gateway có route đúng và không chứa business logic không?

Nếu một trong các câu trả lời là "không", chưa nên chuyển sang Redis vì cache/lock sẽ che lỗi thiết kế nghiệp vụ gốc và làm debug khó hơn.

