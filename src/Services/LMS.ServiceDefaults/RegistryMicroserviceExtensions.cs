using System.Reflection;
using FluentValidation;
using LMS.EventBus;
using LMS.SharedKernal.AspNetCore;
using LMS.SharedKernal.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LMS.ServiceDefaults;

/// <summary>
/// Registration for global registry services (e.g. Tenant API) that are not tenant-scoped.
/// </summary>
public static class RegistryMicroserviceExtensions
{
    public static IServiceCollection AddLmsRegistryMicroservice<TContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName,
        Assembly applicationAssembly,
        IHostEnvironment environment)
        where TContext : DbContext
    {
        services.AddLmsApiDefaults(serviceName);
        services.AddControllers();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));
        services.AddValidatorsFromAssembly(applicationAssembly);

        services.AddDbContext<TContext>(options =>
            options.ConfigureLmsDatabase<TContext>(configuration));

        services.AddScoped<IUnitOfWork>(sp => (IUnitOfWork)sp.GetRequiredService<TContext>());

        if (environment.IsProduction())
            services.AddRabbitMqEventBus(configuration);
        else
            services.AddInMemoryEventBus();

        return services;
    }
}
