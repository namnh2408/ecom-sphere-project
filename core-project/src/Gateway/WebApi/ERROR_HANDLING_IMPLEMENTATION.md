# Custom Error Handling Implementation

## 📋 Overview

Complete custom error handling system with middleware, exceptions, and helpers for consistent API responses.

**Status:** ✅ Fully Implemented  
**Date:** January 2024

---

## 📁 Files Created

### Middleware (`Middleware/`)
```
Middleware/
├── GlobalExceptionMiddleware.cs     [NEW] Catches all exceptions, formats responses
├── InputValidationMiddleware.cs     [NEW] Validates JSON input before routing
└── README.md                        [UPDATED] Middleware documentation
```

### Exceptions (`Exceptions/`)
```
Exceptions/
├── ApiException.cs                  [NEW] Base class + 6 specialized exceptions
│   ├── ApiException
│   ├── ValidationException
│   ├── ResourceNotFoundException
│   ├── DuplicateResourceException
│   ├── UnauthorizedException
│   ├── ForbiddenException
│   └── BusinessRuleException
└── CUSTOM_ERRORS_GUIDE.md          [NEW] Comprehensive guide with examples
```

### Helpers (`Helpers/`)
```
Helpers/
└── ApiResponseHelper.cs             [NEW] Static methods + extension methods
    ├── Success response builders
    ├── Exception throwing methods
    ├── Error response builders
    └── Conditional helpers
```

### Documentation
```
Documentation/
├── CUSTOM_ERRORS_SUMMARY.md        [NEW] Quick start guide
├── ERROR_HANDLING_IMPLEMENTATION.md [NEW] This file - implementation overview
└── CONTROLLER_EXAMPLE.md            [NEW] Practical controller examples
```

### Configuration
```
Program.cs                           [UPDATED] Middleware registration
```

---

## 🏗️ Architecture

### Request Flow

```
HTTP Request
    ↓
[HttpsRedirection]
    ↓
[GlobalExceptionMiddleware] ← Wraps everything
    ↓
[InputValidationMiddleware] ← Validates JSON
    ↓
[Authorization]
    ↓
[Controllers/Services]
    ├─→ Success: Return response
    └─→ Error: Throw exception ← Caught by GlobalExceptionMiddleware
    ↓
Middleware Formats Response
    ↓
Consistent ErrorResponse
    ↓
HTTP Response (to client)
```

### Response Format

All errors follow this structure:
```json
{
  "error": "ERROR_CODE",
  "message": "Human readable message",
  "details": { "additional": "information" }  // Optional
}
```

---

## 🎯 Key Components

### 1. GlobalExceptionMiddleware

**File:** `Middleware/GlobalExceptionMiddleware.cs`

**Purpose:** Catch all exceptions and return consistent error responses

**Features:**
- Catches all exception types
- Maps exceptions to HTTP status codes
- Handles custom `ApiException` with custom codes
- Includes stack traces in Development only
- Logs errors appropriately

**Supported Exceptions:**
- `ApiException` - Custom API errors
- `ValidationException` - Input validation
- `ResourceNotFoundException` - 404 errors
- `DuplicateResourceException` - 409 conflicts
- `UnauthorizedException` - 401 auth failures
- `ForbiddenException` - 403 permission failures
- Plus standard .NET exceptions

### 2. InputValidationMiddleware

**File:** `Middleware/InputValidationMiddleware.cs`

**Purpose:** Validate request bodies before routing

**Features:**
- Validates JSON format
- Checks for empty bodies
- Only validates POST/PUT/PATCH/DELETE
- Returns 400 with clear error message

### 3. Custom Exceptions

**File:** `Exceptions/ApiException.cs`

**Exception Classes:**

| Exception | Status | Use Case |
|-----------|--------|----------|
| `ApiException` | Custom | Base for all API errors |
| `ValidationException` | 400 | Input validation failures |
| `ResourceNotFoundException` | 404 | Missing resources |
| `DuplicateResourceException` | 409 | Duplicate resources |
| `UnauthorizedException` | 401 | Auth failures |
| `ForbiddenException` | 403 | Permission issues |
| `BusinessRuleException` | 400+ | Business logic violations |

### 4. Helper Methods

**File:** `Helpers/ApiResponseHelper.cs`

**Three Types of Methods:**

1. **Throw Methods** (for services/business logic)
   ```csharp
   ApiResponseHelper.ThrowValidation("message")
   ApiResponseHelper.ThrowNotFound("Resource", id)
   ApiResponseHelper.ThrowDuplicate("User", "email", "user@example.com")
   ApiResponseHelper.ThrowUnauthorized()
   ApiResponseHelper.ThrowForbidden()
   ApiResponseHelper.ThrowBusinessRule("CODE", "message")
   ```

2. **Extension Methods** (on ControllerBase)
   ```csharp
   this.ValidationError("message")
   this.NotFoundError("Resource", id)
   this.DuplicateError("User", "email", "user@example.com")
   this.UnauthorizedError()
   this.ForbiddenError()
   this.CustomError("CODE", "message")
   ```

3. **Conditional Methods** (for cleaner logic)
   ```csharp
   ApiResponseHelper.ThrowValidationIf(condition, "message")
   ApiResponseHelper.ThrowNotFoundIfNull(value, "Resource", id)
   ```

---

## 🚀 Quick Start

### 1. Throw Exception (Recommended)

**In your controller/service:**
```csharp
using Gateway.WebApi.Exceptions;

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

### 2. Return Error Response (Alternative)

**In your controller:**
```csharp
using Gateway.WebApi.Helpers;

if (string.IsNullOrWhiteSpace(email))
{
    return this.ValidationError("EMAIL_REQUIRED", "Email cannot be empty");
}
```

### 3. Handle Not Found

**In your controller:**
```csharp
var user = await _userService.GetUserAsync(id);
if (user == null)
{
    throw new ResourceNotFoundException("User", id);
}
```

**Response (404 Not Found):**
```json
{
  "error": "RESOURCE_NOT_FOUND",
  "message": "User with ID '123' not found"
}
```

### 4. Handle Duplicates

**In your controller:**
```csharp
if (await _userService.EmailExistsAsync(email))
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

### 5. Complex Validation Errors

**In your controller:**
```csharp
var errors = new Dictionary<string, object>
{
    { "email", "Invalid format" },
    { "password", "Too weak" },
    { "age", "Must be 18+" }
};

throw new ValidationException(
    "VALIDATION_FAILED",
    "Multiple validation errors",
    errors
);
```

**Response (400 Bad Request):**
```json
{
  "error": "VALIDATION_FAILED",
  "message": "Multiple validation errors",
  "details": {
    "email": "Invalid format",
    "password": "Too weak",
    "age": "Must be 18+"
  }
}
```

---

## 📖 Documentation

### For Detailed Guides:

1. **`CUSTOM_ERRORS_GUIDE.md`**
   - All exception types explained
   - Usage patterns
   - Real-world examples
   - Best practices

2. **`CONTROLLER_EXAMPLE.md`**
   - Controller implementation examples
   - Before/after comparisons
   - Response examples
   - Migration guide

3. **`CUSTOM_ERRORS_SUMMARY.md`**
   - Quick reference
   - Exception mapping table
   - File locations
   - Next steps

4. **`Middleware/README.md`**
   - Middleware details
   - Execution order
   - Testing guide

---

## 💡 Usage Patterns

### Pattern 1: Service Layer (Throws Exceptions)

```csharp
public class UserService
{
    public async Task<User> CreateUserAsync(CreateUserRequest request)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ValidationException("EMAIL_REQUIRED", "Email is required");

        // Check duplicate
        if (await _repo.ExistsByEmailAsync(request.Email))
            throw new DuplicateResourceException("User", "email", request.Email);

        return await _repo.CreateAsync(request);
    }
}
```

### Pattern 2: Controller Layer (Catches/Returns)

```csharp
[HttpPost("create")]
public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
{
    // Middleware catches any exceptions thrown by service
    var user = await _userService.CreateUserAsync(request);
    return Created($"/api/users/{user.Id}", user);
}
```

### Pattern 3: Helper Methods

```csharp
[HttpPost("create")]
public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
{
    // Using helper extension methods
    if (string.IsNullOrWhiteSpace(request.Email))
        return this.ValidationError("EMAIL_REQUIRED", "Email is required");

    var user = await _userService.CreateUserAsync(request);
    return this.Created(user, $"/api/users/{user.Id}");
}
```

---

## ✅ Features

✨ **Automatic Response Formatting**
- All errors return consistent structure
- No manual formatting needed

✨ **Clean Code**
- Throw exceptions instead of building responses
- Middleware handles formatting

✨ **Flexible Error Details**
- Simple: error code + message
- Complex: include details dictionary

✨ **Development vs Production**
- Development: Full stack traces
- Production: Generic error messages

✨ **Comprehensive Exception Types**
- Ready for common scenarios
- Extensible for custom cases

✨ **Helper Methods**
- Extension methods on controllers
- Static methods for services
- Reduces boilerplate

---

## 🔧 Configuration

### Program.cs Registration

The middleware is registered in `Program.cs` in the correct order:

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

**Execution Order:**
1. HttpsRedirection
2. GlobalExceptionMiddleware ← Wraps everything
3. InputValidationMiddleware ← Validates JSON
4. Authorization
5. Controllers

---

## 🧪 Testing

### Test Malformed JSON
```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{invalid}'
```

### Test Empty Body
```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d ''
```

### Test Custom Exception
```csharp
// In any controller action
throw new ValidationException("TEST_ERROR", "This is a test");

// Returns: 400 Bad Request
// {
//   "error": "TEST_ERROR",
//   "message": "This is a test"
// }
```

---

## 📊 Exception to Status Code Mapping

| Exception | HTTP Status | Error Code |
|-----------|-------------|-----------|
| `ValidationException` | 400 | VALIDATION_FAILED |
| `ResourceNotFoundException` | 404 | RESOURCE_NOT_FOUND |
| `DuplicateResourceException` | 409 | RESOURCE_ALREADY_EXISTS |
| `UnauthorizedException` | 401 | UNAUTHORIZED |
| `ForbiddenException` | 403 | FORBIDDEN |
| `BusinessRuleException` | 400+ | Custom |
| `ApiException` | Custom | Custom |
| `ArgumentException` | 400 | INVALID_ARGUMENT |
| `InvalidOperationException` | 400 | INVALID_OPERATION |
| `TimeoutException` | 504 | REQUEST_TIMEOUT |
| Other exceptions | 500 | INTERNAL_SERVER_ERROR |

---

## 🎓 Learning Path

1. **Start Here:** `CUSTOM_ERRORS_SUMMARY.md`
   - Overview and quick examples

2. **Deep Dive:** `CUSTOM_ERRORS_GUIDE.md`
   - All exception types
   - Usage patterns
   - Real-world examples

3. **See Examples:** `CONTROLLER_EXAMPLE.md`
   - Before/after comparisons
   - Full controller implementations
   - Response examples

4. **Understand Middleware:** `Middleware/README.md`
   - How it works
   - Execution flow
   - Testing guide

---

## 🚦 Migration Checklist

- [ ] Review `CUSTOM_ERRORS_SUMMARY.md`
- [ ] Read `CUSTOM_ERRORS_GUIDE.md` for your use case
- [ ] Look at `CONTROLLER_EXAMPLE.md` for patterns
- [ ] Update your first controller
- [ ] Test endpoints with different error scenarios
- [ ] Update remaining controllers
- [ ] Test full integration
- [ ] Review error logs

---

## 📚 File Reference

| File | Location | Purpose |
|------|----------|---------|
| ApiException.cs | `Exceptions/` | Base and specialized exceptions |
| ApiResponseHelper.cs | `Helpers/` | Helper methods and extensions |
| GlobalExceptionMiddleware.cs | `Middleware/` | Exception handling middleware |
| InputValidationMiddleware.cs | `Middleware/` | Input validation middleware |
| CUSTOM_ERRORS_GUIDE.md | `Exceptions/` | Comprehensive guide |
| CONTROLLER_EXAMPLE.md | `WebApi/` | Controller examples |
| CUSTOM_ERRORS_SUMMARY.md | `WebApi/` | Quick start |
| ERROR_HANDLING_IMPLEMENTATION.md | `WebApi/` | This file |
| Middleware/README.md | `Middleware/` | Middleware docs |

---

## 🆘 Common Questions

**Q: Where should I throw exceptions - service or controller?**  
A: Throw from service layer. Controllers return responses.

**Q: Do I need try-catch blocks?**  
A: No! Middleware catches all exceptions automatically.

**Q: Can I include custom details in errors?**  
A: Yes! Pass `Dictionary<string, object>` to exceptions.

**Q: How are exceptions logged?**  
A: GlobalExceptionMiddleware logs all exceptions using ILogger.

**Q: Will users see stack traces?**  
A: Only in Development environment for security.

**Q: Can I create custom exception types?**  
A: Yes! Extend ApiException with your own error codes.

---

## ✨ Next Steps

1. Update controllers to use new exceptions
2. Migrate from manual error handling
3. Test all endpoints thoroughly
4. Monitor error logs
5. Document custom error codes for API consumers

---

## 📞 Support Resources

- See `CUSTOM_ERRORS_GUIDE.md` for detailed examples
- Check `CONTROLLER_EXAMPLE.md` for real patterns
- Review `Middleware/README.md` for middleware details
- Look at `CUSTOM_ERRORS_SUMMARY.md` for quick reference

---

**Implementation Complete! 🎉**

Your API now has a robust, consistent error handling system.