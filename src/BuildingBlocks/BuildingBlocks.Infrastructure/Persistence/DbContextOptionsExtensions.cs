using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace SchoolErp.BuildingBlocks.Infrastructure.Persistence;

public static class DbContextOptionsExtensions
{
    public static DbContextOptionsBuilder ConfigureSchoolErpDatabase(
        this DbContextOptionsBuilder options,
        IConfiguration configuration,
        string databaseName)
    {
        var useInMemory = configuration.GetValue<bool>("UseInMemoryDatabase");
        if (useInMemory)
            return options.UseInMemoryDatabase($"SchoolErp_{databaseName}");

        var connectionString = configuration.GetConnectionString("SchoolErp")
            ?? "Server=(localdb)\\mssqllocaldb;Database=SchoolErp;Trusted_Connection=True;TrustServerCertificate=True";
        return options.UseSqlServer(connectionString, sql => sql.MigrationsHistoryTable($"__{databaseName}Migrations", databaseName));
    }
}
