using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolErp.BuildingBlocks.Infrastructure.Persistence;
using SchoolErp.Modules.TenantManagement.Domain.Repositories;
using SchoolErp.Modules.TenantManagement.Persistence;
using SchoolErp.Modules.TenantManagement.Persistence.Repositories;

namespace SchoolErp.Modules.TenantManagement.Infrastructure;

public static class TenantManagementModuleExtensions
{
    public static IServiceCollection AddTenantManagementModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TenantManagementDbContext>(options =>
            options.ConfigureSchoolErpDatabase(configuration, "TenantManagement"));

        services.AddScoped<ITenantRepository, TenantRepository>();
        return services;
    }
}
