using MediatR;
using System.Diagnostics;

namespace ShopHub.Common.CQRS;

/// <summary>
/// MediatR Behavior cho logging và monitoring execution time
/// </summary>
/// <typeparam name="TRequest">Kiểu request</typeparam>
/// <typeparam name="TResponse">Kiểu response</typeparam>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Handle logging
    /// </summary>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        _logger.LogInformation($"[CQRS] Executing {requestName}");

        var sw = Stopwatch.StartNew();
        try
        {
            var response = await next();
            sw.Stop();

            _logger.LogInformation(
                $"[CQRS] Completed {requestName} in {sw.ElapsedMilliseconds}ms");

            return response;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(
                ex,
                $"[CQRS] Error executing {requestName} after {sw.ElapsedMilliseconds}ms");
            throw;
        }
    }
}