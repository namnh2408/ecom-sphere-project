using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Gateway.WebApi.Models;

namespace Gateway.WebApi.Middleware;

/// <summary>
/// Middleware for validating JSON input before it reaches the controllers
/// </summary>
public class InputValidationMiddleware(RequestDelegate next, ILogger<InputValidationMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // Only validate POST, PUT, PATCH requests with JSON body
        if (!ShouldValidateRequest(context))
        {
            await next(context);
            return;
        }

        try
        {
            // Buffer the request body so we can read it multiple times
            context.Request.EnableBuffering();
            
            // Read the body
            using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;

            // Validate JSON structure
            if (string.IsNullOrWhiteSpace(body))
            {
                await SendValidationError(context, "Request body cannot be empty");
                return;
            }

            // Try to parse JSON to catch malformed JSON early
            try
            {
                JsonSerializer.Deserialize<JsonElement>(body);
            }
            catch (JsonException jsonEx)
            {
                await SendValidationError(context, $"Invalid JSON format: {jsonEx.Message}");
                return;
            }

            await next(context);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error in input validation middleware");
            await SendValidationError(context, "An error occurred while validating the request");
        }
    }

    private static bool ShouldValidateRequest(HttpContext context)
    {
        var method = context.Request.Method.ToUpper();
        var contentType = context.Request.ContentType?.ToLower() ?? string.Empty;

        return (method is "POST" or "PUT" or "PATCH" or "DELETE") &&
               (contentType.Contains("application/json") || contentType == string.Empty);
    }

    private static Task SendValidationError(HttpContext context, string message)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/json";

        var errorResponse = new ErrorResponse
        {
            Error = "VALIDATION_ERROR",
            Message = message
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
    }
}