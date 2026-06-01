using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolErp.BuildingBlocks.Infrastructure.Persistence;
using SchoolErp.Modules.Transport.Domain.Repositories;
using SchoolErp.Modules.Transport.Persistence;
using SchoolErp.Modules.Transport.Persistence.Repositories;

namespace SchoolErp.Modules.Transport.Infrastructure;

public static class TransportModuleExtensions
{
    public static IServiceCollection AddTransportModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TransportDbContext>(options =>
            options.ConfigureSchoolErpDatabase(configuration, "Transport"));

        services.AddScoped<IRouteRepository, RouteRepository>();
        return services;
    }
}
