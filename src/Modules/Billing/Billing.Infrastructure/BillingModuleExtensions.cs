using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolErp.BuildingBlocks.Infrastructure.Persistence;
using SchoolErp.Modules.Billing.Domain.Repositories;
using SchoolErp.Modules.Billing.Persistence;
using SchoolErp.Modules.Billing.Persistence.Repositories;

namespace SchoolErp.Modules.Billing.Infrastructure;

public static class BillingModuleExtensions
{
    public static IServiceCollection AddBillingModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<BillingDbContext>(options =>
            options.ConfigureSchoolErpDatabase(configuration, "Billing"));

        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        return services;
    }
}
