# Common Issues Runbook

## Docker Báo Access Denied Khi Đọc Config

**Dấu hiệu**

```text
WARNING: Error loading config file: open C:\Users\namnh\.docker\config.json: Access is denied.
```

**Tác động**

Docker CLI vẫn có thể trả version, nhưng các lệnh cần registry credential hoặc Docker config có thể lỗi.

**Cách xử lý**

- Kiểm tra quyền đọc file `.docker/config.json`.
- Mở Docker Desktop và đăng nhập lại nếu cần.
- Chạy lại `docker --version` và `docker compose version`.

## Thiếu .NET SDK 8.x

**Dấu hiệu**

`dotnet --version` trả về version khác `8.x`.

**Tác động**

Project tạo mới có thể dùng target framework hoặc template khác roadmap.

**Cách xử lý**

- Cài .NET SDK 8.x.
- Chạy lại `dotnet --list-sdks`.
- Khi tạo solution ở giai đoạn 1, cân nhắc thêm `global.json` để khóa SDK.

## Thiếu Kind Hoặc Minikube

**Dấu hiệu**

`kind version` hoặc `minikube version` báo không tìm thấy lệnh.

**Tác động**

Không ảnh hưởng giai đoạn 1-4, nhưng sẽ chặn giai đoạn Kubernetes.

**Cách xử lý**

- Cài ít nhất một công cụ local Kubernetes.
- Ưu tiên Kind nếu muốn môi trường nhẹ và dễ script hóa.

## Thiếu psql

**Dấu hiệu**

`psql --version` báo không tìm thấy lệnh.

**Tác động**

Khó kiểm tra PostgreSQL bằng CLI.

**Cách xử lý**

- Cài PostgreSQL client tools hoặc dùng DBeaver/pgAdmin.
- Ghi rõ công cụ thay thế trong README nếu không dùng `psql`.

## Port Local Bị Trùng

**Dấu hiệu**

Service hoặc container không start được vì port đã được sử dụng.

**Cách xử lý**

- Kiểm tra process đang chiếm port.
- Chỉ đổi port host, giữ port container/service.
- Cập nhật `Docs/architecture/local-port-mapping.md` nếu thay đổi trở thành convention chung.
