using Microsoft.AspNetCore.Mvc;
using Gateway.WebApi.Exceptions;
using Gateway.WebApi.Models;
using BuildingBlocks.Abstractions;

namespace Gateway.WebApi.Helpers;

/// <summary>
/// Helper class to create consistent API responses and throw custom exceptions
/// </summary>
public static class ApiResponseHelper
{
    #region Success Responses

    /// <summary>
    /// Create a success response with data
    /// </summary>
    public static IActionResult Success<T>(this ControllerBase controller, T data, int statusCode = StatusCodes.Status200OK)
    {
        return controller.StatusCode(statusCode, data);
    }

    /// <summary>
    /// Create a success response with message
    /// </summary>
    public static IActionResult Success(this ControllerBase controller, string message, int statusCode = StatusCodes.Status200OK)
    {
        return controller.StatusCode(statusCode, new SuccessResponse { Message = message });
    }

    /// <summary>
    /// Create a created response (201)
    /// </summary>
    public static IActionResult Created<T>(this ControllerBase controller, T data, string? location = null)
    {
        if (!string.IsNullOrEmpty(location))
        {
            controller.Response.Headers.Location = location;
        }
        return controller.StatusCode(StatusCodes.Status201Created, data);
    }

    /// <summary>
    /// Create a no content response (204)
    /// </summary>
    public static IActionResult NoContent(this ControllerBase controller)
    {
        return controller.StatusCode(StatusCodes.Status204NoContent);
    }

    #endregion

    #region Error Responses - Throw Exceptions

    /// <summary>
    /// Throw a validation error exception
    /// </summary>
    public static void ThrowValidation(string message, Dictionary<string, object>? errors = null)
    {
        throw new ValidationException(message, errors);
    }

    /// <summary>
    /// Throw a validation error exception with custom error code
    /// </summary>
    public static void ThrowValidation(string errorCode, string message, Dictionary<string, object>? errors = null)
    {
        throw new ValidationException(errorCode, message, errors);
    }

    /// <summary>
    /// Throw a resource not found exception
    /// </summary>
    public static void ThrowNotFound(string resourceName, object resourceId)
    {
        throw new ResourceNotFoundException(resourceName, resourceId);
    }

    /// <summary>
    /// Throw a resource not found exception with custom message
    /// </summary>
    public static void ThrowNotFound(string errorCode, string message)
    {
        throw new ResourceNotFoundException(errorCode, message);
    }

    /// <summary>
    /// Throw a duplicate resource exception
    /// </summary>
    public static void ThrowDuplicate(string resourceName, string fieldName, object fieldValue)
    {
        throw new DuplicateResourceException(resourceName, fieldName, fieldValue);
    }

    /// <summary>
    /// Throw a duplicate resource exception with custom message
    /// </summary>
    public static void ThrowDuplicate(string errorCode, string message)
    {
        throw new DuplicateResourceException(errorCode, message);
    }

    /// <summary>
    /// Throw an unauthorized exception
    /// </summary>
    public static void ThrowUnauthorized(string message = "User is not authorized to perform this action")
    {
        throw new UnauthorizedException(message);
    }

    /// <summary>
    /// Throw an unauthorized exception with custom error code
    /// </summary>
    public static void ThrowUnauthorized(string errorCode, string message)
    {
        throw new UnauthorizedException(errorCode, message);
    }

    /// <summary>
    /// Throw a forbidden exception
    /// </summary>
    public static void ThrowForbidden(string message = "User does not have permission to perform this action")
    {
        throw new ForbiddenException(message);
    }

    /// <summary>
    /// Throw a forbidden exception with custom error code
    /// </summary>
    public static void ThrowForbidden(string errorCode, string message)
    {
        throw new ForbiddenException(errorCode, message);
    }

    /// <summary>
    /// Throw a business rule exception
    /// </summary>
    public static void ThrowBusinessRule(string errorCode, string message, int statusCode = StatusCodes.Status400BadRequest, Dictionary<string, object>? details = null)
    {
        throw new BusinessRuleException(errorCode, message, statusCode, details);
    }

    /// <summary>
    /// Throw a custom API exception
    /// </summary>
    public static void ThrowCustom(string errorCode, string message, int statusCode = StatusCodes.Status400BadRequest, Dictionary<string, object>? errorDetails = null)
    {
        throw new ApiException(errorCode, message, statusCode, errorDetails);
    }

    #endregion

    #region Error Responses - Return Responses

    /// <summary>
    /// Create a validation error response
    /// </summary>
    public static IActionResult ValidationError(this ControllerBase controller, string message, Dictionary<string, object>? errors = null)
    {
        var response = new ErrorResponse
        {
            Error = "VALIDATION_ERROR",
            Message = message,
            Details = errors
        };
        return controller.BadRequest(response);
    }

    /// <summary>
    /// Create a validation error response with custom error code
    /// </summary>
    public static IActionResult ValidationError(this ControllerBase controller, string errorCode, string message, Dictionary<string, object>? errors = null)
    {
        var response = new ErrorResponse
        {
            Error = errorCode,
            Message = message,
            Details = errors
        };
        return controller.BadRequest(response);
    }

    /// <summary>
    /// Create a not found error response
    /// </summary>
    public static IActionResult NotFoundError(this ControllerBase controller, string resourceName, object resourceId)
    {
        var response = new ErrorResponse
        {
            Error = "RESOURCE_NOT_FOUND",
            Message = $"{resourceName} with ID '{resourceId}' not found"
        };
        return controller.NotFound(response);
    }

    /// <summary>
    /// Create a not found error response with custom message
    /// </summary>
    public static IActionResult NotFoundError(this ControllerBase controller, string errorCode, string message)
    {
        var response = new ErrorResponse
        {
            Error = errorCode,
            Message = message
        };
        return controller.NotFound(response);
    }

    /// <summary>
    /// Create a duplicate resource error response
    /// </summary>
    public static IActionResult DuplicateError(this ControllerBase controller, string resourceName, string fieldName, object fieldValue)
    {
        var response = new ErrorResponse
        {
            Error = "RESOURCE_ALREADY_EXISTS",
            Message = $"{resourceName} with {fieldName} '{fieldValue}' already exists"
        };
        return controller.Conflict(response);
    }

    /// <summary>
    /// Create a duplicate resource error response with custom message
    /// </summary>
    public static IActionResult DuplicateError(this ControllerBase controller, string errorCode, string message)
    {
        var response = new ErrorResponse
        {
            Error = errorCode,
            Message = message
        };
        return controller.Conflict(response);
    }

    /// <summary>
    /// Create an unauthorized error response
    /// </summary>
    public static IActionResult UnauthorizedError(this ControllerBase controller, string message = "User is not authorized to perform this action")
    {
        var response = new ErrorResponse
        {
            Error = "UNAUTHORIZED",
            Message = message
        };
        return controller.Unauthorized(response);
    }

    /// <summary>
    /// Create an unauthorized error response with custom error code
    /// </summary>
    public static IActionResult UnauthorizedError(this ControllerBase controller, string errorCode, string message)
    {
        var response = new ErrorResponse
        {
            Error = errorCode,
            Message = message
        };
        return controller.Unauthorized(response);
    }

    /// <summary>
    /// Create a forbidden error response
    /// </summary>
    public static IActionResult ForbiddenError(this ControllerBase controller, string message = "User does not have permission to perform this action")
    {
        var response = new ErrorResponse
        {
            Error = "FORBIDDEN",
            Message = message
        };
        return controller.StatusCode(StatusCodes.Status403Forbidden, response);
    }

    /// <summary>
    /// Create a forbidden error response with custom error code
    /// </summary>
    public static IActionResult ForbiddenError(this ControllerBase controller, string errorCode, string message)
    {
        var response = new ErrorResponse
        {
            Error = errorCode,
            Message = message
        };
        return controller.StatusCode(StatusCodes.Status403Forbidden, response);
    }

    /// <summary>
    /// Create a business rule error response
    /// </summary>
    public static IActionResult BusinessRuleError(this ControllerBase controller, string errorCode, string message, int statusCode = StatusCodes.Status400BadRequest, Dictionary<string, object>? details = null)
    {
        var response = new ErrorResponse
        {
            Error = errorCode,
            Message = message,
            Details = details
        };
        return controller.StatusCode(statusCode, response);
    }

    /// <summary>
    /// Create a custom error response
    /// </summary>
    public static IActionResult CustomError(this ControllerBase controller, string errorCode, string message, int statusCode = StatusCodes.Status400BadRequest, Dictionary<string, object>? details = null)
    {
        var response = new ErrorResponse
        {
            Error = errorCode,
            Message = message,
            Details = details
        };
        return controller.StatusCode(statusCode, response);
    }

    #endregion

    #region Conditional Helpers

    /// <summary>
    /// Throw validation error if condition is true
    /// </summary>
    public static void ThrowValidationIf(bool condition, string message, Dictionary<string, object>? errors = null)
    {
        if (condition)
        {
            throw new ValidationException(message, errors);
        }
    }

    /// <summary>
    /// Throw validation error if condition is true with custom error code
    /// </summary>
    public static void ThrowValidationIf(bool condition, string errorCode, string message, Dictionary<string, object>? errors = null)
    {
        if (condition)
        {
            throw new ValidationException(errorCode, message, errors);
        }
    }

    /// <summary>
    /// Throw not found error if value is null
    /// </summary>
    public static T ThrowNotFoundIfNull<T>(T? value, string resourceName, object resourceId) where T : class
    {
        if (value is null)
        {
            throw new ResourceNotFoundException(resourceName, resourceId);
        }
        return value;
    }

    /// <summary>
    /// Throw not found error if value is null with custom message
    /// </summary>
    public static T ThrowNotFoundIfNull<T>(T? value, string errorCode, string message) where T : class
    {
        if (value is null)
        {
            throw new ResourceNotFoundException(errorCode, message);
        }
        return value;
    }

    #endregion

    #region Result Pattern Helpers

    /// <summary>
    /// Convert Result pattern to action result
    /// </summary>
    public static IActionResult ToActionResult<T>(this Result<T> result, ControllerBase controller, int successStatusCode = StatusCodes.Status200OK)
    {
        if (result.IsSuccess)
        {
            return controller.StatusCode(successStatusCode, result.Value);
        }

        var errorResponse = new ErrorResponse
        {
            Error = result.Error?.Code ?? "UNKNOWN_ERROR",
            Message = result.Error?.Message ?? "An error occurred"
        };

        return controller.BadRequest(errorResponse);
    }

    #endregion
}

/// <summary>
/// Extension methods for convenient imports - using static
/// </summary>
/// <remarks>
/// Usage: using static Gateway.WebApi.Helpers.ApiResponseExtensions;
/// Then: ThrowValidation("message"), ThrowNotFound("Resource", 123), etc.
/// </remarks>
public static class ApiResponseExtensions
{
    public static void ThrowValidation(string message, Dictionary<string, object>? errors = null) =>
        ApiResponseHelper.ThrowValidation(message, errors);

    public static void ThrowValidation(string errorCode, string message, Dictionary<string, object>? errors = null) =>
        ApiResponseHelper.ThrowValidation(errorCode, message, errors);

    public static void ThrowNotFound(string resourceName, object resourceId) =>
        ApiResponseHelper.ThrowNotFound(resourceName, resourceId);

    public static void ThrowNotFound(string errorCode, string message) =>
        ApiResponseHelper.ThrowNotFound(errorCode, message);

    public static void ThrowDuplicate(string resourceName, string fieldName, object fieldValue) =>
        ApiResponseHelper.ThrowDuplicate(resourceName, fieldName, fieldValue);

    public static void ThrowDuplicate(string errorCode, string message) =>
        ApiResponseHelper.ThrowDuplicate(errorCode, message);

    public static void ThrowUnauthorized(string message = "User is not authorized to perform this action") =>
        ApiResponseHelper.ThrowUnauthorized(message);

    public static void ThrowUnauthorized(string errorCode, string message) =>
        ApiResponseHelper.ThrowUnauthorized(errorCode, message);

    public static void ThrowForbidden(string message = "User does not have permission to perform this action") =>
        ApiResponseHelper.ThrowForbidden(message);

    public static void ThrowForbidden(string errorCode, string message) =>
        ApiResponseHelper.ThrowForbidden(errorCode, message);

    public static void ThrowBusinessRule(string errorCode, string message, int statusCode = StatusCodes.Status400BadRequest, Dictionary<string, object>? details = null) =>
        ApiResponseHelper.ThrowBusinessRule(errorCode, message, statusCode, details);

    public static void ThrowCustom(string errorCode, string message, int statusCode = StatusCodes.Status400BadRequest, Dictionary<string, object>? errorDetails = null) =>
        ApiResponseHelper.ThrowCustom(errorCode, message, statusCode, errorDetails);
}