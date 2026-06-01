using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolErp.Modules.Billing.Infrastructure;

namespace SchoolErp.Modules.Billing.Api;

public static class BillingApiExtensions
{
    public static IServiceCollection AddBillingApi(this IServiceCollection services, IConfiguration configuration) =>
        services.AddBillingModule(configuration);
}
