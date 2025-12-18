using BuildingBlocks.Infrastructure.Shared.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Catalog.Domain.Repositories;
using Catalog.Infrastructure.Persistence;
using Catalog.Infrastructure.Repositories;

namespace Catalog.Infrastructure.Extensions;

/// <summary>
/// Service collection extensions for Catalog infrastructure layer
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Catalog infrastructure services to the DI container
    /// </summary>
    public static IServiceCollection AddCatalogInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        // Add DbContext
        services.AddDbContext<CatalogDbContext>(options =>
            options.UseSqlServer(connectionString)
        );

        // Add repositories
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();

        // Add generic Unit of Work from BuildingBlocks
        services.AddUnitOfWork<CatalogDbContext>();

        return services;
    }
}