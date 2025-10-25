using System.Reflection;
using MediatR;
using ShopHub.Domain.CQRS;
using ShopHub.Domain.Abstractions;
using Microsoft.Extensions.Logging;

namespace ShopHub.Common.CQRS;

/// <summary>
/// Attribute để đánh dấu command cần transaction
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class TransactionalAttribute : Attribute
{
}

/// <summary>
/// MediatR Behavior cho transaction handling trên Commands
/// </summary>
/// <typeparam name="TRequest">Kiểu request (Command)</typeparam>
/// <typeparam name="TResponse">Kiểu response</typeparam>
public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull, ICommand
    where TResponse : notnull
{
    private readonly IUnitOfWork? _unitOfWork;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    public TransactionBehavior(
        IUnitOfWork? unitOfWork,
        ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handle transaction
    /// </summary>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (_unitOfWork == null)
            return await next();

        var transactionalAttribute = typeof(TRequest)
            .GetCustomAttributes(typeof(TransactionalAttribute), true)
            .FirstOrDefault() as TransactionalAttribute;

        // Nếu không có TransactionalAttribute hoặc không implement IUnitOfWork, skip transaction
        if (transactionalAttribute == null)
            return await next();

        _logger.LogInformation($"[CQRS] Starting transaction for {typeof(TRequest).Name}");

        using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var response = await next();

            // Commit nếu handler thành công
            await _unitOfWork.CommitAsync(cancellationToken);
            _logger.LogInformation($"[CQRS] Transaction committed for {typeof(TRequest).Name}");

            return response;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            _logger.LogError(ex, $"[CQRS] Transaction rolled back for {typeof(TRequest).Name}");
            throw;
        }
    }
}

/// <summary>
/// Transactional Command Handler base class
/// </summary>
/// <typeparam name="TCommand">Kiểu command</typeparam>
/// <typeparam name="TResponse">Kiểu response</typeparam>
[Transactional]
public abstract class TransactionalCommandHandler<TCommand, TResponse>
    : ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
    where TResponse : notnull
{
    protected readonly IUnitOfWork UnitOfWork;
    protected readonly ILogger Logger;

    protected TransactionalCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        UnitOfWork = unitOfWork;
        Logger = logger;
    }

    public abstract Task<Common.Results.Result<TResponse>> Handle(
        TCommand request,
        CancellationToken cancellationToken);
}