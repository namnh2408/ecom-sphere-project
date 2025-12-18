namespace Gateway.WebApi.Exceptions;

/// <summary>
/// Custom API exception that allows returning structured error responses
/// </summary>
public class ApiException : Exception
{
    /// <summary>
    /// Error code identifier (e.g., "USER_NOT_FOUND", "INVALID_EMAIL")
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// HTTP status code for this error
    /// </summary>
    public int StatusCode { get; }

    /// <summary>
    /// Additional error details to include in the response
    /// </summary>
    public Dictionary<string, object>? ErrorDetails { get; }

    /// <summary>
    /// Initialize a new instance of ApiException
    /// </summary>
    /// <param name="errorCode">Error code identifier</param>
    /// <param name="message">Human-readable error message</param>
    /// <param name="statusCode">HTTP status code (default: 400)</param>
    /// <param name="errorDetails">Additional error details (optional)</param>
    public ApiException(
        string errorCode,
        string message,
        int statusCode = StatusCodes.Status400BadRequest,
        Dictionary<string, object>? errorDetails = null)
        : base(message)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
        ErrorDetails = errorDetails;
    }

    /// <summary>
    /// Initialize a new instance of ApiException with inner exception
    /// </summary>
    public ApiException(
        string errorCode,
        string message,
        Exception innerException,
        int statusCode = StatusCodes.Status400BadRequest,
        Dictionary<string, object>? errorDetails = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
        ErrorDetails = errorDetails;
    }
}

/// <summary>
/// Exception thrown when a requested resource is not found
/// </summary>
public class ResourceNotFoundException : ApiException
{
    public ResourceNotFoundException(
        string resourceName,
        object resourceId)
        : base(
            "RESOURCE_NOT_FOUND",
            $"{resourceName} with ID '{resourceId}' not found",
            StatusCodes.Status404NotFound)
    {
    }

    public ResourceNotFoundException(
        string errorCode,
        string message)
        : base(
            errorCode,
            message,
            StatusCodes.Status404NotFound)
    {
    }
}

/// <summary>
/// Exception thrown when validation fails
/// </summary>
public class ValidationException : ApiException
{
    public ValidationException(
        string message,
        Dictionary<string, object>? validationErrors = null)
        : base(
            "VALIDATION_FAILED",
            message,
            StatusCodes.Status400BadRequest,
            validationErrors)
    {
    }

    public ValidationException(
        string errorCode,
        string message,
        Dictionary<string, object>? validationErrors = null)
        : base(
            errorCode,
            message,
            StatusCodes.Status400BadRequest,
            validationErrors)
    {
    }
}

/// <summary>
/// Exception thrown when a resource already exists
/// </summary>
public class DuplicateResourceException : ApiException
{
    public DuplicateResourceException(
        string resourceName,
        string fieldName,
        object fieldValue)
        : base(
            "RESOURCE_ALREADY_EXISTS",
            $"{resourceName} with {fieldName} '{fieldValue}' already exists",
            StatusCodes.Status409Conflict)
    {
    }

    public DuplicateResourceException(
        string errorCode,
        string message)
        : base(
            errorCode,
            message,
            StatusCodes.Status409Conflict)
    {
    }
}

/// <summary>
/// Exception thrown when user is not authorized to perform an action
/// </summary>
public class UnauthorizedException : ApiException
{
    public UnauthorizedException(
        string message = "User is not authorized to perform this action")
        : base(
            "UNAUTHORIZED",
            message,
            StatusCodes.Status401Unauthorized)
    {
    }

    public UnauthorizedException(
        string errorCode,
        string message)
        : base(
            errorCode,
            message,
            StatusCodes.Status401Unauthorized)
    {
    }
}

/// <summary>
/// Exception thrown when user lacks permission for an action
/// </summary>
public class ForbiddenException : ApiException
{
    public ForbiddenException(
        string message = "User does not have permission to perform this action")
        : base(
            "FORBIDDEN",
            message,
            StatusCodes.Status403Forbidden)
    {
    }

    public ForbiddenException(
        string errorCode,
        string message)
        : base(
            errorCode,
            message,
            StatusCodes.Status403Forbidden)
    {
    }
}

/// <summary>
/// Exception thrown when a business rule is violated
/// </summary>
public class BusinessRuleException : ApiException
{
    public BusinessRuleException(
        string errorCode,
        string message,
        int statusCode = StatusCodes.Status400BadRequest,
        Dictionary<string, object>? details = null)
        : base(errorCode, message, statusCode, details)
    {
    }
}