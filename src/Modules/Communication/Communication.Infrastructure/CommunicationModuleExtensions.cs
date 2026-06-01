using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolErp.BuildingBlocks.Infrastructure.Persistence;
using SchoolErp.Modules.Communication.Domain.Repositories;
using SchoolErp.Modules.Communication.Persistence;
using SchoolErp.Modules.Communication.Persistence.Repositories;

namespace SchoolErp.Modules.Communication.Infrastructure;

public static class CommunicationModuleExtensions
{
    public static IServiceCollection AddCommunicationModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CommunicationDbContext>(options =>
            options.ConfigureSchoolErpDatabase(configuration, "Communication"));

        services.AddScoped<INotificationRepository, NotificationRepository>();
        return services;
    }
}
