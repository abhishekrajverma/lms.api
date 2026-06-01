using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolErp.BuildingBlocks.Infrastructure.Persistence;
using SchoolErp.Modules.Students.Domain.Repositories;
using SchoolErp.Modules.Students.Persistence;
using SchoolErp.Modules.Students.Persistence.Repositories;

namespace SchoolErp.Modules.Students.Infrastructure;

public static class StudentsModuleExtensions
{
    public static IServiceCollection AddStudentsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<StudentsDbContext>(options =>
            options.ConfigureSchoolErpDatabase(configuration, "Students"));

        services.AddScoped<IStudentRepository, StudentRepository>();
        return services;
    }
}
