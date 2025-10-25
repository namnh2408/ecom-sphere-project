# 🏗️ ShopHub - Kiến Trúc Hệ Sinh Thái Thương Mại Điện Tử

## 📌 Tổng Quan

ShopHub là một **hệ sinh thái thương mại điện tử** được xây dựng trên **Microservices Architecture** + **Clean Architecture** + **Domain-Driven Design (DDD)**.

### Stack Công Nghệ
- **Framework**: ASP.NET Core (.NET 9)
- **Database**: SQL Server
- **API Communication**: REST + gRPC (future)
- **Message Queue**: RabbitMQ (future)
- **Caching**: Redis (future)
- **Authentication**: JWT
- **Containerization**: Docker

---

## 🎯 Các Dịch Vụ Chính

| Service | Trách Nhiệm |
|---------|-----------|
| **API Gateway** | Entry point, routing, rate limiting, authentication |
| **ProductService** | Quản lý sản phẩm, danh mục, thông tin chi tiết |
| **OrderService** | Quản lý đơn hàng, trạng thái, lịch sử |
| **PaymentService** | Xử lý thanh toán, ghi doanh thu, tích hợp gateway |
| **CartService** | Quản lý giỏ hàng, tạm tính |
| **UserService** | Quản lý người dùng, profiling, authentication |
| **InventoryService** | Quản lý kho, stock, nhập xuất |
| **ReviewService** | Quản lý đánh giá, bình luận, rating |
| **PromotionService** | Quản lý khuyến mãi, mã giảm giá, deal |
| **NotificationService** | Gửi email, SMS, push notifications |
| **AnalyticsService** | Thống kê, báo cáo, insights |
| **AdminService** | Dashboard admin, quản lý hệ thống |

---

## 📁 Cấu Trúc Thư Mục Tổng Thể

```
ShopHub/
│
├── 📂 src/
│   ├── 📂 ApiGateway/
│   ├── 📂 Services/
│   │   ├── ProductService/
│   │   │   ├── Application/
│   │   │   ├── Domain/
│   │   │   ├── Infrastructure/
│   │   │   ├── Presentation/
│   │   │   └── Tests/
│   │   ├── OrderService/
│   │   ├── PaymentService/
│   │   ├── CartService/
│   │   ├── UserService/
│   │   ├── InventoryService/
│   │   ├── ReviewService/
│   │   ├── PromotionService/
│   │   ├── NotificationService/
│   │   ├── AnalyticsService/
│   │   └── AdminService/
│   │
│   └── 📂 Shared/
│       ├── ShopHub.Common/
│       ├── ShopHub.Domain/
│       ├── ShopHub.Infrastructure/
│       └── ShopHub.Security/
│
├── 📂 tests/
│   ├── 📂 Unit/
│   ├── 📂 Integration/
│   └── 📂 Performance/
│
├── 📂 docker/
├── 📂 docs/
├── 📂 scripts/
├── ShopHub.sln
└── ARCHITECTURE.md
```

---

## 📂 SRC - Cấu Trúc Chi Tiết

### Services/ProductService/Application/

```
Application/
├── DTOs/
│   ├── CreateProductDto.cs           # Input DTO
│   ├── UpdateProductDto.cs           # Update DTO
│   ├── ProductResponseDto.cs         # Output DTO
│   └── ProductListDto.cs
│
├── Services/
│   ├── IProductApplicationService.cs
│   └── ProductApplicationService.cs
│
├── UseCases/
│   ├── CreateProductUseCase.cs       # Use case command
│   ├── UpdateProductUseCase.cs
│   ├── GetProductByIdUseCase.cs
│   ├── SearchProductsUseCase.cs
│   └── DeleteProductUseCase.cs
│
├── Validators/
│   ├── CreateProductValidator.cs
│   └── UpdateProductValidator.cs
│
└── Mappings/
    └── ProductMappingProfile.cs
```

### Services/ProductService/Domain/

```
Domain/
├── Entities/
│   ├── Product.cs                    # Aggregate Root
│   ├── Category.cs
│   └── ProductImage.cs
│
├── ValueObjects/
│   ├── ProductPrice.cs               # Price with currency
│   ├── ProductStock.cs               # Stock quantity
│   ├── ProductRating.cs              # Rating calculation
│   └── ProductDescription.cs
│
├── Events/
│   ├── ProductCreatedEvent.cs
│   ├── ProductUpdatedEvent.cs
│   ├── ProductPriceChangedEvent.cs
│   └── ProductDeletedEvent.cs
│
├── Services/
│   ├── IProductDomainService.cs
│   └── ProductDomainService.cs
│
└── Repositories/
    └── IProductRepository.cs         # Interface only
```

### Services/ProductService/Infrastructure/

```
Infrastructure/
├── Persistence/
│   ├── ProductDbContext.cs           # EF DbContext
│   ├── Repositories/
│   │   └── ProductRepository.cs      # Implementation
│   ├── Migrations/
│   │   └── Initial_CreateTables.cs
│   └── EntityConfigurations/
│       └── ProductEntityConfiguration.cs
│
├── ExternalServices/
│   ├── ImageUploadService.cs
│   └── ProductSearchService.cs (Elasticsearch)
│
├── DependencyInjection.cs            # DI registration
└── ServiceRegistration.cs
```

### Services/ProductService/Presentation/

```
Presentation/
├── Controllers/
│   ├── ProductsController.cs
│   └── CategoriesController.cs
│
├── Filters/
│   ├── ValidationFilter.cs
│   └── ExceptionFilter.cs
│
├── Middleware/
│   └── ErrorHandlingMiddleware.cs
│
└── Program.cs                        # Entry point
```

---

## 📂 SHARED - Cấu Trúc Chi Tiết

### src/Shared/ShopHub.Common/

```
ShopHub.Common/
│
├── Constants/
│   ├── ErrorCodes.cs
│   │   public static class ErrorCodes
│   │   {
│   │       public const string PRODUCT_NOT_FOUND = "PRODUCT_NOT_FOUND";
│   │       public const string INVALID_PRICE = "INVALID_PRICE";
│   │   }
│   │
│   ├── HttpStatusCodes.cs
│   └── AppConstants.cs
│
├── Exceptions/
│   ├── ApplicationException.cs       # Base exception
│   ├── DomainException.cs           # Domain logic exception
│   ├── ValidationException.cs       # Data validation
│   ├── NotFoundException.cs
│   └── UnauthorizedException.cs
│
├── Results/
│   ├── Result.cs                    # Generic result wrapper
│   │   public class Result<T>
│   │   {
│   │       public bool IsSuccess { get; set; }
│   │       public T Data { get; set; }
│   │       public string Message { get; set; }
│   │       public List<string> Errors { get; set; }
│   │   }
│   │
│   ├── PagedResult.cs               # Paginated result
│   └── ResultExtensions.cs
│
├── Pagination/
│   ├── PaginationParams.cs
│   │   public class PaginationParams
│   │   {
│   │       public int PageNumber { get; set; } = 1;
│   │       public int PageSize { get; set; } = 10;
│   │   }
│   │
│   └── PaginatedList.cs
│
├── Utilities/
│   ├── IdGenerator.cs               # GUID, Snowflake ID
│   ├── DateTimeProvider.cs          # Abstraction for DateTime
│   └── StringHelper.cs
│
└── Extensions/
    ├── StringExtensions.cs          # Helper methods
    ├── EnumerableExtensions.cs
    └── DateTimeExtensions.cs
```

### src/Shared/ShopHub.Domain/

```
ShopHub.Domain/
│
├── Abstractions/
│   ├── IAggregateRoot.cs            # Marker interface
│   │   public interface IAggregateRoot
│   │   {
│   │   }
│   │
│   ├── IEntity.cs
│   │   public interface IEntity
│   │   {
│   │       Guid Id { get; }
│   │       DateTime CreatedAt { get; }
│   │   }
│   │
│   ├── IRepository.cs               # Base repository interface
│   │   public interface IRepository<T> where T : IAggregateRoot
│   │   {
│   │       Task<T> GetByIdAsync(Guid id);
│   │       Task AddAsync(T entity);
│   │       void Update(T entity);
│   │       void Delete(T entity);
│   │   }
│   │
│   ├── IDomainEvent.cs
│   │   public interface IDomainEvent
│   │   {
│   │       Guid Id { get; }
│   │       DateTime OccurredAt { get; }
│   │   }
│   │
│   └── IUnitOfWork.cs
│
├── Base/
│   ├── BaseEntity.cs                # Base for all entities
│   │   public abstract class BaseEntity : IEntity
│   │   {
│   │       public Guid Id { get; protected set; }
│   │       public DateTime CreatedAt { get; set; }
│   │       public DateTime? UpdatedAt { get; set; }
│   │       public bool IsDeleted { get; set; }
│   │   }
│   │
│   ├── BaseAggregateRoot.cs         # Base for aggregate roots
│   │   public abstract class BaseAggregateRoot 
│   │       : BaseEntity, IAggregateRoot
│   │   {
│   │       private readonly List<IDomainEvent> _domainEvents = new();
│   │       public IReadOnlyList<IDomainEvent> DomainEvents 
│   │           => _domainEvents.AsReadOnly();
│   │       
│   │       protected void AddDomainEvent(IDomainEvent evt)
│   │           => _domainEvents.Add(evt);
│   │   }
│   │
│   ├── ValueObject.cs               # Base for value objects
│   │   public abstract class ValueObject : IEquatable<ValueObject>
│   │   {
│   │       public abstract IEnumerable<object> GetEqualityComponents();
│   │   }
│   │
│   ├── Enumeration.cs               # Base for enumerations
│   └── DomainEventBase.cs
│
├── Specifications/
│   ├── ISpecification.cs            # Query object pattern
│   │   public interface ISpecification<T>
│   │   {
│   │       IQueryable<T> Apply(IQueryable<T> query);
│   │   }
│   │
│   └── BaseSpecification.cs
│
└── Events/
    ├── DomainEventDispatcher.cs    # Event publishing
    └── IDomainEventHandler.cs
```

### src/Shared/ShopHub.Infrastructure/

```
ShopHub.Infrastructure/
│
├── Data/
│   ├── DbContextBase.cs             # Base DbContext
│   │   public abstract class DbContextBase : DbContext
│   │   {
│   │       public DbContextBase(DbContextOptions options) 
│   │           : base(options) { }
│   │       
│   │       protected override void OnModelCreating(...)
│   │       {
│   │           // Configure soft delete, auditing, etc
│   │       }
│   │   }
│   │
│   ├── UnitOfWork.cs                # Transaction management
│   ├── AuditableEntity.cs
│   └── Repository.cs                # Base repository implementation
│
├── Cache/
│   ├── ICacheService.cs
│   │   public interface ICacheService
│   │   {
│   │       Task<T> GetAsync<T>(string key);
│   │       Task SetAsync<T>(string key, T value, TimeSpan? expiry);
│   │       Task RemoveAsync(string key);
│   │   }
│   │
│   └── RedisCacheService.cs         # Redis implementation
│
├── MessageBus/
│   ├── IMessageBus.cs
│   │   public interface IMessageBus
│   │   {
│   │       Task PublishAsync<T>(T message, CancellationToken token);
│   │       Task SubscribeAsync<T>(Func<T, Task> handler);
│   │   }
│   │
│   └── RabbitMqBus.cs               # RabbitMQ implementation
│
├── Http/
│   ├── IHttpClientService.cs
│   └── HttpClientService.cs
│
└── Extensions/
    ├── ServiceCollectionExtensions.cs
    └── IQueryableExtensions.cs
```

### src/Shared/ShopHub.Security/

```
ShopHub.Security/
│
├── Authentication/
│   ├── JwtSettings.cs
│   │   public class JwtSettings
│   │   {
│   │       public string SecretKey { get; set; }
│   │       public string Issuer { get; set; }
│   │       public string Audience { get; set; }
│   │       public int ExpirationMinutes { get; set; }
│   │   }
│   │
│   ├── JwtProvider.cs               # Token generation/validation
│   └── TokenValidator.cs
│
├── Authorization/
│   ├── Policies/
│   │   ├── AdminPolicy.cs
│   │   └── UserPolicy.cs
│   └── Requirements/
│       └── AdminRequirement.cs
│
└── Middleware/
    ├── AuthenticationMiddleware.cs
    └── AuthorizationMiddleware.cs
```

---

## 🧪 TESTS - Cấu Trúc Chi Tiết

### tests/Unit/Services.ProductService.Tests/

```
Unit Tests/
│
├── Domain/
│   ├── ProductTests.cs
│   │   [TestClass]
│   │   public class ProductTests
│   │   {
│   │       [TestMethod]
│   │       public void CreateProduct_WithValidData_ShouldSucceed()
│   │       {
│   │           // Arrange
│   │           var name = "Test Product";
│   │           var price = new ProductPrice(100);
│   │           
│   │           // Act
│   │           var product = Product.Create(name, price);
│   │           
│   │           // Assert
│   │           Assert.AreEqual(name, product.Name);
│   │       }
│   │   }
│   │
│   ├── ProductValueObjectTests.cs
│   ├── ProductPriceTests.cs
│   └── ProductDomainServiceTests.cs
│
├── Application/
│   ├── CreateProductUseCaseTests.cs
│   │   [TestClass]
│   │   public class CreateProductUseCaseTests
│   │   {
│   │       private Mock<IProductRepository> _repositoryMock;
│   │       private CreateProductUseCase _useCase;
│   │       
│   │       [TestInitialize]
│   │       public void Setup()
│   │       {
│   │           _repositoryMock = new Mock<IProductRepository>();
│   │           _useCase = new CreateProductUseCase(_repositoryMock.Object);
│   │       }
│   │       
│   │       [TestMethod]
│   │       public async Task Execute_WithValidDto_ShouldCreateProduct()
│   │       {
│   │           // Arrange
│   │           var dto = new CreateProductDto { ... };
│   │           
│   │           // Act
│   │           var result = await _useCase.ExecuteAsync(dto);
│   │           
│   │           // Assert
│   │           Assert.IsTrue(result.IsSuccess);
│   │       }
│   │   }
│   │
│   ├── UpdateProductUseCaseTests.cs
│   ├── SearchProductsUseCaseTests.cs
│   └── ProductMappingTests.cs
│
└── TestFixtures/
    ├── ProductFixture.cs
    │   public class ProductFixture
    │   {
    │       public static Product CreateValidProduct()
    │       {
    │           return Product.Create("Test", new ProductPrice(100));
    │       }
    │       
    │       public static CreateProductDto CreateValidDto()
    │       {
    │           return new CreateProductDto { ... };
    │       }
    │   }
    │
    └── CategoryFixture.cs
```

### tests/Integration/ApiGateway.IntegrationTests/

```
Integration Tests/
│
├── Controllers/
│   ├── ProductsControllerTests.cs
│   │   [TestClass]
│   │   public class ProductsControllerTests : IDisposable
│   │   {
│   │       private WebApplicationFactory<Program> _factory;
│   │       private HttpClient _client;
│   │       
│   │       [TestInitialize]
│   │       public void Setup()
│   │       {
│   │           _factory = new WebApplicationFactory<Program>();
│   │           _client = _factory.CreateClient();
│   │       }
│   │       
│   │       [TestMethod]
│   │       public async Task GetProduct_WithValidId_ShouldReturn200()
│   │       {
│   │           // Arrange
│   │           var productId = Guid.NewGuid();
│   │           
│   │           // Act
│   │           var response = await _client.GetAsync($"/api/products/{productId}");
│   │           
│   │           // Assert
│   │           Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
│   │       }
│   │   }
│   │
│   ├── OrdersControllerTests.cs
│   └── UsersControllerTests.cs
│
├── Database/
│   ├── RepositoryTests.cs
│   │   [TestClass]
│   │   public class ProductRepositoryTests
│   │   {
│   │       private DbContext _dbContext;
│   │       private ProductRepository _repository;
│   │       
│   │       [TestInitialize]
│   │       public async Task Setup()
│   │       {
│   │           _dbContext = new TestDbContext();
│   │           _repository = new ProductRepository(_dbContext);
│   │           await _dbContext.Database.EnsureCreatedAsync();
│   │       }
│   │   }
│   │
│   └── QueryTests.cs
│
└── Helpers/
    ├── TestDatabaseFactory.cs
    │   public class TestDatabaseFactory
    │   {
    │       public static DbContext CreateInMemoryContext()
    │       {
    │           var options = new DbContextOptionsBuilder<DbContext>()
    │               .UseInMemoryDatabase(Guid.NewGuid().ToString())
    │               .Options;
    │           
    │           return new DbContext(options);
    │       }
    │   }
    │
    └── TestDataBuilder.cs
```

### tests/Performance/LoadTests/

```
Performance Tests/
│
├── Products/
│   ├── GetProductLoadTest.cs
│   │   [TestClass]
│   │   public class GetProductLoadTest
│   │   {
│   │       private LoadTestClient _client;
│   │       
│   │       [LoadTest(Iterations = 1000, Threads = 10)]
│   │       public async Task GetProduct_Under1Second()
│   │       {
│   │           // Arrange & Act
│   │           var sw = Stopwatch.StartNew();
│   │           await _client.GetProductAsync(Guid.NewGuid());
│   │           sw.Stop();
│   │           
│   │           // Assert
│   │           Assert.IsTrue(sw.ElapsedMilliseconds < 1000);
│   │       }
│   │   }
│   │
│   └── SearchProductsLoadTest.cs
│
└── Helpers/
    └── LoadTestConfig.cs
```

---

## 🏛️ Clean Architecture - Layers

### 1. Domain Layer (Lớp Miền)
- **Trách nhiệm**: Business logic thuần túy
- **Không phụ thuộc**: Framework, Database, HTTP
- **Chứa**: Entities, ValueObjects, Aggregates, Domain Services, Domain Events
- **Test**: Unit tests

### 2. Application Layer (Lớp Ứng Dụng)
- **Trách nhiệm**: Coordination logic, use cases
- **Phụ thuộc**: Domain layer only
- **Chứa**: UseCases, Services, DTOs, Mappings, Validators
- **Test**: Unit tests + mocking

### 3. Infrastructure Layer (Lớp Hạ Tầng)
- **Trách nhiệm**: Technical concerns
- **Phụ thuộc**: Domain + Application layers
- **Chứa**: Database, External APIs, Cache, Message Queue, Email
- **Test**: Integration tests

### 4. Presentation Layer (Lớp Trình Bày)
- **Trách nhiệm**: HTTP endpoints, API contracts
- **Phụ thuộc**: Toàn bộ
- **Chứa**: Controllers, Filters, Middleware, Response DTOs
- **Test**: Integration + E2E tests

---

## 🔄 Dependency Injection Strategy

```csharp
// ServiceCollectionExtensions.cs trong mỗi Service
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProductService(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Domain Services
        services.AddScoped<IProductDomainService, ProductDomainService>();
        
        // Application Services
        services.AddScoped<IProductApplicationService, ProductApplicationService>();
        
        // Use Cases
        services.AddScoped<CreateProductUseCase>();
        services.AddScoped<UpdateProductUseCase>();
        services.AddScoped<GetProductByIdUseCase>();
        
        // Repositories
        services.AddScoped<IProductRepository, ProductRepository>();
        
        // DbContext
        services.AddDbContext<ProductDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("ProductDb")));
        
        // AutoMapper
        services.AddAutoMapper(typeof(ProductMappingProfile));
        
        // Validators
        services.AddScoped<IValidator<CreateProductDto>, 
            CreateProductValidator>();
        
        return services;
    }
}

// Program.cs
builder.Services.AddProductService(builder.Configuration);
builder.Services.AddOrderService(builder.Configuration);
builder.Services.AddPaymentService(builder.Configuration);
// ... other services
```

---

## 📊 Data Flow Example

```
HTTP Request
   ↓
API Gateway (Routing, Auth, Rate Limiting)
   ↓
ProductsController.GetProduct(id)
   ↓
IProductApplicationService.GetProductAsync(id)
   ↓
GetProductByIdUseCase.ExecuteAsync(id)
   ↓
IProductDomainService.ValidateProduct(id)
   ↓
IProductRepository.GetByIdAsync(id)
   ↓
ProductDbContext.Products.FindAsync(id)
   ↓
SQL Server (Database)
   ↓
Domain Entity (Product)
   ↓
AutoMapper → ProductResponseDto
   ↓
HTTP Response (200 OK)
```

---

## 🔐 Cross-Cutting Concerns

### Authentication
- JWT tokens (Header: Authorization: Bearer {token})
- User context injection
- Role-based access control (RBAC)

### Logging
- Structured logging (Serilog)
- Request/Response logging middleware
- Error tracking (Application Insights)

### Validation
- Fluent validation (Data annotation)
- Custom business rule validators
- Async validators

### Exception Handling
- Global exception middleware
- Domain exceptions
- Application-level exception mapping

---

## 📈 Testing Strategy

```
Overall Test Coverage: 80%
├── Unit Tests: 60%
│   ├── Domain layer: 20%
│   ├── Application layer: 20%
│   └── Utilities: 20%
├── Integration Tests: 15%
│   ├── Repositories: 8%
│   ├── Controllers: 5%
│   └── External services: 2%
└── E2E Tests: 5%
    ├── API scenarios: 3%
    └── Critical workflows: 2%
```

---

## 📝 Naming Conventions

| Thành phần | Convention | Ví dụ |
|-----------|-----------|--------|
| Namespace | [Company].[Project].[Service].[Layer] | `ShopHub.Services.Product.Application` |
| Interface | I[Name] | `IProductService` |
| Class | [Name][Type] | `ProductService`, `ProductValidator` |
| Entity | Singular, PascalCase | `Product`, `Order` |
| Repository | I[Entity]Repository | `IProductRepository` |
| Service | I[Entity]Service | `IProductApplicationService` |
| UseCase | [Verb][Entity]UseCase | `CreateProductUseCase` |
| DTO | [Entity][Type]Dto | `CreateProductDto`, `ProductResponseDto` |
| Event | [Entity][Action]Event | `ProductCreatedEvent` |
| Enum | [Entity]Status | `OrderStatus`, `PaymentStatus` |
| Method | Verb[Object]Async | `GetProductByIdAsync` |
| Property | PascalCase | `ProductName`, `UnitPrice` |

---

## ✅ Checklist Mỗi Service Mới

- [ ] Tạo cấu trúc thư mục (Domain, Application, Infrastructure, Presentation)
- [ ] Định nghĩa Entities + Value Objects
- [ ] Tạo Domain Services + Domain Events
- [ ] Tạo Repository interfaces
- [ ] Tạo DTOs + Validators
- [ ] Tạo Use Cases
- [ ] Tạo Repository implementations
- [ ] Tạo DbContext + Entity Configuration
- [ ] Tạo Controllers
- [ ] Tạo Unit Tests (Domain + Application)
- [ ] Tạo Integration Tests
- [ ] Tạo ServiceCollectionExtensions
- [ ] Cập nhật API Gateway
- [ ] Viết documentation

---

## 🚀 Deployment Strategy

### Local Development
```yaml
docker-compose.yml:
- ProductService (Port 5001)
- OrderService (Port 5002)
- PaymentService (Port 5003)
- SQL Server (Port 1433)
- Redis (Port 6379)
- RabbitMQ (Port 5672)
```

### Production
- Container orchestration (Kubernetes)
- Service mesh (Istio)
- Load balancing
- Auto-scaling
- Health checks
- Monitoring (Prometheus, Grafana)

---

## 📚 Best Practices

1. **DDD Principles**
   - Ubiquitous Language across team
   - Aggregates for transaction boundaries
   - Bounded Contexts per service
   - Domain Events for async communication

2. **Clean Code**
   - SOLID principles
   - DRY (Don't Repeat Yourself)
   - Single Responsibility Principle
   - Dependency Inversion

3. **API Design**
   - RESTful conventions
   - Proper HTTP status codes
   - Consistent naming
   - API versioning (v1, v2, ...)

4. **Database Design**
   - Soft deletes (IsDeleted flag)
   - Audit columns (CreatedAt, UpdatedAt, CreatedBy)
   - Proper indexing
   - Foreign key constraints

5. **Version Control**
   - Feature branches per feature
   - Pull requests with code review
   - Semantic versioning
   - Meaningful commit messages

---

## 📖 File References

- See: `ARCHITECTURE.md` (this file)
- See: Project structure in source code
- See: Example templates in test directories