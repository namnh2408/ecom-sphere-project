# Users Module - Quick Start Guide

## 🚀 Getting Started

### Step 1: Verify Projects Build

```powershell
cd d:\projects\core-project

# Build all layers
dotnet build "src/Modules/Users/Users.Domain/Users.Domain.csproj"
dotnet build "src/Modules/Users/Users.Application/Users.Application.csproj"
dotnet build "src/Modules/Users/Users.Infrastructure/Users.Infrastructure.csproj"
```

### Step 2: Configure appsettings.json

Add to `src/Gateway/WebApi/appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "JwtSettings": {
    "SecretKey": "your-super-secret-key-that-is-at-least-32-characters-long-make-it-random",
    "Issuer": "UserService",
    "Audience": "UserServiceAPI",
    "ExpiresInMinutes": 60
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=UsersDb;Trusted_Connection=true;Encrypt=false;"
  },
  "AllowedHosts": "*"
}
```

### Step 3: Update Program.cs

Add to `src/Gateway/WebApi/Program.cs`:

```csharp
using Users.Application.Extensions;
using Users.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplicationBuilder.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

// Add Users Module services
builder.Services.AddUsersApplication();
builder.Services.AddUsersInfrastructure(builder.Configuration);

// Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(secretKey),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

### Step 4: Create Database Migrations

```powershell
# Install EF Tools if not already installed
dotnet tool install --global dotnet-ef

# Create initial migration
dotnet ef migrations add InitialUsersMigration `
  -p src/Modules/Users/Users.Infrastructure `
  -s src/Gateway/WebApi

# Apply migration
dotnet ef database update -s src/Gateway/WebApi
```

### Step 5: Run the Application

```powershell
cd src/Gateway/WebApi
dotnet run
```

The API will be available at `https://localhost:5001` with Swagger at `/swagger`.

---

## 📝 API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login user
- `POST /api/auth/change-password` - Change password

### Users Management
- `GET /api/users/{userId}` - Get user by ID
- `GET /api/users/email/{email}` - Get user by email
- `PUT /api/users/{userId}/profile` - Update user profile
- `POST /api/users/{userId}/roles/{roleId}` - Assign role to user
- `DELETE /api/users/{userId}/roles/{roleId}` - Remove role from user

### Roles Management
- `GET /api/roles` - Get all roles
- `GET /api/roles/{roleId}` - Get role by ID
- `POST /api/roles` - Create role
- `PUT /api/roles/{roleId}` - Update role
- `POST /api/roles/{roleId}/permissions/{permissionId}` - Assign permission to role

---

## 🧪 Testing with Curl

### Register
```bash
curl -X POST "https://localhost:5001/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "Password123!",
    "firstName": "John",
    "lastName": "Doe"
  }'
```

### Login
```bash
curl -X POST "https://localhost:5001/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "Password123!"
  }'
```

### Get User (using JWT token)
```bash
curl -X GET "https://localhost:5001/api/users/user-id-here" \
  -H "Authorization: Bearer <your-jwt-token>"
```

---

## ✅ Completed Handlers

- ✅ `RegisterUserCommandHandler`
- ✅ `LoginUserCommandHandler`
- ✅ `ChangePasswordCommandHandler`
- ✅ `ResetPasswordCommandHandler`
- ✅ `CreateRoleCommandHandler`
- ✅ `UpdateRoleCommandHandler`
- ✅ `DeleteRoleCommandHandler`
- ✅ `AssignRoleToUserCommandHandler`
- ✅ `RemoveRoleFromUserCommandHandler`
- ✅ `AssignPermissionToRoleCommandHandler`
- ✅ `RemovePermissionFromRoleCommandHandler`
- ✅ `CreatePermissionCommandHandler`
- ✅ `DeletePermissionCommandHandler`
- ✅ `UpdateUserProfileCommandHandler`
- ✅ `GetUserByIdQueryHandler`
- ✅ `GetUserByEmailQueryHandler`
- ✅ `GetAllRolesQueryHandler`
- ✅ `GetAllPermissionsQueryHandler`
- ✅ `GetPermissionsByRoleIdQueryHandler`

---

## 📚 Key Features Implemented

✅ **Authentication**
- User registration with password hashing (BCrypt)
- Login with JWT token generation
- Password change functionality
- Secure password storage

✅ **Authorization**
- Role-based access control (RBAC)
- Permission management
- Role-permission assignment
- User-role assignment

✅ **Data Validation**
- Email format validation
- Password strength requirements (min 8 chars)
- First/Last name validation
- Role/Permission name validation

✅ **Database**
- SQL Server integration
- EF Core with proper entity configurations
- Value object support (Email, Password)
- Owned entities

✅ **API**
- RESTful endpoints
- Swagger documentation
- JWT authentication
- Proper HTTP status codes and error responses

---

## 🔧 Troubleshooting

### JWT Not Working
- Check SecretKey length (min 32 characters)
- Verify issuer and audience match in configuration
- Check token expiration

### Database Connection Issues
- Verify connection string in appsettings.json
- Ensure SQL Server is running
- Check database name and permissions

### Password Hashing Issues
- Make sure BCrypt.Net-Next package is installed
- Verify password requirements (min 8 chars)

---

## 📖 Architecture Notes

The Users module follows clean architecture principles:

1. **Domain Layer** - Pure business logic, no external dependencies
2. **Application Layer** - Use cases and orchestration using MediatR
3. **Infrastructure Layer** - Database, external services, implementations
4. **API Layer** - HTTP endpoints and controllers

Each layer has clear dependencies:
- API → Application → Domain
- Infrastructure → Domain
- All layers use dependency injection

---

## 🎯 Next Steps

1. ✅ Verify all projects build successfully
2. ✅ Update appsettings.json with JWT and database configuration
3. ✅ Update Program.cs with dependency injection and authentication
4. ✅ Run database migrations
5. ✅ Start the application
6. ✅ Test endpoints with Swagger or Postman
7. Optional: Add email verification flow
8. Optional: Implement password reset via email
9. Optional: Add two-factor authentication

---

## 📞 Support

For issues or questions about the implementation, refer to:
- Domain models in `Users.Domain`
- Command/Query definitions in `Users.Application`
- DbContext in `Users.Infrastructure/Persistence`
- Controllers in `Gateway.WebApi/Controllers`

Good luck! 🚀