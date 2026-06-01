using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolErp.Modules.Inventory.Infrastructure;

namespace SchoolErp.Modules.Inventory.Api;

public static class InventoryApiExtensions
{
    public static IServiceCollection AddInventoryApi(this IServiceCollection services, IConfiguration configuration) =>
        services.AddInventoryModule(configuration);
}
