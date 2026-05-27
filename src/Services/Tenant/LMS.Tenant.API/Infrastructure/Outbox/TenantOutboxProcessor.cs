using LMS.EventBus.Outbox;
using LMS.Tenant.API.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LMS.Tenant.API.Infrastructure.Outbox;

public sealed class TenantOutboxProcessor : OutboxProcessorBase<TenantDbContext>
{
    public TenantOutboxProcessor(IServiceScopeFactory scopeFactory, ILogger<TenantOutboxProcessor> logger)
        : base(scopeFactory, logger)
    {
    }
}
