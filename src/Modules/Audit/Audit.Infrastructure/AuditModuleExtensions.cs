using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolErp.BuildingBlocks.Infrastructure.Persistence;
using SchoolErp.Modules.Audit.Domain.Repositories;
using SchoolErp.Modules.Audit.Persistence;
using SchoolErp.Modules.Audit.Persistence.Repositories;

namespace SchoolErp.Modules.Audit.Infrastructure;

public static class AuditModuleExtensions
{
    public static IServiceCollection AddAuditModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AuditDbContext>(options =>
            options.ConfigureSchoolErpDatabase(configuration, "Audit"));

        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        return services;
    }
}
