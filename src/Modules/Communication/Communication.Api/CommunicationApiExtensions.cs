using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolErp.Modules.Communication.Infrastructure;

namespace SchoolErp.Modules.Communication.Api;

public static class CommunicationApiExtensions
{
    public static IServiceCollection AddCommunicationApi(this IServiceCollection services, IConfiguration configuration) =>
        services.AddCommunicationModule(configuration);
}
