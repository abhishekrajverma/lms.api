using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolErp.BuildingBlocks.Infrastructure.Persistence;
using SchoolErp.Modules.Employees.Domain.Repositories;
using SchoolErp.Modules.Employees.Persistence;
using SchoolErp.Modules.Employees.Persistence.Repositories;

namespace SchoolErp.Modules.Employees.Infrastructure;

public static class EmployeesModuleExtensions
{
    public static IServiceCollection AddEmployeesModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<EmployeesDbContext>(options =>
            options.ConfigureSchoolErpDatabase(configuration, "Employees"));

        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        return services;
    }
}
