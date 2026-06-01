using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolErp.Modules.Library.Infrastructure;

namespace SchoolErp.Modules.Library.Api;

public static class LibraryApiExtensions
{
    public static IServiceCollection AddLibraryApi(this IServiceCollection services, IConfiguration configuration) =>
        services.AddLibraryModule(configuration);
}
