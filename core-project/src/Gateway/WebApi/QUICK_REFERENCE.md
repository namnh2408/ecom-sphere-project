# Custom Error Handling - Quick Reference

## 🎯 Start Here

1. **Already implemented?** Yes! ✅  
2. **Need to update controllers?** See [Migration Guide](#migration-guide)  
3. **Want to understand how it works?** See [How It Works](#how-it-works)

---

## 📚 Documentation Files

| File | Purpose | When to Read |
|------|---------|------|
| **QUICK_REFERENCE.md** | This file - quick answers | You're reading it! |
| **CUSTOM_ERRORS_SUMMARY.md** | Overview and quick start | Get started in 5 minutes |
| **CUSTOM_ERRORS_GUIDE.md** | Comprehensive guide with all examples | Deep dive into features |
| **CONTROLLER_EXAMPLE.md** | Real controller implementations | See before/after examples |
| **ERROR_HANDLING_IMPLEMENTATION.md** | Technical overview | Understand architecture |
| **Middleware/README.md** | Middleware documentation | How middleware works |

---

## 🚀 Quick Examples

### Validation Error
```csharp
throw new ValidationException("EMAIL_REQUIRED", "Email is required");
// Returns: 400 Bad Request
```

### Not Found
```csharp
throw new ResourceNotFoundException("User", userId);
// Returns: 404 Not Found
```

### Duplicate
```csharp
throw new DuplicateResourceException("User", "email", email);
// Returns: 409 Conflict
```

### Authorization
```csharp
throw new UnauthorizedException("Invalid token");
// Returns: 401 Unauthorized
```

### Forbidden
```csharp
throw new ForbiddenException("Admin role required");
// Returns: 403 Forbidden
```

### Business Rule
```csharp
throw new BusinessRuleException("ACCOUNT_LOCKED", "Account is locked");
// Returns: 400 Bad Request
```

### Custom Error
```csharp
throw new ApiException("MY_CODE", "Custom message", StatusCodes.Status418ImATeapot);
// Returns: 418 I'm a teapot
```

---

## 💡 How It Works

### The Flow
```
Request → Validation → Logic → Error?
                                  ↓
                            Throw Exception
                                  ↓
                         Middleware Catches
                                  ↓
                         Format Response
                                  ↓
                         Return to Client
```

### What Middleware Does

1. **GlobalExceptionMiddleware** - Catches exceptions, formats response
2. **InputValidationMiddleware** - Validates JSON format
3. Both return consistent error structure

---

## 📝 Response Format

Every error response has this structure:

```json
{
  "error": "ERROR_CODE",
  "message": "Human readable message",
  "details": {
    "field1": "error detail",
    "field2": "error detail"
  }
}
```

---

## 🎓 Learning Paths

### 1-Minute Learning
Just need to know the basics?
1. See [Quick Examples](#quick-examples) above
2. Done! Copy and paste examples

### 5-Minute Learning
Want a quick overview?
1. Read `CUSTOM_ERRORS_SUMMARY.md`
2. Copy an example
3. Start using it

### 30-Minute Learning
Want to understand fully?
1. Read `CUSTOM_ERRORS_SUMMARY.md`
2. Read `CONTROLLER_EXAMPLE.md`
3. Read `CUSTOM_ERRORS_GUIDE.md`
4. Review `ERROR_HANDLING_IMPLEMENTATION.md`

### Deep Dive
Want to understand the architecture?
1. Read `ERROR_HANDLING_IMPLEMENTATION.md` first
2. Then read `Middleware/README.md`
3. Review code in `Middleware/` and `Exceptions/`
4. Check `Helpers/ApiResponseHelper.cs`

---

## ❓ Common Questions

### How do I throw an error?
```csharp
throw new ValidationException("CODE", "message");
```

### How do I return an error response?
```csharp
return this.ValidationError("CODE", "message");
```

### Do I need try-catch?
No! Middleware catches everything automatically.

### Where do I catch errors?
Nowhere! Middleware does it for you.

### How do I log errors?
Automatically - GlobalExceptionMiddleware logs them.

### Can I include error details?
Yes! Pass a Dictionary to the exception.

### Will users see stack traces?
Only in Development environment.

### Which exception should I use?
- ValidationException - Input validation failures
- ResourceNotFoundException - 404 errors
- DuplicateResourceException - 409 conflicts
- UnauthorizedException - 401 auth failures
- ForbiddenException - 403 permission failures
- BusinessRuleException - Business logic violations
- ApiException - Custom errors

---

## 🔄 Migration Guide

### Old Code
```csharp
if (result.IsFailure)
{
    return BadRequest(new { error = result.Error!.Code, message = result.Error.Message });
}
```

### New Code
```csharp
if (result.IsFailure)
{
    throw new BusinessRuleException(result.Error!.Code, result.Error.Message);
}
```

### Benefits
- Cleaner code
- Automatic formatting
- Consistent responses
- Less boilerplate

---

## 📋 Error Code Reference

| Code | Meaning | HTTP Status |
|------|---------|-------------|
| EMAIL_REQUIRED | Email is required | 400 |
| VALIDATION_FAILED | Validation errors | 400 |
| RESOURCE_NOT_FOUND | Resource missing | 404 |
| RESOURCE_ALREADY_EXISTS | Duplicate resource | 409 |
| UNAUTHORIZED | Not authenticated | 401 |
| FORBIDDEN | Permission denied | 403 |
| ACCOUNT_LOCKED | Account locked | 403 |
| INVALID_CREDENTIALS | Wrong credentials | 401 |
| INTERNAL_SERVER_ERROR | Server error | 500 |

---

## 📂 File Structure

```
src/Gateway/WebApi/
├── Middleware/
│   ├── GlobalExceptionMiddleware.cs
│   ├── InputValidationMiddleware.cs
│   └── README.md
├── Exceptions/
│   ├── ApiException.cs (contains 7 exception classes)
│   └── CUSTOM_ERRORS_GUIDE.md
├── Helpers/
│   └── ApiResponseHelper.cs
├── Models/
│   └── ResponseModels.cs (ErrorResponse class)
├── Program.cs (middleware registered here)
├── QUICK_REFERENCE.md (this file)
├── CUSTOM_ERRORS_SUMMARY.md
├── CONTROLLER_EXAMPLE.md
└── ERROR_HANDLING_IMPLEMENTATION.md
```

---

## ✅ Checklist

### For API Developers
- [ ] Read Quick Reference (you're here!)
- [ ] Read CUSTOM_ERRORS_SUMMARY.md
- [ ] Pick an example from CONTROLLER_EXAMPLE.md
- [ ] Update your first endpoint
- [ ] Test with different error scenarios
- [ ] Update remaining endpoints

### For Code Reviewers
- [ ] Verify exceptions are thrown from services
- [ ] Check responses use helper methods
- [ ] Ensure no try-catch blocks for formatting
- [ ] Verify error codes are consistent
- [ ] Check error messages are user-friendly

### For Testers
- [ ] Test validation errors (400)
- [ ] Test missing resources (404)
- [ ] Test duplicate resources (409)
- [ ] Test authorization errors (401/403)
- [ ] Test malformed JSON
- [ ] Test empty request bodies

---

## 🧪 Test Commands

### Test Validation Error
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"","password":""}'
```

### Test Malformed JSON
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{invalid}'
```

### Test Empty Body
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d ''
```

---

## 🎯 Next Steps

1. **Start small** - Update one endpoint
2. **Test thoroughly** - Try different error scenarios
3. **Expand gradually** - Update all endpoints
4. **Monitor logs** - Check error handling
5. **Document** - List custom error codes for API consumers

---

## 📞 When to Check What

| I Need To... | Read This |
|--------------|-----------|
| Understand basics | CUSTOM_ERRORS_SUMMARY.md |
| See real examples | CONTROLLER_EXAMPLE.md |
| Deep dive | CUSTOM_ERRORS_GUIDE.md |
| Understand architecture | ERROR_HANDLING_IMPLEMENTATION.md |
| Understand middleware | Middleware/README.md |
| Find a specific method | Helpers/ApiResponseHelper.cs |
| Understand exceptions | Exceptions/ApiException.cs |

---

## 🎉 You're Ready!

Everything is set up. Now:

1. Pick an endpoint to update
2. Choose exception type from examples
3. Throw it instead of building response
4. Test the endpoint
5. Done!

For more help, check the file in the "When to Check What" table above.

---

## 💬 Exception Cheat Sheet

```csharp
// Validation
throw new ValidationException("CODE", "message");

// Not Found
throw new ResourceNotFoundException("Resource", id);

// Duplicate
throw new DuplicateResourceException("Resource", "field", value);

// Unauthorized
throw new UnauthorizedException("message");

// Forbidden
throw new ForbiddenException("message");

// Business Rule
throw new BusinessRuleException("CODE", "message", StatusCodes.Status400BadRequest, details);

// Custom
throw new ApiException("CODE", "message", StatusCodes.Status418ImATeapot);
```

---

Happy Coding! 🚀