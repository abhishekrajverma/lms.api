using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolErp.BuildingBlocks.Infrastructure.Persistence;
using SchoolErp.Modules.Fees.Domain.Repositories;
using SchoolErp.Modules.Fees.Persistence;
using SchoolErp.Modules.Fees.Persistence.Repositories;

namespace SchoolErp.Modules.Fees.Infrastructure;

public static class FeesModuleExtensions
{
    public static IServiceCollection AddFeesModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<FeesDbContext>(options =>
            options.ConfigureSchoolErpDatabase(configuration, "Fees"));

        services.AddScoped<IFeeInvoiceRepository, FeeInvoiceRepository>();
        return services;
    }
}
