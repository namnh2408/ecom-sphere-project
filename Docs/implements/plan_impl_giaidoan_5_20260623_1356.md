# Implementation Plan Giai Đoạn 5 - Redis Cache, Session Và Distributed Lock

> Thuộc roadmap: `Docs/plans/plan_overral_time20260623.md`  
> Nguồn tham chiếu: `Docs/ecommerce-microservice-roadmap.md`, `Docs/microservice_full_architecture.svg`, `Docs/architecture/local-port-mapping.md`, `Docs/architecture/naming-conventions.md`, `Docs/implements/plan_impl_giaidoan_3_20260623_1345.md`, `Docs/implements/plan_impl_giaidoan_4_20260623_1350.md`  
> Giai đoạn: 5  
> Timestamp: 20260623_1356  
> Mục tiêu thời gian: Tuần 9  
> Runtime mục tiêu: .NET 8 LTS  
> Trạng thái: Kế hoạch implement chi tiết, chưa thực thi code

---

## 1. Mục Tiêu Giai Đoạn 5

Giai đoạn 5 đưa Redis vào hệ thống để giải quyết ba nhu cầu chính:

- Tăng hiệu năng đọc bằng cache-aside cho các query đọc nhiều.
- Chuẩn bị cart/session tạm thời cho trải nghiệm mua hàng.
- Bảo vệ một số critical section bằng distributed lock, đặc biệt là reserve stock trong Product Service.

Kết thúc giai đoạn này cần có:

- Redis local chạy được và được cấu hình rõ ràng.
- Shared Redis abstraction đủ dùng, không làm rò rỉ implementation ra Domain/Application.
- Product Service cache được product list/detail.
- Order Service cache được order detail nếu phù hợp.
- Cart/session tối thiểu có thể lưu trong Redis theo user/customer.
- Product Service dùng distributed lock đúng cách cho stock reservation hoặc đoạn cạnh tranh tương đương.
- Cache invalidation được xử lý khi dữ liệu Product/Order thay đổi.
- Redis down không làm sập toàn bộ service trong các use case có thể degrade.
- Key naming convention có prefix rõ ràng theo service, environment và purpose.
- Test plan kiểm chứng cache miss, cache hit, invalidation, lock timeout và Redis unavailable.

Redis không được xem là source of truth. Database của từng service vẫn là nguồn dữ liệu chính. Redis chỉ là lớp hỗ trợ hiệu năng, trạng thái tạm thời hoặc coordination ngắn hạn.

---

## 2. Phạm Vi Công Việc

### 2.1. Trong phạm vi

- Cấu hình Redis local.
- Tạo hoặc hoàn thiện `ECommerceMS.Shared.Infrastructure` cho Redis abstraction.
- Thiết kế key naming convention.
- Implement cache-aside cho Product query:
  - Product detail.
  - Product list/page.
  - Product inventory read nếu phù hợp.
- Implement cache-aside cho Order detail nếu use case đọc nhiều.
- Implement invalidation khi Product hoặc Order thay đổi.
- Implement cart/session tối thiểu bằng Redis.
- Implement distributed lock cho reserve stock hoặc critical section tương đương.
- Bổ sung health check Redis.
- Bổ sung logging cho cache hit/miss, invalidation và lock acquisition.
- Viết unit test cho abstraction và application logic.
- Viết integration/manual test với Redis thật hoặc Testcontainers nếu có.
- Cập nhật tài liệu local setup và key convention nếu cần.

### 2.2. Ngoài phạm vi

- Không dùng Redis thay database giao dịch.
- Không lưu dữ liệu thanh toán nhạy cảm trong Redis.
- Không triển khai Redis Cluster/Sentinel production.
- Không tối ưu eviction policy production-level.
- Không bắt buộc SignalR Redis backplane ở giai đoạn này, chỉ chuẩn bị convention cho giai đoạn 7.
- Không bắt buộc metrics Prometheus đầy đủ, nhưng nên chuẩn bị hook/log counter cache hit/miss.
- Không thay thế idempotency database bằng Redis nếu idempotency cần độ bền cao.
- Không dùng distributed lock để che lỗi transaction/state machine.

---

## 3. Điều Kiện Đầu Vào

### 3.1. Bắt buộc

- `.NET SDK 8.x` sẵn sàng.
- Solution build pass trước khi thêm Redis.
- Product Service từ giai đoạn 4 đã có query đọc product.
- Product Service đã có reserve stock logic.
- Order Service đã có endpoint lấy order detail.
- Gateway route đến Product và Order hoạt động.
- Kafka/event flow chính không bị lỗi nghiêm trọng.
- Docker Desktop hoặc Redis local có thể chạy được.

### 3.2. Nên có

- `ECommerceMS.Shared.Infrastructure` đã tồn tại hoặc có chỗ phù hợp để tạo.
- Local port mapping đã có Redis port, mặc định `6379`.
- Product seed data đã có để test cache.
- Manual test happy path order -> stock -> payment -> notification từ giai đoạn 4 đã chạy được.

### 3.3. Lệnh kiểm tra trước khi bắt đầu

```powershell
dotnet --version
dotnet build ECommerceMS.sln
docker --version
docker compose version
```

Nếu Redis chạy bằng Docker:

```powershell
docker run --name ecommerce-redis -p 6379:6379 -d redis:7-alpine
docker exec -it ecommerce-redis redis-cli ping
```

Kết quả mong đợi:

```text
PONG
```

Nếu Redis đã nằm trong compose local, dùng lệnh tương ứng:

```powershell
docker compose ps
docker compose exec redis redis-cli ping
```

---

## 4. Kiến Trúc Redis Mục Tiêu

### 4.1. Vai trò của Redis

| Use case | Service | Dữ liệu | TTL | Source of truth |
|---|---|---|---:|---|
| Product detail cache | Product | Product read DTO | 5-15 phút | `product_db` |
| Product list cache | Product | Paged result/filter result | 1-5 phút | `product_db` |
| Order detail cache | Order | Order detail DTO | 1-5 phút | `order_db` |
| Cart/session | Gateway hoặc Order/Cart area | Cart item tạm thời | 1-7 ngày | Redis tạm thời |
| Distributed lock | Product | Lock key cho stock reservation | 5-30 giây | Không lưu dữ liệu nghiệp vụ |
| SignalR backplane chuẩn bị | Notification | Pub/sub runtime | N/A | Notification history DB |

### 4.2. Nguyên tắc bắt buộc

- Cache chỉ lưu read model/DTO, không lưu entity EF Core tracked.
- Cache miss phải fallback về database.
- Cache stale phải được kiểm soát bằng TTL và invalidation.
- Redis unavailable không làm sập query đọc nếu database vẫn hoạt động.
- Lock phải có timeout và owner token.
- Lock release chỉ được thực hiện bởi owner đã acquire lock.
- Không cache dữ liệu nhạy cảm không cần thiết.
- Không đặt TTL vô hạn cho dữ liệu dễ thay đổi.
- Không để key không prefix vì dễ va chạm giữa service/môi trường.

### 4.3. Luồng cache-aside

```text
Request
  -> Application Query Handler
  -> Check Redis by cache key
     -> hit: return cached DTO
     -> miss: query database
              map DTO
              set Redis with TTL
              return DTO
```

### 4.4. Luồng invalidation

```text
Command thay đổi Product/Order
  -> Update database trong transaction
  -> Commit
  -> Remove cache keys liên quan
  -> Publish event nếu cần
```

Nếu invalidation xảy ra từ event consumer:

```text
Consumer nhận event
  -> Kiểm tra idempotency nếu có side effect
  -> Remove cache key liên quan
  -> Log correlation id
```

---

## 5. Redis Key Naming Convention

### 5.1. Format chung

```text
ecom:{environment}:{service}:{purpose}:{identifier}
```

Ví dụ:

```text
ecom:local:product:detail:{productId}
ecom:local:product:list:{hash}
ecom:local:product:inventory:{productId}
ecom:local:order:detail:{orderId}
ecom:local:cart:user:{userId}
ecom:local:lock:stock-reservation:{productId}
ecom:local:lock:order:{orderId}
```

### 5.2. Quy tắc key

- Dùng chữ thường cho segment cố định.
- Không đưa raw query string dài vào key; phải hash filter/sort/paging.
- Không đưa email, token, phone number trực tiếp vào key.
- Với user/customer id, dùng `userId` hoặc `customerId` dạng GUID/id nội bộ.
- Key lock phải có prefix `lock`.
- Key cart phải có prefix `cart`.
- Key cache phải thể hiện resource và identifier.

### 5.3. Key matrix đề xuất

| Use case | Key | TTL đề xuất |
|---|---|---:|
| Product detail | `ecom:{env}:product:detail:{productId}` | 10 phút |
| Product list | `ecom:{env}:product:list:{filterHash}` | 3 phút |
| Product inventory read | `ecom:{env}:product:inventory:{productId}` | 30-60 giây |
| Order detail | `ecom:{env}:order:detail:{orderId}` | 2 phút |
| Cart by user | `ecom:{env}:cart:user:{userId}` | 7 ngày |
| Stock reservation lock | `ecom:{env}:lock:stock-reservation:{productId}` | 10-30 giây |
| Order process lock | `ecom:{env}:lock:order:{orderId}` | 10-30 giây |

### 5.4. Key builder

Nên có abstraction:

```text
IRedisKeyBuilder
  ProductDetail(productId)
  ProductList(filter)
  ProductInventory(productId)
  OrderDetail(orderId)
  CartByUser(userId)
  StockReservationLock(productId)
```

Mục tiêu là không rải string key thủ công khắp code.

---

## 6. Shared Infrastructure Redis

### 6.1. Project mục tiêu

Nếu chưa có:

```text
src/Shared/ECommerceMS.Shared.Infrastructure/
```

Nội dung đề xuất:

```text
ECommerceMS.Shared.Infrastructure/
├── Caching/
│   ├── ICacheService.cs
│   ├── RedisCacheService.cs
│   ├── CacheOptions.cs
│   └── RedisKeyBuilder.cs
├── Locking/
│   ├── IDistributedLock.cs
│   ├── IDistributedLockManager.cs
│   ├── RedisDistributedLockManager.cs
│   └── DistributedLockOptions.cs
├── Redis/
│   ├── RedisOptions.cs
│   └── RedisServiceCollectionExtensions.cs
└── HealthChecks/
    └── RedisHealthCheckExtensions.cs
```

### 6.2. Packages đề xuất

```powershell
dotnet add src/Shared/ECommerceMS.Shared.Infrastructure package StackExchange.Redis
dotnet add src/Shared/ECommerceMS.Shared.Infrastructure package Microsoft.Extensions.Caching.StackExchangeRedis
dotnet add src/Shared/ECommerceMS.Shared.Infrastructure package Microsoft.Extensions.Diagnostics.HealthChecks
```

Nếu muốn distributed lock bằng thư viện thay vì tự implement:

```powershell
dotnet add src/Shared/ECommerceMS.Shared.Infrastructure package Medallion.Threading.Redis
```

Khuyến nghị:

- Dùng `IDistributedCache` cho cache đơn giản.
- Dùng `StackExchange.Redis` hoặc `Medallion.Threading.Redis` cho lock vì cần command atomic.
- Không tự viết lock phức tạp nếu chưa cần; ưu tiên thư viện đã được dùng rộng rãi.

### 6.3. Abstraction cache tối thiểu

```text
ICacheService
  GetAsync<T>(key, cancellationToken)
  SetAsync<T>(key, value, ttl, cancellationToken)
  RemoveAsync(key, cancellationToken)
  RemoveByPrefixAsync(prefix, cancellationToken) nếu thật sự cần
  GetOrSetAsync<T>(key, factory, ttl, cancellationToken)
```

Lưu ý:

- `RemoveByPrefixAsync` trên Redis production có thể tốn kém nếu dùng `KEYS`.
- Nếu cần remove nhiều key theo prefix, cân nhắc lưu key registry hoặc dùng versioned key.
- Giai đoạn 5 có thể tránh prefix scan bằng cách invalidate key cụ thể và TTL ngắn cho list cache.

### 6.4. Serialization

Quy tắc:

- Dùng `System.Text.Json`.
- Cấu hình camelCase hoặc giữ PascalCase phải thống nhất.
- Không serialize object có vòng tham chiếu.
- Không cache EF entity trực tiếp.
- Cache DTO/response model ổn định.

### 6.5. Resilience

Cache service phải hỗ trợ:

- Redis timeout không làm fail request đọc nếu database fallback được.
- Log warning khi Redis lỗi.
- Có option tắt cache theo config.
- Timeout ngắn cho Redis operation.

Config đề xuất:

```text
Redis:ConnectionString=localhost:6379
Redis:InstanceName=ecom-local
Redis:Enabled=true
Redis:DefaultTtlSeconds=300
Redis:OperationTimeoutMilliseconds=500
```

---

## 7. Product Cache

### 7.1. Mục tiêu

Product Service là nơi ưu tiên áp dụng cache vì product list/detail thường được đọc nhiều hơn ghi.

Cache áp dụng cho:

- `GetProductByIdQuery`
- `GetProductsQuery`
- `GetInventoryByProductIdQuery` nếu endpoint này có tần suất đọc cao

Không cache:

- Command write.
- Reserve stock command.
- Dữ liệu đang trong transaction.
- Dữ liệu phục vụ quyết định nghiệp vụ stock nếu cần độ chính xác tức thời.

### 7.2. Product detail cache

Luồng:

```text
GET /api/products/{id}
  -> key ecom:{env}:product:detail:{productId}
  -> cache hit: return ProductDto
  -> cache miss: query product_db
                   set cache TTL 10 phút
                   return ProductDto
```

Invalidation:

- Khi update product: remove detail key.
- Khi deactivate product: remove detail key và list keys liên quan.
- Khi adjust inventory nếu detail DTO chứa inventory: remove detail key.

### 7.3. Product list cache

Key:

```text
ecom:{env}:product:list:{filterHash}
```

Filter hash nên được tạo từ:

- Page number.
- Page size.
- Search keyword nếu có.
- Category id.
- Sort.
- IsActive.

TTL đề xuất:

- 1-3 phút nếu product thay đổi thường xuyên.
- 5 phút nếu demo đơn giản.

Invalidation:

- Khi create/update/deactivate product: có thể remove list cache bằng versioned key hoặc chấp nhận TTL ngắn.
- Giai đoạn 5 khuyến nghị TTL ngắn cho list cache để tránh scan prefix.

### 7.4. Inventory cache

Inventory là dữ liệu nhạy với thay đổi, cần cẩn thận.

Khuyến nghị:

- Không dùng cache inventory cho logic reserve stock.
- Chỉ cache inventory read endpoint cho admin nếu cần.
- TTL rất ngắn, 30-60 giây.
- Invalidate khi adjust inventory hoặc reserve/release stock.

### 7.5. Product cache deliverables

- Product detail cache-aside.
- Product list cache-aside.
- Invalidation khi product write.
- TTL rõ ràng.
- Log cache hit/miss.
- Manual test chứng minh hit/miss/invalidation.

---

## 8. Order Cache

### 8.1. Mục tiêu

Order detail có thể được cache để giảm tải khi client liên tục refresh trạng thái order.

Cache áp dụng cho:

- `GetOrderByIdQuery`
- Có thể áp dụng cho `GetOrdersByCustomerQuery` nếu đã có endpoint.

Không cache:

- Command tạo/hủy/cập nhật order.
- State machine transition.
- Consumer xử lý payment/stock events.

### 8.2. Order detail cache

Key:

```text
ecom:{env}:order:detail:{orderId}
```

TTL:

- 1-2 phút trong giai đoạn order còn thay đổi liên tục.
- Có thể tăng TTL cho order đã terminal như `Confirmed` hoặc `Cancelled`, nhưng chưa cần ở giai đoạn 5.

Invalidation:

- Sau CreateOrder: có thể set cache hoặc bỏ qua.
- Sau CancelOrder: remove order detail key.
- Sau StockReserved/StockInsufficient event: remove order detail key.
- Sau PaymentCompleted/PaymentFailed event: remove order detail key.

### 8.3. Order cache lưu ý bảo mật

- Chỉ cache DTO đã lọc field cần trả về.
- Không cache claim/token.
- Không để user A đọc cache order của user B; authorization vẫn phải chạy trước hoặc sau cache theo thiết kế an toàn.

Khuyến nghị:

- Với endpoint owner-based, validate quyền truy cập trước khi trả cache nếu cache key chỉ theo `orderId`.
- Hoặc thêm `customerId` vào key: `ecom:{env}:order:detail:{customerId}:{orderId}`.

### 8.4. Order cache deliverables

- Order detail cache-aside nếu endpoint đã ổn định.
- Invalidation theo command/event.
- Kiểm tra không trả dữ liệu cũ sau trạng thái order đổi.
- Kiểm tra authorization không bị bypass vì cache.

---

## 9. Cart Và Session Tối Thiểu

### 9.1. Mục tiêu

Tạo nền tảng cart/session bằng Redis để hỗ trợ trải nghiệm mua hàng trước khi tạo order.

Giai đoạn này chỉ cần cart tối thiểu:

- Add item.
- Update quantity.
- Remove item.
- Get cart.
- Clear cart sau khi tạo order thành công.

### 9.2. Ownership lựa chọn

Có hai hướng:

| Hướng | Khi dùng | Ghi chú |
|---|---|---|
| Tạo Cart module trong Order Service | Muốn nhanh, ít service mới | Phù hợp giai đoạn 5 |
| Tạo Cart Service riêng | Muốn tách bounded context rõ | Tăng scope, chưa cần ngay |

Khuyến nghị giai đoạn 5:

- Không tạo service mới nếu roadmap chưa yêu cầu.
- Tạo cart use case trong Order API hoặc một area/module rõ ràng, dùng Redis làm storage tạm.
- Ghi rõ cart không phải order, không giữ stock.

### 9.3. Cart data model

```text
Cart
  UserId
  Items
    ProductId
    Sku
    NameSnapshot
    UnitPriceSnapshot
    Currency
    Quantity
  UpdatedAtUtc
  ExpiresAtUtc
```

Lưu ý:

- Snapshot price/name chỉ phục vụ UI tạm thời.
- Khi tạo order thật, phải re-validate product, price và stock từ Product/Order flow.
- Không xem cart là cam kết giữ hàng.

### 9.4. Cart key

```text
ecom:{env}:cart:user:{userId}
```

TTL đề xuất:

- 7 ngày cho authenticated user.
- 1-2 ngày cho anonymous session nếu có.

### 9.5. Cart endpoints đề xuất

| Method | Path | Auth | Mục đích |
|---|---|---|---|
| `GET` | `/api/cart` | Customer | Lấy cart hiện tại |
| `POST` | `/api/cart/items` | Customer | Thêm item |
| `PUT` | `/api/cart/items/{productId}` | Customer | Cập nhật quantity |
| `DELETE` | `/api/cart/items/{productId}` | Customer | Xóa item |
| `DELETE` | `/api/cart` | Customer | Clear cart |

Nếu chưa muốn mở API cart trong giai đoạn này, tối thiểu phải có service abstraction và test nội bộ.

### 9.6. Cart consistency

Quy tắc:

- Add/update cart nên validate product tồn tại qua Product API hoặc product read model nếu đã có.
- Không reserve stock khi add cart.
- Khi checkout/tạo order, kiểm tra lại product và stock qua flow order -> product.
- Clear cart chỉ sau khi CreateOrder thành công.
- Nếu CreateOrder fail, giữ cart để user sửa.

### 9.7. Cart deliverables

- Cart storage Redis.
- Cart operations cơ bản.
- TTL cart rõ ràng.
- Clear cart sau checkout thành công nếu flow đã có.
- Manual test add/update/remove/clear.

---

## 10. Distributed Lock

### 10.1. Mục tiêu

Distributed lock dùng để giảm rủi ro race condition khi nhiều instance hoặc nhiều request cùng xử lý stock reservation cho cùng product/order.

Use case ưu tiên:

- Reserve stock trong Product Service.
- Idempotent job quan trọng nếu có.
- Không dùng lock đại trà cho mọi command.

### 10.2. Nguyên tắc lock

- Lock key phải đủ hẹp, ví dụ theo `ProductId` hoặc `OrderId`.
- Lock phải có expiry/lease time.
- Lock acquisition phải có timeout.
- Lock release chỉ bởi owner token.
- Trong lock vẫn cần transaction database.
- Lock không thay thế unique constraint/idempotency.
- Lock fail phải trả lỗi hoặc retry có kiểm soát, không chờ vô hạn.

### 10.3. Lock key đề xuất

Nếu reserve từng product:

```text
ecom:{env}:lock:stock-reservation:{productId}
```

Nếu reserve cả order:

```text
ecom:{env}:lock:stock-reservation:order:{orderId}
```

Khuyến nghị:

- Nếu order có nhiều product, tránh deadlock bằng cách sort product id trước khi acquire nhiều lock.
- Tốt hơn ở giai đoạn đầu: dùng lock theo order hoặc transaction DB với row lock, tùy implementation hiện tại.
- Nếu dùng nhiều lock theo product, phải release theo thứ tự ngược.

### 10.4. Reserve stock với lock

Luồng đề xuất:

```text
OrderCreatedConsumer
  -> Build lock key
  -> Try acquire lock với timeout ngắn
  -> Nếu không acquire: retry message hoặc publish failure có lý do rõ
  -> Trong lock:
       - Check idempotency
       - Load inventory từ DB
       - Validate available quantity
       - Update reserved quantity
       - Save stock reservation
       - Save outbox stock event
       - Commit transaction
  -> Release lock
```

### 10.5. Timeout đề xuất

| Setting | Giá trị đề xuất |
|---|---:|
| Lock wait timeout | 1-3 giây |
| Lock lease time | 10-30 giây |
| Redis operation timeout | 300-500 ms |
| Retry khi lock busy | Theo MassTransit retry policy, số lần thấp |

### 10.6. Khi không acquire được lock

Lựa chọn:

- Throw exception để MassTransit retry.
- Requeue/delay nếu đã có policy.
- Publish `stock.insufficient` là không phù hợp nếu nguyên nhân là lock busy, vì không phải thiếu hàng.

Khuyến nghị:

- Với lock busy, retry.
- Nếu retry hết, đưa vào error queue để điều tra.

### 10.7. Lock deliverables

- `IDistributedLockManager`.
- Implementation Redis lock hoặc thư viện lock.
- Product stock reservation sử dụng lock ở boundary phù hợp.
- Test lock timeout.
- Test hai request/event cạnh tranh không reserve vượt tồn kho.

---

## 11. Cache Invalidation Strategy

### 11.1. Invalidation theo command

Product:

- `CreateProductCommand`: remove/expire product list cache.
- `UpdateProductCommand`: remove detail key và list cache liên quan.
- `DeactivateProductCommand`: remove detail key và list cache liên quan.
- `AdjustInventoryCommand`: remove inventory key và detail key nếu detail chứa stock.

Order:

- `CreateOrderCommand`: không bắt buộc set cache.
- `CancelOrderCommand`: remove order detail key.
- `MarkOrderStockReservedCommand`: remove order detail key.
- `MarkOrderPaymentCompletedCommand`: remove order detail key.
- `MarkOrderPaymentFailedCommand`: remove order detail key.

Cart:

- `AddCartItem`: set cart key với TTL mới.
- `UpdateCartItem`: set cart key với TTL mới.
- `RemoveCartItem`: set/remove cart key.
- `ClearCart`: remove cart key.

### 11.2. Invalidation theo event

Nếu service khác có read model phụ thuộc event:

- `ProductUpdatedIntegrationEvent`: remove product cache liên quan.
- `StockReservedIntegrationEvent`: remove inventory cache.
- `OrderConfirmedIntegrationEvent`: remove order detail cache nếu consumer trong service liên quan.

Giai đoạn 5 chỉ cần event invalidation nếu event đó đã tồn tại và có consumer phù hợp. Không tạo event mới chỉ để invalidation nếu command-local invalidation đã đủ.

### 11.3. Tránh prefix scan nguy hiểm

Không khuyến nghị:

```text
KEYS ecom:local:product:list:*
```

Thay thế:

- TTL ngắn cho list cache.
- Versioned list key: `ecom:{env}:product:list:v{version}:{hash}`.
- Lưu danh sách key list đã tạo vào set để remove có kiểm soát.

Giai đoạn 5 khuyến nghị:

- Detail cache invalidate key cụ thể.
- List cache TTL ngắn.
- Chỉ thêm versioned key nếu thật sự cần.

---

## 12. Health Check Và Degradation

### 12.1. Redis health check

Mỗi service dùng Redis nên có health check:

```text
/health/live
/health/ready
```

Quy tắc:

- Liveness không nên fail chỉ vì Redis down nếu process vẫn sống.
- Readiness có thể degraded/fail tùy service phụ thuộc Redis đến mức nào.
- Với cache-only dependency, Redis down nên là degraded chứ không làm toàn bộ service chết.
- Với cart dependency, Redis down ảnh hưởng cart endpoint nhưng không nhất thiết ảnh hưởng product/order read từ DB.

### 12.2. Degradation behavior

Product cache:

- Redis down -> query DB trực tiếp -> log warning.

Order cache:

- Redis down -> query DB trực tiếp -> log warning.

Cart:

- Redis down -> trả 503 cho cart endpoints hoặc error rõ ràng.

Distributed lock:

- Redis down -> không được reserve stock nếu lock là cơ chế bảo vệ bắt buộc.
- Có thể fallback sang DB transaction/row lock nếu đã thiết kế rõ.
- Không silent bypass lock nếu sẽ gây oversell.

---

## 13. Configuration

### 13.1. Shared config

```json
{
  "Redis": {
    "Enabled": true,
    "ConnectionString": "localhost:6379",
    "InstanceName": "ecom-local",
    "DefaultTtlSeconds": 300,
    "OperationTimeoutMilliseconds": 500
  }
}
```

### 13.2. Product config

```json
{
  "Caching": {
    "ProductDetailTtlSeconds": 600,
    "ProductListTtlSeconds": 180,
    "InventoryTtlSeconds": 60
  },
  "DistributedLock": {
    "StockReservationWaitTimeoutSeconds": 3,
    "StockReservationLeaseSeconds": 20
  }
}
```

### 13.3. Order config

```json
{
  "Caching": {
    "OrderDetailTtlSeconds": 120
  },
  "Cart": {
    "TtlDays": 7,
    "MaxItems": 100,
    "MaxQuantityPerItem": 99
  }
}
```

### 13.4. Environment variables

Đề xuất:

```text
Redis__Enabled=true
Redis__ConnectionString=localhost:6379
Redis__InstanceName=ecom-local
Redis__DefaultTtlSeconds=300
Redis__OperationTimeoutMilliseconds=500
```

Không đưa password thật vào repository. Nếu Redis local chưa có password thì để connection string đơn giản.

---

## 14. Thứ Tự Triển Khai Chi Tiết

### Bước 1 - Rà soát baseline

Việc làm:

- Build solution.
- Chạy Product API và Order API.
- Kiểm tra Product query đang trả đúng dữ liệu từ DB.
- Kiểm tra reserve stock hiện tại hoạt động.
- Kiểm tra Gateway route còn hoạt động.

Output mong đợi:

- Baseline sạch trước khi thêm Redis.
- Có endpoint cụ thể để đo cache hit/miss.

### Bước 2 - Chạy Redis local

Việc làm:

- Start Redis bằng Docker hoặc compose.
- Kiểm tra `redis-cli ping`.
- Cập nhật `.env.example` nếu thiếu Redis config.
- Cập nhật local setup nếu cần.

Output mong đợi:

- Redis chạy tại port `6379`.
- Developer biết cách start/stop Redis.

### Bước 3 - Tạo Shared Redis abstraction

Việc làm:

- Tạo/cập nhật `ECommerceMS.Shared.Infrastructure`.
- Thêm Redis options.
- Thêm `ICacheService`.
- Thêm `RedisCacheService`.
- Thêm key builder.
- Thêm DI extension `AddRedisInfrastructure`.

Output mong đợi:

- Product/Order có thể inject cache abstraction.
- Không có service nào gọi trực tiếp Redis connection ở Application layer.

### Bước 4 - Thêm Redis health check

Việc làm:

- Đăng ký health check Redis ở service dùng Redis.
- Phân biệt liveness/readiness nếu kiến trúc hiện tại có.
- Log trạng thái Redis khi startup.

Output mong đợi:

- `/health/ready` thể hiện Redis status.
- Redis down được báo rõ.

### Bước 5 - Implement Product detail cache

Việc làm:

- Bọc `GetProductByIdQuery` bằng cache-aside.
- Tạo key `product:detail`.
- Set TTL.
- Log hit/miss.
- Invalidate khi update/deactivate product.

Output mong đợi:

- Request đầu cache miss, request sau cache hit.
- Update product không trả dữ liệu cũ.

### Bước 6 - Implement Product list cache

Việc làm:

- Tạo filter hash.
- Cache paged result.
- TTL ngắn.
- Invalidate hoặc rely TTL cho list cache theo quyết định đã ghi.

Output mong đợi:

- Product list query có cache.
- Không dùng raw query string dài làm key.

### Bước 7 - Implement inventory cache nếu phù hợp

Việc làm:

- Chỉ cache endpoint đọc inventory.
- Không dùng cache cho reserve stock decision.
- TTL ngắn.
- Invalidate khi inventory đổi.

Output mong đợi:

- Inventory read endpoint có cache an toàn.
- Reserve stock vẫn đọc DB/transaction.

### Bước 8 - Implement Order detail cache

Việc làm:

- Bọc `GetOrderByIdQuery`.
- Đảm bảo authorization không bị bypass.
- Invalidate khi order status thay đổi do command/event.

Output mong đợi:

- Order detail có cache.
- Sau payment/stock event, order detail không stale.

### Bước 9 - Implement cart/session Redis

Việc làm:

- Tạo cart model.
- Tạo cart service abstraction.
- Implement Redis cart storage.
- Tạo endpoints cart nếu scope cho phép.
- Clear cart sau khi tạo order thành công.

Output mong đợi:

- User có thể add/update/remove/get cart.
- Cart có TTL.
- Cart không reserve stock.

### Bước 10 - Implement distributed lock abstraction

Việc làm:

- Tạo `IDistributedLockManager`.
- Chọn implementation bằng thư viện hoặc Redis command atomic.
- Thiết lập wait timeout và lease time.
- Log acquire/release/failure.

Output mong đợi:

- Có thể acquire/release lock bằng key.
- Lock không giữ vô hạn.

### Bước 11 - Áp dụng lock cho reserve stock

Việc làm:

- Xác định lock scope: theo order hoặc product.
- Đặt lock quanh critical section reserve stock.
- Giữ transaction DB bên trong flow phù hợp.
- Không publish event trước commit.
- Xử lý lock busy bằng retry/error.

Output mong đợi:

- Concurrent reserve không làm reserved quantity vượt available quantity.
- Lock busy có log và behavior rõ ràng.

### Bước 12 - Bổ sung logging cache/lock

Việc làm:

- Log cache hit/miss/set/remove.
- Log lock acquired/released/timeout.
- Gắn `CorrelationId`, `OrderId`, `ProductId` nếu có.

Output mong đợi:

- Có thể debug cache behavior qua log.
- Có thể trace reserve stock cạnh tranh.

### Bước 13 - Viết test tự động trọng tâm

Việc làm:

- Unit test key builder.
- Unit test cache decorator/query handler.
- Unit test cart service.
- Unit test lock behavior nếu abstraction mock được.
- Integration test Redis nếu có Testcontainers.

Output mong đợi:

- `dotnet test` pass.
- Test bảo vệ invalidation và cart behavior.

### Bước 14 - Manual verification

Việc làm:

- Test cache miss/hit.
- Test invalidation sau update product.
- Test Redis down fallback.
- Test cart TTL/clear.
- Test concurrent reserve stock.

Output mong đợi:

- Checklist nghiệm thu có kết quả rõ.
- Không có stale data nghiêm trọng.

### Bước 15 - Cập nhật tài liệu

Việc làm:

- Cập nhật local setup Redis.
- Cập nhật key naming convention.
- Cập nhật runbook lỗi Redis.
- Ghi chú cache policy trong architecture notes.

Output mong đợi:

- Developer mới biết Redis dùng cho việc gì và không dùng cho việc gì.

---

## 15. Test Plan

### 15.1. Unit tests

Key builder:

- Product detail key đúng format.
- Product list hash ổn định với cùng filter.
- Cart key không chứa email/token.
- Lock key có prefix `lock`.

Cache service:

- `GetOrSetAsync` gọi factory khi miss.
- `GetOrSetAsync` không gọi factory khi hit.
- `RemoveAsync` xóa đúng key.
- Serialization/deserialization đúng DTO.

Product:

- Get product detail miss -> query DB -> set cache.
- Get product detail hit -> không query DB.
- Update product -> remove cache.
- Product list filter khác nhau tạo key khác nhau.

Order:

- Get order detail hit không bypass authorization.
- Payment event invalidates order cache.
- Cancel order invalidates order cache.

Cart:

- Add item mới.
- Update quantity.
- Remove item.
- Clear cart.
- Reject quantity không hợp lệ.
- TTL được set khi save cart.

Lock:

- Acquire thành công.
- Timeout khi lock đang bị giữ.
- Release đúng owner.
- Không release lock của owner khác.

### 15.2. Integration tests nên có

Nếu dùng Testcontainers:

- Redis container start được.
- Cache set/get/remove hoạt động với Redis thật.
- Cart lưu và đọc lại đúng.
- Lock contention giữa hai task chỉ cho một task vào critical section.
- Product cache hit/miss với API hoặc handler.

### 15.3. Manual test cache miss/hit

Điều kiện:

- Redis chạy.
- Product A tồn tại.

Các bước:

1. Clear Redis key product detail.
2. Gọi `GET /api/products/{id}` qua Gateway.
3. Kiểm tra log cache miss.
4. Gọi lại endpoint.
5. Kiểm tra log cache hit.

Kết quả pass:

- Response giống nhau.
- Request thứ hai không query DB nếu log/query instrumentation cho thấy cache hit.

### 15.4. Manual test invalidation

Các bước:

1. Gọi product detail để tạo cache.
2. Update product name/price.
3. Gọi lại product detail.
4. Kiểm tra dữ liệu mới được trả về.

Kết quả pass:

- Cache cũ bị remove hoặc không còn được dùng.
- Không trả product data cũ sau update.

### 15.5. Manual test Redis unavailable

Product/Order read:

1. Stop Redis.
2. Gọi product detail.
3. Gọi order detail.

Kết quả pass:

- Endpoint vẫn trả dữ liệu từ DB nếu Redis chỉ là cache.
- Log warning Redis unavailable.

Cart:

1. Stop Redis.
2. Gọi cart endpoint.

Kết quả pass:

- Trả lỗi rõ ràng, ví dụ 503.
- Không crash service.

Reserve stock:

1. Stop Redis nếu lock bắt buộc.
2. Trigger reserve stock.

Kết quả pass:

- Không silent bypass lock nếu có nguy cơ oversell.
- Message retry/error rõ ràng.

### 15.6. Manual test concurrent reserve stock

Điều kiện:

- Product A available quantity = 1.

Các bước:

1. Gửi hai order/event gần như đồng thời cùng mua Product A quantity 1.
2. Theo dõi Product Service log.
3. Kiểm tra inventory và stock reservation.

Kết quả pass:

- Tối đa một order reserve thành công.
- Order còn lại nhận insufficient hoặc retry/failure hợp lệ.
- `QuantityReserved` không vượt `QuantityOnHand`.

---

## 16. Observability Tối Thiểu

### 16.1. Log fields

Cache log cần có:

- `CorrelationId`
- `CacheKey` hoặc key đã mask nếu có thông tin nhạy cảm
- `CacheOperation`
- `CacheHit`
- `TtlSeconds`
- `ServiceName`

Lock log cần có:

- `CorrelationId`
- `LockKey`
- `LockOwner`
- `WaitTimeout`
- `LeaseTime`
- `Acquired`
- `ElapsedMilliseconds`

### 16.2. Metrics chuẩn bị

Nếu metrics chưa có hạ tầng đầy đủ, vẫn nên chuẩn bị abstraction hoặc log event để sau này map sang Prometheus:

- `cache_hit_total`
- `cache_miss_total`
- `cache_set_total`
- `cache_remove_total`
- `cache_error_total`
- `distributed_lock_acquired_total`
- `distributed_lock_timeout_total`
- `distributed_lock_error_total`

### 16.3. Trace

Khi giai đoạn 9 có OpenTelemetry đầy đủ, cache/lock nên xuất hiện trong trace. Giai đoạn 5 chỉ cần:

- Giữ correlation id.
- Log duration của Redis operation.
- Không nuốt exception âm thầm.

---

## 17. Security Và Data Privacy

### 17.1. Không cache

- JWT/access token/refresh token.
- Password, OTP, secret.
- Payment card data hoặc thông tin thanh toán nhạy cảm.
- Dữ liệu cá nhân không cần thiết.
- Full request header.

### 17.2. Cart/session

- Cart chỉ lưu dữ liệu cần cho UI.
- Không lưu token trong cart.
- Không dùng email làm key trực tiếp.
- Nếu anonymous cart được thêm sau này, session id phải đủ ngẫu nhiên.

### 17.3. Authorization

- Cache không được bypass authorization.
- Với dữ liệu per-user, key nên chứa user/customer id hoặc validate quyền trước khi trả response.
- Admin cache và customer cache không dùng chung response nếu field khác nhau.

---

## 18. Performance Và TTL Policy

### 18.1. TTL mặc định

| Loại dữ liệu | TTL |
|---|---:|
| Product detail | 10 phút |
| Product list | 3 phút |
| Inventory read | 30-60 giây |
| Order detail active | 1-2 phút |
| Order detail terminal | 5-10 phút, nếu đã phân biệt trạng thái |
| Cart authenticated | 7 ngày |
| Lock lease | 10-30 giây |

### 18.2. Cache stampede

Rủi ro:

- Nhiều request cùng miss một key và cùng query DB.

Kiểm soát giai đoạn 5:

- TTL vừa phải.
- Có thể dùng lock nhẹ cho hot key nếu thật sự cần.
- Chưa tối ưu quá sớm nếu traffic demo thấp.

### 18.3. Payload size

Quy tắc:

- Không cache payload quá lớn.
- Với product list, page size cần giới hạn.
- Không cache full catalog trong một key.
- Nếu DTO lớn, cân nhắc chỉ cache detail.

---

## 19. Deliverables Tổng Hợp

### 19.1. Infrastructure deliverables

- Redis local chạy được.
- Redis config trong appsettings/env.
- Shared Redis cache abstraction.
- Shared distributed lock abstraction.
- Key builder hoặc naming convention được enforce.
- Redis health check.

### 19.2. Product deliverables

- Product detail cache.
- Product list cache.
- Product cache invalidation.
- Optional inventory read cache.
- Distributed lock cho reserve stock.

### 19.3. Order deliverables

- Order detail cache nếu phù hợp.
- Order cache invalidation khi status đổi.
- Cart/session Redis tối thiểu nếu scope được chọn trong Order API/module.

### 19.4. Testing deliverables

- Unit tests cho key builder/cache/cart/lock logic.
- Manual test cache hit/miss.
- Manual test invalidation.
- Manual test Redis down.
- Manual test concurrent reserve stock.

### 19.5. Documentation deliverables

- Redis setup local.
- Redis key convention.
- Cache policy theo service.
- Lock policy và timeout.
- Runbook lỗi Redis thường gặp.

---

## 20. Definition Of Done

Giai đoạn 5 hoàn thành khi đạt tất cả điều kiện sau:

- `dotnet build ECommerceMS.sln` pass.
- Redis local chạy được và `PING` trả `PONG`.
- Product detail cache hoạt động theo cache-aside.
- Product list cache hoạt động hoặc có lý do rõ nếu defer.
- Product cache được invalidate khi product thay đổi.
- Order detail cache không trả stale status sau event quan trọng nếu được implement.
- Cart/session Redis có TTL và không reserve stock.
- Distributed lock có timeout, owner token và release an toàn.
- Reserve stock không oversell trong test cạnh tranh cơ bản.
- Redis down không làm sập Product/Order read nếu Redis chỉ đóng vai trò cache.
- Cart endpoint trả lỗi rõ nếu Redis down.
- Lock failure không bị silent bypass ở flow reserve stock.
- Key Redis có prefix `ecom:{env}:{service}:{purpose}` hoặc tương đương.
- Không cache dữ liệu nhạy cảm.
- Log có cache hit/miss, invalidation và lock acquisition.
- Test tự động trọng tâm pass.
- Manual verification được ghi nhận cho cache hit/miss, invalidation, Redis down và concurrent reserve.

---

## 21. Checklist Nghiệm Thu

### 21.1. Redis setup

- [ ] Redis chạy local.
- [ ] Port không trùng.
- [ ] Connection string lấy từ config.
- [ ] Không hardcode connection string trong code.
- [ ] Health check Redis hoạt động.

### 21.2. Key convention

- [ ] Key có prefix environment.
- [ ] Key có service segment.
- [ ] Key có purpose rõ ràng.
- [ ] Không dùng email/token trong key.
- [ ] Product list dùng filter hash.
- [ ] Lock key có prefix `lock`.

### 21.3. Cache

- [ ] Product detail cache miss hoạt động.
- [ ] Product detail cache hit hoạt động.
- [ ] Product list cache có TTL ngắn.
- [ ] Product update invalidate cache.
- [ ] Order status change invalidate order cache nếu có.
- [ ] Redis down fallback DB cho query cache-only.
- [ ] Không cache EF tracked entity.

### 21.4. Cart/session

- [ ] Add item hoạt động.
- [ ] Update quantity hoạt động.
- [ ] Remove item hoạt động.
- [ ] Clear cart hoạt động.
- [ ] TTL cart được set.
- [ ] Cart không reserve stock.
- [ ] Checkout re-validate product/stock qua flow nghiệp vụ.

### 21.5. Distributed lock

- [ ] Lock acquire thành công.
- [ ] Lock timeout có xử lý rõ.
- [ ] Lock có lease time.
- [ ] Release lock đúng owner.
- [ ] Reserve stock dùng lock hoặc có giải pháp concurrency tương đương.
- [ ] Concurrent reserve không vượt tồn kho.

### 21.6. Observability

- [ ] Log cache hit/miss.
- [ ] Log cache invalidation.
- [ ] Log lock acquired/released/timeout.
- [ ] Log có correlation id.
- [ ] Không log dữ liệu nhạy cảm.

---

## 22. Rủi Ro Và Cách Kiểm Soát

### 22.1. Cache stale làm sai nghiệp vụ

Rủi ro:

- Product price/name/inventory cũ được trả sau update.
- Order status cũ được trả sau payment event.

Kiểm soát:

- Invalidate key cụ thể sau command/event.
- TTL ngắn cho dữ liệu biến động.
- Không dùng cache để quyết định reserve stock.

### 22.2. Redis thành source of truth không chủ ý

Rủi ro:

- Cart/session tạm bị dùng như order thật.
- Cache chứa dữ liệu không còn trong DB.

Kiểm soát:

- Ghi rõ source of truth theo use case.
- Checkout luôn tạo order qua database và event flow.
- Redis chỉ giữ transient/read model data.

### 22.3. Distributed lock gây deadlock logic

Rủi ro:

- Lock không timeout.
- Acquire nhiều lock không theo thứ tự.
- Release sai owner.

Kiểm soát:

- Luôn set lease time.
- Sort product ids nếu acquire nhiều lock.
- Dùng owner token.
- Ưu tiên thư viện lock đáng tin cậy.

### 22.4. Redis down làm sập hệ thống

Rủi ro:

- Cache exception bubble lên API.
- Cart/lock dependency không có behavior rõ.

Kiểm soát:

- Cache-only use case fallback DB.
- Cart trả lỗi rõ.
- Lock không silent bypass nếu ảnh hưởng tính đúng đắn.
- Health check/readiness rõ ràng.

### 22.5. Key explosion

Rủi ro:

- Product list tạo quá nhiều key theo filter.
- Không có TTL.

Kiểm soát:

- Giới hạn filter/page size.
- TTL bắt buộc cho list cache.
- Không cache mọi query tùy ý.

### 22.6. Dữ liệu nhạy cảm trong cache

Rủi ro:

- Lưu token/payment data/PII không cần thiết.

Kiểm soát:

- Chỉ cache DTO đã duyệt field.
- Review DTO trước khi cache.
- Không log full cached payload.

---

## 23. Thứ Tự Ưu Tiên Khi Thiếu Thời Gian

Nếu không đủ thời gian, ưu tiên:

1. Redis setup và shared cache abstraction.
2. Product detail cache và invalidation.
3. Distributed lock cho reserve stock.
4. Manual test concurrent reserve stock.
5. Product list cache TTL ngắn.
6. Order detail cache.
7. Cart/session Redis.
8. Metrics nâng cao.

Không được bỏ qua:

- Key prefix convention.
- Redis unavailable behavior.
- Lock timeout.
- Không cache dữ liệu nhạy cảm.
- Không dùng cache làm source of truth.

---

## 24. Cổng Kiểm Soát Sau Giai Đoạn 5 - Cache Và Concurrency Gate

Trước khi chuyển sang giai đoạn 6 Elasticsearch, phải trả lời được:

- Product detail/list cache có làm sai dữ liệu sau update không?
- Order detail cache có trả trạng thái cũ sau payment/stock event không?
- Redis down có behavior rõ cho từng endpoint không?
- Cart/session có TTL và không reserve stock không?
- Reserve stock có còn nguy cơ oversell trong test cạnh tranh cơ bản không?
- Lock có timeout và release an toàn không?
- Key Redis có prefix rõ theo service/environment không?
- Có dữ liệu nhạy cảm nào bị cache/log không?
- Developer mới có thể start Redis và test cache theo tài liệu không?

Nếu các câu trả lời trên chưa rõ, chưa nên chuyển sang Elasticsearch vì search/read model sẽ làm bài toán consistency phức tạp hơn.

