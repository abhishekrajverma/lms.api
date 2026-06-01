using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolErp.BuildingBlocks.Infrastructure.Persistence;
using SchoolErp.Modules.Identity.Domain.Repositories;
using SchoolErp.Modules.Identity.Persistence;
using SchoolErp.Modules.Identity.Persistence.Repositories;

namespace SchoolErp.Modules.Identity.Infrastructure;

public static class IdentityModuleExtensions
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<IdentityDbContext>(options =>
            options.ConfigureSchoolErpDatabase(configuration, "Identity"));

        services.AddScoped<IUserRepository, UserRepository>();
        return services;
    }
}
