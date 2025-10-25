using Microsoft.EntityFrameworkCore;
using ShopHub.Services.UserService.Domain.Repositories;
using ShopHub.Services.UserService.Infrastructure.Persistence;
using ShopHub.Services.UserService.Infrastructure.Persistence.Repositories;

namespace ShopHub.Services.UserService.Infrastructure;

/// <summary>
/// Dependency Injection registration cho User Service
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Thêm User Service dependencies
    /// </summary>
    public static IServiceCollection AddUserService(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register DbContext
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured");

        services.AddDbContext<UserDbContext>(options =>
            options.UseSqlServer(connectionString, builder =>
                builder.MigrationsAssembly("ShopHub")));

        // Register Repository
        services.AddScoped<IUserRepository, UserRepository>();

        // Register Handlers (MediatR)
        services.AddMediatR(cfg => 
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        return services;
    }
}