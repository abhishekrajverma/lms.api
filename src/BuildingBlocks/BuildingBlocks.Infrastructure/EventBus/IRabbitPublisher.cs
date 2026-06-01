namespace SchoolErp.BuildingBlocks.Infrastructure.EventBus;

public interface IRabbitPublisher
{
    Task PublishAsync(string routingKey, string payload, CancellationToken cancellationToken = default);
}
