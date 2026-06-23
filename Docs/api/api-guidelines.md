# API Guidelines

## Base Path

API dùng format:

```text
/api/{resource}
```

Resource dùng số nhiều, chữ thường:

```text
/api/orders
/api/products
/api/payments
```

## Endpoint Mục Tiêu

```text
POST   /api/orders
GET    /api/orders/{id}
GET    /api/orders?pageNumber=1&pageSize=20
PATCH  /api/orders/{id}/cancel
GET    /api/products
GET    /api/products/{id}
GET    /api/products/search?q=laptop
POST   /api/payments/checkout
GET    /api/payments/{id}
GET    /health/live
GET    /health/ready
```

## Pagination, Sorting Và Filtering

| Mục | Query string |
|---|---|
| Page number | `pageNumber` |
| Page size | `pageSize` |
| Sort field | `sortBy` |
| Sort direction | `sortDirection` |
| Search query | `q` |

`pageSize` phải có giới hạn tối đa khi implement.

## Error Response

Ưu tiên dùng `ProblemDetails` hoặc envelope thống nhất. Không trả stack trace cho client.

Mapping mục tiêu:

| Loại lỗi | HTTP status |
|---|---:|
| Validation | 400 |
| Unauthorized | 401 |
| Forbidden | 403 |
| Not found | 404 |
| Conflict | 409 |
| External dependency unavailable | 503 |
| Unexpected | 500 |

## Health Check

- `/health/live`: service process còn sống.
- `/health/ready`: service sẵn sàng nhận traffic và dependency quan trọng đang đạt yêu cầu.

## Security

- Endpoint ghi dữ liệu phải yêu cầu auth, trừ khi có quyết định khác trong roadmap.
- Admin endpoint phải kiểm tra role.
- Không log access token, refresh token hoặc thông tin nhạy cảm.

