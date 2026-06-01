using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolErp.BuildingBlocks.Infrastructure.Persistence;
using SchoolErp.Modules.Inventory.Domain.Repositories;
using SchoolErp.Modules.Inventory.Persistence;
using SchoolErp.Modules.Inventory.Persistence.Repositories;

namespace SchoolErp.Modules.Inventory.Infrastructure;

public static class InventoryModuleExtensions
{
    public static IServiceCollection AddInventoryModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<InventoryDbContext>(options =>
            options.ConfigureSchoolErpDatabase(configuration, "Inventory"));

        services.AddScoped<IInventoryItemRepository, InventoryItemRepository>();
        return services;
    }
}
