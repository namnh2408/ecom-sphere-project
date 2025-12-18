using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Users.Application.Extensions;

/// <summary>
/// Extension methods for Users.Application dependency injection
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add Users application services to the dependency injection container
    /// </summary>
    public static IServiceCollection AddUsersApplication(this IServiceCollection services)
    {
        // Register MediatR
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly);
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        // Register validators
        services.AddValidatorsFromAssembly(typeof(ServiceCollectionExtensions).Assembly);

        return services;
    }
}

/// <summary>
/// Pipeline behavior for validation
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .Where(r => r.Errors.Count != 0)
            .SelectMany(r => r.Errors)
            .ToList();

        if (failures.Count == 0)
        {
            return await next();
        }

        throw new ValidationException(failures);
    }
}