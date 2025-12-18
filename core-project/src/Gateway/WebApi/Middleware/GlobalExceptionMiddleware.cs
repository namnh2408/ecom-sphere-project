using System.Text.Json;
using Gateway.WebApi.Exceptions;
using Gateway.WebApi.Models;

namespace Gateway.WebApi.Middleware;

/// <summary>
/// Global exception handling middleware that catches all unhandled exceptions
/// and returns a consistent error response format
/// </summary>
public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
            
            // Handle cases where response status code indicates an error but no body was set
            if (context.Response.StatusCode >= 400 && context.Response.Body.Length == 0 && !context.Response.HasStarted)
            {
                context.Response.ContentType = "application/json";
                
                var errorResponse = new ErrorResponse
                {
                    Error = context.Response.StatusCode switch
                    {
                        400 => "BAD_REQUEST",
                        401 => "UNAUTHORIZED",
                        403 => "FORBIDDEN",
                        404 => "NOT_FOUND",
                        409 => "CONFLICT",
                        422 => "UNPROCESSABLE_ENTITY",
                        500 => "INTERNAL_SERVER_ERROR",
                        503 => "SERVICE_UNAVAILABLE",
                        _ => "ERROR"
                    },
                    Message = context.Response.StatusCode switch
                    {
                        400 => "Invalid request",
                        401 => "Unauthorized access",
                        403 => "Access forbidden",
                        404 => "Resource not found",
                        409 => "Resource conflict",
                        422 => "Unprocessable entity",
                        500 => "An unexpected error occurred",
                        503 => "Service unavailable",
                        _ => "An error occurred"
                    }
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
            }
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "An unhandled exception has occurred: {ExceptionMessage}", exception.Message);
            await HandleExceptionAsync(context, exception, logger);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception, ILogger<GlobalExceptionMiddleware> logger)
    {
        if (!context.Response.HasStarted)
        {
            context.Response.ContentType = "application/json";
        }

        var response = new ErrorResponse();

        switch (exception)
        {
            // Handle custom API exceptions
            case ApiException apiEx:
                context.Response.StatusCode = apiEx.StatusCode;
                response.Error = apiEx.ErrorCode;
                response.Message = apiEx.Message;
                response.Details = apiEx.ErrorDetails;
                
                // Log based on status code
                if (apiEx.StatusCode >= 500)
                {
                    logger.LogError(apiEx, "API exception: {ErrorCode} - {Message}", apiEx.ErrorCode, apiEx.Message);
                }
                else
                {
                    logger.LogWarning(apiEx, "API exception: {ErrorCode} - {Message}", apiEx.ErrorCode, apiEx.Message);
                }
                break;

            case ArgumentNullException argNullEx:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response.Error = "ARGUMENT_NULL";
                response.Message = $"Required parameter is missing: {argNullEx.ParamName}";
                break;

            case ArgumentException argEx:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response.Error = "INVALID_ARGUMENT";
                response.Message = argEx.Message;
                break;

            case InvalidOperationException invOpEx:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response.Error = "INVALID_OPERATION";
                response.Message = invOpEx.Message;
                break;

            case TimeoutException timeoutEx:
                context.Response.StatusCode = StatusCodes.Status504GatewayTimeout;
                response.Error = "REQUEST_TIMEOUT";
                response.Message = "The request timed out. Please try again.";
                logger.LogWarning(timeoutEx, "Request timeout occurred");
                break;

            case IOException ioEx:
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                response.Error = "IO_ERROR";
                response.Message = "An I/O error occurred while processing your request.";
                logger.LogError(ioEx, "I/O error occurred");
                break;

            case OperationCanceledException canceledEx:
                context.Response.StatusCode = StatusCodes.Status408RequestTimeout;
                response.Error = "REQUEST_CANCELED";
                response.Message = "The request was canceled.";
                logger.LogWarning(canceledEx, "Operation was canceled");
                break;

            default:
                // Log full exception details for debugging
                logger.LogError(
                    exception,
                    "Unhandled exception of type {ExceptionType}: {ExceptionMessage}\nStackTrace: {StackTrace}",
                    exception.GetType().Name,
                    exception.Message,
                    exception.StackTrace);

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                response.Error = "INTERNAL_SERVER_ERROR";
                response.Message = "An unexpected error occurred while processing your request.";
                
                // Include exception details in development environment only
                if (context.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
                {
                    response.Details = new Dictionary<string, object>
                    {
                        { "exceptionType", exception.GetType().Name },
                        { "exceptionMessage", exception.Message },
                        { "stackTrace", exception.StackTrace ?? string.Empty },
                        { "innerException", exception.InnerException?.Message ?? "None" }
                    };
                }

                break;
        }

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}