using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;
using Gateway.WebApi.Middleware;
using Users.Application.Extensions;
using Users.Infrastructure.Extensions;
using Catalog.Application.Extensions;
using Catalog.Infrastructure.Extensions;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add authentication and authorization
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        var jwtSettings = builder.Configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        var issuer = jwtSettings["Issuer"] ?? "UserService";
        var audience = jwtSettings["Audience"] ?? "UserServiceAPI";

        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(secretKey)),
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization();

// Add application services
builder.Services.AddUsersApplication();
builder.Services.AddCatalogApplicationServices();

// Add infrastructure services
builder.Services.AddUsersInfrastructure(builder.Configuration);
builder.Services.AddCatalogInfrastructure(
    builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string not found"));

// OpenAPI configuration
builder.Services.AddOpenApi();

// Swagger configuration
builder.Services.AddSwaggerGen(options =>
{
    // API Info
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1.0.0",
        Title = "Core Project API",
        Description = "Users Module Management System API with Authentication, Authorization, and Audit Logging",
        TermsOfService = new Uri("https://example.com/terms"),
        Contact = new OpenApiContact
        {
            Name = "API Support",
            Email = "support@example.com",
            Url = new Uri("https://example.com/contact")
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // Security scheme for JWT Bearer
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    // Security requirement
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // XML documentation
    var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

    // Organize by tags
    options.DocInclusionPredicate((name, api) => true);

    // Use full schema names to avoid conflicts
    options.CustomSchemaIds(type => type.FullName);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger(options =>
    {
        options.SerializeAsV2 = false;
    });
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Core Project API v1");
        options.RoutePrefix = string.Empty;
        options.DocumentTitle = "Core Project API Documentation";

        // UI Settings
        options.DefaultModelsExpandDepth(0);
        options.DefaultModelExpandDepth(2);
        options.DocExpansion(DocExpansion.List);
        options.EnableFilter();
        options.ShowExtensions();
        options.EnableValidator();
        options.DisplayOperationId();
    });
}

app.UseHttpsRedirection();

// Add global exception handling middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

// Add input validation middleware
app.UseMiddleware<InputValidationMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
