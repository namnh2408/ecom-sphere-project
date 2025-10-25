using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using ShopHub.Common.Results;
using ShopHub.Domain.CQRS;

namespace ShopHub.Common.CQRS;

/// <summary>
/// MediatR Behavior cho validation trước khi execute command/query
/// </summary>
/// <typeparam name="TRequest">Kiểu request (Command hoặc Query)</typeparam>
/// <typeparam name="TResponse">Kiểu response</typeparam>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IBaseRequest
    where TResponse : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

    public ValidationBehavior(
        IEnumerable<IValidator<TRequest>> validators,
        ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    {
        _validators = validators;
        _logger = logger;
    }

    /// <summary>
    /// Handle validation
    /// </summary>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var validationFailures = new List<ValidationFailure>();

        foreach (var validator in _validators)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            validationFailures.AddRange(validationResult.Errors);
        }

        if (validationFailures.Any())
        {
            _logger.LogWarning($"Validation failed for {typeof(TRequest).Name}: {string.Join(", ", validationFailures.Select(x => x.ErrorMessage))}");

            var errorMessages = validationFailures
                .Select(f => $"{f.PropertyName}: {f.ErrorMessage}")
                .ToList();

            // Nếu là Command, trả về lỗi validation
            if (request is ICommand)
            {
                return (TResponse)(object)Result.Failure(errorMessages, 400);
            }

            // Nếu là Query, trả về lỗi validation
            if (request is IQuery<object>)
            {
                return (TResponse)(object)Result<object>.Failure(errorMessages, 400);
            }
        }

        return await next();
    }
}

/// <summary>
/// Interface marker để xác định request cần validate
/// </summary>
public interface IBaseRequest : IRequest
{
}