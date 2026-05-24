namespace EMS.Infrastructure.Extensions;

using EMS.Domain.Entities;
using EMS.Infrastructure.Caching;
using EMS.Infrastructure.Repositories;
using EMS.Infrastructure.Repositories.Organisation;
using EMS.Infrastructure.Security;
using EMS.Shared.Interfaces.Caching;
using EMS.Shared.Interfaces.Repositories;
using EMS.Shared.Interfaces.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using EMS.Application.Interfaces.Repositories.Organisation;

/// <summary>
/// Infrastructure layer service registration
/// Registers all repositories, caching, security, and logging services
/// </summary>
public static class ServiceExtensions
{
    /// <summary>
    /// Add infrastructure services to DI container
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Connection factory
        services.AddScoped<EMS.Shared.Interfaces.Repositories.IDbConnectionFactory, DbConnectionFactory>();

        // Unit of Work
        services.AddScoped<EMS.Shared.Interfaces.Repositories.IUnitOfWork, UnitOfWork>();

        // Generic repositories
        services.AddScoped(typeof(EMS.Shared.Interfaces.Repositories.IGenericRepository<>), typeof(GenericRepository<>));

        // Specific repositories
        
        services.AddScoped<EMS.Shared.Interfaces.Repositories.IGenericRepository<User>, GenericRepository<User>>();
      

        // Stored procedure repository
        services.AddScoped<EMS.Shared.Interfaces.Repositories.IStoredProcedureRepository, StoredProcedureRepository>();
        services.AddScoped<IOrganisationRepository, OrganisationRepository>();

        // Redis cache
        services.AddStackExchangeRedisCache(options =>
        {
            var redisConnection = configuration.GetConnectionString("Redis");
            options.Configuration = redisConnection;
        });

        services.AddScoped<EMS.Shared.Interfaces.Caching.ICacheService, RedisCacheService>();

        // Security services
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenProvider, JwtTokenProvider>();
        services.AddScoped<ITokenBlacklistService, TokenBlacklistService>();
        services.AddScoped<ILoginAttemptTracker, LoginAttemptTracker>();

        // Configure JWT settings
        var jwtSettings = new JwtSettings();
        var jwtSection = configuration.GetSection("JwtSettings");
        if (jwtSection.Exists())
        {
            jwtSettings.SecretKey = jwtSection["SecretKey"] ?? "your-secret-key-change-in-production";
            jwtSettings.ExpirationMinutes = int.TryParse(jwtSection["ExpirationMinutes"], out var exp) ? exp : 60;
            jwtSettings.RefreshTokenExpirationDays = int.TryParse(jwtSection["RefreshTokenExpirationDays"], out var refTokExp) ? refTokExp : 7;
            jwtSettings.Issuer = jwtSection["Issuer"] ?? "ems-api";
            jwtSettings.Audience = jwtSection["Audience"] ?? "ems-clients";
        }
        services.AddSingleton(jwtSettings);

        return services;
    }
}
