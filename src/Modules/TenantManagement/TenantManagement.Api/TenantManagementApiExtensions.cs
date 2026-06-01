using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolErp.Modules.TenantManagement.Infrastructure;

namespace SchoolErp.Modules.TenantManagement.Api;

public static class TenantManagementApiExtensions
{
    public static IServiceCollection AddTenantManagementApi(this IServiceCollection services, IConfiguration configuration) =>
        services.AddTenantManagementModule(configuration);
}
