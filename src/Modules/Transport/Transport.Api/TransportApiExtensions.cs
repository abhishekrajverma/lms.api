using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolErp.Modules.Transport.Infrastructure;

namespace SchoolErp.Modules.Transport.Api;

public static class TransportApiExtensions
{
    public static IServiceCollection AddTransportApi(this IServiceCollection services, IConfiguration configuration) =>
        services.AddTransportModule(configuration);
}
