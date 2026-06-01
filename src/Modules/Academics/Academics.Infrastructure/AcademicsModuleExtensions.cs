using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolErp.BuildingBlocks.Infrastructure.Persistence;
using SchoolErp.Modules.Academics.Domain.Repositories;
using SchoolErp.Modules.Academics.Persistence;
using SchoolErp.Modules.Academics.Persistence.Repositories;

namespace SchoolErp.Modules.Academics.Infrastructure;

public static class AcademicsModuleExtensions
{
    public static IServiceCollection AddAcademicsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AcademicsDbContext>(options =>
            options.ConfigureSchoolErpDatabase(configuration, "Academics"));

        services.AddScoped<IAcademicYearRepository, AcademicYearRepository>();
        return services;
    }
}
