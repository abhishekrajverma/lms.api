using LMS.EventBus.Outbox;
using LMS.Fee.API.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LMS.Fee.API.Infrastructure.Outbox;

public sealed class FeeOutboxProcessor : OutboxProcessorBase<FeeDbContext>
{
    public FeeOutboxProcessor(IServiceScopeFactory scopeFactory, ILogger<FeeOutboxProcessor> logger)
        : base(scopeFactory, logger) { }
}
