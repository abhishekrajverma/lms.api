using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolErp.Modules.Employees.Infrastructure;

namespace SchoolErp.Modules.Employees.Api;

public static class EmployeesApiExtensions
{
    public static IServiceCollection AddEmployeesApi(this IServiceCollection services, IConfiguration configuration) =>
        services.AddEmployeesModule(configuration);
}
