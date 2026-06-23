# Local Setup Runbook

## Mục Tiêu

Tài liệu này ghi lại trạng thái tooling local và các bước chuẩn bị trước khi bắt đầu implement service.

## Tooling Đã Kiểm Tra

Kết quả kiểm tra tại thời điểm thực hiện giai đoạn 0:

| Công cụ | Lệnh | Kết quả ghi nhận | Trạng thái |
|---|---|---|---|
| .NET SDK | `dotnet --version` | `10.0.301` | Chưa khớp mục tiêu .NET 8.x LTS |
| .NET SDK list | `dotnet --list-sdks` | `10.0.301` | Chưa có SDK 8.x trong PATH |
| Docker | `docker --version` | `Docker version 29.5.3` | Có warning quyền đọc Docker config |
| Docker Compose | `docker compose version` | `Docker Compose version v5.1.4` | Có warning quyền đọc Docker config |
| Git | `git --version` | `2.51.2.windows.1` | Đạt |
| kubectl | `kubectl version --client` | `Client Version: v1.34.1` | Đạt |
| Kind | `kind version` | Không tìm thấy lệnh | Còn thiếu |
| Minikube | `minikube version` | Không tìm thấy lệnh | Còn thiếu |
| psql | `psql --version` | Không tìm thấy lệnh | Còn thiếu hoặc dùng client thay thế |

## Việc Còn Nợ Trước Khi Sang Các Giai Đoạn Sau

- Cài hoặc cấu hình .NET SDK 8.x vì dự án chốt runtime là .NET 8 LTS.
- Sửa warning Docker: `Access is denied` khi đọc `C:\Users\namnh\.docker\config.json`.
- Cài Kind hoặc Minikube trước giai đoạn Kubernetes.
- Cài `psql` hoặc thống nhất dùng pgAdmin/DBeaver làm PostgreSQL client.

## Lệnh Kiểm Tra Lại

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

## Ghi Chú

Giai đoạn 1 có thể bắt đầu nếu .NET SDK mục tiêu được xử lý. Không nên tạo solution bằng SDK 10 nếu roadmap đã chốt .NET 8 mà chưa có quyết định nâng version.
