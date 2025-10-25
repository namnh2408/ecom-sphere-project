# ShopHub - Hệ Sinh Thái Thương Mại Điện Tử

## 📌 Giới Thiệu

ShopHub là một **nền tảng thương mại điện tử hiện đại** được xây dựng với:
- **Microservices Architecture** - Mỗi service độc lập, dễ scale
- **Clean Architecture** - Code sạch, dễ maintain
- **Domain-Driven Design (DDD)** - Thiết kế hướng miền kinh doanh

---

## 🗂️ Cấu Trúc Dự Án

```
ShopHub/
├── 📄 ARCHITECTURE.md              # Kiến trúc chi tiết
├── 📄 IMPLEMENTATION_GUIDE.md      # Hướng dẫn triển khai
├── 📄 README_ARCHITECTURE.md       # File này
│
├── 📂 src/
│   ├── 📂 ApiGateway/             # Cổng vào chính (rate limit, auth, routing)
│   │
│   ├── 📂 Services/               # Các microservices
│   │   ├── 📂 ProductService/
│   │   │   ├── Application/       # Use cases, DTOs, validators
│   │   │   ├── Domain/            # Business logic, entities
│   │   │   ├── Infrastructure/    # Database, external services
│   │   │   ├── Presentation/      # Controllers, API endpoints
│   │   │   └── Tests/
│   │   ├── 📂 OrderService/
│   │   ├── 📂 PaymentService/
│   │   ├── 📂 CartService/
│   │   ├── 📂 UserService/
│   │   ├── 📂 InventoryService/
│   │   ├── 📂 ReviewService/
│   │   ├── 📂 PromotionService/
│   │   ├── 📂 NotificationService/
│   │   ├── 📂 AnalyticsService/
│   │   └── 📂 AdminService/
│   │
│   └── 📂 Shared/                 # Shared libraries
│       ├── 📂 ShopHub.Common/     # Constants, exceptions, utilities
│       ├── 📂 ShopHub.Domain/     # Base classes, interfaces
│       ├── 📂 ShopHub.Infrastructure/ # Database, cache, messaging
│       └── 📂 ShopHub.Security/   # Authentication, authorization
│
├── 📂 tests/
│   ├── 📂 Unit/                   # Unit tests cho mỗi service
│   ├── 📂 Integration/            # Integration tests
│   └── 📂 Performance/            # Load tests
│
├── 📂 docker/                     # Docker files
├── 📂 docs/                       # Documentation
├── 📂 scripts/                    # Build, deploy scripts
└── ShopHub.sln                    # Solution file
```

---

## 🏛️ Kiến Trúc Layered

### Mỗi Service Có 4 Layers:

```
┌─────────────────────────────────┐
│  Presentation Layer             │
│  (Controllers, HTTP)            │
├─────────────────────────────────┤
│  Application Layer              │
│  (Use Cases, DTOs, Validators)  │
├─────────────────────────────────┤
│  Domain Layer                   │
│  (Entities, Business Logic)     │
├─────────────────────────────────┤
│  Infrastructure Layer           │
│  (Database, External Services)  │
└─────────────────────────────────┘
```

### Dependency Direction (Dependency Inversion):

```
Presentation → Application → Domain ← Infrastructure
```

---

## 📚 Core Concepts

### 1. Aggregate Root (DDD)
- Ranh giới transaction
- Quản lý domain events
- Ví dụ: `Product`, `Order`, `User`

```csharp
public class Product : BaseAggregateRoot
{
    // Business logic
    public void UpdatePrice(decimal newPrice) { }
    
    // Domain events
    protected void AddDomainEvent(IDomainEvent @event) { }
}
```

### 2. Value Objects (DDD)
- Không có định danh
- So sánh dựa trên giá trị
- Ví dụ: `ProductPrice`, `Money`, `Address`

```csharp
public class ProductPrice : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }
    
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}
```

### 3. Repository Pattern
- Abstract data access
- Per-aggregate root
- DDD compliant

```csharp
public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetBySkuAsync(string sku);
    Task<IReadOnlyList<Product>> GetByCategoryAsync(Guid categoryId);
}
```

### 4. Use Cases
- Application services
- Orchestration logic
- Transaction boundaries

```csharp
public class CreateProductUseCase
{
    public async Task<Result<ProductDto>> ExecuteAsync(CreateProductDto dto)
    {
        // Validate
        // Create domain entity
        // Save via repository
        // Return result
    }
}
```

### 5. Domain Events
- Sự kiện kinh doanh quan trọng
- Async communication giữa services
- Event sourcing ready

```csharp
public class ProductCreatedEvent : IDomainEvent
{
    public Guid ProductId { get; }
    public string ProductName { get; }
    public DateTime OccurredAt { get; }
}
```

---

## 🔄 Data Flow

```
HTTP Request
    ↓
API Gateway (Auth, Rate Limit)
    ↓
Controller (Routing)
    ↓
Use Case (Business Logic)
    ↓
Domain Entity (Core Logic)
    ↓
Repository (Data Access)
    ↓
Database
    ↓
Result DTO
    ↓
HTTP Response
```

---

## 🎯 Services Overview

| Service | Chức Năng | Port |
|---------|----------|------|
| **ProductService** | Quản lý sản phẩm, danh mục | 5001 |
| **OrderService** | Quản lý đơn hàng | 5002 |
| **PaymentService** | Xử lý thanh toán | 5003 |
| **CartService** | Giỏ hàng | 5004 |
| **UserService** | Người dùng, authentication | 5005 |
| **InventoryService** | Kho hàng | 5006 |
| **ReviewService** | Đánh giá, comments | 5007 |
| **PromotionService** | Khuyến mãi, mã giảm giá | 5008 |
| **NotificationService** | Email, SMS, push | 5009 |
| **AnalyticsService** | Thống kê, reports | 5010 |
| **AdminService** | Dashboard admin | 5011 |
| **ApiGateway** | Entry point | 3000 |

---

## 🛠️ Tech Stack

### Backend
- **Framework**: ASP.NET Core 9.0
- **Language**: C# 13
- **Database**: SQL Server 2022
- **ORM**: Entity Framework Core
- **Testing**: MSTest, Moq

### Infrastructure
- **Cache**: Redis
- **MessageBus**: RabbitMQ
- **Logging**: Serilog
- **Monitoring**: Application Insights
- **Container**: Docker

### Tools
- **API Gateway**: YARP / Ocelot
- **API Documentation**: Swagger/OpenAPI
- **DI Container**: Microsoft.Extensions.DependencyInjection

---

## 🚀 Quick Start

### 1. Clone Repository
```bash
git clone https://github.com/yourusername/ShopHub.git
cd ShopHub
```

### 2. Setup Database
```bash
# Update connection string in appsettings.json
# Run migrations
dotnet ef database update --project src/Services/ProductService
```

### 3. Run Services
```bash
# Product Service
dotnet run --project src/Services/ProductService

# Order Service
dotnet run --project src/Services/OrderService

# API Gateway
dotnet run --project src/ApiGateway
```

### 4. Access APIs
- **Swagger**: http://localhost:3000/swagger
- **ProductService**: http://localhost:5001
- **OrderService**: http://localhost:5002

---

## 📝 File Structure Template

### Tạo Service Mới
1. Sao chép template từ `ProductService`
2. Đổi tên namespaces
3. Xóa ví dụ code
4. Triển khai theo IMPLEMENTATION_GUIDE.md

---

## 🧪 Testing Strategy

```
Total Coverage: 80%
├── Unit Tests (60%)
│   └── Domain + Application logic
├── Integration Tests (15%)
│   └── API + Database
└── E2E Tests (5%)
    └── Critical workflows
```

### Run Tests
```bash
# Unit tests
dotnet test tests/Unit/

# Integration tests
dotnet test tests/Integration/

# All tests
dotnet test
```

---

## 📖 Documentation Files

| File | Nội Dung |
|------|---------|
| **ARCHITECTURE.md** | Kiến trúc chi tiết, layers, patterns |
| **IMPLEMENTATION_GUIDE.md** | Step-by-step triển khai service |
| **README_ARCHITECTURE.md** | File này - Overview |

---

## 🔐 Security

- **Authentication**: JWT tokens
- **Authorization**: Role-based access control (RBAC)
- **API Gateway**: Rate limiting, request validation
- **Data**: Encryption at rest + in transit
- **Secrets**: Environment variables, Azure Key Vault

---

## 📈 Scalability

### Horizontal Scaling
- Services độc lập → deploy riêng
- Kubernetes orchestration
- Load balancing

### Vertical Scaling
- Database optimization
- Caching strategy (Redis)
- Connection pooling

### Data Isolation
- Per-service database
- No direct table sharing
- Event-driven communication

---

## 🔗 Communication Patterns

### Sync Communication
- REST APIs
- gRPC (future)
- Service-to-service

### Async Communication
- Domain Events → Message Bus (RabbitMQ)
- Email/Notification Service
- Analytics aggregation

---

## 🚨 Error Handling

### Exception Hierarchy
```
Exception
├── ApplicationException
│   ├── ValidationException (400)
│   ├── NotFoundException (404)
│   ├── BusinessRuleException (400)
│   ├── UnauthorizedException (401)
│   ├── ForbiddenException (403)
│   └── ConflictException (409)
└── DomainException
```

### Global Exception Handler
```csharp
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        // Handle exceptions globally
        // Return standardized error response
    });
});
```

---

## 📋 Development Workflow

### 1. Feature Branch
```bash
git checkout -b feature/add-product-discount
```

### 2. Implement
- Write domain logic (TDD)
- Write use case
- Implement repository
- Create controller

### 3. Test
- Unit tests
- Integration tests
- Manual testing

### 4. Pull Request
- Code review
- CI/CD pipeline
- Deploy to staging

### 5. Merge & Deploy
- Merge to main
- Deploy to production
- Monitor

---

## 🔍 Best Practices

### Code Quality
- SOLID principles
- DRY (Don't Repeat Yourself)
- Clean code (Robert C. Martin)

### DDD
- Ubiquitous Language
- Bounded Contexts per service
- Aggregates for transactions
- Domain Events

### Testing
- Test pyramid (Unit → Integration → E2E)
- Test behavior, not implementation
- Use fixtures and builders

### Git
- Meaningful commit messages
- Atomic commits
- Feature branches
- Code review

---

## 📞 Support

- **Documentation**: See docs/ folder
- **Issues**: GitHub Issues
- **Discussions**: GitHub Discussions

---

## 📜 License

MIT License - See LICENSE file

---

## ✨ Next Steps

1. **Read ARCHITECTURE.md** - Hiểu kiến trúc tổng thể
2. **Read IMPLEMENTATION_GUIDE.md** - Học cách triển khai
3. **Explore ProductService** - Tham khảo service ví dụ
4. **Implement OrderService** - Tập luyện
5. **Setup Database** - SQL Server + migrations
6. **Configure & Deploy** - Docker + Kubernetes

---

**Happy Coding! 🚀**