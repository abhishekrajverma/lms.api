using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolErp.Modules.Identity.Infrastructure;

namespace SchoolErp.Modules.Identity.Api;

public static class IdentityApiExtensions
{
    public static IServiceCollection AddIdentityApi(this IServiceCollection services, IConfiguration configuration) =>
        services.AddIdentityModule(configuration);
}
