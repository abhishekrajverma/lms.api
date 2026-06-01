using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SchoolErp.MultiTenancy.Middleware;
using SchoolErp.MultiTenancy.Resolvers;

namespace SchoolErp.MultiTenancy;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSchoolErpMultiTenancy(this IServiceCollection services)
    {
        services.AddSingleton<ITenantStore, InMemoryTenantStore>();
        services.AddSingleton<ITenantResolver, HeaderTenantResolver>();
        services.AddScoped<ITenantContext, TenantContext>();
        return services;
    }

    public static IApplicationBuilder UseSchoolErpMultiTenancy(this IApplicationBuilder app) =>
        app.UseMiddleware<TenantResolutionMiddleware>();
}
