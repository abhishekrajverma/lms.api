using LMS.SharedKernal.Primitives;
using Microsoft.Extensions.DependencyInjection;

namespace LMS.SharedKernal.AspNetCore;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTenantContext(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ITenantContext, HttpTenantContext>();
        return services;
    }
}
