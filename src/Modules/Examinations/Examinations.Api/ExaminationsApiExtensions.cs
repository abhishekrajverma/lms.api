using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolErp.Modules.Examinations.Infrastructure;

namespace SchoolErp.Modules.Examinations.Api;

public static class ExaminationsApiExtensions
{
    public static IServiceCollection AddExaminationsApi(this IServiceCollection services, IConfiguration configuration) =>
        services.AddExaminationsModule(configuration);
}
