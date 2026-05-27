using LMS.Academic.API.Infrastructure.Persistence;
using LMS.EventBus.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LMS.Academic.API.Infrastructure.Outbox;

public sealed class AcademicOutboxProcessor : OutboxProcessorBase<AcademicDbContext>
{
    public AcademicOutboxProcessor(IServiceScopeFactory scopeFactory, ILogger<AcademicOutboxProcessor> logger)
        : base(scopeFactory, logger) { }
}
