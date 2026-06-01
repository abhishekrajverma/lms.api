using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SchoolErp.BuildingBlocks.Application.Behaviors;

namespace SchoolErp.BuildingBlocks.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSchoolErpMediatR(
        this IServiceCollection services,
        params Assembly[] moduleAssemblies)
    {
        var assemblies = new List<Assembly> { typeof(ServiceCollectionExtensions).Assembly };
        assemblies.AddRange(moduleAssemblies);

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(assemblies.ToArray());
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(TransactionBehavior<,>));
        });

        services.AddValidatorsFromAssemblies(assemblies.ToArray());
        return services;
    }
}
