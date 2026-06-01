using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using SchoolErp.BuildingBlocks.Infrastructure.EventBus;
using SchoolErp.EventBus.RabbitMq;

namespace SchoolErp.EventBus;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSchoolErpEventBus(this IServiceCollection services, IConfiguration configuration)
    {
        var host = configuration["RabbitMQ:Host"] ?? "localhost";
        var factory = new ConnectionFactory { HostName = host, DispatchConsumersAsync = true };
        services.AddSingleton<IConnection>(_ => factory.CreateConnection());
        services.AddSingleton<IRabbitPublisher, RabbitPublisher>();
        return services;
    }
}
