using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using SchoolErp.BuildingBlocks.Infrastructure.EventBus;

namespace SchoolErp.EventBus.RabbitMq;

public sealed class RabbitPublisher : IRabbitPublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitPublisher> _logger;

    public RabbitPublisher(IConnection connection, ILogger<RabbitPublisher> logger)
    {
        _connection = connection;
        _logger = logger;
        _channel = _connection.CreateModel();
        _channel.ExchangeDeclare(Exchanges.SchoolErp, Exchanges.TypeTopic, durable: true);
    }

    public Task PublishAsync(string routingKey, string payload, CancellationToken cancellationToken = default)
    {
        var body = System.Text.Encoding.UTF8.GetBytes(payload);
        _channel.BasicPublish(Exchanges.SchoolErp, routingKey, body: body);
        _logger.LogInformation("Published {RoutingKey} to {Exchange}", routingKey, Exchanges.SchoolErp);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel.Dispose();
        _connection.Dispose();
    }
}
