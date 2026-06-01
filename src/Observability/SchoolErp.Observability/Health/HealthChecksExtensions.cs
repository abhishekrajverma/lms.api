using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SchoolErp.Observability.Health;

public static class HealthChecksExtensions
{
    public static IServiceCollection AddSchoolErpHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var builder = services.AddHealthChecks();
        var useInMemory = configuration.GetValue<bool>("UseInMemoryDatabase");
        if (!useInMemory)
        {
            var cs = configuration.GetConnectionString("SchoolErp");
            if (!string.IsNullOrWhiteSpace(cs))
                builder.AddSqlServer(cs, name: "sqlserver");
        }

        return services;
    }
}
