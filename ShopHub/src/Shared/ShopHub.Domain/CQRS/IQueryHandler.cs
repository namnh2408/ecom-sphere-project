using MediatR;
using ShopHub.Common.Results;

namespace ShopHub.Domain.CQRS;

/// <summary>
/// Interface cho Query Handler
/// </summary>
/// <typeparam name="TQuery">Kiểu query</typeparam>
/// <typeparam name="TResponse">Kiểu dữ liệu response</typeparam>
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
    where TResponse : notnull
{
}