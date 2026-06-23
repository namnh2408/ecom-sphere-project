# Naming Conventions

## Solution, Project Và Namespace

| Loại | Convention | Ví dụ |
|---|---|---|
| Solution | `ECommerceMS` | `ECommerceMS.sln` |
| API Gateway | `ECommerceMS.ApiGateway` | `src/ApiGateway/ECommerceMS.ApiGateway` |
| Service Domain | `ECommerceMS.{Service}.Domain` | `ECommerceMS.Order.Domain` |
| Service Application | `ECommerceMS.{Service}.Application` | `ECommerceMS.Order.Application` |
| Service Infrastructure | `ECommerceMS.{Service}.Infrastructure` | `ECommerceMS.Order.Infrastructure` |
| Service API | `ECommerceMS.{Service}.API` | `ECommerceMS.Order.API` |
| Shared Contracts | `ECommerceMS.Shared.Contracts` | `ECommerceMS.Shared.Contracts` |
| Shared Infrastructure | `ECommerceMS.Shared.Infrastructure` | `ECommerceMS.Shared.Infrastructure` |
| Shared Domain | `ECommerceMS.Shared.Domain` | `ECommerceMS.Shared.Domain` |

Namespace phải khớp tên project. Không dùng namespace viết tắt như `EMS` hoặc `Ecom`.

## Branch

| Loại branch | Format | Ví dụ |
|---|---|---|
| Feature | `feature/{phase}-{short-name}` | `feature/p1-order-service` |
| Fix | `fix/{short-name}` | `fix/order-validation` |
| Chore | `chore/{short-name}` | `chore/setup-solution` |
| Docs | `docs/{short-name}` | `docs/phase-0-plan` |
| Release | `release/{version}` | `release/v0.1.0` |

Tên branch dùng chữ thường, dấu gạch ngang và không dùng dấu tiếng Việt.

## Commit

Format:

```text
<type>(<scope>): <summary>
```

Các `type` hợp lệ:

- `feat`
- `fix`
- `docs`
- `test`
- `refactor`
- `chore`
- `ci`
- `infra`

Ví dụ:

```text
docs(phase-0): add local setup runbook
chore(solution): initialize ecommerce solution structure
feat(order): add create order command
```

## Docker Image

Format:

```text
ecommerce/{service-name}:{tag}
```

Ví dụ:

```text
ecommerce/api-gateway:local
ecommerce/order-service:local
ecommerce/product-service:local
ecommerce/payment-service:local
ecommerce/notification-service:local
```

Tag chuẩn:

- `local`
- `latest`
- `{git-sha}`

## Kafka Topic

Format:

```text
{bounded-context}.{event-name}
```

Topic mục tiêu:

```text
order.created
order.confirmed
order.cancelled
payment.completed
payment.failed
stock.reserved
stock.insufficient
notification.requested
```

Không đặt topic theo tên consumer. Topic phản ánh event đã xảy ra trong domain.

## Redis Key

Format:

```text
ecommerce:{service}:{type}:{id}
```

Ví dụ:

```text
ecommerce:product:detail:{productId}
ecommerce:product:list:{hash}
ecommerce:order:detail:{orderId}
ecommerce:cart:user:{userId}
ecommerce:lock:stock:{productId}
```

Cache key phải có TTL. Lock key phải có timeout.

