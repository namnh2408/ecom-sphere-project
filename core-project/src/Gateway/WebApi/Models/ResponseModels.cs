namespace Gateway.WebApi.Models;

/// <summary>
/// Standard error response model
/// </summary>
/// <remarks>
/// Used for all error responses across the API
/// </remarks>
public class ErrorResponse
{
    /// <summary>
    /// Error code identifier
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Human-readable error message
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Additional error details (optional)
    /// </summary>
    public Dictionary<string, object>? Details { get; set; }
}

/// <summary>
/// Standard success response model
/// </summary>
/// <remarks>
/// Used for simple success responses
/// </remarks>
public class SuccessResponse
{
    /// <summary>
    /// Success message
    /// </summary>
    public string? Message { get; set; }
}

/// <summary>
/// Profile picture upload response
/// </summary>
public class PictureUploadResponse
{
    /// <summary>
    /// Success message
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Path or URL to the uploaded picture
    /// </summary>
    public string? PicturePath { get; set; }
}

