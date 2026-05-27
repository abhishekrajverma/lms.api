using LMS.EventBus.Abstractions;
using LMS.EventBus.InMemory;
using LMS.EventBus.RabbitMQ;
using LMS.EventBus.Subscriptions;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LMS.EventBus;

public static class EventBusServiceExtensions
{
    /// <summary>
    /// Registers the in-memory EventBus — use in Development and Tests.
    /// </summary>
    public static IServiceCollection AddInMemoryEventBus(this IServiceCollection services)
    {
        services.AddSingleton<EventBusSubscriptionsManager>();
        services.AddSingleton<IEventBus, EventBusInMemory>();
        return services;
    }

    /// <summary>
    /// Registers the RabbitMQ EventBus via MassTransit — use in Production.
    ///
    /// Requires appsettings:
    /// "RabbitMQ": {
    ///   "Host": "rabbitmq://localhost",
    ///   "Username": "guest",
    ///   "Password": "guest"
    /// }
    /// </summary>
    public static IServiceCollection AddRabbitMqEventBus(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IBusRegistrationConfigurator>? configure = null)
    {
        services.AddSingleton<EventBusSubscriptionsManager>();

        services.AddMassTransit(x =>
        {
            configure?.Invoke(x);

            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(
                    configuration["RabbitMQ:Host"] ?? "rabbitmq://localhost",
                    h =>
                    {
                        h.Username(configuration["RabbitMQ:Username"] ?? "guest");
                        h.Password(configuration["RabbitMQ:Password"] ?? "guest");
                    });

                cfg.ConfigureEndpoints(ctx);
            });
        });

        services.AddSingleton<IEventBus, EventBusRabbitMQ>();
        return services;
    }
}
