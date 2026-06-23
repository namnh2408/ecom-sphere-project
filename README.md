# E-Commerce Microservice

Hệ sinh thái thương mại điện tử theo kiến trúc Microservice bằng .NET.

## Trạng Thái Hiện Tại

Dự án đang ở giai đoạn 0: chuẩn bị môi trường, convention và repository structure. Chưa tạo solution, project service, migration, Dockerfile hoặc pipeline.

## Tài Liệu Chính

- Roadmap tổng thể: [Docs/ecommerce-microservice-roadmap.md](Docs/ecommerce-microservice-roadmap.md)
- Sơ đồ kiến trúc: [Docs/microservice_full_architecture.svg](Docs/microservice_full_architecture.svg)
- Plan tổng thể: [Docs/plans/plan_overral_time20260623.md](Docs/plans/plan_overral_time20260623.md)
- Implementation plan giai đoạn 0: [Docs/implements/plan_impl_giaidoan_0_20260623_1048.md](Docs/implements/plan_impl_giaidoan_0_20260623_1048.md)

## Tài Liệu Nền

- Nguyên tắc kiến trúc: [Docs/architecture/architecture-principles.md](Docs/architecture/architecture-principles.md)
- Naming conventions: [Docs/architecture/naming-conventions.md](Docs/architecture/naming-conventions.md)
- Local port mapping: [Docs/architecture/local-port-mapping.md](Docs/architecture/local-port-mapping.md)
- API guidelines: [Docs/api/api-guidelines.md](Docs/api/api-guidelines.md)
- Local setup runbook: [Docs/runbooks/local-setup.md](Docs/runbooks/local-setup.md)
- Common issues runbook: [Docs/runbooks/common-issues.md](Docs/runbooks/common-issues.md)

## Cấu Trúc Repository Mục Tiêu

```text
src/
  ApiGateway/
  Services/
    Identity/
    Order/
    Product/
    Payment/
    Notification/
  Shared/
tests/
  Unit/
  Integration/
  E2E/
infra/
  docker/
  k8s/
  helm/
Docs/
  plans/
  implements/
  architecture/
  api/
  runbooks/
```

## Kiểm Tra Môi Trường

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

Xem kết quả kiểm tra hiện tại tại [Docs/runbooks/local-setup.md](Docs/runbooks/local-setup.md).

## Nguyên Tắc Bảo Mật Local

- Không commit `.env` thật.
- Chỉ dùng `.env.example` cho giá trị mẫu.
- Không commit token, password production, private key hoặc certificate thật.
