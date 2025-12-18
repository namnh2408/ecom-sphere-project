using BuildingBlocks.Infrastructure.Shared.Dapper;
using BuildingBlocks.Infrastructure.Shared.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Users.Domain.Repositories;
using Users.Domain.Services;
using Users.Infrastructure.Persistence;
using Users.Infrastructure.Repositories.Commands;
using Users.Infrastructure.Repositories.Queries;
using Users.Infrastructure.Services;

namespace Users.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUsersInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        // Add DbContext
        services.AddDbContext<UsersDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });

        // Add Dapper Connection Provider (separate from EF Core)
        services.AddScoped<IDapperConnectionProvider>(sp =>
            new DapperConnectionProvider(connectionString, sp.GetRequiredService<ILogger<DapperConnectionProvider>>()));

        // Add Write Repositories (EF Core)
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IEmailVerificationTokenRepository, EmailVerificationTokenRepository>();
        services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
        services.AddScoped<ILoginAttemptRepository, LoginAttemptRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IUserActivityHistoryRepository, UserActivityHistoryRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IChangeHistoryRepository, ChangeHistoryRepository>();
        services.AddScoped<IAccessLogRepository, AccessLogRepository>();

        // Add Read Repositories (Dapper - Query Optimization)
        services.AddScoped<UserQueryRepository>();
        services.AddScoped<RoleQueryRepository>();
        services.AddScoped<PermissionQueryRepository>();
        services.AddScoped<ActivityHistoryQueryRepository>();
        services.AddScoped<AuditLogQueryRepository>();

        // Add generic Unit of Work from BuildingBlocks
        services.AddUnitOfWork<UsersDbContext>();

        // Add Services
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        var issuer = jwtSettings["Issuer"] ?? "UserService";
        var audience = jwtSettings["Audience"] ?? "UserServiceAPI";

        services.AddScoped<ITokenGenerationService>(sp =>
            new JwtTokenGenerationService(secretKey, issuer, audience));

        services.AddScoped<IPasswordHashingService, BcryptPasswordHashingService>();

        return services;
    }
}