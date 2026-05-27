using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace LMS.SharedKernal.AspNetCore;

public static class WebApplicationExtensions
{
    public static WebApplication UseLmsDefaults(this WebApplication app, bool requireTenant = true)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthentication();
        app.UseAuthorization();

        if (requireTenant)
            app.UseMiddleware<TenantMiddleware>();

        app.MapHealthChecks("/health");
        app.MapHealthChecks("/alive");

        return app;
    }

    public static IServiceCollection AddLmsApiDefaults(this IServiceCollection services, string serviceName)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new() { Title = serviceName, Version = "v1" });
        });
        services.AddHealthChecks();
        services.AddTenantContext();
        return services;
    }
}
