using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolErp.Modules.Fees.Infrastructure;

namespace SchoolErp.Modules.Fees.Api;

public static class FeesApiExtensions
{
    public static IServiceCollection AddFeesApi(this IServiceCollection services, IConfiguration configuration) =>
        services.AddFeesModule(configuration);
}
