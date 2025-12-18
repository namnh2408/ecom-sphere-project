using BuildingBlocks.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace BuildingBlocks.Infrastructure.Shared.Redis;

/// <summary>
/// Redis connection provider interface
/// </summary>
public interface IRedisConnectionProvider
{
    /// <summary>
    /// Gets the Redis connection multiplexer
    /// </summary>
    IConnectionMultiplexer Connection { get; }

    /// <summary>
    /// Gets the database
    /// </summary>
    IDatabase Database { get; }

    /// <summary>
    /// Executes a command on Redis
    /// </summary>
    Task<T> ExecuteAsync<T>(Func<IDatabase, Task<T>> operation);

    /// <summary>
    /// Checks if Redis is connected
    /// </summary>
    bool IsConnected { get; }
}

/// <summary>
/// Extension methods for Redis registration
/// </summary>
public static class RedisExtensions
{
    /// <summary>
    /// Adds Redis services to the dependency injection container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
    {
        var redisOptions = new RedisOptions();
        configuration.GetSection(RedisOptions.SectionName).Bind(redisOptions, x => x.BindNonPublicProperties = true);

        services.AddSingleton(redisOptions);
        
        // Register Redis connection
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var configurationOptions = ConfigurationOptions.Parse(redisOptions.ConnectionString);
            configurationOptions.DefaultDatabase = redisOptions.DefaultDatabase;
            configurationOptions.ConnectTimeout = redisOptions.ConnectTimeout;
            configurationOptions.SyncTimeout = redisOptions.SyncTimeout;
            configurationOptions.AllowAdmin = redisOptions.AllowAdmin;
            configurationOptions.Ssl = redisOptions.Ssl;

            if (!string.IsNullOrEmpty(redisOptions.Password))
            {
                configurationOptions.Password = redisOptions.Password;
            }

            return ConnectionMultiplexer.Connect(configurationOptions);
        });

        // Register Redis connection provider
        services.AddSingleton<IRedisConnectionProvider, RedisConnectionProvider>();

        // Register Redis services
        services.AddSingleton<ICacheService, CacheService>();
        services.AddSingleton<ISessionService, SessionService>();
        services.AddSingleton<IRedisEventBus, RedisEventBus>();
        services.AddSingleton<IRedisMessageQueue, RedisMessageQueue>();

        // Also register as IEventBus for compatibility
        services.AddSingleton<IEventBus>(sp => sp.GetRequiredService<IRedisEventBus>());

        return services;
    }

    /// <summary>
    /// Adds Redis services with custom configuration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configure">The configuration action</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddRedis(this IServiceCollection services, Action<RedisOptions> configure)
    {
        var redisOptions = new RedisOptions();
        configure(redisOptions);

        services.AddSingleton(redisOptions);

        // Register Redis connection
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var configurationOptions = ConfigurationOptions.Parse(redisOptions.ConnectionString);
            configurationOptions.DefaultDatabase = redisOptions.DefaultDatabase;
            configurationOptions.ConnectTimeout = redisOptions.ConnectTimeout;
            configurationOptions.SyncTimeout = redisOptions.SyncTimeout;
            configurationOptions.AllowAdmin = redisOptions.AllowAdmin;
            configurationOptions.Ssl = redisOptions.Ssl;

            if (!string.IsNullOrEmpty(redisOptions.Password))
            {
                configurationOptions.Password = redisOptions.Password;
            }

            return ConnectionMultiplexer.Connect(configurationOptions);
        });

        // Register Redis connection provider
        services.AddSingleton<IRedisConnectionProvider, RedisConnectionProvider>();

        // Register Redis services
        services.AddSingleton<ICacheService, CacheService>();
        services.AddSingleton<ISessionService, SessionService>();
        services.AddSingleton<IRedisEventBus, RedisEventBus>();
        services.AddSingleton<IRedisMessageQueue, RedisMessageQueue>();

        // Also register as IEventBus for compatibility
        services.AddSingleton<IEventBus>(sp => sp.GetRequiredService<IRedisEventBus>());

        return services;
    }
}