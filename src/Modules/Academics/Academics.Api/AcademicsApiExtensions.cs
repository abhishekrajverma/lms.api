using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolErp.Modules.Academics.Infrastructure;

namespace SchoolErp.Modules.Academics.Api;

public static class AcademicsApiExtensions
{
    public static IServiceCollection AddAcademicsApi(this IServiceCollection services, IConfiguration configuration) =>
        services.AddAcademicsModule(configuration);
}
