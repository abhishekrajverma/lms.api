using Microsoft.Extensions.Logging;

namespace SchoolErp.BuildingBlocks.Infrastructure.EventBus;

public sealed class NoOpRabbitPublisher : IRabbitPublisher
{
    private readonly ILogger<NoOpRabbitPublisher> _logger;

    public NoOpRabbitPublisher(ILogger<NoOpRabbitPublisher> logger) => _logger = logger;

    public Task PublishAsync(string routingKey, string payload, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("NoOp publish {RoutingKey}", routingKey);
        return Task.CompletedTask;
    }
}
