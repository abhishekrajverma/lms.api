using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolErp.BuildingBlocks.Infrastructure.Persistence;
using SchoolErp.Modules.Examinations.Domain.Repositories;
using SchoolErp.Modules.Examinations.Persistence;
using SchoolErp.Modules.Examinations.Persistence.Repositories;

namespace SchoolErp.Modules.Examinations.Infrastructure;

public static class ExaminationsModuleExtensions
{
    public static IServiceCollection AddExaminationsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ExaminationsDbContext>(options =>
            options.ConfigureSchoolErpDatabase(configuration, "Examinations"));

        services.AddScoped<IExamRepository, ExamRepository>();
        return services;
    }
}
