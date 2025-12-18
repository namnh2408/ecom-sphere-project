using BuildingBlocks.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Infrastructure.Shared.UnitOfWork;

/// <summary>
/// Extension methods for registering Unit of Work in dependency injection
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds a generic EFCoreUnitOfWork for the specified DbContext
    /// Use this in each module's infrastructure service registration
    /// </summary>
    /// <typeparam name="TDbContext">The DbContext type for the module</typeparam>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection</returns>
    /// <example>
    /// In Users.Infrastructure:
    ///     services.AddUnitOfWork&lt;UsersDbContext&gt;();
    /// 
    /// In Catalog.Infrastructure:
    ///     services.AddUnitOfWork&lt;CatalogDbContext&gt;();
    /// </example>
    public static IServiceCollection AddUnitOfWork<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        services.AddScoped<IUnitOfWork>(sp =>
            new EFCoreUnitOfWork<TDbContext>(sp.GetRequiredService<TDbContext>()));

        return services;
    }
}