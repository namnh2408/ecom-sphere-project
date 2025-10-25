# CQRS + MediatR Implementation Guide

## Giới Thiệu

Dự án ShopHub sử dụng **CQRS (Command Query Responsibility Segregation)** pattern kết hợp với **MediatR** library để tạo một kiến trúc ứng dụng sạch, dễ bảo trì và có khả năng mở rộng cao.

## CQRS Pattern

### Khái Niệm

CQRS tách riêng các thao tác **đọc dữ liệu (Query)** và **ghi dữ liệu (Command)**:

- **Command**: Thay đổi trạng thái hệ thống (Create, Update, Delete)
  - Không trả về dữ liệu hoặc trả về ít dữ liệu
  - Luôn có side effects
  - Có thể gây lỗi validation

- **Query**: Lấy dữ liệu từ hệ thống (Read)
  - Không thay đổi trạng thái
  - Có thể cache
  - Không có side effects

### Lợi Ích

✅ **Separation of Concerns**: Commands và Queries có logic độc lập
✅ **Scalability**: Có thể scale read và write operations riêng biệt
✅ **Performance**: Có thể optimize caching cho Queries
✅ **Testability**: Dễ dàng test Commands và Queries riêng lẻ
✅ **Maintainability**: Code dễ hiểu và dễ bảo trì

## MediatR Library

### Khái Niệm

MediatR là một .NET library giúp implement **Mediator Pattern** và **Pipeline Pattern**:

- **Mediator**: Điều phối giao tiếp giữa sender (Controller) và handler (Command/Query Handler)
- **Pipeline**: Cho phép thêm "behaviors" (như validation, logging, caching) vào pipeline xử lý

### Cấu Trúc

```
Controller (IMediator.Send)
    ↓
MediatR Pipeline
    ↓
Behaviors (Validation → Logging → Caching → Transaction)
    ↓
Handler (Command/Query Handler)
    ↓
Repository/Database
```

## Kiến Trúc CQRS + MediatR trong ShopHub

### 1. Commands

Commands đại diện cho các thao tác thay đổi dữ liệu.

#### Cấu Trúc

```csharp
// Command interface (implements ICommand<TResponse>)
public class CreateProductCommand : ICommand<ProductResponseDto>
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    // ... properties
}

// Validator (extends AbstractValidator<TCommand>)
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MinimumLength(3);
        // ... more rules
    }
}

// Handler (implements ICommandHandler<TCommand, TResponse>)
public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, ProductResponseDto>
{
    public async Task<Result<ProductResponseDto>> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        // Business logic here
        return Result<ProductResponseDto>.Success(data);
    }
}
```

#### Attributes

- **`[Transactional]`**: Tự động wrap command trong transaction

```csharp
[Transactional]
public class CreateProductCommand : ICommand<ProductResponseDto>
{
    // Sẽ tự động chạy trong transaction
}
```

### 2. Queries

Queries đại diện cho các thao tác lấy dữ liệu.

#### Cấu Trúc

```csharp
// Query interface (implements IQuery<TResponse>)
[Cacheable(durationSeconds: 600)]
public class GetProductByIdQuery : IQuery<ProductResponseDto>
{
    public Guid Id { get; set; }
}

// Validator
public class GetProductByIdQueryValidator : AbstractValidator<GetProductByIdQuery>
{
    public GetProductByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Product ID is required");
    }
}

// Handler
public class GetProductByIdQueryHandler : IQueryHandler<GetProductByIdQuery, ProductResponseDto>
{
    public async Task<Result<ProductResponseDto>> Handle(
        GetProductByIdQuery request,
        CancellationToken cancellationToken)
    {
        // Query logic here
        return Result<ProductResponseDto>.Success(data);
    }
}
```

#### Attributes

- **`[Cacheable(durationSeconds)]`**: Cache query results

```csharp
[Cacheable(durationSeconds: 300)] // Cache 5 minutes
public class SearchProductsQuery : IQuery<PaginatedList<ProductResponseDto>>
{
    // Kết quả sẽ được cache 5 phút
}
```

### 3. MediatR Behaviors (Pipeline)

Behaviors là middleware trong pipeline MediatR. Chúng được thực thi **trước** handler.

#### Validation Behavior

Tự động validate Command/Query trước khi execute handler:

```
Controller
    ↓
ValidationBehavior (kiểm tra validators)
    ↓
Handler
```

```csharp
// Thêm validators và behavior sẽ tự động kiểm tra
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Price).GreaterThan(0);
    }
}

// Nếu validation fails:
// - Behavior trả về Result.Failure với lỗi validation
// - Handler không được execute
```

#### Logging Behavior

Tự động log execution time và errors:

```
[INFO] Executing CreateProductCommand
[INFO] Completed CreateProductCommand in 150ms
```

#### Caching Behavior

Tự động cache Query results:

```
Query được execute → Result được cache → Query tiếp theo lấy từ cache
```

```csharp
// Cache hit example
[Cacheable(durationSeconds: 300)]
public class GetProductByIdQuery : IQuery<ProductResponseDto>
{
    public Guid Id { get; set; }
}

// First request: Execute handler, cache result
// Second request: Return cached result
// After 5 minutes: Cache expires, execute handler again
```

#### Transaction Behavior

Tự động wrap Commands trong database transaction:

```csharp
[Transactional]
public class CreateProductCommand : ICommand<ProductResponseDto>
{
    // Behavior sẽ:
    // 1. BeginTransaction()
    // 2. Execute handler
    // 3. Commit() if success
    // 4. Rollback() if error
}
```

## Sử Dụng trong Controllers

### Trước (với Use Cases)

```csharp
public class ProductsController : ControllerBase
{
    private readonly CreateProductUseCase _createProductUseCase;

    public async Task<IActionResult> CreateProduct(CreateProductDto dto)
    {
        var result = await _createProductUseCase.ExecuteAsync(dto);
        return CreatedAtAction(nameof(GetProduct), result.Data);
    }
}
```

### Sau (với CQRS + MediatR)

```csharp
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public async Task<IActionResult> CreateProduct(CreateProductDto dto)
    {
        // 1. Tạo command
        var command = new CreateProductCommand(
            dto.Name, dto.Price, dto.Stock, dto.CategoryId);

        // 2. Send command qua MediatR
        var result = await _mediator.Send(command);

        // 3. Return result
        return CreatedAtAction(nameof(GetProduct), result.Data);
    }

    public async Task<IActionResult> GetProduct(Guid id)
    {
        // 1. Tạo query
        var query = new GetProductByIdQuery(id);

        // 2. Send query qua MediatR (sẽ được cache)
        var result = await _mediator.Send(query);

        // 3. Return result
        return Ok(result.Data);
    }
}
```

## Dependency Injection

### Setup

```csharp
// Program.cs hoặc DependencyInjection.cs
public static IServiceCollection AddProductService(
    this IServiceCollection services,
    IConfiguration configuration)
{
    // 1. MediatR - registers all handlers from assembly
    var assembly = typeof(ServiceCollectionExtensions).Assembly;
    services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

    // 2. Behaviors (Pipeline)
    services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    services.AddScoped(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
    services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

    // 3. Validators
    services.AddValidatorsFromAssemblyContaining<CreateProductCommandValidator>();

    // 4. Memory Cache (cho caching)
    services.AddMemoryCache();

    return services;
}
```

## File Structure

```
src/Services/ProductService/
├── Application/
│   ├── Commands/
│   │   ├── CreateProductCommand.cs
│   │   ├── UpdateProductCommand.cs
│   │   └── DeleteProductCommand.cs
│   ├── Queries/
│   │   ├── GetProductByIdQuery.cs
│   │   └── SearchProductsQuery.cs
│   ├── Handlers/
│   │   ├── CreateProductCommandHandler.cs
│   │   ├── UpdateProductCommandHandler.cs
│   │   ├── DeleteProductCommandHandler.cs
│   │   ├── GetProductByIdQueryHandler.cs
│   │   └── SearchProductsQueryHandler.cs
│   └── DTOs/
│       └── ProductResponseDto.cs
├── Domain/
│   ├── Entities/
│   │   └── Product.cs
│   └── Repositories/
│       └── IProductRepository.cs
├── Infrastructure/
│   ├── DependencyInjection.cs
│   └── Persistence/
│       └── ProductDbContext.cs
└── Presentation/
    └── Controllers/
        └── ProductsController.cs

src/Shared/
├── ShopHub.Domain/
│   └── CQRS/
│       ├── ICommand.cs
│       ├── IQuery.cs
│       ├── ICommandHandler.cs
│       └── IQueryHandler.cs
└── ShopHub.Common/
    └── CQRS/
        ├── ValidationBehavior.cs
        ├── LoggingBehavior.cs
        ├── CachingBehavior.cs
        └── TransactionBehavior.cs
```

## Best Practices

### 1. Naming Conventions

- **Commands**: `<Action><Entity>Command`
  - ✅ `CreateProductCommand`, `UpdateProductCommand`, `DeleteProductCommand`
  - ❌ `ProductCommand`, `CreateCmd`

- **Queries**: `<Action><Entity>Query`
  - ✅ `GetProductByIdQuery`, `SearchProductsQuery`, `ListProductsQuery`
  - ❌ `ProductQuery`, `GetCmd`

- **Handlers**: `<Command/Query>Handler`
  - ✅ `CreateProductCommandHandler`, `GetProductByIdQueryHandler`
  - ❌ `ProductHandler`, `Handler`

### 2. Validation

```csharp
// ✅ Good: Validators cho Command/Query
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MinimumLength(3);
    }
}

// ❌ Bad: Validation logic trong handler
public async Task<Result<ProductResponseDto>> Handle(CreateProductCommand request)
{
    if (string.IsNullOrEmpty(request.Name)) // Đừng làm vậy!
        return Result.Failure("Name is required");
}
```

### 3. Handlers

```csharp
// ✅ Good: Handler chỉ orchestrate logic
public async Task<Result<ProductResponseDto>> Handle(CreateProductCommand request)
{
    var product = Product.Create(...); // Domain logic
    await _repository.AddAsync(product); // Data access
    await _unitOfWork.SaveChangesAsync();
    return Result.Success(MapToDto(product));
}

// ❌ Bad: Validation logic trong handler
public async Task<Result<ProductResponseDto>> Handle(CreateProductCommand request)
{
    if (request.Price < 0) return Result.Failure("Price invalid");
    // ...
}
```

### 4. Caching

```csharp
// ✅ Good: Cache cho Queries
[Cacheable(durationSeconds: 300)]
public class GetProductByIdQuery : IQuery<ProductResponseDto> { }

// ❌ Bad: Cache cho Commands (Commands không nên cache!)
[Cacheable(durationSeconds: 300)]
public class CreateProductCommand : ICommand<ProductResponseDto> { }
```

### 5. Transactions

```csharp
// ✅ Good: Transactional Commands
[Transactional]
public class CreateProductCommand : ICommand<ProductResponseDto> { }

// ✅ Good: Non-transactional Queries
public class GetProductByIdQuery : IQuery<ProductResponseDto> { }
```

## Error Handling

### Command Failure Example

```csharp
// ProductsController
[HttpPost]
public async Task<IActionResult> CreateProduct(CreateProductDto dto)
{
    var command = new CreateProductCommand(...);
    var result = await _mediator.Send(command);

    if (!result.IsSuccess)
        return BadRequest(new { 
            message = result.Message, 
            errors = result.Errors 
        });

    return CreatedAtAction(nameof(GetProduct), result.Data);
}

// Response example (validation failed)
{
    "message": "Validation failed",
    "errors": {
        "Name": ["Name is required", "Name must be at least 3 characters"],
        "Price": ["Price must be greater than 0"]
    }
}
```

## Performance Considerations

### 1. Caching Strategy

```csharp
// Cache 5 minutes
[Cacheable(durationSeconds: 300)]
public class GetProductByIdQuery { }

// Cache 10 minutes (less frequently changed data)
[Cacheable(durationSeconds: 600)]
public class ListCategoriesQuery { }

// No cache (frequently changing data)
public class SearchProductsQuery { }
```

### 2. Query Optimization

```csharp
// ✅ Good: Efficient query
public async Task<Result<ProductResponseDto>> Handle(GetProductByIdQuery request)
{
    var product = await _repository.GetByIdAsync(request.Id);
    return Result.Success(MapToDto(product));
}

// ❌ Bad: N+1 query problem
public async Task<Result<ProductResponseDto>> Handle(GetProductByIdQuery request)
{
    var product = await _context.Products.FirstAsync(x => x.Id == request.Id);
    var category = await _context.Categories.FirstAsync(x => x.Id == product.CategoryId);
    // Should use .Include()
}
```

## Testing

### Unit Testing Commands

```csharp
[Fact]
public async Task CreateProductCommand_WithValidData_ShouldCreateProduct()
{
    // Arrange
    var command = new CreateProductCommand(
        "Test Product", 100m, 10, Guid.NewGuid());
    var handler = new CreateProductCommandHandler(
        _repositoryMock.Object, _unitOfWorkMock.Object, _loggerMock.Object);

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Data);
    _repositoryMock.Verify(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()));
}

[Fact]
public async Task CreateProductCommand_WithInvalidPrice_ShouldFail()
{
    // Arrange
    var command = new CreateProductCommand("Test", -100m, 10, Guid.NewGuid());
    var validator = new CreateProductCommandValidator();

    // Act
    var result = await validator.ValidateAsync(command);

    // Assert
    Assert.False(result.IsValid);
    Assert.Single(result.Errors.Where(x => x.PropertyName == "Price"));
}
```

### Integration Testing Handlers

```csharp
[Fact]
public async Task CreateProductCommandHandler_ShouldPersistToDatabase()
{
    // Arrange
    var options = new DbContextOptionsBuilder<ProductDbContext>()
        .UseInMemoryDatabase("TestDb")
        .Options;
    
    using var context = new ProductDbContext(options);
    var repository = new ProductRepository(context);
    var handler = new CreateProductCommandHandler(
        repository, _unitOfWorkMock.Object, _loggerMock.Object);

    var command = new CreateProductCommand(
        "Test Product", 100m, 10, Guid.NewGuid());

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    var savedProduct = await context.Products.FirstAsync(x => x.Id == result.Data.ProductId);
    Assert.NotNull(savedProduct);
    Assert.Equal("Test Product", savedProduct.Name);
}
```

## Common Issues & Solutions

### Issue 1: Query không cache

**Problem**: `[Cacheable]` attribute không hoạt động

**Solution**: Đảm bảo behavior đã register trong DI

```csharp
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
```

### Issue 2: Validation không chạy

**Problem**: Validators không được invoke

**Solution**: Đảm bảo validators đã register

```csharp
services.AddValidatorsFromAssemblyContaining<CreateProductCommandValidator>();
```

### Issue 3: Transaction không hoạt động

**Problem**: `[Transactional]` không rollback

**Solution**: Đảm bảo `IUnitOfWork` được inject

```csharp
services.AddScoped<IUnitOfWork, UnitOfWork>();
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
```

## Next Steps

1. ✅ Implement Commands/Queries/Handlers
2. ✅ Add FluentValidation validators
3. ✅ Decorate Commands với `[Transactional]`
4. ✅ Decorate Queries với `[Cacheable]`
5. ✅ Update Controllers để sử dụng IMediator
6. ✅ Add unit & integration tests
7. ✅ Monitor performance (logging)
8. ✅ Apply CQRS + MediatR cho các services khác

## Resources

- [MediatR Documentation](https://github.com/jbogard/MediatR)
- [FluentValidation Documentation](https://fluentvalidation.net/)
- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
