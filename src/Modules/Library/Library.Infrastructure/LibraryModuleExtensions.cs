using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolErp.BuildingBlocks.Infrastructure.Persistence;
using SchoolErp.Modules.Library.Domain.Repositories;
using SchoolErp.Modules.Library.Persistence;
using SchoolErp.Modules.Library.Persistence.Repositories;

namespace SchoolErp.Modules.Library.Infrastructure;

public static class LibraryModuleExtensions
{
    public static IServiceCollection AddLibraryModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<LibraryDbContext>(options =>
            options.ConfigureSchoolErpDatabase(configuration, "Library"));

        services.AddScoped<IBookRepository, BookRepository>();
        return services;
    }
}
