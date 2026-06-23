# Local Port Mapping

Port host dưới đây là chuẩn mặc định khi chạy local. Nếu máy cá nhân bị trùng port, chỉ đổi port host và phải cập nhật tài liệu liên quan.

| Thành phần | Port host | Port container/service | Ghi chú |
|---|---:|---:|---|
| API Gateway | 5000 | 8080 | Entrypoint chính |
| Order Service | 5001 | 8080 | Debug local |
| Product Service | 5002 | 8080 | Debug local |
| Payment Service | 5003 | 8080 | Debug local |
| Notification Service | 5004 | 8080 | SignalR hub |
| Keycloak | 8080 | 8080 | Auth server local |
| PostgreSQL | 5432 | 5432 | Database chính |
| MongoDB | 27017 | 27017 | Notification history |
| Redis | 6379 | 6379 | Cache, session, lock |
| Kafka broker | 9092 | 9092 | Event bus |
| Kafka UI | 8085 | 8080 | Tùy chọn |
| Elasticsearch | 9200 | 9200 | Product search |
| Kibana | 5601 | 5601 | Tùy chọn |
| Seq | 5341 | 80 | Structured logs |
| Jaeger UI | 16686 | 16686 | Distributed traces |
| OTLP gRPC | 4317 | 4317 | OpenTelemetry exporter |
| Prometheus | 9090 | 9090 | Metrics |
| Grafana | 3000 | 3000 | Dashboard |
| Hangfire Dashboard | 5010 | 8080 | Nếu expose riêng |

## Nguyên Tắc

- Client gọi Gateway qua port `5000`.
- Service port `5001-5004` chỉ dùng debug local, không phải luồng chuẩn.
- Trong Docker network, các service gọi nhau bằng service name và port container.
- Không dùng port ngẫu nhiên trong tài liệu hoặc script chung.

