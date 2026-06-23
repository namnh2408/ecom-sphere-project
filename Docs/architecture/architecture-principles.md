# Nguyên Tắc Kiến Trúc

Tài liệu này khóa các nguyên tắc nền cho hệ thống E-Commerce Microservice trước khi bắt đầu implement service.

## Mục Tiêu

- Xây dựng hệ thống theo kiến trúc microservice bằng .NET.
- Giữ boundary rõ giữa Identity, Order, Product, Payment và Notification.
- Đảm bảo mỗi service có thể phát triển, test, deploy và scale độc lập.
- Ưu tiên event-driven communication cho nghiệp vụ liên service.

## Nguyên Tắc Bắt Buộc

- API Gateway YARP là entrypoint chính cho client.
- Mỗi service sở hữu database riêng, không truy cập database của service khác.
- Giao tiếp nghiệp vụ liên service đi qua Kafka/MassTransit event.
- Shared project không chứa business logic riêng của service.
- Domain layer không phụ thuộc ASP.NET Core, EF Core, Kafka, Redis hoặc thư viện hạ tầng.
- Service phải có health check, logging có cấu trúc và correlation id.
- Secret thật không được commit vào repository.

## Service Boundary

| Service | Trách nhiệm chính | Lưu trữ mục tiêu |
|---|---|---|
| Identity | Auth, role, claim mapping, Keycloak config | Keycloak/PostgreSQL |
| Order | Order aggregate, state machine, order workflow | PostgreSQL |
| Product | Catalog, stock, stock reservation, search sync | PostgreSQL, Elasticsearch |
| Payment | Checkout, payment record, payment result events | PostgreSQL |
| Notification | Notification history, email giả lập, SignalR | MongoDB |

## Quy Tắc Shared

Được phép đưa vào `src/Shared`:

- Event contracts công khai.
- Base abstractions thật sự dùng chung.
- Helper hạ tầng không chứa nghiệp vụ riêng.

Không đưa vào `src/Shared`:

- Entity riêng của một service.
- Handler, repository hoặc business rule riêng.
- DTO nội bộ chỉ phục vụ một API.

## Quality Gate Trước Giai Đoạn 1

- Repository structure đã rõ.
- Naming convention đã rõ.
- Local setup và port mapping đã rõ.
- Không còn blocker tooling bắt buộc cho .NET, Git và Docker.

