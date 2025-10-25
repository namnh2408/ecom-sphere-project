using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShopHub.Common.CQRS;
using ShopHub.Domain.CQRS;
using ShopHub.Infrastructure.Caching;
using ShopHub.Services.ProductService.Application.Commands;
using ShopHub.Services.ProductService.Application.DTOs;
using ShopHub.Services.ProductService.Application.Handlers;
using ShopHub.Services.ProductService.Application.Queries;
using ShopHub.Services.ProductService.Domain.Repositories;
using ShopHub.Services.ProductService.Infrastructure.Persistence;
using ShopHub.Services.ProductService.Infrastructure.Persistence.Repositories;
using StackExchange.Redis;

namespace ShopHub.Services.ProductService.Infrastructure;

/// <summary>
/// Dependency Injection registration cho Product Service
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Thêm Product Service vào DI container
    /// </summary>
    public static IServiceCollection AddProductService(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext
        var connectionString = configuration.GetConnectionString("ProductDb")
            ?? throw new InvalidOperationException("ProductDb connection string not found");

        services.AddDbContext<ProductDbContext>(options =>
            options.UseSqlServer(connectionString,
                sqlServerOptions =>
                {
                    sqlServerOptions.MigrationsAssembly(
                        typeof(ServiceCollectionExtensions).Assembly.FullName);
                    sqlServerOptions.CommandTimeout(30);
                }));

        // Repositories
        services.AddScoped<IProductRepository, ProductRepository>();
        
        // Query Repository (Dapper for read operations)
        services.AddScoped<IProductQueryRepository, ProductQueryRepository>();

        // Redis Caching
        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnection))
        {
            try
            {
                var multiplexer = ConnectionMultiplexer.Connect(redisConnection);
                services.AddSingleton<IConnectionMultiplexer>(multiplexer);
                services.AddSingleton<ICacheService, RedisCacheService>();
            }
            catch (Exception ex)
            {
                // Fallback to in-memory cache if Redis connection fails
                services.AddMemoryCache();
                services.AddSingleton<ICacheService, InMemoryCacheService>();
            }
        }
        else
        {
            // Fallback to in-memory cache if Redis not configured
            services.AddMemoryCache();
            services.AddSingleton<ICacheService, InMemoryCacheService>();
        }

        // MediatR - CQRS Pattern
        var assembly = typeof(ServiceCollectionExtensions).Assembly;
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        // Command Handlers
        services.AddScoped<ICommandHandler<CreateProductCommand, ProductResponseDto>, CreateProductCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateProductCommand, ProductResponseDto>, UpdateProductCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteProductCommand>, DeleteProductCommandHandler>();

        // Query Handlers
        services.AddScoped<IQueryHandler<GetProductByIdQuery, ProductResponseDto>, GetProductByIdQueryHandler>();
        services.AddScoped<IQueryHandler<SearchProductsQuery, Common.Pagination.PaginatedList<ProductResponseDto>>, SearchProductsQueryHandler>();

        // MediatR Behaviors (Pipeline)
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

        // FluentValidation - validators will be discovered by MediatR assembly scanning
        var validatorType = typeof(CreateProductCommandValidator);
        var validators = validatorType.Assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && 
                   t.GetInterfaces().Any(i => 
                       i.IsGenericType && 
                       i.GetGenericTypeDefinition() == typeof(IValidator<>)));
        
        foreach (var validator in validators)
        {
            var iface = validator.GetInterfaces()
                .First(i => i.IsGenericType && 
                       i.GetGenericTypeDefinition() == typeof(IValidator<>));
            services.AddScoped(iface, validator);
        }

        // Domain Services
        // services.AddScoped<IProductDomainService, ProductDomainService>();

        // Application Services
        // services.AddScoped<IProductApplicationService, ProductApplicationService>();

        // AutoMapper
        // services.AddAutoMapper(typeof(ProductMappingProfile));

        return services;
    }

    /// <summary>
    /// Áp dụng migrations
    /// </summary>
    public static async Task ApplyMigrationsAsync(this IServiceProvider serviceProvider)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
            await dbContext.Database.MigrateAsync();
        }
    }
}