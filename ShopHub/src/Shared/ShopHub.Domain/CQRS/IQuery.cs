using MediatR;
using ShopHub.Common.Results;

namespace ShopHub.Domain.CQRS;

/// <summary>
/// Interface cho Query - không modify state, chỉ read
/// </summary>
/// <typeparam name="TResponse">Kiểu dữ liệu response</typeparam>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>
    where TResponse : notnull
{
}