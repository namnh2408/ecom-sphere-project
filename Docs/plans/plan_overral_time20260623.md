# Plan Triển Khai E-Commerce Microservice

> Nguồn tham chiếu:
> - `Docs/ecommerce-microservice-roadmap.md`
> - `Docs/microservice_full_architecture.svg`
>
> Ngày lập plan: 2026-06-23  
> Thời lượng mục tiêu: 18 tuần  
> Mục tiêu: xây dựng hệ thống E-Commerce theo kiến trúc Microservice bằng .NET, có khả năng chạy local bằng Docker Compose, triển khai Kubernetes, quan sát được hệ thống, kiểm thử được và có CI/CD cơ bản.

---

## 1. Nguyên Tắc Triển Khai

Plan này dùng để biến roadmap tổng quát thành lộ trình thực hiện có thể tuân thủ theo từng giai đoạn. Mỗi giai đoạn phải có đầu ra cụ thể, tiêu chí nghiệm thu rõ ràng và không chuyển sang giai đoạn tiếp theo khi các tiêu chí bắt buộc chưa đạt.

Các nguyên tắc kiến trúc cần giữ xuyên suốt:

- API Gateway YARP là điểm vào duy nhất cho client.
- Mỗi service sở hữu database riêng, không truy cập trực tiếp database của service khác.
- Giao tiếp nghiệp vụ liên service ưu tiên thông qua Kafka event và MassTransit.
- Các service phải có khả năng deploy độc lập, scale độc lập và fail độc lập.
- Shared project chỉ chứa contracts, base abstractions và helper hạ tầng thật sự dùng chung; không đưa business logic riêng của service vào shared.
- Mọi luồng quan trọng phải có logging, tracing, metrics và health check.
- Các thay đổi lớn phải đi kèm test hoặc checklist nghiệm thu tương ứng.

---

## 2. Timeline Tổng Thể

| Giai đoạn | Thời gian | Trọng tâm | Kết quả chính |
|---|---:|---|---|
| 0 | Tuần 0 | Chuẩn bị | Convention, môi trường, cấu trúc repo |
| 1 | Tuần 1-2 | Clean Architecture | Order Service hoàn chỉnh nền tảng |
| 2 | Tuần 3-4 | Gateway & Auth | YARP, Keycloak, JWT, rate limit |
| 3 | Tuần 5-6 | Event-driven | Kafka, MassTransit, contracts, Outbox |
| 4 | Tuần 7-8 | Business services | Product, Payment, Notification theo luồng nghiệp vụ |
| 5 | Tuần 9 | Redis | Cache, session/cart, distributed lock |
| 6 | Tuần 10 | Elasticsearch | Search, indexing, query tối ưu |
| 7 | Tuần 11 | SignalR | Real-time notification và order tracking |
| 8 | Tuần 12 | Hangfire | Background jobs, retry, dashboard |
| 9 | Tuần 13 | Observability | Serilog, Seq, OpenTelemetry, Jaeger, Prometheus, Grafana |
| 10 | Tuần 14 | Docker Compose | Chạy full stack local |
| 11 | Tuần 15-16 | Kubernetes | Deployment, Service, Ingress, Secret, HPA |
| 12 | Tuần 17-18 | Testing & CI/CD | Unit, integration, pipeline, README, demo |

---

## 3. Giai Đoạn Triển Khai Chi Tiết

### Giai đoạn 0: Chuẩn Bị Môi Trường, Convention Và Repository Structure

**Mục tiêu**

Chuẩn hóa môi trường phát triển, cấu trúc thư mục, coding convention và nguyên tắc làm việc trước khi tạo service.

**Công việc cần làm**

- Xác nhận phiên bản .NET 8 SDK, Docker Desktop, Git, IDE, PostgreSQL client, kubectl, Kind hoặc Minikube.
- Tạo cấu trúc thư mục gốc: `src`, `tests`, `infra`, `docs`, `.github`.
- Định nghĩa naming convention cho solution, project, namespace, branch, commit message và Docker image.
- Tạo file guideline ngắn cho coding style, API style, logging style, exception handling và configuration.
- Xác định môi trường local mặc định: port mapping, connection string mẫu, tên database, tên Kafka topic, tên Redis key prefix.

**Thứ tự triển khai**

1. Chuẩn hóa tooling.
2. Tạo repository skeleton.
3. Chốt convention.
4. Kiểm tra repo sạch và có tài liệu setup ban đầu.

**Deliverables**

- Cấu trúc thư mục nền.
- Tài liệu convention.
- Checklist môi trường local.

**Tiêu chí hoàn thành**

- Developer mới có thể clone repo, đọc hướng dẫn và biết cần cài gì.
- Cấu trúc repo khớp với roadmap.
- Không có quyết định naming hoặc port mapping còn mơ hồ.

**Rủi ro cần kiểm soát**

- Dùng version tool không đồng nhất giữa các máy.
- Convention thay đổi muộn làm phát sinh rename hàng loạt.
- Cấu trúc shared bị lạm dụng ngay từ đầu.

---

### Giai đoạn 1: Nền Móng Clean Architecture Và Order Service

**Mục tiêu**

Dựng service đầu tiên làm chuẩn mẫu cho các service còn lại, gồm Domain, Application, Infrastructure và API.

**Công việc cần làm**

- Tạo solution `ECommerceMS` và các project Order: Domain, Application, Infrastructure, API.
- Thiết kế Order aggregate, OrderItem, Money, Address, OrderStatus và domain events.
- Implement CQRS với MediatR cho CreateOrder, CancelOrder, GetOrderById, GetOrders.
- Thêm FluentValidation, Mapster, pipeline behavior cho validation và logging.
- Thêm EF Core, PostgreSQL migration, Repository pattern và Unit of Work.
- Tạo exception middleware, response format thống nhất và Scalar/OpenAPI UI.

**Thứ tự triển khai**

1. Tạo project structure và dependency direction.
2. Implement Domain model không phụ thuộc framework.
3. Implement Application commands/queries.
4. Implement Infrastructure persistence.
5. Expose API endpoints.
6. Chạy migration và kiểm thử thủ công qua Scalar.

**Deliverables**

- Order Service chạy được độc lập.
- Database `order_db` có migration ban đầu.
- API tạo, hủy, lấy danh sách và lấy chi tiết đơn hàng.

**Tiêu chí hoàn thành**

- Dependency chỉ đi từ API/Infrastructure vào Application/Domain, Domain không phụ thuộc layer ngoài.
- CreateOrder tính tổng tiền đúng và phát sinh domain event.
- CancelOrder tuân thủ state machine.
- API trả lỗi validation rõ ràng.

**Rủi ro cần kiểm soát**

- Đưa EF Core attribute hoặc infrastructure concern vào Domain.
- Handler chứa quá nhiều logic nghiệp vụ thay vì aggregate.
- Thiếu transaction boundary khi ghi order.

---

### Giai đoạn 2: API Gateway YARP Và Authentication Với Keycloak

**Mục tiêu**

Thiết lập entrypoint duy nhất cho client, route request đến service phù hợp và bảo vệ endpoint bằng OAuth2/OIDC.

**Công việc cần làm**

- Tạo `ECommerceMS.ApiGateway` dùng YARP.
- Cấu hình route cho Order, Product, Payment, Notification và health endpoints.
- Tích hợp Keycloak local bằng Docker.
- Tạo realm `ecommerce`, client `ecommerce-api`, role `customer`, `admin`.
- Thêm JWT Bearer authentication cho Gateway và các API service cần bảo vệ.
- Cấu hình rate limiting tại Gateway.
- Áp dụng Polly cho các HTTP client nội bộ nếu có downstream call đồng bộ.

**Thứ tự triển khai**

1. Tạo Gateway và route đến Order Service.
2. Chạy Keycloak local.
3. Cấu hình JWT validation.
4. Bảo vệ endpoint theo role.
5. Kiểm tra 401, 403 và happy path có token.
6. Mở rộng route cho các service dự kiến.

**Deliverables**

- Gateway chạy được tại một port thống nhất.
- Keycloak realm export cho local dev.
- Endpoint protected yêu cầu token hợp lệ.

**Tiêu chí hoàn thành**

- Client không gọi trực tiếp service port trong luồng chuẩn.
- Request thiếu token trả 401.
- Request sai role trả 403.
- Gateway route đúng đến service tương ứng.
- Rate limit có thể kiểm chứng bằng test thủ công.

**Rủi ro cần kiểm soát**

- Mỗi service tự định nghĩa auth khác nhau.
- Hardcode Keycloak URL hoặc secret trong code.
- Gateway trở thành nơi chứa business logic.

---

### Giai đoạn 3: Kafka, MassTransit, Event Contracts Và Outbox

**Mục tiêu**

Thiết lập giao tiếp bất đồng bộ giữa các service, đảm bảo event được publish đáng tin cậy và consumer xử lý idempotent.

**Công việc cần làm**

- Tạo `ECommerceMS.Shared.Contracts` cho event contracts.
- Định nghĩa topic chính: `order.created`, `order.confirmed`, `order.cancelled`, `payment.completed`, `payment.failed`, `stock.reserved`, `stock.insufficient`.
- Cấu hình Kafka và MassTransit.
- Implement Outbox pattern cho event phát sinh từ transaction database.
- Tạo consumer mẫu cho Product, Payment và Notification.
- Định nghĩa retry policy, dead-letter topic hoặc error topic.
- Bổ sung correlation id, causation id và timestamp cho event.

**Thứ tự triển khai**

1. Tạo contracts và naming convention cho events.
2. Cấu hình Kafka local.
3. Publish `order.created` từ Order Service qua Outbox.
4. Tạo consumer nhận event và log payload.
5. Bổ sung retry, error handling và idempotency.
6. Viết integration test cơ bản cho publish/consume.

**Deliverables**

- Event contracts dùng chung.
- Kafka chạy local.
- Order Service publish event đáng tin cậy.
- Consumer mẫu xử lý được message.

**Tiêu chí hoàn thành**

- Event không bị mất khi transaction database thành công.
- Consumer xử lý lại cùng một message không tạo side effect trùng.
- Event có version hoặc khả năng mở rộng schema.
- Log có correlation id xuyên suốt publish/consume.

**Rủi ro cần kiểm soát**

- Coupling bằng cách share DTO nội bộ thay vì public event contract.
- Publish event trực tiếp trước khi transaction database commit.
- Consumer không idempotent gây trừ tồn kho hoặc gửi notification trùng.

---

### Architecture Gate Sau Giai Đoạn 1-3

Không tiếp tục mở rộng service nếu các điều kiện sau chưa đạt:

- Clean Architecture của Order Service đúng dependency direction.
- Gateway là entrypoint chuẩn cho request từ client.
- Kafka event flow tối thiểu hoạt động.
- Outbox đã được chứng minh bằng test hoặc kiểm thử thủ công có log.
- Shared contracts không chứa business logic riêng của service.

---

### Giai đoạn 4: Product, Payment, Notification Service Theo Luồng Nghiệp Vụ

**Mục tiêu**

Hoàn thiện các business service chính để tạo được luồng đặt hàng cơ bản từ order đến giữ hàng, thanh toán và thông báo.

**Công việc cần làm**

- Product Service: catalog, stock, reserve stock, release stock, publish `stock.reserved` hoặc `stock.insufficient`.
- Payment Service: checkout, payment record, payment status, publish `payment.completed` hoặc `payment.failed`.
- Notification Service: nhận event, lưu lịch sử notification, gửi email giả lập và chuẩn bị SignalR.
- Identity Service hoặc Identity area: giữ cấu hình Keycloak, role, claim mapping và auth helper.
- Đồng bộ trạng thái Order theo event từ Product và Payment.
- Tài liệu hóa business flow chính: create order, reserve stock, checkout, notify.

**Thứ tự triển khai**

1. Dựng skeleton Product theo mẫu Order.
2. Implement stock reservation.
3. Dựng Payment Service và luồng checkout giả lập.
4. Dựng Notification Service với MongoDB.
5. Nối event flow end-to-end.
6. Kiểm thử luồng thành công và thất bại.

**Deliverables**

- Product, Payment, Notification Service chạy được.
- Luồng đặt hàng cơ bản có event qua Kafka.
- Order state cập nhật theo kết quả stock/payment.

**Tiêu chí hoàn thành**

- Product không đọc database của Order.
- Payment không sửa trực tiếp trạng thái Order.
- Notification chỉ phản ứng theo event.
- Có kịch bản payment failed và stock insufficient.

**Rủi ro cần kiểm soát**

- Service gọi trực tiếp database của nhau để "cho nhanh".
- Luồng event thiếu bù trừ khi một bước thất bại.
- State machine của Order không nhất quán với event nhận được.

---

### Giai đoạn 5: Redis Cache, Session Và Distributed Lock

**Mục tiêu**

Tăng hiệu năng đọc, hỗ trợ cart/session và bảo vệ các đoạn critical section bằng distributed lock.

**Công việc cần làm**

- Cấu hình Redis connection dùng chung qua Shared Infrastructure.
- Áp dụng cache-aside cho Product list/detail và Order detail nếu phù hợp.
- Thiết kế key prefix theo service để tránh va chạm.
- Implement invalidation khi Product hoặc Order thay đổi.
- Dùng distributed lock cho reserve stock hoặc idempotent job quan trọng.
- Bổ sung metric cache hit/miss nếu đã có metrics nền.

**Thứ tự triển khai**

1. Tạo Redis abstraction tối giản.
2. Cache Product read model.
3. Thêm invalidation theo event hoặc command.
4. Thêm distributed lock cho stock reservation.
5. Kiểm thử cache miss, cache hit và cache invalidation.

**Deliverables**

- Redis chạy local.
- Product query có cache.
- Distributed lock bảo vệ reserve stock.

**Tiêu chí hoàn thành**

- Cache không trả dữ liệu cũ sau update quan trọng.
- Redis down không làm sập toàn bộ service nếu use case cho phép degrade.
- Lock có timeout, không giữ lock vô hạn.

**Rủi ro cần kiểm soát**

- Cache key không có prefix rõ ràng.
- Cache dữ liệu nhạy cảm không cần thiết.
- Lock dùng sai làm giảm throughput hoặc gây deadlock logic.

---

### Giai đoạn 6: Elasticsearch Cho Product Search Và Analytics

**Mục tiêu**

Tách nhu cầu search khỏi database giao dịch, hỗ trợ full-text search và filter sản phẩm.

**Công việc cần làm**

- Cấu hình Elasticsearch local.
- Thiết kế index `products` với fields cần search, filter và sort.
- Đồng bộ Product read model sang Elasticsearch qua event.
- Implement endpoint `/api/products/search?q=...`.
- Bổ sung reindex job hoặc command cho dữ liệu hiện có.
- Xử lý tình huống Elasticsearch tạm thời không sẵn sàng.

**Thứ tự triển khai**

1. Tạo index mapping.
2. Publish product change event.
3. Consumer cập nhật index.
4. Implement search API.
5. Thêm reindex flow.
6. Kiểm thử search, filter, sort.

**Deliverables**

- Elasticsearch index cho Product.
- Search API hoạt động qua Gateway.
- Reindex command/job cơ bản.

**Tiêu chí hoàn thành**

- Search không query trực tiếp database chính cho full-text.
- Product update phản ánh lên index trong thời gian chấp nhận được.
- Có cách phục hồi index khi dữ liệu lệch.

**Rủi ro cần kiểm soát**

- Coi Elasticsearch là source of truth.
- Không có cơ chế reindex.
- Mapping thay đổi nhưng không có kế hoạch migration index.

---

### Integration Gate Sau Giai Đoạn 4-6

Chỉ chuyển sang real-time/background/platform khi các điều kiện sau đạt:

- Luồng order -> stock -> payment -> notification chạy end-to-end.
- Event failure path được kiểm thử tối thiểu.
- Redis cache không phá vỡ tính đúng đắn nghiệp vụ.
- Elasticsearch có cơ chế đồng bộ và phục hồi index.
- Không có service truy cập database của service khác.

---

### Giai đoạn 7: SignalR Real-Time Order Tracking

**Mục tiêu**

Cung cấp cập nhật real-time cho client khi trạng thái đơn hàng hoặc notification thay đổi.

**Công việc cần làm**

- Tạo SignalR hub trong Notification Service.
- Xác thực kết nối WebSocket bằng JWT.
- Map user/customer id vào group tương ứng.
- Khi nhận event order/payment/stock, push thông báo đến group liên quan.
- Nếu scale nhiều instance, chuẩn bị Redis backplane.
- Tạo client test đơn giản hoặc script test WebSocket.

**Thứ tự triển khai**

1. Tạo hub endpoint `/hubs/orders`.
2. Thêm auth cho hub.
3. Join group theo customer/order.
4. Push message từ consumer.
5. Kiểm thử một client và nhiều client.
6. Bật Redis backplane khi chạy multi-instance.

**Deliverables**

- SignalR hub hoạt động.
- Client nhận cập nhật trạng thái đơn hàng.
- Notification history vẫn được lưu bền vững.

**Tiêu chí hoàn thành**

- User chỉ nhận notification thuộc quyền của mình.
- Mất kết nối không làm mất dữ liệu lịch sử.
- Real-time message có correlation id để trace.

**Rủi ro cần kiểm soát**

- Gửi message broadcast toàn hệ thống thay vì theo user/group.
- Không xác thực WebSocket.
- Phụ thuộc SignalR cho dữ liệu bắt buộc thay vì lưu notification history.

---

### Giai đoạn 8: Hangfire Background Jobs

**Mục tiêu**

Tách các tác vụ nền khỏi request path, hỗ trợ retry, scheduled job và dashboard quan sát.

**Công việc cần làm**

- Cấu hình Hangfire trong Notification hoặc Worker service phù hợp.
- Tạo recurring jobs: cleanup notification cũ, retry failed notification, reindex products nếu cần.
- Tạo fire-and-forget job cho gửi email giả lập.
- Bảo vệ Hangfire Dashboard bằng auth/role admin.
- Cấu hình retry policy và queue naming.

**Thứ tự triển khai**

1. Cấu hình Hangfire storage.
2. Tạo job gửi notification.
3. Tạo recurring cleanup/retry jobs.
4. Bảo vệ dashboard.
5. Kiểm thử retry và failed job.

**Deliverables**

- Hangfire Dashboard chạy local.
- Jobs quan trọng có retry.
- Scheduled jobs có lịch rõ ràng.

**Tiêu chí hoàn thành**

- Job thất bại được retry và có log.
- Dashboard không public không kiểm soát.
- Job idempotent hoặc có cơ chế chống xử lý trùng.

**Rủi ro cần kiểm soát**

- Job xử lý nghiệp vụ chính nhưng không idempotent.
- Dashboard lộ ra ngoài không auth.
- Retry vô hạn gây spam hoặc quá tải dependency.

---

### Giai đoạn 9: Observability, Logging, Tracing, Metrics Và Health Checks

**Mục tiêu**

Làm cho hệ thống quan sát được khi chạy nhiều service, giúp debug luồng request/event và phát hiện lỗi dependency.

**Công việc cần làm**

- Chuẩn hóa Serilog structured logging cho tất cả service.
- Gửi log local về Seq.
- Cấu hình OpenTelemetry tracing cho HTTP, EF Core, Kafka/MassTransit.
- Gửi trace về Jaeger.
- Expose Prometheus metrics.
- Tạo Grafana dashboard cơ bản.
- Thêm `/health/live` và `/health/ready` cho mỗi service.

**Thứ tự triển khai**

1. Tạo Shared observability extension.
2. Áp dụng cho Order, Product, Payment, Notification, Gateway.
3. Bổ sung correlation id middleware.
4. Cấu hình Seq và Jaeger trong Docker Compose.
5. Expose metrics và cấu hình Prometheus.
6. Tạo dashboard Grafana tối thiểu.

**Deliverables**

- Log có cấu trúc và correlation id.
- Trace end-to-end qua Gateway, service và Kafka.
- Metrics scrape được bởi Prometheus.
- Health checks phân biệt live và ready.

**Tiêu chí hoàn thành**

- Một request tạo order có thể trace qua các service liên quan.
- Dependency down làm readiness fail đúng.
- Log không chứa secret hoặc token.
- Dashboard hiển thị request rate, error rate, latency và resource cơ bản.

**Rủi ro cần kiểm soát**

- Log quá nhiều gây nhiễu hoặc lộ dữ liệu nhạy cảm.
- Trace không truyền context qua Kafka.
- Health check quá nặng làm ảnh hưởng service.

---

### Giai đoạn 10: Docker Và Docker Compose

**Mục tiêu**

Đóng gói toàn bộ hệ thống để chạy local nhất quán bằng một lệnh Docker Compose.

**Công việc cần làm**

- Tạo multi-stage Dockerfile cho Gateway và từng service.
- Tạo `.dockerignore`.
- Tạo `infra/docker/docker-compose.yml` cho app services và dependencies: PostgreSQL, MongoDB, Kafka, Redis, Elasticsearch, Keycloak, Seq, Jaeger, Prometheus, Grafana.
- Cấu hình healthcheck và `depends_on` theo readiness local.
- Tách biến môi trường ra `.env.example`.
- Viết hướng dẫn chạy local.

**Thứ tự triển khai**

1. Dockerize từng service.
2. Compose dependency hạ tầng.
3. Compose app services.
4. Gắn environment variables.
5. Kiểm thử `docker compose up -d`.
6. Kiểm tra logs, health và Gateway route.

**Deliverables**

- Dockerfile cho tất cả service.
- Docker Compose chạy full stack local.
- `.env.example` và README chạy local.

**Tiêu chí hoàn thành**

- `docker compose up -d` chạy không lỗi.
- Gateway gọi được các service qua network nội bộ.
- Volumes persist dữ liệu cần thiết.
- Không hardcode secret trong Dockerfile.

**Rủi ro cần kiểm soát**

- Compose khởi động app trước khi dependency ready.
- Image quá lớn do Dockerfile chưa tối ưu.
- Config local lẫn với config production.

---

### Deployment Gate Sau Giai Đoạn 7-10

Chỉ chuyển sang Kubernetes khi:

- Full stack chạy được bằng Docker Compose.
- Health checks và observability hoạt động trong môi trường compose.
- Các service có Dockerfile riêng.
- Gateway là entrypoint duy nhất trong hướng dẫn chạy chuẩn.
- Không có secret thật trong repository.

---

### Giai đoạn 11: Kubernetes, Ingress, ConfigMap, Secret Và HPA

**Mục tiêu**

Triển khai hệ thống lên Kubernetes local để hiểu mô hình production orchestration.

**Công việc cần làm**

- Tạo namespace `ecommerce`.
- Tạo manifests theo `infra/k8s/base` và `infra/k8s/overlays/dev`.
- Tạo Deployment, Service, ConfigMap, Secret cho Gateway và từng service.
- Cấu hình Ingress qua NGINX cho Gateway.
- Cấu hình liveness/readiness probes.
- Cấu hình resource requests/limits.
- Tạo HPA cho service có tải cao như Order hoặc Product.
- Chuẩn bị Helm chart nếu cần đóng gói.

**Thứ tự triển khai**

1. Tạo Kind hoặc Minikube cluster.
2. Deploy dependencies cần thiết.
3. Deploy Gateway và service theo thứ tự dependency.
4. Cấu hình Ingress.
5. Kiểm tra probes và logs.
6. Test rolling update và HPA.

**Deliverables**

- K8s manifests cho môi trường dev.
- Ingress route đến Gateway.
- Probes và HPA hoạt động.

**Tiêu chí hoàn thành**

- Pod restart khi liveness fail.
- Pod không nhận traffic khi readiness fail.
- Rolling update không downtime rõ rệt.
- Secret không lưu plaintext thật trong git.

**Rủi ro cần kiểm soát**

- Copy nguyên Docker Compose config vào Kubernetes mà không tách ConfigMap/Secret.
- Không đặt resource limit gây khó kiểm soát cluster.
- Ingress bỏ qua Gateway và route thẳng vào service.

---

### Giai đoạn 12: Testing, CI/CD, Tài Liệu Hóa Và Hoàn Thiện Demo

**Mục tiêu**

Đưa hệ thống về trạng thái có thể bảo trì, kiểm thử và demo như một portfolio production-ready ở mức thực hành.

**Công việc cần làm**

- Unit test cho Domain logic, đặc biệt Order state machine và Product stock.
- Application test cho command/query handlers.
- Integration test với Testcontainers cho PostgreSQL, Redis, Kafka và API.
- API endpoint test cho happy path và error cases.
- CI pipeline GitHub Actions: restore, build, test.
- Build và push Docker image lên registry khi merge main.
- Bổ sung README: architecture, setup, run local, test, deploy local k8s.
- Bổ sung runbook ngắn cho các lỗi thường gặp.

**Thứ tự triển khai**

1. Viết unit test cho nghiệp vụ cốt lõi.
2. Viết integration test cho Order flow.
3. Thêm Kafka integration test.
4. Tạo GitHub Actions test pipeline.
5. Thêm build/push image.
6. Hoàn thiện README và demo checklist.

**Deliverables**

- Bộ test có thể chạy bằng `dotnet test`.
- GitHub Actions pipeline cơ bản.
- README và runbook đủ để người khác chạy demo.

**Tiêu chí hoàn thành**

- Domain test coverage đạt tối thiểu 80%, ưu tiên Order và Product.
- Integration test kiểm tra create order, cancel order và event publish.
- Pipeline fail nếu build hoặc test fail.
- README giúp chạy được `docker compose up` từ repo.

**Rủi ro cần kiểm soát**

- Chỉ test happy path.
- Integration test phụ thuộc môi trường máy cá nhân.
- CI/CD deploy khi test chưa pass.

---

### Production-Readiness Gate Cuối Cùng

Trước khi xem dự án là hoàn thiện, cần đạt các điều kiện:

- Gateway, service, database, event bus và observability chạy được trong Docker Compose.
- Kubernetes local deploy được các service chính.
- Có test cho nghiệp vụ cốt lõi và integration flow.
- Có CI build/test tự động.
- README mô tả rõ kiến trúc, cách chạy, cách test và cách demo.
- Không có secret thật, token thật hoặc password production trong repository.

---

## 4. Checklist Tuân Thủ Kiến Trúc

### API Gateway Và Security

- [ ] Client chỉ đi qua API Gateway trong luồng chuẩn.
- [ ] Gateway route đến đúng service theo path.
- [ ] Protected endpoint yêu cầu JWT hợp lệ.
- [ ] Role admin/customer được kiểm tra đúng.
- [ ] Rate limit hoạt động ở Gateway.

### Microservice Boundaries

- [ ] Identity, Order, Product, Payment, Notification là các boundary độc lập.
- [ ] Mỗi service có database riêng.
- [ ] Không service nào đọc/ghi trực tiếp database của service khác.
- [ ] Shared contracts không chứa business logic riêng.
- [ ] Mỗi service có Dockerfile và config riêng.

### Event-Driven Communication

- [ ] Kafka topics được đặt tên rõ ràng.
- [ ] Event contracts có version hoặc khả năng mở rộng.
- [ ] Outbox dùng cho event phát sinh từ transaction database.
- [ ] Consumer idempotent.
- [ ] Có retry và dead-letter/error handling.
- [ ] Correlation id đi qua HTTP và Kafka.

### Data Và Caching

- [ ] PostgreSQL dùng cho Order, Product, Payment.
- [ ] MongoDB dùng cho Notification history.
- [ ] Redis dùng cho cache/session/lock với key prefix rõ ràng.
- [ ] Elasticsearch chỉ là read/search model, không là source of truth.
- [ ] Có cách rebuild Elasticsearch index.

### Observability

- [ ] Serilog structured logging có correlation id.
- [ ] Seq nhận log local.
- [ ] OpenTelemetry trace được request và event flow.
- [ ] Jaeger hiển thị trace end-to-end.
- [ ] Prometheus scrape metrics.
- [ ] Grafana dashboard có request rate, error rate, latency.
- [ ] `/health/live` và `/health/ready` có trên mọi service.

### Deployment

- [ ] Docker Compose chạy được toàn bộ local stack.
- [ ] Kubernetes manifests có Deployment, Service, ConfigMap, Secret.
- [ ] Ingress route vào Gateway.
- [ ] Liveness/readiness probes đúng.
- [ ] HPA cấu hình cho service cần scale.
- [ ] Rolling update không làm mất traffic rõ rệt.

### Testing Và CI/CD

- [ ] Unit test bao phủ domain logic quan trọng.
- [ ] Integration test dùng Testcontainers.
- [ ] Kafka publish/consume được kiểm thử.
- [ ] API test có happy path và error cases.
- [ ] CI chạy restore, build, test.
- [ ] Docker image chỉ build/push khi test pass.

---

## 5. Definition Of Done Tổng Thể

Dự án được xem là đạt mục tiêu roadmap khi có thể chứng minh các điểm sau:

- Tạo đơn hàng qua Gateway, xử lý stock, payment và notification bằng event-driven flow.
- Các service có boundary rõ ràng và deploy độc lập.
- Hệ thống chạy local bằng Docker Compose.
- Hệ thống deploy được lên Kubernetes local.
- Có logging, tracing, metrics và health checks đủ để debug luồng chính.
- Có test tự động cho domain và integration flow quan trọng.
- Có README và runbook để người khác setup, chạy, test và demo.
