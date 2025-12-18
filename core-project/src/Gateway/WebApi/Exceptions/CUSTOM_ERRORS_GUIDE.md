# Custom Errors Guide

This guide explains how to use custom exceptions and error responses in your API controllers.

## Overview

There are two approaches to handle custom errors in your API:

1. **Throw Exceptions** - For use in service/business logic (recommended)
2. **Return Error Responses** - For use directly in controllers

Both approaches are supported and automatically handled by `GlobalExceptionMiddleware`.

---

## Approach 1: Throw Exceptions (Recommended)

Exceptions are ideal for business logic as they provide early exit and flow control. The middleware catches them and formats the response.

### Basic Exception Classes

#### ValidationException
Thrown when input validation fails.

```csharp
// Simple usage
throw new ValidationException("Email is required");

// With custom error code
throw new ValidationException("INVALID_EMAIL_FORMAT", "Email format is invalid");

// With validation errors dictionary
var errors = new Dictionary<string, object>
{
    { "email", "Email format is invalid" },
    { "password", "Password must be at least 8 characters" }
};
throw new ValidationException("VALIDATION_FAILED", "Multiple validation errors", errors);
```

**Response (400 Bad Request):**
```json
{
  "error": "VALIDATION_FAILED",
  "message": "Multiple validation errors",
  "details": {
    "email": "Email format is invalid",
    "password": "Password must be at least 8 characters"
  }
}
```

#### ResourceNotFoundException
Thrown when a requested resource is not found.

```csharp
// With resource name and ID
throw new ResourceNotFoundException("User", userId);

// With custom error code and message
throw new ResourceNotFoundException("USER_NOT_FOUND", "User account has been deleted");
```

**Response (404 Not Found):**
```json
{
  "error": "RESOURCE_NOT_FOUND",
  "message": "User with ID '123' not found"
}
```

#### DuplicateResourceException
Thrown when attempting to create a duplicate resource.

```csharp
// With resource name, field name, and value
throw new DuplicateResourceException("User", "email", "user@example.com");

// With custom error code and message
throw new DuplicateResourceException("EMAIL_ALREADY_EXISTS", "This email is already registered");
```

**Response (409 Conflict):**
```json
{
  "error": "RESOURCE_ALREADY_EXISTS",
  "message": "User with email 'user@example.com' already exists"
}
```

#### UnauthorizedException
Thrown when user is not authenticated.

```csharp
// Default message
throw new UnauthorizedException();

// Custom message
throw new UnauthorizedException("Authentication token has expired");

// Custom error code and message
throw new UnauthorizedException("INVALID_CREDENTIALS", "Email or password is incorrect");
```

**Response (401 Unauthorized):**
```json
{
  "error": "UNAUTHORIZED",
  "message": "Authentication token has expired"
}
```

#### ForbiddenException
Thrown when user lacks permission for an action.

```csharp
// Default message
throw new ForbiddenException();

// Custom message
throw new ForbiddenException("You cannot delete this user");

// Custom error code and message
throw new ForbiddenException("INSUFFICIENT_PERMISSIONS", "Admin role is required");
```

**Response (403 Forbidden):**
```json
{
  "error": "FORBIDDEN",
  "message": "Admin role is required"
}
```

#### BusinessRuleException
Thrown when a business rule is violated.

```csharp
// Simple usage
throw new BusinessRuleException(
    errorCode: "ACCOUNT_LOCKED",
    message: "Account has been locked due to multiple failed login attempts",
    statusCode: StatusCodes.Status400BadRequest
);

// With error details
throw new BusinessRuleException(
    errorCode: "INSUFFICIENT_BALANCE",
    message: "Insufficient balance for this transaction",
    statusCode: StatusCodes.Status400BadRequest,
    details: new Dictionary<string, object>
    {
        { "required", 100.50m },
        { "available", 50.25m },
        { "shortfall", 50.25m }
    }
);
```

**Response (400 Bad Request):**
```json
{
  "error": "INSUFFICIENT_BALANCE",
  "message": "Insufficient balance for this transaction",
  "details": {
    "required": 100.50,
    "available": 50.25,
    "shortfall": 50.25
  }
}
```

#### ApiException
Generic exception for custom errors.

```csharp
throw new ApiException(
    errorCode: "CUSTOM_ERROR",
    message: "Something went wrong",
    statusCode: StatusCodes.Status400BadRequest,
    errorDetails: new Dictionary<string, object>
    {
        { "timestamp", DateTime.UtcNow },
        { "requestId", context.TraceIdentifier }
    }
);
```

---

## Approach 2: Helper Methods

The `ApiResponseHelper` class provides convenient methods for both throwing exceptions and returning error responses.

### Using ApiResponseHelper Static Methods

#### Throw Methods
```csharp
// Validation errors
ApiResponseHelper.ThrowValidation("Email is required");
ApiResponseHelper.ThrowValidation("INVALID_EMAIL", "Email format is invalid");
ApiResponseHelper.ThrowValidation("VALIDATION_FAILED", "Errors found", errorsDictionary);

// Not found errors
ApiResponseHelper.ThrowNotFound("User", userId);
ApiResponseHelper.ThrowNotFound("USER_DELETED", "User account no longer exists");

// Duplicate errors
ApiResponseHelper.ThrowDuplicate("User", "email", "user@example.com");
ApiResponseHelper.ThrowDuplicate("EMAIL_EXISTS", "Email already registered");

// Authorization errors
ApiResponseHelper.ThrowUnauthorized();
ApiResponseHelper.ThrowUnauthorized("Invalid credentials");
ApiResponseHelper.ThrowUnauthorized("TOKEN_EXPIRED", "Your session has expired");

ApiResponseHelper.ThrowForbidden();
ApiResponseHelper.ThrowForbidden("Admin role required");
ApiResponseHelper.ThrowForbidden("INSUFFICIENT_ROLE", "Admin role is required");

// Business rule errors
ApiResponseHelper.ThrowBusinessRule("ACCOUNT_LOCKED", "Account locked due to failed attempts");

// Custom errors
ApiResponseHelper.ThrowCustom("MY_ERROR_CODE", "Custom error message");
```

#### Extension Methods on Controllers

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        // Validation error response
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return this.ValidationError("Email is required");
        }

        // Not found error response
        var existingUser = await _userService.GetUserAsync(request.Id);
        if (existingUser == null)
        {
            return this.NotFoundError("User", request.Id);
        }

        // Duplicate error response
        var userWithEmail = await _userService.GetUserByEmailAsync(request.Email);
        if (userWithEmail != null)
        {
            return this.DuplicateError("User", "email", request.Email);
        }

        // Authorization errors
        if (!User.IsInRole("Admin"))
        {
            return this.ForbiddenError("Admin role is required");
        }

        // Custom error response
        return this.CustomError("CUSTOM_CODE", "Custom message", 
            statusCode: StatusCodes.Status418ImATeapot);

        // Success response
        var newUser = await _userService.CreateUserAsync(request);
        return this.Created(newUser, $"/api/users/{newUser.Id}");
    }
}
```

---

## Practical Examples

### Example 1: User Registration with Validation

```csharp
[HttpPost("register")]
[AllowAnonymous]
public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
{
    // Validation - throw exception
    if (string.IsNullOrWhiteSpace(command.Email))
    {
        throw new ValidationException("EMAIL_REQUIRED", "Email is required");
    }

    if (command.Password.Length < 8)
    {
        throw new ValidationException("PASSWORD_TOO_SHORT", "Password must be at least 8 characters");
    }

    // Check for duplicate - throw exception
    var existingUser = await _userService.GetUserByEmailAsync(command.Email);
    if (existingUser != null)
    {
        throw new DuplicateResourceException("User", "email", command.Email);
    }

    // Execute business logic
    var result = await mediator.Send(command);

    if (result.IsFailure)
    {
        throw new BusinessRuleException(
            result.Error!.Code,
            result.Error.Message
        );
    }

    // Return success
    return Created($"/api/users/{result.Value!.UserId}", result.Value);
}
```

**Possible Responses:**

```json
// 400 Bad Request - Email required
{
  "error": "EMAIL_REQUIRED",
  "message": "Email is required"
}

// 409 Conflict - Email already exists
{
  "error": "RESOURCE_ALREADY_EXISTS",
  "message": "User with email 'user@example.com' already exists"
}

// 400 Bad Request - Business rule
{
  "error": "INVALID_PASSWORD",
  "message": "Password does not meet security requirements"
}

// 201 Created - Success
{
  "userId": "123",
  "email": "user@example.com",
  "createdAt": "2024-01-15T10:30:00Z"
}
```

### Example 2: Update User with Authorization Check

```csharp
[HttpPut("{id}")]
[Authorize]
public async Task<IActionResult> Update(string id, [FromBody] UpdateUserRequest request)
{
    // Not found - throw exception
    var user = ApiResponseHelper.ThrowNotFoundIfNull(
        await _userService.GetUserAsync(id),
        "User",
        id
    );

    // Authorization check - throw exception
    if (user.Id != User.FindFirst(ClaimTypes.NameIdentifier)?.Value && 
        !User.IsInRole("Admin"))
    {
        throw new ForbiddenException("CANNOT_UPDATE_OTHER_USER", "You can only update your own profile");
    }

    // Validation with errors dictionary
    var validationErrors = new Dictionary<string, object>();
    
    if (!IsValidEmail(request.Email))
    {
        validationErrors["email"] = "Invalid email format";
    }
    
    if (request.FirstName?.Length > 100)
    {
        validationErrors["firstName"] = "First name cannot exceed 100 characters";
    }

    if (validationErrors.Count > 0)
    {
        throw new ValidationException("UPDATE_VALIDATION_FAILED", "Multiple validation errors", validationErrors);
    }

    // Update and return
    await _userService.UpdateUserAsync(id, request);
    return this.Success("User updated successfully");
}
```

### Example 3: Delete User with Business Rule

```csharp
[HttpDelete("{id}")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> Delete(string id)
{
    var user = ApiResponseHelper.ThrowNotFoundIfNull(
        await _userService.GetUserAsync(id),
        "User",
        id
    );

    // Business rule check
    if (user.Role == "SuperAdmin")
    {
        throw new BusinessRuleException(
            errorCode: "CANNOT_DELETE_SUPERADMIN",
            message: "SuperAdmin users cannot be deleted",
            statusCode: StatusCodes.Status400BadRequest
        );
    }

    // Check if user has active orders
    var activeOrders = await _orderService.GetActiveOrdersAsync(id);
    if (activeOrders.Count > 0)
    {
        throw new BusinessRuleException(
            errorCode: "USER_HAS_ACTIVE_ORDERS",
            message: "Cannot delete user with active orders",
            statusCode: StatusCodes.Status400BadRequest,
            details: new Dictionary<string, object>
            {
                { "activeOrderCount", activeOrders.Count },
                { "orderIds", activeOrders.Select(o => o.Id).ToList() }
            }
        );
    }

    await _userService.DeleteUserAsync(id);
    return this.NoContent();
}
```

---

## Error Handling in Service/Business Logic

Services and business logic should throw exceptions rather than return error responses:

```csharp
public class UserService
{
    public async Task<User> GetUserAsync(string userId)
    {
        var user = await _repository.FindAsync(userId);
        
        // Throw exception instead of returning null or error
        if (user == null)
        {
            throw new ResourceNotFoundException("User", userId);
        }

        return user;
    }

    public async Task<User> CreateUserAsync(CreateUserRequest request)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ValidationException("EMAIL_REQUIRED", "Email is required");
        }

        // Check for duplicates
        var existing = await _repository.FindByEmailAsync(request.Email);
        if (existing != null)
        {
            throw new DuplicateResourceException("User", "email", request.Email);
        }

        // Check business rules
        if (!IsValidEmailDomain(request.Email))
        {
            throw new BusinessRuleException(
                errorCode: "INVALID_EMAIL_DOMAIN",
                message: $"Email domain is not allowed",
                statusCode: StatusCodes.Status400BadRequest
            );
        }

        return await _repository.CreateAsync(request);
    }
}
```

---

## Key Points

1. **Always throw exceptions** in service/business logic layer
2. **Middleware catches and formats** exception responses automatically
3. **Use extension methods** in controllers for easy response creation
4. **Include error details** for debugging (only in Development)
5. **Consistent error format** across all endpoints
6. **Custom error codes** enable client-side error handling
7. **HTTP status codes** automatically set based on exception type

---

## Migration Guide

If you have existing error handling code:

### Before:
```csharp
public async Task<IActionResult> Register([FromBody] RegisterCommand command)
{
    if (string.IsNullOrWhiteSpace(command.Email))
    {
        return BadRequest(new { error = "EMAIL_REQUIRED", message = "Email is required" });
    }

    var result = await mediator.Send(command);
    if (result.IsFailure)
    {
        return BadRequest(new { error = result.Error!.Code, message = result.Error.Message });
    }

    return Created($"/api/users/{result.Value!.UserId}", result.Value);
}
```

### After:
```csharp
public async Task<IActionResult> Register([FromBody] RegisterCommand command)
{
    if (string.IsNullOrWhiteSpace(command.Email))
    {
        throw new ValidationException("EMAIL_REQUIRED", "Email is required");
    }

    var result = await mediator.Send(command);
    if (result.IsFailure)
    {
        throw new BusinessRuleException(result.Error!.Code, result.Error.Message);
    }

    return Created($"/api/users/{result.Value!.UserId}", result.Value);
}
```

Much cleaner! 🎉