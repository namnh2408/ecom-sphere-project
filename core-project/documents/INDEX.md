# 📚 Hướng dẫn Tài liệu - Core Project

## 🎯 Thứ tự đọc khuyến nghị

### 1️⃣ **00_Getting_Started** - Bắt Đầu Nhanh (Đọc trước)
Phần này giúp bạn hiểu nhanh về dự án và kiến trúc cơ bản.

- **[00_README.md](./00_Getting_Started/00_README.md)** - Giới thiệu dự án
- **[01_QUICK_START.md](./00_Getting_Started/01_QUICK_START.md)** - Hướng dẫn bắt đầu nhanh chóng
- **[02_ArchitectureDemo.md](./00_Getting_Started/02_ArchitectureDemo.md)** - Demo kiến trúc & nguyên tắc thiết kế

---

### 2️⃣ **01_Architecture** - Kiến Trúc Tổng Quan

Chi tiết về kiến trúc Modular Monolith và cấu trúc dự án.

- **[01_ArchitectureSummary.md](./01_Architecture/01_ArchitectureSummary.md)** - Tóm tắt kiến trúc E-commerce
- **[02_ProjectStructure.md](./01_Architecture/02_ProjectStructure.md)** - Hướng dẫn cấu trúc thư mục

---

### 3️⃣ **02_Users_Module** - Module Người Dùng

Tài liệu về authentication, authorization và quản lý người dùng.

- **[01_Quickstart.md](./02_Users_Module/01_Quickstart.md)** - Bắt đầu nhanh với Users Module
- **[02_Implementation.md](./02_Users_Module/02_Implementation.md)** - Chi tiết cách triển khai
- **[03_Roadmap.md](./02_Users_Module/03_Roadmap.md)** - Lộ trình phát triển hoàn chỉnh

---

### 4️⃣ **03_API_Documentation** - Tài Liệu API

Swagger/OpenAPI documentation và best practices.

- **[01_SwaggerImplementation.md](./03_API_Documentation/01_SwaggerImplementation.md)** - Triển khai Swagger hoàn chỉnh
- **[02_DocumentationGuide.md](./03_API_Documentation/02_DocumentationGuide.md)** - Hướng dẫn viết tài liệu API
- **[03_ImprovementsSummary.md](./03_API_Documentation/03_ImprovementsSummary.md)** - Tóm tắt cải tiến Swagger
- **[04_QuickReference.md](./03_API_Documentation/04_QuickReference.md)** - Tham khảo nhanh Swagger

---

### 5️⃣ **04_Features** - Tính Năng & Giai Đoạn Phát Triển

Danh sách các tính năng trong từng giai đoạn phát triển.

- **[01_Phase1_NewFeatures.md](./04_Features/01_Phase1_NewFeatures.md)** - Tính năng Giai đoạn 1
- **[02_Phase2_FeaturesExtended.md](./04_Features/02_Phase2_FeaturesExtended.md)** - Tính năng Giai đoạn 2 (Mở rộng)
- **[03_CustomErrorHandling.md](./04_Features/03_CustomErrorHandling.md)** - Hệ thống xử lý lỗi tùy chỉnh

---

### 6️⃣ **05_Database** - Cơ Sở Dữ Liệu

Schema, migrations và cấu hình database.

- **[01_DatabaseSchema.md](./05_Database/01_DatabaseSchema.md)** - Lược đồ cơ sở dữ liệu

---

### 7️⃣ **06_Operations** - Vận Hành & Hỗ Trợ

Hướng dẫn xử lý sự cố và thống kê phát triển.

- **[01_Troubleshooting.md](./06_Operations/01_Troubleshooting.md)** - Hướng dẫn xử lý sự cố
- **[02_DevelopmentStatistics.md](./06_Operations/02_DevelopmentStatistics.md)** - Thống kê phát triển hệ thống

---

### 8️⃣ **07_Redis** - Cấu Hình Redis

Hướng dẫn thiết lập và sử dụng Redis cho caching, sessions và event bus.

- **[07_Redis_Configuration.md](./07_Redis_Configuration.md)** - Cấu hình Redis chi tiết
- **[00_QuickStart.md](./07_Redis/00_QuickStart.md)** - Bắt đầu nhanh với Redis

---

### 9️⃣ **08_Testing** - Unit Testing & QA

Hướng dẫn viết unit tests và test coverage.

- **[00_QuickStart.md](./08_Testing/00_QuickStart.md)** - Bắt đầu nhanh với Unit Tests

---

## 📖 Hướng dẫn sử dụng

### 🚀 Nếu bạn là developer mới:
1. Bắt đầu với **Getting_Started** folder
2. Hiểu kiến trúc qua **Architecture** folder
3. Tìm hiểu module bạn cần phát triển

### 🔧 Nếu bạn cần phát triển feature:
1. Xem **Features** để hiểu roadmap
2. Tham khảo **API_Documentation** nếu cần integrate
3. Kiểm tra **Database** schema nếu cần thêm data

### 🐛 Nếu bạn gặp lỗi:
1. Kiểm tra **Troubleshooting** trong Operations
2. Xem lại **Database** configuration
3. Tham khảo lại **Architecture** principles

---

## 📁 Cấu trúc file

```
documents/
├── INDEX.md (file này)
├── 07_Redis_Configuration.md    # Cấu hình Redis chi tiết
├── 00_Getting_Started/          # Phần bắt đầu
├── 01_Architecture/             # Kiến trúc tổng quan
├── 02_Users_Module/             # Module Users
├── 03_API_Documentation/        # Tài liệu API
├── 04_Features/                 # Tính năng & roadmap
│   ├── 01_Phase1_NewFeatures.md
│   ├── 02_Phase2_FeaturesExtended.md
│   └── 03_CustomErrorHandling.md
├── 05_Database/                 # Database schema
├── 06_Operations/               # Vận hành & hỗ trợ
├── 07_Redis/                    # Redis Quick Start
└── 08_Testing/                  # Unit Testing & QA
```

---

**Lần cập nhật cuối:** October 2025