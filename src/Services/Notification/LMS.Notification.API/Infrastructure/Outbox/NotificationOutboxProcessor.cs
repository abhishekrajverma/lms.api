using LMS.EventBus.Outbox;
using LMS.Notification.API.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LMS.Notification.API.Infrastructure.Outbox;

public sealed class NotificationOutboxProcessor : OutboxProcessorBase<NotificationDbContext>
{
    public NotificationOutboxProcessor(IServiceScopeFactory scopeFactory, ILogger<NotificationOutboxProcessor> logger)
        : base(scopeFactory, logger) { }
}
