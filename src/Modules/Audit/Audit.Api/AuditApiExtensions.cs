using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolErp.Modules.Audit.Infrastructure;

namespace SchoolErp.Modules.Audit.Api;

public static class AuditApiExtensions
{
    public static IServiceCollection AddAuditApi(this IServiceCollection services, IConfiguration configuration) =>
        services.AddAuditModule(configuration);
}
