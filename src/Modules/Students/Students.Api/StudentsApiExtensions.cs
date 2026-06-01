using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolErp.Modules.Students.Infrastructure;

namespace SchoolErp.Modules.Students.Api;

public static class StudentsApiExtensions
{
    public static IServiceCollection AddStudentsApi(this IServiceCollection services, IConfiguration configuration) =>
        services.AddStudentsModule(configuration);
}
