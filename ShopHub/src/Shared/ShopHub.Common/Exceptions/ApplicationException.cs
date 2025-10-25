namespace ShopHub.Common.Exceptions;

/// <summary>
/// Base exception cho tất cả application exceptions
/// </summary>
public class ApplicationException : Exception
{
    /// <summary>
    /// Error code
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// HTTP status code
    /// </summary>
    public int StatusCode { get; }

    /// <summary>
    /// Danh sách lỗi chi tiết
    /// </summary>
    public List<string> Errors { get; }

    /// <summary>
    /// Constructor
    /// </summary>
    public ApplicationException(
        string message,
        string errorCode = "INTERNAL_ERROR",
        int statusCode = 500,
        List<string>? errors = null)
        : base(message)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
        Errors = errors ?? new List<string>();
    }
}

/// <summary>
/// Exception cho validation errors
/// </summary>
public class ValidationException : ApplicationException
{
    /// <summary>
    /// Constructor
    /// </summary>
    public ValidationException(
        string message,
        List<string>? errors = null)
        : base(
            message,
            "VALIDATION_ERROR",
            400,
            errors)
    {
    }
}

/// <summary>
/// Exception khi không tìm thấy resource
/// </summary>
public class NotFoundException : ApplicationException
{
    /// <summary>
    /// Constructor
    /// </summary>
    public NotFoundException(
        string resourceName,
        object key)
        : base(
            $"{resourceName} with key '{key}' was not found.",
            "NOT_FOUND",
            404)
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public NotFoundException(string message)
        : base(
            message,
            "NOT_FOUND",
            404)
    {
    }
}

/// <summary>
/// Exception cho business rule violations
/// </summary>
public class BusinessRuleException : ApplicationException
{
    /// <summary>
    /// Constructor
    /// </summary>
    public BusinessRuleException(
        string message,
        string errorCode = "BUSINESS_RULE_VIOLATION")
        : base(
            message,
            errorCode,
            400)
    {
    }
}

/// <summary>
/// Exception cho unauthorized access
/// </summary>
public class UnauthorizedException : ApplicationException
{
    /// <summary>
    /// Constructor
    /// </summary>
    public UnauthorizedException(string message = "Unauthorized")
        : base(
            message,
            "UNAUTHORIZED",
            401)
    {
    }
}

/// <summary>
/// Exception cho forbidden access
/// </summary>
public class ForbiddenException : ApplicationException
{
    /// <summary>
    /// Constructor
    /// </summary>
    public ForbiddenException(string message = "Access forbidden")
        : base(
            message,
            "FORBIDDEN",
            403)
    {
    }
}

/// <summary>
/// Exception cho conflict (duplicate resources)
/// </summary>
public class ConflictException : ApplicationException
{
    /// <summary>
    /// Constructor
    /// </summary>
    public ConflictException(
        string message,
        string errorCode = "CONFLICT")
        : base(
            message,
            errorCode,
            409)
    {
    }
}