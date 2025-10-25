using MediatR;
using ShopHub.Common.Results;

namespace ShopHub.Domain.CQRS;

/// <summary>
/// Interface cho Command không có return value
/// </summary>
public interface ICommand : IRequest<Result>
{
}

/// <summary>
/// Interface cho Command có return value
/// </summary>
/// <typeparam name="TResponse">Kiểu dữ liệu response</typeparam>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>
    where TResponse : notnull
{
}