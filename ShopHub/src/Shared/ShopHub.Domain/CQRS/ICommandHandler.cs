using MediatR;
using ShopHub.Common.Results;

namespace ShopHub.Domain.CQRS;

/// <summary>
/// Interface cho Command Handler không có return value
/// </summary>
/// <typeparam name="TCommand">Kiểu command</typeparam>
public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand
{
}

/// <summary>
/// Interface cho Command Handler có return value
/// </summary>
/// <typeparam name="TCommand">Kiểu command</typeparam>
/// <typeparam name="TResponse">Kiểu dữ liệu response</typeparam>
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>
    where TResponse : notnull
{
}