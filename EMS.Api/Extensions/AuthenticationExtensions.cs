namespace EMS.Api.Extensions;

using EMS.Application.Interfaces.Services;
using EMS.Application.Services;
using EMS.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.IdentityModel.Tokens;
using System.Text;

/// <summary>
/// Authentication and authorization extensions for dependency injection
/// Registers JWT authentication, services, and configuration
/// </summary>
public static class AuthenticationExtensions
{
    /// <summary>
    /// Add authentication and authorization services
    /// </summary>
    public static IServiceCollection AddAuthenticationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // JWT Settings
        var jwtSettings = new JwtSettings();
        configuration.GetSection("JwtSettings").Bind(jwtSettings);
        services.AddSingleton(jwtSettings);

        // Security services
        //services.AddScoped<IPasswordHasher, PasswordHasher>();
        //services.AddScoped<IJwtTokenProvider, JwtTokenProvider>();
        //services.AddScoped<ITokenBlacklistService, TokenBlacklistService>();
        //services.AddScoped<ILoginAttemptTracker, LoginAttemptTracker>();

        // Authentication services
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IAuthorizationService, AuthorizationService>();

        // JWT Authentication
        AddJwtAuthentication(services, jwtSettings);

        return services;
    }

    /// <summary>
    /// Configure JWT authentication
    /// </summary>
    private static void AddJwtAuthentication(IServiceCollection services, JwtSettings jwtSettings)
    {
        var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

        //services.AddAuthentication(options =>
        //{
        //    //options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        //    //options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        //})
        //.AddJwtBearer(options =>
        //{
        //    options.TokenValidationParameters = new TokenValidationParameters
        //    {
        //        ValidateIssuerSigningKey = true,
        //        IssuerSigningKey = new SymmetricSecurityKey(key),
        //        ValidateIssuer = true,
        //        ValidIssuer = jwtSettings.Issuer,
        //        ValidateAudience = true,
        //        ValidAudience = jwtSettings.Audience,
        //        ValidateLifetime = true,
        //        ClockSkew = TimeSpan.Zero, // Don't add extra time for token expiration
        //        NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
        //    };

        //    // Custom validation events
        //    options.Events = new JwtBearerEvents
        //    {
        //        OnAuthenticationFailed = context =>
        //        {
        //            // Log authentication failures
        //            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
        //            logger.LogWarning("Authentication failed: {Exception}", context.Exception.Message);
        //            return Task.CompletedTask;
        //        },
        //        OnTokenValidated = context =>
        //        {
        //            // Token is valid - can add additional processing here
        //            return Task.CompletedTask;
        //        },
        //        OnChallenge = context =>
        //        {
        //            // Handle challenge (e.g., missing or invalid token)
        //            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
        //            logger.LogWarning("JWT Challenge: {Error}", context.ErrorDescription);
        //            return Task.CompletedTask;
        //        }
        //    };
        //});

        // Authorization policies
        //services.AddAuthorization(options =>
        //{
        //    options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder(
        //        JwtBearerDefaults.AuthenticationScheme)
        //        .RequireAuthenticatedUser()
        //        .Build();

        //    // Policy: Student only
        //    options.AddPolicy("StudentOnly", policy =>
        //        policy.RequireRole("Student"));

        //    // Policy: Faculty and Admin
        //    options.AddPolicy("FacultyOrAdmin", policy =>
        //        policy.RequireRole("Faculty", "Admin"));

        //    // Policy: Faculty only
        //    options.AddPolicy("FacultyOnly", policy =>
        //        policy.RequireRole("Faculty"));

        //    // Policy: Admin only
        //    options.AddPolicy("AdminOnly", policy =>
        //        policy.RequireRole("Admin"));

        //    // Policy: Department Head
        //    options.AddPolicy("DepartmentHeadOrAdmin", policy =>
        //        policy.RequireRole("DepartmentHead", "Admin"));
        //});
    }
}
