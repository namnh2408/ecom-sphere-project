# Custom Error Handling - Implementation Summary

## What Was Added

### 📁 Folder Structure
```
src/Gateway/WebApi/
├── Middleware/
│   ├── GlobalExceptionMiddleware.cs
│   ├── InputValidationMiddleware.cs
│   └── README.md
├── Exceptions/
│   ├── ApiException.cs
│   ├── CUSTOM_ERRORS_GUIDE.md
│   └── (7 specialized exception classes)
├── Helpers/
│   └── ApiResponseHelper.cs
├── CUSTOM_ERRORS_SUMMARY.md (this file)
└── Program.cs (updated)
```

### 🔧 Components

#### 1. **Middleware** (Catch & Format Errors)
- **GlobalExceptionMiddleware** - Catches all exceptions and formats responses
- **InputValidationMiddleware** - Validates JSON input before routing

#### 2. **Custom Exceptions** (Throw Errors)
- `ApiException` - Base exception with error code, message, status code
- `ValidationException` - For validation failures
- `ResourceNotFoundException` - For missing resources
- `DuplicateResourceException` - For duplicate resources
- `UnauthorizedException` - For auth failures
- `ForbiddenException` - For permission issues
- `BusinessRuleException` - For business logic violations

#### 3. **Helpers** (Make It Easy)
- `ApiResponseHelper` - Static methods for throwing exceptions
- Extension methods on `ControllerBase` - For returning error responses
- Conditional helpers - For cleaner if/throw logic

---

## How It Works

### Flow Diagram

```
Request
  ↓
[InputValidationMiddleware] ← Validates JSON format
  ↓
Application Logic (Controller/Service)
  ↓
  ├─→ Success: Return response ✅
  ├─→ Exception thrown ❌
       ↓
    [GlobalExceptionMiddleware] ← Catches & formats
       ↓
    Return consistent error response
```

### Error Response Format

All errors follow this format:
```json
{
  "error": "ERROR_CODE",
  "message": "Human readable message",
  "details": { "field1": "error detail" }  // Optional
}
```

---

## Quick Start Examples

### Example 1: Throw Validation Error

**In Your Service/Controller:**
```csharp
if (string.IsNullOrWhiteSpace(email))
{
    throw new ValidationException("EMAIL_REQUIRED", "Email cannot be empty");
}
```

**Response (400 Bad Request):**
```json
{
  "error": "EMAIL_REQUIRED",
  "message": "Email cannot be empty"
}
```

### Example 2: Throw Not Found Error

**In Your Service/Controller:**
```csharp
var user = await _userService.GetUserAsync(userId);
if (user == null)
{
    throw new ResourceNotFoundException("User", userId);
}
```

**Response (404 Not Found):**
```json
{
  "error": "RESOURCE_NOT_FOUND",
  "message": "User with ID '123' not found"
}
```

### Example 3: Throw Duplicate Error

**In Your Service/Controller:**
```csharp
var existing = await _userService.GetUserByEmailAsync(email);
if (existing != null)
{
    throw new DuplicateResourceException("User", "email", email);
}
```

**Response (409 Conflict):**
```json
{
  "error": "RESOURCE_ALREADY_EXISTS",
  "message": "User with email 'test@example.com' already exists"
}
```

### Example 4: Complex Validation Error

**In Your Service/Controller:**
```csharp
var errors = new Dictionary<string, object>
{
    { "email", "Invalid email format" },
    { "password", "Password too weak" },
    { "age", "Must be at least 18 years old" }
};

throw new ValidationException(
    "MULTIPLE_VALIDATION_ERRORS",
    "Please fix the validation errors",
    errors
);
```

**Response (400 Bad Request):**
```json
{
  "error": "MULTIPLE_VALIDATION_ERRORS",
  "message": "Please fix the validation errors",
  "details": {
    "email": "Invalid email format",
    "password": "Password too weak",
    "age": "Must be at least 18 years old"
  }
}
```

### Example 5: Custom Business Rule Error

**In Your Service/Controller:**
```csharp
if (user.IsLocked)
{
    throw new BusinessRuleException(
        errorCode: "ACCOUNT_LOCKED",
        message: "Your account has been locked",
        statusCode: StatusCodes.Status403Forbidden,
        details: new Dictionary<string, object>
        {
            { "lockedUntil", DateTime.UtcNow.AddHours(24) },
            { "reason", "Too many failed login attempts" }
        }
    );
}
```

**Response (403 Forbidden):**
```json
{
  "error": "ACCOUNT_LOCKED",
  "message": "Your account has been locked",
  "details": {
    "lockedUntil": "2024-01-16T10:30:00Z",
    "reason": "Too many failed login attempts"
  }
}
```

---

## Usage Patterns

### Pattern 1: Throw from Service Layer

```csharp
public class UserService
{
    public async Task<User> CreateUserAsync(CreateUserRequest request)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ValidationException("EMAIL_REQUIRED", "Email is required");

        // Check duplicates
        if (await _repo.ExistsByEmailAsync(request.Email))
            throw new DuplicateResourceException("User", "email", request.Email);

        // Create and return
        return await _repo.CreateAsync(request);
    }
}
```

### Pattern 2: Throw from Controller

```csharp
[HttpPost("create")]
public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
{
    var user = await _userService.CreateUserAsync(request);
    return this.Created(user, $"/api/users/{user.Id}");
}
```

**What happens:**
- If `CreateUserAsync` throws, middleware catches it
- Response is formatted automatically
- No try-catch needed in controller!

### Pattern 3: Return Error Response (Alternative)

```csharp
[HttpPost("create")]
public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
{
    if (string.IsNullOrWhiteSpace(request.Email))
    {
        return this.ValidationError("EMAIL_REQUIRED", "Email is required");
    }

    var user = await _userService.CreateUserAsync(request);
    return this.Created(user, $"/api/users/{user.Id}");
}
```

### Pattern 4: Use Helper Methods

```csharp
using static Gateway.WebApi.Helpers.ApiResponseExtensions;

[HttpPost("create")]
public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
{
    // Now you can use methods directly without the class name
    ThrowValidation("EMAIL_REQUIRED", "Email is required");
    ThrowNotFound("User", userId);
    ThrowDuplicate("User", "email", email);
    // ... etc
}
```

---

## Migration Guide: Updating Existing Controllers

### Before (Old Pattern)
```csharp
[HttpPost("register")]
public async Task<IActionResult> Register([FromBody] RegisterCommand command)
{
    var result = await mediator.Send(command);

    if (result.IsFailure)
    {
        return BadRequest(new { error = result.Error!.Code, message = result.Error.Message });
    }

    return Created($"/api/users/{result.Value!.UserId}", result.Value);
}
```

### After (New Pattern - Clean!)
```csharp
[HttpPost("register")]
public async Task<IActionResult> Register([FromBody] RegisterCommand command)
{
    var result = await mediator.Send(command);

    if (result.IsFailure)
    {
        throw new BusinessRuleException(result.Error!.Code, result.Error.Message);
    }

    return Created($"/api/users/{result.Value!.UserId}", result.Value);
}
```

**Benefits:**
- Cleaner code
- Automatic response formatting
- Consistent error codes
- Better separation of concerns

---

## Exception to HTTP Status Code Mapping

| Exception | HTTP Status | Error Code |
|-----------|-------------|-----------|
| `ValidationException` | 400 | VALIDATION_FAILED |
| `ArgumentException` | 400 | INVALID_ARGUMENT |
| `InvalidOperationException` | 400 | INVALID_OPERATION |
| `ResourceNotFoundException` | 404 | RESOURCE_NOT_FOUND |
| `DuplicateResourceException` | 409 | RESOURCE_ALREADY_EXISTS |
| `UnauthorizedException` | 401 | UNAUTHORIZED |
| `ForbiddenException` | 403 | FORBIDDEN |
| `TimeoutException` | 504 | REQUEST_TIMEOUT |
| `OperationCanceledException` | 408 | REQUEST_CANCELED |
| `IOException` | 500 | IO_ERROR |
| Any other Exception | 500 | INTERNAL_SERVER_ERROR |

---

## Key Features

✅ **Automatic Response Formatting**
- All errors return consistent structure
- No manual formatting needed

✅ **Clean Code**
- Throw exceptions instead of building responses
- Middleware handles everything

✅ **Flexible Error Details**
- Simple errors: just code & message
- Complex errors: include details dictionary

✅ **Development vs Production**
- Production: Hide sensitive details
- Development: Include stack traces for debugging

✅ **Comprehensive Exception Types**
- Ready for common scenarios
- Easy to extend for custom cases

✅ **Helper Methods**
- Extension methods on controllers
- Static methods for services
- Reduces boilerplate code

---

## File Locations

📄 **Exception Classes:**
`src/Gateway/WebApi/Exceptions/ApiException.cs`

📄 **Helper Methods:**
`src/Gateway/WebApi/Helpers/ApiResponseHelper.cs`

📄 **Middleware:**
- `src/Gateway/WebApi/Middleware/GlobalExceptionMiddleware.cs`
- `src/Gateway/WebApi/Middleware/InputValidationMiddleware.cs`

📄 **Documentation:**
- `src/Gateway/WebApi/Exceptions/CUSTOM_ERRORS_GUIDE.md` - Detailed guide with examples
- `src/Gateway/WebApi/Middleware/README.md` - Middleware documentation

---

## Testing

### Test with cURL/Postman

**Test 1: Malformed JSON**
```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{invalid json}'
```

Response: 400 Bad Request - VALIDATION_ERROR

**Test 2: Empty Body**
```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d ''
```

Response: 400 Bad Request - VALIDATION_ERROR

**Test 3: Unhandled Exception**
- Trigger any exception in your code
- Middleware catches and formats it
- Response: 500 INTERNAL_SERVER_ERROR (with details in Development)

---

## Next Steps

1. **Update Controllers** - Replace old error handling with new exceptions
2. **Use in Services** - Throw exceptions from business logic
3. **Test** - Verify error responses with your API
4. **Documentation** - Add error codes to API docs
5. **Client Integration** - Implement client-side error handling based on error codes

---

## Support

For detailed examples and use cases, see:
- `Exceptions/CUSTOM_ERRORS_GUIDE.md` - Comprehensive guide
- `Middleware/README.md` - Middleware details

Need help? Check the practical examples in the guide! 🚀