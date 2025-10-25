namespace ShopHub.Common.Results;

/// <summary>
/// Generic result wrapper cho API responses
/// </summary>
/// <typeparam name="T">Kiểu dữ liệu được trả về</typeparam>
public class Result<T>
{
    /// <summary>
    /// Liệu operation có thành công không
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Dữ liệu kết quả (null nếu thất bại)
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Message mô tả kết quả
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Danh sách lỗi (nếu có)
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// HTTP Status Code
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Tạo result thành công
    /// </summary>
    public static Result<T> Success(T data, string? message = null, int statusCode = 200)
    {
        return new Result<T>
        {
            IsSuccess = true,
            Data = data,
            Message = message ?? "Operation successful",
            StatusCode = statusCode
        };
    }

    /// <summary>
    /// Tạo result thất bại
    /// </summary>
    public static Result<T> Failure(string error, int statusCode = 400)
    {
        return new Result<T>
        {
            IsSuccess = false,
            Message = error,
            Errors = new List<string> { error },
            StatusCode = statusCode
        };
    }

    /// <summary>
    /// Tạo result thất bại với nhiều lỗi
    /// </summary>
    public static Result<T> Failure(List<string> errors, int statusCode = 400)
    {
        return new Result<T>
        {
            IsSuccess = false,
            Message = "Operation failed",
            Errors = errors,
            StatusCode = statusCode
        };
    }

    /// <summary>
    /// Tạo result không tìm thấy
    /// </summary>
    public static Result<T> NotFound(string message = "Resource not found")
    {
        return new Result<T>
        {
            IsSuccess = false,
            Message = message,
            Errors = new List<string> { message },
            StatusCode = 404
        };
    }

    /// <summary>
    /// Tạo result không được phép
    /// </summary>
    public static Result<T> Unauthorized(string message = "Unauthorized")
    {
        return new Result<T>
        {
            IsSuccess = false,
            Message = message,
            Errors = new List<string> { message },
            StatusCode = 401
        };
    }

    /// <summary>
    /// Tạo result forbidden
    /// </summary>
    public static Result<T> Forbidden(string message = "Forbidden")
    {
        return new Result<T>
        {
            IsSuccess = false,
            Message = message,
            Errors = new List<string> { message },
            StatusCode = 403
        };
    }
}

/// <summary>
/// Result wrapper cho operations không trả về dữ liệu
/// </summary>
public class Result
{
    /// <summary>
    /// Liệu operation có thành công không
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Message mô tả kết quả
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Danh sách lỗi
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// HTTP Status Code
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Tạo result thành công
    /// </summary>
    public static Result Success(string? message = null, int statusCode = 200)
    {
        return new Result
        {
            IsSuccess = true,
            Message = message ?? "Operation successful",
            StatusCode = statusCode
        };
    }

    /// <summary>
    /// Tạo result thất bại
    /// </summary>
    public static Result Failure(string error, int statusCode = 400)
    {
        return new Result
        {
            IsSuccess = false,
            Message = error,
            Errors = new List<string> { error },
            StatusCode = statusCode
        };
    }

    /// <summary>
    /// Tạo result thất bại với nhiều lỗi
    /// </summary>
    public static Result Failure(List<string> errors, int statusCode = 400)
    {
        return new Result
        {
            IsSuccess = false,
            Message = "Operation failed",
            Errors = errors,
            StatusCode = statusCode
        };
    }
}