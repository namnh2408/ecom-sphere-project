# Implementation Plan Giai Đoạn 0 - Chuẩn Bị Môi Trường, Convention Và Repository Structure

> Thuộc roadmap: `Docs/plans/plan_overral_time20260623.md`  
> Nguồn tham chiếu: `Docs/ecommerce-microservice-roadmap.md`, `Docs/microservice_full_architecture.svg`  
> Giai đoạn: 0  
> Timestamp: 20260623_1048  
> Mục tiêu thời gian: Tuần 0  
> Trạng thái: Kế hoạch implement chi tiết trước khi bắt đầu code service

---

## 1. Mục Tiêu Giai Đoạn 0

Giai đoạn 0 nhằm chuẩn hóa toàn bộ nền móng phát triển trước khi tạo các microservice. Sau giai đoạn này, bất kỳ developer nào tham gia dự án đều phải biết:

- Cần cài những công cụ nào và kiểm tra bằng lệnh gì.
- Repository sẽ được tổ chức theo cấu trúc nào.
- Cách đặt tên solution, project, namespace, branch, commit, Docker image, Kafka topic và Redis key.
- Cách cấu hình local development mà không hardcode secret thật.
- Các port local mặc định để tránh xung đột khi chạy full stack.
- Những tài liệu nền nào phải có trước khi bước sang giai đoạn 1.

Giai đoạn này không tạo business service, không implement API, không viết migration và không dựng Docker Compose hoàn chỉnh. Đây là bước khóa tiêu chuẩn để các giai đoạn sau triển khai nhất quán.

---

## 2. Phạm Vi Công Việc

### 2.1. Trong phạm vi

- Xác minh tooling local.
- Chuẩn hóa cấu trúc thư mục repository.
- Định nghĩa convention bắt buộc.
- Đề xuất port mapping local.
- Chuẩn bị danh sách file tài liệu nền cần tạo.
- Chuẩn bị checklist nghiệm thu để bước sang giai đoạn 1.

### 2.2. Ngoài phạm vi

- Không tạo source code cho `Order`, `Product`, `Payment`, `Notification`, `Identity` hoặc `ApiGateway`.
- Không tạo `.sln`, `.csproj`, migration, Dockerfile hoặc Kubernetes manifest.
- Không chạy code generation.
- Không tạo pipeline GitHub Actions.
- Không tạo secret thật, token thật hoặc password production.
- Không chỉnh sửa roadmap hoặc sơ đồ kiến trúc gốc.

---

## 3. Điều Kiện Đầu Vào

Trước khi bắt đầu giai đoạn 1, máy phát triển nên có các công cụ sau.

| Nhóm | Công cụ | Phiên bản mục tiêu | Mục đích |
|---|---|---:|---|
| Runtime | .NET SDK | 8.x LTS | Build và chạy ASP.NET Core service |
| Container | Docker Desktop | Latest stable | Chạy PostgreSQL, Kafka, Redis, Keycloak và các dependency local |
| Source control | Git | 2.4x+ | Quản lý source, branch, commit |
| IDE | Visual Studio 2022 hoặc Rider/VS Code | Latest stable | Phát triển .NET |
| Database client | psql, pgAdmin hoặc DBeaver | Tương thích PostgreSQL 16 | Kiểm tra database local |
| Kubernetes CLI | kubectl | Latest stable | Chuẩn bị cho giai đoạn Kubernetes |
| Local Kubernetes | Kind hoặc Minikube | Latest stable | Chạy cluster local ở giai đoạn sau |
| API client | Postman, Insomnia hoặc HTTP file | Latest stable | Test API thủ công |
| Shell | PowerShell 7 hoặc Windows PowerShell | Có sẵn | Chạy script local |

Nếu chưa cài đủ công cụ Kubernetes ở giai đoạn 0, vẫn có thể bắt đầu giai đoạn 1, nhưng phải ghi rõ trong checklist còn thiếu `kubectl` hoặc `Kind/Minikube` để xử lý trước giai đoạn 11.

---

## 4. Checklist Xác Minh Môi Trường

Chạy các lệnh sau và ghi nhận kết quả vào checklist nội bộ hoặc pull request đầu tiên của dự án.

```powershell
dotnet --version
dotnet --list-sdks
docker --version
docker compose version
git --version
kubectl version --client
kind version
minikube version
psql --version
```

Kết quả mong đợi:

- `dotnet --version` trả về `8.x`.
- `docker --version` và `docker compose version` chạy không lỗi.
- `git --version` chạy không lỗi.
- Ít nhất một trong hai công cụ `kind` hoặc `minikube` chạy được.
- `kubectl version --client` chạy được, kể cả khi chưa có cluster.
- `psql --version` hoặc một database client tương đương đã sẵn sàng.

Nếu lệnh nào chưa có, ghi vào mục "Tooling còn thiếu" và không tự ý thay bằng công cụ khác nếu công cụ đó ảnh hưởng đến script hoặc hướng dẫn chung.

---

## 5. Repository Structure Cần Chuẩn Bị

Cấu trúc repository mục tiêu:

```text
ECommerceMS/
├── src/
│   ├── ApiGateway/
│   ├── Services/
│   │   ├── Identity/
│   │   ├── Order/
│   │   ├── Product/
│   │   ├── Payment/
│   │   └── Notification/
│   └── Shared/
├── tests/
│   ├── Unit/
│   ├── Integration/
│   └── E2E/
├── infra/
│   ├── docker/
│   ├── k8s/
│   │   ├── base/
│   │   └── overlays/
│   └── helm/
├── Docs/
│   ├── plans/
│   ├── implements/
│   ├── architecture/
│   ├── api/
│   └── runbooks/
└── .github/
    └── workflows/
```

Quy tắc:

- `src/Services/{ServiceName}` chứa code của từng bounded context.
- `src/Shared` chỉ chứa contracts, base abstractions và helper hạ tầng dùng chung.
- `tests/Unit` không phụ thuộc Docker hoặc external service.
- `tests/Integration` được phép dùng Testcontainers.
- `infra/docker` chứa Docker Compose và config local.
- `infra/k8s` chứa Kubernetes manifests.
- `Docs/plans` chứa plan tổng thể.
- `Docs/implements` chứa plan implement chi tiết theo từng giai đoạn.
- `Docs/runbooks` chứa hướng dẫn xử lý lỗi vận hành.

---

## 6. Convention Bắt Buộc

### 6.1. Solution, project và namespace

| Loại | Convention | Ví dụ |
|---|---|---|
| Solution | `ECommerceMS` | `ECommerceMS.sln` |
| API Gateway | `ECommerceMS.ApiGateway` | `src/ApiGateway/ECommerceMS.ApiGateway` |
| Service Domain | `ECommerceMS.{Service}.Domain` | `ECommerceMS.Order.Domain` |
| Service Application | `ECommerceMS.{Service}.Application` | `ECommerceMS.Order.Application` |
| Service Infrastructure | `ECommerceMS.{Service}.Infrastructure` | `ECommerceMS.Order.Infrastructure` |
| Service API | `ECommerceMS.{Service}.API` | `ECommerceMS.Order.API` |
| Shared Contracts | `ECommerceMS.Shared.Contracts` | Kafka events, integration DTOs |
| Shared Infrastructure | `ECommerceMS.Shared.Infrastructure` | Auth, logging, health check extensions |
| Shared Domain | `ECommerceMS.Shared.Domain` | Base entity, domain event abstractions |

Namespace phải khớp với tên project. Không dùng namespace viết tắt như `EMS` hoặc `Ecom`.

### 6.2. Branch convention

| Loại branch | Format | Ví dụ |
|---|---|---|
| Feature | `feature/{phase}-{short-name}` | `feature/p1-order-service` |
| Fix | `fix/{short-name}` | `fix/order-validation` |
| Chore | `chore/{short-name}` | `chore/setup-solution` |
| Docs | `docs/{short-name}` | `docs/phase-0-plan` |
| Release | `release/{version}` | `release/v0.1.0` |

Quy tắc:

- Tên branch dùng chữ thường, dấu gạch ngang, không dùng dấu tiếng Việt.
- Một branch chỉ nên phục vụ một mục tiêu rõ ràng.
- Không commit trực tiếp vào `main` nếu dự án đã bật branch protection.

### 6.3. Commit convention

Dùng format:

```text
<type>(<scope>): <summary>
```

Các `type` được phép:

- `feat`: thêm tính năng.
- `fix`: sửa lỗi.
- `docs`: tài liệu.
- `test`: test.
- `refactor`: refactor không đổi behavior.
- `chore`: cấu hình, tooling, dependency.
- `ci`: pipeline.
- `infra`: Docker, Kubernetes, infrastructure config.

Ví dụ:

```text
docs(phase-0): add implementation plan for environment setup
chore(solution): initialize ecommerce solution structure
feat(order): add create order command
```

### 6.4. API convention

| Nội dung | Quy tắc |
|---|---|
| Base path | `/api/{resource}` |
| Resource name | Số nhiều, chữ thường |
| Health live | `/health/live` |
| Health ready | `/health/ready` |
| Error response | Dùng format thống nhất theo ProblemDetails hoặc custom envelope |
| Pagination | `pageNumber`, `pageSize` |
| Sorting | `sortBy`, `sortDirection` |
| Filtering | Query string rõ nghĩa |

Ví dụ endpoint mục tiêu:

```text
POST   /api/orders
GET    /api/orders/{id}
GET    /api/orders?pageNumber=1&pageSize=20
PATCH  /api/orders/{id}/cancel
GET    /api/products/search?q=laptop
```

### 6.5. Logging convention

Logging phải là structured logging, không nối chuỗi tùy tiện.

Thông tin tối thiểu nên có:

- `CorrelationId`
- `TraceId`
- `ServiceName`
- `Environment`
- `UserId` nếu có auth context
- `OrderId`, `ProductId`, `PaymentId` nếu liên quan nghiệp vụ

Ví dụ message style:

```text
Order created successfully
Payment failed
Stock reservation rejected
```

Không log:

- Password.
- Access token.
- Refresh token.
- Secret key.
- Full payment information.
- Dữ liệu cá nhân không cần thiết.

### 6.6. Exception convention

Phân loại exception:

| Loại | HTTP status mục tiêu | Ví dụ |
|---|---:|---|
| Validation | 400 | Quantity phải lớn hơn 0 |
| Unauthorized | 401 | Thiếu token |
| Forbidden | 403 | Không đủ role |
| NotFound | 404 | Không tìm thấy order |
| Conflict | 409 | Trạng thái order không hợp lệ |
| ExternalDependency | 503 | Kafka, Redis hoặc DB tạm thời unavailable |
| Unexpected | 500 | Lỗi chưa dự đoán |

Quy tắc:

- Domain exception không phụ thuộc ASP.NET Core.
- API layer map exception sang response.
- Không trả stack trace cho client.
- Luôn log lỗi 5xx với correlation id.

### 6.7. Configuration convention

Dùng hierarchical configuration theo ASP.NET Core:

```text
ConnectionStrings__Default
Kafka__BootstrapServers
Redis__ConnectionString
Keycloak__Authority
Keycloak__Audience
Elasticsearch__Url
Seq__Url
Otlp__Endpoint
```

Quy tắc:

- `appsettings.json` chỉ chứa default không nhạy cảm.
- `appsettings.Development.json` chỉ dùng cho local.
- Secret thật không commit vào git.
- `.env.example` chỉ chứa giá trị mẫu.
- Docker Compose dùng environment variables.
- Kubernetes dùng ConfigMap và Secret.

### 6.8. Docker image convention

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

Tag mục tiêu:

- `local`: build local.
- `latest`: bản mới nhất cho môi trường demo.
- `{git-sha}`: tag immutable trong CI/CD.

### 6.9. Kafka topic convention

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

Quy tắc:

- Tên topic chữ thường, phân tách bằng dấu chấm.
- Event dùng quá khứ phân từ hoặc trạng thái đã xảy ra.
- Không đặt topic theo tên consumer.
- Event contract phải nằm trong `ECommerceMS.Shared.Contracts`.

### 6.10. Redis key convention

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

Quy tắc:

- Key có prefix `ecommerce`.
- Key có service hoặc domain rõ ràng.
- Key cache phải có TTL.
- Lock key phải có timeout.
- Không lưu token hoặc secret trong Redis nếu chưa có yêu cầu bảo mật cụ thể.

---

## 7. Port Mapping Local Đề Xuất

| Thành phần | Port host | Port container/service | Ghi chú |
|---|---:|---:|---|
| API Gateway | 5000 | 8080 | Entrypoint chính |
| Order Service | 5001 | 8080 | Chỉ expose khi debug local |
| Product Service | 5002 | 8080 | Chỉ expose khi debug local |
| Payment Service | 5003 | 8080 | Chỉ expose khi debug local |
| Notification Service | 5004 | 8080 | SignalR hub |
| Keycloak | 8080 | 8080 | Auth server local |
| PostgreSQL | 5432 | 5432 | Database chính |
| MongoDB | 27017 | 27017 | Notification history |
| Redis | 6379 | 6379 | Cache/session/lock |
| Kafka broker | 9092 | 9092 | Internal/external listener cần cấu hình rõ |
| Kafka UI | 8085 | 8080 | Tùy chọn |
| Elasticsearch | 9200 | 9200 | Product search |
| Kibana | 5601 | 5601 | Tùy chọn |
| Seq | 5341 | 80 | Log UI |
| Jaeger UI | 16686 | 16686 | Trace UI |
| OTLP gRPC | 4317 | 4317 | OpenTelemetry exporter |
| Prometheus | 9090 | 9090 | Metrics |
| Grafana | 3000 | 3000 | Dashboard |
| Hangfire Dashboard | 5010 | 8080 | Nếu tách worker hoặc expose riêng |

Nếu port bị trùng trên máy local, ưu tiên đổi port host nhưng giữ port container/service nhất quán để Docker network nội bộ không đổi.

---

## 8. File Tài Liệu Nền Cần Có Sau Giai Đoạn 0

Các file sau nên được tạo trong hoặc ngay sau giai đoạn 0 trước khi bắt đầu implement giai đoạn 1:

| File | Mục đích |
|---|---|
| `Docs/architecture/architecture-principles.md` | Nguyên tắc kiến trúc và boundary giữa service |
| `Docs/architecture/naming-conventions.md` | Naming convention cho solution, project, topic, key, branch |
| `Docs/architecture/local-port-mapping.md` | Danh sách port local chuẩn |
| `Docs/runbooks/local-setup.md` | Hướng dẫn setup môi trường local |
| `Docs/runbooks/common-issues.md` | Lỗi thường gặp và cách xử lý |
| `Docs/api/api-guidelines.md` | API convention, error response, pagination |
| `.env.example` | Biến môi trường mẫu, không chứa secret thật |
| `.gitignore` | Loại trừ bin/obj, secret local, IDE files, container artifacts |
| `README.md` | Tổng quan repo, cách setup nhanh và link đến tài liệu chi tiết |

Ở giai đoạn 0, có thể chỉ tạo skeleton các tài liệu này với nội dung tối thiểu. Nội dung sẽ được cập nhật dần ở các giai đoạn tiếp theo.

---

## 9. Thứ Tự Thực Hiện Chi Tiết

### Bước 0.1: Xác minh công cụ local

**Việc làm**

- Chạy các lệnh version trong mục checklist môi trường.
- Ghi nhận công cụ thiếu hoặc sai version.

**Output mong đợi**

- Danh sách tooling đã sẵn sàng.
- Danh sách tooling còn thiếu, nếu có.

**Điều kiện pass**

- .NET SDK 8.x sẵn sàng.
- Docker và Docker Compose sẵn sàng.
- Git sẵn sàng.

### Bước 0.2: Chuẩn hóa cấu trúc thư mục

**Việc làm**

- Tạo các thư mục nền theo repository structure mục tiêu.
- Không tạo project code ở bước này nếu chưa sang giai đoạn 1.

**Output mong đợi**

- `src`, `tests`, `infra`, `Docs`, `.github` có cấu trúc rõ ràng.

**Điều kiện pass**

- Không có thư mục tạm hoặc tên sai convention.
- `Docs/plans` và `Docs/implements` được giữ đúng vai trò.

### Bước 0.3: Chốt naming convention

**Việc làm**

- Ghi lại convention cho solution, project, namespace, branch, commit, Docker image, Kafka topic và Redis key.

**Output mong đợi**

- Một tài liệu convention có thể dùng lại khi tạo service.

**Điều kiện pass**

- Không còn nhiều kiểu đặt tên khác nhau cho cùng một khái niệm.
- Tên project tương lai có thể suy ra từ convention mà không cần hỏi lại.

### Bước 0.4: Chốt local configuration convention

**Việc làm**

- Xác định tên biến môi trường.
- Xác định port mapping local.
- Chuẩn bị `.env.example` ở mức mẫu nếu bắt đầu tạo repo skeleton.

**Output mong đợi**

- Bảng port mapping.
- Danh sách environment variables chuẩn.

**Điều kiện pass**

- Không có secret thật trong tài liệu.
- Config local và production không bị trộn lẫn.

### Bước 0.5: Tạo skeleton tài liệu nền

**Việc làm**

- Tạo hoặc lên kế hoạch tạo các file trong `Docs/architecture`, `Docs/api`, `Docs/runbooks`.
- README trỏ đến các tài liệu chính.

**Output mong đợi**

- Người mới có thể tìm thấy nơi đọc architecture, setup local và convention.

**Điều kiện pass**

- Không có tài liệu trùng vai trò.
- Tài liệu ngắn nhưng đủ điều hướng.

### Bước 0.6: Review readiness trước giai đoạn 1

**Việc làm**

- Chạy lại checklist.
- Review naming, port mapping và repo structure.
- Ghi nhận các quyết định còn nợ.

**Output mong đợi**

- Giai đoạn 0 được đánh dấu hoàn thành.
- Có thể bắt đầu tạo `ECommerceMS.sln` và Order Service ở giai đoạn 1.

**Điều kiện pass**

- Không còn blocker về tooling quan trọng.
- Convention đủ rõ để tạo project mà không tự quyết thêm.

---

## 10. Deliverables

Kết thúc giai đoạn 0 cần có:

- Checklist môi trường đã xác minh.
- Repository structure mục tiêu được thống nhất.
- Naming convention được thống nhất.
- Local port mapping được thống nhất.
- Configuration convention được thống nhất.
- Danh sách tài liệu nền cần có.
- Definition of Done cho phép chuyển sang giai đoạn 1.

Nếu có tạo file thật trong giai đoạn 0, các file tối thiểu nên là:

- `Docs/architecture/naming-conventions.md`
- `Docs/architecture/local-port-mapping.md`
- `Docs/runbooks/local-setup.md`
- `Docs/api/api-guidelines.md`
- `.env.example`
- `README.md`

---

## 11. Definition Of Done

Giai đoạn 0 hoàn thành khi tất cả điều kiện sau đạt:

- [ ] .NET SDK 8.x đã được xác minh.
- [ ] Docker và Docker Compose đã được xác minh.
- [ ] Git đã được xác minh.
- [ ] `kubectl` đã sẵn sàng hoặc đã ghi nhận là việc còn nợ trước giai đoạn 11.
- [ ] Kind hoặc Minikube đã sẵn sàng hoặc đã ghi nhận là việc còn nợ trước giai đoạn 11.
- [ ] PostgreSQL client đã sẵn sàng hoặc có công cụ thay thế được thống nhất.
- [ ] Repository structure mục tiêu đã được thống nhất.
- [ ] Naming convention cho solution/project/namespace đã rõ.
- [ ] Branch và commit convention đã rõ.
- [ ] API convention đã rõ.
- [ ] Logging và exception convention đã rõ.
- [ ] Configuration convention đã rõ.
- [ ] Docker image convention đã rõ.
- [ ] Kafka topic convention đã rõ.
- [ ] Redis key convention đã rõ.
- [ ] Port mapping local đã rõ.
- [ ] Danh sách tài liệu nền đã rõ.
- [ ] Không có secret thật trong bất kỳ tài liệu hoặc config mẫu nào.

---

## 12. Checklist Nghiệm Thu

Người review có thể dùng checklist này trước khi approve chuyển sang giai đoạn 1.

### Tooling

- [ ] `dotnet --version` trả về version 8.x.
- [ ] `docker compose version` chạy không lỗi.
- [ ] `git --version` chạy không lỗi.
- [ ] Công cụ Kubernetes đã được kiểm tra hoặc ghi nhận còn nợ.
- [ ] Database client đã được kiểm tra hoặc có phương án thay thế.

### Repository

- [ ] Cấu trúc thư mục không mâu thuẫn với roadmap.
- [ ] Thư mục `src/Shared` không bị định nghĩa quá rộng.
- [ ] Thư mục `tests` tách Unit, Integration và E2E.
- [ ] Thư mục `infra` tách Docker, Kubernetes và Helm.
- [ ] Thư mục `Docs` tách plans, implements, architecture, api và runbooks.

### Convention

- [ ] Có quy tắc đặt tên project cho mọi service.
- [ ] Có quy tắc branch và commit.
- [ ] Có quy tắc endpoint API.
- [ ] Có quy tắc error response.
- [ ] Có quy tắc logging không lộ secret.
- [ ] Có quy tắc configuration không hardcode secret.
- [ ] Có quy tắc topic Kafka.
- [ ] Có quy tắc key Redis.

### Readiness

- [ ] Có thể bắt đầu giai đoạn 1 mà không cần hỏi lại cách đặt tên solution/project.
- [ ] Có thể tạo Order Service theo Clean Architecture từ convention đã chốt.
- [ ] Có thể viết README setup local dựa trên thông tin hiện có.
- [ ] Không có quyết định nền tảng quan trọng còn mơ hồ.

---

## 13. Rủi Ro Và Cách Kiểm Soát

| Rủi ro | Tác động | Cách kiểm soát |
|---|---|---|
| Version .NET không đồng nhất | Build lỗi giữa các máy | Ghi version SDK mục tiêu, dùng `global.json` ở giai đoạn setup solution nếu cần |
| Docker Desktop chưa chạy ổn định | Không chạy được dependency local | Kiểm tra Docker trước khi dựng Docker Compose |
| Port local bị trùng | Service không start được | Dùng bảng port chuẩn, nếu đổi thì ghi rõ port host mới |
| Convention không rõ | Tên project/topic/key lệch nhau | Review convention trước khi tạo solution |
| Shared project bị lạm dụng | Coupling giữa service tăng | Chỉ cho phép contracts/base/helper thật sự dùng chung |
| Secret bị commit | Rủi ro bảo mật | Chỉ dùng `.env.example`, không commit `.env` thật |
| API style không thống nhất | Client khó tích hợp | Chốt API guideline trước khi tạo controller |
| Logging thiếu correlation id | Khó debug microservice | Đưa correlation id vào convention ngay từ đầu |
| Kafka topic đặt theo consumer | Khó mở rộng consumer mới | Đặt topic theo domain event đã xảy ra |
| Redis key không có prefix | Va chạm key giữa service | Dùng prefix `ecommerce:{service}:{type}:{id}` |

---

## 14. Lỗi Thường Gặp Khi Thực Hiện Giai Đoạn 0

### Lỗi 1: Bắt đầu code service trước khi chốt convention

**Dấu hiệu**

- Project đầu tiên dùng tên khác pattern.
- Namespace không khớp thư mục.
- Topic Kafka bị đặt lại nhiều lần.

**Cách xử lý**

- Dừng tạo thêm service.
- Chốt convention bằng tài liệu.
- Rename sớm khi blast radius còn nhỏ.

### Lỗi 2: Đưa quá nhiều thứ vào Shared

**Dấu hiệu**

- Shared chứa service logic.
- Shared phụ thuộc EF Core hoặc dependency riêng của một service.
- Service phải update theo nhau vì đổi shared model.

**Cách xử lý**

- Chỉ giữ contracts và abstraction thật sự dùng chung.
- Business logic phải ở bounded context tương ứng.

### Lỗi 3: Hardcode local config vào code

**Dấu hiệu**

- Connection string nằm trực tiếp trong C#.
- Token hoặc password nằm trong repo.
- Docker config khó đổi theo môi trường.

**Cách xử lý**

- Chuyển sang configuration provider.
- Dùng environment variables.
- Chỉ commit `.env.example`.

### Lỗi 4: Port mapping không được ghi lại

**Dấu hiệu**

- Mỗi người chạy service ở port khác nhau.
- README không khớp Docker Compose.
- Gateway route sai host/port.

**Cách xử lý**

- Dùng bảng port chuẩn trong tài liệu.
- Mọi thay đổi port phải cập nhật tài liệu.

---

## 15. Kết Luận Giai Đoạn 0

Giai đoạn 0 là lớp nền cho toàn bộ roadmap microservice. Nếu làm kỹ, các giai đoạn sau sẽ giảm đáng kể việc đổi tên, sửa config, tranh luận về convention và debug lỗi môi trường.

Khi checklist Definition of Done đạt, dự án có thể chuyển sang giai đoạn 1: tạo solution `ECommerceMS`, dựng Order Service theo Clean Architecture và bắt đầu implement nghiệp vụ đầu tiên.
