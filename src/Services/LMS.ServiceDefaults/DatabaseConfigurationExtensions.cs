using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace LMS.ServiceDefaults;

internal static class DatabaseConfigurationExtensions
{
    public static void ConfigureLmsDatabase<TContext>(
        this DbContextOptionsBuilder options,
        IConfiguration configuration)
        where TContext : DbContext
    {
        if (configuration.GetValue("UseInMemoryDatabase", false))
        {
            options.UseInMemoryDatabase(typeof(TContext).Name + "_Dev");
            return;
        }

        options.UseSqlServer(
            configuration.GetConnectionString("DefaultConnection"),
            sql => sql.MigrationsAssembly(typeof(TContext).Assembly.FullName));
    }
}
