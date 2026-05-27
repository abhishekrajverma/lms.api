using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace LMS.Identity.API.Infrastructure;

public static class KeycloakAuthenticationExtensions
{
    public static IServiceCollection AddKeycloakAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var authority = configuration["Keycloak:Authority"] ?? "http://localhost:8080/realms/lms";
        var audience = configuration["Keycloak:Audience"] ?? "lms-api";

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = authority;
                options.Audience = audience;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateLifetime = true,
                    NameClaimType = "preferred_username"
                };
            });

        services.AddAuthorization();
        return services;
    }
}
