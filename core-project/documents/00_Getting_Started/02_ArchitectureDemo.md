# Mục tiêu & nguyên tắc

* **Mục tiêu**: tạo một source base bền vững cho dự án lớn (nhiều team, vòng đời 3–5 năm), dễ mở rộng, tách dịch vụ khi cần.
* **Nguyên tắc**: Domain-first, boundaries rõ (DDD), phụ thuộc hướng vào trong (Clean Architecture), **Modular Monolith trước – Microservices ready**.
* **Ưu tiên**: quan sát hóa (OpenTelemetry), test được, CI/CD sẵn, cấu hình tách bạch, bảo mật theo chính sách.

---

# Lựa chọn kiến trúc

1. **Modular Monolith** (khuyến nghị khởi đầu): mỗi domain là **Module** độc lập (Domain + Application + Infrastructure), cùng host trong một Web API duy nhất. Giao tiếp **qua ứng dụng** (Application) bằng in-process messages/domain events.
2. **Lộ trình microservices**: khi module đủ lớn/độc lập, tách ra service riêng. Áp dụng **Transactional Outbox + Message Broker** để đảm bảo nhất quán khi tách.

---

# Sơ đồ tầng & hướng phụ thuộc

```
┌───────────────────────────── Host (WebApi/Gateway) ─────────────────────────────┐
│  Presentation (Endpoints/Controllers, BFF)                                      │
└───────────────▲─────────────────────────────────────────────────────────────────┘
                │ calls
┌───────────────┴─────────────────────── Application ─────────────────────────────┐
│ Commands/Queries (CQRS), Handlers, Validation, Pipeline Behaviors, DTOs         │
└───────────────▲─────────────────────────────────────────────────────────────────┘
                │ uses abstractions only
┌───────────────┴────────────────────────── Domain ───────────────────────────────┐
│ Entities, Value Objects, Aggregates, Domain Events, Specifications               │
└─────────────────────────────────────────────────────────────────────────────────┘
                │ implemented by
┌───────────────▼──────────────────────── Infrastructure ──────────────────────────┐
│ EF Core/Dapper repos, Outbox, Email/SMS, Cache, Files, Message Bus, 3rd party   │
└─────────────────────────────────────────────────────────────────────────────────┘
```

* **Application** chỉ tham chiếu **Domain** & **BuildingBlocks.Abstractions**.
* **Infrastructure** tham chiếu Application & Domain để hiện thực interface.
* **Host** (WebApi/Worker) tham chiếu tất cả module cần dùng.

---

# Bố cục solution (gợi ý)

```
/Directory.Packages.props               # Quản lý version NuGet tập trung
/.editorconfig                           # Quy chuẩn code style
/.gitignore
/.github/workflows/ci.yml                # CI build/test
/.devcontainer/                          # Dev Container (tùy chọn)
/docker/                                 # Dockerfile, compose
/src
  /BuildingBlocks
    /Abstractions                        # Contracts chung (EventBus, Outbox, IClock...)
    /Infrastructure.Shared               # Hiện thực chia sẻ: EF base, Otel, Serilog, Polly, YARP (tuỳ)
  /Gateway
    /WebApi                              # Host ứng dụng (ASP.NET Core) – tải Modules động
  /Modules
    /Identity
      Identity.Domain
      Identity.Application
      Identity.Infrastructure
      Identity.Api                       # Optional nếu muốn endpoints riêng theo module
    /Inventory
      Inventory.Domain
      Inventory.Application
      Inventory.Infrastructure
      Inventory.Api
    /Procurement
      Procurement.Domain
      Procurement.Application
      Procurement.Infrastructure
      Procurement.Api
  /Workers
    Inventory.Worker                     # Background jobs; AOT-friendly
  /BFF                                   # (Tùy chọn) BFF cho web/mobile
/tests
  /Shared.Testing                        # Test utils, BaseWebAppFactory, Fixtures
  /Identity.UnitTests
  /Identity.IntegrationTests
  /Inventory.UnitTests
  /Inventory.IntegrationTests
```

> **Biến thể gọn**: Không tạo `*.Api` cho mỗi module. Thay vào đó, central `Gateway/WebApi` load endpoints từ các **Module installer** qua `IEndpointDefinition`/`IModule`.

---

# Chuẩn đặt tên & dependency

* **Tên module** = danh từ nghiệp vụ: `Inventory`, `Procurement`, `Orders`, `Billing`, `Reporting`…
* Mỗi module: `*.Domain`, `*.Application`, `*.Infrastructure`, `*.Api` (tuỳ chọn).
* `BuildingBlocks.Abstractions` chứa **contracts chung**: `IEventBus`, `IOutbox`, `IDateTime`, `IUserContext`, `IPermissionService`, `IDomainEventDispatcher`…
* Cấm **tham chiếu ngang module** trực tiếp (Inventory ↔ Procurement). Giao tiếp qua **Application events** hoặc **Domain events** rồi dùng Outbox nếu cần跨 biên.

---

# Gói NuGet/tech gợi ý (chung)

* **Persistence**: `Microsoft.EntityFrameworkCore` (+ provider SqlServer/PostgreSQL). Per-module **DbContext** + **schema riêng** (`inventory.*`, `proc.*`).
* **Validation**: `FluentValidation`. Áp dụng **PipelineBehavior** để auto-validate Commands/Queries.
* **Mediator** (tùy): `MediatR` *hoặc* tự triển khai dispatcher tối giản để giảm coupling.
* **Mapping**: `Mapster` (*nhẹ*) hoặc `AutoMapper`.
* **Observability**: `OpenTelemetry` (Tracing + Metrics + Logs), exporter OTLP → Jaeger/Tempo.
* **Logging**: `Serilog` + sink Console/Seq/ELK. CorrelationId middleware.
* **Resilience**: `Polly` (retry, circuit-breaker) – bọc HttpClient/DB.
* **Caching**: `Microsoft.Extensions.Caching.StackExchangeRedis`.
* **Messaging**: ban đầu InMemory; khi tách service dùng `RabbitMQ`/`Kafka`. Áp dụng **Transactional Outbox**.
* **Background jobs**: `Hangfire`/`Quartz`. Worker có thể **PublishAot** để tối ưu.
* **Security**: JWT bearer (Azure AD/Keycloak/Auth0/IdentityServer) + **policy-based authorization** + permission matrix theo module.

---

# Quy ước cấu hình & bí mật

* `appsettings.json` + `appsettings.{Environment}.json`. Dùng `IOptions<T>` + `ValidateOnStart()`.
* Secrets từ **User Secrets** (dev) và **Key Vault** (prod).
* **Health checks**: `/health`, `/healthz/ready`, `/healthz/live` per host.

---

# Quy ước DB & migration

* Mỗi module **một DbContext**; mỗi DbContext viết migration riêng thư mục `Migrations` của module.
* Tên schema: theo module (`inventory`, `procurement`…). Tên bảng theo Aggregate (`Items`, `Receipts`, `Orders`…).
* Bật **Optimistic Concurrency** (rowversion/timestamp). Soft-delete nếu cần (shadow property).
* **Outbox**: một bảng chung/hoặc per module: `outbox_messages` + daemon phát lên bus.

---

# Lệnh scaffold nhanh (dotnet CLI)

```bash
# 0) Solution
mkdir MyCompany && cd MyCompany
 dotnet new sln -n MyCompany

# 1) BuildingBlocks
mkdir -p src/BuildingBlocks/Abstractions src/BuildingBlocks/Infrastructure.Shared
 dotnet new classlib -f net9.0 -n BuildingBlocks.Abstractions -o src/BuildingBlocks/Abstractions
 dotnet new classlib -f net9.0 -n BuildingBlocks.Infrastructure.Shared -o src/BuildingBlocks/Infrastructure.Shared

# 2) Gateway (WebApi host)
mkdir -p src/Gateway
 dotnet new webapi -f net9.0 -n Gateway.WebApi --use-controllers -o src/Gateway/WebApi

# 3) Module Inventory (mẫu)
mkdir -p src/Modules/Inventory
 dotnet new classlib -f net9.0 -n Inventory.Domain -o src/Modules/Inventory/Inventory.Domain
 dotnet new classlib -f net9.0 -n Inventory.Application -o src/Modules/Inventory/Inventory.Application
 dotnet new classlib -f net9.0 -n Inventory.Infrastructure -o src/Modules/Inventory/Inventory.Infrastructure
 # (tuỳ) dotnet new classlib -f net9.0 -n Inventory.Api -o src/Modules/Inventory/Inventory.Api

# 4) Tham chiếu
 dotnet sln add src/**/**/*.csproj
 dotnet add src/Modules/Inventory/Inventory.Application/Inventory.Application.csproj reference \
           src/Modules/Inventory/Inventory.Domain/Inventory.Domain.csproj \
           src/BuildingBlocks/Abstractions/BuildingBlocks.Abstractions.csproj
 dotnet add src/Modules/Inventory/Inventory.Infrastructure/Inventory.Infrastructure.csproj reference \
           src/Modules/Inventory/Inventory.Application/Inventory.Application.csproj \
           src/Modules/Inventory/Inventory.Domain/Inventory.Domain.csproj \
           src/BuildingBlocks/Abstractions/BuildingBlocks.Abstractions.csproj \
           src/BuildingBlocks/Infrastructure.Shared/BuildingBlocks.Infrastructure.Shared.csproj
 dotnet add src/Gateway/WebApi/Gateway.WebApi.csproj reference \
           src/Modules/Inventory/Inventory.Application/Inventory.Application.csproj \
           src/Modules/Inventory/Inventory.Infrastructure/Inventory.Infrastructure.csproj
```

---

# Mẫu `.csproj`

**Domain** (thuần C#):

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <WarningsAsErrors>true</WarningsAsErrors>
    <AnalysisLevel>latest</AnalysisLevel>
  </PropertyGroup>
</Project>
```

**Application** (CQRS + Validation):

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="FluentValidation" />
    <PackageReference Include="MediatR" />
    <PackageReference Include="Mapster" />
  </ItemGroup>
</Project>
```

**Infrastructure** (EF + Outbox + Redis):

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Relational" />
    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" /> <!-- hoặc SqlServer -->
    <PackageReference Include="StackExchange.Redis.Extensions.Core" />
  </ItemGroup>
</Project>
```

**Gateway.WebApi** (host):

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Serilog.AspNetCore" />
    <PackageReference Include="OpenTelemetry.Exporter.Otlp" />
    <PackageReference Include="OpenTelemetry.Extensions.Hosting" />
    <PackageReference Include="Swashbuckle.AspNetCore" />
    <PackageReference Include="Polly.Extensions.Http" />
  </ItemGroup>
</Project>
```

---

# BuildingBlocks: contracts mẫu

```csharp
namespace BuildingBlocks.Abstractions;
public interface IClock { DateTime UtcNow { get; } }
public interface IDomainEvent { DateTime OccurredOnUtc { get; } }
public interface IEventBus
{
    Task PublishAsync<T>(T @event, CancellationToken ct = default) where T : class;
}
public interface IOutbox
{
    Task EnqueueAsync(object message, string type, CancellationToken ct = default);
}
```

---

# Domain & Application skeleton (Inventory)

**Entity/VO/Domain event**

```csharp
namespace Inventory.Domain.Items;
public sealed class Item : Entity<ItemId>
{
    private Item(ItemId id, string sku, string name, int qty)
        => (Id, Sku, Name, Quantity) = (id, sku, name, qty);
    public string Sku { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public int Quantity { get; private set; }

    public static Result<Item> Create(string sku, string name, int qty, IClock clock)
    {
        if (string.IsNullOrWhiteSpace(sku) || string.IsNullOrWhiteSpace(name) || qty < 0)
            return Result<Item>.Fail("Item.Invalid", "Invalid input");
        var item = new Item(ItemId.New(), sku, name, qty);
        item.Raise(new ItemCreated(item.Id, clock.UtcNow));
        return Result<Item>.Success(item);
    }
}
public readonly record struct ItemId(Guid Value) { public static ItemId New() => new(Guid.NewGuid()); }
public sealed record ItemCreated(ItemId Id, DateTime OccurredOnUtc) : IDomainEvent;
```

**CQRS + Validation**

```csharp
// Application/Items/CreateItem.cs
public static class CreateItem
{
    public sealed record Command(string Sku, string Name, int Quantity) : IRequest<Result<Guid>>;

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Sku).NotEmpty().MaximumLength(64);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
            RuleFor(x => x.Quantity).GreaterThanOrEqualTo(0);
        }
    }

    internal sealed class Handler(IInventoryRepository repo, IClock clock) : IRequestHandler<Command, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(Command request, CancellationToken ct)
        {
            var entity = Item.Create(request.Sku, request.Name, request.Quantity, clock);
            if (!entity.IsSuccess) return Result<Guid>.Fail(entity.Error.Code, entity.Error.Message);
            await repo.AddAsync(entity.Value!, ct);
            await repo.UnitOfWork.SaveChangesAsync(ct);
            return Result<Guid>.Success(entity.Value!.Id.Value);
        }
    }
}
```

**Pipeline Behaviors** (validation, transaction):

```csharp
public sealed class ValidationBehavior<TRequest, TResponse>(IValidator<TRequest>? validator)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (validator is not null)
        {
            var result = await validator.ValidateAsync(request, ct);
            if (!result.IsValid) throw new ValidationException(result.Errors);
        }
        return await next();
    }
}
```

---

# Program.cs (Gateway) – đăng ký module theo convention

```csharp
var builder = WebApplication.CreateBuilder(args);

// Observability & logging
builder.Services.AddOpenTelemetry().UseOtlpExporter();
// Serilog config (rút gọn)

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Đăng ký shared services
builder.Services.AddSingleton<IClock, SystemClock>();

// Load installers từ các module (Infrastructure/Application)
builder.Services.Scan(s => s
    .FromApplicationDependencies(a => a.FullName!.StartsWith("Inventory") || a.FullName!.StartsWith("Procurement"))
    .AddClasses(c => c.AssignableTo<IModuleInstaller>())
    .AsImplementedInterfaces()
    .WithSingletonLifetime());

var app = builder.Build();
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }

// Map endpoints từ các module
var endpointProviders = app.Services.GetServices<IEndpointDefinition>();
foreach (var ep in endpointProviders) ep.Map(app);

app.MapGet("/health", () => Results.Ok("ok"));
app.Run();
```

`IModuleInstaller` đăng ký DI + DB của module; `IEndpointDefinition` map routes.

---

# Dev Experience

* **Directory.Packages.props**: khoá version NuGet toàn solution, cập nhật đồng nhất.
* **.editorconfig** + `dotnet format` + `Analyzers` (StyleCop/FxCop) + `WarningsAsErrors` ở Core.
* **Dev Container**: dựng sẵn Postgres, Redis, Jaeger, Seq bằng `docker-compose`.

**docker-compose mẫu**

```yaml
version: "3.9"
services:
  postgres:
    image: postgres:16
    environment:
      POSTGRES_USER: app
      POSTGRES_PASSWORD: app
      POSTGRES_DB: app
    ports: ["5432:5432"]
  redis:
    image: redis:7
    ports: ["6379:6379"]
  jaeger:
    image: jaegertracing/all-in-one:1.57
    ports: ["16686:16686", "4317:4317"]
  seq:
    image: datalust/seq:latest
    environment: { ACCEPT_EULA: "Y" }
    ports: ["5341:5341", "8081:80"]
```

---

# Test chiến lược

* **Unit tests**: Domain & Application (không chạm DB). Dùng **Bogus/AutoFixture** tạo mẫu dữ liệu.
* **Integration tests**: WebApi + EF Core (InMemory/Testcontainers → Postgres/SqlServer thật), **WebApplicationFactory**.
* **Contract tests** (khi tách service): Pact hoặc schema-first + Dredd.

---

# CI/CD skeleton (GitHub Actions rút gọn)

```yaml
name: ci
on: [push, pull_request]
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with: { dotnet-version: '9.0.x' }
      - run: dotnet restore
      - run: dotnet build --no-restore -c Release
      - run: dotnet test  --no-build   -c Release --collect:"XPlat Code Coverage"
      - run: dotnet publish src/Gateway/WebApi/Gateway.WebApi.csproj -c Release -o out
      # (tuỳ) build docker & push ghcr.io/mycompany/gateway:sha
```

---

# Bảo mật & quyền

* AuthN: OAuth2/OIDC (Azure AD B2C/Keycloak/Auth0). JWT bearer, rotate keys.
* AuthZ: **Policy-based** + **permission matrix** theo module (ví dụ: `Inventory.Read`, \`Inventory.E
