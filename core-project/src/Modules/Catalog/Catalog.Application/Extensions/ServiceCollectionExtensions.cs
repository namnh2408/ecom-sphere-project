using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Catalog.Application.Extensions;

/// <summary>
/// Service collection extensions for Catalog application layer
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Catalog application services to the DI container
    /// </summary>
    public static IServiceCollection AddCatalogApplicationServices(this IServiceCollection services)
    {
        // Add MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        // Add FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}