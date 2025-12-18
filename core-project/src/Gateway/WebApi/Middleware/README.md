# API Middleware Documentation

## Overview
This folder contains middleware components that handle cross-cutting concerns for the API, including exception handling and input validation.

## Middleware Components

### 1. GlobalExceptionMiddleware
**Purpose:** Catches all unhandled exceptions and returns consistent error responses.

**Features:**
- Catches all exceptions thrown in the request pipeline
- Maps exception types to appropriate HTTP status codes
- Returns structured error responses using `ErrorResponse` model
- Logs detailed error information for debugging
- Includes exception details in Development environment only (for security)

**Supported Exception Types:**
- `ArgumentNullException` → 400 Bad Request
- `ArgumentException` → 400 Bad Request
- `InvalidOperationException` → 400 Bad Request
- `TimeoutException` → 504 Gateway Timeout
- `IOException` → 500 Internal Server Error
- `OperationCanceledException` → 408 Request Timeout
- All other exceptions → 500 Internal Server Error

**Example Response:**
```json
{
  "error": "INVALID_ARGUMENT",
  "message": "Value cannot be null. (Parameter 'email')",
  "details": null
}
```

### 2. InputValidationMiddleware
**Purpose:** Validates JSON request bodies before they reach controllers.

**Features:**
- Validates that request body is not empty
- Validates JSON format (catches malformed JSON)
- Only validates requests with Content-Type: application/json
- Only validates POST, PUT, PATCH, DELETE methods
- Returns 400 Bad Request with structured error response on validation failure
- Buffers request body to allow multiple reads

**Validated Scenarios:**
- Empty request body
- Malformed JSON syntax
- Invalid JSON structure

**Example Response (Malformed JSON):**
```json
{
  "error": "VALIDATION_ERROR",
  "message": "Invalid JSON format: The JSON value could not be converted to System.Int32. Line: 1 | BytePositionInLine: 10."
}
```

## Order of Execution

Middleware is executed in the order it's registered in `Program.cs`:

1. **HttpsRedirection** - Redirects HTTP to HTTPS
2. **GlobalExceptionMiddleware** - Wraps all subsequent middleware with exception handling
3. **InputValidationMiddleware** - Validates input before reaching authorization
4. **Authorization** - Checks user permissions
5. **Controllers** - Route handlers

## Integration in Program.cs

```csharp
app.UseHttpsRedirection();

// Add global exception handling middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

// Add input validation middleware
app.UseMiddleware<InputValidationMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
```

## Error Response Format

All errors follow the standardized `ErrorResponse` model:

```csharp
public class ErrorResponse
{
    public string? Error { get; set; }           // Error code identifier
    public string? Message { get; set; }         // Human-readable message
    public Dictionary<string, object>? Details { get; set; }  // Additional details
}
```

## Common Error Codes

| Error Code | HTTP Status | Description |
|---|---|---|
| `VALIDATION_ERROR` | 400 | Input validation failed |
| `INVALID_ARGUMENT` | 400 | Invalid argument provided |
| `ARGUMENT_NULL` | 400 | Required parameter is missing |
| `INVALID_OPERATION` | 400 | Invalid operation |
| `UNAUTHORIZED` | 401 | User not authenticated |
| `FORBIDDEN` | 403 | User lacks permission |
| `NOT_FOUND` | 404 | Resource not found |
| `CONFLICT` | 409 | Resource conflict |
| `REQUEST_TIMEOUT` | 408/504 | Request timed out |
| `INTERNAL_SERVER_ERROR` | 500 | Unexpected server error |
| `SERVICE_UNAVAILABLE` | 503 | Service temporarily unavailable |

## Usage Guidelines

### For Controller Developers
- You can now rely on GlobalExceptionMiddleware to catch unhandled exceptions
- No need to wrap controller methods with try-catch for simple error handling
- Return appropriate HTTP status codes using `BadRequest()`, `Unauthorized()`, etc.
- The middleware will automatically format responses consistently

### For API Consumers
- All error responses follow the same structure
- Check the `Error` field for error code to implement specific error handling
- The `Message` field contains human-readable description
- In development environment, `Details` field may contain debugging information

## Logging

Both middleware components use ASP.NET Core's `ILogger` interface:
- GlobalExceptionMiddleware logs all caught exceptions
- InputValidationMiddleware logs validation errors and middleware exceptions
- Configure logging level in `appsettings.json` to control verbosity

## Security Considerations

1. **Development vs Production**: Exception details are only included in Development environment
2. **Information Disclosure**: Production errors don't leak sensitive stack trace information
3. **JSON Validation**: Validates and safely parses JSON to prevent injection attacks
4. **Request Buffering**: InputValidationMiddleware enables request body buffering for safe multiple reads

## Custom Exceptions & Error Handling

The middleware works seamlessly with custom exceptions in the `Exceptions` folder:

### Available Custom Exceptions

- `ApiException` - Base class for all custom API exceptions
- `ValidationException` - For validation errors (400)
- `ResourceNotFoundException` - For missing resources (404)
- `DuplicateResourceException` - For duplicate resources (409)
- `UnauthorizedException` - For authentication failures (401)
- `ForbiddenException` - For authorization failures (403)
- `BusinessRuleException` - For business logic violations (400+)

### Usage in Controllers

```csharp
// Throw from service/business logic
if (user == null)
    throw new ResourceNotFoundException("User", userId);

if (string.IsNullOrWhiteSpace(email))
    throw new ValidationException("EMAIL_REQUIRED", "Email is required");

// Or use helper methods
throw new ApiException("CUSTOM_CODE", "Custom message", 
    StatusCodes.Status418ImATeapot, 
    new Dictionary<string, object> { { "details", "..." } });

// Return error response from controller
return this.NotFoundError("User", userId);
return this.ValidationError("Invalid input");
```

**See `CUSTOM_ERRORS_GUIDE.md` in the Exceptions folder for detailed examples.**

## Testing

To test the middleware:

1. **Test GlobalExceptionMiddleware:**
   ```
   POST /api/auth/register
   Content-Type: application/json
   
   {"invalid": data}  // Malformed JSON
   ```

2. **Test InputValidationMiddleware:**
   ```
   POST /api/auth/register
   Content-Type: application/json
   
   (empty body)
   ```

3. **Test Custom Exception:**
   ```csharp
   // In a controller action
   throw new ValidationException("VALIDATION_FAILED", "Email is required");
   
   // Should return:
   // 400 Bad Request
   // {
   //   "error": "VALIDATION_FAILED",
   //   "message": "Email is required"
   // }
   ```

4. **Test Both Together:**
   - Create a request that triggers an exception in your application logic
   - Verify the response follows ErrorResponse format